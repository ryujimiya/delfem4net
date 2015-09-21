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
// The definition of each functions can be found in eqn_linear_solid_3d.cpp
/*
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_LINEAR_SOLID_3D_H)
#define DELFEM4NET_EQN_LINEAR_SOLID_3D_H

#include <vector>

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "delfem/femeqn/eqn_linear_solid3d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

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

    interface class ILinearSystem_Eqn;

public ref class CEqnLinearSolid3D
{
public:
    // lienar elastic solid static
    static bool AddLinSys_LinearSolid3D_Static
        (DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp );

    // linear elastic solid dynamic with newmark-beta time integration
    static bool AddLinSys_LinearSolid3D_NonStatic_NewmarkBeta
        (double dt, double gamma, double beta,
         DelFEM4NetFem::Eqn::ILinearSystem_Eqn^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp );

    // linear elastic solid static (saving stiffness matrix)
    static bool AddLinSys_LinearSolid3D_Static_SaveStiffMat
        (DelFEM4NetFem::Ls::CLinearSystem_Save^ ls,
         double lambda, double myu,
         double  rho, double g_x, double g_y, double g_z,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp );

    // buld linear system for eigenanalysis of dynamic linear elastic solid
    static bool AddLinSys_LinearSolid3D_Eigen
        (DelFEM4NetFem::Ls::CLinearSystem_Eigen^ ls,
         double lambda, double myu, double rho,
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_disp );
};

}
}

#endif
