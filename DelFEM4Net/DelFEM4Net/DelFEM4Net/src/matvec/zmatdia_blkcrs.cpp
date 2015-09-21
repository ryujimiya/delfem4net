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

#include <iostream>
#include <cassert>
#include <math.h>
#include <vector>
#include <algorithm>
#include <stdio.h>

#include "DelFEM4Net/complex.h"
#include "DelFEM4Net/indexed_array.h"

#include "DelFEM4Net/matvec/zmatdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"

using namespace DelFEM4NetMatVec;

//////////////////////////////////////////////////////////////////////
// DelFEM4Net
//////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CZMatDia_BlkCrs * CZMatDia_BlkCrs::CreateNativeInstance(const MatVec::CZMatDia_BlkCrs& rhs)
{
    MatVec::CZMatDia_BlkCrs * ptr = NULL;
    
    unsigned int nblk_col = rhs.NBlkMatCol();
    unsigned int nblk_row = rhs.NBlkMatRow();
    unsigned int len_col = rhs.LenBlkCol();
    unsigned int len_row = rhs.LenBlkRow();
    assert(nblk_col == nblk_row && len_col == len_row);

    unsigned int nblk_col_row = nblk_row;
    unsigned int len_col_row = len_row;
    ptr = new MatVec::CZMatDia_BlkCrs(nblk_col_row, len_col_row);
    ptr->SetValue(rhs, true);
    
    return ptr;
}

/*デフォルトコンストラクタなし
CZMatDia_BlkCrs::CZMatDia_BlkCrs() : CZMat_BlkCrs(false)  // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);
    this->self = new MatVec::CZMatDia_BlkCrs();
    
    setBaseSelf();
}
*/


CZMatDia_BlkCrs::CZMatDia_BlkCrs(bool isCreateInstance) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(isCreateInstance == false);
    assert(CZMat_BlkCrs::self == NULL);

    // インスタンスを生成しない
    this->self = NULL;

    setBaseSelf();
}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(const CZMatDia_BlkCrs% rhs) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);
    const MatVec::CZMatDia_BlkCrs& rhs_instance_ = *(rhs.self);
    // shallow copyとなる
    //this->self = new MatVec::CZMatDia_BlkCrs(rhs_instance_);
    // 修正版
    this->self = CZMatDia_BlkCrs::CreateNativeInstance(rhs_instance_);

    setBaseSelf();
}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(const MatVec::CZMatDia_BlkCrs& native_instance)
{
    this->self = CZMatDia_BlkCrs::CreateNativeInstance(native_instance);

    setBaseSelf();
}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(MatVec::CZMatDia_BlkCrs *self) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);

    this->self = self;

    setBaseSelf();
}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(unsigned int nblk_colrow, unsigned int len_colrow) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);

    this->self = new MatVec::CZMatDia_BlkCrs(nblk_colrow, len_colrow);

    setBaseSelf();
}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(String^ file_path) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);

    const std::string& file_path_ = DelFEM4NetCom::ClrStub::StringToStd(file_path);
    this->self = new MatVec::CZMatDia_BlkCrs(file_path_);

    setBaseSelf();

}

CZMatDia_BlkCrs::CZMatDia_BlkCrs(const CZMatDia_BlkCrs% rhs, bool is_value, bool isnt_trans, bool isnt_conj) : CZMat_BlkCrs(false) // 基本クラスではnativeインスタンスを生成しない
{
    assert(CZMat_BlkCrs::self == NULL);

    this->self = new MatVec::CZMatDia_BlkCrs(*(rhs.self), is_value, isnt_trans, isnt_conj);

    setBaseSelf();
}


CZMatDia_BlkCrs::~CZMatDia_BlkCrs()
{
    this->!CZMatDia_BlkCrs();
}

CZMatDia_BlkCrs::!CZMatDia_BlkCrs()
{
    delete this->self;
    
    this->self = NULL;
    setBaseSelf();
}

MatVec::CZMatDia_BlkCrs * CZMatDia_BlkCrs::Self::get() { return this->self; }


bool CZMatDia_BlkCrs::DeletePattern()
{
    return this->self->DeletePattern();
}

bool CZMatDia_BlkCrs::AddPattern(DelFEM4NetCom::CIndexedArray^ crs)
{
    return this->self->AddPattern(*(crs->Self));
}

bool CZMatDia_BlkCrs::AddPattern(CZMatDia_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->AddPattern(*(rhs->self), isnt_trans);
}

