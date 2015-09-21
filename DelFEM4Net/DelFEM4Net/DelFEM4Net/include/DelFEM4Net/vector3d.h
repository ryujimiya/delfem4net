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
@brief ３次元ベクトルクラス(Com::CVector3D)の実装
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_VECTOR_3D_H)
#define DELFEM4NET_VECTOR_3D_H

#include <vector>
#include <cassert>
#include <math.h>
#include <iostream>

//#define NEARLY_ZERO 1.e-16

#include "DelFEM4Net/stub/clr_stub.h" // DelFEM4NetCom::DoubleArrayIndexer
#include "delfem/vector3d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

ref class CVector3D;

//! @{
//! @}

//! ３次元ベクトルクラス
public ref class CVector3D  
{
public:
    static int C_VEC_SIZE = 3;
    inline static double Dot(CVector3D^ arg1, CVector3D^ arg2);
    inline static CVector3D^ Cross(CVector3D^ arg1, CVector3D^ arg2);
    inline static CVector3D^ operator + (CVector3D^ lhs, CVector3D^ rhs);
    inline static CVector3D^ operator - (CVector3D^ lhs, CVector3D^ rhs);
    inline static CVector3D^ operator * (double d, CVector3D^ rhs);
    inline static CVector3D^ operator / (CVector3D^ vec, double d);
    inline static CVector3D^ operator * (CVector3D^ vec, double d);
    inline static void GetVertical2Vector(CVector3D^ vec_n, [Out] CVector3D^% vec_x, [Out] CVector3D^% vec_y);
    inline static double ScalarTripleProduct3D(array<double>^ a, array<double>^ b, array<double>^ c);
    inline static double Dot3D(array<double>^ a, array<double>^ b);
    inline static void Cross3D([Out] array<double>^% r, array<double>^ v1, array<double>^ v2);
    inline static double Length3D(array<double>^ v);
    inline static double Distance3D(array<double>^ p0, array<double>^ p1);
    inline static double TriArea3D(array<double>^ v1, array<double>^ v2, array<double>^ v3);
    inline static void UnitNormalAreaTri3D([Out] array<double>^% n, [Out] double% a, array<double>^ v1, array<double>^ v2, array<double>^ v3);  //[OUT] n, a
    inline static void NormalTri3D([Out] array<double>^% n, array<double>^ v1, array<double>^ v2, array<double>^ v3);  //[OUT] n
    inline static double TetVolume3D(array<double>^ v1,
                                     array<double>^ v2, 
                                     array<double>^ v3, 
                                     array<double>^ v4 );
    inline static void GetVertical2Vector3D(array<double>^ vec_n, [Out] array<double>^% vec_x, [Out] array<double>^% vec_y); //[OUT]vec_x, vec_y


    inline static double Height(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3, CVector3D^ v4);
    inline static double TetVolume(CVector3D^ v1,
                                   CVector3D^ v2, 
                                   CVector3D^ v3, 
                                   CVector3D^ v4 );
    inline static double TetVolume( int iv1, int iv2, int iv3, int iv4, IList<CVector3D^>^ node);
    inline static void Cross( [Out] CVector3D^% lhs, CVector3D^ v1, CVector3D^ v2 );  //OUT : rhs
    inline static double TriArea(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3);
    inline static double TriArea(const int iv1, const int iv2, const int iv3, IList<CVector3D^>^ node);
    inline static double SquareTriArea(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3);

    inline static double SquareDistance(CVector3D^ ipo0, CVector3D^ ipo1);
    inline static double SquareLength(CVector3D^ point);
    inline static double Length(CVector3D^ point);
    inline static double Distance(CVector3D^ ipo0, CVector3D^ ipo1);
    inline static double SqareLongestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 );
    inline static double LongestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 );

    inline static double SqareShortestEdgeLength(CVector3D^ ipo0,
                          CVector3D^ ipo1,
                          CVector3D^ ipo2,
                          CVector3D^ ipo3 );
    inline static double ShortestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 );
    inline static void Normal(
        [Out] CVector3D^% vnorm,  //OUT
        CVector3D^ v1, 
        CVector3D^ v2, 
        CVector3D^ v3);
    inline static void UnitNormal(
        [Out] CVector3D^% vnorm,  //OUT
        CVector3D^ v1, 
        CVector3D^ v2, 
        CVector3D^ v3);
    inline static double SquareCircumradius(
        CVector3D^ ipo0, 
        CVector3D^ ipo1, 
        CVector3D^ ipo2, 
        CVector3D^ ipo3);
    inline static double Circumradius(CVector3D^ ipo0, 
                           CVector3D^ ipo1, 
                           CVector3D^ ipo2, 
                           CVector3D^ ipo3);
    inline static CVector3D^ RotateVector(CVector3D^ vec0, CVector3D^ rot );

