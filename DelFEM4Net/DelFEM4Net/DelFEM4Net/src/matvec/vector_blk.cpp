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


#include "DelFEM4Net/matvec/vector_blk.h"

using namespace DelFEM4NetMatVec;

double CVector_Blk::operator*(CVector_Blk^ lhs, CVector_Blk^ rhs)
{
    return MatVec::operator*(*(lhs->self), *(rhs->self));
}

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
MatVec::CVector_Blk * CVector_Blk::CreateNativeInstance(const MatVec::CVector_Blk& rhs)
{
    // !!!! コピーコンストラクタはrhs.Len() == -1 のとき失敗する。
    //  operator=はある。これを使用してもいい?
    //
    MatVec::CVector_Blk *ptr = NULL; 
    unsigned int nblk = rhs.NBlk();
    int len = rhs.Len();
    if (len == -1)
    {
        std::vector<unsigned int> aLen;
        for (int iblk = 0; iblk < nblk; iblk++)
        {
            unsigned int len_blk = rhs.Len(iblk);
            aLen.push_back(len_blk);
        }
        ptr = new MatVec::CVector_Blk(nblk, aLen);
        ptr->operator = (rhs); 
    }
    else
    {
        ptr = new MatVec::CVector_Blk(rhs);
    }
    return ptr;
}

CVector_Blk::CVector_Blk()
{
    this->self = new MatVec::CVector_Blk();
}

CVector_Blk::CVector_Blk(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CVector_Blk::CVector_Blk(const CVector_Blk% rhs)
{
    const MatVec::CVector_Blk& rhs_instance_ = *(rhs.self);
    // !!!! コピーコンストラクタはrhs.Len() == -1 のとき失敗する。
    //this->self = new MatVec::CVector_Blk(rhs_instance_);
    this->self = CreateNativeInstance(rhs_instance_);
}

CVector_Blk::CVector_Blk(MatVec::CVector_Blk *self)
{
    this->self = self;
}

/*!
@brief コンストラクタ
@param[in] iblkveclen ブロックの数
@param[in] iblklen １つのブロックのサイズ
*/
CVector_Blk::CVector_Blk(unsigned int nblk, unsigned int len)
{
    this->self = new MatVec::CVector_Blk(nblk, len);
}

CVector_Blk::CVector_Blk(unsigned int nblk, List<unsigned int>^ aLen)
{
    std::vector<unsigned int> aLen_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aLen, aLen_);

    this->self = new MatVec::CVector_Blk(nblk, aLen_);
}


CVector_Blk::~CVector_Blk()
{
    this->!CVector_Blk();
}

CVector_Blk::!CVector_Blk()
{
    delete this->self;
}

MatVec::CVector_Blk * CVector_Blk::Self::get() { return this->self; }

bool CVector_Blk::Initialize(unsigned int nblk, unsigned int len )
{
    return this->self->Initialize(nblk, len);
}
  
bool CVector_Blk::Initialize(unsigned int nblk, List<unsigned int>^ alen)
{
    std::vector<unsigned int> alen_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(alen, alen_);
    
    return this->self->Initialize(nblk, alen_);
}

// @{
CVector_Blk^ CVector_Blk::operator*=(double d0)    //!< Scaler Product
{
    MatVec::CVector_Blk &ret = this->self->operator*=(d0);
    
    return this;
}

CVector_Blk^ CVector_Blk::operator=(CVector_Blk^ rhs)    //!< Substitue Vector
{
    MatVec::CVector_Blk &ret = this->self->operator=(*(rhs->self));

    return this;
}

CVector_Blk^ CVector_Blk::operator+=(CVector_Blk^ rhs) //!< Add 
{
    MatVec::CVector_Blk &ret = this->self->operator+=(*(rhs->self));
    
    return this;
}
// @}

////////////////////////////////
// Menber function

/*
@brief ベクトルのスカラー倍の足し算
{this} += alhpa*{rhs}
*/
CVector_Blk^ CVector_Blk::AXPY(double alpha, CVector_Blk^ rhs)
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->AXPY(alpha, *(rhs->self));
    MatVec::CVector_Blk *ret = new MatVec::CVector_Blk(ret_instance_);
    
    CVector_Blk^ retManaged = gcnew CVector_Blk(ret);
    
    return retManaged;
}

void CVector_Blk::SetVectorZero()   //!< Set 0 to Value
{
    this->self->SetVectorZero();
}

double CVector_Blk::GetSquaredVectorNorm()    //!< ベクトルの２乗ノルムの２乗を計算する
{
    return this->self->GetSquaredVectorNorm();
}

unsigned int CVector_Blk::NBlk()
{
    return this->self->NBlk();
}    //!< Size of Bocks Vector have
int CVector_Blk::Len()
{
    return this->self->Len();
}    //!< Size of One Block(もしFlexサイズなら-1を返す)

//! Size of One Block
unsigned int CVector_Blk::Len(unsigned int iblk)
{
    return this->self->Len(iblk);
}  
  
/*!
@brief 値を取得
@param [in] iblk ブロックのインデックス
@param [in] idofblk ブロックの中の自由度番号
@retval 値
*/
double CVector_Blk::GetValue(unsigned int iblk, unsigned int idofblk)
{
    return this->self->GetValue(iblk, idofblk);
}

/*!
@brief 値をセット
@param [in] iblk ブロックのインデックス
@param [in] idofblk ブロックの中の自由度番号
@param [in] val 値
*/
void CVector_Blk::SetValue(unsigned int iblk, unsigned int idofblk, double val)
{
    this->self->SetValue(iblk, idofblk, val);
}

/*!
@brief 特定の要素に値を足し合わせる
@param [in] iblk ブロックのインデックス
@param [in] idofblk ブロックの中の自由度番号
@param [in] val 足し合わせる値
*/
void CVector_Blk::AddValue(unsigned int iblk, unsigned int idofblk, double val)
{
    this->self->AddValue(iblk, idofblk, val);
}

/* アセンブリの外へはポインタを直接返却できない
double* CVector_Blk::GetValuePtr(unsigned int iblk)
{
    double *ptr = this->self->GetValuePtr(iblk);
    
    retun ptr;
}
*/
// ブロックの先頭のポインタをインデクサでラップして返却する
//   C#側では
//     DoubleArrayIndexer ptr = vectorBlk.GetValuePtr(iblk);
//     double value = ptr[idofblk];
//     ptr[idofblk] = 0;
//   のようにアクセス可能
DelFEM4NetCom::DoubleArrayIndexer^ CVector_Blk::GetValuePtr(unsigned int iblk)
{
    unsigned int len = this->self->Len(iblk);
    double *ptr = this->self->GetValuePtr(iblk);
    
    return gcnew DelFEM4NetCom::DoubleArrayIndexer((int)len, ptr);
}

