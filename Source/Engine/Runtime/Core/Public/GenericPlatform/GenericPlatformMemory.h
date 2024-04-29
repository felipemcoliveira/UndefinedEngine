#pragma once

#include "HAL/Platform.h"
#include <exception>

struct FGenericPlatformMemory
{
	FORCEINLINE static void* SystemMalloc(SIZE_T size)
	{
		NOT_IMPLEMENTED()
	}

	FORCEINLINE static void SystemFree(void* ptr)
	{
		NOT_IMPLEMENTED()
	}

	FORCEINLINE static bool TryGetMemorySize(void*& ptr, SIZE_T& outSize)
	{
		return false;
	}

	[[noreturn]] FORCEINLINE static void OnOutOfMemory()
	{
		NOT_IMPLEMENTED()
	}
};
