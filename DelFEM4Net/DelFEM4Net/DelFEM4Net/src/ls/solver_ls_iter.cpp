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

#include <iostream>
#include <math.h>

#include "DelFEM4Net/ls/solver_ls_iter.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"

using namespace DelFEM4NetLsSol;

// conv_ratio : IN/OUT num_iter: IN/OUT ls:IN/OUT(ハンドル変更なし)
bool CSolverLsIter::Solve_CG(double% conv_ratio, unsigned int% num_iter, ILinearSystem_Sol^ ls)
{
    double conv_ratio_ = conv_ratio;
    unsigned int num_iter_ = num_iter;
    
    bool ret = LsSol::Solve_CG(conv_ratio_, num_iter_, *(ls->SolSelf));
    
    conv_ratio = conv_ratio_;
    num_iter = num_iter_;
    
    return ret;
}

bool CSolverLsIter::Solve_PCG(double% conv_ratio,unsigned int% iteration, ILinearSystemPreconditioner_Sol^ lsp)
{
    double conv_ratio_ = conv_ratio;
    unsigned int iteration_ = iteration;
    
    bool ret = LsSol::Solve_PCG(conv_ratio_, iteration_, *(lsp->SolSelf));
    
    conv_ratio = conv_ratio_;
    iteration = iteration_;
    
    return ret;
}

bool CSolverLsIter::Solve_BiCGSTAB(double% conv_ratio, unsigned int% num_iter, ILinearSystem_Sol^ ls)
{
    double conv_ratio_ = conv_ratio;
    unsigned int num_iter_ = num_iter;
    
    bool ret = LsSol::Solve_BiCGSTAB(conv_ratio_, num_iter_, *(ls->SolSelf));
    
    conv_ratio = conv_ratio_;
    num_iter = num_iter_;
    
    return ret;
}

bool CSolverLsIter::Solve_PBiCGSTAB(double% conv_ratio, unsigned int% num_iter, ILinearSystemPreconditioner_Sol^ ls)
{
    double conv_ratio_ = conv_ratio;
    unsigned int num_iter_ = num_iter;
    
    bool ret = LsSol::Solve_PBiCGSTAB(conv_ratio_, num_iter_, *(ls->SolSelf));
    
    conv_ratio = conv_ratio_;
    num_iter = num_iter_;
    
    return ret;
}
