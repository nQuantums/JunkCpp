// loggerTest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include <iostream>
#include "../../src/GlobalSocketLogger.h"
#include "../../src/DateTime.h"

void LogServerTest();

int main() {
	auto secTick = jk::DateTime(2017, 7, 14, 18, 0, 1).Tick - jk::DateTime(2017, 7, 14, 18, 0, 0).Tick;
	std::cout << secTick  / 1000.0 << std::endl;
	return 0;
}


void LogServerTest() {
}
