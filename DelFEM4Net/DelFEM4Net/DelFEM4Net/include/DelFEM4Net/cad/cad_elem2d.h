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
@brief Interfaces define the geometry of 2d cad elements
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_ELEM_2D_H)
#define DELFEM4NET_CAD_ELEM_2D_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>
#include <assert.h>
#include <iostream> // needed only in debug

#include "DelFEM4Net/vector2d.h"

#include "delfem/cad/cad_elem2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////////////////////

namespace DelFEM4NetCad{
  
public enum class CURVE_TYPE
{
  CURVE_END_POINT,
  CURVE_LINE,
  CURVE_ARC,
  CURVE_POLYLINE,
  CURVE_BEZIER
};
  
/*!
@addtogroup CAD
*/
//!@{

//! 2dim loop class
public ref class CLoop2D
{
public:
    CLoop2D()
    {
        this->self = new Cad::CLoop2D();
    }
    
    CLoop2D(CLoop2D^ rhs)
    {
        Cad::CLoop2D *rhs_ = rhs->Self;
        this->self = new Cad::CLoop2D(*rhs_); // コピーコンストラクタを使用
    }
    
    CLoop2D(Cad::CLoop2D *self)
    {
        this->self = self;
    }
    
    ~CLoop2D()
    {
        this->!CLoop2D();
    }
    
    !CLoop2D()
    {
        delete this->self;
    }
    
public:
    property Cad::CLoop2D * Self
    {
        Cad::CLoop2D * get() { return this->self; }
    }
    property array<double>^ m_color   // double m_color[3]
    {
         array<double>^ get()
         {
             array<double>^ colorArray = gcnew array<double>(3) ;
             
             for (int i = 0; i < 3; i++)
             {
                 colorArray[i] = this->self->m_color[i];
             }
             return colorArray;
         }
         void set(array<double>^ value)
         {
             for (int i = 0; i < 3; i++)
             {
                 this->self->m_color[i] = value[i];
             }
         }
    }
    property unsigned int ilayer
    {
        unsigned int get() { return this->self->ilayer; }
        void set(unsigned int value) { this->self->ilayer = value; }
    }

protected:
    Cad::CLoop2D *self;
    
};
  
  
double GetDist_LineSeg_Point(DelFEM4NetCom::CVector2D^ po_c,
                             DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e);

double GetDist_LineSeg_LineSeg(DelFEM4NetCom::CVector2D^ po_s0, DelFEM4NetCom::CVector2D^ po_e0,
                               DelFEM4NetCom::CVector2D^ po_s1, DelFEM4NetCom::CVector2D^ po_e1);

  

// line-line intersection detection
double IsCross_LineSeg_LineSeg(DelFEM4NetCom::CVector2D^ po_s0, DelFEM4NetCom::CVector2D^ po_e0,
                               DelFEM4NetCom::CVector2D^ po_s1, DelFEM4NetCom::CVector2D^ po_e1);
  
//! circle-circle interseciton detection
bool IsCross_Circle_Circle(DelFEM4NetCom::CVector2D^ po_c0, double radius0,
                           DelFEM4NetCom::CVector2D^ po_c1, double radius1,
                           DelFEM4NetCom::CVector2D^% po0, DelFEM4NetCom::CVector2D^% po1 );  //po0, po1 [OUT]
/*!
 @brief 円弧と直線の交点を求める
 交点がある場合は２つの交点のposからpoeへのパラメータがt1,t2に入る．
 @retval true 交点がある場合
 @retval false 交点が無い場合
 */  
bool IsCross_Line_Circle(DelFEM4NetCom::CVector2D^ po_c, const double radius, 
                         DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e, double% t0, double% t1); //t0, t1 OUT]
//! 点と直線の一番近い点を探す
double FindNearestPointParameter_Line_Point(DelFEM4NetCom::CVector2D^ po_c,
                                            DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e);
  
DelFEM4NetCom::CVector2D^ GetProjectedPointOnCircle(DelFEM4NetCom::CVector2D^ c, double r, 
                                                DelFEM4NetCom::CVector2D^ v);

//! 2dim edge
public ref class CEdge2D
{
public:
    CEdge2D();
    CEdge2D(CEdge2D^ rhs);
    CEdge2D(unsigned int id_v_s, unsigned int id_v_e);
    CEdge2D(Cad::CEdge2D *self);
    ~CEdge2D();
    !CEdge2D();
    property Cad::CEdge2D * Self
    {
        Cad::CEdge2D * get();
    }
  
    void SetVtxCoords(DelFEM4NetCom::CVector2D^ ps, DelFEM4NetCom::CVector2D^ pe);

    void SetIdVtx(unsigned int id_vs, unsigned int id_ve);
  
    // return minimum distance between this edge and e1
    // if the edge obviouly intersects, this function return 0 or -1
    double Distance(CEdge2D^ e1);

    // get bounding box of edge
    // lazy evaluation
    // make sure the value is set in po_s, po_e
    DelFEM4NetCom::CBoundingBox2D^ GetBoundingBox();
    
    bool IsCrossEdgeSelf();    // check self intersection
    bool IsCrossEdge(CEdge2D^ e1);    // intersection between me and e1
    //! 一端が共有された辺同士の交差判定
    bool IsCrossEdge_ShareOnePoint(CEdge2D^ e1, bool is_share_s0, bool is_share_s1);
    //! 両端が共有された辺同士の交差判定
    bool IsCrossEdge_ShareBothPoints(CEdge2D^ e1, bool is_share_s1s0);

    /*!
     @brief カーブと辺の２頂点を結ぶ直線で囲まれる面積を計算(直線の右側にあれば＋)
     @remarks ループの面積を得るのに使う
     */  
    double AreaEdge();

    //! 辺の始点/終点における接線を計算する
    DelFEM4NetCom::CVector2D^ GetTangentEdge(bool is_s);
    //! 入力点から最も近い辺上の点と距離を返す
    DelFEM4NetCom::CVector2D^ GetNearestPoint(DelFEM4NetCom::CVector2D^ po_in);

    // get number of intersection between half line direction (=dir) from point (=org)
    // this function is used for in-out detection
    int NumIntersect_AgainstHalfLine(DelFEM4NetCom::CVector2D^ org, DelFEM4NetCom::CVector2D^ dir);
    bool GetNearestIntersectionPoint_AgainstHalfLine(DelFEM4NetCom::CVector2D^% sec, DelFEM4NetCom::CVector2D^ org, DelFEM4NetCom::CVector2D^ dir); // sec: [OUT]
    bool GetCurveAsPolyline(IList<DelFEM4NetCom::CVector2D^>^% aCo, int ndiv);
    double GetCurveLength();

    ////////////////////////////////

    // Get Arc property if this curve is not arc, this returns false
    // if is_left_side is true, this arc lies left side from the line connect start and end point of this edge (ID:id_e).
    // The dist means how far is the center of the circle from the line connect start and end point of edge (ID:id_e).  
    void GetCurve_Arc(bool% is_left_side, double% dist);

    /*!
     @brief 円が円弧の時、円の中心と半径を計算する
     @remarks 円弧じゃなかったらfalseを返す
     */
    bool GetCenterRadius(DelFEM4NetCom::CVector2D^% po_c, double% radius);
    bool GetCenterRadiusThetaLXY(DelFEM4NetCom::CVector2D^% pc, double% radius,
                                 double% theta, DelFEM4NetCom::CVector2D^% lx, DelFEM4NetCom::CVector2D^% ly);
  
    
  
    ////////////////////////////////
    

    // 現在の辺が２つに分割されて，一端がedge_aに入る
    bool Split(CEdge2D^% edge_a, DelFEM4NetCom::CVector2D^ pa);
    // is_add_aheadはe1がこの辺の前にあるか，is_same_dirはe1がこの辺と同じ向きか
    bool ConnectEdge(DelFEM4NetCad::CEdge2D^ e1, bool is_add_ahead, bool is_same_dir);
  
    // get vertex on edge with distance (len) from point v0 along the edge
    // is_front==true:same direction is_front==false:opposite direciton
    bool GetPointOnCurve_OnCircle(DelFEM4NetCom::CVector2D^ v0, double len, bool is_front,
                                  bool% is_exceed, DelFEM4NetCom::CVector2D^% out);    
  
    DelFEM4NetCad::CURVE_TYPE GetCurveType();
    
    void SetCurve_Line();
    void SetCurve_Arc(bool is_left_side, double dist);
    void SetCurve_Polyline(IList<double>^ aRelCo);
    void SetCurve_Bezier(double cx0, double cy0, double cx1, double cy1);
    IList<double>^ GetCurve_Polyline();
    void GetCurve_Bezier(array<double>^% aRelCo);
  
    inline unsigned int GetIdVtx(bool is_root)
    {
        return this->self->GetIdVtx(is_root);
    }
    inline DelFEM4NetCom::CVector2D^ GetVtxCoord(bool is_root)
    {
        Com::CVector2D *ret = new Com::CVector2D();
        
        *ret = this->self->GetVtxCoord(is_root);
        
        DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret);
        
        return retManaged;
    }
    inline void GetColor([Out] array<double>^% color)
    {
        color = gcnew array<double>(3);
        pin_ptr<double> color_ = &color[0];
        this->self->GetColor(color_);
    }
    inline void SetColor(array<double>^ color)
    {
        pin_ptr<double> color_ = &color[0];
        this->self->SetColor(color_);
    }

protected:
    Cad::CEdge2D *self;

};

