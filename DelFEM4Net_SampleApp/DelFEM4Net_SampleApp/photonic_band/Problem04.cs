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
    class Problem04
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;

        /// <summary>
        /// フォトニック結晶 三角形格子 六角形境界
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
            // フォトニック結晶 三角形格子
            isTriLattice = true; // 三角形格子
            //isTriLattice = false; // 長方形格子
            
            // 空孔？
            //bool isAirHole = false; // dielectric rod
            bool isAirHole = true; // air hole
            // 三角形格子の内角
            //double latticeTheta = 60.0;
            // 格子定数
            latticeA = (Math.Sqrt(3.0) / 2.0) * periodicDistanceY;
            // 周期構造距離
            periodicDistanceX = latticeA;
            // ロッドの半径
            double rodRadius = 0.30 * latticeA;
            // ロッドの比誘電率
            double rodEps = 2.76 * 2.76;
            // 格子１辺の分割数
            const int ndivForOneLattice = 8;
            // ロッドの円周分割数
            const int rodCircleDiv = 16;// 12;
            // ロッドの半径の分割数
            const int rodRadiusDiv = 5;// 4;
            // メッシュの長さ
            //double meshL = 1.05 * periodicDistanceY / ndivForOneLattice;
            double meshL = 1.05 * (latticeA / Math.Sqrt(3.0)) / ndivForOneLattice;

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
            int ndiv = ndivForOneLattice;
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
                    pts.Add(new CVector2D(0.0, 0.5 * periodicDistanceY));
                    pts.Add(new CVector2D(-0.5 * periodicDistanceX, 0.25 * periodicDistanceY));
                    pts.Add(new CVector2D(-0.5 * periodicDistanceX, -0.25 * periodicDistanceY));
                    pts.Add(new CVector2D(0.0, -0.5 * periodicDistanceY));
                    pts.Add(new CVector2D(0.5 * periodicDistanceX, -0.25 * periodicDistanceY));
                    pts.Add(new CVector2D(0.5 * periodicDistanceX, 0.25 * periodicDistanceY));
                    // 多角形追加
                    uint lId = cad2d.AddPolygon(pts).id_l_add;
                    baseLoopId = lId;
                }
                // 周期構造境界上の頂点を追加
                //  逆から追加しているのは、頂点によって新たに生成される辺に頂点を追加しないようにするため
                // 境界1：左上
                {
                    uint id_e = 1;
                    double x1 = 0.0;
                    double y1 = 0.5 * periodicDistanceY;
                    double x2 = -0.5 * periodicDistanceX;
                    double y2 = 0.25 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }
                // 境界2：左
                {
                    uint id_e = 2;
                    double x1 = -0.5 * periodicDistanceX;
                    double y1 = 0.25 * periodicDistanceY;
                    double x2 = -0.5 * periodicDistanceX;
                    double y2 = -0.25 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }
                // 境界3：左下
                {
                    uint id_e = 3;
                    double x1 = -0.5 * periodicDistanceX;
                    double y1 = -0.25 * periodicDistanceY;
                    double x2 = 0.0;
                    double y2 = -0.5 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }
                // 境界4：右下
                {
                    uint id_e = 4;
                    double x1 = 0.0;
                    double y1 = -0.5 * periodicDistanceY;
                    double x2 = 0.5 * periodicDistanceX;
                    double y2 = -0.25 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }
                // 境界5：右
                {
                    uint id_e = 5;
                    double x1 = 0.5 * periodicDistanceX;
                    double y1 = -0.25 * periodicDistanceY;
                    double x2 = 0.5 * periodicDistanceX;
                    double y2 = 0.25 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }
                // 境界6：右上
                {
                    uint id_e = 6;
                    double x1 = 0.5 * periodicDistanceX;
                    double y1 = 0.25 * periodicDistanceY;
                    double x2 = 0.0;
                    double y2 = 0.5 * periodicDistanceY;
                    WgCadUtil.DivideBoundary(cad2d, id_e, ndiv, x1, y1, x2, y2);
                }

                // ロッドを追加
                {
                    double x0 = 0.0;
                    double y0 = 0.0;
                    uint lId = WgCadUtil.AddRod(cad2d, baseLoopId, x0, y0, rodRadius, rodCircleDiv, rodRadiusDiv);
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
            for (int boundaryIndex = 0; boundaryIndex < 6; boundaryIndex++)
            {
                int cur_ndivPlus = ndiv;
                uint[] eId_cad_list = new uint[cur_ndivPlus];
                int[] mediaIndex_list = new int[eId_cad_list.Length];

                eId_cad_list[0] = (uint)(boundaryIndex + 1);

                mediaIndex_list[0] = Medias.IndexOf(mediaCladding);
                for (int i = 1; i <= cur_ndivPlus - 1; i++)
                {
                    eId_cad_list[i] = (uint)(6 + (cur_ndivPlus - 1) * (boundaryIndex + 1) - (i - 1));

                    mediaIndex_list[i] = Medias.IndexOf(mediaCladding);
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
