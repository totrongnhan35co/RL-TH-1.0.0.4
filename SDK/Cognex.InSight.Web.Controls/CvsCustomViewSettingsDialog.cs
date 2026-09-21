// Copyright (c) 2022 Cognex Corporation. All Rights Reserved

using System;
using System.Windows.Forms;
using Cognex.InSight.Remoting.Serialization;

namespace Cognex.InSight.Web.Controls
{
  public partial class CvsCustomViewSettingsDialog : Form
  {
    private HmiCustomViewSettings _settings;

    public HmiCustomViewSettings Settings 
    { 
      get 
      {
        _settings.FromCellRange(tbCellRange.Text);
        _settings.X = (int)numX.Value;
        _settings.Y = (int)numY.Value;
        _settings.Width = (int)numWidth.Value;
        _settings.Height = (int)numHeight.Value;
        return _settings; 
      } 
    }

    public CvsCustomViewSettingsDialog()
    {
      InitializeComponent();
    }

    public CvsCustomViewSettingsDialog(HmiCustomViewSettings settings)
    {
      _settings = settings;
      InitializeComponent();
    }

    private void CvsCustomViewSettingsDialog_Load(object sender, EventArgs e)
    {
      if (_settings != null)
      {
        tbCellRange.Text = _settings.ToCellRange();
        numX.Value = _settings.X;
        numY.Value = _settings.Y;
        numWidth.Value = _settings.Width;
        numHeight.Value = _settings.Height;
      }
    }
  }
}
