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

#include "DelFEM4Net/drawer.h"
#include "DelFEM4Net/camera.h"
#include "DelFEM4Net/vector3d.h"
//#include "DelFEM4Net/quaternion.h"

using namespace DelFEM4NetCom::View;


//////////////////////////////////////////////////////////////////////////
// CEmptyDrawer
//////////////////////////////////////////////////////////////////////////
DelFEM4NetCom::CBoundingBox3D^ CEmptyDrawer::GetBoundingBox(array<double>^ rot)
{    
    DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D();
    
    return retManaged;
    
}

//////////////////////////////////////////////////////////////////////////
// CDrawerArray
//////////////////////////////////////////////////////////////////////////
CDrawerArray::CDrawerArray()
{
    this->self = new Com::View::CDrawerArray();

    // manageインスタンスのリストを生成
    this->drawer_ary_Managed = gcnew  List<CDrawer^>();
}

CDrawerArray::CDrawerArray(const CDrawerArray% rhs)
{
    const Com::View::CDrawerArray& rhs_instance_ = *(rhs.self);
    this->self = new Com::View::CDrawerArray(rhs_instance_);

    // manageインスタンスのリストを生成
    this->drawer_ary_Managed = gcnew  List<CDrawer^>();

    List<CDrawer^>^ rhs_drawer_ary_Managed = (List<CDrawer^>^)(rhs.drawer_ary_Managed);
    for each (CDrawer^ drawer in rhs_drawer_ary_Managed)
    {
        this->PushBack(drawer);
    }
}

CDrawerArray::CDrawerArray(Com::View::CDrawerArray *self)
{
    this->self = self;

    // manageインスタンスのリストを生成
    this->drawer_ary_Managed = gcnew  List<CDrawer^>();
}

CDrawerArray::~CDrawerArray()
{
    this->!CDrawerArray();
}
    
CDrawerArray::!CDrawerArray()
{
    // Note: アンマネージドのクラスのデストラクタでは各CDrawer要素のdeleteは行っていない
    //        Clearを明示的に呼ばない限り、CDrawerの破棄はユーザー任せのようである。
    delete this->self;

    // manageインスタンスリストを破棄
    //delete this->drawer_ary_Managed;
}
    
void CDrawerArray::PushBack(CDrawer^ pDrawer)
{
    assert(pDrawer != nullptr);
    Com::View::CDrawer *pDrawer_ = pDrawer->DrawerSelf;

    if (pDrawer_ != NULL) // CEmptyDrawerの場合、NULLになるので追加しない
    {
        this->self->PushBack(pDrawer_);
    }

    // managedのリストにも追加する
    drawer_ary_Managed->Add(pDrawer);
}

void CDrawerArray::Draw()
{
    //アンマネージドのメソッドは呼ばない
    //this->self->Draw();
    // マネージドを介してnativeを呼び出す
    for (int idraw=0; idraw < drawer_ary_Managed->Count; idraw++)
    {
        drawer_ary_Managed[idraw]->Draw();
    }
}

void CDrawerArray::DrawSelection()
{
    //アンマネージドのメソッドは呼ばない
    //this->self->DrawSelection();
    // マネージドを介してnativeを呼び出す
    for (int idraw=0; idraw < drawer_ary_Managed->Count; idraw++)
    {
        drawer_ary_Managed[idraw]->DrawSelection(idraw);
    }
}

void CDrawerArray::AddSelected(array<int>^ selec_flg)
{
    //アンマネージドのメソッドは呼ばない
    //pin_ptr<int> ptr = &selec_flg[0];
    //int *selec_flg_ = ptr;
    //
    //this->self->AddSelected(selec_flg_);
    // マネージドを介してnativeを呼び出す
    for (int idraw=0; idraw < drawer_ary_Managed->Count; idraw++)
    {
        drawer_ary_Managed[idraw]->AddSelected(selec_flg);
    }    
}

void CDrawerArray::ClearSelected()
{
    //アンマネージドのメソッドは呼ばない
    //this->self->ClearSelected();
    // マネージドを介してnativeを呼び出す
    for (int idraw=0; idraw < drawer_ary_Managed->Count; idraw++)
    {
        drawer_ary_Managed[idraw]->ClearSelected();
    }
}

void CDrawerArray::Clear()
{
    // Note: アンマネージドのクラスのClearで、CDrawerインスタンスも破棄される。
    //       マネージド側では、nativeインスタンスは削除してはいけない
    //       ここでは応急処置として、GCのファイナライザ呼び出しを抑制する
    for (int idraw = 0; idraw < drawer_ary_Managed->Count; idraw++)
    {
        if (drawer_ary_Managed[idraw]->DrawerSelf != NULL)  // CEmptyDrawerを除き、GCのファイナライザ呼び出しを抑制する
        {
            GC::SuppressFinalize(drawer_ary_Managed[idraw]);
        }
    }
    // アンマネージドのクリア処理を実行する
    this->self->Clear();

    // managedのリストもクリアする
    drawer_ary_Managed->Clear();
}

