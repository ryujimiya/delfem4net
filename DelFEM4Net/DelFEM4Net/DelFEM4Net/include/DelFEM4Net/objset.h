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
@brief interface&implementation of ID admin template class(ObjSet
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_OBJ_SET_H)
#define DELFEM4NET_OBJ_SET_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

//#include <vector>
//#include <utility>
#include "DelFEM4Net/stub/clr_stub.h"
#include "delfem/objset.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

//! template to store objects with ID 

/*
// このクラスは、managedクラスとunmanagedクラスの相互変換に対応していません。
// 幸い、unmanagedのCom::CObjSetを引数に使用しているメソッドやpublicなメンバ変数を持つクラスはない
generic<class T>  
public ref class CObjSet
{        
public:
    CObjSet()
    {
    }
    CObjSet(const CObjSet% rhs)
    {
        Clear();
        for each (unsigned int id in rhs.m_aIndex2ID)
        {
            this->m_aIndex2ID->Add(id);
        }
        for each (int index in rhs.m_aID2Index)
        {
            this->m_aID2Index->Add(index);
        }
        for each (T obj in rhs.m_aObj)
        {
            this->m_aObj->Add(obj);
        }
    }
    ~CObjSet()
    {
        this->!CObjSet();
    }
    !CObjSet()
    {
    }
    
    void Clear()
    {
        m_aIndex2ID->Clear();
        m_aID2Index->Clear();
        m_aObj->Clear();
    }
    
    T GetObj(unsigned int id_e){
        const int ie = this->GetAryInd(id_e);
        assert( ie >= 0 && ie < (int)m_aObj->Count );
        return m_aObj[ie];
    }
    
    unsigned int AddObj(DelFEM4NetCom::Pair<unsigned int, T>^ id_obj)
    {
        unsigned int id1 = AddID(id_obj->First);
        assert( IsObjID(id1) );
        this->m_aObj->Add( id_obj->Second );
        assert( m_aIndex2ID->Count == m_aObj->Count );
        return id1;

    }

    bool IsObjID( unsigned int id_e ) 
    {
        if( id_e == 0 ) return false;
        const int ie1 = this->GetAryInd(id_e);
        if( ie1 != -1 ) return true;
        return false;
    }
    IList<unsigned int>^ GetAry_ObjID()
    {
        return this->m_aIndex2ID;
    }
    
    unsigned int GetFreeObjID()
    {
        if( m_aID2Index->Count == 0 )
        {
            return 1;
        }
        unsigned int iid;
        for (iid = 1; iid < m_aID2Index->Count; iid++) // find ID from 1
        {    
            if( m_aID2Index[iid] == -1 )
            {
                return iid;
            }
        }
        return m_aID2Index->Count;
    }
    
    IList<unsigned int>^ GetFreeObjID(unsigned int size)
    {
        IList<unsigned int>^ res = gcnew List<unsigned int>(size);

        unsigned int isize = 0;
        assert( m_aID2Index->Count != 1 );
        if ( m_aID2Index->Count == 0 )
        {
            for(;;){
                res[isize] = isize + 1;
                isize++;
                if(isize == size)
                {
                    return res;
                }
            }
        }
        for(unsigned int iid = 1; iid < m_aID2Index->Count;iid++)
        {
            if( m_aID2Index[iid] == -1 )
            {
                res[isize] = iid;
                isize++;
                if(isize == size)
                {
                    return res;
                }
            }
        }
        unsigned int i;
        for(i=0;;i++)
        {
            res[isize] = m_aID2Index->Count + i;
            isize++;
            if(isize == size)
            {
                return res;
            }
        }
        return res;
    }    
    bool DeleteObj(unsigned int id_e)
    {
        if( !this->IsObjID(id_e) )
        {
            return false;
        }
        const int ie = this->GetAryInd(id_e);
        assert( ie >= 0 && ie < (int)m_aObj->Count );
        m_aObj->RemoveAt(ie);
        assert( ie >= 0 && ie < (int)m_aIndex2ID->Count );
        m_aIndex2ID->RemoveAt(ie);
        for(unsigned int i = 0; i < m_aID2Index->Count; i++)
        {
            m_aID2Index[i] = -1;
        }
        for(unsigned int iie = 0 ; iie < m_aIndex2ID->Count; iie++)
        {
            unsigned int id_e = m_aIndex2ID[iie];
            assert( id_e < m_aID2Index->Count );
            m_aID2Index[id_e] = iie;
        }
        return true;
    }
    void Reserve(unsigned int size)
    {
        // do nothing for List (std::vector 's method)
    }

    unsigned int MaxID()
    {
        return m_aID2Index->Count;
    }
    
protected:
    unsigned int AddID(const int% tmp_id)
    {
        unsigned int id1 = 0;
        if( !IsObjID( tmp_id ) && tmp_id>0 && tmp_id<255 )
        {
            id1 = tmp_id;
            if( m_aID2Index->Count <= id1 )
            {
                for (int i = m_aID2Index->Count; i < (id1 + 1); i++)
                {
                    m_aID2Index->Add( -1 );
                }
                m_aID2Index[id1] = m_aIndex2ID->Count;
            }
            m_aIndex2ID->Add(id1);
            m_aID2Index[id1] = m_aIndex2ID->Count - 1;
        }
        else
        {
            id1 = this->GetFreeObjID();
            assert( !IsObjID(id1) );
            if( m_aID2Index->Count <= id1 )
            {
                for (int i = m_aID2Index->Count; i < (id1 + 1); i++)
                {
                    m_aID2Index->Add( -1 );
                }
                m_aID2Index[id1] = m_aIndex2ID->Count;
            }
            m_aIndex2ID->Add(id1);
            m_aID2Index[id1] = m_aIndex2ID->Count - 1;
        }
        assert( IsObjID(id1) );
        return id1;
    }
    int GetAryInd(unsigned int id_e)
    {
        int ie1;
        if( m_aID2Index->Count <= id_e )
        {
            ie1 = -1;
        }
        else
        {
            ie1 = m_aID2Index[id_e];
        }
        return ie1;
    }

protected:
    // managedのリストを格納
    IList<unsigned int>^ m_aIndex2ID;
    IList<int>^ m_aID2Index;
    IList<T>^ m_aObj;
};
*/

} // namespace DelFEM4NetCom

#endif
