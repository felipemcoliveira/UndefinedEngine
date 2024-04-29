#pragma once

#include "HAL/Platform.h"
#include "Math/Math.h"
#include "Math/Vector.h"

struct FQuat
{
public:

	static const FQuat Identity;

public:

	FORCEINLINE FQuat() : X(0.f), Y(0.f), Z(0.f), W(1.f) {}
	FORCEINLINE FQuat(float x, float y, float z, float w) : X(x), Y(y), Z(z), W(w) {}
	FORCEINLINE FQuat(const FVector& axis, float angleRad)
	{
		const float halfAngle = angleRad * 0.5f;
		const float s = FMath::Sin(halfAngle);
		const FVector normalizedAxis = axis.GetNormalized();
		X = normalizedAxis.X * s;
		Y = normalizedAxis.Y * s;
		Z = normalizedAxis.Z * s;
		W = FMath::Cos(halfAngle);
	}

public:

	FORCEINLINE FQuat operator*(const FQuat& q) const
	{
		return FQuat
		(
			W * q.X + X * q.W + Y * q.Z - Z * q.Y,
			W * q.Y - X * q.Z + Y * q.W + Z * q.X,
			W * q.Z + X * q.Y - Y * q.X + Z * q.W,
			W * q.W - X * q.X - Y * q.Y - Z * q.Z
		);
	}

	FORCEINLINE FVector operator*(const FVector& v) const
	{
		const FVector qVec(X, Y, Z);
		const FVector uv = FVector::Cross(qVec, v);
		const FVector uuv = FVector::Cross(qVec, uv);
		return v + ((uv * W) + uuv) * 2.0f;
	}

public:

	FORCEINLINE static FQuat Inverse(const FQuat& q)
	{
		const float norm = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
		return FQuat(-q.X / norm, -q.Y / norm, -q.Z / norm, q.W / norm);
	}

	FORCEINLINE static FQuat Slerp(const FQuat& a, const FQuat& b, float t)
	{
		return SlerpUnclamped(a, b, FMath::Clamp(t, 0.f, 1.f));
	}

	FORCEINLINE static FQuat SlerpUnclamped(const FQuat& a, const FQuat& b, float t)
	{
		const float cosHalfTheta = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

		if (FMath::Abs(cosHalfTheta) >= 1.f)
		{
			return a;
		}

		const float halfTheta = FMath::Acos(cosHalfTheta);
		const float sinHalfTheta = FMath::Sqrt(1.f - cosHalfTheta * cosHalfTheta);

		if (FMath::Abs(sinHalfTheta) < 0.001f)
		{
			return FQuat
			(
				a.X * 0.5f + b.X * 0.5f,
				a.Y * 0.5f + b.Y * 0.5f,
				a.Z * 0.5f + b.Z * 0.5f,
				a.W * 0.5f + b.W * 0.5f
			);
		}

		const float ratioA = FMath::Sin((1.f - t) * halfTheta) / sinHalfTheta;
		const float ratioB = FMath::Sin(t * halfTheta) / sinHalfTheta;

		return FQuat
		(
			a.X * ratioA + b.X * ratioB,
			a.Y * ratioA + b.Y * ratioB,
			a.Z * ratioA + b.Z * ratioB,
			a.W * ratioA + b.W * ratioB
		);
	}

	FORCEINLINE static FQuat FromEulerAngles(const FVector& euler)
	{
		const float halfToRad = 0.5f * FMath::Pi / 180.f;
		const float pitch = euler.X * halfToRad;
		const float yaw = euler.Y * halfToRad;
		const float roll = euler.Z * halfToRad;

		const float sp = FMath::Sin(pitch);
		const float cp = FMath::Cos(pitch);
		const float sy = FMath::Sin(yaw);
		const float cy = FMath::Cos(yaw);
		const float sr = FMath::Sin(roll);
		const float cr = FMath::Cos(roll);

		const float cpcy = cp * cy;
		const float spcy = sp * cy;
		const float cpsy = cp * sy;
		const float spsy = sp * sy;

		return FQuat
		(
			(sr * cpcy - cr * spsy),
			(cr * spcy + sr * cpsy),
			(cr * cpsy - sr * spcy),
			(cr * cpcy + sr * spsy)
		);
	}

public:

	FORCEINLINE FVector GetEulerAngles()
	{
		const float sqw = W * W;
		const float sqx = X * X;
		const float sqy = Y * Y;
		const float sqz = Z * Z;

		const float unit = sqx + sqy + sqz + sqw;
		const float test = X * Y + Z * W;

		FVector euler;

		if (test > 0.499f * unit)
		{
			euler.X = 2.f * FMath::Atan2(X, W);
			euler.Y = FMath::Pi * 0.5f;
			euler.Z = 0.f;
		}
		else if (test < -0.499f * unit)
		{
			euler.X = -2.f * FMath::Atan2(X, W);
			euler.Y = -FMath::Pi * 0.5f;
			euler.Z = 0.f;
		}
		else
		{
			euler.X = FMath::Atan2(2.f * Y * W - 2.f * X * Z, sqx - sqy - sqz + sqw);
			euler.Y = FMath::Asin(2.f * test / unit);
			euler.Z = FMath::Atan2(2.f * X * W - 2.f * Y * Z, -sqx + sqy - sqz + sqw);
		}

		return euler;
	}

	FORCEINLINE FQuat GetNormalized() const
	{
		const float mag = FMath::Sqrt(X * X + Y * Y + Z * Z + W * W);
		return FQuat(X / mag, Y / mag, Z / mag, W / mag);
	}

	FORCEINLINE void Normalize()
	{
		*this = GetNormalized();
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