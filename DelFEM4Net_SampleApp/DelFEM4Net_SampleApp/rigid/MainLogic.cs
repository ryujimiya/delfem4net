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
using DelFEM4NetRigid; // 剛体
using DelFEM4NetLs; // 剛体リニアシステム
using Tao.FreeGlut;
using Tao.OpenGl;

namespace rigid
{
    // original header:
    //   none. probably like this
    ////////////////////////////////////////////////////////
    //   rigid
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
        /// <summary>
        /// Newmarkβ法のγ
        /// </summary>
        private const double NewmarkGamma = 0.7;
        /// <summary>
        /// Newmarkβ法のβ
        /// </summary>
        private const double NewmarkBeta = 0.25 * (0.5 + NewmarkGamma) * (0.5 + NewmarkGamma);
        /// <summary>
        ///
        /// </summary>
        private static readonly CVector3D Gravity = new CVector3D(0, 0, -1.0);

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
        /// 押されたマウスのボタン
        /// </summary>
        private int PressButton = 0;

        /// <summary>
        /// 剛体3Dのリスト
        /// </summary>
        private IList<CRigidBody3D> ARB = null;
        /// <summary>
        /// 剛体の拘束リスト
        /// </summary>
        private IList<CConstraint> AFix = null;
        /// <summary>
        /// アニメーションする?
        /// </summary>
        private bool IsAnimation = true;

        /// <summary>
        /// 現在の時間
        /// </summary>
        private double CurTime = 0;
        /// <summary>
        /// 時間刻み幅
        /// </summary>
        private double Dt = 0.05;

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

