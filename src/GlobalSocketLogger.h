#pragma once
#ifndef __JUNK_GLOBALSOCKETLOGGER_H__
#define __JUNK_GLOBALSOCKETLOGGER_H__

#include "JunkConfig.h"
#include "JunkDef.h"
#include "Socket.h"
#include "Thread.h"
#include "File.h"
#include <vector>
#include <sstream>
#include <string.h>

_JUNK_BEGIN

//! GlobalSocketLogger からのログを処理するサーバー処理クラス
class JUNKAPICLASS LogServer {
public:
	//! サーバーへのコマンドID
#if (defined _MSC_VER) && (_MSC_VER <= 1500)
	enum CommandEnum {
#else
	enum class CommandEnum : int32_t {
#endif
		Unknown = 0,
		WriteLog,
		Flush,
		FileClose,
		BinaryLog,
	};

	//! サーバーからの応答ID
#if (defined _MSC_VER) && (_MSC_VER <= 1500)
	enum ResultEnum {
#else
	enum class ResultEnum : int32_t {
#endif
		Ok = 0,
		Error,
	};

#pragma pack(push, 1)
	//! サーバーへのコマンド、応答パケット基本クラス
	struct Pkt {
		int32_t Size; // 以降に続くパケットバイト数
		union {
			CommandEnum Command; //!< コマンドID
			ResultEnum Result; //!< 応答コードID
		};

		//! パケット全体のサイズ(bytes)
		_FINLINE size_t PacketSize() const {
			return sizeof(this->Size) + this->Size;
		}

		static Pkt* Allocate(size_t size) {
			Pkt* pPkt = (Pkt*)new uint8_t[sizeof(pPkt->Size) + size];
			pPkt->Size = (int32_t)size;
			return pPkt;
		}

		static Pkt* Allocate(CommandEnum command, size_t dataSize) {
			Pkt* pPkt = Allocate(sizeof(pPkt->Command) + dataSize);
			pPkt->Command = command;
			return pPkt;
		}

		static Pkt* Allocate(CommandEnum command, const void* pData, size_t dataSize) {
			Pkt* pPkt = Allocate(sizeof(pPkt->Command) + dataSize);
			pPkt->Command = command;
			memcpy(&pPkt[1], pData, dataSize);
			return pPkt;
		}

		static void Deallocate(Pkt* pPkt) {
			if(pPkt != NULL)
				delete pPkt;
		}
	};

	//! ログ出力コマンドパケット
	struct PktCommandLogWrite : public Pkt {
		uint32_t Pid; //!< プロセスID
		uint32_t Tid; //!< スレッドID
		uint32_t Depth; //! 呼び出し階層深度
		char Text[1]; //!< UTF-8エンコードされた文字列データ

		//! テキストサイズ(bytes)
		_FINLINE size_t TextSize() const {
			return this->Size - sizeof(this->Command) - sizeof(this->Pid) - sizeof(this->Tid) - sizeof(this->Depth);
		}

		static PktCommandLogWrite* Allocate(uint32_t pid, uint32_t tid, uint32_t depth, const char* pszText, size_t size) {
			PktCommandLogWrite* pPkt = (PktCommandLogWrite*)Pkt::Allocate(CommandEnum::WriteLog, sizeof(uint32_t) * 3 + size);
			pPkt->Pid = pid;
			pPkt->Tid = tid;
			pPkt->Depth = depth;
			memcpy(pPkt->Text, pszText, size);
			return pPkt;
		}
	};

	//! バイナリ形式ログ出力設定コマンドパケット
	struct PktCommandBinaryLog : public Pkt {
		int32_t Binary; //!< 0以外ならバイナリ

		static PktCommandBinaryLog* Allocate(bool binary) {
			int32_t binaryValue = binary;
			PktCommandBinaryLog* pPkt = (PktCommandBinaryLog*)Pkt::Allocate(CommandEnum::BinaryLog, &binaryValue, sizeof(binaryValue));
			return pPkt;
		}
	};
#pragma pack(pop)

	//! CommandWriteLog 内から呼び出されるハンドラ
	typedef void (*CommandWriteLogHandler)(SocketRef sock, PktCommandLogWrite* pCmd, const char* pszRemoteName);

    static void Startup(); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ

	LogServer();

