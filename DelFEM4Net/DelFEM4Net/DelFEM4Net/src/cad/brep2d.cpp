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
// brep2d.cpp : ２次元位相クラス(Cad::CBrep2D)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#define for if(0);else for

#include <iostream>
#include <set>
#include <map>
#include <vector>    
#include <cassert>    
#include <math.h>    
#include <cstring>    // strlen

#include "DelFEM4Net/cad/brep2d.h"
//#include "DelFEM4Net/cad/brep.h"

using namespace DelFEM4NetCad;

/////////////////////////////////////////////////////////////////////////////////////////
// CBRepSurface::CItrLoop::CItrLoop
/////////////////////////////////////////////////////////////////////////////////////////
/*
CBRepSurface::CItrLoop::CItrLoop()
{
    this->self = new Cad::CBRepSurface::CItrLoop();  // 明示的には実装されていない
}
*/

CBRepSurface::CItrLoop::CItrLoop(CBRepSurface^ pBRep2D, unsigned int id_l)
{
    Cad::CBRepSurface *pBRep2D_ = pBRep2D->Self;
    this->self = new Cad::CBRepSurface::CItrLoop(*pBRep2D_, id_l);
}

CBRepSurface::CItrLoop::CItrLoop(CBRepSurface^ pBRep2D, unsigned int id_he, unsigned int id_ul)
{
    Cad::CBRepSurface *pBRep2D_ = pBRep2D->Self;
    this->self = new Cad::CBRepSurface::CItrLoop(*pBRep2D_, id_he, id_ul);
}

CBRepSurface::CItrLoop::CItrLoop(Cad::CBRepSurface::CItrLoop *self)
{
    this->self = self;
}

CBRepSurface::CItrLoop::~CItrLoop()
{
    this->!CItrLoop();
}

CBRepSurface::CItrLoop::!CItrLoop()
{
    delete this->self;
}

Cad::CBRepSurface::CItrLoop * CBRepSurface::CItrLoop::Self::get()
{
    return this->self;
}


void CBRepSurface::CItrLoop::Begin()
{
    this->self->Begin();
}

bool CBRepSurface::CItrLoop::IsEnd()
{
    return this->self->IsEnd();
}

void CBRepSurface::CItrLoop::operator++()
{
    this->self->operator++();
}

void CBRepSurface::CItrLoop::operator++(int n)
{
    this->self->operator++(n);
}

bool CBRepSurface::CItrLoop::GetIdEdge([Out] unsigned int% id_e, [Out] bool% is_same_dir) 
{
    unsigned int id_e_ = id_e;
    bool is_same_dir_ = is_same_dir;
    
    bool ret = this->self->GetIdEdge(id_e_, is_same_dir_);  // 参照で値を取得

    id_e = id_e_;
    is_same_dir = is_same_dir_;
    return ret ;
}

bool CBRepSurface::CItrLoop::ShiftChildLoop()
{
    return this->self->ShiftChildLoop();
}

bool CBRepSurface::CItrLoop::IsEndChild()
{
    return this->self->IsEndChild();
}

unsigned int CBRepSurface::CItrLoop::GetIdVertex()
{
    return this->self->GetIdVertex();
}

unsigned int CBRepSurface::CItrLoop::GetIdVertex_Ahead() 
{
    return this->self->GetIdVertex_Ahead();
}

unsigned int CBRepSurface::CItrLoop::GetIdVertex_Behind() 
{
    return this->self->GetIdVertex_Behind();
}

unsigned int CBRepSurface::CItrLoop::GetIdHalfEdge()
{
    return this->self->GetIdHalfEdge();
}
unsigned int CBRepSurface::CItrLoop::GetIdUseLoop()
{
    return this->self->GetIdUseLoop();
}

unsigned int CBRepSurface::CItrLoop::GetIdLoop()
{
    return this->self->GetIdLoop();
}

unsigned int CBRepSurface::CItrLoop::GetType() // 0:浮遊点 1:浮遊辺 2:面積がある
{
    return this->self->GetType();
}

unsigned int CBRepSurface::CItrLoop::CountVertex_UseLoop()
{
    return this->self->CountVertex_UseLoop();
}

bool CBRepSurface::CItrLoop::IsParent()
{
    return this->self->IsParent();
}

bool CBRepSurface::CItrLoop::IsSameUseLoop(CItrLoop^ itrl)
{
    Cad::CBRepSurface::CItrLoop *itrl_ = itrl->Self;
    return this->self->IsSameUseLoop(*itrl_);
}

