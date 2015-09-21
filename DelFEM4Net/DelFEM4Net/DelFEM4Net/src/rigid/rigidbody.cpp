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

#include "DelFEM4Net/rigid/rigidbody.h"
#include "DelFEM4Net/rigid/linearsystem_rigid.h"

using namespace DelFEM4NetRigid;

///////////////////////////////////////////////////////////////////////////////////////////////
// CRigidBody3D
///////////////////////////////////////////////////////////////////////////////////////////////
CRigidBody3D::CRigidBody3D()
{
    this->self = new Rigid::CRigidBody3D();
}

CRigidBody3D::CRigidBody3D(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CRigidBody3D::CRigidBody3D(const CRigidBody3D% rhs)
{
    const Rigid::CRigidBody3D& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CRigidBody3D(rhs_instance_);
}

CRigidBody3D::CRigidBody3D(Rigid::CRigidBody3D *self)
{
    this->self = self;
}

CRigidBody3D::~CRigidBody3D()
{
    this->!CRigidBody3D();
}

CRigidBody3D::!CRigidBody3D()
{
    delete this->self;
}

Rigid::CRigidBody3D * CRigidBody3D::Self::get()
{
    return this->self;
}

unsigned int CRigidBody3D::GetDOF()
{
    return this->self->GetDOF();
}

void CRigidBody3D::GetInvRotMatrix44(array<double>^% rot)
{
    assert(rot->Length == 16);
    pin_ptr<double> ptr = &rot[0];
    this->self->GetInvRotMatrix44((double*)ptr);
}

void CRigidBody3D::GetRotMatrix33(array<double>^% rot)
{
    assert(rot->Length == 9);
    pin_ptr<double> ptr = &rot[0];
    this->self->GetRotMatrix33((double*)ptr);
}

DelFEM4NetCom::CMatrix3^ CRigidBody3D::GetRotMatrix()
{
    Com::CMatrix3 instance_ = this->self->GetRotMatrix();
    Com::CMatrix3 *unmanaged = new Com::CMatrix3(instance_);
    DelFEM4NetCom::CMatrix3^ retManaged = gcnew DelFEM4NetCom::CMatrix3(unmanaged);
    return retManaged;
}

void CRigidBody3D::Clear()
{
    this->self->Clear();
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::GetPositionFromInital(DelFEM4NetCom::CVector3D^ vec)
{
    Com::CVector3D instance_ = this->self->GetPositionFromInital(*(vec->Self));
    Com::CVector3D *unmanaged = new Com::CVector3D(instance_);
    DelFEM4NetCom::CVector3D^ retManaged = gcnew DelFEM4NetCom::CVector3D(unmanaged);
    return retManaged;
}

void CRigidBody3D::AddRotation(array<double>^ rot)
{
    pin_ptr<double> ptr = &rot[0];
    this->self->AddRotation((double *)ptr);
}

void CRigidBody3D::UpdateSolution(array<double>^ upd,
    double dt, double newmark_gamma, double newmark_beta, 
    bool is_first_iter)
{
    pin_ptr<double> ptr = &upd[0];
    this->self->UpdateSolution((double *)ptr,
        dt, newmark_gamma, newmark_beta, 
        is_first_iter);
}

void CRigidBody3D::AddLinearSystem(
        DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int irb,
        double dt, double newmark_gamma, double newmark_beta,
        DelFEM4NetCom::CVector3D^ gravity, 
        bool is_first)
{
    this->self->AddLinearSystem(*(ls->LsSelf), irb,
        dt, newmark_gamma, newmark_beta,
        *(gravity->Self),
        is_first);
}


void CRigidBody3D::Set_Omega_Vector(double x, double y, double z)
{
    this->self->Omega.SetVector(x, y, z);
}

void CRigidBody3D::Set_dOmega_Vector(double x, double y, double z)
{
    this->self->dOmega.SetVector(x, y, z);
}

void CRigidBody3D::Set_velo_cg_Vector(double x, double y, double z)
{
    this->self->velo_cg.SetVector(x, y, z);
}

void CRigidBody3D::Set_disp_cg_Vector(double x, double y, double z)
{
    this->self->disp_cg.SetVector(x, y, z);
}

void CRigidBody3D::Set_acc_cg_Vector(double x, double y, double z)
{
    this->self->acc_cg.SetVector(x, y, z);
}

void CRigidBody3D::Set_ini_pos_cg_Vector(double x, double y, double z)
{
    this->self->ini_pos_cg.SetVector(x, y, z);
}

//double crv[3];
DelFEM4NetCom::DoubleArrayIndexer^ CRigidBody3D::crv::get()
{
    const int size = 3;
    double *ptr_ = &this->self->crv[0];
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::Omega::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->Omega);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::Omega::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->Omega = *(value->Self);
}


DelFEM4NetCom::CVector3D^ CRigidBody3D::dOmega::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->dOmega);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::dOmega::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->dOmega = *(value->Self);
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::velo_cg::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->velo_cg);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::velo_cg::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->velo_cg = *(value->Self);
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::disp_cg::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->disp_cg);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::disp_cg::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->disp_cg = *(value->Self);
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::acc_cg::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->acc_cg);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::acc_cg::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->acc_cg = *(value->Self);
}

