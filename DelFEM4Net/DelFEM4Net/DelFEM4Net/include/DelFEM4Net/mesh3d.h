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
@brief ３次元メッシュクラス(Msh::CMesh3D)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/
#if !defined(DELFEM4NET_MESH_3D_H)
#define DELFEM4NET_MESH_3D_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/vector2d.h"
//#include "DelFEM4Net/quaternion.h"

#include "DelFEM4Net/mesh_interface.h"
#include "DelFEM4Net/mesher2d.h"           //　ここをInterfaceクラスにできるように頑張る
#include "DelFEM4Net/msh/meshkernel2d.h"
#include "DelFEM4Net/msh/meshkernel3d.h"

#include "delfem/mesh3d.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////

namespace DelFEM4NetMsh{

/*! 
@addtogroup Msh3D
*/
// @{

//! 頂点構造体
public ref class SVertex3D
{
public:
    SVertex3D ()
    {
       this->self = new Msh::SVertex3D;
    }
    
    SVertex3D(const SVertex3D% rhs)
    {
        Msh::SVertex3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::SVertex3D(rhs_instance_);
    }
    
    SVertex3D(Msh::SVertex3D *self)
    {
        this->self = self;
    }
    
    ~SVertex3D()
    {
        this->!SVertex3D();
    }
    
    !SVertex3D()
    {
        delete this->self;
    }
    
public:
    property Msh::SVertex3D * Self
    {
        Msh::SVertex3D *get() { return this->self; }
    }

    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }
    
    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }
    
    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3) {０か奇数になるはず}
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property unsigned int v    //!< 点のID，今は一つだけだけど、そのうち配列にして複数扱えるようにしたいな。
    {
        unsigned int get() { return this->self->v; }
        void set(unsigned int value) { this->self->v = value; }
    }

protected:
    Msh::SVertex3D *self;

};

//! 線要素配列
public ref class CBarAry3D
{
public:
    CBarAry3D ()
    {
       this->self = new Msh::CBarAry3D;
    }
    
    CBarAry3D(const CBarAry3D% rhs)
    {
        Msh::CBarAry3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CBarAry3D(rhs_instance_);
    }
    
    CBarAry3D(Msh::CBarAry3D *self)
    {
        this->self = self;
    }
    
    ~CBarAry3D()
    {
        this->!CBarAry3D();
    }
    
    !CBarAry3D()
    {
        delete this->self;
    }
    
public:
    property Msh::CBarAry3D * Self
    {
        Msh::CBarAry3D *get() { return this->self; }
    }


    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }

    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3)
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property IList<SBar^>^ m_aBar    //!< ３次元線分要素配列
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
    Msh::CBarAry3D *self;

};

//! ４面体配列クラス    
public ref class CTetAry
{
public:
    CTetAry ()
    {
       this->self = new Msh::CTetAry;
    }
    
    CTetAry(const CTetAry% rhs)
    {
        Msh::CTetAry rhs_instance_ = *(rhs.self);
        this->self = new Msh::CTetAry(rhs_instance_);
    }
    
    CTetAry(Msh::CTetAry *self)
    {
        this->self = self;
    }
    
    ~CTetAry()
    {
        this->!CTetAry();
    }
    
    !CTetAry()
    {
        delete this->self;
    }
    
public:
    property Msh::CTetAry * Self
    {
        Msh::CTetAry *get() { return this->self; }
    }


    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }

    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3)
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property IList<STet^>^ m_aTet    //!< ３次元四面体要素配列
    {
        IList<STet^>^ get()
        {
            const std::vector<Msh::STet>& vec = this->self->m_aTet;

            IList<STet^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::STet, STet>(vec);
            return list;
        }
        void set(IList<STet^>^ list)
        {
            std::vector<Msh::STet>& vec = this->self->m_aTet;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<STet, Msh::STet>(list, vec);
        }
    }

protected:
    Msh::CTetAry *self;

};

