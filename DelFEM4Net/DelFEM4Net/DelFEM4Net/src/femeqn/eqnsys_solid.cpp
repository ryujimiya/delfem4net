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

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/field_value_setter.h"

#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femls/linearsystem_fieldsave.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/ls/preconditioner.h"
#include "DelFEM4Net/ls/solver_ls_iter.h"
//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"
//#include "DelFEM4Net/femeqn/eqn_linear_solid2d.h"
//#include "DelFEM4Net/femeqn/eqn_linear_solid3d.h"
//#include "DelFEM4Net/femeqn/eqn_st_venant.h"

#include "DelFEM4Net/eqnsys_solid.h"

using namespace DelFEM4NetFem::Eqn;
using namespace DelFEM4NetFem::Field;
using namespace DelFEM4NetFem::Ls;
using namespace DelFEM4NetMatVec;


////////////////////////////////////////////////////////////////
// CEqn_Solid3D_Linear
////////////////////////////////////////////////////////////////

CEqn_Solid3D_Linear::CEqn_Solid3D_Linear() : CEqnSystem()
{
    this->self = new Fem::Eqn::CEqn_Solid3D_Linear();
}

CEqn_Solid3D_Linear::CEqn_Solid3D_Linear(const CEqn_Solid3D_Linear% rhs) : CEqnSystem()/*CEqnSystem(rhs)*/
{
    const Fem::Eqn::CEqn_Solid3D_Linear& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Fem::Eqn::CEqn_Solid3D_Linear(rhs_instance_);
    assert(false);
}

/* nativeなライブラリで実装されていない
CEqn_Solid3D_Linear::CEqn_Solid3D_Linear(DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem()
{
    this->self = new Fem::Eqn::CEqn_Solid3D_Linear(*(world->Self));
}
*/

CEqn_Solid3D_Linear::CEqn_Solid3D_Linear(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem()
{
    this->self = new Fem::Eqn::CEqn_Solid3D_Linear(id_field_val, *(world->Self));
}

CEqn_Solid3D_Linear::CEqn_Solid3D_Linear(Fem::Eqn::CEqn_Solid3D_Linear *self) : CEqnSystem() /*CEqnSystem(self)*/
{
    this->self = self;
}

CEqn_Solid3D_Linear::~CEqn_Solid3D_Linear()
{
    this->!CEqn_Solid3D_Linear();
}

CEqn_Solid3D_Linear::!CEqn_Solid3D_Linear()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqn_Solid3D_Linear::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqn_Solid3D_Linear * CEqn_Solid3D_Linear::Self::get()
{
    return this->self;
}

////////////////////////////////////////////////////////////////
// 仮想化可能関数
bool CEqn_Solid3D_Linear::SetDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain_Field(id_field, *(world->Self));
}

bool CEqn_Solid3D_Linear::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

////////////////////////////////////////////////////////////////
// 固定境界条件を追加&削除する
bool CEqn_Solid3D_Linear::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqn_Solid3D_Linear::AddFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqn_Solid3D_Linear::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    std::vector<unsigned int> aIdEA_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA, aIdEA_);

    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqn_Solid3D_Linear::ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqn_Solid3D_Linear::ClearFixElemAry()
{
    return this->self->ClearFixElemAry();
}

unsigned int CEqn_Solid3D_Linear::GetIdField_Disp()
{
    return this->self->GetIdField_Disp();
}

//////////////////////////////////////////////////////////////////////////////
// 非virtual

void CEqn_Solid3D_Linear::SetYoungPoisson( double young, double poisson )
{
    this->self->SetYoungPoisson(young, poisson);
}

void CEqn_Solid3D_Linear::SetGravitation( double g_x, double g_y, double g_z )
{
    this->self->SetGravitation(g_x, g_y, g_z);
}

void CEqn_Solid3D_Linear::SetGeometricalNonLinear()
{
    this->self->SetGeometricalNonLinear();
}

void CEqn_Solid3D_Linear::UnSetGeometricalNonLinear()
{
    this->self->UnSetGeometricalNonLinear();
}

