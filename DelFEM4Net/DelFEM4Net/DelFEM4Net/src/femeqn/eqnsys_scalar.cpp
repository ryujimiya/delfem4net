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
// eqnsys_scalar.cpp : スカラー型の連立方程式クラス
// (Fem::Eqn::CEqnSystem_Scalar2D,Fem::Eqn::CEqn_Scalar2D,Fem::Eqn::CEqn_Scalar3D)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )   // C4786なんて表示すんな( ﾟДﾟ)ｺﾞﾙｧ
#endif
#define for if(0); else for

#include "DelFEM4Net/stub/clr_stub.h"

#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/field_value_setter.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/ls/solver_ls_iter.h"
#include "DelFEM4Net/ls/preconditioner.h"

#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femls/linearsystem_fieldsave.h"
//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"

//#include "delfem/DelFEM4Net/eqn_diffusion.h"
//#include "delfem/DelFEM4Net/eqn_poisson.h"
//#include "delfem/DelFEM4Net/eqn_advection_diffusion.h"

#include "DelFEM4Net/eqnsys_scalar.h"

using namespace DelFEM4NetFem::Eqn;
using namespace DelFEM4NetFem::Field;
using namespace DelFEM4NetFem::Ls;


////////////////////////////////////////////////////////////////
// CEqn_Scalar2D
////////////////////////////////////////////////////////////////
/*
CEqn_Scalar2D::CEqn_Scalar2D()
{
    this->self = new Fem::Eqn::CEqn_Scalar2D();
}
*/

CEqn_Scalar2D::CEqn_Scalar2D(const CEqn_Scalar2D% rhs)
{
    const Fem::Eqn::CEqn_Scalar2D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある? --> nativeライブラリ内では普通にoperator=でコピーしているので復活させる
    this->self = new Fem::Eqn::CEqn_Scalar2D(rhs_instance_);
}

CEqn_Scalar2D::CEqn_Scalar2D(unsigned int id_ea, unsigned int id_field_val)
{
    this->self = new Fem::Eqn::CEqn_Scalar2D(id_ea, id_field_val);
}

CEqn_Scalar2D::CEqn_Scalar2D(Fem::Eqn::CEqn_Scalar2D *self)
{
    this->self = self;
}

CEqn_Scalar2D::~CEqn_Scalar2D()
{
    this->!CEqn_Scalar2D();
}

CEqn_Scalar2D::!CEqn_Scalar2D()
{
    delete this->self;
}

Fem::Eqn::CEqn_Scalar2D * CEqn_Scalar2D::Self::get()
{
    return this->self;
}

void CEqn_Scalar2D::SetAlpha(double alpha )
{
    this->self->SetAlpha(alpha);
}

void CEqn_Scalar2D::SetCapacity(double capa)
{
    this->self->SetCapacity(capa);
}

void CEqn_Scalar2D::SetSource(double source)
{
    this->self->SetSource(source);
}

void CEqn_Scalar2D::SetAdvection(unsigned int id_field_advec)
{
    this->self->SetAdvection(id_field_advec);
}

double CEqn_Scalar2D::GetAlpha()
{
    return this->self->GetAlpha();
}

double CEqn_Scalar2D::GetCapacity()
{
    return this->self->GetCapacity();
}

double CEqn_Scalar2D::GetSource()
{
    return this->self->GetSource();
}

unsigned int CEqn_Scalar2D::GetIdEA()
{
    return this->self->GetIdEA();
}
bool CEqn_Scalar2D::IsAdvection()
{
    return this->self->IsAdvection();
}

bool CEqn_Scalar2D::AddLinSys( DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys(*(ls->Self), *(world->Self));
}

bool CEqn_Scalar2D::AddLinSys_Newmark( double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        bool is_ax_sym,
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_Newmark(dt, gamma, *(ls->Self), is_ax_sym, *(world->Self));
}

bool CEqn_Scalar2D::AddLinSys_Save( DelFEM4NetFem::Ls::CLinearSystem_Save^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_Save(*(ls->Self), *(world->Self));
}

bool CEqn_Scalar2D::AddLinSys_SaveKDiaC( DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_SaveKDiaC(*(ls->Self), *(world->Self));
}

////////////////////////////////////////////////////////////////
// CEqnSystem_Scalar2D
////////////////////////////////////////////////////////////////

CEqnSystem_Scalar2D::CEqnSystem_Scalar2D() : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Scalar2D();
}

CEqnSystem_Scalar2D::CEqnSystem_Scalar2D(const CEqnSystem_Scalar2D% rhs) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    const Fem::Eqn::CEqnSystem_Scalar2D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Fem::Eqn::CEqnSystem_Scalar2D(rhs_instance_);
    assert(false);
}

/*nativeライブラリで実装されていない
CEqnSystem_Scalar2D::CEqnSystem_Scalar2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Scalar2D(id_field_val, *(world->Self));
}
*/

