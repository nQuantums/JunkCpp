#pragma once
#ifndef __JUNK_TEMPLATEMETA_H__
#define __JUNK_TEMPLATEMETA_H__

#include "JunkDef.h"

_JUNK_BEGIN

// 昔のVCだと static なメソッドはインライン展開してくれないことがあったので強制インライン展開を指定

//! 何もしないオペレータ
struct TmNone {
};

//! 値代入オペレータ
struct TmSet {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v = v1;
	}
};

//! 値キャストして代入オペレータ
struct TmCastAndSet {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v = CT(v1);
	}
};

//! 四捨五入して代入オペレータ
struct TmNint {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v = Nint<CT>(v1);
	}
};

//! +符号オペレータ
struct TmPlus {
	template<class CT, class T1> static _FINLINE void Op(CT& v) {
	}
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v = v1;
	}
};

//! -符号オペレータ
struct TmMinus {
	template<class CT, class T1> static _FINLINE void Op(CT& v) {
		v = -v;
	}
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v = -v1;
	}
};

//! 加算オペレータ
struct TmAdd {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v += v1;
	}
	template<class CT, class T1, class T2> static _FINLINE void Op(CT& v, const T1& v1, const T2& v2) {
		v = v1 + v2;
	}
};

//! 減算オペレータ
struct TmSub {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v -= v1;
	}
	template<class CT, class T1, class T2> static _FINLINE void Op(CT& v, const T1& v1, const T2& v2) {
		v = v1 - v2;
	}
};

//! 乗算オペレータ
struct TmMul {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v *= v1;
	}
	template<class CT, class T1, class T2> static _FINLINE void Op(CT& v, const T1& v1, const T2& v2) {
		v = v1 * v2;
	}
};

//! 除算オペレータオペレータ
struct TmDiv {
	template<class CT, class T1> static _FINLINE void Op(CT& v, const T1& v1) {
		v /= v1;
	}
	template<class CT, class T1, class T2> static _FINLINE void Op(CT& v, const T1& v1, const T2& v2) {
		v = v1 / v2;
	}
};

//! 指定数の要素に指定オペレータを適用するテンプレートメタなクラス、コンパイラによっては for 文展開してくれるかわからないため確実に展開するために使用する
template<
	class OPCLS, //!< TmAdd などのオペレータクラス
	intptr_t N, //!< 要素数
	intptr_t LIM = 32 //!< 最大要素数制限、この値以上の要素数を処理する場合には for が使用される、コンパイラが展開してくれるかもしれない
