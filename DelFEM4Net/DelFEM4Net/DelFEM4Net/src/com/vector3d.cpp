/*
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

////////////////////////////////////////////////////////////////
// Vector3D.cpp: CVector3D クラスのインプリメンテーション
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#endif

#include <cassert>
#include <math.h>
#include <iostream>
#include <stack>

#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetCom;

////////////////////////////////////////////////////////////////////
// メンバ関数のフレンド関数
////////////////////////////////////////////////////////////////////

namespace DelFEM4NetCom{
/*
bool operator == (const CVector3D% lhs, const CVector3D% rhs)
{
    return Com::operator == (*(lhs.Self), *(rhs.Self));
}

bool operator != (const CVector3D% lhs, const CVector3D% rhs)
{
    return Com::operator != (*(lhs.Self), *(rhs.Self));
}
*/
}

//////////////////////////////////////////////////////////////////////
//    メンバ関数の非フレンド関数
//////////////////////////////////////////////////////////////////////

namespace DelFEM4NetCom{

////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////

COctTree::COctTree()
{
    this->self = new Com::COctTree();
}

COctTree::COctTree(const COctTree% rhs)
{
    const Com::COctTree& rhs_instance_ = *(rhs.self);
    
    this->self = new Com::COctTree(rhs_instance_);
}

COctTree::COctTree(Com::COctTree *self)
{
    this->self = self;
}

COctTree::~COctTree()
{
    this->!COctTree();
}

COctTree::!COctTree()
{
    delete this->self;
}

Com::COctTree * COctTree::Self::get()
{
    return this->self;
}

void COctTree::SetBoundingBox(CBoundingBox3D^ bb )
{
    Com::CBoundingBox3D *bb_ = bb->Self;
    
    this->self->SetBoundingBox(*bb_);
}

bool COctTree::Check()
{
    return this->self->Check();
}

void COctTree::GetAllPointInCell(unsigned int icell_in, [Runtime::InteropServices::Out] IList<unsigned int>^% aIndexVec ) // OUT:aIndexVec
{

    std::vector<unsigned int> aIndexVec_;
    std::vector<unsigned int>::iterator itr;
    
    this->self->GetAllPointInCell(icell_in, aIndexVec_);
    
    List<unsigned int>^ list = gcnew List<unsigned int>();
    if (aIndexVec_.size() > 0)
    {
        for (itr = aIndexVec_.begin(); itr != aIndexVec_.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    
    aIndexVec = list;
}

int COctTree::GetIndexCell_IncludePoint( CVector3D^ VecIns ) 
{
    Com::CVector3D *VecIns_ = VecIns->Self;
    
    return this->self->GetIndexCell_IncludePoint(*VecIns_);
}

bool COctTree::IsPointInSphere( double radius, CVector3D^ vec )
{
    Com::CVector3D *vec_ = vec->Self;
    
    return this->self->IsPointInSphere(radius, *vec_);
}


void COctTree::GetBoundaryOfCell(unsigned int icell_in, CBoundingBox3D^% bb )
{
    Com::CBoundingBox3D *bb_ = new Com::CBoundingBox3D();
    
    this->self->GetBoundaryOfCell(icell_in, *bb_);
    
    bb = gcnew CBoundingBox3D(bb_);
}


//  -1：成功
//  -2：範囲外
// 0～：ダブってる点の番号
int COctTree::InsertPoint( unsigned int ipo_ins, CVector3D^ VecIns )
{
    Com::CVector3D *VecIns_ = VecIns->Self;
    
    return this->self->InsertPoint(ipo_ins, *VecIns_);
}


}