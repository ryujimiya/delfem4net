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
@brief 剛体クラス(Com::CRigidBody3D)の実装
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_LINEAR_SYSTEM_RIGID_H)
#define DELFEM4NET_LINEAR_SYSTEM_RIGID_H

#include "DelFEM4Net/vector3d.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/mat_blkcrs.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"

#include "delfem/rigid/linearsystem_rigid.h"


////////////////////////////////////////////////////////////////

namespace DelFEM4NetRigid{
    ref class CRigidBody3D;
    ref class CConstraint;
}

namespace DelFEM4NetLs
{

public interface class ILinearSystem_RigidBody 
{
public:
    property Ls::CLinearSystem_RigidBody* LsSelf
    {
        Ls::CLinearSystem_RigidBody* get();
    }

    unsigned int GetSizeRigidBody();
    
    ////////////////
    void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, 
        DelFEM4NetCom::CVector3D^ vres, double d );
    void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, unsigned int size,
        array<double>^ eres, double d );
    void SubResidual(unsigned int ind, bool is_rb, 
        array<double>^ res);
    void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl,
                           DelFEM4NetCom::CMatrix3^ m, double d, bool isnt_trans);
    void AddMatrix_Vector(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                                  unsigned int indl, bool is_rb_l, unsigned int offsetl,
                                  DelFEM4NetCom::CVector3D^ vec, double d, bool is_colum);
    void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr, unsigned int sizer,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl, unsigned int sizel,
                           array<double>^ emat, double d);
    bool UpdateValueOfRigidSystem(
        IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst, 
        double dt, double newmark_gamma, double newmark_beta, 
        bool is_first);
};
////////////////////////////////////////////////////////////////


public ref class CLinearSystem_RigidBody_CRS2 : public ILinearSystem_RigidBody, public DelFEM4NetLsSol::ILinearSystem_Sol
{
public:
    CLinearSystem_RigidBody_CRS2();
private:
    CLinearSystem_RigidBody_CRS2(const CLinearSystem_RigidBody_CRS2% rhs);
public:
    CLinearSystem_RigidBody_CRS2(Ls::CLinearSystem_RigidBody_CRS2 *self);
    CLinearSystem_RigidBody_CRS2(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB,
        IList<DelFEM4NetRigid::CConstraint^>^ aConst);

    property Ls::CLinearSystem_RigidBody* LsSelf
    {
        virtual Ls::CLinearSystem_RigidBody* get();
    }
    property LsSol::ILinearSystem_Sol * SolSelf
    {
        virtual LsSol::ILinearSystem_Sol * get();
    }
    property Ls::CLinearSystem_RigidBody_CRS2 * Self
    {
        Ls::CLinearSystem_RigidBody_CRS2 * get();
    }

    ~CLinearSystem_RigidBody_CRS2();
    !CLinearSystem_RigidBody_CRS2();

    void Clear();
    void SetRigidSystem(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB,
        IList<DelFEM4NetRigid::CConstraint^>^ aConst);

    virtual unsigned int GetSizeRigidBody();

    ////////////////////////////////////////////////////////////////
    virtual void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, 
        DelFEM4NetCom::CVector3D^ vres, double d );
    virtual void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, unsigned int size,
        array<double>^ eres, double d );
    virtual void SubResidual(unsigned int ind, bool is_rb, array<double>^ res);
    virtual void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl,
                           DelFEM4NetCom::CMatrix3^ m, double d, bool isnt_trans );
    virtual void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr, unsigned int sizer,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl, unsigned int sizel,
                           array<double>^ emat, double d);
    virtual void AddMatrix_Vector(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                         unsigned int indl, bool is_rb_l, unsigned int offsetl,
                         DelFEM4NetCom::CVector3D^ vec, double d, bool is_column);
    virtual void InitializeMarge();
    virtual double FinalizeMarge();
    ////////////////////////////////////////////////////////////////
    virtual unsigned int GetTmpVectorArySize();
    virtual bool ReSizeTmpVecSolver(unsigned int isize);
    virtual double DOT(int iv1,int iv2); // {v2}*{v1}
    virtual bool COPY(int iv1,int iv2);  // {v2} := {v1}
    virtual bool SCAL(double d,int iv);  // {v1} := alpha * {v1}
    virtual bool AXPY(double d,int iv1,int iv2); // {v2} := alpha*{v1} + {v2}
    virtual bool MATVEC(double a,int iv1,double b,int iv2);  //  {v2} := alpha*[MATRIX]*{v1} + beta*{v2}

    // ポインタを取得するI/F
    //   Note: ivをintに変更した(-1:残差ベクトルとか-2:更新ベクトルを指定するようなので)
    DelFEM4NetMatVec::CVector_Blk_Ptr^ GetVectorPtr(int iv);
    // コピーを取得するI/F
    //   Note: ivをintに変更した(-1:残差ベクトルとか-2:更新ベクトルを指定するようなので)
    DelFEM4NetMatVec::CVector_Blk^ GetVector(int iv);
    DelFEM4NetMatVec::CMatDia_BlkCrs^ GetMatrix();
    virtual bool UpdateValueOfRigidSystem(
        IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst, 
        double dt, double newmark_gamma, double newmark_beta, 
        bool is_first);
    ////////////////////////////////////////////////////////////////