bool CBRepSurface::CItrLoop::IsEdge_BothSideSameLoop()
{
    return this->self->IsEdge_BothSideSameLoop();
}



////////////////////////////////////////////////////////////////
// CBRepSurface::CItrVertex
////////////////////////////////////////////////////////////////
/*
CBRepSurface::CItrVertex::CItrVertex()
{
    this->self = new Cad::CBRepSurface::CItrVertex(); // 明示的には実装されていない
}
*/

// 頂点周りのループを巡ることができるイテレータ
CBRepSurface::CItrVertex::CItrVertex(CBRepSurface^ ptr_cad_2d, unsigned int id_v)
{
    Cad::CBRepSurface *ptr_cad_2d_ = ptr_cad_2d->Self;
    this->self = new Cad::CBRepSurface::CItrVertex(*ptr_cad_2d_, id_v);
}

CBRepSurface::CItrVertex::CItrVertex(Cad::CBRepSurface::CItrVertex *self)
{
    this->self = self;
}

CBRepSurface::CItrVertex::~CItrVertex()
{
    this->!CItrVertex();
}

CBRepSurface::CItrVertex::!CItrVertex()
{
    delete this->self;
}

Cad::CBRepSurface::CItrVertex* CBRepSurface::CItrVertex::Self::get()
{
    return this->self;
}

// 反時計周りに頂点まわりをめぐる
void CBRepSurface::CItrVertex::operator++()
{
    this->self->operator++();
}

// ダミーのオペレータ(++と同じ働き)
void CBRepSurface::CItrVertex::operator++(int n)
{
    this->self->operator++(n);
}

bool CBRepSurface::CItrVertex::GetIdEdge_Behind([Out] unsigned int% id_e, [Out] bool% is_same_dir)
{
    unsigned int id_e_ = id_e;
    bool is_same_dir_ = is_same_dir;
    
    bool ret = this->self->GetIdEdge_Behind(id_e_, is_same_dir_);
    
    id_e = id_e_;
    is_same_dir = is_same_dir_;
    return ret ;
}

// 頂点周りの辺のIDと、その辺の始点がid_vと一致しているかどうか
bool CBRepSurface::CItrVertex::GetIdEdge_Ahead([Out] unsigned int% id_e, [Out] bool% is_same_dir)
{
    unsigned int id_e_ = id_e;
    bool is_same_dir_ = is_same_dir;
    
    bool ret = this->self->GetIdEdge_Ahead(id_e_, is_same_dir_);
    
    id_e = id_e_;
    is_same_dir = is_same_dir_;
    return ret ;
}

// ループのIDを得る
unsigned int CBRepSurface::CItrVertex::GetIdLoop() 
{
    return this->self->GetIdLoop();
}

void CBRepSurface::CItrVertex::Begin()
{
    this->self->Begin();
}

// 面周りの辺が一周したらtrueを返す
bool CBRepSurface::CItrVertex::IsEnd()
{
    return this->self->IsEnd();
}

unsigned int CBRepSurface::CItrVertex::GetIdHalfEdge()
{
    return this->self->GetIdHalfEdge();
}
unsigned int CBRepSurface::CItrVertex::GetIdUseVertex()
{
    return this->self->GetIdUseVertex();
}

unsigned int CBRepSurface::CItrVertex::CountEdge()
{
    return this->self->CountEdge();
}

bool CBRepSurface::CItrVertex::IsParent()
{
    return this->self->IsParent();
}

bool CBRepSurface::CItrVertex::IsSameUseLoop(CItrVertex^ itrl)
{
    Cad::CBRepSurface::CItrVertex* itrl_ = itrl->Self;
    return this->self->IsSameUseLoop(*itrl_);
}

////////////////////////////////////////////////////////////////
// CBRepSurface
////////////////////////////////////////////////////////////////

CBRepSurface::CBRepSurface()
{
    this->self = new Cad::CBRepSurface();  // 明示的には実装されていないがインスタンス化は可能(Cad::CBrepSerface Cad::CCadObj2D::m_Brep)
}

CBRepSurface::CBRepSurface(Cad::CBRepSurface *self)
{
    this->self = self;
}

CBRepSurface::~CBRepSurface()
{
    this->!CBRepSurface();
}

CBRepSurface::!CBRepSurface()
{
    delete this->self;
}

Cad::CBRepSurface * CBRepSurface::Self::get()
{
    return this->self;
}


bool CBRepSurface::AssertValid()
{
    return this->self->AssertValid();
}

bool CBRepSurface::IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE type, unsigned int id)
{
    Cad::CAD_ELEM_TYPE type_ = static_cast<Cad::CAD_ELEM_TYPE>(type);
    return this->self->IsElemID(type_, id);
}

