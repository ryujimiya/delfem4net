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
    class WgUtilForPeriodicEigenExt : WgUtilForPeriodicEigen
    {
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        /// <summary>
        /// 緩慢変化包絡線近似？
        ///    true  緩慢変化包絡線近似 Φ = φ(x, y) exp(-jβx) と置く方法
        ///    false Φを直接解く方法(exp(-jβd)を固有値として扱う)
        /// </summary>
        private const bool DefIsSVEA = true;  // Φ = φ(x, y) exp(-jβx) と置く方法
        //private const bool DefIsSVEA = false; // Φを直接解く方法(exp(-jβd)を固有値として扱う

        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 導波路ポート情報
        /// </summary>
        public class WgPortInfo
        {
            /// <summary>
            /// 入射ポート？
            /// </summary>
            public bool IsIncidentPort = false;
            /// <summary>
            /// 入射モードインデックス
            /// </summary>
            public int IncidentModeIndex = 0;
            /// <summary>
            /// 入射振幅
            /// </summary>
            public Complex[] Amps = null;

            /// <summary>
            /// 固有値問題を反復で解く？
            ///   true: 取得するモード数が1または2の場合に固有値問題を反復で解く
            ///     PropModeCntToSolve == 1の場合：線形固有方程式の反復を基本モードについて行う
            ///     PropModeCntToSolve == 2の場合：線形固有方程式の反復を基本モードと高次モードについて行う
            ///   false: 全モードを一括で解く
            /// </summary>
            public bool IsSolveEigenItr = true; // 反復で解く
            //public bool IsSolveEigenItr = false; // 全モード一括で解く
            /// <summary>
            /// 取得する伝搬モード数
            /// </summary>
            public int PropModeCntToSolve = 1;
            /// <summary>
            /// 緩慢変化包絡線近似？
            ///    true:  Φ = φ(x, y) exp(-jβx) の場合
            ///    false: Φ(x, y)を直接解く場合
            /// </summary>
            public bool IsSVEA = DefIsSVEA;
            /// <summary>
            /// モード追跡する？
            /// </summary>
            public bool IsModeTrace = true;

            /// <summary>
            /// ポートの境界フィールドID
            /// </summary>
            public uint FieldPortBcId = 0;

            /// <summary>
            /// 格子定数
            /// </summary>
            public double LatticeA = 0.0;
            /// <summary>1
            /// 周期距離
            /// </summary>
            public double PeriodicDistance = 0.0;
            /// <summary>
            /// 周期構造入出力導波路 ループID
            /// </summary>
            public uint FieldInputWgLoopId = 0;
            /// <summary>
            /// 周期構造入出力導波路 境界辺ID（内部側）
            /// </summary>
            public uint FieldInputWgBcId = 0;
            /// <summary>
            /// 周期構造入出力導波路 ループID→ループ情報のマップ
            /// </summary>
            public Dictionary<uint, wg2d.World.Loop> InputWgLoopDic = new Dictionary<uint, wg2d.World.Loop>();
            /// <summary>
            /// 周期構造入出力導波路 辺ID→辺の情報のマップ
            /// </summary>
            public Dictionary<uint, wg2d.World.Edge> InputWgEdgeDic = new Dictionary<uint, wg2d.World.Edge>();
            /// <summary>
            /// 周期構造入出力導波路 フォトニック導波路？
            /// </summary>
            public bool IsPCWaveguide = false;
            /// <summary>
            /// 周期構造入出力導波路 フォトニック導波路のポート（チャンネル)節点リストのリスト
            /// </summary>
            public IList<IList<uint>> PCWaveguidePorts = new List<IList<uint>>();
            /// <summary>
            /// 最小屈折率
            /// </summary>
            public double MinEffN = 0.0;
            /// <summary>
            /// 最大屈折率
            /// </summary>
            public double MaxEffN = 1.0;
            /// <summary>
            /// 考慮する波数ベクトルの最小値
            /// </summary>
            public double MinWaveNum = 0.0;
            /// <summary>
            /// 考慮する波数ベクトルの最大値
            /// </summary>
            public double MaxWaveNum = 0.5;
            /// <summary>
            /// 前回の固有モードベクトルのリスト
            /// </summary>
            public KrdLab.clapack.Complex[][] PrevModalVecList = null;

            public WgPortInfo()
            {
            }
        }

        /// <summary>
        /// 周期構造導波路開口固有値解析(FEM行列と固有モードの取得)
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldInputWgLoopId">フィールド値ID(周期構造領域のループ)</param>
        /// <param name="fieldPortBcId1">フィールド値ID(周期構造領域の境界1)</param>
        /// <param name="fieldInputWgBcId1">フィールド値ID(周期構造領域の境界2=内部側境界)</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
        /// <param name="IsPCWaveguide">フォトニック結晶導波路？</param>
        /// <param name="PCWaveguidePorts">フォトニック結晶導波路のポート(ロッド欠陥部分)の節点のリスト</param>
        /// <param name="medias">媒質リスト</param>
        /// <param name="inputWgLoopDic1">周期構造領域のワールド座標系ループ→ループ情報マップ</param>
        /// <param name="inputWgEdgeDic1">周期構造領域のワールド座標系辺→辺情報マップ</param>
        /// <param name="isPortBc2Reverse">境界２の方向が境界１と逆方向？</param>
        /// <param name="ryy_1d">1次元有限要素法[ryy]配列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <returns></returns>
        public static bool GetPortPeriodicWaveguideFemMatAndEigenVec(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            CFieldWorld world,
            uint fieldInputWgLoopId,
            uint fieldPortBcId1,
            uint fieldInputWgBcId1,
            uint[] fixedBcNodes,
            bool IsPCWaveguide,
            double latticeA,
            double periodicDistance,
            IList<IList<uint>> PCWaveguidePorts,
            int incidentModeIndex,
            bool isSolveEigenItr,
            int propModeCntToSolve,
            bool isSVEA,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[][] PrevModalVecList,
            double minBeta,
            double maxBeta,
            double minWaveNum,
            double maxWaveNum,
            IList<MediaInfo> medias,
            Dictionary<uint, wg2d.World.Loop> inputWgLoopDic1,
            Dictionary<uint, World.Edge> inputWgEdgeDic1,
            bool isPortBc2Reverse,
            out double[,] ryy_1d,
            out Complex[] eigen_values,
            out Complex[,] eigen_vecs,
            out Complex[,] eigen_dFdXs)
        {
            ryy_1d = null;
            eigen_values = null;
            eigen_vecs = null;
            eigen_dFdXs = null;

            if (!world.IsIdField(fieldInputWgLoopId))
            {
                return false;
            }
            // フィールドを取得
            CField valField = world.GetField(fieldInputWgLoopId);
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
            res = WgUtil.getRectangleWaveguideStructInfo(world, fieldPortBcId1, out coord_c_first, out coord_c_last, out waveguideWidth);
            if (!res)
            {
                return false;
            }
            // Y方向に周期構造?
            bool isYDirectionPeriodic = false;
            if (Math.Abs(coord_c_first[1] - coord_c_last[1]) < 1.0e-12)
            {
                isYDirectionPeriodic = true;
            }
            // 回転移動
            double[] rotOrigin = null;
            double rotAngle = 0.0; // ラジアン
            if (!isYDirectionPeriodic)
            {
                // 境界の傾きから回転角度を算出する
                if (Math.Abs(coord_c_first[0] - coord_c_last[0]) >= 1.0e-12)
                {
                    // X軸からの回転角
                    rotAngle = Math.Atan2((coord_c_last[1] - coord_c_first[1]), (coord_c_last[0] - coord_c_first[0]));
                    // Y軸からの回転角に変換 (境界はY軸に平行、X方向周期構造)
                    rotAngle = rotAngle - 0.5 * pi;
                    rotOrigin = coord_c_first;
                    System.Diagnostics.Debug.WriteLine("rotAngle: {0} rotOrigin:{1} {2}", rotAngle * 180.0 / pi, rotOrigin[0], rotOrigin[1]);
                }
            }

            // 境界上のすべての節点番号を取り出す
            res = WgUtil.GetBoundaryNodeList(world, fieldPortBcId1, out no_c_all, out to_no_boundary);
            if (!res)
            {
                return false;
            }

            // 境界積分用ryy_1dを取得する
            //   周期境界1について取得する
            res = getPortWaveguideFemMat(
                waveLength,
                world,
                fieldPortBcId1,
                fixedBcNodes,
                coord_c_first,
                waveguideWidth,
                no_c_all,
                to_no_boundary,
                medias,
                inputWgEdgeDic1,
                out ryy_1d,
                out coord_c_all);
            if (!res)
            {
                return false;
            }

            // 周期構造導波路の固有値、固有ベクトルを取得する
            //   固有ベクトルは境界上のみを取得する
            //uint max_mode = 1;
            uint max_mode = int.MaxValue;
            res = solvePortPeriodicWaveguideEigen(
                ls,
                waveLength,
                waveModeDv,
                isYDirectionPeriodic,
                rotAngle,
                rotOrigin,
                world,
                fieldInputWgLoopId,
                fieldPortBcId1,
                fieldInputWgBcId1,
                fixedBcNodes,
                IsPCWaveguide,
                latticeA,
                periodicDistance,
                PCWaveguidePorts,
                incidentModeIndex,
                isSolveEigenItr,
                propModeCntToSolve,
                isSVEA,
                max_mode,
                isModeTrace,
                ref PrevModalVecList,
                minBeta,
                maxBeta,
                minWaveNum,
                maxWaveNum,
                medias,
                inputWgLoopDic1,
                inputWgEdgeDic1,
                isPortBc2Reverse,
                ryy_1d,
                out eigen_values,
                out eigen_vecs,
                out eigen_dFdXs);
            if (!res)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 周期構造導波路開口境界条件
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldInputWgLoopId">フィールド値ID(周期構造領域のループ)</param>
        /// <param name="fieldPortBcId1">フィールド値ID(周期構造領域の境界1)</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
        /// <param name="isInputPort">入射ポート？</param>
        /// <param name="incidentModeIndex">入射モードのインデックス</param>
        /// <param name="isFreeBc">境界条件を課さない？</param>
        /// <param name="ryy_1d">1次元有限要素法[ryy]配列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <returns></returns>
        public static bool AddLinSys_PeriodicWaveguidePortBC(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            double periodicDistance,
            CFieldWorld world,
            uint fieldInputWgLoopId,
            uint fieldPortBcId1,
            uint[] fixedBcNodes,
            bool isInputPort,
            int incidentModeIndex,
            Complex[] amps,
            bool isFreeBc,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs,
            Complex[,] eigen_dFdXs)
        {
            if (!world.IsIdField(fieldInputWgLoopId))
            {
                return false;
            }
            // フィールドを取得
            CField valField = world.GetField(fieldInputWgLoopId);
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
            res = WgUtil.GetBoundaryNodeList(world, fieldPortBcId1, out no_c_all, out to_no_boundary);
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
            res = addLinSys_PeriodicWaveguidePortBC_Core(
                ls,
                waveLength,
                waveModeDv,
                periodicDistance,
                world,
                fieldPortBcId1,
                isInputPort,
                incidentModeIndex,
                amps,
                isFreeBc,
                ryy_1d,
                eigen_values,
                eigen_vecs,
                eigen_dFdXs,
                no_c_all,
                ref tmpBuffer);
            if (!res)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ryy_1d(1次線要素のpyy{N}t{N}マトリクス)取得
        ///     Note: 境界の要素はy = 0からy = waveguideWidth へ順番に要素アレイに格納され、節点2は次の要素の節点1となっていることを前提にしています。
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="world">ワールド座標系</param>
        /// <param name="fieldValId">フィールド値ID</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
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
        private static bool getPortWaveguideFemMat(
            double waveLength,
            CFieldWorld world,
            uint fieldValId,
            uint[] fixedBcNodes,
            double[] coord_c_first,
            double waveguideWidth,
            uint[] no_c_all,
            Dictionary<uint, uint> to_no_boundary,
            IList<MediaInfo> medias,
            Dictionary<uint, World.Edge> edgeDic,
            out double[,] ryy_1d,
            out double[,] coord_c_all)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 * c0;
            uint node_cnt = (uint)no_c_all.Length;

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
                bool res = WgUtil.addElementMatOf1dEigenValueProblem(
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

            if (fixedBcNodes != null)
            {
                // 強制境界
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
        /// <param name="incidentModeIndex">入射モードのインデックス</param>
        /// <param name="isFreeBc">境界条件を課さない？</param>
        /// <param name="ryy_1d">FEM[ryy]行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="no_c_all">節点番号配列</param>
        /// <param name="tmpBuffer">一時バッファ</param>
        /// <returns></returns>
        private static bool addLinSys_PeriodicWaveguidePortBC_Core(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            double periodicDistance,
            CFieldWorld world,
            uint fieldValId,
            bool isInputPort,
            int incidentModeIndex,
            Complex[] amps,
            bool isFreeBc,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs,
            Complex[,] eigen_dFdXs,
            uint[] no_c_all,
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
            if (!isFreeBc)
            {
                for (uint imode = 0; imode < max_mode; imode++)
                {
                    Complex betam = eigen_values[imode];
                    
                    Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
                    Complex[] dfmdxVec = MyMatrixUtil.matrix_GetRowVec(eigen_dFdXs, (int)imode);
                    Complex[] fmVec_Modify = new Complex[fmVec.Length];
                    Complex imagOne = new Complex(0.0, 1.0);
                    DelFEM4NetCom.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
                    for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                    {
                        //fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam);
                        fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam_periodic);
                    }
                    //Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                    //Complex[] vecj = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
                    Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec_Modify);
                    Complex[] vecj = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec_Modify));
                    for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                    {
                        for (uint jno_boundary = 0; jno_boundary < node_cnt; jno_boundary++)
                        {
                            Complex cvalue = 0.0;
                            if (waveModeDv == WaveModeDV.TM)
                            {
                                // TMモード
                                //cvalue = (imagOne / (omega * eps0)) * betam * Complex.Norm(betam) * veci[ino_boundary] * vecj[jno_boundary];
                                cvalue = (imagOne / (omega * eps0)) * (Complex.Norm(betam) * betam_periodic * Complex.Conjugate(betam_periodic) / Complex.Conjugate(betam)) * veci[ino_boundary] * vecj[jno_boundary];
                            }
                            else
                            {
                                // TEモード
                                //cvalue = (imagOne / (omega * myu0)) * betam * Complex.Norm(betam) * veci[ino_boundary] * vecj[jno_boundary];
                                cvalue = (imagOne / (omega * myu0)) * (Complex.Norm(betam) * betam_periodic * Complex.Conjugate(betam_periodic) / Complex.Conjugate(betam)) * veci[ino_boundary] * vecj[jno_boundary];
                            }
                            mat_all[ino_boundary, jno_boundary] += cvalue;
                        }
                    }
                }
                // check 対称行列
                bool isSymmetrix = true;
                for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                {
                    for (uint jno_boundary = ino_boundary; jno_boundary < node_cnt; jno_boundary++)
                    {
                        if (Math.Abs(mat_all[ino_boundary, jno_boundary].Real - mat_all[jno_boundary, ino_boundary].Real) >= 1.0e-12)
                        {
                            isSymmetrix = false;
                            break;
                            //System.Diagnostics.Debug.Assert(false);
                        }
                        if (Math.Abs(mat_all[ino_boundary, jno_boundary].Imag - mat_all[jno_boundary, ino_boundary].Imag) >= 1.0e-12)
                        {
                            isSymmetrix = false;
                            break;
                            //System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    if (!isSymmetrix)
                    {
                        break;
                    }
                }
                if (!isSymmetrix)
                {
                    System.Diagnostics.Debug.WriteLine("!!!!!!!!!!matrix is NOT symmetric!!!!!!!!!!!!!!");
                    //System.Diagnostics.Debug.Assert(false);
                }
                //MyMatrixUtil.printMatrix("emat_all", emat_all);
            }

            // 残差ベクトルの作成
            Complex[] res_c_all = new Complex[node_cnt];
            for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
            {
                res_c_all[ino_boundary] = 0.0;
            }
            if (isInputPort && incidentModeIndex < eigen_values.Length)
            {
                if (amps != null)
                {
                    // 全モードを入射させる
                    for (uint imode = 0; imode < max_mode; imode++)
                    {
                        Complex betam = eigen_values[imode];
                        Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
                        Complex[] dfmdxVec = MyMatrixUtil.matrix_GetRowVec(eigen_dFdXs, (int)imode);
                        Complex[] fmVec_Modify = new Complex[fmVec.Length];
                        Complex imagOne = new Complex(0.0, 1.0);
                        DelFEM4NetCom.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
                        for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                        {
                            //fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam);
                            fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam_periodic);
                        }
                        //Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                        Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec_Modify);
                        for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                        {
                            // TEモード、TMモード共通
                            //Complex cvalue = 2.0 * imagOne * betam * veci[ino_boundary] * amList[imode];
                            Complex cvalue = 2.0 * imagOne * betam_periodic * veci[ino_boundary] * amps[imode];
                            res_c_all[ino_boundary] += cvalue;
                        }
                    }
                }
                else
                {
                    uint imode = (uint)incidentModeIndex;
                    Complex betam = eigen_values[imode];
                    Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs, (int)imode);
                    Complex[] dfmdxVec = MyMatrixUtil.matrix_GetRowVec(eigen_dFdXs, (int)imode);
                    Complex[] fmVec_Modify = new Complex[fmVec.Length];
                    Complex imagOne = new Complex(0.0, 1.0);
                    DelFEM4NetCom.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
                    for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                    {
                        //fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam);
                        fmVec_Modify[ino_boundary] = fmVec[ino_boundary] - dfmdxVec[ino_boundary] / (imagOne * betam_periodic);
                    }
                    //Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                    Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec_Modify);
                    for (uint ino_boundary = 0; ino_boundary < node_cnt; ino_boundary++)
                    {
                        // TEモード、TMモード共通
                        //res_c_all[ino_boundary] = 2.0 * imagOne * betam * veci[ino_boundary];
                        res_c_all[ino_boundary] = 2.0 * imagOne * betam_periodic * veci[ino_boundary];
                    }
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

            if (!isFreeBc)
            {
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
        /// <param name="eigen_vecs">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <returns>散乱係数</returns>
        public static Complex GetPeriodicWaveguidePortReflectionCoef(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            double periodicDistance,
            CFieldWorld world,
            uint fieldValId,
            uint imode,
            bool isIncidentMode,
            Complex[] amps,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs,
            Complex[,] eigen_dFdXs)
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
            res = WgUtil.GetBoundaryFieldValueList(world, fieldValId, no_c_all, to_no_boundary, out value_c_all);
            if (!res)
            {
                return s11;
            }

            uint node_cnt = (uint)no_c_all.Length;
            System.Diagnostics.Debug.Assert(node_cnt == ryy_1d.GetLength(0));
            System.Diagnostics.Debug.Assert(node_cnt == ryy_1d.GetLength(1));
            System.Diagnostics.Debug.Assert(node_cnt == eigen_vecs.GetLength(1));
            s11 = getPeriodicWaveguidePortReflectionCoef_Core(
                waveLength,
                waveModeDv,
                periodicDistance,
                imode,
                isIncidentMode,
                amps,
                no_c_all,
                to_no_boundary,
                value_c_all,
                ryy_1d,
                eigen_values,
                eigen_vecs,
                eigen_dFdXs);

            return s11;
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
        /// <param name="eigen_vecs">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <returns></returns>
        private static Complex getPeriodicWaveguidePortReflectionCoef_Core(
            double waveLength,
            WaveModeDV waveModeDv,
            double periodicDistance,
            uint imode,
            bool isIncidentMode,
            Complex[] amps,
            uint[] no_c_all,
            Dictionary<uint, uint> to_no_boundary,
            Complex[] value_c_all,
            double[,] ryy_1d,
            Complex[] eigen_values,
            Complex[,] eigen_vecs,
            Complex[,] eigen_dFdXs)
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
            Complex[] dFdXVec = MyMatrixUtil.matrix_GetRowVec(eigen_dFdXs, (int)imode);
            Complex[] fmVec_Modify = new Complex[fmVec.Length];
            DelFEM4NetCom.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
            for (int ino = 0; ino < fmVec_Modify.Length; ino++)
            {
                //fmVec_Modify[ino] = fmVec[ino] - dFdXVec[ino] / (imagOne * betam);
                fmVec_Modify[ino] = fmVec[ino] - dFdXVec[ino] / (imagOne * betam_periodic);
            }
            //Complex[] tmp_vec = MyMatrixUtil.product(MyMatrixUtil.matrix_ConjugateTranspose(ryy_1d), MyMatrixUtil.vector_Conjugate(fmVec));
            // ryyが実数のとき
            //Complex[] tmp_vec = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
            Complex[] tmp_vec = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec_Modify));

            // s11 = {tmp_vec}t {value_all}
            s11 = MyMatrixUtil.vector_Dot(tmp_vec, value_c_all);
            if (waveModeDv == WaveModeDV.TM)
            {
                // TMモード
                //s11 *= (Complex.Norm(betam) / (omega * eps0));
                s11 *= ((Complex.Norm(betam) * Complex.Conjugate(betam_periodic) / Complex.Conjugate(betam)) / (omega * eps0));
                if (isIncidentMode)
                {
                    if (amps != null)
                    {
                        s11 += -1.0 * amps[imode];
                    }
                    else
                    {
                        s11 += -1.0;
                    }
                }
            }
            else
            {
                // TEモード
                //s11 *= (Complex.Norm(betam) / (omega * myu0));
                s11 *= ((Complex.Norm(betam) * Complex.Conjugate(betam_periodic) / Complex.Conjugate(betam)) / (omega * myu0));
                if (isIncidentMode)
                {
                    if (amps != null)
                    {
                        s11 += -1.0 * amps[imode];
                    }
                    else
                    {
                        s11 += -1.0;
                    }
                }
            }
            return s11;
        }
        
        /// <summary>
        /// 周期構造導波路の固有モード取得
        /// </summary>
        /// <param name="ls">リニアシステム</param>
        /// <param name="waveLength">波長</param>
        /// <param name="isYDirectionPeriodic">Y方向周期構造？</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldLoopId">フィールド値ID(周期構造領域のループ)</param>
        /// <param name="FieldPortBcId1">フィールド値ID(周期構造領域の境界1)</param>
        /// <param name="FieldPortBcId2">フィールド値ID(周期構造領域の境界2=内部側境界)</param>
        /// <param name="fixedBcNodes">強制境界節点配列</param>
        /// <param name="IsPCWaveguide">フォトニック結晶導波路？</param>
        /// <param name="PCWaveguidePorts">フォトニック結晶導波路のポート(ロッド欠陥部分)の節点のリスト</param>
        /// <param name="propModeCntToSolve">解く伝搬モードの数（固有値解法の選択基準に用いる)</param>
        /// <param name="max_mode">固有モードの考慮数</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">周期構造領域のワールド座標系ループ→ループ情報マップ</param>
        /// <param name="EdgeDic">周期構造領域のワールド座標系辺→辺情報マップ</param>
        /// <param name="isPortBc2Reverse">境界２の方向が境界１と逆方向？</param>
        /// <param name="ryy_1d">[ryy]FEM行列</param>
        /// <param name="eigen_values">固有値配列</param>
        /// <param name="eigen_vecs_Bc1">固有ベクトル行列{f}(i,j: i:固有値インデックス j:節点)</param>
        /// <param name="eigen_dFdXs_Bc1">固有ベクトル行列{df/dx}(i,j: i:固有値インデックス j:節点)</param>
        /// <returns></returns>
        private static bool solvePortPeriodicWaveguideEigen(
            CZLinearSystem ls,
            double waveLength,
            WaveModeDV waveModeDv,
            bool isYDirectionPeriodic,
            double rotAngle,
            double[] rotOrigin,
            CFieldWorld World,
            uint FieldLoopId,
            uint FieldPortBcId1,
            uint FieldPortBcId2,
            uint[] fixedBcNodes,
            bool IsPCWaveguide,
            double latticeA,
            double periodicDistance,
            IList<IList<uint>> PCWaveguidePorts,
            int incidentModeIndex,
            bool isSolveEigenItr,
            int propModeCntToSolve,
            bool isSVEA,
            uint max_mode,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[][] PrevModalVecList,
            double minEffN,
            double maxEffN,
            double minWaveNum,
            double maxWaveNum,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            bool isPortBc2Reverse,
            double[,] ryy_1d,
            out Complex[] eigen_values,
            out Complex[,] eigen_vecs_Bc1,
            out Complex[,] eigen_dFdXs_Bc1)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 * c0;

            eigen_values = null;
            eigen_vecs_Bc1 = null;
            eigen_dFdXs_Bc1 = null;
            //System.Diagnostics.Debug.Assert(max_mode == 1);

            double minBeta = minEffN;
            double maxBeta = maxEffN;

            //////////////////////////////////////////////////////////////////////////////////////
            // 周期構造導波路の固有値解析

            // 全節点数を取得する
            uint node_cnt = 0;
            //node_cnt = WgUtilForPeriodicEigen.GetNodeCnt(world, fieldLoopId);
            //uint[] no_c_all = null;
            //Dictionary<uint, uint> to_no_all = new Dictionary<uint, uint>();
            //WgUtilForPeriodicEigen.GetNodeList(World, FieldLoopId, out no_c_all);
            //node_cnt = (uint)no_c_all.Length;
            //for (int i = 0; i < node_cnt; i++)
            //{
            //    uint nodeNumber = no_c_all[i];
            //    to_no_all.Add(nodeNumber, (uint)i);
            //}
            uint[] no_c_all = null;
            Dictionary<uint, uint> to_no_all = null;
            double[][] coord_c_all = null;
            WgUtil.GetLoopCoordList(World, FieldLoopId, rotAngle, rotOrigin, out no_c_all, out to_no_all, out coord_c_all);
            node_cnt = (uint)no_c_all.Length;

            System.Diagnostics.Debug.WriteLine("solvePortPeriodicWaveguideEigen node_cnt: {0}", node_cnt);

            // 境界の節点リストを取得する
            uint[] no_boundary_fieldForceBcId = null;
            Dictionary<uint, uint> to_no_boundary_fieldForceBcId = null;
            if (fixedBcNodes != null)
            {
                to_no_boundary_fieldForceBcId = new Dictionary<uint, uint>();
                IList<uint> fixedBcNodesInLoop = new List<uint>();
                foreach (uint nodeNumber in fixedBcNodes)
                {
                    if (to_no_all.ContainsKey(nodeNumber))
                    {
                        fixedBcNodesInLoop.Add(nodeNumber);
                        to_no_boundary_fieldForceBcId.Add(nodeNumber, (uint)(fixedBcNodesInLoop.Count - 1));
                    }
                }
                no_boundary_fieldForceBcId = fixedBcNodesInLoop.ToArray();
            }
            uint[] no_c_all_fieldPortBcId1 = null;
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1 = null;
            WgUtil.GetBoundaryNodeList(World, FieldPortBcId1, out no_c_all_fieldPortBcId1, out to_no_boundary_fieldPortBcId1);
            uint[] no_c_all_fieldPortBcId2 = null;
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId2 = null;
            WgUtil.GetBoundaryNodeList(World, FieldPortBcId2, out no_c_all_fieldPortBcId2, out to_no_boundary_fieldPortBcId2);

            // 節点のソート
            IList<uint> sortedNodes = new List<uint>();
            Dictionary<uint, int> toSorted = new Dictionary<uint, int>();
            //   境界1と境界2は周期構造条件より同じ界の値をとる
            // ポート境界1
            for (int i = 0; i < no_c_all_fieldPortBcId1.Length; i++)
            {
                // 境界1の節点を追加
                uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[i];
                if (fixedBcNodes != null)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumberPortBc1)) continue;
                }
                sortedNodes.Add(nodeNumberPortBc1);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc1, nodeIndex);
            }
            uint boundary_node_cnt = (uint)sortedNodes.Count; // 境界1
            // 内部領域
            for (int i = 0; i < node_cnt; i++)
            {
                uint nodeNumber = no_c_all[i];
                // 追加済み節点はスキップ
                //if (toSorted.ContainsKey(nodeNumber)) continue;
                // 境界1は除く
                if (to_no_boundary_fieldPortBcId1.ContainsKey(nodeNumber)) continue;
                // 境界2は除く
                if (to_no_boundary_fieldPortBcId2.ContainsKey(nodeNumber)) continue;
                if (fixedBcNodes != null)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumber)) continue;
                }
                sortedNodes.Add(nodeNumber);
                toSorted.Add(nodeNumber, sortedNodes.Count - 1);
            }
            uint free_node_cnt = (uint)sortedNodes.Count;  // 境界1 + 内部領域
            for (int i = 0; i < no_c_all_fieldPortBcId2.Length; i++)
            {
                // 境界2の節点を追加
                uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[i];
                if (fixedBcNodes != null)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumberPortBc2)) continue;
                }
                sortedNodes.Add(nodeNumberPortBc2);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc2, nodeIndex);
            }
            uint free_node_cnt0 = (uint)sortedNodes.Count;  // 境界1 + 内部領域 + 境界2

            // 剛性行列、質量行列を作成
            double[] KMat0 = null;
            double[] CMat0 = null;
            double[] MMat0 = null;
            WgUtilForPeriodicEigen.MkPeriodicHelmholtzMat(
                waveLength,
                isYDirectionPeriodic,
                rotAngle,
                rotOrigin,
                World,
                FieldLoopId,
                Medias,
                LoopDic,
                node_cnt,
                free_node_cnt0,
                toSorted,
                out KMat0,
                out CMat0,
                out MMat0);

            // 緩慢変化包絡線近似？
            //bool isSVEA = true;  // 緩慢変化包絡線近似 Φ = φ(x, y) exp(-jβx) と置く方法
            //bool isSVEA = false; // Φを直接解く方法(exp(-jβd)を固有値として扱う)
            System.Diagnostics.Debug.WriteLine("isSVEA: {0}", isSVEA);
            System.Diagnostics.Debug.WriteLine("isModeTrace: {0}, isSolveEigenItr: {1}, propModeCntToSolve: {2}", isModeTrace, isSolveEigenItr, propModeCntToSolve);
            // 逆行列を使う？
            //bool isUseInvMat = false; // 逆行列を使用しない
            bool isUseInvMat = true; // 逆行列を使用する
            System.Diagnostics.Debug.WriteLine("isUseInvMat: {0}", isUseInvMat);
            
            /*
            // 反復計算のときはモード追跡をOFFにする(うまくいかないときがあるので)
            if (isSolveEigenItr && isModeTrace)
            {
                isModeTrace = false;
                System.Diagnostics.Debug.WriteLine("isModeTrace force to false.(isSolveEigenItr == true)");
            }
             */

            // 境界2の節点は境界1の節点と同一とみなす
            //   境界上の分割が同じであることが前提条件
            double[] KMat = null;
            double[] CMat = null;
            double[] MMat = null;
            if (isSVEA)
            {
                KMat = new double[free_node_cnt * free_node_cnt];
                CMat = new double[free_node_cnt * free_node_cnt];
                MMat = new double[free_node_cnt * free_node_cnt];
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                        CMat[i + free_node_cnt * j] = CMat0[i + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] = MMat0[i + free_node_cnt0 * j];
                    }
                }
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                        KMat[i + free_node_cnt * j] += KMat0[i + free_node_cnt0 * jno_B2];
                        CMat[i + free_node_cnt * j] += CMat0[i + free_node_cnt0 * jno_B2];
                        MMat[i + free_node_cnt * j] += MMat0[i + free_node_cnt0 * jno_B2];
                    }
                }
                for (int i = 0; i < boundary_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        int ino_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - i) : (int)(free_node_cnt + i);
                        KMat[i + free_node_cnt * j] += KMat0[ino_B2 + free_node_cnt0 * j];
                        CMat[i + free_node_cnt * j] += CMat0[ino_B2 + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] += MMat0[ino_B2 + free_node_cnt0 * j];
                    }
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        int ino_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - i) : (int)(free_node_cnt + i);
                        int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                        KMat[i + free_node_cnt * j] += KMat0[ino_B2 + free_node_cnt0 * jno_B2];
                        CMat[i + free_node_cnt * j] += CMat0[ino_B2 + free_node_cnt0 * jno_B2];
                        MMat[i + free_node_cnt * j] += MMat0[ino_B2 + free_node_cnt0 * jno_B2];
                    }
                }
                // 行列要素check
                {
                    for (int i = 0; i < free_node_cnt; i++)
                    {
                        for (int j = i; j < free_node_cnt; j++)
                        {
                            // [K]は対称行列
                            System.Diagnostics.Debug.Assert(Math.Abs(KMat[i + free_node_cnt * j] - KMat[j + free_node_cnt * i]) < Constants.PrecisionLowerLimit);
                            // [M]は対称行列
                            System.Diagnostics.Debug.Assert(Math.Abs(MMat[i + free_node_cnt * j] - MMat[j + free_node_cnt * i]) < Constants.PrecisionLowerLimit);
                            // [C]は反対称行列
                            System.Diagnostics.Debug.Assert(Math.Abs((-CMat[i + free_node_cnt * j]) - CMat[j + free_node_cnt * i]) < Constants.PrecisionLowerLimit);
                        }
                    }
                }
            }
            else
            {
                if (!isUseInvMat)
                {
                    KMat = new double[free_node_cnt * free_node_cnt];
                    CMat = new double[free_node_cnt * free_node_cnt];
                    MMat = new double[free_node_cnt * free_node_cnt];

                    CMat0 = null;
                    MMat0 = null;
                    uint inner_node_cnt = free_node_cnt - boundary_node_cnt;
                    for (int i = 0; i < boundary_node_cnt; i++)
                    {
                        int ino_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - i) : (int)(free_node_cnt + i);
                        for (int j = 0; j < boundary_node_cnt; j++)
                        {
                            int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                            // [K21]
                            KMat[i + free_node_cnt * j] = KMat0[ino_B2 + free_node_cnt0 * j];
                            // [K11] + [K22]
                            CMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j] + KMat0[ino_B2 + free_node_cnt0 * jno_B2];
                            // [K12]
                            MMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * jno_B2];
                        }
                        for (int j = 0; j < inner_node_cnt; j++)
                        {
                            // [K20]
                            KMat[i + free_node_cnt * (j + boundary_node_cnt)] = KMat0[ino_B2 + free_node_cnt0 * (j + boundary_node_cnt)];
                            // [K10]
                            CMat[i + free_node_cnt * (j + boundary_node_cnt)] = KMat0[i + free_node_cnt0 * (j + boundary_node_cnt)];
                            // [0]
                            MMat[i + free_node_cnt * (j + boundary_node_cnt)] = 0.0;
                        }
                    }
                    for (int i = 0; i < inner_node_cnt; i++)
                    {
                        for (int j = 0; j < boundary_node_cnt; j++)
                        {
                            int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                            // [0]
                            KMat[(i + boundary_node_cnt) + free_node_cnt * j] = 0.0;
                            // [K01]
                            CMat[(i + boundary_node_cnt) + free_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * j];
                            // [K02]
                            MMat[(i + boundary_node_cnt) + free_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * jno_B2];
                        }
                        for (int j = 0; j < inner_node_cnt; j++)
                        {
                            // [0]
                            KMat[(i + boundary_node_cnt) + free_node_cnt * (j + boundary_node_cnt)] = 0.0;
                            // [K00]
                            CMat[(i + boundary_node_cnt) + free_node_cnt * (j + boundary_node_cnt)] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * (j + boundary_node_cnt)];
                            // [0]
                            MMat[(i + boundary_node_cnt) + free_node_cnt * (j + boundary_node_cnt)] = 0.0;
                        }
                    }
                }
                else
                {
                    KMat = null;
                    CMat = null;
                    MMat = null;
                }
            }

            // 伝搬定数
            KrdLab.clapack.Complex[] betamToSolveList = null;
            // 界ベクトルは全節点分作成
            KrdLab.clapack.Complex[][] resVecList = null;
            // PC導波路の場合は、波数が[0, π]の領域から探索する
            if (IsPCWaveguide)
            {
                //minBeta = 0.0;
                //maxBeta = 0.5 * (2.0 * pi / periodicDistance) / k0;
                //minBeta = minWaveNum * (2.0 * pi / periodicDistance) / k0;
                //maxBeta = maxWaveNum * (2.0 * pi / periodicDistance) / k0;
                double minBeta_BZ = minWaveNum * (2.0 * pi / periodicDistance) / k0;
                double maxBeta_BZ = maxWaveNum * (2.0 * pi / periodicDistance) / k0;
                if (minBeta_BZ > minBeta)
                {
                    minBeta = minBeta_BZ;
                }
                if (maxBeta_BZ < maxBeta)
                {
                    maxBeta = maxBeta_BZ;
                }
                System.Diagnostics.Debug.WriteLine("minWaveNum:{0}, maxWaveNum: {1}", minWaveNum, maxWaveNum);
                System.Diagnostics.Debug.WriteLine("minBeta: {0}, maxBeta: {1}", minBeta, maxBeta);
            }

            // 緩慢変化包絡線近似でない場合は反復計算しない方法を使用する
            if (!isSolveEigenItr || propModeCntToSolve >= 3 || !isSVEA)
            {
                KrdLab.clapack.Complex[] tmpPrevModalVec_1stMode = null;
                if (isModeTrace && PrevModalVecList != null)
                {
                    // 前回の固有モードベクトルを取得する
                    //   現状１つだけ
                    if (PrevModalVecList.Length >= 0)
                    {
                        tmpPrevModalVec_1stMode = PrevModalVecList[0];
                    }
                }
                /*
                // マルチモードの場合
                // 周期構造導波路固有値問題を２次一般化固有値問題として解く
                solveAsQuadraticGeneralizedEigen(
                    k0,
                    KMat,
                    CMat,
                    MMat,
                    node_cnt,
                    free_node_cnt,
                    boundary_node_cnt,
                    sortedNodes,
                    toSorted,
                    to_no_all,
                    to_no_boundary_fieldPortBcId1,
                    isModeTrace,
                    ref PrevModalVec,
                    IsPCWaveguide,
                    PCWaveguidePorts,
                    out betamToSolveList,
                    out resVecList);
                 */

                if (!isUseInvMat)
                {
                    // マルチモードの場合
                    // 周期構造導波路固有値問題を２次一般化固有値問題として解く(実行列として解く)
                    solveAsQuadraticGeneralizedEigenWithRealMat(
                        incidentModeIndex,
                        isSVEA,
                        periodicDistance,
                        k0,
                        KMat,
                        CMat,
                        MMat,
                        node_cnt,
                        free_node_cnt,
                        boundary_node_cnt,
                        sortedNodes,
                        toSorted,
                        to_no_all,
                        to_no_boundary_fieldPortBcId1,
                        isYDirectionPeriodic,
                        coord_c_all,
                        IsPCWaveguide,
                        PCWaveguidePorts,
                        isModeTrace,
                        ref tmpPrevModalVec_1stMode,
                        minBeta,
                        maxBeta,
                        (2.0 * pi / periodicDistance), //k0, //1.0,
                        out betamToSolveList,
                        out resVecList);
                }
                else
                {
                    // 逆行列を使用する方法
                    if (isSVEA)
                    {
                        // マルチモードの場合
                        // 周期構造導波路固有値問題を２次一般化固有値問題→標準固有値問題として解く(実行列として解く)(緩慢変化包絡線近似用)
                        System.Diagnostics.Debug.Assert(isSVEA == true);
                        solveAsQuadraticGeneralizedEigenToStandardWithRealMat(
                            incidentModeIndex,
                            k0,
                            KMat,
                            CMat,
                            MMat,
                            node_cnt,
                            free_node_cnt,
                            boundary_node_cnt,
                            sortedNodes,
                            toSorted,
                            to_no_all,
                            to_no_boundary_fieldPortBcId1,
                            IsPCWaveguide,
                            PCWaveguidePorts,
                            isModeTrace,
                            ref tmpPrevModalVec_1stMode,
                            minBeta,
                            maxBeta,
                            k0, //(2.0 * pi / periodicDistance), //k0, //1.0,
                            out betamToSolveList,
                            out resVecList);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(isSVEA == false);
                        solveNonSVEAModeAsQuadraticGeneralizedEigenWithRealMat(
                            incidentModeIndex,
                            periodicDistance,
                            k0,
                            KMat0,
                            isPortBc2Reverse,
                            node_cnt,
                            free_node_cnt0,
                            free_node_cnt,
                            boundary_node_cnt,
                            sortedNodes,
                            toSorted,
                            to_no_all,
                            to_no_boundary_fieldPortBcId1,
                            isYDirectionPeriodic,
                            coord_c_all,
                            IsPCWaveguide,
                            PCWaveguidePorts,
                            isModeTrace,
                            ref tmpPrevModalVec_1stMode,
                            minBeta,
                            maxBeta,
                            (2.0 * pi / periodicDistance), //k0, //1.0,
                            out betamToSolveList,
                            out resVecList);
                    }
                }
                 
                if (isModeTrace && tmpPrevModalVec_1stMode != null)
                {
                    PrevModalVecList = new KrdLab.clapack.Complex[1][];
                    PrevModalVecList[0] = tmpPrevModalVec_1stMode;
                }
            }
            else if (isSolveEigenItr && propModeCntToSolve == 2)
            {
                // ２次の固有値問題として解くより、シングルモードのルーチンで
                // 基本モードと高次モードを計算した方が速い
                // 基本モード
                // 周期構造導波路固有値問題を一般化固有値問題の反復計算で解く
                KrdLab.clapack.Complex[] tmpPrevModalVec_1stMode = null;
                KrdLab.clapack.Complex[] tmpPrevModalVec_2ndMode = null;
                if (isModeTrace && PrevModalVecList != null)
                {
                    // 前回の固有モードベクトルを後ろから順に取得する
                    if (PrevModalVecList.Length >= 1)
                    {
                        tmpPrevModalVec_1stMode = PrevModalVecList[PrevModalVecList.Length - 1];
                    }
                    if (PrevModalVecList.Length >= 2)
                    {
                        tmpPrevModalVec_2ndMode = PrevModalVecList[PrevModalVecList.Length - 2];
                    }
                }
                System.Diagnostics.Debug.Assert(isSVEA == true);
                solveItrAsLinearGeneralizedEigen(
                    k0,
                    KMat,
                    CMat,
                    MMat,
                    node_cnt,
                    free_node_cnt,
                    boundary_node_cnt,
                    sortedNodes,
                    toSorted,
                    to_no_all,
                    to_no_boundary_fieldPortBcId1,
                    IsPCWaveguide,
                    PCWaveguidePorts,
                    false, // isCalcSecondMode: false
                    isModeTrace,
                    ref tmpPrevModalVec_1stMode,
                    minBeta,
                    maxBeta,
                    out betamToSolveList,
                    out resVecList);
                if (isModeTrace && tmpPrevModalVec_1stMode != null)
                {
                    PrevModalVecList = new KrdLab.clapack.Complex[1][];
                    PrevModalVecList[0] = tmpPrevModalVec_1stMode;
                }
                if (betamToSolveList != null)
                {
                    // 基本モードの解を退避
                    KrdLab.clapack.Complex[] betamToSolveList_1stMode = betamToSolveList;
                    KrdLab.clapack.Complex[][] resVecList_1stMode = resVecList;
                    // 高次モード
                    KrdLab.clapack.Complex[] betamToSolveList_2ndMode = null;
                    KrdLab.clapack.Complex[][] resVecList_2ndMode = null;
                    // 高次モードを反復計算で解く
                    // 周期構造導波路固有値問題を一般化固有値問題の反復計算で解く
                    System.Diagnostics.Debug.Assert(isSVEA == true);
                    solveItrAsLinearGeneralizedEigen(
                        k0,
                        KMat,
                        CMat,
                        MMat,
                        node_cnt,
                        free_node_cnt,
                        boundary_node_cnt,
                        sortedNodes,
                        toSorted,
                        to_no_all,
                        to_no_boundary_fieldPortBcId1,
                        IsPCWaveguide,
                        PCWaveguidePorts,
                        true, // isCalcSecondMode: true
                        isModeTrace,
                        ref tmpPrevModalVec_2ndMode,
                        minBeta,
                        maxBeta,
                        out betamToSolveList_2ndMode,
                        out resVecList_2ndMode);
                    if (betamToSolveList_2ndMode != null)
                    {
                        // betamToSolveListは伝搬定数の実部の昇順で並べる
                        // したがって、基本モードは最後に格納
                        betamToSolveList = new KrdLab.clapack.Complex[2];
                        resVecList = new KrdLab.clapack.Complex[2][];
                        // 2nd mode
                        betamToSolveList[0] = betamToSolveList_2ndMode[0];
                        resVecList[0] = resVecList_2ndMode[0];
                        // 1st mode
                        betamToSolveList[1] = betamToSolveList_1stMode[0];
                        resVecList[1] = resVecList_1stMode[0];

                        if (isModeTrace)
                        {
                            PrevModalVecList = new KrdLab.clapack.Complex[2][];
                            // 2nd mode
                            PrevModalVecList[0] = tmpPrevModalVec_2ndMode;
                            // 1st mode
                            PrevModalVecList[1] = tmpPrevModalVec_1stMode;
                        }
                    }
                }
            }
            else if (isSolveEigenItr && propModeCntToSolve == 1)
            {
                KrdLab.clapack.Complex[] tmpPrevModalVec_1stMode = null;
                if (isModeTrace && PrevModalVecList != null)
                {
                    // 前回の固有モードベクトルを後ろから順に取得する
                    if (PrevModalVecList.Length >= 1)
                    {
                        tmpPrevModalVec_1stMode = PrevModalVecList[PrevModalVecList.Length - 1];
                    }
                }
                // シングルモードの場合
                // 周期構造導波路固有値問題を一般化固有値問題の反復計算で解く
                System.Diagnostics.Debug.Assert(isSVEA == true);
                solveItrAsLinearGeneralizedEigen(
                    k0,
                    KMat,
                    CMat,
                    MMat,
                    node_cnt,
                    free_node_cnt,
                    boundary_node_cnt,
                    sortedNodes,
                    toSorted,
                    to_no_all,
                    to_no_boundary_fieldPortBcId1,
                    IsPCWaveguide,
                    PCWaveguidePorts,
                    false, // isCalcSecondMode: false
                    isModeTrace,
                    ref tmpPrevModalVec_1stMode,
                    minBeta,
                    maxBeta,
                    out betamToSolveList,
                    out resVecList);
                if (isModeTrace && tmpPrevModalVec_1stMode != null)
                {
                    PrevModalVecList = new KrdLab.clapack.Complex[1][];
                    PrevModalVecList[0] = tmpPrevModalVec_1stMode;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }

            // 固有値が1つでも取得できているかチェック
            if (betamToSolveList == null)
            {
                return false;
            }
            for (int imode = 0; imode < betamToSolveList.Length; imode++)
            {
                KrdLab.clapack.Complex betam = betamToSolveList[imode];
                KrdLab.clapack.Complex[] resVec = resVecList[imode];
                // ポート境界1の節点の値を境界2にも格納する
                for (int ino = 0; ino < no_c_all_fieldPortBcId1.Length; ino++)
                {
                    // 境界1の節点
                    uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[ino];
                    uint ino_InLoop_PortBc1 = to_no_all[nodeNumberPortBc1];
                    // 境界1の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[ino_InLoop_PortBc1];

                    // 境界2の節点
                    int ino_B2 = isPortBc2Reverse ? (int)(no_c_all_fieldPortBcId2.Length - 1 - ino) : (int)ino;
                    uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[ino_B2];

                    uint ino_InLoop_PortBc2 = to_no_all[nodeNumberPortBc2];
                    if (isSVEA)
                    {
                        // 緩慢変化包絡線近似の場合は、Φ2 = Φ1
                        resVec[ino_InLoop_PortBc2] = cvalue;
                    }
                    else
                    {
                        // 緩慢変化包絡線近似でない場合は、Φ2 = expA * Φ1
                        KrdLab.clapack.Complex expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam * periodicDistance);
                        resVec[ino_InLoop_PortBc2] = expA * cvalue; // 直接Bloch境界条件を指定する場合
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////
            // 位相調整

            for (int imode = 0; imode < betamToSolveList.Length; imode++)
            {
                KrdLab.clapack.Complex[] resVec = resVecList[imode];
                KrdLab.clapack.Complex phaseShift = 1.0;
                double maxAbs = double.MinValue;
                KrdLab.clapack.Complex fValueAtMaxAbs = 0.0;
                {
                    /*
                    // 境界上で位相調整する
                    for (int ino = 0; ino < no_c_all_fieldPortBcId1.Length; ino++)
                    {
                        uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[ino];
                        uint ino_InLoop_PortBc1 = to_no_all[nodeNumberPortBc1];
                        KrdLab.clapack.Complex cvalue = resVec[ino_InLoop_PortBc1];
                        double abs = KrdLab.clapack.Complex.Abs(cvalue);
                        if (abs > maxAbs)
                        {
                            maxAbs = abs;
                            fValueAtMaxAbs = cvalue;
                        }
                    }
                     */
                    // 領域全体で位相調整する
                    for (int ino_InLoop = 0; ino_InLoop < resVec.Length; ino_InLoop++)
                    {
                        KrdLab.clapack.Complex cvalue = resVec[ino_InLoop];
                        double abs = KrdLab.clapack.Complex.Abs(cvalue);
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
                System.Diagnostics.Debug.WriteLine("phaseShift: {0} (°)", Math.Atan2(phaseShift.Imaginary, phaseShift.Real) * 180.0 / pi);
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] /= phaseShift;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////
            // X方向の微分値を取得する
            KrdLab.clapack.Complex[][] resDFDXVecList = new KrdLab.clapack.Complex[betamToSolveList.Length][];
            for (int imode = 0; imode < betamToSolveList.Length; imode++)
            {
                KrdLab.clapack.Complex[] resVec = resVecList[imode];
                KrdLab.clapack.Complex[] resDFDXVec = null;
                KrdLab.clapack.Complex[] resDFDYVec = null;
                getDFDXValues(World, FieldLoopId, to_no_all, Medias, LoopDic, rotAngle, rotOrigin, resVec, out resDFDXVec, out resDFDYVec);
                if (isYDirectionPeriodic)
                {
                    // Y方向周期構造の場合
                    resDFDXVecList[imode] = resDFDYVec;
                }
                else
                {
                    // X方向周期構造の場合
                    resDFDXVecList[imode] = resDFDXVec;
                }
            }
            // 境界1と境界2の節点の微分値は同じという条件を弱形式で課している為、微分値は同じにならない。
            // 加えて、getDFDXValuesは内部節点からの寄与を片側のみしか計算していない。
            // →境界の両側からの寄与を考慮する為に境界1の微分値と境界2の微分値を平均してみる
            for (int imode = 0; imode < betamToSolveList.Length; imode++)
            {
                KrdLab.clapack.Complex betam = betamToSolveList[imode];
                KrdLab.clapack.Complex[] resDFDXVec = resDFDXVecList[imode];
                // ポート境界1の節点の微分値、ポート境界2の微分値は、両者の平均をとる
                for (int ino = 0; ino < no_c_all_fieldPortBcId1.Length; ino++)
                {
                    // 境界1の節点
                    uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[ino];
                    uint ino_InLoop_PortBc1 = to_no_all[nodeNumberPortBc1];
                    // 境界1の節点の界の微分値値を取得
                    KrdLab.clapack.Complex cdFdXValue1 = resDFDXVec[ino_InLoop_PortBc1];

                    // 境界2の節点
                    int ino_B2 = isPortBc2Reverse ? (int)(no_c_all_fieldPortBcId2.Length - 1 - ino) : (int)ino;
                    uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[ino_B2];
                    uint ino_InLoop_PortBc2 = to_no_all[nodeNumberPortBc2];
                    // 境界2の節点の界の微分値値を取得
                    KrdLab.clapack.Complex cdFdXValue2 = resDFDXVec[ino_InLoop_PortBc2];

                    // 平均値を計算し、境界の微分値として再格納する
                    if (isSVEA)
                    {
                        KrdLab.clapack.Complex cdFdXValue = (cdFdXValue1 + cdFdXValue2) * 0.5;
                        resDFDXVec[ino_InLoop_PortBc1] = cdFdXValue;
                        resDFDXVec[ino_InLoop_PortBc2] = cdFdXValue;
                    }
                    else
                    {
                        // 緩慢変化包絡線近似でない場合は、Φ2 = expA * Φ1
                        KrdLab.clapack.Complex expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam * periodicDistance);
                        // Φ2から逆算でΦ1を求め、平均をとる
                        KrdLab.clapack.Complex cdFdXValue = (cdFdXValue1 + cdFdXValue2 / expA) * 0.5;
                        resDFDXVec[ino_InLoop_PortBc1] = cdFdXValue;
                        resDFDXVec[ino_InLoop_PortBc2] = cdFdXValue * expA;
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////
            if (!isSVEA)
            {
                // 緩慢変化包絡線近似でない場合は、緩慢変化包絡線近似の分布に変換する
                for (int imode = 0; imode < betamToSolveList.Length; imode++)
                {
                    KrdLab.clapack.Complex betam = betamToSolveList[imode];
                    KrdLab.clapack.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
                    KrdLab.clapack.Complex[] resVec = resVecList[imode];
                    KrdLab.clapack.Complex[] resDFDXVec = resDFDXVecList[imode];
                    System.Diagnostics.Debug.Assert(resVec.Length == coord_c_all.Length);
                    uint nodeNumber1st = sortedNodes[0];
                    uint ino_InLoop_1st = to_no_all[nodeNumber1st];
                    double[] coord1st = coord_c_all[ino_InLoop_1st];
                    for (int ino_InLoop = 0; ino_InLoop < resVec.Length; ino_InLoop++)
                    {
                        // 節点の界の値
                        KrdLab.clapack.Complex fieldVal = resVec[ino_InLoop];
                        // 節点の界の微分値
                        KrdLab.clapack.Complex dFdXVal = resDFDXVec[ino_InLoop];
                        // 節点の座標
                        double[] coord = coord_c_all[ino_InLoop];
                        double x_pt = 0.0;
                        if (isYDirectionPeriodic)
                        {
                            x_pt = (coord[1] - coord1st[1]);
                        }
                        else
                        {
                            x_pt = (coord[0] - coord1st[0]);
                        }
                        //KrdLab.clapack.Complex expX = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam * x_pt);
                        KrdLab.clapack.Complex expX = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam_periodic * x_pt);
                        
                        // ΦのSVEA(φ)
                        KrdLab.clapack.Complex fieldVal_SVEA = fieldVal / expX;
                        resVec[ino_InLoop] = fieldVal_SVEA;

                        // SVEAの微分( exp(-jβx)dφ/dx = dΦ/dx + jβφexp(-jβx))
                        //resDFDXVec[ino_InLoop] = dFdXVal / expX + KrdLab.clapack.Complex.ImaginaryOne * betam * fieldVal_SVEA;
                        resDFDXVec[ino_InLoop] = dFdXVal / expX + KrdLab.clapack.Complex.ImaginaryOne * betam_periodic * fieldVal_SVEA;
                    }
                }
            }

            //////////////////////////////////////////////////////////////////////////////////////
            // 固有値、固有ベクトル
            // モード数の修正
            if (max_mode > betamToSolveList.Length)
            {
                max_mode = (uint)betamToSolveList.Length;
            }
            // 格納
            uint node_cnt_Bc1 = (uint)no_c_all_fieldPortBcId1.Length;
            eigen_values = new DelFEM4NetCom.Complex[max_mode];
            eigen_vecs_Bc1 = new DelFEM4NetCom.Complex[max_mode, node_cnt_Bc1];
            eigen_dFdXs_Bc1 = new DelFEM4NetCom.Complex[max_mode, node_cnt_Bc1];
            for (int imode = 0; imode < max_mode; imode++)
            {
                eigen_values[imode] = new DelFEM4NetCom.Complex(0, 0);
                for (int ino = 0; ino < node_cnt_Bc1; ino++)
                {
                    eigen_vecs_Bc1[imode, ino] = new DelFEM4NetCom.Complex(0, 0);
                    eigen_dFdXs_Bc1[imode, ino] = new DelFEM4NetCom.Complex(0, 0);
                }
            }
            for (int imode = betamToSolveList.Length - 1, tagtModeIndex = 0; imode >= 0 && tagtModeIndex < max_mode; imode--, tagtModeIndex++)
            {
                KrdLab.clapack.Complex workBetam = betamToSolveList[imode];
                KrdLab.clapack.Complex[] resVec = resVecList[imode];
                KrdLab.clapack.Complex[] resDFDXVec = resDFDXVecList[imode];

                DelFEM4NetCom.Complex betam = new DelFEM4NetCom.Complex(workBetam.Real, workBetam.Imaginary);
                bool isComplexConjugateMode = false;
                //   減衰定数は符号がマイナス(β = -jα)
                if (betam.Imag > 0.0 && Math.Abs(betam.Real) <= 1.0e-12)
                {
                    betam = new Complex(betam.Real, -betam.Imag);
                    isComplexConjugateMode = true;
                }
                DelFEM4NetCom.Complex[] evec = new DelFEM4NetCom.Complex[node_cnt_Bc1];
                DelFEM4NetCom.Complex[] evec_dFdX = new DelFEM4NetCom.Complex[node_cnt_Bc1];
                for (int ino = 0; ino < node_cnt_Bc1; ino++)
                {
                    uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[ino];
                    uint ino_InLoop_PortBc1 = to_no_all[nodeNumberPortBc1];
                    DelFEM4NetCom.Complex cvalue = new DelFEM4NetCom.Complex(resVec[ino_InLoop_PortBc1].Real, resVec[ino_InLoop_PortBc1].Imaginary);
                    DelFEM4NetCom.Complex dFdXValue = new DelFEM4NetCom.Complex(resDFDXVec[ino_InLoop_PortBc1].Real, resDFDXVec[ino_InLoop_PortBc1].Imaginary);
                    if (isComplexConjugateMode)
                    {
                        cvalue = DelFEM4NetCom.Complex.Conjugate(cvalue);
                        dFdXValue = DelFEM4NetCom.Complex.Conjugate(dFdXValue);
                    }
                    evec[ino] = cvalue;
                    evec_dFdX[ino] = dFdXValue;
                    if (tagtModeIndex == incidentModeIndex)
                    {
                        //System.Diagnostics.Debug.WriteLine("phase: {0} evec {1} evec_dFdX {2}", ino, Math.Atan2(cvalue.Imag, cvalue.Real) * 180 / pi, Math.Atan2(dFdXValue.Imag, dFdXValue.Real) * 180 / pi);
                    }
                }
                // 規格化定数を求める
                DelFEM4NetCom.Complex[] workVec = MyMatrixUtil.product(ryy_1d, evec);
                DelFEM4NetCom.Complex[] evec_Modify = new DelFEM4NetCom.Complex[node_cnt_Bc1];
                DelFEM4NetCom.Complex imagOne = new DelFEM4NetCom.Complex(0.0, 1.0);
                DelFEM4NetCom.Complex betam_periodic = ToBetaPeriodic(betam, periodicDistance);
                for (int ino = 0; ino < node_cnt_Bc1; ino++)
                {
                    //evec_Modify[ino] = evec[ino] - evec_dFdX[ino] / (imagOne * betam);
                    evec_Modify[ino] = evec[ino] - evec_dFdX[ino] / (imagOne * betam_periodic);
                }
                //Complex dm = MyMatrixUtil.vector_Dot(MyMatrixUtil.vector_Conjugate(evec), workVec);
                Complex dm = MyMatrixUtil.vector_Dot(MyMatrixUtil.vector_Conjugate(evec_Modify), workVec);
                if (waveModeDv == WaveModeDV.TM)
                {
                    // TMモード
                    //dm = MyMatrixUtil.complex_Sqrt(omega * eps0 / Complex.Norm(betam) / dm);
                    dm = MyMatrixUtil.complex_Sqrt(omega * eps0 * Complex.Conjugate(betam) / (Complex.Norm(betam) * Complex.Conjugate(betam_periodic)) / dm);
                }
                else
                {
                    // TEモード
                    //dm = MyMatrixUtil.complex_Sqrt(omega * myu0 / Complex.Norm(betam) / dm);
                    dm = MyMatrixUtil.complex_Sqrt(omega * myu0 * Complex.Conjugate(betam) / (Complex.Norm(betam) * Complex.Conjugate(betam_periodic)) / dm);
                }

                // 伝搬定数の格納
                eigen_values[tagtModeIndex] = betam;
                if (tagtModeIndex < 10)
                {
                    System.Diagnostics.Debug.WriteLine("β/k0  ( " + tagtModeIndex + " ) = " + betam.Real / k0 + " + " + betam.Imag / k0 + " i " + ((incidentModeIndex == tagtModeIndex) ? " incident" : ""));
                }
                // 固有ベクトルの格納(規格化定数を掛ける)
                for (int ino = 0; ino < evec.Length; ino++)
                {
                    DelFEM4NetCom.Complex fm = dm * evec[ino];
                    DelFEM4NetCom.Complex dfmdx = dm * evec_dFdX[ino];
                    eigen_vecs_Bc1[tagtModeIndex, ino] = fm;
                    eigen_dFdXs_Bc1[tagtModeIndex, ino] = dfmdx;
                    //System.Diagnostics.Debug.WriteLine("eigen_vecs_Bc1({0}, {1}) = {2} + {3} i", imode, ino, fm.Real, fm.Imag);
                }
                /*
                //DEBUG モード確認
                if (tagtModeIndex == incidentModeIndex)
                {
                    DelFEM4NetCom.Complex[] values_all = new DelFEM4NetCom.Complex[resVec.Length];
                    for (int ino = 0; ino < values_all.Length; ino++)
                    {
                        KrdLab.clapack.Complex cval = resVec[ino];
                        //KrdLab.clapack.Complex cval = resDFDXVec[ino];
                        values_all[ino] = new DelFEM4NetCom.Complex(cval.Real, cval.Imaginary);
                        //values_all[ino] = Complex.Norm(values_all[ino]);
                    }
                    WgUtil.SetFieldValueForDisplay(World, FieldLoopId, values_all, to_no_all);
                }
                 */
            }
            return true;
        }

        /// <summary>
        /// 周期構造導波路の伝搬定数に変換する(βdが[-π, π]に収まるように変換)
        /// </summary>
        /// <param name="beta"></param>
        /// <param name="periodicDistance"></param>
        /// <returns></returns>
        public static KrdLab.clapack.Complex ToBetaPeriodic(KrdLab.clapack.Complex betam, double periodicDistance)
        {
            // βの再変換
            KrdLab.clapack.Complex expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam * periodicDistance);
            KrdLab.clapack.Complex betam_periodic = -1.0 * MyUtilLib.Matrix.MyMatrixUtil.complex_Log(expA) / (KrdLab.clapack.Complex.ImaginaryOne * periodicDistance);
            return betam_periodic;
        }

        /// <summary>
        /// 周期構造導波路の伝搬定数に変換する(βdが[-π, π]に収まるように変換)
        /// </summary>
        /// <param name="beta"></param>
        /// <param name="periodicDistance"></param>
        /// <returns></returns>
        public static DelFEM4NetCom.Complex ToBetaPeriodic(DelFEM4NetCom.Complex betam, double periodicDistance)
        {
            KrdLab.clapack.Complex betam_periodic = ToBetaPeriodic(new KrdLab.clapack.Complex(betam.Real, betam.Imag), periodicDistance);
            return new DelFEM4NetCom.Complex(betam_periodic.Real, betam_periodic.Imaginary);
        }

        /*
        /// <summary>
        /// 周期構造導波路固有値問題を２次一般化固有値問題として解く
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="KMat"></param>
        /// <param name="CMat"></param>
        /// <param name="MMat"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        private static void solveAsQuadraticGeneralizedEigen(
            int incidentModeIndex,
            double k0,
            double[] KMat,
            double[] CMat,
            double[] MMat,
            uint node_cnt,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 非線形固有値問題
            //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
            //  λ= - jβとおくと
            //  [K] + λ[C] + λ^2[M]{Φ}= {0}
            //
            // Lisys(Lapack)による固有値解析
            // マトリクスサイズは、強制境界及び境界3を除いたサイズ
            int matLen = (int)free_node_cnt;
            KrdLab.clapack.Complex[] evals = null;
            KrdLab.clapack.Complex[,] evecs = null;

            // 一般化固有値解析
            KrdLab.clapack.Complex[] A = new KrdLab.clapack.Complex[(matLen * 2) * (matLen * 2)];
            KrdLab.clapack.Complex[] B = new KrdLab.clapack.Complex[(matLen * 2) * (matLen * 2)];
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    A[i + j * (matLen * 2)] = 0.0;
                    A[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                    //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                    // λ = -jβと置いた場合
                    //A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMat[i + j * matLen];
                    //A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * CMat[i + j * matLen];
                    //
                    //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                    // -[K] --> [K]
                    // j[C] --> [C]
                    //  とおいてλ=βとした場合
                    A[(i + matLen) + j * (matLen * 2)] = KMat[i + j * matLen];
                    A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * KrdLab.clapack.Complex.ImaginaryOne * CMat[i + j * matLen];
                }
            }
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    B[i + j * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                    B[i + (j + matLen) * (matLen * 2)] = 0.0;
                    B[(i + matLen) + j * (matLen * 2)] = 0.0;
                    B[(i + matLen) + (j + matLen) * (matLen * 2)] = MMat[i + j * matLen];
                }
            }
            KrdLab.clapack.Complex[] ret_evals = null;
            KrdLab.clapack.Complex[][] ret_evecs = null;
            System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.zggev");
            KrdLab.clapack.FunctionExt.zggev(A, (matLen * 2), (matLen * 2), B, (matLen * 2), (matLen * 2), ref ret_evals, ref ret_evecs);

            //evals = ret_evals;
            evals = new KrdLab.clapack.Complex[ret_evals.Length];
            // βを格納
            for (int i = 0; i < ret_evals.Length; i++)
            {
                KrdLab.clapack.Complex eval = ret_evals[i];
                //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                // λ = -jβと置いた場合
                //evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne;
                //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                // -[K] --> [K]
                // j[C] --> [C]
                //  とおいてλ=βとした場合
                evals[i] = eval;
            }

            System.Diagnostics.Debug.Assert(ret_evals.Length == ret_evecs.Length);
            // 2次元配列に格納する
            evecs = new KrdLab.clapack.Complex[ret_evecs.Length, (matLen * 2)];
            for (int i = 0; i < ret_evecs.Length; i++)
            {
                KrdLab.clapack.Complex[] ret_evec = ret_evecs[i];
                for (int j = 0; j < ret_evec.Length; j++)
                {
                    evecs[i, j] = ret_evec[j];
                }
            }

            // 固有値をソートする
            System.Diagnostics.Debug.Assert(evecs.GetLength(1) == free_node_cnt * 2);
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
                to_no_all,
                IsPCWaveguide,
                PCWaveguidePorts,
                isModeTrace,
                ref PrevModalVec,
                minBeta,
                maxBeta,
                evals,
                evecs,
                true, // isDebugShow
                out betamToSolveList,
                out resVecList);
        }
         */

        /// <summary>
        /// 周期構造導波路固有値問題を２次一般化固有値問題として解く(実行列として解く)
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="KMat"></param>
        /// <param name="CMat"></param>
        /// <param name="MMat"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        private static void solveAsQuadraticGeneralizedEigenWithRealMat(
            int incidentModeIndex,
            bool isSVEA,
            double periodicDistance,
            double k0,
            double[] KMat,
            double[] CMat,
            double[] MMat,
            uint node_cnt,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool isYDirectionPeriodic,
            double[][] coord_c_all,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            double betaNormalizingFactor,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 非線形固有値問題
            //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
            //  λ= - jβとおくと
            //  [K] + λ[C] + λ^2[M]{Φ}= {0}
            //
            // Lisys(Lapack)による固有値解析
            // マトリクスサイズは、強制境界及び境界3を除いたサイズ
            int matLen = (int)free_node_cnt;
            KrdLab.clapack.Complex[] evals = null;
            KrdLab.clapack.Complex[,] evecs = null;

            // 一般化固有値解析(実行列として解く)
            double[] A = new double[(matLen * 2) * (matLen * 2)];
            double[] B = new double[(matLen * 2) * (matLen * 2)];
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    if (isSVEA)
                    {
                        A[i + j * (matLen * 2)] = 0.0;
                        A[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                        //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                        // λ = -jβと置いた場合
                        //A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMat[i + j * matLen];
                        //A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * CMat[i + j * matLen];
                        // λ = -j(β/k0)と置いた場合
                        //A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMat[i + j * matLen];
                        //A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * k0 * CMat[i + j * matLen];
                        // λ = -j(β/betaNormalizingFactor)と置いた場合
                        A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMat[i + j * matLen];
                        A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * betaNormalizingFactor * CMat[i + j * matLen];
                    }
                    else
                    {
                        A[i + j * (matLen * 2)] = 0.0;
                        A[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                        A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMat[i + j * matLen];
                        A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * CMat[i + j * matLen];
                    }
                }
            }
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    if (isSVEA)
                    {
                        B[i + j * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                        B[i + (j + matLen) * (matLen * 2)] = 0.0;
                        B[(i + matLen) + j * (matLen * 2)] = 0.0;
                        // λ = -jβと置いた場合
                        //B[(i + matLen) + (j + matLen) * (matLen * 2)] = MMat[i + j * matLen];
                        // λ = -j(β/k0)と置いた場合
                        //B[(i + matLen) + (j + matLen) * (matLen * 2)] = k0 * k0 * MMat[i + j * matLen];
                        // λ = -j(β/betaNormalizingFactor)と置いた場合
                        B[(i + matLen) + (j + matLen) * (matLen * 2)] = betaNormalizingFactor * betaNormalizingFactor * MMat[i + j * matLen];
                    }
                    else
                    {
                        B[i + j * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                        B[i + (j + matLen) * (matLen * 2)] = 0.0;
                        B[(i + matLen) + j * (matLen * 2)] = 0.0;
                        B[(i + matLen) + (j + matLen) * (matLen * 2)] = MMat[i + j * matLen];
                    }
                }
            }
            double[] ret_r_evals = null;
            double[] ret_i_evals = null;
            double[][] ret_r_evecs = null;
            double[][] ret_i_evecs = null;
            System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.dggev");
            KrdLab.clapack.FunctionExt.dggev(A, (matLen * 2), (matLen * 2), B, (matLen * 2), (matLen * 2), ref ret_r_evals, ref ret_i_evals, ref ret_r_evecs, ref ret_i_evecs);

            evals = new KrdLab.clapack.Complex[ret_r_evals.Length];
            // βを格納
            for (int i = 0; i < ret_r_evals.Length; i++)
            {
                KrdLab.clapack.Complex eval = new KrdLab.clapack.Complex(ret_r_evals[i], ret_i_evals[i]);
                if (isSVEA)
                {
                    //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                    // λ = -jβと置いた場合(β = jλ)
                    //evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne;
                    // λ = -j(β/k0)と置いた場合
                    //evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne * k0;
                    // λ = -j(β/betaNormalizingFactor)と置いた場合
                    evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne * betaNormalizingFactor;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("exp(-jβd) = {0} + {1} i", eval.Real, eval.Imaginary);
                    if ((Math.Abs(eval.Real) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(eval.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        || double.IsInfinity(eval.Real) || double.IsInfinity(eval.Imaginary)
                        || double.IsNaN(eval.Real) || double.IsNaN(eval.Imaginary)
                        )
                    {
                        // 無効な固有値
                        //evals[i] = -1.0 * KrdLab.clapack.Complex.ImaginaryOne * double.MaxValue;
                        evals[i] = KrdLab.clapack.Complex.ImaginaryOne * double.MaxValue;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("exp(-jβd) = {0} + {1} i", eval.Real, eval.Imaginary);
                        KrdLab.clapack.Complex betatmp = -1.0 * MyUtilLib.Matrix.MyMatrixUtil.complex_Log(eval) / (KrdLab.clapack.Complex.ImaginaryOne * periodicDistance);
                        evals[i] = new KrdLab.clapack.Complex(betatmp.Real, betatmp.Imaginary);
                    }
                }
            }
            System.Diagnostics.Debug.Assert(ret_r_evals.Length == ret_r_evecs.Length);
            // 2次元配列に格納する
            evecs = new KrdLab.clapack.Complex[ret_r_evecs.Length, (matLen * 2)];
            for (int i = 0; i < ret_r_evecs.Length; i++)
            {
                double[] ret_r_evec = ret_r_evecs[i];
                double[] ret_i_evec = ret_i_evecs[i];
                for (int j = 0; j < ret_r_evec.Length; j++)
                {
                    evecs[i, j] = new KrdLab.clapack.Complex(ret_r_evec[j], ret_i_evec[j]);
                }
            }

            ////////////////////////////////////////////////////////////////////

            if (!isSVEA)
            {
                System.Diagnostics.Debug.Assert(free_node_cnt == (sortedNodes.Count - boundary_node_cnt));
                for (int imode = 0; imode < evals.Length; imode++)
                {
                    // 伝搬定数
                    KrdLab.clapack.Complex betatmp = evals[imode];
                    // 界ベクトル
                    KrdLab.clapack.Complex[] fieldVec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);

                    KrdLab.clapack.Complex beta_d_tmp = betatmp * periodicDistance;
                    if (
                        // [-π, 0]の解を[π, 2π]に移動する
                        ((minBeta * k0 * periodicDistance / (2.0 * pi)) >= (0.5 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                             && (minBeta * k0 * periodicDistance / (2.0 * pi)) < 1.0
                             && Math.Abs(betatmp.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && Math.Abs(betatmp.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && (beta_d_tmp.Real / (2.0 * pi)) >= (-0.5 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                             && (beta_d_tmp.Real / (2.0 * pi)) < 0.0)
                        // [0, π]の解を[2π, 3π]に移動する
                        || ((minBeta * k0 * periodicDistance / (2.0 * pi)) >= (1.0 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                                && (minBeta * k0 * periodicDistance / (2.0 * pi)) < 1.5
                                && Math.Abs(betatmp.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && Math.Abs(betatmp.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && (beta_d_tmp.Real / (2.0 * pi)) >= (0.0 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                                && (beta_d_tmp.Real / (2.0 * pi)) < 0.5)
                        )
                    /*
                    if (
                        // [-π, 0]の解を[π, 2π]に移動する
                        (
                             Math.Abs(betatmp.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && Math.Abs(betatmp.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && (beta_d_tmp.Real / (2.0 * pi)) >= (-0.5 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                             && (beta_d_tmp.Real / (2.0 * pi)) < 0.0)
                        )
                     */
                    {
                        // [0, π]の解を2πだけ移動する
                        double delta_phase = 2.0 * pi;
                        beta_d_tmp.Real += delta_phase;
                        betatmp = beta_d_tmp / periodicDistance;
                        //check
                        System.Diagnostics.Debug.WriteLine("shift beta * d / (2π): {0} + {1} i to {2} + {3} i",
                            evals[imode].Real * periodicDistance / (2.0 * pi),
                            evals[imode].Imaginary * periodicDistance / (2.0 * pi),
                            beta_d_tmp.Real / (2.0 * pi),
                            beta_d_tmp.Imaginary / (2.0 * pi));
                        // 再設定
                        evals[imode] = betatmp;

                        /*
                        //βを与えてk0を求める方法で計算した分布Φは、Φ|β2 = Φ|β1 (β2 = β1 + 2π)のように思われる
                        // したがって、界分布はずらさないことにする
                        // 界分布の位相をexp(j((2π/d)x))ずらす
                        uint nodeNumber1st = sortedNodes[0];
                        uint ino_InLoop_1st = to_no_all[nodeNumber1st];
                        double[] coord1st = coord_c_all[ino_InLoop_1st];
                        // 界ベクトルは{Φ}, {λΦ}の順にならんでいる
                        System.Diagnostics.Debug.Assert(free_node_cnt == (fieldVec.Length / 2));
                        for (int ino = 0; ino < fieldVec.Length; ino++)
                        {
                            uint nodeNumber = 0;
                            if (ino < free_node_cnt)
                            {
                                nodeNumber = sortedNodes[ino];
                            }
                            else
                            {
                                nodeNumber = sortedNodes[ino - (int)free_node_cnt];
                            }
                            uint ino_InLoop = to_no_all[nodeNumber];
                            double[] coord = coord_c_all[ino_InLoop];
                            // 界分布の位相をexp(j((2π/d)x))ずらす
                            double x_pt = 0.0;
                            if (isYDirectionPeriodic)
                            {
                                x_pt = (coord[1] - coord1st[1]);
                            }
                            else
                            {
                                x_pt = (coord[0] - coord1st[0]);
                            }
                            double delta_beta = -1.0 * delta_phase / periodicDistance;
                            KrdLab.clapack.Complex delta_expX = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * delta_beta * x_pt);
                            fieldVec[ino] *= delta_expX;
                        }
                        // 再設定
                        MyUtilLib.Matrix.MyMatrixUtil.matrix_setRowVec(evecs, imode, fieldVec);
                         */
                    }
                }
            }

            // 固有値をソートする
            System.Diagnostics.Debug.Assert(evecs.GetLength(1) == free_node_cnt * 2);
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
                to_no_all,
                IsPCWaveguide,
                PCWaveguidePorts,
                isModeTrace,
                ref PrevModalVec,
                minBeta,
                maxBeta,
                evals,
                evecs,
                true, // isDebugShow
                out betamToSolveList,
                out resVecList);
        }

        /// <summary>
        /// 固有値をソートする
        /// </summary>
        /// <param name="incidentModeIndex"></param>
        /// <param name="k0"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="isModeTrace"></param>
        /// <param name="PrevModalVec"></param>
        /// <param name="minBeta"></param>
        /// <param name="maxBeta"></param>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        public static void GetSortedModes(
            int incidentModeIndex,
            double k0,
            uint node_cnt,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            KrdLab.clapack.Complex[] evals,
            KrdLab.clapack.Complex[,] evecs,
            bool isDebugShow,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 固有値のソート
            WgUtilForPeriodicEigen.Sort1DEigenMode(evals, evecs);

            // 欠陥モードを取得
            IList<int> defectModeIndexList = new List<int>();
            // 追跡するモードのインデックス退避
            int traceModeIndex = -1;
            // フォトニック結晶導波路解析用
            if (IsPCWaveguide)
            {
                double hitNorm = 0.0;
                for (int imode = 0; imode < evals.Length; imode++)
                {
                    // 伝搬定数
                    KrdLab.clapack.Complex betam = evals[imode];
                    // 界ベクトル
                    KrdLab.clapack.Complex[] fieldVec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);

                    // フォトニック結晶導波路の導波モードを判定する
                    //System.Diagnostics.Debug.Assert((free_node_cnt * 2) == fieldVec.Length);
                    bool isHitDefectMode = isDefectMode(
                        k0,
                        free_node_cnt,
                        sortedNodes,
                        toSorted,
                        PCWaveguidePorts,
                        minBeta,
                        maxBeta,
                        betam,
                        fieldVec);

                    // 入射モードを追跡する
                    if (isHitDefectMode
                        && isModeTrace && PrevModalVec != null)
                    {
                        // 同じ固有モード？
                        double ret_norm = 0.0;
                        bool isHitSameMode = isSameMode(
                            k0,
                            node_cnt,
                            PrevModalVec,
                            free_node_cnt,
                            to_no_all,
                            sortedNodes,
                            toSorted,
                            PCWaveguidePorts,
                            betam,
                            fieldVec,
                            out ret_norm);
                        if (isHitSameMode)
                        {
                            // より分布の近いモードを採用する
                            if (Math.Abs(ret_norm - 1.0) < Math.Abs(hitNorm - 1.0))
                            {
                                // 追跡するモードのインデックス退避
                                traceModeIndex = imode;
                                hitNorm = ret_norm;
                                if (isDebugShow)
                                {
                                    System.Diagnostics.Debug.WriteLine("PCWaveguideMode(ModeTrace): imode = {0} β/k0 = {1} + {2} i", imode, betam.Real / k0, betam.Imaginary / k0);
                                }
                            }
                        }
                    }

                    if (isHitDefectMode)
                    {
                        if (isDebugShow)
                        {
                            System.Diagnostics.Debug.WriteLine("PCWaveguideMode: imode = {0} β/k0 = {1} + {2} i", imode, betam.Real / k0, betam.Imaginary / k0);
                        }
                        if (imode != traceModeIndex) // 追跡するモードは除外、あとで追加
                        {
                            defectModeIndexList.Add(imode);
                        }
                    }
                }
                if (isModeTrace && traceModeIndex != -1)
                {
                    if (isDebugShow)
                    {
                        System.Diagnostics.Debug.WriteLine("traceModeIndex:{0}", traceModeIndex);
                    }
                    // 追跡している入射モードがあれば最後に追加する
                    //defectModeIndexList.Add(traceModeIndex);
                    // 追跡している入射モードがあれば最後から入射モードインデックス分だけシフトした位置に挿入する
                    if (defectModeIndexList.Count >= (0 + incidentModeIndex))
                    {
                        defectModeIndexList.Insert(defectModeIndexList.Count - incidentModeIndex, traceModeIndex);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("other modes dissappeared ! defectModeIndexList cleared.");
                        traceModeIndex = -1;
                        defectModeIndexList.Clear();
                    }
                }
            }
            IList<int> selectedModeList = new List<int>();
            // フォトニック結晶導波路解析用
            if (IsPCWaveguide)
            {
                // フォトニック結晶導波路
                if (defectModeIndexList.Count > 0)
                {
                    // フォトニック結晶欠陥部閉じ込めモード
                    for (int iDefectModeIndex = defectModeIndexList.Count - 1; iDefectModeIndex >= 0; iDefectModeIndex--)
                    {
                        int imode = defectModeIndexList[iDefectModeIndex];
                        // 伝搬定数
                        KrdLab.clapack.Complex betam = evals[imode];
                        // 界ベクトル
                        KrdLab.clapack.Complex[] fieldVec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);
                        if (Math.Abs(betam.Real) < 1.0e-12 && Math.Abs(betam.Imaginary) >= 1.0e-12)
                        {
                            // 減衰モード
                            // 正しく計算できていないように思われる
                            if (selectedModeList.Count > incidentModeIndex)
                            {
                                // 基本モード以外の減衰モードは除外する
                                //System.Diagnostics.Debug.WriteLine("skip evanesent mode:β/k0  ( " + imode + " ) = " + betam.Real / k0 + " + " + betam.Imaginary / k0 + " i ");
                                continue;
                            }
                        }
                        selectedModeList.Add(imode);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not converged photonic crystal waveguide mode");
                }
            }
            else
            {
                // 通常の導波路
                for (int imode = evals.Length - 1; imode >= 0; imode--)
                {
                    KrdLab.clapack.Complex betam = evals[imode];
                    // 範囲外の伝搬モードを除外
                    //if (Math.Abs(betam.Real / k0) > maxBeta)
                    if (Math.Abs(betam.Real / k0) > maxBeta || Math.Abs(betam.Real / k0) < minBeta)
                    {
                        if (isDebugShow)
                        {
                            System.Diagnostics.Debug.WriteLine("skip: β/k0 ({0}) = {1} + {2} i at 2.0/λ = {3}", imode, betam.Real / k0, betam.Imaginary / k0, 2.0 / (2.0 * pi / k0));
                            Console.WriteLine("skip: β/k0 ({0}) = {1} + {2} i at 2.0/λ = {3}", imode, betam.Real / k0, betam.Imaginary / k0, 2.0 / (2.0 * pi / k0));
                        }
                        continue;
                    }

                    if (Math.Abs(betam.Real) >= 1.0e-12 && Math.Abs(betam.Imaginary) >= 1.0e-12)
                    {
                        // 複素モード
                        continue;
                    }
                    else if (Math.Abs(betam.Real) < 1.0e-12 && Math.Abs(betam.Imaginary) >= 1.0e-12)
                    {
                        // 減衰モード
                        ////  正の減衰定数の場合は除外する
                        //if (betam.Imaginary > 0)
                        //{
                        //    continue;
                        //}
                        // 正しく計算できていないように思われる
                        if (selectedModeList.Count > 0)
                        {
                            // 基本モード以外の減衰モードは除外する
                            //System.Diagnostics.Debug.WriteLine("skip evanesent mode:β/k0  ( " + imode + " ) = " + betam.Real / k0 + " + " + betam.Imaginary / k0 + " i ");
                            continue;
                        }
                    }
                    else if (Math.Abs(betam.Real) >= 1.0e-12 && Math.Abs(betam.Imaginary) < 1.0e-12)
                    {
                        // 伝搬モード
                        //  負の伝搬定数の場合は除外する
                        if (betam.Real < 0)
                        {
                            continue;
                        }
                    }
                    selectedModeList.Add(imode);
                }
            }
            if (selectedModeList.Count > 0)
            {
                betamToSolveList = new KrdLab.clapack.Complex[selectedModeList.Count];
                resVecList = new KrdLab.clapack.Complex[selectedModeList.Count][];
                for (int imode = 0; imode < betamToSolveList.Length; imode++)
                {
                    resVecList[imode] = new KrdLab.clapack.Complex[node_cnt]; // 全節点
                }
                for (int i = selectedModeList.Count - 1, selectedModeIndex = 0; i >= 0; i--, selectedModeIndex++)
                {
                    int imode = selectedModeList[i];
                    // 伝搬定数、固有ベクトルの格納
                    betamToSolveList[selectedModeIndex] = evals[imode];

                    KrdLab.clapack.Complex[] evec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);
                    // 非線形固有値方程式の解は{Φ} {λΦ}の順になっている
                    //System.Diagnostics.Debug.Assert((evec.Length == free_node_cnt * 2));
                    // 前半の{Φ}のみ取得する
                    for (int ino = 0; ino < free_node_cnt; ino++)
                    {
                        uint nodeNumber = sortedNodes[ino];
                        uint ino_InLoop = to_no_all[nodeNumber];
                        resVecList[selectedModeIndex][ino_InLoop] = evec[ino];
                    }
                    if (isDebugShow)
                    {
                        System.Diagnostics.Debug.WriteLine("mode({0}): index:{1} β/k0 = {2} + {3} i", selectedModeIndex, imode, (betamToSolveList[selectedModeIndex].Real / k0), (betamToSolveList[selectedModeIndex].Imaginary / k0));
                    }
                }
            }

            if (isModeTrace)
            {
                if (PrevModalVec != null && (traceModeIndex == -1))
                {
                    // モード追跡に失敗した場合
                    betamToSolveList = null;
                    resVecList = null;
                    System.Diagnostics.Debug.WriteLine("fail to trace mode at k0 = {0}", k0);
                    Console.WriteLine("fail to trace mode at k0 = {0}", k0);
                    return;
                }

                // 前回の固有ベクトルを更新
                if ((PrevModalVec == null || (PrevModalVec != null && traceModeIndex != -1)) // 初回格納、またはモード追跡に成功した場合だけ更新
                    && (betamToSolveList != null && betamToSolveList.Length >= (1 + incidentModeIndex))
                    )
                {
                    // 返却用リストでは伝搬定数の昇順に並んでいる→入射モードは最後
                    KrdLab.clapack.Complex betam = betamToSolveList[betamToSolveList.Length - 1 - incidentModeIndex];
                    KrdLab.clapack.Complex[] resVec = resVecList[betamToSolveList.Length - 1 - incidentModeIndex];
                    if (betam.Real != 0.0 && Math.Abs(betam.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                    {
                        //PrevModalVec = resVec;
                        PrevModalVec = new KrdLab.clapack.Complex[node_cnt];
                        resVec.CopyTo(PrevModalVec, 0);
                    }
                    else
                    {
                        // クリアしない(特定周波数で固有値が求まらないときがある。その場合でも同じモードを追跡できるように)
                        //PrevModalVec = null;
                    }
                }
                else
                {
                    // クリアしない(特定周波数で固有値が求まらないときがある。その場合でも同じモードを追跡できるように)
                    //PrevModalVec = null;
                }
            }
        }

        /// <summary>
        /// 周期構造導波路固有値問題を２次一般化固有値問題として解く(実行列として解く)(Φを直接解く方法)
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="KMat0"></param>
        /// <param name="CMat0"></param>
        /// <param name="MMat0"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        private static void solveNonSVEAModeAsQuadraticGeneralizedEigenWithRealMat(
            int incidentModeIndex,
            double periodicDistance,
            double k0,
            double[] KMat0,
            bool isPortBc2Reverse,
            uint node_cnt,
            uint free_node_cnt0,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool isYDirectionPeriodic,
            double[][] coord_c_all,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            double betaNormalizingFactor,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 複素モード、エバネセントモードの固有ベクトル計算をスキップする？ (計算時間短縮するため)
            bool isSkipCalcComplexAndEvanescentModeVec = true;
            System.Diagnostics.Debug.WriteLine("isSkipCalcComplexAndEvanescentModeVec: {0}", isSkipCalcComplexAndEvanescentModeVec);
            // 緩慢変化包絡線近似？ Φを直接解く方法なので常にfalse
            const bool isSVEA = false; // Φを直接解く方法
            // 境界1のみの式に変換
            uint inner_node_cnt = free_node_cnt - boundary_node_cnt;
            double[] P11 = new double[boundary_node_cnt * boundary_node_cnt];
            double[] P10 = new double[boundary_node_cnt * inner_node_cnt];
            double[] P12 = new double[boundary_node_cnt * boundary_node_cnt];
            double[] P01 = new double[inner_node_cnt * boundary_node_cnt];
            double[] P00 = new double[inner_node_cnt * inner_node_cnt];
            double[] P02 = new double[inner_node_cnt * boundary_node_cnt];
            double[] P21 = new double[boundary_node_cnt * boundary_node_cnt];
            double[] P20 = new double[boundary_node_cnt * inner_node_cnt];
            double[] P22 = new double[boundary_node_cnt * boundary_node_cnt];

            for (int i = 0; i < boundary_node_cnt; i++)
            {
                int ino_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - i) : (int)(free_node_cnt + i);
                for (int j = 0; j < boundary_node_cnt; j++)
                {
                    int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                    // [K11]
                    P11[i + boundary_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                    // [K12]
                    P12[i + boundary_node_cnt * j] = KMat0[i + free_node_cnt0 * jno_B2];
                    // [K21]
                    P21[i + boundary_node_cnt * j] = KMat0[ino_B2 + free_node_cnt0 * j];
                    // [K22]
                    P22[i + boundary_node_cnt * j] = KMat0[ino_B2 + free_node_cnt0 * jno_B2];
                }
                for (int j = 0; j < inner_node_cnt; j++)
                {
                    // [K10]
                    P10[i + boundary_node_cnt * j] = KMat0[i + free_node_cnt0 * (j + boundary_node_cnt)];
                    // [K20]
                    P20[i + boundary_node_cnt * j] = KMat0[ino_B2 + free_node_cnt0 * (j + boundary_node_cnt)];
                }
            }
            for (int i = 0; i < inner_node_cnt; i++)
            {
                for (int j = 0; j < boundary_node_cnt; j++)
                {
                    int jno_B2 = isPortBc2Reverse ? (int)(free_node_cnt + boundary_node_cnt - 1 - j) : (int)(free_node_cnt + j);
                    // [K01]
                    P01[i + inner_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * j];
                    // [K02]
                    P02[i + inner_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * jno_B2];
                }
                for (int j = 0; j < inner_node_cnt; j++)
                {
                    // [K00]
                    P00[i + inner_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * (j + boundary_node_cnt)];
                }
            }

            System.Diagnostics.Debug.WriteLine("setup [K]B [C]B [M]B");
            double[] invP00 = MyMatrixUtil.matrix_Inverse(P00, (int)(free_node_cnt - boundary_node_cnt));
            double[] P10_invP00 = MyMatrixUtil.product(
                P10, (int)boundary_node_cnt, (int)inner_node_cnt,
                invP00, (int)inner_node_cnt, (int)inner_node_cnt);
            double[] P20_invP00 = MyMatrixUtil.product(
                P20, (int)boundary_node_cnt, (int)inner_node_cnt,
                invP00, (int)inner_node_cnt, (int)inner_node_cnt);
            // for [C]B
            double[] P10_invP00_P01 = MyMatrixUtil.product(
                P10_invP00, (int)boundary_node_cnt, (int)inner_node_cnt,
                P01, (int)inner_node_cnt, (int)boundary_node_cnt);
            double[] P20_invP00_P02 = MyMatrixUtil.product(
                P20_invP00, (int)boundary_node_cnt, (int)inner_node_cnt,
                P02, (int)inner_node_cnt, (int)boundary_node_cnt);
            // for [M]B
            double[] P10_invP00_P02 = MyMatrixUtil.product(
                P10_invP00, (int)boundary_node_cnt, (int)inner_node_cnt,
                P02, (int)inner_node_cnt, (int)boundary_node_cnt);
            // for [K]B
            double[] P20_invP00_P01 = MyMatrixUtil.product(
                P20_invP00, (int)boundary_node_cnt, (int)inner_node_cnt,
                P01, (int)inner_node_cnt, (int)boundary_node_cnt);
            // [C]B
            double[] CMatB = new double[boundary_node_cnt * boundary_node_cnt];
            // [M]B
            double[] MMatB = new double[boundary_node_cnt * boundary_node_cnt];
            // [K]B
            double[] KMatB = new double[boundary_node_cnt * boundary_node_cnt];
            for (int i = 0; i < boundary_node_cnt; i++)
            {
                for (int j = 0; j < boundary_node_cnt; j++)
                {
                    CMatB[i + boundary_node_cnt * j] = 
                        - P10_invP00_P01[i + boundary_node_cnt * j]
                        + P11[i + boundary_node_cnt * j]
                        - P20_invP00_P02[i + boundary_node_cnt * j]
                        + P22[i + boundary_node_cnt * j];
                    MMatB[i + boundary_node_cnt * j] =
                        - P10_invP00_P02[i + boundary_node_cnt * j]
                        + P12[i + boundary_node_cnt * j];
                    KMatB[i + boundary_node_cnt * j] =
                        - P20_invP00_P01[i + boundary_node_cnt * j]
                        + P21[i + boundary_node_cnt * j];
                }
            }

            // 非線形固有値問題
            //  [K] + λ[C] + λ^2[M]{Φ}= {0}
            //
            // Lisys(Lapack)による固有値解析
            // マトリクスサイズは、強制境界及び境界3を除いたサイズ
            int matLen = (int)boundary_node_cnt;
            KrdLab.clapack.Complex[] evals = null;
            KrdLab.clapack.Complex[,] evecs = null;

            // 一般化固有値解析(実行列として解く)
            double[] A = new double[(matLen * 2) * (matLen * 2)];
            double[] B = new double[(matLen * 2) * (matLen * 2)];
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    A[i + j * (matLen * 2)] = 0.0;
                    A[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                    A[(i + matLen) + j * (matLen * 2)] = -1.0 * KMatB[i + j * matLen];
                    A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * CMatB[i + j * matLen];
                }
            }
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    B[i + j * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                    B[i + (j + matLen) * (matLen * 2)] = 0.0;
                    B[(i + matLen) + j * (matLen * 2)] = 0.0;
                    B[(i + matLen) + (j + matLen) * (matLen * 2)] = MMatB[i + j * matLen];
                }
            }
            double[] ret_r_evals = null;
            double[] ret_i_evals = null;
            double[][] ret_r_evecs = null;
            double[][] ret_i_evecs = null;
            System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.dggev");
            KrdLab.clapack.FunctionExt.dggev(A, (matLen * 2), (matLen * 2), B, (matLen * 2), (matLen * 2), ref ret_r_evals, ref ret_i_evals, ref ret_r_evecs, ref ret_i_evecs);

            evals = new KrdLab.clapack.Complex[ret_r_evals.Length];
            // βを格納
            for (int i = 0; i < ret_r_evals.Length; i++)
            {
                KrdLab.clapack.Complex eval = new KrdLab.clapack.Complex(ret_r_evals[i], ret_i_evals[i]);
                //System.Diagnostics.Debug.WriteLine("exp(-jβd) = {0} + {1} i", eval.Real, eval.Imaginary);
                if ((Math.Abs(eval.Real) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(eval.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                    || double.IsInfinity(eval.Real) || double.IsInfinity(eval.Imaginary)
                    || double.IsNaN(eval.Real) || double.IsNaN(eval.Imaginary)
                    )
                {
                    // 無効な固有値
                    //evals[i] = -1.0 * KrdLab.clapack.Complex.ImaginaryOne * double.MaxValue;
                    evals[i] = KrdLab.clapack.Complex.ImaginaryOne * double.MaxValue;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("exp(-jβd) = {0} + {1} i", eval.Real, eval.Imaginary);
                    KrdLab.clapack.Complex betatmp = -1.0 * MyUtilLib.Matrix.MyMatrixUtil.complex_Log(eval) / (KrdLab.clapack.Complex.ImaginaryOne * periodicDistance);
                    evals[i] = new KrdLab.clapack.Complex(betatmp.Real, betatmp.Imaginary);
                }
            }
            System.Diagnostics.Debug.Assert(ret_r_evals.Length == ret_r_evecs.Length);
            // 2次元配列に格納する ({Φ}のみ格納)
            evecs = new KrdLab.clapack.Complex[ret_r_evecs.Length, free_node_cnt];

            System.Diagnostics.Debug.WriteLine("calc {Φ}0");
            double[] invP00_P01 = MyMatrixUtil.product(
                invP00, (int)inner_node_cnt, (int)inner_node_cnt,
                P01, (int)inner_node_cnt, (int)boundary_node_cnt);
            double[] invP00_P02 = MyMatrixUtil.product(
                invP00, (int)inner_node_cnt, (int)inner_node_cnt,
                P02, (int)inner_node_cnt, (int)boundary_node_cnt);
            KrdLab.clapack.Complex[] transMat = new KrdLab.clapack.Complex[inner_node_cnt * boundary_node_cnt];
            System.Diagnostics.Debug.Assert(evals.Length == ret_r_evecs.Length);
            System.Diagnostics.Debug.Assert(evals.Length == ret_i_evecs.Length);
            for (int imode = 0; imode < evals.Length; imode++)
            {
                KrdLab.clapack.Complex betam = evals[imode];
                // 複素モード、エバネセントモードの固有モード計算をスキップする？
                if (isSkipCalcComplexAndEvanescentModeVec)
                {
                    if (Math.Abs(betam.Imaginary) >= Constants.PrecisionLowerLimit)
                    {
                        // 複素モード、エバネセントモードの固有モード計算をスキップする
                        continue;
                    }
                }

                KrdLab.clapack.Complex expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betam * periodicDistance);
                double[] ret_r_evec = ret_r_evecs[imode];
                double[] ret_i_evec = ret_i_evecs[imode];
                System.Diagnostics.Debug.Assert(ret_r_evec.Length == boundary_node_cnt * 2);
                KrdLab.clapack.Complex[] fVecB = new KrdLab.clapack.Complex[boundary_node_cnt];
                ///////////////////////////////
                // {Φ}Bのみ格納
                for (int ino = 0; ino < boundary_node_cnt; ino++)
                {
                    KrdLab.clapack.Complex cvalue = new KrdLab.clapack.Complex(ret_r_evec[ino], ret_i_evec[ino]);
                    evecs[imode, ino] = cvalue;
                    fVecB[ino] = cvalue;
                }

                ///////////////////////////////
                // {Φ}0を計算
                //   変換行列を計算
                for (int i = 0; i < inner_node_cnt; i++)
                {
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        transMat[i + inner_node_cnt * j] = -1.0 * (invP00_P01[i + inner_node_cnt * j] + expA * invP00_P02[i + inner_node_cnt * j]);
                    }
                }
                //   {Φ}0を計算
                KrdLab.clapack.Complex[] fVecInner = MyMatrixUtil.product(
                    transMat, (int)inner_node_cnt, (int)boundary_node_cnt,
                    fVecB, (int)boundary_node_cnt);
                //   {Φ}0を格納
                for (int ino = 0; ino < inner_node_cnt; ino++)
                {
                    evecs[imode, ino + boundary_node_cnt] = fVecInner[ino];
                }
            }

            ////////////////////////////////////////////////////////////////////

            if (!isSVEA)
            {
                System.Diagnostics.Debug.Assert(free_node_cnt == (sortedNodes.Count - boundary_node_cnt));
                for (int imode = 0; imode < evals.Length; imode++)
                {
                    // 伝搬定数
                    KrdLab.clapack.Complex betatmp = evals[imode];
                    // 界ベクトル
                    KrdLab.clapack.Complex[] fieldVec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);

                    KrdLab.clapack.Complex beta_d_tmp = betatmp * periodicDistance;
                    if (
                        // [-π, 0]の解を[π, 2π]に移動する
                        ((minBeta * k0 * periodicDistance / (2.0 * pi)) >= (0.5 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                             && (minBeta * k0 * periodicDistance / (2.0 * pi)) < 1.0
                             && Math.Abs(betatmp.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && Math.Abs(betatmp.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                             && (beta_d_tmp.Real / (2.0 * pi)) >= (-0.5 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                             && (beta_d_tmp.Real / (2.0 * pi)) < 0.0)
                        // [0, π]の解を[2π, 3π]に移動する
                        || ((minBeta * k0 * periodicDistance / (2.0 * pi)) >= (1.0 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                                && (minBeta * k0 * periodicDistance / (2.0 * pi)) < 1.5
                                && Math.Abs(betatmp.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && Math.Abs(betatmp.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && (beta_d_tmp.Real / (2.0 * pi)) >= (0.0 - MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                                && (beta_d_tmp.Real / (2.0 * pi)) < 0.5)
                        )
                    {
                        // [0, π]の解を2πだけ移動する
                        double delta_phase = 2.0 * pi;
                        beta_d_tmp.Real += delta_phase;
                        betatmp = beta_d_tmp / periodicDistance;
                        //check
                        System.Diagnostics.Debug.WriteLine("shift beta * d / (2π): {0} + {1} i to {2} + {3} i",
                            evals[imode].Real * periodicDistance / (2.0 * pi),
                            evals[imode].Imaginary * periodicDistance / (2.0 * pi),
                            beta_d_tmp.Real / (2.0 * pi),
                            beta_d_tmp.Imaginary / (2.0 * pi));
                        // 再設定
                        evals[imode] = betatmp;

                        /*
                        //βを与えてk0を求める方法で計算した分布Φは、Φ|β2 = Φ|β1 (β2 = β1 + 2π)のように思われる
                        // したがって、界分布はずらさないことにする
                        // 界分布の位相をexp(j((2π/d)x))ずらす
                        uint nodeNumber1st = sortedNodes[0];
                        uint ino_InLoop_1st = to_no_all[nodeNumber1st];
                        double[] coord1st = coord_c_all[ino_InLoop_1st];
                        // 界ベクトルは{Φ}, {λΦ}の順にならんでいる
                        System.Diagnostics.Debug.Assert(free_node_cnt == (fieldVec.Length / 2));
                        for (int ino = 0; ino < fieldVec.Length; ino++)
                        {
                            uint nodeNumber = 0;
                            if (ino < free_node_cnt)
                            {
                                nodeNumber = sortedNodes[ino];
                            }
                            else
                            {
                                nodeNumber = sortedNodes[ino - (int)free_node_cnt];
                            }
                            uint ino_InLoop = to_no_all[nodeNumber];
                            double[] coord = coord_c_all[ino_InLoop];
                            // 界分布の位相をexp(j((2π/d)x))ずらす
                            double x_pt = 0.0;
                            if (isYDirectionPeriodic)
                            {
                                x_pt = (coord[1] - coord1st[1]);
                            }
                            else
                            {
                                x_pt = (coord[0] - coord1st[0]);
                            }
                            double delta_beta = -1.0 * delta_phase / periodicDistance;
                            KrdLab.clapack.Complex delta_expX = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * delta_beta * x_pt);
                            fieldVec[ino] *= delta_expX;
                        }
                        // 再設定
                        MyUtilLib.Matrix.MyMatrixUtil.matrix_setRowVec(evecs, imode, fieldVec);
                         */
                    }
                }
            }

            // 固有値をソートする
            System.Diagnostics.Debug.Assert(evecs.GetLength(1) == free_node_cnt);
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
                to_no_all,
                IsPCWaveguide,
                PCWaveguidePorts,
                isModeTrace,
                ref PrevModalVec,
                minBeta,
                maxBeta,
                evals,
                evecs,
                true, // isDebugShow
                out betamToSolveList,
                out resVecList);
        }

        /// <summary>
        /// 周期構造導波路固有値問題を２次一般化固有値問題→標準固有値問題として解く(実行列として解く)(緩慢変化包絡線近似用)
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="KMat"></param>
        /// <param name="CMat"></param>
        /// <param name="MMat"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        private static void solveAsQuadraticGeneralizedEigenToStandardWithRealMat(
            int incidentModeIndex,
            double k0,
            double[] KMat,
            double[] CMat,
            double[] MMat,
            uint node_cnt,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            double betaNormalizingFactor,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 非線形固有値問題
            //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
            //  λ= - jβとおくと
            //  [K] + λ[C] + λ^2[M]{Φ}= {0}
            //
            // Lisys(Lapack)による固有値解析
            // マトリクスサイズは、強制境界及び境界3を除いたサイズ
            int matLen = (int)free_node_cnt;
            KrdLab.clapack.Complex[] evals = null;
            KrdLab.clapack.Complex[,] evecs = null;

            // [M]の逆行列が存在する緩慢変化包絡線近似の場合のみ有効な方法
            //   Φを直接解く場合は使えない
            System.Diagnostics.Debug.WriteLine("calc [M]-1");
            // [M]の逆行列を求める
            double[] invMMat = MyUtilLib.Matrix.MyMatrixUtil.matrix_Inverse(MMat, matLen);
            System.Diagnostics.Debug.WriteLine("calc [M]-1[K]");
            // [M]-1[K]
            double[] invMKMat = MyUtilLib.Matrix.MyMatrixUtil.product(invMMat, matLen, matLen, KMat, matLen, matLen);
            System.Diagnostics.Debug.WriteLine("calc [M]-1[C]");
            // [M]-1[C]
            double[] invMCMat = MyUtilLib.Matrix.MyMatrixUtil.product(invMMat, matLen, matLen, CMat, matLen, matLen);

            // 標準固有値解析(実行列として解く)
            double[] A = new double[(matLen * 2) * (matLen * 2)];
            System.Diagnostics.Debug.WriteLine("set [A]");
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    A[i + j * (matLen * 2)] = 0.0;
                    A[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
                    //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                    // λ = -jβと置いた場合
                    //A[(i + matLen) + j * (matLen * 2)] = -1.0 * invMKMat[i + j * matLen];
                    //A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * invMCMat[i + j * matLen];
                    // λ = -j(β/k0)と置いた場合
                    //A[(i + matLen) + j * (matLen * 2)] = -1.0 * invMKMat[i + j * matLen] / (k0 * k0);
                    //A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * invMCMat[i + j * matLen] / (k0);
                    // λ = -j(β/betaNormalizingFactor)と置いた場合
                    A[(i + matLen) + j * (matLen * 2)] = -1.0 * invMKMat[i + j * matLen] / (betaNormalizingFactor * betaNormalizingFactor);
                    A[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * invMCMat[i + j * matLen] / (betaNormalizingFactor);
                }
            }
            double[] ret_r_evals = null;
            double[] ret_i_evals = null;
            double[][] ret_r_evecs = null;
            double[][] ret_i_evecs = null;
            System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.dgeev");
            KrdLab.clapack.FunctionExt.dgeev(A, (matLen * 2), (matLen * 2), ref ret_r_evals, ref ret_i_evals, ref ret_r_evecs, ref ret_i_evecs);

            evals = new KrdLab.clapack.Complex[ret_r_evals.Length];
            // βを格納
            for (int i = 0; i < ret_r_evals.Length; i++)
            {
                KrdLab.clapack.Complex eval = new KrdLab.clapack.Complex(ret_r_evals[i], ret_i_evals[i]);
                //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
                // λ = -jβと置いた場合(β = jλ)
                //evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne;
                // λ = -j(β/k0)と置いた場合
                //evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne * k0;
                // λ = -j(β/betaNormalizingFactor)と置いた場合
                evals[i] = eval * KrdLab.clapack.Complex.ImaginaryOne * betaNormalizingFactor;
            }

            System.Diagnostics.Debug.Assert(ret_r_evals.Length == ret_r_evecs.Length);
            // 2次元配列に格納する
            evecs = new KrdLab.clapack.Complex[ret_r_evecs.Length, (matLen * 2)];
            for (int i = 0; i < ret_r_evecs.Length; i++)
            {
                double[] ret_r_evec = ret_r_evecs[i];
                double[] ret_i_evec = ret_i_evecs[i];
                for (int j = 0; j < ret_r_evec.Length; j++)
                {
                    evecs[i, j] = new KrdLab.clapack.Complex(ret_r_evec[j], ret_i_evec[j]);
                }
            }

            // 固有値をソートする
            System.Diagnostics.Debug.Assert(evecs.GetLength(1) == free_node_cnt * 2);
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
                to_no_all,
                IsPCWaveguide,
                PCWaveguidePorts,
                isModeTrace,
                ref PrevModalVec,
                minBeta,
                maxBeta,
                evals,
                evecs,
                true, // isDebugShow
                out betamToSolveList,
                out resVecList);
        }
         
        /// <summary>
        /// 周期構造導波路固有値問題を一般化固有値問題として解く(反復計算) (緩慢変化包絡線近似用)
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="KMat"></param>
        /// <param name="CMat"></param>
        /// <param name="MMat"></param>
        /// <param name="node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="to_no_all"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="isCalcSecondMode"></param>
        /// <param name="betamToSolveList"></param>
        /// <param name="resVecList"></param>
        private static void solveItrAsLinearGeneralizedEigen(
            double k0,
            double[] KMat,
            double[] CMat,
            double[] MMat,
            uint node_cnt,
            uint free_node_cnt,
            uint boundary_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            Dictionary<uint, uint> to_no_all,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isCalcSecondMode,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            out KrdLab.clapack.Complex[] betamToSolveList,
            out KrdLab.clapack.Complex[][] resVecList)
        {
            betamToSolveList = null;
            resVecList = null;

            // 伝搬定数の初期値を設定
            KrdLab.clapack.Complex betamToSolve = 0.0;

            //int itrMax = 10;
            //int itrMax = 20;
            int itrMax = 400;
            //double resMin = 1.0e-4;
            //double resMin = 1.0e-12;
            double resMin = 1.0e-6;
            double prevRes = double.MaxValue;
            bool isModeTraceWithinItr = true;
            KrdLab.clapack.Complex[] prevResVecItr = PrevModalVec;
            int itr = 0;
            for (itr = 0; itr < itrMax; itr++)
            {
                int matLen = (int)free_node_cnt;
                KrdLab.clapack.Complex[] evals = null;
                KrdLab.clapack.Complex[,] evecs = null;
                KrdLab.clapack.Complex[] A = new KrdLab.clapack.Complex[KMat.Length];
                KrdLab.clapack.Complex[] B = new KrdLab.clapack.Complex[MMat.Length];
                for (int i = 0; i < matLen * matLen; i++)
                {
                    A[i] = KMat[i] - KrdLab.clapack.Complex.ImaginaryOne * betamToSolve * CMat[i];
                    B[i] = MMat[i];
                }
                // 複素エルミートバンド行列の一般化固有値問題
                if (Math.Abs(betamToSolve.Imaginary) >= Constants.PrecisionLowerLimit) // 伝搬定数が実数の時のみに限定
                {
                    //Console.WriteLine("!!!!!!!!Not propagation mode. Skip calculate: betamToSolve: {0} + {1}i", betamToSolve.Real, betamToSolve.Imaginary);
                    System.Diagnostics.Debug.WriteLine("!!!!!!!!Not propagation mode. Skip calculate: betamToSolve: {0} + {1}i", betamToSolve.Real, betamToSolve.Imaginary);
                    betamToSolveList = null;
                    break;
                }
                else
                {
                    // エルミートバンド行列の一般化固有値問題を解く
                    solveHermitianBandMatGeneralizedEigen(matLen, A, B, ref evals, ref evecs);
                    /*
                    {
                        // 複素一般化固有値問題を解く
                        // 複素非対称行列の一般化固有値問題
                        // 一般化複素固有値解析
                        //   [A],[B]は内部で書き換えられるので注意
                        KrdLab.clapack.Complex[] ret_evals = null;
                        KrdLab.clapack.Complex[][] ret_evecs = null;
                        System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.zggev");
                        KrdLab.clapack.FunctionExt.zggev(A, matLen, matLen, B, matLen, matLen, ref ret_evals, ref ret_evecs);

                        evals = ret_evals;
                        System.Diagnostics.Debug.Assert(ret_evals.Length == ret_evecs.Length);
                        // 2次元配列に格納する
                        evecs = new KrdLab.clapack.Complex[ret_evecs.Length, matLen];
                        for (int i = 0; i < ret_evecs.Length; i++)
                        {
                            KrdLab.clapack.Complex[] ret_evec = ret_evecs[i];
                            for (int j = 0; j < ret_evec.Length; j++)
                            {
                                evecs[i, j] = ret_evec[j];
                            }
                        }
                    }
                     */
                }

                // βを格納
                for (int i = 0; i < evals.Length; i++)
                {
                    KrdLab.clapack.Complex eval = evals[i];
                    // 固有値がβ2の場合
                    // Note: 固有値は２乗で求まる
                    //   βを求める
                    KrdLab.clapack.Complex eval_sqrt = KrdLab.clapack.Complex.Sqrt(eval);
                    if (eval_sqrt.Real > 0 && eval_sqrt.Imaginary > 0)
                    {
                        eval_sqrt.Imaginary = -eval_sqrt.Imaginary;
                    }
                    evals[i] = eval_sqrt;
                }

                // 固有値をソートする
                KrdLab.clapack.Complex[] work_betamToSolveList = null;
                KrdLab.clapack.Complex[][] work_resVecList = null;
                int work_incidentModeIndex = (isCalcSecondMode ? 1 : 0);
                bool work_isModeTrace = isModeTrace;
                KrdLab.clapack.Complex[] workPrevModalVec = null;
                if (!IsPCWaveguide)
                {
                    work_isModeTrace = false;
                    workPrevModalVec = isModeTraceWithinItr ? prevResVecItr : PrevModalVec;
                }
                System.Diagnostics.Debug.Assert(evecs.GetLength(1) == free_node_cnt);
                GetSortedModes(
                    work_incidentModeIndex,
                    k0,
                    node_cnt,
                    free_node_cnt,
                    boundary_node_cnt,
                    sortedNodes,
                    toSorted,
                    to_no_all,
                    IsPCWaveguide,
                    PCWaveguidePorts,
                    work_isModeTrace,
                    ref workPrevModalVec,
                    minBeta,
                    maxBeta,
                    evals,
                    evecs,
                    false, // isDebugShow
                    out work_betamToSolveList,
                    out work_resVecList);
                if (work_betamToSolveList == null || work_betamToSolveList.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                    betamToSolve = 0;
                    betamToSolveList = null;
                    resVecList = null;
                    break;
                }
                if (isCalcSecondMode && work_incidentModeIndex >= work_betamToSolveList.Length)
                {
                    // 高次モード反復計算時
                    // 反復によって固有値を取得する場合は、基本モードがなくなるときがある（特定固有モードに収束させているため)
                    // fail safe
                    if (itr != 0 && work_incidentModeIndex >= work_betamToSolveList.Length)
                    {
                        work_incidentModeIndex = 0;
                    }
                }
                int tagtModeIndex = work_betamToSolveList.Length - 1 - work_incidentModeIndex;
                if (tagtModeIndex < 0 || tagtModeIndex >= work_betamToSolveList.Length)
                {
                    System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode [Error] tagtModeIndex = {0}", tagtModeIndex);
                    betamToSolve = 0;
                    betamToSolveList = null;
                    resVecList = null;
                    break;
                }

                // 伝搬定数、固有ベクトルの格納
                betamToSolveList = new KrdLab.clapack.Complex[1];
                resVecList = new KrdLab.clapack.Complex[1][];
                resVecList[0] = new KrdLab.clapack.Complex[node_cnt]; // 全節点
                // 収束判定用に前の伝搬定数を退避
                KrdLab.clapack.Complex prevBetam = betamToSolve;
                // 伝搬定数
                betamToSolve = work_betamToSolveList[tagtModeIndex];
                // 反復による固有モード計算ではモードは１つしか計算できない
                betamToSolveList[0] = betamToSolve;
                // 固有ベクトル
                KrdLab.clapack.Complex[] resVec = resVecList[0];
                work_resVecList[tagtModeIndex].CopyTo(resVec, 0);
                
                // 収束判定
                double res = KrdLab.clapack.Complex.Abs(prevBetam - betamToSolve);
                if (res < resMin)
                {
                    System.Diagnostics.Debug.WriteLine("converged itr:{0} betam:{1} + {2} i", itr, betamToSolve.Real, betamToSolve.Imaginary);
                    break;
                }
                // 発散判定
                if (itr >= 20 && Math.Abs(res) > Math.Abs(prevRes))
                {
                    System.Diagnostics.Debug.WriteLine("!!!!!!!! Not converged : prevRes = {0} res = {1} at 2/λ = {2}", prevRes, res, 2.0 / (2.0 * pi / k0));
                    Console.WriteLine("!!!!!!!! Not converged : prevRes = {0} res = {1} at 2/λ = {2}", prevRes, res, 2.0 / (2.0 * pi / k0));
                    betamToSolve = 0.0;
                    break;
                }
                prevRes = res;
                if (isModeTraceWithinItr && resVec != null)
                {
                    if (prevResVecItr == null)
                    {
                        // 初回のみメモリ確保
                        prevResVecItr = new KrdLab.clapack.Complex[node_cnt]; //全節点
                    }
                    // resVecは同じバッファを使いまわすので、退避用のprevResVecItrへはコピーする必要がある
                    resVec.CopyTo(prevResVecItr, 0);
                }
                // check
                if (itr % 20 == 0 && itr >= 20)
                {
                    System.Diagnostics.Debug.WriteLine("itr: {0}", itr);
                    Console.WriteLine("itr: {0}", itr);
                }
            }
            if (itr == itrMax)
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!! Not converged itr:{0} betam:{1} + {2} i at 2/λ = {3}", itr, betamToSolve.Real, betamToSolve.Imaginary, 2.0 / (2.0 * pi / k0));
                Console.WriteLine("!!!!!!!! Not converged itr:{0} betam:{1} + {2} i at 2/λ = {3}", itr, betamToSolve.Real, betamToSolve.Imaginary, 2.0 / (2.0 * pi / k0));
            }
            else if (itr >= 20 && (Math.Abs(betamToSolve.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit || Math.Abs(betamToSolve.Imaginary) >=  MyUtilLib.Matrix.Constants.PrecisionLowerLimit))
            {
                System.Diagnostics.Debug.WriteLine("converged but too slow!!!: itr: {0} at 2/λ = {1}", itr, 2.0 / (2.0 * pi / k0));
                Console.WriteLine("converged but too slow!!!: itr: {0} at 2/λ = {1}", itr, 2.0 / (2.0 * pi / k0));
            }
            // 前回の固有ベクトルを更新
            if (isModeTrace && betamToSolve.Real != 0.0 && Math.Abs(betamToSolve.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                //PrevModalVec = resVecList[0];
                PrevModalVec = new KrdLab.clapack.Complex[node_cnt];
                resVecList[0].CopyTo(PrevModalVec, 0);
            }
            else
            {
                // クリアしない(特定周波数で固有値が求まらないときがある。その場合でも同じモードを追跡できるように)
                //PrevModalVec = null;
            }
        }

        /// <summary>
        /// 欠陥モード？
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="minBeta"></param>
        /// <param name="maxBeta"></param>
        /// <param name="betam"></param>
        /// <param name="fieldVec"></param>
        /// <returns></returns>
        private static bool isDefectMode(
            double k0,
            uint free_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            IList<IList<uint>> PCWaveguidePorts,
            double minBeta,
            double maxBeta,
            KrdLab.clapack.Complex betam,
            KrdLab.clapack.Complex[] fieldVec)
        {
            bool isHit = false;
            if (Math.Abs(betam.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(betam.Imaginary) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 複素モードは除外する
                return isHit;
            }
            else if (Math.Abs(betam.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(betam.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 伝搬モード
                // 後進波は除外する
                if (betam.Real < 0)
                {
                    return isHit;
                }
            }
            else if (Math.Abs(betam.Real) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(betam.Imaginary) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 減衰モード
                //  利得のある波は除外する
                if (betam.Imaginary > 0)
                {
                    return isHit;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                return isHit;
            }

            // フォトニック結晶導波路の導波モードを判定する
            //
            // 領域内の節点の界の絶対値の２乗の和を計算
            //   要素分割が均一であることが前提。面積を考慮しない。
            double totalPower = 0.0;
            //for (int ino = 0; ino < fieldVec.Length; ino++)
            for (int ino = 0; ino < free_node_cnt; ino++)
            {
                double fieldAbs = fieldVec[ino].Magnitude;
                double power = fieldAbs * fieldAbs;
                totalPower += power;
            }
            // チャンネル上の節点の界の絶対値の２乗の和を計算
            //   要素分割が均一であることが前提。面積を考慮しない。
            int channelNodeCnt = 0;
            double channelTotalPower = 0.0;
            for (int portIndex = 0; portIndex < PCWaveguidePorts.Count; portIndex++)
            {
                IList<uint> portNodes = PCWaveguidePorts[portIndex];

                foreach (uint portNodeNumber in portNodes)
                {
                    if (!toSorted.ContainsKey(portNodeNumber)) continue;
                    int noSorted = toSorted[portNodeNumber];
                    //if (noSorted >= fieldVec.Length) continue;
                    if (noSorted >= free_node_cnt) continue;
                    KrdLab.clapack.Complex cvalue = fieldVec[noSorted];
                    double valAbs = cvalue.Magnitude;
                    double channelPower = valAbs * valAbs;
                    channelTotalPower += channelPower;
                    channelNodeCnt++;
                }
            }
            // 密度で比較する
            //totalPower /= fieldVec.Length;
            //channelTotalPower /= channelNodeCnt;
            ////const double powerRatioLimit = 3.0;
            //const double powerRatioLimit = 2.0;
            ////System.Diagnostics.Debug.WriteLine("channelTotalPower = {0}", (channelTotalPower / totalPower));
            // 総和で比較する
            const double powerRatioLimit = 0.5;
            if (Math.Abs(totalPower) >= Constants.PrecisionLowerLimit && (channelTotalPower / totalPower) >= powerRatioLimit)
            {
                //if (((Math.Abs(betam.Real) / k0) > maxBeta))
                if (Math.Abs(betam.Real / k0) > maxBeta || Math.Abs(betam.Real / k0) < minBeta)
                {
                    //Console.WriteLine("PCWaveguideMode: beta is invalid skip: β/k0 = {0} at k0 = {1}", betam.Real / k0, k0);
                    System.Diagnostics.Debug.WriteLine("PCWaveguideMode: beta is invalid skip: β/k0 = {0} at k0 = {1}", betam.Real / k0, k0);
                }
                else
                {
                    isHit = true;
                }
            }
            return isHit;
        }

        /// <summary>
        /// 同じ固有モード？
        /// </summary>
        /// <param name="k0"></param>
        /// <param name="node_cnt"></param>
        /// <param name="PrevModalVec"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betam"></param>
        /// <param name="fieldVec"></param>
        /// <returns></returns>
        private static bool isSameMode(
            double k0,
            uint node_cnt,
            KrdLab.clapack.Complex[] PrevModalVec,
            uint free_node_cnt,
            Dictionary<uint, uint> to_no_all,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            IList<IList<uint>> PCWaveguidePorts,
            KrdLab.clapack.Complex betam,
            KrdLab.clapack.Complex[] fieldVec,
            out double ret_norm)
        {
            bool isHit = false;
            ret_norm = 0.0;
            if (betam.Real > 0.0 && Math.Abs(betam.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                KrdLab.clapack.Complex[] workModalVec1 = new KrdLab.clapack.Complex[node_cnt]; // 前回
                KrdLab.clapack.Complex[] workModalVec2 = new KrdLab.clapack.Complex[node_cnt]; // 今回
                // 前半の{Φ}のみ取得する
                for (int ino = 0; ino < free_node_cnt; ino++)
                {
                    // 今回の固有ベクトル
                    //System.Diagnostics.Debug.WriteLine("    ( " + imode + ", " + ino + " ) = " + evec[ino].Real + " + " + evec[ino].Imaginary + " i ");
                    uint nodeNumber = sortedNodes[ino];
                    uint ino_InLoop = to_no_all[nodeNumber];
                    workModalVec2[ino_InLoop] = fieldVec[ino];
                    // 対応する前回の固有ベクトル
                    workModalVec1[ino_InLoop] = PrevModalVec[ino_InLoop];
                }
                KrdLab.clapack.Complex norm1 = MyUtilLib.Matrix.MyMatrixUtil.vector_Dot(MyUtilLib.Matrix.MyMatrixUtil.vector_Conjugate(workModalVec1), workModalVec1);
                KrdLab.clapack.Complex norm2 = MyUtilLib.Matrix.MyMatrixUtil.vector_Dot(MyUtilLib.Matrix.MyMatrixUtil.vector_Conjugate(workModalVec2), workModalVec2);
                for (int i = 0; i < node_cnt; i++)
                {
                    workModalVec1[i] /= Math.Sqrt(norm1.Magnitude);
                    workModalVec2[i] /= Math.Sqrt(norm2.Magnitude);
                }
                KrdLab.clapack.Complex norm12 = MyUtilLib.Matrix.MyMatrixUtil.vector_Dot(MyUtilLib.Matrix.MyMatrixUtil.vector_Conjugate(workModalVec1), workModalVec2);
                double thLikeMin = 0.9;
                double thLikeMax = 1.1;
                if (norm12.Magnitude >= thLikeMin && norm12.Magnitude < thLikeMax)
                {
                    isHit = true;
                    ret_norm = norm12.Magnitude;
                    System.Diagnostics.Debug.WriteLine("norm (prev * current): {0} + {1}i (Abs: {2})", norm12.Real, norm12.Imaginary, norm12.Magnitude);
                }
            }
            return isHit;
        }

        /// <summary>
        /// エルミートバンド行列の一般化固有値問題を解く(clapack)
        /// </summary>
        /// <param name="matLen"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        private static void solveHermitianBandMatGeneralizedEigen(int matLen, KrdLab.clapack.Complex[] A, KrdLab.clapack.Complex[] B, ref KrdLab.clapack.Complex[] evals, ref KrdLab.clapack.Complex[,] evecs)
        {
            // エルミート行列、正定値行列チェック
            bool isHermitianA = true;
            bool isHermitianB = true;
            bool isPositiveDefiniteB = true;
            for (int i = 0; i < matLen; i++)
            {
                // [B]の正定値行列チェック
                if (B[i + matLen * i].Real <= 0)
                {
                    isPositiveDefiniteB = false;
                    break;
                }
                for (int j = i; j < matLen; j++)
                {
                    // [A]のエルミート行列チェック
                    if (KrdLab.clapack.Complex.Abs(KrdLab.clapack.Complex.Conjugate(A[i + matLen * j]) - A[j + matLen * i]) >= Constants.PrecisionLowerLimit)
                    {
                        isHermitianA = false;
                        break;
                    }
                    // [B]のエルミート行列チェック
                    if (KrdLab.clapack.Complex.Abs(KrdLab.clapack.Complex.Conjugate(B[i + matLen * j]) - B[j + matLen * i]) >= Constants.PrecisionLowerLimit)
                    {
                        isHermitianB = false;
                        break;
                    }
                }
                if (!isHermitianA || !isHermitianB)
                {
                    break;
                }
            }
            System.Diagnostics.Debug.Assert(isHermitianA);
            System.Diagnostics.Debug.Assert(isHermitianB);
            System.Diagnostics.Debug.Assert(isPositiveDefiniteB);

            // パターン取得
            bool[,] patternA = new bool[matLen, matLen];
            bool[,] patternB = new bool[matLen, matLen];
            for (int i = 0; i < matLen; i++)
            {
                for (int j = 0; j < matLen; j++)
                {
                    //patternA[i, j] = (A[i + matLen * j].Magnitude >= Constants.PrecisionLowerLimit);
                    //patternB[i, j] = (B[i + matLen * j].Magnitude >= Constants.PrecisionLowerLimit);
                    patternA[i, j] = (A[i + matLen * j].Real != 0 || A[i + matLen * j].Imaginary != 0);
                    patternB[i, j] = (B[i + matLen * j].Real != 0 || B[i + matLen * j].Imaginary != 0);
                }
            }
            // バンド行列のバンド幅を縮小する
            KrdLab.clapack.Complex[] optA = new KrdLab.clapack.Complex[matLen * matLen];
            bool[,] optPatternA = new bool[matLen, matLen];
            IList<int> optNodesA = null;
            Dictionary<int, int> toOptNodesA = null;
            KrdLab.clapack.Complex[] optB = new KrdLab.clapack.Complex[matLen * matLen];
            bool[,] optPatternB = new bool[matLen, matLen];
            IList<int> optNodesB = null;
            Dictionary<int, int> toOptNodesB = null;
            // [B]のバンド幅を縮小する
            {
                GetOptBandMatNodes(patternB, out optNodesB, out toOptNodesB);
                for (int i = 0; i < matLen; i++)
                {
                    int ino_optB = toOptNodesB[i];
                    for (int j = 0; j < matLen; j++)
                    {
                        int jno_optB = toOptNodesB[j];
                        optPatternB[ino_optB, jno_optB] = patternB[i, j];
                        optB[ino_optB + matLen * jno_optB] = B[i + matLen * j];
                    }
                }
            }
            // [A]は[B]の節点並び替えに合わせて変更する
            {
                optNodesA = optNodesB;
                toOptNodesA = toOptNodesB;
                for (int i = 0; i < matLen; i++)
                {
                    int ino_optA = toOptNodesA[i];
                    for (int j = 0; j < matLen; j++)
                    {
                        int jno_optA = toOptNodesA[j];
                        optPatternA[ino_optA, jno_optA] = patternA[i, j];
                        optA[ino_optA + matLen * jno_optA] = A[i + matLen * j];
                    }
                }
            }
            patternA = null;
            patternB = null;
            A = null;
            B = null;

            // バンド行列のサイズ取得
            int a_rowcolSize;
            int a_subdiaSize;
            int a_superdiaSize;
            GetBandMatrixSubDiaSizeAndSuperDiaSize(optPatternA, out a_rowcolSize, out a_subdiaSize, out a_superdiaSize);
            int b_rowcolSize;
            int b_subdiaSize;
            int b_superdiaSize;
            GetBandMatrixSubDiaSizeAndSuperDiaSize(optPatternB, out b_rowcolSize, out b_subdiaSize, out b_superdiaSize);

            // バンド行列作成
            int _a_rsize = a_superdiaSize + 1;
            int _a_csize = a_rowcolSize;
            int _b_rsize = b_superdiaSize + 1;
            int _b_csize = b_rowcolSize;
            KrdLab.clapack.Complex[] AB = new KrdLab.clapack.Complex[_a_rsize * _a_csize];
            KrdLab.clapack.Complex[] BB = new KrdLab.clapack.Complex[_b_rsize * _b_csize];
            // [A]の値を[AB]にコピーする
            for (int c = 0; c < a_rowcolSize; c++)
            {
                // 対角成分
                AB[a_superdiaSize + c * _a_rsize] = optA[c + c * a_rowcolSize];

                // superdiagonals成分
                if (c > 0)
                {
                    for (int r = c - 1; r >= c - a_superdiaSize && r >= 0; r--)
                    {
                        AB[(r - c) + a_superdiaSize + c * _a_rsize] = optA[r + c * a_rowcolSize];
                    }
                }
            }
            // [B]の値を[BB]にコピーする
            for (int c = 0; c < b_rowcolSize; c++)
            {
                // 対角成分
                BB[b_superdiaSize + c * _b_rsize] = optB[c + c * b_rowcolSize];

                // superdiagonals成分
                if (c > 0)
                {
                    for (int r = c - 1; r >= c - b_superdiaSize && r >= 0; r--)
                    {
                        BB[(r - c) + b_superdiaSize + c * _b_rsize] = optB[r + c * b_rowcolSize];
                    }
                }
            }
            optA = null;
            optB = null;

            double[] ret_evals = null;
            KrdLab.clapack.Complex[][] ret_evecs = null;
            System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.zhbgv");
            KrdLab.clapack.FunctionExt.zhbgv(AB, matLen, matLen, a_superdiaSize, BB, matLen, matLen, b_superdiaSize, ref ret_evals, ref ret_evecs);

            // エルミート行列の固有値は実数なので複素数配列への移し替えを行う
            evals = new KrdLab.clapack.Complex[ret_evals.Length];
            for (int i = 0; i < ret_evals.Length; i++)
            {
                evals[i].Real = ret_evals[i];
                evals[i].Imaginary = 0;
            }
            System.Diagnostics.Debug.Assert(ret_evals.Length == ret_evecs.Length);
            // 2次元配列に格納する
            evecs = new KrdLab.clapack.Complex[ret_evecs.Length, matLen];
            for (int i = 0; i < ret_evecs.Length; i++)
            {
                KrdLab.clapack.Complex[] ret_evec = ret_evecs[i];
                for (int j = 0; j < ret_evec.Length; j++)
                {
                    // バンド幅縮小で並び替えた節点→元の節点番号変換
                    int jnoGlobal = optNodesB[j];
                    evecs[i, jnoGlobal] = ret_evec[j];
                }
            }
        }

        /// <summary>
        /// 界のx方向微分値を取得する
        /// </summary>
        /// <param name="world"></param>
        /// <param name="fieldValId"></param>
        public static void getDFDXValues(
            CFieldWorld world,
            uint fieldValId,
            Dictionary<uint, uint> to_no_all,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            double rotAngle,
            double[] rotOrigin,
            KrdLab.clapack.Complex[] fVec,
            out KrdLab.clapack.Complex[] dFdXVec,
            out KrdLab.clapack.Complex[] dFdYVec)
        {
            dFdXVec = new KrdLab.clapack.Complex[fVec.Length];
            dFdYVec = new KrdLab.clapack.Complex[fVec.Length];

            CField valField_base = world.GetField(fieldValId);
            System.Diagnostics.Debug.Assert(valField_base.GetFieldType() == FIELD_TYPE.ZSCALAR);
            Dictionary<uint, int> nodeElemCntH = new Dictionary<uint, int>();
            //Dictionary<uint, double> nodeMediaP11Sum = new Dictionary<uint,double>(); //境界に限ればこの計算は誤差がある
            Dictionary<uint, double> nodeAreaSum = new Dictionary<uint, double>();

            IList<uint> aIdEA = valField_base.GetAryIdEA();
            foreach (uint eaId in aIdEA)
            {
                CElemAry ea = world.GetEA(eaId);
                CField valField = world.GetField(fieldValId);
                if (valField.GetInterpolationType(eaId, world) != INTERPOLATION_TYPE.TRI11) continue;
                
                // 媒質を取得する
                MediaInfo media = new MediaInfo();
                {
                    // ループのIDのはず
                    uint lId = eaId;
                    if (LoopDic.ContainsKey(lId))
                    {
                        World.Loop loop = LoopDic[lId];
                        media = Medias[loop.MediaIndex];
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                 
                // 要素セグメントコーナー値
                //CElemAry.CElemSeg es_c_va = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, true, world);
                // 要素セグメントコーナー座標
                CElemAry.CElemSeg es_c_co = valField.GetElemSeg(eaId, ELSEG_TYPE.CORNER, false, world);
                uint nno = 3;
                uint ndim = 2;
                // 要素節点の全体節点番号
                uint[] no_c = new uint[nno];
                // 要素節点の値
                //Complex[] value_c = new Complex[nno];
                // 要素節点の座標
                double[,] coord_c = new double[nno, ndim];
                //CNodeAry.CNodeSeg ns_c_val = valField.GetNodeSeg(ELSEG_TYPE.CORNER, true, world);
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
                    if (Math.Abs(rotAngle) >= Constants.PrecisionLowerLimit)
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

                    // 節点の値を取って来る
                    //es_c_va.GetNodes(ielem, no_c);
                    //for (uint inoes = 0; inoes < nno; inoes++)
                    //{
                    //    Complex[] tmpval = null;
                    //    ns_c_val.GetValue(no_c[inoes], out tmpval);
                    //    System.Diagnostics.Debug.Assert(tmpval.Length == 1);
                    //    value_c[inoes] = tmpval[0];
                    //}
                    // 界の微分値を計算
                    // 界の微分値は一次三角形要素の場合、要素内で一定値
                    KrdLab.clapack.Complex dFdX = 0.0;
                    KrdLab.clapack.Complex dFdY = 0.0;
                    for (int inoes = 0; inoes < nno; inoes++)
                    {
                        uint iNodeNumber = no_c[inoes];
                        if (!to_no_all.ContainsKey(iNodeNumber))
                        {
                            System.Diagnostics.Debug.Assert(false);
                            continue;
                        }
                        uint inoGlobal = to_no_all[iNodeNumber];
                        KrdLab.clapack.Complex fVal = fVec[inoGlobal];
                        dFdX += dldx[inoes, 0] * fVal;
                        dFdY += dldx[inoes, 1] * fVal;
                    }
                    // 格納
                    for (int inoes = 0; inoes < nno; inoes++)
                    {
                        uint iNodeNumber = no_c[inoes];
                        if (!to_no_all.ContainsKey(iNodeNumber))
                        {
                            System.Diagnostics.Debug.Assert(false);
                            continue;
                        }
                        uint inoGlobal = to_no_all[iNodeNumber];

                        
                        //dFdXVec[inoGlobal] += dFdX;
                        //dFdYVec[inoGlobal] += dFdY;
                        // Note:
                        //  TEzモードのとき -dHz/dx = jωDy
                        dFdXVec[inoGlobal] += dFdX * area;
                        dFdYVec[inoGlobal] += dFdY * area;


                        /*境界に限ればこの計算は誤差がある
                        // 媒質を考慮する
                        double media_P11 = media.P[1, 1];
                        // pyy (dF/dx)を格納
                        //  TEzのとき pyy (dF/dx) = (1/εyy)dHz/dx = -jωEy
                        //dFdXVec[inoGlobal] += media_P11 * dFdX;
                        //dFdYVec[inoGlobal] += media_P11 * dFdY;
                        dFdXVec[inoGlobal] += media_P11 * dFdX * area;
                        dFdYVec[inoGlobal] += media_P11 * dFdY * area;
                         */

                        if (nodeElemCntH.ContainsKey(inoGlobal))
                        {
                            nodeElemCntH[inoGlobal]++;
                            /*境界に限ればこの計算は誤差がある
                            // 媒質パラメータpyyの逆数を格納
                            //nodeMediaP11Sum[inoGlobal] += (1.0 / media_P11);
                            nodeMediaP11Sum[inoGlobal] += (1.0 / media_P11) * area;
                             */
                            // 面積を格納
                            nodeAreaSum[inoGlobal] += area;
                        }
                        else
                        {
                            nodeElemCntH.Add(inoGlobal, 1);
                            /*境界に限ればこの計算は誤差がある
                            // 媒質パラメータpyyの逆数を格納
                            //nodeMediaP11Sum.Add(inoGlobal, (1.0 / media_P11));
                            nodeMediaP11Sum.Add(inoGlobal, (1.0 / media_P11) * area);
                             */
                            nodeAreaSum.Add(inoGlobal, area);
                        }
                    }
                }
            }
            for (uint i = 0; i < dFdXVec.Length; i++)
            {
                
                //int cnt = nodeElemCntH[i];
                //dFdXVec[i] /= cnt;
                //dFdYVec[i] /= cnt;
                double areaSum = nodeAreaSum[i];
                dFdXVec[i] = (dFdXVec[i] / areaSum);
                dFdYVec[i] = (dFdYVec[i] / areaSum);

                /*境界に限ればこの計算は誤差がある
                // 媒質を考慮する
                // 媒質パラメータpyyの平均
                //int cnt = nodeElemCntH[i];
                double areaSum = nodeAreaSum[i];
                
                //double mediaP11 = 1.0 / (nodeMediaP11Sum[i] / cnt);
                double mediaP11 = 1.0 / (nodeMediaP11Sum[i] / areaSum);
                //System.Diagnostics.Debug.Assert((1.0 / mediaP11) >= 1.0);
                // dF/dxの平均
                //dFdXVec[i] = (dFdXVec[i] / cnt) / mediaP11;
                //dFdYVec[i] = (dFdYVec[i] / cnt) / mediaP11;
                dFdXVec[i] = (dFdXVec[i] / areaSum) / mediaP11;
                dFdYVec[i] = (dFdYVec[i] / areaSum) / mediaP11;
                 */
            }
        }

        /// <summary>
        /// FEM行列のバンドマトリクス情報を取得する
        /// </summary>
        /// <param name="matPattern">非０パターンの配列</param>
        /// <param name="rowcolSize">行数=列数</param>
        /// <param name="subdiaSize">subdiagonalのサイズ</param>
        /// <param name="superdiaSize">superdiagonalのサイズ</param>
        public static void GetBandMatrixSubDiaSizeAndSuperDiaSize(
            bool[,] matPattern,
            out int rowcolSize,
            out int subdiaSize,
            out int superdiaSize)
        {
            rowcolSize = matPattern.GetLength(0);

            // subdiaサイズ、superdiaサイズを取得する
            subdiaSize = 0;
            superdiaSize = 0;
            // Note: c == rowcolSize - 1は除く
            for (int c = 0; c < rowcolSize - 1; c++)
            {
                if (subdiaSize >= (rowcolSize - 1 - c))
                {
                    break;
                }
                int cnt = 0;
                for (int r = rowcolSize - 1; r >= c + 1; r--)
                {
                    // 非０要素が見つかったら抜ける
                    if (matPattern[r, c])
                    {
                        cnt = r - c;
                        break;
                    }
                }
                if (cnt > subdiaSize)
                {
                    subdiaSize = cnt;
                }
            }
            // Note: c == 0は除く
            for (int c = rowcolSize - 1; c >= 1; c--)
            {
                if (superdiaSize >= c)
                {
                    break;
                }
                int cnt = 0;
                for (int r = 0; r <= c - 1; r++)
                {
                    // 非０要素が見つかったら抜ける
                    if (matPattern[r, c])
                    {
                        cnt = c - r;
                        break;
                    }
                }
                if (cnt > superdiaSize)
                {
                    superdiaSize = cnt;
                }
            }
            //System.Diagnostics.Debug.WriteLine("rowcolSize: {0} subdiaSize: {1} superdiaSize: {2}", rowcolSize, subdiaSize, superdiaSize);
        }

        /// <summary>
        /// バンド幅を縮小する
        /// </summary>
        /// <param name="matPattern"></param>
        /// <returns></returns>
        public static void GetOptBandMatNodes(bool[,] matPattern, out IList<int> optNodesGlobal, out Dictionary<int, int> toOptNodes)
        {
            // バンド幅を縮小する

            // 元の行列のrow, colのインデックスは節点番号と同じとする
            IList<int> sortedNodes = new List<int>();
            for (int i = 0; i < matPattern.GetLength(0); i++)
            {
                sortedNodes.Add(i);
            }

            // 非０要素出現順に節点番号を格納
            IList<int> optNodes = new List<int>();
            Queue<int> chkQueue = new Queue<int>();
            int[] remainNodes = new int[matPattern.GetLength(0)];
            for (int i = 0; i < matPattern.GetLength(0); i++)
            {
                remainNodes[i] = i;
            }
            while (optNodes.Count < sortedNodes.Count)
            {
                // 飛び地領域対応
                for (int rIndex = 0; rIndex < remainNodes.Length; rIndex++)
                {
                    int i = remainNodes[rIndex];
                    if (i == -1) continue;
                    //System.Diagnostics.Debug.Assert(!optNodes.Contains(i));
                    chkQueue.Enqueue(i);
                    remainNodes[rIndex] = -1;
                    break;
                }
                while (chkQueue.Count > 0)
                {
                    int i = chkQueue.Dequeue();
                    optNodes.Add(i);
                    for (int rIndex = 0; rIndex < remainNodes.Length; rIndex++)
                    {
                        int j = remainNodes[rIndex];
                        if (j == -1) continue;
                        //System.Diagnostics.Debug.Assert(i != j);
                        if (matPattern[i, j])
                        {
                            //System.Diagnostics.Debug.Assert(!optNodes.Contains(j) && !chkQueue.Contains(j));
                            chkQueue.Enqueue(j);
                            remainNodes[rIndex] = -1;
                        }
                    }
                }
            }
            optNodesGlobal = new List<int>();
            toOptNodes = new Dictionary<int, int>();
            foreach (int i in optNodes)
            {
                int ino = sortedNodes[i];
                optNodesGlobal.Add(ino);
                toOptNodes.Add(ino, optNodesGlobal.Count - 1);
            }
        }
    }
}
