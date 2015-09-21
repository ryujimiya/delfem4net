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

namespace wg2d_periodic_eigen
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
    /// メインロジック
    /// </summary>
    class MainLogic : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        delegate bool SetProblemProcDelegate(
            int probNo,
            double WaveguideWidth,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double FreqDelta,
            ref double GraphFreqInterval,
            ref double MinBeta,
            ref double MaxBeta,
            ref double GraphBetaInterval,
            ref double minEffN,
            ref double maxEffN,
            ref double minWaveNum,
            ref double maxWaveNum,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref bool isSolveEigenItr,
            ref int CalcModeIndex,
            ref bool IsSVEA,
            ref bool IsModeTrace,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            );

        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        private const double pi = WgUtil.pi;
        private const double c0 = WgUtil.c0;
        private const double myu0 = WgUtil.myu0;
        private const double eps0 = WgUtil.eps0;
        /// <summary>
        /// 問題の数
        /// </summary>
        private const int ProbCnt = 6;
        /// <summary>
        /// 最初に表示する問題番号
        /// </summary>
        private const int DefProbNo = 5;//3;//1;
        /// <summary>
        /// ウィンドウの位置X
        /// </summary>
        private const int DefWinPosX = 200;
        /// <summary>
        /// ウィンドウの位置Y
        /// </summary>
        private const int DefWinPosY = 200;
        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private const int DefWinWidth = 670;//420;
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private const int DefWinHeight = 500;//380;

        /// <summary>
        /// 緩慢変化包絡線近似？
        ///    true  緩慢変化包絡線近似 Φ = φ(x, y) exp(-jβx) と置く方法
        ///    false Φを直接解く方法(exp(-jβd)を固有値として扱う)
        /// </summary>
        private const bool DefIsSVEA = true;  // Φ = φ(x, y) exp(-jβx) と置く方法
        //private const bool DefIsSVEA = false; // Φを直接解く方法(exp(-jβd)を固有値として扱う

        /////////////////////////////////////////////////
        // 変数
        /////////////////////////////////////////////////
        /// <summary>
        /// 破棄された？
        /// </summary>
        private bool Disposed = false;
        /// <summary>
        /// カメラ
        /// </summary>
        private CCamera Camera = null;
        /// <summary>
        /// マウス移動量X方向
        /// </summary>
        private double MovBeginX = 0;
        /// <summary>
        /// マウス移動量Y方向
        /// </summary>
        private double MovBeginY = 0;
        /// <summary>
        /// キー入力修飾子
        /// </summary>
        //private int Modifier = 0;
        /// <summary>
        /// アニメーション中？
        /// </summary>
        private bool IsAnimation = true;
        /// <summary>
        /// 描画オブジェクトアレイ
        /// </summary>
        private CDrawerArrayField DrawerAry = null;
        /// <summary>
        /// フィールドのワールド座標系
        /// </summary>
        private CFieldWorld World = null;
        /// <summary>
        /// フィールドの値のセット操作を行うオブジェクト
        /// </summary>
        //private CFieldValueSetter FieldValueSetter = null;
        /// <summary>
        /// リニアシステム（複素数版)
        /// </summary>
        //private CZLinearSystem Ls = null;
        /// <summary>
        /// プリコンディショナ―（複素数版)
        /// </summary>
        //private CZPreconditioner_ILU Prec = null;

        /// <summary>
        /// 問題番号
        /// </summary>
        private int ProbNo = 0;
        /// <summary>
        /// 計算対象周波数
        /// </summary>
        private int FreqIndex = 0;
        /// <summary>
        /// 開始規格化周波数
        /// </summary>
        private double NormalizedFreq1 = 1.0;
        /// <summary>
        /// 終了規格化周波数
        /// </summary>
        private double NormalizedFreq2 = 2.0;
        /// <summary>
        /// 周波数間隔
        /// </summary>
        private double FreqDelta = 0.01;
        /// <summary>
        /// グラフの周波数目盛間隔
        /// </summary>
        private double GraphFreqInterval = 0.2;
        /// <summary>
        /// 伝搬定数の最小値
        /// </summary>
        private double MinBeta = 0.0;
        /// <summary>
        /// 伝搬定数の最大値
        /// </summary>
        private double MaxBeta = 1.0;
        /// <summary>
        /// グラフの伝搬定数目盛間隔
        /// </summary>
        private double GraphBetaInterval = 0.2;
        /// <summary>
        /// 最小屈折率
        /// </summary>
        private double MinEffN = 0.0;
        /// <summary>
        /// 最大屈折率
        /// </summary>
        private double MaxEffN = 1.0;
        /// <summary>
        /// 考慮する波数ベクトルの最小値
        /// </summary>
        private double MinWaveNum = 0.0;
        /// <summary>
        /// 考慮する波数ベクトルの最大値
        /// </summary>
        private double MaxWaveNum = 0.5;
        /// <summary>
        /// 波のモード区分
        /// </summary>
        private WgUtil.WaveModeDV WaveModeDv = WgUtil.WaveModeDV.TE;

        /// <summary>
        /// 導波管の幅
        /// </summary>
        private double WaveguideWidth = 1.0;
        /// <summary>
        // 格子定数
        /// </summary>
        private double LatticeA = 0.0;
        /// <summary>
        // 周期距離
        /// </summary>
        private double PeriodicDistance = 0.0;
        /// <summary>
        // ループのフィールドID
        /// </summary>
        private uint FieldLoopId = 0;
        /// <summary>
        // 値のフィールドID
        /// </summary>
        private uint FieldValId = 0;
        /// <summary>
        // 強制境界のフィールドID
        /// </summary>
        private uint FieldForceBcId = 0;
        /// <summary>
        // ポート１の境界フィールドID
        /// </summary>
        private uint FieldPortBcId1 = 0;
        /// <summary>
        // ポート２の境界フィールドID
        /// </summary>
        private uint FieldPortBcId2 = 0;
        /// <summary>
        // 媒質リスト
        /// </summary>
        IList<MediaInfo> Medias = new List<MediaInfo>();
        /// <summary>
        // ループID→ループの情報のマップ
        /// </summary>
        Dictionary<uint, wg2d.World.Loop> LoopDic = new Dictionary<uint, wg2d.World.Loop>();
        /// <summary>
        // 辺ID→辺の情報のマップ
        /// </summary>
        Dictionary<uint, wg2d.World.Edge> EdgeDic = new Dictionary<uint, wg2d.World.Edge>();

        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        private int Frame = 0;
        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        private int Timebase = 0;
        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        private string Stringfps = "";
        /// <summary>
        /// フレーム単位描画用タイマー
        /// </summary>
        private System.Windows.Forms.Timer MyTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// 伝搬定数(各周波数に対するβ)のリスト
        /// </summary>
        private IList<Complex> EigenValueList = null;

        /// <summary>
        /// 図面を表示する？
        /// </summary>
        private bool IsCadShow = false;
        /// <summary>
        /// 図面表示用描画オブジェクトアレイ
        /// </summary>
        private CDrawerArray CadDrawerAry = null;
        /// <summary>
        /// フォトニック結晶導波路?
        /// </summary>
        private bool IsPCWaveguide = false;
        /// <summary>
        /// フォトニック結晶導波路のポート境界上節点番号リストのリスト
        /// </summary>
        private IList<IList<uint>> PCWaveguidePorts = new List<IList<uint>>();
        /// <summary>
        /// 取得するモードのインデックス
        /// </summary>
        private int CalcModeIndex = 0;
        /// <summary>
        /// 反復で解く？
        /// </summary>
        private bool IsSolveEigenItr = false;
        /// <summary>
        /// 緩慢変化包絡線近似？
        ///    true  緩慢変化包絡線近似 Φ = φ(x, y) exp(-jβx) と置く方法
        ///    false Φを直接解く方法(exp(-jβd)を固有値として扱う)
        /// </summary>
        private bool IsSVEA = DefIsSVEA;
        /// <summary>
        /// モード追跡する?
        /// </summary>
        private bool IsModeTrace = true;
        /// <summary>
        /// 前回の固有モードベクトル
        /// </summary>
        private KrdLab.clapack.Complex[] PrevModalVec = null;
        /// <summary>
        /// 結合長を計算する？
        /// </summary>
        private bool IsCalcCouplingLength = false;
        /// <summary>
        /// 界の絶対値を表示する？
        /// </summary>
        private bool IsShowAbsField = false;
        /// <summary>
        /// 波数ベクトルの分散特性を表示する？
        /// </summary>
        private bool IsWaveNumberGraph = true;
        /// タイマーのタスク処理中？
        /// </summary>
        private bool IsTimerProcRun = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainLogic()
        {
            Disposed = false;
            Camera = new CCamera();
            DrawerAry = new CDrawerArrayField();
            World = new CFieldWorld();
            //FieldValueSetter = new CFieldValueSetter();
            //Ls = new CZLinearSystem();
            //Prec = new CZPreconditioner_ILU();
            EigenValueList = new List<Complex>();
            CadDrawerAry = new CDrawerArray();

            // Glutのアイドル時処理でなく、タイマーで再描画イベントを発生させる
            MyTimer.Tick += (sender, e) =>
            {
                if (IsTimerProcRun)
                {
                    return;
                }
                IsTimerProcRun = true;
                if (IsAnimation && FreqIndex != -1 && !IsCadShow)
                {
                    // 問題を解く
                    solveProblem(
                        ProbNo,
                        ref FreqIndex,
                        NormalizedFreq1,
                        NormalizedFreq2,
                        FreqDelta,
                        (FreqIndex == 0), //false,
                        WaveguideWidth,
                        WaveModeDv,
                        IsPCWaveguide,
                        LatticeA,
                        PeriodicDistance,
                        PCWaveguidePorts,
                        IsSolveEigenItr,
                        CalcModeIndex,
                        IsSVEA,
                        IsModeTrace,
                        ref PrevModalVec,
                        IsShowAbsField,
                        MinEffN,//MinBeta,
                        MaxEffN,//MaxBeta,
                        MinWaveNum,
                        MaxWaveNum,
                        ref World,
                        FieldValId,
                        FieldLoopId,
                        FieldForceBcId,
                        FieldPortBcId1,
                        FieldPortBcId2,
                        Medias,
                        LoopDic,
                        EdgeDic,
                        ref IsCalcCouplingLength,
                        ref EigenValueList,
                        ref DrawerAry,
                        Camera
                        );
                    if (FreqIndex != -1)
                    {
                        FreqIndex++;
                    }
                    //DEBUG
                    //Glut.glutPostRedisplay();
                    {
                        // POSTだとメッセージが１つにまとめられる場合がある？
                        // 直接描画する
                        int[] viewport = new int[4];
                        Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                        int winW = viewport[2];
                        int winH = viewport[3];
                        Camera.SetWindowAspect((double)winW / winH);
                        Gl.glViewport(0, 0, winW, winH);
                        Gl.glMatrixMode(Gl.GL_PROJECTION);
                        Gl.glLoadIdentity();
                        DelFEM4NetCom.View.DrawerGlUtility.SetProjectionTransform(Camera);
                        myGlutDisplay();
                    }
                }
                IsTimerProcRun = false;
            };
            //MyTimer.Interval = 1000 / 60;
            MyTimer.Interval = 1000 / 10;
            //MyTimer.Interval = 2000;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MainLogic()
        {
            Dispose(false);
        }

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }
            Disposed = true;
            MyTimer.Enabled = false;
            if (DrawerAry != null)
            {
                DrawerAry.Clear();
                DrawerAry.Dispose();
                DrawerAry = null;
            }
            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }
            if (World != null)
            {
                World.Clear();
                World.Dispose();
                World = null;
            }
            //if (FieldValueSetter != null)
            //{
            //    FieldValueSetter.Clear();
            //    FieldValueSetter.Dispose();
            //    FieldValueSetter = null;
            //}
            //if (Ls != null)
            //{
            //    Ls.Clear();
            //    Ls.Dispose();
            //    Ls = null;
            //}
            //if (Prec != null)
            //{
            //    Prec.Clear();
            //    Prec.Dispose();
            //    Prec = null;
            //}
            if (CadDrawerAry != null)
            {
                CadDrawerAry.Clear();
                CadDrawerAry.Dispose();
                CadDrawerAry = null;
            }
        }

        /// <summary>
        /// リソース破棄
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 処理を実行する
        /// </summary>
        public void Run()
        {
            // Initailze GLUT
            Glut.glutInitWindowPosition(DefWinPosX, DefWinPosY);
            Glut.glutInitWindowSize(DefWinWidth, DefWinHeight);
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            int argc = commandLineArgs.Length;
            StringBuilder[] argv = new StringBuilder[argc];
            for (int i = 0; i < argc; i++)
            {
                argv[i] = new StringBuilder(commandLineArgs[i]);
            }
            Glut.glutInit(ref argc, argv);
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_RGBA | Glut.GLUT_DEPTH);
            Glut.glutCreateWindow("FEM View");

            // Set callback function
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutSpecialFunc(myGlutSpecial);
            //Glut.glutIdleFunc(myGlutIdle);

            // 最初の問題を作成し、最初の周波数で解く
            ProbNo = DefProbNo;
            FreqIndex = 0;
            // 問題を作成
            setProblem();
            IsTimerProcRun = false;
            MyTimer.Enabled = true;

            // Enter main loop
            Glut.glutMainLoop();
        }

        /// <summary>
        /// Glut : リサイズイベントハンドラ
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void myGlutResize(int w, int h)
        {
            Camera.SetWindowAspect((double)w / h);
            Gl.glViewport(0, 0, w, h);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetProjectionTransform(Camera);
            Glut.glutPostRedisplay();
        }

        /// <summary>
        /// Glut : ディスプレイイベントハンドラ
        /// </summary>
        private void myGlutDisplay()
        {
            Gl.glClearColor(0.2f, 0.7f, 0.7f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glPolygonOffset(1.1f, 4.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            DelFEM4NetCom.GlutUtility.ShowBackGround();
            if (IsCadShow)
            {
                CadDrawerAry.Draw();
            }
            else
            {
                DrawerAry.Draw();
                drawEigenResults(ProbNo);
            }
            DelFEM4NetCom.GlutUtility.ShowFPS(ref Frame, ref Timebase, ref Stringfps);
            Glut.glutSwapBuffers();
        }

        /// <summary>
        /// Glut：アイドル時イベントハンドラ
        /// </summary>
        private void myGlutIdle()
        {
            //早すぎるので、タイマーのイベントで再描画イベントを発生させるようにした
            //Glut.glutPostRedisplay();
        }

        /// <summary>
        /// Glut：マウスドラッグ(マウスボタンを押したまま移動)イベントハンドラ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void myGlutMotion(int x, int y)
        {
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            double movEndX = (2.0 * x - winW) / winW;
            double movEndY = (winH - 2.0 * y) / winH;
            Camera.MousePan(MovBeginX, MovBeginY, movEndX, movEndY);
            MovBeginX = movEndX;
            MovBeginY = movEndY;
            Glut.glutPostRedisplay();
        }

        /// <summary>
        /// Glut：マウスクリックイベントハンドラ
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void myGlutMouse(int button, int state, int x, int y)
        {
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            MovBeginX = (2.0 * x - winW) / winW;
            MovBeginY = (winH - 2.0 * y) / winH;
        }

        /// <summary>
        /// Glut : キー入力イベントハンドラ
        /// </summary>
        /// <param name="key"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void myGlutKeyboard(byte key, int x, int y)
        {
            if (key == getSingleByte('q')
                || key == getSingleByte('Q')
                || key == 27  // ESC
                )
            {
                System.Environment.Exit(0);
            }
            else if (key == getSingleByte('a'))
            {
                IsAnimation = !IsAnimation;
                Console.WriteLine("IsAnimation: {0}", IsAnimation);
                System.Diagnostics.Debug.WriteLine("IsAnimation: {0}", IsAnimation);
            }
            else if (key == getSingleByte('f'))
            {
                IsShowAbsField = !IsShowAbsField;
                Console.WriteLine("IsShowAbsField: {0}", IsShowAbsField);
                System.Diagnostics.Debug.WriteLine("IsShowAbsField: {0}", IsShowAbsField);
            }
            else if (key == getSingleByte('g'))
            {
                IsWaveNumberGraph = !IsWaveNumberGraph;
                Console.WriteLine("IsWaveNumberGraph: {0}", IsWaveNumberGraph);
                System.Diagnostics.Debug.WriteLine("IsWaveNumberGraph: {0}", IsWaveNumberGraph);
            }
            else if (key == getSingleByte('c'))
            {
                IsCadShow = !IsCadShow;
                Console.WriteLine("IsCadShow: {0}", IsCadShow);
                System.Diagnostics.Debug.WriteLine("IsCadShow: {0}", IsCadShow);
                // 問題を作成
                setProblem();
            }
            else if (key == getSingleByte(' '))
            {
                // 問題を変える
                ProbNo++;
                if (ProbNo == ProbCnt) ProbNo = 0;
                Console.WriteLine("Problem No: {0}", ProbNo);
                System.Diagnostics.Debug.WriteLine("Problem No: {0}", ProbNo);
                CadDrawerAry.Clear();
                DrawerAry.Clear();
                // 周波数はリセット
                FreqIndex = -1;
                EigenValueList.Clear();
                // 問題を作成
                setProblem();
                FreqIndex = 0;
            }
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DrawerGlUtility.SetProjectionTransform(Camera);
            Glut.glutPostRedisplay();
        }

        /// <summary>
        /// 文字をバイトデータへ変換する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static byte getSingleByte(char c)
        {
            Encoding enc = Encoding.ASCII;
            byte[] bytes = enc.GetBytes(new char[] { c });
            return bytes[0];
        }

        /// <summary>
        /// 特殊キーのイベントハンドラ
        /// </summary>
        /// <param name="key"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void myGlutSpecial(int key, int x, int y)
        {
            if (key == Glut.GLUT_KEY_PAGE_UP)
            {
                if ((Glut.glutGetModifiers() & Glut.GLUT_ACTIVE_SHIFT) == Glut.GLUT_ACTIVE_SHIFT)
                {
                    if (Camera.IsPers())
                    {
                        double tmp_fov_y = Camera.GetFovY() + 10.0;
                        Camera.SetFovY(tmp_fov_y);
                    }
                }
                else
                {
                    double tmp_scale = Camera.GetScale() * 0.9;
                    Camera.SetScale(tmp_scale);
                }
            }
            else if (key == Glut.GLUT_KEY_PAGE_DOWN)
            {
                if ((Glut.glutGetModifiers() & Glut.GLUT_ACTIVE_SHIFT) == Glut.GLUT_ACTIVE_SHIFT)
                {
                    if (Camera.IsPers())
                    {
                        double tmp_fov_y = Camera.GetFovY() - 10.0;
                        Camera.SetFovY(tmp_fov_y);
                    }
                }
                else
                {
                    double tmp_scale = Camera.GetScale() * 1.111;
                    Camera.SetScale(tmp_scale);
                }
            }
            else if (key == Glut.GLUT_KEY_HOME)
            {
                Camera.Fit();
            }
            else if (key == Glut.GLUT_KEY_END)
            {
                if (Camera.IsPers())
                {
                    Camera.SetIsPers(false);
                }
                else
                {
                    Camera.SetIsPers(true);
                }
            }
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DrawerGlUtility.SetProjectionTransform(Camera);
            Glut.glutPostRedisplay();
        }

        /// <summary>
        /// 問題を設定する
        /// </summary>
        /// <returns></returns>
        private bool setProblem()
        {
            bool ret = setProblem(
                ProbNo,
                WaveguideWidth,
                ref NormalizedFreq1,
                ref NormalizedFreq2,
                ref FreqDelta,
                ref GraphFreqInterval,
                ref MinBeta,
                ref MaxBeta,
                ref GraphBetaInterval,
                ref MinEffN,
                ref MaxEffN,
                ref MinWaveNum,
                ref MaxWaveNum,
                ref WaveModeDv,
                ref IsPCWaveguide,
                ref LatticeA,
                ref PeriodicDistance,
                ref PCWaveguidePorts,
                ref IsSolveEigenItr,
                ref CalcModeIndex,
                ref IsSVEA,
                ref IsModeTrace,
                ref World,
                ref FieldValId,
                ref FieldLoopId,
                ref FieldForceBcId,
                ref FieldPortBcId1,
                ref FieldPortBcId2,
                ref Medias,
                ref LoopDic,
                ref EdgeDic,
                ref IsCadShow,
                ref CadDrawerAry,
                ref Camera
                );
            return ret;
        }

        /// <summary>
        /// 問題を設定する
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="NormalizedFreq1"></param>
        /// <param name="NormalizedFreq2"></param>
        /// <param name="FreqDelta"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinBeta"></param>
        /// <param name="MaxBeta"></param>
        /// <param name="GraphBetaInterval"></param>
        /// <param name="minEffN"></param>
        /// <param name="maxEffN"></param>
        /// <param name="minWaveNum"></param>
        /// <param name="maxWaveNum"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="IsSolveEigenItr"></param>
        /// <param name="CalcModeIndex"></param>
        /// <param name="IsSVEA"></param>
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
        private static bool setProblem(
            int probNo,
            double WaveguideWidth,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double FreqDelta,
            ref double GraphFreqInterval,
            ref double MinBeta,
            ref double MaxBeta,
            ref double GraphBetaInterval,
            ref double minEffN,
            ref double maxEffN,
            ref double minWaveNum,
            ref double maxWaveNum,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref bool IsSolveEigenItr,
            ref int CalcModeIndex,
            ref bool IsSVEA,
            ref bool IsModeTrace,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            bool success = false;
            
            // 媒質リストのクリア
            Medias.Clear();
            // ワールド座標系ループ情報のクリア
            LoopDic.Clear();
            // ワールド座標系辺情報のクリア
            EdgeDic.Clear();

            // 屈折率の最小値
            minWaveNum = 0.0;
            // 屈折率の最大値
            maxWaveNum = 1.0;
            // 考慮する波数ベクトルの最小値
            minWaveNum = 0.0;
            // 考慮する波数ベクトルの最大値
            maxWaveNum = 0.5;

            // フォトニック結晶導波路解析用
            IsPCWaveguide = false;
            latticeA = 0.0;
            periodicDistance = 0.0;
            PCWaveguidePorts.Clear();
            // 反復で解く？
            IsSolveEigenItr = true;
            // 基本モードを計算する
            CalcModeIndex = 0;
            // 緩慢変化包絡線近似？
            IsSVEA = DefIsSVEA;
            // モード追跡する?
            IsModeTrace = true;

            SetProblemProcDelegate func = null;

            //isCadShow = false;
            try
            {
                if (probNo == 0)
                {
                    // 直線導波管
                    func = Problem00.SetProblem;
                }
                else if (probNo == 1)
                {
                    // PC欠陥導波路
                    func = Problem01.SetProblem;
                }
                else if (probNo == 2)
                {
                    // PC欠陥導波路 2チャンネル
                    func = Problem02.SetProblem;
                }
                else if (probNo == 3)
                {
                    // 三角形格子 PC欠陥導波路
                    func = Problem03.SetProblem;
                }
                else if (probNo == 4)
                {
                    // 三角形格子　PC欠陥導波路(斜め領域)
                    func = Problem04.SetProblem;
                }
                else if (probNo == 5)
                {
                    // 三角形格子 PC欠陥導波路 2チャンネル
                    func = Problem05.SetProblem;
                }
                else
                {
                    return success;
                }

                success = func(
                    probNo,
                    WaveguideWidth,
                    ref NormalizedFreq1,
                    ref NormalizedFreq2,
                    ref FreqDelta,
                    ref GraphFreqInterval,
                    ref MinBeta,
                    ref MaxBeta,
                    ref GraphBetaInterval,
                    ref minEffN,
                    ref maxEffN,
                    ref minWaveNum,
                    ref maxWaveNum,
                    ref WaveModeDv,
                    ref IsPCWaveguide,
                    ref latticeA,
                    ref periodicDistance,
                    ref PCWaveguidePorts,
                    ref IsSolveEigenItr,
                    ref CalcModeIndex,
                    ref IsSVEA,
                    ref IsModeTrace,
                    ref World,
                    ref FieldValId,
                    ref FieldLoopId,
                    ref FieldForceBcId,
                    ref FieldPortBcId1,
                    ref FieldPortBcId2,
                    ref Medias,
                    ref LoopDic,
                    ref EdgeDic,
                    ref isCadShow,
                    ref CadDrawerAry,
                    ref Camera);
                //success = true;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            return success;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <param name="initFlg">カメラ初期化フラグ</param>
        /// <returns></returns>
        private bool solveProblem(bool initFlg)
        {
            bool ret = solveProblem(
                ProbNo,
                ref FreqIndex,
                NormalizedFreq1,
                NormalizedFreq2,
                FreqDelta,
                initFlg,
                WaveguideWidth,
                WaveModeDv,
                IsPCWaveguide,
                LatticeA,
                PeriodicDistance,
                PCWaveguidePorts,
                IsSolveEigenItr,
                CalcModeIndex,
                IsSVEA,
                IsModeTrace,
                ref PrevModalVec,
                IsShowAbsField,
                MinEffN,//MinBeta,
                MaxEffN,//MaxBeta,
                MinWaveNum,
                MaxWaveNum,
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPortBcId1,
                FieldPortBcId2,
                Medias,
                LoopDic,
                EdgeDic,
                ref IsCalcCouplingLength,
                ref EigenValueList,
                ref DrawerAry,
                Camera
                );
            return ret;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="freqIndex"></param>
        /// <param name="NormalizedFreq1"></param>
        /// <param name="NormalizedFreq2"></param>
        /// <param name="FreqDelta"></param>
        /// <param name="initFlg"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="IsSolveEigenItr"></param>
        /// <param name="CalcModeIndex"></param>
        /// <param name="isSVEA"></param>
        /// <param name="PrevModalVec"></param>
        /// <param name="IsShowAbsField"></param>
        /// <param name="minEffN"></param>
        /// <param name="maxEffN"></param>
        /// <param name="minWaveNum"></param>
        /// <param name="maxWaveNum"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPortBcId1"></param>
        /// <param name="FieldPortBcId2"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="EigenValueList"></param>
        /// <param name="DrawerAry"></param>
        /// <param name="Camera"></param>
        /// <returns></returns>
        private static bool solveProblem(
            int probNo,
            ref int freqIndex,
            double NormalizedFreq1,
            double NormalizedFreq2,
            double FreqDelta,
            bool initFlg,
            double WaveguideWidth,
            WgUtil.WaveModeDV WaveModeDv,
            bool IsPCWaveguide,
            double latticeA,
            double periodicDistance,
            IList<IList<uint>> PCWaveguidePorts,
            bool IsSolveEigenItr,
            int CalcModeIndex,
            bool isSVEA,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            bool IsShowAbsField,
            double minEffN,
            double maxEffN,
            double minWaveNum,
            double maxWaveNum,
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            uint FieldPortBcId1,
            uint FieldPortBcId2,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCalcCouplingLength,
            ref IList<Complex> EigenValueList,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            // 結合長を求める？
            double betaDiffForCoupler = 0.0;
            isCalcCouplingLength = false; // 求めない
            if (!isSVEA)
            {
                if (probNo == 2 || probNo == 5)
                {
                    //isCalcCouplingLength = false; // 求めない
                    isCalcCouplingLength = true; // 求める
                }
            }

            double minBeta = minEffN;
            double maxBeta = maxEffN;
            bool success = false;
            bool showException = true;
            try
            {
                // モード追跡する？
                //bool isModeTrace = true;
                if (!IsPCWaveguide)
                {
                    isModeTrace = false;
                }

                if (freqIndex == 0)
                {
                    PrevModalVec = null;
                }

                // 規格化周波数
                double normalizedFreq = getNormalizedFreq(
                    ref freqIndex,
                    NormalizedFreq1,
                    NormalizedFreq2,
                    FreqDelta);
                if (freqIndex == -1)
                {
                    return success;
                }
                // 波数
                double k0 = 0.0;
                if (IsPCWaveguide)
                {
                    k0 = normalizedFreq * 2.0 * pi / latticeA;
                    System.Diagnostics.Debug.WriteLine("a/λ:{0}", normalizedFreq);
                }
                else
                {
                    k0 = normalizedFreq * pi / WaveguideWidth;
                    System.Diagnostics.Debug.WriteLine("2W/λ:{0}", normalizedFreq);
                }
                // 波長
                double waveLength = 2.0 * pi / k0;
                // 反復計算の初期値
                double initialBeta = 0.0;
                /*
                if (freqIndex != 0)
                {
                    int workFreqIndex = freqIndex - 1;
                    double prevNormalizedFreq = getNormalizedFreq(
                        ref workFreqIndex,
                        NormalizedFreq1,
                        NormalizedFreq2,
                        FreqDelta);
                    double prev_k0 = 0.0;
                    if (IsPCWaveguide)
                    {
                        prev_k0 = prevNormalizedFreq * 2.0 * pi / latticeA;
                    }
                    else
                    {
                        prev_k0 = prevNormalizedFreq * pi / WaveguideWidth;
                    }
                    initialBeta = EigenValueList[EigenValueList.Count - 1].Real * prev_k0;
                }
                 */

                // 全節点数を取得する
                uint node_cnt = 0;
                //node_cnt = WgUtilForPeriodicEigen.GetNodeCnt(World, FieldLoopId);
                double[][] coord_c_all = null;
                {
                    uint[] no_c_all_tmp = null;
                    Dictionary<uint, uint> to_no_all_tmp = null;
                    double[][] coord_c_all_tmp = null;
                    WgUtilForPeriodicEigen.GetLoopCoordList(World, FieldLoopId, out no_c_all_tmp, out to_no_all_tmp, out coord_c_all_tmp);
                    node_cnt = (uint)no_c_all_tmp.Length;

                    // 座標リストを節点番号順に並び替えて格納
                    coord_c_all = new double[node_cnt][];
                    for (int ino = 0; ino < node_cnt; ino++)
                    {
                        uint nodeNumber = no_c_all_tmp[ino];
                        double[] coord = coord_c_all_tmp[ino];
                        coord_c_all[nodeNumber] = coord;
                    }
                }

                System.Diagnostics.Debug.WriteLine("node_cnt: {0}", node_cnt);

                // 境界の節点リストを取得する
                uint[] no_c_all_fieldForceBcId = null;
                Dictionary<uint, uint> to_no_boundary_fieldForceBcId = null;
                if (FieldForceBcId != 0)
                {
                    WgUtil.GetBoundaryNodeList(World, FieldForceBcId, out no_c_all_fieldForceBcId, out to_no_boundary_fieldForceBcId);
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
                    if (FieldForceBcId != 0)
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
                for (uint nodeNumber = 0; nodeNumber < node_cnt; nodeNumber++)
                {
                    // 追加済み節点はスキップ
                    //if (toSorted.ContainsKey(nodeNumber)) continue;
                    // 境界1は除く
                    if (to_no_boundary_fieldPortBcId1.ContainsKey(nodeNumber)) continue;
                    // 境界2は除く
                    if (to_no_boundary_fieldPortBcId2.ContainsKey(nodeNumber)) continue;
                    if (FieldForceBcId != 0)
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
                    if (FieldForceBcId != 0)
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
                    false, // isYDirectionPeriodic: false
                    0.0, // rotAngle
                    null, // rotOrigin
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
                System.Diagnostics.Debug.WriteLine("isModeTrace: {0}, CalcModeIndex: {1}, IsSolveEigenItr: {2}", isModeTrace, CalcModeIndex, IsSolveEigenItr);
                // 逆行列を使う？
                //bool isUseInvMat = false; // 逆行列を使用しない
                bool isUseInvMat = true; // 逆行列を使用する
                System.Diagnostics.Debug.WriteLine("isUseInvMat: {0}", isUseInvMat);

                /*
                // 反復計算のときはモード追跡をOFFにする(うまくいかないときがあるので)
                if (IsSolveEigenItr && isModeTrace)
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
                            KMat[i + free_node_cnt * j] += KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                            CMat[i + free_node_cnt * j] += CMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                            MMat[i + free_node_cnt * j] += MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        }
                    }
                    for (int i = 0; i < boundary_node_cnt; i++)
                    {
                        for (int j = 0; j < free_node_cnt; j++)
                        {
                            KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                            CMat[i + free_node_cnt * j] += CMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                            MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                        }
                        for (int j = 0; j < boundary_node_cnt; j++)
                        {
                            KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                            CMat[i + free_node_cnt * j] += CMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                            MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
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
                        for (int j = 0; j < boundary_node_cnt; j++)
                        {
                            // [K21]
                            KMat[i + free_node_cnt * j] = KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                            // [K11] + [K22]
                            CMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j]
                                + KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                            // [K12]
                            MMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        }
                        for (int j = 0; j < inner_node_cnt; j++)
                        {
                            // [K20]
                            KMat[i + free_node_cnt * (j + boundary_node_cnt)] = KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (j + boundary_node_cnt)];
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
                            // [0]
                            KMat[(i + boundary_node_cnt) + free_node_cnt * j] = 0.0;
                            // [K01]
                            CMat[(i + boundary_node_cnt) + free_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * j];
                            // [K02]
                            MMat[(i + boundary_node_cnt) + free_node_cnt * j] = KMat0[(i + boundary_node_cnt) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
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
                KrdLab.clapack.Complex betamToSolve = 0.0;
                // 界ベクトルは全節点分作成
                KrdLab.clapack.Complex[] resVec = null;
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

                // 緩慢変化包絡線近似は反復計算の時だけしか対応できていない
                // 緩慢変化包絡線近似でない場合は、マルチモードと同じく２次一般化固有値問題を解く
                if (!IsSolveEigenItr || CalcModeIndex >= 2 || !isSVEA)
                {
                    /*
                    // マルチモードの場合
                    // 周期構造導波路固有値問題を２次一般化固有値問題として解く
                    solveAsQuadraticGeneralizedEigen(
                        CalcModeIndex,
                        k0,
                        KMat,
                        CMat,
                        MMat,
                        node_cnt,
                        free_node_cnt,
                        boundary_node_cnt,
                        sortedNodes,
                        toSorted,
                        to_no_boundary_fieldPortBcId1,
                        isModeTrace,
                        ref PrevModalVec,
                        IsPCWaveguide,
                        PCWaveguidePorts,
                        minBeta,
                        maxBeta,
                        out betamToSolve,
                        out resVec);
                     */

                    if (!isUseInvMat)
                    {
                        // マルチモードの場合
                        // 周期構造導波路固有値問題を２次一般化固有値問題として解く(実行列として解く)
                        solveAsQuadraticGeneralizedEigenWithRealMat(
                            CalcModeIndex,
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
                            to_no_boundary_fieldPortBcId1,
                            false, // isYDirectionPeriodic : false
                            coord_c_all,
                            IsPCWaveguide,
                            PCWaveguidePorts,
                            isModeTrace,
                            ref PrevModalVec,
                            minBeta,
                            maxBeta,
                            (2.0 * pi / periodicDistance), //k0, //1.0,
                            out betamToSolve,
                            out resVec);
                    }
                    else
                    {
                        // 逆行列を使用する方法
                        if (isSVEA)
                        {
                            System.Diagnostics.Debug.Assert(isSVEA == true);
                            // 周期構造導波路固有値問題を２次一般化固有値問題→標準固有値問題として解く(実行列として解く)
                            solveAsQuadraticGeneralizedEigenToStandardWithRealMat(
                                CalcModeIndex,
                                k0,
                                KMat,
                                CMat,
                                MMat,
                                node_cnt,
                                free_node_cnt,
                                boundary_node_cnt,
                                sortedNodes,
                                toSorted,
                                to_no_boundary_fieldPortBcId1,
                                IsPCWaveguide,
                                PCWaveguidePorts,
                                isModeTrace,
                                ref PrevModalVec,
                                minBeta,
                                maxBeta,
                                k0, //(2.0 * pi / periodicDistance), //k0, //1.0,
                                out betamToSolve,
                                out resVec);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(isSVEA == false);
                            solveNonSVEAModeAsQuadraticGeneralizedEigenWithRealMat(
                                CalcModeIndex,
                                periodicDistance,
                                k0,
                                KMat0,
                                true, // isPortBc2Reverse : true
                                node_cnt,
                                free_node_cnt0,
                                free_node_cnt,
                                boundary_node_cnt,
                                sortedNodes,
                                toSorted,
                                to_no_boundary_fieldPortBcId1,
                                false, // isYDirectionPeriodic : false
                                coord_c_all,
                                IsPCWaveguide,
                                PCWaveguidePorts,
                                isModeTrace,
                                ref PrevModalVec,
                                minBeta,
                                maxBeta,
                                (2.0 * pi / periodicDistance), //k0, //1.0,
                                isCalcCouplingLength,
                                out betamToSolve,
                                out resVec,
                                out betaDiffForCoupler);
                        }
                    }
                    if (betamToSolve.Real == 0.0 && betamToSolve.Imaginary == 0.0)
                    {
                        //Console.WriteLine("skipped calculation. No propagation mode exists At 2W /λ = {0}", normalizedFreq);
                        System.Diagnostics.Debug.WriteLine("skipped calculation. No propagation mode exists At 2W /λ = {0}", normalizedFreq);
                    }
                }
                else
                {
                    // 反復計算の初期値
                    betamToSolve = initialBeta;
                    // 高次モード？
                    bool isCalcSecondMode = (CalcModeIndex == 1) ? true : false;
                    // 周期構造導波路固有値問題を一般化固有値問題として解く(反復計算)
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
                        to_no_boundary_fieldPortBcId1,
                        IsPCWaveguide,
                        PCWaveguidePorts,
                        isCalcSecondMode,
                        isModeTrace,
                        ref PrevModalVec,
                        minBeta,
                        maxBeta,
                        ref betamToSolve,
                        out resVec);
                    if (betamToSolve.Real == 0.0 && betamToSolve.Imaginary == 0.0)
                    {
                        //Console.WriteLine("skipped calculation. No propagation mode exists At 2W /λ = {0}", normalizedFreq);
                        System.Diagnostics.Debug.WriteLine("skipped calculation. No propagation mode exists At 2W /λ = {0}", normalizedFreq);
                    }
                }

                // ポート境界1の節点の値を境界2にも格納する
                for (int i = 0; i < no_c_all_fieldPortBcId1.Length; i++)
                {
                    // 境界1の節点
                    uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[i];
                    // 境界1の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc1];

                    // 境界2の節点
                    uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[no_c_all_fieldPortBcId2.Length - 1 - i];
                    if (isSVEA)
                    {
                        // 緩慢変化包絡線近似の場合は、Φ2 = Φ1
                        resVec[nodeNumberPortBc2] = cvalue;
                    }
                    else
                    {
                        // 緩慢変化包絡線近似でない場合は、Φ2 = expA * Φ1
                        KrdLab.clapack.Complex expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betamToSolve * periodicDistance);
                        resVec[nodeNumberPortBc2] = expA * cvalue; // 直接Bloch境界条件を指定する場合
                    }
                }

                // 位相調整
                KrdLab.clapack.Complex phaseShift = 1.0;
                double maxAbs = double.MinValue;
                KrdLab.clapack.Complex fValueAtMaxAbs = 0.0;
                {
                    /*
                    for (int ino = 0; ino < no_c_all_fieldPortBcId1.Length; ino++)
                    {
                        uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[ino];
                        KrdLab.clapack.Complex cvalue = resVec[ino];
                        double abs = KrdLab.clapack.Complex.Abs(cvalue);
                        if (abs > maxAbs)
                        {
                            maxAbs = abs;
                            fValueAtMaxAbs = cvalue;
                        }
                    }
                     */
                    for (int ino = 0; ino < resVec.Length; ino++)
                    {
                        KrdLab.clapack.Complex cvalue = resVec[ino];
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

                //------------------------------------------------------------------
                // 計算結果の後処理
                //------------------------------------------------------------------
                // 固有ベクトルの計算結果をワールド座標系にセットする
                WgUtilForPeriodicEigen.SetFieldValueForDisplay(World, FieldValId, resVec);

                // 表示用にデータを格納する
                if (freqIndex == 0)
                {
                    EigenValueList.Clear();
                }

                // 表示用加工
                if (isCalcCouplingLength)
                {
                    // 結合長(位相差)を格納する
                    EigenValueList.Add(betaDiffForCoupler / k0);
                    System.Diagnostics.Debug.WriteLine("betaDiffForCoupler: {0}  {1}  Lc: {2}",
                        betaDiffForCoupler / k0,
                        betaDiffForCoupler / k0 * (periodicDistance / latticeA) * normalizedFreq,
                        0.5 / (betaDiffForCoupler / k0 * (periodicDistance / latticeA) * normalizedFreq)
                        );
                }
                else
                {
                    // 伝搬定数をk0で規格化する
                    KrdLab.clapack.Complex normalizedBetam = betamToSolve / k0;
                    Complex evalueToShow = new Complex(normalizedBetam.Real, normalizedBetam.Imaginary);
                    EigenValueList.Add(evalueToShow);
                }

                // 描画する界の値を加工して置き換える
                //    そのまま描画オブジェクトにフィールドを渡すと、複素数で格納されているフィールド値の実数部が表示されます。
                //    絶対値を表示したかったので、下記処理を追加しています。
                if (IsShowAbsField)
                {
                    WgUtil.ReplaceFieldValueForDisplay(World, FieldValId); // 絶対値表示
                }
                // DEBUG
                //WgUtil.ReplaceFieldValueForDisplay(World, FieldValId); // 絶対値表示
                //WgUtil.ReplaceFieldValueForDisplay(World, FieldValId, 1); // 虚数部表示

                //------------------------------------------------------------------
                // 描画する界の追加
                //------------------------------------------------------------------
                DrawerAry.Clear();
                DrawerAry.PushBack(new CDrawerFace(FieldValId, true, World, FieldValId));
                DrawerAry.PushBack(new CDrawerEdge(FieldValId, true, World));
                if (initFlg)
                {
                    // カメラの変換行列初期化
                    DrawerAry.InitTrans(Camera);

                    // 表示位置調整
                    setupPanAndScale(probNo, Camera);
                }
                success = true;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                if (showException)
                {
                    Console.WriteLine(exception.Message + " " + exception.StackTrace);
                }
                // 表示用にデータを格納する
                if (freqIndex == 0)
                {
                    EigenValueList.Clear();
                }
                EigenValueList.Add(new Complex(0.0, 0.0));

                DrawerAry.Clear();
                DrawerAry.PushBack(new CDrawerFace(FieldValId, true, World, FieldValId));
                DrawerAry.PushBack(new CDrawerEdge(FieldValId, true, World));
                if (initFlg)
                {
                    // カメラの変換行列初期化
                    DrawerAry.InitTrans(Camera);

                    // 表示位置調整
                    setupPanAndScale(probNo, Camera);
                }
            }
            return success;
        }

        /// <summary>
        /// パンとスケールの設定
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="Camera"></param>
        private static void setupPanAndScale(int probNo, CCamera Camera)
        {
            if (probNo == 4)
            {
                // 表示位置調整
                Camera.MousePan(0, 0, -0.10, 0.50);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 5)
            {
                // 表示位置調整
                Camera.MousePan(0, 0, 0.38, 0);
                double tmp_scale = 1.4;
                Camera.SetScale(tmp_scale);
            }
            else
            {
                // 表示位置調整
                Camera.MousePan(0, 0, 0.35, 0);
                double tmp_scale = 1.3;
                Camera.SetScale(tmp_scale);
            }
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
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolve"></param>
        /// <param name="resVec"></param>
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
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            double minBeta,
            double maxBeta,
            out KrdLab.clapack.Complex betamToSolve,
            out KrdLab.clapack.Complex[] resVec)
        {
            betamToSolve = 0.0;
            resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点

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

            //// 標準固有値解析
            //double[] massInv = MyUtilLib.Matrix.MyMatrixUtil.matrix_Inverse(MMat, matLen);
            //double[] massInvKMat = MyUtilLib.Matrix.MyMatrixUtil.product(massInv, matLen, matLen, KMat, matLen, matLen);
            //double[] massInvCMat = MyUtilLib.Matrix.MyMatrixUtil.product(massInv, matLen, matLen, CMat, matLen, matLen);
            //KrdLab.clapack.Complex[] X = new KrdLab.clapack.Complex[(matLen * 2) * (matLen * 2)];
            //for (int i = 0; i < matLen; i++)
            //{
            //    for (int j = 0; j < matLen; j++)
            //    {
            //        X[i + j * (matLen * 2)] = 0.0;
            //        X[i + (j + matLen) * (matLen * 2)] = (i == j) ? 1.0 : 0.0;
            //        //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
            //        // λ = -jβと置いた場合
            //        //X[(i + matLen) + j * (matLen * 2)] = -1.0 * massInvKMat[i + j * matLen];
            //        //X[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * massInvCMat[i + j * matLen];
            //        //
            //        //  { [K] - jβ[C] - β^2[M] }{Φ}= {0}
            //        // -[K] --> [K]
            //        // j[C] --> [C]
            //        //  とおいてλ=βとした場合
            //        X[(i + matLen) + j * (matLen * 2)] = massInvKMat[i + j * matLen];
            //        X[(i + matLen) + (j + matLen) * (matLen * 2)] = -1.0 * KrdLab.clapack.Complex.ImaginaryOne * massInvCMat[i + j * matLen];
            //    }
            //}
            //KrdLab.clapack.Complex[] ret_evals = null;
            //KrdLab.clapack.Complex[][] ret_evecs = null;
            //System.Diagnostics.Debug.WriteLine("KrdLab.clapack.FunctionExt.zgeev");
            //KrdLab.clapack.FunctionExt.zgeev(X, (matLen * 2), (matLen * 2), ref ret_evals, ref ret_evecs);

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
            KrdLab.clapack.Complex[] betamToSolveList = null;
            KrdLab.clapack.Complex[][] resVecList = null;
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
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
            if (betamToSolveList != null && betamToSolveList.Length > incidentModeIndex)
            {
                int tagtModeIndex = betamToSolveList.Length - 1 - incidentModeIndex;
                // 伝搬定数、固有ベクトルの格納
                betamToSolve = betamToSolveList[tagtModeIndex];
                resVecList[tagtModeIndex].CopyTo(resVec, 0);
                System.Diagnostics.Debug.WriteLine("result(" + tagtModeIndex + "): β/k0: {0} + {1} i", betamToSolve.Real / k0, betamToSolve.Imaginary / k0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                betamToSolve = 0;
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] = 0;
                }
            }
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
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="CalcModeIndex"></param>
        /// <param name="betamToSolve"></param>
        /// <param name="resVec"></param>
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
            out KrdLab.clapack.Complex betamToSolve,
            out KrdLab.clapack.Complex[] resVec)
        {
            betamToSolve = 0.0;
            resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点

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
                    //System.Diagnostics.Debug.WriteLine("βd/2π: {0} + {1} i", beta_d_tmp.Real / (2.0 * pi), beta_d_tmp.Imaginary / (2.0 * pi));
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
                        double[] coord1st = coord_c_all[nodeNumber1st];
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
                            double[] coord = coord_c_all[nodeNumber];
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
            KrdLab.clapack.Complex[] betamToSolveList = null;
            KrdLab.clapack.Complex[][] resVecList = null;
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
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
            if (betamToSolveList != null && betamToSolveList.Length > incidentModeIndex)
            {
                int tagtModeIndex = betamToSolveList.Length - 1 - incidentModeIndex;
                // 伝搬定数、固有ベクトルの格納
                betamToSolve = betamToSolveList[tagtModeIndex];
                resVecList[tagtModeIndex].CopyTo(resVec, 0);
                System.Diagnostics.Debug.WriteLine("result(" + tagtModeIndex + "): β/k0: {0} + {1} i", betamToSolve.Real / k0, betamToSolve.Imaginary / k0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                betamToSolve = 0;
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] = 0;
                }
            }
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
                        resVecList[selectedModeIndex][nodeNumber] = evec[ino];
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
            bool isCalcCouplingLength,
            out KrdLab.clapack.Complex betamToSolve,
            out KrdLab.clapack.Complex[] resVec,
            out double betaDiffForCoupler)
        {
            betamToSolve = 0.0;
            resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点
            betaDiffForCoupler = 0.0;

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
                        double[] coord1st = coord_c_all[nodeNumber1st];
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
                            double[] coord = coord_c_all[nodeNumber];
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
            KrdLab.clapack.Complex[] betamToSolveList = null;
            KrdLab.clapack.Complex[][] resVecList = null;
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
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
            if (betamToSolveList != null && betamToSolveList.Length > incidentModeIndex)
            {
                int tagtModeIndex = betamToSolveList.Length - 1 - incidentModeIndex;
                // 伝搬定数、固有ベクトルの格納
                betamToSolve = betamToSolveList[tagtModeIndex];
                resVecList[tagtModeIndex].CopyTo(resVec, 0);
                System.Diagnostics.Debug.WriteLine("result(" + tagtModeIndex + "): β/k0: {0} + {1} i", betamToSolve.Real / k0, betamToSolve.Imaginary / k0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                betamToSolve = 0;
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] = 0;
                }
            }
            // ２チャンネル導波路のとき結合長を求める
            if (isCalcCouplingLength && betamToSolveList != null && betamToSolveList.Length >= 2)
            {
                // 実数部の差
                betaDiffForCoupler = betamToSolveList[betamToSolveList.Length - 1].Real - betamToSolveList[betamToSolveList.Length - 2].Real;
            }
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
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="betamToSolve"></param>
        /// <param name="resVec"></param>
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
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            double betaNormalizingFactor,
            out KrdLab.clapack.Complex betamToSolve,
            out KrdLab.clapack.Complex[] resVec)
        {
            betamToSolve = 0.0;
            resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点

            //const bool isSVEA = true;

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
            KrdLab.clapack.Complex[] betamToSolveList = null;
            KrdLab.clapack.Complex[][] resVecList = null;
            GetSortedModes(
                incidentModeIndex,
                k0,
                node_cnt,
                free_node_cnt,
                boundary_node_cnt,
                sortedNodes,
                toSorted,
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
            if (betamToSolveList != null && betamToSolveList.Length > incidentModeIndex)
            {
                int tagtModeIndex = betamToSolveList.Length - 1 - incidentModeIndex;
                // 伝搬定数、固有ベクトルの格納
                betamToSolve = betamToSolveList[tagtModeIndex];
                resVecList[tagtModeIndex].CopyTo(resVec, 0);
                System.Diagnostics.Debug.WriteLine("result(" + tagtModeIndex + "): β/k0: {0} + {1} i", betamToSolve.Real / k0, betamToSolve.Imaginary / k0);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                betamToSolve = 0;
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] = 0;
                }
            }
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
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="IsCalcSecondMode"></param>
        /// <param name="betamToSolve"></param>
        /// <param name="resVec"></param>
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
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            bool IsPCWaveguide,
            IList<IList<uint>> PCWaveguidePorts,
            bool IsCalcSecondMode,
            bool isModeTrace,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            double minBeta,
            double maxBeta,
            ref KrdLab.clapack.Complex betamToSolve,
            out KrdLab.clapack.Complex[] resVec)
        {
            //初期値は引数で与える
            //betamToSolve = 0.0;
            resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点

            //const bool isSVEA = true;

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
                    betamToSolve = 0.0;
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
                int work_incidentModeIndex = (IsCalcSecondMode ? 1 : 0);
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
                    for (int i = 0; i < resVec.Length; i++)
                    {
                        resVec[i] = 0;
                    }
                    break;
                }
                if (IsCalcSecondMode && work_incidentModeIndex >= work_betamToSolveList.Length)
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
                    for (int i = 0; i < resVec.Length; i++)
                    {
                        resVec[i] = 0;
                    }
                    break;
                }

                // 伝搬定数、固有ベクトルの格納
                // 収束判定用に前の伝搬定数を退避
                KrdLab.clapack.Complex prevBetam = betamToSolve;
                // 伝搬定数
                betamToSolve = work_betamToSolveList[tagtModeIndex];
                // 固有ベクトル
                work_resVecList[tagtModeIndex].CopyTo(resVec, 0);

                // 収束判定
                double res = KrdLab.clapack.Complex.Abs(prevBetam - betamToSolve);
                if (res < resMin)
                {
                    System.Diagnostics.Debug.WriteLine("converged itr: {0} betam: {1} + {2} i", itr, betamToSolve.Real, betamToSolve.Imaginary);
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
                    Console.WriteLine("PCWaveguideMode: beta is invalid skip: β/k0 = {0} at k0 = {1}", betam.Real / k0, k0);
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
                    workModalVec2[nodeNumber] = fieldVec[ino];
                    // 対応する前回の固有ベクトル
                    workModalVec1[nodeNumber] = PrevModalVec[nodeNumber];
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

        /// <summary>
        /// 規格化周波数の取得
        /// </summary>
        /// <param name="freqIndex"></param>
        /// <returns></returns>
        private static double getNormalizedFreq(
            ref int freqIndex,
            double NormalizedFreq1,
            double NormalizedFreq2,
            double FreqDelta)
        {
            double normalizedFreq = freqIndex * FreqDelta + NormalizedFreq1;
            if (normalizedFreq > NormalizedFreq2)
            {
                //最初に戻る
                //freqIndex = 0;
                //normalizedFreq = NormalizedFreq1;

                // 終了
                freqIndex = -1;
                normalizedFreq = NormalizedFreq2;
            }
            else if (freqIndex == -1)
            {
                normalizedFreq = NormalizedFreq2;
            }
            if (Math.Abs(normalizedFreq) < 1.0e-12)
            {
                normalizedFreq += 1.0e-4;
            }
            return normalizedFreq;
        }

        /// <summary>
        /// 伝搬定数の計算結果表示
        /// </summary>
        private void drawEigenResults(int probNo)
        {
            if (EigenValueList.Count > 0)
            {
                int dispFreqIndex = EigenValueList.Count - 1;
                double normalizedFreq = getNormalizedFreq(
                    ref dispFreqIndex,
                    NormalizedFreq1,
                    NormalizedFreq2,
                    FreqDelta);

                // 周波数を表示
                drawString(10, 45, 0.0, 0.7, 1.0, string.Format((IsPCWaveguide? "a/lambda: " : "2W/lamda: ") + "{0}", normalizedFreq));
                // 伝搬定数を表示
                Complex evalue =EigenValueList[dispFreqIndex];
                //drawString(10, 60, 0.0, 0.7, 1.0, string.Format("beta/k0:{0:F4}", evalue.Real));
                if (IsCalcCouplingLength)
                {
                    // 伝搬定数(真空の波数で規格化)
                    double work_betak0 = evalue.Real;
                    // 動作波数
                    double work_k0 = normalizedFreq * (2.0 * pi) / LatticeA;
                    // 伝搬定数(規格化なし)
                    double work_beta = work_betak0 * work_k0;
                    //drawString(10, 60, 0.0, 0.7, 1.0, string.Format("phase shift [/k0]: {0:F4}  [(2.0 * pi)/d]: {1:F4}",
                    //    work_betak0,                               // 伝搬定数(真空の波数で規格化)
                    //    work_beta * PeriodicDistance / (2.0 * pi)  // 波数ベクトル
                    //    ));
                    drawString(10, 60, 0.0, 0.7, 1.0, string.Format("phase shift [(2.0 * pi)/d]: {0:F4}  Lc [/d]: {1:F4}",
                        work_beta * PeriodicDistance / (2.0 * pi),  // 波数ベクトル
                        0.5 / (work_beta * PeriodicDistance / (2.0 * pi))
                        ));
                }
                else
                {
                    // 伝搬定数(真空の波数で規格化)
                    double work_betak0 = evalue.Real;
                    // 動作波数
                    double work_k0 = normalizedFreq * (2.0 * pi) / LatticeA;
                    // 伝搬定数(規格化なし)
                    double work_beta = work_betak0 * work_k0;
                    drawString(10, 60, 0.0, 0.7, 1.0, string.Format("beta/k0: {0:F4}   beta*d/(2.0 * pi): {1:F4}",
                        work_betak0,                               // 伝搬定数(真空の波数で規格化)
                        work_beta * PeriodicDistance / (2.0 * pi)  // 波数ベクトル
                        ));
                }

                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                // 伝搬定数の周波数特性データをグラフ表示用バッファに格納
                int dataCnt = EigenValueList.Count;
                int axisYCnt = 2;
                if (IsCalcCouplingLength)
                {
                    axisYCnt = 1;
                }
                double[] valueX = new double[dataCnt];
                IList<double[]> valueYs = new List<double[]>();
                for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                {
                    double[] valueY = new double[dataCnt];
                    valueYs.Add(valueY);
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    int workFreqIndex = i;
                    valueX[i] = getNormalizedFreq(
                        ref workFreqIndex,
                        NormalizedFreq1,
                        NormalizedFreq2,
                        FreqDelta);
                    for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                    {
                        if (IsCalcCouplingLength)
                        {
                            double work_k0 = valueX[i] * (2.0 * pi) / LatticeA;
                            double work_betak0 = EigenValueList[i].Real;
                            double work_beta = work_betak0 * work_k0;
                            if (axisYIndex == 0)
                            {
                                // 波数ベクトル
                                //valueYs[axisYIndex][i] = work_beta * PeriodicDistance / (2.0 * pi);
                                // 完全結合長
                                valueYs[axisYIndex][i] = 0.5/(work_beta * PeriodicDistance / (2.0 * pi));
                            }
                            else
                            {
                                valueYs[axisYIndex][i] = 0.0;
                            }
                        }
                        else if (IsWaveNumberGraph)
                        {
                            double work_k0 = valueX[i] * (2.0 * pi) / LatticeA;
                            double work_betak0 = EigenValueList[i].Real;
                            double work_beta = work_betak0 * work_k0;
                            if (axisYIndex == 0)
                            {
                                // 波数ベクトル
                                valueYs[axisYIndex][i] = work_beta * PeriodicDistance / (2.0 * pi);
                            }
                            else
                            {
                                // light line
                                valueYs[axisYIndex][i] = valueX[i] * PeriodicDistance / LatticeA;
                            }
                        }
                        else
                        {
                            if (axisYIndex == 0)
                            {
                                // 伝搬定数
                                valueYs[axisYIndex][i] = EigenValueList[i].Real;
                            }
                            else if (axisYIndex == 1)
                            {
                                // 伝搬定数の取り得る最大値の表示
                                //valueYs[axisYIndex][i] = 0.5 * ((2.0 * pi) / (periodicDistance)) / ((2.0 * pi * valueX[i]) / latticeA) ;
                                valueYs[axisYIndex][i] = 0.5 * LatticeA / (PeriodicDistance * valueX[i]);
                            }
                        }
                    }
                }

                // 周波数特性グラフの描画
                double graphWidth = 350 * win_w / (double)DefWinWidth;
                double graphHeight = 170 * win_w / (double)DefWinWidth;
                double graphX = 40;
                double graphY = 0;
                if (probNo == 4)
                {
                    graphY = win_h - graphHeight - 20;
                }
                else
                {
                    graphY = win_h - graphHeight - 20 - 150;
                }
                double minXValue = NormalizedFreq1;
                double maxXValue = NormalizedFreq2;
                //double minYValue = IsWaveNumberGraph ? 0.0 : MinBeta;
                //double maxYValue = IsWaveNumberGraph ? 0.5 : MaxBeta;
                double minYValue = IsWaveNumberGraph ? MinWaveNum : MinBeta;
                double maxYValue = IsWaveNumberGraph ? MaxWaveNum : MaxBeta;
                if (IsCalcCouplingLength)
                {
                    minYValue = 0.0;
                    //maxYValue = 0.16;
                    maxYValue = 20;
                }
                double intervalXValue = GraphFreqInterval;
                double intervalYValue = IsWaveNumberGraph ? 0.1 : GraphBetaInterval;
                if (IsCalcCouplingLength)
                {
                    //intervalYValue = 0.02;
                    intervalYValue = 5;
                }
                IList<string> valuesYTitles = null;
                if (IsCalcCouplingLength)
                {
                    //valuesYTitles = new List<string>() { "phase shift [(2.0 * pi) / d]" };
                    valuesYTitles = new List<string>() { "Lc [/d]" };
                }
                else
                {
                    if (IsWaveNumberGraph)
                    {
                        valuesYTitles = new List<string>() { "wave num", "light" };
                    }
                    else
                    {
                        valuesYTitles = new List<string>() { "beta/k0", "limit" };
                    }
                }
                drawGraph(
                    graphX, graphY, graphWidth, graphHeight,
                    minXValue, maxXValue, minYValue, maxYValue,
                    intervalXValue, intervalYValue,
                    valueX, valueYs,
                    valuesYTitles);
            }
        }

        /// <summary>
        /// 文字列描画
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="str"></param>
        private static void drawString(int screenX, int screenY, double r, double g, double b, string str)
        {
            int is_lighting = Gl.glIsEnabled(Gl.GL_LIGHTING);
            int is_texture = Gl.glIsEnabled(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            IntPtr font = Glut.GLUT_BITMAP_8_BY_13;
            int[] viewport = new int[4];

            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int win_w = viewport[2];
            int win_h = viewport[3];
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, win_w, 0, win_h);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glScalef(1, -1, 1);
            Gl.glTranslatef(0, -win_h, 0);
            Gl.glDisable(Gl.GL_LIGHTING);
            //Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glColor3d(r, g, b);
            DelFEM4NetCom.GlutUtility.RenderBitmapString(screenX, screenY, font, str);
            //Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            ////
            if (is_texture == 1) { Gl.glEnable(Gl.GL_TEXTURE_2D); }
            if (is_lighting == 1) { Gl.glEnable(Gl.GL_LIGHTING); }
        }

        /// <summary>
        /// グラフ描画
        /// </summary>
        /// <param name="graphAreaX">グラフの表示位置X(左)</param>
        /// <param name="graphAreaY">グラフの表示位置Y(上)</param>
        /// <param name="graphAreaWidth">グラフの幅</param>
        /// <param name="graphAreaHeight">グラフの高さ</param>
        /// <param name="minXValue">X軸の最小値</param>
        /// <param name="maxXValue">X軸の最大値</param>
        /// <param name="minYValue">Y軸の最小値</param>
        /// <param name="maxYValue">Y軸の最大値</param>
        /// <param name="intervalXValue">X軸の目盛刻み幅</param>
        /// <param name="intervalYValue">Y軸の目盛刻み幅</param>
        /// <param name="valueX">X軸のデータ配列</param>
        /// <param name="valueYs">Y軸のデータの（X軸のデータの個数分)配列のリスト</param>
        private static void drawGraph(
            double graphAreaX,
            double graphAreaY,
            double graphAreaWidth,
            double graphAreaHeight,
            double minXValue,
            double maxXValue,
            double minYValue,
            double maxYValue,
            double intervalXValue,
            double intervalYValue,
            double[] valueX,
            IList<double[]> valueYs,
            IList<string> valueYTitles)
        {
            // ウィンドウの寸法を取得
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int win_w = viewport[2];
            int win_h = viewport[3];
            // 変換
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, win_w, 0, win_h);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glScalef(1, -1, 1);
            Gl.glTranslatef(0, -win_h, 0);

            // グラフの領域
            double xxmin = graphAreaX;
            double xxmax = graphAreaX + graphAreaWidth;
            double yymin = graphAreaY;
            double yymax = graphAreaY + graphAreaHeight;
            // データ→座標のスケーリング係数
            double scaleX = graphAreaWidth / (maxXValue - minXValue);
            double scaleY = graphAreaHeight / (maxYValue - minYValue);

            // グラフ領域の塗りつぶし
            Gl.glColor3d(1.0, 1.0, 1.0);
            Gl.glBegin(Gl.GL_POLYGON);
            // LeftTop
            Gl.glVertex2d(xxmin, yymax);
            // LeftBottom
            Gl.glVertex2d(xxmin, yymin);
            // RightBottom
            Gl.glVertex2d(xxmax, yymin);
            // RightTop
            Gl.glVertex2d(xxmax, yymax);
            Gl.glEnd();

            double[] foreColor = { 0.1, 0.1, 0.1 };
            double[] lineColor = { 0.1, 0.1, 0.1 };
            double[,] graphLineColors = new double[2, 3]
            {
                {0.0, 0.0, 1.0},
                {0.5, 0.5, 0.5},//{1.0, 0.0, 1.0}
            };
            // X軸
            Gl.glColor3d(lineColor[0], lineColor[1], lineColor[2]);
            Gl.glLineWidth(1.0f);
            Gl.glBegin(Gl.GL_LINES);
            // Bottom
            Gl.glVertex2d(xxmin, yymax);
            Gl.glVertex2d(xxmax, yymax);
            // Top
            Gl.glVertex2d(xxmin, yymin);
            Gl.glVertex2d(xxmax, yymin);
            Gl.glEnd();
            // Y軸
            Gl.glColor3d(lineColor[0], lineColor[1], lineColor[2]);
            Gl.glLineWidth(1.0f);
            Gl.glBegin(Gl.GL_LINES);
            // Left
            Gl.glVertex2d(xxmin, yymin);
            Gl.glVertex2d(xxmin, yymax);
            // Right
            Gl.glVertex2d(xxmax, yymin);
            Gl.glVertex2d(xxmax, yymax);
            Gl.glEnd();
            // X軸補助線（縦の線)
            int nDivX = (int)Math.Floor((maxXValue - minXValue) / intervalXValue) + 1;
            for (int iDivX = 0; iDivX < nDivX; iDivX++)
            {
                Gl.glColor3d(lineColor[0], lineColor[1], lineColor[2]);
                Gl.glLineWidth(1.0f);
                Gl.glBegin(Gl.GL_LINES);
                double xx = graphAreaX + intervalXValue * iDivX * scaleX;
                Gl.glVertex2d(xx, yymin);
                Gl.glVertex2d(xx, yymax);
                Gl.glEnd();
                double x = minXValue + intervalXValue * iDivX;
                drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F4}", x));
            }
            // Y軸補助線（横の線)
            int nDivY = (int)Math.Floor((maxYValue - minYValue) / intervalYValue) + 1;
            for (int iDivY = 0; iDivY < nDivY; iDivY++)
            {
                Gl.glColor3d(lineColor[0], lineColor[1], lineColor[2]);
                Gl.glLineWidth(1.0f);
                Gl.glBegin(Gl.GL_LINES);
                double yy = graphAreaY + graphAreaHeight - (intervalYValue * iDivY * scaleY);
                Gl.glVertex2d(xxmin, yy);
                Gl.glVertex2d(xxmax, yy);
                Gl.glEnd();
                double y = minYValue + intervalYValue * iDivY;
                drawString((int)(xxmin - 35), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F2}", y));
            }

            // 凡例
            for (int iAxisY = 0; iAxisY < valueYs.Count; iAxisY++)
            {
                if (iAxisY % 2 == 0)
                {
                    Gl.glColor3d(graphLineColors[0, 0], graphLineColors[0, 1], graphLineColors[0, 2]);
                    // 実践
                    Gl.glDisable(Gl.GL_LINE_STIPPLE);
                }
                else
                {
                    Gl.glColor3d(graphLineColors[1, 0], graphLineColors[1, 1], graphLineColors[1, 2]);
                    // 破線
                    Gl.glEnable(Gl.GL_LINE_STIPPLE);
                    Gl.glLineStipple(1, 0xF0F0);
                }
                Gl.glLineWidth(1.0f);
                Gl.glBegin(Gl.GL_LINES);
                double xx = graphAreaX + graphAreaWidth - 220 + iAxisY * 110;
                double yy = graphAreaY - 15;
                Gl.glVertex2d(xx, yy);
                Gl.glVertex2d(xx + 30, yy);
                Gl.glEnd();
                // 破線を無効にする
                Gl.glDisable(Gl.GL_LINE_STIPPLE);
                drawString((int)(xx + 40), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], valueYTitles[iAxisY]);
            }

            // データのプロット
            int dataCnt = valueX.Length;
            for (int iAxisY = 0; iAxisY < valueYs.Count; iAxisY++)
            {
                double[] valueY = valueYs[iAxisY];
                if (iAxisY % 2 == 0)
                {
                    Gl.glColor3d(graphLineColors[0, 0], graphLineColors[0, 1], graphLineColors[0, 2]);
                }
                else
                {
                    Gl.glColor3d(graphLineColors[1, 0], graphLineColors[1, 1], graphLineColors[1, 2]);
                }
                //Gl.glPointSize(2.0f);
                //Gl.glBegin(Gl.GL_POINTS);
                if (iAxisY % 2 == 0)
                {
                    // 実践
                    Gl.glDisable(Gl.GL_LINE_STIPPLE);
                }
                else
                {
                    // 破線
                    Gl.glEnable(Gl.GL_LINE_STIPPLE);
                    Gl.glLineStipple(1, 0xF0F0);
                }
                Gl.glLineWidth(2.0f);
                Gl.glBegin(Gl.GL_LINE_STRIP);
                System.Diagnostics.Debug.Assert(valueX.Length == valueY.Length);
                if (valueX.Length != valueY.Length)
                {
                    return;
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    if (valueY[i] < minYValue || valueY[i] > maxYValue)
                    {
                        continue;
                    }
                    double xx;
                    double yy;
                    xx = graphAreaX + (valueX[i] - minXValue) * scaleX;
                    yy = graphAreaY + graphAreaHeight - (valueY[i] - minYValue) * scaleY;
                    Gl.glVertex2d(xx, yy);
                    //System.Diagnostics.Debug.WriteLine("{0},{1}", xx, yy);
                }
                Gl.glEnd();
                // 破線を無効にする
                Gl.glDisable(Gl.GL_LINE_STIPPLE);
            }

            //Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }
    }
}
