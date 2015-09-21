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
@brief ２次元ベクトルクラス(Com::CVector2D)の実装
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
 */

#if !defined(DELFEM4NET_FIELD_VALUE_SETTER_H)
#define DELFEM4NET_FIELD_VALUE_SETTER_H

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/eval.h"
#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"

#include "delfem/field_value_setter.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{

public ref class CValueFieldDof
{
public:
    CValueFieldDof()
    {
        this->self = new Fem::Field::CValueFieldDof();
    }
    
    CValueFieldDof(const CValueFieldDof% rhs)
    {
        Fem::Field::CValueFieldDof rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::CValueFieldDof(rhs_instance_);
    }
    
    CValueFieldDof(Fem::Field::CValueFieldDof *self)
    {
        this->self = self;
    }
    
    ~CValueFieldDof()
    {
        this->!CValueFieldDof();
    }
    
    !CValueFieldDof()
    {
        delete this->self;
    }
    
    ////////////////
    void SetValue(String^ str)
    {
        std::string str_ = DelFEM4NetCom::ClrStub::StringToStd(str);
        
        this->self->SetValue(str_);
    }
  
    void SetValue(double val)
    {
        this->self->SetValue(val);
    }
    
    bool IsTimeDependent()
    {
        return this->self->IsTimeDependent();
    }
  
    bool GetValue(double cur_t, [Out] double% value)
    {
        double value_;
        
        bool ret = this->self->GetValue(cur_t, value_);
        
        value = value_;
        
        return ret;
    }

    String^ GetString()
    {
        const std::string& ret = this->self->GetString();
        
        String^ retManaged = DelFEM4NetCom::ClrStub::StdToString(ret);
        
        return retManaged;
    }
public:
    property Fem::Field::CValueFieldDof * Self
    {
        Fem::Field::CValueFieldDof * get() { return this->self; }
        void set(Fem::Field::CValueFieldDof *value) { this->self = value; }
    }
    
    // 0 : not set
    // 1 : const value 
    // 2 : math_exp
    property int itype
    {
        int get() { return this->self->itype; }
        void set(int value) { this->self->itype = value; }
    }

    property double val
    {
        double get() { return this->self->val; }
        void set(double value) { this->self->val = value; }
    }
    
    property String^ math_exp
    {
        String^ get()
        {
            return DelFEM4NetCom::ClrStub::StdToString(this->self->math_exp);
        }
        void set(String^ managed)
        {
            this->self->math_exp = DelFEM4NetCom::ClrStub::StringToStd(managed);
        }
    }

protected:
    Fem::Field::CValueFieldDof *self;

};


public ref class CFieldValueSetter
{    //! set saved value to the entire field
public:
    //! set constant value to the field
    static bool SetFieldValue_Constant
        (unsigned int id_field_to, unsigned int idofns, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, 
         DelFEM4NetFem::Field::CFieldWorld^ world,
         double val)
    {
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        return Fem::Field::SetFieldValue_Constant(id_field_to, idofns, fdt_, *world_, val);
    }
      
    //! set mathematical expression to the field
    static bool SetFieldValue_MathExp
        (unsigned int id_field_to, unsigned int idofns, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, 
         DelFEM4NetFem::Field::CFieldWorld^ world,                          
         String^ str_exp)
    {
        double t = 0;
        return CFieldValueSetter::SetFieldValue_MathExp(id_field_to, idofns, fdt, world, str_exp, t);
    }
    
    static bool SetFieldValue_MathExp
        (unsigned int id_field_to, unsigned int idofns, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, 
         DelFEM4NetFem::Field::CFieldWorld^ world,                          
         String^ str_exp, double t)
    {
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        std::string str_exp_ = DelFEM4NetCom::ClrStub::StringToStd(str_exp);
        
        bool ret = Fem::Field::SetFieldValue_MathExp(id_field_to, idofns, fdt_, *world_, str_exp_);
        
        return ret;
    }
      
    //! set random field to the field
    static void SetFieldValue_Random
        (unsigned int id_field_to, unsigned int idofns, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, 
         DelFEM4NetFem::Field::CFieldWorld^ world,
         double ave, double range)
    {
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
    
        return Fem::Field::SetFieldValue_Random(id_field_to, idofns, fdt_, *world_, ave, range);
    }
      
    //! copy value to the field
    static void SetFieldValue_Copy
        (unsigned int id_field_to, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt, 
         DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_from)
    {
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
    
        Fem::Field::SetFieldValue_Copy(id_field_to, fdt_, *world_, id_field_from);
    }
      
    //! set gradient value to the field
    static bool SetFieldValue_Gradient
        (unsigned int id_field_to, DelFEM4NetFem::Field::CFieldWorld^ world,
         unsigned int id_field_from)
    {
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        return Fem::Field::SetFieldValue_Gradient(id_field_to, *world_, id_field_from);
    }

public:
    CFieldValueSetter()
    {
        this->self = new Fem::Field::CFieldValueSetter();
    }
    
    CFieldValueSetter(const CFieldValueSetter% rhs)
    {
        Fem::Field::CFieldValueSetter rhs_instance_ = *(rhs.self);
        this->self = new Fem::Field::CFieldValueSetter(rhs_instance_);
    }
    
    CFieldValueSetter(Fem::Field::CFieldValueSetter *self)
    {
        this->self = self;
    }
    
    CFieldValueSetter(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        Fem::Field::CFieldWorld *world_ = world->Self;
        this->self = new Fem::Field::CFieldValueSetter(id_field, *world_);
    }
    
    ~CFieldValueSetter()
    {
        this->!CFieldValueSetter();
    }
    
    !CFieldValueSetter()
    {
        delete this->self;
    }

    property Fem::Field::CFieldValueSetter * Self
    {
        Fem::Field::CFieldValueSetter * get() { return this->self; }
        void set(Fem::Field::CFieldValueSetter *value) { this->self = value; }
    }

    void Clear()
    {
        this->self->Clear();
    }

    void SetMathExp
        (String^ math_exp, unsigned int idof, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt,
         DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        std::string math_exp_ = DelFEM4NetCom::ClrStub::StringToStd(math_exp);
        
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        this->self->SetMathExp(math_exp_, idof, fdt_, *world_);
    }
    
    void SetConstant
        (double val, unsigned int idof, DelFEM4NetFem::Field::FIELD_DERIVATION_TYPE fdt,
         DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        Fem::Field::FIELD_DERIVATION_TYPE fdt_ = static_cast<Fem::Field::FIELD_DERIVATION_TYPE>(fdt);
        Fem::Field::CFieldWorld *world_ = world->Self;
    
        this->self->SetConstant(val, idof, fdt_, *world_);
    }

    void SetGradient
        (unsigned int id_field_from, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        this->self->SetGradient(id_field_from, *world_);
    }
  
    bool ExecuteValue(double time, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        Fem::Field::CFieldWorld *world_ = world->Self;
        
        return this->self->ExecuteValue(time, *world_);
    }
  
protected:
    Fem::Field::CFieldValueSetter *self;

};
  
}
}

#endif
