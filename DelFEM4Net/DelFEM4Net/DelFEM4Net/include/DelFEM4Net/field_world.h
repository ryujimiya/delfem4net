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
@brief interface of field administration class (Fem::Field::CFieldWorld)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_FIELD_WORLD_H)
#define DELFEM4NET_FIELD_WORLD_H

//#include <map>

#include "DelFEM4Net/elem_ary.h"    // need because reference of class "CElemAry" used
#include "DelFEM4Net/node_ary.h"    // need because reference of class "CNodeAry" used
#include "DelFEM4Net/field.h"
//#include "DelFEM4Net/objset.h"      // template for container with ID
#include "DelFEM4Net/cad_com.h"     // need for enum CAD_ELEM_TYPE

#include "delfem/field_world.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMsh
{
    interface class IMesh;    
}

namespace DelFEM4NetFem
{
namespace Field
{
  
/*! 
@brief ID converter between element array, mesh, CAD
@ingroup Fem
*/
public ref class CIDConvEAMshCad
{
public:
    CIDConvEAMshCad()
    {
        this->self = new Fem::Field::CIDConvEAMshCad();
    }

    CIDConvEAMshCad(const CIDConvEAMshCad% rhs)
    {
        Fem::Field::CIDConvEAMshCad rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::CIDConvEAMshCad(rhs_instance_);
    }
    
    CIDConvEAMshCad(Fem::Field::CIDConvEAMshCad *self)
    {
        this->self = self;
    }
    
    ~CIDConvEAMshCad()
    {
        this->!CIDConvEAMshCad();
    }
    
    !CIDConvEAMshCad()
    {
        delete this->self;
    }
    
    property Fem::Field::CIDConvEAMshCad * Self
    {
        Fem::Field::CIDConvEAMshCad * get() { return this->self; }
    }


    void Clear()
    {
        this->self->Clear();
    }
    
    bool IsIdEA(unsigned int id_ea)
    {
        return this->self->IsIdEA(id_ea);
    }

    unsigned int GetIdEA_fromMsh(unsigned int id_part_msh)
    {
        return this->self->GetIdEA_fromMsh(id_part_msh);
    }

    unsigned int GetIdEA_fromMshExtrude(unsigned int id_part_msh, unsigned int inum_ext)
    {
        return this->self->GetIdEA_fromMshExtrude(id_part_msh, inum_ext);
    }

    // itype_cad_part : Vertex(0), Edge(1), Loop(2)
    unsigned int GetIdEA_fromCad(unsigned int id_part_cad, DelFEM4NetCad::CAD_ELEM_TYPE itype_cad_part)
    {
        unsigned int inum_ext = 0;

        return GetIdEA_fromCad(id_part_cad, itype_cad_part, inum_ext);
    }    
    unsigned int GetIdEA_fromCad(unsigned int id_part_cad, DelFEM4NetCad::CAD_ELEM_TYPE itype_cad_part, unsigned int inum_ext)
    {
        Cad::CAD_ELEM_TYPE itype_cad_part_ = static_cast<Cad::CAD_ELEM_TYPE>(itype_cad_part);
        
        return this->self->GetIdEA_fromCad(id_part_cad, itype_cad_part_, inum_ext);
    }
    
    void GetIdCad_fromIdEA(unsigned int id_ea, [Out] unsigned int% id_part_cad, [Out] DelFEM4NetCad::CAD_ELEM_TYPE% itype_part_cad )
    {
        unsigned int id_part_cad_;
        Cad::CAD_ELEM_TYPE itype_part_cad_;
        
        this->self->GetIdCad_fromIdEA(id_ea, id_part_cad_, itype_part_cad_);
        
        id_part_cad = id_part_cad_;
        itype_part_cad = static_cast<DelFEM4NetCad::CAD_ELEM_TYPE>(itype_part_cad_);
    }

protected:
    Fem::Field::CIDConvEAMshCad *self;

};

/*! 
@brief field administration class
@ingroup Fem
*/
public ref class CFieldWorld
{
public:
    CFieldWorld();
    CFieldWorld(const CFieldWorld% world);
    CFieldWorld(Fem::Field::CFieldWorld *self);
    ~CFieldWorld();
    !CFieldWorld();
    property Fem::Field::CFieldWorld *Self
    {
        Fem::Field::CFieldWorld * get();
    }
  
    //CFieldWorld^ operator = (CFieldWorld^ world);
    
    CFieldWorld^ Clone();
    
    // import mesh into FEM world
    unsigned int AddMesh(DelFEM4NetMsh::IMesh^ mesh);

    unsigned int SetCustomBaseField(unsigned int id_base,
        IList<unsigned int>^ aIdEA_Inc,
        IList< IList<int>^ >^ aLnods,
        IList<unsigned int>^ mapVal2Co);

    CIDConvEAMshCad^ GetIDConverter(unsigned int id_field_base);

    void Clear();    // Delate all field, elem_ary, node_ary
  
    ////////////////////////////////////////////////////////////////
    // functionas for element array
    
    bool IsIdEA( unsigned int id_ea ) ;    // check if id_ea is a ID of element array
    IList<unsigned int>^ GetAry_IdEA();    // get all ID of element array
    CElemAry^ GetEA(unsigned int id_ea);    // get element array ( with-const )
    CElemAryPtr^ GetEAPtr(unsigned int id_ea); // get element array ( without-const )
    // add element array ( return id>0, return 0 if fail )
    unsigned int AddElemAry(unsigned int size, DelFEM4NetFem::Field::ELEM_TYPE elem_type);
    //nativeのライブラリで実装されていない？
    //bool AddIncludeRelation(unsigned int id_ea, unsigned int id_ea_inc);     // AddElemAryと統一したい．

    ////////////////////////////////////////////////////////////////
    // functions for node arary

    bool IsIdNA( unsigned int id_na );    // check if id_na is a ID of node array
    IList<unsigned int>^ GetAry_IdNA();    // get all ID of node array
    CNodeAry^ GetNA(unsigned int id_na);    // Get Node Array ( with-const )
    CNodeAryPtr^ GetNAPtr(unsigned int id_na);    // Get Node Array ( without-const )
    // add node array ( return id>0, return 0 if fail )
    unsigned int AddNodeAry(unsigned int size);
    
    ////////////////////////////////////////////////////////////////
    // functions for field
    
    bool IsIdField( unsigned int id_field );    // check if id_field is a ID of field
    IList<unsigned int>^ GetAry_IdField();    // get all ID of field
    CField^ GetField(unsigned int id_field);    // get field ( with const )
    CFieldPtr^ GetFieldPtr(unsigned int id_field); // get field ( without-const)

    ////////////////////////////////////////////////////////////////
    // functions to add field

    unsigned int MakeField_FieldElemAry(unsigned int id_field, unsigned int id_ea)
    {
        DelFEM4NetFem::Field::FIELD_TYPE field_type = DelFEM4NetFem::Field::FIELD_TYPE::NO_VALUE;
        //int derivative_type = 1;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        //int node_configuration_type = 1;
        DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type = DelFEM4NetFem::Field::ELSEG_TYPE::CORNER;
        return MakeField_FieldElemAry(id_field, id_ea, field_type, derivative_type, node_configuration_type);
    }
    unsigned int MakeField_FieldElemAry(unsigned int id_field, unsigned int id_ea, DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type, /*int*/DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type);
    
    unsigned int MakeField_FieldElemDim(unsigned int id_field, int idim_elem)
    {
        DelFEM4NetFem::Field::FIELD_TYPE field_type = DelFEM4NetFem::Field::FIELD_TYPE::NO_VALUE;
        //int derivative_type = 1;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        //int node_configuration_type = 1;
        DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type = DelFEM4NetFem::Field::ELSEG_TYPE::CORNER;
        return MakeField_FieldElemDim(id_field, idim_elem, field_type, derivative_type, node_configuration_type);
    }

    unsigned int MakeField_FieldElemDim(unsigned int id_field, int idim_elem, DelFEM4NetFem::Field::FIELD_TYPE field_type)
    {
        //int derivative_type = 1;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        //int node_configuration_type = 1;
        DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type = DelFEM4NetFem::Field::ELSEG_TYPE::CORNER;
        return MakeField_FieldElemDim(id_field, idim_elem, field_type, derivative_type, node_configuration_type);
    }
    unsigned int MakeField_FieldElemDim(unsigned int id_field, int idim_elem, DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type, /*int*/DelFEM4NetFem::Field::ELSEG_TYPE node_configuration_type);
    
    unsigned int AddField(unsigned int id_field_parent,    // parent field
                          IList<CField::CElemInterpolation^>^ aEI, 
                          CField::CNodeSegInNodeAry^ nsna_c, CField::CNodeSegInNodeAry^ nsna_b)
    {
        unsigned int id_field_candidate = 0;
        return AddField(id_field_parent, aEI, nsna_c, nsna_b, id_field_candidate);
    }
    unsigned int AddField(unsigned int id_field_parent,    // parent field
                          IList<CField::CElemInterpolation^>^ aEI, 
                          CField::CNodeSegInNodeAry^ nsna_c, CField::CNodeSegInNodeAry^ nsna_b,
                          unsigned int id_field_candidate);  
  
    //! Get partial field consists of element array with ID:id_ea
    unsigned int GetPartialField(unsigned int id_field, unsigned int IdEA );
    //! Get partial field consists of IDs of element array:aIdEA
    unsigned int GetPartialField(unsigned int id_field, IList<unsigned int>^ aIdEA);
  
    // update field
    bool UpdateMeshCoord(unsigned int id_base, DelFEM4NetMsh::IMesh^ mesh);
    bool UpdateMeshCoord(unsigned int id_base, unsigned int id_field_disp, DelFEM4NetMsh::IMesh^ mesh);  
  ////
    bool UpdateConnectivity(unsigned int id_base, DelFEM4NetMsh::IMesh^ mesh);
    bool UpdateConnectivity_CustomBaseField(unsigned int id_base,
                                            IList<unsigned int>^ aIdEA_Inc, 
                                            IList< IList<int>^ >^ aLnods,
                                            IList<unsigned int>^ mapVal2Co);
    bool UpdateConnectivity_HingeField_Tri(unsigned int id_field, unsigned int id_field_base);
    bool UpdateConnectivity_EdgeField_Tri( unsigned int id_field, unsigned int id_field_base);
    
  
    // Delete Field and referenced EA and NA. the EAs and NAs that is referenced from Field in use are not deleted
    void DeleteField(IList<unsigned int>^ aIdFieldDel );

protected:
    Fem::Field::CFieldWorld *self;

};

}    // end namespace Field
}    // end namespace DelFEM4NetFem

#endif
