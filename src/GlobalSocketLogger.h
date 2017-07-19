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

		_FINLINE intptr_t PacketSize() const {
			return sizeof(this->Size) + this->Size;
		}

		static Pkt* Allocate(size_t size) {
			Pkt* pPkt = (Pkt*)new uint8_t[sizeof(pPkt->Size) + size];
			pPkt->Size = size;
			return pPkt;
		}

		static Pkt* Allocate(CommandEnum command, void* pData, size_t dataSize) {
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
		char Text[1]; //!< 文字列データ
	};
#pragma pack(pop)

    static void Startup(); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ

	LogServer();

	ibool Start(const wchar_t* pszLogFolder, int port); //!< 別スレッドでサーバー処理を開始する、スレッドアンセーフ
	void Stop(); //!< サーバー処理スレッドを停止する、スレッドアンセーフ
	void Write(const char* bytes, size_t size); //!< ログファイルへ書き込む
	void CommandWriteLog(SocketRef sock, Pkt* pCmd, const std::string& remoteName); //!< ログ出力コマンド処理
	void CommandFlush(SocketRef sock, Pkt* pCmd); //!< フラッシュコマンド処理
	void CommandFileClose(SocketRef sock, Pkt* pCmd); //!< 現在のログファイルを閉じる

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
};

#define JUNKLOG_DELIMITER (L',')

//! ソケットを使いサーバーへログを送るシングルトンクラス
class JUNKAPICLASS GlobalSocketLogger {
public:
	//! インスタンス
	struct Instance;
	
	static void Startup(const wchar_t* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
    static void Startup(const char* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
	static void Startup(wchar_t* pszIniFile = L"GlobalSocketLogger.ini"); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドセーフ
	static void Startup(Instance* pInstance); //!< 他DLLのインスタンスを指定して初期化する
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
	static LogServer::Pkt* Command(LogServer::Pkt* pCmd); //!< サーバーへコマンドパケットを送り応答を取得する、スレッドセーフ
    static void WriteLog(const wchar_t* pszText); //!< サーバーへログを送る、スレッドセーフ
	static void Flush(); //!< サーバーへログをファイルへフラッシュ要求
	static void FileClose(); //!< サーバーへ現在のログファイルを閉じる要求
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

#define JUNK_LOG_FRAMEARGS __frame_Args__
#define JUNK_LOG_DEF_FRAMEARGS std::wstringstream JUNK_LOG_FRAMEARGS
#define JUNK_LOG_DEF_FRAMEVAR jk::GlobalSocketLogger::Frame __frame_Var__(__FUNCTIONW__, JUNK_LOG_FRAMEARGS.str().c_str())
//#define JUNKLOG_FUNC(func) JUNKLOG_DEF_FUNCARGS(func); GlobalSocketLogger __frame_ #func __(GlobalSocketLogger::GetDepth(), L"(
#define JUNK_LOG_FRAME() JUNK_LOG_DEF_FRAMEARGS; JUNK_LOG_DEF_FRAMEVAR
#define JUNK_LOG_FRAME1(arg1) JUNK_LOG_DEF_FRAMEARGS; JUNK_LOG_FRAMEARGS << L#arg1 L" = " << (arg1); JUNK_LOG_DEF_FRAMEVAR
#define JUNK_LOG_FRAME2(arg1, arg2) JUNK_LOG_DEF_FRAMEARGS; JUNK_LOG_FRAMEARGS << L#arg1 L" = " << (arg1) << L", " L#arg2 L" = " << (arg2); JUNK_LOG_DEF_FRAMEVAR
//#define JUNKLOG_FUNC1(func, 

_JUNK_END

#endif
