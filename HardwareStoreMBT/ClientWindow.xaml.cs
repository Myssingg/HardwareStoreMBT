using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HardwareStoreMBT
{
    public partial class ClientWindow : Window
    {
        private clients _client;
        private List<products> _cart = new List<products>();

        public ClientWindow()
        {
            InitializeComponent();
            _client = App.Current.Properties["CurrentClient"] as clients;
            if (_client == null) { Close(); return; }
            LoadCategories(); LoadProducts(); LoadProfile(); LoadOrders();
        }

        private void LoadCategories()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                var list = db.categories.ToList(); list.Insert(0, new categories { id = 0, name = "Все" });
                cmbCategory.ItemsSource = list; cmbCategory.SelectedIndex = 0;
            }
        }

        private void LoadProducts(string s = "")
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                var q = db.products.Include(p => p.categories).AsQueryable();
                if (!string.IsNullOrWhiteSpace(s)) q = q.Where(p => p.name.Contains(s));
                if (cmbCategory.SelectedValue is int id && id > 0) q = q.Where(p => p.categoryId == id);
                dgProducts.ItemsSource = q.ToList();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) => LoadProducts(txtSearch.Text);

        private void btnAddCart_Click(object sender, RoutedEventArgs e)
        {
            var p = (sender as Button).Tag as products;
            if (p != null) { _cart.Add(p); tbCart.Text = $"В корзине: {_cart.Count} товар(ов)"; }
        }

        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) { MessageBox.Show("Корзина пуста"); return; }
            var msg = string.Join("\n", _cart.GroupBy(x => x.id).Select(g => $"{g.First().name} × {g.Count()}"));
            MessageBox.Show(msg, "Корзина");
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cart.Clear();
            tbCart.Text = $"В корзине: {_cart.Count} товар(ов)";
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) { MessageBox.Show("Добавьте товары"); return; }
            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    var order = new orders
                    {
                        clientId = _client.id,
                        paymentMethodId = 1,
                        paymentStatusId = 1,
                        orderStatusId = 1,
                        summ = 0
                    };
                    db.orders.Add(order); db.SaveChanges();

                    foreach (var g in _cart.GroupBy(x => x.id))
                    {
                        var prod = db.products.Find(g.Key);
                        db.productsInOrder.Add(new productsInOrder
                        {
                            orderId = order.id,
                            productId = g.Key,
                            productCount = g.Count(),
                            productPrice = prod.price
                        });
                    }
                    db.SaveChanges();
                    MessageBox.Show("Заказ оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    _cart.Clear(); tbCart.Text = "Корзина пуста"; LoadOrders();
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void LoadOrders()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                dgOrders.ItemsSource = db.orders.Include(o => o.orderStatuses).Include(o => o.paymentStatuses)
                    .Where(o => o.clientId == _client.id).OrderByDescending(o => o.id).ToList();
            }
        }

        private void btnRefreshOrders_Click(object sender, RoutedEventArgs e) => LoadOrders();

        private void LoadProfile()
        {
            txtSurname.Text = _client.surname; txtName.Text = _client.name;
            txtPatronymic.Text = _client.patronymic; txtPhone.Text = _client.phoneNumber;
            txtEmail.Text = _client.email; txtAddress.Text = _client.address;
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    var c = db.clients.Find(_client.id);
                    c.surname = txtSurname.Text; c.name = txtName.Text;
                    c.patronymic = txtPatronymic.Text; c.phoneNumber = txtPhone.Text; c.address = txtAddress.Text;
                    db.SaveChanges();
                    MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


    }
}