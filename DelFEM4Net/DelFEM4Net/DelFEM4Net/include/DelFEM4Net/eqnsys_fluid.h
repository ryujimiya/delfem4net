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
@brief 流体の方程式クラス(DelFEM4NetFem::Eqn::CEqnSystem_Fluid2D, DelFEM4NetFem::Eqn::CEqn_Fluid2D, DelFEM4NetFem::Eqn::CEqn_Fluid3D,)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/



#if !defined(DELFEM4NET_EQN_OBJ_FLUID_H)
#define DELFEM4NET_EQN_OBJ_FLUID_H

#include <vector>
#include <cassert>
#include "DelFEM4Net/eqnsys.h"

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "delfem/eqnsys_fluid.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


namespace DelFEM4NetFem
{

namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}
namespace Eqn
{

/*! 
@brief ２次元流体方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Fluid2D
{
public:
    //CEqn_Fluid2D();
    CEqn_Fluid2D(const CEqn_Fluid2D% rhs);  // nativeライブラリ内では普通にoperator=でコピーしているので復活させる
    CEqn_Fluid2D(unsigned int id_ea, unsigned int id_velo, unsigned int id_press);
    CEqn_Fluid2D(Fem::Eqn::CEqn_Fluid2D *self);
    virtual ~CEqn_Fluid2D();
    !CEqn_Fluid2D();
    property Fem::Eqn::CEqn_Fluid2D * Self
    {
        Fem::Eqn::CEqn_Fluid2D * get();
    }

    bool IsNavierStokes();
    //! 密度を設定する
    void SetRho(double rho);
    double GetRho();
    //! 粘性係数を設定する
    void SetMyu(double myu);
    double GetMyu();
    //! 外力項
    void SetBodyForce(double g_x, double g_y );
    void SetStokes();
    void SetNavierStokes();
    void SetNavierStokesALE(unsigned int id_field_msh_velo);
    bool AddLinSys(DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world );
    bool AddLinSys_NewmarkBetaAPrime( double dt, double gamma, double beta, bool is_initial, 
        DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world );

    unsigned int GetIdEA();
    void SetIdEA(unsigned int id_ea);
    void SetIdFieldVelocity(unsigned int id_field_velo);
    void SetIdFieldPressure(unsigned int id_field_press);

protected:
    Fem::Eqn::CEqn_Fluid2D *self;
};

/*! 
@brief ２次元流体,連成方程式クラス
@ingroup FemEqnSystem
*/
public ref class CEqnSystem_Fluid2D : public CEqnSystem
{
public:
    CEqnSystem_Fluid2D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Fluid2D(const CEqnSystem_Fluid2D% rhs);
public:
    //nativeクラスに実装なし
    //CEqnSystem_Fluid2D(DelFEM4NetFem::Field::CFieldWorld^ world);
    CEqnSystem_Fluid2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Fluid2D(Fem::Eqn::CEqnSystem_Fluid2D *self);
public:
    virtual ~CEqnSystem_Fluid2D();
    !CEqnSystem_Fluid2D();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqnSystem_Fluid2D * Self
    {
        Fem::Eqn::CEqnSystem_Fluid2D * get();
    }

    ////////////////
    // virtual関数
    virtual bool UpdateDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool UpdateDomain_FieldVeloPress(unsigned int id_base_field_velo, unsigned int id_field_press, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool UpdateDomain_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world);

    ////////////////////////////////
    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  //idof = -1
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  //idof = -1
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  //idof = -1
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;

    ////////////////
    // 非virtual
    unsigned int GetIdField_Velo();
    unsigned int GetIdField_Press();
    // 静的か動的かを定める
    void SetIsStationary(bool is_stat);
    // 静的か動的かを得る
    bool GetIsStationary();
    // 方程式を全てクリアする
    virtual void Clear() override;
    // 方程式の要素IDの配列を得る
    IList<unsigned int>^ GetAry_EqnIdEA();
    // 要素IDがid_eaの方程式を得る
    CEqn_Fluid2D^ GetEquation(unsigned int id_ea);
    // 要素単位で方程式を定める．要素が存在しなければ方程式を追加する
    bool SetEquation( CEqn_Fluid2D^ eqn );
    // 要素IDを更新する
    void UpdateIdEA( IList<unsigned int>^ map_ea2ea );
    ////////////////
    void SetNavierStokes();
    void SetNavierStokesALE(unsigned int id_field_msh_velo);
    void SetForceField(unsigned int id_field_force);
    void SetInterpolationBubble();
    void UnSetInterpolationBubble();
    ////////////////////////////////
    void SetRho(double rho);
    void SetMyu(double myu);
    void SetStokes();

protected:
    Fem::Eqn::CEqnSystem_Fluid2D *self;
    
};

////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////
/*! 
@brief ３次元流体方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Fluid3D : public CEqnSystem
{
public:
    CEqn_Fluid3D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Fluid3D(const CEqn_Fluid3D% rhs);
public:
    //nativeクラスに実装なし
    //CEqn_Fluid3D(DelFEM4NetFem::Field::CFieldWorld^ world);
    CEqn_Fluid3D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Fluid3D(Fem::Eqn::CEqn_Fluid3D *self);
public:
    virtual ~CEqn_Fluid3D();
    !CEqn_Fluid3D();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqn_Fluid3D * Self
    {
        Fem::Eqn::CEqn_Fluid3D * get();
    }

    // 仮想化可能関数
    virtual bool SetDomain(unsigned int id_base, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;

    ////////////////////////////////
    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override; // idof = -1
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override; // idof = -1
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override; // idof = -1
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    void SetGravitation(double g_x, double g_y, double g_z);

    // 非virtual
    unsigned int GetIDField_Velo();
    unsigned int GetIDField_Press();

protected:
    Fem::Eqn::CEqn_Fluid3D *self;
    
};

}
}

#endif

