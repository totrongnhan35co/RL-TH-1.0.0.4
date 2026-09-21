// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Cognex.InSight.Remoting.Serialization;
using Cognex.InSight.Web;

namespace Cognex.InSight.Web.Controls
{
  public partial class CvsCustomView : CvsSpreadsheet
  {
    public CvsCustomView() : base(true)
    {
      InitializeComponent();
    }

    public override void InitSpreadsheet()
    {
      if (IsCustomView)
      {
        if (_inSight.CustomViewSettings != null)
        {
          HmiCustomViewSettings cvSettings = _inSight.CustomViewSettings;
          _cellRange = new CellRange(cvSettings.Top, cvSettings.Left, cvSettings.Bottom - cvSettings.Top + 1, cvSettings.Right - cvSettings.Left + 1);
        }
      }
      base.InitSpreadsheet();
    }

    /// <summary>
    /// Sets the CvsInSight and cell range for this Spreadsheet.
    /// </summary>
    /// <param name="inSight"></param>
    /// <param name="range"></param>
    public override void SetInSight(CvsInSight inSight, CellRange range = null)
    {
      base.SetInSight(inSight, range);
    }
  }
}
