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

namespace photonic_band
{
    ///////////////////////////////////////////////////////////////
    //  DelFEM4Net サンプル
    //  フォトニックバンドの計算 (2D)
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
            double periodicDistanceY,
            ref bool isTriLattice,
            ref int calcBetaCnt,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref double latticeA,
            ref double periodicDistanceX,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPortBcIds,
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
        private const int ProbCnt = 5;
        /// <summary>
        /// 最初に表示する問題番号
        /// </summary>
        private const int DefProbNo = 4;//0;
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
        /// 最大モード数
        /// </summary>
        private const uint MaxModeCnt = 20;
        /// <summary>
        /// 伝搬定数計算点数(既定値)
        /// </summary>
        private const int DefCalcBetaCnt = 20;

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
        /// 計算する伝搬定数の点数
        /// </summary>
        private int CalcBetaCnt = DefCalcBetaCnt;
        /// <summary>
        /// グラフの周波数目盛間隔
        /// </summary>
        private double GraphFreqInterval = 0.2;
        /// <summary>
        /// 規格化定数の最小値
        /// </summary>
        private double MinNormalizedFreq = 0.0;
        /// <summary>
        /// 規格化定数の最大値
        /// </summary>
        private double MaxNormalizedFreq = 0.0;
        /// <summary>
        /// 波のモード区分
        /// </summary>
        private WgUtil.WaveModeDV WaveModeDv = WgUtil.WaveModeDV.TE;

        /// <summary>
        /// 三角形格子？
        /// </summary>
        private bool IsTriLattice = false;
        /// <summary>
        // Y方向周期距離
        /// </summary>
        private double PeriodicDistanceY = 1.0;
        /// <summary>
        // 格子定数
        /// </summary>
        private double LatticeA = 0.0;
        /// <summary>
        // X方向周期距離
        /// </summary>
        private double PeriodicDistanceX = 0.0;
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
        // ポートの境界フィールドIDリスト
        /// </summary>
        private IList<uint> FieldPortBcIds = new List<uint>();
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
        private IList<Complex[]> EigenValueList = null;

