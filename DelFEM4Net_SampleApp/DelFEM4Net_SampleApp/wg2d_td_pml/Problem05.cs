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

namespace wg2d_td_pml
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
        /// フォトニック結晶導波路 空洞共振器
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="timeLoopCnt"></param>
        /// <param name="timeDelta"></param>
        /// <param name="gaussianT0"></param>
        /// <param name="gaussianTp"></param>
        /// <param name="NormalizedFreqSrc"></param>
        /// <param name="NormalizedFreq1"></param>
        /// <param name="NormalizedFreq2"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPmlLoopIdList"></param>
        /// <param name="PmlStPosXList"></param>
        /// <param name="FieldPortSrcBcId"></param>
        /// <param name="VIdRefList"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="isCadShow"></param>
        /// <param name="CadDrawerAry"></param>
        /// <param name="Camera"></param>
        /// <returns></returns>
        public static bool SetProblem(
            int probNo,
            ref double WaveguideWidth,
            ref bool isPCWaveguide,
            ref double latticeA,
            ref int timeLoopCnt,
            ref double timeDelta,
            ref double gaussianT0,
            ref double gaussianTp,
            ref double NormalizedFreqSrc,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double GraphFreqInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
             ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPmlLoopIdList,
            ref IList<bool> IsPmlYDirectionList,
            ref IList<double> PmlStPosXList,
            ref IList<double> PmlLengthList,
            ref uint FieldPortSrcBcId,
            ref IList<uint> VIdRefList,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            WaveguideWidth = 1.0e-3 * 240;
            isPCWaveguide = true;
            latticeA = 0.0;

            // フォトニック結晶導波路 直線
            // メッシュの分割長さ
            //double meshL = WaveguideWidth * 0.05;
            double meshL = WaveguideWidth * 0.02;
            //double meshL = WaveguideWidth * 0.03;

            /////////////////////////////////////////////
            // フォトニック導波路
            // ロッドの数（半分）
            const int rodCntHalf = 5;
            //const int rodCntHalf = 3;
            // 欠陥ロッド数
            const int defectRodCnt = 1;
            // 格子数
            const int latticeCnt = rodCntHalf * 2 + defectRodCnt;
            // 格子定数
            latticeA = WaveguideWidth / (double)latticeCnt;
            // 周期構造距離
            double periodicDistance = latticeA;
            // ロッドの半径
            double rodRadius = 0.18 * latticeA;
            // ロッドの比誘電率
            double rodEps = 3.4 * 3.4;
            // ロッドの円周分割数
            const int rodCircleDiv = 8;
            // ロッドの半径の分割数
            const int rodRadiusDiv = 1;
            /////////////////////////////////////////////

            // PMLの長さ
            //double pmlLength = 5 * periodicDistance;
            //double pmlLength = 10 * periodicDistance;
            double pmlLength = 6 * periodicDistance;
            // 導波管不連続領域の長さ
            double disconLength = 15 * periodicDistance;
            disconLength += 2.0 * pmlLength;

            // 励振位置
            //double srcPosX = pmlLength + 4.5 * periodicDistance;
            double srcPosX = pmlLength + 1.5 * periodicDistance;
            // 観測点
            double port1PosX = srcPosX + 1.5 * periodicDistance;
            double port1PosY = WaveguideWidth * 0.5;
            double port2PosX = disconLength - pmlLength - 1.0 * periodicDistance;
            double port2PosY = WaveguideWidth * 0.5;

            // 時間領域
            //double courantNumber = 0.5;
            double courantNumber = 0.5;
            //timeLoopCnt = 100000;
            timeLoopCnt = 3000;
            // 時刻刻み
            timeDelta = courantNumber * (WaveguideWidth * 0.02) / (c0 * Math.Sqrt(2.0));

            // モード計算規格化周波数(搬送波規格化周波数)
            // フォトニック結晶導波路の場合、a/λを規格化周波数とする
            //NormalizedFreqSrc = 0.400;
            NormalizedFreqSrc = 0.400;
            NormalizedFreqSrc *= (2.0 * WaveguideWidth) / latticeA;
            Console.WriteLine("NormalizedFreqSrc: {0}", NormalizedFreqSrc);

            // 周波数領域
            NormalizedFreq1 = 0.300;//0.302;
            NormalizedFreq2 = 0.440;//0.443;
            GraphFreqInterval = 0.02;
            NormalizedFreq1 *= (2.0 * WaveguideWidth) / latticeA;
            NormalizedFreq2 *= (2.0 * WaveguideWidth) / latticeA;
            GraphFreqInterval *= (2.0 * WaveguideWidth) / latticeA;

            /*
            // ガウシアンパルス
            //gaussianT0 = 30 * timeDelta;
            //gaussianT0 = 40 * timeDelta;
            gaussianT0 = 20 * timeDelta;
            gaussianTp = gaussianT0 / (Math.Sqrt(2.0) * 4.0);
             */

            
            // 正弦波変調ガウシアンパルス
            // 波数
            double k0Src = NormalizedFreqSrc * pi / WaveguideWidth;
            // 波長
            double waveLengthSrc = 2.0 * pi / k0Src;
            // 周波数
            double freqSrc = c0 / waveLengthSrc;
            // 角周波数
            double omegaSrc = 2.0 * pi * freqSrc;
            // 搬送波のバンド幅
            double k0BandWidthSrc = (NormalizedFreq2 - NormalizedFreq1) *pi / WaveguideWidth; ;
            double waveLengthBandWidthSrc = 2.0 * pi / k0BandWidthSrc;
            double freqBandWidthSrc = c0 / waveLengthBandWidthSrc;
            gaussianT0 = (1.0 / freqBandWidthSrc);
            gaussianTp = gaussianT0 / (Math.Sqrt(2.0));

            // ポート数
            const int portCnt = 2;

            // 媒質リスト作成
            double claddingP = 1.0;
            double claddingQ = 1.0;
            double coreP = 1.0;
            double coreQ = 1.0;

            // 媒質リスト作成
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
            // 全ループ数
            uint loopCnt_cad = 2 + portCnt;
            // ポート1のPML領域ループリスト
            uint[] port1_pml_loopIds_cad = { 1 };
            // ポート2のPML領域ループリスト
            uint[] port2_pml_loopIds_cad = { 4 };
            // PML開始位置X座標
            double[] pmlStPosX_list = { pmlLength, (disconLength - pmlLength) };
            // 観測点頂点ID
            uint[] portRef_vIds_cad = new uint[portCnt];
            // 励振面の辺ID
            uint[] eId_cad_src_list = new uint[1 + 3 * (2 * rodCntHalf)];
            uint[] core_eId_cad_src_list = new uint[2 * (2 * rodCntHalf)];
            eId_cad_src_list[0] = 12;
            for (int i = 0; i < 3 * (2 * rodCntHalf); i++)
            {
                eId_cad_src_list[1 + i] = (uint)(14 + (3 * (2 * rodCntHalf) - 1) - i);
            }
            for (int i = 0; i < (2 * rodCntHalf); i++)
            {
                uint eId1 = eId_cad_src_list[1 + i * 3];
                uint eId2 = eId_cad_src_list[1 + i * 3 + 1];
                uint eId3 = eId_cad_src_list[1 + i * 3 + 2];
                core_eId_cad_src_list[i * 2] = eId1;
                core_eId_cad_src_list[i * 2 + 1] = eId2;
            }
            // ロッドのループIDリスト
            IList<uint> rodLoopIds = new List<uint>();
            // ロッドのループIDリスト(PML領域)
            IList<uint> rodLoopIds_port1_pml = new List<uint>();
            IList<uint> rodLoopIds_port2_pml = new List<uint>();
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                IList<CVector2D> pts = new List<CVector2D>();
                pts.Add(new CVector2D(0.0, WaveguideWidth));  // 頂点1
                pts.Add(new CVector2D(0.0, 0.0)); // 頂点2
                pts.Add(new CVector2D(pmlLength, 0.0)); // 頂点3
                pts.Add(new CVector2D(srcPosX, 0.0)); // 頂点4
                pts.Add(new CVector2D(disconLength - pmlLength, 0.0)); // 頂点5
                pts.Add(new CVector2D(disconLength, 0.0)); // 頂点6
                pts.Add(new CVector2D(disconLength, WaveguideWidth)); // 頂点7
                pts.Add(new CVector2D(disconLength - pmlLength, WaveguideWidth)); // 頂点8
                pts.Add(new CVector2D(srcPosX, WaveguideWidth)); // 頂点9
                pts.Add(new CVector2D(pmlLength, WaveguideWidth)); // 頂点10
                uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                uint lId2 = cad2d.ConnectVertex_Line(3, 10).id_l_add;
                uint lId3 = cad2d.ConnectVertex_Line(4, 9).id_l_add;
                uint lId4 = cad2d.ConnectVertex_Line(5, 8).id_l_add;
                // 観測点
                portRef_vIds_cad[0] = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, lId3, new CVector2D(port1PosX, port1PosY)).id_v_add; // 頂点11
                portRef_vIds_cad[1] = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, lId3, new CVector2D(port2PosX, port2PosY)).id_v_add; // 頂点12
                // check
                foreach (uint vId in portRef_vIds_cad)
                {
                    System.Diagnostics.Debug.Assert(vId != 0);
                }

                {
                    double workX = srcPosX;
                    IList<uint> work_vIds = new List<uint>();
                    IList<uint> work_vIds_center = new List<uint>();
                    for (int row = (rodCntHalf * 2 + defectRodCnt - 1); row >= 0; row--)
                    {
                        if (row >= (rodCntHalf) && row < (rodCntHalf + defectRodCnt))
                        {
                            // 欠陥部
                            continue;
                        }

                        uint parent_eId = eId_cad_src_list[0]; // 先頭の辺のID
                        double workY1 = row * latticeA + 0.5 * latticeA + rodRadius;
                        double workY2 = row * latticeA + 0.5 * latticeA - rodRadius;
                        uint vId1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(workX, workY1)).id_v_add;
                        uint vId_center = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(workX, (workY1 + workY2) * 0.5)).id_v_add;
                        uint vId2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(workX, workY2)).id_v_add;
                        System.Diagnostics.Debug.Assert(vId1 != 0);
                        System.Diagnostics.Debug.Assert(vId2 != 0);

                        work_vIds.Add(vId1);
                        work_vIds_center.Add(vId_center);
                        work_vIds.Add(vId2);
                    }
                    uint[] work_vIdAry = work_vIds.ToArray();
                    uint[] work_vIdAry_center = work_vIds_center.ToArray();
                    // 境界の右側半円
                    for (int row = (rodCntHalf * 2 + defectRodCnt - 1), vIndex = 0; row >= 0; row--)
                    {
                        if (row >= (rodCntHalf) && row < (rodCntHalf + defectRodCnt))
                        {
                            // 欠陥部
                            continue;
                        }
                        uint parent_lId = 3;
                        double workY1 = row * latticeA + 0.5 * latticeA - rodRadius * 0.5;
                        double workY2 = row * latticeA + 0.5 * latticeA + rodRadius * 0.5;
                        uint vId1 = work_vIdAry[vIndex * 2 + 1];
                        uint vId2 = work_vIdAry[vIndex * 2];
                        uint vId_center = work_vIds_center[vIndex];
                        uint lId = WgCadUtil.AddLeftRod(cad2d, parent_lId, vId1, vId_center, vId2, workX, (workY1 + workY2) * 0.5, rodRadius, rodCircleDiv, 1);
                        rodLoopIds.Add(lId);
                        vIndex++;
                    }
                    // 境界の左側半円
                    for (int row = (rodCntHalf * 2 + defectRodCnt - 1), vIndex = 0; row >= 0; row--)
                    {
                        if (row >= (rodCntHalf) && row < (rodCntHalf + defectRodCnt))
                        {
                            // 欠陥部
                            continue;
                        }
                        uint parent_lId = 2;
                        double workY1 = row * latticeA + 0.5 * latticeA - rodRadius * 0.5;
                        double workY2 = row * latticeA + 0.5 * latticeA + rodRadius * 0.5;
                        uint vId1 = work_vIdAry[vIndex * 2];
                        uint vId2 = work_vIdAry[vIndex * 2 + 1];
                        uint vId_center = work_vIds_center[vIndex];
                        uint lId = WgCadUtil.AddRightRod(cad2d, parent_lId, vId1, vId_center, vId2, workX, (workY1 + workY2) * 0.5, rodRadius, rodCircleDiv, 1);
                        rodLoopIds.Add(lId);
                        vIndex++;
                    }
                }

                //////////////////////////////////////////////////////////
                // ロッド追加
                int colCntX = (int)Math.Round(disconLength / periodicDistance);
                int colIndexCavity = (int)Math.Round((srcPosX + (disconLength - srcPosX - pmlLength) * 0.5) / periodicDistance);
                int colCntXCavityPort = 2;
                for (int col = 0; col < colCntX; col++)
                {
                    uint baseLoopId = 0;
                    double x1 = col * periodicDistance;
                    double x2 = (col + 1) * periodicDistance;
                    if (x1 < srcPosX && x2 > srcPosX)
                    {
                        continue;
                    }

                    for (int row = 0; row < (rodCntHalf * 2 + defectRodCnt); row++)
                    {
                        int pmlWgNo = 0;
                        if (x1 < (pmlLength - Constants.PrecisionLowerLimit))
                        {
                            pmlWgNo = 1;
                            baseLoopId = 1;
                        }
                        else if (x1 >= (pmlLength - Constants.PrecisionLowerLimit) && x1 < (srcPosX - Constants.PrecisionLowerLimit))
                        {
                            baseLoopId = 2;
                        }
                        else if (x1 >= (srcPosX - Constants.PrecisionLowerLimit) && x1 < (disconLength - pmlLength - Constants.PrecisionLowerLimit))
                        {
                            baseLoopId = 3;
                        }
                        else
                        {
                            pmlWgNo = 2;
                            baseLoopId = 4;
                        }
                        if ((col >= (colIndexCavity - colCntXCavityPort) && col <= (colIndexCavity - 1))
                            || (col >= (colIndexCavity + 1) && col <= (colIndexCavity + colCntXCavityPort)))
                        {
                            // 空洞共振器の入出力ロッド部
                        }
                        else
                        {
                            if (row >= (rodCntHalf) && row < (rodCntHalf + defectRodCnt)) continue; // 欠陥部
                        }
                        double x0 = col * periodicDistance + periodicDistance * 0.5;
                        double y0 = WaveguideWidth - row * latticeA - latticeA * 0.5;
                        uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
                        rodLoopIds.Add(lId);
                        if (pmlWgNo == 1)
                        {
                            rodLoopIds_port1_pml.Add(lId);
                        }
                        else if (pmlWgNo == 2)
                        {
                            rodLoopIds_port2_pml.Add(lId);
                        }
                    }
                }

                //isCadShow = true;
                // 図面表示
                if (isCadShow)
                {
                    // PML領域に色を付ける
                    uint[][] port_pml_loopIds_cad_list = { port1_pml_loopIds_cad, port2_pml_loopIds_cad };
                    for (int portIndex = 0; portIndex < portCnt; portIndex++)
                    {
                        uint[] work_lId_list = port_pml_loopIds_cad_list[portIndex];
                        foreach (uint lId in work_lId_list)
                        {
                            cad2d.SetColor_Loop(lId, new double[] { 0.5, 0.5, 0.5 });
                        }
                    }
                    // 誘電体ロッドに色を付ける
                    {
                        uint[] work_lId_list = rodLoopIds.ToArray();
                        foreach (uint lId in work_lId_list)
                        {
                            cad2d.SetColor_Loop(lId, new double[] { 1.0, 0.5, 0.0 });
                        }
                    }
                    // 境界の辺に色を付ける
                    {
                        uint[] work_eId_list = eId_cad_src_list;
                        foreach (uint eId in work_eId_list)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 0.8, 0.0, 0.0 });
                        }
                    }
                    // 境界の誘電体スラブの辺
                    {
                        uint[] work_port_core_eIds = core_eId_cad_src_list;
                        foreach (uint eId in work_port_core_eIds)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 0.0, 0.0, 1.0 });
                        }
                    }

                    /*
                    // DEBUG
                    {
                        uint[] work_eId_list = { 2, 3, 4, 5, 7, 8, 9, 10 };
                        foreach (uint eId in work_eId_list)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 1.0, 1.0, 1.0 });
                        }
                    }
                     */


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
                // ワールド座標系のループIDを取得
                uint[] loopId_cad_list = new uint[loopCnt_cad + rodLoopIds.Count];
                for (int i = 0; i < loopCnt_cad; i++)
                {
                    loopId_cad_list[i] = (uint)(i + 1);
                }
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    loopId_cad_list[i + loopCnt_cad] = rodLoopIds[i];
                }
                int[] mediaIndex_list = new int[loopCnt_cad + rodLoopIds.Count];
                for (int i = 0; i < loopCnt_cad; i++)
                {
                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
                }
                for (int i = 0; i < rodLoopIds.Count; i++)
                {
                    mediaIndex_list[i + loopCnt_cad] = Medias.IndexOf(mediaCore);
                }

                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                for (int i = 0; i < loopId_cad_list.Length; i++)
                {
                    uint loopId_cad = loopId_cad_list[i];
                    int mediaIndex = mediaIndex_list[i];
                    uint lId1 = conv.GetIdEA_fromCad(loopId_cad, CAD_ELEM_TYPE.LOOP);
                    aEA.Add(lId1);
                    {
                        wg2d.World.Loop loop = new wg2d.World.Loop();
                        loop.Set(lId1, mediaIndex);
                        LoopDic.Add(lId1, loop);
                    }
                }
                //System.Diagnostics.Debug.WriteLine("lId:" + lId1);
                FieldLoopId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldLoopId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0);
            }

            // PML領域
            FieldPmlLoopIdList.Clear();
            IsPmlYDirectionList.Clear();
            PmlStPosXList.Clear();
            PmlLengthList.Clear();
            uint[][] loopId_cad_list_pml = new uint[portCnt][];
            // ポート1PML
            loopId_cad_list_pml[0] = new uint[port1_pml_loopIds_cad.Length + rodLoopIds_port1_pml.Count];
            port1_pml_loopIds_cad.CopyTo(loopId_cad_list_pml[0], 0);
            rodLoopIds_port1_pml.CopyTo(loopId_cad_list_pml[0], port1_pml_loopIds_cad.Length);
            // ポート2 PML
            loopId_cad_list_pml[1] = new uint[port2_pml_loopIds_cad.Length + rodLoopIds_port2_pml.Count];
            port2_pml_loopIds_cad.CopyTo(loopId_cad_list_pml[1], 0);
            rodLoopIds_port2_pml.CopyTo(loopId_cad_list_pml[1], port2_pml_loopIds_cad.Length);
            for (int portIndex = 0; portIndex < portCnt; portIndex++)
            {
                uint[] loopId_cad_list = loopId_cad_list_pml[portIndex];
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                foreach (uint loopId_cad in loopId_cad_list)
                {
                    uint lId1 = conv.GetIdEA_fromCad(loopId_cad, CAD_ELEM_TYPE.LOOP);
                    aEA.Add(lId1);
                }
                uint workFieldLoopId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(workFieldLoopId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0);
                FieldPmlLoopIdList.Add(workFieldLoopId);
                // Y方向PML?
                bool isPmlYDirection = false;
                IsPmlYDirectionList.Add(isPmlYDirection);
                // PML開始位置
                double pmlStPosX = pmlStPosX_list[portIndex];
                PmlStPosXList.Add(pmlStPosX);
                // PML長さ
                PmlLengthList.Add(pmlLength);
            }

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            {
                uint[] eId_cad_list = { 2, 3, 4, 5, 7, 8, 9, 10 }; // PML終端磁気壁
                //uint[] eId_cad_list = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // PML終端電気壁
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                foreach (uint eId_cad in eId_cad_list)
                {
                    uint eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                    aEA.Add(eId);
                }
                // フィールドIDを取得
                FieldForceBcId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(FieldForceBcId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
            }
            // 開口条件
            //   ワールド座標系の辺IDを取得
            //   辺単位で媒質を指定する
            FieldPortSrcBcId = 0;
            {
                // PMLを用いる場合は、励振面のみ
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                foreach (uint eId_cad in eId_cad_src_list)
                {
                    uint eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                    aEA.Add(eId);
                    {
                        wg2d.World.Edge edge = new wg2d.World.Edge();
                        int mediaIndex = Medias.IndexOf(mediaCladding);
                        if (core_eId_cad_src_list.Contains(eId_cad))
                        {
                            mediaIndex = Medias.IndexOf(mediaCore);
                        }
                        edge.Set(eId, mediaIndex);
                        EdgeDic.Add(eId, edge);
                    }
                }
                uint workFieldPortBcId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(workFieldPortBcId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
                FieldPortSrcBcId = workFieldPortBcId;
            }

            // 観測点
            VIdRefList.Clear();
            uint[] vId_cad_refPort = portRef_vIds_cad;
            foreach (uint vId_cad in vId_cad_refPort)
            {
                uint vId = conv.GetIdEA_fromCad(vId_cad, CAD_ELEM_TYPE.VERTEX);
                VIdRefList.Add(vId);
            }

            return true;
        }
    }
}
