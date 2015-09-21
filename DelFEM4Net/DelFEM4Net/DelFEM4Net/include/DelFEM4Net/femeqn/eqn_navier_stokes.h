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
@brief ナビア・ストークス方程式の要素剛性作成部のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_NAVIER_STOKES_H)
#define DELFEM4NET_EQN_NAVIER_STOKES_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "delfem/femeqn/eqn_navier_stokes.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Ls
{
    ref class CLinearSystem_Field;
}

namespace Field{

    ref class CField;
    ref class CFieldWorld;
}
namespace Eqn
{

/*! @defgroup eqn_ns ナビア・ストークス方程式の連立一次方程式へのマージ関数群
@ingroup FemEqnMargeFunction
*/

public ref class CEqnNavierStokes
{
public:
    /*! 
    @brief ナビア・ストークス方程式の連立一次方程式を作成するクラス
    @param [in] rho 質量密度
    @param [in] alpha 粘性
    @param [in] g_x ｘ方向の体積力
    @param [in] g_y ｙ方向の体積力
    @param [in] gamma Newmark法のパラメータ
    @param [in] dt 時間刻み
    @param [in] id_field_velo 流速場のID
    @param [in] id_field_press 圧力場のID
    */
    bool AddLinSys_NavierStokes2D_NonStatic_Newmark(
            double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
            double rho, double alpha, double g_x, double g_y,
            unsigned int id_field_velo, unsigned int id_field_press, 
            DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        return AddLinSys_NavierStokes2D_NonStatic_Newmark(
            dt, gamma, ls, 
            rho, alpha, g_x, g_y,
            id_field_velo, id_field_press, 
            world,
            0);
    }
    bool AddLinSys_NavierStokes2D_NonStatic_Newmark(
        double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        double rho, double alpha,      double g_x, double g_y,
        unsigned int id_field_velo, unsigned int id_field_press, 
        DelFEM4NetFem::Field::CFieldWorld^ world,
        unsigned int id_ea);
    
    /*! 
    @brief ALE座標におけるナビア・ストークス方程式の連立一次方程式を作成するクラス
    @param [in] rho 質量密度
    @param [in] alpha 粘性
    @param [in] g_x ｘ方向の体積力
    @param [in] g_y ｙ方向の体積力
    @param [in] gamma Newmark法のパラメータ
    @param [in] dt 時間刻み
    @param [in] id_field_velo 流速場のID
    @param [in] id_field_press 圧力場のID
    */
    bool AddLinSys_NavierStokesALE2D_NonStatic_Newmark(
            double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
            double rho, double alpha, double g_x, double g_y,
            unsigned int id_field_velo, unsigned int id_field_press, unsigned int id_field_msh_velo,
            DelFEM4NetFem::Field::CFieldWorld^ world );
    
    /*! 
    @brief 熱の浮力をうけるナビア・ストークス方程式の連立一次方程式を作成するクラス
    @param [in] rho 質量密度
    @param [in] alpha 粘性
    @param [in] g_x ｘ方向の体積力
    @param [in] g_y ｙ方向の体積力
    @param [in] gamma Newmark法のパラメータ
    @param [in] dt 時間刻み
    @param [in] id_field_velo 流速場のID
    @param [in] id_field_press 圧力場のID
    */
    bool AddLinSys_NavierStokes2DThermalBuoy_NonStatic_Newmark(
        double rho, double alpha, double g_x, double g_y,
        double gamma, double dt,
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        unsigned int id_field_velo, unsigned int id_field_press, unsigned int id_field_temp,
        DelFEM4NetFem::Field::CFieldWorld^ world );

};

}
}

#endif
