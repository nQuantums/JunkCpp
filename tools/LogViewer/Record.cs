using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogViewer {
	public struct Record {
        public const int NoCached = int.MinValue;

		public ulong Address;

        public int Index;
		public DateTime DateTime;
		public string Ip;
		public UInt32 Pid;
		public UInt32 Tid;
		public int Depth;
		public bool Enter;
		public string FrameName;

        public int CachedPairIndex;
        public int CachedParentIndex;

		public override bool Equals(object obj) {
			if (obj is Record) {
				return this == (Record)obj;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			var hash = this.Ip != null ? this.Ip.GetHashCode() : 0;
			hash ^= this.Pid.GetHashCode();
			hash ^= this.Tid.GetHashCode();
			hash ^= this.Depth.GetHashCode();
			hash ^= this.Enter.GetHashCode();
			if (this.FrameName != null)
				hash ^= this.FrameName.GetHashCode();
			return hash;
		}

		public static bool operator ==(Record a, Record b) {
			if (a.Ip != b.Ip)
				return false;
			if (a.Pid != b.Pid)
				return false;
			if (a.Tid != b.Tid)
				return false;
			if (a.Depth != b.Depth)
				return false;
			if (a.Enter != b.Enter)
				return false;
			if (a.FrameName != b.FrameName)
				return false;
			return true;
		}

		public static bool operator !=(Record a, Record b) {
			return !(a == b);
		}

		public static bool TryParse(string line, out Record record) {
			record = new Record();
			var result = new Record();
            result.CachedPairIndex = NoCached;
            result.CachedParentIndex = NoCached;

			// 日時解析
			int start = 0;
			var end = line.IndexOf(',');
			if (end < 0)
				return false;

			var dateTimeString = line.Substring(start, end - start);
			if (!DateTime.TryParseExact(
				dateTimeString,
				"yyyy/MM/dd HH:mm:ss.fff",
				System.Globalization.DateTimeFormatInfo.InvariantInfo,
				System.Globalization.DateTimeStyles.None,
				out result.DateTime)) {
				return false;
			}

			// IP解析
			start = end + 1;
			end = line.IndexOf("Ip", start);
			if (end < 0)
				return false;
			start = end + 2;
			end = line.IndexOf(' ', start);
			if (end < 0)
				return false;
			result.Ip = line.Substring(start, end - start);

			// PID解析
			start = end + 1;
			end = line.IndexOf("Pid", start);
			if (end < 0)
				return false;
			start = end + 3;
			end = line.IndexOf(' ', start);
			if (end < 0)
				return false;
			if (!UInt32.TryParse(line.Substring(start, end - start), out result.Pid))
				return false;

			// TID解析
			start = end + 1;
			end = line.IndexOf("Tid", start);
			if (end < 0)
				return false;
			start = end + 3;
			end = line.IndexOf(',', start);
			if (end < 0)
				return false;
			if (!UInt32.TryParse(line.Substring(start, end - start), out result.Tid))
				return false;

			// 深度解析
			start = end + 1;
			end = line.IndexOf(',', start);
			if (end < 0)
				return false;
			if (!int.TryParse(line.Substring(start, end - start), out result.Depth))
				return false;

			// 開始終了判定解析
			start = end + 1;
			end = line.IndexOf(':', start);
			if (end < 0)
				return false;
			result.Enter = line[end - 1] == '+';

			// フレーム内容解析
			start = end + 2;
			result.FrameName = line.Substring(start, line.Length - start - 1);

			record = result;

			return true;
		}

		public static Record[] ReadFromCsv(string fileName) {
			var lines = new List<string>();

			using (var csv = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sr = new StreamReader(csv, Encoding.UTF8)) {
				while (!sr.EndOfStream) {
					lines.Add(sr.ReadLine());
				}
			}

			var records = new Record[lines.Count];
			for (int i = 0; i < records.Length; i++) {
				Record.TryParse(lines[i], out records[i]);
                records[i].Index = i;
			}
			return records;
		}
	}
}
