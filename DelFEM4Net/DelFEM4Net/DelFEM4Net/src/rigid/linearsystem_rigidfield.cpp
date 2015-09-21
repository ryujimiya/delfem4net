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

#include "DelFEM4Net/rigid/linearsystem_rigidfield.h"
#include "DelFEM4Net/rigid/rigidbody.h"
#include "DelFEM4Net/ls/solver_ls_iter.h"
#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/field.h"

using namespace DelFEM4NetLs;

/////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_RigidField2
/////////////////////////////////////////////////////////////////////////////////
CLinearSystem_RigidField2::CLinearSystem_RigidField2()
{
    this->self = new Ls::CLinearSystem_RigidField2();
}

CLinearSystem_RigidField2::CLinearSystem_RigidField2(const CLinearSystem_RigidField2% rhs)
{
    assert(false);
    this->self = NULL;
    /*
    const Ls::CLinearSystem_RigidField2& rhs_instance_ = *(rhs.self);
    this->self = new Ls::CLinearSystem_RigidField2(rhs_instance_);
    */
}

CLinearSystem_RigidField2::CLinearSystem_RigidField2(Ls::CLinearSystem_RigidField2 *self)
{
    this->self = self;
}

CLinearSystem_RigidField2::~CLinearSystem_RigidField2()
{
    this->!CLinearSystem_RigidField2();
}

CLinearSystem_RigidField2::!CLinearSystem_RigidField2()
{
    delete this->self;
}

Ls::CLinearSystem_RigidBody * CLinearSystem_RigidField2::LsSelf::get()
{
    return this->self;
}

Fem::Eqn::ILinearSystem_Eqn * CLinearSystem_RigidField2::EqnSelf::get()
{
    return this->self;
}

LsSol::ILinearSystem_Sol * CLinearSystem_RigidField2::SolSelf::get()
{
    return this->self;
}

Ls::CLinearSystem_RigidField2 * CLinearSystem_RigidField2::Self::get()
{
    return this->self;
}

void CLinearSystem_RigidField2::Clear()
{
    this->self->Clear();
}

///////////////////////////////////////////////////////////////
// 場へのインターフェース
int CLinearSystem_RigidField2::FindIndexArray_Seg(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->FindIndexArray_Seg(id_field, static_cast<Fem::Field::ELSEG_TYPE>(type), *(world->Self));
}

int CLinearSystem_RigidField2::GetIndexSegRigid()
{
    return this->self->GetIndexSegRigid();
}

bool CLinearSystem_RigidField2::AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->AddPattern_Field(id_field, *(world->Self));
}

bool CLinearSystem_RigidField2::AddPattern_Field(unsigned int id_field, unsigned int id_field0, 
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddPattern_Field(id_field, id_field0, 
        *(world->Self) );
}

bool CLinearSystem_RigidField2::SetFixedBoundaryCondition_Field(unsigned int id_disp_fix0, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetFixedBoundaryCondition_Field(id_disp_fix0, *(world->Self));
}

bool CLinearSystem_RigidField2::UpdateValueOfField_NewmarkBeta(double newmark_gamma, double newmark_beta, double dt,
        unsigned int id_disp  , DelFEM4NetFem::Field::CFieldWorld^ world, bool is_first)
{
    return this->self->UpdateValueOfField_NewmarkBeta(newmark_gamma, newmark_beta, dt,
        id_disp  , *(world->Self), is_first);
}

DelFEM4NetMatVec::CVector_Blk^ CLinearSystem_RigidField2::GetResidual( 
        unsigned int id_field, 
        DelFEM4NetFem::Field::ELSEG_TYPE type, 
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetResidual( 
        id_field, 
        static_cast<Fem::Field::ELSEG_TYPE>(type), 
        *(world->Self) );
    // nativeのコピーコンストラクタを使用してコピーする
    MatVec::CVector_Blk *ret_ = new MatVec::CVector_Blk(ret_instance_);
    return gcnew DelFEM4NetMatVec::CVector_Blk(ret_);
}

DelFEM4NetMatVec::CMatDia_BlkCrs^ CLinearSystem_RigidField2::GetMatrix( 
        unsigned int id_field, 
        DelFEM4NetFem::Field::ELSEG_TYPE type, 
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    const MatVec::CMatDia_BlkCrs& ret_instance_ = this->self->GetMatrix( 
        id_field, 
        static_cast<Fem::Field::ELSEG_TYPE>(type), 
        *(world->Self) );
    // nativeインスタンスからマネージドクラスインスタンスを生成
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs(ret_instance_);
}

