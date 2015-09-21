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

namespace cad2d
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   CAD 2D
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
        private const int ProbCnt = 13;

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
            Glut.glutCreateWindow("Cad View");

            // Set callback function
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutKeyboardFunc(myGlutKeyboard);
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
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glPolygonOffset(1.1f, 4.0f);
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
        /// 問題を設定する
        /// </summary>
        /// <returns></returns>
        private bool setNewProblem()
        {
            bool success = false;
            try
            {
                using (CCadObj2D cad2d = new CCadObj2D())
                {
                    if (ProbNo == 0)
                    {
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add(new CVector2D(0.0, 0.0));
                        pts.Add(new CVector2D(1.0, 0.0));
                        pts.Add(new CVector2D(1.0, 1.0));
                        pts.Add(new CVector2D(0.0, 1.0));
                        uint id_l0 = cad2d.AddPolygon(pts, 0).id_l_add;
                        uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.5)).id_v_add;
                        uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.8)).id_v_add;
                        uint id_v7 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.8, 0.5)).id_v_add;
                        uint id_v8 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.2)).id_v_add;
                        uint id_v9 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.2, 0.5)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v5, id_v6);
                        cad2d.ConnectVertex_Line(id_v5, id_v7);
                        cad2d.ConnectVertex_Line(id_v5, id_v8);
                        cad2d.ConnectVertex_Line(id_v5, id_v9);

                        using (CCadObj2D cad2dtmp = cad2d.Clone()) // コピーのテスト
                        {
                            // export to the file
                            using (CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                cad2dtmp.Serialize(fout);
                            }
                        }
                        // import form the file
                        cad2d.Clear();
                        using (CSerializer fin = new CSerializer("hoge.txt", true))
                        {
                            cad2d.Serialize(fin);
                        }
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 1)
                    {
                        CCadObj2D.CResAddPolygon res;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(2.0, 1.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(1.0, 2.0));
                            pts.Add(new CVector2D(0.0, 2.0));
                            res = cad2d.AddPolygon(pts);
                        }
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, res.aIdE[1]);
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, res.aIdE[2]);
                        cad2d.RemoveElement(CAD_ELEM_TYPE.VERTEX, res.aIdV[2]);
                        uint id_e_new = cad2d.ConnectVertex_Line(res.aIdE[3], res.aIdE[1]).id_e_add;
                        CCadObj2D.CResAddVertex res0 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e_new, new CVector2D(1, 0.4));
                        CCadObj2D.CResAddVertex res1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e_new, new CVector2D(1, 0.6));
                        CCadObj2D.CResAddVertex res2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, res.aIdE[5], new CVector2D(0, 0.4));
                        CCadObj2D.CResAddVertex res3 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, res.aIdE[5], new CVector2D(0, 0.6));
                        cad2d.ConnectVertex_Line(res0.id_v_add, res2.id_v_add);
                        cad2d.ConnectVertex_Line(res1.id_v_add, res3.id_v_add);
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, res1.id_e_add);
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, res3.id_e_add);
                        using (CCadObj2D cad2dtmp = cad2d.Clone()) // コピーのテスト
                        {
                            // export to the file
                            using (CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                cad2dtmp.Serialize(fout);
                            }
                        }
                        // import form the file
                        cad2d.Clear();
                        using (CSerializer fin = new CSerializer("hoge.txt", true))
                        {
                            cad2d.Serialize(fin);
                        }
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 2)
                    {
                        uint id_l0;
                        {    // Make model
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.4));
                            pts.Add(new CVector2D(0.0, 0.4));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        //cad2d.SetCurve_Arc(1,true,-0.2);
                        //cad2d.SetCurve_Arc(2,true, -0.5);
                        cad2d.SetCurve_Arc(3, false, 0);
                        //cad2d.SetCurve_Arc(4,true, -0.5);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 3)
                    {
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(0, 0)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(1, 0)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1, id_v2);
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(1, 1)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v2, id_v3);
                        uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(0, 1)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v4, id_v3);
                        uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, 2, new CVector2D(1, 0.5)).id_v_add;
                        uint id_v7 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(0.5, 0.5)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v5, id_v7);
                        uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.NOT_SET, 0, new CVector2D(1.5, 0.5)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v5, id_v6);
                        cad2d.ConnectVertex_Line(id_v4, id_v1);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 4)
                    {
                        uint id_l0;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        //uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l0,new CVector2D(0.5,0.5));
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.2)).id_v_add;
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.8, 0.5)).id_v_add;
                        uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.8)).id_v_add;
                        uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.2, 0.5)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v2, id_v3);
                        cad2d.ConnectVertex_Line(id_v3, id_v4);
                        cad2d.ConnectVertex_Line(id_v4, id_v5);
                        uint id_e1 = cad2d.ConnectVertex_Line(id_v5, id_v2).id_e_add;
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, id_e1);
                        cad2d.ConnectVertex_Line(id_v5, id_v2);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 5)
                    {
                        uint id_l0;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 6)
                    {
                        uint id_l0;
                        {    // define initial loop
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.3, 0.1));
                            pts.Add(new CVector2D(0.9, 0.1));
                            pts.Add(new CVector2D(0.9, 0.7));
                            cad2d.AddPolygon(pts, id_l0);
                        }
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.1, 0.9));
                            pts.Add(new CVector2D(0.7, 0.9));
                            pts.Add(new CVector2D(0.1, 0.3));
                            cad2d.AddPolygon(pts, id_l0);
                        }
                        uint id_e0 = cad2d.ConnectVertex_Line(1, 3).id_e_add;
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, id_e0);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 7)
                    {
                        uint id_l0;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.9, 0.7)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.9, 0.1)).id_v_add;
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.3, 0.1)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1, id_v2);
                        cad2d.ConnectVertex_Line(id_v2, id_v3);
                        cad2d.ConnectVertex_Line(id_v3, id_v1);
                        {
                            uint id_e0 = cad2d.ConnectVertex_Line(id_v2, 2).id_e_add;
                            cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, id_e0);
                        }
                        {
                            uint id_e0 = cad2d.ConnectVertex_Line(2, id_v2).id_e_add;
                            cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, id_e0);
                        }
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 8)
                    {
                        uint id_e3, id_e4;
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));    // 1
                            pts.Add(new CVector2D(1.0, 0.0));    // 2
                            pts.Add(new CVector2D(1.5, 0.0));    // 3
                            pts.Add(new CVector2D(2.0, 0.0));    // 4
                            pts.Add(new CVector2D(2.0, 1.0));    // 5
                            pts.Add(new CVector2D(1.5, 1.0));    // 6
                            pts.Add(new CVector2D(1.0, 1.0));    // 7
                            pts.Add(new CVector2D(0.0, 1.0));    // 8
                            uint id_l0 = cad2d.AddPolygon(pts).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.5)).id_v_add;
                            uint id_e1 = cad2d.ConnectVertex_Line(2, 7).id_e_add;
                            uint id_e2 = cad2d.ConnectVertex_Line(3, 6).id_e_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e1, new CVector2D(1.0, 0.5)).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, 1, new CVector2D(0.5, 0.0)).id_v_add;
                            id_e3 = cad2d.ConnectVertex_Line(id_v1, id_v2).id_e_add;
                            id_e4 = cad2d.ConnectVertex_Line(id_v1, id_v3).id_e_add;
                        }
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 9)
                    {
                        uint id_l0;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.2, 0.7)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.2, 0.3)).id_v_add;
                        uint id_e0 = cad2d.ConnectVertex_Line(id_v1, id_v2).id_e_add;
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, id_e0, new CVector2D(0.2, 0.5)).id_v_add;
                        uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.5, 0.5)).id_v_add;
                        uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l0, new CVector2D(0.7, 0.5)).id_v_add;
                        uint id_e1 = cad2d.ConnectVertex_Line(id_v3, id_v4).id_e_add;
                        uint id_e2 = cad2d.ConnectVertex_Line(id_v4, id_v5).id_e_add;
                        cad2d.RemoveElement(CAD_ELEM_TYPE.EDGE, id_e1);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 10)
                    {
                        uint id_l0;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            id_l0 = cad2d.AddPolygon(pts).id_l_add;
                        }
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, 0, new CVector2D(1.1, 0)).id_v_add;
                        cad2d.RemoveElement(CAD_ELEM_TYPE.VERTEX, id_v1);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 11)
                    {
                        {    // define initial loop
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add(new CVector2D(0.0, 0.0));
                            pts.Add(new CVector2D(1.0, 0.0));
                            pts.Add(new CVector2D(1.0, 1.0));
                            pts.Add(new CVector2D(0.0, 1.0));
                            pts.Add(new CVector2D(0.0, 0.5));
                            cad2d.AddPolygon(pts);
                        }
                        /*
                        {
                            IList<CVector2D> aRelCo = new List<CVector2D>();
                            aRelCo.Add(new CVector2D(0.25, -0.1));
                            aRelCo.Add(new CVector2D(0.5, -0.0));
                            aRelCo.Add(new CVector2D(0.75, -0.1));
                            cad2d.SetCurve_Polyline(1, aRelCo);
                        }
                        cad2d.SetCurve_Arc(2, true, -1);
                        {
                            IList<CVector2D> aRelCo = new List<CVector2D>();
                            aRelCo.Add(new CVector2D(+0.01, 0.35));
                            aRelCo.Add(new CVector2D(-0.05, 0.25));
                            aRelCo.Add(new CVector2D(+0.01, 0.15));
                            cad2d.SetCurve_Polyline(5, aRelCo);
                        }
                         */
                        cad2d.RemoveElement(CAD_ELEM_TYPE.VERTEX, 1);  // removing this point marges 2 polylines
                        cad2d.SetCurve_Bezier(3, 0.2, +0.5, 0.8, -0.5);
                        //cad2d.SetCurve_Bezier(4, 0.0,-1.0, 0.3,+1.0);
                        //cad2d.SetCurve_Bezier(5, 0.0,-1.0, 0.3,-1.0);
                        Console.WriteLine("area : {0}", cad2d.GetArea_Loop(1));
                        cad2d.WriteToFile_dxf("hoge.dxf", 1);
                        DrawerAry.Clear();
                        DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                        DrawerAry.InitTrans(Camera);
                    }
                    else if (ProbNo == 12)
                    {
                        string svgfname = "../../../input_file/shape2d_0.svg";
                        CCadObj2D cad2dtmp = null;
                        if (File.Exists(svgfname))
                        {
                            DelFEM4NetCad.CCadSVG.ReadSVG_AddLoopCad(svgfname, out cad2dtmp);
                            // export to the file
                            using (CSerializer fout = new CSerializer("hoge.txt", false))
                            {
                                cad2dtmp.Serialize(fout);
                            }
                            cad2dtmp.Dispose();
                            // import form the file
                            cad2d.Clear();
                            using (CSerializer fin = new CSerializer("hoge.txt", true))
                            {
                                cad2d.Serialize(fin);
                            }
                            DrawerAry.Clear();
                            DrawerAry.PushBack(new CDrawer_Cad2D(cad2d));
                            DrawerAry.InitTrans(Camera);
                        }
                        else
                        {
                            MessageBox.Show("SVGファイルがありません");
                        }
                    }
                } // using cad2d
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
