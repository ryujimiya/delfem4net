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
@brief 六面体要素で要素剛性行列を作る際のUtility関数の集まり
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_KER_EMAT_HEX_H)
#define DELFEM4NET_KER_EMAT_HEX_H

#include <cassert>

#include "DelFEM4Net/femeqn/ker_emat_bar.h"

#include "delfem/femeqn/ker_emat_hex.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Eqn
{

public ref class CKerEMatHex
{
public:
    static const int VEC_DIM         = 3;  // 座標の次元
    static const int NODE_CNT        = 8;  // 節点の数
public:
    /*!
    @brief 積分点における形状関数の値とその微分を作る関数
    */
    static void ShapeFunc_Hex8(
        double r1, double r2, double r3,    // (入力)積分点の自然座標における位置
        array<double, 2>^ coords,           // (入力)節点座標
        [Out] double% detjac,               // 積分点におけるヤコビアンの値
        [Out] array<double, 2>^% dndx,      // 積分点における形状関数の微分値
        [Out] array<double>^% an );         // 積分点における形状関数の値
};
}
}

#endif
