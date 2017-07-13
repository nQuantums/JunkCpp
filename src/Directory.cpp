#include "Directory.h"
#include "Error.h"
#include "FilePath.h"

#if defined __GNUC__

#include <unistd.h>
#include <linux/limits.h>

#elif defined  _WIN32

#include <Windows.h>

#endif


_JUNK_BEGIN

//! カレントディレクトリを取得する
ibool Directory::GetCurrent(
	std::string& curDir //<! [out] カレントディレクトリパス名が返る
) {
#if defined __GNUC__
	char buf[PATH_MAX];
	if (getcwd(buf, PATH_MAX) == NULL) {
		Error::SetLastErrorFromErrno();
		return false;
	}
	curDir = buf;
	return true;
#elif defined  _WIN32
	char buf[MAX_PATH];
	if (!::GetCurrentDirectoryA(sizeof(buf), buf)) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
	curDir = buf;
	return true;
#endif
}

//!< 指定されたディレクトリが存在しているか調べる
ibool Directory::Exists(
	const wchar_t* pszDir // [in] ディレクトリパス名
) {
#if defined __GNUC__
#error gcc version is not implemented.
#elif defined  _WIN32
	DWORD dwAttrib = ::GetFileAttributesW(pszDir);
	if(dwAttrib == INVALID_FILE_ATTRIBUTES) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
	return (dwAttrib != INVALID_FILE_ATTRIBUTES &&  (dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
#endif
}

//!< 指定されたディレクトリが存在しているか調べる
ibool Directory::Exists(
	const char* pszDir // [in] ディレクトリパス名
) {
#if defined __GNUC__
#error gcc version is not implemented.
#elif defined  _WIN32
	DWORD dwAttrib = ::GetFileAttributesA(pszDir);
	if(dwAttrib == INVALID_FILE_ATTRIBUTES) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
	return (dwAttrib != INVALID_FILE_ATTRIBUTES &&  (dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
#endif
}

//! 指定されたディレクトリを作成する
ibool Directory::Create(
	const wchar_t* pszDir // [in] ディレクトリパス名
) {
#if defined __GNUC__
#error gcc version is not implemented.
#elif defined  _WIN32
	if(!::CreateDirectoryW(pszDir, NULL)) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
	return true;
#endif
}


//! 指定されたディレクトリを作成する
ibool Directory::Create(
	const char* pszDir // [in] ディレクトリパス名
) {
#if defined __GNUC__
#error gcc version is not implemented.
#elif defined  _WIN32
	if(!::CreateDirectoryA(pszDir, NULL)) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
	return true;
#endif
}

//! カレントディレクトリを取得する
std::wstring Directory::GetCurrent() {
#if defined _MSC_VER
	wchar_t path[MAX_PATH];
	if (!::GetCurrentDirectoryW(MAX_PATH, path)) {
		Error::SetLastErrorFromWinErr();
		return false;
	}
#if 1700 <= _MSC_VER
	return std::move(std::wstring(path));
#else
	return path;
#endif
#else
#error gcc version is not implemented.
#endif
}

//! 実行中EXEの置かれているディレクトリパス名を取得する
std::wstring Directory::GetExeDirectory() {
#if defined _MSC_VER
	wchar_t path[MAX_PATH] = L"";
	::GetModuleFileNameW(NULL, path, MAX_PATH);
#if 1700 <= _MSC_VER
	return std::move(FilePath::GetDirectoryName(path));
#else
	return FilePath::GetDirectoryName(path);
#endif
#else
#error gcc version is not implemented.
#endif
}

_JUNK_END
