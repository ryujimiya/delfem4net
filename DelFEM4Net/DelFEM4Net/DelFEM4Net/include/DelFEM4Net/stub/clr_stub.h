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
@brief  C++/CLI(CLR) stub for DelFEM
@author ryujimiya (original DelFEM code was created by Nobuyuki Umetani)
*/

#if !defined(DELFEM4NET_CLR_STUB)
#define DELFEM4NET_CLR_STUB

#include <assert.h>
#include <vector>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

namespace DelFEM4NetCom
{

////////////////////////////////////////////////////////////////////////////////
// C++/CLI用スタブユーティリティ(このアセンブリ内でのみ参照)
////////////////////////////////////////////////////////////////////////////////
public ref class ClrStub
{
public:
    // マネージドストリングをstd::stringに変換
    // @param str [IN]  String^
    // @return std::string
    static std::string StringToStd(String^ str)
    {
        IntPtr mPtr = Marshal::StringToHGlobalAnsi(str);
        const char *c_str = static_cast<const char *>(mPtr.ToPointer());
        std::string str_ = c_str;  // const char *を元にstd::stringを生成(コピー)
        Marshal::FreeHGlobal(mPtr);
        
        return str_;
    }

    // std::stringをマネージドストリングに変換
    // @param str_ [IN] std::string
    // @return String^
    static String^ StdToString(const std::string& str_)
    {
        std::string str_copy_ = str_;
        char *ptr = &str_copy_[0];
        String^ str = Marshal::PtrToStringAnsi((IntPtr)ptr);
        
        return str;
    }
    
    // std::char*をマネージドストリングに変換
    // @param ptr [IN] char *  native文字列へのポインタ(const char *でないのでは、IntPtrに変換できないため)
    // @return String^
    static String^ PtrToString(char *ptr)
    {
        return Marshal::PtrToStringAnsi((IntPtr)ptr);
    }

    static String^ PtrToString(const char *c_ptr)
    {
        char *ptr = const_cast<char *>(c_ptr);   // constを外す.Marshal::PtrToStringAnsiはString^に変換する際コピーを取っているから問題ないはず
        return Marshal::PtrToStringAnsi((IntPtr)ptr);
    }


    // マネージドリストをstd::vectorに変換
    // @param list  [IN] IList<T>^
    // @return std::vector<T>
    template<typename T>
    static std::vector<T> ListToVector(IList<T>^ list)
    {
        std::vector<T> vec;
        
        ListToVector(list, vec);

        return vec;
    }

    // マネージドリストをstd::vectorに変換
    //   パラメータで出力先指定 (nativeクラスのメンバ変数のリファレンスを渡して書き換えるときに使用)
    // @param list  [IN] IList<T>^
    // @param std::vector<T>
    template<typename T>
    static void ListToVector(IList<T>^ list, std::vector<T>& vec)
    {
        vec.clear();
        if (list->Count > 0)
        {
            for each (T e in list)
            {
                vec.push_back(e);
            }
        }
    }

    // マネージドリストをstd::vectorに変換(クラスインスタンス版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンス 変換が行われる
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2>  T2 アンマネージドクラス(インスタンス指定) 
    template<typename T1, typename T2>
    static std::vector<T2> ListToInstanceVector(IList<T1^>^ list)
    {
        std::vector<T2> vec;
        
        ListToInstanceVector(list, vec);

        return vec;
    }
    
    // マネージドリストをstd::vectorに変換(クラスインスタンス版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンス 変換が行われる
    //   パラメータで出力先指定 (nativeクラスのメンバ変数のリファレンスを渡して書き換えるときに使用)
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @param std::vector<T2>   [OUT] T2 アンマネージドクラス(インスタンス指定) 
    template<typename T1, typename T2>
    static void ListToInstanceVector(IList<T1^>^ list, std::vector<T2>& vec)
    {
        vec.clear();
        if (list->Count > 0)
        {
            for each (T1^ e in list)
            {
                T2 e_instance_ = *(e->Self);
                vec.push_back(e_instance_);
            }
        }
    }

    // マネージドリストをstd::vectorに変換(クラスインスタンス版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンス 変換が行われる
    //   nativeインスタンスは生成しない
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2>  T2 アンマネージドクラス(インスタンス指定) 
    template<typename T1, typename T2>
    static std::vector<T2> ListToInstanceVector_NoCreate(IList<T1^>^ list)
    {
        std::vector<T2> vec;
        
        ListToInstanceVector_NoCreate(list, vec);

        return vec;
    }
    
    // マネージドリストをstd::vectorに変換(クラスインスタンス版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンス 変換が行われる
    //   nativeインスタンスは生成しない
    //   パラメータで出力先指定 (nativeクラスのメンバ変数のリファレンスを渡して書き換えるときに使用)
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @param std::vector<T2>   [OUT] T2 アンマネージドクラス(インスタンス指定) 
    template<typename T1, typename T2>
    static void ListToInstanceVector_NoCreate(IList<T1^>^ list, std::vector<T2>& vec)
    {
        vec.clear();
        if (list->Count > 0)
        {
            for each (T1^ e in list)
            {
                // インスタンスのリファレンスを渡す
                T2& e_instance_ = *(e->Self);
                vec.push_back(e_instance_);
            }
        }
    }

    // std::vectorをマネージドリストに変換
    // @param std::vector<T>
    // @return list  List<T>^
    template<typename T>
    static IList<T>^ VectorToList(const std::vector<T>& vec)
    {
        IList<T>^ list = gcnew List<T>();
        if (vec.size() > 0)
        {
            for (std::vector<T>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
            {
                list->Add(*itr);
            }
        }
        return list;
    }
    
    // std::vectorをマネージドリストに変換(クラスインスタンス版)
    //   内部でネイティブインスタンス → マネージドインスタンスハンドル 変換が行われる
    // @param std::vector<T1>   T1 アンマネージドクラス(インスタンス指定) 
    // @return list  IList<T2^>^  T2 マネージドクラス
    template<typename T1, typename T2>
    static IList<T2^>^ InstanceVectorToList(const std::vector<T1>& vec)
    {
        IList<T2^>^ list = gcnew List<T2^>();
        if (vec.size() > 0)
        {
            for (std::vector<T1>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
            {
                T1 * e_ptr_ = new T1(*itr); // nativeインスタンスをコピーコンストラクタで新たに生成
                T2^ e = gcnew T2(e_ptr_) ; // マネージドクラスインスタンスを生成
                list->Add(e);  // ハンドルをリストに追加
            }
        }
        return list;
    }

    /*
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
                T2* e_instance_ptr_ = e->Self;
                vec.push_back(e_instance_ptr_);
            }
        }
    }
    */

    /*
    // マネージドリストをstd::vectorに変換(クラスインスタンスのポインタ版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンスのポインタ 変換が行われる
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2*>  T2 アンマネージドクラスインスタンス(インスタンス指定)
    template<typename T1, typename T2>
    static std::vector<T2*> ListToInstancePtrVector(IList<T1^>^ list)
    {
        std::vector<T2*> vec;
        
        ListToInstancePtrVector(list, vec);

        return vec;
    }
    
    // マネージドリストをstd::vectorに変換(クラスインスタンスのポインタ版)
    //   内部でマネージドインスタンスハンドル → ネイティブインスタンスのポインタ 変換が行われる
    //   パラメータで出力先指定 (nativeクラスのメンバ変数のリファレンスを渡して書き換えるときに使用)
    // @param list  IList<T1^>^  [IN]  T1 マネージドクラス
    // @return std::vector<T2*>  T2 アンマネージドクラスインスタンス(インスタンス指定)
    template<typename T1, typename T2>
    static void ListToInstancePtrVector(IList<T1^>^ list, std::vector<T2*>& vec)
    {
        vec.clear();
        if (list->Count > 0)
        {
            for each (T1^ e in list)
            {
                // マネージドインスタンスで管理しているnativeポインタをコピーして渡す。
                T2* e_instance_ptr_ = new T2(*(e->Self));
                vec.push_back(e_instance_ptr_);
            }
        }
    }

    // std::vectorをマネージドリストに変換(クラスインスタンス版)
    //   内部でネイティブインスタンスポインタ → マネージドインスタンスハンドル 変換が行われる
    // @param std::vector<T1*>   T1Ptr アンマネージドクラスポインタ 
    // @return list  IList<T2^>^  T2 マネージドクラス
    template<typename T1, typename T2>
    static IList<T2^>^ InstanceVectorToList(const std::vector<T1*>& vec)
    {
        IList<T2^>^ list = gcnew List<T2^>();
        if (vec.size() > 0)
        {
            for (std::vector<T1*>::const_iterator itr = vec.begin(); itr != vec.end(); itr++)
            {
                T1 * e_ptr_src_ = *itr;
                T1 * e_ptr_ = new T1(*e_ptr_src_); // nativeインスタンスをコピーコンストラクタで新たに生成
                T2^ e = gcnew T2(e_ptr_) ; // マネージドクラスインスタンスを生成
                list->Add(e);  // ハンドルをリストに追加
            }
        }
        return list;
    }
    */

};

////////////////////////////////////////////////////////////////////////////////
// std::pairの代替
////////////////////////////////////////////////////////////////////////////////
generic<typename T, typename U>
public ref class Pair
{
public:
    Pair()
    {
    }
    Pair(T first, U second)
    {
        this->First = first;
        this->Second = second;
    }
    ~Pair()
    {
        this->!Pair();
    }
    !Pair()
    {
    }
    
public:
    property T First;
    property U Second;
};

/*
generic <class T1, class T2>
public value struct pair
{
    typedef T1 first_type;
    typedef T2 second_type;

    T1 first;
    T2 second;

    pair( T1 t1, T2 t2 ): first( t1 ), second( t2 ) {}  
}
*/

////////////////////////////////////////////////////////////////////////////////
// 配列
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// native配列の要素にアクセスするためのインデクサクラス
//    T foo[];
////////////////////////////////////////////////////////////////////////////////
template<typename T>
public ref class NativeArrayIndexer : public System::Collections::Generic::IList<T>
{
public:
    ref class NativeArrayIndexerEnumerator : System::Collections::Generic::IEnumerator<T>
    {
    public:
        NativeArrayIndexerEnumerator( NativeArrayIndexer^ indexer )
        {
            this->nativeArrayIndexer = indexer;
            this->currentIndex = -1;
        }
        ~NativeArrayIndexerEnumerator()
        {
            this->!NativeArrayIndexerEnumerator();
        }
        !NativeArrayIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->nativeArrayIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property T Current
        {
            virtual T get()
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        NativeArrayIndexer^ nativeArrayIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator<T>^ GetEnumerator()
    {
        return gcnew NativeArrayIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew NativeArrayIndexerEnumerator(this);
    }

public:
    NativeArrayIndexer(int size_, T *ptr_) : size(size_) , ptr(ptr_)
    {
    }
    
    NativeArrayIndexer(const NativeArrayIndexer% rhs) : size(rhs.size) , ptr(rhs.ptr)
    {
    }
    
    ~NativeArrayIndexer()
    {
        this->!NativeArrayIndexer();
    }
    
    !NativeArrayIndexer()
    {
    }
    
    property T default[int]
    {
        virtual T get(int index)
        {
            assert(index >= 0 && index < this->size);
            return this->ptr[index];
        }
        virtual void set(int index, T value)
        {
            assert(index >= 0 && index < this->size);
            this->ptr[index] = value;
        }
    }

    property int Count
    {
        virtual int get() { return this->size; }
    }
    
    property bool IsReadOnly
    {
        virtual bool get() { return false; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() { return true; }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(T value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void Clear()
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Contains(T value)
    {
        bool inList = false;
        for (int i = 0; i < this->size; i++)
        {
            if (this->default[i] == value)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(T value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, T value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Remove(T value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void CopyTo(array<T>^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }
    
private:
    // 配列のサイズと配列先頭のポインタを保持する
    int size;
    T* ptr;

};

//typedef NativeArrayIndexer<double> DoubleArrayIndexer;
public ref class DoubleArrayIndexer : public NativeArrayIndexer<double>
{
public:
    DoubleArrayIndexer(int size_, double *ptr_) : NativeArrayIndexer<double>(size_, ptr_)
    {
    }
    
    DoubleArrayIndexer(const DoubleArrayIndexer% rhs) : NativeArrayIndexer<double>(rhs)
    {
    }
    
    ~DoubleArrayIndexer()
    {
        this->!DoubleArrayIndexer();
    }
    
    !DoubleArrayIndexer()
    {
    }
};

//typedef NativeArrayIndexer<unsigned int> UIntArrayIndexer;
public ref class UIntArrayIndexer : public NativeArrayIndexer<unsigned int>
{
public:
    UIntArrayIndexer(int size_, unsigned int *ptr_) : NativeArrayIndexer<unsigned int>(size_, ptr_)
    {
    }
    
    UIntArrayIndexer(const UIntArrayIndexer% rhs) : NativeArrayIndexer<unsigned int>(rhs)
    {
    }
    
    ~UIntArrayIndexer()
    {
        this->!UIntArrayIndexer();
    }
    
    !UIntArrayIndexer()
    {
    }
};

////////////////////////////////////////////////////////////////////////////////
// native配列の要素にアクセスするためのインデクサクラス(const版)
//    const T foo[];
////////////////////////////////////////////////////////////////////////////////
template<typename T>
public ref class ConstNativeArrayIndexer : public System::Collections::Generic::IList<T>
{
public:
    ref class ConstNativeArrayIndexerEnumerator : System::Collections::Generic::IEnumerator<T>
    {
    public:
        ConstNativeArrayIndexerEnumerator( ConstNativeArrayIndexer^ indexer )
        {
            this->nativeArrayIndexer = indexer;
            this->currentIndex = -1;
        }
        ~ConstNativeArrayIndexerEnumerator()
        {
            this->!ConstNativeArrayIndexerEnumerator();
        }
        !ConstNativeArrayIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->nativeArrayIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property T Current
        {
            virtual T get()
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        ConstNativeArrayIndexer^ nativeArrayIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator<T>^ GetEnumerator()
    {
        return gcnew ConstNativeArrayIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew ConstNativeArrayIndexerEnumerator(this);
    }

public:
    ConstNativeArrayIndexer(int size_, const T *ptr_) : size(size_) , ptr(ptr_)
    {
    }
    
    ConstNativeArrayIndexer(const ConstNativeArrayIndexer% rhs) : size(rhs.size) , ptr(rhs.ptr)
    {
    }
    
    ~ConstNativeArrayIndexer()
    {
        this->!ConstNativeArrayIndexer();
    }
    
    !ConstNativeArrayIndexer()
    {
    }
    
    property T default[int]
    {
        virtual T get(int index)
        {
            assert(index >= 0 && index < this->size);
            return this->ptr[index];
        }
        private:
            virtual void set(int index, T value) sealed = System::Collections::Generic::IList<T>::default::set
            {
                throw gcnew NotImplementedException();
                //assert(index >= 0 && index < this->size);
                //this->ptr[index] = value;
            }
    }

    property int Count
    {
        virtual int get() { return this->size; }
    }

    property bool IsReadOnly
    {
        virtual bool get() { return true; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() { return true; }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(T value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void Clear()
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Contains(T value)
    {
        bool inList = false;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(T value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, T value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Remove(T value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void CopyTo(array<T>^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }

private:
    // 配列のサイズと配列先頭のポインタを保持する
    int size;
    const T* ptr;

};

//typedef ConstNativeArrayIndexer<unsigned int> ConstUIntArrayIndexer;
public ref class ConstUIntArrayIndexer : public ConstNativeArrayIndexer<unsigned int>
{
public:
    ConstUIntArrayIndexer(int size_, const unsigned int *ptr_) : ConstNativeArrayIndexer<unsigned int>(size_, ptr_)
    {
    }
    
    ConstUIntArrayIndexer(const ConstUIntArrayIndexer% rhs) : ConstNativeArrayIndexer<unsigned int>(rhs)
    {
    }
    
    ~ConstUIntArrayIndexer()
    {
        this->!ConstUIntArrayIndexer();
    }
    
    !ConstUIntArrayIndexer()
    {
    }
};

//typedef ConstNativeArrayIndexer<double> ConstDoubleArrayIndexer;
public ref class ConstDoubleArrayIndexer : public ConstNativeArrayIndexer<double>
{
public:
    ConstDoubleArrayIndexer(int size_, const double *ptr_) : ConstNativeArrayIndexer<double>(size_, ptr_)
    {
    }
    
    ConstDoubleArrayIndexer(const ConstDoubleArrayIndexer% rhs) : ConstNativeArrayIndexer<double>(rhs)
    {
    }
    
    ~ConstDoubleArrayIndexer()
    {
        this->!ConstDoubleArrayIndexer();
    }
    
    !ConstDoubleArrayIndexer()
    {
    }
};


////////////////////////////////////////////////////////////////////////////////
// nativeインスタンス配列の要素にアクセスするためのインデクサクラス
//    T1 foo[];   T1;ネイティブクラス(インスタンスの配列)
//    --> T2^ bar[];  T2:マネージドクラス(ハンドルの配列)
////////////////////////////////////////////////////////////////////////////////
template<typename T1, typename T2, typename T2Handle>
public ref class NativeInstanceArrayIndexer : public System::Collections::Generic::IList<T2Handle>
{
public:
    ref class NativeInstanceArrayIndexerEnumerator : System::Collections::Generic::IEnumerator<T2Handle>
    {
    public:
        NativeInstanceArrayIndexerEnumerator( NativeInstanceArrayIndexer^ indexer )
        {
            this->nativeArrayIndexer = indexer;
            this->currentIndex = -1;
        }
        ~NativeInstanceArrayIndexerEnumerator()
        {
            this->!NativeInstanceArrayIndexerEnumerator();
        }
        !NativeInstanceArrayIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->nativeArrayIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property T2^ Current
        {
            virtual T2^ get()
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->nativeArrayIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        NativeInstanceArrayIndexer^ nativeArrayIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator<T2^>^ GetEnumerator()
    {
        return gcnew NativeInstanceArrayIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew NativeInstanceArrayIndexerEnumerator(this);
    }

public:
    NativeInstanceArrayIndexer(int size_, T1 *ptr_) : size(size_) , ptr(ptr_)
    {
    }
    
    NativeInstanceArrayIndexer(const NativeInstanceArrayIndexer% rhs) : size(rhs.size) , ptr(rhs.ptr)
    {
    }
    
    ~NativeInstanceArrayIndexer()
    {
        this->!NativeInstanceArrayIndexer();
    }
    
    !NativeInstanceArrayIndexer()
    {
    }
    
    property T2^ default[int]
    {
        virtual T2^ get(int index)
        {
            assert(index >= 0 && index < this->size);
            const T1& e_instance_ = this->ptr[index];
            T1 * e_ = new T1(e_instance_);
            return gcnew T2(e_);
        }
        virtual void set(int index, T2^ value)
        {
            assert(index >= 0 && index < this->size);
            const T1& e_instance_ = *(value->Self);
            this->ptr[index] = e_instance_;
        }
    }

    property int Count
    {
        virtual int get() { return this->size; }
    }
    
    property bool IsReadOnly
    {
        virtual bool get() { return false; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() { return true; }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(T2^ value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void Clear()
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Contains(T2^ value)
    {
        bool inList = false;
        for (int i = 0; i < this->size; i++)
        {
            if (this->default[i] == value)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(T2^ value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, T2^ value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Remove(T2^ value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void CopyTo(array<T2^>^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }
    
private:
    // 配列のサイズと配列先頭のポインタを保持する
    int size;
    T1* ptr;

};

////////////////////////////////////////////////////////////////////////////////
// ベクター
////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
// std::vectorの要素にアクセスするためのインデクサ
//    std::vector<T> foo;
//    ※このクラスは追加、削除等可能
////////////////////////////////////////////////////////////////////////////////
template<typename T>
public ref class VectorIndexer : public System::Collections::Generic::IList<T>
{
public:
    ref class VectorIndexerEnumerator : System::Collections::Generic::IEnumerator<T>
    {
    public:
        VectorIndexerEnumerator( VectorIndexer^ indexer )
        {
            this->vectorIndexer = indexer;
            this->currentIndex = -1;
        }
        ~VectorIndexerEnumerator()
        {
            this->!VectorIndexerEnumerator();
        }
        !VectorIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->vectorIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property T Current
        {
            virtual T get()
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        VectorIndexer^ vectorIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator<T>^ GetEnumerator()
    {
        return gcnew VectorIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew VectorIndexerEnumerator(this);
    }

public:
    VectorIndexer(std::vector<T>& vec_) : vec(vec_)
    {
    }
    
    VectorIndexer(const VectorIndexer% rhs) : vec(rhs.vec)
    {
    }
    
    ~VectorIndexer()
    {
        this->!VectorIndexer();
    }
    
    !VectorIndexer()
    {
    }
    
    property T default[int]
    {
        virtual T get(int index)
        {
            assert(index >= 0 && index < (int)this->vec.size());
            return this->vec[index];
        }
        virtual void set(int index, T value)
        {
            assert(index >= 0 && index < (int)this->vec.size());
            this->vec[index] = value;
        }
    }
    
    property int Count
    {
        virtual int get() { return this->vec.size(); }
    }
    
    property bool IsReadOnly
    {
        virtual bool get() { return false; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() 
        {
            // サイズ変更可能
            return false;
        }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(T value)
    {
        //throw gcnew NotImplementedException();
        vec.push_back(value);
    }
    
    virtual void Clear()
    {
        //throw gcnew NotImplementedException();
        vec.clear();
    }
    
    virtual bool Contains(T value)
    {
        bool inList = false;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(T value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, T value)
    {
        //throw gcnew NotImplementedException();
        vec.insert(vec.begin() + index, value);
    }
    
    virtual bool Remove(T value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index)
    {
        //throw gcnew NotImplementedException();
        vec.erase(vec.begin() + index);
    }
    
    virtual void CopyTo(array<T>^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }

private:
    // ベクタの参照を保持する
    std::vector<T>& vec;

};

//typedef VectorIndexer<unsigned int> UIntVectorIndexer;
public ref class UIntVectorIndexer : public VectorIndexer<unsigned int>
{
public:
    UIntVectorIndexer(std::vector<unsigned int>& vec_) : VectorIndexer<unsigned int>(vec_)
    {
    }
    
    UIntVectorIndexer(const UIntVectorIndexer% rhs) : VectorIndexer<unsigned int>(rhs)
    {
    }
    
    ~UIntVectorIndexer()
    {
        this->!UIntVectorIndexer();
    }
    
    !UIntVectorIndexer()
    {
    }
};

////////////////////////////////////////////////////////////////////////////////
// std::vector<std::pair<,>>のpair要素にアクセスするためのインデクサ
//   std::vector< std::pair<T, U> > foo;  --> std::pairをDelFEM4NetCom::Pairに置き換え
//   const版
////////////////////////////////////////////////////////////////////////////////
template<typename T, typename U>
public ref class ConstPairVectorIndexer : public System::Collections::Generic::IList< DelFEM4NetCom::Pair<T, U>^ >
{

public:
    ref class ConstPairVectorIndexerEnumerator : System::Collections::Generic::IEnumerator< DelFEM4NetCom::Pair<T, U>^ >
    {
    public:
        ConstPairVectorIndexerEnumerator( ConstPairVectorIndexer^ indexer )
        {
            this->vectorIndexer = indexer;
            this->currentIndex = -1;
        }
        ~ConstPairVectorIndexerEnumerator()
        {
            this->!ConstPairVectorIndexerEnumerator();
        }
        !ConstPairVectorIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->vectorIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property DelFEM4NetCom::Pair<T, U>^ Current
        {
            virtual DelFEM4NetCom::Pair<T, U>^ get()
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        ConstPairVectorIndexer^ vectorIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator< DelFEM4NetCom::Pair<T, U>^ >^ GetEnumerator()
    {
        return gcnew ConstPairVectorIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew ConstPairVectorIndexerEnumerator(this);
    }

public:
    ConstPairVectorIndexer(const std::vector< std::pair<T, U> >& vec_) : vec(vec_)
    {
    }
    
    ConstPairVectorIndexer(const ConstPairVectorIndexer% rhs) : vec(rhs.vec)
    {
    }
    
    ~ConstPairVectorIndexer()
    {
        this->!ConstPairVectorIndexer();
    }
    
    !ConstPairVectorIndexer()
    {
    }
    
    property DelFEM4NetCom::Pair<T, U>^ default[int]
    {
        virtual DelFEM4NetCom::Pair<T, U>^ get(int index)
        {
            assert(index >= 0 && index < (int)this->vec.size());
            const std::pair<T, U>& pair = this->vec[index];
            return gcnew DelFEM4NetCom::Pair<T, U>(pair.first, pair.second);
        }
        private:
            virtual void set(int index, DelFEM4NetCom::Pair<T, U>^ value) sealed = System::Collections::Generic::IList< DelFEM4NetCom::Pair<T, U>^ >::default::set
            {
                throw gcnew NotImplementedException();
                //assert(index >= 0 && index < (int)this->vec.size());
                //this->vec[index] = std::pair( value->First, value->Second ) ;
            }
    }
    
    property int Count
    {
        virtual int get() { return this->vec.size(); }
    }
    
    property bool IsReadOnly
    {
        virtual bool get() { return false; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() { return true; }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(DelFEM4NetCom::Pair<T, U>^ value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void Clear()
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Contains(DelFEM4NetCom::Pair<T, U>^ value)
    {
        bool inList = false;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(DelFEM4NetCom::Pair<T, U>^ value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            if (this->default[i] == value)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, DelFEM4NetCom::Pair<T, U>^ value)
    {
        throw gcnew NotImplementedException();
    }
    
    virtual bool Remove(DelFEM4NetCom::Pair<T, U>^ value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index) 
    {
        throw gcnew NotImplementedException();
    }
    
    virtual void CopyTo(array< DelFEM4NetCom::Pair<T, U>^ >^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }

private:
    // ベクタの参照を保持する
    const std::vector< std::pair<T, U> >& vec;

};

//typedef ConstPairVectorIndexer<unsigned int, double> ConstUIntDoublePairVectorIndexer;
public ref class ConstUIntDoublePairVectorIndexer : public ConstPairVectorIndexer<unsigned int, double>
{
public:
    ConstUIntDoublePairVectorIndexer(const std::vector< std::pair<unsigned int, double> >& vec_) : ConstPairVectorIndexer<unsigned int, double>(vec_)
    {
    }
    
    ConstUIntDoublePairVectorIndexer(const ConstPairVectorIndexer% rhs) : ConstPairVectorIndexer<unsigned int, double>(rhs)
    {
    }
    
    ~ConstUIntDoublePairVectorIndexer()
    {
        this->!ConstUIntDoublePairVectorIndexer();
    }
    
    !ConstUIntDoublePairVectorIndexer()
    {
    }

};

////////////////////////////////////////////////////////////////////////////////
// std::vectorのnativeインスタンスの要素にアクセスするためのインデクサクラス
//    std::vector<T1> foo[];   T1;ネイティブクラス(インスタンスのstd::vector)
//    --> T2^ bar[];  T2:マネージドクラス(ハンドルの配列)
//    ※このクラスは追加、削除等可能
////////////////////////////////////////////////////////////////////////////////
template<typename T1, typename T2, typename T2Handle>
public ref class NativeInstanceVectorIndexer : public System::Collections::Generic::IList<T2Handle>
{
public:
    ref class NativeInstanceVectorIndexerEnumerator : System::Collections::Generic::IEnumerator<T2Handle>
    {
    public:
        NativeInstanceVectorIndexerEnumerator( NativeInstanceVectorIndexer^ indexer )
        {
            this->vectorIndexer = indexer;
            this->currentIndex = -1;
        }
        ~NativeInstanceVectorIndexerEnumerator()
        {
            this->!NativeInstanceVectorIndexerEnumerator();
        }
        !NativeInstanceVectorIndexerEnumerator()
        {
        }

        virtual bool MoveNext()
        {
            if( currentIndex < this->vectorIndexer->Count - 1 )
            {
                currentIndex++;
                return true;
            }
            return false;
        }
    
        property T2Handle Current
        {
            virtual T2Handle get()
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        // This is required as IEnumerator<T> also implements IEnumerator
        property Object^ CurrentNoGeneric
        {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get
            {
                return this->vectorIndexer[currentIndex];
            }
        };
        
        virtual void Reset()
        {
            currentIndex = -1;
        }

     private:
        NativeInstanceVectorIndexer^ vectorIndexer;
        int currentIndex;
    };
    
    virtual System::Collections::Generic::IEnumerator<T2Handle>^ GetEnumerator()
    {
        return gcnew NativeInstanceVectorIndexerEnumerator(this);
    }
    
private:
    virtual System::Collections::IEnumerator^ GetEnumeratorNoGeneric() sealed = System::Collections::IEnumerable::GetEnumerator
    {
        return gcnew NativeInstanceVectorIndexerEnumerator(this);
    }

public:
    NativeInstanceVectorIndexer(std::vector<T1>& vec_) : vec(vec_)
    {
    }
    
    NativeInstanceVectorIndexer(const NativeInstanceVectorIndexer% rhs) : vec(rhs.vec)
    {
    }
    
    ~NativeInstanceVectorIndexer()
    {
        this->!NativeInstanceVectorIndexer();
    }
    
    !NativeInstanceVectorIndexer()
    {
    }
    
    property T2^ default[int]
    {
        virtual T2^ get(int index)
        {
            assert(index >= 0 && index < this->vec.size());
            const T1& e_instance_ = this->vec[index];
            T1 * e_ = new T1(e_instance_);
            return gcnew T2(e_);
        }
        virtual void set(int index, T2^ value)
        {
            assert(index >= 0 && index < this->vec.size());
            const T1& e_instance_ = *(value->Self);
            this->vec[index] = e_instance_;
        }
    }
    
    property int Count
    {
        virtual int get() { return this->vec.size(); }
    }
    
    property bool IsReadOnly
    {
        virtual bool get() { return false; }
    }
    
    property bool IsFixedSize
    {
        virtual bool get() 
        {
            // サイズ変更可能
            return false;
        }
    }
    
    property bool IsSyncronized
    {
        virtual bool get() { return false; }
    }
    
    property Object^ SyncRoot
    {
        virtual Object^ get() { return this; } 
    }
    
    virtual void Add(T2^ value)
    {
        //throw gcnew NotImplementedException();
        const T1& e_instance_ = *(value->Self);
        vec.push_back(e_instance);
    }
    
    virtual void Clear()
    {
        //throw gcnew NotImplementedException();
        vec.clear();
    }
    
    virtual bool Contains(T2^ value)
    {
        bool inList = false;
        const T1& e_instance_ = *(value->Self);
        for (int i = 0; i < this->Count; i++)
        {
            // == 演算子の定義されているnativeインスタンスでなければならない
            if (this->default[i] == e_instance)
            {
                inList = true;
                break;
            }
        }
        return inList;
    }

    virtual int IndexOf(T2^ value)
    {
        int itemIndex = -1;
        for (int i = 0; i < this->Count; i++)
        {
            // == 演算子の定義されているnativeインスタンスでなければならない
            if (this->default[i] == e_instance)
            {
                itemIndex = i;
                break;
            }
        }
        return itemIndex;
    }
    
    virtual void Insert(int index, T2^ value)
    {
        //throw gcnew NotImplementedException();
        const T1& e_instance_ = *(value->Self);
        vec.insert(vec.begin() + index, e_instance);
    }
    
    virtual bool Remove(T2^ value)
    {
        bool isSuccess = false;
        int hitIndex = IndexOf(value);
        if (hitIndex >= 0 )
        {
            RemoveAt(hitIndex);
            isSuccess = true;
        }
        return isSuccess;
    }
    
    virtual void RemoveAt(int index)
    {
        //throw gcnew NotImplementedException();
        vec.erase(vec.begin() + index);
    }
    
    virtual void CopyTo(array<T2^>^ array, int index)
    {
        int j = index;
        for (int i = 0; i < this->Count; i++)
        {
            array->SetValue(this->default[i], j);
            j++;
        }
    }

private:
    // ベクタの参照を保持する
    std::vector<T1>& vec;

};







}  // namespace DelFEM4NetCom

#endif