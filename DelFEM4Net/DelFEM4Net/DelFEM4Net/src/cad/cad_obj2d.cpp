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


///////////////////////////////////////////////////////////////////////////////
// CadObj2D.cpp : ２次元ＣＡＤモデルクラス(CCadObj2D)の実装のC++/CLR wrapper
///////////////////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#include <iostream>
#include <set>
#include <map>
#include <vector>
#include <cassert>
#include <math.h>
#include <cstring>    // strlen
#include <typeinfo> // typeid

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/cad_obj2d.h"
#include "DelFEM4Net/cad/cad_elem2d.h"

using namespace DelFEM4NetCad;
using namespace DelFEM4NetCom;

////////////////////////////////////////////////////////////////
// CCadObj2D::CResAddVertex
////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////
// CCadObj2D::CResAddPolygon
////////////////////////////////////////////////////////////////

IList<unsigned int>^ CCadObj2D::CResAddPolygon::aIdV::get()
{
    return DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(this->self->aIdV);
}

void CCadObj2D::CResAddPolygon::aIdV::set(IList<unsigned int>^ value)
{
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(value, this->self->aIdV);
}

IList<unsigned int>^ CCadObj2D::CResAddPolygon::aIdE::get()
{
    return DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(this->self->aIdE);
}

void CCadObj2D::CResAddPolygon::aIdE::set(IList<unsigned int>^ value)
{
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(value, this->self->aIdE);
}

////////////////////////////////////////////////////////////////
// CCadObj2D
////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////
// constructor & destructor

CCadObj2D::CCadObj2D()
{
    self = new Cad::CCadObj2D();
}

CCadObj2D::CCadObj2D(const CCadObj2D% rhs)
{
    const Cad::CCadObj2D& rhs_instance_ = *(rhs.self);
    this->self = new Cad::CCadObj2D(rhs_instance_);  //コピーコンストラクタを使用する
}

CCadObj2D::CCadObj2D(Cad::CCadObj2D* self)
{
    this->self = self;
}

CCadObj2D::~CCadObj2D()
{
    this->!CCadObj2D();
}

CCadObj2D::!CCadObj2D()
{
    delete this->self;
}

Cad::ICad2D_Msh* CCadObj2D::Cad2DMshSelf::get()
{
    return this->self;
}

Cad::CCadObj2D* CCadObj2D::Self::get()
{
    return this->self;
}

void CCadObj2D::Clear()
{
    this->self->Clear();
}

CCadObj2D^ CCadObj2D::Clone()
{
    return gcnew CCadObj2D(*this);
}


////////////////////////////////////////////////////////////////
// Get method
DelFEM4NetCad::IItrLoop^ CCadObj2D::GetPtrItrLoop(unsigned int id_l)
{
    DelFEM4NetCad::IItrLoop^ retManaged = nullptr;

    // 生成された後でないと型が分からない
    std::auto_ptr<Cad::IItrLoop> autoPtr = this->self->GetPtrItrLoop(id_l);  // インスタンスのオートポインタが返却される
    // オートポインタを解除
    Cad::IItrLoop *ptr = autoPtr.release();

    if (typeid(*ptr) == typeid(Cad::CBRepSurface::CItrLoop))
    {
        DelFEM4NetCad::CBRepSurface::CItrLoop^ managed = gcnew DelFEM4NetCad::CBRepSurface::CItrLoop((Cad::CBRepSurface::CItrLoop *)ptr);
        retManaged = (DelFEM4NetCad::IItrLoop^)managed;
    }
    else
    {
        throw gcnew NotImplementedException();
    }

    return retManaged;
}

bool CCadObj2D::IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype,unsigned int id)
{
    Cad::CAD_ELEM_TYPE t = (Cad::CAD_ELEM_TYPE)itype;
    return this->self->IsElemID(t, id);
}

