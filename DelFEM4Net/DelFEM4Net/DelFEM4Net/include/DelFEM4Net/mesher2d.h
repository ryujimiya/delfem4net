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
@brief interface of 2D mesher class (Msh::CMesher2D)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_MSHER_2D_H)
#define DELFEM4NET_MSHER_2D_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#include <vector>
#include <string>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/vector2d.h"
#include "DelFEM4Net/serialize.h"
#include "DelFEM4Net/mesh_interface.h"
#include "DelFEM4Net/cad2d_interface.h"

#include "DelFEM4Net/msh/meshkernel2d.h"

#include "delfem/mesher2d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////

namespace DelFEM4NetMsh
{

/*! 
@addtogroup Msh2D
*/
// @{

//! structure of vertex
public ref class SVertex
{
public:
    SVertex ()
    {
       this->self = new Msh::SVertex;
    }
    
    SVertex(const SVertex% rhs)
    {
        Msh::SVertex rhs_instance_ = *(rhs.self);
        this->self = new Msh::SVertex(rhs_instance_);
    }
    
    SVertex(Msh::SVertex *self)
    {
        this->self = self;
    }
    
    ~SVertex()
    {
        this->!SVertex();
    }
    
    !SVertex()
    {
        delete this->self;
    }
    
public:
    property Msh::SVertex * Self
    {
        Msh::SVertex *get() { return this->self; }
    }
    
    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }
    
    property unsigned int id_v_cad    //!< vertex id in CAD（0 if not related to CAD）
    {
        unsigned int get() { return this->self->id_v_cad; }
        void set(unsigned int value) { this->self->id_v_cad = value; }
    }
    
    property int ilayer
    {
        int get() { return this->self->ilayer; }
        void set(int value) { this->self->ilayer = value; }
    }

    property unsigned int v    //!< index of node
    {
        unsigned int get() { return this->self->v; }
        void set(unsigned int value) { this->self->v = value; }
    }

protected:
    Msh::SVertex *self;
    
};

//! array of line element
public ref class CBarAry
{
public:
    static int C_V_CNT = 2;
    
    CBarAry ()
    {
       this->self = new Msh::CBarAry;
    }
    
    CBarAry(const CBarAry% rhs)
    {
        Msh::CBarAry rhs_instance_ = *(rhs.self);
        this->self = new Msh::CBarAry(rhs_instance_);
    }
    
    CBarAry(Msh::CBarAry *self)
    {
        this->self = self;
    }
    
    ~CBarAry()
    {
        this->!CBarAry();
    }
    
    !CBarAry()
    {
        delete this->self;
    }
    
public:
    property Msh::CBarAry * Self
    {
        Msh::CBarAry *get() { return this->self; }
    }

    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_e_cad    //!< CADの辺ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_e_cad; }
        void set(unsigned int value) { this->self->id_e_cad = value; }
    }

    property array<unsigned int>^ id_se  // id_se[2]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->id_se;
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
            unsigned int* unmanaged = this->self->id_se;
            for (int i = 0; i < C_V_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }
    
    property array<unsigned int>^ id_lr  // id_lr[2]
    {
        array<unsigned int>^ get()
        {
            unsigned int* unmanaged = this->self->id_lr;
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
            unsigned int* unmanaged = this->self->id_lr;
            for (int i = 0; i < C_V_CNT; i++)
            {
                unmanaged[i] = managed[i];
            }
        }
    }
    
    property int ilayer
    {
        int get() { return this->self->ilayer; }
        void set(int value) { this->self->ilayer = value; }
    }
    
    property IList<SBar^>^ m_aBar    //!< array of line element
    {
        IList<SBar^>^ get()
        {
            const std::vector<Msh::SBar>& vec = this->self->m_aBar;
            
            IList<SBar^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SBar, SBar>(vec);
            return list;
        }
        void set(IList<SBar^>^ list)
        {
            std::vector<Msh::SBar>& vec = this->self->m_aBar;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<SBar, Msh::SBar>(list, vec);
        }
    }

protected:
    Msh::CBarAry *self;
    
};

