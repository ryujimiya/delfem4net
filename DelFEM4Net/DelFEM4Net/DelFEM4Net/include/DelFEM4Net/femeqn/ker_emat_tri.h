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


/*! @file:
@brief 三角形要素で要素剛性行列を作る際のUtility関数の集まり
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_KER_EMAT_TRI_H)
#define DELFEM4NET_KER_EMAT_TRI_H

#include <cassert>

#include "delfem/femeqn/ker_emat_tri.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Eqn
{

public ref class CKerEMatTri
{
public:
    static const int VEC_DIM         = 2;  // 座標の次元
    static const int NODE_CNT        = 3;  // 節点の数
public:
    //! calculate Area of Triangle 
    static double TriArea(array<double>^ p0, array<double>^ p1, array<double>^ p2);
    
    //! calculate AreaCoord of Triangle
    static void TriAreaCoord([Out] array<double>^% vc_p,
                      array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ pb );
    
    //! caluculate Derivative of Area Coord
    static void TriDlDx([Out] array<double, 2>^% dldx, [Out] array<double>^% const_term,
                 array<double>^ p0, array<double>^ p1, array<double>^ p2);
    
    //! 積分点の数
    static initonly array<unsigned int>^ NIntTriGauss = gcnew array<unsigned int>(3){ 
        1, 3, 7
    };
    
    //! 積分点の位置(r1,r2)と重みの配列
    static initonly array<double, 3>^ TriGauss = gcnew array<double, 3>(3, 7, 3)
    {
        {
            { 0.3333333333, 0.3333333333, 1.0 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
        },
        {
            { 0.1666666667, 0.1666666667, 0.3333333333 },
            { 0.6666666667, 0.1666666667, 0.3333333333 },
            { 0.1666666667, 0.6666666667, 0.3333333333 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
            { 0.0, 0.0, 0.0 },
        },
        {    
            { 0.1012865073, 0.1012865073, 0.1259391805 },
            { 0.7974269854, 0.1012865073, 0.1259391805 },
            { 0.1012865073, 0.7974269854, 0.1259391805 },
            { 0.4701420641, 0.0597158718, 0.1323941527 },
            { 0.4701420641, 0.4701420641, 0.1323941527 },
            { 0.0597158718, 0.4701420641, 0.1323941527 },
            { 0.3333333333, 0.3333333333, 0.225        },
        }
    };

};
}
}

#endif // !defined(KER_EMAT_TRI_H)
