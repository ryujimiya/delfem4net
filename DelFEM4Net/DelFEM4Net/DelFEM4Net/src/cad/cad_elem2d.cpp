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

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif
#define for if(0);else for

#include <iostream>
#include <cstdlib>    // abort

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/cad/cad_elem2d.h"

using namespace DelFEM4NetCad;


/////////////////////////////////////////////////////////////////////////////////
// CLoop2D
/////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////////
double GetDist_LineSeg_Point(DelFEM4NetCom::CVector2D^ po_c,
                             DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e)
{
    Com::CVector2D* po_c_ = po_c->Self;
    Com::CVector2D* po_s_ = po_s->Self;
    Com::CVector2D* po_e_ = po_e->Self;
    
    return Cad::GetDist_LineSeg_Point(*po_c_, *po_s_, *po_e_);
}

double GetDist_LineSeg_LineSeg(DelFEM4NetCom::CVector2D^ po_s0, DelFEM4NetCom::CVector2D^ po_e0,
                               DelFEM4NetCom::CVector2D^ po_s1, DelFEM4NetCom::CVector2D^ po_e1)
{
    Com::CVector2D* po_s0_ = po_s0->Self;
    Com::CVector2D* po_e0_ = po_e0->Self;
    Com::CVector2D* po_s1_ = po_s1->Self;
    Com::CVector2D* po_e1_ = po_e1->Self;

    return Cad::GetDist_LineSeg_LineSeg(*po_s0_, *po_e0_, *po_s1_, *po_e1_);
}

double IsCross_LineSeg_LineSeg(DelFEM4NetCom::CVector2D^ po_s0, DelFEM4NetCom::CVector2D^ po_e0,
                               DelFEM4NetCom::CVector2D^ po_s1, DelFEM4NetCom::CVector2D^ po_e1)
{
    Com::CVector2D* po_s0_ = po_s0->Self;
    Com::CVector2D* po_e0_ = po_e0->Self;
    Com::CVector2D* po_s1_ = po_s1->Self;
    Com::CVector2D* po_e1_ = po_e1->Self;

    return Cad::IsCross_LineSeg_LineSeg(*po_s0_, *po_e0_, *po_s1_, *po_e1_);
}

bool IsCross_Circle_Circle(DelFEM4NetCom::CVector2D^ po_c0, double radius0,
                           DelFEM4NetCom::CVector2D^ po_c1, double radius1,
                           DelFEM4NetCom::CVector2D^% po0, DelFEM4NetCom::CVector2D^% po1 ) //po0,po1 [OUT]
{
    Com::CVector2D* po_c0_ = po_c0->Self;
    Com::CVector2D* po_c1_ = po_c1->Self;
    Com::CVector2D* po0_ = new Com::CVector2D();
    Com::CVector2D* po1_ = new Com::CVector2D();
    
    bool ret = Cad::IsCross_Circle_Circle(*po_c0_, radius0,
                                          *po_c1_, radius1,
                                          *po0_, *po1_);
    
    po0 = gcnew DelFEM4NetCom::CVector2D(po0_);
    po1 = gcnew DelFEM4NetCom::CVector2D(po1_);

    return ret;
}


bool IsCross_Line_Circle(DelFEM4NetCom::CVector2D^ po_c, const double radius, 
                         DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e, double% t0, double% t1)
{
    Com::CVector2D* po_c_ = po_c->Self;
    Com::CVector2D* po_s_ = po_s->Self;
    Com::CVector2D* po_e_ = po_e->Self;
    double t0_ = t0;
    double t1_ = t1;
    
    bool ret = Cad::IsCross_Line_Circle(*po_c_, radius,
                                        *po_s_, *po_e_, t0_, t1_);  //t0, t1:[OUT]
    
    t0 = t0_;
    t1 = t1_;
    
    return ret;
}


double FindNearestPointParameter_Line_Point(DelFEM4NetCom::CVector2D^ po_c,
                                            DelFEM4NetCom::CVector2D^ po_s, DelFEM4NetCom::CVector2D^ po_e)
{
    Com::CVector2D* po_c_ = po_c->Self;
    Com::CVector2D* po_s_ = po_s->Self;
    Com::CVector2D* po_e_ = po_e->Self;

    return Cad::FindNearestPointParameter_Line_Point(*po_c_, *po_s_, *po_e_);
}

DelFEM4NetCom::CVector2D^ GetProjectedPointOnCircle(DelFEM4NetCom::CVector2D^ c, double r, 
                                                DelFEM4NetCom::CVector2D^ v)
{
    Com::CVector2D* c_ = c->Self;
    Com::CVector2D* v_ = v->Self;
    Com::CVector2D *ret = new Com::CVector2D();

    *ret = Cad::GetProjectedPointOnCircle(*c_, r, *v_);
    
    DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret);
    
    return retManaged;
}

/////////////////////////////////////////////////////////////////////////////////
// CEdge2D
/////////////////////////////////////////////////////////////////////////////////
CEdge2D::CEdge2D()
{
    this->self = new Cad::CEdge2D();
}

