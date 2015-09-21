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
@brief interface of the class (Cad::CBRep) wich represents topology with B-Rep data strcutre
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_B_REP_H)
#define DELFEM4NET_B_REP_H

#ifdef __VISUALC__
    #pragma warning( disable : 4786 )
#endif

//#include "DelFEM4Net/cad/objset_cad.h"

#include "delfem/cad/brep.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

//! @addtogroup CAD
//! @{

//! topology loop class
public ref class CUseLoop
{
public:
    //CUseLoop() {}
    
    CUseLoop(unsigned int id,
             unsigned int id_he, unsigned int id_ul_c, unsigned int id_ul_p)
    {
         this->self = new Cad::CUseLoop(id, id_he, id_ul_c, id_ul_p);
    }

    CUseLoop(const CUseLoop% rhs)
    {
        const Cad::CUseLoop& ret_instance_ = *(rhs.self);
        this->self = new Cad::CUseLoop(ret_instance_);
    }

    CUseLoop(Cad::CUseLoop *self)
    {
        this->self = self;
    }

    ~CUseLoop()
    {
        this->!CUseLoop();
    }
    
    !CUseLoop()
    {
        delete this->self;
    }
    
    property Cad::CUseLoop * Self
    {
        Cad::CUseLoop * get() { return this->self; }
    }
    
public:
    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }
  
    // geometry element ID (brep.cppやbrep2d.cppでは参照されないはず？)
    property unsigned int id_l    //!< 外側のループの場合は０
    {
        unsigned int get() { return this->self->id_l; }
        void set(unsigned int value) { this->self->id_l = value; }
    }
  
    // topology elemnet ID
    property unsigned int id_he   //!< HalfEdgeのID
    {
        unsigned int get() { return this->self->id_he; }
        void set(unsigned int value) { this->self->id_he = value; }
    }

    property unsigned int id_ul_c    //!< 子ループ、id_ul_c=0の場合はリストの終わり
    {
        unsigned int get() { return this->self->id_ul_c; }
        void set(unsigned int value) { this->self->id_ul_c = value; }
    }
    
    property unsigned int id_ul_p    //!< 親ループ。id_lはid_ul_pを持っている．id_ul_p==idの場合は自分が親，id_ul_p==0の場合は外側のループ
    {
        unsigned int get() { return this->self->id_ul_p; }
        void set(unsigned int value) { this->self->id_ul_p = value; }
    }
    
protected:
    Cad::CUseLoop *self;
    
};

//! topoloty HalfEdge class
public ref class CHalfEdge
{
public:
    //CHalfEdge() {}
    
    CHalfEdge(unsigned int id,
              unsigned int id_uv,
              unsigned int id_he_f, unsigned int id_he_b, unsigned int id_he_o,
              unsigned int id_ul )
    {
         this->self = new Cad::CHalfEdge(id, id_uv, id_he_f, id_he_b, id_he_o, id_ul);
    }

    CHalfEdge(const CHalfEdge% rhs)
    {
        const Cad::CHalfEdge& ret_instance_ = *(rhs.self);
        this->self = new Cad::CHalfEdge(ret_instance_);
    }
    
    CHalfEdge(Cad::CHalfEdge *self)
    {
        this->self = self;
    }
    
    ~CHalfEdge()
    {
        this->!CHalfEdge();
    }
    
    !CHalfEdge()
    {
        delete this->self;
    }
    
    property Cad::CHalfEdge * Self
    {
        Cad::CHalfEdge * get() { return this->self; }
    }

public:
    property unsigned int id        //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_uv     //!< ID of UseVertex
    {
        unsigned int get() { return this->self->id_uv; }
        void set(unsigned int value) { this->self->id_uv = value; }
    }
    
    property unsigned int id_he_f   //!< ID of previous HalfEdge
    {
        unsigned int get() { return this->self->id_he_f; }
        void set(unsigned int value) { this->self->id_he_f = value; }
    }

    property unsigned int id_he_b   //!< ID of later HalfEdge
    {
        unsigned int get() { return this->self->id_he_b; }
        void set(unsigned int value) { this->self->id_he_b = value; }
    }

    property unsigned int id_he_o      //!< ID of opposite side HalfEdge (if UV is floating point, this become self )
    {
        unsigned int get() { return this->self->id_he_o; }
        void set(unsigned int value) { this->self->id_he_o = value; }
    }

    property unsigned int id_ul     //!< ID of UseLoop
    {
        unsigned int get() { return this->self->id_ul; }
        void set(unsigned int value) { this->self->id_ul = value; }
    }
  
    ////
    property unsigned int id_e      //!< id of edge geometry. 0 if uv is a floating vertex
    {
        unsigned int get() { return this->self->id_e; }
        void set(unsigned int value) { this->self->id_e = value; }
    }

    property bool is_same_dir       //!< is the geometry edge goes same direction as topology edge
    {
        bool get() { return this->self->is_same_dir; }
        void set(bool value) { this->self->is_same_dir = value; }
    }
    
protected:
    Cad::CHalfEdge *self;
    
};

//! 位相頂点クラス
public ref class CUseVertex
{
public:
    //CUseVertex() {}
    
    CUseVertex(unsigned int id, unsigned int id_he)
    {
        this->self = new Cad::CUseVertex(id, id_he);
    }

    CUseVertex(const CUseVertex% rhs)
    {
        this->self = new Cad::CUseVertex(*(rhs.self));
    }

    CUseVertex(Cad::CUseVertex *self)
    {
        this->self = self;
    }
    
    ~CUseVertex()
    {
        this->!CUseVertex();
    }
    
    !CUseVertex()
    {
        delete this->self;
    }
    
    property Cad::CUseVertex * Self
    {
        Cad::CUseVertex * get() { return this->self; }
    }
    
public:
    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_he //!< HalfEdgeのID
    {
        unsigned int get() { return this->self->id_he; }
        void set(unsigned int value) { this->self->id_he = value; }
    }

    //ここからは幾何要素ID
    property unsigned int id_v  //!< 幾何頂点のID
    {
        unsigned int get() { return this->self->id_v; }
        void set(unsigned int value) { this->self->id_v = value; }
    }

protected:
    Cad::CUseVertex *self;
    
};

