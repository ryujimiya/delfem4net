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
    //  導波管伝達問題 2D
    //
    //      Copyright (C) 2012-2013 ryujimiya
    //
    //      e-mail: ryujimiya@mail.goo.ne.jp
    ///////////////////////////////////////////////////////////////
    /// <summary>
    /// 導波管解析のユーティリティ
    /// </summary>
    class WgUtil
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 波のモード区分
        /// </summary>
        public enum WaveModeDV { TE, TM };

        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        public const double pi = Math.PI;
        public const double c0 = 2.99792458e+8;
        public const double myu0 = 4.0e-7 * pi;
        public const double eps0 = 8.85418782e-12;//1.0 / (myu0 * c0 * c0);

        /// <summary>
        /// ワールド座標系の界の値をクリアする
        /// </summary>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        public static void ClearFieldValues(CFieldWorld world, uint fieldValId)
        {
            // フィールドを取得する
            CField valField = world.GetField(fieldValId);
            System.Diagnostics.Debug.Assert(valField.GetFieldType() == FIELD_TYPE.ZSCALAR);

            IList<uint> aIdEA = valField.GetAryIdEA();
            // 要素アレイIDを走査
            foreach (uint eaId in aIdEA)
            {
                /*
                // 要素アレイを取得する
                CElemAry ea = world.GetEA(eaId);
                // 補間タイプを取得
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    // 三角形要素の場合

                    // フィールド値の要素セグメントを取得する
                    CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
                    // 座標の要素セグメントを取得する
                    CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                    // 三角形の節点数
                    uint nno = 3;
                    // 座標系次元
                    //uint ndim = 2;
                    // 要素節点の全体節点番号
                    uint[] no_c = new uint[nno];
                    // 要素節点の値
                    //Complex[] value_c = new Complex[nno];
                    // フィールド値の節点セグメントを取得
                    CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
                    // 座標の節点セグメントを取得
                    //CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

                    // 節点セグメントのフィールド値をすべて０にする
                    ns_c_val.SetZero();
                    //for (uint ielem = 0; ielem < ea.Size(); ielem++)
                    //{
                    //    // 要素配列から要素セグメントの節点番号を取り出す
                    //    es_c_co.GetNodes(ielem, no_c);
                    //    //// 節点の値を取って来る
                    //    //es_c_va.GetNodes(ielem, no_c);
                    //    //for (uint inoes = 0; inoes < nno; inoes++)
                    //    //{
                    //    //    Complex[] tmpval = null;
                    //    //    ns_c_val.GetValue(no_c[inoes], out tmpval);
                    //    //    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                    //    //    value_c[inoes] = tmpval[0];
                    //    //}
                    //    // 節点の値を０にする
                    //    for (uint inoes = 0; inoes < nno; inoes++)
                    //    {
                    //        // 複素数として格納してるので、自由度は２。
                    //        ns_c_val.SetValue(no_c[inoes], 0, 0.0);
                    //        ns_c_val.SetValue(no_c[inoes], 1, 0.0);
                    //    }
                    //}
                }
                 */
                // 要素アレイを取得する
                CElemAry ea = world.GetEA(eaId);
                // 要素補間に関係なくクリアする
                // フィールド値の節点セグメントを取得
                CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
                // 節点セグメントのフィールド値をすべて０にする
                ns_c_val.SetZero();
            }
        }

        /// <summary>
        /// ヘルムホルツの方程式 剛性行列 残差ベクトルの追加
        /// </summary>
        /// <param name="ls">リニアシステムオブジェクト(複素数版)</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="loopDic">ワールド座標系のループID→ループ情報マップ</param>
        /// <returns></returns>
        public static  bool AddLinSysHelmholtz(
            CZLinearSystem ls,
            double waveLength,
            CFieldWorld world,
            uint fieldValId,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Loop> loopDic)
        {
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            IList<uint> aIdEA = valField.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                //System.Diagnostics.Debug.WriteLine("AddLinSysHelmholtz eaId:" + eaId);
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
                    }
                    // 一時バッファの作成
                    uint ntmp = ls.GetTmpBufferSize();
                    int[] tmpBuffer = new int[ntmp];
                    for (int i = 0; i < ntmp; i++)
                    {
                        tmpBuffer[i] = -1;
                    }
                    // 要素アレイ単位の処理
                    bool res = addLinSysHelmholtz_EachElementAry(
                        ls,
                        waveLength,
                        world,
                        fieldValId,
                        eaId,
                        media,
                        tmpBuffer);
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
        /// <param name="ls">リニアシステム（複素数版)</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="media">媒質情報</param>
        /// <param name="tmpBuffer">一時バッファ</param>
        /// <returns></returns>
        private static bool addLinSysHelmholtz_EachElementAry(
            CZLinearSystem ls,
            double waveLength,
            CFieldWorld world,
            uint fieldValId,
            uint eaId,
            MediaInfo media,
            int[] tmpBuffer)
        {
            // 波数
            double k0 = 2 * pi / waveLength;

            // 要素アレイを取得
            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.TRI);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            // フィールド値を扱うフィールドを取得
            CField valField = world.GetField(fieldValId);
            // フィールド値のセグメントを取得
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            // 座標のセグメントを取得
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 三角形要素の頂点数(１次補間三角形要素の節点数）
            uint nno = 3;
            // 座標の次元(2次元)
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の値
            Complex[] value_c = new Complex[nno];
            // 要素節点の座標
            double[][] coord_c = new double[nno][];
            for (int inoes = 0; inoes < nno; inoes++)
            {
                coord_c[inoes] = new double[ndim];
            }
            // 要素剛性行列
            //Complex[,] emat = new Complex[nno, nno];
            Complex[] ematBuffer = new Complex[nno * nno];

            // 要素剛性行列(コーナ-コーナー)
            CZMatDia_BlkCrs_Ptr mat_cc = ls.GetMatrixPtr(fieldValId, ELSEG_TYPE.CORNER, world);
            // 要素残差ベクトル(コーナー)
            CZVector_Blk_Ptr res_c = ls.GetResidualPtr(fieldValId, ELSEG_TYPE.CORNER, world);

            // フィールド値のノードセグメントを取得
            CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
            // 座標のノードセグメントを取得
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            // 要素を走査
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
                        coord_c[inoes][i] = tmpval[i];
                    }
                }
                //MatrixUtil.printMatrix("coord_c", coord_c);

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
                double[] p1 = coord_c[0];
                double[] p2 = coord_c[1];
                double[] p3 = coord_c[2];
                // 面積を求める
                double area = CKerEMatTri.TriArea(p1, p2, p3);

                // 形状関数の微分を求める
                double[,] dldx = null;
                double[] const_term = null;
                CKerEMatTri.TriDlDx(out dldx, out const_term, p1, p2, p3);

                double[,] integralDLDX = new double[3, 3];
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
                double[,] integralL = new double[3, 3]
                {
                    { area / 6.0 , area / 12.0, area / 12.0 },
                    { area / 12.0, area /  6.0, area / 12.0 },
                    { area / 12.0, area / 12.0, area /  6.0 },
                    
                };

                // 要素剛性行列を作る
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        //emat[ino, jno]
                        ematBuffer[ino * nno + jno] = media.P[0, 0] * integralDLDY[ino, jno] + media.P[1, 1] * integralDLDX[ino, jno]
                                             - k0 * k0 * media.Q[2, 2] * integralL[ino, jno];
                    }
                }
                // 要素剛性行列にマージする
                mat_cc.Mearge(nno, no_c, nno, no_c, 1, ematBuffer, ref tmpBuffer);
            }

            return true;
        }

        /// <summary>
        /// 境界上の節点番号の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上の節点番号のマップ</param>
        /// <returns></returns>
        public static bool GetBoundaryNodeList(CFieldWorld world, uint fieldValId, out uint[] no_c_all, out Dictionary<uint, uint> to_no_boundary)
        {
            no_c_all = null;
            to_no_boundary = null;

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            IList<uint> aIdEA = valField.GetAryIdEA();

            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary)作成
            to_no_boundary = new Dictionary<uint, uint>();

            foreach (uint eaId in aIdEA)
            {
                bool res = getBoundaryNodeList_EachElementAry(world, fieldValId, eaId, to_no_boundary);
                if (!res)
                {
                    return false;
                }
            }
            //境界節点番号→全体節点番号変換テーブル(no_c_all)作成 
            no_c_all = new uint[to_no_boundary.Count];
            foreach (KeyValuePair<uint, uint> kvp in to_no_boundary)
            {
                uint ino_boundary = kvp.Value;
                uint ino = kvp.Key;
                no_c_all[ino_boundary] = ino;
            }

            return true;
        }

        /// <summary>
        /// 境界上の節点番号の取得(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="to_no_boundary">全体節点番号→境界上節点番号マップ</param>
        /// <returns></returns>
        private static bool getBoundaryNodeList_EachElementAry(CFieldWorld world, uint fieldValId, uint eaId, Dictionary<uint, uint> to_no_boundary)
        {
            // 要素アレイを取得する
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (ea.ElemType() != ELEM_TYPE.LINE)
            {
                return false;
            }

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            // 座標セグメントを取得
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            //境界節点番号→全体節点番号変換テーブル(no_c_all)作成
            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary)作成
            uint node_cnt = ea.Size() + 1;  // 全節点数

            // 線要素の節点数
            uint nno = 2;
            uint[] no_c = new uint[nno]; // 要素節点の全体節点番号

            for (uint ielem = 0; ielem < ea.Size(); ielem++)
            {
                // 要素配列から要素セグメントの節点番号を取り出す
                es_c_co.GetNodes(ielem, no_c);

                for (uint ino = 0; ino < nno; ino++)
                {
                    if (!to_no_boundary.ContainsKey(no_c[ino]))
                    {
                        uint ino_boundary_tmp = (uint)to_no_boundary.Count;
                        to_no_boundary[no_c[ino]] = ino_boundary_tmp;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 境界上の節点番号と座標の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上の節点番号のマップ</param>
        /// <param name="coord_c_all">座標リスト</param>
        /// <returns></returns>
        public static bool GetBoundaryCoordList(
            CFieldWorld world, uint fieldValId,
            out uint[] no_c_all, out Dictionary<uint, uint> to_no_boundary, out double[][] coord_c_all)
        {
            return GetBoundaryCoordList(
                world, fieldValId,
                0.0, null,
                out no_c_all, out to_no_boundary, out coord_c_all);
        }

        /// <summary>
        /// 境界上の節点番号と座標の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上の節点番号のマップ</param>
        /// <param name="coord_c_all">座標リスト</param>
        /// <returns></returns>
        public static bool GetBoundaryCoordList(
            CFieldWorld world, uint fieldValId,
            double rotAngle, double[] rotOrigin,
            out uint[] no_c_all, out Dictionary<uint, uint> to_no_boundary, out double[][] coord_c_all)
        {
            no_c_all = null;
            to_no_boundary = null;
            coord_c_all = null;

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            IList<uint> aIdEA = valField.GetAryIdEA();

            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary)作成
            to_no_boundary = new Dictionary<uint, uint>();
            IList<double[]> coord_c_list = new List<double[]>();

            foreach (uint eaId in aIdEA)
            {
                bool res = getBoundaryCoordList_EachElementAry(
                    world, fieldValId, eaId,
                    rotAngle, rotOrigin,
                    ref to_no_boundary, ref coord_c_list);
                if (!res)
                {
                    return false;
                }
            }
            //境界節点番号→全体節点番号変換テーブル(no_c_all)作成 
            no_c_all = new uint[to_no_boundary.Count];
            foreach (KeyValuePair<uint, uint> kvp in to_no_boundary)
            {
                uint ino_boundary = kvp.Value;
                uint ino = kvp.Key;
                no_c_all[ino_boundary] = ino;
            }
            coord_c_all = coord_c_list.ToArray();

            return true;
        }

        /// <summary>
        /// 境界上の節点番号と座標の取得(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="to_no_boundary">全体節点番号→境界上節点番号マップ</param>
        /// <returns></returns>
        private static bool getBoundaryCoordList_EachElementAry(
            CFieldWorld world, uint fieldValId, uint eaId,
            double rotAngle, double[] rotOrigin,
            ref Dictionary<uint, uint> to_no_boundary, ref IList<double[]> coord_c_list)
        {
            // 要素アレイを取得する
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (ea.ElemType() != ELEM_TYPE.LINE)
            {
                return false;
            }

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            // 座標セグメントを取得
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
            // 座標のノードセグメントを取得
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            //境界節点番号→全体節点番号変換テーブル(no_c_all)作成
            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary)作成
            uint node_cnt = ea.Size() + 1;  // 全節点数

            // 線要素の節点数
            uint nno = 2;
            // 座標の次元
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の座標
            double[][] coord_c = new double[nno][];
            for (int inoes = 0; inoes < nno; inoes++)
            {
                coord_c[inoes] = new double[ndim];
            }

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
                        coord_c[inoes][i] = tmpval[i];
                    }
                }
                if (Math.Abs(rotAngle) >= Constants.PrecisionLowerLimit)
                {
                    // 座標を回転移動する
                    for (uint inoes = 0; inoes < nno; inoes++)
                    {
                        double[] srcPt = coord_c[inoes];
                        double[] destPt = GetRotCoord(srcPt, rotAngle, rotOrigin);
                        for (int i = 0; i < ndim; i++)
                        {
                            coord_c[inoes][i] = destPt[i];
                        }
                    }
                }

                for (uint ino = 0; ino < nno; ino++)
                {
                    if (!to_no_boundary.ContainsKey(no_c[ino]))
                    {
                        uint ino_boundary_tmp = (uint)to_no_boundary.Count;
                        to_no_boundary[no_c[ino]] = ino_boundary_tmp;
                        coord_c_list.Add(new double[] { coord_c[ino][0], coord_c[ino][1] });
                        System.Diagnostics.Debug.Assert(coord_c_list.Count == (ino_boundary_tmp + 1));
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// 回転移動する
        /// </summary>
        /// <param name="srcPt"></param>
        /// <param name="rotAngle"></param>
        /// <param name="rotOrigin"></param>
        /// <returns></returns>
        public static double[] GetRotCoord(double[] srcPt, double rotAngle, double[] rotOrigin = null)
        {
            double[] destPt = new double[2];
            double x0 = 0;
            double y0 = 0;
            if (rotOrigin != null)
            {
                x0 = rotOrigin[0];
                y0 = rotOrigin[1];
            }
            double cosA = Math.Cos(rotAngle);
            double sinA = Math.Sin(rotAngle);
            destPt[0] = cosA * (srcPt[0] - x0) + sinA * (srcPt[1] - y0);
            destPt[1] = -sinA * (srcPt[0] - x0) + cosA * (srcPt[1] - y0);
            return destPt;
        }

        /// <summary>
        /// 境界のパターンを追加してみる
        /// ※既定で生成されるブロック行列にマージされていなかったので対応
        ///   節点を含む隣接要素以外のマージができないようになっている？ようです。
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <returns></returns>
        public static bool MakeWaveguidePortBCPattern(CZLinearSystem ls, CFieldWorld world, uint fieldValId)
        {
            // フィールドを取得する
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }

            bool res;

            //境界節点番号→全体節点番号変換テーブル(no_c_all)
            uint[] no_c_all = null;
            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary
            Dictionary<uint, uint> to_no_boundary = null;

            // 境界上のすべての節点番号を取り出す
            res = GetBoundaryNodeList(world, fieldValId, out no_c_all, out to_no_boundary);
            if (!res)
            {
                return false;
            }

            /*
            // 要素アレイIDのリストを取得
            IList<uint> aIdEA = valField.GetAryIdEA();
            if (aIdEA.Count > 0)
            {
                // check
                // 要素アレイを走査
                foreach (uint eaId in aIdEA)
                {
                    System.Diagnostics.Debug.Assert(valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.LINE11);
                    if (valField.GetInterpolationType(eaId, world) != INTERPOLATION_TYPE.LINE11)
                    {
                        return false;
                    }
                    // 要素アレイを取得
                    System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
                    CElemAry ea = world.GetEA(eaId);
                    System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
                    if (ea.ElemType() != ELEM_TYPE.LINE)
                    {
                        return false;
                    }
                }
            }
             */
            res = makeWaveguidePortBCPattern_Core(ls, world, fieldValId, no_c_all, to_no_boundary);
            if (!res)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 境界のパターン追加のコア処理
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上節点番号マップ</param>
        /// <returns></returns>
        private static bool makeWaveguidePortBCPattern_Core(CZLinearSystem ls, CFieldWorld world, uint fieldValId, uint[] no_c_all, Dictionary<uint, uint> to_no_boundary)
        {
            // フィールドを取得
            //CField valField = world.GetField(fieldValId);

            if (!world.IsIdField(fieldValId))
            {
                return false;
            }

            // 境界上の節点数
            uint node_cnt = (uint)no_c_all.Length;
            // 要素剛性行列(コーナ-コーナー)
            CZMatDia_BlkCrs_Ptr mat_cc = ls.GetMatrixPtr(fieldValId, ELSEG_TYPE.CORNER, world);
            // 要素残差ベクトル(コーナー)
            CZVector_Blk_Ptr res_c = ls.GetResidualPtr(fieldValId, ELSEG_TYPE.CORNER, world);

            //System.Diagnostics.Debug.WriteLine("fieldValId: {0}", fieldValId);
            //System.Diagnostics.Debug.WriteLine("NBlkMatCol:" + mat_cc.NBlkMatCol()); // （境界でなく領域の総節点数と同じ?)
            //System.Diagnostics.Debug.WriteLine("NBlkMatRow:" + mat_cc.NBlkMatRow());
            //System.Diagnostics.Debug.WriteLine("LenBlkCol:" + mat_cc.LenBlkCol());
            //System.Diagnostics.Debug.WriteLine("LenBlkRow:" + mat_cc.LenBlkRow());
            //System.Diagnostics.Debug.WriteLine("node_cnt:" + node_cnt);

            // どうもマトリクスにマージされていない 節点を含む隣接要素以外のマージができないようになっている？
            // パターンを追加してみる
            using (CIndexedArray crs = new CIndexedArray())
            {
                crs.InitializeSize(mat_cc.NBlkMatCol());

                //crs.Fill(mat_cc.NBlkMatCol(), mat_cc.NBlkMatRow());
                using (UIntVectorIndexer index = crs.index)
                using (UIntVectorIndexer ary = crs.array)
                {
                    //BUGFIX
                    //for (int iblk = 0; iblk < (int)crs.Size(); iblk++)
                    // indexのサイズは crs.Size() + 1 (crs.Size() > 0のとき)
                    for (int iblk = 0; iblk < index.Count; iblk++)
                    {
                        index[iblk] = 0;
                    }
                    for (int iblk = 0; iblk < mat_cc.NBlkMatCol(); iblk++)
                    {
                        // 現在のマトリクスのインデックス設定をコピーする
                        uint npsup = 0;
                        ConstUIntArrayIndexer cur_rows = mat_cc.GetPtrIndPSuP((uint)iblk, out npsup);
                        foreach (uint row_index in cur_rows)
                        {
                            ary.Add(row_index);
                        }
                        index[iblk + 1] = (uint)ary.Count;

                        // 境界の節点に関して列を追加
                        int ino_boundary = -1;
                        int col = -1;
                        for (uint ino_boundary_tmp = 0; ino_boundary_tmp < node_cnt; ino_boundary_tmp++)
                        {
                            int tmp_col = (int)no_c_all[ino_boundary_tmp];
                            if (tmp_col == iblk)
                            {
                                col = tmp_col;
                                ino_boundary = (int)ino_boundary_tmp;
                                break;
                            }
                        }
                        if (col != -1 && ino_boundary != -1)
                        {
                            // 関連付けられていない節点をその行の列データの最後に追加
                            //int last_index = (int)index[col + 1] - 1;
                            //System.Diagnostics.Debug.Assert(last_index == ary.Count - 1);
                            int add_cnt = 0;
                            for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                            {
                                uint row = no_c_all[jno_boundary];
                                //if (ino_boundary != jno_boundary)  // 対角要素は除く
                                if (col != row)  // 対角要素は除く
                                {
                                    if (!cur_rows.Contains(row))
                                    {
                                        //ary.Insert(last_index + 1 + add_cnt, row);
                                        ary.Add(row);
                                        add_cnt++;
                                        //System.Diagnostics.Debug.WriteLine("added:" + col + " " + row);
                                    }
                                }
                            }
                            if (add_cnt > 0)
                            {
                                //index[col + 1] += (uint)add_cnt;
                                index[col + 1] = (uint)ary.Count;
                            }
                            //System.Diagnostics.Debug.WriteLine("{0} {1}", iblk, index[col + 1] - index[col]);
                            //check
                            //System.Diagnostics.Debug.Write(string.Format("{0}: ", col));
                            //for (uint i = index[col]; i < index[col + 1]; i++)
                            //{
                            //    System.Diagnostics.Debug.Write(string.Format(" {0}", ary[(int)i])); 
                            //}
                            //System.Diagnostics.Debug.WriteLine(" ");
                        }
                    }
                }
                crs.Sort();
                System.Diagnostics.Debug.Assert(crs.CheckValid());
                //check1
                //for (uint ino_boundary_tmp = 0; ino_boundary_tmp < node_cnt; ino_boundary_tmp++)
                //{
                //    int col = (int)no_c_all[ino_boundary_tmp];
                //    System.Diagnostics.Debug.WriteLine("chk1 {0} {1}", col, crs.index[col + 1] - crs.index[col]);
                //}
                // パターンを削除する
                mat_cc.DeletePattern();
                // パターンを追加する
                mat_cc.AddPattern(crs);
                // check2
                //for (uint ino_boundary_tmp = 0; ino_boundary_tmp < node_cnt; ino_boundary_tmp++)
                //{
                //    int col = (int)no_c_all[ino_boundary_tmp];
                //    uint npsup = 0;
                //    ConstUIntArrayIndexer cur_rows = mat_cc.GetPtrIndPSuP((uint)col, out npsup);
                //    System.Diagnostics.Debug.WriteLine("chk2:{0} {1}", col, npsup);
                //}
            }
            return true;
        }

        /// <summary>
        /// 導波管開口境界条件
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
        /// <param name="isInputPort">入射ポート？</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="edgeDic">ワールド座標系の辺ID→辺情報のマップ</param>
        /// <param name="ryy_1d">1次元有限要素法[ryy]配列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列(i,j: i:固有値インデックス j:節点)</param>
        /// <returns></returns>
        public static bool AddLinSys_WaveguidePortBC(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            CFieldWorld world,
            uint fieldValId,
            uint[] fixedBcNodes,
            bool isInputPort,
            int incidentModeIndex,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Edge> edgeDic,
            out double[,] ryy_1d,
            out Complex[] eigen_values,
            out Complex[,] eigen_vecs)
        {
            ryy_1d = null;
            eigen_values = null;
            eigen_vecs = null;

            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            bool res;
            //境界節点番号→全体節点番号変換テーブル(no_c_all)
            uint[] no_c_all = null;
            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary
            Dictionary<uint, uint> to_no_boundary = null;
            // 節点座標
            double[,] coord_c_all = null;

            // 導波管の幅取得
            double[] coord_c_first = null;
            double[] coord_c_last = null;
            double waveguideWidth = 0;
            res = getRectangleWaveguideStructInfo(world, fieldValId, out coord_c_first, out coord_c_last, out waveguideWidth);
            if (!res)
            {
                return false;
            }

            // 境界上のすべての節点番号を取り出す
            res = GetBoundaryNodeList(world, fieldValId, out no_c_all, out to_no_boundary);
            if (!res)
            {
                return false;
            }

            // ryy_1d行列、座標、固有値、固有ベクトルを取得する
            //uint max_mode = 1;
            uint max_mode = int.MaxValue;
            res = solvePortWaveguideEigen(
                ls,
                waveLength,
                waveModeDv,
                world,
                fieldValId,
                fixedBcNodes,
                max_mode,
                coord_c_first,
                waveguideWidth,
                no_c_all,
                to_no_boundary,
                medias,
                edgeDic,
                out ryy_1d,
                out eigen_values,
                out eigen_vecs,
                out coord_c_all);
            if (!res)
            {
                return false;
            }

            // 境界条件をリニアシステムに設定する
            uint ntmp = ls.GetTmpBufferSize();
            int[] tmpBuffer = new int[ntmp];
            for (int i = 0; i < ntmp; i++)
            {
                tmpBuffer[i] = -1;
            }
            res = addLinSys_WaveguidePortBC_Core(
                ls,
                waveLength,
                waveModeDv,
                world,
                fieldValId,
                isInputPort,
                incidentModeIndex,
                ryy_1d,
                eigen_values,
                eigen_vecs,
                no_c_all,
                coord_c_all,
                ref tmpBuffer);
            if (!res)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 導波管の固有モード取得 + ryy_1d(1次線要素のpyy{N}t{N}マトリクス)取得
        ///     Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることを前提にしています。
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
        /// <param name="max_mode">固有モードの考慮数</param>
        /// <param name="coord_c_first">始点の座標</param>
        /// <param name="waveguideWidth">導波管の幅</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="to_no_boundary">節点番号→境界上節点番号マップ</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="edgeDic">ワールド座標系の辺ID→辺情報のマップ</param>
        /// <param name="ryy_1d">[ryy]FEM行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列(i, j)i:固有値インデックス, j:節点</param>
        /// <param name="coord_c_all">節点座標配列</param>
        /// <returns></returns>
        private static bool solvePortWaveguideEigen(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            CFieldWorld world,
            uint fieldValId,
            uint[] fixedBcNodes,
            uint max_mode,
            double[] coord_c_first,
            double waveguideWidth,
            uint[] no_c_all,
            Dictionary<uint, uint> to_no_boundary,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Edge> edgeDic,
            out double[,] ryy_1d,
            out Complex[] eigen_values,
            out Complex[,] eigen_vecs,
            out double[,] coord_c_all)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 * c0;
            uint node_cnt = (uint)no_c_all.Length;

            // 固有値、固有ベクトル
            eigen_values = null;
            eigen_vecs = null;
            // 節点座標
            uint ndim = 2;
            coord_c_all = new double[node_cnt, ndim];
            // 固有モード解析でのみ使用するuzz_1d, txx_1d
            double[,] txx_1d = new double[node_cnt, node_cnt];
            double[,] uzz_1d = new double[node_cnt, node_cnt];
            // ryy_1dマトリクス (1次線要素)
            ryy_1d = new double[node_cnt, node_cnt];
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                {
                    ryy_1d[ino_boundary, jno_boundary] = 0.0;
                    txx_1d[ino_boundary, jno_boundary] = 0.0;
                    uzz_1d[ino_boundary, jno_boundary] = 0.0;
                }
            }

            // フィールドを取得する
            CField valField = world.GetField(fieldValId);
            // 要素アレイIDのリストを取得する
            IList<uint> aIdEA = valField.GetAryIdEA();

            // ryy_1dマトリクスの取得
            foreach (uint eaId in aIdEA)
            {
                // 媒質を取得する
                MediaInfo media = new MediaInfo();
                {
                    // 辺のIDのはず
                    uint eId = eaId;
                    if (edgeDic.ContainsKey(eId))
                    {
                        World.Edge edge = edgeDic[eId];
                        media = medias[edge.MediaIndex];
                    }
                }
                bool res = addElementMatOf1dEigenValueProblem(
                    world,
                    fieldValId,
                    eaId,
                    no_c_all,
                    to_no_boundary,
                    media,
                    ref txx_1d,
                    ref ryy_1d,
                    ref uzz_1d,
                    ref coord_c_all);
                if (!res)
                {
                    return false;
                }
            }

            // 強制境界
            if (fixedBcNodes != null)
            {
                foreach (uint fixedBcNode in fixedBcNodes)
                {
                    if (to_no_boundary.ContainsKey(fixedBcNode))
                    {
                        uint ino_boundary = to_no_boundary[fixedBcNode];
                        //System.Diagnostics.Debug.WriteLine("fixedBcNode: " + fixedBcNode + " ino_boundary: " + ino_boundary);
                        for (int k = 0; k < node_cnt; k++)
                        {
                            txx_1d[ino_boundary, k] = 0.0;
                            txx_1d[k, ino_boundary] = 0.0;
                            ryy_1d[ino_boundary, k] = 0.0;
                            ryy_1d[k, ino_boundary] = 0.0;
                            uzz_1d[ino_boundary, k] = 0.0;
                            uzz_1d[k, ino_boundary] = 0.0;
                        }
                        //ryy_1d[ino_boundary, ino_boundary] = 1.0;
                    }
                }
            }
            Complex[] evals = null;
            Complex[,] evecs = null;
            // 固有値、固有ベクトルを求める
            {
                // 対角が０の要素を除く（強制境界のはず)
                IList<int> sortedNodes = new List<int>();
                Dictionary<int, int> orgNodeToSortedNode = new Dictionary<int, int>();
                int orgNodeCnt = txx_1d.GetLength(0);
                for (int ino = 0; ino < orgNodeCnt; ino++)
                {
                    if (Math.Abs(txx_1d[ino, ino]) < 1.0e-12)
                    {
                        //
                    }
                    else
                    {
                        sortedNodes.Add(ino);
                        orgNodeToSortedNode.Add(ino, sortedNodes.Count - 1);
                    }
                }
                int sortedNodeCnt = sortedNodes.Count;
                double[,] stiffnessMat = new double[sortedNodeCnt, sortedNodeCnt];
                double[,] massMat = new double[sortedNodeCnt, sortedNodeCnt];
                for (int ino = 0; ino < sortedNodeCnt; ino++)
                {
                    int org_ino = sortedNodes[ino];
                    for (int jno = 0; jno < sortedNodeCnt; jno++)
                    {
                        int org_jno = sortedNodes[jno];
                        stiffnessMat[ino, jno] = txx_1d[org_ino, org_jno] - k0 * k0 * uzz_1d[org_ino, org_jno];
                        // 定式化のBUGFIX
                        //massMat[ino, jno] = ryy_1d[org_ino, org_jno];
                        // ( [txx] - k0^2[uzz] + β^2[ryy]){Ez} = {0}より
                        // [K]{x} = λ[M]{x}としたとき、λ = β^2 とすると[M] = -[ryy]
                        massMat[ino, jno] = -ryy_1d[org_ino, org_jno];
                    }
                }
                Complex[,] evecs_sorted = null;
                {
                    // 標準固有値問題
                    //double[,] matA = MyMatrixUtil.product(MyMatrixUtil.matrix_Inverse(massMat), stiffnessMat);
                    //solveEigen(matA, out evals, out evecs_sorted);

                    // 一般化固有値問題
                    solveGeneralizedEigen(stiffnessMat, massMat, out evals, out evecs_sorted);
                }
                evecs = new Complex[evals.Length, orgNodeCnt];
                for (int imode = 0; imode < evals.Length; imode++)
                {
                    for (int ino = 0; ino < orgNodeCnt; ino++)
                    {
                        evecs[imode, ino] = new Complex(0.0);
                        if (orgNodeToSortedNode.ContainsKey(ino))
                        {
                            evecs[imode, ino] = evecs_sorted[imode, orgNodeToSortedNode[ino]];
                        }
                    }
                }
                // 固有値のソート
                sort1DEigenMode(evals, evecs);

                // 位相調整
                for (int imode = 0; imode < evals.Length; imode++)
                {
                    Complex[] fieldVec = MyMatrixUtil.matrix_GetRowVec(evecs, imode);
                    Complex phaseShift = 1.0;
                    double maxAbs = double.MinValue;
                    Complex fValueAtMaxAbs = 0.0;
                    {
                        // 領域全体で位相調整する
                        for (int ino = 0; ino < fieldVec.Length; ino++)
                        {
                            Complex cvalue = fieldVec[ino];
                            double abs = Complex.Norm(cvalue);
                            if (abs > maxAbs)
                            {
                                maxAbs = abs;
                                fValueAtMaxAbs = cvalue;
                            }
                        }
                    }
                    if (maxAbs >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                    {
                        phaseShift = fValueAtMaxAbs / maxAbs;
                    }
                    //System.Diagnostics.Debug.WriteLine("phaseShift: {0} (°)", Math.Atan2(phaseShift.Imag, phaseShift.Real) * 180.0 / pi);
                    for (int i = 0; i < fieldVec.Length; i++)
                    {
                        fieldVec[i] /= phaseShift;
                    }
                    MyMatrixUtil.matrix_setRowVec(evecs, imode, fieldVec);
                }
            }

            // モード数の修正
            if (max_mode > evals.Length)
            {
                max_mode = (uint)evals.Length;
            }
            // 固有値、固有ベクトルの格納先を確保する
            eigen_values = new Complex[max_mode];
            eigen_vecs = new Complex[max_mode, node_cnt];

            int tagtModeIdx = evals.Length - 1;
            for (uint imode = 0; imode < max_mode; imode++)
            {
                if (tagtModeIdx == -1)
                {
                    // fail safe
                    eigen_values[imode] = 0.0;
                    continue;
                }
                // 伝搬定数は固有値のsqrt
                Complex betam = MyMatrixUtil.complex_Sqrt(evals[tagtModeIdx]);
                // 定式化BUGFIX
                //   減衰定数は符号がマイナス(β = -jα)
                if (betam.Imag >= 0.0)
                {
                    betam = new Complex(betam.Real, -betam.Imag);
                }
                // 固有ベクトル
                Complex[] evec = MyMatrixUtil.matrix_GetRowVec(evecs, tagtModeIdx);
                // 規格化定数を求める
                Complex[] workVec = MyMatrixUtil.product(ryy_1d, evec);
                DelFEM4NetCom.Complex imagOne = new DelFEM4NetCom.Complex(0.0, 1.0);
                Complex dm = MyMatrixUtil.vector_Dot(MyMatrixUtil.vector_Conjugate(evec), workVec);
                if (waveModeDv == WaveModeDV.TM)
                {
                    // TMモード
                    dm = MyMatrixUtil.complex_Sqrt(omega * eps0 / Complex.Norm(betam) / dm);
                }
                else
                {
                    // TEモード
                    dm = MyMatrixUtil.complex_Sqrt(omega * myu0 / Complex.Norm(betam) / dm);
                }

                //System.Diagnostics.Debug.WriteLine("dm = " + dm);

                // 伝搬定数の格納
                eigen_values[imode] = betam;
                if (imode == 0 || betam.Real >= 1.0e-12)
                {
                    System.Diagnostics.Debug.WriteLine("    β/k0[ " + imode + "] = " + betam.Real / k0 + " + " + betam.Imag / k0 + " i " + " tagtModeIdx :" + tagtModeIdx);
                    // 理論値と比較
                    //Complex betamStrict = getRectangleWaveguideBeta(k0, imode, waveguideWidth, 1.0); // er = 1.0
                    //System.Diagnostics.Debug.WriteLine("    β/k0(strict)[ " + imode + "] = " + betamStrict.Real / k0 + " + " + betamStrict.Imag / k0 + " i " + " tagtModeIdx :" + tagtModeIdx);
                }
                // 固有ベクトルの格納(規格化定数を掛ける)
                for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                {
                    Complex fm = dm * evec[ino_boundary];
                    eigen_vecs[imode, ino_boundary] = fm;
                    //System.Diagnostics.Debug.WriteLine("eigen_vecs [ " + imode + ", " + ino_boundary + "] = " + fm.Real + " + " + fm.Imag + " i  Abs:" + Complex.Norm(fm));
                }

                tagtModeIdx--;
            }
            return true;
        }

        /// <summary>
        /// Lisys(Lapack)による固有値解析
        /// </summary>
        private static bool solveEigen(double[,] srcMat, out Complex[] evals, out Complex[,] evecs)
        {
            // Lisys(Lapack)による固有値解析
            double[] X = MyMatrixUtil.matrix_ToBuffer(srcMat);
            double[] r_evals = null;
            double[] i_evals = null;
            double[][] r_evecs = null;
            double[][] i_evecs = null;
            KrdLab.clapack.Function.dgeev(X, srcMat.GetLength(0), srcMat.GetLength(1), ref r_evals, ref i_evals, ref r_evecs, ref i_evecs);

            evals = new Complex[r_evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i] = new Complex(r_evals[i], i_evals[i]);
                //System.Diagnostics.Debug.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imag + " i ");
            }
            evecs = new Complex[r_evecs.Length, r_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    evecs[i, j] = new Complex(r_evecs[i][j], i_evecs[i][j]);
                    //System.Diagnostics.Debug.WriteLine("( " + i + ", " + j + " ) = " + evecs[i, j].Real + " + " + evecs[i, j].Imag + " i ");
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stiffnessMat"></param>
        /// <param name="massMat"></param>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        /// <returns></returns>
        private static bool solveGeneralizedEigen(double[,] stiffnessMat, double[,] massMat, out Complex[] evals, out Complex[,] evecs)
        {
            int matLen = stiffnessMat.GetLength(0);
            double[] A = MyMatrixUtil.matrix_ToBuffer(stiffnessMat);
            double[] B = MyMatrixUtil.matrix_ToBuffer(massMat);
            double[] r_evals = null;
            double[] i_evals = null;
            double[][] r_evecs = null;
            double[][] i_evecs = null;
            KrdLab.clapack.FunctionExt.dggev(A, matLen, matLen, B, matLen, matLen, ref r_evals, ref i_evals, ref r_evecs, ref i_evecs);

            evals = new Complex[r_evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i] = new Complex(r_evals[i], i_evals[i]);
                //System.Diagnostics.Debug.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imag + " i ");
            }
            evecs = new Complex[r_evecs.Length, r_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    evecs[i, j] = new Complex(r_evecs[i][j], i_evecs[i][j]);
                    //System.Diagnostics.Debug.WriteLine("( " + i + ", " + j + " ) = " + evecs[i, j].Real + " + " + evecs[i, j].Imag + " i ");
                }
            }
            return true;
        }

        /// <summary>
        /// 固有値のソート用クラス
        /// </summary>
        class EigenModeItem : IComparable<EigenModeItem>
        {
            public uint index;
            public Complex eval;
            public Complex[] evec;

            public EigenModeItem(uint index_, Complex eval_, Complex[] evec_)
            {
                index = index_;
                eval = eval_;
                evec = evec_;
            }
            int IComparable<EigenModeItem>.CompareTo(EigenModeItem other)
            {
                //BUG: 小数点以下の場合、比較できない
                //int cmp = (int)(this.eval.Real - other.eval.Real);
                double cmpf = this.eval.Real - other.eval.Real;
                int cmp = 0;
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
                return cmp;
            }

        }

        /// <summary>
        /// 固有モードをソートする(伝搬モードを先に)
        /// </summary>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        private static void sort1DEigenMode(Complex[] evals, Complex[,] evecs)
        {
            /*
            // 符号調整
            {
                // 正の固有値をカウント
                //int positive_cnt = 0;
                //foreach (Complex c in evals)
                //{
                //    if (c.Real > 0.0 && Math.Abs(c.Imaginary) <Constants.PrecisionLowerLimit)
                //    {
                //        positive_cnt++;
                //    }
                //}
                // 計算範囲では、正の固有値(伝搬定数の二乗が正)は高々1個のはず
                // 半分以上正の固有値であれば、固有値が逆転して計算されていると判断する
                // 誘電体導波路の場合、これでは破綻する？
                // 最大、最小値を求め、最大値 > 最小値の絶対値なら逆転している
                double minEval = double.MaxValue;
                double maxEval = double.MinValue;
                foreach (Complex c in evals)
                {
                    if (minEval > c.Real) { minEval = c.Real; }
                    if (maxEval < c.Real) { maxEval = c.Real; }
                }
                //if (positive_cnt >= evals.Length / 2)  // 導波管の場合
                if (Math.Abs(maxEval) > Math.Abs(minEval))
                {
                    for (int i = 0; i < evals.Length; i++)
                    {
                        evals[i] = -evals[i];
                    }
                    //System.Diagnostics.Debug.WriteLine("eval sign changed");
                }
            }
            */
            EigenModeItem[] items = new EigenModeItem[evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                Complex[] evec = MyMatrixUtil.matrix_GetRowVec(evecs, i);
                items[i] = new EigenModeItem((uint)i, evals[i], evec);
            }
            Array.Sort(items);

            uint imode = 0;
            foreach (EigenModeItem item in items)
            {
                evals[imode] = item.eval;
                MyMatrixUtil.matrix_setRowVec(evecs, (int)imode, item.evec);
                imode++;
            }

        }

        /// <summary>
        /// 1D固有値解析のFEM要素マトリクスを加算する
        ///     ryy_1d行列、境界上の節点座標の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="to_no_boundary">節点番号→境界上節点マップ</param>
        /// <param name="media">媒質情報</param>
        /// <param name="txx_1d">FEM[txx]行列</param>
        /// <param name="ryy_1d">FEM[ryy]行列</param>
        /// <param name="uzz_1d">FEM[uzz]行列</param>
        /// <param name="coord_c_all">節点座標配列</param>
        /// <returns></returns>
        protected static bool addElementMatOf1dEigenValueProblem(
            CFieldWorld world,
            uint fieldValId,
            uint eaId,
            uint[] no_c_all,
            Dictionary<uint, uint> to_no_boundary,
            MediaInfo media,
            ref double[,] txx_1d,
            ref double[,] ryy_1d,
            ref double[,] uzz_1d,
            ref double[,] coord_c_all)
        {
            uint node_cnt = (uint)no_c_all.Length;

            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            CField valField = world.GetField(fieldValId);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 線要素の節点数
            uint nno = 2;
            // 座標の次元
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の座標
            double[][] coord_c = new double[nno][];
            for (uint inoes = 0; inoes < nno; inoes++)
            {
                coord_c[inoes] = new double[ndim];
            }
            // 節点の値の節点セグメント
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
                        coord_c[inoes][i] = tmpval[i];
                    }
                }
                //MyMatrixUtil.printMatrix("coord_c", coord_c);

                // 座標チェック用
                for (uint ino = 0; ino < nno; ino++)
                {
                    System.Diagnostics.Debug.Assert(to_no_boundary.ContainsKey(no_c[ino]));
                    uint ino_boundary = to_no_boundary[no_c[ino]];
                    for (uint idim = 0; idim < ndim; idim++)
                    {
                        coord_c_all[ino_boundary, idim] = coord_c[ino][idim];
                    }
                }
                double[][] pp = coord_c;
                double elen = CVector2D.Distance2D(pp[0], pp[1]);
                double[,] integralN = new double[2, 2]
                {
                    { elen / 3.0, elen / 6.0 },
                    { elen / 6.0, elen / 3.0 },
                };
                double[,] integralDNDY = new double[2, 2]
                {
                    {  1.0 / elen, -1.0 / elen },
                    { -1.0 / elen,  1.0 / elen },
                };

                for (uint ino = 0; ino < nno; ino++)
                {
                    System.Diagnostics.Debug.Assert(to_no_boundary.ContainsKey(no_c[ino]));
                    uint ino_boundary = to_no_boundary[no_c[ino]];
                    System.Diagnostics.Debug.Assert(ino_boundary < node_cnt);
                    for (uint jno = 0; jno < nno; jno++)
                    {
                        System.Diagnostics.Debug.Assert(to_no_boundary.ContainsKey(no_c[jno]));
                        uint jno_boundary = to_no_boundary[no_c[jno]];
                        System.Diagnostics.Debug.Assert(jno_boundary < node_cnt);

                        txx_1d[ino_boundary, jno_boundary] += media.P[0, 0] * integralDNDY[ino, jno];
                        ryy_1d[ino_boundary, jno_boundary] += media.P[1, 1] * integralN[ino, jno];
                        uzz_1d[ino_boundary, jno_boundary] += media.Q[2, 2] * integralN[ino, jno];
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// 矩形導波管開口境界条件(要素アレイ単位)
        /// Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることが前提
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="isInputPort">入射ポート？</param>
        /// <param name="ryy_1d">FEM[ryy]行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列(i,j) i:固有値インデックス, j:節点インデックス</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="coord_c_all">節点座標配列</param>
        /// <param name="tmpBuffer">一時バッファ</param>
        /// <returns></returns>
        private static bool addLinSys_WaveguidePortBC_Core(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            CFieldWorld world,
            uint fieldValId,
            bool isInputPort,
            int incidentModeIndex,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs,
            uint[] no_c_all,
            double[,] coord_c_all,
            ref int[] tmpBuffer)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 / Math.Sqrt(myu0 * eps0);

            //System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            //CElemAry ea = world.GetEA(eaId);
            //System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            CField valField = world.GetField(fieldValId);
            //CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            //CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 境界上の節点数(1次線要素を想定)
            uint node_cnt = (uint)ryy_1d.GetLength(0);
            // 考慮するモード数
            uint max_mode = (uint)eigen_values.Length;

            // 全体剛性行列の作成
            Complex[,] mat_all = new Complex[node_cnt, node_cnt];
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                {
                    mat_all[ino_boundary, jno_boundary] = new Complex(0.0, 0.0);
                }
            }
            for (uint imode = 0; imode < max_mode; imode++)
            {
                Complex betam = eigen_values[imode];

                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                Complex[] vecj = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
                Complex imagOne = new Complex(0.0, 1.0);
                for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                {
                    for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                    {
                        Complex cvalue = 0.0;
                        if (waveModeDv == WaveModeDV.TM)
                        {
                            // TMモード
                            cvalue = (imagOne / (omega * eps0)) * betam * Complex.Norm(betam) * veci[ino_boundary] * vecj[jno_boundary];
                        }
                        else
                        {
                            // TEモード
                            cvalue = (imagOne / (omega * myu0)) * betam * Complex.Norm(betam) * veci[ino_boundary] * vecj[jno_boundary];
                        }
                        mat_all[ino_boundary, jno_boundary] += cvalue;
                    }
                }
            }
            // check 対称行列
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                for (uint jno_boundary = ino_boundary; jno_boundary < node_cnt; jno_boundary++)
                {
                    if (Math.Abs(mat_all[ino_boundary, jno_boundary].Real - mat_all[jno_boundary, ino_boundary].Real) >= 1.0e-12)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    if (Math.Abs(mat_all[ino_boundary, jno_boundary].Imag - mat_all[jno_boundary, ino_boundary].Imag) >= 1.0e-12)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
            }
            //MyMatrixUtil.printMatrix("emat_all", emat_all);

            // 残差ベクトルの作成
            Complex[] res_c_all = new Complex[node_cnt];
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                res_c_all[ino_boundary] = 0.0;
            }
            if (isInputPort)
            {
                //uint imode = 0;
                uint imode = (uint)incidentModeIndex;
                System.Diagnostics.Debug.Assert(imode < eigen_values.Length);
                Complex betam = eigen_values[imode];
                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                Complex imagOne = new Complex(0.0, 1.0);
                for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                {
                    // TEモード、TMモード共通
                    res_c_all[ino_boundary] = 2.0 * imagOne * betam * veci[ino_boundary];
                }
            }
            //MyMatrixUtil.printVec("eres_c_all", eres_c_all);

            // 線要素の節点数
            uint nno = 2;
            // 座標の次元
            //uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素剛性行列(コーナ-コーナー)
            CZMatDia_BlkCrs_Ptr mat_cc = ls.GetMatrixPtr(fieldValId, ELSEG_TYPE.CORNER, world);
            // 要素残差ベクトル(コーナー)
            CZVector_Blk_Ptr res_c = ls.GetResidualPtr(fieldValId, ELSEG_TYPE.CORNER, world);

            //System.Diagnostics.Debug.WriteLine("fieldValId: {0}", fieldValId);
            //System.Diagnostics.Debug.WriteLine("NBlkMatCol:" + mat_cc.NBlkMatCol()); // （境界でなく領域の総節点数と同じ?)

            // 要素剛性行列にマージ
            //   この定式化では行列のスパース性は失われている(隣接していない要素の節点間にも関連がある)
            // 要素剛性行列にマージする
            bool[,] add_flg = new bool[node_cnt, node_cnt];
            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = 0; j < node_cnt; j++)
                {
                    add_flg[i, j] = false;
                }
            }
            // このケースではmat_ccへのマージは対角行列でマージしなければならないようです。
            // 1 x node_cntの横ベクトルでマージしようとするとassertに引っかかります。
            // 境界上の節点に関しては非０要素はないので、境界上の節点に関する
            //  node_cnt x node_cntの行列を一括でマージできます。
            // col, rowの全体節点番号ベクトル
            uint[] no_c_tmp = new uint[node_cnt];
            // 要素行列(ここでは境界の剛性行列を一括でマージします)
            Complex[] emattmp = new Complex[node_cnt * node_cnt];
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                // colブロックのインデックス(全体行列の節点番号)
                uint iblk = no_c_all[ino_boundary];
                uint npsup = 0;
                ConstUIntArrayIndexer cur_rows = mat_cc.GetPtrIndPSuP((uint)iblk, out npsup);
                //System.Diagnostics.Debug.WriteLine("chk3:{0} {1}", iblk, npsup);
                for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                {
                    if (ino_boundary != jno_boundary)
                    {
                        uint rowno = no_c_all[jno_boundary];
                        if (cur_rows.IndexOf(rowno) == -1)
                        {
                            System.Diagnostics.Debug.Assert(false);
                            return false;
                        }
                    }
                    if (!add_flg[ino_boundary, jno_boundary])
                    {
                        // 要素行列を作成
                        Complex cvalue = mat_all[ino_boundary, jno_boundary];
                        //emattmp[ino_boundary, jno_boundary]
                        emattmp[ino_boundary * node_cnt + jno_boundary] = cvalue;
                        add_flg[ino_boundary, jno_boundary] = true;
                    }
                    else
                    {
                        // ここにはこない
                        System.Diagnostics.Debug.Assert(false);
                        //emattmp[ino_boundary, jno_boundary]
                        emattmp[ino_boundary * node_cnt + jno_boundary] = new Complex(0, 0);
                    }
                }
                no_c_tmp[ino_boundary] = iblk;
            }
            // 一括マージ
            mat_cc.Mearge(node_cnt, no_c_tmp, node_cnt, no_c_tmp, 1, emattmp, ref tmpBuffer);

            for (int i = 0; i < node_cnt; i++)
            {
                for (int j = 0; j < node_cnt; j++)
                {
                    //System.Diagnostics.Debug.WriteLine( i + " " + j + " " + add_flg[i, j] );
                    System.Diagnostics.Debug.Assert(add_flg[i, j]);
                }
            }

            // 残差ベクトルにマージ
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                // 残差ベクトルにマージする
                uint no_tmp = no_c_all[ino_boundary];
                Complex val = res_c_all[ino_boundary];

                res_c.AddValue(no_tmp, 0, val);
            }

            return true;
        }

        /// <summary>
        /// 導波管 構造情報取得
        ///     Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることが前提です。
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="coord_c_first">始点座標</param>
        /// <param name="coord_c_last">終点座標</param>
        /// <param name="waveguideWidth">導波管の幅</param>
        /// <returns></returns>
        protected static bool getRectangleWaveguideStructInfo(CFieldWorld world, uint fieldValId, out double[] coord_c_first, out double[] coord_c_last, out double waveguideWidth)
        {
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                coord_c_first = null;
                coord_c_last = null;
                waveguideWidth = 0;
                return false;
            }

            IList<uint> aIdEA = valField.GetAryIdEA();
            coord_c_first = null;
            coord_c_last = null;
            waveguideWidth = 0;

            for (int i = 0; i < aIdEA.Count; i++)
            {
                uint eaId = aIdEA[i];
                double[] tmp_coord_c_first = null;
                double[] tmp_coord_c_last = null;
                double tmpWaveguideWidth = 0;
                bool res = getRectangleWaveguideStructInfo_EachElementAry(world, fieldValId, eaId, out tmp_coord_c_first, out tmp_coord_c_last, out tmpWaveguideWidth);
                if (!res)
                {
                    return false;
                }
                if (i == 0)
                {
                    coord_c_first = tmp_coord_c_first;
                }
                if (i == aIdEA.Count - 1)
                {
                    coord_c_last = tmp_coord_c_last;
                }
                waveguideWidth += tmpWaveguideWidth;
            }
            System.Diagnostics.Debug.WriteLine("    waveguideWidth = " + waveguideWidth);
            return true;
        }

        /// <summary>
        /// 導波管 構造情報取得(要素アレイ単位の処理)
        ///     Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることが前提です。
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="coord_c_first">始点座標</param>
        /// <param name="coord_c_last">終点座標</param>
        /// <param name="waveguideWidth">導波管の幅</param>
        /// <returns></returns>
        private static bool getRectangleWaveguideStructInfo_EachElementAry(CFieldWorld world, uint fieldValId, uint eaId, out double[] coord_c_first, out double[] coord_c_last, out double waveguideWidth)
        {
            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (!world.IsIdField(fieldValId))
            {
                coord_c_first = null;
                coord_c_last = null;
                waveguideWidth = 0;
                return false;
            }
            CField valField = world.GetField(fieldValId);
            //CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 線要素の節点数
            uint nno = 2;
            // 座標の次元
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の座標
            //double[][] coord_c = new double[nno][];
            //for (uint inoes = 0; inoes < nno; inoes++)
            //{
            //    coord_c[inoes] = new double[ndim];
            //}
            // 節点の値の節点セグメント
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            // 取得する情報初期化
            coord_c_first = new double[ndim];
            coord_c_last = new double[ndim];
            for (int i = 0; i < ndim; i++)
            {
                coord_c_first[i] = 0.0;
                coord_c_last[i] = 0.0;
            }
            waveguideWidth = 0.0;

            // 最初の節点の座標
            {
                uint ielem = 0;
                es_c_co.GetNodes(ielem, no_c);
                {
                    uint inoes = 0;
                    double[] tmpval = null;
                    ns_c_co.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == ndim);
                    for (int i = 0; i < tmpval.Length; i++)
                    {
                        coord_c_first[i] = tmpval[i];
                    }
                }
            }
            // 最後の節点の座標
            {
                uint ielem = ea.Size() - 1;
                es_c_co.GetNodes(ielem, no_c);
                {
                    uint inoes = nno - 1;
                    double[] tmpval = null;
                    ns_c_co.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == ndim);
                    for (int i = 0; i < tmpval.Length; i++)
                    {
                        coord_c_last[i] = tmpval[i];
                    }
                }
            }
            waveguideWidth = CVector2D.Distance2D(coord_c_last, coord_c_first);

            return true;
        }

        /// <summary>
        /// 矩形導波管 伝搬定数(b -ja)理論値
        /// </summary>
        /// <param name="k0">波数</param>
        /// <param name="imode">モード次数(TEimode,0)</param>
        /// <param name="waveguideWidth">導波管の幅</param>
        /// <param name="q">比誘電率</param>
        /// <returns></returns>
        private static Complex getRectangleWaveguideBeta(double k0, uint imode, double waveguideWidth, double q)
        {
            Complex betam = new Complex();
            uint m = imode + 1;
            double squareBeta = k0 * k0 * q - (m * pi / waveguideWidth) * (m * pi / waveguideWidth);
            if (squareBeta > 0)
            {
                betam = Math.Sqrt(squareBeta);
            }
            else
            {
                betam = new Complex(0.0, -1.0) * Math.Sqrt(-squareBeta);
            }
            return betam;
        }
        /// <summary>
        /// 矩形導波管 モード関数値(Ez)理論値
        /// </summary>
        /// <param name="k0">波数</param>
        /// <param name="imode">モード次数</param>
        /// <param name="waveguideWidth">導波管の幅</param>
        /// <param name="betam">モードの伝搬定数</param>
        /// <param name="y">座標</param>
        /// <returns></returns>
        private Complex getRectangleWaveguideModalFuncValue(double k0, uint imode, double waveguideWidth, Complex betam, double y)
        {
            double omega = k0 * c0;
            Complex fm = new Complex();
            if (Complex.Norm(betam) >= 1.0e-10)
            {
                double d = Math.Sqrt(2.0 * omega * myu0 / (waveguideWidth * Complex.Norm(betam)));
                fm = d * Math.Sin((imode + 1) * pi * y / waveguideWidth);
            }
            return fm;
        }

        /// <summary>
        /// 矩形導波管開口反射（透過)係数
        ///    ※要素アレイは1つに限定しています。
        ///     ポート境界を指定するときに複数の辺をリストで境界条件設定することでこの条件をクリアできます。
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="fieldValId">フィールド値のID</param>
        /// <param name="imode">固有モードのモード次数</param>
        /// <param name="isIncidentMode">入射モード？</param>
        /// <param name="ryy_1d">[ryy]FEM行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列(i, j)i:固有値インデックス, j:節点</param>
        public static Complex GetWaveguidePortReflectionCoef(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            CFieldWorld world,
            uint fieldValId,
            uint imode,
            bool isIncidentMode,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs)
        {
            Complex s11 = new Complex(0.0, 0.0);
            if (ryy_1d == null || eigen_values == null || eigen_vecs == null)
            {
                return s11;
            }
            if (!world.IsIdField(fieldValId))
            {
                return s11;
            }
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return s11;
            }

            bool res;

            //境界節点番号→全体節点番号変換テーブル(no_c_all)
            uint[] no_c_all = null;
            // 全体節点番号→境界節点番号変換テーブル(to_no_boundary
            Dictionary<uint, uint> to_no_boundary = null;

            // 境界上のすべての節点番号を取り出す
            res = GetBoundaryNodeList(world, fieldValId, out no_c_all, out to_no_boundary);
            if (!res)
            {
                return s11;
            }

            // 境界上のすべての節点の界の値を取り出す
            Complex[] value_c_all = null;
            res = GetBoundaryFieldValueList(world, fieldValId, no_c_all, to_no_boundary, out value_c_all);
            if (!res)
            {
                return s11;
            }

            uint node_cnt = (uint)no_c_all.Length;
            System.Diagnostics.Debug.Assert(node_cnt == ryy_1d.GetLength(0));
            System.Diagnostics.Debug.Assert(node_cnt == ryy_1d.GetLength(1));
            System.Diagnostics.Debug.Assert(node_cnt == eigen_vecs.GetLength(1));
            s11 = getWaveguidePortReflectionCoef_Core(
                waveLength,
                waveModeDv,
                imode,
                isIncidentMode,
                no_c_all,
                to_no_boundary,
                value_c_all,
                ryy_1d,
                eigen_values,
                eigen_vecs);
            return s11;
        }

        /// <summary>
        /// 境界上の界の値を取得する
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="to_no_boundary">節点番号→境界上節点マップ</param>
        /// <param name="value_c_all">フィールド値の配列</param>
        /// <returns></returns>
        protected static bool GetBoundaryFieldValueList(CFieldWorld world, uint fieldValId, uint[] no_c_all, Dictionary<uint, uint> to_no_boundary, out Complex[] value_c_all)
        {
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                value_c_all = null;
                return false;
            }

            IList<uint> aIdEA = valField.GetAryIdEA();

            uint node_cnt = (uint)no_c_all.Length;

            // 境界上の界の値をすべて取得する
            value_c_all = new Complex[node_cnt];

            foreach (uint eaId in aIdEA)
            {
                bool res = getBoundaryFieldValueList_EachElementAry(world, fieldValId, no_c_all, to_no_boundary, eaId, value_c_all);
            }

            return true;
        }

        /// <summary>
        /// 境界上の界の値を取得する(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="to_no_boundary">節点番号→境界上節点番号マップ</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="value_c_all">フィールド値配列</param>
        /// <returns></returns>
        private static bool getBoundaryFieldValueList_EachElementAry(CFieldWorld world, uint fieldValId, uint[] no_c_all, Dictionary<uint, uint> to_no_boundary, uint eaId, Complex[] value_c_all)
        {
            uint node_cnt = (uint)no_c_all.Length;

            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);

            CField valField = world.GetField(fieldValId);
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 線要素の節点数
            uint nno = 2;
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
                    System.Diagnostics.Debug.Assert(to_no_boundary.ContainsKey(no_c[ino]));
                    uint ino_boundary = to_no_boundary[no_c[ino]];
                    System.Diagnostics.Debug.Assert(ino_boundary < node_cnt);

                    value_c_all[ino_boundary] = value_c[ino];
                    //System.Diagnostics.Debug.WriteLine("value_c_all [ " + ino_boundary + " ] = " + "(" + value_c_all[ino_boundary].Real + ", " + value_c_all[ino_boundary].Imag + ") " + Complex.Norm(value_c_all[ino_boundary]));
                }
            }
            return true;
        }

        /// <summary>
        /// 矩形導波管開口反射（透過)係数(要素アレイ単位)
        ///   Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることが前提です。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="imode">モード次数</param>
        /// <param name="isIncidentMode">入射ポート？</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="to_no_boundary">節点番号→境界節点番号マップ</param>
        /// <param name="value_c_all">フィールド値配列</param>
        /// <param name="ryy_1d">FEM[ryy]行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有値行列(i,j) i:固有値インデックス j:節点</param>
        /// <returns></returns>
        private static Complex getWaveguidePortReflectionCoef_Core(
            double waveLength,
            WaveModeDV waveModeDv,
            uint imode,
            bool isIncidentMode,
            uint[] no_c_all, Dictionary<uint, uint> to_no_boundary,
            Complex[] value_c_all,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 * c0;

            Complex s11 = new Complex(0.0, 0.0);
            //uint node_cnt = (uint)ryy_1d.GetLength(0);
            //uint max_mode = (uint)eigen_values.Length;
            Complex betam = eigen_values[imode];
            Complex imagOne = new Complex(0.0, 1.0);

            // {tmp_vec}*t = {fm}*t[ryy]*t
            // {tmp_vec}* = [ryy]* {fm}*
            //   ([ryy]*)t = [ryy]*
            //    [ryy]が実数のときは、[ryy]* -->[ryy]
            Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
            Complex[] fmVec_Modify = new Complex[fmVec.Length];
            //Complex[] tmp_vec = MyMatrixUtil.product(MyMatrixUtil.matrix_ConjugateTranspose(ryy_1d), MyMatrixUtil.vector_Conjugate(fmVec));
            // ryyが実数のとき
            Complex[] tmp_vec = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
            // s11 = {tmp_vec}t {value_all}
            s11 = MyMatrixUtil.vector_Dot(tmp_vec, value_c_all);

            if (waveModeDv == WaveModeDV.TM)
            {
                // TMモード
                s11 *= (Complex.Norm(betam) / (omega * eps0));
                if (isIncidentMode)
                {
                    s11 += -1.0;
                }
            }
            else
            {
                // TEモード
                s11 *= (Complex.Norm(betam) / (omega * myu0));
                if (isIncidentMode)
                {
                    s11 += -1.0;
                }
            }
            return s11;
        }

        /// <summary>
        /// 表示用に界の値を置き換える(複素数を複素数絶対値にする)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="showFieldDv">表示する界の値の区分 0: 絶対値 1: 虚数部 (Note: 実数部表示はこの置き換えを行わなくてもデフォルトで表示される)</param>
        public static void ReplaceFieldValueForDisplay(CFieldWorld world, uint fieldValId, int showFieldDv = 0)
        {
            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            System.Diagnostics.Debug.Assert(valField.GetFieldType() == FIELD_TYPE.ZSCALAR);

            // 要素アレイIDのリストを取得
            IList<uint> aIdEA = valField.GetAryIdEA();
            // 要素アレイIDのリストを走査
            foreach (uint eaId in aIdEA)
            {
                // 要素アレイを取得
                CElemAry ea = world.GetEA(eaId);
                // 補間タイプをチェック
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    // 三角形要素の場合

                    // フィールド値に対する要素セグメントを取得
                    CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
                    // 座標に対する要素セグメントを取得
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
                    // フィールド値の節点セグメントを取得
                    CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
                    // 座標の節点セグメントを取得
                    //CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);
                    // 要素アレイを走査
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
                        }
                        // 節点の値を加工して再設定する
                        for (uint inoes = 0; inoes < nno; inoes++)
                        {
                            Complex val = value_c[inoes];
                            //ns_c_val.SetValue(no_c[inoes], 0, val.Real);
                            //ns_c_val.SetValue(no_c[inoes], 1, val.Imag);
                            if (showFieldDv == 1)
                            {
                                // 節点の値の虚数部を表示する
                                ns_c_val.SetValue(no_c[inoes], 0, val.Imag);
                                ns_c_val.SetValue(no_c[inoes], 1, 0.0);
                            }
                            else
                            {
                                // 節点の値を絶対値にする
                                double valAbs = Complex.Norm(val);
                                // 複素数として格納ているので、自由度は２。
                                ns_c_val.SetValue(no_c[inoes], 0, valAbs);
                                ns_c_val.SetValue(no_c[inoes], 1, 0.0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 表示用に界の値を置き換える(外部データ)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        public static void SetFieldValueForDisplay(CFieldWorld world, uint fieldValId, Complex[] values_all, Dictionary<uint, uint> toNodeIndex)
        {
            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            System.Diagnostics.Debug.Assert(valField.GetFieldType() == FIELD_TYPE.ZSCALAR);

            // 要素アレイIDのリストを取得
            IList<uint> aIdEA = valField.GetAryIdEA();
            // 要素アレイIDのリストを走査
            foreach (uint eaId in aIdEA)
            {
                // 要素アレイを取得
                CElemAry ea = world.GetEA(eaId);
                // 補間タイプをチェック
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    // 三角形要素の場合

                    // 座標に対する要素セグメントを取得
                    CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                    // 三角形要素の節点数
                    uint nno = 3;
                    // 要素節点の全体節点番号
                    uint[] no_c = new uint[nno];
                    // フィールド値の節点セグメントを取得
                    CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
                    // 要素アレイを走査
                    for (uint ielem = 0; ielem < ea.Size(); ielem++)
                    {
                        // 要素配列から要素セグメントの節点番号を取り出す
                        es_c_co.GetNodes(ielem, no_c);
                        // 節点の値を絶対値にする
                        for (uint inoes = 0; inoes < nno; inoes++)
                        {
                            uint nodeNumber = no_c[inoes];
                            if (!toNodeIndex.ContainsKey(nodeNumber)) continue;
                            uint nodeIndex = toNodeIndex[nodeNumber];
                            Complex val = values_all[nodeIndex];
                            ns_c_val.SetValue(nodeNumber, 0, val.Real);
                            ns_c_val.SetValue(nodeNumber, 1, val.Imag);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GCを実行する
        /// </summary>
        public static void GC_Collect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TotalMemory: {0}", GC.GetTotalMemory(false));
                // GC.Collect 呼び出し後に GC.WaitForPendingFinalizers を呼び出します。これにより、すべてのオブジェクトに対するファイナライザが呼び出されるまで、現在のスレッドは待機します。
                // ファイナライザ作動後は、回収すべき、(ファイナライズされたばかりの) アクセス不可能なオブジェクトが増えます。もう1度 GC.Collect を呼び出し、それらを回収します。
                GC.Collect(); // アクセス不可能なオブジェクトを除去
                GC.WaitForPendingFinalizers(); // ファイナライゼーションが終わるまでスレッド待機
                GC.Collect(0); // ファイナライズされたばかりのオブジェクトに関連するメモリを開放
                System.Diagnostics.Debug.WriteLine("TotalMemory: {0}", GC.GetTotalMemory(false));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// ループ内の節点番号と座標の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上の節点番号のマップ</param>
        /// <param name="coord_c_all">座標リスト</param>
        /// <returns></returns>
        public static bool GetLoopCoordList(
            CFieldWorld world, uint fieldValId,
            out uint[] no_c_all, out Dictionary<uint, uint> to_no_loop, out double[][] coord_c_all)
        {
            return GetLoopCoordList(
                world, fieldValId,
                0.0, null,
                out no_c_all, out to_no_loop, out coord_c_all);
        }

        /// <summary>
        /// ループ内の節点番号と座標の取得
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="no_c_all">全体節点番号配列</param>
        /// <param name="to_no_boundary">全体節点番号→境界上の節点番号のマップ</param>
        /// <param name="coord_c_all">座標リスト</param>
        /// <returns></returns>
        public static bool GetLoopCoordList(CFieldWorld world, uint fieldValId,
            double rotAngle, double[] rotOrigin,
            out uint[] no_c_all, out Dictionary<uint, uint> to_no_loop, out double[][] coord_c_all)
        {
            no_c_all = null;
            to_no_loop = null;
            coord_c_all = null;

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            if (valField.GetFieldType() != FIELD_TYPE.ZSCALAR)
            {
                return false;
            }
            IList<uint> aIdEA = valField.GetAryIdEA();

            // 全体節点番号→ループ内節点番号変換テーブル作成
            to_no_loop = new Dictionary<uint, uint>();
            IList<double[]> coord_c_list = new List<double[]>();

            foreach (uint eaId in aIdEA)
            {
                bool res = getLoopCoordList_EachElementAry(
                    world, fieldValId, eaId,
                    rotAngle, rotOrigin,
                    ref to_no_loop, ref coord_c_list);
                if (!res)
                {
                    return false;
                }
            }
            //境界節点番号→全体節点番号変換テーブル(no_c_all)作成 
            no_c_all = new uint[to_no_loop.Count];
            foreach (KeyValuePair<uint, uint> kvp in to_no_loop)
            {
                uint ino_boundary = kvp.Value;
                uint ino = kvp.Key;
                no_c_all[ino_boundary] = ino;
            }
            coord_c_all = coord_c_list.ToArray();

            return true;
        }

        /// <summary>
        /// ループ内の節点番号と座標の取得(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="to_no_boundary">全体節点番号→境界上節点番号マップ</param>
        /// <returns></returns>
        private static bool getLoopCoordList_EachElementAry(
            CFieldWorld world, uint fieldValId, uint eaId,
            double rotAngle, double[] rotOrigin,
            ref Dictionary<uint, uint> to_no_loop, ref IList<double[]> coord_c_list)
        {
            // 要素アレイを取得する
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.TRI);
            if (ea.ElemType() != ELEM_TYPE.TRI)
            {
                return false;
            }

            // フィールドを取得
            CField valField = world.GetField(fieldValId);
            // 座標セグメントを取得
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
            // 座標のノードセグメントを取得
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            uint node_cnt = ea.Size() + 1;  // 全節点数

            // 三角形要素の節点数
            uint nno = 3;
            // 座標の次元
            uint ndim = 2;
            // 要素節点の全体節点番号
            uint[] no_c = new uint[nno];
            // 要素節点の座標
            double[][] coord_c = new double[nno][];
            for (int inoes = 0; inoes < nno; inoes++)
            {
                coord_c[inoes] = new double[ndim];
            }

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
                        coord_c[inoes][i] = tmpval[i];
                    }
                }
                if (Math.Abs(rotAngle) >= Constants.PrecisionLowerLimit)
                {
                    // 座標を回転移動する
                    for (uint inoes = 0; inoes < nno; inoes++)
                    {
                        double[] srcPt = coord_c[inoes];
                        double[] destPt = GetRotCoord(srcPt, rotAngle, rotOrigin);
                        for (int i = 0; i < ndim; i++)
                        {
                            coord_c[inoes][i] = destPt[i];
                        }
                    }
                }

                for (uint ino = 0; ino < nno; ino++)
                {
                    if (!to_no_loop.ContainsKey(no_c[ino]))
                    {
                        uint ino_loop_tmp = (uint)to_no_loop.Count;
                        to_no_loop[no_c[ino]] = ino_loop_tmp;
                        coord_c_list.Add(new double[] { coord_c[ino][0], coord_c[ino][1] });
                        System.Diagnostics.Debug.Assert(coord_c_list.Count == (ino_loop_tmp + 1));
                    }
                }

            }
            return true;
        }


        /// <summary>
        /// 領域（ループのリスト）の部分フィールドを取得する
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="World"></param>
        /// <param name="loopId_cad_list"></param>
        /// <param name="mediaIndex_list"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="LoopDic"></param>
        public static void GetPartialField_Loop(
            CIDConvEAMshCad conv,
            CFieldWorld World,
            uint[] loopId_cad_list,
            int[] mediaIndex_list,
            uint FieldValId,
            out uint FieldLoopId,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic)
        {
            // 要素アレイのリスト
            IList<uint> aEA = new List<uint>();
            for (int i = 0; i < loopId_cad_list.Length; i++)
            {
                uint loopId_cad = loopId_cad_list[i];
                int mediaIndex = mediaIndex_list[i];
                // ワールド座標系のループIDを取得
                uint lId = conv.GetIdEA_fromCad(loopId_cad, CAD_ELEM_TYPE.LOOP);
                aEA.Add(lId);
                {
                    wg2d.World.Loop loop = new wg2d.World.Loop();
                    loop.Set(lId, mediaIndex);
                    LoopDic.Add(lId, loop);
                }
                //System.Diagnostics.Debug.WriteLine("lId:" + lId);
            }
            FieldLoopId = World.GetPartialField(FieldValId, aEA);
        }

        /// <summary>
        /// 境界（辺のリスト）の部分フィールドを取得する
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="World"></param>
        /// <param name="eId_cad_list"></param>
        /// <param name="mediaIndex_list"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldEdgeId"></param>
        /// <param name="EdgeDic"></param>
        public static void GetPartialField_Edge(
            CIDConvEAMshCad conv,
            CFieldWorld World,
            uint[] eId_cad_list,
            int[] mediaIndex_list,
            uint FieldValId,
            out uint FieldEdgeId,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic)
        {
            // 要素アレイのリスト
            IList<uint> aEA = new List<uint>();
            for (int i = 0; i < eId_cad_list.Length; i++)
            {
                uint eId_cad = eId_cad_list[i];
                int mediaIndex = mediaIndex_list != null ? mediaIndex_list[i] : -1;
                uint eId = conv.GetIdEA_fromCad(eId_cad, CAD_ELEM_TYPE.EDGE);
                aEA.Add(eId);
                if (EdgeDic != null)
                {
                    wg2d.World.Edge edge = new wg2d.World.Edge();
                    edge.Set(eId, mediaIndex);
                    EdgeDic.Add(eId, edge);
                }
            }
            FieldEdgeId = World.GetPartialField(FieldValId, aEA);
            CFieldValueSetter.SetFieldValue_Constant(FieldEdgeId, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0); // 境界の界を0で設定
        }
    }
}
