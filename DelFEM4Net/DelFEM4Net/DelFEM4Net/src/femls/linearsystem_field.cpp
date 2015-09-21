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
// LinearSystem.cpp : 連立一次方程式クラス(CLinearSystem.h)の実装
////////////////////////////////////////////////////////////////


#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#ifndef for 
#define for if(0); else for
#endif

//#include <math.h>

#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/femls/linearsystem_field.h"

#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/vector3d.h"
//#include "DelFEM4Net/quaternion.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

//using namespace DelFEM4NetMatVec;
using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetFem::Field;

//////////////////////////////////////
void CLinearSystem_Field::BoundaryCondition
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,  
         DelFEM4NetMatVec::CBCFlag^% bc_flag, DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int ioffset)  // bc_flag: IN/OUT
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
 
// コピーできない(nativeクラスのコピーコンストラクタはshallow copyになるため問題あり
//    MatVec::CBCFlag *bc_flag_ = new MatVec::CBCFlag(*(bc_flag->Self)); // コピーを取る
//    bc_flag = gcnew DelFEM4NetMatVec::CBCFlag(bc_flag_);
    
    Fem::Ls::BoundaryCondition(id_field, elseg_type_, *(bc_flag->Self), *(world->Self), ioffset);
}

void CLinearSystem_Field::BoundaryCondition
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, unsigned int idofns, 
         DelFEM4NetMatVec::CBCFlag^ bc_flag, DelFEM4NetFem::Field::CFieldWorld^ world) // bc_flag: IN/OUT
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
    
// コピーできない(nativeクラスのコピーコンストラクタはshallow copyになるため問題あり
//    MatVec::CBCFlag *bc_flag_ = new MatVec::CBCFlag(*(bc_flag->Self)); // コピーを取る
//    DelFEM4NetMatVec::CBCFlag^ bc_flag = gcnew DelFEM4NetMatVec::CBCFlag(bc_flag_);
    
    Fem::Ls::BoundaryCondition(id_field, elseg_type_, idofns, *(bc_flag->Self), *(world->Self));
}


//////////////////////////////////////
CLinearSystem_Field::CLinearSystem_Field()
{
    this->self = new Fem::Ls::CLinearSystem_Field();
}

CLinearSystem_Field::CLinearSystem_Field(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    this->self = NULL;
}

CLinearSystem_Field::CLinearSystem_Field(const CLinearSystem_Field% rhs)
{
    const Fem::Ls::CLinearSystem_Field& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CLinearSystem_Field(rhs_instance_);
}

CLinearSystem_Field::CLinearSystem_Field(Fem::Ls::CLinearSystem_Field *self)
{
    this->self = self;
}

CLinearSystem_Field::~CLinearSystem_Field()
{
    this->!CLinearSystem_Field();
}

CLinearSystem_Field::!CLinearSystem_Field()
{
    delete this->self;
}

Fem::Ls::CLinearSystem_Field * CLinearSystem_Field::Self::get() { return this->self; }

LsSol::ILinearSystem_Sol * CLinearSystem_Field::SolSelf::get() { return this->self; }

Fem::Eqn::ILinearSystem_Eqn * CLinearSystem_Field::EqnSelf::get() { return this->self; }


//////////////////////////////////////
void CLinearSystem_Field::Clear()
{
    this->self->Clear();
}

////////////////////////////////
// function for marge element matrix

// 読み取り用
DelFEM4NetMatVec::CVector_Blk^ CLinearSystem_Field::GetResidual(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetResidual(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CVector_Blk *ret = new MatVec::CVector_Blk(ret_instance_);

    return gcnew DelFEM4NetMatVec::CVector_Blk(ret);
}

// 読み取り用
DelFEM4NetMatVec::CVector_Blk^ CLinearSystem_Field::GetUpdate(  unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetUpdate(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));

    MatVec::CVector_Blk *ret = new MatVec::CVector_Blk(ret_instance_);
    return gcnew DelFEM4NetMatVec::CVector_Blk(ret);
}

// 更新用
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem_Field::GetResidualPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CVector_Blk& ret_instance_ref_ = this->self->GetResidual(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CVector_Blk *ret_instance_ptr_ = &ret_instance_ref_;

    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ret_instance_ptr_);
}

// 更新用
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem_Field::GetUpdatePtr(  unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CVector_Blk& ret_instance_ref_ = this->self->GetUpdate(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CVector_Blk *ret_instance_ptr_ = &ret_instance_ref_;

    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ret_instance_ptr_);
}

// 読み取り用
DelFEM4NetMatVec::CMatDia_BlkCrs^ CLinearSystem_Field::GetMatrix
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CMatDia_BlkCrs& ret_instance_ = this->self->GetMatrix(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    // shallow copyとなるため問題あり
    //MatVec::CMatDia_BlkCrs *ret = new MatVec::CMatDia_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CMatDia_BlkCrs(ret_instance_);
}

