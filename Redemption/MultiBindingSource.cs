using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace TabApp.Redemption;

/// <summary>
/// A binding source that is part of a 
/// <see cref="MultiBinding"/>. A bug/quirk in
/// WinUI appears to prevent defining <see cref="BindingBase"/>s
/// as direct members of a collection in XAML, plus this is more
/// friendly to curly brace binding notation.
/// </summary>
[ContentProperty(Name = nameof(Binding))]
public class MultiBindingSource
{
    /// <summary>
    /// The <see cref="Microsoft.UI.Xaml.Data.Binding"/> instance.
    /// Note all binding types and options are supported except for
    /// <see cref="BindingMode.TwoWay"/>.
    /// </summary>
    public Binding Binding { get; set; }
}