        /*DEBUG
        private static void printLsMat(CLinearSystem_RigidBody_CRS2 ls)
        {
            //for check
            using (CMatDia_BlkCrs mat = ls.GetMatrix())
            {
                uint nblk = mat.NBlkMatCol();
                for (uint iblk = 0; iblk < nblk; iblk++)
                {
                    //エラーになる(m_DiaValPtr == 0でなければならないらしい)
                    //DoubleArrayIndexer ptr = mat.GetPtrValDia(iblk);
                    //for (int i = 0; i < ptr.Count; i++)
                    //{
                    //    Console.WriteLine("[" + iblk + "]" + "(" + i + ") = " + ptr[i]);
                    //}
                    uint npsup = 0;
                    ConstUIntArrayIndexer ptrInd = mat.GetPtrIndPSuP(iblk, out npsup);
                    DoubleArrayIndexer ptrVal = mat.GetPtrValPSuP(iblk, out npsup);
                    for (int i = 0; i < npsup; i++)
                    {
                        Console.WriteLine("GetPtrValPSuP [{0}] ( {1} ) < {2} > ( {3} )" , iblk , i, ptrInd[i], ptrVal[i]);
                    }
                }
            }
            // for check 更新ベクトル
            using (CVector_Blk upd = ls.GetVector(-2))
            {
                for (uint iblk = 0; iblk < upd.NBlk(); iblk++)
                {
                    double val = upd.GetValue(iblk, 0);
                    System.Console.WriteLine("vec(-2)[" + iblk + "] = " + val);
                }
            }
            // for check 残差ベクトル
            using (CVector_Blk residual = ls.GetVector(-1))
            {
                for (uint iblk = 0; iblk < residual.NBlk(); iblk++)
                {
                    double val = residual.GetValue(iblk, 0);
                    System.Console.WriteLine("vec(-1)[" + iblk + "] = " + val);
                }
            }
        }
        */
        private void StepTime3()
        {
            using(CLinearSystem_RigidBody_CRS2 ls = new CLinearSystem_RigidBody_CRS2(ARB,AFix))
            using (CPreconditioner_RigidBody_CRS2 prec = new CPreconditioner_RigidBody_CRS2())
            {
                prec.SetLinearSystem(ls);
                ////////////////
                ls.InitializeMarge();
                //printLsMat(ls);
                ls.UpdateValueOfRigidSystem(ref ARB, ref AFix, Dt, NewmarkGamma, NewmarkBeta, true);
                //printLsMat(ls);
                double norm_res0 = 0.0;
                for (uint itr = 0; itr < 10; itr++)
                {
                    ls.InitializeMarge();
                    ILinearSystem_RigidBody baseLs = ls as ILinearSystem_RigidBody;
                    for (int irb = 0; irb < ARB.Count; irb++)
                    {
                        ARB[irb].AddLinearSystem(ref baseLs, (uint)irb, Dt, NewmarkGamma, NewmarkBeta, Gravity, itr == 0);
                    }
                    for (int ifix = 0; ifix < AFix.Count; ifix++)
                    {
                        AFix[ifix].AddLinearSystem(ref baseLs, (uint)ifix, Dt, NewmarkGamma, NewmarkBeta, ARB, itr == 0);
                    }
                    ////////////////////////////////
                    double res = ls.FinalizeMarge();
                    //printLsMat(ls);

                    if (res < 1.0e-30) break;
                    if (itr == 0)
                    {
                        norm_res0 = res;
                    }
                    Console.WriteLine("itr : " + itr + "     Residual : " + res);
                    //ls.ReSizeTmpVecSolver(1);
                    //ls.COPY(-1,0);
                    //ls.COPY(-1,-2);
                    {
                        ls.COPY(-1, -2);
                        prec.SetValue(ls);
                        CVector_Blk_Ptr updVecPtr = ls.GetVectorPtr(-2);
                        CVector_Blk updVec = updVecPtr;
                        prec.Solve(ref updVec);
                    }
                    //ls.MATVEC(-1.0, -2, 1, 0);
                    //Console.WriteLine("dot : " + ls.DOT(0,0));
                    //prec.SetValue(ls);
                    //ls2.Solve(prec);
                    /*
                    {
                        double conv_ratio = 1.0e-6;
                        uint max_iter = 1000;
                        prec.SetValue(ls);
                        CLinearSystemPreconditioner_RigidBody_CRS2 lsp(ls,prec);
                        Sol::Solve_PBiCGSTAB(conv_ratio,max_iter,lsp);
                        //Sol::Solve_BiCGSTAB(conv_ratio,max_iter,ls);
                        //Console.WriteLine("       solver itr : " + max_iter + "  conv : " + conv_ratio);
                    }
                    */
                    ////////////////////////////////
                    ls.UpdateValueOfRigidSystem(ref ARB, ref AFix, Dt, NewmarkGamma, NewmarkBeta, false);
                    if (res < norm_res0 * 1.0e-8) return;
                }
            }
        }

        /// <summary>
        /// 剛体を描画する
        /// </summary>
        /// <param name="r"></param>
        private static void DrawRigidBody(CRigidBody3D r)
        {
            uint imode = 1;
            if ( imode == 0 )
            {
            }
            else if ( imode == 1 )
            {
                Gl.glPushMatrix();
                Gl.glTranslated( 
                    r.ini_pos_cg.x + r.disp_cg.x,
                    r.ini_pos_cg.y + r.disp_cg.y,
                    r.ini_pos_cg.z + r.disp_cg.z );
                {
                    double[] rot0 = new double[16];
                    r.GetInvRotMatrix44(ref rot0);
                    Gl.glMultMatrixd(rot0);
                }
                Gl.glColor3d(0,0,0);
                Glut.glutSolidCube(0.2);
                //Glut.glutSolidTeapot(1.0);
                //Glut.glutWireTeapot(0.2);
                //Glut.glutWireOctahederon();
                //Glut.glutWireDodecahedron();
                Gl.glLineWidth(1);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glColor3d(1,0,0);
                Gl.glVertex3d(0,0,0);
                Gl.glVertex3d(0.4,0,0);
                Gl.glColor3d(0,1,0);
                Gl.glVertex3d(0,0,0);
                Gl.glVertex3d(0,0.4,0);
                Gl.glColor3d(0,0,1);
                Gl.glVertex3d(0,0,0);
                Gl.glVertex3d(0,0,0.4);
                Gl.glEnd();
                Gl.glPopMatrix();
            }
        }
        
