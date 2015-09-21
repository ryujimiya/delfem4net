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
    //  時間領域導波路 2D
    //
    //      Copyright (C) 2012-2013 ryujimiya
    //
    //      e-mail: ryujimiya@mail.goo.ne.jp
    ///////////////////////////////////////////////////////////////
    /// <summary>
    /// 時間領域導波路解析のユーティリティ
    /// </summary>
    class WgUtilForTD : WgUtil
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ヘルムホルツの方程式 剛性行列、質量行列の作成
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="loopDic">ループID→ループ情報マップ</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="KMat">剛性行列</param>
        /// <param name="MMat">質量行列</param>
        /// <returns></returns>
        public static bool MkHelmholtzMat(
            CFieldWorld world,
            uint fieldValId,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Loop> loopDic,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            out double[] KMat,
            out double[] MMat)
        {
            KMat = null;
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
                    bool res = MkHelmholtzMat_EachElementAry(
                        world,
                        fieldValId,
                        media,
                        eaId,
                        node_cnt,
                        free_node_cnt,
                        toSorted,
                        ref KMat,
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
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="media">媒質</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="KMat">剛性行列</param>
        /// <param name="MMat">質量行列</param>
        /// <returns></returns>
        private static bool MkHelmholtzMat_EachElementAry(
            CFieldWorld world,
            uint fieldValId,
            MediaInfo media,
            uint eaId,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            ref double[] KMat,
            ref double[] MMat)
        {
            System.Diagnostics.Debug.Assert(free_node_cnt * free_node_cnt == KMat.Length);
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
                /*
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
                 */
                // ∫LiLj dxdy
                double[,] integralL = new double[3, 3]
                {
                    { area / 6.0 , area / 12.0, area / 12.0 },
                    { area / 12.0, area /  6.0, area / 12.0 },
                    { area / 12.0, area / 12.0, area /  6.0 },
                    
                };

                // 要素剛性行列、要素質量行列を作る
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        // 要素剛性行列
                        eKMat[ino, jno] = media.P[0, 0] * integralDLDY[ino, jno] + media.P[1, 1] * integralDLDX[ino, jno];
                        // 要素質量行列
                        eMMat[ino, jno] = eps0 * myu0 * media.Q[2, 2] * integralL[ino, jno];
                    }
                }
                // 要素剛性行列にマージする
                for (int ino = 0; ino < nno; ino++)
                {
                    uint iNodeNumber = no_c[ino];
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int inoGlobal = (int)toSorted[iNodeNumber];
                    for (int jno = 0; jno < nno; jno++)
                    {
                        uint jNodeNumber = no_c[jno];
                        if (!toSorted.ContainsKey(jNodeNumber)) continue;
                        int jnoGlobal = (int)toSorted[jNodeNumber];

                        // clapack形式の行列格納方法で格納
                        KMat[inoGlobal + free_node_cnt * jnoGlobal] += eKMat[ino, jno];
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
        /// 散乱パラメータを計算する
        /// </summary>
        /// <param name="timeAry"></param>
        /// <param name="waveAryPort1Inc"></param>
        /// <param name="waveAryPort1"></param>
        /// <param name="waveAryPort2"></param>
        /// <param name="freqAry"></param>
        /// <param name="S11Ary"></param>
        /// <param name="S21Ary"></param>
        /// <returns></returns>
        public static bool CalcSParameter(
            double[] timeAry,
            double[] waveAryPort1Inc,
            double[] waveAryPort1,
            double[] waveAryPort2,
            out double[] freqAry,
            out double[] S11Ary,
            out double[] S21Ary
            )
        {
            int dataCnt = timeAry.Length;

            // 入射波
            double[] freqAry_Inc = null;
            System.Numerics.Complex[] fd_Inc = null;
            double[] pd_Inc = null;
            doFFTField(timeAry, waveAryPort1Inc, out freqAry_Inc, out fd_Inc, out pd_Inc);

            // 反射波
            double[] workWaveAry_R = new double[dataCnt];
            for (int i = 0; i < dataCnt; i++)
            {
                workWaveAry_R[i] = waveAryPort1[i] - waveAryPort1Inc[i];
            }
            double[] freqAry_R = null;
            System.Numerics.Complex[] fd_R = null;
            double[] pd_R = null;
            doFFTField(timeAry, workWaveAry_R, out freqAry_R, out fd_R, out pd_R);

            // 透過波
            double[] freqAry_T = null;
            System.Numerics.Complex[] fd_T = null;
            double[] pd_T = null;
            doFFTField(timeAry, waveAryPort2, out freqAry_T, out fd_T, out pd_T);

            double maxVal = 0.0;
            foreach (double val in pd_Inc)
            {
                if (val > maxVal)
                {
                    maxVal = val;
                }
            }
            //double thVal = 0.01 * maxVal;
            double thVal = 0.001 * maxVal;

            freqAry = freqAry_Inc;
            S11Ary = new double[dataCnt];
            S21Ary = new double[dataCnt];
            for (int i = 0; i < dataCnt; i++)
            {
                if (pd_Inc[i] < thVal)
                {
                    S11Ary[i] = 0;
                    S21Ary[i] = 0;
                    continue;
                }
                S11Ary[i] = Math.Sqrt(pd_R[i] / pd_Inc[i]);
                S21Ary[i] = Math.Sqrt(pd_T[i] / pd_Inc[i]);
            }

            return true;
        }

        /// <summary>
        /// 界を高速フーリエ変換する
        /// </summary>
        /// <param name="timeAry"></param>
        /// <param name="waveAry"></param>
        /// <param name="freqAry"></param>
        /// <param name="fd"></param>
        /// <param name="pd"></param>
        private static void doFFTField(
            double[] timeAry,
            double[] waveAry,
            out double[] freqAry,
            out System.Numerics.Complex[] fd,
            out double[] pd
            )
        {
            System.Diagnostics.Debug.Assert(timeAry.Length == waveAry.Length);
            int dataCnt = waveAry.Length;
            double dt = timeAry[1] - timeAry[0];

            // FFTを実行する
            {
                ILNumerics.ILInArray<double> workWaveAry = waveAry;

                // FFT
                ILNumerics.ILRetArray<ILNumerics.complex> work_fd = ILNumerics.ILMath.fft(workWaveAry);

                fd = new System.Numerics.Complex[work_fd.Length];
                pd = new double[work_fd.Length];
                int counter = 0;
                foreach (ILNumerics.complex val in work_fd)
                {
                    // フーリエ複素振幅
                    fd[counter] = new System.Numerics.Complex(val.real, val.imag);
                    // パワースペクトル
                    double abs = fd[counter].Magnitude;
                    pd[counter] = abs * abs;

                    counter++;
                }
            }

            // 時間長さ
            double tl = dt * dataCnt;
            // 周波数刻み
            double df = 1.0 / tl;
            // 周波数アレイ
            freqAry = new double[dataCnt];
            for (int i = 0; i < dataCnt; i++)
            {
                freqAry[i] = i * df;
            }
        }
    }
}
