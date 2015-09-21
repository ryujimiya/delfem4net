/*
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

#ifdef __VISUALC__
	#pragma warning(disable:4786)
	#pragma warning(disable:4996)
#endif

#include <set>
#include <iostream>
#include <assert.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/cad/brep.h"

using namespace DelFEM4NetCad;

////////////////////////////////////////////////////////////////////////////////
// CBRep
////////////////////////////////////////////////////////////////////////////////

CBRep::CBRep()
{
    this->self = new Cad::CBRep();  // 明示的には実装されていないがインスタンス化は可能
}

CBRep::CBRep(const CBRep% rhs)
{
    this->self = new Cad::CBRep(*(rhs.self));
}

CBRep::CBRep(Cad::CBRep *self)
{
    this->self = self;
}

CBRep::~CBRep()
{
    this->!CBRep();
}

CBRep::!CBRep()
{
    delete this->self;
}

Cad::CBRep * CBRep::Self::get()
{
    return this->self;
}


void CBRep::Clear()
{
    this->self->Clear();
}

bool CBRep::IsID_UseLoop(unsigned int id_ul)
{
    return this->self->IsID_UseLoop(id_ul);
}

bool CBRep::IsID_HalfEdge(unsigned int id_he)
{
    return this->self->IsID_HalfEdge(id_he);
}

bool CBRep::IsID_UseVertex(unsigned int id_uv)
{
    return this->self->IsID_UseVertex(id_uv);
}

IList<unsigned int>^ CBRep::GetAry_UseVertexID()
{
    const std::vector<unsigned int>& vec = this->self->GetAry_UseVertexID();

    IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
    return list;
}

CUseLoop^ CBRep::GetUseLoop(unsigned int id_ul)
{
    const Cad::CUseLoop& ret_instance_ = this->self->GetUseLoop(id_ul);
    Cad::CUseLoop *ret = new Cad::CUseLoop(ret_instance_);
    
    return gcnew CUseLoop(ret);
}

CUseVertex^ CBRep::GetUseVertex(unsigned int id_uv)
{
    const Cad::CUseVertex& ret_instance_ = this->self->GetUseVertex(id_uv);
    Cad::CUseVertex *ret = new Cad::CUseVertex(ret_instance_);
    
    return gcnew CUseVertex(ret);
}

CHalfEdge^ CBRep::GetHalfEdge(unsigned int id_he)
{
    const Cad::CHalfEdge& ret_instance_ = this->self->GetHalfEdge(id_he);
    Cad::CHalfEdge *ret = new Cad::CHalfEdge(ret_instance_);
    
    return gcnew CHalfEdge(ret);
}

bool CBRep::SetLoopIDtoUseLoop(unsigned int id_ul, unsigned int id_l)
{
    return this->self->SetLoopIDtoUseLoop(id_ul, id_l);
}

bool CBRep::SetVertexIDtoUseVertex(unsigned int id_uv, unsigned int id_v)
{
    return this->self->SetVertexIDtoUseVertex(id_uv, id_v);
}

bool CBRep::SetEdgeIDtoHalfEdge(unsigned int id_he, unsigned int id_e, bool is_same_dir)
{
    return this->self->SetEdgeIDtoHalfEdge(id_he, id_e, is_same_dir);
}

int CBRep::AssertValid_Use()
{
    return this->self->AssertValid_Use();
}

IList<unsigned int>^ CBRep::FindHalfEdge_Edge(unsigned int id_e)
{
    const std::vector<unsigned int>& vec = this->self->FindHalfEdge_Edge(id_e);
    
    return DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
}

IList<unsigned int>^ CBRep::FindHalfEdge_Vertex(unsigned int id_v)
{
    const std::vector<unsigned int>& vec = this->self->FindHalfEdge_Vertex(id_v);
    
    return DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
}

////////////////////////////////
// オイラー操作

bool CBRep::MEVVL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2,
                  [Out] unsigned int% id_uv_add1, [Out] unsigned int% id_uv_add2, [Out] unsigned int% id_ul_add )
{
    unsigned int id_he_add1_;
    unsigned int id_he_add2_;
    unsigned int id_uv_add1_;
    unsigned int id_uv_add2_;
    unsigned int id_ul_add_;
    
    bool ret = this->self->MEVVL(id_he_add1_, id_he_add2_, id_uv_add1_, id_uv_add2_, id_ul_add_);
    
    id_he_add1 = id_he_add1_;
    id_he_add2 = id_he_add2_;
    id_uv_add1 = id_uv_add1_;
    id_uv_add2 = id_uv_add2_;
    id_ul_add = id_ul_add_;
    
    return ret;
}

bool CBRep::MEL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_ul_add,
                unsigned int id_he1, unsigned int id_he2)
{
    unsigned int id_he_add1_;
    unsigned int id_he_add2_;
    unsigned int id_ul_add_;
    
    bool ret = this->self->MEL(id_he_add1_, id_he_add2_, id_ul_add_, id_he1, id_he2);
    
    id_he_add1 = id_he_add1_;
    id_he_add2 = id_he_add2_;
    id_ul_add = id_ul_add_;
    
    return ret;
}

bool CBRep::KEL(unsigned int id_he_rem1)
{
    return this->self->KEL(id_he_rem1);
}

bool CBRep::MEV([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_uv_add, 
                unsigned int id_he)
{
    unsigned int id_he_add1_;
    unsigned int id_he_add2_;
    unsigned int id_uv_add_;
    
    bool ret = this->self->MEV(id_he_add1_, id_he_add2_, id_uv_add_, id_he);
    
    id_he_add1 = id_he_add1_;
    id_he_add2 = id_he_add2_;
    id_uv_add = id_uv_add_;
    
    return ret;
}

bool CBRep::KVE(unsigned int id_he_rem1 )
{
    unsigned id_he_rem1_ = id_he_rem1;
    
    bool ret = this->self->KVE(id_he_rem1_);  // natveライブラリではbool CBRep::KVE( unsigned int& id_he1 )となっているが id_he1は入力のみで const 抜けと思われる

    return ret;
}

bool CBRep::MVE([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_uv_add, 
         unsigned int id_he)
{
    unsigned int id_he_add1_;
    unsigned int id_he_add2_;
    unsigned int id_uv_add_;
    
    bool ret = this->self->MVE(id_he_add1_, id_he_add2_, id_uv_add_, id_he);
    
    id_he_add1 = id_he_add1_;
    id_he_add2 = id_he_add2_;
    id_uv_add = id_uv_add_;
    
    return ret;
}

bool CBRep::MEKL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2,  
          unsigned int id_he1, unsigned int id_he2 )
{
    unsigned int id_he_add1_;
    unsigned int id_he_add2_;
    
    bool ret = this->self->MEKL(id_he_add1_, id_he_add2_, id_he1, id_he2);
    
    id_he_add1 = id_he_add1_;
    id_he_add2 = id_he_add2_;
    
    return ret;
}

bool CBRep::KEML([Out] unsigned int% id_ul_add1,
                 unsigned int id_he1 )
{
    unsigned int id_ul_add1_;
    unsigned int id_he1_;
    
    bool ret = this->self->KEML(id_ul_add1_, id_he1_);
    
    id_ul_add1 = id_ul_add1_;
    id_he1 = id_he1_;
    
    return ret;
}

bool CBRep::MEKL_OneFloatingVertex([Out] unsigned int% id_he_add1,
                            unsigned int id_he1, unsigned int id_he2)
{
    unsigned int id_he_add1_;
    
    bool ret = this->self->MEKL_OneFloatingVertex(id_he_add1_, id_he1, id_he2);
    
    id_he_add1 = id_he_add1_;
    
    return ret;
}

bool CBRep::MEKL_TwoFloatingVertex(unsigned int id_he1, unsigned int id_he2)
{
    return this->self->MEKL_TwoFloatingVertex(id_he1, id_he2);
}

bool CBRep::KEML_OneFloatingVertex([Out] unsigned int% id_ul_add, 
                            unsigned int id_he1)
{
    unsigned int id_ul_add_;
    
    bool ret = this->self->KEML_OneFloatingVertex(id_ul_add_, id_he1);
    
    id_ul_add = id_ul_add_;
    
    return ret;
}

bool CBRep::KEML_TwoFloatingVertex([Out] unsigned int% id_ul_add,
                            unsigned int id_he1)
{
    unsigned int id_ul_add_;
    
    bool ret = this->self->KEML_TwoFloatingVertex(id_ul_add_, id_he1);
    
    id_ul_add = id_ul_add_;
    
    return ret;
}

bool CBRep::MVEL([Out] unsigned int% id_uv_add, [Out] unsigned int% id_he_add, [Out] unsigned int% id_ul_add, 
          unsigned int id_ul_p)
{
    unsigned int id_uv_add_;
    unsigned int id_he_add_;
    unsigned int id_ul_add_;
    
    bool ret = this->self->MVEL(id_uv_add_, id_he_add_, id_ul_add_, id_ul_p);
    
    id_uv_add = id_uv_add_;
    id_he_add = id_he_add_;
    id_ul_add = id_ul_add_;
    
    return ret;
}

bool CBRep::KVEL(unsigned int id_uv_rem)
{
    return this->self->KVEL(id_uv_rem);
}

bool CBRep::SwapUseLoop(unsigned int id_ul1, unsigned int id_ul2)
{
    return this->self->SwapUseLoop(id_ul1, id_ul2);
}

bool CBRep::MoveUseLoop(unsigned int id_ul1, unsigned int id_ul2)
{
    return this->self->MoveUseLoop(id_ul1, id_ul2);
}

