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

		public struct RecordRange {
			public int StartIndex;
			public int EndIndex;
			public string Ip;
			public UInt32 Pid;
			public UInt32 Tid;
			public int Depth;
			public Color Color;

			public override bool Equals(object obj) {
				if (obj is RecordRange) {
					return this == (RecordRange)obj;
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

			public static bool operator ==(RecordRange a, RecordRange b) {
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

			public static bool operator !=(RecordRange a, RecordRange b) {
				return !(a == b);
			}
		}

		string _CsvFileName;
		Record[] _Records = new Record[0];
		RecordRange _Range;
		List<RecordRange> _Ranges = new List<RecordRange>();

		public RecordRange Range {
			get => _Range;
			set {
				if (_Range == value)
					return;

				int start = Math.Min(value.StartIndex, _Range.StartIndex);
				int end = Math.Max(value.EndIndex, _Range.EndIndex);
				_Range = value;
				_Range.Color = Color.FromArgb(192, 200, 255);

				AfterFilterChange(start, end);
			}
		}

		public void AddRange(RecordRange range) {
			range.Color = ColorTable[ColorIndex++ % ColorTable.Length];
			_Ranges.Add(range);
			AfterFilterChange(range.StartIndex, range.EndIndex);

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

		public void RemoveRangeAt(int index) {
			if (index < 0 || _Ranges.Count <= index)
				return;

			var range = _Ranges[index];
			_Ranges.RemoveAt(index);
			AfterFilterChange(range.StartIndex, range.EndIndex);

			this.lvSelRanges.Items.RemoveAt(index);
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
		}

		private void LvRecords_SelectedIndexChanged(object sender, EventArgs e) {
			this.Range = GetDepthRange();
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
			e.Item.SubItems.Add(record.Open ? "Enter" : "Leave");
			e.Item.SubItems.Add(new string('　', record.Depth) + record.FrameName);

			for (int sel = _Ranges.Count; sel != -1; sel--) {
				var range = sel == _Ranges.Count ? this.Range : _Ranges[sel];
				if (range.Ip == record.Ip && range.Pid == record.Pid && range.Tid == record.Tid) {
					if (range.StartIndex <= index && index <= range.EndIndex) {
						for (int i = 0; i < e.Item.SubItems.Count; i++)
							e.Item.SubItems[i].BackColor = range.Color;
						break;
					}
				}
			}
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
			SetRecords(Record.ReadFromCsv(_CsvFileName));
		}

		public Record[] GetRecords() {
			return _Records;
		}

		public RecordRange GetDepthRange() {
			var si = this.lvRecords.SelectedIndices;
			if (si.Count == 0)
				return new RecordRange();

			var index = si[0];
			var records = GetRecords();
			var curRecord = records[index];
			var range = new RecordRange();
			range.StartIndex = index;
			range.EndIndex = index;
			range.Ip = curRecord.Ip;
			range.Pid = curRecord.Pid;
			range.Tid = curRecord.Tid;
			range.Depth = curRecord.Depth;

			if (curRecord.Open) {
				for (int i = index + 1; i < records.Length; i++) {
					var r = records[i];
					if (r.Ip != range.Ip)
						continue;
					if (r.Pid != range.Pid)
						continue;
					if (r.Tid != range.Tid)
						continue;
					if (r.Depth < range.Depth)
						break;
					range.EndIndex = i;
					if (r.Depth == range.Depth && !r.Open)
						break;
				}
			} else {
				for (int i = index - 1; i != -1; i--) {
					var r = records[i];
					if (r.Ip != range.Ip)
						continue;
					if (r.Pid != range.Pid)
						continue;
					if (r.Tid != range.Tid)
						continue;
					if (r.Depth < range.Depth)
						break;
					range.StartIndex = i;
					if (r.Depth == range.Depth && r.Open)
						break;
				}
			}

			return range;
		}

		void AfterFilterChange(int start = -1, int end = -1) {
			if (start < 0)
				start = 0;
			if (end < 0 || this.lvRecords.VirtualListSize <= end)
				end = this.lvRecords.VirtualListSize - 1;
			if (end < start)
				return;
			this.lvRecords.RedrawItems(start, end, true);
		}

		void Search(bool forward, string ip, UInt32 pid, UInt32 tid) {
			var si = this.lvRecords.SelectedIndices;
			var startIndex = si.Count != 0 ? si[0] : -1;
			var records = GetRecords();

			if (forward) {
				for (int i = startIndex + 1; i < records.Length; i++) {
					var r = records[i];
					if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid)) {
						si.Clear();
						si.Add(i);
						this.lvRecords.EnsureVisible(i);
						break;
					}
				}
			} else {
				for (int i = startIndex - 1; 0 <= i; i--) {
					var r = records[i];
					if ((ip == null || r.Ip == ip) && (pid == 0 || r.Pid == pid) && (tid == 0 || r.Tid == tid)) {
						si.Clear();
						si.Add(i);
						this.lvRecords.EnsureVisible(i);
						break;
					}
				}
			}
		}

		private void btnSearch_Click(object sender, EventArgs e) {
			string ip;
			UInt32 pid;
			UInt32 tid;

			ip = this.tbIp.Text;
			if (string.IsNullOrEmpty(ip))
				ip = null;
			UInt32.TryParse(this.tbPid.Text, out pid);
			UInt32.TryParse(this.tbTid.Text, out tid);

			Search(sender == this.btnSearchForward, ip, pid, tid);
		}

		private void btnSelAdd_Click(object sender, EventArgs e) {
			AddRange(this.Range);
		}

		private void btnSelDel_Click(object sender, EventArgs e) {
			var si = this.lvSelRanges.SelectedIndices;
			if (si.Count == 0)
				return;
			RemoveRangeAt(si[0]);
		}

		private void lvSelRanges_DoubleClick(object sender, EventArgs e) {
			var si = this.lvSelRanges.SelectedIndices;
			if (si.Count == 0)
				return;
			var index = si[0];
			if (index < 0 || _Ranges.Count <= index)
				return;

			var range = _Ranges[index];
			this.lvRecords.EnsureVisible(range.StartIndex);
		}

		private void btnJunpEnterLeave_Click(object sender, EventArgs e) {
			var si = this.lvRecords.SelectedIndices;
			if (si.Count == 0)
				return;

			var range = this.Range;
			var index = si[0] == range.StartIndex ? range.EndIndex : range.StartIndex;

			si.Clear();
			si.Add(index);
			this.lvRecords.EnsureVisible(index);
		}
	}
}
