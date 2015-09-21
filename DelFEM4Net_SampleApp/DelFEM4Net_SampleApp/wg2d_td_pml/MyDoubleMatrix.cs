using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace MyUtilLib.Matrix
{
    /// <summary>
    /// �s��N���X(double)
    ///    KrdLab.Lisys���x�[�X�ɕύX
    ///    C#�Q�����z���clapack�̔z��̕ϊ��I�[�o�w�b�h�𖳂������� + �������ߖ�̂��߂ɓ���
    ///
    ///    Lisys��Matrix�̃f�[�^�\���Ɠ�����1�����z��Ƃ��čs��f�[�^��ێ����܂��B
    ///    1�����z��́Aclapack�̔z�񐔒l�i�[�����Ɠ����i�s�f�[�^���Ɋi�[����)
    ///    ������double[,]����̒u�������|�C���g
    ///       double[,] --> MyDoubleMatrix
    ///       GetLength(0) --> RowSize
    ///       GetLength(1) --> ColumnSize
    /// </summary>
    public class MyDoubleMatrix
    {
        internal double[] _body = null;
        internal int _rsize = 0;
        internal int _csize = 0;

        /// <summary>
        /// �����o�b�t�@�̃C���f�b�N�X���擾����
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        internal virtual int GetBufferIndex(int row, int col)
        {
            System.Diagnostics.Debug.Assert(row >= 0 && row < RowSize && col >= 0 && col < ColumnSize);
            return (row + col * _rsize);
        }

        /// <summary>
        /// ��̃I�u�W�F�N�g���쐬����D
        /// </summary>
        internal MyDoubleMatrix()
        {
            Clear();
        }

        /// <summary>
        /// �w�肳�ꂽ�z����R�s�[���āC�V�����s����쐬����D
        /// </summary>
        /// <param name="body">�R�s�[�����z��</param>
        /// <param name="rowSize">�V�����s��</param>
        /// <param name="columnSize">�V������</param>
        internal MyDoubleMatrix(Double[] body, int rowSize, int columnSize)
        {
            CopyFrom(body, rowSize, columnSize);
        }

        /// <summary>
        /// �w�肳�ꂽ�T�C�Y�̍s����쐬����D
        /// �e�v�f��0�ɏ����������D ---> ��U�폜 �������ߖ�̈�
        /// </summary>
        /// <param name="rowSize">�s��</param>
        /// <param name="columnSize">��</param>
        public MyDoubleMatrix(int rowSize, int columnSize)
        {
            //Resize(rowSize, columnSize, 0.0); // ��U�폜 �������ߖ�̈�
            Resize(rowSize, columnSize);
        }

        /// <summary>
        /// �w�肳�ꂽ�s����R�s�[���āC�V�����s����쐬����D
        /// </summary>
        /// <param name="m">�R�s�[�����s��</param>
        public MyDoubleMatrix(MyDoubleMatrix m)
        {
            CopyFrom(m);
        }

        /// <summary>
        /// 2�����z�񂩂�V�����s����쐬����D
        /// </summary>
        /// <param name="arr">�s��̗v�f���i�[����2�����z��</param>
        public MyDoubleMatrix(double[,] arr)
        {
            int rsize = arr.GetLength(0);
            int csize = arr.GetLength(1);

            Resize(rsize, csize);

            for (int r = 0; r < rsize; ++r)
            {
                for (int c = 0; c < csize; ++c)
                {
                    this[r, c] = arr[r, c];
                }
            }
        }

        /// <summary>
        /// ���̍s��̊e�v�f��ݒ�C�擾����D
        /// </summary>
        /// <param name="row">�sindex�i�͈́F[0, <see cref="RowSize"/>) �j</param>
        /// <param name="col">��index�i�͈́F[0, <see cref="ColumnSize"/>) �j</param>
        /// <returns>�v�f�̒l</returns>
        public virtual double this[int row, int col]
        {
            get
            {
                if (row < 0 || this.RowSize <= row || col < 0 || this.ColumnSize <= col)
                {
                    throw new IndexOutOfRangeException();
                }
                return this._body[row + col * this._rsize];
            }
            set
            {
                if (row < 0 || this.RowSize <= row || col < 0 || this.ColumnSize <= col)
                {
                    throw new IndexOutOfRangeException();
                }
                this._body[row + col * this._rsize] = value;
            }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g�̍s�����擾����D
        /// </summary>
        public virtual int RowSize
        {
            get { return this._rsize; }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g�̗񐔂��擾����D
        /// </summary>
        public virtual int ColumnSize
        {
            get { return this._csize; }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g���N���A����i<c>RowSize == 0 and ColumnSize == 0</c> �ɂȂ�j�D
        /// </summary>
        public virtual void Clear()
        {
            this._body = new double[0];
            this._rsize = 0;
            this._csize = 0;
        }

        /// <summary>
        /// ���T�C�Y����D���T�C�Y��̊e�v�f�l��0�ɂȂ�D
        /// </summary>
        /// <param name="rowSize">�V�����s��</param>
        /// <param name="columnSize">�V������</param>
        /// <returns>���T�C�Y��̎��g�ւ̎Q��</returns>
        public virtual MyDoubleMatrix Resize(int rowSize, int columnSize)
        {
            this._body = new double[rowSize * columnSize];
            this._rsize = rowSize;
            this._csize = columnSize;
            return this;
        }

        /*
        /// <summary>
        /// ���T�C�Y����D
        /// </summary>
        /// <param name="rowSize">�V�����s��</param>
        /// <param name="columnSize">�V������</param>
        /// <param name="val">�e�v�f�̒l</param>
        /// <returns>���T�C�Y��̎��g�ւ̎Q��</returns>
        public MyDoubleMatrix Resize(int rowSize, int columnSize, double val)
        {
            Resize(rowSize, columnSize);
            for (int i = 0; i < this._body.Length; ++i)
            {
                this._body[i] = val;
            }
            return this;
        }
         */

        /// <summary>
        /// �w�肳�ꂽ�s����R�s�[����D
        /// </summary>
        /// <param name="m">�R�s�[�����s��</param>
        /// <returns>�R�s�[��̎��g�ւ̎Q��</returns>
        public virtual MyDoubleMatrix CopyFrom(MyDoubleMatrix m)
        {
            return CopyFrom(m._body, m._rsize, m._csize);
        }

        /// <summary>
        /// <para>�w�肳�ꂽ1�����z����C�w�肳�ꂽ�s��`���ŃR�s�[����D</para>
        /// <para>�z��̃T�C�Y�ƁurowSize * columnSize�v�͈�v���Ȃ���΂Ȃ�Ȃ��D</para>
        /// </summary>
        /// <param name="body">�R�s�[�����z��</param>
        /// <param name="rowSize">�s��</param>
        /// <param name="columnSize">��</param>
        /// <returns>�R�s�[��̎��g�ւ̎Q��</returns>
        internal virtual MyDoubleMatrix CopyFrom(Double[] body, int rowSize, int columnSize)
        {
            // ���͂̌���
            System.Diagnostics.Debug.Assert(body.Length == rowSize * columnSize);
            if (body.Length != rowSize * columnSize)
            {
                return this;
            }

            // �o�b�t�@�m��
            if (this._rsize == rowSize && this._csize == columnSize)
            {
                // �������Ȃ�
            }
            else if (this._body != null && this._body.Length == rowSize * columnSize)
            {
                this._rsize = rowSize;
                this._csize = columnSize;
            }
            else
            {
                Resize(rowSize, columnSize);
            }

            // �R�s�[
            body.CopyTo(this._body, 0);
            return this;
        }

        /// <summary>
        /// �s��� 2�����z��Ƃ��ďo�͂���D
        /// </summary>
        /// <returns>2�����z��i<c>array[r, c] == matrix[r, c]</c>�j</returns>
        public double[,] ToArray()
        {
            double[,] ret = new double[this.RowSize, this.ColumnSize];
            for (int r = 0; r < this.RowSize; ++r)
            {
                for (int c = 0; c < this.ColumnSize; ++c)
                {
                    ret[r, c] = this[r, c];
                }
            }
            return ret;
        }

        /// <summary>
        /// �s���1�����z��Ƃ��ďo�͂���
        /// </summary>
        /// <returns></returns>
        public virtual double[] ToArray1D()
        {
            double[] buffer = new double[this.RowSize * this.ColumnSize];
            for (int r = 0; r < this.RowSize; ++r)
            {
                for (int c = 0; c < this.ColumnSize; ++c)
                {
                    buffer[r + this.RowSize * c] = this[r, c];
                }
            }
            return buffer;
        }

        /// <summary>
        /// ���̍s����[���s��ɂ���D
        /// </summary>
        /// <returns></returns>
        public MyDoubleMatrix Zero()
        {
            int size = this._body.Length;
            for (int i = 0; i < size; ++i)
            {
                this._body[i] = 0.0;
            }
            return this;
        }

        /// <summary>
        /// ���̍s���P�ʍs��iI = diag(1,1,...,1)�j�ɂ���D
        /// <para>Unit�́C�S�Ă̗v�f��1�ł���s��̂��Ƃ������DIdentify�Ƃ͈قȂ邱�Ƃɒ��ӂ���D</para>
        /// </summary>
        /// <returns></returns>
        public MyDoubleMatrix Identity()
        {
            //MyDoubleMatrixChecker.IsSquare(this);

            this.Zero();
            for (int i = 0; i < this.RowSize; ++i)
            {
                this[i, i] = 1;
            }
            return this;
        }

        /// <summary>
        /// �S�Ă̗v�f�̕����𔽓]����D
        /// </summary>
        /// <returns>���g�ւ̎Q��</returns>
        public MyDoubleMatrix Flip()
        {
            int size = this._body.Length;
            for (int i = 0; i < size; ++i)
            {
                this._body[i] = -this._body[i];
            }
            return this;
        }

        /// <summary>
        /// �]�u����D
        /// </summary>
        /// <returns>�]�u��̎��g�ւ̎Q��</returns>
        public virtual MyDoubleMatrix Transpose()
        {
            MyDoubleMatrix t = new MyDoubleMatrix(this._csize, this._rsize);

            for (int r = 0; r < this._rsize; ++r)
            {
                for (int c = 0; c < this._csize; ++c)
                {
                    t[c, r] = this[r, c];
                }
            }

            this.Clear();
            this._body = t._body;
            this._rsize = t._rsize;
            this._csize = t._csize;

            return this;
        }
    }
}
