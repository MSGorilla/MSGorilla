using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.Diagnostics;

namespace MSGorilla.OutlookAddin
{
    public partial class Ribbon
    {
        private void Ribbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void gotoMSGorilla(object sender, RibbonControlEventArgs e)
        {
            Process.Start("https://msgorilla.cloudapp.net/");
        }

        private void showTaskPaneBtn_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.ThisAddIn.MSGorillaTaskPane.Visible = ((RibbonToggleButton)sender).Checked;
        }
    }
}
