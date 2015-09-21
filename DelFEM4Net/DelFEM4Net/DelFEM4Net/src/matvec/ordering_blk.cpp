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


////////////////////////////////////////////////////////////////
// Ordering_Blk.cpp : オーダリングクラス(COrdering_Blk)の実装
////////////////////////////////////////////////////////////////


#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#ifndef for 
#define for if(0); else for
#endif

#include <vector>
//#include <list>
//#include <map>
//#include <algorithm>
//#include <iostream>

#include "DelFEM4Net/matvec/ordering_blk.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"

using namespace DelFEM4NetMatVec;


COrdering_Blk::COrdering_Blk()
{
    this->self = new MatVec::COrdering_Blk();
}

COrdering_Blk::COrdering_Blk(const COrdering_Blk% rhs)
{
    const MatVec::COrdering_Blk& rhs_instance_ = *(rhs.self);
    // shallow copyの問題あり
    //this->self = new MatVec::COrdering_Blk(rhs_instance_);
    assert(false);
}

COrdering_Blk::COrdering_Blk(MatVec::COrdering_Blk *self)
{
    this->self = self;
}

COrdering_Blk::~COrdering_Blk()
{
    this->!COrdering_Blk();
}

COrdering_Blk::!COrdering_Blk()
{
    delete this->self;
}

MatVec::COrdering_Blk * COrdering_Blk::Self::get() { return this->self; }

void COrdering_Blk::SetOrdering(IList<int>^ ord)
{
    std::vector<int> ord_;
    DelFEM4NetCom::ClrStub::ListToVector<int>(ord, ord_);
    
    this->self->SetOrdering(ord_);
}

void COrdering_Blk::MakeOrdering_RCM(CMatDia_BlkCrs^ mat)
{
    this->self->MakeOrdering_RCM(*(mat->Self));
}

void COrdering_Blk::MakeOrdering_RCM2(CMatDia_BlkCrs^ mat)
{
    this->self->MakeOrdering_RCM2(*(mat->Self));
}

void COrdering_Blk::MakeOrdering_AMD(CMatDia_BlkCrs^ mat)
{
    this->self->MakeOrdering_AMD(*(mat->Self));
}

unsigned int COrdering_Blk::NBlk()
{
    return this->self->NBlk();
}

int COrdering_Blk::NewToOld(unsigned int iblk_new)
{
    return this->self->NewToOld(iblk_new);
}

int COrdering_Blk::OldToNew(unsigned int iblk_old)
{
    return this->self->OldToNew(iblk_old);
}

void COrdering_Blk::OrderingVector_NewToOld([Out] CVector_Blk^% vec_to, CVector_Blk^ vec_from)
{
    // 格納先を作成(ブロック数、ブロックサイズで初期化する。)
    unsigned int nblk = vec_from->NBlk();
    unsigned int len = vec_from->Len();
    MatVec::CVector_Blk *vec_to_ = new MatVec::CVector_Blk(nblk, len);
    vec_to = gcnew CVector_Blk(vec_to_);
    
    this->self->OrderingVector_NewToOld(*vec_to_, *(vec_from->Self));
}

void COrdering_Blk::OrderingVector_OldToNew([Out] CVector_Blk^% vec_to, CVector_Blk^ vec_from)
{
    // 格納先を作成(ブロック数、ブロックサイズで初期化する。)
    unsigned int nblk = vec_from->NBlk();
    unsigned int len = vec_from->Len();
    MatVec::CVector_Blk *vec_to_ = new MatVec::CVector_Blk(nblk, len);
    vec_to = gcnew CVector_Blk(vec_to_);
    
    this->self->OrderingVector_OldToNew(*vec_to_, *(vec_from->Self));
}

