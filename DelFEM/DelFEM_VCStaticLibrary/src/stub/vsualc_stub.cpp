﻿/*
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

////////////////////////////////////////////////////////////////
// visualc_stub.cpp: Visual C++ 2010でスタティックライブラリを作成する際のコンパイルエラー除去のために追加したファイルです
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
    #pragma warning ( disable : 4786 )
    #pragma warning ( disable : 4996 )
    #pragma warning ( disable : 4018 ) 
#endif
/* warning C4018: '<' : signed と unsigned の数値を比較しようとしました。*/

#if defined(__VISUALC__)
#include "delfem/stub/visualc_stub.h"
#include <math.h>

//unsigned int abs(unsigned int x){ return (unsigned int)::abs((int)x); } 
unsigned int abs(unsigned int x){ return x ; } 

#endif /* defined(__VISUAL_C__) */

