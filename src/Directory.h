#pragma once
#ifndef __JUNK_DIRECTORY_H__
#define __JUNK_DIRECTORY_H__

#include "JunkConfig.h"
#include <vector>
#include <string>

_JUNK_BEGIN

//! ディレクトリ
class Directory {
public:
	static ibool GetCurrent(std::string& curDir); //!< カレントディレクトリを取得する
	static ibool Exists(const wchar_t* pszDir); //!< 指定されたディレクトリが存在しているか調べる
	static ibool Exists(const char* pszDir); //!< 指定されたディレクトリが存在しているか調べる
	static ibool Create(const wchar_t* pszDir); //!< 指定されたディレクトリを作成する
	static ibool Create(const char* pszDir); //!< 指定されたディレクトリを作成する

	static std::wstring GetCurrent(); //!< カレントディレクトリを取得する
	static std::wstring GetExeDirectory(); //!< 実行中EXEの置かれているディレクトリパス名を取得する
};

_JUNK_END

#endif
