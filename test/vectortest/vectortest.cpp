// vectortest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include "../../src/Vector.h"
#include <iostream>
#include <cassert>

using namespace jk;

Vector3d v1 = { 1.1, 2.2, 3.3, 4.4 };
Vector3d v2 = { 2, 3, 4 };

int main() {
	// 代入関係
	assert(v1[0] == 1.1 && v1[1] == 2.2 && v1[2] == 3.3);

	double a[] = { 0.1, 0.2, 0.3 };
	v1.Set(a);
	assert(v1[0] == 0.1 && v1[1] == 0.2 && v1[2] == 0.3);

	v1.Set({ 1, 2, 3 });
	assert(v1[0] == 1 && v1[1] == 2 && v1[2] == 3);

	v1 = { 12, 23, 34 };
	assert(v1[0] == 12 && v1[1] == 23 && v1[2] == 34);

	Vector3f vf = { 3.5, 2.5, 1.5 };
	v1 = vf;
	assert(v1[0] == 3.5 && v1[1] == 2.5 && v1[2] == 1.5);

	Vector3i vi = { 5, 6, 7 };
	v1 = vi;
	assert(v1[0] == 5 && v1[1] == 6 && v1[2] == 7);

	// 代入四則演算
	v1 = { 5, 6, 7 };
	v1 += v2;
	assert(v1[0] == 7 && v1[1] == 9 && v1[2] == 11);

	v1 = { 5, 6, 7 };
	v1 += 1;
	assert(v1[0] == 6 && v1[1] == 7 && v1[2] == 8);

	v1 = { 10, 10, 10 };
	v1 -= v2;
	assert(v1[0] == 8 && v1[1] == 7 && v1[2] == 6);

	v1 = { 1, 2, 3 };
	v1 -= 2;
	assert(v1[0] == -1 && v1[1] == 0 && v1[2] == 1);

	v1 = { 5, 6, 7 };
	v1 *= v2;
	assert(v1[0] == 10 && v1[1] == 18 && v1[2] == 28);

	v1 = { 5, 6, 7 };
	v1 *= 3;
	assert(v1[0] == 15 && v1[1] == 18 && v1[2] == 21);

	v1 = { 2, 4, 6 };
	v1 /= 2;
	assert(v1[0] == 1 && v1[1] == 2 && v1[2] == 3);

	v1 = { 2, 4, 6 };
	v2 = { 1, 2, 2 };
	v1 /= v2;
	assert(v1[0] == 2 && v1[1] == 2 && v1[2] == 3);

	// 四則演算
	v1 = Vector3d(11, 22, 33) + Vector3d(1, 2, 3);
	assert(v1[0] == 12 && v1[1] == 24 && v1[2] == 36);

	v1 = Vector3d(11, 22, 33) - Vector3d(1, 2, 3);
	assert(v1[0] == 10 && v1[1] == 20 && v1[2] == 30);

	v1 = Vector3d(11, 22, 33) * Vector3d(2, 3, 4);
	assert(v1[0] == 22 && v1[1] == 66 && v1[2] == 132);

	v1 = Vector3d(11, 22, 33) * 3;
	assert(v1[0] == 33 && v1[1] == 66 && v1[2] == 99);

	v1 = Vector3d(12, 24, 33) / Vector3d(2, 3, 3);
	assert(v1[0] == 6 && v1[1] == 8 && v1[2] == 11);

	v1 = Vector3d(12, 24, 34) / 2;
	assert(v1[0] == 6 && v1[1] == 12 && v1[2] == 17);

	// 軸ベクトル生成
	v1 = Vector3d::Axis(0);
	assert(v1[0] == 1 && v1[1] == 0 && v1[2] == 0);
	v1 = Vector3d::Axis(1);
	assert(v1[0] == 0 && v1[1] == 1 && v1[2] == 0);
	v1 = Vector3d::Axis(2);
	assert(v1[0] == 0 && v1[1] == 0 && v1[2] == 1);

	// ベクトル長
	v1 = { 2, 3, 4 };
	assert(v1.LengthSquare() == 2 * 2 + 3 * 3 + 4 * 4);
	auto len = std::sqrt(2 * 2 + 3 * 3 + 4 * 4);
	assert(v1.Length() == len);
	v1.NormalizeSelf();
	assert(v1[0] == 2 / len && v1[1] == 3 / len && v1[2] == 4 / len);

	v2 = { 0, 2, 0 };
	v1 = v2.Normalize();
	assert(v1[0] == 0 && v1[1] == 1 && v1[2] == 0);

	v1 = { 2, 0, 0 };
	v1.RelengthSelf(4);
	assert(v1[0] == 4 && v1[1] == 0 && v1[2] == 0);

	v2 = { 0, 2, 0 };
	v1 = v2.Relength(4);
	assert(v1[0] == 0 && v1[1] == 4 && v1[2] == 0);

	// 水平演算
	v1 = { 3, 2, 1 };
	assert(v1.Max() == 3);
	v1 = { 3, 2, 1 };
	assert(v1.Min() == 1);
	v1 = { 3, 2, 1 };
	assert(v1.ArgMax() == 0);
	v1 = { 3, 2, 1 };
	assert(v1.ArgMin() == 2);
	v1 = { 11, 22, 33 };
	assert(v1.Sum() == 11 + 22 + 33);
	assert(v1.Product() == 11 * 22 * 33);

	// 比較
	v1 = { 0, 0, 0 };
	assert(v1.IsZero());
	v1 = { 0, 1, 0 };
	assert(!v1.IsZero());
	v1 = Vector3d::Zero();
	assert(v1.IsZero());
	v1 = { 33, 44, 55 };
	v2 = { 33, 44, 55 };
	assert(v1 == v2);
	assert(!(v1 != v2));
	v1 = { 33, 44, 55 };
	v2 = { 33, 41, 55 };
	assert(!(v1 == v2));
	assert(v1 != v2);
	v1 = { 33, 44, 55 };
	v2 = { 34, 45, 56 };
	assert(v1 < v2);
	assert(v1 <= v2);
	assert(!(v2 < v1));
	assert(!(v2 <= v1));
	v1 = { 33, 44, 55 };
	v2 = { 33, 44, 55 };
	assert(!(v1 < v2));
	assert(v1 <= v2);
	assert(!(v2 < v1));
	assert(v2 <= v1);

	// 要素毎比較
	v1 = { 11, 22, 33 };
	v2 = { 11, 322, 13 };
	v1.ElementWiseMaxSelf(v2);
	assert(v1[0] == 11 && v1[1] == 322 && v1[2] == 33);

	v1 = { 11, 22, 33 };
	v2 = { 11, 322, 13 };
	v1 = v1.ElementWiseMax(v2);
	assert(v1[0] == 11 && v1[1] == 322 && v1[2] == 33);

	v1 = { 11, 22, 33 };
	v2 = { 11, 322, 13 };
	v1.ElementWiseMinSelf(v2);
	assert(v1[0] == 11 && v1[1] == 22 && v1[2] == 13);

	v1 = { 11, 22, 33 };
	v2 = { 11, 322, 13 };
	v1 = v1.ElementWiseMin(v2);
	assert(v1[0] == 11 && v1[1] == 22 && v1[2] == 13);

	// ベクトル特有の演算
	assert(Vector3d(2, 3, 4).Dot(Vector3d(2, 3, 4)) == 2 * 2 + 3 * 3 + 4 * 4);

	assert(Vector3d::Axis(0).Cross(Vector3d::Axis(1)) == Vector3d::Axis(2));
	assert(Vector3d::Axis(1).Cross(Vector3d::Axis(2)) == Vector3d::Axis(0));
	assert(Vector3d::Axis(2).Cross(Vector3d::Axis(0)) == Vector3d::Axis(1));

	v1 = { 1, 1, 1 };
	v2 = { -1, -1, -1 };
	v2.NormalizeSelf();
	v1.ReflectSelf(v2);
	assert(std::abs(v1[0] - -1) <= Epsilon<float>() && std::abs(v1[1] - -1) <= Epsilon<float>() && std::abs(v1[2] - -1) <= Epsilon<float>());
	v1 = { 1, 1, 1 };
	v2 = { -1, -1, -1 };
	v2.NormalizeSelf();
	v1 = v1.Reflect(v2);
	assert(std::abs(v1[0] - -1) <= Epsilon<float>() && std::abs(v1[1] - -1) <= Epsilon<float>() && std::abs(v1[2] - -1) <= Epsilon<float>());

	return 0;
}

