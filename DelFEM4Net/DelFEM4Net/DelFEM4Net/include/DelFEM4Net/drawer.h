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
@brief 抽象描画クラス(Com::View::CDrawer)のインターフェース
@remark このファイルの中のオブジェクトは頑張ってOpenGL非依存にしてある
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/



#if !defined(DELFEM4NET_DRAWER_H)
#define DELFEM4NET_DRAWER_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#include <vector>
#include <assert.h>

#include "DelFEM4Net/stub/clr_stub.h"   // DelFEM4NetCom::DoubleArrayIndexer

#include "delfem/drawer.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


namespace DelFEM4NetCom
{
ref class CBoundingBox3D;

namespace View
{

//! Abstract class of drawing something
public ref  class  CDrawer abstract
{
public:
    // nativeなインスタンス作成は派生クラスで行うため、コンストラクタ、デストラクタ、ファイナライザは何もしない
    CDrawer(){}
private:
    CDrawer(const CDrawer% rhs){ assert(false); }
public:
    virtual ~CDrawer(){ this->!CDrawer(); }
    !CDrawer(){}

    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() = 0;
    }

    /*! 
    @brief セレクションバッファへの書き出し
    @param[in] idraw 名前付けの最初に付けられる数
    */
    virtual void DrawSelection(unsigned int idraw) = 0;
    //! draw in openGL frame buffer
    virtual void Draw() = 0;
    /*!
    @brief Obtain 3D Bounding Box (Com::CBoundingBox)
    @param[in] rot : 3x3 rotation matrix
    */
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) = 0;
    //! 選択されたオブジェクトを追加して，ハイライトさせる
    virtual void AddSelected(array<int>^ selec_flg) = 0;
    //! 選択を解除する
    virtual void ClearSelected() = 0;

    virtual void SetAntiAliasing(bool is_aa)
    {
        if (this->DrawerSelf == NULL)
        {
            return;
        }
        this->DrawerSelf->SetAntiAliasing(is_aa);
    }

    // return suitable rotation mode ( 0:2D, 1:2DH, 2:3D )
    virtual unsigned int GetSutableRotMode()
    {
        if (this->DrawerSelf == NULL)
        {
            return 0;
        }
       return this->DrawerSelf->GetSutableRotMode();
    }
};

// 空のDrawerクラス : C#側でCDrawer派生クラスを定義したいときに使用
//                    DrawerSelfをC#側で実装できないので設けた
public ref class CEmptyDrawer : CDrawer
{
public:
    // nativeなインスタンス作成は派生クラスで行うため、コンストラクタ、デストラクタ、ファイナライザは何もしない
    CEmptyDrawer(){}
private:
    CEmptyDrawer(const CEmptyDrawer% rhs){ assert(false); }
public:
    virtual ~CEmptyDrawer(){ this->!CEmptyDrawer(); }
    !CEmptyDrawer(){}

    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override { return NULL; }
    }

    /*! 
    @brief セレクションバッファへの書き出し
    @param[in] idraw 名前付けの最初に付けられる数
    */
    virtual void DrawSelection(unsigned int idraw) override {}
    //! draw in openGL frame buffer
    virtual void Draw() override {};
    /*!
    @brief Obtain 3D Bounding Box (Com::CBoundingBox)
    @param[in] rot : 3x3 rotation matrix
    */
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    //! 選択されたオブジェクトを追加して，ハイライトさせる
    virtual void AddSelected(array<int>^ selec_flg) override {};
    //! 選択を解除する
    virtual void ClearSelected() override {};

    virtual void SetAntiAliasing(bool is_aa) override {}

    // return suitable rotation mode ( 0:2D, 1:2DH, 2:3D )
    virtual unsigned int GetSutableRotMode() override { return 0; }
};

ref class CCamera;

//! CDrawerの派生クラスのポインタを配列として格納するためのクラス
public ref class CDrawerArray
{
public:
    CDrawerArray();
    CDrawerArray(const CDrawerArray% rhs);
    CDrawerArray(Com::View::CDrawerArray *self);
    virtual ~CDrawerArray();
    !CDrawerArray();
    
    void PushBack(CDrawer^ pDrawer);
    void Draw();
    void DrawSelection();
    void AddSelected(array<int>^ selec_flg);
    void ClearSelected();
    void Clear();

    DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot);
    void InitTrans (DelFEM4NetCom::View::CCamera^ mvp_trans);
    
public:
    property Com::View::CDrawerArray * Self
    {
        Com::View::CDrawerArray * get();
    };
    
    property IList<CDrawer^>^ m_drawer_ary
    {
        IList<CDrawer^>^ get();
        //void set(IList<CDrawer^>^ list);
    }
    
protected:
    Com::View::CDrawerArray *self;
    IList<CDrawer^>^ drawer_ary_Managed;
};


/*! 
@brief OpenGLに頂点配列として渡す配列
@remark このクラスはOpenGL非依存なので，ユーザーが生データ(public変数orz)で取ってきてOpenGLに投げる仕様（あんまり良くない）
*/
public ref class CVertexArray
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static Com::View::CVertexArray * CreateNativeInstance(const Com::View::CVertexArray& rhs);
    
public:
    CVertexArray();
    CVertexArray(const CVertexArray% rhs);
    CVertexArray(const unsigned int np, const unsigned int nd);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CVertexArray(Com::View::CVertexArray *self);
public:
    virtual ~CVertexArray();
    !CVertexArray();

    void SetSize(unsigned int npoin, unsigned int ndim);
    // rot == 0 : return object axis bounding box
    // rot != 0 : return rotated axis bounding box (rot is 3x3 matrix)
    DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot);
    /* プロパティに移動
    unsigned int NDim() ;
    unsigned int NPoin() ;
    */
    void EnableUVMap(bool is_uv_map);

public:
    property Com::View::CVertexArray * Self
    {
        Com::View::CVertexArray * get();
    }
    
    property unsigned int NDim
    {
        unsigned int get();
    }
    property unsigned int NPoin
    {
        unsigned int get();
    }
    
    property DelFEM4NetCom::DoubleArrayIndexer^ pVertexArray
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    
    property DoubleArrayIndexer^ pUVArray
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }

protected:
    Com::View::CVertexArray *self;
    
    DelFEM4NetCom::DoubleArrayIndexer^ pVertexArrayIndexer;
    DelFEM4NetCom::DoubleArrayIndexer^ pUVArrayIndexer;
    
};
    
}    // end namespace View
}    // end namespace DelFEM4NetCom


#endif
