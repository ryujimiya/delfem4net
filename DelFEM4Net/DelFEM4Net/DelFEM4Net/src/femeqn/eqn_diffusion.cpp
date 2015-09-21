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
// eqn_diffusion.cpp : 拡散方程式の要素剛性作成部の実装
////////////////////////////////////////////////////////////////

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/femeqn/eqn_diffusion.h"
#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femls/linearsystem_fieldsave.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"

using namespace DelFEM4NetFem::Eqn;
using namespace DelFEM4NetFem::Field;
using namespace DelFEM4NetFem::Ls;
using namespace DelFEM4NetMatVec;

bool CEqnDiffusion::AddLinSys_Diffusion(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_Diffusion(
               dt, gamma,
               *(ls->Self),
               rho, alpha, source,
               *(world->Self), id_field_val,
               id_ea);
}

bool CEqnDiffusion::AddLinSys_Diffusion_AxSym(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_Diffusion_AxSym(
               dt, gamma,
               *(ls->Self),
               rho, alpha, source,
               *(world->Self), id_field_val,
               id_ea);
}

bool CEqnDiffusion::AddLinSys_Diffusion(
        DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_Diffusion(
               *(ls->Self),
               rho, alpha, source,
               *(world->Self),
               id_field_val, id_ea);
}

