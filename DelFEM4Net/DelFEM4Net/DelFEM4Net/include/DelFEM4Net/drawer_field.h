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

// this fiele define abstract fem discretized field (Fem::Field::View::CDrawerField)
// and utility classes
/*
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_DRAWER_FIELD_H)
#define DELFEM4NET_DRAWER_FIELD_H

#include <memory> //auto_ptr

#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/drawer_gl_utility.h"
#include "DelFEM4Net/elem_ary.h" // needed for Fem::Field::ELEM_TYPE

//warning C4244: '=' : 'double' から 'float' への変換です。データが失われる可能性があります。
//を抑制する
#if defined(__VISUALC__)
    #pragma warning ( disable : 4244 )
#endif
#include "delfem/drawer_field.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{

namespace Field
{
ref class CFieldWorld;

namespace View
{

public ref class CColorMap
{
public:
    CColorMap ()
    {
       this->self = new Fem::Field::View::CColorMap();
    }
    
    CColorMap(const CColorMap% rhs)
    {
        Fem::Field::View::CColorMap rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::View::CColorMap(rhs_instance_);
    }
    
    CColorMap(double min, double max)
    {
       this->self = new Fem::Field::View::CColorMap(min, max);
    }
    
    CColorMap(Fem::Field::View::CColorMap *self)
    {
        this->self = self;
    }
    
    virtual ~CColorMap()
    {
        this->!CColorMap();
    }
    
    !CColorMap()
    {
        delete this->self;
    }
    
    property Fem::Field::View::CColorMap * Self
    {
        Fem::Field::View::CColorMap *get() { return this->self; }
    }

    virtual void GetColor([Out] array<float>^% color, double val )  // [Out] color[3]
    {
        color = gcnew array<float>(3);
        pin_ptr<float> ptr = &color[0];
        float *color_ = (float *)ptr;

        this->self->GetColor(color_, val);
    }

    virtual bool IsMinMaxFix()
    {
        return this->self->IsMinMaxFix();
    }
    
    virtual void SetMinMax(double min, double max)
    {
        this->self->SetMinMax(min, max);
    }
    
    double GetMax()
    {
        return this->self->GetMax();
    }

    double GetMin()
    {
        return this->self->GetMin();
    }

    static void DrawColorLegend(CColorMap^ map)
    {
        Fem::Field::View::DrawColorLegend(*(map->self));
    }

protected:
    Fem::Field::View::CColorMap * self;
    
};

// CColorMapのstaticメソッドへ移動
//void DrawColorLegend(CColorMap^ map);


//! IndexArray storing class for OpenGL vertex array
public ref class CIndexArrayElem
{
public : 
    /*
    CIndexArrayElem ()
    {
       this->self = new Fem::Field::View::CIndexArrayElem();
    }
    */
    
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CIndexArrayElem(const CIndexArrayElem% rhs)
    {
        const Fem::Field::View::CIndexArrayElem& rhs_instance_ = *(rhs.self);
        //shallow copyになるので問題あり
        //this->self = new Fem::Field::View::CIndexArrayElem(rhs_instance_);
        assert(false);
    }
    
public:
    CIndexArrayElem(unsigned int id_ea, unsigned int id_es, DelFEM4NetFem::Field::CFieldWorld^ world);
    
private: // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CIndexArrayElem(Fem::Field::View::CIndexArrayElem *self)
    {
        this->self = self;
    }

public:
    virtual ~CIndexArrayElem()
    {
        this->!CIndexArrayElem();
    }
    
    !CIndexArrayElem()
    {
        delete this->self;
    }
    
    property Fem::Field::View::CIndexArrayElem * Self
    {
        Fem::Field::View::CIndexArrayElem *get() { return this->self; }
    }

    void DrawElements()
    {
        this->self->DrawElements();
    }

    unsigned int GetElemDim()
    {
        return this->self->GetElemDim();
    }
    
    unsigned int GetIdEA()
    {
        return this->self->GetIdEA();
    }
    
    void SetColor(double r, double g, double b)
    {
        this->self->SetColor(r, g, b);
    }

    bool SetColor(unsigned int id_es_v, unsigned int id_ns_v, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map);

    unsigned int GetSize()
    {
        return this->self->GetSize();
    }
    
    // GetNoesのnoの配列サイズ(nnoel) nativeのnnoelは非公開メンバーなので追加
    unsigned int GetNNoel()
    {
        DelFEM4NetFem::Field::ELEM_TYPE itype = this->GetElemType();
        unsigned int nnoel = 0;
        switch (itype)
        {
            case DelFEM4NetFem::Field::ELEM_TYPE::LINE: nnoel = 2; break;
            case DelFEM4NetFem::Field::ELEM_TYPE::TRI:  nnoel = 3; break;
            case DelFEM4NetFem::Field::ELEM_TYPE::QUAD: nnoel = 4; break;
            case DelFEM4NetFem::Field::ELEM_TYPE::TET:  nnoel = 3; break;
            case DelFEM4NetFem::Field::ELEM_TYPE::HEX:  nnoel = 4; break;
        }
        return nnoel;
    }

    void GetNoes(unsigned int ielem, [Out] array<unsigned int>^% no)  //[OUT] no
    {
        unsigned int nnoel = this->GetNNoel();
        no = gcnew array<unsigned int>(nnoel);
        
        pin_ptr<unsigned int> ptr = &no[0];
        unsigned int *no_ = (unsigned int*)ptr;
        
        this->self->GetNoes(ielem, no_);
        
    }
    
    DelFEM4NetFem::Field::ELEM_TYPE GetElemType()
    {
        Fem::Field::ELEM_TYPE itype_ = this->self->GetElemType();
        return static_cast<DelFEM4NetFem::Field::ELEM_TYPE>(itype_);
    }

