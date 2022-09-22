using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Birds;

public partial class SightingsWindow : Window
{
    private struct Wazo:IComparable<Wazo>
        {
            private string _engName;
            private int _birdKey;
            private bool _commonasmuck;
            public Wazo(string nom, int bird, bool freq)
            {
                _engName = nom;
                _birdKey = bird;
                _commonasmuck = freq;
            }

            public string EngName { get => _engName; }
            public int BirdKey { get => _birdKey; }
            public bool CommonAsMuck { get => _commonasmuck; }

            int IComparable<Wazo>.CompareTo(Wazo other)
            {
                return _engName.CompareTo(other._engName);
            }
        }

        private List<Wazo> _birds = new List<Wazo>();
        private List<int> _dolValues = new List<int>();
        
        public SightingsWindow()
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

        private void BuildLists()
        {
            List<int> birdIds = new List<int>();
            _birds.Clear();
            _dolValues.Clear();
            Core.Instance.SightingList.Sort();
            foreach (Sighting sght in Core.Instance.SightingList)
            {
                if (!_dolValues.Contains(sght.ViewDayOfLife)) { _dolValues.Add(sght.ViewDayOfLife); }
                if (!birdIds.Contains(sght.BirdId))
                {
                    birdIds.Add(sght.BirdId);
                    Bird b = Core.Instance.BirdDictionary[sght.BirdId];
                    Wazo w = new Wazo(b.EnglishNameCurrent, sght.BirdId, b.SeenFrequently);
                    _birds.Add(w);
                }
            }
            _birds.Sort();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RubricTextBlock.Text = "Click on a date or a bird in the first two columns to see the list of sightings.\n\nBirds are shown in red if the species has been flagged as too frequently seen to warrant recording of individual sightings.\n\nBy default rare birds are not listed. If listed, they are shown in blue.\n\nDouble-click an entry in the list of sightings to delete that sighting or to toggle its 'First' status .\n\nSelect a bird and a date below to add a new sighting.";
            NewSightingDateBox.DateValue = DateTime.Today;
            RefreshBirdList(false);
            RefreshSightingLists();
        }

