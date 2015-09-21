/*! @file
@brief Visual C++ 2010でスタティックライブラリを作成する際のコンパイルエラー除去のために追加したファイルです(プリプロセッサに__VISUALC__, _CRT_SECURE_NO_WARNINGS[fopen等のwarning抑止]を追加してください)
@author Nobuyuki Umetani
*/

#if !defined(VISUALC_STUB_H)
#define VISUALC_STUB_H


#if defined(__VISUALC__)
    #pragma warning( disable : 4786 )
    #pragma warning ( disable : 4996 )
    #pragma warning ( disable : 4018 ) 
#endif
/* warning C4018: '<' : signed と unsigned の数値を比較しようとしました。*/

#if defined(__VISUALC__)
// 下記エラー回避のために追加
//! error C2668: 'abs' : オーバーロード関数の呼び出しを解決することができません。
unsigned int abs(unsigned int x);
#endif

#endif
