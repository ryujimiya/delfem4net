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
#pragma warning( disable : 4786 )   // C4786なんて表示すんな( ﾟДﾟ)ｺﾞﾙｧ
#endif
#define for if(0); else for


#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

#include "DelFEM4Net/ls/linearsystem.h"

using namespace DelFEM4NetLsSol;

CLinearSystem::CLinearSystem()
{
    this->self = new LsSol::CLinearSystem();
}

CLinearSystem::CLinearSystem(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを作成しない
    this->self = NULL;
}

CLinearSystem::CLinearSystem(const CLinearSystem% rhs)
{
    const LsSol::CLinearSystem& rhs_instance_ = *(rhs.self);
    // shallow copyになる
    //this->self = new LsSol::CLinearSystem(rhs_instance_);
    assert(false);
}

CLinearSystem::CLinearSystem(LsSol::CLinearSystem *self)
{
    this->self = self;
}

CLinearSystem::~CLinearSystem()
{
    this->!CLinearSystem();
}

CLinearSystem::!CLinearSystem()
{
    delete this->self;
}

LsSol::CLinearSystem * CLinearSystem::Self::get() { return this->self; }

LsSol::ILinearSystem_Sol * CLinearSystem::SolSelf::get() { return this->self; }

////////////////
void CLinearSystem::Clear()
{
    this->self->Clear();
}

void CLinearSystem::ClearFixedBoundaryCondition()
{
    this->self->ClearFixedBoundaryCondition();
}

////////////////////////////////
// function for marge
void CLinearSystem::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CLinearSystem::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

////////////////////////////////
// function for linear solver

unsigned int CLinearSystem::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CLinearSystem::ReSizeTmpVecSolver(unsigned int size_new)
{
    return this->self->ReSizeTmpVecSolver(size_new);
}

double CLinearSystem::DOT(int iv1, int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystem::COPY(int iv1, int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystem::SCAL(double alpha, int iv1)
{
    return this->self->SCAL(alpha, iv1);
}

bool CLinearSystem::AXPY(double alpha, int iv1, int iv2)
{
    return this->self->AXPY(alpha, iv1, iv2);
}
bool CLinearSystem::MATVEC(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MATVEC(alpha, iv1, beta, iv2);
}

////////////////////////////////
// function for preconditioner

int CLinearSystem::AddLinSysSeg( unsigned int nnode, unsigned int len )
{
    return this->self->AddLinSysSeg(nnode, len);
}

int CLinearSystem::AddLinSysSeg( unsigned int nnode, IList<unsigned int>^ aLen )
{
    std::vector<unsigned int> aLen_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aLen, aLen_);
    
    return this->self->AddLinSysSeg(nnode, aLen_);
}
unsigned int CLinearSystem::GetNLinSysSeg()
{
    return this->self->GetNLinSysSeg();
}

unsigned int CLinearSystem::GetBlkSizeLinSeg(unsigned int ilss)
{
    return this->self->GetBlkSizeLinSeg(ilss);
}

bool CLinearSystem::IsMatrix(unsigned int ilss, unsigned int jlss)
{
    return this->self->IsMatrix(ilss, jlss);
}

// 読み取り用
DelFEM4NetMatVec::CMatDia_BlkCrs^ CLinearSystem::GetMatrix(unsigned int ilss)
{
    const MatVec::CMatDia_BlkCrs& ret_instance_ = this->self->GetMatrix(ilss);
    // shallow copyなのでNG
    //MatVec::CMatDia_BlkCrs * ret = new MatVec::CMatDia_BlkCrs(ret_instance_);
    //DelFEM4NetMatVec::CMatDia_BlkCrs^ retManaged = gcnew DelFEM4NetMatVec::CMatDia_BlkCrs(ret);
    //return retManaged;
    // nativeインスタンスから生成
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs(ret_instance_);
}

// 読み取り用
DelFEM4NetMatVec::CMat_BlkCrs^ CLinearSystem::GetMatrix(unsigned int ilss, unsigned int jlss)
{
    const MatVec::CMat_BlkCrs& ret_instance_ = this->self->GetMatrix(ilss, jlss);
    // shallow copy
    //MatVec::CMat_BlkCrs * ret = new MatVec::CMat_BlkCrs(ret_instance_);    
    //DelFEM4NetMatVec::CMat_BlkCrs^ retManaged = gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret);
    //return retManaged;
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret_instance_);
}

