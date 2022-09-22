using System.Windows;

namespace Birds;

public partial class NotepadWindow : Window
{
    private string _filepath;

    public string FilePath { set { _filepath = value; LoadFromFile(); } }

    public NotepadWindow(string Caption)
    {
        InitializeComponent();
        Title = Caption;
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        string note = Clipboard.GetText(TextDataFormat.Text);
        note = SuperTrimmed(note);
        NoteTextBox.Text = note;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (NoteTextBox.Text.Length == 0)
        {
            if (System.IO.File.Exists(_filepath)) { System.IO.File.Delete(_filepath); }
        }
        else
        {
            SaveToFile();
        }
        DialogResult = true;
    }

    private void LoadFromFile()
    {
        if (!System.IO.File.Exists(_filepath)) { return; }
        string nb = string.Empty;
        using (System.IO.StreamReader sr = new System.IO.StreamReader(_filepath))
        {
            nb = sr.ReadToEnd();
        }
        nb = SuperTrimmed(nb);
        NoteTextBox.Text = nb;
    }

    private void SaveToFile()
    {
        string nb = NoteTextBox.Text.Trim();
        nb = SuperTrimmed(nb);
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(_filepath))
        {
            foreach (char c in nb)
            {
                sw.Write(c);
            }
        }
    }

    private string SuperTrimmed(string raw)
    {
        string answ = string.Empty;
        foreach(char c in raw)
        {
            if (!char.IsControl(c)) { answ += c; }
        }
        answ = answ.Trim();
        return answ;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        NoteTextBox.Clear();
    }
}