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
/*
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_SVG_H)
#define DELFEM4NET_CAD_SVG_H

#include <string>

#include "DelFEM4Net/cad_obj2d.h"

#include "delfem/cad/cad_svg.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

public ref class CCadSVG
{
public:
    static IList<unsigned int>^ ReadSVG_AddLoopCad(String^ fname, [Out] CCadObj2D^% cad_2d)
    {
        double scale = 1;
        return ReadSVG_AddLoopCad(fname, cad_2d, scale);
    }
    static IList<unsigned int>^ ReadSVG_AddLoopCad(String^ fname, [Out] CCadObj2D^% cad_2d, double scale);
};

} // namespace DelFEM4NetCad

#endif

