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

			var dmmv = document.Dmmv;
            var records = document.Records;
            var curRecord = records[index];
			var core = curRecord.GetCore(dmmv);
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
					var c = r.GetCore(dmmv);
                    if (c.Ip != frame.Ip)
                        continue;
                    if (c.Pid != frame.Pid)
                        continue;
                    if (c.Tid != frame.Tid)
                        continue;
                    if (c.Depth < frame.Depth)
                        break;
					if(frame.Depth < c.Depth) {
						// 子要素ならその終了を検索、キャッシュ持ってたら高速化される
						i = Search(document, i, cancelInfo).EndRecordIndex;
					} else {
						// 自分の終了か判定
						if (!c.Enter) {
							frame.EndRecordIndex = i;
							break;
						}
					}
				}

                if (frame.EndRecordIndex != NullEndIndex) {
					records[index].CachedPairIndex = frame.EndRecordIndex;
					records[frame.EndRecordIndex].CachedPairIndex = index;
				}
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
					var c = r.GetCore(dmmv);
                    if (c.Ip != frame.Ip)
                        continue;
                    if (c.Pid != frame.Pid)
                        continue;
                    if (c.Tid != frame.Tid)
                        continue;
                    if (c.Depth < frame.Depth)
                        break;
					if (frame.Depth < c.Depth) {
						// 子要素ならその開始を検索、キャッシュ持ってたら高速化される
						i = Search(document, i, cancelInfo).StartRecordIndex;
					} else {
						// 自分の開始か判定
						if (c.Enter) {
							frame.StartRecordIndex = i;
							break;
						}
					}
                }

                if (frame.StartRecordIndex != NullStartIndex) {
					records[index].CachedPairIndex = frame.StartRecordIndex;
					records[frame.StartRecordIndex].CachedPairIndex = index;
				}
			}

            return frame;
        }

		/// <summary>
		/// どちらかのインデックスが有効なら有効な方のプロパティを自動的に取得する
		/// </summary>
		struct IndexPair {
			public LogDocument Document;
			public DynamicMemMapView Dmmv;
			public MemMapRecord[] Records;
			public int Index1;
			public int Index2;
			public bool Valid1;
			public bool Valid2;

			public bool Valid => this.Valid1 || this.Valid2;
			public int StartIndex {
				get {
					if (this.Valid1) {
						if (this.Records[this.Index1].GetLogType(this.Dmmv) == MemMapRecord.LogType.Enter)
							return this.Index1;
					}
					if (this.Valid2) {
						if (this.Records[this.Index2].GetLogType(this.Dmmv) == MemMapRecord.LogType.Enter)
							return this.Index2;
					}
					if (this.Valid1) {
						var f = Search(this.Document, this.Index1, null);
						if (!this.Valid2)
							this.Index2 = f.StartRecordIndex;
						return f.StartRecordIndex;
					}
					if (this.Valid2) {
						var f = Search(this.Document, this.Index2, null);
						if (!this.Valid1)
							this.Index1 = f.StartRecordIndex;
						return f.StartRecordIndex;
					}
					return NullStartIndex;
				}
			}
			public int EndIndex => Math.Max(this.Index1, this.Index2);
			public int ParentIndex {
				get {
					var parentIndex1 = this.Valid1 ? this.Records[this.Index1].CachedParentIndex : -1;
					var parentIndex2 = this.Valid2 ? this.Records[this.Index2].CachedParentIndex : -1;
					if (0 <= parentIndex1) {
						if (parentIndex2 == Record.NoCached)
							this.Records[this.Index2].CachedParentIndex = parentIndex1;
						return parentIndex1;
					}
					if (0 <= parentIndex2) {
						if (parentIndex1 == Record.NoCached)
							this.Records[this.Index1].CachedParentIndex = parentIndex2;
						return parentIndex2;
					}
					return Record.NoCached;
				}
				set {
					if (this.Valid1) {
						this.Records[this.Index1].CachedParentIndex = value;
					}
					if (this.Valid2) {
						this.Records[this.Index2].CachedParentIndex = value;
					}
				}
			}
			public MemMapRecord.Core Core {
				get {
					if (this.Valid1) {
						return this.Records[this.Index1].GetCore(this.Dmmv);
					}
					if (this.Valid2) {
						return this.Records[this.Index2].GetCore(this.Dmmv);
					}
					return new MemMapRecord.Core();
				}
			}

			public IndexPair(LogDocument document, int index1, int index2) {
				this.Document = document;
				this.Dmmv = document.Dmmv;
				this.Records = document.Records;
				this.Index1 = index1;
				this.Index2 = index2;
				this.Valid1 = document.IsValidIndex(index1);
				this.Valid2 = document.IsValidIndex(index2);
			}
		}

		/// <summary>
		/// 指定インデックスの呼び出し元インデックスを検索する
		/// </summary>
		/// <param name="document">検索対象ログドキュメント</param>
		/// <param name="index1">対象インデックス(フレームの開始か終了のどちらか index2 の逆側)</param>
		/// <param name="index2">対象インデックス(フレームの開始か終了のどちらか index1 の逆側)</param>
		/// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
		/// <returns>レコードインデックス1、レコードインデックス2、見つからなかったら負数</returns>
		public static Tuple<int, int> SearchParentIndex(LogDocument document, int index1, int index2, CancelInfo cancelInfo) {
			var ip = new IndexPair(document, index1, index2);
			if (!ip.Valid) {
				return new Tuple<int, int>(-1, -1);
			}

			// 親インデックスがキャッシュに記録されているならそっちを使用
			var dmmv = document.Dmmv;
			var records = document.Records;
			var parentIndex = ip.ParentIndex;
			if (parentIndex != Record.NoCached) {
				return new Tuple<int, int>(parentIndex, -1);
			}

			// 検索対象範囲を設定
			int start = ip.StartIndex;
			if (start < 0) {
				return new Tuple<int, int>(-1, -1);
			}
			var end = document.StartRecordIndex;

			// 呼び出し元なので深度をデクリメント
			var core = ip.Core;
			core.Depth--; // 親を探すので深度デクリメント
			for (int i = start; end <= i; i--) {
				CancelInfo.Handle(cancelInfo);

				var r = records[i];
				var c = r.GetCore(dmmv);
				if (c.Ip != core.Ip)
					continue;
				if (c.Pid != core.Pid)
					continue;
				if (c.Tid != core.Tid)
					continue;
				if (c.Depth < core.Depth)
					break;
				if (core.Depth < c.Depth) {
					// 子要素ならその開始を検索、キャッシュ持ってたら高速化される
					if (c.LogType == MemMapRecord.LogType.Leave)
						i = Search(document, i, cancelInfo).StartRecordIndex;
				} else {
					// 自分の開始か判定
					if (c.Enter) {
						ip.ParentIndex = i;
						return new Tuple<int, int>(i, r.CachedPairIndex);
					}
				}
			}

			// 見つからなかったことを記録
			ip.ParentIndex = MemMapRecord.NoParent;

			return new Tuple<int, int>(MemMapRecord.NoParent, MemMapRecord.NoParent);
		}

		/// <summary>
		/// 指定フレームの呼び出し元フレームを検索する
		/// </summary>
		/// <param name="document">検索対象ログドキュメント</param>
		/// <param name="frame">対象フレーム</param>
		/// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
		/// <returns>フレーム</returns>
		public static Frame SearchParentFrame(LogDocument document, Frame frame, CancelInfo cancelInfo)
        {
			return Search(document, SearchParentIndex(document, frame.StartRecordIndex, frame.EndRecordIndex, cancelInfo).Item1, cancelInfo);
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
		/// <param name="index1">対象インデックス(フレームの開始か終了のどちらか index2 の逆側)</param>
		/// <param name="index2">対象インデックス(フレームの開始か終了のどちらか index1 の逆側)</param>
		/// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
		/// <returns>コールスタックテキスト</returns>
		public static string GetCallStackText(LogDocument document, int index1, int index2, CancelInfo cancelInfo)
        {
            if (!document.IsValidIndex(index1))
                return "";

			var dmmv = document.Dmmv;
			var records = document.Records;
            var frameRecords = new List<MemMapRecord>();
            frameRecords.Add(records[index1]);

			var p = new Tuple<int, int>(index1, index2);
            for (; ; )
            {
                p = SearchParentIndex(document, p.Item1, p.Item2, cancelInfo);
				if (p.Item1 < 0 || p.Item1 == MemMapRecord.NoParent)
					break;
				frameRecords.Add(records[p.Item1]);
            }

            var sb = new StringBuilder();
            for (int i = frameRecords.Count - 1; i != -1; i--)
            {
				var c = frameRecords[i].GetCore(dmmv);
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