CEqnSystem_Scalar2D::CEqnSystem_Scalar2D(Fem::Eqn::CEqnSystem_Scalar2D *self)  : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = self;
}

CEqnSystem_Scalar2D::~CEqnSystem_Scalar2D()
{
    this->!CEqnSystem_Scalar2D();
}

CEqnSystem_Scalar2D::!CEqnSystem_Scalar2D()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqnSystem_Scalar2D::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqnSystem_Scalar2D * CEqnSystem_Scalar2D::Self::get()
{
    return this->self;
}

bool CEqnSystem_Scalar2D::SetDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain_Field(id_field, *(world->Self));
}

bool CEqnSystem_Scalar2D::SetDomain_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain_FieldElemAry(id_field, id_ea, *(world->Self));
}

bool CEqnSystem_Scalar2D::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

bool CEqnSystem_Scalar2D::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqnSystem_Scalar2D::AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqnSystem_Scalar2D::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    const std::vector<unsigned int>& aIdEA_ = DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA);
    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqnSystem_Scalar2D::ClearFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqnSystem_Scalar2D::ClearFixElemAry()
{
    this->self->ClearFixElemAry();
}

unsigned int CEqnSystem_Scalar2D::GetIdField_Value()
{
    return this->self->GetIdField_Value();
}

bool CEqnSystem_Scalar2D::SetEquation(CEqn_Scalar2D^ eqn )
{
    return this->self->SetEquation(*(eqn->Self));
}

CEqn_Scalar2D^ CEqnSystem_Scalar2D::GetEquation(unsigned int id_ea)
{
    const Fem::Eqn::CEqn_Scalar2D& ret_instance_ = this->self->GetEquation(id_ea);
    Fem::Eqn::CEqn_Scalar2D *ret_ = new Fem::Eqn::CEqn_Scalar2D(ret_instance_);  // shallow copy???
    
    return gcnew CEqn_Scalar2D(ret_);
}

void CEqnSystem_Scalar2D::SetAlpha(double alpha)
{
    this->self->SetAlpha(alpha);
}

void CEqnSystem_Scalar2D::SetCapacity(double capa)
{
    this->self->SetCapacity(capa);
}

void CEqnSystem_Scalar2D::SetSource(double source)
{
    this->self->SetSource(source);
}

void CEqnSystem_Scalar2D::SetStationary(bool is_stat)
{
    this->self->SetStationary(is_stat);
}

void CEqnSystem_Scalar2D::SetAdvection(unsigned int id_field_advec)
{
    this->self->SetAdvection(id_field_advec);
}

void CEqnSystem_Scalar2D::SetAxialSymmetry(bool is_ax_sym)
{
    this->self->SetAxialSymmetry(is_ax_sym);
}

void CEqnSystem_Scalar2D::SetSaveStiffMat(bool is_save)
{
    this->self->SetSaveStiffMat(is_save);
}



////////////////////////////////////////////////////////////////
// CEqn_Scalar3D
////////////////////////////////////////////////////////////////

CEqn_Scalar3D::CEqn_Scalar3D() : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqn_Scalar3D();
}

CEqn_Scalar3D::CEqn_Scalar3D(const CEqn_Scalar3D% rhs) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    const Fem::Eqn::CEqn_Scalar3D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Fem::Eqn::CEqn_Scalar3D(rhs_instance_);
    assert(false);
}

/*nativeライブラリで実装されていない
CEqn_Scalar3D::CEqn_Scalar3D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqn_Scalar3D(id_field_val, *(world->Self));
}
*/

CEqn_Scalar3D::CEqn_Scalar3D(Fem::Eqn::CEqn_Scalar3D *self) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = self;
}

CEqn_Scalar3D::~CEqn_Scalar3D()
{
    this->!CEqn_Scalar3D();
}

CEqn_Scalar3D::!CEqn_Scalar3D()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqn_Scalar3D::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqn_Scalar3D * CEqn_Scalar3D::Self::get()
{
    return this->self;
}

bool CEqn_Scalar3D::SetDomain(unsigned int id_base, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain(id_base, *(world->Self));
}

bool CEqn_Scalar3D::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

bool CEqn_Scalar3D::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqn_Scalar3D::AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqn_Scalar3D::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    const std::vector<unsigned int>& aIdEA_ = DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA);
    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqn_Scalar3D::ClearFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqn_Scalar3D::ClearFixElemAry()
{
    this->self->ClearFixElemAry();
}

unsigned int CEqn_Scalar3D::GetIdField_Value()
{
    return this->self->GetIdField_Value();
}

void CEqn_Scalar3D::SetAlpha(double alpha)
{
    this->self->SetAlpha(alpha);
}

void CEqn_Scalar3D::SetSource(double source)
{
    this->self->SetSource(source);
}

