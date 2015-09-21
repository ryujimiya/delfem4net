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


#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#ifndef for
#define for if(0); else for
#endif

#include <cassert>
//#include <iostream>
//#include <algorithm>

#include "DelFEM4Net/matvec/diamat_blk.h"

using namespace DelFEM4NetMatVec;


//////////////////////////////////////////////////////////////////////
// CDiaMat_Blk
//////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CDiaMat_Blk * CDiaMat_Blk::CreateNativeInstance(const MatVec::CDiaMat_Blk& rhs)
{
    MatVec::CDiaMat_Blk * ptr = NULL;
    
    MatVec::CDiaMat_Blk& rhs_work = const_cast<MatVec::CDiaMat_Blk&>(rhs);   // constを外さないとサイズが取得できない。しかたがないのでconstをはずす
    unsigned int nblk = rhs_work.NBlk();
    unsigned int nlen = rhs_work.LenBlk();
    unsigned int blockSize = nlen * nlen;
    ptr = new MatVec::CDiaMat_Blk(nblk, nlen);
    //this->self->SetZero();
    for (unsigned int iblk = 0; iblk < nblk; iblk++)
    {
       const double *ptr_blk = rhs_work.GetPtrValDia(iblk);

       ptr->Mearge(iblk, blockSize, ptr_blk);
    }
    
    return ptr;
}


/*
CDiaMat_Blk::CDiaMat_Blk()
{
    //実装されていない
    //this->self = new MatVec::CDiaMat_Blk();
}
*/

CDiaMat_Blk::CDiaMat_Blk(const CDiaMat_Blk% rhs)
{
    const MatVec::CDiaMat_Blk& rhs_instance_ = *(rhs.self);
    // shallow copyとなる
    //this->self = new MatVec::CDiaMat_Blk(rhs_instance_);
    // 修正版
    this->self = CDiaMat_Blk::CreateNativeInstance(rhs_instance_);
}

CDiaMat_Blk::CDiaMat_Blk(const MatVec::CDiaMat_Blk& native_instance)
{
    this->self = CDiaMat_Blk::CreateNativeInstance(native_instance);
}

CDiaMat_Blk::CDiaMat_Blk(MatVec::CDiaMat_Blk *self)
{
    this->self = self;
}

CDiaMat_Blk::CDiaMat_Blk(unsigned int nblk, unsigned int nlen)
{
    this->self = new MatVec::CDiaMat_Blk(nblk, nlen);
}

CDiaMat_Blk::~CDiaMat_Blk()
{
    this->!CDiaMat_Blk();
}

CDiaMat_Blk::!CDiaMat_Blk()
{
    delete this->self;
}

MatVec::CDiaMat_Blk * CDiaMat_Blk::Self::get() { return this->self; }

unsigned int CDiaMat_Blk::NBlk()
{
    return this->self->NBlk();
}

unsigned int CDiaMat_Blk::LenBlk()
{
    return this->self->LenBlk();
}

void CDiaMat_Blk::SetZero()
{
    this->self->SetZero();
}

bool CDiaMat_Blk::Mearge(unsigned int iblk, unsigned int blksize, array<double>^ emat )
{
    pin_ptr<double> emat_= &emat[0];
    return this->self->Mearge(iblk, blksize, (double*)emat_);
}


bool CDiaMat_Blk::CholeskyDecomp()
{
    return this->self->CholeskyDecomp();
}

bool CDiaMat_Blk::MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% b)  // [IN/OUT]lhs
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    //MatVec::CVector_Blk *b_ = new MatVec::CVector_Blk(*b->Self); // コピーを取る
    //b = gcnew CVector_Blk(b_);
    
    bool ret = this->self->MatVec(alpha, *(x->Self), beta, *(b->Self));
    
    return ret;
}

DelFEM4NetCom::ConstDoubleArrayIndexer^ CDiaMat_Blk::GetPtrValDia(unsigned int iblk)
{
    const double *ptr = this->self->GetPtrValDia(iblk);
    unsigned int blockSize = this->self->LenBlk() * this->self->LenBlk();
    
    return gcnew DelFEM4NetCom::ConstDoubleArrayIndexer(blockSize, ptr);
}