DelFEM4NetCom::CVector3D^ CRigidBody3D::ini_pos_cg::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_cg);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CRigidBody3D::ini_pos_cg::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_cg = *(value->Self);
}

double CRigidBody3D::mass::get()
{
    return this->self->mass;
}

void CRigidBody3D::mass::set(double value)
{
    this->self->mass = value;
}

//double mineatia[3];
DelFEM4NetCom::DoubleArrayIndexer^ CRigidBody3D::mineatia::get()
{
    const int size = 3;
    double *ptr_ = &(this->self->mineatia[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}


///////////////////////////////////////////////////////////////////////////////////////////////
// CFix_Spherical
///////////////////////////////////////////////////////////////////////////////////////////////

CFix_Spherical::CFix_Spherical() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CFix_Spherical();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CFix_Spherical::CFix_Spherical(const CFix_Spherical% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CFix_Spherical& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CFix_Spherical(rhs_instance_);
}

CFix_Spherical::CFix_Spherical(unsigned int irb) : CConstraint()
{
    this->self = new Rigid::CFix_Spherical(irb);
}

CFix_Spherical::CFix_Spherical(Rigid::CFix_Spherical *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CFix_Spherical::~CFix_Spherical()
{
    this->!CFix_Spherical();
}

CFix_Spherical::!CFix_Spherical()
{
    delete this->self;
}

Rigid::CConstraint * CFix_Spherical::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CFix_Spherical * CFix_Spherical::Self::get()
{
    return this->self;
}

unsigned int CFix_Spherical::GetDOF()
{
    return this->self->GetDOF();
}

void CFix_Spherical::Clear()
{
    this->self->Clear();
}

void CFix_Spherical::SetIniPosFix(double x, double y, double z)
{
    this->self->SetIniPosFix(x, y, z);
}

void CFix_Spherical::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
        this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CFix_Spherical::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[3];
DelFEM4NetCom::DoubleArrayIndexer^ CFix_Spherical::lambda::get()
{
    const int size = 3;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CFix_Spherical::ini_pos_fix::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_fix);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CFix_Spherical::ini_pos_fix::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_fix = *(value->Self);
}


///////////////////////////////////////////////////////////////////////////////////////////////
// CFix_Hinge
///////////////////////////////////////////////////////////////////////////////////////////////

CFix_Hinge::CFix_Hinge() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CFix_Hinge();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CFix_Hinge::CFix_Hinge(const CFix_Hinge% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CFix_Hinge& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CFix_Hinge(rhs_instance_);
}

CFix_Hinge::CFix_Hinge(unsigned int irb) : CConstraint()
{
    this->self = new Rigid::CFix_Hinge(irb);
}

CFix_Hinge::CFix_Hinge(Rigid::CFix_Hinge *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CFix_Hinge::~CFix_Hinge()
{
    this->!CFix_Hinge();
}

CFix_Hinge::!CFix_Hinge()
{
    delete this->self;
}

Rigid::CConstraint * CFix_Hinge::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CFix_Hinge * CFix_Hinge::Self::get()
{
    return this->self;
}

unsigned int CFix_Hinge::GetDOF()
{
    return this->self->GetDOF();
}

void CFix_Hinge::Clear()
{
    this->self->Clear();
}

void CFix_Hinge::SetIniPosFix(double x, double y, double z)
{
    this->self->SetIniPosFix(x, y, z);
}

void CFix_Hinge::SetAxis(double ax, double ay, double az)
{
    this->self->SetAxis(ax, ay, az);
}

void CFix_Hinge::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
    this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CFix_Hinge::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[5];
DelFEM4NetCom::DoubleArrayIndexer^ CFix_Hinge::lambda::get()
{
    const int size = 5;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CFix_Hinge::ini_pos_fix::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_fix);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CFix_Hinge::ini_pos_fix::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_fix = *(value->Self);
}

//Com::CVector3D loc_coord[2];
DelFEM4NetCom::CVector3DArrayIndexer^ CFix_Hinge::loc_coord::get()
{
    const int size = 2;
    Com::CVector3D *ptr_ = &(this->self->loc_coord[0]);
    DelFEM4NetCom::CVector3DArrayIndexer^ indexer
        = gcnew DelFEM4NetCom::CVector3DArrayIndexer(size, ptr_);
    return indexer;
}


///////////////////////////////////////////////////////////////////////////////////////////////
// CFix_HingeRange
///////////////////////////////////////////////////////////////////////////////////////////////

CFix_HingeRange::CFix_HingeRange() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CFix_HingeRange();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CFix_HingeRange::CFix_HingeRange(const CFix_HingeRange% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CFix_HingeRange& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CFix_HingeRange(rhs_instance_);
}

CFix_HingeRange::CFix_HingeRange(unsigned int irb) : CConstraint()
{
    this->self = new Rigid::CFix_HingeRange(irb);
}

CFix_HingeRange::CFix_HingeRange(Rigid::CFix_HingeRange *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CFix_HingeRange::~CFix_HingeRange()
{
    this->!CFix_HingeRange();
}

CFix_HingeRange::!CFix_HingeRange()
{
    delete this->self;
}

Rigid::CConstraint * CFix_HingeRange::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CFix_HingeRange * CFix_HingeRange::Self::get()
{
    return this->self;
}

unsigned int CFix_HingeRange::GetDOF()
{
    return this->self->GetDOF();
}

void CFix_HingeRange::Clear()
{
    this->self->Clear();
}

void CFix_HingeRange::SetIniPosFix(double x, double y, double z)
{
    this->self->SetIniPosFix(x, y, z);
}

void CFix_HingeRange::SetAxis(double ax, double ay, double az)
{
    this->self->SetAxis(ax, ay, az);
}

void CFix_HingeRange::SetRange(double min_t, double max_t)
{
    this->self->SetRange(min_t, max_t);
}

void CFix_HingeRange::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
    this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CFix_HingeRange::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[6];
DelFEM4NetCom::DoubleArrayIndexer^ CFix_HingeRange::lambda::get()
{
    const int size = 6;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CFix_HingeRange::ini_pos_fix::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_fix);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CFix_HingeRange::ini_pos_fix::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_fix = *(value->Self);
}

//Com::CVector3D loc_coord[2];
DelFEM4NetCom::CVector3DArrayIndexer^ CFix_HingeRange::loc_coord::get()
{
    const int size = 2;
    Com::CVector3D *ptr_ = &(this->self->loc_coord[0]);
    DelFEM4NetCom::CVector3DArrayIndexer^ indexer
        = gcnew DelFEM4NetCom::CVector3DArrayIndexer(size, ptr_);
    return indexer;
}

double CFix_HingeRange::min_t::get()
{
    return this->self->min_t;
}

void CFix_HingeRange::min_t::set(double value)
{
    this->self->min_t = value;
}

double CFix_HingeRange::max_t::get()
{
    return this->self->max_t;
}

void CFix_HingeRange::max_t::set(double value)
{
    this->self->max_t = value;
}


///////////////////////////////////////////////////////////////////////////////////////////////
// CJoint_Spherical
///////////////////////////////////////////////////////////////////////////////////////////////

CJoint_Spherical::CJoint_Spherical() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CJoint_Spherical();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CJoint_Spherical::CJoint_Spherical(const CJoint_Spherical% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CJoint_Spherical& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CJoint_Spherical(rhs_instance_);
}

CJoint_Spherical::CJoint_Spherical(unsigned int irb0, unsigned int irb1) : CConstraint()
{
    this->self = new Rigid::CJoint_Spherical(irb0, irb1);
}

CJoint_Spherical::CJoint_Spherical(Rigid::CJoint_Spherical *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CJoint_Spherical::~CJoint_Spherical()
{
    this->!CJoint_Spherical();
}

CJoint_Spherical::!CJoint_Spherical()
{
    delete this->self;
}

Rigid::CConstraint * CJoint_Spherical::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CJoint_Spherical * CJoint_Spherical::Self::get()
{
    return this->self;
}

unsigned int CJoint_Spherical::GetDOF()
{
    return this->self->GetDOF();
}

void CJoint_Spherical::Clear()
{
    this->self->Clear();
}

void CJoint_Spherical::SetIniPosJoint(double x, double y, double z)
{
    this->self->SetIniPosJoint(x, y, z);
}

void CJoint_Spherical::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
    this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CJoint_Spherical::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[3];
DelFEM4NetCom::DoubleArrayIndexer^ CJoint_Spherical::lambda::get()
{
    const int size = 3;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CJoint_Spherical::ini_pos_joint::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_joint);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CJoint_Spherical::ini_pos_joint::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_joint = *(value->Self);
}

///////////////////////////////////////////////////////////////////////////////////////////////
// CJoint_Hinge
///////////////////////////////////////////////////////////////////////////////////////////////

CJoint_Hinge::CJoint_Hinge() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CJoint_Hinge();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CJoint_Hinge::CJoint_Hinge(const CJoint_Hinge% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CJoint_Hinge& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CJoint_Hinge(rhs_instance_);
}

CJoint_Hinge::CJoint_Hinge(unsigned int irb0, unsigned int irb1) : CConstraint()
{
    this->self = new Rigid::CJoint_Hinge(irb0, irb1);
}

CJoint_Hinge::CJoint_Hinge(Rigid::CJoint_Hinge *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CJoint_Hinge::~CJoint_Hinge()
{
    this->!CJoint_Hinge();
}

CJoint_Hinge::!CJoint_Hinge()
{
    delete this->self;
}

Rigid::CConstraint * CJoint_Hinge::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CJoint_Hinge * CJoint_Hinge::Self::get()
{
    return this->self;
}

unsigned int CJoint_Hinge::GetDOF()
{
    return this->self->GetDOF();
}

void CJoint_Hinge::Clear()
{
    this->self->Clear();
}

void CJoint_Hinge::SetIniPosJoint(double x, double y, double z)
{
    this->self->SetIniPosJoint(x, y, z);
}

void CJoint_Hinge::SetAxis(double ax, double ay, double az)
{
    this->self->SetAxis(ax, ay, az);
}

void CJoint_Hinge::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
    this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CJoint_Hinge::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[5];
DelFEM4NetCom::DoubleArrayIndexer^ CJoint_Hinge::lambda::get()
{
    const int size = 5;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CJoint_Hinge::ini_pos_joint::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_joint);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CJoint_Hinge::ini_pos_joint::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_joint = *(value->Self);
}

//Com::CVector3D loc_coord[2];
DelFEM4NetCom::CVector3DArrayIndexer^ CJoint_Hinge::loc_coord::get()
{
    const int size = 2;
    Com::CVector3D *ptr_ = &(this->self->loc_coord[0]);
    DelFEM4NetCom::CVector3DArrayIndexer^ indexer
        = gcnew DelFEM4NetCom::CVector3DArrayIndexer(size, ptr_);
    return indexer;
}


///////////////////////////////////////////////////////////////////////////////////////////////
// CJoint_HingeRange
///////////////////////////////////////////////////////////////////////////////////////////////

CJoint_HingeRange::CJoint_HingeRange() : CConstraint()
{
    assert(false);
    //this->self = new Rigid::CJoint_HingeRange();  // 引数なしコンストラクタは定義されていない
    this->self = NULL;
}

CJoint_HingeRange::CJoint_HingeRange(const CJoint_HingeRange% rhs) : CConstraint()/*CConstraint(rhs)*/
{
    const Rigid::CJoint_HingeRange& rhs_instance_ = *(rhs.self);
    this->self = new Rigid::CJoint_HingeRange(rhs_instance_);
}

CJoint_HingeRange::CJoint_HingeRange(unsigned int irb0, unsigned int irb1) : CConstraint()
{
    this->self = new Rigid::CJoint_HingeRange(irb0, irb1);
}

CJoint_HingeRange::CJoint_HingeRange(Rigid::CJoint_HingeRange *self) : CConstraint() /*CConstraint(self)*/
{
    this->self = self;
}

CJoint_HingeRange::~CJoint_HingeRange()
{
    this->!CJoint_HingeRange();
}

CJoint_HingeRange::!CJoint_HingeRange()
{
    delete this->self;
}

Rigid::CConstraint * CJoint_HingeRange::ConstraintSelf::get()
{
    return this->self;
}

Rigid::CJoint_HingeRange * CJoint_HingeRange::Self::get()
{
    return this->self;
}

unsigned int CJoint_HingeRange::GetDOF()
{
    return this->self->GetDOF();
}

void CJoint_HingeRange::Clear()
{
    this->self->Clear();
}

void CJoint_HingeRange::SetIniPosJoint(double x, double y, double z)
{
    this->self->SetIniPosJoint(x, y, z);
}

void CJoint_HingeRange::SetAxis(double ax, double ay, double az)
{
    this->self->SetAxis(ax, ay, az);
}

void CJoint_HingeRange::SetRange(double min_t, double max_t)
{
    this->self->SetRange(min_t, max_t);
}

void CJoint_HingeRange::UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta)
{
    pin_ptr<double> upd_ = &upd[0];
    this->self->UpdateSolution((double *)upd_,
        dt, newmark_gamma, newmark_beta);
}

void CJoint_HingeRange::AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial)
{
    std::vector<Rigid::CRigidBody3D> aRB_ = DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetRigid::CRigidBody3D, Rigid::CRigidBody3D>(aRB);
    this->self->AddLinearSystem(*(ls->LsSelf), icst,
        dt, newmark_gamma, newmark_beta,
        aRB_, 
        is_initial);
}

