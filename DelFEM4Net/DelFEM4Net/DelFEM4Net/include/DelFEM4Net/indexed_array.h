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
@brief the interface and implementation of jagged-array class (Com::CIndexedArray)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_INDEXED_ARRAY_H)
#define DELFEM4NET_INDEXED_ARRAY_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>
#include <assert.h>
#include <iostream>

#include "DelFEM4Net/stub/clr_stub.h"  //DelFEM4NetCom::UIntVectorIndexer

#include "delfem/indexed_array.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{
    
//! Jagged array
public ref class CIndexedArray
{
public:
    CIndexedArray()
    {
        this->self = new Com::CIndexedArray();
        
        initIndexser();
    }
    
    CIndexedArray(const CIndexedArray% rhs)
    {
        Com::CIndexedArray rhs_instance_ = *(rhs.self);
        this->self = new Com::CIndexedArray(rhs_instance_);

        initIndexser();
    }
    
    CIndexedArray(Com::CIndexedArray *self)
    {
        this->self = self;

        initIndexser();
    }
    
    virtual ~CIndexedArray()
    {
        this->!CIndexedArray();
    }
    
    !CIndexedArray()
    {
        delete this->self;
    }
    
    property Com::CIndexedArray * Self
    {
        Com::CIndexedArray * get() { return this->self; }
    }

    void InitializeSize(unsigned int size)
    {
        this->self->InitializeSize(size);
    }

    unsigned int Size()
    {
        return this->self->Size();
    }

    //! initialize with transpose pattern
    void SetTranspose(unsigned int size, CIndexedArray^ crs)
    {
        this->self->SetTranspose(size, *(crs->self));
    }

public:
    //! initialize as a dense matrix
    void Fill(unsigned int ncol, unsigned int nrow)
    {
        this->self->Fill(ncol, nrow);
    }

    //! sort that each sub array have incremental odering
    void Sort()
    {
        this->self->Sort();
    }

    bool CheckValid()
    {
        return this->self->CheckValid();
    }


public:
    // nativeクラスのstd::vector<unsigned int> index のアクセス用
    property DelFEM4NetCom::UIntVectorIndexer^ index
    {
        DelFEM4NetCom::UIntVectorIndexer^ get() { return this->indexIndexer; }
    }
  
    // nativeクラスのstd::vector<unsigned int> array のアクセス用
    property DelFEM4NetCom::UIntVectorIndexer^ array
    {
        DelFEM4NetCom::UIntVectorIndexer^ get() { return this->arrayIndexer; }
    }

    /*
    // nativeクラスのstd::vector<unsigned int> index のアクセス用
    property unsigned int index[int]
    {
        unsigned int get(int index)
        {
            assert(index >= 0 && index < this->self->index.size());
            return this->self->index[index];
        }
    
        void set(int index, unsigned int value)
        {
            assert(index >= 0 && index < this->self->index.size());
            this->self->index[index] = value;
        }
    }

    // nativeクラスのstd::vector<unsigned int> array のアクセス用
    property unsigned int array[int]
    {
        unsigned int get(int index)
        {
            assert(index >= 0 && index < this->self->array.size());
            return this->self->array[index];
        }
    
        void set(int index, unsigned int value)
        {
            assert(index >= 0 && index < this->self->array.size());
            this->self->array[index] = value;
        }
    }
    */

protected:
    Com::CIndexedArray *self;
    
    DelFEM4NetCom::UIntVectorIndexer^ indexIndexer;
    DelFEM4NetCom::UIntVectorIndexer^ arrayIndexer;
    
    void initIndexser()
    {
        this->indexIndexer = gcnew DelFEM4NetCom::UIntVectorIndexer(this->self->index);
        this->arrayIndexer = gcnew DelFEM4NetCom::UIntVectorIndexer(this->self->array);
    }
    
};

}    // end namespace Fem

#endif
