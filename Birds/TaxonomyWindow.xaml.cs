using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Birds;

public partial class TaxonomyWindow : Window
{
    public TaxonomyWindow()
        {
            InitializeComponent();
        }

        private ContextMenu _genusContextMenu;
        private ContextMenu _familyContextMenu;
        private ContextMenu _orderContextMenu;

        private MenuItem _genusAllocateMenuItem;

        private MenuItem _familyDeleteMenuItem;
        private MenuItem _familyRenameMenuItem;
        private MenuItem _familyAllocateMenuItem;

        private MenuItem _orderDeleteMenuItem;
        private MenuItem _orderRenameMenuItem;

        private void Window_ContentRendered(object sender, EventArgs e)
        {
           NewFamilyButton.IsEnabled = AddGenusButton.IsEnabled= false;
            FillOrdersList();
            FillUnallocatedGeneraList();
        }

        private void FillOrdersList()
        {
            OrdersListBox.Items.Clear();
            List<int> orderKeys = Core.Instance.OrderKeysInAlphabeticalOrder();
            foreach (int ky in orderKeys)
            {
                BirdOrder bo = Core.Instance.OrderDictionary[ky];
                int examples = 0;
                foreach(BirdFamily bf in Core.Instance.FamilyDictionary.Values)
                {
                    if (bf.OrderKey == bo.Key) { examples++; }
                }
                ListBoxItem lbi = new ListBoxItem() { ContextMenu=_orderContextMenu};
                string numfam = "families";
                if (examples == 1) { numfam = "family"; }
                TextBlock tbk = new TextBlock() { Text =$"{bo.Name} ({examples} {numfam})", FontSize = 14, FontWeight = FontWeights.Medium };
                lbi.Content = tbk;
                lbi.Tag = bo.Key;
                OrdersListBox.Items.Add(lbi);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FamiliesListBox.Items.Clear();
            GeneraListBox.Items.Clear();
            SpeciesListBox.Items.Clear();
            if (OrdersListBox.SelectedIndex < 0) { NewFamilyButton.IsEnabled = false; return; }
            NewFamilyButton.IsEnabled = true;
            ListBoxItem orderItem = (ListBoxItem)OrdersListBox.SelectedItem;
            int b = (int)orderItem.Tag;
            FillFamiliesList(b);
        }

        private void FillFamiliesList(int orderKey)
        {
            string ord = Core.Instance.OrderDictionary[orderKey].Name;
            GeneraHeading.Text = $"Genera in {ord}";
            SpeciesHeading.Text = $"Species in {ord}";
            FamiliesHeading.Text = $"Families in {ord}";
            FamiliesListBox.Items.Clear();
            List<BirdFamily> OrderFamilies = new List<BirdFamily>();
            List<BirdGenus> OrderGenera = new List<BirdGenus>();
            List<string> OrderSpecies = new List<string>();
            foreach (int g in Core.Instance.FamilyDictionary.Keys)
            {
                if (Core.Instance.FamilyDictionary[g].OrderKey.Equals(orderKey)) { OrderFamilies.Add(Core.Instance.FamilyDictionary[g]); }
            }
            OrderFamilies.Sort();
            foreach (BirdFamily fm in OrderFamilies)
            {
                // add genera for this family
                foreach (BirdGenus g in Core.Instance.GenusDictionary.Values)
                {
                    if (g.FamilyKey == fm.Key) { OrderGenera.Add(g); }
                }

                Brush paint = Brushes.Black;
                if (OrderGenera.Count < 1) paint = Brushes.Red;
                TextBlock tbk = new TextBlock() { Text = fm.Name, FontSize = 14, FontWeight = FontWeights.Medium, Foreground = paint };
                ListBoxItem lbi = new ListBoxItem() { ContextMenu = _familyContextMenu, Content = tbk, Tag = fm.Key };
                FamiliesListBox.Items.Add(lbi);
            }
            OrderGenera.Sort();
            foreach (BirdGenus g in OrderGenera)
            {
                // add species for this genus
                foreach (Bird bd in Core.Instance.BirdDictionary.Values)
                {
                    if (bd.Genus == g.Name) { OrderSpecies.Add(bd.EnglishNameCurrent); }
                }

                TextBlock tbk = new TextBlock() { Text = $"{g.Name} [{Core.Instance.FamilyDictionary[g.FamilyKey].Name}]", FontSize = 14, FontWeight = FontWeights.Medium };
                ListBoxItem lbi = new ListBoxItem() { Content = tbk };
                GeneraListBox.Items.Add(lbi);
            }
            OrderSpecies.Sort();
            foreach (string sp in OrderSpecies)
            {
                TextBlock tbk = new TextBlock() { Text = sp, FontSize = 14, FontWeight = FontWeights.Medium };
                ListBoxItem lbi = new ListBoxItem() { Content = tbk };
                SpeciesListBox.Items.Add(lbi);
            }
        }

        private void FamiliesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFamily();
        }

