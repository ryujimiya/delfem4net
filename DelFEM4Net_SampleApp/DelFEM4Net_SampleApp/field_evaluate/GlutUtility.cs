/*
 *  glut_utility.h
 *  dfm_core
 *
 *  Created by Nobuyuki Umetani on 1/20/11.
 *  Copyright 2011 The University of Tokyo. All rights reserved.
 *
 */
/*
   DelFEMのinclude/glut_utility.hをC#で書き直したものです
   created by ryujimiya (original DelFEM codes created by Nobuyuki Umetani
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.FreeGlut;
using Tao.OpenGl;

namespace DelFEM4NetCom
{
    public class GlutUtility
    {
    
        public static void RenderBitmapString(float x, float y, IntPtr font,string str)
        {
            Gl.glRasterPos2f(x, y);
            Encoding enc = Encoding.ASCII;
            byte[] bytes = enc.GetBytes(str);
            foreach (byte c in bytes)
            {
                Glut.glutBitmapCharacter(font, c);
            }
        }
    
        public static void ShowBackGround()
        {
            int is_lighting = Gl.glIsEnabled(Gl.GL_LIGHTING);
            int is_texture  = Gl.glIsEnabled(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            ////
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
              Gl.glLoadIdentity();
              Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
              Gl.glLoadIdentity();
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            ////
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glColor3d(0.2,0.7,0.7);
            Gl.glVertex3d(-1,-1,0);
            Gl.glVertex3d( 1,-1,0);
            Gl.glColor3d(1,1,1);
            Gl.glVertex3d( 1, 1,0);
            Gl.glVertex3d(-1, 1,0);
            Gl.glEnd();
            ////
            Gl.glEnable(Gl.GL_DEPTH_TEST);
              Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
              Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
            if( is_texture == 1 ){  Gl.glEnable(Gl.GL_TEXTURE_2D); }
            if( is_lighting == 1 ){ Gl.glEnable(Gl.GL_LIGHTING); }
        }

        public static void ShowFPS(ref int frame, ref int timebase, ref string s_fps)
        {
            int is_lighting = Gl.glIsEnabled(Gl.GL_LIGHTING);
            int is_texture  = Gl.glIsEnabled(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            /////
            IntPtr font = Glut.GLUT_BITMAP_8_BY_13;
            {
                int time;
                frame++;
                time = Glut.glutGet(Glut.GLUT_ELAPSED_TIME);
                if (time - timebase > 500)
                {
                    s_fps = string.Format("FPS:{0}",frame * 1000.0 / (time-timebase));
                    timebase = time;
                    frame = 0;
                }
            }
            string s_tmp = "";

            int[] viewport = new int[4];
           
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);
            int win_w = viewport[2];
            int win_h = viewport[3];
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, win_w, 0,   win_h);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glScalef(1, -1, 1);
            Gl.glTranslatef(0, -win_h,   0);
            Gl.glDisable(Gl.GL_LIGHTING  );
            //Gl.glDisable(Gl.GL_DEPTH_TEST);
            //Gl.glColor3d(1.0, 1.0, 0.0);
            Gl.glColor3d(1.0, 0.0, 0.0)  ;
            s_tmp = "DelFEM demo";
            RenderBitmapString(10,15, font, s_tmp);
            Gl.glColor3d(0.0, 0.0, 1.0);
            s_tmp = "Press 'space' key!";
            RenderBitmapString(120,15, font, s_tmp);
            //Gl.glColor3d(1.0, 0.0,   0.0);
            Gl.glColor3d(0.0, 0.0, 0.0)  ;
            RenderBitmapString(10,30, font, s_fps);
            //Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            ////                             
            if( is_texture == 1 ){ Gl.glEnable(Gl.GL_TEXTURE_2D); }
            if( is_lighting == 1 ){ Gl.glEnable(Gl.GL_LIGHTING); }
        }
    }
}