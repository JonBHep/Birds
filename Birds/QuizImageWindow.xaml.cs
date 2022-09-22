using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Birds;

public partial class QuizImageWindow : Window
{
    public QuizImageWindow()
        {
            InitializeComponent();
        }
        private int _birdKey;
        private int _counter;
        private int _population;
        private void AdvanceButton_Click(object sender, RoutedEventArgs e)
        {
            string what = AdvanceButton.Content.ToString();
            switch (what)
            {
                case "Next":
                    {
                        _counter++;
                        float pc =100*( _counter / (float)_population);
                        CountTextBlock.Text =$"{ _counter} = {(int)pc}%";
                        if (ExclusionCheckBox.IsChecked.HasValue && ExclusionCheckBox.IsChecked.Value) { Core.Instance.BirdDictionary[_birdKey].BarFromImageQuiz = true; DisplayStats(); }
                        ExclusionCheckBox.Visibility = Visibility.Hidden;
                        SeenCheckBox.Visibility = Visibility.Hidden;

                        _birdKey = Core.Instance.RandomBirdWithImageKey();
                        
                        Bird oie = Core.Instance.BirdDictionary[_birdKey];
                        string imagepath = Core.Instance.RandomBirdPicture(oie.Key);
                        if (string.IsNullOrWhiteSpace(imagepath))
                        {
                            BirdImage.Source = null;
                        }
                        else
                        {
                            if (System.IO.File.Exists(imagepath))
                            {
                                System.Uri uri = new Uri(imagepath);
                                BitmapImage bmi = uri.GetBitmapImage();
                                BirdImage.Source = bmi;
                            }
                            else
                            {
                                BirdImage.Source = null;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExclusionCheckBox.Visibility = Visibility.Hidden;
            SeenCheckBox.Visibility = Visibility.Hidden;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Core.Instance.ResetImageQuizExclusions();
            DisplayStats();
        }

        private void DisplayStats()
        {
            Tuple<int, int, int, int, int, int> figs = Core.Instance.ImageQuizNumbers();
            IncludedTextBlock.Text = figs.Item1.ToString();
            IncludedRareTextBlock.Text = figs.Item2.ToString();
            IncludedRecoTextBlock.Text = figs.Item3.ToString();
            ExcludedTextBlock.Text = figs.Item4.ToString();
            ExcludedRareTextBlock.Text = figs.Item5.ToString();
            ExcludedRecoTextBlock.Text = figs.Item6.ToString();
            _population = figs.Item1;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DisplayStats();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            QuizChecklistWindow w = new QuizChecklistWindow(false) { Owner = this };
            w.ShowDialog();
            DisplayStats();
        }
}