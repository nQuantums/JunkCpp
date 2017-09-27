// loggerTest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include "../../src/GlobalSocketLogger.h"
#include "../../src/Logger.h"
#include "../../src/Thread.h"
#include "../../src/Error.h"
#include "../../src/Directory.h"
#include "../../src/DateTime.h"
#include "../../src/Logger.h"
#include <time.h>
#include <atltime.h>

bool LogServerTest();
bool LogClientTest();

int main(int argc, char *argv[]) {
	if (argc < 2) {
		std::cout << "loggerTest.exe <mode>" << std::endl;
		std::cout << "<mode> : server / client" << std::endl;
		return 0;
	}

	std::string mode = argv[1];
	if (mode == "server") {
		LogServerTest();
	} else if (mode == "client") {
		LogClientTest();
	}

	return 0;
}


static std::string g_MethodToHandle = "\"+: Struct1::Func4(";

void CommandWriteLogHandler(jk::SocketRef sock, jk::LogServer::PktCommandLogWrite* pCmd, const char* pszRemoteName) {
	//char* pszMethodPos = strchr(pCmd->Text, '"');
	//if (pszMethodPos == NULL)
	//	return;
	//if (strncmp(pszMethodPos, g_MethodToHandle.c_str(), g_MethodToHandle.size()) != 0)
	//	return;

	//std::cout << "Locking: " << pszLogText << std::endl;
	//::Sleep(1000);
}

bool LogServerTest() {
	jk::LogServer::Startup();
	jk::LogServer server;

	server.SetCommandWriteLogHandler(CommandWriteLogHandler);

	if (!server.Start(L"Logs", 33777)) {
		std::cerr << "Failed to start log server." << std::endl;
		return false;
	}

	std::cout << "Log server started." << std::endl;

	jk_Logger_Startup(L"127.0.0.1", 33777);
	JUNK_LOG_FUNC1(jk_ExeFileName());

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit") {
			break;
		} else if (line == "flush") {
			jk::GlobalSocketLogger::Flush();
		} else if (line == "fclose") {
			jk::GlobalSocketLogger::FileClose();
		} else if (line == "binary") {
			jk::GlobalSocketLogger::BinaryLog(true);
		} else if (line == "text") {
			jk::GlobalSocketLogger::BinaryLog(false);
		}
	}

	server.Stop();

	jk::LogServer::Cleanup();

	return true;
}

struct Struct1 {
	void Func() {
		JUNK_LOG_FUNC();
		//jk::Thread::Sleep(100);
		{
			JUNK_LOG_FRAME(Frame);
		}
	}
	void Func1(int a) {
		JUNK_LOG_FUNC1(a);
		//jk::Thread::Sleep(100);
		Func2(a, a * 2);
		{
			JUNK_LOG_FRAME1(Frame, a);
		}
	}
	void Func2(int a, int b) {
		JUNK_LOG_FUNC2(a, b);
		//jk::Thread::Sleep(100);
		Func3(a, b, b * 2);
		{
			JUNK_LOG_FRAME2(Frame, a, b);
		}
	}
	void Func3(int a, int b, int c) {
		JUNK_LOG_FUNC3(a, b, c);
		//jk::Thread::Sleep(100);
		for (int i = 0; i < 10; i++)
			Func4(a, b, c, c * 2);
		{
			JUNK_LOG_FRAME3(Frame, a, b, c);
		}
	}
	void Func4(int a, int b, int c, int d) {
		JUNK_LOG_FUNC4(a, b, c, d);
		//jk::Thread::Sleep(100);
		{
			JUNK_LOG_FRAME4(Frame, a, b, c, d);
		}
	}
	void Func5(const wchar_t* msg) {
		JUNK_LOG_FUNC1(msg);
	}
	void Func5(const char* msg) {
		JUNK_LOG_FUNC1(msg);
	}
	void Func5(const std::string& msg) {
		JUNK_LOG_FUNC1(msg);
	}
};

bool LogClientTest() {
	JUNK_LOG_FRAME1(Test, jk_ExeFileName()); // Startup() 呼び出し前に呼べるかのテスト
	jk::GlobalSocketLogger::Startup("127.0.0.1", 33777);
	JUNK_LOG_FUNC1(jk_ExeFileName());

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit") {
			break;
		} else if (line == "func") {
			Struct1 s1;
			//s1.Func();
			for (int i = 0; i < 10; i++) {
				//s1.Func1(i + 1);
				//s1.Func5(L"メッセージだよ");
				s1.Func5(L"UTF-16 メッセージだよ\r\n改行ありだよ");
				s1.Func5((const wchar_t*)nullptr);
				s1.Func5("SJis メッセージだよ\r\n改行ありだよ");
				s1.Func5(std::string("SJis の std::string メッセージだよ\r\n改行ありだよ"));
				s1.Func5((const char*)nullptr);
			}
		} else if (line == "flush") {
			jk::GlobalSocketLogger::Flush();
		} else if (line == "fclose") {
			jk::GlobalSocketLogger::FileClose();
		} else if (line == "binary") {
			jk::GlobalSocketLogger::BinaryLog(true);
		} else if (line == "text") {
			jk::GlobalSocketLogger::BinaryLog(false);
		}
	}

	jk::GlobalSocketLogger::Cleanup();

	return true;
}