        /// <summary>
        /// 図面を表示する？
        /// </summary>
        private bool IsCadShow = false;
        /// <summary>
        /// 図面表示用描画オブジェクトアレイ
        /// </summary>
        private CDrawerArray CadDrawerAry = null;
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
            EigenValueList = new List<Complex[]>();
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
                        IsTriLattice,
                        CalcBetaCnt,
                        (BetaIndex == 0), // false,
                        PeriodicDistanceY,
                        WaveModeDv,
                        LatticeA,
                        PeriodicDistanceX,
                        IsShowAbsField,
                        MinNormalizedFreq,
                        MaxNormalizedFreq,
                        ref World,
                        FieldValId,
                        FieldLoopId,
                        FieldForceBcId,
                        FieldPortBcIds,
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
                PeriodicDistanceY,
                ref IsTriLattice,
                ref CalcBetaCnt,
                ref GraphFreqInterval,
                ref MinNormalizedFreq,
                ref MaxNormalizedFreq,
                ref WaveModeDv,
                ref LatticeA,
                ref PeriodicDistanceX,
                ref World,
                ref FieldValId,
                ref FieldLoopId,
                ref FieldForceBcId,
                ref FieldPortBcIds,
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
        /// <param name="periodicDistanceY"></param>
        /// <param name="isTriLattice"></param>
        /// <param name="calcBetaCnt"></param>
        /// <param name="GraphFreqInterval"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistanceX"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPortBcIds"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="isCadShow"></param>
        /// <param name="CadDrawerAry"></param>
        /// <param name="Camera"></param>
        /// <returns></returns>
        private static bool setProblem(
            int probNo,
            double periodicDistanceY,
            ref bool isTriLattice,
            ref int calcBetaCnt,
            ref double GraphFreqInterval,
            ref double MinNormalizedFreq,
            ref double MaxNormalizedFreq,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref double latticeA,
            ref double periodicDistanceX,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPortBcIds,
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
            // 境界フィールドIDのクリア
            FieldPortBcIds.Clear();

            // フォトニック結晶導波路解析用
            latticeA = 0.0;
            periodicDistanceX = 0.0;
            calcBetaCnt = DefCalcBetaCnt;

            SetProblemProcDelegate func = null;
            //isCadShow = false;
            try
            {
                if (probNo == 0)
                {
                    // 正方格子
                    func = Problem00.SetProblem;
                }
                else if (probNo == 1)
                {
                    // 三角形格子(斜め領域)
                    //func = Problem01.SetProblem;  // 中央に１つのロッド
                    //func = Problem01_2.SetProblem; // 上下に半円ロッド
                    func = Problem01_3.SetProblem; // コーナーに1/4円ロッド
                }
                else if (probNo == 2)
                {
                    // 三角形格子
                    //func = Problem02.SetProblem; // 境界上に半円ロッド x 4
                    func = Problem02_2.SetProblem; // コーナーに1/4ロッド + 中央にロッド
                }
                else if (probNo == 3)
                {
                    // 三角形格子
                    func = Problem03.SetProblem; // 上境界コーナーに1/4ロッド + 下境界中央にロッド
                }
                else if (probNo == 4)
                {
                    // 三角形格子
                    func = Problem04.SetProblem; // 六角形領域
                }
                else
                {
                    return success;
                }

                success = func(
                    probNo,
                    periodicDistanceY,
                    ref isTriLattice,
                    ref calcBetaCnt,
                    ref GraphFreqInterval,
                    ref MinNormalizedFreq,
                    ref MaxNormalizedFreq,
                    ref WaveModeDv,
                    ref latticeA,
                    ref periodicDistanceX,
                    ref World,
                    ref FieldValId,
                    ref FieldLoopId,
                    ref FieldForceBcId,
                    ref FieldPortBcIds,
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
                IsTriLattice,
                CalcBetaCnt,
                (BetaIndex == 0), // false,
                PeriodicDistanceY,
                WaveModeDv,
                LatticeA,
                PeriodicDistanceX,
                IsShowAbsField,
                MinNormalizedFreq,
                MaxNormalizedFreq,
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPortBcIds,
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
        /// <param name="isTriLattice"></param>
        /// <param name="calcBetaCnt"></param>
        /// <param name="initFlg"></param>
        /// <param name="periodicDistanceY"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="latticeA"></param>
        /// <param name="periodicDistanceX"></param>
        /// <param name="IsShowAbsField"></param>
        /// <param name="MinNormalizedFreq"></param>
        /// <param name="MaxNormalizedFreq"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPortBcIds"></param>
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
            bool isTriLattice,
            int calcBetaCnt,
            bool initFlg,
            double periodicDistanceY,
            WgUtil.WaveModeDV WaveModeDv,
            double latticeA,
            double periodicDistanceX,
            bool IsShowAbsField,
            double MinNormalizedFreq,
            double MaxNormalizedFreq,
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            IList<uint> FieldPortBcIds,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref IList<Complex[]> EigenValueList,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            bool success = false;
            bool showException = true;
            try
            {
                // 規格化伝搬定数
                double beta = 0.0;
                double betaX = 0.0;
                double betaY = 0.0;
                getBeta(
                    ref betaIndex,
                    isTriLattice,
                    calcBetaCnt,
                    latticeA,
                    periodicDistanceX,
                    periodicDistanceY,
                    ref betaX,
                    ref betaY);
                if (betaIndex == -1)
                {
                    return success;
                }
                System.Diagnostics.Debug.WriteLine("beta: {0}    beta*d/(2.0 * pi): {1}",
                    beta,
                    beta * latticeA / (2.0 * pi));

                // 全節点数を取得する
                uint node_cnt = 0;
                //node_cnt = WgUtilForPeriodicEigenBetaSpecified.GetNodeCnt(World, FieldLoopId);
                double[][] coord_c_all = null;
                {
                    uint[] no_c_all_tmp = null;
                    Dictionary<uint, uint> to_no_all_tmp = null;
                    double[][] coord_c_all_tmp = null;
                    WgUtilForPeriodicEigenBetaSpecified.GetLoopCoordList(World, FieldLoopId, out no_c_all_tmp, out to_no_all_tmp, out coord_c_all_tmp);
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
                int boundaryCnt = FieldPortBcIds.Count;
                IList<uint[]> no_c_all_fieldPortBcId_list = new List<uint[]>();
                IList<Dictionary<uint, uint>> to_no_boundary_fieldPortBcId_list = new List<Dictionary<uint, uint>>();
                for (int i = 0; i < boundaryCnt; i++)
                {
                    uint[] work_no_c_all_fieldPortBcId = null;
                    Dictionary<uint, uint> work_to_no_boundary_fieldPortBcId = null;
                    uint work_fieldPortBcId = FieldPortBcIds[i];
                    WgUtil.GetBoundaryNodeList(World, work_fieldPortBcId, out work_no_c_all_fieldPortBcId, out work_to_no_boundary_fieldPortBcId);
                    no_c_all_fieldPortBcId_list.Add(work_no_c_all_fieldPortBcId);
                    to_no_boundary_fieldPortBcId_list.Add(work_to_no_boundary_fieldPortBcId);
                }

                uint[] no_c_all_fieldPortBcId1 = no_c_all_fieldPortBcId_list[0];
                uint[] no_c_all_fieldPortBcId2 = no_c_all_fieldPortBcId_list[1];
                uint[] no_c_all_fieldPortBcId3 = no_c_all_fieldPortBcId_list[2];
                uint[] no_c_all_fieldPortBcId4 = no_c_all_fieldPortBcId_list[3];
                uint[] no_c_all_fieldPortBcId5 = null;
                uint[] no_c_all_fieldPortBcId6 = null;
                if (boundaryCnt == 6)
                {
                    no_c_all_fieldPortBcId5 = no_c_all_fieldPortBcId_list[4];
                    no_c_all_fieldPortBcId6 = no_c_all_fieldPortBcId_list[5];
                }
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId1 = to_no_boundary_fieldPortBcId_list[0];
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId2 = to_no_boundary_fieldPortBcId_list[1];
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId3 = to_no_boundary_fieldPortBcId_list[2];
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId4 = to_no_boundary_fieldPortBcId_list[3];
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId5 = null;
                Dictionary<uint, uint> to_no_boundary_fieldPortBcId6 = null;
                if (boundaryCnt == 6)
                {
                    to_no_boundary_fieldPortBcId5 = to_no_boundary_fieldPortBcId_list[4];
                    to_no_boundary_fieldPortBcId6 = to_no_boundary_fieldPortBcId_list[5];
                }
                /////////////////////////////////////////////////////////////////
                // コーナーの節点
                uint[] sharedNodes = new uint[boundaryCnt];
                if (boundaryCnt == 6)
                {
                    // 六角形領域の場合
                    // 境界の始点
                    for (int boundaryIndex = 0; boundaryIndex < boundaryCnt; boundaryIndex++)
                    {
                        uint[] work_no_c_all_fieldPortBcId = no_c_all_fieldPortBcId_list[boundaryIndex];
                        Dictionary<uint, uint> work_to_no_boundary_fieldPortBcId_prev = to_no_boundary_fieldPortBcId_list[(boundaryIndex - 1 + boundaryCnt) % boundaryCnt];
                        for (int i = 0; i < work_no_c_all_fieldPortBcId.Length; i++)
                        {
                            uint work_nodeNumberPortBc = work_no_c_all_fieldPortBcId[i];
                            if (work_to_no_boundary_fieldPortBcId_prev.ContainsKey(work_nodeNumberPortBc))
                            {
                                // 前の境界との共有頂点
                                sharedNodes[boundaryIndex] = work_nodeNumberPortBc;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 四角形領域の場合
                    System.Diagnostics.Debug.Assert(boundaryCnt == 4);
                    // 境界1の両端
                    for (int i = 0; i < no_c_all_fieldPortBcId1.Length; i++)
                    {
                        uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[i];
                        if (to_no_boundary_fieldPortBcId4.ContainsKey(nodeNumberPortBc1))
                        {
                            // 境界1と境界4の共有の頂点
                            // 左上頂点
                            sharedNodes[0] = nodeNumberPortBc1;
                        }
                        // 境界1と境界3の共有の頂点
                        if (to_no_boundary_fieldPortBcId3.ContainsKey(nodeNumberPortBc1))
                        {
                            // 左下頂点
                            sharedNodes[1] = nodeNumberPortBc1;
                        }
                    }
                    // 境界2の両端
                    for (int i = 0; i < no_c_all_fieldPortBcId2.Length; i++)
                    {
                        // 境界2の節点を追加
                        uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[i];
                        // 境界2と境界4の共有の頂点
                        if (to_no_boundary_fieldPortBcId4.ContainsKey(nodeNumberPortBc2))
                        {
                            // 右上頂点
                            sharedNodes[2] = nodeNumberPortBc2;
                        }
                        // 境界2と境界3の共有の頂点
                        if (to_no_boundary_fieldPortBcId3.ContainsKey(nodeNumberPortBc2))
                        {
                            // 右下頂点
                            sharedNodes[3] = nodeNumberPortBc2;
                        }
                    }
                }
                // 共有境界の節点座標
                double[][] sharedNodeCoords = new double[sharedNodes.Length][];
                for (int i = 0; i < sharedNodes.Length; i++)
                {
                    sharedNodeCoords[i] = coord_c_all[sharedNodes[i]];
                }

                /////////////////////////////////////////////////////////////////////////////////
                // 節点のソート
                IList<uint> sortedNodes = new List<uint>();
                Dictionary<uint, int> toSorted = new Dictionary<uint, int>();

                // 四角形領域の場合
                uint boundary_node_cnt_B1 = 0; // 境界1
                uint boundary_node_cnt_B3 = 0; // 境界3
                uint boundary_node_cnt = 0; // 境界1 + 3
                // 六角形領域の場合
                uint boundary_node_cnt_each = 0;
                uint free_node_cnt = 0;
                uint free_node_cnt0 = 0;
                
                // ソートされた節点を取得する
                if (boundaryCnt == 6)
                {
                    // 六角形領域の場合
                    uint work_boundary_node_cnt_B1 = 0;
                    uint work_boundary_node_cnt_B2 = 0;
                    uint work_boundary_node_cnt_B3 = 0;
                    uint work_boundary_node_cnt_B4 = 0;
                    uint work_boundary_node_cnt_B5 = 0;
                    uint work_boundary_node_cnt_B6 = 0;
                    getSortedNodes_Hex(
                        probNo,
                        node_cnt,
                        FieldForceBcId,
                        to_no_boundary_fieldForceBcId,
                        no_c_all_fieldPortBcId_list,
                        to_no_boundary_fieldPortBcId_list,
                        sharedNodes,
                        ref sortedNodes,
                        ref toSorted,
                        ref work_boundary_node_cnt_B1,
                        ref work_boundary_node_cnt_B2,
                        ref work_boundary_node_cnt_B3,
                        ref work_boundary_node_cnt_B4,
                        ref work_boundary_node_cnt_B5,
                        ref work_boundary_node_cnt_B6,
                        ref boundary_node_cnt,
                        ref free_node_cnt,
                        ref free_node_cnt0
                        );
                    boundary_node_cnt_each = work_boundary_node_cnt_B1;
                    System.Diagnostics.Debug.Assert(work_boundary_node_cnt_B2 == (boundary_node_cnt_each - 2));
                    System.Diagnostics.Debug.Assert(work_boundary_node_cnt_B3 == (boundary_node_cnt_each - 2));
                    System.Diagnostics.Debug.Assert(work_boundary_node_cnt_B4 == (boundary_node_cnt_each - 2));
                    System.Diagnostics.Debug.Assert(work_boundary_node_cnt_B5 == (boundary_node_cnt_each - 2));
                    System.Diagnostics.Debug.Assert(work_boundary_node_cnt_B6 == (boundary_node_cnt_each - 2));
                }
                else
                {
                    // 四角形領域の場合
                    getSortedNodes_Rect(
                        probNo,
                        node_cnt,
                        FieldForceBcId,
                        to_no_boundary_fieldForceBcId,
                        no_c_all_fieldPortBcId1,
                        no_c_all_fieldPortBcId2,
                        no_c_all_fieldPortBcId3,
                        no_c_all_fieldPortBcId4,
                        to_no_boundary_fieldPortBcId1,
                        to_no_boundary_fieldPortBcId2,
                        to_no_boundary_fieldPortBcId3,
                        to_no_boundary_fieldPortBcId4,
                        sharedNodes,
                        ref sortedNodes,
                        ref toSorted,
                        ref boundary_node_cnt_B1,
                        ref boundary_node_cnt_B3,
                        ref boundary_node_cnt,
                        ref free_node_cnt,
                        ref free_node_cnt0
                        );
                }

                // バンド構造計算時は、強制境界なし
                System.Diagnostics.Debug.Assert(FieldForceBcId == 0);
                System.Diagnostics.Debug.Assert(free_node_cnt0 == node_cnt);

                // 剛性行列、質量行列を作成
                KrdLab.clapack.Complex[] KMat0 = null;
                KrdLab.clapack.Complex[] MMat0 = null;
                {
                    KrdLab.clapack.Complex betaForMakingMat = 0.0;
                    betaForMakingMat = 0.0; // 直接Bloch境界条件を指定する場合
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

                // ソートされた行列を取得する
                KrdLab.clapack.Complex[] KMat = null;
                KrdLab.clapack.Complex[] MMat = null;
                // 四角形領域
                KrdLab.clapack.Complex expAX = 0.0;
                KrdLab.clapack.Complex expAY = 0.0;
                // 四角形領域 問題3
                KrdLab.clapack.Complex expAY1 = 0.0;
                KrdLab.clapack.Complex expAY2 = 0.0;
                // 六角形領域
                KrdLab.clapack.Complex expA1 = 0.0;
                KrdLab.clapack.Complex expA2 = 0.0;
                KrdLab.clapack.Complex expA3 = 0.0;
                if (boundaryCnt == 6)
                {
                    getSortedMatrix_Hex(
                        probNo,
                        periodicDistanceX,
                        periodicDistanceY,
                        betaX,
                        betaY,
                        coord_c_all,
                        sharedNodes,
                        sharedNodeCoords,
                        sortedNodes, toSorted,
                        boundary_node_cnt_each,
                        boundary_node_cnt,
                        free_node_cnt,
                        free_node_cnt0,
                        KMat0,
                        MMat0,
                        out expA1,
                        out expA2,
                        out expA3,
                        out KMat,
                        out MMat
                        );
                }
                else
                {
                    getSortedMatrix_Rect(
                        probNo,
                        periodicDistanceX,
                        periodicDistanceY,
                        betaX,
                        betaY,
                        no_c_all_fieldPortBcId1,
                        no_c_all_fieldPortBcId3,
                        coord_c_all,
                        sharedNodes,
                        sharedNodeCoords,
                        sortedNodes, toSorted,
                        boundary_node_cnt_B1,
                        boundary_node_cnt_B3,
                        boundary_node_cnt,
                        free_node_cnt,
                        free_node_cnt0,
                        KMat0,
                        MMat0,
                        out expAX,
                        out expAY,
                        out expAY1,
                        out expAY2,
                        out KMat,
                        out MMat
                        );
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

                    // 表示用にデータを格納する
                    if (betaIndex == 0)
                    {
                        EigenValueList.Clear();
                    }
                    Complex[] complexNormalizedFreqList = new Complex[MaxModeCnt];
                    for (int imode = 0; imode < evals.Length; imode++)
                    {
                        // 固有周波数
                        KrdLab.clapack.Complex complex_k0_eigen = KrdLab.clapack.Complex.Sqrt(evals[imode]);
                        // 規格化周波数
                        KrdLab.clapack.Complex complexNormalizedFreq = latticeA * (complex_k0_eigen / (2.0 * pi));
                        if (imode < MaxModeCnt)
                        {
                            System.Diagnostics.Debug.WriteLine("a/λ  ( " + imode + " ) = " + complexNormalizedFreq.Real + " + " + complexNormalizedFreq.Imaginary + " i ");
                            complexNormalizedFreqList[imode] = new Complex(complexNormalizedFreq.Real, complexNormalizedFreq.Imaginary);
                        }
                    }
                    EigenValueList.Add(complexNormalizedFreqList);

                    // 固有ベクトルの格納(1つだけ格納)
                    int tagtModeIndex = 0;
                    KrdLab.clapack.Complex[] evec = MyUtilLib.Matrix.MyMatrixUtil.matrix_GetRowVec(evecs, tagtModeIndex);
                    for (int ino = 0; ino < evec.Length; ino++)
                    {
                        uint nodeNumber = sortedNodes[ino];
                        resVec[nodeNumber] = evec[ino];
                    }
                }

                // 従属な節点の界をセットする
                if (boundaryCnt == 6)
                {
                    // 六角形
                    setDependentFieldVal_Hex(
                        probNo,
                        no_c_all_fieldPortBcId1,
                        no_c_all_fieldPortBcId2,
                        no_c_all_fieldPortBcId3,
                        no_c_all_fieldPortBcId4,
                        no_c_all_fieldPortBcId5,
                        no_c_all_fieldPortBcId6,
                        sharedNodes,
                        expA1,
                        expA2,
                        expA3,
                        ref resVec);
                }
                else
                {
                    // 四角形
                    setDependentFieldVal_Rect(
                        probNo,
                        no_c_all_fieldPortBcId1,
                        no_c_all_fieldPortBcId2,
                        no_c_all_fieldPortBcId3,
                        no_c_all_fieldPortBcId4,
                        sharedNodes,
                        expAX,
                        expAY,
                        expAY1,
                        expAY2,
                        ref resVec);
                }

                // 位相調整
                KrdLab.clapack.Complex phaseShift = 1.0;
                double maxAbs = double.MinValue;
                KrdLab.clapack.Complex fValueAtMaxAbs = 0.0;
                {
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
                for (int i = 0; i < resVec.Length; i++)
                {
                    resVec[i] /= phaseShift;
                }
                //------------------------------------------------------------------
                // 計算結果の後処理
                //------------------------------------------------------------------
                // 固有ベクトルの計算結果をワールド座標系にセットする
                WgUtilForPeriodicEigenBetaSpecified.SetFieldValueForDisplay(World, FieldValId, resVec);
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
                Complex[] work_complexNormalizedFreqList = new Complex[MaxModeCnt];
                for (int imode = 0; imode < MaxModeCnt; imode++)
                {
                    work_complexNormalizedFreqList[imode] = new Complex(0.0, 0.0);
                }
                EigenValueList.Add(work_complexNormalizedFreqList);

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
        /// 四角形領域：ソートされた節点リストを取得する
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="node_cnt"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="to_no_boundary_fieldForceBcId"></param>
        /// <param name="no_c_all_fieldPortBcId1"></param>
        /// <param name="no_c_all_fieldPortBcId2"></param>
        /// <param name="no_c_all_fieldPortBcId3"></param>
        /// <param name="no_c_all_fieldPortBcId4"></param>
        /// <param name="to_no_boundary_fieldPortBcId1"></param>
        /// <param name="to_no_boundary_fieldPortBcId2"></param>
        /// <param name="to_no_boundary_fieldPortBcId3"></param>
        /// <param name="to_no_boundary_fieldPortBcId4"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="boundary_node_cnt_B1"></param>
        /// <param name="boundary_node_cnt_B3"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="free_node_cnt0"></param>
        private static void getSortedNodes_Rect(
            int probNo,
            uint node_cnt,
            uint FieldForceBcId,
            Dictionary<uint, uint> to_no_boundary_fieldForceBcId,
            uint[] no_c_all_fieldPortBcId1,
            uint[] no_c_all_fieldPortBcId2,
            uint[] no_c_all_fieldPortBcId3,
            uint[] no_c_all_fieldPortBcId4,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId1,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId2,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId3,
            Dictionary<uint, uint> to_no_boundary_fieldPortBcId4,
            uint[] sharedNodes,
            ref IList<uint> sortedNodes,
            ref Dictionary<uint, int> toSorted,
            ref uint boundary_node_cnt_B1,
            ref uint boundary_node_cnt_B3,
            ref uint boundary_node_cnt,
            ref uint free_node_cnt,
            ref uint free_node_cnt0
            )
        {
            // 四角形領域の場合

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
                if (probNo == 3)
                {
                }
                else
                {
                    // 境界1と境界3の共有の頂点(左下頂点)
                    if (nodeNumberPortBc1 == sharedNodes[1])
                    {
                        continue;
                    }
                }
                sortedNodes.Add(nodeNumberPortBc1);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc1, nodeIndex);
            }
            boundary_node_cnt_B1 = (uint)sortedNodes.Count; // 境界1
            //   境界3と境界4は周期構造条件より同じ界の値をとる
            // ポート境界3
            for (int i = 0; i < no_c_all_fieldPortBcId3.Length; i++)
            {
                // 境界3の節点を追加
                uint nodeNumberPortBc3 = no_c_all_fieldPortBcId3[i];
                if (FieldForceBcId != 0)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumberPortBc3)) continue;
                }
                // 境界3と境界1の共有の頂点（左下頂点）
                if (nodeNumberPortBc3 == sharedNodes[1])
                {
                    continue;
                }
                // 境界3と境界2の共有の頂点（右下頂点）
                if (nodeNumberPortBc3 == sharedNodes[3])
                {
                    continue;
                }
                sortedNodes.Add(nodeNumberPortBc3);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc3, nodeIndex);
            }
            boundary_node_cnt = (uint)sortedNodes.Count; // 境界1 + 3
            boundary_node_cnt_B3 = boundary_node_cnt - boundary_node_cnt_B1; // 境界3
            // 内部領域
            for (uint nodeNumber = 0; nodeNumber < node_cnt; nodeNumber++)
            {
                // 追加済み節点はスキップ
                //if (toSorted.ContainsKey(nodeNumber)) continue;
                // 境界1は除く
                if (to_no_boundary_fieldPortBcId1.ContainsKey(nodeNumber)) continue;
                // 境界2は除く
                if (to_no_boundary_fieldPortBcId2.ContainsKey(nodeNumber)) continue;
                // 境界3は除く
                if (to_no_boundary_fieldPortBcId3.ContainsKey(nodeNumber)) continue;
                // 境界4は除く
                if (to_no_boundary_fieldPortBcId4.ContainsKey(nodeNumber)) continue;
                if (FieldForceBcId != 0)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumber)) continue;
                }
                sortedNodes.Add(nodeNumber);
                toSorted.Add(nodeNumber, sortedNodes.Count - 1);
            }
            free_node_cnt = (uint)sortedNodes.Count;  // 境界1 + 境界3 + 内部領域
            // 境界2
            for (int i = 0; i < no_c_all_fieldPortBcId2.Length; i++)
            {
                // 境界2の節点を追加
                uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[i];
                if (FieldForceBcId != 0)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumberPortBc2)) continue;
                }
                if (probNo == 3)
                {
                }
                else
                {
                    // 境界2と境界3の共有の頂点（右下頂点）
                    if (nodeNumberPortBc2 == sharedNodes[3])
                    {
                        continue;
                    }
                }
                sortedNodes.Add(nodeNumberPortBc2);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc2, nodeIndex);
            }
            // 境界4
            for (int i = 0; i < no_c_all_fieldPortBcId4.Length; i++)
            {
                // 境界4の節点を追加
                uint nodeNumberPortBc4 = no_c_all_fieldPortBcId4[i];
                if (FieldForceBcId != 0)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumberPortBc4)) continue;
                }
                // 境界4と境界1の共有の節点(左上頂点)
                if (nodeNumberPortBc4 == sharedNodes[0])
                {
                    continue;
                }
                // 境界4と境界2の共有の節点(右上頂点）
                if (nodeNumberPortBc4 == sharedNodes[2])
                {
                    continue;
                }
                sortedNodes.Add(nodeNumberPortBc4);
                int nodeIndex = sortedNodes.Count - 1;
                toSorted.Add(nodeNumberPortBc4, nodeIndex);
            }
            if (probNo == 3)
            {
            }
            else
            {
                // 境界3上の共有節点
                // 左下
                sortedNodes.Add(sharedNodes[1]);
                toSorted.Add(sharedNodes[1], sortedNodes.Count - 1);
                // 右下
                sortedNodes.Add(sharedNodes[3]);
                toSorted.Add(sharedNodes[3], sortedNodes.Count - 1);
            }
            free_node_cnt0 = (uint)sortedNodes.Count;  // 境界1 + 内部領域 + 境界2 + 境界4
        }

        /// <summary>
        /// 四角形領域：ソートされた行列を取得する
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="periodicDistanceX"></param>
        /// <param name="periodicDistanceY"></param>
        /// <param name="betaX"></param>
        /// <param name="betaY"></param>
        /// <param name="no_c_all_fieldPortBcId1"></param>
        /// <param name="no_c_all_fieldPortBcId3"></param>
        /// <param name="coord_c_all"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="sharedNodeCoords"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="boundary_node_cnt_B1"></param>
        /// <param name="boundary_node_cnt_B3"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="free_node_cnt0"></param>
        /// <param name="KMat0"></param>
        /// <param name="MMat0"></param>
        /// <param name="expAX"></param>
        /// <param name="expAY"></param>
        /// <param name="expAY1"></param>
        /// <param name="expAY2"></param>
        /// <param name="KMat"></param>
        /// <param name="MMat"></param>
        private static void getSortedMatrix_Rect(
            int probNo,
            double periodicDistanceX,
            double periodicDistanceY,
            KrdLab.clapack.Complex betaX,
            KrdLab.clapack.Complex betaY,
            uint[] no_c_all_fieldPortBcId1,
            uint[] no_c_all_fieldPortBcId3,
            double[][] coord_c_all,
            uint[] sharedNodes,
            double[][] sharedNodeCoords,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            uint boundary_node_cnt_B1,
            uint boundary_node_cnt_B3,
            uint boundary_node_cnt,
            uint free_node_cnt,
            uint free_node_cnt0,
            KrdLab.clapack.Complex[] KMat0,
            KrdLab.clapack.Complex[] MMat0,
            out KrdLab.clapack.Complex expAX,
            out KrdLab.clapack.Complex expAY,
            out KrdLab.clapack.Complex expAY1,
            out KrdLab.clapack.Complex expAY2,
            out KrdLab.clapack.Complex[] KMat,
            out KrdLab.clapack.Complex[] MMat
            )
        {

            // 境界2の節点は境界1の節点と同一とみなす
            //   境界上の分割が同じであることが前提条件
            KMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
            MMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
            // 直接Bloch境界条件を指定する場合
            expAX = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX);
            expAY = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaY * periodicDistanceY);
            // 左下と左上のX座標オフセット
            double ofsX = sharedNodeCoords[0][0] - sharedNodeCoords[1][0];
            if (Math.Abs(ofsX) >= Constants.PrecisionLowerLimit)
            {
                // 斜め領域の場合
                //  Y方向の周期境界条件にはオフセット分のX方向成分の因子が入る
                System.Diagnostics.Debug.WriteLine("ofsX: {0}", ofsX);
                expAY *= KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * ofsX);
            }
            expAY1 = 0.0;
            expAY2 = 0.0;
            if (probNo == 3)
            {
                expAY = 1.0;
                expAY1 = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX * 0.5)
                    * KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaY * periodicDistanceY);
                expAY2 = KrdLab.clapack.Complex.Exp(1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX * 0.5)
                    * KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaY * periodicDistanceY);
            }
            int nodeIndexShared0 = toSorted[sharedNodes[0]]; // 左上頂点
            System.Diagnostics.Debug.Assert((free_node_cnt + boundary_node_cnt_B1 - 1 - nodeIndexShared0) == toSorted[sharedNodes[2]]);//左上と右上
            if (probNo == 3)
            {
                nodeIndexShared0 = -1;
            }
            uint boundary_node_cnt_Half_B3 = 0;
            if (probNo == 3)
            {
                System.Diagnostics.Debug.Assert(no_c_all_fieldPortBcId3.Length % 2 == 1);
                boundary_node_cnt_Half_B3 = ((uint)no_c_all_fieldPortBcId3.Length - 1) / 2;
                System.Diagnostics.Debug.Assert(boundary_node_cnt_B3 == (no_c_all_fieldPortBcId3.Length - 2));
                System.Diagnostics.Debug.Assert(boundary_node_cnt_B3 == (boundary_node_cnt_Half_B3 * 2 - 1));
            }

            /*
            if (probNo == 3)
            {
                ////////////////////////////////////////////////////////////////////////////////////
                // check
                {
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        int j_3_L = (int)(j + boundary_node_cnt_B1);
                        uint nodeNumber_3_L = sortedNodes[j_3_L];
                        double[] coord_3_L = coord_c_all[nodeNumber_3_L];
                        System.Diagnostics.Debug.WriteLine("3_L: {0}  {1}  {2} {3}", j_3_L, nodeNumber_3_L, coord_3_L[0], coord_3_L[1]); 

                        int j_4_R = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j);
                        uint nodeNumber_4_R = sortedNodes[j_4_R];
                        double[] coord_4_R = coord_c_all[nodeNumber_4_R];
                        System.Diagnostics.Debug.WriteLine("4_R: {0}  {1}  {2} {3}", j_4_R, nodeNumber_4_R, coord_4_R[0], coord_4_R[1]);
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        uint nodeNumber_shared1 = sharedNodes[1];
                        int j_shared1 = toSorted[nodeNumber_shared1];
                        double[] coord_shared1 = coord_c_all[nodeNumber_shared1];
                        System.Diagnostics.Debug.WriteLine("shared1: {0}  {1}  {2} {3}", j_shared1, nodeNumber_shared1, coord_shared1[0], coord_shared1[1]); 

                        // 境界4中点
                        int jm_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                        uint nodeNumber_m_B4 = sortedNodes[jm_B4];
                        double[] coord_m_B4 = coord_c_all[nodeNumber_m_B4];
                        System.Diagnostics.Debug.WriteLine("m_B4: {0}  {1}  {2} {3}", jm_B4, nodeNumber_m_B4, coord_m_B4[0], coord_m_B4[1]);
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        int j_3_R = (int)(j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3);
                        uint nodeNumber_3_R = sortedNodes[j_3_R];
                        double[] coord_3_R = coord_c_all[nodeNumber_3_R];
                        System.Diagnostics.Debug.WriteLine("3_R: {0}  {1}  {2} {3}", j_3_R, nodeNumber_3_R, coord_3_R[0], coord_3_R[1]);
                            
                        int j_4_L = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j);
                        uint nodeNumber_4_L = sortedNodes[j_4_L];
                        double[] coord_4_L = coord_c_all[nodeNumber_4_L];
                        System.Diagnostics.Debug.WriteLine("4_L: {0}  {1}  {2} {3}", j_4_L, nodeNumber_4_L, coord_4_L[0], coord_4_L[1]);
                    }
                }
                ////////////////////////////////////////////////////////////////////////////////////
            }
             */

            /////////////////////////////////////////////////////////////////
            // 境界1+3+内部
            for (int i = 0; i < free_node_cnt; i++)
            {
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                    MMat[i + free_node_cnt * j] = MMat0[i + free_node_cnt0 * j];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 境界1+3+内部
            for (int i = 0; i < free_node_cnt; i++)
            {
                // 境界2
                for (int j = 0; j < boundary_node_cnt_B1; j++)
                {
                    if (probNo == 3)
                    {
                    }
                    else
                    {
                        if (j == nodeIndexShared0) continue; // 右上頂点(左上頂点に対応)は別処理
                    }
                    KMat[i + free_node_cnt * j] += expAX * KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    MMat[i + free_node_cnt * j] += expAX * MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                }
                if (probNo == 3)
                {
                    ////////////////////////////////////////////////////////////////////////////////////
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            expAY1 * KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            expAY1 * MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        int j1 = toSorted[sharedNodes[1]];
                        // 境界4中点
                        int jm_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                        KMat[i + free_node_cnt * j1] += expAY1 * KMat0[i + free_node_cnt0 * jm_B4];
                        MMat[i + free_node_cnt * j1] += expAY1 * MMat0[i + free_node_cnt0 * jm_B4];
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            expAY2 * KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            expAY2 * MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    ////////////////////////////////////////////////////////////////////////////////////
                }
                else
                {
                    // 通常の処理
                    // 境界4
                    for (int j = 0; j < boundary_node_cnt_B3; j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            expAY * KMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            expAY * MMat0[i + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    // 左上頂点と左下頂点
                    {
                        int j0 = toSorted[sharedNodes[0]]; // 左上頂点
                        int j1 = toSorted[sharedNodes[1]]; // 左下頂点
                        int j2 = toSorted[sharedNodes[2]]; // 右上頂点
                        int j3 = toSorted[sharedNodes[3]]; // 右下頂点
                        KMat[i + free_node_cnt * j0] +=
                              (1.0 / expAY) * KMat0[i + free_node_cnt0 * j1]
                            + expAX * KMat0[i + free_node_cnt0 * j2]
                            + (expAX / expAY) * KMat0[i + free_node_cnt0 * j3];
                        MMat[i + free_node_cnt * j0] +=
                              (1.0 / expAY) * MMat0[i + free_node_cnt0 * j1]
                            + expAX * MMat0[i + free_node_cnt0 * j2]
                            + (expAX / expAY) * MMat0[i + free_node_cnt0 * j3];
                    }
                }
            }

            /////////////////////////////////////////////////////////////////
            // 境界2
            for (int i = 0; i < boundary_node_cnt_B1; i++)
            {
                if (probNo == 3)
                {
                }
                else
                {
                    if (i == nodeIndexShared0) continue; // 右上頂点(左上頂点に対応)は別処理
                }
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i + free_node_cnt * j] += (1.0 / expAX) * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j];
                    MMat[i + free_node_cnt * j] += (1.0 / expAX) * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j];
                }
                // 境界2
                for (int j = 0; j < boundary_node_cnt_B1; j++)
                {
                    if (probNo == 3)
                    {
                    }
                    else
                    {
                        if (j == nodeIndexShared0) continue; // 右上頂点(左上頂点に対応)は別処理
                    }
                    KMat[i + free_node_cnt * j] += KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    MMat[i + free_node_cnt * j] += MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                }
                if (probNo == 3)
                {
                    ////////////////////////////////////////////////////////////////////////////////////
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAX) * expAY1 * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAX) * expAY1 * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        int j1 = toSorted[sharedNodes[1]];
                        // 境界4中点
                        int jm_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                        KMat[i + free_node_cnt * j1] += (1.0 / expAX) * expAY1 * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * jm_B4];
                        MMat[i + free_node_cnt * j1] += (1.0 / expAX) * expAY1 * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * jm_B4];
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAX) * expAY2 * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAX) * expAY2 * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    ////////////////////////////////////////////////////////////////////////////////////
                }
                else
                {
                    // 通常の処理
                    // 境界4
                    for (int j = 0; j < boundary_node_cnt_B3; j++)
                    {
                        KMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAX) * expAY * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                          + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAX) * expAY * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i)
                                                          + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    {
                        int j0 = toSorted[sharedNodes[0]]; // 左上頂点
                        int j1 = toSorted[sharedNodes[1]]; // 左下頂点
                        int j2 = toSorted[sharedNodes[2]]; // 右上頂点
                        int j3 = toSorted[sharedNodes[3]]; // 右下頂点
                        KMat[i + free_node_cnt * j0] +=
                              (1.0 / expAX) * (1.0 / expAY) * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j1]
                            + (1.0 / expAX) * expAX * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j2]
                            + (1.0 / expAX) * (expAX / expAY) * KMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j3];
                        MMat[i + free_node_cnt * j0] +=
                              (1.0 / expAX) * (1.0 / expAY) * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j1]
                            + (1.0 / expAX) * expAX * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j2]
                            + (1.0 / expAX) * (expAX / expAY) * MMat0[(free_node_cnt + boundary_node_cnt_B1 - 1 - i) + free_node_cnt0 * j3];
                    }
                }
            }

            if (probNo == 3)
            {
                ////////////////////////////////////////////////////////////////////////////////////
                // 境界4右半分
                for (int i = 0; i < (boundary_node_cnt_Half_B3 - 1); i++)
                {
                    // 境界1+3+内部
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY1) * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i) + free_node_cnt0 * j];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY1) * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i) + free_node_cnt0 * j];
                    }
                    // 境界2
                    for (int j = 0; j < boundary_node_cnt_B1; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY1) * expAX * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY1) * expAX * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                                           + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    }
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        int j1 = toSorted[sharedNodes[1]];
                        // 境界4中点
                        int jm_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j1] +=
                            (1.0 / expAY1) * expAY1 * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i) + free_node_cnt0 * jm_B4];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j1] +=
                            (1.0 / expAY1) * expAY1 * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i) + free_node_cnt0 * jm_B4];
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAY1) * expAY2 * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAY1) * expAY2 * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                }
                // 境界4中点
                {
                    // 左下頂点
                    int i1 = toSorted[sharedNodes[1]];
                    // 境界4中点
                    int im_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                    // 境界1+3+内部
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i1 + free_node_cnt * j] += (1.0 / expAY1) * KMat0[im_B4 + free_node_cnt0 * j];
                        MMat[i1 + free_node_cnt * j] += (1.0 / expAY1) * MMat0[im_B4 + free_node_cnt0 * j];
                    }
                    // 境界2
                    for (int j = 0; j < boundary_node_cnt_B1; j++)
                    {
                        KMat[i1 + free_node_cnt * j] += (1.0 / expAY1) * expAX * KMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                        MMat[i1 + free_node_cnt * j] += (1.0 / expAY1) * expAX * MMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    }
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAY1) * expAY1 * KMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                        MMat[i1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAY1) * expAY1 * MMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        int j1 = i1;
                        // 境界4中点
                        int jm_B4 = im_B4;
                        KMat[i1 + free_node_cnt * j1] +=
                            KMat0[im_B4 + free_node_cnt0 * jm_B4];
                        MMat[i1 + free_node_cnt * j1] +=
                            MMat0[im_B4 + free_node_cnt0 * jm_B4];
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i1 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAY1) * expAY2 * KMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i1 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            (1.0 / expAY1) * expAY2 * MMat0[im_B4 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                }
                // 境界4左半分
                for (int i = 0; i < (boundary_node_cnt_Half_B3 - 1); i++)
                {
                    // 境界1+3+内部
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j] +=
                            (1.0 / expAY2) * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j];
                        MMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j] +=
                            (1.0 / expAY2) * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j];
                    }
                    // 境界2
                    for (int j = 0; j < boundary_node_cnt_B1; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j] +=
                            (1.0 / expAY2) * expAX * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j] +=
                            (1.0 / expAY2) * expAX * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    }
                    // 境界4右半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAY2) * expAY1 * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                        MMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            (1.0 / expAY2) * expAY1 * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                            + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 2 - j)];
                    }
                    // 境界4中点
                    {
                        // 左下頂点
                        int j1 = toSorted[sharedNodes[1]];
                        // 境界4中点
                        int jm_B4 = (int)(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 - 1);
                        KMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j1] +=
                            (1.0 / expAY2) * expAY1 * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * jm_B4];
                        MMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * j1] +=
                            (1.0 / expAY2) * expAY1 * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * jm_B4];
                    }
                    // 境界4左半分
                    for (int j = 0; j < (boundary_node_cnt_Half_B3 - 1); j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3 + free_node_cnt * (j + boundary_node_cnt_B1 + boundary_node_cnt_Half_B3)] +=
                            MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                }
                ////////////////////////////////////////////////////////////////////////////////////
            }
            else
            {
                // 通常の処理
                // 境界4
                for (int i = 0; i < boundary_node_cnt_B3; i++)
                {
                    // 境界1+3+内部
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY) * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY) * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j];
                    }
                    // 境界2
                    for (int j = 0; j < boundary_node_cnt_B1; j++)
                    {
                        if (j == nodeIndexShared0) continue; // 右上頂点(左上頂点に対応)は別処理
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY) * expAX * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                          + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j] +=
                            (1.0 / expAY) * expAX * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                                          + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    }
                    // 境界4
                    for (int j = 0; j < boundary_node_cnt_B3; j++)
                    {
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                            MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i)
                                  + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    {
                        int j0 = toSorted[sharedNodes[0]]; // 左上頂点
                        int j1 = toSorted[sharedNodes[1]]; // 左下頂点
                        int j2 = toSorted[sharedNodes[2]]; // 右上頂点
                        int j3 = toSorted[sharedNodes[3]]; // 右下頂点
                        KMat[i + boundary_node_cnt_B1 + free_node_cnt * j0] +=
                              (1.0 / expAY) * (1.0 / expAY) * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j1]
                            + (1.0 / expAY) * expAX * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j2]
                            + (1.0 / expAY) * (expAX / expAY) * KMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j3];
                        MMat[i + boundary_node_cnt_B1 + free_node_cnt * j0] +=
                              (1.0 / expAY) * (1.0 / expAY) * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j1]
                            + (1.0 / expAY) * expAX * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j2]
                            + (1.0 / expAY) * (expAX / expAY) * MMat0[(free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - i) + free_node_cnt0 * j3];
                    }
                }
                // 左下頂点、右上頂点、右下頂点
                {
                    int i0 = toSorted[sharedNodes[0]]; // 左上頂点
                    int i1 = toSorted[sharedNodes[1]]; // 左下頂点
                    int i2 = toSorted[sharedNodes[2]]; // 右上頂点
                    int i3 = toSorted[sharedNodes[3]]; // 右下頂点
                    // 境界1+3+内部
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        KMat[i0 + free_node_cnt * j] += expAY * KMat0[i1 + free_node_cnt0 * j]
                            + (1.0 / expAX) * KMat0[i2 + free_node_cnt0 * j]
                            + (expAY / expAX) * KMat0[i3 + free_node_cnt0 * j];
                        MMat[i0 + free_node_cnt * j] += expAY * MMat0[i1 + free_node_cnt0 * j]
                            + (1.0 / expAX) * MMat0[i2 + free_node_cnt0 * j]
                            + (expAY / expAX) * MMat0[i3 + free_node_cnt0 * j];
                    }
                    // 境界2
                    for (int j = 0; j < boundary_node_cnt_B1; j++)
                    {
                        if (j == nodeIndexShared0) continue; // 右上頂点(左上頂点に対応)は別処理
                        KMat[i0 + free_node_cnt * j] +=
                              expAY * expAX * KMat0[i1 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)]
                            + (1.0 / expAX) * expAX * KMat0[i2 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)]
                            + (expAY / expAX) * expAX * KMat0[i3 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                        MMat[i0 + free_node_cnt * j] +=
                              expAY * expAX * MMat0[i1 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)]
                            + (1.0 / expAX) * expAX * MMat0[i2 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)]
                            + (expAY / expAX) * expAX * MMat0[i3 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 - 1 - j)];
                    }
                    // 境界4
                    for (int j = 0; j < boundary_node_cnt_B3; j++)
                    {
                        KMat[i0 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                              expAY * expAY * KMat0[i1 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)]
                            + (1.0 / expAX) * expAY * KMat0[i2 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)]
                            + (expAY / expAX) * expAY * KMat0[i3 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                        MMat[i0 + free_node_cnt * (j + boundary_node_cnt_B1)] +=
                              expAY * expAY * MMat0[i1 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)]
                            + (1.0 / expAX) * expAY * MMat0[i2 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)]
                            + (expAY / expAX) * expAY * MMat0[i3 + free_node_cnt0 * (free_node_cnt + boundary_node_cnt_B1 + boundary_node_cnt_B3 - 1 - j)];
                    }
                    {
                        int j0 = i0;
                        int j1 = i1;
                        int j2 = i2;
                        int j3 = i3;
                        KMat[i0 + free_node_cnt * j0] +=
                              expAY * (1.0 / expAY) * KMat0[i1 + free_node_cnt0 * j1]
                            + expAY * expAX * KMat0[i1 + free_node_cnt0 * j2]
                            + expAY * (expAX / expAY) * KMat0[i1 + free_node_cnt0 * j3]
                            + (1.0 / expAX) * (1.0 / expAY) * KMat0[i2 + free_node_cnt0 * j1]
                            + (1.0 / expAX) * expAX * KMat0[i2 + free_node_cnt0 * j2]
                            + (1.0 / expAX) * (expAX / expAY) * KMat0[i2 + free_node_cnt0 * j3]
                            + (expAY / expAX) * (1.0 / expAY) * KMat0[i3 + free_node_cnt0 * j1]
                            + (expAY / expAX) * expAX * KMat0[i3 + free_node_cnt0 * j2]
                            + (expAY / expAX) * (expAX / expAY) * KMat0[i3 + free_node_cnt0 * j3];
                        MMat[i0 + free_node_cnt * j0] +=
                              expAY * (1.0 / expAY) * MMat0[i1 + free_node_cnt0 * j1]
                            + expAY * expAX * MMat0[i1 + free_node_cnt0 * j2]
                            + expAY * (expAX / expAY) * MMat0[i1 + free_node_cnt0 * j3]
                            + (1.0 / expAX) * (1.0 / expAY) * MMat0[i2 + free_node_cnt0 * j1]
                            + (1.0 / expAX) * expAX * MMat0[i2 + free_node_cnt0 * j2]
                            + (1.0 / expAX) * (expAX / expAY) * MMat0[i2 + free_node_cnt0 * j3]
                            + (expAY / expAX) * (1.0 / expAY) * MMat0[i3 + free_node_cnt0 * j1]
                            + (expAY / expAX) * expAX * MMat0[i3 + free_node_cnt0 * j2]
                            + (expAY / expAX) * (expAX / expAY) * MMat0[i3 + free_node_cnt0 * j3];
                    }
                }
            }
        }

        /// <summary>
        /// 四角形領域：従属な節点の界をセットする
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="no_c_all_fieldPortBcId1"></param>
        /// <param name="no_c_all_fieldPortBcId2"></param>
        /// <param name="no_c_all_fieldPortBcId3"></param>
        /// <param name="no_c_all_fieldPortBcId4"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="expAX"></param>
        /// <param name="expAY"></param>
        /// <param name="expAY1"></param>
        /// <param name="expAY2"></param>
        /// <param name="resVec"></param>
        private static void setDependentFieldVal_Rect(
            int probNo,
            uint[] no_c_all_fieldPortBcId1,
            uint[] no_c_all_fieldPortBcId2,
            uint[] no_c_all_fieldPortBcId3,
            uint[] no_c_all_fieldPortBcId4,
            uint[] sharedNodes,
            KrdLab.clapack.Complex expAX,
            KrdLab.clapack.Complex expAY,
            KrdLab.clapack.Complex expAY1,
            KrdLab.clapack.Complex expAY2,
            ref KrdLab.clapack.Complex[] resVec
            )
        {
            if (probNo == 3)
            {
                // ポート境界1の節点の値を境界2にも格納する
                for (int i = 0; i < no_c_all_fieldPortBcId1.Length; i++)
                {
                    // 境界1の節点
                    uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[i];
                    // 境界1の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc1];

                    // 境界2の節点
                    uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[no_c_all_fieldPortBcId2.Length - 1 - i];
                    resVec[nodeNumberPortBc2] = expAX * cvalue; // 直接Bloch境界条件を指定する場合
                }
                // ポート境界3の節点の値を境界4にも格納する
                for (int i = 0; i < (no_c_all_fieldPortBcId3.Length - 1) / 2; i++)
                {
                    // 境界3の節点
                    uint nodeNumberPortBc3 = no_c_all_fieldPortBcId3[i];
                    // 境界3の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc3];

                    // 境界4の節点
                    uint nodeNumberPortBc4 = no_c_all_fieldPortBcId4[(no_c_all_fieldPortBcId4.Length - 1) / 2 - 1 - i];
                    resVec[nodeNumberPortBc4] = expAY1 * cvalue; // 直接Bloch境界条件を指定する場合
                }
                for (int i = (no_c_all_fieldPortBcId3.Length - 1) / 2; i < no_c_all_fieldPortBcId3.Length; i++)
                {
                    // 境界3の節点
                    uint nodeNumberPortBc3 = no_c_all_fieldPortBcId3[i];
                    // 境界3の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc3];

                    // 境界4の節点
                    uint nodeNumberPortBc4 = no_c_all_fieldPortBcId4[no_c_all_fieldPortBcId4.Length - 1 - (i - ((no_c_all_fieldPortBcId3.Length - 1) / 2))];
                    resVec[nodeNumberPortBc4] = expAY2 * cvalue; // 直接Bloch境界条件を指定する場合
                }
            }
            else
            {
                // 共有頂点の値を格納
                {
                    // 左上
                    uint nodeNumberShared0 = sharedNodes[0];
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberShared0];
                    // 左下
                    uint nodeNumberShared1 = sharedNodes[1];
                    resVec[nodeNumberShared1] = (1.0 / expAY) * cvalue;
                    // 右上
                    uint nodeNumberShared2 = sharedNodes[2];
                    resVec[nodeNumberShared2] = expAX * cvalue;
                    // 右下
                    uint nodeNumberShared3 = sharedNodes[3];
                    resVec[nodeNumberShared3] = (expAX / expAY) * cvalue;
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
                    resVec[nodeNumberPortBc2] = expAX * cvalue; // 直接Bloch境界条件を指定する場合
                }
                // ポート境界3の節点の値を境界4にも格納する
                for (int i = 0; i < no_c_all_fieldPortBcId3.Length; i++)
                {
                    // 境界3の節点
                    uint nodeNumberPortBc3 = no_c_all_fieldPortBcId3[i];
                    // 境界3の節点の界の値を取得
                    KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc3];

                    // 境界4の節点
                    uint nodeNumberPortBc4 = no_c_all_fieldPortBcId4[no_c_all_fieldPortBcId4.Length - 1 - i];
                    resVec[nodeNumberPortBc4] = expAY * cvalue; // 直接Bloch境界条件を指定する場合
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 六角形領域：ソートされた節点リストを取得する
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="node_cnt"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="to_no_boundary_fieldForceBcId"></param>
        /// <param name="no_c_all_fieldPortBcId_list"></param>
        /// <param name="to_no_boundary_fieldPortBcId_list"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="boundary_node_cnt_B1"></param>
        /// <param name="boundary_node_cnt_B2"></param>
        /// <param name="boundary_node_cnt_B3"></param>
        /// <param name="boundary_node_cnt_B4"></param>
        /// <param name="boundary_node_cnt_B5"></param>
        /// <param name="boundary_node_cnt_B6"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="free_node_cnt0"></param>
        private static void getSortedNodes_Hex(
            int probNo,
            uint node_cnt,
            uint FieldForceBcId,
            Dictionary<uint, uint> to_no_boundary_fieldForceBcId,
            IList<uint[]> no_c_all_fieldPortBcId_list,
            IList<Dictionary<uint, uint>> to_no_boundary_fieldPortBcId_list,
            uint[] sharedNodes,
            ref IList<uint> sortedNodes,
            ref Dictionary<uint, int> toSorted,
            ref uint boundary_node_cnt_B1,
            ref uint boundary_node_cnt_B2,
            ref uint boundary_node_cnt_B3,
            ref uint boundary_node_cnt_B4,
            ref uint boundary_node_cnt_B5,
            ref uint boundary_node_cnt_B6,
            ref uint boundary_node_cnt,
            ref uint free_node_cnt,
            ref uint free_node_cnt0
            )
        {
            int boundaryCnt = 6;

            // ポート境界1, 2, 3
            for (int boundaryIndex = 0; boundaryIndex < (boundaryCnt / 2); boundaryIndex++)
            {
                uint[] work_no_c_all_fieldPortBcId = no_c_all_fieldPortBcId_list[boundaryIndex];
                int workcnt = 0;
                for (int i = 0; i < work_no_c_all_fieldPortBcId.Length; i++)
                {
                    // 境界の節点を追加
                    uint work_nodeNumberPortBc = work_no_c_all_fieldPortBcId[i];
                    if (FieldForceBcId != 0)
                    {
                        // 強制境界を除く
                        if (to_no_boundary_fieldForceBcId.ContainsKey(work_nodeNumberPortBc)) continue;
                    }
                    if (boundaryIndex == 0)
                    {
                    }
                    else if (boundaryIndex == 1)
                    {
                        // 境界2の場合
                        // 左上頂点
                        if (work_nodeNumberPortBc == sharedNodes[1])
                        {
                            continue;
                        }
                        // 左下頂点
                        if (work_nodeNumberPortBc == sharedNodes[2])
                        {
                            continue;
                        }
                    }
                    else if (boundaryIndex == 2)
                    {
                        // 境界3の場合
                        // 左下頂点
                        if (work_nodeNumberPortBc == sharedNodes[2])
                        {
                            continue;
                        }
                        // 下頂点
                        if (work_nodeNumberPortBc == sharedNodes[3])
                        {
                            continue;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    sortedNodes.Add(work_nodeNumberPortBc);
                    int nodeIndex = sortedNodes.Count - 1;
                    toSorted.Add(work_nodeNumberPortBc, nodeIndex);
                    workcnt++;
                }
                if (boundaryIndex == 0)
                {
                    boundary_node_cnt_B1 = (uint)workcnt;
                }
                else if (boundaryIndex == 1)
                {
                    boundary_node_cnt_B2 = (uint)workcnt;
                }
                else if (boundaryIndex == 2)
                {
                    boundary_node_cnt_B3 = (uint)workcnt;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            boundary_node_cnt = (uint)sortedNodes.Count;
            // 内部領域
            for (uint nodeNumber = 0; nodeNumber < node_cnt; nodeNumber++)
            {
                bool isBoundaryNode = false;
                // 境界は除く
                for (int boundaryIndex = 0; boundaryIndex < boundaryCnt; boundaryIndex++)
                {
                    if (to_no_boundary_fieldPortBcId_list[boundaryIndex].ContainsKey(nodeNumber))
                    {
                        isBoundaryNode = true;
                        break;
                    }
                }
                if (isBoundaryNode)
                {
                    // 境界は除く
                    continue;
                }
                if (FieldForceBcId != 0)
                {
                    // 強制境界を除く
                    if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumber)) continue;
                }
                sortedNodes.Add(nodeNumber);
                toSorted.Add(nodeNumber, sortedNodes.Count - 1);
            }
            free_node_cnt = (uint)sortedNodes.Count;  // 境界1 + 境界2 + 境界3 + 内部領域

            // 境界4, 5, 6
            for (int boundaryIndex = (boundaryCnt / 2); boundaryIndex < boundaryCnt; boundaryIndex++)
            {
                uint[] work_no_c_all_fieldPortBcId = no_c_all_fieldPortBcId_list[boundaryIndex];
                int workcnt = 0;
                for (int i = 0; i < work_no_c_all_fieldPortBcId.Length; i++)
                {
                    // 境界の節点を追加
                    uint work_nodeNumberPortBc = work_no_c_all_fieldPortBcId[i];
                    if (FieldForceBcId != 0)
                    {
                        // 強制境界を除く
                        if (to_no_boundary_fieldForceBcId.ContainsKey(work_nodeNumberPortBc)) continue;
                    }
                    if (boundaryIndex == 3)
                    {
                        // 境界4の場合
                        // 下頂点
                        if (work_nodeNumberPortBc == sharedNodes[3])
                        {
                            continue;
                        }
                        // 右下頂点
                        if (work_nodeNumberPortBc == sharedNodes[4])
                        {
                            continue;
                        }
                    }
                    else if (boundaryIndex == 4)
                    {
                        // 境界5の場合
                        // 右下頂点
                        if (work_nodeNumberPortBc == sharedNodes[4])
                        {
                            continue;
                        }
                        // 右上頂点
                        if (work_nodeNumberPortBc == sharedNodes[5])
                        {
                            continue;
                        }
                    }
                    else if (boundaryIndex == 5)
                    {
                        // 境界6の場合
                        // 右上頂点
                        if (work_nodeNumberPortBc == sharedNodes[5])
                        {
                            continue;
                        }
                        // 上頂点
                        if (work_nodeNumberPortBc == sharedNodes[0])
                        {
                            continue;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    sortedNodes.Add(work_nodeNumberPortBc);
                    int nodeIndex = sortedNodes.Count - 1;
                    toSorted.Add(work_nodeNumberPortBc, nodeIndex);
                    workcnt++;
                }
                if (boundaryIndex == 3)
                {
                    boundary_node_cnt_B4 = (uint)workcnt;
                }
                else if (boundaryIndex == 4)
                {
                    boundary_node_cnt_B5 = (uint)workcnt;
                }
                else if (boundaryIndex == 5)
                {
                    boundary_node_cnt_B6 = (uint)workcnt;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            // 左下
            sortedNodes.Add(sharedNodes[2]);
            toSorted.Add(sharedNodes[2], sortedNodes.Count - 1);
            // 下
            sortedNodes.Add(sharedNodes[3]);
            toSorted.Add(sharedNodes[3], sortedNodes.Count - 1);
            // 右下
            sortedNodes.Add(sharedNodes[4]);
            toSorted.Add(sharedNodes[4], sortedNodes.Count - 1);
            // 右上
            sortedNodes.Add(sharedNodes[5]);
            toSorted.Add(sharedNodes[5], sortedNodes.Count - 1);

            free_node_cnt0 = (uint)sortedNodes.Count;  // 境界1 + 境界2 + 境界3 + 内部領域 + 境界4 + 境界5 + 境界6
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="periodicDistanceX"></param>
        /// <param name="periodicDistanceY"></param>
        /// <param name="betaX"></param>
        /// <param name="betaY"></param>
        /// <param name="coord_c_all"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="sharedNodeCoords"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="boundary_node_cnt_each"></param>
        /// <param name="boundary_node_cnt"></param>
        /// <param name="free_node_cnt"></param>
        /// <param name="free_node_cnt0"></param>
        /// <param name="KMat0"></param>
        /// <param name="MMat0"></param>
        /// <param name="expA1"></param>
        /// <param name="expA2"></param>
        /// <param name="expA3"></param>
        /// <param name="KMat"></param>
        /// <param name="MMat"></param>
        private static void getSortedMatrix_Hex(
            int probNo,
            double periodicDistanceX,
            double periodicDistanceY,
            KrdLab.clapack.Complex betaX,
            KrdLab.clapack.Complex betaY,
            double[][] coord_c_all,
            uint[] sharedNodes,
            double[][] sharedNodeCoords,
            IList<uint> sortedNodes,
            Dictionary<uint, int> toSorted,
            uint boundary_node_cnt_each,
            uint boundary_node_cnt,
            uint free_node_cnt,
            uint free_node_cnt0,
            KrdLab.clapack.Complex[] KMat0,
            KrdLab.clapack.Complex[] MMat0,
            out KrdLab.clapack.Complex expA1,
            out KrdLab.clapack.Complex expA2,
            out KrdLab.clapack.Complex expA3,
            out KrdLab.clapack.Complex[] KMat,
            out KrdLab.clapack.Complex[] MMat
            )
        {
            KMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
            MMat = new KrdLab.clapack.Complex[free_node_cnt * free_node_cnt];
            expA1 = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX * 0.5)
                * KrdLab.clapack.Complex.Exp(1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaY * periodicDistanceY * 0.75);
            expA2 = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX);
            expA3 = KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaX * periodicDistanceX * 0.5)
                * KrdLab.clapack.Complex.Exp(-1.0 * KrdLab.clapack.Complex.ImaginaryOne * betaY * periodicDistanceY * 0.75);
            /*
            // check
            {
                // 境界4→境界1
                for (int j = 0; j < (boundary_node_cnt_each - 1); j++)
                {
                    int j_B1 = j;
                    uint nodeNumber_B1 = sortedNodes[j_B1];
                    System.Diagnostics.Debug.WriteLine("B1 {0} {1} {2} {3}",
                        j_B1, nodeNumber_B1, coord_c_all[nodeNumber_B1][0], coord_c_all[nodeNumber_B1][1]);
                    int j_B4 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1));
                    uint nodeNumber_B4 = sortedNodes[j_B4];
                    System.Diagnostics.Debug.WriteLine("B4 {0} {1} {2} {3}",
                        j_B4, nodeNumber_B4, coord_c_all[nodeNumber_B4][0], coord_c_all[nodeNumber_B4][1]);
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    int j_B2 = (int)(j + boundary_node_cnt_each);
                    uint nodeNumber_B2 = sortedNodes[j_B2];
                    System.Diagnostics.Debug.WriteLine("B2 {0} {1} {2} {3}",
                        j_B2, nodeNumber_B2, coord_c_all[nodeNumber_B2][0], coord_c_all[nodeNumber_B2][1]);
                    int j_B5 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j);
                    uint nodeNumber_B5 = sortedNodes[j_B5];
                    System.Diagnostics.Debug.WriteLine("B5 {0} {1} {2} {3}",
                        j_B5, nodeNumber_B5, coord_c_all[nodeNumber_B5][0], coord_c_all[nodeNumber_B5][1]);
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    int j_B3 = (int)(j + boundary_node_cnt_each + (boundary_node_cnt_each - 2));
                    uint nodeNumber_B3 = sortedNodes[j_B3];
                    System.Diagnostics.Debug.WriteLine("B3 {0} {1} {2} {3}",
                        j_B3, nodeNumber_B3, coord_c_all[nodeNumber_B3][0], coord_c_all[nodeNumber_B3][1]);
                    int j_B6 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j);
                    uint nodeNumber_B6 = sortedNodes[j_B6];
                    System.Diagnostics.Debug.WriteLine("B6 {0} {1} {2} {3}",
                        j_B6, nodeNumber_B6, coord_c_all[nodeNumber_B6][0], coord_c_all[nodeNumber_B6][1]);
                }
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    uint nodeNumber_shared0 = sortedNodes[j0];
                    uint nodeNumber_shared4 = sortedNodes[j4];
                    uint nodeNumber_shared2 = sortedNodes[j2];
                    System.Diagnostics.Debug.WriteLine("shared0 {0} {1} {2} {3}",
                        j0, nodeNumber_shared0, coord_c_all[nodeNumber_shared0][0], coord_c_all[nodeNumber_shared0][1]);
                    System.Diagnostics.Debug.WriteLine("shared4 {0} {1} {2} {3}",
                        j4, nodeNumber_shared4, coord_c_all[nodeNumber_shared4][0], coord_c_all[nodeNumber_shared4][1]);
                    System.Diagnostics.Debug.WriteLine("shared2 {0} {1} {2} {3}",
                        j2, nodeNumber_shared2, coord_c_all[nodeNumber_shared2][0], coord_c_all[nodeNumber_shared2][1]);
                }
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    uint nodeNumber_shared1 = sortedNodes[j1];
                    uint nodeNumber_shared3 = sortedNodes[j3];
                    uint nodeNumber_shared5 = sortedNodes[j5];
                    System.Diagnostics.Debug.WriteLine("shared1 {0} {1} {2} {3}",
                        j1, nodeNumber_shared1, coord_c_all[nodeNumber_shared1][0], coord_c_all[nodeNumber_shared1][1]);
                    System.Diagnostics.Debug.WriteLine("shared3 {0} {1} {2} {3}",
                        j3, nodeNumber_shared3, coord_c_all[nodeNumber_shared3][0], coord_c_all[nodeNumber_shared3][1]);
                    System.Diagnostics.Debug.WriteLine("shared5 {0} {1} {2} {3}",
                        j5, nodeNumber_shared5, coord_c_all[nodeNumber_shared5][0], coord_c_all[nodeNumber_shared5][1]);
                }
            }
             */

            /////////////////////////////////////////////////////////////////
            // 境界1+2 + 3+内部
            for (int i = 0; i < free_node_cnt; i++)
            {
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i + free_node_cnt * j] = KMat0[i + free_node_cnt0 * j];
                    MMat[i + free_node_cnt * j] = MMat0[i + free_node_cnt0 * j];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 境界1+2+3+内部
            for (int i = 0; i < free_node_cnt; i++)
            {
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i + free_node_cnt * j] += expA1 * KMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i + free_node_cnt * j] += expA1 * MMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        expA2 * KMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        expA2 * MMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        expA3 * KMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        expA3 * MMat0[i + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    KMat[i + free_node_cnt * j0] +=
                          expA1 * KMat0[i + free_node_cnt0 * j4]
                        + (1.0 / expA3) * KMat0[i + free_node_cnt0 * j2];
                    MMat[i + free_node_cnt * j0] +=
                          expA1 * MMat0[i + free_node_cnt0 * j4]
                        + (1.0 / expA3) * MMat0[i + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    KMat[i + free_node_cnt * j1] +=
                          expA1 * KMat0[i + free_node_cnt0 * j3]
                        + expA2 * KMat0[i + free_node_cnt0 * j5];
                    MMat[i + free_node_cnt * j1] +=
                          expA1 * MMat0[i + free_node_cnt0 * j3]
                        + expA2 * MMat0[i + free_node_cnt0 * j5];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 境界4
            for (int i = 1; i < (boundary_node_cnt_each - 1); i++)
            {
                int i_B1 = i; // 境界1
                int i_B4 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (i - 1)); // 境界4
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i_B1 + free_node_cnt * j] += (1.0 / expA1) * KMat0[i_B4 + free_node_cnt0 * j];
                    MMat[i_B1 + free_node_cnt * j] += (1.0 / expA1) * MMat0[i_B4 + free_node_cnt0 * j];
                }
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i_B1 + free_node_cnt * j] += KMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i_B1 + free_node_cnt * j] += MMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B1 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        (1.0 / expA1) * expA2 * KMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i_B1 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        (1.0 / expA1) * expA2 * MMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B1 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        (1.0 / expA1) * expA3 * KMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i_B1 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        (1.0 / expA1) * expA3 * MMat0[i_B4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    KMat[i_B1 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * KMat0[i_B4 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * KMat0[i_B4 + free_node_cnt0 * j2];
                    MMat[i_B1 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * MMat0[i_B4 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * MMat0[i_B4 + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    KMat[i_B1 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * KMat0[i_B4 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * KMat0[i_B4 + free_node_cnt0 * j5];
                    MMat[i_B1 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * MMat0[i_B4 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * MMat0[i_B4 + free_node_cnt0 * j5];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 境界5
            for (int i = 0; i < (boundary_node_cnt_each - 2); i++)
            {
                int i_B2 = (int)(i + boundary_node_cnt_each); // 境界2
                int i_B5 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - i); // 境界5
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i_B2 + free_node_cnt * j] += (1.0 / expA2) * KMat0[i_B5 + free_node_cnt0 * j];
                    MMat[i_B2 + free_node_cnt * j] += (1.0 / expA2) * MMat0[i_B5 + free_node_cnt0 * j];
                }
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i_B2 + free_node_cnt * j] += (1.0 / expA2) * expA1 * KMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i_B2 + free_node_cnt * j] += (1.0 / expA2) * expA1 * MMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B2 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        KMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i_B2 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        MMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B2 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        (1.0 / expA2) * expA3 * KMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i_B2 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        (1.0 / expA2) * expA3 * MMat0[i_B5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    KMat[i_B2 + free_node_cnt * j0] +=
                          (1.0 / expA2) * expA1 * KMat0[i_B5 + free_node_cnt0 * j4]
                        + (1.0 / expA2) * (1.0 / expA3) * KMat0[i_B5 + free_node_cnt0 * j2];
                    MMat[i_B2 + free_node_cnt * j0] +=
                          (1.0 / expA2) * expA1 * MMat0[i_B5 + free_node_cnt0 * j4]
                        + (1.0 / expA2) * (1.0 / expA3) * MMat0[i_B5 + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    KMat[i_B2 + free_node_cnt * j1] +=
                          (1.0 / expA2) * expA1 * KMat0[i_B5 + free_node_cnt0 * j3]
                        + (1.0 / expA2) * expA2 * KMat0[i_B5 + free_node_cnt0 * j5];
                    MMat[i_B2 + free_node_cnt * j1] +=
                          (1.0 / expA2) * expA1 * MMat0[i_B5 + free_node_cnt0 * j3]
                        + (1.0 / expA2) * expA2 * MMat0[i_B5 + free_node_cnt0 * j5];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 境界6
            for (int i = 0; i < (boundary_node_cnt_each - 2); i++)
            {
                int i_B3 = (int)(i + boundary_node_cnt_each + (boundary_node_cnt_each - 2)); // 境界3
                int i_B6 = (int)(free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - i); // 境界6
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i_B3 + free_node_cnt * j] += (1.0 / expA3) * KMat0[i_B6 + free_node_cnt0 * j];
                    MMat[i_B3 + free_node_cnt * j] += (1.0 / expA3) * MMat0[i_B6 + free_node_cnt0 * j];
                }
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i_B3 + free_node_cnt * j] += (1.0 / expA3) * expA1 * KMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i_B3 + free_node_cnt * j] += (1.0 / expA3) * expA1 * MMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B3 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        (1.0 / expA3) * expA2 * KMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i_B3 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                        (1.0 / expA3) * expA2 * MMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i_B3 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        KMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i_B3 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                        MMat0[i_B6 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    KMat[i_B3 + free_node_cnt * j0] +=
                          (1.0 / expA3) * expA1 * KMat0[i_B6 + free_node_cnt0 * j4]
                        + (1.0 / expA3) * (1.0 / expA3) * KMat0[i_B6 + free_node_cnt0 * j2];
                    MMat[i_B3 + free_node_cnt * j0] +=
                          (1.0 / expA3) * expA1 * MMat0[i_B6 + free_node_cnt0 * j4]
                        + (1.0 / expA3) * (1.0 / expA3) * MMat0[i_B6 + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    KMat[i_B3 + free_node_cnt * j1] +=
                          (1.0 / expA3) * expA1 * KMat0[i_B6 + free_node_cnt0 * j3]
                        + (1.0 / expA3) * expA2 * KMat0[i_B6 + free_node_cnt0 * j5];
                    MMat[i_B3 + free_node_cnt * j1] +=
                          (1.0 / expA3) * expA1 * MMat0[i_B6 + free_node_cnt0 * j3]
                        + (1.0 / expA3) * expA2 * MMat0[i_B6 + free_node_cnt0 * j5];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 頂点4→0, 頂点2→0
            {
                int i0 = toSorted[sharedNodes[0]];
                int i4 = toSorted[sharedNodes[4]];
                int i2 = toSorted[sharedNodes[2]];
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i0 + free_node_cnt * j] +=
                          (1.0 / expA1) * KMat0[i4 + free_node_cnt0 * j]
                        + expA3 * KMat0[i2 + free_node_cnt0 * j];
                    MMat[i0 + free_node_cnt * j] +=
                          (1.0 / expA1) * MMat0[i4 + free_node_cnt0 * j]
                        + expA3 * MMat0[i2 + free_node_cnt0 * j];
                }
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i0 + free_node_cnt * j] +=
                          (1.0 / expA1) * expA1 * KMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))]
                        + expA3 * expA1 * KMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i0 + free_node_cnt * j] +=
                          (1.0 / expA1) * expA1 * MMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))]
                        + expA3 * expA1 * MMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i0 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                          (1.0 / expA1) * expA2 * KMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)]
                        + expA3 * expA2 * KMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i0 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                          (1.0 / expA1) * expA2 * MMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)]
                        + expA3 * expA2 * MMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i0 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                          (1.0 / expA1) * expA3 * KMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)]
                        + expA3 * expA3 * KMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i0 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                          (1.0 / expA1) * expA3 * MMat0[i4 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)]
                        + expA3 * expA3 * MMat0[i2 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = i0;
                    int j4 = i4;
                    int j2 = i2;
                    KMat[i0 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * KMat0[i4 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * KMat0[i4 + free_node_cnt0 * j2]
                        + expA3 * expA1 * KMat0[i2 + free_node_cnt0 * j4]
                        + expA3 * (1.0 / expA3) * KMat0[i2 + free_node_cnt0 * j2];
                    MMat[i0 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * MMat0[i4 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * MMat0[i4 + free_node_cnt0 * j2]
                        + expA3 * expA1 * MMat0[i2 + free_node_cnt0 * j4]
                        + expA3 * (1.0 / expA3) * MMat0[i2 + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = toSorted[sharedNodes[1]];
                    int j3 = toSorted[sharedNodes[3]];
                    int j5 = toSorted[sharedNodes[5]];
                    KMat[i0 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * KMat0[i4 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * KMat0[i4 + free_node_cnt0 * j5]
                        + expA3 * expA1 * KMat0[i2 + free_node_cnt0 * j3]
                        + expA3 * expA2 * KMat0[i2 + free_node_cnt0 * j5];
                    MMat[i0 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * MMat0[i4 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * MMat0[i4 + free_node_cnt0 * j5]
                        + expA3 * expA1 * MMat0[i2 + free_node_cnt0 * j3]
                        + expA3 * expA2 * MMat0[i2 + free_node_cnt0 * j5];
                }
            }
            /////////////////////////////////////////////////////////////////
            // 頂点3→1, 頂点5→1
            {
                int i1 = toSorted[sharedNodes[1]];
                int i3 = toSorted[sharedNodes[3]];
                int i5 = toSorted[sharedNodes[5]];
                // 境界1+3+内部
                for (int j = 0; j < free_node_cnt; j++)
                {
                    KMat[i1 + free_node_cnt * j] +=
                          (1.0 / expA1) * KMat0[i3 + free_node_cnt0 * j]
                        + (1.0 / expA2) * KMat0[i5 + free_node_cnt0 * j];
                    MMat[i1 + free_node_cnt * j] +=
                          (1.0 / expA1) * MMat0[i3 + free_node_cnt0 * j]
                        + (1.0 / expA2) * MMat0[i5 + free_node_cnt0 * j];
                }
                // 境界4→境界1
                for (int j = 1; j < (boundary_node_cnt_each - 1); j++)
                {
                    KMat[i1 + free_node_cnt * j] +=
                          (1.0 / expA1) * expA1 * KMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))]
                        + (1.0 / expA2) * expA1 * KMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                    MMat[i1 + free_node_cnt * j] +=
                          (1.0 / expA1) * expA1 * MMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))]
                        + (1.0 / expA2) * expA1 * MMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) - 1 - (j - 1))];
                }
                // 境界5→境界2
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i1 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                          (1.0 / expA1) * expA2 * KMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)]
                        + (1.0 / expA2) * expA2 * KMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                    MMat[i1 + free_node_cnt * (j + boundary_node_cnt_each)] +=
                          (1.0 / expA1) * expA2 * MMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)]
                        + (1.0 / expA2) * expA2 * MMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 2 - 1 - j)];
                }
                // 境界6→境界3
                for (int j = 0; j < (boundary_node_cnt_each - 2); j++)
                {
                    KMat[i1 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                          (1.0 / expA1) * expA3 * KMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)]
                        + (1.0 / expA2) * expA3 * KMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                    MMat[i1 + free_node_cnt * (j + boundary_node_cnt_each + (boundary_node_cnt_each - 2))] +=
                          (1.0 / expA1) * expA3 * MMat0[i3 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)]
                        + (1.0 / expA2) * expA3 * MMat0[i5 + free_node_cnt0 * (free_node_cnt + (boundary_node_cnt_each - 2) * 3 - 1 - j)];
                }
                // 頂点4→0, 頂点2→0
                {
                    int j0 = toSorted[sharedNodes[0]];
                    int j4 = toSorted[sharedNodes[4]];
                    int j2 = toSorted[sharedNodes[2]];
                    KMat[i1 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * KMat0[i3 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * KMat0[i3 + free_node_cnt0 * j2]
                        + (1.0 / expA2) * expA1 * KMat0[i5 + free_node_cnt0 * j4]
                        + (1.0 / expA2) * (1.0 / expA3) * KMat0[i5 + free_node_cnt0 * j2];
                    MMat[i1 + free_node_cnt * j0] +=
                          (1.0 / expA1) * expA1 * MMat0[i3 + free_node_cnt0 * j4]
                        + (1.0 / expA1) * (1.0 / expA3) * MMat0[i3 + free_node_cnt0 * j2]
                        + (1.0 / expA2) * expA1 * MMat0[i5 + free_node_cnt0 * j4]
                        + (1.0 / expA2) * (1.0 / expA3) * MMat0[i5 + free_node_cnt0 * j2];
                }
                // 頂点3→1, 頂点5→1
                {
                    int j1 = i1;
                    int j3 = i3;
                    int j5 = i5;
                    KMat[i1 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * KMat0[i3 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * KMat0[i3 + free_node_cnt0 * j5]
                        + (1.0 / expA2) * expA1 * KMat0[i5 + free_node_cnt0 * j3]
                        + (1.0 / expA2) * expA2 * KMat0[i5 + free_node_cnt0 * j5];
                    MMat[i1 + free_node_cnt * j1] +=
                          (1.0 / expA1) * expA1 * MMat0[i3 + free_node_cnt0 * j3]
                        + (1.0 / expA1) * expA2 * MMat0[i3 + free_node_cnt0 * j5]
                        + (1.0 / expA2) * expA1 * MMat0[i5 + free_node_cnt0 * j3]
                        + (1.0 / expA2) * expA2 * MMat0[i5 + free_node_cnt0 * j5];
                }
            }

        }

        /// <summary>
        /// 六角形領域：従属な節点の界をセットする
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="no_c_all_fieldPortBcId1"></param>
        /// <param name="no_c_all_fieldPortBcId2"></param>
        /// <param name="no_c_all_fieldPortBcId3"></param>
        /// <param name="no_c_all_fieldPortBcId4"></param>
        /// <param name="sharedNodes"></param>
        /// <param name="expAX"></param>
        /// <param name="expAY"></param>
        /// <param name="expAY1"></param>
        /// <param name="expAY2"></param>
        /// <param name="resVec"></param>
        private static void setDependentFieldVal_Hex(
            int probNo,
            uint[] no_c_all_fieldPortBcId1,
            uint[] no_c_all_fieldPortBcId2,
            uint[] no_c_all_fieldPortBcId3,
            uint[] no_c_all_fieldPortBcId4,
            uint[] no_c_all_fieldPortBcId5,
            uint[] no_c_all_fieldPortBcId6,
            uint[] sharedNodes,
            KrdLab.clapack.Complex expA1,
            KrdLab.clapack.Complex expA2,
            KrdLab.clapack.Complex expA3,
            ref KrdLab.clapack.Complex[] resVec
            )
        {
            // 共有頂点の値を格納
            {
                // 上
                uint nodeNumberShared0 = sharedNodes[0];
                KrdLab.clapack.Complex cvalue = resVec[nodeNumberShared0];
                // 右下
                uint nodeNumberShared4 = sharedNodes[4];
                resVec[nodeNumberShared4] = expA1 * cvalue;
                // 左下
                uint nodeNumberShared2 = sharedNodes[2];
                resVec[nodeNumberShared2] = (1.0 / expA3) * cvalue;
            }
            {
                // 左上
                uint nodeNumberShared1 = sharedNodes[1];
                KrdLab.clapack.Complex cvalue = resVec[nodeNumberShared1];
                // 下
                uint nodeNumberShared3 = sharedNodes[3];
                resVec[nodeNumberShared3] = expA1 * cvalue;
                // 右上
                uint nodeNumberShared5 = sharedNodes[5];
                resVec[nodeNumberShared5] = expA2 * cvalue;
            }
            // ポート境界1の節点の値を境界4にも格納する
            for (int i = 0; i < no_c_all_fieldPortBcId1.Length; i++)
            {
                // 境界1の節点
                uint nodeNumberPortBc1 = no_c_all_fieldPortBcId1[i];
                if (nodeNumberPortBc1 == sharedNodes[0]) continue;
                if (nodeNumberPortBc1 == sharedNodes[1]) continue;
                // 境界1の節点の界の値を取得
                KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc1];

                // 境界4の節点
                uint nodeNumberPortBc4 = no_c_all_fieldPortBcId4[no_c_all_fieldPortBcId4.Length - 1 - i];
                resVec[nodeNumberPortBc4] = expA1 * cvalue; // 直接Bloch境界条件を指定する場合
            }
            // ポート境界2の節点の値を境界5にも格納する
            for (int i = 0; i < no_c_all_fieldPortBcId2.Length; i++)
            {
                // 境界2の節点
                uint nodeNumberPortBc2 = no_c_all_fieldPortBcId2[i];
                if (nodeNumberPortBc2 == sharedNodes[1]) continue;
                if (nodeNumberPortBc2 == sharedNodes[2]) continue;
                // 境界3の節点の界の値を取得
                KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc2];

                // 境界5の節点
                uint nodeNumberPortBc5 = no_c_all_fieldPortBcId5[no_c_all_fieldPortBcId5.Length - 1 - i];
                resVec[nodeNumberPortBc5] = expA2 * cvalue; // 直接Bloch境界条件を指定する場合
            }
            // ポート境界3の節点の値を境界6にも格納する
            for (int i = 0; i < no_c_all_fieldPortBcId2.Length; i++)
            {
                // 境界2の節点
                uint nodeNumberPortBc3 = no_c_all_fieldPortBcId3[i];
                if (nodeNumberPortBc3 == sharedNodes[2]) continue;
                if (nodeNumberPortBc3 == sharedNodes[3]) continue;
                // 境界3の節点の界の値を取得
                KrdLab.clapack.Complex cvalue = resVec[nodeNumberPortBc3];

                // 境界6の節点
                uint nodeNumberPortBc6 = no_c_all_fieldPortBcId6[no_c_all_fieldPortBcId6.Length - 1 - i];
                resVec[nodeNumberPortBc6] = expA3 * cvalue; // 直接Bloch境界条件を指定する場合
            }
        }

        /// <summary>
        /// パンとスケールの設定
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="Camera"></param>
        private static void setupPanAndScale(int probNo, CCamera Camera)
        {
            {
                // 表示位置調整
                Camera.MousePan(0, 0, 0.7, 1.3);
                double tmp_scale = 0.5;
                Camera.SetScale(tmp_scale);
            }
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
        private static void getBeta(
            ref int betaIndex,
            bool isTriLattice,
            int calcBetaCnt,
            double latticeA,
            double periodicDistanceX,
            double periodicDistanceY,
            ref double betaX,
            ref double betaY)
        {
            betaX = 0.0;
            betaY = 0.0;
            if (betaIndex >= (calcBetaCnt * 3 + 1))
            {
                //最初に戻る
                //betaIndex = 0;

                // 終了
                betaIndex = -1;
                return;
            }

            if (!isTriLattice)
            {
                /*
                // 正方格子
                if (betaIndex < calcBetaCnt)
                {
                    // Γ-X
                    int workIndex = betaIndex;
                    betaX = workIndex * (pi / latticeA) / calcBetaCnt;
                    betaY = 0.0;
                }
                else if (betaIndex < 2.0 * calcBetaCnt)
                {
                    // X -M
                    int workIndex = betaIndex - calcBetaCnt;
                    betaX = pi / latticeA;
                    betaY = workIndex * (pi / latticeA) / calcBetaCnt;
                }
                else
                {
                    // M - Γ
                    int workIndex = betaIndex - calcBetaCnt * 2;
                    betaX = pi / latticeA - workIndex * (pi / latticeA) / calcBetaCnt;
                    betaY = betaX;
                }
                 */

                // 長方形格子
                if (betaIndex < calcBetaCnt)
                {
                    // Γ-X
                    int workIndex = betaIndex;
                    betaX = workIndex * (pi / periodicDistanceX) / calcBetaCnt;
                    betaY = 0.0;
                }
                else if (betaIndex < 2.0 * calcBetaCnt)
                {
                    // X -M
                    int workIndex = betaIndex - calcBetaCnt;
                    betaX = pi / periodicDistanceX;
                    betaY = workIndex * (pi / periodicDistanceY) / calcBetaCnt;
                }
                else
                {
                    // M - Γ
                    int workIndex = betaIndex - calcBetaCnt * 2;
                    betaX = pi / periodicDistanceX - workIndex * (pi / periodicDistanceX) / calcBetaCnt;
                    betaY = pi / periodicDistanceY - workIndex * (pi / periodicDistanceY) / calcBetaCnt;
                }
            }
            else
            {
                // 三角形格子
                if (betaIndex < calcBetaCnt)
                {
                    // Γ-K
                    int workIndex = betaIndex;
                    double kVec = workIndex * (pi / latticeA) / calcBetaCnt;
                    betaX = kVec * (4.0 / 3.0);
                    betaY = 0.0;
                }
                else if (betaIndex < 2.0 * calcBetaCnt)
                {
                    // K -M
                    int workIndex = betaIndex - calcBetaCnt;
                    double kVec = workIndex * (pi / latticeA) / calcBetaCnt;
                    betaX = kVec * (1.0 - 4.0 / 3.0) + (4.0 / 3.0) * pi / latticeA;
                    betaY = kVec * Math.Sqrt(3.0) / 3.0;
                }
                else
                {
                    // M - Γ
                    int workIndex = betaIndex - calcBetaCnt * 2;
                    double kVec = workIndex * (pi / latticeA) / calcBetaCnt;
                    betaX = kVec * (-1.0) + pi / latticeA;
                    betaY = kVec * (-Math.Sqrt(3.0) / 3.0) + (Math.Sqrt(3.0) / 3.0) * pi / latticeA;
                }

            }
        }

        /// <summary>
        /// フォトニックバンドギャップを計算する
        /// </summary>
        /// <param name="EigenValueList"></param>
        /// <param name="minFreq"></param>
        /// <param name="maxFreq"></param>
        /// <param name="gapMinFreq"></param>
        /// <param name="gapMaxFreq"></param>
        private static void getPBG(
            IList<Complex[]> EigenValueList,
            double minFreq,
            double maxFreq,
            out double gapMinFreq,
            out double gapMaxFreq)
        {
            gapMinFreq = 0.0;
            gapMaxFreq = 0.0;
            IList<double> freqList = new List<double>();
            foreach (Complex[] values in EigenValueList)
            {
                foreach (Complex value in values)
                {
                    freqList.Add(value.Real);
                }
            }
            double[] freqAry = freqList.ToArray();
            Array.Sort(freqAry);
            double maxGap = double.MinValue;
            for (int i = 1; i < freqAry.Length; i++)
            {
                if (freqAry[i - 1] < minFreq)
                {
                    continue;
                }
                if (freqAry[i] > maxFreq)
                {
                    break;
                }
                double gap = freqAry[i] - freqAry[i - 1];
                if (gap > maxGap)
                {
                    gapMinFreq = freqAry[i - 1];
                    gapMaxFreq = freqAry[i];
                    maxGap = gap;
                }
            }
        }

        /// <summary>
        /// ２つのベクトル間距離を取得する
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private static double getDistance(double[] v1, double[] v2)
        {
            return CVector2D.Distance2D(v1, v2);
        }

        /// <summary>
        /// 伝搬定数の計算結果表示
        /// </summary>
        private void drawEigenResults(int probNo)
        {
            if (EigenValueList.Count > 0)
            {
                int dispBetaIndex = EigenValueList.Count - 1;
                // 規格化伝搬定数
                double betaX = 0.0;
                double betaY = 0.0;
                getBeta(
                    ref dispBetaIndex,
                    IsTriLattice,
                    CalcBetaCnt,
                    LatticeA,
                    PeriodicDistanceX,
                    PeriodicDistanceY,
                    ref betaX,
                    ref betaY);
                int graphPartCnt = 3;
                double[][] betaSt = new double[graphPartCnt][];
                double[][] betaEnd = new double[graphPartCnt][];
                double[] betaDistance = new double[graphPartCnt];
                for (int i = 0; i < graphPartCnt; i++)
                {
                    int workBetaIndex1 = i * CalcBetaCnt;
                    int workBetaIndex2 = (i + 1) * CalcBetaCnt;
                    double workBetaX = 0.0;
                    double workBetaY = 0.0;
                    getBeta(
                        ref workBetaIndex1,
                        IsTriLattice,
                        CalcBetaCnt,
                        LatticeA,
                        PeriodicDistanceX,
                        PeriodicDistanceY,
                        ref workBetaX,
                        ref workBetaY);
                    betaSt[i] = new double[] { workBetaX, workBetaY };
                    getBeta(
                        ref workBetaIndex2,
                        IsTriLattice,
                        CalcBetaCnt,
                        LatticeA,
                        PeriodicDistanceX,
                        PeriodicDistanceY,
                        ref workBetaX,
                        ref workBetaY);
                    betaEnd[i] = new double[] {workBetaX, workBetaY};
                    betaDistance[i] = getDistance(betaEnd[i], betaSt[i]);
                }

                // 伝搬定数を表示
                drawString(10, 45, 0.0, 0.7, 1.0, string.Format("betaX:{0:F4} betaY:{1:F4}",
                    betaX * LatticeA / (2.0 * pi),
                    betaY * LatticeA / (2.0 * pi)));
                //    ));
                // フォトニックバンドギャップの取得
                double gapMinFreq = 0.0;
                double gapMaxFreq = 0.0;
                getPBG(EigenValueList, MinNormalizedFreq, MaxNormalizedFreq, out gapMinFreq, out gapMaxFreq);
                // 固有周波数を表示
                drawString(10, 60, 1.0, 0.7, 0.0, string.Format("PBG(a/lambda): {0:F4} - {1:F4}", gapMinFreq, gapMaxFreq));

                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                // 伝搬定数の周波数特性データをグラフ表示用バッファに格納
                int dataCnt = EigenValueList.Count;
                int axisYCnt = (int)MaxModeCnt;
                double[] valueX = new double[dataCnt];
                IList<double[]> valueYs = new List<double[]>();
                for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                {
                    double[] valueY = new double[dataCnt];
                    valueYs.Add(valueY);
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    int workBetaIndex = i;
                    double workBetaX = 0.0;
                    double workBetaY = 0.0;
                    getBeta(
                        ref workBetaIndex,
                        IsTriLattice,
                        CalcBetaCnt,
                        LatticeA,
                        PeriodicDistanceX,
                        PeriodicDistanceY,
                        ref workBetaX,
                        ref workBetaY);
                    double[] workBeta = new double[] {workBetaX, workBetaY};
                    double xx = 0.0;
                    if (i < CalcBetaCnt)
                    {
                        xx = getDistance(workBeta, betaSt[0]);
                    }
                    else if (i < CalcBetaCnt * 2)
                    {
                        xx = getDistance(workBeta, betaSt[1]);
                        xx += betaDistance[0];
                    }
                    else
                    {
                        xx = getDistance(workBeta, betaSt[2]);
                        xx += betaDistance[0] + betaDistance[1];
                    }
                    valueX[i] = xx * LatticeA / (2.0 * pi);
                    for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                    {
                        valueYs[axisYIndex][i] = EigenValueList[i][axisYIndex].Real;
                    }
                }

                // 周波数特性グラフの描画
                double graphWidth = 500 * win_w / (double)DefWinWidth;
                double graphHeight = 280 * win_w / (double)DefWinWidth;
                double graphX = 45;
                double graphY = 0;
                graphY = win_h - graphHeight - 20 - 20;
                double minXValue = 0.0;
                double maxXValue = (betaDistance[0] + betaDistance[1] + betaDistance[2]) * LatticeA / (2.0 * pi);
                double minYValue = MinNormalizedFreq;
                double maxYValue = MaxNormalizedFreq;
                double intervalXValue = 0.1;
                double intervalYValue = GraphFreqInterval;
                IList<string> valuesYTitles = null;
                valuesYTitles = new List<string>() { "a/lambda" };
                IList<KeyValuePair<double, string>> labelXAxisValues = new List<KeyValuePair<double, string>>();
                if (!IsTriLattice)
                {
                    // 正方格子
                    labelXAxisValues.Add(new KeyValuePair<double, string>(0.0, "G"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        betaDistance[0] * LatticeA / (2.0 * pi), "X"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        (betaDistance[0] + betaDistance[1]) * LatticeA / (2.0 * pi), "M"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        (betaDistance[0] + betaDistance[1] + betaDistance[2]) * LatticeA / (2.0 * pi), "G"));
                }
                else
                {
                    // 三角形格子
                    labelXAxisValues.Add(new KeyValuePair<double, string>(0.0, "G"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        betaDistance[0] * LatticeA / (2.0 * pi), "K"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        (betaDistance[0] + betaDistance[1]) * LatticeA / (2.0 * pi), "M"));
                    labelXAxisValues.Add(new KeyValuePair<double, string>(
                        (betaDistance[0] + betaDistance[1] + betaDistance[2]) * LatticeA / (2.0 * pi), "G"));
                }
                drawGraph(
                    graphX, graphY, graphWidth, graphHeight,
                    minXValue, maxXValue, minYValue, maxYValue,
                    intervalXValue, intervalYValue,
                    valueX, valueYs,
                    valuesYTitles,
                    labelXAxisValues,
                    gapMinFreq,
                    gapMaxFreq
                    );
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
            IList<string> valueYTitles,
            IList<KeyValuePair<double, string>> labelXAxisValues,
            double gapMinValue,
            double gapMaxValue)
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

            // フォトニックバンドギャップ領域を塗りつぶす
            double gap_yymax = graphAreaY + graphAreaHeight - (gapMaxValue - minYValue) * scaleY;
            double gap_yymin = graphAreaY + graphAreaHeight - (gapMinValue - minYValue) * scaleY;
            Gl.glColor3d(0.5, 0.5, 0.5);
            Gl.glBegin(Gl.GL_POLYGON);
            // LeftTop
            Gl.glVertex2d(xxmin, gap_yymax);
            // LeftBottom
            Gl.glVertex2d(xxmin, gap_yymin);
            // RightBottom
            Gl.glVertex2d(xxmax, gap_yymin);
            // RightTop
            Gl.glVertex2d(xxmax, gap_yymax);
            Gl.glEnd();

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
            double[,] graphLineColors = new double[1, 3]
            {
                {0.0, 0.0, 1.0},//{1.0, 0.5, 0.0},
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
            /*
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
                drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F1}", x));
            }
             */
            for (int i = 1; i < labelXAxisValues.Count; i++)
            {
                KeyValuePair<double, string> pair1 = labelXAxisValues[i - 1];
                KeyValuePair<double, string> pair2 = labelXAxisValues[i];
                int nDivX = (int)Math.Floor((pair2.Key - pair1.Key) / intervalXValue) + 1;
                for (int iDivX = 1; iDivX < nDivX; iDivX++)
                {
                    Gl.glColor3d(lineColor[0], lineColor[1], lineColor[2]);
                    Gl.glLineWidth(1.0f);
                    Gl.glBegin(Gl.GL_LINES);
                    double xx = graphAreaX + pair1.Key * scaleX + intervalXValue * iDivX * scaleX;
                    Gl.glVertex2d(xx, yymin);
                    Gl.glVertex2d(xx, yymax);
                    Gl.glEnd();
                }
            }
            for(int i = 0; i <labelXAxisValues.Count; i++)
            {
                KeyValuePair<double, string> pair = labelXAxisValues[i];
                double tmp_valueX = pair.Key;
                string tmp_label = pair.Value;
                double xx = graphAreaX + tmp_valueX * scaleX;
                Gl.glColor3d(0.0, 1.0, 0.5);
                Gl.glLineWidth(2.0f);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(xx, yymin);
                Gl.glVertex2d(xx, yymax);
                Gl.glEnd();
                //double x = tmp_valueX;
                drawString((int)(xx - 5), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], tmp_label);
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

            /*
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
             */

            // データのプロット
            int dataCnt = valueX.Length;
            for (int iAxisY = 0; iAxisY < valueYs.Count; iAxisY++)
            {
                double[] valueY = valueYs[iAxisY];
                Gl.glColor3d(graphLineColors[0, 0], graphLineColors[0, 1], graphLineColors[0, 2]);
                Gl.glPointSize(3.0f);
                Gl.glBegin(Gl.GL_POINTS);
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
