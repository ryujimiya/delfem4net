/*
DelFEM4Net (C++/CLI wrapper for DelFEM)

DelFEM is:

CCopyright (C) 2009  Nobuyuki Umetani    n.umetani@gmail.com

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
@brief 複素数反復法関数群
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET__Z_SOLVER_CG_H)
#define DELFEM4NET__Z_SOLVER_CG_H

#include "DelFEM4Net/femls/zpreconditioner.h"

#include "DelFEM4Net/matvec/zmatdia_blkcrs.h"
#include "DelFEM4Net/matvec/zvector_blk.h"
//#include "DelFEM4Net/matvec/zmatprecond_blk.h"

#include "delfem/femls/zsolver_ls_iter.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


namespace DelFEM4NetFem
{
namespace Ls
{

//! @ingroup FemLs
//@{
public ref class CZSolverLsIter
{
public:
    ////////////////////////////////////////////////////////////////
    // Solve Hermetizn matrix "ls" with Conjugate Gradient Method
    
    //! CG法でエルミート行列を解く
    static bool Solve_CG(double% conv_ratio, unsigned int% num_iter, 
                         CZLinearSystem^ ls);  // conv_ratio: IN/OUT, num_iter: IN/OUT
                  
    //! 前処理付きCG法でエルミート行列を解く
    static bool Solve_PCG(double% conv_ratio, unsigned int% iteration,
                          CZLinearSystem^ ls, CZPreconditioner^ precond ); // conv_ratio: IN/OUT, iteration: IN/OUT
                    
    ////////////////////////////////////////////////////////////////
    // Solve Matrix with COCG Methods
    
    //! COCG法で行列を解く
    static bool Solve_PCOCG(double% conv_ratio, unsigned int% iteration,
                            CZLinearSystem^ ls, CZPreconditioner^ precond ); // conv_ratio: IN/OUT, iteration: IN/OUT
    
    ////////////////////////////////////////////////////////////////
    // Solve Matrix with Conjugate Gradient NR Methods
    
    //! CGNR法で行列を解く
    static bool Solve_CGNR(double% conv_ratio, unsigned int% num_iter,
                    CZLinearSystem^ ls); // conv_ratio: IN/OUT, num_iter: IN/OUT
    
    
    ////////////////////////////////////////////////////////////////
    // Solve Matrix with BiCGSTAB Methods
    
    //! BiCGSTAB法で行列を解く
    static bool Solve_BiCGSTAB(double% conv_ratio, unsigned int% iteration,
                               CZLinearSystem^ ls); // conv_ratio: IN/OUT, iteration: IN/OUT
                        
    //! 前処理付きBiCGSTAB法で行列を解く
    static bool Solve_BiCGStabP(double% conv_ratio, unsigned int% iteration,
                                CZLinearSystem^ ls, CZPreconditioner^ precond ); // conv_ratio: IN/OUT, iteration: IN/OUT
    
};
//@}

}
}




#endif


