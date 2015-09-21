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
@brief the interface of node array class (Fem::Field::CNodeAry)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_NODE_ARY_H)
#define DELFEM4NET_NODE_ARY_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#ifndef for 
#define for if(0); else for
#endif

#include <iostream>
#include <cassert>

#include "DelFEM4Net/stub/clr_stub.h" // DelFEM4NetCom::Pair
#include "DelFEM4Net/complex.h"
//#include "DelFEM4Net/objset.h"
#include "DelFEM4Net/elem_ary.h"    // remove this dependency in future

#include "delfem/node_ary.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

// the temp class declearation
namespace DelFEM4NetMatVec
{

    ref class CVector_Blk;
    ref class CZVector_Blk;
    ref class CBCFlag;
}

namespace DelFEM4NetFem
{
namespace Field
{

/*! 
@brief class which contains nodes value (coordinte,displacement,temparature....etc )
@ingroup Fem
*/
public ref class CNodeAry
{
public:
    //! Class for Node Segment (Holding Data of Node)
    ref class CNodeSeg
    {
    public:
        /*
        CNodeSeg()
        {
            this->self = new Fem::Field::CNodeAry::CNodeSeg();
        }
        */
        CNodeSeg(const CNodeSeg% rhs)
        {
            const Fem::Field::CNodeAry::CNodeSeg& rhs_instance_ = *(rhs.self);
            this->self = new Fem::Field::CNodeAry::CNodeSeg(rhs_instance_);
        }
        
        CNodeSeg(Fem::Field::CNodeAry::CNodeSeg *self)
        {
            this->self = self;
        }
        
        CNodeSeg(unsigned int len, String^ name)
        {
            std::string name_ = DelFEM4NetCom::ClrStub::StringToStd(name);
            
            this->self = new Fem::Field::CNodeAry::CNodeSeg(len, name_);
            
        }
        
        ~CNodeSeg()
        {
            this->!CNodeSeg();
        }
        
        !CNodeSeg()
        {
            delete this->self;
        }

        property Fem::Field::CNodeAry::CNodeSeg * Self
        {
            Fem::Field::CNodeAry::CNodeSeg * get() { return this->self; }
        }

        unsigned int Length()    //!< The length of value
        {
            return this->self->Length();
        }
        
        unsigned int Size()     //!< The number of nodes
        {
            return this->self->Size();
        }
        
        inline void GetValue(unsigned int inode, [Out] array<double>^% aVal )    //!< get value from node
        {
            const unsigned int len = this->self->Length();
            aVal = gcnew array<double>(len);
            
            pin_ptr<double> ptr = &aVal[0];
            double *aVal_ = (double *)ptr;
            
            this->self->GetValue(inode, aVal_);
        }

        void GetValue(unsigned int inode, [Out] array<DelFEM4NetCom::Complex^>^% aVal )    //!< get complex value from node
        {
            const unsigned int n = this->self->Length()/2;
            aVal = gcnew array<DelFEM4NetCom::Complex^>(n);
            
            Com::Complex * aVal_ = new Com::Complex[n];
            
            this->self->GetValue(inode, aVal_);
            
            for (unsigned int i = 0; i < n; i++)
            {
                Com::Complex *c_ = new Com::Complex(aVal_[i]); // 移し替える
                aVal[i] = gcnew DelFEM4NetCom::Complex(c_);
            }

            delete[] aVal_;
        }

        inline void SetValue(unsigned int inode, unsigned int idofns, double val )    //!< set value to node 
        {
            this->self->SetValue(inode, idofns, val);
        }

        inline void AddValue(unsigned int inode, unsigned int idofns, double val )    //!< add value to node
        {
            this->self->AddValue(inode, idofns, val);
        }
        
        void SetZero()    //!< set zero to all value
        {
            this->self->SetZero();
        }
        
    protected:
        Fem::Field::CNodeAry::CNodeSeg *self;

    };

public:
    CNodeAry()
    {
        this->self = new Fem::Field::CNodeAry();
    }
    
    CNodeAry(bool isCreateInstance)  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    {
        assert(isCreateInstance == false);
        // インスタンスを生成しない
        this->self = NULL;
    }
    
    CNodeAry(const CNodeAry% rhs)
    {
        Fem::Field::CNodeAry rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::CNodeAry(rhs_instance_);
    }
    
    CNodeAry(unsigned int size)
    {
        this->self = new Fem::Field::CNodeAry(size);
    }
    
    CNodeAry(Fem::Field::CNodeAry *self)
    {
        this->self = self;
    }
    
    virtual ~CNodeAry()
    {
        this->!CNodeAry();
    }
    
    !CNodeAry()
    {
        delete this->self;
    }
    
    property Fem::Field::CNodeAry * Self
    {
        Fem::Field::CNodeAry * get() { return this->self; }
    }

    bool ClearSegment();

    ////////////////////////////////
    // Get Methods

    unsigned int Size()  //!< number of nodes
    {
        return this->self->Size();
    }

    //! Get a not-used ID of Node Segment
    unsigned int GetFreeSegID()
    {
        return this->self->GetFreeSegID();
    }
    
    //! Get not-used num IDs of Node Segment
    IList<unsigned int>^ GetFreeSegID(unsigned int num)
    {
        const std::vector<unsigned int>& vec = this->self->GetFreeSegID(num);
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }

    //! Check if id_ns is whether ID of Node Segment or not
    bool IsSegID( unsigned int id_ns )
    {
        return this->self->IsSegID(id_ns);
    }

    IList<unsigned int>^ GetAry_SegID()
    {
        const std::vector<unsigned int>& vec = this->self->GetAry_SegID();
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }

    //! Get Node Segment (const)
    CNodeAry::CNodeSeg^ GetSeg(unsigned int id_ns)
    {        
        const Fem::Field::CNodeAry::CNodeSeg& ret_instance_ = this->self->GetSeg(id_ns);
        Fem::Field::CNodeAry::CNodeSeg *ret = new Fem::Field::CNodeAry::CNodeSeg(ret_instance_);
        
        CNodeAry::CNodeSeg^ retManaged = gcnew CNodeAry::CNodeSeg(ret);
        
        return retManaged;
    }

    //! 節点セグメントの値をvecに代入する
    bool GetValueFromNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec)  // vec: IN/OUT
    {
        unsigned int ioffset = 0;
        return GetValueFromNodeSegment(id_ns, vec, ioffset);
    }
    bool GetValueFromNodeSegment(unsigned int id_ns, [Out] DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset); //vec: IN/OUT
     
