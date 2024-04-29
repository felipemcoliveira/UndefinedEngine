#pragma once

#include "HAL/PreprocessorHelpers.h"

#include COMPILED_PLATFORM_HEADER(Platform.h)

// ---------------------------------------------------------------------------
//	Defines that haven't default values.
// ---------------------------------------------------------------------------

#if !defined(PLATFORM_64BITS)
	#error PLATFORM_64BITS not defined.
#endif

#if !defined(DLLEXPORT)
	#error DLLEXPORT not defined.
#endif

#if !defined(DLLIMPORT)
	#error DLLIMPORT not defined.
#endif

// ---------------------------------------------------------------------------
//	Defines that have default values.
// ---------------------------------------------------------------------------

#if !defined(PLATFORM_WINDOWS)
	#define PLATFORM_WINDOWS 0
#endif

#if !defined(FORCEINLINE)
	#define FORCEINLINE 
#endif

#if !defined(CONSTEXPR)
	#define CONSTEXPR
#endif

#if !defined(ABSTRACT)
	#define ABSTRACT
#endif

#if !defined(CHECK)
#define CHECK(Expression)
#endif

#if !defined(ASSERT)
	#define ASSERT(Expression)
#endif

#if !defined(NOT_IMPLEMENTED)
	#define NOT_IMPLEMENTED()
#endif

// ---------------------------------------------------------------------------
//	Computed defines
// ---------------------------------------------------------------------------

#define PLATFORM_32BITS (!PLATFORM_64BITS)

// ---------------------------------------------------------------------------
//	Types
// ---------------------------------------------------------------------------

typedef FPlatformTypes::u8 u8;
typedef FPlatformTypes::u16 u16;
typedef FPlatformTypes::u32 u32;
typedef FPlatformTypes::u64 u64;
typedef FPlatformTypes::i8 i8;
typedef FPlatformTypes::i16 i16;
typedef FPlatformTypes::i32 i32;
typedef FPlatformTypes::i64 i64;
typedef FPlatformTypes::ANSICHAR ANSICHAR;
typedef FPlatformTypes::WIDECHAR WIDECHAR;
typedef FPlatformTypes::CHAR8 CHAR8;
typedef FPlatformTypes::CHAR16 CHAR16;
typedef FPlatformTypes::CHAR32 CHAR32;
typedef FPlatformTypes::TCHAR TCHAR;
typedef FPlatformTypes::UPTRINT UPTRINT;
typedef FPlatformTypes::PTRINT PTRINT;
typedef FPlatformTypes::SIZE_T SIZE_T;
typedef FPlatformTypes::SSIZE_T SSIZE_T;
typedef FPlatformTypes::TYPE_OF_NULL TYPE_OF_NULL;
typedef FPlatformTypes::TYPE_OF_NULLPTR TYPE_OF_NULLPTR;

#include "TypeTraits.h"

static_assert(!PLATFORM_64BITS || sizeof(void*) == 8, "Pointer size is 64bit, but pointers are short.");
static_assert(PLATFORM_64BITS || sizeof(void*) == 4, "Pointer size is 32bit, but pointers are long.");

static_assert(char(-1) < char(0), "Unsigned char type test failed.");

static_assert(!TIsSame<ANSICHAR, WIDECHAR>::Value, "ANSICHAR and WIDECHAR should be different types.");
static_assert(!TIsSame<ANSICHAR, CHAR16>::Value, "ANSICHAR and CHAR16 should be different types.");
static_assert(!TIsSame<WIDECHAR, CHAR16>::Value, "WIDECHAR and CHAR16 should be different types.");
static_assert(TOr<TIsSame<TCHAR, ANSICHAR>, TIsSame<TCHAR, WIDECHAR>>::Value, "TCHAR should either be ANSICHAR or WIDECHAR.");

static_assert(sizeof(u8) == 1, "u8 type size test failed.");
static_assert(sizeof(u16) == 2, "u16 type size test failed.");
static_assert(sizeof(u32) == 4, "u32 type size test failed.");
static_assert(sizeof(u64) == 8, "u64 type size test failed.");

static_assert(sizeof(i8) == 1, "i8 type size test failed.");
static_assert(sizeof(i16) == 2, "i16 type size test failed.");
static_assert(sizeof(i32) == 4, "i32 type size test failed.");
static_assert(sizeof(i64) == 8, "i64 type size test failed.");

static_assert(sizeof(PTRINT) == sizeof(void*), "PTRINT type size test failed.");
static_assert(sizeof(UPTRINT) == sizeof(void*), "UPTRINT type size test failed.");
static_assert(sizeof(SIZE_T) == sizeof(void*), "SIZE_T type size test failed.");

static_assert(sizeof(ANSICHAR) == 1, "ANSICHAR type size test failed.");
static_assert(sizeof(WIDECHAR) == 2 || sizeof(WIDECHAR) == 4, "WIDECHAR type size test failed.");
static_assert(sizeof(CHAR16) == 2, "CHAR16 type size test failed.");

static_assert(i32(u8(-1)) == 0xFF, "u8 type sign test failed.");
static_assert(i32(u16(-1)) == 0xFFFF, "u16 type sign test failed.");
static_assert(i64(u32(-1)) == i64(0xFFFFFFFF), "u32 type sign test failed.");
static_assert(u64(-1) > u64(0), "u64 type sign test failed.");

static_assert(i32(i8(-1)) == -1, "i8 type sign test failed.");
static_assert(i32(i16(-1)) == -1, "i16 type sign test failed.");
static_assert(i64(i32(-1)) == i64(-1), "i32 type sign test failed.");
static_assert(i64(-1) < i64(0), "i64 type sign test failed.");

static_assert(PTRINT(-1) < PTRINT(0), "PTRINT type sign test failed.");
static_assert(UPTRINT(-1) > UPTRINT(0), "UPTRINT type sign test failed.");
static_assert(SIZE_T(-1) > SIZE_T(0), "SIZE_T type sign test failed.");
