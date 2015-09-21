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
// MatDia_Crs.cpp: Implementation of class "CMatDia_BlkCrs"
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning( disable : 4786 ) 
#endif
#define for if(0); else for

//#include <iostream>
//#include <cassert>
//#include <math.h>
#include <vector>
//#include <algorithm>

#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
//#include "DelFEM4Net/matvec/matfrac_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/ordering_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

using namespace DelFEM4NetMatVec;

//////////////////////////////////////////////////////////////////////
// CMatDia_BlkCrs
//////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CMatDia_BlkCrs * CMatDia_BlkCrs::CreateNativeInstance(const MatVec::CMatDia_BlkCrs& rhs)
{
    MatVec::CMatDia_BlkCrs * ptr = NULL;
    
    unsigned int nblk_col = rhs.NBlkMatCol();
    unsigned int nblk_row = rhs.NBlkMatRow();
    unsigned int len_col = rhs.LenBlkCol();
    unsigned int len_row = rhs.LenBlkRow();
    assert(nblk_col == nblk_row && len_col == len_row);

    //NG
    //unsigned int nblk_col_row = nblk_row;
    //unsigned int len_col_row = len_row;
    //ptr = new MatVec::CMatDia_BlkCrs(nblk_col_row, len_col_row);
    // ptr->SetValue(rhs, true);

    std::vector<unsigned int> alen_col;
    std::vector<unsigned int> alen_row;
    for (int iblk = 0; iblk < nblk_col; iblk++)
    {
        unsigned int lencol_rhs = rhs.LenBlkCol(iblk);
        alen_col.push_back(lencol_rhs);
        unsigned int lenrow_rhs = rhs.LenBlkRow(iblk);
        alen_row.push_back(lenrow_rhs);
    }
    ptr = new MatVec::CMatDia_BlkCrs();
    ptr->Initialize(nblk_col, alen_col, nblk_row, alen_row);
    assert(nblk_col == ptr->NBlkMatCol());
    assert(nblk_row == ptr->NBlkMatRow());
    ptr->AddPattern(rhs, true);
    assert(nblk_col == ptr->NBlkMatCol());
    assert(nblk_row == ptr->NBlkMatRow());
    ptr->SetValue(rhs, true);
    assert(nblk_col == ptr->NBlkMatCol());
    assert(nblk_row == ptr->NBlkMatRow());
    return ptr;
}


CMatDia_BlkCrs::CMatDia_BlkCrs() : CMat_BlkCrs(false)  // 基本クラスではnativeインスタンスを生成しない
{
    assert(CMat_BlkCrs::self == NULL);
    this->self = new MatVec::CMatDia_BlkCrs();
    
    setBaseSelf();
}

CMatDia_BlkCrs::CMatDia_BlkCrs(bool isCreateInstance) : CMat_BlkCrs(false)  // 基本クラスではnativeインスタンスを生成しない
{
    assert(isCreateInstance == false);
    assert(CMat_BlkCrs::self == NULL);

    // インスタンスを生成しない
    this->self = NULL;

    setBaseSelf();
}


CMatDia_BlkCrs::CMatDia_BlkCrs(const CMatDia_BlkCrs% rhs) : CMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CMat_BlkCrs::self == NULL);
    const MatVec::CMatDia_BlkCrs& rhs_instance_ = *(rhs.self);
    // shallow copyとなる
    //this->self = new MatVec::CMatDia_BlkCrs(rhs_instance_);
    // 修正版
    this->self = CMatDia_BlkCrs::CreateNativeInstance(rhs_instance_);

    // BUGFIX ベースクラスのnativeインスタンスポインタを設定する
    setBaseSelf();
}

CMatDia_BlkCrs::CMatDia_BlkCrs(const MatVec::CMatDia_BlkCrs& native_instance)
{
    this->self = CMatDia_BlkCrs::CreateNativeInstance(native_instance);

    // BUGFIX ベースクラスのnativeインスタンスポインタを設定する
    setBaseSelf();
}

CMatDia_BlkCrs::CMatDia_BlkCrs(MatVec::CMatDia_BlkCrs *self) : CMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CMat_BlkCrs::self == NULL);

    this->self = self;

    setBaseSelf();
}

