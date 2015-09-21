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

namespace solid2d_lowlev
{
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
        private const int ProbCnt = 1;
        /// <summary>
        /// 
        /// </summary>
        private readonly double[] Gravity = new double[]{ 0.0, -10.0 };

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
        private CFieldValueSetter FieldValueSetter = null;
        /// <summary>
        /// 
        /// </summary>
        private uint Id_disp = 0;
        /// <summary>
        /// 
        /// </summary>
        private CLinearSystem_Field Ls = null;
        /// <summary>
        /// 
        /// </summary>
        private DelFEM4NetLsSol.CPreconditioner_ILU Prec = null;

        /// <summary>
        /// 問題番号
        /// </summary>
        private int ProbNo = 0;
        /// <summary>
        /// 計算対象時刻
        /// </summary>
        private double CurTime = 0.0;
        /// <summary>
        /// 計算時刻刻み幅
        /// </summary>
        private double Dt = 0.02;

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
        /// コンストラクタ
        /// </summary>
        public MainLogic()
        {
            Disposed = false;
            Camera = new CCamera();
            DrawerAry = new CDrawerArrayField();
            World = new CFieldWorld();
            FieldValueSetter = new CFieldValueSetter();
            Ls = new CLinearSystem_Field();
            Prec = new CPreconditioner_ILU();

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
            if (FieldValueSetter != null)
            {
                FieldValueSetter.Clear();
                FieldValueSetter.Dispose();
                FieldValueSetter = null;
            }
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
            MyTimer.Enabled = true;

            // Initailze GLUT
            Glut.glutInitWindowPosition(200, 200);
            Glut.glutInitWindowSize(400, 300);
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
            Gl.glClearColor(0.2f, 0.7f, 0.7f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glPolygonOffset(1.1f, 4.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            if (IsAnimation)
            {
                CurTime += Dt;
                //World.FieldValueExec(CurTime);
                Solve();
                DrawerAry.Update(World);
            }

            DrawerAry.Draw();
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
        /// 
        /// </summary>
        private void Solve()
        {
            CVector_Blk velo_pre = null;
            using (CField field = World.GetField(Id_disp))
            {
                using (CField.CNodeSegInNodeAry nodeSeg = field.GetNodeSegInNodeAry(ELSEG_TYPE.CORNER))
                {
                    uint id_ns_v = nodeSeg.id_ns_ve;
                    uint id_na = nodeSeg.id_na_va;
                    CNodeAry na = World.GetNA(id_na);
                    using (CNodeAry.CNodeSeg ns_v = field.GetNodeSeg(ELSEG_TYPE.CORNER, true, World, FIELD_DERIVATION_TYPE.VELOCITY))
                    {
                        uint nblk = ns_v.Size();
                        uint len = ns_v.Length();
                        velo_pre = new CVector_Blk(nblk, len);
                        na.GetValueFromNodeSegment(id_ns_v, velo_pre);
                    }
                }
            }
            for (int itr = 0; itr < 4; itr++)
            {
                double res;
                Ls.InitializeMarge();
                CEqnStVenant.AddLinSys_StVenant2D_NonStatic_BackwardEular(
                    Dt,
                    Ls,
                    0.00, 4000.0,
                    1.0, Gravity[0], Gravity[1],
                    World, Id_disp,
                    velo_pre,
                    (itr == 0));
                res = Ls.FinalizeMarge();

                double convRatio = 1.0e-9;
                uint maxItr = 1000;
                /*
                using (CLinearSystemAccesser lsAccesser = Ls.GetLs())
                {
                    Prec.SetValue(lsAccesser);
                    using (CLinearSystemPreconditioner lsp = new CLinearSystemPreconditioner(lsAccesser, Prec))
                    {
                        //Console.WriteLine("Solve_PCG");
                        CSolverLsIter.Solve_PCG(ref convRatio, ref maxItr, lsp);
                        Console.WriteLine(maxItr + " " + convRatio);
                    }
                }
                 */
                Prec.SetValue(Ls.GetLs());
                using (CLinearSystemPreconditioner lsp = new CLinearSystemPreconditioner(Ls.GetLs(), Prec))
                {
                    //Console.WriteLine("Solve_PCG");
                    CSolverLsIter.Solve_PCG(ref convRatio, ref maxItr, lsp);
                    Console.WriteLine(maxItr + " " + convRatio);
                }
                Ls.UpdateValueOfField_BackwardEular(Dt, Id_disp, World, itr == 0);
                Console.WriteLine("iter : " + itr + " Res : " + res);
                if (res < 1.0e-6)
                {
                    break;
                }
            }
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
                if( ProbNo == 0 )
                {
                    CurTime = 0;
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0,1.0) );
                        pts.Add( new CVector2D(5,1.0) );
                        pts.Add( new CVector2D(5,2.0) );
                        pts.Add( new CVector2D(0,2.0) );
                        cad2d.AddPolygon( pts );
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.2) );
                        conv = World.GetIDConverter(id_base);
                    }
                    ////////////////        
                    Id_disp = World.MakeField_FieldElemDim(id_base, 2,
                        FIELD_TYPE.VECTOR2,
                        FIELD_DERIVATION_TYPE.VALUE | FIELD_DERIVATION_TYPE.VELOCITY | FIELD_DERIVATION_TYPE.ACCELERATION,
                        ELSEG_TYPE.CORNER);
                    uint Id_disp_fix0;
                    { // get fixed field
                        IList<uint> aIdEAFix = new List<uint>();
                        aIdEAFix.Add( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE) );
                        Id_disp_fix0 = World.GetPartialField(Id_disp,aIdEAFix);
                    }
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(Id_disp_fix0, World);
                    FieldValueSetter.SetMathExp("sin(t)",    0, FIELD_DERIVATION_TYPE.VALUE,World);
                    FieldValueSetter.SetMathExp("sin(0.5*t)",1, FIELD_DERIVATION_TYPE.VALUE,World);
                    
                    // set linear system
                    Ls.Clear();
                    Ls.AddPattern_Field(Id_disp,World);
                    Ls.SetFixedBoundaryCondition_Field(Id_disp_fix0,World);
                    // set Preconditioner
                    Prec.SetFillInLevel(0);
                    Prec.SetLinearSystem(Ls.GetLs());
            
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerEdge(Id_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerFace(Id_disp,false,World) );
                    //DrawerAry.PushBack( new CDrawerEdge(Id_disp,true ,World) );
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
