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
// drawer_field_image_based_flow_vis.cpp : 場可視化クラス(DrawerField)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
#endif

#include <assert.h>
#include <iostream>
#include <vector>
#include <memory>
#include <stdio.h>

#include "DelFEM4Net/drawer_field_image_based_flow_vis.h"
//#include "DelFEM4Net/elem_ary.h"
#include "DelFEM4Net/field.h"
#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetFem::Field::View;
using namespace DelFEM4NetFem::Field;

////////////////////////////////////////////////////////////////////////////////
// CEdgeTextureColor
////////////////////////////////////////////////////////////////////////////////
/*
CEdgeTextureColor::CEdgeTextureColor()
{
    this->self = new Fem::Field::View::CEdgeTextureColor();
}
*/
CEdgeTextureColor::CEdgeTextureColor(const CEdgeTextureColor% rhs)
{
    const Fem::Field::View::CEdgeTextureColor& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Field::View::CEdgeTextureColor(rhs_instance_);
}

CEdgeTextureColor::CEdgeTextureColor
    (unsigned int id_field_velo, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, 
     double r, double g, double b)
{
    this->self = new Fem::Field::View::CEdgeTextureColor(id_field_velo, id_ea, *(world->Self), r, g, b);
}

CEdgeTextureColor::CEdgeTextureColor(Fem::Field::View::CEdgeTextureColor *self)
{
    this->self = self;
}

CEdgeTextureColor::~CEdgeTextureColor()
{
    this->!CEdgeTextureColor();
}

CEdgeTextureColor::!CEdgeTextureColor()
{
    delete this->self;
}

Fem::Field::View::CEdgeTextureColor * CEdgeTextureColor::Self::get()
{
    return this->self;
}

bool CEdgeTextureColor::Set(unsigned int id_field_velo, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Set(id_field_velo, id_ea, *(world->Self));
}

bool CEdgeTextureColor::Update(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Update(*(world->Self));
}

void CEdgeTextureColor::Draw()
{
    this->self->Draw();
}













////////////////////////////////////////////////////////////////
// CDrawerImageBasedFlowVis
////////////////////////////////////////////////////////////////

CDrawerImageBasedFlowVis::CDrawerImageBasedFlowVis() : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerImageBasedFlowVis();
}

CDrawerImageBasedFlowVis::CDrawerImageBasedFlowVis(const CDrawerImageBasedFlowVis% rhs) : CDrawerField()/*CDrawerField(rhs)*/
{
    const Fem::Field::View::CDrawerImageBasedFlowVis& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Fem::Field::View::CDrawerImageBasedFlowVis(rhs_instance_);
    assert(false);
}

CDrawerImageBasedFlowVis::CDrawerImageBasedFlowVis(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world ) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerImageBasedFlowVis(id_field, *(world->Self));
}

CDrawerImageBasedFlowVis::CDrawerImageBasedFlowVis(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int imode) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerImageBasedFlowVis(id_field, *(world->Self), imode);
}

CDrawerImageBasedFlowVis::CDrawerImageBasedFlowVis(Fem::Field::View::CDrawerImageBasedFlowVis *self) : CDrawerField() /*CDrawerField(self)*/
{
    this->self = self;
}

CDrawerImageBasedFlowVis::~CDrawerImageBasedFlowVis()
{
    this->!CDrawerImageBasedFlowVis();
}

CDrawerImageBasedFlowVis::!CDrawerImageBasedFlowVis()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerImageBasedFlowVis::DrawerSelf::get()
{
    return this->self;
}

Fem::Field::View::CDrawerImageBasedFlowVis * CDrawerImageBasedFlowVis::Self::get()
{
    return this->self;
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerImageBasedFlowVis::GetBoundingBox(array<double>^ rot)
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

void CDrawerImageBasedFlowVis::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawerImageBasedFlowVis::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
 }

void CDrawerImageBasedFlowVis::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawerImageBasedFlowVis::Draw()
{
    this->self->Draw();
}

bool CDrawerImageBasedFlowVis::Update(DelFEM4NetFem::Field::CFieldWorld^ world) 
{
    return this->self->Update(*(world->Self));
}

void CDrawerImageBasedFlowVis::AddFlowInOutEdgeColor(unsigned int id_e, DelFEM4NetFem::Field::CFieldWorld^ world, 
        double r, double g, double b)
{
    this->self->AddFlowInOutEdgeColor(id_e, *(world->Self), r, g, b);
}

void CDrawerImageBasedFlowVis::SetColorField(unsigned int id_field_color, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map)
{
    Fem::Field::View::CColorMap *color_map_ptr_ = new Fem::Field::View::CColorMap(*(color_map->Self));
    std::auto_ptr<Fem::Field::View::CColorMap> color_map_(color_map_ptr_);
    
    this->self->SetColorField(id_field_color, *(world->Self), color_map_);
}


