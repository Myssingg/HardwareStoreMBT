using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace HardwareStoreMBT
{
    public partial class EditDialog : Window
    {
        private object _entity;
        private List<(PropertyInfo prop, Control ctrl)> _map = new List<(PropertyInfo, Control)>();

        public EditDialog(object entity, string title)
        {
            InitializeComponent();
            _entity = entity;
            tbTitle.Text = title;
            BuildFields();
        }

        private void BuildFields()
        {
            FieldsPanel.Children.Clear();
            _map.Clear();

            var props = _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && IsSimple(p.PropertyType))
                .Where(p => !p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var prop in props)
            {
                FieldsPanel.Children.Add(new TextBlock
                {
                    Text = prop.Name,
                    Foreground = (System.Windows.Media.Brush)FindResource("DarkText"),
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 8, 0, 2)
                });

                Control c = null;
                var val = prop.GetValue(_entity);

                if (prop.Name.Equals("relevanceStatusId", StringComparison.OrdinalIgnoreCase))
                {
                    // КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ: Вытаскиваем только чистые свойства ID и Name БЕЗ отслеживания изменений.
                    // Контекст уничтожается сразу после вычитки, оставляя только безопасный список объектов на экране.
                    using (var tempDb = new HardwareStoreMBTEntities())
                    {
                        var statusList = tempDb.relevanceStatuses
                            .AsNoTracking()
                            .Select(s => new { Id = s.id, Name = s.name })
                            .ToList();

                        var comboBox = new ComboBox
                        {
                            Margin = new Thickness(5),
                            ItemsSource = statusList,
                            DisplayMemberPath = "Name",  // С большой буквы, так как это свойства анонимного типа выше
                            SelectedValuePath = "Id",    // С большой буквы, так как это свойства анонимного типа выше
                            SelectedValue = val
                        };
                        c = comboBox;
                    }
                }
                else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                {
                    c = new CheckBox { IsChecked = val as bool? ?? false, Margin = new Thickness(5) };
                }
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    c = new DatePicker { SelectedDate = val as DateTime?, Margin = new Thickness(5) };
                }
                else
                {
                    c = new TextBox { Text = val?.ToString() ?? "", Margin = new Thickness(5) };
                }

                FieldsPanel.Children.Add(c);
                _map.Add((prop, c));
            }
        }

        private bool IsSimple(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) t = t.GetGenericArguments()[0];
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var (prop, ctrl) in _map)
                {
                    object v = null;
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (ctrl is ComboBox cb)
                    {
                        var selectedVal = cb.SelectedValue;
                        if (selectedVal == null)
                        {
                            v = Nullable.GetUnderlyingType(prop.PropertyType) != null ? null : Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            v = Convert.ChangeType(selectedVal, targetType);
                        }

                        string navigationPropName = prop.Name.Replace("Id", "");

                        var navProp = _entity.GetType().GetProperty(navigationPropName)
                                      ?? _entity.GetType().GetProperty(prop.Name.Replace("StatusId", "Statuses"));

                        if (navProp != null && navProp.CanWrite)
                        {
                            navProp.SetValue(_entity, null); 
                        }

                    }
                    else if (ctrl is TextBox tb)
                    {
                        var txt = tb.Text;
                        if (string.IsNullOrWhiteSpace(txt))
                        {
                            v = Nullable.GetUnderlyingType(prop.PropertyType) != null ? null : Activator.CreateInstance(targetType);
                        }
                        else
                        {
                            v = Convert.ChangeType(txt, targetType);
                        }
                    }
                    else if (ctrl is CheckBox chk) v = chk.IsChecked;
                    else if (ctrl is DatePicker dp) v = dp.SelectedDate;

                    // Теперь безопасно записываем числовой ID
                    prop.SetValue(_entity, v);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}
