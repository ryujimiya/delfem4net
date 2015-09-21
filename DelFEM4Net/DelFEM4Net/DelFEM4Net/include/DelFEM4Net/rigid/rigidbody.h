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
@brief 剛体クラス(Rigid::CRigidBody3D)の実装
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_RIGID_BODY_H)
#define DELFEM4NET_RIGID_BODY_H

#include "DelFEM4Net/vector3d.h"
#include "delfem/rigid/rigidbody.h"


namespace DelFEM4NetLs{
    interface class ILinearSystem_RigidBody;
}

namespace DelFEM4NetRigid
{

public ref class CRigidBody3D
{
public:
    CRigidBody3D();
    CRigidBody3D(bool isCreateInstance);
    CRigidBody3D(const CRigidBody3D% rhs);
    CRigidBody3D(Rigid::CRigidBody3D* self);
    virtual ~CRigidBody3D();
    !CRigidBody3D();
    property Rigid::CRigidBody3D * Self
    {
        virtual Rigid::CRigidBody3D * get();
    }

    unsigned int GetDOF();
    void GetInvRotMatrix44(array<double>^% rot);
    void GetRotMatrix33(array<double>^% rot);
    DelFEM4NetCom::CMatrix3^ GetRotMatrix();
    void Clear();
    DelFEM4NetCom::CVector3D^ GetPositionFromInital(DelFEM4NetCom::CVector3D^ vec);
    void AddRotation(array<double>^ rot );
    void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta, 
        bool is_first_iter);
    void AddLinearSystem(
        DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int irb,
        double dt, double newmark_gamma, double newmark_beta,
        DelFEM4NetCom::CVector3D^ gravity, 
        bool is_first);

    // accessor
    void Set_Omega_Vector(double x, double y, double z);
    void Set_dOmega_Vector(double x, double y, double z);
    void Set_velo_cg_Vector(double x, double y, double z);
    void Set_disp_cg_Vector(double x, double y, double z);
    void Set_acc_cg_Vector(double x, double y, double z);
    void Set_ini_pos_cg_Vector(double x, double y, double z);
public:
    //double crv[3];
    property DelFEM4NetCom::DoubleArrayIndexer^ crv
    {
         DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ Omega
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    property DelFEM4NetCom::CVector3D^ dOmega
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    property DelFEM4NetCom::CVector3D^ velo_cg
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    property DelFEM4NetCom::CVector3D^ disp_cg
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    property DelFEM4NetCom::CVector3D^ acc_cg
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }

    // 初期設定
    property DelFEM4NetCom::CVector3D^ ini_pos_cg
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    property double mass
    {
        double get();
        void set(double value);
    }
    //double mineatia[3];
    property DelFEM4NetCom::DoubleArrayIndexer^ mineatia
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }

protected:
    Rigid::CRigidBody3D *self;
};

////////////////////////////////////////////////////////////////

public ref class CConstraint abstract
{
public:
    // nativeなインスタンス作成は派生クラスで行うため、コンストラクタ、デストラクタ、ファイナライザは何もしない
    CConstraint(){}
private:
    CConstraint(const CConstraint% rhs){ assert(false); }
public:
    virtual ~CConstraint(){ this->!CConstraint(); }
    !CConstraint(){}

    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() = 0;
    }
public:
    virtual unsigned int GetDOF() = 0;
    virtual void Clear() = 0;
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta ) = 0;
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, 
        bool is_initial) = 0;
    IList<unsigned int>^ GetAry_IndexRB()
    {
        if (this->ConstraintSelf == NULL)
        {
            return nullptr;
        }
        IList<unsigned int>^ list = DelFEM4NetCom::ClrStub::VectorToList(this->ConstraintSelf->aIndRB);
        return list;
    }
public:
    property DelFEM4NetCom::UIntVectorIndexer^ aIndRB
    {
        DelFEM4NetCom::UIntVectorIndexer^ get()
        {
            if (this->ConstraintSelf == NULL)
            {
                return nullptr;
            }
            // リファレンスを取得
            std::vector<unsigned int>& vec = this->ConstraintSelf->aIndRB;
            DelFEM4NetCom::UIntVectorIndexer^ indexer = gcnew DelFEM4NetCom::UIntVectorIndexer(vec);
            return indexer;
        }
    }

