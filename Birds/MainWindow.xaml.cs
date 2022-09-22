using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Birds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _selectedBirdId = 0;
        private bool _ready = false;
        private List<string> _picturePaths;
        private string _picturePath;
        private MediaPlayer _player = new MediaPlayer();
        private DispatcherTimer _tim = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double scrX = System.Windows.SystemParameters.PrimaryScreenWidth;
            double scrY = System.Windows.SystemParameters.PrimaryScreenHeight;
            double winX = scrX * .99;
            double winY = scrY * .94;
            double Xm = (scrX - winX) / 2;
            double Ym = (scrY - winY) / 4;
            this.Width = winX;
            this.Height = winY;
            this.Left = Xm;
            this.Top = Ym;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Core.Instance.LoadData();
            _ready = true;
            RefreshBirdList();
            SoundProgressBar.Maximum = 100;
            _tim.Interval = TimeSpan.FromMilliseconds(300);
            _tim.Tick += Timer_Tick;
            SoundProgressBar.Visibility = Visibility.Hidden;
            SoundComboBox.Visibility = Visibility.Hidden;
            SoundPlayButton.Visibility = Visibility.Hidden;
            FillAlphabetListBox();
        }

        private void FillAlphabetListBox()
        {
            string alpha = "-ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int l=0; l < 26; l++)
            {
                TextBlock ab = new TextBlock() { Text = alpha.Substring(l, 1), TextAlignment= TextAlignment.Center, FontFamily=new FontFamily( "Consolas" )};
                ListBoxItem it = new ListBoxItem() { Content = ab, Background= Brushes.Ivory };
                AlphabetListBox.Items.Add(it);
            }
        }
        private void RefreshBirdList()
        {
            if (!_ready) { return; }
            Dictionary<int, int> audioCounts = Core.SoundCounts();
            Dictionary<int, int> videoCounts = Core.ImageCounts();
            List<Tuple<string, int>> birds = GetListType();
            string filter = FilterTextBox.Text.Trim();
            BirdListBox.Items.Clear();
            BirdListBox.FontFamily = new FontFamily("Consolas");

            int stats_tot = 0;
            int stats_moi = 0;
            foreach (Tuple<string, int> k in birds)
            {
                Bird bd = Core.Instance.BirdDictionary[k.Item2];
                stats_tot++;
                if (bd.SeenEver) { stats_moi++; }
                StackPanel VerticalPanel = new StackPanel() { Margin = new Thickness(0, 0, 0, 6) };
                TextBlock tbkTick = new TextBlock() { Width = 10, Padding = new Thickness(0, 0, 4, 0), FontFamily = new FontFamily("Wingdings"), Text = string.Empty };
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                if (bd.SeenEver) { tbkTick.Text = "ü"; }
                TextBlock tbkAmi = new TextBlock() { Width = 10, Padding = new Thickness(0, 0, 4, 0), FontFamily = new FontFamily("Wingdings"), Text = string.Empty };
                if (bd.Recognisable) { tbkAmi.Text = "J"; }
                TextBlock tbkRare = new TextBlock() { Width = 16, Padding = new Thickness(0, 0, 4, 0), FontFamily = new FontFamily("Wingdings"), Text = string.Empty };
                if (bd.Rare) { tbkRare.Text = "²"; }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                TextBlock tbkMain = new TextBlock() { Foreground = bd.BirdBrush };
                string tip = bd.BirdBrushMeaning;
                if (!string.IsNullOrEmpty(tip)) { tbkMain.ToolTip = tip; }

                List<string> shownnames = new List<string>() { k.Item1 };
                tbkMain.Text = k.Item1; // name varies with type of listing (Eng, Fre, Int etc.)

                StackPanel spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                spnl.Children.Add(tbkTick);
                spnl.Children.Add(tbkAmi);
                spnl.Children.Add(tbkRare);
                spnl.Children.Add(tbkMain);
                VerticalPanel.Children.Add(spnl);

                string countString = string.Empty;
                if (videoCounts.ContainsKey(bd.Key)) { countString = $"P{videoCounts[bd.Key]}"; }
                if (audioCounts.ContainsKey(bd.Key)) { countString += $" S{audioCounts[bd.Key]}"; }

                spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock countsTB = new TextBlock() { Width = 44, Text = countString, Foreground = Brushes.DarkOliveGreen };
                spnl.Children.Add(countsTB);

                Brush brosse = Brushes.Sienna;
                if (!shownnames.Contains( bd.EnglishNameCurrent))
                {
                    TextBlock tb = new TextBlock() { Foreground = brosse, Text = bd.EnglishNameCurrent };
                    shownnames.Add(bd.EnglishNameCurrent);
                    spnl.Children.Add(tb);
                    VerticalPanel.Children.Add(spnl);
                    spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                    spnl.Children.Add(new TextBlock() { Width = 44 });
                }
                if (!shownnames.Contains( bd.UniversalEnglishName))
                {
                    TextBlock tb = new TextBlock() { Foreground = brosse, Text = bd.UniversalEnglishName };
                    shownnames.Add(bd.UniversalEnglishName);
                    spnl.Children.Add(tb);
                    VerticalPanel.Children.Add(spnl);
                    spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                    spnl.Children.Add(new TextBlock() { Width = 44 });
                }
                if (!shownnames.Contains( bd.ScientificNameCurrent ))
                {
                    TextBlock tb = new TextBlock() { Foreground = brosse, Text = bd.ScientificNameCurrent, FontStyle = FontStyles.Italic };
                    shownnames.Add(bd.ScientificNameCurrent);
                    spnl.Children.Add(tb);
                    VerticalPanel.Children.Add(spnl);
                    spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                    spnl.Children.Add(new TextBlock() { Width = 44 });
                }

                ListBoxItem itm = new ListBoxItem() { Content = VerticalPanel, Tag = k.Item2 };
                itm.MouseDoubleClick += BirdListBox_MouseDoubleClick;
                BirdListBox.Items.Add(itm);
            }
            StatsTextBlock.Text = $"Listed {stats_tot} species, of which seen {stats_moi}";
        }

        private List<Tuple<string, int>> GetListType()
        {
            int listsort = 0;
            if (ListUkEnglishMenuItem.IsChecked) { listsort = 1; }
            if (ListScientificMenuItem.IsChecked) { listsort = 2; }
            if (ListFrenchMenuItem.IsChecked) { listsort = 3; }
            if (ListInternationalMenuItem.IsChecked) { listsort = 4; }
            if (ListAllEnglishMenuItem.IsChecked) { listsort = 5; }
            if (ListAllScientificMenuItem.IsChecked) { listsort = 6; }
            List<Tuple<string, int>> keySeq;
            switch (listsort)
            {
                case 1:
                    { keySeq = Core.Instance.BirdNamesInEnglishSequence(); break; }
                case 2:
                    { keySeq = Core.Instance.BirdNamesInScientificSequence(); break; }
                case 3:
                    { keySeq = Core.Instance.BirdNamesInFrenchSequence(); break; }
                case 4:
                    { keySeq = Core.Instance.BirdNamesInInternationalSequence(); break; }
                case 5:
                    { keySeq = Core.Instance.BirdNamesInEnglishTotalSequence(); break; }
                case 6:
                    { keySeq = Core.Instance.BirdNamesInScientificTotalSequence(); break; }
                default:
                    { keySeq = Core.Instance.BirdNamesInTaxonomicSequence(); break; }
            }

            if (ListNoAudioMenuItem.IsChecked)
            {
                Dictionary<int, int> audioCounts = Core.SoundCounts();
                List<Tuple<string, int>> thinned = new List<Tuple<string, int>>();
                foreach (Tuple<string, int> wazo in keySeq)
                {
                    if (!audioCounts.ContainsKey(wazo.Item2)) { thinned.Add(wazo); }
                }
                keySeq.Clear();
                foreach (Tuple<string, int> wazo in thinned)
                {
                    keySeq.Add(wazo);
                }
            }
            if (ListNoVideoMenuItem.IsChecked)
            {
                Dictionary<int, int> videoCounts = Core.ImageCounts();
                List<Tuple<string, int>> thinned = new List<Tuple<string, int>>();
                foreach (Tuple<string, int> wazo in keySeq)
                {
                    if (!videoCounts.ContainsKey(wazo.Item2)) { thinned.Add(wazo); }
                }
                keySeq.Clear();
                foreach (Tuple<string, int> wazo in thinned)
                {
                    keySeq.Add(wazo);
                }
            }
            string filtre = FilterTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(filtre))
            {
                List<Tuple<string, int>> thinned = new List<Tuple<string, int>>();
                foreach (Tuple<string, int> wazo in keySeq)
                {
                    int pos = wazo.Item1.IndexOf(filtre, StringComparison.OrdinalIgnoreCase);
                    if (pos >= 0) { thinned.Add(wazo); }
                }
                keySeq.Clear();
                foreach (Tuple<string, int> wazo in thinned)
                {
                    keySeq.Add(wazo);
                }
            }
            int initial = AlphabetListBox.SelectedIndex;
            if (initial > 0)
            {
                char q = "-ABCDEFGHIJKLMNOPQRSTUVWXYZ"[initial];
                List<Tuple<string, int>> thinned = new List<Tuple<string, int>>();
                foreach (Tuple<string, int> wazo in keySeq)
                {
                    if (wazo.Item1[0]==q) { thinned.Add(wazo); }
                }
                keySeq.Clear();
                foreach (Tuple<string, int> wazo in thinned)
                {
                    keySeq.Add(wazo);
                }
            }
            return keySeq;
        }

        private void BirdListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SoundComboBox.Items.Clear();
            SoundComboBox.Visibility = Visibility.Hidden;
            SoundPlayButton.IsEnabled = false;
            SoundPlayButton.Visibility = Visibility.Hidden;
            SoundProgressBar.Visibility = Visibility.Hidden;
            ArrestSound();

            ListBoxItem itm = (ListBoxItem)BirdListBox.SelectedItem;
            if (itm == null)
            {
                _selectedBirdId = 0;
                BirdOccurrenceMenuItem.IsEnabled = false;
                BirdDeleteMenuItem.IsEnabled = false;
                SearchEngMenuItem.IsEnabled = false;
                SearchSciMenuItem.IsEnabled = false;
                XenoCantoSpeciesMenuItem.IsEnabled = false;

                BirdDetailsMenuItem.IsEnabled = false;
                NotesBorder.ToolTip = string.Empty;

                EnglishTextBlock.Text = string.Empty;
                ScientificTextBlock.Text = string.Empty;
                InternationalTextBlock.Text = string.Empty;
                OccurrenceTextBlock.Text = string.Empty;
                FrenchTextBlock.Text = string.Empty;
                BouTextBlock.Text = string.Empty;
                SeenCheckBox.IsChecked = null;
                FreqCheckBox.IsChecked = null;
                RareCheckBox.IsChecked = null;
                FamiliarSightCheckBox.IsChecked = null;
                FamiliarSoundCheckBox.IsChecked = null;
                BirdImage.Source = null;

                return;
            }

            _selectedBirdId = (int)itm.Tag;
            Bird bd = Core.Instance.BirdDictionary[_selectedBirdId];

            string nom = bd.EnglishNameCurrent;
            if (!string.IsNullOrWhiteSpace(bd.OtherEnglishNamesBloc)) { nom = $"{nom} (or {bd.OtherEnglishNamesBloc.Replace("#", ", ")})"; }
            EnglishTextBlock.Text = nom;

            nom = bd.ScientificNameCurrent;
            if (!string.IsNullOrWhiteSpace(bd.FormerScientificNamesBloc)) { nom = $"{nom} (formerly {bd.FormerScientificNamesBloc.Replace("#", ", ")})"; }
            ScientificTextBlock.Text = nom;

            InternationalTextBlock.Text = bd.DifferentInternationalEnglishName;
            BouTextBlock.Text = bd.BOUCode;
            OccurrenceTextBlock.Text = bd.Status;

            FrenchTextBlock.Text = bd.French;

            if (bd.FamilyKey < 1)
            {
                FamilyTextBlock.Text = string.Empty;
            }
            else
            {
                int fk = bd.FamilyKey;
                string fmy = Core.Instance.FamilyDictionary[fk].Name;
                int ok = Core.Instance.FamilyDictionary[fk].OrderKey;
                string ord = Core.Instance.OrderDictionary[ok].Name;
                string fmyord = $"{fmy} ({ord})";
                FamilyTextBlock.Text = fmyord;
            }

            SeenCheckBox.IsChecked = bd.SeenEver;
            FreqCheckBox.IsChecked = bd.SeenFrequently;
            RareCheckBox.IsChecked = bd.Rare;
            FamiliarSightCheckBox.IsChecked = bd.Recognisable;
            FamiliarSoundCheckBox.IsChecked = bd.RecognisableVoice;
            BirdOccurrenceMenuItem.IsEnabled = true;
            BirdDeleteMenuItem.IsEnabled = true;

            SearchEngMenuItem.IsEnabled = true;
            SearchSciMenuItem.IsEnabled = true;
            XenoCantoSpeciesMenuItem.IsEnabled = true;
            BirdDetailsMenuItem.IsEnabled = true;

            DisplayNotes();

            _picturePaths = Core.BirdPictureFilePaths(bd.Key);
            if (_picturePaths.Count > 0)
            {
                _picturePath = _picturePaths[0];
                DisplayPicture();
            }
            else
            {
                _picturePath = string.Empty;
                BirdImage.Source = null;
            }

            List<string> soundPaths = Core.BirdAudioFilePaths(bd.Key);
            if (soundPaths.Count > 0)
            {
                SoundComboBox.Visibility = Visibility.Visible;
                SoundPlayButton.Visibility = Visibility.Visible;
                SoundProgressBar.Visibility = Visibility.Visible;

                int au = 0;
                foreach (string s in soundPaths)
                {
                    au++;
                    TextBlock tb = new TextBlock() { Text = $"Sound {au}" };
                    ComboBoxItem it = new ComboBoxItem() { Content = tb, Tag = s };
                    SoundComboBox.Items.Add(it);
                }
                if (SoundComboBox.Items.Count > 0) { SoundComboBox.SelectedIndex = 0; }
            }
        }

        private void DisplayAnotherPicture()
        {
            if (_picturePaths.Count < 1) { return; }
            int i = _picturePaths.IndexOf(_picturePath);
            i++;
            if (i >= _picturePaths.Count) { i = 0; }
            _picturePath = _picturePaths[i];
            DisplayPicture();
        }

        private void DisplayPicture()
        {
            if (string.IsNullOrEmpty(_picturePath)) { return; }
            if (System.IO.File.Exists(_picturePath))
            {
                System.Uri uri = new Uri(_picturePath);
                BitmapImage bmi = uri.GetBitmapImage();
                BirdImage.Source = bmi;
            }
            else
            {
                BirdImage.Source = null;
            }
        }

        private void DisplayNotes()
        {
            NotesBorder.ToolTip = string.Empty;
            NotesTextBlock.Foreground = Brushes.Silver;
            NotesTextBlock.FontWeight = FontWeights.Light;
            if (_selectedBirdId > 0)
            {
                Bird bd = Core.Instance.BirdDictionary[_selectedBirdId];

                string notepath = Core.BirdTextSpecification(bd.Key);

                if (System.IO.File.Exists(notepath))
                {
                    using (System.IO.StreamReader sw = new System.IO.StreamReader(notepath))
                    {
                        string nt = sw.ReadToEnd();
                        TextBlock tbk = new TextBlock() { Text = nt, Foreground = Brushes.Chocolate, FontFamily = new FontFamily("Arial"), TextWrapping = TextWrapping.Wrap, MaxWidth = 400 };
                        Border bdr = new Border() { Child = tbk, Margin = new Thickness(0, 0, 0, 0), Padding = new Thickness(8, 4, 8, 4), Background = Brushes.Cornsilk, BorderBrush = Brushes.SaddleBrown, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4) };
                        ToolTip tip = new System.Windows.Controls.ToolTip() { Content = bdr, Background = Brushes.Transparent, BorderThickness = new Thickness(0) };
                        NotesBorder.ToolTip = tip;
                        ToolTipService.SetInitialShowDelay(NotesBorder, 10);
                        ToolTipService.SetBetweenShowDelay(NotesBorder, 0);
                        ToolTipService.SetShowDuration(NotesBorder, Int32.MaxValue);
                        NotesTextBlock.Foreground = Brushes.SaddleBrown;
                        NotesTextBlock.FontWeight = FontWeights.Medium;
                    }
                }
            }
        }

        private void TaxonomyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TaxonomyWindow tw = new TaxonomyWindow() { Owner = this };
            tw.ShowDialog();
            RefreshBirdList();
        }

        private void SearchEngButton_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.google.co.uk/search?tbm=isch&q=" + Core.Instance.BirdDictionary[_selectedBirdId].EnglishNameCurrent);
        }

        private void SearchSciButton_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.google.co.uk/search?tbm=isch&q=" + Core.Instance.BirdDictionary[_selectedBirdId].ScientificNameCurrent);
        }

        private void BirdDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditBirdDetails();
        }

        private void EditBirdDetails()
        {
            BirdWindow bw = new BirdWindow(_selectedBirdId) { Owner = this };
            bool? Q = bw.ShowDialog();
            if (Q.HasValue && Q.Value)
            {
                Core.Instance.BirdDictionary[_selectedBirdId].Mirror(bw.BirdData);
                int si = BirdListBox.SelectedIndex;
                RefreshBirdList();
                BirdListBox.SelectedIndex = si;
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BirdListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedBirdId > 0) { EditBirdDetails(); }
        }

        private void BirdCreateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BirdWindow bw = new BirdWindow(0) { Owner = this };
            bool? Q = bw.ShowDialog();
            if (Q.HasValue && Q.Value)
            {
                Core.Instance.CreateNewBird(bw.BirdData);
                RefreshBirdList();
            }
        }

        private void BirdDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Bird bd = Core.Instance.BirdDictionary[_selectedBirdId];
            MessageBoxResult ans = MessageBox.Show($"Delete {bd.EnglishNameCurrent}?", bd.ScientificNameCurrent, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ans == MessageBoxResult.No) { return; }
            Core.Instance.DeleteBird(bd.Key);
            RefreshBirdList();
        }

        private void BtoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("http://bto.org");
        }

        private void BtoListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://bto.org/about-birds/birdfacts/british-list");
        }

        private void BirdOccurrenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Bird bd = Core.Instance.BirdDictionary[_selectedBirdId];
            MessageBox.Show(Core.OccurrenceRubric(bd.BOUCode), $"Occurrence of {bd.ScientificNameCurrent}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ListChoiceMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!_ready) { return; }
            MenuItem item = sender as MenuItem;
            if ((sender != ListNoAudioMenuItem) && (item != ListNoVideoMenuItem))
            {
                MenuItem other = ListUkEnglishMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListFrenchMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListAllEnglishMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListInternationalMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListScientificMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListAllScientificMenuItem;
                if (other != item) { other.IsChecked = false; }
                other = ListTaxonomicMenuItem;
                if (other != item) { other.IsChecked = false; }
            }
            RefreshBirdList();
        }

        private void AviBirdMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("http://www.avibirds.com/");
        }

        private void InternetBirdCollectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.macaulaylibrary.org/the-internet-bird-collection-the-macaulay-library/");
        }

        private void XenoCantoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("http://www.xeno-canto.org/");
        }

        private void BouMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.bou.org.uk/");
        }

        private void BSpotMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.birdspot.co.uk/");
        }

        private void OiseauxNetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.oiseaux.net/");
        }

        private void BouListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.StartProcess("https://www.bou.org.uk/british-list/");
        }

        private void XenoCantoSpeciesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Bird bd = Core.Instance.BirdDictionary[_selectedBirdId];
            string link = bd.ScientificNameCurrent.Replace(' ', '-');
            Core.StartProcess("http://www.xeno-canto.org/species/" + link);
        }

        private void BouListPdfMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string spec = "British-List*.pdf";
            string[] foundfiles = System.IO.Directory.GetFiles(Jbh.AppManager.DataPath, spec);
            if (foundfiles.Count() == 1)
            {
                Core.StartProcess(foundfiles[0]);
            }
            else
            {
                MessageBox.Show($"There are either no files or more than one file matching the specification\n\n{spec}", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                Core.StartProcess(Jbh.AppManager.DataPath);
            }
        }

        private void TaxonomicSequenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SequenceWindow sw = new SequenceWindow() { Owner = this };
            sw.ShowDialog();
            RefreshBirdList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.Instance.SaveData();
        }

        private void NonCongruentNamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListingWindow w = new ListingWindow("international") { Owner = this };
            w.ShowDialog();
        }

        private void NonABirdsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListingWindow w = new ListingWindow("non-A") { Owner = this };
            w.ShowDialog();
        }

        private void BritishListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListingWindow w = new ListingWindow("british") { Owner = this };
            w.ShowDialog();
        }

        private void PersonalListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListingWindow w = new ListingWindow("personal") { Owner = this };
            w.ShowDialog();
        }

        private void SightingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SightingsWindow w = new SightingsWindow() { Owner = this };
            w.ShowDialog();
        }

        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            StatsWindow w = new StatsWindow() { Owner = this };
            w.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CompleteIndexMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListingWindow w = new ListingWindow("index") { Owner = this };
            w.ShowDialog();
        }

        private void QuizPhotoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            QuizImageWindow w = new QuizImageWindow() { Owner = this };
            w.ShowDialog();
        }

        private void BirdImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DisplayAnotherPicture();
        }

        private void QuizAudioMenuItem_Click(object sender, RoutedEventArgs e)
        {
            QuizAudioWindow w = new QuizAudioWindow() { Owner = this };
            w.ShowDialog();
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshBirdList();
        }

        private void ClearFilterButton_Click_1(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Clear();
            RefreshBirdList();
            FilterTextBox.Focus();
        }

        private void SoundsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SoundPlayButton.IsEnabled = SoundComboBox.SelectedIndex >= 0;
        }

        private void SoundPlayButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem it = (ComboBoxItem)SoundComboBox.SelectedItem;
            if (it != null)
            {
                string path = it.Tag.ToString();
                _player.Open(new Uri(path));
                _tim.Start();
                _player.Position = TimeSpan.Zero;
                _player.Play();
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source == null)
            {
                SoundProgressBar.Value = 0;
            }
            else
            {
                if (_player.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan el = _player.Position;
                    TimeSpan dr = _player.NaturalDuration.TimeSpan;
                    TimeSpan rm = dr - el;

                    double prop = 100 * el.TotalMilliseconds / dr.TotalMilliseconds;

                    SoundProgressBar.Value = prop;
                    if (el >= dr) { ArrestSound(); }
                }
            }
        }

        private void ArrestSound()
        {
            _tim.Stop();
            _player.Stop();
            SoundProgressBar.Value = 0;
        }

        private void AlphabetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBirdList();
        }
    }
}