        /// <summary>
        /// 剛体と剛体を結ぶ拘束を描画する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="ARB"></param>
        private static void DrawConstraint(CConstraint c, IList<CRigidBody3D> ARB)
        {
            if (c is CFix_Hinge)
            {
                CFix_Hinge cfh = c as CFix_Hinge;
                //Console.WriteLine("CFix_Hinge");
                uint irb = cfh.aIndRB[0];
                CRigidBody3D rb = ARB[(int)irb];
                CVector3D vec_j = rb.GetPositionFromInital(cfh.ini_pos_fix);
                CVector3D vec_cg = rb.ini_pos_cg + rb.disp_cg;
                CVector3D ini_pos_fix = cfh.ini_pos_fix;
        
                Gl.glPushMatrix();
                Gl.glTranslated(ini_pos_fix.x, ini_pos_fix.y, ini_pos_fix.z);
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
        
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                Gl.glVertex3d(vec_cg.x,vec_cg.y,vec_cg.z);
                Gl.glEnd();
        
                CVector3D lcb0 = cfh.loc_coord[0];
                CVector3D lcb1 = cfh.loc_coord[1];
                uint ndiv = 16;
                double Dtheta = 2*3.1416/ndiv;
                double radius = 0.4;
                Gl.glColor3d(0,1,1);
                Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                for (uint idiv = 0; idiv < ndiv + 1; idiv++)
                {
                    CVector3D v0 = vec_j + Math.Sin(idiv * Dtheta) * lcb0 * radius + Math.Cos(idiv * Dtheta) * lcb1 * radius;
                    Gl.glVertex3d(v0.x,  v0.y,  v0.z);
                }
                Gl.glEnd();
                return;
            }
            else if(c is CFix_HingeRange)
            {
                CFix_HingeRange cfhr = c as CFix_HingeRange;
                //Console.WriteLine("CFix_HingeRange" );
                uint irb = cfhr.aIndRB[0];
                CRigidBody3D rb = ARB[(int)irb];
                CVector3D ini_pos_fix = cfhr.ini_pos_fix;
                CVector3D vec_j = rb.GetPositionFromInital(ini_pos_fix);
                CVector3D vec_cg = rb.ini_pos_cg + rb.disp_cg;
        
                Gl.glPushMatrix();
                Gl.glTranslated(ini_pos_fix.x, ini_pos_fix.y, ini_pos_fix.z);
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
        
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                Gl.glVertex3d(vec_cg.x,vec_cg.y,vec_cg.z);
                Gl.glEnd();
        
                double max_t=cfhr.max_t;
                double min_t=cfhr.min_t;
                CVector3D lcb0 = cfhr.loc_coord[0];
                CVector3D lcb1 = cfhr.loc_coord[1];
                uint ndiv_t = 32;
                uint ndiv0 = (uint)((double)ndiv_t * (max_t - min_t) / 360.0 + 1);
                double Dtheta0 = 2*3.1416/ndiv0*(max_t-min_t)/360.0;
                double radius = 1;
                Gl.glColor3d(0,1,1);
                Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                for (uint idiv = 0; idiv < ndiv0 + 1; idiv++)
                {
                    CVector3D v0 = vec_j + 
                        Math.Sin(idiv * Dtheta0 - max_t * 3.14 / 180) * lcb0 * radius + 
                        Math.Cos(idiv * Dtheta0 - max_t * 3.14 / 180) * lcb1 * radius;
                    Gl.glVertex3d(v0.x,  v0.y,  v0.z);
                }
                Gl.glEnd();
                return;
            }
            else if(c is CFix_Spherical)
            {
                CFix_Spherical cfs = c as CFix_Spherical;
                //Console.WriteLine("CFix_HingeSpherical");
                uint irb = cfs.aIndRB[0];
                CRigidBody3D rb = ARB[(int)irb];
                CVector3D ini_pos_fix = cfs.ini_pos_fix;
                CVector3D vec_j = rb.GetPositionFromInital(ini_pos_fix);
                CVector3D vec_cg = rb.ini_pos_cg + rb.disp_cg;
        
                Gl.glPushMatrix();
                Gl.glTranslated(ini_pos_fix.x, ini_pos_fix.y, ini_pos_fix.z);
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
        
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x, vec_j.y, vec_j.z);
                Gl.glVertex3d(vec_cg.x, vec_cg.y, vec_cg.z);
                Gl.glEnd();
                return;
            }
            else if(c is CJoint_Hinge)
            {
                CJoint_Hinge cjh = c as CJoint_Hinge;
                //Console.WriteLine("CJoint_Hinge");
                uint irb0 = cjh.aIndRB[0];
                uint irb1 = cjh.aIndRB[1];
                CRigidBody3D rb0 = ARB[(int)irb0];
                CRigidBody3D rb1 = ARB[(int)irb1];
                CVector3D vec_j = rb0.GetPositionFromInital(cjh.ini_pos_joint);
                CVector3D vec_cg0 = rb0.ini_pos_cg + rb0.disp_cg;
                CVector3D vec_cg1 = rb1.ini_pos_cg + rb1.disp_cg;
        
                Gl.glPushMatrix();
                Gl.glTranslated( vec_j.x, vec_j.y, vec_j.z);
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x, vec_j.y, vec_j.z);
                Gl.glVertex3d(vec_cg0.x,vec_cg0.y,vec_cg0.z);
                Gl.glVertex3d(vec_j.x, vec_j.y, vec_j.z);
                Gl.glVertex3d(vec_cg1.x, vec_cg1.y, vec_cg1.z);
                Gl.glEnd();
            
