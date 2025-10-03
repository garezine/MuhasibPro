﻿// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.



using MuhasibPro.ViewModels.Firmalar;

namespace MuhasibPro.Views.Firma;
public sealed partial class FirmaDetails : UserControl
{
    public FirmaDetails()
    {
        InitializeComponent();
    }
    #region ViewModel
    public FirmaDetailsViewModel ViewModel
    {
        get { return (FirmaDetailsViewModel)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(FirmaDetailsViewModel), typeof(FirmaCard), new PropertyMetadata(null));
    #endregion
    public void SetFocus()
    {
        details.SetFocus();
    }
}
