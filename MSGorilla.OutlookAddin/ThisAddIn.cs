using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools;

using MSGorilla.OutlookAddin.GUI;

namespace MSGorilla.OutlookAddin
{
    public partial class ThisAddIn
    {
        private CustomTaskPane _msgorillaTaskPane;

        public CustomTaskPane MSGorillaTaskPane
        {
            get
            {
                return _msgorillaTaskPane;
            }
        }

        public bool KeepUpdatingInfo { get; set; }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            KeepUpdatingInfo = true;
            Globals.Ribbons.Ribbon.keepUpdateBtn.Checked = true;

            ShortcutContainer shortcut = new ShortcutContainer();
            _msgorillaTaskPane = this.CustomTaskPanes.Add(new ShortcutContainer(), "MSGorilla");
            _msgorillaTaskPane.Visible = true;

            _msgorillaTaskPane.VisibleChanged += taskPaneValue_VisibleChanged;
        }

        private void taskPaneValue_VisibleChanged(object sender, System.EventArgs e)
        {
            Globals.Ribbons.Ribbon.showTaskPaneBtn.Checked =
                _msgorillaTaskPane.Visible;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
