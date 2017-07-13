#include <string>
#include <iostream>
#include <sstream>
#include <strstream>
#include <iomanip>
#include <string.h>
#include "GlobalSocketLogger.h"

_JUNK_BEGIN


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

static CriticalSection g_CS;
static Socket g_Socket;
static std::string g_Host;
static int g_Port;
static std::string g_LocalIpAddress;

//! まだ接続していなかったら接続してソケットを返す
static SocketRef GetSocket() {
	if(g_Socket.IsInvalidHandle()) {
		g_LocalIpAddress = Socket::GetLocalIPAddress(Socket::Af::IPv4);

        char port[32];
		std::strstream ss(port, 30, std::ios::out);
		ss << g_Port << std::ends;

		Socket::Endpoint ep;
		ep.Create(g_Host.c_str(), port, Socket::St::Stream, Socket::Af::IPv4, false);
		g_Socket.Connect(ep);
	}
	return g_Socket;
}


//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
void GlobalSocketLogger::Startup(const char* pszHost, int port) {
	g_Host = pszHost;
	g_Port = port;
	Socket::Startup();
}

//! 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
void GlobalSocketLogger::Cleanup() {
	Socket::Cleanup();
}

//! サーバーへログを送る、スレッドセーフ
void GlobalSocketLogger::Write(const char* pszText) {
	CriticalSectionLock lock(&g_CS);
	SocketRef sock = GetSocket();
	sock.Send(pszText, ::strlen(pszText));
}


//==============================================================================
//		SocketLoggerServer クラス

//! ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
void SocketLoggerServer::Startup() {
	Socket::Startup();
}

//! 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
void SocketLoggerServer::Cleanup() {
	Socket::Cleanup();
}

SocketLoggerServer::SocketLoggerServer() {
	m_RequestStop = false;
}

//! 別スレッドでサーバー処理を開始する、スレッドアンセーフ
ibool SocketLoggerServer::Start(const char* pszLogFolder, int port) {
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
void SocketLoggerServer::Stop() {
	// まずサーバーの接続受付を停止させる
	m_RequestStop = true;
	m_RequestStopEvent.Set();
	m_AcceptanceThread.Join();

	// 全クライアントの通信スレッドを停止する
	std::vector<Client*> clients;

	{
		CriticalSectionLock lock(&m_ClientsCs);
		clients = m_Clients;
	}

	for(size_t i = 0; i < clients.size(); i++) {
		RemoveClient(clients[i]);
	}
}

//! ログファイルへ書き込む
void SocketLoggerServer::Write(const char* pText) {
	CriticalSectionLock lock(&m_OStreamCs);
	m_OStream << pText;
}

//! 接続受付スレッド開始アドレス
intptr_t SocketLoggerServer::ThreadStart(void* pObj) {
	((SocketLoggerServer*)pObj)->ThreadProc();
	return 0;
}

//! 接続受付スレッド処理
void SocketLoggerServer::ThreadProc() {
	SocketRef sock = m_AcceptanceSocket;

	for(;;) {
		sockaddr_storage saddr;
		socklen_t saddrlen = sizeof(saddr);
		jk::Socket client(sock.Accept(&saddr, &saddrlen));
		if(client.IsInvalidHandle())
			break;

		std::cout << jk::Socket::GetRemoteName(saddr);

		client.SetNoDelay(1);

		char buf[256];

		intptr_t n = client.Recv(buf, sizeof(buf) - 1);
		if(n <= 0)
			break;

		buf[n] = '\0';
		std::cout << buf << std::endl;
	}

}

//! 指定クライアントを管理下へ追加する
void SocketLoggerServer::AddClient(Client* pClient) {
	CriticalSectionLock lock(&m_ClientsCs);
	m_Clients.push_back(pClient);
}

//! 指定クライアントを管理下から除外する
bool SocketLoggerServer::RemoveClient(Client* pClient, bool wait) {
	CriticalSectionLock lock(&m_ClientsCs);
	for(size_t i = 0; i < m_Clients.size(); i++) {
		if(m_Clients[i] == pClient) {
			pClient->Stop(wait);
			m_Clients.erase(m_Clients.begin() + i);
			return true;
		}
	}
	return false;
}


//==============================================================================
//		SocketLoggerServer クラス

//! コンストラクタ、クライアント通信スレッドが開始される
SocketLoggerServer::Client::Client(SocketLoggerServer* pOwner, Socket::Handle hClient, const char* pszRemoteName) {
	m_pOwner = pOwner;
	m_Socket.Attach(hClient);
	m_RemoteName = pszRemoteName;
	m_Thread.Start(&ThreadStart, this);
}

//! クライアント通信スレッドを停止する
void SocketLoggerServer::Client::Stop(bool wait) {
}

intptr_t SocketLoggerServer::Client::ThreadStart(void* pObj) {
	((SocketLoggerServer::Client*)pObj)->ThreadProc();
	return 0;
}

//! クライアント用通信スレッド処理
void SocketLoggerServer::Client::ThreadProc() {
	SocketRef sock = m_Socket;
	std::string remoteName = m_RemoteName;
	std::vector<char> buf(4096);
	std::stringstream ss;

	// ディレイは必要ない
	sock.SetNoDelay(1);

	// 受信ループ
	for(;;) {
		SocketLoggerServer::PktCommandLogWrite* pCmd = (SocketLoggerServer::PktCommandLogWrite*)&buf[0];

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
			pCmd = (SocketLoggerServer::PktCommandLogWrite*)&buf[0];
		}

		// コマンドID以降を受信
		if(RecvToBytes(sock, &pCmd->Command, sizeof(pCmd->Size)) <= 0)
			break;

		// コマンド別処理
		switch(pCmd->Command) {
		case SocketLoggerServer::CommandEnum::LogWrite:
			break;
		case SocketLoggerServer::CommandEnum::Flush:
			break;
		}

	}

	// ソケットクローズ
	sock.Shutdown(Socket::Sd::Both);
	sock.Close();

	// 自分自身を破棄
	delete this;
}


_JUNK_END
