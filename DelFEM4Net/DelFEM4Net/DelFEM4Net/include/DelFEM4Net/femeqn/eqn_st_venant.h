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

// AUTHOR
// Nobuyuki Umetani

// DESCRIPTION
// This file declar the functions for St.Venant-Kirchhoff solid equation.
// The function builds element matrices and marges them to linear system.
// The definition of each functions can be found in eqn_linear_solid.cpp
/*
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_ST_VENSNT_H)
#define DELFEM4NET_EQN_ST_VENSNT_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "DelFEM4Net/linearsystem_interface_eqnsys.h"
#include "delfem/femeqn/eqn_st_venant.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Ls
{
    ref class CLinearSystem_Field;
}
namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}
namespace Eqn
{

public ref class CEqnStVenant
{
public:
    // marge 2D static St.Venant-Kirchhoff material euqtion
    // ls [in,out] : linear system
    // lambda [in] : first lame constant
    // myu [in] : second lame constant
    // rho [in] : mass density
    // g_x [in] : gravity in x axes
    // g_y [in] : gravity in y axes    
    static bool AddLinSys_StVenant2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double f_x, double f_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp)
    {
        unsigned int id_ea = 0;
        return CEqnStVenant::AddLinSys_StVenant2D_Static
            (ls,
             lambda, myu,
             rho, f_x, f_y,
             world,
             id_field_disp,
             id_ea);
    }
    
    static bool AddLinSys_StVenant2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double f_x, double f_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp,
         unsigned int id_ea);
    
    // marge 2D dynamic St.Venant-Kirchhoff material euqtion
    // newmark-beta time integration
    // ls [in,out] : linear system
    // lambda [in] : first lame constant
    // myu [in] : second lame constant
    // rho [in] : mass density
    // g_x [in] : gravity in x axes
    // g_y [in] : gravity in y axes
    static bool AddLinSys_StVenant2D_NonStatic_NewmarkBeta
        (double dt, double gamma_newmark, double beta, 
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp, 
         bool is_initial)
    {
        unsigned int id_ea = 0;
        return CEqnStVenant::AddLinSys_StVenant2D_NonStatic_NewmarkBeta
            (dt, gamma_newmark, beta, 
             ls,
             lambda, myu,
             rho, g_x, g_y,
             world,
             id_field_disp,
             is_initial,
             id_ea);
    }
    static bool AddLinSys_StVenant2D_NonStatic_NewmarkBeta
        (double dt, double gamma_newmark, double beta, 
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp, 
         bool is_initial,
         unsigned int id_ea);
    
    static bool AddLinSys_StVenant2D_NonStatic_BackwardEular
        (double dt, 
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp, 
         DelFEM4NetMatVec::CVector_Blk^ velo_pre,
         bool is_initial)
    {
        unsigned int id_ea = 0;
        return CEqnStVenant::AddLinSys_StVenant2D_NonStatic_BackwardEular
            (dt,
             ls, 
             lambda, myu, rho, 
             g_x, g_y, 
             world, 
             id_field_disp, 
             velo_pre, 
             is_initial, 
             id_ea);
    }
    static bool AddLinSys_StVenant2D_NonStatic_BackwardEular
        (double dt, 
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp, 
         DelFEM4NetMatVec::CVector_Blk^ velo_pre,
         bool is_initial,
         unsigned int id_ea);
    
    // marge 3D static St.Venant-Kirchhoff material euqtion
    // ls [in,out] : linear system
    // lambda [in] : first lame constant
    // myu [in] : second lame constant
    // rho [in] : mass density
    // g_x [in] : gravity in x axes
    // g_y [in] : gravity in y axes
    // g_z [in] : gravity in z axes
    static bool AddLinSys_StVenant3D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp)
    {
        unsigned int id_ea = 0;
        return CEqnStVenant::AddLinSys_StVenant3D_Static
            (ls,
             lambda, myu, 
             rho, g_x, g_y, g_z, 
             world, 
             id_field_disp, 
             id_ea);
    }
    static bool AddLinSys_StVenant3D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp,
         unsigned int id_ea);

    // marge 3D dynamic St.Venant-Kirchhoff material euqtion
    // newmark-beta time integration
    // ls [in,out] : linear system
    // lambda [in] : first lame constant
    // myu [in] : second lame constant
    // rho [in] : mass density
    // g_x [in] : gravity in x axes
    // g_y [in] : gravity in y axes
    // g_z [in] : gravity in z axes
    static bool AddLinSys_StVenant3D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta,
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp,
         bool is_initial)
    {
        unsigned int id_ea = 0;
        return CEqnStVenant::AddLinSys_StVenant3D_NonStatic_NewmarkBeta
            (dt, gamma, beta,
             ls, 
             lambda, myu, rho, 
             g_x, g_y, g_z, 
             world, 
             id_field_disp, 
             is_initial, 
             id_ea);
    }
    static bool AddLinSys_StVenant3D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta,
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp,
         bool is_initial,
         unsigned int id_ea);
};

} // namespace Eqn
} // namespace DelFEM4NetFem

#endif
