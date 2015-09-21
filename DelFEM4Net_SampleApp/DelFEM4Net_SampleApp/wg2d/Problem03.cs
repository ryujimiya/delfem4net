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

namespace wg2d
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
        /// 誘電体スラブ導波路グレーティング
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
            ref double MinSParameter,
            ref double MaxSParameter,
            ref double GraphSParameterInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
             ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            // メッシュの分割長さ
            double meshL = WaveguideWidth * 1.0 / 60.0;
            //double meshL = WaveguideWidth * 1.0 / 69.0;

            // 導波管不連続領域の長さ
            //double disconLength = 65.0 / 60.0 * WaveguideWidth;
            double disconLength = 40.0 / 60.0 * WaveguideWidth;

            // 誘電体スラブ導波路幅
            double slabWidth =WaveguideWidth / 30.0;
            // 誘電体スラブ比誘電率
            double coreEps = 2.402500;
            // グレーティング比誘電率
            double gratingEps = 2.102500;
            // グレーティングの数
            int gratingCnt = 8;  // 偶数とする
            // グレーティング１つの長さ
            double gratingOneLength = slabWidth;
            // グレーティングの全長
            double gratingAllLength = gratingOneLength * ((gratingCnt - 1) * 2 + 1);
            // グレーティング開始位置
            //double grating_X1 = 25.0 / 60.0 * WaveguideWidth;
            double grating_X1 = 5.0 / 60.0 * WaveguideWidth;
            double grating_X2 = grating_X1 + gratingAllLength;

            NormalizedFreq1 = 10.0;
            NormalizedFreq2 = 20.0;
            FreqDelta = 0.25;
            GraphFreqInterval = 2.0;

            MinSParameter = 0.0;
            MaxSParameter = 1.0;
            GraphSParameterInterval = 0.2;

            // ポート数
            const int portCnt = 2;

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
            MediaInfo mediaCore = new MediaInfo
            (
                new double[3, 3]
                {
                   { 1.0/1.0, 0.0,     0.0     },
                   { 0.0,     1.0/1.0, 0.0     },
                   { 0.0,     0.0,     1.0/1.0 }
                },
                new double[3, 3]
                {
                   { coreEps,  0.0,  0.0 },
                   {  0.0, coreEps,  0.0 },
                   {  0.0,  0.0, coreEps }
                }
            );
            MediaInfo mediaGrating = new MediaInfo
            (
                new double[3, 3]
                {
                   { 1.0/1.0, 0.0,     0.0     },
                   { 0.0,     1.0/1.0, 0.0     },
                   { 0.0,     0.0,     1.0/1.0 }
                },
                new double[3, 3]
                {
                   { gratingEps,  0.0,  0.0 },
                   {  0.0, gratingEps,  0.0 },
                   {  0.0,  0.0, gratingEps }
                }
            );
            Medias.Add(mediaVacumn);
            Medias.Add(mediaCore);
            Medias.Add(mediaGrating);

            // 図面作成、メッシュ生成
            double coreY1 = (WaveguideWidth - slabWidth) * 0.5;
            double coreY2 = coreY1 + slabWidth;
            uint[] all_loopId_cad_list = new uint[3 + (2 * (gratingCnt - 1) + 1) + 1];
            for (int i = 0; i < all_loopId_cad_list.Length; i++)
            {
                all_loopId_cad_list[i] = (uint)(i + 1);
            }
            uint[] slab_loopId_cad_list = new uint[1 + (gratingCnt - 1) + 1];
            slab_loopId_cad_list[0] = 3;
            for (int i = 0; i < gratingCnt; i++)
            {
                slab_loopId_cad_list[1 + i] = (uint)(5 + i * 2);
            }
            uint[] grating_loopId_cad_list = new uint[gratingCnt];
            for (int i = 0; i < gratingCnt; i++)
            {
                grating_loopId_cad_list[i] = (uint)(4 + i * 2);
            }

            uint[] port1_eId_list = { 1, 6, 5 };
            uint port1_core_eId = 6;
            uint[] port2_eId_list = { 3, 8, 7 };
            uint port2_core_eId = 8;
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                {
                    // ループ1
                    IList<CVector2D> pts = new List<CVector2D>();
                    pts.Add(new CVector2D(0.0, WaveguideWidth));  // 頂点1
                    pts.Add(new CVector2D(0.0, 0.0)); // 頂点2
                    pts.Add(new CVector2D(disconLength, 0.0)); // 頂点3
                    pts.Add(new CVector2D(disconLength, WaveguideWidth)); // 頂点4
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                }

                // スラブ導波路と境界の交点
                uint[] parent_eId_list = {1, 3};
                double[] portX_list = {0.0, disconLength};
                IList<uint[]> slab_vIds_list = new List<uint[]>();
                for (int portIndex = 0; portIndex < (portCnt); portIndex++)
                {
                    uint parent_eId = parent_eId_list[portIndex];
                    double portX = portX_list[portIndex];
                    
                    double workY1 = 0.0;
                    double workY2 = 0.0;
                    if (portIndex == 0)
                    {
                        workY1 = coreY1;
                        workY2 = coreY2;
                    }
                    else
                    {
                        workY1 = coreY2;
                        workY2 = coreY1;
                    }
                    uint vId1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(portX, workY1)).id_v_add;
                    uint vId2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(portX, workY2)).id_v_add;
                    uint[] work_vIds = new uint[2];
                    if (portIndex == 0)
                    {
                        work_vIds[0] = vId1;
                        work_vIds[1] = vId2;
                    }
                    else
                    {
                        work_vIds[0] = vId2;
                        work_vIds[1] = vId1;
                    }
                    slab_vIds_list.Add(work_vIds);
                }
                // スラブ導波路
                {
                    {
                        uint work_vId1 = slab_vIds_list[0][0];
                        uint work_vId2 = slab_vIds_list[1][0];
                        uint work_eId = cad2d.ConnectVertex_Line(work_vId1, work_vId2).id_e_add;
                    }
                    {
                        uint work_vId1 = slab_vIds_list[0][1];
                        uint work_vId2 = slab_vIds_list[1][1];
                        uint work_eId = cad2d.ConnectVertex_Line(work_vId1, work_vId2).id_e_add;
                    }
                }
                // グレーティング
                {
                    // 誘電体スラブの上下の辺
                    uint[] slabTopBottom_eIds = {9, 10};
                    double[] work_Ys = {coreY1, coreY2};
                    uint[][] work_vIds_list = new uint[2][];
                    // 上下の辺に頂点を追加
                    for (int edgeIndex = 0; edgeIndex < 2; edgeIndex++) // スラブの上下の辺
                    {
                        uint parent_eId = slabTopBottom_eIds[edgeIndex];
                        double workY = work_Ys[edgeIndex];
                        work_vIds_list[edgeIndex] = new uint[2 * gratingCnt];
                        for (int i = (2 * gratingCnt) - 1; i >= 0; i--)
                        {
                            double workX = grating_X1 + gratingOneLength * i;
                            uint vId = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(workX, workY)).id_v_add;
                            work_vIds_list[edgeIndex][i] = vId;
                        }
                    }
                    // 上下の頂点を結ぶ
                    uint[] grating_eIds = new uint[(2 * gratingCnt)];
                    uint[] grating_lIds = new uint[(2 * gratingCnt)];
                    for (int i = 0; i < (2 * gratingCnt); i++)
                    {
                        uint work_vId1 = work_vIds_list[0][i];
                        uint work_vId2 = work_vIds_list[1][i];
                        CBRepSurface.CResConnectVertex res =cad2d.ConnectVertex_Line(work_vId1, work_vId2);
                        grating_eIds[i] = res.id_e_add;
                        grating_lIds[i] = res.id_l_add;
                    }
                    /*
                    // 分割数調整
                    for (int i = 0; i < (2 * gratingCnt); i++)
                    {
                        double workY = (coreY1 + coreY2) * 0.5;
                        double workX = grating_X1 + gratingOneLength * i;
                        uint parent_eId = grating_eIds[i];
                        uint vId = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, parent_eId, new CVector2D(workX, workY)).id_v_add;
                        System.Diagnostics.Debug.Assert(vId != 0);
                    }
                    for (int i = 0; i < (2 * gratingCnt - 1); i++)
                    {
                        double workY = (coreY1 + coreY2) * 0.5;
                        double workX = grating_X1 + gratingOneLength * i + gratingOneLength * 0.5;
                        uint parent_lId = grating_lIds[i];
                        uint vId = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, parent_lId, new CVector2D(workX, workY)).id_v_add;
                        System.Diagnostics.Debug.Assert(vId != 0);
                    }
                     */
                }
                
                
                //isCadShow = true;
                // 図面表示
                if (isCadShow)
                {
                    // 誘電体スラブ導波路に色を付ける
                    {
                        uint[] work_lId_list = slab_loopId_cad_list;
                        foreach (uint lId in work_lId_list)
                        {
                            cad2d.SetColor_Loop(lId, new double[] { 1.0, 0.0, 1.0 });
                        }
                    }
                    {
                        uint[] work_lId_list = grating_loopId_cad_list;
                        foreach (uint lId in work_lId_list)
                        {
                            cad2d.SetColor_Loop(lId, new double[] { 0.0, 0.0, 1.0 });
                        }
                    }
                    // 境界の辺に色を付ける
                    {
                        uint[] work_eId_list = port1_eId_list;
                        foreach (uint eId in work_eId_list)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 0.8, 0.0, 0.0 });
                        }
                    }
                    {
                        uint[] work_eId_list = port2_eId_list;
                        foreach (uint eId in work_eId_list)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 1.0, 1.0, 0.0 });
                        }
                    }
                    // 境界の誘電体スラブの辺
                    cad2d.SetColor_Edge(port1_core_eId, new double[] { 1.0, 0.0, 0.0 });
                    cad2d.SetColor_Edge(port2_core_eId, new double[] { 1.0, 0.0, 0.0 });

                    // DEBUG
                    // 右側領域誘電体スラブ上下の辺
                    cad2d.SetColor_Edge(9, new double[] { 0.0, 0.0, 1.0 });
                    cad2d.SetColor_Edge(10, new double[] { 0.0, 0.0, 1.0 });

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
                uint[] loopId_cad_list  = all_loopId_cad_list;
                int[] mediaIndex_list = new int[loopId_cad_list.Length];
                for (int i = 0; i < loopId_cad_list.Length; i++)
                {
                    int mediaIndex = Medias.IndexOf(mediaVacumn);
                    if (slab_loopId_cad_list.Contains(loopId_cad_list[i]))
                    {
                        mediaIndex = Medias.IndexOf(mediaCore);
                    }
                    else if (grating_loopId_cad_list.Contains(loopId_cad_list[i]))
                    {
                        mediaIndex = Medias.IndexOf(mediaGrating);
                    }
                    mediaIndex_list[i] = mediaIndex;
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

            // 境界条件を設定する
            //   固定境界条件（強制境界)
            //   ワールド座標系の辺IDを取得
            //   媒質は指定しない
            FieldForceBcId = 0;
            {
                uint[] eId_cad_list = {2, 4};
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
            FieldPortBcId1 = 0;
            FieldPortBcId2 = 0;
            uint[][] eId_cad_port_list = { port1_eId_list, port2_eId_list };
            uint[] core_eId_cad_list = { port1_core_eId, port2_core_eId };
            for (int portIndex = 0; portIndex < (portCnt); portIndex++) // ポート + 励振境界
            {
                uint[] eId_cad_port = eId_cad_port_list[portIndex];
                uint core_eId = core_eId_cad_list[portIndex];
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                for (int i = 0; i < eId_cad_port.Length; i++)
                {
                    uint eId_cad = eId_cad_port[i];
                    int mediaIndex = Medias.IndexOf(mediaVacumn);
                    if (eId_cad == core_eId)
                    {
                        mediaIndex = Medias.IndexOf(mediaCore);
                    }
                    else
                    {
                        mediaIndex = Medias.IndexOf(mediaVacumn);
                    }
                    uint eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                    aEA.Add(eId);
                    {
                        wg2d.World.Edge edge = new wg2d.World.Edge();
                        edge.Set(eId, mediaIndex);
                        EdgeDic.Add(eId, edge);
                    }
                }
                uint workFieldPortBcId = World.GetPartialField(FieldValId, aEA);
                CFieldValueSetter.SetFieldValue_Constant(workFieldPortBcId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
                if (portIndex == 0)
                {
                    FieldPortBcId1 = workFieldPortBcId;
                }
                else if (portIndex == 1)
                {
                    FieldPortBcId2 = workFieldPortBcId;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }

            return true;
        }
    }
}
