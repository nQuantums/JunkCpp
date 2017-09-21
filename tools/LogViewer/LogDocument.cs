using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    public class LogDocument : IDisposable
    {
        public string FileName = "";
        public string Title = "";
		public MemMapFile MemMapFile;
		public DynamicMemMapView Dmmv;
		public MemMapRecord[] Records = new MemMapRecord[0];
        public int StartRecordIndex = 0;
        public int EndRecordIndex = -1;

        public int RecordCount
        {
            get
            {
                return this.EndRecordIndex - this.StartRecordIndex + 1;
            }
        }

        public LogDocument()
        {
        }

        public LogDocument(string fileName, Tuple<MemMapFile, DynamicMemMapView, MemMapRecord[]> data, int startRecordIndex = Frame.NullStartIndex, int endRecordIndex = Frame.NullEndIndex)
        {
            this.FileName = fileName;
			this.MemMapFile = data.Item1;
			this.Dmmv = data.Item2;
			this.Records = data.Item3;

            if (startRecordIndex < 0)
                startRecordIndex = 0;
            if (this.Records.Length <= endRecordIndex)
                endRecordIndex = this.Records.Length - 1;

            this.StartRecordIndex = startRecordIndex;
            this.EndRecordIndex = endRecordIndex;

            this.Title = this.FileName;
            if (this.StartRecordIndex != 0 || this.EndRecordIndex != this.Records.Length - 1)
                this.Title += "(" + (this.StartRecordIndex + 1) + "～" + (this.EndRecordIndex + 1) + ") " + this.Records[this.StartRecordIndex].GetCore(this.Dmmv).FrameName;
        }

        public bool IsValidIndex(int index)
        {
            return this.StartRecordIndex <= index && index <= this.EndRecordIndex;
        }

        public bool IsValidIndex(Frame frame)
        {
            if (frame.StartRecordIndex == Frame.NullStartIndex)
                return IsValidIndex(frame.EndRecordIndex);
            if (frame.EndRecordIndex == Frame.NullEndIndex)
                return IsValidIndex(frame.StartRecordIndex);
            return IsValidIndex(frame.StartRecordIndex) && IsValidIndex(frame.EndRecordIndex);
        }

        public bool Adjust(ref int startIndex, ref int endIndex)
        {
            if (startIndex < this.StartRecordIndex)
                startIndex = this.StartRecordIndex;
            if (this.EndRecordIndex < endIndex)
                endIndex = this.EndRecordIndex;
            return startIndex <= endIndex;
        }

        public bool MapToListIndex(ref int startRecordIndex, ref int endRecordIndex)
        {
            if (startRecordIndex < this.StartRecordIndex)
                startRecordIndex = this.StartRecordIndex;
            if (this.EndRecordIndex < endRecordIndex)
                endRecordIndex = this.EndRecordIndex;
            if (endRecordIndex < startRecordIndex)
                return false;
            startRecordIndex -= this.StartRecordIndex;
            endRecordIndex -= this.StartRecordIndex;
            return true;
        }

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				}

				// アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// 大きなフィールドを null に設定します。
				if (this.MemMapFile != null) {
					this.MemMapFile.Dispose();
					this.MemMapFile = null;
				}
				if (this.Dmmv != null) {
					this.Dmmv.Dispose();
					this.Dmmv = null;
				}
				this.Records = null;

				disposedValue = true;
			}
		}

		~LogDocument() {
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(false);
		}

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose() {
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
