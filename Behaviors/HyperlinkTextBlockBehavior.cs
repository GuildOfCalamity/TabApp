using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity; // NuGet => Microsoft.Xaml.Behaviors.WinUI.Managed

namespace TabApp.Behaviors;

/// <summary>
/// Parts of this code borrowed from https://xamlbrewer.wordpress.com/2023/01/16/xaml-behaviors-and-winui-3/
/// I have converted this to a standard <see cref="Behavior{T}"/> from the BehaviorBase (Community version) and fixed the RegEx.
/// </summary>
public class HyperlinkTextBlockBehavior : Behavior<TextBlock>
{
    long? _textChangedRegistration;
    static readonly Regex UrlRegex = new Regex(@"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?", RegexOptions.Compiled);

    public Brush? Foreground { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (_textChangedRegistration == null)
            _textChangedRegistration = AssociatedObject.RegisterPropertyChangedCallback(TextBlock.TextProperty, Text_PropertyChanged);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null && _textChangedRegistration != null && _textChangedRegistration.HasValue)
            AssociatedObject.UnregisterPropertyChangedCallback(TextBlock.TextProperty, _textChangedRegistration.Value);
    }

    void Text_PropertyChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (AssociatedObject != null && !string.IsNullOrWhiteSpace(AssociatedObject.Text))
        {
            var first = true;
            var text = AssociatedObject.Text;
            var last_index = 0;

            foreach (Match match in UrlRegex.Matches(text))
            {
                if (first)
                {
                    AssociatedObject.Inlines.Clear();
                    first = false;
                }

                var left_text = text.Substring(last_index, match.Index - last_index);
                last_index = match.Index + match.Length;

                AssociatedObject.Inlines.Add(new Run() { Text = left_text });
                AssociatedObject.Inlines.Add(new Hyperlink()
                {
                    NavigateUri = new System.Uri(match.Value),
                    Inlines = { new Run() { Text = match.Value } }
                });
            }

            if (!first && last_index < text.Length)
            {
                var right_text = text.Substring(last_index);
                AssociatedObject.Inlines.Add(new Run() { Text = right_text });
            }
        }
    }
}