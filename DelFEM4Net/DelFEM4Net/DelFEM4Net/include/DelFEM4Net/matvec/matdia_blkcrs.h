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
@brief 正方行列クラス(MatVec::CMatDia_BlkCrs)のインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_MATDIA_CRS_H)
#define DELFEM4NET_MATDIA_CRS_H

#include "DelFEM4Net/stub/clr_stub.h"  // DelFEM4NetCom::ClrStub
#include "DelFEM4Net/matvec/mat_blkcrs.h"    // このクラスを継承している

#include "delfem/matvec/matdia_blkcrs.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{
    ref class CIndexedArray;
}

namespace DelFEM4NetMatVec
{

ref class CVector_Blk;
ref class CBCFlag;
ref class COrdering_Blk;

/*!
@brief square crs matrix class
@ingroup MatVec

データ構造:対角成分を持つブロックCRSクラス
*/
public ref class CMatDia_BlkCrs  : public CMat_BlkCrs
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CMatDia_BlkCrs * CreateNativeInstance(const MatVec::CMatDia_BlkCrs& rhs);
    
public:
    CMatDia_BlkCrs();
    CMatDia_BlkCrs(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CMatDia_BlkCrs(const CMatDia_BlkCrs% rhs);
    CMatDia_BlkCrs(const MatVec::CMatDia_BlkCrs& native_instance); // nativeインスタンスから生成
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CMatDia_BlkCrs(MatVec::CMatDia_BlkCrs *self);
public:

    /*!
    @brief サイズを引数とするコンストラクタ
    @param[in] nblk_colrow ブロック数
    @param[in] len_colrow ブロックのサイズ
    */
    CMatDia_BlkCrs(unsigned int nblk_colrow, unsigned int len_colrow);
    
    virtual ~CMatDia_BlkCrs();
    !CMatDia_BlkCrs();
    
    property MatVec::CMatDia_BlkCrs * Self
    {
       MatVec::CMatDia_BlkCrs * get();
    }

    // Mat_BlkCrsクラスの隠蔽
    virtual bool Initialize(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
                            unsigned int nblk_row, IList<unsigned int>^ alen_row ) override;
    bool Initialize(unsigned int nblk, IList<unsigned int>^ alen)
    {
        return this->Initialize(nblk, alen, nblk, alen);
    }

    // Mat_BlkCrsクラスの隠蔽
    virtual bool Initialize(unsigned int nblk_col, unsigned int len_col,
                            unsigned int nblk_row, unsigned int len_row ) override;
    bool Initialize(unsigned int nblk, unsigned int len)
    {
        return this->Initialize(nblk,len, nblk,len);
    }

    //! パターンを全て消去　RowPtr,Valはメモリ解放
    virtual bool DeletePattern() override;
    virtual bool AddPattern(DelFEM4NetCom::CIndexedArray^ crs) override;
    bool AddPattern(CMatDia_BlkCrs^ rhs, bool isnt_trans);
    bool AddPattern(CMatDia_BlkCrs^ rhs, COrdering_Blk^ order);
    bool AddPattern(CMat_BlkCrs^ m1, CMatDia_BlkCrs^ m2, CMat_BlkCrs^ m3);

    virtual bool SetValue(CMatDia_BlkCrs^ rhs, bool isnt_trans);
    virtual bool SetValue(CMat_BlkCrs^ m1, CMatDia_BlkCrs^ m2, CMat_BlkCrs^ m3);    // := m1*m2*m3
    virtual bool SetValue(CMatDia_BlkCrs^ rhs, COrdering_Blk^ order);

    //! 行列に０をセットするための関数(親クラスの隠蔽)
    virtual bool SetZero() override;
    //! マージするための関数(親クラスの隠蔽)
    virtual bool Mearge(
        unsigned int nblkel_col, array<unsigned int>^ blkel_col,
        unsigned int nblkel_row, array<unsigned int>^ blkel_row,
        unsigned int blksize, array<double>^ emat) override;
    /*!
    @brief 行列ベクトル積(親クラスの隠蔽)
    {y} = alpha * [A]{x} + beta * {y}
    */
    virtual bool MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% y);  // [IN/OUT] y

    //! bc_flagが１の自由度の行と列を０に設定，但し対角成分は１を設定
    bool SetBoundaryCondition(CBCFlag^ bc_flag);

    ////////////////////////////////////////////////////////////////
    // 前処理用のクラス

    DelFEM4NetCom::DoubleArrayIndexer^ GetPtrValDia(unsigned int ipoin);
    /*
    const double* GetPtrValDia(const unsigned int ipoin);
    double* GetPtrValDia(const unsigned int ipoin);
    */

protected:
    MatVec::CMatDia_BlkCrs *self;
    
    virtual void setBaseSelf();

};

// ポインタ操作用クラス
public ref class CMatDia_BlkCrs_Ptr : public CMatDia_BlkCrs
{
public:
    CMatDia_BlkCrs_Ptr(MatVec::CMatDia_BlkCrs *baseSelf) : CMatDia_BlkCrs(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
        
        setBaseSelf();
    }

private:
    CMatDia_BlkCrs_Ptr(const CMatDia_BlkCrs_Ptr% rhs) : CMatDia_BlkCrs(false)
    {
        this->self = rhs.self;

        setBaseSelf();
    }

public:
    ~CMatDia_BlkCrs_Ptr()
    {
        this->!CMatDia_BlkCrs_Ptr();
    }

    !CMatDia_BlkCrs_Ptr()
    {
        // 削除しない
        this->self = NULL;

        setBaseSelf();
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace 'Ls'

#endif // MATDIA_CRS_H
