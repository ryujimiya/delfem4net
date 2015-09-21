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


/*! @file:
@brief 四面体要素で要素剛性行列を作る際のUtility関数の集まり
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_KER_EMAT_TET_H)
#define DELFEM4NET_KER_EMAT_TET_H

#include <cassert>

#include "delfem/femeqn/ker_emat_tet.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Eqn
{
public ref class CKerEMatTet
{
public:
    static const int VEC_DIM         = 3;  // 座標の次元
    static const int NODE_CNT        = 4;  // 節点の数
public:
    static double TetVolume(array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ p3);

    // caluculate Derivative of Area Coord
    static void TetDlDx([Out] array<double, 2>^ dldx, [Out] array<double>^ a,
                        array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ p3);

    static initonly array<unsigned int>^ NIntTetGauss = gcnew array<unsigned int>(4){ // 積分点の数
        1, 4, 5, 16
    };
    static initonly array<double, 3>^ TetGauss = gcnew array<double, 3>(4, 16, 4) // 積分点の位置(r1,r2)と重みの配列
    {
        {    // order-1    1point
            { 0.25, 0.25, 0.25, 1.0 },
        },
        {    // order-2    4point
            { 0.585410196624968, 0.138196601125015, 0.138196601125015, 0.25 },
            { 0.138196601125015, 0.585410196624968, 0.138196601125015, 0.25 },
            { 0.138196601125015, 0.138196601125015, 0.585410196624968, 0.25 },
            { 0.138196601125015, 0.138196601125015, 0.138196601125015, 0.25 },
        },
        {    // order-3    5point
            { 0.25, 0.25, 0.25, -0.8 },
            { 0.5               , 0.1666666666666667, 0.1666666666666667, 0.45 },
            { 0.1666666666666667, 0.5,                0.1666666666666667, 0.45 },
            { 0.1666666666666667, 0.1666666666666667, 0.5,                0.45 },
            { 0.1666666666666667, 0.1666666666666667, 0.1666666666666667, 0.45 },
        },
        {    // order-4    16point
            { 0.7716429020672371 , 0.07611903264425430, 0.07611903264425430, 0.05037379410012282 },
            { 0.07611903264425430, 0.7716429020672371,  0.07611903264425430, 0.05037379410012282 },
            { 0.07611903264425430, 0.07611903264425430, 0.7716429020672371,  0.05037379410012282 },
            { 0.07611903264425430, 0.07611903264425430, 0.07611903264425430, 0.05037379410012282 },
   
            { 0.1197005277978019,  0.4042339134672644,  0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.1197005277978019,  0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.4042339134672644,  0.1197005277978019,  0.06654206863329239 },
    
            { 0.07183164526766925, 0.4042339134672644,  0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.07183164526766925, 0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.4042339134672644,  0.07183164526766925, 0.06654206863329239 },
    
            { 0.1197005277978019,  0.07183164526766925, 0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.1197005277978019,  0.07183164526766925, 0.06654206863329239 },
            { 0.07183164526766925, 0.4042339134672644,  0.1197005277978019,  0.06654206863329239 },
    
            { 0.07183164526766925, 0.1197005277978019,  0.4042339134672644,  0.06654206863329239 },
            { 0.4042339134672644,  0.07183164526766925, 0.1197005277978019,  0.06654206863329239 },
            { 0.1197005277978019,  0.4042339134672644,  0.07183164526766925, 0.06654206863329239 },
        }
    };
};
}
}

#endif