IList<unsigned int>^ CCadObj2D::GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype)
{
    Cad::CAD_ELEM_TYPE itype_ = (Cad::CAD_ELEM_TYPE)itype;
    
    const std::vector<unsigned int> vec = this->self->GetAryElemID(itype_);
    
    IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
    return list;
}

bool CCadObj2D::GetIdVertex_Edge([Out] unsigned int% id_v_s, [Out] unsigned int% id_v_e, unsigned int id_e)
{
    unsigned int id_v_s_ = id_v_s;
    unsigned int id_v_e_ = id_v_e;
    
    bool ret = this->self->GetIdVertex_Edge(id_v_s_, id_v_e_, id_e);
    
    id_v_s = id_v_s_;
    id_v_e = id_v_e_;
    return ret;
}

bool CCadObj2D::GetIdLoop_Edge([Out] unsigned int% id_l_l, [Out] unsigned int% id_l_r, unsigned int id_e)
{
    unsigned int id_l_l_ = id_l_l;
    unsigned int id_l_r_ = id_l_r;
    
    bool ret = this->self->GetIdLoop_Edge(id_l_l_, id_l_r_, id_e);

    id_l_l = id_l_l_;
    id_l_r = id_l_r_;
    return ret;
}

CBRepSurface::CItrVertex^ CCadObj2D::GetItrVertex(unsigned int id_v)
{
    Cad::CBRepSurface::CItrVertex& itrv_instance_ = this->self->GetItrVertex(id_v);
    Cad::CBRepSurface::CItrVertex *itrv_ = new Cad::CBRepSurface::CItrVertex(itrv_instance_);
    
    CBRepSurface::CItrVertex^ retManaged = gcnew CBRepSurface::CItrVertex(itrv_);
    return retManaged;
}

CBRepSurface::CItrLoop^ CCadObj2D::GetItrLoop(unsigned int id_l)
{
    Cad::CBRepSurface::CItrLoop& itrl_instance_ = this->self->GetItrLoop(id_l);
    Cad::CBRepSurface::CItrLoop *itrl_ = new Cad::CBRepSurface::CItrLoop(itrl_instance_);

    CBRepSurface::CItrLoop^ retManaged = gcnew CBRepSurface::CItrLoop(itrl_);
    return retManaged;
}

int CCadObj2D::GetLayer(CAD_ELEM_TYPE type, unsigned int id)
{
    Cad::CAD_ELEM_TYPE t = (Cad::CAD_ELEM_TYPE)type;
    return this->self->GetLayer(t, id);
}

void CCadObj2D::GetLayerMinMax(int% layer_min, int% layer_max)
{
    int layer_min_ = layer_min;
    int layer_max_ = layer_max;
    
    this->self->GetLayerMinMax(layer_min_, layer_max_);

    layer_min = layer_min_;
    layer_max = layer_max_;
}

bool CCadObj2D::ShiftLayer_Loop(unsigned int id_l, bool is_up)
{
    return this->self->ShiftLayer_Loop(id_l, is_up);
}

double CCadObj2D::GetMinClearance()
{
    return this->self->GetMinClearance();
}

bool CCadObj2D::CheckIsPointInsideLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point)
{
    Com::CVector2D point_  = *(point->Self);
    return this->self->CheckIsPointInsideLoop(id_l1, point_);
}

double CCadObj2D::SignedDistPointLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point)
{
    return SignedDistPointLoop(id_l1, point, 0);
}

double CCadObj2D::SignedDistPointLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point, unsigned int id_v_ignore)
{
    Com::CVector2D point_  = *(point->Self);
    return this->self->SignedDistPointLoop(id_l1, point_, id_v_ignore);
}

bool CCadObj2D::GetColor_Loop(unsigned int id_l, [Out] array<double>^% color)
{    
    color = gcnew array<double>(3);
    pin_ptr<double> color_ = &color[0];
    bool ret = this->self->GetColor_Loop(id_l, color_);
    
    return ret;
}

