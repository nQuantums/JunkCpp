using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    public class LogDocument
    {
        public string FileName = "";
        public string Title = "";
        public Record[] Records = new Record[0];
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

        public LogDocument(string fileName, Record[] records, int startRecordIndex = Frame.NullStartIndex, int endRecordIndex = Frame.NullEndIndex)
        {
            this.FileName = fileName;
            this.Records = records;

            if (startRecordIndex < 0)
                startRecordIndex = 0;
            if (this.Records.Length <= endRecordIndex)
                endRecordIndex = this.Records.Length - 1;

            this.StartRecordIndex = startRecordIndex;
            this.EndRecordIndex = endRecordIndex;

            this.Title = this.FileName;
            if (this.StartRecordIndex != 0 || this.EndRecordIndex != this.Records.Length - 1)
                this.Title += "(" + (this.StartRecordIndex + 1) + "～" + (this.EndRecordIndex + 1) + ") " + this.Records[this.StartRecordIndex].FrameName;
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
    }
}
