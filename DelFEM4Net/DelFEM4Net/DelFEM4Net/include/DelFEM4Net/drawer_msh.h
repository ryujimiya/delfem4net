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
@brief メッシュ描画クラス(CDrawerMsh2D,CDrawerMsh3D)のインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_DRAWER_MSH_H)
#define DELFEM4NET_DRAWER_MSH_H

#include <vector>

#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/vector3d.h"
#include "DelFEM4Net/mesher2d.h"
#include "DelFEM4Net/mesh3d.h"

#include "delfem/drawer_msh.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMsh
{
    ref class CMesher2D;
    ref class CMesh3D;

namespace View
{

/*! 
@brief ２次元メッシュ描画クラス
@ingroup Msh2D
*/
public ref class CDrawerMsh2D : public DelFEM4NetCom::View::CDrawer
{
public : 
    //CDrawerMsh2D();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerMsh2D(const CDrawerMsh2D% rhs);
public:
    CDrawerMsh2D(DelFEM4NetMsh::CMesher2D^ msh);
    CDrawerMsh2D(DelFEM4NetMsh::CMesher2D^ msh, bool is_draw_face);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CDrawerMsh2D(Msh::View::CDrawerMsh2D *self);
public:
    virtual ~CDrawerMsh2D();
    !CDrawerMsh2D();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Msh::View::CDrawerMsh2D * Self
    {
        Msh::View::CDrawerMsh2D * get();
    }

    virtual void Draw() override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    void SetLineWidth(unsigned int iwidth);

    //! 座標を更新するためのルーティン
    void UpdateCoord(DelFEM4NetMsh::CMesher2D^ msh );

    /*! 
    @brief セレクションバッファへの書き出し
    @param[in] idraw 名前付けの最初に付けられる数
    */
    virtual void DrawSelection(unsigned int idraw) override;
    virtual void AddSelected(array<int>^ selec_flg) override;
    virtual void ClearSelected() override;

    void GetMshPartID(array<int>^ selec_flg, [Out] unsigned int% msh_part_id);

protected:
    Msh::View::CDrawerMsh2D *self;
};

////////////////////////////////
////////////////////////////////

/*! 
@brief ３次元メッシュ描画クラス
@ingroup Msh3D
*/
public ref class CDrawerMsh3D : public DelFEM4NetCom::View::CDrawer
{
public : 
    //CDrawerMsh3D();
    CDrawerMsh3D(const CDrawerMsh3D% rhs);
    CDrawerMsh3D(DelFEM4NetMsh::CMesh3D^ msh);
    CDrawerMsh3D(Msh::View::CDrawerMsh3D *self);
    virtual ~CDrawerMsh3D();
    !CDrawerMsh3D();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Msh::View::CDrawerMsh3D * Self
    {
        Msh::View::CDrawerMsh3D * get();
    }

    // virtual関数
    virtual void Draw() override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flg) override;
    virtual void ClearSelected() override;

    virtual void Hide(unsigned int id_msh);
    void SetColor(unsigned int id_msh, double r, double g, double b);
    
protected:
    Msh::View::CDrawerMsh3D *self;
};

}    // end namespace View
}    // end namespace Msh


#endif
