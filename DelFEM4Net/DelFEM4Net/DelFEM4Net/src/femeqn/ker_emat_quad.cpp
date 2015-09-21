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

#include "DelFEM4Net/femeqn/ker_emat_quad.h"

using namespace DelFEM4NetFem::Eqn;

////////////////////////////////////////////////////////////////////////////////
// nativeライブラリではnamespaceの指定がないので、区別するためここで指定している
namespace Fem
{
namespace Eqn
{
static void ShapeFunc_Quad4(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx[][2],    // 積分点における形状関数の微分値
    double an[] );        // 積分点における形状関数の値

static void ShapeFunc_Quad5(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_b[2],    // 積分点における形状関数の微分値
    double an_c[],        // 積分点における形状関数の値
    double& an_b );        // 積分点における形状関数の値

static void ShapeFunc_Quad8(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_e[][2],    // 積分点における形状関数の微分値
    double an_c[],         // 積分点における形状関数の値
    double an_e[] );        // 積分点における形状関数の値

static void ShapeFunc_Quad9(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_e[][2],    // 積分点における形状関数の微分値
    double dndx_b[2],    // 積分点における形状関数の微分値
    double an_c[],         // 積分点における形状関数の値
    double an_e[],        // 積分点における形状関数の値
    double& an_b );        // 積分点における形状関数の値
}
}

static void Fem::Eqn::ShapeFunc_Quad4(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx[][2],    // 積分点における形状関数の微分値
    double an[] )        // 積分点における形状関数の値
{
    ::ShapeFunc_Quad4(
        r1, r2,
        coords,
        detjac,
        dndx,
        an);
}

static void Fem::Eqn::ShapeFunc_Quad5(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_b[2],    // 積分点における形状関数の微分値
    double an_c[],        // 積分点における形状関数の値
    double& an_b )        // 積分点における形状関数の値
{
    ::ShapeFunc_Quad5(
        r1, r2,
        coords,
        detjac,
        dndx_c,
        dndx_b,
        an_c,
        an_b);
}

static void Fem::Eqn::ShapeFunc_Quad8(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_e[][2],    // 積分点における形状関数の微分値
    double an_c[],         // 積分点における形状関数の値
    double an_e[] )        // 積分点における形状関数の値
{
    ::ShapeFunc_Quad8(
        r1, r2,
        coords,
        detjac,
        dndx_c,
        dndx_e,
        an_c,
        an_e);
}

static void Fem::Eqn::ShapeFunc_Quad9(
    const double& r1, const double& r2,    // (入力)積分点の自然座標における位置
    const double coords[][2],            // (入力)節点座標
    double& detjac,        // 積分点におけるヤコビアンの値
    double dndx_c[][2],    // 積分点における形状関数の微分値
    double dndx_e[][2],    // 積分点における形状関数の微分値
    double dndx_b[2],    // 積分点における形状関数の微分値
    double an_c[],         // 積分点における形状関数の値
    double an_e[],        // 積分点における形状関数の値
    double& an_b )        // 積分点における形状関数の値
{
    ::ShapeFunc_Quad9(
        r1, r2,
        coords,
        detjac,
        dndx_c,
        dndx_e,
        dndx_b,
        an_c,
        an_e,
        an_b);
}
////////////////////////////////////////////////////////////////////////////////


void CKerEMatQuad::ShapeFunc_Quad4(
        double r1, double r2,
        array<double, 2>^ coords,
        [Out] double% detjac,
        [Out] array<double, 2>^% dndx,
        [Out] array<double>^% an )
{
    assert(coords->GetLength(0) == NODE_CNT);
    assert(coords->GetLength(1) == VEC_DIM);
    pin_ptr<double> coords_ = &coords[0, 0];
    double detjac_;
    dndx = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_ = &dndx[0, 0];
    an = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_ = &an[0];
    
    Fem::Eqn::ShapeFunc_Quad4(
       r1, r2,
       (const double (*)[VEC_DIM])coords_,
       detjac_,
       (double (*)[VEC_DIM])dndx_,
       (double *)an_);
       
    detjac = detjac_;
}

