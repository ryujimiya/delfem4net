/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

Copyrig1ht (C) 2009  Nobuyuki Umetani    n.umetani@gmail.com

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
@brief 複素数ブロックCRS行列クラス(MatVec::CZMat_BlkCrs)クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_ZMAT_BLK_CRS_H)
#define DELFEM4NET_ZMAT_BLK_CRS_H

#include <assert.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/complex.h"

#include "delfem/matvec/zmat_blkcrs.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom{
    
    ref class CIndexedArray;
}

namespace DelFEM4NetMatVec
{

ref class CBCFlag;
ref class CZVector_Blk;
/*! 
@brief 複素数ブロックCRS行列クラス
@ingroup MatVec
*/
public ref class CZMat_BlkCrs  
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CZMat_BlkCrs * CreateNativeInstance(const MatVec::CZMat_BlkCrs& rhs);
public:
    CZMat_BlkCrs();
    CZMat_BlkCrs(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CZMat_BlkCrs(const CZMat_BlkCrs% rhs);
    CZMat_BlkCrs(const MatVec::CZMat_BlkCrs& native_instance); // nativeインスタンスから生成
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CZMat_BlkCrs(MatVec::CZMat_BlkCrs *self);
public:
    CZMat_BlkCrs(unsigned int nblk_col, unsigned int len_col, 
                 unsigned int nblk_row, unsigned int len_row );
    CZMat_BlkCrs(const CZMat_BlkCrs% rhs, bool is_value, bool isnt_trans, bool isnt_conj);

    virtual ~CZMat_BlkCrs();
    !CZMat_BlkCrs();
    
    property MatVec::CZMat_BlkCrs * Self
    {
       MatVec::CZMat_BlkCrs * get();
    }

    void Initialize(unsigned int nblk_col,unsigned int len_col, 
                    unsigned int nblk_row, unsigned int len_row );

    unsigned int NBlkMatCol();
    unsigned int NBlkMatRow();
    
    unsigned int LenBlkCol();
    unsigned int LenBlkRow();

    virtual bool SetZero();
    virtual bool DeletePattern();
    
    virtual bool AddPattern(DelFEM4NetCom::CIndexedArray^ crs);
    virtual bool AddPattern(DelFEM4NetMatVec::CZMat_BlkCrs^ rhs, bool isnt_trans);

    virtual bool SetValue(DelFEM4NetMatVec::CZMat_BlkCrs^ rhs, bool isnt_trans, bool isnt_conj);
    
    virtual bool MatVec(double alpha, DelFEM4NetMatVec::CZVector_Blk^ x, double beta, DelFEM4NetMatVec::CZVector_Blk^% b, bool isnt_trans) ;  // IN/OUT: b

    bool SetBoundaryCondition_Row(DelFEM4NetMatVec::CBCFlag^ bc_flag);
    bool SetBoundaryCondition_Colum(DelFEM4NetMatVec::CBCFlag^ bc_flag);
    
    ////////////////////////////////////////////////
    // Crs Original Function ( Non-virtual )
    ////////////////////////////////////////////////
    unsigned int NCrs();
    

    DelFEM4NetCom::ConstUIntArrayIndexer^ GetPtrIndPSuP(unsigned int ipoin, [Out]unsigned int% npsup);
    DelFEM4NetCom::ComplexArrayIndexer^ GetPtrValPSuP(unsigned int ipoin, [Out]unsigned int% npsup);

    
    /*
    const unsigned int* GetPtrIndPSuP(const unsigned int ipoin, unsigned int& npsup) const;
    const Com::Complex* GetPtrValPSuP(const unsigned int ipoin, unsigned int& npsup) const;
    Com::Complex* GetPtrValPSuP(const unsigned int ipoin, unsigned int& npsup);
    */

protected:
    MatVec::CZMat_BlkCrs *self;

};


// ポインタ操作用クラス
public ref class CZMat_BlkCrs_Ptr : public CZMat_BlkCrs
{
public:
    CZMat_BlkCrs_Ptr(MatVec::CZMat_BlkCrs *baseSelf) : CZMat_BlkCrs(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CZMat_BlkCrs_Ptr(const CZMat_BlkCrs_Ptr% rhs) : CZMat_BlkCrs(false)
    {
        this->self = rhs.self;
    }

public:
    ~CZMat_BlkCrs_Ptr()
    {
        this->!CZMat_BlkCrs_Ptr();
    }

    !CZMat_BlkCrs_Ptr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // namespace LS

#endif 