public:
    CVector3D()
    {
        this->self = new Com::CVector3D();
    }
    
    CVector3D(const CVector3D% rhs)
    {
        Com::CVector3D rhs_instance_ = *rhs.self;
        this->self = new Com::CVector3D(rhs_instance_);
    }
    
    CVector3D(double vx, double vy, double vz)
    {
        this->self = new Com::CVector3D(vx, vy, vz);
    }

    CVector3D(Com::CVector3D *self)
    {
        this->self = self;
    }
    
    virtual ~CVector3D()
    {
        this->!CVector3D();
    }
    
    !CVector3D()
    {
        delete this->self;
    }

    void SetVector(double vx, double vy, double vz)
    {
        this->self->SetVector(vx, vy, vz);
    }

    /* C++/CLIでは提供しない operator=は既定のハンドルの単純コピーとなる
    inline CVector3D^ operator=(CVector3D^ rhs)
    {
        //const Com::CVector3D& ret_instance_ = this->self->operator=(*(rhs->self));
        //return this;
        
        //return gcnew CVector3D(*this);
    }
    */
    
    inline CVector3D^ operator+=(CVector3D^ rhs)
    {
        const Com::CVector3D& ret_instance_ = this->self->operator+=(*(rhs->self));
        return this;
    }
    
    inline CVector3D^ operator-=(const CVector3D^ rhs)
    {
        const Com::CVector3D& ret_instance_ = this->self->operator-=(*(rhs->self));
        return this;
    }
    
    inline CVector3D^ operator*=(double d)
    {
        const Com::CVector3D& ret_instance_ = this->self->operator*=(d);
        return this;
    }
    
    inline CVector3D^ operator/=(double d)
    {
        const Com::CVector3D& ret_instance_ = this->self->operator/=(d);
        return this;
    }
    
    inline CVector3D^ operator+()
    {
        //const Com::CVector3D& ret_instance_ = this->self->operator+();
        //Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
        //return gcnew CVector3D(ret);

        //return CVector3D::operator(1.0, this);
        return gcnew CVector3D(*this);
    }
    
    inline CVector3D^ operator-()
    { 
        //const Com::CVector3D& ret_instance_ = this->self->operator-();
        //Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
        //return gcnew CVector3D(ret);
        
        return CVector3D::operator*(-1.0, this);
    }

    inline double Length() 
    {
        return this->self->Length();
    }

    inline double DLength() 
    {
        return this->self->DLength();
    }

    void Normalize()
    {
        this->self->Normalize();
    }
    
    void SetZero()
    {
        this->self->SetZero();
    }
    
public:
    property Com::CVector3D * Self
    {
        Com::CVector3D * get() { return this->self; }
    }
    property double x    //!< ｘ座標値
    {
        double get () { return this->self->x ; }
        void set (double value) { this->self->x = value; }
    }
    
    property double y    //!< ｙ座標値
    {
        double get () { return this->self->y ; }
        void set (double value) { this->self->y = value; }
    }
    
    property double z    //!< ｚ座標値
    {
        double get () { return this->self->z ; }
        void set (double value) { this->self->z = value; }
    }
    
protected:
    Com::CVector3D * self;
    
};


//! @{
    
/*! 
@brief 内積の計算
*/
inline double CVector3D::Dot(CVector3D^ arg1, CVector3D^ arg2)
{
    return Com::Dot( *(arg1->Self), *(arg2->Self) );
}

/*! 
@brief 外積の計算
*/
inline CVector3D^ CVector3D::Cross(CVector3D^ arg1, CVector3D^ arg2)
{
    const Com::CVector3D& ret_instance_ = Com::Cross( *(arg1->Self), *(arg2->Self) );
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}

