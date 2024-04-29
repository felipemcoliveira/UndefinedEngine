#pragma once

namespace Private
{
	template<bool, typename T32Bits, typename T64Bits>
	struct TSelectPlatformTypeHelper { using Type = T32Bits; };

	template<typename T32Bits, typename T64Bits>
	struct TSelectPlatformTypeHelper<false, T32Bits, T64Bits> { using Type = T64Bits; };

	template<typename T32Bits, typename T64Bits> struct TSelectPlatformType
		: TSelectPlatformTypeHelper<sizeof(void*) == 4, T32Bits, T64Bits> { };
}

struct FGenericPlatformTypes
{
public:

	typedef unsigned char u8;
	typedef unsigned short u16;
	typedef unsigned int u32;
	typedef unsigned long long	u64;

public:

	typedef signed char i8;
	typedef signed short int i16;
	typedef signed int i32;
	typedef signed long long i64;

public:

	typedef char ANSICHAR;
	typedef wchar_t WIDECHAR;
	typedef u8 CHAR8;
	typedef u16 CHAR16;
	typedef u32 CHAR32;
	typedef WIDECHAR TCHAR;

public:

	typedef Private::TSelectPlatformType<u32, u64>::Type UPTRINT;
	typedef Private::TSelectPlatformType<i32, i64>::Type PTRINT;

public:

	typedef UPTRINT SIZE_T;
	typedef PTRINT SSIZE_T;
	typedef i32 TYPE_OF_NULL;
	typedef decltype(nullptr) TYPE_OF_NULLPTR;
};