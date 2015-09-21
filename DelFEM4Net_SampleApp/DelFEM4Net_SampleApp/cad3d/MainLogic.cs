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

namespace cad3d
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   CAD 3D
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
        private const int ProbCnt = 4;

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
        /// Cad3Dオブジェクト
        /// </summary>
        private CCadObj3D Cad3D = null;
        /// <summary>
        /// 選択中ループID
        /// </summary>
        private uint Id_loop_selected = 0;
        /// <summary>
        /// 選択された点
        /// </summary>
        private CVector3D PickedPos = new CVector3D();
        /// <summary>
        /// マウスの位置
        /// </summary>
        private CVector3D MousePos = new CVector3D();
        /// <summary>
        /// モード区分(メニューに対応)
        ///    0: ループをドラッグ
        ///    1: ループを追加
        /// </summary>
        private uint ModeDv = 0;
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
            Cad3D = new CCadObj3D();

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
            if (Cad3D != null)
            {
                Cad3D.Clear();
                Cad3D.Dispose();
                Cad3D = null;
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
            Glut.glutInitWindowPosition(200,200);
            Glut.glutInitWindowSize(400, 300);
            string[] commandLineArgs = System.Environment.GetCommandLineArgs();
            int argc = commandLineArgs.Length;
            StringBuilder[] argv = new StringBuilder[argc];
            for (int i = 0; i < argc; i++)
            {
                argv[i] = new StringBuilder(commandLineArgs[i]);
            }
            Glut.glutInit(ref argc, argv);
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE|Glut.GLUT_RGBA|Glut.GLUT_DEPTH);
            Glut.glutCreateWindow("Cad 3D View");

            // Set callback function
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            //Glut.glutIdleFunc(myGlutIdle);
            Glut.glutSpecialFunc(myGlutSpecial);

            setNewProblem();

            {
                Gl.glEnable(Gl.GL_LIGHTING);
                Gl.glEnable(Gl.GL_LIGHT0);
                float[] light0pos = new float[4] {0, 0, +20, 0};
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, light0pos);
                float[] white1 = new float[3] {0.7f, 0.7f, 0.7f};
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, white1);
                ////
                Gl.glEnable(Gl.GL_LIGHT1);
                float[] light1pos = new float[4] {0,10, 0, 0};
                float[] white2 = new float[3] {0.9f, 0.9f, 0.9f};
                Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, light1pos);
                Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, white2);
            }

            Glut.glutCreateMenu(myGlutMenu);
            Glut.glutAddMenuEntry("Drag Loop", 0);
            Glut.glutAddMenuEntry("Add Loop", 1);
            Glut.glutAttachMenu(Glut.GLUT_RIGHT_BUTTON);

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
            Camera.SetWindowAspect((double)w/h);
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
            //Gl.glClearColor(0.2f, 0.7f, 0.7f, 1.0f);
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            DelFEM4NetCom.GlutUtility.ShowBackGround();
            DelFEM4NetCom.GlutUtility.ShowFPS(ref Frame, ref Timebase, ref Stringfps);

            {
                Gl.glDisable(Gl.GL_LIGHTING);
                Gl.glLineWidth(2);
                /*
                IList<uint> aIdL = Cad3D.GetAryElemID(CAD_ELEM_TYPE.LOOP);
                for (uint iil = 0; iil < aIdL.Count; iil++)
                {
                    uint id_l = aIdL[iil];
                    CLoop3D l = Cad3D.GetLoop(id_l);
                    CVector3D o = l.org;
                    CVector3D n = l.normal;
                    double r = 0.1;
                    Gl.glBegin(Gl.GL_LINES);
                    Gl.glColor3d(1,0,0);
                    Gl.glVertex3d(o.x, o.y, o.z);
                    Gl.glVertex3d(o.x + n.x * r, o.y + n.y * r, o.z + n.z * r);
                    Gl.glEnd();
                }
                 */
                if ( ModeDv == 1 )
                {
                    if ( Cad3D.IsElemID(CAD_ELEM_TYPE.LOOP,Id_loop_selected) )
                    {
                        CLoop3D l = Cad3D.GetLoop(Id_loop_selected);
                        CVector3D o0 = l.org;
                        CVector3D n0 = l.normal;
                        CVector3D x0 = l.dirx;
                        CVector3D y0 = CVector3D.Cross(n0, x0);
                        x0 = CVector3D.Dot(MousePos - PickedPos, x0) * x0;
                        y0 = CVector3D.Dot(MousePos - PickedPos, y0) * y0;
                        CVector3D p = PickedPos;
                        CVector3D px = PickedPos+x0;
                        CVector3D py = PickedPos+y0;
                        CVector3D pxy = PickedPos+x0+y0;
                        Gl.glLineWidth(1);
                        Gl.glColor3d(0, 0, 0);
                        Gl.glBegin(Gl.GL_LINES);
                        Gl.glVertex3d(p.x, p.y, p.z);
                        Gl.glVertex3d(px.x, px.y, px.z);
                        Gl.glVertex3d(p.x, p.y, p.z);
                        Gl.glVertex3d(py.x, py.y, py.z);
                        Gl.glVertex3d(pxy.x, pxy.y, pxy.z);
                        Gl.glVertex3d(px.x, px.y, px.z);
                        Gl.glVertex3d(pxy.x, pxy.y, pxy.z);
                        Gl.glVertex3d(py.x, py.y, py.z);
                        Gl.glEnd();
                    }
                    /*
                    Gl.glBegin(Gl.GL_POINTS);
                    Gl.glColor3d(1, 0, 0);
                    Gl.glVertex3d(PickedPos.x, PickedPos.y, PickedPos.z);
                    Gl.glColor3d(1, 0, 1);
                    Gl.glVertex3d(MousePos. x,MousePos.y, MousePos.z);
                    Gl.glEnd();
                     */
                }
                Gl.glEnable(Gl.GL_LIGHTING);
            }
            
            DrawerAry.Draw();
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
            if ((Modifier & Glut.GLUT_ACTIVE_CTRL) == Glut.GLUT_ACTIVE_CTRL)
            {
                Camera.MouseRotation(MovBeginX, MovBeginY, movEndX, movEndY);
            }
            else if ((Modifier & Glut.GLUT_ACTIVE_SHIFT) == Glut.GLUT_ACTIVE_SHIFT)
            {
                Camera.MousePan(MovBeginX, MovBeginY, movEndX, movEndY);
            }
            else if( Cad3D.IsElemID(CAD_ELEM_TYPE.LOOP, Id_loop_selected) )
            {
                if( ModeDv == 0 )
                {
                    if( Cad3D.IsElemID(CAD_ELEM_TYPE.LOOP, Id_loop_selected) )
                    {
                        CVector3D nv = Cad3D.GetLoop(Id_loop_selected).normal;
                        double[] n = new double[3] { nv.x, nv.y, nv.z };
                        double[] r = null;// new double[9];
                        Camera.RotMatrix33(out r);
                        double[] nr = new double[3]
                            {
                                r[0] * n[0] + r[1] * n[1] + r[2] * n[2],
                                r[3] * n[0] + r[4] * n[1] + r[5] * n[2],
                                r[6] * n[0] + r[7] * n[1] + r[8] * n[2]
                            };
                        double[] del = new double[2] { movEndX-MovBeginX, movEndY-MovBeginY };
                        nr[0] /= Camera.GetHalfViewHeight() * Camera.GetWindowAspect();
                        nr[1] /= Camera.GetHalfViewHeight();
                        if( nr[0]*nr[0] + nr[1]*nr[1] > 1.0e-5 )
                        {
                            double rr = (nr[0] * del[0] + nr[1] * del[1]) / (nr[0] * nr[0] + nr[1] * nr[1]);
                            Cad3D.LiftLoop(Id_loop_selected, nv * rr);
                            DrawerAry.Clear();
                            DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                        }
                    }
                }
                else if( ModeDv == 1 )
                {
                    if( Cad3D.IsElemID(CAD_ELEM_TYPE.LOOP, Id_loop_selected) )
                    {
                        CLoop3D l = Cad3D.GetLoop(Id_loop_selected);
                        CVector3D o0 = l.org;
                        CVector3D n0 = l.normal;
                        CMatrix3 r = Camera.RotMatrix33();
                        CVector3D d1 = r.MatVecTrans( new CVector3D(0,0,-1) );
                        double hvh = Camera.GetHalfViewHeight();
                        double asp = Camera.GetWindowAspect();
                        CVector3D cp = - 1.0 * Camera.GetCenterPosition() + new CVector3D(hvh * asp * movEndX, hvh * movEndY, 0);
                        CVector3D o1 = r.MatVecTrans( cp ) + Camera.GetObjectCenter();
                        double tmp0 = CVector3D.Dot(d1, n0);
                        double tmp1 = CVector3D.Dot(o0 - o1, n0);
                        double tmp2 = tmp1 / tmp0;
                        MousePos = o1 + d1 * tmp2;
                    }
                }
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
            if (state == Glut.GLUT_DOWN && Modifier == 0)
            {
                int sizeBuffer = 2048;
                DelFEM4NetCom.View.DrawerGlUtility.PickSelectBuffer pickSelectBuffer = null;
                DelFEM4NetCom.View.DrawerGlUtility.PickPre((uint)sizeBuffer, out pickSelectBuffer, (uint)x, (uint)y, 5, 5, Camera);
                DrawerAry.DrawSelection();
                List<DelFEM4NetCom.View.SSelectedObject> aSelecObj = (List<DelFEM4NetCom.View.SSelectedObject>)DelFEM4NetCom.View.DrawerGlUtility.PickPost(pickSelectBuffer, (uint)x, (uint)y, Camera);
                /*
                uint[] select_buffer = pickSelectBuffer.ToArray();
                foreach (uint buf in select_buffer)
                {
                    Console.Write("[" + buf + "]");
                }
                Console.WriteLine();
                */
                DrawerAry.ClearSelected();
                if (aSelecObj.Count > 0)
                {
                    DrawerAry.AddSelected(aSelecObj[0].name);
                    if( aSelecObj[0].name[1] == 3 )
                    {
                        Id_loop_selected = (uint)aSelecObj[0].name[2];
                        PickedPos = aSelecObj[0].picked_pos;
                    }
                    else
                    {
                        Id_loop_selected = 0;
                    }
                    MousePos = PickedPos;
                }
            }
            if(state == Glut.GLUT_UP && Modifier == 0)
            {
                if( ModeDv == 1 )
                {
                    if( Cad3D.IsElemID(CAD_ELEM_TYPE.LOOP, Id_loop_selected) )
                    {
                        CLoop3D l = Cad3D.GetLoop(Id_loop_selected);
                        CVector2D v0 = l.Project(PickedPos);
                        CVector2D v1 = l.Project(MousePos);
                        Cad3D.AddRectLoop(Id_loop_selected,v0,v1);
                        Id_loop_selected = 0;
                        DrawerAry.Clear();
                        DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                    }
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
        /// Glut: メニューイベントハンドラ
        /// </summary>
        /// <param name="value"></param>
        private void myGlutMenu(int value)
        {
            if (value == 0)
            {
                ModeDv = 0;
                Console.WriteLine("Drag Loop");
            }
            else if (value == 1)
            {
                ModeDv = 1;
                Console.WriteLine("Add Loop");
            }
        }


        /// <summary>
        /// 問題を設定する
        /// </summary>
        /// <returns></returns>
        private bool setNewProblem()
        {
            bool success = false;

            Console.WriteLine("SetNewProblem() " + ProbNo);
            try
            {
                Cad3D.Clear();
                if (ProbNo == 0)
                {
                    uint id_s0 = Cad3D.AddCuboid(1, 1, 1);
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                    DrawerAry.InitTrans(Camera);
                }
                else if( ProbNo == 1 )
                {
                    uint id_s0 = Cad3D.AddCuboid(1,1,0.7);
                    uint id_l1 = 0;
                    {
                        IList<CVector3D> aVec = new List<CVector3D>();
                        aVec.Add( new CVector3D(0.5, 0.2, 0.7) );
                        aVec.Add( new CVector3D(0.8, 0.2, 0.7) );
                        aVec.Add( new CVector3D(0.8, 0.8, 0.7) );
                        aVec.Add( new CVector3D(0.5, 0.8, 0.7) );
                        id_l1 = Cad3D.AddPolygon(aVec, 6);
                    }
                    uint id_l2 = 0;
                    {
                        IList<CVector3D> aVec = new List<CVector3D>();
                        aVec.Add( new CVector3D(0.2, 1.0, 0.3) );
                        aVec.Add( new CVector3D(0.2, 1.0, 0.5) );
                        aVec.Add( new CVector3D(0.8, 1.0, 0.5) );
                        aVec.Add( new CVector3D(0.8, 1.0, 0.3) );
                        id_l2 = Cad3D.AddPolygon(aVec, 3);
                    }
                    Cad3D.LiftLoop(id_l1, Cad3D.GetLoop(id_l1).normal *   0.2);
                    Cad3D.LiftLoop(id_l2, Cad3D.GetLoop(id_l2).normal * (-0.2));

                    {
                        uint id_v1 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 1, new CVector3D(0, 0.7, 0  ) );
                        uint id_v2 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 9, new CVector3D(0, 0.7, 0.7) );
                        CBRepSurface.CResConnectVertex res = Cad3D.ConnectVertex(id_v1, id_v2);
                        uint id_l_lift = Cad3D.GetIdLoop_Edge(res.id_e_add, true);
                        Cad3D.LiftLoop(id_l_lift, Cad3D.GetLoop(id_l_lift).normal * 0.2);
                    }
         
                    {
                      uint id_v1 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 9, new CVector3D(0, 0.3, 0.7 ) );
                      uint id_v2 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 5, new CVector3D(0, 0.0, 0.2 ) );
                      uint id_v3 = Cad3D.AddPoint( CAD_ELEM_TYPE.LOOP, 17, new CVector3D(0, 0.3, 0.2 ) );
                      Cad3D.ConnectVertex(id_v1, id_v3);
                      CBRepSurface.CResConnectVertex res = Cad3D.ConnectVertex(id_v2, id_v3);
                      uint id_l_lift = res.id_l_add;
                      Cad3D.LiftLoop(id_l_lift, Cad3D.GetLoop(id_l_lift).normal * (-0.2));
                    }

                    Cad3D.LiftLoop(6, new CVector3D(0, 0, -0.1));
                    
                    {
                        uint id_v1 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 3, new CVector3D(1.0, 0.2, 0.0 ) );
                        uint id_v2 = Cad3D.AddPoint( CAD_ELEM_TYPE.LOOP, 4, new CVector3D(1.0, 0.2, 0.2 ) );
                        uint id_v3 = Cad3D.AddPoint( CAD_ELEM_TYPE.LOOP, 4, new CVector3D(1.0, 0.6, 0.2 ) );
                        uint id_v4 = Cad3D.AddPoint( CAD_ELEM_TYPE.EDGE, 3, new CVector3D(1.0, 0.6, 0.0 ) );
                        Cad3D.ConnectVertex(id_v1, id_v2);
                        Cad3D.ConnectVertex(id_v2, id_v3);
                        CBRepSurface.CResConnectVertex res = Cad3D.ConnectVertex(id_v3, id_v4);
                        uint id_l_lift = res.id_l_add;
                        Cad3D.LiftLoop(id_l_lift, Cad3D.GetLoop(id_l_lift).normal * (-0.1));
                    }
                  
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                    DrawerAry.InitTrans(Camera);
                }
                else if( ProbNo == 2 )
                {
                    uint id_s0 = Cad3D.AddCuboid(1,1,1);
                    Cad3D.AddRectLoop(1, new CVector2D(0.2, 0.2), new CVector2D(0.8, 1.2) );
                    Cad3D.LiftLoop(1, Cad3D.GetLoop(1).normal * 0.1);
                    //Cad3D.LiftLoop(7, Cad3D.GetLoop(7).normal * 0.1);
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                    DrawerAry.InitTrans(Camera);
                }
                else if( ProbNo == 3 )
                {
                    uint id_s0 = Cad3D.AddCuboid(1,1,1);
                    Cad3D.AddRectLoop(
                        6, 
                        Cad3D.GetLoop(6).Project(new CVector3D(0.2, 0.2, 1)),
                        Cad3D.GetLoop(6).Project(new CVector3D(0.8, 0.8, 1)));
                    Cad3D.AddRectLoop(
                       7, 
                       Cad3D.GetLoop(7).Project(new CVector3D(0.3, 0.3, 1)),
                       Cad3D.GetLoop(7).Project(new CVector3D(0.7, 0.7, 1)));
                    //Cad3D.AddRectLoop(7, new CVector2D(0.3, 0.3), new CVector2D(0.7, 0.7) );
                    Cad3D.LiftLoop(7, Cad3D.GetLoop(7).normal * 0.1);
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawer_Cad3D(Cad3D) );
                    DrawerAry.InitTrans(Camera);
                }
                success = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetProjectionTransform(Camera);
            //Glut.glutPostRedisplay();
            ProbNo++;
            if (ProbNo == ProbCnt) ProbNo = 0;
            return success;
        }
    }
}
