using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Birds;

public partial class StatsWindow : Window
{
    private int _birds;
        private int _rareBirds;
        private int _commonSights;
        private int _catABirds;

        private  List<int> _rareSightsList = new List<int>();
      private  List<int> _nonASightsList = new List<int>();

        public StatsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            foreach (int k in Core.Instance.BirdDictionary.Keys)
            {
                Bird b = Core.Instance.BirdDictionary[k];
                _birds++;
                if (b.Rare)
                {
                    _rareBirds++;
                    if (b.SeenEver) { _rareSightsList.Add(k); }
                }
                else
                {
                    if (b.SeenEver) { _commonSights++; }
                }
                if (b.BOUCode.Contains("A"))
                {
                    _catABirds++;
                }
                else
                {
                    if (b.SeenEver) {_nonASightsList.Add(k); }
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            int seenBirds = _commonSights + _rareSightsList.Count;
            int commonBirds = _birds - _rareBirds;
            int seenCatA = seenBirds - _nonASightsList.Count;
            int nonABirds = _birds - _catABirds;
            
            TotalTextBlock.Text = _birds.ToString();
            RegularTextBlock.Text = commonBirds.ToString();
            RareTextBlock.Text = _rareBirds.ToString();
            CatATextBlock.Text = _catABirds.ToString();
            NotCatATextBlock.Text = nonABirds.ToString();

            SeenTextBlock.Text = seenBirds.ToString();
            SeenRegularTextBlock.Text = _commonSights.ToString();
            SeenRareTextBlock.Text = _rareSightsList.Count.ToString();
            SeenCatATextBlock.Text = seenCatA.ToString();
            SeenNotCatATextBlock.Text = _nonASightsList.Count.ToString();

            TotalProgressBar.Value = seenBirds / (double)_birds;
            RegularProgressBar.Value = _commonSights / (double)commonBirds;
            RareProgressBar.Value = _rareSightsList.Count / (double)_rareBirds;
            CatAProgressBar.Value = seenCatA / (double)_catABirds;
            NotCatAProgressBar.Value = _nonASightsList.Count / (double)nonABirds;

            foreach(int k in _rareSightsList)
            {
                TextBlock tb = new TextBlock() { Text = Core.Instance.BirdDictionary[k].EnglishNameCurrent, Foreground=Brushes.DarkSlateGray };
                SeenRareListBox.Items.Add(new ListBoxItem() { Content = tb, IsHitTestVisible = false });
            }
            foreach (int k in _nonASightsList)
            {
                TextBlock tb = new TextBlock() { Text = Core.Instance.BirdDictionary[k].EnglishNameCurrent, Foreground = Brushes.DarkSlateGray };
                SeenNonAListBox.Items.Add(new ListBoxItem() { Content = tb , IsHitTestVisible=false});
            }
        }
}