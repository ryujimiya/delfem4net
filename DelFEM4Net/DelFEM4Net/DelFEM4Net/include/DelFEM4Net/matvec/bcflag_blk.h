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
@brief 境界条件フラグクラス(MatVec::CBCFlag)のインターフェイス
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/


#if !defined(DELFEM4NET_BC_FLAG_H)
#define DELFEM4NET_BC_FLAG_H

#include "DelFEM4Net/stub/clr_stub.h"
#include "DelFEM4Net/matvec/vector_blk.h"
#include "DelFEM4Net/matvec/zvector_blk.h"

#include "delfem/matvec/bcflag_blk.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetMatVec
{
/*!
@brief 境界条件フラグ
@ingroup MatVec
*/
public ref class CBCFlag
{
public:
    // nativeインスタンスを作成する(コピーコンストラクタがないので)
    // @param rhs コピー元nativeインスタンス参照
    // @return 新たに生成されたnativeインスタンス
    static MatVec::CBCFlag * CreateNativeInstance(const MatVec::CBCFlag& rhs);
    
public:
    //CBCFlag();
    CBCFlag(const CBCFlag% rhs);
    CBCFlag(const MatVec::CBCFlag& native_instance); // nativeインスタンスから生成
    
private:  // nativeクラスのコピーコンストラクタ不備によりprivateに変更: マネージドクラスのコピーコンストラクタで対応しています
    CBCFlag(MatVec::CBCFlag *self);

public:
    CBCFlag(bool isCreateInstance);  // nativeインスタンスを生成しない(派生クラスのコンストラクタで使用)
    CBCFlag(unsigned int nBlk, unsigned int lenBlk);
    CBCFlag(unsigned int nBlk, IList<unsigned int>^ aLen);
    virtual ~CBCFlag();
    !CBCFlag();
    property MatVec::CBCFlag * Self
    {
       MatVec::CBCFlag * get();
    }

    ////////////////
    int LenBlk();
    unsigned int LenBlk(unsigned int iblk);
    unsigned int NBlk();
    int GetBCFlag(unsigned int iblk, unsigned int idofblk);

    ////////////////
    // フラグを全てリセット
    void SetAllFlagZero();
    void SetZeroToBCDof(CVector_Blk^ vec);   // [IN/OUT] vec
    void SetZeroToBCDof(DelFEM4NetMatVec::CZVector_Blk^ vec); // [IN/OUT] vec

    // iblk番目のブロックのidofblk番目の自由度を固定する
    bool SetBC(unsigned int iBlk, unsigned int iDofBlk);

protected:
    MatVec::CBCFlag *self;

};


// ポインタ操作用クラス
public ref class CBCFlag_Ptr : public CBCFlag
{
public:
    CBCFlag_Ptr(MatVec::CBCFlag *baseSelf) : CBCFlag(false)
    {
        // インスタンスを生成しない.受け皿を作るだけ
        this->self = baseSelf;
    }

private:
    CBCFlag_Ptr(const CBCFlag_Ptr% rhs) : CBCFlag(false)
    {
        this->self = rhs.self;
    }

public:
    ~CBCFlag_Ptr()
    {
        this->!CBCFlag_Ptr();
    }

    !CBCFlag_Ptr()
    {
        // 削除しない
        this->self = NULL;
    }

private:
    // selfはベースクラスのものを使用する

};

}    // end namespace 'Ls'

#endif