//! 足し算
inline CVector3D^ CVector3D::operator + (CVector3D^ lhs, CVector3D^ rhs)
{
    const Com::CVector3D& ret_instance_ = Com::operator + (*(lhs->Self), *(rhs->Self));
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}

//! 引き算
inline CVector3D^ CVector3D::operator - (CVector3D^ lhs, CVector3D^ rhs)
{
    const Com::CVector3D& ret_instance_ = Com::operator - (*(lhs->Self), *(rhs->Self));
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}

//! 実数倍
inline CVector3D^ CVector3D::operator * (double d, CVector3D^ rhs)
{
    const Com::CVector3D& ret_instance_ = Com::operator * (d, *(rhs->Self));
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}

//! 実数で割る
inline CVector3D^ CVector3D::operator / (CVector3D^ vec, double d)
{
    const Com::CVector3D& ret_instance_ = Com::operator / (*(vec->Self), d);
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}


//! 実数倍
inline CVector3D^ CVector3D::operator * (CVector3D^ vec, double d)
{
    const Com::CVector3D& ret_instance_ = Com::operator * (*(vec->Self), d);
    Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
    return gcnew CVector3D(ret);
}
//! @}
  

inline void CVector3D::GetVertical2Vector(CVector3D^ vec_n, [Out] CVector3D^% vec_x, [Out] CVector3D^% vec_y)  //[OUT]vec_x,vec_y
{
    Com::CVector3D *vec_n_ = vec_n->Self;
    Com::CVector3D *vec_x_ = new Com::CVector3D();
    Com::CVector3D *vec_y_ = new Com::CVector3D();
    
    Com::GetVertical2Vector( *vec_n_, *vec_x_, *vec_y_);
    
    vec_x = gcnew CVector3D(vec_x_);
    vec_y = gcnew CVector3D(vec_y_);
}

inline double CVector3D::ScalarTripleProduct3D(array<double>^ a, array<double>^ b, array<double>^ c)
{
    assert(a->Length == C_VEC_SIZE);
    assert(b->Length == C_VEC_SIZE);
    assert(c->Length == C_VEC_SIZE);
    pin_ptr<double> a_ = &a[0];
    pin_ptr<double> b_ = &b[0];
    pin_ptr<double> c_ = &c[0];
    return Com::ScalarTripleProduct3D(a_, b_, c_) ;
}

inline double CVector3D::Dot3D(array<double>^ a, array<double>^ b)
{
    assert(a->Length == C_VEC_SIZE);
    assert(b->Length == C_VEC_SIZE);
    pin_ptr<double> a_ = &a[0];
    pin_ptr<double> b_ = &b[0];
    return Com::Dot3D(a_, b_);
}
  
inline void CVector3D::Cross3D([Out] array<double>^% r, array<double>^ v1, array<double>^ v2) // [OUT] r
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];

    // 戻り値用配列を用意
    r = gcnew array<double>(3);
    pin_ptr<double> r_ = &r[0];

    Com::Cross3D(r_, v1_, v2_) ; // [OUT] r
}
  
inline double CVector3D::Length3D(array<double>^ v)
{
    assert(v->Length == C_VEC_SIZE);
    pin_ptr<double> v_ = &v[0];
    return Com::Length3D(v_);
}  

inline double CVector3D::Distance3D(array<double>^ p0, array<double>^ p1)
{
    assert(p0->Length == C_VEC_SIZE);
    assert(p1->Length == C_VEC_SIZE);
    pin_ptr<double> p0_ = &p0[0];
    pin_ptr<double> p1_ = &p1[0];
    return Com::Distance3D(p0_, p1_);
}

inline double CVector3D::TriArea3D(array<double>^ v1, array<double>^ v2, array<double>^ v3)
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    assert(v3->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    pin_ptr<double> v3_ = &v3[0];
    return Com::TriArea3D(v1_, v2_, v3_);
}

