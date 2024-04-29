#pragma once

#include "HAL/Platform.h"
#include "Math/Math.h"

struct FVector2
{
public:

	static const FVector2 Zero;
	static const FVector2 One;
	static const FVector2 Left;
	static const FVector2 Right;
	static const FVector2 Up;
	static const FVector2 Down;

public:

	FORCEINLINE FVector2() : X(0.0f), Y(0.0f) {}
	FORCEINLINE FVector2(float x, float y) : X(x), Y(y) {}
	FORCEINLINE FVector2(float x) : X(x), Y(x) {}
	FORCEINLINE FVector2(const FVector2& v) : X(v.X), Y(v.Y) {}

public:

	FORCEINLINE float& operator[](int index) 
	{
		CHECK(index >= 0 && index < 2);
		return Components[index]; 
	}
	FORCEINLINE const float& operator[](int index) const 
	{
		CHECK(index >= 0 && index < 2);
		return Components[index]; 
	}

	FORCEINLINE FVector2 operator+(const FVector2& v) const { return FVector2(X + v.X, Y + v.Y); }
	FORCEINLINE FVector2 operator-(const FVector2& v) const { return FVector2(X - v.X, Y - v.Y); }

	FORCEINLINE FVector2& operator+=(const FVector2& Other) { X += Other.X; Y += Other.Y; return *this; }
	FORCEINLINE FVector2 operator-=(const FVector2& Other) { X -= Other.X; Y -= Other.Y; return *this; }

	template<typename T>
	FORCEINLINE FVector2 operator*(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

		return FVector2(X * s, Y * s);
	}

	template<typename T>
	FORCEINLINE FVector2 operator/(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

		CHECK(s != 0);
		return FVector2(X / s, Y / s);
	}

public:

	FORCEINLINE static float Dot(const FVector2& v1, const FVector2& v2)
	{
		return v1.X * v2.X + v1.Y * v2.Y;
	}

	FORCEINLINE static float Distance(const FVector2& v1, const FVector2& v2)
	{
		return (v1 - v2).GetLength();
	}

	FORCEINLINE static FVector2 Normalize(const FVector2& v)
	{
		const float length = v.GetLength();

		CHECK(length != 0.0f);
		return FVector2(v.X / length, v.Y / length);
	}

public:

	FORCEINLINE float GetLength() const { return FMath::Sqrt(X * X + Y * Y); }
	FORCEINLINE float GetSquaredLength() const { return X * X + Y * Y; }
	FORCEINLINE FVector2 GetNormalized() const { return FVector2::Normalize(*this); }
	FORCEINLINE void Normalize() { *this = GetNormalized(); }

public:

	union
	{
		struct
		{
			float X, Y;
		};

		float Components[2];
	};
};

template<typename T>
FORCEINLINE FVector2 operator*(const T& s, const FVector2& v)
{
	static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

	return v * s;
}