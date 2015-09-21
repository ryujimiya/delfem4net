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
@brief interface of 3D cad class (Cad::CCadObj2Dm)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_OBJ_3D_H)
#define DELFEM4NET_CAD_OBJ_3D_H

#include <vector>

#include "DelFEM4Net/vector3d.h"
#include "DelFEM4Net/vector2d.h"
//#include "DelFEM4Net/objset.h"
#include "DelFEM4Net/cad_com.h"
#include "DelFEM4Net/cad/brep2d.h"
#include "DelFEM4Net/cad/cad_elem2d.h"
#include "DelFEM4Net/cad/cad_elem3d.h"

#include "delfem/cad_obj3d.h"

namespace DelFEM4NetCad{

//ref class CVertex3D;
ref class CLoop3D;
//ref class CEdge3D;
//ref class CTopology;

/*! 
@brief 3 dimentional cad model class
@ingroup CAD
*/
public ref class CCadObj3D
{
public:  
    //! iterator go around vertex
    ref class CItrVertex
    {
    public:
        /*
        CItrVertex()
        {
            this->self = new Cad::CCadObj3D::CItrVertex();
        }
        */

        CItrVertex(const CCadObj3D::CItrVertex% rhs)
        {
            Cad::CCadObj3D::CItrVertex& rhs_instance_ = *(rhs.self);
            this->self = new Cad::CCadObj3D::CItrVertex(rhs_instance_); // コピーコンストラクタを使用
        }

        CItrVertex(Cad::CCadObj3D::CItrVertex* value)
        {
            this->self = value;
        }

        CItrVertex(CCadObj3D^ pCadObj3D, unsigned int id_v)
        {
            this->self = new Cad::CCadObj3D::CItrVertex(pCadObj3D->Self, id_v);
        }

        ~CItrVertex()
        {
            this->!CItrVertex();
        }

        !CItrVertex()
        {
            delete this->self;
        }

        property Cad::CCadObj3D::CItrVertex* Self
        {
           Cad::CCadObj3D::CItrVertex* get() { return this->self; }
        }

        void operator++()    //!< go around (cc-wise) loop around vertex 
        {
            this->self->operator++();
        }
        
        void operator++(int n)    //!< dummy operator (for ++)        
        {
            this->self->operator++(n);
        }
        
        //! cc-wise ahead  edge-id and its direction(true root of edge is this vertex)
        bool GetIdEdge_Ahead([Out] unsigned int% id_e, [Out] bool% is_same_dir)
        {
            unsigned int id_e_ = id_e;
            bool is_same_dir_= is_same_dir;
            bool ret = this->self->GetIdEdge_Ahead(id_e_,is_same_dir_);
            id_e = id_e_;
            is_same_dir = is_same_dir_;
            return ret;
        }

        //! cc-wise behind edge-id and its direction(true root of edge is this vertex)
        bool GetIdEdge_Behind([Out] unsigned int% id_e, [Out] bool% is_same_dir)
        {
            unsigned int id_e_ = id_e;
            bool is_same_dir_= is_same_dir;
            bool ret = this->self->GetIdEdge_Behind(id_e_,is_same_dir_);
            id_e = id_e_;
            is_same_dir = is_same_dir_;
            return ret;
        }

        unsigned int GetIdLoop()  //!< get loop-id
        {
            return this->self->GetIdLoop();
        }

        bool IsEnd()    //!< return true if iterator go around
        {
            return this->self->IsEnd();
        }
    
    protected:
        Cad::CCadObj3D::CItrVertex *self;

    };

public:
    ////////////////////////////////
    // constructor & destructor
  
    CCadObj3D();
    CCadObj3D(const CCadObj3D% rhs);
    CCadObj3D(Cad::CCadObj3D* self);
    //! destructor
    ~CCadObj3D();
    !CCadObj3D();
    
    property Cad::CCadObj3D * Self
    {
        Cad::CCadObj3D * get();
    }

    void Clear();

    CCadObj3D^ Clone();
  
    //! function gives iterator which travel edge and loop around vertex (ID:id_v)
  
    bool IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE, unsigned int id);
    IList<unsigned int>^ GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype);

    unsigned int AddCuboid(double len_x, double len_y, double len_z);
    unsigned int AddPolygon(IList<DelFEM4NetCom::CVector3D^>^ aVec, unsigned int id_l);
    unsigned int AddRectLoop(unsigned int id_l, DelFEM4NetCom::CVector2D^ p0, DelFEM4NetCom::CVector2D^ p1);
    void LiftLoop(unsigned int id_l, DelFEM4NetCom::CVector3D^ dir);
    unsigned int AddPoint(DelFEM4NetCad::CAD_ELEM_TYPE type, unsigned int id_elem, DelFEM4NetCom::CVector3D^ po );
    CBRepSurface::CResConnectVertex^ ConnectVertex(unsigned int id_v1, unsigned int id_v2);
  
    DelFEM4NetCom::CVector3D^ GetVertexCoord(unsigned int id_v);
    bool GetIdVertex_Edge([Out] unsigned int% id_v_s, [Out] unsigned int% id_v_e, unsigned int id_e);
    unsigned int GetIdLoop_Edge(unsigned int id_e, bool is_left);
  
    CBRepSurface::CItrLoop^ GetItrLoop(unsigned int id_l);
    DelFEM4NetCad::CLoop3D^ GetLoop(unsigned int id_l);
    unsigned int AssertValid();
    
protected:
    Cad::CCadObj3D *self;

};

}    // end namespace CAD

#endif
