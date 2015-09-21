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


#include "DelFEM4Net/matvec/bcflag_blk.h"

using namespace DelFEM4NetMatVec;

//----------------------------------------------------------------------
// CBCFlag
//----------------------------------------------------------------------
MatVec::CBCFlag * CBCFlag::CreateNativeInstance(const MatVec::CBCFlag& rhs)
{
    MatVec::CBCFlag * ptr = NULL;

    int len = rhs.LenBlk();
    unsigned int nblk = rhs.NBlk();


    if (len != -1)
    {
        ptr = new MatVec::CBCFlag(nblk, len);
    }
    else
    {
        std::vector<unsigned int> alen;
        for (unsigned int iblk = 0; iblk < nblk; iblk++)
        {
            unsigned int nlen_blk = rhs.LenBlk(iblk);
            alen.push_back(nlen_blk);
        }
        ptr = new MatVec::CBCFlag(nblk, alen);
    }

    ptr->SetAllFlagZero(); // フラグをすべてOFFにする
    // ONの箇所だけ設定
    for (unsigned int iblk = 0; iblk < nblk; iblk++)
    {
        unsigned int nlen_blk = rhs.LenBlk(iblk);
        for (unsigned int idofblk = 0; idofblk < nlen_blk; idofblk++)
        {
            int bc_flag = rhs.GetBCFlag(iblk, idofblk);
            if (bc_flag != 0)
            {
                // len != -1の場合の処理しかない?
                assert(nlen_blk == len);
                ptr->SetBC(iblk, idofblk); // ONにする
            }
        }
    }

    return ptr;
}

/*
CBCFlag::CBCFlag()
{
}
*/

CBCFlag::CBCFlag(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CBCFlag::CBCFlag(const CBCFlag% rhs)
{
    MatVec::CBCFlag rhs_instance_ = *(rhs.self);
    // shallow copyとなる
    //this->self = new MatVec::CBCFlag(rhs_instance_);
    // 修正版
    this->self = CBCFlag::CreateNativeInstance(rhs_instance_);
}

CBCFlag::CBCFlag(const MatVec::CBCFlag& native_instance)
{
    this->self = CBCFlag::CreateNativeInstance(native_instance);
}

CBCFlag::CBCFlag(MatVec::CBCFlag *self)
{
    this->self = self;
}

CBCFlag::CBCFlag(unsigned int nBlk, unsigned int lenBlk)
{
    this->self = new MatVec::CBCFlag(nBlk, lenBlk);
}

CBCFlag::CBCFlag(unsigned int nBlk, IList<unsigned int>^ aLen)
{
    std::vector<unsigned int> aLen_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aLen, aLen_);

    this->self = new MatVec::CBCFlag(nBlk, aLen_);
}

CBCFlag::~CBCFlag()
{
    this->!CBCFlag();
}

CBCFlag::!CBCFlag()
{
    delete this->self;
}

MatVec::CBCFlag * CBCFlag::Self::get() { return this->self; }

int CBCFlag::LenBlk()
{
    return this->self->LenBlk();
}

unsigned int CBCFlag::LenBlk(unsigned int iblk)
{
    return this->self->LenBlk(iblk);
}

unsigned int CBCFlag::NBlk()
{
    return this->self->NBlk();
}

int CBCFlag::GetBCFlag(unsigned int iblk, unsigned int idofblk)
{
    return this->self->GetBCFlag(iblk, idofblk);
}

void CBCFlag::SetAllFlagZero()
{
    this->self->SetAllFlagZero();
}

void CBCFlag::SetZeroToBCDof(CVector_Blk^ vec)   // [IN/OUT] vec
{
    this->self->SetZeroToBCDof(*(vec->Self));
}

void CBCFlag::SetZeroToBCDof(DelFEM4NetMatVec::CZVector_Blk^ vec) // [IN/OUT] vec
{
    this->self->SetZeroToBCDof(*(vec->Self));
}

// iblk番目のブロックのidofblk番目の自由度を固定する
bool CBCFlag::SetBC(unsigned int iBlk, unsigned int iDofBlk)
{
    return this->self->SetBC(iBlk, iDofBlk);
}
