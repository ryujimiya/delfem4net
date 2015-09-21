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

namespace solid2d
{
    // original header:
    ////////////////////////////////////////////////////////////////
    //                                                            //
    //      DelFEM demo : Solid2D                                 //
    //                                                            //
    //          Copy Rights (c) Nobuyuki Umetani 2008             //
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
        private const int ProbCnt = 30;

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
        /// 固体2D方程式システムオブジェクト
        /// </summary>
        private CEqnSystem_Solid2D Solid = null;
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
        private double Dt = 0.05;
        /// <summary>
        /// 表示オブジェクトのフィールドID
        /// </summary>
        private uint Id_field_disp = 0;
        /// <summary>
        /// handle of equiv stress (scalar value)
        /// </summary>
        private uint Id_field_equiv_stress = 0;
        /// <summary>
        /// handle of stress field (sym-tensor field)
        /// </summary>
        private uint Id_field_stress = 0;
        /// <summary>
        /// 固定境界のフィールドID
        /// </summary>
        private uint Id_field_disp_fix0 = 0;
        /// <summary>
        /// 
        /// </summary>
        private uint Id_field_temp = 0;

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
            Solid = new CEqnSystem_Solid2D();

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
            if (Solid != null)
            {
                Solid.Clear();
                Solid.Dispose();
                Solid = null;
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
            Gl.glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
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
                FieldValueSetter.ExecuteValue(CurTime, World);
                Solid.Solve(World);
                if (Id_field_equiv_stress != 0) { Solid.SetEquivStressValue(Id_field_equiv_stress, World); }
                if (Id_field_stress != 0) { Solid.SetStressValue(Id_field_stress, World); }
                using (ConstUIntDoublePairVectorIndexer indexer = Solid.GetAry_ItrNormRes())
                {
                    if (indexer.Count() > 0)
                    {
                        Console.WriteLine("Iter : {0} Res : {1}", Solid.GetAry_ItrNormRes()[0].First, Solid.GetAry_ItrNormRes()[0].Second);
                    }
                }
                //World.FieldValueDependExec();
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
        /// 問題を設定する
        /// </summary>
        /// <returns></returns>
        private bool setNewProblem()
        {
            bool success = false;
            
            try
            {
                if (ProbNo == 0)
                {
                     // linear Solid stationary analysis
                    Id_field_disp_fix0 = 0;
                    Id_field_temp = 0;
                    Id_field_stress = 0;
                    Id_field_equiv_stress = 0;
                    ////////////////
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {
                        {
                            // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(5.0,0.0) );
                            pts.Add( new CVector2D(5.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            cad2d.AddPolygon( pts );
                        }
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.1) );
                        conv = World.GetIDConverter(id_base);
                    }

                    Solid.Clear();
                    Solid.UpdateDomain_Field(id_base, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // Setting Material Parameter
                    Solid.SetYoungPoisson(10.0,0.3,true);    // planter stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
            
                    uint id_field_bc0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE),World);
                
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0,World);
                    FieldValueSetter.SetMathExp("sin(t*PI*2*0.1)", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
            
                    // Setting Visualiziation
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 1 )    // save stiffness matrix for the efficency of computation
                {
                    Solid.SetSaveStiffMat(true);
                }
                else if( ProbNo == 2 )    // non-stationary analysis
                {
                    Solid.SetSaveStiffMat(true);
                    Solid.SetStationary(false);
                }
                else if( ProbNo == 3 )    // set stiffer material
                {
                    Solid.SetYoungPoisson(50,0.3,true);
                }
                else if( ProbNo == 4 )    // set more stiffer material
                {
                    Solid.SetYoungPoisson(100,0.3,true);
                }
                else if( ProbNo == 5 )    // geometrical non-linear stationaly
                {
                    Solid.SetStationary(true);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 6 )    // geometrical non-linear non-stationary
                {
                    Solid.SetYoungPoisson(10,0.0,true);
                    Solid.SetStationary(false);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 7 )    // display equivalent stress field in deformedconfigulation
                {
                    Id_field_equiv_stress = World.MakeField_FieldElemDim(Id_field_disp, 2,
                        DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetStationary(true);
                    // set up visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_equiv_stress,false,World,Id_field_equiv_stress, 0,0.5) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // init view transformation
                }
                else if( ProbNo == 8 )    // display equivalent stress field in initial configulation
                {
                    // set up visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World,Id_field_equiv_stress, 0,0.5) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // init view transformation
                }
                else if( ProbNo == 9 )    // thermal-Solid analysis
                {
                    Id_field_equiv_stress = 0;
                    Id_field_stress = 0;
                    ////////////////
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using (CCadObj2D cad2d = new CCadObj2D())
                    {
                         {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(3.0,0.0) );
                            pts.Add( new CVector2D(3.0,1.0) );
                            pts.Add( new CVector2D(2.0,1.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            cad2d.AddPolygon( pts );
                        }
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.1) );
                        conv = World.GetIDConverter(id_base);
                    }
                    Solid.UpdateDomain_Field(id_base,World);
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(true);
                    // set material property
                    Solid.SetYoungPoisson(10.0,0.3,true);    // set planer stress
                    Solid.SetGeometricalNonlinear(false);    // geometricaly linear model
                    Solid.SetGravitation(0.0,-0.1);
                    Solid.SetTimeIntegrationParameter(Dt);    // set time setp
                    
                    uint id_field_bc0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(6,CAD_ELEM_TYPE.EDGE),World);
            
                    // set temparature field
                    Id_field_temp = World.MakeField_FieldElemDim(Id_field_disp, 2,
                        DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.CORNER);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(Id_field_temp,World);
                    FieldValueSetter.SetMathExp("sin(6.28*y)*sin(x)*sin(t)", 0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
                
                    Solid.SetThermalStress(Id_field_temp);
                    Solid.ClearFixElemAry(3,World);
            
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false, World, Id_field_temp, -1,1) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 10 )    // show contour in undeformed configuration
                {
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_temp,true, World, Id_field_temp, -1,1) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 11 )    // stop considering thermal-effect
                {
                    Solid.SetThermalStress(0);
                }
                else if( ProbNo == 12 )
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;        // get ID converter
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        {    // define shape
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
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.05) );
                        conv = World.GetIDConverter(id_base);        // get ID converter
                    }
            
                    Solid.SetDomain_FieldEA(id_base,conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP),World);
                    Solid.SetSaveStiffMat(true);
                    Solid.SetStationary(true);
                    Solid.SetTimeIntegrationParameter(Dt);    // set time step
                    Solid.SetYoungPoisson(2.5,0.3,true);    // planer stress
                    Solid.SetGeometricalNonlinear(false);    // set geometrical liner
                    Solid.SetGravitation(0.0,0.0);    // set gravitation
            
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc2 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1,World);
                    FieldValueSetter.SetMathExp("0.3*sin(1.5*t)", 0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.1*(cos(t)+1)", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
            
                    // set visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 13 )
                {
                    Solid.SetSaveStiffMat(true);
                }
                else if( ProbNo == 14 )
                {
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(false);
                }
                else if( ProbNo == 15 ){
                    Solid.SetStationary(true);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 16 ){
                    Solid.SetStationary(false);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 17 )    // hard and soft Solid are connected
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(0.5,0.0)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(0.5,1.0)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1,id_v2);
                        
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.05) );
                        conv = World.GetIDConverter(id_base);  // get ID converter
                    }
                    
                    Solid.SetDomain_FieldEA(id_base,conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP),World);
                    Solid.SetTimeIntegrationParameter(Dt);
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(true);
                    
                    Solid.SetYoungPoisson(3.0,0.3,true);
                    Solid.SetGeometricalNonlinear(false);    // set geometrically linear model
                    Solid.SetGravitation(0.0,0.0);

                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc2 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1,World);
                    FieldValueSetter.SetMathExp("0.3*sin(1.5*t)",     0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.1*(cos(t)+1)+0.1", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis

                    // set up visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    //DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.PushBack( new CDrawerEdge(id_base,true,World) );
                    DrawerAry.InitTrans(Camera);    // set view trnsformation
                }
                else if( ProbNo == 18 )
                {
                    Solid.SetSaveStiffMat(true);
                }
                else if( ProbNo == 19 )
                {
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(false);
                }
                else if( ProbNo == 20 )
                {
                    Solid.SetStationary(true);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 21 )
                {
                    Solid.SetStationary(false);
                    Solid.SetGeometricalNonlinear(true);
                }
                else if( ProbNo == 22 )    // 4 different type of Solid combined
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.5) );
                        pts.Add( new CVector2D(0.0,0.5) );
                        cad2d.AddPolygon( pts );
                        uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(1.5,0.0)).id_v_add;
                        uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(1.0,0.0)).id_v_add;
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(0.5,0.0)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(0.5,0.5)).id_v_add;
                        uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(1.0,0.5)).id_v_add;
                        uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(1.5,0.5)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1,id_v2);
                        cad2d.ConnectVertex_Line(id_v3,id_v4);
                        cad2d.ConnectVertex_Line(id_v5,id_v6);

                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.05) );
                        conv = World.GetIDConverter(id_base);  // get ID converter
                    }
                    
                    Solid.UpdateDomain_Field(id_base,World);    // set domain of Solid analysis
                    Solid.SetTimeIntegrationParameter(Dt);    // set time step
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(true);
                    // set material property
                    Solid.SetYoungPoisson(1.0,0.3,true);
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,-0.0);

                    {    // St.Venant-Kirchhoff material
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP));
                        eqn.SetGeometricalNonlinear(true);
                        Solid.SetEquation(eqn);
                    }
                    {    // soft elastic material
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP));
                        eqn.SetYoungPoisson(0.1,0.3,true);
                        Solid.SetEquation(eqn);
                    }
                    uint Id_field_temp = World.MakeField_FieldElemAry(id_base, conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.LOOP),
                        DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.CORNER);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(Id_field_temp,World);
                    FieldValueSetter.SetMathExp("0.1*sin(3.14*4*y)*sin(2*t)", 0,FIELD_DERIVATION_TYPE.VALUE, World);
                    {    // linear elastic material concidering thrmal-Solid
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.LOOP));
                        eqn.SetThermalStress(Id_field_temp);
                        Solid.SetEquation(eqn);
                    }
                    {    // hard elastic material
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.LOOP));
                        eqn.SetYoungPoisson(10,0.3,true);
                        Solid.SetEquation(eqn);
                    }
                    
                    Id_field_disp_fix0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    
                    // set up visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    //DrawerAry.PushBack( new CDrawerEdge(id_base,false,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 23 )
                {
                    Solid.SetRho(0.0001);
                    Solid.SetStationary(false);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(Id_field_disp_fix0,World);
                    FieldValueSetter.SetMathExp("0.5*cos(2*t)", 1,FIELD_DERIVATION_TYPE.VALUE, World);
                }
                else if( ProbNo == 24 )
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.0) );
                        pts.Add( new CVector2D(2.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,1, new CVector2D(1.0,0.0)).id_v_add;
                        uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE,3, new CVector2D(1.0,1.0)).id_v_add;
                        cad2d.ConnectVertex_Line(id_v1,id_v2);
                        
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.05) );
                        conv = World.GetIDConverter(id_base);  // ID converter
                    }
                    Solid.UpdateDomain_Field(id_base,World);    // set displacement field
                    Solid.SetTimeIntegrationParameter(Dt);    // set time step
                    Solid.SetSaveStiffMat(false);
                    Solid.SetStationary(false);
                    // set material property
                    Solid.SetYoungPoisson(1.0,0.3,true);    // planter stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,-0.0);
                    Solid.SetRho(0.001);
                    
                    {    // soft elastic material
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP));
                        eqn.SetYoungPoisson(0.1,0.3,true);
                        Solid.SetEquation(eqn);
                    }
                    {    // hard elastic material
                        DelFEM4NetFem.Eqn.CEqn_Solid2D eqn = Solid.GetEquation(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.LOOP));
                        eqn.SetYoungPoisson(100000000,0.3,true);
                        Solid.SetEquation(eqn);
                    }
                    
                    //Id_field_disp_fix0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,1),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1,World);
                    FieldValueSetter.SetMathExp("0.3*sin(1.5*t)",     0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.1*(cos(t)+1)+0.1", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis    
                    
                    // set up visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.InitTrans(Camera);    // initialize view transmation
                }
                else if( ProbNo == 25 )
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    uint id_base2 = 0;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        uint id_l;
                        uint id_e1,id_e2,id_e3,id_e4,id_e5;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(0.2,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            id_l = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.2,0.5) ).id_v_add;
                            id_e1 = cad2d.ConnectVertex_Line(2,id_v1).id_e_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5,0.2) ).id_v_add;
                            uint id_v3 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5,0.5) ).id_v_add;
                            uint id_v4 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.5,0.8) ).id_v_add;
                            uint id_v5 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.8,0.5) ).id_v_add;
                            uint id_v6 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(0.3,0.5) ).id_v_add;
                            id_e2 = cad2d.ConnectVertex_Line(id_v2,id_v3).id_e_add;
                            id_e3 = cad2d.ConnectVertex_Line(id_v3,id_v4).id_e_add;
                            id_e4 = cad2d.ConnectVertex_Line(id_v3,id_v5).id_e_add;
                            id_e5 = cad2d.ConnectVertex_Line(id_v3,id_v6).id_e_add;
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d,0.1))
                        {
                            World.Clear();
                            id_base = World.AddMesh(mesh2d);
                            conv = World.GetIDConverter(id_base);  // get ID converter
                    
                            {    // cut mesh
                                IList<uint> mapVal2Co = null;
                                IList< IList<int> > aLnods = null;
                                {
                                    IList<uint> aIdMsh_Inc = new List<uint>();
                                    aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l,CAD_ELEM_TYPE.LOOP) );
                                    IList<uint> aIdMshCut = new List<uint>();
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e1,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e2,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e3,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e4,CAD_ELEM_TYPE.EDGE) );
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e5,CAD_ELEM_TYPE.EDGE) );
                                    mesh2d.GetClipedMesh(out aLnods, out mapVal2Co, aIdMsh_Inc,aIdMshCut);
                                }
                                IList<uint> aIdEA_Inc = new List<uint>();
                                aIdEA_Inc.Add( conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.LOOP) );
                                id_base2 = World.SetCustomBaseField(id_base,aIdEA_Inc, aLnods, mapVal2Co);
                            }
                        } // mesh2d
                    } // cad2d

                    Solid.UpdateDomain_Field(id_base2, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // set material parameter
                    Solid.SetYoungPoisson(10.0,0.3,true);
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
            
                    uint id_field_bc0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(3,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0,World);
                    FieldValueSetter.SetMathExp("0.1*sin(t*PI*2*0.1)",    0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.1*(1-cos(t*PI*2*0.1))",1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
                
                    // set visualization
                    DrawerAry.Clear();
                    Id_field_disp = Solid.GetIdField_Disp();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set view transformation
                }
                else if( ProbNo == 26 )
                {
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    uint id_base2 = 0;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {
                        uint id_e;
                        uint id_l;
                        {    // define shape
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(5.0,0.0) );
                            pts.Add( new CVector2D(5.0,2.0) );
                            pts.Add( new CVector2D(0.0,2.0) );
                            id_l = cad2d.AddPolygon( pts ).id_l_add;
                            uint id_v1 = cad2d.AddVertex(CAD_ELEM_TYPE.EDGE, 3, new CVector2D(2.5,2.0)).id_v_add;
                            uint id_v2 = cad2d.AddVertex(CAD_ELEM_TYPE.LOOP, id_l, new CVector2D(2.5,1.0)).id_v_add;
                            id_e = cad2d.ConnectVertex_Line(id_v1,id_v2).id_e_add;
                        }
                        using(CMesher2D mesh2d = new CMesher2D(cad2d,0.2))
                        {
                            World.Clear();
                            CurTime = 0;
                            id_base = World.AddMesh(mesh2d);
                            conv = World.GetIDConverter(id_base);
                            id_base2 = 0;
                            {    // cut mesh
                                IList<uint> mapVal2Co = new List<uint>();
                                IList< IList<int> > aLnods = new List<IList<int>>();
                                {
                                    IList<uint> aIdMsh_Inc = new List<uint>();
                                    aIdMsh_Inc.Add( mesh2d.GetElemID_FromCadID(id_l,CAD_ELEM_TYPE.LOOP) );
                                    IList<uint> aIdMshCut = new List<uint>();
                                    aIdMshCut.Add( mesh2d.GetElemID_FromCadID(id_e,CAD_ELEM_TYPE.EDGE) );
                                    mesh2d.GetClipedMesh(out aLnods, out mapVal2Co, aIdMsh_Inc,aIdMshCut);
                                }
                                IList<uint> aIdEA_Inc = new List<uint>();
                                aIdEA_Inc.Add( conv.GetIdEA_fromCad(id_l,CAD_ELEM_TYPE.LOOP) );
                                id_base2 = World.SetCustomBaseField(id_base,aIdEA_Inc, aLnods,mapVal2Co);
                            }
                        } // mesh2d
                    } // cad2d
            
                    Solid.UpdateDomain_Field(id_base2, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // set material parameter
                    Solid.SetYoungPoisson(10.0,0.3,true);    // set planer stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
            
                    uint id_field_bc0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0,World);
                    FieldValueSetter.SetMathExp("0.5*(1-cos(t*PI*2*0.1))", 0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.2*sin(t*PI*2*0.1)",     1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
                    
                    Id_field_disp = Solid.GetIdField_Disp();
                    Id_field_equiv_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                        DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    
                    // set up visualization
                    DrawerAry.Clear();
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World,Id_field_equiv_stress) );
                    //DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // set transformation
                }    
                else if( ProbNo == 27 )
                {    
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(3.0,0.0) );
                        pts.Add( new CVector2D(3.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.3) );
                        conv = World.GetIDConverter(id_base);    // Get ID converter
                    }
                    
                    Solid.Clear();
                    Solid.UpdateDomain_Field(id_base, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // Setting Material Parameter
                    Solid.SetYoungPoisson(2,0.3,true);    // planter stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
                    
                    uint id_field_bc0 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE),World);
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(4,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc0,World);
                    FieldValueSetter.SetMathExp("0.5*sin(t*PI*2*0.1)",    0,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis
                    FieldValueSetter.SetMathExp("0.3*(1-cos(t*PI*2*0.1))",1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field y axis
                
                    Id_field_disp = Solid.GetIdField_Disp();
                    Id_field_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                        DelFEM4NetFem.Field.FIELD_TYPE.STSR2,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    //Id_field_equiv_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                    //    DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                    //    DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                    //    DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    
                    // set up visualization
                    DrawerAry.Clear();
                    //DrawerAry.PushBack( new CDrawerFace(Id_field_equiv_stress,false,World,Id_field_equiv_stress, 0,0.5) );
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerVector(Id_field_stress,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // init view transformation
                }
                else if( ProbNo == 28 )
                {    
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(0.2,0.0) );
                        pts.Add( new CVector2D(0.2,0.5) );
                        pts.Add( new CVector2D(0.8,0.5) );
                        pts.Add( new CVector2D(0.8,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.7) );
                        pts.Add( new CVector2D(0.6,0.7) );
                        pts.Add( new CVector2D(0.4,0.7) );
                        pts.Add( new CVector2D(0.0,0.7) );
                        cad2d.AddPolygon( pts );
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.1) );
                        conv = World.GetIDConverter(id_base);    // Get ID converter
                    }
                    
                    Solid.Clear();
                    Solid.UpdateDomain_Field(id_base, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // Setting Material Parameter
                    Solid.SetYoungPoisson(2,0.1,true);    // planter stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
                    
                    uint id_field_bc0;
                    {
                        IList<uint> aIdEA = new List<uint>();
                        aIdEA.Add(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE));
                        aIdEA.Add(conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE));
                        id_field_bc0 = Solid.AddFixElemAry(aIdEA,World);
                    }
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(8,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1,World);
                    FieldValueSetter.SetMathExp("-0.03*(1-cos(t*PI*2*0.1))", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis    
                    
                    Id_field_disp = Solid.GetIdField_Disp();
                    Id_field_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                        DelFEM4NetFem.Field.FIELD_TYPE.STSR2,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    //Id_field_equiv_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                    //    DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                    //    DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                    //    DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);

                    // set up visualization
                    DrawerAry.Clear();        
                    //DrawerAry.PushBack( new CDrawerFace(Id_field_equiv_stress,false,World,Id_field_equiv_stress, 0,0.5) );
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerVector(Id_field_stress,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // init view transformation
                }
                else if( ProbNo == 29 )
                {    
                    uint id_base = 0;
                    CIDConvEAMshCad conv = null;
                    using(CCadObj2D cad2d = new CCadObj2D())
                    {    // define shape
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(0.2,0.0) );
                        pts.Add( new CVector2D(0.2,0.5) );
                        pts.Add( new CVector2D(0.8,0.5) );
                        pts.Add( new CVector2D(0.8,0.3) );
                        pts.Add( new CVector2D(1.0,0.3) );
                        pts.Add( new CVector2D(1.0,0.7) );
                        pts.Add( new CVector2D(0.6,0.7) );
                        pts.Add( new CVector2D(0.4,0.7) );
                        pts.Add( new CVector2D(0.0,0.7) );
                        cad2d.AddPolygon( pts );
                        World.Clear();
                        id_base = World.AddMesh( new CMesher2D(cad2d,0.1) );
                        conv = World.GetIDConverter(id_base);    // Get ID converter
                    }
                    
                    Solid.Clear();
                    Solid.UpdateDomain_Field(id_base, World);
                    Solid.SetSaveStiffMat(false);    
                    Solid.SetStationary(true);
                    // Setting Material Parameter
                    Solid.SetYoungPoisson(2,0.1,true);    // planter stress
                    Solid.SetGeometricalNonlinear(false);
                    Solid.SetGravitation(0.0,0.0);
                    Solid.SetTimeIntegrationParameter(Dt,0.7);
                    
                    uint id_field_bc0;
                    {
                        IList<uint> aIdEA = new List<uint>();
                        aIdEA.Add(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE));
                        //aIdEA.Add(conv.GetIdEA_fromCad(5,CAD_ELEM_TYPE.EDGE));
                        id_field_bc0 = Solid.AddFixElemAry(aIdEA,World);
                    }
                    uint id_field_bc1 = Solid.AddFixElemAry(conv.GetIdEA_fromCad(8,CAD_ELEM_TYPE.EDGE),World);
                    if (FieldValueSetter != null)
                    {
                        FieldValueSetter.Clear();
                        FieldValueSetter.Dispose();
                    }
                    FieldValueSetter = new CFieldValueSetter(id_field_bc1,World);
                    FieldValueSetter.SetMathExp("-0.03*(1-cos(t*PI*2*0.1))", 1,FIELD_DERIVATION_TYPE.VALUE, World);    // oscilate bc1_field x axis        
                    
                    Id_field_disp = Solid.GetIdField_Disp();
                    Id_field_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                        DelFEM4NetFem.Field.FIELD_TYPE.STSR2,
                        DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                        DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    //Id_field_equiv_stress = World.MakeField_FieldElemDim(Id_field_disp,2,
                    //    DelFEM4NetFem.Field.FIELD_TYPE.SCALAR,
                    //    DelFEM4NetFem.Field.FIELD_DERIVATION_TYPE.VALUE,
                    //    DelFEM4NetFem.Field.ELSEG_TYPE.BUBBLE);
                    
                    // set up visualization
                    DrawerAry.Clear();        
                    //DrawerAry.PushBack( new CDrawerFace(Id_field_equiv_stress,false,World,Id_field_equiv_stress, 0,0.5) );
                    DrawerAry.PushBack( new CDrawerFace(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerVector(Id_field_stress,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,false,World) );
                    DrawerAry.PushBack( new CDrawerEdge(Id_field_disp,true ,World) );
                    DrawerAry.InitTrans(Camera);    // init view transformation
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
