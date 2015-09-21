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
// implementation of field administration class (CFieldWorld)
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
    #pragma warning ( disable : 4996 )
#endif

#include <fstream>
#include <iostream>
#include <vector>
#include <string>
#include <assert.h>
#include <set>

#include "DelFEM4Net/stub/clr_stub.h"
//#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/elem_ary.h"
#include "DelFEM4Net/mesh_interface.h"


using namespace DelFEM4NetFem::Field;

////////////////////////////////////////////////////////////////
// 生成/消滅
////////////////////////////////////////////////////////////////

CFieldWorld::CFieldWorld()
{
    this->self = new Fem::Field::CFieldWorld();
}

CFieldWorld::CFieldWorld(const CFieldWorld% rhs)
{
    const Fem::Field::CFieldWorld& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Field::CFieldWorld(rhs_instance_);
}

CFieldWorld::CFieldWorld(Fem::Field::CFieldWorld *self)
{
    this->self = self;
}

CFieldWorld::~CFieldWorld()
{
    this->!CFieldWorld();
}

CFieldWorld::!CFieldWorld()
{
    delete this->self;
}

Fem::Field::CFieldWorld * CFieldWorld::Self::get()
{
    return this->self;
}

CFieldWorld^ CFieldWorld::Clone()
{
    return gcnew CFieldWorld(*this);
}

unsigned int CFieldWorld::AddMesh(DelFEM4NetMsh::IMesh^ mesh)
{
    Msh::IMesh *mesh_ = mesh->MshSelf;
    
    return this->self->AddMesh(*mesh_);
}


unsigned int CFieldWorld::SetCustomBaseField(unsigned int id_base,
    IList<unsigned int>^ aIdEA_Inc,
    IList< IList<int>^ >^ aLnods,
    IList<unsigned int>^ mapVal2Co)
{
    std::vector<unsigned int> aIdEA_Inc_;
    std::vector<unsigned int> mapVal2Co_;
    
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA_Inc, aIdEA_Inc_);

    std::vector<std::vector<int>> aLnods_;
    if (aLnods->Count > 0)
    {
        for each (IList<int>^ list in aLnods)
        {
            // なぜかIListで渡すとコンパイルが異常終了する
            // リストに戻す
            List<int>^ list2 = gcnew List<int>(list);
            std::vector<int> vec;
            DelFEM4NetCom::ClrStub::ListToVector<int>(list2, vec);

            aLnods_.push_back(vec);
        }
    }
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(mapVal2Co, mapVal2Co_);
    
    return this->self->SetCustomBaseField(id_base,
        aIdEA_Inc_,
        aLnods_,
        mapVal2Co_);
}

CIDConvEAMshCad^ CFieldWorld::GetIDConverter(unsigned int id_field_base)
{
    const Fem::Field::CIDConvEAMshCad& ret_instance_ = this->self->GetIDConverter(id_field_base);
    Fem::Field::CIDConvEAMshCad *ret = new Fem::Field::CIDConvEAMshCad(ret_instance_);
    
    CIDConvEAMshCad^ retManaged = gcnew CIDConvEAMshCad(ret);
    return retManaged;
}

void CFieldWorld::Clear()
{
    this->self->Clear();
}

bool CFieldWorld::IsIdEA( unsigned int id_ea )
{
    return this->self->IsIdEA(id_ea);
}

