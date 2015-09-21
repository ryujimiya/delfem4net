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

#define for if(0);else for

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#include <assert.h>
#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/camera.h"
//#include "DelFEM4Net/uglyfont.h"
//#include "DelFEM4Net/quaternion.h"
#include "DelFEM4Net/drawer_gl_utility.h"

using namespace DelFEM4NetCom::View;

void DelFEM4NetCom::View::DrawerGlUtility::SetProjectionTransform(CCamera^ mvp_trans)
{
    Com::View::CCamera *mvp_trans_ = mvp_trans->Self;

    Com::View::SetProjectionTransform(*mvp_trans_);
}

void DelFEM4NetCom::View::DrawerGlUtility::SetModelViewTransform(CCamera^ mvp_trans)
{
    Com::View::CCamera *mvp_trans_ = mvp_trans->Self;

    Com::View::SetModelViewTransform(*mvp_trans_);
}


/* managedな配列select_bufferのunmanagedポインタをPickPostが呼ばれるまでメモリの再配置を防止する方法が分からない
   pin_ptr<unsigned int> ptr = &select_buffer[0]; でPickPre関数内ではピン止めできるが、関数外に抜けたら保証はない
   (glSelectBufferでOpenGLに通知したバッファがmanagedな配列のselect_bufferに同期しなくなる、かつバッファ自体無効となる)

void DelFEM4NetCom::View::DrawerGlUtility::PickPre(unsigned int size_buffer, [Out] array<unsigned int>^% select_buffer,
         unsigned int point_x, unsigned int point_y,
         unsigned int delX, unsigned int delY,
         DelFEM4NetCom::View::CCamera^ mvp_trans);

IList<SSelectedObject^>^ DelFEM4NetCom::View::DrawerGlUtility::PickPost(array<unsigned int>^ select_buffer,
                              unsigned int point_x, unsigned int point_y,
                              DelFEM4NetCom::View::CCamera^ mvp_trans);
*/

void DelFEM4NetCom::View::DrawerGlUtility::PickPre(unsigned int size_buffer, [Out]PickSelectBuffer^% pick_select_buffer, 
             unsigned int point_x, unsigned int point_y,
             unsigned int delX, unsigned int delY,
             DelFEM4NetCom::View::CCamera^ mvp_trans)   
{
    pick_select_buffer = gcnew PickSelectBuffer(size_buffer);
    
    Com::View::CCamera *mvp_trans_ = mvp_trans->Self;
    
    Com::View::PickPre(pick_select_buffer->Size, pick_select_buffer->Buffer,
                       point_x, point_y,
                       delX, delY,
                       *mvp_trans_);
}

IList<SSelectedObject^>^ DelFEM4NetCom::View::DrawerGlUtility::PickPost(PickSelectBuffer^ pick_select_buffer,
                                        unsigned int point_x, unsigned int point_y,
                                        DelFEM4NetCom::View::CCamera^ mvp_trans)
{
    Com::View::CCamera *mvp_trans_ = mvp_trans->Self;

    const std::vector<Com::View::SSelectedObject>& vec = Com::View::PickPost(pick_select_buffer->Buffer, point_x, point_y, *mvp_trans_);

    std::vector<Com::View::SSelectedObject>::const_iterator itr;
                              
    IList<SSelectedObject^>^ list = gcnew List<SSelectedObject^>();
    if (vec.size() > 0)
    {
         for (itr = vec.begin(); itr != vec.end(); itr++)
         {
             Com::View::SSelectedObject * e_ = new Com::View::SSelectedObject;
             *e_ = *itr;
             
             SSelectedObject^ e = gcnew SSelectedObject(e_);
             list->Add(e);
             
         }
    }
    
    return list;
}

