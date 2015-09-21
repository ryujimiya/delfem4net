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

#include <math.h>

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/femls/linearsystem_fieldsave.h"

#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/vector3d.h"
//#include "DelFEM4Net/quaternion.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

//using namespace DelFEM4NetMatVec;
using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetFem::Field;


////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_Save
////////////////////////////////////////////////////////////////////////////////

CLinearSystem_Save::CLinearSystem_Save() : CLinearSystem_Field(false)
{
    this->self = new Fem::Ls::CLinearSystem_Save();
    
    setBaseSelf();
}

CLinearSystem_Save::CLinearSystem_Save(bool isCreateInstance) : CLinearSystem_Field(false)
{
    assert(isCreateInstance == false);

    this->self = NULL;
    setBaseSelf();
}

CLinearSystem_Save::CLinearSystem_Save(const CLinearSystem_Save% rhs) : CLinearSystem_Field(false)
{
    const Fem::Ls::CLinearSystem_Save& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CLinearSystem_Save(rhs_instance_);

    setBaseSelf();
}

CLinearSystem_Save::CLinearSystem_Save(Fem::Ls::CLinearSystem_Save *self) : CLinearSystem_Field(false)
{
    this->self = self;

    setBaseSelf();
}

CLinearSystem_Save::~CLinearSystem_Save()
{
    this->!CLinearSystem_Save();
}

CLinearSystem_Save::!CLinearSystem_Save()
{
    delete this->self;
    
    this->self = NULL;
    setBaseSelf();
}

Fem::Ls::CLinearSystem_Save * CLinearSystem_Save::Self::get() { return this->self; }

void CLinearSystem_Save::setBaseSelf()
{
    CLinearSystem_Field::self = this->self;
}

bool CLinearSystem_Save::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

bool CLinearSystem_Save::AddPattern_Field(unsigned int id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, id_field2, *(world->Self));
}

bool CLinearSystem_Save::AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_CombinedField(id_field, id_field2, *(world->Self));
}

// 読み取り用
DelFEM4NetMatVec::CMat_BlkCrs^ CLinearSystem_Save::GetMatrix_Boundary(
    unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
    unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
    DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CMat_BlkCrs& ret_instance_ = this->self->GetMatrix_Boundary(
        id_field_col, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_col),
        id_field_row, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_row),
        *(world->Self));
    // shallow copyなのでNG
    //MatVec::CMat_BlkCrs *ret = new MatVec::CMat_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret);
    // nativeインスタンスからマネージドインスタンス生成
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret_instance_);
}

// 更新用
DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ CLinearSystem_Save::GetMatrixPtr_Boundary(
    unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
    unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
    DelFEM4NetFem::Field::CFieldWorld^ world)
{
    // リファレンスで受け取る
    MatVec::CMat_BlkCrs& ret_instance_ref_ = this->self->GetMatrix_Boundary(
        id_field_col, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_col),
        id_field_row, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_row),
        *(world->Self));
    MatVec::CMat_BlkCrs* ret_instance_ptr_ = &ret_instance_ref_;

    return gcnew DelFEM4NetMatVec::CMat_BlkCrs_Ptr(ret_instance_ptr_);
}

// 読み取り用
DelFEM4NetMatVec::CVector_Blk^ CLinearSystem_Save::GetForce(
    unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,
    DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetForce(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CVector_Blk *ret = new MatVec::CVector_Blk(ret_instance_);
    
    return gcnew DelFEM4NetMatVec::CVector_Blk(ret);
}

// 更新用
DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem_Save::GetForcePtr(
    unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,
    DelFEM4NetFem::Field::CFieldWorld^ world)
{
    // リファレンスで受け取る
    MatVec::CVector_Blk& ret_instance_ref_ = this->self->GetForce(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    MatVec::CVector_Blk *ret_instance_ptr_ = &ret_instance_ref_;
    
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ret_instance_ptr_);
}

//! CLinearSystem_Save::マージ前の初期化（基底クラスの隠蔽）
void CLinearSystem_Save::InitializeMarge()
{
    this->self->InitializeMarge();
}

//! CLinearSystem_Save::マージ後の処理（基底クラスの隠蔽）
double CLinearSystem_Save::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

//! CLinearSystem_Save::残差を作る
double CLinearSystem_Save::MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->MakeResidual(*(world->Self));
}

