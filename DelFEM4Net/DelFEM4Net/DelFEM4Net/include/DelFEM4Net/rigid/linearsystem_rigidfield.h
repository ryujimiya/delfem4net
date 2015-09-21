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

#if !defined(DELFEM4NET_LINEAR_SYSTEM_RIGID_FIELD_H)
#define DELFEM4NET_LINEAR_SYSTEM_RIGID_FIELD_H

#include "DelFEM4Net/vector3d.h"
#include "DelFEM4Net/linearsystem_interface_eqnsys.h"
#include "DelFEM4Net/ls/linearsystem_interface_solver.h"
#include "DelFEM4Net/ls/linearsystem.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/mat_blkcrs.h"
#include "DelFEM4Net/matvec/matdia_blkcrs.h"
#include "DelFEM4Net/rigid/linearsystem_rigid.h"

#include "delfem/rigid/linearsystem_rigidfield.h"


////////////////////////////////////////////////////////////////

namespace DelFEM4NetRigid{
    ref class CRigidBody3D;
    ref class CConstraint;
}

namespace DelFEM4NetLs
{
////////////////////////////////////////////////////////////////

public ref class CLinearSystem_RigidField2
    : public ILinearSystem_RigidBody,
      public DelFEM4NetFem::Eqn::ILinearSystem_Eqn, 
      public DelFEM4NetLsSol::ILinearSystem_Sol
{
public:
    CLinearSystem_RigidField2();
private:
    CLinearSystem_RigidField2(const CLinearSystem_RigidField2% rhs);
public:
    CLinearSystem_RigidField2(Ls::CLinearSystem_RigidField2 *self);

    property Ls::CLinearSystem_RigidBody * LsSelf
    {
        virtual Ls::CLinearSystem_RigidBody * get();
    }

    property Fem::Eqn::ILinearSystem_Eqn *EqnSelf
    {
        virtual Fem::Eqn::ILinearSystem_Eqn *get();
    }

    property LsSol::ILinearSystem_Sol * SolSelf
    {
        virtual LsSol::ILinearSystem_Sol * get();
    }

    property Ls::CLinearSystem_RigidField2 * Self
    {
        Ls::CLinearSystem_RigidField2 * get();
    }

    ~CLinearSystem_RigidField2();
    !CLinearSystem_RigidField2();

    void Clear();

    ////////////////////////////////////////////////////////////////
    // 場へのインターフェース
    int FindIndexArray_Seg(unsigned int id_field, DelFEM4NetFem::Field::ELSEG_TYPE type, DelFEM4NetFem::Field::CFieldWorld^ world );
    int GetIndexSegRigid();

    bool AddPattern_Field(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world);
    bool AddPattern_Field(unsigned int id_field, unsigned int id_field0, 
        DelFEM4NetFem::Field::CFieldWorld^ world );
    bool SetFixedBoundaryCondition_Field(unsigned int id_disp_fix0, DelFEM4NetFem::Field::CFieldWorld^ world);
    bool UpdateValueOfField_NewmarkBeta(double newmark_gamma, double newmark_beta, double dt,
        unsigned int id_disp  , DelFEM4NetFem::Field::CFieldWorld^ world, bool is_first);

    virtual DelFEM4NetMatVec::CVector_Blk^ GetResidual( 
        unsigned int id_field, 
        DelFEM4NetFem::Field::ELSEG_TYPE type, 
        DelFEM4NetFem::Field::CFieldWorld^ world );
    virtual DelFEM4NetMatVec::CMatDia_BlkCrs^ GetMatrix( 
        unsigned int id_field, 
        DelFEM4NetFem::Field::ELSEG_TYPE type, 
        DelFEM4NetFem::Field::CFieldWorld^ world );
    virtual DelFEM4NetMatVec::CMat_BlkCrs^ GetMatrix( 
        unsigned int id_field1, DelFEM4NetFem::Field::ELSEG_TYPE type1, 
        unsigned int id_field2, DelFEM4NetFem::Field::ELSEG_TYPE type2, 
        DelFEM4NetFem::Field::CFieldWorld^ world );

    ////////////////////////////////////////////////////////////////
    // 剛体へのインターフェース
    void SetRigidSystem(IList<DelFEM4NetRigid::CRigidBody3D^>^ aRB, 
        IList<DelFEM4NetRigid::CConstraint^>^ aConst);
    virtual unsigned int GetSizeRigidBody();
    unsigned int GetSizeConstraint();
    virtual bool UpdateValueOfRigidSystem(
        IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst, 
        double dt, double newmark_gamma, double newmark_beta, bool is_first);

    virtual void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, 
        DelFEM4NetCom::CVector3D^ vres, double d);
    virtual void AddResidual(unsigned int ind, bool is_rb, unsigned int offset, unsigned int size,
        array<double>^ eres, double d);
    virtual void SubResidual(unsigned int ind, bool is_rb, array<double>^ res);
    virtual void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl,
                           DelFEM4NetCom::CMatrix3^ m, double d, bool isnt_trans);
    virtual void AddMatrix_Vector(unsigned int indr, bool is_rb_r, unsigned int offsetr,
                                  unsigned int indl, bool is_rb_l, unsigned int offsetl,
                                  DelFEM4NetCom::CVector3D^ vec, double d, bool is_column);
    virtual void AddMatrix(unsigned int indr, bool is_rb_r, unsigned int offsetr, unsigned int sizer,
                           unsigned int indl, bool is_rb_l, unsigned int offsetl, unsigned int sizel,
                           array<double>^ emat, double d);

    ////////////////////////////////
    // 剛体弾性体連成インターフェース
    bool AddPattern_RigidField(
        unsigned int id_field, unsigned int id_field0, DelFEM4NetFem::Field::CFieldWorld^ world, 
        unsigned int irb, IList<DelFEM4NetRigid::CRigidBody3D^>^% aRB, IList<DelFEM4NetRigid::CConstraint^>^% aConst);

    ////////////////////////////////

    virtual DelFEM4NetMatVec::CMatDia_BlkCrs_Ptr^ GetMatrixPtr( unsigned int ils );
    virtual DelFEM4NetMatVec::CMat_BlkCrs_Ptr^ GetMatrixPtr( unsigned int ils, unsigned int jls );
    virtual DelFEM4NetMatVec::CVector_Blk_Ptr^ GetResidualPtr( unsigned int ils );
    unsigned int GetNLinSysSeg();
    void InitializeMarge();
    double FinalizeMarge();

    ////////////////////////////////////////////////////////////////
    // Solverへのインターフェース
    virtual unsigned int GetTmpVectorArySize();
    virtual bool ReSizeTmpVecSolver(unsigned int isize);
    virtual double DOT(int iv0,int iv1);
    virtual bool COPY(int iv0,int iv1);
    virtual bool SCAL(double d,int iv);
    virtual bool AXPY(double d,int iv0,int iv1);
    virtual bool MATVEC(double a,int iv0,double b,int iv1);

    ////////////////////////////////////////////////////////////////
