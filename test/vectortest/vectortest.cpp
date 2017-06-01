// vectortest.cpp : コンソール アプリケーションのエントリ ポイントを定義します。
//

#include "stdafx.h"
#include "../../src/Vector.h"
#include <iostream>
#include <cassert>

using namespace jk;

int main() {
	// 代入関係
	Vector3d v1 = { 1.1, 2.2, 3.3, 4.4 };
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
	Vector3d v2 = { 2, 3, 4 };
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



	std::cout << v1.ToJsonString() << std::endl;

	return 0;
}

