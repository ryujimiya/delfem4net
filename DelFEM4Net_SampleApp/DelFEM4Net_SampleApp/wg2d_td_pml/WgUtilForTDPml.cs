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
    //  時間領域導波路 2D PML
    //
    //      Copyright (C) 2012-2013 ryujimiya
    //
    //      e-mail: ryujimiya@mail.goo.ne.jp
    ///////////////////////////////////////////////////////////////
    /// <summary>
    /// 時間領域導波路解析のユーティリティ  PML
    /// </summary>
    class WgUtilForTDPml : WgUtilForTD
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ヘルムホルツの方程式 PML要素の全体行列の追加
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="pmlStPosX">PML媒質開始位置X座標</param>
        /// <param name="pmlLength">PML長さ</param>
        /// <param name="dt">時刻刻み幅</param>
        /// <param name="newmarkBeta">Newmark法 β</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="loopDic">ループID→ループ情報マップ</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="AMat">全体行列</param>
        /// <returns></returns>
        public static bool AddPmlMat(
            CFieldWorld world,
            uint fieldValId,
            bool isPmlYDirection,
            double pmlStPosX,
            double pmlLength,
            double dt,
            double newmarkBeta,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Loop> loopDic,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            ref MyDoubleBandMatrix AMat)
        {
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
                    bool res = addPmlMat_EachElementAry(
                        world,
                        fieldValId,
                        isPmlYDirection,
                        pmlStPosX,
                        pmlLength,
                        dt,
                        newmarkBeta,
                        media,
                        eaId,
                        node_cnt,
                        free_node_cnt,
                        toSorted,
                        ref AMat
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
        /// ヘルムホルツの方程式 PML要素の全体行列の追加(要素アレイ単位)
        /// </summary>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">値のフィールドID</param>
        /// <param name="pmlStPosX">PML媒質開始位置X座標</param>
        /// <param name="pmlLength">PML長さ</param>
        /// <param name="dt">時刻刻み幅</param>
        /// <param name="newmarkBeta">Newmark法β</param>
        /// <param name="media">媒質</param>
        /// <param name="eaId">要素アレイID</param>
        /// <param name="node_cnt">解析領域全体の節点数</param>
        /// <param name="free_node_cnt">解析領域全体の自由節点数（強制境界を除いた節点数)</param>
        /// <param name="toSorted">節点番号→ソート済み（強制境界を除いた)節点リストのインデックスマップ</param>
        /// <param name="AMat">全体行列</param>
        /// <returns></returns>
        private static bool addPmlMat_EachElementAry(
            CFieldWorld world,
            uint fieldValId,
            bool isPmlYDirection,
            double pmlStPosX,
            double pmlLength,
            double dt,
            double newmarkBeta,
            MediaInfo media,
            uint eaId,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            ref MyDoubleBandMatrix AMat)
        {
            System.Diagnostics.Debug.Assert(free_node_cnt == AMat.RowSize && free_node_cnt == AMat.ColumnSize);
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
            // 要素行列
            double[,] eMMat = new double[nno, nno];
            double[,] eKXMat = new double[nno, nno];
            double[,] eKYMat = new double[nno, nno];

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
                        eKXMat[ino, jno] = media.P[1, 1] * integralDLDX[ino, jno];
                        eKYMat[ino, jno] = media.P[0, 0] * integralDLDY[ino, jno];
                        // 要素質量行列
                        eMMat[ino, jno] = eps0 * myu0 * media.Q[2, 2] * integralL[ino, jno];
                    }
                }

                // PML媒質パラメータ
                double posXG = (p1[0] + p2[0] + p3[0]) / 3.0;
                if (isPmlYDirection)
                {
                    posXG = (p1[1] + p2[1] + p3[1]) / 3.0;
                }
                double sigmaX_e = 0.0;
                double c1_F_e = 0.0;
                double c1_G_e = 0.0;
                double c3_G_e = 0.0;
                GetPmlParameter(
                    posXG,
                    pmlStPosX,
                    pmlLength,
                    dt,
                    out sigmaX_e,
                    out c1_F_e,
                    out c1_G_e,
                    out c3_G_e);
                
                // 要素剛性行列にマージする
                double[,] work_eKXMat = eKXMat;
                double[,] work_eKYMat = eKYMat;
                if (isPmlYDirection)
                {
                    work_eKXMat = eKYMat;
                    work_eKYMat = eKXMat;
                }
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

                        // clapack形式の行列格納方法で格納(バンド行列)
                        AMat._body[AMat.GetBufferIndex(inoGlobal, jnoGlobal)] +=
                            (sigmaX_e / eps0) * (1.0 / (2.0 * dt)) * eMMat[ino, jno]
                            + c1_F_e * newmarkBeta * work_eKYMat[ino, jno]
                            + c1_G_e * newmarkBeta * work_eKXMat[ino, jno];
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// PML媒質内の要素の残差ベクトルを加算する
        /// </summary>
        /// <param name="timeIndex"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <param name="pmlStPosX"></param>
        /// <param name="pmlLength"></param>
        /// <param name="dt"></param>
        /// <param name="newmarkBeta"></param>
        /// <param name="medias"></param>
        /// <param name="loopDic"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="toSorted"></param>
        /// <param name="Ez_Prev"></param>
        /// <param name="Ez_Prev2"></param>
        /// <param name="W2_F_e_List"></param>
        /// <param name="W2_G_e_List"></param>
        /// <param name="resVec"></param>
        /// <returns></returns>
        public static bool AddPmlResVec(
            int timeIndex,
            CFieldWorld world,
            uint fieldValId,
            bool isPmlYDirection,
            double pmlStPosX,
            double pmlLength,
            double dt,
            double newmarkBeta,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Loop> loopDic,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            double[] Ez_Prev,
            double[] Ez_Prev2,
            ref double[][][] W2_F_e_List,
            ref double[][][] W2_G_e_List,
            ref double[] resVec)
        {
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

            // 要素アレイのリストを取得する
            IList<uint> aIdEA = valField.GetAryIdEA();
            // 要素毎のv, Φの値の初期化
            if (timeIndex == 0)
            {
                W2_F_e_List = new double[aIdEA.Count][][];
                W2_G_e_List = new double[aIdEA.Count][][];
            }
            for(int iElemAry = 0; iElemAry < aIdEA.Count; iElemAry++)
            {
                uint eaId = aIdEA[iElemAry];
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
                    double[][] w2_F_e_EA = W2_F_e_List[iElemAry];
                    double[][] w2_G_e_EA = W2_G_e_List[iElemAry];
                    bool res = addPmlResVec_EachElementAry(
                        timeIndex,
                        world,
                        fieldValId,
                        isPmlYDirection,
                        pmlStPosX,
                        pmlLength,
                        dt,
                        newmarkBeta,
                        media,
                        eaId,
                        node_cnt,
                        free_node_cnt,
                        toSorted,
                        Ez_Prev,
                        Ez_Prev2,
                        ref w2_F_e_EA,
                        ref w2_G_e_EA,
                        ref resVec
                        );
                    // 再格納
                    W2_F_e_List[iElemAry] = w2_F_e_EA;
                    W2_G_e_List[iElemAry] = w2_G_e_EA;
                    if (!res)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// PML媒質内の要素の残差ベクトルを加算する(要素アレイ単位)
        /// </summary>
        /// <param name="timeIndex"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <param name="pmlStPosX"></param>
        /// <param name="pmlLength"></param>
        /// <param name="dt"></param>
        /// <param name="newmarkBeta"></param>
        /// <param name="media"></param>
        /// <param name="eaId"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="toSorted"></param>
        /// <param name="Ez_Prev"></param>
        /// <param name="Ez_Prev2"></param>
        /// <param name="w2_F_e_EA"></param>
        /// <param name="w2_G_e_EA"></param>
        /// <param name="resVec"></param>
        /// <returns></returns>
        private static bool addPmlResVec_EachElementAry(
            int timeIndex,
            CFieldWorld world,
            uint fieldValId,
            bool isPmlYDirection,
            double pmlStPosX,
            double pmlLength,
            double dt,
            double newmarkBeta,
            MediaInfo media,
            uint eaId,
            uint node_cnt,
            uint free_node_cnt,
            Dictionary<uint, uint> toSorted,
            double[] Ez_Prev,
            double[] Ez_Prev2,
            ref double[][] w2_F_e_EA,
            ref double[][] w2_G_e_EA,
            ref double[] resVec)
        {
            System.Diagnostics.Debug.Assert(free_node_cnt == resVec.Length);
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
            // 要素行列
            double[,] eMMat = new double[nno, nno];
            double[,] eKXMat = new double[nno, nno];
            double[,] eKYMat = new double[nno, nno];

            CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
            CNodeAry.CNodeSeg ns_c_co = valField.GetNodeSeg(ELSEG_TYPE.CORNER, false, world);

            // 初回ステップ
            if (timeIndex == 0)
            {
                w2_F_e_EA = new double[ea.Size()][];
                w2_G_e_EA = new double[ea.Size()][];
                for (int ielem = 0; ielem < ea.Size(); ielem++)
                {
                    w2_F_e_EA[ielem] = new double[nno];
                    w2_G_e_EA[ielem] = new double[nno];
                }
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
                        eKXMat[ino, jno] = media.P[1, 1] * integralDLDX[ino, jno];
                        eKYMat[ino, jno] = media.P[0, 0] * integralDLDY[ino, jno];
                        // 要素質量行列
                        eMMat[ino, jno] = eps0 * myu0 * media.Q[2, 2] * integralL[ino, jno];
                    }
                }

                // PML媒質パラメータ
                double posXG = (p1[0] + p2[0] + p3[0]) / 3.0;
                if (isPmlYDirection)
                {
                    posXG = (p1[1] + p2[1] + p3[1]) / 3.0;
                }
                double sigmaX_e = 0.0;
                double c1_F_e = 0.0;
                double c1_G_e = 0.0;
                double c3_G_e = 0.0;
                GetPmlParameter(
                    posXG,
                    pmlStPosX,
                    pmlLength,
                    dt,
                    out sigmaX_e,
                    out c1_F_e,
                    out c1_G_e,
                    out c3_G_e);

                // w2_F, w2_Gの更新
                double[] w2_F_e = w2_F_e_EA[(int)ielem];
                double[] w2_G_e = w2_G_e_EA[(int)ielem];
                for (int ino = 0; ino < nno; ino++)
                {
                    uint iNodeNumber = no_c[ino];
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int inoGlobal = (int)toSorted[iNodeNumber];

                    w2_F_e[ino] = c1_F_e * Ez_Prev2[inoGlobal] + w2_F_e[ino];
                    w2_G_e[ino] = c1_G_e * Ez_Prev2[inoGlobal] + c3_G_e * w2_G_e[ino];
                }

                // 残差ベクトルにマージする
                double[,] work_eKXMat = eKXMat;
                double[,] work_eKYMat = eKYMat;
                if (isPmlYDirection)
                {
                    work_eKXMat = eKYMat;
                    work_eKYMat = eKXMat;
                }
                for (int ino = 0; ino < nno; ino++)
                {
                    uint iNodeNumber = no_c[ino];
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int inoGlobal = (int)toSorted[iNodeNumber];
                    double resVec_inoGlobal = 0.0;
                    for (int jno = 0; jno < nno; jno++)
                    {
                        uint jNodeNumber = no_c[jno];
                        if (!toSorted.ContainsKey(jNodeNumber)) continue;
                        int jnoGlobal = (int)toSorted[jNodeNumber];

                        resVec_inoGlobal +=
                            (sigmaX_e / eps0) * (1.0 / (2.0 * dt)) * eMMat[ino, jno] * Ez_Prev2[jnoGlobal]
                            - 1.0 * work_eKYMat[ino, jno] * (
                                (1.0 - newmarkBeta) * c1_F_e * Ez_Prev[jnoGlobal]
                                + w2_F_e[jno]
                                )
                            - 1.0 * work_eKXMat[ino, jno] * (
                                (newmarkBeta * c3_G_e * c1_G_e + (1.0 - 2.0 * newmarkBeta) * c1_G_e) * Ez_Prev[jnoGlobal]
                                + (newmarkBeta * c3_G_e * c3_G_e + (1.0 - 2.0 * newmarkBeta) * c3_G_e + newmarkBeta) * w2_G_e[jno]
                                );
                    }
                    resVec[inoGlobal] += resVec_inoGlobal;
                }
            }

            return true;
        }

        /// <summary>
        /// PML媒質のパラメータを取得する
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="pmlStPosX"></param>
        /// <param name="pmlLength"></param>
        /// <param name="dt"></param>
        /// <param name="sigmaX_e"></param>
        /// <param name="c1_F_e"></param>
        /// <param name="c1_G_e"></param>
        /// <param name="c3_G_e"></param>
        public static void GetPmlParameter(
            double posX,
            double pmlStPosX,
            double pmlLength,
            double dt,
            out double sigmaX_e,
            out double c1_F_e,
            out double c1_G_e,
            out double c3_G_e)
        {
            double reflect0 = 1.0e-8;
            double sigmaXMax = -3.0 * eps0 * c0 * Math.Log(reflect0) / (2.0 * pmlLength);
            double pmlX = Math.Abs(posX - pmlStPosX) / pmlLength;
            System.Diagnostics.Debug.Assert(pmlX <= 1.0);
            sigmaX_e = sigmaXMax * pmlX * pmlX;
            c1_F_e = (dt / eps0) * sigmaX_e;
            c1_G_e = -1.0 * (1.0 - Math.Exp(-(dt / eps0) * sigmaX_e));
            c3_G_e = Math.Exp(-(dt / eps0) * sigmaX_e);
        }

    }
}