inline void CVector3D::UnitNormalAreaTri3D([Runtime::InteropServices::Out] array<double>^% n, [Runtime::InteropServices::Out] double% a, array<double>^ v1, array<double>^ v2, array<double>^ v3)  //[OUT] n, a
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    assert(v3->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    pin_ptr<double> v3_ = &v3[0];

    // 戻り値用配列を用意
    n = gcnew array<double>(C_VEC_SIZE);
    pin_ptr<double> n_ = &n[0];
    double a_;
    
    Com::UnitNormalAreaTri3D(n_, a_, v1_, v2_, v3_); //[OUT]n_, a_
    
    a = a_;
}
  
inline void CVector3D::NormalTri3D([Out] array<double>^% n, array<double>^ v1, array<double>^ v2, array<double>^ v3)  //[OUT] n
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    assert(v3->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    pin_ptr<double> v3_ = &v3[0];

    // 戻り値用配列を用意
    n = gcnew array<double>(C_VEC_SIZE);
    pin_ptr<double> n_ = &n[0];
    
    Com::NormalTri3D(n_, v1_, v2_, v3_); //[OUT]n_
}
  
inline double CVector3D::TetVolume3D(array<double>^ v1,
                                     array<double>^ v2, 
                                     array<double>^ v3, 
                                     array<double>^ v4 )
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    assert(v3->Length == C_VEC_SIZE);
    assert(v4->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    pin_ptr<double> v3_ = &v3[0];
    pin_ptr<double> v4_ = &v4[0];

    return Com::TetVolume3D(v1_, v2_, v3_, v4_) ;
}

inline void CVector3D::GetVertical2Vector3D(array<double>^ vec_n, [Out] array<double>^% vec_x, [Out] array<double>^% vec_y) //[OUT]vec_x, vec_y
{
    assert(vec_n->Length == C_VEC_SIZE);
    pin_ptr<double> vec_n_ = &vec_n[0];

    // 戻り値用配列を用意
    vec_x = gcnew array<double>(C_VEC_SIZE);
    vec_y = gcnew array<double>(C_VEC_SIZE);
    pin_ptr<double> vec_x_ = &vec_x[0];
    pin_ptr<double> vec_y_ = &vec_y[0];
    
    Com::GetVertical2Vector3D(vec_n_, vec_x_, vec_y_);
}
  