void CEqn_Solid3D_Linear::SetSaveStiffMat()
{
    this->self->SetSaveStiffMat();
}

void CEqn_Solid3D_Linear::UnSetSaveStiffMat()
{
    this->self->UnSetSaveStiffMat();
}

void CEqn_Solid3D_Linear::SetStationary()
{
    this->self->SetStationary();
}

void CEqn_Solid3D_Linear::UnSetStationary()
{
    this->self->UnSetStationary();
}



////////////////////////////////////////////////////////////////
// CEqn_Solid2D
////////////////////////////////////////////////////////////////

CEqn_Solid2D::CEqn_Solid2D()
{
    this->self = new Fem::Eqn::CEqn_Solid2D();
}

CEqn_Solid2D::CEqn_Solid2D(const CEqn_Solid2D% rhs)
{
    const Fem::Eqn::CEqn_Solid2D& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Eqn::CEqn_Solid2D(rhs_instance_);
}

CEqn_Solid2D::CEqn_Solid2D(unsigned int id_ea, unsigned int id_field_disp)
{
    this->self = new Fem::Eqn::CEqn_Solid2D(id_ea, id_field_disp);
}

CEqn_Solid2D::CEqn_Solid2D(Fem::Eqn::CEqn_Solid2D *self)
{
    this->self = self;
}

CEqn_Solid2D::~CEqn_Solid2D()
{
    this->!CEqn_Solid2D();
}

CEqn_Solid2D::!CEqn_Solid2D()
{
    delete this->self;
}

Fem::Eqn::CEqn_Solid2D * CEqn_Solid2D::Self::get()
{
    return this->self;
}

unsigned int CEqn_Solid2D::GetIdField_Disp()
{
    return this->self->GetIdField_Disp();
}

void CEqn_Solid2D::CopyParameters(CEqn_Solid2D^ eqn )
{
    this->self->CopyParameters(*(eqn->self));
}

////////////////////////////////////////////////////////////////////////////////
// Setメソッド
void CEqn_Solid2D::SetYoungPoisson( double young, double poisson, bool is_plane_stress )
{
    this->self->SetYoungPoisson(young, poisson, is_plane_stress);
}

void CEqn_Solid2D::GetLambdaMyu( [Out] double% lambda, [Out] double% myu)
{
    double lambda_;
    double myu_;
    
    this->self->GetLambdaMyu(lambda_, myu_);

    lambda = lambda_;
    myu = myu_;
}

double CEqn_Solid2D::GetRho()
{
    return this->self->GetRho();
}

void CEqn_Solid2D::GetYoungPoisson( [Out] double% young, [Out] double% poisson)
{
    double young_;
    double poisson_;

    this->self->GetYoungPoisson(young_, poisson_);
    
    young = young_;
    poisson = poisson_;
}

void CEqn_Solid2D::SetRho(double rho)
{
    this->self->SetRho(rho);
}

void CEqn_Solid2D::SetGravitation( double g_x, double g_y )
{
    this->self->SetGravitation(g_x, g_y);
}

void CEqn_Solid2D::GetGravitation( [Out] double% g_x, [Out] double% g_y )
{
    double g_x_;
    double g_y_;
    
    this->self->GetGravitation(g_x_, g_y_);
    
    g_x = g_x_;
    g_y = g_y_;
}

void CEqn_Solid2D::SetIdFieldDisp(unsigned int id_field_disp)
{
    this->self->SetIdFieldDisp(id_field_disp);
}

void CEqn_Solid2D::SetGeometricalNonlinear(bool is_nonlin)
{
    this->self->SetGeometricalNonlinear(is_nonlin);
}

void CEqn_Solid2D::SetThermalStress(unsigned int id_field_temp)
{
    this->self->SetThermalStress(id_field_temp);
}

/////////////////////////////////////////////////////////////////////
// Getメソッド

unsigned int CEqn_Solid2D::GetIdEA()
{
    return this->self->GetIdEA();
}

