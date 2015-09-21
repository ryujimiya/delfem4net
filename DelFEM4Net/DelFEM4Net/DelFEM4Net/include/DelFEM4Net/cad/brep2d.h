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
@brief B-repを用いた位相情報格納クラス(Cad::CBrep)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_B_REP_2D_H)
#define DELFEM4NET_B_REP_2D_H

#ifdef __VISUALC__
    #pragma warning( disable : 4786 )
#endif

#include "DelFEM4Net/stub/clr_stub.h" //DelFEM4NetCom::Pair
//#include "DelFEM4Net/cad/brep.h"
#include "DelFEM4Net/cad_com.h"
#include "DelFEM4Net/serialize.h"

#include "delfem/cad/brep2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

public ref class CBRepSurface
{
public:
    //! iterator goes edge and vetex around one specific loop
    ref class CItrLoop : public DelFEM4NetCad::IItrLoop
    {
    public:
        //CItrLoop();
        CItrLoop(CBRepSurface^ pBRep2D, unsigned int id_l);
        CItrLoop(CBRepSurface^ pBRep2D, unsigned int id_he, unsigned int id_ul);
        CItrLoop(Cad::CBRepSurface::CItrLoop *self);
        ~CItrLoop();
        !CItrLoop();
        property Cad::CBRepSurface::CItrLoop* Self
        {
            Cad::CBRepSurface::CItrLoop* get();
        }
        virtual void Begin();        
        virtual bool IsEnd();
        virtual void operator++(); //!< move to next edge
        virtual void operator++(int n);    //!< dummy operator(to implement ++)
        virtual void Increment(){ (*this)++; } // C#から演算子++が使用できなかったので追加
        virtual bool GetIdEdge([Out] unsigned int% id_e, [Out] bool% is_same_dir);
        ////////////////
        virtual bool ShiftChildLoop();    
        virtual bool IsEndChild() ;
        virtual unsigned int GetIdVertex();
        virtual unsigned int GetIdVertex_Ahead() ;
        virtual unsigned int GetIdVertex_Behind();
        ////////////////
        unsigned int GetIdHalfEdge();
        unsigned int GetIdUseLoop();
        unsigned int GetIdLoop();
        unsigned int GetType(); // 0:浮遊点 1:浮遊辺 2:面積がある
        unsigned int CountVertex_UseLoop();
        bool IsParent();
        bool IsSameUseLoop(CItrLoop^ itrl);
        bool IsEdge_BothSideSameLoop();
    
    protected:
        Cad::CBRepSurface::CItrLoop *self;
    };
  
    //! iterator goes through loops and edges around one vertex
    ref class CItrVertex : public DelFEM4NetCad::IItrVertex
    {
    public:
        //CItrVertex();
        CItrVertex(CBRepSurface^ pBRep2D, unsigned int id_v);
        CItrVertex(Cad::CBRepSurface::CItrVertex *self);
        ~CItrVertex();
        !CItrVertex();
        property Cad::CBRepSurface::CItrVertex * Self
        {
            Cad::CBRepSurface::CItrVertex * get();
        }
        //! 反時計周りに頂点まわりをめぐる
        virtual void operator++();
        virtual void operator++(int n);    //!< ダミーのオペレータ(++と同じ働き)
        virtual void Increment(){ (*this)++; } // C#から演算子++が使用できなかったので追加
        //! 頂点周りの辺のIDと、その辺の始点がid_vと一致しているかどうか
        virtual bool GetIdEdge_Ahead([Out] unsigned int% id_e, [Out] bool% is_same_dir);
        virtual bool GetIdEdge_Behind([Out] unsigned int% id_e, [Out] bool% is_same_dir);
        //! Get ID of the loop
        virtual unsigned int GetIdLoop();
        void Begin();            
        virtual bool IsEnd(); //! return true if iterator goes around vertex
        ////////////////
        // non virtual hrom here
        unsigned int GetIdHalfEdge() ;
        unsigned int GetIdUseVertex();
        unsigned int CountEdge();
        bool IsParent();
        bool IsSameUseLoop(CItrVertex^ itrl);
        
    protected:
        Cad::CBRepSurface::CItrVertex *self;
    };
    
    ////
    ref class CResConnectVertex
    {
    public:
        CResConnectVertex()
        {
            this->self = new Cad::CBRepSurface::CResConnectVertex();
        }
        CResConnectVertex(Cad::CBRepSurface::CResConnectVertex *self)
        {
            this->self = self;
        }
        ~CResConnectVertex()
        {
            this->!CResConnectVertex();
        }
        !CResConnectVertex()
        {
            delete this->self;
        }
    public:
        property Cad::CBRepSurface::CResConnectVertex *Self
        {
            Cad::CBRepSurface::CResConnectVertex * get() { return this->self; }
        }
        property unsigned int id_v1
        {
            unsigned int get() { return this->self->id_v1; }
            void set(unsigned int value) { this->self->id_v1 = value; }
        }
        property unsigned int id_v2
        {
            unsigned int get() { return this->self->id_v2; }
            void set(unsigned int value) { this->self->id_v2 = value; }
        }
        property unsigned int id_l
        {
            unsigned int get() { return this->self->id_l; }
            void set(unsigned int value) { this->self->id_l = value; }
        }
        property unsigned int id_e_add
        {
            unsigned int get() { return this->self->id_e_add; }
            void set(unsigned int value) { this->self->id_e_add = value; }
        }
        property unsigned int id_l_add
        {
            unsigned int get() { return this->self->id_l_add; }
            void set(unsigned int value) { this->self->id_l_add = value; }
        }
        property bool is_left_l_add
        {
            bool get() { return this->self->is_left_l_add; }
            void set(bool value) { this->self->is_left_l_add = value; }
        }
    protected:
        Cad::CBRepSurface::CResConnectVertex *self;
    };
    
public:
    CBRepSurface();
    CBRepSurface(Cad::CBRepSurface *self);
    ~CBRepSurface();
    !CBRepSurface();
    property Cad::CBRepSurface * Self
    {
        Cad::CBRepSurface * get();
    }

    bool AssertValid();
    bool IsElemID(DelFEM4NetCad::CAD_ELEM_TYPE,unsigned int id);
    IList<unsigned int>^ GetAryElemID(DelFEM4NetCad::CAD_ELEM_TYPE itype);
    bool GetIdLoop_Edge(unsigned int id_e, [Out] unsigned int% id_l_l, [Out] unsigned int% id_l_r);
    unsigned int GetIdLoop_Edge(unsigned int id_e, bool is_left);    
    bool GetIdVertex_Edge(unsigned int id_e, [Out] unsigned int% id_v1, [Out] unsigned int% id_v2);
    unsigned int GetIdVertex_Edge(unsigned int id_e, bool is_root );
    
    CItrLoop^ GetItrLoop(unsigned int id_l);
    CItrLoop^ GetItrLoop_SideEdge(unsigned int id_e, bool is_left);
    CItrVertex^ GetItrVertex(unsigned int id_v);

    ////////////////

    void Clear();

    // Add vertex to edge; ret val is id of vertex; ret 0 if it fails
    unsigned int AddVertex_Edge(unsigned int id_e); 
    // 面に頂点を加える関数(失敗したら０を返す)
    unsigned int AddVertex_Loop(unsigned int id_l);
  
    bool RemoveEdge(unsigned int id_e, bool is_del_cp);
    bool RemoveVertex(unsigned int id_v);
  
    bool MakeHole_fromLoop(unsigned int id_l);
    unsigned int SealHole(unsigned int id_e, bool is_left);
  
    // function to make edge from 2 vertices (ID:id_v1,id_v2)
    CBRepSurface::CResConnectVertex^ ConnectVertex(CItrVertex^ itrv1, CItrVertex^ itrv2, bool is_id_l_add_left);
    IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ GetItrLoop_ConnectVertex(CItrVertex^ itrv1, CItrVertex^ itrv2);
    IList< DelFEM4NetCom::Pair<unsigned int, bool>^ >^ GetItrLoop_RemoveEdge(unsigned int id_e);

    bool SwapItrLoop(CItrLoop^ itrl, unsigned int id_l_to );
    
    //! save and load file
    bool Serialize( DelFEM4NetCom::CSerializer^ serialize );
 
protected:
    Cad::CBRepSurface *self;

};



}


#endif
