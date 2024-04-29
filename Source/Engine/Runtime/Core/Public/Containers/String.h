#pragma once

#include "HAL/Platform.h"
#include "TypeTraits.h"

#include <atomic>	

template<typename T, bool ThreadSafe = false>
struct TSharedPtr
{
private:

	using RefCountType = typename TConditional<ThreadSafe, std::atomic<i32>, i32>::Type;

public:

	FORCEINLINE TSharedPtr() : m_RefCount(nullptr), m_Ptr(nullptr) { }

public:

	FORCEINLINE TSharedPtr(const TSharedPtr& other) : m_RefCount(other.m_RefCount), m_Ptr(other.m_Ptr)
	{
		if (!m_RefCount)
		{
			return;
		}

		++(*m_RefCount);
	}

	FORCEINLINE TSharedPtr(TSharedPtr&& other) : m_RefCount(other.m_RefCount), m_Ptr(other.m_Ptr)
	{
		other.m_RefCount = nullptr;
		other.m_Ptr = nullptr;
	}

	FORCEINLINE ~TSharedPtr()
	{
		if (!m_RefCount)
		{
			return;
		}

		if (--(*m_RefCount) == 0)
		{
			// TODO: Use a custom allocator
			delete m_RefCount;
			delete m_Ptr;
		}
	}

public:

	FORCEINLINE i32 GetRefCount() const
	{
		return m_RefCount ? *m_RefCount : 0;
	}

private:

	RefCountType* m_RefCount;
	T* m_Ptr;
};

struct FString
{
	WCHAR* m_Data;
	u32 m_Size;
};