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
// Eqn_AdvectionDiffusion.cpp : 移流拡散方程式の要素剛性作成関数の実装
////////////////////////////////////////////////////////////////

#include <math.h>

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/femeqn/eqn_advection_diffusion.h"

#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femls/linearsystem_fieldsave.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"
//#include "DelFEM4Net/matvec/ker_emat_tri.h"
//#include "DelFEM4Net/matvec/ker_emat_quad.h"

using namespace DelFEM4NetFem::Eqn;
//using namespace DelFEM4NetFem::Field;
//using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetMatVec;

bool CEqnAdvectionDiffusion::AddLinSys_AdvectionDiffusion_Static(
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double myu, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_field_velo, 
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_AdvectionDiffusion_Static(
        *(ls->Self),
        myu, source,
        *(world->Self),
        id_field_val, id_field_velo,
        id_ea);
}

bool CEqnAdvectionDiffusion::AddLinSys_AdvectionDiffusion_Static(
        DelFEM4NetFem::Ls::CLinearSystem_Save^ ls,
        double myu, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_field_velo, 
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_AdvectionDiffusion_Static(
        *(ls->Self),
        myu, source,
        *(world->Self),
        id_field_val, id_field_velo,
        id_ea);
}

bool CEqnAdvectionDiffusion::AddLinSys_AdvectionDiffusion_NonStatic_Newmark(
        double dt, double gamma, 
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double myu, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_field_velo, 
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_AdvectionDiffusion_NonStatic_Newmark(
        dt, gamma, 
        *(ls->Self),
        rho, myu, source,
        *(world->Self),
        id_field_val, id_field_velo,
        id_ea);
}

bool CEqnAdvectionDiffusion::AddLinSys_AdvectionDiffusion_NonStatic_Newmark(
        DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls,
        double rho, double myu, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_field_velo, 
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_AdvectionDiffusion_NonStatic_Newmark(
        *(ls->Self),
        rho, myu, source,
        *(world->Self),
        id_field_val, id_field_velo,
        id_ea);
}
