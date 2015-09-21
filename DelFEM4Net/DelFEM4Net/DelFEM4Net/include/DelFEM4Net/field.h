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
@brief interface of FEM descretization field class (Fem::Field::CField)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_FIELD_H)
#define DELFEM4NET_FIELD_H

#if defined(__VISUALC__)
    #pragma warning ( disable : 4996 )
#endif

#include <string>
//#include <stdio.h>

#include "DelFEM4Net/elem_ary.h"
#include "DelFEM4Net/node_ary.h"

//warning C4244: '=' : 'double' から 'float' への変換です。データが失われる可能性があります。
//を抑制する
#if defined(__VISUALC__)
    #pragma warning ( disable : 4244 )
#endif

#include "delfem/field.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{
  
ref class CFieldWorld;

//! the type of field
public enum class FIELD_TYPE
{ 
  NO_VALUE,    //!< not setted   
  SCALAR,        //!< real value scalar
  VECTOR2,    //!< 2D vector
  VECTOR3,    //!< 3D vector
  STSR2,        //!< 2D symmetrical tensor
  ZSCALAR        //!< complex value scalar
};

//! types of the value derivaration
[Flags]
public enum class FIELD_DERIVATION_TYPE
{ 
    VALUE=1,        //!< not being derivated
    VELOCITY=2,        //!< first time derivative (velocity)
    ACCELERATION=4    //!< 2nd time derivative (acceralation)
};

// このネーミング規則分かりにくいかも？
// cbef(corner,bubble,edge,face)の順. 番号はそれぞれに幾つ頂点があるか．
// [要素タイプ] (座標)[cbef],(値)[cbef]のように定義する
// befのうち座標，値とも０ならば，後ろから(febの順に)省略してよい
////////////////
//! the types of interpolation
public enum class INTERPOLATION_TYPE
{
    LINE11,        //!< line element (1st order)
    TRI11,        //!< triangle element (1st order)
    TRI1001,    //!< triangle element (constatnt value in elemnet)
    TRI1011,    //!< triangle element (bubble interpolation)
    TET11,        //!< tetrahedral element (1st order)
    TET1001,    //!< tetrahedral element (constant value in element)
    HEX11,        //!< hexagonal element (1st order,iso-parametric)
    HEX1001        //!< hexagonal element (constant value in element)
};    

