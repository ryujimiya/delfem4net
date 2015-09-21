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
@brief シリアライゼーションのためのクラス(Com::Serializer)のインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_SERIALIZE_H)
#define DELFEM4NET_SERIALIZE_H

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include "delfem/serialize.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

//! class for serialization
public ref class CSerializer
{
public:
    //CSerializer();
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CSerializer(const CSerializer% rhs);
public:
    CSerializer(String^ fname, bool is_loading);
    CSerializer(String^ fname, bool is_loading, bool isnt_binary);
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CSerializer(Com::CSerializer *self);
public:
    ~CSerializer();
    !CSerializer();
    property Com::CSerializer * Self
    {
        Com::CSerializer * get();
    }

    bool IsLoading();
    void Get(String^ format, ... array<Object^>^ arg);
    //void Get(const char* format,...);
    String^ GetLine();
    //void GetLine(char* buffer, unsigned int buff_size);
    void Out(String^ format, ... array<Object^>^ arg);
    //void Out(const char* format,...);
    String^ ReadDepthClassName();
    //void ReadDepthClassName(char* class_name, unsigned int buff_size);
    void WriteDepthClassName(String^ class_name);
    void ShiftDepth( bool is_add );
    unsigned int GetDepth();
    void Close();
    
protected:
    static unsigned int MAX_LINE_BUFFER_SIZE = 1024;
    Com::CSerializer *self;
};

}

#endif
