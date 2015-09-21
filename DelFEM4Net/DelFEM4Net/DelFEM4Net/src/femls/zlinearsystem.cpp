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

#include <math.h>
#include <cstdlib> //(abort)

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/matvec/zmatdia_blkcrs.h"
#include "DelFEM4Net/matvec/zmat_blkcrs.h"
#include "DelFEM4Net/matvec/zvector_blk.h"
#include "DelFEM4Net/matvec/diamat_blk.h"

#include "DelFEM4Net/femls/zlinearsystem.h"

#ifndef for 
#define for if(0); else for
#endif

using namespace DelFEM4NetMatVec;
using namespace DelFEM4NetFem::Ls;
using namespace DelFEM4NetFem::Field;

///////////////////////////////////////////////////////////////////////////////
// CZLinearSystem
///////////////////////////////////////////////////////////////////////////////

CZLinearSystem::CZLinearSystem()
{
    this->self = new Fem::Ls::CZLinearSystem();
}

CZLinearSystem::CZLinearSystem(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    this->self = NULL;
}

CZLinearSystem::CZLinearSystem(const CZLinearSystem% rhs)
{
    const Fem::Ls::CZLinearSystem& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CZLinearSystem(rhs_instance_);
}

CZLinearSystem::CZLinearSystem(Fem::Ls::CZLinearSystem *self)
{
    this->self = self;
}

CZLinearSystem::~CZLinearSystem()
{
    this->!CZLinearSystem();
}

CZLinearSystem::!CZLinearSystem()
{
    delete this->self;
}

Fem::Ls::CZLinearSystem * CZLinearSystem::Self::get() { return this->self; }


//////////////////////////////////////
void CZLinearSystem::Clear()
{
    this->self->Clear();
}

bool CZLinearSystem::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}
/*nativeライブラリで実装されていない
bool CZLinearSystem::SetVector_fromField
        (unsigned int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt)
{
    return this->self->SetVector_fromField(iv, id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}
*/

bool CZLinearSystem::NormalizeVector(int iv)
{
    return this->self->NormalizeVector(iv);
}


////////////////////////////////
// function for marge

DelFEM4NetMatVec::CZVector_Blk_Ptr^ CZLinearSystem::GetResidualPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CZVector_Blk* ret_ptr_ = this->self->GetResidualPtr(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    /*
    const MatVec::CZVector_Blk& ret_instance_ = *ret_ptr_;
    // shallow copyとなるため問題あり
    //MatVec::CZVector_Blk *ret = new MatVec::CZVector_Blk(ret_instance);
    //return gcnew DelFEM4NetMatVec::CZVector_Blk(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew DelFEM4NetMatVec::CZVector_Blk(ret_instance_);
    */
    // ポインタを操作するために渡すので、コピーではなく、ポインタをラップしたインスタンスを渡す
    return gcnew CZVector_Blk_Ptr(ret_ptr_);
}

DelFEM4NetMatVec::CZVector_Blk_Ptr^ CZLinearSystem::GetUpdatePtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CZVector_Blk* ret_ptr_ = this->self->GetUpdatePtr(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    /*
    const MatVec::CZVector_Blk& ret_instance_ = *ret_ptr_;
    // shallow copyとなるため問題あり
    //MatVec::CZVector_Blk *ret = new MatVec::CZVector_Blk(ret_instance);
    //return gcnew DelFEM4NetMatVec::CZVector_Blk(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew DelFEM4NetMatVec::CZVector_Blk(ret_instance_);
    */
    // ポインタを操作するために渡すので、コピーではなく、ポインタをラップしたインスタンスを渡す
    return gcnew CZVector_Blk_Ptr(ret_ptr_);
}

DelFEM4NetMatVec::CZMatDia_BlkCrs_Ptr^ CZLinearSystem::GetMatrixPtr
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CZMatDia_BlkCrs* ret_ptr_= this->self->GetMatrixPtr(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    /*
    const MatVec::CZMatDia_BlkCrs& ret_instance_ = *ret_ptr_;
    // shallow copyとなるため問題あり
    //MatVec::CZMatDia_BlkCrs *ret = new MatVec::CZMatDia_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CZMatDia_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CZMatDia_BlkCrs(ret_instance_);
    */
    // ポインタを操作するために渡すので、コピーではなく、ポインタをラップしたインスタンスを渡す
    return gcnew CZMatDia_BlkCrs_Ptr(ret_ptr_);
}
DelFEM4NetMatVec::CZMat_BlkCrs_Ptr^ CZLinearSystem::GetMatrixPtr
    (unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
     unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
     DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CZMat_BlkCrs* ret_ptr_ = this->self->GetMatrixPtr
        (id_field_col, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_col),
        id_field_row, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type_row),
         *(world->Self));
    /*
    const MatVec::CZMat_BlkCrs& ret_instance_ = *ret_ptr_; // 参照を取得。コピーしない
    //MatVec::CZMatDia_BlkCrs *ret = new MatVec::CZMatDia_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CZMatDia_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CZMat_BlkCrs(ret_instance_);
    */
    // ポインタを操作するために渡すので、コピーではなく、ポインタをラップしたインスタンスを渡す
    return gcnew CZMat_BlkCrs_Ptr(ret_ptr_);
}

