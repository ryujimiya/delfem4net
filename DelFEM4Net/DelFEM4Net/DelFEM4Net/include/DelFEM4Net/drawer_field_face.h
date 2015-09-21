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


/*! @file
@brief Interfacde for field visualization class of element surface(Fem::Field::View::CDrawerFace)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_DRAWER_FIELD_FACE_H)
#define DELFEM4NET_DRAWER_FIELD_FACE_H

//#include <memory>

#include "DelFEM4Net/drawer_field.h"

#include "delfem/drawer_field_face.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{
namespace View
{

//! drawing face class
public ref class CDrawerFace : public CDrawerField
{
public:
    CDrawerFace();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerFace(const CDrawerFace% rhs);
public:
    CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world);
    CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color);
    CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color, double min, double max);
    CDrawerFace(unsigned int id_field, bool isnt_value_disp, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int id_field_color, CColorMap^ color_map);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerFace(Fem::Field::View::CDrawerFace *self);
public:
    virtual ~CDrawerFace();
    !CDrawerFace();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Fem::Field::View::CDrawerFace * Self
    {
        Fem::Field::View::CDrawerFace * get();
    }
  
    ////////////////////////////////
    // declaration of virtual functions
  
    virtual void Draw() override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flag) override;
    virtual void ClearSelected() override;
    virtual bool Update(DelFEM4NetFem::Field::CFieldWorld^ world) override;
  
    ////////////////////////////////
    // declaration of non-virtual functions
  
    void SetColor(double r, double g, double b)
    {
        unsigned int id_ea = 0;
        SetColor(r, g, b, id_ea);
    }
    void SetColor(double r, double g, double b, unsigned int id_ea);
    /*nativeのライブラリで実装されていない？
    void SetColor(unsigned int id_es_v, unsigned int id_ns_v, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map);
    */
    void EnableNormal(bool is_lighting);
    void EnableUVMap(bool is_uv_map, DelFEM4NetFem::Field::CFieldWorld^ world);
    void SetTexCenter(double cent_x, double cent_y);
    void GetTexCenter([Out] double% cent_x, [Out] double% cent_y);
    void SetTexScale(double scale, DelFEM4NetFem::Field::CFieldWorld^ world);
    
protected:
    Fem::Field::View::CDrawerFace *self;


};

}    // View
}    // Field
}    // Fem


#endif
