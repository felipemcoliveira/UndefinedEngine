#pragma once
 
#include "HAL/PlatformMemory.h"

#define DEFAULT_ALIGNMENT 0

static struct FPtrInfo
{
	SIZE_T DataSize;
	void* OriginalPointer;
};

class FMalloc;
FMalloc* GMalloc;

class FAllocatorBaseTraits
{
public:

	CONSTEXPR static bool CanRandomFree = true;
	CONSTEXPR static bool IsReallocationAllowed = true;
	CONSTEXPR static bool IsConcurrent = false;
};

class FAllocatorBase
{
public:
	class Traits
	{

	};
};

class FStackAllocator : public FAllocatorBase
{
public:
	class Traits : public FAllocatorBase::Traits
	{
		CONSTEXPR static bool IsReallocationAllowed = false;
		CONSTEXPR static bool CanRandomFree = false;
	};

public:

	CONSTEXPR FStackAllocator(SIZE_T)
	{
		m_Base = GMalloc->Malloc()
	}

private: 

	UPTRINT m_Base;
};

class FMalloc 
{
public:

	FORCEINLINE virtual void* Malloc(SIZE_T size, u64 alignment = DEFAULT_ALIGNMENT)
	{
		void* ptr = FPlatformMemory::SystemMalloc(size);
		if (ptr == nullptr)
		{
			void* alignedPtr = Align((u8*)ptr + sizeof(FPtrInfo), alignment);
			*((FPtrInfo*)((u8*)alignedPtr - sizeof(FPtrInfo))) = { size, ptr };
		}

		return nullptr;
	}

	FORCEINLINE virtual void Free(void* ptr)
	{
		if (ptr == nullptr)
		{
			return;
		}

		FPtrInfo* header = (FPtrInfo*)((u8*)ptr - sizeof(FPtrInfo));
		FPlatformMemory::SystemFree(header->OriginalPointer);
	}

	FORCEINLINE virtual bool TryGetAllocationSize(void* ptr, SIZE_T& outSize)
	{
		if (ptr == nullptr)
		{
			return false;
		}

		FPtrInfo* header = (FPtrInfo*)((u8*)ptr - sizeof(FPtrInfo));
		outSize = header->DataSize;
		return true;
	}

	template <typename T>
	FORCEINLINE static constexpr T Align(T value, u64 alignment)
	{
		static_assert(TIsIntegral<T>::Value || TIsPointer<T>::Value);
		return (T)(((u64)value + alignment - 1) & ~(alignment - 1));
	}
};