bool DelFEM4NetCom::View::DrawerGlUtility::ReadPPM_SetTexture(String^ fname, 
                        [Runtime::InteropServices::Out] unsigned int% texName, 
                        [Runtime::InteropServices::Out] unsigned int% texWidth, [Runtime::InteropServices::Out] unsigned int% texHeight)
{
    std::string fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);
    
    unsigned int texName_;
    unsigned int texWidth_;
    unsigned int texHeight_;
    
    bool ret = Com::View::ReadPPM_SetTexture(fname_, texName_, texWidth_, texHeight_);
    
    texName = texName_;
    texWidth = texWidth_;
    texHeight = texHeight_;
        
    return ret;
}

bool DelFEM4NetCom::View::DrawerGlUtility::WritePPM_ScreenImage(String^ fname)
{
    std::string fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);
    
    bool ret = Com::View::WritePPM_ScreenImage(fname_);

    return ret;
}

////////////////////////////////////////////////////////////////////////////////
// CDrawerCoord
////////////////////////////////////////////////////////////////////////////////
CDrawerCoord::CDrawerCoord() : CDrawer()
{
    this->self = new Com::View::CDrawerCoord();
}

CDrawerCoord::CDrawerCoord(const CDrawerCoord% rhs) : CDrawer()
{
    const Com::View::CDrawerCoord& rhs_instance_ = *(rhs.self);
    this->self = new Com::View::CDrawerCoord(rhs_instance_);
}

CDrawerCoord::CDrawerCoord(CCamera^ trans, unsigned int win_h )
{
    Com::View::CCamera *trans_ = trans->Self;

    this->self = new Com::View::CDrawerCoord(*trans_, win_h);
}

CDrawerCoord::CDrawerCoord(Com::View::CDrawerCoord *self)
{
    this->self = self;
}

CDrawerCoord::~CDrawerCoord()
{
    this->!CDrawerCoord();
}

CDrawerCoord::!CDrawerCoord()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerCoord::DrawerSelf::get()
{
    return this->self;
}

Com::View::CDrawerCoord * CDrawerCoord::Self::get()
{
    return this->self;
}

void CDrawerCoord::Draw()
{
    this->self->Draw();
}

// virutal DrawerGlUtilitys which do nothing
void CDrawerCoord::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerCoord::GetBoundingBox(array<double>^ rot)
{
    double *rot_ = NULL;

    if (rot != nullptr && rot->Length > 0)
    {
        pin_ptr<double> ptr = &rot[0];
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

void CDrawerCoord::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
}

void CDrawerCoord::ClearSelected()
{
    this->self->ClearSelected();
}

// non-DrawerGlUtilitys
void CDrawerCoord::SetTrans(CCamera^ trans, int win_h)
{
    Com::View::CCamera *trans_ = trans->Self;
    this->self->SetTrans(*trans_, win_h);
}

void CDrawerCoord::SetIsShown(bool is_show)
{
    this->self->SetIsShown(is_show);
}

bool CDrawerCoord::GetIsShown()
{
    return this->self->GetIsShown();
}


////////////////////////////////////////////////////////////////////////////////
// CDrawerRect
////////////////////////////////////////////////////////////////////////////////


CDrawerRect::CDrawerRect() : CDrawer()
{
    this->self = new Com::View::CDrawerRect();
}

CDrawerRect::CDrawerRect(const CDrawerRect% rhs) : CDrawer()
{
    const Com::View::CDrawerRect& rhs_instance_ = *(rhs.self);
    this->self = new Com::View::CDrawerRect(rhs_instance_);
}

CDrawerRect::CDrawerRect(double x, double y) : CDrawer()
{
    this->self = new Com::View::CDrawerRect(x, y);
}

CDrawerRect::CDrawerRect(double x, double y, unsigned int imode) : CDrawer()
{
    this->self = new Com::View::CDrawerRect(x, y, imode);
}

CDrawerRect::CDrawerRect(Com::View::CDrawerRect *self)
{
    this->self = self;
}

CDrawerRect::~CDrawerRect()
{
    this->!CDrawerRect();
}

CDrawerRect::!CDrawerRect()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerRect::DrawerSelf::get() { return this->self; }

Com::View::CDrawerRect * CDrawerRect::Self::get() { return this->self; }

void CDrawerRect::Draw()
{
    this->self->Draw();
}

void CDrawerRect::SetInitialPositionMode(double x, double y, unsigned int imode)
{
    this->self->SetInitialPositionMode(x, y, imode);
}

void CDrawerRect::SetPosition(double x, double y)
{
    this->self->SetPosition(x, y);
}

void CDrawerRect::GetCenterSize([Out] double% cent_x, [Out] double% cent_y, [Out] double% size_x, [Out] double% size_y)
{
    double cent_x_ = cent_x;
    double cent_y_ = cent_y;
    double size_x_ = size_x;
    double size_y_ = size_y;
    
    this->self->GetCenterSize(cent_x_, cent_y_, size_x_, size_y_);
    
    cent_x = cent_x_;
    cent_y = cent_y_;
    size_x = size_x_;
    size_y = size_y_;
}

void CDrawerRect::GetPosition([Out] double% x0, [Out] double% y0, [Out] double% x1, [Out] double% y1)
{
    double x0_ = x0;
    double y0_ = y0;
    double x1_ = x1;
    double y1_ = y1;
    
    this->self->GetPosition(x0_, y0_, x1_, y1_);
    
    x0 = x0_;
    y0 = y0_;
    x1 = x1_;
    y1 = y1_;
}

// virutal DrawerGlUtilitys
void CDrawerRect::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerRect::GetBoundingBox(array<double>^ rot)
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

void CDrawerRect::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
}