//! 六面体配列クラス
public ref class CHexAry
{
public:
    CHexAry ()
    {
       this->self = new Msh::CHexAry;
    }
    
    CHexAry(const CHexAry% rhs)
    {
        Msh::CHexAry rhs_instance_ = *(rhs.self);
        this->self = new Msh::CHexAry(rhs_instance_);
    }
    
    CHexAry(Msh::CHexAry *self)
    {
        this->self = self;
    }
    
    ~CHexAry()
    {
        this->!CHexAry();
    }
    
    !CHexAry()
    {
        delete this->self;
    }
    
public:
    property Msh::CHexAry * Self
    {
        Msh::CHexAry *get() { return this->self; }
    }


    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }

    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3)
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property IList<SHex^>^ m_aHex    //!< ３次元６面体要素配列
    {
        IList<SHex^>^ get()
        {
            const std::vector<Msh::SHex>& vec = this->self->m_aHex;

            IList<SHex^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SHex, SHex>(vec);
            return list;
        }
        void set(IList<SHex^>^ list)
        {
            std::vector<Msh::SHex>& vec = this->self->m_aHex;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<SHex, Msh::SHex>(list, vec);
        }
    }

protected:
    Msh::CHexAry *self;

};

//! ３次元三角形配列クラス
public ref class CTriAry3D
{
public:
    CTriAry3D ()
    {
       this->self = new Msh::CTriAry3D;
    }
    
    CTriAry3D(const CTriAry3D% rhs)
    {
        Msh::CTriAry3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CTriAry3D(rhs_instance_);
    }
    
    CTriAry3D(Msh::CTriAry3D *self)
    {
        this->self = self;
    }
    
    ~CTriAry3D()
    {
        this->!CTriAry3D();
    }
    
    !CTriAry3D()
    {
        delete this->self;
    }
    
public:
    property Msh::CTriAry3D * Self
    {
        Msh::CTriAry3D *get() { return this->self; }
    }


    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }

    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3)
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property IList<STri3D^>^ m_aTri    //!< ３次元３角形要素配列
    {
        IList<STri3D^>^ get()
        {
            const std::vector<Msh::STri3D>& vec = this->self->m_aTri;

            IList<STri3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::STri3D, STri3D>(vec);
            return list;
        }
        void set(IList<STri3D^>^ list)
        {
            std::vector<Msh::STri3D>& vec = this->self->m_aTri;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<STri3D, Msh::STri3D>(list, vec);
        }
    }

protected:
    Msh::CTriAry3D *self;

};


//! ３次元４角形配列クラス
public ref class CQuadAry3D
{
public:
    CQuadAry3D ()
    {
       this->self = new Msh::CQuadAry3D;
    }
    
    CQuadAry3D(const CQuadAry3D% rhs)
    {
        Msh::CQuadAry3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CQuadAry3D(rhs_instance_);
    }
    
    CQuadAry3D(Msh::CQuadAry3D *self)
    {
        this->self = self;
    }
    
    ~CQuadAry3D()
    {
        this->!CQuadAry3D();
    }
    
    !CQuadAry3D()
    {
        delete this->self;
    }
    
public:
    property Msh::CQuadAry3D * Self
    {
        Msh::CQuadAry3D *get() { return this->self; }
    }


    property unsigned int id    //!< ID
    {
        unsigned int get() { return this->self->id; }
        void set(unsigned int value) { this->self->id = value; }
    }

    property unsigned int id_cad    //!< CADの頂点ID（CADに関連されてなければ０）
    {
        unsigned int get() { return this->self->id_cad; }
        void set(unsigned int value) { this->self->id_cad = value; }
    }

    property unsigned int id_msh_before_extrude
    {
        unsigned int get() { return this->self->id_msh_before_extrude; }
        void set(unsigned int value) { this->self->id_msh_before_extrude = value; }
    }

    property unsigned int inum_extrude    //!< 突き出されてない(0), 底面(1), 側面(2), 上面(3)
    {
        unsigned int get() { return this->self->inum_extrude; }
        void set(unsigned int value) { this->self->inum_extrude = value; }
    }

    property IList<SQuad3D^>^ m_aQuad    //!< ３次元４角形要素配列
    {
        IList<SQuad3D^>^ get()
        {
            const std::vector<Msh::SQuad3D>& vec = this->self->m_aQuad;

            IList<SQuad3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SQuad3D, SQuad3D>(vec);
            return list;
        }
        void set(IList<SQuad3D^>^ list)
        {
            std::vector<Msh::SQuad3D>& vec = this->self->m_aQuad;
            
            DelFEM4NetCom::ClrStub::ListToInstanceVector<SQuad3D, Msh::SQuad3D>(list, vec);
        }
    }

protected:
    Msh::CQuadAry3D *self;

};


