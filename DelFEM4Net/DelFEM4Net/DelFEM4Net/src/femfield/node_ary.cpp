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
// NodeAry.cpp : 節点配列クラス(NodeAry)の実装
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
    #pragma warning ( disable : 4996 )
#endif

#include <iostream>
#include <vector>
#include <string>
#include <stdio.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/node_ary.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/zvector_blk.h"

using namespace DelFEM4NetFem::Field;
using namespace DelFEM4NetMatVec;

//////////////////////////////////////////////////////////////////////
// CNodeAry
//////////////////////////////////////////////////////////////////////

bool CNodeAry::ClearSegment()
{
    return this->self->ClearSegment();
}

bool CNodeAry::GetValueFromNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset)  // vec: IN/OUT
{
    return this->self->GetValueFromNodeSegment(id_ns, *(vec->Self), ioffset);
}

bool CNodeAry::AddValueFromNodeSegment(double alpha, unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset) // vec: IN/OUT
{
    return this->self->AddValueFromNodeSegment(alpha, id_ns, *(vec->Self), ioffset);
}

//////////////////////////////////////////////////////////////////////
// 参照要素セグメント追加メソッド

void CNodeAry::AddEaEs(DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes)
{
    std::pair<unsigned int, unsigned int> eaes_;
    eaes_.first = eaes->First;
    eaes_.second = eaes->Second;
    
    this->self->AddEaEs(eaes_);
}

IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ CNodeAry::GetAryEaEs()
{
    const std::vector< std::pair<unsigned int, unsigned int> >& vec = this->self->GetAryEaEs();
    
    IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ list = gcnew List< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >();
    if (vec.size() > 0)
    {
        for (std::vector< std::pair<unsigned int, unsigned int> >::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            const std::pair<unsigned int, unsigned int>& pair_ = *itr;
            DelFEM4NetCom::Pair<unsigned int, unsigned int>^ pair = gcnew DelFEM4NetCom::Pair<unsigned int, unsigned int>();
            pair->First = pair_.first;
            pair->Second = pair_.second;
            
            list->Add(pair);
        }
    }
    return list;
}

void CNodeAry::SetIncludeEaEs_InEaEs( DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_included,
                                      DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_container )
{
    std::pair<unsigned int, unsigned int> eaes_included_(eaes_included->First, eaes_included->Second);
    std::pair<unsigned int, unsigned int> eaes_container_(eaes_container->First, eaes_container->Second);
    
    this->self->SetIncludeEaEs_InEaEs(eaes_included_, eaes_container_);
}

bool CNodeAry::IsIncludeEaEs_InEaEs( DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_inc,
                                      DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_in )
{
    std::pair<unsigned int, unsigned int> eaes_inc_(eaes_inc->First, eaes_inc->Second);
    std::pair<unsigned int, unsigned int> eaes_in_(eaes_in->First, eaes_in->Second);
    
    return this->self->IsIncludeEaEs_InEaEs(eaes_inc_, eaes_in_);
}

IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ CNodeAry::GetAry_EaEs_Min()
{
    const std::vector< std::pair<unsigned int, unsigned int> >& vec = this->self->GetAry_EaEs_Min();
    
    IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ list = gcnew List< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >();
    if (vec.size() > 0)
    {
        for (std::vector< std::pair<unsigned int, unsigned int>>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            const std::pair<unsigned int, unsigned int>& pair_ = *itr;
            DelFEM4NetCom::Pair<unsigned int, unsigned int>^ pair = gcnew DelFEM4NetCom::Pair<unsigned int, unsigned int>();
            pair->First = pair_.first;
            pair->Second = pair_.second;
            
            list->Add(pair);
        }
    }
    return list;
}

 unsigned int CNodeAry::IsContainEa_InEaEs(DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes, unsigned int id_ea)
 {
    std::pair<unsigned int, unsigned int> eaes_(eaes->First, eaes->Second);
    return this->self->IsContainEa_InEaEs(eaes_, id_ea);
 }