DelFEM4NetCom::CBoundingBox3D^ CDrawerArray::GetBoundingBox(array<double>^ rot)
{
    //アンマネージドのメソッドは呼ばない
    //pin_ptr<double> ptr = nullptr;
    //double *rot_ = NULL;
    //
    //if (rot != nullptr && rot->Length > 0)
    //{
    //    ptr = &rot[0];
    //    rot_ = ptr;
    //}
    //else
    //{
    //    rot_ = NULL;  // rotに0を指定したときの処理があるようなので、エラーにしないでunmanagedインスタンスへ処理を渡す
    //}
    //
    //const Com::CBoundingBox3D& ret_instance_ = this->self->GetBoundingBox(rot_);
    //Com::CBoundingBox3D *ret = new Com::CBoundingBox3D(ret_instance_);
    //
    //DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
    //return retManaged;

    // 自力でアンマネージドと同等の処理をする
    if (drawer_ary_Managed->Count == 0)
    {
        return gcnew DelFEM4NetCom::CBoundingBox3D(-0.5,0.5, -0.5,0.5, -0.5,0.5);
    }    
    DelFEM4NetCom::CBoundingBox3D^ bb_Managed = drawer_ary_Managed[0]->GetBoundingBox(rot);
    for (int idraw = 1; idraw < drawer_ary_Managed->Count; idraw++)
    {
        DelFEM4NetCom::CBoundingBox3D^ work_Managed = drawer_ary_Managed[idraw]->GetBoundingBox(rot);
        *(bb_Managed->Self) += *(work_Managed->Self);
    }
    return bb_Managed;

}

void CDrawerArray::InitTrans (DelFEM4NetCom::View::CCamera^ mvp_trans)
{
    //アンマネージドの処理は呼ばない
    //Com::View::CCamera *mvp_trans_ = mvp_trans->Self;
    //this->self->InitTrans (*mvp_trans_);

    // 自力でアンマネージドと同等の処理をする
    {    // get suitable rot mode
        unsigned int irot_mode = 0;
        for(int idraw = 0; idraw < drawer_ary_Managed->Count; idraw++)
        {
            unsigned int irot_mode0 = drawer_ary_Managed[idraw]->GetSutableRotMode();
            irot_mode = (irot_mode0>irot_mode) ? irot_mode0 : irot_mode;
        }
        if(      irot_mode == 1 ){ mvp_trans->SetRotationMode(DelFEM4NetCom::View::ROTATION_MODE::ROT_2D);  }
        else if( irot_mode == 2 ){ mvp_trans->SetRotationMode(DelFEM4NetCom::View::ROTATION_MODE::ROT_2DH); }
        else if( irot_mode == 3 ){ mvp_trans->SetRotationMode(DelFEM4NetCom::View::ROTATION_MODE::ROT_3D);  }
    }
    // set object size to the transformation
    array<double>^ rot = gcnew array<double>(9);
    mvp_trans->RotMatrix33(rot);
    DelFEM4NetCom::CBoundingBox3D^ bb = this->GetBoundingBox( rot );
    mvp_trans->Fit(bb);
}


/////////////////////////////////////////////////////////////
// プロパティ
Com::View::CDrawerArray * CDrawerArray::Self::get()
{
    return this->self;
}

IList<CDrawer^>^ CDrawerArray::m_drawer_ary::get()
{
    /*
    std::vector<Com::View::CDrawer*>& vec = this->self->m_drawer_ary;  // リファレンスを取得
    std::vector<Com::View::CDrawer*>::iterator itr;
    
    IList<CDrawer^>^ list = gcnew List<CDrawer^>();
    
    if (vec.size() > 0)
    {
        for (itr = vec.begin(); itr != vec.end(); itr++)
        {            
            Com::View::CDrawer *e_ = new Com::View::CDrawer(*itr);// 抽象クラスをインスタンス化できない
            CDrawer^ e = gcnew CDrawer(e_); //'abstract' として宣言されたクラスをインスタンス生成することはできません
            list->Add(e);
        }
    }
    return list;
    */
    
    return this->drawer_ary_Managed;

}

/*
void CDrawerArray::m_drawer_ary::set(IList<CDrawer^>^ list)
{
    //std::vector<Com::View::CDrawer*>& vec = this->self->m_drawer_ary;  // リファレンスを取得(書き換え用)

    this->drawer_ary_Managed->Clear();
    //vec.clear();
    this->self->Clear();
    if (list->Count > 0)
    {
        for each (CDrawer^ e in list)
        {
            //this->drawer_ary_Managed->Add(e);
            //Com::View::CDrawer *e_ = e->Self;
            //vec.push_back(e_);

            this->PushBack(e);
        }
    }
    
}
*/

