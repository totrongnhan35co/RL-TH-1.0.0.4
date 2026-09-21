using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeVerificationSystem.View
{
    public partial class frmBased : Form
    {
        public frmBased()
        {
            InitializeComponent();
        }

        public void UpdateIcon()
        {
            String path = Application.StartupPath + "\\Label\\icon.ico"; // logores.ico
            if (File.Exists(path))
            {
                this.Icon = Icon.ExtractAssociatedIcon(path);
            }
            else
            {
                this.ShowIcon = false;
            }
        }
        
    }
}
