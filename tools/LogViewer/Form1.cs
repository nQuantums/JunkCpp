using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace LogViewer {
	public partial class Form1 : Form {
		static Color[] ColorTable = new Color[] {
			Color.FromArgb(127, 127, 255),
			Color.FromArgb(127, 255, 127),
			Color.FromArgb(127, 255, 255),
			Color.FromArgb(255, 127, 127),
			Color.FromArgb(255, 127, 255),
			Color.FromArgb(255, 255, 127),
		};
		static int ColorIndex = 0;

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

        /// <summary>
        /// 対象処理の開始～終了範囲
        /// </summary>
		public struct Frame {
			public int StartIndex;
			public int EndIndex;
			public string Ip;
			public UInt32 Pid;
			public UInt32 Tid;
			public int Depth;
			public Color Color;

			public override bool Equals(object obj) {
				if (obj is Frame) {
					return this == (Frame)obj;
				}
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				int hash = this.StartIndex.GetHashCode();
				hash ^= this.EndIndex.GetHashCode();
				if (this.Ip != null)
					hash ^= this.Ip.GetHashCode();
				hash ^= this.Pid.GetHashCode();
				hash ^= this.Tid.GetHashCode();
				hash ^= this.Depth.GetHashCode();
				return hash;
			}

			public static bool operator ==(Frame a, Frame b) {
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

			public static bool operator !=(Frame a, Frame b) {
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
                if (0 <= curRecord.CachedPairIndex)
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
                        frame.EndIndex = i;
                        if (r.Depth == frame.Depth && !r.Enter)
                            break;
                    }
                    records[index].CachedPairIndex = frame.EndIndex;
                }
                else
                {
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
                        frame.StartIndex = i;
                        if (r.Depth == frame.Depth && r.Enter)
                            break;
                    }
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
            /// <returns>レコードインデックス</returns>
            public static int SearchParentIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
            {
                if (frame.Depth == 0)
                    return frame.StartIndex;

                // 親インデックスがキャッシュに記録されているならそっちを使用
                var parentIndex = records[frame.StartIndex].CachedParentIndex;
                if (0 <= parentIndex)
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
                        records[frame.EndIndex].CachedParentIndex = i;
                        return i;
                    }
                }

                // 見つからなかったので現在フレームの開始を返しておく
                records[frame.StartIndex].CachedParentIndex = frame.StartIndex;
                records[frame.EndIndex].CachedParentIndex = frame.StartIndex;
                return frame.StartIndex;
            }

            /// <summary>
            /// 指定フレームの最初の呼び出し先レコードインデックスを検索する
            /// </summary>
            /// <param name="records">全レコード配列</param>
            /// <param name="frame">対象フレーム</param>
            /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
            /// <returns>レコードインデックス</returns>
            public static int SearchChildIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
            {
                if (frame.Depth == 0)
                    return frame.StartIndex;

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

                // 見つからなかったので現在フレームの開始を返しておく
                return frame.StartIndex;
            }


            /// <summary>
            /// 指定フレームの次の処理の開始レコードを検索する ※同スレッド内の次の処理に相当
            /// </summary>
            /// <param name="records">全レコード配列</param>
            /// <param name="frame">対象フレーム</param>
            /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
            /// <returns>レコードインデックス</returns>
            public static int SearchNextIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
            {
                var callerFrame = Search(records, SearchParentIndex(records, frame, cancelInfo), cancelInfo);

                for (int i = frame.EndIndex + 1; i < callerFrame.EndIndex; i++)
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

                return frame.EndIndex;
            }

            /// <summary>
            /// 指定フレームの前の処理の開始レコードを検索する ※同スレッド内の前の処理に相当
            /// </summary>
            /// <param name="records">全レコード配列</param>
            /// <param name="frame">対象フレーム</param>
            /// <param name="cancelInfo">処理キャンセルを行えるようにするなら null 以外を指定する</param>
            /// <returns>レコードインデックス</returns>
            public static int SearchPrevIndex(Record[] records, Frame frame, CancelInfo cancelInfo)
            {
                var callerFrame = Search(records, SearchParentIndex(records, frame, cancelInfo), cancelInfo);

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

                return frame.StartIndex;
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
                    if (index == f.StartIndex)
                        break;
                    f = Search(records, index, cancelInfo);
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

		string _CsvFileName;
		Record[] _Records = new Record[0];
		Record[] _Interrupts = new Record[0];
		Frame _CurrentFrame;
		List<Frame> _MarkedFrames = new List<Frame>();

        /// <summary>
        /// 現在対象となるフレーム
        /// </summary>
		public Frame CurrentFrame {
			get {
				return _CurrentFrame;
			}
			set {
				if (_CurrentFrame == value)
					return;

                // 変更行範囲取得
                int start = int.MinValue;
                int end = int.MinValue;
                if (_CurrentFrame.Depth != 0)
                {
                    start = _CurrentFrame.StartIndex;
                    end = _CurrentFrame.EndIndex;
                }
                if (value.Depth != 0)
                {
                    start = start != int.MinValue ? Math.Min(start, value.StartIndex) : value.StartIndex;
                    end = end != int.MinValue ? Math.Min(end, value.EndIndex) : value.EndIndex;
                }

                // 値更新
                _CurrentFrame = value;
				_CurrentFrame.Color = Color.FromArgb(192, 200, 255);

                // 変更される行範囲があれば変更を予約する
                if (start != int.MinValue && end != int.MinValue)
                    UpdateRecordsListItem(start, end);

                // 現在フレーム実行中に割り込まれた他スレッドの処理を表示
				SetInterrupts(QueryInterrupts(_CurrentFrame).ToArray());
                // コールスタックを表示
                this.tbCallStack.Text = Frame.GetCallStackText(GetRecords(), _CurrentFrame, null);
			}
		}


		public Form1() {
			InitializeComponent();

			this.lvRecords.GetType().InvokeMember(
			   "DoubleBuffered",
			   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
			   null,
			   this.lvRecords,
			   new object[] { true });
			this.lvRecords.VirtualMode = true;
			this.lvRecords.RetrieveVirtualItem += LvRecords_RetrieveVirtualItem;
			this.lvRecords.SelectedIndexChanged += LvRecords_SelectedIndexChanged;
            this.lvRecords.KeyDown += lvRecords_KeyDown;

			this.lvInterrupts.GetType().InvokeMember(
			   "DoubleBuffered",
			   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
			   null,
			   this.lvInterrupts,
			   new object[] { true });
			this.lvInterrupts.VirtualMode = true;
			this.lvInterrupts.RetrieveVirtualItem += LvInterrupts_RetrieveVirtualItem;
		}

		void SetRecords(Record[] records) {
			this.lvRecords.BeginUpdate();
			try {
				_Records = records;
				this.lvRecords.VirtualListSize = 0;
				this.lvRecords.VirtualListSize = _Records.Length;
			} finally {
				this.lvRecords.EndUpdate();
			}
		}

        /// <summary>
        /// 現在のレコード配列の取得
        /// </summary>
		public Record[] GetRecords() {
			return _Records;
		}

        /// <summary>
        /// マークするフレームの追加
        /// </summary>
        /// <param name="range">フレーム</param>
        public void AddMarkedFrame(Frame range)
        {
            range.Color = ColorTable[ColorIndex++ % ColorTable.Length];
            _MarkedFrames.Add(range);
            UpdateRecordsListItem(range.StartIndex, range.EndIndex);

            var lvi = new ListViewItem();
            lvi.UseItemStyleForSubItems = false;
            lvi.Text = (range.StartIndex + 1).ToString() + "～" + (range.EndIndex + 1).ToString();
            lvi.SubItems.Add("");
            lvi.SubItems.Add(range.Ip);
            lvi.SubItems.Add(range.Pid.ToString());
            lvi.SubItems.Add(range.Tid.ToString());
            lvi.SubItems.Add(GetRecords()[range.StartIndex].FrameName);
            lvi.SubItems[1].BackColor = range.Color;
            this.lvSelRanges.Items.Add(lvi);
        }

        /// <summary>
        /// マークするフレームの削除
        /// </summary>
        /// <param name="index">範囲インデックス</param>
        public void RemoveMarkedFrameAt(int index)
        {
            if (index < 0 || _MarkedFrames.Count <= index)
                return;

            var range = _MarkedFrames[index];
            _MarkedFrames.RemoveAt(index);
            UpdateRecordsListItem(range.StartIndex, range.EndIndex);

            this.lvSelRanges.Items.RemoveAt(index);
        }

		void SetInterrupts(Record[] records) {
			this.lvInterrupts.BeginUpdate();
			try {
				_Interrupts = records;
				this.lvInterrupts.VirtualListSize = 0;
				this.lvInterrupts.VirtualListSize = _Interrupts.Length;
			} finally {
				this.lvInterrupts.EndUpdate();
			}
		}

		public Record[] GetInterrupts() {
			return _Interrupts;
		}

        public HashSet<Record> QueryInterrupts(Frame range) {
			var result = new HashSet<Record>();
			var records = GetRecords();
			for (int i = range.StartIndex + 1; i < range.EndIndex; i++) {
				var r = records[i];
				// 本流の処理は無視
				if (r.Ip == range.Ip && r.Pid == range.Pid && r.Tid == range.Tid)
					continue;

				// 他スレッドの処理を解析
				result.Add(r);
			}
			return result;
		}

        public Frame SearchCurrentFrame()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return new Frame();
            return Frame.Search(GetRecords(), si[0], null);
        }

		public int SearchParentIndex() {
            return Frame.SearchParentIndex(GetRecords(), this.CurrentFrame, null);
		}

		public int SearchChildIndex() {
            return Frame.SearchChildIndex(GetRecords(), this.CurrentFrame, null);
        }

		public int SearchNextIndex() {
            return Frame.SearchNextIndex(GetRecords(), this.CurrentFrame, null);
        }

		public int SearchPrevIndex() {
            return Frame.SearchPrevIndex(GetRecords(), this.CurrentFrame, null);
        }

        /// <summary>
        /// レコード表示を更新する
        /// </summary>
        /// <param name="start">更新開始レコードインデックス</param>
        /// <param name="end">更新終了レコードインデックス</param>
		void UpdateRecordsListItem(int start = -1, int end = -1) {
			if (start < 0)
				start = 0;
			if (end < 0 || this.lvRecords.VirtualListSize <= end)
				end = this.lvRecords.VirtualListSize - 1;
			if (end < start)
				return;
			this.lvRecords.RedrawItems(start, end, true);
		}

        /// <summary>
        /// レコード検索
        /// </summary>
        /// <param name="forward">進みながら検索するなら true</param>
        /// <param name="ip">検索対象IP、 null が指定されたらIPで絞らない</param>
        /// <param name="pid">検索対象プロセスID、0 が指定されたらIPで絞らない</param>
        /// <param name="pid">検索対象スレッドID、0 が指定されたらIPで絞らない</param>
        /// <param name="method">検索対象処理内容文字列、大小無視で部分一致で検索される、null が指定されたら絞らない</param>
		void Search(bool forward, string ip, UInt32 pid, UInt32 tid, string method) {
			var si = this.lvRecords.SelectedIndices;
			var startIndex = si.Count != 0 ? si[0] : -1;
			var records = GetRecords();

			if (forward) {
				for (int i = startIndex + 1; i < records.Length; i++) {
					var r = records[i];
					if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid) && (method == null || 0 <= r.FrameName.IndexOf(method, StringComparison.CurrentCultureIgnoreCase))) {
						si.Clear();
						si.Add(i);
						this.lvRecords.EnsureVisible(i);
						break;
					}
				}
			} else {
				for (int i = startIndex - 1; 0 <= i; i--) {
					var r = records[i];
                    if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid) && (method == null || 0 <= r.FrameName.IndexOf(method, StringComparison.CurrentCultureIgnoreCase))) {
						si.Clear();
						si.Add(i);
						this.lvRecords.EnsureVisible(i);
						break;
					}
				}
			}
		}

        /// <summary>
        /// 現在の検索設定でレコード検索
        /// </summary>
        /// <param name="forward">進みながら検索するなら true</param>
        void Search(bool forward)
        {
            string ip;
            UInt32 pid;
            UInt32 tid;
            string method;

            ip = this.tbIp.Text;
            if (string.IsNullOrEmpty(ip))
                ip = null;
            method = this.tbMethod.Text;
            if (string.IsNullOrEmpty(method))
                method = null;
            UInt32.TryParse(this.tbPid.Text, out pid);
            UInt32.TryParse(this.tbTid.Text, out tid);

            Search(forward, ip, pid, tid, method);
        }

        void MoveFrameEnterLeave(int mode)
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return;

            var frame = this.CurrentFrame;
            int index;

            switch (mode)
            {
                case 0:
                default:
                    index = si[0] == frame.StartIndex ? frame.EndIndex : frame.StartIndex;
                    break;
                case 1:
                    index = frame.StartIndex;
                    break;
                case 2:
                    index = frame.EndIndex;
                    break;
            }

            si.Clear();
            si.Add(index);
            this.lvRecords.Items[index].Focused = true;
            this.lvRecords.EnsureVisible(index);
        }

        void MoveFrameParent()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return;

            var index = SearchParentIndex();

            si.Clear();
            si.Add(index);
            this.lvRecords.Items[index].Focused = true;
            this.lvRecords.EnsureVisible(index);
        }

        void MoveFrameChild()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return;

            var index = SearchChildIndex();

            si.Clear();
            si.Add(index);
            this.lvRecords.Items[index].Focused = true;
            this.lvRecords.EnsureVisible(index);
        }

        void MoveFrameNext()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return;

            var index = SearchNextIndex();

            si.Clear();
            si.Add(index);
            this.lvRecords.Items[index].Focused = true;
            this.lvRecords.EnsureVisible(index);
        }

        void MoveFramePrev()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return;

            var index = SearchPrevIndex();

            si.Clear();
            si.Add(index);
            this.lvRecords.Items[index].Focused = true;
            this.lvRecords.EnsureVisible(index);
        }

        void Reopen()
        {
            SetRecords(Record.ReadFromCsv(_CsvFileName));
        }

		private void tsmiOpen_Click(object sender, EventArgs e) {
			//OpenFileDialogクラスのインスタンスを作成
			using (var ofd = new OpenFileDialog()) {
				ofd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.*)|*.*";
				ofd.FilterIndex = 0;
				ofd.Title = "開くログファイルを選択してください";
				ofd.CheckFileExists = true;
				ofd.CheckPathExists = true;
				if (ofd.ShowDialog() != DialogResult.OK)
					return;

				SetRecords(Record.ReadFromCsv(ofd.FileName));
				_CsvFileName = ofd.FileName;
			}
		}

		private void tsmiReopen_Click(object sender, EventArgs e) {
            Reopen();
		}

		private void btnSearch_Click(object sender, EventArgs e) {
			Search(sender == this.btnSearchForward);
		}

		private void btnSelAdd_Click(object sender, EventArgs e) {
			AddMarkedFrame(this.CurrentFrame);
		}

		private void btnSelDel_Click(object sender, EventArgs e) {
			var si = this.lvSelRanges.SelectedIndices;
			if (si.Count == 0)
				return;
			RemoveMarkedFrameAt(si[0]);
		}

		private void lvSelRanges_DoubleClick(object sender, EventArgs e) {
			var si = this.lvSelRanges.SelectedIndices;
			if (si.Count == 0)
				return;
			var index = si[0];
			if (index < 0 || _MarkedFrames.Count <= index)
				return;

			var range = _MarkedFrames[index];
			this.lvRecords.EnsureVisible(range.StartIndex);
		}

		private void btnJunpEnterLeave_Click(object sender, EventArgs e) {
            MoveFrameEnterLeave(0);
		}

		private void btnJumpSource_Click(object sender, EventArgs e) {
            MoveFrameParent();
		}

		private void btnJumpDest_Click(object sender, EventArgs e) {
            MoveFrameChild();
		}

		private void btnJumpNext_Click(object sender, EventArgs e) {
            MoveFrameNext();
		}

		private void btnJumpPrev_Click(object sender, EventArgs e) {
            MoveFramePrev();
		}

		private void LvRecords_SelectedIndexChanged(object sender, EventArgs e) {
			this.CurrentFrame = SearchCurrentFrame();
		}

        void lvRecords_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Up)
                {
                    MoveFramePrev();
                    e.Handled = true;
                } else if (e.KeyCode == Keys.Down)
                {
                    MoveFrameNext();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    MoveFrameParent();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    MoveFrameChild();
                    e.Handled = true;
                }
            }
            else if (e.Shift)
            {
                if (e.KeyCode == Keys.Up)
                {
                    MoveFrameEnterLeave(1);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    MoveFrameEnterLeave(2);
                    e.Handled = true;
                }
            }
            else
            {
                if (e.KeyCode == Keys.F3)
                {
                    Search(true);
                }
                else if (e.KeyCode == Keys.F4)
                {
                    Search(false);
                } if (e.KeyCode == Keys.F5)
                {
                    Reopen();
                }
            }
        }

        private void LvRecords_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
			var records = GetRecords();
			var index = e.ItemIndex;
			if (records.Length <= index)
				return;

			var record = records[index];

			e.Item = new ListViewItem();
			e.Item.UseItemStyleForSubItems = false;
			e.Item.Text = (index + 1).ToString();
			e.Item.SubItems.Add(record.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
			e.Item.SubItems.Add(record.Ip);
			e.Item.SubItems.Add(record.Pid.ToString());
			e.Item.SubItems.Add(record.Tid.ToString());
			e.Item.SubItems.Add(record.Enter ? "Enter" : "Leave");
			e.Item.SubItems.Add(new string('　', record.Depth) + record.FrameName);

			for (int sel = _MarkedFrames.Count; sel != -1; sel--) {
				var frame = sel == _MarkedFrames.Count ? this.CurrentFrame : _MarkedFrames[sel];
				if (frame.Ip == record.Ip && frame.Pid == record.Pid && frame.Tid == record.Tid) {
					if (frame.StartIndex <= index && index <= frame.EndIndex) {
                        var n = e.Item.SubItems.Count;
                        if (index != frame.StartIndex && index != frame.EndIndex)
                            n--;
						for (int i = 0; i < n; i++)
							e.Item.SubItems[i].BackColor = frame.Color;
						break;
					}
				}
			}
		}

		private void LvInterrupts_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
			var records = GetInterrupts();
			var index = e.ItemIndex;
			if (records.Length <= index)
				return;

			var record = records[index];

			e.Item = new ListViewItem();
			e.Item.UseItemStyleForSubItems = false;
			e.Item.Text = record.Ip;
			e.Item.SubItems.Add(record.Pid.ToString());
			e.Item.SubItems.Add(record.Tid.ToString());
			e.Item.SubItems.Add(new string('　', record.Depth) + record.FrameName);
		}

        private void btnMarksToClipBoard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewToString(this.lvSelRanges));
        }

        private void btnInterruptsToClipBoard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewToString(this.lvInterrupts));
        }

        static string ListViewToString(ListView lv)
        {
            var sb = new StringBuilder();

            // ヘッダ作成
            bool first = true;
            foreach (ColumnHeader c in lv.Columns)
            {
                if(first)
                    first = false;
                else
                    sb.Append("\t");
                sb.Append(c.Text);
            }
            sb.AppendLine();

            // データ部作成
            var items = lv.Items;
            for (int i = 0, n = items.Count; i < n; i++)
            {
                var lvi = items[i];
                var sitems = lvi.SubItems;

                first = true;
                for (int j = 0, m = sitems.Count; j < m; j++)
                {
                    var si = sitems[j];
                    if (first)
                        first = false;
                    else
                        sb.Append("\t");
                    sb.Append(si.Text);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
	}
}
