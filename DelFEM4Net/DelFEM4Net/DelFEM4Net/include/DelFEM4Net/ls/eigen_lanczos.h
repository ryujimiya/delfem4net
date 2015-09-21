/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

Copyright (C) 2009  Nobuyuki Umetani    n.umetani@gmail.com

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distriｑbuted in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

/*! @file
@brief 
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EIGEN_LANCZOS_H)
#define DELFEM4NET_EIGEN_LANCZOS_H

#include <vector>

#include "delfem/ls/eigen_lanczos.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


namespace DelFEM4NetLsSol
{
ref class CLinearSystem;
interface class CPreconditioner;

public ref class CEigenLanczos
{
public:
    // ls, pls : IN/OUT(ハンドルは変更なし)
    static double MinimumEigenValueVector_InvPower(
        DelFEM4NetLsSol::CLinearSystem^ ls,
        DelFEM4NetLsSol::CPreconditioner^ pls,
        IList<unsigned int>^ aIdVec, 
        unsigned int itr_invp,    // 逆べき乗法の最大反復回数
        unsigned int itr_lssol,   // ICCG法の最大反復回数
        double conv_res_lssol,    // ICCG法の収束基準の相対残差
        [Out] int% iflag_conv );  // 0:正常に終了  1:ICCGが収束しなかった
};

}

#endif
