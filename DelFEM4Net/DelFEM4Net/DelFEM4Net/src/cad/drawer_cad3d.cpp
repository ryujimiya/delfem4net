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
// DrawerCad.cpp : implementation of the class CDrawer_Cad3D which visualize class CCadObj3D
// Please make it clash-less because this is just visualization
// Hence, Don't put assertion in the program 
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif
#define for if(0);else for

#if defined(_WIN32)
#include <windows.h>
#if defined(__VISUALC__)
#pragma comment (lib, "winmm.lib")     /* link with Windows MultiMedia lib */
#pragma comment (lib, "opengl32.lib")  /* link with Microsoft OpenGL lib */
#pragma comment (lib, "glu32.lib")     /* link with Microsoft OpenGL Utility lib */
#endif
#endif  /* _WIN32 */


#if defined(__APPLE__) && defined(__MACH__)
#include <OpenGL/gl.h>
#include <OpenGL/glu.h>
#else
#include <GL/gl.h>
#include <GL/glu.h>
#endif

#include <assert.h>
#include <iostream>

#include "DelFEM4Net/drawer_cad3d.h"
#include "DelFEM4Net/cad2d_interface.h"
#include "DelFEM4Net/mesher2d.h"

using namespace DelFEM4NetCad::View;
using namespace DelFEM4NetCom;


////////////////////////////////////////////////////////////////
// CDrawer_Cad3D
////////////////////////////////////////////////////////////////
/*
CDrawer_Cad3D::CDrawer_Cad3D() : CDrawer()
{
    this->self = new Cad::View::CDrawer_Cad3D();
}
*/

CDrawer_Cad3D::CDrawer_Cad3D(const CDrawer_Cad3D% rhs) : CDrawer()/*CDrawer(rhs)*/
{
    const Cad::View::CDrawer_Cad3D& rhs_instance_ = *(rhs.self);
    this->self = new Cad::View::CDrawer_Cad3D(rhs_instance_);
}

CDrawer_Cad3D::CDrawer_Cad3D(DelFEM4NetCad::CCadObj3D^ cad) : CDrawer()
{
    Cad::CCadObj3D *cad_ = cad->Self;
    this->self = new Cad::View::CDrawer_Cad3D(*cad_);
}

CDrawer_Cad3D::CDrawer_Cad3D(Cad::View::CDrawer_Cad3D *self) : CDrawer() /*CDrawer(self)*/
{
    this->self = self;
}

CDrawer_Cad3D::~CDrawer_Cad3D()
{
    this->!CDrawer_Cad3D();
}

CDrawer_Cad3D::!CDrawer_Cad3D()
{
    delete this->self;
}

Com::View::CDrawer * CDrawer_Cad3D::DrawerSelf::get()
{
    return this->self;
}

Cad::View::CDrawer_Cad3D * CDrawer_Cad3D::Self::get()
{
    return this->self;
}


/////////////////////////////////////////////////////////////////
// virtual関数

bool CDrawer_Cad3D::UpdateCAD_TopologyGeometry(DelFEM4NetCad::CCadObj3D^ cad)
{
    Cad::CCadObj3D *cad_ = cad->Self;
    return this->self->UpdateCAD_TopologyGeometry(*cad_);
}


/////////////////////////////////////////////////////////////////
// 描画

void CDrawer_Cad3D::Draw()
{
    this->self->Draw();
}

void CDrawer_Cad3D::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawer_Cad3D::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;

    this->self->AddSelected(selec_flg_);
}

void CDrawer_Cad3D::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawer_Cad3D::SetLineWidth(unsigned int linewidth)
{
    this->self->SetLineWidth(linewidth);
}

void CDrawer_Cad3D::SetPointSize(unsigned int pointsize)
{
    this->self->SetPointSize(pointsize);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawer_Cad3D::GetBoundingBox(array<double>^ rot)
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

