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
@brief CMatDia_BlkCrs クラスのインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_DIAMAT_BLK_H)
#define DELFEM4NET_DIAMAT_BLK_H

#ifndef for 
#define for if(0); else for
#endif

#include <math.h>

#include "DelFEM4Net/stub/clr_stub.h"  // DelFEM4NetCom::DoubleArrayIndexer
#include "DelFEM4Net/matvec/vector_blk.h"

#include "delfem/matvec/diamat_blk.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{

/*! 
@brief diagonal matrix class
@ingroup MatVec
*/
public ref class CDiaMat_Blk
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CDiaMat_Blk * CreateNativeInstance(const MatVec::CDiaMat_Blk& rhs);
public:
    //CDiaMat_Blk();
    CDiaMat_Blk(const CDiaMat_Blk% rhs);
    CDiaMat_Blk(const MatVec::CDiaMat_Blk& native_instance); // nativeインスタンスから生成
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CDiaMat_Blk(MatVec::CDiaMat_Blk *self);
public:
    CDiaMat_Blk(unsigned int nblk, unsigned int nlen);
    virtual ~CDiaMat_Blk();
    !CDiaMat_Blk();
    
    property MatVec::CDiaMat_Blk * Self
    {
       MatVec::CDiaMat_Blk * get();
    }

    unsigned int NBlk();
    unsigned int LenBlk();

    //! 行列に０をセットするための関数
    virtual void SetZero();

    //! マージするための関数
    virtual bool Mearge(unsigned int iblk, unsigned int blksize, array<double>^ emat );

    bool CholeskyDecomp();

    /*!
    @brief 行列ベクトル積
    {lhs} = beta*{b} + alpha*[A]{x}
    */
    virtual bool MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% b);  // [IN/OUT]lhs

    DelFEM4NetCom::ConstDoubleArrayIndexer^ GetPtrValDia(unsigned int iblk);
    //const double* GetPtrValDia(unsigned int iblk);

protected:
    MatVec::CDiaMat_Blk *self;
};

}

#endif