//!< ３×３の行列
public ref class CMatrix3
{
public:
    static int C_MAT_SIZE = 9;
    static int C_VEC_SIZE = 3;
    
public:
    CMatrix3()
    {
        this->self = new Com::CMatrix3();
        
        initMatIndexer();
    }
  
    CMatrix3(const CMatrix3% rhs)
    {
        Com::CMatrix3 rhs_instance_ = *(rhs.self);
        this->self = new Com::CMatrix3(rhs_instance_);
        
        initMatIndexer();
    }

    CMatrix3(CVector3D^ vec0)
    {
        Com::CVector3D *vec0_ = vec0->Self;
        this->self = new Com::CMatrix3(*vec0_);
        
        initMatIndexer();
    }
    
    CMatrix3(array<double>^ m)
    {
        assert(m->Length == C_MAT_SIZE);
        pin_ptr<double> m_ = &m[0];
        this->self = new Com::CMatrix3(m_);
        
        initMatIndexer();
    }

    CMatrix3(Com::CMatrix3 *self)
    {
        this->self = self;
        
        initMatIndexer();
    }
    
    ~CMatrix3()
    {
        this->!CMatrix3();
    }
    
    !CMatrix3()
    {
        delete this->self;
    }
    
    ////
    // {vec} = [mat]{vec0}
    CVector3D^ MatVec(CVector3D^ vec0) 
    {
        const Com::CVector3D& ret_instance_ = this->self->MatVec(*(vec0->Self));
        Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
        CVector3D^ retManaged = gcnew CVector3D(ret);
        return retManaged;
    }
    
    void MatVec(array<double>^ vec0, [Out] array<double>^% vec1)  //[OUT] vec1
    {
        assert(vec0->Length == C_VEC_SIZE);
        pin_ptr<double> vec0_ = &vec0[0];
        // 出力用の配列作成
        vec1 = gcnew array<double>(C_VEC_SIZE);
        pin_ptr<double> vec1_ = &vec1[0];

        this->self->MatVec(vec0_, vec1_);
    }
    
    // {vec1} = [mat]T{vec0}
    void MatVecTrans(array<double>^ vec0, [Out] array<double>^% vec1)  //[OUT] vec1
    {
        assert(vec0->Length == C_VEC_SIZE);
        pin_ptr<double> vec0_ = &vec0[0];
        // 出力用の配列作成
        vec1 = gcnew array<double>(C_VEC_SIZE);
        pin_ptr<double> vec1_ = &vec1[0];

        this->self->MatVecTrans(vec0_, vec1_);
    }
  
    CVector3D^ MatVecTrans(CVector3D^ vec0)
    {
        const Com::CVector3D& ret_instance_ = this->self->MatVecTrans(*(vec0->Self));
        Com::CVector3D *ret = new Com::CVector3D(ret_instance_);
        CVector3D^ retManaged = gcnew CVector3D(ret);
        return retManaged;
    }

    // [ret] = [mat][mat0]
    CMatrix3^ MatMat(CMatrix3^ mat0)
    {
        const Com::CMatrix3& ret_instance_ = this->self->MatMat(*(mat0->self));
        Com::CMatrix3 *ret = new Com::CMatrix3(ret_instance_);
        CMatrix3^ retManaged = gcnew CMatrix3(ret);
        return retManaged;
    }

    // [ret] = [mat]T[mat0]
    CMatrix3^ MatMatTrans(CMatrix3^ mat0)
    {
        const Com::CMatrix3& ret_instance_ = this->self->MatMatTrans(*(mat0->self));
        Com::CMatrix3 *ret = new Com::CMatrix3(ret_instance_);
        CMatrix3^ retManaged = gcnew CMatrix3(ret);
        return retManaged;
    }

    void SetRotMatrix_Rodrigues(array<double>^ vec)
    {
        assert(vec->Length == C_VEC_SIZE);
        pin_ptr<double> vec_ = &vec[0];
        this->self->SetRotMatrix_Rodrigues(vec_);
    }

    void SetRotMatrix_CRV(array<double>^ crv)
    {
        assert(crv->Length == C_VEC_SIZE);
        pin_ptr<double> crv_ = &crv[0];
        this->self->SetRotMatrix_CRV(crv_);
    }

    void GetCRV_RotMatrix([Runtime::InteropServices::Out] array<double>^% crv)  // [OUT]crv
    {
        // 出力用の配列作成
        crv = gcnew array<double>(C_VEC_SIZE);
        pin_ptr<double> crv_ = &crv[0];

        this->self->GetCRV_RotMatrix(crv_);// [OUT]crv_
    }
    
    void SetSpinTensor(CVector3D^ vec0)
    {
        this->self->SetSpinTensor(*(vec0->Self));
    }
    
    void SetIdentity()
    {
        this->self->SetIdentity();
    }
    void SetIdentity(double scale)
    {
        this->self->SetIdentity(scale);
    }

public:
    property Com::CMatrix3* Self
    {
        Com::CMatrix3* get() { return this->self; }
    }

    // nativeクラスのdouble mat[9]アクセス用
    property DelFEM4NetCom::DoubleArrayIndexer^ mat
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get() { return this->matIndexser; }
        //protected:
        //    void set(DelFEM4NetCom::DoubleArrayIndexer^ value) { this->matIndexser = value; }
    }
    
    /*
    // nativeクラスのdouble mat[9]アクセス用
    //    double get_mat(int index)
    //    void set_mat(int index, double value)
    property double mat[int]
    {
        double get(int index)
        {
            assert(index >= 0 && index < C_MAT_SIZE);
            return this->self->mat[index];
        }
        void set(int index, double value)
        {
            assert(index >= 0 && index < C_MAT_SIZE);
            this->self->mat[index] = value;
        }
    }
    */

protected:
    Com::CMatrix3 *self;

    DelFEM4NetCom::DoubleArrayIndexer^ matIndexser;

    void initMatIndexer()
    {
        this->matIndexser = gcnew DelFEM4NetCom::DoubleArrayIndexer(C_MAT_SIZE, &this->self->mat[0]);
    }
};


