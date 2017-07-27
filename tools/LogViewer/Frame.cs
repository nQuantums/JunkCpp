using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LogViewer
{
    /// <summary>
    /// 対象処理の開始～終了範囲
    /// </summary>
    public struct Frame
    {
        /// <summary>
        /// レコード配列内での開始インデックス、見つからない場合は-1
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// レコード配列内での終了インデックス、見つからない場合はint.MaxValue
        /// </summary>
        public int EndIndex;

        public string Ip;
        public UInt32 Pid;
        public UInt32 Tid;
        public int Depth;
        public Color Color;

        public bool IsStartValid
        {
            get
            {
                return this.StartIndex != -1;
            }
        }

        public bool IsEndValid
        {
            get
            {
                return this.EndIndex != int.MaxValue;
            }
        }

        public int ValidIndex
        {
            get
            {
                if (this.StartIndex != -1)
                    return this.StartIndex;
                if (this.EndIndex != int.MaxValue)
                    return this.EndIndex;
                return -1;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Frame)
            {
                return this == (Frame)obj;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = this.StartIndex.GetHashCode();
            hash ^= this.EndIndex.GetHashCode();
            if (this.Ip != null)
                hash ^= this.Ip.GetHashCode();
            hash ^= this.Pid.GetHashCode();
            hash ^= this.Tid.GetHashCode();
            hash ^= this.Depth.GetHashCode();
            return hash;
        }

        public static bool operator ==(Frame a, Frame b)
        {
            if (a.StartIndex != b.StartIndex)
                return false;
            if (a.EndIndex != b.EndIndex)
                return false;
            if (a.Ip != b.Ip)
                return false;
            if (a.Pid != b.Pid)
                return false;
            if (a.Tid != b.Tid)
                return false;
            if (a.Depth != b.Depth)
                return false;
            return true;
        }

        public static bool operator !=(Frame a, Frame b)
        {
            return !(a == b);
        }


        /// <summary>
        /// 指定レコード配列内の指定インデックスのレコードに対応するフレームを検索する
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="index">検索対象レコードインデックス</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>フレーム</returns>
        public static Frame Search(Record[] records, int index, CancelInfo cancelInfo)
        {
            if (index < 0 || records.Length <= index)
                return new Frame();

            var curRecord = records[index];
            var frame = new Frame();
            frame.StartIndex = index;
            frame.EndIndex = index;
            frame.Ip = curRecord.Ip;
            frame.Pid = curRecord.Pid;
            frame.Tid = curRecord.Tid;
            frame.Depth = curRecord.Depth;

            // キャッシュにペアインデックスが記録されているならそっちを使用
            if (curRecord.CachedPairIndex != Record.NoCached)
            {
                if (curRecord.Enter)
                {
                    frame.EndIndex = curRecord.CachedPairIndex;
                }
                else
                {
                    frame.StartIndex = curRecord.CachedPairIndex;
                }
                return frame;
            }

            // 開始または終了レコードを探す
            if (curRecord.Enter)
            {
                frame.EndIndex = int.MaxValue;

                for (int i = index + 1; i < records.Length; i++)
                {
                    CancelInfo.Handle(cancelInfo);

                    var r = records[i];
                    if (r.Ip != frame.Ip)
                        continue;
                    if (r.Pid != frame.Pid)
                        continue;
                    if (r.Tid != frame.Tid)
                        continue;
                    if (r.Depth < frame.Depth)
                        break;
                    if (r.Depth == frame.Depth && !r.Enter)
                    {
                        frame.EndIndex = i;
                        break;
                    }
                }

                if (frame.EndIndex != int.MaxValue)
                    records[index].CachedPairIndex = frame.EndIndex;
            }
            else
            {
                frame.StartIndex = -1;

                for (int i = index - 1; i != -1; i--)
                {
                    CancelInfo.Handle(cancelInfo);

                    var r = records[i];
                    if (r.Ip != frame.Ip)
                        continue;
                    if (r.Pid != frame.Pid)
                        continue;
                    if (r.Tid != frame.Tid)
                        continue;
                    if (r.Depth < frame.Depth)
                        break;
                    if (r.Depth == frame.Depth && r.Enter)
                    {
                        frame.StartIndex = i;
                        break;
                    }
                }

                if (frame.StartIndex != -1)
                    records[index].CachedPairIndex = frame.StartIndex;
            }

            return frame;
        }

        /// <summary>
        /// 指定フレームの呼び出し元レコードインデックスを検索する
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchParentIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
        {
            if (frame.Depth == 0)
                return -1;

            // 親インデックスがキャッシュに記録されているならそっちを使用
            if (!frame.IsStartValid)
                return -1;
            var parentIndex = records[frame.StartIndex].CachedParentIndex;
            if (parentIndex != Record.NoCached)
                return parentIndex;

            // 呼び出し元なので深度をデクリメント
            frame.Depth--;

            for (int i = frame.StartIndex - 1; i != -1; i--)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
                if (r.Ip != frame.Ip)
                    continue;
                if (r.Pid != frame.Pid)
                    continue;
                if (r.Tid != frame.Tid)
                    continue;
                if (r.Depth < frame.Depth)
                    break;
                if (r.Depth == frame.Depth && r.Enter)
                {
                    records[frame.StartIndex].CachedParentIndex = i;
                    if (frame.IsEndValid)
                        records[frame.EndIndex].CachedParentIndex = i;
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 指定フレームの最初の呼び出し先レコードインデックスを検索する
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchChildIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
        {
            if (frame.Depth == 0)
                return -1;

            // 呼び出し先なので深度をインクリメント
            frame.Depth++;

            for (int i = frame.StartIndex + 1; i < frame.EndIndex; i++)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
                if (r.Ip != frame.Ip)
                    continue;
                if (r.Pid != frame.Pid)
                    continue;
                if (r.Tid != frame.Tid)
                    continue;
                if (r.Depth == frame.Depth && r.Enter)
                    return i;
            }

            return -1;
        }


        /// <summary>
        /// 指定フレームの次の処理の開始レコードを検索する ※同スレッド内の次の処理に相当
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchNextIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
        {
            if(!frame.IsEndValid)
                return -1;

            var parentIndex = SearchParentIndex(records, frame, cancelInfo);
            if(parentIndex < 0)
                return -1;

            var callerFrame = Search(records, parentIndex, cancelInfo);
            var start = frame.EndIndex + 1;
            var end = Math.Min(callerFrame.EndIndex, records.Length);

            for (int i = start; i < end; i++)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
                if (r.Ip != frame.Ip)
                    continue;
                if (r.Pid != frame.Pid)
                    continue;
                if (r.Tid != frame.Tid)
                    continue;
                if (r.Depth == frame.Depth && r.Enter)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定フレームの前の処理の開始レコードを検索する ※同スレッド内の前の処理に相当
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchPrevIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
        {
            if (!frame.IsStartValid)
                return -1;

            var parentIndex = SearchParentIndex(records, frame, cancelInfo);
            if (parentIndex < 0)
                return -1;

            var callerFrame = Search(records, parentIndex, cancelInfo);
            var start = callerFrame.StartIndex;
            var end = frame.StartIndex;

            for (int i = frame.StartIndex - 1; callerFrame.StartIndex < i; i--)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
                if (r.Ip != frame.Ip)
                    continue;
                if (r.Pid != frame.Pid)
                    continue;
                if (r.Tid != frame.Tid)
                    continue;
                if (r.Depth == frame.Depth && r.Enter)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定フレームのコールスタックテキストを作成する
        /// </summary>
        /// <param name="records">全レコード配列</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>コールスタックテキスト</returns>
        public static string GetCallStackText(Record[] records, Frame frame, CancelInfo cancelInfo)
        {
            var frameRecords = new List<Record>();
            frameRecords.Add(records[frame.StartIndex]);

            var f = frame;
            for (; ; )
            {
                var index = SearchParentIndex(records, f, cancelInfo);
                if (index < 0)
                    break;

                f = Search(records, index, cancelInfo);
                if (!f.IsStartValid)
                    break;

                frameRecords.Add(records[f.StartIndex]);
            }

            var sb = new StringBuilder();
            for (int i = frameRecords.Count - 1; i != -1; i--)
            {
                var r = frameRecords[i];
                sb.AppendLine(new string(' ', r.Depth * 4) + r.FrameName);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 処理キャンセル時の例外
    /// </summary>
    public class CancelException : ApplicationException
    {
        public CancelException()
            : base()
        {
        }
    }

    /// <summary>
    /// キャンセル情報
    /// </summary>
    public class CancelInfo
    {
        /// <summary>
        /// キャンセルするなら true に設定される
        /// </summary>
        public volatile bool Cancel;

        public static void Handle(CancelInfo cancelInfo)
        {
            if (cancelInfo == null)
                return;
            var cancel = cancelInfo.Cancel;
            if (cancel)
                throw new CancelException();
        }
    }
}
