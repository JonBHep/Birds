using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Birds;

public partial class ListingWindow : Window
{
    private string _listType;

        public ListingWindow(string listType)
        {
            InitializeComponent();
            _listType = listType;
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
            switch(_listType)
            {
                case "international":
                    {
                        ListInternationalNames();
                        break;
                    }
                case "non-A":
                    {
                        ListNonABirds();
                        break;
                    }
                case "british":
                    {
                        BritishList();
                        break;
                    }
                case "personal":
                    {
                        PersonalList();
                        break;
                    }
                case "index":
                    {
                        DisplayIndex();
                        break;
                    }
            }
        }

        private void DisplayIndex()
        {
            bool flag = false;
            List<Tuple<string, string, string, bool>> index = new List<Tuple<string, string, string, bool>>();
            foreach (Bird fowl in Core.Instance.BirdDictionary.Values)
            {
                string deflt = fowl.ScientificNameCurrent;
                Tuple<string, string, string, bool> nom = new Tuple<string, string, string, bool>(deflt, string.Empty, string.Empty, true);
                index.Add(nom);
                nom = new Tuple<string, string, string, bool>(fowl.EnglishNameCurrent, string.Empty, deflt, false);
                index.Add(nom);
                if (!string.IsNullOrEmpty(fowl.DifferentInternationalEnglishName))
                {
                    nom = new Tuple<string, string, string, bool>(fowl.DifferentInternationalEnglishName,"International English name", deflt, false);
                    index.Add(nom);
                }
                if (fowl.OtherEnglishNames.Count > 1) { flag = true; }
                foreach (string nm in fowl.OtherEnglishNames)
                {
                    nom = new Tuple<string, string, string, bool>(nm,$"= {fowl.EnglishNameCurrent}", deflt, false);
                    index.Add(nom);
                }
                foreach (string nm in fowl.FormerScientificNames)
                {
                    nom = new Tuple<string, string, string, bool>(nm,"Former name of", deflt, true);
                    index.Add(nom);
                }
            }
            index.Sort();
            foreach (Tuple<string, string,string, bool> ois in index)
            {
                TextBlock tb1 = new TextBlock() { Width = 200, Text = ois.Item1 };
                TextBlock tb1a = new TextBlock() { Width = 200, Text = ois.Item2 };
                if (ois.Item4) { tb1.FontStyle = FontStyles.Italic; tb1.Foreground = Brushes.DarkMagenta; }
                TextBlock tb2 = new TextBlock() { Width = 200, Text = ois.Item3, FontStyle = FontStyles.Italic, Foreground = Brushes.DarkMagenta };
                StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal };
                pnl.Children.Add(tb1);
                pnl.Children.Add(tb1a);
                pnl.Children.Add(tb2);
                ListBoxItem item = new ListBoxItem() { Content = pnl };
                BirdListBox.Items.Add(item);
            }
        }

        private void ListInternationalNames()
        {
            List<int> keySeq = Core.Instance.BirdKeysInTaxonomicSequence();
            int a = 0;
            int b = 0;
            int c = 0;
            int x = 0;
            int t = 0;
            foreach (int k in keySeq)
            {
                Bird bd = Core.Instance.BirdDictionary[k];
                if (!string.IsNullOrWhiteSpace(bd.DifferentInternationalEnglishName))
                {
                    t++;
                    TextBlock tbkId = new TextBlock() { Width = 26, Padding = new Thickness(0, 0, 4, 0), TextAlignment = TextAlignment.Right, Text = bd.TaxonomicSequenceIndex.ToString(), Foreground = Brushes.BurlyWood };
                    TextBlock tbkEn = new TextBlock() { Foreground = Brushes.Brown, Text = bd.EnglishNameCurrent, Width=200 };
                    TextBlock tbkIn = new TextBlock() { Foreground = Brushes.SaddleBrown, Text = bd.DifferentInternationalEnglishName , Width=200};
                    if (bd.EnglishNameCurrent == bd.DifferentInternationalEnglishName) { tbkIn.Foreground = Brushes.Red; }
                    TextBlock tbkSc = new TextBlock() { Foreground = Brushes.Brown, Text = bd.ScientificNameCurrent, Width = 200 };
                    StackPanel spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                    spnl.Children.Add(tbkId);
                    spnl.Children.Add(tbkEn);
                    spnl.Children.Add(tbkIn);
                    spnl.Children.Add(tbkSc);
                    ListBoxItem itm = new ListBoxItem() { Content = spnl, Tag = k };
                    BirdListBox.Items.Add(itm);
                    if (bd.BOUCode.Contains("A"))
                    {
                        a++;
                    }
                    else if (bd.BOUCode.Contains("B"))
                    {
                        b++;
                    }
                    else if (bd.BOUCode.Contains("C"))
                    {
                        c++;
                    }
                    else if (bd.BOUCode == "X")
                    {
                        x++;
                    }
                }
            }
            Title = $"Birds with distinct common and international English names ({BirdListBox.Items.Count})";
        }

        private void ListNonABirds()
        {
            List<int> fullSeq = Core.Instance.BirdKeysInTaxonomicSequence();
            List<int> trimSeq = new List<int>();
            foreach (int k in fullSeq)
            {
                if (!Core.Instance.BirdDictionary[k].BOUCode.Contains("A")) { trimSeq.Add(k); }
            }
            DisplayList(trimSeq,"Birds with BOU category other than 'A'");
        }

        private void BritishList()
        {
            List<int> fullSeq = Core.Instance.BirdKeysInTaxonomicSequence();
            List<int> trimSeq = new List<int>();
            foreach (int k in fullSeq)
            {
                if (Core.Instance.BirdDictionary[k].BOUCode != "X")
                {
                    trimSeq.Add(k);
                }
            }
            DisplayList(trimSeq, "Birds on the BOU British List");
        }

        private void PersonalList()
        {
            List<int> fullSeq = Core.Instance.BirdKeysInTaxonomicSequence();
            List<int> trimSeq = new List<int>();
            foreach (int k in fullSeq)
            {
                if (Core.Instance.BirdDictionary[k].SeenEver) { trimSeq.Add(k); }
            }
            DisplayList(trimSeq, "Birds I have seen in the wild");
        }

        private void DisplayList(List<int> birdlist, string caption)
        {
            int a = 0;
            int b = 0;
            int c = 0;
            int x = 0;
            int t = 0;
            foreach (int k in birdlist)
            {
                Bird bd = Core.Instance.BirdDictionary[k];
                t++;
                StackPanel spnl = new StackPanel() { Orientation = Orientation.Horizontal };

                TextBlock tbkId = new TextBlock() { Width = 26, Padding = new Thickness(0, 0, 4, 0), TextAlignment = TextAlignment.Right, Text = bd.TaxonomicSequenceIndex.ToString(), Foreground = Brushes.BurlyWood };
                spnl.Children.Add(tbkId);

                if (bd.FamilyKey > 0)
                {
                    int okey = Core.Instance.FamilyDictionary[bd.FamilyKey].OrderKey;
                    TextBlock tbkOr = new TextBlock() { Foreground = Brushes.RosyBrown, Text = Core.Instance.OrderDictionary[okey].Name, Width = 128, FontWeight = FontWeights.Bold };
                    TextBlock tbkFm = new TextBlock() { Foreground = Brushes.RosyBrown, Text = Core.Instance.FamilyDictionary[bd.FamilyKey].Name, Width = 128, FontWeight = FontWeights.Bold };
                    spnl.Children.Add(tbkOr);
                    spnl.Children.Add(tbkFm);
                }

                TextBlock tbkEn = new TextBlock() { Foreground = Brushes.SaddleBrown, Text = bd.EnglishNameCurrent, Width = 200 };
                if (bd.SeenFrequently) { tbkEn.FontWeight = FontWeights.Bold; }
                TextBlock tbkIn = new TextBlock() { Foreground = Brushes.SaddleBrown, Text = bd.DifferentInternationalEnglishName, Width = 200 };
                if (bd.EnglishNameCurrent == bd.DifferentInternationalEnglishName) { tbkIn.Foreground = Brushes.Red; }
                TextBlock tbkSc = new TextBlock() { Foreground = Brushes.Brown, Text = bd.ScientificNameCurrent, Width = 200 };
                TextBlock tbkCd = new TextBlock() { Foreground = Brushes.Black, Text = bd.BOUCode, Width = 100, FontWeight = FontWeights.Bold };
                TextBlock tbkSt = new TextBlock() { Foreground = Brushes.Black, Text = bd.Status };
                spnl.Children.Add(tbkEn);
                spnl.Children.Add(tbkIn);
                spnl.Children.Add(tbkSc);
                spnl.Children.Add(tbkCd);
                spnl.Children.Add(tbkSt);

                ListBoxItem itm = new ListBoxItem() { Content = spnl, Tag = k };
                BirdListBox.Items.Add(itm);
                if (bd.BOUCode.Contains("A"))
                {
                    a++;
                }
                else if (bd.BOUCode.Contains("B"))
                {
                    b++;
                }
                else if (bd.BOUCode.Contains("C"))
                {
                    c++;
                }
                else if (bd.BOUCode == "X")
                {
                    x++;
                }
            }
            Title = caption;
            TotlBloc.Text = t.ToString();
            CatABloc.Text = a.ToString();
            CatBBloc.Text = b.ToString();
            CatCBloc.Text = c.ToString();
            CatXBloc.Text = x.ToString();
        }
}