#pragma once 

#include "HAL/Platform.h"
#include "Math/Vector4.h"
#include "Math/Vector.h"
#include "Math/Quat.h"
#include "Math/Math.h"

struct FMatrix
{
public:

	static const FMatrix Identity;

public:

	FORCEINLINE FMatrix()
	{
		Rows[0] = FVector4::Zero;
		Rows[1] = FVector4::Zero;
		Rows[2] = FVector4::Zero;
		Rows[3] = FVector4::Zero;
	}

	FORCEINLINE FMatrix(const FMatrix& Other)
	{
		Rows[0] = Other.Rows[0];
		Rows[1] = Other.Rows[1];
		Rows[2] = Other.Rows[2];
		Rows[3] = Other.Rows[3];
	}

	FORCEINLINE FMatrix(FVector4 r0, FVector4 r1, FVector4 r2, FVector4 r3)
	{
		Rows[0] = r0;
		Rows[1] = r1;
		Rows[2] = r2;
		Rows[3] = r3;
	}

public:

	FORCEINLINE FMatrix operator*(const FMatrix& other) const
	{
		FMatrix result;

		for (i32 Row = 0; Row < 4; ++Row)
		{
			for (i32 Col = 0; Col < 4; ++Col)
			{
				result.M[Row][Col] = Rows[Row][0] * other.M[0][Col]
					+ Rows[Row][1] * other.M[1][Col]
					+ Rows[Row][2] * other.M[2][Col]
					+ Rows[Row][3] * other.M[3][Col];
			}
		}

		return result;
	}

	FORCEINLINE FVector operator*(const FVector& other) const
	{
		FVector result;

		result.X = Rows[0][0] * other.X + Rows[0][1] * other.Y + Rows[0][2] * other.Z + Rows[0][3];
		result.Y = Rows[1][0] * other.X + Rows[1][1] * other.Y + Rows[1][2] * other.Z + Rows[1][3];
		result.Z = Rows[2][0] * other.X + Rows[2][1] * other.Y + Rows[2][2] * other.Z + Rows[2][3];

		return result;
	}

public:

	FORCEINLINE FMatrix Transpose() const
	{
		FMatrix result;

		for (i32 Row = 0; Row < 4; ++Row)
		{
			for (i32 Col = 0; Col < 4; ++Col)
			{
				result.M[Row][Col] = M[Col][Row];
			}
		}

		return result;
	}

	FORCEINLINE float GetDeterminant() const
	{
		return M[0][0] *
			(
				M[1][1] * (M[2][2] * M[3][3] - M[2][3] * M[3][2]) -
				M[1][2] * (M[2][1] * M[3][3] - M[2][3] * M[3][1]) +
				M[1][3] * (M[2][1] * M[3][2] - M[2][2] * M[3][1])
				)
			- M[0][1] *
			(
				M[1][0] * (M[2][2] * M[3][3] - M[2][3] * M[3][2]) -
				M[1][2] * (M[2][0] * M[3][3] - M[2][3] * M[3][0]) +
				M[1][3] * (M[2][0] * M[3][2] - M[2][2] * M[3][0])
				)
			+ M[0][2] *
			(
				M[1][0] * (M[2][1] * M[3][3] - M[2][3] * M[3][1]) -
				M[1][1] * (M[2][0] * M[3][3] - M[2][3] * M[3][0]) +
				M[1][3] * (M[2][0] * M[3][1] - M[2][1] * M[3][0]))
			- M[0][3] *
			(
				M[1][0] * (M[2][1] * M[3][2] - M[2][2] * M[3][1]) -
				M[1][1] * (M[2][0] * M[3][2] - M[2][2] * M[3][0]) +
				M[1][2] * (M[2][0] * M[3][1] - M[2][1] * M[3][0])
				);
	}

	FORCEINLINE FMatrix Inverse() const
	{
		FMatrix inverse;
		float det = GetDeterminant();

		if (det == 0)
		{
			return FMatrix::Identity;
		}

		float invDet = 1.0f / det;

		for (int i = 0; i < 4; ++i)
		{
			for (int j = 0; j < 4; ++j)
			{
				inverse.M[i][j] /= invDet;
			}
		}

		return inverse;
	}

public:

	FORCEINLINE static FMatrix Translation(const FVector& translation)
	{
		return FMatrix
		(
			FVector4(1.0f, 0.0f, 0.0f, translation.X),
			FVector4(0.0f, 1.0f, 0.0f, translation.Y),
			FVector4(0.0f, 0.0f, 1.0f, translation.Z),
			FVector4(0.0f, 0.0f, 0.0f, 1.0f)
		);
	}


	FORCEINLINE static FMatrix Scale(const FVector& scale)
	{
		return FMatrix
		(
			FVector4(scale.X, 0.0f, 0.0f, 0.0f),
			FVector4(0.0f, scale.Y, 0.0f, 0.0f),
			FVector4(0.0f, 0.0f, scale.Z, 0.0f),
			FVector4(0.0f, 0.0f, 0.0f, 1.0f)
		);
	}

	FORCEINLINE static FMatrix Rotation(FQuat rotation)
	{
		float x2 = rotation.X + rotation.X;
		float y2 = rotation.Y + rotation.Y;
		float z2 = rotation.Z + rotation.Z;

		float xx2 = rotation.X * x2;
		float yy2 = rotation.Y * y2;
		float zz2 = rotation.Z * z2;

		float yz2 = rotation.Y * z2;
		float wx2 = rotation.W * x2;

		float xy2 = rotation.X * y2;
		float wz2 = rotation.W * z2;

		float xz2 = rotation.X * z2;
		float wy2 = rotation.W * y2;

		return FMatrix
		(
			FVector4(1.0f - yy2 - zz2, xy2 + wz2, xz2 - wy2, 0.0f),
			FVector4(xy2 - wz2, 1.0f - xx2 - zz2, yz2 + wx2, 0.0f),
			FVector4(xz2 + wy2, yz2 - wx2, 1.0f - xx2 - yy2, 0.0f),
			FVector4(0.0f, 0.0f, 0.0f, 1.0f)
		);
	}

	FORCEINLINE static FMatrix TRS(const FVector& translation, const FQuat& rotation, const FVector& scale)
	{
		return Translation(translation) * Rotation(rotation) * Scale(scale);
	}

	FORCEINLINE static FMatrix TranslationRotation(const FVector& translation, const FQuat& rotation)
	{
		return Translation(translation) * Rotation(rotation);
	}

public:

	union
	{
		float M[4][4];
		float Elements[16];
		FVector4 Rows[4];
	};
};