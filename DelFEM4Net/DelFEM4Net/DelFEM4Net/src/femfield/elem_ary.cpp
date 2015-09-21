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
// ElemAry.cpp: interface of element array class (CElemAry)
////////////////////////////////////////////////////////////////


#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
    #pragma warning ( disable : 4996 )
#endif
#define for if(0);else for

#include <fstream>
#include <vector>
#include <string>
#include <assert.h>
#include <algorithm>
#include <iostream>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/elem_ary.h"

using namespace DelFEM4NetFem::Field;


// FEM用の行列パターンを作る（非対角ブロック行列用）
bool CElemAry::MakePattern_FEM(
    unsigned int id_es0, unsigned int id_es1, DelFEM4NetCom::CIndexedArray^ crs )
{
    Com::CIndexedArray *crs_ = crs->Self;
    
    bool ret = this->self->MakePattern_FEM(id_es0, id_es1, *crs_);  // [IN/OUT] crs

    return ret;
}

// FEM用の行列パターンを作る（対角ブロック行列用）
bool CElemAry::MakePattern_FEM(
    int id_es0, DelFEM4NetCom::CIndexedArray^ crs )
{
    Com::CIndexedArray *crs_ = crs->Self;
    
    bool ret = this->self->MakePattern_FEM(id_es0, *crs_);  // [IN/OUT] crs

    return ret;
}

bool CElemAry::MakeEdge(unsigned int id_es_co, [Out] unsigned int% nedge, [Out] IList<unsigned int>^% edge_ary)
{
    unsigned int nedge_;
    std::vector<unsigned int> edge_ary_;
    
    bool ret = this->self->MakeEdge(id_es_co, nedge_, edge_ary_); //[OUT]nedge, edge_ary
    
    nedge = nedge_;
    edge_ary = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(edge_ary_);
    return ret;
}

bool CElemAry::MakeElemToEdge(unsigned int id_es_corner, 
        unsigned int nedge, IList<unsigned int>^ edge_ary,
        [Out] IList<int>^% el2ed )
{
    std::vector<unsigned int> edge_ary_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(edge_ary, edge_ary_);
    std::vector<int> el2ed_;
    
    bool ret= this->self->MakeElemToEdge(id_es_corner,
        nedge, edge_ary_,
        el2ed_) ; // [OUT]edge_ary
    
    el2ed = DelFEM4NetCom::ClrStub::VectorToList<int>(el2ed_);
    return ret;
}

CElemAry^ CElemAry::MakeBoundElemAry(unsigned int id_es_corner, [Out] unsigned int% id_es_add, [Out] IList<unsigned int>^ aIndElemFace)
{
    unsigned int id_es_add_;
    std::vector<unsigned int> aIndElemFace_;
    
    Fem::Field::CElemAry *ret;
    
    // new されたポインタが返ってくる
    ret = this->self->MakeBoundElemAry(id_es_corner, id_es_add_, aIndElemFace_);
    
    id_es_add = id_es_add_;
    aIndElemFace = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(aIndElemFace_);

    CElemAry^ retManaged = gcnew CElemAry(ret);
    return retManaged;
}


int CElemAry::InitializeFromFile(String^ file_name, long% offset)   //[IN/OUT] offset
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    long offset_ = offset;
    
    int ret = this->self->InitializeFromFile(file_name_, offset_);
    
    offset = offset_;
    
    return ret;
}

int CElemAry::WriteToFile(String^ file_name, long% offset, unsigned int id) //[IN/OUT] offset
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    long offset_ = offset;
    
    int ret = this->self->WriteToFile(file_name_, offset_, id);
    
    offset = offset_;
    
    return ret;
}

IList<int>^ CElemAry::AddSegment(IList< DelFEM4NetCom::Pair<unsigned int, CElemAry::CElemSeg^>^ >^ es_ary, IList<int>^ lnods )
{
    std::vector< std::pair<unsigned int, Fem::Field::CElemAry::CElemSeg> > es_ary_;
    std::vector<int> lnods_;
    
    if (es_ary->Count > 0)
    {
        for each (DelFEM4NetCom::Pair<unsigned int, CElemAry::CElemSeg^>^ pair in es_ary)
        {
            Fem::Field::CElemAry::CElemSeg elemSeg_Instance_ = *(pair->Second->Self);

            std::pair<unsigned int, Fem::Field::CElemAry::CElemSeg> pair_(pair->First, elemSeg_Instance_);            

            es_ary_.push_back(pair_);
        }
    }
    DelFEM4NetCom::ClrStub::ListToVector<int>(lnods, lnods_);
    
    std::vector<int> vec;
    
    vec = this->self->AddSegment(es_ary_, lnods_);
    
    IList<int>^ list = DelFEM4NetCom::ClrStub::VectorToList<int>(vec);
    return list;
}

int CElemAry::AddSegment(unsigned int id, CElemAry::CElemSeg^ es, IList<int>^ lnods)
{
    Fem::Field::CElemAry::CElemSeg *es_ = es->Self;
    std::vector<int> lnods_;
    DelFEM4NetCom::ClrStub::ListToVector<int>(lnods, lnods_);
    
    int ret = this->self->AddSegment(id, *es_, lnods_);
    
    return ret;
}

bool CElemAry::MakeElemSurElem(int id_es_corner, array<int>^ elsuel)
{
    pin_ptr<int> ptr = &elsuel[0];
    int * elsuel_ = (int *)ptr;
    
    bool ret = this->self->MakeElemSurElem(id_es_corner, elsuel_);

    return ret;
}

