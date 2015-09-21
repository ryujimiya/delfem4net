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
// Field.cpp：場クラス(CField)の実装
////////////////////////////////////////////////////////////////


#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#define for if(0);else for

#include <math.h>
#include <fstream>
#include <stdio.h>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"
#include "DelFEM4Net/eval.h"
//#include "DelFEM4Net/femeqn/ker_emat_hex.h"
//#include "DelFEM4Net/femeqn/ker_emat_tet.h"

using namespace DelFEM4NetFem::Field;
using namespace DelFEM4NetMatVec;


CField::CField()
{
    this->self = new Fem::Field::CField();
}

CField::CField(bool isCreateInstance)
{
    assert(isCreateInstance == false);
    // インスタンスを生成しない
    this->self = NULL;
}

CField::CField(const CField% rhs)
{
    const Fem::Field::CField& rhs_instance_ = *(rhs.self);
    this->self = new Fem::Field::CField(rhs_instance_);
}

CField::CField(Fem::Field::CField *self)
{
    this->self = self;
}

CField::CField(unsigned int id_field_parent,    // parent field
    IList<CField::CElemInterpolation^>^ aEI, 
    CField::CNodeSegInNodeAry^ nsna_c, CField::CNodeSegInNodeAry^ nsna_b, 
    CFieldWorld^ world)
{
    std::vector<Fem::Field::CField::CElemInterpolation> aEI_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CField::CElemInterpolation, Fem::Field::CField::CElemInterpolation>(aEI, aEI_);
    Fem::Field::CField::CNodeSegInNodeAry *nsna_c_ = nsna_c->Self;
    Fem::Field::CField::CNodeSegInNodeAry *nsna_b_ = nsna_b->Self;
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    this->self = new Fem::Field::CField(id_field_parent,
        aEI_,
        *nsna_c_, *nsna_b_,
        *world_);
}

CField::~CField()
{
    this->!CField();
}

CField::!CField()
{
    delete this->self;
}

Fem::Field::CField * CField::Self::get()
{
    return this->self;
}

bool CField::IsValid()
{
    return this->self->IsValid();
}

bool CField::AssertValid(CFieldWorld^ world)
{
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    return this->self->AssertValid(*world_);
}

unsigned int CField::GetNDimCoord()
{
    return this->self->GetNDimCoord();
}

unsigned int CField::GetNLenValue()
{
    return this->self->GetNLenValue();
}

DelFEM4NetFem::Field::FIELD_TYPE CField::GetFieldType()
{
    Fem::Field::FIELD_TYPE ret = this->self->GetFieldType();
    
    return static_cast<DelFEM4NetFem::Field::FIELD_TYPE>(ret);
}

unsigned int CField::GetFieldDerivativeType()
{
    return this->self->GetFieldDerivativeType();
}


bool CField::IsPartial()
{
    return this->self->IsPartial();
}

unsigned int CField::GetIDFieldParent()
{
    return this->self->GetIDFieldParent();
}

IList<unsigned int>^ CField::GetAryIdEA()
{
    const std::vector<unsigned int>& vec = this->self->GetAryIdEA();
    
    IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
    return list;
}

DelFEM4NetFem::Field::INTERPOLATION_TYPE CField::GetInterpolationType(unsigned int id_ea, CFieldWorld^ world)
{
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    Fem::Field::INTERPOLATION_TYPE ret = this->self->GetInterpolationType(id_ea, *world_);
    
    return static_cast<DelFEM4NetFem::Field::INTERPOLATION_TYPE>(ret);
}


CElemAry::CElemSeg^ CField::GetElemSeg(unsigned int id_ea, ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world )
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    const Fem::Field::CElemAry::CElemSeg& ret_instance_ = this->self->GetElemSeg(id_ea, elseg_type_, is_value, *world_);
    Fem::Field::CElemAry::CElemSeg *ret = new Fem::Field::CElemAry::CElemSeg(ret_instance_);  // copy
    
    CElemAry::CElemSeg^ retManaged = gcnew CElemAry::CElemSeg(ret);

    return retManaged;
}

