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

namespace scalar3d
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   scalar 3D
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
        IList<CFieldValueSetter> FieldValueSetterAry = null;
        /// <summary>
        ///  スカラー3D方程式オブジェクト
        CEqn_Scalar3D EqnScalar = null;
        
        /// <summary>
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
        /// フィールド値のフィールドID
        /// </summary>
        //private uint Id_field_val;
        /// <summary>
        /// ベースのフィールドID
        /// </summary>
        private uint Id_base = 0;
        /// <summary>
        /// 境界(0)のフィールドID
        /// </summary>
        //private uint Id_val_bc0 = 0;
        /// <summary>
        /// 境界(1)のフィールドID
        /// </summary>
        //private uint Id_val_bc1 = 0;
        /// <summary>
        /// 境界(2)のフィールドID
        /// </summary>
        //private uint Id_val_bc2 = 0;

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
            FieldValueSetterAry = new List<CFieldValueSetter>();
            EqnScalar = new CEqn_Scalar3D();

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
            if (World != null)
            {
                World.Clear();
                World.Dispose();
                World = null;
            }
            if (FieldValueSetterAry != null)
            {
                clearFieldValueSetterAry(FieldValueSetterAry);
                FieldValueSetterAry = null;
            }
            if (EqnScalar != null)
            {
                EqnScalar.Clear();
                EqnScalar.Dispose();
                EqnScalar = null;
            }
        }
        
        /// <summary>
        /// フィールド値セッターのリストをクリアする
        /// </summary>
        private static void clearFieldValueSetterAry(IList<CFieldValueSetter> fieldValueSetterAry)
        {
            foreach (CFieldValueSetter fvs in fieldValueSetterAry)
            {
                fvs.Clear();
                fvs.Dispose();
            }
            fieldValueSetterAry.Clear();
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

            // glutの初期設定
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
            Glut.glutCreateWindow("FEM View");
            
            // コールバック関数の設定
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutSpecialFunc(myGlutSpecial);
            Glut.glutIdleFunc(myGlutIdle);
            
            // 問題設定
            setNewProblem();
            
            // メインループ
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
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            if (IsAnimation)
            {
                CurTime += Dt;
                foreach (CFieldValueSetter fvs in FieldValueSetterAry)
                {
                    fvs.ExecuteValue(CurTime, World);
                }
                EqnScalar.Solve(World);
                ConstUIntDoublePairVectorIndexer res = EqnScalar.GetAry_ItrNormRes();
                if (res.Count > 0)
                {
                    Console.WriteLine("Iter : " + res[0].First + "  Res : " + res[0].Second);
                }
                DrawerAry.Update(World);
            }

            DelFEM4NetCom.GlutUtility.ShowFPS(ref Frame, ref Timebase, ref Stringfps);
            DrawerAry.Draw();
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
            Camera.MouseRotation(MovBeginX,MovBeginY,movEndX,movEndY);
            //Camera.MousePan(MovBeginX, MovBeginY, movEndX, movEndY);
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
            Console.WriteLine("setNewProblem {0}", ProbNo);
            try
            {

                if ( ProbNo == 0 )
                {
                    ////////////////
                    using (CCadObj2D cad2d = new CCadObj2D())
                     {    // 形を作る
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        cad2d.AddPolygon( pts );
                        // メッシュを作る
                        using (CMesh3D_Extrude msh3d = new CMesh3D_Extrude())
                        {
                            msh3d.Extrude(new CMesher2D(cad2d, 0.1), 1.0, 0.1);
                            World.Clear();
                            Id_base = World.AddMesh(msh3d);
                        }
                    }
                    CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                    // 方程式の設定
                    EqnScalar.SetDomain(Id_base,World);
                    EqnScalar.SetAlpha(1.0);
                    EqnScalar.SetTimeIntegrationParameter(Dt);
                    uint id_val_bc0 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE,1),World);
                    uint id_val_bc1 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromCad(2,CAD_ELEM_TYPE.EDGE,2),World);
                    uint id_val_bc2 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromCad(1,CAD_ELEM_TYPE.EDGE,2),World);
                    clearFieldValueSetterAry(FieldValueSetterAry);
                    {
                        CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc0,World);
                        fvs.SetMathExp("cos(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE,World);
                        FieldValueSetterAry.Add(fvs);
                    }
                    CFieldValueSetter.SetFieldValue_Constant(id_val_bc1,0,FIELD_DERIVATION_TYPE.VALUE,World, 1.0);
                    {
                        CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc2,World);
                        fvs.SetMathExp("sin(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE,World);
                        FieldValueSetterAry.Add(fvs);
                    }
                    // 描画オブジェクトの登録
                    DrawerAry.Clear();
                    uint id_field_val = EqnScalar.GetIdField_Value();
                    DrawerAry.PushBack( new CDrawerFace(id_field_val,true,World, id_field_val,-1.0,1.0) );
                    DrawerAry.PushBack( new CDrawerEdge(id_field_val,true,World) );
                    DrawerAry.InitTrans(Camera);
                }
                else if ( ProbNo == 1 )
                {
                    string mshfn = "../../../input_file/rect_quad.msh";
                    if (File.Exists(mshfn))
                    {
                        Id_base = 0;
                        using (CMesher2D mesh2d = new CMesher2D())
                        using(CMesh3D_Extrude mesh3d = new CMesh3D_Extrude())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            mesh3d.Extrude(mesh2d, 5.0, 0.5);
                            World.Clear();
                            Id_base = World.AddMesh(mesh3d);
                        }
                        CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                        // 方程式の設定
                        EqnScalar.SetDomain(Id_base, World);
                        EqnScalar.SetAlpha(1.0);
                        EqnScalar.SetTimeIntegrationParameter(Dt);
                        uint id_val_bc0 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(1, 1), World);
                        uint id_val_bc1 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(3, 2), World);
                        uint id_val_bc2 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(1, 3), World);

                        clearFieldValueSetterAry(FieldValueSetterAry);
                        {
                            CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc0, World);
                            fvs.SetMathExp("cos(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE, World);
                            FieldValueSetterAry.Add(fvs);
                        }
                        CFieldValueSetter.SetFieldValue_Constant(id_val_bc1, 0, FIELD_DERIVATION_TYPE.VALUE, World, 1.0);
                        {
                            CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc2, World);
                            fvs.SetMathExp("sin(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE, World);
                            FieldValueSetterAry.Add(fvs);
                        }
                        // 描画オブジェクトの登録
                        DrawerAry.Clear();
                        uint id_field_val = EqnScalar.GetIdField_Value();
                        DrawerAry.PushBack(new CDrawerFace(id_field_val, true, World, id_field_val, -1.0, 1.0));
                        DrawerAry.PushBack(new CDrawerEdge(id_field_val, true, World));
                        DrawerAry.InitTrans(Camera);
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if( ProbNo == 2 )
                {
                    string mshfn = "../../../input_file/hexa_tri.msh";
                    if (File.Exists(mshfn))
                    {
                        using (CMesher2D mesh2d = new CMesher2D())
                        using (CMesh3D_Extrude mesh3d = new CMesh3D_Extrude())
                        {
                            mesh2d.ReadFromFile_GiDMsh(mshfn);
                            mesh3d.Extrude(mesh2d, 5.0, 0.5);
                            World.Clear();
                            Id_base = World.AddMesh(mesh3d);
                        }
                        CIDConvEAMshCad conv = World.GetIDConverter(Id_base);
                        // 方程式の設定
                        EqnScalar.SetDomain(Id_base, World);
                        EqnScalar.SetAlpha(1.0);
                        EqnScalar.SetTimeIntegrationParameter(Dt);
                        uint id_val_bc0 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(10, 2), World);
                        uint id_val_bc1 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(1, 1), World);
                        uint id_val_bc2 = EqnScalar.AddFixElemAry(conv.GetIdEA_fromMshExtrude(11, 2), World);
                        clearFieldValueSetterAry(FieldValueSetterAry);
                        {
                            CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc0, World);
                            fvs.SetMathExp("cos(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE, World);
                            FieldValueSetterAry.Add(fvs);
                        }
                        CFieldValueSetter.SetFieldValue_Constant(id_val_bc1, 0, FIELD_DERIVATION_TYPE.VALUE, World, 1.0);
                        {
                            CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc2, World);
                            fvs.SetMathExp("sin(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE, World);
                            FieldValueSetterAry.Add(fvs);
                        }
                        // 描画オブジェクトの登録
                        DrawerAry.Clear();
                        uint id_field_val = EqnScalar.GetIdField_Value();
                        DrawerAry.PushBack(new CDrawerFace(id_field_val, true, World, id_field_val, -1.0, 1.0));
                        DrawerAry.PushBack(new CDrawerEdge(id_field_val, true, World));
                        DrawerAry.InitTrans(Camera);
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
                }
                else if ( ProbNo == 3 )
                {
                    string mshfn = "../../../input_file/cylinder_tet.msh";
                    if (File.Exists(mshfn))
                    {
                        using (CMesher3D mesh3d = new CMesher3D())
                        {
                            mesh3d.ReadFromFile_GiDMsh(mshfn);
                            World.Clear();
                            Id_base = World.AddMesh(mesh3d);
                        }
                        // 方程式の設定
                        EqnScalar.SetDomain(Id_base, World);
                        EqnScalar.SetAlpha(1.0);
                        EqnScalar.SetTimeIntegrationParameter(Dt);
                        uint id_val_bc0 = EqnScalar.AddFixElemAry(3, World);
                        uint id_val_bc1 = EqnScalar.AddFixElemAry(4, World);
                        clearFieldValueSetterAry(FieldValueSetterAry);
                        {
                            CFieldValueSetter fvs = new CFieldValueSetter(id_val_bc0, World);
                            fvs.SetMathExp("cos(2*PI*t+0.1)", 0, FIELD_DERIVATION_TYPE.VALUE, World);
                            FieldValueSetterAry.Add(fvs);
                        }
                        CFieldValueSetter.SetFieldValue_Constant(id_val_bc1, 0, FIELD_DERIVATION_TYPE.VALUE, World, 0.0);
                        // 描画オブジェクトの登録
                        DrawerAry.Clear();
                        uint id_field_val = EqnScalar.GetIdField_Value();
                        DrawerAry.PushBack(new CDrawerFace(id_field_val, true, World, id_field_val, -1.0, 1.0));
                        DrawerAry.PushBack(new CDrawerEdge(id_field_val, true, World));
                        DrawerAry.InitTrans(Camera);
                    }
                    else
                    {
                        Console.WriteLine("not exist:{0}", mshfn);
                        MessageBox.Show(string.Format("メッシュファイル:{0}がありません", mshfn));
                    }
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