CEdge2D::CEdge2D(CEdge2D^ rhs)
{
    Cad::CEdge2D *rhs_ = rhs->Self;
    this->self = new Cad::CEdge2D(*rhs_);
}

CEdge2D::CEdge2D(unsigned int id_v_s, unsigned int id_v_e)
{
    this->self = new Cad::CEdge2D(id_v_s, id_v_e);
}

CEdge2D::CEdge2D(Cad::CEdge2D *self)
{
    this->self = self;
}

CEdge2D::~CEdge2D()
{
    this->!CEdge2D();
}

CEdge2D::!CEdge2D()
{
    delete this->self;
}

Cad::CEdge2D * CEdge2D::Self::get()
{
    return this->self;
}

void CEdge2D::SetVtxCoords(DelFEM4NetCom::CVector2D^ ps, DelFEM4NetCom::CVector2D^ pe)
{
    Com::CVector2D *ps_ = ps->Self;
    Com::CVector2D *pe_ = pe->Self;
    
    this->self->SetVtxCoords(*ps_, *pe_);
}

void CEdge2D::SetIdVtx(unsigned int id_vs, unsigned int id_ve)
{
    this->self->SetIdVtx(id_vs, id_ve);
}

double CEdge2D::Distance(CEdge2D^ e1)
{
    Cad::CEdge2D *e1_ = e1->Self;
    return this->self->Distance(*e1_);
}

DelFEM4NetCom::CBoundingBox2D^ CEdge2D::GetBoundingBox()
{
    const Com::CBoundingBox2D& ret_instance_ = this->self->GetBoundingBox();
    Com::CBoundingBox2D *ret = new Com::CBoundingBox2D(ret_instance_);
    
    DelFEM4NetCom::CBoundingBox2D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox2D(ret);
    
    return retManaged;
}

bool CEdge2D::IsCrossEdgeSelf()
{
    return this->self->IsCrossEdgeSelf();
}

bool CEdge2D::IsCrossEdge(CEdge2D^ e1)
{
    Cad::CEdge2D *e1_ = e1->Self;
    return this->self->IsCrossEdge(*e1_);
}

bool CEdge2D::IsCrossEdge_ShareOnePoint(CEdge2D^ e1, bool is_share_s0, bool is_share_s1)
{
    Cad::CEdge2D *e1_ = e1->Self;
    return this->self->IsCrossEdge_ShareOnePoint(*e1_, is_share_s0, is_share_s1);
}

bool CEdge2D::IsCrossEdge_ShareBothPoints(CEdge2D^ e1, bool is_share_s1s0)
{
    Cad::CEdge2D *e1_ = e1->Self;
    return this->self->IsCrossEdge_ShareBothPoints(*e1_, is_share_s1s0);
}


double CEdge2D::AreaEdge()
{
    return this->self->AreaEdge();
}


DelFEM4NetCom::CVector2D^ CEdge2D::GetTangentEdge(bool is_s)
{
    Com::CVector2D *ret = new Com::CVector2D();
    
    *ret = this->self->GetTangentEdge(is_s);
    
    DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret);
    
    return retManaged;
}

DelFEM4NetCom::CVector2D^ CEdge2D::GetNearestPoint(DelFEM4NetCom::CVector2D^ po_in)
{
    Com::CVector2D *po_in_ = po_in->Self;
    
    Com::CVector2D *ret = new Com::CVector2D();
    
    *ret = this->self->GetNearestPoint(*po_in_);
    
    DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret);
    
    return retManaged;
}

int CEdge2D::NumIntersect_AgainstHalfLine(DelFEM4NetCom::CVector2D^ org, DelFEM4NetCom::CVector2D^ dir)
{
    Com::CVector2D *org_ = org->Self;
    Com::CVector2D *dir_ = dir->Self;
    
    return this->self->NumIntersect_AgainstHalfLine(*org_, *dir_);

}

bool CEdge2D::GetNearestIntersectionPoint_AgainstHalfLine(DelFEM4NetCom::CVector2D^% sec, DelFEM4NetCom::CVector2D^ org, DelFEM4NetCom::CVector2D^ dir)
{
    Com::CVector2D *org_ = org->Self;
    Com::CVector2D *dir_ = dir->Self;
    //Com::CVector2D *sec_ = sec->Self;
    Com::CVector2D *sec_ = new Com::CVector2D();
    
    bool ret = this->self->GetNearestIntersectionPoint_AgainstHalfLine(*sec_,  *org_, *dir_);
    
    sec = gcnew DelFEM4NetCom::CVector2D(sec_);
    
    return ret;
}

bool CEdge2D::GetCurveAsPolyline(IList<DelFEM4NetCom::CVector2D^>^% aCo, int ndiv)
{
    std::vector<Com::CVector2D> aCo_;
    
    bool ret = this->self->GetCurveAsPolyline(aCo_, ndiv);
    
    aCo = DelFEM4NetCom::ClrStub::InstanceVectorToList<Com::CVector2D, DelFEM4NetCom::CVector2D>(aCo_);
    
    return ret;
}


double CEdge2D::GetCurveLength()
{
    return this->self->GetCurveLength();
}

////////////////////////////////

