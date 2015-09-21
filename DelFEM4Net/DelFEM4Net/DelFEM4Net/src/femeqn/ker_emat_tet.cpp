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

#include "DelFEM4Net/femeqn/ker_emat_tet.h"

using namespace DelFEM4NetFem::Eqn;


////////////////////////////////////////////////////////////////////////////////
// nativeライブラリではnamespaceの指定がないので、区別するためここで指定している
////////////////////////////////////////////////////////////////////////////////
// nativeライブラリではnamespaceの指定がないので、区別するためここで指定している
namespace Fem
{
namespace Eqn
{
static double TetVolume(const double p0[], const double p1[], const double p2[], const double p3[]);
static void TetDlDx(double dldx[][3], double a[],
    const double p0[], const double p1[], const double p2[], const double p3[]);
}
}

static double Fem::Eqn::TetVolume(const double p0[], const double p1[], const double p2[], const double p3[])
{
    return ::TetVolume(p0, p1, p2, p3);
}

static void Fem::Eqn::TetDlDx(double dldx[][3], double a[],
    const double p0[], const double p1[], const double p2[], const double p3[])
{
    return ::TetDlDx(dldx, a, p0, p1, p2, p3);
}
////////////////////////////////////////////////////////////////////////////////


double CKerEMatTet::TetVolume(array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ p3)
{
    assert(p0->Length == VEC_DIM);
    assert(p1->Length == VEC_DIM);
    assert(p2->Length == VEC_DIM);
    assert(p3->Length == VEC_DIM);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    pin_ptr<double> p2_ = &p2[0];
    pin_ptr<double> p3_ = &p3[0];
    return Fem::Eqn::TetVolume((const double *)p0_, (const double *)p1_, (const double *)p2_, (const double *)p3_);
}

void CKerEMatTet::TetDlDx([Out] array<double, 2>^ dldx, [Out] array<double>^ a,
                          array<double>^ p0, array<double>^ p1, array<double>^ p2, array<double>^ p3)
{
    dldx = gcnew array<double, 2>(NODE_CNT, VEC_DIM);
    pin_ptr<double> dldx_ = &dldx[0, 0];
    a = gcnew array<double>(NODE_CNT);
    pin_ptr<double> a_ = &a[0];
    assert(p0->Length == VEC_DIM);
    assert(p1->Length == VEC_DIM);
    assert(p2->Length == VEC_DIM);
    assert(p3->Length == VEC_DIM);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    pin_ptr<double> p2_ = &p2[0];
    pin_ptr<double> p3_ = &p3[0];

    Fem::Eqn::TetDlDx((double (*)[VEC_DIM])dldx_, (double *)a_, (const double *)p0_, (const double *)p1_, (const double *)p2_, (const double *)p3_);
}