unsigned int CField::GetIdElemSeg(unsigned int id_ea, ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world )
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
    Fem::Field::CFieldWorld *world_ = world->Self;

    return this->self->GetIdElemSeg(id_ea, elseg_type_, is_value, *world_);
}

CNodeAry::CNodeSeg^ CField::GetNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world, /*unsigned int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type)
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
    Fem::Field::CFieldWorld *world_ = world->Self;
    unsigned int derivative_type_ = (unsigned int)derivative_type;
    
    const Fem::Field::CNodeAry::CNodeSeg& ret_instance_ = this->self->GetNodeSeg(elseg_type_, is_value, *world_, derivative_type_);
    Fem::Field::CNodeAry::CNodeSeg *ret = new Fem::Field::CNodeAry::CNodeSeg(ret_instance_); // copy
    
    CNodeAry::CNodeSeg^ retManaged = gcnew CNodeAry::CNodeSeg(ret);
    
    return retManaged;
}

bool CField::IsNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world, /*unsigned int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type)
{
    Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
    Fem::Field::CFieldWorld *world_ = world->Self;
    unsigned int derivative_type_ = (unsigned int)derivative_type;
    
    return this->self->IsNodeSeg(elseg_type_, is_value, *world_, derivative_type_);
}

CField::CNodeSegInNodeAry^ CField::GetNodeSegInNodeAry(DelFEM4NetFem::Field::ELSEG_TYPE es_type)
{
    Fem::Field::ELSEG_TYPE es_type_ = static_cast<Fem::Field::ELSEG_TYPE>(es_type);
    
    const Fem::Field::CField::CNodeSegInNodeAry& ret_instance_ = this->self->GetNodeSegInNodeAry(es_type_);
    Fem::Field::CField::CNodeSegInNodeAry *ret = new Fem::Field::CField::CNodeSegInNodeAry(ret_instance_); // copy
    
    CField::CNodeSegInNodeAry^ retManaged = gcnew CField::CNodeSegInNodeAry(ret);
    
    return retManaged;
}

void CField::GetMinMaxValue([Out] double% min, [Out] double% max, CFieldWorld^ world, 
                      unsigned int idof, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt)
{
    double min_;
    double max_;
    Fem::Field::CFieldWorld *world_ = world->Self;
    int fdt_ = (int)fdt;
    
    this->self->GetMinMaxValue(min_, max_, *world_, idof, fdt_);
    
    min = min_;
    max = max_;
}

bool CField::SetValueType( DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, CFieldWorld^ world)
{
    Fem::Field::FIELD_TYPE field_type_ = static_cast<Fem::Field::FIELD_TYPE>(field_type);
    Fem::Field::CFieldWorld *world_ = world->Self;
    int fdt_ = (int)fdt;
    
    return this->self->SetValueType(field_type_, fdt_, *world_);
}

int CField::GetLayer(unsigned int id_ea)
{
    return this->self->GetLayer(id_ea);
}

bool CField::FindVelocityAtPoint(array<double>^ velo,
        [Out] unsigned int %id_ea_stat, [Out] unsigned int% ielem_stat, [Out] double% r1, [Out] double% r2,
        array<double>^ co, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    pin_ptr<double> velo_ptr = &velo[0];
    double *velo_ = (double *)velo_ptr;
    unsigned int id_ea_stat_;
    unsigned int ielem_stat_;
    double r1_;
    double r2_;
    pin_ptr<double> co_ptr = &co[0];
    double *co_ = (double *)co_ptr;    
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    bool ret = this->self->FindVelocityAtPoint(velo_,
        id_ea_stat_, ielem_stat_, r1_, r2_,
        co_, *world_);
        
    return ret;
}

unsigned int CField::GetMapVal2Co(unsigned int inode_va)
{
    return this->self->GetMapVal2Co(inode_va);
}

bool CField::ExportFile_Inp(String^ file_name, CFieldWorld^ world)
{
    std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
    
    Fem::Field::CFieldWorld *world_ = world->Self;
    
    bool ret = this->self->ExportFile_Inp(file_name_, *world_);
    
    return ret;
}
