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
@brief CADを可視化するためのクラス(Cad::View::CDrawer_Cad2D)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_DRAWER_CAD_3D_H)
#define DELFEM4NET_DRAWER_CAD_3D_H

#include <vector>

#include "DelFEM4Net/cad_obj3d.h"
#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/vector3d.h"

#include "delfem/drawer_cad3d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////

namespace DelFEM4NetMsh
{
    ref class CBarAry;
    ref class CTriAry2D;
}

namespace DelFEM4NetCad
{
namespace View{

/*! 
@brief CadをOpenGLで可視化するクラス
@ingroup CAD
*/
public ref class CDrawer_Cad3D : public DelFEM4NetCom::View::CDrawer
{
public : 
    //CDrawer_Cad3D();
    CDrawer_Cad3D(const CDrawer_Cad3D% rhs);
    CDrawer_Cad3D(DelFEM4NetCad::CCadObj3D^ cad);
    CDrawer_Cad3D(Cad::View::CDrawer_Cad3D *self);
    virtual ~CDrawer_Cad3D();
    !CDrawer_Cad3D();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Cad::View::CDrawer_Cad3D * Self
    {
        Cad::View::CDrawer_Cad3D * get();
    }

    // virtual関数
  
    //! トポロジーと幾何を更新する
    bool UpdateCAD_TopologyGeometry(DelFEM4NetCad::CCadObj3D^ cad);

    //! 描画
    virtual void Draw() override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual void AddSelected(array<int>^ selec_flag) override;
    virtual void ClearSelected() override;
    //! 線の太さを設定
    void SetLineWidth(unsigned int linewidth);
    //! 点の大きさを設定
    void SetPointSize(unsigned int pointsize);

    //! バウンディング・ボックスを得る
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;

protected:
     Cad::View::CDrawer_Cad3D *self;

};

} // end namespace View
} // end namespace Cad

#endif
