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
// DrawerCad.cpp : CADモデル描画クラス(CDrawerCad)の実装
// このクラスは何が起きても絶対落ちないように実装すること．
// assertionも原則しないこと
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif
#define for if(0);else for

#include <assert.h>
#include <iostream>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/drawer_cad.h"
#include "DelFEM4Net/cad2d_interface.h"

//#include "DelFEM4Net/mesher2d.h"

using namespace DelFEM4NetCad::View;
using namespace DelFEM4NetCom;


////////////////////////////////////////////////////////////////
// CDrawer_Cad2D
////////////////////////////////////////////////////////////////

CDrawer_Cad2D::CDrawer_Cad2D() : CDrawer()
{
    this->self = new Cad::View::CDrawer_Cad2D();
}

CDrawer_Cad2D::CDrawer_Cad2D(const CDrawer_Cad2D% rhs) : CDrawer()/*CDrawer(rhs)*/
{
    const Cad::View::CDrawer_Cad2D& rhs_instance_ = *(rhs.self);
    this->self = new Cad::View::CDrawer_Cad2D(rhs_instance_);
}

CDrawer_Cad2D::CDrawer_Cad2D(DelFEM4NetCad::CCadObj2D^ cad) : CDrawer()
{
    Cad::CCadObj2D *cad_ = cad->Self;
    this->self = new Cad::View::CDrawer_Cad2D(*cad_);
}

CDrawer_Cad2D::CDrawer_Cad2D(Cad::View::CDrawer_Cad2D *self) : CDrawer() /*CDrawer(self)*/
{
    this->self = self;
}

CDrawer_Cad2D::~CDrawer_Cad2D()
{
    this->!CDrawer_Cad2D();
}

CDrawer_Cad2D::!CDrawer_Cad2D()
{
    delete this->self;
}

Com::View::CDrawer * CDrawer_Cad2D::DrawerSelf::get()
{
    return this->self;
}

Cad::View::CDrawer_Cad2D * CDrawer_Cad2D::Self::get()
{
    return this->self;
}


/////////////////////////////////////////////////////////////////
// virtual関数

void CDrawer_Cad2D::UpdateCAD_Geometry(DelFEM4NetCad::CCadObj2D^ cad)
{
    Cad::CCadObj2D *cad_ = cad->Self;
    this->self->UpdateCAD_Geometry(*cad_);
}

bool CDrawer_Cad2D::UpdateCAD_TopologyGeometry(DelFEM4NetCad::CCadObj2D^ cad)
{
    Cad::CCadObj2D *cad_ = cad->Self;
    return this->self->UpdateCAD_TopologyGeometry(*cad_);
}

/////////////////////////////////////////////////////////////////
// 描画

void CDrawer_Cad2D::Draw()
{
    this->self->Draw();
}

void CDrawer_Cad2D::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawer_Cad2D::SetLineWidth(unsigned int linewidth)
{
    this->self->SetLineWidth(linewidth);
}

void CDrawer_Cad2D::SetPointSize(unsigned int pointsize)
{
    this->self->SetPointSize(pointsize);
}

void CDrawer_Cad2D::SetTextureScale(double tex_scale)
{
    this->self->SetTextureScale(tex_scale);
}

void CDrawer_Cad2D::SetTexCenter(double cent_x, double cent_y)
{
    this->self->SetTexCenter(cent_x, cent_y);
}

void CDrawer_Cad2D::GetTexCenter(double% cent_x, double% cent_y)
{
    double cent_x_ = cent_x;
    double cent_y_ = cent_y;
    
    this->self->GetTexCenter(cent_x_, cent_y_);
    
    cent_x = cent_x_;
    cent_y = cent_y_;
}

DelFEM4NetCom::CBoundingBox3D^ CDrawer_Cad2D::GetBoundingBox(array<double>^ rot)
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
        rot_ = NULL;  // rotに0を指定したときの処理があるようなので、エラーにしないでunmanagedインスタンスへ処理を渡す
    }
    
    const Com::CBoundingBox3D& ret_instance_ = this->self->GetBoundingBox(rot_);
    Com::CBoundingBox3D *ret = new Com::CBoundingBox3D(ret_instance_);
    
    DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
    
    return retManaged;
    
}

void CDrawer_Cad2D::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    this->self->AddSelected(selec_flg_); 
}

void CDrawer_Cad2D::AddSelected(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id)
{
    Cad::CAD_ELEM_TYPE itype_ = static_cast<Cad::CAD_ELEM_TYPE>(itype);
    
    this->self->AddSelected(itype_, id);
}

void CDrawer_Cad2D::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawer_Cad2D::GetCadPartID(array<int>^ selec_flg, DelFEM4NetCad::CAD_ELEM_TYPE% part_type, unsigned int% part_id)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    Cad::CAD_ELEM_TYPE part_type_ = static_cast<Cad::CAD_ELEM_TYPE>(part_type);
    unsigned int part_id_ = part_id;
    
    this->self->GetCadPartID(selec_flg_, part_type_, part_id_); // [OUT]part_type_, part_id
    
    part_type = static_cast<DelFEM4NetCad::CAD_ELEM_TYPE>(part_type_);
    part_id = part_id_;
}

