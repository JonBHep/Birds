using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Birds;

public partial class SequenceWindow : Window
{
    private List<int> _seqKeySeq = Core.Instance.BirdKeysInTaxonomicSequence();
        private List<int> _engKeySeq = Core.Instance.BirdKeysInEnglishSequence();

        private int _selectedBirdKey;
        //private int _lastClicked = -1;
        private int _lastDoubleClicked = -1;
        private List<Rectangle> _marker = new List<Rectangle>();

        public SequenceWindow()
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
            for (int a = 0; a < 26; a++)
            {
                string i = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(a, 1);
                TextBlock tbkInitial = new TextBlock() { Foreground = Brushes.DarkBlue, Text = i, Width = 30, TextAlignment = TextAlignment.Center };
                ListBoxItem itm = new ListBoxItem() { Content = tbkInitial, Tag = i };
                AlphabetListBox.Items.Add(itm);
            }

            DefineMarkers();
            RefreshSequenceList();

            foreach (int k in _engKeySeq)
            {
                Bird bd = Core.Instance.BirdDictionary[k];
                TextBlock tbkName = new TextBlock() { Foreground = Brushes.DarkBlue, Text = bd.EnglishNameCurrent };
                ListBoxItem itm = new ListBoxItem() { Content = tbkName, Tag = k };
                EnglishListBox.Items.Add(itm);
            }

            RefreshGenera("");

