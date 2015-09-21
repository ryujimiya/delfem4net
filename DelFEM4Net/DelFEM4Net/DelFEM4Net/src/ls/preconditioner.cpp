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

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )   // C4786なんて表示すんな( ﾟДﾟ)ｺﾞﾙｧ
#endif
#define for if(0); else for

#include <assert.h>
#include <time.h>
#include <stdio.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
//#include "DelFEM4Net/matvec/matdiafrac_blkcrs.h"
//#include "DelFEM4Net/matvec/matfrac_blkcrs.h"
#include "DelFEM4Net/matvec/vector_blk.h"
//#include "DelFEM4Net/matvec/solver_mg.h"
#include "DelFEM4Net/matvec/ordering_blk.h"

#include "DelFEM4Net/ls/preconditioner.h"

using namespace DelFEM4NetLsSol;

////////////////////////////////////////////////////////////////////////////////
// CPreconditioner_ILU
////////////////////////////////////////////////////////////////////////////////

CPreconditioner_ILU::CPreconditioner_ILU()
{
    this->self = new LsSol::CPreconditioner_ILU();
}

CPreconditioner_ILU::CPreconditioner_ILU(const CPreconditioner_ILU% rhs)
{
    const LsSol::CPreconditioner_ILU& rhs_instance_ = *(rhs.self);
    this->self = new LsSol::CPreconditioner_ILU(rhs_instance_);
}

CPreconditioner_ILU::CPreconditioner_ILU(CLinearSystem^ ls)
{
    this->self = new LsSol::CPreconditioner_ILU(*(ls->Self));
}

CPreconditioner_ILU::CPreconditioner_ILU(CLinearSystem^ ls, unsigned int nlev)
{
    this->self = new LsSol::CPreconditioner_ILU(*(ls->Self), nlev);
}

CPreconditioner_ILU::CPreconditioner_ILU(LsSol::CPreconditioner_ILU *self)
{
    this->self = self;
}

CPreconditioner_ILU::~CPreconditioner_ILU()
{
    this->!CPreconditioner_ILU();
}

CPreconditioner_ILU::!CPreconditioner_ILU()
{
    delete this->self;
}

LsSol::CPreconditioner_ILU * CPreconditioner_ILU::Self::get() { return this->self; }

LsSol::CPreconditioner * CPreconditioner_ILU::PrecondSelf::get() { return this->self; }

void CPreconditioner_ILU::Clear()
{
    this->self->Clear();
}

void CPreconditioner_ILU::SetFillInLevel(int lev, int ilss0)
{
    this->self->SetFillInLevel(lev, ilss0);
}

void CPreconditioner_ILU::SetFillBlk(IList<unsigned int>^ aBlk)
{
    std::vector<unsigned int> aBlk_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aBlk, aBlk_);
    
    this->self->SetFillBlk(aBlk_);
}

void CPreconditioner_ILU::SetLinearSystem(CLinearSystem^ ls)
{
    this->self->SetLinearSystem(*(ls->Self));
}

bool CPreconditioner_ILU::SetValue(CLinearSystem^ ls)
{
    return this->self->SetValue(*(ls->Self));
}

bool CPreconditioner_ILU::SolvePrecond(CLinearSystem^ ls, unsigned int iv)  // ls:IN/OUT(ハンドル変更なし)
{
    return this->self->SolvePrecond(*(ls->Self), iv);
}

void CPreconditioner_ILU::SetOrdering(IList<int>^ aind)
{
    std::vector<int> aind_;
    DelFEM4NetCom::ClrStub::ListToVector<int>(aind, aind_);
    
    this->self->SetOrdering(aind_);
}

////////////////////////////////////////////////////////////////////////////////
// CLinearSystemPreconditioner
////////////////////////////////////////////////////////////////////////////////
/*
CLinearSystemPreconditioner::CLinearSystemPreconditioner()
{
    this->self = new LsSol::CLinearSystemPreconditioner();
}
*/

CLinearSystemPreconditioner::CLinearSystemPreconditioner(const CLinearSystemPreconditioner% rhs)
{
    const LsSol::CLinearSystemPreconditioner& rhs_instance_ = *(rhs.self);
    this->self = new LsSol::CLinearSystemPreconditioner(rhs_instance_);
}

CLinearSystemPreconditioner::CLinearSystemPreconditioner(CLinearSystem^ ls, CPreconditioner^ prec )  // ls, prec :IN/OUT(ハンドルの変更なし)
{
    this->self = new LsSol::CLinearSystemPreconditioner(*(ls->Self), *(prec->PrecondSelf));
}

CLinearSystemPreconditioner::CLinearSystemPreconditioner(LsSol::CLinearSystemPreconditioner *self)
{
    this->self = self;
}

CLinearSystemPreconditioner::~CLinearSystemPreconditioner()
{
    this->!CLinearSystemPreconditioner();
}

CLinearSystemPreconditioner::!CLinearSystemPreconditioner()
{
    delete this->self;
}

LsSol::CLinearSystemPreconditioner * CLinearSystemPreconditioner::Self::get() { return this->self; }

LsSol::ILinearSystemPreconditioner_Sol * CLinearSystemPreconditioner::SolSelf::get() { return this->self; }

unsigned int CLinearSystemPreconditioner::GetTmpVectorArySize()
{
    return this->self->GetTmpVectorArySize();
}

bool CLinearSystemPreconditioner::ReSizeTmpVecSolver(unsigned int size_new)
{
    return this->self->ReSizeTmpVecSolver(size_new);
}

double CLinearSystemPreconditioner::DOT(int iv1, int iv2)
{
    return this->self->DOT(iv1, iv2);
}

bool CLinearSystemPreconditioner::COPY(int iv1, int iv2)
{
    return this->self->COPY(iv1, iv2);
}

bool CLinearSystemPreconditioner::SCAL(double alpha, int iv1)
{
    return this->self->SCAL(alpha, iv1);
}

bool CLinearSystemPreconditioner::AXPY(double alpha, int iv1, int iv2)
{
    return this->self->AXPY(alpha, iv1, iv2);
}

bool CLinearSystemPreconditioner::MATVEC(double alpha, int iv1, double beta, int iv2)
{
    return this->self->MATVEC(alpha, iv1, beta, iv2);
}

bool CLinearSystemPreconditioner::SolvePrecond(int iv)
{
    return this->self->SolvePrecond(iv);
}