/*
private:
    ref class CLinSysSegRF{
    public:
        CLinSysSegRF(const CLinSysSegRF% rhs)
        {
            this->self = new Ls::CLinearSystem_RigidField2::CLinSysSegRF(*(rhs.self));
        }
        CLinSysSegRF(unsigned int id_f, DelFEM4NetFem::Field::ELSEG_TYPE iconf)
        {
            FEM4NetFem::Field::ELSEG_TYPE iconf_ = static_cast<FEM4NetFem::Field::ELSEG_TYPE>(iconf);
            this->self = new Ls::CLinearSystem_RigidField2::CLinSysSegRF(id_f, iconf_);
        }
        CLinSysSegRF(unsigned int nRB, unsigned int nConst)
        {
            this->self = new Ls::CLinearSystem_RigidField2::CLinSysSegRF(nRB, nConst);
        }
    private:
        CLinSysSegRF() { assert(false); this->self = NULL; }
    public:
        property bool is_rigid
        {
            bool get() { return this->self->is_rigid; }
            void set(bool value) { this->self->is_rigid = value; }
        }
        property unsigned int id_field // parent_fieldでなければならない
        {
            unsigned int get() { return this->self->id_field; }
            void set(unsigned int value) { this->self->id_field = value; }
        }
        property DelFEM4NetFem::Field::ELSEG_TYPE node_config
        {
            DelFEM4NetFem::Field::ELSEG_TYPE get() { return static_cast<DelFEM4NetFem::Field::ELSEG_TYPE>(this->self->node_config); }
            void set(DelFEM4NetFem::Field::ELSEG_TYPE value) { this->self->node_config = static_cast<Fem::Field::ELSEG_TYPE>(value); }
        }
        ////////////////
        // nRB+nConstが行や列のブロックの数
        property unsigned int nRB
        {
            unsigned int get() { return this->self->nRB; }
            void set(unsigned int value) { this->self->nRB = value; }
        }
        property unsigned int nConst
        {
            unsigned int get() { return this->self->nConst; }
            void set(unsigned int value) { this->self->nConst = value; }
        }
    };

    //typedef DelFEM4NetCom::NativeInstanceVectorIndexer<Ls::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF^> CLinSysSegRFVectorIndexer;
    ref class CLinSysSegRFVectorIndexer : public DelFEM4NetCom::NativeInstanceVectorIndexer<Ls::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF^>
    {
    public:
        CLinSysSegRFVectorIndexer(std::vector<CLinSysSegRF>& vec_)
          : DelFEM4NetCom::NativeInstanceVectorIndexer<Ls::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF^>(vec_)
        {
        }
    
        CLinSysSegRFVectorIndexer(const CLinSysSegRFVectorIndexer% rhs)
          : DelFEM4NetCom::NativeInstanceVectorIndexer<Ls::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF, DelFEM4NetLs::CLinearSystem_RigidField2::CLinSysSegRF^>(rhs)
        {
        }
    
        ~CLinSysSegRFVectorIndexer()
        {
            this->!CLinSysSegRFVectorIndexer();
        }
    
        !CLinSysSegRFVectorIndexer()
        {
        }
    };
*/
public:
    /*
    //std::vector<CLinSysSegRF> m_aSegRF;
    property CLinSysSegRFVectorIndexer^ aIndRB
    {
        CLinSysSegRFVectorIndexer^ get();
    }
    */
    //LsSol::CLinearSystem m_ls;
    DelFEM4NetLsSol::CLinearSystemAccesser^ GetLs();

