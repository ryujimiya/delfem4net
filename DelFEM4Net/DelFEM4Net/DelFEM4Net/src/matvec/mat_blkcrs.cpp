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

////////////////////////////////////////////////////////////////
// Mat_BlkCrs.h : interface of blk crs matrix class (CMat_BlkCrs)
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#ifndef for
#define for if(0); else for
#endif

#include <cassert>
//#include <iostream>
//#include <algorithm>

#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/matvec/mat_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/ordering_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

using namespace DelFEM4NetMatVec;

//////////////////////////////////////////////////////////////////////
// CMat_BlkCrs
//////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CMat_BlkCrs * CMat_BlkCrs::CreateNativeInstance(const MatVec::CMat_BlkCrs& rhs)
{
    MatVec::CMat_BlkCrs * ptr = NULL;
    
    /*恐らくNG
    unsigned int nblk_col = rhs.NBlkMatCol();
    unsigned int nblk_row = rhs.NBlkMatRow();
    unsigned int len_col = rhs.LenBlkCol();
    unsigned int len_row = rhs.LenBlkRow();

    ptr = new MatVec::CMat_BlkCrs(nblk_col, len_col, nblk_row, len_row);
    ptr->SetValue(rhs, true);
    */
    unsigned int nblk_col = rhs.NBlkMatCol();
    unsigned int nblk_row = rhs.NBlkMatRow();
    unsigned int len_col = rhs.LenBlkCol();
    unsigned int len_row = rhs.LenBlkRow();
    assert(nblk_col == nblk_row && len_col == len_row);

    std::vector<unsigned int> alen_col;
    std::vector<unsigned int> alen_row;
    for (int iblk = 0; iblk < nblk_col; iblk++)
    {
        unsigned int lencol_rhs = rhs.LenBlkCol(iblk);
        alen_col.push_back(lencol_rhs);
        unsigned int lenrow_rhs = rhs.LenBlkRow(iblk);
        alen_row.push_back(lenrow_rhs);
    }
    ptr = new MatVec::CMat_BlkCrs();
    ptr->Initialize(nblk_col, alen_col, nblk_row, alen_row);
    ptr->AddPattern(rhs, true);
    ptr->SetValue(rhs, true);

    return ptr;
}

CMat_BlkCrs::CMat_BlkCrs()
{
    this->self = new MatVec::CMat_BlkCrs();
}

CMat_BlkCrs::CMat_BlkCrs(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CMat_BlkCrs::CMat_BlkCrs(const CMat_BlkCrs% rhs)
{
    const MatVec::CMat_BlkCrs& rhs_instance_ = *(rhs.self);
    // shallow copyの問題あり
    //this->self = new MatVec::CMat_BlkCrs(rhs_instance_);
    // 修正版
    this->self = CMat_BlkCrs::CreateNativeInstance(rhs_instance_);
}

CMat_BlkCrs::CMat_BlkCrs(const MatVec::CMat_BlkCrs& native_instance)
{
    this->self = CMat_BlkCrs::CreateNativeInstance(native_instance);
}

CMat_BlkCrs::CMat_BlkCrs(MatVec::CMat_BlkCrs *self)
{
    this->self = self;
}

CMat_BlkCrs::CMat_BlkCrs(unsigned int nblk_col, unsigned int len_col, 
            unsigned int nblk_row, unsigned int len_row )
{
    this->self = new MatVec::CMat_BlkCrs(nblk_col, len_col, nblk_row, len_row);
}

CMat_BlkCrs::CMat_BlkCrs(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
            unsigned int nblk_row, IList<unsigned int>^ alen_row )
{
    std::vector<unsigned int> alen_col_;
    std::vector<unsigned int> alen_row_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_col, alen_col_);
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_row, alen_row_);
    
    this->self = new MatVec::CMat_BlkCrs(nblk_col, alen_col_, nblk_row, alen_row_);
}

CMat_BlkCrs::CMat_BlkCrs(const CMat_BlkCrs% rhs, bool is_value, bool isnt_trans)
{
    this->self = new MatVec::CMat_BlkCrs(*(rhs.self), is_value, isnt_trans);
}


CMat_BlkCrs::~CMat_BlkCrs()
{
    this->!CMat_BlkCrs();
}

CMat_BlkCrs::!CMat_BlkCrs()
{
    delete this->self;
}

MatVec::CMat_BlkCrs * CMat_BlkCrs::Self::get() { return this->self; }

bool CMat_BlkCrs::Initialize(unsigned int nblk_col, unsigned int len_col, 
                        unsigned int nblk_row, unsigned int len_row )
{
     return this->self->Initialize(nblk_col, len_col, nblk_row, len_row);
}

bool CMat_BlkCrs::Initialize(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
                        unsigned int nblk_row, IList<unsigned int>^ alen_row )
{
    std::vector<unsigned int> alen_col_;
    std::vector<unsigned int> alen_row_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_col, alen_col_);
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_row, alen_row_);
    
    return this->self->Initialize(nblk_col, alen_col_, nblk_row, alen_row_);
}

unsigned int CMat_BlkCrs::NBlkMatCol()
{
    return this->self->NBlkMatCol();
}

unsigned int CMat_BlkCrs::NBlkMatRow()
{
    return this->self->NBlkMatRow();
}

int CMat_BlkCrs::LenBlkCol()
{
    return this->self->LenBlkCol();
}

unsigned int CMat_BlkCrs::LenBlkCol(unsigned int iblk)
{ 
    return this->self->LenBlkCol(iblk);
}

