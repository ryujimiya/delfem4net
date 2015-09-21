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
// This file declar the functions for linear solid equation.
// The function builds element matrices and marges them to linear system.
// The definition of each functions can be found in eqn_linear_solid.cpp
/*
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_LINEAR_SOLID_2D_H)
#define DELFEM4NET_EQN_LINEAR_SOLID_2D_H


#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>

#include "delfem/femeqn/eqn_linear_solid2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
    ref class CVector_Blk;
}

namespace DelFEM4NetFem
{
namespace Ls
{
    ref class CLinearSystem_Field;
    ref class CLinearSystem_Save;
    ref class CLinearSystem_SaveDiaM_NewmarkBeta;
    ref class CLinearSystem_Eigen;
    ref class CPreconditioner;
}

namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}

namespace Eqn
{

    interface  class ILinearSystem_Eqn;

public ref class CEqnLinearSolid2D
{
public:
    ////////////////////////////////////////////////////////////////
    // 2-dimensional euqations

    // 2D static linear elastic solid 
    static bool AddLinSys_LinearSolid2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp)
    {
        return AddLinSys_LinearSolid2D_Static
            (ls,
             lambda, myu, rho, g_x, g_y,
             world, id_field_disp,
             0);
    }
    static bool AddLinSys_LinearSolid2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp,
         unsigned int id_ea);

    // 2D static linear elastic solid with thermal-stress
    static bool AddLinSys_LinearSolidThermalStress2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y, double thermoelastic, 
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, unsigned int id_field_temp)
    {
        return AddLinSys_LinearSolidThermalStress2D_Static
            (ls,
             lambda, myu, rho, g_x, g_y, thermoelastic,
             world, id_field_disp, id_field_temp,
             0);
    }
    static bool AddLinSys_LinearSolidThermalStress2D_Static
        (DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,     double thermoelastic, 
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, unsigned int id_field_temp,
         unsigned int id_ea);

    // 2D dynamic linear elastic solid
    // this function use backward eular time integration method
    static bool AddLinSys_LinearSolid2D_NonStatic_BackwardEular
        (double dt, DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         DelFEM4NetMatVec::CVector_Blk^ velo_pre)
    {
        return AddLinSys_LinearSolid2D_NonStatic_BackwardEular
            (dt, ls,
             lambda, myu, rho, g_x, g_y,
             world, id_field_disp,
             velo_pre,
             true,
             0);
    }
    static bool AddLinSys_LinearSolid2D_NonStatic_BackwardEular
        (double dt, DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         DelFEM4NetMatVec::CVector_Blk^ velo_pre,
         bool is_initial,     
         unsigned int id_ea);
    
    // 2D dynamic linear elastic solid
    // this function use newmark-beta time integration method
    static bool AddLinSys_LinearSolid2D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta, DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp)
    {
        return AddLinSys_LinearSolid2D_NonStatic_NewmarkBeta
            (dt, gamma, beta, ls,
             lambda, myu, rho, g_x, g_y,
             world, id_field_disp,
             true,
             0);
    }
    static bool AddLinSys_LinearSolid2D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta, DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         bool is_initial,
         unsigned int id_ea );

    // 2D dynamic lienar solid with thermal-stress
    static bool AddLinSys_LinearSolidThermalStress2D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
         double lambda, double myu, double rho, double g_x, double g_y, double thermoelastic, 
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, unsigned int id_field_temp)
    {
        return AddLinSys_LinearSolidThermalStress2D_NonStatic_NewmarkBeta
            (dt, gamma, beta, ls,
             lambda, myu, rho, g_x, g_y, thermoelastic,
             world, id_field_disp, id_field_temp,
             true,
             0);
    }
    static bool AddLinSys_LinearSolidThermalStress2D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
         double lambda, double myu, double rho, double g_x, double g_y, double thermoelastic, 
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, unsigned int id_field_temp, 
         bool is_inital,
         unsigned int id_ea);

    // static linear elastic solid (saving stiffness matrix)
    static bool AddLinSys_LinearSolid2D_Static_SaveStiffMat
        (DelFEM4NetFem::Ls::CLinearSystem_Save^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp)
    {
        return AddLinSys_LinearSolid2D_Static_SaveStiffMat
            (ls,
             lambda, myu, rho, g_x, g_y,
             world, id_field_disp,
             0);
    }
    static bool AddLinSys_LinearSolid2D_Static_SaveStiffMat
        (DelFEM4NetFem::Ls::CLinearSystem_Save^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         unsigned int id_ea);

    // dynamic linear elastic solid (saving stiffness matrix)
    static bool AddLinSys_LinearSolid2D_NonStatic_Save_NewmarkBeta
        (DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp)
    {
        return AddLinSys_LinearSolid2D_NonStatic_Save_NewmarkBeta
            (ls,
             lambda, myu, rho, g_x, g_y,
             world, id_field_disp,
             0);
    }
    static bool AddLinSys_LinearSolid2D_NonStatic_Save_NewmarkBeta
        (DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta^ ls,
         double lambda, double myu, double rho, double g_x, double g_y,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         unsigned int id_ea);

    // buidling matirx for eigennalysis
    static bool AddLinSys_LinearSolid2D_Eigen
        (DelFEM4NetFem::Ls::CLinearSystem_Eigen^ ls,
         double lambda, double myu, double rho,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp)
    {
        return AddLinSys_LinearSolid2D_Eigen
            (ls,
             lambda, myu, rho,
             world, id_field_disp,
             0);
    }
    static bool AddLinSys_LinearSolid2D_Eigen
        (DelFEM4NetFem::Ls::CLinearSystem_Eigen^ ls,
         double lambda, double myu, double rho,
         DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_disp, 
         unsigned int id_ea);
};

}
}

#endif
