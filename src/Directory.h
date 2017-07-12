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
	static ibool Exists(const char* pszDir); //!< 指定されたディレクトリが存在しているか調べる
	static ibool Create(const char* pszDir); //!< 指定されたディレクトリを作成する
};

_JUNK_END

#endif
