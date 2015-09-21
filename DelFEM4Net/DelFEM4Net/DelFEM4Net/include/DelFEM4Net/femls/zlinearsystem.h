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
@brief 複素数有限要素法連立一次方程式クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_ZLINEAR_SYSTEM_H)
#define DELFEM4NET_ZLINEAR_SYSTEM_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "DelFEM4Net/field.h"

#include "delfem/femls/zlinearsystem.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
    ref class CZVector_Blk;
    ref class CZMatDia_BlkCrs;
    ref class CDiaMat_Blk;
    ref class CZMat_BlkCrs;
    ref class CBCFlag;

    ref class CZVector_Blk_Ptr;
    ref class CZMatDia_BlkCrs_Ptr;
    ref class CZMat_BlkCrs_Ptr;

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
@brief 複素数有限要素法連立一次方程式クラス
@ingroup FemLs
*/
public ref class CZLinearSystem
{
public:
    CZLinearSystem();
    CZLinearSystem(bool isCreateInstance);
private:
    CZLinearSystem(const CZLinearSystem% rhs);
    CZLinearSystem(Fem::Ls::CZLinearSystem *self);
public:
    virtual ~CZLinearSystem();
    !CZLinearSystem();
    
    property Fem::Ls::CZLinearSystem * Self
    {
        Fem::Ls::CZLinearSystem * get();
    }

    ////////////////
    virtual void Clear();

    // fieldで初期化する、fieldの中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
/* nativeライブラリで実装されていない
    bool SetVector_fromField
        (unsigned int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt);
*/
    bool NormalizeVector(int iv);

    ////////////////////////////////
    // function for marge element matrix

    // 残差ベクトルをゲットする
    DelFEM4NetMatVec::CZVector_Blk_Ptr^ GetResidualPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 更新ベクトルをゲットする
    DelFEM4NetMatVec::CZVector_Blk_Ptr^ GetUpdatePtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 対角行列をゲットする
    DelFEM4NetMatVec::CZMatDia_BlkCrs_Ptr^ GetMatrixPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    // 非対角行列をゲットする
    DelFEM4NetMatVec::CZMat_BlkCrs_Ptr^ GetMatrixPtr(unsigned int id_field_col,DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_col,
        unsigned int id_field_row, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type_row,
        DelFEM4NetFem::Field::CFieldWorld^ world);
    

    ////////////////////////////////
    // function for marge

    // マージに必要なバッファサイズを得る
    unsigned int GetTmpBufferSize();
    // マージ前の初期化
    virtual void InitializeMarge();
    // マージ後の処理（残差ノルムを返す)
    virtual double FinalizeMarge(); 

    ////////////////////////////////
    // function for fixed boundary condition

    // 固定境界条件を全解除
    void ClearFixedBoundaryCondition();
    // 固定境界条件の設定
    // idof:固定する自由度
    bool SetFixedBoundaryCondition_Field( unsigned int id_field, unsigned int idofns, DelFEM4NetFem::Field::CFieldWorld^ world );
    // 固定境界条件の設定---field中の全自由度固定
    bool SetFixedBoundaryCondition_Field( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world );

    ////////////////////////////////
    // function for update solution

    // 残差を作る
    virtual double MakeResidual(unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world);
    virtual bool UpdateValueOfField( unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt );

    ////////////////////////////////
    // function for linear solver
    // v=-1:residual    v=-2:update

    // ソルバに必要な作業ベクトルの数を得る
    unsigned int GetTmpVectorArySize() ;
    // ソルバに必要な作業ベクトルの数を設定
    bool ReSizeTmpVecSolver(unsigned int size_new);
    // ベクトルの共役を取る
    bool Conjugate(int iv1);
    // ベクトルの掛け算
    // return {v1} * {v2}
    DelFEM4NetCom::Complex^ DOT(int iv1, int iv2);
    // ベクトルの内積
    // return {v1} * {v2}^H
    DelFEM4NetCom::Complex^ INPROCT(int iv1, int iv2);
    // ベクトルのコピー
    // {v2} := {v1}
    bool COPY(int iv_from, int iv_to);
    // ベクトルのスカラー倍
    // {v1} := alpha * {v1}
    bool SCAL(DelFEM4NetCom::Complex^ alpha, int iv1);
    // ベクトルの足し算
    // {v2} := alpha*{v1} +　{v2}
    bool AXPY(DelFEM4NetCom::Complex^ alpha, int iv1, int iv2);
    // 行列ベクトル積
    // {v2} := alpha*[MATRIX]*{v1} + beta*{v2}
    bool MatVec(double alpha, int iv1, double beta, int iv2);
    // 行列のエルミートとベクトル積
    // {v2} := alpha*[MATRIX]^H*{v1} + beta*{v2}
    bool MatVec_Hermitian(double alpha, int iv1, double beta, int iv2);

    ////////////////////////////////
    // function for preconditioner
    
    unsigned int GetNLynSysSeg() ;
    // 存在しないなら-1を返す
    int FindIndexArray_Seg( unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world );

    bool IsMatrix(unsigned int ilss, unsigned int jlss);
    DelFEM4NetMatVec::CZMatDia_BlkCrs^ GetMatrix(unsigned int ilss);
    DelFEM4NetMatVec::CZMat_BlkCrs^ GetMatrix(unsigned int ilss, unsigned int jlss);
    DelFEM4NetMatVec::CZVector_Blk^ GetVector(int iv, unsigned int ilss);


protected:
    Fem::Ls::CZLinearSystem *self;


};

////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////

public ref class CZLinearSystem_GeneralEigen : public CZLinearSystem
{
public:
    CZLinearSystem_GeneralEigen();
    CZLinearSystem_GeneralEigen(bool isCreateInstance);
private:
    CZLinearSystem_GeneralEigen(const CZLinearSystem_GeneralEigen% rhs);
    CZLinearSystem_GeneralEigen(Fem::Ls::CZLinearSystem_GeneralEigen *self);
public:
    virtual ~CZLinearSystem_GeneralEigen();
    !CZLinearSystem_GeneralEigen();
    
    property Fem::Ls::CZLinearSystem_GeneralEigen * Self
    {
        Fem::Ls::CZLinearSystem_GeneralEigen * get();
    }

    virtual void Clear() override;
    // fieldで初期化する、fieldの中の非ゼロパターンを作る
    virtual bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world) override;
    virtual void InitializeMarge() override;
    DelFEM4NetMatVec::CDiaMat_Blk^ GetDiaMassMatrixPtr(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE elseg_type, DelFEM4NetFem::Field::CFieldWorld^ world);
    /*!
    @brief 場の値をベクトルに設定
    @param[in] iv 0以上なら作業ベクトル, -1なら残差ベクトル, -2なら更新ベクトル
    */
    bool SetVector_fromField(int iv, unsigned int id_field_val, DelFEM4NetFem::Field::CFieldWorld^ world, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt );
    bool DecompMultMassMatrix();
    void OffsetDiagonal(double lambda);
    bool MultUpdateInvMassDecomp();
    bool MultVecMassDecomp(int ivec);
    /* nativeライブラリで実装されていない
    void RemoveConstant(int iv);
    */
    
protected:
    Fem::Ls::CZLinearSystem_GeneralEigen *self;

    void setBaseSelf();
};


}    // Ls
}    // Fem

#endif