DelFEM4NetMatVec::CMat_BlkCrs^ CLinearSystem_RigidField2::GetMatrix( 
        unsigned int id_field1, DelFEM4NetFem::Field::ELSEG_TYPE type1, 
        unsigned int id_field2, DelFEM4NetFem::Field::ELSEG_TYPE type2, 
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    const MatVec::CMat_BlkCrs& ret_instance_ = this->self->GetMatrix( 
        id_field1, static_cast<Fem::Field::ELSEG_TYPE>(type1), 
        id_field2, static_cast<Fem::Field::ELSEG_TYPE>(type2), 
        *(world->Self) );
    // nativeインスタンスからマネージドクラスインスタンスを生成
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs(ret_instance_);
}

////////////////////////////////////////////////////////////////
// 剛体へのインターフェース
void CLinearSystem_RigidField2::SetRigidSystem(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB, 
        IList<DelFEM4NetRigid::CConstraint^>^ aConst)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す-->しかしpush_backでコピーが生成される
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    this->self->SetRigidSystem(aRB_, aConst_);
}

unsigned int CLinearSystem_RigidField2::GetSizeRigidBody()
{
    return this->self->GetSizeRigidBody();
}

unsigned int CLinearSystem_RigidField2::GetSizeConstraint()
{
    return this->self->GetSizeConstraint();
}

bool CLinearSystem_RigidField2::UpdateValueOfRigidSystem(
        IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst, 
        double dt, double newmark_gamma, double newmark_beta, bool is_first)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    bool ret = this->self->UpdateValueOfRigidSystem(
        aRB_, aConst_,
        dt, newmark_gamma, newmark_beta, is_first);

    // BUGFIX: 変更されたインスタンスを戻す
    for (int i = 0; i < aRB_.size(); i++)
    {
        aRB[i] = nullptr;
        Rigid::CRigidBody3D *modified_= new Rigid::CRigidBody3D(aRB_[i]);
        aRB[i] = gcnew DelFEM4NetRigid::CRigidBody3D(modified_);
    }
    return ret;
}

void CLinearSystem_RigidField2::AddResidual(unsigned int ind, bool is_rb, unsigned int offset, 
        DelFEM4NetCom::CVector3D^ vres, double d)
{
    this->self->AddResidual(ind, is_rb, offset, 
        *(vres->Self), d);
}

void CLinearSystem_RigidField2::AddResidual(unsigned int ind, bool is_rb, unsigned int offset, unsigned int size,
        array<double>^ eres, double d)
{
    pin_ptr<double> eres_ = &eres[0];
    this->self->AddResidual(ind, is_rb, offset, size,
        (double *) eres_, d);
}

void CLinearSystem_RigidField2::SubResidual(unsigned int ind, bool is_rb, array<double>^ res)
{
    pin_ptr<double> res_ = &res[0];
    this->self->SubResidual(ind, is_rb, (double*)res_);
}

void CLinearSystem_RigidField2::AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl,
                           DelFEM4NetCom::CMatrix3^ m, double d, bool isnt_trans)
{
    this->self->AddMatrix(indr, is_rb_r, offsetr,
                           indl, is_rb_l, offsetl,
                           *(m->Self), d, isnt_trans);
}

void CLinearSystem_RigidField2::AddMatrix_Vector(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                                  unsigned int indl, bool is_rb_l, unsigned int offsetl,
                                  DelFEM4NetCom::CVector3D^ vec, double d, bool is_column)
{
    this->self->AddMatrix_Vector(indr, is_rb_r, offsetr,
                                 indl, is_rb_l, offsetl,
                                  *(vec->Self), d, is_column);
}

void CLinearSystem_RigidField2::AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr, unsigned int sizer,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl, unsigned int sizel,
                           array<double>^ emat, double d)
{
    pin_ptr<double> emat_ = &emat[0];
    this->self->AddMatrix(indr, is_rb_r, offsetr, sizer,
                           indl, is_rb_l, offsetl, sizel,
                           (double *)emat_, d);
}

