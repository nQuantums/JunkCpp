#pragma once
#ifndef __JUNK_GLOBALSOCKETLOGGER_H__
#define __JUNK_GLOBALSOCKETLOGGER_H__

#include "JunkConfig.h"
#include "JunkDef.h"
#include "Socket.h"
#include "Thread.h"
#include "File.h"
#include <vector>
#include <string.h>

_JUNK_BEGIN

//! GlobalSocketLogger からのログを処理するサーバー処理クラス
class LogServer {
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

//! ソケットを使いサーバーへログを送るシングルトンクラス
class GlobalSocketLogger {
public:
    static void Startup(const wchar_t* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Startup(const char* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Startup(wchar_t* pszIniFile = L"GlobalSocketLogger.ini"); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
	static LogServer::Pkt* Command(LogServer::Pkt* pCmd); //!< サーバーへコマンドパケットを送り応答を取得する、スレッドセーフ
    static void WriteLog(const wchar_t* pszText); //!< サーバーへログを送る、スレッドセーフ
    static void Flush(); //!< サーバーへログをファイルへフラッシュ要求
};

_JUNK_END

#endif
