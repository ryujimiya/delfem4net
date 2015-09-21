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

namespace wg2d_periodic_eigen_k
{
    ///////////////////////////////////////////////////////////////
    //  DelFEM4Net サンプル
    //  周期構造導波路固有値問題 2D (周波数を固有値として解く)
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
            ref double Beta1,
            ref double Beta2,
            ref double BetaDelta,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref double GraphBetaInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref int CalcModeIndex,
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
        private const int DefProbNo = 3;//5;//3;//1;
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
        //private const bool DefIsSVEA = true;  // Φ = φ(x, y) exp(-jβx) と置く方法
        private const bool DefIsSVEA = false; // Φを直接解く方法(exp(-jβd)を固有値として扱う

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
        /// 計算対象伝搬定数インデックス
        /// </summary>
        private int BetaIndex = 0;
        /// <summary>
        /// 開始規格化伝搬定数
        /// </summary>
        private double Beta1 = 0.0;
        /// <summary>
        /// 終了規格化伝搬定数
        /// </summary>
        private double Beta2 = 0.5;
        /// <summary>
        /// 周波数間隔
        /// </summary>
        private double BetaDelta = 0.01;
        /// <summary>
        /// グラフの周波数目盛間隔
        /// </summary>
        private double GraphFreqInterval = 0.2;
        /// <summary>
        /// 伝搬定数の最小値
        /// </summary>
        private double MinNormalizedFreq = 0.0;
        /// <summary>
        /// 伝搬定数の最大値
        /// </summary>
        private double MaxNormalizedFreq = 0.0;
        /// <summary>
        /// グラフの伝搬定数目盛間隔
        /// </summary>
        private double GraphBetaInterval = 0.2;
        /// <summary>
        /// 波のモード区分
        /// </summary>
        private WgUtil.WaveModeDV WaveModeDv = WgUtil.WaveModeDV.TE;