bool CZMatDia_BlkCrs::AddPattern(CZMat_BlkCrs^ m1, CZMatDia_BlkCrs^ m2, CZMat_BlkCrs^ m3)
{
    return this->self->AddPattern(*(m1->Self), *(m2->self), *(m3->Self));
}


bool CZMatDia_BlkCrs::SetValue(CZMatDia_BlkCrs^ rhs, bool isnt_trans)
{
    return this->self->SetValue(*(rhs->self), isnt_trans);
}

bool CZMatDia_BlkCrs::SetValue(CZMat_BlkCrs^ m1, CZMatDia_BlkCrs^ m2, CZMat_BlkCrs^ m3)
{
    return this->self->SetValue(*(m1->Self), *(m2->self), *(m3->Self));
}

bool CZMatDia_BlkCrs::SetZero()
{
    return this->self->SetZero();
}

bool CZMatDia_BlkCrs::Mearge(
    unsigned int nblkel_col, array<unsigned int>^ blkel_col,
    unsigned int nblkel_row, array<unsigned int>^ blkel_row,
    unsigned int blksize, array<DelFEM4NetCom::Complex^>^ emat, array<int>^% tmp_buffer)   // IN/OUT tmp_buffer(新たにハンドルを生成しない)
{
    pin_ptr<unsigned int> blkel_col_= &blkel_col[0];
    pin_ptr<unsigned int> blkel_row_ = &blkel_row[0];
    Com::Complex *emat_ = new Com::Complex[emat->Length];
    for (int i = 0; i < emat->Length; i++)
    {
        emat_[i] = Com::Complex(emat[i]->Real, emat[i]->Imag);
    }
    // tmp_bufferは新たにハンドルを生成しない
    pin_ptr<int> tmp_buffer_ = &tmp_buffer[0];
    
    bool ret = this->self->Mearge(nblkel_col, (unsigned int*)blkel_col_,
        nblkel_row, (unsigned int *)blkel_row_,
        blksize, emat_, (int *)tmp_buffer_);
    // BUGFIX:削除忘れ
    delete[] emat_;
    return ret;
}

void CZMatDia_BlkCrs::AddUnitMatrix(DelFEM4NetCom::Complex^ epsilon)
{
    this->self->AddUnitMatrix(*(epsilon->Self));
}

bool CZMatDia_BlkCrs::SetBoundaryCondition(CBCFlag^ bc_flag)
{
    return this->self->SetBoundaryCondition(*(bc_flag->Self));
}

bool CZMatDia_BlkCrs::MatVec(double alpha, CZVector_Blk^ x, double beta, CZVector_Blk^% y)  // [IN/OUT] y
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    ////コピーコンストラクタ不備によりshallow copyとなる
    ////MatVec::CZVector_Blk *y_ = new MatVec::CZVector_Blk(*b->Self); // コピーを取る
    ////y = gcnew CZVector_Blk(y_);
    //const MatVec::CZVector_Blk& y_instance_ = *(y->Self);
    //y = gcnew CZVector_Blk(y_instance_);
    
    bool ret = this->self->MatVec(alpha, *(x->Self), beta, *(y->Self));

    return ret;
}

bool CZMatDia_BlkCrs::MatVec_Hermitian(double alpha, CZVector_Blk^ x, double beta, CZVector_Blk^% y)  // [IN/OUT] y
{
    //BUGFIX  コピー処理を削除
    // コピーを取るとnativeのインスタンスが新しくなるが、
    // LinearSystemの更新ベクトルを書き換えで使う場合、nativeを変更してほしくないので。
    ////コピーコンストラクタ不備によりshallow copyとなる
    ////MatVec::CZVector_Blk *y_ = new MatVec::CZVector_Blk(*b->Self); // コピーを取る
    ////y = gcnew CZVector_Blk(y_);
    //const MatVec::CZVector_Blk& y_instance_ = *(y->Self);
    //y = gcnew CZVector_Blk(y_instance_);
    
     bool ret = this->self->MatVec_Hermitian(alpha, *(x->Self), beta, *(y->Self));

    return ret;
}

DelFEM4NetCom::ComplexArrayIndexer^ CZMatDia_BlkCrs::GetPtrValDia(unsigned int ipoin)
{
    Com::Complex *ptr = this->self->GetPtrValDia(ipoin);
    unsigned int blksize = this->self->LenBlkCol() * this->self->LenBlkRow();
    
    return gcnew DelFEM4NetCom::ComplexArrayIndexer(blksize, ptr);
}


void CZMatDia_BlkCrs::setBaseSelf()
{
    CZMat_BlkCrs::self = this->self;
}

