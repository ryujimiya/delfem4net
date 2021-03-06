﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms; // MessageBox
using DelFEM4NetCad;
using DelFEM4NetCom;
using DelFEM4NetCad.View;
using DelFEM4NetCom.View;
using DelFEM4NetFem;
using DelFEM4NetFem.Field;
using DelFEM4NetFem.Field.View;
using DelFEM4NetFem.Eqn;
using DelFEM4NetFem.Ls;
using DelFEM4NetMsh;
using DelFEM4NetMsh.View;
using DelFEM4NetMatVec;
using DelFEM4NetLsSol;
using Tao.FreeGlut;
using Tao.OpenGl;
using MyUtilLib.Matrix;
using wg2d;
using wg2d.World;

namespace wg2d
{
    class Problem01
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// 導波管ベンド
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="NormalizedFreq1"></param>
        /// <param name="NormalizedFreq2"></param>
        /// <param name="FreqDelta"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinSParameter"></param>
        /// <param name="MaxSParameter"></param>
        /// <param name="GraphSParameterInterval"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPortBcId1"></param>
        /// <param name="FieldPortBcId2"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="isCadShow"></param>
        /// <param name="CadDrawerAry"></param>
        /// <param name="Camera"></param>
        /// <returns></returns>
        public static bool SetProblem(
            int probNo,
            double WaveguideWidth,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double FreqDelta,
            ref double GraphFreqInterval,
            ref double MinSParameter,
            ref double MaxSParameter,
            ref double GraphSParameterInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
             ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            // 導波管ベンド
            // 入力部の長さ
            double inputWGLength = 1.0 * WaveguideWidth;
            double meshL = WaveguideWidth * 0.05;

            NormalizedFreq1 = 1.0;
            NormalizedFreq2 = 2.0;
            FreqDelta = 0.01;
            GraphFreqInterval = 0.2;

            MinSParameter = 0.0;
            MaxSParameter = 1.0;
            GraphSParameterInterval = 0.2;

            // 媒質リスト作成
            MediaInfo mediaVacumn = new MediaInfo
            (
                new double[3, 3]
                {
                   { 1.0/1.0, 0.0,     0.0     },
                   { 0.0,     1.0/1.0, 0.0     },
                   { 0.0,     0.0,     1.0/1.0 }
                },
                new double[3, 3]
                {
                   { 1.0, 0.0, 0.0 },
                   { 0.0, 1.0, 0.0 },
                   { 0.0, 0.0, 1.0 }
                }
            );
            Medias.Add(mediaVacumn);

            // 図面作成、メッシュ生成
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                IList<CVector2D> pts = new List<CVector2D>();
                pts.Add(new CVector2D(0.0, WaveguideWidth));  // 頂点1
                pts.Add(new CVector2D(0.0, 0.0)); // 頂点2
                pts.Add(new CVector2D(inputWGLength, 0.0)); // 頂点3
                pts.Add(new CVector2D(inputWGLength, -inputWGLength)); // 頂点4
                pts.Add(new CVector2D(inputWGLength + WaveguideWidth, -inputWGLength)); // 頂点5
                pts.Add(new CVector2D(inputWGLength + WaveguideWidth, WaveguideWidth)); // 頂点6
                cad2d.AddPolygon(pts);

                // 図面表示
                if (isCadShow)
                {
                    CadDrawerAry.Clear();
                    CadDrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                    CadDrawerAry.InitTrans(Camera);
                    return true;
                }
                /*
                 // 図面表示
                isCadShow = true;
                CadDrawerAry.Clear();
                CadDrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                CadDrawerAry.InitTrans(Camera);
                */
                /*
                // メッシュ表示
                isCadShow = true;
                CadDrawerAry.Clear();
                CadDrawerAry.PushBack(new CDrawerMsh2D(new CMesher2D(cad2d, meshL)));
                CadDrawerAry.InitTrans(Camera);
                */

                //------------------------------------------------------------------
                // メッシュ作成
                //------------------------------------------------------------------
                // メッシュを作成し、ワールド座標系にセットする
                World.Clear();
                using (CMesher2D mesher2d = new CMesher2D(cad2d, meshL))
                {
                    baseId = World.AddMesh(mesher2d);
                    conv = World.GetIDConverter(baseId);
                }
            }
            // 界の値を扱うバッファ？を生成する。
            // フィールド値IDが返却される。
            //    要素の次元: 2次元 界: 複素数スカラー 微分タイプ: 値 要素セグメント: 角節点
            FieldValId = World.MakeField_FieldElemDim(baseId, 2,
                FIELD_TYPE.ZSCALAR, FIELD_DERIVATION_TYPE.VALUE, ELSEG_TYPE.CORNER);

            FieldLoopId = 0;
            {
                // ワールド座標系のループIDを取得
                uint loopId_cad = 1;
                uint lId1 = conv.GetIdEA_fromCad(loopId_cad, CAD_ELEM_TYPE.LOOP);
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                aEA.Add(lId1);
                {
                    World.Loop loop = new World.Loop();
                    loop.Set(lId1, Medias.IndexOf(mediaVacumn));
                    LoopDic.Add(lId1, loop);
                }
                //System.Diagnostics.Debug.WriteLine("lId:" + lId1);
                FieldLoopId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldLoopId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0);
            }

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            FieldForceBcId = 0;
            {
                uint eId_cad;
                uint eId;
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                // 頂点2-頂点3
                eId_cad = 2;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                // 頂点3-頂点4
                eId_cad = 3;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                // 頂点5-頂点6
                eId_cad = 5;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                // 頂点6-頂点1
                eId_cad = 6;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                // フィールドIDを取得
                FieldForceBcId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldForceBcId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
            }
            // 開口条件1
            FieldPortBcId1 = 0;
            {
                uint eId_cad;
                uint eId;
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                // 頂点1-頂点2
                eId_cad = 1;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                {
                    World.Edge edge = new World.Edge();
                    edge.Set(eId, Medias.IndexOf(mediaVacumn));
                    EdgeDic.Add(eId, edge);
                }
                FieldPortBcId1 = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldPortBcId1, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
            }

            // 開口条件2
            FieldPortBcId2 = 0;
            {
                uint eId_cad;
                uint eId;
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                // 頂点4-頂点5
                eId_cad = 4;
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                {
                    World.Edge edge = new World.Edge();
                    edge.Set(eId, Medias.IndexOf(mediaVacumn));
                    EdgeDic.Add(eId, edge);
                }
                FieldPortBcId2 = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldPortBcId2, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
            }

            return true;
        }
    }
}
