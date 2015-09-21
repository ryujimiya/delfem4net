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

namespace wg2d_periodic_eigen_k
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
        /// <param name="Beta1"></param>
        /// <param name="Beta2"></param>
        /// <param name="BetaDelta"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
        /// <param name="GraphBetaInterval"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="CalcModeIndex"></param>
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
            ref double Beta1,
            ref double Beta2,
            ref double BetaDelta,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref double GraphBetaInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref int CalcModeIndex,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            // 直線導波管
            // 導波管不連続領域の長さ
            double disconLength = WaveguideWidth * 0.05;
            // 周期構造距離
            periodicDistance = disconLength;
            // 格子定数（互換性の為設定)
            latticeA = 2.0 * WaveguideWidth;
            // 境界分割数
            const int ndiv = 20 * 3;
            // メッシュの長さ
            double meshL = 1.1 * WaveguideWidth / ndiv;
            // 波のモード
            WaveModeDv = WgUtil.WaveModeDV.TE;

            Beta1 = 0.0;
            Beta2 = 6.0;
            BetaDelta = 0.1;
            GraphBetaInterval = 2.0;

            MinNormalizedFreq = 0.0;
            MaxNormalizedFreq = 2.0;
            GraphFreqInterval = 0.2;

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
                // ToDo: 周期境界1, 2上の分割が同じになるように設定する必要がある
                // 
                IList<CVector2D> pts = new List<CVector2D>();
                pts.Add(new CVector2D(0, WaveguideWidth));
                pts.Add(new CVector2D(0.0, 0));
                pts.Add(new CVector2D(disconLength, 0.0));
                pts.Add(new CVector2D(disconLength, WaveguideWidth));
                cad2d.AddPolygon(pts);
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
                // 出力導波路
                {
                    uint id_e = 3;
                    double x1 = latticeA;
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

            // 領域
            //   ワールド座標系のループIDを取得
            //   媒質をループ単位で指定する
            FieldLoopId = 0;
            {
                // 領域
                uint[] loopId_cad_list = { 1 };
                int[] mediaIndex_list = { Medias.IndexOf(mediaVacumn) };
                WgUtilForPeriodicEigenBetaSpecified.GetPartialField_Loop(
                    conv,
                    World,
                    loopId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out FieldLoopId,
                    ref LoopDic);
            }

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            {
                uint[] eId_cad_list = { 2, 4 };
                Dictionary<uint, Edge> dummyEdgeDic = null;
                WgUtilForPeriodicEigenBetaSpecified.GetPartialField_Edge(
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
            FieldPortBcId1 = 0;
            {
                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 1;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndiv - 1) - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                WgUtil.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    null,
                    FieldValId,
                    out FieldPortBcId1,
                    ref EdgeDic);
            }

            // 開口条件2
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            FieldPortBcId2 = 0;
            {
                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 3;
                mediaIndex_list[0] = Medias.IndexOf(mediaVacumn);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndiv - 1) * 2 - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                WgUtilForPeriodicEigenBetaSpecified.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    null,
                    FieldValId,
                    out FieldPortBcId2,
                    ref EdgeDic);
            }
            return true;
        }
    }
}
