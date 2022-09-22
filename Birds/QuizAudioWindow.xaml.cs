using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Birds;

public partial class QuizAudioWindow : Window
{
    public QuizAudioWindow()
        {
            InitializeComponent();
        }
        private int _birdKey;
        private int _counter;
        private int _population;
        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _tim = new DispatcherTimer();

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Instance.ResetImageQuizExclusions();
            DisplayStats();
        }

        private void DisplayStats()
        {
            Tuple<int, int, int, int, int, int> figs = Core.Instance.AudioQuizNumbers();
            IncludedTextBlock.Text = figs.Item1.ToString();
            IncludedRareTextBlock.Text = figs.Item2.ToString();
            IncludedRecoTextBlock.Text = figs.Item3.ToString();
            ExcludedTextBlock.Text = figs.Item4.ToString();
            ExcludedRareTextBlock.Text = figs.Item5.ToString();
            ExcludedRecoTextBlock.Text = figs.Item6.ToString();
            _population = figs.Item1;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            QuizChecklistWindow w = new QuizChecklistWindow(true) { Owner = this };
            w.ShowDialog();
            DisplayStats();
        }

        private void AdvanceButton_Click(object sender, RoutedEventArgs e)
        {
            string what = AdvanceButton.Content.ToString();
            switch (what)
            {
                case "Next":
                    {
                        _counter++;
                        float pc = 100 * (_counter / (float)_population);
                        CountTextBlock.Text = $"{ _counter} = {(int)pc}%";
                        if (ExclusionCheckBox.IsChecked.HasValue && ExclusionCheckBox.IsChecked.Value) { Core.Instance.BirdDictionary[_birdKey].BarFromAudioQuiz = true; DisplayStats(); }
                        ExclusionCheckBox.Visibility = Visibility.Hidden;
                        SeenCheckBox.Visibility = Visibility.Hidden;
                        ArrestButton.IsEnabled = true;
                        _birdKey = Core.Instance.RandomBirdWithAudioKey();

                        Bird oie = Core.Instance.BirdDictionary[_birdKey];
                        string audiopath = Core.Instance.RandomBirdAudioClip(oie.Key);
                        if (string.IsNullOrWhiteSpace(audiopath))
                        {
                            _player.Stop();
                        }
                        else
                        {
                            if (System.IO.File.Exists(audiopath))
                            {
                                _player.Open(new Uri(audiopath));
                                AdvanceButton.IsEnabled = false;
                                
                                _tim.Start();

                                _player.Position = TimeSpan.Zero;
                                _player.Play();
                            }
                            else
                            {
                                _player.Stop();
                            }
                        }

                        AdvanceButton.Content = "Reveal";
                        NameETextBlock.Text = string.Empty;
                        NameSTextBlock.Text = string.Empty;
                        break;
                    }
                case "Reveal":
                    {
                        Bird oie = Core.Instance.BirdDictionary[_birdKey];
                        string rareString = (oie.Rare) ? " (rare)" : string.Empty;
                        Brush pinceau = (oie.Rare) ? Brushes.Red : Brushes.ForestGreen;
                        NameETextBlock.Text = oie.EnglishNameCurrent;
                        NameETextBlock.Foreground = pinceau;
                        NameSTextBlock.Text = $"{oie.ScientificNameCurrent} {rareString}";
                        NameSTextBlock.Foreground = pinceau;

                        ExclusionCheckBox.Visibility = Visibility.Visible;
                        ExclusionCheckBox.IsChecked = false;
                        SeenCheckBox.Visibility = Visibility.Visible;
                        SeenCheckBox.IsChecked = oie.SeenEver;
                        AdvanceButton.Content = "Next";
                        break;
                    }
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            if (_player.Source == null)
            {
                PlayProgressBar.Value = 0;
            }
            else
            {
                if (_player.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan el = _player.Position;
                    TimeSpan dr = _player.NaturalDuration.TimeSpan;
                    TimeSpan rm = dr - el;

                    double prop = 100 * el.TotalMilliseconds / dr.TotalMilliseconds;
                   
                    PlayProgressBar.Value = prop;
                    if (el >= dr) { _tim.Stop(); _player.Stop(); AdvanceButton.IsEnabled = true; ArrestButton.IsEnabled = false; }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _player.Stop();
            _tim.Stop();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ExclusionCheckBox.Visibility = Visibility.Hidden;
            SeenCheckBox.Visibility = Visibility.Hidden;
            ArrestButton.IsEnabled = false;
            DisplayStats();
            PlayProgressBar.Maximum = 100;
            _tim.Interval = TimeSpan.FromSeconds(1);
            _tim.Tick += Timer_Tick;
        }

        private void ArrestButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
            _tim.Stop();
            AdvanceButton.IsEnabled = true;
            ArrestButton.IsEnabled = false;
        }
}