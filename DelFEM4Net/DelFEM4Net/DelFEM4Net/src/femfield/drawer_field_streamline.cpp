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
// DrawerField.cpp : 場可視化クラス(DrawerField)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
#endif

#include <assert.h>
#include <iostream>
#include <vector>
#include <stdio.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/drawer_field_streamline.h"
//#include "DelFEM4Net/elem_ary.h"
//#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"
//#include "DelFEM4Net/drawer.h"
//#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetFem::Field::View;
using namespace DelFEM4NetFem::Field;


////////////////////////////////////////////////////////////////
// CDrawerStreamline
////////////////////////////////////////////////////////////////
// static関数
void CDrawerStreamline::MakeStreamLine(unsigned int id_field_velo, DelFEM4NetFem::Field::CFieldWorld^ world,
                               IList< IList<double>^ >^ aStLine)
{
    std::vector<std::vector<double>> aStLine_;
    for each (IList<double>^ list  in aStLine)
    {
        std::vector<double> vec;
        /////DelFEM4NetCom::ClrStub::ListToVector<double>(list, vec); 
        // コンパイル中にコンパイラが死ぬので上記はコメントアウトしている
        // C:\Program Files (x86)\MSBuild\Microsoft.Cpp\v4.0\Platforms\Win32\Microsoft.Cpp.Win32.Targets(153,5): error MSB6006: "CL.exe" はコード 1 を伴って終了しました。
        for each(double data in list)
        {
            vec.push_back(data);
        }
        aStLine_.push_back(vec);
    }

    Fem::Field::View::MakeStreamLine(id_field_velo, *(world->Self), aStLine_);
}

/*natveライブラリで実装されていない
CDrawerStreamline::CDrawerStreamline() : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerStreamline();
}
*/

CDrawerStreamline::CDrawerStreamline(const CDrawerStreamline% rhs) : CDrawerField()/*CDrawerField(rhs)*/
{
    const Fem::Field::View::CDrawerStreamline& rhs_instance_ = *(rhs.self);
    // shallow copyになるので問題あり
    //this->self = new Fem::Field::View::CDrawerStreamline(rhs_instance_);
    assert(false);
}

CDrawerStreamline::CDrawerStreamline(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world ) : CDrawerField()
{
    this->self = new Fem::Field::View::CDrawerStreamline(id_field, *(world->Self));
}

CDrawerStreamline::CDrawerStreamline(Fem::Field::View::CDrawerStreamline *self) : CDrawerField() /*CDrawerField(self)*/
{
    this->self = self;
}

CDrawerStreamline::~CDrawerStreamline()
{
    this->!CDrawerStreamline();
}

CDrawerStreamline::!CDrawerStreamline()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerStreamline::DrawerSelf::get()
{
    return this->self;
}

Fem::Field::View::CDrawerStreamline * CDrawerStreamline::Self::get()
{
    return this->self;
}

bool CDrawerStreamline::Update(DelFEM4NetFem::Field::CFieldWorld^ world) 
{
    return this->self->Update(*(world->Self));
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerStreamline::GetBoundingBox(array<double>^ rot)
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

void CDrawerStreamline::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
 }

void CDrawerStreamline::ClearSelected()
{
    this->self->ClearSelected();
}

void CDrawerStreamline::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

void CDrawerStreamline::Draw()
{
    this->self->Draw();
}


