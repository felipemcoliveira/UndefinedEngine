#pragma once

#include "HAL/Platform.h"

#include <cmath> 


struct FMath
{
private:

	union IntFloatUnion;

public:

	CONSTEXPR static float Pi = 3.14159265358979323f;
	CONSTEXPR static float SmallNumber = 1.e-8f;
	CONSTEXPR static float KindaSmallNumber = 1.e-4f;
	CONSTEXPR static float EulersNumber = 2.71828182845904523536f;
	CONSTEXPR static float InversedPi = 0.31830988618f;
	CONSTEXPR static float HalfPi = 1.57079632679f;
	CONSTEXPR static float Delta = 0.00001f;
	CONSTEXPR static float BigNumber = 3.4e+38f;

public:

	CONSTEXPR static float Rad2Deg = (360.0f / (Pi * 2));
	CONSTEXPR static float Deg2Rad = ((Pi * 2) / 360.0f);

public:

	FORCEINLINE static float Sqrt(float value)
	{
		IntFloatUnion Union(value);
		Union.Int = (1 << 29) + (Union.Int >> 1) - (1 << 22);
		Union.Float = Union.Float + value / Union.Float;
		return 0.25f * Union.Float + value / Union.Float;
	}

	FORCEINLINE static float InvSqrt(const float value)
	{
		IntFloatUnion Union(value);
		Union.Int = 0x5F3759DF - (Union.Int >> 1);
		return Union.Float * (1.5f - (0.5f * value * Union.Float * Union.Float));
	}

public:

	template<class T>
	FORCEINLINE static T Abs(const T value)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return value < 0 ? -value : value;
	}

	template<class T>
	FORCEINLINE static T Max(const T lhs, const T rhs)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return lhs > rhs ? lhs : rhs;
	}

	template<class T>
	FORCEINLINE static T Min(const T lhs, const T rhs)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return lhs < rhs ? lhs : rhs;
	}

	template<class T>
	FORCEINLINE static T Square(const T value)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return value * value;
	}

	template<class T>
	FORCEINLINE static T Clamp(const T value, const T Min, const T Max)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return value > Max ? Max : value < Min ? Min : value;
	}

	template<class T>
	FORCEINLINE static T Clamp01(const T value)
	{
		static_assert(TIsArithmetic<T>::Value, "T must be an arithmetic type.");
		return value > 1 ? 1 : value < 0 ? 0 : value;
	}

	FORCEINLINE static float Lerp(const float lhs, const float rhs, const float t)
	{
		return LerpUnclamped(lhs, rhs, Clamp01(t));
	}

	FORCEINLINE static float LerpUnclamped(const float lhs, const float rhs, const float t)
	{
		return lhs + (rhs - lhs) * t;
	}

	FORCEINLINE static float InverseLerp(const float lhs, const float rhs, const float value)
	{
		CHECK((rhs - lhs) != 0.0f)
			return Clamp01((value - lhs) / (rhs - lhs));
	}

	FORCEINLINE static float RadiansToDegrees(const float Radians)
	{
		return Radians * Rad2Deg;
	}

public:

	FORCEINLINE static float Sin(float Radians) { return sin(Radians); }
	FORCEINLINE static float Cos(const float Randians) { return cos(Randians); }
	FORCEINLINE static float Tan(const float Randians) { return tan(Randians); }
	FORCEINLINE static float Asin(const float Value) { return asin(Value); }
	FORCEINLINE static float Acos(const float Value) { return acos(Value); }
	FORCEINLINE static float Atan(const float Value) { return atan(Value); }
	FORCEINLINE static float Atan2(const float Y, const float X) { return atan2(Y, X); }

private:

	union IntFloatUnion
	{
		i32 Int;
		float Float;

		FORCEINLINE IntFloatUnion(const i32 InInt) { Int = InInt; }
		FORCEINLINE IntFloatUnion(const float InFloat) { Float = InFloat; }
	};
};
