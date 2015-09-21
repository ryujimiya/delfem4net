using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace wg2d.World
{
    /// <summary>
    /// ワールド座標系のループ情報
    /// </summary>
    public class Loop
    {
        /////////////////////////////////////////////////
        // 変数
        /////////////////////////////////////////////////
        /// <summary>
        /// ループID
        /// </summary>
        public uint LoopId
        {
            get;
            private set;
        }
        /// <summary>
        /// 媒質インデックス
        /// </summary>
        public int MediaIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Loop()
        {
            LoopId = 0;
            MediaIndex = 0;
        }

        /// <summary>
        /// コンストラクタ２
        /// </summary>
        /// <param name="loopId"></param>
        /// <param name="mediaIndex"></param>
        public Loop(uint loopId, int mediaIndex)
        {
            this.Set(loopId, mediaIndex);
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="src"></param>
        public Loop(Loop src)
        {
            this.Copy(src);
        }

        /// <summary>
        /// 有効?
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return LoopId != 0;
        }

        /// <summary>
        /// 値のセット
        /// </summary>
        /// <param name="loopId"></param>
        /// <param name="mediaIndex"></param>
        public void Set(uint loopId, int mediaIndex)
        {
            System.Diagnostics.Debug.Assert(loopId != 0);
            LoopId = loopId;
            MediaIndex = mediaIndex;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="src"></param>
        public void Copy(Loop src)
        {
            this.LoopId = src.LoopId;
            this.MediaIndex = src.MediaIndex;
        }
    }
}