CMatDia_BlkCrs::CMatDia_BlkCrs(unsigned int nblk_colrow, unsigned int len_colrow) : CMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CMat_BlkCrs::self == NULL);

    this->self = new MatVec::CMatDia_BlkCrs(nblk_colrow, len_colrow);

    setBaseSelf();
}

CMatDia_BlkCrs::~CMatDia_BlkCrs()
{
    this->!CMatDia_BlkCrs();
}

CMatDia_BlkCrs::!CMatDia_BlkCrs()
{
    delete this->self;
    
    this->self = NULL;
    setBaseSelf();
}

MatVec::CMatDia_BlkCrs * CMatDia_BlkCrs::Self::get() { return this->self; }

bool CMatDia_BlkCrs::Initialize(unsigned int nblk_col, IList<unsigned int>^ alen_col, 
                                unsigned int nblk_row, IList<unsigned int>^ alen_row )
{
    std::vector<unsigned int> alen_col_;
    std::vector<unsigned int> alen_row_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_col, alen_col_);
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen_row, alen_row_);
    
    return this->self->Initialize(nblk_col, alen_col_,
                                  nblk_row, alen_row_); 
}

bool CMatDia_BlkCrs::Initialize(unsigned int nblk_col, unsigned int len_col,
                                unsigned int nblk_row, unsigned int len_row )
{
    return this->self->Initialize(nblk_col, len_col, nblk_row, len_row);
}

bool CMatDia_BlkCrs::DeletePattern()
{
    return this->self->DeletePattern();
}

bool CMatDia_BlkCrs::AddPattern(DelFEM4NetCom::CIndexedArray^ crs)
{
    return this->self->AddPattern(*(crs->Self));
}

bool CMatDia_BlkCrs::AddPattern(CMatDia_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->AddPattern(*(rhs->self), isnt_trans);
}

bool CMatDia_BlkCrs::AddPattern(CMatDia_BlkCrs^ rhs, COrdering_Blk^ order)
{
    return this->self->AddPattern(*(rhs->self), *(order->Self));
}

bool CMatDia_BlkCrs::AddPattern(CMat_BlkCrs^ m1, CMatDia_BlkCrs^ m2, CMat_BlkCrs^ m3)
{
    return this->self->AddPattern(*(m1->Self), *(m2->self), *(m3->Self));
}

bool CMatDia_BlkCrs::SetValue(CMatDia_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->SetValue(*(rhs->self), isnt_trans);
}

bool CMatDia_BlkCrs::SetValue(CMat_BlkCrs^ m1, CMatDia_BlkCrs^ m2, CMat_BlkCrs^ m3)
{
    return this->self->SetValue(*(m1->Self), *(m2->self), *(m3->Self));
}

bool CMatDia_BlkCrs::SetValue(CMatDia_BlkCrs^ rhs, COrdering_Blk^ order)
{
    return this->self->SetValue(*(rhs->self), *(order->Self));
}

bool CMatDia_BlkCrs::SetZero()
{
    return this->self->SetZero();
}

bool CMatDia_BlkCrs::Mearge(
    unsigned int nblkel_col, array<unsigned int>^ blkel_col,
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

bool CMatDia_BlkCrs::MatVec(double alpha, CVector_Blk^ x, double beta, CVector_Blk^% y)  // [IN/OUT] y
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    //MatVec::CVector_Blk *y_ = new MatVec::CVector_Blk(*y->Self); // コピーを取る
    //y = gcnew CVector_Blk(y_);
    
    bool ret = this->self->MatVec(alpha, *(x->Self), beta, *(y->Self));

    return ret;
}

bool CMatDia_BlkCrs::SetBoundaryCondition(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition(*(bc_flag->Self));
}

////////////////////////////////////////////////////////////////
// 前処理用のクラス

DelFEM4NetCom::DoubleArrayIndexer^ CMatDia_BlkCrs::GetPtrValDia(unsigned int ipoin)
{
    double *ptr = this->self->GetPtrValDia(ipoin);
    unsigned int blksize = this->self->LenBlkCol() * this->self->LenBlkRow();
    
    return gcnew DelFEM4NetCom::DoubleArrayIndexer(blksize, ptr);
}


void CMatDia_BlkCrs::setBaseSelf()
{
    CMat_BlkCrs::self = this->self;
}
