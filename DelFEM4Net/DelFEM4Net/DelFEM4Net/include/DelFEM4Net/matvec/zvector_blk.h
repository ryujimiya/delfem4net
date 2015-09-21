/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

DelFEM (Finite Element Analysis)
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
@brief 複素数ブロックベクトルクラス(MatVec::CZVector_Blk)のインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_ZVECTOR_BLK_H)
#define DELFEM4NET_ZVECTOR_BLK_H

#include <assert.h>
#include <stdio.h>

#include "DelFEM4Net/complex.h"

#include "delfem/matvec/zvector_blk.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


namespace DelFEM4NetMatVec
{

//ref class CZMat_BlkCrs;
//ref class CZMatDia_BlkCrs;
//ref class CZMatDiaInv_BlkDia;

/*!
@brief 複素数ベクトルクラス
@ingroup MatVec
*/
public ref class CZVector_Blk  
{
public:
    static DelFEM4NetCom::Complex^ operator*(CZVector_Blk^ lhs, CZVector_Blk^ rhs);
    static DelFEM4NetCom::Complex^ InnerProduct(CZVector_Blk^ lhs, CZVector_Blk^ rhs);
    
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CZVector_Blk * CreateNativeInstance(const MatVec::CZVector_Blk& rhs);

public:
    /*デフォルトコンストラクタなし
    CZVector_Blk();
    */
    CZVector_Blk(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CZVector_Blk(const CZVector_Blk% rhs);
    CZVector_Blk(const MatVec::CZVector_Blk& native_instance); // nativeインスタンスから生成
    
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CZVector_Blk(MatVec::CZVector_Blk *self);

public:
    CZVector_Blk(unsigned int iblkveclen, unsigned int iblklen);
    virtual ~CZVector_Blk();
    !CZVector_Blk();

    property MatVec::CZVector_Blk * Self
    {
       MatVec::CZVector_Blk * get();
    }

    CZVector_Blk^ operator*=(double d0);    // Scaler Product
    CZVector_Blk^ operator*=(DelFEM4NetCom::Complex^ c0);    // Scaler Product
    CZVector_Blk^ operator=(CZVector_Blk^ rhs); // Substitue Vector
    CZVector_Blk^ operator+=(CZVector_Blk^ rhs); // Add 

    ////////////////////////////////
    // Menber function

    CZVector_Blk^ AXPY(double alpha, CZVector_Blk^ rhs); // Add scaler scaled Vector    
    CZVector_Blk^ AXPY(DelFEM4NetCom::Complex^ alpha, CZVector_Blk^ rhs); // Add scaler scaled Vector

    void SetVectorZero();    // Set 0 to Value
    double GetSquaredVectorNorm();    // return norm of this vector
    void SetVectorConjugate();

    unsigned int BlkLen();    // Size of One Block
    unsigned int BlkVecLen();    // Size of Bocks Vector have
    DelFEM4NetCom::Complex^ GetValue(unsigned int iblk, unsigned int idofblk);
    void SetValue(unsigned int iblk, unsigned int idofblk, DelFEM4NetCom::Complex^ val);
    void AddValue(unsigned int iblk, unsigned int idofblk, DelFEM4NetCom::Complex^ val);
    
protected:
    MatVec::CZVector_Blk *self;

};

// ポインタ操作用クラス
public ref class CZVector_Blk_Ptr : public CZVector_Blk
{
public:
    CZVector_Blk_Ptr(MatVec::CZVector_Blk *baseSelf) : CZVector_Blk(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CZVector_Blk_Ptr(const CZVector_Blk_Ptr% rhs) : CZVector_Blk(false)
    {
        this->self = rhs.self;
    }

public:
    ~CZVector_Blk_Ptr()
    {
        this->!CZVector_Blk_Ptr();
    }

    !CZVector_Blk_Ptr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace Ls

#endif // VEC_H
