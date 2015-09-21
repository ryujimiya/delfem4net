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
@brief interface of 2D cad class (DelFEM4NetCad::CCadObj2Dm)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_OBJ_2D_H)
#define DELFEM4NET_CAD_OBJ_2D_H

#include "DelFEM4Net/vector2d.h"
#include "DelFEM4Net/serialize.h"
#include "DelFEM4Net/cad2d_interface.h"
//#include "DelFEM4Net/objset.h"
#include "DelFEM4Net/cad/brep2d.h"
#include "DelFEM4Net/cad/cad_elem2d.h"

#include "delfem/cad_obj2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

//ref class CVertex2D;
//ref class CLoop2D;
ref class CEdge2D;
//ref class CTopology;

/*! 
@brief 2 dimentional cad model class
@ingroup CAD
*/
public ref class CCadObj2D : public DelFEM4NetCad::ICad2D_Msh
{
public:
    ref class CResAddVertex
    {
    public:
        CResAddVertex()
        {
            this->self = new Cad::CCadObj2D::CResAddVertex();
        }
        
        CResAddVertex(const CCadObj2D::CResAddVertex% rhs)
        {
            Cad::CCadObj2D::CResAddVertex& rhs_instance_ = *(rhs.self);
            this->self = new Cad::CCadObj2D::CResAddVertex(rhs_instance_); // コピーコンストラクタを使用
        }
        
        CResAddVertex(Cad::CCadObj2D::CResAddVertex* value)
        {
            this->self = value;
        }
        
        ~CResAddVertex()
        {
            this->!CResAddVertex();
        }
        
        !CResAddVertex()
        {
            delete this->self;
        }
        
    public:
        property Cad::CCadObj2D::CResAddVertex* Self
        {
           Cad::CCadObj2D::CResAddVertex* get() { return this->self; }
        }
        
        property unsigned int id_v_add
        {
            unsigned int get() { return this->self->id_v_add; }
            void set(unsigned int value) { this->self->id_v_add = value; }
        }
        
        property unsigned int id_e_add
        {
            unsigned int get() { return this->self->id_e_add; }
            void set(unsigned int value) { this->self->id_e_add = value; }
        }

    protected:
        Cad::CCadObj2D::CResAddVertex *self;

    };
  
    ref class CResAddPolygon
    {
    public:
        CResAddPolygon()
        {
            this->self = new Cad::CCadObj2D::CResAddPolygon();
        }

        CResAddPolygon(const CCadObj2D::CResAddPolygon% rhs)
        {
            Cad::CCadObj2D::CResAddPolygon& rhs_instance_ = *(rhs.self);
            this->self = new Cad::CCadObj2D::CResAddPolygon(rhs_instance_); // コピーコンストラクタを使用
        }

        CResAddPolygon(Cad::CCadObj2D::CResAddPolygon* value)
        {
            this->self = value;
        }

        ~CResAddPolygon()
        {
            this->!CResAddPolygon();
        }

        !CResAddPolygon()
        {
            delete this->self;
        }

    public:
        property Cad::CCadObj2D::CResAddPolygon* Self
        {
           Cad::CCadObj2D::CResAddPolygon* get() { return this->self; }
        }

        property unsigned int id_l_add
        {
            unsigned int get() { return this->self->id_l_add; }
            void set(unsigned int value) { this->self->id_l_add = value; }
        }

        property IList<unsigned int>^ aIdV
        {
            IList<unsigned int>^ get();
            void set(IList<unsigned int>^ value);
        }
    
        property IList<unsigned int>^ aIdE
        {
            IList<unsigned int>^ get();
            void set(IList<unsigned int>^ value);
        }

    protected:
        Cad::CCadObj2D::CResAddPolygon *self;

    };
  
  
    ////////////////////////////////
    // constructor & destructor
  
    //! constructor
    CCadObj2D();
    CCadObj2D(const CCadObj2D% rhs);
    CCadObj2D(Cad::CCadObj2D* self);
    //! destructor
    ~CCadObj2D();
    !CCadObj2D();
    property Cad::ICad2D_Msh * Cad2DMshSelf
    {
        virtual Cad::ICad2D_Msh * get();
    }
    
    property Cad::CCadObj2D * Self
    {
        Cad::CCadObj2D * get();
    }

  
    //! initialization clear all element
    void Clear();

    CCadObj2D^ Clone();

    ////////////////////////////////
    // Get method

    //! function gives iterator which travel vtx and edge inside the loop (ID:id_l)
    virtual DelFEM4NetCad::IItrLoop^ GetPtrItrLoop(unsigned int id_l);
    virtual bool IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE, unsigned int id);
    virtual IList<unsigned int>^ GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype); 
    virtual bool GetIdVertex_Edge([Out] unsigned int% id_v_s, [Out] unsigned int% id_v_e, unsigned int id_e);
    virtual bool GetIdLoop_Edge([Out] unsigned int% id_l_l, [Out] unsigned int% id_l_r, unsigned int id_e);
    CBRepSurface::CItrVertex^ GetItrVertex(unsigned int id_v);
    CBRepSurface::CItrLoop^ GetItrLoop(unsigned int id_l);
    
    // functions related to layer
    virtual int GetLayer(DelFEM4NetCad::CAD_ELEM_TYPE, unsigned int id);
    virtual void GetLayerMinMax(int% layer_min, int% layer_max);
    bool ShiftLayer_Loop(unsigned int id_l, bool is_up);
  
    double GetMinClearance();
  
    // loop functions
    //! @{
    bool CheckIsPointInsideLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point);

    double SignedDistPointLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point);  //!< id_v_ignore = 0
    double SignedDistPointLoop(unsigned int id_l1, DelFEM4NetCom::CVector2D^ point, unsigned int id_v_ignore);

    //! get color(double[3]) of loop(ID:id_l), return false if there is no loop(ID:id_l)
    virtual bool GetColor_Loop(unsigned int id_l, [Out] array<double>^% color);
    //! ID:id_l set color of loop
    virtual bool SetColor_Loop(unsigned int id_l, array<double>^ color);
    //! ID:id_l return are of the loop
    virtual double GetArea_Loop(unsigned int id_l);
    //! @}

    ////////////////////////////////
    // Edge member functions

    CEdge2D^ GetEdge(unsigned int id_e);  

    // Get Geometric Type of the Curve  (0:line, 1:arc, 2:polyline)
    virtual DelFEM4NetCad::CURVE_TYPE GetEdgeCurveType(const unsigned int id_e);
  
    //! get information of edge(ID:id_e)
    /*virtual */bool GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo); //!< elen = 1
    virtual bool GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo, double elen);

    // Get edge (ID:id_e) geometry as polyoine wich have ndiv divisions. The start and end points is ps and pe each
    virtual bool GetCurveAsPolyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^% aCo,
                                    unsigned int ndiv, DelFEM4NetCom::CVector2D^ ps, DelFEM4NetCom::CVector2D^ pe);

    bool GetPointOnCurve_OnCircle(unsigned int id_e,
                                  DelFEM4NetCom::CVector2D^ v0, double len, bool is_front,
                                  bool% is_exceed, DelFEM4NetCom::CVector2D^% out);

    //! Get point on the edge (ID:id_e) that is nearest from point (po_in)
    DelFEM4NetCom::CVector2D^ GetNearestPoint(unsigned int id_e, DelFEM4NetCom::CVector2D^ po_in);
    virtual bool GetColor_Edge(unsigned int id_e, [Out] array<double>^% color);
    virtual bool SetColor_Edge(unsigned int id_e, array<double>^ color);

    ////////////////////////////////
    // Vertex

    // get position of the vertex
    virtual DelFEM4NetCom::CVector2D^ GetVertexCoord(unsigned int id_v);
    virtual bool GetColor_Vertex(unsigned int id_v, [Out] array<double>^% color);
    virtual bool SetColor_Vertex(unsigned int id_v, array<double>^ color);

    ////////////////////////////////////////////////
    // Toplogy affecting shape edit functions
    
    // Add Polyton to loop
    // The vec_ary is a array of vertex points (both clockwise and anti-clockwise is possible.)
    // If id_l is 0 or omitted, the loop will be added outside.
    // This returns CResAddPolygon which contains IDs of all vertices and edges.
    CResAddPolygon^ AddPolygon(IList<DelFEM4NetCom::CVector2D^>^ vec_ary); //!< id_l = 0
    CResAddPolygon^ AddPolygon(IList<DelFEM4NetCom::CVector2D^>^ vec_ary, unsigned int id_l);
    
    CResAddPolygon^ AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal);  //!< id_l = 0, scale = 1
    CResAddPolygon^ AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal, unsigned int id_l); //!< scale = 1
    CResAddPolygon^ AddLoop(IList<DelFEM4NetCom::Pair<DelFEM4NetCad::CURVE_TYPE, IList<double>^>^>^ aVal, unsigned int id_l, double scale);

    // Add vertex to Cad element
    // add vertex (with the position vec) to elemnet (type:itype,id:ielem)
    // if itype is Cad::NOT_SET, the vertex is added outside of cad shape
    CResAddVertex^ AddVertex(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id_elem, DelFEM4NetCom::CVector2D^ vec);
    
    // Remove CAD element
    // return true if this operation was sucessfull. 
    // in case it returns false, the cad shape is intact.
    bool RemoveElement(DelFEM4NetCad::CAD_ELEM_TYPE itype, unsigned int id_elem);

    CBRepSurface::CResConnectVertex^ ConnectVertex(CEdge2D^ edge);
    CBRepSurface::CResConnectVertex^ ConnectVertex_Line(unsigned int id_v1, unsigned int id_v2);
    //! @}

    ////////////////////////////////////////////////
    // Geometry editing functions (topoloty intact)

    bool SetCurve_Bezier(unsigned int id_e, double cx0, double cy0, double cx1, double cy1);
    bool SetCurve_Polyline(const unsigned int id_e);
    
    //! set edge (ID:id_e) mesh
    bool SetCurve_Polyline(unsigned int id_e, IList<DelFEM4NetCom::CVector2D^>^ aVec);
    
    //! set edge (ID:id_e) arc 
    bool SetCurve_Arc(const unsigned int id_e); //!< is_left_side=false, rdist=10.0
    bool SetCurve_Arc(const unsigned int id_e, bool is_left_side); //!< rdist=10.0
    bool SetCurve_Arc(const unsigned int id_e, bool is_left_side, double rdist);
    
    //! set edge (ID:id_e) straight line
    bool SetCurve_Line(const unsigned int id_e);

    ////////////////////////////////////////////////
    // IO routines

    bool WriteToFile_dxf(String^ file_name, double scale);
    bool Serialize(DelFEM4NetCom::CSerializer^ serialize);

protected:


protected:
    Cad::CCadObj2D *self;
};

}

#endif