	ibool Start(const wchar_t* pszLogFolder, int port); //!< 別スレッドでサーバー処理を開始する、スレッドアンセーフ
	void Stop(); //!< サーバー処理スレッドを停止する、スレッドアンセーフ
	void Write(const char* bytes, size_t size); //!< ログファイルへ書き込む
	void CommandBinaryLog(SocketRef sock, PktCommandBinaryLog* pCmd); //!< バイナリ形式でログ出力するかどうか設定する
	void CommandWriteLog(SocketRef sock, PktCommandLogWrite* pCmd, const std::string& remoteName); //!< ログ出力コマンド処理
	void CommandFlush(SocketRef sock, Pkt* pCmd); //!< フラッシュコマンド処理
	void CommandFileClose(SocketRef sock, Pkt* pCmd); //!< 現在のログファイルを閉じる

	void SetCommandWriteLogHandler(CommandWriteLogHandler handler); //!< CommandWriteLog 内から呼び出されるハンドラを設定する
	CommandWriteLogHandler GetCommandWriteLogHandler(); //!< CommandWriteLog 内から呼び出されるハンドラを取得する

private:
	//! クライアント毎の処理
	class Client {
	public:
		Client(LogServer* pOwner, Socket::Handle hClient, const char* pszRemoteName); //!< コンストラクタ、クライアント通信スレッドが開始される

		void Stop(bool wait = false); //!< クライアント通信スレッドを停止する

	private:
		static intptr_t ThreadStart(void* pObj);
		void ThreadProc();

		LogServer* m_pOwner;
		Socket m_Socket;
		std::string m_RemoteName;
		Thread m_Thread;
	};

	static intptr_t ThreadStart(void* pObj); //!< 接続受付スレッド開始アドレス
	void ThreadProc(); //!< 接続受付スレッド処理
	void AddClient(Client* pClient); //!< 指定クライアントを管理下へ追加する
	bool RemoveClient(Client* pClient, bool wait = false); //!< 指定クライアントを管理下から除外する

	std::wstring m_LogFolder; //!< ログの置き場フォルダ
	File m_LogFile; //!< ログファイル
	CriticalSection m_LogFileCs; //!< m_LogFile アクセス排他処理用

	volatile ibool m_RequestStop; //!< サーバー停止要求フラグ
	Event m_RequestStopEvent; //!< サーバー停止要求イベント
	Socket m_AcceptanceSocket; //!< 接続受付ソケット
	Thread m_AcceptanceThread; //!< 接続受付処理スレッド

	std::vector<Client*> m_Clients; //!< クライアント処理配列
	CriticalSection m_ClientsCs; //!< m_Clients アクセス排他処理用

	bool m_BinaryLog; //!< バイナリ形式でログを出力するかどうか

	CommandWriteLogHandler m_CommandWriteLogHandler; //!< CommandWriteLog 内から呼び出されるハンドラ
};

#define JUNKLOG_DELIMITER (L',')

//! ソケットを使いサーバーへログを送るシングルトンクラス
class JUNKAPICLASS GlobalSocketLogger {
public:
	//! インスタンス
	struct Instance;
	
