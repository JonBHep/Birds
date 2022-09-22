using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Birds;

public partial class BirdWindow : Window
{
    private Bird _oiseau;
        //private string _originalSciName;

        public BirdWindow(int oiso)
        {
            InitializeComponent();
            _oiseau = new Bird();
            _oiseau.Key = oiso;
            if (oiso > 0) { _oiseau.Mirror(Core.Instance.BirdDictionary[oiso]); }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            StatusComboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                      new System.Windows.Controls.TextChangedEventHandler(Box_TextChanged));
            // The above enables using a textchanged event with an editable combobox

            KeyTextBlock.Text = $"Key: {_oiseau.Key} Taxonomic sequence index: {_oiseau.TaxonomicSequenceIndex}";
            Eng2007TextBox.Text = _oiseau.EnglishNameCurrent;
            Eng1923TextBox.Text = _oiseau.OtherEnglishNamesBloc;
            EngIntlTextBox.Text = _oiseau.DifferentInternationalEnglishName;
            FrenchTextBox.Text = _oiseau.French;
            Sci2007TextBox.Text = _oiseau.ScientificNameCurrent;
            Sci1923TextBox.Text = _oiseau.FormerScientificNamesBloc;
            BOUCodeTextBox.Text = _oiseau.BOUCode;
            List<string> list = Core.Instance.StatusPickList;
            foreach (string s in list) { StatusComboBox.Items.Add(s); }
            StatusComboBox.Text = _oiseau.Status;
            List<BirdFamily> families = new List<BirdFamily>();
            foreach (BirdFamily fmy in Core.Instance.FamilyDictionary.Values)
            {
                families.Add(fmy);
            }
            families.Sort();
            int fk = _oiseau.FamilyKey;
            FreqCheckBox.IsChecked = _oiseau.SeenFrequently;
            FamiliarSightCheckBox.IsChecked = _oiseau.Recognisable;
            FamiliarSoundCheckBox.IsChecked = _oiseau.RecognisableVoice;
            
            RareCheckBox.IsChecked = _oiseau.Rare;
            SeenCheckBox.IsChecked = _oiseau.SeenEver;

            Eng2007TextBox.Focus();

            DisplayPictureCount();
            DisplayAudioCount();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            bool missing = false;

            _oiseau.BOUCode = BOUCodeTextBox.Text.Trim();

            _oiseau.OtherEnglishNamesBloc = Eng1923TextBox.Text.Trim();
            _oiseau.EnglishNameCurrent = Eng2007TextBox.Text.Trim();

