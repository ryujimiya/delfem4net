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
@brief GLUTの便利な関数，クラス群
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_DRAWER_GL_UTILITY_H)
#define DELFEM4NET_DRAWER_GL_UTILITY_H

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
#endif

#include <vector>
#include <assert.h>

#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/vector3d.h"

#include "delfem/drawer_gl_utility.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{
namespace View{

//! 選択オブジェクト
public ref class SSelectedObject
{
public:
    SSelectedObject()
    {
        this->self = new Com::View::SSelectedObject;
    }
    
    SSelectedObject(SSelectedObject% rhs)
    {
        Com::View::SSelectedObject rhs_instance_ = *(rhs.self);
        // 構造体なのでコピーする
        *(this->self) = rhs_instance_;
    }
    
    SSelectedObject(Com::View::SSelectedObject *self)
    {
        this->self = self;
    }
    
    ~SSelectedObject()
    {
        this->!SSelectedObject();
    }
    
    !SSelectedObject()
    {
        delete this->self;
    }
    
    property Com::View::SSelectedObject* Self
    {
        Com::View::SSelectedObject * get() { return this->self; }
    }
    
    property unsigned int name_depth    //!< 名前の深さ
    {
        unsigned int get() { return this->self->name_depth; }
        void set(unsigned int value ) { this->self->name_depth = value; }
    }
    
    property array<int>^ name //!< 名前を格納している配列(4つまでしかないのは問題じゃね？)
    {
         array<int>^ get()
         {
             int *unmanaged = this->self->name;  // アンマネージドのポインタを取得
             array<int>^ managed = gcnew array<int>(4);
             for (int i = 0; i < 4; i++)
             {
                 managed[i] = unmanaged[i];
             }
             return managed;
         }
         void set(array<int>^ value)
         {
             array<int>^ managed = value;
             int *unmanaged = this->self->name; // アンマネージドのポインタを取得
             for (int i = 0; i < 4; i++)
             {
                 unmanaged[i] = managed[i];
             }
         }
    }
    
    property DelFEM4NetCom::CVector3D^ picked_pos //!< ピックされた点の３次元的な位置
    {
        DelFEM4NetCom::CVector3D^ get()
        {
            Com::CVector3D unmanaged_instance = this->self->picked_pos;
            Com::CVector3D *unmanaged = new Com::CVector3D(unmanaged_instance);
            
            DelFEM4NetCom::CVector3D^ managed = gcnew DelFEM4NetCom::CVector3D(unmanaged);
            
            return managed;
        }
        
        void set(DelFEM4NetCom::CVector3D^ value)
        {
            Com::CVector3D unmanaged_instance = *(value->Self);
            
            this->self->picked_pos = unmanaged_instance;
        }
    }

protected:
    Com::View::SSelectedObject *self;
 
};
  
  
ref class CCamera;

// スタティックファンクション群のクラス
public ref class DrawerGlUtility
{
public:
    /*!
     @brief Projection変換をする
     @remark glLoadIdentity()はこの関数に含まれないので，この関数を呼ぶ前に行うこと(ピックで用いるため)
     */
    static void SetProjectionTransform(CCamera^ mvp_trans);
    /*!
     @brief ModelView変換をする
     @remark glLoadIdentity()はこの関数に含まれないので，この関数を呼ぶ前に行うこと(ピックで用いるため)
     */
    static void SetModelViewTransform(CCamera^ mvp_trans);
  
    //ピック処理用セレクトバッファのクラス
    ref class PickSelectBuffer
    {
    public:
        PickSelectBuffer(unsigned int size)
        {
            this->size = size;
            this->buffer = new unsigned int[size];
        }
        
        ~PickSelectBuffer()
        {
            this->!PickSelectBuffer();
        }
        
        !PickSelectBuffer()
        {
            delete[] this->buffer;
        }
        
        array<unsigned int>^ ToArray()
        {
            array<unsigned int>^ ary = gcnew array<unsigned int>(this->size);
            for (int i = 0; i < this->size; i++)
            {
                ary[i] = this->buffer[i];
            }
            return ary;
        }
    
    public:
        property unsigned int Size
        {
            unsigned int get() { return this->size; }
        }
        
        property unsigned int * Buffer
        {
            unsigned int * get() { return this->buffer; }
        }

    private:
        unsigned int size;
        unsigned int * buffer;
    };
    
    //! ピック前処理
    static void PickPre(unsigned int size_buffer, [Out] PickSelectBuffer^% select_buffer,
                    unsigned int point_x, unsigned int point_y,
                    unsigned int delX, unsigned int delY,
                    DelFEM4NetCom::View::CCamera^ mvp_trans);
  
    //! ピック後処理
    static IList<SSelectedObject^>^ PickPost(PickSelectBuffer^ select_buffer,
                                         unsigned int point_x, unsigned int point_y,
                                         DelFEM4NetCom::View::CCamera^ mvp_trans);

public:
    static bool ReadPPM_SetTexture(String^ fname, 
                               [Out] unsigned int% texName, 
                               [Out] unsigned int% texWidth, [Out] unsigned int% texHeight);
  
    static bool WritePPM_ScreenImage(String^ fname);

};

//! Draw coordinate
public ref class CDrawerCoord : public CDrawer
{
public:
    CDrawerCoord();
    CDrawerCoord(const CDrawerCoord% rhs);
    CDrawerCoord(CCamera^ trans, unsigned int win_h );
    CDrawerCoord(Com::View::CDrawerCoord *self);
    ~CDrawerCoord();    
    !CDrawerCoord();
    
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }

