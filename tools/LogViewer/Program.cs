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

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main() {
            AppDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "appdata.json");
            AppData = ApplicationData.ReadFromJson(AppDataPath);

            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());

            ApplicationData.WriteToJson(AppDataPath, AppData);
		}
	}
}
