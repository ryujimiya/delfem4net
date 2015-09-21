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
// CadObj3D.cpp : implementation of 3D cad class
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

#include "DelFEM4Net/cad_obj3d.h"
#include "DelFEM4Net/cad/cad_elem3d.h"
#include "DelFEM4Net/cad/cad_elem2d.h"


using namespace DelFEM4NetCad;
using namespace DelFEM4NetCom;


CCadObj3D::CCadObj3D()
{
    self = new Cad::CCadObj3D();
}

CCadObj3D::CCadObj3D(const CCadObj3D% rhs)
{
    const Cad::CCadObj3D& rhs_instance_ = *(rhs.self);
    this->self = new Cad::CCadObj3D(rhs_instance_);  //コピーコンストラクタを使用する
}

CCadObj3D::CCadObj3D(Cad::CCadObj3D* self)
{
    this->self = self;
}

CCadObj3D::~CCadObj3D()
{
    this->!CCadObj3D();
}

CCadObj3D::!CCadObj3D()
{
    delete this->self;
}

Cad::CCadObj3D* CCadObj3D::Self::get()
{
    return this->self;
}

void CCadObj3D::Clear()
{
    this->self->Clear();
}

CCadObj3D^ CCadObj3D::Clone()
{
    return gcnew CCadObj3D(*this);
}


bool CCadObj3D::IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype,unsigned int id)
{
    Cad::CAD_ELEM_TYPE t = (Cad::CAD_ELEM_TYPE)itype;
    return this->self->IsElemID(t, id);
}


IList<unsigned int>^ CCadObj3D::GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype)
{
    Cad::CAD_ELEM_TYPE itype_ = (Cad::CAD_ELEM_TYPE)itype;
    const std::vector<unsigned int>& vec = this->self->GetAryElemID(itype_);
    std::vector<unsigned int>::const_iterator itr;

    IList<unsigned int>^ list = gcnew List<unsigned int>();
    if (vec.size() > 0)
    {
         for (itr = vec.begin(); itr != vec.end(); itr++)
         {
             list->Add(*itr);
         }
    }
    return list;
}

unsigned int CCadObj3D::AddCuboid(double len_x, double len_y, double len_z)
{
    return this->self->AddCuboid(len_x, len_y, len_z);
}

unsigned int CCadObj3D::AddPolygon(IList<DelFEM4NetCom::CVector3D^>^ aVec, unsigned int id_l)
{
    std::vector<Com::CVector3D> aVec_;
    if (aVec->Count > 0)
    {
        for each (DelFEM4NetCom::CVector3D^ e in aVec)
        {
            aVec_.push_back(*(e->Self));
        }
    }
    return this->self->AddPolygon(aVec_, id_l);
}

unsigned int CCadObj3D::AddRectLoop(unsigned int id_l, DelFEM4NetCom::CVector2D^ p0, DelFEM4NetCom::CVector2D^ p1)
{
    return this->self->AddRectLoop(id_l, *(p0->Self), *(p1->Self));
}

void CCadObj3D::LiftLoop(unsigned int id_l, DelFEM4NetCom::CVector3D^ dir)
{
    return this->self->LiftLoop(id_l, *(dir->Self));
}

unsigned int CCadObj3D::AddPoint(DelFEM4NetCad::CAD_ELEM_TYPE type, unsigned int id_elem, DelFEM4NetCom::CVector3D^ po )
{
    Cad::CAD_ELEM_TYPE type_ = static_cast<Cad::CAD_ELEM_TYPE>(type);
    return this->self->AddPoint(type_, id_elem, *(po->Self));
}

CBRepSurface::CResConnectVertex^ CCadObj3D::ConnectVertex(unsigned int id_v1, unsigned int id_v2)
{
    const Cad::CBRepSurface::CResConnectVertex& ret_instance_ = this->self->ConnectVertex(id_v1, id_v2);
    Cad::CBRepSurface::CResConnectVertex *ret = new Cad::CBRepSurface::CResConnectVertex(ret_instance_);
    
    CBRepSurface::CResConnectVertex^ retManaged = gcnew CBRepSurface::CResConnectVertex(ret);
    
    return retManaged;
}

DelFEM4NetCom::CVector3D^ CCadObj3D::GetVertexCoord(unsigned int id_v)
{
    Com::CVector3D *ret = new Com::CVector3D();
    
    *ret = this->self->GetVertexCoord(id_v);
    
    DelFEM4NetCom::CVector3D^ retManaged = gcnew DelFEM4NetCom::CVector3D(ret) ;
    
    return retManaged;
}

bool CCadObj3D::GetIdVertex_Edge([Out] unsigned int% id_v_s, [Out] unsigned int% id_v_e, unsigned int id_e)
{
    unsigned int id_v_s_ = id_v_s;
    unsigned int id_v_e_ = id_v_e;
    
    bool ret = this->self->GetIdVertex_Edge(id_v_s_, id_v_e_, id_e);
    
    id_v_s = id_v_s_;
    id_v_e = id_v_e_;
    return ret;
}

unsigned int CCadObj3D::GetIdLoop_Edge(unsigned int id_e, bool is_left)
{
    return this->self->GetIdLoop_Edge(id_e, is_left);
}
   
CBRepSurface::CItrLoop^ CCadObj3D::GetItrLoop(unsigned int id_l)
{
    Cad::CBRepSurface::CItrLoop& itrl_instance_ = this->self->GetItrLoop(id_l);
    Cad::CBRepSurface::CItrLoop *itrl_ = new Cad::CBRepSurface::CItrLoop(itrl_instance_);

    CBRepSurface::CItrLoop^ retManaged = gcnew CBRepSurface::CItrLoop(itrl_);
    return retManaged;
}

DelFEM4NetCad::CLoop3D^ CCadObj3D::GetLoop(unsigned int id_l)
{
    const Cad::CLoop3D& ret_instance_ = this->self->GetLoop(id_l);
    Cad::CLoop3D *ret = new Cad::CLoop3D(ret_instance_);
    
    DelFEM4NetCad::CLoop3D^ retManaged = gcnew DelFEM4NetCad::CLoop3D(ret);
    
    return retManaged;
}

unsigned int CCadObj3D::AssertValid()
{
    return this->self->AssertValid();
}