//  helper
public:
    // マネージドリストをstd::vectorに変換(クラスインスタンスのポインタ版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンスのポインタ 変換が行われる
    //   ネイティブインスタンスの生成は行わない
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2*>  T2 アンマネージドクラスインスタンス(インスタンス指定)
    template<typename T1, typename T2>
    static std::vector<T2*> ListToInstancePtrVector_NoCreate(IList<T1^>^ list)
    {
        std::vector<T2*> vec;
        
        ListToInstancePtrVector_NoCreate(list, vec);

        return vec;
    }

    // マネージドリストをstd::vectorに変換(クラスインスタンスのポインタ版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンスのポインタ 変換が行われる
    //   パラメータで出力先指定 (nativeクラスのメンバ変数のリファレンスを渡して書き換えるときに使用)
    //   ネイティブインスタンスの生成は行わない
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2*>  T2 アンマネージドクラスインスタンス(インスタンス指定)
    template<typename T1, typename T2>
    static void ListToInstancePtrVector_NoCreate(IList<T1^>^ list, std::vector<T2*>& vec)
    {
        vec.clear();
        if (list->Count > 0)
        {
            for each (T1^ e in list)
            {
                // マネージドインスタンスで管理しているnativeポインタをそのまま渡す。
                T2* e_instance_ptr_ = e->ConstraintSelf;
                vec.push_back(e_instance_ptr_);
            }
        }
    }
};

public ref class CFix_Spherical : public CConstraint
{
private:
    CFix_Spherical();
public:
    CFix_Spherical(const CFix_Spherical% rhs);
    CFix_Spherical(unsigned int irb);

    CFix_Spherical(Rigid::CFix_Spherical *self);
    virtual ~CFix_Spherical();
    !CFix_Spherical();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CFix_Spherical * Self
    {
        Rigid::CFix_Spherical * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosFix(double x, double y, double z);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
public:
    //double lambda[3];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_fix
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }

protected:
    Rigid::CFix_Spherical *self;
};

