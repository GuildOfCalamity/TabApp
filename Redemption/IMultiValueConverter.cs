namespace TabApp.Redemption;

/// <summary>
/// Interface which must be implemented by multi-binding
/// converters assigned to <see cref="MultiBinding.Converter"/>.
/// </summary>
public interface IMultiValueConverter
{
    object Convert(object[] values, object parameter);
}
