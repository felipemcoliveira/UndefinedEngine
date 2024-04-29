#pragma once 

#include "Math/Matrix.h"
#include "Math/Vector4.h"

const FMatrix FMatrix::Identity = FMatrix
(
	FVector4(1.0f, 0.0f, 0.0f, 0.0f),
	FVector4(0.0f, 1.0f, 0.0f, 0.0f),
	FVector4(0.0f, 0.0f, 1.0f, 0.0f),
	FVector4(0.0f, 0.0f, 0.0f, 1.0f)
);