#pragma once

#include "HAL/Platform.h"
#include "Math/Math.h"

struct FVector4
{
public:

	static const FVector4 Zero;
	static const FVector4 One;

public:

	FORCEINLINE FVector4() : X(0), Y(0), Z(0), W(0) {}
	FORCEINLINE FVector4(const float InX, const float InY, const float InZ, const float InW) : X(InX), Y(InY), Z(InZ), W(InW) {}
	FORCEINLINE FVector4(const FVector4& Other) : X(Other.X), Y(Other.Y), Z(Other.Z), W(Other.W) {}

public:

	FORCEINLINE float& operator[](int index) 
	{
		CHECK(index >= 0 && index < 4);
		return Components[index]; 
	}

	FORCEINLINE const float& operator[](int index) const 
	{
		CHECK(index >= 0 && index < 4);
		return Components[index]; 
	}

	FORCEINLINE FVector4 operator+(const FVector4& Other) const { return FVector4(X + Other.X, Y + Other.Y, Z + Other.Z, W + Other.W); }
	FORCEINLINE FVector4 operator-(const FVector4& Other) const { return FVector4(X - Other.X, Y - Other.Y, Z - Other.Z, W - Other.W); }

	FORCEINLINE FVector4& operator+=(const FVector4& Other) { X += Other.X; Y += Other.Y; Z += Other.Z; W += Other.W; return *this; }
	FORCEINLINE FVector4& operator-=(const FVector4& Other) { X -= Other.X; Y -= Other.Y; Z -= Other.Z; W -= Other.W; return *this; }

	template<typename T>
	FORCEINLINE FVector4 operator*(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

		return FVector4(X * s, Y * s, Z * s, W * s);
	}

	template<typename T>
	FORCEINLINE FVector4 operator/(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");
		
		CHECK(s != 0);
		return FVector4(X / s, Y / s, Z / s, W * s);
	}

public:

	union
	{
		struct
		{
			float X, Y, Z, W;
		};

		float Components[4];
	};
};
