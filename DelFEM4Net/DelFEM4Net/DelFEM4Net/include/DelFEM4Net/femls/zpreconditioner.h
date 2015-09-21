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
@brief 複素数前処理クラスのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_ZPRECONDITIONER_H)
#define DELFEM4NET_ZPRECONDITIONER_H

#include <assert.h>

#include "DelFEM4Net/femls/zlinearsystem.h"

#include "DelFEM4Net/matvec/zmatdia_blkcrs.h"
//#include "DelFEM4Net/matvec/zmatdiafrac_blkcrs.h"
#include "DelFEM4Net/matvec/zvector_blk.h"
#include "DelFEM4Net/matvec/bcflag_blk.h"

#include "delfem/femls/zpreconditioner.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Ls
{

/*!
@brief 複素数抽象前処理クラス
@ingroup FemLs
*/
public interface class CZPreconditioner
{
public:
    property Fem::Ls::CZPreconditioner *PrecondSelf
    {
        Fem::Ls::CZPreconditioner * get();
    }
    void SetLinearSystem(CZLinearSystem^ ls);
    bool SetValue(CZLinearSystem^ ls);
    bool SolvePrecond(CZLinearSystem^ ls, unsigned int iv);
};

/*!
@brief 複素数ILU前処理クラス
@ingroup FemLs
*/
public ref class CZPreconditioner_ILU : public CZPreconditioner
{
public:
    CZPreconditioner_ILU();
private:
    CZPreconditioner_ILU(const CZPreconditioner_ILU% rhs);
public:
    CZPreconditioner_ILU(CZLinearSystem^ ls);
    CZPreconditioner_ILU(CZLinearSystem^ ls, unsigned int nlev);
private:
    CZPreconditioner_ILU(Fem::Ls::CZPreconditioner_ILU *self);
public:
    virtual ~CZPreconditioner_ILU();
    !CZPreconditioner_ILU();
    
    property Fem::Ls::CZPreconditioner_ILU * Self
    {
       Fem::Ls::CZPreconditioner_ILU * get();
    }

    property Fem::Ls::CZPreconditioner * PrecondSelf
    {
       virtual Fem::Ls::CZPreconditioner * get();
    }

    void Clear();
    void SetFillInLevel(unsigned int nlev);
    
    // ILU(0)のパターン初期化
    virtual void SetLinearSystem(CZLinearSystem^ ls);

    // 値を設定してILU分解を行う関数
    // ILU分解が成功しているかどうかはもっと詳細なデータを返したい
    virtual bool SetValue(CZLinearSystem^ ls);

    // Solve Preconditioning System
    virtual bool SolvePrecond(CZLinearSystem^ ls, unsigned int iv);  // ls:IN/OUT(ハンドル変更なし)

protected:
    Fem::Ls::CZPreconditioner_ILU *self;
};

}    // end namespace Ls
}    // end namespace Fem


#endif
