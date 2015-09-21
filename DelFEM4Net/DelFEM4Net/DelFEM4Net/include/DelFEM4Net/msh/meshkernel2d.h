﻿/*
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
@brief ２次元メッシュのUtility関数群
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_MESH_KERNEL_2D_H)
#define DELFEM4NET_MESH_KERNEL_2D_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#include <vector>

#include "DelFEM4Net/vector2d.h"

#include "delfem/msh/meshkernel2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMsh
{

/*! 
@addtogroup Msh2D
*/
//! @{

////////////////////////////////////////////////
// ここから本当は共有させたい

#if 0

const double MIN_TRI_AREA = 1.0e-10;

const unsigned int nNoEd  = 2;    //!< 線分にいくら点があるか

const unsigned int nNoTri = 3;    //!< ３角形にいくら頂点があるか
const unsigned int nEdTri = 3;    //!< ３角形にいくら辺があるか
//! ３角形の各辺の頂点番号
const unsigned int noelTriEdge[nEdTri][nNoEd] = {    
    { 1, 2 },
    { 2, 0 },
    { 0, 1 },
};
//! ３角形の隣接関係
const unsigned int relTriTri[3][3] = {
    { 0, 2, 1 }, //  0
    { 2, 1, 0 }, //  1 
    { 1, 0, 2 }, //  2
};

const unsigned int nNoQuad = 4;    //!< ４角形にいくら頂点があるか
const unsigned int nEdQuad = 4;    //!< ４角形にいくら辺があるか
//! ４角形の各辺の頂点番号
const unsigned int noelQuadEdge[nEdQuad][nNoEd] = {    
    { 0, 1 },
    { 1, 2 },
    { 2, 3 },
    { 3, 0 }
};
//! ４角形の隣接関係
static const unsigned int relQuadQuad[nNoQuad][nNoQuad] = {
    { 0, 3, 2, 1 }, //  0
    { 1, 0, 3, 2 }, //  1
    { 2, 1, 0, 3 }, //  2
    { 3, 2, 1, 0 }, //  3
};

#endif

//! 辺要素構造体
public ref class SBar
{
public:
    static int C_V_CNT = 2;  // 頂点数
    static int C_S_CNT = 2;  // 隣接要素数 左側が０右側が1

    SBar ()
    {
       this->self = new Msh::SBar;
    }
    
    SBar(const SBar% rhs)
    {
        Msh::SBar rhs_instance_ = *(rhs.self);
        this->self = new Msh::SBar(rhs_instance_);
    }
    
    SBar(Msh::SBar *self)
    {
        this->self = self;
    }
    
    ~SBar()
    {
        this->!SBar();
    }
    
    !SBar()
    {
        delete this->self;
    }
    
public:
    property Msh::SBar * Self
    {
        Msh::SBar *get() { return this->self; }
    }

    property array<unsigned int>^ v  //!< 頂点番号 v[2]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->v;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_V_CNT);
            for (int i = 0; i < C_V_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_V_CNT);
            unsigned int* unmanaged = this->self->v;
            for (int i = 0; i < C_V_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }
    
    ////////////////
    // 左側が０右側が1
    property array<unsigned int>^ s2  //s2[2]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->s2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->s2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<unsigned int>^ r2  //r2[2]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->r2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->r2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }
    
protected:
    Msh::SBar *self;
    
};


////////////////////////////////////////////////////////////////

