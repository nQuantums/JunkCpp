#include <string>
#include <iostream>
#include <sstream>
#include <strstream>
#include <iomanip>
#include <memory>
#include <string.h>
#include "GlobalSocketLogger.h"
#include "Encoding.h"
#include "FilePath.h"
#include "Directory.h"
#include "DateTime.h"
#include "Str.h"
#include "Clock.h"
#include "ThreadLocalStorage.h"

#if defined _MSC_VER
#include <Windows.h>
#include <stdlib.h>
#else
#error gcc version is not implemented.
#endif

_JUNK_BEGIN

static Encoding g_Enc = Encoding::ASCII();


//! 指定サイズまできっちり送信する、指定サイズに満たないで終了するならエラーとなる
static intptr_t SendToBytes(SocketRef sock, const void* pBuf, intptr_t size) {
	intptr_t len = 0;
	while (size) {
		intptr_t n = sock.Send((char*)pBuf + len, size);
		if (n <= 0) {
			if (sock.TimedOutSendError())
				return len;
			else
				return -1;
		}
		size -= n;
		len += n;
	}
	return len;
}

//! 指定サイズまできっちり受信する、指定サイズに満たないで終了するならエラーとなる
static intptr_t RecvToBytes(SocketRef sock, void* pBuf, intptr_t size) {
	intptr_t len = 0;
	while(size) {
		intptr_t n = sock.Recv((char*)pBuf + len, size);
		if(n <= 0) {
			if(sock.TimedOutRecvError())
				return len;
			else
				return -1;
		}
		size -= n;
		len += n;
	}
	return len;
}


//==============================================================================
//		GlobalSocketLogger クラス

struct GlobalSocketLogger::Instance {
	CriticalSection CS;
	Socket Sock;
	std::wstring Host;
	int Port;
	std::wstring LocalIpAddress;
	bool BinaryLog;

	virtual intptr_t& Depth() {
		return s_Depth.Get();
	}

private:
	static JUNK_TLS(intptr_t) s_Depth;
};

JUNK_TLS(intptr_t) GlobalSocketLogger::Instance::s_Depth;

static GlobalSocketLogger::Instance g_Instance;
static GlobalSocketLogger::Instance* g_pInstance = &g_Instance;


//! まだ接続していなかったら接続してソケットを返す
static SocketRef GetSocket() {
	CriticalSectionLock lock(&g_pInstance->CS);
	if(g_pInstance->Sock.IsInvalidHandle()) {
		g_pInstance->LocalIpAddress = Encoding::ASCII().GetString(Socket::GetLocalIPAddress(Socket::Af::IPv4));

		std::wstringstream ss;
		ss << g_pInstance->Port;

		Socket::Endpoint ep;
		ep.Create(g_pInstance->Host.c_str(), ss.str().c_str(), Socket::St::Stream, Socket::Af::IPv4, false);
		std::vector<std::string> hosts, services;
		ep.GetNames(hosts, services);

		g_pInstance->Sock.Create(ep);
		if(!g_pInstance->Sock.Connect(ep)) {
			std::cerr << "Failed to connect to server." << std::endl;
			return SocketRef();
		}

		g_pInstance->Sock.SetNoDelay(1);
	}
	return g_pInstance->Sock;
}


//! 通信用ソケットなどの情報を保持するインスタンスを取得する
GlobalSocketLogger::Instance* GlobalSocketLogger::GetInstance() {
	return g_pInstance;
}

//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
void GlobalSocketLogger::Startup(const wchar_t* pszHost, int port) {
	CriticalSectionLock lock(&g_pInstance->CS);
	g_pInstance->Host = pszHost;
	g_pInstance->Port = port;
	Socket::Startup();
}

//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
void GlobalSocketLogger::Startup(const char* pszHost, int port) {
	CriticalSectionLock lock(&g_pInstance->CS);
	g_pInstance->Host = Encoding::ASCII().GetString(pszHost);
	g_pInstance->Port = port;
	Socket::Startup();
}

