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
// eqn_helmholtz.cpp : ヘルムホルツ方程式の要素剛性作成部の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif


#include <math.h>

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/matvec/zmatdia_blkcrs.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/zvector_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

#include "DelFEM4Net/femeqn/eqn_helmholtz.h"
//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"
//#include "DelFEM4Net/femeqn/ker_emat_hex.h"

#include "DelFEM4Net/femls/zlinearsystem.h"

using namespace DelFEM4NetFem::Eqn;
//using namespace DelFEM4NetFem::Field;
//using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetMatVec;



bool CEqnHelmholz::AddLinSys_Helmholtz(
        DelFEM4NetFem::Ls::CZLinearSystem^ ls,
        double wave_length,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_Helmholtz(
        *(ls->Self), 
        wave_length,
        *(world->Self),
        id_field_val,
        id_ea);
}

bool CEqnHelmholz::AddLinSys_Helmholtz_AxalSym(
        DelFEM4NetFem::Ls::CZLinearSystem^ ls,
        double wave_length,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_Helmholtz_AxalSym(
        *(ls->Self), 
        wave_length,
        *(world->Self),
        id_field_val,
        id_ea);
}

bool CEqnHelmholz::AddLinSys_SommerfeltRadiationBC(
        DelFEM4NetFem::Ls::CZLinearSystem^ ls,
        double wave_length,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_SommerfeltRadiationBC(
        *(ls->Self), 
        wave_length,
        *(world->Self),
        id_field_val,
        id_ea);
}


bool CEqnHelmholz::AddLinSys_SommerfeltRadiationBC_AxalSym(
        DelFEM4NetFem::Ls::CZLinearSystem^ ls,
        double wave_length,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_SommerfeltRadiationBC_AxalSym(
        *(ls->Self), 
        wave_length,
        *(world->Self),
        id_field_val,
        id_ea);
}

bool CEqnHelmholz::AddLinSys_MassMatrixEigen_AxalSym(
        DelFEM4NetFem::Ls::CZLinearSystem_GeneralEigen^ ls,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_MassMatrixEigen_AxalSym(
        *(ls->Self), 
        *(world->Self),
        id_field_val,
        id_ea);
}