        private void RefreshFamily()
        {
            if (FamiliesListBox.SelectedIndex < 0) { AddGenusButton.IsEnabled = false; return; }
            AddGenusButton.IsEnabled = (UnallocatedGeneraListBox.SelectedIndex >= 0);

            ; GeneraListBox.Items.Clear();
            ListBoxItem lbi = (ListBoxItem)FamiliesListBox.SelectedItem;
            int fmyKey = (int)lbi.Tag;
            string famnom = Core.Instance.FamilyDictionary[fmyKey].Name;
            GeneraHeading.Text = $"Genera and species in {famnom}";
            List<Bird> speciesList = new List<Bird>();
            List<string> genusList = new List<string>();

            foreach (Bird bd in Core.Instance.BirdDictionary.Values)
            {
                if (bd.FamilyKey.Equals(fmyKey))
                {
                    if (!genusList.Contains(bd.Genus)) { genusList.Add(bd.Genus); }
                }
            }
            genusList.Sort();

            foreach (string gn in genusList)
            {
                speciesList.Clear();
                foreach (Bird bd in Core.Instance.BirdDictionary.Values)
                {
                    if (bd.Genus.Equals(gn)) { speciesList.Add(bd); }
                }
                TextBlock tbkG = new TextBlock() { Text = $"{gn} ({speciesList.Count} species)", FontSize = 14, FontWeight = FontWeights.Medium };
                ListBoxItem itmG = new ListBoxItem() { ContextMenu = _genusContextMenu, Tag = gn, Content = tbkG };
                GeneraListBox.Items.Add(itmG);

                speciesList.Sort();
                foreach (Bird bd in speciesList)
                {
                    StackPanel spnl = new StackPanel() { Orientation = Orientation.Horizontal };
                    TextBlock tbkS = new TextBlock() { Text = bd.Genus, FontSize = 14, FontWeight = FontWeights.Light, Margin = new Thickness(24, 0, 0, 0), FontStyle = FontStyles.Italic };
                    spnl.Children.Add(tbkS);
                    tbkS = new TextBlock() { Text =$" {bd.Binomial.Item2}", FontSize = 14, FontWeight = FontWeights.Medium, FontStyle = FontStyles.Italic };
                    spnl.Children.Add(tbkS);
                    tbkS = new TextBlock() { Text = $" - {bd.EnglishNameCurrent}", FontSize = 14, FontWeight = FontWeights.Bold };
                    spnl.Children.Add(tbkS);
                    ListBoxItem itmS = new ListBoxItem() { Content = spnl };
                    GeneraListBox.Items.Add(itmS);
                }
            }
        }