//! B-repを用いた位相情報格納クラス
public ref class CBRep
{
public:
    CBRep();
    CBRep(const CBRep% rhs);
    CBRep(Cad::CBRep *self);
    ~CBRep();
    !CBRep();
    
    property Cad::CBRep * Self
    {
        Cad::CBRep * get();
    }

    //! 全ての要素を削除して，初期状態に戻る
    void Clear();

    bool IsID_UseLoop(unsigned int id_ul);
    bool IsID_HalfEdge(unsigned int id_he);
    bool IsID_UseVertex(unsigned int id_uv);
    IList<unsigned int>^ GetAry_UseVertexID();

    CUseLoop^ GetUseLoop(unsigned int id_ul);
    CUseVertex^ GetUseVertex(unsigned int id_uv);
    CHalfEdge^ GetHalfEdge(unsigned int id_he);

    bool SetLoopIDtoUseLoop(unsigned int id_ul, unsigned int id_l);
    bool SetVertexIDtoUseVertex(unsigned int id_uv, unsigned int id_v);
    bool SetEdgeIDtoHalfEdge(unsigned int id_he, unsigned int id_e, bool is_same_dir);
    
    int AssertValid_Use();
    IList<unsigned int>^ FindHalfEdge_Edge(unsigned int id_e);
    IList<unsigned int>^ FindHalfEdge_Vertex(unsigned int id_v);
    
    ////////////////////////////////
    // オイラー操作

    //! 点と点を繋げてループとエッジを作る
    bool MEVVL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2,
               [Out] unsigned int% id_uv_add1, [Out] unsigned int% id_uv_add2, [Out] unsigned int% id_ul_add );
    //! ループを２つに分ける
    bool MEL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_ul_add,
             unsigned int id_he1, unsigned int id_he2);
    /*!
    @brief 辺を消去して２つのループを１つにする 半辺he_remの反対側の半辺が接する半ループを消去する
    */
    bool KEL(unsigned int id_he_rem1);
    //! id_heの起点から、線分を伸ばす
    bool MEV([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_uv_add, 
             unsigned int id_he);
    //! id_heの起点を消去して２つの辺を１つにする。
    bool KVE(unsigned int id_he_rem1 );
    /*!
    @brief id_heを２つに分ける
    入力ではhe2はheに向かいあう半辺とすると
    出力ではheの前にhe_add1,heの向かいにhe_add2,he_add2の後ろにhe2
    */
    bool MVE([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2, [Out] unsigned int% id_uv_add, 
             unsigned int id_he);
    /*!
    @brief he1の起点uv1とhe2の起点uv2を結んで、２つのループをつなげる
    he1は[uv1-uv2]、he2は[uv2-uv1]
    */
    bool MEKL([Out] unsigned int% id_he_add1, [Out] unsigned int% id_he_add2,  
              unsigned int id_he1, unsigned int id_he2 );
    //! ループをつなげる
    bool KEML([Out] unsigned int% id_ul_add1,
              unsigned int id_he1 );
    /*!
    @brief ループと浮遊点をつなげる,he1がLoop上のEdgeでhe2が浮遊点Edge
    he2は[uv2-uv1], he_add1は[uv1-uv2]のHalfEdgeとなる
    */
    bool MEKL_OneFloatingVertex([Out] unsigned int% id_he_add1,
                                unsigned int id_he1, unsigned int id_he2);
    /*!
    @brief ループと浮遊点をつなげる,he1,he2が浮遊点Edge
    he1は[uv1-uv2],he2は[uv2-uv1]のHalfEdgeとなる
    */
    bool MEKL_TwoFloatingVertex(unsigned int id_he1, unsigned int id_he2);
    /*! 
    @brief 片方が端点であるEdgeを削除する。
    he1の起点uv1は他の辺につながっていない端点である
    */
    bool KEML_OneFloatingVertex([Out] unsigned int% id_ul_add, 
                                unsigned int id_he1);
    //! 両方が端点であるEdgeを削除する。
    bool KEML_TwoFloatingVertex([Out] unsigned int% id_ul_add,
                                unsigned int id_he1);
    //! 浮遊点を作る
    bool MVEL([Out] unsigned int% id_uv_add, [Out] unsigned int% id_he_add, [Out] unsigned int% id_ul_add, 
              unsigned int id_ul_p);
    //! 浮遊点を消す
    bool KVEL(unsigned int id_uv_rem);

    //! 位相ループを入れ替える
    bool SwapUseLoop(unsigned int id_ul1, unsigned int id_ul2);
    //! 位相ループを動かす、id_ul1のループをid_ul2の親の子にする。
    bool MoveUseLoop(unsigned int id_ul1, unsigned int id_ul2);

/*下記public変数はとりあえず削除
//private:    // そのうちprivateにする予定(残りはSerializ)
  CObjSetCad<CUseLoop>   m_UseLoopSet;   //!< UseLoopのIDで管理された集合
  CObjSetCad<CHalfEdge>  m_HalfEdgeSet;  //!< HalfEdgeのIDで管理された集合
  CObjSetCad<CUseVertex> m_UseVertexSet; //!< UseVertexのIDで管理された集合
*/
protected:
    Cad::CBRep *self;

};

//! @}

}

#endif

