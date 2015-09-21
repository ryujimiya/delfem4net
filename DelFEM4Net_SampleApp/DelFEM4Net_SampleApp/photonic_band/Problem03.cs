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

namespace photonic_band
{
    class Problem03
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// フォトニック結晶 三角形格子 境界条件を工夫する 上コーナーに1/4ロッド + 下境界中央に1/2ロッド
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="periodicDistanceY"></param>
        /// <param name="isTriLattice"></param>
        /// <param name="calcBetaCnt"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistanceX"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPortBcIds"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="isCadShow"></param>
        /// <param name="CadDrawerAry"></param>
        /// <param name="Camera"></param>
        /// <returns></returns>
        public static bool SetProblem(
            int probNo,
            double periodicDistanceY,
            ref bool isTriLattice,
            ref int calcBetaCnt,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref double latticeA,
            ref double periodicDistanceX,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPortBcIds,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            // この問題では、第一BZ外の波数空間を含むため、計算結果に不要なバンドが含まれます。
            //MessageBox.Show("Note the band diagram contains unwanted \"folded \" bands.", "Note for Problem " + probNo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // フォトニック結晶 三角形格子
            isTriLattice = true; // 三角形格子
            //isTriLattice = false; // 長方形格子
            
            // 空孔？
            //bool isAirHole = false; // dielectric rod
            bool isAirHole = true; // air hole
            // 三角形格子の内角
            double latticeTheta = 60.0;
            // 格子定数
            latticeA = periodicDistanceY / Math.Sin(latticeTheta * pi / 180.0);
            // 周期構造距離
            periodicDistanceX = periodicDistanceY * 2.0 / Math.Tan(latticeTheta * pi / 180.0);
            // ロッドの半径
            double rodRadius = 0.30 * latticeA;
            // ロッドの比誘電率
            double rodEps = 2.76 * 2.76;
            // 格子１辺の分割数
            const int ndivForOneLattice = 10;
            // ロッドの円周分割数
            const int rodCircleDiv = 16;// 12;
            // ロッドの半径の分割数
            const int rodRadiusDiv = 5;// 4;
            // メッシュの長さ
            double meshL = 1.05 * periodicDistanceY / ndivForOneLattice;

            MinNormalizedFreq = 0.000;
            MaxNormalizedFreq = 1.000;
            GraphFreqInterval = 0.10;

            // 波のモード
            if (isAirHole)
            {
                WaveModeDv = WgUtil.WaveModeDV.TM; // air hole
            }
            else
            {
                WaveModeDv = WgUtil.WaveModeDV.TE; // dielectric rod
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
            int ndivPlus_B1 = 0;
            int ndivPlus_B2 = 0;
            int ndivPlus_B3 = 0;
            int ndivPlus_B4 = 0;
            int rodCntY = 2;
            IList<uint> id_e_rod_B1 = new List<uint>();
            IList<uint> id_e_rod_B2 = new List<uint>();
            IList<uint> id_e_rod_B3 = new List<uint>();
            IList<uint> id_e_rod_B4 = new List<uint>();
            // ワールド座標系
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                // 領域を追加
                {
                    List<CVector2D> pts = new List<CVector2D>();
                    pts.Add(new CVector2D(0.0, periodicDistanceY));
                    pts.Add(new CVector2D(0.0, 0.0));
                    pts.Add(new CVector2D(periodicDistanceX, 0.0));
                    pts.Add(new CVector2D(periodicDistanceX, periodicDistanceY));
                    // 多角形追加
                    uint lId = cad2d.AddPolygon(pts).id_l_add;
                    baseLoopId = lId;
                }
                // 周期構造境界上の頂点を追加
                IList<double> ys_B1 = new List<double>();
                IList<double> ys_rod_B1 = new List<double>();
                IList<double> ys_B2 = new List<double>();
                IList<double> ys_rod_B2 = new List<double>();
                IList<double> xs_B3 = new List<double>();
                IList<double> xs_rod_B3 = new List<double>();
                IList<double> xs_B4 = new List<double>();
                IList<double> xs_rod_B4 = new List<double>();
                IList<uint> id_v_list_rod_B1 = new List<uint>();
                IList<uint> id_v_list_rod_B2 = new List<uint>();
                IList<uint> id_v_list_rod_B3 = new List<uint>();
                IList<uint> id_v_list_rod_B4 = new List<uint>();

                int ndivForOneLatticeX = (int)Math.Ceiling((double)ndivForOneLattice * (periodicDistanceX / periodicDistanceY));
                if (ndivForOneLatticeX % 2 == 1) // 上下の境界条件は中点の前後で別個指定するので、丁度分割の点が中点に来るようにする
                {
                    ndivForOneLatticeX--;
                }
                System.Diagnostics.Debug.Assert(ndivForOneLatticeX % 2 == 0);

                int outofAreaRodPtCnt_B1 = rodRadiusDiv + 1;
                int outofAreaRodPtCnt_B2 = rodRadiusDiv + 1;
                int outofAreaRodPtCnt_B4 = rodRadiusDiv + 1;
                for (int axisIndex = 0; axisIndex < 2; axisIndex++) // X方向、Y方向の意味
                {
                    for (int boundaryIndex = 0; boundaryIndex < 2; boundaryIndex++)
                    {
                        int cur_ndivForOneLattice = 0;
                        double cur_periodicDistanceY = 0.0;
                        IList<double> ys = null;
                        IList<double> ys_rod = null;
                        int cur_rodCntY = 0;

                        if (axisIndex == 0 && boundaryIndex == 0)
                        {
                            // X方向周期(境界1)
                            cur_ndivForOneLattice = ndivForOneLattice;
                            cur_periodicDistanceY = periodicDistanceY;
                            ys = ys_B1;
                            ys_rod = ys_rod_B1;
                            cur_rodCntY = 1;
                            System.Diagnostics.Debug.Assert(ys.Count == 0);
                            System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                        }
                        else if (axisIndex == 0 && boundaryIndex == 1)
                        {
                            // X方向周期(境界2)
                            cur_ndivForOneLattice = ndivForOneLattice;
                            cur_periodicDistanceY = periodicDistanceY;
                            ys = ys_B2;
                            ys_rod = ys_rod_B2;
                            cur_rodCntY = 1;
                            System.Diagnostics.Debug.Assert(ys.Count == 0);
                            System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                        }
                        else if (axisIndex == 1 && boundaryIndex == 0)
                        {
                            // Y方向周期(境界3)
                            cur_ndivForOneLattice = ndivForOneLatticeX;
                            cur_periodicDistanceY = periodicDistanceX;
                            ys = xs_B3;
                            ys_rod = xs_rod_B3;
                            cur_rodCntY = 1;
                            System.Diagnostics.Debug.Assert(ys.Count == 0);
                            System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                        }
                        else if (axisIndex == 1 && boundaryIndex == 1)
                        {
                            // Y方向周期(境界4)
                            cur_ndivForOneLattice = ndivForOneLatticeX;
                            cur_periodicDistanceY = periodicDistanceX;
                            ys = xs_B4;
                            ys_rod = xs_rod_B4;
                            cur_rodCntY = rodCntY;
                            System.Diagnostics.Debug.Assert(ys.Count == 0);
                            System.Diagnostics.Debug.Assert(ys_rod.Count == 0);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        if (axisIndex == 1 && boundaryIndex == 0)
                        {
                            // 中点のロッド
                            // 境界上にロッドのある格子
                            // 境界上のロッドの頂点
                            {
                                double y0 = cur_periodicDistanceY * 0.5;
                                ys_rod.Add(y0);
                                for (int k = 1; k <= rodRadiusDiv; k++)
                                {
                                    double y1 = y0 - k * rodRadius / rodRadiusDiv;
                                    double y2 = y0 + k * rodRadius / rodRadiusDiv;
                                    ys_rod.Add(y1);
                                    ys_rod.Add(y2);
                                }
                            }
                        }
                        else
                        {
                            // コーナーのロッド
                            // 境界上にロッドのある格子
                            // 境界上のロッドの頂点
                            for (int i = 0; i < cur_rodCntY; i++)
                            {
                                double y0 = cur_periodicDistanceY - i * cur_periodicDistanceY;
                                //ys_rod.Add(y0);
                                for (int k = 1; k <= rodRadiusDiv; k++)
                                {
                                    if (i == 0)
                                    {
                                        double y1 = y0 - k * rodRadius / rodRadiusDiv;
                                        ys_rod.Add(y1);
                                    }
                                    else if (i == 1)
                                    {
                                        double y2 = y0 + k * rodRadius / rodRadiusDiv;
                                        ys_rod.Add(y2);
                                    }
                                }
                            }
                        }
                        foreach (double y_rod in ys_rod)
                        {
                            ys.Add(y_rod);
                        }
                        // 境界上のロッドの外の頂点はロッドから少し離さないとロッドの追加で失敗するのでマージンをとる
                        double radiusMargin = cur_periodicDistanceY * 0.01;
                        if (axisIndex == 1 && boundaryIndex == 0)
                        {
                            // 中点のロッド
                            // 境界上にロッドのある格子
                            // ロッドの外
                            {
                                for (int k = 1; k <= (cur_ndivForOneLattice - 1); k++)
                                {
                                    double y_divpt = cur_periodicDistanceY - k * (cur_periodicDistanceY / cur_ndivForOneLattice);
                                    double y_min_rod = cur_periodicDistanceY * 0.5 - rodRadius + radiusMargin;
                                    double y_max_rod = cur_periodicDistanceY * 0.5 + rodRadius - radiusMargin;
                                    if (y_divpt < (y_min_rod - Constants.PrecisionLowerLimit) || y_divpt > (y_max_rod - Constants.PrecisionLowerLimit))
                                    {
                                        ys.Add(y_divpt);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // コーナーのロッド
                            // 境界上にロッドのある格子
                            // ロッドの外
                            {
                                for (int k = 1; k <= (cur_ndivForOneLattice - 1); k++)
                                {
                                    double y_divpt = cur_periodicDistanceY - k * (cur_periodicDistanceY / cur_ndivForOneLattice);
                                    double y_min_rod = rodRadius + radiusMargin;
                                    double y_max_rod = cur_periodicDistanceY - rodRadius - radiusMargin;
                                    if ((axisIndex == 1
                                          && (y_divpt >= (y_min_rod + Constants.PrecisionLowerLimit) && y_divpt < (y_max_rod - Constants.PrecisionLowerLimit)))
                                        || (axisIndex == 0
                                          && (y_divpt < (y_max_rod - Constants.PrecisionLowerLimit)))
                                        )
                                    {
                                        ys.Add(y_divpt);
                                    }
                                }
                            }
                        }
                        // 昇順でソート
                        double[] yAry = ys.ToArray();
                        Array.Sort(yAry);
                        int cur_ndivPlus = 0;
                        cur_ndivPlus = yAry.Length + 1;
                        if (axisIndex == 0 && boundaryIndex == 0)
                        {
                            ndivPlus_B1 = cur_ndivPlus;
                        }
                        else if (axisIndex == 0 && boundaryIndex == 1)
                        {
                            ndivPlus_B2 = cur_ndivPlus;
                        }
                        else if (axisIndex == 1 && boundaryIndex == 0)
                        {
                            ndivPlus_B3 = cur_ndivPlus;
                        }
                        else if (axisIndex == 1 && boundaryIndex == 1)
                        {
                            ndivPlus_B4 = cur_ndivPlus;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        // yAryは昇順なので、yAryの並びの順に追加すると境界1上を逆方向に移動することになる
                        //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                        int cur_outofAreaRodPtCnt = 0;
                        bool isInRod = false;
                        if (axisIndex == 0 && boundaryIndex == 0)
                        {
                            isInRod = false;
                            cur_outofAreaRodPtCnt = 0;
                        }
                        else if (axisIndex == 0 && boundaryIndex == 1)
                        {
                            isInRod = true;
                            cur_outofAreaRodPtCnt = outofAreaRodPtCnt_B2;
                        }
                        else if (axisIndex == 1 && boundaryIndex == 0)
                        {
                            isInRod = false;
                            cur_outofAreaRodPtCnt = 0;
                        }
                        else if (axisIndex == 1 && boundaryIndex == 1)
                        {
                            isInRod = true;
                            cur_outofAreaRodPtCnt = outofAreaRodPtCnt_B4;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }

                        for (int i = 0; i < yAry.Length; i++)
                        {
                            uint id_e = 0;
                            double x_pt = 0.0;
                            double y_pt = 0.0;

                            IList<uint> work_id_e_rod_B = null;
                            IList<uint> work_id_v_list_rod_B = null;
                            int yAryIndex = 0;
                            if (axisIndex == 0 && boundaryIndex == 0)
                            {
                                // 境界1：左側
                                id_e = 1;
                                x_pt = 0.0;
                                y_pt = yAry[i];
                                yAryIndex = i;
                                work_id_e_rod_B = id_e_rod_B1;
                                work_id_v_list_rod_B = id_v_list_rod_B1;
                            }
                            else if (axisIndex == 0 && boundaryIndex == 1)
                            {
                                // 境界2：右側
                                id_e = 3;
                                x_pt = periodicDistanceX;
                                y_pt = yAry[yAry.Length - 1 - i];
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B2;
                                work_id_v_list_rod_B = id_v_list_rod_B2;
                            }
                            else if (axisIndex == 1 && boundaryIndex == 0)
                            {
                                // 境界3：下側
                                id_e = 2;
                                x_pt = yAry[yAry.Length - 1 - i];
                                y_pt = 0.0;
                                yAryIndex = yAry.Length - 1 - i;
                                work_id_e_rod_B = id_e_rod_B3;
                                work_id_v_list_rod_B = id_v_list_rod_B3;
                            }
                            else if (axisIndex == 1 && boundaryIndex == 1)
                            {
                                // 境界4：上側
                                id_e = 4;
                                x_pt = yAry[i];
                                y_pt = periodicDistanceY;
                                yAryIndex = i;
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
                                if ((work_id_v_list_rod_B.Count + cur_outofAreaRodPtCnt) % (rodRadiusDiv * 2 + 1) == 1)
                                {
                                    isInRod = true;
                                }
                                else if ((work_id_v_list_rod_B.Count + cur_outofAreaRodPtCnt) % (rodRadiusDiv * 2 + 1) == 0)
                                {
                                    isInRod = false;
                                }
                            }
                            if (i == (yAry.Length - 1))
                            {
                                if (axisIndex == 0 && boundaryIndex == 0)
                                {
                                    System.Diagnostics.Debug.Assert(isInRod == true);
                                    work_id_e_rod_B.Add(id_e);
                                }
                                else if (axisIndex == 0 && boundaryIndex == 1)
                                {
                                    System.Diagnostics.Debug.Assert(isInRod == false);
                                }
                                else if (axisIndex == 1 && boundaryIndex == 0)
                                {
                                    System.Diagnostics.Debug.Assert(isInRod == false);
                                }
                                else if (axisIndex == 1 && boundaryIndex == 1)
                                {
                                    System.Diagnostics.Debug.Assert(isInRod == true);
                                    work_id_e_rod_B.Add(id_e);
                                }
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.Assert(id_v_list_rod_B1.Count == (rodRadiusDiv * 2 + 1 - outofAreaRodPtCnt_B1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B2.Count == (rodRadiusDiv * 2 + 1 - outofAreaRodPtCnt_B1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B3.Count == (rodRadiusDiv * 2 + 1));
                System.Diagnostics.Debug.Assert(id_v_list_rod_B4.Count == rodCntY * (rodRadiusDiv * 2 + 1 - outofAreaRodPtCnt_B4));
                System.Diagnostics.Debug.Assert(ndivPlus_B1 == ndivPlus_B2);
                System.Diagnostics.Debug.Assert(ndivPlus_B3 == ndivPlus_B4);

                uint id_v_B1_top_rod_center = 1;
                //uint id_v_B1_bottom_rod_center = 2;
                uint id_v_B2_top_rod_center = 4;
                //uint id_v_B2_bottom_rod_center = 3;
                // 下のロッド
                {
                    uint id_v0 = 0;
                    uint id_v1 = 0;
                    uint id_v2 = 0;
                    {
                        int index_v0 = rodRadiusDiv * 2;
                        int index_v1 = rodRadiusDiv;
                        int index_v2 = 0;
                        id_v0 = id_v_list_rod_B3[index_v0];
                        id_v1 = id_v_list_rod_B3[index_v1];
                        id_v2 = id_v_list_rod_B3[index_v2];
                    }
                    double x0 = periodicDistanceX * 0.5;
                    double y0 = 0.0;
                    uint lId = 0;
                    // 下のロッド
                    lId = WgCadUtil.AddBottomRod(
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
                }

                // 上のロッド
                for (int rodIndex = 0; rodIndex < 2; rodIndex++)
                {
                    uint id_v0 = 0;
                    uint id_v1 = 0;
                    uint id_v2 = 0;
                    double x0 = 0.0;
                    double y0 = 0.0;
                    double startAngle = 0.0;
                    double endAngle = 0.0;
                    bool isReverseAddVertex = false;
                    if (rodIndex == 0)
                    {
                        // 左上
                        int index_v0 = rodRadiusDiv - 1;
                        int index_v2 = 0;
                        id_v0 = id_v_list_rod_B4[index_v0];
                        id_v1 = id_v_B1_top_rod_center;
                        id_v2 = id_v_list_rod_B1[index_v2];
                        x0 = 0.0;
                        y0 = periodicDistanceY;
                        startAngle = 360.0;
                        endAngle = 270.0;
                        isReverseAddVertex = true;
                    }
                    else if (rodIndex == 1)
                    {
                        // 右上
                        int index_v0 = rodRadiusDiv - 1;
                        int index_v2 = rodRadiusDiv;
                        id_v0 = id_v_list_rod_B2[index_v0];
                        id_v1 = id_v_B2_top_rod_center;
                        id_v2 = id_v_list_rod_B4[index_v2];
                        x0 = periodicDistanceX;
                        y0 = periodicDistanceY;
                        startAngle = 270.0;
                        endAngle = 180.0;
                        isReverseAddVertex = true;
                    }
                    uint lId = 0;
                    // 上の1/4ロッド
                    lId = WgCadUtil.AddQuarterRod(
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
                        endAngle,
                        isReverseAddVertex);
                    rodLoopIds.Add(lId);
                }
                 
                // 図面表示
                //isCadShow = true;
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
                WgUtil.GetPartialField_Loop(
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
            FieldForceBcId = 0; // なし

            // 開口条件1
            for (int boundaryIndex = 0; boundaryIndex < 4; boundaryIndex++)
            {
                int cur_ndivPlus = 0;
                if (boundaryIndex == 0)
                {
                    cur_ndivPlus = ndivPlus_B1;
                }
                else if (boundaryIndex == 1)
                {
                    cur_ndivPlus = ndivPlus_B1;
                }
                else if (boundaryIndex == 2)
                {
                    cur_ndivPlus = ndivPlus_B3;
                }
                else if (boundaryIndex == 3)
                {
                    cur_ndivPlus = ndivPlus_B3;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                uint[] eId_cad_list = new uint[cur_ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];
                IList<uint> work_id_e_rod_B = null;
                if (boundaryIndex == 0)
                {
                    eId_cad_list[0] = 1;
                    work_id_e_rod_B = id_e_rod_B1;
                }
                else if (boundaryIndex == 1)
                {
                    eId_cad_list[0] = 3;
                    work_id_e_rod_B = id_e_rod_B2;
                }
                else if (boundaryIndex == 2)
                {
                    eId_cad_list[0] = 2;
                    work_id_e_rod_B = id_e_rod_B3;
                }
                else if (boundaryIndex == 3)
                {
                    eId_cad_list[0] = 4;
                    work_id_e_rod_B = id_e_rod_B4;
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
                for (int i = 1; i <= cur_ndivPlus - 1; i++)
                {
                    if (boundaryIndex == 0)
                    {
                        eId_cad_list[i] = (uint)(4 + (ndivPlus_B1 - 1) - (i - 1));
                    }
                    else if (boundaryIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(4 + (ndivPlus_B1 - 1) * 2 - (i - 1));
                    }
                    else if (boundaryIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(4 + (ndivPlus_B1 - 1) * 2 + (ndivPlus_B3 - 1) - (i - 1));
                    }
                    else if (boundaryIndex == 3)
                    {
                        eId_cad_list[i] = (uint)(4 + (ndivPlus_B1 - 1) * 2 + (ndivPlus_B3 - 1) * 2 - (i - 1));
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
                uint cur_fieldPortBcId = 0;
                WgUtilForPeriodicEigenBetaSpecified.GetPartialField_Edge(
                    conv,
                    World,
                    eId_cad_list,
                    null,
                    FieldValId,
                    out cur_fieldPortBcId,
                    ref EdgeDic);
                FieldPortBcIds.Add(cur_fieldPortBcId);
            }
            
            return true;
        }
    }
}