        private void NewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox box = new InputBox("New order", "Enter the order name", string.Empty) { Owner = this };
            bool? ans = box.ShowDialog();
            if (ans.HasValue && ans.Value)
            {
                Core.Instance.CreateNewOrder(box.ResponseText);
                FillOrdersList();
            }
        }

        private void NewFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            int i = OrdersListBox.SelectedIndex;
            ListBoxItem lbi = (ListBoxItem)OrdersListBox.Items[i];
            int eraId = (int)lbi.Tag;
            string eraName = Core.Instance.OrderDictionary[eraId].Name;

            InputBox box = new InputBox("New family in " + eraName, "Enter the family name", string.Empty) { Owner = this };
            bool? ans = box.ShowDialog();
            if (ans.HasValue && ans.Value)
            {
                Core.Instance.CreateNewFamily(box.ResponseText, eraId);
                FillFamiliesList(eraId);
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            int ei = OrdersListBox.SelectedIndex;
            ListBoxItem lbi = (ListBoxItem)OrdersListBox.Items[ei];
            byte orderId = (Byte)lbi.Tag;
            if (Core.Instance.OrderHasFamilies(orderId))
            {
                MessageBox.Show("This order contains families", "Cannot delete order", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult response = MessageBox.Show($"Confirm deletion of the order \"{Core.Instance.OrderDictionary[orderId].Name}\"", "Delete order", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (response.Equals(MessageBoxResult.Cancel)) { return; }
            Core.Instance.OrderDictionary.Remove(orderId);
            FillOrdersList();
        }

        private void DeleteFamily_Click(object sender, RoutedEventArgs e)
        {
            int ei = OrdersListBox.SelectedIndex;
            ListBoxItem lbi = (ListBoxItem)OrdersListBox.Items[ei];
            int orderId = (int)lbi.Tag;

            int gi = FamiliesListBox.SelectedIndex;
            lbi = (ListBoxItem)FamiliesListBox.Items[gi];
            int fmyId = (int)lbi.Tag;
            if (Core.Instance.FamilyHasSpecies(fmyId))
            {
                MessageBox.Show("This family contains species", "Cannot delete family", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult response = MessageBox.Show($"Confirm deletion of the family \"{Core.Instance.FamilyDictionary[fmyId].Name}\" from \"{Core.Instance.OrderDictionary[orderId].Name}\"", "Delete family", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (response.Equals(MessageBoxResult.Cancel)) { return; }
            Core.Instance.FamilyDictionary.Remove(fmyId);
            FillFamiliesList(orderId);
        }

        private void RenameOrder_Click(object sender, RoutedEventArgs e)
        {
            int i = OrdersListBox.SelectedIndex;
            ListBoxItem lbi = (ListBoxItem)OrdersListBox.Items[i];
            int orderId = (int)lbi.Tag;
            BirdOrder ord = Core.Instance.OrderDictionary[orderId];
            InputBox box = new InputBox("Rename order", "Edit the order name ", ord.Name) { Owner = this };
            bool? ans = box.ShowDialog();
            if (ans.HasValue && ans.Value) { ord.Name = box.ResponseText; FillOrdersList(); }
        }

        private void RenameFamily_Click(object sender, RoutedEventArgs e)
        {
            int i = OrdersListBox.SelectedIndex;
            ListBoxItem lbi = (ListBoxItem)OrdersListBox.Items[i];
            int orderId = (int)lbi.Tag;

            i = FamiliesListBox.SelectedIndex;
            lbi = (ListBoxItem)FamiliesListBox.Items[i];
            int famId = (int)lbi.Tag;
            BirdFamily fmy = Core.Instance.FamilyDictionary[famId];
            InputBox box = new InputBox("Rename family", $"Edit the name of family \"{Core.Instance.FamilyDictionary[famId].Name}\" in \"{Core.Instance.OrderDictionary[orderId].Name}\"", fmy.Name) { Owner = this };
            bool? ans = box.ShowDialog();
            if (ans.HasValue && ans.Value) { fmy.Name = box.ResponseText; FillFamiliesList(orderId); }
        }

        private void AllocateFamily_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbiO = (ListBoxItem)OrdersListBox.SelectedItem;
            int orderId = (int)lbiO.Tag;

            ListBoxItem lbiF = (ListBoxItem)FamiliesListBox.SelectedItem;
            int famId = (int)lbiF.Tag;
            BirdFamily fmy = Core.Instance.FamilyDictionary[famId];
            PickerWindow picker = new PickerWindow("orders") { Owner = this };
            if (picker.ShowDialog()== true)
            {
                int q = picker.Selection;
                fmy.OrderKey = q;
            }
            FillFamiliesList(orderId); 
        }

        private void AllocateGenus_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbiF = (ListBoxItem)FamiliesListBox.SelectedItem;
            int familyId = (int)lbiF.Tag;

            ListBoxItem lbiG = (ListBoxItem)GeneraListBox.SelectedItem;
            string genusName = lbiG.Tag.ToString();
            BirdGenus gen = Core.Instance.GenusDictionary[genusName];
            PickerWindow picker = new PickerWindow("families") { Owner = this };
            if (picker.ShowDialog() == true)
            {
                int q = picker.Selection;
                gen.FamilyKey = q;
            }
            RefreshFamily();
        }

        private void FillUnallocatedGeneraList()
        {
            UnallocatedGeneraListBox.Items.Clear();
            List<string> lostGeneraList = Core.Instance.GeneraUnallocated();
            foreach (string g in lostGeneraList)
            {
                ListBoxItem lbi = new ListBoxItem();
                TextBlock tbk = new TextBlock() { Text = g, FontSize = 14, FontWeight = FontWeights.Medium };
                lbi.Content = tbk;
                lbi.Tag = g;
                UnallocatedGeneraListBox.Items.Add(lbi);
            }
        }

        private void AddGenusButton_Click(object sender, RoutedEventArgs e)
        {
            if (UnallocatedGeneraListBox.SelectedIndex < 0) { return; }
            if (FamiliesListBox.SelectedIndex < 0) { return; }
            ListBoxItem lbig = (ListBoxItem)UnallocatedGeneraListBox.SelectedItem;
            string genus = lbig.Tag.ToString();
            ListBoxItem lbif = (ListBoxItem)FamiliesListBox.SelectedItem;
            int family = (int)lbif.Tag;
            string familyName = Core.Instance.FamilyDictionary[family].Name;
            MessageBoxResult answ = MessageBox.Show($"Add {genus} to {familyName}?", Jbh.AppManager.AppName, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (answ == MessageBoxResult.Cancel) { return; }
            BirdGenus bg = new BirdGenus(genus, family);
            Core.Instance.GenusDictionary.Add(genus, bg);
            RefreshFamily();
            FillUnallocatedGeneraList();
        }

        private void UnallocatedGeneraListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddGenusButton.IsEnabled = ((UnallocatedGeneraListBox.SelectedIndex >= 0) && (FamiliesListBox.SelectedIndex>=0));
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

            _familyContextMenu = new ContextMenu();
            _genusContextMenu = new ContextMenu();
            _orderContextMenu = new ContextMenu();

            _familyAllocateMenuItem = new MenuItem() { Header = "Allocate to a different Order" };
            _familyDeleteMenuItem=new MenuItem() { Header = "Delete family" };
            _familyRenameMenuItem = new MenuItem() { Header = "Rename family" };

            _familyDeleteMenuItem.Click += DeleteFamily_Click;
            _familyRenameMenuItem.Click += RenameFamily_Click;
            _familyAllocateMenuItem.Click += AllocateFamily_Click;

            _familyContextMenu.Items.Add(_familyAllocateMenuItem);
            _familyContextMenu.Items.Add(_familyDeleteMenuItem);
            _familyContextMenu.Items.Add(_familyRenameMenuItem);

            _genusAllocateMenuItem = new MenuItem() { Header = "Allocate to a different Family" };
            _genusAllocateMenuItem.Click += AllocateGenus_Click;

            _genusContextMenu.Items.Add(_genusAllocateMenuItem);

            _orderDeleteMenuItem = new MenuItem() { Header = "Delete order" };
            _orderRenameMenuItem = new MenuItem() { Header = "Rename order" };

            _orderDeleteMenuItem.Click += DeleteOrder_Click;
            _orderRenameMenuItem.Click += RenameOrder_Click;

            _orderContextMenu.Items.Add(_orderDeleteMenuItem);
            _orderContextMenu.Items.Add(_orderRenameMenuItem);
        }
}