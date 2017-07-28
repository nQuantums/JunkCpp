using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LogViewer {
	static class Program {

        public static string AppDataPath
        {
            get;
            private set;
        }

        public static ApplicationData AppData
        {
            get;
            private set;
        }

        public static readonly AutoCompleteStringCollection SearchedIps = new AutoCompleteStringCollection();
        public static readonly AutoCompleteStringCollection SearchedPids = new AutoCompleteStringCollection();
        public static readonly AutoCompleteStringCollection SearchedTids = new AutoCompleteStringCollection();
        public static readonly AutoCompleteStringCollection SearchedMethods = new AutoCompleteStringCollection();

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main() {
            AppDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "appdata.json");
            AppData = ApplicationData.ReadFromJson(AppDataPath);


            SearchedIps.AddRange(Program.AppData.Ips.ToArray());
            SearchedPids.AddRange(Program.AppData.Pids.ToArray());
            SearchedTids.AddRange(Program.AppData.Tids.ToArray());
            SearchedMethods.AddRange(Program.AppData.Methods.ToArray());

            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

            AppData.Ips = new List<string>(SearchedIps.Cast<string>());
            AppData.Pids = new List<string>(SearchedPids.Cast<string>());
            AppData.Tids = new List<string>(SearchedTids.Cast<string>());
            AppData.Methods = new List<string>(SearchedMethods.Cast<string>());

            ApplicationData.WriteToJson(AppDataPath, AppData);
		}
	}
}
