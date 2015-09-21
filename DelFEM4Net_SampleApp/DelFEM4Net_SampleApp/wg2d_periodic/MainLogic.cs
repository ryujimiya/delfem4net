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

namespace wg2d_periodic
{
    ///////////////////////////////////////////////////////////////
    //  DelFEM4Net サンプル
    //  入出力が周期構造導波路の伝達問題 2D
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
            ref IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool IsInoutWgSame,
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
        /// 問題番号リスト
        /// </summary>
        private static readonly int[] ProbNoList = 
        {
            //0, // 直線導波管
            //1, // PC欠陥導波路 直線
            //2, // PC導波路　ベンド
            3, // PC導波路　ベンド最適形状
            4, // PC導波路 4ポート方向性結合器
            //5, // PC導波路 4ポート方向性結合器(ポート2入射)
            6, // PC導波路 マルチモード干渉(MMI)
            7, // PC導波路 マルチモード干渉結合器(MMI directional coupler)
            //8, // PC導波路 三角形格子 直線
            //9, // PC導波路 45°三角形格子 誘電体ロッド 90°ベンド
            10, // PC導波路 60°三角形格子 60°ベンド
            11, // PC導波路 60°三角形格子 エアホール型60°-30°三角形格子ベンド
            12, // PC導波路 60°三角形格子 ダブルベンド
            13, // PC導波路 60°三角形格子 方向性結合器 (60°ベンド)
            14, // PC導波路 60°三角形格子 Wavelength division demultiplexer(2 ladder)
            15, // PC導波路 三角形格子 空洞共振器
        };
        /// <summary>
        /// 最初の問題番号
        /// </summary>
        private const int DefProbNo = 14;//3;
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
        /// 問題番号インデックス
        /// </summary>
        private int ProbNoIndex = 0;
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
        private double WaveguideWidth = 1.0;
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
        /// <summary>
        /// 入出力導波路情報リスト
        /// </summary>
        private IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList = new List<WgUtilForPeriodicEigenExt.WgPortInfo>();

        /// <summary>
        /// 媒質リスト
        /// </summary>
        IList<MediaInfo> Medias = new List<MediaInfo>();
        /// <summary>
        /// ループID→ループの情報のマップ
        /// </summary>
        Dictionary<uint, wg2d.World.Loop> LoopDic = new Dictionary<uint, wg2d.World.Loop>();
        /// <summary>
        /// 辺ID→辺の情報のマップ
        /// </summary>
        Dictionary<uint, wg2d.World.Edge> EdgeDic = new Dictionary<uint, wg2d.World.Edge>();