void CDrawer_Cad2D::HideEffected(DelFEM4NetCad::CCadObj2D^ cad_2d, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id)
{
    Cad::CCadObj2D *cad_2d_ = cad_2d->Self;
    Cad::CAD_ELEM_TYPE part_type_ = static_cast<Cad::CAD_ELEM_TYPE>(part_type);
    
    this->self->HideEffected(*cad_2d_, part_type_, part_id);
}

void CDrawer_Cad2D::ShowEffected(DelFEM4NetCad::CCadObj2D^ cad_2d, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id)
{
    Cad::CCadObj2D *cad_2d_ = cad_2d->Self;
    Cad::CAD_ELEM_TYPE part_type_ = static_cast<Cad::CAD_ELEM_TYPE>(part_type);
    
    this->self->ShowEffected(*cad_2d_, part_type_, part_id);
}

void CDrawer_Cad2D::SetIsShow(bool is_show, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id)
{
    Cad::CAD_ELEM_TYPE part_type_ = static_cast<Cad::CAD_ELEM_TYPE>(part_type);
    
    this->self->SetIsShow(is_show, part_type_, part_id);
}

void CDrawer_Cad2D::SetIsShow(bool is_show, DelFEM4NetCad::CAD_ELEM_TYPE part_type, IList<unsigned int>^ aIdPart)
{
    Cad::CAD_ELEM_TYPE part_type_ = static_cast<Cad::CAD_ELEM_TYPE>(part_type);
    std::vector<unsigned int>  aIdPart_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdPart, aIdPart_);
    
    this->self->SetIsShow(is_show, part_type_, aIdPart_);
}

void CDrawer_Cad2D::SetRigidDisp(unsigned int id_l, double xdisp, double ydisp)
{
    this->self->SetRigidDisp(id_l, xdisp, ydisp);
}

void CDrawer_Cad2D::EnableUVMap(bool is_uv_map)
{
    this->self->EnableUVMap(is_uv_map);
}

////////////////////////////////////////////////////////////////
// CDrawerRubberBand
////////////////////////////////////////////////////////////////
/*
CDrawerRubberBand::CDrawerRubberBand() :  CDrawer()
{
    this->self = new Cad::View::CDrawerRubberBand(); // 既定のコンストラクターがありません。
}
*/

CDrawerRubberBand::CDrawerRubberBand(const CDrawerRubberBand% rhs) : CDrawer()
{
    const Cad::View::CDrawerRubberBand& rhs_instance_ = *(rhs.self);
    this->self = new Cad::View::CDrawerRubberBand(rhs_instance_);
}

CDrawerRubberBand::CDrawerRubberBand(DelFEM4NetCom::CVector3D^ initial_position) : CDrawer()
{
    Com::CVector3D *initial_position_ = initial_position->Self;
    this->self = new Cad::View::CDrawerRubberBand(*initial_position_);
}

CDrawerRubberBand::CDrawerRubberBand(DelFEM4NetCad::CCadObj2D^ cad, unsigned int id_v_cad) : CDrawer()
{
    Cad::CCadObj2D *cad_ = cad->Self;
    this->self = new Cad::View::CDrawerRubberBand(*cad_, id_v_cad);
}

CDrawerRubberBand::CDrawerRubberBand(DelFEM4NetCad::CCadObj2D^ cad, unsigned int id_v, DelFEM4NetCom::CVector3D^ initial ): CDrawer()
{
    Cad::CCadObj2D *cad_ = cad->Self;
    Com::CVector3D *initial_ = initial->Self;
    this->self = new Cad::View::CDrawerRubberBand(*cad_, id_v, *initial_);
}

CDrawerRubberBand::CDrawerRubberBand(Cad::View::CDrawerRubberBand *self) : CDrawer()
{
    this->self = self;
}

CDrawerRubberBand::~CDrawerRubberBand()
{
    this->!CDrawerRubberBand();
}

CDrawerRubberBand::!CDrawerRubberBand()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerRubberBand::DrawerSelf::get()
{
    return this->self;
}

Cad::View::CDrawerRubberBand * CDrawerRubberBand::Self::get()
{
    return this->self;
}
    
void CDrawerRubberBand::Draw()
{
    this->self->Draw();
}

void CDrawerRubberBand::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerRubberBand::GetBoundingBox(array<double>^ rot)
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
        rot_ = NULL;  // rotに0を指定したときの処理があるようなので、エラーにしないでunmanagedインスタンスへ処理を渡す
    }
    
    const Com::CBoundingBox3D& ret_instance_ = this->self->GetBoundingBox(rot_);
    Com::CBoundingBox3D *ret = new Com::CBoundingBox3D(ret_instance_);
    
    DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
    
    return retManaged;
}

void CDrawerRubberBand::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
}

void CDrawerRubberBand::ClearSelected()
{
    this->self->ClearSelected();
}

unsigned int CDrawerRubberBand::WhatKindOfYou()
{
    return this->self->WhatKindOfYou();
}

void CDrawerRubberBand::SetMousePosition(DelFEM4NetCom::CVector3D^ mouse)
{
    Com::CVector3D *mouse_ = mouse->Self;
    
    this->self->SetMousePosition(*mouse_);
}


