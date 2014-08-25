namespace MSGorilla.OutlookAddin
{
    partial class Ribbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab = this.Factory.CreateRibbonTab();
            this.MSGorillaGroup = this.Factory.CreateRibbonGroup();
            this.gotoMSGorillaBtn = this.Factory.CreateRibbonButton();
            this.showTaskPaneBtn = this.Factory.CreateRibbonToggleButton();
            this.keepUpdateBtn = this.Factory.CreateRibbonToggleButton();
            this.tab.SuspendLayout();
            this.MSGorillaGroup.SuspendLayout();
            // 
            // tab1
            // 
            this.tab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab.Groups.Add(this.MSGorillaGroup);
            this.tab.Label = "TabAddIns";
            this.tab.Name = "tab1";
            // 
            // MSGorillaGroup
            // 
            this.MSGorillaGroup.Items.Add(this.gotoMSGorillaBtn);
            this.MSGorillaGroup.Items.Add(this.showTaskPaneBtn);
            this.MSGorillaGroup.Items.Add(this.keepUpdateBtn);
            this.MSGorillaGroup.Label = "MSGorilla";
            this.MSGorillaGroup.Name = "MSGorillaGroup";
            // 
            // gotoMSGorillaBtn
            // 
            this.gotoMSGorillaBtn.Label = "Goto MSGorilla";
            this.gotoMSGorillaBtn.Name = "gotoMSGorillaBtn";
            this.gotoMSGorillaBtn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.gotoMSGorilla);
            // 
            // showTaskPane
            // 
            this.showTaskPaneBtn.Label = "Show TaskPane";
            this.showTaskPaneBtn.Name = "showTaskPane";
            this.showTaskPaneBtn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.showTaskPaneBtn_Click);
            // 
            // keepUpdate
            // 
            this.keepUpdateBtn.Label = "Updating Info";
            this.keepUpdateBtn.Name = "keepUpdate";
            this.keepUpdateBtn.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.keepUpdateBtn_Click);
            // 
            // Ribbon
            // 
            this.Name = "Ribbon";
            this.RibbonType = "Microsoft.Outlook.Explorer";
            this.Tabs.Add(this.tab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon_Load);
            this.tab.ResumeLayout(false);
            this.tab.PerformLayout();
            this.MSGorillaGroup.ResumeLayout(false);
            this.MSGorillaGroup.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup MSGorillaGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton gotoMSGorillaBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton showTaskPaneBtn;
        internal Microsoft.Office.Tools.Ribbon.RibbonToggleButton keepUpdateBtn;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon Ribbon
        {
            get { return this.GetRibbon<Ribbon>(); }
        }
    }
}