        /// <summary>
        /// 入出力導波路が同じ？
        /// </summary>
        private bool IsInoutWgSame = true;
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
            ProbNoIndex = 0;
            int defProbNoIndex = ProbNoList.ToList().IndexOf(DefProbNo);
            if (defProbNoIndex >= 0)
            {
                ProbNoIndex = defProbNoIndex;
            }
            int probNo = ProbNoList[ProbNoIndex];
            System.Diagnostics.Debug.WriteLine("Problem No: {0}", probNo);
            Console.WriteLine("Problem No: {0}", probNo);
            FreqIndex = 0;
            // 問題を作成
            setProblem(
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
                ref WgPortInfoList,
                ref Medias,
                ref LoopDic,
                ref EdgeDic,
                ref IsInoutWgSame,
                ref IsCadShow,
                ref CadDrawerAry,
                ref Camera
                );
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
                ProbNoIndex++;
                if (ProbNoIndex == ProbNoList.Length) ProbNoIndex = 0;
                int probNo = ProbNoList[ProbNoIndex];
                System.Diagnostics.Debug.WriteLine("Problem No: {0}", probNo);
                Console.WriteLine("Problem No: {0}", probNo);
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
            int probNo = ProbNoList[ProbNoIndex];
            bool ret = setProblem(
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
                ref WgPortInfoList,
                ref Medias,
                ref LoopDic,
                ref EdgeDic,
                ref IsInoutWgSame,
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
        /// <param name="FieldLoopId">ループの値のフィールドID</param>
        /// <param name="FieldForceBcId">強制境界の値のフィールドID</param>
        /// <param name="WgPortInfoList">入出力導波路情報リスト</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="IsInoutWgSame">同じ入出力導波路？</param>
        /// <param name="isCadShow">図面を表示する？</param>
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
            ref IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool IsInoutWgSame,
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

            // 入出力導波路のループ情報、辺情報
            WgPortInfoList.Clear();

            // 入出力導波路が同じ？
            IsInoutWgSame = true;

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
                    // PC導波路 直線
                    func = Problem01.SetProblem;
                }
                else if (probNo == 2 || probNo == 3)
                {
                    // PC導波路ベンド
                    func = Problem02.SetProblem;
                }
                else if (probNo == 4 || probNo == 5)
                {
                    // PC導波路 4ポート方向性結合器
                    func = Problem04.SetProblem;
                }
                else if (probNo == 6)
                {
                    // PC導波路 マルチモード干渉(MMI)
                    func = Problem06.SetProblem;
                }
                else if (probNo == 7)
                {
                    // PC導波路 マルチモード干渉結合器(MMI directional coupler)
                    func = Problem07.SetProblem;
                }
                else if (probNo == 8)
                {
                    // PC導波路 三角形格子 直線
                    func = Problem08.SetProblem;
                }
                else if (probNo == 9 || probNo == 11)
                {
                    // PC導波路 三角形格子 90°ベンド
                    // 問題09: 45°三角形格子
                    // 問題11: 60°- 30°三角形格子
                    func = Problem09.SetProblem;
                }
                else if (probNo == 10)
                {
                    // PC導波路 三角形格子 60°ベンド
                    func = Problem10.SetProblem;
                }
                else if (probNo == 12)
                {
                    // PC導波路 三角形格子 60°ダブルベンド
                    func = Problem12.SetProblem;
                }
                else if (probNo == 13)
                {
                    // PC導波路 三角形格子 方向性結合器(60°ベンド)
                    func = Problem13.SetProblem;
                    //func = Problem13_2.SetProblem; // 方向性結合器 2チャンネル入力(片方チャンネル入射)
                    //func = Problem13_3.SetProblem; // 2チャンネル直線(片方チャンネル入射)
                    //func = Problem13_4.SetProblem; // スルー出力側にベンド
                    //func = Problem13_5.SetProblem; // ドロップ出力チャンネル終端なし(4ポート)
                    //func = Problem13_5_2.SetProblem; // ドロップ出力チャンネルを曲げて終端
                }
                else if (probNo == 14)
                {
                    // PC導波路 三角形格子 Wavelength division demultiplexer(1 ladder)
                    //func = Problem14.SetProblem; // Wavelength division demultiplexer 1段
                    func = Problem14_2.SetProblem; // Wavelength division demultiplexer 2段
                }
                else if (probNo == 15)
                {
                    // PC導波路 三角形格子 共振器
                    //func = Problem15.SetProblem; // 共振器 阻止型
                    //func = Problem15_2.SetProblem;  // 共振器 通過型
                    func = Problem15_3.SetProblem;  // 共振器 通過型 3ポート
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
                    ref WgPortInfoList,
                    ref Medias,
                    ref LoopDic,
                    ref EdgeDic,
                    ref IsInoutWgSame,
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
        /// <returns></returns>
        private bool solveProblem(bool initFlg)
        {
            int probNo = ProbNoList[ProbNoIndex];
            bool ret = solveProblem(
                probNo,
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
                WgPortInfoList,
                Medias,
                LoopDic,
                EdgeDic,
                IsInoutWgSame,
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
        /// <param name="freqIndex">計算する周波数のインデックス</param>
        /// <param name="NormalizedFreq1">開始規格化周波数</param>
        /// <param name="NormalizedFreq2">終了規格化周波数</param>
        /// <param name="FreqDelta">計算刻み幅</param>
        /// <param name="initFlg">カメラ初期化フラグ</param>
        /// <param name="WaveguideWidth">波長</param>
        /// <param name="WaveModeDv">波のモード区分</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldValId">値のフィールドID</param>
        /// <param name="FieldLoopId">ループの値のフィールドID</param>
        /// <param name="FieldForceBcId">強制境界の値のフィールドID</param>
        /// <param name="WgPortInfoList">入出力導波路情報リスト</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="IsInoutWgSame">同じ入出力導波路？</param>
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
            IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            bool IsInoutWgSame,
            bool IsShowAbsField,
            ref CZLinearSystem Ls, ref CZPreconditioner_ILU Prec,
            ref IList<IList<Complex[]>> ScatterVecList,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            bool success = false;
            // ポート数
            int portCnt = WgPortInfoList.Count;
            // モード電力の合計を出力として表示する
            //bool isShowTotalPortPower = true;
            bool isShowTotalPortPower = false;
            // PC導波路？
            bool isPCWaveguide = false;
            foreach (WgUtilForPeriodicEigenExt.WgPortInfo workWgPortInfo in WgPortInfoList)
            {
                if (workWgPortInfo.IsPCWaveguide)
                {
                    isPCWaveguide = true;
                    break;
                }
            }
            //// モード追跡する？
            //bool isModeTrace = true;
            //if (!isPCWaveguide)
            //{
            //    isModeTrace = false;
            //}

            // モード追跡用の固有ベクトル格納先の初期化
            if (freqIndex == 0)
            {
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    WgPortInfoList[portIndex].PrevModalVecList = null;
                }
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
                // 格子定数
                double latticeA = 0.0;
                double periodicDistance = 0.0;
                if (portCnt > 0)
                {
                    latticeA = WgPortInfoList[0].LatticeA;
                    periodicDistance = WgPortInfoList[0].PeriodicDistance;
                }
                // 波数
                double k0 = 0;
                if (isPCWaveguide)
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
                foreach (WgUtilForPeriodicEigenExt.WgPortInfo workWgPortInfo in WgPortInfoList)
                {
                    WgUtil.MakeWaveguidePortBCPattern(Ls, World, workWgPortInfo.FieldPortBcId);
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
                // 剛性行列、残差ベクトルのマージ
                //------------------------------------------------------------------
                Ls.InitializeMarge();

                IList<double[,]> ryy_1d_port_list = new List<double[,]>();
                IList<Complex[]> eigen_values_port_list = new List<Complex[]>();
                IList<Complex[,]> eigen_vecs_port_list = new List<Complex[,]>();
                IList<Complex[,]> eigen_dFdXs_port_list = new List<Complex[,]>();

                // 境界の界に導波路開口条件を追加
                bool retAddPort = false;

                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    WgUtilForPeriodicEigenExt.WgPortInfo workWgPortInfo = WgPortInfoList[portIndex];

                    if (!IsInoutWgSame || (IsInoutWgSame && portIndex == 0))
                    {
                        // 導波路開口1
                        double[,] ryy_1d_port1 = null;
                        Complex[] eigen_values_port1 = null;
                        Complex[,] eigen_vecs_port1 = null;
                        Complex[,] eigen_dFdXs_port1 = null;
                        bool isModeTrace = workWgPortInfo.IsModeTrace;
                        if (!workWgPortInfo.IsPCWaveguide)
                        {
                            isModeTrace = false;
                        }
                        retAddPort = WgUtilForPeriodicEigenExt.GetPortPeriodicWaveguideFemMatAndEigenVec(
                            Ls,
                            waveLength,
                            WaveModeDv,
                            World,
                            workWgPortInfo.FieldInputWgLoopId,
                            workWgPortInfo.FieldPortBcId,
                            workWgPortInfo.FieldInputWgBcId,
                            fixedBcNodes,
                            workWgPortInfo.IsPCWaveguide,
                            workWgPortInfo.LatticeA,
                            workWgPortInfo.PeriodicDistance,
                            workWgPortInfo.PCWaveguidePorts,
                            workWgPortInfo.IncidentModeIndex,
                            workWgPortInfo.IsSolveEigenItr,
                            workWgPortInfo.PropModeCntToSolve,
                            workWgPortInfo.IsSVEA,
                            isModeTrace,
                            ref workWgPortInfo.PrevModalVecList,
                            workWgPortInfo.MinEffN,
                            workWgPortInfo.MaxEffN,
                            workWgPortInfo.MinWaveNum,
                            workWgPortInfo.MaxWaveNum,
                            Medias,
                            workWgPortInfo.InputWgLoopDic,
                            workWgPortInfo.InputWgEdgeDic,
                            (portIndex == 0) ? true : false, // isPortBc2Reverse : 周期構造導波路の外部境界と内部境界の辺の方向が逆かどうかを指定する。
                                                             //                    図面の作成手順より、ポート１の場合true ポート２の場合false
                            out ryy_1d_port1,
                            out eigen_values_port1,
                            out eigen_vecs_port1,
                            out eigen_dFdXs_port1);
                        /*
                        // DEBUG モード確認
                        {
                            //暫定描画処理
                            DrawerAry.Clear();
                            DrawerAry.PushBack(new CDrawerFace(FieldValId, true, World, FieldValId));
                            DrawerAry.PushBack(new CDrawerEdge(FieldValId, true, World));
                            // カメラの変換行列初期化
                            DrawerAry.InitTrans(Camera);
                            return true;
                        }
                         */
                        // 格納する
                        ryy_1d_port_list.Add(ryy_1d_port1);
                        eigen_values_port_list.Add(eigen_values_port1);
                        eigen_vecs_port_list.Add(eigen_vecs_port1);
                        eigen_dFdXs_port_list.Add(eigen_dFdXs_port1);

                        // 入射モードチェック
                        if (retAddPort)
                        {
                            if ((workWgPortInfo.IncidentModeIndex >= eigen_values_port_list[portIndex].Length)
                                || (Math.Abs(eigen_values_port_list[portIndex][workWgPortInfo.IncidentModeIndex].Real) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit) // 入射モードが伝搬モードでない
                                )
                            {
                                retAddPort = false;
                            }
                        }
                    }
                    else if (IsInoutWgSame && portIndex != 0)
                    {
                        double[,] ryy_1d_port2 = null;
                        Complex[] eigen_values_port2 = null;
                        Complex[,] eigen_vecs_port2 = null;
                        Complex[,] eigen_dFdXs_port2 = null;

                        //  出力側は入力側と同じ導波路とする
                        double[,] ryy_1d_port1 = ryy_1d_port_list[0];
                        Complex[] eigen_values_port1 = eigen_values_port_list[0];
                        Complex[,] eigen_vecs_port1 = eigen_vecs_port_list[0];
                        Complex[,] eigen_dFdXs_port1 = eigen_dFdXs_port_list[0];
                        ryy_1d_port2 = new double[ryy_1d_port1.GetLength(0), ryy_1d_port1.GetLength(1)];
                        for (int i = 0; i < ryy_1d_port2.GetLength(0); i++)
                        {
                            // 位置を反転
                            for (int j = 0; j < ryy_1d_port2.GetLength(1); j++)
                            {
                                double value = ryy_1d_port1[ryy_1d_port2.GetLength(0) - 1 - i, ryy_1d_port2.GetLength(1) - 1 - j];
                                ryy_1d_port2[i, j] = value;
                            }
                        }
                        eigen_values_port2 = new Complex[eigen_values_port1.Length];
                        eigen_vecs_port2 = new Complex[eigen_vecs_port1.GetLength(0), eigen_vecs_port1.GetLength(1)];
                        eigen_dFdXs_port2 = new Complex[eigen_dFdXs_port1.GetLength(0), eigen_dFdXs_port1.GetLength(1)];
                        for (int i = 0; i < eigen_vecs_port2.GetLength(0); i++)
                        {
                            Complex betam = eigen_values_port1[i];
                            eigen_values_port2[i] = betam;
                            // 位置を反転
                            for (int j = 0; j < eigen_vecs_port2.GetLength(1); j++)
                            {
                                eigen_vecs_port2[i, j] = eigen_vecs_port1[i, eigen_vecs_port2.GetLength(1) - 1 - j];
                                eigen_dFdXs_port2[i, j] = eigen_dFdXs_port1[i, eigen_dFdXs_port2.GetLength(1) - 1 - j];
                            }
                        }

                        // 格納する
                        ryy_1d_port_list.Add(ryy_1d_port2);
                        eigen_values_port_list.Add(eigen_values_port2);
                        eigen_vecs_port_list.Add(eigen_vecs_port2);
                        eigen_dFdXs_port_list.Add(eigen_dFdXs_port2);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                        retAddPort = false;
                    }
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
                                WgUtilForPeriodicEigenExt.WgPortInfo tmpWgPortInfo = WgPortInfoList[p];
                                int propModeCnt = tmpWgPortInfo.PropModeCntToSolve;
                                if (isShowTotalPortPower)
                                {
                                    propModeCnt = 1;
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
                        {
                            DrawerAry.Clear();
                            Ls.UpdateValueOfField(FieldValId, World, FIELD_DERIVATION_TYPE.VALUE);
                            DrawerAry.PushBack(new CDrawerFace(FieldValId, true, World, FieldValId));
                            DrawerAry.PushBack(new CDrawerEdge(FieldValId, true, World));
                            // カメラの変換行列初期化
                            DrawerAry.InitTrans(Camera);
                            // 表示位置調整
                            setupPanAndScale(probNo, Camera);
                        }
                        return false;
                    }

                    // 入射振幅を指定する(全モード入射)
                    Complex[] work_amps = null;
                    if (probNo == 13)
                    {
                        if (workWgPortInfo.IsIncidentPort)
                        {
                            /*
                            work_amps = Problem13_2.GetIncidentAmplitude(
                                World,
                                workWgPortInfo.FieldPortBcId,
                                workWgPortInfo.PCWaveguidePorts,
                                eigen_values_port_list[portIndex],
                                eigen_vecs_port_list[portIndex]);
                             */
                            /*
                            work_amps = Problem13_3.GetIncidentAmplitude(
                                World,
                                workWgPortInfo.FieldPortBcId,
                                workWgPortInfo.PCWaveguidePorts,
                                eigen_values_port_list[portIndex],
                                eigen_vecs_port_list[portIndex]);
                             */
                        }
                    }
                    workWgPortInfo.Amps = work_amps;

                    retAddPort = WgUtilForPeriodicEigenExt.AddLinSys_PeriodicWaveguidePortBC(
                        Ls,
                        waveLength,
                        WaveModeDv,
                        workWgPortInfo.PeriodicDistance,
                        World,
                        workWgPortInfo.FieldInputWgLoopId,
                        workWgPortInfo.FieldPortBcId,
                        fixedBcNodes,
                        workWgPortInfo.IsIncidentPort, // isInputPort : true
                        workWgPortInfo.IncidentModeIndex,
                        workWgPortInfo.Amps,
                        false, // isFreeBc: false:モード展開  true:PMLを装荷する場合、Sommerfeld境界条件を適用する場合
                        ryy_1d_port_list[portIndex],
                        eigen_values_port_list[portIndex],
                        eigen_vecs_port_list[portIndex],
                        eigen_dFdXs_port_list[portIndex]);
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
                                WgUtilForPeriodicEigenExt.WgPortInfo tmpWgPortInfo = WgPortInfoList[p];
                                int propModeCnt = tmpWgPortInfo.PropModeCntToSolve;
                                if (isShowTotalPortPower)
                                {
                                    propModeCnt = 1;
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
                /*
                // TEST Sommerfelt
                CEqnHelmholz.AddLinSys_SommerfeltRadiationBC(Ls, waveLength, World, FieldPortBcId1);
                CEqnHelmholz.AddLinSys_SommerfeltRadiationBC(Ls, waveLength, World, FieldPortBcId2);
                 */
                /*
                //  散乱係数計算用に1のモード分布をセット
                ryy_1d_port2 = ryy_1d_port1;
                eigen_values_port2 = eigen_values_port1;
                eigen_vecs_port2 = eigen_vecs_port1;
                eigen_dFdXs_port2 = eigen_dFdXs_port1;
                */

                // 領域
                WgUtil.AddLinSysHelmholtz(Ls, waveLength, World, FieldLoopId
                    , Medias, LoopDic);

                double res = Ls.FinalizeMarge();
                //System.Diagnostics.Debug.WriteLine("Residual : " + res);

                //------------------------------------------------------------------
                // プリコンディショナ―
                //------------------------------------------------------------------
                // set Preconditioner
                //Prec.SetFillInLevel(1);
                //Prec.SetFillInLevel(int.MaxValue);  // 対称でなくても解けてしまう
                // ILU(0)のパターン初期化
                //Prec.SetLinearSystem(Ls);

                //------------------------------------------------------------------
                // リニアシステムを解く
                //------------------------------------------------------------------
                // メモリを解放する
                WgUtil.GC_Collect();
                /*
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
                 */

                //// 非対称複素行列の連立方程式をclapackで解く
                //solveAsymmetricCompplexMatEqn(Ls, World, FieldValId);

                // 非対称複素帯行列の連立方程式をclapackで解く
                solveAsymmetricComplexBandMatEqn(Ls, World, FieldValId);

                //------------------------------------------------------------------
                // 計算結果の後処理
                //------------------------------------------------------------------
                // 反射、透過係数
                double totalPower = 0.0;
                int incidentPortNo = getIncidentPortNo(WgPortInfoList);
                IList<Complex[]> portScatterVecList = new List<Complex[]>();
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    Complex[] eigen_values_port1 = eigen_values_port_list[portIndex];
                    double[,] ryy_1d_port1 = ryy_1d_port_list[portIndex];
                    Complex[,] eigen_vecs_port1 = eigen_vecs_port_list[portIndex];
                    Complex[,] eigen_dFdXs_port1 = eigen_dFdXs_port_list[portIndex];
                    WgUtilForPeriodicEigenExt.WgPortInfo wgPortInfo1 = WgPortInfoList[portIndex];
                    int propModeCntToShow = wgPortInfo1.PropModeCntToSolve;
                    if (isShowTotalPortPower)
                    {
                        propModeCntToShow = 1;
                    }
                    Complex[] scatterVec = new Complex[propModeCntToShow];
                    for (int m = 0; m < propModeCntToShow; m++)
                    {
                        scatterVec[m] = 0.0;
                    }

                    for (uint imode = 0; imode < eigen_values_port1.Length; imode++)
                    {
                        if (Math.Abs(eigen_values_port1[imode].Real) >= 1.0e-12 && Math.Abs(eigen_values_port1[imode].Imag) < 1.0e-12)
                        {
                            Complex s11 = 0.0;
                            bool isIncidentMode = (wgPortInfo1.IsIncidentPort && imode == wgPortInfo1.IncidentModeIndex);
                            if (wgPortInfo1.Amps != null)
                            {
                                // 全モード入射の時
                                isIncidentMode = wgPortInfo1.IsIncidentPort;
                            }
                            s11 = WgUtilForPeriodicEigenExt.GetPeriodicWaveguidePortReflectionCoef(
                                Ls,
                                waveLength,
                                WaveModeDv,
                                wgPortInfo1.PeriodicDistance,
                                World,
                                wgPortInfo1.FieldPortBcId,
                                imode,
                                isIncidentMode,
                                wgPortInfo1.Amps,
                                ryy_1d_port1,
                                eigen_values_port1,
                                eigen_vecs_port1,
                                eigen_dFdXs_port1);
                            System.Diagnostics.Debug.WriteLine("    s" + (portIndex + 1) + incidentPortNo + " = (" + s11.Real + "," + s11.Imag + ")" + " |s" + (portIndex + 1) + incidentPortNo + "|^2 = " + Complex.SquaredNorm(s11) + ((imode == wgPortInfo1.IncidentModeIndex) ? "  incident" : ""));
                            if (Math.Abs(eigen_values_port1[imode].Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit
                                && Math.Abs(eigen_values_port1[imode].Imag) < MyUtilLib.Matrix.Constants.PrecisionLowerLimit) // 伝搬モード
                            {
                                totalPower += Complex.SquaredNorm(s11);
                            }
                            if (isShowTotalPortPower)
                            {
                                // ポート電力を計算
                                scatterVec[0] += Complex.Norm(s11) * Complex.Norm(s11);
                            }
                            else
                            {
                                if (imode < propModeCntToShow)
                                {
                                    scatterVec[imode] = s11;
                                }
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
                        WgUtilForPeriodicEigenExt.WgPortInfo tmpWgPortInfo = WgPortInfoList[p];
                        int propModeCnt = tmpWgPortInfo.PropModeCntToSolve;
                        if (isShowTotalPortPower)
                        {
                            propModeCnt = 1;
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
                {
                    DrawerAry.Clear();
                    Ls.UpdateValueOfField(FieldValId, World, FIELD_DERIVATION_TYPE.VALUE);
                    DrawerAry.PushBack(new CDrawerFace(FieldValId, true, World, FieldValId));
                    DrawerAry.PushBack(new CDrawerEdge(FieldValId, true, World));
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
            if (probNo == 0)
            {
                Camera.MousePan(0, 0, -0.15, 0.3);
                double tmp_scale = 1.2;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 1)
            {
                Camera.MousePan(0, 0, -0.05, 0.5);
                double tmp_scale = 0.7;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 4 || probNo == 5)
            {
                Camera.MousePan(0, 0, -0.15, 0.27);
                double tmp_scale = 1.18;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 6 || probNo == 7)
            {
                Camera.MousePan(0, 0, -0.15, 0.27);
                double tmp_scale = 1.18;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 8 || probNo == 9 || probNo == 11 || probNo == 12 || probNo == 15)
            {
                Camera.MousePan(0, 0, 0.06, 0.53);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 14)
            {
                //Camera.MousePan(0, 0, 0.00, 0.48);
                //double tmp_scale = 0.9;
                //Camera.SetScale(tmp_scale);
                
                Camera.MousePan(0, 0, -0.15, 0.27);
                double tmp_scale = 1.10;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 10)
            {
                Camera.MousePan(0, 0, 0.14, 0.55);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 13)
            {
                Camera.MousePan(0, 0, 0.06, 0.53);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
            else
            {
                Camera.MousePan(0, 0, -0.05, 0.5);
                double tmp_scale = 0.8;
                Camera.SetScale(tmp_scale);
            }
        }

        /// <summary>
        /// 非対称複素行列の連立方程式をLisys(clapack)で解く
        /// </summary>
        /// <param name="Ls"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        private static void solveAsymmetricCompplexMatEqn(CZLinearSystem Ls, CFieldWorld World, uint FieldValId)
        {
            // 非対称複素行列の連立方程式をLisys(clapack)で解く
            CZMatDia_BlkCrs_Ptr femMat = Ls.GetMatrixPtr(FieldValId, ELSEG_TYPE.CORNER, World);
            uint nblk = femMat.NBlkMatCol();
            KrdLab.clapack.Complex[] A = new KrdLab.clapack.Complex[nblk * nblk];

            System.Diagnostics.Debug.Assert(nblk == femMat.NBlkMatRow());
            for (uint iblk = 0; iblk < nblk; iblk++)
            {
                ComplexArrayIndexer ptr = femMat.GetPtrValDia(iblk);
                System.Diagnostics.Debug.Assert(ptr.Count == 1);
                A[iblk + nblk * iblk].Real = ptr[0].Real;
                A[iblk + nblk * iblk].Imaginary = ptr[0].Imag;
                //Console.WriteLine("    ( " + iblk + " ) = " + ptr[0].Real + " + " + ptr[0].Imag + " i");

                uint npsup = 0;
                ConstUIntArrayIndexer ptrInd = femMat.GetPtrIndPSuP(iblk, out npsup);
                ComplexArrayIndexer ptrVal = femMat.GetPtrValPSuP(iblk, out npsup);
                System.Diagnostics.Debug.Assert(ptrInd.Count == ptrVal.Count);
                for (int i = 0; i < ptrVal.Count; i++)
                {
                    A[iblk + nblk * ptrInd[i]].Real = ptrVal[i].Real;
                    A[iblk + nblk * ptrInd[i]].Imaginary = ptrVal[i].Imag;
                    //Console.WriteLine("    ( " + iblk + ", " + ptrInd[i] + " ) = " + ptrVal[i].Real + " + " + ptrVal[i].Imag + " i");
                }
            }
            // 残差ベクトル
            CZVector_Blk_Ptr resPtr = Ls.GetResidualPtr(FieldValId, ELSEG_TYPE.CORNER, World);
            System.Diagnostics.Debug.Assert(nblk == resPtr.BlkVecLen());
            System.Diagnostics.Debug.Assert(1 == resPtr.BlkLen());
            KrdLab.clapack.Complex[] B = new KrdLab.clapack.Complex[nblk];
            for (uint iblk = 0; iblk < resPtr.BlkVecLen(); iblk++)
            {
                Complex cvalue = resPtr.GetValue(iblk, 0);
                B[iblk].Real = cvalue.Real;
                B[iblk].Imaginary = cvalue.Imag;
                //System.Console.WriteLine("res( " + iblk + " ) = " + resPtr.GetValue(iblk, 0));
            }
            // 対称行列チェック
            bool isSymmetrix = true;
            for (int i = 0; i < nblk; i++)
            {
                for (int j = i; j < nblk; j++)
                {
                    KrdLab.clapack.Complex aij = A[i + nblk * j];
                    KrdLab.clapack.Complex aji = A[j + nblk * i];
                    if (Math.Abs(aij.Real - aji.Real) >= 1.0e-12)
                    {
                        isSymmetrix = false;
                        break;
                        //System.Diagnostics.Debug.Assert(false);
                    }
                    if (Math.Abs(aij.Imaginary - aji.Imaginary) >= 1.0e-12)
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
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!matrix A is NOT symmetric!!!!!!!!!!!!!!");
                //System.Diagnostics.Debug.Assert(false);
            }

            // 非対称行列の線形方程式を解く
            // 解ベクトル
            KrdLab.clapack.Complex[] X = null;
            // 連立方程式AX = Bを解く
            int x_row = (int)nblk;
            int x_col = 1;
            int a_row = (int)nblk;
            int a_col = (int)nblk;
            int b_row = (int)nblk;
            int b_col = 1;
            System.Diagnostics.Debug.WriteLine("solve : KrdLab.clapack.FunctionExt.zgesv");
            KrdLab.clapack.FunctionExt.zgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col);

            // 解ベクトルをワールド座標系にセット
            Complex[] valuesAll = new Complex[nblk];
            Dictionary<uint, uint> toNodeIndex = new Dictionary<uint, uint>();
            for (uint i = 0; i < nblk; i++)
            {
                valuesAll[i] = new Complex(X[i].Real, X[i].Imaginary);
                toNodeIndex.Add(i, i);
            }
            WgUtil.SetFieldValueForDisplay(World, FieldValId, valuesAll, toNodeIndex);
        }

        /// <summary>
        /// 非対称複素帯行列の連立方程式をclapackで解く
        /// </summary>
        /// <param name="Ls"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        private static void solveAsymmetricComplexBandMatEqn(CZLinearSystem Ls, CFieldWorld World, uint FieldValId)
        {
            // エルミート帯行列の連立方程式をLisys(clapack)で解く
            CZMatDia_BlkCrs_Ptr femMat = Ls.GetMatrixPtr(FieldValId, ELSEG_TYPE.CORNER, World);
            uint nblk = femMat.NBlkMatCol();
            Complex[] A = new Complex[nblk * nblk]; // ポインタを格納

            System.Diagnostics.Debug.Assert(nblk == femMat.NBlkMatRow());
            for (uint iblk = 0; iblk < nblk; iblk++)
            {
                ComplexArrayIndexer ptr = femMat.GetPtrValDia(iblk);
                System.Diagnostics.Debug.Assert(ptr.Count == 1);
                A[iblk + nblk * iblk] = new Complex(ptr[0].Real, ptr[0].Imag);
                //Console.WriteLine("    ( " + iblk + " ) = " + ptr[0].Real + " + " + ptr[0].Imag + " i");

                uint npsup = 0;
                ConstUIntArrayIndexer ptrInd = femMat.GetPtrIndPSuP(iblk, out npsup);
                ComplexArrayIndexer ptrVal = femMat.GetPtrValPSuP(iblk, out npsup);
                System.Diagnostics.Debug.Assert(ptrInd.Count == ptrVal.Count);
                for (int i = 0; i < ptrVal.Count; i++)
                {
                    A[iblk + nblk * ptrInd[i]] = new Complex(ptrVal[i].Real, ptrVal[i].Imag);
                    //Console.WriteLine("    ( " + iblk + ", " + ptrInd[i] + " ) = " + ptrVal[i].Real + " + " + ptrVal[i].Imag + " i");
                }
            }
            // 残差ベクトル
            CZVector_Blk_Ptr resPtr = Ls.GetResidualPtr(FieldValId, ELSEG_TYPE.CORNER, World);
            System.Diagnostics.Debug.Assert(nblk == resPtr.BlkVecLen());
            System.Diagnostics.Debug.Assert(1 == resPtr.BlkLen());
            KrdLab.clapack.Complex[] B = new KrdLab.clapack.Complex[nblk];
            for (uint iblk = 0; iblk < resPtr.BlkVecLen(); iblk++)
            {
                Complex cvalue = resPtr.GetValue(iblk, 0);
                B[iblk].Real = cvalue.Real;
                B[iblk].Imaginary = cvalue.Imag;
                //System.Console.WriteLine("res( " + iblk + " ) = " + resPtr.GetValue(iblk, 0));
            }
            // 対称行列チェック
            bool isSymmetrix = true;
            for (int i = 0; i < nblk; i++)
            {
                for (int j = i; j < nblk; j++)
                {
                    Complex aij = A[i + nblk * j];
                    Complex aji = A[j + nblk * i];
                    if (null == aij && null != aji)
                    {
                        isSymmetrix = false;
                        break;
                    }
                    else if (null != aij && null == aji)
                    {
                        isSymmetrix = false;
                        break;
                    }
                    else if (null != aij && null != aji)
                    {
                        if (Math.Abs(aij.Real - aji.Real) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        {
                            isSymmetrix = false;
                            break;
                        }
                        else if (Math.Abs(aij.Imag - aji.Imag) >= MyUtilLib.Matrix.Constants.PrecisionLowerLimit)
                        {
                            isSymmetrix = false;
                            break;
                        }
                    }
                }
                if (!isSymmetrix)
                {
                    break;
                }
            }
            if (!isSymmetrix)
            {
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!matrix A is NOT symmetric!!!!!!!!!!!!!!");
                //System.Diagnostics.Debug.Assert(false);
            }

            // 複素バンド行列の線形方程式を解く
            KrdLab.clapack.Complex[] X = null;
            {
                // 非０パターンを作成
                bool[,] patternA = new bool[nblk, nblk];
                for (int i = 0; i < nblk; i++)
                {
                    for (int j = 0; j < nblk; j++)
                    {
                        Complex aij = A[i + nblk * j];
                        patternA[i, j] = (aij != null);
                    }
                }
                // バンド行列のバンド幅を縮小する
                bool[,] optPatternA = new bool[nblk, nblk];
                IList<int> optNodesA = null;
                Dictionary<int, int> toOptNodesA = null;
                // [A]のバンド幅を縮小する
                {
                    WgUtilForPeriodicEigenExt.GetOptBandMatNodes(patternA, out optNodesA, out toOptNodesA);

                    for (int i = 0; i < nblk; i++)
                    {
                        int ino_optA = toOptNodesA[i];
                        for (int j = 0; j < nblk; j++)
                        {
                            int jno_optA = toOptNodesA[j];
                            optPatternA[ino_optA, jno_optA] = patternA[i, j];
                        }
                    }
                }
                patternA = null;

                // バンド行列のサイズ取得
                int a_rowcolSize;
                int a_subdiaSize;
                int a_superdiaSize;
                WgUtilForPeriodicEigenExt.GetBandMatrixSubDiaSizeAndSuperDiaSize(optPatternA, out a_rowcolSize, out a_subdiaSize, out a_superdiaSize);
                // 扱っている問題ではsubdiagonalとsuperdiagonalは同じサイズ
                System.Diagnostics.Debug.Assert(a_subdiaSize == a_superdiaSize);
                // バンド行列作成
                int _a_rsize = a_subdiaSize * 2 + a_superdiaSize + 1;
                int _a_csize = a_rowcolSize;
                KrdLab.clapack.Complex[] AB = new KrdLab.clapack.Complex[_a_rsize * _a_csize];
                // [A]の値を[AB]にコピーする
                //   [A]はバンド幅最適化前の並び、[AB]は最適化後の並び
                for (int c = 0; c < a_rowcolSize; c++)
                {
                    int c_org = optNodesA[c];
                    // 対角成分
                    Complex a_c_org_c_org = A[c_org + c_org * a_rowcolSize];
                    if (a_c_org_c_org != null)
                    {
                        AB[a_subdiaSize + a_superdiaSize + c * _a_rsize].Real = a_c_org_c_org.Real;
                        AB[a_subdiaSize + a_superdiaSize + c * _a_rsize].Imaginary = a_c_org_c_org.Imag;
                    }

                    // subdiagonal成分
                    if (c < a_rowcolSize - 1)
                    {
                        for (int r = c + 1; r <= c + a_subdiaSize && r < a_rowcolSize; r++)
                        {
                            int r_org = optNodesA[r];
                            System.Diagnostics.Debug.Assert(r >= 0 && r < a_rowcolSize && c >= 0 && c < a_rowcolSize);
                            System.Diagnostics.Debug.Assert((r >= c - a_superdiaSize && r <= c + a_subdiaSize));
                            Complex a_r_c = A[r_org + c_org * a_rowcolSize];
                            if (a_r_c != null)
                            {
                                AB[(r - c) + a_subdiaSize + a_superdiaSize + c * _a_rsize].Real = a_r_c.Real;
                                AB[(r - c) + a_subdiaSize + a_superdiaSize + c * _a_rsize].Imaginary = a_r_c.Imag;
                            }
                        }
                    }
                    if (c > 0)
                    {
                        for (int r = c - 1; r >= c - a_superdiaSize && r >= 0; r--)
                        {
                            int r_org = optNodesA[r];
                            System.Diagnostics.Debug.Assert(r >= 0 && r < a_rowcolSize && c >= 0 && c < a_rowcolSize);
                            System.Diagnostics.Debug.Assert((r >= c - a_superdiaSize && r <= c + a_subdiaSize));
                            Complex a_r_c = A[r_org + c_org * a_rowcolSize];
                            if (a_r_c != null)
                            {
                                AB[(r - c) + a_subdiaSize + a_superdiaSize + c * _a_rsize].Real = a_r_c.Real;
                                AB[(r - c) + a_subdiaSize + a_superdiaSize + c * _a_rsize].Imaginary = a_r_c.Imag;
                            }
                        }
                    }
                }
                // 残差ベクトル{B}をoptAにあわせて並び替える
                KrdLab.clapack.Complex[] optB = new KrdLab.clapack.Complex[nblk];
                for (int i = 0; i < nblk; i++)
                {
                    int ino_optA = toOptNodesA[i];
                    optB[ino_optA].Real = B[i].Real;
                    optB[ino_optA].Imaginary = B[i].Imaginary;
                }
                A = null;
                B = null;

                // 解ベクトル
                KrdLab.clapack.Complex[] optX = null;
                // 連立方程式[AB]{X} = {B}を解く
                int x_row = (int)nblk;
                int x_col = 1;
                int a_row = a_rowcolSize;
                int a_col = a_rowcolSize;
                int kl = a_subdiaSize;
                int ku = a_superdiaSize;
                int b_row = (int)nblk;
                int b_col = 1;
                System.Diagnostics.Debug.WriteLine("solve : KrdLab.clapack.FunctionExt.zgbsv");
                KrdLab.clapack.FunctionExt.zgbsv(ref optX, ref x_row, ref x_col, AB, a_row, a_col, kl, ku, optB, b_row, b_col);
                // 解ベクトルを元の並びに戻す
                X = new KrdLab.clapack.Complex[nblk];
                for (int i = 0; i < nblk; i++)
                {
                    int inoGlobal = optNodesA[i];
                    X[inoGlobal].Real = optX[i].Real;
                    X[inoGlobal].Imaginary = optX[i].Imaginary;
                }
                optX = null;
            }

            // 解ベクトルをワールド座標系にセット
            Complex[] valuesAll = new Complex[nblk];
            Dictionary<uint, uint> toNodeIndex = new Dictionary<uint, uint>();
            for (uint i = 0; i < nblk; i++)
            {
                valuesAll[i] = new Complex(X[i].Real, X[i].Imaginary);
                toNodeIndex.Add(i, i);
            }
            WgUtil.SetFieldValueForDisplay(World, FieldValId, valuesAll, toNodeIndex);
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
        /// <param name="WgPortInfoList"></param>
        /// <returns></returns>
        private static int getIncidentPortNo(IList<WgUtilForPeriodicEigenExt.WgPortInfo> WgPortInfoList)
        {
            int incidentPortNo = -1;
            for (int portIndex = 0; portIndex < WgPortInfoList.Count; portIndex++)
            {
                if (WgPortInfoList[portIndex].IsIncidentPort)
                {
                    incidentPortNo = portIndex + 1;
                    break;
                }
            }
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
                int incidentPortNo = getIncidentPortNo(WgPortInfoList);

                bool isPCWaveguide = false;
                foreach (WgUtilForPeriodicEigenExt.WgPortInfo workWgPortInfo in WgPortInfoList)
                {
                    if (workWgPortInfo.IsPCWaveguide)
                    {
                        isPCWaveguide = true;
                        break;
                    }
                }
                // 周波数を表示
                drawString(10, 45, 0.0, 0.7, 1.0, string.Format((isPCWaveguide ? "a/lamda:" : "2W/lamda:") + "{0}", normalizedFreq));
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
            double[,] graphLineColors = new double[8, 3]
            {
                {0.0, 0.0, 1.0},
                {1.0, 0.0, 1.0},
                {0.0, 1.0, 0.0},
                {1.0, 0.5, 0.0},
                {0.0, 0.5, 0.0},
                {0.7, 0.5, 0.0},
                {0.0, 0.7, 1.0},
                {1.0, 0.0, 0.0},
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
