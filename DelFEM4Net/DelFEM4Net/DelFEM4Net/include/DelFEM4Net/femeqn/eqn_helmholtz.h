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

/*! @file
@brief ヘルムホルツ方程式の要素剛性作成部のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
@sa http://ums.futene.net/wiki/FEM/46454D20666F7220536F756E64204669656C6420416E616C79736973.html
*/

#if !defined(DELFEM4NET_EQN_HELMHOLTZ_H)
#define DELFEM4NET_EQN_HELMHOLTZ_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>

#include "delfem/femeqn/eqn_helmholtz.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{

namespace Ls
{
    ref class CZLinearSystem;
    ref class CZLinearSystem_GeneralEigen;
    ref class CPreconditioner;
}
namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}

namespace Eqn
{

public ref class CEqnHelmholz
{
public:
    /*!
    @brief Helmholtz方程式のマージ
    @param[in] wave_length 波長
    */
    static bool AddLinSys_Helmholtz(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val)
    {
        // 0だとid_field_valすべてについて
        unsigned int id_ea = 0 ;
        return AddLinSys_Helmholtz(ls, wave_length, world, id_field_val, id_ea);
    }
    static bool AddLinSys_Helmholtz(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val,
            unsigned int id_ea);    // 0だとid_field_valすべてについて
    
    /*!
    @brief Helmholtz方程式のマージ
    @param[in] wave_length 波長
    */
    static bool AddLinSys_Helmholtz_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val)
    {
        // 0だとid_field_valすべてについて
        unsigned int id_ea = 0 ;
        return AddLinSys_Helmholtz_AxalSym(ls, wave_length, world, id_field_val, id_ea);
    }
    static bool AddLinSys_Helmholtz_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val,
            unsigned int id_ea);    // 0だとid_field_valすべてについて
    
    /*!
    @brief Helmholtz方程式の放射境界条件のマージ
    @param[in] wave_length 波長
    */
    static bool AddLinSys_SommerfeltRadiationBC(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val)
    {
        // 0だとid_field_valすべてについて
        unsigned int id_ea = 0 ;
        return AddLinSys_SommerfeltRadiationBC(ls, wave_length, world, id_field_val, id_ea);
    }
    static bool AddLinSys_SommerfeltRadiationBC(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val,
            unsigned int id_ea);    // 0だとid_field_valすべてについて
    
    /*!
    @brief Helmholtz方程式の放射境界条件のマージ
    @param[in] wave_length 波長
    */
    static bool AddLinSys_SommerfeltRadiationBC_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val)
    {
        // 0だとid_field_valすべてについて
        unsigned int id_ea = 0 ;
        return AddLinSys_SommerfeltRadiationBC_AxalSym(ls, wave_length, world, id_field_val, id_ea);
    }
    static bool AddLinSys_SommerfeltRadiationBC_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem^ ls,
            double wave_length,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val,
            unsigned int id_ea);    // 0だとid_field_valすべてについて
    
    /*!
    @brief Helmholtz方程式のマージ
    @param[in] wave_length 波長
    */
    static bool AddLinSys_MassMatrixEigen_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem_GeneralEigen^ ls,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val)
    {
        // 0だとid_field_valすべてについて
        unsigned int id_ea = 0 ;
        return AddLinSys_MassMatrixEigen_AxalSym(ls, world, id_field_val, id_ea);
    }
    static bool AddLinSys_MassMatrixEigen_AxalSym(
            DelFEM4NetFem::Ls::CZLinearSystem_GeneralEigen^ ls,
            DelFEM4NetFem::Field::CFieldWorld^ world,
            unsigned int id_field_val,
            unsigned int id_ea);    // 0だとid_field_valすべてについて
};

}
}

#endif
