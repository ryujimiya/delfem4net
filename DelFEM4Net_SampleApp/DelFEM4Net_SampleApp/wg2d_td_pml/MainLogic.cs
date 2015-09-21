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

namespace wg2d_td_pml
{
    ///////////////////////////////////////////////////////////////
    //  DelFEM4Net サンプル
    //  導波管伝達問題 2D Time domain PML
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
            ref double WaveguideWidth,
            ref bool isPCWaveguide,
            ref double latticeA,
            ref int timeLoopCnt,
            ref double timeDelta,
            ref double gaussianT0,
            ref double gaussianTp,
            ref double NormalizedFreqSrc,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double GraphFreqInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
            ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPmlLoopIdList,
            ref IList<bool> IsPmlYDirectionList,
            ref IList<double> PmlStPosXList,
            ref IList<double> PmlLengthList,
            ref uint FieldPortSrcBcId,
            ref IList<uint> VIdRefList,
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
        private const int DefProbNo = 1;//0;
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
        /// 問題番号
        /// </summary>
        private int ProbNo = 0;
        /// <summary>
        /// 直線導波管を解いている？
        /// </summary>
        private bool IsSolveStraightWg = false;
        /// <summary>
        /// Newmarkのβ法
        /// </summary>
        double NewmarkBeta = 0.25;
        /// <summary>
        /// 時刻ステップ数
        /// </summary>
        private int TimeLoopCnt = 0;
        /// <summary>
        /// 計算時刻インデックス
        /// </summary>
        private int TimeIndex = 0;
        /// <summary>
        /// 時間ステップ幅
        /// </summary>
        private double TimeDelta = 0.0;
        /// <summary>
        /// ガウシアンパルスの遅延時間
        /// </summary>
        private double GaussianT0 = 0.0;
        /// <summary>
        /// ガウシアンパルスの時間幅
        /// </summary>
        private double GaussianTp = 0.0;
        /// <summary>
        /// 励振源のモード分布の規格化周波数
        /// </summary>
        private double NormalizedFreqSrc = 1.5;
        /// <summary>
        /// 開始規格化周波数
        /// </summary>
        private double NormalizedFreq1 = 1.0;
        /// <summary>
        /// 終了規格化周波数
        /// </summary>
        private double NormalizedFreq2 = 2.0;
        /// <summary>
        /// グラフの周波数目盛間隔
        /// </summary>
        private double GraphFreqInterval = 0.2;

        /// <summary>
        /// 導波管の幅
        /// </summary>
        private double WaveguideWidth = 3.0e-3 * 20 * 4;
        //private double WaveguideWidth = 2.0;
        /// <summary>
        /// フォトニック結晶導波路?
        /// </summary>
        private bool IsPCWaveguide = false;
        /// <summary>
        /// 格子定数（フォトニック結晶導波路)
        /// </summary>
        private double LatticeA = 0.0;
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
        /// PML領域のループのフィールドID(ポート1、ポート2)
        /// </summary>
        private IList<uint> FieldPmlLoopIdList = new List<uint>();
        /// <summary>
        /// Y方向PML?
        /// </summary>
        private IList<bool> IsPmlYDirectionList = new List<bool>();
        /// <summary>
        /// PML領域の開始位置(X座標) (ポート1、ポート2)
        /// </summary>
        private IList<double> PmlStPosXList = new List<double>();
        /// <summary>
        /// PML長さ(ポート1、ポート2)
        /// </summary>
        private IList<double> PmlLengthList = new List<double>();
        /// <summary>
        /// 励振位置の境界フィールドID
        /// </summary>
        private uint FieldPortSrcBcId = 0;
        /// <summary>
        /// 観測点の辺ID(ワールド座標系)
        /// </summary>
        private IList<uint> VIdRefList = new List<uint>();

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
        /// <summary>
        /// 界の絶対値を表示する？
        /// </summary>
        private bool IsShowAbsField = false;
        /// <summary>
        /// グラフをすべて表示する 
        /// </summary>
        private bool IsShowAllGraph = false;

        /// <summary>
        /// 全節点数(強制境界を含む)
        /// </summary>
        private int NodeCnt = 0;
        /// <summary>
        ///  ソート済み節点番号リスト(強制境界を除外)
        /// </summary>
        private IList<uint> SortedNodes = new List<uint>();
        /// <summary>
        /// 節点番号→ソート済み節点インデックスマップ
        /// </summary>
        private Dictionary<uint, uint> ToSorted = new Dictionary<uint, uint>();
        /// <summary>
        /// 境界のソート済み節点番号リスト(励振面)
        /// </summary>
        private IList<uint> SortedNodes_PortSrc = null;
        /// <summary>
        /// 境界の節点番号→ソート済み節点インデックスマップ(励振面)
        /// </summary>
        private Dictionary<uint, uint> ToSorted_PortSrc = null;
        /// <summary>
        /// 剛性行列
        /// </summary>
        private MyDoubleBandMatrix KMat = null;
        /// <summary>
        /// 質量行列
        /// </summary>
        private MyDoubleBandMatrix MMat = null;
        /// <summary>
        /// 境界質量行列(励振面)  p∫NiNj dy
        /// </summary>
        private double[] QbMatSrc = null;
        /// <summary>
        /// 境界の界の伝搬定数(励振面)
        /// </summary>
        private double BetaXSrc = 0.0;
        /// <summary>
        /// 境界の界のモード分布(励振面)
        /// </summary>
        private double[] ProfileSrc = null;

