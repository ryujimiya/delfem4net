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
@brief Interface for Msh::CMesher2D
@remarks A class derieve this class can be cut mesh using class (Msh::CMesher2D)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_2D_INTERFACE_H)
#define DELFEM4NET_CAD_2D_INTERFACE_H

#include "DelFEM4Net/vector2d.h"
#include "DelFEM4Net/cad_com.h"

#include "delfem/cad2d_interface.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

/*! 
@ingroup CAD
@brief 2D CAD model class (Model class of 2D CAD for mesher)
*/
public interface class ICad2D_Msh
{
public:
    property Cad::ICad2D_Msh * Cad2DMshSelf
    {
        Cad::ICad2D_Msh * get();
    }
    bool GetIdVertex_Edge(unsigned int% id_v_s, unsigned int% id_v_e, unsigned int id_e);
    bool IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE, unsigned int id);
    IList<unsigned int>^ GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype);  
    int GetLayer(DelFEM4NetCad::CAD_ELEM_TYPE, unsigned int id);
    void GetLayerMinMax(int% layer_min, int% layer_max);
    bool GetColor_Loop(unsigned int id_l, array<double>^% color);
    //! get area loop (ID:id_l)
    double GetArea_Loop(unsigned int id_l);
    IItrLoop^ GetPtrItrLoop(unsigned int id_l);
    bool GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo, double elen);
    DelFEM4NetCom::CVector2D^ GetVertexCoord(unsigned int id_v);
};

}

#endif
