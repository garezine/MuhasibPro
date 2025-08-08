using Muhasebe.Business.Common;
using System.Collections.ObjectModel;

namespace MuhasibPro.Core.Models;

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
        Glyph = Char.ConvertFromUtf32(glyph).ToString();
    }

    public NavigationItem(int glyph, string label, Type viewModel, ObservableCollection<NavigationItem> children) : this(glyph, label, viewModel)
    {
        Children = children ?? new ObservableCollection<NavigationItem>();
    }

    public readonly string Glyph;
    public readonly string Label;
    public readonly Type ViewModel;
    public readonly ObservableCollection<NavigationItem> Children;

    private string _badge = null;
    public string Badge
    {
        get => _badge;
        set => Set(ref _badge, value);
    }

    private bool _isExpanded = false;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    // Alt menü olup olmadığını kontrol eder
    public bool HasChildren => Children?.Count > 0;

    // Ana menü item mı yoksa alt menü item mi kontrol eder
    public bool IsParent => HasChildren;

    // Alt menü eklemek için helper method
    public void AddChild(NavigationItem child)
    {
        Children?.Add(child);
    }

    // Alt menü eklemek için overload
    public void AddChild(int glyph, string label, Type viewModel)
    {
        var child = new NavigationItem(glyph, label, viewModel);
        Children?.Add(child);
    }
}

