#pragma once

#include "GenericPlatform/GenericPlatformTypes.h"

#include <windows.h>
#include <winbase.h>

struct FWindowsPlatformTypes;
typedef FWindowsPlatformTypes FPlatformTypes;

struct FWindowsPlatformTypes : public FGenericPlatformTypes
{
#ifdef _WIN64
	typedef unsigned __int64 SIZE_T;
	typedef __int64 SSIZE_T;
#else
	typedef unsigned long SIZE_T;
	typedef long SSIZE_T;
#endif
};

#if defined(__clang__) || _MSC_VER >= 1900
	#define CONSTEXPR constexpr
#else
	#define CONSTEXPR
#endif 

#if defined(__GNUC__)
	#define GCC_ALIGN(n) __attribute__((aligned(n)))
	#define MS_ALIGN(n)
#else
	#define GCC_ALIGN(n)
	#define MS_ALIGN(n) __declspec(align(n)) 
#endif

#define FORCEINLINE __forceinline
#define ABSTRACT abstract
#define DLLIMPORT __declspec(dllimport)
#define DLLEXPORT __declspec(dllexpsort)


#define PLATFORM_64BITS (_WIN64)
#define PLATFORM_DESKTOP 1