bool CEqn_Solid2D::IsTemperature()
{
    return this->self->IsTemperature();
}

bool CEqn_Solid2D::IsGeometricalNonlinear()
{
    return this->self->IsGeometricalNonlinear();
}

/////////////////////////////////////////////////////////////////////
// 連立一次方程式マージメソッド
bool CEqn_Solid2D::AddLinSys_NewmarkBetaAPrime
    ( double dt, double gamma, double beta, bool is_inital, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_NewmarkBetaAPrime(dt, gamma, beta, is_inital, *(ls->Self), *(world->Self));
}
bool CEqn_Solid2D::AddLinSys_NewmarkBetaAPrime_Save( DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_NewmarkBetaAPrime_Save(*(ls->Self), *(world->Self));
}

bool CEqn_Solid2D::AddLinSys( DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys(*(ls->Self), *(world->Self));
}

bool CEqn_Solid2D::AddLinSys_Save( DelFEM4NetFem::Ls::CLinearSystem_Save^ ls, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->AddLinSys_Save(*(ls->Self), *(world->Self));
}

////////////////////////////////////////////////////////////////
// CEqnSystem_Solid2D
////////////////////////////////////////////////////////////////

CEqnSystem_Solid2D::CEqnSystem_Solid2D() : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Solid2D();
}

CEqnSystem_Solid2D::CEqnSystem_Solid2D(const CEqnSystem_Solid2D% rhs) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    const Fem::Eqn::CEqnSystem_Solid2D& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Fem::Eqn::CEqnSystem_Solid2D(rhs_instance_);
    assert(false);
}

/* nativeなライブラリで実装されていない
CEqnSystem_Solid2D::CEqnSystem_Solid2D(DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Solid2D(*(world->Self));
}
*/

CEqnSystem_Solid2D::CEqnSystem_Solid2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = new Fem::Eqn::CEqnSystem_Solid2D(id_field_val, *(world->Self));
}


CEqnSystem_Solid2D::CEqnSystem_Solid2D(Fem::Eqn::CEqnSystem_Solid2D *self) : CEqnSystem() // 基本クラスコンストラクタは何もしない
{
    this->self = self;
}

CEqnSystem_Solid2D::~CEqnSystem_Solid2D()
{
    this->!CEqnSystem_Solid2D();
}

CEqnSystem_Solid2D::!CEqnSystem_Solid2D()
{
    delete this->self;
}

Fem::Eqn::CEqnSystem * CEqnSystem_Solid2D::EqnSysSelf::get()
{
    return this->self;
}

Fem::Eqn::CEqnSystem_Solid2D * CEqnSystem_Solid2D::Self::get()
{
    return this->self;
}

////////////////////////////////////////////////////////////////
// 仮想化可能関数
bool CEqnSystem_Solid2D::UpdateDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->UpdateDomain_Field(id_field, *(world->Self));
}

bool CEqnSystem_Solid2D::SetDomain_FieldEA(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetDomain_FieldEA(id_field, id_ea, *(world->Self));
}

////////////////////////////////////////////////////////////////
// 方程式を解く
bool CEqnSystem_Solid2D::Solve(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Solve(*(world->Self));
}

void CEqnSystem_Solid2D::Clear()
{
    this->self->Clear();
}


////////////////////////////////////////////////////////////////
// 固定境界条件を追加&削除する
bool CEqnSystem_Solid2D::AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixField(id_field, *(world->Self), idof);
}

unsigned int CEqnSystem_Solid2D::AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    return this->self->AddFixElemAry(id_ea, *(world->Self), idof);
}

unsigned int CEqnSystem_Solid2D::AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof)
{
    std::vector<unsigned int> aIdEA_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA, aIdEA_);

    return this->self->AddFixElemAry(aIdEA_, *(world->Self), idof);
}

bool CEqnSystem_Solid2D::ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return this->self->ClearFixElemAry(id_ea, *(world->Self));
}

void CEqnSystem_Solid2D::ClearFixElemAry()
{
    return this->self->ClearFixElemAry();
}

