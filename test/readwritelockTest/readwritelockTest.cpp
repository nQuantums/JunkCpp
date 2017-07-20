// readwritelockTest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include <iomanip>
#include <sstream>
#include "../../src/Thread.h"

static intptr_t ThreadProc(void* pArg);

static jk::CriticalSection g_StdOutCs;

int main() {
	jk::Thread t1, t2;

	t1.Start(&ThreadProc, (void*)1);
	t2.Start(&ThreadProc, (void*)2);

	// TODO: テスト実装

	for (;;) {
		std::string line;
		std::getline(std::cin, line);
		if (line == "quit")
			break;
	}

	t1.Join();
	t2.Join();

	return 0;
}

intptr_t ThreadProc(void* pArg) {
	jk::CriticalSectionLock lock(&g_StdOutCs);
	std::cout << "Thread" << (intptr_t)pArg << std::endl;
	return 0;
}
