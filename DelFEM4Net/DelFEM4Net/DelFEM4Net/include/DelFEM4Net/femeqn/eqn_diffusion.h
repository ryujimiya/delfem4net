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

/*! @file
@brief 拡散方程式の要素剛性作成部のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_DIFFUSION_H)
#define DELFEM4NET_EQN_DIFFUSION_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>

#include "delfem/femeqn/eqn_diffusion.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Ls
{
    ref class CLinearSystem_Field;
    ref class CLinearSystem_SaveDiaM_Newmark;
    ref class CPreconditioner;
}
namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}

namespace Eqn
{

/*! @defgroup eqn_diffusion 拡散方程式を連立一次方程式にマージする関数群
@ingroup FemEqnMargeFunction
　　
@f$ \rho \frac{\partial\phi}{\partial t} = -\mu \nabla^2\phi + f@f$

拡散方程式とは，濃度勾配に比例して流束(Flux)が決定されるような場の方程式を記述する偏微分方程式です．
例えば熱や，移流のない場の濃度変化などがこれに従います．
*/
//! @{

public ref class CEqnDiffusion
{
public:
    /*!
    @brief 拡散方程式のマージ
    @param [in,out] ls 連立一次方程式
    @param [in] rho 熱容量 @f$\rho@f$
    @param [in] alpha 熱拡散係数 @f$\mu@f$
    @param [in] source ソース @f$ f @f$
    @param [in] world 場管理クラス
    @param [in] id_field_val 値場のID
    */
    static bool AddLinSys_Diffusion(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val)
    {
        return AddLinSys_Diffusion(
                   dt, gamma,
                   ls,
                   rho, alpha, source,
                   world, id_field_val,
                   0);
    }
    static bool AddLinSys_Diffusion(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val,
        unsigned int id_ea); // id_ea = 0
    
    /*!
    @brief 軸対称，拡散方程式のマージ
    @param [in,out] ls 連立一次方程式
    @param [in] rho 熱容量 @f$\rho@f$
    @param [in] alpha 熱拡散係数 @f$\mu@f$
    @param [in] source ソース @f$ f @f$
    @param [in] world 場管理クラス
    @param [in] id_field_val 値場のID
    @remark 今は軸はｙ軸に一致しているとするが，そのうちこれも入力にする．
    */    
    static bool AddLinSys_Diffusion_AxSym(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val)
    {
        return AddLinSys_Diffusion_AxSym(
                   dt, gamma,
                   ls,
                   rho, alpha, source,
                   world, id_field_val,
                   0);
    }
    static bool AddLinSys_Diffusion_AxSym(
        double dt, double gamma,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_val,
        unsigned int id_ea); // id_ea = 0
    
    /*!
    @brief 拡散方程式(剛性行列保存)のマージ
    @param [in,out] ls 連立一次方程式(剛性行列保存型)
    @param [in] rho 熱容量 @f$\rho@f$
    @param [in] alpha 熱拡散係数 @f$\mu@f$
    @param [in] source ソース @f$ f @f$
    @param [in] world 場管理クラス
    @param [in] id_field_val 値場のID
    */
    static bool AddLinSys_Diffusion(
        DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val)
    {
        return AddLinSys_Diffusion(
                   ls,
                   rho, alpha, source,
                   world,
                   id_field_val, 0);
    }
    static bool AddLinSys_Diffusion(
        DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls,
        double rho, double alpha, double source,
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_field_val, unsigned int id_ea); // id_ea = 0
};

//! @}

}    // end namespace Eqn
}    // end namespace Fem

#endif