                CMatrix3 mrot = rb0.GetRotMatrix();
                CVector3D lcb0 = mrot.MatVec(cjh.loc_coord[0]);
                CVector3D lcb1 = mrot.MatVec(cjh.loc_coord[1]);
                uint ndiv = 16;
                double Dtheta = 2*3.1416/ndiv;
                double radius = 0.4;
                Gl.glColor3d(0,1,1);
                Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                for (uint idiv = 0; idiv < ndiv + 1; idiv++)
                {
                    CVector3D v0 = vec_j + Math.Sin(idiv*Dtheta) * lcb0 * radius + Math.Cos(idiv*Dtheta) * lcb1 * radius;
                    Gl.glVertex3d(v0.x, v0.y, v0.z);
                }
                Gl.glEnd();
                return;
            }
            else if(c is CJoint_HingeRange)
            {
                CJoint_HingeRange cjhr = c as CJoint_HingeRange;
                //Console.WriteLine("CJoint_HingeRange");
                uint irb0 = cjhr.aIndRB[0];
                uint irb1 = cjhr.aIndRB[1];
                CRigidBody3D rb0 = ARB[(int)irb0];
                CRigidBody3D rb1 = ARB[(int)irb1];
                CVector3D vec_j = rb0.GetPositionFromInital(cjhr.ini_pos_joint);
                CVector3D vec_cg0 = rb0.ini_pos_cg + rb0.disp_cg;
                CVector3D vec_cg1 = rb1.ini_pos_cg + rb1.disp_cg;
        
                Gl.glPushMatrix();
                Gl.glTranslated( vec_j.x, vec_j.y, vec_j.z );
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x, vec_j.y, vec_j.z);
                Gl.glVertex3d(vec_cg0.x,vec_cg0.y,vec_cg0.z);
                Gl.glVertex3d(vec_j.x, vec_j.y, vec_j.z);
                Gl.glVertex3d(vec_cg1.x,vec_cg1.y,vec_cg1.z);
                Gl.glEnd();
            
                CMatrix3 mrot = rb0.GetRotMatrix();
                CVector3D lcb0 = mrot.MatVec(cjhr.loc_coord[0]);
                CVector3D lcb1 = mrot.MatVec(cjhr.loc_coord[1]);
                uint ndiv_t = 32;
                double max_t=cjhr.max_t;
                double min_t=cjhr.min_t;
                uint ndiv0 = (uint)((double)ndiv_t*(max_t-min_t)/360.0 + 1);
                double Dtheta = 2*3.1416/ndiv0*(max_t-min_t)/360.0;
                double radius = 1;
                Gl.glColor3d(0,1,1);
                Gl.glBegin(Gl.GL_TRIANGLE_FAN);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                for (uint idiv=0; idiv < ndiv0 + 1; idiv++)
                {
                    CVector3D v0 = vec_j 
                        + Math.Sin(idiv*Dtheta - max_t*3.1416/180.0 )*lcb0*radius 
                        + Math.Cos(idiv*Dtheta - max_t*3.1416/180.0 )*lcb1*radius;
                    Gl.glVertex3d(v0.x,  v0.y,  v0.z);
                }
                Gl.glEnd();
                return;
            }
            else if(c is CJoint_Spherical)
            {
                CJoint_Spherical cjs = c as CJoint_Spherical;
                //Console.WriteLine("CJoint_Spherical");
                uint irb0 = cjs.aIndRB[0];
                CRigidBody3D rb0 = ARB[(int)irb0];
                CVector3D vec_cg0 = rb0.ini_pos_cg + rb0.disp_cg;
        
                uint irb1 = cjs.aIndRB[1];
                CRigidBody3D rb1 = ARB[(int)irb1];
                CVector3D vec_cg1 = rb1.ini_pos_cg + rb1.disp_cg;
            
                CVector3D vec_j = rb0.GetPositionFromInital(cjs.ini_pos_joint);
        
                Gl.glPushMatrix();
                Gl.glTranslated( vec_j.x, vec_j.y, vec_j.z );
                Gl.glColor3d(1,1,0);
                Glut.glutSolidSphere(0.1,10,5);
                Gl.glPopMatrix();
                Gl.glColor3d(1,1,1);
                Gl.glLineWidth(2);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                Gl.glVertex3d(vec_cg0.x,vec_cg0.y,vec_cg0.z);
                Gl.glVertex3d(vec_j.x,  vec_j.y,  vec_j.z);
                Gl.glVertex3d(vec_cg1.x,vec_cg1.y,vec_cg1.z);
                Gl.glEnd();
                return;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainLogic()
        {
            Disposed = false;
            Camera = new CCamera();
            ARB = new List<CRigidBody3D>();
            AFix = new List<CConstraint>();

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
            if (Camera != null)
            {
                Camera.Dispose();
                Camera = null;
            }
            if (ARB != null)
            {
                clearARB(ARB);
                ARB = null;
            }
            if (AFix != null)
            {
                clearAFix(AFix);
                AFix = null;
            }
        }
        
        /// <summary>
        /// 剛体リストをクリアする
        /// </summary>
        /// <param name="aRB">剛体リスト</param>
        void clearARB(IList<CRigidBody3D> aRB)
        {
            foreach (CRigidBody3D rb in aRB)
            {
                rb.Dispose();
            }
            aRB.Clear();
        }
        
        /// <summary>
        /// 剛体と剛体を結ぶ拘束のリストをクリアする
        /// </summary>
        /// <param name="aFix">剛体と剛体を結ぶ拘束のリスト</param>
        void clearAFix(IList<CConstraint> aFix)
        {
            foreach (CConstraint fix in aFix)
            {
                fix.Dispose();
            }
            aFix.Clear();
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
            Glut.glutCreateWindow("Cad View");
        
            // Set callback function
            Glut.glutMotionFunc(myGlutMotion);
            Glut.glutMouseFunc(myGlutMouse);
            Glut.glutDisplayFunc(myGlutDisplay);
            Glut.glutReshapeFunc(myGlutResize);
            Glut.glutKeyboardFunc(myGlutKeyboard);
            Glut.glutSpecialFunc(myGlutSpecial);
            Glut.glutIdleFunc(myGlutIdle);
        
            Camera.SetRotationMode(ROTATION_MODE.ROT_3D);
            //Camera.SetIsPers(true);
            {
                CBoundingBox3D bb = new CBoundingBox3D(-2,2,-2,2,-2,2);
                Camera.SetObjectBoundingBox(bb);
                Camera.Fit(bb);
            }
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
            Gl.glClearColor(0.2f, .7f, 0.7f, 1.0f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT|Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
        
            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL );
            Gl.glPolygonOffset( 1.1f, 4.0f );
        
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetModelViewTransform(Camera);
        
            if( IsAnimation )
            {
                StepTime3();    // solve rigid motion
                CurTime += Dt;
                // caliculation of kinematic energy
                double eng = 0;
                foreach (CRigidBody3D rb in ARB)
                {
                    double e = 0;
                    CVector3D omg = rb.Omega;
                    e += 0.5*(omg.x * omg.x * rb.mineatia[0]
                             +omg.y * omg.y * rb.mineatia[1]
                             +omg.z * omg.z * rb.mineatia[2]);
                    CVector3D velo = rb.velo_cg;
                    e += 0.5 * CVector3D.Dot(velo, velo) * rb.mass;
                    e -= CVector3D.Dot(Gravity, rb.disp_cg) * rb.mass;
                    eng += e;
                }
                Console.WriteLine("cur time " + CurTime + " " + eng );
            }
        
            DelFEM4NetCom.GlutUtility.ShowBackGround();
        
            /*
            {
                Gl.glColor3d(0.7,0.7,0.7);
                Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex3d(-2,-2,0);    Gl.glVertex3d( 2,-2,0);    Gl.glVertex3d( 2, 2,0);    Gl.glVertex3d(-2, 2,0);
                Gl.glEnd();
            }
            */
            {
                Gl.glLineWidth(1);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glColor3d(1,0,0);    Gl.glVertex3d(0,0,0);    Gl.glVertex3d(1,0,0);
                Gl.glColor3d(0,1,0);    Gl.glVertex3d(0,0,0);    Gl.glVertex3d(0,1,0);
                Gl.glColor3d(0,0,1);    Gl.glVertex3d(0,0,0);    Gl.glVertex3d(0,0,1);
                Gl.glEnd();
            }
        
            foreach (CRigidBody3D rb in ARB)
            {
                DrawRigidBody(rb);
            }
            foreach (CConstraint fix in AFix)
            {
                DrawConstraint(fix, ARB);
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

            if( PressButton == Glut.GLUT_MIDDLE_BUTTON )
            {
                Camera.MouseRotation(MovBeginX, MovBeginY, movEndX, movEndY);
            }
            else if( PressButton == Glut.GLUT_RIGHT_BUTTON )
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
            //Modifier = Glut.glutGetModifiers();
            PressButton = button;
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
            else if (key == getSingleByte('s'))
            {
               //StepTime();
            }
            else if (key == getSingleByte('c'))
            {
                clearARB(ARB);
                clearAFix(AFix);
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
                Camera.SetScale( Camera.GetScale()*0.9 );
            }
            else if (key == Glut.GLUT_KEY_PAGE_DOWN)
            {
                Camera.SetScale( Camera.GetScale()*1.1111 );
            }
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DrawerGlUtility.SetProjectionTransform(Camera);
            //Glut.glutPostRedisplay();
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
                clearAFix(AFix);
                clearARB(ARB);
                CurTime = 0;

                if( ProbNo == 0 )
                {
                    CRigidBody3D rb = new CRigidBody3D();
                    // Note: DelFEM4Netでは、irb.ini_pos_cgはコピーを取得するプロパティなので、変更したらセットする必要があります。
                    //rb.ini_pos_cg.SetVector(1.0,0,0);  // 変更はrbへは反映されません
                    rb.Set_ini_pos_cg_Vector(1.0, 0, 0);
                    ARB.Add(rb);
                    CFix_Spherical fix = new CFix_Spherical(0);
                    fix.SetIniPosFix(0,0,0);
                    AFix.Add( fix );
                }
                else if( ProbNo == 1 )
                {
                    double tot_len = 3.0;
                    uint nRB = 6; 
                    double div_len = tot_len / nRB;
                    for (uint irb = 0; irb < nRB; irb++)
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector(div_len*(irb+1),0,0);
                        ARB.Add(rb);
                        if ( irb == 0 )
                        {
                            CFix_Spherical fix = new CFix_Spherical(irb);
                            fix.SetIniPosFix(0,0,0);
                            AFix.Add( fix );
                        }
                        else
                        {
                            CJoint_Spherical fix = new CJoint_Spherical(irb - 1, irb);
                            fix.SetIniPosJoint(div_len * (irb + 0.5), 0, 0);
                            AFix.Add( fix );
                        }
                    }
                }
                else if( ProbNo == 2 )
                {
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector(1.0, 0, 0);
                        ARB.Add(rb);
                    }
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector(1.0, 1, 0);
                        ARB.Add(rb);
                    }
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector(2.0, 1, 0);
                        ARB.Add(rb);
                    }
                    {
                        CFix_Hinge fix = new CFix_Hinge(0);
                        fix.SetIniPosFix(0,0,0);
                        fix.SetAxis(0,1,0);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_Hinge fix = new CJoint_Hinge(0,1);
                        fix.SetIniPosJoint(1,0.5,0);
                        fix.SetAxis(1,0,0);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_Spherical fix = new CJoint_Spherical(1,2);
                        fix.SetIniPosJoint(1.5,1,0);
                        AFix.Add( fix );
                    }
                }
                else if( ProbNo == 3 )
                {
                    double tot_len = 3.0;
                    uint nRB = 3;
                    double div_len = tot_len / nRB;
                    for (uint irb=0; irb <nRB; irb++)
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector(div_len * (irb + 1), 0, 0);
                        ARB.Add(rb);
                        if ( irb == 0 )
                        {
                            CFix_Hinge fix = new CFix_Hinge(irb);
                            fix.SetIniPosFix(0,0,0);
                            fix.SetAxis(0.0,0.5,1);
                            AFix.Add( fix );
                        }
                        else
                        {
                            CJoint_Spherical fix = new CJoint_Spherical(irb-1, irb);
                            fix.SetIniPosJoint(div_len * (irb + 0.5), 0, 0);
                            AFix.Add( fix );
                        }
                    }
                }
                else if( ProbNo == 4 )
                {
                    CRigidBody3D rb = new CRigidBody3D();
                    rb.Set_ini_pos_cg_Vector(1.0, 0, 0);
                    ARB.Add(rb);
                    CFix_HingeRange fix = new CFix_HingeRange(0);
                    fix.SetIniPosFix(0,0,0);
                    fix.SetAxis(0,1,0);
                    fix.SetRange(-30,60);
                    AFix.Add( fix );
                }
                else if( ProbNo == 5 )
                {
                    uint nRB = 3;
                    // (1.0, 0, 0)
                    // (2.0, 0, 0)
                    // (3.0, 0, 0)
                    for (uint irb = 0; irb < nRB; irb++)
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector( (irb + 1) ,0,0);
                        ARB.Add(rb);
                    }
                    {
                        CFix_HingeRange fix = new CFix_HingeRange(0);
                        fix.SetIniPosFix(0,0,0);
                        fix.SetAxis(0,1,0);
                        fix.SetRange(-30,60);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_Hinge fix = new CJoint_Hinge(0,1);
                        fix.SetIniPosJoint(1.5,0,0);
                        fix.SetAxis(0,1,0);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_Hinge fix = new CJoint_Hinge(1,2);
                        fix.SetIniPosJoint(2.5,0,0);
                        fix.SetAxis(0,1,0);
                        AFix.Add( fix );
                    }
                }
                else if( ProbNo == 6 )
                {
                    uint nRB = 2;
                    // (1.0, 0, 0)
                    // (2.0, 0, 0)
                    for (uint irb = 0; irb < nRB; irb++)
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector( (irb + 1) ,0,0);
                        ARB.Add(rb);
                    }
                    {
                        CFix_HingeRange fix = new CFix_HingeRange(0);
                        fix.SetRange(-30,35);
                        //CFix_Hinge fix = new CFix_Hinge(0);
                        fix.SetIniPosFix(0,0,0);
                        fix.SetAxis(0,1,0);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_HingeRange fix = new CJoint_HingeRange(0,1);
                        fix.SetIniPosJoint(1.5,0,0);
                        fix.SetAxis(0,1,0);
                        fix.SetRange(-30,30);
                        AFix.Add( fix );
                    }
                }
                else if( ProbNo == 7 )
                {
                    uint nRB = 4;
                    // (1.0, 0, 0)
                    // (2.0, 0, 0)
                    // (3.0, 0, 0)
                    // (4.0, 0, 0)
                    for (uint irb = 0; irb < nRB; irb++)
                    {
                        CRigidBody3D rb = new CRigidBody3D();
                        rb.Set_ini_pos_cg_Vector( (irb + 1) ,0,0);
                        ARB.Add(rb);
                    }
                    {
                        CFix_HingeRange fix = new CFix_HingeRange(0);
                        fix.SetRange(-30,35);
                        //CFix_Hinge fix = new CFix_Hinge(0);
                        fix.SetIniPosFix(0,0,0);
                        fix.SetAxis(0,1,0);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_HingeRange fix = new CJoint_HingeRange(0,1);
                        fix.SetIniPosJoint(1.5,0,0);
                        fix.SetAxis(0,1,0);
                        fix.SetRange(-30,30);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_HingeRange fix = new CJoint_HingeRange(1,2);
                        fix.SetIniPosJoint(2.5,0,0);
                        fix.SetAxis(0,1,0);
                        fix.SetRange(-30,30);
                        AFix.Add( fix );
                    }
                    {
                        CJoint_HingeRange fix = new CJoint_HingeRange(2,3);
                        fix.SetIniPosJoint(3.5,0,0);
                        fix.SetAxis(0,1,0);
                        fix.SetRange(-30,30);
                        AFix.Add( fix );
                    }
                }
                else if( ProbNo == 8 )
                {
                    CRigidBody3D rb = new CRigidBody3D();
                    // rb.ini_pos_cg等は変更したらセットする必要があります。
                    rb.Set_ini_pos_cg_Vector(0.3,0.0,1.0);
                    rb.Set_Omega_Vector(0.0, 0.0, 1.0); // rb.Omega.z = 1.0;
                    // rb.mineatiaはDoubleArrayIndexerなので配列要素に代入できます。
                    rb.mineatia[2] = 10.0;
                    rb.mass = 0.001;
                    ARB.Add(rb);
                    CFix_Spherical fix = new CFix_Spherical(0);
                    fix.SetIniPosFix(0,0,0);
                    AFix.Add( fix );
                }
                success = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }

            /*
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            DelFEM4NetCom.View.DrawerGlUtility.SetProjectionTransform(Camera);
            //Glut.glutPostRedisplay();
            */
            ProbNo++;
            if (ProbNo == ProbCnt) ProbNo = 0;
            return success;
        }
    }
}
