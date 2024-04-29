#pragma once

#include "GenericPlatform/GenericPlatformMemory.h"

#include <heapapi.h>

struct FWindowsPlatformMemory;
typedef FWindowsPlatformMemory FPlatformMemory;

struct FWindowsPlatformMemory : public FGenericPlatformMemory
{
	FORCEINLINE static void* SystemMalloc(SIZE_T size)
	{
		HANDLE hProcessHeap = GetProcessHeap();
		void* ptr = HeapAlloc(hProcessHeap, 0, size);
		
		if (ptr == nullptr)
		{
			OnOutOfMemory();
		}
		
		return ptr;
	}

	FORCEINLINE static void SystemFree(void* ptr)
	{
		HANDLE hProcessHeap = GetProcessHeap();
		BOOL bSuccess = HeapFree(hProcessHeap, 0, ptr);
		CHECK(bSuccess);
	}

	FORCEINLINE static bool TryGetMemorySize(void* ptr, SIZE_T& outSize)
	{
		HANDLE hProcessHeap = GetProcessHeap();
		SIZE_T size = HeapSize(hProcessHeap, 0, ptr);
		if (size == 0)
		{
			return false;
		}
		outSize = size;
		return true;
	}
};