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
    class Problem12
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC導波路 60°三角形格子 ダブルベンド
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
            double minEffN = 0.0;
            // 最大屈折率
            double maxEffN = 0.0;

            // 考慮する波数ベクトルの最小値
            //double minWaveNum = 0.0;
            double minWaveNum = 0.5; // for latticeTheta = 60 r = 0.30a air hole
            // 考慮する波数ベクトルの最大値
            //double maxWaveNum = 0.5;
            double maxWaveNum = 1.0; // for latticeTheta = 60 r = 0.30a air hole

            // 入出力導波路が同じ？
            //IsInoutWgSame = true; // 同じ
            //IsInoutWgSame = false; // 同じでない
            // 磁気壁を使用する？
            bool isMagneticWall = false; // 電気壁を使用する
            //bool isMagneticWall = true; // 磁気壁を使用する
            // 空孔？
            //bool isAirHole = false; // dielectric rod
            bool isAirHole = true; // air hole
            // 周期を180°ずらす
            bool isShift180 = false;
            //bool isShift180 = true;
            // Y方向周期をずらす
            bool isShiftY = false; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            //bool isShiftY = true; // for latticeTheta = 60 r = 0.35a air hole defectRodCnt_Bend == 1
            //bool isShiftY = true; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            // ロッドの数(半分)
            //const int rodCntHalf = 5; // for latticeTheta = 60 r = 0.30a air hole
            const int rodCntHalf = 3;
            System.Diagnostics.Debug.Assert(rodCntHalf % 2 == 1); // 奇数を指定（図面の都合上)
            // 欠陥ロッド数
            // for latticeTheta = 60 r = 0.30a dielectric rod
            const int defectRodCnt = 1;
            // 三角形格子の内角
            double latticeTheta = 60.0;
            // ロッドの半径
            //double rodRadiusRatio = 0.35; // for latticeTheta = 60 r = 0.35a air hole defectRodCnt_Bend == 1
            //double rodRadiusRatio = 0.28; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            double rodRadiusRatio = 0.344; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            // ロッドの比誘電率
            //double rodEps = 2.80 * 2.80; // for latticeTheta = 60 r = 0.35a air hole defectRodCnt_Bend == 1
            //double rodEps = 2.80 * 2.80; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            double rodEps = 2.95 * 2.95; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            // 1格子当たりの分割点の数
            //const int ndivForOneLattice = 9; // for latticeTheta = 60 r = 0.35a air hole
            //const int ndivForOneLattice = 8; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            const int ndivForOneLattice = 8;// 8;// 9;
            // ロッド円周の分割数
            //const int rodCircleDiv = 12; // for latticeTheta = 60 r = 0.30a air hole
            const int rodCircleDiv = 12;
            // ロッドの半径の分割数
            //const int rodRadiusDiv = 4; // for latticeTheta = 60 r = 0.30a air hole
            const int rodRadiusDiv = 4;
            // 導波路不連続領域の長さ
            //const int rodCntDiscon = 0; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3 
            //const int rodCntDiscon = 1; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            const int rodCntDiscon = 1;
            // ベンド部のロッド数（半分）
            const int rodCntHalf_Bend = 5;
            // ベンド部の欠陥ロッド数
            //const int defectRodCnt_Bend = 1; // for latticeTheta = 60 r = 0.35a air hole defectRodCnt_Bend == 1
            //const int defectRodCnt_Bend = 3; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            const int defectRodCnt_Bend = 5; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            // ベンド部の長さ
            //const int rodCntY_Bend = 9; // for latticeTheta = 60 r = 0.35a air hole defectRodCnt_Bend == 1
            //const int rodCntY_Bend = 11;//19; // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            //const int rodCntY_Bend = 7; // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            const int rodCntY_Bend = 7;

            // 格子の数
            const int latticeCnt = rodCntHalf * 2 + defectRodCnt;
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
            // 入出力導波路の周期構造部分の長さ
            double inputWgLength = rodDistanceX;
            // 入出力導波路の不連続部の長さ
            double disconLength = rodCntDiscon * rodDistanceX;
            // ベンド部のX方向長さ
            double bendLengthX = (rodCntHalf_Bend * 2 + defectRodCnt_Bend) * (rodDistanceX * 0.5);
            // ベンド部のY方向長さ
            double bendLengthY = (rodCntY_Bend + rodCntHalf * 2 + defectRodCnt * 2) * rodDistanceY;
            // メッシュのサイズ
            double meshL = 1.05 * WaveguideWidth / (latticeCnt * ndivForOneLattice);

            // for latticeTheta = 60 r = 0.35a air hole
            //NormalizedFreq1 = 0.294;//0.295;// 0.280;
            //NormalizedFreq2 = 0.315;//0.320;// 0.3301;
            //FreqDelta = 0.001;
            //GraphFreqInterval = 0.002;// 0.010;
            // for latticeTheta = 60 r = 0.28a air hole defectRodCnt_Bend == 3
            //NormalizedFreq1 = 0.260;
            //NormalizedFreq2 = 0.2901;
            //FreqDelta = 0.0005;
            //GraphFreqInterval = 0.005;
            // for latticeTheta = 60 r = 0.344a air hole defectRodCnt_Bend == 5
            NormalizedFreq1 = 0.265;
            NormalizedFreq2 = 0.3201;
            FreqDelta = 0.0005;
            GraphFreqInterval = 0.005;

            //minEffN = 0.0;
            //maxEffN = 0.5 * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            //minEffN = minWaveNum * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            //maxEffN = maxWaveNum * 1.0 / (NormalizedFreq1 * (periodicDistance / latticeA));
            //minEffN_port2 = minWaveNum_port2 * 1.0 / (NormalizedFreq1 * (periodicDistance_port2 / latticeA));
            //maxEffN_port2 = maxWaveNum_port2 * 1.0 / (NormalizedFreq1 * (periodicDistance_port2 / latticeA));
            if (isAirHole)
            {
                minEffN = 0.0;// 1.0;//0.0;
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
            //WaveModeDv = WaveModeDV.TE; // dielectric rod
            if (isAirHole)
            {
                WaveModeDv = WgUtil.WaveModeDV.TM; // air hole
                //isMagneticWall = true;
            }
            else
            {
                WaveModeDv = WgUtil.WaveModeDV.TE; // dielectric rod
                isMagneticWall = false;
                incidentModeIndex = 0;
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
            const uint portCnt = 2;

            // Cad
            IList<uint> rodLoopIds = new List<uint>();
            IList<uint> rodLoopIds_InputWg1 = new List<uint>();
            IList<uint> rodLoopIds_InputWg2 = new List<uint>();
            int ndivPlus_port1 = 0;
            int ndivPlus_port2 = 0;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_rod_B3 = new List<uint>();
            IList<uint> id_e_rod_B4 = new List<uint>();
            IList<uint> id_e_F1 = new List<uint>();
            IList<uint> id_e_F2 = new List<uint>();
            IList<uint> id_e_F1_Bend = new List<uint>();
            IList<uint> id_e_F2_Bend = new List<uint>();

            if (!isShiftY)
            {
                System.Diagnostics.Debug.Assert(rodCntDiscon > 0);
            }
            // ベンド下側角
            double bendX1 = inputWgLength + disconLength + (rodDistanceX * 0.5) + bendLengthX;
            if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
            {
                bendX1 += rodDistanceX * 0.5;
            }
            double bendY1 = 0.0;
            // 出力部境界
            double port2_X = bendX1 + disconLength + inputWgLength;
            if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
            {
                port2_X += rodDistanceX * 0.5;
            }
            double port2_Y = bendY1 + bendLengthY;
            // ベンド部上側角
            double bendX2 = inputWgLength + disconLength;
            if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
            {
                bendX2 += rodDistanceX * 0.5;
            }

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
                    pts.Add(new CVector2D(bendX1, bendY1)); // 頂点4
                    pts.Add(new CVector2D(bendX1, port2_Y - WaveguideWidth)); // 頂点5
                    pts.Add(new CVector2D(port2_X - inputWgLength, port2_Y - WaveguideWidth)); // 頂点6
                    pts.Add(new CVector2D(port2_X, port2_Y - WaveguideWidth)); // 頂点7
                    pts.Add(new CVector2D(port2_X, port2_Y)); // 頂点8
                    pts.Add(new CVector2D(port2_X - inputWgLength, port2_Y)); // 頂点9
                    pts.Add(new CVector2D(bendX2, port2_Y)); // 頂点10
                    pts.Add(new CVector2D(bendX2, WaveguideWidth)); // 頂点11
                    pts.Add(new CVector2D(inputWgLength, WaveguideWidth)); // 頂点12
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                }
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 12).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(6, 9).id_e_add;

                // 入出力導波路の周期構造境界上の頂点を追加
                IList<double> ys_port1 = new List<double>();
                IList<double> ys_rod_port1 = new List<double>();
                IList<double> ys_port2 = new List<double>();
                IList<double> ys_rod_port2 = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                IList<uint> id_v_list_rod_B3 = new List<uint>();
                IList<uint> id_v_list_rod_B4 = new List<uint>();
                
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    int cur_rodCntHalf = 0;
                    int cur_defectRodCnt = 0;
                    int cur_ndivForOneLattice = 0;
                    double cur_WaveguideWidth = 0.0;
                    double cur_rodDistanceY = 0.0;
                    IList<double> ys = null;
                    IList<double> ys_rod = null;
                    if (portIndex == 0)
                    {
                        cur_rodCntHalf = rodCntHalf;
                        cur_defectRodCnt = defectRodCnt;
                        cur_ndivForOneLattice = ndivForOneLattice;
                        cur_WaveguideWidth = WaveguideWidth;
                        cur_rodDistanceY = rodDistanceY;
                        ys = ys_port1;
                        ys_rod = ys_rod_port1;
                        System.Diagnostics.Debug.Assert(ys.Count == 0);
                        System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                    }
                    else if (portIndex == 1)
                    {
                        cur_rodCntHalf = rodCntHalf;
                        cur_defectRodCnt = defectRodCnt;
                        cur_ndivForOneLattice = ndivForOneLattice;
                        cur_WaveguideWidth = WaveguideWidth;
                        cur_rodDistanceY = rodDistanceY;
                        ys = ys_port2;
                        ys_rod = ys_rod_port2;
                        System.Diagnostics.Debug.Assert(ys.Count == 0);
                        System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    
                    // 境界上にロッドのある格子
                    // 境界上のロッドの頂点
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if ((cur_rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
                        double y0 = cur_WaveguideWidth - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY;
                        ys_rod.Add(y0);
                        for (int k = 1; k <= rodRadiusDiv; k++)
                        {
                            ys_rod.Add(y0 - k * rodRadius / rodRadiusDiv);
                            ys_rod.Add(y0 + k * rodRadius / rodRadiusDiv);
                        }
                    }
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 1 : 0)) continue;
                        double y0 = cur_rodDistanceY * cur_rodCntHalf - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY;
                        ys_rod.Add(y0);
                        for (int k = 1; k <= rodRadiusDiv; k++)
                        {
                            ys_rod.Add(y0 - k * rodRadius / rodRadiusDiv);
                            ys_rod.Add(y0 + k * rodRadius / rodRadiusDiv);
                        }
                    }
                    foreach (double y_rod in ys_rod)
                    {
                        ys.Add(y_rod);
                    }

                    // 境界上のロッドの外の頂点はロッドから少し離さないとロッドの追加で失敗するのでマージンをとる
                    double radiusMargin = cur_rodDistanceY * 0.01;
                    // 境界上にロッドのある格子
                    // ロッドの外
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if ((cur_rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)) continue;
                        for (int k = 1; k <= (cur_ndivForOneLattice - 1); k++)
                        {
                            double y_divpt = cur_WaveguideWidth - i * cur_rodDistanceY - k * (cur_rodDistanceY / cur_ndivForOneLattice);
                            double y_min_rod = cur_WaveguideWidth - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY - rodRadius - radiusMargin;
                            double y_max_rod = cur_WaveguideWidth - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY + rodRadius + radiusMargin;
                            if (y_divpt < y_min_rod || y_divpt > y_max_rod)
                            {
                                ys.Add(y_divpt);
                            }
                        }
                    }
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 1 : 0)) continue;
                        for (int k = 1; k <= (cur_ndivForOneLattice - 1); k++)
                        {
                            double y_divpt = cur_rodDistanceY * cur_rodCntHalf - i * cur_rodDistanceY - k * (cur_rodDistanceY / cur_ndivForOneLattice);
                            double y_min_rod = cur_rodDistanceY * cur_rodCntHalf - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY - rodRadius - radiusMargin;
                            double y_max_rod = cur_rodDistanceY * cur_rodCntHalf - i * cur_rodDistanceY - 0.5 * cur_rodDistanceY + rodRadius + radiusMargin;
                            if (y_divpt < y_min_rod || y_divpt > y_max_rod)
                            {
                                ys.Add(y_divpt);
                            }
                        }
                    }

                    // 境界上にロッドのない格子
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if ((cur_rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1)) continue;
                        for (int k = 0; k <= cur_ndivForOneLattice; k++)
                        {
                            if (i == 0 && k == 0) continue;
                            double y_divpt = cur_WaveguideWidth - i * cur_rodDistanceY - k * (cur_rodDistanceY / cur_ndivForOneLattice);
                            ys.Add(y_divpt);
                        }
                    }
                    for (int i = 0; i < cur_rodCntHalf; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1)) continue;
                        for (int k = 0; k <= cur_ndivForOneLattice; k++)
                        {
                            if (i == (cur_rodCntHalf - 1) && k == cur_ndivForOneLattice) continue;
                            double y_divpt = cur_rodDistanceY * cur_rodCntHalf - i * cur_rodDistanceY - k * (cur_rodDistanceY / cur_ndivForOneLattice);
                            ys.Add(y_divpt);
                        }
                    }
                    // 欠陥部
                    for (int i = 0; i <= (cur_defectRodCnt * cur_ndivForOneLattice); i++)
                    {
                        if (!isShift180 && (i == 0 || i == (cur_defectRodCnt * cur_ndivForOneLattice))) continue;
                        double y_divpt = cur_rodDistanceY * (cur_rodCntHalf + cur_defectRodCnt) - i * (cur_rodDistanceY / cur_ndivForOneLattice);
                        ys.Add(y_divpt);
                    }

                    // 昇順でソート
                    double[] yAry = ys.ToArray();
                    Array.Sort(yAry);
                    int cur_ndivPlus = 0;
                    cur_ndivPlus = yAry.Length + 1;
                    if (portIndex == 0)
                    {
                        ndivPlus_port1 = cur_ndivPlus;
                    }
                    else if (portIndex == 1)
                    {
                        ndivPlus_port2 = cur_ndivPlus;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    // yAryは昇順なので、yAryの並びの順に追加すると境界1上を逆方向に移動することになる
                    //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                    // 入力導波路 外側境界
                    // 入力導波路 内部側境界
                    // 出力導波路 外側境界
                    // 出力導波路 内部側境界
                    for (int boundaryIndex = 0; boundaryIndex < 2;  boundaryIndex++)
                    {
                        bool isInRod = false;
                        for (int i = 0; i < yAry.Length; i++)
                        {
                            uint id_e = 0;
                            double x_pt = 0.0;
                            double y_pt = 0.0;
                            
                            IList<uint> work_id_e_rod_B = null;
                            IList<uint> work_id_v_list_rod_B = null;
                            int yAryIndex = 0;
                            if (portIndex == 0 && boundaryIndex == 0)
                            {
                                // 入力導波路 外側境界
                                id_e = 1;
                                x_pt = 0.0;
                                y_pt = yAry[i];
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B1;
                                work_id_v_list_rod_B = id_v_list_rod_B1;
                            }
                            else if (portIndex == 0 && boundaryIndex == 1)
                            {
                                // 入力導波路 内側境界
                                id_e = 13;
                                x_pt = inputWgLength;
                                y_pt = yAry[yAry.Length - 1 - i];
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B2;
                                work_id_v_list_rod_B = id_v_list_rod_B2;
                            }
                            else if (portIndex == 1 && boundaryIndex == 0)
                            {
                                // 出力導波路 外側境界
                                id_e = 7;
                                x_pt = port2_X - inputWgLength;
                                y_pt = port2_Y - WaveguideWidth + yAry[yAry.Length - 1 - i];
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B3;
                                work_id_v_list_rod_B = id_v_list_rod_B3;
                            }
                            else if (portIndex == 1 && boundaryIndex == 1)
                            {
                                // 出力導波路 内側境界
                                id_e = 14;
                                x_pt = port2_X;
                                y_pt = port2_Y - WaveguideWidth + yAry[yAry.Length - 1 - i];
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B4;
                                work_id_v_list_rod_B = id_v_list_rod_B4;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                            
                            CCadObj2D.CResAddVertex resAddVertex = null;
                            resAddVertex = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e, new CVector2D(x_pt, y_pt));
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
                }
                int bRodCntHalf_port1 = (isShift180 ? (int)((rodCntHalf + 1) / 2) : (int)((rodCntHalf) / 2));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));

                int rodCntHalf_port2 = rodCntHalf;
                int bRodCntHalf_port2 = (isShift180 ? (int)((rodCntHalf + 1) / 2) : (int)((rodCntHalf) / 2));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == bRodCntHalf_port2 * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == bRodCntHalf_port2 * 2 * (rodRadiusDiv * 2 + 1));

                // ロッドを追加
                /////////////////////////////////////////////////////////////
                // 入力導波路側ロッド
                // 左のロッドを追加
                for (int colIndex = 0; colIndex < 2; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 左のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

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
                            int i2 = bRodCntHalf_port1 - 1 - (int)((rodCntHalf - 1 - i) / 2);
                            // 左のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                else
                                {
                                    id_v0 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
                                }
                                uint lId = WgCadUtil.AddLeftRod(
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
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - 1 - i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                else
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint work_id_v0 = id_v0;
                                uint work_id_v2 = id_v2;
                                if (isReverse)
                                {
                                    work_id_v0 = id_v2;
                                    work_id_v2 = id_v0;
                                }
                                uint lId = WgCadUtil.AddLeftRod(
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
                            }
                        }
                    }
                }
                // 右のロッドを追加
                for (int colIndex = 0; colIndex < 1; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 右のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    if (colIndex == 0)
                    {
                        // 入力境界 内側
                        x_B = inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = bRodCntHalf_port1 - 1 - (int)((rodCntHalf - 1 - i) / 2);
                            // 右のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                if (work_id_v_list_rod_B == id_v_list_rod_B1)
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count - 1 - i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                else
                                {
                                    id_v0 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                                CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
                                uint lId = WgCadUtil.AddRightRod(
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
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv * 2 + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - (rodRadiusDiv + 1) - i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 - 1 - i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                else
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint lId = WgCadUtil.AddRightRod(
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
                            }
                        }
                    }
                }
                
                // 中央のロッド (入力導波路)
                int periodCntInputWg1 = 1;
                int periodCntX_port1 = periodCntInputWg1 + rodCntDiscon;
                for (int col = 1; col <= (periodCntX_port1 * 2 + 1); col++)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                    }
                    else
                    {
                        if (col > (periodCntX_port1 * 2)) continue;
                    }
                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    if (col >= 0 && col < (periodCntInputWg1 * 2))
                    {
                        baseLoopId = 1;
                        inputWgNo = 1;
                    }
                    else
                    {
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    // 中央のロッド
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if ((col % 2 == 1 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
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
                    // 中央のロッド
                    for (int i = 0; i < rodCntHalf; i++)
                    {
                        if ((col % 2 == 1 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && ((rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = rodCntHalf * rodDistanceY - i * rodDistanceY - rodDistanceY * 0.5;
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

                // ベンド部
                int periodCntX_Bend = (rodCntHalf_Bend * 2 + defectRodCnt_Bend) / 2;
                int rodCntY_All_Bend = rodCntHalf * 2 + defectRodCnt * 2 + rodCntY_Bend;
                for (int col = (periodCntX_port1 * 2 + 1); col <= (periodCntX_port1 * 2 + periodCntX_Bend * 2 + 2); col++)
                {
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                        if (col < (periodCntX_port1 * 2 + 2)) continue;
                    }
                    else
                    {
                        if (col > (periodCntX_port1 * 2 + periodCntX_Bend * 2 + 1)) continue;
                    }

                    uint baseLoopId = 2;

                    // 中央のロッド
                    for (int i = 0; i < rodCntY_All_Bend; i++)
                    {
                        // ロッドの半径
                        double rr = rodRadius;
                        // ロッドの半径方向分割数
                        const int nr_nearWg = 3;// rodRadiusDiv;
                        const int nr_farFromWg = 2;
                        // ロッドの半径方向分割数
                        int nr = nr_farFromWg;
                        // ロッドの周方向分割数
                        int nc = rodCircleDiv;
                        // ずらす距離
                        double ofs_x_rod = 0.0;
                        double ofs_y_rod = 0.0;

                        int work_col = col;
                        if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                        {
                            work_col -= 1;
                        }
                        // 出力導波路
                        if ((work_col >= (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1))
                            && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                        {
                            if (defectRodCnt_Bend == 1)
                            {
                                if (isShiftY
                                    && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1))
                                {
                                    // 結合部
                                    rr = 0.16 * latticeA;
                                    nr = nr_nearWg;
                                }
                                else if (!isShiftY
                                    && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 2))
                                {
                                    // ベンド部共振器
                                    nr = nr_nearWg;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (defectRodCnt_Bend == 3)
                            {
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        // 入力導波路
                        if ((work_col <= (periodCntX_port1 * 2 + rodCntHalf_Bend))
                            && (i >= (rodCntY_All_Bend - rodCntHalf - defectRodCnt) && i <= (rodCntY_All_Bend - rodCntHalf - 1)))
                        {
                            if (defectRodCnt_Bend == 1)
                            {
                                if (isShiftY
                                    && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend))
                                {
                                    // 結合部
                                    rr = 0.16 * latticeA;
                                    nr = nr_nearWg;
                                }
                                else if (!isShiftY
                                    && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend - 1))
                                {
                                    // ベンド部共振器
                                    nr = nr_nearWg;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (defectRodCnt_Bend == 3)
                            {
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        // 中央導波路
                        if ((work_col >= (periodCntX_port1 * 2 + rodCntHalf_Bend + 1) && work_col <= (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend))
                            && (i >= (rodCntHalf) && i <= (rodCntY_All_Bend - rodCntHalf - 1)))
                        {
                            continue;
                        }

                        if (defectRodCnt_Bend == 5 && !isShiftY)
                        {
                            // 広帯域構造 ベンド部に円弧エアホールと重なるロッドを除去
                            // 出力部
                            if (i == (rodCntHalf - 1)
                                && (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend - 3)
                                    || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend - 1)
                                    /*|| work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1)*/)
                                )
                            {
                                continue;
                            }
                            // 入力部
                            if (i == (rodCntY_All_Bend - rodCntHalf)
                                && (/*work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend)
                                    ||*/ work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + 2)
                                    || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + 4))
                                )
                            {
                                continue;
                            }
                            // 右
                            if (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1)
                                && (i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 1)
                                    || i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 3))
                                )
                            {
                                continue;
                            }
                            if (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 2)
                                && i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 2))
                            {
                                continue;
                            }
                            // 左
                            if (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend)
                                && (i == (rodCntHalf + defectRodCnt)
                                    || i == (rodCntHalf + defectRodCnt + 2))
                                )
                            {
                                continue;
                            }
                            if (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend - 1)
                                && i == (rodCntHalf + defectRodCnt + 1))
                            {
                                continue;
                            }
                        }

                        // 導波路上下のロッドだけ半径方向分割数を指定する
                        //  出力導波路上下
                        if (work_col >= (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1)
                            && (i == (rodCntHalf - 1) || i == (rodCntHalf + defectRodCnt)))
                        {
                            nr = nr_nearWg;
                        }
                        //  入力導波路上下
                        if ((work_col <= (periodCntX_port1 * 2 + rodCntHalf_Bend))
                            && (i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 1) || i == (rodCntY_All_Bend - rodCntHalf)))
                        {
                            nr = nr_nearWg;
                        }
                        //  中央導波路上下
                        if ((work_col >= (periodCntX_port1 * 2 + rodCntHalf_Bend) && work_col <= (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1))
                            && (i == (rodCntHalf + defectRodCnt - 2) || i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt+ 1)))
                        {
                            nr = nr_nearWg;
                        }
                        //  中央導波路左右
                        if ((work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend)
                                || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend - 1)
                                || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1)
                                || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 2)
                                )
                            && (i >= rodCntHalf && i <= (rodCntY_All_Bend - rodCntHalf - defectRodCnt)))
                        {
                            if (defectRodCnt_Bend == 3 && isShiftY)
                            {
                                nr = nr_nearWg;

                                /*
                                //double work_r1 = 0.12 * latticeA; // r1 = 52 nm (a : 430 nm)
                                double work_r1 = 0.116 * latticeA; // r1 = 50 nm (a : 430 nm)
                                int work_nr1 = 1;
                                //double work_r2 = 0.28 * latticeA;  // r2 : 120 nm (a : 430 nm)
                                //double work_r2 = 0.30 * latticeA;  // r2 : 130 nm (a : 430 nm)
                                //double work_r2 = 0.35 * latticeA;  // r2 : 150 nm (a : 430 nm)
                                //double work_r2 = 0.40 * latticeA;  // r2 : 170 nm (a : 430 nm)
                                //double work_r2 = 0.44 * latticeA;  // r2 : 190 nm (a : 430 nm)
                                double work_r2 = 0.44 * latticeA;
                                int work_nr2 = 4;
                                // 中央導波路の広帯域化
                                if (i == (rodCntHalf + defectRodCnt) && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 2))
                                {
                                    rr = work_r1;
                                    nr = work_nr1;
                                }
                                else if (i == (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 1) && work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend - 1))
                                {
                                    rr = work_r1;
                                    nr = work_nr1;
                                }
                                else if (i >= (rodCntHalf + defectRodCnt + 1) && i <= (rodCntY_All_Bend - rodCntHalf - defectRodCnt - 2))
                                {
                                    if (work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend)
                                        || work_col == (periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend + 1))
                                    {
                                        rr = work_r1;
                                        nr = work_nr1;
                                    }
                                    else
                                    {
                                        rr = work_r2;
                                        nr = work_nr2;
                                    }
                                }
                                 */
                            }
                            else
                            {
                                nr = nr_nearWg;
                            }
                        }

                        if ((col % 2 == 1 && ((rodCntY_All_Bend - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && ((rodCntY_All_Bend - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = port2_Y - i * rodDistanceY - rodDistanceY * 0.5;
                            //uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                            x0 += ofs_x_rod;
                            y0 += ofs_y_rod;
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rr, nc, nr);
                            rodLoopIds.Add(lId);
                        }
                    }
                }

                /////////////////////////////////////////////////////////////
                // 出力導波路側ロッド
                // 右のロッドを追加
                for (int colIndex = 0; colIndex < 2; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 右のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    if (colIndex == 0)
                    {
                        // 出力境界 外側
                        x_B = port2_X;
                        work_id_v_list_rod_B = id_v_list_rod_B3;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else if (colIndex == 1)
                    {
                        // 出力境界内側
                        x_B = port2_X - inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if ((rodCntHalf_port2 - 1 - i) % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = bRodCntHalf_port2 - 1 - (int)((rodCntHalf_port2 - 1 - i) / 2);
                            // 右のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                {
                                    id_v0 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = port2_Y - i * rodDistanceY - rodDistanceY * 0.5;
                                CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
                                uint lId = WgCadUtil.AddRightRod(
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
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
                            // 右のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = port2_Y - WaveguideWidth + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint lId = WgCadUtil.AddRightRod(
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
                            }
                        }
                    }
                }

                // 左のロッドを追加
                for (int colIndex = 0; colIndex < 1; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                {
                    // 左のロッド
                    IList<uint> work_id_v_list_rod_B = null;
                    double x_B = 0;
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    if (colIndex == 0)
                    {
                        // 出力境界 内側
                        x_B = port2_X - inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }

                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if ((rodCntHalf_port2 - 1 - i) % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = bRodCntHalf_port2 - 1 - (int)((rodCntHalf_port2 - 1 - i) / 2);
                            // 左のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                {
                                    id_v0 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = port2_Y - i * rodDistanceY - rodDistanceY * 0.5;
                                CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
                                uint lId = WgCadUtil.AddLeftRod(
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
                            }
                        }
                    }
                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if (i % 2 == (isShift180 ? 0 : 1))
                        {
                            int i2 = i / 2;
                            // 左のロッド
                            {
                                uint id_v0 = 0;
                                uint id_v1 = 0;
                                uint id_v2 = 0;
                                {
                                    id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                    id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                }
                                double x0 = x_B;
                                double y0 = port2_Y - WaveguideWidth + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
                                uint lId = WgCadUtil.AddLeftRod(
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
                            }
                        }
                    }
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド (出力導波路)
                int periodCntInputWg2 = 1;
                int periodCntX_port2 = periodCntInputWg2 + rodCntDiscon;

                // 中央のロッド(出力導波路)(左右の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1 = new List<uint>();
                IList<uint> id_v_list_F2 = new List<uint>();
                
                // 中央のロッド (出力導波路)
                for (int col = 1; col <= (periodCntX_port2 * 2 + 1); col++)
                {
                    if (col == (periodCntInputWg2 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                    }
                    else
                    {
                        if (col > (periodCntX_port2 * 2)) continue;
                    }

                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    if (col >= 0 && col < (periodCntInputWg2 * 2))
                    {
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else
                    {
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }

                    // 中央のロッド(出力導波路)
                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if ((col % 2 == 1 && ((rodCntHalf_port2 - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && ((rodCntHalf_port2 - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = port2_X - rodDistanceX * 0.5 * col;
                            double y0 = port2_Y - i * rodDistanceY - rodDistanceY * 0.5;
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
                    for (int i = 0; i < rodCntHalf_port2; i++)
                    {
                        if ((col % 2 == 1 && (i % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (i % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = port2_X - rodDistanceX * 0.5 * col;
                            double y0 = port2_Y - WaveguideWidth + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
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

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド(ベンド部分)(左の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1_Bend = new List<uint>();
                int rodCntY_F1_Bend = rodCntY_All_Bend - (rodCntHalf * 2 + defectRodCnt);
                for (int i = (rodCntY_F1_Bend - 1); i >= 0; i--)
                {
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                        isRodLattice = (i % 2 == 0);
                    }
                    else
                    {
                        isRodLattice = (i % 2 == 1);
                    }
                    if (isRodLattice)
                    {
                        uint id_e = 10;
                        double x0 = bendX2;
                        double y0 = port2_Y - rodDistanceY * i - rodDistanceY * 0.5;
                        double x_cross = bendX2;
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
                            id_v_list_F1_Bend.Add(id_v_add);
                            id_e_F1_Bend.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }
                // 中央のロッド (ベンド、左境界と接する半円)
                for (int i = 0; i < rodCntY_F1_Bend; i++)
                {
                    // 不連続領域
                    uint baseLoopId = 2;
                    // 左の強制境界と交差するロッド
                    bool isRodLattice = false;
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                        isRodLattice = (i % 2 == 0);
                    }
                    else
                    {
                        isRodLattice = (i % 2 == 1);
                    }
                    if (isRodLattice)
                    {
                        {
                            // 右の強制境界と交差するロッド
                            // 半円（右半分)を追加
                            double x0 = bendX2;
                            double y0 = port2_Y - rodDistanceY * i - rodDistanceY * 0.5;
                            int row2 = (rodCntY_F1_Bend - 1 - i) / 2;
                            System.Diagnostics.Debug.Assert(row2 >= 0);
                            uint id_v0 = id_v_list_F1_Bend[row2 * 3 + 0];
                            uint id_v1 = id_v_list_F1_Bend[row2 * 3 + 1];
                            uint id_v2 = id_v_list_F1_Bend[row2 * 3 + 2];
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
                IList<uint> id_v_list_F2_Bend = new List<uint>();
                int rodCntY_F2_Bend = rodCntY_All_Bend - (rodCntHalf * 2 + defectRodCnt);
                for (int i = 0; i < rodCntY_F2_Bend; i++)
                {
                    bool isRodLattice = false;
                    // 右の強制境界と交差するロッド
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                        isRodLattice = ((rodCntY_F2_Bend - 1 - i) % 2 == 0);
                    }
                    else
                    {
                        isRodLattice = ((rodCntY_F2_Bend - 1 - i) % 2 == 1);
                    }
                    if (isRodLattice)
                    {
                        uint id_e = 4;
                        double x0 = bendX1;
                        double y0 = port2_Y - WaveguideWidth - rodDistanceY * i - rodDistanceY * 0.5;
                        double x_cross = bendX1;
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
                            id_v_list_F2_Bend.Add(id_v_add);
                            id_e_F2_Bend.Add(id_e_add);
                            // DEBUG
                            //cad2d.SetColor_Edge(id_e_add, new double[] { 1.0, 0.0, 0.0 });
                        }
                    }
                }
                // 中央のロッド (ベンド、右境界と接する半円)
                for (int i = 0; i < rodCntY_F2_Bend; i++)
                {
                    // 不連続領域
                    uint baseLoopId = 2;
                    // 右の強制境界と交差するロッド
                    bool isRodLattice = false;
                    if (/*defectRodCnt_Bend == 1 &&*/ isShiftY)
                    {
                        isRodLattice = ((rodCntY_F2_Bend - 1 - i) % 2 == 0);
                    }
                    else
                    {
                        isRodLattice = ((rodCntY_F2_Bend - 1 - i) % 2 == 1);
                    }
                    if (isRodLattice)
                    {
                        {
                            // 右の強制境界と交差するロッド
                            // 半円（左半分)を追加
                            double x0 = bendX1;
                            double y0 = port2_Y - WaveguideWidth - rodDistanceY * i - rodDistanceY * 0.5;
                            int row2 = i / 2;
                            System.Diagnostics.Debug.Assert(row2 >= 0);
                            uint id_v0 = id_v_list_F2_Bend[row2 * 3 + 0];
                            uint id_v1 = id_v_list_F2_Bend[row2 * 3 + 1];
                            uint id_v2 = id_v_list_F2_Bend[row2 * 3 + 2];
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
                if (defectRodCnt_Bend == 5)
                {
                    // 広帯域化: 円弧形状エアホールを追加
                    double arcWidth = 0.6 * latticeA;
                    double arc_r1 = (defectRodCnt_Bend + 1) * rodDistanceX * 0.5 - rodDistanceX * 0.25 + arcWidth * 0.5;
                    double arc_r2 = arc_r1 - arcWidth;
                    // 上部ベンドの1/4リング
                    {
                        uint baseLoopId = 2;
                        int col = periodCntX_port1 * 2 + rodCntHalf_Bend + defectRodCnt_Bend;
                        double x0 = rodDistanceX * 0.5 * col + rodDistanceX * 0.25;
                        double y0 = port2_Y - (rodCntHalf - 1) * rodDistanceY - 0.5 * rodDistanceY + arcWidth * 0.5 - arc_r1 ;
                        //double endAngle = 180.0 - 8.0; // 左2列目ロッドに接する構造
                        //double endAngle = 180.0 + 7.0; // 左1列目ロッドに接する構造
                        double endAngle = 180.0 + 30.0; // トポロジー最適化構造に近い？構造
                        double startAngle = 90 + 3.0;
                        uint lId = addArcRod(cad2d, baseLoopId, x0, y0, arc_r1, arc_r2, startAngle, endAngle, false);
                        rodLoopIds.Add(lId);
                    }
                    // 下部ベンドの1/4リング
                    {
                        uint baseLoopId = 2;
                        int col = periodCntX_port1 * 2 + rodCntHalf_Bend + 1;
                        double x0 = rodDistanceX * 0.5 * col - rodDistanceX * 0.25;
                        double y0 = (rodCntHalf - 1) * rodDistanceY + rodDistanceY * 0.5 - arcWidth * 0.5 + arc_r1;
                        //double endAngle = 360.0 -8.0; // 右2列目ロッドに接する構造
                        //double endAngle = 360.0 + 7.0; // 右1列目ロッドに接する構造
                        double endAngle = 360.0 + 30.0; // トポロジー最適化構造に近い？構造
                        double startAngle = 270 + 3.0;
                        uint lId = addArcRod(cad2d, baseLoopId, x0, y0, arc_r1, arc_r2, startAngle, endAngle, false);
                        rodLoopIds.Add(lId);
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
                    //check
                    //for (int i = 0; i < ((ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2); i++)
                    //{
                    //    uint eId = (uint)(14 + i + 1);
                    //    cad2d.SetColor_Edge(eId, new double[] { 1.0, 1.0, 1.0 });
                    //}
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
            //const uint portCnt = 2;
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgPortInfoList.Add(new WgUtilForPeriodicEigenExt.WgPortInfo());
                System.Diagnostics.Debug.Assert(WgPortInfoList.Count == (portIndex + 1));
                WgPortInfoList[portIndex].LatticeA = latticeA;
                WgPortInfoList[portIndex].PeriodicDistance = periodicDistance;
                WgPortInfoList[portIndex].MinEffN = minEffN;
                WgPortInfoList[portIndex].MaxEffN = maxEffN;
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
                uint[] eId_cad_list = new uint[10 + id_e_F1.Count + id_e_F2.Count + id_e_F1_Bend.Count + id_e_F2_Bend.Count];
                eId_cad_list[0] = 2;
                eId_cad_list[1] = 3;
                eId_cad_list[2] = 4;
                eId_cad_list[3] = 5;
                eId_cad_list[4] = 6;
                eId_cad_list[5] = 8;
                eId_cad_list[6] = 9;
                eId_cad_list[7] = 10;
                eId_cad_list[8] = 11;
                eId_cad_list[9] = 12;
                for (int i = 0; i < id_e_F1.Count; i++)
                {
                    eId_cad_list[10 + i] = id_e_F1[i];
                }
                for (int i = 0; i < id_e_F2.Count; i++)
                {
                    eId_cad_list[10 + id_e_F1.Count + i] = id_e_F2[i];
                }
                for (int i = 0; i < id_e_F1_Bend.Count; i++)
                {
                    eId_cad_list[10 + id_e_F1.Count + id_e_F2.Count + i] = id_e_F1_Bend[i];
                }
                for (int i = 0; i < id_e_F2_Bend.Count; i++)
                {
                    eId_cad_list[10 + id_e_F1.Count + id_e_F2.Count + id_e_F1_Bend.Count + i] = id_e_F2_Bend[i];
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

                int ndivPlus = 0;
                if (portIndex == 0)
                {
                    ndivPlus = ndivPlus_port1;
                }
                else if (portIndex == 1)
                {
                    ndivPlus = ndivPlus_port2;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
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
                    eId_cad_list[0] = 7;
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
                        eId_cad_list[i] = (uint)(14 + (ndivPlus_port1 - 1) - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(14 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) - (i - 1));
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

                int ndivPlus = 0;
                if (portIndex == 0)
                {
                    ndivPlus = ndivPlus_port1;
                }
                else if (portIndex == 1)
                {
                    ndivPlus = ndivPlus_port2;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                uint[] eId_cad_list = new uint[ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                IList<uint> work_id_e_rod_B = null;

                if (portIndex == 0)
                {
                    eId_cad_list[0] = 13;
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
                    eId_cad_list[0] = 14;
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
                        eId_cad_list[i] = (uint)(14 + (ndivPlus_port1 - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(14 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 - (i - 1));
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
                if (portIndex == 0 || portIndex == 1)
                {
                    WgUtil.GetLoopCoordList(World, wgPortInfo1.FieldInputWgLoopId, out no_c_all, out to_no_loop, out coord_c_all);
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
                        //if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt)) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf))
                        //if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - (0.5 * rodDistanceY - rodRadius)) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + (0.5 * rodDistanceY - rodRadius))) // dielectric rod
                        //if (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY) && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)) // air hole
                        if ((portIndex == 0 &&
                                (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY)
                                  && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)))
                            || (portIndex == 1 &&
                                (coord[1] >= (port2_Y - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY)
                                  && coord[1] <= (port2_Y - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)))
                            )
                        {
                            portNodes.Add(no_c_all[i]);
                        }
                    }
                    wgPortInfo1.PCWaveguidePorts.Add(portNodes);
                }
            }
            return true;
        }

        /// <summary>
        /// 弧ロッドを追加する
        /// </summary>
        /// <param name="cad2d"></param>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="arc_r1"></param>
        /// <param name="arc_r2"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        /// <param name="isReverseArc"></param>
        /// <returns></returns>
        private static uint addArcRod(
            CCadObj2D cad2d,
            uint baseLoopId,
            double x0,
            double y0,
            double arc_r1,
            double arc_r2,
            double startAngle,
            double endAngle,
            bool isReverseArc)
        {
            System.Diagnostics.Debug.Assert(isReverseArc == false); // 現状順方向のみ
            uint retLoopId = 0;

            const int ndivCircle = 24;
            IList<CVector2D> pts = new List<CVector2D>();
            double angle = endAngle - startAngle;
            int ndivArc = (int)Math.Ceiling(ndivCircle * Math.Abs(angle) / 360.0);
            for (int i = 0; i <= ndivArc; i++)
            {
                double theta = startAngle + i * angle / ndivArc;
                double x = x0 + arc_r1 * Math.Cos(pi * theta / 180.0);
                double y = y0 + arc_r1 * Math.Sin(pi * theta / 180.0);
                pts.Add(new CVector2D(x, y));
            }
            for (int i = 0; i <= ndivArc; i++)
            {
                double theta = endAngle - i * angle / ndivArc;
                double x = x0 + arc_r2 * Math.Cos(pi * theta / 180.0);
                double y = y0 + arc_r2 * Math.Sin(pi * theta / 180.0);
                pts.Add(new CVector2D(x, y));
            }
            /* DEBUG
            foreach (CVector2D pp in pts)
            {
                cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, baseLoopId, pp);
            }
             */
            retLoopId = cad2d.AddPolygon(pts, baseLoopId).id_l_add;
            System.Diagnostics.Debug.Assert(retLoopId != 0);

            return retLoopId;
        }

    }
}