protected:
    Ls::CLinearSystem_RigidField2 *self;

};


public ref class CPreconditioner_RigidField2
{
public:
    CPreconditioner_RigidField2();
private:
    CPreconditioner_RigidField2(const CPreconditioner_RigidField2% rhs);
public:
    CPreconditioner_RigidField2(Ls::CPreconditioner_RigidField2 *self);

    property Ls::CPreconditioner_RigidField2 * Self
    {
        Ls::CPreconditioner_RigidField2 * get();
    }

    ~CPreconditioner_RigidField2();
    !CPreconditioner_RigidField2();


    ////////////////
    void SetFillInLevel(int lev)
    {
        int ilss0 = -1;
        SetFillInLevel(lev, ilss0);
    }
	void SetFillInLevel(int lev, int ilss0);
    void Clear();
    void SetLinearSystem(CLinearSystem_RigidField2^ ls);
    void SetValue(CLinearSystem_RigidField2^ ls);
    void SolvePrecond(CLinearSystem_RigidField2^% ls, int iv);

protected:
    Ls::CPreconditioner_RigidField2 *self;

};


//! 前処理行列クラスの抽象クラス
public ref class CLinearSystemPreconditioner_RigidField2: public DelFEM4NetLsSol::ILinearSystemPreconditioner_Sol
{
public:
    //CLinearSystemPreconditioner_RigidField2();
private:
    CLinearSystemPreconditioner_RigidField2(const CLinearSystemPreconditioner_RigidField2% rhs);
public:
    CLinearSystemPreconditioner_RigidField2(CLinearSystem_RigidField2^ ls, CPreconditioner_RigidField2^ prec);
private:
    CLinearSystemPreconditioner_RigidField2(Ls::CLinearSystemPreconditioner_RigidField2 *self);
public:
    virtual ~CLinearSystemPreconditioner_RigidField2();
    !CLinearSystemPreconditioner_RigidField2();
    
    property LsSol::ILinearSystemPreconditioner_Sol * SolSelf
    {
       virtual LsSol::ILinearSystemPreconditioner_Sol * get();
    }

    property Ls::CLinearSystemPreconditioner_RigidField2 * Self
    {
       Ls::CLinearSystemPreconditioner_RigidField2 * get();
    }

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
    Ls::CLinearSystemPreconditioner_RigidField2 *self;
};

} // end namespace LS

#endif