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
@brief 複素数クラス(Com::Complex)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET__COMPLEX_H)
#define DELFEM4NET__COMPLEX_H

#include <math.h>

#include "delfem/complex.h"
#include "DelFEM4Net/stub/clr_stub.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

//! 複素数クラス
public ref class Complex
{
public:
    inline static /*explicit*/ operator Complex^ (double);
    inline static Complex^ operator +(Complex^ rhs, Complex^ lhs);
    inline static Complex^ operator -(Complex^ rhs, Complex^ lhs);
    inline static Complex^ operator *(Complex^ rhs, Complex^ lhs);
    inline static Complex^ operator *(double, Complex^);
    inline static Complex^ operator *(Complex^, double);
    inline static Complex^ operator /(Complex^ rhs, Complex^ lhs);
    inline static Complex^ operator /(double, Complex^);
    inline static Complex^ operator /(Complex^, double);
    inline static Complex^ operator -(Complex^);
    inline static Complex^ Conjugate(Complex^);

    inline static double SquaredNorm(Complex^ c);
    inline static double Norm(Complex^ c);
    inline static void SetZero(Complex^ c);
    inline Complex^ Complex::InnerProduct(Complex^ lhs, Complex^ rhs);
public:
    Complex()
    {
        this->self = new Com::Complex();
    }
    
    Complex(const Complex% rhs)
    {
        Com::Complex rhs_instance_ = *(rhs.self);
        this->self = new Com::Complex(rhs_instance_);
    }
    
    Complex(double real, double imag )    //!< 実部，虚部で初期化
    {
        this->self = new Com::Complex(real, imag);
    }

    Complex(double real)
    {
        this->self = new Com::Complex(real);
    }
    
    Complex(Com::Complex *self)
    {
        this->self = self;
    }
    
    virtual ~Complex()
    {
        this->!Complex();
    }
    
    !Complex()
    {
        delete this->self;
    }
    
    property Com::Complex * Self
    {
        Com::Complex * get() { return this->self; }
    }


    inline Complex^ operator=(double);    //!< 代入

    inline Complex^ operator *= ( double );    //!< スカラー倍
    inline Complex^ operator *= ( Complex^ );    //!< 複素数との積
    inline Complex^ operator += ( Complex^ );    //!< 加える
    inline Complex^ operator -= ( Complex^ );    //!< 引く

    property double Real
    {
        double get() { return this->self->Real(); }    //!< Get Real Part
    }
    
    property double Imag
    {
        double get() { return this->self->Imag(); }    //!< Get Imaginary Part
    }

protected:
    Com::Complex *self;
    
};

inline Complex::operator Complex^ (double d)
{
    return gcnew Complex(d);
}
inline Complex^ Complex::operator = (double d)
{
    const Com::Complex&ret = this->self->operator = (d);
    
    return this;
}

inline Complex^ Complex::operator *= ( double d )
{
    const Com::Complex&ret = this->self->operator *= (d);
    
    return this;
}

inline Complex^ Complex::operator *= ( Complex^ c )
{
    const Com::Complex&ret = this->self->operator *= (*(c->self));
    
    return this;
}

inline Complex^ Complex::operator += ( Complex^ c )
{
    const Com::Complex&ret = this->self->operator += (*(c->self));
    
    return this;
}

inline Complex^ Complex::operator -= ( Complex^ c )
{
    const Com::Complex&ret = this->self->operator -= (*(c->self));
    
    return this;
}

///////////////////////////////////////
// static functions

inline Complex^ Complex::operator -(Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator -(*(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::Conjugate(Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::Conjugate(*(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator +(Complex^ lhs, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator +(*(lhs->Self), *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator -(Complex^ lhs, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator -(*(lhs->Self), *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator *(Complex^ lhs, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator *(*(lhs->Self), *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator *(double d, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator *(d, *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator *(Complex^ lhs, double d)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator *(*(lhs->Self), d);
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator /(Complex^ lhs, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator /(*(lhs->Self), *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator /(double d, Complex^ c)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator /(d, *(c->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline Complex^ Complex::operator /(Complex^ c, double d)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::operator /(*(c->Self), d);
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

inline double Complex::SquaredNorm(Complex^ c)
{ 
    return Com::SquaredNorm(*(c->Self));
}

inline double Complex::Norm(Complex^ c)
{
    return Com::Norm(*(c->Self));
}

inline void Complex::SetZero(Complex^ c)
{
    return Com::SetZero(*(c->Self));
}

inline Complex^ Complex::InnerProduct(Complex^ lhs, Complex^ rhs)
{
    Com::Complex *ret = new Com::Complex();
    *ret = Com::InnerProduct(*(lhs->Self), *(rhs->Self));
    
    Complex^ retManaged = gcnew Complex(ret);

    return retManaged;
}

//typedef NativeInstanceArrayIndexer<Com::Complex, DelFEM4NetCom::Complex, DelFEM4NetCom::Complex^> ComplexArrayIndexer;
public ref class ComplexArrayIndexer : public NativeInstanceArrayIndexer<Com::Complex, DelFEM4NetCom::Complex, DelFEM4NetCom::Complex^>
{
public:
    ComplexArrayIndexer(int size_, Com::Complex *ptr_) : NativeInstanceArrayIndexer<Com::Complex, DelFEM4NetCom::Complex, DelFEM4NetCom::Complex^>(size_, ptr_)
    {
    }
    
    ComplexArrayIndexer(const ComplexArrayIndexer% rhs) : NativeInstanceArrayIndexer<Com::Complex, DelFEM4NetCom::Complex, DelFEM4NetCom::Complex^>(rhs)
    {
    }
    
    ~ComplexArrayIndexer()
    {
        this->!ComplexArrayIndexer();
    }
    
    !ComplexArrayIndexer()
    {
    }
};

}

#endif
