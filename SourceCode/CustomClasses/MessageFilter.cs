using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;           // For Debugging

namespace TMCAnalyzer
{
    public class MyMessageFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
 
            //  THIS EVENT FIRES ALMOST CONSTANTLY. WHAT'S HAPPENING?
            //
            // System.Diagnostics.Debugger.Break();
            // GARY-FIX:
            // First:   What is this?? 
            //          It appears to be something written by Venture Technologies to catch a particular
            //          error or situation when using the terminal form.
            //
            //          
            //
            // Second:  When other errors occur (specifically on the Connection Screen around setting
            //          values (or Null) to the "Controllers" array... we end up in here.
            //
            //          But no error message is generated. It just stops running the code but the system
            //          doesn't crash. It just puts up the existing form and returns control to the 
            //          user / user interface.
            //
            //  I'm putting this BREAK-POINT here in CODE so that I realize what is happening.
            //
            //  This should be understood & fixed
            //



            //if (m.Msg == 513 && formMain.MenuModeFlag)
            //{
            //    // Left mouse button is down (513) while Terminal form is in advanced menu mode 
            //    DialogResult result;

            //    string messageOut = "The Virtual Terminal window is running in Advanced Menu mode.\r\n" +
            //    "You must leave Advanced menu mode to use this window.\r\n" +
            //    "Do you wish to leave Virtual Terminal Advanced mode?";

            //    result = MessageBox.Show(messageOut, "Terminal Advanced Menu and Other forms", MessageBoxButtons.YesNo);

            //    if (result.Equals(DialogResult.Yes))
            //    {
            //        formMain.myfrmTerminal.EnsureMenuOut();
            //        return false;
            //    }
            //    else
            //        return true; // ignore mouse down
            //}

           return false;
        }
    }

}