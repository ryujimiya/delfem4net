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
@brief two-dimensional vector class (DelFEM4NetCom::CVector2D)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_VECTOR2D_H)
#define DELFEM4NET_VECTOR2D_H

#if defined(__VISUALC__)
    #pragma warning(disable:4786)
#endif

#include "DelFEM4Net/stub/clr_stub.h"
#include "delfem/vector2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

ref class CVector2D;

//! ２次元ベクトルクラス
public ref class CVector2D
{
public:
    static int C_VEC_SIZE = 2;
    
    inline static CVector2D^ operator*(double, CVector2D^);
    inline static CVector2D^ operator*(CVector2D^, double);
    inline static CVector2D^ operator+(CVector2D^, CVector2D^);
    inline static CVector2D^ operator-(CVector2D^, CVector2D^);

    inline static double TriArea(CVector2D^ v1, CVector2D^ v2, CVector2D^ v3);
    inline static double TriArea(int iv1, int iv2, int iv3, 
                                 IList<CVector2D^>^ point);
    inline static double SquareLength(CVector2D^ ipo0, CVector2D^ ipo1);
    inline static double SquareLength(CVector2D^ point);
    inline static double Length(CVector2D^ point);
    inline static double Distance(CVector2D^ ipo0, CVector2D^ ipo1);
    inline static double TriHeight(CVector2D^ v1, CVector2D^ v2, CVector2D^ v3);
    inline static double Dot(CVector2D^ ipo0, CVector2D^ ipo1);
    inline static double SquareCircumradius(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2 );
    inline static bool CenterCircumcircle(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2,
        CVector2D^% center);
    inline int DetDelaunay(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2,
        CVector2D^ p3);
    inline static double TriArea2D(array<double>^ v1, array<double>^ v2, array<double>^ v3);
    inline static double SqDistance2D(array<double>^ v1, array<double>^ v2);
    inline static double CVector2D::Distance2D(array<double>^ v1, array<double>^ v2);

public:
    CVector2D()
    {
        this->self = new Com::CVector2D();
    }
    CVector2D(Com::CVector2D *self)
    {
        this->self = self;
    }
    CVector2D(const CVector2D% rhs )
    {
        Com::CVector2D& rhs_instance_ = *(rhs.self);
        this->self = new Com::CVector2D(rhs_instance_); //コピーコンストラクタを使用する
    }
    CVector2D(double x, double y)
    {
        this->self = new Com::CVector2D(x, y);
    }
    ~CVector2D()
    {
        this->!CVector2D();
    }
    !CVector2D()
    {
        delete self;
    }

    // オペレータ定義
    inline CVector2D^ operator+=(CVector2D^ rhs)
    {
        const Com::CVector2D& ret_instance_ = this->self->operator+=(*(rhs->self));
        return this;
    }
    
    inline CVector2D^ operator-=(const CVector2D^ rhs)
    {
        const Com::CVector2D& ret_instance_ = this->self->operator-=(*(rhs->self));
        return this;
    }
    
    inline CVector2D^ operator*=(double scale)
    {
        const Com::CVector2D& ret_instance_ = this->self->operator*=(scale);
        return this;
    }
    
    inline CVector2D^ operator+(CVector2D^ rhs)
    {
        const Com::CVector2D& ret_instance_ = this->self->operator+(*(rhs->self));
        Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
        return gcnew CVector2D(ret);
    }
    
    inline CVector2D^ operator-(CVector2D^ rhs)
    {
        const Com::CVector2D& ret_instance_ = this->self->operator-(*(rhs->self));
        Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
        return gcnew CVector2D(ret);
    }
    
    // C++/CLI用に追加
    inline CVector2D^ operator+()
    {
        //return CVector2D::operator*(1.0, this);
        return gcnew CVector2D(*this);
    }

    inline CVector2D^ operator-()
    {
        return CVector2D::operator*(-1.0, this);
    }
    
    //! 長さを正規化する
    inline void Normalize()
    {
        this->self->Normalize();
    }
    //! 座標値に０を代入する
    inline void SetZero()
    {
        this->self->SetZero();
    }
    //! ベクトルの長さを計算する
    double Length()
    {
        return this->self->Length();
    }
    //! ベクトルの長さの２乗を計算する
    double SqLength()
    {
        return this->self->SqLength();
    }
public:
    property Com::CVector2D* Self
    {
        Com::CVector2D* get() { return this->self; }
    }
    property double x    //!< ｘ座標値
    {
        double get() { return this->self->x; }
        void set(double value) { this->self->x = value; }
    }
    property double y    //!< ｙ座標値
    {
        double get() { return this->self->y; }
        void set(double value) { this->self->y = value; }
    }
protected:
    Com::CVector2D *self;
};
  
  
//! 2D bounding box class
public ref class CBoundingBox2D
{
public:
    CBoundingBox2D()
    {
        this->self = new Com::CBoundingBox2D();
    }
    CBoundingBox2D(Com::CBoundingBox2D *self)
    {
        this->self = self;
    }
    CBoundingBox2D(double x_min0,double x_max0,  double y_min0,double y_max0)
    {
        this->self = new Com::CBoundingBox2D(x_min0, x_max0, y_min0, y_max0);
    }
    CBoundingBox2D(const CBoundingBox2D% rhs)
    {
        Com::CBoundingBox2D& rhs_instance_ = *(rhs.self);
        this->self = new Com::CBoundingBox2D(rhs_instance_); //コピーコンストラクタを使用する
    }
    ~CBoundingBox2D()
    {
        this->!CBoundingBox2D();
    }
    !CBoundingBox2D()
    {
        delete self;
    }
  
    CBoundingBox2D^ operator+=(CBoundingBox2D^ rhs)
    {
        const Com::CBoundingBox2D& ret_instance_ = this->self->operator+=(*(rhs->self));
        return this;
    }
    
    bool IsInside(CVector2D^ vec)
    {
        Com::CVector2D *vec_ = vec->Self;
        return this->self->IsInside(*vec_);
    }
    bool IsIntersectSphere(CVector2D^ vec, double radius )
    {
        Com::CVector2D *vec_ = vec->Self;
        return this->self->IsIntersectSphere(*vec_, radius);
    }
    bool IsIntersect(CBoundingBox2D^ bb_j, double clearance)
    {
        Com::CBoundingBox2D *bb_j_ = bb_j->Self;
        return this->self->IsIntersect(*bb_j_, clearance);
    }

public:
    property Com::CBoundingBox2D* Self
    {
        Com::CBoundingBox2D* get() { return this->self; }
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
    property bool isnt_empty    //!< false if there is nothing inside
    {
        bool get() { return this->self->isnt_empty; }
        void set(bool value) { this->self->isnt_empty = value; }
    }

protected:
    Com::CBoundingBox2D *self;

};  
  
////////////////////////////////////////////////////////////////

inline CVector2D^ CVector2D::operator*(double c, CVector2D^ v0)
{
    const Com::CVector2D& ret_instance_ = Com::operator*(c, *(v0->self));
    Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
    return gcnew CVector2D(ret);
}

inline CVector2D^ CVector2D::operator*(CVector2D^ v0, double c)
{
    const Com::CVector2D& ret_instance_ = Com::operator*(*(v0->self), c);
    Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
    return gcnew CVector2D(ret);
}

inline CVector2D^ CVector2D::operator+(CVector2D^ lhs, CVector2D^ rhs)
{
    const Com::CVector2D& ret_instance_ = *(lhs->self) + *(rhs->self);
    Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
    return gcnew CVector2D(ret);
}

inline CVector2D^ CVector2D::operator-(CVector2D^ lhs, CVector2D^ rhs)
{
    const Com::CVector2D& ret_instance_ = *(lhs->self) - *(rhs->self);
    Com::CVector2D *ret = new Com::CVector2D(ret_instance_);
    return gcnew CVector2D(ret);
}


////////////////////////////////////////////////////////////////

//! Area of the Triangle
inline double CVector2D::TriArea(CVector2D^ v1, CVector2D^ v2, CVector2D^ v3)
{
    Com::CVector2D *v1_ = v1->Self;
    Com::CVector2D *v2_ = v2->Self;
    Com::CVector2D *v3_ = v3->Self;
    return Com::TriArea(*v1_, *v2_, *v3_);
}
//! Area of the Triangle (3 indexes and vertex array)
inline double CVector2D::TriArea(int iv1, int iv2, int iv3, 
                      IList<CVector2D^>^ point)
{
    return TriArea(point[iv1],point[iv2],point[iv3]);
    /*
    std::vector<Com::CVector2D> point_;
    DelFEM4NetCom::ClrStub::ListToInstanceVector<CVector2D, Com::CVector2D>(point, point_);

    return Com::TriArea(iv1, iv2, iv3, point_);
    */
}


////////////////

//! 長さの２乗
inline double CVector2D::SquareLength(CVector2D^ ipo0, CVector2D^ ipo1)
{
    Com::CVector2D* ipo0_ = ipo0->Self;
    Com::CVector2D* ipo1_ = ipo1->Self;
    return Com::SquareLength(*ipo0_, *ipo1_);
}

//! 長さの２乗
inline double CVector2D::SquareLength(CVector2D^ point)
{
    Com::CVector2D* point_ = point->Self;
    return Com::SquareLength(*point_);
}
  
inline double CVector2D::Length(CVector2D^ point)
{
  return point->Length();
}
  

////////////////

//! 長さ
inline double CVector2D::Distance(CVector2D^ ipo0, CVector2D^ ipo1)
{
    Com::CVector2D* ipo0_ = ipo0->Self;
    Com::CVector2D* ipo1_ = ipo1->Self;
    return Com::Distance(*ipo0_, *ipo1_);
}

////////////////

//! ３角形の高さ
inline double CVector2D::TriHeight(CVector2D^ v1, CVector2D^ v2, CVector2D^ v3)
{
    Com::CVector2D* v1_ = v1->Self;
    Com::CVector2D* v2_ = v2->Self;
    Com::CVector2D* v3_ = v3->Self;
    return Com::TriHeight(*v1_, *v2_, *v3_);
}

////////////////

//! 内積の計算
inline double CVector2D::Dot(CVector2D^ ipo0, CVector2D^ ipo1)
{
    Com::CVector2D* ipo0_ = ipo0->Self;
    Com::CVector2D* ipo1_ = ipo1->Self;
    return Com::Dot(*ipo0_, *ipo1_);
}

//! 外接円の半径の２乗
inline double CVector2D::SquareCircumradius(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2 )
{
    Com::CVector2D* p0_ = p0->Self;
    Com::CVector2D* p1_ = p1->Self;
    Com::CVector2D* p2_ = p2->Self;
    return Com::SquareCircumradius(*p0_, *p1_, *p2_);
}

//! 外接円の中心
inline bool CVector2D::CenterCircumcircle(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2,
        CVector2D^% center)
{
    Com::CVector2D* p0_ = p0->Self;
    Com::CVector2D* p1_ = p1->Self;
    Com::CVector2D* p2_ = p2->Self;
    Com::CVector2D* center_ = center->Self; // 書き換わる
    //Com::CVector2D* center_ = new Com::CVector2D();

    bool ret = Com::CenterCircumcircle(*p0_, *p1_, *p2_, *center_);

    //center = gcnew CVector2D(center_->Self);
    return ret;
}


////////////////////////////////

//! ドロネー条件を満たすかどうか調べる
inline int CVector2D::DetDelaunay(
        CVector2D^ p0, 
        CVector2D^ p1, 
        CVector2D^ p2,
        CVector2D^ p3)
{
    Com::CVector2D* p0_ = p0->Self;
    Com::CVector2D* p1_ = p1->Self;
    Com::CVector2D* p2_ = p2->Self;
    Com::CVector2D* p3_ = p3->Self;
    return Com::DetDelaunay(*p0_, *p1_, *p2_, *p3_);
}

////////////////////////////////////////////////

inline double CVector2D::TriArea2D(array<double>^ v1, array<double>^ v2, array<double>^ v3)
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    assert(v3->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    pin_ptr<double> v3_ = &v3[0];
    return Com::TriArea2D((double *)v1_, (double *)v2_, (double *)v3_);
}

inline double CVector2D::SqDistance2D(array<double>^ v1, array<double>^ v2)
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    return Com::SqDistance2D(v1_, v2_);
}
inline double CVector2D::Distance2D(array<double>^ v1, array<double>^ v2)
{
    assert(v1->Length == C_VEC_SIZE);
    assert(v2->Length == C_VEC_SIZE);
    pin_ptr<double> v1_ = &v1[0];
    pin_ptr<double> v2_ = &v2[0];
    return Com::Distance2D(v1_, v2_);
}


//typedef NativeInstanceArrayIndexer<Com::CVector2D, DelFEM4NetCom::CVector2D, DelFEM4NetCom::CVector2D^> CVector2DArrayIndexer;
public ref class CVector2DArrayIndexer : public NativeInstanceArrayIndexer<Com::CVector2D, DelFEM4NetCom::CVector2D, DelFEM4NetCom::CVector2D^>
{
public:
    CVector2DArrayIndexer(int size_, Com::CVector2D *ptr_) : NativeInstanceArrayIndexer<Com::CVector2D, DelFEM4NetCom::CVector2D, DelFEM4NetCom::CVector2D^>(size_, ptr_)
    {
    }
    
    CVector2DArrayIndexer(const CVector2DArrayIndexer% rhs) : NativeInstanceArrayIndexer<Com::CVector2D, DelFEM4NetCom::CVector2D, DelFEM4NetCom::CVector2D^>(rhs)
    {
    }
    
    ~CVector2DArrayIndexer()
    {
        this->!CVector2DArrayIndexer();
    }
    
    !CVector2DArrayIndexer()
    {
    }
};


} // end namespace Com

#endif // DELFEM4NET_VECTOR2D_H


