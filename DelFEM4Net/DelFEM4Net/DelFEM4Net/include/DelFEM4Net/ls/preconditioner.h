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
@brief 前処理行列クラス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_PRECONDITIONER_H)
#define DELFEM4NET_PRECONDITIONER_H

#include <assert.h>
#include <time.h>

#include "DelFEM4Net/ls/linearsystem.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"

#include "DelFEM4Net/matvec/matdia_blkcrs.h"
//#include "DelFEM4Net/matvec/matdiafrac_blkcrs.h"
//#include "DelFEM4Net/matvec/matfrac_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
//#include "DelFEM4Net/matvec/solver_mg.h"
#include "DelFEM4Net/matvec/ordering_blk.h"

#include "delfem/ls/preconditioner.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetLsSol
{

/*! 
@brief 前処理行列クラスの抽象クラス
@ingroup LsSol
*/
public interface class CPreconditioner
{
public:
    property LsSol::CPreconditioner *PrecondSelf
    {
        LsSol::CPreconditioner * get();
    }
    void SetLinearSystem(CLinearSystem^ ls);
    bool SetValue(CLinearSystem^ ls);
    bool SolvePrecond(CLinearSystem^ ls, unsigned int iv);   // ls:IN/OUT(ハンドル変更なし)
};

/*! 
@brief ILUによる前処理行列クラス
@ingroup LsSol
*/
public ref class CPreconditioner_ILU : public CPreconditioner
{
public:
    CPreconditioner_ILU();
private:
    CPreconditioner_ILU(const CPreconditioner_ILU% rhs);
public:
    CPreconditioner_ILU(CLinearSystem^ ls);
    CPreconditioner_ILU(CLinearSystem^ ls, unsigned int nlev);
private:
    CPreconditioner_ILU(LsSol::CPreconditioner_ILU *self);
public:
    virtual ~CPreconditioner_ILU();
    !CPreconditioner_ILU();
    
    property LsSol::CPreconditioner_ILU * Self
    {
       LsSol::CPreconditioner_ILU * get();
    }

    property LsSol::CPreconditioner * PrecondSelf
    {
       virtual LsSol::CPreconditioner * get();
    }

    //! データを全て消去する
    void Clear();

    //! fill_inのレベル設定
     void SetFillInLevel(int lev)
     {
         int ilss0 = -1;
         SetFillInLevel(lev, ilss0);
     }
    void SetFillInLevel(int lev, int ilss0);

    // このノードには必ずFill_Inを入れる
    void SetFillBlk(IList<unsigned int>^ aBlk);

    // ILU(0)のパターン初期化
    virtual void SetLinearSystem(CLinearSystem^ ls);

    // 値を設定してILU分解を行う関数
    // ILU分解が成功しているかどうかはもっと詳細なデータを返したい
    virtual bool SetValue(CLinearSystem^ ls);

    // Solve Preconditioning System
    virtual bool SolvePrecond(CLinearSystem^ ls, unsigned int iv);  // ls:IN/OUT(ハンドル変更なし)
  
    //! Orderingの有無を設定
    void SetOrdering(IList<int>^ aind);

protected:
    LsSol::CPreconditioner_ILU *self;
};


/*! 
@brief 連立一次方程式と前処理クラスの抽象クラス
@ingroup LsSol
*/
public ref class CLinearSystemPreconditioner : public DelFEM4NetLsSol::ILinearSystemPreconditioner_Sol
{
public:
    //CLinearSystemPreconditioner();
private:
    CLinearSystemPreconditioner(const CLinearSystemPreconditioner% rhs);
public:
    CLinearSystemPreconditioner(CLinearSystem^ ls, CPreconditioner^ prec );  // ls, prec :IN/OUT(ハンドルの変更なし)
private:
    CLinearSystemPreconditioner(LsSol::CLinearSystemPreconditioner *self);
public:
    virtual ~CLinearSystemPreconditioner();
    !CLinearSystemPreconditioner();
    
    property LsSol::CLinearSystemPreconditioner * Self
    {
       LsSol::CLinearSystemPreconditioner * get();
    }

    property LsSol::ILinearSystemPreconditioner_Sol * SolSelf
    {
       virtual LsSol::ILinearSystemPreconditioner_Sol * get();
    }

    ////////////////////////////////
    // function for linear solver
    // v=-1:residual    v=-2:update

    //! ソルバに必要な作業ベクトルの数を得る
    virtual unsigned int GetTmpVectorArySize();
    //! ソルバに必要な作業ベクトルの数を設定
    virtual bool ReSizeTmpVecSolver(unsigned int size_new);

    //! ベクトルの内積 (return {v1} * {v2})
    virtual double DOT(int iv1, int iv2);
    //! ベクトルのコピー ({v2} := {v1})
    virtual bool COPY(int iv1, int iv2);
    //! ベクトルのスカラー倍 ({v1} := alpha * {v1})
    virtual bool SCAL(double alpha, int iv1);
    //! ベクトルの足し算({v2} := alpha*{v1} +　{v2})    
    virtual bool AXPY(double alpha, int iv1, int iv2);
    //! 行列ベクトル積 ({v2} := alpha*[MATRIX]*{v1} + beta*{v2})
    virtual bool MATVEC(double alpha, int iv1, double beta, int iv2);

    virtual bool SolvePrecond(int iv);

protected:
    LsSol::CLinearSystemPreconditioner *self;
    
};

}    // end namespace LsSol

#endif
