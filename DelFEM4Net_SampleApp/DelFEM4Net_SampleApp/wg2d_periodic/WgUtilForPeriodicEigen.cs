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
using MyUtilLib.Matrix;

namespace wg2d
{
    ///////////////////////////////////////////////////////////////
    //  DelFEM4Net サンプル
    //  周期構造導波路固有値問題 2D
    //
    //      Copyright (C) 2012-2013 ryujimiya
    //
    //      e-mail: ryujimiya@mail.goo.ne.jp
    ///////////////////////////////////////////////////////////////
    /// <summary>
    /// 周期構造導波路解析のユーティリティ
    /// </summary>
    class WgUtilForPeriodicEigen : WgUtil
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 周期構造のヘルムホルツの方程式 剛性行列、質量行列の作成
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="isYDirectionPeriodic">Y方向周期構造？</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="loopDic">ループID→ループ情報マップ</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="KMat">剛性行列（周期構造導波路)</param>
        /// <param name="CMat">結合行列（周期構造導波路)</param>
        /// <param name="MMat">質量行列（周期構造導波路)</param>
        /// <returns></returns>
        public static bool MkPeriodicHelmholtzMat(
            double waveLength,
            bool isYDirectionPeriodic,
            double rotAngle,
            double[] rotOrigin,
            CFieldWorld world,
            uint fieldValId,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Loop> loopDic,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, int> toSorted,
            out double[] KMat,
            out double[] CMat,
            out double[] MMat)
        {
            KMat = null;
            CMat = null;
            MMat = null;

            // 値のフィールドIDかチェック
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            // 値のフィールドを取得する
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            KMat = new double[free_node_cnt * free_node_cnt];
            CMat = new double[free_node_cnt * free_node_cnt];
            MMat = new double[free_node_cnt * free_node_cnt];

            // 要素アレイのリストを取得する
            IList<uint> aIdEA = valField.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    // 媒質を取得する
                    MediaInfo media = new MediaInfo();
                    {
                        // ループのIDのはず
                        uint lId = eaId;
                        if (loopDic.ContainsKey(lId))
                        {
                            World.Loop loop = loopDic[lId];
                            media = medias[loop.MediaIndex];
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    bool res = MkPeriodicHelmholtzMat_EachElementAry(
                        waveLength,
                        isYDirectionPeriodic,
                        rotAngle,
                        rotOrigin,
                        world,
                        fieldValId,
                        media,
                        eaId,
                        node_cnt,
                        free_node_cnt,
                        toSorted,
                        ref KMat,
                        ref CMat,
                        ref MMat
                        );
                    if (!res)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ヘルムホルツの方程式 剛性行列 残差ベクトルの追加(要素アレイ単位)
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="isYDirectionPeriodic">Y方向周期構造？</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="media">媒質</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="KMat">剛性行列（周期構造導波路)</param>
        /// <param name="CMat">結合行列（周期構造導波路)</param>
        /// <param name="MMat">質量行列（周期構造導波路)</param>
        /// <returns></returns>
        private static bool MkPeriodicHelmholtzMat_EachElementAry(
            double waveLength,
            bool isYDirectionPeriodic,
            double rotAngle,
            double[] rotOrigin,
            CFieldWorld world,
            uint fieldValId,
            MediaInfo media,
            uint eaId,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, int> toSorted,
            ref double[] KMat,
            ref double[] CMat,
            ref double[] MMat)
        {
            double k0 = 2 * pi / waveLength;

            System.Diagnostics.Debug.Assert(!isYDirectionPeriodic || (isYDirectionPeriodic && rotAngle == 0.0));
            System.Diagnostics.Debug.Assert(free_node_cnt * free_node_cnt == KMat.Length);
            System.Diagnostics.Debug.Assert(free_node_cnt * free_node_cnt == CMat.Length);
            System.Diagnostics.Debug.Assert(free_node_cnt * free_node_cnt == MMat.Length);
            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            // 要素アレイを取得
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.TRI);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            // 値のフィールドを取得
            CField valField = world.GetField(fieldValId);
            // 値の要素セグメント（コーナー)を取得
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            // 座標の要素セグメント(コーナー)を取得
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            uint nno = 3;
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の値
            Complex[] value_c = new Complex[nno];
            // 要素節点の座標
            double[,] coord_c = new double[nno, ndim];
            // 要素剛性行列
            double[,] eKMat = new double[nno, nno];
            // 要素結合行列
            double[,] eCMat = new double[nno, nno];
            // 要素質量行列
            double[,] eMMat = new double[nno, nno];

            CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            for (uint ielem = 0; ielem < ea.Size(); ielem++)
            {
                // 要素配列から要素セグメントの節点番号を取り出す
                es_c_co.GetNodes(ielem, no_c);
                // 座標を取得
                for (uint inoes = 0; inoes < nno; inoes++)
                {
                    double[] tmpval = null;
                    ns_c_co.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == ndim);
                    for (int i = 0; i < tmpval.Length; i++)
                    {
                        coord_c[inoes, i] = tmpval[i];
                    }
                }
                if (!isYDirectionPeriodic && Math.Abs(rotAngle) >= Constants.PrecisionLowerLimit)
                {
                    // 座標を回転移動する
                    for (uint inoes = 0; inoes < nno; inoes++)
                    {
                        double[] srcPt = new double[] { coord_c[inoes, 0], coord_c[inoes, 1] };
                        double[] destPt = GetRotCoord(srcPt, rotAngle, rotOrigin);
                        for (int i = 0; i < ndim; i++)
                        {
                            coord_c[inoes, i] = destPt[i];
                        }
                    }
                }

                // 節点の値を取って来る
                es_c_va.GetNodes(ielem, no_c);
                for (uint inoes = 0; inoes < nno; inoes++)
                {
                    Complex[] tmpval = null;
                    ns_c_val.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                    value_c[inoes] = tmpval[0];
                }

                // 節点座標
                double[] p1 = new double[ndim];
                double[] p2 = new double[ndim];
                double[] p3 = new double[ndim];
                for (int i = 0; i < ndim; i++)
                {
                    p1[i] = coord_c[0, i];
                    p2[i] = coord_c[1, i];
                    p3[i] = coord_c[2, i];
                }
                // 面積を求める
                double area = CKerEMatTri.TriArea(p1, p2, p3);

                // 形状関数の微分を求める
                double[,] dldx = null;
                double[] const_term = null;
                CKerEMatTri.TriDlDx(out dldx, out const_term, p1, p2, p3);

                // ∫(dLi/dx)(dLj/dx) dxdy
                double[,] integralDLDX = new double[3, 3];
                // ∫(dLi/dy)(dLj/dy) dxdy
                double[,] integralDLDY = new double[3, 3];
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        integralDLDX[ino, jno] = area * dldx[ino, 0] * dldx[jno, 0];
                    }
                }
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        integralDLDY[ino, jno] = area * dldx[ino, 1] * dldx[jno, 1];
                    }
                }
                // ∫(dLi/dx)Lj dxdy
                double[,] integralDLDXL = new double[3, 3];
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        integralDLDXL[ino, jno] = area * dldx[ino, 0] / 3.0;
                    }
                }
                // ∫(dLi/dy)Lj dxdy
                double[,] integralDLDYL = new double[3, 3];
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        integralDLDYL[ino, jno] = area * dldx[ino, 1] / 3.0;
                    }
                }
                // ∫LiLj dxdy
                double[,] integralL = new double[3, 3]
                {
                    { area / 6.0 , area / 12.0, area / 12.0 },
                    { area / 12.0, area /  6.0, area / 12.0 },
                    { area / 12.0, area / 12.0, area /  6.0 },
                    
                };

                // 要素剛性行列、要素質量行列を作る
                //  { [K]e - jβ[C]e - β^2[M]e }{Φ}= {0}
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        // 要素剛性行列
                        //  要素質量行列を正定値行列にするために剛性行列、結合行列の符号を反転する
                        eKMat[ino, jno] = - media.P[0, 0] * integralDLDY[ino, jno] - media.P[1, 1] * integralDLDX[ino, jno]
                                             + k0 * k0 * media.Q[2, 2] * integralL[ino, jno];
                        if (isYDirectionPeriodic)
                        {
                            eCMat[ino, jno] = -media.P[1, 1] * (integralDLDYL[ino, jno] - integralDLDYL[jno, ino]);
                        }
                        else
                        {
                            eCMat[ino, jno] = -media.P[1, 1] * (integralDLDXL[ino, jno] - integralDLDXL[jno, ino]);
                        }
                        // 要素質量行列
                        eMMat[ino, jno] = media.P[1, 1] * integralL[ino, jno];
                    }
                }
                // 要素剛性行列にマージする
                for (int ino = 0; ino < nno; ino++)
                {
                    uint iNodeNumber = no_c[ino];
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int inoGlobal = toSorted[iNodeNumber];
                    for (int jno = 0; jno < nno; jno++)
                    {
                        uint jNodeNumber = no_c[jno];
                        if (!toSorted.ContainsKey(jNodeNumber)) continue;
                        int jnoGlobal = toSorted[jNodeNumber];

                        // clapack形式の行列格納方法で格納
                        KMat[inoGlobal + free_node_cnt * jnoGlobal] += eKMat[ino, jno];
                        CMat[inoGlobal + free_node_cnt * jnoGlobal] += eCMat[ino, jno];
                        MMat[inoGlobal + free_node_cnt * jnoGlobal] += eMMat[ino, jno];
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 節点数を取得する（ループ)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <returns></returns>
        public static uint GetNodeCnt(CFieldWorld world, uint fieldValId)
        {
            /*
            uint node_cnt = 0;
            Dictionary<uint, bool> nodeNoH = new Dictionary<uint, bool>();

            CField valField = world.GetField(fieldValId);
            uint nno = 3;
            //uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素アレイのリストを取得する
            IList<uint> aIdEA = valField.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                // 要素アレイを取得
                CElemAry ea = world.GetEA(eaId);
                // 座標の要素セグメント(コーナー)を取得
                CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                for (uint ielem = 0; ielem < ea.Size(); ielem++)
                {
                    // 要素配列から要素セグメントの節点番号を取り出す
                    es_c_co.GetNodes(ielem, no_c);
                    for (int ino = 0; ino < nno; ino++)
                    {
                        uint inoGlobal = no_c[ino];
                        if (!nodeNoH.ContainsKey(inoGlobal))
                        {
                            nodeNoH.Add(inoGlobal, true);
                        }
                    }
                }
            }
            node_cnt = (uint)nodeNoH.Keys.Count;
            */

            uint[] node_c_all = null;
            GetNodeList(world, fieldValId, out node_c_all);
            uint node_cnt = (uint)node_c_all.Length;
            
            return node_cnt;
        }

        /// <summary>
        /// 節点リストを取得する
        /// </summary>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        public static void GetNodeList(CFieldWorld world, uint fieldValId, out uint[] node_c_all)
        {
            node_c_all = null;
            //IList<uint> nodeList = new List<uint>();
            Dictionary<uint, bool> nodeNoH = new Dictionary<uint, bool>();

            CField valField = world.GetField(fieldValId);
            uint nno = 3;
            //uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素アレイのリストを取得する
            IList<uint> aIdEA = valField.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                // 要素アレイを取得
                CElemAry ea = world.GetEA(eaId);
                // 座標の要素セグメント(コーナー)を取得
                CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                for (uint ielem = 0; ielem < ea.Size(); ielem++)
                {
                    // 要素配列から要素セグメントの節点番号を取り出す
                    es_c_co.GetNodes(ielem, no_c);
                    for (int ino = 0; ino < nno; ino++)
                    {
                        uint inoGlobal = no_c[ino];
                        if (!nodeNoH.ContainsKey(inoGlobal))
                        {
                            //nodeList.Add(inoGlobal);
                            nodeNoH.Add(inoGlobal, true);
                        }
                    }
                }
            }
            //node_c_all = nodeList.ToArray();
            node_c_all = nodeNoH.Keys.ToArray();
        }

        /// <summary>
        /// 領域の界の値を取得する
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_area">節点番号配列</param>
        /// <param name="to_no_area">節点番号→境界上節点マップ</param>
        /// <param name="value_c_all">フィールド値の配列</param>
        /// <returns></returns>
        protected static bool GetAreaFieldValueList(CFieldWorld world, uint fieldValId, uint[] no_c_area, Dictionary<uint, uint> to_no_area, out Complex[] value_c_all)
        {
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                value_c_all = null;
                return false;
            }

            IList<uint> aIdEA = valField.GetAryIdEA();

            uint node_cnt = (uint)no_c_area.Length;

            // 境界上の界の値をすべて取得する
            value_c_all = new Complex[node_cnt];

            foreach (uint eaId in aIdEA)
            {
                bool res = getAreaFieldValueList_EachElementAry(world, fieldValId, no_c_area, to_no_area, eaId, value_c_all);
            }

            return true;
        }

        /// <summary>
        /// 領域の界の値を取得する(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_area">節点番号配列</param>
        /// <param name="to_no_area">節点番号→境界上節点番号マップ</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="value_c_all">フィールド値配列</param>
        /// <returns></returns>
        private static bool getAreaFieldValueList_EachElementAry(CFieldWorld world, uint fieldValId, uint[] no_c_area, Dictionary<uint, uint> to_no_area, uint eaId, Complex[] value_c_all)
        {
            uint node_cnt = (uint)no_c_area.Length;

            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.TRI);

            CField valField = world.GetField(fieldValId);
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 三角形要素の節点数
            uint nno = 3;
            // 座標の次元
            //uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の値
            Complex[] value_c = new Complex[nno];
            // 要素節点の座標
            //double[][] coord_c = new double[nno][];
            //for (uint inoes = 0; inoes < ndim; inoes++)
            //{
            //    coord_c[inoes] = new double[ndim];
            //}
            // 節点の値の節点セグメント
            CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
            // 節点座標の節点セグメント
            //CNodeAry.CNodeSeg ns_c_co  = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            for (uint ielem = 0; ielem < ea.Size(); ielem++)
            {
                // 要素配列から要素セグメントの節点番号を取り出す
                es_c_co.GetNodes(ielem, no_c);

                // 座標を取得
                //for(uint inoes = 0; inoes < nno; inoes++)
                //{
                //    double[] tmpval = null;
                //    ns_c_co.GetValue(no_c[inoes], out tmpval);
                //    System.Diagnostics.Debug.Assert(tmpval.Length == ndim);
                //    for (int i = 0; i < tmpval.Length; i++)
                //    {
                //        coord_c[inoes][i] = tmpval[i];
                //    }
                //}

                // 節点の値を取って来る
                es_c_va.GetNodes(ielem, no_c);
                for (uint inoes = 0; inoes < nno; inoes++)
                {
                    Complex[] tmpval = null;
                    ns_c_val.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                    value_c[inoes] = tmpval[0];
                    //System.Diagnostics.Debug.Write( "(" + value_c[inoes].Real + "," + value_c[inoes].Imag + ")" );
                }

                for (uint ino = 0; ino < nno; ino++)
                {
                    System.Diagnostics.Debug.Assert(to_no_area.ContainsKey(no_c[ino]));
                    uint ino_boundary = to_no_area[no_c[ino]];
                    System.Diagnostics.Debug.Assert(ino_boundary < node_cnt);

                    value_c_all[ino_boundary] = value_c[ino];
                    //System.Diagnostics.Debug.WriteLine("value_c_all [ " + ino_boundary + " ] = " + "(" + value_c_all[ino_boundary].Real + ", " + value_c_all[ino_boundary].Imag + ") " + Complex.Norm(value_c_all[ino_boundary]));
                }
            }
            return true;
        }

        /// <summary>
        /// 表示用に界の値を置き換える(複素数を複素数絶対値にする)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        public static void SetFieldValueForDisplay(CFieldWorld world, uint fieldValId, KrdLab.clapack.Complex[] resVec)
        {
            CField valField_base = world.GetField(fieldValId);
            System.Diagnostics.Debug.Assert(valField_base.GetFieldType() == FIELD_TYPE.ZSCALAR);

            IList<uint> aIdEA = valField_base.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                CElemAry ea = world.GetEA(eaId);
                CField valField = world.GetField(fieldValId);
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    // 要素セグメントコーナー値
                    //CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
                    // 要素セグメントコーナー座標
                    CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                    uint nno = 3;
                    //uint ndim = 2;
                    // 要素節点の全体節点番号
                    uint[] no_c = new uint[nno];
                    // 要素節点の値
                    //Complex[] value_c = new Complex[nno];
                    // 要素節点の座標
                    //double[,] coord_c = new double[nno, ndim];
                    CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
                    //CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);
                    for (uint ielem = 0; ielem < ea.Size(); ielem++)
                    {
                        // 要素配列から要素セグメントの節点番号を取り出す
                        es_c_co.GetNodes(ielem, no_c);
                        // 座標を取得
                        //for (uint inoes = 0; inoes < nno; inoes++)
                        //{
                        //    double[] tmpval = null;
                        //    ns_c_co.GetValue(no_c[inoes], out tmpval);
                        //    System.Diagnostics.Debug.Assert(tmpval.Length == ndim);
                        //    for (int i = 0; i < tmpval.Length; i++)
                        //    {
                        //        coord_c[inoes, i] = tmpval[i];
                        //    }
                        //}
                        // 節点の値を取って来る
                        //es_c_va.GetNodes(ielem, no_c);
                        //for (uint inoes = 0; inoes < nno; inoes++)
                        //{
                        //    Complex[] tmpval = null;
                        //    ns_c_val.GetValue(no_c[inoes], out tmpval);
                        //    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                        //    value_c[inoes] = tmpval[0];
                        //}
                        // 節点の値を絶対値にする
                        for (uint inoes = 0; inoes < nno; inoes++)
                        {
                            uint inoGlobal = no_c[inoes];
                            KrdLab.clapack.Complex cvalue = resVec[inoGlobal];
                            ns_c_val.SetValue(no_c[inoes], 0, cvalue.Real);
                            ns_c_val.SetValue(no_c[inoes], 1, cvalue.Imaginary);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 固有値のソート用クラス
        /// </summary>
        class KrdLabComplexEigenModeItem : IComparable<KrdLabComplexEigenModeItem>
        {
            public uint index;
            public KrdLab.clapack.Complex eval;
            public KrdLab.clapack.Complex[] evec;

            public KrdLabComplexEigenModeItem(uint index_, KrdLab.clapack.Complex eval_, KrdLab.clapack.Complex[] evec_)
            {
                index = index_;
                eval = eval_;
                evec = evec_;
            }
            int IComparable<KrdLabComplexEigenModeItem>.CompareTo(KrdLabComplexEigenModeItem other)
            {
                //BUG: 小数点以下の場合、比較できない
                //int cmp = (int)(this.eval.Real - other.eval.Real);
                int cmp = 0;
                if (Math.Abs(this.eval.Real) < 1.0e-12 && Math.Abs(other.eval.Real) < 1.0e-12)
                {
                    double cmpfi = this.eval.Imaginary - other.eval.Imaginary;
                    if (Math.Abs(cmpfi) < 1.0E-15)
                    {
                        cmp = 0;
                    }
                    else if (cmpfi > 0)
                    {
                        cmp = 1;
                    }
                    else
                    {
                        cmp = -1;
                    }
                }
                else if (Math.Abs(this.eval.Real) < 1.0e-12 && Math.Abs(other.eval.Real) >= 1.0e-12)
                {
                    if (other.eval.Real >= 0)
                    {
                        cmp = -1;
                    }
                    else
                    {
                        cmp = 1;
                    }
                }
                else if (Math.Abs(this.eval.Real) >= 1.0e-12 && Math.Abs(other.eval.Real) < 1.0e-12)
                {
                    if (this.eval.Real >= 0)
                    {
                        cmp = 1;
                    }
                    else
                    {
                        cmp = -1;
                    }
                }
                else
                {
                    double cmpf = this.eval.Real - other.eval.Real;
                    if (Math.Abs(cmpf) < 1.0E-15)
                    {
                        cmp = 0;
                    }
                    else
                    {
                        if (cmpf > 0)
                        {
                            cmp = 1;
                        }
                        else
                        {
                            cmp = -1;
                        }
                    }
                }
                return cmp;
            }

        }
        
        /// <summary>
        /// 固有値をソートする
        /// </summary>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        public static  void Sort1DEigenMode(KrdLab.clapack.Complex[] evals, KrdLab.clapack.Complex[,] evecs)
        {
            KrdLabComplexEigenModeItem[] items = new KrdLabComplexEigenModeItem[evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                KrdLab.clapack.Complex[] evec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, i);
                items[i] = new KrdLabComplexEigenModeItem((uint)i, evals[i], evec);
            }
            Array.Sort(items);

            uint imode = 0;
            foreach (KrdLabComplexEigenModeItem item in items)
            {
                evals[imode] = item.eval;
                MyUtilLib.Matrix.MyMatrixUtil.matrix_setRowVec(evecs, (int)imode, item.evec);
                imode++;
            }

        }
    }
}
