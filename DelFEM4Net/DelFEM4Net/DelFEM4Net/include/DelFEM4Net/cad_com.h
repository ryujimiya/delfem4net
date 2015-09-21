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
@brief the basical definition refered from whole system including mesh and fem
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CAD_COM_H)
#define DELFEM4NET_CAD_COM_H

#include "delfem/cad_com.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCad
{

/*!
@ingroup CAD
@brief the type of component 
*/
public enum class CAD_ELEM_TYPE
{
  NOT_SET,  //!< not setted(for the error handling)
  VERTEX,   //!< vertex
  EDGE,     //!< edge
  LOOP,     //!< loop
  SOLID,    //!< solid
};
  
  

//! iterator go around loop
public interface class IItrLoop
{
public:
    void Begin();    //!< back to initial point of current use-loop
    void operator++(); //!< move to next edge
    void operator++(int n);    //!< dummy operator (for ++)
    void Increment(); // C#から演算子++が使用できなかったので追加
   //! return current edge id and whether if this edge is same dirrection as loop
    bool GetIdEdge([Out] unsigned int% id_e, [Out] bool% is_same_dir) ;    
    bool ShiftChildLoop();    //!< move to next use-loop in this loop
    bool IsEndChild();    //!< return true if iterator go around
    unsigned int GetIdVertex();    //!< return current vertex id
    unsigned int GetIdVertex_Ahead();    //!< return next vertex
    unsigned int GetIdVertex_Behind();    //!< return previous vertex
    bool IsEnd();    //!< return true if iterator go around
};
  
public interface class IItrVertex
{
public:
    void operator++() ; //!< go around (cc-wise) loop around vertex 
    void operator++(int n) ; //!< dummy operator (for ++)        
    void Increment(); // C#から演算子++が使用できなかったので追加
    //! cc-wise ahead  edge-id and its direction(true root of edge is this vertex)
    bool GetIdEdge_Ahead([Out] unsigned int% id_e, [Out] bool% is_same_dir);
    //! cc-wise behind edge-id and its direction(true root of edge is this vertex)
    bool GetIdEdge_Behind([Out] unsigned int% id_e, [Out] bool% is_same_dir);
    unsigned int GetIdLoop(); //!< get loop-id        
    bool IsEnd(); //!< return true if iterator go around
};

}

#endif
