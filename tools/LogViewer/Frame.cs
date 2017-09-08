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
        public const int NullStartIndex = -1;
        public const int NullEndIndex = int.MaxValue;
        public static readonly Frame InvalidFrame = new Frame { StartRecordIndex = NullStartIndex, EndRecordIndex = NullEndIndex, Depth = -1 };

        /// <summary>
        /// レコード配列内での開始インデックス、見つからない場合は<see cref="NullStartIndex"/>
        /// </summary>
        public int StartRecordIndex;

        /// <summary>
        /// レコード配列内での終了インデックス、見つからない場合は<see cref="NullEndIndex"/>
        /// </summary>
        public int EndRecordIndex;

        public string Ip;
        public UInt32 Pid;
        public UInt32 Tid;
        public int Depth;
        public Color Color;

        public bool IsValid
        {
            get
            {
                return 1 <= this.Depth;
            }
        }

        public bool IsStartValid
        {
            get
            {
                return this.StartRecordIndex != NullStartIndex;
            }
        }

        public bool IsEndValid
        {
            get
            {
                return this.EndRecordIndex != NullEndIndex;
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
            int hash = this.StartRecordIndex.GetHashCode();
            hash ^= this.EndRecordIndex.GetHashCode();
            if (this.Ip != null)
                hash ^= this.Ip.GetHashCode();
            hash ^= this.Pid.GetHashCode();
            hash ^= this.Tid.GetHashCode();
            hash ^= this.Depth.GetHashCode();
            return hash;
        }

        public static bool operator ==(Frame a, Frame b)
        {
            if (a.StartRecordIndex != b.StartRecordIndex)
                return false;
            if (a.EndRecordIndex != b.EndRecordIndex)
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

        public void AdjustRecordIndexBy(LogDocument document)
        {
            if (!document.IsValidIndex(this.StartRecordIndex))
                this.StartRecordIndex = NullStartIndex;
            if (!document.IsValidIndex(this.EndRecordIndex))
                this.EndRecordIndex = NullEndIndex;
        }


        /// <summary>
        /// 指定レコード配列内の指定インデックスのレコードに対応するフレームを検索する
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="index">検索対象レコードインデックス</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>フレーム</returns>
        public static Frame Search(LogDocument document, int index, CancelInfo cancelInfo)
        {
            if (!document.IsValidIndex(index))
                return InvalidFrame;

            var records = document.Records;
            var curRecord = records[index];
			var core = curRecord.GetCore(document.Dmmv);
            var frame = new Frame();
            frame.StartRecordIndex = index;
            frame.EndRecordIndex = index;
            frame.Ip = core.Ip;
            frame.Pid = core.Pid;
            frame.Tid = core.Tid;
            frame.Depth = core.Depth;

            // キャッシュにペアインデックスが記録されているならそっちを使用
            if (curRecord.CachedPairIndex != Record.NoCached)
            {
                if (core.Enter)
                {
                    frame.EndRecordIndex = curRecord.CachedPairIndex;
                }
                else
                {
                    frame.StartRecordIndex = curRecord.CachedPairIndex;
                }
                frame.AdjustRecordIndexBy(document);
                return frame;
            }

            // 開始または終了レコードを探す
            if (core.Enter)
            {
                frame.EndRecordIndex = NullEndIndex;

                var start = index + 1;
                var end = document.EndRecordIndex;
                for (int i = start; i <= end; i++)
                {
                    CancelInfo.Handle(cancelInfo);

                    var r = records[i];
					var c = r.GetCore(document.Dmmv);
                    if (c.Ip != frame.Ip)
                        continue;
                    if (c.Pid != frame.Pid)
                        continue;
                    if (c.Tid != frame.Tid)
                        continue;
                    if (c.Depth < frame.Depth)
                        break;
                    if (c.Depth == frame.Depth && !c.Enter)
                    {
                        frame.EndRecordIndex = i;
                        break;
                    }
                }

                if (frame.EndRecordIndex != NullEndIndex)
                    records[index].CachedPairIndex = frame.EndRecordIndex;
            }
            else
            {
                frame.StartRecordIndex = NullStartIndex;

                var start = index - 1;
                var end = document.StartRecordIndex;
                for (int i = start; end <= i; i--)
                {
                    CancelInfo.Handle(cancelInfo);

                    var r = records[i];
					var c = r.GetCore(document.Dmmv);
                    if (c.Ip != frame.Ip)
                        continue;
                    if (c.Pid != frame.Pid)
                        continue;
                    if (c.Tid != frame.Tid)
                        continue;
                    if (c.Depth < frame.Depth)
                        break;
                    if (c.Depth == frame.Depth && c.Enter)
                    {
                        frame.StartRecordIndex = i;
                        break;
                    }
                }

                if (frame.StartRecordIndex != NullStartIndex)
                    records[index].CachedPairIndex = frame.StartRecordIndex;
            }

            return frame;
        }

        /// <summary>
        /// 指定フレームの呼び出しフレームを検索する
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>フレーム</returns>
        public static Frame SearchParentFrame(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
            if (!frame.IsValid || !document.IsValidIndex(frame))
                return InvalidFrame;

            // 親インデックスがキャッシュに記録されているならそっちを使用
            if (!frame.IsStartValid)
                return new Frame { StartRecordIndex = NullStartIndex, EndRecordIndex = NullEndIndex };
            var records = document.Records;
            var parentIndex = records[frame.StartRecordIndex].CachedParentIndex;
            if (parentIndex != Record.NoCached)
                return Search(document, parentIndex, cancelInfo);

            // 呼び出し元なので深度をデクリメント
            frame.Depth--;

            var start = frame.StartRecordIndex - 1;
            var end = document.StartRecordIndex;
            for (int i = start; end <= i; i--)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
				var c = r.GetCore(document.Dmmv);
                if (c.Ip != frame.Ip)
                    continue;
                if (c.Pid != frame.Pid)
                    continue;
                if (c.Tid != frame.Tid)
                    continue;
                if (c.Depth < frame.Depth)
                    break;
                if (c.Depth == frame.Depth && c.Enter)
                {
                    records[frame.StartRecordIndex].CachedParentIndex = i;
                    if (frame.IsEndValid)
                        records[frame.EndRecordIndex].CachedParentIndex = i;
                    return Search(document, i, cancelInfo);
                }
            }

            return InvalidFrame;
        }

        /// <summary>
        /// 指定フレームの最初の呼び出し先フレームを検索する
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>フレーム</returns>
        public static Frame SearchChildFrame(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
            if (!frame.IsValid || !document.IsValidIndex(frame))
                return InvalidFrame;

            // 呼び出し先なので深度をインクリメント
            frame.Depth++;

            var records = document.Records;
            var start = frame.StartRecordIndex + 1;
            var end = frame.EndRecordIndex - 1;
            for (int i = start; i <= end; i++)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
				var c = r.GetCore(document.Dmmv);
                if (c.Ip != frame.Ip)
                    continue;
                if (c.Pid != frame.Pid)
                    continue;
                if (c.Tid != frame.Tid)
                    continue;
                if (c.Depth == frame.Depth && c.Enter)
                    return Search(document, i, cancelInfo);
            }

            return InvalidFrame;
        }


        /// <summary>
        /// 指定フレームの次の処理の開始レコードを検索する ※同スレッド内の次の処理に相当
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchNextIndex(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
            if(!frame.IsEndValid)
                return -1;

            var parentFrame = SearchParentFrame(document, frame, cancelInfo);
            parentFrame.AdjustRecordIndexBy(document);

            var records = document.Records;
            var start = frame.EndRecordIndex + 1;
            var end = Math.Min(parentFrame.EndRecordIndex - 1, document.EndRecordIndex);

            for (int i = start; i <= end; i++)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
				var c = r.GetCore(document.Dmmv);
                if (c.Ip != frame.Ip)
                    continue;
                if (c.Pid != frame.Pid)
                    continue;
                if (c.Tid != frame.Tid)
                    continue;
                if (c.Depth < frame.Depth)
                    break;
                if (c.Depth == frame.Depth && c.Enter)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定フレームの前の処理の開始レコードを検索する ※同スレッド内の前の処理に相当
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>レコードインデックス、見つからないなら負数</returns>
        public static int SearchPrevIndex(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
            if (!frame.IsStartValid)
                return -1;

            var parentFrame = SearchParentFrame(document, frame, cancelInfo);
            parentFrame.AdjustRecordIndexBy(document);

            var records = document.Records;
            var start = frame.StartRecordIndex - 1;
            var end = Math.Max(parentFrame.StartRecordIndex + 1, document.StartRecordIndex);

            for (int i = start; end <= i; i--)
            {
                CancelInfo.Handle(cancelInfo);

                var r = records[i];
				var c = r.GetCore(document.Dmmv);
                if (c.Ip != frame.Ip)
                    continue;
                if (c.Pid != frame.Pid)
                    continue;
                if (c.Tid != frame.Tid)
                    continue;
                if (c.Depth < frame.Depth)
                    break;
                if (c.Depth == frame.Depth && c.Enter)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 指定フレームのコールスタックテキストを作成する
        /// </summary>
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="frame">対象フレーム</param>
        /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
        /// <returns>コールスタックテキスト</returns>
        public static string GetCallStackText(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
            if (!document.IsValidIndex(frame.StartRecordIndex))
                return "";

            var records = document.Records;
            var frameRecords = new List<MemMapRecord>();
            frameRecords.Add(records[frame.StartRecordIndex]);

            var f = frame;
            for (; ; )
            {
                f = SearchParentFrame(document, f, cancelInfo);
                if (!f.IsStartValid)
                    break;

                frameRecords.Add(records[f.StartRecordIndex]);
            }

            var sb = new StringBuilder();
            for (int i = frameRecords.Count - 1; i != -1; i--)
            {
                var r = frameRecords[i];
				var c = r.GetCore(document.Dmmv);
                sb.AppendLine(new string(' ', c.Depth * 4) + c.FrameName);
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
