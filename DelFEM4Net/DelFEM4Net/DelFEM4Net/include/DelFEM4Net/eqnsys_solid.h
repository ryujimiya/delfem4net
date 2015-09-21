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
@brief 固体の方程式クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#if !defined(DELFEM4NET_EQN_SYS_SOLID_H)
#define DELFEM4NET_EQN_SYS_SOLID_H

#include <vector>
#include <iostream>

#include "DelFEM4Net/eqnsys.h"

#include "delfem/eqnsys_solid.h"

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
@brief ３次元固体方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Solid3D_Linear : public CEqnSystem
{
public:
    CEqn_Solid3D_Linear();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Solid3D_Linear(const CEqn_Solid3D_Linear% rhs);
public:
    // nativeなライブラリで実装されていない
    //CEqn_Solid3D_Linear(DelFEM4NetFem::Field::CFieldWorld^ world);
    CEqn_Solid3D_Linear(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Solid3D_Linear(Fem::Eqn::CEqn_Solid3D_Linear *self);
public:
    virtual ~CEqn_Solid3D_Linear();
    !CEqn_Solid3D_Linear();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqn_Solid3D_Linear * Self
    {
        Fem::Eqn::CEqn_Solid3D_Linear * get();
    }

    // 仮想化可能関数
    virtual bool SetDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;

    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    unsigned int GetIdField_Disp();

    // 非virtual
    void SetYoungPoisson( double young, double poisson );

    //! 重力加速度設定
    void SetGravitation( double g_x, double g_y, double g_z );

    void SetGeometricalNonLinear();

    void UnSetGeometricalNonLinear();

    void SetSaveStiffMat();
    void UnSetSaveStiffMat();

    void SetStationary();
    void UnSetStationary();
    
protected:
    Fem::Eqn::CEqn_Solid3D_Linear *self;

};


////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////

/*! 
@brief ２次元固体方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Solid2D
{
public:
    CEqn_Solid2D();
    CEqn_Solid2D(const CEqn_Solid2D% rhs);
    CEqn_Solid2D(unsigned int id_ea, unsigned int id_field_disp);
    CEqn_Solid2D(Fem::Eqn::CEqn_Solid2D *self);
    virtual ~CEqn_Solid2D();
    !CEqn_Solid2D();
    property Fem::Eqn::CEqn_Solid2D * Self
    {
        Fem::Eqn::CEqn_Solid2D * get();
    }
    
    unsigned int GetIdField_Disp();
    void CopyParameters(CEqn_Solid2D^ eqn );

    // Setメソッド
    void SetYoungPoisson( double young, double poisson, bool is_plane_stress );
    //! ラメ定数の取得
    void GetLambdaMyu( [Out] double% lambda, [Out] double% myu);
    //! 質量密度の取得
    double GetRho();
    void GetYoungPoisson( [Out] double% young, [Out] double% poisson);
    //! 質量密度の設定
    void SetRho(  double rho);
    //! 体積力の設定
    void SetGravitation( double g_x, double g_y );
    void GetGravitation( [Out] double% g_x, [Out] double% g_y );
    void SetIdFieldDisp(unsigned int id_field_disp);
    void SetGeometricalNonlinear(bool is_nonlin);
    void SetThermalStress(unsigned int id_field_temp);
    // Getメソッド
    unsigned int GetIdEA();
    bool IsTemperature();
    bool IsGeometricalNonlinear();

    // 連立一次方程式マージメソッド
    bool AddLinSys_NewmarkBetaAPrime( double dt, double gamma, double beta, bool is_inital, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, Field::CFieldWorld^ world );
    bool AddLinSys_NewmarkBetaAPrime_Save( Ls::CLinearSystem_SaveDiaM_NewmarkBeta^ ls, Field::CFieldWorld^ world );
    bool AddLinSys( DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, Field::CFieldWorld^ world );
    bool AddLinSys_Save( DelFEM4NetFem::Ls::CLinearSystem_Save^ ls, Field::CFieldWorld^ world );

protected:
    Fem::Eqn::CEqn_Solid2D *self;
};


//2Dと3Dは分ける．外力の次元が違うし、平面応力平面歪の設定があるから．


/*!
@brief ２次元固体，連成方程式クラス
@ingroup FemEqnSystem
*/
public ref class CEqnSystem_Solid2D : public CEqnSystem
{
public:
    CEqnSystem_Solid2D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Solid2D(const CEqnSystem_Solid2D% rhs);
public:
    // nativeなライブラリで実装されていない
    //CEqnSystem_Solid2D(DelFEM4NetFem::Field::CFieldWorld^ world);
    CEqnSystem_Solid2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Solid2D(Fem::Eqn::CEqnSystem_Solid2D *self);
public:
    virtual ~CEqnSystem_Solid2D();
    !CEqnSystem_Solid2D();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqnSystem_Solid2D * Self
    {
        Fem::Eqn::CEqnSystem_Solid2D * get();
    }

    // 仮想化可能関数
    virtual bool UpdateDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool SetDomain_FieldEA(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world);
    //! 方程式を解く
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual void Clear() override;

    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    unsigned int GetIdField_Disp();

    //nativeなライブラリで実装されていない
    //bool ToplogicalChangeCad_InsertLoop(DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_l_back, unsigned id_l_ins);

    ////////////////////////////////
    // 非virtual
    
    //! 方程式の取得，設定
    //! @{
    bool SetEquation( CEqn_Solid2D^ eqn );
    CEqn_Solid2D^ GetEquation(unsigned int id_ea);
    //! @}

    ////////////////
    // 各領域の方程式のパラメータを一度に変えるオプション

    //! @{
    void SetYoungPoisson( double young, double poisson, bool is_plane_stress ); //!< 剛性パラメータ設定
    void SetRho( double rho ); //!< 密度設定
    void SetGravitation( double g_x, double g_y ); //!< 重力加速度設定
    void SetThermalStress(unsigned int id_field_temp);    //!< 熱応力を考慮する０を代入するとで解除される
    void SetGeometricalNonlinear(bool is_nonlin);    //!< 幾何学的非線形性を考慮する
    void SetStationary(bool is_stationary);    //!< 静的問題に設定
    void SetSaveStiffMat(bool is_save);        //!< 剛性行列を保存する
    
    bool IsStationary();
    bool IsSaveStiffMat();
    void GetYoungPoisson([Out] double% young, [Out] double% poisson, [Out] bool% is_plane_str);
    bool IsGeometricalNonlinear();
    double GetRho();

    //! @}

    //! 荷重の設定
    void SetLoad(double load, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world);
    void ClearLoad(unsigned int id_ea);
    void ClearLoad();

    //! 変位から応力の値を計算して場(ID:id_field)にセットする
    bool SetEquivStressValue(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    bool SetStressValue(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);

protected:
    Fem::Eqn::CEqnSystem_Solid2D *self;
    
};

}
}

#endif

