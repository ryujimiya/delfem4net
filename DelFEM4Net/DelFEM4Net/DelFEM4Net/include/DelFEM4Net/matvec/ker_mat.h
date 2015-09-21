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


#if !defined(DELFEM4NET_KER_MAT_H)
#define DELFEM4NET_KER_MAT_H

#include <cassert>
//#include <math.h>

//////////////////////////////////////////////////////////////////////
namespace MatVec  // delfem/ker_mat.hのnamespaceがないので暫定的に追加
{
//////////////////////////////////////////////////////////////////////
#include "delfem/matvec/ker_mat.h"
//////////////////////////////////////////////////////////////////////
#undef KER_MAT_H // delfem/ker_mat.hのnamespaceがないので暫定的に追加
} // delfem/ker_mat.hのnamespaceがないので暫定的に追加
//////////////////////////////////////////////////////////////////////

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
public ref class KerMat
{
public:
    //! ３×３の行列の逆行列を求める
    static void CalcInvMat3(array<double, 2>^ a, [Out]array<double, 2>^% a_inv, [Out] double% det)
    {
        pin_ptr<double> a_ptr_ = &a[0, 0];
        double (*a_)[3] = (double (*)[3])a_ptr_;
        
        a_inv = gcnew array<double, 2>(3, 3);
        pin_ptr<double> a_inv_ptr_ = &a_inv[0, 0];
        double (*a_inv_)[3] = (double (*)[3])a_inv_ptr_;
        
        double det_ = 0;
        
        MatVec::CalcInvMat3(a_, a_inv_, det_);
        
        det = det_;
    }

    /*! 
    @brief 行列の逆行列を求める
    このアルゴリズムはn^3のオーダーを持つので密行列でしかも小さな行列以外使わないこと
    @param a 行列の値ｎ×n
    @param n 行列のサイズ
    @param info １なら特異行列，－なら不定行列，０なら正定行列
    */
    static void CalcInvMat(array<double>^% a, unsigned int n, [Out] int% info )
    {
        array<double>^ out_a = gcnew array<double>(a->Length);
        a->CopyTo(out_a, 0);
        a = out_a;
        pin_ptr<double> ptr = &a[0];
        double *a_ = (double *)ptr;
        int info_;
        
        MatVec::CalcInvMat(a_, n, info_);
        
        info = info_;
    }
};

}

#endif
