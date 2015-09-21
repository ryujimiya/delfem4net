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

namespace cad2d_edit
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   CAD 2D edit
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
        private const int ProbCnt = 9;

        ///   0:drag
        ///   1:drag curve
        ///   2:add point
        ///   3:delete point
        ///   4:smooth curve
        private  enum ModeDv
        {
            Drag,
            DragCurve,
            AddPoint,
            DeletePoint,
            SmoothCurve
        };

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
        /// Cad移動オブジェクト
        /// </summary>
        private CCadObj2D_Move Cad2D = null;
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
        /// 描画オブジェクトアレイ
        /// </summary>
        private CDrawerArray DrawerAry = null;
        /// <summary>
        /// Cad描画オブジェクト
        /// </summary>
        private CDrawer_Cad2D Drawer = null;
        /// <summary>
        /// 押されたマウスのボタン
        /// </summary>
        private int PressButton = 0;
        /// <summary>
        /// 選択されたCADパーツのID
        /// </summary>
        private uint Id_part_cad = 0;
        /// <summary>
        /// 選択されたCADパーツの要素タイプ
        /// </summary>
        private CAD_ELEM_TYPE ElemType_part_cad = CAD_ELEM_TYPE.NOT_SET;
        /// <summary>
        /// 動作モード
        ///   0:drag
        ///   1:drag curve
        ///   2:add point
        ///   3:delete point
        ///   4:smooth curve
        /// </summary>
        private ModeDv Mode = ModeDv.Drag;
        /// <summary>
        /// 図面が更新された？
        /// </summary>
        private bool IsUpdatedCad = false;
        /// <summary>
        /// ストロークするときの点のリスト
        /// </summary>
        IList< CVector2D > AVecStrok = null;
        /// <summary>
        /// ループID
        ///     問題5と6の間で使用
        /// </summary>
        uint Id_l0 = 0;

        /// <summary>
        /// メニューがクリックされた?
        /// </summary>
        private bool isMenuItemClicked = false;

        /// <summary>
        /// 問題番号
        /// </summary>
        //private int ProbNo = 0;
        private int ProbNo = 8;

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
            Cad2D = new CCadObj2D_Move();
            DrawerAry = new CDrawerArray();
            Drawer = null; // インスタンスを生成しない
            AVecStrok = new List<CVector2D>();

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
            if (Drawer != null)
            {
               // DrawerAryのクリアで破棄処理が実行されているので破棄の必要はない。nullにセットするだけ。
               Drawer = null;
            }
            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }
            if (AVecStrok != null)
            {
                clearAVec(AVecStrok);
                AVecStrok = null;
            }
        }

        /// <summary>
        /// CVector2Dリストのクリア
        /// </summary>
        private static void clearAVec(IList<CVector2D> aVec)
        {
            foreach (CVector2D vec in aVec)
            {
                vec.Dispose();
            }
            aVec.Clear();
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
            string helpStr = 
@"-------------------------------
key assign:
  0:drag
  1:drag curve
  2:add point
  3:delete point
  4:smooth curve
-------------------------------";
            Console.WriteLine(helpStr);

            MyTimer.Enabled = true;

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
            Glut.glutCreateWindow("MSH View");

            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutPassiveMotionFunc(myGlutPassiveMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutSpecialFunc(myGlutSpecial);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            //Glut.glutIdleFunc(myGlutIdle);

            Glut.glutCreateMenu(myGlutMenu);
            Glut.glutAddMenuEntry("EditCurve", 1);
            Glut.glutAddMenuEntry("ChangeToLine", 2);
            Glut.glutAddMenuEntry("ChangeToArc", 3);
            Glut.glutAddMenuEntry("ChangeToPolyline", 4);
            Glut.glutAttachMenu(Glut.GLUT_RIGHT_BUTTON);
            
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
            ///   0:drag
            ///   1:drag curve
            ///   2:add point
            ///   3:delete point
            ///   4:smooth curve
            if (Mode == ModeDv.Drag) { Gl.glClearColor(0.2f, .7f, .7f, 1.0f); }
            else if ( Mode == ModeDv.DragCurve ){ Gl.glClearColor(0.7f, .2f, .7f ,1.0f); }
            else if ( Mode == ModeDv.AddPoint ){ Gl.glClearColor(0.7f, .7f, .2f ,1.0f); }
            else if ( Mode == ModeDv.DeletePoint ){ Gl.glClearColor(0.7f, .2f, .2f ,1.0f); }
            else if ( Mode == ModeDv.SmoothCurve ){ Gl.glClearColor(0.2f, .7f, .2f ,1.0f); }
            
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);

            if( IsUpdatedCad ){
                IsUpdatedCad = false;
                //DrawerAry.Clear();
                //if (Drawer != null)
                //{
                //    Drawer = null;
                //}
                //Drawer = new CDrawer_Cad2D(Cad2D);
                //DrawerAry.PushBack( Drawer );
                Drawer.UpdateCAD_Geometry(Cad2D);
            }

            {
                Gl.glBegin(Gl.GL_LINE_STRIP);
                foreach (CVector2D pp in AVecStrok)
                {
                    Gl.glVertex2d( pp.x, pp.y );
                }
                Gl.glEnd();
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
        /// Glut：マウス移動(マウスボタンを離したまま移動)イベントハンドラ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void myGlutPassiveMotion( int x, int y )
        {
            { // hilight cad element
                int sizeBuffer = 128;
                DelFEM4NetCom.View.DrawerGlUtility.PickSelectBuffer pickSelectBuffer = null;
                DelFEM4NetCom.View.DrawerGlUtility.PickPre((uint)sizeBuffer, out pickSelectBuffer, (uint)x, (uint)y, 5, 5, Camera);
                DrawerAry.DrawSelection();
                List<DelFEM4NetCom.View.SSelectedObject> aSelecObj = (List<DelFEM4NetCom.View.SSelectedObject>)DelFEM4NetCom.View.DrawerGlUtility.PickPost(pickSelectBuffer, (uint)x, (uint)y, Camera);
                DrawerAry.ClearSelected();
                
                uint id_part_cad0 = 0;
                CAD_ELEM_TYPE elemType_part_cad0 = CAD_ELEM_TYPE.NOT_SET;
                if( aSelecObj.Count > 0 )
                {
                     Drawer.GetCadPartID(aSelecObj[0].name, ref elemType_part_cad0, ref id_part_cad0);
                     DrawerAry.AddSelected( aSelecObj[0].name );
                }
                else
                {
                    elemType_part_cad0 = CAD_ELEM_TYPE.NOT_SET;
                    id_part_cad0 = 0;
                }
            }
            if( Mode == ModeDv.Drag ) return;
            
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            double movEndX = (2.0 * x - winW) / winW;
            double movEndY = (winH - 2.0 * y) / winH;
            /*
            if( Mode == ModeDv.DragCurve ){    // CurveEditMode
                if( ElemType_part_cad == CAD_ELEM_TYPE.EDGE && Cad2D.IsElemID(CAD_ELEM_TYPE.EDGE,Id_part_cad) )
                {
                    CVector3D oloc = Camera.ProjectionOnPlane(movEndX, movEndY);
                    bool res  = Cad2D.DragArc(Id_part_cad, new CVector2D(oloc.x,oloc.y));
                    IsUpdatedCad = true;
                }
            }
            */
        }

        /// <summary>
        /// Glut：マウスドラッグ(マウスボタンを押したまま移動)イベントハンドラ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void myGlutMotion(int x, int y)
        {
            if ( Mode != ModeDv.Drag ){

                { // hilight
                    int sizeBuffer = 128;
                    DelFEM4NetCom.View.DrawerGlUtility.PickSelectBuffer pickSelectBuffer = null;
                    DelFEM4NetCom.View.DrawerGlUtility.PickPre((uint)sizeBuffer, out pickSelectBuffer, (uint)x, (uint)y, 5, 5, Camera);
                    DrawerAry.DrawSelection();
                    List<DelFEM4NetCom.View.SSelectedObject> aSelecObj = (List<DelFEM4NetCom.View.SSelectedObject>)DelFEM4NetCom.View.DrawerGlUtility.PickPost(pickSelectBuffer, (uint)x, (uint)y, Camera);
                    DrawerAry.ClearSelected();
                
                    uint id_part_cad0 = 0;
                    CAD_ELEM_TYPE elemType_part_cad0 = CAD_ELEM_TYPE.NOT_SET;
                    if( aSelecObj.Count > 0 )
                    {
                        Drawer.GetCadPartID(aSelecObj[0].name, ref elemType_part_cad0, ref id_part_cad0);
                        DrawerAry.AddSelected( aSelecObj[0].name );
                    }
                    else
                    {
                        elemType_part_cad0 = CAD_ELEM_TYPE.NOT_SET;
                        id_part_cad0 = 0;
                    }
                }
            }
            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            double movEndX = (2.0 * x - winW) / winW;
            double movEndY = (winH - 2.0 * y) / winH;

            if ( PressButton == Glut.GLUT_MIDDLE_BUTTON )
            {
                Camera.MousePan(MovBeginX, MovBeginY, movEndX, movEndY);
            }
            else if ( PressButton == Glut.GLUT_LEFT_BUTTON )
            {
                if (Mode == ModeDv.Drag)
                {    // MoveMode
                    if( ElemType_part_cad == CAD_ELEM_TYPE.VERTEX && Cad2D.IsElemID(CAD_ELEM_TYPE.VERTEX, Id_part_cad) )
                    {
                        CVector3D oloc = Camera.ProjectionOnPlane(movEndX,movEndY);
                        bool res = Cad2D.MoveVertex(Id_part_cad, new CVector2D(oloc.x,oloc.y));
                        IsUpdatedCad = true;
                    }
                    if( ElemType_part_cad == CAD_ELEM_TYPE.EDGE && Cad2D.IsElemID(CAD_ELEM_TYPE.EDGE, Id_part_cad) )
                    {
                        CVector3D oloc0 = Camera.ProjectionOnPlane(MovBeginX, MovBeginY);
                        CVector3D oloc1 = Camera.ProjectionOnPlane(movEndX,   movEndY);
                        bool res = Cad2D.MoveEdge(Id_part_cad, new CVector2D(oloc1.x-oloc0.x, oloc1.y-oloc0.y) );
                        IsUpdatedCad = true;
                    }
                    if( ElemType_part_cad == CAD_ELEM_TYPE.LOOP && Cad2D.IsElemID(CAD_ELEM_TYPE.LOOP, Id_part_cad) )
                    {
                        CVector3D oloc0 = Camera.ProjectionOnPlane(MovBeginX,MovBeginY);
                        CVector3D oloc1 = Camera.ProjectionOnPlane(movEndX, movEndY);
                        bool res = Cad2D.MoveLoop(Id_part_cad, new CVector2D(oloc1.x-oloc0.x, oloc1.y-oloc0.y));
                        IsUpdatedCad = true;
                    }
                }
                else if (Mode == ModeDv.DragCurve)
                {
                    if( ElemType_part_cad == CAD_ELEM_TYPE.EDGE && Cad2D.IsElemID(ElemType_part_cad, Id_part_cad) )
                    {
                        CVector3D oloc = Camera.ProjectionOnPlane(movEndX, movEndY);
                        if(      Cad2D.GetEdgeCurveType(Id_part_cad) == CURVE_TYPE.CURVE_ARC )
                        {
                            bool res = Cad2D.DragArc(Id_part_cad, new CVector2D(oloc.x,oloc.y));
                        }
                        else if (Cad2D.GetEdgeCurveType(Id_part_cad) == CURVE_TYPE.CURVE_POLYLINE)
                        {
                            bool res = Cad2D.DragPolyline(Id_part_cad, new CVector2D(oloc.x,oloc.y));
                        }
                        IsUpdatedCad = true;
                    }
                }
                else if ( Mode == ModeDv.AddPoint )
                {    // sketch strok
                    CVector3D oloc = Camera.ProjectionOnPlane(movEndX, movEndY);
                    AVecStrok.Add( new CVector2D(oloc.x, oloc.y) );
                }
                else if ( Mode == ModeDv.SmoothCurve )
                { // smooth strok
                    if ( ElemType_part_cad == CAD_ELEM_TYPE.EDGE && Cad2D.IsElemID(ElemType_part_cad, Id_part_cad) )
                    {
                        CVector3D oloc = Camera.ProjectionOnPlane(movEndX, movEndY);
                        Cad2D.SmoothingPolylineEdge(
                            Id_part_cad, 3,
                            new CVector2D(oloc.x,oloc.y), 0.1);
                        IsUpdatedCad = true;
                    }
                }
            }
            
            ////////////////
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
            //Console.WriteLine("myGlutMouse");
            // ryujimiya
            // メニューが左クリックでメニューイベントハンドラが処理された後、MouseUpのイベントが発生する
            // それは望んでいるイベントではないと思われるので除外するようにした。
            if (isMenuItemClicked)
            {
                isMenuItemClicked = false;
                //Console.WriteLine("skipped: state {0}", state);
                return;
            }

            int[] viewport = new int[4];
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            MovBeginX = (2.0 * x - winW) / winW;
            MovBeginY = (winH - 2.0 * y) / winH;
            PressButton = button;

            uint id_part_cad0 = Id_part_cad;
            CAD_ELEM_TYPE elemType_part_cad0 = ElemType_part_cad;
            { // hilight the picked element
                int sizeBuffer = 128;
                DelFEM4NetCom.View.DrawerGlUtility.PickSelectBuffer pickSelectBuffer = null;
                DelFEM4NetCom.View.DrawerGlUtility.PickPre((uint)sizeBuffer, out pickSelectBuffer, (uint)x, (uint)y, 5, 5, Camera);
                DrawerAry.DrawSelection();
                List<DelFEM4NetCom.View.SSelectedObject> aSelecObj = (List<DelFEM4NetCom.View.SSelectedObject>)DelFEM4NetCom.View.DrawerGlUtility.PickPost(pickSelectBuffer, (uint)x, (uint)y, Camera);
                DrawerAry.ClearSelected();
                
                Id_part_cad = 0;
                elemType_part_cad0 = CAD_ELEM_TYPE.NOT_SET;
                if( aSelecObj.Count > 0 )
                {
                     Drawer.GetCadPartID(aSelecObj[0].name, ref ElemType_part_cad, ref Id_part_cad);
                     DrawerAry.AddSelected( aSelecObj[0].name );
                }
                else
                {
                    ElemType_part_cad = CAD_ELEM_TYPE.NOT_SET;
                    Id_part_cad = 0;
                }
            }
            //Console.WriteLine("ElemType_part_cad: {0}  Id_part_cad: {1}", ElemType_part_cad, Id_part_cad);

            if ( state == Glut.GLUT_DOWN )
            {
                if (Mode == ModeDv.DragCurve)
                { 
                    if( ElemType_part_cad == CAD_ELEM_TYPE.EDGE && Cad2D.IsElemID(ElemType_part_cad,Id_part_cad) )
                    {
                        CVector3D oloc = Camera.ProjectionOnPlane(MovBeginX, MovBeginY);
                        if( Cad2D.GetEdgeCurveType(Id_part_cad) == CURVE_TYPE.CURVE_POLYLINE )
                        {
                            bool res = Cad2D.PreCompDragPolyline(Id_part_cad, new CVector2D(oloc.x,oloc.y));
                        }
                        return;
                    }
                }
                else if( Mode == ModeDv.AddPoint )
                {   // add point
                    clearAVec(AVecStrok);
                    if( ElemType_part_cad == CAD_ELEM_TYPE.VERTEX )
                    {
                        return;
                    }
                    CVector3D oloc = Camera.ProjectionOnPlane(MovBeginX, MovBeginY);
                    CVector2D v0 = new CVector2D(oloc.x, oloc.y);
                    uint id_v0 = Cad2D.AddVertex(ElemType_part_cad, Id_part_cad, v0).id_v_add;
                    ElemType_part_cad = CAD_ELEM_TYPE.VERTEX;
                    Id_part_cad = id_v0;
                    if( !Cad2D.IsElemID(CAD_ELEM_TYPE.VERTEX, id_v0) )
                    {
                        clearAVec(AVecStrok);
                        return;
                    }
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer ); 
                }
                else if( Mode == ModeDv.DeletePoint )
                {   // delete point
                    if( Cad2D.IsElemID(ElemType_part_cad, Id_part_cad) )
                    {
                        bool iflag = Cad2D.RemoveElement(ElemType_part_cad, Id_part_cad);
                        if( iflag )
                        {
                            DrawerAry.Clear();
                            if (Drawer != null)
                            {
                                Drawer = null;
                            }
                            Drawer = new CDrawer_Cad2D(Cad2D);
                            DrawerAry.PushBack( Drawer ); 
                        }
                    }
                    Mode = ModeDv.Drag;
                    Console.WriteLine("Mode:{0}", Mode);
                }
            }
            if( state == Glut.GLUT_UP )
            {
                if (Mode == ModeDv.AddPoint)
                {
                    //Mode = Mode.Drag;
                    //Console.WriteLine("Mode:{0}", Mode);
                    if (elemType_part_cad0 != CAD_ELEM_TYPE.VERTEX || !Cad2D.IsElemID(CAD_ELEM_TYPE.VERTEX, id_part_cad0))
                    {
                        clearAVec(AVecStrok);
                        return;
                    }
                    uint id_v0 = id_part_cad0;
                    uint id_v1 = 0;
                    if( ElemType_part_cad == CAD_ELEM_TYPE.VERTEX )
                    {
                        if( !Cad2D.IsElemID(CAD_ELEM_TYPE.VERTEX, Id_part_cad) )
                        {
                            clearAVec(AVecStrok);
                            return;
                        }
                        id_v1 = Id_part_cad;
                    }
                    else
                    {
                        CVector2D v1 = AVecStrok[AVecStrok.Count - 1];
                        id_v1 = Cad2D.AddVertex(ElemType_part_cad, Id_part_cad, v1).id_v_add;
                        if( !Cad2D.IsElemID(CAD_ELEM_TYPE.VERTEX,id_v1) )
                        {
                            clearAVec(AVecStrok);
                            return;
                        }
                    }
                    CEdge2D e = new CEdge2D(id_v0,id_v1);
                    {
                        //e.itype = CURVE_TYPE.CURVE_POLYLINE;
                       int n = AVecStrok.Count;
                        CVector2D pos = Cad2D.GetVertexCoord(e.GetIdVtx(true));
                        CVector2D poe = Cad2D.GetVertexCoord(e.GetIdVtx(false));
                        e.SetVtxCoords(pos, poe);
                        double sqlen = CVector2D.SquareLength(poe - pos);
                        CVector2D eh = (poe - pos) * (1/sqlen);
                        CVector2D ev = new CVector2D(-eh.y, eh.x);
                        {
                            IList<double> aRelCo = new List<double>();
                             foreach (CVector2D pp in AVecStrok)
                             {
                                 double x1 = CVector2D.Dot(pp - pos, eh);
                                 double y1 = CVector2D.Dot(pp - pos, ev);
                                 aRelCo.Add(x1);
                                 aRelCo.Add(y1);
                             }
                             e.SetCurve_Polyline(aRelCo);
                        }
                        {
                            IList<CVector2D> aVecDiv = new List<CVector2D>();
                            int ndiv = (int)((double)e.GetCurveLength() / (Camera.GetHalfViewHeight() * 0.04) + 1);
                            e.GetCurveAsPolyline(ref aVecDiv, ndiv);
                            IList<double> aRelCo = new List<double>();
                            foreach (CVector2D pp in aVecDiv)
                            {
                                double x1 = CVector2D.Dot(pp - pos, eh);
                                double y1 = CVector2D.Dot(pp - pos, ev);
                                aRelCo.Add(x1);
                                aRelCo.Add(y1);
                            }
                            e.SetCurve_Polyline(aRelCo);
                        }
                    }
                    Cad2D.ConnectVertex(e);
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer ); 
                    clearAVec(AVecStrok);
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
            ///   0:drag
            ///   1:drag curve
            ///   2:add point
            ///   3:delete point
            ///   4:smooth curve
            else if (key == getSingleByte('0'))
            {
                Mode = ModeDv.Drag;
                Console.WriteLine("Mode:{0}", Mode);
            }
            else if (key == getSingleByte('1'))
            {
                Mode = ModeDv.DragCurve;
                Console.WriteLine("Mode:{0}", Mode);
            }
            else if (key == getSingleByte('2'))
            {
                Mode = ModeDv.AddPoint;
                Console.WriteLine("Mode:{0}", Mode);
            }
            else if (key == getSingleByte('3'))
            {
                Mode = ModeDv.DeletePoint;
                Console.WriteLine("Mode:{0}", Mode);
            }
            else if (key == getSingleByte('4'))
            {
                Mode = ModeDv.SmoothCurve;
                Console.WriteLine("Mode:{0}", Mode);
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
            isMenuItemClicked = true;
            if (value == 1)
            {
                Console.WriteLine("EditCurve");
                Mode = ModeDv.DragCurve;
                Console.WriteLine("Mode:{0}", Mode);
                return;
            }
            else if (value == 2)
            {
                // ChangeToLine
                if( ElemType_part_cad != CAD_ELEM_TYPE.EDGE )
                {
                    return;
                }
                Console.WriteLine("ChangeToLine");
                IsUpdatedCad = Cad2D.SetCurve_Line(Id_part_cad);
                Glut.glutPostRedisplay();
                return;
            }
            else if (value == 3)
            {
                // ChangeToArc
                if( ElemType_part_cad != CAD_ELEM_TYPE.EDGE )
                {
                    return;
                }
                Console.WriteLine("ChangeToArc");
                {
                    uint id_vs, id_ve;
                    Cad2D.GetIdVertex_Edge(out id_vs, out id_ve, Id_part_cad);
                    CVector2D vs = Cad2D.GetVertexCoord(id_vs);
                    CVector2D ve = Cad2D.GetVertexCoord(id_ve);
                    double dist = Math.Sqrt( CVector2D.SquareLength(vs, ve) ) * 10.0;
                    IsUpdatedCad = Cad2D.SetCurve_Arc(Id_part_cad, false, dist);
                    Glut.glutPostRedisplay();
                }
                return;
            }
            else if (value == 4)
            {
                if( ElemType_part_cad != CAD_ELEM_TYPE.EDGE )
                {
                    return;
                }
                Console.WriteLine("ChangeToPolyline");
                Cad2D.SetCurve_Polyline(Id_part_cad);
                Glut.glutPostRedisplay();
                return;
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
                    Cad2D.Clear();
                    {    // Make model
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.0) );
                        pts.Add( new CVector2D(2.0,1.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(1.0,2.0) );
                        pts.Add( new CVector2D(0.0,2.0) );
                        uint id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                    }
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 1 )
                {
                    Cad2D.Clear();
                    {    // Make model
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.5,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        uint id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                    }
                    Cad2D.SetCurve_Arc(1,false,-0.2);
                    Cad2D.SetCurve_Arc(2,false, 0.5);
                    //Cad2D.SetCurve_Arc(3,false,-0.5);
                    //Cad2D.SetCurve_Arc(4,true, -0.5);
                    Cad2D.ConnectVertex_Line(1,2);
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 2 )
                {
                    Cad2D.Clear();
                    {    // Make model
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.5,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        uint id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                    }
                    Cad2D.SetCurve_Arc(1,false,  0);
                    //Cad2D.SetCurve_Arc(2,false,  0);
                    
                    //Cad2D.SetCurve_Arc(1,true, -1.0);
                    Cad2D.SetCurve_Arc(2,true, -1.0);
                    Cad2D.SetCurve_Arc(3,true,  -1.0);
                    //Cad2D.SetCurve_Arc(4,true, -0.5);
                    Cad2D.SetCurve_Arc(5,true,  -0.5);
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 3 )
                {
                    Cad2D.Clear();
                    {
                        uint id_l = 0;
                        {
                            IList<CVector2D> pts = new List<CVector2D>();
                            pts.Add( new CVector2D(0.0,0.0) );
                            pts.Add( new CVector2D(1.0,0.0) );
                            pts.Add( new CVector2D(1.0,1.0) );
                            pts.Add( new CVector2D(0.0,1.0) );
                            id_l = Cad2D.AddPolygon( pts ).id_l_add;
                        }
                        uint id_v1 = Cad2D.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.2)).id_v_add;
                        uint id_v2 = Cad2D.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.7,0.2)).id_v_add;
                        uint id_v3 = Cad2D.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.7,0.8)).id_v_add;
                        uint id_v4 = Cad2D.AddVertex(CAD_ELEM_TYPE.LOOP,id_l, new CVector2D(0.3,0.8)).id_v_add;
                        Cad2D.ConnectVertex_Line(id_v1,id_v2);
                        Cad2D.ConnectVertex_Line(id_v2,id_v3);
                        Cad2D.ConnectVertex_Line(id_v3,id_v4);
                        Cad2D.ConnectVertex_Line(id_v4,id_v1);
                    }
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 4 ){
                    Cad2D.Clear();
                    {
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        Cad2D.AddPolygon( pts );
                        IList<CVector2D> aRelCo = new List<CVector2D>();
                        for (int i = 0; i < 3; i++)
                        {
                            aRelCo.Add(new CVector2D());
                        }
                        {
                            aRelCo[0].x = 0.25;
                            aRelCo[0].y = -0.1;
                            aRelCo[1].x = 0.5;
                            aRelCo[1].y = -0.0;
                            aRelCo[2].x= 0.75;
                            aRelCo[2].y = -0.1;
                        }
                        Cad2D.SetCurve_Polyline(1, aRelCo);
                        Cad2D.SetCurve_Arc(2, true, -1);
                    }
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 5 )
                {
                    Cad2D.Clear();
                    uint id_l_l, id_l_r;
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
                        Id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                        uint id_e1 = Cad2D.ConnectVertex_Line(5,9).id_e_add;
                        Cad2D.ShiftLayer_Loop(Id_l0, true);
                        double[] col = new double[3]{ 0.9, 0.4, 0.4 };
                        Cad2D.SetColor_Loop(Id_l0, col);
                        Cad2D.AddVertex(CAD_ELEM_TYPE.EDGE, 3, new CVector2D(1.3,0.5) );
                        Cad2D.GetIdLoop_Edge(out id_l_l, out id_l_r, id_e1);
                    }
                    ////////////////
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    // ryujimiya:なぜかグローバルなDrawerへは格納していない。あとでチェック。-->格納するように変更
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    Drawer.SetPointSize(10);
                    DrawerAry.PushBack( Drawer);
                    DrawerAry.InitTrans(Camera);
                }
                else if( ProbNo == 6 )
                {
                    Cad2D.ShiftLayer_Loop(Id_l0,false);
                    Cad2D.ShiftLayer_Loop(Id_l0,false);
                    ////////////////
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    // ryujimiya:なぜかグローバルなDrawerへは格納していない。あとでチェック。-->格納するように変更
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    Drawer.SetPointSize(10);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans(Camera);
                }
                else if( ProbNo == 7 )
                {
                    Cad2D.Clear();
                    {    // Make model
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(2.0,0.0) );
                        pts.Add( new CVector2D(3.0,0.0) );
                        pts.Add( new CVector2D(3.0,1.0) );
                        pts.Add( new CVector2D(2.0,1.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        uint id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                    }
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
                }
                else if( ProbNo == 8 )
                {
                    Cad2D.Clear();
                    {    // Make model
                        IList<CVector2D> pts = new List<CVector2D>();
                        pts.Add( new CVector2D(0.0,0.0) );
                        pts.Add( new CVector2D(1.0,0.0) );
                        pts.Add( new CVector2D(1.0,1.0) );
                        pts.Add( new CVector2D(0.5,1.0) );
                        pts.Add( new CVector2D(0.0,1.0) );
                        uint id_l0 = Cad2D.AddPolygon( pts ).id_l_add;
                    }
                    Cad2D.SetCurve_Bezier(1,0.2,+0.5, 0.2,-0.5);
                    Cad2D.SetCurve_Bezier(2,0.5,+0.5, 0.2,-0.5);
                    Cad2D.ConnectVertex_Line(1,2);
                    DrawerAry.Clear();
                    if (Drawer != null)
                    {
                        Drawer = null;
                    }
                    Drawer = new CDrawer_Cad2D(Cad2D);
                    DrawerAry.PushBack( Drawer );
                    DrawerAry.InitTrans( Camera );
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
