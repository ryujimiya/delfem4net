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


///////////////////////////////////////////////////////////////////////////////
// serialize.cpp : 
///////////////////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#pragma warning ( disable : 4786 )
#pragma warning ( disable : 4996 )
#endif

#include <vector>
#include <string>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/serialize.h"

using namespace DelFEM4NetCom;


/*
CSerializer::CSerializer()
{
    this->self = new Com::CSerializer();
}
*/

CSerializer::CSerializer(const CSerializer% rhs)
{
    const Com::CSerializer& rhs_instance_ = *(rhs.self);
    //shallow copyになるので問題あり
    //this->self = new Com::CSerializer(rhs_instance_) ; // コピーコンストラクタを使用
    assert(false);
}

CSerializer::CSerializer(String^ fname, bool is_loading)
{
    std::string fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);

    this->self = new Com::CSerializer(fname_, is_loading);
}

CSerializer::CSerializer(String^ fname, bool is_loading, bool isnt_binary)
{
    std::string fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);

    this->self = new Com::CSerializer(fname_, is_loading, isnt_binary);
}

CSerializer::CSerializer(Com::CSerializer *self)
{
    this->self = self;
}

CSerializer::~CSerializer()
{
    this->!CSerializer();
}

CSerializer::!CSerializer()
{
    //this->self->Close();
    delete this->self;
}

Com::CSerializer* CSerializer::Self::get()
{
    return this->self;
}

bool CSerializer::IsLoading()
{
    return this->self->IsLoading();
}

void CSerializer::Get(String^ format, ... array<Object^>^ arg)
{
    gcnew NotImplementedException();
}

String^ CSerializer::GetLine()
{
    String^ retManaged = "";
    
    char *buffer = NULL;
    try
    {
         buffer = new char[MAX_LINE_BUFFER_SIZE];
         this->self->GetLine(buffer, MAX_LINE_BUFFER_SIZE);
         retManaged = DelFEM4NetCom::ClrStub::PtrToString(buffer);
    }
    catch(Exception^ exception)
    {
        Console::WriteLine(exception->StackTrace);
        throw;
    }
    finally
    {
        if (buffer != NULL)
        {
            delete[] buffer;
        }
    }
    return retManaged;
}

void CSerializer::Out(String^ format, ... array<Object^>^ arg)
{
    gcnew NotImplementedException();
}

String^ CSerializer::ReadDepthClassName()
{
    String^ retManaged = "";
    
    char *buffer = NULL;
    try
    {
         buffer = new char[MAX_LINE_BUFFER_SIZE];
         this->self->ReadDepthClassName(buffer, MAX_LINE_BUFFER_SIZE);
         retManaged = DelFEM4NetCom::ClrStub::PtrToString(buffer);
    }
    catch(Exception^ exception)
    {
        Console::WriteLine(exception->StackTrace);
        throw;
    }
    finally
    {
        if (buffer != NULL)
        {
            delete[] buffer;
        }
    }
    return retManaged;
}

void CSerializer::WriteDepthClassName(String^ class_name)
{
    std::string class_name_ = DelFEM4NetCom::ClrStub::StringToStd(class_name);

    this->self->WriteDepthClassName(class_name_.c_str());
    
}

void CSerializer::ShiftDepth( bool is_add )
{
    this->self->ShiftDepth(is_add);
}

unsigned int CSerializer::GetDepth()
{
    return this->self->GetDepth();
}

void CSerializer::Close()
{
    this->self->Close();
}