unsigned int CEqnSystem_Solid2D::GetIdField_Disp()
{
    return this->self->GetIdField_Disp();
}

/* nativeなライブラリで実装されていない
bool CEqnSystem_Solid2D::ToplogicalChangeCad_InsertLoop(DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_l_back, unsigned id_l_ins)
{
    return this->self->ToplogicalChangeCad_InsertLoop(*(world->Self), id_l_back, id_l_ins);
}
*/

/////////////////////////////////////////////////////////////////////////////////
// 非virtual

bool CEqnSystem_Solid2D::SetEquation( DelFEM4NetFem::Eqn::CEqn_Solid2D^ eqn )
{
    return this->self->SetEquation( *(eqn->Self) );
}

DelFEM4NetFem::Eqn::CEqn_Solid2D^ CEqnSystem_Solid2D::GetEquation(unsigned int id_ea)
{
    const Fem::Eqn::CEqn_Solid2D& ret_instance_ = this->self->GetEquation(id_ea);
    Fem::Eqn::CEqn_Solid2D *ret = new Fem::Eqn::CEqn_Solid2D(ret_instance_);
    
    DelFEM4NetFem::Eqn::CEqn_Solid2D^ retManaged = gcnew DelFEM4NetFem::Eqn::CEqn_Solid2D(ret);
    
    return retManaged;
}

///////////////////////////////////////////////////////////////////////////////////
// 各領域の方程式のパラメータを一度に変えるオプション
void CEqnSystem_Solid2D::SetYoungPoisson( double young, double poisson, bool is_plane_stress )
{
    this->self->SetYoungPoisson(young, poisson, is_plane_stress);
}

void CEqnSystem_Solid2D::SetRho( double rho )
{
    this->self->SetRho(rho);
}

void CEqnSystem_Solid2D::SetGravitation( double g_x, double g_y )
{
    this->self->SetGravitation(g_x, g_y);
}

void CEqnSystem_Solid2D::SetThermalStress(unsigned int id_field_temp)
{
    this->self->SetThermalStress(id_field_temp);
}

void CEqnSystem_Solid2D::SetGeometricalNonlinear(bool is_nonlin)
{
    this->self->SetGeometricalNonlinear(is_nonlin);
}

void CEqnSystem_Solid2D::SetStationary(bool is_stationary)
{
    this->self->SetStationary(is_stationary);
}

void CEqnSystem_Solid2D::SetSaveStiffMat(bool is_save)
{
    this->self->SetSaveStiffMat(is_save);
}

bool CEqnSystem_Solid2D::IsStationary()
{
    return this->self->IsStationary();
}

bool CEqnSystem_Solid2D::IsSaveStiffMat()
{
    return this->self->IsSaveStiffMat();
}


void CEqnSystem_Solid2D::GetYoungPoisson([Out] double% young, [Out] double% poisson, [Out] bool% is_plane_str)
{
    double young_;
    double poisson_;
    bool is_plane_str_;

    this->self->GetYoungPoisson(young_, poisson_, is_plane_str_);
    
    young = young_;
    poisson = poisson_;
    is_plane_str = is_plane_str_;
}

bool CEqnSystem_Solid2D::IsGeometricalNonlinear()
{
    return this->self->IsGeometricalNonlinear();
}

double CEqnSystem_Solid2D::GetRho()
{
    return this->self->GetRho();
}

void CEqnSystem_Solid2D::SetLoad(double load, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    this->self->SetLoad(load, id_ea, *(world->Self));
}

void CEqnSystem_Solid2D::ClearLoad(unsigned int id_ea)
{
    this->self->ClearLoad(id_ea);
}

void CEqnSystem_Solid2D::ClearLoad()
{
    this->self->ClearLoad();
}

bool CEqnSystem_Solid2D::SetEquivStressValue(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetEquivStressValue(id_field, *(world->Self));
}

bool CEqnSystem_Solid2D::SetStressValue(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->SetStressValue(id_field, *(world->Self));
}

