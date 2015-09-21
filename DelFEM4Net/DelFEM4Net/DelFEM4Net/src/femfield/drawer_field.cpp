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

////////////////////////////////////////////////////////////////
// DrawerField.cpp : implementation of field visualization class (DrawerField)
////////////////////////////////////////////////////////////////

#if defined(__VISUALC__)
#  pragma warning ( disable : 4786 )
#endif

#include <assert.h>
#include <iostream>
#include <vector>
#include <stdio.h>
#include <memory>

//#include "delfem/uglyfont.h"
#include "DelFEM4Net/drawer_field.h"
//#include "DelFEM4Net/elem_ary.h"
//#include "DelFEM4Net/field.h"
#include "DelFEM4Net/field_world.h"
//#include "DelFEM4Net/drawer.h"
//#include "DelFEM4Net/vector3d.h"

using namespace DelFEM4NetFem::Field::View;
using namespace DelFEM4NetFem::Field;


/////////////////////////////////////////////////////////////////////////////////////
// CIndexArrayElem
/////////////////////////////////////////////////////////////////////////////////////
CIndexArrayElem::CIndexArrayElem(unsigned int id_ea, unsigned int id_es, DelFEM4NetFem::Field::CFieldWorld^ world)
{
    this->self = new Fem::Field::View::CIndexArrayElem(id_ea, id_es, *(world->Self));
}


bool CIndexArrayElem::SetColor(unsigned int id_es_v, unsigned int id_ns_v, DelFEM4NetFem::Field::CFieldWorld^ world, CColorMap^ color_map)
{
    std::auto_ptr<Fem::Field::View::CColorMap> color_map_(new Fem::Field::View::CColorMap(*color_map->Self));

    bool ret = this->self->SetColor(id_es_v, id_ns_v, *(world->Self), color_map_);

    return ret;
}


/////////////////////////////////////////////////////////////////////////////////////
// CDrawerArrayField
/////////////////////////////////////////////////////////////////////////////////////
bool CDrawerArrayField::Update(DelFEM4NetFem::Field::CFieldWorld^ world)
{
    return this->self->Update(*(world->Self));
}
