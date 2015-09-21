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
// DrawerField.cpp : implementation of the field visualization class (DrawerField)
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#  pragma warning ( disable : 4786 )
#endif

#include <assert.h>
#include <iostream>
#include <vector>
#include <stdio.h>
#include <memory>

#include "DelFEM4Net/drawer_field_face.h"
//#include "DelFEM4Net/elem_ary.h"
//#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"
//#include "DelFEM4Net/drawer.h"
//#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetFem::Field::View;
using namespace DelFEM4NetFem::Field;


////////////////////////////////////////////////////////////////
// CDrawerFace
////////////////////////////////////////////////////////////////

CDrawerFace::CDrawerFace() : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerFace();
}

CDrawerFace::CDrawerFace(const CDrawerFace% rhs) : CDrawerField()/*CDrawerField(rhs)*/
{
    const Fem::Field::View::CDrawerFace& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Fem::Field::View::CDrawerFace(rhs_instance_);
    assert(false);
}

CDrawerFace::CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerFace(id_field, isnt_value_disp, *(world->Self));
}

CDrawerFace::CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerFace(id_field, isnt_value_disp, *(world->Self), id_field_color);
}

CDrawerFace::CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color, double min, double max) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerFace(id_field, isnt_value_disp, *(world->Self), id_field_color, min, max);
}

CDrawerFace::CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color, CColorMap^ color_map) : CDrawerField()
{
    Fem::Field::View::CColorMap *color_map_ptr_ = new Fem::Field::View::CColorMap(*(color_map->Self)); //新たに生成する
    std::auto_ptr<Fem::Field::View::CColorMap> color_map_(color_map_ptr_);  // auto_ptr生成

    this->self = new Fem::Field::View::CDrawerFace(id_field, isnt_value_disp, *(world->Self), id_field_color, color_map_);
}

CDrawerFace::CDrawerFace(Fem::Field::View::CDrawerFace *self) : CDrawerField() /*CDrawerField(self)*/
{
    this->self = self;
}

CDrawerFace::~CDrawerFace()
{
    this->!CDrawerFace();
}

CDrawerFace::!CDrawerFace()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerFace::DrawerSelf::get()
{
    return this->self;
}

Fem::Field::View::CDrawerFace * CDrawerFace::Self::get()
{
    return this->self;
}

////////////////////////////////////////////////////////////////////////////////
// 描画

void CDrawerFace::Draw()
{
    this->self->Draw();
}

void CDrawerFace::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerFace::GetBoundingBox(array<double>^ rot)
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

void CDrawerFace::AddSelected(array<int>^ selec_flg)
{
    assert(selec_flg != nullptr);
    assert(selec_flg->Length > 0);

    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    this->self->AddSelected(selec_flg_);
}

void CDrawerFace::ClearSelected()
{
    this->self->ClearSelected();
}
bool CDrawerFace::Update(DelFEM4NetFem::Field::CFieldWorld^ world) 
{
    return this->self->Update(*(world->Self));
}

void CDrawerFace::SetColor(double r, double g, double b, unsigned int id_ea)
{
    this->self->SetColor(r, g, b, id_ea);
}

/*nativeのライブラリで実装されていない？
void CDrawerFace::SetColor(unsigned int id_es_v, unsigned int id_ns_v, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map)
{
    Fem::Field::View::CColorMap *ptr_ = new Fem::Field::View::CColorMap(*(color_map->Self)); // 
    std::auto_ptr<Fem::Field::View::CColorMap> color_map_(ptr_);
    this->self->SetColor(id_es_v, id_ns_v, *(world->Self), color_map_);
}
*/

void CDrawerFace::EnableNormal(bool is_lighting)
{
    this->self->EnableNormal(is_lighting);
}

void CDrawerFace::EnableUVMap(bool is_uv_map, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    this->self->EnableUVMap(is_uv_map, *(world->Self));
}

void CDrawerFace::SetTexCenter(double cent_x, double cent_y)
{
    this->self->SetTexCenter(cent_x, cent_y);
}

void CDrawerFace::GetTexCenter([Out] double% cent_x, [Out] double% cent_y)
{
    double cent_x_;
    double cent_y_;
    
    this->self->GetTexCenter(cent_x_, cent_y_);
    
    cent_x = cent_x_;
    cent_y = cent_y_;
}

void CDrawerFace::SetTexScale(double scale, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    this->self->SetTexScale(scale, *(world->Self));
}

