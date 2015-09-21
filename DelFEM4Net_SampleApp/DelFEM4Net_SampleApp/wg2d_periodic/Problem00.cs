using System;
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

namespace wg2d_periodic
{
    class Problem00
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// 直線導波管
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
        /// <param name="WgPortInfoList"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="IsInoutWgSame"></param>
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
            ref IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool IsInoutWgSame,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            // 格子定数
            double latticeA = 0;
            // 周期構造距離
            double periodicDistance = 0;
            // 最小屈折率
            double minEffN = 0;
            // 最大屈折率
            double maxEffN = 0;

            // 直線導波管
            // 導波路不連続領域の長さ
            double disconLength = 4.0 * WaveguideWidth / 2.0;
            // 入出力導波路の周期構造部分の長さ
            double inputWgLength = 0.1 * WaveguideWidth / 2.0;
            // 周期構造距離
            periodicDistance = inputWgLength;
            // 格子定数（互換性の為設定)
            latticeA = 2.0 * WaveguideWidth;
            // 境界分割数
            const int ndiv = 20 * 3;
            double meshL = 1.1 * WaveguideWidth / ndiv;
            minEffN = 0.0;
            maxEffN = 1.0;

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
                // 領域追加
                pts.Add(new CVector2D(0.0, WaveguideWidth));  // 頂点1
                pts.Add(new CVector2D(0.0, 0.0)); // 頂点2
                pts.Add(new CVector2D(inputWgLength, 0.0)); // 頂点3
                pts.Add(new CVector2D(inputWgLength + disconLength, 0.0)); // 頂点4
                pts.Add(new CVector2D(inputWgLength * 2 + disconLength, 0.0)); // 頂点5
                pts.Add(new CVector2D(inputWgLength * 2 + disconLength, WaveguideWidth)); // 頂点6
                pts.Add(new CVector2D(inputWgLength + disconLength, WaveguideWidth)); // 頂点7
                pts.Add(new CVector2D(inputWgLength, WaveguideWidth)); // 頂点8
                uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 8).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(4, 7).id_e_add;
                // 入出力導波路の周期構造境界上の頂点を追加
                //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                // 入力導波路
                {
                    uint id_e = 1;
                    double x1 = 0.0;
                    double y1 = WaveguideWidth;
                    double y2 = 0.0;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x1, y2);
                }
                {
                    uint id_e = 9;
                    double x1 = inputWgLength;
                    double y1 = 0.0;
                    double y2 = WaveguideWidth;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x1, y2);
                }
                // 出力導波路
                {
                    uint id_e = 5;
                    double x1 = inputWgLength * 2 + disconLength;
                    double y1 = 0.0;
                    double y2 = WaveguideWidth;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x1, y2);
                }
                {
                    uint id_e = 10;
                    double x1 = inputWgLength + disconLength;
                    double y1 = 0.0;
                    double y2 = WaveguideWidth;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x1, y2);
                }
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
                return true;
                 */
                /*
                // メッシュ表示
                isCadShow = true;
                CadDrawerAry.Clear();
                CadDrawerAry.PushBack(new CDrawerMsh2D(new CMesher2D(cad2d, meshL)));
                CadDrawerAry.InitTrans(Camera);
                return true;
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

            // 領域
            //   ワールド座標系のループIDを取得
            //   媒質をループ単位で指定する
            FieldLoopId = 0;
            {
                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = { 1, 2, 3 };
                int[] mediaIndex_list = { Medias.IndexOf(mediaVacumn), Medias.IndexOf(mediaVacumn), Medias.IndexOf(mediaVacumn) };
                WgUtilForPeriodicEigen.GetPartialField_Loop(
                    conv,
                    World,
                    loopId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out FieldLoopId,
                    ref LoopDic);
            }

            // ２ポート情報リスト作成
            const uint portCnt = 2;
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgPortInfoList.Add(new WgUtilForPeriodicEigenExt.WgPortInfo());
                System.Diagnostics.Debug.Assert(WgPortInfoList.Count == (portIndex + 1));
                WgPortInfoList[portIndex].LatticeA = latticeA;
                WgPortInfoList[portIndex].PeriodicDistance = periodicDistance;
                WgPortInfoList[portIndex].MinEffN = minEffN;
                WgPortInfoList[portIndex].MaxEffN = maxEffN;
            }
            // 入射ポートの設定
            //   ポート1を入射ポートとする
            WgPortInfoList[0].IsIncidentPort = true;

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            {
                uint[] eId_cad_list = { 2, 3, 4, 6, 7, 8 };
                Dictionary<uint, Edge> dummyEdgeDic = null;
                WgUtilForPeriodicEigen.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    null,
                    FieldValId,
                    out FieldForceBcId,
                    ref dummyEdgeDic);
            }

            // 開口条件1
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[0];
                wgPortInfo1.FieldPortBcId = 0;

                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 1;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(10 + (ndiv - 1) - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                Dictionary<uint, Edge> workEdgeDic = new Dictionary<uint, Edge>();
                uint fieldPortBcId = 0;
                WgUtil.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldPortBcId,
                    ref workEdgeDic);
                wgPortInfo1.FieldPortBcId = fieldPortBcId;
                foreach (var pair in workEdgeDic)
                {
                    EdgeDic.Add(pair.Key, pair.Value);
                    wgPortInfo1.InputWgEdgeDic.Add(pair.Key, pair.Value);
                }
            }

            // 開口条件2
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo2 = WgPortInfoList[1];
                wgPortInfo2.FieldPortBcId = 0;

                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 5;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(10 + (ndiv - 1) * 3 - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                Dictionary<uint, Edge> workEdgeDic = new Dictionary<uint, Edge>();
                uint fieldPortBcId = 0;
                WgUtilForPeriodicEigen.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldPortBcId,
                    ref workEdgeDic);
                wgPortInfo2.FieldPortBcId = fieldPortBcId;
                foreach (var pair in workEdgeDic)
                {
                    EdgeDic.Add(pair.Key, pair.Value);
                    wgPortInfo2.InputWgEdgeDic.Add(pair.Key, pair.Value);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            // 周期構造入出力導波路1
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[0];
                wgPortInfo1.FieldInputWgLoopId = 0;

                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = { 1 };
                int[] mediaIndex_list = { Medias.IndexOf(mediaVacumn) };

                uint fieldInputWgLoopId = 0;
                WgUtilForPeriodicEigen.GetPartialField_Loop(
                    conv,
                    World,
                    loopId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldInputWgLoopId,
                    ref wgPortInfo1.InputWgLoopDic);
                wgPortInfo1.FieldInputWgLoopId = fieldInputWgLoopId;
            }
            // 周期構造境界
            //    周期構造境界は２つあり、１つは入出力ポート境界を使用。ここで指定するのは、内部側の境界)
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[0];
                wgPortInfo1.FieldInputWgBcId = 0;

                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];

                eId_cad_list[0] = 9;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(10 + (ndiv - 1) * 2 - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                uint fieldPortBcId = 0;
                WgUtilForPeriodicEigen.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldPortBcId,
                    ref wgPortInfo1.InputWgEdgeDic);
                wgPortInfo1.FieldInputWgBcId = fieldPortBcId;
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            // 周期構造入出力導波路2
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo2 = WgPortInfoList[1];
                wgPortInfo2.FieldInputWgLoopId = 0;

                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = { 3 };
                int[] mediaIndex_list = { Medias.IndexOf(mediaVacumn) };

                uint fieldInputWgLoopId = 0;
                WgUtilForPeriodicEigen.GetPartialField_Loop(
                    conv,
                    World,
                    loopId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldInputWgLoopId,
                    ref wgPortInfo2.InputWgLoopDic);
                wgPortInfo2.FieldInputWgLoopId = fieldInputWgLoopId;
            }
            // 周期構造境界
            //    周期構造境界は２つあり、１つは入出力ポート境界を使用。ここで指定するのは、内部側の境界)
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo2 = WgPortInfoList[1];
                wgPortInfo2.FieldInputWgBcId = 0;

                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];

                eId_cad_list[0] = 10;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(10 + (ndiv - 1) * 4 - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                uint fieldPortBcId = 0;
                WgUtilForPeriodicEigen.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out fieldPortBcId,
                    ref wgPortInfo2.InputWgEdgeDic);
                wgPortInfo2.FieldInputWgBcId = fieldPortBcId;
            }
            return true;
        }
    }
}