IList<unsigned int>^ CFieldWorld::GetAry_IdEA()
{
    const std::vector<unsigned int>& vec = this->self->GetAry_IdEA();
    IList<unsigned int>^ list = gcnew List<unsigned int>();
    
    for (std::vector<unsigned int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
    {
        list->Add(*itr);
    }
    return list;
}

CElemAry^ CFieldWorld::GetEA(unsigned int id_ea)
{
    const Fem::Field::CElemAry& ret_instance_ = this->self->GetEA(id_ea);
    Fem::Field::CElemAry *ret = new Fem::Field::CElemAry(ret_instance_);
    
    CElemAry^ retManaged = gcnew CElemAry(ret);
    
    return retManaged;
}

/// <summary>
/// 要素アレイを書き込みアクセス用に取得する
/// </summary>
/// <param name="id_ea">要素アレイID</param>
/// <returns></returns>
CElemAryPtr^ CFieldWorld::GetEAPtr(unsigned int id_ea)
{
    Fem::Field::CElemAry& ret_instance_ = this->self->GetEA(id_ea);
    Fem::Field::CElemAry* ret_ptr = &ret_instance_;
    
    CElemAryPtr^ retManaged = gcnew CElemAryPtr(ret_ptr);

    return retManaged;
}

unsigned int CFieldWorld::AddElemAry(unsigned int size, DelFEM4NetFem::Field::ELEM_TYPE elem_type)
{
    Fem::Field::ELEM_TYPE elem_type_ = static_cast<Fem::Field::ELEM_TYPE>(elem_type);
    
    return this->self->AddElemAry(size, elem_type_);
}

/*nativeのライブラリで実装されていない？
bool CFieldWorld::AddIncludeRelation(unsigned int id_ea, unsigned int id_ea_inc)
{
    return this->self->AddIncludeRelation(id_ea, id_ea_inc);
}
*/

bool CFieldWorld::IsIdNA( unsigned int id_na )
{
    return this->self->IsIdNA(id_na);
}

IList<unsigned int>^ CFieldWorld::GetAry_IdNA()
{
    const std::vector<unsigned int>& vec = this->self->GetAry_IdNA();
    
    IList<unsigned int>^ list = gcnew List<unsigned int>();
    
    if (vec.size() > 0)
    {
        for (std::vector<unsigned int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

CNodeAry^ CFieldWorld::GetNA(unsigned int id_na)
{
    const Fem::Field::CNodeAry& ret_instance_ = this->self->GetNA(id_na);
    Fem::Field::CNodeAry *ret = new Fem::Field::CNodeAry(ret_instance_);  // copyコンストラクタを使用
    
    CNodeAry^ retManaged = gcnew CNodeAry(ret);
    return retManaged;
}

/// <summary>
/// 節点アレイを書き込みアクセス用に取得する
/// </summary>
/// <param name="id_ea">節点アレイID</param>
/// <returns></returns>
CNodeAryPtr^ CFieldWorld::GetNAPtr(unsigned int id_na)
{
    Fem::Field::CNodeAry& ret_instance_ = this->self->GetNA(id_na);
    Fem::Field::CNodeAry* ret_ptr = &ret_instance_;
    
    CNodeAryPtr^ retManaged = gcnew CNodeAryPtr(ret_ptr);

    return retManaged;
}

unsigned int CFieldWorld::AddNodeAry(unsigned int size)
{
    return this->self->AddNodeAry(size);
}

bool CFieldWorld::IsIdField( unsigned int id_field )
{
    return this->self->IsIdField(id_field);
}

IList<unsigned int>^ CFieldWorld::GetAry_IdField()
{
    const std::vector<unsigned int>& vec = this->self->GetAry_IdField();
    
    IList<unsigned int>^ list = gcnew List<unsigned int>();
    if (vec.size() > 0)
    {
        for (std::vector<unsigned int>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
        {
            list->Add(*itr);
        }
    }
    return list;
}

CField^ CFieldWorld::GetField(unsigned int id_field)
{
    const Fem::Field::CField& ret_instance_ = this->self->GetField(id_field);
    Fem::Field::CField *ret = new Fem::Field::CField(ret_instance_);
    
    CField^ retManaged = gcnew CField(ret);
    
    return retManaged;
}

/// <summary>
/// フィールドを書き込みアクセス用に取得する
/// </summary>
/// <param name="id_ea">フィールドID</param>
/// <returns></returns>
CFieldPtr^ CFieldWorld::GetFieldPtr(unsigned int id_field)
{
    Fem::Field::CField& ret_instance_ = this->self->GetField(id_field);
    Fem::Field::CField* ret_ptr = &ret_instance_;
    
    CFieldPtr^ retManaged = gcnew CFieldPtr(ret_ptr);

    return retManaged;
}

unsigned int CFieldWorld::MakeField_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type, /*int*/DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type)
{
    Fem::Field::FIELD_TYPE field_type_ = static_cast<Fem::Field::FIELD_TYPE>(field_type);
    int derivative_type_ = (int)derivative_type;
    int node_configuration_type_ = (int)node_configuration_type;
    return this->self->MakeField_FieldElemAry(id_field, id_ea, field_type_, derivative_type_, node_configuration_type_);
}

unsigned int CFieldWorld::MakeField_FieldElemDim(unsigned int id_field, int idim_elem, DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type, /*int*/DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type)
{
    Fem::Field::FIELD_TYPE field_type_ = static_cast<Fem::Field::FIELD_TYPE>(field_type);
    int derivative_type_ = (int)derivative_type;
    int node_configuration_type_ = (int)node_configuration_type;
    return this->self->MakeField_FieldElemDim(id_field, idim_elem, field_type_, derivative_type_, node_configuration_type_);
}

unsigned int CFieldWorld::AddField(unsigned int id_field_parent,    // parent field
    IList<CField::CElemInterpolation^>^ aEI, 
    CField::CNodeSegInNodeAry^ nsna_c, CField::CNodeSegInNodeAry^ nsna_b,
    unsigned int id_field_candidate)
{
    std::vector<Fem::Field::CField::CElemInterpolation> aEI_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CField::CElemInterpolation, Fem::Field::CField::CElemInterpolation>(aEI, aEI_);
    Fem::Field::CField::CNodeSegInNodeAry *nsna_c_ = nsna_c->Self;
    Fem::Field::CField::CNodeSegInNodeAry *nsna_b_ = nsna_b->Self;
    
    return this->self->AddField(id_field_parent,
        aEI_, *nsna_c_, *nsna_b_,
        id_field_candidate);
}

unsigned int CFieldWorld::GetPartialField(unsigned int id_field, unsigned int IdEA )
{
    return this->self->GetPartialField(id_field, IdEA);
}

unsigned int CFieldWorld::GetPartialField(unsigned int id_field, IList<unsigned int>^ aIdEA)
{
    std::vector<unsigned int> aIdEA_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA, aIdEA_);
    
    return this->self->GetPartialField(id_field, aIdEA_);
}

bool CFieldWorld::UpdateMeshCoord(unsigned int id_base, DelFEM4NetMsh::IMesh^ mesh)
{
    //Msh::IMesh *mesh_ = mesh->Self;
    Msh::IMesh *mesh_ = mesh->MshSelf;
    
    return this->self->UpdateMeshCoord(id_base, *mesh_);
}

bool CFieldWorld::UpdateMeshCoord(unsigned int id_base, unsigned int id_field_disp, DelFEM4NetMsh::IMesh^ mesh)
{
    //Msh::IMesh *mesh_ = mesh->Self;
    Msh::IMesh *mesh_ = mesh->MshSelf;
    
    return this->self->UpdateMeshCoord(id_base, id_field_disp, *mesh_);
}

bool CFieldWorld::UpdateConnectivity(unsigned int id_base, DelFEM4NetMsh::IMesh^ mesh)
{
    //Msh::IMesh *mesh_ = mesh->Self;
    Msh::IMesh *mesh_ = mesh->MshSelf;

    return this->self->UpdateConnectivity(id_base, *mesh_);
}

bool CFieldWorld::UpdateConnectivity_CustomBaseField(unsigned int id_base,
        IList<unsigned int>^ aIdEA_Inc, 
        IList< IList<int>^ >^ aLnods,
        IList<unsigned int>^ mapVal2Co)
{
    std::vector<unsigned int> aIdEA_Inc_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdEA_Inc, aIdEA_Inc_);

    std::vector<std::vector<int>> aLnods_;
    if (aLnods->Count > 0)
    {
        for each (IList<int>^ list in aLnods)
        {
            // なぜかIListで渡すとコンパイルが異常終了する
            // リストに戻す
            List<int>^ list2 = gcnew List<int>(list);
            std::vector<int> vec;
            DelFEM4NetCom::ClrStub::ListToVector<int>(list2, vec);

            aLnods_.push_back(vec);
        }
    }
    std::vector<unsigned int> mapVal2Co_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(mapVal2Co, mapVal2Co_);

    return this->self->UpdateConnectivity_CustomBaseField(id_base,
        aIdEA_Inc_,
        aLnods_,
        mapVal2Co_);
}

bool CFieldWorld::UpdateConnectivity_HingeField_Tri(unsigned int id_field, unsigned int id_field_base)
{
    return this->self->UpdateConnectivity_HingeField_Tri(id_field, id_field_base);
}

bool CFieldWorld::UpdateConnectivity_EdgeField_Tri( unsigned int id_field, unsigned int id_field_base)
{
    return this->self->UpdateConnectivity_EdgeField_Tri(id_field, id_field_base);
}

void CFieldWorld::DeleteField(IList<unsigned int>^ aIdFieldDel )
{
    std::vector<unsigned int> aIdFieldDel_;
    DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdFieldDel, aIdFieldDel_);
    
    this->self->DeleteField(aIdFieldDel_);
}
