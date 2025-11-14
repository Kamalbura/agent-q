using System.Collections.Generic;
using System.Windows;

namespace Assistant.UI;

public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog(IReadOnlyList<string> ambiguousItems, string focusedWindowTitle)
    {
        InitializeComponent();
        ItemsList.ItemsSource = ambiguousItems;
        FocusedWindowText.Text = string.IsNullOrWhiteSpace(focusedWindowTitle)
            ? "Focused window unknown"
            : $"Focused window: {focusedWindowTitle}";
    }

    private void OnConfirm(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
