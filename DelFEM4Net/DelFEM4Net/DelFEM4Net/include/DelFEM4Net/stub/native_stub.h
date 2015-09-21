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
@brief  C++/CLI(CLR) stub for DelFEM
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_NATIVE_STUB)
#define DELFEM4NET_NATIVE_STUB

#include <assert.h>
#include <vector>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;


////////////////////////////////////////////////////////////////////////////////
// nativeライブラリのwarning除去
////////////////////////////////////////////////////////////////////////////////
// nativeライブラリのヘッダ
#include "delfem/drawer_msh.h"
#include "delfem/eval.h"

// warning LNK4248: 未解決の typeref トークン (01000026) ('Msh.View.CDrawPart') です。イメージを実行できません。
// warning LNK4248: 未解決の typeref トークン (01000029) ('Fem.Field.CCmd') です。イメージを実行できません。

////////////////////////////////////////////////////////////////
//DelFEM_VCStaticLibrary/src/msh/drawer_msh.cppより抽出
namespace Msh
{
namespace View
{
class CDrawPart
{
public : 
    CDrawPart();
    CDrawPart(const Msh::CTriAry2D& TriAry);
    CDrawPart(const Msh::CTriAry3D& TriAry);
    CDrawPart(const Msh::CBarAry& BarAry);
    CDrawPart(const Msh::CBarAry3D& BarAry);
    CDrawPart(const Msh::CQuadAry2D& QuadAry);
    CDrawPart(const Msh::CQuadAry3D& QuadAry);
    CDrawPart(const Msh::CTetAry& TetAry);
    CDrawPart(const Msh::CHexAry& HexAry);
    ////////////////
    ~CDrawPart();
    void DrawElements();
    void DrawElements_Select();
    unsigned int GetElemDim() const;
    void SetHeight(double height);
    double GetHeight() const;
private:
    void DrawElements_Bar();
    void DrawElements_Tri();
    void DrawElements_Quad();
    void DrawElements_Tet();
    void DrawElements_Hex();
    ////////////////
    void DrawElements_Select_Bar();
    void DrawElements_Select_Tri();
    void DrawElements_Select_Quad();
    void DrawElements_Select_Tet();
    void DrawElements_Select_Hex();
public:
    bool is_selected;
    bool is_shown;
    std::vector<unsigned int> selec_elem;
    unsigned int id_msh;
    unsigned int id_cad;
    ////////////////
    double r, g, b;
    unsigned int line_width;
    ////////////////
    unsigned int nelem;
    unsigned int npoel;
    unsigned int* pIA_Elem;
    unsigned int nedge;
    unsigned int* pIA_Edge;
    ////////////////
    double height;
private:
    unsigned int type_elem;  // bar:1，tri:2，quad:3, tet:4, hex:5
};
} // namespace View
} // namespace Msh

////////////////////////////////////////////////////////////////
//DelFEM_VCStaticLibrary/src/femfield/eval.cppより抽出
namespace Fem
{
namespace Field
{

class CCmd
{
public:
	virtual bool DoOperation(std::vector<double>& stack) = 0;
	virtual void SetValue(const double& val) = 0;
};
} // namesace Field
} // namespace Fem

#endif