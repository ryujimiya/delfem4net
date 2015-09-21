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

namespace fluid2d
{
    // original header:
    ////////////////////////////////////////////////////////////////
    //                                                            //
    //    DelFEM Test_glut of Fluid 2D                            //
    //                                                            //
    //          Copy Rights (c) Nobuyuki Umetani 2009             //
    //          e-mail : numetani@gmail.com                       //
    ////////////////////////////////////////////////////////////////
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
        /// 流体2D方程式システムオブジェクト
        /// </summary>
        private CEqnSystem_Fluid2D Fluid = null;

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
        private double Dt = 0.8;
        /// <summary>
        /// ワールド座標系のベースID
        /// </summary>
        private uint Id_base = 0;

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
        /////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////
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
            Fluid = new CEqnSystem_Fluid2D();

            // Glutのアイドル時処理でなく、タイマーで再描画イベントを発生させる
            //MyTimer.Tick += (sender, e) =>
            //{
            //    Glut.glutPostRedisplay();
            //};
            //MyTimer.Interval = 1000 / 60;
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
            if (Fluid != null)
            {
                Fluid.Clear();
                Fluid.Dispose();
                Fluid = null;
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

            // initialize GLUT
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
            Glut.glutCreateWindow("DelFEM Demo");

            // set call-back functions
            Glut.glutIdleFunc(myGlutIdle);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutSpecialFunc(myGlutSpecial);;
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutIdleFunc(myGlutIdle);

            setNewProblem();

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
            //Gl.glClearColor(0.2f, 0.7f, 0.7f, 1.0f);
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            if( IsAnimation )
            {
                CurTime += Dt;
                FieldValueSetter.ExecuteValue(CurTime,World);
                Fluid.Solve(World);
                using (ConstUIntDoublePairVectorIndexer indexer = Fluid.GetAry_ItrNormRes())
                {
                    if (indexer.Count() > 0)
                    {
                        //Console.WriteLine("Iter : {0} Res : {1}", Fluid.GetAry_ItrNormRes()[0].First, Fluid.GetAry_ItrNormRes()[0].Second);
                    }
                }
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
            Glut.glutPostRedisplay();
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
            if(      Modifier == Glut.GLUT_ACTIVE_SHIFT )
            {
                Camera.MousePan(MovBeginX,MovBeginY,movEndX,movEndY); 
            }
            else if( Modifier == Glut.GLUT_ACTIVE_CTRL )
            {
                Camera.MouseRotation(MovBeginX,MovBeginY,movEndX,movEndY); 
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
            Modifier = Glut.glutGetModifiers();
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
                if ( ProbNo == 0 )    // cavity flow (stationaly stokes)
                {
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(-0.5,-0.5) );
                        pts.Add( new CVector2D( 0.5,-0.5) );
                        pts.Add( new CVector2D( 0.5, 0.5) );
                        pts.Add( new CVector2D(-0.5, 0.5) );
                        uint id_l = cad2d.AddPolygon( pts ).id_l_add;
                        cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.0,0.0));
                        World.Clear();
                        Id_base = World.AddMesh(new CMesher2D(cad2d, 0.04));
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                    Fluid.Clear();
                    Fluid.UnSetInterpolationBubble();
                    Fluid.UpdateDomain_Field(Id_base,World);

                    uint id_field_press = Fluid.GetIdField_Press();
                    //uint id_field_press_bc0 = World.GetPartialField(id_field_press,10);
                    //Fluid.AddFixField(id_field_press_bc0,World);

                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0,World);
                    FieldValueSetter.SetMathExp("0.5*sin(0.05*t)", 0,FIELD_DERIVATION_TYPE.VELOCITY, World);
                    uint id_field_bc1;
                    {
                        IList<uint> id_ea_bc1 = new List<uint>();
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE));
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE));
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE));
                        id_field_bc1 = Fluid.AddFixElemAry(id_ea_bc1,World);
                    }
                    Fluid.SetRho(0.1);
                    Fluid.SetMyu(0.0002);
                    Fluid.SetStokes();
                    Fluid.SetIsStationary(true);
                    Dt = 0.5;
                    Fluid.SetTimeIntegrationParameter(Dt);

                    // registration of visualization objects
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World, id_field_press) );
                    DrawerAry.PushBack( new CDrawerEdge(id_field_velo,true,World) );
                    //DrawerAry.PushBack( new CDrawerImageBasedFlowVis(id_field_velo,World,1) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 1 )    // cavity flow (non-stationary storks)
                {
                    Fluid.SetIsStationary(false);
                }
                else if ( ProbNo == 2 )    // cavity flow (non-stationary storks with larger rho )
                {
                    Fluid.SetRho(0.5);
                }
                else if ( ProbNo == 3 )    // cavity flow，non-static Naiver-Stokes flow
                {
                    Fluid.SetRho(0.02);
                    Fluid.SetMyu(0.00001);
                    Fluid.SetNavierStokes();
                }
                else if ( ProbNo == 4 )    // cavity flow，bubble interpolation，constant storks
                {
                    Fluid.Clear();
                    Fluid.SetInterpolationBubble();
                    Fluid.UpdateDomain_Field(Id_base,World);
                    Fluid.SetStokes();
                    Fluid.SetIsStationary(true);
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                    uint id_field_press = Fluid.GetIdField_Press();
                    Console.WriteLine("press : " + id_field_press);
                    uint id_field_press_bc0 = World.GetPartialField(id_field_press,conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.VERTEX));
                    Fluid.AddFixField(id_field_press_bc0,World);

                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    Console.WriteLine("velo : " + id_field_velo);
                    uint id_field_bc0 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0, World);
                    FieldValueSetter.SetMathExp("0.5*sin(0.05*t)", 0,FIELD_DERIVATION_TYPE.VELOCITY, World);
                    uint id_field_bc1;
                    {
                        IList<uint> id_ea_bc1 = new List<uint>();
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE));
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE));
                        id_ea_bc1.Add(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE));
                        id_field_bc1 = Fluid.AddFixElemAry(id_ea_bc1,World);
                    }

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World, id_field_press) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 5 ) // l shaped flow
                {
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0, 0.0) );
                        pts.Add( new CVector2D(1.0, 0.0) );
                        pts.Add( new CVector2D(1.0, 0.5) );
                        pts.Add( new CVector2D(0.5, 0.5) );
                        pts.Add( new CVector2D(0.5, 1.0) );
                        pts.Add( new CVector2D(0.0, 1.0) );
                        cad2d.AddPolygon( pts );
                        World.Clear();
                        Id_base = World.AddMesh(new CMesher2D(cad2d, 0.04));
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                    Fluid.UnSetInterpolationBubble();
                    Fluid.Clear();
                    Fluid.UpdateDomain_Field(Id_base,World);
                    Fluid.SetTimeIntegrationParameter(Dt);

                    uint id_field_press = Fluid.GetIdField_Press();
                    //uint id_field_press_bc0 = World.GetPartialField(id_field_press,10);
                    //Fluid.AddFixField(id_field_press_bc0,World);

                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0, World);
                    FieldValueSetter.SetMathExp("0.1*sin(0.1*t)", 0,FIELD_DERIVATION_TYPE.VELOCITY, World);
                    uint id_field_bc1;
                    {
                        IList<uint> id_ea_bc1 = new List<uint>();
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(6,CAD_ELEM_TYPE.EDGE) );
                        id_field_bc1 = Fluid.AddFixElemAry(id_ea_bc1,World);
                    }
                    Fluid.SetRho(0.1);
                    Fluid.SetMyu(0.0002);
                    //Fluid.UnSetStationary(World);
                    Fluid.SetIsStationary(true);
                    Fluid.SetStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World, id_field_press) );
                    DrawerAry.PushBack( new CDrawerEdge(id_field_velo,true,World) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 6 )
                {
                    Fluid.SetIsStationary(false);
                    Fluid.SetNavierStokes();
                    Fluid.SetRho(1.3);
                    Fluid.SetMyu(0.0002);
                }
                else if( ProbNo == 7 )
                {
                    CCadObj2D cad2d = new CCadObj2D();
                    {    // define shape ( square devided vertically in half )
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(0.5,0.0)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(0.5,1.0)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1,id_v2);
                    }

                    CurTime = 0;
                    World.Clear();
                    Id_base = World.AddMesh( new CMesher2D(cad2d,0.03) );
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                    Fluid.Clear();
                    Fluid.UpdateDomain_FieldElemAry(Id_base, conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP) ,World);
                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();

                    uint id_field_bc1 = Fluid.AddFixElemAry( conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE) ,World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1, World);
                    FieldValueSetter.SetMathExp("0.3*sin(0.5*t)", 1,FIELD_DERIVATION_TYPE.VELOCITY, World);    
                    uint id_field_bc2 = Fluid.AddFixElemAry( conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE) ,World);

                    Fluid.SetRho(0.1);
                    Fluid.SetMyu(0.0002);
                    Fluid.SetStokes();
                    Fluid.SetIsStationary(true);
                    Fluid.SetTimeIntegrationParameter(Dt);

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_base,true,World) );
                    //DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 8 )
                {
                    Fluid.SetIsStationary(false);
                }
                else if ( ProbNo == 9 )
                {
                    Fluid.SetNavierStokes();
                }
                else if ( ProbNo == 10 )
                {    // Karman vortex sheet problem
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape (hole in rectangle)
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.6) );
                        pts.Add( new CVector2D(0.0,0.6) );
                        uint id_l = cad2d.AddPolygon( pts ).id_l_add;
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.2,0.2)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.2)).id_v_add;
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.4)).id_v_add;
                        uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.2,0.4)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1,id_v2);
                        cad2d.ConnectVertex_Line(id_v2,id_v3);
                        cad2d.ConnectVertex_Line(id_v3,id_v4);
                        cad2d.ConnectVertex_Line(id_v4,id_v1);
                        World.Clear();
                        Id_base = World.AddMesh( new CMesher2D(cad2d,0.05) );
                    }

                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);

                    Fluid.Clear();
                    Fluid.UpdateDomain_FieldElemAry(Id_base,conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP),World);
                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();

                    uint id_field_bc1 = Fluid.AddFixElemAry( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) ,World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter();
                    CFieldValueSetter.SetFieldValue_Constant(id_field_bc1,0,FIELD_DERIVATION_TYPE.VELOCITY,World,0.1);
                    {
                        IList<uint> aIdEAFixVelo = new List<uint>();
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE) );
                        uint id_field_bc2 = Fluid.AddFixElemAry(aIdEAFixVelo,World);
                    }
                    {
                        IList<uint> aIdEAFixVelo = new List<uint>();
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE) );
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(6,CAD_ELEM_TYPE.EDGE) );
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(7,CAD_ELEM_TYPE.EDGE) );
                        aIdEAFixVelo.Add( conv.GetIdEA_fromCad(8,CAD_ELEM_TYPE.EDGE) );
                        uint id_field_bc3 = Fluid.AddFixElemAry(aIdEAFixVelo,World);
                    }
                    Dt = 0.13;
                    Fluid.SetRho(200.0);
                    Fluid.SetMyu(0.0001);
                    Fluid.SetNavierStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    DrawerAry.PushBack( new CDrawerEdge(id_field_velo,true,World) );
                    //DrawerAry.PushBack( new CDrawerImageBasedFlowVis(id_field_velo,World,1) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 11 )    // two element array problem
                {
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0, 0.0) );
                        pts.Add( new CVector2D(1.0, 0.0) );
                        pts.Add( new CVector2D(1.0, 0.5) );
                        pts.Add( new CVector2D(1.0, 1.0) );
                        pts.Add( new CVector2D(0.0, 1.0) );
                        pts.Add( new CVector2D(0.0, 0.5) );
                        cad2d.AddPolygon( pts );
                        cad2d.ConnectVertex_Line(3,6);
                        World.Clear();
                        Id_base = World.AddMesh( new CMesher2D(cad2d,0.04) );
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);

                    Fluid.Clear();
                    Fluid.UnSetInterpolationBubble();
                    Fluid.UpdateDomain_Field(Id_base,World);
                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0, World);
                    FieldValueSetter.SetMathExp("0.1*sin(0.1*t)", 0,FIELD_DERIVATION_TYPE.VELOCITY, World);
                    uint id_field_bc1;
                    {
                        IList<uint> id_ea_bc1 = new List<uint>();
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(6,CAD_ELEM_TYPE.EDGE) );
                        id_field_bc1 = Fluid.AddFixElemAry(id_ea_bc1,World);
                    }
                    Dt = 0.8;
                    Fluid.SetRho(1);
                    Fluid.SetMyu(0.0002);
                    Fluid.SetIsStationary(false);
                    //Fluid.SetStationary(World);
                    Fluid.SetNavierStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    //DrawerAry.PushBack( new CDrawerFaceContour(id_field_press,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 12 )
                { // back-step flow
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D( 0.0, 0.0) );
                        pts.Add( new CVector2D( 1.4, 0.0) );
                        pts.Add( new CVector2D( 1.5, 0.0) );
                        pts.Add( new CVector2D( 1.5, 1.0) );
                        pts.Add( new CVector2D( 1.4, 1.0) );
                        pts.Add( new CVector2D(-0.5, 1.0) );
                        pts.Add( new CVector2D(-0.5, 0.7) );
                        pts.Add( new CVector2D( 0.0, 0.7) );
                        cad2d.AddPolygon( pts );
                        cad2d.ConnectVertex_Line(2,5);
                        World.Clear();
                        Id_base = World.AddMesh( new CMesher2D(cad2d,0.04) );
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);

                    Fluid.Clear();
                    Fluid.UnSetInterpolationBubble();
                    Fluid.UpdateDomain_Field(Id_base,World);

                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(6,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter();
                    CFieldValueSetter.SetFieldValue_Constant(id_field_bc0,0,FIELD_DERIVATION_TYPE.VELOCITY, World, 0.2);
                    uint id_field_bc1;
                    {
                        IList<uint> id_ea_bc1 = new List<uint>();
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(7,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc1.Add( conv.GetIdEA_fromCad(8,CAD_ELEM_TYPE.EDGE) );
                        id_field_bc1 = Fluid.AddFixElemAry(id_ea_bc1,World);
                    }
                    Fluid.SetRho(5);
                    Fluid.SetMyu(0.0002);
                    Fluid.SetIsStationary(false);
                    //Fluid.SetStationary(World);
                    Fluid.SetNavierStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);
                    using (CEqn_Fluid2D eqn_fluid = Fluid.GetEquation( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP) ))
                    {
                        eqn_fluid.SetMyu(0.01);
                        Fluid.SetEquation(eqn_fluid);
                    }

                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    //DrawerAry.PushBack( new CDrawerFaceContour(id_field_press,World) );
                    //DrawerAry.PushBack( new CDrawerImageBasedFlowVis(id_field_velo,World,1) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );    
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 13 )
                {
                    // buoyant problem
                    CurTime = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {
                        // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D( 0.0, 0.0) );
                        pts.Add( new CVector2D( 3.0, 0.0) );
                        pts.Add( new CVector2D( 3.0, 3.0) );
                        pts.Add( new CVector2D( 0.0, 3.0) );
                        uint id_l = cad2d.AddPolygon( pts ).id_l_add;
                        pts.Clear();
                        pts.Add( new CVector2D( 1.0, 1.0) );
                        pts.Add( new CVector2D( 1.5, 1.0) );
                        pts.Add( new CVector2D( 1.5, 1.5) );
                        pts.Add( new CVector2D( 1.0, 1.5) );
                        cad2d.AddPolygon( pts, id_l );
                        World.Clear();
                        Id_base = World.AddMesh( new CMesher2D(cad2d,0.1) );
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);

                    Fluid.Clear();
                    Fluid.UnSetInterpolationBubble();
                    Fluid.UpdateDomain_Field(Id_base,World);

                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0;
                    {
                        IList<uint> id_ea_bc = new List<uint>();
                        id_ea_bc.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) );
                        id_field_bc0 = Fluid.AddFixElemAry(id_ea_bc,World);
                    }
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter();
                    Fluid.SetRho(5);
                    Fluid.SetMyu(0.005);
                    Fluid.SetIsStationary(false);
                    Fluid.SetNavierStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);
                    using (CEqn_Fluid2D eqn_fluid = Fluid.GetEquation( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP) ))
                    {
                        eqn_fluid.SetBodyForce(0.0,0.5);
                        Fluid.SetEquation(eqn_fluid);
                    }

                    // registration of visualization objects
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(id_field_velo,World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    //DrawerAry.PushBack( new CDrawerFaceContour(id_field_press,World);
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    DrawerAry.InitTrans( Camera );
                }
                else if ( ProbNo == 14 )
                {
                    CurTime = 0;
                    CIDConvEAMshCad conv = null;
                    uint id_l;
                    uint id_e1, id_e2, id_e3, id_e4, id_e5, id_e6;
                    uint Id_base2 = 0;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D( 0.0,0.0) );
                            pts.Add( new CVector2D( 0.5,0.0) );
                            pts.Add( new CVector2D( 2.0,0.0) );
                            pts.Add( new CVector2D( 2.0,1.0) );
                            pts.Add( new CVector2D( 0.0,1.0) );
                            id_l = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5,0.5) ).id_v_add;
                            id_e1 = cad2d.ConnectVertex_Line(2,id_v1).id_e_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.0,0.3) ).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.0,0.9) ).id_v_add;
                            id_e2 = cad2d.ConnectVertex_Line(id_v2,id_v3).id_e_add;
                            uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.5,0.4) ).id_v_add;
                            uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.5,0.1) ).id_v_add;
                            uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.5,0.7) ).id_v_add;
                            uint id_v7 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.2,0.4) ).id_v_add;
                            uint id_v8 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(1.8,0.4) ).id_v_add;
                            id_e3 = cad2d.ConnectVertex_Line(id_v4,id_v5).id_e_add;
                            id_e4 = cad2d.ConnectVertex_Line(id_v4,id_v6).id_e_add;
                            id_e5 = cad2d.ConnectVertex_Line(id_v4,id_v7).id_e_add;
                            id_e6 = cad2d.ConnectVertex_Line(id_v4,id_v8).id_e_add;
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d,0.05))
                        {
                            World.Clear();
                            Id_base = World.AddMesh(mesh2d);

                            conv = World.GetIDConverter(Id_base);
                            {
                                IList<uint> mapVal2Co = null; //new List<uint>();
                                IList< IList<int> > aLnods = null;//new List<IList<int>>();
                                {
                                    IList<uint> aIdMsh_Inc = new List<uint>();
                                    aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l,CAD_ELEM_TYPE.LOOP) );
                                    IList<uint> aIdMshBar_Cut = new List<uint>();
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e1,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e2,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e3,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e4,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e5,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshBar_Cut.Add( mesh2d.GetElemID_FromCadID(id_e6,CAD_ELEM_TYPE.EDGE) );
                                    mesh2d.GetClipedMesh(out aLnods, out mapVal2Co, aIdMsh_Inc,aIdMshBar_Cut);
                                }
                                IList<uint> aIdEA_Inc = new List<uint>();
                                aIdEA_Inc.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP) );
                                Id_base2 = World.SetCustomBaseField(Id_base,aIdEA_Inc,aLnods,mapVal2Co);
                            }
                        }
                    }

                    Fluid.Clear();
                    Fluid.UnSetInterpolationBubble();
                    Fluid.UpdateDomain_FieldVeloPress(Id_base,Id_base2,World);
                    uint id_field_press = Fluid.GetIdField_Press();
                    uint id_field_velo  = Fluid.GetIdField_Velo();
                    uint id_field_bc0;
                    {
                        IList<uint> id_ea_bc = new List<uint>();
                        id_ea_bc.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e1,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e2,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e3,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e4,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e5,CAD_ELEM_TYPE.EDGE) );
                        id_ea_bc.Add( conv.GetIdEA_fromCad(id_e6,CAD_ELEM_TYPE.EDGE) );
                        id_field_bc0 = Fluid.AddFixElemAry(id_ea_bc,World);
                    }
                    uint id_field_bc1 = Fluid.AddFixElemAry(conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                        FieldValueSetter = null;
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1, World);
                    FieldValueSetter.SetMathExp("0.1*sin(t*PI*0.1+0.01)", 0,FIELD_DERIVATION_TYPE.VELOCITY, World);

                    Dt = 0.8;
                    Fluid.SetRho(10);
                    Fluid.SetMyu(0.005);
                    Fluid.SetIsStationary(false);
                    //Fluid.SetStokes();
                    Fluid.SetNavierStokes();
                    Fluid.SetTimeIntegrationParameter(Dt);

                    // registration of visualization objects
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerVector(     id_field_velo, World) );
                    DrawerAry.PushBack( new CDrawerFace(id_field_press,true,World,id_field_press) );
                    //DrawerAry.PushBack( new CDrawerImageBasedFlowVis(id_field_velo,World,1) );
                    //DrawerAry.PushBack( new CDrawerStreamline(id_field_velo,World) );
                    //DrawerAry.PushBack( new CDrawerFaceContour(id_field_press,World) );
                    DrawerAry.InitTrans( Camera );
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
