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

#include "DelFEM4Net/rigid/linearsystem_rigid.h"
#include "DelFEM4Net/rigid/rigidbody.h"
#include "DelFEM4Net/indexed_array.h"

using namespace DelFEM4NetLs;

/////////////////////////////////////////////////////////////////////////////////
// CLinearSystem_RigidBody_CRS2
/////////////////////////////////////////////////////////////////////////////////
CLinearSystem_RigidBody_CRS2::CLinearSystem_RigidBody_CRS2()
{
    this->self = new Ls::CLinearSystem_RigidBody_CRS2();
}

CLinearSystem_RigidBody_CRS2::CLinearSystem_RigidBody_CRS2(const CLinearSystem_RigidBody_CRS2% rhs)
{
    assert(false);
    this->self = NULL;
    /*
    const Ls::CLinearSystem_RigidBody_CRS2& rhs_instance_ = *(rhs.self);
    this->self = new Ls::CLinearSystem_RigidBody_CRS2(rhs_instance_);
    */
}

CLinearSystem_RigidBody_CRS2::CLinearSystem_RigidBody_CRS2(Ls::CLinearSystem_RigidBody_CRS2 *self)
{
    this->self = self;
}

CLinearSystem_RigidBody_CRS2::CLinearSystem_RigidBody_CRS2(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB,
        IList<DelFEM4NetRigid::CConstraint^>^ aConst)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    this->self = new Ls::CLinearSystem_RigidBody_CRS2(aRB_, aConst_);

}

CLinearSystem_RigidBody_CRS2::~CLinearSystem_RigidBody_CRS2()
{
    this->!CLinearSystem_RigidBody_CRS2();
}

CLinearSystem_RigidBody_CRS2::!CLinearSystem_RigidBody_CRS2()
{
    delete this->self;
}

Ls::CLinearSystem_RigidBody * CLinearSystem_RigidBody_CRS2::LsSelf::get()
{
    return this->self;
}

LsSol::ILinearSystem_Sol * CLinearSystem_RigidBody_CRS2::SolSelf::get()
{
    return this->self;
}

Ls::CLinearSystem_RigidBody_CRS2 * CLinearSystem_RigidBody_CRS2::Self::get()
{
    return this->self;
}

void CLinearSystem_RigidBody_CRS2::Clear()
{
    this->self->Clear();
}

void CLinearSystem_RigidBody_CRS2::SetRigidSystem(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB,
        IList<DelFEM4NetRigid::CConstraint^>^ aConst)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    this->self->SetRigidSystem(aRB_, aConst_);
}

unsigned int CLinearSystem_RigidBody_CRS2::GetSizeRigidBody()
{
    return this->self->GetSizeRigidBody();
}

void CLinearSystem_RigidBody_CRS2::AddResidual(unsigned int ind, bool is_rb, unsigned int offset, 
        DelFEM4NetCom::CVector3D^ vres, double d )
{
    this->self->AddResidual(ind, is_rb, offset, 
        *(vres->Self), d);
}

void CLinearSystem_RigidBody_CRS2::AddResidual(unsigned int ind, bool is_rb, unsigned int offset, unsigned int size,
        array<double>^ eres, double d)
{
    pin_ptr<double> eres_ = &eres[0];
    this->self->AddResidual(ind, is_rb, offset, size,
        (double *)eres_, d);
}

void CLinearSystem_RigidBody_CRS2::SubResidual(unsigned int ind, bool is_rb, array<double>^ res)
{
    pin_ptr<double> res_ = &res[0];
    this->self->SubResidual(ind, is_rb, (double *) res_);
}

void CLinearSystem_RigidBody_CRS2::AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl,
                           DelFEM4NetCom::CMatrix3^ m, double d, bool isnt_trans)
{
    this->self->AddMatrix(indr, is_rb_r, offsetr,
                           indl, is_rb_l, offsetl,
                           *(m->Self), d, isnt_trans);
}

void CLinearSystem_RigidBody_CRS2::AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr, unsigned int sizer,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl, unsigned int sizel,
                           array<double>^ emat, double d)
{
    pin_ptr<double> emat_ = &emat[0];
    this->self->AddMatrix(indr, is_rb_r, offsetr, sizer,
                           indl, is_rb_l, offsetl, sizel,
                           (double *) emat_, d);
}

void CLinearSystem_RigidBody_CRS2::AddMatrix_Vector(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                         unsigned int indl, bool is_rb_l, unsigned int offsetl,
                         DelFEM4NetCom::CVector3D^ vec, double d, bool is_column)
{
    this->self->AddMatrix_Vector(indr, is_rb_r, offsetr,
                         indl, is_rb_l, offsetl,
                         *(vec->Self), d, is_column);
}

void CLinearSystem_RigidBody_CRS2::InitializeMarge()
{
    this->self->InitializeMarge();
}