//! 3D bounding box class
public ref class CBoundingBox3D
{
public:
    static int BB_SIZE = 8;
    
    CBoundingBox3D()
    {
        this->self = new Com::CBoundingBox3D();
    }
    
    CBoundingBox3D(const CBoundingBox3D% rhs)
    {
        Com::CBoundingBox3D rhs_instance_ = *(rhs.self);
        this->self = new Com::CBoundingBox3D(rhs_instance_);
    }
    
    CBoundingBox3D(Com::CBoundingBox3D *self)
    {
        this->self = self;
    }
    
    CBoundingBox3D(double x_min0,double x_max0,  double y_min0,double y_max0,  double z_min0,double z_max0)
    {
        this->self = new Com::CBoundingBox3D(x_min0, x_max0, y_min0, y_max0, z_min0, z_max0);
    }

    ~CBoundingBox3D()
    {
        this->!CBoundingBox3D();
    }
    
    !CBoundingBox3D()
    {
        delete this->self;
    }

    CBoundingBox3D^ operator+=(CBoundingBox3D^ bb)
    {
        const Com::CBoundingBox3D& ret_instance_ = this->self->operator+=(*(bb->self));
        return this;
    }

    bool IsInside(CVector3D^ vec)
    {
        Com::CVector3D *vec_ = vec->Self;
        
        return this->self->IsInside(*vec_);
    }

    bool IsPossibilityIntersectSphere(CVector3D^ vec, const double radius )
    {
        Com::CVector3D *vec_ = vec->Self;
        return this->self->IsPossibilityIntersectSphere(*vec_, radius);
    }

    bool AddPoint(CVector3D^ vec, double eps)
    {
        Com::CVector3D *vec_ = vec->Self;
        return this->self->AddPoint(*vec_, eps);
    }
    
    void SetValueToArray([Runtime::InteropServices::Out] array<double>^% bb)  // OUT : bb
    {
        bb = gcnew array<double>(BB_SIZE);
        pin_ptr<double> bb_ = &bb[0];

        this->self->SetValueToArray(bb_);
    }
    
    void ProjectOnLine([Runtime::InteropServices::Out] double% min_r, [Runtime::InteropServices::Out] double% max_r, 
                       DelFEM4NetCom::CVector3D^ org, DelFEM4NetCom::CVector3D^ dir) //OUT: min_r, max_r
    {
        Com::CVector3D *org_ = org->Self;
        Com::CVector3D *dir_ = dir->Self;
        double min_r_;
        double max_r_;
        
        this->self->ProjectOnLine(min_r_, max_r_, *org_, *dir_);
        
        min_r = min_r_;
        max_r = max_r_;
    }

public:
    property Com::CBoundingBox3D * Self
    {
        Com::CBoundingBox3D * get() { return this->self; }
    }
    property double x_min
    {
        double get() { return this->self->x_min; }
        void set(double value) { this->self->x_min = value; }
    }
    property double x_max
    {
        double get() { return this->self->x_max; }
        void set(double value) { this->self->x_max = value; }
    }
    property double y_min
    {
        double get() { return this->self->y_min; }
        void set(double value) { this->self->y_min = value; }
    }
    property double y_max
    {
        double get() { return this->self->y_max; }
        void set(double value) { this->self->y_max = value; }
    }
    property double z_min
    {
        double get() { return this->self->z_min; }
        void set(double value) { this->self->z_min = value; }
    }
    property double z_max
    {
        double get() { return this->self->z_max; }
        void set(double value) { this->self->z_max= value; }
    }
    property bool isnt_empty //!< false if there is nothing inside
    {
        bool get() { return this->self->isnt_empty; }
        void set(bool value) { this->self->isnt_empty= value; }
    }
    
protected:
    Com::CBoundingBox3D *self;
    
};

public ref class COctTree
{
public:
    COctTree();
    COctTree(const COctTree% rhs);
    COctTree(Com::COctTree *self);
    ~COctTree();
    !COctTree();
    property Com::COctTree* Self
    {
        Com::COctTree * get();
    }
    void SetBoundingBox(CBoundingBox3D^ bb );
    int InsertPoint( unsigned int ipo_ins, CVector3D^ vec_ins );

    bool Check();
    int GetIndexCell_IncludePoint( CVector3D^ vec );
    void GetAllPointInCell( unsigned int icell0, [Runtime::InteropServices::Out] IList<unsigned int>^% ipoins ); //[OUT]ipoins
    void GetBoundaryOfCell(unsigned int icell0, [Runtime::InteropServices::Out] CBoundingBox3D^% bb ); //[OUT] bb
    bool IsPointInSphere( double radius, CVector3D^ vec );

protected:
    Com::COctTree *self;
};


