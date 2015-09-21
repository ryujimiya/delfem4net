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

namespace wg2d_periodic_eigen
{
    class Problem02
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC欠陥導波路 2チャンネル
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="NormalizedFreq1"></param>
        /// <param name="NormalizedFreq2"></param>
        /// <param name="FreqDelta"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinBeta"></param>
        /// <param name="MaxBeta"></param>
        /// <param name="GraphBetaInterval"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="isSolveEigenItr"></param>
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
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double FreqDelta,
            ref double GraphFreqInterval,
            ref double MinBeta,
            ref double MaxBeta,
            ref double GraphBetaInterval,
            ref double minEffN,
            ref double maxEffN,
            ref double minWaveNum,
            ref double maxWaveNum,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref bool isSolveEigenItr,
            ref int CalcModeIndex,
            ref bool IsSVEA,
            ref bool IsModeTrace,
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
            // フォトニック結晶導波路(TEモード) 2チャンネル合分波
            //   問題2: 中央のロッド2本 偶モード
            //   問題-: 中央のロッド2本 奇モード
            //   問題-: 中央のロッド1本 奇モード
            //   問題-: 中央のロッド1本 偶モード
            //   Note: 中央のロッドが1本の場合、奇モードの方が基本モードになる
            IsPCWaveguide = true;
            // 反復で解く？
            //isSolveEigenItr = false; // 反復で解かない
            // 基本モードを計算する
            CalcModeIndex = 0;
            // 高次モードを計算する
            //CalcModeIndex = 1;

            // ロッドの数（外側半分）
            const int rodCntHalf = 5;
            // ロッドの数（中央)
            //int rodCntMiddle = 1;
            int rodCntMiddle = 2;
            // 格子の数
            int latticeCnt = rodCntHalf * 2 + rodCntMiddle + 2;
            // 格子定数
            latticeA = WaveguideWidth / (double)latticeCnt;
            // 周期構造距離
            periodicDistance = latticeA;
            // ロッドの半径
            double rodRadius = 0.18 * latticeA;
            // ロッドの比誘電率
            double rodEps = 3.4 * 3.4;
            // 格子１辺の分割数
            //const int ndivForOneLattice = 4;
            const int ndivForOneLattice = 6;
            // 境界の総分割数
            int ndiv = latticeCnt * ndivForOneLattice;
            // ロッドの円周の分割数
            const int rodCircleDiv = 8;
            // ロッドの半径の分割数
            const int rodRadiusDiv = 1;
            // メッシュの長さ
            //double meshL = waveguideWidth * 0.1;
            double meshL = 1.05 * WaveguideWidth / ndiv;

            // フォトニック結晶導波路の場合、a/λを規格化周波数とする
            NormalizedFreq1 = 0.300;//0.302;
            NormalizedFreq2 = 0.440;//0.443;
            FreqDelta = 0.002;
            GraphFreqInterval = 0.02;

            // 速波モードを表示
            MinBeta = 0;
            MaxBeta = 1.0; //((int)Math.Ceiling(Math.Sqrt(rodEps) * 10.0)) * 0.1 + 1.0e-12;
            GraphBetaInterval = 0.2; //0.5;
            minEffN = 0;
            maxEffN = 1.0;//Math.Sqrt(rodEps);

            // 波のモード
            WaveModeDv = WgUtil.WaveModeDV.TE;

            // 媒質リスト作成
            double claddingP = 1.0;
            double claddingQ = 1.0;
            double coreP = 1.0;
            double coreQ = 1.0;
            if (WaveModeDv == WgUtil.WaveModeDV.TM)
            {
                // TMモード
                claddingP = 1.0 / 1.0;
                claddingQ = 1.0;
                coreP = 1.0 / rodEps;
                coreQ = 1.0;
            }
            else
            {
                // TEモード
                claddingP = 1.0;
                claddingQ = 1.0;
                coreP = 1.0;
                coreQ = rodEps;
            }
            MediaInfo mediaCladding = new MediaInfo
            (
                new double[3, 3]
                        {
                           { claddingP,       0.0,       0.0 },
                           {       0.0, claddingP,       0.0 },
                           {       0.0,       0.0, claddingP }
                        },
                new double[3, 3]
                        {
                           { claddingQ,       0.0,       0.0 },
                           {       0.0, claddingQ,       0.0 },
                           {       0.0,       0.0, claddingQ }
                        }
            );
            MediaInfo mediaCore = new MediaInfo
            (
                new double[3, 3]
                        {
                           { coreP,   0.0,   0.0 },
                           {   0.0, coreP,   0.0 },
                           {   0.0,   0.0, coreP }
                        },
                new double[3, 3]
                        {
                           { coreQ,   0.0,   0.0 },
                           {   0.0, coreQ,   0.0 },
                           {   0.0,   0.0, coreQ }
                        }
            );
            Medias.Add(mediaCladding);
            Medias.Add(mediaCore);

