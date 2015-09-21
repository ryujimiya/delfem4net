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
// eqnsys_fluid.cpp : 
// 流体の連立方程式クラス(CEqnSystem_Fluid2D,CEqn_Fluid2D,CEqn_Fluid3D)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "DelFEM4Net/stub/clr_stub.h"

#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/field_value_setter.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/ls/preconditioner.h"
#include "DelFEM4Net/ls/solver_ls_iter.h"

#include "DelFEM4Net/femls/linearsystem_field.h"

//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"

//#include "DelFEM4Net/femeqn/eqn_stokes.h"
//#include "DelFEM4Net/femeqn/eqn_navier_stokes.h"

#include "DelFEM4Net/eqnsys_fluid.h"

using namespace DelFEM4NetFem::Eqn;
//using namespace DelFEM4NetFem::Field;
//using namespace DelFEM4NetFem::Ls;

////////////////////////////////////////////////////////////////
// CEqn_Fluid2D
////////////////////////////////////////////////////////////////
/*
CEqn_Fluid2D::CEqn_Fluid2D()
{
    this->self = new Fem::Eqn::CEqn_Fluid2D();
}
*/

CEqn_Fluid2D::CEqn_Fluid2D(const CEqn_Fluid2D% rhs)
{
    const Fem::Eqn::CEqn_Fluid2D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある? --> nativeライブラリ内では普通にoperator=でコピーしているので復活させる
    this->self = new Fem::Eqn::CEqn_Fluid2D(rhs_instance_);
}

CEqn_Fluid2D::CEqn_Fluid2D(unsigned int id_ea, unsigned int id_velo, unsigned int id_press)
{
    this->self = new Fem::Eqn::CEqn_Fluid2D(id_ea, id_velo, id_press);
}

CEqn_Fluid2D::CEqn_Fluid2D(Fem::Eqn::CEqn_Fluid2D *self)
{
    this->self = self;
}

CEqn_Fluid2D::~CEqn_Fluid2D()
{
    this->!CEqn_Fluid2D();
}

CEqn_Fluid2D::!CEqn_Fluid2D()
{
    delete this->self;
}

Fem::Eqn::CEqn_Fluid2D * CEqn_Fluid2D::Self::get()
{
    return this->self;
}

bool CEqn_Fluid2D::IsNavierStokes()
{
    return this->self->IsNavierStokes();
}

void CEqn_Fluid2D::SetRho(double rho)
{
    this->self->SetRho(rho);
}

double CEqn_Fluid2D::GetRho()
{
    return this->self->GetRho();
}

void CEqn_Fluid2D::SetMyu(double myu)
{
    this->self->SetMyu(myu);
}

double CEqn_Fluid2D::GetMyu()
{
    return this->self->GetMyu();
}

void CEqn_Fluid2D::SetBodyForce(double g_x, double g_y)
{
    this->self->SetBodyForce(g_x, g_y);
}

void CEqn_Fluid2D::SetStokes()
{
    this->self->SetStokes();
}

void CEqn_Fluid2D::SetNavierStokes()
{
    this->self->SetNavierStokes();
}

void CEqn_Fluid2D::SetNavierStokesALE(unsigned int id_field_msh_velo)
{
    this->self->SetNavierStokesALE(id_field_msh_velo);
}

bool CEqn_Fluid2D::AddLinSys(DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys(*(ls->Self), *(world->Self));
}

bool CEqn_Fluid2D::AddLinSys_NewmarkBetaAPrime( double dt, double gamma, double beta, bool is_initial, 
    DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_NewmarkBetaAPrime(dt, gamma, beta, is_initial,
        *(ls->Self), *(world->Self));
}

unsigned int CEqn_Fluid2D::GetIdEA()
{
    return this->self->GetIdEA();
}

void CEqn_Fluid2D::SetIdEA(unsigned int id_ea)
{
    this->self->SetIdEA(id_ea);
}

void CEqn_Fluid2D::SetIdFieldVelocity(unsigned int id_field_velo)
{
    this->self->SetIdFieldVelocity(id_field_velo);
}

void CEqn_Fluid2D::SetIdFieldPressure(unsigned int id_field_press)
{
    this->self->SetIdFieldPressure(id_field_press);
}


////////////////////////////////////////////////////////////////
// CEqnSystem_Fluid2D
////////////////////////////////////////////////////////////////

CEqnSystem_Fluid2D::CEqnSystem_Fluid2D() : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Fluid2D();
}

CEqnSystem_Fluid2D::CEqnSystem_Fluid2D(const CEqnSystem_Fluid2D% rhs) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    const Fem::Eqn::CEqnSystem_Fluid2D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Fem::Eqn::CEqnSystem_Fluid2D(rhs_instance_);
    assert(false);
}

/*nativeクラスに実装なし
CEqnSystem_Fluid2D::CEqnSystem_Fluid2D(DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Fluid2D(*(world->Self));
}
*/

CEqnSystem_Fluid2D::CEqnSystem_Fluid2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Fluid2D(id_field_val, *(world->Self));
}

CEqnSystem_Fluid2D::CEqnSystem_Fluid2D(Fem::Eqn::CEqnSystem_Fluid2D *self) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = self;
}

CEqnSystem_Fluid2D::~CEqnSystem_Fluid2D()
{
    this->!CEqnSystem_Fluid2D();
}

CEqnSystem_Fluid2D::!CEqnSystem_Fluid2D()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqnSystem_Fluid2D::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqnSystem_Fluid2D * CEqnSystem_Fluid2D::Self::get()
{
    return this->self;
}

////////////////
// virtual関数
bool CEqnSystem_Fluid2D::UpdateDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->UpdateDomain_Field(id_field, *(world->Self));
}

