// loggerTest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include "../../src/GlobalSocketLogger.h"
#include "../../src/Thread.h"

bool LogServerTest();
bool LogClientTest();
bool LogClientTestDll();

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
	} else if (mode == "client_dll") {
		LogClientTest();
	}

	return 0;
}


bool LogServerTest() {
	jk::LogServer::Startup();

	jk::LogServer server;
	if (!server.Start(L"Logs", 33777)) {
		std::cerr << "Failed to start log server." << std::endl;
		return false;
	}

	std::cout << "Log server started." << std::endl;

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit")
			break;
	}

	server.Stop();

	jk::LogServer::Cleanup();

	return true;
}

struct Struct1 {
	void Func1(int a, int b) {
		JUNK_LOG_FRAME2(a, b);
		jk::Thread::Sleep(100);
		Func2(a, b);
	}
	void Func2(int a, int b) {
		JUNK_LOG_FRAME2(a, b);
		jk::Thread::Sleep(100);
		Func3(a, b);
	}
	void Func3(int a, int b) {
		JUNK_LOG_FRAME2(a, b);
		jk::Thread::Sleep(100);
		for (int i = 0; i < 10; i++)
			Func4(a, b);
	}
	void Func4(int a, int b) {
		JUNK_LOG_FRAME2(a, b);
		jk::Thread::Sleep(100);
	}
};

bool LogClientTest() {
	jk::GlobalSocketLogger::Startup("127.0.0.1", 33777);

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit") {
			break;
		} else if (line == "func") {
			Struct1 s1;
			for (int i = 0; i < 10; i++)
				s1.Func1(32, 64);
		} else if (line == "flush") {
			jk::GlobalSocketLogger::Flush();
		} else if (line == "fclose") {
			jk::GlobalSocketLogger::FileClose();
		}
	}

	jk::GlobalSocketLogger::Cleanup();

	return true;
}

bool LogClientTestDll() {
	auto hDll = ::LoadLibraryW(L"astMulticast.dll");
	if (hDll == NULL) {
		std::cout << "Failed to load astMulticast.dll DLL." << std::endl;
		return false;
	}

	typedef jk::GlobalSocketLogger::Instance* (__stdcall* Func_GetGlobalSocketLoggerInstance)();
	auto GetGlobalSocketLoggerInstance = reinterpret_cast<Func_GetGlobalSocketLoggerInstance>(::GetProcAddress(hDll, "GetGlobalSocketLoggerInstance"));
	if (GetGlobalSocketLoggerInstance == NULL) {
		std::cout << "Failed to get load GetGlobalSocketLoggerInstance address." << std::endl;
		return false;
	}

	jk::GlobalSocketLogger::Startup(GetGlobalSocketLoggerInstance());

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit") {
			break;
		} else if (line == "func") {
			Struct1 s1;
			for (int i = 0; i < 10; i++)
				s1.Func1(32, 64);
		} else if (line == "flush") {
			jk::GlobalSocketLogger::Flush();
		} else if (line == "fclose") {
			jk::GlobalSocketLogger::FileClose();
		}
	}

	jk::GlobalSocketLogger::Cleanup();

	return true;
}
