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
@brief Interface of class (MatVec::CMat_BlkCrs)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_MAT_CRS_H)
#define DELFEM4NET_MAT_CRS_H

#include <assert.h>
#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"  // DelFEM4NetCom::DoubleArrayIndexer

#include "delfem/matvec/mat_blkcrs.h"

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
ref class COrdering_Blk;
ref class CBCFlag;

/*! 
@brief ブロックCRS構造の行列クラス
@ingroup MatVec
*/
public ref class CMat_BlkCrs  
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CMat_BlkCrs * CreateNativeInstance(const MatVec::CMat_BlkCrs& rhs);
public:
    CMat_BlkCrs();
    CMat_BlkCrs(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CMat_BlkCrs(const CMat_BlkCrs% rhs);
    CMat_BlkCrs(const MatVec::CMat_BlkCrs& native_instance); // nativeインスタンスから生成
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CMat_BlkCrs(MatVec::CMat_BlkCrs *self);
public:

    /*!
    @brief サイズを引数としたコンストラクタ
    @param[in] nblk_col 列のブロック数
    @param[in] len_col  ブロックの列の長さ
    @param[in] nblk_row 行のブロック数
    @param[in] len_row  ブロックの行の長さ
    */
    CMat_BlkCrs(unsigned int nblk_col, unsigned int len_col, 
                unsigned int nblk_row, unsigned int len_row );
    CMat_BlkCrs(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
                unsigned int nblk_row, IList<unsigned int>^ alen_row );

    /*!
    @brief 行列をコピーするためのコンストラクタ
    @param[in] rhs 引数行列
    @param[in] is_value 値までコピーするかどうか
    @param[in] isnt_trans 転置としてコピーしないか
    */
    CMat_BlkCrs(const CMat_BlkCrs% rhs, bool is_value, bool isnt_trans);

    virtual ~CMat_BlkCrs();
    !CMat_BlkCrs();
    
    property MatVec::CMat_BlkCrs * Self
    {
       MatVec::CMat_BlkCrs * get();
    }

    virtual bool Initialize(unsigned int nblk_col, unsigned int len_col, 
                            unsigned int nblk_row, unsigned int len_row );

    // Diaで隠蔽するためにvirtualにしておく
    virtual bool Initialize(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
                            unsigned int nblk_row, IList<unsigned int>^ alen_row );

    unsigned int NBlkMatCol();    //!< 行に幾つブロックがあるか
    unsigned int NBlkMatRow();    //!< 列に幾つブロックがあるか
    
    //! ブロックの行のサイズ(縦の長さ)を取得.もしもFixされて無い場合は-1を返す
    int LenBlkCol();
    //! ブロックの行のサイズ(縦の長さ)を取得.
    unsigned int LenBlkCol(unsigned int iblk);

    //! ブロックの列のサイズ(横の長さ)を取得.もしもFixされて無い場合は-1を返す
    int LenBlkRow();
    
    //! ブロックの行のサイズ(横の長さ)を取得.
    unsigned int LenBlkRow(unsigned int iblk);

    //! 値に０を設定．データ領域が確保されていなければ，確保する
    virtual bool SetZero();
    
    //! パターンを全て消去　RowPtr,Valはメモリ解放
    virtual bool DeletePattern();

    virtual void FillPattern();
    
    virtual bool AddPattern(DelFEM4NetCom::CIndexedArray^ crs);    //!< 非ゼロパターンを追加する
    
    virtual bool AddPattern(CMat_BlkCrs^ rhs, bool isnt_trans);    //!< 非ゼロパターンを追加する
    
    virtual bool AddPattern(CMat_BlkCrs^ rhs, 
                            COrdering_Blk^ order_col, COrdering_Blk^ order_row);    //!< 非ゼロパターンを追加する

    // ここは派生クラスに任せるつもり
    virtual bool SetPatternBoundary(CMat_BlkCrs^ rhs, CBCFlag^ bc_flag_col, CBCFlag^ bc_flag_row);
    
    virtual bool SetPatternDia(CMat_BlkCrs^ rhs);
    
    virtual bool SetValue(CMat_BlkCrs^ rhs, bool isnt_trans);
    virtual bool SetValue(CMat_BlkCrs^ rhs, 
                          COrdering_Blk^ order_col, COrdering_Blk^ order_row);
    
    //! 要素剛性行列をマージする
    virtual bool Mearge
        (unsigned int nblkel_col, array<unsigned int>^ blkel_col,
         unsigned int nblkel_row, array<unsigned int>^ blkel_row,
         unsigned int blksize, array<double>^ emat);

    //! 作業用配列の領域を解放する
    void DeleteMargeTmpBuffer();

    //! 行列ベクトル積
    virtual bool MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% b, bool isnt_trans);  // IN/OUT: b

    //! bc_flagが１の行の要素を０にする
    bool SetBoundaryCondition_Row(CBCFlag^ bc_flag);
    
    //! bc_flagが１の列の要素を０にする
    bool SetBoundaryCondition_Colum(CBCFlag^ bc_flag);
    
    //! bc_flagが０の要素を０にする（全体剛性行列保存による高速化法）
    bool SetBoundaryConditionInverse_Colum(CBCFlag^ bc_flag);
    
    ////////////////////////////////////////////////
    // Crs Original Function ( Non-virtual )

    //! CRSのサイズ取得
    unsigned int NCrs();
    
    // 
    // [C#]
    //   ConstUIntArrayIndexer rowPtrBlkIndexer = matBlkCrs.GetPtrIndPSuP(ipoin, out npsup);
    //   foreach (uint iRowBlk in rowPtrBlkIndexer) { }
    //   for (int i = 0; i < npsup; i++) { uint iRowBlk = rowPtrBlkIndexer[i]; }
    DelFEM4NetCom::ConstUIntArrayIndexer^ GetPtrIndPSuP(unsigned int ipoin, [Out]unsigned int% npsup);

    DelFEM4NetCom::DoubleArrayIndexer^ GetPtrValPSuP(unsigned int ipoin, [Out]unsigned int% npsup);
    
    /* nativeなポインタは返却不可
    const unsigned int* GetPtrIndPSuP(const unsigned int ipoin, [Out] unsigned int% npsup);
    const double* GetPtrValPSuP(const unsigned int ipoin, [Out] unsigned int% npsup);
    double* GetPtrValPSuP(const unsigned int ipoin, [Out] unsigned int% npsup);
    */

protected:
    MatVec::CMat_BlkCrs *self;
};


// ポインタ操作用クラス
public ref class CMat_BlkCrs_Ptr : public CMat_BlkCrs
{
public:
    CMat_BlkCrs_Ptr(MatVec::CMat_BlkCrs *baseSelf) : CMat_BlkCrs(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CMat_BlkCrs_Ptr(const CMat_BlkCrs_Ptr% rhs) : CMat_BlkCrs(false)
    {
        this->self = rhs.self;
    }

public:
    ~CMat_BlkCrs_Ptr()
    {
        this->!CMat_BlkCrs_Ptr();
    }

    !CMat_BlkCrs_Ptr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace 'Ls'

#endif 
