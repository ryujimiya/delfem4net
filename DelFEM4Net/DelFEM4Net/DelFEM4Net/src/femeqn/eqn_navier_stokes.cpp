﻿/*
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
// eqn_navier_stokes.cpp : ナビア・ストークス流体方程式の要素剛性作成部の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
	#pragma warning( disable : 4786 )
#endif

#include <math.h>

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femeqn/eqn_navier_stokes.h"
//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"

using namespace DelFEM4NetFem::Eqn;
//using namespace DelFEM4NetFem::Field;
//using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetMatVec;

bool CEqnNavierStokes::AddLinSys_NavierStokes2D_NonStatic_Newmark(
        double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        double rho, double alpha,      double g_x, double g_y,
        unsigned int id_field_velo, unsigned int id_field_press, 
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_ea)
{
    return Fem::Eqn::AddLinSys_NavierStokes2D_NonStatic_Newmark(
               dt, gamma, *(ls->Self), 
               rho, alpha, g_x, g_y,
               id_field_velo, id_field_press, 
               *(world->Self),
               id_ea);
}

bool CEqnNavierStokes::AddLinSys_NavierStokesALE2D_NonStatic_Newmark(
            double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
            double rho, double alpha, double g_x, double g_y,
            unsigned int id_field_velo, unsigned int id_field_press, unsigned int id_field_msh_velo,
            DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return Fem::Eqn::AddLinSys_NavierStokesALE2D_NonStatic_Newmark(
               dt, gamma, *(ls->Self), 
               rho, alpha, g_x, g_y,
               id_field_velo, id_field_press, id_field_msh_velo,
               *(world->Self));
}

bool CEqnNavierStokes::AddLinSys_NavierStokes2DThermalBuoy_NonStatic_Newmark(
        double rho, double alpha, double g_x, double g_y,
        double gamma, double dt,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        unsigned int id_field_velo, unsigned int id_field_press, unsigned int id_field_temp,
        DelFEM4NetFem::Field::CFieldWorld^ world )
{
    return Fem::Eqn::AddLinSys_NavierStokes2DThermalBuoy_NonStatic_Newmark(
               rho, alpha, g_x, g_y,
               gamma, dt,
               *(ls->Self), 
               id_field_velo, id_field_press, id_field_temp,
               *(world->Self));
}

