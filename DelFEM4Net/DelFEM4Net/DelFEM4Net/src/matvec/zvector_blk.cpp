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


#include "DelFEM4Net/matvec/zvector_blk.h"

using namespace DelFEM4NetMatVec;

DelFEM4NetCom::Complex^ CZVector_Blk::operator*(CZVector_Blk^ lhs, CZVector_Blk^ rhs)
{
    const Com::Complex& c_instance_ = MatVec::operator*(*(lhs->self), *(rhs->self));
    
    Com::Complex *c_ = new Com::Complex(c_instance_);
    return gcnew DelFEM4NetCom::Complex(c_);
}

DelFEM4NetCom::Complex^ CZVector_Blk::InnerProduct(CZVector_Blk^ lhs, CZVector_Blk^ rhs)
{
    const Com::Complex& c_instance_ = MatVec::InnerProduct(*(lhs->self), *(rhs->self));
    
    Com::Complex *c_ = new Com::Complex(c_instance_);
    return gcnew DelFEM4NetCom::Complex(c_);
}

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CZVector_Blk * CZVector_Blk::CreateNativeInstance(const MatVec::CZVector_Blk& rhs)
{
    // !!!! コピーコンストラクタはないが、operator=はある。これを使用してもいい?
    // !!!! デフォルトコンストラクタはない
    unsigned int iblklen = rhs.BlkLen();
    unsigned int iblkveclen = rhs.BlkVecLen();
    
    MatVec::CZVector_Blk *ptr = NULL;
    
    ptr = new MatVec::CZVector_Blk(iblkveclen, iblklen);
    
    ptr->operator = (rhs);
    
    return ptr;
}

/*デフォルトコンストラクタなし
CZVector_Blk::CZVector_Blk()
{
    this->self = new MatVec::CZVector_Blk();
}
*/

CZVector_Blk::CZVector_Blk(bool isCreateInstance)  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
{
    assert(isCreateInstance == false);
    
    this->self = NULL;
}

CZVector_Blk::CZVector_Blk(const CZVector_Blk% rhs)
{
    MatVec::CZVector_Blk rhs_instance_ = *(rhs.self);
    //shallow copyになる
    //this->self = new MatVec::CZVector_Blk(rhs_instance_);
    // 修正版
    this->self = CZVector_Blk::CreateNativeInstance(rhs_instance_);
}

CZVector_Blk::CZVector_Blk(const MatVec::CZVector_Blk& native_instance) // nativeインスタンスから生成
{
    this->self = CZVector_Blk::CreateNativeInstance(native_instance);
}

// nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
CZVector_Blk::CZVector_Blk(MatVec::CZVector_Blk *self)
{
    this->self = self;
}

CZVector_Blk::CZVector_Blk(unsigned int iblkveclen, unsigned int iblklen)
{
    this->self = new MatVec::CZVector_Blk(iblkveclen, iblklen);
}

CZVector_Blk::~CZVector_Blk()
{
    this->!CZVector_Blk();
}

CZVector_Blk::!CZVector_Blk()
{
    delete this->self;
}

MatVec::CZVector_Blk * CZVector_Blk::Self::get() { return this->self; }

CZVector_Blk^ CZVector_Blk::operator*=(double d0)    // Scaler Product
{
   MatVec::CZVector_Blk& ret = this->self->operator*=(d0);
   
   return this;
}

CZVector_Blk^ CZVector_Blk::operator*=(DelFEM4NetCom::Complex^ c0)    // Scaler Product
{
   Com::Complex *c0_ = c0->Self;
   MatVec::CZVector_Blk& ret = this->self->operator*=(*c0_);
   
   return this;
}

CZVector_Blk^ CZVector_Blk::operator=(CZVector_Blk^ rhs) // Substitue Vector
{
   MatVec::CZVector_Blk& ret = this->self->operator=(*(rhs->self));
   
   return this;
}

CZVector_Blk^ CZVector_Blk::operator+=(CZVector_Blk^ rhs) // Add 
{
   MatVec::CZVector_Blk& ret = this->self->operator+=(*(rhs->self));
   
   return this;
}

////////////////////////////////
// Menber function

CZVector_Blk^ CZVector_Blk::AXPY(double alpha, CZVector_Blk^ rhs) // Add scaler scaled Vector
{
    // 戻り値は自分自身
    MatVec::CZVector_Blk& ret_instance_ = this->self->AXPY(alpha, *(rhs->self));

    return this;
}

CZVector_Blk^ CZVector_Blk::AXPY(DelFEM4NetCom::Complex^ alpha, CZVector_Blk^ rhs) // Add scaler scaled Vector
{
    // 戻り値は自分自身
    MatVec::CZVector_Blk& ret_instance_ = this->self->AXPY(*(alpha->Self), *(rhs->self));

    return this;
}

void CZVector_Blk::SetVectorZero()    // Set 0 to Value
{
    this->self->SetVectorZero();
}

double CZVector_Blk::GetSquaredVectorNorm()    // return norm of this vector
{
    return this->self->GetSquaredVectorNorm();
}

void CZVector_Blk::SetVectorConjugate()
{
    this->self->SetVectorConjugate();
}

unsigned int CZVector_Blk::BlkLen()    // Size of One Block
{
    return this->self->BlkLen();
}

unsigned int CZVector_Blk::BlkVecLen()    // Size of Bocks Vector have
{
    return this->self->BlkVecLen();
}

DelFEM4NetCom::Complex^ CZVector_Blk::GetValue(unsigned int iblk, unsigned int idofblk)
{
   Com::Complex *ret = new Com::Complex();
   
    *ret = this->self->GetValue(iblk, idofblk);
    
    DelFEM4NetCom::Complex^ retManaged = gcnew DelFEM4NetCom::Complex(ret);
    
    return retManaged;
}

void CZVector_Blk::SetValue(unsigned int iblk, unsigned int idofblk, DelFEM4NetCom::Complex^ val)
{
    this->self->SetValue(iblk, idofblk, *(val->Self));
}

void CZVector_Blk::AddValue(unsigned int iblk, unsigned int idofblk, DelFEM4NetCom::Complex^ val)
{
    this->self->AddValue(iblk, idofblk, *(val->Self));
}



