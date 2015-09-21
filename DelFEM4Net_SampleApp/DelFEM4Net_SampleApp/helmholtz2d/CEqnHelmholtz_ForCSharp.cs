////////////////////////////////////////////////////////////////////////////////////////////////////
// DelFEMのヘルムホルツ方程式ライブラリ処理をC#で記述してみました
//  オリジナルは DelFEM/src/femeqn/eqn_helmholtz.cppです
//  (C) 2012 ryujimiya 
////////////////////////////////////////////////////////////////////////////////////////////////////
/*
DelFEM (Finite Element Analysis)
Copyright (C) 2009  Nobuyuki Umetani    n.umetani@gmail.com

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace DelFEM4NetFem.Eqn
{
    /// <summary>
    ///  ヘルムホルツ方程式
    /// </summary>
    public class CEqnHelmholtz_ForCSharp
    {
        ///////////////////////////////////////////////////////////////////////////////////////
        // 定数
        ///////////////////////////////////////////////////////////////////////////////////////
        const double pi = 3.1416;

        /// <summary>
        /// ヘルムホルツの方程式 剛性行列 残差ベクトルの追加
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="waveLength"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <returns></returns>
        public static bool AddLinSysHelmholtz(CZLinearSystem ls, double waveLength, CFieldWorld world, uint fieldValId)
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
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.TRI11)
                {
                    uint ntmp = ls.GetTmpBufferSize();
                    int[] tmpBuffer = new int[ntmp];
                    for (int i = 0; i < ntmp; i++)
                    {
                        tmpBuffer[i] = -1;
                    }
                    bool res = AddLinSysHelmholtz_EachElementAry(ls, waveLength, world, fieldValId, eaId, tmpBuffer);
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
        /// <param name="ls"></param>
        /// <param name="waveLength"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <param name="eaId"></param>
        /// <param name="tmpBuffer"></param>
        /// <returns></returns>
        private static bool AddLinSysHelmholtz_EachElementAry(CZLinearSystem ls, double waveLength, CFieldWorld world, uint fieldValId, uint eaId, int[] tmpBuffer)
        {
            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.TRI);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            CField valField = world.GetField(fieldValId);
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 三角形要素の節点数
            uint nno = 3;
            // 座標の次元
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
            // 要素剛性行列のバッファ (i, j) --> (i * rowSize + j)
            Complex[] ematBuffer = new Complex[nno * nno];
            // 要素節点等価内力、外力、残差ベクトル
            Complex[] eres_c = new Complex[nno];
            // 要素剛性行列(コーナ-コーナー)
            CZMatDia_BlkCrs_Ptr mat_cc = ls.GetMatrixPtr(fieldValId, ELSEG_TYPE.CORNER, world);
            // 要素残差ベクトル(コーナー)
            CZVector_Blk_Ptr res_c = ls.GetResidualPtr(fieldValId, ELSEG_TYPE.CORNER, world);

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
                        coord_c[inoes][i] = tmpval[i];
                    }
                    //Console.WriteLine("coord_c [" + no_c[inoes] + " ] = " + coord_c[inoes, 0] + " " +  coord_c[inoes, 1]);
                }
                // 節点の値を取って来る
                es_c_va.GetNodes(ielem, no_c);
                for (uint inoes = 0; inoes < nno; inoes++)
                {
                    Complex[] tmpval = null;
                    ns_c_val.GetValue(no_c[inoes], out tmpval);
                    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                    value_c[inoes] = tmpval[0];
                    //Console.WriteLine("value_c [" + no_c[inoes] + " ] = " + tmpval[0].Real + " " +  tmpval[0].Imag);
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

                // 要素剛性行列を作る
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        //emat[ino, jno]
                        ematBuffer[ino * nno + jno] = area * (dldx[ino, 0] * dldx[jno, 0] + dldx[ino, 1] * dldx[jno, 1]);
                    }
                }
                double k0 = 2 * pi / waveLength;
                double tmp_val = k0 * k0 * area / 12.0;
                for (int ino = 0; ino < nno; ino++)
                {
                    //emat[ino, ino]
                    ematBuffer[ino * nno + ino] -= new Complex(tmp_val);
                    for (int jno = 0; jno < nno; jno++)
                    {
                        //emat[ino, jno]
                        ematBuffer[ino * nno + jno] -= new Complex(tmp_val);
                    }
                }
                // 要素節点等価内力ベクトルを求める
                for (int ino = 0; ino < nno; ino++)
                {
                    eres_c[ino] = new Complex(0.0);
                    for (int jno = 0; jno < nno; jno++)
                    {
                        eres_c[ino] -= ematBuffer[ino * nno + jno] * value_c[jno];
                    }
                }
                // 要素剛性行列にマージする
                mat_cc.Mearge(nno, no_c, nno, no_c, 1, ematBuffer, ref tmpBuffer);

                // 残差ベクトルにマージする
                for (int inoes = 0; inoes < nno; inoes++)
                {
                    res_c.AddValue(no_c[inoes], 0, eres_c[inoes]);
                }
            }

            return true;
        }

        /// <summary>
        /// ヘルムホルツの方程式 Sommerfelt放射条件
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="waveLength"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <returns></returns>
        public static bool AddLinSys_SommerfeltRadiationBC(CZLinearSystem ls, double waveLength, CFieldWorld world, uint fieldValId)
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
                if (valField.GetInterpolationType(eaId, world) == INTERPOLATION_TYPE.LINE11)
                {
                    uint ntmp = ls.GetTmpBufferSize();
                    int[] tmpBuffer = new int[ntmp];
                    for (int i = 0; i < ntmp; i++)
                    {
                        tmpBuffer[i] = -1;
                    }
                    bool res = AddLinSys_SommerfeltRadiationBC_EachElementAry(ls, waveLength, world, fieldValId, eaId, tmpBuffer);
                    if (!res)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// ヘルムホルツの方程式 Sommerfelt放射条件(要素アレイ単位)
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="waveLength"></param>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        /// <param name="eaId"></param>
        /// <param name="tmpBuffer"></param>
        /// <returns></returns>
        private static  bool AddLinSys_SommerfeltRadiationBC_EachElementAry(CZLinearSystem ls, double waveLength, CFieldWorld world, uint fieldValId, uint eaId, int[] tmpBuffer)
        {
            System.Diagnostics.Debug.Assert(world.IsIdEA(eaId));
            CElemAry ea = world.GetEA(eaId);
            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.LINE);
            if (!world.IsIdField(fieldValId))
            {
                return false;
            }
            CField valField = world.GetField(fieldValId);
            CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
            CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);

            // 線要素の節点数
            uint nno = 2;
            // 座標の次元
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
            // 要素剛性行列 (i, j) --> (i * RowSize + j)
            Complex[] ematBuffer = new Complex[nno * nno];
            // 要素節点等価内力、外力、残差ベクトル
            Complex[] eres_c = new Complex[nno];
            // 要素剛性行列(コーナ-コーナー)
            CZMatDia_BlkCrs_Ptr mat_cc = ls.GetMatrixPtr(fieldValId, ELSEG_TYPE.CORNER, world);
            // 要素残差ベクトル(コーナー)
            CZVector_Blk_Ptr res_c = ls.GetResidualPtr(fieldValId, ELSEG_TYPE.CORNER, world);

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
                        coord_c[inoes][i] = tmpval[i];
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
                double elen = Math.Sqrt((coord_c[0][0] - coord_c[1][0]) * (coord_c[0][0] - coord_c[1][0]) + (coord_c[0][1] - coord_c[1][1]) * (coord_c[0][1] - coord_c[1][1]));

                double k = 2.0 * pi / waveLength;
                Complex tmp_val1 = (k / 6.0 * elen) * (new Complex(0, 1));
                Complex tmp_val2 = -1 / (2.0 * elen * k) * (new Complex(0, 1));
                //emat[0, 0]
                ematBuffer[0] = tmp_val1 * 2 + tmp_val2;
                //emat[0, 1]
                ematBuffer[1] = tmp_val1 - tmp_val2;
                //emat[1, 0]
                ematBuffer[nno] = tmp_val1 - tmp_val2;
                //emat[1, 1]
                ematBuffer[nno + 1] = tmp_val1 * 2 + tmp_val2;

                // 要素節点等価内力ベクトルを求める
                for (int ino = 0; ino < nno; ino++)
                {
                    eres_c[ino] = new Complex(0.0);
                    for (int jno = 0; jno < nno; jno++)
                    {
                        eres_c[ino] -= ematBuffer[ino * nno + jno] * value_c[jno];
                    }
                }

                // 要素剛性行列にマージする
                mat_cc.Mearge(nno, no_c, nno, no_c, 1, ematBuffer, ref tmpBuffer);

                // 残差ベクトルにマージする
                for (int inoes = 0; inoes < nno; inoes++)
                {
                    res_c.AddValue(no_c[inoes], 0, eres_c[inoes]);
                }
            }
            return true;
        }
    }
}
