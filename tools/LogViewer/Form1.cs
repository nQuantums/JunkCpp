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
        List<int> _SameThreadRecordIndices = new List<int>();
        List<int> _OtherThreadRecordIndices = new List<int>();
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
                var sameThread = new List<int>();
                var otherThread = new List<int>();
				if (value.IsValid)
					QueryFrameInternalRecordIndices(_CurrentFrame, sameThread, otherThread);
				SetSameThreadRecordIndices(sameThread);
                SetOtherThreadRecordIndices(otherThread);
                // コールスタックを表示
                this.tbCallStack.Text = Frame.GetCallStackText(_LogDocument, _CurrentFrame.StartRecordIndex, _CurrentFrame.EndRecordIndex, null);
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

            this.tbMethod.KeyDown += tbMethod_KeyDown;


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

            this.lvSameThread.GetType().InvokeMember(
               "DoubleBuffered",
               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
               null,
               this.lvSameThread,
               new object[] { true });
            this.lvSameThread.VirtualMode = true;
            this.lvSameThread.RetrieveVirtualItem += lvSameThread_RetrieveVirtualItem;
            this.lvSameThread.DoubleClick += lvSameThread_DoubleClick;

            this.tbMethod.KeyDown += tbMethod_KeyDown;

			this.lvOtherThread.GetType().InvokeMember(
			   "DoubleBuffered",
			   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
			   null,
			   this.lvOtherThread,
			   new object[] { true });
			this.lvOtherThread.VirtualMode = true;
			this.lvOtherThread.RetrieveVirtualItem += LvOtherThread_RetrieveVirtualItem;
            this.lvOtherThread.DoubleClick += lvOtherThread_DoubleClick;
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

            var records = _LogDocument.Records;
            var index = frame.IsStartValid ? frame.StartRecordIndex : frame.EndRecordIndex;

            frame.Color = ColorTable[ColorIndex++ % ColorTable.Length];
            _MarkedFrames.Add(frame);
            UpdateRecordsListItem(frame.StartRecordIndex, frame.EndRecordIndex);

            var lvi = new ListViewItem();
            lvi.UseItemStyleForSubItems = false;
            lvi.Text = records[index].GetCore(_LogDocument.Dmmv).DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");
            lvi.SubItems.Add((frame.IsStartValid && frame.IsEndValid) ? (records[frame.EndRecordIndex].GetCore(_LogDocument.Dmmv).DateTime - records[frame.StartRecordIndex].GetCore(_LogDocument.Dmmv).DateTime).ToString(@"hh\:mm\:ss\.fff") : "?");
            lvi.SubItems.Add((frame.IsStartValid ? (frame.StartRecordIndex + 1).ToString() : "") + "～" + (frame.IsEndValid ? (frame.EndRecordIndex + 1).ToString() : ""));
            lvi.SubItems.Add("");
            lvi.SubItems.Add(frame.Ip);
            lvi.SubItems.Add(frame.Pid.ToString());
            lvi.SubItems.Add(frame.Tid.ToString());
            lvi.SubItems.Add(_LogDocument.Records[index].GetCore(_LogDocument.Dmmv).FrameName);
            lvi.SubItems[3].BackColor = frame.Color;
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

        void SetSameThreadRecordIndices(List<int> indices)
        {
            this.lvSameThread.BeginUpdate();
            try
            {
                _SameThreadRecordIndices = indices;
                this.lvSameThread.VirtualListSize = 0;
                this.lvSameThread.VirtualListSize = indices.Count;
            }
            finally
            {
                this.lvSameThread.EndUpdate();
            }
        }
        
        void SetOtherThreadRecordIndices(List<int> indices)
        {
			this.lvOtherThread.BeginUpdate();
			try {
				_OtherThreadRecordIndices = indices;
				this.lvOtherThread.VirtualListSize = 0;
                this.lvOtherThread.VirtualListSize = indices.Count;
			} finally {
				this.lvOtherThread.EndUpdate();
			}
		}

        public void QueryFrameInternalRecordIndices(Frame frame, List<int> sameThreadRecordIndicesRecords, List<int> otherThreadRecordIndicesRecords)
        {
			var dmmv = _LogDocument.Dmmv;
			var records = _LogDocument.Records;
            var start = Math.Max(_LogDocument.StartRecordIndex, frame.StartRecordIndex);
            var end = Math.Min(_LogDocument.EndRecordIndex, frame.EndRecordIndex);
			var args = new MemMapRecord.SearchArgs { Ip = frame.Ip, Pid = frame.Pid, Tid = frame.Tid };
			for (int i = start; i <= end; i++) {
				var r = records[i];
                if (r.IsMatched(dmmv, args))
                    sameThreadRecordIndicesRecords.Add(i);
                else
                    otherThreadRecordIndicesRecords.Add(i);
			}
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
        /// <param name="document">検索対象ログドキュメント</param>
        /// <param name="forward">進みながら検索するなら true</param>
        /// <param name="startIndex">検索開始レコードインデックス、<see cref="int.MinValue"/>なら無指定とみなす</param>
        /// <param name="args">検索設定</param>
        /// <param name="countMax">検索最大レコード数</param>
        /// <returns>該当するレコードインデックスの配列</returns>
        List<int> SearchRecordIndices(LogDocument document, bool forward, int startIndex, MemMapRecord.SearchArgs args, int countMax)
        {
            var result = new List<int>();
			var dmmv = document.Dmmv;
            var records = document.Records;

            if (forward)
            {
                var start = startIndex == int.MinValue ? document.StartRecordIndex : startIndex;
                var end = document.EndRecordIndex;
                for (int i = start; i <= end; i++)
                {
					if(records[i].IsMatched(dmmv, args)) {
						result.Add(i);
						if (result.Count == countMax)
							break;
					}
                }
            }
            else
            {
                var start = startIndex == int.MinValue ? document.StartRecordIndex : startIndex;
                var end = document.StartRecordIndex;
                for (int i = start; end <= i; i--)
                {
					if (records[i].IsMatched(dmmv, args)) {
						result.Add(i);
						if (result.Count == countMax)
							break;
					}
                }
            }

            return result;
        }

        /// <summary>
        /// 現在のコントロールに設定されている検索条件から検索用引数を取得する
        /// </summary>
        MemMapRecord.SearchArgs GetSearchArgs()
        {
            var args = new MemMapRecord.SearchArgs();

            args.Ip = this.tbIp.Text;
            if (string.IsNullOrEmpty(args.Ip))
                args.Ip = null;
            args.Method = this.tbMethod.Text;
            if (string.IsNullOrEmpty(args.Method))
                args.Method = null;
            UInt32.TryParse(this.tbPid.Text, out args.Pid);
            UInt32.TryParse(this.tbTid.Text, out args.Tid);

            if (!string.IsNullOrEmpty(args.Ip) && !Program.SearchedIps.Contains(args.Ip))
                Program.SearchedIps.Add(args.Ip);
            if (args.Pid != 0 && !Program.SearchedPids.Contains(args.Pid.ToString()))
                Program.SearchedPids.Add(args.Pid.ToString());
            if (args.Tid != 0 && !Program.SearchedTids.Contains(args.Tid.ToString()))
                Program.SearchedTids.Add(args.Tid.ToString());
            if (!string.IsNullOrEmpty(args.Method) && !Program.SearchedMethods.Contains(args.Method))
                Program.SearchedMethods.Add(args.Method);

            return args;
        }

        /// <summary>
        /// レコード検索
        /// </summary>
        /// <param name="forward">進みながら検索するなら true</param>
        /// <param name="args">検索設定</param>
        void Search(bool forward, MemMapRecord.SearchArgs args)
        {
			var startIndex = GetSelectedRecordIndex();

            if (forward)
                startIndex++;
            else
                startIndex--;

            var result = SearchRecordIndices(_LogDocument, forward, startIndex, args, 1);
            if (result.Count != 0)
            {
                MoveToRecord(result[0]);
            }
		}

        /// <summary>
        /// 現在の検索設定でレコード検索
        /// </summary>
        /// <param name="forward">進みながら検索するなら true</param>
        void Search(bool forward)
        {
            Search(forward, GetSearchArgs());
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

        void MoveToMarkedFrame(int index)
        {
            if (index < 0 || _MarkedFrames.Count <= index)
                return;
            var range = _MarkedFrames[index];
            MoveToRecord(range.StartRecordIndex);
            this.lvRecords.Focus();
        }

        void Reopen()
        {
			SetDocument(new LogDocument(_LogDocument.FileName, MemMapRecord.ReadFromBinLog(_LogDocument.FileName)));
        }

		private void tsmiOpen_Click(object sender, EventArgs e) {
			//OpenFileDialogクラスのインスタンスを作成
			using (var ofd = new OpenFileDialog()) {
				ofd.Filter = "バイナリログファイル(*.binlog)|*.binlog|すべてのファイル(*.*)|*.*";
				ofd.FilterIndex = 0;
				ofd.Title = "開くログファイルを選択してください";
				ofd.CheckFileExists = true;
				ofd.CheckPathExists = true;
				if (ofd.ShowDialog() != DialogResult.OK)
					return;

				SetDocument(new LogDocument(ofd.FileName, MemMapRecord.ReadFromBinLog(ofd.FileName)));
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


        private void btnSelClear_Click(object sender, EventArgs e)
        {
            _MarkedFrames.Clear();
            this.lvSelRanges.Items.Clear();
        }

		private void lvSelRanges_DoubleClick(object sender, EventArgs e) {
			var si = this.lvSelRanges.SelectedIndices;
			if (si.Count == 0)
				return;
            MoveToMarkedFrame(si[0]);
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
			var core = record.GetCore(_LogDocument.Dmmv);

			e.Item = new ListViewItem();
			e.Item.UseItemStyleForSubItems = false;
			e.Item.Text = (record.Index + 1).ToString();
			e.Item.SubItems.Add(core.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
			e.Item.SubItems.Add(core.Ip);
			e.Item.SubItems.Add(core.Pid.ToString());
			e.Item.SubItems.Add(core.Tid.ToString());
			e.Item.SubItems.Add(core.LogType == MemMapRecord.LogType.Enter ? "Enter" : "Leave");
			e.Item.SubItems.Add(new string('　', core.Depth) + core.FrameName);

			for (int sel = _MarkedFrames.Count; sel != -1; sel--) {
				var frame = sel == _MarkedFrames.Count ? this.CurrentFrame : _MarkedFrames[sel];
                if (!frame.IsValid)
                    continue;

                if (frame.Ip == core.Ip && frame.Pid == core.Pid && frame.Tid == core.Tid) {
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

        void lvSameThread_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            var recordIndices = _SameThreadRecordIndices;
            var index = e.ItemIndex;
            if (recordIndices.Count <= index)
                return;

            var recordIndex = recordIndices[index];
            var record = _LogDocument.Records[recordIndex];
			var core = record.GetCore(_LogDocument.Dmmv);

            e.Item = new ListViewItem();
            e.Item.UseItemStyleForSubItems = false;
            e.Item.Text = (record.Index + 1).ToString();
            e.Item.SubItems.Add(core.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            e.Item.SubItems.Add(core.Ip);
            e.Item.SubItems.Add(core.Pid.ToString());
            e.Item.SubItems.Add(core.Tid.ToString());
            e.Item.SubItems.Add(new string('　', core.Depth) + core.FrameName);
        }

		private void LvOtherThread_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            var recordIndices = _OtherThreadRecordIndices;
            var index = e.ItemIndex;
            if (recordIndices.Count <= index)
                return;

            var recordIndex = recordIndices[index];
            var record = _LogDocument.Records[recordIndex];
			var core = record.GetCore(_LogDocument.Dmmv);

            e.Item = new ListViewItem();
            e.Item.UseItemStyleForSubItems = false;
            e.Item.Text = (record.Index + 1).ToString();
            e.Item.SubItems.Add(core.DateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            e.Item.SubItems.Add(core.Ip);
            e.Item.SubItems.Add(core.Pid.ToString());
            e.Item.SubItems.Add(core.Tid.ToString());
            e.Item.SubItems.Add(new string('　', core.Depth) + core.FrameName);
        }

        private void btnMarksToClipBoard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewToString(this.lvSelRanges));
        }

        private void btnOtherThreadToClipBoard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewToString(this.lvSameThread));
        }

        private void btnSameThreadToClipBoard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ListViewToString(this.lvOtherThread));
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
				new Tuple<MemMapFile, DynamicMemMapView, MemMapRecord[]>(_LogDocument.MemMapFile, _LogDocument.Dmmv, _LogDocument.Records),
                frame.StartRecordIndex,
                frame.EndRecordIndex);

            var form = new Form1(document);
            form.FormClosed += (obj, ev) =>
            {
                form.Dispose();
            };
            form.Show();
        }

        private void btnSearchSelAdd_Click(object sender, EventArgs e)
        {
            var indices = SearchRecordIndices(_LogDocument, true, int.MinValue, GetSearchArgs(), int.MaxValue);
            foreach (var index in indices)
            {
                AddMarkedFrame(Frame.Search(_LogDocument, index, null));
            }
        }

        private void lvSelRanges_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var si = this.lvSelRanges.SelectedIndices;
                if (si.Count == 0)
                    return;
                MoveToMarkedFrame(si[0]);
            }
        }

        void lvSameThread_DoubleClick(object sender, EventArgs e) {
            var si = this.lvSameThread.SelectedIndices;
            if(si.Count == 0)
                return;
            var index = si[0];
            if(index < 0)
                return;
            MoveToRecord(_SameThreadRecordIndices[index]);
            this.lvRecords.Focus();
        }

        void lvOtherThread_DoubleClick(object sender, EventArgs e) {
            var si = this.lvOtherThread.SelectedIndices;
            if (si.Count == 0)
                return;
            var index = si[0];
            if (index < 0)
                return;
            MoveToRecord(_OtherThreadRecordIndices[index]);
            this.lvRecords.Focus();
        }
    }
}
