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
@brief 面で場を可視化するクラス(Fem::Field::View::CDrawerFaceContour)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_DRAWER_FIELD_IMAGE_BASED_FLOW_VIS_H)
#define DELFEM4NET_DRAWER_FIELD_IMAGE_BASED_FLOW_VIS_H

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/drawer_field.h"
#include "DelFEM4Net/field_world.h"

#include "delfem/drawer_field_image_based_flow_vis.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{
namespace View
{
public ref class CEdgeTextureColor
{
public:
    //CEdgeTextureColor();
    CEdgeTextureColor(const CEdgeTextureColor% rhs );
    CEdgeTextureColor(unsigned int id_field_velo, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, 
        double r, double g, double b);
    CEdgeTextureColor(Fem::Field::View::CEdgeTextureColor *self);
    virtual ~CEdgeTextureColor();
    !CEdgeTextureColor();
    property Fem::Field::View::CEdgeTextureColor * Self
    {
        Fem::Field::View::CEdgeTextureColor * get();
    }
    
    bool Set(unsigned int id_field_velo, unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world);
    bool Update(DelFEM4NetFem::Field::CFieldWorld^ world);
    void Draw();
protected:
    Fem::Field::View::CEdgeTextureColor *self;
    
};

//! 辺の描画クラス
public ref class CDrawerImageBasedFlowVis : public CDrawerField
{
public:
    CDrawerImageBasedFlowVis();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerImageBasedFlowVis(const CDrawerImageBasedFlowVis% rhs);
public:
    // imode (0:拡散なし) (1:ランダムノイズ) (2:格子) (3:ドット)
    CDrawerImageBasedFlowVis(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world); // imode = 1 
    CDrawerImageBasedFlowVis(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, unsigned int imode);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerImageBasedFlowVis(Fem::Field::View::CDrawerImageBasedFlowVis *self);
public:
    virtual ~CDrawerImageBasedFlowVis();
    !CDrawerImageBasedFlowVis();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Fem::Field::View::CDrawerImageBasedFlowVis * Self
    {
        Fem::Field::View::CDrawerImageBasedFlowVis * get();
    }

    ////////////////    
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual void AddSelected(array<int>^ selec_flag) override;
    virtual void ClearSelected() override;
    virtual void Draw() override;
    virtual bool Update(DelFEM4NetFem::Field::CFieldWorld^ world) override;
    void AddFlowInOutEdgeColor(unsigned int id_e, DelFEM4NetFem::Field::CFieldWorld^ world, 
        double r, double g, double b);
    void SetColorField(unsigned int id_field_color, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map);
public:
    property IList<CEdgeTextureColor^>^ aEdgeColor
    {
        IList<CEdgeTextureColor^>^ get()
        {
            const std::vector<Fem::Field::View::CEdgeTextureColor>& vec = this->self->aEdgeColor;
            return DelFEM4NetCom::ClrStub::InstanceVectorToList<Fem::Field::View::CEdgeTextureColor, CEdgeTextureColor>(vec);
        }
        void set(IList<CEdgeTextureColor^>^ list)
        {
            std::vector<Fem::Field::View::CEdgeTextureColor>& vec= this->self->aEdgeColor;  // 書換えの為に参照を取得
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<CEdgeTextureColor, Fem::Field::View::CEdgeTextureColor>(list, vec);  // 書換え
        }
    }

protected:
    Fem::Field::View::CDrawerImageBasedFlowVis *self;
    
};


}
}
}

#endif
