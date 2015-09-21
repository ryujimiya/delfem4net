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
@brief 連立一次方程式クラス(LsSol::CLinearSystem)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_LINEAR_SYSTEM_H)
#define DELFEM4NET_LINEAR_SYSTEM_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/indexed_array.h"

#include "delfem/ls/linearsystem.h"

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

    ref class CVector_Blk_Ptr;
    ref class CMatDia_BlkCrs_Ptr;
    ref class CMat_BlkCrs_Ptr;
    ref class CBCFlag_Ptr;
}

namespace DelFEM4NetLsSol
{

/*! 
@brief 連立一次方程式クラス
@ingroup LsSol
*/
public ref class CLinearSystem : public DelFEM4NetLsSol::ILinearSystem_Sol
{
public:
    CLinearSystem();
    CLinearSystem(bool isCreateInstance);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CLinearSystem(const CLinearSystem% rhs);
    CLinearSystem(LsSol::CLinearSystem *self);
public:
    virtual ~CLinearSystem();
    !CLinearSystem();
    
    property LsSol::CLinearSystem * Self
    {
        LsSol::CLinearSystem * get();
    }

    property LsSol::ILinearSystem_Sol * SolSelf
    {
        virtual LsSol::ILinearSystem_Sol * get();
    }

    ////////////////
    virtual void Clear();    //!< 全てのデータのクリア(行列のサイズは保持，非ゼロパターンは消去)
    void ClearFixedBoundaryCondition();

    ////////////////////////////////
    // function for marge
    //! マージ前の初期化(行列，ベクトルに０を設定する)
    virtual void InitializeMarge();
    //! マージ後の処理（境界条件を設定し，残差ノルムを返す)
    virtual double FinalizeMarge(); 

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
    virtual bool MATVEC(double alpha, int iv1, double beta, int iv2); //!< 行列ベクトル積 ({v2} := alpha*[MATRIX]*{v1} + beta*{v2})

    ////////////////////////////////
    // function for preconditioner

    int AddLinSysSeg( unsigned int nnode, unsigned int len );
    int AddLinSysSeg( unsigned int nnode, IList<unsigned int>^ aLen );
    unsigned int GetNLinSysSeg();
    unsigned int GetBlkSizeLinSeg(unsigned int ilss);

    bool IsMatrix(unsigned int ilss, unsigned int jlss);
    // 読み取り用
    DelFEM4NetMatVec::CMatDia_BlkCrs^ GetMatrix(unsigned int ilss);
    DelFEM4NetMatVec::CMat_BlkCrs^ GetMatrix(unsigned int ilss, unsigned int jlss);
    DelFEM4NetMatVec::CVector_Blk^ GetVector(int iv, unsigned int ilss);
    DelFEM4NetMatVec::CBCFlag^ GetBCFlag(unsigned int ilss);
    // 更新用
    DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ GetMatrixPtr(unsigned int ilss);
    DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ GetMatrixPtr(unsigned int ilss, unsigned int jlss);
    DelFEM4NetMatVec::CVector_Blk_Ptr^ GetVectorPtr(int iv, unsigned int ilss);
    DelFEM4NetMatVec::CBCFlag_Ptr^ GetBCFlagPtr(unsigned int ilss);

    bool AddMat_NonDia(unsigned int ils_col, unsigned int ils_row, DelFEM4NetCom::CIndexedArray^ crs );
    bool AddMat_Dia(unsigned int ils, DelFEM4NetCom::CIndexedArray^ crs );

/* natveクラスにて公開されている下記メンバ変数はいったん廃止
public:
    std::vector< std::vector< MatVec::CMat_BlkCrs* > > m_Matrix_NonDia;
    std::vector< MatVec::CMatDia_BlkCrs* > m_Matrix_Dia;
    std::vector< MatVec::CVector_Blk* > m_Residual, m_Update;
*/
    // メンバ変数のポインタを取得する関数を用意
    // 残差ベクトルをゲットする
    DelFEM4NetMatVec::CVector_Blk_Ptr^ GetResidualPtr(unsigned int ilss); // GetVectorでiv : -1 を指定した場合に相当
    // 更新ベクトルをゲットする
    DelFEM4NetMatVec::CVector_Blk_Ptr^ GetUpdatePtr(unsigned int ilss); // GetVectorでiv : -2 を指定した場合に相当
    // 対角行列をゲットする
    DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ GetMatrixPtr_DirectAccess(unsigned int ilss);
    // 非対角行列をゲットする
    DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ GetMatrixPtr_DirectAccess(unsigned int ilss, unsigned int jlss);

protected:
    LsSol::CLinearSystem *self;

};

// CLinearSystem_Field::m_ls nativeインスタンスの参照用(リファレンス必須)
public ref class CLinearSystemAccesser : public CLinearSystem
{
public:
    CLinearSystemAccesser(LsSol::CLinearSystem& nativeInstance_) : nativeInstance(nativeInstance_), CLinearSystem(false)
    {
        // 参照のポインタを基本クラスへ渡す
        CLinearSystem::self = &nativeInstance;
    }
private:
    CLinearSystemAccesser(const CLinearSystemAccesser% rhs) : nativeInstance(rhs.nativeInstance), CLinearSystem(false)
    {
    }
public:
    ~CLinearSystemAccesser()
    {
        this->!CLinearSystemAccesser();
    }
    !CLinearSystemAccesser()
    {
        // 基本クラスでdeleteされないように基本クラスのselfをクリアする
        CLinearSystem::self = NULL;
    }

private:
    // 参照なのでnativeのクラスでこのインスタンスそのものを公開している必要がある
    LsSol::CLinearSystem& nativeInstance;
};

}

#endif

