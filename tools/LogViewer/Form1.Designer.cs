namespace LogViewer {
	partial class Form1 {
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiReopen = new System.Windows.Forms.ToolStripMenuItem();
			this.現在のフレーム内のみ表示VToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.btnOtherThreadToClipBoard = new System.Windows.Forms.Button();
			this.lvOtherThread = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.btnSameThreadToClipBoard = new System.Windows.Forms.Button();
			this.lvSameThread = new System.Windows.Forms.ListView();
			this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.tbCallStack = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnJumpPrev = new System.Windows.Forms.Button();
			this.btnJumpNext = new System.Windows.Forms.Button();
			this.btnJumpDest = new System.Windows.Forms.Button();
			this.btnJumpSource = new System.Windows.Forms.Button();
			this.btnJunpEnterLeave = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnSelClear = new System.Windows.Forms.Button();
			this.btnMarksToClipBoard = new System.Windows.Forms.Button();
			this.btnSelDel = new System.Windows.Forms.Button();
			this.btnSelAdd = new System.Windows.Forms.Button();
			this.lvSelRanges = new System.Windows.Forms.ListView();
			this.colSelDateTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelTimeSpan = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelRange = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelColor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelIp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelPid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelTid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSelMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnSearchSelAdd = new System.Windows.Forms.Button();
			this.tbMethod = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbTid = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbPid = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnSearchBackward = new System.Windows.Forms.Button();
			this.btnSearchForward = new System.Windows.Forms.Button();
			this.tbIp = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.lvRecords = new System.Windows.Forms.ListView();
			this.colIndex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colDateTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colTid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colEnterLeave = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.menuStrip1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.現在のフレーム内のみ表示VToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1295, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// tsmiFile
			// 
			this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiOpen,
            this.tsmiReopen});
			this.tsmiFile.Name = "tsmiFile";
			this.tsmiFile.Size = new System.Drawing.Size(67, 20);
			this.tsmiFile.Text = "ファイル(&F)";
			// 
			// tsmiOpen
			// 
			this.tsmiOpen.Name = "tsmiOpen";
			this.tsmiOpen.Size = new System.Drawing.Size(132, 22);
			this.tsmiOpen.Text = "開く(&O)";
			this.tsmiOpen.Click += new System.EventHandler(this.tsmiOpen_Click);
			// 
			// tsmiReopen
			// 
			this.tsmiReopen.Name = "tsmiReopen";
			this.tsmiReopen.Size = new System.Drawing.Size(132, 22);
			this.tsmiReopen.Text = "開き直す(&R)";
			this.tsmiReopen.Click += new System.EventHandler(this.tsmiReopen_Click);
			// 
			// 現在のフレーム内のみ表示VToolStripMenuItem
			// 
			this.現在のフレーム内のみ表示VToolStripMenuItem.Name = "現在のフレーム内のみ表示VToolStripMenuItem";
			this.現在のフレーム内のみ表示VToolStripMenuItem.Size = new System.Drawing.Size(159, 20);
			this.現在のフレーム内のみ表示VToolStripMenuItem.Text = "現在のフレーム内のみ表示(&V)";
			this.現在のフレーム内のみ表示VToolStripMenuItem.Click += new System.EventHandler(this.現在のフレーム内のみ表示VToolStripMenuItem_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.groupBox4);
			this.panel1.Controls.Add(this.groupBox6);
			this.panel1.Controls.Add(this.groupBox5);
			this.panel1.Controls.Add(this.groupBox3);
			this.panel1.Controls.Add(this.groupBox2);
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(645, 24);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(650, 1139);
			this.panel1.TabIndex = 2;
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox4.Controls.Add(this.btnOtherThreadToClipBoard);
			this.groupBox4.Controls.Add(this.lvOtherThread);
			this.groupBox4.Location = new System.Drawing.Point(7, 725);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(640, 232);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "フレーム内別スレッド処理";
			// 
			// btnOtherThreadToClipBoard
			// 
			this.btnOtherThreadToClipBoard.Location = new System.Drawing.Point(8, 18);
			this.btnOtherThreadToClipBoard.Name = "btnOtherThreadToClipBoard";
			this.btnOtherThreadToClipBoard.Size = new System.Drawing.Size(104, 23);
			this.btnOtherThreadToClipBoard.TabIndex = 4;
			this.btnOtherThreadToClipBoard.Text = "クリップボードへ";
			this.btnOtherThreadToClipBoard.UseVisualStyleBackColor = true;
			this.btnOtherThreadToClipBoard.Click += new System.EventHandler(this.btnSameThreadToClipBoard_Click);
			// 
			// lvOtherThread
			// 
			this.lvOtherThread.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lvOtherThread.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
			this.lvOtherThread.FullRowSelect = true;
			this.lvOtherThread.GridLines = true;
			this.lvOtherThread.HideSelection = false;
			this.lvOtherThread.Location = new System.Drawing.Point(8, 47);
			this.lvOtherThread.Name = "lvOtherThread";
			this.lvOtherThread.Size = new System.Drawing.Size(626, 179);
			this.lvOtherThread.TabIndex = 1;
			this.lvOtherThread.UseCompatibleStateImageBehavior = false;
			this.lvOtherThread.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Index";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "日時";
			this.columnHeader2.Width = 130;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "IP";
			this.columnHeader3.Width = 70;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "プロセスID";
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "スレッドID";
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "メソッド";
			this.columnHeader6.Width = 242;
			// 
			// groupBox6
			// 
			this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox6.Controls.Add(this.btnSameThreadToClipBoard);
			this.groupBox6.Controls.Add(this.lvSameThread);
			this.groupBox6.Location = new System.Drawing.Point(7, 487);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(640, 232);
			this.groupBox6.TabIndex = 5;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "フレーム内同一スレッド処理";
			// 
			// btnSameThreadToClipBoard
			// 
			this.btnSameThreadToClipBoard.Location = new System.Drawing.Point(8, 18);
			this.btnSameThreadToClipBoard.Name = "btnSameThreadToClipBoard";
			this.btnSameThreadToClipBoard.Size = new System.Drawing.Size(104, 23);
			this.btnSameThreadToClipBoard.TabIndex = 4;
			this.btnSameThreadToClipBoard.Text = "クリップボードへ";
			this.btnSameThreadToClipBoard.UseVisualStyleBackColor = true;
			this.btnSameThreadToClipBoard.Click += new System.EventHandler(this.btnOtherThreadToClipBoard_Click);
			// 
			// lvSameThread
			// 
			this.lvSameThread.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lvSameThread.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12});
			this.lvSameThread.FullRowSelect = true;
			this.lvSameThread.GridLines = true;
			this.lvSameThread.HideSelection = false;
			this.lvSameThread.Location = new System.Drawing.Point(8, 47);
			this.lvSameThread.Name = "lvSameThread";
			this.lvSameThread.Size = new System.Drawing.Size(626, 179);
			this.lvSameThread.TabIndex = 1;
			this.lvSameThread.UseCompatibleStateImageBehavior = false;
			this.lvSameThread.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Index";
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "日時";
			this.columnHeader8.Width = 130;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "IP";
			this.columnHeader9.Width = 70;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "プロセスID";
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "スレッドID";
			// 
			// columnHeader12
			// 
			this.columnHeader12.Text = "メソッド";
			this.columnHeader12.Width = 242;
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox5.Controls.Add(this.tbCallStack);
			this.groupBox5.Location = new System.Drawing.Point(7, 963);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(640, 173);
			this.groupBox5.TabIndex = 4;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "コールスタック";
			// 
			// tbCallStack
			// 
			this.tbCallStack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbCallStack.HideSelection = false;
			this.tbCallStack.Location = new System.Drawing.Point(3, 15);
			this.tbCallStack.Multiline = true;
			this.tbCallStack.Name = "tbCallStack";
			this.tbCallStack.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.tbCallStack.Size = new System.Drawing.Size(634, 155);
			this.tbCallStack.TabIndex = 0;
			this.tbCallStack.WordWrap = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.btnJumpPrev);
			this.groupBox3.Controls.Add(this.btnJumpNext);
			this.groupBox3.Controls.Add(this.btnJumpDest);
			this.groupBox3.Controls.Add(this.btnJumpSource);
			this.groupBox3.Controls.Add(this.btnJunpEnterLeave);
			this.groupBox3.Location = new System.Drawing.Point(399, 15);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(195, 163);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "ジャンプ";
			// 
			// btnJumpPrev
			// 
			this.btnJumpPrev.Location = new System.Drawing.Point(8, 134);
			this.btnJumpPrev.Name = "btnJumpPrev";
			this.btnJumpPrev.Size = new System.Drawing.Size(169, 23);
			this.btnJumpPrev.TabIndex = 4;
			this.btnJumpPrev.Text = "前へ";
			this.btnJumpPrev.UseVisualStyleBackColor = true;
			this.btnJumpPrev.Click += new System.EventHandler(this.btnJumpPrev_Click);
			// 
			// btnJumpNext
			// 
			this.btnJumpNext.Location = new System.Drawing.Point(8, 105);
			this.btnJumpNext.Name = "btnJumpNext";
			this.btnJumpNext.Size = new System.Drawing.Size(169, 23);
			this.btnJumpNext.TabIndex = 3;
			this.btnJumpNext.Text = "次へ";
			this.btnJumpNext.UseVisualStyleBackColor = true;
			this.btnJumpNext.Click += new System.EventHandler(this.btnJumpNext_Click);
			// 
			// btnJumpDest
			// 
			this.btnJumpDest.Location = new System.Drawing.Point(8, 76);
			this.btnJumpDest.Name = "btnJumpDest";
			this.btnJumpDest.Size = new System.Drawing.Size(169, 23);
			this.btnJumpDest.TabIndex = 2;
			this.btnJumpDest.Text = "呼び出し先へ";
			this.btnJumpDest.UseVisualStyleBackColor = true;
			this.btnJumpDest.Click += new System.EventHandler(this.btnJumpDest_Click);
			// 
			// btnJumpSource
			// 
			this.btnJumpSource.Location = new System.Drawing.Point(8, 47);
			this.btnJumpSource.Name = "btnJumpSource";
			this.btnJumpSource.Size = new System.Drawing.Size(169, 23);
			this.btnJumpSource.TabIndex = 1;
			this.btnJumpSource.Text = "呼び出し元へ";
			this.btnJumpSource.UseVisualStyleBackColor = true;
			this.btnJumpSource.Click += new System.EventHandler(this.btnJumpSource_Click);
			// 
			// btnJunpEnterLeave
			// 
			this.btnJunpEnterLeave.Location = new System.Drawing.Point(8, 18);
			this.btnJunpEnterLeave.Name = "btnJunpEnterLeave";
			this.btnJunpEnterLeave.Size = new System.Drawing.Size(169, 23);
			this.btnJunpEnterLeave.TabIndex = 0;
			this.btnJunpEnterLeave.Text = "対応する開始または終了へ";
			this.btnJunpEnterLeave.UseVisualStyleBackColor = true;
			this.btnJunpEnterLeave.Click += new System.EventHandler(this.btnJunpEnterLeave_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.btnSelClear);
			this.groupBox2.Controls.Add(this.btnMarksToClipBoard);
			this.groupBox2.Controls.Add(this.btnSelDel);
			this.groupBox2.Controls.Add(this.btnSelAdd);
			this.groupBox2.Controls.Add(this.lvSelRanges);
			this.groupBox2.Location = new System.Drawing.Point(7, 184);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(640, 297);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "選択範囲";
			// 
			// btnSelClear
			// 
			this.btnSelClear.Location = new System.Drawing.Point(170, 18);
			this.btnSelClear.Name = "btnSelClear";
			this.btnSelClear.Size = new System.Drawing.Size(75, 23);
			this.btnSelClear.TabIndex = 4;
			this.btnSelClear.Text = "クリア";
			this.btnSelClear.UseVisualStyleBackColor = true;
			this.btnSelClear.Click += new System.EventHandler(this.btnSelClear_Click);
			// 
			// btnMarksToClipBoard
			// 
			this.btnMarksToClipBoard.Location = new System.Drawing.Point(251, 18);
			this.btnMarksToClipBoard.Name = "btnMarksToClipBoard";
			this.btnMarksToClipBoard.Size = new System.Drawing.Size(104, 23);
			this.btnMarksToClipBoard.TabIndex = 3;
			this.btnMarksToClipBoard.Text = "クリップボードへ";
			this.btnMarksToClipBoard.UseVisualStyleBackColor = true;
			this.btnMarksToClipBoard.Click += new System.EventHandler(this.btnMarksToClipBoard_Click);
			// 
			// btnSelDel
			// 
			this.btnSelDel.Location = new System.Drawing.Point(89, 18);
			this.btnSelDel.Name = "btnSelDel";
			this.btnSelDel.Size = new System.Drawing.Size(75, 23);
			this.btnSelDel.TabIndex = 2;
			this.btnSelDel.Text = "削除";
			this.btnSelDel.UseVisualStyleBackColor = true;
			this.btnSelDel.Click += new System.EventHandler(this.btnSelDel_Click);
			// 
			// btnSelAdd
			// 
			this.btnSelAdd.Location = new System.Drawing.Point(8, 18);
			this.btnSelAdd.Name = "btnSelAdd";
			this.btnSelAdd.Size = new System.Drawing.Size(75, 23);
			this.btnSelAdd.TabIndex = 1;
			this.btnSelAdd.Text = "追加";
			this.btnSelAdd.UseVisualStyleBackColor = true;
			this.btnSelAdd.Click += new System.EventHandler(this.btnSelAdd_Click);
			// 
			// lvSelRanges
			// 
			this.lvSelRanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lvSelRanges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSelDateTime,
            this.colSelTimeSpan,
            this.colSelRange,
            this.colSelColor,
            this.colSelIp,
            this.colSelPid,
            this.colSelTid,
            this.colSelMethod});
			this.lvSelRanges.FullRowSelect = true;
			this.lvSelRanges.GridLines = true;
			this.lvSelRanges.HideSelection = false;
			this.lvSelRanges.Location = new System.Drawing.Point(8, 47);
			this.lvSelRanges.Name = "lvSelRanges";
			this.lvSelRanges.Size = new System.Drawing.Size(626, 244);
			this.lvSelRanges.TabIndex = 0;
			this.lvSelRanges.UseCompatibleStateImageBehavior = false;
			this.lvSelRanges.View = System.Windows.Forms.View.Details;
			this.lvSelRanges.DoubleClick += new System.EventHandler(this.lvSelRanges_DoubleClick);
			this.lvSelRanges.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvSelRanges_KeyDown);
			// 
			// colSelDateTime
			// 
			this.colSelDateTime.Text = "日時";
			// 
			// colSelTimeSpan
			// 
			this.colSelTimeSpan.Text = "所要時間";
			// 
			// colSelRange
			// 
			this.colSelRange.Text = "範囲";
			this.colSelRange.Width = 46;
			// 
			// colSelColor
			// 
			this.colSelColor.Text = "色";
			this.colSelColor.Width = 24;
			// 
			// colSelIp
			// 
			this.colSelIp.Text = "IP";
			// 
			// colSelPid
			// 
			this.colSelPid.Text = "プロセスID";
			// 
			// colSelTid
			// 
			this.colSelTid.Text = "スレッドID";
			// 
			// colSelMethod
			// 
			this.colSelMethod.Text = "メソッド";
			this.colSelMethod.Width = 207;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.btnSearchSelAdd);
			this.groupBox1.Controls.Add(this.tbMethod);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.tbTid);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.tbPid);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.btnSearchBackward);
			this.groupBox1.Controls.Add(this.btnSearchForward);
			this.groupBox1.Controls.Add(this.tbIp);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(7, 15);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(386, 163);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "検索";
			// 
			// btnSearchSelAdd
			// 
			this.btnSearchSelAdd.Location = new System.Drawing.Point(8, 134);
			this.btnSearchSelAdd.Name = "btnSearchSelAdd";
			this.btnSearchSelAdd.Size = new System.Drawing.Size(75, 23);
			this.btnSearchSelAdd.TabIndex = 5;
			this.btnSearchSelAdd.Text = "全て追加";
			this.btnSearchSelAdd.UseVisualStyleBackColor = true;
			this.btnSearchSelAdd.Click += new System.EventHandler(this.btnSearchSelAdd_Click);
			// 
			// tbMethod
			// 
			this.tbMethod.Location = new System.Drawing.Point(49, 109);
			this.tbMethod.Name = "tbMethod";
			this.tbMethod.Size = new System.Drawing.Size(266, 19);
			this.tbMethod.TabIndex = 11;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 112);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 12);
			this.label4.TabIndex = 10;
			this.label4.Text = "メソッド:";
			// 
			// tbTid
			// 
			this.tbTid.Location = new System.Drawing.Point(49, 84);
			this.tbTid.Name = "tbTid";
			this.tbTid.Size = new System.Drawing.Size(266, 19);
			this.tbTid.TabIndex = 9;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 87);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(23, 12);
			this.label3.TabIndex = 8;
			this.label3.Text = "Tid:";
			// 
			// tbPid
			// 
			this.tbPid.Location = new System.Drawing.Point(49, 55);
			this.tbPid.Name = "tbPid";
			this.tbPid.Size = new System.Drawing.Size(266, 19);
			this.tbPid.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(23, 12);
			this.label2.TabIndex = 4;
			this.label2.Text = "Pid:";
			// 
			// btnSearchBackward
			// 
			this.btnSearchBackward.Location = new System.Drawing.Point(321, 18);
			this.btnSearchBackward.Name = "btnSearchBackward";
			this.btnSearchBackward.Size = new System.Drawing.Size(59, 35);
			this.btnSearchBackward.TabIndex = 3;
			this.btnSearchBackward.Text = "↑";
			this.btnSearchBackward.UseVisualStyleBackColor = true;
			this.btnSearchBackward.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// btnSearchForward
			// 
			this.btnSearchForward.Location = new System.Drawing.Point(321, 60);
			this.btnSearchForward.Name = "btnSearchForward";
			this.btnSearchForward.Size = new System.Drawing.Size(59, 35);
			this.btnSearchForward.TabIndex = 2;
			this.btnSearchForward.Text = "↓";
			this.btnSearchForward.UseVisualStyleBackColor = true;
			this.btnSearchForward.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// tbIp
			// 
			this.tbIp.Location = new System.Drawing.Point(49, 26);
			this.tbIp.Name = "tbIp";
			this.tbIp.Size = new System.Drawing.Size(266, 19);
			this.tbIp.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 29);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP:";
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(642, 24);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 1139);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// lvRecords
			// 
			this.lvRecords.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIndex,
            this.colDateTime,
            this.colIp,
            this.colPid,
            this.colTid,
            this.colEnterLeave,
            this.colMethod});
			this.lvRecords.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvRecords.FullRowSelect = true;
			this.lvRecords.GridLines = true;
			this.lvRecords.HideSelection = false;
			this.lvRecords.Location = new System.Drawing.Point(0, 24);
			this.lvRecords.Name = "lvRecords";
			this.lvRecords.Size = new System.Drawing.Size(642, 1139);
			this.lvRecords.TabIndex = 4;
			this.lvRecords.UseCompatibleStateImageBehavior = false;
			this.lvRecords.View = System.Windows.Forms.View.Details;
			// 
			// colIndex
			// 
			this.colIndex.Text = "Index";
			// 
			// colDateTime
			// 
			this.colDateTime.Text = "日時";
			this.colDateTime.Width = 141;
			// 
			// colIp
			// 
			this.colIp.Text = "IP";
			this.colIp.Width = 82;
			// 
			// colPid
			// 
			this.colPid.Text = "プロセスID";
			this.colPid.Width = 71;
			// 
			// colTid
			// 
			this.colTid.Text = "スレッドID";
			this.colTid.Width = 83;
			// 
			// colEnterLeave
			// 
			this.colEnterLeave.Text = "開始/終了";
			this.colEnterLeave.Width = 67;
			// 
			// colMethod
			// 
			this.colMethod.Text = "メソッド";
			this.colMethod.Width = 1000;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1295, 1163);
			this.Controls.Add(this.lvRecords);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "メソッドログビューワ";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem tsmiFile;
		private System.Windows.Forms.ToolStripMenuItem tsmiOpen;
		private System.Windows.Forms.ToolStripMenuItem tsmiReopen;
        private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbTid;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbPid;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnSearchBackward;
		private System.Windows.Forms.Button btnSearchForward;
		private System.Windows.Forms.TextBox tbIp;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnSelDel;
		private System.Windows.Forms.Button btnSelAdd;
		private System.Windows.Forms.ListView lvSelRanges;
		private System.Windows.Forms.ColumnHeader colSelRange;
		private System.Windows.Forms.ColumnHeader colSelColor;
		private System.Windows.Forms.ColumnHeader colSelMethod;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnJunpEnterLeave;
		private System.Windows.Forms.ColumnHeader colSelIp;
		private System.Windows.Forms.ColumnHeader colSelPid;
		private System.Windows.Forms.ColumnHeader colSelTid;
        private System.Windows.Forms.TextBox tbMethod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnJumpPrev;
        private System.Windows.Forms.Button btnJumpNext;
        private System.Windows.Forms.Button btnJumpDest;
        private System.Windows.Forms.Button btnJumpSource;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.ListView lvOtherThread;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox tbCallStack;
        private System.Windows.Forms.Button btnOtherThreadToClipBoard;
        private System.Windows.Forms.Button btnMarksToClipBoard;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripMenuItem 現在のフレーム内のみ表示VToolStripMenuItem;
        private System.Windows.Forms.Button btnSelClear;
        private System.Windows.Forms.Button btnSearchSelAdd;
        private System.Windows.Forms.ColumnHeader colSelDateTime;
        private System.Windows.Forms.ColumnHeader colSelTimeSpan;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListView lvRecords;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colDateTime;
        private System.Windows.Forms.ColumnHeader colIp;
        private System.Windows.Forms.ColumnHeader colPid;
        private System.Windows.Forms.ColumnHeader colTid;
        private System.Windows.Forms.ColumnHeader colEnterLeave;
        private System.Windows.Forms.ColumnHeader colMethod;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnSameThreadToClipBoard;
        private System.Windows.Forms.ListView lvSameThread;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
	}
}