////////////////////////////////
// 剛体弾性体連成インターフェース
bool CLinearSystem_RigidField2::AddPattern_RigidField(
        unsigned int id_field, unsigned int id_field0, DelFEM4NetFem::Field::CFieldWorld^ world, 
        unsigned int irb, IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    bool ret = this->self->AddPattern_RigidField(
        id_field, id_field0, *(world->Self), 
        irb, aRB_, aConst_);

    // BUGFIX: 変更されたインスタンスを戻す
    for (int i = 0; i < aRB_.size(); i++)
    {
        aRB[i] = nullptr;
        Rigid::CRigidBody3D *modified_= new Rigid::CRigidBody3D(aRB_[i]);
        aRB[i] = gcnew DelFEM4NetRigid::CRigidBody3D(modified_);
    }
    return ret;
}

////////////////////////////////

DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ CLinearSystem_RigidField2::GetMatrixPtr( unsigned int ils )
{
    // リファレンスを取得
    MatVec::CMatDia_BlkCrs& ret_ref_ = this->self->GetMatrix(ils);
    MatVec::CMatDia_BlkCrs* ptr_ = &ret_ref_;
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr(ptr_);
}

DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ CLinearSystem_RigidField2::GetMatrixPtr( unsigned int ils, unsigned int jls )
{
    // リファレンスを取得
    MatVec::CMat_BlkCrs& ret_ref_ = this->self->GetMatrix(ils, jls);
    MatVec::CMat_BlkCrs *ptr_ = &ret_ref_;
    return gcnew DelFEM4NetMatVec::CMat_BlkCrs_Ptr(ptr_);
}

DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem_RigidField2::GetResidualPtr( unsigned int ils )
{
    // リファレンスを取得
    MatVec::CVector_Blk& ret_ref_ = this->self->GetResidual(ils);
    MatVec::CVector_Blk* ptr_ = &ret_ref_;
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ptr_);
}

unsigned int CLinearSystem_RigidField2::GetNLinSysSeg()
{
    return this->self->GetNLinSysSeg();
}

void CLinearSystem_RigidField2::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CLinearSystem_RigidField2::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

////////////////////////////////////////////////////////////////
// Solverへのインターフェース
unsigned int CLinearSystem_RigidField2::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CLinearSystem_RigidField2::ReSizeTmpVecSolver(unsigned int isize)
{
    return this->self->ReSizeTmpVecSolver(isize);
}