    property Com::View::CDrawerCoord * Self
    {
        Com::View::CDrawerCoord * get();
    }
    
    virtual void Draw() override;
    
    // virutal Funcs which do nothing
    virtual void DrawSelection(unsigned int idraw) override;
    
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flg) override;
    virtual void ClearSelected() override;
    
    // non-virtual Funcs
    void SetTrans(CCamera^ trans)
    {
        int win_h = -1;
        SetTrans(trans, win_h);
    }
    void SetTrans(CCamera^ trans, int win_h);
    void SetIsShown(bool is_show);
    bool GetIsShown();

protected:
    Com::View::CDrawerCoord *self;

};
    
    
//! Draweing Rectangular Box for selection or specifying region
public ref class CDrawerRect : public CDrawer
{
public:
    CDrawerRect();
    CDrawerRect(const CDrawerRect% rhs);
    CDrawerRect(double x, double y);
    CDrawerRect(double x, double y, unsigned int imode);
    CDrawerRect(Com::View::CDrawerRect *self);
    ~CDrawerRect();
    !CDrawerRect();
    
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }

    property Com::View::CDrawerRect * Self
    {
        Com::View::CDrawerRect * get();
    }
    
    virtual void Draw() override;
    
    void SetInitialPositionMode(double x, double y, unsigned int imode);
    void SetPosition(double x, double y);
    void GetCenterSize([Out] double% cent_x, [Out] double% cent_y, [Out] double% size_x, [Out] double% size_y);
    void GetPosition([Out] double% x0, [Out] double% y0, [Out] double% x1, [Out] double% y1);

    // virutal Funcs
    virtual void DrawSelection(unsigned int idraw) override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flg) override;
    virtual void ClearSelected() override;

protected:
    Com::View::CDrawerRect * self;
};  
    
    
//! Draw texture in the background
public ref class CDrawerImageTexture : public CDrawer
{
public:
    CDrawerImageTexture();
    CDrawerImageTexture(const CDrawerImageTexture% rhs);
    CDrawerImageTexture(Com::View::CDrawerImageTexture *self);
    ~CDrawerImageTexture();
    !CDrawerImageTexture();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }

    property Com::View::CDrawerImageTexture * Self
    {
        Com::View::CDrawerImageTexture * get();
    }

    bool IsTexture();
    bool ReadPPM(String^ fname);
    void DeleteTexture();
    bool SetImage(unsigned int w, unsigned int h, IList<char>^ aRGB);
    virtual void Draw() override;
    
    // 以下のvirtual関数は実装されない
    virtual void DrawSelection(unsigned int idraw) override;
    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void AddSelected(array<int>^ selec_flg) override;
    virtual void ClearSelected() override;

protected:
    Com::View::CDrawerImageTexture * self;
  };  
  
} // end namespace View
} // end namespace DelFEM4NetCom


#endif