void CDrawerRect::ClearSelected()
{
    this->self->ClearSelected();
}

////////////////////////////////////////////////////////////////////////////////
// CDrawerImageTexture
////////////////////////////////////////////////////////////////////////////////

CDrawerImageTexture::CDrawerImageTexture() : CDrawer()
{
    this->self = new Com::View::CDrawerImageTexture();
}

CDrawerImageTexture::CDrawerImageTexture(const CDrawerImageTexture% rhs) : CDrawer()
{
    const Com::View::CDrawerImageTexture& rhs_instance_ = *(rhs.self);
    this->self = new Com::View::CDrawerImageTexture(rhs_instance_);
}

CDrawerImageTexture::CDrawerImageTexture(Com::View::CDrawerImageTexture *self)
{
    this->self = self;
}

CDrawerImageTexture::~CDrawerImageTexture()
{
    this->!CDrawerImageTexture();
}

CDrawerImageTexture::!CDrawerImageTexture()
{
    delete this->self;
}

Com::View::CDrawer * CDrawerImageTexture::DrawerSelf::get() { return this->self; }

Com::View::CDrawerImageTexture * CDrawerImageTexture::Self::get() { return this->self; }

bool CDrawerImageTexture::IsTexture()
{
    return this->self->IsTexture();
}

bool CDrawerImageTexture::ReadPPM(String^ fname)
{
    std::string fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);
     
    bool ret = this->self->ReadPPM(fname_);

    return ret;
}

void CDrawerImageTexture::DeleteTexture()
{
    this->self->DeleteTexture();
}

bool CDrawerImageTexture::SetImage(unsigned int w, unsigned int h, IList<char>^ aRGB)
{
    std::vector<char> aRGB_;
    for (int i = 0; i < aRGB->Count; i++)
    {
        aRGB_.push_back(aRGB[i]);
    }
    return this->self->SetImage(w, h, aRGB_);
}

void CDrawerImageTexture::Draw()
{
    this->self->Draw();
}

// 以下のvirtual関数は実装されない
void CDrawerImageTexture::DrawSelection(unsigned int idraw)
{
    this->self->DrawSelection(idraw);
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerImageTexture::GetBoundingBox(array<double>^ rot)
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

void CDrawerImageTexture::AddSelected(array<int>^ selec_flg)
{
    pin_ptr<int> ptr = &selec_flg[0];
    int *selec_flg_ = ptr;
    
    this->self->AddSelected(selec_flg_);
}
void CDrawerImageTexture::ClearSelected()
{
    this->self->ClearSelected();
}