//////////////////////////////////////////////////////////////////////
// 変更・削除・追加メソッド
IList<int>^ CNodeAry::AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec )
{
    std::vector< std::pair<unsigned int,Fem::Field::CNodeAry::CNodeSeg> > seg_vec_;
    
    if (seg_vec->Count > 0)
    {
        for each(DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ pair in seg_vec)
        {
            std::pair<unsigned int, Fem::Field::CNodeAry::CNodeSeg> pair_( pair->First, *(pair->Second->Self) );
            seg_vec_.push_back(pair_);
        }
    }

    const std::vector<int>& vec = this->self->AddSegment(seg_vec_);

    IList<int>^ list = gcnew List<int>();
    if (vec.size() > 0)
    {
        for (std::vector<int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

IList<int>^ CNodeAry::AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec , double val)
{
    std::vector< std::pair<unsigned int, Fem::Field::CNodeAry::CNodeSeg> > seg_vec_;
    
    if (seg_vec->Count > 0)
    {
        for each(DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ pair in seg_vec)
        {
            std::pair<unsigned int, Fem::Field::CNodeAry::CNodeSeg> pair_( pair->First, *(pair->Second->Self) );
            seg_vec_.push_back(pair_);
        }
    }

    const std::vector<int>& vec = this->self->AddSegment(seg_vec_, val);

    IList<int>^ list = gcnew List<int>();
    if (vec.size() > 0)
    {
        for (std::vector<int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

IList<int>^ CNodeAry::AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec, IList<double>^ val_vec)
{
    std::vector< std::pair<unsigned int, Fem::Field::CNodeAry::CNodeSeg> > seg_vec_;
    std::vector<double> val_vec_;
    
    if (seg_vec->Count > 0)
    {
        for each(DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ pair in seg_vec)
        {
            std::pair<unsigned int, Fem::Field::CNodeAry::CNodeSeg> pair_( pair->First, *(pair->Second->Self) );
            seg_vec_.push_back(pair_);
        }
    }
    DelFEM4NetCom::ClrStub::ListToVector<double>(val_vec, val_vec_);

    const std::vector<int>& vec = this->self->AddSegment(seg_vec_, val_vec_);

    IList<int>^ list = gcnew List<int>();
    if (vec.size() > 0)
    {
        for (std::vector<int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

////////////////////////////////////////////////////////////////////////////////
// 値の変更メソッド

bool CNodeAry::SetValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset)
{
    return this->self->SetValueToNodeSegment(id_ns, *(vec->Self), ioffset);
}


bool CNodeAry::AddValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, double alpha,  unsigned int ioffset)
{
    return this->self->AddValueToNodeSegment(id_ns, *(vec->Self), alpha, ioffset);
}

bool CNodeAry::AddValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CZVector_Blk^ vec, double alpha)
{
    return this->self->AddValueToNodeSegment(id_ns, *(vec->Self), alpha);
}

bool CNodeAry::AddValueToNodeSegment(unsigned int id_ns_to, unsigned int id_ns_from, double alpha )
{
    return this->self->AddValueToNodeSegment(id_ns_to, id_ns_from, alpha);
}


int CNodeAry::InitializeFromFile(String^ file_name, long% offset)
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    long offset_ = offset;
    
    int ret =  this->self->InitializeFromFile(file_name_, offset_);

    offset = offset_;
    return ret;
}

int CNodeAry::WriteToFile(String^ file_name, long% offset, unsigned int id )
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    long offset_ = offset;
    
    int ret =  this->self->WriteToFile(file_name_, offset_, id);

    offset = offset_;

    return ret;
}

/*nativeのライブラリで実装されていない？
int CNodeAry::DumpToFile_UpdatedValue(String^ file_name, long% offset, unsigned int id )
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    long offset_ = offset;
    
    int ret =  this->self->DumpToFile_UpdatedValue(file_name_, offset_, id);

    offset = offset_;

    return ret;
}
*/
