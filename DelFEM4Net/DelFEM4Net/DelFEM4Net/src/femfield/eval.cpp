﻿/*
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

/*!
@brief 数式評価をするクラス(Fem::Field::CEval)の実装
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )   // C4786なんて表示すんな( ﾟДﾟ)ｺﾞﾙｧ
#endif
#define for if(0);else for

#include <cassert>
#include <errno.h>    /* Need for Using "ERANGE" */
#include <math.h>    /* Need for Using "HUGE_VAL" */
#include <iostream>
#include <cstdlib> //(strtod)

#include "DelFEM4Net/eval.h"

using namespace DelFEM4NetFem::Field;

