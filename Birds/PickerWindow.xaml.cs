using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Birds;

public partial class PickerWindow : Window
{
    public PickerWindow(string mode)
        {
            InitializeComponent();
            _mode = mode;
            _selection = -1;
        }

        private string _mode;
        private int _selection;

        public int Selection { get => _selection; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PickerListBox.Items.Clear();
            switch (_mode)
            {
                case "orders":
                    {
                        List<BirdOrder> mylist = new List<BirdOrder>();
                        foreach(BirdOrder bo in Core.Instance.OrderDictionary.Values)
                        {
                            mylist.Add(bo);
                        }
                        mylist.Sort();
                        foreach (BirdOrder bo in mylist)
                        {
                            TextBlock tbk = new TextBlock() { Text = bo.Name };
                            ListBoxItem lbi = new ListBoxItem() { Tag = bo.Key, Content = tbk };
                            PickerListBox.Items.Add(lbi);
                        }
                        break;
                    }
                case "families":
                    {
                        List<BirdFamily> mylist = new List<BirdFamily>();
                        foreach (BirdFamily bf in Core.Instance.FamilyDictionary.Values)
                        {
                            mylist.Add(bf);
                        }
                        mylist.Sort();
                        foreach (BirdFamily bf in mylist)
                        {
                            TextBlock tbk = new TextBlock() { Text = bf.Name };
                            ListBoxItem lbi = new ListBoxItem() { Tag = bf.Key, Content = tbk };
                            PickerListBox.Items.Add(lbi);
                        }
                        break;
                    }
                default:
                    {
                        MessageBox.Show("Unknown mode for Picker Window", Jbh.AppManager.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi =(ListBoxItem) PickerListBox.SelectedItem;
            _selection = (int)lbi.Tag;
            DialogResult = true;
        }
}