double CLinearSystem_RigidField2::DOT(int iv1,int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystem_RigidField2::COPY(int iv1,int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystem_RigidField2::SCAL(double d,int iv)
{
    return this->self->SCAL(d, iv);
}

bool CLinearSystem_RigidField2::AXPY(double d,int iv1,int iv2)
{
    return this->self->AXPY(d, iv1, iv2);
}

bool CLinearSystem_RigidField2::MATVEC(double a,int iv1,double b,int iv2)
{
    return this->self->MATVEC(a, iv1, b, iv2);
}

/*
//std::vector<CLinSysSegRF> m_aSegRF;
CLinearSystem_RigidField2::CLinSysSegRFVectorIndexer^ CLinearSystem_RigidField2::aIndRB::get()
{
    std::vector<Ls::CLinearSystem_RigidField2::CLinSysSegRF>& vec_ref_ = this->self->m_aSegRF;
    return gcnew CLinearSystem_RigidField2::CLinSysSegRFVectorIndexer(vec_ref_);
}
*/

//LsSol::CLinearSystem m_ls;
DelFEM4NetLsSol::CLinearSystemAccesser^ CLinearSystem_RigidField2::GetLs()
{
    return gcnew DelFEM4NetLsSol::CLinearSystemAccesser(this->self->m_ls);
}


/////////////////////////////////////////////////////////////////////////////////
// CPreconditioner_RigidField2
/////////////////////////////////////////////////////////////////////////////////

CPreconditioner_RigidField2::CPreconditioner_RigidField2()
{
    this->self = new Ls::CPreconditioner_RigidField2();
}

CPreconditioner_RigidField2::CPreconditioner_RigidField2(const CPreconditioner_RigidField2% rhs)
{
    assert(false);
    this->self = NULL;
    /*
    const Ls::CPreconditioner_RigidField2& rhs_instance_ = *(rhs.self);
    this->self = new Ls::CPreconditioner_RigidField2(rhs_instance_);
    */
}

CPreconditioner_RigidField2::CPreconditioner_RigidField2(Ls::CPreconditioner_RigidField2 *self)
{
    this->self = self;
}


CPreconditioner_RigidField2::~CPreconditioner_RigidField2()
{
    this->!CPreconditioner_RigidField2();
}

CPreconditioner_RigidField2::!CPreconditioner_RigidField2()
{
    delete this->self;
}

Ls::CPreconditioner_RigidField2 * CPreconditioner_RigidField2::Self::get()
{
    return this->self;
}

void CPreconditioner_RigidField2::SetFillInLevel(int lev, int ilss0)
{
    this->self->SetFillInLevel(lev, ilss0);
}

void CPreconditioner_RigidField2::Clear()
{
    this->self->Clear();
}


void CPreconditioner_RigidField2::SetLinearSystem(CLinearSystem_RigidField2^ ls)
{
    this->self->SetLinearSystem(*(ls->Self));
}

void CPreconditioner_RigidField2::SetValue(CLinearSystem_RigidField2^ ls)
{
    this->self->SetValue(*(ls->Self));
}

void CPreconditioner_RigidField2::SolvePrecond(CLinearSystem_RigidField2^% ls, int iv)
{
    this->self->SolvePrecond(*(ls->Self), iv);
}






/////////////////////////////////////////////////////////////////////////////////
// CLinearSystemPreconditioner_RigidField2
/////////////////////////////////////////////////////////////////////////////////

//CLinearSystemPreconditioner_RigidField2::CLinearSystemPreconditioner_RigidField2()
//{
//    this->self = new Ls::CLinearSystemPreconditioner_RigidField2();
//}

CLinearSystemPreconditioner_RigidField2::CLinearSystemPreconditioner_RigidField2(const CLinearSystemPreconditioner_RigidField2% rhs)
{
    assert(false);
    this->self = NULL;
    /*
    const Ls::CLinearSystemPreconditioner_RigidField2& rhs_instance_ = *(rhs.self);
    this->self = new Ls::CLinearSystemPreconditioner_RigidField2(rhs_instance_);
    */
}

CLinearSystemPreconditioner_RigidField2::CLinearSystemPreconditioner_RigidField2(Ls::CLinearSystemPreconditioner_RigidField2 *self)
{
    this->self = self;
}

CLinearSystemPreconditioner_RigidField2::CLinearSystemPreconditioner_RigidField2(CLinearSystem_RigidField2^ ls, CPreconditioner_RigidField2^ prec)
{
    this->self = new Ls::CLinearSystemPreconditioner_RigidField2(*(ls->Self), *(prec->Self));
}


CLinearSystemPreconditioner_RigidField2::~CLinearSystemPreconditioner_RigidField2()
{
    this->!CLinearSystemPreconditioner_RigidField2();
}

CLinearSystemPreconditioner_RigidField2::!CLinearSystemPreconditioner_RigidField2()
{
    delete this->self;
}

LsSol::ILinearSystemPreconditioner_Sol * CLinearSystemPreconditioner_RigidField2::SolSelf::get()
{
    return this->self;
}

Ls::CLinearSystemPreconditioner_RigidField2 * CLinearSystemPreconditioner_RigidField2::Self::get()
{
    return this->self;
}

//! ソルバに必要な作業ベクトルの数を得る
unsigned int CLinearSystemPreconditioner_RigidField2::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

//! ソルバに必要な作業ベクトルの数を設定
bool CLinearSystemPreconditioner_RigidField2::ReSizeTmpVecSolver(unsigned int isize)
{
    return this->self->ReSizeTmpVecSolver(isize);
}

double CLinearSystemPreconditioner_RigidField2::DOT(int iv1,int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystemPreconditioner_RigidField2::COPY(int iv1,int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystemPreconditioner_RigidField2::SCAL(double d,int iv)
{
    return this->self->SCAL(d, iv);
}

bool CLinearSystemPreconditioner_RigidField2::AXPY(double d,int iv1,int iv2)
{
    return this->self->AXPY(d, iv1, iv2);
}

bool CLinearSystemPreconditioner_RigidField2::MATVEC(double a,int iv1,double b,int iv2)
{
    return this->self->MATVEC(a, iv1, b, iv2);
}

bool CLinearSystemPreconditioner_RigidField2::SolvePrecond(int iv)
{
    return this->self->SolvePrecond(iv);
}

