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
// DrawerMsh.cpp : メッシュ描画クラス(CDrawerMsh2D,CDrawerMsh3D)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif
#define for if(0);else for
        
#include <assert.h>
#include <iostream>

#include "DelFEM4Net/drawer_msh.h"

using namespace DelFEM4NetMsh::View;
using namespace DelFEM4NetCom;


////////////////////////////////////////////////////////////////
// CDrawerMsh2D
////////////////////////////////////////////////////////////////
/*
CDrawerMsh2D::CDrawerMsh2D() : CDrawer()
{
    this->self = new Msh::View::CDrawerMsh2D();
}
*/

CDrawerMsh2D::CDrawerMsh2D(const CDrawerMsh2D% rhs) : CDrawer()/*CDrawer(rhs)*/
{
    const Msh::View::CDrawerMsh2D& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Msh::View::CDrawerMsh2D(rhs_instance_);
    assert(false);
}

CDrawerMsh2D::CDrawerMsh2D(DelFEM4NetMsh::CMesher2D^ msh) : CDrawer()
{
    Msh::CMesher2D *msh_ = msh->Self;
    this->self = new Msh::View::CDrawerMsh2D(*msh_);
}

CDrawerMsh2D::CDrawerMsh2D(DelFEM4NetMsh::CMesher2D^ msh, bool is_draw_face) : CDrawer()
{
    Msh::CMesher2D *msh_ = msh->Self;
    this->self = new Msh::View::CDrawerMsh2D(*msh_, is_draw_face);
}


CDrawerMsh2D::CDrawerMsh2D(Msh::View::CDrawerMsh2D *self) : CDrawer() /*CDrawer(self)*/
{
    this->self = self;
}

CDrawerMsh2D::~CDrawerMsh2D()
{
    this->!CDrawerMsh2D();
}

CDrawerMsh2D::!CDrawerMsh2D()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerMsh2D::DrawerSelf::get()
{
    return this->self;
}

Msh::View::CDrawerMsh2D * CDrawerMsh2D::Self::get()
{
    return this->self;
}


/////////////////////////////////////////////////////////////////
// 描画

void CDrawerMsh2D::Draw()
{
    this->self->Draw();
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerMsh2D::GetBoundingBox(array<double>^ rot)
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

void CDrawerMsh2D::SetLineWidth(unsigned int linewidth)
{
    this->self->SetLineWidth(linewidth);
}

void CDrawerMsh2D::UpdateCoord(DelFEM4NetMsh::CMesher2D^ msh )
{
    this->self->UpdateCoord(*(msh->Self));
}

void CDrawerMsh2D::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawerMsh2D::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    this->self->AddSelected(selec_flg_);
}

void CDrawerMsh2D::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawerMsh2D::GetMshPartID(array<int>^ selec_flg, [Out] unsigned int% msh_part_id)
{
    assert(selec_flg != nullptr);
    assert(selec_flg->Length > 0);

    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    unsigned int msh_part_id_ = msh_part_id;
    
    this->self->GetMshPartID(selec_flg_, msh_part_id_);
    
    msh_part_id = msh_part_id_;
}


////////////////////////////////////////////////////////////////
// CDrawerMsh3D
////////////////////////////////////////////////////////////////
/*
CDrawerMsh3D::CDrawerMsh3D() : CDrawer()
{
    this->self = new Msh::View::CDrawerMsh3D();
}
*/

CDrawerMsh3D::CDrawerMsh3D(const CDrawerMsh3D% rhs) : CDrawer()/*CDrawer(rhs)*/
{
    const Msh::View::CDrawerMsh3D& rhs_instance_ = *(rhs.self);
    this->self = new Msh::View::CDrawerMsh3D(rhs_instance_);
}

CDrawerMsh3D::CDrawerMsh3D(DelFEM4NetMsh::CMesh3D^ msh) : CDrawer()
{
    Msh::CMesh3D *msh_ = msh->Self;
    this->self = new Msh::View::CDrawerMsh3D(*msh_);
}

CDrawerMsh3D::CDrawerMsh3D(Msh::View::CDrawerMsh3D *self) : CDrawer() /*CDrawer(self)*/
{
    this->self = self;
}

CDrawerMsh3D::~CDrawerMsh3D()
{
    this->!CDrawerMsh3D();
}

CDrawerMsh3D::!CDrawerMsh3D()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerMsh3D::DrawerSelf::get()
{
    return this->self;
}

Msh::View::CDrawerMsh3D * CDrawerMsh3D::Self::get()
{
    return this->self;
}


/////////////////////////////////////////////////////////////////
// 描画

void CDrawerMsh3D::Draw()
{
    this->self->Draw();
}

void CDrawerMsh3D::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerMsh3D::GetBoundingBox(array<double>^ rot)
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

void CDrawerMsh3D::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    this->self->AddSelected(selec_flg_);
}

void CDrawerMsh3D::ClearSelected()
{
    this->self->ClearSelected();
}


void CDrawerMsh3D::Hide(unsigned int id_msh)
{
    this->self->Hide(id_msh);
}

void CDrawerMsh3D::SetColor(unsigned int id_msh, double r, double g, double b)
{
    this->self->SetColor(id_msh, r, g, b);
}

