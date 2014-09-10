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

        private string _currentUser;
        public string CurrentUserID
        {
            get
            {
                return _currentUser;
            }
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Globals.Ribbons.Ribbon.keepUpdateBtn.Checked = true;
            Globals.Ribbons.Ribbon.showTaskPaneBtn.Checked = true;

            GetCurrentUser();

            ShortcutContainer shortcut = new ShortcutContainer();
            _msgorillaTaskPane = this.CustomTaskPanes.Add(shortcut, "MSGorilla");
            _msgorillaTaskPane.Visible = Globals.Ribbons.Ribbon.showTaskPaneBtn.Checked;
            _msgorillaTaskPane.VisibleChanged += taskPaneValue_VisibleChanged;
        }

        void GetCurrentUser()
        {
            try
            {
                string currentUser = Application.Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress;
                string[] split = currentUser.Split('@');
                if (split.Length != 2 || !split[1].Equals("microsoft.com"))
                {
                    throw new Exception("Unrecognized email account");
                }
                _currentUser = split[0];
                _currentUser = "user1";
            }
            catch (Exception e)
            {
                Globals.Ribbons.Ribbon.showTaskPaneBtn.Checked = false;
                Globals.Ribbons.Ribbon.keepUpdateBtn.Checked = false;
            }            
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