//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
void GlobalSocketLogger::Startup(wchar_t* pszIniFile) {
#if defined _MSC_VER
	std::wstring iniFilePath = FilePath::Combine(Directory::GetExeDirectoryW(), pszIniFile);
	std::wstring address;
	std::wstring port;

	wchar_t buf[MAX_PATH];
	DWORD n = ::GetPrivateProfileStringW(L"GlobalSocketLogger", L"ServerAddress", L"127.0.0.1", buf, _countof(buf) - 1, iniFilePath.c_str());
	buf[n] = L'\0';
	address = buf;
	n = ::GetPrivateProfileStringW(L"GlobalSocketLogger", L"ServerPort", L"33777", buf, _countof(buf) - 1, iniFilePath.c_str());
	buf[n] = L'\0';
	port = buf;


    std::wistringstream iss(port);
    int portNumber = 0;
    iss >> portNumber;

	Startup(address.c_str(), portNumber);
#else
#error gcc version is not implemented.
#endif
}

//! 他DLLのインスタンスを指定して初期化する
void GlobalSocketLogger::Startup(Instance* pInstance) {
	g_pInstance = pInstance;
}

//! 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
void GlobalSocketLogger::Cleanup() {
	Socket::Cleanup();
}

//! サーバーへコマンドパケットを送り応答を取得する
LogServer::Pkt* GlobalSocketLogger::Command(LogServer::Pkt* pCmd) {
	// 送って受け取るまでを排他処理とする
	CriticalSectionLock lock(&g_pInstance->CS);
	SocketRef sock = GetSocket();

	// とりあえず送る
	size_t size = pCmd->PacketSize();
	if (SendToBytes(sock, pCmd, size) < (intptr_t)size)
		return NULL;

	// 応答パケットサイズ受信
	LogServer::Pkt result;
	if (RecvToBytes(sock, &result.Size, sizeof(result.Size)) < sizeof(result.Size))
		return NULL;

	// 残りのパケット全体を受信
	LogServer::Pkt* pResult = LogServer::Pkt::Allocate(result.Size);
	if(RecvToBytes(sock, &pResult->Command, result.Size) < result.Size) {
		LogServer::Pkt::Deallocate(pResult);
		return NULL;
	}

	return pResult;
}

//! サーバーのログ出力形式をバイナリかどうか設定する、スレッドセーフ
void GlobalSocketLogger::BinaryLog(bool binary) {
	std::auto_ptr<LogServer::PktCommandBinaryLog> pCmd(LogServer::PktCommandBinaryLog::Allocate(binary));
	std::auto_ptr<LogServer::Pkt> pResult(Command(pCmd.get()));
}

//! サーバーへログを送る、スレッドセーフ
void GlobalSocketLogger::WriteLog(const wchar_t* pszText) {
	size_t textSize = ::wcslen(pszText) * sizeof(wchar_t);
	if(textSize == 0)
		return;

	std::string bytes;
	g_Enc.GetBytes(pszText, textSize, bytes);

	std::auto_ptr<LogServer::PktCommandLogWrite> pCmd(
		LogServer::PktCommandLogWrite::Allocate(
			::GetCurrentProcessId(),
			::GetCurrentThreadId(),
			&bytes[0],
			bytes.size()));
	std::auto_ptr<LogServer::Pkt> pResult(Command(pCmd.get()));
}

//! サーバーへログをファイルへフラッシュ要求
void GlobalSocketLogger::Flush() {
	LogServer::Pkt cmd;
	cmd.Size = sizeof(cmd.Command);
	cmd.Command = LogServer::CommandEnum::Flush;
	std::auto_ptr<LogServer::Pkt> pResult(Command(&cmd));
}

//! サーバーへ現在のログファイルを閉じる要求
void GlobalSocketLogger::FileClose() {
	LogServer::Pkt cmd;
	cmd.Size = sizeof(cmd.Command);
	cmd.Command = LogServer::CommandEnum::FileClose;
	std::auto_ptr<LogServer::Pkt> pResult(Command(&cmd));
}

//! 現在のスレッドの呼び出し深度の取得
intptr_t GlobalSocketLogger::GetDepth() {
	return g_pInstance->Depth();
}

//! 現在のスレッドの呼び出し深度をインクリメント
intptr_t GlobalSocketLogger::IncrementDepth() {
	return g_pInstance->Depth()++;
}

//! 現在のスレッドの呼び出し深度をデクリメント
intptr_t GlobalSocketLogger::DecrementDepth() {
	return g_pInstance->Depth()--;
}


//==============================================================================
//		LogServer クラス

//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
void LogServer::Startup() {
	Socket::Startup();
}