int CMat_BlkCrs::LenBlkRow()
{
    return this->self->LenBlkRow();
}

unsigned int CMat_BlkCrs::LenBlkRow(unsigned int iblk)
{ 
    return this->self->LenBlkRow(iblk);
}

bool CMat_BlkCrs::SetZero()
{
    return this->self->SetZero();
}

bool CMat_BlkCrs::DeletePattern()
{
    return this->self->DeletePattern();
}

void CMat_BlkCrs::FillPattern()
{
    this->self->FillPattern();
}

bool CMat_BlkCrs::AddPattern(DelFEM4NetCom::CIndexedArray^ crs)
{
    return this->self->AddPattern(*(crs->Self));
}

bool CMat_BlkCrs::AddPattern(CMat_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->AddPattern(*(rhs->self), isnt_trans);
}

bool CMat_BlkCrs::AddPattern(CMat_BlkCrs^ rhs, 
                        COrdering_Blk^ order_col, COrdering_Blk^ order_row)
{
    return this->self->AddPattern(*(rhs->self), *(order_col->Self), *(order_row->Self));
}

bool CMat_BlkCrs::SetPatternBoundary(CMat_BlkCrs^ rhs, CBCFlag^ bc_flag_col, CBCFlag^ bc_flag_row)
{
    return this->self->SetPatternBoundary(*(rhs->self), *(bc_flag_col->Self), *(bc_flag_row->Self));
}

bool CMat_BlkCrs::SetPatternDia(CMat_BlkCrs^ rhs)
{
    return this->self->SetPatternDia(*(rhs->self));
}

bool CMat_BlkCrs::SetValue(CMat_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->SetValue(*(rhs->self), isnt_trans);
}
bool CMat_BlkCrs::SetValue(CMat_BlkCrs^ rhs, 
                      COrdering_Blk^ order_col, COrdering_Blk^ order_row)
{
    return this->self->SetValue(*(rhs->self), *(order_col->Self), *(order_row->Self));
}

bool CMat_BlkCrs::Mearge
    (unsigned int nblkel_col, array<unsigned int>^ blkel_col,
     unsigned int nblkel_row, array<unsigned int>^ blkel_row,
     unsigned int blksize, array<double>^ emat)
{
    pin_ptr<unsigned int> blkel_col_= &blkel_col[0];
    pin_ptr<unsigned int> blkel_row_ = &blkel_row[0];
    pin_ptr<double> emat_ = &emat[0];
    return this->self->Mearge(nblkel_col, (unsigned int*)blkel_col_,
        nblkel_row, (unsigned int *)blkel_row_,
        blksize, (double *)emat_);
}

void CMat_BlkCrs::DeleteMargeTmpBuffer()
{
    this->self->DeleteMargeTmpBuffer();
}

bool CMat_BlkCrs::MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% b, bool isnt_trans)  // IN/OUT: b
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    //MatVec::CVector_Blk *b_ = new MatVec::CVector_Blk(*b->Self); // コピーを取る
    //b = gcnew CVector_Blk(b_);
    
    bool ret = this->self->MatVec(alpha, *(x->Self), beta, *(b->Self), isnt_trans);
    
    return ret;
}

bool CMat_BlkCrs::SetBoundaryCondition_Row(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition_Row(*(bc_flag->Self));
}

bool CMat_BlkCrs::SetBoundaryCondition_Colum(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition_Colum(*(bc_flag->Self));
}

bool CMat_BlkCrs::SetBoundaryConditionInverse_Colum(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryConditionInverse_Colum(*(bc_flag->Self));
}

////////////////////////////////////////////////
// Crs Original Function ( Non-virtual )

unsigned int CMat_BlkCrs::NCrs()
{
    return this->self->NCrs();
}

DelFEM4NetCom::ConstUIntArrayIndexer^ CMat_BlkCrs::GetPtrIndPSuP(unsigned int ipoin, [Out]unsigned int% npsup)
{
    unsigned int npsup_;
    const unsigned int * ptr_ = this->self->GetPtrIndPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return gcnew DelFEM4NetCom::ConstUIntArrayIndexer(npsup_, ptr_);
}

DelFEM4NetCom::DoubleArrayIndexer^ CMat_BlkCrs::GetPtrValPSuP(unsigned int ipoin, [Out]unsigned int% npsup)
{
    unsigned int npsup_;
    double * ptr_ = this->self->GetPtrValPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return gcnew DelFEM4NetCom::DoubleArrayIndexer(npsup_, ptr_);
}
    
/* nativeなポインタは返却不可
const unsigned int* CMat_BlkCrs::GetPtrIndPSuP(const unsigned int ipoin, [Out] unsigned int% npsup)
{
    unsigned int npsup_;
    const unsigned int * rowPtr_Blk = this->self->GetPtrIndPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return rowPtr_Blk;
}
const double* CMat_BlkCrs::GetPtrValPSuP(const unsigned int ipoin, [Out] unsigned int% npsup)
{
    unsigned int npsup_;
    const double * valCrs_Blk = this->self->GetPtrValPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return valCrs_Blk;
}
double* CMat_BlkCrs::GetPtrValPSuP(const unsigned int ipoin, [Out] unsigned int% npsup)
{
    unsigned int npsup_;
    double * valCrs_Blk = this->self->GetPtrValPSuP(ipoin, npsup_);
    npsup = npsup_;
    
    return valCrs_Blk;
}
*/


