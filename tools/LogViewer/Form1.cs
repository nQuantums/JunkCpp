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


		string _CsvFileName;
		Record[] _Records = new Record[0];
		Record[] _Interrupts = new Record[0];
		Frame _CurrentFrame;
		List<Frame> _MarkedFrames = new List<Frame>();
        AutoCompleteStringCollection _SearchedIps = new AutoCompleteStringCollection();
        AutoCompleteStringCollection _SearchedPids = new AutoCompleteStringCollection();
        AutoCompleteStringCollection _SearchedTids = new AutoCompleteStringCollection();
        AutoCompleteStringCollection _SearchedMethods = new AutoCompleteStringCollection();

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

            _SearchedIps.AddRange(Program.AppData.Ips.ToArray());
            _SearchedPids.AddRange(Program.AppData.Pids.ToArray());
            _SearchedTids.AddRange(Program.AppData.Tids.ToArray());
            _SearchedMethods.AddRange(Program.AppData.Methods.ToArray());

            this.tbIp.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbIp.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbIp.AutoCompleteCustomSource = _SearchedIps;

            this.tbPid.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbPid.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbPid.AutoCompleteCustomSource = _SearchedPids;

            this.tbTid.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbTid.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbTid.AutoCompleteCustomSource = _SearchedTids;

            this.tbMethod.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.tbMethod.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbMethod.AutoCompleteCustomSource = _SearchedMethods;


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

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Program.AppData.Ips = new List<string>(_SearchedIps.Cast<string>());
            Program.AppData.Pids = new List<string>(_SearchedPids.Cast<string>());
            Program.AppData.Tids = new List<string>(_SearchedTids.Cast<string>());
            Program.AppData.Methods = new List<string>(_SearchedMethods.Cast<string>());

            base.OnHandleDestroyed(e);
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
        /// <param name="frame">フレーム</param>
        public void AddMarkedFrame(Frame frame)
        {
            frame.Color = ColorTable[ColorIndex++ % ColorTable.Length];
            _MarkedFrames.Add(frame);
            UpdateRecordsListItem(frame.StartIndex, frame.EndIndex);

            var lvi = new ListViewItem();
            lvi.UseItemStyleForSubItems = false;
            lvi.Text = (frame.IsStartValid ? (frame.StartIndex + 1).ToString() : "") + "～" + (frame.IsEndValid ? (frame.EndIndex + 1).ToString() : "");
            lvi.SubItems.Add("");
            lvi.SubItems.Add(frame.Ip);
            lvi.SubItems.Add(frame.Pid.ToString());
            lvi.SubItems.Add(frame.Tid.ToString());
            lvi.SubItems.Add(GetRecords()[frame.StartIndex].FrameName);
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

        public List<Record> QueryInterrupts(Frame frame) {
			var result = new List<Record>();
			var records = GetRecords();
            var start = Math.Max(0, frame.StartIndex + 1);
            var end = Math.Min(records.Length, frame.EndIndex);
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

            if (!string.IsNullOrEmpty(ip) && !_SearchedIps.Contains(ip))
                _SearchedIps.Add(ip);
            if (pid != 0 && !_SearchedPids.Contains(pid.ToString()))
                _SearchedPids.Add(pid.ToString());
            if (tid != 0 && !_SearchedTids.Contains(tid.ToString()))
                _SearchedTids.Add(tid.ToString());
            if (!string.IsNullOrEmpty(method) && !_SearchedMethods.Contains(method))
                _SearchedMethods.Add(method);

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
            if (index < 0 || this.lvRecords.Items.Count <= index)
                return;

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
            if (index < 0)
                return;

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
            if (index < 0)
                return;

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
            if (index < 0)
                return;

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
            if (index < 0)
                return;

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
			var records = GetRecords();
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
	}
}
