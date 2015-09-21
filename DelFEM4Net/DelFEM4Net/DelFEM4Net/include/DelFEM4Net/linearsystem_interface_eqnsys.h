/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

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
@brief 連立一次方程式の方程式へのインターフェスクラス(Fem::Eqn::CLinearSystem_EqnInterface)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQUATION_INTERFACE_H)
#define DELFEM4NET_EQUATION_INTERFACE_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "DelFEM4Net/elem_ary.h"   // CORNER, EDDGE, BUBBLEの名前のために追加（何とかしてこのincludeを削除したい）

#include "delfem/linearsystem_interface_eqnsys.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
    ref class CMatDia_BlkCrs;
    ref class CMat_BlkCrs;
    ref class CVector_Blk;
}

namespace DelFEM4NetFem
{
namespace Field
{
    ref class CFieldWorld;
}
namespace Eqn
{

//! interface class of linear system for equation
public interface class ILinearSystem_Eqn
{
public:
    property Fem::Eqn::ILinearSystem_Eqn *EqnSelf
    {
        Fem::Eqn::ILinearSystem_Eqn *get();
    }

    virtual DelFEM4NetMatVec::CMat_BlkCrs^ GetMatrix(
        unsigned int id_field1,
        DelFEM4NetFem::Field::ELSEG_TYPE node_config1,
        unsigned int id_field2,
        DelFEM4NetFem::Field::ELSEG_TYPE node_config2,
        DelFEM4NetFem::Field::CFieldWorld^ world) = 0;

    virtual DelFEM4NetMatVec::CMatDia_BlkCrs^ GetMatrix(
        unsigned int id_field_disp,
        DelFEM4NetFem::Field::ELSEG_TYPE node_config,
        DelFEM4NetFem::Field::CFieldWorld^ world) = 0;

    virtual DelFEM4NetMatVec::CVector_Blk^ GetResidual(
        unsigned int id_field_disp,
        DelFEM4NetFem::Field::ELSEG_TYPE node_config,
        DelFEM4NetFem::Field::CFieldWorld^ world) = 0;
public:
};
	
}	// Eqn
}	// Fem

#endif
