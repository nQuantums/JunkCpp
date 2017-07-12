#include "Directory.h"
#include "Error.h"

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

_JUNK_END
