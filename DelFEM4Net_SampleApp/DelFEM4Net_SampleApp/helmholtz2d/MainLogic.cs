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

namespace helmholtz2d
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   ヘルムホルツ2D
    //          Copy Rights (c) Nobuyuki Umetani 2008
    ////////////////////////////////////////////////////////
    // written in C# by ryujimiya (c) 2012
    /// <summary>
    /// メインロジック
    /// </summary>
    class MainLogic : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
        /// <summary>
        /// 問題の数
        /// </summary>
        private const int ProbCnt = 2; // 実質１

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
        /// 問題番号
        /// </summary>
        private int ProbNo = 0;
        /// <summary>
        /// 計算対象時刻
        /// </summary>
        //private double CurTime = 0.0;
        /// <summary>
        /// 計算時刻刻み幅
        /// </summary>
        //private double Dt = 0.02;

        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        //private int Frame = 0;
        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        //private int Timebase = 0;
        /// <summary>
        /// ShowFPSで使用するパラメータ
        /// </summary>
        //private string Stringfps = "";
        /// <summary>
        /// フレーム単位描画用タイマー
        /// </summary>
        private System.Windows.Forms.Timer MyTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainLogic()
        {
            Disposed = false;
            Camera = new CCamera();
            DrawerAry = new CDrawerArrayField();
            World = new CFieldWorld();

            // Glutのアイドル時処理でなく、タイマーで再描画イベントを発生させる
            MyTimer.Tick +=(sender, e) =>
            {
                Glut.glutPostRedisplay();
            };
            MyTimer.Interval = 1000 / 60;
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
            //MyTimer.Enabled = true;

            // Initailze GLUT
            Glut.glutInitWindowPosition(200, 200);
            Glut.glutInitWindowSize(250, 250);
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

            setNewProblem();

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
            Gl.glClearColor(1.0f, 1.0f, 1.0f ,1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glPolygonOffset(1.1f, 4.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            DrawerAry.Draw();
            //DelFEM4NetCom.GlutUtility.ShowFPS(ref Frame, ref Timebase, ref Stringfps);
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
            }
            else if (key == getSingleByte(' '))
            {
                setNewProblem();
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
        private bool setNewProblem()
        {
            bool success = false;
            try
            {
                if (ProbNo == 0 || ProbNo == 1)
                {
                    ////////////////
                    // 波源の頂点
                    uint id_v = 0;
                    // ワールド座標系のベースID
                    uint id_base = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add(new CVector2D(0.0, 0.0));  // 頂点1
                        pts.Add(new CVector2D(2.0, 0.0));  // 頂点2
                        pts.Add(new CVector2D(2.0, 2.0));  // 頂点3
                        pts.Add(new CVector2D(0.0, 2.0));  // 頂点4
                        uint id_l = cad2d.AddPolygon(pts).id_l_add;
                        id_v = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5, 0.05)).id_v_add;
                        World.Clear();
                        id_base = World.AddMesh(new CMesher2D(cad2d, 0.04));
                    }
                    // CADのIDからワールド座標系のIDへ変換するコンバーターを取得
                    CIDConvEAMshCad conv = World.GetIDConverter(id_base);

                    // フィールドを作成する
                    //   図形の次元２次元、値、コーナー（要素の角頂点）
                    uint id_field_val = World.MakeField_FieldElemDim(
                        id_base,
                        2,
                        FIELD_TYPE.ZSCALAR,
                        FIELD_DERIVATION_TYPE.VALUE,
                        ELSEG_TYPE.CORNER);
                    //uint id_field_bc0 = World.GetPartialField(id_field_val,conv.GetIdEA_fromCad(2, 1));
                    uint id_field_bc1 = 0;
                    {
                        IList<uint> aEA = new List<uint>();
                        aEA.Add(conv.GetIdEA_fromCad(1, CAD_ELEM_TYPE.EDGE));
                        aEA.Add(conv.GetIdEA_fromCad(2, CAD_ELEM_TYPE.EDGE));
                        aEA.Add(conv.GetIdEA_fromCad(3, CAD_ELEM_TYPE.EDGE));
                        aEA.Add(conv.GetIdEA_fromCad(4, CAD_ELEM_TYPE.EDGE));
                        id_field_bc1 = World.GetPartialField(id_field_val, aEA);
                    }
                    CFieldValueSetter.SetFieldValue_Constant(id_field_bc1, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0);

                    using (CZLinearSystem ls = new CZLinearSystem())
                    using (CZPreconditioner_ILU prec = new CZPreconditioner_ILU())
                    {
                        // 場のパターンをリニアシステムに追加する
                        ls.AddPattern_Field(id_field_val, World);

                        // 境界条件
                        //ls.SetFixedBoundaryCondition_Field(id_field_bc0,World);
                        //ls.SetFixedBoundaryCondition_Field(id_field_bc1,World);

                        // プリコンディショナ―フィルインあり
                        prec.SetFillInLevel(1);
                        // プリコンディショナ―にリニアシステムをセットする
                        prec.SetLinearSystem(ls);

                        // 波長
                        double wave_length = 0.4;

                        // 全体行列の作成
                        ls.InitializeMarge();

                        if (ProbNo == 0)
                        {
                            Console.WriteLine("///// DelFEM4NetFem.Eqn.CEqnHelmholtz");
                            // DelFEMのライブラリを使用
                            CEqnHelmholz.AddLinSys_Helmholtz(ls, wave_length, World, id_field_val);
                            CEqnHelmholz.AddLinSys_SommerfeltRadiationBC(ls, wave_length, World, id_field_bc1);
                        }
                        else
                        {
                            Console.WriteLine("///// CEqnHelmholtz_ForCSharp");
                            // C#で記述した関数を使用(TEST)
                            System.Diagnostics.Debug.Assert(ProbNo == 1);
                            CEqnHelmholtz_ForCSharp.AddLinSysHelmholtz(ls, wave_length, World, id_field_val);
                            CEqnHelmholtz_ForCSharp.AddLinSys_SommerfeltRadiationBC(ls, wave_length, World, id_field_bc1);
                        }
                        double res = ls.FinalizeMarge();

                        // リニアシステムのマトリクスの値をセットしてILU分解を行う
                        prec.SetValue(ls);

                        // 励振：波源を設定する
                        {
                            // 波源の頂点IDからワールド座標系の要素アレイIDを取得する
                            uint id_ea_v = conv.GetIdEA_fromCad(id_v, CAD_ELEM_TYPE.VERTEX);
                            Console.WriteLine(id_ea_v);
                            // 要素アレイを取得する
                            CElemAry ea = World.GetEA(id_ea_v);
                            // 要素セグメントを取得する
                            CElemAry.CElemSeg es = ea.GetSeg(1);  // セグメントID: 1
                            System.Diagnostics.Debug.Assert(ea.ElemType() == ELEM_TYPE.POINT);
                            // 最初の要素の節点番号を取り出す
                            uint[] noes = new uint[1];
                            es.GetNodes(0, noes); // 要素インデックス: 0
                            Console.WriteLine(noes[0]);

                            // 残差ベクトルのポインタを取得
                            using (CZVector_Blk_Ptr residualPtr = ls.GetResidualPtr(id_field_val, ELSEG_TYPE.CORNER, World))
                            {
                                // 節点番号noes[0]の0番目の自由度に値を加算する
                                residualPtr.AddValue(noes[0], 0, new Complex(1, 0));
                            }
                        }
                        Console.WriteLine("Residual : " + res);
                        {
                            double tol = 1.0e-6;
                            uint iter = 2000;
                            //CZSolverLsIter.Solve_CG(ref tol, ref iter, ls);
                            //CZSolverLsIter.Solve_PCG(ref tol, ref iter,ls, prec);
                            CZSolverLsIter.Solve_PCOCG(ref tol,ref iter,ls, prec);
                            //CZSolverLsIter.Solve_CGNR(ref tol,ref iter, ls);
                            //CZSolverLsIter.Solve_BiCGSTAB(ref tol,ref iter,ls);
                            //CZSolverLsIter.Solve_BiCGStabP(ref tol, ref iter,ls, prec);
                            Console.WriteLine(iter + " " + tol);
                        }
                        ls.UpdateValueOfField(id_field_val, World, FIELD_DERIVATION_TYPE.VALUE);
                    }

                    // 描画オブジェクトアレイの追加
                    DrawerAry.Clear();
                    DrawerAry.PushBack(new CDrawerFace(id_field_val, true, World, id_field_val, -0.05, 0.05));
                    //DrawerAry.PushBack( new CDrawerFaceContour(id_field_ val, World) );
                    DrawerAry.PushBack(new CDrawerEdge(id_field_val, true, World));
                    DrawerAry.InitTrans(Camera);
                }
                success = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            ProbNo++;
            if (ProbNo == ProbCnt) ProbNo = 0;
            return success;
        }
    }
}
