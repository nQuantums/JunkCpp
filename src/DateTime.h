#pragma once
#ifndef __JUNK_DATETIME_H__
#define __JUNK_DATETIME_H__

#include "JunkConfig.h"
#include "JunkDef.h"

#if defined _MSC_VER
#include <Windows.h>
#else
#error gcc version is not implemented.
#endif

_JUNK_BEGIN

//! 年月日時分秒とミリ秒
struct DateTimeValue {
	uint16_t Year; //!< 年
	uint16_t Month; //!< 月、1～12
	uint16_t DayOfWeek; //!< 曜日、0:日曜日～6:土曜日
	uint16_t Day; //!< 日、1～31
	uint16_t Hour; //!< 時、0～23
	uint16_t Minute; //!< 分、0～59
	uint16_t Second; //!< 秒、0～59
	uint16_t Milliseconds; //!< ミリ秒、0～999
};

//! 日時
struct DateTime {
#if defined _MSC_VER
	uint64_t Tick; //!< 規定日時からの経過時間値

	//! 現在日時の取得
	static DateTime Now() {
		FILETIME ft;
		DateTime dt;
		::GetSystemTimeAsFileTime(&ft);
		::FileTimeToLocalFileTime(&ft, (FILETIME*)&dt.Tick);
		return dt;
	}

	DateTime() {
	}

	DateTime(uint16_t year, uint16_t month = 0, uint16_t day = 0, uint16_t hour = 0, uint16_t minute = 0, uint16_t second = 0, uint16_t msecond = 0) {
		SYSTEMTIME st;
		st.wYear = year;
		st.wMonth = month;
		st.wDay = day;
		st.wHour = hour;
		st.wMinute = minute;
		st.wSecond = second;
		st.wMilliseconds = msecond;
		::SystemTimeToFileTime(&st, (FILETIME*)&this->Tick);
	}

	//! 年月日時分秒とミリ秒を取得する
	DateTimeValue Value() const {
		SYSTEMTIME st;
		DateTimeValue dtv;
		::FileTimeToSystemTime((FILETIME*)&this->Tick, &st);

		dtv.Year = st.wYear;
		dtv.Month =  st.wMonth;
		dtv.DayOfWeek = st.wDayOfWeek;
		dtv.Day = st.wDay;
		dtv.Hour = st.wHour;
		dtv.Minute = st.wMinute;
		dtv.Second = st.wSecond;
		dtv.Milliseconds = st.wMilliseconds;

		return dtv;
	}

	bool operator==(const DateTime& dt) const {
		return this->Tick == dt.Tick;
	}
	bool operator<(const DateTime& dt) const {
		return this->Tick < dt.Tick;
	}
	bool operator<=(const DateTime& dt) const {
		return this->Tick <= dt.Tick;
	}
	bool operator>(const DateTime& dt) const {
		return this->Tick > dt.Tick;
	}
	bool operator>=(const DateTime& dt) const {
		return this->Tick >= dt.Tick;
	}
#else
#error gcc version is not implemented.
#endif
};

_JUNK_END

#endif