        // 導波管の幅
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
        /// 緩慢変化包絡線近似？
        ///    true  緩慢変化包絡線近似 Φ = φ(x, y) exp(-jβx) と置く方法
        ///    false Φを直接解く方法(exp(-jβd)を固有値として扱う)
        /// </summary>
        private bool IsSVEA = DefIsSVEA;
        /// <summary>
        /// 前回の固有モードベクトル
        /// </summary>
        private KrdLab.clapack.Complex[] PrevModalVec = null;
        /// <summary>
        /// 界の絶対値を表示する？
        /// </summary>
        private bool IsShowAbsField = false;
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
                if (IsAnimation && BetaIndex != -1 && !IsCadShow)
                {
                    // 問題を解く
                    solveProblem(
                        ProbNo,
                        ref BetaIndex,
                        Beta1,
                        Beta2,
                        BetaDelta,
                        (BetaIndex == 0), // false,
                        WaveguideWidth,
                        WaveModeDv,
                        IsPCWaveguide,
                        LatticeA,
                        PeriodicDistance,
                        PCWaveguidePorts,
                        CalcModeIndex,
                        IsSVEA,
                        ref PrevModalVec,
                        IsShowAbsField,
                        MinNormalizedFreq,
                        MaxNormalizedFreq,
                        ref World,
                        FieldValId,
                        FieldLoopId,
                        FieldForceBcId,
                        FieldPortBcId1,
                        FieldPortBcId2,
                        Medias,
                        LoopDic,
                        EdgeDic,
                        ref EigenValueList,
                        ref DrawerAry,
                        Camera
                        );
                    if (BetaIndex != -1)
                    {
                        BetaIndex++;
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
            BetaIndex = 0;
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
                BetaIndex = -1;
                EigenValueList.Clear();
                // 問題を作成
                setProblem();
                BetaIndex = 0;
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
                ref Beta1,
                ref Beta2,
                ref BetaDelta,
                ref GraphFreqInterval,
                ref MinNormalizedFreq,
                ref MaxNormalizedFreq,
                ref GraphBetaInterval,
                ref WaveModeDv,
                ref IsPCWaveguide,
                ref LatticeA,
                ref PeriodicDistance,
                ref PCWaveguidePorts,
                ref CalcModeIndex,
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
        /// <param name="Beta1"></param>
        /// <param name="Beta2"></param>
        /// <param name="BetaDelta"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
        /// <param name="GraphBetaInterval"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="CalcModeIndex"></param>
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
            ref double Beta1,
            ref double Beta2,
            ref double BetaDelta,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref double GraphBetaInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref bool IsPCWaveguide,
            ref double latticeA,
            ref double periodicDistance,
            ref IList<IList<uint>> PCWaveguidePorts,
            ref int CalcModeIndex,
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

            // フォトニック結晶導波路解析用
            IsPCWaveguide = false;
            latticeA = 0.0;
            periodicDistance = 0.0;
            PCWaveguidePorts.Clear();
            // 基本モードを計算する
            CalcModeIndex = 0;

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
                    //func = Problem03_2.SetProblem; // １列目ロッドの半径を大きくする
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
                    ref Beta1,
                    ref Beta2,
                    ref BetaDelta,
                    ref GraphFreqInterval,
                    ref MinNormalizedFreq,
                    ref MaxNormalizedFreq,
                    ref GraphBetaInterval,
                    ref WaveModeDv,
                    ref IsPCWaveguide,
                    ref latticeA,
                    ref periodicDistance,
                    ref PCWaveguidePorts,
                    ref CalcModeIndex,
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
                    ref Camera
                    );
                //success = true;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            return success;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <returns></returns>
        private bool solveProblem()
        {
            bool ret = solveProblem(
                ProbNo,
                ref BetaIndex,
                Beta1,
                Beta2,
                BetaDelta,
                (BetaIndex == 0), // false,
                WaveguideWidth,
                WaveModeDv,
                IsPCWaveguide,
                LatticeA,
                PeriodicDistance,
                PCWaveguidePorts,
                CalcModeIndex,
                IsSVEA,
                ref PrevModalVec,
                IsShowAbsField,
                MinNormalizedFreq,
                MaxNormalizedFreq,
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPortBcId1,
                FieldPortBcId2,
                Medias,
                LoopDic,
                EdgeDic,
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
        /// <param name="betaIndex"></param>
        /// <param name="Beta1"></param>
        /// <param name="Beta2"></param>
        /// <param name="BetaDelta"></param>
        /// <param name="initFlg"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="IsPCWaveguide"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistance"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="CalcModeIndex"></param>
        /// <param name="isSVEA"></param>
        /// <param name="PrevModalVec"></param>
        /// <param name="IsShowAbsField"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
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
            ref int betaIndex,
            double Beta1,
            double Beta2,
            double BetaDelta,
            bool initFlg,
            double WaveguideWidth,
            WgUtil.WaveModeDV WaveModeDv,
            bool IsPCWaveguide,
            double latticeA,
            double periodicDistance,
            IList<IList<uint>> PCWaveguidePorts,
            int CalcModeIndex,
            bool isSVEA,
            ref KrdLab.clapack.Complex[] PrevModalVec,
            bool IsShowAbsField,
            double MinNormalizedFreq,
            double MaxNormalizedFreq,
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            uint FieldPortBcId1,
            uint FieldPortBcId2,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref IList<Complex> EigenValueList,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            bool success = false;
            bool showException = true;
            try
            {
                // 緩慢変化包絡線近似 SVEA(slowly varying envelope approximation)で表現？
                //  true: v = v(x, y)exp(-jβx)と置いた場合
                //  false: 直接Bloch境界条件を指定する場合
                //bool isSVEA = false; // falseの時の方が妥当な解が得られる
                //bool isSVEA = false;
                // モード追跡する？
                bool isModeTrace = true;
                if (!IsPCWaveguide)
                {
                    isModeTrace = false;
                }
                System.Diagnostics.Debug.WriteLine("isSVEA: {0}", isSVEA);
                System.Diagnostics.Debug.WriteLine("isModeTrace: {0}, CalcModeIndex: {1}", isModeTrace, CalcModeIndex);

                if (betaIndex == 0)
                {
                    PrevModalVec = null;
                }

                // 規格化伝搬定数
                double beta = getBeta(
                    ref betaIndex,
                    Beta1,
                    Beta2,
                    BetaDelta);
                if (betaIndex == -1)
                {
                    return success;
                }
                System.Diagnostics.Debug.WriteLine("beta: {0}    beta*d/(2.0 * pi): {1}",
                    beta,
                    beta * periodicDistance / (2.0 * pi));

                if (probNo == 3)
                {
                    /*
                    // probNo == 3 theta = 45 defectRodCnt = 3 even 1st
                    if (beta < 0.32 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                     */

                    /*
                    // probNo == 3 theta = 60 defectRodCnt = 1 r = 0.30 even
                    if (beta < 0.20 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                     */
                    
                    /*
                    // probNo == 3 theta = 30 defectRodCnt = 1 r = 0.30 n = 3.4 even
                    if (beta < 0.08 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.92 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                     

                    /*
                    // probNo == 3 theta = 60 defectRodCnt = 1 r = 0.35 even
                    if (beta < 0.08 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.92 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                    /*
                    // probNo == 3 theta = 30 defectRodCnt = 3 r = 0.28 even 1st
                    if (beta < 0.16 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                     */
                    /*
                    // probNo == 3 theta = 30 defectRodCnt = 3 r = 0.28 even 2nd
                    if (beta > 0.39 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                    /*
                    // probNo == 3 theta = 30 defectRodCnt = 3 r = 0.28 odd 2nd
                    if (beta > 0.20 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */

                    
                    if (beta < 0.10 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                     
                }
                else if (probNo == 5)
                {
                    /*
                    // for latticeTheta = 60 r = 0.30a  n = 3.4 air hole even 1st above decoupling point
                    // for latticeTheta = 60 r = 0.30a  n = 3.4 air hole odd 1st above decoupling point
                    if (beta < 0.13 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.2501 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                    /*
                    // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 even 1st below decoupling point
                    // for latticeTheta = 60 r = 0.30a air hole  n = 3.4 odd 1st below decoupling point
                    if (beta < 0.2501 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.4801 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */

                    
                    /*
                    // for latticeTheta = 60 r = 0.30a  n = 2.76 air hole even 1st above & below decoupling point
                    if (beta < 0.1601 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.4801 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                    /*
                    // for latticeTheta = 60 r = 0.30a  n = 2.76 air hole odd 1st above decoupling point
                    if (beta < 0.1601 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.2601 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     */
                    
                    
                    // for latticeTheta = 60 r = 0.30a air hole  n = 2.76 even 1st below decoupling point
                    // for latticeTheta = 60 r = 0.30a air hole  n = 2.76 odd 1st below decoupling point
                    if (beta < 0.2601 * (2.0 * pi / periodicDistance))
                    {
                        showException = false;
                        throw new Exception();
                    }
                    if (beta > 0.4801 * (2.0 * pi / periodicDistance))
                    {
                        betaIndex = -1;
                        return success;
                    }
                     

                }

                // 全節点数を取得する
                uint node_cnt = 0;
                node_cnt = WgUtilForPeriodicEigenBetaSpecified.GetNodeCnt(World, FieldLoopId);

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
                KrdLab.clapack.Complex[] KMat0 = null;
                KrdLab.clapack.Complex[] MMat0 = null;
                {
                    KrdLab.clapack.Complex betaForMakingMat = 0.0;
                    if (isSVEA)
                    {
                        betaForMakingMat = new KrdLab.clapack.Complex(beta, 0.0); // v = v(x, y)exp(-jβx)と置いた場合
                    }
                    else
                    {
                        betaForMakingMat = 0.0; // 直接Bloch境界条件を指定する場合
                    }
                    WgUtilForPeriodicEigenBetaSpecified.MkPeriodicHelmholtzMat(
                        betaForMakingMat,
                        false, // isYDirectionPeriodic: false
                        World,
                        FieldLoopId,
                        Medias,
                        LoopDic,
                        node_cnt,
                        free_node_cnt0,
                        toSorted,
                        out KMat0,
                        out MMat0);
                }

                // 境界2の節点は境界1の節点と同一とみなす
                //   境界上の分割が同じであることが前提条件
                KrdLab.clapack.Complex[] KMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
                KrdLab.clapack.Complex[] MMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
                /*
                // v = v(x, y)exp(-jβx)と置いた場合
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] = MMat0[i + free_node_cnt0 * j];
                    }
                }
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] += KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        MMat[i + free_node_cnt * j] += MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                    }
                }
                for (int i = 0; i < boundary_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                    }
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                    }
                }
                 */
                // 直接Bloch境界条件を指定する場合
                KrdLab.clapack.Complex expA = 1.0;
                if (isSVEA)
                {
                    expA = 1.0;
                }
                else
                {
                    expA = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * beta * periodicDistance);
                }
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] = MMat0[i + free_node_cnt0 * j];
                    }
                }
                for (int i = 0; i < free_node_cnt; i++)
                {
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] += expA * KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        MMat[i + free_node_cnt * j] += expA * MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                    }
                }
                for (int i = 0; i < boundary_node_cnt; i++)
                {
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + free_node_cnt * j] += (1.0 / expA) * KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                        MMat[i + free_node_cnt * j] += (1.0 / expA) * MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * j];
                    }
                    for (int j = 0; j < boundary_node_cnt; j++)
                    {
                        //KMat[i + free_node_cnt * j] += (1.0 / expA) * expA * KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        //MMat[i + free_node_cnt * j] += (1.0 / expA) * expA * MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        //より下記と等価
                        KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                        MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt - 1 - j)];
                    }
                }

                // 規格化周波数
                KrdLab.clapack.Complex complexNormalizedFreq_ans = 0.0;
                // 界ベクトルは全節点分作成
                KrdLab.clapack.Complex[] resVec = null;
                resVec = new KrdLab.clapack.Complex[node_cnt]; //全節点

                {
                    int matLen = (int)free_node_cnt;
                    KrdLab.clapack.Complex[] evals = null;
                    KrdLab.clapack.Complex[,] evecs = null;
                    KrdLab.clapack.Complex[] A = new KrdLab.clapack.Complex[KMat.Length];
                    KrdLab.clapack.Complex[] B = new KrdLab.clapack.Complex[MMat.Length];
                    for (int i = 0; i < matLen * matLen; i++)
                    {
                        A[i] = KMat[i];
                        B[i] = MMat[i];
                    }
                    /*
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
                     */
                    // エルミートバンド行列の固有値解析
                    {
                        KrdLab.clapack.Complex[] ret_evals = null;
                        KrdLab.clapack.Complex[,] ret_evecs = null;
                        solveHermitianBandMatGeneralizedEigen(matLen, A, B, ref ret_evals, ref ret_evecs);
                        evals = ret_evals;
                        evecs = ret_evecs;
                    }

                    // 固有値のソート
                    WgUtilForPeriodicEigenBetaSpecified.Sort1DEigenMode(evals, evecs);
                    /*
                    // check
                    for (int imode = 0; imode < evals.Length; imode++)
                    {
                        // 固有周波数
                        KrdLab.clapack.Complex complex_k0_eigen = KrdLab.clapack.Complex.Sqrt(evals[imode]);
                        // 規格化周波数
                        KrdLab.clapack.Complex complexNormalizedFreq = latticeA * (complex_k0_eigen / (2.0 * pi));
                        System.Diagnostics.Debug.WriteLine("a/λ  ( " + imode + " ) = " + complexNormalizedFreq.Real + " + " + complexNormalizedFreq.Imaginary + " i ");
                    }
                     */
                    // 欠陥モードを取得
                    IList<int> defectModeIndexList = new List<int>();
                    // フォトニック結晶導波路解析用
                    if (IsPCWaveguide)
                    {
                        int hitModeIndex = -1;
                        double hitNorm = 0.0;
                        for (int imode = 0; imode < evals.Length; imode++)
                        {
                            // 固有周波数
                            KrdLab.clapack.Complex complex_k0_eigen = KrdLab.clapack.Complex.Sqrt(evals[imode]);
                            // 規格化周波数
                            KrdLab.clapack.Complex complexNormalizedFreq = latticeA * (complex_k0_eigen / (2.0 * pi));
                            // 界ベクトル
                            KrdLab.clapack.Complex[] fieldVec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, imode);

                            // フォトニック結晶導波路の導波モードを判定する
                            System.Diagnostics.Debug.Assert(free_node_cnt == fieldVec.Length);
                            bool isHitDefectMode = isDefectModeBetaSpecified(
                                free_node_cnt,
                                sortedNodes,
                                toSorted,
                                PCWaveguidePorts,
                                MinNormalizedFreq,
                                MaxNormalizedFreq,
                                complexNormalizedFreq,
                                fieldVec);

                            if (isHitDefectMode
                                && isModeTrace && PrevModalVec != null)
                            {
                                // 同じ固有モード？
                                double ret_norm = 0.0;
                                bool isHitSameMode = isSameModeBetaSpecified(
                                    node_cnt,
                                    PrevModalVec,
                                    free_node_cnt,
                                    sortedNodes,
                                    toSorted,
                                    PCWaveguidePorts,
                                    MinNormalizedFreq,
                                    MaxNormalizedFreq,
                                    complexNormalizedFreq,
                                    fieldVec,
                                    out ret_norm);
                                if (isHitSameMode)
                                {
                                    // より分布の近いモードを採用する
                                    if (Math.Abs(ret_norm - 1.0) < Math.Abs(hitNorm - 1.0))
                                    {
                                        hitModeIndex = imode;
                                        hitNorm = ret_norm;
                                        System.Diagnostics.Debug.WriteLine("PC defectMode(ModeTrace): a/λ  ( " + imode + " ) = " + complexNormalizedFreq.Real + " + " + complexNormalizedFreq.Imaginary + " i ");
                                    }
                                }
                            }
                            if (isHitDefectMode)
                            {
                                System.Diagnostics.Debug.WriteLine("PC defectMode: a/λ  ( " + imode + " ) = " + complexNormalizedFreq.Real + " + " + complexNormalizedFreq.Imaginary + " i ");
                                if (!isModeTrace || PrevModalVec == null) // モード追跡でないとき、またはモード追跡用の参照固有モードベクトルがないとき
                                {
                                    defectModeIndexList.Add(imode);
                                }
                            }
                        }
                        if (isModeTrace && hitModeIndex != -1)
                        {
                            System.Diagnostics.Debug.Assert(defectModeIndexList.Count == 0);
                            System.Diagnostics.Debug.WriteLine("hitModeIndex: {0}", hitModeIndex);
                            defectModeIndexList.Add(hitModeIndex);
                        }
                    }
                    // 基本モードを取得する(k0^2最小値)
                    int tagtModeIndex = 0;
                    // フォトニック結晶導波路解析用
                    if (IsPCWaveguide)
                    {
                        tagtModeIndex = -1;
                        if (isModeTrace && PrevModalVec != null)
                        {
                            if (defectModeIndexList.Count > 0)
                            {
                                tagtModeIndex = defectModeIndexList[0];
                            }
                        }
                        else
                        {
                            if (defectModeIndexList.Count > 0)
                            {
                                if ((defectModeIndexList.Count - 1) >= CalcModeIndex)
                                {
                                    tagtModeIndex = defectModeIndexList[CalcModeIndex];
                                }
                                else
                                {
                                    tagtModeIndex = -1;
                                }
                            }
                            else
                            {
                                tagtModeIndex = -1;
                                System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not converged photonic crystal waveguide mode");
                            }
                        }
                    }
                    if (tagtModeIndex == -1)
                    {
                        System.Diagnostics.Debug.WriteLine("!!!!!!!!! Not found mode");
                        complexNormalizedFreq_ans = 0;
                        for (int i = 0; i < resVec.Length; i++)
                        {
                            resVec[i] = 0;
                        }
                    }
                    else
                    {
                        // 伝搬定数、固有ベクトルの格納
                        KrdLab.clapack.Complex complex_k0_eigen_ans = KrdLab.clapack.Complex.Sqrt(evals[tagtModeIndex]);
                        complexNormalizedFreq_ans = latticeA * (complex_k0_eigen_ans / (2.0 * pi));
                        KrdLab.clapack.Complex[] evec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, tagtModeIndex);
                        System.Diagnostics.Debug.WriteLine("a/λ  ( " + tagtModeIndex + " ) = " + complexNormalizedFreq_ans.Real + " + " + complexNormalizedFreq_ans.Imaginary + " i ");
                        for (int ino = 0; ino < evec.Length; ino++)
                        {
                            //System.Diagnostics.Debug.WriteLine("    ( " + imode + ", " + ino + " ) = " + evec[ino].Real + " + " + evec[ino].Imaginary + " i ");
                            uint nodeNumber = sortedNodes[ino];
                            resVec[nodeNumber] = evec[ino];
                        }
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
                    //resVec[nodeNumberPortBc2] = cvalue; // v = v(x, y)exp(-jβx)と置いた場合
                    resVec[nodeNumberPortBc2] = expA * cvalue; // 直接Bloch境界条件を指定する場合
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

                // 前回の固有ベクトルを更新
                if (isModeTrace && complexNormalizedFreq_ans.Real != 0.0 && Math.Abs(complexNormalizedFreq_ans.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
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

                //------------------------------------------------------------------
                // 計算結果の後処理
                //------------------------------------------------------------------
                // 固有ベクトルの計算結果をワールド座標系にセットする
                WgUtilForPeriodicEigenBetaSpecified.SetFieldValueForDisplay(World, FieldValId, resVec);

                // 表示用にデータを格納する
                if (betaIndex == 0)
                {
                    EigenValueList.Clear();
                }

                // 表示用加工
                Complex evalueToShow = new Complex(complexNormalizedFreq_ans.Real, complexNormalizedFreq_ans.Imaginary);
                EigenValueList.Add(evalueToShow);

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
                if (betaIndex == 0)
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
                Camera.MousePan(0, 0, 0.38, 0);
                double tmp_scale = 1.3;
                Camera.SetScale(tmp_scale);
            }
        }

        /// <summary>
        /// 欠陥モード？
        /// </summary>
        /// <param name="free_node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="PCWaveguidePorts"></param>
        /// <param name="minNormalizedFreq"></param>
        /// <param name="maxNormalizedFreq"></param>
        /// <param name="normalizedFreq"></param>
        /// <param name="fieldVec"></param>
        /// <returns></returns>
        private static bool isDefectModeBetaSpecified(
            uint free_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            IList<IList<uint>> PCWaveguidePorts,
            double minNormalizedFreq,
            double maxNormalizedFreq,
            KrdLab.clapack.Complex normalizedFreq,
            KrdLab.clapack.Complex[] fieldVec)
        {
            bool isHit = false;
            if (Math.Abs(normalizedFreq.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(normalizedFreq.Imaginary) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 複素モードは除外する
                return isHit;
            }
            else if (Math.Abs(normalizedFreq.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(normalizedFreq.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 伝搬モード
                // 後進波は除外する
                if (normalizedFreq.Real < 0)
                {
                    return isHit;
                }
            }
            else if (Math.Abs(normalizedFreq.Real) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit && Math.Abs(normalizedFreq.Imaginary) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
            {
                // 減衰モード
                //  利得のある波は除外する
                if (normalizedFreq.Imaginary > 0)
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
                if (normalizedFreq.Real >= minNormalizedFreq && normalizedFreq.Real <= maxNormalizedFreq)
                {
                    isHit = true;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("skip: a/λ = {0} + {1} i", normalizedFreq.Real, normalizedFreq.Imaginary);
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
        /// <param name="minNormalizedFreq"></param>
        /// <param name="maxNormalizedFreq"></param>
        /// <param name="complexNormalizedFreq"></param>
        /// <param name="fieldVec"></param>
        /// <returns></returns>
        private static bool isSameModeBetaSpecified(
            uint node_cnt,
            KrdLab.clapack.Complex[] PrevModalVec,
            uint free_node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            IList<IList<uint>> PCWaveguidePorts,
            double minNormalizedFreq,
            double maxNormalizedFreq,
            KrdLab.clapack.Complex complexNormalizedFreq,
            KrdLab.clapack.Complex[] fieldVec,
            out double ret_norm)
        {
            bool isHit = false;
            ret_norm = 0.0;
            if (complexNormalizedFreq.Real > 0.0 && Math.Abs(complexNormalizedFreq.Imaginary) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
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
                    System.Diagnostics.Debug.WriteLine("norm (prev * current): {0} + {1}i (Abs: {2})", norm12.Real, norm12.Imaginary, norm12.Magnitude);
                    if (complexNormalizedFreq.Real >= minNormalizedFreq && complexNormalizedFreq.Real <= maxNormalizedFreq)
                    {
                        isHit = true;
                        ret_norm = norm12.Magnitude;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("skip: a/λ = {0} + {1} i", complexNormalizedFreq.Real, complexNormalizedFreq.Imaginary);
                    }
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
                    if (KrdLab.clapack.Complex.Abs(KrdLab.clapack.Complex.Conjugate(A[i + matLen * j]) - A[j + matLen * i]) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                    {
                        isHermitianA = false;
                        break;
                    }
                    // [B]のエルミート行列チェック
                    if (KrdLab.clapack.Complex.Abs(KrdLab.clapack.Complex.Conjugate(B[i + matLen * j]) - B[j + matLen * i]) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
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
        /// 規格化伝搬定数の取得
        /// </summary>
        /// <param name="betaIndex"></param>
        /// <returns></returns>
        private static double getBeta(
            ref int betaIndex,
            double Beta1,
            double Beta2,
            double BetaDelta)
        {
            double beta = betaIndex * BetaDelta + Beta1;
            if (beta > Beta2)
            {
                //最初に戻る
                //betaIndex = 0;
                //beta = Beta1;

                // 終了
                betaIndex = -1;
                beta = Beta2;
            }
            else if (betaIndex == -1)
            {
                beta = Beta2;
            }
            return beta;
        }

        /// <summary>
        /// 伝搬定数の計算結果表示
        /// </summary>
        private void drawEigenResults(int probNo)
        {
            if (EigenValueList.Count > 0)
            {
                int dispBetaIndex = EigenValueList.Count - 1;
                double beta = getBeta(
                    ref dispBetaIndex,
                    Beta1,
                    Beta2,
                    BetaDelta);

                Complex evalue = EigenValueList[dispBetaIndex];
                // 伝搬定数を表示
                drawString(10, 45, 0.0, 0.7, 1.0, string.Format("beta:{0:F4} beta*d/(2.0 * pi):{1:F4} beta/k0:{2:F4}",
                    beta,
                    beta * PeriodicDistance / (2.0 * pi),
                    ((LatticeA != 0 && Math.Abs(evalue.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit) ? beta / (2.0 * pi * evalue.Real / LatticeA) : 0)
                    ));
                // 固有周波数を表示
                drawString(10, 60, 0.0, 0.7, 1.0, string.Format("a/lambda:{0:F4}", evalue.Real));

                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                // 伝搬定数の周波数特性データをグラフ表示用バッファに格納
                int dataCnt = EigenValueList.Count;
                int axisYCnt = 2;
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
                    valueX[i] = getBeta(
                        ref workFreqIndex,
                        Beta1,
                        Beta2,
                        BetaDelta);
                    valueX[i] *= PeriodicDistance / (2.0 * pi);
                    for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                    {
                        if (axisYIndex == 0)
                        {
                            valueYs[axisYIndex][i] = EigenValueList[i].Real;
                        }
                        else if (axisYIndex == 1)
                        {
                            // light line
                            valueYs[axisYIndex][i] = valueX[i] * LatticeA / PeriodicDistance;
                        }
                    }
                }

                // 周波数特性グラフの描画
                double graphWidth = 350 * win_w / (double)DefWinWidth;
                double graphHeight = 170 * win_w / (double)DefWinWidth;
                double graphX = 45;
                double graphY = 0;
                if (probNo == 4)
                {
                    graphY = win_h - graphHeight - 20;
                }
                else
                {
                    graphY = win_h - graphHeight - 20 - 150;
                }
                double minXValue = Beta1 * PeriodicDistance / (2.0 * pi);
                double maxXValue = Beta2 * PeriodicDistance / (2.0 * pi);
                double minYValue = MinNormalizedFreq;
                double maxYValue = MaxNormalizedFreq;
                double intervalXValue = GraphBetaInterval * PeriodicDistance / (2.0 * pi);
                double intervalYValue = GraphFreqInterval;
                IList<string> valuesYTitles = null;
                valuesYTitles = new List<string>() { "a/lambda", "light" };
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
                drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F2}", x));
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
                drawString((int)(xxmin - 45), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F3}", y));
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
