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
@brief 抽象メッシュクラス(Msh::CMesh_Interface)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_MESH_INTERFACE_H)
#define DELFEM4NET_MESH_INTERFACE_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#include <assert.h>
#include <vector>
//#include <map>

#include "DelFEM4Net/stub/clr_stub.h"
#include "delfem/mesh_interface.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

////////////////////////////////////////////////

namespace DelFEM4NetMsh
{

/*!
@addtogroup Msh
*/
//@{
public enum class MSH_TYPE
{
    VERTEX,    //!< 頂点要素
    BAR,    //!< 線分要素
    TRI,    //!< 三角形要素
    QUAD,    //!< ４角形要素
    TET,    //!< 四面体要素
    HEX        //!< 六面体要素
};

////////////////////////////////////////////////

/*!
@brief メッシュインターフェースクラス
@ingroup Msh
*/
public interface class  IMesh
{
public:
    property Msh::IMesh* MshSelf
    {
        Msh::IMesh* get();
    }
    
    //! 座標の次元を得る
    unsigned int GetDimention();
    void GetInfo(unsigned int id_msh,
        [Out] unsigned int% id_cad_part, [Out] unsigned int% id_msh_before_ext, [Out] unsigned int% inum_ext,
        [Out] int% ilayer);
    //! 座標の配列を得る
    void GetCoord([Out] IList<double>^% coord);
    //! コネクティビティの配列を得る
    DelFEM4NetMsh::MSH_TYPE GetConnectivity(unsigned int id_msh, [Out]IList<int>^% lnods);
    //! 要素配列IDの配列得る
    IList<unsigned int>^ GetAry_ID();
    //! 包含関係を得る
    IList<unsigned int>^ GetIncludeElemIDAry(unsigned int id_msh);
};

//! ２次元メッシュを３次元に射影したメッシュのクラス
public ref class CMeshProjector2Dto3D : public IMesh
{
public:
    /*
    CMeshProjector2Dto3D()
    {
        this->self = new Msh::CMeshProjector2Dto3D();
    }
    */
    
    CMeshProjector2Dto3D(const CMeshProjector2Dto3D% rhs)
    {
        Msh::CMeshProjector2Dto3D rhs_instance_ = *(rhs.self);

        this->self = new Msh::CMeshProjector2Dto3D(rhs_instance_);
    }
    
    CMeshProjector2Dto3D(Msh::CMeshProjector2Dto3D *self)
    {
        this->self = self;
    }
    
    CMeshProjector2Dto3D(IMesh^ msh_input)
    {
        Msh::IMesh *msh_input_ = msh_input->MshSelf;
        this->self = new Msh::CMeshProjector2Dto3D(*msh_input_);
    }
    
    ~CMeshProjector2Dto3D()
    {
        this->!CMeshProjector2Dto3D();
    }
    
    !CMeshProjector2Dto3D()
    {
        delete this->self;
    }
    
    virtual Object^ Clone()
    {
        return gcnew CMeshProjector2Dto3D(*this);
    }

public:
    property Msh::IMesh* MshSelf
    {
        virtual Msh::IMesh* get() { return this->self; }
    }

    property Msh::CMeshProjector2Dto3D* Self
    {
        Msh::CMeshProjector2Dto3D* get() { return this->self; }
    }

    virtual unsigned int GetDimention()
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
        
        IList<unsigned int>^ list =DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);

        return list;
    }
    
    virtual IList<unsigned int>^ GetIncludeElemIDAry(unsigned int id_msh)
    {
        std::vector<unsigned int> vec;
        
        vec = this->self->GetIncludeElemIDAry(id_msh);
        
        IList<unsigned int>^ list =DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);

        return list;
    }

protected:
    Msh::CMeshProjector2Dto3D *self;

};

// @}
};

#endif