// 読み取り用
DelFEM4NetMatVec::CVector_Blk^ CLinearSystem::GetVector(int iv, unsigned int ilss)
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetVector(iv, ilss);
    MatVec::CVector_Blk * ret = new MatVec::CVector_Blk(ret_instance_);
    
    DelFEM4NetMatVec::CVector_Blk^ retManaged = gcnew DelFEM4NetMatVec::CVector_Blk(ret);
    return retManaged;
}

// 読み取り用
DelFEM4NetMatVec::CBCFlag^ CLinearSystem::GetBCFlag(unsigned int ilss)
{
    const MatVec::CBCFlag& ret_instance_ = this->self->GetBCFlag(ilss);
    //shallow copy
    //MatVec::CBCFlag * ret = new MatVec::CBCFlag(ret_instance_);
    //DelFEM4NetMatVec::CBCFlag^ retManaged = gcnew DelFEM4NetMatVec::CBCFlag(ret);
    //return retManaged;
    return gcnew DelFEM4NetMatVec::CBCFlag(ret_instance_);
}

// 更新用
DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ CLinearSystem::GetMatrixPtr(unsigned int ilss)
{
    MatVec::CMatDia_BlkCrs& ret_instance_ref_ = this->self->GetMatrix(ilss);
    MatVec::CMatDia_BlkCrs* ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr(ret_instance_ptr_);
}

// 更新用
DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ CLinearSystem::GetMatrixPtr(unsigned int ilss, unsigned int jlss)
{
    MatVec::CMat_BlkCrs& ret_instance_ref_ = this->self->GetMatrix(ilss, jlss);
    MatVec::CMat_BlkCrs* ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs_Ptr(ret_instance_ptr_);
}

// 更新用
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem::GetVectorPtr(int iv, unsigned int ilss)
{
    MatVec::CVector_Blk& ret_instance_ref_ = this->self->GetVector(iv, ilss);
    MatVec::CVector_Blk * ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ret_instance_ptr_);
}

// 更新用
DelFEM4NetMatVec::CBCFlag_Ptr^ CLinearSystem::GetBCFlagPtr(unsigned int ilss)
{
    MatVec::CBCFlag& ret_instance_ref_ = this->self->GetBCFlag(ilss);
    MatVec::CBCFlag* ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew DelFEM4NetMatVec::CBCFlag_Ptr(ret_instance_ptr_);
}

bool CLinearSystem::AddMat_NonDia(unsigned int ils_col, unsigned int ils_row, DelFEM4NetCom::CIndexedArray^ crs )
{
    return this->self->AddMat_NonDia(ils_col, ils_row, *(crs->Self));
}

bool CLinearSystem::AddMat_Dia(unsigned int ils, DelFEM4NetCom::CIndexedArray^ crs )
{
    return this->self->AddMat_Dia(ils, *(crs->Self));
}

// メンバ変数のポインタを取得する関数を用意
// 残差ベクトルをゲットする
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem::GetResidualPtr(unsigned int ilss) // GetVectorでiv : -1 を指定した場合に相当
{
    assert(ilss < this->self->m_Residual.size());
    MatVec::CVector_Blk *native_ptr = this->self->m_Residual[ilss];
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(native_ptr);
}

// 更新ベクトルをゲットする
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem::GetUpdatePtr(unsigned int ilss) // GetVectorでiv : -2 を指定した場合に相当
{
    assert(ilss < this->self->m_Update.size());
    MatVec::CVector_Blk *native_ptr = this->self->m_Update[ilss];
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(native_ptr);
}

// 対角行列をゲットする
DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ CLinearSystem::GetMatrixPtr_DirectAccess(unsigned int ilss)
{
    assert(ilss < this->self->m_Matrix_Dia.size());
    MatVec::CMatDia_BlkCrs *native_ptr = this->self->m_Matrix_Dia[ilss];
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr(native_ptr);
}

// 非対角行列をゲットする
DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ CLinearSystem::GetMatrixPtr_DirectAccess(unsigned int ilss, unsigned int jlss)
{
    assert(ilss < this->self->m_Matrix_NonDia.size());
    assert(jlss < this->self->m_Matrix_NonDia[ilss].size());
    MatVec::CMat_BlkCrs *native_ptr = this->self->m_Matrix_NonDia[ilss][jlss];
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs_Ptr(native_ptr);
}


