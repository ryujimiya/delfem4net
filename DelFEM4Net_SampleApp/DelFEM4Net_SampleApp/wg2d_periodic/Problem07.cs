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
    class Problem07
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// PC導波路 マルチモード干渉結合器(MMI directional coupler)
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

            // フォトニック結晶導波路 マルチモード干渉結合器(MMI directional coupler)
            // ロッドの数（半分）
            //const int rodCntHalf = 5;
            const int rodCntHalf = 3;
            // 欠陥ロッド数
            const int defectRodCnt = 1; // シングルモード線路
            const int defectRodCntMMI = 5; // 多モード線路
            // 格子の数
            const int latticeCnt = rodCntHalf * 2 + defectRodCntMMI;
            // 格子定数
            latticeA = WaveguideWidth / (double)latticeCnt;
            // 周期構造距離
            periodicDistance = latticeA;
            // ロッドの半径
            double rodRadius = 0.18 * latticeA;
            // ロッドの比誘電率
            double rodEps = 3.4 * 3.4;
            // 格子１辺の分割数
            //const int ndivForOneLattice = 6;
            const int ndivForOneLattice = 5;
            // 境界の総分割数
            const int ndiv = latticeCnt * ndivForOneLattice;
            // ロッドの円周の分割数
            //const int rodCircleDiv = 8;
            const int rodCircleDiv = 8;
            // ロッドの半径の分割数
            const int rodRadiusDiv = 1;
            // 導波路不連続領域の長さ
            //const int rodCntDiscon = 37;  // l = 31a
            const int rodCntDiscon = 34; // l = 28a
            double disconLength = latticeA * rodCntDiscon;
            // 入出力導波路の周期構造部分の長さ
            double inputWgLength = latticeA;
            // メッシュのサイズ
            double meshL = 1.05 * WaveguideWidth / ndiv;

            minEffN = 0.0;
            maxEffN = 1.0;

            NormalizedFreq1 = 0.300;//0.302;
            NormalizedFreq2 = 0.440;//0.443;
            //FreqDelta = 0.002;
            FreqDelta = 0.001;
            GraphFreqInterval = 0.02;

            MinSParameter = 0.0;
            MaxSParameter = 1.0;
            GraphSParameterInterval = 0.2;

            // 媒質リスト作成
            double claddingP = 1.0;
            double claddingQ = 1.0;
            double coreP = 1.0;
            double coreQ = 1.0;
            /*
            if (WaveModeDv == WaveModeDV.TM)
            {
                // TMモード
                claddingP = 1.0 / 1.0;
                claddingQ = 1.0;
                coreP = 1.0 / rodEps;
                coreQ = 1.0;
            }
            else
             */
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
            IList<uint> rodLoopIds = new List<uint>();
            IList<uint> rodLoopIds_InputWg1 = new List<uint>();
            IList<uint> rodLoopIds_InputWg2 = new List<uint>();
            IList<uint> rodLoopIds_InputWg3 = new List<uint>();
            // ポート数
            const uint portCnt = 3;
            // シングルモード導波路の幅の格子数換算
            int latticeCntSingleModeWg = (rodCntHalf * 2 + defectRodCnt);
            // シングルモード導波路の幅
            double singleModeWgWidth = latticeCntSingleModeWg * latticeA;
            // シングルモードの分割数
            int ndivSingleModeWg = latticeCntSingleModeWg * ndivForOneLattice;
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
                    pts.Add(new CVector2D(0.0, (WaveguideWidth - singleModeWgWidth))); // 頂点2
                    pts.Add(new CVector2D(inputWgLength, (WaveguideWidth - singleModeWgWidth))); // 頂点3
                    pts.Add(new CVector2D(inputWgLength, 0)); // 頂点4
                    pts.Add(new CVector2D(inputWgLength + disconLength, 0.0)); // 頂点5
                    pts.Add(new CVector2D(inputWgLength + disconLength, -inputWgLength)); // 頂点6
                    pts.Add(new CVector2D(inputWgLength + disconLength + singleModeWgWidth, -inputWgLength)); // 頂点7
                    pts.Add(new CVector2D(inputWgLength + disconLength + singleModeWgWidth, 0)); // 頂点8
                    pts.Add(new CVector2D(inputWgLength + disconLength + singleModeWgWidth, WaveguideWidth)); // 頂点9
                    pts.Add(new CVector2D(inputWgLength + disconLength + singleModeWgWidth, WaveguideWidth + inputWgLength)); // 頂点10
                    pts.Add(new CVector2D(inputWgLength + disconLength, WaveguideWidth + inputWgLength)); // 頂点11
                    pts.Add(new CVector2D(inputWgLength + disconLength, WaveguideWidth)); // 頂点12
                    pts.Add(new CVector2D(inputWgLength, WaveguideWidth)); // 頂点13
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                }
                // 入出力領域を分離
                uint eIdAdd1 = cad2d.ConnectVertex_Line(3, 13).id_e_add;
                uint eIdAdd2 = cad2d.ConnectVertex_Line(5, 8).id_e_add;
                uint eIdAdd3 = cad2d.ConnectVertex_Line(9, 12).id_e_add;
                // 入出力導波路の周期構造境界上の頂点を追加
                //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                // 入力導波路(ポート1：左)
                {
                    uint id_e = 1;
                    double x1 = 0.0;
                    double y1 = WaveguideWidth;
                    double x2 = x1;
                    double y2 = WaveguideWidth - singleModeWgWidth;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }
                {
                    uint id_e = 14;
                    double x1 = inputWgLength;
                    double y1 = WaveguideWidth - singleModeWgWidth;
                    double x2 = x1;
                    double y2 = WaveguideWidth;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }
                // 出力導波路(ポート2：右下)
                {
                    uint id_e = 6;
                    double x1 = inputWgLength + disconLength;
                    double y1 = -inputWgLength;
                    double x2 = inputWgLength + disconLength + singleModeWgWidth;
                    double y2 = y1;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }
                {
                    uint id_e = 15;
                    double x1 = inputWgLength + disconLength;
                    double y1 = 0;
                    double x2 = inputWgLength + disconLength + singleModeWgWidth;
                    double y2 = y1;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }
                // 出力導波路(ポート3：右上)
                {
                    uint id_e = 10;
                    double x1 = inputWgLength + disconLength + singleModeWgWidth;
                    double y1 = WaveguideWidth + inputWgLength;
                    double x2 = inputWgLength + disconLength;
                    double y2 = y1;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }
                {
                    uint id_e = 16;
                    double x1 = inputWgLength + disconLength + singleModeWgWidth;
                    double y1 = WaveguideWidth;
                    double x2 = inputWgLength + disconLength;
                    double y2 = y1;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndivSingleModeWg, x1, y1, x2, y2);
                }

                // ロッドを追加
                // ポート1
                int rodCntInputWg1 = 1;
                uint[] baseLoopIds = { 1, 3, 4 };
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    uint baseLoopId = baseLoopIds[portIndex];
                    int inputWgNo = portIndex + 1;
                    for (int col = 0; col < rodCntInputWg1; col++)
                    {
                        for (int row = 0; row < rodCntHalf; row++)
                        {
                            double x0 = 0;
                            double y0 = 0;
                            if (inputWgNo == 1)
                            {
                                x0 = latticeA * 0.5 + col * latticeA;
                                y0 = WaveguideWidth - row * latticeA - latticeA * 0.5;
                            }
                            else if (inputWgNo == 2)
                            {
                                x0 = inputWgLength + disconLength + latticeA * 0.5 + row * latticeA;
                                y0 = -inputWgLength + latticeA * 0.5 + col * latticeA;
                            }
                            else if (inputWgNo == 3)
                            {
                                x0 = inputWgLength + disconLength + latticeA * 0.5 + row * latticeA;
                                y0 = WaveguideWidth + inputWgLength - latticeA * 0.5 - col * latticeA;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                            }
                            uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                            System.Diagnostics.Debug.Assert(lId != 0);
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
                        for (int row = 0; row < rodCntHalf; row++)
                        {
                            double x0 = 0;
                            double y0 = 0;
                            if (inputWgNo == 1)
                            {
                                x0 = latticeA * 0.5 + col * latticeA;
                                y0 = WaveguideWidth - singleModeWgWidth + latticeA * rodCntHalf - row * latticeA - latticeA * 0.5;
                            }
                            else if (inputWgNo == 2)
                            {
                                x0 = inputWgLength + disconLength + singleModeWgWidth - latticeA * rodCntHalf + latticeA * 0.5 + row * latticeA;
                                y0 = -inputWgLength + latticeA * 0.5 + col * latticeA;
                            }
                            else if (inputWgNo == 3)
                            {
                                x0 = inputWgLength + disconLength + singleModeWgWidth - latticeA * rodCntHalf + latticeA * 0.5 + row * latticeA;
                                y0 = WaveguideWidth + inputWgLength - latticeA * 0.5 - col * latticeA;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
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
                            else if (inputWgNo == 3)
                            {
                                rodLoopIds_InputWg3.Add(lId);
                            }
                        }
                    }
                }

                int rodCntInputWg1_add = 3;
                int rodCntAll = rodCntDiscon + latticeCntSingleModeWg;
                int rodCntAllMMI = rodCntHalf * 2 + defectRodCntMMI;
                for (int col = 0; col < rodCntAll; col++)
                {
                    uint baseLoopId = 2;
                    for (int row = 0; row < rodCntAllMMI; row++)
                    {
                        if (col >= 0 && col < rodCntInputWg1_add)
                        {
                            if (row == rodCntHalf) continue;
                        }
                        else if (col >= rodCntInputWg1_add && col < (rodCntAll - latticeCntSingleModeWg - rodCntInputWg1_add))
                        {
                            if (row >= rodCntHalf && row <= (rodCntHalf + defectRodCntMMI - 1)) continue;
                        }
                        else if (col >= (rodCntAll - latticeCntSingleModeWg - rodCntInputWg1_add) && col < (rodCntAll - latticeCntSingleModeWg + rodCntHalf - 1))
                        {
                            if (row == rodCntHalf || row == (rodCntHalf + defectRodCntMMI - 1)) continue;
                        }
                        else if (col >= (rodCntAll - latticeCntSingleModeWg + rodCntHalf - 1))
                        {
                            int workcol = col - (rodCntAll - latticeCntSingleModeWg + rodCntHalf - 1);
                            if (workcol == 0 && (row == (rodCntHalf - 1) || row == rodCntHalf)) continue;
                            if (workcol == 0 && (row == ((rodCntHalf + defectRodCntMMI - 1) + 1) || row == (rodCntHalf + defectRodCntMMI - 1))) continue;
                            if (workcol == 1 && (row >= 0 && row <= (rodCntHalf - 1))) continue;
                            if (workcol == 1 && (row >= (rodCntHalf + defectRodCntMMI))) continue;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        double x0 = inputWgLength + latticeA * 0.5 + col * latticeA;
                        double y0 = WaveguideWidth - row * latticeA - latticeA * 0.5;
                        uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                        rodLoopIds.Add(lId);
                    }
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
                for (int i = 0; i < 4; i++)
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

            // 3ポート情報リスト作成
            //const uint portCnt = 3;
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgPortInfoList.Add(new WgUtilForPeriodicEigenExt.WgPortInfo());
                System.Diagnostics.Debug.Assert(WgPortInfoList.Count == (portIndex + 1));
                WgPortInfoList[portIndex].LatticeA = latticeA;
                WgPortInfoList[portIndex].PeriodicDistance = periodicDistance;
                WgPortInfoList[portIndex].MinEffN = minEffN;
                WgPortInfoList[portIndex].MaxEffN = maxEffN;
                // 伝搬モード数 = 欠陥ロッド数 = 1
                WgPortInfoList[portIndex].PropModeCntToSolve = defectRodCnt;
            }
            // 入射ポートの設定
            //   ポート1を入射ポートとする
            WgPortInfoList[0].IsIncidentPort = true;
            // ポート毎にモードは同じ
            IsInoutWgSame = true;

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            {
                uint[] eId_cad_list = { 2, 3, 4, 5, 7, 8, 9, 11, 12, 13 };
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
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                wgPortInfo1.FieldPortBcId = 0;

                uint[] eId_cad_list = new uint[ndivSingleModeWg];
                int[] mediaIndex_list = new int[eId_cad_list.Length];

                if (portIndex == 0)
                {
                    eId_cad_list[0] = 1;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else if (portIndex == 1)
                {
                    eId_cad_list[0] = 6;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else if (portIndex == 2)
                {
                    eId_cad_list[0] = 10;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                for (int i = 1; i <= ndivSingleModeWg - 1; i++)
                {
                    if (portIndex == 0)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) * 3 - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) * 5 - (i - 1));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
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

                uint[] eId_cad_list = new uint[ndivSingleModeWg];
                int[] mediaIndex_list = new int[eId_cad_list.Length];

                if (portIndex == 0)
                {
                    eId_cad_list[0] = 14;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else if (portIndex == 1)
                {
                    eId_cad_list[0] = 15;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else if (portIndex == 2)
                {
                    eId_cad_list[0] = 16;
                    mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                for (int i = 1; i <= ndivSingleModeWg - 1; i++)
                {
                    if (portIndex == 0)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) * 2 - (i - 1));
                    }
                    else if (portIndex == 1)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) * 4 - (i - 1));
                    }
                    else if (portIndex == 2)
                    {
                        eId_cad_list[i] = (uint)(16 + (ndivSingleModeWg - 1) * 6 - (i - 1));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
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
                        if ((portIndex == 0 &&
                                (coord[1] >= (WaveguideWidth - latticeA * (rodCntHalf + defectRodCnt)) && coord[1] <= (WaveguideWidth - latticeA * (rodCntHalf))))
                            || (portIndex == 1 &&
                                    (coord[0] >= ((inputWgLength + disconLength + singleModeWgWidth) - latticeA * (rodCntHalf + defectRodCnt)) && coord[0] <= ((inputWgLength + disconLength + singleModeWgWidth) - latticeA * (rodCntHalf))))
                            || (portIndex == 2 && (coord[0] >= ((inputWgLength + disconLength + singleModeWgWidth) - latticeA * (rodCntHalf + defectRodCnt)) && coord[0] <= ((inputWgLength + disconLength + singleModeWgWidth) - latticeA * (rodCntHalf))))
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