            _selectedBirdKey = -1;
        }

        private void DefineMarkers()
        {
            for (int x=0; x<_seqKeySeq.Count;x++)
            {
                Rectangle r = new Rectangle() { Width = 16, Height = 8 , Fill=Brushes.Gray};
                _marker.Add(r);
            }
        }

        private void RefreshGenera(string initial)
        {
            GeneraListBox.Items.Clear();
            foreach (string genus in Core.Instance.Genera())
            {
                if ((string.IsNullOrEmpty(initial))||( genus.StartsWith(initial)))
                {
                    TextBlock tbkGenus = new TextBlock() { Foreground = Brushes.DarkBlue, Text = genus };
                    ListBoxItem itm = new ListBoxItem() { Content = tbkGenus, Tag = genus };
                    GeneraListBox.Items.Add(itm);
                }
            }
            _selectedBirdKey = -1;
        }

        private void EnglishListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem itm = (ListBoxItem)EnglishListBox.SelectedItem;
            if (itm == null) { return; }
            AlphabetListBox.SelectedIndex = -1;
            RefreshGenera("");
 
            int targetKey = (int)itm.Tag;
            string g = Core.Instance.BirdDictionary[targetKey].Binomial.Item1;
            int targetIndex = -1;
            for (int i = 0; i < GeneraListBox.Items.Count; i++)
            {
                ListBoxItem it = (ListBoxItem)GeneraListBox.Items[i];
                if (it.Tag.ToString().Equals(g)) { targetIndex = i; break; }
            }
            if (GeneraListBox.SelectedIndex != targetIndex)
            {
                int qq = GeneraListBox.Items.Count;
                GeneraListBox.SelectedIndex = targetIndex;
                GeneraListBox.ScrollIntoView(GeneraListBox.Items[targetIndex]);
            }
        }

        private void GeneraListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeciesListBox.Items.Clear();

            int targetKey = -1;
            ListBoxItem itmE = (ListBoxItem)EnglishListBox.SelectedItem;
            if (itmE != null) { targetKey = (int)itmE.Tag; }

            ListBoxItem itm = (ListBoxItem)GeneraListBox.SelectedItem;
            if (itm == null) { return; }
            string genus = itm.Tag.ToString();

            List<Tuple<string, int>> speciesList = Core.Instance.Species(genus);
            int targetIndex = -1;
            foreach (Tuple<string, int> oi in speciesList)
            {
                TextBlock sBlock = new TextBlock() { Foreground = Brushes.DarkBlue, Text = oi.Item1 };
                ListBoxItem sItem = new ListBoxItem() { Content = sBlock, Tag = oi.Item2 };
                SpeciesListBox.Items.Add(sItem);
                if (targetKey.Equals(oi.Item2)) { targetIndex = SpeciesListBox.Items.Count - 1; }
            }
    
            if (targetIndex >= 0)
            {
                SpeciesListBox.SelectedIndex = targetIndex;
                SpeciesListBox.ScrollIntoView(SpeciesListBox.Items[targetIndex]);
            }
        }

        private void SequenceListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_selectedBirdKey < 0) { return; }
            _lastDoubleClicked = SequenceListBox.SelectedIndex;
            ListBoxItem itm = (ListBoxItem)SequenceListBox.SelectedItem;
            if (itm == null) { return; }
            int targetKey = (int)itm.Tag;
            int targetPosn = Core.Instance.BirdDictionary[targetKey].TaxonomicSequenceIndex;

            foreach (Bird av in Core.Instance.BirdDictionary.Values)
            {
                if (av.TaxonomicSequenceIndex >= targetPosn) { av.TaxonomicSequenceIndex++; }
            }
            if (Core.Instance.BirdDictionary[_selectedBirdKey].TaxonomicSequenceIndex < targetPosn) { _lastDoubleClicked--; } // to ensure highlight is in correct position
            Core.Instance.BirdDictionary[_selectedBirdKey].TaxonomicSequenceIndex =targetPosn;
            _seqKeySeq = Core.Instance.BirdKeysInTaxonomicSequence();
            RefreshSequenceList();
        }

        private void RefreshSequenceList()
        {
            for(int z = 0; z < SequenceListBox.Items.Count; z++) // clear stackpanels so that the markers no longer have a 'parent'
            {
                ListBoxItem i = (ListBoxItem)SequenceListBox.Items[z];
                StackPanel s = (StackPanel)i.Content;
                s.Children.Clear();
            }
            SequenceListBox.Items.Clear();
            int p = -1;
            foreach (int k in _seqKeySeq)
            {
                p++;
                Bird bd = Core.Instance.BirdDictionary[k];
                StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal };
                pnl.Children.Add(_marker[p]);
                _marker[p].Fill = Brushes.Silver;
                TextBlock bloc = new TextBlock();
                Run rn = new Run() { Foreground = Brushes.DimGray, Text = $" {bd.TaxonomicSequenceIndex} " };
                bloc.Inlines.Add(rn);
                rn = new Run() { Foreground = Brushes.DarkBlue, Text= $"{bd.ScientificNameCurrent} " };
                bloc.Inlines.Add(rn);
                rn = new Run() { Foreground = Brushes.CornflowerBlue, Text = $"({bd.EnglishNameCurrent}) {bd.BOUCode}" };
                bloc.Inlines.Add(rn);
                if (bd.Rare)
                {
                    rn = new Run() { Foreground = Brushes.CornflowerBlue, Text = " (rare)" };
                    bloc.Inlines.Add(rn);
                }
                pnl.Children.Add(bloc);
                             if (p == _lastDoubleClicked) { _marker[p].Fill = Brushes.Lime; }
                ListBoxItem it = new ListBoxItem() { Content= pnl, Tag = k };
                SequenceListBox.Items.Add(it);
            }
        }

        private void SpeciesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBirdKey = -1;
            if (SpeciesListBox.SelectedIndex < 0) { return; }
            ListBoxItem it= (ListBoxItem)SpeciesListBox.SelectedItem;
            _selectedBirdKey = (int)it.Tag;
            int targetIndex = -1;
            for (int i = 0; i < _engKeySeq.Count; i++)
            {
                if (_engKeySeq[i] == _selectedBirdKey) { targetIndex = i; break; }
            }
            if (EnglishListBox.SelectedIndex != targetIndex)
            {
                EnglishListBox.SelectedIndex = targetIndex;
                EnglishListBox.ScrollIntoView(EnglishListBox.Items[targetIndex]);
            }

            Bird bd = Core.Instance.BirdDictionary[_selectedBirdKey];
            RubricEnglishTextBlock.Text = bd.EnglishNameCurrent;
            RubricScientificTextBlock.Text = bd.ScientificNameCurrent;
        }

        private void AlphabetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlphabetListBox.SelectedIndex < 0) { return; }
            ListBoxItem item = AlphabetListBox.SelectedItem as ListBoxItem;
            string initial = item.Tag.ToString();
            RefreshGenera(initial);
        }

        private void SequenceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SequenceListBox.SelectedIndex >= 0) { _marker[SequenceListBox.SelectedIndex].Fill = Brushes.Gold; } 
        }
}