//! 四面体の高さ
inline double CVector3D::Height(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3, CVector3D^ v4)
{
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;
    Com::CVector3D *v4_ = v4->Self;

    return Com::Height(*v1_, *v2_, *v3_, *v4_);
}


////////////////////////////////////////////////////////////////

//! 四面体の体積
inline double CVector3D::TetVolume(CVector3D^ v1,
                                   CVector3D^ v2, 
                                   CVector3D^ v3, 
                                   CVector3D^ v4 )
{
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;
    Com::CVector3D *v4_ = v4->Self;

    return Com::TetVolume(*v1_, *v2_, *v3_, *v4_);
}

//! 四面体の体積
inline double CVector3D::TetVolume( int iv1, int iv2, int iv3, int iv4, IList<CVector3D^>^ node)
{
    std::vector<Com::CVector3D> node_;
    
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CVector3D, Com::CVector3D>(node, node_);
    
    return Com::TetVolume(iv1, iv2, iv3, iv4, node_);
}

////////////////////////////////////////////////

//! 外接ベクトル
inline void CVector3D::Cross( [Out] CVector3D^% lhs, CVector3D^ v1, CVector3D^ v2 )  //OUT : rhs
{
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *lhs_ = new Com::CVector3D();
    
    Com::Cross(*lhs_, *v1_, *v2_);  //OUT : rhs_

    lhs = gcnew CVector3D(lhs_);
}

//! ３次元３角形の面積
inline double CVector3D::TriArea(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3)
{
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;

    return Com::TriArea(*v1_, *v2_, *v3_);
}

//! ３次元３角形の面積
inline double CVector3D::TriArea(const int iv1, const int iv2, const int iv3, IList<CVector3D^>^ node)
{
    std::vector<Com::CVector3D> node_;
    
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CVector3D, Com::CVector3D>(node, node_);
    
    return Com::TriArea(iv1, iv2, iv3, node_);
}

//! ３次元３角形の面積の２乗
inline double CVector3D::SquareTriArea(CVector3D^ v1, CVector3D^ v2, CVector3D^ v3)
{
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;

    return Com::SquareTriArea(*v1_, *v2_, *v3_);
}

////////////////////////////////////////////////

//! 長さの２乗
inline double CVector3D::SquareDistance(CVector3D^ ipo0, CVector3D^ ipo1)
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    return Com::SquareDistance(*ipo0_, *ipo0_);
}

//! 長さの２乗
inline double CVector3D::SquareLength(CVector3D^ point)
{
    Com::CVector3D *point_ = point->Self;
    return Com::SquareLength(*point_);
}

////////////////////////////////////////////////

//! length of vector
inline double CVector3D::Length(CVector3D^ point)
{
    Com::CVector3D *point_ = point->Self;
    return Com::Length(*point_);
}

//! distance between two points
inline double CVector3D::Distance(CVector3D^ ipo0, CVector3D^ ipo1)
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    return Com::Distance(*ipo0_, *ipo0_);
}

////////////////////////////////////////////////

