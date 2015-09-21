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
    class Problem13_5_2
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC導波路 60°三角形格子 方向性結合器 (60°ベンド) ドロップチャンネルを曲げて終端
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
            //int propModeCntToSolve = 3;
            int propModeCntToSolve = 2;
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
            // ロッドの数(半分)
            //const int rodCntHalf = 3; // for latticeTheta = 60 r = 0.30a air hole
            const int rodCntHalf = 3;
            System.Diagnostics.Debug.Assert(rodCntHalf % 2 == 1); // 奇数を指定（図面の都合上)
            // 欠陥ロッド数
            // for latticeTheta = 60 r = 0.30a dielectric rod
            const int defectRodCnt = 1;
            // 三角形格子の内角
            double latticeTheta = 60.0;
            // ロッドの半径
            //double rodRadiusRatio = 0.30; // for latticeTheta = 60 r = 0.30a air hole n = 3.40
            double rodRadiusRatio = 0.30; // for latticeTheta = 60 r = 0.30a air hole n = 2.76
            // ロッドの比誘電率
            //double rodEps = 3.40 * 3.40; // for latticeTheta = 60 r = 0.30a air hole n = 3.40
            double rodEps = 2.76 * 2.76; // for latticeTheta = 60 r = 0.30a air hole n = 2.76
            // 1格子当たりの分割点の数
            //const int ndivForOneLattice = 9; // for latticeTheta = 60 r = 0.30a air hole
            const int ndivForOneLattice = 8;// 9;
            // ロッド円周の分割数
            //const int rodCircleDiv = 12; // for latticeTheta = 60 r = 0.30a air hole
            const int rodCircleDiv = 12;
            // ロッドの半径の分割数
            //const int rodRadiusDiv = 4; // for latticeTheta = 60 r = 0.30a air hole
            const int rodRadiusDiv = 4;
            // 導波路不連続領域の長さ
            //const int rodCntDiscon = 4;
            const int rodCntDiscon = 4;
            // 結合部ロッド数
            const int rodCntMiddle = 2;
            // 最適形状？
            bool isOptBend = false;
            //bool isOptBend = true;
            // 結合長
            //const int rodCntCoupling = 4; // 7a for latticeTheta = 60 r = 0.30a air hole n = 3.40 (rodCntHalf == 3とき、4a + 2a<ベンド部> + 0.5a * 2<左右両端> = 7a)
            //const int rodCntCoupling = 2; // 5a for latticeTheta = 60 r = 0.30a air hole n = 3.40 (完全結合長に合わせた場合)
            //const int rodCntCoupling = 3;// 6a for latticeTheta = 60 r = 0.30a air hole n = 2.76
            const int rodCntCoupling = 3;
            // 入出力不連続部の距離
            const int rodCntDiscon_port1 = (rodCntDiscon + rodCntCoupling);  // 入力部は結合領域を含む
            //const int rodCntDiscon_port3 = 3;
            const int rodCntDiscon_port3 = 3;// 2;

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
            // メッシュのサイズ
            double meshL = 1.05 * WaveguideWidth / (latticeCnt * ndivForOneLattice);

            // for latticeTheta = 60 r = 0.30a air hole n = 3.40
            //NormalizedFreq1 = 0.215;//0.210;
            //NormalizedFreq2 = 0.2401;// 0.2561;
            //FreqDelta = 0.00025; //0.0005;
            //GraphFreqInterval = 0.005;
            // for latticeTheta = 60 r = 0.30a air hole n = 2.76
            NormalizedFreq1 = 0.267;
            NormalizedFreq2 = 0.2831;//0.2871;
            FreqDelta = 0.00025;// 0.0005;
            GraphFreqInterval = 0.004;

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
            const uint portCnt = 4; // 構造上のポート数
            const uint portCnt_real = 3; // 実際のポート数(ポート4は終端する)
            System.Diagnostics.Debug.Assert(Math.Abs(WaveguideWidth - WaveguideWidth) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit);
            double disconLength1 = rodDistanceX * rodCntDiscon_port1;
            double disconLength3 = rodDistanceX * rodCntDiscon_port3;

            // Cad
            IList<uint> rodLoopIds = new List<uint>();
            IList<uint> rodLoopIds_InputWg1 = new List<uint>();
            IList<uint> rodLoopIds_InputWg2 = new List<uint>();
            IList<uint> rodLoopIds_InputWg3 = new List<uint>();
            IList<uint> rodLoopIds_InputWg4 = new List<uint>();
            int ndivPlus_port1 = 0;
            int ndivPlus_port2 = 0;
            int ndivPlus_port3 = 0;
            int ndivPlus_port4 = 0;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_rod_B3 = new List<uint>();
            IList<uint> id_e_rod_B4 = new List<uint>();
            IList<uint> id_e_rod_B5 = new List<uint>();
            IList<uint> id_e_rod_B6 = new List<uint>();
            IList<uint> id_e_rod_B7 = new List<uint>();
            IList<uint> id_e_rod_B8 = new List<uint>();
            IList<uint> id_e_F1 = new List<uint>();
            IList<uint> id_e_F2 = new List<uint>();

            // ベンド下側角
            double bendX1_0 = inputWgLength + disconLength1 + WaveguideWidth / Math.Sqrt(3.0);
            double bendY1_0 = 0.0;
            bendX1_0 -= latticeA * Math.Sqrt(3.0) / 4.0;
            bendY1_0 -= rodDistanceY / 4.0;
            if (rodCntMiddle % 2 == 0)
            {
                bendX1_0 -= rodDistanceX / 4.0;
                bendY1_0 -= rodDistanceY / 2.0;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            double bendX1 = bendX1_0 + (0.0 - bendY1_0) / Math.Sqrt(3.0);
            double bendY1 = 0.0;
            // ベンド部と出力部の境界
            double bendX2 = bendX1_0 + WaveguideWidth / (2.0 * Math.Sqrt(3.0));
            double bendY2 = bendY1_0 + WaveguideWidth / 2.0;
            // 出力部内部境界の終点
            double port3_X2_B6 = bendX2 + disconLength3 / 2.0;
            double port3_Y2_B6 = bendY2 + disconLength3 * Math.Sqrt(3.0) / 2.0;
            // 出力部境界の終点
            double port3_X2_B5 = port3_X2_B6 + inputWgLength / 2.0;
            double port3_Y2_B5 = port3_Y2_B6 + inputWgLength * Math.Sqrt(3.0) / 2.0;
            // ベンド部上側角
            double bendX3_0 = inputWgLength + disconLength1;
            double bendY3_0 = WaveguideWidth;
            bendX3_0 -= latticeA * Math.Sqrt(3.0) / 4.0;
            bendY3_0 -= rodDistanceY / 4.0;
            if (rodCntMiddle % 2 == 0)
            {
                bendX3_0 -= rodDistanceX / 4.0;
                bendY3_0 -= rodDistanceY / 2.0;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            double bendX3 = bendX3_0 + (WaveguideWidth - bendY3_0) / Math.Sqrt(3.0);
            double bendY3 = WaveguideWidth;
            // 出力部内部境界の始点
            double port3_X1_B6 = bendX3_0 + disconLength3 / 2.0;
            double port3_Y1_B6 = bendY3_0 + disconLength3 * Math.Sqrt(3.0) / 2.0;
            // 出力部境界の始点
            double port3_X1_B5 = port3_X1_B6 + inputWgLength / 2.0;
            double port3_Y1_B5 = port3_Y1_B6 + inputWgLength * Math.Sqrt(3.0) / 2.0;

            // 導波路2
            double wg2_Y2 = WaveguideWidth - (rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle) * rodDistanceY;
            double wg2_Y1 = wg2_Y2 + WaveguideWidth;
            double bendX1_wg2 = bendX1 + (wg2_Y1 - bendY1) / Math.Sqrt(3.0);
            // ポート2
            double port2_X = bendX1_wg2 + rodDistanceX * 0.25 + rodDistanceX * 0.5 + inputWgLength;
            double bendX4 = port2_X - inputWgLength - rodDistanceX * 0.5 - rodDistanceX * 0.25;

            System.Diagnostics.Debug.Assert(port3_Y2_B6 > wg2_Y1);
            // check
            {
                double[] pt1_port3_B5 = new double[] {port3_X1_B5, port3_Y1_B5};
                double[] pt2_port3_B5 = new double[] {port3_X2_B5, port3_Y2_B5};
                double distance_B5 = CVector2D.Distance2D(pt1_port3_B5, pt2_port3_B5);
                System.Diagnostics.Debug.Assert(Math.Abs(distance_B5 - WaveguideWidth) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit);
                double[] pt1_port3_B6 = new double[] { port3_X1_B6, port3_Y1_B6 };
                double[] pt2_port3_B6 = new double[] { port3_X2_B6, port3_Y2_B6 };
                double distance_B6 = CVector2D.Distance2D(pt1_port3_B6, pt2_port3_B6);
                System.Diagnostics.Debug.Assert(Math.Abs(distance_B6 - WaveguideWidth) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit);
            }
            
            // 右側ベンド
            double bendX5_0 = inputWgLength + rodCntDiscon * rodDistanceX + 2.0 * rodDistanceX;
            double bendY5_0 = WaveguideWidth;
            bendX5_0 += latticeA * Math.Sqrt(3.0) / 4.0;
            bendY5_0 -= rodDistanceY / 4.0;
            if (rodCntMiddle % 2 == 0)
            {
                bendX5_0 += rodDistanceX / 4.0;
                bendY5_0 -= rodDistanceY / 2.0;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            double bendX5 = bendX5_0 + (bendY5_0 - WaveguideWidth) / Math.Sqrt(3.0);
            double bendY5 = WaveguideWidth;

            // 左出力部内部境界の終点
            double port4_X2_B8 = bendX5_0 - disconLength3 / 2.0;
            double port4_Y2_B8 = bendY5_0 + disconLength3 * Math.Sqrt(3.0) / 2.0;
            // 左出力部境界の終点
            double port4_X2_B7 = port4_X2_B8 - inputWgLength / 2.0;
            double port4_Y2_B7 = port4_Y2_B8 + inputWgLength * Math.Sqrt(3.0) / 2.0;
            // 左出力部境界の始点
            double port4_X1_B7 = port4_X2_B7 - WaveguideWidth * Math.Sqrt(3.0) / 2.0;
            double port4_Y1_B7 = port4_Y2_B7 - WaveguideWidth / 2.0;
            // 左出力部内部境界の始点
            double port4_X1_B8 = port4_X2_B8 - WaveguideWidth * Math.Sqrt(3.0) / 2.0;
            double port4_Y1_B8 = port4_Y2_B8 - WaveguideWidth / 2.0;

            // 導波路２の入力側
            double bendX6 = inputWgLength + rodDistanceX * 0.5 + rodDistanceX * 0.25;

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
                    pts.Add(new CVector2D(0.0, wg2_Y1)); // 頂点1
                    pts.Add(new CVector2D(0.0, wg2_Y2)); // 頂点2
                    pts.Add(new CVector2D(inputWgLength, wg2_Y2)); // 頂点3
                    pts.Add(new CVector2D(port2_X - inputWgLength, wg2_Y2)); // 頂点4
                    pts.Add(new CVector2D(port2_X, wg2_Y2)); // 頂点5
                    pts.Add(new CVector2D(port2_X, wg2_Y1)); // 頂点6
                    pts.Add(new CVector2D(port2_X - inputWgLength, wg2_Y1)); // 頂点7
                    pts.Add(new CVector2D(bendX4, wg2_Y1)); // 頂点8
                    pts.Add(new CVector2D(port3_X2_B6, port3_Y2_B6)); // 頂点9
                    pts.Add(new CVector2D(port3_X2_B5, port3_Y2_B5)); // 頂点10
                    pts.Add(new CVector2D(port3_X1_B5, port3_Y1_B5)); // 頂点11
                    pts.Add(new CVector2D(port3_X1_B6, port3_Y1_B6)); // 頂点12
                    pts.Add(new CVector2D(bendX3, bendY3)); // 頂点13
                    pts.Add(new CVector2D(bendX5, bendY5)); // 頂点14
                    pts.Add(new CVector2D(port4_X2_B8, port4_Y2_B8)); // 頂点15
                    pts.Add(new CVector2D(port4_X2_B7, port4_Y2_B7)); // 頂点16
                    pts.Add(new CVector2D(port4_X1_B7, port4_Y1_B7)); // 頂点17
                    pts.Add(new CVector2D(port4_X1_B8, port4_Y1_B8)); // 頂点18
                    pts.Add(new CVector2D(bendX6, wg2_Y1)); // 頂点19
                    pts.Add(new CVector2D(inputWgLength, wg2_Y1)); // 頂点20
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                }
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 20).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(4, 7).id_e_add;
                uint eIdAdd3 = cad2d.ConnectVertex_Line(9, 12).id_e_add;
                uint eIdAdd4 = cad2d.ConnectVertex_Line(15, 18).id_e_add;
                // 入出力導波路の周期構造境界上の頂点を追加
                IList<double> ys_port1 = new List<double>();
                IList<double> ys_rod_port1 = new List<double>();
                IList<double> ys_port2 = new List<double>();
                IList<double> ys_rod_port2 = new List<double>();
                IList<double> xs_port3 = new List<double>();
                IList<double> xs_rod_port3 = new List<double>();
                IList<double> xs_port4 = new List<double>();
                IList<double> xs_rod_port4 = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                IList<uint> id_v_list_rod_B3 = new List<uint>();
                IList<uint> id_v_list_rod_B4 = new List<uint>();
                IList<uint> id_v_list_rod_B5 = new List<uint>();
                IList<uint> id_v_list_rod_B6 = new List<uint>();
                IList<uint> id_v_list_rod_B7 = new List<uint>();
                IList<uint> id_v_list_rod_B8 = new List<uint>();

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
                    else if (portIndex == 2)
                    {
                        cur_rodCntHalf = rodCntHalf;
                        cur_defectRodCnt = defectRodCnt;
                        cur_ndivForOneLattice = ndivForOneLattice;
                        cur_WaveguideWidth = WaveguideWidth;
                        cur_rodDistanceY = rodDistanceY;
                        ys = xs_port3;
                        ys_rod = xs_rod_port3;
                        System.Diagnostics.Debug.Assert(ys.Count == 0);
                        System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                    }
                    else if (portIndex == 3)
                    {
                        cur_rodCntHalf = rodCntHalf;
                        cur_defectRodCnt = defectRodCnt;
                        cur_ndivForOneLattice = ndivForOneLattice;
                        cur_WaveguideWidth = WaveguideWidth;
                        cur_rodDistanceY = rodDistanceY;
                        ys = xs_port4;
                        ys_rod = xs_rod_port4;
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
                    if (portIndex == 3)
                    {
                        int i = cur_rodCntHalf;
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
                        if (portIndex == 3) continue; // ポート4は除外
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
                        if (portIndex == 3) continue; // ポート4は除外
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
                        if (portIndex == 3) continue; // ポート4は除外
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
                        if (portIndex == 3) continue; // ポート4は除外
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
                        if (portIndex == 3) continue; // ポート4は除外
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
                    else if (portIndex == 2)
                    {
                        ndivPlus_port3 = cur_ndivPlus;
                    }
                    else if (portIndex == 3)
                    {
                        ndivPlus_port4 = cur_ndivPlus;
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
                    for (int boundaryIndex = 0; boundaryIndex < 2; boundaryIndex++)
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
                                y_pt = yAry[i] - (WaveguideWidth - wg2_Y1);
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B1;
                                work_id_v_list_rod_B = id_v_list_rod_B1;
                            }
                            else if (portIndex == 0 && boundaryIndex == 1)
                            {
                                // 入力導波路 内側境界
                                id_e = 21;
                                x_pt = inputWgLength;
                                y_pt = yAry[yAry.Length - 1 - i] - (WaveguideWidth - wg2_Y1);
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B2;
                                work_id_v_list_rod_B = id_v_list_rod_B2;
                            }
                            else if (portIndex == 1 && boundaryIndex == 0)
                            {
                                // 入力導波路 外側境界
                                id_e = 5;
                                x_pt = port2_X;
                                y_pt = yAry[yAry.Length - 1 - i] - (WaveguideWidth - wg2_Y1);
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B3;
                                work_id_v_list_rod_B = id_v_list_rod_B3;
                            }
                            else if (portIndex == 1 && boundaryIndex == 1)
                            {
                                // 入力導波路 内側境界
                                id_e = 22;
                                x_pt = port2_X - inputWgLength;
                                y_pt = yAry[yAry.Length - 1 - i] - (WaveguideWidth - wg2_Y1);
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B4;
                                work_id_v_list_rod_B = id_v_list_rod_B4;
                            }
                            else if (portIndex == 2 && boundaryIndex == 0)
                            {
                                // 出力導波路 外側境界
                                id_e = 10;
                                x_pt = port3_X1_B5 + yAry[i] * Math.Sqrt(3.0) / 2.0;
                                y_pt = port3_Y1_B5 - yAry[i] / 2.0;
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B5;
                                work_id_v_list_rod_B = id_v_list_rod_B5;
                            }
                            else if (portIndex == 2 && boundaryIndex == 1)
                            {
                                // 出力導波路 内側境界
                                id_e = 23;
                                x_pt = port3_X1_B6 + yAry[i] * Math.Sqrt(3.0) / 2.0;
                                y_pt = port3_Y1_B6 - yAry[i] / 2.0;
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B6;
                                work_id_v_list_rod_B = id_v_list_rod_B6;
                            }
                            else if (portIndex == 3 && boundaryIndex == 0)
                            {
                                // 出力導波路 外側境界
                                id_e = 16;
                                x_pt = port4_X1_B7 + yAry[i] * Math.Sqrt(3.0) / 2.0;
                                y_pt = port4_Y1_B7 + yAry[i] / 2.0;
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B7;
                                work_id_v_list_rod_B = id_v_list_rod_B7;
                            }
                            else if (portIndex == 3 && boundaryIndex == 1)
                            {
                                // 出力導波路 内側境界
                                id_e = 24;
                                x_pt = port4_X1_B8 + yAry[i] * Math.Sqrt(3.0) / 2.0;
                                y_pt = port4_Y1_B8 + yAry[i] / 2.0;
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B8;
                                work_id_v_list_rod_B = id_v_list_rod_B8;
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
                System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B5.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B6.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                //System.Diagnostics.Debug.Assert(id_v_list_rod_B7.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));
                //System.Diagnostics.Debug.Assert(id_v_list_rod_B8.Count == bRodCntHalf_port1 * 2 * (rodRadiusDiv * 2 + 1));

                // ロッドを追加
                /////////////////////////////////////////////////////////////
                // 入力導波路側ロッド
                // 左のロッドを追加
                for (int colIndex = 0; colIndex < 3; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
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
                    else if (colIndex == 2)
                    {
                        // ポート2内側
                        x_B = port2_X - inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
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
                                double y0 = wg2_Y1 - i * rodDistanceY - rodDistanceY * 0.5;
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
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                                else if (inputWgNo == 4)
                                {
                                    rodLoopIds_InputWg4.Add(lId);
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
                                double y0 = wg2_Y2 + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
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
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                                else if (inputWgNo == 4)
                                {
                                    rodLoopIds_InputWg4.Add(lId);
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

                    if (colIndex == 0)
                    {
                        // 入力境界 内側
                        x_B = inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B2;
                        // 入力導波路領域
                        baseLoopId = 1;
                        inputWgNo = 1;
                    }
                    else if (colIndex == 1)
                    {
                        // ポート2内側
                        x_B = port2_X - inputWgLength;
                        work_id_v_list_rod_B = id_v_list_rod_B4;
                        // 不連続領域
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }
                    else if (colIndex == 2)
                    {
                        // ポート2外側
                        x_B = port2_X;
                        work_id_v_list_rod_B = id_v_list_rod_B3;
                        // 出力導波路領域
                        baseLoopId = 3;
                        inputWgNo = 2;
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
                                double y0 = wg2_Y1 - i * rodDistanceY - rodDistanceY * 0.5;
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
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                                else if (inputWgNo == 4)
                                {
                                    rodLoopIds_InputWg4.Add(lId);
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
                                double y0 = wg2_Y2 + rodDistanceY * rodCntHalf - i * rodDistanceY - rodDistanceY * 0.5;
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
                                else if (inputWgNo == 3)
                                {
                                    rodLoopIds_InputWg3.Add(lId);
                                }
                                else if (inputWgNo == 4)
                                {
                                    rodLoopIds_InputWg4.Add(lId);
                                }
                            }
                        }
                    }
                }

                // 中央のロッド (入力導波路 + 不連続部)
                int periodCntInputWg1 = 1;
                int periodCntBendX = (rodCntHalf * 2 + defectRodCnt) / 2;
                int rodCnt_BendY = rodCntHalf * 2 + defectRodCnt * 2 + rodCntMiddle;
                int rodCnt_Wg2 = rodCntHalf * 2 + defectRodCnt;
                int periodCntX_BendX6 = (int)Math.Round(bendX6 / rodDistanceX);
                int periodCntX_BendX5 = (int)Math.Round(bendX5 / rodDistanceX);
                int periodCntX_port3 = (int)Math.Round(port3_X2_B6 / rodDistanceX);
                int periodCntX_BendX4 = (int)Math.Round(bendX4 / rodDistanceX);
                int periodCntX_port2 = (int)Math.Round(port2_X / rodDistanceX);
                int row_wg2_Y1 = (int)Math.Round((WaveguideWidth - wg2_Y1) / rodDistanceY);

                // ベンド最適化構造：ジグザグ
                //   r = 0.30a のとき
                // 小さいロッドの半径
                double rodRadius_Small_Zigzag = 0.0; // 小さいロッド
                //double rodRadius_Small_Zigzag = 0.14 * latticeA; // 小さいロッド
                //double rodRadius_Small_Zigzag = 0.12 * latticeA; // 小さいロッド
                double rodRadius_Big_Zigzag = rodRadius;//0.30 * latticeA; // 大きいロッド
                //double rodRadius_Big_Zigzag = 0.32 * latticeA; // 大きいロッド
                // 小さいロッドの周方向分割数
                int rodCircleDiv_Small_Zigzag = 12;// 12;
                int rodRadiusDiv_Small_Zigzag = 2; // 4;

                for (int col = 1; col <= (periodCntX_port3 * 2); col++)
                {
                    if (col == (periodCntInputWg1 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    uint baseLoopId = 0;
                    int inputWgNo = 0;

                    // 中央のロッド
                    for (int i = 0; i < rodCnt_BendY; i++)
                    {
                        if (col >= 0 && col <= (periodCntX_BendX6 * 2)
                            && (i >= 0 && i <= (rodCnt_BendY - rodCntHalf * 2 - defectRodCnt - 1))
                            )
                        {
                            continue;
                        }
                        else if (col >= (periodCntX_BendX6 * 2 + 1) && col <= (periodCntX_BendX5 * 2 - 1)
                           && (i >= 0 && i <= (rodCnt_BendY - rodCntHalf * 2 - defectRodCnt - 1)))
                        {
                            int workCol = col - ((periodCntX_BendX6) * 2);
                            if (workCol >= 0
                                && (i >= (rodCnt_BendY - rodCntHalf * 2 - defectRodCnt - (workCol + 1) / 4))
                                )
                            {
                                // 斜め境界
                            }
                            else
                            {
                                continue;
                            }
                        }

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

                        // 出力ポートとベンド部の境界判定
                        if (col >= ((periodCntInputWg1 + rodCntDiscon_port1) * 2 + 1))
                        {
                            int rowMin = (col - (periodCntInputWg1 + rodCntDiscon_port1 - 1) * 2) / 3;
                            if (i < rowMin)
                            {
                                continue;
                            }
                        }
                        // 下部の境界判定
                        if (col >= (periodCntX_BendX4 * 2))
                        {
                            int rowMax = row_wg2_Y1 - (col - (periodCntX_BendX4 * 2));
                            if (i > rowMax)
                            {
                                continue;
                            }
                        }
                        // ロッドの半径
                        double rr = rodRadius;
                        // ロッドの半径方向分割数
                        int nr = rodRadiusDiv;
                        // ロッドの周方向分割数
                        int nc = rodCircleDiv;
                        // ずらす距離
                        double ofs_x_rod = 0.0;
                        double ofs_y_rod = 0.0;

                        if (inputWgNo == 0)
                        {
                            // 分割数を調整
                            nr = 2;
                        }

                        // 欠陥(導波路2)
                        if (i >= (rodCnt_BendY - 1 - rodCntHalf - defectRodCnt + 1) && i <= (rodCnt_BendY - 1 - rodCntHalf))
                        {
                            continue;
                        }

                        // ドロップチャンネルの終端を曲げて延長する
                        if (col == ((periodCntInputWg1 + rodCntDiscon) * 2) && i == (rodCntHalf - 1))
                        {
                            //continue;
                            if (!isOptBend)
                            {
                                continue;
                            }
                        }

                        if (!isOptBend)
                        {
                            // ベンド初期構造
                            // 欠陥（導波路1入力部）
                            if ((col >= (periodCntInputWg1 + rodCntDiscon) * 2 && col <= ((periodCntInputWg1 + rodCntDiscon_port1) * 2 - 1))
                                && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                            {
                                continue;
                            }
                            // 欠陥（ベンド部入力側）
                            int colBend_port1 = (col - (periodCntInputWg1 + rodCntDiscon_port1) * 2);
                            int colBend_centerLine = (periodCntInputWg1 + rodCntDiscon_port1) * 2 + periodCntBendX;
                            if ((col >= ((periodCntInputWg1 + rodCntDiscon_port1) * 2))
                                && col <= (colBend_centerLine - 1)
                                && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                            {
                                continue;
                            }
                            // 欠陥（ベンド部出力側）
                            if ((col >= (colBend_centerLine))
                                )
                            {
                                int colBend_port2 = col - colBend_centerLine;
                                if (i >= (rodCntHalf - colBend_port2) && i <= (rodCntHalf + defectRodCnt - 1 - colBend_port2))
                                {
                                    continue;
                                }
                            }
                        }

                        /*
                        // ベンド最適化構造：ベンド部に小さいロッドを3つ挿入
                        //   r = 0.30a, r = 0.29 のとき
                        // 小さいロッドの半径
                        //double rodRadius_Small = 0.14 * latticeA; // 小さいロッド３つ
                        //double rodRadius_Small = 0.1627 * latticeA; // ロッド２つ(中央のロッドがない)
                        //double rodRadius_Small = 0.13 * latticeA; // ベンド中央にロッド１つ
                        double rodRadius_Small = 0.16 * latticeA; // ロッド２つ(中央のロッドがない)
                        // 小さいロッドの周方向分割数
                        int rodCircleDiv_Small = 12;// 12;
                        int rodRadiusDiv_Small = 2; // 4;

                        // 左ベンド出力
                        if (col == ((periodCntInputWg1 + rodCntDiscon) * 2) && i == (rodCntHalf - 1))
                        {
                            rr = rodRadius_Small;
                            //nr = (int)Math.Ceiling(rodRadiusDiv * rr / rodRadius);
                            nr = rodRadiusDiv_Small;
                            nc = rodCircleDiv_Small;
                        }

                        // 欠陥（入力部）
                        if ((col >= (periodCntInputWg1 + rodCntDiscon) * 2 && col <= ((periodCntInputWg1 + rodCntDiscon_port1) * 2 - 1))
                            && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                        {
                            //continue;
                            if (col == ((periodCntInputWg1 + rodCntDiscon) * 2 + 3) && i == (rodCntHalf))
                            {
                                // 左ベンド入力部
                                rr = rodRadius_Small;
                                //nr = (int)Math.Ceiling(rodRadiusDiv * rr / rodRadius);
                                nr = rodRadiusDiv_Small;
                                nc = rodCircleDiv_Small;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        // 欠陥（ベンド部入力側）
                        int colBend_port1 = (col - (periodCntInputWg1 + rodCntDiscon_port1) * 2);
                        int colBend_centerLine = (periodCntInputWg1 + rodCntDiscon_port1) * 2 + periodCntBendX;
                        if ((col >= ((periodCntInputWg1 + rodCntDiscon_port1) * 2))
                            && col <= (colBend_centerLine - 1)
                            && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                        {
                            //continue; // ベンド中央にロッド１つ
                            if (col == (colBend_centerLine - 2))
                            {
                                rr = rodRadius_Small;
                                //nr = (int)Math.Ceiling(rodRadiusDiv * rr / rodRadius);
                                nr = rodRadiusDiv_Small;
                                nc = rodCircleDiv_Small;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        // 欠陥（ベンド部出力側）
                        if ((col >= (colBend_centerLine))
                            )
                        {
                            int colBend_port2 = col - colBend_centerLine;
                            if (i >= (rodCntHalf - colBend_port2) && i <= (rodCntHalf + defectRodCnt - 1 - colBend_port2))
                            {
                                //if (col == (colBend_centerLine)) // ベンド中央にロッド１つ
                                //if (col == (colBend_centerLine) || col == (colBend_centerLine + 1))  // rodRadius_Small = 0.14 * latticeA  小さいロッド３つ
                                if (col == (colBend_centerLine + 1)) // ロッド２つ(中央のロッドがない)
                                {
                                    rr = rodRadius_Small;
                                    //nr = (int)Math.Ceiling(rodRadiusDiv * rr / rodRadius);
                                    nr = rodRadiusDiv_Small;
                                    nc = rodCircleDiv_Small;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                         */


                        if (isOptBend)
                        {
                            // ベンド最適形状：ジグザグ
                            // 欠陥（入力部）
                            if ((col >= (periodCntInputWg1 + rodCntDiscon) * 2 && col <= ((periodCntInputWg1 + rodCntDiscon_port1) * 2 - 1))
                                && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                            {
                                if (col == ((periodCntInputWg1 + rodCntDiscon_port1) * 2 - 1)) // ベンド入力部
                                {
                                    if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                    {
                                        continue;
                                    }
                                    rr = rodRadius_Small_Zigzag;
                                    nr = rodRadiusDiv_Small_Zigzag;
                                    nc = rodCircleDiv_Small_Zigzag;
                                }
                                else if (col == ((periodCntInputWg1 + rodCntDiscon) * 2 + 3)) // 左ベンド入力部
                                {
                                    if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                    {
                                        continue;
                                    }
                                    rr = rodRadius_Small_Zigzag;
                                    nr = rodRadiusDiv_Small_Zigzag;
                                    nc = rodCircleDiv_Small_Zigzag;
                                }
                                else if (col == ((periodCntInputWg1 + rodCntDiscon) * 2 + 1)) // 左ベンド入力部角
                                {
                                    rr = rodRadius_Big_Zigzag;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            // 欠陥（ベンド部入力側）
                            int colBend_port1 = (col - (periodCntInputWg1 + rodCntDiscon_port1) * 2);
                            int colBend_centerLine = (periodCntInputWg1 + rodCntDiscon_port1) * 2 + periodCntBendX;
                            if ((col >= ((periodCntInputWg1 + rodCntDiscon_port1) * 2))
                                && col <= (colBend_centerLine - 1)
                                && (i >= rodCntHalf && i <= (rodCntHalf + defectRodCnt - 1)))
                            {
                                if (col == (colBend_centerLine - 2))
                                {
                                    if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                    {
                                        continue;
                                    }
                                    rr = rodRadius_Small_Zigzag;
                                    nr = rodRadiusDiv_Small_Zigzag;
                                    nc = rodCircleDiv_Small_Zigzag;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            // 欠陥（ベンド部出力側）
                            if ((col >= (colBend_centerLine))
                                )
                            {
                                int colBend_port2 = col - colBend_centerLine;
                                if (i >= (rodCntHalf - colBend_port2) && i <= (rodCntHalf + defectRodCnt - 1 - colBend_port2))
                                {
                                    if (col == (colBend_centerLine)) // ベンド中央
                                    {
                                    }
                                    else if (col == (colBend_centerLine + 1)) // 出力側ロッド
                                    {
                                        if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                        {
                                            continue;
                                        }
                                        rr = rodRadius_Small_Zigzag;
                                        nr = rodRadiusDiv_Small_Zigzag;
                                        nc = rodCircleDiv_Small_Zigzag;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            // ベンド上側角
                            if ((col == (colBend_centerLine - 3) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt * 2 + rodCntMiddle + 1)))
                                || (col == (colBend_centerLine - 2) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt * 2 + rodCntMiddle + 2)))
                                || (col == colBend_centerLine && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt * 2 + rodCntMiddle + 2)))
                                )
                            {
                                rr = rodRadius_Big_Zigzag;
                            }
                            // ジグザグ中央
                            if (col == (colBend_centerLine - 1) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt * 2 + rodCntMiddle + 1)))
                            {
                                if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                {
                                    continue;
                                }
                                rr = rodRadius_Small_Zigzag;
                                nr = rodRadiusDiv_Small_Zigzag;
                                nc = rodCircleDiv_Small_Zigzag;
                            }
                            // ベンド下側
                            if ((col == (colBend_centerLine - 3) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt + rodCntMiddle)))
                                || (col == (colBend_centerLine - 1) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt + rodCntMiddle)))
                                || (col == (colBend_centerLine) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt + rodCntMiddle + 1)))
                                || (col == (colBend_centerLine + 2) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt + rodCntMiddle + 1)))
                                || (col == (colBend_centerLine + 3) && i == (rodCnt_BendY - (rodCntHalf + defectRodCnt + rodCntMiddle + 2)))
                                )
                            {
                                rr = rodRadius_Big_Zigzag;
                            }

                            // 左ベンド
                            if ((col == ((periodCntInputWg1 + rodCntDiscon) * 2) && i == (rodCntHalf - 1))
                                || (col == ((periodCntInputWg1 + rodCntDiscon) * 2 + 2) && i == (rodCntHalf - 1))
                                )
                            {
                                if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                {
                                    continue;
                                }
                                rr = rodRadius_Small_Zigzag;
                                nr = rodRadiusDiv_Small_Zigzag;
                                nc = rodCircleDiv_Small_Zigzag;
                            }
                        }


                        if ((col % 2 == 1 && (Math.Abs(i - (rodCnt_BendY - rodCnt_Wg2)) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (Math.Abs(i - (rodCnt_BendY - rodCnt_Wg2)) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
                            //uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                            x0 += ofs_x_rod;
                            y0 += ofs_y_rod;
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rr, nc, nr);
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
                            else if (inputWgNo == 4)
                            {
                                rodLoopIds_InputWg4.Add(lId);
                            }
                        }
                    }
                }

                /////////////////////////////////////////////////////////////
                // 導波路２
                for (int col = (periodCntX_BendX4 * 2); col <= (periodCntX_port2 * 2 - 1); col++)
                {
                    if (col == ((periodCntX_port2 - periodCntInputWg1) * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)
                    uint baseLoopId = 0;
                    int inputWgNo = 0;
                    if (col >= (periodCntX_port2 - periodCntInputWg1) * 2)
                    {
                        baseLoopId = 3;
                        inputWgNo = 2;
                    }
                    else
                    {
                        baseLoopId = 2;
                        inputWgNo = 0;
                    }

                    for (int i = (rodCnt_BendY - rodCntHalf * 2 - defectRodCnt); i < rodCnt_BendY; i++)
                    {
                        double rr = rodRadius;
                        int nr = rodRadiusDiv;
                        if (inputWgNo == 0)
                        {
                            // 分割数を調整
                            nr = 2;
                        }

                        // 欠陥(導波路2)
                        if (i >= (rodCnt_BendY - 1 - rodCntHalf - defectRodCnt + 1) && i <= (rodCnt_BendY - 1 - rodCntHalf))
                        {
                            continue;
                        }

                        if ((col % 2 == 1 && (Math.Abs(i - (rodCnt_BendY - rodCnt_Wg2)) % 2 == (isShift180 ? 1 : 0)))
                            || (col % 2 == 0 && (Math.Abs(i - (rodCnt_BendY - rodCnt_Wg2)) % 2 == (isShift180 ? 0 : 1))))
                        {
                            // 中央ロッド
                            double x0 = rodDistanceX * 0.5 * col;
                            double y0 = WaveguideWidth - i * rodDistanceY - rodDistanceY * 0.5;
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
                            else if (inputWgNo == 4)
                            {
                                rodLoopIds_InputWg4.Add(lId);
                            }
                        }
                    }
                }

                /////////////////////////////////////////////////////////////
                // 出力導波路側ロッド
                for (int portIndex = 2; portIndex <= 3; portIndex++)
                {
                    // 上のロッドを追加
                    for (int colIndex = 0; colIndex < 2; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                    {
                        // 上のロッド
                        IList<uint> work_id_v_list_rod_B = null;
                        double x1_B = 0;
                        double y1_B = 0;
                        uint baseLoopId = 0;
                        int inputWgNo = 0;

                        if (portIndex == 2 && colIndex == 0)
                        {
                            // 出力境界 外側
                            x1_B = port3_X1_B5;
                            y1_B = port3_Y1_B5;
                            work_id_v_list_rod_B = id_v_list_rod_B5;
                            // 出力導波路領域
                            baseLoopId = 4;
                            inputWgNo = 3;
                        }
                        else if (portIndex == 2 && colIndex == 1)
                        {
                            // 出力境界内側
                            x1_B = port3_X1_B6;
                            y1_B = port3_Y1_B6;
                            work_id_v_list_rod_B = id_v_list_rod_B6;
                            // 不連続領域
                            baseLoopId = 2;
                            inputWgNo = 0;
                        }
                        else if (portIndex == 3 && colIndex == 0)
                        {
                            // 左出力境界 外側
                            x1_B = port4_X1_B7;
                            y1_B = port4_Y1_B7;
                            work_id_v_list_rod_B = id_v_list_rod_B7;
                            // 左出力導波路領域
                            baseLoopId = 5;
                            inputWgNo = 4;
                        }
                        else if (portIndex == 3 && colIndex == 1)
                        {
                            // 左出力境界内側
                            x1_B = port4_X1_B8;
                            y1_B = port4_Y1_B8;
                            work_id_v_list_rod_B = id_v_list_rod_B8;
                            // 不連続領域
                            baseLoopId = 2;
                            inputWgNo = 0;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        //for (int i = 0; i < rodCntHalf; i++)
                        for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt); i++)
                        {
                            if (portIndex == 3)
                            {
                                // ポート4は欠陥部も埋める
                            }
                            else
                            {
                                if (i >= rodCntHalf) continue;
                            }
                            if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                            {
                                int i2 = 0;
                                if (portIndex == 2)
                                {
                                    i2 = bRodCntHalf_port1 - 1 - (int)((rodCntHalf - 1 - i) / 2);
                                }
                                else if (portIndex == 3)
                                {
                                    i2 = i / 2;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(false);
                                }
                                // 上のロッド
                                {
                                    uint id_v0 = 0;
                                    uint id_v1 = 0;
                                    uint id_v2 = 0;
                                    {
                                        id_v0 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v2 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                    }
                                    double y_proj = i * rodDistanceY + rodDistanceY * 0.5;
                                    double x0 = x1_B + y_proj * Math.Sqrt(3.0) / 2.0;
                                    double y0 = 0.0;
                                    if (portIndex == 2)
                                    {
                                        y0 = y1_B - y_proj / 2.0;
                                    }
                                    else
                                    {
                                        y0 = y1_B + y_proj / 2.0;
                                    }
                                    double startAngle = 0.0;
                                    if (portIndex == 2)
                                    {
                                        startAngle = (0.0 - 30.0);
                                    }
                                    else
                                    {
                                        startAngle = (0.0 + 30.0);
                                    }
                                    CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
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
                                        startAngle,
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
                                    else if (inputWgNo == 3)
                                    {
                                        rodLoopIds_InputWg3.Add(lId);
                                    }
                                    else if (inputWgNo == 4)
                                    {
                                        rodLoopIds_InputWg4.Add(lId);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < rodCntHalf; i++)
                        {
                            if (portIndex == 3) continue; // ポート4は上の処理で追加済みなのでスキップ
                            if (i % 2 == (isShift180 ? 0 : 1))
                            {
                                int i2 = i / 2;
                                // 上のロッド
                                {
                                    uint id_v0 = 0;
                                    uint id_v1 = 0;
                                    uint id_v2 = 0;
                                    {
                                        id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                    }
                                    double y_proj = WaveguideWidth - rodDistanceY * rodCntHalf + i * rodDistanceY + rodDistanceY * 0.5;
                                    double x0 = x1_B + y_proj * Math.Sqrt(3.0) / 2.0;
                                    double y0 = 0.0;
                                    if (portIndex == 2)
                                    {
                                        y0 = y1_B - y_proj / 2.0;
                                    }
                                    else
                                    {
                                        y0 = y1_B + y_proj / 2.0;
                                    }
                                    double startAngle = 0.0;
                                    if (portIndex == 2)
                                    {
                                        startAngle = (0.0 - 30.0);
                                    }
                                    else
                                    {
                                        startAngle = (0.0 + 30.0);
                                    }
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
                                        startAngle,
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
                                    else if (inputWgNo == 3)
                                    {
                                        rodLoopIds_InputWg3.Add(lId);
                                    }
                                    else if (inputWgNo == 4)
                                    {
                                        rodLoopIds_InputWg4.Add(lId);
                                    }
                                }
                            }
                        }
                    }

                    // 下のロッドを追加
                    for (int colIndex = 0; colIndex < 1; colIndex++) // このcolIndexは特に図面上のカラムを指すわけではない（ループ変数)
                    {
                        // 下のロッド
                        IList<uint> work_id_v_list_rod_B = null;
                        double x1_B = 0;
                        double y1_B = 0;
                        uint baseLoopId = 0;
                        int inputWgNo = 0;

                        if (portIndex == 2 && colIndex == 0)
                        {
                            // 出力境界 内側
                            x1_B = port3_X1_B6;
                            y1_B = port3_Y1_B6;
                            work_id_v_list_rod_B = id_v_list_rod_B6;
                            // 出力導波路領域
                            baseLoopId = 4;
                            inputWgNo = 3;
                        }
                        else if (portIndex == 3 && colIndex == 0)
                        {
                            // 左出力境界 内側
                            x1_B = port4_X1_B8;
                            y1_B = port4_Y1_B8;
                            work_id_v_list_rod_B = id_v_list_rod_B8;
                            // 左出力導波路領域
                            baseLoopId = 5;
                            inputWgNo = 4;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        //for (int i = 0; i < rodCntHalf; i++)
                        for (int i = 0; i < (rodCntHalf * 2 + defectRodCnt); i++)
                        {
                            if (portIndex == 3)
                            {
                                // ポート4は欠陥部も埋める
                            }
                            else
                            {
                                if (i >= rodCntHalf) continue;
                            }
                            if (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))
                            {
                                int i2 = 0;
                                if (portIndex == 2)
                                {
                                    i2 = bRodCntHalf_port1 - 1 - (int)((rodCntHalf - 1 - i) / 2);
                                }
                                else if (portIndex == 3)
                                {
                                    i2 = i / 2;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(false);
                                }
                                // 下のロッド
                                {
                                    uint id_v0 = 0;
                                    uint id_v1 = 0;
                                    uint id_v2 = 0;
                                    {
                                        id_v0 = work_id_v_list_rod_B[(rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v1 = work_id_v_list_rod_B[(rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v2 = work_id_v_list_rod_B[0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    }
                                    double y_proj = i * rodDistanceY + rodDistanceY * 0.5;
                                    double x0 = x1_B + y_proj * Math.Sqrt(3.0) / 2.0;
                                    double y0 = 0.0;
                                    if (portIndex == 2)
                                    {
                                        y0 = y1_B - y_proj / 2.0;
                                    }
                                    else
                                    {
                                        y0 = y1_B + y_proj / 2.0;
                                    }
                                    double startAngle = 0.0;
                                    if (portIndex == 2)
                                    {
                                        startAngle = (180.0 - 30.0);
                                    }
                                    else
                                    {
                                        startAngle = (180.0 + 30.0);
                                    }
                                    CVector2D pt_center = cad2d.GetVertexCoord(id_v1);
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
                                        startAngle,
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
                                    else if (inputWgNo == 3)
                                    {
                                        rodLoopIds_InputWg3.Add(lId);
                                    }
                                    else if (inputWgNo == 4)
                                    {
                                        rodLoopIds_InputWg4.Add(lId);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < rodCntHalf; i++)
                        {
                            if (portIndex == 3) continue; // ポート4は上の処理で追加済みなのでスキップ
                            if (i % 2 == (isShift180 ? 0 : 1))
                            {
                                int i2 = i / 2;
                                // 下のロッド
                                {
                                    uint id_v0 = 0;
                                    uint id_v1 = 0;
                                    uint id_v2 = 0;
                                    {
                                        id_v0 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv * 2) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v1 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + (rodRadiusDiv) + i2 * (rodRadiusDiv * 2 + 1)];
                                        id_v2 = work_id_v_list_rod_B[work_id_v_list_rod_B.Count / 2 + 0 + i2 * (rodRadiusDiv * 2 + 1)];
                                    }
                                    double y_proj = WaveguideWidth - rodDistanceY * rodCntHalf + i * rodDistanceY + rodDistanceY * 0.5;
                                    double x0 = x1_B + y_proj * Math.Sqrt(3.0) / 2.0;
                                    double y0 = 0.0;
                                    if (portIndex == 2)
                                    {
                                        y0 = y1_B - y_proj / 2.0;
                                    }
                                    else
                                    {
                                        y0 = y1_B + y_proj / 2.0;
                                    }
                                    double startAngle = 0.0;
                                    if (portIndex == 2)
                                    {
                                        startAngle = (180.0 - 30.0);
                                    }
                                    else
                                    {
                                        startAngle = (180.0 + 30.0);
                                    }
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
                                        startAngle,
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
                                    else if (inputWgNo == 3)
                                    {
                                        rodLoopIds_InputWg3.Add(lId);
                                    }
                                    else if (inputWgNo == 4)
                                    {
                                        rodLoopIds_InputWg4.Add(lId);
                                    }
                                }
                            }
                        }
                    }
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                // 中央のロッド (出力導波路)
                int periodCntInputWg2 = 1;
                int periodCntY = periodCntInputWg2 + rodCntDiscon_port3;

                // 中央のロッド(出力導波路)(左右の強制境界と交差する円)と境界の交点
                IList<uint> id_v_list_F1 = new List<uint>();
                IList<uint> id_v_list_F2 = new List<uint>();

                // 中央のロッド (出力導波路)
                for (int portIndex = 2; portIndex <= 3; portIndex++)
                {
                    for (int col = 1; col <= (periodCntY * 2 - 2); col++)
                    {
                        if (col == (periodCntInputWg2 * 2)) continue; // 入力導波路内部境界 (既にロッド追加済み)

                        uint baseLoopId = 0;
                        int inputWgNo = 0;
                        if (col >= 0 && col < (periodCntInputWg2 * 2))
                        {
                            if (portIndex == 2)
                            {
                                baseLoopId = 4;
                                inputWgNo = 3;
                            }
                            else
                            {
                                baseLoopId = 5;
                                inputWgNo = 4;
                            }
                        }
                        else
                        {
                            baseLoopId = 2;
                            inputWgNo = 0;
                        }
                        double x1_B = 0.0;
                        double y1_B = 0.0;
                        if (portIndex == 2)
                        {
                            x1_B = port3_X1_B5 - col * ((rodDistanceX * 0.5) / 2.0);
                            y1_B = port3_Y1_B5 - col * ((rodDistanceX * 0.5) * Math.Sqrt(3.0) / 2.0);
                        }
                        else
                        {
                            x1_B = port4_X1_B7 + col * ((rodDistanceX * 0.5) / 2.0);
                            y1_B = port4_Y1_B7 - col * ((rodDistanceX * 0.5) * Math.Sqrt(3.0) / 2.0);
                        }

                        // 中央のロッド(出力導波路)
                        for (int i = 0; i <= (rodCntHalf * 2 + defectRodCnt); i++)
                        {
                            if (rodCntHalf == 5)
                            {
                                if (col < (periodCntY * 2 - 2))
                                {
                                    if (i >= (rodCntHalf * 2 + defectRodCnt)) continue;
                                }
                            }
                            else
                            {
                                if (i >= (rodCntHalf * 2 + defectRodCnt)) continue;
                            }

                            double rr = rodRadius;
                            int nr = rodRadiusDiv;
                            if (inputWgNo == 0 || portIndex == 3)
                            {
                                // 分割数を調整
                                nr = 2;
                            }
                            // 欠陥部
                            if (i == (rodCntHalf))
                            {
                                if (portIndex == 3)
                                {
                                    if (col == (periodCntY * 2 - 2)) // ベンドから2格子目
                                    {
                                        continue;
                                    }
                                    else if (col == (periodCntY * 2 - 4)) // ベンドから3格子目
                                    {
                                        //rr = 0.20 * latticeA;
                                        //nr = 1;
                                        //rr = 0.40 * latticeA;
                                        //nr = 2;
                                    }
                                    else
                                    {
                                        // ポート4は終端する
                                    }
                                }
                                else if (!isOptBend)
                                {
                                    continue;
                                }
                                else
                                {
                                    // ベンド最適形状：ジグザグ
                                    if (col == (periodCntY * 2 - 2))
                                    {
                                        if (Math.Abs(rodRadius_Small_Zigzag) < Constants.PrecisionLowerLimit)
                                        {
                                            continue;
                                        }
                                        rr = rodRadius_Small_Zigzag;
                                        nr = rodRadiusDiv_Small_Zigzag;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            if ((col % 2 == 1 && (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 1 : 0)))
                                || (col % 2 == 0 && (Math.Abs(rodCntHalf - 1 - i) % 2 == (isShift180 ? 0 : 1))))
                            {
                                // 中央ロッド
                                double y_proj = i * rodDistanceY + rodDistanceY * 0.5;
                                double x0 = x1_B + y_proj * Math.Sqrt(3.0) / 2.0;
                                double y0 = 0.0;
                                if (portIndex == 2)
                                {
                                    y0 = y1_B - y_proj / 2.0;
                                }
                                else
                                {
                                    y0 = y1_B + y_proj / 2.0;
                                }
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
                                else if (inputWgNo == 4)
                                {
                                    rodLoopIds_InputWg4.Add(lId);
                                }
                            }
                        }
                    }
                }

                // ポート4の外部境界を強制境界辺リストに登録
                {
                    uint eId = 16;
                    id_e_F1.Add(eId);
                }
                for (int i = 0; i < (ndivPlus_port4 - 1); i++)
                {
                    uint eId = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 + (ndivPlus_port3 - 1) * 2 + i + 1);
                    id_e_F1.Add(eId);
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
                    foreach (uint eId in id_e_rod_B7)
                    {
                        cad2d.SetColor_Edge(eId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    foreach (uint eId in id_e_rod_B8)
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
                uint[] loopId_cad_list = new uint[5 + rodLoopIds.Count];
                loopId_cad_list[0] = 1;
                loopId_cad_list[1] = 2;
                loopId_cad_list[2] = 3;
                loopId_cad_list[3] = 4;
                loopId_cad_list[4] = 5;
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    loopId_cad_list[i + 5] = rodLoopIds[i];
                }
                int[] mediaIndex_list = new int[5 + rodLoopIds.Count];
                for (int i = 0; i < 5; i++)
                {
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    mediaIndex_list[i + 5] = Medias.IndexOf(mediaCore);
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

            // ポート情報リスト作成
            for (int portIndex = 0; portIndex < portCnt_real; portIndex++)
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
                WgPortInfoList[2].IncidentModeIndex = incidentModeIndex;
                WgPortInfoList[3].IncidentModeIndex = incidentModeIndex;
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
                uint[] eId_cad_list = new uint[16 + id_e_F1.Count + id_e_F2.Count];
                eId_cad_list[0] = 2;
                eId_cad_list[1] = 3;
                eId_cad_list[2] = 4;
                eId_cad_list[3] = 6;
                eId_cad_list[4] = 7;
                eId_cad_list[5] = 8;
                eId_cad_list[6] = 9;
                eId_cad_list[7] = 11;
                eId_cad_list[8] = 12;
                eId_cad_list[9] = 13;
                eId_cad_list[10] = 14;
                eId_cad_list[11] = 15;
                eId_cad_list[12] = 17;
                eId_cad_list[13] = 18;
                eId_cad_list[14] = 19;
                eId_cad_list[15] = 20;
                for (int i = 0; i < id_e_F1.Count; i++)
                {
                    eId_cad_list[16 + i] = id_e_F1[i];
                }
                for (int i = 0; i < id_e_F2.Count; i++)
                {
                    eId_cad_list[16 + id_e_F1.Count + i] = id_e_F2[i];
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
            for (int portIndex = 0; portIndex < portCnt_real; portIndex++)
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
                else if (portIndex == 2)
                {
                    ndivPlus = ndivPlus_port3;
                }
                else if (portIndex == 3)
                {
                    ndivPlus = ndivPlus_port4;
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
                    eId_cad_list[0] = 5;
                    work_id_e_rod_B = id_e_rod_B3;
                }
                else if (portIndex == 2)
                {
                    eId_cad_list[0] = 10;
                    work_id_e_rod_B = id_e_rod_B5;
                }
                else if (portIndex == 3)
                {
                    eId_cad_list[0] = 16;
                    work_id_e_rod_B = id_e_rod_B7;
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
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 + (ndivPlus_port3 - 1) - (i - 1));
                    }
                    else if (portIndex == 3)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 + (ndivPlus_port3 - 1) * 2 + (ndivPlus_port4 - 1) - (i - 1));
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
            for (int portIndex = 0; portIndex < portCnt_real; portIndex++)
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
                else if (portIndex == 3)
                {
                    loopId_cad_list = new uint[1 + rodLoopIds_InputWg4.Count];
                    loopId_cad_list[0] = 5;
                    for (int i = 0; i < rodLoopIds_InputWg4.Count; i++)
                    {
                        loopId_cad_list[i + 1] = rodLoopIds_InputWg4[i];
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
                else if (portIndex == 3)
                {
                    mediaIndex_list = new int[1 + rodLoopIds_InputWg4.Count];
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                    for (int i = 0; i < rodLoopIds_InputWg4.Count; i++)
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
            for (int portIndex = 0; portIndex < portCnt_real; portIndex++)
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
                else if (portIndex == 2)
                {
                    ndivPlus = ndivPlus_port3;
                }
                else if (portIndex == 3)
                {
                    ndivPlus = ndivPlus_port4;
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
                    eId_cad_list[0] = 21;
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
                    eId_cad_list[0] = 22;
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
                    eId_cad_list[0] = 23;
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
                else if (portIndex == 3)
                {
                    eId_cad_list[0] = 24;
                    work_id_e_rod_B = id_e_rod_B8;
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
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 + (ndivPlus_port3 - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 3)
                    {
                        eId_cad_list[i] = (uint)(24 + (ndivPlus_port1 - 1) * 2 + (ndivPlus_port2 - 1) * 2 + (ndivPlus_port3 - 1) * 2 + (ndivPlus_port4 - 1) * 2 - (i - 1));
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
            for (int portIndex = 0; portIndex < portCnt_real; portIndex++)
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
                else if (portIndex == 2)
                {
                    // Y軸から見ての回転角
                    double rotAngle = 60.0 * pi / 180.0;
                    double[] rotOrigin = new double[] { port3_X2_B5, port3_Y2_B5 };
                    WgUtil.GetLoopCoordList(World, wgPortInfo1.FieldInputWgLoopId, rotAngle, rotOrigin, out no_c_all, out to_no_loop, out coord_c_all);
                }
                else if (portIndex == 3)
                {
                    // Y軸から見ての回転角
                    double rotAngle = 120.0 * pi / 180.0;
                    double[] rotOrigin = new double[] { port4_X2_B7, port4_Y2_B7 };
                    WgUtil.GetLoopCoordList(World, wgPortInfo1.FieldInputWgLoopId, rotAngle, rotOrigin, out no_c_all, out to_no_loop, out coord_c_all);
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
                        if (((portIndex == 0 || portIndex == 1) &&
                                (coord[1] >= (wg2_Y1 - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY)
                                  && coord[1] <= (wg2_Y1 - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)))
                            || ((portIndex == 2 || portIndex == 3) &&
                                (coord[1] >= (WaveguideWidth - rodDistanceY * (rodCntHalf + defectRodCnt) - 1.0 * rodDistanceY)
                                  && coord[1] <= (WaveguideWidth - rodDistanceY * rodCntHalf + 1.0 * rodDistanceY)))
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
    }
}