protected:
    Ls::CLinearSystem_RigidBody_CRS2 *self;

};

public ref class CPreconditioner_RigidBody_CRS2
{
public:
    CPreconditioner_RigidBody_CRS2();
private:
    CPreconditioner_RigidBody_CRS2(const CPreconditioner_RigidBody_CRS2% rhs);
public:
    CPreconditioner_RigidBody_CRS2(Ls::CPreconditioner_RigidBody_CRS2 *self);
    property Ls::CPreconditioner_RigidBody_CRS2 * Self
    {
        Ls::CPreconditioner_RigidBody_CRS2 * get();
    }
    ~CPreconditioner_RigidBody_CRS2();
    !CPreconditioner_RigidBody_CRS2();

    void SetLinearSystem(CLinearSystem_RigidBody_CRS2^ ls)
    {
        int ilev = 0;
        SetLinearSystem(ls, ilev);
    }
    void SetLinearSystem(CLinearSystem_RigidBody_CRS2^ ls, int ilev);
    void SetValue(CLinearSystem_RigidBody_CRS2^ ls);
    bool Solve(DelFEM4NetMatVec::CVector_Blk^% vec );

protected:
    Ls::CPreconditioner_RigidBody_CRS2 *self;
};

//! 前処理行列クラスの抽象クラス
public ref class CLinearSystemPreconditioner_RigidBody_CRS2 : public DelFEM4NetLsSol::ILinearSystemPreconditioner_Sol
{
public:
    CLinearSystemPreconditioner_RigidBody_CRS2(CLinearSystem_RigidBody_CRS2^ ls, 
        CPreconditioner_RigidBody_CRS2^ prec);
private:
    CLinearSystemPreconditioner_RigidBody_CRS2();
    CLinearSystemPreconditioner_RigidBody_CRS2(const CLinearSystemPreconditioner_RigidBody_CRS2% rhs);
    CLinearSystemPreconditioner_RigidBody_CRS2(Ls::CLinearSystemPreconditioner_RigidBody_CRS2 *self);
public:
    property LsSol::ILinearSystemPreconditioner_Sol * SolSelf
    {
        virtual LsSol::ILinearSystemPreconditioner_Sol * get();
    }
    property Ls::CLinearSystemPreconditioner_RigidBody_CRS2 * Self
    {
        virtual Ls::CLinearSystemPreconditioner_RigidBody_CRS2 * get();
    }
    ~CLinearSystemPreconditioner_RigidBody_CRS2();
    !CLinearSystemPreconditioner_RigidBody_CRS2();

public:
	//! ソルバに必要な作業ベクトルの数を得る
    virtual unsigned int GetTmpVectorArySize();
	//! ソルバに必要な作業ベクトルの数を設定
    virtual bool ReSizeTmpVecSolver(unsigned int size_new);
    virtual double DOT(int iv1, int iv2);
    virtual bool COPY(int iv1, int iv2);
    virtual bool SCAL(double alpha, int iv1);
    virtual bool AXPY(double alpha, int iv1, int iv2);
    virtual bool MATVEC(double alpha, int iv1, double beta, int iv2);

    virtual bool SolvePrecond(int iv);

protected:
    Ls::CLinearSystemPreconditioner_RigidBody_CRS2 *self;
};

}

#endif