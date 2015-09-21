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

#if defined(__VISUALC__)
#pragma warning(disable: 4786)
#pragma warning(disable: 4996)
#endif
#define for if(0);else for

#include <stdio.h>
#include <set>
#include <vector>
#include <queue>
#include <cassert>
#include <math.h>

#include <string>
#include <iostream>
#include <fstream>

//#include "DelFEM4Net/msh/meshkernel2d.h"
//#include "DelFEM4Net/msh/meshkernel3d.h"
#include "DelFEM4Net/mesher2d.h"
#include "DelFEM4Net/mesh3d.h"

