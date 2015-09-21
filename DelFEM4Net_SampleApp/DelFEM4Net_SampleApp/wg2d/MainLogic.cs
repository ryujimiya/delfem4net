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
            ref double MinSParameter,
            ref double MaxSParameter,
            ref double GraphSParameterInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
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
        private const int ProbCnt = 4;
        /// <summary>
        /// 最初に表示する問題番号
        /// </summary>
        private const int DefProbNo = 1;
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
        private const int DefWinWidth = 800;//420;
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private const int DefWinHeight = 700;//380;

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
        private CZLinearSystem Ls = null;
        /// <summary>
        /// プリコンディショナ―（複素数版)
        /// </summary>
        private CZPreconditioner_ILU Prec = null;

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
        /// 散乱係数の最小値
        /// </summary>
        private double MinSParameter = 0.0;
        /// <summary>
        /// 散乱係数の最大値
        /// </summary>
        private double MaxSParameter = 1.0;
        /// <summary>
        /// グラフの散乱係数目盛間隔
        /// </summary>
        private double GraphSParameterInterval = 0.2;

        /// <summary>
        /// 導波管の幅
        /// </summary>
        private double WaveguideWidth = 2.0;
        /// <summary>
        /// 波のモード区分
        /// </summary>
        private WgUtil.WaveModeDV WaveModeDv = WgUtil.WaveModeDV.TE;
        /// <summary>
        /// ループのフィールドID
        /// </summary>
        private uint FieldLoopId = 0;
        /// <summary>
        /// 値のフィールドID
        /// </summary>
        private uint FieldValId = 0;
        /// <summary>
        /// 強制境界のフィールドID
        /// </summary>
        private uint FieldForceBcId = 0;
        // ポート１の境界フィールドID
        private uint FieldPortBcId1 = 0;
        // ポート２の境界フィールドID
        private uint FieldPortBcId2 = 0;

        /// <summary>
        /// 媒質リスト
        /// </summary>
        IList<MediaInfo> Medias = new List<MediaInfo>();
        /// <summary>
        /// ループID→ループの情報のマップ
        /// </summary>
        Dictionary<uint, World.Loop> LoopDic = new Dictionary<uint, World.Loop>();
        /// <summary>
        /// 辺ID→辺の情報のマップ
        /// </summary>
        Dictionary<uint, World.Edge> EdgeDic = new Dictionary<uint, World.Edge>();
        /// <summary>
        /// <summary>
        /// 界の絶対値を表示する？
        /// </summary>
        private bool IsShowAbsField = false;

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
        /// 散乱パラメータ(各周波数に対するS11とS21)のリスト
        /// </summary>
        private IList<IList<Complex[]>> ScatterVecList = null;

        /// <summary>
        /// 図面を表示する？
        /// </summary>
        private bool IsCadShow = false;
        /// <summary>
        /// 図面表示用描画オブジェクトアレイ
        /// </summary>
        private CDrawerArray CadDrawerAry = null;

        /// <summary>
        /// カメラ初期化済み？
        /// </summary>
        private bool IsInitedCamera = true;
        /// <summary>
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
            Ls = new CZLinearSystem();
            Prec = new CZPreconditioner_ILU();
            ScatterVecList = new List<IList<Complex[]>>();
            CadDrawerAry = new CDrawerArray();
            IsInitedCamera = true;

            // Glutのアイドル時処理でなく、タイマーで再描画イベントを発生させる
            MyTimer.Tick +=(sender, e) =>
            {
                if (IsTimerProcRun)
                {
                    return;
                }
                IsTimerProcRun = true;
                if (IsAnimation && FreqIndex != -1 && !IsCadShow)
                {
                    // 問題を解く
                    bool ret = solveProblem(
                        (IsInitedCamera? false : true)
                        );
                    if (FreqIndex != -1)
                    {
                        FreqIndex++;
                    }
                    if (ret)
                    {
                        if (!IsInitedCamera)
                        {
                            IsInitedCamera = true;
                        }
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
            if (Ls != null)
            {
                Ls.Clear();
                Ls.Dispose();
                Ls = null;
            }
            if (Prec != null)
            {
                Prec.Clear();
                Prec.Dispose();
                Prec = null;
            }
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
            IsInitedCamera = false;

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
                drawScatterResults();
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
                if (!IsCadShow)
                {
                    IsInitedCamera = false;
                }
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
                ScatterVecList.Clear();
                // 問題を作成
                setProblem();
                if (!IsCadShow)
                {
                    IsInitedCamera = false;
                }
                FreqIndex =0;
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
                ref MinSParameter,
                ref MaxSParameter,
                ref GraphSParameterInterval,
                ref WaveModeDv,
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
        /// <param name="probNo">問題番号</param>
        /// <param name="WaveguideWidth">波長</param>
        /// <param name="NormalizedFreq1">開始規格化周波数</param>
        /// <param name="NormalizedFreq2">終了規格化周波数</param>
        /// <param name="FreqDelta">計算刻み幅</param>
        /// <param name="GraphFreqInterval">グラフの周波数目盛幅</param>
        /// <param name="MinSParameter">最小散乱係数</param>
        /// <param name="MaxSParameter">最大散乱係数</param>
        /// <param name="GraphSParameterInterval">グラフの散乱係数目盛幅</param>
        /// <param name="WaveModeDv">波のモード区分</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldValId">値のフィールドID</param>
        /// <param name="FieldLoopId">ループのフィールドID</param>
        /// <param name="FieldForceBcId">強制境界のフィールドID</param>
        /// <param name="FieldPortBcId1">ポート１のフィールドID</param>
        /// <param name="FieldPortBcId2">ポート２のフィールドID</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="isCadShow">図面表示する？</param>
        /// <param name="CadDrawerAry">図面表示用描画オブジェクトアレイ</param>
        /// <param name="Camera">カメラ</param>
        /// <returns></returns>
        private static bool setProblem(
            int probNo,
            double WaveguideWidth,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double FreqDelta,
            ref double GraphFreqInterval,
            ref double MinSParameter,
            ref double MaxSParameter,
            ref double GraphSParameterInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
             ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref uint FieldPortBcId1,
            ref uint FieldPortBcId2,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, World.Loop> LoopDic,
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

            // 波のモード区分
            WaveModeDv = WgUtil.WaveModeDV.TE;
            //isCadShow = false;

            SetProblemProcDelegate func = null;
            try
            {

                if (probNo == 0)
                {
                    // 直線導波管
                    func = Problem00.SetProblem;
                }
                else if (probNo == 1)
                {
                    // 導波管ベンド
                    func = Problem01.SetProblem;
                }
                else if (probNo == 2)
                {
                    // 誘電体のボックス装荷導波管
                    func = Problem02.SetProblem;
                }
                else if (probNo == 3)
                {
                    // 誘電体スラブ導波路グレーティング
                    func = Problem03.SetProblem;
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
                    ref MinSParameter,
                    ref MaxSParameter,
                    ref GraphSParameterInterval,
                    ref WaveModeDv,
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
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
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
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPortBcId1,
                FieldPortBcId2,
                Medias,
                LoopDic,
                EdgeDic,
                IsShowAbsField,
                ref Ls,
                ref Prec,
                ref ScatterVecList,
                ref DrawerAry,
                Camera
                );
            return ret;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <param name="probNo">問題番号</param>
        /// <param name="freqIndex">周波数のインデックス</param>
        /// <param name="NormalizedFreq1">開始規格化周波数</param>
        /// <param name="NormalizedFreq2">終了規格化周波数</param>
        /// <param name="FreqDelta">計算刻み幅</param>
        /// <param name="initFlg">カメラ初期化フラグ</param>
        /// <param name="WaveguideWidth">導波路幅</param>
        /// <param name="WaveModeDv">波のモード区分</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldValId">値のフィールドID</param>
        /// <param name="FieldLoopId">ループのフィールドID</param>
        /// <param name="FieldForceBcId">強制境界のフィールドID</param>
        /// <param name="FieldPortBcId1">ポート１境界のフィールドID</param>
        /// <param name="FieldPortBcId2">ポート２境界のフィールドID</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="IsShowAbsField">絶対値表示する？</param>
        /// <param name="Ls">リニアシステム</param>
        /// <param name="Prec">プリコンディショナ―</param>
        /// <param name="ScatterVecList">散乱係数リスト（ポート毎のモード散乱係数リストのリスト）</param>
        /// <param name="DrawerAry">描画オブジェクトアレイ</param>
        /// <param name="Camera">カメラ</param>
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
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            uint FieldPortBcId1,
            uint FieldPortBcId2,
            IList<MediaInfo> Medias,
            Dictionary<uint, World.Loop> LoopDic,
            Dictionary<uint, World.Edge> EdgeDic,
            bool IsShowAbsField,
            ref CZLinearSystem Ls,
            ref CZPreconditioner_ILU Prec,
            ref IList<IList<Complex[]>> ScatterVecList,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            bool success = false;
            // 入射モードインデックス
            int incidentModeIndex = 0;
            // ポート数
            int portCnt = 2;
            int incidentPortNo = getIncidentPortNo();
            int propModeCnt_port1 = 1;
            int propModeCnt_port2 = 1;
            // モード電力の合計を出力として表示する
            bool isShowTotalPortPower = false;
            if (probNo == 3)
            {
                /*
                // 入力5モード表示
                //propModeCnt_port1 = 5;
                //propModeCnt_port2 = 1;
                // 出力5モード表示
                //propModeCnt_port1 = 1;
                //propModeCnt_port2 = 5;
                propModeCnt_port1 = 3;
                propModeCnt_port2 = 3;
                 */
                //isShowTotalPortPower = true; // モード電力の合計を出力として表示する
            }
            try
            {

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
                double k0 = normalizedFreq * pi / WaveguideWidth;
                // 波長
                double waveLength = 2.0 * pi / k0;
                System.Diagnostics.Debug.WriteLine("2W/λ:{0}", normalizedFreq);

                //------------------------------------------------------------------
                // リニアシステム
                //------------------------------------------------------------------
                Ls.Clear();
                Prec.Clear();
                WgUtil.GC_Collect();

                //------------------------------------------------------------------
                // 界パターン追加
                //------------------------------------------------------------------
                // ワールド座標系のフィールド値をクリア
                WgUtil.ClearFieldValues(World, FieldValId);
                // 領域全体の界パターンを追加
                Ls.AddPattern_Field(FieldValId, World);

                // 上記界パターンでは、隣接する要素以外の剛性行列の値は追加できないようです。境界用に追加パターンを作成
                uint[] fieldPortBcId_list = {FieldPortBcId1, FieldPortBcId2};
                System.Diagnostics.Debug.Assert(fieldPortBcId_list.Length == portCnt);
                foreach (uint workFieldPortBcId in fieldPortBcId_list)
                {
                    WgUtil.MakeWaveguidePortBCPattern(Ls, World, workFieldPortBcId);
                }

                //------------------------------------------------------------------
                // 固定境界条件を設定
                //------------------------------------------------------------------
                if (FieldForceBcId != 0)
                {
                    Ls.SetFixedBoundaryCondition_Field(FieldForceBcId, 0, World); // 固定境界条件を設定
                }
                uint[] fixedBcNodes = null;
                // 強制境界条件を課す節点を取得する
                // 境界の固有モード解析で強制境界を指定する際に使用する(CZLinearSystemのBCFlagを参照できないので)
                Dictionary<uint, uint> tmp_to_no_boundary = null;
                if (FieldForceBcId != 0)
                {
                    WgUtil.GetBoundaryNodeList(World, FieldForceBcId, out fixedBcNodes, out tmp_to_no_boundary);
                }
                //------------------------------------------------------------------
                // プリコンディショナ―
                //------------------------------------------------------------------
                // set Preconditioner
                //Prec.SetFillInLevel(1);
                uint fillInLevel = 1;
                if (probNo == 3)
                {
                    // 誘電体スラブ導波路ではILU(1)では収束しないので、フィルインレベルを大きくとる
                    fillInLevel = 15;
                }
                System.Diagnostics.Debug.WriteLine("fillInLevel: {0}", fillInLevel);
                Prec.SetFillInLevel(fillInLevel);

                // ILU(0)のパターン初期化
                Prec.SetLinearSystem(Ls);

                //------------------------------------------------------------------
                // 剛性行列、残差ベクトルのマージ
                //------------------------------------------------------------------
                Ls.InitializeMarge();

                IList<double[,]> ryy_1d_port_list = new List<double[,]>();
                IList<Complex[]> eigen_values_port_list = new List<Complex[]>();
                IList<Complex[,]> eigen_vecs_port_list = new List<Complex[,]>();
                System.Diagnostics.Debug.Assert(incidentPortNo == 1);

                // 境界の界に導波路開口条件を追加
                bool retAddPort = false;

                // 境界の界に導波路開口条件を追加
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    uint workFieldPortBcId = fieldPortBcId_list[portIndex]; 
                    double[,] ryy_1d_port1 = null;
                    Complex[] eigen_values_port1 = null;
                    Complex[,] eigen_vecs_port1 = null;
                    retAddPort = WgUtil.AddLinSys_WaveguidePortBC(
                        Ls,
                        waveLength,
                        WaveModeDv,
                        World,
                        workFieldPortBcId,
                        fixedBcNodes,
                        (portIndex == (incidentPortNo - 1)),
                        incidentModeIndex,
                        Medias,
                        EdgeDic,
                        out ryy_1d_port1,
                        out eigen_values_port1,
                        out eigen_vecs_port1);
                    // 格納する
                    ryy_1d_port_list.Add(ryy_1d_port1);
                    eigen_values_port_list.Add(eigen_values_port1);
                    eigen_vecs_port_list.Add(eigen_vecs_port1);
                    if (!retAddPort)
                    {
                        System.Diagnostics.Debug.WriteLine("failed addPort (port{0})", (portIndex + 1));
                        // 表示用にデータを格納する
                        if (freqIndex == 0)
                        {
                            ScatterVecList.Clear();
                        }
                        {
                            IList<Complex[]> work_portScatterVecList = new List<Complex[]>();
                            for (int p = 0; p < portCnt; p++)
                            {
                                int propModeCnt = 0;
                                if (p == 0)
                                {
                                    propModeCnt = propModeCnt_port1;
                                }
                                else if (p == 1)
                                {
                                    propModeCnt = propModeCnt_port2;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Assert(false);
                                }
                                Complex[] work_scatterVec = new Complex[propModeCnt];
                                for (int imode = 0; imode < propModeCnt; imode++)
                                {
                                    work_scatterVec[imode] = 0;
                                }
                                work_portScatterVecList.Add(work_scatterVec);
                            }
                            ScatterVecList.Add(work_portScatterVecList);
                        }
                        return false;
                    }
                    System.Diagnostics.Debug.WriteLine("port{0} node_cnt: {1}", (portIndex + 1), eigen_vecs_port_list[portIndex].GetLength(1));
                }

                // 領域
                WgUtil.AddLinSysHelmholtz(Ls, waveLength, World, FieldLoopId, Medias, LoopDic);

                double res = Ls.FinalizeMarge();
                //System.Diagnostics.Debug.WriteLine("Residual : " + res);

                //------------------------------------------------------------------
                // リニアシステムを解く
                //------------------------------------------------------------------
                // プリコンディショナに値を設定してILU分解を行う
                Prec.SetValue(Ls);
                double tol = 1.0e-6;
                uint maxIter = 2000;
                uint iter = maxIter;
                CZSolverLsIter.Solve_PCOCG(ref tol, ref iter, Ls, Prec);
                if (iter == maxIter)
                {
                    Console.WriteLine("Not converged at 2W/λ = {0}", normalizedFreq);
                    System.Diagnostics.Debug.WriteLine("Not converged at 2W/λ = {0}", normalizedFreq);
                }
                //System.Diagnostics.Debug.WriteLine("Solved!");

                // 計算結果をワールド座標系に反映する
                Ls.UpdateValueOfField(FieldValId, World, FIELD_DERIVATION_TYPE.VALUE);

                //------------------------------------------------------------------
                // 計算結果の後処理
                //------------------------------------------------------------------
                // 反射、透過係数
                double totalPower = 0.0;
                IList<Complex[]> portScatterVecList = new List<Complex[]>();
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    Complex[] eigen_values_port1 = eigen_values_port_list[portIndex];
                    double[,] ryy_1d_port1 = ryy_1d_port_list[portIndex];
                    Complex[,] eigen_vecs_port1 = eigen_vecs_port_list[portIndex];
                    int propModeCnt = 0;
                    if (portIndex == 0)
                    {
                        propModeCnt = propModeCnt_port1;
                    }
                    else if (portIndex == 1)
                    {
                        propModeCnt = propModeCnt_port2;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    Complex[] scatterVec = new Complex[propModeCnt];
                    for (int m = 0; m < propModeCnt; m++)
                    {
                        scatterVec[m] = 0.0;
                    }

                    uint workFieldPortBcId = fieldPortBcId_list[portIndex];
                    for (uint imode = 0; imode < eigen_values_port1.Length; imode++)
                    {
                        Complex s11 = WgUtil.GetWaveguidePortReflectionCoef(
                            Ls,
                            waveLength,
                            WaveModeDv,
                            World,
                            workFieldPortBcId,
                            imode,
                            ((portIndex == (incidentPortNo - 1)) && imode == incidentModeIndex),
                            ryy_1d_port1,
                            eigen_values_port1,
                            eigen_vecs_port1);
                        if (imode < propModeCnt || Math.Abs(eigen_values_port1[imode].Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        {
                            System.Diagnostics.Debug.WriteLine("    s" + (portIndex + 1) + incidentPortNo + " = (" + s11.Real + "," + s11.Imag + ")" + " |s" + (portIndex + 1) + incidentPortNo + "|^2 = " + Complex.SquaredNorm(s11) + ((imode == 0) ? "  incident" : ""));
                        }
                        if (Math.Abs(eigen_values_port1[imode].Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                            && Math.Abs(eigen_values_port1[imode].Imag) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit) // 伝搬モード
                        {
                            totalPower += Complex.SquaredNorm(s11);
                        }
                        if (isShowTotalPortPower)
                        {
                            if (Math.Abs(eigen_values_port1[imode].Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && Math.Abs(eigen_values_port1[imode].Imag) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit) // 伝搬モード
                            {
                                // ポート電力を計算
                                scatterVec[0] += Complex.Norm(s11) * Complex.Norm(s11);
                            }
                        }
                        else
                        {
                            if (imode < propModeCnt)
                            {
                                scatterVec[imode] = s11;
                            }
                        }
                    }
                    portScatterVecList.Add(scatterVec);
                }
                System.Diagnostics.Debug.WriteLine("    totalPower = {0}", totalPower);
                if (isShowTotalPortPower)
                {
                    for (int p = 0; p < portCnt; p++)
                    {
                        // ルート電力に変換
                        Complex[] scatterVec = portScatterVecList[p];
                        double sqrtPower = Math.Sqrt(Complex.Norm(scatterVec[0]));
                        scatterVec[0] = sqrtPower;
                        //portScatterVecList[p] = scatterVec;
                        System.Diagnostics.Debug.WriteLine("port total sqrt power s{0}{1} : {2}", (p + 1), incidentPortNo, sqrtPower);
                    }
                }

                // 表示用にデータを格納する
                if (freqIndex == 0)
                {
                    ScatterVecList.Clear();
                }
                {
                    ScatterVecList.Add(portScatterVecList);
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
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                // 表示用にデータを格納する
                if (freqIndex == 0)
                {
                    ScatterVecList.Clear();
                }
                {
                    IList<Complex[]> work_portScatterVecList = new List<Complex[]>();
                    for (int p = 0; p < portCnt; p++)
                    {
                        int propModeCnt = 0;
                        if (p == 0)
                        {
                            propModeCnt = propModeCnt_port1;
                        }
                        else if (p == 1)
                        {
                            propModeCnt = propModeCnt_port2;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        Complex[] work_scatterVec = new Complex[propModeCnt];
                        for (int imode = 0; imode < propModeCnt; imode++)
                        {
                            work_scatterVec[imode] = 0;
                        }
                        work_portScatterVecList.Add(work_scatterVec);
                    }
                    ScatterVecList.Add(work_portScatterVecList);
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
            if (probNo == 0)
            {
                Camera.MousePan(0, 0, -0.14, 0.45);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 1)
            {
                Camera.MousePan(0, 0, 0.14, 0.42);
                double tmp_scale = 0.85;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 2)
            {
                Camera.MousePan(0, 0, -0.1, 0.35);
                double tmp_scale = 0.9;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 3)
            {
                Camera.MousePan(0, 0, 0.06, 0.53);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else
            {
                Camera.MousePan(0, 0, -0.1, 0.25);
                double tmp_scale = 1.0;
                Camera.SetScale(tmp_scale);
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
            //if (Math.Abs(normalizedFreq - NormalizedFreq1) < 1.0e-12)
            //{
            //    normalizedFreq += 1.0e-4;
            //}
            return normalizedFreq;
        }

        /// <summary>
        /// 入射ポート番号(>= 1)を取得する
        /// </summary>
        /// <returns></returns>
        private static int getIncidentPortNo()
        {
            int incidentPortNo = 1;
            return incidentPortNo;
        }

        /// <summary>
        /// 散乱パラメータの計算結果表示
        /// </summary>
        private void drawScatterResults()
        {
            if (ScatterVecList.Count > 0)
            {
                int dispFreqIndex = ScatterVecList.Count - 1;
                double normalizedFreq = getNormalizedFreq(
                    ref dispFreqIndex,
                    NormalizedFreq1,
                    NormalizedFreq2,
                    FreqDelta);
                int incidentPortNo = getIncidentPortNo();

                // 周波数を表示
                drawString(10, 45, 0.0, 0.7, 1.0, string.Format("2W/lamda:{0}", normalizedFreq));
                // S11とS21の数値を表示
                IList<Complex[]> latest_portScatterVecList = ScatterVecList[dispFreqIndex];
                int axisYCnt = 0;
                int portCnt = latest_portScatterVecList.Count;
                int drawX = 10;
                int drawY = 60;
                for (int portIndex = 0, axisYIndex = 0; portIndex < portCnt; portIndex++)
                {
                    Complex[] scatterVec = latest_portScatterVecList[portIndex];
                    if (scatterVec.Length == 1)
                    {
                        uint imode = 0;
                        drawString(drawX, drawY, 0.0, 0.7, 1.0, string.Format("S{0}{1}:{2:F4}", (portIndex + 1), incidentPortNo, Complex.Norm(scatterVec[imode])));
                        drawX += 100;
                        if (drawX >= 400)
                        {
                            drawX = 10;
                            drawY += 15;
                        }
                        axisYIndex++;
                    }
                    else
                    {
                        for (uint imode = 0; imode < scatterVec.Length; imode++)
                        {
                            drawString(drawX, drawY, 0.0, 0.7, 1.0, string.Format("S{0}({1}){2}:{3:F4}",
                                (portIndex + 1), (imode + 1), incidentPortNo, Complex.Norm(scatterVec[imode])));
                            drawX += 115;
                            if (drawX >= 200)
                            {
                                drawX = 10;
                                drawY += 15;
                            }
                            axisYIndex++;
                        }
                    }
                    axisYCnt = axisYIndex;
                }

                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                // Sパラメータの周波数特性データをグラフ表示用バッファに格納
                int dataCnt = ScatterVecList.Count;
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
                    IList<Complex[]> work_portScatterVecList = ScatterVecList[i];
                    for (int portIndex = 0, axisYIndex = 0; portIndex < portCnt; portIndex++)
                    {
                        Complex[] scatterVec = work_portScatterVecList[portIndex];
                        for (uint imode = 0; imode < scatterVec.Length; imode++)
                        {
                            valueYs[axisYIndex][i] = Complex.Norm(scatterVec[imode]);
                            axisYIndex++;
                        }
                    }
                }

                // 周波数特性グラフの描画
                //double graphWidth = 200 * win_w / (double)DefWinWidth;
                //double graphHeight = 100 * win_w / (double)DefWinWidth;
                double graphWidth = 500 * win_w / (double)DefWinWidth;
                double graphHeight = 250 * win_w / (double)DefWinWidth;
                double graphX = 40;
                double graphY = win_h - graphHeight - 20;
                double minXValue = NormalizedFreq1;
                double maxXValue = NormalizedFreq2;
                double minYValue = MinSParameter;
                double maxYValue = MaxSParameter;
                double intervalXValue = GraphFreqInterval;
                double intervalYValue = GraphSParameterInterval;
                IList<string> valuesYTitles = new List<string>();
                if (ScatterVecList.Count > 0)
                {
                    IList<Complex[]> work_portScatterVecList = ScatterVecList[0];
                    for (int portIndex = 0, axisYIndex = 0; portIndex < portCnt; portIndex++)
                    {
                        Complex[] scatterVec = work_portScatterVecList[portIndex];
                        if (scatterVec.Length == 1)
                        {
                            valuesYTitles.Add(string.Format("S{0}{1}", (portIndex + 1), incidentPortNo));
                            axisYIndex++;
                        }
                        else
                        {
                            for (uint imode = 0; imode < scatterVec.Length; imode++)
                            {
                                valuesYTitles.Add(string.Format("S{0}({1}){2}", (portIndex + 1), (imode + 1), incidentPortNo));
                                axisYIndex++;
                            }
                        }
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
            double[,] graphLineColors = new double[6, 3]
            {
                {0.0, 0.0, 1.0},
                {1.0, 0.0, 1.0},
                {0.0, 1.0, 0.0},
                {1.0, 0.5, 0.0},
                {0.0, 0.5, 0.0},
                {0.5, 0.5, 0.0},
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
                drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F3}", x));
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
                drawString((int)(xxmin - 30), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F1}", y));
            }

            // 凡例
            const int maxAxisOneLine = 4;
            int maxLineCnt = (valueYs.Count - 1) / maxAxisOneLine + 1;
            for (int iAxisY = 0; iAxisY < valueYs.Count; iAxisY++)
            {
                int colorIndex = (iAxisY % graphLineColors.GetLength(0));
                Gl.glColor3d(graphLineColors[colorIndex, 0], graphLineColors[colorIndex, 1], graphLineColors[colorIndex, 2]);
                Gl.glLineWidth(1.0f);
                Gl.glBegin(Gl.GL_LINES);
                double xx = graphAreaX + graphAreaWidth - 95 * maxAxisOneLine + (iAxisY % maxAxisOneLine) * 95;
                double yy = graphAreaY - 15 - (maxLineCnt - 1 - iAxisY / maxAxisOneLine) * 15;

                Gl.glVertex2d(xx, yy);
                Gl.glVertex2d(xx + 30, yy);
                Gl.glEnd();
                drawString((int)(xx + 40), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], valueYTitles[iAxisY]);
            }

            // データのプロット
            int dataCnt = valueX.Length;
            for (int iAxisY = 0; iAxisY < valueYs.Count; iAxisY++)
            {
                double[] valueY = valueYs[iAxisY];
                int colorIndex = (iAxisY % graphLineColors.GetLength(0));
                Gl.glColor3d(graphLineColors[colorIndex, 0], graphLineColors[colorIndex, 1], graphLineColors[colorIndex, 2]);
                //Gl.glPointSize(2.0f);
                //Gl.glBegin(Gl.GL_POINTS);
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