public:
    property int ilayer
    {
        int get() { return this->self->ilayer; }
        void set(int value) { this->self->ilayer = value; }
    }

    property array<float>^ color  // color[3]
    {
        array<float>^ get()
        {
            array<float>^ ary = gcnew array<float>(3);
            for (int i = 0; i < 3; i++)
            {
                ary[i] = this->self->color[i];
            }
            return ary;
        }
        void set(array<float>^ ary)
        {
            assert(ary->Length == 3);
            for (int i = 0; i < 3; i++)
            {
                this->self->color[i] = ary[i];
            }
        }
    }

protected:
    Fem::Field::View::CIndexArrayElem *self;

};

//! class storing vertex index
public ref class CIndexVertex
{
public:
    /*
    CIndexVertex ()
    {
       this->self = new Fem::Field::View::CIndexVertex();
    }
    */

    CIndexVertex(const CIndexVertex% rhs)
    {
        Fem::Field::View::CIndexVertex rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::View::CIndexVertex(rhs_instance_);
    }
    
    CIndexVertex(unsigned int id_v, unsigned int id_ea, unsigned int id_es)
    {
       this->self = new Fem::Field::View::CIndexVertex(id_v, id_ea, id_es);
    }
    
    CIndexVertex(Fem::Field::View::CIndexVertex *self)
    {
        this->self = self;
    }
    
    virtual ~CIndexVertex()
    {
        this->!CIndexVertex();
    }
    
    !CIndexVertex()
    {
        delete this->self;
    }
    
    property Fem::Field::View::CIndexVertex * Self
    {
        Fem::Field::View::CIndexVertex *get() { return this->self; }
    }

public:
    property unsigned int id_v
    {
        unsigned int get() { return this->self->id_v; }
        void set(unsigned int value) { this->self->id_v = value; }
    }

    property unsigned int id_ea
    {
        unsigned int get() { return this->self->id_ea; }
        void set(unsigned int value) { this->self->id_ea = value; }
    }

    property unsigned int id_es
    {
        unsigned int get() { return this->self->id_es; }
        void set(unsigned int value) { this->self->id_es = value; }
    }

    property bool is_selected
    {
        bool get() { return this->self->is_selected; }
        void set(bool value) { this->self->is_selected = value; }
    }

protected:
    Fem::Field::View::CIndexVertex * self;
    
};

////////////////////////////////////////////////////////////////

//! abstruct class for field visualization
public ref class CDrawerField abstract : public DelFEM4NetCom::View::CDrawer 
{
public:
    virtual bool Update(DelFEM4NetFem::Field::CFieldWorld^ world) = 0;
};

////////////////////////////////////////////////////////////////

//! array of DrawerField class
public ref class CDrawerArrayField : public DelFEM4NetCom::View::CDrawerArray
{
public:
    CDrawerArrayField () : CDrawerArray((Com::View::CDrawerArray *)NULL)  // 基本クラスのコンストラクタでnativeインスタンスを作成しない
    {
       this->self = new Fem::Field::View::CDrawerArrayField();
       
       // 基本クラスのselfを設定
       setBaseSelf();
    }
    
    CDrawerArrayField(const CDrawerArrayField% rhs) : CDrawerArray((Com::View::CDrawerArray *)NULL)  // 基本クラスのコンストラクタでnativeインスタンスを作成しない
    {
        Fem::Field::View::CDrawerArrayField rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::View::CDrawerArrayField(rhs_instance_);

       // 基本クラスのselfを設定
       setBaseSelf();
    }
    
    CDrawerArrayField(Fem::Field::View::CDrawerArrayField *self) : CDrawerArray((Com::View::CDrawerArray *)NULL) // 基本クラスのコンストラクタでnativeインスタンスを作成しない
    {
        this->self = self;

       // 基本クラスのselfを設定
       setBaseSelf();
    }
    
    virtual ~CDrawerArrayField()
    {
        this->!CDrawerArrayField();
    }
    
    !CDrawerArrayField()
    {
        delete this->self;
        
        // 基本クラスでdelete処理が無効となるように基本クラスのselfにNULLを設定する
        this->self = NULL;
        setBaseSelf();
    }
    
    property Fem::Field::View::CDrawerArrayField * Self
    {
        Fem::Field::View::CDrawerArrayField *get() { return this->self; }
    }

    bool Update(DelFEM4NetFem::Field::CFieldWorld^ world);

    virtual void PushBack(DelFEM4NetCom::View::CDrawer^ pDrawer)
    {
        //Com::View::CDrawer *pDrawer_ = pDrawer->Self;
        //this->self->PushBack(p_Drawer);

        CDrawerArray::PushBack(pDrawer);
    }

protected:
    Fem::Field::View::CDrawerArrayField * self;
    
    virtual void setBaseSelf()
    {
        CDrawerArray::self = this->self;
    }
    
};

}    // end namespace View
}    // end namespace Feild
}    // end namespace Fem

#endif
