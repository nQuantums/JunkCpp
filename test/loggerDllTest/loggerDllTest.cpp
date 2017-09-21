// loggerDllTest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include "../../src/Logger.h"

#if defined(_WIN64)
#pragma comment(lib, "../../test/loggerDll/x64/Release/loggerDll.lib")
#else
#pragma comment(lib, "../../test/loggerDll/Win32/Release/loggerDll32.lib")
#endif


bool LogClientTest();


int _tmain(int argc, _TCHAR* argv[])
{
	LogClientTest();

	return 0;
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
	jk_Logger_Startup(L"192.168.56.1", 33777);

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit") {
			break;
		} else if (line == "func") {
			//Struct1 s1;
			//s1.Func();
			//for (int i = 0; i < 10; i++)
			//	s1.Func1(i + 1);
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
		}
	}

	return true;
}
