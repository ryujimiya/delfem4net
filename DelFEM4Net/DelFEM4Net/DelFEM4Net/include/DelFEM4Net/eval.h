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
@brief 数式評価クラス(Fem::Field::CEval)のインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EVAL_H)
#define DELFEM4NET_EVAL_H

#include <iostream>
#include <string>
#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"
#include "delfem/eval.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{

//ref class CCmd;

/*!
@brief 数式評価クラス

文字列で表された数式を評価します．
*/
public ref class CEval
{
public:
    ref class CKey
    {    // 数式の要素の値を保持するクラス
    public:
        /*
        CKey()
        {
            this->self = new Fem::Field::CEval::CKey();
        }
        */

        CKey(const CKey% rhs)
        {
            const Fem::Field::CEval::CKey& rhs_instance_ = *(rhs.self);
            this->self = new Fem::Field::CEval::CKey(rhs_instance_);
        }
        
        CKey(Fem::Field::CEval::CKey *self)
        {
            this->self = self;
        }
        
        CKey(String^ name, double val)
        {
            const std::string& name_ = DelFEM4NetCom::ClrStub::StringToStd(name);

            this->self = new Fem::Field::CEval::CKey(name_, val);
        }
        
        ~CKey()
        {
            this->!CKey();
        }
        
        !CKey()
        {
            delete this->self;
        }

        property Fem::Field::CEval::CKey * Self
        {
            Fem::Field::CEval::CKey * get() { return this->self; }
        }
    
        property String^ m_Name
        {
            String^ get()
            {
                return DelFEM4NetCom::ClrStub::StdToString(this->self->m_Name);
            }
            void set(String^ managed)
            {
                this->self->m_Name = DelFEM4NetCom::ClrStub::StringToStd(managed);
            }
        }
        
        property IList<unsigned int>^ m_aiCmd
        {
            IList<unsigned int>^ get()
            {
                const std::vector<unsigned int>& vec = this->self->m_aiCmd;

                IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
                return list;
            }
            
            void set(IList<unsigned int>^ list)
            {
                 std::vector<unsigned int>& vec = this->self->m_aiCmd;
                 
                 DelFEM4NetCom::ClrStub::ListToVector<unsigned int>(list, vec);
            }
        }
        
        property double m_Val
        {
            double get() { return this->self->m_Val; }
            void set(double value) { this->self->m_Val = value; }
        }

    protected:
        Fem::Field::CEval::CKey *self;

    };
    
public:
    CEval()
    {
        this->self = new Fem::Field::CEval();
    }
    
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEval(const CEval% rhs)
    {
        // nativeインスタンスがshallow copyになるので問題あり
        //this->self = new Fem::Field::CEval(*(rhs.self));
    }

    CEval(Fem::Field::CEval *self)
    {
        this->self = self;
    }
    
public:
    CEval(String^ exp)
    {
        std::string exp_ = DelFEM4NetCom::ClrStub::StringToStd(exp);

        this->self = new Fem::Field::CEval(exp_);
    }
    
    virtual ~CEval()
    {
        this->!CEval();
    }
    
    !CEval()
    {
        delete this->self;
    }
    
    property Fem::Field::CEval * Self
    {
        Fem::Field::CEval * get() { return this->self; }
    }


    ////////////////
    bool SetExp(String^ key_name)
    {
        std::string key_name_ = DelFEM4NetCom::ClrStub::StringToStd(key_name);
        
        bool ret = this->self->SetExp(key_name_);

        return ret;
    }

    void SetKey(String^ key_name, double val)
    {
        std::string key_name_ = DelFEM4NetCom::ClrStub::StringToStd(key_name);

        this->self->SetKey(key_name_, val);
    }
    
    bool IsKeyUsed(String^ key_name)
    {
        std::string key_name_ = DelFEM4NetCom::ClrStub::StringToStd(key_name);

        bool ret = this->self->IsKeyUsed(key_name_);

        return ret;
    }

    double Calc()
    {
        return this->self->Calc();
    }

protected:
    Fem::Field::CEval *self;

};

}
}

#endif    // !defind EVAL_H