bool CCadObj2D::SetColor_Loop(unsigned int id_l, array<double>^ color)
{
    assert(color != nullptr);
    assert(color->Length == 3);
    pin_ptr<double> color_ = &color[0];

    bool ret = this->self->SetColor_Loop(id_l, color_);
    
    return ret;
}

double CCadObj2D::GetArea_Loop(unsigned int id_l)
{
    return this->self->GetArea_Loop(id_l);
}

////////////////////////////////////////////////////////////////
// Edge member functions

CEdge2D^ CCadObj2D::GetEdge(unsigned int id_e)
{
    const Cad::CEdge2D& ret_instance_ = this->self->GetEdge(id_e);
    Cad::CEdge2D *ret = new Cad::CEdge2D(ret_instance_);

    CEdge2D^ retManaged = gcnew CEdge2D(ret);
    return retManaged;
}


DelFEM4NetCad::CURVE_TYPE CCadObj2D::GetEdgeCurveType(const unsigned int id_e)
{
    Cad::CURVE_TYPE ret = this->self->GetEdgeCurveType(id_e);
    return static_cast<DelFEM4NetCad::CURVE_TYPE>(ret);
}

bool CCadObj2D::GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo)
{
    return GetCurveAsPolyline(id_e, aCo, -1);
}

bool CCadObj2D::GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo, double elen)
{
    std::vector<Com::CVector2D> aCo_;

    bool ret = this->self->GetCurveAsPolyline(id_e, aCo_, elen); //[OUT]:aCo_
    
    aCo = DelFEM4NetCom::ClrStub::InstanceVectorToList<Com::CVector2D, DelFEM4NetCom::CVector2D>(aCo_);    

    return ret;
} 

bool CCadObj2D::GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo,
                                   unsigned int ndiv, DelFEM4NetCom::CVector2D^ ps, DelFEM4NetCom::CVector2D^ pe)
{
    std::vector<Com::CVector2D> aCo_;
    Com::CVector2D* ps_ = ps->Self;
    Com::CVector2D* pe_ = pe->Self;

    bool ret = this->self->GetCurveAsPolyline(id_e, aCo_, ndiv, *ps_, *pe_);//[OUT]aCo_
    
    aCo = DelFEM4NetCom::ClrStub::InstanceVectorToList<Com::CVector2D, DelFEM4NetCom::CVector2D>(aCo_);    

    return ret;
}


bool CCadObj2D::GetPointOnCurve_OnCircle(unsigned int id_e,
                                         DelFEM4NetCom::CVector2D^ v0, double len, bool is_front,
                                         bool% is_exceed, DelFEM4NetCom::CVector2D^% out)
{
    Com::CVector2D *v0_ = v0->Self;
    bool is_exceed_ = is_exceed;
    Com::CVector2D *out_ = new Com::CVector2D() ;

    bool ret = this->self->GetPointOnCurve_OnCircle(id_e, *v0_, len, is_front,
                                                    is_exceed_, *out_);   //[OUT]is_exceed_, out_
    is_exceed = is_exceed;
    out = gcnew DelFEM4NetCom::CVector2D(out_);
    
    return ret;
}

DelFEM4NetCom::CVector2D^ CCadObj2D::GetNearestPoint(unsigned int id_e, DelFEM4NetCom::CVector2D^ po_in)
{
    Com::CVector2D *po_in_ = po_in->Self;
    Com::CVector2D *ret = new Com::CVector2D();
    
    *ret = this->self->GetNearestPoint(id_e, *po_in_);
    
    DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret) ;
    
    return retManaged;
}

bool CCadObj2D::GetColor_Edge(unsigned int id_e, [Out] array<double>^% color)
{    
    color = gcnew array<double>(3);
    pin_ptr<double> color_ = &color[0];
    bool ret = this->self->GetColor_Edge(id_e, color_);
    
    return ret;
}

bool CCadObj2D::SetColor_Edge(unsigned int id_e, array<double>^ color)
{
    assert(color != nullptr);
    assert(color->Length == 3);
    pin_ptr<double> color_ = &color[0];

    bool ret = this->self->SetColor_Edge(id_e, color_);
    
    return ret;
}

