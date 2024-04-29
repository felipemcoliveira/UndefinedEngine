#pragma once

#include "HAL/Platform.h"
#include "Math/Math.h"

// TODO: move to a file
struct FColor32
{
public:

	static const FColor32 Clear;
	static const FColor32 Black;
	static const FColor32 White;
	static const FColor32 Red;
	static const FColor32 Green;
	static const FColor32 Blue;
	static const FColor32 Yellow;
	static const FColor32 Cyan;
	static const FColor32 Magenta;
	static const FColor32 Orange;
	static const FColor32 Purple;
	static const FColor32 Turquoise;
	static const FColor32 Silver;
	static const FColor32 Emerald;

public:

	FORCEINLINE FColor32() : UnsingnedIntValue(0) {}
	FORCEINLINE FColor32(u8 r, u8 g, u8 b, u8 a = 255) : R(r), G(g), B(b), A(a) {}
	FORCEINLINE FColor32(const FColor32& c) : UnsingnedIntValue(c.UnsingnedIntValue) {}

	FORCEINLINE FColor32(const FColor32& c, u8 a) : UnsingnedIntValue(c.UnsingnedIntValue)
	{
		A = a;
	}

	FORCEINLINE FColor32(const FColor32& c, float a) : UnsingnedIntValue(c.UnsingnedIntValue)
	{
		A = (u8)(a * 255.f);
	}

public: 

	FORCEINLINE const u8& operator[](const i32 index) const
	{
		CHECK(index >= 0 && index < 4);
		return Components[index];
	}

	FORCEINLINE u8& operator[](const i32 index)
	{
		CHECK(index >= 0 && index < 4);
		return Components[index];
	}

public:

	union
	{
		u32 UnsingnedIntValue;
		i32 SignedIntValue;

		struct
		{
			u8 R, G, B, A;
		};

		u8 Components[4];
	};
};

struct FVector
{
public:

	static const FVector Zero;
	static const FVector One;
	static const FVector Forward;
	static const FVector Back;
	static const FVector Up;
	static const FVector Down;
	static const FVector Left;
	static const FVector Right;

public:

	FORCEINLINE FVector() : X(0.f), Y(0.f), Z(0.f) {}
	FORCEINLINE FVector(float x, float y, float z) : X(x), Y(y), Z(z) {}
	FORCEINLINE FVector(float x, float y) : X(x), Y(y), Z(0.f) {}
	FORCEINLINE FVector(float x) : X(x), Y(x), Z(x) {}
	FORCEINLINE FVector(const FVector& v) : X(v.X), Y(v.Y), Z(v.Z) {}

public:

	FORCEINLINE float& operator[](int index) 
	{
		CHECK(index >= 0 && index < 3);
		return Components[index]; 
	}

	FORCEINLINE const float& operator[](int index) const 
	{
		CHECK(index >= 0 && index < 3);
		return Components[index]; 
	}

	FORCEINLINE FVector operator+(const FVector& v) const { return FVector(X + v.X, Y + v.Y, Z + v.Z); }
	FORCEINLINE FVector operator-(const FVector& v) const { return FVector(X - v.X, Y - v.Y, Z - v.Z); }

	FORCEINLINE FVector& operator+=(const FVector& Other) { X += Other.X; Y += Other.Y; Z += Other.Z; return *this; }
	FORCEINLINE FVector operator-=(const FVector& Other) { X -= Other.X; Y -= Other.Y; Z -= Other.Z; return *this; }

	template<typename T>
	FORCEINLINE FVector operator*(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

		return FVector(X * s, Y * s, Z * s);
	}

	template<typename T>
	FORCEINLINE FVector operator/(const T& s) const
	{
		static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");
		
		CHECK(s != 0);
		return FVector(X / s, Y / s, Z / s);
	}

public:

	FORCEINLINE static float Dot(const FVector& v1, const FVector& v2)
	{
		return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
	}

	FORCEINLINE static FVector Cross(const FVector& v1, const FVector& v2)
	{
		return FVector
		(
			v1.Y * v2.Z - v1.Z * v2.Y,
			v1.Z * v2.X - v1.X * v2.Z,
			v1.X * v2.Y - v1.Y * v2.X
		);
	}

	FORCEINLINE static float Distance(const FVector& v1, const FVector& v2)
	{
		return (v1 - v2).GetLength();
	}

	FORCEINLINE static FVector Normalize(const FVector& v)
	{
		const float length = v.GetLength();

		return FVector(v.X / length, v.Y / length, v.Z / length);
	}

	FORCEINLINE static FVector Lerp(const FVector& v1, const FVector& v2, float t)
	{
		return FVector
		(
			FMath::Lerp(v1.X, v2.X, t),
			FMath::Lerp(v1.Y, v2.Y, t),
			FMath::Lerp(v1.Z, v2.Z, t)
		);
	}

	FORCEINLINE static FVector Scale(const FVector& v1, const FVector& v2)
	{
		return FVector(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
	}

	FORCEINLINE static FVector AngleBetween(const FVector& v1, const FVector& v2)
	{
		return FVector
		(
			FMath::RadiansToDegrees(FMath::Acos(FVector::Dot(v1, v2))),
			FMath::RadiansToDegrees(FMath::Acos(FVector::Dot(v1, v2))),
			FMath::RadiansToDegrees(FMath::Acos(FVector::Dot(v1, v2)))
		);
	}

	FORCEINLINE static FVector Project(const FVector& v1, const FVector& v2)
	{
		CHECK(FVector::Dot(v2, v2) != 0.f);
		return v2 * (FVector::Dot(v1, v2) / FVector::Dot(v2, v2));
	}

	FORCEINLINE static FVector ProjectOnPlane(const FVector& v, const FVector& planeNormal)
	{
		return v - FVector::Project(v, planeNormal);
	}

	FORCEINLINE static FVector Reflect(const FVector& v1, const FVector& v2)
	{
		return v1 - v2 * (2.f * FVector::Dot(v1, v2));
	}

	FORCEINLINE static FVector MoveTowards(const FVector& v1, const FVector& v2, float maxDelta)
	{
		const FVector delta = v2 - v1;

		if (delta.GetSquaredLength() <= maxDelta * maxDelta)
		{
			return v2;
		}

		return v1 + delta.GetNormalized() * maxDelta;
	}

public:

	FORCEINLINE float GetLength() const { return FMath::Sqrt(X * X + Y * Y + Z * Z); }
	FORCEINLINE float GetSquaredLength() const { return X * X + Y * Y + Z * Z; }
	FORCEINLINE FVector GetNormalized() const { return FVector::Normalize(*this); }
	FORCEINLINE void Normalize() { *this = GetNormalized(); }

public:

	union
	{
		struct
		{
			float X, Y, Z;
		};

		float Components[3];
	};
};

template<typename T>
FORCEINLINE FVector operator*(const T& s, const FVector& v)
{
	static_assert(TIsArithmetic<T>::Value, "T must be a arithmetic type.");

	return v * s;
}