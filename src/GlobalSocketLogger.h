#pragma once
#ifndef __JUNK_GLOBALSOCKETLOGGER_H__
#define __JUNK_GLOBALSOCKETLOGGER_H__

#include "JunkConfig.h"
#include "JunkDef.h"
#include "Socket.h"
#include "Thread.h"
#include <fstream>
#include <vector>

_JUNK_BEGIN

//! ソケットを使いサーバーへログを送るシングルトンクラス
class GlobalSocketLogger {
public:
    static void Startup(const char* pszHost, int port); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ
    static void Write(const char* pszText); //!< サーバーへログを送る、スレッドセーフ
};

//! GlobalSocketLogger からのログを処理するサーバー処理クラス
class SocketLoggerServer {
public:
	//! サーバーへのコマンドID
#if _MSC_VER <= 1500
	enum CommandEnum {
#else
	enum class CommandEnum : int32_t {
#endif
		Unknown = 0,
		LogWrite,
		Flush,
	};

	//! サーバーからの応答ID
#if _MSC_VER <= 1500
	enum ResultEnum {
#else
	enum class ResultEnum : int32_t {
#endif
		Ok = 0,
		Error,
	};

#pragma pack(push, 1)
	//! サーバーへのコマンドパケット
	struct PktCommand {
		int32_t Size; // 以降に続くパケットバイト数
		CommandEnum Command; //!< コマンドID

		_FINLINE intptr_t PacketSize() const {
			return sizeof(this->Size) + this->Size;
		}
	};

	//! ログ出力コマンドパケット
	struct PktCommandLogWrite : public PktCommand {
		char Text[1]; //!< 文字列データ
	};

	//! サーバーからの応答パケット
	struct PktResult {
		int32_t Size; // 以降に続くパケットバイト数
		ResultEnum Result; //!< 応答コードID
	};
#pragma pack(pop)

    static void Startup(); //!< ログ出力先など初期化、プログラム起動時一回だけ呼び出す、スレッドアンセーフ
    static void Cleanup(); //!< 終了処理、プログラム終了時一回だけ呼び出す、スレッドアンセーフ

	SocketLoggerServer();

	ibool Start(const char* pszLogFolder, int port); //!< 別スレッドでサーバー処理を開始する、スレッドアンセーフ
	void Stop(); //!< サーバー処理スレッドを停止する、スレッドアンセーフ
	void Write(const char* pText); //!< ログファイルへ書き込む

private:
	//! クライアント毎の処理
	class Client {
	public:
		Client(SocketLoggerServer* pOwner, Socket::Handle hClient, const char* pszRemoteName); //!< コンストラクタ、クライアント通信スレッドが開始される

		void Stop(bool wait = false); //!< クライアント通信スレッドを停止する

	private:
		static intptr_t ThreadStart(void* pObj);
		void ThreadProc();

		SocketLoggerServer* m_pOwner;
		Socket m_Socket;
		std::string m_RemoteName;
		Thread m_Thread;
	};

	static intptr_t ThreadStart(void* pObj); //!< 接続受付スレッド開始アドレス
	void ThreadProc(); //!< 接続受付スレッド処理
	void AddClient(Client* pClient); //!< 指定クライアントを管理下へ追加する
	bool RemoveClient(Client* pClient, bool wait = false); //!< 指定クライアントを管理下から除外する

	std::ofstream m_OStream; //!< ファイル出力用ストリーム
	CriticalSection m_OStreamCs; //!< m_OStream アクセス排他処理用

	volatile ibool m_RequestStop; //!< サーバー停止要求フラグ
	Event m_RequestStopEvent; //!< サーバー停止要求イベント
	Socket m_AcceptanceSocket; //!< 接続受付ソケット
	Thread m_AcceptanceThread; //!< 接続受付処理スレッド

	std::vector<Client*> m_Clients; //!< クライアント処理配列
	CriticalSection m_ClientsCs; //!< m_Clients アクセス排他処理用
};

_JUNK_END

#endif
