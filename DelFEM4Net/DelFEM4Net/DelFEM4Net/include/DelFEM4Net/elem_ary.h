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
@brief interface of element array class (Fem::Field::CElemAry)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#if !defined(DELFEM4NET_ELEM_ARY_H)
#define DELFEM4NET_ELEM_ARY_H

#include <vector>
#include <assert.h>
#include <cstdlib> //(abs)
#include <cstring> //(strspn, strlen, strncmp, strtok)

//#include "DelFEM4Net/objset.h"
#include "DelFEM4Net/indexed_array.h"
#include "DelFEM4Net/stub/clr_stub.h" // DelFEM4NetCom::Pair

#include "delfem/elem_ary.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{

//! type of element segment
public enum class ELSEG_TYPE
{ 
    CORNER=1,    //!< corner node
    EDGE=2,        //!< edge node
    BUBBLE=4    //!< in the element node
};

// this integet index will be used in "elem_ary.cpp". Please don't change without consideration.
// ここだけは別のヘッダファイルに移したほうがいいかも．このIndexだけ欲しいクラス(drawer_field.hとか)のために
//! type of element
public enum class ELEM_TYPE
{
    ELEM_TYPE_NOT_SET=0,
    POINT=1,    //!< point element
    LINE=2,        //!< line element
    TRI=3,         //!< triangle element
    QUAD=4,     //!< quadratic element
    TET=5,         //!< tetrahedra element
    HEX=6         //!< hexagonal element
};

