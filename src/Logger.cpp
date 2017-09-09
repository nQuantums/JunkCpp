#include <string>
#include <iostream>
#include <sstream>
#include <strstream>
#include <iomanip>
#include <memory>
#include <string.h>
#include "GlobalSocketLogger.h"
#include "Encoding.h"
#include "FilePath.h"
#include "Directory.h"
#include "DateTime.h"
#include "Str.h"
#include "Clock.h"
#include "ThreadLocalStorage.h"
#include "Logger.h"

#if defined _MSC_VER
#include <Windows.h>
#include <stdlib.h>
#else
#error gcc version is not implemented.
#endif


static JUNK_TLS(bool) s_Recurse;


//==============================================================================
//		エクスポート関数

JUNKLOGGERAPI void JUNKLOGGERCALL jk_Logger_Startup(const wchar_t* pszHost, int port) {
	jk::GlobalSocketLogger::Startup(pszHost, port);
}

JUNKLOGGERAPI void JUNKLOGGERCALL jk_Logger_FrameStart(jk_Logger_Frame* pFrame, const wchar_t* pszFrameName, const wchar_t* pszArgs) {
	// APIフックからも呼ばれるため再入防止
	if(s_Recurse.Get())
		return;
	s_Recurse.Get() = true;

	pFrame->EnterTime = jk::Clock::SysNS();
#if defined _MSC_VER
	size_t len = wcslen(pszFrameName);
	pFrame->pFrameName = new wchar_t[len + 1];
	memcpy(pFrame->pFrameName, pszFrameName, (len + 1) * sizeof(wchar_t));

	jk::GlobalSocketLogger::IncrementDepth();

	std::wstringstream ss;

	// フレーム名と引数追加
	ss << pFrame->pFrameName << L"(" << (pszArgs != NULL ? pszArgs : L"") << L")";

	// ログをサーバーへ送る
	jk::GlobalSocketLogger::WriteLog((uint32_t)jk::GlobalSocketLogger::GetDepth(), jk::LogServer::LogTypeEnum::Enter, ss.str().c_str());
#else
#error gcc version is not implemented.
#endif

	s_Recurse.Get() = false;
}

JUNKLOGGERAPI void JUNKLOGGERCALL jk_Logger_FrameEnd(jk_Logger_Frame* pFrame) {
	// APIフックからも呼ばれるため再入防止
	if(s_Recurse.Get())
		return;
	s_Recurse.Get() = true;

#if defined _MSC_VER
	std::wstringstream ss;

	// フレーム名と所要時間追加
	ss << pFrame->pFrameName << JUNKLOG_DELIMITER << (jk::Clock::SysNS() - pFrame->EnterTime) / 1000000 << L"ms";

	// ログをサーバーへ送る
	jk::GlobalSocketLogger::WriteLog((uint32_t)jk::GlobalSocketLogger::GetDepth(), jk::LogServer::LogTypeEnum::Leave, ss.str().c_str());

	jk::GlobalSocketLogger::DecrementDepth();

	delete [] pFrame->pFrameName;
#else
#error gcc version is not implemented.
#endif

	s_Recurse.Get() = false;
}
