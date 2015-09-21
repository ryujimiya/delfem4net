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
@brief 複素数対角ブロックCRS行列クラス(MatVec::CZMatDia_BlkCrs)クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_ZMAT_DIA_BLK_CRS_H)
#define DELFEM4NET_ZMAT_DIA_BLK_CRS_H

#include <string>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/complex.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"
#include "DelFEM4Net/matvec/zmat_blkcrs.h"

#include "delfem/matvec/zmatdia_blkcrs.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{

ref class CZVector_Blk;
/*! 
@brief 複素数対角ブロックCRS行列クラス
@ingroup MatVec
*/
public ref class CZMatDia_BlkCrs  : public CZMat_BlkCrs
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CZMatDia_BlkCrs * CreateNativeInstance(const MatVec::CZMatDia_BlkCrs& rhs);
    
public:
    //CZMatDia_BlkCrs();
    CZMatDia_BlkCrs(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CZMatDia_BlkCrs(const CZMatDia_BlkCrs% rhs);
    CZMatDia_BlkCrs(const MatVec::CZMatDia_BlkCrs& native_instance); // nativeインスタンスから生成
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CZMatDia_BlkCrs(MatVec::CZMatDia_BlkCrs *self);
public:
    CZMatDia_BlkCrs(unsigned int nblk_colrow, unsigned int len_colrow);
    CZMatDia_BlkCrs(String^ file_path);
    CZMatDia_BlkCrs(const CZMatDia_BlkCrs% rhs, bool is_value, bool isnt_trans, bool isnt_conj);
    virtual ~CZMatDia_BlkCrs();
    !CZMatDia_BlkCrs();
    
    property MatVec::CZMatDia_BlkCrs * Self
    {
       MatVec::CZMatDia_BlkCrs * get();
    }

    virtual bool DeletePattern() override;

    virtual bool AddPattern(DelFEM4NetCom::CIndexedArray^ crs) override;
    bool AddPattern(CZMatDia_BlkCrs^ rhs, bool isnt_trans);
    bool AddPattern(CZMat_BlkCrs^ m1, CZMatDia_BlkCrs^ m2, CZMat_BlkCrs^ m3);

    bool SetValue(CZMatDia_BlkCrs^ rhs, bool isnt_trans);
    bool SetValue(CZMat_BlkCrs^ m1, CZMatDia_BlkCrs^ m2, CZMat_BlkCrs^ m3);

    virtual bool SetZero() override;
    virtual bool Mearge(
        unsigned int nblkel_col, array<unsigned int>^ blkel_col,
        unsigned int nblkel_row, array<unsigned int>^ blkel_row,
        unsigned int blksize, array<DelFEM4NetCom::Complex^>^ emat, array<int>^% tmp_buffer) ;  // IN/OUT tmp_buffer(新たにハンドルを生成しない)
    void AddUnitMatrix(DelFEM4NetCom::Complex^ epsilon);

    bool SetBoundaryCondition(CBCFlag^ bc_flag);

    bool MatVec(double alpha, CZVector_Blk^ x, double beta, CZVector_Blk^% y) ;  // IN/OUT: y
    bool MatVec_Hermitian(double alpha, CZVector_Blk^ x, double beta, CZVector_Blk^% y) ; //IN/OUT: y

    ////////////////////////////////

    DelFEM4NetCom::ComplexArrayIndexer^ GetPtrValDia(unsigned int ipoin);
    /*
    const Com::Complex* GetPtrValDia(const unsigned int ipoin) const ;
    Com::Complex* GetPtrValDia(const unsigned int ipoin);
    */
    
protected:
    MatVec::CZMatDia_BlkCrs *self;
    
    virtual void setBaseSelf();

};


// ポインタ操作用クラス
public ref class CZMatDia_BlkCrs_Ptr : public CZMatDia_BlkCrs
{
public:
    CZMatDia_BlkCrs_Ptr(MatVec::CZMatDia_BlkCrs *baseSelf) : CZMatDia_BlkCrs(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
        
        setBaseSelf();
    }

private:
    CZMatDia_BlkCrs_Ptr(const CZMatDia_BlkCrs_Ptr% rhs) : CZMatDia_BlkCrs(false)
    {
        this->self = rhs.self;

        setBaseSelf();
    }

public:
    ~CZMatDia_BlkCrs_Ptr()
    {
        this->!CZMatDia_BlkCrs_Ptr();
    }

    !CZMatDia_BlkCrs_Ptr()
    {
        // 削除しない
        this->self = NULL;

        setBaseSelf();
    }

private:
    // selfはベースクラスのものを使用する

};

}    // namespace Ls

#endif // MATDIA_CRS_H