public ref class CFix_Hinge : public CConstraint
{
private:
    CFix_Hinge();
public:
    CFix_Hinge(const CFix_Hinge% rhs);
    CFix_Hinge(unsigned int irb);

    CFix_Hinge(Rigid::CFix_Hinge *self);
    virtual ~CFix_Hinge();
    !CFix_Hinge();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CFix_Hinge * Self
    {
        Rigid::CFix_Hinge * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosFix(double x, double y, double z);
    void SetAxis(double ax, double ay, double az);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
public:
    //double lambda[5];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_fix
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    //Com::CVector3D loc_coord[2];
    property DelFEM4NetCom::CVector3DArrayIndexer^ loc_coord
    {
        DelFEM4NetCom::CVector3DArrayIndexer^ get();
    }

protected:
    Rigid::CFix_Hinge *self;
};

public ref class CFix_HingeRange : public CConstraint
{
private:
    CFix_HingeRange();
public:
    CFix_HingeRange(const CFix_HingeRange% rhs);
    CFix_HingeRange(unsigned int irb);

    CFix_HingeRange(Rigid::CFix_HingeRange *self);
    virtual ~CFix_HingeRange();
    !CFix_HingeRange();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CFix_HingeRange * Self
    {
        Rigid::CFix_HingeRange * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosFix(double x, double y, double z);
    void SetAxis(double ax, double ay, double az);
    void SetRange(double min_t, double max_t);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
public:
    //double lambda[6];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_fix
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    //Com::CVector3D loc_coord[2];
    property DelFEM4NetCom::CVector3DArrayIndexer^ loc_coord
    {
        DelFEM4NetCom::CVector3DArrayIndexer^ get();
    }
    property double min_t
    {
        double get();
        void set(double value);
    }
    property double max_t
    {
        double get();
        void set(double value);
    }

protected:
    Rigid::CFix_HingeRange *self;
};

public ref class CJoint_Spherical : public CConstraint
{
private:
    CJoint_Spherical();
public:
    CJoint_Spherical(const CJoint_Spherical% rhs);
    CJoint_Spherical(unsigned int irb0, unsigned int irb1);

    CJoint_Spherical(Rigid::CJoint_Spherical *self);
    virtual ~CJoint_Spherical();
    !CJoint_Spherical();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CJoint_Spherical * Self
    {
        Rigid::CJoint_Spherical * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosJoint(double x, double y, double z);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
    //virtual void Draw(IList<CRigidBody3D^>^ aRB);
public:
    //double lambda[3];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_joint
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }

protected:
    Rigid::CJoint_Spherical *self;
};

public ref class CJoint_Hinge : public CConstraint
{
private:
    CJoint_Hinge();
public:
    CJoint_Hinge(const CJoint_Hinge% rhs);
    CJoint_Hinge(unsigned int irb0, unsigned int irb1);

    CJoint_Hinge(Rigid::CJoint_Hinge *self);
    virtual ~CJoint_Hinge();
    !CJoint_Hinge();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CJoint_Hinge * Self
    {
        Rigid::CJoint_Hinge * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosJoint(double x, double y, double z);
    void SetAxis(double ax, double ay, double az);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
    //virtual void Draw(IList<CRigidBody3D^>^ aRB);
public:
    //double lambda[5];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_joint
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    //Com::CVector3D loc_coord[2];
    property DelFEM4NetCom::CVector3DArrayIndexer^ loc_coord
    {
        DelFEM4NetCom::CVector3DArrayIndexer^ get();
    }

protected:
    Rigid::CJoint_Hinge *self;
};

public ref class CJoint_HingeRange : public CConstraint
{
private:
    CJoint_HingeRange();
public:
    CJoint_HingeRange(const CJoint_HingeRange% rhs);
    CJoint_HingeRange(unsigned int irb0, unsigned int irb1);

    CJoint_HingeRange(Rigid::CJoint_HingeRange *self);
    virtual ~CJoint_HingeRange();
    !CJoint_HingeRange();
    property Rigid::CConstraint * ConstraintSelf
    {
        virtual Rigid::CConstraint * get() override;
    }
    property Rigid::CJoint_HingeRange * Self
    {
        Rigid::CJoint_HingeRange * get();
    }

    virtual unsigned int GetDOF() override;
    virtual void Clear() override;
    void SetIniPosJoint(double x, double y, double z);
    void SetAxis(double ax, double ay, double az);
    void SetRange(double min_t, double max_t);
    virtual void UpdateSolution(array<double>^ upd,
        double dt, double newmark_gamma, double newmark_beta) override;
    void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB)
    {
        AddLinearSystem(ls, icst,
            dt, newmark_gamma, newmark_beta,
            aRB, false);
    }
    virtual void AddLinearSystem(DelFEM4NetLs::ILinearSystem_RigidBody^% ls, unsigned int icst,
        double dt, double newmark_gamma, double newmark_beta,
        IList<CRigidBody3D^>^ aRB, bool is_initial) override;
public:
    //double lambda[6];
    property DelFEM4NetCom::DoubleArrayIndexer^ lambda
    {
        DelFEM4NetCom::DoubleArrayIndexer^ get();
    }
    property DelFEM4NetCom::CVector3D^ ini_pos_joint
    {
        DelFEM4NetCom::CVector3D^ get();
        void set(DelFEM4NetCom::CVector3D^ value);
    }
    //Com::CVector3D loc_coord[2];
    property DelFEM4NetCom::CVector3DArrayIndexer^ loc_coord
    {
        DelFEM4NetCom::CVector3DArrayIndexer^ get();
    }
    property double min_t
    {
        double get();
        void set(double value);
    }
    property double max_t
    {
        double get();
        void set(double value);
    }
protected:
    Rigid::CJoint_HingeRange *self;
};

}

#endif