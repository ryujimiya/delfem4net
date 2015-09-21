using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace wg2d.World
{
    /// <summary>
    /// ワールド座標系の辺情報
    /// </summary>
    public class Edge
    {
        /////////////////////////////////////////////////
        // 変数
        /////////////////////////////////////////////////
        /// <summary>
        /// 辺ID
        /// </summary>
        public uint EdgeId
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
        public Edge()
        {
            EdgeId = 0;
            MediaIndex = 0;
        }

        /// <summary>
        /// コンストラクタ２
        /// </summary>
        /// <param name="loopId"></param>
        /// <param name="mediaIndex"></param>
        public Edge(uint loopId, int mediaIndex)
        {
            this.Set(loopId, mediaIndex);
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="src"></param>
        public Edge(Edge src)
        {
            this.Copy(src);
        }

        /// <summary>
        /// 有効?
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return EdgeId != 0;
        }

        /// <summary>
        /// 値のセット
        /// </summary>
        /// <param name="loopId"></param>
        /// <param name="mediaIndex"></param>
        public void Set(uint loopId, int mediaIndex)
        {
            System.Diagnostics.Debug.Assert(loopId != 0);
            EdgeId = loopId;
            MediaIndex = mediaIndex;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="src"></param>
        public void Copy(Edge src)
        {
            this.EdgeId = src.EdgeId;
            this.MediaIndex = src.MediaIndex;
        }
    }
}
