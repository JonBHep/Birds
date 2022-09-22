using System;
using System.Windows;

namespace Birds;

public partial class InputBox : Window
{
    public InputBox(string BoxTitle, string PromptText, string DefaultResponse)
    {
        InitializeComponent();
        this.Title = BoxTitle;
        textblockPrompt.Text = PromptText;
        textboxResponse.SelectedText = DefaultResponse;
    }

    public string ResponseText { get { return textboxResponse.Text; } }

    private void ButtonOkay_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        Icon = this.Owner.Icon;
        textboxResponse.Focus();
    }
}