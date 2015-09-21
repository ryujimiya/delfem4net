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
@brief This file implements the class CCamera. This class have a data represents model-view transformation.
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/
// DESCRIPTION
// This file implements the class CCamera. 
// This class have a data represents model-view transformation.
// This class doesn't have actual transformation function so as to keep indipendency with OpenGL
// If you want to apply transformation use functions in drawer_gl_utility.h

#if !defined(DELFEM4NET_CAMERA_H)
#define DELFEM4NET_CAMERA_H

#include <assert.h>

#include "DelFEM4Net/vector3d.h"
//////#include "DelFEM4Net/quaternion.h"

#include "delfem/camera.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{
namespace View
{

//! modes of rotation transformation
public enum class ROTATION_MODE{    
    ROT_2D,        //!< 2dim rotation
    ROT_2DH,    //!< z axis is allways pararell to the upright direction of screan
    ROT_3D        //!< track ball rotation
};

public ref class CCamera
{
public:
    CCamera()
    {
        this->self = new Com::View::CCamera();
    }
    
    CCamera(const CCamera% rhs)
    {
        Com::View::CCamera rhs_instance_ = *(rhs.self);
        this->self = new Com::View::CCamera(rhs_instance_);
    }
    
    CCamera(ROTATION_MODE rotation_mode)
    {
        this->self = new Com::View::CCamera();
    }
    
    CCamera(Com::View::CCamera *self)
    {
        this->self = self;
    }
    
    ~CCamera()
    {
        this->!CCamera();
    }

    !CCamera()
    {
        delete this->self;
    }
    
    property Com::View::CCamera * Self
    {
        Com::View::CCamera * get() { return this->self; }
    }
    
    void GetObjectCenter([Runtime::InteropServices::Out] double% x, [Runtime::InteropServices::Out] double% y, [Runtime::InteropServices::Out] double% z) 
    {
        double x_ = x;
        double y_ = y;
        double z_ = z;
        
        this->self->GetObjectCenter(x_, y_, z_);
        
        x = x_;
        y = y_;
        z = z_;
    }
    
    CVector3D^ GetObjectCenter()
    {
        Com::CVector3D *ret = new Com::CVector3D();
        
        *ret = this->self->GetObjectCenter();
        
        CVector3D^ retManaged = gcnew CVector3D(ret);
        
        return retManaged;
    }
    
    // Set the center of object witch is used for Fit
    void SetObjectCenter(double x, double y, double z)
    {
        this->self->SetObjectCenter(x, y, z);
    }

    // Set the size of object in xyz direction
    void SetObjectSize(double w, double h, double d)
    {
        this->self->SetObjectSize(w, h, d);
    }

    void SetObjectBoundingBox(DelFEM4NetCom::CBoundingBox3D^ bb)
    {
        Com::CBoundingBox3D *bb_ = bb->Self;
        this->self->SetObjectBoundingBox(*bb_);
    }

    void Fit()
    {
        this->self->Fit();
    }

    void Fit(DelFEM4NetCom::CBoundingBox3D^ bb )
    {
        Com::CBoundingBox3D *bb_ = bb->Self;
        this->self->Fit(*bb_);
    }
    
    //! get scale :  
    double GetScale()
    {
        return this->self->GetScale();
    }
    
    //! set scale
    void SetScale(double scale)
    {
        this->self->SetScale(scale);
    }

    ////////////////////////
    // Perspective
    
    bool IsPers()
    {
        return this->self->IsPers();
    }
    
    void SetIsPers(bool is_pers)
    {
        this->self->SetIsPers(is_pers);
    }    

    double GetFovY()  //!< Get the view angle
    {
        return this->self->GetFovY();
    }
    
    //! set the view angle
    void SetFovY(double fov_y)
    {
        this->self->SetFovY(fov_y);
    }
    void GetPerspective([Runtime::InteropServices::Out] double% fov_y, [Runtime::InteropServices::Out] double% aspect, [Runtime::InteropServices::Out] double% clip_near, [Runtime::InteropServices::Out] double% clip_far)
    {
        double fov_y_ = fov_y;
        double aspect_ = aspect;
        double clip_near_ = clip_near;
        double clip_far_ = clip_far;
        
        this->self->GetPerspective(fov_y_, aspect_, clip_near_, clip_far_);
        
        fov_y = fov_y_;
        aspect = aspect_;
        clip_near = clip_near_;
        clip_far = clip_far_;
    }
    
    void GetOrtho([Runtime::InteropServices::Out] double% hw, [Runtime::InteropServices::Out] double% hh, [Runtime::InteropServices::Out] double% hd)
    {
        double hw_ = hw;
        double hh_ = hh;
        double hd_ = hd;
        
        this->self->GetOrtho(hw_, hh_, hd_);
        
        hw = hw_;
        hh = hh_;
        hd = hd_;
    }

    ////////////////
    // 
    double GetHalfViewHeight()
    {
        return this->self->GetHalfViewHeight();
    }

    // get the 3D location where the center is located
    void GetCenterPosition([Runtime::InteropServices::Out] double% x, [Runtime::InteropServices::Out] double% y,[Runtime::InteropServices::Out] double% z)
    {
        double x_ = x;
        double y_ = y;
        double z_ = z;
        
        this->self->GetCenterPosition(x_, y_, z_);
        
        x = x_;
        y = y_;
        z = z_;
    }

    CVector3D^ GetCenterPosition()
    {
        Com::CVector3D *ret = new Com::CVector3D();
        
        *ret = this->self->GetCenterPosition();
        
        CVector3D^ retManaged = gcnew CVector3D();
        
        return retManaged;
     }

    void MousePan(double mov_begin_x, double mov_begin_y, double mov_end_x, double mov_end_y )
    {
        this->self->MousePan(mov_begin_x, mov_begin_y, mov_end_x, mov_end_y);
    }

    void SetWindowAspect(double asp) // set window aspect ratio
    {
        this->self->SetWindowAspect(asp);
    }

    double GetWindowAspect() // get window aspect ratio
    {
        return this->self->GetWindowAspect();
    }

    ////////////////
    // rotation
    void RotMatrix33([Runtime::InteropServices::Out] array<double>^% m)  //[OUT] m
    {
        double m_[9];

        this->self->RotMatrix33(m_);

        m = gcnew array<double>(9);
        for (int i = 0; i < 9; i++)
        {
            m[i] = m_[i];
        }
    }

    CMatrix3^ RotMatrix33()
    {
        Com::CMatrix3 *ret = new Com::CMatrix3();
        
        *ret = this->self->RotMatrix33();
        
        CMatrix3^ retManaged = gcnew CMatrix3(ret);
        
        return retManaged;
    }

    void RotMatrix44Trans([Runtime::InteropServices::Out] array<double>^% m) //[OUT] m
    {
        double m_[16];

        this->self->RotMatrix44Trans(m_);

        m = gcnew array<double>(16);
        for (int i = 0; i < 16; i++)
        {
            m[i] = m_[i];
        }
    }
    
    void SetRotationMode(ROTATION_MODE rot_mode)
    {
        Com::View::ROTATION_MODE rot_mode_ = static_cast<Com::View::ROTATION_MODE>(rot_mode);
        this->self->SetRotationMode(rot_mode_);
    }
    
    ROTATION_MODE GetRotationMode()
    {
        Com::View::ROTATION_MODE rot_mode_ = this->self->GetRotationMode();
        
        return static_cast<DelFEM4NetCom::View::ROTATION_MODE>(rot_mode_);
    }
    
    void MouseRotation(double mov_begin_x, double mov_begin_y, double mov_end_x, double mov_end_y )
    {
        this->self->MouseRotation(mov_begin_x, mov_begin_y, mov_end_x, mov_end_y);
    }
    
    DelFEM4NetCom::CVector3D^ ProjectionOnPlane(double pos_x,   double pos_y)
    {
        return ProjectionOnPlane(pos_x, pos_y, 0, 0, 0, 0, 0, 1);
    }
    DelFEM4NetCom::CVector3D^ ProjectionOnPlane(
        double pos_x,   double pos_y,
        double plane_x, double plane_y, double plane_z)
    {
        return ProjectionOnPlane(pos_x, pos_y, plane_x, plane_y, plane_z, 0, 0, 1);
    }

    DelFEM4NetCom::CVector3D^ ProjectionOnPlane(
        double pos_x,   double pos_y,
        double plane_x, double plane_y, double plane_z,
        double norm_x,  double norm_y,  double norm_z ) 
    {
        Com::CVector3D *ret = new Com::CVector3D();
        
        *ret = this->self->ProjectionOnPlane(
            pos_x, pos_y,
            plane_x, plane_y, plane_z,
            norm_x, norm_y, norm_z);
        
        CVector3D^ retManaged = gcnew CVector3D(ret);
        
        return retManaged;
    }
    
    void SetRotation2DH(double theta, double phi)
    {
        this->self->SetRotation2DH(theta, phi);
    }
  
    void SetWindowCenter(double winx, double winy)
    {
        this->self->SetWindowCenter(winx, winy);
    }

protected:
    Com::View::CCamera *self;
};

}    // end namespace View
}    // end namespace DelFEM4NetCom

#endif
