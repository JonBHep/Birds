using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Birds;

public partial class QuizChecklistWindow : Window
{
    private bool _audioMode = false;
        public QuizChecklistWindow(bool audio)
        {
            InitializeComponent();
            _audioMode = audio;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_audioMode)
            {
                _pcounts = Core.Instance.BirdAudioCounts();
            }
            else
            {
                _pcounts = Core.Instance.BirdPictureCounts();
            }
            Populate();
        }

        private Dictionary<int, int> _pcounts;

        private void Populate()
        {
            QuizBirdListBox.Items.Clear();
            FriendsListBox.Items.Clear();
            List<int> avis = Core.Instance.BirdKeysInTaxonomicSequence();
            List<string> amis = new List<string>();

            foreach (int a in avis)
            {
                Bird b = Core.Instance.BirdDictionary[a];
                int exemplars = 0;
                bool familiar = (_audioMode) ? b.RecognisableVoice : b.Recognisable;
                if (_pcounts.ContainsKey(a)) { exemplars = _pcounts[a]; }
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };
                CheckBox cf = new CheckBox() { IsThreeState = false, IsChecked = familiar, Width = 100, Tag = b, Foreground = Brushes.Black };
                CheckBox cq = new CheckBox() { IsThreeState = false, IsChecked = b.BarFromImageQuiz, Content = $"{b.EnglishNameCurrent} ({exemplars})", Tag = b, Foreground = Brushes.Black };
                cq.Checked += BarringCheckBoxTick;
                cq.Unchecked += BarringCheckBoxTick;
                cf.Checked += FamiliarCheckBoxTick;
                cf.Unchecked += FamiliarCheckBoxTick;
                if (b.Rare) { cq.Foreground = Brushes.Red; }
                if (exemplars < 1) { cq.Foreground = Brushes.Magenta; }
                sp.Children.Add(cf);
                sp.Children.Add(cq);
                ListBoxItem d = new ListBoxItem() { Content = sp };
                QuizBirdListBox.Items.Add(d);

                if (familiar)
                {
                    amis.Add(b.EnglishNameCurrent);
                }
            }
            amis.Sort();
            foreach (string g in amis)
            {
                TextBlock tb = new TextBlock() { Text = g };
                ListBoxItem li = new ListBoxItem() { Content = tb };
                FriendsListBox.Items.Add(li);
            }
            DisplayStats();
        }

        private void BarringCheckBoxTick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Bird b = (Bird)cb.Tag;
            if (_audioMode) { b.BarFromAudioQuiz = cb.IsChecked.Value; } else { b.BarFromImageQuiz = cb.IsChecked.Value; }
            DisplayStats();
        }

        private void FamiliarCheckBoxTick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Bird b = (Bird)cb.Tag;
            if (_audioMode) { b.RecognisableVoice = cb.IsChecked.Value; } else { b.Recognisable = cb.IsChecked.Value; }
            DisplayStats();
        }

        private void DisplayStats()
        {
            Tuple<int, int, int, int, int, int> figs;
            if (_audioMode)
            {
                figs = Core.Instance.AudioQuizNumbers();
            }
            else
            {
                figs = Core.Instance.ImageQuizNumbers();
            }
            IncludedTextBlock.Text = figs.Item1.ToString(System.Globalization.CultureInfo.CurrentCulture);
            IncludedRareTextBlock.Text = figs.Item2.ToString(System.Globalization.CultureInfo.CurrentCulture);
            IncludedRecoTextBlock.Text = figs.Item3.ToString(System.Globalization.CultureInfo.CurrentCulture);
            ExcludedTextBlock.Text = figs.Item4.ToString(System.Globalization.CultureInfo.CurrentCulture);
            ExcludedRareTextBlock.Text = figs.Item5.ToString(System.Globalization.CultureInfo.CurrentCulture);
            ExcludedRecoTextBlock.Text = figs.Item6.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        private void TickRareButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Bird b in Core.Instance.BirdDictionary.Values)
            { if (b.Rare) { if (_audioMode) { b.BarFromAudioQuiz = true; } else { b.BarFromImageQuiz = true; } } }
            Populate();
        }

        private void TickRecognisableButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Bird b in Core.Instance.BirdDictionary.Values)
            {
                if (_audioMode)
                {
                    if (b.RecognisableVoice) { b.BarFromAudioQuiz = true; }
                }
                else
                {
                    if (b.Recognisable) { b.BarFromImageQuiz = true; }
                }
            }

            Populate();
        }

        private void UntickAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_audioMode)
            {
                foreach (Bird b in Core.Instance.BirdDictionary.Values) { b.BarFromAudioQuiz = false; }
            }
            else
            {
                foreach (Bird b in Core.Instance.BirdDictionary.Values) { b.BarFromImageQuiz = false; }
            }
            Populate();
        }
}