//double lambda[6];
DelFEM4NetCom::DoubleArrayIndexer^ CJoint_HingeRange::lambda::get()
{
    const int size = 6;
    double *ptr_ = &(this->self->lambda[0]);
    DelFEM4NetCom::DoubleArrayIndexer^ indexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, ptr_);
    return indexer;
}

DelFEM4NetCom::CVector3D^ CJoint_HingeRange::ini_pos_joint::get()
{
    Com::CVector3D *unmanaged = new Com::CVector3D(this->self->ini_pos_joint);
    return gcnew DelFEM4NetCom::CVector3D(unmanaged);
}

void CJoint_HingeRange::ini_pos_joint::set(DelFEM4NetCom::CVector3D^ value)
{
    this->self->ini_pos_joint = *(value->Self);
}

//Com::CVector3D loc_coord[2];
DelFEM4NetCom::CVector3DArrayIndexer^ CJoint_HingeRange::loc_coord::get()
{
    const int size = 2;
    Com::CVector3D *ptr_ = &(this->self->loc_coord[0]);
    DelFEM4NetCom::CVector3DArrayIndexer^ indexer
        = gcnew DelFEM4NetCom::CVector3DArrayIndexer(size, ptr_);
    return indexer;
}

double CJoint_HingeRange::min_t::get()
{
    return this->self->min_t;
}

void CJoint_HingeRange::min_t::set(double value)
{
    this->self->min_t = value;
}

double CJoint_HingeRange::max_t::get()
{
    return this->self->max_t;
}

void CJoint_HingeRange::max_t::set(double value)
{
    this->self->max_t = value;
}