//! 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
void LogServer::Cleanup() {
	Socket::Cleanup();
}

LogServer::LogServer() {
	m_RequestStop = false;
	m_BinaryLog = false;
	m_CommandWriteLogHandler = NULL;
}

//! 別スレッドでサーバー処理を開始する、スレッドアンセーフ
ibool LogServer::Start(const wchar_t* pszLogFolder, int port) {
	m_LogFolder = FilePath::Combine(Directory::GetExeDirectoryW(), pszLogFolder);
	m_LogFile.Close();

	if (!Directory::Exists(m_LogFolder.c_str())) {
		if (!Directory::Create(m_LogFolder.c_str())) {
			std::cerr << "Failed to create directory." << std::endl;
			return false;
		}
	}


	jk::Socket sock;

	if(!sock.Create()) {
		std::cerr << "Failed to create socket." << std::endl;
		return false;
	}

    char strPort[32];
	std::strstream ss(strPort, 30, std::ios::out);
	ss << port << std::ends;

	jk::Socket::Endpoint ep;
	if(!ep.Create(NULL, strPort, jk::Socket::St::Stream, jk::Socket::Af::IPv4)) {
		std::cerr << "Failed to create endpoint." << std::endl;
		return false;
	}

	if(!sock.Bind(ep)) {
		std::cerr << "Failed to bind." << std::endl;
		return false;
	}

	if(!sock.Listen(10)) {
		std::cerr << "Failed to listen." << std::endl;
		return false;
	}

	m_RequestStop = false;
	m_RequestStopEvent.Reset();
	m_AcceptanceSocket.Attach(sock.Detach());
	return m_AcceptanceThread.Start(&ThreadStart, this);
}

//! サーバー処理スレッドを停止する、スレッドアンセーフ
void LogServer::Stop() {
	// まずサーバーの接続受付を停止させる
	m_RequestStop = true;
	m_RequestStopEvent.Set();
	m_AcceptanceSocket.Shutdown(Socket::Sd::Both);
	m_AcceptanceSocket.Close();
	m_AcceptanceThread.Join();

	// 全クライアントの通信スレッドを停止する
	std::vector<Client*> clients;

	{
		CriticalSectionLock lock(&m_ClientsCs);
		clients = m_Clients;
	}

	for(size_t i = 0; i < clients.size(); i++) {
		RemoveClient(clients[i], true);
	}
}

//! ログファイルへ書き込む
void LogServer::Write(const char* bytes, size_t size) {
	CriticalSectionLock lock(&m_LogFileCs);
	if (m_LogFile.IsInvalidHandle()) {
		std::wstringstream ss;

		// 必要なら年月ディレクトリ作成
		DateTimeValue now = DateTime::Now().Value();
		ss << std::setfill(L'0');

		ss
			<< std::setw(4) << now.Year
			<< std::setw(2) << now.Month;

		std::wstring dir = FilePath::Combine(m_LogFolder, ss.str());
		if (!Directory::Exists(dir.c_str())) {
			if (!Directory::Create(dir.c_str())) {
				std::cerr << "Failed to create directory." << std::endl;
				return;
			}
		}

		// ファイル作成
		ss.str(L"");
		ss.clear(std::wstringstream::goodbit);
		ss
			<< std::setw(4) << now.Year
			<< std::setw(2) << now.Month
			<< std::setw(2) << now.Day
			<< std::setw(2) << now.Hour
			<< std::setw(2) << now.Minute
			<< std::setw(2) << now.Second
			<< std::setw(3) << now.Milliseconds
			<< L".csv";

#if 1700 <= _MSC_VER
		dir = std::move(FilePath::Combine(dir, ss.str().c_str()));
#else
		dir = FilePath::Combine(dir, ss.str().c_str());
#endif

		if (!m_LogFile.Open(dir.c_str(), File::AccessWrite | File::AccessRead, File::OpenAppend | File::OpenCreate)) {
			std::cerr << "Failed to create log file." << std::endl;
			return;
		}
	}
	m_LogFile.Write(bytes, size);
}

//! バイナリ形式でログ出力するかどうか設定する
void LogServer::CommandBinaryLog(SocketRef sock, PktCommandBinaryLog* pCmd) {
	m_BinaryLog = pCmd->Binary != 0;
}

