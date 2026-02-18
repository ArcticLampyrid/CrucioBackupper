using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CrucioBackupper;

public partial class LoginViaTokenDialog : Window
{
    public string? Token { get; private set; }

    public LoginViaTokenDialog()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        Token = TokenTextBox.Text?.Trim();
        Close();
    }
}
