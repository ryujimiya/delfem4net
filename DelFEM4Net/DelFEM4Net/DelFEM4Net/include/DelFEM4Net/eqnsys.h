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
@brief 抽象方程式オブジェクトのインターフェース
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_EQN_SYS_H)
#define DELFEM4NET_EQN_SYS_H

#include <cassert>

#include <vector>
#include <map>

#if defined(__VISUALC__)
#pragma warning( disable : 4786 )
#endif

#include <vector>

#include "DelFEM4Net/stub/clr_stub.h"  // DelFEM4NetCom::Pair

#include "delfem/eqnsys.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

//namespace DelFEM4NetLsSol
//{
//    ref class CPreconditioner;    // 前処理行列
//}

namespace DelFEM4NetFem
{

// 派生クラスで参照
namespace Ls
{
    ref class CLinearSystem_Field;                   // 連立一次方程式
    ref class CLinearSystem_Save;                    // 連立一次方程式(剛性行列保存)
    ref class CLinearSystem_SaveDiaM_NewmarkBeta;    // 連立一次方程式(NewmarkBeta法で剛性行列保存)
}

namespace Field
{
    ref class CField;
    ref class CFieldWorld;
}

namespace Eqn
{

/*! 
@brief 抽象連成方程式クラス
@ingroup FemEqnSystem
*/
public ref class CEqnSystem abstract
{
public:
    // nativeなインスタンス作成は派生クラスで行うため、コンストラクタ、デストラクタ、ファイナライザは何もしない
    CEqnSystem(){}
    // NOTE!!!!!!!!
    // コピーコンストラクタおよびself設定用のコンストラクタは定義しないこと shallowコピーの問題あり
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更
    CEqnSystem(const CEqnSystem% rhs) { assert(false); }
public:
    virtual ~CEqnSystem(){ this->!CEqnSystem(); }
    !CEqnSystem(){}

    property Fem::Eqn::CEqnSystem * EqnSysSelf
    {
        virtual Fem::Eqn::CEqnSystem * get() = 0;
    }

    virtual void Clear()
    {
        this->EqnSysSelf->Clear();
    }

    //! 方程式を解く
    virtual bool Solve(DelFEM4NetFem::Field::CFieldWorld^ world) = 0;

/*
    IList< DelFEM4NetCom::Pair<unsigned int, double>^ >^ GetAry_ItrNormRes() 
    {
        const std::vector< std::pair<unsigned int, double> >& vec = this->EqnSysSelf->GetAry_ItrNormRes();
        
        IList< DelFEM4NetCom::Pair<unsigned int, double>^ >^ list = gcnew List< DelFEM4NetCom::Pair<unsigned int, double>^ >();
        if (vec.size() > 0)
        {
            for (std::vector< std::pair<unsigned int, double> >::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
            {
                std::pair<unsigned int, double> pair_ = *itr;
                DelFEM4NetCom::Pair<unsigned int, double>^ pair = gcnew DelFEM4NetCom::Pair<unsigned int, double>();
                pair->First = pair_.first;
                pair->Second = pair_.second;
                
                list->Add(pair);
            }
        }
        
        return list;
    }
*/
    DelFEM4NetCom::ConstUIntDoublePairVectorIndexer^ GetAry_ItrNormRes()
    {
        const std::vector< std::pair<unsigned int, double> >& vec = this->EqnSysSelf->GetAry_ItrNormRes();
        return gcnew DelFEM4NetCom::ConstUIntDoublePairVectorIndexer(vec);
    }

    ////////////////////////////////
    // 固定境界条件を追加する

    //! @{
    //! 場(id_field)の要素配列を全て固定境界にする
    virtual bool AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        int idof = -1 ;
        return AddFixField(id_field, world, idof);
    }
    virtual bool AddFixField(unsigned int id_field, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) = 0;
    
    //! 場(m_id_val)の要素配列(id_ea)を固定境界にする
    virtual unsigned int AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        int idof = -1 ;
        return AddFixElemAry(id_ea, world, idof);
    }
    virtual unsigned int AddFixElemAry(unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) = 0;
    
    //! 場(m_id_val)の要素配列の配列(aIdEA)を固定境界にする
    virtual unsigned int AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world)
    {
        int idof = -1 ;
        return AddFixElemAry(aIdEA, world, idof);
    }
    virtual unsigned int AddFixElemAry(IList<unsigned int>^ aIdEA, DelFEM4NetFem::Field::CFieldWorld^ world, int idof) = 0;
    
    //! 固定境界条件を削除する
    virtual bool ClearFixElemAry( unsigned int id_ea, DelFEM4NetFem::Field::CFieldWorld^ world ) = 0;
    
    //! 全ての固定境界条件を削除する
    virtual void ClearFixElemAry() = 0;
    //! @}

    /*! 
    @brief 時間積分についてのパラメータを設定する
    @param [in] dt 時間刻み
    @param [in] gamma Newmark法のgamma(省略すれば0.6にセット)
    @param [in] beta Newmark法のbeta(gammaがあれば省略可)
    */
    void SetTimeIntegrationParameter(double dt)
    {
        double gamma = 0.6;
        double beta = -1.0;
        SetTimeIntegrationParameter(dt, gamma, beta);
    }
    void SetTimeIntegrationParameter(double dt, double gamma)
    {
        double beta = -1.0;
        SetTimeIntegrationParameter(dt, gamma, beta);
    }
    void SetTimeIntegrationParameter(double dt, double gamma, double beta)
    {
        this->EqnSysSelf->SetTimeIntegrationParameter(dt, gamma, beta);
    }

    ////////////////////////////////
    // 連立一次方程式クラスや前処理クラスの再評価，再構築指定関数

    //! @{
    virtual void ClearValueLinearSystemPreconditioner()
    {
        this->EqnSysSelf->ClearValueLinearSystemPreconditioner();
    }
    
    virtual void ClearValueLinearSystem()    // フラグを立てるとSolveの時に値が再評価される
    {
        this->EqnSysSelf->ClearValueLinearSystem();
    }
    
    virtual void ClearValuePreconditioner()    // フラグを立てるとSolveの時に値が再評価される
    {
        this->EqnSysSelf->ClearValuePreconditioner();
    }

    virtual void ClearLinearSystemPreconditioner()
    {
        this->EqnSysSelf->ClearLinearSystemPreconditioner();
    }
    
    virtual void ClearPreconditioner()
    {
        this->EqnSysSelf->ClearPreconditioner();
    }
    
    virtual void ClearLinearSystem()
    {
        this->EqnSysSelf->ClearLinearSystem();
    }
    
    //! @}
};

}    // end namespace Eqn
}    // end namespace Fem

#endif
