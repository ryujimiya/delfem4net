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
@brief スカラー型の方程式クラス(Fem::Eqn::CEqnSystem_Scalar2D, Fem::Eqn::CEqn_Scalar2D, Fem::Eqn::CEqn_Scalar3D)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)

@li ポアソン方程式
@li 拡散方程式
@li 静的移流拡散方程式
@li 動的移流拡散方程式
*/



#if !defined(DELFEM4NET_EQN_OBJ_SCALAR_H)
#define DELFEM4NET_EQN_OBJ_SCALAR_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>
#include "DelFEM4Net/eqnsys.h"

#include "delfem/eqnsys_scalar.h"

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
namespace Eqn{

/*! 
@brief 2Dのスカラー型方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Scalar2D
{
public:
    //CEqn_Scalar2D();
    CEqn_Scalar2D(const CEqn_Scalar2D% rhs);  // nativeライブラリ内では普通にoperator=でコピーしているので復活させる
    CEqn_Scalar2D(unsigned int id_ea, unsigned int id_field_val);
    CEqn_Scalar2D(Fem::Eqn::CEqn_Scalar2D *self);
    virtual ~CEqn_Scalar2D();
    !CEqn_Scalar2D();
    property Fem::Eqn::CEqn_Scalar2D * Self
    {
        Fem::Eqn::CEqn_Scalar2D * get();
    }

    // Setメソッド
    //! @{
    void SetAlpha(   double alpha );
    void SetCapacity(double capa  );
    void SetSource(  double source);
    void SetAdvection(unsigned int id_field_advec);
    //! @}
    double GetAlpha();
    double GetCapacity();
    double GetSource();
    // Getメソッド
    unsigned int GetIdEA();
    bool IsAdvection();

    // 連立一次方程式マージメソッド
    bool AddLinSys( DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, DelFEM4NetFem::Field::CFieldWorld^ world );
    bool AddLinSys_Newmark( double dt, double gamma, DelFEM4NetFem::Ls::CLinearSystem_Field^ ls, 
        bool is_ax_sym,
        DelFEM4NetFem::Field::CFieldWorld^ world );
    bool AddLinSys_Save( DelFEM4NetFem::Ls::CLinearSystem_Save^ ls, DelFEM4NetFem::Field::CFieldWorld^ world );
    bool AddLinSys_SaveKDiaC( DelFEM4NetFem::Ls::CLinearSystem_SaveDiaM_Newmark^ ls, DelFEM4NetFem::Field::CFieldWorld^ world );

protected:
    Fem::Eqn::CEqn_Scalar2D *self;
};
    
////////////////////////////////////////////////////////////////

/*! 
@ingroup FemEqnSystem
@brief 2Dのスカラー型,連成方程式クラス

解くことができる方程式
@li ポアソン方程式
@li 拡散方程式
@li 静的移流拡散方程式
@li 動的移流拡散方程式
*/
public ref class CEqnSystem_Scalar2D : public CEqnSystem
{
public:
    CEqnSystem_Scalar2D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Scalar2D(const CEqnSystem_Scalar2D% rhs);
public:
    //nativeライブラリで実装されていない
    //CEqnSystem_Scalar2D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem_Scalar2D(Fem::Eqn::CEqnSystem_Scalar2D *self);
public:
    virtual ~CEqnSystem_Scalar2D();
    !CEqnSystem_Scalar2D();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqnSystem_Scalar2D * Self
    {
        Fem::Eqn::CEqnSystem_Scalar2D * get();
    }

    // vritual 関数
    virtual bool SetDomain_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool SetDomain_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world);

    //! 方程式を解く
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    
    ////////////////////////////////
    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    virtual unsigned int GetIdField_Value();
    
    //! 方程式を設定する
    bool SetEquation(CEqn_Scalar2D^ eqn );
    //! 方程式を取得する
    CEqn_Scalar2D^ GetEquation(unsigned int id_ea);

    //! 拡散係数の設定
    void SetAlpha(double alpha);
    //! 容量の設定
    void SetCapacity(double capa);
    //! 面積に比例するソース項の設定
    void SetSource(double source);
    /*! 
    @brief 静的問題
    @param [in] is_stat 静的問題かどうか
    */
    void SetStationary(bool is_stat);
    /*! 
    @brief 移流項の設定
    @param [in] id_field_advec 値を移流させる速度場のID
    @remark もしもid_field_advecが０または存在しないIDの場合は移流項は無視される
    */
    void SetAdvection(unsigned int id_field_advec);
    void SetAxialSymmetry(bool is_ax_sym);
    
    void SetSaveStiffMat(bool is_save);

protected:
    Fem::Eqn::CEqnSystem_Scalar2D *self;
};

// 

////////////////

/*! 
@brief 3Dのスカラー型方程式クラス
@ingroup FemEqnObj
*/
public ref class CEqn_Scalar3D : public CEqnSystem
{
public:
    CEqn_Scalar3D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Scalar3D(const CEqn_Scalar3D% rhs);
public:
    //nativeライブラリで実装されていない
    //CEqn_Scalar3D(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqn_Scalar3D(Fem::Eqn::CEqn_Scalar3D *self);
public:
    virtual ~CEqn_Scalar3D();
    !CEqn_Scalar3D();
    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() override;
    }
    property Fem::Eqn::CEqn_Scalar3D * Self
    {
        Fem::Eqn::CEqn_Scalar3D * get();
    }

    
    virtual bool SetDomain(unsigned int id_base, DelFEM4NetFem::Field::CFieldWorld^ world);
    //! 解く
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) override;

    ////////////////////////////////
    // 固定境界条件を追加&削除する
    virtual bool         AddFixField(   unsigned int id_field,      DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual unsigned int AddFixElemAry( unsigned int id_ea,         DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual unsigned int AddFixElemAry( IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) override;  // idof = -1
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) override;
    virtual void ClearFixElemAry() override;

    virtual unsigned int GetIdField_Value();

    //! 拡散係数を設定する
    void SetAlpha(double alpha);
    //! 体積に比例するソース項を設定する
    void SetSource(double source);

protected:
    Fem::Eqn::CEqn_Scalar3D *self;
};

}
}

#endif
