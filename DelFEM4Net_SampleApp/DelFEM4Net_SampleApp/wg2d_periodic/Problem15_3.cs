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
    class Problem15_3
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC導波路 三角形格子 空洞共振器 (通過型 3ポート)
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
            int propModeCntToSolve = 1;
            //int propModeCntToSolve = 3;
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
            bool isShift180 = false;
            //bool isShift180 = true;
            // ロッドの数(半分)
            //const int rodCntHalf = 3; // for latticeTheta = 60 r = 0.35a air hole
            const int rodCntHalf = 3;
            // 欠陥ロッド数
            const int defectRodCnt = 1;
            // 三角形格子の内角
            double latticeTheta = 60.0; // for latticeTheta = 60 r = 0.34a air hole
            // ロッドの半径
            double rodRadiusRatio = 0.33721;// 0.30952;// 0.31; // for latticeTheta = 60 r = 0.34a air hole defectRodCnt_cavity == 3
            // ロッドの比誘電率
            double rodEps = 2.76 * 2.76; // for latticeTheta = 60 r = 0.31a air hole
            // 1格子当たりの分割点の数
            //const int ndivForOneLattice = 7; // for latticeTheta = 60 r = 0.34a air hole defectRodCnt_cavity == 3
            const int ndivForOneLattice = 7;
            // ロッド円周の分割数
            //const int rodCircleDiv = 12;
            const int rodCircleDiv = 12;
            // ロッドの半径の分割数
            //const int rodRadiusDiv = 4;
            const int rodRadiusDiv = 4;
            // 導波路不連続領域の長さ
            //const int rodCntDiscon = 5; // defectRodCnt_cavity == 3 
            const int rodCntDiscon = 3;
            // 入出力導波路の入力端からの長さ
            //const int rodCntX_Wg = 2;//10; // defectRodCnt_cavity == 3
            const int rodCntX_Wg = 1;
            // 共振器の欠陥ロッド数
            const int defectRodCnt_cavity = 3; // defectRodCnt_cavity == 3
            // 共振器と入力導波路の間のロッド数
            //const int spacingRodCnt = 3; // defectRodCnt_cavity == 3
            const int spacingRodCnt = 2;// 3;
            // 最適形状?
            //const bool isOpt = false;
            const bool isOpt = true;

            // 格子の数
            int latticeCnt = rodCntHalf * 2 + defectRodCnt;
            // ロッド間の距離(Y方向)
            double rodDistanceY = WaveguideWidth / (double)latticeCnt;
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

            if (Math.Abs(latticeTheta - 60.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                    || Math.Abs(latticeTheta - 30.0) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // for latticeTheta = 60 r = 0.34a
                NormalizedFreq1 = 0.278;
                NormalizedFreq2 = 0.28701;
                FreqDelta = 0.0001;// 0.0002;
                GraphFreqInterval = 0.001;
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
            // 入力導波路の長さ（ロッド数)
            const int rodCnt_inputWg = 1;
            // 共振器のY方向ロッド数
            const int rodCntY_cavity = 1;
            // 計算領域のX方向ロッド数
            int rodCntX = rodCnt_inputWg * 2 + rodCntDiscon + (rodCntDiscon - 1) + defectRodCnt_cavity;
            if (spacingRodCnt % 2 == 0)
            {
                rodCntX++;
            }
            // 計算領域のY方向ロッド数
            int rodCntY = rodCntHalf * 2 + defectRodCnt * 3 + spacingRodCnt * 4 + rodCntY_cavity * 2;
            // 上端Y座標
            double topY =  (rodCntHalf * 2 + defectRodCnt * 2 + spacingRodCnt * 2 + rodCntY_cavity) * rodDistanceY;
            // 下端Y座標
            double bottomY = topY - rodCntY * rodDistanceY;
            // 右端X座標
            double port2_X = rodCntX * rodDistanceX;

            IList<uint> rodLoopIds = new List<uint>();
            IList<uint> rodLoopIds_InputWg1 = new List<uint>();
            IList<uint> rodLoopIds_InputWg2 = new List<uint>();
            IList<uint> rodLoopIds_InputWg3 = new List<uint>();
            int ndivPlus = 0;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_rod_B3 = new List<uint>();
            IList<uint> id_e_rod_B4 = new List<uint>();
            IList<uint> id_e_rod_B5 = new List<uint>();
            IList<uint> id_e_rod_B6 = new List<uint>();
            IList<uint> id_e_F1 = new List<uint>();
            IList<uint> id_e_F2 = new List<uint>();
            IList<uint> id_e_F1_cavity = new List<uint>();
            IList<uint> id_e_F2_cavity = new List<uint>();
            IList<uint> id_e_F3_cavity = new List<uint>();
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
                    pts.Add(new CVector2D(inputWgLength, bottomY)); // 頂点4
                    pts.Add(new CVector2D(port2_X - inputWgLength, bottomY)); // 頂点5
                    pts.Add(new CVector2D(port2_X, bottomY)); // 頂点6
                    pts.Add(new CVector2D(port2_X, bottomY + WaveguideWidth)); // 頂点7
                    pts.Add(new CVector2D(port2_X - inputWgLength, bottomY + WaveguideWidth)); // 頂点8
                    pts.Add(new CVector2D(port2_X - inputWgLength, topY - WaveguideWidth)); // 頂点9
                    pts.Add(new CVector2D(port2_X, topY - WaveguideWidth)); // 頂点10
                    pts.Add(new CVector2D(port2_X, topY)); // 頂点11
                    pts.Add(new CVector2D(port2_X - inputWgLength, topY)); // 頂点12
                    pts.Add(new CVector2D(inputWgLength, topY)); // 頂点13
                    pts.Add(new CVector2D(inputWgLength, WaveguideWidth)); // 頂点14
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                }
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 14).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(9, 12).id_e_add;
                uint eIdAdd3 = cad2d.ConnectVertex_Line(5, 8).id_e_add;

                // 入出力導波路の周期構造境界上の頂点を追加
                IList<double> ys = new List<double>();
                IList<double> ys_rod = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                IList<uint> id_v_list_rod_B3 = new List<uint>();
                IList<uint> id_v_list_rod_B4 = new List<uint>();
                IList<uint> id_v_list_rod_B5 = new List<uint>();
                IList<uint> id_v_list_rod_B6 = new List<uint>();
                // 境界上にロッドのある格子
                // 境界上のロッドの頂点
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
                    double y0 = WaveguideWidth - i * rodDistanceY - 0.5 * rodDistanceY;
                    ys_rod.Add(y0);
                    for (int k = 1; k <= rodRadiusDiv; k++)
                    {
                        double y1 = y0 - k * rodRadius / rodRadiusDiv;
                        double y2 = y0 + k * rodRadius / rodRadiusDiv;
                        ys_rod.Add(y1);
                        ys_rod.Add(y2);
                    }
                }
                for (int i = 0; i < rodCntHalf; i++)
                {
                    if (i % 2 == (isShift180 ? 1 : 0)) continue;
                    double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - 0.5 * rodDistanceY;
                    ys_rod.Add(y0);
                    for (int k = 1; k <= rodRadiusDiv; k++)
                    {
                        double y1 = y0 - k * rodRadius / rodRadiusDiv;
                        double y2 = y0 + k * rodRadius / rodRadiusDiv;
                        ys_rod.Add(y1);
                        ys_rod.Add(y2);
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
                        ys.Add(y_divpt);
                    }
                }
                // 欠陥部
                for (int i = 0; i <= (defectRodCnt * ndivForOneLattice); i++)
                {
                    if (!isShift180 && (i == 0 || i == (defectRodCnt * ndivForOneLattice))) continue;
                    double y_divpt = rodDistanceY * (rodCntHalf + defectRodCnt) - i * (rodDistanceY / ndivForOneLattice);
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
                for (int boundaryIndex = 0; boundaryIndex < 6; boundaryIndex++)
                {
                    bool isInRod = false;
                    for (int i = 0; i < yAry.Length; i++)
                    {
                        uint id_e = 0;
                        double x1 = 0.0;
                        double y_pt = 0.0;
                        IList<uint> work_id_e_rod_B = null;
                        IList<uint> work_id_v_list_rod_B = null;
                        int yAryIndex = 0;

                        if (boundaryIndex == 0)
                        {
                            // 入力導波路 外側境界
                            id_e = 1;
                            x1 = 0.0;
                            y_pt = yAry[i];
                            yAryIndex = i;
                            work_id_e_rod_B = id_e_rod_B1;
                            work_id_v_list_rod_B = id_v_list_rod_B1;
                        }
                        else if (boundaryIndex == 1)
                        {
                            // 入力導波路 内側境界
                            id_e = 15;
                            x1 = inputWgLength;
                            y_pt = yAry[yAry.Length - 1 - i];
                            yAryIndex = yAry.Length - 1 - i;
                            work_id_e_rod_B = id_e_rod_B2;
                            work_id_v_list_rod_B = id_v_list_rod_B2;
                        }
                        else if (boundaryIndex == 2)
                        {
                            // ポート2出力導波路 外側境界
                            id_e = 10;
                            x1 = port2_X;
                            y_pt = yAry[yAry.Length - 1 - i] + topY - WaveguideWidth;
                            yAryIndex = yAry.Length - 1 - i;
                            work_id_e_rod_B = id_e_rod_B3;
                            work_id_v_list_rod_B = id_v_list_rod_B3;
                        }
                        else if (boundaryIndex == 3)
                        {
                            // ポート2出力導波路 内側境界
                            id_e = 16;
                            x1 = port2_X - inputWgLength;
                            y_pt = yAry[yAry.Length - 1 - i] + topY - WaveguideWidth;
                            yAryIndex = yAry.Length - 1 - i;
                            work_id_e_rod_B = id_e_rod_B4;
                            work_id_v_list_rod_B = id_v_list_rod_B4;
                        }
                        else if (boundaryIndex == 4)
                        {
                            // ポート3出力導波路 外側境界
                            id_e = 6;
                            x1 = port2_X;
                            y_pt = yAry[yAry.Length - 1 - i] + bottomY;
                            yAryIndex = yAry.Length - 1 - i;
                            work_id_e_rod_B = id_e_rod_B5;
                            work_id_v_list_rod_B = id_v_list_rod_B5;
                        }
                        else if (boundaryIndex == 5)
                        {
                            // ポート3出力導波路 内側境界
                            id_e = 17;
                            x1 = port2_X - inputWgLength;
                            y_pt = yAry[yAry.Length - 1 - i] + bottomY;
                            yAryIndex = yAry.Length - 1 - i;
                            work_id_e_rod_B = id_e_rod_B6;
                            work_id_v_list_rod_B = id_v_list_rod_B6;
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
                            if (Math.Abs(y_rod - yAry[yAryIndex]) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                            {
                                contains = true;
                                break;
                            }
                        }
                        if (contains)
                        {
                            work_id_v_list_rod_B.Add(id_v_add);

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
                }

                int bRodCntHalf = (isShift180 ? (int)((rodCntHalf + 1) / 2) : (int)((rodCntHalf) / 2));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B5.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B6.Count == bRodCntHalf * 2 * (rodRadiusDiv * 2 + 1));

                /////////////////////////////////////////////////////////////////////////////
                // ロッドを追加

                // 左のロッドを追加
                for (int colIndex = 0; colIndex < 4; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 左のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    double y_B = 0.0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    // 始点、終点が逆？
                    bool isReverse = false;
                    if (colIndex == 0)
                    {
                        // 入力境界 外側
                        x_B = 0.0;
                        y_B = WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B1;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                        isReverse = false;
                    }
                    else if (colIndex == 1)
                    {
                        // 入力境界 内側
                        x_B = inputWgLength;
                        y_B = WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                        isReverse = true;
                    }
                    else if (colIndex == 2)
                    {
                        // ポート2出力境界 内側
                        x_B = port2_X - inputWgLength;
                        y_B = topY;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                        isReverse = true;
                    }
                    else if (colIndex == 3)
                    {
                        // ポート3出力境界 内側
                        x_B = port2_X - inputWgLength;
                        y_B = bottomY + WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B6;
                        // 出力導波路領域
                        baseLoopId = 4;
                        inputWgNo = 3;
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
                            // 左のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        System.Diagnostics.Debug.Assert(false);
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
                                    int index_v0 = (0 + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = ((rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = ((rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v0 < 0)
                                    {
                                        System.Diagnostics.Debug.Assert(false);
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
                                double y0 = y_B - i * rodDistanceY - rodDistanceY * 0.5;
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
                                }
                                uint lId = 0;
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
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
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
                                        System.Diagnostics.Debug.Assert(false);
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
                                        System.Diagnostics.Debug.Assert(false);
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }

                                }
                                double x0 = x_B;
                                double y0 = (y_B - WaveguideWidth) + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
                                }
                                uint lId = 0;
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
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                            }
                        }
                    }
                }

                // 右のロッドを追加
                for (int colIndex = 0; colIndex < 5; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 右のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0.0;
                    double y_B = 0.0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    if (colIndex == 0)
                    {
                        // 入力境界 内側
                        x_B = inputWgLength;
                        y_B = WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                    }
                    else if (colIndex == 1)
                    {
                        // ポート2出力境界 内側
                        x_B = port2_X - inputWgLength;
                        y_B = topY;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    else if (colIndex == 2)
                    {
                        // ポート2出力境界 外側
                        x_B = port2_X;
                        y_B = topY;
                        work_id_v_list_rod_B = id_v_list_rod_B3;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else if (colIndex == 3)
                    {
                        // ポート3出力境界 内側
                        x_B = port2_X - inputWgLength;
                        y_B = bottomY + WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B6;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    else if (colIndex == 4)
                    {
                        // ポート3出力境界 外側
                        x_B = port2_X;
                        y_B = bottomY + WaveguideWidth;
                        work_id_v_list_rod_B = id_v_list_rod_B5;
                        // 出力導波路領域
                        baseLoopId = 4;
                        inputWgNo = 3;
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

                            // 右のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    int index_v0 = (work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = (work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = (work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v2 > work_id_v_list_rod_B.Count - 1)
                                    {
                                        System.Diagnostics.Debug.Assert(false);
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
                                    int index_v0 = (0 + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v1 = ((rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1));
                                    int index_v2 = ((rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1));
                                    if (index_v0 < 0)
                                    {
                                        System.Diagnostics.Debug.Assert(false);
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
                                double y0 = y_B - i * rodDistanceY - rodDistanceY * 0.5;
                                CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
                                uint lId = 0;
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
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
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
                                        System.Diagnostics.Debug.Assert(false);
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
                                        System.Diagnostics.Debug.Assert(false);
                                    }
                                    else
                                    {
                                        id_v0 = work_id_v_list_rod_B[index_v0];
                                        id_v1 = work_id_v_list_rod_B[index_v1];
                                        id_v2 = work_id_v_list_rod_B[index_v2];
                                    }
                                }
                                double x0 = x_B;
                                double y0 = (y_B - WaveguideWidth) + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint lId = 0;
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
                                rodLoopIds.Add(lId);
                                if (inputWgNo == 1)
                                {
                                    rodLoopIds_InputWg1.Add(lId);
                                }
                                else if (inputWgNo == 2)
                                {
                                    rodLoopIds_InputWg2.Add(lId);
                                }
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                            }
                        }
                    }
                }

                // 中央ロッド
                int periodCntInputWg1 = rodCnt_inputWg;
                int periodCntX = rodCntX;

                // 中央のロッド(上下の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1 = new List<uint>();
                IList<uint> id_v_list_F2 = new List<uint>();

                // 中央のロッド
                for (int col = 1; col <= (periodCntX * 2 - 1); col++)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (col == (periodCntX - periodCntInputWg1) * 2) continue; // 出力導波路内部境界  (既にロッド追加済み)

                    // 中央のロッド
                    int wgRodCnt = rodCntHalf * 2 + defectRodCnt;
                    int port1_stRodIndex = rodCntHalf * 2+ defectRodCnt * 2+ spacingRodCnt * 2 + rodCntY_cavity - wgRodCnt;
                    int port2_stRodIndex = (rodCntY - (rodCntHalf * 2 + defectRodCnt));
                    for (int i = 0; i < rodCntY; i++)
                    {
                        uint baseLoopId = 0;
                        int inputWgNo = 0;

                        if (col >= 0 && col < (periodCntInputWg1 * 2))
                        {
                            if (i >= port1_stRodIndex && i < (port1_stRodIndex + wgRodCnt))
                            {
                                baseLoopId = 1;
                                inputWgNo = 1;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (col >= (periodCntInputWg1 * 2 + 1) && col < (periodCntX - periodCntInputWg1) * 2)
                        {
                            baseLoopId = 2;
                            inputWgNo = 0;
                        }
                        else if (col >= ((periodCntX - periodCntInputWg1) * 2 + 1) && col < periodCntX * 2)
                        {
                            if (i >= 0 && i < wgRodCnt)
                            {
                                baseLoopId = 3;
                                inputWgNo = 2;
                            }
                            else if (i >= port2_stRodIndex)
                            {
                                baseLoopId = 4;
                                inputWgNo = 3;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        double rr = rodRadius;
                        int nr = rodRadiusDiv;
                        if (inputWgNo == 0)
                        {
                            nr = 2; // 内部の分割数調整
                        }

                        // ロッドの位置のオフセット
                        double ofs_x_rod = 0.0;
                        double ofs_y_rod = 0.0;

                        // 導波路欠陥部
                        if (col <= (rodCntX_Wg * 2))
                        {
                            if (i >= (port1_stRodIndex + rodCntHalf) && i < (port1_stRodIndex + rodCntHalf + defectRodCnt)) continue; // ポート1
                        }
                        if (col >= (periodCntX - rodCntX_Wg) * 2)
                        {
                            if (i >= rodCntHalf && i < (rodCntHalf + defectRodCnt)) continue; // ポート2
                            if (i >= (rodCntY - rodCntHalf - defectRodCnt) && i < (rodCntY - rodCntHalf)) continue; // ポート3
                        }

                        // 共振器
                        int col_cavity_min = (periodCntInputWg1 * 2 + rodCntDiscon * 2);
                        int col_cavity_max = (periodCntInputWg1 * 2 + rodCntDiscon * 2 + defectRodCnt_cavity * 2 - 2);
                        if (spacingRodCnt % 2 == 0)
                        {
                            col_cavity_min++;
                            col_cavity_max++;
                        }
                        if (col >= col_cavity_min && col <= col_cavity_max)
                        {
                            // 上側共振器
                            if (i >= (rodCntHalf + defectRodCnt + spacingRodCnt)
                                && i <= (rodCntHalf + defectRodCnt + spacingRodCnt + rodCntY_cavity - 1))
                            {
                                continue;
                            }
                            // 下側共振器
                            if (i >= (rodCntY - rodCntHalf - defectRodCnt - spacingRodCnt - rodCntY_cavity)
                                && i <= (rodCntY - rodCntHalf - defectRodCnt - spacingRodCnt - 1))
                            {
                                continue;
                            }
                        }
                        // 共振器の左右のロッドを外方向にずらす
                        if (isOpt)
                        {
                            if (defectRodCnt_cavity == 3)
                            {
                                // for defectRodCnt_cavity == 3
                                // 上側共振器
                                double ofs_ratio_cavity1 = 0.023256; // 10nm / 430 nm
                                double ofs_ratio_cavity2 = 0.046512; // 20nm / 430 nm
                                if (i >= (rodCntHalf + defectRodCnt + spacingRodCnt)
                                    && i <= (rodCntHalf + defectRodCnt + spacingRodCnt + -rodCntY_cavity - 1))
                                {
                                    if (col == (col_cavity_min - 2))
                                    {
                                        ofs_x_rod = -ofs_ratio_cavity1 * latticeA;
                                    }
                                    else if (col == (col_cavity_max + 2))
                                    {
                                        ofs_x_rod = ofs_ratio_cavity1 * latticeA;
                                    }
                                }                                
                                if (i >= (rodCntY - rodCntHalf - defectRodCnt - spacingRodCnt - rodCntY_cavity)
                                    && i <= (rodCntY - rodCntHalf - defectRodCnt - spacingRodCnt - 1))
                                {
                                    if (col == (col_cavity_min - 2))
                                    {
                                        ofs_x_rod = -ofs_ratio_cavity2 * latticeA;
                                    }
                                    else if (col == (col_cavity_max + 2))
                                    {
                                        ofs_x_rod = ofs_ratio_cavity2 * latticeA;
                                    }
                                }
                            }
                        }

                        if ((col % 2 == 1 && (Math.Abs(rodCntY - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (Math.Abs(rodCntY - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col + ofs_x_rod;
                            double y0 = topY - i * rodDistanceY - rodDistanceY * 0.5 + ofs_y_rod;
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rr, rodCircleDiv, nr);
                            rodLoopIds.Add(lId);
                            if (inputWgNo == 1)
                            {
                                rodLoopIds_InputWg1.Add(lId);
                            }
                            else if (inputWgNo == 2)
                            {
                                rodLoopIds_InputWg2.Add(lId);
                            }
                            else if (inputWgNo == 3)
                            {
                                rodLoopIds_InputWg3.Add(lId);
                            }
                        }
                    }
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド(空洞部分)(左の強制境界と交差する円)と境界の交点
                // 左の下側
                IList<uint> id_v_list_F1_cavity = new List<uint>();
                int rodCntY_F1_cavity = (rodCntHalf * 2 + defectRodCnt + spacingRodCnt * 2 + rodCntY_cavity) - (rodCntHalf * 2 + defectRodCnt);
                for (int i = (rodCntY_F1_cavity - 1); i >= 0; i--)
                {
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 0);
                    if (isRodLattice)
                    {
                        uint id_e = 3;
                        double x0 = inputWgLength;
                        double y0 = 0.0 - rodDistanceY * i - rodDistanceY * 0.5;
                        double x_cross = inputWgLength;
                        double[] y_cross_list = new double[3];
                        y_cross_list[0] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        y_cross_list[1] = y0; // 中心
                        y_cross_list[2] = Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        foreach (double y_cross in y_cross_list)
                        {
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F1_cavity.Add(id_v_add);
                            id_e_F1_cavity.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }
                // 中央のロッド (ベンド、左境界と接する半円)
                for (int i = 0; i < rodCntY_F1_cavity; i++)
                {
                    // 不連続領域
                    uint baseLoopId = 2;
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 0);
                    if (isRodLattice)
                    {
                        {
                            // 右の強制境界と交差するロッド
                            // 半円（右半分)を追加
                            double x0 = inputWgLength;
                            double y0 = 0.0 - rodDistanceY * i - rodDistanceY * 0.5;
                            int row2 = (rodCntY_F1_cavity - 1 - i) / 2;
                            System.Diagnostics.Debug.Assert(row2 >= 0);
                            uint id_v0 = id_v_list_F1_cavity[row2 * 3 + 0];
                            uint id_v1 = id_v_list_F1_cavity[row2 * 3 + 1];
                            uint id_v2 = id_v_list_F1_cavity[row2 * 3 + 2];
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
                                90.0,
                                true);
                            rodLoopIds.Add(lId);
                        }
                    }
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド(空洞部分)(左の強制境界と交差する円)と境界の交点
                // 左の上側
                IList<uint> id_v_list_F3_cavity = new List<uint>();
                int rodCntY_F3_cavity = (rodCntHalf * 2 + defectRodCnt + spacingRodCnt * 2 + rodCntY_cavity) - (rodCntHalf * 2 + defectRodCnt) + 1;
                for (int i = (rodCntY_F3_cavity - 1); i >= 0; i--)
                {
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 1);
                    if (isRodLattice)
                    {
                        uint id_e = 13;
                        double x0 = inputWgLength;
                        double y0 = topY - rodDistanceY * i - rodDistanceY * 0.5;
                        double x_cross = inputWgLength;
                        double[] y_cross_list = new double[3];
                        y_cross_list[0] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        y_cross_list[1] = y0; // 中心
                        y_cross_list[2] = Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        foreach (double y_cross in y_cross_list)
                        {
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F3_cavity.Add(id_v_add);
                            id_e_F3_cavity.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }
                // 中央のロッド (ベンド、左境界と接する半円)
                for (int i = 0; i < rodCntY_F3_cavity; i++)
                {
                    // 不連続領域
                    uint baseLoopId = 2;
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 1);
                    if (isRodLattice)
                    {
                        {
                            // 右の強制境界と交差するロッド
                            // 半円（右半分)を追加
                            double x0 = inputWgLength;
                            double y0 = topY - rodDistanceY * i - rodDistanceY * 0.5;
                            int row2 = (rodCntY_F3_cavity - 1 - i) / 2;
                            System.Diagnostics.Debug.Assert(row2 >= 0);
                            uint id_v0 = id_v_list_F3_cavity[row2 * 3 + 0];
                            uint id_v1 = id_v_list_F3_cavity[row2 * 3 + 1];
                            uint id_v2 = id_v_list_F3_cavity[row2 * 3 + 2];
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
                                90.0,
                                true);
                            rodLoopIds.Add(lId);
                        }
                    }
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド(ベンド部分)(右の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F2_cavity = new List<uint>();
                int rodCntY_F2_cavity = rodCntY - (rodCntHalf * 2 + defectRodCnt) * 2;
                for (int i = 0; i <= (rodCntY_F2_cavity - 1); i++)
                {
                    // 右の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 0);
                    if (isRodLattice)
                    {
                        uint id_e = 8;
                        double x0 = port2_X - inputWgLength;
                        double y0 = topY - WaveguideWidth - rodDistanceY * i - rodDistanceY * 0.5;
                        double x_cross = port2_X - inputWgLength;
                        double[] y_cross_list = new double[3];
                        y_cross_list[0] = Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        y_cross_list[1] = y0; // 中心
                        y_cross_list[2] = -1.0 * Math.Sqrt(rodRadius * rodRadius - (x_cross - x0) * (x_cross - x0)) + y0; // 交点
                        foreach (double y_cross in y_cross_list)
                        {
                            CCadObj2D.CResAddVertex resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_cross, y_cross));
                            uint id_v_add = resAddVertex.id_v_add;
                            uint id_e_add = resAddVertex.id_e_add;
                            System.Diagnostics.Debug.Assert(id_v_add != 0);
                            System.Diagnostics.Debug.Assert(id_e_add != 0);
                            id_v_list_F2_cavity.Add(id_v_add);
                            id_e_F2_cavity.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }
                // 中央のロッド (ベンド、右境界と接する半円)
                for (int i = (rodCntY_F2_cavity - 1); i >= 0; i--)
                {
                    // 不連続領域
                    uint baseLoopId = 2;
                    // 右の強制境界と交差するロッド
                    bool isRodLattice = false;
                    isRodLattice = (i % 2 == 0);
                    if (isRodLattice)
                    {
                        {
                            // 右の強制境界と交差するロッド
                            // 半円（左半分)を追加
                            double x0 = port2_X - inputWgLength;
                            double y0 = topY - WaveguideWidth - rodDistanceY * i - rodDistanceY * 0.5;
                            int row2 = i / 2;
                            System.Diagnostics.Debug.Assert(row2 >= 0);
                            uint id_v0 = id_v_list_F2_cavity[row2 * 3 + 0];
                            uint id_v1 = id_v_list_F2_cavity[row2 * 3 + 1];
                            uint id_v2 = id_v_list_F2_cavity[row2 * 3 + 2];
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
                                270.0,
                                true);
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
                    foreach (uint eId in id_e_rod_B3)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B4)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B5)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B6)
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
                uint[] loopId_cad_list = new uint[4 + rodLoopIds.Count];
                loopId_cad_list[0] = 1;
                loopId_cad_list[1] = 2;
                loopId_cad_list[2] = 3;
                loopId_cad_list[3] = 4;
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    loopId_cad_list[i + 4] = rodLoopIds[i];
                }
                int[] mediaIndex_list = new int[4 + rodLoopIds.Count];
                for (int i = 0; i < 3; i++)
                {
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    mediaIndex_list[i + 4] = Medias.IndexOf(mediaCore);
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
            const uint portCnt = 3;
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
                WgPortInfoList[2].IncidentModeIndex = incidentModeIndex;
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
                uint[] eId_cad_list = new uint[11 + id_e_F1.Count + id_e_F2.Count + id_e_F1_cavity.Count + id_e_F2_cavity.Count + id_e_F3_cavity.Count];
                eId_cad_list[0] = 2;
                eId_cad_list[1] = 3;
                eId_cad_list[2] = 4;
                eId_cad_list[3] = 5;
                eId_cad_list[4] = 7;
                eId_cad_list[5] = 8;
                eId_cad_list[6] = 9;
                eId_cad_list[7] = 11;
                eId_cad_list[8] = 12;
                eId_cad_list[9] = 13;
                eId_cad_list[10] = 14;
                for (int i = 0; i < id_e_F1.Count; i++)
                {
                    eId_cad_list[11 + i] = id_e_F1[i];
                }
                for (int i = 0; i < id_e_F2.Count; i++)
                {
                    eId_cad_list[11 + id_e_F1.Count + i] = id_e_F2[i];
                }
                for (int i = 0; i < id_e_F1_cavity.Count; i++)
                {
                    eId_cad_list[11 + id_e_F1.Count + id_e_F2.Count + i] = id_e_F1_cavity[i];
                }
                for (int i = 0; i < id_e_F2_cavity.Count; i++)
                {
                    eId_cad_list[11 + id_e_F1.Count + id_e_F2.Count + id_e_F1_cavity.Count + i] = id_e_F2_cavity[i];
                }
                for (int i = 0; i < id_e_F3_cavity.Count; i++)
                {
                    eId_cad_list[11 + id_e_F1.Count + id_e_F2.Count + id_e_F1_cavity.Count + id_e_F2_cavity.Count + i] = id_e_F3_cavity[i];
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
                    eId_cad_list[0] = 10;
                    work_id_e_rod_B = id_e_rod_B3;
                }
                else if (portIndex == 2)
                {
                    eId_cad_list[0] = 6;
                    work_id_e_rod_B = id_e_rod_B5;
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
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) * 3 - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) * 5 - (i - 1));
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
                else if (portIndex == 2)
                {
                    loopId_cad_list = new uint[1 + rodLoopIds_InputWg3.Count];
                    loopId_cad_list[0] = 4;
                    for (int i = 0; i < rodLoopIds_InputWg3.Count; i++)
                    {
                        loopId_cad_list[i + 1] = rodLoopIds_InputWg3[i];
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
                else if (portIndex == 2)
                {
                    mediaIndex_list = new int[1 + rodLoopIds_InputWg3.Count];
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    for (int i = 0; i < rodLoopIds_InputWg3.Count; i++)
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
                    eId_cad_list[0] = 15;
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
                    eId_cad_list[0] = 16;
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
                else if (portIndex == 2)
                {
                    eId_cad_list[0] = 17;
                    work_id_e_rod_B = id_e_rod_B6;
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
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) * 4 - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(17 + (ndivPlus - 1) * 6 - (i - 1));
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
                double ymax = WaveguideWidth;
                if (portIndex == 0)
                {
                    ymax = WaveguideWidth;
                }
                else if (portIndex == 1)
                {
                    ymax = topY;
                }
                else if (portIndex == 2)
                {
                    ymax = bottomY + WaveguideWidth;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                {
                    // チャンネル１
                    IList<uint> portNodes = new List<uint>();
                    for (int i = 0; i < no_c_all.Length; i++)
                    {
                        // 座標からチャンネル(欠陥部)を判定する
                        double[] coord = coord_c_all[i];
                        if (coord[1] >= (ymax - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY) && coord[1] <= (ymax - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)) // air hole
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
