namespace MuhasibPro.Controls;

public sealed class RoundButton : Button
{
    public RoundButton()
    {
        DefaultStyleKey = typeof(RoundButton);
    }

    public double Radius
    {
        get
        {
            return (double)GetValue(RadiusProperty);
        }
        set
        {
            SetValue(CornerRadiuseProperty, new CornerRadius(value));
            SetValue(RadiusProperty, value);

        }
    }
    public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(Radius), typeof(double), typeof(RoundButton), new PropertyMetadata(0));



    #region CornerRadius
    public CornerRadius CornerRadiuse
    {
        get
        {
            return (CornerRadius)GetValue(CornerRadiuseProperty);
        }
        set
        {
            SetValue(CornerRadiuseProperty, value);
        }
    }

    public static readonly DependencyProperty CornerRadiuseProperty = DependencyProperty.Register(nameof(CornerRadiuse), typeof(CornerRadius), typeof(RoundButton), new PropertyMetadata(new CornerRadius(0)));
    #endregion
}