//! ４点を互いに結ぶ６つの線分のうち最も長いものの長さ（四面体の質評価で用いる）
inline double CVector3D::SqareLongestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 )
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::SqareLongestEdgeLength(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

////////////////////////////////////////////////

//! ４点を互いに結ぶ６つの線分のうち最も長いものの長さ（四面体の質評価で用いる）
inline double CVector3D::LongestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 )
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::LongestEdgeLength(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

////////////////////////////////////////////////

//! ４点を互いに結ぶ６つの線分のうち最も短いものの長さ（四面体の質評価で用いる）
inline double CVector3D::SqareShortestEdgeLength(CVector3D^ ipo0,
                          CVector3D^ ipo1,
                          CVector3D^ ipo2,
                          CVector3D^ ipo3 )
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::SqareShortestEdgeLength(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

////////////////////////////////////////////////


//! ４点を互いに結ぶ６つの線分のうち最も短いものの長さ（四面体の質評価で用いる）
inline double CVector3D::ShortestEdgeLength(
        CVector3D^ ipo0,
        CVector3D^ ipo1,
        CVector3D^ ipo2,
        CVector3D^ ipo3 )
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::ShortestEdgeLength(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

////////////////////////////////////////////////

//! 法線ベクトル
inline void CVector3D::Normal(
        [Out] CVector3D^% vnorm,  //OUT
        CVector3D^ v1, 
        CVector3D^ v2, 
        CVector3D^ v3)
{
    Com::CVector3D *vnorm_ = new Com::CVector3D();
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;

    Com::Normal(*vnorm_, *v1_, *v2_, *v3_);
    
    vnorm = gcnew CVector3D(vnorm_);
}

////////////////////////////////////////////////

//! 単位法線ベクトル
inline void CVector3D::UnitNormal(
        [Out] CVector3D^% vnorm,  //OUT
        CVector3D^ v1, 
        CVector3D^ v2, 
        CVector3D^ v3)
{
    Com::CVector3D *vnorm_ = new Com::CVector3D();
    Com::CVector3D *v1_ = v1->Self;
    Com::CVector3D *v2_ = v2->Self;
    Com::CVector3D *v3_ = v3->Self;

    Com::UnitNormal(*vnorm_, *v1_, *v2_, *v3_);
    
    vnorm = gcnew CVector3D(vnorm_);
}

////////////////////////////////////////////////

/*! 
外接球の半径
*/
inline double CVector3D::SquareCircumradius(
        CVector3D^ ipo0, 
        CVector3D^ ipo1, 
        CVector3D^ ipo2, 
        CVector3D^ ipo3)
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::SquareCircumradius(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

////////////////////////////////////////////////

/*! 
外接球の半径
*/
inline double CVector3D::Circumradius(CVector3D^ ipo0, 
                           CVector3D^ ipo1, 
                           CVector3D^ ipo2, 
                           CVector3D^ ipo3)
{
    Com::CVector3D *ipo0_ = ipo0->Self;
    Com::CVector3D *ipo1_ = ipo1->Self;
    Com::CVector3D *ipo2_ = ipo2->Self;
    Com::CVector3D *ipo3_ = ipo3->Self;

    return Com::Circumradius(*ipo0_, *ipo1_, *ipo2_, *ipo3_);
}

inline CVector3D^ CVector3D::RotateVector(CVector3D^ vec0, CVector3D^ rot )
{
    Com::CVector3D *vec0_ = vec0->Self;
    Com::CVector3D *rot_ = rot->Self;
    
    Com::CVector3D *ret = new Com::CVector3D();
    
    *ret = Com::RotateVector(*vec0_, *rot_);
    
    CVector3D^ retManaged = gcnew CVector3D(ret);
    return retManaged;
}

//typedef NativeInstanceArrayIndexer<Com::CVector3D, DelFEM4NetCom::CVector3D, DelFEM4NetCom::CVector3D^> CVector3DArrayIndexer;
public ref class CVector3DArrayIndexer : public NativeInstanceArrayIndexer<Com::CVector3D, DelFEM4NetCom::CVector3D, DelFEM4NetCom::CVector3D^>
{
public:
    CVector3DArrayIndexer(int size_, Com::CVector3D *ptr_) : NativeInstanceArrayIndexer<Com::CVector3D, DelFEM4NetCom::CVector3D, DelFEM4NetCom::CVector3D^>(size_, ptr_)
    {
    }
    
    CVector3DArrayIndexer(const CVector3DArrayIndexer% rhs) : NativeInstanceArrayIndexer<Com::CVector3D, DelFEM4NetCom::CVector3D, DelFEM4NetCom::CVector3D^>(rhs)
    {
    }
    
    ~CVector3DArrayIndexer()
    {
        this->!CVector3DArrayIndexer();
    }
    
    !CVector3DArrayIndexer()
    {
    }
};

}    // end namespace DelFEM4NetCom

#endif // !defined(DELFEM4NET_VECTOR_3D_H)