bool CEqnSystem_Fluid2D::UpdateDomain_FieldVeloPress(unsigned int id_base_field_velo, unsigned int id_field_press, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->UpdateDomain_FieldVeloPress(id_base_field_velo, id_field_press, *(world->Self));
}

bool CEqnSystem_Fluid2D::UpdateDomain_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->UpdateDomain_FieldElemAry(id_field, id_ea, *(world->Self));
}


bool CEqnSystem_Fluid2D::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqnSystem_Fluid2D::AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqnSystem_Fluid2D::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    const std::vector<unsigned int>& aIdEA_ = DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA);
    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqnSystem_Fluid2D::ClearFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqnSystem_Fluid2D::ClearFixElemAry()
{
    this->self->ClearFixElemAry();
}

bool CEqnSystem_Fluid2D::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

////////////////
// 非virtual
unsigned int CEqnSystem_Fluid2D::GetIdField_Velo()
{
    return this->self->GetIdField_Velo();
}

unsigned int CEqnSystem_Fluid2D::GetIdField_Press()
{
    return this->self->GetIdField_Press();
}

void CEqnSystem_Fluid2D::SetIsStationary(bool is_stat)
{
    this->self->SetIsStationary(is_stat);
}

bool CEqnSystem_Fluid2D::GetIsStationary()
{
    return this->self->GetIsStationary();
}

void CEqnSystem_Fluid2D::Clear()
{
    this->self->Clear();
}

IList<unsigned int>^ CEqnSystem_Fluid2D::GetAry_EqnIdEA()
{
    const std::vector<unsigned int>& vec = this->self->GetAry_EqnIdEA();
    
    return DelFEM4NetCom::ClrStub::VectorToList(vec);
}

CEqn_Fluid2D^ CEqnSystem_Fluid2D::GetEquation(unsigned int id_ea)
{
    const Fem::Eqn::CEqn_Fluid2D& ret_instance_ = this->self->GetEquation(id_ea);
    Fem::Eqn::CEqn_Fluid2D *ret = new Fem::Eqn::CEqn_Fluid2D(ret_instance_);
    
    return gcnew CEqn_Fluid2D(ret);
}

bool CEqnSystem_Fluid2D::SetEquation( CEqn_Fluid2D^ eqn )
{
    return this->self->SetEquation(*(eqn->Self));
}

void CEqnSystem_Fluid2D::UpdateIdEA( IList<unsigned int>^ map_ea2ea )
{
    const std::vector<unsigned int>& map_ea2ea_ = DelFEM4NetCom::ClrStub::ListToVector(map_ea2ea);
    
    this->self->UpdateIdEA(map_ea2ea_);
}

void CEqnSystem_Fluid2D::SetNavierStokes()
{
    this->self->SetNavierStokes();
}

void CEqnSystem_Fluid2D::SetNavierStokesALE(unsigned int id_field_msh_velo)
{
    this->self->SetNavierStokesALE(id_field_msh_velo);
}

void CEqnSystem_Fluid2D::SetForceField(unsigned int id_field_force)
{
    this->self->SetForceField(id_field_force);
}

void CEqnSystem_Fluid2D::SetInterpolationBubble()
{
    this->self->SetInterpolationBubble();
}

void CEqnSystem_Fluid2D::UnSetInterpolationBubble()
{
    this->self->UnSetInterpolationBubble();
}

void CEqnSystem_Fluid2D::SetRho(double rho)
{
    this->self->SetRho(rho);
}

void CEqnSystem_Fluid2D::SetMyu(double myu)
{
    this->self->SetMyu(myu);
}

void CEqnSystem_Fluid2D::SetStokes()
{
    this->self->SetStokes();
}


////////////////////////////////////////////////////////////////
// CEqn_Fluid3D
////////////////////////////////////////////////////////////////

CEqn_Fluid3D::CEqn_Fluid3D() : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqn_Fluid3D();
}

CEqn_Fluid3D::CEqn_Fluid3D(const CEqn_Fluid3D% rhs) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    const Fem::Eqn::CEqn_Fluid3D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Fem::Eqn::CEqn_Fluid3D(rhs_instance_);
    assert(false);
}

/*nativeクラスに実装なし
CEqn_Fluid3D::CEqn_Fluid3D(DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqn_Fluid3D(*(world->Self));
}
*/

CEqn_Fluid3D::CEqn_Fluid3D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqn_Fluid3D(id_field_val, *(world->Self));
}

CEqn_Fluid3D::CEqn_Fluid3D(Fem::Eqn::CEqn_Fluid3D *self) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = self;
}

CEqn_Fluid3D::~CEqn_Fluid3D()
{
    this->!CEqn_Fluid3D();
}

CEqn_Fluid3D::!CEqn_Fluid3D()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqn_Fluid3D::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqn_Fluid3D * CEqn_Fluid3D::Self::get()
{
    return this->self;
}


bool CEqn_Fluid3D::SetDomain(unsigned int id_base, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain(id_base, *(world->Self));
}

bool CEqn_Fluid3D::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

bool CEqn_Fluid3D::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqn_Fluid3D::AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqn_Fluid3D::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    const std::vector<unsigned int>& aIdEA_ = DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA);
    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqn_Fluid3D::ClearFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqn_Fluid3D::ClearFixElemAry()
{
    this->self->ClearFixElemAry();
}

void CEqn_Fluid3D::SetGravitation(double g_x, double g_y, double g_z)
{
    this->self->SetGravitation(g_x, g_y, g_z);
}

unsigned int CEqn_Fluid3D::GetIDField_Velo()
{
    return this->self->GetIDField_Velo();
}

unsigned int CEqn_Fluid3D::GetIDField_Press()
{
    return this->self->GetIDField_Press();
}
