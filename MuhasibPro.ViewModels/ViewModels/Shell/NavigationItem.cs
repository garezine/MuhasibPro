using Muhasib.Business.Models.Common;
using System.Collections.ObjectModel;

namespace MuhasibPro.ViewModels.ViewModels.Shell;

public class NavigationItem : ObservableObject
{
    public NavigationItem(Type viewModel)
    {
        ViewModel = viewModel;
        Children = new ObservableCollection<NavigationItem>();
    }

    public NavigationItem(int glyph, string label, Type viewModel) : this(viewModel)
    {
        Label = label;
        Glyph = char.ConvertFromUtf32(glyph).ToString();
    }

    public NavigationItem(int glyph, string label, Type viewModel, ObservableCollection<NavigationItem> children) : this(glyph, label, viewModel)
    {
        Children = children ?? new ObservableCollection<NavigationItem>();
    }

    public string Glyph { get; }
    public string Label { get; }
    public Type ViewModel { get; }
    public ObservableCollection<NavigationItem> Children { get; }

    // Badge - Dinamik olacak
    private string _badge;
    public string Badge
    {
        get => _badge;
        set => Set(ref _badge, value);
    }
    // Badge görünürlüğü
    private bool _isBadgeVisible;
    public bool IsBadgeVisible
    {
        get => _isBadgeVisible;
        set => Set(ref _isBadgeVisible, value);
    }


    // NavigationView için gerekli
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    // Helper - Parent mı child mı?
    public bool IsParent => Children?.Count > 0;
    // Helper method - badge güncelleme
    public void UpdateBadge(string value)
    {
        Badge = value;
        IsBadgeVisible = !string.IsNullOrEmpty(value);
    }

    // Helper method - badge gizleme
    public void HideBadge()
    {
        Badge = null;
        IsBadgeVisible = false;
    }
}