//! ４角形要素構造体
/*
struct SQuad2D{
    unsigned int v[4];    //!< 頂点Index
    int g2[4];            //!< 隣接する要素配列ID(-1:隣接要素なし、-2:自分の要素配列に隣接)
    unsigned int s2[4];    //!< 隣接要素Index
    unsigned int r2[4];            //!< 隣接関係
};
*/
public ref class SQuad2D
{
public:
    static int C_V_CNT = 4;  // 頂点数
    static int C_S_CNT = 4;  // 隣接要素数

    SQuad2D ()
    {
       this->self = new Msh::SQuad2D;
    }
    
    SQuad2D(const SQuad2D% rhs)
    {
        Msh::SQuad2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::SQuad2D(rhs_instance_);
    }
    
    SQuad2D(Msh::SQuad2D *self)
    {
        this->self = self;
    }
    
    ~SQuad2D()
    {
        this->!SQuad2D();
    }
    
    !SQuad2D()
    {
        delete this->self;
    }
    
public:
    property Msh::SQuad2D * Self
    {
        Msh::SQuad2D *get() { return this->self; }
    }

    property array<unsigned int>^ v  //!< 頂点Index v[4]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->v;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_V_CNT);
            for (int i = 0; i < C_V_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_V_CNT);
            unsigned int* unmanaged = this->self->v;
            for (int i = 0; i < C_V_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<int>^ g2  //!< 隣接する要素配列ID(-1:隣接要素なし、-2:自分の要素配列に隣接) g2[4]
    {
        array<int>^ get()
        {
            int* unmanaged = this->self->g2;
            array<int>^ managed = gcnew array<int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            int* unmanaged = this->self->g2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<unsigned int>^ s2  //!< 隣接要素Index s2[4]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->s2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->s2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<unsigned int>^ r2  //!< 隣接関係 r2[4]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->r2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->r2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

protected:
    Msh::SQuad2D *self;
    
};


//! ２次元３角形要素構造体
/*
struct STri2D{
    unsigned int v[3];    //!< 頂点Index
    int g2[3];            //!< 隣接する要素配列ID(-1:隣接要素なし、-2:自分の要素配列に隣接)
    unsigned int s2[3];    //!< 隣接要素Index
    unsigned int r2[3];    //!< 隣接関係
};
*/
public ref class STri2D
{
public:
    static int C_V_CNT = 3;  // 頂点数
    static int C_S_CNT = 3;  // 隣接要素数

    STri2D ()
    {
       this->self = new Msh::STri2D;
    }
    
    STri2D(const STri2D% rhs)
    {
        Msh::STri2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::STri2D(rhs_instance_);
    }
    
    STri2D(Msh::STri2D *self)
    {
        this->self = self;
    }
    
    ~STri2D()
    {
        this->!STri2D();
    }
    
    !STri2D()
    {
        delete this->self;
    }
    
public:
    property Msh::STri2D * Self
    {
        Msh::STri2D *get() { return this->self; }
    }

    property array<unsigned int>^ v  //!< 頂点Index v[3]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->v;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_V_CNT);
            for (int i = 0; i < C_V_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_V_CNT);
            unsigned int* unmanaged = this->self->v;
            for (int i = 0; i < C_V_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<int>^ g2  //!< 隣接する要素配列ID(-1:隣接要素なし、-2:自分の要素配列に隣接) g2[3]
    {
        array<int>^ get()
        {
            int* unmanaged = this->self->g2;
            array<int>^ managed = gcnew array<int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            int* unmanaged = this->self->g2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<unsigned int>^ s2  //!< 隣接要素Index s2[3]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->s2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->s2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

    property array<unsigned int>^ r2  //!< 隣接関係 r2[3]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->r2;
            array<unsigned int>^ managed = gcnew array<unsigned int>(C_S_CNT);
            for (int i = 0; i < C_S_CNT; i++)
            {
                managed[i] = unmanaged[i];
            }
            return managed;
        }
        void set(array<unsigned int>^ managed)
        {
            assert(managed != nullptr);
            assert(managed->Length == C_S_CNT);
            unsigned int* unmanaged = this->self->r2;
            for (int i = 0; i < C_S_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }

protected:
    Msh::STri2D *self;
    
};

/*!
@brief ２次元点クラス
(CPoint2D.e!=-1)なら(aTri[e].no[d])がこの点の全体節点番号であるはず
*/
public ref class CPoint2D
{
public:
    CPoint2D ()
    {
       this->self = new Msh::CPoint2D();
    }
    
    CPoint2D(const CPoint2D% rhs)
    {
        Msh::CPoint2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CPoint2D(rhs_instance_);
    }
    
    CPoint2D(double x, double y, int ielem, unsigned int idir)
    {
       this->self = new Msh::CPoint2D(x, y, ielem, idir);
    }

    CPoint2D(Msh::CPoint2D *self)
    {
        this->self = self;
    }
    
    ~CPoint2D()
    {
        this->!CPoint2D();
    }
    
    !CPoint2D()
    {
        delete this->self;
    }
    
public:
    property Msh::CPoint2D * Self
    {
        Msh::CPoint2D *get() { return this->self; }
    }

    property DelFEM4NetCom::CVector2D^ p   //!< 点の座標
    {
        DelFEM4NetCom::CVector2D^ get()
        {
            const Com::CVector2D& unmanaged_instance = this->self->p;
            Com::CVector2D * unmanaged = new Com::CVector2D(unmanaged_instance); //コピーを作成
            return gcnew DelFEM4NetCom::CVector2D(unmanaged);
        }
        void set(DelFEM4NetCom::CVector2D^ value)
        {
            this->self->p = *(value->Self);
        }
    }
    
    property int e              //!< 点を囲む要素のうち一つの番号(孤立している点なら-1が入る)
    {
        int get() { return this->self->e ; }
        void set(int value) { this->self->e = value; }
    }
    
    property unsigned int d     //!< 点を囲む要素eの要素内節点番号
    {
        unsigned int get() { return this->self->d ; }
        void set(unsigned int value) { this->self->d = value; }
    }
    
protected:
    Msh::CPoint2D *self;
    
};


#if 0
////////////////////////////////////////////////////////////////
/
//! 点を囲む三角形のリストを作る
bool MakePointSurTri(const std::vector<Msh::STri2D>& aTri, const unsigned int npoin, 
        unsigned int* const elsup_ind, unsigned int& nelsup, unsigned int*& elsup );

//! 点を囲む４角形のリストを作る
bool MakePointSurQuad(const std::vector<Msh::SQuad2D>& aQuad, const unsigned int npoin, 
        unsigned int* const elsup_ind, unsigned int& nelsup, unsigned int*& elsup );
//! 点を囲む線分要素のリストを作る
bool MakePointSurBar( const std::vector<Msh::SBar>& aBar, const unsigned int npoin, 
        unsigned int* const elsup_ind, unsigned int& nelsup, unsigned int*& elsup );

//! ３角形分割の内部情報(隣接関係)を作る
bool MakeInnerRelationTri( std::vector<Msh::STri2D>& aTri, const unsigned int npoin, 
    const unsigned int* elsup_ind, const unsigned int nelsup, const unsigned int* elsup);
//! ４角形分割の内部情報(隣接関係)を作る
bool MakeInnerRelationQuad( std::vector<Msh::SQuad2D>& aQuad, const unsigned int npoin, 
    const unsigned int* elsup_ind, const unsigned int nelsup, const unsigned int* elsup);
//! 線分分割の内部情報(隣接関係)を作る.T字連結は含まないとする
//bool MakeInnerRelationBar( std::vector<Msh::SBar>& aBar );

//! 三角形分割の外側を囲む辺要素配列を作る
bool MakeOuterBoundTri( const std::vector<Msh::STri2D>& aTri, std::vector<Msh::SBar>& aBar );
//! ４角形分割の外側を囲む辺要素配列を作る
bool MakeOuterBoundQuad( const std::vector<Msh::SQuad2D>& aQuad, std::vector<Msh::SBar>& aBar );

//! 三角形分割の正当性をチェック
bool CheckTri( const std::vector<Msh::STri2D>& aTri );
//! 三角形分割の正当性をチェック
bool CheckTri( const std::vector<Msh::CPoint2D>& po, const std::vector<Msh::STri2D>& tri );

/*!
@brief 点を要素内部に追加する
@param[in] ipo_ins  要素に追加する頂点の番号
@param[in] itri_ins 追加する要素の番号
@retval true    成功
@retval false   失敗
*/
bool InsertPoint_Elem( const unsigned int ipo_ins, const unsigned int itri_ins,
                      std::vector<Msh::CPoint2D>& po, std::vector<Msh::STri2D>& tri );
//! 点を要素の辺に追加する
bool InsertPoint_ElemEdge( const unsigned int ipo_ins,
    const unsigned int itri_ins, const unsigned int ied_ins,
    std::vector<Msh::CPoint2D>& po, std::vector<Msh::STri2D>& tri );

//! 点周りのデローニ分割
bool DelaunayAroundPoint( unsigned int ipo0,
                         std::vector<Msh::CPoint2D>& po, std::vector<Msh::STri2D>& tri );

/*!
@brief 点周りのデローニ分割
num_flipは辺をFlipした回数を引数に足し合わせる．０で初期化はしない．
*/
bool DelaunayAroundPoint( unsigned int itri, unsigned int inotri, 
                         std::vector<Com::CVector2D>& aVec, std::vector<Msh::STri2D>& tri,
                         unsigned int& num_flip);
//! 辺をフリップする
bool FlipEdge( unsigned int itri0, unsigned int ied0,  std::vector<Msh::STri2D>& aTri );
//! 辺をフリップする
bool FlipEdge( unsigned int itri0, unsigned int ied0,
              std::vector<Msh::CPoint2D>& aPo, std::vector<Msh::STri2D>& aTri );
//! 辺をフリップする
bool FindEdge( const unsigned int& ipo0, const unsigned int& ipo1,
    unsigned int& itri0, unsigned int& inotri0, unsigned int& inotri1,
    const std::vector<Msh::CPoint2D>& po, const std::vector<Msh::STri2D>& tri );
//! 半直線をまたぐ辺を見つける
bool FindEdgePoint_AcrossEdge( const unsigned int& ipo0, const unsigned int& ipo1,
    unsigned int& itri0, unsigned int& inotri0, unsigned int& inotri1, double& ratio,
    std::vector<Msh::CPoint2D>& po, std::vector<Msh::STri2D>& tri );

//! ギフト・ラッピング法による三角形分割
bool Tesselate_GiftWrapping(
    unsigned int ipo0, unsigned int ipo1,
    std::vector< std::pair<unsigned int,unsigned int> >& aTriEd,
    const std::vector<Com::CVector2D>& aPo, std::vector<Msh::STri2D>& aTri);

//! 三角形分割から１点を削除
bool DeletePointFromMesh(unsigned int ipoin, std::vector<Msh::CPoint2D>& aPo, std::vector<Msh::STri2D>& aTri);
//! 三角形分割から１要素を削除
bool DeleteTri(unsigned int itri0, std::vector<Msh::CPoint2D>& aPo, std::vector<Msh::STri2D>& aTri);

//! ラプラシアンスムージング
void LaplacianSmoothing( std::vector<Msh::CPoint2D>& aPo, const std::vector<Msh::STri2D>& aTri,
    const std::vector<unsigned int>& aflg_isnt_move );
//! ラプラシアンスムージング
void LaplaceDelaunaySmoothing( std::vector<Msh::CPoint2D>& aPo, std::vector<Msh::STri2D>& aTri,
    const std::vector<unsigned int>& aflg_isnt_move );
//! バブルメッシュを用いたスムージング
void PliantBossenHeckbertSmoothing( double elen, std::vector<Msh::CPoint2D>& aPo, std::vector<Msh::STri2D>& aTri );


//! BarAryを色分けする
void ColorCodeBarAry( const std::vector<SBar>& aBar, const std::vector<Com::CVector2D>& aVec, 
                     std::vector< std::vector<unsigned int> >& aIndBarAry );

//! @}

#endif

} // end name space mesh;



#endif // MSH_H