	static Instance* GetInstance(); //!< 通信用ソケットなどの情報を保持するインスタンスを取得する
	static void Startup(const wchar_t* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
    static void Startup(const char* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
	static void Startup(wchar_t* pszIniFile = L"GlobalSocketLogger.ini"); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
	static void Startup(Instance* pInstance); //!< 他DLLのインスタンスを指定して初期化する
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
	static LogServer::Pkt* Command(LogServer::Pkt* pCmd); //!< サーバーへコマンドパケットを送り応答を取得する、スレッドセーフ
	static void BinaryLog(bool binary); //!< サーバーのログ出力形式をバイナリかどうか設定する、スレッドセーフ
	static void WriteLog(uint32_t depth, const wchar_t* pszText); //!< サーバーへログを送る、スレッドセーフ
	static void Flush(); //!< サーバーへログをファイルへフラッシュ要求、スレッドセーフ
	static void FileClose(); //!< サーバーへ現在のログファイルを閉じる要求、スレッドセーフ
	static intptr_t GetDepth(); //!< 現在のスレッドの呼び出し深度の取得
	static intptr_t IncrementDepth(); //!< 現在のスレッドの呼び出し深度をインクリメント
	static intptr_t DecrementDepth(); //!< 現在のスレッドの呼び出し深度をデクリメント

	struct JUNKAPICLASS Frame {
		std::wstring FrameName; //!< フレーム名
		int64_t EnterTime; //!< フレーム開始時間

		Frame(const wchar_t* pszFrameName, const wchar_t* pszArgs = NULL);
		~Frame();
	};
};

#define JUNK_LOG_FUNC_ARGSVAR __jk_log_func_args__
#define JUNK_LOG_FUNC_BEGIN std::wstringstream JUNK_LOG_FUNC_ARGSVAR
#define JUNK_LOG_FUNC_ARGS_BEGIN(arg) std::wstringstream JUNK_LOG_FUNC_ARGSVAR; JUNK_LOG_FUNC_ARGSVAR << L#arg L" = " << (arg)
#define JUNK_LOG_FUNC_ARGS(arg) << L", " L#arg L" = " << (arg)
#define JUNK_LOG_FUNC_COMMIT ; jk::GlobalSocketLogger::Frame __jk_log_func__(__FUNCTIONW__, JUNK_LOG_FUNC_ARGSVAR.str().c_str())

#define JUNK_LOG_FRAME_ARGSVAR(name) __jk_log_frame_ ## name ## _args__
#define JUNK_LOG_FRAME_BEGIN(name) std::wstringstream JUNK_LOG_FRAME_ARGSVAR(name)
#define JUNK_LOG_FRAME_ARGS_BEGIN(name, arg) std::wstringstream JUNK_LOG_FRAME_ARGSVAR(name); JUNK_LOG_FRAME_ARGSVAR(name) << L#arg L" = " << (arg)
#define JUNK_LOG_FRAME_ARGS(arg) << L", " L#arg L" = " << (arg)
#define JUNK_LOG_FRAME_COMMIT(name) ; jk::GlobalSocketLogger::Frame __jk_log_frame_ ## name ## __(L#name, JUNK_LOG_FRAME_ARGSVAR(name).str().c_str())

#define JUNK_LOG_FUNC() JUNK_LOG_FUNC_BEGIN JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC1(arg1) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC2(arg1, arg2) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC3(arg1, arg2, arg3) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC4(arg1, arg2, arg3, arg4) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC5(arg1, arg2, arg3, arg4, arg5) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC6(arg1, arg2, arg3, arg4, arg5, arg6) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_ARGS(arg6) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC7(arg1, arg2, arg3, arg4, arg5, arg6, arg7) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_ARGS(arg6) JUNK_LOG_FUNC_ARGS(arg7) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC8(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_ARGS(arg6) JUNK_LOG_FUNC_ARGS(arg7) JUNK_LOG_FUNC_ARGS(arg8) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC9(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_ARGS(arg6) JUNK_LOG_FUNC_ARGS(arg7) JUNK_LOG_FUNC_ARGS(arg8) JUNK_LOG_FUNC_ARGS(arg9) JUNK_LOG_FUNC_COMMIT
#define JUNK_LOG_FUNC10(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) JUNK_LOG_FUNC_ARGS_BEGIN(arg1) JUNK_LOG_FUNC_ARGS(arg2) JUNK_LOG_FUNC_ARGS(arg3) JUNK_LOG_FUNC_ARGS(arg4) JUNK_LOG_FUNC_ARGS(arg5) JUNK_LOG_FUNC_ARGS(arg6) JUNK_LOG_FUNC_ARGS(arg7) JUNK_LOG_FUNC_ARGS(arg8) JUNK_LOG_FUNC_ARGS(arg9) JUNK_LOG_FUNC_ARGS(arg10) JUNK_LOG_FUNC_COMMIT

#define JUNK_LOG_FRAME(name) JUNK_LOG_FRAME_BEGIN(name) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME1(name, arg1) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME2(name, arg1, arg2) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME3(name, arg1, arg2, arg3) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME4(name, arg1, arg2, arg3, arg4) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME5(name, arg1, arg2, arg3, arg4, arg5) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME6(name, arg1, arg2, arg3, arg4, arg5, arg6) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_ARGS(arg6) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME7(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_ARGS(arg6) JUNK_LOG_FRAME_ARGS(arg7) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME8(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_ARGS(arg6) JUNK_LOG_FRAME_ARGS(arg7) JUNK_LOG_FRAME_ARGS(arg8) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME9(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_ARGS(arg6) JUNK_LOG_FRAME_ARGS(arg7) JUNK_LOG_FRAME_ARGS(arg8) JUNK_LOG_FRAME_ARGS(arg9) JUNK_LOG_FRAME_COMMIT(name)
#define JUNK_LOG_FRAME10(name, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) JUNK_LOG_FRAME_ARGS_BEGIN(name, arg1) JUNK_LOG_FRAME_ARGS(arg2) JUNK_LOG_FRAME_ARGS(arg3) JUNK_LOG_FRAME_ARGS(arg4) JUNK_LOG_FRAME_ARGS(arg5) JUNK_LOG_FRAME_ARGS(arg6) JUNK_LOG_FRAME_ARGS(arg7) JUNK_LOG_FRAME_ARGS(arg8) JUNK_LOG_FRAME_ARGS(arg9) JUNK_LOG_FRAME_ARGS(arg10) JUNK_LOG_FRAME_COMMIT(name)

_JUNK_END

#endif
