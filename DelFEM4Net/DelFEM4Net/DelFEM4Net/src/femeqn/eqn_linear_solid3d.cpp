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
// eqn_linear_solid.cpp : 線形弾性体の要素剛性作成部の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
	#pragma warning( disable : 4786 )
#endif

#include "DelFEM4Net/field_world.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/matvec/diamat_blk.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/femls/linearsystem_field.h"
#include "DelFEM4Net/femls/linearsystem_fieldsave.h"

//#include "DelFEM4Net/femeqn/ker_emat_tri.h"
//#include "DelFEM4Net/femeqn/ker_emat_quad.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"
//#include "DelFEM4Net/femeqn/ker_emat_hex.h"
#include "DelFEM4Net/femeqn/eqn_linear_solid3d.h"

using namespace DelFEM4NetFem::Eqn;
//using namespace DelFEM4NetFem::Field;
//using namespace DelFEM4NetFem::Ls;
//using namespace DelFEM4NetMatVec;

bool CEqnLinearSolid3D::AddLinSys_LinearSolid3D_Static
        (DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp )
{
    return Fem::Eqn::AddLinSys_LinearSolid3D_Static
        (*(ls->Self),
         lambda, myu, 
         rho, g_x, g_y, g_z,
         *(world->Self), 
         id_field_disp);
}

bool CEqnLinearSolid3D::AddLinSys_LinearSolid3D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta,
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp )
{
    return Fem::Eqn::AddLinSys_LinearSolid3D_NonStatic_NewmarkBeta
        (dt, gamma, beta, 
         *(ls->EqnSelf),
         lambda, myu,
         rho, g_x, g_y, g_z,
         *(world->Self), 
         id_field_disp);
}
bool CEqnLinearSolid3D::AddLinSys_LinearSolid3D_Static_SaveStiffMat
        (DelFEM4NetFem::Ls::CLinearSystem_Save^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp )
{
    return Fem::Eqn::AddLinSys_LinearSolid3D_Static_SaveStiffMat
        (*(ls->Self),
         lambda, myu,
         rho, g_x, g_y, g_z,
         *(world->Self), 
         id_field_disp);
}

bool CEqnLinearSolid3D::AddLinSys_LinearSolid3D_Eigen
        (DelFEM4NetFem::Ls::CLinearSystem_Eigen^ ls,
         double lambda, double myu, double rho,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp )
{
    return Fem::Eqn::AddLinSys_LinearSolid3D_Eigen
        (*(ls->Self),
         lambda, myu, rho,
         *(world->Self), 
         id_field_disp);
}