    bool AddValueFromNodeSegment(double alpha, unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec) // vec: IN/OUT
    {
        unsigned int ioffset = 0;
        return AddValueFromNodeSegment(alpha, id_ns, vec, ioffset);
    }
    bool AddValueFromNodeSegment(double alpha, unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset); // vec: IN/OUT

    ////////////////
    // 参照要素セグメント追加メソッド

    void AddEaEs( DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes );
    IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ GetAryEaEs();
    void SetIncludeEaEs_InEaEs( DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_included,
                                DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_container );
    bool IsIncludeEaEs_InEaEs( DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_inc,
                                DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes_in );
    IList< DelFEM4NetCom::Pair<unsigned int, unsigned int>^ >^ GetAry_EaEs_Min();
    unsigned int IsContainEa_InEaEs(DelFEM4NetCom::Pair<unsigned int, unsigned int>^ eaes, unsigned int id_ea);


    ////////////////
    // 変更・削除・追加メソッド

    IList<int>^ AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec ); //!< 初期化しない
    IList<int>^ AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec, double val);  //!< 値valで初期化
    IList<int>^ AddSegment( IList< DelFEM4NetCom::Pair<unsigned int, CNodeAry::CNodeSeg^>^ >^ seg_vec, IList<double>^ val_vec);  //!< ベクトルvalで初期化

    ////////////////
    // 値の変更メソッド

    //! set value of vector to the node segment
    bool SetValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec) // Segmentにvecをセットする
    {
        unsigned int ioffset = 0;
        return SetValueToNodeSegment(id_ns, vec, ioffset);
    }
    bool SetValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, unsigned int ioffset); // Segmentにvecをセットする

    //! 境界条件設定に使われる。
    bool SetValueToNodeSegment(DelFEM4NetFem::Field::CElemAry^ ea, unsigned int id_es, unsigned int id_ns, unsigned int idofns, double val)
    {
        return this->self->SetValueToNodeSegment(*(ea->Self), id_es, id_ns, idofns, val);
    }

    
    bool AddValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, double alpha)
    {
        unsigned int ioffset = 0;
        return AddValueToNodeSegment(id_ns, vec, alpha, ioffset);
    };
    bool AddValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CVector_Blk^ vec, double alpha,  unsigned int ioffset); //!< Segmentにalpha倍されたvecを加える
    bool AddValueToNodeSegment(unsigned int id_ns, DelFEM4NetMatVec::CZVector_Blk^ vec, double alpha); //!< Segmentにalpha倍されたvecを加える
    bool AddValueToNodeSegment(unsigned int id_ns_to, unsigned int id_ns_from, double alpha );    //!< ns_toへalpha倍されたns_fromを加える


    //! load from file
    int InitializeFromFile(String^ file_name, long% offset);
    //! write to file
    int WriteToFile(String^ file_name, long% offset, unsigned int id );
    /*nativeのライブラリで実装されていない？
    int DumpToFile_UpdatedValue(String^ file_name, long% offset, unsigned int id );
    */

protected:
    Fem::Field::CNodeAry *self;

};

// ポインタ操作用クラス
public ref class CNodeAryPtr : public CNodeAry
{
public:
    CNodeAryPtr(Fem::Field::CNodeAry *baseSelf) : CNodeAry(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CNodeAryPtr(const CNodeAryPtr% rhs) : CNodeAry(false)
    {
        this->self = rhs.self;
    }

public:
    ~CNodeAryPtr()
    {
        this->!CNodeAryPtr();
    }

    !CNodeAryPtr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};


}    // end namespace field
}    // end namespace Fem

#endif // !defined(NODE_ARY_H)
