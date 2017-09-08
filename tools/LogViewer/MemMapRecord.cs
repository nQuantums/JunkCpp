using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer {
	public struct MemMapRecord {
		public const int NoCached = int.MinValue;

		public enum LogType {
			Enter = 1,
			Leave = 2,
		}

		public struct Core {
			public DateTime DateTime;
			public string Ip;
			public UInt32 Pid;
			public UInt32 Tid;
			public int Depth;
			public LogType LogType;
			public string FrameName;

			public bool Enter => this.LogType == LogType.Enter;

			public Core(DynamicMemMapView dmmv, ulong address) {
				var logSize = dmmv.ReadInt32(address);
				var bytes = dmmv.ReadBytes(address + 4, logSize);

				int offset = 0;
				this.DateTime = UnixTimeStampToDateTime(BitConverter.ToUInt64(bytes, offset));
				offset += 8;

				var remoteNameSize = BitConverter.ToInt32(bytes, offset);
				if (remoteNameSize < 0 || bytes.Length < offset + remoteNameSize)
					throw new MemMapFileException(string.Concat("アドレス ", address.ToString("X16"), " のIPアドレス名サイズが不正(", remoteNameSize, ")です。"), dmmv.Source.FileName);
				offset += 4;

				this.Ip = Encoding.UTF8.GetString(bytes, offset, remoteNameSize);
				offset += remoteNameSize;

				this.Pid = BitConverter.ToUInt32(bytes, offset);
				offset += 4;

				this.Tid = BitConverter.ToUInt32(bytes, offset);
				offset += 4;

				this.Depth = BitConverter.ToInt16(bytes, offset);
				offset += 2;

				this.LogType = (LogType)BitConverter.ToInt16(bytes, offset);
				offset += 2;

				this.FrameName = Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
			}
		}

		public ulong Address;
		public int Index;
		public int CachedPairIndex;
		public int CachedParentIndex;

		public MemMapRecord(DynamicMemMapView dmmv, ref ulong address, ref int index) {
			var logSize = dmmv.ReadInt32(address);
			if (logSize < 0 || logSize < 24)
				throw new MemMapFileException(string.Concat("アドレス ", address.ToString("X16"), " のログサイズが不正(", logSize, ")です。"), dmmv.Source.FileName);

			this.Address = address;
			this.Index = index;
			this.CachedPairIndex = NoCached;
			this.CachedParentIndex = NoCached;
			address += (ulong)logSize + 4;
			index++;
		}

		public Core GetCore(DynamicMemMapView dmmv) {
			return new Core(dmmv, this.Address);
		}

		static DateTime UnixTimeStampToDateTime(ulong utcUnixTimeMs) {
			var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			return dtDateTime.AddMilliseconds(utcUnixTimeMs).ToLocalTime();
		}

		public static Tuple<MemMapFile, DynamicMemMapView, MemMapRecord[]> ReadFromBinLog(string fileName) {
			var mmf = new MemMapFile(fileName, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
			var dmmv = new DynamicMemMapView(mmf);
			return new Tuple<MemMapFile, DynamicMemMapView, MemMapRecord[]>(mmf, dmmv, ReadFromBinLog(dmmv));
		}

		public static MemMapRecord[] ReadFromBinLog(DynamicMemMapView dmmv) {
			ulong address = 0;
			int index = 0;
			var records = new List<MemMapRecord>();
			try {
				for (; ; ) {
					records.Add(new MemMapRecord(dmmv, ref address, ref index));
				}
			} catch (MemMapFileAccessException) {

			}
			return records.ToArray();
		}
	}
}