> struct Order {

	//! 単項演算子を適用し元の値を書き換える
	template<class CT> static _FINLINE void UnaryAssign(CT* p) {
		if(N <= LIM) {
			OPCLS::Op(p[N-1]);
			Order<OPCLS, N-1, LIM>::UnaryAssign(p);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i]);
		}
	}

	//! ２項演算子を適用し元の値を書き換える
	template<class CT, class T1> static _FINLINE void BinaryAssign(CT* p, const T1* p1) {
		if (N <= LIM) {
			OPCLS::Op(p[N - 1], p1[N - 1]);
			Order<OPCLS, N - 1, LIM>::BinaryAssign(p, p1);
		} else {
			for (intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i], p1[i]);
		}
	}

	//! ２項演算子を適用し元の値を書き換える、対となる項はスカラー
	template<class CT, class T1> static _FINLINE void BinaryAssignScalar(CT* p, const T1& s) {
		if(N <= LIM) {
			OPCLS::Op(p[N-1], s);
			Order<OPCLS, N-1, LIM>::BinaryAssignScalar(p, s);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i], s);
		}
	}

	//! 指定インデックスの要素とそれ以外で設定する値を切り替える
	template<class CT, class T1> static _FINLINE void BinaryAssignByIndex(CT* p, intptr_t index, const T1& value, const T1& other) {
		if (N <= LIM) {
			OPCLS::Op(p[N - 1], N - 1 == index ? value : other);
			Order<OPCLS, N - 1, LIM>::BinaryAssignByIndex(p, index, value, other);
		} else {
			for (intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i], i == index ? value : other);
		}
	}

	//! ２項演算子を適用し戻り値用変数へ代入する
	template<class CT, class T1, class T2> static _FINLINE void Binary(CT* p, const T1* p1, const T2* p2) {
		if (N <= LIM) {
			OPCLS::Op(p[N - 1], p1[N - 1], p2[N - 1]);
			Order<OPCLS, N - 1, LIM>::Binary(p, p1, p2);
		} else {
			for (intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i], p1[i], p2[i]);
		}
	}

	//! ２項演算子を適用し戻り値用変数へ代入する、対となる項はスカラー
	template<class CT, class T1, class T2> static _FINLINE void BinaryScalar(CT* p, const T1* p1, const T2& s) {
		if(N <= LIM) {
			OPCLS::Op(p[N-1], p1[N-1], s);
			Order<OPCLS, N-1, LIM>::BinaryScalar(p, p1, s);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				OPCLS::Op(p[i], p1[i], s);
		}
	}

	//! ２項が同じ値なら true を返す、それ以外は false
	template<class T1, class T2> static _FINLINE bool Equal(const T1* p1, const T2* p2) {
		if(N <= LIM) {
			return p1[N-1] == p2[N-1] && Order<OPCLS, N-1, LIM>::Equal(p1, p2);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				if(p1[i] != p2[i])
					return false;
			return true;
		}
	}

	//! ２項が同じ値なら true を返す、それ以外は false、対となる項はスカラー
	template<class T1, class T2> static _FINLINE bool EqualScalar(const T1* p1, const T2& s) {
		if(N <= LIM) {
			return p1[N-1] == s && Order<OPCLS, N-1, LIM>::EqualScalar(p1, s);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				if(p1[i] != s)
					return false;
			return true;
		}
	}

	//! ２項が異なる値なら true を返す、それ以外は false
	template<class T1, class T2> static _FINLINE bool NotEqual(const T1* p1, const T2* p2) {
		if(N <= LIM) {
			return p1[N-1] != p2[N-1] || Order<OPCLS, N-1, LIM>::NotEqual(p1, p2);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				if(p1[i] != p2[i])
					return true;
			return false;
		}
	}

	//! ２項が異なる値なら true を返す、それ以外は false、対となる項はスカラー
	template<class T1, class T2> static _FINLINE bool NotEqualScalar(const T1* p1, const T2& s) {
		if(N <= LIM) {
			return p1[N-1] != s || Order<OPCLS, N-1, LIM>::NotEqualScalar(p1, s);
		} else {
			for(intptr_t i = N - 1; i != -1; --i)
				if(p1[i] != s)
					return true;
			return false;
		}
	}

	//! p1 の全要素の値が p2 より小さいなら true を返す、それ以外は false
	template<class T1, class T2> static _FINLINE bool LessThan(const T1* p1, const T2* p2) {
		if(N <= LIM) {
			if(p2[N-1] <= p1[N-1])
				return false;
			return Order<OPCLS, N-1, LIM>::LessThan(p1, p2);
		} else {
			for(intptr_t i = N - 1; i != -1; --i) {
				if(p2[i] <= p1[i])
					return false;
			}
			return true;
		}
	}

	//! p1 の全要素の値が p2 以下なら true を返す、それ以外は false
	template<class T1, class T2> static _FINLINE bool LessThanOrEqual(const T1* p1, const T2* p2) {
		if (N <= LIM) {
			if (p2[N - 1] < p1[N - 1])
				return false;
			return Order<OPCLS, N - 1, LIM>::LessThanOrEqual(p1, p2);
		} else {
			for (intptr_t i = N - 1; i != -1; --i) {
				if (p2[i] < p1[i])
					return false;
			}
			return true;
		}
	}

	//! p1 と p2 の内積を計算し v へ代入する
	template<class CT, class T1, class T2> static _FINLINE void Dot(CT& v, const T1* p1, const T2* p2) {
		if(N <= LIM) {
			v = p1[N-1] * p2[N-1];
			Order<OPCLS, N-1, LIM>::DotInternal(v, p1, p2);
		} else {
			v = p1[N-1] * p2[N-1];
			for(intptr_t i = N-2; i != -1; --i)
				v += p1[i] * p2[i];
		}
	}

	//! Dot の内部処理
	template<class CT, class T1, class T2> static _FINLINE void DotInternal(CT& v, const T1* p1, const T2* p2) {
		v += p1[N-1] * p2[N-1];
		Order<OPCLS, N-1, LIM>::DotInternal(v, p1, p2);
	}

	//! Cross 内部処理用構造体
	template<intptr_t N1, intptr_t dummy = 0> struct CrossStruct {
		template<class CT, class T1, class T2> static _FINLINE void Cross(CT* p, const T1* p1, const T2* p2) {
			enum { I1 = (N1-1+1) % N, I2 = (N1-1+2) % N };
			p[N1-1] = p1[I1] * p2[I2] - p1[I2] * p2[I1];
			CrossStruct<N1-1>::Cross(p, p1, p2);
		}
	};
	template<intptr_t dummy> struct CrossStruct<0, dummy> {
		template<class CT, class T1, class T2> static _FINLINE void Cross(CT* p, const T1* p1, const T2* p2) {}
	};

	//! p1 と p2 の外積を計算し p へ代入する
	template<class CT, class T1, class T2> static _FINLINE void Cross(CT* p, const T1* p1, const T2* p2) {
		if(N <= LIM) {
			CrossStruct<N>::Cross(p, p1, p2);
		} else {
			for(intptr_t i = 0; i < N; ++i) {
				intptr_t i1 = (i+1) % N, i2 = (i+2) % N;
				p[i] = p1[i1] * p2[i2] - p1[i2] * p2[i1];
			}
		}
	}

	//! p1 と p1 の内積を計算し v へ代入する
	template<class CT, class T1> static _FINLINE void Square(CT& v, const T1* p1) {
		if(N <= LIM) {
			v = p1[N-1] * p1[N-1];
			Order<OPCLS, N-1, LIM>::SquareInternal(v, p1);
		} else {
			v = p1[N-1] * p1[N-1];
			for(intptr_t i = N-2; i != -1; --i)
				v += p1[i] * p1[i];
		}
	}

	//! Square の内部処理
	template<class CT, class T1> static _FINLINE void SquareInternal(CT& v, const T1* p1) {
		v += p1[N-1] * p1[N-1];
		Order<OPCLS, N-1, LIM>::SquareInternal(v, p1);
	}
};

