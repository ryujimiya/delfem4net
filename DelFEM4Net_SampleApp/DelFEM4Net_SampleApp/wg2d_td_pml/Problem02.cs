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
        /// 誘電体のボックス装荷導波管
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
            WaveguideWidth = 3.0e-3 * 20 * 4;

            // 誘電体のボックス装荷導波管
            // メッシュの分割長さ
            //double meshL = WaveguideWidth * 0.05;
            //double meshL = WaveguideWidth * 0.10;
            //double meshL = WaveguideWidth * 0.09;
            double meshL = WaveguideWidth * 0.04;

            // 導波管不連続領域の長さ
            double inputWgLength = 1.0 * WaveguideWidth;
            double disconWgWidth = (10.0 / 20.0) * WaveguideWidth;
            double dielectricBoxWidth = (7.0 / 20.0) * WaveguideWidth;
            double dielectricBoxHeight = (6.0 / 20.0) * WaveguideWidth;
            double disconLength = (19.0 / 20.0) * WaveguideWidth;
            // 形状設定で使用する単位長さ
            double unitLen = WaveguideWidth / 20.0;
            // PMLの長さ
            //double pmlLength = 10 * unitLen;
            double pmlLength = 12 * unitLen;

            // 励振位置
            double srcPosX = pmlLength + 5 * unitLen;
            // 観測点
            int port1OfsX = 5;
            int port2OfsX = 5;
            double port1PosX = srcPosX + port1OfsX * unitLen;
            double port1PosY = WaveguideWidth * 0.5;
            double port2PosX = pmlLength + inputWgLength * 2 + disconLength - port2OfsX * unitLen;
            double port2PosY = WaveguideWidth * 0.5;

            // 時間領域
            //double courantNumber = 0.5;
            double courantNumber = 0.5;
            //timeLoopCnt = 3000;
            timeLoopCnt = 3000;
            // 時刻刻み
            timeDelta = courantNumber * meshL / (c0 * Math.Sqrt(2.0));

            // モード計算規格化周波数(搬送波規格化周波数)
            //NormalizedFreqSrc = 2.0;
            //NormalizedFreqSrc = 1.5;
            NormalizedFreqSrc = 2.0; // 正弦波変調ガウシアンパルス

            /*
            // ガウシアンパルス
            //gaussianT0 = 30 * timeDelta;
            //gaussianT0 = 40 * timeDelta;
            //gaussianT0 = 20 * timeDelta;
            gaussianT0 = 45 * timeDelta;
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
            // 搬送波の振動回数
            int nCycle = 5;
            //gaussianT0 = (1.0 / freqSrc) * nCycle / 2;
            //gaussianT0 = 0.5 * (1.0 / freqSrc) * nCycle / 2;
            //gaussianT0 = 0.67 * (1.0 / freqSrc) * nCycle / 2;
            gaussianT0 = 0.65 * (1.0 / freqSrc) * nCycle / 2;
            gaussianTp = gaussianT0 / (2.0 * Math.Sqrt(2.0 * Math.Log(2.0)));
             

            // 周波数領域
            NormalizedFreq1 = 1.0;
            //NormalizedFreq2 = 2.0;
            NormalizedFreq2 = 3.0;
            GraphFreqInterval = 0.2;

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
            MediaInfo mediaDielectricBox = new MediaInfo
            (
                new double[3, 3]
                {
                   { 1.0/1.0, 0.0,     0.0     },
                   { 0.0,     1.0/1.0, 0.0     },
                   { 0.0,     0.0,     1.0/1.0 }
                },
                new double[3, 3]
                {
                   { 2.62,  0.0,  0.0 },
                   {  0.0, 2.62,  0.0 },
                   {  0.0,  0.0, 2.62 }
                }
            );
            Medias.Add(mediaVacumn);
            Medias.Add(mediaDielectricBox);

            // 図面作成、メッシュ生成
            // 不連続部
            double discon_X1 = pmlLength + inputWgLength;
            double discon_X2 = discon_X1 + disconLength;
            // ポート2
            double port2_X = pmlLength * 2 + inputWgLength * 2 + disconLength;
            // 全ループ数
            uint loopCnt_cad = 2 + portCnt;
            // ポート1のPML領域ループリスト
            uint[] port1_pml_loopIds_cad = { 1 };
            // ポート2のPML領域ループリスト
            uint[] port2_pml_loopIds_cad = { 4 };
            // 誘電体
            uint dielectricBox_loopId = 5;
            // PML開始位置X座標
            double[] pmlStPosX_list = { pmlLength, port2_X -pmlLength };
            // 観測点頂点ID
            uint[] portRef_vIds_cad = new uint[portCnt];
            // 励振面の辺ID
            uint eId_cad_src = 20;
            uint baseId = 0;
            CIDConvEAMshCad conv = null;
            using (CCadObj2D cad2d = new CCadObj2D())
            {
                //------------------------------------------------------------------
                // 図面作成
                //------------------------------------------------------------------
                double yOfs_cutoffWg = (WaveguideWidth - disconWgWidth) * 0.5;
                {
                    // ループ1
                    IList<CVector2D> pts = new List<CVector2D>();
                    pts.Add(new CVector2D(0.0, WaveguideWidth));  // 頂点1
                    pts.Add(new CVector2D(0.0, 0.0)); // 頂点2
                    pts.Add(new CVector2D(pmlLength, 0.0)); // 頂点3 
                    pts.Add(new CVector2D(srcPosX, 0.0)); // 頂点4 
                    pts.Add(new CVector2D(discon_X1, 0.0)); // 頂点5
                    pts.Add(new CVector2D(discon_X1, yOfs_cutoffWg)); // 頂点6
                    pts.Add(new CVector2D(discon_X2, yOfs_cutoffWg)); // 頂点7
                    pts.Add(new CVector2D(discon_X2, 0.0)); // 頂点8
                    pts.Add(new CVector2D(port2_X - pmlLength, 0.0)); // 頂点9
                    pts.Add(new CVector2D(port2_X, 0.0)); // 頂点10
                    pts.Add(new CVector2D(port2_X, WaveguideWidth)); // 頂点11
                    pts.Add(new CVector2D(port2_X - pmlLength, WaveguideWidth)); // 頂点12
                    pts.Add(new CVector2D(discon_X2, WaveguideWidth)); // 頂点13
                    pts.Add(new CVector2D(discon_X2, yOfs_cutoffWg + disconWgWidth)); // 頂点14
                    pts.Add(new CVector2D(discon_X1, yOfs_cutoffWg + disconWgWidth)); // 頂点15
                    pts.Add(new CVector2D(discon_X1, WaveguideWidth)); // 頂点16
                    pts.Add(new CVector2D(srcPosX, WaveguideWidth)); // 頂点17
                    pts.Add(new CVector2D(pmlLength, WaveguideWidth)); // 頂点18
                    uint lId1 = cad2d.AddPolygon(pts).id_l_add;
                    uint lId2 = cad2d.ConnectVertex_Line(3, 18).id_l_add;
                    uint lId3 = cad2d.ConnectVertex_Line(4, 17).id_l_add;
                    uint lId4 = cad2d.ConnectVertex_Line(9, 12).id_l_add;
                }
                {
                    // ループ2
                    // ループの親ID
                    uint parent_id_l_cad = 3;
                    IList<CVector2D> pts = new List<CVector2D>();
                    double x1_dielectricBox = discon_X1 + (disconLength - dielectricBoxWidth) * 0.5;
                    double y1_dielectricBox = yOfs_cutoffWg + (disconWgWidth - dielectricBoxHeight) * 0.5;
                    pts.Add(new CVector2D(x1_dielectricBox, y1_dielectricBox + dielectricBoxHeight)); // 頂点19
                    pts.Add(new CVector2D(x1_dielectricBox, y1_dielectricBox)); // 頂点20
                    pts.Add(new CVector2D(x1_dielectricBox + dielectricBoxWidth, y1_dielectricBox)); // 頂点21
                    pts.Add(new CVector2D(x1_dielectricBox + dielectricBoxWidth, y1_dielectricBox + dielectricBoxHeight)); // 頂点22
                    uint id_l_add_cad = cad2d.AddPolygon(pts, parent_id_l_cad).id_l_add;
                    System.Diagnostics.Debug.Assert(id_l_add_cad == dielectricBox_loopId);
                }
                {
                    uint parent_id_l_cad = 3;
                    // 観測点
                    portRef_vIds_cad[0] = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, parent_id_l_cad, new CVector2D(port1PosX, port1PosY)).id_v_add; // 頂点23
                    portRef_vIds_cad[1] = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, parent_id_l_cad, new CVector2D(port2PosX, port2PosY)).id_v_add; // 頂点24
                    // check
                    foreach (uint vId in portRef_vIds_cad)
                    {
                        System.Diagnostics.Debug.Assert(vId != 0);
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
                    // 誘電体ボックス
                    {
                        cad2d.SetColor_Loop(dielectricBox_loopId, new double[] { 1.0, 0.0, 1.0 });
                    }
                    // 境界の辺に色を付ける
                    {
                        uint[] work_eId_list = { eId_cad_src };
                        foreach (uint eId in work_eId_list)
                        {
                            cad2d.SetColor_Edge(eId, new double[] { 0.8, 0.0, 0.0 });
                        }
                    }
                    /*
                    // DEBUG
                    {
                        uint[] work_eId_list = {2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18};
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
                uint[] loopId_cad_list  = new uint[loopCnt_cad + 1];
                int[] mediaIndex_list = new int[loopId_cad_list.Length];
                for (int i = 0; i < loopCnt_cad; i++)
                {
                    loopId_cad_list[i] = (uint)(i + 1);
                    mediaIndex_list[i] = Medias.IndexOf(mediaVacumn);
                }
                loopId_cad_list[loopId_cad_list.Length - 1] = dielectricBox_loopId;
                mediaIndex_list[loopId_cad_list.Length - 1] = Medias.IndexOf(mediaDielectricBox);

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
            uint[][] loopId_cad_list_pml = { port1_pml_loopIds_cad, port2_pml_loopIds_cad };
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
                uint[] eId_cad_list = {2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14, 15, 16, 17, 18}; // PML終端磁気壁
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
                uint eId_cad = eId_cad_src;
                uint eId;
                // 要素アレイのリスト
                IList<uint> aEA = new List<uint>();
                eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                {
                    wg2d.World.Edge edge = new wg2d.World.Edge();
                    edge.Set(eId, Medias.IndexOf(mediaVacumn));
                    EdgeDic.Add(eId, edge);
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