/*! 
@brief class of Element Array
@ingroup Fem
*/
public ref class CElemAry
{
    
public:
    //! 要素セグメント．要素の場所(Corner,Bubble..etc)ごとの節点番号を取得するインターフェース関数
    ref class CElemSeg
    {
    public:
        /*
        CElemSeg()
        {
            this->self = new Fem::Field::CElemAry::CElemSeg();  // デフォルトコンストラクタなし
        }
        */
        
        CElemSeg(const CElemSeg% rhs)
        {
            Fem::Field::CElemAry::CElemSeg rhs_instance_ = *(rhs.self);
            // shallow copyになるので問題あり? --> ポインタのメンバ変数はインスタンスを生成していないので問題ない
            this->self = new Fem::Field::CElemAry::CElemSeg(rhs_instance_);
            assert(false);
        }
        
        CElemSeg(Fem::Field::CElemAry::CElemSeg *self)
        {
            this->self = self;
        }

        CElemSeg(unsigned int id_na, ELSEG_TYPE elseg_type)
        {
            Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
            this->self = new Fem::Field::CElemAry::CElemSeg(id_na, elseg_type_);
        }
        
        ~CElemSeg()
        {
            this->!CElemSeg();
        }
        
        !CElemSeg()
        {
            delete this->self;
        }
        
        property Fem::Field::CElemAry::CElemSeg * Self
        {
            Fem::Field::CElemAry::CElemSeg *get() { return this->self; }
        }

        unsigned int GetMaxNoes()    //!< ノード番号の一番大きなものを得る（このnoesを格納するためには一つ大きな配列が必要なので注意）
        {
            return this->self->GetMaxNoes();
        }

        unsigned int Length()    //!< return the node size per elem seg  ( will be renamed to Length() );
        {
            return this->self->Length();
        }

        unsigned int Size()      //!< get the number of elements ( will be renamed to Size() )
        {
            return this->self->Size();
        }

        unsigned int GetIdNA()        //!< get ID of node array this element segment refers
        {
            return this->self->GetIdNA();
        }

        ELSEG_TYPE GetElSegType()    //!< 要素セグメントタイプ(Fem::Field::CORNER,Fem::Field::BUBBLE,Fem::Field::EDGE)を得る
        {
            Fem::Field::ELSEG_TYPE elseg_type_ = this->self->GetElSegType();
            return static_cast<DelFEM4NetFem::Field::ELSEG_TYPE>(elseg_type_);
        }

        //! get node indexes of element (ielem)
        void GetNodes(unsigned int ielem, array<unsigned int>^ noes )
        {
            pin_ptr<unsigned int> ptr = &noes[0];
            unsigned int *noes_ = (unsigned int *)ptr;
            this->self->GetNodes(ielem, noes_);
        }

        //! 節点番号を設定
        void SetNodes(unsigned int ielem, unsigned int idofes, int ino)
        {
            this->self->SetNodes(ielem, idofes, ino);
        }

        //! 各場所(Corner,Bubble)に定義されている要素節点の数を出す
        static unsigned int GetLength(ELSEG_TYPE elseg_type, ELEM_TYPE elem_type)
        {
            Fem::Field::ELSEG_TYPE elseg_type_ = static_cast<Fem::Field::ELSEG_TYPE>(elseg_type);
            Fem::Field::ELEM_TYPE elem_type_ = static_cast<Fem::Field::ELEM_TYPE>(elem_type);
            return Fem::Field::CElemAry::CElemSeg::GetLength(elseg_type_, elem_type_);
        }

        //! 要素の次元を取得する
        static unsigned int GetElemDim(ELEM_TYPE elem_type)
        {
            Fem::Field::ELEM_TYPE elem_type_ = static_cast<Fem::Field::ELEM_TYPE>(elem_type);
            return Fem::Field::CElemAry::CElemSeg::GetElemDim(elem_type_);
        }
    
    protected:
        Fem::Field::CElemAry::CElemSeg *self;
    };

public:
    CElemAry()
    {
        this->self = new Fem::Field::CElemAry();
    }
    
    CElemAry(bool isCreateInstance)  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    {
        assert(isCreateInstance == false);
        // インスタンスを生成しない
        this->self = NULL;
    }
    
    CElemAry(const CElemAry% rhs)
    {
        Fem::Field::CElemAry rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::CElemAry(rhs_instance_);
    }
    
    CElemAry(unsigned int nelem, ELEM_TYPE elem_type)
    {
        this->self = new Fem::Field::CElemAry(nelem, static_cast<Fem::Field::ELEM_TYPE>(elem_type));
    }
    
    CElemAry(Fem::Field::CElemAry *self)
    {
        this->self = self;
    }
    
    ~CElemAry()
    {
        this->!CElemAry();
    }
    
    !CElemAry()
    {
        delete this->self;
    }

    property Fem::Field::CElemAry * Self
    {
        Fem::Field::CElemAry * get() { return this->self; }
    }

    bool IsSegID( unsigned int id_es )   //!< Check if there are elem segment with ID:id_es
    {
        return this->self->IsSegID(id_es);
    }

    IList<unsigned int>^ GetAry_SegID() //!< 要素セグメントIDの配列を得る関数
    {
        const std::vector<unsigned int>& vec = this->self->GetAry_SegID();
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }
    unsigned int GetFreeSegID()    //!< 使われていない要素セグメントIDを取得する関数
    {
        return this->self->GetFreeSegID();
    }
  
    CElemAry::CElemSeg^ GetSeg(unsigned int id_es)
    {        
        const Fem::Field::CElemAry::CElemSeg& ret_instance = this->self->GetSeg(id_es);

        Fem::Field::CElemAry::CElemSeg *ret = new Fem::Field::CElemAry::CElemSeg(ret_instance);
        
        CElemAry::CElemSeg^ retManaged = gcnew CElemAry::CElemSeg(ret);

        return retManaged;
    }

    virtual DelFEM4NetFem::Field::ELEM_TYPE ElemType()    //!< get type of element
    {
        return static_cast<DelFEM4NetFem::Field::ELEM_TYPE>(this->self->ElemType());
    }
    virtual unsigned int Size()    //!< get number of elements
    {
        return this->self->Size();
    }

    //! make CRS data (for the off-diaglnal block)
    virtual bool MakePattern_FEM(
        unsigned int id_es0, unsigned int id_es1, 
        DelFEM4NetCom::CIndexedArray^ crs );  // [IN/OUT] crs

    //! make CRS data (for the diaglnal)
    virtual bool MakePattern_FEM(
        int id_es0, 
        DelFEM4NetCom::CIndexedArray^ crs ); // [IN/OUT] crs

    // make edge ( for 2nd order interporation )
    virtual bool MakeEdge(unsigned int id_es_co, [Out] unsigned int% nedge, [Out] IList<unsigned int>^% edge_ary);
    
    // make map from elem to edge
    virtual bool MakeElemToEdge(unsigned int id_es_corner, 
        unsigned int nedge, IList<unsigned int>^ edge_ary,
        [Out] IList<int>^% el2ed );

    /*!
    @brief 境界要素を作る(可視化のための関数)
    */
    virtual CElemAry^ MakeBoundElemAry(unsigned int id_es_corner, [Out] unsigned int% id_es_add, [Out] IList<unsigned int>^ aIndElemFace);
    
    //! IO functions
    int InitializeFromFile(String^ file_name, long% offset);   // [IN/OUT]offset
    int WriteToFile(String^ file_name, long% offset, unsigned int id); // [IN/OUT]offset

    // lnods should be unsigned int?
    //! Add element segment
    IList<int>^ AddSegment(IList< DelFEM4NetCom::Pair<unsigned int, CElemAry::CElemSeg^>^ >^ es_ary, IList<int>^ lnods );
    int AddSegment(unsigned int id, CElemAry::CElemSeg^ es, IList<int>^ lnods);

    // 要素を囲む要素を作る．内部でelsuelのメモリ領域確保はしないので，最初から確保しておく
    bool MakeElemSurElem(int id_es_corner, array<int>^ elsuel);

protected:
    Fem::Field::CElemAry * self;

};

// ポインタ操作用クラス
public ref class CElemAryPtr : public CElemAry
{
public:
    CElemAryPtr(Fem::Field::CElemAry *baseSelf) : CElemAry(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CElemAryPtr(const CElemAryPtr% rhs) : CElemAry(false)
    {
        this->self = rhs.self;
    }

public:
    ~CElemAryPtr()
    {
        this->!CElemAryPtr();
    }

    !CElemAryPtr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

} // namespace Field
} // namespace DelFEM4NetFem

#endif