double CLinearSystem_RigidBody_CRS2::FinalizeMarge()
{
    return this->self->FinalizeMarge();
}

unsigned int CLinearSystem_RigidBody_CRS2::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CLinearSystem_RigidBody_CRS2::ReSizeTmpVecSolver(unsigned int isize)
{
    return this->self->ReSizeTmpVecSolver(isize);
}

double CLinearSystem_RigidBody_CRS2::DOT(int iv1,int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystem_RigidBody_CRS2::COPY(int iv1,int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystem_RigidBody_CRS2::SCAL(double d,int iv)
{
    return this->self->SCAL(d, iv);
}

bool CLinearSystem_RigidBody_CRS2::AXPY(double d,int iv1,int iv2)
{
    return this->self->AXPY(d, iv1, iv2);
}

bool CLinearSystem_RigidBody_CRS2::MATVEC(double a,int iv1,double b,int iv2)
{
    return this->self->MATVEC(a, iv1, b, iv2);
}

DelFEM4NetMatVec::CVector_Blk_Ptr^ CLinearSystem_RigidBody_CRS2::GetVectorPtr(int iv)
{
    // リファレンスを取得
    MatVec::CVector_Blk& ret_ref_ = this->self->GetVector(iv);
    // ポインタ
    MatVec::CVector_Blk* ret_ptr_ = &ret_ref_;
    // アクセッサーを生成
    return gcnew DelFEM4NetMatVec::CVector_Blk_Ptr(ret_ptr_);
}

DelFEM4NetMatVec::CVector_Blk^ CLinearSystem_RigidBody_CRS2::GetVector(int iv)
{
    // 変更不可のインスタンスとして取得
    const MatVec::CVector_Blk& ret_instance_ = this->self->GetVector(iv);
    // コピーを取る
    //コピーコンストラクタは、ret_instance_.Len() == -1のとき失敗する
    //MatVec::CVector_Blk *ret_ptr_ = new MatVec::CVector_Blk(ret_instance_);
    MatVec::CVector_Blk *ret_ptr_ = DelFEM4NetMatVec::CVector_Blk::CreateNativeInstance(ret_instance_);
    // マネージドインスタンス生成
    return gcnew DelFEM4NetMatVec::CVector_Blk(ret_ptr_);
}

DelFEM4NetMatVec::CMatDia_BlkCrs^ CLinearSystem_RigidBody_CRS2::GetMatrix()
{
    // 変更不可のインスタンスしか取得できない
    const MatVec::CMatDia_BlkCrs& native_instance_ = this->self->GetMatrix();
    //  nativeインスタンスからマネージドクラスインスタンスを生成する（nativeインスタンスがコピーされる)
    // (コピーコンストラクタはshallow copyになるので、このコンストラクタを用いる)
    return gcnew DelFEM4NetMatVec::CMatDia_BlkCrs(native_instance_);
}

bool CLinearSystem_RigidBody_CRS2::UpdateValueOfRigidSystem(
        IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst, 
        double dt, double newmark_gamma, double newmark_beta, 
        bool is_first)
{
    //BUGFIX nativeインスタンスは新たに生成しない
    //std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    //リストのnativeインスタンスはマネージドクラスのnativeポインタが指すインスタンスのリファレンスを渡す →しかしpush_backでコピーが格納される
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector_NoCreate<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    // リストのポインタはマネージドクラスのnativeポインタをそのまま渡す
    std::vector<Rigid::CConstraint*> aConst_ = DelFEM4NetRigid::CConstraint::ListToInstancePtrVector_NoCreate<DelFEM4NetRigid::CConstraint, Rigid::CConstraint>(aConst);

    bool ret = this->self->UpdateValueOfRigidSystem(
        aRB_, aConst_, 
        dt, newmark_gamma, newmark_beta, 
        is_first);

    // BUGFIX: 変更されたインスタンスを戻す
    for (int i = 0; i < aRB_.size(); i++)
    {
        aRB[i] = nullptr;
        Rigid::CRigidBody3D *modified_= new Rigid::CRigidBody3D(aRB_[i]);
        aRB[i] = gcnew DelFEM4NetRigid::CRigidBody3D(modified_);
    }

    return ret;

}



/////////////////////////////////////////////////////////////////////////////////
// CPreconditioner_RigidBody_CRS2
/////////////////////////////////////////////////////////////////////////////////

CPreconditioner_RigidBody_CRS2::CPreconditioner_RigidBody_CRS2()
{
    this->self = new Ls::CPreconditioner_RigidBody_CRS2();
}

CPreconditioner_RigidBody_CRS2::CPreconditioner_RigidBody_CRS2(const CPreconditioner_RigidBody_CRS2% rhs)
{
    assert(false);
    this->self = NULL;
    /*
    const Ls::CPreconditioner_RigidBody_CRS2& rhs_instance_ = *(rhs.self);
    this->self = new Ls::CPreconditioner_RigidBody_CRS2(rhs_instance_);
    */
}

CPreconditioner_RigidBody_CRS2::CPreconditioner_RigidBody_CRS2(Ls::CPreconditioner_RigidBody_CRS2 *self)
{
    this->self = self;
}


CPreconditioner_RigidBody_CRS2::~CPreconditioner_RigidBody_CRS2()
{
    this->!CPreconditioner_RigidBody_CRS2();
}

CPreconditioner_RigidBody_CRS2::!CPreconditioner_RigidBody_CRS2()
{
    delete this->self;
}

Ls::CPreconditioner_RigidBody_CRS2 * CPreconditioner_RigidBody_CRS2::Self::get()
{
    return this->self;
}

void CPreconditioner_RigidBody_CRS2::SetLinearSystem(CLinearSystem_RigidBody_CRS2^ ls, int ilev)
{
    this->self->SetLinearSystem(*(ls->Self), ilev);
}

void CPreconditioner_RigidBody_CRS2::SetValue(CLinearSystem_RigidBody_CRS2^ ls)
{
    this->self->SetValue(*(ls->Self));
}

bool CPreconditioner_RigidBody_CRS2::Solve(DelFEM4NetMatVec::CVector_Blk^% vec )
{
    return this->self->Solve(*(vec->Self));
}


//////////////////////////////////////////////////////////////////////////////////////////
// CLinearSystemPreconditioner_RigidBody_CRS2
//////////////////////////////////////////////////////////////////////////////////////////
CLinearSystemPreconditioner_RigidBody_CRS2::CLinearSystemPreconditioner_RigidBody_CRS2(CLinearSystem_RigidBody_CRS2^ ls, 
        CPreconditioner_RigidBody_CRS2^ prec)
{
    this->self = new Ls::CLinearSystemPreconditioner_RigidBody_CRS2(*(ls->Self), *(prec->Self));
}

CLinearSystemPreconditioner_RigidBody_CRS2::CLinearSystemPreconditioner_RigidBody_CRS2()
{
    assert(false);
    //this->self = new Ls::CLinearSystemPreconditioner_RigidBody_CRS2();
    this->self = NULL;
}

CLinearSystemPreconditioner_RigidBody_CRS2::CLinearSystemPreconditioner_RigidBody_CRS2(const CLinearSystemPreconditioner_RigidBody_CRS2% rhs)
{
    assert(false);
}

CLinearSystemPreconditioner_RigidBody_CRS2::CLinearSystemPreconditioner_RigidBody_CRS2(Ls::CLinearSystemPreconditioner_RigidBody_CRS2 *self)
{
    assert(false);
}

LsSol::ILinearSystemPreconditioner_Sol * CLinearSystemPreconditioner_RigidBody_CRS2::SolSelf::get()
{
    return this->self;
}

Ls::CLinearSystemPreconditioner_RigidBody_CRS2 * CLinearSystemPreconditioner_RigidBody_CRS2::Self::get()
{
    return this->self;
}

CLinearSystemPreconditioner_RigidBody_CRS2::~CLinearSystemPreconditioner_RigidBody_CRS2()
{
    this->!CLinearSystemPreconditioner_RigidBody_CRS2();
}

CLinearSystemPreconditioner_RigidBody_CRS2::!CLinearSystemPreconditioner_RigidBody_CRS2()
{
    delete this->self;
}

//! ソルバに必要な作業ベクトルの数を得る
unsigned int CLinearSystemPreconditioner_RigidBody_CRS2::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

//! ソルバに必要な作業ベクトルの数を設定
bool CLinearSystemPreconditioner_RigidBody_CRS2::ReSizeTmpVecSolver(unsigned int size_new)
{
    return this->self->ReSizeTmpVecSolver(size_new);
}

double CLinearSystemPreconditioner_RigidBody_CRS2::DOT(int iv1, int iv2)
{
    return this->self->DOT(iv1, iv2);
}
bool CLinearSystemPreconditioner_RigidBody_CRS2::COPY(int iv1, int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystemPreconditioner_RigidBody_CRS2::SCAL(double alpha, int iv1)
{
    return this->self->SCAL(alpha, iv1);
}

bool CLinearSystemPreconditioner_RigidBody_CRS2::AXPY(double alpha, int iv1, int iv2)
{
    return this->self->AXPY(alpha, iv1, iv2);
}

bool CLinearSystemPreconditioner_RigidBody_CRS2::MATVEC(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MATVEC(alpha, iv1, beta, iv2);
}

bool CLinearSystemPreconditioner_RigidBody_CRS2::SolvePrecond(int iv)
{
    return this->self->SolvePrecond(iv);
}