        /// <summary>
        /// 全体係数行列
        /// </summary>
        private double[] AMat = null;
        /// <summary>
        /// 電界（現在値)
        /// </summary>
        private double[] Ez = null;
        /// <summary>
        /// 電界(1つ前)
        /// </summary>
        private double[] Ez_Prev = null;
        /// <summary>
        /// 電界(2つ前)
        /// </summary>
        private double[] Ez_Prev2 = null;
        /// <summary>
        ///  v = ∫u dt の1つ前の値 (階層: ポート - 要素アレイ - 要素アレイ内要素 - 要素節点)
        /// </summary>
        private double[][][][] W2_F_e_List = null;
        /// <summary>
        /// Φ = -σx/ε0 exp(-σx t /ε0)u(t) ★ Ezの１つ前の値 (階層: ポート - 要素アレイ - 要素アレイ内要素 - 要素節点)
        ///   ★:convolution
        /// </summary>
        private double[][][][] W2_G_e_List = null;
        /// <summary>
        /// 観測点1の電界(時間変化リスト)
        /// </summary>
        private IList<double> EzTimeAryPort1Inc = new List<double>();
        /// <summary>
        /// 観測点1の電界(時間変化リスト)
        /// </summary>
        private IList<double> EzTimeAryPort1 = new List<double>();
        /// <summary>
        /// 観測点2の電界(時間変化リスト)
        /// </summary>
        private IList<double> EzTimeAryPort2 = new List<double>();
        /// <summary>
        /// 周波数リスト
        /// </summary>
        private IList<double> FreqList = new List<double>();
        /// <summary>
        /// 散乱パラメータ(各周波数に対するS11とS21)のリスト
        /// </summary>
        private IList<IList<double[]>> ScatterVecList = new List<IList<double[]>>();

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
                runTimerProc();
                IsTimerProcRun = false;
            };
            MyTimer.Interval = 1000 / 120;
            //MyTimer.Interval = 1000 / 60;
            //MyTimer.Interval = 1000 / 10;
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
            TimeIndex = 0;
            IsSolveStraightWg = true; // 最初に直線導波管を解く
            // 問題を作成
            setProblem();
            IsInitedCamera = false;

            IsTimerProcRun = false;
            MyTimer.Enabled = true;

            // Enter main loop
            Glut.glutMainLoop();
        }

        /// <summary>
        /// タイマー処理
        /// </summary>
        private void runTimerProc()
        {
            if (IsAnimation && TimeIndex != -1 && !IsCadShow)
            {
                int curTimeIndex = TimeIndex;
                if (curTimeIndex == 0)
                {
                    // 初回の計算時
                    IsShowAllGraph = false;

                    // 特別処理
                    if (ProbNo == 0)
                    {
                        IsSolveStraightWg = false;
                        EzTimeAryPort1Inc.Clear();
                    }

                    // 散乱行列の初期化
                    ScatterVecList.Clear();
                    if (IsSolveStraightWg)
                    {
                        EzTimeAryPort1Inc.Clear();
                        EzTimeAryPort1.Clear();
                        EzTimeAryPort2.Clear();
                    }
                    else
                    {
                        EzTimeAryPort1.Clear();
                        EzTimeAryPort2.Clear();
                    }

                    // 特別処理
                    if (ProbNo == 0)
                    {
                        IsSolveStraightWg = false;
                        EzTimeAryPort1Inc.Clear();
                    }
                }

                // 問題を解く
                bool ret = solveProblem(
                    (IsInitedCamera ? false : true)
                    );
                if (TimeIndex != -1)
                {
                    TimeIndex++;
                }
                if (ret)
                {
                    if (!IsInitedCamera)
                    {
                        IsInitedCamera = true;
                    }
                }

                // 特別処理
                if (ProbNo == 0)
                {
                    EzTimeAryPort1Inc.Clear();
                    if (EzTimeAryPort1.Count > 0)
                    {
                        foreach (double fVal in EzTimeAryPort1)
                        {
                            EzTimeAryPort1Inc.Add(fVal);//コピー
                        }
                    }
                }

                if (curTimeIndex == (TimeLoopCnt - 1))
                {
                    // 計算の最後のステップが終了したとき
                    if (IsSolveStraightWg)
                    {
                        // 不連続導波管の問題を設定する
                        TimeIndex = 0;
                        IsSolveStraightWg = false; // 不連続導波管
                        IsInitedCamera = false;
                        setProblem();
                    }
                    else
                    {
                        // Sパラメータを計算する
                        calcSParameter();
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
                if (!IsShowAllGraph)
                {
                    DrawerAry.Draw();
                }
                if (TimeIndex != -1 || IsShowAllGraph)
                {
                    drawEzTimeAryResults();
                }
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
            else if (key == getSingleByte('g'))
            {
                IsShowAllGraph = !IsShowAllGraph;
                Console.WriteLine("IsShowAllGraph: {0}", IsShowAllGraph);
                System.Diagnostics.Debug.WriteLine("IsShowAllGraph: {0}", IsShowAllGraph);
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
                TimeIndex = -1;
                ScatterVecList.Clear();
                // 問題を作成
                IsSolveStraightWg = true; //直線導波管
                setProblem();
                if (!IsCadShow)
                {
                    IsInitedCamera = false;
                }
                TimeIndex = 0;
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
                false,// 不連続問題
                ref WaveguideWidth,
                ref IsPCWaveguide,
                ref LatticeA,
                ref TimeLoopCnt,
                ref TimeDelta,
                ref GaussianT0,
                ref GaussianTp,
                ref NormalizedFreqSrc,
                ref NormalizedFreq1,
                ref NormalizedFreq2,
                ref GraphFreqInterval,
                ref WaveModeDv,
                ref World,
                ref FieldValId,
                ref FieldLoopId,
                ref FieldForceBcId,
                ref FieldPmlLoopIdList,
                ref IsPmlYDirectionList,
                ref PmlStPosXList,
                ref PmlLengthList,
                ref FieldPortSrcBcId,
                ref VIdRefList,
                ref Medias,
                ref LoopDic,
                ref EdgeDic,
                ref IsCadShow,
                ref CadDrawerAry,
                ref Camera
                );

            if (!IsCadShow && ret && IsSolveStraightWg)
            {
                // 直線導波管の場合
                // 計算回数等の条件は不連続問題の方を使用する
                int workTimeLoopCnt = 0;
                double workTimeDelta = 0.0;
                double workGaussianT0 = 0.0;
                double workGaussianTp = 0.0;
                double workNormalizedFreqSrc = 0.0;
                double workNormalizedFreq1 = 0.0;
                double workNormalizedFreq2 = 0.0;
                double workGraphFreqInterval = 0.0;
                ret = setProblem(
                    ProbNo,
                    true, // 直線導波管
                    ref WaveguideWidth,
                    ref IsPCWaveguide,
                    ref LatticeA,
                    ref workTimeLoopCnt,
                    ref workTimeDelta,
                    ref workGaussianT0,
                    ref workGaussianTp,
                    ref workNormalizedFreqSrc,
                    ref workNormalizedFreq1,
                    ref workNormalizedFreq2,
                    ref workGraphFreqInterval,
                    ref WaveModeDv,
                    ref World,
                    ref FieldValId,
                    ref FieldLoopId,
                    ref FieldForceBcId,
                    ref FieldPmlLoopIdList,
                    ref IsPmlYDirectionList,
                    ref PmlStPosXList,
                    ref PmlLengthList,
                    ref FieldPortSrcBcId,
                    ref VIdRefList,
                    ref Medias,
                    ref LoopDic,
                    ref EdgeDic,
                    ref IsCadShow,
                    ref CadDrawerAry,
                    ref Camera
                    );
            }

            return ret;
        }

        /// <summary>
        /// 問題を設定する
        /// </summary>
        /// <param name="probNo">問題番号</param>
        /// <param name="IsSolveStraightWg">直線導波路を解く？</param>
        /// <param name="WaveguideWidth">導波路幅</param>
        /// <param name="timeLoopCnt">時刻計算ステップ回数</param>
        /// <param name="timeDelta">時刻刻み幅</param>
        /// <param name="gaussianT0">ガウシアンパルス遅延時間</param>
        /// <param name="gaussianTp">ガウシアンパルス時間幅</param>
        /// <param name="NormalizedFreqSrc">励振源規格化周波数</param>
        /// <param name="NormalizedFreq1">開始規格化周波数</param>
        /// <param name="NormalizedFreq2">終了規格化周波数</param>
        /// <param name="GraphFreqInterval">グラフの周波数目盛幅</param>
        /// <param name="WaveModeDv">波のモード区分</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldValId">値のフィールドID</param>
        /// <param name="FieldLoopId">ループのフィールドID</param>
        /// <param name="FieldForceBcId">強制境界のフィールドID</param>
        /// <param name="FieldPmlLoopIdList">PML領域のループのフィールドID</param>
        /// <param name="PmlStPosXList">PML領域開始位置X座標リスト</param>
        /// <param name="PmlLengthList">PML長さリスト</param>
        /// <param name="FieldPortSrcBcId">励振面のフィールドID</param>
        /// <param name="VIdRefList">観測点頂点IDリスト</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="isCadShow">図面表示する？</param>
        /// <param name="CadDrawerAry">図面表示用描画オブジェクトアレイ</param>
        /// <param name="Camera">カメラ</param>
        /// <returns></returns>
        private static bool setProblem(
            int probNo,
            bool IsSolveStraightWg,
            ref double WaveguideWidth,
            ref bool isPCWaveguide,
            ref double latticeA,
            ref int timeLoopCnt,
            ref double timeDelta,
            ref double gaussianT0,
            ref double gaussianTp,
            ref double NormalizedFreqSrc,
            ref double NormalizedFreq1,
            ref double NormalizedFreq2,
            ref double GraphFreqInterval,
            ref WgUtil.WaveModeDV WaveModeDv,
             ref CFieldWorld World,
            ref uint FieldValId,
            ref uint FieldLoopId,
            ref uint FieldForceBcId,
            ref IList<uint> FieldPmlLoopIdList,
            ref IList<bool> IsPmlYDirectionList,
            ref IList<double> PmlStPosXList,
            ref IList<double> PmlLengthList,
            ref uint FieldPortSrcBcId,
            ref IList<uint> VIdRefList,
            ref IList<MediaInfo> Medias,
            ref Dictionary<uint, wg2d.World.Loop> LoopDic,
            ref Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref bool isCadShow,
            ref CDrawerArray CadDrawerAry,
            ref CCamera Camera
            )
        {
            bool success = false;

            WaveguideWidth = 0.0;
            isPCWaveguide = false;
            latticeA = 0.0;

            timeLoopCnt = 0;
            timeDelta = 0.0;
            gaussianT0 = 0.0;
            gaussianTp = 0.0;

            FieldValId = 0;
            FieldLoopId = 0;
            FieldForceBcId = 0;
            FieldPmlLoopIdList.Clear();
            IsPmlYDirectionList.Clear();
            PmlStPosXList.Clear();
            PmlLengthList.Clear();
            FieldPortSrcBcId = 0;
            VIdRefList.Clear();

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

                    // 誘電体導波路直線
                    //func = Problem03_0.SetProblem;

                    // フォトニック結晶導波路 直線
                    //func = Problem04_0.SetProblem;
                }
                else if (probNo == 1)
                {
                    if (IsSolveStraightWg)
                    {
                        // 直線導波管
                        func = Problem00.SetProblem;
                    }
                    else
                    {
                        // 計算対象導波管
                        // 直角コーナーベンド
                        func = Problem01.SetProblem;
                    }
                }
                else if (probNo == 2)
                {
                    if (IsSolveStraightWg)
                    {
                        // 直線導波管
                        func = Problem00.SetProblem;
                    }
                    else
                    {
                        // 計算対象導波管
                        // 誘電体装荷共振器
                        func = Problem02.SetProblem;
                    }
                }
                else if (probNo == 3)
                {
                    if (IsSolveStraightWg)
                    {
                        // 誘電体導波路直線
                        func = Problem03_0.SetProblem;
                    }
                    else
                    {
                        // 誘電体導波路グレーティング
                        func = Problem03.SetProblem;
                    }
                }
                else if (probNo == 4)
                {
                    if (IsSolveStraightWg)
                    {
                        // フォトニック結晶導波路直線
                        func = Problem04_0.SetProblem;
                    }
                    else
                    {
                        // フォトニック結晶導波路ベンド
                        func = Problem04.SetProblem;
                    }
                }
                else if (probNo == 5)
                {
                    if (IsSolveStraightWg)
                    {
                        // フォトニック結晶導波路直線
                        func = Problem04_0.SetProblem;
                    }
                    else
                    {
                        // フォトニック結晶導波路共振器
                        func = Problem05.SetProblem;
                    }
                }
                else
                {
                    return success;
                }

                success = func(
                    probNo,
                    ref WaveguideWidth,
                    ref isPCWaveguide,
                    ref latticeA,
                    ref timeLoopCnt,
                    ref timeDelta,
                    ref gaussianT0,
                    ref gaussianTp,
                    ref NormalizedFreqSrc,
                    ref NormalizedFreq1,
                    ref NormalizedFreq2,
                    ref GraphFreqInterval,
                    ref WaveModeDv,
                    ref World,
                    ref FieldValId,
                    ref FieldLoopId,
                    ref FieldForceBcId,
                    ref FieldPmlLoopIdList,
                    ref IsPmlYDirectionList,
                    ref PmlStPosXList,
                    ref PmlLengthList,
                    ref FieldPortSrcBcId,
                    ref VIdRefList,
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
        ///  FEM行列の作成
        /// </summary>
        /// <returns></returns>
        private bool mkFEMMat()
        {
            bool success = false;

            IList<double> tagtEzTimeAryPort1 = null;
            IList<double> tagtEzTimeAryPort2 = null;
            if (IsSolveStraightWg)
            {
                tagtEzTimeAryPort1 = EzTimeAryPort1Inc;
                tagtEzTimeAryPort2 = null;
            }
            else
            {
                tagtEzTimeAryPort1 = EzTimeAryPort1;
                tagtEzTimeAryPort2 = EzTimeAryPort2;
            }

            // FEM行列の作成
            success = mkFEMMat(
                ProbNo,
                NewmarkBeta,
                TimeDelta,
                NormalizedFreqSrc,
                WaveguideWidth,
                IsPCWaveguide,
                LatticeA,
                WaveModeDv,
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPmlLoopIdList,
                IsPmlYDirectionList,
                PmlStPosXList,
                PmlLengthList,
                FieldPortSrcBcId,
                VIdRefList,
                Medias,
                LoopDic,
                EdgeDic,
                ref NodeCnt,
                ref SortedNodes,
                ref ToSorted,
                ref SortedNodes_PortSrc,
                ref ToSorted_PortSrc,
                ref KMat,
                ref MMat,
                ref QbMatSrc,
                ref BetaXSrc,
                ref ProfileSrc,
                ref AMat,
                ref Ez,
                ref Ez_Prev,
                ref Ez_Prev2,
                ref W2_F_e_List,
                ref W2_G_e_List,
                ref tagtEzTimeAryPort1,
                ref tagtEzTimeAryPort2
                );
            
            return success;
        }

        /// <summary>
        /// FEM行列の作成
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="newmarkBeta"></param>
        /// <param name="timeDelta"></param>
        /// <param name="NormalizedFreqSrc"></param>
        /// <param name="WaveguideWidth"></param>
        /// <param name="WaveModeDv"></param>
        /// <param name="World"></param>
        /// <param name="FieldValId"></param>
        /// <param name="FieldLoopId"></param>
        /// <param name="FieldForceBcId"></param>
        /// <param name="FieldPmlLoopIdList"></param>
        /// <param name="PmlStPosXList"></param>
        /// <param name="PmlLengthList"></param>
        /// <param name="FieldPortSrcBcId"></param>
        /// <param name="VIdRefList"></param>
        /// <param name="Medias"></param>
        /// <param name="LoopDic"></param>
        /// <param name="EdgeDic"></param>
        /// <param name="node_cnt"></param>
        /// <param name="sortedNodes"></param>
        /// <param name="toSorted"></param>
        /// <param name="sortedNodes_PortSrc"></param>
        /// <param name="toSorted_PortSrc"></param>
        /// <param name="KMat"></param>
        /// <param name="MMat"></param>
        /// <param name="QbMatSrc"></param>
        /// <param name="betaXSrc"></param>
        /// <param name="profileSrc"></param>
        /// <param name="AMat"></param>
        /// <param name="Ez"></param>
        /// <param name="Ez_Prev"></param>
        /// <param name="Ez_Prev2"></param>
        /// <param name="EzTimeAryPort1"></param>
        /// <param name="EzTimeAryPort2"></param>
        /// <returns></returns>
        private static bool mkFEMMat(
            int probNo,
            double newmarkBeta,
            double timeDelta,
            double NormalizedFreqSrc,
            double WaveguideWidth,
            bool isPCWaveguide,
            double latticeA,
            WgUtil.WaveModeDV WaveModeDv,
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            IList<uint> FieldPmlLoopIdList,
            IList<bool> IsPmlYDirectionList,
            IList<double> PmlStPosXList,
            IList<double> PmlLengthList,
            uint FieldPortSrcBcId,
            IList<uint> VIdRefList,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            ref int node_cnt,
            ref IList<uint> sortedNodes,
            ref Dictionary<uint, uint> toSorted,
            ref IList<uint> sortedNodes_PortSrc,
            ref Dictionary<uint, uint> toSorted_PortSrc,
            ref MyDoubleBandMatrix KMat,
            ref MyDoubleBandMatrix MMat,
            ref double[] QbMatSrc,
            ref double betaXSrc,
            ref double[] profileSrc,
            ref double[] AMat,
            ref double[] Ez,
            ref double[] Ez_Prev,
            ref double[] Ez_Prev2,
            ref double[][][][] W2_F_e_List,
            ref double[][][][] W2_G_e_List,
            ref IList<double> EzTimeAryPort1,
            ref IList<double> EzTimeAryPort2
            )
        {
            System.Diagnostics.Debug.WriteLine("mkFEMMat");
            bool success = false;
            node_cnt = 0;
            sortedNodes = null;
            toSorted = null;
            sortedNodes_PortSrc = null;
            toSorted_PortSrc = null;
            KMat = null;
            MMat = null;
            QbMatSrc = null;
            betaXSrc = 0.0;
            profileSrc = null;
            AMat = null;
            Ez = null;
            Ez_Prev = null;
            Ez_Prev2 = null;
            W2_F_e_List = null;
            W2_G_e_List = null;
            if (EzTimeAryPort1 != null)
            {
                EzTimeAryPort1.Clear();
            }
            if (EzTimeAryPort2 != null)
            {
                EzTimeAryPort2.Clear();
            }

            try
            {
                // 全節点数を取得する
                //uint node_cnt = 0;
                node_cnt = 0;
                //node_cnt = WgUtilForPeriodicEigen.GetNodeCnt(World, FieldLoopId);
                double[][] coord_c_all = null;
                {
                    uint[] no_c_all_tmp = null;
                    Dictionary<uint, uint> to_no_all_tmp = null;
                    double[][] coord_c_all_tmp = null;
                    WgUtilForTD.GetLoopCoordList(World, FieldLoopId, out no_c_all_tmp, out to_no_all_tmp, out coord_c_all_tmp);
                    node_cnt = no_c_all_tmp.Length;

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
                // ポート数
                int portCnt = FieldPmlLoopIdList.Count; 
                uint[] no_c_all_fieldPortSrcBcId = null;
                Dictionary<uint, uint> to_no_boundary_fieldPortSrcBcId = null;

                {
                    uint workFieldPortBcId = FieldPortSrcBcId;
                    uint[] work_no_c_all_fieldPortBcId = null;
                    Dictionary<uint, uint> work_to_no_boundary_fieldPortBcId = null;

                    WgUtil.GetBoundaryNodeList(World, workFieldPortBcId, out work_no_c_all_fieldPortBcId, out work_to_no_boundary_fieldPortBcId);

                    no_c_all_fieldPortSrcBcId = work_no_c_all_fieldPortBcId;
                    to_no_boundary_fieldPortSrcBcId = work_to_no_boundary_fieldPortBcId;
                }
                //強制境界を除いた節点
                //IList<IList<uint>> sortedNodes_Port_List = new List<IList<uint>>();
                //IList<Dictionary<uint, uint>> toSorted_Port_List = new List<Dictionary<uint, uint>>();
                sortedNodes_PortSrc = null;
                toSorted_PortSrc = null;
                {
                    uint[] no_c_all_fieldPortBcId = no_c_all_fieldPortSrcBcId;
                    IList<uint> sortedNodes_Port = new List<uint>();
                    Dictionary<uint, uint> toSorted_Port = new Dictionary<uint, uint>();
                    int nodeCntB = no_c_all_fieldPortBcId.Length;
                    for (int ino = 0; ino < nodeCntB; ino++)
                    {
                        uint nodeNumber = no_c_all_fieldPortBcId[ino];
                        if (FieldForceBcId != 0)
                        {
                            // 強制境界を除く
                            if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumber)) continue;
                        }
                        sortedNodes_Port.Add(nodeNumber);
                        toSorted_Port.Add(nodeNumber, (uint)(sortedNodes_Port.Count - 1));
                    }
                    sortedNodes_PortSrc = sortedNodes_Port;
                    toSorted_PortSrc = toSorted_Port;
                }

                // 節点のソート
                //IList<uint> sortedNodes = new List<uint>();
                //Dictionary<uint, int> toSorted = new Dictionary<uint, int>();
                sortedNodes = new List<uint>();
                toSorted = new Dictionary<uint, uint>();
                for (uint nodeNumber = 0; nodeNumber < node_cnt; nodeNumber++)
                {
                    if (FieldForceBcId != 0)
                    {
                        // 強制境界を除く
                        if (to_no_boundary_fieldForceBcId.ContainsKey(nodeNumber)) continue;
                    }
                    sortedNodes.Add(nodeNumber);
                    toSorted.Add(nodeNumber, (uint)(sortedNodes.Count - 1));
                }
                uint free_node_cnt = (uint)sortedNodes.Count;

                //------------------------------------------------------
                // バンド行列パターンを作成する
                //------------------------------------------------------
                // 非０パターンを作成する
                bool[,] matPattern = null;
                WgUtilForTD.MkMatPattern(
                    World,
                    FieldLoopId,
                    Medias,
                    LoopDic,
                    (uint)node_cnt,
                    free_node_cnt,
                    toSorted,
                    out matPattern);
                /*
                int rowColSize = 0;
                int subdiaSize = 0;
                int superdiaSize = 0;
                WgUtilForTD.GetBandMatrixSubDiaSizeAndSuperDiaSize(
                    matPattern,
                    out rowColSize,
                    out subdiaSize,
                    out superdiaSize
                    );
                System.Diagnostics.Debug.Assert(rowColSize == free_node_cnt);
                 */
                // バンド行列のバンド幅を縮小する
                IList<uint> optNodesGlobal = null;
                Dictionary<uint, uint> toOptNodes = null;
                WgUtilForTD.GetOptBandMatNodes(
                    matPattern,
                    sortedNodes,
                    out optNodesGlobal,
                    out toOptNodes);
                bool[,] matPatternOpt = new bool[free_node_cnt, free_node_cnt];
                for (int i = 0; i < free_node_cnt; i++)
                {
                    uint inoGlobal = sortedNodes[i];
                    uint inoOpt = toOptNodes[inoGlobal];
                    for (int j = 0; j < free_node_cnt; j++)
                    {
                        uint jnoGlobal = sortedNodes[j];
                        uint jnoOpt = toOptNodes[jnoGlobal];
                        matPatternOpt[inoOpt, jnoOpt] = matPattern[i, j];
                    }
                }
                int rowColSizeOpt = 0;
                int subdiaSizeOpt = 0;
                int superdiaSizeOpt = 0;
                WgUtilForTD.GetBandMatrixSubDiaSizeAndSuperDiaSize(
                    matPatternOpt,
                    out rowColSizeOpt,
                    out subdiaSizeOpt,
                    out superdiaSizeOpt
                    );
                System.Diagnostics.Debug.WriteLine("rowColSizeOpt : {0}", rowColSizeOpt);
                System.Diagnostics.Debug.WriteLine("subdiaSizeOpt : {0}", subdiaSizeOpt);
                System.Diagnostics.Debug.WriteLine("superdiaSizeOpt : {0}", superdiaSizeOpt);
                // ソート済み節点を最適化したものに更新
                sortedNodes = optNodesGlobal;
                toSorted = toOptNodes;

                WgUtil.GC_Collect();

                //------------------------------------------------------
                // 剛性行列、質量行列を作成
                //------------------------------------------------------
                WgUtilForTD.MkHelmholtzMat(
                    World,
                    FieldLoopId,
                    Medias,
                    LoopDic,
                    (uint)node_cnt,
                    free_node_cnt,
                    toSorted,
                    subdiaSizeOpt,
                    superdiaSizeOpt,
                    out KMat,
                    out MMat);

                //------------------------------------------------------
                // モード分布計算
                //------------------------------------------------------
                // 波数
                double k0Src = NormalizedFreqSrc * pi / WaveguideWidth;
                // 波長
                double waveLengthSrc = 2.0 * pi / k0Src;
                // 周波数
                double freqSrc = c0 / waveLengthSrc;
                // 角周波数
                double omegaSrc = 2.0 * pi * freqSrc;

                bool retPort = false;

                // 境界の界に導波路開口条件を追加
                {
                    uint workFieldPortBcId = FieldPortSrcBcId;
                    double[,] ryy_1d_port1 = null;
                    double[,] txx_1d_port1 = null;
                    double[,] uzz_1d_port1 = null;
                    Complex[] eigen_values_port1 = null;
                    Complex[,] eigen_vecs_port1 = null;
                    retPort = WgUtil.SolvePortWaveguideEigen(
                        waveLengthSrc,
                        WaveModeDv,
                        World,
                        workFieldPortBcId,
                        no_c_all_fieldForceBcId,
                        Medias,
                        EdgeDic,
                        out ryy_1d_port1,
                        out txx_1d_port1,
                        out uzz_1d_port1,
                        out eigen_values_port1,
                        out eigen_vecs_port1);

                    // 境界節点数 (強制境界を含む)
                    uint[] no_c_all_fieldPortBcId = no_c_all_fieldPortSrcBcId;
                    int nodeCntB = no_c_all_fieldPortBcId.Length;
                    Dictionary<uint, uint> toSorted_Port = toSorted_PortSrc;
                    // 強制境界を除いた節点数
                    int nodeCntB_f = toSorted_Port.Count;

                    // 境界質量行列
                    double[] ryy_1d_port1_Buffer = MyMatrixUtil.matrix_ToBuffer(ryy_1d_port1);
                    double[] txx_1d_port1_Buffer = MyMatrixUtil.matrix_ToBuffer(txx_1d_port1);
                    double[] uzz_1d_port1_Buffer = MyMatrixUtil.matrix_ToBuffer(uzz_1d_port1);
                    double[] ryy_1d_port1_Buffer_f = new double[nodeCntB_f * nodeCntB_f];
                    double[] txx_1d_port1_Buffer_f = new double[nodeCntB_f * nodeCntB_f];
                    double[] uzz_1d_port1_Buffer_f = new double[nodeCntB_f * nodeCntB_f];
                    for (int ino = 0; ino < nodeCntB; ino++)
                    {
                        uint ino_global = no_c_all_fieldPortBcId[ino];
                        if (!toSorted_Port.ContainsKey(ino_global))
                        {
                            continue;
                        }
                        uint ino_f = toSorted_Port[ino_global];
                        for (int jno = 0; jno < nodeCntB; jno++)
                        {
                            uint jno_global = no_c_all_fieldPortBcId[jno];
                            if (!toSorted_Port.ContainsKey(jno_global))
                            {
                                continue;
                            }
                            uint jno_f = toSorted_Port[jno_global];
                            ryy_1d_port1_Buffer_f[ino_f + nodeCntB_f * jno_f] = ryy_1d_port1_Buffer[ino + nodeCntB * jno];
                            txx_1d_port1_Buffer_f[ino_f + nodeCntB_f * jno_f] = txx_1d_port1_Buffer[ino + nodeCntB * jno];
                            uzz_1d_port1_Buffer_f[ino_f + nodeCntB_f * jno_f] = uzz_1d_port1_Buffer[ino + nodeCntB * jno];
                        }
                    }
                    QbMatSrc = ryy_1d_port1_Buffer_f;

                    // 基本モード
                    uint imode = 0;
                    if (isPCWaveguide)
                    {
                        /*
                        // TEST
                        System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!! search DBR mode");
                        Console.WriteLine("!!!!!!!!!!!!!!! search DBR mode");
                        for (int imode_tmp = 0; imode_tmp < eigen_values_port1.Length; imode_tmp++)
                        {
                            if (Math.Abs(eigen_values_port1[imode_tmp].Real) >= Constants.PrecisionLowerLimit
                                && Math.Abs(eigen_values_port1[imode_tmp].Real / k0Src) < 1.0)
                            {
                                imode = (uint)imode_tmp;
                                Console.WriteLine("DBR mode: imode = {0}", imode);
                                System.Diagnostics.Debug.WriteLine("DBR mode: imode = {0}", imode);
                                break;
                            }
                        }
                         */
                    }
                    Complex betam = eigen_values_port1[imode];
                    Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigen_vecs_port1, (int)imode); // 強制境界を含む
                    // 実数部を取得する
                    double betamReal = betam.Real;
                    double[] fmVecReal_f = new double[nodeCntB_f]; // 強制境界を除く
                    for (int ino = 0; ino < fmVec.Length; ino++)
                    {
                        uint ino_global = no_c_all_fieldPortBcId[ino];
                        if (!toSorted_Port.ContainsKey(ino_global))
                        {
                            continue;
                        }
                        uint ino_f = toSorted_Port[ino_global];

                        fmVecReal_f[ino_f] = fmVec[ino].Real;
                        //System.Diagnostics.Debug.WriteLine("{0}", fmVecReal_f[ino_f]);
                    }
                    betaXSrc = betamReal;
                    profileSrc = fmVecReal_f;
                    if (Math.Abs(betamReal) < Constants.PrecisionLowerLimit)
                    {
                        return false;
                    }
                }

                /////////////////////////////////////////////////////////

                //------------------------------------------------------
                // 電界
                //-----------------------------------------------------
                uint free_node_cnt_all = free_node_cnt;
                Ez = new double[free_node_cnt_all];
                Ez_Prev = new double[free_node_cnt_all];
                Ez_Prev2 = new double[free_node_cnt_all];
                W2_F_e_List = new double[portCnt][][][];
                W2_G_e_List = new double[portCnt][][][];

                //------------------------------------------------------
                // 全体係数行列の作成
                //------------------------------------------------------
                MyDoubleBandMatrix AMatB = new MyDoubleBandMatrix((int)free_node_cnt_all, subdiaSizeOpt, superdiaSizeOpt);
                double dt = timeDelta;
                for (int bufferIndex = 0; bufferIndex < AMatB._body.Length; bufferIndex++)
                {
                    AMatB._body[bufferIndex] =
                        (1.0 / (dt * dt)) * MMat._body[bufferIndex]
                        + newmarkBeta * KMat._body[bufferIndex];
                }
                
                // PML領域内のみの係数行列を加算する
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    uint fieldPmlLoopId = FieldPmlLoopIdList[portIndex];
                    bool isPmlYDirection = IsPmlYDirectionList[portIndex];
                    double pmlStPosX = PmlStPosXList[portIndex];
                    double pmlLength = PmlLengthList[portIndex];
                    WgUtilForTDPml.AddPmlMat(
                        World,
                        fieldPmlLoopId,
                        isPmlYDirection,
                        pmlStPosX,
                        pmlLength,
                        dt,
                        newmarkBeta,
                        Medias,
                        LoopDic,
                        (uint)node_cnt,
                        free_node_cnt,
                        toSorted,
                        ref AMatB
                        );
                }

                // 逆行列を計算
                System.Diagnostics.Debug.WriteLine("calc [A]-1");
                Console.Write("calc [A]-1 ....");
                AMat = MyMatrixUtil.matrix_Inverse_NoCopy(AMatB);
                Console.WriteLine(" done");

                WgUtil.GC_Collect();

                success = true;

            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                success = false;
            }
            System.Diagnostics.Debug.WriteLine("mkFEMMat done ret: {0}", success);
            System.Diagnostics.Debug.WriteLine("dt:{0}", timeDelta);
            return success;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <param name="initFlg">カメラ初期化フラグ</param>
        /// <returns></returns>
        private bool solveProblem(bool initFlg)
        {
            // FEM行列の作成
            bool retMkMat = false;
            if (TimeIndex == -1)
            {
                return false;
            }
            if (TimeIndex == 0)
            {
                // FEM行列の作成
                retMkMat = mkFEMMat();
            }
            else
            {
                retMkMat = true;
            }
            if (!retMkMat)
            {
                TimeIndex = -1;
                return false;
            }
            IList<double> tagtEzTimeAryPort1 = null;
            IList<double> tagtEzTimeAryPort2 = null;
            if (IsSolveStraightWg)
            {
                tagtEzTimeAryPort1 = EzTimeAryPort1Inc;
                tagtEzTimeAryPort2 = null;
            }
            else
            {
                tagtEzTimeAryPort1 = EzTimeAryPort1;
                tagtEzTimeAryPort2 = EzTimeAryPort2;
            }

            // 問題を解く
            bool ret = solveProblem(
                ProbNo,
                IsSolveStraightWg,
                ref TimeIndex,
                TimeLoopCnt,
                NewmarkBeta,
                TimeDelta,
                GaussianT0,
                GaussianTp,
                NormalizedFreqSrc,
                NormalizedFreq1,
                NormalizedFreq2,
                initFlg,
                WaveguideWidth,
                IsPCWaveguide,
                LatticeA,
                WaveModeDv,
                ref World,
                FieldValId,
                FieldLoopId,
                FieldForceBcId,
                FieldPmlLoopIdList,
                IsPmlYDirectionList,
                PmlStPosXList,
                PmlLengthList,
                FieldPortSrcBcId,
                VIdRefList,
                Medias,
                LoopDic,
                EdgeDic,
                IsShowAbsField,
                NodeCnt,
                SortedNodes,
                ToSorted,
                SortedNodes_PortSrc,
                ToSorted_PortSrc,
                KMat,
                MMat,
                QbMatSrc,
                BetaXSrc,
                ProfileSrc,
                AMat,
                ref Ez,
                ref Ez_Prev,
                ref Ez_Prev2,
                ref W2_F_e_List,
                ref W2_G_e_List,
                ref tagtEzTimeAryPort1,
                ref tagtEzTimeAryPort2,
                ref DrawerAry,
                Camera
                );

            return ret;
        }

        /// <summary>
        /// 問題を解く
        /// </summary>
        /// <param name="probNo">問題番号</param>
        /// <param name="timeIndex">時刻のインデックス</param>
        /// <param name="timeLoopCnt">時間計算回数</param>
        /// <param name="newmarkBeta">Newmarkβ法のβ</param>
        /// <param name="timeDelta">時刻刻み</param>
        /// <param name="gaussianT0">ガウシアンパルスの遅延時間</param>
        /// <param name="gaussianTp">ガウシアンパルスの時間幅</param>
        /// <param name="NormalizedFreqSrc">励振するモードの規格化周波数</param>
        /// <param name="VIdRefList">観測点の頂点ID(= 節点番号 + 1)</param>
        /// <param name="NormalizedFreq1">開始規格化周波数</param>
        /// <param name="NormalizedFreq2">終了規格化周波数</param>
        /// <param name="initFlg">カメラ初期化フラグ</param>
        /// <param name="WaveguideWidth">導波路幅</param>
        /// <param name="WaveModeDv">波のモード区分</param>
        /// <param name="World">ワールド座標系</param>
        /// <param name="FieldValId">値のフィールドID</param>
        /// <param name="FieldLoopId">ループのフィールドID</param>
        /// <param name="FieldForceBcId">強制境界のフィールドID</param>
        /// <param name="FieldPortBcIdList">ポート１、ポート２、励振面の境界のフィールドID</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="LoopDic">ループID→ループ情報マップ</param>
        /// <param name="EdgeDic">エッジID→エッジ情報マップ</param>
        /// <param name="IsShowAbsField">絶対値表示する？</param>
        /// <param name="node_cnt">節点数(強制境界を含む)</param>
        /// <param name="sortedNodes">ソート済み節点番号リスト</param>
        /// <param name="toSorted">節点番号→ソート済み節点インデックスマップ</param>
        /// <param name="sortedNodes_Port_List">境界のソート済み節点番号リスト(ポート1,ポート2,励振面)</param>
        /// <param name="toSorted_Port_List">境界の節点番号→ソート済み節点インデックスマップ(ポート1,ポート2,励振面)</param>
        /// <param name="KMat">剛性行列</param>
        /// <param name="MMat">質量行列</param>
        /// <param name="QbMatSrc">境界質量行列(励振面)</param>
        /// <param name="betaXSrc">境界の伝搬定数(励振面)</param>
        /// <param name="profileSrc">境界のモード分布(励振面)</param>
        /// <param name="AMat">係数行列（逆行列)</param>
        /// <param name="Ez">電界分布(現在)</param>
        /// <param name="Ez_Prev">電界分布(1つ前)</param>
        /// <param name="Ez_Prev2">電界分布(2つ前)</param>
        /// <param name="EzTimeAryPort1">ポート1観測点の電界リスト(時間変化)</param>
        /// <param name="EzTimeAryPort2">ポート2観測点の電界リスト(時間変化)</param>
        /// <param name="DrawerAry">描画オブジェクトアレイ</param>
        /// <param name="Camera">カメラ</param>
        /// <returns></returns>
        private static bool solveProblem(
            int probNo,
            bool IsSolveStraightWg,
            ref int timeIndex,
            int timeLoopCnt,
            double newmarkBeta,
            double timeDelta,
            double gaussianT0,
            double gaussianTp,
            double NormalizedFreqSrc,
            double NormalizedFreq1,
            double NormalizedFreq2,
            bool initFlg,
            double WaveguideWidth,
            bool isPCWaveguide,
            double latticeA,
            WgUtil.WaveModeDV WaveModeDv,
            ref CFieldWorld World,
            uint FieldValId,
            uint FieldLoopId,
            uint FieldForceBcId,
            IList<uint> FieldPmlLoopIdList,
            IList<bool> IsPmlYDirectionList,
            IList<double> PmlStPosXList,
            IList<double> PmlLengthList,
            uint FieldPortSrcBcId,
            IList<uint> VIdRefList,
            IList<MediaInfo> Medias,
            Dictionary<uint, wg2d.World.Loop> LoopDic,
            Dictionary<uint, wg2d.World.Edge> EdgeDic,
            bool IsShowAbsField,
            int node_cnt,
            IList<uint> sortedNodes,
            Dictionary<uint, uint> toSorted,
            IList<uint> sortedNodes_PortSrc,
            Dictionary<uint, uint> toSorted_PortSrc,
            MyDoubleBandMatrix KMat,
            MyDoubleBandMatrix MMat,
            double[] QbMatSrc,
            double betaXSrc,
            double[] profileSrc,
            double[] AMat,
            ref double[] Ez,
            ref double[] Ez_Prev,
            ref double[] Ez_Prev2,
            ref double[][][][] W2_F_e_List,
            ref double[][][][] W2_G_e_List,
            ref IList<double> EzTimeAryPort1,
            ref IList<double> EzTimeAryPort2,
            ref CDrawerArrayField DrawerAry,
            CCamera Camera)
        {
            //long memorySize1 = GC.GetTotalMemory(false);
            //Console.WriteLine("    total memory: {0}", memorySize1);

            bool success = false;

            // ポート数
            int portCnt = FieldPmlLoopIdList.Count;

            // 時刻の取得
            if (timeIndex == -1)
            {
                return success;
            }
            if (timeIndex >= timeLoopCnt)
            {
                timeIndex = -1;
                return success;
            }
            // 以下curTimeIndexに対する計算
            double curTime = timeIndex * timeDelta;

            double dt = timeDelta;

            try
            {
                // ワールド座標系のフィールド値をクリア
                WgUtil.ClearFieldValues(World, FieldValId);

                // 励振源のパラメータ
                // 波数
                double k0Src = NormalizedFreqSrc * pi / WaveguideWidth;
                // 波長
                double waveLengthSrc = 2.0 * pi / k0Src;
                // 周波数
                double freqSrc = c0 / waveLengthSrc;
                // 角周波数
                double omegaSrc = 2.0 * pi * freqSrc;

                //--------------------------------------------------------------
                // 電界
                //--------------------------------------------------------------
                uint free_node_cnt = (uint)sortedNodes.Count;
                uint free_node_cnt_all = free_node_cnt;
                System.Diagnostics.Debug.Assert(Ez.Length == free_node_cnt_all);

                Ez_Prev.CopyTo(Ez_Prev2, 0);
                Ez.CopyTo(Ez_Prev, 0);
                for (int ino = 0; ino < free_node_cnt_all; ino++)
                {
                    Ez[ino] = 0.0;
                }

                //--------------------------------------------------------------
                // 残差
                //--------------------------------------------------------------
                double[] vec_MMat = new double[free_node_cnt];
                double[] vec_KMat = new double[free_node_cnt];
                for (int ino = 0; ino < free_node_cnt; ino++)
                {
                    vec_MMat[ino] = (2.0 / (dt * dt)) * Ez_Prev[ino] - (1.0 / (dt * dt)) * Ez_Prev2[ino];
                    vec_KMat[ino] = -(1.0 - 2.0 * newmarkBeta) * Ez_Prev[ino] - newmarkBeta * Ez_Prev2[ino];
                }
                vec_MMat = MyMatrixUtil.product(
                    MMat,
                    vec_MMat, (int)free_node_cnt);
                vec_KMat = MyMatrixUtil.product(
                    KMat,
                    vec_KMat, (int)free_node_cnt);

                double[] resVec = MyMatrixUtil.plus(vec_MMat, vec_KMat);
                System.Diagnostics.Debug.Assert(resVec.Length == free_node_cnt_all);

                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    uint workFieldPmlLoopId =FieldPmlLoopIdList[portIndex];
                    bool workIsPmlYDirection = IsPmlYDirectionList[portIndex];
                    double workPmlPosX = PmlStPosXList[portIndex];
                    double workPmlLength = PmlLengthList[portIndex];
                    double[][][] work_w2_F_e_list = W2_F_e_List[portIndex];
                    double[][][] work_w2_G_e_list = W2_G_e_List[portIndex];
                    WgUtilForTDPml.AddPmlResVec(
                        timeIndex,
                        World,
                        workFieldPmlLoopId,
                        workIsPmlYDirection,
                        workPmlPosX,
                        workPmlLength,
                        dt,
                        newmarkBeta,
                        Medias,
                        LoopDic,
                        (uint)node_cnt,
                        free_node_cnt,
                        toSorted,
                        Ez_Prev,
                        Ez_Prev2,
                        ref work_w2_F_e_list,
                        ref work_w2_G_e_list,
                        ref resVec
                        );
                    // 再格納
                    W2_F_e_List[portIndex] = work_w2_F_e_list;
                    W2_G_e_List[portIndex] = work_w2_G_e_list;
                }

                //--------------------------------------------------------------
                // 励振源
                //--------------------------------------------------------------
                {
                    int portIndex = portCnt; // ポートリストの最後の要素が励振境界
                    // 境界の節点番号リスト
                    IList<uint> sortedNodes_Port = sortedNodes_PortSrc;
                    // 境界の節点数(強制境界を含まない)
                    int nodeCntB_f = sortedNodes_Port.Count;
                    double[] QbMat = QbMatSrc;

                    if (isPCWaveguide)
                    {
                        
                        //////////
                        //TEST 中央に点波源を置く
                        profileSrc = new double[profileSrc.Length];
                        profileSrc[profileSrc.Length / 2] = 1.0;
                         

                        /*
                        ///////////
                        // TEST: 欠陥部を平面波励振する
                        //betaXSrc = k0Src;
                        profileSrc = new double[profileSrc.Length];
                        for (int i = 0; i < 5; i++)
                        {
                            profileSrc[profileSrc.Length / 2 - 2 + i] = 1.0;
                        }
                         */
                    }

                    double srcF1 = 0.0;
                    double srcF2 = 0.0;
                    double srcF0 = 0.0;

                    int n = timeIndex;
                    // ガウシアンパルス
                    if ((n * dt) <= (2.0 * gaussianT0 + dt))
                    {
                        /*
                        // ガウシアンパルス
                        srcF1 = Math.Exp(-1.0 * ((n + 1) * dt - gaussianT0) * ((n + 1) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                        srcF2 = Math.Exp(-1.0 * ((n - 1) * dt - gaussianT0) * ((n - 1) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                        srcF0 = Math.Exp(-1.0 * ((n) * dt - gaussianT0) * ((n) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                         */
                        
                        
                        // 正弦波変調ガウシアンパルス
                        srcF1 = Math.Cos(omegaSrc * ((n + 1) * dt - gaussianT0))
                                * Math.Exp(-1.0 * ((n + 1) * dt - gaussianT0) * ((n + 1) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                        srcF2 = Math.Cos(omegaSrc * ((n - 1) * dt - gaussianT0))
                                * Math.Exp(-1.0 * ((n - 1) * dt - gaussianT0) * ((n - 1) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                        srcF0 = Math.Cos(omegaSrc * ((n) * dt - gaussianT0))
                                * Math.Exp(-1.0 * ((n) * dt - gaussianT0) * ((n) * dt - gaussianT0) / (2.0 * gaussianTp * gaussianTp));
                         
                    }

                    /*
                    // 正弦波
                    srcF1 = Math.Sin(omegaSrc * (n + 1) * dt);
                    srcF2 = Math.Sin(omegaSrc * (n - 1) * dt);
                    srcF0 = Math.Sin(omegaSrc * (n) * dt);
                     */

                    
                    // 境界積分
                    double[] srcdFdt = new double[nodeCntB_f];
                    for (int ino = 0; ino < nodeCntB_f; ino++)
                    {
                        double normalizeFactor = -1.0;
                        srcdFdt[ino] = normalizeFactor * profileSrc[ino] * (srcF1 - srcF2) / (2.0 * dt);
                    }
                    double vpx = omegaSrc / betaXSrc;
                    double[] vec_QbMat = MyMatrixUtil.product((-2.0 / vpx), srcdFdt);
                    vec_QbMat = MyMatrixUtil.product_native(
                        QbMat, nodeCntB_f, nodeCntB_f,
                        vec_QbMat, nodeCntB_f);
                    for (int ino = 0; ino < nodeCntB_f; ino++)
                    {
                        uint ino_global = sortedNodes_Port[ino];
                        if (!toSorted.ContainsKey(ino_global))
                        {
                            // 強制境界は除外済み
                            System.Diagnostics.Debug.Assert(false);
                            continue;
                        }
                        int ino_f = (int)toSorted[ino_global];
                        resVec[ino_f] += vec_QbMat[ino];
                    }
                     
                    
                    /*
                    // 領域積分
                    double[] srcdFdt2_f = new double[free_node_cnt];
                    double[] srcF_f = new double[free_node_cnt];
                    for (int ino = 0; ino < nodeCntB_f; ino++)
                    {
                        uint ino_global = sortedNodes_Port[ino];
                        if (!toSorted.ContainsKey(ino_global))
                        {
                            // 強制境界は除外済み
                            System.Diagnostics.Debug.Assert(false);
                            continue;
                        }
                        int ino_f = (int)toSorted[ino_global];
                        double normalizeFactor = -1.0;
                        //double normalizeFactor = -omegaSrc * myu0 / betaXSrc;
                        srcF_f[ino_f] = normalizeFactor * profileSrc[ino] * srcF0;
                        srcdFdt2_f[ino_f] = normalizeFactor * profileSrc[ino] * (srcF1 - 2.0 * srcF0 + srcF2) / (dt * dt);
                    }
                    double[] vec_MMatSrc = MyMatrixUtil.product_native(
                        MMat, (int)free_node_cnt, (int)free_node_cnt,
                        srcdFdt2_f, (int)free_node_cnt
                        );
                    double[] vec_KMatSrc = MyMatrixUtil.product_native(
                        KMat, (int)free_node_cnt, (int)free_node_cnt,
                        srcF_f, (int)free_node_cnt
                        );
                    for (int ino = 0; ino < free_node_cnt; ino++)
                    {
                        resVec[ino] += vec_MMatSrc[ino] + vec_KMatSrc[ino];
                    }
                     */
                }

                //------------------------------------------------------------------
                // Ezを求める
                //------------------------------------------------------------------
                /*
                // 連立方程式を解く
                {
                    int matLen = (int)free_node_cnt_all;
                    double[] A = new double[matLen * matLen];
                    AMat.CopyTo(A, 0); // コピーを取る
                    double[] B = resVec;

                    double[] X = null;
                    int x_row = matLen;
                    int x_col = 1;
                    int a_row = matLen;
                    int a_col = matLen;
                    int b_row = matLen;
                    int b_col = 1;
                    KrdLab.clapack.Function.dgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col);

                    X.CopyTo(Ez, 0);
                }
                 */
                
                // 逆行列を用いる
                Ez = MyMatrixUtil.product_native(
                    AMat, (int)free_node_cnt_all, (int)free_node_cnt_all,
                    resVec, (int)free_node_cnt_all
                    );
                 
                // 観測点
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {
                    // 頂点IDから1を引いたものが節点番号
                    uint nodeNumber = VIdRefList[portIndex] - 1;
                    int ino_f = (int)toSorted[nodeNumber];
                    double fVal = Ez[ino_f];
                    if (portIndex == 0)
                    {
                        if (EzTimeAryPort1 != null)
                        {
                            EzTimeAryPort1.Add(fVal);
                        }
                    }
                    else if (portIndex == 1)
                    {
                        if (EzTimeAryPort2 != null)
                        {
                            EzTimeAryPort2.Add(fVal);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                
                // 解ベクトルをワールド座標系にセット
                WgUtil.SetFieldValueForDisplay(World, FieldValId, Ez, toSorted);

                if ((timeIndex + 1) % 50 == 0)
                {
                    Console.WriteLine("{0}", (timeIndex + 1));
                }

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
                    setupPanAndScale(probNo, IsSolveStraightWg, Camera);
                }
                success = true;

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
            }
            return success;
        }

        /// <summary>
        /// パンとスケールの設定
        /// </summary>
        /// <param name="probNo"></param>
        /// <param name="Camera"></param>
        private static void setupPanAndScale(int probNo, bool IsSolveStraightWg, CCamera Camera)
        {
            if (probNo == 0 || IsSolveStraightWg)
            {
                Camera.MousePan(0, 0, -0.1, 0.50);
                double tmp_scale = 0.7;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 1)
            {
                Camera.MousePan(0, 0, -0.1, 0.50);
                double tmp_scale = 0.7;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 3)
            {
                Camera.MousePan(0, 0, -0.1, 0.50);
                double tmp_scale = 0.7;
                Camera.SetScale(tmp_scale);
            }
            else if (probNo == 4)
            {
                Camera.MousePan(0, 0, -0.1, 0.50);
                double tmp_scale = 0.7;
                Camera.SetScale(tmp_scale);
            }
            else
            {
                Camera.MousePan(0, 0, -0.1, 0.50);
                double tmp_scale = 1.0;
                Camera.SetScale(tmp_scale);
            }
        }

        /// <summary>
        /// Sパラメータの計算
        /// </summary>
        /// <returns></returns>
        private bool calcSParameter()
        {
            bool success = false;

            int dataCnt = EzTimeAryPort1.Count;
            double dt = TimeDelta;

            double[] timeAry = new double[dataCnt];
            for (int i = 0; i < dataCnt; i++)
            {
                timeAry[i] = i * dt;
            }

            // 散乱パラメータを計算する
            double[] workWaveAryPort1Inc = EzTimeAryPort1Inc.ToArray();
            double[] workWaveAryPort1 = EzTimeAryPort1.ToArray();
            double[] workWaveAryPort2 = EzTimeAryPort2.ToArray();
            double[] freqAry = null;
            double[] S11Ary = null;
            double[] S21Ary = null;
            WgUtilForTD.CalcSParameter(
                timeAry,
                workWaveAryPort1Inc,
                workWaveAryPort1,
                workWaveAryPort2,
                out freqAry,
                out S11Ary,
                out S21Ary
                );

            // ポート数
            int portCnt = FieldPmlLoopIdList.Count;
            int maxModeCnt = 1;
            FreqList.Clear();
            ScatterVecList.Clear();
            for (int i = 0; i < dataCnt; i++)
            {
                IList<double[]> scatterVec = new List<double[]>();
                for (int portIndex = 0; portIndex < portCnt; portIndex++)
                {

                    double val = 0;
                    if (portIndex == 0)
                    {
                        val = S11Ary[i];
                    }
                    else if (portIndex == 1)
                    {
                        val = S21Ary[i];
                    }

                    double[] port_scatterVec = new double[maxModeCnt];
                    port_scatterVec[0] = val;

                    scatterVec.Add(port_scatterVec);
                }
                // 格納
                // 周波数
                double freq = freqAry[i];
                // 波長
                double waveLength = c0 /freq;
                // 波数
                double k0 = 2.0 * pi / waveLength;
                // 規格化周波数
                double normalizedFreq = k0 * WaveguideWidth / pi;

                FreqList.Add(normalizedFreq);
                ScatterVecList.Add(scatterVec);
            }

            success = true;
            return success;
        }

        /// <summary>
        /// 電界時間変化計算結果表示
        /// </summary>
        private void drawEzTimeAryResults()
        {
            // ポート数
            int portCnt = FieldPmlLoopIdList.Count;
            double dt = TimeDelta;

            if (TimeIndex != -1)
            {
                // 計算ステップ回数を表示
                drawString(10, 45, 0.0, 0.0, 1.0, string.Format("time step: {0}", (TimeIndex + 1)));
            }

            if (EzTimeAryPort1Inc.Count > 0)
            {
                int axisYCnt = 1 + portCnt; // 入射波 + ポート反射(透過)波
                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                int dataCnt = EzTimeAryPort1Inc.Count;
                double[] valueX = new double[dataCnt];
                IList<double[]> valueYs = new List<double[]>();
                for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                {
                    double[] valueY = new double[dataCnt];
                    valueYs.Add(valueY);
                }
                double maxfVal = double.MinValue;
                for (int i = 0; i < dataCnt; i++)
                {
                    // 時刻(横軸)
                    //valueX[i] = i * dt;
                    valueX[i] = i;

                    // 電界(縦軸)
                    for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                    {
                        double fVal = 0.0;
                        if (axisYIndex == 0)
                        {
                            if (i < EzTimeAryPort1Inc.Count)
                            {
                                fVal = EzTimeAryPort1Inc[i];
                            }
                        }
                        else if (axisYIndex == 1)
                        {
                            if (i < EzTimeAryPort1.Count)
                            {
                                fVal = EzTimeAryPort1[i];
                            }
                        }
                        else if (axisYIndex == 2)
                        {
                            if (i < EzTimeAryPort2.Count)
                            {
                                fVal = EzTimeAryPort2[i];
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                        }
                        valueYs[axisYIndex][i] = fVal;
                        if (Math.Abs(fVal) > maxfVal)
                        {
                            maxfVal = Math.Abs(fVal); 
                        }
                    }
                }

                // グラフの描画
                //double graphWidth = 200 * win_w / (double)DefWinWidth;
                //double graphHeight = 100 * win_w / (double)DefWinWidth;
                double graphWidth = 500 * win_w / (double)DefWinWidth;
                double graphHeight = 250 * win_w / (double)DefWinWidth;
                double graphX = 40;
                double graphY = win_h - graphHeight - 20;
                if (IsShowAllGraph)
                {
                    graphY = 100;
                }
                double minXValue = 0;
                double maxXValue = TimeLoopCnt;
                double minYValue = -1.0 * maxfVal;
                double maxYValue = maxfVal;
                double intervalXValue = Math.Floor(TimeLoopCnt / 4.0);
                double intervalYValue = maxfVal / 4;
                IList<string> valuesYTitles = new List<string>();

                for (int axisYIndex = 0;axisYIndex < axisYCnt; axisYIndex++)
                {
                    if (axisYIndex == 0)
                    {
                        valuesYTitles.Add("EzPort1Inc");
                    }
                    else
                    {
                        int portIndex = axisYIndex - 1;
                        valuesYTitles.Add(string.Format("EzPort{0}", (portIndex + 1)));
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
        /// 散乱パラメータの計算結果表示
        /// </summary>
        private void drawScatterResults()
        {
            // ポート数
            int portCnt = FieldPmlLoopIdList.Count;
            int incidentPortNo = 1;
            bool isMaxSTruc = true; // 最大値を指定値に制限する
            const double maxSTrunc = 1.2;

            if (ScatterVecList.Count > 0)
            {
                // ウィンドウの寸法を取得
                int[] viewport = new int[4];
                Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
                int win_w = viewport[2];
                int win_h = viewport[3];

                double freqScale = 1.0;
                double workNormalizedFreq1 = NormalizedFreq1;
                double workNormalizedFreq2 = NormalizedFreq2;
                double workGraphFreqInterval = GraphFreqInterval;
                if (IsPCWaveguide)
                {
                    freqScale = (LatticeA / (2.0 * WaveguideWidth));
                    workNormalizedFreq1 *= freqScale;
                    workNormalizedFreq2 *= freqScale;
                    workGraphFreqInterval *= freqScale;
                }

                // Sパラメータの周波数特性データをグラフ表示用バッファに格納
                int dataCnt = ScatterVecList.Count;
                int axisYCnt = portCnt;
                double[] valueX = new double[dataCnt];
                IList<double[]> valueYs = new List<double[]>();
                double maxSVal = 0.0;
                for (int axisYIndex = 0; axisYIndex < axisYCnt; axisYIndex++)
                {
                    double[] valueY = new double[dataCnt];
                    valueYs.Add(valueY);
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    double normalizedFreq = FreqList[i];
                    if (IsPCWaveguide)
                    {
                        normalizedFreq *= freqScale;
                    }
                    valueX[i] = normalizedFreq;
                    IList<double[]> work_portScatterVecList = ScatterVecList[i];
                    for (int portIndex = 0, axisYIndex = 0; portIndex < portCnt; portIndex++)
                    {
                        double[] scatterVec = work_portScatterVecList[portIndex];
                        for (uint imode = 0; imode < scatterVec.Length; imode++)
                        {
                            double valueY = Complex.Norm(scatterVec[imode]);
                            if (isMaxSTruc && valueY > maxSTrunc)
                            {
                                valueY = maxSTrunc;
                            }
                            valueYs[axisYIndex][i] = valueY;
                            if (valueY > maxSVal)
                            {
                                maxSVal = valueY;
                            }

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
                double minXValue = workNormalizedFreq1;
                double maxXValue = workNormalizedFreq2;
                double minYValue = 0.0;
                double maxYValue = maxSTrunc;
                if (maxYValue < maxSVal)
                {
                    maxYValue = maxSVal;
                }
                double intervalXValue = workGraphFreqInterval;
                double intervalYValue = 0.2;
                IList<string> valuesYTitles = new List<string>();
                string xvalueFormat = "{0:F1}";
                string yvalueFormat = "{0:F1}";
                if (IsPCWaveguide)
                {
                    xvalueFormat = "{0:F3}";
                }
                if (ScatterVecList.Count > 0)
                {
                    IList<double[]> work_portScatterVecList = ScatterVecList[0];
                    for (int portIndex = 0, axisYIndex = 0; portIndex < portCnt; portIndex++)
                    {
                        double[] scatterVec = work_portScatterVecList[portIndex];
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
                    valuesYTitles,
                    xvalueFormat,
                    yvalueFormat);
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
            string xvalueFormat = "{0:F1}",
            string yvalueFormat = "{0:F1}")
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
                //drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F1}", x));
                drawString((int)(xx - 10), (int)(yymax + 15), foreColor[0], foreColor[1], foreColor[2], string.Format(xvalueFormat, x));
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
                //drawString((int)(xxmin - 30), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], string.Format("{0:F1}", y));
                drawString((int)(xxmin - 30), (int)(yy + 3), foreColor[0], foreColor[1], foreColor[2], string.Format(yvalueFormat, y));
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
                double xx = graphAreaX + graphAreaWidth - 90 * maxAxisOneLine + (iAxisY % maxAxisOneLine) * 130;
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
                    if (valueX[i] < minXValue || valueX[i] > maxXValue)
                    {
                        continue;
                    }
                    if (valueY[i] < minYValue || valueY[i] > maxYValue)
                    {
                        continue;
                    }
                    double xx;
                    double yy;
                    xx = graphAreaX + (valueX[i] - minXValue) * scaleX;
                    yy = graphAreaY + graphAreaHeight - (valueY[i] - minYValue) * scaleY;
                    //Gl.glVertex2d(xx, yy);
                    Gl.glVertex3d(xx, yy, 0.0 + i * 1.0e-5); // 後書きを前面にする
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
