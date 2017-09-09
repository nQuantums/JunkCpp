using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer {
	public struct MemMapRecord {
		public const int NoCached = int.MinValue;
		public const int NoParent = int.MaxValue;

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
				address += 4;

				var end = (long)address + logSize;

				this.DateTime = UnixTimeStampToDateTime(dmmv.ReadUInt64(address));
				address += 8;

				var remoteNameSize = dmmv.ReadInt32(address);
				if (remoteNameSize < 0 || end < (long)address + remoteNameSize)
					throw new MemMapFileException(string.Concat("アドレス ", address.ToString("X16"), " のIPアドレス名サイズが不正(", remoteNameSize, ")です。"), dmmv.Source.FileName);
				address += 4;

				this.Ip = dmmv.ReadStringAnsi(address, remoteNameSize);
				address += (ulong)remoteNameSize;

				this.Pid = dmmv.ReadUInt32(address);
				address += 4;

				this.Tid = dmmv.ReadUInt32(address);
				address += 4;

				this.Depth = dmmv.ReadInt16(address);
				address += 2;

				this.LogType = (LogType)dmmv.ReadInt16(address);
				address += 2;

				this.FrameName = Encoding.UTF8.GetString(dmmv.ReadBytes(address, (int)(end - (long)address)));
			}

			public static LogType GetLogType(DynamicMemMapView dmmv, ulong address) {
				var logSize = dmmv.ReadInt32(address);
				address += 4;
				address += 8;
				var remoteNameSize = dmmv.ReadInt32(address);
				address += 4;
				address += (ulong)remoteNameSize;
				address += 4;
				address += 4;
				address += 2;
				return (LogType)dmmv.ReadInt16(address);
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

		public LogType GetLogType(DynamicMemMapView dmmv) {
			return Core.GetLogType(dmmv, this.Address);
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
