#pragma once

#include "HAL/Platform.h"
#include "Math/Math.h"
#include "Math/Vector2.h"

struct FRect
{
public:

	FORCEINLINE FRect(float x, float y, float width, float height) : X(x), Y(y), Width(width), Height(height) {}
	FORCEINLINE FRect(float x, float y, const FVector2& size) : X(x), Y(y), Width(size.X), Height(size.Y) {}
	FORCEINLINE FRect(const FVector2& position, float width, float height) : X(position.X), Y(position.Y), Width(width), Height(height) {}
	FORCEINLINE FRect(const FVector2& position, const FVector2& size) : X(position.X), Y(position.Y), Width(size.X), Height(size.Y) {}
	FORCEINLINE FRect(const FRect& r) : X(r.X), Y(r.Y), Width(r.Width), Height(r.Height) {}

public:

	FORCEINLINE FRect MinMax(const FVector2& min, const FVector2& max) const
	{
		return FRect(min.X, min.Y, max.X - min.X, max.Y - min.Y);
	}

	FORCEINLINE void Encapsulate(const FVector2& point)
	{
		if (point.X < X)
		{
			Width += X - point.X;
			X = point.X;
		}
		else if (point.X > X + Width)
		{
			Width = point.X - X;
		}

		if (point.Y < Y)
		{
			Height += Y - point.Y;
			Y = point.Y;
		}
		else if (point.Y > Y + Height)
		{
			Height = point.Y - Y;
		}
	}


	FORCEINLINE void SetXMax(float xMax)
	{
		Width = xMax - X;
	}

	FORCEINLINE void SetYMax(float yMax)
	{
		Height = yMax - Y;
	}

	FORCEINLINE void SetXMin(float xMin)
	{
		Width += X - xMin;
		X = xMin;
	}

	FORCEINLINE void SetYMin(float yMin)
	{
		Height += Y - yMin;
		Y = yMin;
	}

	FORCEINLINE bool Contains(const FVector2& point) const
	{
		return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
	}

	FORCEINLINE FVector2 GetMin() const
	{
		return FVector2(X, Y);
	}

	FORCEINLINE FVector2 GetMax() const
	{
		return FVector2(X + Width, Y + Height);
	}

	FORCEINLINE void SetMin(const FVector2& min)
	{
		X = min.X;
		Y = min.Y;
	}

	FORCEINLINE void SetMax(const FVector2& max)
	{
		Width = max.X - X;
		Height = max.Y - Y;
	}

	FORCEINLINE FVector2 GetCenter() const
	{
		return FVector2(X + Width * 0.5f, Y + Height * 0.5f);
	}

	FORCEINLINE bool Overlaps(const FRect& other) const
	{
		return X < other.X + other.Width && X + Width > other.X && Y < other.Y + other.Height && Y + Height > other.Y;
	}

	FORCEINLINE FVector2 NormalizedToPivot(const FVector2& normalized) const
	{
		return FVector2(X + Width * normalized.X, Y + Height * normalized.Y);
	}

public:

	FORCEINLINE bool operator==(const FRect& other) const
	{
		return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
	}

public:

	union
	{
		struct
		{
			float X, Y, Width, Height;
		};

		struct
		{
			FVector2 Position;
			FVector2 Size;
		};

		float Components[4];
	};
};