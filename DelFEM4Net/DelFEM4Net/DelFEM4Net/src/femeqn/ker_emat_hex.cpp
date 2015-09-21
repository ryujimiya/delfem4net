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

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif
#define for if(0);else for

#include <cassert>
#include <math.h>

#include "DelFEM4Net/femeqn/ker_emat_hex.h"

using namespace DelFEM4NetFem::Eqn;

////////////////////////////////////////////////////////////////////////////////
// nativeライブラリではnamespaceの指定がないので、区別するためここで指定している
namespace Fem
{
namespace Eqn
{
static void ShapeFunc_Hex8(
    const double& r1, const double& r2,    const double& r3,    // (入力)積分点の自然座標における位置
    const double coords[][3],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx[][3],    // 積分点における形状関数の微分値
    double an[] );        // 積分点における形状関数の値
}
}

static void Fem::Eqn::ShapeFunc_Hex8(
    const double& r1, const double& r2,    const double& r3,    // (入力)積分点の自然座標における位置
    const double coords[][3],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx[][3],    // 積分点における形状関数の微分値
    double an[] )        // 積分点における形状関数の値
{
    ::ShapeFunc_Hex8(
        r1, r2, r3,
        coords,
        detjac,
        dndx,
        an);
}
////////////////////////////////////////////////////////////////////////////////

void CKerEMatHex::ShapeFunc_Hex8(
        double r1, double r2, double r3,    // (入力)積分点の自然座標における位置
        array<double, 2>^ coords,           // (入力)節点座標
        [Out] double% detjac,               // 積分点におけるヤコビアンの値
        [Out] array<double, 2>^% dndx,      // 積分点における形状関数の微分値
        [Out] array<double>^% an )         // 積分点における形状関数の値
{
    assert(coords->GetLength(0) == NODE_CNT);
    assert(coords->GetLength(1) == VEC_DIM);
    pin_ptr<double> coords_ = &coords[0, 0];
    double detjac_;
    dndx = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_ = &dndx[0, 0];
    an = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_ = &an[0];

    Fem::Eqn::ShapeFunc_Hex8(
       r1, r2, r3,
       (const double (*)[VEC_DIM])coords_,
       detjac_,
       (double (*)[VEC_DIM])dndx_,
       (double *)an_);
       
    detjac = detjac_;
}
