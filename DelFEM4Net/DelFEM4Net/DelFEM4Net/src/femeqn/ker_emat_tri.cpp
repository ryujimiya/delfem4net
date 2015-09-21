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

#include "DelFEM4Net/femeqn/ker_emat_tri.h"

using namespace DelFEM4NetFem::Eqn;

////////////////////////////////////////////////////////////////////////////////
// nativeライブラリではnamespaceの指定がないので、区別するためここで指定している
namespace Fem
{
namespace Eqn
{

static double TriArea(const double p0[], const double p1[], const double p2[]);
static void TriAreaCoord(double vc_p[],
                         const double p0[], const double p1[], const double p2[], const double pb[] );
static void TriDlDx(double dldx[][2], double a[],
                              const double p0[], const double p1[], const double p2[]);

}
}

static double Fem::Eqn::TriArea(const double p0[], const double p1[], const double p2[])
{
    return ::TriArea(p0, p1, p2);
}

static void Fem::Eqn::TriAreaCoord(double vc_p[],
                         const double p0[], const double p1[], const double p2[], const double pb[] )
{
    ::TriAreaCoord(vc_p, p0, p1, p2, pb);
}

static void Fem::Eqn::TriDlDx(double dldx[][2], double a[],
                              const double p0[], const double p1[], const double p2[])
{
    ::TriDlDx(dldx, a, p0, p1, p2);
}
////////////////////////////////////////////////////////////////////////////////


// 三角形の面積を求める関数
double CKerEMatTri::TriArea(array<double>^ p0, array<double>^ p1, array<double>^ p2)
{
    assert(p0->Length == VEC_DIM);
    assert(p1->Length == VEC_DIM);
    assert(p2->Length == VEC_DIM);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    pin_ptr<double> p2_ = &p2[0];
    return Fem::Eqn::TriArea((const double *)p0_, (const double *)p1_, (const double *)p2_);
}

// ある点の面積座標を求める関数
void CKerEMatTri::TriAreaCoord([Out] array<double>^% vc_p,
                               array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ pb)
{
    vc_p = gcnew array<double>(NODE_CNT);
    pin_ptr<double> vc_p_ = &vc_p[0];

    assert(p0->Length == VEC_DIM);
    assert(p1->Length == VEC_DIM);
    assert(p2->Length == VEC_DIM);
    assert(pb->Length == VEC_DIM);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    pin_ptr<double> p2_ = &p2[0];
    pin_ptr<double> pb_ = &pb[0];
   
    Fem::Eqn::TriAreaCoord((double *)vc_p_, (const double *)p0_, (const double *)p1_, (const double *)p2_, (const double *)pb_);
}

// 三角形の一次補間関数の微分とその定数成分
void CKerEMatTri::TriDlDx([Out] array<double, 2>^% dldx, [Out] array<double>^% const_term,
                           array<double>^ p0, array<double>^ p1, array<double>^ p2)
{
    dldx = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dldx_ = &dldx[0, 0];
    const_term = gcnew array<double>(NODE_CNT);
    pin_ptr<double> const_term_ = &const_term[0];
    assert(p0->Length == VEC_DIM);
    assert(p1->Length == VEC_DIM);
    assert(p2->Length == VEC_DIM);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    pin_ptr<double> p2_ = &p2[0];
    
    Fem::Eqn::TriDlDx((double (*)[VEC_DIM])dldx_, (double *)const_term_, (const double *)p0_, (const double *)p1_, (const double *)p2_);
}
