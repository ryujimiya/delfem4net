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

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif
#define for if(0); else for

#include <cassert>
#include <iostream>
#include <cstdlib> //(abort)

#include "DelFEM4Net/matvec/zmat_blkcrs.h"
#include "DelFEM4Net/matvec/zvector_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"
#include "DelFEM4Net/indexed_array.h"

using namespace DelFEM4NetMatVec;

//////////////////////////////////////////////////////////////////////
// CZMat_BlkCrs
//////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CZMat_BlkCrs * CZMat_BlkCrs::CreateNativeInstance(const MatVec::CZMat_BlkCrs& rhs)
{
    MatVec::CZMat_BlkCrs * ptr = NULL;
    
    unsigned int nblk_col = rhs.NBlkMatCol();
    unsigned int nblk_row = rhs.NBlkMatRow();
    unsigned int len_col = rhs.LenBlkCol();
    unsigned int len_row = rhs.LenBlkRow();

    ptr = new MatVec::CZMat_BlkCrs(nblk_col, len_col, nblk_row, len_row);
    ptr->SetValue(rhs, true, true);
    
    return ptr;
}

CZMat_BlkCrs::CZMat_BlkCrs()
{
    this->self = new MatVec::CZMat_BlkCrs();
}

CZMat_BlkCrs::CZMat_BlkCrs(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CZMat_BlkCrs::CZMat_BlkCrs(const CZMat_BlkCrs% rhs)
{
    const MatVec::CZMat_BlkCrs& rhs_instance_ = *(rhs.self);
    // shallow copyの問題あり
    //this->self = new MatVec::CZMat_BlkCrs(rhs_instance_);
    // 修正版
    this->self = CZMat_BlkCrs::CreateNativeInstance(rhs_instance_);
}

CZMat_BlkCrs::CZMat_BlkCrs(const MatVec::CZMat_BlkCrs& native_instance)
{
    this->self = CZMat_BlkCrs::CreateNativeInstance(native_instance);
}

CZMat_BlkCrs::CZMat_BlkCrs(MatVec::CZMat_BlkCrs *self)
{
    this->self = self;
}


CZMat_BlkCrs::CZMat_BlkCrs(unsigned int nblk_col, unsigned int len_col, 
            unsigned int nblk_row, unsigned int len_row )
{
    this->self = new MatVec::CZMat_BlkCrs(nblk_col, len_col, nblk_row, len_row);
}

CZMat_BlkCrs::CZMat_BlkCrs(const CZMat_BlkCrs% rhs, bool is_value, bool isnt_trans, bool isnt_conj)
{
    this->self = new MatVec::CZMat_BlkCrs(*(rhs.self), is_value, isnt_trans, isnt_conj);
}


CZMat_BlkCrs::~CZMat_BlkCrs()
{
    this->!CZMat_BlkCrs();
}

CZMat_BlkCrs::!CZMat_BlkCrs()
{
    delete this->self;
}

MatVec::CZMat_BlkCrs * CZMat_BlkCrs::Self::get() { return this->self; }

void CZMat_BlkCrs::Initialize(unsigned int nblk_col, unsigned int len_col, 
                        unsigned int nblk_row, unsigned int len_row )
{
    this->self->Initialize(nblk_col, len_col, nblk_row, len_row);
}


unsigned int CZMat_BlkCrs::NBlkMatCol()
{
    return this->self->NBlkMatCol();
}

unsigned int CZMat_BlkCrs::NBlkMatRow()
{
    return this->self->NBlkMatRow();
}

unsigned int CZMat_BlkCrs::LenBlkCol()
{
    return this->self->LenBlkCol();
}

unsigned int CZMat_BlkCrs::LenBlkRow()
{
    return this->self->LenBlkRow();
}

bool CZMat_BlkCrs::SetZero()
{
    return this->self->SetZero();
}

bool CZMat_BlkCrs::DeletePattern()
{
    return this->self->DeletePattern();
}


bool CZMat_BlkCrs::AddPattern(DelFEM4NetCom::CIndexedArray^ crs)
{
    return this->self->AddPattern(*(crs->Self));
}

bool CZMat_BlkCrs::AddPattern(CZMat_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->AddPattern(*(rhs->self), isnt_trans);
}

bool CZMat_BlkCrs::SetValue(CZMat_BlkCrs^ rhs, bool isnt_trans, bool isnt_conj)
{
    return this->self->SetValue(*(rhs->self), isnt_trans, isnt_conj);
}

bool CZMat_BlkCrs::MatVec(double alpha, CZVector_Blk^ x, double beta, CZVector_Blk^% b, bool isnt_trans)  // IN/OUT: b
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    ////コピーコンストラクタ不備によりshallow copyとなる
    ////MatVec::CZVector_Blk *b_ = new MatVec::CZVector_Blk(*b->Self); // コピーを取る
    ////b = gcnew CZVector_Blk(b_);
    //const MatVec::CZVector_Blk& b_instance_ = *(b->Self);
    //b = gcnew CZVector_Blk(b_instance_);
    
    bool ret = this->self->MatVec(alpha, *(x->Self), beta, *(b->Self), isnt_trans);
    
    return ret;
}

bool CZMat_BlkCrs::SetBoundaryCondition_Row(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition_Row(*(bc_flag->Self));
}

bool CZMat_BlkCrs::SetBoundaryCondition_Colum(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition_Colum(*(bc_flag->Self));
}

unsigned int CZMat_BlkCrs::NCrs()
{
    return this->self->NCrs();
}

DelFEM4NetCom::ConstUIntArrayIndexer^ CZMat_BlkCrs::GetPtrIndPSuP(unsigned int ipoin, [Out]unsigned int% npsup)
{
    unsigned int npsup_;
    const unsigned int * ptr_ = this->self->GetPtrIndPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return gcnew DelFEM4NetCom::ConstUIntArrayIndexer(npsup_, ptr_);
}

DelFEM4NetCom::ComplexArrayIndexer^ CZMat_BlkCrs::GetPtrValPSuP(unsigned int ipoin, [Out]unsigned int% npsup)
{
    unsigned int npsup_;
    Com::Complex * ptr_ = this->self->GetPtrValPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return gcnew DelFEM4NetCom::ComplexArrayIndexer(npsup_, ptr_);
}