//! array of 2D triangle elemnet
public ref class CTriAry2D
{
public:
    CTriAry2D ()
    {
       this->self = new Msh::CTriAry2D;
    }
    
    CTriAry2D(const CTriAry2D% rhs)
    {
        Msh::CTriAry2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CTriAry2D(rhs_instance_);
    }
    
    CTriAry2D(Msh::CTriAry2D *self)
    {
        this->self = self;
    }
    
    ~CTriAry2D()
    {
        this->!CTriAry2D();
    }
    
    !CTriAry2D()
    {
        delete this->self;
    }
    
public:
    property Msh::CTriAry2D * Self
    {
        Msh::CTriAry2D *get() { return this->self; }
    }

    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_l_cad    //!< CADの面ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_l_cad; }
        void set(unsigned int value) { this->self->id_l_cad = value; }
    }

    property int ilayer
    {
        int get() { return this->self->ilayer; }
        void set(int value) { this->self->ilayer = value; }
    }

    property IList<STri2D^>^ m_aTri    //!< array of 2d triangle element
    {
        IList<STri2D^>^ get()
        {
            const std::vector<Msh::STri2D>& vec = this->self->m_aTri;

            IList<STri2D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::STri2D, STri2D>(vec);
            return list;
        }
        void set(IList<STri2D^>^ list)
        {
            std::vector<Msh::STri2D>& vec = this->self->m_aTri;

            DelFEM4NetCom::ClrStub::ListToInstanceVector<STri2D, Msh::STri2D>(list, vec);
        }
    }

protected:
    Msh::CTriAry2D *self;
    
};

//! array of 2D quadric element 
public ref class CQuadAry2D
{
public:
    CQuadAry2D ()
    {
       this->self = new Msh::CQuadAry2D;
    }
    
    CQuadAry2D(const CQuadAry2D% rhs)
    {
        Msh::CQuadAry2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CQuadAry2D(rhs_instance_);
    }
    
    CQuadAry2D(Msh::CQuadAry2D *self)
    {
        this->self = self;
    }
    
    ~CQuadAry2D()
    {
        this->!CQuadAry2D();
    }
    
    !CQuadAry2D()
    {
        delete this->self;
    }
    
public:
    property Msh::CQuadAry2D * Self
    {
        Msh::CQuadAry2D *get() { return this->self; }
    }
    
    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_l_cad    //!< CADの面ID(CADに関連されてなければ０)
    {
        unsigned int get() { return this->self->id_l_cad; }
        void set(unsigned int value) { this->self->id_l_cad = value; }
    }

    property int ilayer
    {
        int get() { return this->self->ilayer; }
        void set(int value) { this->self->ilayer = value; }
    }

    property IList<SQuad2D^>^ m_aQuad     //!< array of 2D quadric element
    {
        IList<SQuad2D^>^ get()
        {
            const std::vector<Msh::SQuad2D>& vec = this->self->m_aQuad;

            IList<SQuad2D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SQuad2D, SQuad2D>(vec);
            return list;
        }
        void set(IList<SQuad2D^>^ list)
        {
            std::vector<Msh::SQuad2D>& vec = this->self->m_aQuad;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<SQuad2D, Msh::SQuad2D>(list, vec);
        }
    }

protected:
    Msh::CQuadAry2D *self;
    
};

////////////////////////////////////////////////

