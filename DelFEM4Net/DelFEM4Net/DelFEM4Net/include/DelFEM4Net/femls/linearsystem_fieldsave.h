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
@brief 保存方連立一次方程式クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_LINEAR_SYSTEM_FIELD_SAVE_H)
#define DELFEM4NET_LINEAR_SYSTEM_FIELD_SAVE_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "DelFEM4Net/field.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/ls/linearsystem.h"
#include "DelFEM4Net/femls/linearsystem_field.h"

#include "delfem/femls/linearsystem_fieldsave.h"

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

////////////////////////////////////////////////////////////////

/*! 
@brief 固定境界条件を入れていない行列を保存することによって，残差を計算できるようにしたクラス
@ingroup FemLs
*/
public ref class CLinearSystem_Save : public CLinearSystem_Field
{
public:
    CLinearSystem_Save();
    CLinearSystem_Save(bool isCreateInstance);
private:
    CLinearSystem_Save(const CLinearSystem_Save% rhs);
    CLinearSystem_Save(Fem::Ls::CLinearSystem_Save *self);
public:
    virtual ~CLinearSystem_Save();
    !CLinearSystem_Save();
    
    property Fem::Ls::CLinearSystem_Save * Self
    {
       Fem::Ls::CLinearSystem_Save * get();
    }

    //! fieldで初期化する、fieldの中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world) override;
    //! fieldで初期化する、fieldとfield-field2の中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world) override;
    //! fieldとfield2がパターンが同じだとして，ブロックが結合された一つの行列を作る
    virtual bool AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world) override;

    //! 対角行列をゲットする
    // 読み取り用
    DelFEM4NetMatVec::CMat_BlkCrs^ GetMatrix_Boundary(
        unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
        unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
        DelFEM4NetFem::Field::CFieldWorld^ world);
    // ポインタで返却
    DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ GetMatrixPtr_Boundary(
        unsigned int id_field_col, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
        unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
        DelFEM4NetFem::Field::CFieldWorld^ world);

    // 読み取り用
    DelFEM4NetMatVec::CVector_Blk^ GetForce(
        unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,
        DelFEM4NetFem::Field::CFieldWorld^ world);
    // ポインタで返却
    DelFEM4NetMatVec::CVector_Blk_Ptr^ GetForcePtr(
        unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type,
        DelFEM4NetFem::Field::CFieldWorld^ world);

    //! マージ前の初期化（基底クラスの隠蔽）
    virtual void InitializeMarge() override;
    //! マージ後の処理（基底クラスの隠蔽）
    virtual double FinalizeMarge() override;
    //! 残差を作る
    virtual double MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual bool UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt) override;

protected:
    Fem::Ls::CLinearSystem_Save *self;
    
    virtual void setBaseSelf();
};

////////////////////////////////////////////////////////////////

/*!
TODO : 質量項について書き直しの必要あり
     : デストラクタでのメモリ解法をしなければならない
*/
/*! 
@brief 固定境界条件を入れていない行列を保存することによって，残差を高速計算できるようにしたKCシステムの連立一次方程式
@ingroup FemLs
*/
public ref class CLinearSystem_SaveDiaM_Newmark : public CLinearSystem_Save
{
public:    
    CLinearSystem_SaveDiaM_Newmark();
private:
    CLinearSystem_SaveDiaM_Newmark(const CLinearSystem_SaveDiaM_Newmark% rhs);
    CLinearSystem_SaveDiaM_Newmark(Fem::Ls::CLinearSystem_SaveDiaM_Newmark *self);
public:
    virtual ~CLinearSystem_SaveDiaM_Newmark();
    !CLinearSystem_SaveDiaM_Newmark();
    
    property Fem::Ls::CLinearSystem_SaveDiaM_Newmark * Self
    {
       Fem::Ls::CLinearSystem_SaveDiaM_Newmark * get();
    }

    void SetNewmarkParameter(double gamma, double dt);
    double GetGamma();
    double GetDt();

    //! マージ前の初期化（基底クラスの隠蔽）
    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual void InitializeMarge() override;
    //! マージ後の処理（基底クラスの隠蔽）
    virtual double FinalizeMarge() override;
    virtual double MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual bool UpdateValueOfField(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt ) override;
    DelFEM4NetMatVec::CDiaMat_Blk^ GetDiaMassMatrix(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);

protected:
    Fem::Ls::CLinearSystem_SaveDiaM_Newmark *self;
    
    virtual void setBaseSelf() override;
};


////////////////////////////////////////////////////////////////

/*!
TODO : 質量項について書き直しの必要あり
*/
/*! 
@brief 固定境界条件を入れていない行列を保存することによって，残差を計算できるようにしたKMシステムの連立一次方程式クラス
@ingroup FemLs
*/
public ref class CLinearSystem_SaveDiaM_NewmarkBeta : public CLinearSystem_Save
{
public:
    CLinearSystem_SaveDiaM_NewmarkBeta();
private:
    CLinearSystem_SaveDiaM_NewmarkBeta(const CLinearSystem_SaveDiaM_NewmarkBeta% rhs);
    CLinearSystem_SaveDiaM_NewmarkBeta(Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta *self);
public:
    virtual ~CLinearSystem_SaveDiaM_NewmarkBeta();
    !CLinearSystem_SaveDiaM_NewmarkBeta();
    
    property Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta * Self
    {
       Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta * get();
    }

    void SetNewmarkParameter(double beta, double gamma, double dt);

    double GetGamma();
    double GetDt();
    double GetBeta();

    //! マージ前の初期化（基底クラスの隠蔽）
    DelFEM4NetMatVec::CDiaMat_Blk^ GetDiaMassMatrix(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);

    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world) override;
    //! fieldとfield2がパターンが同じだとして，ブロックが結合された一つの行列を作る
    virtual bool AddPattern_CombinedField(unsigned id_field, unsigned int id_field2, DelFEM4NetFem::Field::CFieldWorld^ world) override;

    virtual void InitializeMarge() override;
    //! マージ後の処理（基底クラスの隠蔽）
    virtual double FinalizeMarge() override;
    virtual double MakeResidual(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual bool UpdateValueOfField(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt ) override;
    
protected:
    Fem::Ls::CLinearSystem_SaveDiaM_NewmarkBeta *self;
    
    virtual void setBaseSelf() override;
};


/*! 
@brief 固有値計算用のクラス
@ingroup FemLs
*/
public ref class CLinearSystem_Eigen : public CLinearSystem_Field
{
public:
    CLinearSystem_Eigen();
private:
    CLinearSystem_Eigen(const CLinearSystem_Eigen% rhs);
    CLinearSystem_Eigen(Fem::Ls::CLinearSystem_Eigen *self);
public:
    virtual ~CLinearSystem_Eigen();
    !CLinearSystem_Eigen();
    
    property Fem::Ls::CLinearSystem_Eigen * Self
    {
       Fem::Ls::CLinearSystem_Eigen * get();
    }

    virtual void Clear() override;
    // fieldで初期化する、fieldの中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field,DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual void InitializeMarge() override;
    DelFEM4NetMatVec::CDiaMat_Blk^ GetDiaMassMatrix(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    bool SetVector_fromField(int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt );
    bool DecompMultMassMatrix();
    bool MultUpdateInvMassDecomp();
    bool MultVecMassDecomp(int ivec);
    void OffsetDiagonal(double lambda);

protected:
    Fem::Ls::CLinearSystem_Eigen *self;
    
    virtual void setBaseSelf();
};

}    // end namespace Ls
}    // end namespace Fem

#endif
