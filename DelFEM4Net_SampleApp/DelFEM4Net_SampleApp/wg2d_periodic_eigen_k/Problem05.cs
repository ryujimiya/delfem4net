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
    class Problem05
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// 三角形格子 PC欠陥導波路 2チャンネル
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
            // PC導波路？
            IsPCWaveguide = true;
            // フォトニック結晶導波路(三角形格子)(TEモード)
            // 基本モードを計算する
            //CalcModeIndex = 0;
            // 高次モードを指定する
            //CalcModeIndex = 3; // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 odd 1st above decoupling point
            //CalcModeIndex = 2; // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 even 1st above decoupling point
            //CalcModeIndex = 1; // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 even 1st below coupling point
            //CalcModeIndex = 1; // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 even 2nd  n = 3.4

            //CalcModeIndex = 2; // for latticeTheta = 60 r = 0.30a  n = 2.76 even 1st above & below coupling point a/lambda = 0.16 to 0.48
            //CalcModeIndex = 3; // for latticeTheta = 60 r = 0.30a  n = 2.76 odd 1st above coupling point a/lambda = 0.16 to 0.26
            CalcModeIndex = 1; // for latticeTheta = 60 r = 0.30a n = 2.76 even 1st below coupling point a/lambda = 0.28 to 0.48
            //CalcModeIndex = 0; // for latticeTheta = 60 r = 0.30a n = 2.76 odd 1st below coupling point a/lambda = 0.28 to 0.48

            // 考慮する波数ベクトルの最小値
            //double minWaveNum = 0.0;
            // 考慮する波数ベクトルの最大値
            double maxWaveNum = 0.5;
            //double maxWaveNum = 1.0; // for latticeTheta = 30 r = 0.35a air hole

            // 磁気壁を使用する？
            bool isMagneticWall = false; // 電気壁を使用する
            //bool isMagneticWall = true; // 磁気壁を使用する
            // 空孔？
            //bool isAirHole = false; // dielectric rod
            bool isAirHole = true; // air hole
            // 周期を180°ずらす
            bool isShift180 = false; // for latticeTheta = 60 r = 0.30a air hole
            //bool isShift180 = true; // for latticeTheta = 45 r = 0.18a dielectric rod
            // X方向周期の数
            //const int periodCnt = 1;
            const int periodCnt = 1;
            // ロッドの数(半分)
            //const int rodCntHalf = 5; // for latticeTheta = 60 r = 0.30a air hole
            const int rodCntHalf = 5;
            // 欠陥ロッド数
            const int defectRodCnt = 1;
            // ロッドの数（中央)
            const int rodCntMiddle = 2;
            // 三角形格子の内角
            double latticeTheta = 60.0; // for latticeTheta = 60 r = 0.30a air hole n = 3.4
            // ロッドの半径
            //double rodRadiusRatio = 0.30; // for latticeTheta = 60 r = 0.30a air hole n = 3.4
            double rodRadiusRatio = 0.30;
            // ロッドの比誘電率
            //double rodEps = 3.4 * 3.4; // for latticeTheta = 60 r = 0.30a dielectric rod
            double rodEps = 2.76 * 2.76;
            // 1格子当たりの分割点の数
            //const int ndivForOneLattice = 9; // for latticeTheta = 60 r = 0.30a
            const int ndivForOneLattice = 9;
            // ロッド円周の分割数
            //const int rodCircleDiv = 12; // for latticeTheta = 60 r = 0.30a
            const int rodCircleDiv = 12;
            // ロッドの半径の分割数
            //const int rodRadiusDiv = 4; // for latticeTheta = 60 r = 0.30a air hole
            const int rodRadiusDiv = 4;

            // ロッドが1格子を超える？
            //bool isLargeRod = (rodRadiusRatio >= 0.25);
            bool isLargeRod = (rodRadiusRatio >= 0.5 * Math.Sin(latticeTheta * pi / 180.0));
            // 格子の数
            int latticeCnt = rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle;
            // ロッド間の距離(Y方向)
            double rodDistanceY = WaveguideWidth / (double)latticeCnt;
            if (isLargeRod)
            {
                rodDistanceY = WaveguideWidth / (double)(latticeCnt - 1);
            }
            // 格子定数
            latticeA = rodDistanceY / Math.Sin(latticeTheta * pi / 180.0);
            // ロッド間の距離(X方向)
            double rodDistanceX = rodDistanceY * 2.0 / Math.Tan(latticeTheta * pi / 180.0);
            // 周期構造距離
            periodicDistance = rodDistanceX;
            // ロッドの半径
            double rodRadius = rodRadiusRatio * latticeA;
            // メッシュのサイズ
            double meshL = 1.05 * WaveguideWidth / (latticeCnt * ndivForOneLattice);

            Beta1 = 0.0;
            Beta2 = maxWaveNum * (2.0 * pi / periodicDistance) + 1.0e-06;
            BetaDelta = 0.02 * (2.0 * pi / periodicDistance);
            //BetaDelta = 0.01 * (2.0 * pi / periodicDistance);
            GraphBetaInterval = 0.1 * (2.0 * pi / periodicDistance);

            // フォトニック結晶導波路の場合、a/λを規格化周波数とする
            if (Math.Abs(latticeTheta - 60.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                    || Math.Abs(latticeTheta - 30.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // for latticeTheta = 60 r = 0.30a air hole n = 3.4
                //MinNormalizedFreq = 0.210;
                //MaxNormalizedFreq = 0.2801;
                //GraphFreqInterval = 0.01;
                // for latticeTheta = 60 r = 0.30a air hole n = 2.76
                MinNormalizedFreq = 0.260;
                MaxNormalizedFreq = 0.330;
                GraphFreqInterval = 0.01;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            // 波のモード
            //WaveModeDv = WgUtil.WaveModeDV.TE; // dielectric rod
            if (isAirHole)
            {
                WaveModeDv = WgUtil.WaveModeDV.TM; // air hole
            }
            else
            {
                WaveModeDv = WgUtil.WaveModeDV.TE; // dielectric rod
                isMagneticWall = false;
            }

            // 媒質リスト作成
            double claddingP = 1.0;
            double claddingQ = 1.0;
            double coreP = 1.0;
            double coreQ = 1.0;
            if (isAirHole)
            {
                // 誘電体基盤 + 空孔(air hole)
                if (WaveModeDv == WgUtil.WaveModeDV.TM)
                {
                    // TMモード
                    claddingP = 1.0 / rodEps;
                    claddingQ = 1.0;
                    coreP = 1.0 / 1.0;
                    coreQ = 1.0;
                }
                else
                {
                    // TEモード
                    claddingP = 1.0;
                    claddingQ = rodEps;
                    coreP = 1.0;
                    coreQ = 1.0;
                }
            }
            else
            {
                // 誘電体ロッド(dielectric rod)
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
            int ndivPlus = 0;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_F1 = new List<uint>();
            IList<uint> id_e_F2 = new List<uint>();
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
                    pts.Add(new CVector2D(rodDistanceX * periodCnt, 0.0));
                    pts.Add(new CVector2D(rodDistanceX * periodCnt, WaveguideWidth));
                    // 多角形追加
                    uint lId = cad2d.AddPolygon(pts).id_l_add;
                    baseLoopId = lId;
                }
                // 入出力導波路の周期構造境界上の頂点を追加
                IList<double> ys = new List<double>();
                IList<double> ys_rod = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                int outofAreaRodPtCnt_row_top = 0;
                int outofAreaRodPtCnt_row_bottom = 0;
                // 境界上にロッドのある格子
                // 境界上のロッドの頂点
                for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                {
                    if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                    if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部
                    if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
                    double y0 = WaveguideWidth - i * rodDistanceY - 0.5 * rodDistanceY;
                    if (isLargeRod)
                    {
                        y0 += 0.5 * rodDistanceY;
                    }
                    if (y0 > (0.0 + Constants.PrecisionLowerLimit) && y0 < (WaveguideWidth - Constants.PrecisionLowerLimit))
                    {
                        ys_rod.Add(y0);
                    }
                    else
                    {
                        if (isLargeRod && i == 0)
                        {
                            outofAreaRodPtCnt_row_top++;
                        }
                        else if (isLargeRod && i == (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle - 1))
                        {
                            outofAreaRodPtCnt_row_bottom++;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    for (int k = 1; k <= rodRadiusDiv; k++)
                    {
                        double y1 = y0 - k * rodRadius / rodRadiusDiv;
                        double y2 = y0 + k * rodRadius / rodRadiusDiv;
                        if (y1 > (0.0 + Constants.PrecisionLowerLimit) && y1 < (WaveguideWidth - Constants.PrecisionLowerLimit))
                        {
                            ys_rod.Add(y1);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        if (y2 > (0.0 + Constants.PrecisionLowerLimit) && y2 < (WaveguideWidth - Constants.PrecisionLowerLimit))
                        {
                            ys_rod.Add(y2);
                        }
                        else
                        {
                            if (isLargeRod && i == 0)
                            {
                                outofAreaRodPtCnt_row_top++;
                            }
                            else if (isLargeRod && i == (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle - 1))
                            {
                                outofAreaRodPtCnt_row_bottom++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                    }
                }
                foreach (double y_rod in ys_rod)
                {
                    ys.Add(y_rod);
                }
                // 境界上のロッドの外の頂点はロッドから少し離さないとロッドの追加で失敗するのでマージンをとる
                double radiusMargin = rodDistanceY * 0.01;
                // 境界上にロッドのある格子
                // ロッドの外
                for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                {
                    if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                    if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部
                    if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
                    for (int k = 1; k <= (ndivForOneLattice - 1); k++)
                    {
                        double y_divpt = WaveguideWidth - i * rodDistanceY - k * (rodDistanceY / ndivForOneLattice);
                        double y_min_rod = WaveguideWidth - i * rodDistanceY - 0.5 * rodDistanceY - rodRadius - radiusMargin;
                        double y_max_rod = WaveguideWidth - i * rodDistanceY - 0.5 * rodDistanceY + rodRadius + radiusMargin;
                        if (isLargeRod)
                        {
                            y_divpt += rodDistanceY * 0.5;
                            if (y_divpt >= (WaveguideWidth - Constants.PrecisionLowerLimit)) continue;
                            y_min_rod += rodDistanceY * 0.5;
                            y_max_rod += rodDistanceY * 0.5;
                        }
                        if (y_divpt < (y_min_rod - Constants.PrecisionLowerLimit) || y_divpt > (y_max_rod + Constants.PrecisionLowerLimit))
                        {
                            ys.Add(y_divpt);
                        }
                    }
                }
                
                // 境界上にロッドのない格子
                for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                {
                    if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                    if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部
                    if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1)) continue;
                    for (int k = 0; k <= ndivForOneLattice; k++)
                    {
                        if (i == 0 && k == 0) continue;
                        if (i == (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle - 1) && k == ndivForOneLattice) continue;
                        double y_divpt = WaveguideWidth - i * rodDistanceY - k * (rodDistanceY / ndivForOneLattice);
                        double y_min_upper_rod = WaveguideWidth - i * rodDistanceY + 0.5 * rodDistanceY - rodRadius - radiusMargin;
                        double y_max_lower_rod = WaveguideWidth - (i + 1) * rodDistanceY - 0.5 * rodDistanceY + rodRadius + radiusMargin;
                        if (isLargeRod)
                        {
                            y_divpt += rodDistanceY * 0.5;
                            if (y_divpt >= (WaveguideWidth - Constants.PrecisionLowerLimit)) continue;
                            y_min_upper_rod += rodDistanceY * 0.5;
                            y_max_lower_rod += rodDistanceY * 0.5;
                        }
                        bool isAddHalfRod_row_top = (isLargeRod
                            && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))));
                        if ((i != 0 || (i == 0 && isAddHalfRod_row_top))
                                && y_divpt >= (y_min_upper_rod - Constants.PrecisionLowerLimit))
                        {
                            continue;
                        }
                        if ((isShift180 || (!isShift180 && i != (rodCntHalf - 1)))
                            && y_divpt <= (y_max_lower_rod + Constants.PrecisionLowerLimit))
                        {
                            continue;
                        }

                        ys.Add(y_divpt);
                    }
                }
                
                // 欠陥部
                const int channelCnt = 2;
                System.Diagnostics.Debug.Assert(rodCntMiddle % 2 == 0);
                for (int channelIndex = 0; channelIndex < channelCnt; channelIndex++)
                {
                    for (int i = 0; i <= (defectRodCnt * ndivForOneLattice); i++)
                    {
                        if (channelIndex == 0)
                        {
                            if (!isShift180 && (i == 0 || i == (defectRodCnt * ndivForOneLattice))) continue;
                        }
                        else if (channelIndex == 1)
                        {
                            if (rodCntMiddle % 2 == 0)
                            {
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                        double y_ofs = 0.0;

                        if (channelIndex == 1)
                        {
                            y_ofs = rodDistanceY * (defectRodCnt + rodCntMiddle);
                        }
                        double y_divpt = WaveguideWidth - rodDistanceY * rodCntHalf - i * (rodDistanceY / ndivForOneLattice) - y_ofs;
                        double y_min_upper_rod = WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) + 0.5 * rodDistanceY - rodRadius - radiusMargin - y_ofs;
                        double y_max_lower_rod = WaveguideWidth - rodDistanceY * rodCntHalf - 0.5 * rodDistanceY + rodRadius + radiusMargin - y_ofs;
                        if (isLargeRod)
                        {
                            y_divpt -= rodDistanceY * 0.5;
                            y_min_upper_rod -= rodDistanceY * 0.5;
                            y_max_lower_rod -= rodDistanceY * 0.5;
                        }
                        if (isLargeRod && isShift180)
                        {
                            // for isLargeRod == true
                            if (y_divpt >= (y_min_upper_rod - Constants.PrecisionLowerLimit)
                                    || y_divpt <= (y_max_lower_rod + Constants.PrecisionLowerLimit)
                                )
                            {
                                continue;
                            }
                        }
                        ys.Add(y_divpt);
                    }
                }

                // 昇順でソート
                double[] yAry = ys.ToArray();
                Array.Sort(yAry);
                ndivPlus = yAry.Length + 1;

                // yAryは昇順なので、yAryの並びの順に追加すると境界1上を逆方向に移動することになる
                //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                bool isInRod = false;
                if (isLargeRod
                    && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                {
                    isInRod = true;
                }
                for (int i = 0; i < yAry.Length; i++)
                {
                    uint id_e = 1;
                    CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(0.0, yAry[i]));
                    uint id_v_add = resAddVertex.id_v_add;
                    uint id_e_add = resAddVertex.id_e_add;
                    System.Diagnostics.Debug.Assert(id_v_add != 0);
                    System.Diagnostics.Debug.Assert(id_e_add != 0);
                    if (isInRod)
                    {
                        id_e_rod_B1.Add(id_e_add);
                    }
                    bool contains = false;
                    foreach (double y_rod in ys_rod)
                    {
                        if (Math.Abs(y_rod - yAry[i]) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        id_v_list_rod_B1.Add(id_v_add);

                        if (isLargeRod
                            && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                        {
                            if ((id_v_list_rod_B1.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 1)
                            {
                                isInRod = true;
                            }
                            else if ((id_v_list_rod_B1.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 0)
                            {
                                isInRod = false;
                            }
                        }
                        else
                        {
                            if (id_v_list_rod_B1.Count % (rodRadiusDiv * 2 + 1) == 1)
                            {
                                isInRod = true;
                            }
                            else if (id_v_list_rod_B1.Count % (rodRadiusDiv * 2 + 1) == 0)
                            {
                                isInRod = false;
                            }
                        }
                    }
                    if (isLargeRod
                        && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                    {
                        if (i == (yAry.Length - 1))
                        {
                            System.Diagnostics.Debug.Assert(isInRod == true);
                            id_e_rod_B1.Add(id_e);
                        }
                    }
                }

                isInRod = false;
                if (isLargeRod
                    && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                {
                    isInRod = true;
                }
                for (int i = yAry.Length - 1; i >= 0; i--)
                {
                    uint id_e = 3;
                    CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(rodDistanceX * periodCnt, yAry[i]));
                    uint id_v_add = resAddVertex.id_v_add;
                    uint id_e_add = resAddVertex.id_e_add;
                    System.Diagnostics.Debug.Assert(id_v_add != 0);
                    System.Diagnostics.Debug.Assert(id_e_add != 0);
                    if (isInRod)
                    {
                        id_e_rod_B2.Add(id_e_add);
                    }
                    bool contains = false;
                    foreach (double y_rod in ys_rod)
                    {
                        if (Math.Abs(y_rod - yAry[i]) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        id_v_list_rod_B2.Add(id_v_add);
                        if (isLargeRod
                            && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                        {
                            if ((id_v_list_rod_B2.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 1)
                            {
                                isInRod = true;
                            }
                            else if ((id_v_list_rod_B2.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 0)
                            {
                                isInRod = false;
                            }
                        }
                        else
                        {
                            if (id_v_list_rod_B2.Count % (rodRadiusDiv * 2 + 1) == 1)
                            {
                                isInRod = true;
                            }
                            else if (id_v_list_rod_B2.Count % (rodRadiusDiv * 2 + 1) == 0)
                            {
                                isInRod = false;
                            }
                        }
                    }
                    if (isLargeRod
                        && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                    {
                        if (i == 0)
                        {
                            System.Diagnostics.Debug.Assert(isInRod == true);
                            id_e_rod_B2.Add(id_e);
                        }
                    }
                }

                int bRodCntHalf_Top = (isShift180 ? (int)((rodCntHalf + 1) / 2) : (int)((rodCntHalf) / 2));
                int bRodCntHalf_Bottom = 0;
                if (rodCntMiddle % 2 == 0)
                {
                    bRodCntHalf_Bottom = (int)((rodCntHalf + 1) / 2);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                int bRodCntMiddle = (rodCntMiddle) / 2;
                if (!isLargeRod
                    || (isLargeRod &&
                           (isShift180 && (rodCntHalf % 2 == 0)) || (!isShift180 && (rodCntHalf % 2 == 1))
                       )
                    )
                {
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == (bRodCntHalf_Top + bRodCntHalf_Bottom + bRodCntMiddle) * (rodRadiusDiv * 2 + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == (bRodCntHalf_Top + bRodCntHalf_Bottom + bRodCntMiddle) * (rodRadiusDiv * 2 + 1));
                }
                else
                {
                    // 未サポート
                    System.Diagnostics.Debug.Assert(false);
                    /*
                    System.Diagnostics.Debug.Assert(outofAreaRodPtCnt_row_top == (rodRadiusDiv + 1));
                    System.Diagnostics.Debug.Assert(outofAreaRodPtCnt_row_bottom == (rodRadiusDiv + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                     */
                }

                /////////////////////////////////////////////////////////////////////////////
                // ロッドを追加
                uint id_v_B1_top_rod_center = 1;
                //uint id_v_B1_bottom_rod_center = 2;
                uint id_v_B2_top_rod_center = 4;
                //uint id_v_B2_bottom_rod_center = 3;

                // 左右のロッド(上下の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1_rodQuarter = new List<uint>();
                IList<uint> id_v_list_F2_rodQuarter = new List<uint>();

                // ロッドを追加
                // 左のロッド
                for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                {
                    if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                    if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部
                    if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                    {
                        int i2 = 0;
                        if (i >= 0 && i < rodCntHalf)
                        {
                            i2 = bRodCntHalf_Top - 1 - (int)((rodCntHalf - 1 - i) / 2);
                        }
                        else if (i >= (rodCntHalf + defectRodCnt) && i < (rodCntHalf + defectRodCnt + rodCntMiddle))
                        {
                            i2 = bRodCntHalf_Top + bRodCntMiddle - 1 - (int)((rodCntHalf + defectRodCnt + rodCntMiddle - 1 - i) / 2);
                        }
                        else if (i >= (rodCntHalf + defectRodCnt * 2 + rodCntMiddle) && i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle))
                        {
                            i2 = bRodCntHalf_Top + bRodCntMiddle + bRodCntHalf_Bottom - 1 - (int)((rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle - 1 - i) / 2);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        int ofs_index_left = 0;
                        if (isLargeRod && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                        {
                            ofs_index_left = -outofAreaRodPtCnt_row_top;
                        }
                        bool isQuarterRod = false;
                        // 左のロッド
                        {
                            uint id_v0 = 0;
                            uint id_v1 = 0;
                            uint id_v2 = 0;
                            int index_v0 = (id_v_list_rod_B1.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                            int index_v1 = (id_v_list_rod_B1.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                            int index_v2 = (id_v_list_rod_B1.Count - 1 - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                            if (index_v2 > id_v_list_rod_B1.Count - 1)
                            {
                                isQuarterRod = true;
                                id_v0 = id_v_list_rod_B1[index_v0];
                                //id_v1 = id_v_list_rod_B1[index_v1];
                                id_v1 = id_v_B1_top_rod_center;
                                //id_v2 = id_v_list_rod_B1[work_id_v_list_rod_B.Count - 1];
                                id_v2 = id_v_list_F1_rodQuarter[0]; // 1つ飛ばしで参照;
                            }
                            else
                            {
                                id_v0 = id_v_list_rod_B1[index_v0];
                                id_v1 = id_v_list_rod_B1[index_v1];
                                id_v2 = id_v_list_rod_B1[index_v2];
                            }

                            double x0 = 0.0;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                            if (isLargeRod)
                            {
                                y0 += rodDistanceY * 0.5;
                            }
                            uint lId = 0;
                            if (isQuarterRod)
                            {
                                // 1/4円を追加する
                                lId = WgCadUtil.AddExactlyQuarterRod(
                                    cad2d,
                                    baseLoopId,
                                    x0,
                                    y0,
                                    rodRadius,
                                    rodCircleDiv,
                                    rodRadiusDiv,
                                    id_v2,
                                    id_v1,
                                    id_v0,
                                    0.0,
                                    true);
                            }
                            else
                            {
                                // 左のロッド
                                lId = WgCadUtil.AddLeftRod(
                                    cad2d,
                                    baseLoopId,
                                    id_v0,
                                    id_v1,
                                    id_v2,
                                    x0,
                                    y0,
                                    rodRadius,
                                    rodCircleDiv,
                                    rodRadiusDiv);
                            }
                            rodLoopIds.Add(lId);
                        }
                    }
                }
                // 右のロッド
                for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                {
                    if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                    if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部
                    if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                    {
                        int i2 = 0;
                        if (i >= 0 && i < rodCntHalf)
                        {
                            i2 = bRodCntHalf_Top - 1 - (int)((rodCntHalf - 1 - i) / 2);
                        }
                        else if (i >= (rodCntHalf + defectRodCnt) && i < (rodCntHalf + defectRodCnt + rodCntMiddle))
                        {
                            i2 = bRodCntHalf_Top + bRodCntMiddle - 1 - (int)((rodCntHalf + defectRodCnt + rodCntMiddle - 1 - i) / 2);
                        }
                        else if (i >= (rodCntHalf + defectRodCnt * 2 + rodCntMiddle) && i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle))
                        {
                            i2 = bRodCntHalf_Top + bRodCntMiddle + bRodCntHalf_Bottom - 1 - (int)((rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle - 1 - i) / 2);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        int ofs_index_top = 0;
                        if (isLargeRod && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                        {
                            ofs_index_top = -outofAreaRodPtCnt_row_top;
                        }
                        bool isQuarterRod = false;
                        
                        // 右のロッド
                        {
                            uint id_v0 = 0;
                            uint id_v1 = 0;
                            uint id_v2 = 0;
                            int index_v0 = (0 + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                            int index_v1 = ((rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                            int index_v2 = ((rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                            if (index_v0 < 0)
                            {
                                isQuarterRod = true;
                                //id_v0 = work_id_v_list_rod_B[0]; // DEBUG
                                id_v0 = id_v_list_F1_rodQuarter[1];
                                //id_v1 = id_v_list_rod_B2[index_v1];
                                id_v1 = id_v_B2_top_rod_center;
                                id_v2 = id_v_list_rod_B2[index_v2];
                            }
                            else
                            {
                                id_v0 = id_v_list_rod_B2[index_v0];
                                id_v1 = id_v_list_rod_B2[index_v1];
                                id_v2 = id_v_list_rod_B2[index_v2];
                            }

                            double x0 = rodDistanceX * periodCnt;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                            if (isLargeRod)
                            {
                                y0 += rodDistanceY * 0.5;
                            }
                            CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
                            uint lId = 0;
                            if (isQuarterRod)
                            {
                                // 1/4円を追加する
                                lId = WgCadUtil.AddExactlyQuarterRod(
                                    cad2d,
                                    baseLoopId,
                                    x0,
                                    y0,
                                    rodRadius,
                                    rodCircleDiv,
                                    rodRadiusDiv,
                                    id_v2,
                                    id_v1,
                                    id_v0,
                                    270.0,
                                    true);
                            }
                            else
                            {
                                // 右のロッド
                                lId = WgCadUtil.AddRightRod(
                                    cad2d,
                                    baseLoopId,
                                    id_v0,
                                    id_v1,
                                    id_v2,
                                    x0,
                                    y0,
                                    rodRadius,
                                    rodCircleDiv,
                                    rodRadiusDiv);
                            }
                            rodLoopIds.Add(lId);
                        }
                    }
                }

                // 中央のロッド(上下の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1 = new List<uint>();
                IList<uint> id_v_list_F2 = new List<uint>();

                // 中央のロッド
                for (int col = 1; col <= periodCnt * 2 - 1; col++)
                {
                    // 中央のロッド
                    for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle); i++)
                    {
                        if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // 上側導波路部
                        if (i >= (rodCntHalf + defectRodCnt + rodCntMiddle) && i < (rodCntHalf + defectRodCnt + rodCntMiddle + defectRodCnt)) continue; // 下側導波路部

                        if ((col % 2 == 1 && (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                            if (isLargeRod)
                            {
                                y0 += rodDistanceY * 0.5; // for isLargeRod
                            }
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                            rodLoopIds.Add(lId);
                        }
                    }
                }

                //isCadShow = true;
                // 図面表示
                if (isCadShow)
                {
                    // check
                    // ロッドを色付けする
                    foreach (uint lIdRod in rodLoopIds)
                    {
                        cad2d.SetColor_Loop(lIdRod, new double[] { 0.0, 0.0, 1.0 });
                    }
                    // 境界上のロッドの辺に色を付ける
                    foreach (uint eId in id_e_rod_B1)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B2)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }

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
            // 固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            if ((WaveModeDv == WgUtil.WaveModeDV.TE && !isMagneticWall)  // TEモードで電気壁
                || (WaveModeDv == WgUtil.WaveModeDV.TM && isMagneticWall) // TMモードで磁気壁
                )
            {
                uint[] eId_cad_list = new uint[2 + id_e_F1.Count + id_e_F2.Count];
                eId_cad_list[0] = 2;
                eId_cad_list[1] = 4;
                for (int i = 0; i < id_e_F1.Count; i++)
                {
                    eId_cad_list[2 + i] = id_e_F1[i];
                }
                for (int i = 0; i < id_e_F2.Count; i++)
                {
                    eId_cad_list[2 + id_e_F1.Count + i] = id_e_F2[i];
                }
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
                uint[] eId_cad_list = new uint[ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 1;
                if (id_e_rod_B1.Contains(eId_cad_list[0]))
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCore);
                }
                else
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 1; i <= ndivPlus - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndivPlus - 1) - (i - 1));
                    if (id_e_rod_B1.Contains(eId_cad_list[i]))
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                    }
                }
                WgUtilForPeriodicEigenBetaSpecified.GetPartialField_Edge(
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
                uint[] eId_cad_list = new uint[ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                eId_cad_list[0] = 3;
                if (id_e_rod_B2.Contains(eId_cad_list[0]))
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCore);
                }
                else
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 1; i <= ndivPlus - 1; i++)
                {
                    eId_cad_list[i] = (uint)(4 + (ndivPlus - 1) * 2 - (i - 1));
                    if (id_e_rod_B2.Contains(eId_cad_list[i]))
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                    }
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
                        //if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt)) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf))
                        //if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - (0.5 * rodDistanceY - rodRadius)) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + (0.5 * rodDistanceY - rodRadius))) // dielectric rod
                        if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)) // air hole
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
                        if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt * 2 + rodCntMiddle) - 1.0 * rodDistanceY) && coord[1] <= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt + rodCntMiddle) + 1.0 * rodDistanceY)) // air hole
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