unsigned int CZLinearSystem::GetTmpBufferSize()
{
    return this->self->GetTmpBufferSize();
}

void CZLinearSystem::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CZLinearSystem::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

////////////////////////////////
// function for fixed boundary condition

void CZLinearSystem::ClearFixedBoundaryCondition()
{
    this->self->ClearFixedBoundaryCondition();
}

bool CZLinearSystem::SetFixedBoundaryCondition_Field( unsigned int id_field, unsigned int idofns, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->SetFixedBoundaryCondition_Field(id_field, idofns, *(world->Self));
}

bool CZLinearSystem::SetFixedBoundaryCondition_Field( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->SetFixedBoundaryCondition_Field(id_field, *(world->Self));
}

double CZLinearSystem::MakeResidual(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->MakeResidual(id_field_val, *(world->Self));
}

bool CZLinearSystem::UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->UpdateValueOfField(id_field, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

////////////////////////////////
// function for linear solver

unsigned int CZLinearSystem::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CZLinearSystem::ReSizeTmpVecSolver(unsigned int size_new)
{
    return this->self->ReSizeTmpVecSolver(size_new);
}

bool CZLinearSystem::Conjugate(int iv1)
{
    return this->self->Conjugate(iv1);
}


DelFEM4NetCom::Complex^ CZLinearSystem::DOT(int iv1, int iv2)
{
    const Com::Complex& c_instance_  = this->self->DOT(iv1, iv2);
    Com::Complex * c_ = new Com::Complex(c_instance_);
    return gcnew DelFEM4NetCom::Complex(c_);
}

DelFEM4NetCom::Complex^ CZLinearSystem::INPROCT(int iv1, int iv2)
{
    const Com::Complex& c_instance_ = this->self->INPROCT(iv1, iv2);
    Com::Complex * c_ = new Com::Complex(c_instance_);
    return gcnew DelFEM4NetCom::Complex(c_);
}


bool CZLinearSystem::COPY(int iv_from, int iv_to)
{
    return this->self->COPY(iv_from, iv_to);
}

bool CZLinearSystem::SCAL(DelFEM4NetCom::Complex^ alpha, int iv1)
{
    return this->self->SCAL(*(alpha->Self), iv1);
}

bool CZLinearSystem::AXPY(DelFEM4NetCom::Complex^ alpha, int iv1, int iv2)
{
    return this->self->AXPY(*(alpha->Self), iv1, iv2);
}

bool CZLinearSystem::MatVec(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MatVec(alpha, iv1, beta, iv2);
}

bool CZLinearSystem::MatVec_Hermitian(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MatVec_Hermitian(alpha, iv1, beta, iv2);
}

////////////////////////////////
// function for preconditioner
    
unsigned int CZLinearSystem::GetNLynSysSeg()
{
    return this->self->GetNLynSysSeg();
}

int CZLinearSystem::FindIndexArray_Seg( unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->FindIndexArray_Seg(id_field, static_cast<Fem::Field::ELSEG_TYPE>(type), *(world->Self));
}

bool CZLinearSystem::IsMatrix(unsigned int ilss, unsigned int jlss)
{
    return this->self->IsMatrix(ilss, jlss);
}

DelFEM4NetMatVec::CZMatDia_BlkCrs^ CZLinearSystem::GetMatrix(unsigned int ilss)
{
    const MatVec::CZMatDia_BlkCrs& ret_instance_ = this->self->GetMatrix(ilss);
    // shallow copyとなるため問題あり
    //MatVec::CZMatDia_BlkCrs *ret = new MatVec::CZMatDia_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CZMatDia_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CZMatDia_BlkCrs(ret_instance_);
}


DelFEM4NetMatVec::CZMat_BlkCrs^ CZLinearSystem::GetMatrix(unsigned int ilss, unsigned int jlss)
{
    const MatVec::CZMat_BlkCrs& ret_instance_ = this->self->GetMatrix(ilss, jlss);
    // shallow copyとなるため問題あり
    //MatVec::CZMat_BlkCrs *ret = new MatVec::CZMat_BlkCrs(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CZMat_BlkCrs(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CZMat_BlkCrs(ret_instance_);
}

DelFEM4NetMatVec::CZVector_Blk^ CZLinearSystem::GetVector(int iv, unsigned int ilss)
{
    const MatVec::CZVector_Blk& ret_instance_ = this->self->GetVector(iv, ilss);
    // shallow copyとなるため問題あり
    //MatVec::CZVector_Blk *ret = new MatVec::CZVector_Blk(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CZVector_Blk(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew DelFEM4NetMatVec::CZVector_Blk(ret_instance_);
}




///////////////////////////////////////////////////////////////////////////////
// CZLinearSystem
///////////////////////////////////////////////////////////////////////////////

CZLinearSystem_GeneralEigen::CZLinearSystem_GeneralEigen() : CZLinearSystem(false)
{
    assert(CZLinearSystem::self == NULL);
    
    this->self = new Fem::Ls::CZLinearSystem_GeneralEigen();

    setBaseSelf();
}

CZLinearSystem_GeneralEigen::CZLinearSystem_GeneralEigen(bool isCreateInstance) : CZLinearSystem(false)
{
    assert(CZLinearSystem::self == NULL);

    assert(isCreateInstance == false);

    this->self = NULL;

    setBaseSelf();
}

CZLinearSystem_GeneralEigen::CZLinearSystem_GeneralEigen(const CZLinearSystem_GeneralEigen% rhs) : CZLinearSystem(false)
{
    assert(CZLinearSystem::self == NULL);

    const Fem::Ls::CZLinearSystem_GeneralEigen& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CZLinearSystem_GeneralEigen(rhs_instance_);
}

CZLinearSystem_GeneralEigen::CZLinearSystem_GeneralEigen(Fem::Ls::CZLinearSystem_GeneralEigen *self) : CZLinearSystem(false)
{
    assert(CZLinearSystem::self == NULL);

    this->self = self;

    setBaseSelf();
}

CZLinearSystem_GeneralEigen::~CZLinearSystem_GeneralEigen()
{
    this->!CZLinearSystem_GeneralEigen();
}

CZLinearSystem_GeneralEigen::!CZLinearSystem_GeneralEigen()
{
    delete this->self;
    
    this->self = NULL;
    
    setBaseSelf();
}

Fem::Ls::CZLinearSystem_GeneralEigen * CZLinearSystem_GeneralEigen::Self::get() { return this->self; }


void CZLinearSystem_GeneralEigen::setBaseSelf()
{
    CZLinearSystem::self = this->self;
}

void CZLinearSystem_GeneralEigen::Clear()
{
    this->self->Clear();
}

bool CZLinearSystem_GeneralEigen::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

void CZLinearSystem_GeneralEigen::InitializeMarge()
{
    this->self->InitializeMarge();
}

DelFEM4NetMatVec::CDiaMat_Blk^ CZLinearSystem_GeneralEigen::GetDiaMassMatrixPtr
    (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    MatVec::CDiaMat_Blk* ret_ptr = this->self->GetDiaMassMatrixPtr(id_field, static_cast<Fem::Field::ELSEG_TYPE>(elseg_type), *(world->Self));
    const MatVec::CDiaMat_Blk& ret_instance_ = *ret_ptr;
    // shallow copyとなるため問題あり
    //MatVec::CDiaMat_Blk *ret = new MatVec::CDiaMat_Blk(ret_instance_);
    //return gcnew DelFEM4NetMatVec::CDiaMat_Blk(ret);
    // nativeインスタンスからマネジードインスタンス生成
    return gcnew  DelFEM4NetMatVec::CDiaMat_Blk(ret_instance_);
}



bool CZLinearSystem_GeneralEigen::SetVector_fromField(int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt )
{
    return this->self->SetVector_fromField(iv, id_field_val, *(world->Self), static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt));
}

bool CZLinearSystem_GeneralEigen::DecompMultMassMatrix()
{
    return this->self->DecompMultMassMatrix();
}

void CZLinearSystem_GeneralEigen::OffsetDiagonal(double lambda)
{
    this->self->OffsetDiagonal(lambda);
}

bool CZLinearSystem_GeneralEigen::MultUpdateInvMassDecomp()
{
    return this->self->MultUpdateInvMassDecomp();
}

bool CZLinearSystem_GeneralEigen::MultVecMassDecomp(int ivec)
{
    return this->self->MultVecMassDecomp(ivec);
}

/*nativeライブラリで実装されていない
void CZLinearSystem_GeneralEigen::RemoveConstant(int iv)
{
    return this->self->RemoveConstant(iv);
}
*/