//! ログ出力コマンド処理
void LogServer::CommandWriteLog(SocketRef sock, Pkt* pCmd, const std::string& remoteName) {
	// ハンドラ呼び出し
	CommandWriteLogHandler handler = m_CommandWriteLogHandler;
	if (handler != NULL) {
		handler(sock, (PktCommandLogWrite*)pCmd, remoteName.c_str());
	}

	if (m_BinaryLog) {
		// バイナリ形式でログ出力
		std::vector<uint8_t> buf;

		uint64_t utc = DateTime::NowUtc().UnixTimeMs();
		remoteName.size();


		// TODO: now.Tick
	} else {
		// テキスト形式でログ出力
		std::stringstream ss;

		// 日時登録
		DateTimeValue now = DateTime::Now().Value();
		ss << std::setfill('0');
		ss
			<< std::setw(4) << now.Year << "/"
			<< std::setw(2) << now.Month << "/"
			<< std::setw(2) << now.Day << " "
			<< std::setw(2) << now.Hour << ":"
			<< std::setw(2) << now.Minute << ":"
			<< std::setw(2) << now.Second << "."
			<< std::setw(3) << now.Milliseconds;

		// クライアントIPを登録
		ss << ",Ip" << remoteName;

		// テキストを登録
		ss << std::string(((PktCommandLogWrite*)pCmd)->Text, ((PktCommandLogWrite*)pCmd)->Text + (pCmd->Size - sizeof(pCmd->Command)));

		// ログテキストを取得
#if 1700 <= _MSC_VER
		std::string s = std::move(ss.str());
#else
		std::string s = ss.str();
#endif

		// ファイルへ書き込み
		Write(&s[0], s.size());
	}

	// 応答を返す
	Pkt result;
	result.Size = sizeof(result.Result);
	result.Result = ResultEnum::Ok;
	sock.Send(&result, result.PacketSize());
}

//! フラッシュコマンド処理
void LogServer::CommandFlush(SocketRef sock, Pkt* pCmd) {
	// フラッシュ
	{
		CriticalSectionLock lock(&m_LogFileCs);
		if (!m_LogFile.IsInvalidHandle())
			m_LogFile.Flush();
	}

	// 応答を返す
	Pkt result;
	result.Size = sizeof(result.Result);
	result.Result = ResultEnum::Ok;
	sock.Send(&result, result.PacketSize());
}

//! 現在のログファイルを閉じる
void LogServer::CommandFileClose(SocketRef sock, Pkt* pCmd) {
	// ファイル閉じる
	{
		CriticalSectionLock lock(&m_LogFileCs);
		m_LogFile.Close();
	}

	// 応答を返す
	Pkt result;
	result.Size = sizeof(result.Result);
	result.Result = ResultEnum::Ok;
	sock.Send(&result, result.PacketSize());
}

//! CommandWriteLog 内から呼び出されるハンドラを設定する
void LogServer::SetCommandWriteLogHandler(CommandWriteLogHandler handler) {
	m_CommandWriteLogHandler = handler;
}

//! CommandWriteLog 内から呼び出されるハンドラを取得する
LogServer::CommandWriteLogHandler LogServer::GetCommandWriteLogHandler() {
	return m_CommandWriteLogHandler;
}

//! 接続受付スレッド開始アドレス
intptr_t LogServer::ThreadStart(void* pObj) {
	((LogServer*)pObj)->ThreadProc();
	return 0;
}

//! 接続受付スレッド処理
void LogServer::ThreadProc() {
	SocketRef sock = m_AcceptanceSocket;

	for(;;) {
		sockaddr_storage saddr;
		socklen_t saddrlen = sizeof(saddr);
		jk::Socket client(sock.Accept(&saddr, &saddrlen));
		if(client.IsInvalidHandle())
			break;

		std::string remoteName = jk::Socket::GetRemoteName(saddr);
		std::cout << "Connected from " << remoteName << std::endl;

		AddClient(new Client(this, client.Detach(), remoteName.c_str()));
	}
}

//! 指定クライアントを管理下へ追加する
void LogServer::AddClient(Client* pClient) {
	CriticalSectionLock lock(&m_ClientsCs);
	m_Clients.push_back(pClient);
}

