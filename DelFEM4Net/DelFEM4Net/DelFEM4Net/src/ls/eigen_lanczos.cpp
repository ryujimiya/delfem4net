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
#define for if(0); else for

#include <iostream>
#include <math.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/ls/eigen_lanczos.h"
#include "DelFEM4Net/ls/preconditioner.h"
#include "DelFEM4Net/ls/linearsystem.h"
#include "DelFEM4Net/ls/solver_ls_iter.h"

using namespace DelFEM4NetLsSol;

double CEigenLanczos::MinimumEigenValueVector_InvPower(
        DelFEM4NetLsSol::CLinearSystem^ ls,
        DelFEM4NetLsSol::CPreconditioner^ pls,
        IList<unsigned int>^ aIdVec, 
        unsigned int itr_invp,    // 逆べき乗法の最大反復回数
        unsigned int itr_lssol,   // ICCG法の最大反復回数
        double conv_res_lssol,    // ICCG法の収束基準の相対残差
        [Out] int% iflag_conv )   // 0:正常に終了  1:ICCGが収束しなかった
{
    std::vector<unsigned int> aIdVec_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdVec, aIdVec_);
    int iflag_conv_ = 0;
    
    double ret = LsSol::MinimumEigenValueVector_InvPower(
        *(ls->Self),
        *(pls->PrecondSelf),
        aIdVec_,
        itr_invp,
        itr_lssol,
        conv_res_lssol,
        iflag_conv_);
        
    iflag_conv = iflag_conv_;
    
    return ret;
}
