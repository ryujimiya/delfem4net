using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
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

namespace MyUtilLib.Matrix
{
    /// <summary>
    /// 定数
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// 計算精度下限
        /// </summary>
        public static readonly double PrecisionLowerLimit = 1.0e-12;
    }

    /// <summary>
    /// 行列操作関数群
    /// </summary> 
    public class MyMatrixUtil
    {

        public static void printMatrix(string tag, double[,] mat)
        {
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    double val = mat[i, j];
                    System.Diagnostics.Debug.WriteLine(tag + "(" + i + ", " + j + ")" + " = " + val);
                }
            }
        }
        public static void printMatrix(string tag, Complex[,] mat)
        {
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    Complex val = mat[i, j];
                    System.Diagnostics.Debug.WriteLine(tag + "(" + i + ", " + j + ")" + " = "
                                       + "(" + val.Real + "," + val.Imag + ") " + Complex.Norm(val));
                }
            }
        }
        public static void printMatrixNoZero(string tag, Complex[,] mat)
        {
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    Complex val = mat[i, j];
                    if (Complex.Norm(val) < Constants.PrecisionLowerLimit) continue;
                    System.Diagnostics.Debug.WriteLine(tag + "(" + i + ", " + j + ")" + " = "
                                       + "(" + val.Real + "," + val.Imag + ") ");
                }
            }
        }
        public static void printMatrixNoZero(string tag, double[,] mat)
        {
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    double val = mat[i, j];
                    if (Math.Abs(val) < Constants.PrecisionLowerLimit) continue;
                    System.Diagnostics.Debug.WriteLine(tag + "(" + i + ", " + j + ")" + " = " + val + " ");
                }
            }
        }
        public static void printVec(string tag, double[] vec)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                Complex val = vec[i];
                System.Diagnostics.Debug.WriteLine(tag + "(" + i + ")" + " = " + val);
            }
        }
        public static void printVec(string tag, Complex[] vec)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                Complex val = vec[i];
                System.Diagnostics.Debug.WriteLine(tag + "(" + i + ")" + " = "
                                   + "(" + val.Real + "," + val.Imag + ") " + Complex.Norm(val));
            }
        }
        /*
        public static void printVec(string tag, ValueType[] vec)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                Complex val = (Complex)vec[i];
                System.Diagnostics.Debug.WriteLine(tag + "(" + i + ")" + " = "
                                   + "(" + val.Real + "," + val.Imag + ") " + Complex.Norm(val));
            }
        }
        */

        public static double[] matrix_ToBuffer(double[,] mat)
        {
            double[] mat_ = new double[mat.GetLength(0) * mat.GetLength(1)];
            // rowから先に埋めていく
            for (int j = 0; j < mat.GetLength(1); j++) //col
            {
                for (int i = 0; i < mat.GetLength(0); i++) // row
                {
                    mat_[j * mat.GetLength(0) + i] = mat[i, j];
                }
            }
            return mat_;
        }

        public static double[,] matrix_FromBuffer(double[] mat_, int nRow, int nCol)
        {
            double[,] mat = new double[nRow, nCol];
            // rowから先に埋めていく
            for (int j = 0; j < mat.GetLength(1); j++) // col
            {
                for (int i = 0; i < mat.GetLength(0); i++)  // row
                {
                    mat[i, j] = mat_[j * mat.GetLength(0) + i];
                }
            }
            return mat;
        }

        public static double[,] matrix_Inverse(double[,] matA)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matA.GetLength(1));
            int n = matA.GetLength(0);
            double[] matA_ = matrix_ToBuffer(matA);
            double[] matB_ = new double[n * n];
            // 単位行列
            for (int i = 0; i < matB_.Length; i++)
            {
                matB_[i] = 0.0;
            }
            for (int i = 0; i < n; i++)
            {
                matB_[i * n + i] = 1.0;
            }
            // [A][X] = [B]
            //  [B]の内容が書き換えられるので、matXを新たに生成せず、matBを出力に指定している
            int x_row = 0;
            int x_col = 0;
            KrdLab.clapack.Function.dgesv(ref matB_, ref x_row, ref x_col, matA_, n, n, matB_, n, n);

            double[,] matX = matrix_FromBuffer(matB_, x_row, x_col);
            return matX;
        }

        public static KrdLab.clapack.Complex [] matrix_ToBuffer(Complex[,] mat)
        {
            KrdLab.clapack.Complex[] mat_ = new KrdLab.clapack.Complex[mat.GetLength(0) * mat.GetLength(1)];
            // rowから先に埋めていく
            for (int j = 0; j < mat.GetLength(1); j++) //col
            {
                for (int i = 0; i < mat.GetLength(0); i++) // row
                {
                    //mat_[j * mat.GetLength(0) + i] = new KrdLab.clapack.Complex(mat[i, j].Real, mat[i,j].Imag);
                    mat_[j * mat.GetLength(0) + i].Real = mat[i, j].Real;
                    mat_[j * mat.GetLength(0) + i].Imaginary = mat[i,j].Imag;
                }
            }
            return mat_;
        }

        public static Complex[,] matrix_FromBuffer(KrdLab.clapack.Complex[] mat_, int nRow, int nCol)
        {
            Complex[,] mat = new Complex[nRow, nCol];
            // rowから先に埋めていく
            for (int j = 0; j < mat.GetLength(1); j++) // col
            {
                for (int i = 0; i < mat.GetLength(0); i++)  // row
                {
                    KrdLab.clapack.Complex cval = mat_[j * mat.GetLength(0) + i];
                    mat[i, j] = new Complex(cval.Real, cval.Imaginary);
                }
            }
            return mat;
        }

        public static Complex[,] matrix_Inverse(Complex[,] matA)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matA.GetLength(1));
            int n = matA.GetLength(0);
            KrdLab.clapack.Complex[] matA_ = matrix_ToBuffer(matA);
            KrdLab.clapack.Complex[] matB_ = new KrdLab.clapack.Complex[n * n];
            // 単位行列
            for (int i = 0; i < matB_.Length; i++)
            {
                matB_[i] = (KrdLab.clapack.Complex)0.0;
            }
            for (int i = 0; i < n; i++)
            {
                matB_[i * n + i] = (KrdLab.clapack.Complex)1.0;
            }
            // [A][X] = [B]
            //  [B]の内容が書き換えられるので、matXを新たに生成せず、matBを出力に指定している
            int x_row = 0;
            int x_col = 0;
            KrdLab.clapack.FunctionExt.zgesv(ref matB_, ref x_row, ref x_col, matA_, n, n, matB_, n, n);

            Complex[,] matX = matrix_FromBuffer(matB_, x_row, x_col);
            return matX;
        }

        public static KrdLab.clapack.Complex[] matrix_Inverse(KrdLab.clapack.Complex[] matA, int n)
        {
            System.Diagnostics.Debug.Assert(matA.Length == (n * n));
            KrdLab.clapack.Complex[] matA_ = new KrdLab.clapack.Complex[n * n];
            matA.CopyTo(matA_, 0);
            KrdLab.clapack.Complex[] matB_ = new KrdLab.clapack.Complex[n * n];
            // 単位行列
            for (int i = 0; i < matB_.Length; i++)
            {
                matB_[i] = 0.0;
            }
            for (int i = 0; i < n; i++)
            {
                matB_[i + i * n] = 1.0;
            }
            // [A][X] = [B]
            //  [B]の内容が書き換えられるので、matXを新たに生成せず、matBを出力に指定している
            int x_row = 0;
            int x_col = 0;
            KrdLab.clapack.FunctionExt.zgesv(ref matB_, ref x_row, ref x_col, matA_, n, n, matB_, n, n);

            KrdLab.clapack.Complex[] matX = matB_;
            return matX;
        }

        public static double[] matrix_Inverse(double[] matA, int n)
        {
            System.Diagnostics.Debug.Assert(matA.Length == (n * n));
            double[] matA_ = new double[n * n];
            matA.CopyTo(matA_, 0);
            double[] matB_ = new double[n * n];
            // 単位行列
            for (int i = 0; i < matB_.Length; i++)
            {
                matB_[i] = 0.0;
            }
            for (int i = 0; i < n; i++)
            {
                matB_[i + i * n] = 1.0;
            }
            // [A][X] = [B]
            //  [B]の内容が書き換えられるので、matXを新たに生成せず、matBを出力に指定している
            int x_row = 0;
            int x_col = 0;
            KrdLab.clapack.Function.dgesv(ref matB_, ref x_row, ref x_col, matA_, n, n, matB_, n, n);

            double[] matX = matB_;
            return matX;
        }

        public static double[] matrix_Inverse_NoCopy(double[] matA, int n)
        {
            System.Diagnostics.Debug.Assert(matA.Length == (n * n));
            double[] matA_ = matA;
            double[] matB_ = new double[n * n];
            // 単位行列
            for (int i = 0; i < matB_.Length; i++)
            {
                matB_[i] = 0.0;
            }
            for (int i = 0; i < n; i++)
            {
                matB_[i + i * n] = 1.0;
            }
            // [A][X] = [B]
            //  [B]の内容が書き換えられるので、matXを新たに生成せず、matBを出力に指定している
            int x_row = 0;
            int x_col = 0;
            KrdLab.clapack.Function.dgesv(ref matB_, ref x_row, ref x_col, matA_, n, n, matB_, n, n);

            double[] matX = matB_;
            return matX;
        }

        public static double[,] product(double[,] matA, double[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(0));
            double[,] matX = new double[matA.GetLength(0), matB.GetLength(1)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = 0.0;
                    for (int k = 0; k < matA.GetLength(1); k++)
                    {
                        matX[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
            return matX;
        }

        public static Complex[,] product(Complex[,] matA, double[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(0));
            Complex[,] matX = new Complex[matA.GetLength(0), matB.GetLength(1)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = 0.0;
                    for (int k = 0; k < matA.GetLength(1); k++)
                    {
                        matX[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
            return matX;
        }

        public static Complex[,] product(double[,] matA, Complex[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(0));
            Complex[,] matX = new Complex[matA.GetLength(0), matB.GetLength(1)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = 0.0;
                    for (int k = 0; k < matA.GetLength(1); k++)
                    {
                        matX[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
            return matX;
        }

        public static Complex[,] product(Complex[,] matA, Complex[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(0));
            Complex[,] matX = new Complex[matA.GetLength(0), matB.GetLength(1)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = 0.0;
                    for (int k = 0; k < matA.GetLength(1); k++)
                    {
                        matX[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
            return matX;
        }

        public static KrdLab.clapack.Complex[] product(
            KrdLab.clapack.Complex[] matA, int a_row, int a_col,
            KrdLab.clapack.Complex[] matB, int b_row, int b_col)
        {
            System.Diagnostics.Debug.Assert(a_col == b_row);
            int x_row = a_row;
            int x_col = b_col;
            KrdLab.clapack.Complex[] matX = new KrdLab.clapack.Complex[x_row * x_col];
            for (int i = 0; i < x_row; i++)
            {
                for (int j = 0; j < x_col; j++)
                {
                    matX[i + j * x_row] = 0.0;
                    for (int k = 0; k < a_col; k++)
                    {
                        matX[i + j * x_row] += matA[i + k * a_row] * matB[k + j * b_row];
                    }
                }
            }
            return matX;
        }

        public static double[] product(
            double[] matA, int a_row, int a_col,
            double[] matB, int b_row, int b_col)
        {
            System.Diagnostics.Debug.Assert(a_col == b_row);
            int x_row = a_row;
            int x_col = b_col;
            double[] matX = new double[x_row * x_col];
            for (int i = 0; i < x_row; i++)
            {
                for (int j = 0; j < x_col; j++)
                {
                    matX[i + j * x_row] = 0.0;
                    for (int k = 0; k < a_col; k++)
                    {
                        matX[i + j * x_row] += matA[i + k * a_row] * matB[k + j * b_row];
                    }
                }
            }
            return matX;
        }

        // [X] = [A] + [B]
        public static double[,] plus(double[,] matA, double[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matB.GetLength(0));
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(1));
            double[,] matX = new double[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = matA[i, j] + matB[i, j];
                }
            }
            return matX;
        }

        // [X] = [A] - [B]
        public static double[,] minus(double[,] matA, double[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matB.GetLength(0));
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(1));
            double[,] matX = new double[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = matA[i, j] - matB[i, j];
                }
            }
            return matX;
        }

        // [X] = alpha * [A]
        public static double[,] product(double alpha, double[,] matA)
        {
            double[,] matX = new double[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = alpha * matA[i, j];
                }
            }
            return matX;
        }

        // {x} = alpha * {a}
        public static double[] product(double alpha, double[] vecA)
        {
            double[] vecX = new double[vecA.Length];
            for (int i = 0; i < vecX.Length; i++)
            {
                vecX[i] = alpha * vecA[i];
            }
            return vecX;
        }

        // {x} = ({v})*
        public static Complex[] vector_Conjugate(Complex[] vec)
        {
            Complex[] retVec = new Complex[vec.Length];
            for (int i = 0; i < retVec.Length; i++)
            {
                retVec[i] = Complex.Conjugate(vec[i]);
            }
            return retVec;
        }

        // {x} = ({v})*
        public static KrdLab.clapack.Complex[] vector_Conjugate(KrdLab.clapack.Complex[] vec)
        {
            KrdLab.clapack.Complex[] retVec = new KrdLab.clapack.Complex[vec.Length];
            for (int i = 0; i < retVec.Length; i++)
            {
                retVec[i] = KrdLab.clapack.Complex.Conjugate(vec[i]);
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static Complex[] product(Complex[,] matA, Complex[] vec)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == vec.Length);
            //BUGFIX
            //Complex[] retVec = new Complex[vec.Length];
            Complex[] retVec = new Complex[matA.GetLength(0)];

            for (int i = 0; i < matA.GetLength(0); i++)
            {
                retVec[i] = new Complex(0.0, 0.0);
                for (int k = 0; k < matA.GetLength(1); k++)
                {
                    retVec[i] += matA[i, k] * vec[k];
                }
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static Complex[] product(double[,] matA, Complex[] vec)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == vec.Length);
            //BUGFIX
            //Complex[] retVec = new Complex[vec.Length];
            Complex[] retVec = new Complex[matA.GetLength(0)];

            for (int i = 0; i < matA.GetLength(0); i++)
            {
                retVec[i] = new Complex(0.0, 0.0);
                for (int k = 0; k < matA.GetLength(1); k++)
                {
                    retVec[i] += matA[i, k] * vec[k];
                }
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static KrdLab.clapack.Complex[] product(KrdLab.clapack.Complex[] matA, int a_row, int a_col, KrdLab.clapack.Complex[] vec, int vec_row)
        {
            System.Diagnostics.Debug.Assert(a_col == vec_row);
            //BUGFIX
            //KrdLab.clapack.Complex[] retVec = new KrdLab.clapack.Complex[vec_row];
            KrdLab.clapack.Complex[] retVec = new KrdLab.clapack.Complex[a_row];

            for (int i = 0; i < a_row; i++)
            {
                retVec[i] = new KrdLab.clapack.Complex(0.0, 0.0);
                for (int k = 0; k < a_col; k++)
                {
                    retVec[i] += matA[i + k * a_row] * vec[k];
                }
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static KrdLab.clapack.Complex[] product(double[] matA, int a_row, int a_col, KrdLab.clapack.Complex[] vec, int vec_row)
        {
            System.Diagnostics.Debug.Assert(a_col == vec_row);
            KrdLab.clapack.Complex[] retVec = new KrdLab.clapack.Complex[a_row];

            for (int i = 0; i < a_row; i++)
            {
                retVec[i] = new KrdLab.clapack.Complex(0.0, 0.0);
                for (int k = 0; k < a_col; k++)
                {
                    retVec[i] += matA[i + k * a_row] * vec[k];
                }
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static double[] product(double[] matA, int a_row, int a_col, double[] vec, int vec_row)
        {
            System.Diagnostics.Debug.Assert(a_col == vec_row);
            double[] retVec = new double[a_row];

            for (int i = 0; i < a_row; i++)
            {
                retVec[i] = 0.0;
                for (int k = 0; k < a_col; k++)
                {
                    retVec[i] += matA[i + k * a_row] * vec[k];
                }
            }
            return retVec;
        }

        // {x} = [A]{v}
        public static double[] product_native(double[] matA, int a_row, int a_col, double[] vec, int vec_row)
        {
            System.Diagnostics.Debug.Assert(a_col == vec_row);
            double[] retVec = null;

            int c_row = a_row;
            int c_col = 1;
            KrdLab.clapack.Function.dgemm(ref retVec, ref c_row, ref c_col, matA, a_row, a_row, vec, vec_row, 1);
            return retVec;
        }

        // [X] = alpha * [A]
        public static Complex[,] product(Complex alpha, Complex[,] matA)
        {
            Complex[,] matX = new Complex[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = alpha * matA[i, j];
                }
            }
            return matX;
        }

        // {x} = alpha * {a}
        public static Complex[] product(Complex alpha, Complex[] vecA)
        {
            Complex[] vecX = new Complex[vecA.Length];
            for (int i = 0; i < vecX.Length; i++)
            {
                vecX[i] = alpha * vecA[i];
            }
            return vecX;
        }


        // [X] = [A] + [B]
        public static Complex[,] plus(Complex[,] matA, Complex[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matB.GetLength(0));
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(1));
            Complex[,] matX = new Complex[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = matA[i, j] + matB[i, j];
                }
            }
            return matX;
        }

        // [X] = [A] - [B]
        public static Complex[,] minus(Complex[,] matA, Complex[,] matB)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(0) == matB.GetLength(0));
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == matB.GetLength(1));
            Complex[,] matX = new Complex[matA.GetLength(0), matA.GetLength(1)];
            for (int i = 0; i < matA.GetLength(0); i++)
            {
                for (int j = 0; j < matA.GetLength(1); j++)
                {
                    matX[i, j] = matA[i, j] - matB[i, j];
                }
            }
            return matX;
        }

        // 行列の行ベクトルを抜き出す
        public static Complex[] matrix_GetRowVec(Complex[,] matA, int row)
        {
            Complex[] rowVec = new Complex[matA.GetLength(1)];
            for (int j = 0; j < matA.GetLength(1); j++)
            {
                rowVec[j] = matA[row, j];
            }
            return rowVec;
        }

        public static void matrix_setRowVec(Complex[,] matA, int row, Complex[] rowVec)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == rowVec.Length);
            for (int j = 0; j < matA.GetLength(1); j++)
            {
                matA[row, j] = rowVec[j];
            }
        }

        // 行列の行ベクトルを抜き出す
        public static KrdLab.clapack.Complex[] matrix_GetRowVec(KrdLab.clapack.Complex[,] matA, int row)
        {
            KrdLab.clapack.Complex[] rowVec = new KrdLab.clapack.Complex[matA.GetLength(1)];
            for (int j = 0; j < matA.GetLength(1); j++)
            {
                rowVec[j] = matA[row, j];
            }
            return rowVec;
        }

        public static void matrix_setRowVec(KrdLab.clapack.Complex[,] matA, int row, KrdLab.clapack.Complex[] rowVec)
        {
            System.Diagnostics.Debug.Assert(matA.GetLength(1) == rowVec.Length);
            for (int j = 0; j < matA.GetLength(1); j++)
            {
                matA[row, j] = rowVec[j];
            }
        }

        // x = sqrt(c)
        public static Complex complex_Sqrt(Complex c)
        {
            System.Numerics.Complex work = new System.Numerics.Complex(c.Real, c.Imag);
            work = System.Numerics.Complex.Sqrt(work);
            return new Complex(work.Real, work.Imaginary);
        }

        // x = Log(c)
        public static Complex complex_Log(Complex c)
        {
            System.Numerics.Complex work = new System.Numerics.Complex(c.Real, c.Imag);
            work = System.Numerics.Complex.Log(work);
            return new Complex(work.Real, work.Imaginary);
        }

        // x = Log(c)
        public static KrdLab.clapack.Complex complex_Log(KrdLab.clapack.Complex c)
        {
            System.Numerics.Complex work = new System.Numerics.Complex(c.Real, c.Imaginary);
            work = System.Numerics.Complex.Log(work);
            return new KrdLab.clapack.Complex(work.Real, work.Imaginary);
        }

        // [X] = [A]t
        public static double[,] matrix_Transpose(double[,] matA)
        {
            double[,] matX = new double[matA.GetLength(1), matA.GetLength(0)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = matA[j, i];
                }
            }
            return matX;
        }

        // [X] = [A]t
        public static Complex[,] matrix_Transpose(Complex[,] matA)
        {
            Complex[,] matX = new Complex[matA.GetLength(1), matA.GetLength(0)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = matA[j, i];
                }
            }
            return matX;
        }

        // [X] = ([A]*)t
        public static Complex[,] matrix_ConjugateTranspose(Complex[,] matA)
        {
            Complex[,] matX = new Complex[matA.GetLength(1), matA.GetLength(0)];
            for (int i = 0; i < matX.GetLength(0); i++)
            {
                for (int j = 0; j < matX.GetLength(1); j++)
                {
                    matX[i, j] = new Complex(matA[j, i].Real, -matA[j, i].Imag);
                }
            }
            return matX;
        }

        // x = {v1}t{v2}
        public static Complex vector_Dot(Complex[] v1, Complex[] v2)
        {
            System.Diagnostics.Debug.Assert(v1.Length == v2.Length);
            int n = v1.Length;
            Complex sum = new Complex(0.0, 0.0);
            for (int i = 0; i < n; i++)
            {
                sum += v1[i] * v2[i];
            }
            return sum;
        }

        // x = {v1}t{v2}
        public static KrdLab.clapack.Complex vector_Dot(KrdLab.clapack.Complex[] v1, KrdLab.clapack.Complex[] v2)
        {
            System.Diagnostics.Debug.Assert(v1.Length == v2.Length);
            int n = v1.Length;
            KrdLab.clapack.Complex sum = new KrdLab.clapack.Complex(0.0, 0.0);
            for (int i = 0; i < n; i++)
            {
                sum += v1[i] * v2[i];
            }
            return sum;
        }

        // {x} = {a} + {b}
        public static Complex[] plus(Complex[] vecA, Complex[] vecB)
        {
            System.Diagnostics.Debug.Assert(vecA.Length == vecB.Length);
            Complex[] vecX = new Complex[vecA.Length];
            for (int i = 0; i < vecA.Length; i++)
            {
                vecX[i] = vecA[i] + vecB[i];
            }
            return vecX;
        }

        // {x} = {a} + {b}
        public static double[] plus(double[] vecA, double[] vecB)
        {
            System.Diagnostics.Debug.Assert(vecA.Length == vecB.Length);
            double[] vecX = new double[vecA.Length];
            for (int i = 0; i < vecA.Length; i++)
            {
                vecX[i] = vecA[i] + vecB[i];
            }
            return vecX;
        }

        // {x} = {a} - {b}
        public static Complex[] minus(Complex[] vecA, Complex[] vecB)
        {
            System.Diagnostics.Debug.Assert(vecA.Length == vecB.Length);
            Complex[] vecX = new Complex[vecA.Length];
            for (int i = 0; i < vecA.Length; i++)
            {
                vecX[i] = vecA[i] - vecB[i];
            }
            return vecX;
        }

        // {x} = {a} - {b}
        public static double[] minus(double[] vecA, double[] vecB)
        {
            System.Diagnostics.Debug.Assert(vecA.Length == vecB.Length);
            double[] vecX = new double[vecA.Length];
            for (int i = 0; i < vecA.Length; i++)
            {
                vecX[i] = vecA[i] - vecB[i];
            }
            return vecX;
        }
    }
}
