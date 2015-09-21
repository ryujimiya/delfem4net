/*
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

#include <stdio.h>
#include <iostream>
#include <stack>

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/cad/cad_svg.h"
//#include "DelFEM4Net/objset.h"

using namespace DelFEM4NetCad;

IList<unsigned int>^ CCadSVG::ReadSVG_AddLoopCad(String^ fname, [Out] CCadObj2D^% cad_2d, double scale)
{
    const std::string& fname_ = DelFEM4NetCom::ClrStub::StringToStd(fname);
    // 格納先を生成
    Cad::CCadObj2D *cad_2d_ = new Cad::CCadObj2D();
    cad_2d = gcnew CCadObj2D(cad_2d_);
    
    const std::vector<unsigned int>& vec = Cad::ReadSVG_AddLoopCad(fname_, *cad_2d_, scale);
    
    IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList<unsigned int>(vec);
    
    return list;
}

