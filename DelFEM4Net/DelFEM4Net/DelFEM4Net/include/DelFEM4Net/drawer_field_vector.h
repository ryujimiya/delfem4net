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
@brief interface of vector drawer vector class (Fem::Field::View::CDrawerVector)
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_DRAWER_FIELD_VECTOR_H)
#define DELFEM4NET_DRAWER_FIELD_VECTOR_H

//#include <memory>

#include "DelFEM4Net/drawer_field.h"

#include "delfem/drawer_field_vector.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetFem
{
namespace Field
{
namespace View
{
    
//! visualization class using vector
public ref class CDrawerVector : public CDrawerField
{
public:
    CDrawerVector();
    CDrawerVector(const CDrawerVector% rhs);
    CDrawerVector(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    CDrawerVector(Fem::Field::View::CDrawerVector *self);
    virtual ~CDrawerVector();
    !CDrawerVector();
    property Com::View::CDrawer * DrawerSelf
    {
        virtual Com::View::CDrawer * get() override;
    }
    property Fem::Field::View::CDrawerVector * Self
    {
        Fem::Field::View::CDrawerVector * get();
    }

    virtual DelFEM4NetCom::CBoundingBox3D^ GetBoundingBox(array<double>^ rot) override;
    virtual void DrawSelection(unsigned int idraw) override;
    virtual void AddSelected(array<int>^ selec_flag) override;
    virtual void ClearSelected() override;
    virtual void Draw() override;
    virtual bool Update(DelFEM4NetFem::Field::CFieldWorld^ world) override;

protected:
    Fem::Field::View::CDrawerVector *self;

};

}
}
}

#endif
