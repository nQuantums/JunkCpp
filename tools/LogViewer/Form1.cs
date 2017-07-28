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


        string _BaseTitle;

        LogDocument _LogDocument;
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
                if (_CurrentFrame.IsValid)
                {
                    start = _CurrentFrame.StartRecordIndex;
                    end = _CurrentFrame.EndRecordIndex;
                }
                if (value.IsValid)
                {
                    start = start != int.MinValue ? Math.Min(start, value.StartRecordIndex) : value.StartRecordIndex;
                    end = end != int.MinValue ? Math.Min(end, value.EndRecordIndex) : value.EndRecordIndex;
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
                this.tbCallStack.Text = Frame.GetCallStackText(_LogDocument, _CurrentFrame, null);
			}
		}


		public Form1() {
			InitializeComponent();

            _BaseTitle = this.Text;

            this.tbIp.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbIp.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbIp.AutoCompleteCustomSource = Program.SearchedIps;

            this.tbPid.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbPid.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbPid.AutoCompleteCustomSource = Program.SearchedPids;

            this.tbTid.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbTid.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbTid.AutoCompleteCustomSource = Program.SearchedTids;

            this.tbMethod.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbMethod.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbMethod.AutoCompleteCustomSource = Program.SearchedMethods;


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

            this.tbMethod.KeyDown += tbMethod_KeyDown;
		}

        public Form1(LogDocument document)
            : this()
        {
            SetDocument(document);
        }

        void SetDocument(LogDocument document)
        {
            this.lvRecords.BeginUpdate();
            try
            {
                _LogDocument = document;
                this.Text = _LogDocument.Title + " - " + _BaseTitle;

                this.lvRecords.VirtualListSize = 0;
                this.lvRecords.VirtualListSize = _LogDocument.RecordCount;
            }
            finally
            {
                this.lvRecords.EndUpdate();
            }
        }

        /// <summary>
        /// マークするフレームの追加
        /// </summary>
        /// <param name="frame">フレーム</param>
        public void AddMarkedFrame(Frame frame)
        {
            if (!_LogDocument.IsValidIndex(frame))
                return;

            frame.Color = ColorTable[ColorIndex++ % ColorTable.Length];
            _MarkedFrames.Add(frame);
            UpdateRecordsListItem(frame.StartRecordIndex, frame.EndRecordIndex);

            var lvi = new ListViewItem();
            lvi.UseItemStyleForSubItems = false;
            lvi.Text = (frame.IsStartValid ? (frame.StartRecordIndex + 1).ToString() : "") + "～" + (frame.IsEndValid ? (frame.EndRecordIndex + 1).ToString() : "");
            lvi.SubItems.Add("");
            lvi.SubItems.Add(frame.Ip);
            lvi.SubItems.Add(frame.Pid.ToString());
            lvi.SubItems.Add(frame.Tid.ToString());
            lvi.SubItems.Add(_LogDocument.Records[frame.StartRecordIndex].FrameName);
            lvi.SubItems[1].BackColor = frame.Color;
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
            UpdateRecordsListItem(range.StartRecordIndex, range.EndRecordIndex);

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

        public List<Record> QueryInterrupts(Frame frame) {
			var result = new List<Record>();
			var records = _LogDocument.Records;
            var start = Math.Max(_LogDocument.StartRecordIndex, frame.StartRecordIndex + 1);
            var end = Math.Min(_LogDocument.EndRecordIndex, frame.EndRecordIndex);
			for (int i = start; i < end; i++) {
				var r = records[i];
				// 本流の処理は無視
				if (r.Ip == frame.Ip && r.Pid == frame.Pid && r.Tid == frame.Tid)
					continue;

				// 他スレッドの処理を解析
				result.Add(r);
			}
			return result;
		}

        public Frame SearchCurrentFrame()
        {
            return Frame.Search(_LogDocument, GetSelectedRecordIndex(), null);
        }

		public Frame SearchParentFrame() {
            return Frame.SearchParentFrame(_LogDocument, this.CurrentFrame, null);
		}

        public Frame SearchChildFrame()
        {
            return Frame.SearchChildFrame(_LogDocument, this.CurrentFrame, null);
        }

		public int SearchNextIndex() {
            return Frame.SearchNextIndex(_LogDocument, this.CurrentFrame, null);
        }

		public int SearchPrevIndex() {
            return Frame.SearchPrevIndex(_LogDocument, this.CurrentFrame, null);
        }

        /// <summary>
        /// レコード表示を更新する
        /// </summary>
        /// <param name="start">更新開始レコードインデックス</param>
        /// <param name="end">更新終了レコードインデックス</param>
		void UpdateRecordsListItem(int start = Frame.NullStartIndex, int end = Frame.NullEndIndex) {
            if (!_LogDocument.MapToListIndex(ref start, ref end))
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
			var startIndex = GetSelectedRecordIndex();
			var records = _LogDocument.Records;

			if (forward) {
                var start = Math.Max(startIndex + 1, _LogDocument.StartRecordIndex);
                var end = _LogDocument.EndRecordIndex;
				for (int i = start; i <= end; i++) {
					var r = records[i];
					if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid) && (method == null || 0 <= r.FrameName.IndexOf(method, StringComparison.CurrentCultureIgnoreCase))) {
                        MoveToRecord(i);
						break;
					}
				}
			} else {
                var start = Math.Min(startIndex - 1, _LogDocument.StartRecordIndex);
                var end = _LogDocument.StartRecordIndex;
                for (int i = start; end <= i; i--)
                {
					var r = records[i];
                    if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid) && (method == null || 0 <= r.FrameName.IndexOf(method, StringComparison.CurrentCultureIgnoreCase))) {
                        MoveToRecord(i);
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

            if (!string.IsNullOrEmpty(ip) && !Program.SearchedIps.Contains(ip))
                Program.SearchedIps.Add(ip);
            if (pid != 0 && !Program.SearchedPids.Contains(pid.ToString()))
                Program.SearchedPids.Add(pid.ToString());
            if (tid != 0 && !Program.SearchedTids.Contains(tid.ToString()))
                Program.SearchedTids.Add(tid.ToString());
            if (!string.IsNullOrEmpty(method) && !Program.SearchedMethods.Contains(method))
                Program.SearchedMethods.Add(method);

            Search(forward, ip, pid, tid, method);
        }

        int GetSelectedRecordIndex()
        {
            var si = this.lvRecords.SelectedIndices;
            if (si.Count == 0)
                return -1;
            return si[0] + _LogDocument.StartRecordIndex;
        }

        void MoveToRecord(int recordIndex) {
            var listIndex = recordIndex - _LogDocument.StartRecordIndex;
            if (listIndex < 0 || this.lvRecords.VirtualListSize <= listIndex)
                return;

            var si = this.lvRecords.SelectedIndices;
            si.Clear();
            si.Add(listIndex);
            this.lvRecords.EnsureVisible(listIndex);
        }

        void MoveFrameEnterLeave(int mode)
        {
            var frame = this.CurrentFrame;
            if (!frame.IsValid)
                return;

            int recordIndex;

            switch (mode)
            {
                case 0:
                default:
                    recordIndex = GetSelectedRecordIndex() == frame.StartRecordIndex ? frame.EndRecordIndex : frame.StartRecordIndex;
                    break;
                case 1:
                    recordIndex = frame.StartRecordIndex;
                    break;
                case 2:
                    recordIndex = frame.EndRecordIndex;
                    break;
            }

            MoveToRecord(recordIndex);
        }

        void MoveFrameParent()
        {
            MoveToRecord(SearchParentFrame().StartRecordIndex);
        }

        void MoveFrameChild()
        {
            MoveToRecord(SearchChildFrame().StartRecordIndex);
        }

        void MoveFrameNext()
        {
            MoveToRecord(SearchNextIndex());
        }

        void MoveFramePrev()
        {
            MoveToRecord(SearchPrevIndex());
        }

        void Reopen()
        {
            SetDocument(new LogDocument(_LogDocument.FileName, Record.ReadFromCsv(_LogDocument.FileName)));
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

                SetDocument(new LogDocument(ofd.FileName, Record.ReadFromCsv(ofd.FileName)));
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
            MoveToRecord(range.StartRecordIndex);
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
                else if (e.KeyCode == Keys.F)
                {
                    this.tbMethod.Focus();
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
            var recordIndex = e.ItemIndex + _LogDocument.StartRecordIndex;
            if (!_LogDocument.IsValidIndex(recordIndex))
                return;

            var record = _LogDocument.Records[recordIndex];

			e.Item = new ListViewItem();
			e.Item.UseItemStyleForSubItems = false;
			e.Item.Text = (record.Index + 1).ToString();
			e.Item.SubItems.Add(record.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
			e.Item.SubItems.Add(record.Ip);
			e.Item.SubItems.Add(record.Pid.ToString());
			e.Item.SubItems.Add(record.Tid.ToString());
			e.Item.SubItems.Add(record.Enter ? "Enter" : "Leave");
			e.Item.SubItems.Add(new string('　', record.Depth) + record.FrameName);

			for (int sel = _MarkedFrames.Count; sel != -1; sel--) {
				var frame = sel == _MarkedFrames.Count ? this.CurrentFrame : _MarkedFrames[sel];
                if (!frame.IsValid)
                    continue;

                if (frame.Ip == record.Ip && frame.Pid == record.Pid && frame.Tid == record.Tid) {
                    if (frame.StartRecordIndex <= recordIndex && recordIndex <= frame.EndRecordIndex)
                    {
                        var n = e.Item.SubItems.Count;
                        if (recordIndex != frame.StartRecordIndex && recordIndex != frame.EndRecordIndex)
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
            e.Item.Text = (record.Index + 1).ToString();
            e.Item.SubItems.Add(record.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            e.Item.SubItems.Add(record.Ip);
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

        void tbMethod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search(true);
                this.lvRecords.Focus();
            }
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

        private void 現在のフレーム内のみ表示VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frame = this.CurrentFrame;
            if(!frame.IsValid)
                return;

            var document = new LogDocument(
                _LogDocument.FileName,
                _LogDocument.Records,
                frame.StartRecordIndex,
                frame.EndRecordIndex);

            var form = new Form1(document);
            form.FormClosed += (obj, ev) =>
            {
                form.Dispose();
            };
            form.Show();
        }
	}
}