bool CLinearSystem_Save::UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt)
{
    return this->self->UpdateValueOfField(id_field, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_SaveDiaM_Newmark
////////////////////////////////////////////////////////////////////////////////

CLinearSystem_SaveDiaM_Newmark::CLinearSystem_SaveDiaM_Newmark() : CLinearSystem_Save(false)
{
    this->self = new Fem::Ls::CLinearSystem_SaveDiaM_Newmark();
    
    setBaseSelf();
}

CLinearSystem_SaveDiaM_Newmark::CLinearSystem_SaveDiaM_Newmark(const CLinearSystem_SaveDiaM_Newmark% rhs) : CLinearSystem_Save(false)
{
    const Fem::Ls::CLinearSystem_SaveDiaM_Newmark& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CLinearSystem_SaveDiaM_Newmark(rhs_instance_);

    setBaseSelf();
}

CLinearSystem_SaveDiaM_Newmark::CLinearSystem_SaveDiaM_Newmark(Fem::Ls::CLinearSystem_SaveDiaM_Newmark *self) : CLinearSystem_Save(false)
{
    this->self = self;

    setBaseSelf();
}

CLinearSystem_SaveDiaM_Newmark::~CLinearSystem_SaveDiaM_Newmark()
{
    this->!CLinearSystem_SaveDiaM_Newmark();
}

CLinearSystem_SaveDiaM_Newmark::!CLinearSystem_SaveDiaM_Newmark()
{
    delete this->self;

    this->self = NULL;
    setBaseSelf();
}

Fem::Ls::CLinearSystem_SaveDiaM_Newmark * CLinearSystem_SaveDiaM_Newmark::Self::get() { return this->self; }


void CLinearSystem_SaveDiaM_Newmark::setBaseSelf()
{
    CLinearSystem_Save::setBaseSelf();

    CLinearSystem_Save::self = this->self;
}

void CLinearSystem_SaveDiaM_Newmark::SetNewmarkParameter(double gamma, double dt)
{
    this->self->SetNewmarkParameter(gamma, dt);
}

double CLinearSystem_SaveDiaM_Newmark::GetGamma()
{
    return this->self->GetGamma();
}

double CLinearSystem_SaveDiaM_Newmark::GetDt()
{
    return this->self->GetDt();
}

bool CLinearSystem_SaveDiaM_Newmark::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

void CLinearSystem_SaveDiaM_Newmark::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CLinearSystem_SaveDiaM_Newmark::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

double CLinearSystem_SaveDiaM_Newmark::MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->MakeResidual(*(world->Self));
}

bool CLinearSystem_SaveDiaM_Newmark::UpdateValueOfField
    (unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->UpdateValueOfField(id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

DelFEM4NetMatVec::CDiaMat_Blk^ CLinearSystem_SaveDiaM_Newmark::GetDiaMassMatrix
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CDiaMat_Blk& ret_instance_ = this->self->GetDiaMassMatrix(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    // shallow copyなのでNG
    //MatVec::CDiaMat_Blk *ret = new MatVec::CDiaMat_Blk(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret);
    // bativeインスタンスからマネージドインスタンスを生成
    return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret_instance_);
}




////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_SaveDiaM_NewmarkBeta
////////////////////////////////////////////////////////////////////////////////

CLinearSystem_SaveDiaM_NewmarkBeta::CLinearSystem_SaveDiaM_NewmarkBeta() : CLinearSystem_Save(false)
{
    this->self = new Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta();
    
    setBaseSelf();
}

CLinearSystem_SaveDiaM_NewmarkBeta::CLinearSystem_SaveDiaM_NewmarkBeta(const CLinearSystem_SaveDiaM_NewmarkBeta% rhs) : CLinearSystem_Save(false)
{
    const Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta(rhs_instance_);

    setBaseSelf();
}

CLinearSystem_SaveDiaM_NewmarkBeta::CLinearSystem_SaveDiaM_NewmarkBeta(Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta *self) : CLinearSystem_Save(false)
{
    this->self = self;

    setBaseSelf();
}

CLinearSystem_SaveDiaM_NewmarkBeta::~CLinearSystem_SaveDiaM_NewmarkBeta()
{
    this->!CLinearSystem_SaveDiaM_NewmarkBeta();
}

CLinearSystem_SaveDiaM_NewmarkBeta::!CLinearSystem_SaveDiaM_NewmarkBeta()
{
    delete this->self;

    this->self = NULL;
    setBaseSelf();
}

Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta * CLinearSystem_SaveDiaM_NewmarkBeta::Self::get() { return this->self; }


void CLinearSystem_SaveDiaM_NewmarkBeta::setBaseSelf()
{
    CLinearSystem_Save::setBaseSelf();

    CLinearSystem_Save::self = this->self;
}

void CLinearSystem_SaveDiaM_NewmarkBeta::SetNewmarkParameter(double beta, double gamma, double dt)
{
    this->self->SetNewmarkParameter(beta, gamma, dt);
}

double CLinearSystem_SaveDiaM_NewmarkBeta::GetGamma()
{
    return this->self->GetGamma();
}

double CLinearSystem_SaveDiaM_NewmarkBeta::GetDt()
{
    return this->self->GetDt();
}

double CLinearSystem_SaveDiaM_NewmarkBeta::GetBeta()
{
    return this->self->GetBeta();
}


DelFEM4NetMatVec::CDiaMat_Blk^ CLinearSystem_SaveDiaM_NewmarkBeta::GetDiaMassMatrix
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CDiaMat_Blk& ret_instance_ = this->self->GetDiaMassMatrix(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    //MatVec::CDiaMat_Blk *ret = new MatVec::CDiaMat_Blk(ret_instance_);    
    //return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret);
    // nativeインスタンスからマネージドインスタンスを生成
    return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret_instance_);
}

bool CLinearSystem_SaveDiaM_NewmarkBeta::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

bool CLinearSystem_SaveDiaM_NewmarkBeta::AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_CombinedField(id_field, id_field2, *(world->Self));
}