//////////////////////////////////////////////////////////////////////////
// CVertexArray
//////////////////////////////////////////////////////////////////////////

// nativeインスタンスを作成する(コピーコンストラクタがないので)
// @param rhs コピー元nativeインスタンス参照
// @return 新たに生成されたnativeインスタンス
Com::View::CVertexArray * CVertexArray::CreateNativeInstance(const Com::View::CVertexArray& rhs)
{
    unsigned int npoin = rhs.NPoin();
    unsigned int ndim = rhs.NDim();
    Com::View::CVertexArray *ptr = new Com::View::CVertexArray();
    ptr->SetSize(npoin, ndim);
    {
        int size = (int)npoin * ndim;
        for (int i = 0; i < size; i++)
        {
            ptr->pVertexArray[i] = rhs.pVertexArray[i];
        }
    }
    if (ptr->pUVArray != NULL)
    {
        int size = (int)npoin * 2;
        ptr->EnableUVMap(true);
        for (int i = 0; i < size; i++)
        {
            ptr->pUVArray[i] = rhs.pUVArray[i];
        }
    }
    return ptr;
}

CVertexArray::CVertexArray()
{
    this->self = new Com::View::CVertexArray();
}

CVertexArray::CVertexArray(const CVertexArray% rhs)
{
    const Com::View::CVertexArray& rhs_instance_ = *(rhs.self);
    // コピーコンストラクタはshallow copyとなるので問題がある
    //this->self = new Com::View::CVertexArray(rhs_instance_);
    // 自力でコピー
    this->self = CVertexArray::CreateNativeInstance(rhs_instance_);
}

CVertexArray::CVertexArray(const unsigned int np, const unsigned int nd)
{
    this->self = new Com::View::CVertexArray(np, nd);
}

CVertexArray::CVertexArray(Com::View::CVertexArray *self)
{
    this->self = self;
}

CVertexArray::~CVertexArray()
{
    this->!CVertexArray();
}

CVertexArray::!CVertexArray()
{
    delete this->self;
}

void CVertexArray::SetSize(unsigned int npoin, unsigned int ndim)
{
    this->self->SetSize(npoin, ndim);
}

// rot is 3 by 3 matrix for rotation
// if rot is 0 this function don't perform rotation in measuring size
DelFEM4NetCom::CBoundingBox3D^ CVertexArray::GetBoundingBox(array<double>^ rot)
{
    pin_ptr<double> ptr = nullptr;
    double *rot_ = NULL;
    
    if (rot != nullptr && rot->Length > 0)
    {
        ptr = &rot[0];
        rot_ = ptr;
    }
    else
    {
        rot_ = NULL;  // rotに0を指定したときの処理があるようなので、エラーにしないでunmanagedインスタンスへ処理を渡す
    }
    
    const Com::CBoundingBox3D& ret_instance_ = this->self->GetBoundingBox(rot_);
    Com::CBoundingBox3D *ret = new Com::CBoundingBox3D(ret_instance_);

    DelFEM4NetCom::CBoundingBox3D^ retManaged = gcnew DelFEM4NetCom::CBoundingBox3D(ret);
    return retManaged;
}
/* プロパティに移動
unsigned int CVertexArray::NDim()
{
    return this->self->NDim();
}

unsigned int CVertexArray::NPoin()
{
    return this->self->NPoin();
}
*/

void CVertexArray::EnableUVMap(bool is_uv_map)
{
    this->self->EnableUVMap(is_uv_map);
}

/////////////////////////////////////////////////////////////
// プロパティ
Com::View::CVertexArray * CVertexArray::Self::get()
{
    return this->self;
}

unsigned int CVertexArray::NDim::get()
{
    return this->self->NDim();
}

unsigned int CVertexArray::NPoin::get()
{
    return this->self->NPoin();
}

DelFEM4NetCom::DoubleArrayIndexer^ CVertexArray::pVertexArray::get()
{
    // 配列サイズが更新されるので、プロパティ呼び出し時に初期化する
    int length = this->self->NPoin() * this->self->NDim();
    pVertexArrayIndexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(length, &(this->self->pVertexArray[0]));

    return pVertexArrayIndexer;
}

DelFEM4NetCom::DoubleArrayIndexer^ CVertexArray::pUVArray::get()
{
    // 配列サイズが更新されるので、プロパティ呼び出し時に初期化する
    if (this->self->pUVArray == NULL)
    {
        // EnableUVMap(false)のとき
        pUVArrayIndexer = nullptr;
    }
    else
    {
        // EnableUVMap(true)のとき
        int size  = (int)this->self->NPoin() * 2;
        pUVArrayIndexer = gcnew DelFEM4NetCom::DoubleArrayIndexer(size, &(this->self->pUVArray[0]));
    }
    return pUVArrayIndexer;
}
