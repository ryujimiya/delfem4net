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
@brief Interfaces define the geometry of 2d cad elements
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_ELEM_3D_H)
#define DELFEM4NET_CAD_ELEM_3D_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>
#include <assert.h>
#include <iostream> // needed only in debug

#include "DelFEM4Net/vector3d.h"
#include "DelFEM4Net/vector2d.h"
#include "DelFEM4Net/cad/cad_elem2d.h"

#include "DelFEM4Net/stub/clr_stub.h" // Pair
#include "delfem/cad/cad_elem3d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////////////////////

namespace DelFEM4NetCad{

/*!
@addtogroup CAD
*/
//!@{

//! 2dim loop class
public ref class CLoop3D{
public:
    CLoop3D()
    {
        this->self = new Cad::CLoop3D();
    }
    
    CLoop3D(CLoop3D^ rhs)
    {
        Cad::CLoop3D *rhs_ = rhs->Self;
        this->self = new Cad::CLoop3D(*rhs_); // コピーコンストラクタを使用
    }
    
    CLoop3D(Cad::CLoop3D *self)
    {
        this->self = self;
    }
    
    CLoop3D(DelFEM4NetCom::CVector3D^ o, DelFEM4NetCom::CVector3D^ n, DelFEM4NetCom::CVector3D^ x )
    {
        this->self = new Cad::CLoop3D(*(o->Self), *(n->Self), *(x->Self));
    }

    ~CLoop3D()
    {
        this->!CLoop3D();
    }
    
    !CLoop3D()
    {
        delete this->self;
    }

    property Cad::CLoop3D * Self
    {
        Cad::CLoop3D * get() { return this->self; }
    }

    DelFEM4NetCom::CVector2D^ Project(DelFEM4NetCom::CVector3D^ p) 
    {
        Com::CVector2D *ret = new Com::CVector2D();
        *ret = this->self->Project(*(p->Self));
        
        DelFEM4NetCom::CVector2D^ retManaged = gcnew DelFEM4NetCom::CVector2D(ret);
        return retManaged;
    }
    
    DelFEM4NetCom::CVector3D^ UnProject(DelFEM4NetCom::CVector2D^ p)
    {
        Com::CVector3D *ret = new Com::CVector3D();
        *ret = this->self->UnProject(*(p->Self));
        
        DelFEM4NetCom::CVector3D^ retManaged = gcnew DelFEM4NetCom::CVector3D(ret);
        return retManaged;
    }
  
    ////
    DelFEM4NetCom::CVector3D^ GetNearestPoint(DelFEM4NetCom::CVector3D^ p)
    {
        Com::CVector3D *ret = new Com::CVector3D();
        *ret = this->self->GetNearestPoint(*(p->Self));
        
        DelFEM4NetCom::CVector3D^ retManaged = gcnew DelFEM4NetCom::CVector3D(ret);
        return retManaged;
    }
    
    int NumIntersecRay(DelFEM4NetCom::CVector3D^ org, DelFEM4NetCom::CVector3D^ dir)
    {
        return this->self->NumIntersecRay(*(org->Self), *(dir->Self));
    }

    DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox()
    {
        Com::CBoundingBox3D *ret = new Com::CBoundingBox3D();
        *ret = this->self->GetBoundingBox();
        
        DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
        return retManaged;
    }
    
public:
    // this is when this loop is planer
    property DelFEM4NetCom::CVector3D^ org
    {
        DelFEM4NetCom::CVector3D^ get()
        {
            Com::CVector3D *value_ = new Com::CVector3D();
            
            *value_ = this->self->org;
            
            DelFEM4NetCom::CVector3D^ value = gcnew DelFEM4NetCom::CVector3D(value_);
            return value;
        }
        void set(DelFEM4NetCom::CVector3D^ value)
        {
            Com::CVector3D *value_ = value->Self;
            this->self->org = *value_;
        }
    }

    property DelFEM4NetCom::CVector3D^ normal
    {
        DelFEM4NetCom::CVector3D^ get()
        {
            Com::CVector3D *value_ = new Com::CVector3D();
            
            *value_ = this->self->normal;
            
            DelFEM4NetCom::CVector3D^ value = gcnew DelFEM4NetCom::CVector3D(value_);
            return value;
        }
        void set(DelFEM4NetCom::CVector3D^ value)
        {
            Com::CVector3D *value_ = value->Self;
            this->self->normal = *value_;
        }
    }

    property DelFEM4NetCom::CVector3D^ dirx
    {
        DelFEM4NetCom::CVector3D^ get()
        {
            Com::CVector3D *value_ = new Com::CVector3D();
            
            *value_ = this->self->dirx;
            
            DelFEM4NetCom::CVector3D^ value = gcnew DelFEM4NetCom::CVector3D(value_);
            return value;
        }
        void set(DelFEM4NetCom::CVector3D^ value)
        {
            Com::CVector3D *value_ = value->Self;
            this->self->dirx = *value_;
        }
    }

    property IList< DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ >^ aEdge
    {
        IList< DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ >^ get()
        {
            const std::vector< std::pair<Cad::CEdge2D, bool>>& vec = this->self->aEdge;
            
            IList< DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ >^ list = gcnew List< DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ >();
            if (vec.size() > 0)
            {
                for (std::vector< std::pair<Cad::CEdge2D, bool>>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
                {
                    const std::pair<Cad::CEdge2D, bool>& pair_ = *itr;
                    DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ pair = gcnew DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>();
                    pair->First = gcnew DelFEM4NetCad::CEdge2D(new Cad::CEdge2D(pair_.first)); // unmanagedインスタンスからコピーコンストラクタで新たなunmanagedインスタンスを作成、それをもとにmanaged作成
                    pair->Second = pair_.second;
                    
                    list->Add(pair);
                }
            }
            return list;
        }
        
        void set(IList< DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ >^ list)
        {
            std::vector< std::pair<Cad::CEdge2D, bool> >& vec = this->self->aEdge;
            
            vec.clear();
            if (list->Count > 0)
            {
                for each(DelFEM4NetCom::Pair<DelFEM4NetCad::CEdge2D^, bool>^ pair in list)
                {
                    std::pair<Cad::CEdge2D, bool> pair_(*(pair->First->Self), pair->Second);
                    vec.push_back(pair_);
                }
            }
        }
    }

    property IList<unsigned int>^ aIndEdge
    {
        IList<unsigned int>^ get()
        {
            const std::vector<unsigned int>& vec = this->self->aIndEdge;
            
            IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
            return list;
        }
        
        void set(IList<unsigned int>^ list)
        {
            std::vector<unsigned int>& vec = this->self->aIndEdge;
            
            DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(list, vec);
        }
    }

    property DelFEM4NetCom::CBoundingBox3D^ bb_
    {
        DelFEM4NetCom::CBoundingBox3D^ get()
        {
            const Com::CBoundingBox3D& unmanaged_instance = this->self->bb_;

            Com::CBoundingBox3D * unmanaged = new Com::CBoundingBox3D(unmanaged_instance);
            
            DelFEM4NetCom::CBoundingBox3D^ managed = gcnew DelFEM4NetCom::CBoundingBox3D(unmanaged);
            return managed;
        }
        
        void set(DelFEM4NetCom::CBoundingBox3D^ managed)
        {
            this->self->bb_ = *(managed->Self);
        }
    }
    
protected:
    Cad::CLoop3D *self;
    
};
  

//! 2dim edge
public ref class CEdge3D
{
public:
    CEdge3D()
    {
        this->self = new Cad::CEdge3D();
    }
    
    CEdge3D(CEdge3D^ rhs)
    {
        Cad::CEdge3D *rhs_ = rhs->Self;
        this->self = new Cad::CEdge3D(*rhs_); // コピーコンストラクタを使用
    }
    
    CEdge3D(Cad::CEdge3D *self)
    {
        this->self = self;
    }
    
    ~CEdge3D()
    {
        this->!CEdge3D();
    }
    
    !CEdge3D()
    {
        delete this->self;
    }

    property Cad::CEdge3D * Self
    {
        Cad::CEdge3D * get() { return this->self; }
    }

public:
    property unsigned int id_v_s    //!< start vertex
    {
        unsigned int get() { return this->self->id_v_s; }
        void set(unsigned int value) { this->self->id_v_s = value; }
    }

    property unsigned int id_v_e    //!< start vertex
    {
        unsigned int get() { return this->self->id_v_e; }
        void set(unsigned int value) { this->self->id_v_e = value; }
    }

    property DelFEM4NetCom::CVector3D^ po_s
    {
        DelFEM4NetCom::CVector3D^ get() { return gcnew DelFEM4NetCom::CVector3D(new Com::CVector3D(this->self->po_s)); }
        void set(DelFEM4NetCom::CVector3D^ value) { this->self->po_s = *value->Self; }
    }

    property DelFEM4NetCom::CVector3D^ po_e
    {
        DelFEM4NetCom::CVector3D^ get() { return gcnew DelFEM4NetCom::CVector3D(new Com::CVector3D(this->self->po_e)); }
        void set(DelFEM4NetCom::CVector3D^ value) { this->self->po_e = *value->Self; }
    }
    
protected:
    Cad::CEdge3D *self;
    
};

//! ２次元幾何頂点クラス
public ref class CVertex3D
{
public:
    /*
    CVertex3D()
    {
        this->self = new Cad::CVertex3D();
    }
    */
    
    CVertex3D(CVertex3D^ rhs)
    {
        Cad::CVertex3D *rhs_ = rhs->Self;
        this->self = new Cad::CVertex3D(*rhs_); // コピーコンストラクタを使用
    }
    
    CVertex3D(Cad::CVertex3D *self)
    {
        this->self = self;
    }
    
    CVertex3D(DelFEM4NetCom::CVector3D^ point)
    {
        this->self = new Cad::CVertex3D(*(point->Self));
    }
    
    ~CVertex3D()
    {
        this->!CVertex3D();
    }
    
    !CVertex3D()
    {
        delete this->self;
    }

    property Cad::CVertex3D * Self
    {
        Cad::CVertex3D * get() { return this->self; }
    }

public:
    property DelFEM4NetCom::CVector3D^ point   //!< coordinate
    {
        DelFEM4NetCom::CVector3D^ get() { return gcnew DelFEM4NetCom::CVector3D(new Com::CVector3D(this->self->point)); }
        void set(DelFEM4NetCom::CVector3D^ value) { this->self->point = *value->Self; }
    }

protected:
    Cad::CVertex3D *self;
};


//! @}
}

#endif