IList<unsigned int>^ CBRepSurface::GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE type)
{
    Cad::CAD_ELEM_TYPE type_ = static_cast<Cad::CAD_ELEM_TYPE>(type);
    std::vector<unsigned int> vec;
    std::vector<unsigned int>::iterator itr;
    
    vec = this->self->GetAryElemID(type_);
    
    IList<unsigned int>^ list = gcnew List<unsigned int>();
    if (vec.size() > 0)
    {
        for (itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

bool CBRepSurface::GetIdLoop_Edge(unsigned int id_e, [Out] unsigned int% id_l_l, [Out] unsigned int% id_l_r)
{
    unsigned int id_l_l_ = id_l_l;
    unsigned int id_l_r_ = id_l_r;
    
    bool ret = this->self->GetIdLoop_Edge(id_e, id_l_l_, id_l_r_);
    
    id_l_l = id_l_l_;
    id_l_r = id_l_r_;
    
    return ret;
}

unsigned int CBRepSurface::GetIdLoop_Edge(unsigned int id_e, bool is_left)
{
    return this->self->GetIdLoop_Edge(id_e, is_left);
}

bool CBRepSurface::GetIdVertex_Edge(unsigned int id_e, [Out] unsigned int% id_v1, [Out] unsigned int% id_v2)
{
    unsigned int id_v1_ = id_v1;
    unsigned int id_v2_ = id_v2;
    
    bool ret = this->self->GetIdVertex_Edge(id_e, id_v1_, id_v2_);
    
    id_v1 = id_v1_;
    id_v2 = id_v2_;
    
    return ret;
}

unsigned int CBRepSurface::GetIdVertex_Edge(unsigned int id_e, bool is_root )
{
    return this->self->GetIdVertex_Edge(id_e, is_root);
}


CBRepSurface::CItrLoop^ CBRepSurface::GetItrLoop(unsigned int id_l) 
{
    Cad::CBRepSurface::CItrLoop itrlinstance = this->self->GetItrLoop(id_l);
    
    Cad::CBRepSurface::CItrLoop *itrl = new Cad::CBRepSurface::CItrLoop(itrlinstance);  //コピーコンストラクタでnewする
    CBRepSurface::CItrLoop^ itrlManaged = gcnew CBRepSurface::CItrLoop(itrl);
    return itrlManaged;

    /*
    return gcnew CBRepSurface::CItrLoop(this, id_l);
    */

    /*
    Cad::CBRepSurface::CItrLoop *itrl = new Cad::CBRepSurface::CItrLoop(); // 明示的には実装されていない
    
    *itrl = this->self->GetItrLoop(id_l);  // operator=は実装されていない

    CBRepSurface::CItrLoop^ itrlManaged = gcnew CBRepSurface::CItrLoop(itrl);

    return itrlManaged;
    */
}

CBRepSurface::CItrLoop^ CBRepSurface::GetItrLoop_SideEdge(unsigned int id_e, bool is_left)
{
    Cad::CBRepSurface::CItrLoop itrlinstance = this->self->GetItrLoop_SideEdge(id_e, is_left);
    
    Cad::CBRepSurface::CItrLoop *itrl = new Cad::CBRepSurface::CItrLoop(itrlinstance); // コピーコンストラクタでnewする
    CBRepSurface::CItrLoop^ itrlManaged = gcnew CBRepSurface::CItrLoop(itrl);
    
    return itrlManaged;
    
    /*
    Cad::CBRepSurface::CItrLoop *itrl = new Cad::CBRepSurface::CItrLoop(); // 明示的には実装されていない

    *itrl = this->self->GetItrLoop_SideEdge(id_e, is_left);
    
    CBRepSurface::CItrLoop^ itrlManaged = gcnew CBRepSurface::CItrLoop(itrl);
    
    return itrlManaged;
    */
}

CBRepSurface::CItrVertex^ CBRepSurface::GetItrVertex(unsigned int id_v)
{
    Cad::CBRepSurface::CItrVertex itrvinstance = this->self->GetItrVertex(id_v);

    Cad::CBRepSurface::CItrVertex *itrv = new Cad::CBRepSurface::CItrVertex(itrvinstance); // コピーコンストラクタでnewする
    CBRepSurface::CItrVertex^ retManaged = gcnew CBRepSurface::CItrVertex(itrv);
    
    return retManaged;

    /*
    return gcnew CBRepSurface::CItrVertex(this, id_v);
    */
    
    /*
    Cad::CBRepSurface::CItrVertex *itrv = new Cad::CBRepSurface::CItrVertex(); // 明示的には実装されていない

    *itrv = this->self->GetItrVertex(id_v);
    
    CBRepSurface::CItrVertex^ retManaged = gcnew CBRepSurface::CItrVertex(itrv);
    
    return retManaged;
    */
}

void CBRepSurface::Clear()
{
    this->self->Clear();
}

unsigned int CBRepSurface::AddVertex_Edge(unsigned int id_e)
{
    return this->self->AddVertex_Edge(id_e);
}

unsigned int CBRepSurface::AddVertex_Loop(unsigned int id_l)
{
    return this->self->AddVertex_Loop(id_l);
}


bool CBRepSurface::RemoveEdge(unsigned int id_e, bool is_del_cp)
{
    return this->self->RemoveEdge(id_e, is_del_cp);
}

bool CBRepSurface::RemoveVertex(unsigned int id_v)
{
    return this->self->RemoveVertex(id_v);
}

bool CBRepSurface::MakeHole_fromLoop(unsigned int id_l)
{
    return this->self->MakeHole_fromLoop(id_l);
}

unsigned int CBRepSurface::SealHole(unsigned int id_e, bool is_left)
{
    return this->self->SealHole(id_e, is_left);
}

CBRepSurface::CResConnectVertex^ CBRepSurface::ConnectVertex(CItrVertex^ itrv1, CItrVertex^ itrv2, bool is_id_l_add_left)
{
    Cad::CBRepSurface::CItrVertex *itrv1_ = itrv1->Self;
    Cad::CBRepSurface::CItrVertex *itrv2_ = itrv2->Self;
    
    const Cad::CBRepSurface::CResConnectVertex& ret_instance_ = this->self->ConnectVertex(*itrv1_, *itrv2_, is_id_l_add_left);
    Cad::CBRepSurface::CResConnectVertex *ret = new Cad::CBRepSurface::CResConnectVertex(ret_instance_);
    
    CResConnectVertex^ retManaged = gcnew CResConnectVertex(ret);
    
    return retManaged;
}
IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ CBRepSurface::GetItrLoop_ConnectVertex(CItrVertex^ itrv1, CItrVertex^ itrv2)
{
    Cad::CBRepSurface::CItrVertex *itrv1_ = itrv1->Self;
    Cad::CBRepSurface::CItrVertex *itrv2_ = itrv2->Self;
    
    const std::vector< std::pair<unsigned int, bool> >& vec = this->self->GetItrLoop_ConnectVertex(*itrv1_, *itrv2_);
    std::vector< std::pair<unsigned int, bool> >::const_iterator itr;
    
    IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ list = gcnew List< DelFEM4NetCom::Pair<unsigned int,bool>^ >();
    if (vec.size() > 0)
    {
        for (itr = vec.begin(); itr != vec.end(); itr++)
        {
            std::pair<unsigned int, bool> pair = *itr;
            unsigned int first = pair.first;
            bool second = pair.second;
            
            DelFEM4NetCom::Pair<unsigned int,bool>^ pairManaged = gcnew DelFEM4NetCom::Pair<unsigned int,bool>(first, second);
            list->Add(pairManaged);
        }
    }

    return list;
}

IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ CBRepSurface::GetItrLoop_RemoveEdge(unsigned int id_e)
{    
    const std::vector< std::pair<unsigned int, bool> >& vec = this->self->GetItrLoop_RemoveEdge(id_e);
    std::vector< std::pair<unsigned int, bool> >::const_iterator itr;
    
    IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ list = gcnew List< DelFEM4NetCom::Pair<unsigned int,bool>^ >();
    if (vec.size() > 0)
    {
        for (itr = vec.begin(); itr != vec.end(); itr++)
        {
            std::pair<unsigned int, bool> pair = *itr;
            unsigned int first = pair.first;
            bool second = pair.second;
            
            DelFEM4NetCom::Pair<unsigned int, bool>^ pairManaged = gcnew DelFEM4NetCom::Pair<unsigned int,bool>(first, second);
            list->Add(pairManaged);
        }
    }

    return list;
}

bool CBRepSurface::SwapItrLoop(CItrLoop^ itrl, unsigned int id_l_to )
{
    Cad::CBRepSurface::CItrLoop *itrl_ = itrl->Self;
    
    return this->self->SwapItrLoop(*itrl_, id_l_to);
}

bool CBRepSurface::Serialize( DelFEM4NetCom::CSerializer^ serialize )
{
    Com::CSerializer *serialize_ = serialize->Self;
    return this->self->Serialize(*serialize_);
}

