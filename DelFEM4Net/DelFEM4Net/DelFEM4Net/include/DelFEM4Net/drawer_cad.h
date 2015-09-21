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

#if !defined(DELFEM4NET_DRAWER_CAD_H)
#define DELFEM4NET_DRAWER_CAD_H

#include <vector>

#include "DelFEM4Net/cad_obj2d.h"
#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/vector3d.h"

#include "delfem/drawer_cad.h"

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
namespace View
{

/*! 
@brief CadをOpenGLで可視化するクラス
@ingroup CAD
*/
public ref class CDrawer_Cad2D : public DelFEM4NetCom::View::CDrawer
{
public : 
    /*!
     @brief コンストラクタ
     @param[in] cad ＣＡＤの形状
     @param[in] imode 表示モード{0:面を描画する,1:面を描画しない(セレクションでは面を描画)}(デフォルトで0)
     @remaks 面を描画しないモードに設定してもセレクションはされるので，ピックは可能
    */
    CDrawer_Cad2D();
    CDrawer_Cad2D(const CDrawer_Cad2D% rhs);
    CDrawer_Cad2D(DelFEM4NetCad::CCadObj2D^ cad);
    CDrawer_Cad2D(Cad::View::CDrawer_Cad2D *self);
    virtual ~CDrawer_Cad2D();
    !CDrawer_Cad2D();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Cad::View::CDrawer_Cad2D * Self
    {
        Cad::View::CDrawer_Cad2D * get();
    }
    
    // virtual関数

    //! 幾何形状を更新する(トポロジーの変化は存在しないとして)
    void UpdateCAD_Geometry(DelFEM4NetCad::CCadObj2D^ cad);
  
    //! トポロジーと幾何を更新する
    bool UpdateCAD_TopologyGeometry(DelFEM4NetCad::CCadObj2D^ cad);

    //! @{
    //! 描画
    virtual void Draw() override;
    //! ピッキングのための描画をする
    virtual void DrawSelection(unsigned int idraw) override;
    //! 線の太さを設定
    void SetLineWidth(unsigned int linewidth);
    //! 点の大きさを設定
    void SetPointSize(unsigned int pointsize);
    void SetTextureScale(double tex_scale);
    void SetTexCenter(double cent_x, double cent_y);
    void GetTexCenter(double% cent_x, double% cent_y);
    
    //! バウンディング・ボックスを得る
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    //! Hilight表示する要素に加える(selection_flagから)
    virtual void AddSelected(array<int>^ selec_flag) override;
    //! Hilight表示する要素に加える
    virtual void AddSelected(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id);
    //! Hilight表示をやめる
    virtual void ClearSelected() override;
    //! @}

    void GetCadPartID(array<int>^ selec_flag, DelFEM4NetCad::CAD_ELEM_TYPE% part_type, unsigned int% part_id); //[OUT]part_type, part_id
    
    void HideEffected(DelFEM4NetCad::CCadObj2D^ cad_2d, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id);
    void ShowEffected(DelFEM4NetCad::CCadObj2D^ cad_2d, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id);
    void SetIsShow(bool is_show, DelFEM4NetCad::CAD_ELEM_TYPE part_type, unsigned int part_id);
    void SetIsShow(bool is_show, DelFEM4NetCad::CAD_ELEM_TYPE part_type, IList<unsigned int>^ aIdPart);
    void SetRigidDisp(unsigned int id_l, double xdisp, double ydisp);
    void EnableUVMap(bool is_uv_map);

protected:
    Cad::View::CDrawer_Cad2D *self;
};


////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////

/*!
@brief ラバーバンドクラス
@remark ここはCVector3Dじゃなくて，CVector2Dを使うべき．
*/
public ref class CDrawerRubberBand : public DelFEM4NetCom::View::CDrawer
{
public:
    //CDrawerRubberBand();
    CDrawerRubberBand(const CDrawerRubberBand% rhs);
    CDrawerRubberBand(DelFEM4NetCom::CVector3D^ initial_position);
    CDrawerRubberBand(DelFEM4NetCad::CCadObj2D^ cad, unsigned int id_v_cad);
    CDrawerRubberBand(DelFEM4NetCad::CCadObj2D^ cad, unsigned int id_v, DelFEM4NetCom::CVector3D^ initial);
    CDrawerRubberBand(Cad::View::CDrawerRubberBand *self);
    virtual ~CDrawerRubberBand();
    !CDrawerRubberBand();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Cad::View::CDrawerRubberBand * Self
    {
        Cad::View::CDrawerRubberBand * get();
    }
    
    virtual void Draw() override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flag) override;
    virtual void ClearSelected() override;
    virtual unsigned int WhatKindOfYou();

    void SetMousePosition(DelFEM4NetCom::CVector3D^ mouse);

protected:
     Cad::View::CDrawerRubberBand *self;
};


} // end namespace View
} // end namespace DelFEM4NetCad

#endif
