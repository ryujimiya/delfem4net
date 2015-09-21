/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

Copyright (C) 2009  Nobuyuki Umetani    n.umetani@gmail.com

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

/*! @file
@brief interface of block vector class (MatVec::CVector_Blk)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_VECTOR_BLK_H)
#define DELFEM4NET_VECTOR_BLK_H

#include <assert.h>
#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"

#include "delfem/matvec/vector_blk.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{

//ref class CMat_BlkCrs;
//ref class CMatDia_BlkCrs;

/*! 
@brief real value block vector class
@ingroup MatVec
*/
public ref class CVector_Blk  
{
public:
    static double operator*(CVector_Blk^ lhs, CVector_Blk^ rhs);

    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CVector_Blk * CreateNativeInstance(const MatVec::CVector_Blk& rhs);
public:
    CVector_Blk();
    CVector_Blk(bool isCreateInstance);
    CVector_Blk(const CVector_Blk% rhs);
    CVector_Blk(MatVec::CVector_Blk *self);

    /*!
    @brief コンストラクタ
    @param[in] iblkveclen ブロックの数
    @param[in] iblklen １つのブロックのサイズ
    */
    CVector_Blk(unsigned int nblk, unsigned int len);
    CVector_Blk(unsigned int nblk, List<unsigned int>^ aLen);
    virtual ~CVector_Blk();
    !CVector_Blk();
    property MatVec::CVector_Blk * Self
    {
       MatVec::CVector_Blk * get();
    }

    bool Initialize(unsigned int nblk, unsigned int len );
    bool Initialize(unsigned int nblk, List<unsigned int>^ alen);

    // @{
    CVector_Blk^ operator*=(double d0);    //!< Scaler Product
    CVector_Blk^ operator=(CVector_Blk^ rhs);    //!< Substitue Vector
    CVector_Blk^ operator+=(CVector_Blk^ rhs); //!< Add 
    // @}

    ////////////////////////////////
    // Menber function

    /*
    @brief ベクトルのスカラー倍の足し算
    {this} += alhpa*{rhs}
    */
    CVector_Blk^ AXPY(double alpha, CVector_Blk^ rhs);
    void SetVectorZero();   //!< Set 0 to Value
    double GetSquaredVectorNorm();    //!< ベクトルの２乗ノルムの２乗を計算する
    unsigned int NBlk();
    int Len();
    //! Size of One Block
    unsigned int Len(unsigned int iblk);
  
    /*!
    @brief 値を取得
    @param [in] iblk ブロックのインデックス
    @param [in] idofblk ブロックの中の自由度番号
    @retval 値
    */
    double GetValue(unsigned int iblk, unsigned int idofblk);

    /*!
    @brief 値をセット
    @param [in] iblk ブロックのインデックス
    @param [in] idofblk ブロックの中の自由度番号
    @param [in] val 値
    */
    void SetValue(unsigned int iblk, unsigned int idofblk, double val);

    /*!
    @brief 特定の要素に値を足し合わせる
    @param [in] iblk ブロックのインデックス
    @param [in] idofblk ブロックの中の自由度番号
    @param [in] val 足し合わせる値
    */
    void AddValue(unsigned int iblk, unsigned int idofblk, double val);

    /* アセンブリの外へはポインタを直接返却できない
    double* GetValuePtr(unsigned int iblk);
    */
    // ブロックの先頭のポインタをインデクサでラップして返却する
    //   C#側では
    //     DoubleArrayIndexer ptr = vectorBlk.GetValuePtr(iblk);
    //     double value = ptr[idofblk];
    //     ptr[idofblk] = 0;
    //   のようにアクセス可能
    DelFEM4NetCom::DoubleArrayIndexer^ GetValuePtr(unsigned int iblk);

protected:
    MatVec::CVector_Blk *self;

};

// ポインタ操作用クラス
public ref class CVector_Blk_Ptr : public CVector_Blk
{
public:
    CVector_Blk_Ptr(MatVec::CVector_Blk *baseSelf) : CVector_Blk(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CVector_Blk_Ptr(const CVector_Blk_Ptr% rhs) : CVector_Blk(false)
    {
        this->self = rhs.self;
    }

public:
    ~CVector_Blk_Ptr()
    {
        this->!CVector_Blk_Ptr();
    }

    !CVector_Blk_Ptr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace 'Ls'

#endif // VEC_H