//! ２次元幾何頂点クラス
public ref class CVertex2D
{
public:
    CVertex2D()
    {
        //this->self = new Cad::CVertex2D();  // 明示的には実装されていない

        DelFEM4NetCom::CVector2D^ point = gcnew DelFEM4NetCom::CVector2D(0.0, 0.0);
        Com::CVector2D *point_ = point->Self;
        this->self = new Cad::CVertex2D(*point_);
    }
    
    CVertex2D(DelFEM4NetCom::CVector2D^ point)
    {
        Com::CVector2D *point_ = point->Self;
        this->self = new Cad::CVertex2D(*point_);
    }
    
    CVertex2D(CVertex2D^ rhs)
    {
        Cad::CVertex2D *rhs_ = rhs->Self;
        this->self = new Cad::CVertex2D(*rhs_); // コピーコンストラクタ
    }
    
    CVertex2D(Cad::CVertex2D *self)
    {
        this->self = self;
    }
    
    ~CVertex2D()
    {
        this->!CVertex2D();
    }
    
    !CVertex2D()
    {
        delete this->self;
    }
    
public:
    property Cad::CVertex2D * Self
    {
        Cad::CVertex2D * get() { return this->self; }
    }
    
    property DelFEM4NetCom::CVector2D^ point   //!< coordinate
    {
        DelFEM4NetCom::CVector2D^ get()
        {
            Com::CVector2D *value_ = new Com::CVector2D();
            
            *value_ = this->self->point;
            
            DelFEM4NetCom::CVector2D^ value = gcnew DelFEM4NetCom::CVector2D(value_);
            return value;
        }
        void set(DelFEM4NetCom::CVector2D^ value)
        {
            Com::CVector2D *value_ = value->Self;
            this->self->point = *value_;
        }
    }

    property array<double>^ m_color   // double m_color[3]
    {
         array<double>^ get()
         {
             array<double>^ colorArray = gcnew array<double>(3) ;
             
             for (int i = 0; i < 3; i++)
             {
                 colorArray[i] = this->self->m_color[i];
             }
             return colorArray;
         }
         void set(array<double>^ value)
         {
             for (int i = 0; i < 3; i++)
             {
                 this->self->m_color[i] = value[i];
             }
         }
    }

protected:
    Cad::CVertex2D *self;

};

/*!
干渉チェックを行う
そのうち交錯位置の情報も返したい
*/
int CheckEdgeIntersection(IList<CEdge2D^>^ aEdge);

//! @}
}

#endif