//! 指定クライアントを管理下から除外する
bool LogServer::RemoveClient(Client* pClient, bool wait) {
	CriticalSectionLock lock(&m_ClientsCs);
	for(size_t i = 0; i < m_Clients.size(); i++) {
		if(m_Clients[i] == pClient) {
			m_Clients.erase(m_Clients.begin() + i);
			lock.Detach(true);
			pClient->Stop(wait);
			return true;
		}
	}
	return false;
}


//==============================================================================
//		LogServer::Client クラス

//! コンストラクタ、クライアント通信スレッドが開始される
LogServer::Client::Client(LogServer* pOwner, Socket::Handle hClient, const char* pszRemoteName) {
	m_pOwner = pOwner;
	m_Socket.Attach(hClient);
	m_RemoteName = pszRemoteName;
	m_Thread.Start(&ThreadStart, this);
}

//! クライアント通信スレッドを停止する
void LogServer::Client::Stop(bool wait) {
}

intptr_t LogServer::Client::ThreadStart(void* pObj) {
	((LogServer::Client*)pObj)->ThreadProc();
	return 0;
}

//! クライアント用通信スレッド処理
void LogServer::Client::ThreadProc() {
	SocketRef sock = m_Socket;
	std::string remoteName = m_RemoteName;
	std::vector<char> buf(4096);
	std::stringstream ss;

	// ディレイは必要ない
	sock.SetNoDelay(1);

	// 受信ループ
	for(;;) {
		LogServer::Pkt* pCmd = (LogServer::Pkt*)&buf[0];

		// パケットサイズ受信
		if(RecvToBytes(sock, &pCmd->Size, sizeof(pCmd->Size)) <= 0)
			break;
		if(pCmd->Size < sizeof(pCmd->Command))
			break; // エラー、コマンドID分のサイズは絶対必要
		if(0x100000 < pCmd->Size)
			break; // あまりにも要求サイズが大きい場合にもエラー

		// バッファサイズが足りないなら拡張する
		intptr_t requiredSize = pCmd->PacketSize();
		if((intptr_t)buf.size() < requiredSize) {
			buf.resize(requiredSize);
			pCmd = (LogServer::Pkt*)&buf[0];
		}

		// コマンドID以降を受信
		if(RecvToBytes(sock, &pCmd->Command, pCmd->Size) <= 0)
			break;

		// コマンド別処理
		switch(pCmd->Command) {
		case LogServer::CommandEnum::WriteLog:
			m_pOwner->CommandWriteLog(sock, pCmd, remoteName);
			break;
		case LogServer::CommandEnum::Flush:
			m_pOwner->CommandFlush(sock, pCmd);
			break;
		case LogServer::CommandEnum::FileClose:
			m_pOwner->CommandFileClose(sock, pCmd);
			break;
		}
	}

	// ソケットクローズ
	sock.Shutdown(Socket::Sd::Both);
	sock.Close();

	std::cout << "Disconnected from " << remoteName << std::endl;

	// 自分自身を破棄
	delete this;
}


//==============================================================================
//		GlobalSocketLogger::Frame クラス

GlobalSocketLogger::Frame::Frame(const wchar_t* pszFrameName, const wchar_t* pszArgs) {
#if defined _MSC_VER
	this->EnterTime = Clock::SysNS();
	this->FrameName = pszFrameName;

	IncrementDepth();

	std::wstringstream ss;

	// 呼び出し深さ追加
	ss << GetDepth() << JUNKLOG_DELIMITER;
	// フレーム名と引数追加
	ss << L"\"+: " << this->FrameName << L"(" << (pszArgs != NULL ? pszArgs : L"") << L")\"\n";

	// ログをサーバーへ送る
	WriteLog(ss.str().c_str());
#else
#error gcc version is not implemented.
#endif
}

GlobalSocketLogger::Frame::~Frame() {
#if defined _MSC_VER
	std::wstringstream ss;

	// 呼び出し深さ追加
	ss << GetDepth() << JUNKLOG_DELIMITER;
	// フレーム名と所要時間追加
	ss << L"\"-: " << this->FrameName << JUNKLOG_DELIMITER << (Clock::SysNS() - this->EnterTime) / 1000000 << L"ms\"\n";

	// ログをサーバーへ送る
	WriteLog(ss.str().c_str());

	DecrementDepth();
#else
#error gcc version is not implemented.
#endif
}

_JUNK_END
