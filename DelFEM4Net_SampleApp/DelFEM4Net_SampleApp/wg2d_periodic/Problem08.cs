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
    class Problem08
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC導波路 三角形格子 直線
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
            // 入出力導波路が同じ？
            //IsInoutWgSame = false;// true;

            // 固有値を反復で解く？
            //bool isSolveEigenItr = true; //単一モードのとき反復で解く
            bool isSolveEigenItr = false; // 反復で解かない
            // 解く伝搬モードの数
            //int propModeCntToSolve = 1;
            int propModeCntToSolve = 3;
            // 緩慢変化包絡線近似？
            //bool isSVEA = true;  // Φ = φexp(-jβx)と置く
            bool isSVEA = false; // Φを直接解く

            // 入射モードインデックス
            // 基本モード入射
            int incidentModeIndex = 0;
            // 高次モード入射
            //int incidentModeIndex = 1;

            // 格子定数
            double latticeA = 0;
            // 周期構造距離
            double periodicDistance = 0;
            // 最小屈折率
            double minEffN = 0;
            // 最大屈折率
            double maxEffN = 0;

            // 考慮する波数ベクトルの最小値
            //double minWaveNum = 0.0; // for latticeTheta = 45 r = 0.18a
            double minWaveNum = 0.5; // for latticeTheta = 60 r = 0.30a
            // 考慮する波数ベクトルの最大値
            //double maxWaveNum = 0.5; // for latticeTheta = 45 r = 0.18a
            double maxWaveNum = 1.0; // for latticeTheta = 60 r = 0.30a

            // 磁気壁を使用する？
            bool isMagneticWall = false; // 電気壁を使用する
            //bool isMagneticWall = true; // 磁気壁を使用する
            // 空孔？
            //bool isAirHole = false; // dielectric rod
            bool isAirHole = true; // air hole
            // 周期を180°ずらす
            bool isShift180 = false; // for latticeTheta = 30 r = 0.35a air hole
            //bool isShift180 = true; // for latticeTheta = 45 r = 0.18a dielectric rod
            // ロッドの数(半分)
            //const int rodCntHalf = 5; // for latticeTheta = 45 r = 0.18a dielectric rod
            //const int rodCntHalf = 5; // for latticeTheta = 60 r = 0.30a air hole
            //const int rodCntHalf = 6; // for latticeTheta = 30 r = 0.30a air hole
            const int rodCntHalf = 3;// 5;
            // 欠陥ロッド数
            //const int defectRodCnt = 3; // for latticeTheta = 45 r = 0.18a dielectric rod
            const int defectRodCnt = 1;
            // 三角形格子の内角
            //double latticeTheta = 45.0; // for latticeTheta = 45 r = 0.18a dielectric rod
            double latticeTheta = 60.0; // for latticeTheta = 60 r = 0.30a air hole
            //double latticeTheta = 30.0; // for latticeTheta = 30 r = 0.30a air hole n = 3.4
            // ロッドの半径
            //double rodRadiusRatio = 0.18;  // for latticeTheta = 45 r = 0.18a dielectric rod
            //double rodRadiusRatio = 0.30; // for latticeTheta = 60 r = 0.30a air hole
            //double rodRadiusRatio = 0.35; // for latticeTheta = 60 r = 0.35a air hole
            //double rodRadiusRatio = 0.36; // for latticeTheta = 60 r = 0.36a air hole
            //double rodRadiusRatio = 0.29; // for latticeTheta = 60 r = 0.29a air hole
            //double rodRadiusRatio = 0.30; // for latticeTheta = 30 r = 0.30a air hole n = 3.4
            double rodRadiusRatio = 0.32;
            // ロッドの比誘電率
            //double rodEps = 3.4 * 3.4; // for latticeTheta = 45 r = 0.18a dielectric rod
            //double rodEps = 4.3 * 4.3; // for latticeTheta = 45 r = 0.18a dielectric rod
            //double rodEps = 2.76 * 2.76; // for latticeTheta = 60 r = 0.30a air hole
            //double rodEps = 2.8 * 2.8; // for latticeTheta = 60 r = 0.35a air hole
            //double rodEps = 3.32 * 3.32; // for latticeTheta = 60 r = 0.36a air hole
            //double rodEps = 2.76 * 2.76; // for latticeTheta = 60 r = 0.29a air hole
            //double rodEps = 3.4 * 3.4; // for latticeTheta = 30 r = 0.30a air hole n = 3.4
            double rodEps = 3.476 * 3.476;
            // 1格子当たりの分割点の数
            //const int ndivForOneLattice = 7; // for latticeTheta = 45 r = 0.18a dielectric rod
            //const int ndivForOneLattice = 5; // for latticeTheta = 45 r = 0.18a dielectric rod rotEps = 4.3
            //const int ndivForOneLattice = 10; // for latticeTheta = 60 r = 0.30a air hole
            //const int ndivForOneLattice = 9; // for latticeTheta = 60 r = 0.35a air hole
            //const int ndivForOneLattice = 6; // for latticeTheta = 30 r = 0.35a air hole
            //const int ndivForOneLattice = 9; // for latticeTheta = 60 r = 0.36a air hole
            //const int ndivForOneLattice = 6; // for latticeTheta = 30 r = 0.30a air hole n = 3.4
            const int ndivForOneLattice = 8;// 9;
            // ロッド円周の分割数
            //const int rodCircleDiv = 8; // for latticeTheta = 45 r = 0.18a dielectric rod
            //const int rodCircleDiv = 12; // for latticeTheta = 60 r = 0.30a
            const int rodCircleDiv = 12;
            // ロッドの半径の分割数
            //const int rodRadiusDiv = 2; // for latticeTheta = 45 r = 0.18a dielectric rod
            //const int rodRadiusDiv = 4; // for latticeTheta = 60 r = 0.30a air hole
            const int rodRadiusDiv = 4;
            // 導波路不連続領域の長さ
            //const int rodCntDiscon = 4;
            const int rodCntDiscon = 4;

            // ロッドが1格子を超える？
            //bool isLargeRod = (rodRadiusRatio >= 0.25);
            bool isLargeRod = (rodRadiusRatio >= 0.5 * Math.Sin(latticeTheta * pi / 180.0));
            // 格子の数
            int latticeCnt = rodCntHalf * 2 + defectRodCnt;
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
            // 導波路不連続領域の長さ
            double disconLength = rodDistanceX * rodCntDiscon;
            // 入出力導波路の周期構造部分の長さ
            double inputWgLength = rodDistanceX;
            // メッシュのサイズ
            double meshL = 1.05 * WaveguideWidth / (latticeCnt * ndivForOneLattice);

            if (Math.Abs(latticeTheta - 45.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // for latticeTheta = 45 r = 0.18a dielectric rod  n = 3.6
                //  even 1st
                //NormalizedFreq1 = 0.320;//0.320;
                //NormalizedFreq2 = 0.3601;//0.380;
                //FreqDelta = 0.002;
                //GraphFreqInterval = 0.01;
                //  odd
                //NormalizedFreq1 = 0.384;
                //NormalizedFreq2 = 0.432;
                //FreqDelta = 0.002;//0.001;
                //GraphFreqInterval = 0.01;
                //  even 2nd
                //NormalizedFreq1 = 0.420;
                //NormalizedFreq2 = 0.500;
                //FreqDelta = 0.002;
                //GraphFreqInterval = 0.01;
                // for latticeTheta = 45 r = 0.18a dielectric rod  n = 4.3
                NormalizedFreq1 = 0.260;
                NormalizedFreq2 = 0.3301;
                FreqDelta = 0.002;
                GraphFreqInterval = 0.01;
            }
            else if (Math.Abs(latticeTheta - 60.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                    || Math.Abs(latticeTheta - 30.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // for latticeTheta = 60 r = 0.30a air hole
                //NormalizedFreq1 = 0.267; //0.270;
                //NormalizedFreq2 = 0.2871; //0.330;//0.3401;
                //FreqDelta = 0.001;
                //GraphFreqInterval = 0.004; //0.01;
                // for latticeTheta = 60 r = 0.35a air hole
                //NormalizedFreq1 = 0.294;//0.295;// 0.280;
                //NormalizedFreq2 = 0.315;//0.320;// 0.3301;
                //FreqDelta = 0.001;
                //GraphFreqInterval = 0.002;// 0.010;
                // for latticeTheta = 60 r = 0.36a air hole
                //  wide-band
                //NormalizedFreq1 = 0.236;
                //NormalizedFreq2 = 0.3081;
                //FreqDelta = 0.0005;//0.001;
                //GraphFreqInterval = 0.010;
                // for latticeTheta = 60 r = 0.29a air hole
                //NormalizedFreq1 = 0.268;
                //NormalizedFreq2 = 0.2821;
                //FreqDelta = 0.0005;//0.001;
                //GraphFreqInterval = 0.002;
                // for latticeTheta = 30 r = 0.30a air hole n = 3.4
                //NormalizedFreq1 = 0.230;
                //NormalizedFreq2 = 0.256;//0.260;
                //FreqDelta = 0.001;
                //GraphFreqInterval = 0.004;

                NormalizedFreq1 = 0.215;
                NormalizedFreq2 = 0.2401;
                FreqDelta = 0.0005;
                GraphFreqInterval = 0.005;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            //minBeta = 0.0;
            //maxBeta = 0.5 * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            //minEffN = minWaveNum * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            //maxEffN = maxWaveNum * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            if (isAirHole)
            {
                minEffN = 0.0;//1.0;//0.0;
                maxEffN = Math.Sqrt(rodEps);
            }
            else
            {
                minEffN = 0.0;
                maxEffN = 1.0;//Math.Sqrt(rodEps);
            }

            MinSParameter = 0.0;
            MaxSParameter = 1.0;
            GraphSParameterInterval = 0.2;

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
            IList<uint> rodLoopIds = new List<uint>();
            IList<uint> rodLoopIds_InputWg1 = new List<uint>();
            IList<uint> rodLoopIds_InputWg2 = new List<uint>();
            int ndivPlus = 0;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_rod_B3 = new List<uint>();
            IList<uint> id_e_rod_B4 = new List<uint>();
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
                {
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
                }
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 8).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(4, 7).id_e_add;

                // 入出力導波路の周期構造境界上の頂点を追加
                IList<double> ys = new List<double>();
                IList<double> ys_rod = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                IList<uint> id_v_list_rod_B3 = new List<uint>();
                IList<uint> id_v_list_rod_B4 = new List<uint>();
                int outofAreaRodPtCnt_row_top = 0;
                int outofAreaRodPtCnt_row_bottom = 0;
                // 境界上にロッドのある格子
                // 境界上のロッドの頂点
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
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
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                    }
                }
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if (i % 2 == (isShift180 ? 1 : 0)) continue;
                    double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - 0.5 * rodDistanceY;
                    if (isLargeRod)
                    {
                        y0 -= 0.5 * rodDistanceY;
                    }
                    if (y0 > (0.0 + Constants.PrecisionLowerLimit) && y0 < (WaveguideWidth - Constants.PrecisionLowerLimit))
                    {
                        ys_rod.Add(y0);
                    }
                    else
                    {
                        if (isLargeRod && i == (rodCntHalf - 1))
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
                            if (isLargeRod && i == (rodCntHalf - 1))
                            {
                                outofAreaRodPtCnt_row_bottom++;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                        if (y2 > (0.0 + Constants.PrecisionLowerLimit) && y2 < (WaveguideWidth - Constants.PrecisionLowerLimit))
                        {
                            ys_rod.Add(y2);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
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
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
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
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if (i % 2 == (isShift180 ? 1 : 0)) continue;
                    for (int k = 1; k <= (ndivForOneLattice - 1); k++)
                    {
                        double y_divpt = rodDistanceY * rodCntHalf - i * rodDistanceY - k * (rodDistanceY / ndivForOneLattice);
                        double y_min_rod = rodDistanceY * rodCntHalf - i * rodDistanceY - 0.5 * rodDistanceY - rodRadius - radiusMargin;
                        double y_max_rod = rodDistanceY * rodCntHalf - i * rodDistanceY - 0.5 * rodDistanceY + rodRadius + radiusMargin;
                        if (isLargeRod)
                        {
                            y_divpt -= rodDistanceY * 0.5;
                            if (y_divpt <= (0.0 + Constants.PrecisionLowerLimit)) continue;
                            y_min_rod -= rodDistanceY * 0.5;
                            y_max_rod -= rodDistanceY * 0.5;
                        }
                        if (y_divpt < (y_min_rod - Constants.PrecisionLowerLimit) || y_divpt > (y_max_rod + Constants.PrecisionLowerLimit))
                        {
                            ys.Add(y_divpt);
                        }
                    }
                }
                
                // 境界上にロッドのない格子
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1)) continue;
                    for (int k = 0; k <= ndivForOneLattice; k++)
                    {
                        if (i == 0 && k == 0) continue;
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
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if (i % 2 == (isShift180 ? 0 : 1)) continue;
                    for (int k = 0; k <= ndivForOneLattice; k++)
                    {
                        if (i == (rodCntHalf - 1) && k == ndivForOneLattice) continue;
                        double y_divpt = rodDistanceY * rodCntHalf - i * rodDistanceY - k * (rodDistanceY / ndivForOneLattice);
                        double y_min_upper_rod = rodDistanceY * rodCntHalf - i * rodDistanceY + 0.5 * rodDistanceY - rodRadius - radiusMargin;
                        double y_max_lower_rod = rodDistanceY * rodCntHalf - (i + 1) * rodDistanceY - 0.5 * rodDistanceY + rodRadius + radiusMargin;
                        if (isLargeRod)
                        {
                            y_divpt -= rodDistanceY * 0.5;
                            if (y_divpt <= (0.0 + Constants.PrecisionLowerLimit)) continue;
                            y_min_upper_rod -= rodDistanceY * 0.5;
                            y_max_lower_rod -= rodDistanceY * 0.5;
                        }
                        bool isAddHalfRod_row_bottom = (isLargeRod
                            && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))));
                        if ((isShift180 || (!isShift180 && i != 0))
                            && y_divpt >= (y_min_upper_rod - Constants.PrecisionLowerLimit))
                        {
                            continue;
                        }
                        if ((i != (rodCntHalf - 1) || (i == (rodCntHalf - 1) && isAddHalfRod_row_bottom))
                            && y_divpt <= (y_max_lower_rod + Constants.PrecisionLowerLimit))
                        {
                            continue;
                        }

                        ys.Add(y_divpt);
                    }
                }
                // 欠陥部
                for (int i = 0; i <= (defectRodCnt * ndivForOneLattice); i++)
                {
                    if (!isShift180 && (i == 0 || i == (defectRodCnt * ndivForOneLattice))) continue;
                    double y_divpt = rodDistanceY * (rodCntHalf + defectRodCnt) - i * (rodDistanceY / ndivForOneLattice);
                    double y_min_upper_rod = rodDistanceY * (rodCntHalf + defectRodCnt) + 0.5 * rodDistanceY - rodRadius - radiusMargin;
                    double y_max_lower_rod = rodDistanceY * rodCntHalf - 0.5 * rodDistanceY + rodRadius + radiusMargin;
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

                // 昇順でソート
                double[] yAry = ys.ToArray();
                Array.Sort(yAry);
                ndivPlus = yAry.Length + 1;

                // yAryは昇順なので、yAryの並びの順に追加すると境界1上を逆方向に移動することになる
                //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                // 入力導波路 外側境界
                // 入力導波路 内部側境界
                // 出力導波路 外側境界
                // 出力導波路 内部側境界
                for (int boundaryIndex = 0; boundaryIndex < 4;  boundaryIndex++)
                {
                    bool isInRod = false;
                    if (isLargeRod
                        && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                    {
                        isInRod = true;
                    }

                    for (int i = 0; i < yAry.Length; i++)
                    {
                        uint id_e = 0;
                        double x1 = 0.0;
                        double y_pt = 0.0;
                        IList<uint> work_id_e_rod_B = null;
                        IList<uint> work_id_v_list_rod_B = null;
                        if (boundaryIndex == 0)
                        {
                            // 入力導波路 外側境界
                            id_e = 1;
                            x1 = 0.0;
                            y_pt = yAry[i];
                            work_id_e_rod_B = id_e_rod_B1;
                            work_id_v_list_rod_B = id_v_list_rod_B1;
                        }
                        else if (boundaryIndex == 1)
                        {
                            // 入力導波路 内側境界
                            id_e = 9;
                            x1 = inputWgLength;
                            y_pt = yAry[yAry.Length - 1 - i];
                            work_id_e_rod_B = id_e_rod_B2;
                            work_id_v_list_rod_B = id_v_list_rod_B2;
                        }
                        else if (boundaryIndex == 2)
                        {
                            // 出力導波路 外側境界
                            id_e = 5;
                            x1 = inputWgLength * 2 + disconLength;
                            y_pt = yAry[yAry.Length - 1 - i];
                            work_id_e_rod_B = id_e_rod_B3;
                            work_id_v_list_rod_B = id_v_list_rod_B3;
                        }
                        else if (boundaryIndex == 3)
                        {
                            // 出力導波路 内側境界
                            id_e = 10;
                            x1 = inputWgLength + disconLength;
                            y_pt = yAry[yAry.Length - 1 - i];
                            work_id_e_rod_B = id_e_rod_B4;
                            work_id_v_list_rod_B = id_v_list_rod_B4;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                         
                        CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x1, y_pt));
                        uint id_v_add = resAddVertex.id_v_add;
                        uint id_e_add = resAddVertex.id_e_add;
                        System.Diagnostics.Debug.Assert(id_v_add != 0);
                        System.Diagnostics.Debug.Assert(id_e_add != 0);
                        if (isInRod)
                        {
                            work_id_e_rod_B.Add(id_e_add);
                        }
                        bool contains = false;
                        foreach (double y_rod in ys_rod)
                        {
                            if (Math.Abs(y_rod - y_pt) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                            {
                                contains = true;
                                break;
                            }
                        }
                        if (contains)
                        {
                            work_id_v_list_rod_B.Add(id_v_add);

                            if (isLargeRod
                                && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0))))
                            {
                                if ((work_id_v_list_rod_B.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 1)
                                {
                                    isInRod = true;
                                }
                                else if ((work_id_v_list_rod_B.Count + outofAreaRodPtCnt_row_top) % (rodRadiusDiv * 2 + 1) == 0)
                                {
                                    isInRod = false;
                                }
                            }
                            else
                            {
                                if (work_id_v_list_rod_B.Count % (rodRadiusDiv * 2 + 1) == 1)
                                {
                                    isInRod = true;
                                }
                                else if (work_id_v_list_rod_B.Count % (rodRadiusDiv * 2 + 1) == 0)
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
                                work_id_e_rod_B.Add(id_e);
                            }
                        }
                    }
                }

                int bRodCntHalf = (isShift180 ? (int)((rodCntHalf + 1) / 2) : (int)((rodCntHalf) / 2));
                if (!isLargeRod
                    || (isLargeRod &&
                           (isShift180 && (rodCntHalf % 2 == 0)) || (!isShift180 && (rodCntHalf % 2 == 1))
                       )
                    )
                {
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                }
                else
                {
                    System.Diagnostics.Debug.Assert(outofAreaRodPtCnt_row_top == (rodRadiusDiv + 1));
                    System.Diagnostics.Debug.Assert(outofAreaRodPtCnt_row_bottom == (rodRadiusDiv + 1));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                    System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == (bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1) - outofAreaRodPtCnt_row_top - outofAreaRodPtCnt_row_bottom));
                }

                /////////////////////////////////////////////////////////////////////////////
                // ロッドを追加
                uint id_e_F1_new_port1 = 0;
                uint id_e_F2_new_port1 = 0;
                uint id_e_F1_discon_new_port1 = 0;
                uint id_e_F2_discon_new_port1 = 0;
                uint id_e_F1_new_port2 = 0;
                uint id_e_F2_new_port2 = 0;
                uint id_e_F1_discon_new_port2 = 0;
                uint id_e_F2_discon_new_port2 = 0;
                uint id_v_B1_top_rod_center = 1;
                uint id_v_B1_bottom_rod_center = 2;
                uint id_v_B2_top_rod_center = 8;
                uint id_v_B2_bottom_rod_center = 3;
                uint id_v_B3_top_rod_center = 6;
                uint id_v_B3_bottom_rod_center = 5;
                uint id_v_B4_top_rod_center = 7;
                uint id_v_B4_bottom_rod_center = 4;

                // 左右のロッド(上下の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1_rodQuarter = new List<uint>();
                IList<uint> id_v_list_F2_rodQuarter = new List<uint>();

                for (int colIndex = 0; colIndex < 4; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 上の強制境界と交差する点
                    if (isLargeRod
                        && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0)))
                           )
                    {
                        uint[] id_e_list = new uint[2];
                        if (colIndex == 0)
                        {
                            // 入力境界 外側
                            // 入力導波路領域
                            id_e_list[0] = 8;
                            id_e_list[1] = 8;
                        }
                        else if (colIndex == 1)
                        {
                            // 入力境界内側
                            // 不連続領域
                            id_e_list[0] = 8;
                            id_e_list[1] = 7;
                        }
                        else if (colIndex == 2)
                        {
                            // 出力境界内側
                            // 不連続領域
                            id_e_list[0] = 7;
                            id_e_list[1] = 6;
                        }
                        else if (colIndex == 3)
                        {
                            // 出力境界外側
                            // 出力導波路領域
                            id_e_list[0] = 6;
                            id_e_list[1] = 6;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double x0 = 0.0;
                        if (colIndex == 0 || colIndex == 1)
                        {
                            // 入力側
                            x0 = (rodDistanceX) * colIndex;
                        }
                        else if (colIndex == 2 || colIndex == 3)
                        {
                            // 出力側
                            x0 = inputWgLength + disconLength + (rodDistanceX) * (colIndex - 2);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double y0 = WaveguideWidth;
                        double y_cross = WaveguideWidth;
                        double[] x_cross_list = new double[2];
                        x_cross_list[0] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0;
                        x_cross_list[1] = Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0;
                        for (int k = 0; k < 2; k++)
                        {
                            uint id_e = id_e_list[k];
                            double x_cross = x_cross_list[k];
                            if (colIndex == 0 || colIndex == 1)
                            {
                                if (x_cross <= (0.0 + Constants.PrecisionLowerLimit))
                                {
                                    continue;
                                }
                            }
                            else if (colIndex == 2 || colIndex == 3)
                            {
                                if (x_cross >= (inputWgLength * 2.0 + disconLength - Constants.PrecisionLowerLimit))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F1_rodQuarter.Add(id_v_add);
                            id_e_F1.Add(id_e_add);
                            // 上側境界の中央部分の辺IDが新しくなる
                            if (colIndex == 0 && k == 1)
                            {
                                // 入力部
                                // 不変
                            }
                            else if (colIndex == 1 && k == 0)
                            {
                                // 不連続部
                                id_e_F1_new_port1 = id_e_add;
                            }
                            else if (colIndex == 1 && k == 1)
                            {
                                // 不連続部
                                //不変
                            }
                            else if (colIndex == 2 && k == 0)
                            {
                                // 不連続部（出力側)
                                id_e_F1_discon_new_port1 = id_e_add;
                                id_e_F1_discon_new_port2 = id_e_add;
                            }
                            else if (colIndex == 2 && k == 1)
                            {
                                // 不連続部（出力側)
                                // 不変
                            }
                            else if (colIndex == 3 && k == 0)
                            {
                                // 出力部
                                id_e_F1_new_port2 = id_e_add;
                            }
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }

                for (int colIndex = 3; colIndex >= 0; colIndex--) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 下の強制境界と交差するロッド
                    if (isLargeRod
                        && ((isShift180 && (rodCntHalf % 2 == 1)) || (!isShift180 && (rodCntHalf % 2 == 0)))
                           )
                    {
                        uint[] id_e_list = new uint[2];
                        if (colIndex == 0)
                        {
                            // 入力境界 外側
                            // 入力導波路領域
                            id_e_list[0] = 2;
                            id_e_list[1] = 2;
                        }
                        else if (colIndex == 1)
                        {
                            // 入力境界内側
                            // 不連続領域
                            id_e_list[0] = 3;
                            id_e_list[1] = 2;
                        }
                        else if (colIndex == 2)
                        {
                            // 出力境界内側
                            // 不連続領域
                            id_e_list[0] = 4;
                            id_e_list[1] = 3;
                        }
                        else if (colIndex == 3)
                        {
                            // 入力境界 外側
                            // 入力導波路領域
                            id_e_list[0] = 4;
                            id_e_list[1] = 4;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double x0 = 0.0;
                        if (colIndex == 0 || colIndex == 1)
                        {
                            // 入力側
                            x0 = (rodDistanceX) * colIndex;
                        }
                        else if (colIndex == 2 || colIndex == 3)
                        {
                            // 出力側
                            x0 = inputWgLength + disconLength + (rodDistanceX) * (colIndex - 2);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double y0 = 0.0;
                        double y_cross = 0.0;
                        double[] x_cross_list = new double[2];
                        x_cross_list[0] = Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0;
                        x_cross_list[1] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0;
                        for (int k = 0; k < 2; k++)
                        {
                            uint id_e = id_e_list[k];
                            double x_cross = x_cross_list[k];
                            if (colIndex == 0 || colIndex == 1)
                            {
                                if (x_cross <= (0.0 + Constants.PrecisionLowerLimit))
                                {
                                    continue;
                                }
                            }
                            else if (colIndex == 2 || colIndex == 3)
                            {
                                if (x_cross >= (inputWgLength * 2.0 + disconLength - Constants.PrecisionLowerLimit))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F2_rodQuarter.Add(id_v_add);
                            id_e_F2.Add(id_e_add);
                            // 下側境界の中央部分の辺IDが新しくなる
                            //   Note:colInde == 3から追加されることに注意
                            if (colIndex == 0 && k == 0)
                            {
                                // 入力部
                                id_e_F2_new_port1 = id_e_add;
                            }
                            else if (colIndex == 0 && k == 1)
                            {
                                // 入力部
                                // 不変
                            }
                            else if (colIndex == 1 && k == 0)
                            {
                                // 不連続部
                                id_e_F2_discon_new_port1 = id_e_add;
                                id_e_F2_discon_new_port2 = id_e_add;
                            }
                            else if (colIndex == 2 && k == 1)
                            {
                                // 不連続部（出力側)
                                // 不変
                            }
                            else if (colIndex == 2 && k == 0)
                            {
                                // 不連続部（出力側)
                                id_e_F2_new_port2 = id_e_add;
                            }
                            else if (colIndex == 3 && k == 1)
                            {
                                // 出力部
                                // 不変
                            }
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }

                // 左のロッドを追加
                for (int colIndex = 0; colIndex < 3; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 左のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    uint work_id_v_B_top_rod_center = 0;
                    uint work_id_v_B_bottom_rod_center = 0;

                    // 始点、終点が逆？
                    bool isReverse = false;
                    if (colIndex == 0)
                    {
                        // 入力境界 外側
                        x_B = 0.0;
                        work_id_v_list_rod_B = id_v_list_rod_B1;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                        work_id_v_B_top_rod_center = id_v_B1_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B1_bottom_rod_center;
                        isReverse = false;
                    }
                    else if (colIndex == 1)
                    {
                       // 入力境界 内側
                        x_B = inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                        work_id_v_B_top_rod_center = id_v_B2_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B2_bottom_rod_center;
                        isReverse = true;
                    }
                    else if (colIndex == 2)
                    {
                        // 出力境界 内側
                        x_B = inputWgLength + disconLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                        work_id_v_B_top_rod_center = id_v_B4_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B4_bottom_rod_center;
                        isReverse = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = bRodCntHalf - 1 - (int)((rodCntHalf - 1 - i) / 2);
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
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                                    int index_v1 = (work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                                    int index_v2 = (work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_left;
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        isQuarterRod = true;
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_top_rod_center;
                                        //id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1];
                                        id_v2 = id_v_list_F1_rodQuarter[0 + colIndex * 2]; // 1つ飛ばしで参照;
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                else
                                {
                                    int index_v0 = (0 + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_left;
                                    int index_v1 = ((rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_left;
                                    int index_v2 = ((rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_left;
                                    if (index_v0 < 0)
                                    {
                                        isQuarterRod = true;
                                        //id_v0 = work_id_v_list_rod_B[0];
                                        id_v0 = id_v_list_F1_rodQuarter[0 + colIndex * 2]; // 1つ飛ばしで参照
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_top_rod_center;
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
                                double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                                if (isLargeRod)
                                {
                                    y0 += rodDistanceY * 0.5;
                                }
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
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
                                        work_id_v2,
                                        id_v1,
                                        work_id_v0,
                                        0.0,
                                        true);
                                }
                                else
                                {
                                    // 左のロッド
                                    lId = WgCadUtil.AddLeftRod(
                                        cad2d,
                                        baseLoopId,
                                        work_id_v0,
                                        id_v1,
                                        work_id_v2,
                                        x0,
                                        y0,
                                        rodRadius,
                                        rodCircleDiv,
                                        rodRadiusDiv);
                                }
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }

                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
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
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count / 2 - 1 - i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v0 < 0)
                                    {
                                        isQuarterRod = true;
                                        //id_v0 = work_id_v_list_rod_B[0]; // DEBUG
                                        id_v0 = id_v_list_F2_rodQuarter[id_v_list_F2_rodQuarter.Count - 1 - colIndex * 2]; // 1つ飛ばしで参照
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_bottom_rod_center;
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                else
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        isQuarterRod = true;
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_bottom_rod_center;
                                        //id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1]; // DEBUG
                                        id_v2 = id_v_list_F2_rodQuarter[id_v_list_F2_rodQuarter.Count - 1 - colIndex * 2]; // 1つ飛ばしで参照
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                
                                }
                                double x0 = x_B;
                                double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                if (isLargeRod)
                                {
                                    y0 -= rodDistanceY * 0.5;
                                }
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
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
                                        work_id_v2,
                                        id_v1,
                                        work_id_v0,
                                        90.0,
                                        true);
                                }
                                else
                                {
                                    // 左のロッド
                                    lId = WgCadUtil.AddLeftRod(
                                         cad2d,
                                         baseLoopId,
                                         work_id_v0,
                                         id_v1,
                                         work_id_v2,
                                         x0,
                                         y0,
                                         rodRadius,
                                         rodCircleDiv,
                                         rodRadiusDiv);
                                }
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                            }
                        }
                    }
                }

                // 右のロッドを追加
                for (int colIndex = 0; colIndex < 3; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 右のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    uint work_id_v_B_top_rod_center = 0;
                    uint work_id_v_B_bottom_rod_center = 0;

                    if (colIndex == 0)
                    {
                        // 入力境界 内側
                        x_B = inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                        work_id_v_B_top_rod_center = id_v_B2_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B2_bottom_rod_center;
                    }
                    else if (colIndex == 1)
                    {
                        // 出力境界 内側
                        x_B = inputWgLength + disconLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                        work_id_v_B_top_rod_center = id_v_B4_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B4_bottom_rod_center;
                    }
                    else if (colIndex == 2)
                    {
                        // 出力境界 外側
                        x_B = inputWgLength * 2.0 + disconLength;
                        work_id_v_list_rod_B = id_v_list_rod_B3;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                        work_id_v_B_top_rod_center = id_v_B3_top_rod_center;
                        work_id_v_B_bottom_rod_center = id_v_B3_bottom_rod_center;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = bRodCntHalf - 1 - (int)((rodCntHalf - 1 - i) / 2);
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
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_top;
                                    int index_v1 = (work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)) - ofs_index_top;
                                    int index_v2 = (work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        isQuarterRod = true;
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = id_v_B2_top_rod_center;
                                        //id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1]; //DEBUG
                                        id_v2 = id_v_list_F1_rodQuarter[1 + colIndex * 2]; // 1つ飛ばしで参照;
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                else
                                {
                                    int index_v0 = (0 + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                                    int index_v1 = ((rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                                    int index_v2 = ((rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)) + ofs_index_top;
                                    if (index_v0 < 0)
                                    {
                                        isQuarterRod = true;
                                        //id_v0 = work_id_v_list_rod_B[0]; // DEBUG
                                        id_v0 = id_v_list_F1_rodQuarter[1 + colIndex * 2];
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_top_rod_center;
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
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
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
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
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count / 2 - 1 - i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v0 < 0)
                                    {
                                        isQuarterRod = true;
                                        //id_v0 = work_id_v_list_rod_B[0]; //DEBUG
                                        id_v0 = id_v_list_F2_rodQuarter[id_v_list_F2_rodQuarter.Count - 2 - colIndex * 2]; // 1つ飛ばしで参照
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_bottom_rod_center;
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                else
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        isQuarterRod = true;
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        //id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v1 = work_id_v_B_bottom_rod_center;
                                        //id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1]; // DEBUG
                                        id_v2 = id_v_list_F2_rodQuarter[id_v_list_F2_rodQuarter.Count - 2 - colIndex * 2]; // 1つ飛ばしで参照
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
                                double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                if (isLargeRod)
                                {
                                    y0 -= rodDistanceY * 0.5;
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
                                        180.0,
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
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                            }
                        }
                    }
                }

                // 中央ロッド
                int periodCntInputWg1 = 1;
                int periodCntInputWg2 = 1;
                int periodCntX = periodCntInputWg1 + rodCntDiscon  + periodCntInputWg2;

                // 中央のロッド(上下の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1 = new List<uint>();
                IList<uint> id_v_list_F2 = new List<uint>();
                for (int col = 1; col <= (periodCntX * 2 - 1); col++)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (col == (periodCntInputWg1 + rodCntDiscon) * 2) continue; // 出力導波路内部境界  (既にロッド追加済み)
                    if (col == (periodCntX * 2)) continue; // 出力導波路外側境界  (既にロッド追加済み)
                    int inputWgNo = 0;
                    if (col >= 0 && col < (periodCntInputWg1 * 2))
                    {
                        inputWgNo = 1;
                    }
                    else if (col >= (periodCntInputWg1 * 2 + 1) && col < (periodCntInputWg1 + rodCntDiscon) * 2)
                    {
                        inputWgNo = 0;
                    }
                    else if (col >= ((periodCntInputWg1 + rodCntDiscon) * 2 + 1) && col < ((periodCntInputWg1 + rodCntDiscon + periodCntInputWg2) * 2))
                    {
                        inputWgNo = 2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    // 上の強制境界と交差するロッド
                    if (isLargeRod
                           && ((col % 2 == 1 && ((rodCntHalf - 1 - 0) % 2 == (isShift180 ? 1 : 0)))
                               || (col % 2 == 0 && ((rodCntHalf - 1 - 0) % 2 == (isShift180 ? 0 : 1)))
                                )
                        )
                    {
                        uint id_e = 0;
                        if (inputWgNo == 1)
                        {
                            id_e = 8;
                            //if (!isShift180)
                            if (id_e_F1_new_port1 != 0)
                            {
                                id_e = id_e_F1_new_port1;
                            }
                        }
                        else if (inputWgNo == 0)
                        {
                            id_e = 7;
                            //if (!isShift180)
                            if (id_e_F1_discon_new_port1 != 0)
                            {
                                id_e = id_e_F1_discon_new_port1;
                            }
                        }
                        else if (inputWgNo == 2)
                        {
                            id_e = 6;
                            //if (!isShift180)
                            if (id_e_F1_new_port2 != 0)
                            {
                                id_e = id_e_F1_new_port2;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double x0 = rodDistanceX * 0.5 * col;
                        double y0 = WaveguideWidth;
                        double y_cross = WaveguideWidth;
                        double[] x_cross_list = new double[3];
                        x_cross_list[0] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0; // 交点
                        x_cross_list[1] = x0; // 中心
                        x_cross_list[2] = Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0; // 交点
                        foreach (double x_cross in x_cross_list)
                        {
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F1.Add(id_v_add);
                            id_e_F1.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }

                for (int col = (periodCntX * 2 - 1); col >= 1; col--)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (col == (periodCntInputWg1 + rodCntDiscon) * 2) continue; // 出力導波路内部境界  (既にロッド追加済み)
                    if (col == (periodCntX * 2)) continue; // 出力導波路外側境界  (既にロッド追加済み)
                    int inputWgNo = 0;
                    if (col >= 0 && col < (periodCntInputWg1 * 2))
                    {
                        inputWgNo = 1;
                    }
                    else if (col >= (periodCntInputWg1 * 2 + 1) && col < (periodCntInputWg1 + rodCntDiscon) * 2)
                    {
                        inputWgNo = 0;
                    }
                    else if (col >= ((periodCntInputWg1 + rodCntDiscon) * 2 + 1) && col < ((periodCntInputWg1 + rodCntDiscon + periodCntInputWg2) * 2))
                    {
                        inputWgNo = 2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    // 下の強制境界と交差するロッド
                    if (isLargeRod
                           && ((col % 2 == 1 && ((rodCntHalf - 1 - 0) % 2 == (isShift180 ? 1 : 0)))
                              || (col % 2 == 0 && ((rodCntHalf - 1 - 0) % 2 == (isShift180 ? 0 : 1)))
                              )
                        )
                    {
                        uint id_e = 0;
                        if (inputWgNo == 1)
                        {
                            id_e = 2;
                            //if (!isShift180)
                            if (id_e_F2_new_port1 != 0)
                            {
                                id_e = id_e_F2_new_port1;
                            }
                        }
                        else if (inputWgNo == 0)
                        {
                            id_e = 3;
                            //if (!isShift180)
                            if (id_e_F2_discon_new_port1 != 0)
                            {
                                id_e = id_e_F2_discon_new_port1;
                            }
                        }
                        else if (inputWgNo == 2)
                        {
                            id_e = 4;
                            //if (!isShift180)
                            if (id_e_F2_new_port2 != 0)
                            {
                                id_e = id_e_F2_new_port2;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double x0 = rodDistanceX * 0.5 * col;
                        double y0 = 0.0;
                        double y_cross = 0.0;
                        double[] x_cross_list = new double[3];
                        x_cross_list[0] = Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0; // 交点
                        x_cross_list[1] = x0; // 中心
                        x_cross_list[2] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (y_cross - y0) * (y_cross - y0)) + x0; // 交点
                        foreach (double x_cross in x_cross_list)
                        {
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F2.Add(id_v_add);
                            id_e_F2.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }

                // 中央のロッド
                for (int col = 1; col <= (periodCntX * 2 - 1); col++)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (col == (periodCntInputWg1 + rodCntDiscon) * 2) continue; // 出力導波路内部境界  (既にロッド追加済み)
                    if (col == (periodCntX * 2)) continue; // 出力導波路外側境界  (既にロッド追加済み)
                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    if (col >= 0 && col < (periodCntInputWg1 * 2))
                    {
                        baseLoopId = 1;
                        inputWgNo = 1;
                    }
                    else if (col >= (periodCntInputWg1 * 2 + 1) && col < (periodCntInputWg1 + rodCntDiscon) * 2)
                    {
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    else if (col >= ((periodCntInputWg1 + rodCntDiscon) * 2 + 1) && col < ((periodCntInputWg1 + rodCntDiscon + periodCntInputWg2) * 2))
                    {
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    // 中央のロッド
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (isLargeRod &&
                              ((col % 2 == 1 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                                || (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                            )
                        {
                            if (i == 0)
                            {
                                {
                                    // 半円（下半分)を追加
                                    double x0 = rodDistanceX * 0.5 * col;
                                    double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                                    if (isLargeRod)
                                    {
                                        y0 += rodDistanceY * 0.5; // for isLargeRod
                                    }
                                    int col2 = col / 2;
                                    if (isShift180 && (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                                    {
                                        col2 = col2 - 2;
                                    }
                                    else if (!isShift180 && (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                                    {
                                        col2 = col2 - 2;
                                    }
                                    uint id_v0 = id_v_list_F1[col2 * 3 + 0];
                                    uint id_v1 = id_v_list_F1[col2 * 3 + 1];
                                    uint id_v2 = id_v_list_F1[col2 * 3 + 2];
                                    uint lId = WgCadUtil.AddExactlyHalfRod(
                                        cad2d,
                                        baseLoopId,
                                        id_v0,
                                        id_v1,
                                        id_v2,
                                        x0,
                                        y0,
                                        rodRadius,
                                        rodCircleDiv,
                                        rodRadiusDiv,
                                        0.0,
                                        true);
                                    rodLoopIds.Add(lId);
                                    if (inputWgNo == 1)
                                    {
                                        rodLoopIds_InputWg1.Add(lId);
                                    }
                                    else if (inputWgNo == 2)
                                    {
                                        rodLoopIds_InputWg2.Add(lId);
                                    }
                                }

                                continue;
                            }
                        }
                        if ((col % 2 == 1 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
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
                            if (inputWgNo == 1)
                            {
                                rodLoopIds_InputWg1.Add(lId);
                            }
                            else if (inputWgNo == 2)
                            {
                                rodLoopIds_InputWg2.Add(lId);
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (isLargeRod
                               && ((col % 2 == 1 && (i % 2 == (isShift180 ? 1 : 0)))
                                    || (col % 2 == 0 && (i % 2 == (isShift180 ? 0 : 1))))
                               )
                        {
                            if (i == (rodCntHalf - 1))
                            {
                                {
                                    // 半円（上半分)を追加
                                    double x0 = rodDistanceX * 0.5 * col;
                                    double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                    if (isLargeRod)
                                    {
                                        y0 -= rodDistanceY * 0.5; // for isLargeRod
                                    }
                                    int col2 = (periodCntX * 2 - 1 - col) / 2;
                                    if (isShift180 && (col % 2 == 0 && (i % 2 == (isShift180 ? 0 : 1))))
                                    {
                                        col2 = col2 - 1;
                                    }
                                    else if (!isShift180 && (col % 2 == 0 && (i % 2 == (isShift180 ? 0 : 1))))
                                    {
                                        col2 = col2 - 1;
                                    }
                                    uint id_v0 = id_v_list_F2[col2 * 3 + 0];
                                    uint id_v1 = id_v_list_F2[col2 * 3 + 1];
                                    uint id_v2 = id_v_list_F2[col2 * 3 + 2];
                                    uint lId = WgCadUtil.AddExactlyHalfRod(
                                        cad2d,
                                        baseLoopId,
                                        id_v0,
                                        id_v1,
                                        id_v2,
                                        x0,
                                        y0,
                                        rodRadius,
                                        rodCircleDiv,
                                        rodRadiusDiv,
                                        180.0,
                                        true);
                                    rodLoopIds.Add(lId);
                                    if (inputWgNo == 1)
                                    {
                                        rodLoopIds_InputWg1.Add(lId);
                                    }
                                    else if (inputWgNo == 2)
                                    {
                                        rodLoopIds_InputWg2.Add(lId);
                                    }
                                }

                                continue;
                            }
                        }
                        if ((col % 2 == 1 && (i % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (i % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                            if (isLargeRod)
                            {
                                y0 -= rodDistanceY * 0.5; // for isLargeRod
                            }
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                            rodLoopIds.Add(lId);
                            if (inputWgNo == 1)
                            {
                                rodLoopIds_InputWg1.Add(lId);
                            }
                            else if (inputWgNo == 2)
                            {
                                rodLoopIds_InputWg2.Add(lId);
                            }
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
                    foreach (uint eId in id_e_rod_B3)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B4)
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
                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = new uint[3 + rodLoopIds.Count];
                loopId_cad_list[0] = 1;
                loopId_cad_list[1] = 2;
                loopId_cad_list[2] = 3;
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    loopId_cad_list[i + 3] = rodLoopIds[i];
                }
                int[] mediaIndex_list = new int[3 + rodLoopIds.Count];
                for (int i = 0; i < 3; i++)
                {
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    mediaIndex_list[i + 3] = Medias.IndexOf(mediaCore);
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
                // 最小、最大波数ベクトル
                WgPortInfoList[portIndex].MinWaveNum = minWaveNum;
                WgPortInfoList[portIndex].MaxWaveNum = maxWaveNum;

                // 緩慢変化包絡線近似？
                WgPortInfoList[portIndex].IsSVEA = isSVEA;
                // 固有値問題を反復で解く？
                WgPortInfoList[portIndex].IsSolveEigenItr = isSolveEigenItr;
                // 伝搬モードの数
                WgPortInfoList[portIndex].PropModeCntToSolve = propModeCntToSolve;
            }
            // 入射ポートの設定
            //   ポート1を入射ポートとする
            WgPortInfoList[0].IsIncidentPort = true;
            // 入射インデックスの設定
            if (incidentModeIndex != 0)
            {
                System.Diagnostics.Debug.WriteLine("IncidentModeIndex: {0}", incidentModeIndex);
                WgPortInfoList[0].IncidentModeIndex = incidentModeIndex;
                WgPortInfoList[1].IncidentModeIndex = incidentModeIndex;
            }

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            if ((WaveModeDv == WgUtil.WaveModeDV.TE && !isMagneticWall)  // TEモードで電気壁
                || (WaveModeDv == WgUtil.WaveModeDV.TM && isMagneticWall) // TMモードで磁気壁
                )
            {
                uint[] eId_cad_list = new uint[6 + id_e_F1.Count + id_e_F2.Count];
                eId_cad_list[0] = 2;
                eId_cad_list[1] = 3;
                eId_cad_list[2] = 4;
                eId_cad_list[3] = 6;
                eId_cad_list[4] = 7;
                eId_cad_list[5] = 8;
                for (int i = 0; i < id_e_F1.Count; i++)
                {
                    eId_cad_list[6 + i] = id_e_F1[i];
                }
                for (int i = 0; i < id_e_F2.Count; i++)
                {
                    eId_cad_list[6 + id_e_F1.Count + i] = id_e_F2[i];
                }
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
            // 開口条件
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                wgPortInfo1.FieldPortBcId = 0;

                uint[] eId_cad_list = new uint[ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                IList<uint> work_id_e_rod_B = null;

                if (portIndex == 0)
                {
                    eId_cad_list[0] = 1;
                    work_id_e_rod_B = id_e_rod_B1;
                }
                else if (portIndex == 1)
                {
                    eId_cad_list[0] = 5;
                    work_id_e_rod_B = id_e_rod_B3;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                if (work_id_e_rod_B.Contains(eId_cad_list[0]))
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCore);
                }
                else
                {
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 1; i <= ndivPlus - 1; i++)
                {
                    if (portIndex == 0)
                    {
                        eId_cad_list[i] = (uint)(10 + (ndivPlus - 1) - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(10 + (ndivPlus - 1) * 3 - (i - 1));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    if (work_id_e_rod_B.Contains(eId_cad_list[i]))
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                    }
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
                wgPortInfo1.FieldPortBcId = fieldPortBcId;
                foreach (var pair in workEdgeDic)
                {
                    EdgeDic.Add(pair.Key, pair.Value);
                    wgPortInfo1.InputWgEdgeDic.Add(pair.Key, pair.Value);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            // 周期構造入出力導波路1
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                wgPortInfo1.FieldInputWgLoopId = 0;

                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = null;
                if (portIndex == 0)
                {
                    loopId_cad_list = new uint[1 + rodLoopIds_InputWg1.Count];
                    loopId_cad_list[0] = 1;
                    for (int i = 0; i < rodLoopIds_InputWg1.Count; i++)
                    {
                        loopId_cad_list[i + 1] = rodLoopIds_InputWg1[i];
                    }
                }
                else if (portIndex == 1)
                {
                    loopId_cad_list = new uint[1 + rodLoopIds_InputWg2.Count];
                    loopId_cad_list[0] = 3;
                    for (int i = 0; i < rodLoopIds_InputWg2.Count; i++)
                    {
                        loopId_cad_list[i + 1] = rodLoopIds_InputWg2[i];
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                int[] mediaIndex_list = null;
                if (portIndex == 0)
                {
                    mediaIndex_list = new int[1 + rodLoopIds_InputWg1.Count];
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    for (int i = 0; i < rodLoopIds_InputWg1.Count; i++)
                    {
                        mediaIndex_list[i + 1] = Medias.IndexOf(mediaCore);
                    }
                }
                else if (portIndex == 1)
                {
                    mediaIndex_list = new int[1 + rodLoopIds_InputWg2.Count];
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    for (int i = 0; i < rodLoopIds_InputWg2.Count; i++)
                    {
                        mediaIndex_list[i + 1] = Medias.IndexOf(mediaCore);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }

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
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                wgPortInfo1.FieldInputWgBcId = 0;

                uint[] eId_cad_list = new uint[ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                IList<uint> work_id_e_rod_B = null;

                if (portIndex == 0)
                {
                    eId_cad_list[0] = 9;
                    work_id_e_rod_B = id_e_rod_B2;
                    if (work_id_e_rod_B.Contains(eId_cad_list[0]))
                    {
                        mediaIndex_list[0] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    }
                }
                else if (portIndex == 1)
                {
                    eId_cad_list[0] = 10;
                    work_id_e_rod_B = id_e_rod_B4;
                    if (work_id_e_rod_B.Contains(eId_cad_list[0]))
                    {
                        mediaIndex_list[0] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                for (int i = 1; i <= ndivPlus - 1; i++)
                {
                    if (portIndex == 0)
                    {
                        eId_cad_list[i] = (uint)(10 + (ndivPlus - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(10 + (ndivPlus - 1) * 4 - (i - 1));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    if (work_id_e_rod_B.Contains(eId_cad_list[i]))
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                    }
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
            // フォトニック結晶導波路チャンネル上節点を取得する
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                wgPortInfo1.IsPCWaveguide = true;

                uint[] no_c_all = null;
                Dictionary<uint, uint> to_no_loop = null;
                double[][] coord_c_all = null;
                WgUtil.GetLoopCoordList(World, wgPortInfo1.FieldInputWgLoopId, out no_c_all, out to_no_loop, out coord_c_all);
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
                    wgPortInfo1.PCWaveguidePorts.Add(portNodes);
                }
            }
            return true;
        }
    }
}