////////////////////////////////////////////////////////////////
// Vertex
DelFEM4NetCom::CVector2D^ CCadObj2D::GetVertexCoord(unsigned int id_v)
{
    Com::CVector2D *ret = new Com::CVector2D();
    
    *ret = this->self->GetVertexCoord(id_v);
    
    DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret) ;
    
    return retManaged;
}

bool CCadObj2D::GetColor_Vertex(unsigned int id_v, [Out] array<double>^% color)
{    
    color = gcnew array<double>(3);
    pin_ptr<double> color_ = &color[0];
    bool ret = this->self->GetColor_Vertex(id_v, color_);
    
    return ret;
}

bool CCadObj2D::SetColor_Vertex(unsigned int id_v, array<double>^ color)
{
    assert(color != nullptr);
    assert(color->Length == 3);
    pin_ptr<double> color_ = &color[0];

    bool ret = this->self->SetColor_Vertex(id_v, color_);
    
    return ret;
}

////////////////////////////////////////////////////////////////
// Toplogy affecting shape edit functions
CCadObj2D::CResAddPolygon^ CCadObj2D::AddPolygon(IList<DelFEM4NetCom::CVector2D^>^ vec_ary)
{
    return AddPolygon(vec_ary, 0);
}

CCadObj2D::CResAddPolygon^ CCadObj2D::AddPolygon(IList<DelFEM4NetCom::CVector2D^>^ vec_ary, unsigned int id_l)
{
    std::vector<Com::CVector2D> vec_ary_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetCom::CVector2D, Com::CVector2D>(vec_ary, vec_ary_);
    
    const Cad::CCadObj2D::CResAddPolygon& ret_instance_ = this->self->AddPolygon(vec_ary_, id_l);
    Cad::CCadObj2D::CResAddPolygon *ret = new Cad::CCadObj2D::CResAddPolygon(ret_instance_);
    
    CCadObj2D::CResAddPolygon^ retManaged = gcnew CCadObj2D::CResAddPolygon(ret);
    
    return retManaged;
}

CCadObj2D::CResAddPolygon^ CCadObj2D::AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal)
{
    return AddLoop(aVal, 0, 1);
}

CCadObj2D::CResAddPolygon^ CCadObj2D::AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal, unsigned int id_l)
{
    return AddLoop(aVal, id_l, 1);
}

CCadObj2D::CResAddPolygon^ CCadObj2D::AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal, unsigned int id_l, double scale)
{
    std::vector< std::pair<Cad::CURVE_TYPE, std::vector<double> > > aVal_;
    
    if (aVal->Count > 0)
    {
        for each (DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^ pair in aVal)
        {
            DelFEM4NetCad::CURVE_TYPE iType = static_cast<DelFEM4NetCad::CURVE_TYPE>(pair->First);
            IList<double>^ dataList = pair->Second;
            
            Cad::CURVE_TYPE iType_ = static_cast<Cad::CURVE_TYPE>(iType);
            std::vector<double> dataList_;
            DelFEM4NetCom::ClrStub::ListToVector<double>(dataList, dataList_);

            std::pair<Cad::CURVE_TYPE, std::vector<double>> pair_(iType_, dataList_);
            
            aVal_.push_back(pair_);
        } 
    }
    
    const Cad::CCadObj2D::CResAddPolygon& ret_instance_ = this->self->AddLoop(aVal_, id_l, scale);
    Cad::CCadObj2D::CResAddPolygon *ret = new Cad::CCadObj2D::CResAddPolygon(ret_instance_);
    
    CCadObj2D::CResAddPolygon^ retManaged = gcnew CCadObj2D::CResAddPolygon(ret);
    return retManaged;
}