void CLinearSystem_SaveDiaM_NewmarkBeta::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CLinearSystem_SaveDiaM_NewmarkBeta::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

double CLinearSystem_SaveDiaM_NewmarkBeta::MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->MakeResidual(*(world->Self));
}

bool CLinearSystem_SaveDiaM_NewmarkBeta::UpdateValueOfField
    (unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->UpdateValueOfField(id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}



////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_Eigen
////////////////////////////////////////////////////////////////////////////////

CLinearSystem_Eigen::CLinearSystem_Eigen() : CLinearSystem_Field((Fem::Ls::CLinearSystem_Field *)NULL)
{
    this->self = new Fem::Ls::CLinearSystem_Eigen();
    
    setBaseSelf();
}

CLinearSystem_Eigen::CLinearSystem_Eigen(const CLinearSystem_Eigen% rhs) : CLinearSystem_Field((Fem::Ls::CLinearSystem_Field *)NULL)
{
    const Fem::Ls::CLinearSystem_Eigen& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CLinearSystem_Eigen(rhs_instance_);

    setBaseSelf();
}

CLinearSystem_Eigen::CLinearSystem_Eigen(Fem::Ls::CLinearSystem_Eigen *self) : CLinearSystem_Field((Fem::Ls::CLinearSystem_Field *)NULL)
{
    this->self = self;

    setBaseSelf();
}

CLinearSystem_Eigen::~CLinearSystem_Eigen()
{
    this->!CLinearSystem_Eigen();
}

CLinearSystem_Eigen::!CLinearSystem_Eigen()
{
    delete this->self;

    this->self = NULL;
    setBaseSelf();
}

Fem::Ls::CLinearSystem_Eigen * CLinearSystem_Eigen::Self::get() { return this->self; }


void CLinearSystem_Eigen::setBaseSelf()
{
    CLinearSystem_Field::self = this->self;
}

void CLinearSystem_Eigen::Clear()
{
    this->self->Clear();
}

bool CLinearSystem_Eigen::AddPattern_Field(unsigned int id_field,DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

void CLinearSystem_Eigen::InitializeMarge()
{
    this->self->InitializeMarge();
}

DelFEM4NetMatVec::CDiaMat_Blk^ CLinearSystem_Eigen::GetDiaMassMatrix
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    const MatVec::CDiaMat_Blk& ret_instance_ = this->self->GetDiaMassMatrix(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    // shallow copyなのでNG
    //MatVec::CDiaMat_Blk *ret = new MatVec::CDiaMat_Blk(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret);
    // bativeインスタンスからマネージドインスタンスを生成
    return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret_instance_);
}

bool CLinearSystem_Eigen::SetVector_fromField
    (int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->SetVector_fromField(iv, id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

bool CLinearSystem_Eigen::DecompMultMassMatrix()
{
    return this->self->DecompMultMassMatrix();
}

bool CLinearSystem_Eigen::MultUpdateInvMassDecomp()
{
    return this->self->MultUpdateInvMassDecomp();
}

bool CLinearSystem_Eigen::MultVecMassDecomp(int ivec)
{
    return this->self->MultVecMassDecomp(ivec);
}

void CLinearSystem_Eigen::OffsetDiagonal(double lambda)
{
    this->self->OffsetDiagonal(lambda);
}