/*!
@brief ２次元メッシュクラス
@ingroup Msh2D

要素の種類に依存せずに通しでIDが振られている．
辺要素に関しては，CADの辺と同じ向きに要素番号順に並んでいる．（荷重境界条件からの制約）
*/
public ref class CMesher2D : public IMesh
{
public:
    CMesher2D()
    {
        this->self = new Msh::CMesher2D();
    }
    
    CMesher2D(const CMesher2D% rhs)
    {
        Msh::CMesher2D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CMesher2D(rhs_instance_);
    }
    
    CMesher2D(Msh::CMesher2D *self)
    {
        this->self = self;
    }
    
    //! できるだけ少ない要素数でメッシュを切る
    CMesher2D(DelFEM4NetCad::ICad2D_Msh^ cad_2d)
    {
        Cad::ICad2D_Msh *cad_2d_ = cad_2d->Cad2DMshSelf;
        
        this->self = new Msh::CMesher2D(*cad_2d_);
    }
    
    //! 要素の長さがelenとなるようにメッシュを切る
    CMesher2D(DelFEM4NetCad::ICad2D_Msh^ cad_2d, double elen)
    {
        Cad::ICad2D_Msh *cad_2d_ = cad_2d->Cad2DMshSelf;
        
        this->self = new Msh::CMesher2D(*cad_2d_, elen);
    }
    
    property Msh::IMesh * MshSelf
    {
        virtual Msh::IMesh * get() { return this->self; }
    }

    property Msh::CMesher2D * Self
    {
        Msh::CMesher2D * get() { return this->self; }
    }

    ~CMesher2D()
    {
        this->!CMesher2D();
    }
    
    !CMesher2D()
    {
        delete this->self;
    }

    virtual Object^ Clone()
    {
        return gcnew CMesher2D(*this);
    }

    ////////////////////////////////
    
    virtual void AddIdLCad_CutMesh(unsigned int id_l_cad)
    {
        this->self->AddIdLCad_CutMesh(id_l_cad);
    }
    
    virtual bool IsIdLCad_CutMesh(unsigned int id_l_cad)
    {
        return this->self->IsIdLCad_CutMesh(id_l_cad);
    }
    
    virtual void RemoveIdLCad_CutMesh(unsigned int id_l_cad)
    {
        this->self->RemoveIdLCad_CutMesh(id_l_cad);
    }
    
    virtual IList<unsigned int>^ GetIdLCad_CutMesh()
    {
        const std::vector<unsigned int>& vec = this->self->GetIdLCad_CutMesh();
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }
    virtual void SetMeshingMode_ElemLength(double len)
    {
        this->self->SetMeshingMode_ElemLength(len);
    }

    virtual void SetMeshingMode_ElemSize(unsigned int esize)
    {
        this->self->SetMeshingMode_ElemSize(esize);
    }
    
    virtual bool Meshing(DelFEM4NetCad::ICad2D_Msh^ cad_2d)
    {
        Cad::ICad2D_Msh *cad_2d_ = cad_2d->Cad2DMshSelf;
        return this->self->Meshing(*cad_2d_);
    }

    ////////////////////////////////

    virtual unsigned int GetDimention() //!< 座標の次元（２）を返す
    {
        return this->self->GetDimention();
    }
    
    virtual void GetInfo(unsigned int id_msh,
        [Out] unsigned int% id_cad, [Out] unsigned int% id_msh_before_ext, [Out] unsigned int% inum_ext,
        [Out] int% ilayer )
    {
        unsigned int id_cad_;
        unsigned int id_msh_before_ext_;
        unsigned int inum_ext_;
        int ilayer_;
        this->self->GetInfo(id_msh, id_cad_, id_msh_before_ext_, inum_ext_, ilayer_);  //out:id_cad_, id_msh_before_ext_, inum_ext_, ilayer_
        
        id_cad = id_cad_;
        id_msh_before_ext = id_msh_before_ext_;
        inum_ext = inum_ext_;
        ilayer = ilayer_;
    }

    virtual void GetCoord([Out] IList<double>^% coord)
    {
        std::vector<double> coord_;
        
        this->self->GetCoord(coord_);

        coord = DelFEM4NetCom::ClrStub::VectorToList<double>(coord_);
    }

    /*! メッシュの接続関係を取得する
    vectorコンテナでデータを受け渡しするのは，安全だけど低速
    ポインターを渡してデータの受け渡しを実装する予定
    */
    virtual DelFEM4NetMsh::MSH_TYPE GetConnectivity(unsigned int id_msh, [Out] IList<int>^% lnods)
    {
        Msh::MSH_TYPE ret;
        std::vector<int> lnods_;
        
        ret = this->self->GetConnectivity(id_msh, lnods_);
        
        lnods = DelFEM4NetCom::ClrStub::VectorToList<int>(lnods_);
        
        return static_cast<DelFEM4NetMsh::MSH_TYPE>(ret);
    }
    
    virtual IList<unsigned int>^ GetAry_ID()
    {
        std::vector<unsigned int> vec;
        
        vec = this->self->GetAry_ID();
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }
    
    virtual IList<unsigned int>^ GetIncludeElemIDAry(unsigned int id_msh)
    {
        std::vector<unsigned int> vec;
        
        vec = this->self->GetIncludeElemIDAry(id_msh);
        
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
        return list;
    }

    bool GetClipedMesh(
        [Out] IList< IList<int>^ >^% lnods_tri,
        [Out] IList<unsigned int>^% mapVal2Co,
        IList<unsigned int>^ aIdMsh_Ind,
        IList<unsigned int>^ aIdMshBar_Cut)
    {
        std::vector< std::vector<int> > lnods_tri_;
        std::vector<unsigned int> mapVal2Co_;
        std::vector<unsigned int> aIdMsh_Ind_;
        std::vector<unsigned int> aIdMshBar_Cut_;
        
        DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdMsh_Ind, aIdMsh_Ind_);
        DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(aIdMshBar_Cut, aIdMshBar_Cut_);
        
        bool ret = this->self->GetClipedMesh(
            lnods_tri_,
            mapVal2Co_,
            aIdMsh_Ind_,
            aIdMshBar_Cut_);  //OUT: lnods_tri_, mapVal2Co_
        
        lnods_tri = gcnew List< IList<int>^ >();
        if (lnods_tri_.size() > 0)
        {
            for (std::vector< std::vector<int> >::iterator itr = lnods_tri_.begin(); itr != lnods_tri_.end(); itr++)
            {
                const std::vector<int>& vec2 = *itr;
                IList<int>^ list2 = DelFEM4NetCom::ClrStub::VectorToList<int>(vec2);

                lnods_tri->Add(list2);
            }
        }
        mapVal2Co = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(mapVal2Co_);
        
        return ret;
    }

    //! データをすべてクリアする
    virtual void Clear()
    {
        this->self->Clear();
    }

    void SmoothingMesh_Laplace(unsigned int num_iter)
    {
        this->self->SmoothingMesh_Laplace(num_iter);
    }
    
    void SmoothingMesh_Delaunay(unsigned int% num_flip)   // IN/OUT num_flip
    {
        unsigned int num_flip_ = num_flip; 

        this->self->SmoothingMesh_Delaunay(num_flip_); // IN/OUT num_flip_

        num_flip = num_flip_;
    }

    ////////////////////////////////
    // const関数
    
    unsigned int GetElemID_FromCadID(unsigned int id_cad,  DelFEM4NetCad::CAD_ELEM_TYPE type_cad)
    {
        return this->self->GetElemID_FromCadID(id_cad, static_cast<Cad::CAD_ELEM_TYPE>(type_cad));
    }
    
    bool IsID(unsigned int id)
    {
        return this->self->IsID(id);
    }
    
    bool GetMshInfo(unsigned int id, [Out] unsigned int% nelem, [Out] DelFEM4NetMsh::MSH_TYPE% msh_type, [Out] unsigned int% iloc,  [Out] unsigned int% id_cad )
    {
        unsigned int nelem_;
        Msh::MSH_TYPE msh_type_;
        unsigned int iloc_;
        unsigned int id_cad_;
        
        bool ret = this->self->GetMshInfo(id, nelem_, msh_type_, iloc_, id_cad_);
        
        nelem = nelem_;
        msh_type = static_cast<DelFEM4NetMsh::MSH_TYPE>(msh_type_);
        iloc =iloc_;
        id_cad = id_cad_;
        
        return ret;
    }

    ////////////////////////////////
    // 要素配列、節点配列に関するGetメソッド

    IList<CTriAry2D^>^ GetTriArySet()
    {
        const std::vector<Msh::CTriAry2D>& vec = this->self->GetTriArySet();
        
        IList<CTriAry2D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CTriAry2D, CTriAry2D>(vec);
        return list;
    }

    IList<CQuadAry2D^>^ GetQuadArySet()
    {
        const std::vector<Msh::CQuadAry2D>& vec = this->self->GetQuadArySet();
        
        IList<CQuadAry2D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CQuadAry2D, CQuadAry2D>(vec);
        return list;
    }
    
    IList<CBarAry^>^ GetBarArySet()
    {
        const std::vector<Msh::CBarAry>& vec = this->self->GetBarArySet();
        
        IList<CBarAry^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CBarAry, CBarAry>(vec);
        return list;
    }

    IList<SVertex^>^ GetVertexAry()
    {
        const std::vector<Msh::SVertex>& vec = this->self->GetVertexAry();
        
        IList<SVertex^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SVertex, SVertex>(vec);
        return list;
    }

    IList<DelFEM4NetCom::CVector2D^>^ GetVectorAry()
    {
        const std::vector<Com::CVector2D>& vec = this->self->GetVectorAry();
        
        IList<DelFEM4NetCom::CVector2D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Com::CVector2D, DelFEM4NetCom::CVector2D>(vec);
        return list;
    }

    ////////////////////////////////////////////////////////////////
    // IOメソッド
    
    //! 読み込み書き出し
    bool Serialize(DelFEM4NetCom::CSerializer^ serialize)
    {
        bool is_only_cadmshlink = false;
        return Serialize(serialize, is_only_cadmshlink);
    }
    
    bool Serialize(DelFEM4NetCom::CSerializer^ serialize, bool is_only_cadmshlink)
    {
        Com::CSerializer *serialize_ = serialize->Self;
        return this->self->Serialize(*serialize_, is_only_cadmshlink);
    }

    //! GiDメッシュの読み込み
    bool ReadFromFile_GiDMsh(String^ file_name)
    {
        std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
        
        bool ret = this->self->ReadFromFile_GiDMsh(file_name_);
        
        return ret;
    }

protected:
    Msh::CMesher2D *self;

};

// @}
}

#endif
