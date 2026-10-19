using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HardwareStoreMBT
{
    public partial class AdminWindow : Window
    {
        public AdminWindow() { InitializeComponent(); LoadOrders(); LoadProducts(); LoadSupplies(); LoadEmployees(); cmbRef.SelectedIndex = 0; }

        private void LoadOrders()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                dgOrders.ItemsSource = db.orders.Include(o => o.clients)
                    .Include(o => o.orderStatuses)
                    .OrderByDescending(o => o.id)
                    .ToList();
            }
        }
        private void btnRefreshOrders_Click(object sender, RoutedEventArgs e) => LoadOrders();

        private void LoadProducts()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                dgProducts.ItemsSource = db.products
                    .Include(p => p.categories)
                    .Include(p => p.manufacturers)
                    .Include(p => p.relevanceStatuses)
                    .ToList();
            }
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var p = new products();
            if (new EditDialog(p, "Новый товар").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        db.products.Add(p);
                        db.SaveChanges();
                        LoadProducts();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgProducts.SelectedItem is products src)) return;
            var copy = new products { 
                id = src.id, 
                name = src.name, 
                model = src.model, 
                description = src.description, 
                price = src.price, 
                stock = src.stock, 
                categoryId = src.categoryId, 
                manufacturerId = src.manufacturerId, 
                relevanceStatusId = src.relevanceStatusId };

            if (new EditDialog(copy, "Редактирование").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        var o = db.products.Find(copy.id);
                        o.name = copy.name;
                        o.model = copy.model;
                        o.description = copy.description;
                        o.price = copy.price;
                        o.stock = copy.stock;
                        o.categoryId = copy.categoryId;
                        o.manufacturerId = copy.manufacturerId;
                        o.relevanceStatusId = copy.relevanceStatusId;

                        db.SaveChanges();
                        LoadProducts();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgProducts.SelectedItem is products p)) return;
            if (MessageBox.Show("Удалить?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    p.relevanceStatusId = 2;
                    db.SaveChanges();
                    LoadProducts();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadSupplies()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                dgSupplies.ItemsSource = db.supplies.Include(s => s.suppliers).
                    Include(s => s.employees).
                    OrderByDescending(s => s.id).
                    ToList();
            }
        }

        private void dgSupplies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(dgSupplies.SelectedItem is supplies s)) return;
            using (var db = new HardwareStoreMBTEntities())
            {
                dgSupplyItems.ItemsSource = db.productsInSupply
                    .Include(p => p.products)
                    .Where(p => p.supplyId == s.id)
                    .ToList();
            }
        }

        private void btnAddSupply_Click(object sender, RoutedEventArgs e)
        {
            var s = new supplies { date = DateTime.Today, supplierId = 1, employeeId = 1 };
            if (new EditDialog(s, "Новая поставка").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        db.supplies.Add(s);
                        db.SaveChanges();
                        LoadSupplies();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void LoadEmployees()
        {
            using (var db = new HardwareStoreMBTEntities())
            {
                dgEmployees.ItemsSource = db.employees.Include(x => x.roles)
                    .Include(x => x.relevanceStatuses)
                    .ToList();
            }
        }

        private void btnAddEmp_Click(object sender, RoutedEventArgs e)
        {
            var emp = new employees { passwordHash = App.ComputeSha256("123456") };
            if (new EditDialog(emp, "Новый сотрудник").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        db.employees.Add(emp);
                        db.SaveChanges();
                        LoadEmployees();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnEditEmp_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgEmployees.SelectedItem is employees src)) return;
            var copy = new employees
            {
                id = src.id,
                login = src.login,
                passwordHash = src.passwordHash,
                surname = src.surname,
                name = src.name,
                patronymic = src.patronymic,
                roleId = src.roleId,
                relevanceStatusId = src.relevanceStatusId
            };
            if (new EditDialog(copy, "Редактирование").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        var o = db.employees.Find(copy.id);
                        o.login = copy.login;
                        o.surname = copy.surname;
                        o.name = copy.name;
                        o.patronymic = copy.patronymic;
                        o.roleId = copy.roleId;
                        o.relevanceStatusId = copy.relevanceStatusId;

                        if (copy.passwordHash != o.passwordHash && copy.passwordHash.Length != 64)
                            o.passwordHash = App.ComputeSha256(copy.passwordHash);
                        else if (copy.passwordHash.Length == 64)
                            o.passwordHash = copy.passwordHash;
                        db.SaveChanges();
                        LoadEmployees();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnDeleteEmp_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgEmployees.SelectedItem is employees emp)) return;
            if (MessageBox.Show("Удалить?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HardwareStoreMBTEntities())
                {
                    emp.relevanceStatusId = 2;
                    db.SaveChanges();
                    LoadEmployees();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void cmbRef_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadRef();

        private void LoadRef()
        {
            var tag = (cmbRef.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            using (var db = new HardwareStoreMBTEntities())
            {
                switch (tag)
                {
                    case "categories": 
                        dgRef.ItemsSource = db.categories
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    case "manufacturers": 
                        dgRef.ItemsSource = db.manufacturers
                            .Include(x => x.relevanceStatuses)
                            .ToList();
                        break;
                    case "suppliers": 
                        dgRef.ItemsSource = db.suppliers
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    case "roles": 
                        dgRef.ItemsSource = db.roles
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    case "paymentMethods": 
                        dgRef.ItemsSource = db.paymentMethods
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    case "paymentStatuses": 
                        dgRef.ItemsSource = db.paymentStatuses
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    case "orderStatuses": 
                        dgRef.ItemsSource = db.orderStatuses
                            .Include(x => x.relevanceStatuses)
                            .ToList(); 
                        break;
                    default: dgRef.ItemsSource = null; break;
                }
            }
        }

        private void btnAddRef_Click(object sender, RoutedEventArgs e)
        {
            object ent = null;
            var tag = (cmbRef.SelectedItem as ComboBoxItem).Tag.ToString();
            switch (tag)
            {
                case "categories": ent = new categories(); break;
                case "manufacturers": ent = new manufacturers(); break;
                case "suppliers": ent = new suppliers(); break;
                case "roles": ent = new roles(); break;
                case "paymentMethods": ent = new paymentMethods(); break;
                case "paymentStatuses": ent = new paymentStatuses(); break;
                case "orderStatuses": ent = new orderStatuses(); break;
            }
            if (new EditDialog(ent, "Новая запись").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        db.Set(ent.GetType()).Add(ent);
                        db.SaveChanges();
                        LoadRef();
                    }
                }

                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnEditRef_Click(object sender, RoutedEventArgs e)
        {
            if (dgRef.SelectedItem == null) return;
            var ent = dgRef.SelectedItem;
            if (new EditDialog(ent, "Редактирование").ShowDialog() == true)
            {
                try
                {
                    using (var db = new HardwareStoreMBTEntities())
                    {
                        var entry = db.Entry(ent);
                        if (entry.State == EntityState.Detached)
                            db.Set(ent.GetType()).Attach(ent);
                        entry.State = EntityState.Modified;
                        db.SaveChanges();
                        LoadRef();
                    }

                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void btnDeleteRef_Click(object sender, RoutedEventArgs e)
        {
            if (dgRef.SelectedItem == null) return;
            if (MessageBox.Show("Удалить?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                var ent = dgRef.SelectedItem;
                using (var db = new HardwareStoreMBTEntities())
                {
                    var property = ent.GetType().GetProperty("relevanceStatusId");
                    var entry = db.Entry(ent);
                    if (entry.State == EntityState.Detached)
                        db.Set(ent.GetType()).Attach(ent);
                    property.SetValue(ent, 2);
                    db.Entry(ent).State = EntityState.Modified;
                    db.SaveChanges();
                    LoadRef();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}