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
// CDrawerEdge.cpp : implementation of edge drawing class(CDrawerEdge)
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#  pragma warning ( disable : 4786 )
#endif

#include <assert.h>
#include <iostream>
#include <vector>
#include <stdio.h>
#include <memory>

#include "DelFEM4Net/drawer_field_edge.h"
//#include "DelFEM4Net/elem_ary.h"
#include "DelFEM4Net/field_world.h"
//#include "DelFEM4Net/field.h"
//#include "DelFEM4Net/drawer.h"
//#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetFem::Field::View;
using namespace DelFEM4NetFem::Field;


////////////////////////////////////////////////////////////////
// CDrawerEdge
////////////////////////////////////////////////////////////////

CDrawerEdge::CDrawerEdge() : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerEdge();
}

CDrawerEdge::CDrawerEdge(const CDrawerEdge% rhs) : CDrawerField()/*CDrawerField(rhs)*/
{
    const Fem::Field::View::CDrawerEdge& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Fem::Field::View::CDrawerEdge(rhs_instance_);
    assert(false);
}

CDrawerEdge::CDrawerEdge(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerEdge(id_field, isnt_value_disp, *(world->Self));
}

CDrawerEdge::CDrawerEdge(Fem::Field::View::CDrawerEdge *self) : CDrawerField() /*CDrawerField(self)*/
{
    this->self = self;
}

CDrawerEdge::~CDrawerEdge()
{
    this->!CDrawerEdge();
}

CDrawerEdge::!CDrawerEdge()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerEdge::DrawerSelf::get()
{
    return this->self;
}

Fem::Field::View::CDrawerEdge * CDrawerEdge::Self::get()
{
    return this->self;
}

////////////////////////////////////////////////////////////////////////////////
// 描画

DelFEM4NetCom::CBoundingBox3D^ CDrawerEdge::GetBoundingBox(array<double>^ rot)
{
    pin_ptr<double> ptr = nullptr;
    double *rot_ = NULL;

    if (rot != nullptr && rot->Length > 0)
    {
        ptr = &rot[0];
        rot_ = ptr;
    }
    else
    {
        rot_ = NULL;
    }
    
    const Com::CBoundingBox3D& ret_instance_ = this->self->GetBoundingBox(rot_);
    Com::CBoundingBox3D *ret = new Com::CBoundingBox3D(ret_instance_);
    
    DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
    return retManaged;
    
}

void CDrawerEdge::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawerEdge::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
 }

void CDrawerEdge::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawerEdge::Draw()
{
    this->self->Draw();
}

bool CDrawerEdge::Update(DelFEM4NetFem::Field::CFieldWorld^ world) 
{
    return this->self->Update(*(world->Self));
}

void CDrawerEdge::SetLineWidth(unsigned int linewidth)
{
    this->self->SetLineWidth(linewidth);
}