////////////////////////////////

//! ３次元メッシュクラス
public ref class CMesh3D  : public IMesh
{
public:
    CMesh3D()
    {
        this->self = new Msh::CMesh3D();
    }
    
    CMesh3D(const CMesh3D% rhs)
    {
        Msh::CMesh3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CMesh3D(rhs_instance_);
    }
    
    CMesh3D(Msh::CMesh3D *self)
    {
        this->self = self;
    }
   
    property Msh::IMesh * MshSelf
    {
        virtual Msh::IMesh * get() { return this->self; }
    }

    property Msh::CMesh3D * Self
    {
        Msh::CMesh3D * get() { return this->self; }
    }

    virtual ~CMesh3D()
    {
        this->!CMesh3D();
    }
    
    !CMesh3D()
    {
        delete this->self;
    }

    CMesh3D^ Clone()
    {
        return gcnew CMesh3D(*this);
    }

    ////////////////////////////////

    virtual unsigned int GetDimention() 
    {
        return this->self->GetDimention();
    }

    virtual void GetCoord([Out] IList<double>^% coord)
    {
        std::vector<double> coord_;

        this->self->GetCoord(coord_);

        coord = DelFEM4NetCom::ClrStub::VectorToList<double>(coord_);
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


    void Scale(double r)
    {
        this->self->Scale(r);
    }

    void Translate(double x, double y, double z)
    {
        this->self->Translate(x, y, z);
    }
    
    
    void Rotate(double t, double x, double y, double z)
    {
        this->self->Rotate(t, x, y, z);
    }


    ////////////////////////////////

    void Clear() //!< メッシュの情報を全てクリアして削除する
    {
        this->self->Clear();
    }

    // const関数
    bool IsID(unsigned int id)    //!< メッシュのIDを調べる
    {
        return this->self->IsID(id);
    }

    // 要素配列、節点配列に関するGetメソッド
    IList<CTetAry^>^ GetTetArySet() //!< 四面体要素配列の配列を得る
    {
        const std::vector<Msh::CTetAry>& vec = this->self->GetTetArySet();

        IList<CTetAry^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CTetAry, CTetAry>(vec);
        return list;
    }

    IList<CHexAry^>^ GetHexArySet() //!< 六面体要素配列の配列を得る
    {
        const std::vector<Msh::CHexAry>& vec = this->self->GetHexArySet();

        IList<CHexAry^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CHexAry, CHexAry>(vec);
        return list;
    }

    IList<CTriAry3D^>^ GetTriArySet() //!< ３次元３角形要素配列の配列を得る
    {
        const std::vector<Msh::CTriAry3D>& vec = this->self->GetTriArySet();
        
        IList<CTriAry3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CTriAry3D, CTriAry3D>(vec);
        return list;
    }

    IList<CQuadAry3D^>^ GetQuadArySet() //!< ３次元４角形要素配列の配列を得る
    {
        const std::vector<Msh::CQuadAry3D>& vec = this->self->GetQuadArySet();
        
        IList<CQuadAry3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CQuadAry3D, CQuadAry3D>(vec);
        return list;
    }

    IList<CBarAry3D^>^ GetBarArySet() //!< 線分要素配列の配列を得る
    {
        const std::vector<Msh::CBarAry3D>& vec = this->self->GetBarArySet();
        
        IList<CBarAry3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::CBarAry3D, CBarAry3D>(vec);
        return list;
    }

    IList<SVertex3D^>^ GetVertexAry() //!< 点要素の配列を得る
    {
        const std::vector<Msh::SVertex3D>& vec = this->self->GetVertexAry();
        
        IList<SVertex3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Msh::SVertex3D, SVertex3D>(vec);
        return list;
    }

    IList<DelFEM4NetCom::CVector3D^>^ GetVectorAry() //!< 座標値の配列を得る
    {
        const std::vector<Com::CVector3D>& vec = this->self->GetVectorAry();
        
        IList<DelFEM4NetCom::CVector3D^>^ list = DelFEM4NetCom::ClrStub::InstanceVectorToList<Com::CVector3D, DelFEM4NetCom::CVector3D>(vec);
        return list;
    }

    ////////////////////////////////////////////////////////////////
    // IOメソッド

    // 読み込み書き出し
    bool Serialize(DelFEM4NetCom::CSerializer^ serialize)
    {
        Com::CSerializer *serialize_ = serialize->Self;
        return this->self->Serialize(*serialize_);
    }


    // GiDメッシュの読み込み
    bool ReadFromFile_GiDMsh(String^ file_name)
    {
        std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
        
        bool ret = this->self->ReadFromFile_GiDMsh(file_name_);
        
        return ret;
    }

    bool ReadFromFile_TetgenMsh(String^ file_name)
    {
        std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
        
        bool ret = this->self->ReadFromFile_TetgenMsh(file_name_);
        
        return ret;
    }

protected:
    Msh::CMesh3D *self;

};

////////////////////////////////

public ref class CMesh3D_Extrude : public CMesh3D
{
public:
    CMesh3D_Extrude() : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        this->self = new Msh::CMesh3D_Extrude();

        setBaseSelf();
    }
    
    CMesh3D_Extrude(const CMesh3D_Extrude% rhs) : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        Msh::CMesh3D_Extrude& rhs_instance_ = *(rhs.self);
        this->self = new Msh::CMesh3D_Extrude(rhs_instance_);

        setBaseSelf();
    }
    
    CMesh3D_Extrude(Msh::CMesh3D_Extrude *self) : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        this->self = self;

        setBaseSelf();
    }
   
    virtual ~CMesh3D_Extrude()
    {
        this->!CMesh3D_Extrude();
    }
    
    !CMesh3D_Extrude()
    {
        delete this->self;

        // 基本クラスのファイナライザでdeleteされないようにnativeインスタンスのポインタをNULLを設定し、基本クラスへ反映する
        this->self = NULL;
        setBaseSelf();
    }

    CMesh3D_Extrude^ Clone()
    {
        return gcnew CMesh3D_Extrude(*this);
    }

    bool UpdateMeshCoord(CMesher2D^ msh_2d)
    {
        return this->self->UpdateMeshCoord(*(msh_2d->Self));
    }
    
    bool UpdateMeshConnectivity(CMesher2D^ msh_2d)
    {
        return this->self->UpdateMeshConnectivity(*(msh_2d->Self));
    }

    // メッシュを突き出す
    // height : メッシュ高さ
    // elen : 高さ方向のメッシュ幅
    bool Extrude(CMesher2D^ msh_2d, double height, double elen)    // 本当は面を突き出すようにしたい。
    {
        return this->self->Extrude(*(msh_2d->Self), height, elen);
    }

