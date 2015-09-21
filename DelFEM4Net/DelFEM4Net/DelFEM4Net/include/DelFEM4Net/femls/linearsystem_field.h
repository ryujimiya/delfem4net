/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

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
@brief 連立一次方程式クラス(Fem::Ls::CLinearSystem_Field)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_LINEAR_SYSTEM_FIELD_H)
#define DELFEM4NET_LINEAR_SYSTEM_FIELD_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "DelFEM4Net/field.h"
#include "DelFEM4Net/linearsystem_interface_eqnsys.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/ls/linearsystem.h"

#include "delfem/femls/linearsystem_field.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
    ref class CVector_Blk;
    ref class CZVector_Blk;
    ref class CMat_BlkCrs;
    ref class CMatDia_BlkCrs;
    ref class CDiaMat_Blk;
    ref class CBCFlag;
}

namespace DelFEM4NetFem
{
namespace Field
{
    ref class CFieldWorld;
}
namespace Ls
{

/*! 
@brief 連立一次方程式クラス
@ingroup FemLs
*/
public ref class CLinearSystem_Field : public DelFEM4NetLsSol::ILinearSystem_Sol, public DelFEM4NetFem::Eqn::ILinearSystem_Eqn
{
public:
    static void BoundaryCondition
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,  
         DelFEM4NetMatVec::CBCFlag^% bc_flag, DelFEM4NetFem::Field::CFieldWorld^ world)  // bc_flag: IN/OUT
    {
        unsigned int ioffset = 0;
        BoundaryCondition(id_field, elseg_type, bc_flag, world, ioffset);
    }
    static void BoundaryCondition
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,  
         DelFEM4NetMatVec::CBCFlag^% bc_flag, DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int ioffset);  // bc_flag: IN/OUT
  
    static void BoundaryCondition
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, unsigned int idofns, 
         DelFEM4NetMatVec::CBCFlag^ bc_flag, DelFEM4NetFem::Field::CFieldWorld^ world); // bc_flag: IN/OUT

public:
    CLinearSystem_Field();
    CLinearSystem_Field(bool isCreateInstance);
private:
    CLinearSystem_Field(const CLinearSystem_Field% rhs);
    CLinearSystem_Field(Fem::Ls::CLinearSystem_Field *self);
public:
    virtual ~CLinearSystem_Field();
    !CLinearSystem_Field();
    
    property Fem::Ls::CLinearSystem_Field * Self
    {
        Fem::Ls::CLinearSystem_Field * get();
    }
    
    property LsSol::ILinearSystem_Sol * SolSelf
    {
        virtual LsSol::ILinearSystem_Sol * get();
    }

    property  Fem::Eqn::ILinearSystem_Eqn * EqnSelf
    {
        virtual  Fem::Eqn::ILinearSystem_Eqn * get();
    }

    ////////////////
    virtual void Clear();

    ////////////////////////////////
    // function for marge element matrix

    //! Get residual vector for node location (elseg_type) in field (id_field)
    // 読み取り用
    virtual DelFEM4NetMatVec::CVector_Blk^ GetResidual(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 読み取り用
    virtual DelFEM4NetMatVec::CVector_Blk^ GetUpdate(  unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 更新用
    virtual DelFEM4NetMatVec::CVector_Blk_Ptr^ GetResidualPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 更新用
    virtual DelFEM4NetMatVec::CVector_Blk_Ptr^ GetUpdatePtr(  unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);

    //! Get square sub-matrix from diagonal part of full linear system
    // 読み取り用
    virtual DelFEM4NetMatVec::CMatDia_BlkCrs^ GetMatrix
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 更新用
    virtual DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ GetMatrixPtr
        (unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    //! Get non-square sub-matrix from off-diagonal part of full linear system
    // 読み取り用
    virtual DelFEM4NetMatVec::CMat_BlkCrs^ GetMatrix
        (unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
         unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
         DelFEM4NetFem::Field::CFieldWorld^ world);
    // 更新用
    virtual DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ GetMatrixPtr
        (unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
         unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
         DelFEM4NetFem::Field::CFieldWorld^ world);
    
    ////////////////////////////////
    // function for marge
    
    //! Initialize before marge (set zero value to residual & matrix)    
    virtual void InitializeMarge();
    //! Finalization before marge (set boundary condition, return residual square norm)
    virtual double FinalizeMarge();

    ////////////////////////////////
    // パターン初期化関数

    //! fieldで初期化する、fieldの中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    //! fieldで初期化する、fieldとfield-field2の中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world);
    //! fieldとfield2がパターンが同じだとして，ブロックが結合された一つの行列を作る
    virtual bool AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world);

    ////////////////////////////////
    // function for fixed boundary condition

    void ClearFixedBoundaryCondition();
    //! set fix boundary condition to dof (idof) in field (id_field)
    bool SetFixedBoundaryCondition_Field( unsigned int id_field, unsigned int idofns, DelFEM4NetFem::Field::CFieldWorld^ world );
    //! set fix boundary condition to all dof in field (id_field)
    bool SetFixedBoundaryCondition_Field( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world );

    ////////////////////////////////
    // function for update solution

    //! 残差を作る(LinearSystemSaveと宣言を一致させるためのダミーの関数)
    virtual double MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world);

    //! fieldの値を更新する
    virtual bool UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt );
    virtual bool UpdateValueOfField_RotCRV( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt );
    bool UpdateValueOfField_Newmark(double gamma, double dt, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, bool IsInitial );
    bool UpdateValueOfField_NewmarkBeta(double gamma, double beta, double dt, unsigned int id_field_val, Field::CFieldWorld^ world, bool IsInitial );
    bool UpdateValueOfField_BackwardEular(double dt, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, bool IsInitial );

    // 存在しないなら-1を返す
    int FindIndexArray_Seg(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world );
    unsigned int GetNLynSysSeg();

    ////////////////////////////////
    // function for linear solver
    // v=-1:residual    v=-2:update

    //! ソルバに必要な作業ベクトルの数を得る
    virtual unsigned int GetTmpVectorArySize();
    //! ソルバに必要な作業ベクトルの数を設定
    virtual bool ReSizeTmpVecSolver(unsigned int size_new);
    virtual double DOT(int iv1, int iv2); //!< ベクトルの内積 (return {v1} * {v2})
    virtual bool COPY(int iv1, int iv2); //!< ベクトルのコピー ({v2} := {v1})
    virtual bool SCAL(double alpha, int iv1); //!< ベクトルのスカラー倍 ({v1} := alpha * {v1})
    virtual bool AXPY(double alpha, int iv1, int iv2); //!< ベクトルの足し算({v2} := alpha*{v1} +　{v2})    
    virtual bool MATVEC(double alpha, int iv1, double beta, int iv2);  //!< 行列ベクトル積 ({v2} := alpha*[MATRIX]*{v1} + beta*{v2})

/* nativeライブラリでは下記メンバ変数が公開されているが、いったん廃止
public:
    LsSol::CLinearSystem m_ls;
*/
    DelFEM4NetLsSol::CLinearSystemAccesser^ GetLs()
    {
        return gcnew DelFEM4NetLsSol::CLinearSystemAccesser(this->self->m_ls);
    }
    
protected:
    Fem::Ls::CLinearSystem_Field *self;

};


}    // end namespace Ls
}    // end namespace Fem

#endif