            // 図面作成、メッシュ生成
            // Cad
            uint baseLoopId = 0;
            IList<uint> rodLoopIds = new List<uint>();
            // ワールド座標系
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                // ToDo: 周期境界1, 2上の分割が同じになるように設定する必要がある
                // 
                // 領域を追加
                {
                    List<CVector2D> pts = new List<CVector2D>();
                    pts.Add(new CVector2D(0.0, WaveguideWidth));
                    pts.Add(new CVector2D(0.0, 0.0));
                    pts.Add(new CVector2D(latticeA, 0.0));
                    pts.Add(new CVector2D(latticeA, WaveguideWidth));
                    // 多角形追加
                    uint lId = cad2d.AddPolygon(pts).id_l_add;
                    baseLoopId = lId;
                }
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
                // ロッドを追加
                for (int i = 0; i < rodCntHalf; i++)
                {
                    double x0 = latticeA * 0.5;
                    double y0 = WaveguideWidth - i * latticeA - latticeA * 0.5;
                    uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                    rodLoopIds.Add(lId);
                }
                for (int i = 0; i < rodCntMiddle; i++)
                {
                    double x0 = latticeA * 0.5;
                    double y0 = WaveguideWidth * 0.5 + latticeA * rodCntMiddle * 0.5 - i * latticeA - latticeA * 0.5;
                    uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                    rodLoopIds.Add(lId);
                }
                for (int i = 0; i < rodCntHalf; i++)
                {
                    double x0 = latticeA * 0.5;
                    double y0 = latticeA * rodCntHalf - i * latticeA - latticeA * 0.5;
                    uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                    rodLoopIds.Add(lId);
                }
                // 中央の2つのロッドとロッドの間のメッシュの対称性を改善する
                if (rodCntMiddle % 2 == 0) // 中央のロッドが偶数本のとき
                {
                    // 中央のロッドとロッドの間に頂点を追加
                    double x = latticeA * 0.5;
                    double y = WaveguideWidth * 0.5;
                    uint id_v_center = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, baseLoopId, new CVector2D(x, y)).id_v_add;
                    System.Diagnostics.Debug.Assert(id_v_center != 0);
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
                // 領域 + ロッド
                uint[] loopId_cad_list = new uint[1 + rodLoopIds.Count];
                int[] mediaIndex_list = new int[loopId_cad_list.Length];

                // 領域
                loopId_cad_list[0] = baseLoopId;
                mediaIndex_list[0] = Medias.IndexOf(mediaCladding);

                // ロッド
                int offset = 1;
                rodLoopIds.ToArray().CopyTo(loopId_cad_list, offset);
                for (int i = offset; i < mediaIndex_list.Length; i++)
                {
                    mediaIndex_list[i] = Medias.IndexOf(mediaCore);
                }
                WgUtilForPeriodicEigen.GetPartialField_Loop(
                    conv,
                    World,
                    loopId_cad_list,
                    mediaIndex_list,
                    FieldValId,
                    out FieldLoopId,
                    ref LoopDic);
            }

            // 境界条件を設定する
            // 固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            if (WaveModeDv == WgUtil.WaveModeDV.TE)
            {
                uint[] eId_cad_list = { 2, 4 };
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
            FieldPortBcId1 = 0;
            {
                uint[] eId_cad_list = new uint[ndiv];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 1;
                mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndiv - 1) - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                WgUtilForPeriodicEigen.GetPartialField_Edge(
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
                mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                for (int i = 1; i <= ndiv - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndiv - 1) * 2 - (i - 1));
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                WgUtilForPeriodicEigen.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    null,
                    FieldValId,
                    out FieldPortBcId2,
                    ref EdgeDic);
            }
            // フォトニック結晶導波路チャンネル上節点を取得する
            {
                uint[] no_c_all = null;
                Dictionary<uint, uint> to_no_loop = null;
                double[][] coord_c_all = null;
                WgUtil.GetLoopCoordList(World, FieldLoopId, out no_c_all, out to_no_loop, out coord_c_all);
                {
                    // チャンネル１
                    IList<uint> portNodes = new List<uint>();
                    for (int i = 0; i < no_c_all.Length; i++)
                    {
                        // 座標からチャンネル(欠陥部)を判定する
                        double[] coord = coord_c_all[i];
                        if (coord[1] >= (WaveguideWidth - latticeA * (rodCntHalf + 1)) && coord[1] <= (WaveguideWidth - latticeA * (rodCntHalf)))
                        {
                            portNodes.Add(no_c_all[i]);
                        }
                    }
                    PCWaveguidePorts.Add(portNodes);
                }
                {
                    // チャンネル２
                    IList<uint> portNodes = new List<uint>();
                    for (int i = 0; i < no_c_all.Length; i++)
                    {
                        // 座標からチャンネル(欠陥部)を判定する
                        double[] coord = coord_c_all[i];
                        if (coord[1] >= (latticeA * rodCntHalf) && coord[1] <= (latticeA * (rodCntHalf + 1)))
                        {
                            portNodes.Add(no_c_all[i]);
                        }
                    }
                    PCWaveguidePorts.Add(portNodes);
                }
            }
            return true;
        }
    }
}