            _oiseau.DifferentInternationalEnglishName = EngIntlTextBox.Text.Trim();
            _oiseau.French = FrenchTextBox.Text.Trim();
            _oiseau.FormerScientificNamesBloc = Sci1923TextBox.Text.Trim();
            _oiseau.ScientificNameCurrent = Sci2007TextBox.Text.Trim();
            _oiseau.SeenEver = (SeenCheckBox.IsChecked.HasValue && SeenCheckBox.IsChecked.Value);
            _oiseau.SeenFrequently = (FreqCheckBox.IsChecked.HasValue && FreqCheckBox.IsChecked.Value);
            _oiseau.Recognisable =  (FamiliarSightCheckBox.IsChecked.HasValue && FamiliarSightCheckBox.IsChecked.Value);
            _oiseau.RecognisableVoice = (FamiliarSoundCheckBox.IsChecked.HasValue && FamiliarSoundCheckBox.IsChecked.Value);
            _oiseau.Rare = (RareCheckBox.IsChecked.HasValue && RareCheckBox.IsChecked.Value);
            _oiseau.Status = StatusComboBox.Text.Trim();
            // error checking
            if (string.IsNullOrWhiteSpace(_oiseau.EnglishNameCurrent)) { missing = true; }
            if (string.IsNullOrWhiteSpace(_oiseau.ScientificNameCurrent)) { missing = true; }
            if (string.IsNullOrWhiteSpace(_oiseau.BOUCode)) { missing = true; }
            if (missing)
            {
                MessageBox.Show("Essential data is missing.\n\nEssential items are the current English and scientific names and the BOU occurrence code.\n\nThe BTO codes, if present, must be of length 2 and 5 characters.", "Birds", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            
            DialogResult = true;
        }

        private void ShowTrafficLight()
        {
            bool missing = false;
            bool missingA = false;
            
            if (string.IsNullOrWhiteSpace(StatusComboBox.Text)) { missing = true; }

            if (string.IsNullOrWhiteSpace(Eng2007TextBox.Text)) { missing = true; }
            if (string.IsNullOrWhiteSpace(Sci2007TextBox.Text)) { missing = true; }
            if (string.IsNullOrWhiteSpace(BOUCodeTextBox.Text)) { missing = true; }
            if (string.IsNullOrWhiteSpace(FrenchTextBox.Text)) { missingA = true; }

            TrafficLight.Fill = Brushes.Green;
            if (missingA) { TrafficLight.Fill = Brushes.Orange; }
            if (missing) { TrafficLight.Fill = Brushes.Red; }
        }

        public Bird BirdData { get { return _oiseau; } }

        private void NotesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oiseau.Key<1)
            {
                MessageBox.Show("Close the window to create the new bird in the dictionary before trying to edit notes.", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string notepath = Core.BirdTextSpecification(_oiseau.Key);
            NotepadWindow npw = new NotepadWindow($"Notes for {_oiseau.EnglishNameCurrent}") { FilePath = notepath };
            npw.ShowDialog();
        }
        
        private void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowTrafficLight();
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowTrafficLight();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oiseau.Key < 1)
            {
                MessageBox.Show("Close the window to create the new bird in dictionary before adding pictures", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int index = Core.Instance.BirdPictureUnusedIndex(_oiseau.Key);
            string destination = Core.BirdPictureSpecification(_oiseau.Key, index);
            // find user's Downloads directory
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Personal is supposedly equivalent to Documents, but Documents leads to Cloud directory rather than PC hard drive
            path = System.IO.Directory.GetParent(path).FullName;
            path = System.IO.Path.Combine(path, "Downloads");

            string source = string.Empty;
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "jpg",
                Filter = "Jpg images (*.jpg)|*.jpg",
                Multiselect = false,
                Title = "Select image",
                ValidateNames = true,
                InitialDirectory = path
            };
            if (ofd.ShowDialog() == true)
            {
                source = ofd.FileName;
                System.IO.File.Copy(source, destination);
                try
                {
                    System.IO.File.Delete(source);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error deleting source image", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            DisplayPictureCount();
        }

        private void AudioButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oiseau.Key < 1)
            {
                MessageBox.Show("Close the window to create the new bird in dictionary before adding audio files", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // find user's Downloads directory
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Personal is supposedly equivalent to Documents, but Documents leads to Cloud directory rather than PC hard drive
            path = System.IO.Directory.GetParent(path).FullName;
            path = System.IO.Path.Combine(path, "Downloads");

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "mp3",
                Filter = "Mp3 files (*.mp3)|*.mp3",
                Multiselect = true,
                Title = "Select audio file",
                ValidateNames = true,
                InitialDirectory = path
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (string source in ofd.FileNames)
                {
                    int index = Core.Instance.BirdAudioUnusedIndex(_oiseau.Key);
                    string destination = Core.BirdAudioSpecification(_oiseau.Key, index);
                    System.IO.File.Copy(source, destination);
                    try
                    {
                        System.IO.File.Delete(source);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error deleting source audio file", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            DisplayAudioCount();
        }
        private void DisplayPictureCount()
        {
            int h= Core.BirdPictureFilePaths(_oiseau.Key).Count;
            ImageCountTextBlock.Text =(h==1) ? "1 picture": $"{h} pictures";
        }

        private void DisplayAudioCount()
        {
            int h =Core.BirdAudioFilePaths(_oiseau.Key).Count;
           AudioCountTextBlock.Text = (h == 1) ? "1 clip" : $"{h} clips";
        }

        private void FrenchTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FrenchTextBox.Text))
            {
                string francy = Clipboard.GetText(TextDataFormat.Text);
                francy = francy.Trim();
                FrenchTextBox.Text = francy;
            }
        }

        private void CopySciNameButton_Click(object sender, RoutedEventArgs e)
        {
            string s = _oiseau.ScientificNameCurrent;
            Clipboard.SetText(s);
            Button b = sender as Button;
            b.IsEnabled = false;
        }
}