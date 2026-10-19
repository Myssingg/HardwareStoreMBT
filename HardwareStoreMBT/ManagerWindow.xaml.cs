using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HardwareStoreMBT
{
    public partial class ManagerWindow : Window
    {
        public ManagerWindow() { InitializeComponent(); LoadOrders(); LoadClients(); LoadProducts(); LoadStatuses(); }

        private void LoadStatuses()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                var list = db.orderStatuses.ToList(); list.Insert(0, new orderStatuses { id = 0, name = "Все" });
                cmbFilterStatus.ItemsSource = list; cmbFilterStatus.SelectedIndex = 0;
            }
        }

        private void LoadOrders(int? sid = null)
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                var q = db.orders.Include(o => o.clients).Include(o => o.orderStatuses).Include(o => o.paymentStatuses).AsQueryable();
                if (sid.HasValue) q = q.Where(o => o.orderStatusId == sid.Value);
                dgOrders.ItemsSource = q.OrderByDescending(o => o.id).ToList();
            }
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (cmbFilterStatus.SelectedValue is int id && id > 0) LoadOrders(id); else LoadOrders();
        }

        private void btnAll_Click(object sender, RoutedEventArgs e) { cmbFilterStatus.SelectedIndex = 0; LoadOrders(); }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadOrders();

        private void dgOrders_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(dgOrders.SelectedItem is  orders o)) return;
            using (var db = new HardwareStoreMBTEntities())
            {
                var items = db.productsInOrder.Include(p => p.products).Where(p => p.orderId == o.id).ToList();
                var msg = string.Join("\n", items.Select(i => $"{i.products.name} × {i.productCount} = {i.productPrice * i.productCount:C}"));
                MessageBox.Show(msg, $"Заказ №{o.id}");
            }
        }

        private void btnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgOrders.SelectedItem is orders order)) return;
            var dlg = new Window { Title = "Статус", Width = 300, Height = 150, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            var sp = new StackPanel { Margin = new Thickness(10) };
            var cb = new ComboBox { DisplayMemberPath = "name", SelectedValuePath = "id" };
            using (var db = new HardwareStoreMBTEntities()) cb.ItemsSource = db.orderStatuses.ToList();
            cb.SelectedValue = order.orderStatusId;
            sp.Children.Add(new TextBlock { Text = "Новый статус:" }); sp.Children.Add(cb);
            var btn = new Button { Content = "Сохранить", Margin = new Thickness(0, 10, 0, 0) };
            btn.Click += (s, ev) =>
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        var o = db.orders.Find(order.id); 
                        o.orderStatusId = (int)cb.SelectedValue; 
                        db.SaveChanges();

                        dlg.Close(); 
                        LoadOrders();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            };
            sp.Children.Add(btn); dlg.Content = sp; dlg.ShowDialog();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is orders o)
                try { WordReportHelper.PrintOrderCheck(o.id); }
                catch (Exception ex) { MessageBox.Show("Ошибка печати: " + ex.Message); }
        }

        private void LoadClients(string s = "")
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                var q = db.clients.AsQueryable();
                if (!string.IsNullOrWhiteSpace(s)) q = q.Where(c => c.surname.Contains(s) || c.email.Contains(s));
                dgClients.ItemsSource = q.ToList();
            }
        }

        private void txtClientSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadClients(txtClientSearch.Text);

        private void LoadProducts()
        {
            using (var db = new HardwareStoreMBTEntities())
                dgProducts.ItemsSource = db.products.Include(p => p.categories).ToList();
        }
    }
}