protected:
    Msh::CMesh3D_Extrude *self;
    
    virtual void setBaseSelf()
    {
        CMesh3D::self = this->self;
    }
    
};

public ref class CMesher3D : public CMesh3D
{
public:
    CMesher3D() : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        this->self = new Msh::CMesher3D();

        setBaseSelf();
    }
    
    CMesher3D(const CMesher3D% rhs) : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        Msh::CMesher3D rhs_instance_ = *(rhs.self);
        this->self = new Msh::CMesher3D(rhs_instance_);

        setBaseSelf();
    }
    
    CMesher3D(Msh::CMesher3D *self) : CMesh3D((Msh::CMesh3D *)NULL)  // 基本クラスではunmanagedのインスタンスを作成しない
    {
        this->self = self;

        setBaseSelf();
    }
   
    virtual ~CMesher3D()
    {
        this->!CMesher3D();
    }
    
    !CMesher3D()
    {
        delete this->self;

        // 基本クラスのファイナライザでdeleteされないようにnativeインスタンスのポインタをNULLを設定し、基本クラスへ反映する
        this->self = NULL;
        setBaseSelf();
    }

    CMesher3D^ Clone()
    {
        return gcnew CMesher3D(*this);
    }
    
    bool ReadFile_PLY(String^ file_name )
    {
        std::string file_name_ = DelFEM4NetCom::ClrStub::StringToStd(file_name);
        
        bool ret = this->self->ReadFile_PLY(file_name_);
        
        return ret;
    }

    bool CutMesh(double elen)    // メッシュを切る
    {
        return this->self->CutMesh(elen);
    }
    
    bool HomogenizeSurface(double elen)
    {
        return this->self->HomogenizeSurface(elen);
    }
    

protected:
    Msh::CMesher3D *self;

    virtual void setBaseSelf()
    {
        CMesh3D::self = this->self;
    }
    
};

//! @}
};

#endif