// 読み取り用
DelFEM4NetMatVec::CMat_BlkCrs^ CLinearSystem_Field::GetMatrix
    (unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
     unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
     DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CMat_BlkCrs& ret_instance_ = this->self->GetMatrix
        (id_field_col, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_col),
        id_field_row, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_row),
         *(world->Self));    
    //MatVec::CMat_BlkCrs *ret = new MatVec::CMat_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CMat_BlkCrs(ret_instance_);
}

// 更新用
DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ CLinearSystem_Field::GetMatrixPtr
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CMatDia_BlkCrs& ret_instance_ref_ = this->self->GetMatrix(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CMatDia_BlkCrs* ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew  DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr(ret_instance_ptr_);
}
// 更新用
DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ CLinearSystem_Field::GetMatrixPtr
    (unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
     unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
     DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CMat_BlkCrs& ret_instance_ref_ = this->self->GetMatrix
        (id_field_col, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_col),
        id_field_row, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_row),
         *(world->Self));
    MatVec::CMat_BlkCrs *ret_instance_ptr_ = &ret_instance_ref_;
    return gcnew  DelFEM4NetMatVec::CMat_BlkCrs_Ptr(ret_instance_ptr_);
}

////////////////////////////////
// function for marge

void CLinearSystem_Field::InitializeMarge()
{
    this->self->InitializeMarge();
}

//! Finalization before marge (set boundary condition, return residual square norm)
double CLinearSystem_Field::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

////////////////////////////////
// パターン初期化関数

bool CLinearSystem_Field::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

bool CLinearSystem_Field::AddPattern_Field(unsigned int id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, id_field2, *(world->Self));
}

//! fieldとfield2がパターンが同じだとして，ブロックが結合された一つの行列を作る
bool CLinearSystem_Field::AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_CombinedField(id_field, id_field2, *(world->Self));
}

////////////////////////////////
// function for fixed boundary condition

void CLinearSystem_Field::ClearFixedBoundaryCondition()
{
    this->self->ClearFixedBoundaryCondition();
}

bool CLinearSystem_Field::SetFixedBoundaryCondition_Field( unsigned int id_field, unsigned int idofns, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->SetFixedBoundaryCondition_Field(id_field, idofns, *(world->Self));
}

//! set fix boundary condition to all dof in field (id_field)
bool CLinearSystem_Field::SetFixedBoundaryCondition_Field( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->SetFixedBoundaryCondition_Field(id_field, *(world->Self));
}

////////////////////////////////
// function for update solution

double CLinearSystem_Field::MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->MakeResidual(*(world->Self));
}

//! fieldの値を更新する
bool CLinearSystem_Field::UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->UpdateValueOfField(id_field, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

bool CLinearSystem_Field::UpdateValueOfField_RotCRV( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->UpdateValueOfField_RotCRV(id_field, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

bool CLinearSystem_Field::UpdateValueOfField_Newmark
    (double gamma, double dt, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, bool IsInitial )
{
    return this->self->UpdateValueOfField_Newmark(gamma, dt, id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt), IsInitial);
}

bool CLinearSystem_Field::UpdateValueOfField_NewmarkBeta
    (double gamma, double beta, double dt, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, bool IsInitial )
{
    return this->self->UpdateValueOfField_NewmarkBeta(gamma, beta, dt, id_field_val, *(world->Self), IsInitial);
}

bool CLinearSystem_Field::UpdateValueOfField_BackwardEular(double dt, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, bool IsInitial )
{
    return this->self->UpdateValueOfField_BackwardEular(dt, id_field_val, *(world->Self), IsInitial);
}

int CLinearSystem_Field::FindIndexArray_Seg(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->FindIndexArray_Seg(id_field, static_cast<Fem::Field::ELSEG_TYPE>(type), *(world->Self));
}

unsigned int CLinearSystem_Field::GetNLynSysSeg()
{
    return this->self->GetNLynSysSeg();
}

////////////////////////////////
// function for linear solver

unsigned int CLinearSystem_Field::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CLinearSystem_Field::ReSizeTmpVecSolver(unsigned int size_new)
{
    return this->self->ReSizeTmpVecSolver(size_new);
}

double CLinearSystem_Field::DOT(int iv1, int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystem_Field::COPY(int iv1, int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystem_Field::SCAL(double alpha, int iv1)
{
    return this->self->SCAL(alpha, iv1);
}

bool CLinearSystem_Field::AXPY(double alpha, int iv1, int iv2)
{
    return this->self->AXPY(alpha, iv1, iv2);
}
bool CLinearSystem_Field::MATVEC(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MATVEC(alpha, iv1, beta, iv2);
}

