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

#include "DelFEM4Net/femls/zpreconditioner.h"

using namespace DelFEM4NetFem::Ls;

////////////////////////////////////////////////////////////////////////////////
// CZPreconditioner_ILU
////////////////////////////////////////////////////////////////////////////////

CZPreconditioner_ILU::CZPreconditioner_ILU()
{
    this->self = new Fem::Ls::CZPreconditioner_ILU();
}

CZPreconditioner_ILU::CZPreconditioner_ILU(const CZPreconditioner_ILU% rhs)
{
    const Fem::Ls::CZPreconditioner_ILU& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Ls::CZPreconditioner_ILU(rhs_instance_);
}

CZPreconditioner_ILU::CZPreconditioner_ILU(CZLinearSystem^ ls)
{
    this->self = new Fem::Ls::CZPreconditioner_ILU(*(ls->Self));
}

CZPreconditioner_ILU::CZPreconditioner_ILU(CZLinearSystem^ ls, unsigned int nlev)
{
    this->self = new Fem::Ls::CZPreconditioner_ILU(*(ls->Self), nlev);
}

CZPreconditioner_ILU::CZPreconditioner_ILU(Fem::Ls::CZPreconditioner_ILU *self)
{
    this->self = self;
}

CZPreconditioner_ILU::~CZPreconditioner_ILU()
{
    this->!CZPreconditioner_ILU();
}

CZPreconditioner_ILU::!CZPreconditioner_ILU()
{
    delete this->self;
}

Fem::Ls::CZPreconditioner_ILU * CZPreconditioner_ILU::Self::get() { return this->self; }

Fem::Ls::CZPreconditioner * CZPreconditioner_ILU::PrecondSelf::get() { return this->self; }

void CZPreconditioner_ILU::Clear()
{
    this->self->Clear();
}

void CZPreconditioner_ILU::SetFillInLevel(unsigned int nlev)
{
    this->self->SetFillInLevel(nlev);
}

void CZPreconditioner_ILU::SetLinearSystem(CZLinearSystem^ ls)
{
    this->self->SetLinearSystem(*(ls->Self));
}

bool CZPreconditioner_ILU::SetValue(CZLinearSystem^ ls)
{
    return this->self->SetValue(*(ls->Self));
}

bool CZPreconditioner_ILU::SolvePrecond(CZLinearSystem^ ls, unsigned int iv)  // ls:IN/OUT(ハンドル変更なし)
{
    return this->self->SolvePrecond(*(ls->Self), iv);
}