        private void RefreshSightingLists()
        {
            ItemsCaptionTextBlock.Text = "Sightings";
            SightingsListBox.Items.Clear();
            BuildLists();
            DatesListBox.Items.Clear();
            foreach (int q in _dolValues)
            {
                DateTime d = Sighting.DateFromDayOfLife(q);
                TextBlock tb = new TextBlock() { Text = d.ToString("ddd d MMM yyyy") };
                ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag = q };
                DatesListBox.Items.Add(lbi);
            }
            SpeciesListBox.Items.Clear();
            foreach (Wazo w in _birds)
            {
                TextBlock tb = new TextBlock() { Text = w.EngName };
                if (w.CommonAsMuck) { tb.Foreground = Brushes.OrangeRed; }
                if (!Core.Instance.BirdDictionary[w.BirdKey].SeenEver) { tb.Text = w.EngName + " ! not listed as ever seen"; tb.FontWeight = FontWeights.Bold; }
                
                ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag = w.BirdKey };
                SpeciesListBox.Items.Add(lbi);
            }
        }

        private void RefreshBirdList(bool withRarities)
        {
            List<int> birdkeys = Core.Instance.BirdKeysInEnglishSequence();
            AllBirdsListBox.Items.Clear();
            foreach (int k in birdkeys)
            {
                Bird b = Core.Instance.BirdDictionary[k];
                if (withRarities || !b.Rare)
                {
                    TextBlock tb = new TextBlock() { Text = b.EnglishNameCurrent };
                    if (b.SeenFrequently) { tb.Foreground = Brushes.OrangeRed; }
                    if (b.Rare) { tb.Foreground = Brushes.Blue; }
                    ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag = k };
                    AllBirdsListBox.Items.Add(lbi);
                }
            }
        }

        private void DatesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem itm =(ListBoxItem) DatesListBox.SelectedItem;
            if (itm == null) { return; }
            int q = (int)itm.Tag;
            ItemsCaptionTextBlock.Text = "Sightings on " + Sighting.DateFromDayOfLife(q).ToShortDateString();
            SightingsListBox.Items.Clear();
            for(int n=0;n<Core.Instance.SightingList.Count;n++)
            {
                    Sighting s = Core.Instance.SightingList[n];
                if (s.ViewDayOfLife.Equals(q))
                {
                    Bird b = Core.Instance.BirdDictionary[s.BirdId];
                    TextBlock tb = new TextBlock() { Text = b.EnglishNameCurrent };
                    if (s.FirstSighting)
                    { tb.FontWeight = FontWeights.Bold; tb.Foreground = Brushes.BlueViolet;tb.Text += " FIRST!"; }
                    else
                    { if (b.SeenFrequently) { tb.Foreground = Brushes.OrangeRed; } }
                    
                    ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag=n };
                    SightingsListBox.Items.Add(lbi);
                }
            }
        }

        private void SpeciesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem itm = (ListBoxItem)SpeciesListBox.SelectedItem;
            if (itm == null) { return; }
            int q = (int)itm.Tag;
            ItemsCaptionTextBlock.Text ="Sightings of "+ Core.Instance.BirdDictionary[q].EnglishNameCurrent;
            SightingsListBox.Items.Clear();
            for (int n = 0; n < Core.Instance.SightingList.Count; n++)
            {
                Sighting s = Core.Instance.SightingList[n];
                if (s.BirdId.Equals(q))
                {
                    TextBlock tb = new TextBlock() { Text =s.ViewDate.ToString("ddd d MMM yyyy") };
                    if (s.FirstSighting) { tb.FontWeight = FontWeights.Bold; tb.Foreground = Brushes.BlueViolet; tb.Text += " FIRST!"; }
                    ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag=n };
                    SightingsListBox.Items.Add(lbi);
                }
            }
        }

        private void ListFirstSightings()
        {
            ItemsCaptionTextBlock.Text = "First sightings";
            SightingsListBox.Items.Clear();
            for (int n = 0; n < Core.Instance.SightingList.Count; n++)
            {
                Sighting s = Core.Instance.SightingList[n];
                if (s.FirstSighting)
                {
                    TextBlock tb = new TextBlock();
                    tb.Inlines.Clear();
                    Run r = new Run() { Text = s.ViewDate.ToString("yyyy, dd MMM "), FontWeight = FontWeights.Medium, Foreground = Brushes.CornflowerBlue };
                    tb.Inlines.Add(r);
                    Run b = new Run() { Text = Core.Instance.BirdDictionary[s.BirdId].EnglishNameCurrent, FontWeight=FontWeights.Bold, Foreground=Brushes.BlueViolet };
                    tb.Inlines.Add(b);
                    ListBoxItem lbi = new ListBoxItem() { Content = tb, Tag = n };
                    SightingsListBox.Items.Add(lbi);
                }
            }
        }

        private void SightingsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem itm = (ListBoxItem)SightingsListBox.SelectedItem;
            if (itm == null) { return; }
            int q = (int)itm.Tag;
            Sighting s = Core.Instance.SightingList[q];
            Bird b = Core.Instance.BirdDictionary[s.BirdId];
            string msg = $"Sighting of {b.EnglishNameCurrent} on {s.ViewDate.ToShortDateString()}\n\nDelete this?";
            MessageBoxResult answ1 = MessageBox.Show(msg, "Delete sighting?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answ1.Equals(MessageBoxResult.Cancel)) { return; }
            if (answ1.Equals(MessageBoxResult.Yes))
            {
                Core.Instance.SightingList.RemoveAt(q);
            }
            else
            {
                if (s.FirstSighting)
                {
                    msg = $"Sighting of {b.EnglishNameCurrent} on {s.ViewDate.ToShortDateString()}\n\nUnmark as a first sighting?";
                }
                else
                {
                    msg = $"Sighting of {b.EnglishNameCurrent} on {s.ViewDate.ToShortDateString()}\n\nMark as a first sighting?";
                }
                MessageBoxResult answ2 = MessageBox.Show(msg, "Toggle whether marked as a first sighting?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answ2.Equals(MessageBoxResult.No)) { return; }
                Core.Instance.SightingList[q].FirstSighting =!Core.Instance.SightingList[q].FirstSighting;
            }
            RefreshSightingLists();
        }

        private void RareCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            bool with = (RareCheckBox.IsChecked.HasValue && RareCheckBox.IsChecked.Value);
            RefreshBirdList(with);
        }

        private void AddSightingButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem itm = (ListBoxItem)AllBirdsListBox.SelectedItem;
            if (itm == null)
            {
                MessageBox.Show("Failed: no species selected", "Add sighting", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            int q = (int)itm.Tag;
            DateTime? sdate = NewSightingDateBox.DateValue;
            bool first = false;
            if (FirstSightingCheckBox.IsChecked.HasValue && FirstSightingCheckBox.IsChecked.Value) { first = true; }

            if (sdate.HasValue)
            {
                Sighting s = new Sighting(sdate.Value,q ,first);
                MessageBoxResult answ = MessageBoxResult.Cancel;
                if (first)
                {
                    answ = MessageBox.Show($"Add a FIRST sighting of {Core.Instance.BirdDictionary[s.BirdId].EnglishNameCurrent} on {s.ViewDate.ToLongDateString()}?", "New sighting", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                }
                else
                {
                    answ = MessageBox.Show($"Add a sighting of {Core.Instance.BirdDictionary[s.BirdId].EnglishNameCurrent} on {s.ViewDate.ToLongDateString()}?", "New sighting", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                }
                FirstSightingCheckBox.IsChecked = false;
                if (answ== MessageBoxResult.Cancel) { return; }
                Core.Instance.SightingList.Add(s);
                Core.Instance.SightingList.Sort();
                RefreshSightingLists();
            }
            else
            {
                MessageBox.Show("Failed: cannot interpret date", "Add sighting", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (NewSightingDateBox.DateValue.HasValue)
            {
                DateTime d = NewSightingDateBox.DateValue.Value;
                d = d.AddDays(-1);
                NewSightingDateBox.DateValue = d;
            }
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (NewSightingDateBox.DateValue.HasValue)
            {
                DateTime d = NewSightingDateBox.DateValue.Value;
                d = d.AddDays(1);
                NewSightingDateBox.DateValue = d;
            }
        }

        private void FirstsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DatesDockPanel.IsEnabled = false;
            SpeciesDockPanel.IsEnabled = false;
            DatesDockPanel.Opacity = 0.3;
            SpeciesDockPanel.Opacity = 0.3;
            ListFirstSightings();
        }

        private void FirstsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DatesDockPanel.IsEnabled = true;
            SpeciesDockPanel.IsEnabled = true;
            DatesDockPanel.Opacity = 1;
            SpeciesDockPanel.Opacity = 1;
            SightingsListBox.Items.Clear();
        }
}