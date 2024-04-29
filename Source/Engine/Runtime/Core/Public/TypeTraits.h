#pragma once

#include "HAL/Platform.h"

/*--------------------------------------------------------------------------*/

namespace Private
{
	template<typename T>
	struct TType
	{
		using Type = T;
	};
}

/*--------------------------------------------------------------------------*/ 

template<typename T, T U>
struct TIntegralConst
{
	CONSTEXPR static T Value = U;
};

/*--------------------------------------------------------------------------*/

template<bool Predicate> struct TConstBoolean : TIntegralConst<bool, Predicate> { };

struct FTrueType : TConstBoolean<true> { };
struct TFalseType : TConstBoolean<false> { };

/*--------------------------------------------------------------------------*/

template <typename... Types> struct TAnd;
template <bool Lhs, typename... Rhs> struct TAndValue : TConstBoolean<TAnd<Rhs...>::Value> {};
template <typename... Rhs> struct TAndValue<false, Rhs...> : TConstBoolean<false> {};
template <typename Lhs, typename... Rhs> struct TAnd<Lhs, Rhs...> : TAndValue<Lhs::Value, Rhs...> {};
template <> struct TAnd<> : TConstBoolean<true> {};

/*--------------------------------------------------------------------------*/

template <typename... Types> struct TOr;
template <bool Lhs, typename... Rhs> struct TOrValue : TConstBoolean<TOr<Rhs...>::Value> {};
template <typename... Rhs> struct TOrValue<true, Rhs...> : TConstBoolean<true> {};
template <typename Lhs, typename... Rhs> struct TOr<Lhs, Rhs...> : TOrValue<Lhs::Value, Rhs...> {};
template <> struct TOr<> : TConstBoolean<false> {};

/*--------------------------------------------------------------------------*/

template <typename Type> struct TNot : TConstBoolean<!Type::Value> {};

/*--------------------------------------------------------------------------*/

template<bool, typename TrueType, typename FalseType> struct TConditional : Private::TType<FalseType> { };
template<typename TrueType, typename FalseType> struct TConditional<true, TrueType, FalseType> : Private::TType<TrueType> { };

/*--------------------------------------------------------------------------*/

template<typename T, typename U> struct TIsSame : TFalseType { };
template<typename T> struct TIsSame<T, T> : FTrueType { };

/*--------------------------------------------------------------------------*/

template<typename T> struct TRemoveConst : Private::TType<T> { };
template<typename T> struct TRemoveConst<const T> : TRemoveConst<T> { };

/*--------------------------------------------------------------------------*/

template<typename T> struct TRemoveVolatile : Private::TType<T> { };
template<typename T> struct TRemoveVolatile<volatile T> : TRemoveVolatile<T> { };

/*--------------------------------------------------------------------------*/

template<typename T> struct TRemoveCV : Private::TType<typename TRemoveConst<typename TRemoveVolatile<T>::Type>::Type> { };

/*--------------------------------------------------------------------------*/

namespace Private
{
	template<typename T> struct TIsIntegralHelper : TFalseType { };
	template<> struct TIsIntegralHelper<bool> : FTrueType { };
	template<> struct TIsIntegralHelper<char> : FTrueType { };
	template<> struct TIsIntegralHelper<char16_t> : FTrueType { };
	template<> struct TIsIntegralHelper<char32_t> : FTrueType { };
	template<> struct TIsIntegralHelper<wchar_t> : FTrueType { };
	template<> struct TIsIntegralHelper<short> : FTrueType { };
	template<> struct TIsIntegralHelper<int> : FTrueType { };
	template<> struct TIsIntegralHelper<long> : FTrueType { };
	template<> struct TIsIntegralHelper<long long> : FTrueType { };
}

template<typename T> struct TIsIntegral : Private::TIsIntegralHelper<typename TRemoveCV<T>::Type> { };

/*--------------------------------------------------------------------------*/

namespace Private
{
	template<typename T> struct TIsFloatingPointHelper : TFalseType { };
	template<> struct TIsFloatingPointHelper<float> : FTrueType { };	
	template<> struct TIsFloatingPointHelper<double> : FTrueType { };
	template<> struct TIsFloatingPointHelper<long double> : FTrueType { };
}

template<typename T> struct TIsFloatingPoint : Private::TIsFloatingPointHelper<typename TRemoveCV<T>::Type> { };

/*--------------------------------------------------------------------------*/

template<typename T> struct TIsArithmetic : TOr<TIsIntegral<T>, TIsFloatingPoint<T>> { };

/*--------------------------------------------------------------------------*/

namespace Private
{
	template<typename T> struct TIsPointerHelper : TFalseType { };
	template<typename T> struct TIsPointerHelper<T*> : FTrueType { };
}

template <typename T> struct TIsPointer : Private::TIsPointerHelper<typename TRemoveCV<T>::Type> { };

/*--------------------------------------------------------------------------*/

#define DECLARE_HAS_INSTANCE_FUNCTION(TraitName, FunctionName, Signature) \
WARNING(push) \
WARNING(disable:4067) \
template <typename U> \
struct TraitName \
{ \
private: \
   template<typename T, T> struct Helper; \
   template<typename T> static u8 Check(Helper<Signature, &T::FunctionName>*); \
   template<typename T> static u32 Check(...); \
public: \
   CONSTEXPR static bool Value = sizeof(Check<U>(0)) == sizeof(u8); \
}; \
WARNING(pop)

#define DECLARE_HAS_STATIC_FUNCTION(TraitName, FunctionName, Signature)  \
WARNING(push) \
WARNING(disable:4067) \
template <typename U> \
struct TraitName \
{ \
private: \
   template<typename T, T> struct Helper; \
   template<typename T> static u8 Check(Helper<Signature, T::FunctionName>*); \
   template<typename T> static u32 Check(...); \
public: \
   CONSTEXPR static bool Value = sizeof(Check<U>(0)) == sizeof(u8); \
}; \
WARNING(pop)