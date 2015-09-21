using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace MyUtilLib.Matrix
{
    /// <summary>
    /// �o���h�s��N���X(double)
    ///    KrdLab.Lisys���x�[�X�ɕύX
    ///    C#�Q�����z���clapack�̔z��̕ϊ��I�[�o�w�b�h�𖳂������� + �������ߖ�̂��߂ɓ���
    ///
    ///    Lisys��Matrix�̃f�[�^�\���Ɠ�����1�����z��Ƃ��čs��f�[�^��ێ����܂��B
    ///    1�����z��́Aclapack�̔z�񐔒l�i�[�����Ɠ����i�s�f�[�^���Ɋi�[����)
    /// </summary>
    public class MyDoubleBandMatrix : MyDoubleMatrix
    {
        internal int _rowcolSize = 0;
        internal int _subdiaSize = 0;
        internal int _superdiaSize = 0;

        /// <summary>
        /// �����o�b�t�@�̃C���f�b�N�X���擾����
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        internal override int GetBufferIndex(int row, int col)
        {
            System.Diagnostics.Debug.Assert(row >= 0 && row < RowSize && col >= 0 && col < ColumnSize);
            if (!(row >= col - this._superdiaSize && row <= col + this._subdiaSize))
            {
                System.Diagnostics.Debug.Assert(false);
                return -1;
            }

            return ((row - col) + this._subdiaSize + this._subdiaSize + col * this._rsize);
        }

        /// <summary>
        /// ��̃I�u�W�F�N�g���쐬����D
        /// </summary>
        internal MyDoubleBandMatrix()
        {
            Clear();
        }

        /// <summary>
        /// �w�肳�ꂽ�z����R�s�[���āC�V�����s����쐬����D
        /// </summary>
        /// <param name="body">�R�s�[�����z��</param>
        /// <param name="rowSize">�V�����s��=�V������</param>
        /// <param name="columnSize">subdiagonal�̃T�C�Y</param>
        /// <param name="columnSize">superdiagonal�̃T�C�Y</param>
        internal MyDoubleBandMatrix(double[] body, int rowcolSize, int subdiaSize, int superdiaSize)
        {
            CopyFrom(body, rowcolSize, subdiaSize, superdiaSize);
        }

        /// <summary>
        /// �w�肳�ꂽ�T�C�Y�̍s����쐬����D
        /// �e�v�f��0�ɏ����������D--> ��U�폜 �������ߖ�̈�
        /// </summary>
        /// <param name="rowcolSize">�s��=��</param>
        /// <param name="subdiaSize">subdiagonal�̃T�C�Y</param>
        /// <param name="superdiaSize">superdiagonal�̃T�C�Y</param>
        public MyDoubleBandMatrix(int rowcolSize, int subdiaSize, int superdiaSize)
        {
            //Resize(rowSize, columnSize, 0.0); // ��U�폜 �������ߖ�̈�
            Resize(rowcolSize, subdiaSize, superdiaSize);
        }

        /// <summary>
        /// �x�[�X�N���X�̃R���X�g���N�^�Ɠ�������
        /// </summary>
        /// <param name="rowSize"></param>
        /// <param name="colSize"></param>
        private MyDoubleBandMatrix(int rowSize, int colSize)
            //: base(rowSize, colSize)
        {
            System.Diagnostics.Debug.Assert(false);
            Clear();
        }

        /// <summary>
        /// �w�肳�ꂽ�s����R�s�[���āC�V�����s����쐬����D
        /// </summary>
        /// <param name="m">�R�s�[�����s��</param>
        public MyDoubleBandMatrix(MyDoubleBandMatrix m)
        {
            CopyFrom(m);
        }

        /// <summary>
        /// 2�����z�񂩂�V�����s����쐬����D
        /// </summary>
        /// <param name="arr">�s��̗v�f���i�[����2�����z��</param>
        public MyDoubleBandMatrix(double[,] arr)
        {
            System.Diagnostics.Debug.Assert(arr.GetLength(0) == arr.GetLength(1));
            if (arr.GetLength(0) != arr.GetLength(1))
            {
                Clear();
                return;
            }
            int rowcolSize = arr.GetLength(0);

            // subdia�T�C�Y�Asuperdia�T�C�Y���擾����
            int subdiaSize = 0;
            int superdiaSize = 0;
            for (int c = 0; c < rowcolSize; c++)
            {
                if (c < rowcolSize - 1)
                {
                    int cnt = 0;
                    for (int r = rowcolSize - 1; r >= c + 1; r--)
                    {
                        // ��O�v�f�����������甲����
                        if (Math.Abs(arr[r, c]) >= Constants.PrecisionLowerLimit)
                        {
                            cnt = r - c;
                            break;
                        }
                    }
                    if (cnt > subdiaSize)
                    {
                        subdiaSize = cnt;
                    }
                }
                if (c > 0)
                {
                    int cnt = 0;
                    for (int r = 0; r <= c - 1; r++)
                    {
                        // ��O�v�f�����������甲����
                        if (Math.Abs(arr[r, c]) >= Constants.PrecisionLowerLimit)
                        {
                            cnt = c - r;
                            break;
                        }
                    }
                    if (cnt > superdiaSize)
                    {
                        superdiaSize = cnt;
                    }
                }
            }

            // �o�b�t�@�̊m��
            Resize(rowcolSize, subdiaSize, superdiaSize);
            // �l���R�s�[����
            for (int c = 0; c < rowcolSize; ++c)
            {
                // �Ίp����
                this[c, c] = arr[c, c];

                // subdiagonal����
                if (c < rowcolSize - 1)
                {
                    for (int r = c + 1; r <= c + subdiaSize && r < rowcolSize; r++)
                    {
                        this[r, c] = arr[r, c];
                    }
                }
                if (c > 0)
                {
                    for (int r = c - 1; r >= c - superdiaSize && r >= 0; r--)
                    {
                        this[r, c] = arr[r, c];
                    }
                }
            }
        }

        /// <summary>
        /// ���̍s��̊e�v�f��ݒ�C�擾����D(�x�[�X�N���X��I/F�̃I�[�o�[���C�h)
        /// </summary>
        /// <param name="row">�sindex�i�͈́F[0, <see cref="RowSize"/>) �j</param>
        /// <param name="col">��index�i�͈́F[0, <see cref="ColumnSize"/>) �j</param>
        /// <returns>�v�f�̒l</returns>
        public override double this[int row, int col]
        {
            get
            {
                if (row < 0 || this.RowSize <= row || col < 0 || this.ColumnSize <= col)
                {
                    throw new IndexOutOfRangeException();
                }
                if (!(row >= col - this._superdiaSize && row <= col + this._subdiaSize))
                {
                    return 0.0;
                }
                return this._body[(row - col) +this._subdiaSize + this._superdiaSize + col * this._rsize];
            }
            set
            {
                if (row < 0 || this.RowSize <= row || col < 0 || this.ColumnSize <= col)
                {
                    throw new IndexOutOfRangeException();
                }
                if (!(row >= col - this._superdiaSize && row <= col + this._subdiaSize))
                {
                    return;
                }
                this._body[(row - col) + this._subdiaSize + this._superdiaSize + col * this._rsize] = value;
            }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g�̍s�����擾����D
        /// </summary>
        public override int RowSize
        {
            get { return this._rowcolSize; }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g�̗񐔂��擾����D
        /// </summary>
        public override int ColumnSize
        {
            get { return this._rowcolSize; }
        }

        /// <summary>
        /// subdiagonal�̃T�C�Y���擾����
        /// </summary>
        public int SubdiaSize
        {
            get { return this._subdiaSize; }
        }

        /// <summary>
        /// superdiaginal�̃T�C�Y���擾����
        /// </summary>
        public int SuperdiaSize
        {
            get { return this._superdiaSize; }
        }

        /// <summary>
        /// ���̃I�u�W�F�N�g���N���A����i<c>RowSize == 0 and ColumnSize == 0</c> �ɂȂ�j�D(�x�[�X�N���X��I/F�̃I�[�o�[���C�h)
        /// </summary>
        public override void Clear()
        {
            // �x�[�X�N���X�̃N���A�����s����
            base.Clear();

            //this._body = new double[0];
            //this._rsize = 0;
            //this._csize = 0;
            this._rowcolSize = 0;
            this._subdiaSize = 0;
            this._superdiaSize = 0;
        }

        /// <summary>
        /// ���T�C�Y����D���T�C�Y��̊e�v�f�l��0�ɂȂ�D
        /// </summary>
        /// <param name="rowcolSize">�V�����s��=�V������</param>
        /// <param name="subdiaSize">subdiagonal�̃T�C�Y</param>
        /// <param name="superdiaSize">subdiagonal�̃T�C�Y</param>
        /// <returns>���T�C�Y��̎��g�ւ̎Q��</returns>
        public virtual MyDoubleBandMatrix Resize(int rowcolSize, int subdiaSize, int superdiaSize)
        {
            int rsize = subdiaSize * 2 + superdiaSize + 1;
            int csize = rowcolSize;
            base.Resize(rsize, csize);
            //this._body = new double[rsize * csize];
            //this._rsize = rsize;
            //this._csize = csize;
            this._rowcolSize = rowcolSize;
            this._subdiaSize = subdiaSize;
            this._superdiaSize = superdiaSize;
            return this;
        }

        /// <summary>
        /// �x�[�X�N���X�̃��T�C�YI/F (����)
        /// </summary>
        /// <param name="rowSize"></param>
        /// <param name="columnSize"></param>
        /// <returns></returns>
        public override sealed MyDoubleMatrix Resize(int rowSize, int columnSize)
        {
            System.Diagnostics.Debug.Assert(false);
            //return base.Resize(rowSize, columnSize);
            return this;
        }

        /// <summary>
        /// �w�肳�ꂽ�s����R�s�[����D
        /// </summary>
        /// <param name="m">�R�s�[�����s��</param>
        /// <returns>�R�s�[��̎��g�ւ̎Q��</returns>
        public virtual MyDoubleBandMatrix CopyFrom(MyDoubleBandMatrix m)
        {
            return CopyFrom(m._body, m._rowcolSize, m._subdiaSize, m._superdiaSize);
        }

        /// <summary>
        /// �x�[�X�N���X�̃R�s�[I/F (����)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public override sealed MyDoubleMatrix CopyFrom(MyDoubleMatrix m)
        {
            System.Diagnostics.Debug.Assert(false);
            //return base.CopyFrom(m);
            return this;
        }

        /// <summary>
        /// <para>�w�肳�ꂽ1�����z����C�w�肳�ꂽ�s��`���ŃR�s�[����D</para>
        /// <para>�z��̃T�C�Y�ƁurowSize * columnSize�v�͈�v���Ȃ���΂Ȃ�Ȃ��D</para>
        /// </summary>
        /// <param name="body">�R�s�[�����z��</param>
        /// <param name="rowcolSize">�s��=��</param>
        /// <param name="subdiaSize">subdiagonal�̃T�C�Y</param>
        /// <param name="superdiaSize">superdiagonal�̃T�C�Y</param>
        /// <returns>�R�s�[��̎��g�ւ̎Q��</returns>
        internal virtual MyDoubleBandMatrix CopyFrom(double[] body, int rowcolSize, int subdiaSize, int superdiaSize)
        {
            int rsize = subdiaSize * 2 + superdiaSize + 1;
            int csize = rowcolSize;

            // ���͂̌���
            System.Diagnostics.Debug.Assert(body.Length == rsize * csize);
            if (body.Length != rsize * csize)
            {
                return this;
            }

            // �o�b�t�@�m��
            if (this._rsize == rsize && this._csize == csize)
            {
            }
            else if (this._body != null && this._body.Length == rsize * csize)
            {
                this._rsize = rsize;
                this._csize = csize;
            }
            else
            {
                base.Resize(rsize, csize);
            }
            this._rowcolSize = rowcolSize;
            this._subdiaSize = subdiaSize;
            this._superdiaSize = superdiaSize;

            // �R�s�[
            body.CopyTo(this._body, 0);
            return this;
        }

        /// <summary>
        /// �x�[�X�N���X�̃R�s�[I/F (����)
        /// </summary>
        /// <param name="body"></param>
        /// <param name="rowSize"></param>
        /// <param name="columnSize"></param>
        /// <returns></returns>
        internal override sealed MyDoubleMatrix CopyFrom(double[] body, int rowSize, int columnSize)
        {
            System.Diagnostics.Debug.Assert(false);
            //return base.CopyFrom(body, rowSize, columnSize);
            return this;
        } 

        /// <summary>
        /// �]�u����D(�x�[�X�N���X��I/F�̃I�[�o�[���C�h)
        /// </summary>
        /// <returns>�]�u��̎��g�ւ̎Q��</returns>
        public override MyDoubleMatrix Transpose()
        {
            //return base.Transpose();

            int rowcolSize = this._rowcolSize;
            int subdiaSize = 0;
            int superdiaSize = 0;
            // �]�u���subdiaSize, superdiaSize���擾����
            // r��c������ւ���Ă��邱�Ƃɒ���
            for (int r = 0; r < rowcolSize; r++)
            {
                if (r < rowcolSize - 1)
                {
                    int cnt = 0;
                    for (int c = rowcolSize - 1; c >= r + 1; c--)
                    {
                        // ��O�v�f�����������甲����
                        if (Math.Abs(this[c, r]) >= Constants.PrecisionLowerLimit)
                        {
                            cnt = c - r;
                            break;
                        }
                    }
                    if (cnt > subdiaSize)
                    {
                        subdiaSize = cnt;
                    }
                }
                if (r > 0)
                {
                    int cnt = 0;
                    for (int c = 0; c <= r - 1; c++)
                    {
                        // ��O�v�f�����������甲����
                        if (Math.Abs(this[c, r]) >= Constants.PrecisionLowerLimit)
                        {
                            cnt = r - c;
                            break;
                        }
                    }
                    if (cnt > superdiaSize)
                    {
                        superdiaSize = cnt;
                    }
                }
            }

            MyDoubleBandMatrix t = new MyDoubleBandMatrix(rowcolSize, subdiaSize, superdiaSize);
            for (int r = 0; r < this._rsize; ++r)
            {
                for (int c = 0; c < this._csize; ++c)
                {
                    t._body[(c - r) + t._subdiaSize + t._superdiaSize + r * t._rsize] = this._body[(r - c) + this._subdiaSize + this._superdiaSize + c * this._rsize];
                }
            }

            this.Clear();
            this._body = t._body;
            this._rsize = t._rsize;
            this._csize = t._csize;
            this._rowcolSize = t._rowcolSize;
            this._subdiaSize = t._subdiaSize;
            this._superdiaSize = t._superdiaSize;

            return this;
        }

        /// <summary>
        /// �s���1�����z��Ƃ��ďo�͂���
        /// </summary>
        /// <returns></returns>
        public override double[] ToArray1D()
        {
            double[] buffer = new double[this.RowSize * this.ColumnSize];

            int rowcolSize = this.RowSize;
            int subdiaSize = this.SubdiaSize;
            int superdiaSize = this.SuperdiaSize;
            
            for (int c = 0; c < rowcolSize; ++c)
            {
                // �Ίp����
                buffer[c + this.RowSize * c] = this._body[this.GetBufferIndex(c, c)];

                // subdiagonal����
                if (c < rowcolSize - 1)
                {
                    for (int r = c + 1; r <= c + subdiaSize && r < rowcolSize; r++)
                    {
                        buffer[r + this.RowSize * c] = this._body[this.GetBufferIndex(r, c)];
                    }
                }
                if (c > 0)
                {
                    for (int r = c - 1; r >= c - superdiaSize && r >= 0; r--)
                    {
                        buffer[r + this.RowSize * c] = this._body[this.GetBufferIndex(r, c)];
                    }
                }
            }
             
            /*
            for (int r = 0; r < this.RowSize; r++)
            {
                for (int c = 0; c < this.ColumnSize; c++)
                {
                    if (Math.Max(0, c - superdiaSize) <= r && r <= Math.Min((this.RowSize - 1), c + subdiaSize))
                    {
                        buffer[r + this.RowSize * c] = this._body[subdiaSize + superdiaSize + r - c + (2 * subdiaSize + superdiaSize + 1) * c];
                    }
                }
            }
             */

            return buffer;
        }

    }
}