/*! 
@brief fem discretization class
@ingroup Fem
*/
public ref class CField
{
public:
    //! store ID of element segment
    ref class CElemInterpolation
    {
    public:
        /*
        CElemInterpolation()
        {
            this->self = new Fem::Field::CField::CElemInterpolation();
        }
        */

        CElemInterpolation(const CElemInterpolation% rhs)
        {
            const Fem::Field::CField::CElemInterpolation& rhs_instance_ = *(rhs.self);
            this->self = new Fem::Field::CField::CElemInterpolation(rhs_instance_);
        }
        
        CElemInterpolation(Fem::Field::CField::CElemInterpolation *self)
        {
            this->self = self;
        }
        
        CElemInterpolation(unsigned int id_ea, 
           unsigned int id_es_c_va, unsigned int id_es_c_co,
           unsigned int id_es_e_va, unsigned int id_es_e_co,
           unsigned int id_es_b_va, unsigned int id_es_b_co)
        {
            this->self = new Fem::Field::CField::CElemInterpolation(id_ea,
                id_es_c_va, id_es_c_co,
                id_es_e_va, id_es_e_co,
                id_es_b_va, id_es_b_co);
        }
        
        ~CElemInterpolation()
        {
            this->!CElemInterpolation();
        }
        
        !CElemInterpolation()
        {
            delete this->self;
        }
        
    public:
        property Fem::Field::CField::CElemInterpolation * Self
        {
            Fem::Field::CField::CElemInterpolation * get() { return this->self; }
        }
        
        property unsigned int id_ea    //!< ID of element array
        {
            unsigned int get() { return this->self->id_ea; }
            void set(unsigned int value) { this->self->id_ea = value; }
        }
        
        property unsigned int id_es_c_va    //!< ID of element segment of corner (value or coordinate )
        {
            unsigned int get() { return this->self->id_es_c_va; }
            void set(unsigned int value) { this->self->id_es_c_va = value; }
        }
        
        property unsigned int id_es_c_co
        {
            unsigned int get() { return this->self->id_es_c_co; }
            void set(unsigned int value) { this->self->id_es_c_co = value; }
        }
        property unsigned int id_es_e_va    //!< ID of element segment of edge   (value or coordinate )
        {
            unsigned int get() { return this->self->id_es_e_va; }
            void set(unsigned int value) { this->self->id_es_e_va = value; }
        }
        
        property unsigned int id_es_e_co
        {
            unsigned int get() { return this->self->id_es_e_co; }
            void set(unsigned int value) { this->self->id_es_e_co = value; }
        }
        
        property unsigned int id_es_b_va    //!< ID of element segment of bubble (value or coordinate )
        {
            unsigned int get() { return this->self->id_es_b_va; }
            void set(unsigned int value) { this->self->id_es_b_va = value; }
        }
        
        property unsigned int id_es_b_co
        {
            unsigned int get() { return this->self->id_es_b_co; }
            void set(unsigned int value) { this->self->id_es_b_co = value; }
        }
        
        property int ilayer
        {
            int get() { return this->self->ilayer; }
            void set(int value) { this->self->ilayer = value; }
        }

    protected:
        Fem::Field::CField::CElemInterpolation *self;

    };
    
public: 
    //! store ID of node segment in eace node configuration (CORNER, BUBBLE or EDGE..etc)
    ref class CNodeSegInNodeAry
    {
    public:
        CNodeSegInNodeAry()
        {
            this->self = new Fem::Field::CField::CNodeSegInNodeAry();
        }
        
        CNodeSegInNodeAry(const CNodeSegInNodeAry% rhs)
        {
            const Fem::Field::CField::CNodeSegInNodeAry rhs_instance_ = *(rhs.self);
            this->self = new Fem::Field::CField::CNodeSegInNodeAry(rhs_instance_);
        }
        
        CNodeSegInNodeAry(Fem::Field::CField::CNodeSegInNodeAry *self)
        {
            this->self = self;
        }
        
        CNodeSegInNodeAry(unsigned int id_na_co, bool is_part_co, unsigned int id_ns_co,
            unsigned int id_na_va, bool is_part_va, 
            unsigned int id_ns_va, unsigned int id_ns_ve, unsigned int id_ns_ac )
        {
            this->self = new Fem::Field::CField::CNodeSegInNodeAry(id_na_co, is_part_co, id_ns_co,
                id_na_va, is_part_va,
                id_ns_va, id_ns_ve, id_ns_ac);
        }
        
        ~CNodeSegInNodeAry()
        {
            this->!CNodeSegInNodeAry();
        }
        
        !CNodeSegInNodeAry()
        {
            delete this->self;
        }

    public:
        property Fem::Field::CField::CNodeSegInNodeAry * Self
        {
            Fem::Field::CField::CNodeSegInNodeAry * get() { return this->self; }
        }
    
        property unsigned int id_na_co
        {
            unsigned int get() { return this->self->id_na_co; }
            void set(unsigned int value) { this->self->id_na_co = value; }
        }
        
        property bool is_part_co        //!< is all the nodes are refered by element
        {
            bool get() { return this->self->is_part_co; }
            void set(bool value) { this->self->is_part_co = value; }
        }
        
        property unsigned int id_ns_co
        {
            unsigned int get() { return this->self->id_ns_co; }
            void set(unsigned int value) { this->self->id_ns_co = value; }
        }
        
        property unsigned int id_na_va
        {
            unsigned int get() { return this->self->id_na_va; }
            void set(unsigned int value) { this->self->id_na_va = value; }
        }
        
        property bool is_part_va
        {
            bool get() { return this->self->is_part_va; }
            void set(bool value) { this->self->is_part_va = value; }
        }
        
        property unsigned int id_ns_va    //!< value
        {
            unsigned int get() { return this->self->id_ns_va; }
            void set(unsigned int value) { this->self->id_ns_va = value; }
        }
        
        property unsigned int id_ns_ve    //!< velocity
        {
            unsigned int get() { return this->self->id_ns_ve; }
            void set(unsigned int value) { this->self->id_ns_ve = value; }
        }
        
        property unsigned int id_ns_ac    //!< acceleration
        {
            unsigned int get() { return this->self->id_ns_ac; }
            void set(unsigned int value) { this->self->id_ns_ac = value; }
        }
        
    protected:
        Fem::Field::CField::CNodeSegInNodeAry *self;

    };

public:
    CField();
    CField(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CField(const CField% rhs);
    CField(Fem::Field::CField *self);
    CField(unsigned int id_field_parent,    // parent field
        IList<CField::CElemInterpolation^>^ aEI, 
        CField::CNodeSegInNodeAry^ nsna_c, CField::CNodeSegInNodeAry^ nsna_b, 
        CFieldWorld^ world );
    virtual ~CField();
    !CField();
    property Fem::Field::CField * Self
    {
        Fem::Field::CField * get();
    }

    bool IsValid(); //!< get the validation flag
    bool AssertValid(CFieldWorld^ world);    //!< check if it is valid
    unsigned int GetNDimCoord();    //!< get dimension of the coordinate
    unsigned int GetNLenValue(); //!< get length of the value
    DelFEM4NetFem::Field::FIELD_TYPE GetFieldType();     //!< get type of field (i.e. SCALAR,VECTOR2)
    unsigned int GetFieldDerivativeType();
  
    // TODO:ここは高次補間では要書き換え(PartialかどうかはELSEG_TYPEごとに違う)
    //! 部分場かどうかを調べる
    bool IsPartial();
  
    // TODO:ここは高次補間では要書き換え(PartialかどうかはELSEG_TYPEごとに違う)
    // 親フィールドなら０を返す
    unsigned int GetIDFieldParent();
    
    IList<unsigned int>^ GetAryIdEA();

    //! 補完の種類を得る
    DelFEM4NetFem::Field::INTERPOLATION_TYPE GetInterpolationType(unsigned int id_ea, CFieldWorld^ world);
    //! ElemSegを得る関数
    CElemAry::CElemSeg^ GetElemSeg(unsigned int id_ea, ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world );
    unsigned int GetIdElemSeg(unsigned int id_ea, ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world );

    //! NodeSegを得る関数
    CNodeAry::CNodeSeg^ GetNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world)
    {
        //unsigned int derivative_type = 1;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        return GetNodeSeg(elseg_type, is_value, world, derivative_type);
    }
    CNodeAry::CNodeSeg^ GetNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world, /*unsigned int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type);
    
    //! NodeSegmentのIDかどうかを調べる
    bool IsNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world)
    {
        //unsigned int derivative_type = 7;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type = FIELD_DERIVATION_TYPE::VALUE|FIELD_DERIVATION_TYPE::VELOCITY|FIELD_DERIVATION_TYPE::ACCELERATION;
        return IsNodeSeg(elseg_type, is_value, world, derivative_type);
    }
    bool IsNodeSeg(ELSEG_TYPE elseg_type, bool is_value, CFieldWorld^ world, /*unsigned int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE derivative_type);
    
    CField::CNodeSegInNodeAry^ GetNodeSegInNodeAry(DelFEM4NetFem::Field::ELSEG_TYPE es_type);

    //! 最大値最小値を取得
    void GetMinMaxValue([Out] double% min, [Out] double% max, CFieldWorld^ world)
    {
        unsigned int idof = 0;
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        GetMinMaxValue(min, max, world, idof, fdt);
    }
    void GetMinMaxValue([Out] double% min, [Out] double% max, CFieldWorld^ world,
                      unsigned int idof)
    {
        DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt = DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE::VALUE;
        GetMinMaxValue(min, max, world, idof, fdt);
    }
    void GetMinMaxValue([Out] double% min, [Out] double% max, CFieldWorld^ world, 
                      unsigned int idof, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt);

    // 場の追加
    bool SetValueType( DelFEM4NetFem::Field::FIELD_TYPE field_type, /*int*/DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, CFieldWorld^ world);
    int GetLayer(unsigned int id_ea);
  
    // TODO: 一度この関数を呼んだら，次に呼んだ時は高速に処理されるように，ハッシュを構築する
    // TODO: 座標やコネクティビティの変更があった場合は，ハッシュを削除する
    bool FindVelocityAtPoint(array<double>^ velo,
        [Out] unsigned int %id_ea_stat, [Out] unsigned int% ielem_stat, [Out] double% r1, [Out] double% r2,
        array<double>^ co, DelFEM4NetFem::Field::CFieldWorld^ world);
  
    unsigned int GetMapVal2Co(unsigned int inode_va);

    ////////////////////////////////
    // ＩＯ入出力のための関数

    // MicroAVS inpファイルへの書き出し
    bool ExportFile_Inp(String^ file_name, CFieldWorld^ world);
    
protected:
    Fem::Field::CField *self;

};    // end class CField;

// ポインタ操作用クラス
public ref class CFieldPtr : public CField
{
public:
    CFieldPtr(Fem::Field::CField *baseSelf) : CField(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CFieldPtr(const CFieldPtr% rhs) : CField(false)
    {
        this->self = rhs.self;
    }

public:
    ~CFieldPtr()
    {
        this->!CFieldPtr();
    }

    !CFieldPtr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace Field
}    // end namespace DelFEM4NetFem

#endif
