// loggerTest.cpp : �R���\�[�� �A�v���P�[�V�����̃G���g�� �|�C���g���`���܂��B
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include "../../src/GlobalSocketLogger.h"
#include "../../src/Thread.h"
#include "../../src/Error.h"
#include "../../src/Directory.h"

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
		LogClientTestDll();
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
	void Func() {
		JUNK_LOG_FUNC();
		jk::Thread::Sleep(100);
		{
			JUNK_LOG_FRAME(Frame);
		}
	}
	void Func1(int a) {
		JUNK_LOG_FUNC1(a);
		jk::Thread::Sleep(100);
		Func2(a, a * 2);
		{
			JUNK_LOG_FRAME1(Frame, a);
		}
	}
	void Func2(int a, int b) {
		JUNK_LOG_FUNC2(a, b);
		jk::Thread::Sleep(100);
		Func3(a, b, b * 2);
		{
			JUNK_LOG_FRAME2(Frame, a, b);
		}
	}
	void Func3(int a, int b, int c) {
		JUNK_LOG_FUNC3(a, b, c);
		jk::Thread::Sleep(100);
		for (int i = 0; i < 10; i++)
			Func4(a, b, c, c * 2);
		{
			JUNK_LOG_FRAME3(Frame, a, b, c);
		}
	}
	void Func4(int a, int b, int c, int d) {
		JUNK_LOG_FUNC4(a, b, c, d);
		jk::Thread::Sleep(100);
		{
			JUNK_LOG_FRAME4(Frame, a, b, c, d);
		}
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
			s1.Func();
			for (int i = 0; i < 10; i++)
				s1.Func1(i + 1);
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
		jk::Error::SetLastErrorFromWinErr();
		std::cout << jk::Directory::GetCurrentA() << std::endl;
		std::cout << jk::Error::GetLastErrorString() << std::endl;
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
				s1.Func1(i + 1);
		} else if (line == "flush") {
			jk::GlobalSocketLogger::Flush();
		} else if (line == "fclose") {
			jk::GlobalSocketLogger::FileClose();
		}
	}

	jk::GlobalSocketLogger::Cleanup();

	return true;
}