void CKerEMatQuad::ShapeFunc_Quad5(
        double r1, double r2,
        array<double, 2>^ coords,
        [Out] double% detjac,
        [Out] array<double, 2>^% dndx_c,
        [Out] array<double>^% dndx_b,
        [Out] array<double>^% an_c,
        [Out] double% an_b)
{
    assert(coords->GetLength(0) == NODE_CNT);
    assert(coords->GetLength(1) == VEC_DIM);
    pin_ptr<double> coords_ = &coords[0, 0];
    double detjac_;
    dndx_c = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_c_ = &dndx_c[0, 0];
    dndx_b = gcnew array<double>(VEC_DIM);
    pin_ptr<double> dndx_b_ = &dndx_b[0];
    an_c = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_c_ = &an_c[0];
    double an_b_;
    
    Fem::Eqn::ShapeFunc_Quad5(
       r1, r2,
       (const double (*)[VEC_DIM])coords_,
       detjac_,
       (double (*)[VEC_DIM])dndx_c_,
       (double *)dndx_b_,
       (double *)an_c_,
       an_b_);
    
    detjac = detjac_;
    an_b = an_b_;
}

void CKerEMatQuad::ShapeFunc_Quad8(
        double r1, double r2,
        array<double, 2>^ coords,
        [Out] double% detjac,
        [Out] array<double, 2>^% dndx_c,
        [Out] array<double, 2>^% dndx_e,
        [Out] array<double>^% an_c,
        [Out] array<double>^% an_e)
{
    assert(coords->GetLength(0) == NODE_CNT);
    assert(coords->GetLength(1) == VEC_DIM);
    pin_ptr<double> coords_ = &coords[0, 0];
    double detjac_;
    dndx_c = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_c_ = &dndx_c[0, 0];
    dndx_e = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_e_ = &dndx_e[0, 0];
    an_c = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_c_ = &an_c[0];
    an_e = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_e_ = &an_e[0];
    
    Fem::Eqn::ShapeFunc_Quad8(
       r1, r2,
       (const double (*)[VEC_DIM])coords_,
       detjac_,
       (double (*)[VEC_DIM])dndx_c_,
       (double (*)[VEC_DIM])dndx_e_,
       (double *)an_c_,
       (double *)an_e_);
    
    detjac = detjac_;
}


void CKerEMatQuad::ShapeFunc_Quad9(
        double r1, double r2,
        array<double, 2>^ coords,
        [Out] double% detjac,
        [Out] array<double, 2>^% dndx_c,
        [Out] array<double, 2>^% dndx_e,
        [Out] array<double>^% dndx_b,
        [Out] array<double>^% an_c,
        [Out] array<double>^% an_e,
        [Out] double% an_b)
{
    assert(coords->GetLength(0) == NODE_CNT);
    assert(coords->GetLength(1) == VEC_DIM);
    pin_ptr<double> coords_ = &coords[0, 0];
    double detjac_;
    dndx_c = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_c_ = &dndx_c[0, 0];
    dndx_e = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dndx_e_ = &dndx_e[0, 0];
    dndx_b = gcnew array<double>(VEC_DIM);
    pin_ptr<double> dndx_b_ = &dndx_b[0];
    an_c = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_c_ = &an_c[0];
    an_e = gcnew array<double>(NODE_CNT);
    pin_ptr<double> an_e_ = &an_e[0];
    double an_b_;
    
    Fem::Eqn::ShapeFunc_Quad9(
       r1, r2,
       (const double (*)[VEC_DIM])coords_,
       detjac_,
       (double (*)[VEC_DIM])dndx_c_,
       (double (*)[VEC_DIM])dndx_e_,
       (double *)dndx_b_,
       (double *)an_c_,
       (double *)an_e_,
       an_b_);
    
    detjac = detjac_;
    an_b = an_b_;
}
