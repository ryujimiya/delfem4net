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

namespace msh_view
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   msh view
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
        private const int ProbCnt = 15;

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
        private int Modifier = 0;
        /// <summary>
        /// 描画オブジェクトアレイ
        /// </summary>
        private CDrawerArray DrawerAry = null;
        /// <summary>
        /// 問題番号
        /// </summary>
        private int ProbNo = 0;

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
            DrawerAry = new CDrawerArray();

            // Glutのアイドル時処理でなく、タイマーで再描画イベントを発生させる
            MyTimer.Tick += (sender, e) =>
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

            Glut.glutInitWindowPosition(200,200);
            Glut.glutInitWindowSize(400, 400);
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            int argc = commandLineArgs.Length;
            StringBuilder[] argv = new StringBuilder[argc];
            for (int i = 0; i < argc; i++)
            {
                argv[i] = new StringBuilder(commandLineArgs[i]);
            }
            Glut.glutInit(ref argc, argv);
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE|Glut.GLUT_RGBA|Glut.GLUT_DEPTH);
            Glut.glutCreateWindow("DelFEM demo");

            // Set call back function
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutSpecialFunc(myGlutSpecial);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            //Glut.glutIdleFunc(myGlutIdle);

            Camera.SetRotationMode(ROTATION_MODE.ROT_3D);

            setNewProblem();

            // Enter Main Loop
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
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );
         
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

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
            if (Modifier == Glut.GLUT_ACTIVE_CTRL)
            {
                Camera.MouseRotation(MovBeginX, MovBeginY, movEndX, movEndY);
            }
            else if (Modifier == Glut.GLUT_ACTIVE_SHIFT)
            {
                Camera.MousePan(MovBeginX, MovBeginY, movEndX, movEndY);
            }
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
            Modifier = Glut.glutGetModifiers();
            if (state == Glut.GLUT_DOWN && state == Glut.GLUT_DOWN)
            {
                int sizeBuffer = 128;
                DelFEM4NetCom.View.DrawerGlUtility.PickSelectBuffer pickSelectBuffer = null;
                DelFEM4NetCom.View.DrawerGlUtility.PickPre((uint)sizeBuffer, out pickSelectBuffer, (uint)x, (uint)y, 5, 5, Camera);
                DrawerAry.DrawSelection();

                List<DelFEM4NetCom.View.SSelectedObject> aSelecObj = (List<DelFEM4NetCom.View.SSelectedObject>)DelFEM4NetCom.View.DrawerGlUtility.PickPost(pickSelectBuffer, (uint)x, (uint)y, Camera);
                if (aSelecObj.Count > 0)
                {
                    DrawerAry.AddSelected(aSelecObj[0].name);
                }
                else
                {
                    DrawerAry.ClearSelected();
                }
            }
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
            else if (key == getSingleByte(' '))
            {
               setNewProblem();
            }
        }

        /// <summary>
        /// 文字をバイトデータへ変換する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static byte getSingleByte(char c)
        {
            Encoding enc = Encoding.ASCII;
            byte[] bytes = enc.GetBytes(new char[]{c});
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
                DrawerAry.InitTrans(Camera);
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
                if( ProbNo == 0 )
                {
                    CMesher2D mesh2d = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.2) );
                        pts.Add( new CVector2D(0.5,0.2) );
                        pts.Add( new CVector2D(0.5,0.8) );
                        pts.Add( new CVector2D(1.0,0.8) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        cad2d.ConnectVertex_Line(6,3);
                        mesh2d = new CMesher2D(cad2d,0.05);
                        ////
                        using(CMesher2D msh_tmp = (CMesher2D)mesh2d.Clone())
                        {
                            mesh2d.Clear();
                            mesh2d.Dispose();
                            mesh2d = (CMesher2D)msh_tmp.Clone();
                        }
                    }
                    using(CSerializer fout = new CSerializer("hoge.txt", false))
                    {
                        // write file
                        mesh2d.Serialize(fout);
                    }
                    using(CSerializer fin = new CSerializer("hoge.txt", true))
                    {    // load file
                        mesh2d.Serialize(fin);
                    }
                    ////
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                    DrawerAry.InitTrans( Camera );
                    mesh2d.Clear();
                    mesh2d.Dispose();
                }
                else if( ProbNo == 1 )
                {
                    string mshfn = "../../../input_file/hexa_tri.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher2D mesh2d = new CMesher2D())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh2d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh2d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 2 )
                {
                    string mshfn = "../../../input_file/rect_quad.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher2D mesh2d = new CMesher2D())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh2d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh2d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 3 )
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.5) );
                        pts.Add( new CVector2D(0.5,0.5) );
                        pts.Add( new CVector2D(0.5,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.1))
                        using(CMesh3D_Extrude mesh3d = new CMesh3D_Extrude())
                        {
                            mesh3d.Extrude(mesh2d, 0.5, 0.1 );
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh3d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh3d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh3D(mesh3d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 4 )    // load mesh of GiD
                {
                    string mshfn = "../../../input_file/hexa_tri.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher2D mesh2d = new CMesher2D())
                        using(CMesh3D_Extrude mesh3d = new CMesh3D_Extrude())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            mesh3d.Extrude(mesh2d, 5.0, 0.5 );
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh3D(mesh3d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 5 )
                {
                    string mshfn = "../../../input_file/cylinder_hex.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher3D mesh3d = new CMesher3D())
                        {
                            mesh3d.ReadFromFile_GiDMsh(mshfn);
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh3d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh3d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh3D(mesh3d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 6 )
                {
                    string mshfn = "../../../input_file/cylinder_tet.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher3D mesh3d = new CMesher3D())
                        {
                            mesh3d.ReadFromFile_GiDMsh(mshfn);
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh3d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh3d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh3D(mesh3d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 7 )
                {
                    string mshfn = "../../../input_file/rect_quad.msh";
                    if (File.Exists(mshfn))
                    {
                        using(CMesher2D mesh2d = new CMesher2D())
                        using(CMesh3D_Extrude mesh3d = new CMesh3D_Extrude())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            mesh3d.Extrude(mesh2d, 5.0, 0.5 );
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh3D(mesh3d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 8 )
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        uint id_l = 0;
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            id_l = cad2d.AddPolygon( pts ).id_l_add;
                        }
                        
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.8,0.6) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.6,0.6) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.4,0.6) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.2,0.6) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.8,0.4) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.6,0.4) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.4,0.4) );
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.2,0.4) );
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.02))
                        {
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh2d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh2d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 9 )
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            cad2d.AddPolygon( pts );
                        }
                        cad2d.SetCurve_Arc(1,true, -0.5);
                        cad2d.SetCurve_Arc(2,false,-0.5);
                        cad2d.SetCurve_Arc(3,true, -0.5);
                        cad2d.SetCurve_Arc(4,false,-0.5);
                        
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.05))
                        {
                            using(CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                // write file
                                mesh2d.Serialize(fout);
                            }
                            using(CSerializer fin = new CSerializer("hoge.txt", true))
                            {    // load file
                                mesh2d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 10 )
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            uint id_l = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.2)).id_v_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.7,0.2)).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.7,0.8)).id_v_add;
                            uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.8)).id_v_add;
                            cad2d.ConnectVertex_Line(id_v1,id_v2);
                            cad2d.ConnectVertex_Line(id_v2,id_v3);
                            cad2d.ConnectVertex_Line(id_v3,id_v4);
                            cad2d.ConnectVertex_Line(id_v4,id_v1);
                        }
                        
                        using(CMesher2D mesh2d = new CMesher2D())
                        {
                            mesh2d.AddIdLCad_CutMesh(1);    // cut mesh to loop whitch have id 1
                            mesh2d.SetMeshingMode_ElemLength(0.05);
                            mesh2d.Meshing(cad2d);

                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 11 )    // mesh with cut
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        uint id_l;
                        uint id_e1, id_e2, id_e3, id_e4, id_e5;
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(0.3,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            id_l = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.3,0.5) ).id_v_add;
                            id_e1 = cad2d.ConnectVertex_Line(2,id_v1).id_e_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.7,0.5) ).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.7,0.2) ).id_v_add;
                            uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.7,0.8) ).id_v_add;
                            uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5,0.5) ).id_v_add;
                            uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.9,0.5) ).id_v_add;
                            id_e2 = cad2d.ConnectVertex_Line(id_v2,id_v3).id_e_add;
                            id_e3 = cad2d.ConnectVertex_Line(id_v2,id_v4).id_e_add;
                            id_e4 = cad2d.ConnectVertex_Line(id_v2,id_v5).id_e_add;
                            id_e5 = cad2d.ConnectVertex_Line(id_v2,id_v6).id_e_add;
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.2))
                        {
                            IList<uint> aIdMsh_Inc = new List<uint>();
                            aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l,CAD_ELEM_TYPE.LOOP) );
                            
                            IList<uint> aIdMshBar_Cut = new List<uint>();
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e1,CAD_ELEM_TYPE.EDGE) );
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e2,CAD_ELEM_TYPE.EDGE) );
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e3,CAD_ELEM_TYPE.EDGE) );
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e4,CAD_ELEM_TYPE.EDGE) );
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e5,CAD_ELEM_TYPE.EDGE) );
                            ////////////////
                            IList< IList<int> > aLnods = new List<IList<int>>();
                            IList<uint> mapVal2Co = new List<uint>();
                            mesh2d.GetClipedMesh(out aLnods, out mapVal2Co, aIdMsh_Inc, aIdMshBar_Cut);

                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 12 )    // mesh with cut
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        uint id_l1, id_l2;
                        uint id_e3, id_e4;
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0, 0.0) );
                            pts.Add( new CVector2D(1.0, 0.0) );
                            pts.Add( new CVector2D(1.5, 0.0) );
                            pts.Add( new CVector2D(2.0, 0.0) );
                            pts.Add( new CVector2D(2.0, 1.0) );
                            pts.Add( new CVector2D(1.5, 1.0) );
                            pts.Add( new CVector2D(1.0, 1.0) );
                            pts.Add( new CVector2D(0.0, 1.0) );
                            uint id_l0 = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l0, new CVector2D(0.5,0.5) ).id_v_add;
                            uint id_e1 = cad2d.ConnectVertex_Line(2,7).id_e_add;
                            uint id_e2 = cad2d.ConnectVertex_Line(3,6).id_e_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,id_e1, new CVector2D(1.0,0.5) ).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(0.5,0.0) ).id_v_add;
                            id_e3 = cad2d.ConnectVertex_Line(id_v1,id_v2).id_e_add;
                            id_e4 = cad2d.ConnectVertex_Line(id_v1,id_v3).id_e_add;
                            id_l1 = 1;
                            id_l2 = 2;
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.2))
                        {
                            IList<uint> aIdMsh_Inc = new List<uint>();
                            aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l1,CAD_ELEM_TYPE.LOOP) );
                            aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l2,CAD_ELEM_TYPE.LOOP) );

                            IList<uint> aIdMshBar_Cut = new List<uint>();
                            //aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e3,CAD_ELEM_TYPE.EDGE) );
                            aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e4,CAD_ELEM_TYPE.EDGE) );
                            ////////////////
                            IList< IList<int> > aLnods = new List<IList<int>>();
                            IList<uint> mapVal2Co = new List<uint>();
                            mesh2d.GetClipedMesh(out aLnods, out mapVal2Co, aIdMsh_Inc, aIdMshBar_Cut);

                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 13 )
                {
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0, 0.0) );    // 1
                            pts.Add( new CVector2D(1.5, 0.0) );    // 2
                            pts.Add( new CVector2D(1.5, 0.4) );    // 3
                            pts.Add( new CVector2D(1.0, 0.4) );    // 4
                            pts.Add( new CVector2D(1.0, 0.5) );    // 5
                            pts.Add( new CVector2D(2.0, 0.5) );    // 6
                            pts.Add( new CVector2D(2.0, 1.0) );    // 7
                            pts.Add( new CVector2D(0.0, 1.0) );    // 8
                            pts.Add( new CVector2D(0.0, 0.5) );    // 9
                            uint id_l0 = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_e1 = cad2d.ConnectVertex_Line(5,9).id_e_add;
                            cad2d.ShiftLayer_Loop(id_l0,true);
                            double[] col = new double[3] { 0.9, 0.4, 0.4 };
                            cad2d.SetColor_Loop(id_l0, col);
                            cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(1.3,0.5));
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d, 0.05))
                        {
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawerMsh2D(mesh2d) );
                            DrawerAry.InitTrans( Camera );
                        }
                    }
                }
                else if( ProbNo == 14 )
                {
                    string svgfn = "../../../input_file/shape2d_0.svg";
                    if (File.Exists(svgfn))
                    {
                        CCadObj2D cad2d = null;
                        CCadSVG.ReadSVG_AddLoopCad(svgfn, out cad2d);
                        using (CMesher2D mesh2d = new CMesher2D())
                        {
                            mesh2d.SetMeshingMode_ElemSize(400);
                            mesh2d.AddIdLCad_CutMesh(1);
                            mesh2d.Meshing(cad2d);

                            DrawerAry.Clear();
                            DrawerAry.PushBack(new CDrawerMsh2D(mesh2d));
                            DrawerAry.InitTrans(Camera);
                        }
                        cad2d.Clear();
                        cad2d.Dispose();
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", svgfn);
                        MessageBox.Show(string.Format("SVGファイル:{0}がありません", svgfn));
                    }
                }
                success = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DrawerGlUtility.SetProjectionTransform(Camera);
            Glut.glutPostRedisplay();
            ProbNo++;
            if (ProbNo == ProbCnt) ProbNo = 0;
            return success;
        }
    }
}
