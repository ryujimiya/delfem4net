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


////////////////////////////////////////////////////////////////
// CadObj2D.cpp : ２次元ＣＡＤモデルクラス(CCadObj2Dm)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#define for if(0);else for

#include <iostream>
#include <set>
#include <map>
#include <vector>	
#include <cassert>	
#include <math.h>	
#include <cstring>	// strlen

#include "DelFEM4Net/cad/cad_elem2d.h"

#include "DelFEM4Net/cad_obj2d_move.h"

using namespace DelFEM4NetCad;
//using namespace DelFEM4NetCom;

////////////////////////////////////////////////////////////////////////////////
// CCadObj2D_Move
////////////////////////////////////////////////////////////////////////////////

CCadObj2D_Move::CCadObj2D_Move() : CCadObj2D((Cad::CCadObj2D *)NULL) //基本クラスではインスタンスを作成しない
{
    assert(CCadObj2D::self == NULL);
    this->self = new Cad::CCadObj2D_Move();  // 明示的には実装されていないがインスタンス化は可能

    setBaseSelf();
}

CCadObj2D_Move::CCadObj2D_Move(const CCadObj2D_Move% rhs) : CCadObj2D((Cad::CCadObj2D *)NULL)
{
    assert(CCadObj2D::self == NULL);
    this->self = new Cad::CCadObj2D_Move(*(rhs.self));  // 明示的には実装されていないがインスタンス化は可能

    setBaseSelf();
}

CCadObj2D_Move::CCadObj2D_Move(Cad::CCadObj2D_Move *self)  : CCadObj2D((Cad::CCadObj2D *)NULL)
{
    assert(CCadObj2D::self == NULL);
    this->self = self;
    setBaseSelf();
}

CCadObj2D_Move::~CCadObj2D_Move()
{
    this->!CCadObj2D_Move();
}

CCadObj2D_Move::!CCadObj2D_Move()
{
    delete this->self;

    this->self = NULL;
    setBaseSelf();
}

Cad::CCadObj2D_Move * CCadObj2D_Move::Self::get()
{
    return this->self;
}

void CCadObj2D_Move::setBaseSelf()
{
    CCadObj2D::self = this->self;
}

bool CCadObj2D_Move::MoveVertex( unsigned int id_v, DelFEM4NetCom::CVector2D^ vec)
{
    return this->self->MoveVertex(id_v, *(vec->Self));
}

bool CCadObj2D_Move::MoveVertex( IList< DelFEM4NetCom::Pair<unsigned int, DelFEM4NetCom::CVector2D^>^ >^ vec)
{
    std::vector< std::pair<unsigned int, Com::CVector2D> > vec_;
    
    if (vec->Count > 0)
    {
        for each (DelFEM4NetCom::Pair<unsigned int, DelFEM4NetCom::CVector2D^>^ pair in vec)
        {
            std::pair<unsigned int, Com::CVector2D> pair_(pair->First, *(pair->Second->Self));
            vec_.push_back(pair_);
        }
    }
    
    return this->self->MoveVertex(vec_);
}

bool CCadObj2D_Move::MoveEdge(unsigned int id_e, DelFEM4NetCom::CVector2D^ vec_delta)
{
    return this->self->MoveEdge(id_e, *(vec_delta->Self));
}

bool CCadObj2D_Move::MoveLoop(unsigned int id_l, DelFEM4NetCom::CVector2D^ vec_delta)
{
    return this->self->MoveLoop(id_l, *(vec_delta->Self));
}

bool CCadObj2D_Move::DragArc(unsigned int id_e, DelFEM4NetCom::CVector2D^ dist)
{
    return this->self->DragArc(id_e, *(dist->Self));
}

bool CCadObj2D_Move::DragPolyline(unsigned int id_e, DelFEM4NetCom::CVector2D^ dist)
{
    return this->self->DragPolyline(id_e, *(dist->Self));
}

bool CCadObj2D_Move::PreCompDragPolyline(unsigned int id_e, DelFEM4NetCom::CVector2D^ pick_pos)
{
    return this->self->PreCompDragPolyline(id_e, *(pick_pos->Self));
}

bool CCadObj2D_Move::SmoothingPolylineEdge(unsigned int id_e, unsigned int niter,
                           DelFEM4NetCom::CVector2D^ pos, double radius)
{
    return this->self->SmoothingPolylineEdge(id_e, niter, *(pos->Self), radius);
}