void CEdge2D::GetCurve_Arc(bool% is_left_side, double% dist)
{
    bool is_left_side_ = is_left_side;
    double dist_ = dist;
    
    this->self->GetCurve_Arc(is_left_side_, dist_);
    
    is_left_side = is_left_side_;
    dist = dist_;
}

bool CEdge2D::GetCenterRadius(DelFEM4NetCom::CVector2D^% po_c, double% radius)
{
    //Com::CVector2D *po_c_ = po_c->Self;
    Com::CVector2D *po_c_ = new Com::CVector2D();
    double radius_ = radius;
    
    bool ret = this->self->GetCenterRadius(*po_c_, radius_);
    
    po_c = gcnew DelFEM4NetCom::CVector2D(po_c_);
    
    return ret;
}

bool CEdge2D::GetCenterRadiusThetaLXY(DelFEM4NetCom::CVector2D^% pc, double% radius,
                             double% theta, DelFEM4NetCom::CVector2D^% lx, DelFEM4NetCom::CVector2D^% ly)
{
    Com::CVector2D *pc_ = new Com::CVector2D();
    double radius_ = radius;
    double theta_ = theta;
    Com::CVector2D *lx_ = new Com::CVector2D();
    Com::CVector2D *ly_ = new Com::CVector2D();
    
    bool ret = this->self->GetCenterRadiusThetaLXY(*pc_, radius_, theta_, *lx_, *ly_);
    
    pc = gcnew DelFEM4NetCom::CVector2D(pc_);
    radius = radius_;
    theta = theta_;
    lx = gcnew DelFEM4NetCom::CVector2D(lx_);
    ly = gcnew DelFEM4NetCom::CVector2D(ly_);
    return ret;
}


////////////////////////////////

bool CEdge2D::Split(CEdge2D^% edge_a, DelFEM4NetCom::CVector2D^ pa)
{
    Cad::CEdge2D *edge_a_ = new Cad::CEdge2D();
    Com::CVector2D *pa_ = pa->Self;
    
    bool ret = this->self->Split(*edge_a_, *pa_);  // edge_a_ [OUT] pa [IN]
    
    edge_a = gcnew CEdge2D(edge_a_);
    
    return ret;
}

bool CEdge2D::ConnectEdge(DelFEM4NetCad::CEdge2D^ e1, bool is_add_ahead, bool is_same_dir)
{
    Cad::CEdge2D *e1_ = e1->Self;
    
    return this->self->ConnectEdge(*e1_, is_add_ahead, is_same_dir);
}


bool CEdge2D::GetPointOnCurve_OnCircle(DelFEM4NetCom::CVector2D^ v0, double len, bool is_front,
                                       bool% is_exceed, DelFEM4NetCom::CVector2D^% out)
{
    Com::CVector2D *v0_ = v0->Self;
    bool is_exceed_ = is_exceed;
    Com::CVector2D *out_ = new Com::CVector2D();
    
    bool ret = this->self->GetPointOnCurve_OnCircle(*v0_, len, is_front,
                                                    is_exceed_, *out_);
    
    is_exceed = is_exceed_;
    out = gcnew DelFEM4NetCom::CVector2D(out_);
    
    return ret;
}

DelFEM4NetCad::CURVE_TYPE CEdge2D::GetCurveType()
{
    Cad::CURVE_TYPE ret = this->self->GetCurveType();
    return static_cast<DelFEM4NetCad::CURVE_TYPE>(ret);
}

void CEdge2D::SetCurve_Line()
{
    this->self->SetCurve_Line();
}

void CEdge2D::SetCurve_Arc(bool is_left_side, double dist)
{
    this->self->SetCurve_Arc(is_left_side, dist);
}

void CEdge2D::SetCurve_Polyline(IList<double>^ aRelCo)
{
    std::vector<double> aRelCo_;
    DelFEM4NetCom::ClrStub::ListToVector<double>(aRelCo, aRelCo_);
    
    this->self->SetCurve_Polyline(aRelCo_);
    
}

void CEdge2D::SetCurve_Bezier(double cx0, double cy0, double cx1, double cy1)
{
    this->self->SetCurve_Bezier(cx0, cy0, cx1, cy1);
}


IList<double>^ CEdge2D::GetCurve_Polyline()
{
    const std::vector<double>& vec = this->self->GetCurve_Polyline();
    
    IList<double>^ list =  DelFEM4NetCom::ClrStub::VectorToList<double>(vec);
    
    return list;
}

void CEdge2D::GetCurve_Bezier(array<double>^% aRelCo)
{
    double aRelCo_[4];
    
    this->self->GetCurve_Bezier(aRelCo_);
    
    aRelCo = gcnew array<double>(4);
    for (int i = 0; i < 4; i++)
    {
        aRelCo[i] = aRelCo_[i];
    }
}

//////////////////////////////////////////////////////////////////////////////////////////////
int CheckEdgeIntersection(IList<CEdge2D^>^ aEdge)
{
    std::vector<Cad::CEdge2D> aEdge_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CEdge2D, Cad::CEdge2D>(aEdge, aEdge_);

    return Cad::CheckEdgeIntersection(aEdge_);
}
