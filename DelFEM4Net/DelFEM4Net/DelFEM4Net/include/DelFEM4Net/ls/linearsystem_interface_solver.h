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
@brief 連立一次方程式クラス(Sol::CLinearSystem_SolInterface, Sol::CLinearSystemPreconditioner_SolInterface, Sol::CLinearSystemPreconditioner_SolInterface)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_LINEARSYSTEM_INTERFACE_SOLVER_H)
#define DELFEM4NET_LINEARSYSTEM_INTERFACE_SOLVER_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <assert.h>

#include "delfem/ls/linearsystem_interface_solver.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetLsSol
{

/*! 
@brief interface class of linear system for solver
@ingroup LsSol
*/
public interface class ILinearSystem_Sol
{
public:
    property LsSol::ILinearSystem_Sol * SolSelf
    {
        LsSol::ILinearSystem_Sol * get();
    }

    ////////////////////////////////
    // function for linear solver
    // v=-1:residual    v=-2:update

    //! ソルバに必要な作業ベクトルの数を得る
    unsigned int GetTmpVectorArySize();
    //! ソルバに必要な作業ベクトルの数を設定
    bool ReSizeTmpVecSolver(unsigned int size_new);

    double DOT(int iv1, int iv2); //!< ベクトルの内積 (return {v1} * {v2})
    bool COPY(int iv1, int iv2); //!< ベクトルのコピー ({v2} := {v1})
    bool SCAL(double alpha, int iv1); //!< ベクトルのスカラー倍 ({v1} := alpha * {v1})
    bool AXPY(double alpha, int iv1, int iv2); //!< ベクトルの足し算({v2} := alpha*{v1} +　{v2})    
    bool MATVEC(double alpha, int iv1, double beta, int iv2); //!< 行列ベクトル積 ({v2} := alpha*[MATRIX]*{v1} + beta*{v2})
};


/*! 
@brief Interface class of linear system and preconditioner for solver
@ingroup LsSol
*/
public interface class ILinearSystemPreconditioner_Sol
{
public:
    property LsSol::ILinearSystemPreconditioner_Sol * SolSelf
    {
        LsSol::ILinearSystemPreconditioner_Sol * get();
    }

    ////////////////////////////////
    // function for linear solver
    // v=-1:residual    v=-2:update

    //! ソルバに必要な作業ベクトルの数を得る
    unsigned int GetTmpVectorArySize();
    //! ソルバに必要な作業ベクトルの数を設定
    bool ReSizeTmpVecSolver(unsigned int size_new);

    double DOT(int iv1, int iv2); //!< ベクトルの内積 (return {v1} * {v2})
    bool COPY(int iv1, int iv2); //!< ベクトルのコピー ({v2} := {v1})
    bool SCAL(double alpha, int iv1); //!< ベクトルのスカラー倍 ({v1} := alpha * {v1})
    bool AXPY(double alpha, int iv1, int iv2); //!< ベクトルの足し算({v2} := alpha*{v1} +　{v2})    
    bool MATVEC(double alpha, int iv1, double beta, int iv2); //!< 行列ベクトル積 ({v2} := alpha*[MATRIX]*{v1} + beta*{v2})

    bool SolvePrecond(int iv);
};



}

#endif