//! 指定数の要素に指定オペレータを適用するテンプレートメタなクラスの終端
template<class OPCLS, intptr_t LIM> struct Order<OPCLS, 0, LIM> {
	template<class CT> static _FINLINE void UnaryAssign(CT* p) {}
	template<class CT, class T1> static _FINLINE void BinaryAssign(CT* p, const T1* p1) {}
	template<class CT, class T1> static _FINLINE void BinaryAssignScalar(CT* p, const T1& s) {}
	template<class CT, class T1> static _FINLINE void BinaryAssignByIndex(CT* p, intptr_t index, const T1& value, const T1& other) {}
	template<class CT, class T1, class T2> static _FINLINE void Binary(CT* p, const T1* p1, const T2* p2) {}
	template<class CT, class T1, class T2> static _FINLINE void BinaryScalar(CT* p, const T1* p1, const T2& s) {}
	template<class T1, class T2> static _FINLINE bool Equal(const T1* p1, const T2* p2) {
		return true;
	}
	template<class T1, class T2> static _FINLINE bool EqualScalar(const T1* p1, const T2& v) {
		return true;
	}
	template<class T1, class T2> static _FINLINE bool NotEqual(const T1* p1, const T2* p2) {
		return false;
	}
	template<class T1, class T2> static _FINLINE bool NotEqualScalar(const T1* p1, const T2& v) {
		return false;
	}
	template<class T1, class T2> static _FINLINE bool LessThan(const T1* p1, const T2* p2) {
		return true;
	}
	template<class T1, class T2> static _FINLINE bool LessThanOrEqual(const T1* p1, const T2* p2) {
		return true;
	}
	template<class CT, class T1, class T2> static _FINLINE void DotInternal(CT& v, const T1* p1, const T2* p2) {}
	template<class CT, class T1, class T2> static _FINLINE void Cross(CT* p, const T1* p1, const T2* p2) {}
	template<class CT, class T1> static _FINLINE void SquareInternal(CT& v, const T1* p1) {}
};

_JUNK_END

#endif
