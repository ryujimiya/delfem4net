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


/*! @file
@brief interface of node ordering class (MatVec::COrdering_Blk)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_ORDERING_BLK_H)
#define DELFEM4NET_ORDERING_BLK_H

#include <assert.h>
#include <vector>

#include "delfem/matvec/ordering_blk.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{

ref class CMatDia_BlkCrs;
ref class CVector_Blk;

/*! 
@brief node ordering class
@ingroup MatVec
*/
public ref class COrdering_Blk
{
public:
    COrdering_Blk();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    COrdering_Blk(const COrdering_Blk% rhs);
    COrdering_Blk(MatVec::COrdering_Blk *self);
public:
    virtual ~COrdering_Blk();
    !COrdering_Blk();
    
    property MatVec::COrdering_Blk * Self
    {
       MatVec::COrdering_Blk * get();
    }
    
    void SetOrdering(IList<int>^ ord);
    void MakeOrdering_RCM(CMatDia_BlkCrs^ mat);
    void MakeOrdering_RCM2(CMatDia_BlkCrs^ mat);
    void MakeOrdering_AMD(CMatDia_BlkCrs^ mat);
    unsigned int NBlk();
    int NewToOld(unsigned int iblk_new);
    int OldToNew(unsigned int iblk_old);
    void OrderingVector_NewToOld([Out] CVector_Blk^% vec_to, CVector_Blk^ vec_from);
    void OrderingVector_OldToNew([Out] CVector_Blk^% vec_to, CVector_Blk^ vec_from);

protected:
    MatVec::COrdering_Blk *self;
};

}

#endif