CCadObj2D::CResAddVertex^ CCadObj2D::AddVertex(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id_elem, DelFEM4NetCom::CVector2D^ vec)
{
    Cad::CAD_ELEM_TYPE itype_ = static_cast<Cad::CAD_ELEM_TYPE>(itype);
    Com::CVector2D *vec_ = vec->Self;
    
    const Cad::CCadObj2D::CResAddVertex& ret_instance_ = this->self->AddVertex(itype_, id_elem, *vec_);
    Cad::CCadObj2D::CResAddVertex *ret = new Cad::CCadObj2D::CResAddVertex(ret_instance_);
    
    CCadObj2D::CResAddVertex^ retManaged = gcnew CCadObj2D::CResAddVertex(ret);
    return retManaged;
}

bool CCadObj2D::RemoveElement(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id_elem)
{
    Cad::CAD_ELEM_TYPE itype_ = static_cast<Cad::CAD_ELEM_TYPE>(itype);
    return this->self->RemoveElement(itype_, id_elem);
}

CBRepSurface::CResConnectVertex^ CCadObj2D::ConnectVertex(CEdge2D^ edge)
{
    Cad::CEdge2D *edge_ = edge->Self;
    
    const Cad::CBRepSurface::CResConnectVertex& ret_instance_ = this->self->ConnectVertex(*edge_);
    Cad::CBRepSurface::CResConnectVertex *ret = new Cad::CBRepSurface::CResConnectVertex(ret_instance_);
    
    CBRepSurface::CResConnectVertex^ retManaged = gcnew CBRepSurface::CResConnectVertex(ret);
    return retManaged;
}

CBRepSurface::CResConnectVertex^ CCadObj2D::ConnectVertex_Line(unsigned int id_v1, unsigned int id_v2)
{
    const Cad::CBRepSurface::CResConnectVertex& ret_instance_ = this->self->ConnectVertex_Line(id_v1, id_v2);
    Cad::CBRepSurface::CResConnectVertex *ret = new Cad::CBRepSurface::CResConnectVertex(ret_instance_);
    
    CBRepSurface::CResConnectVertex^ retManaged = gcnew CBRepSurface::CResConnectVertex(ret);
    return retManaged;
}

////////////////////////////////////////////////
// Geometry editing functions (topoloty intact)

bool CCadObj2D::SetCurve_Bezier(unsigned int id_e, double cx0, double cy0, double cx1, double cy1)
{
    return this->self->SetCurve_Bezier(id_e, cx0, cy0, cx1, cy1);
}

bool CCadObj2D::SetCurve_Polyline(const unsigned int id_e)
{
    return this->self->SetCurve_Polyline(id_e);
}

bool CCadObj2D::SetCurve_Polyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^ aVec)
{
    std::vector<Com::CVector2D> aVec_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<DelFEM4NetCom::CVector2D,Com::CVector2D>(aVec, aVec_);
    
    return this->self->SetCurve_Polyline(id_e, aVec_) ;
}

bool CCadObj2D::SetCurve_Arc(const unsigned int id_e)
{
    return SetCurve_Arc(id_e, false, 10.0);
}

bool CCadObj2D::SetCurve_Arc(const unsigned int id_e, bool is_left_side)
{
    return SetCurve_Arc(id_e, is_left_side, 10.0);
}

bool CCadObj2D::SetCurve_Arc(const unsigned int id_e, bool is_left_side, double rdist)
{
    return this->self->SetCurve_Arc(id_e, is_left_side, rdist);
}

bool CCadObj2D::SetCurve_Line(const unsigned int id_e)
{
    return this->self->SetCurve_Line(id_e);
}

////////////////////////////////////////////////
// IO routines

bool CCadObj2D::WriteToFile_dxf(String^ file_name, double scale)
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    
    bool ret = this->self->WriteToFile_dxf(file_name_, scale);

    return ret;
}

bool CCadObj2D::Serialize(DelFEM4NetCom::CSerializer^ serialize)
{
    Com::CSerializer *serialize_ = serialize->Self;
    
    return this->self->Serialize(*serialize_);
}

