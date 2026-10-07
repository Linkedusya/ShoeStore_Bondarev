using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShoeStore_Bondarev
{
    public partial class MainMenuWindow : Window
    {
        private List<Product> _allProducts;
        private List<Category> _allCategories;
        private List<Manufacturer> _allManufacturers;
        private List<Supplier> _allSuppliers;

        private static bool _isEditWindowOpen = false;
        private int _currentRoleId;

        
        private string _currentFio;
        private string _currentRole;

        public MainMenuWindow(string role, string fio, int roleId)
        {
            InitializeComponent();
            _currentRoleId = roleId;
            _currentFio = fio;
            _currentRole = role;

          
            if (roleId == 4) 
            {
                TbUserRole.Text = "Гость";
            }
            else
            {
                TbUserRole.Text = $"Пользователь: {fio} | Роль: {role}";
            }

            
            if (roleId != 1 && roleId != 2)
            {
                FilterPanel.Visibility = Visibility.Collapsed;
            }

           
            if (roleId != 1)
            {
                AdminPanel.Visibility = Visibility.Collapsed;
            }

           
            if (roleId != 1 && roleId != 2)
            {
                BtnOrders.Visibility = Visibility.Collapsed;
            }

            LoadData();
            LoadManufacturers();
        }

        private void LoadData()
        {
            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    _allProducts = context.Products.ToList();
                    _allCategories = context.Categories.ToList();
                    _allManufacturers = context.Manufacturers.ToList();
                    _allSuppliers = context.Suppliers.ToList();
                }
                Filtr();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadManufacturers()
        {
            if (_allSuppliers == null) return;

            var displayList = new List<object>();
            displayList.Add(new { Id = 0, Name = "Все поставщики" });

            foreach (var s in _allSuppliers)
            {
                displayList.Add(new { Id = s.Id_Поставщика, Name = s.Наименование_Поставщика });
            }

            CmbManufacturer.ItemsSource = displayList;
            CmbManufacturer.DisplayMemberPath = "Name";
            CmbManufacturer.SelectedValuePath = "Id";
            CmbManufacturer.SelectedIndex = 0;
        }

        private void Filtr()
        {
            if (_allProducts == null) return;

            var filteredList = _allProducts.ToList();

            if (CmbManufacturer.SelectedValue != null && (int)CmbManufacturer.SelectedValue != 0)
            {
                int selectedManId = (int)CmbManufacturer.SelectedValue;
                filteredList = filteredList.Where(x => x.Id_Поставщика == selectedManId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                string searchText = TxtSearch.Text.ToLower();
                filteredList = filteredList.Where(x =>
                    (x.Наименование_Товара != null && x.Наименование_Товара.ToLower().Contains(searchText)) ||
                    (x.Описание_Товара != null && x.Описание_Товара.ToLower().Contains(searchText)) ||
                    (x.Артикул != null && x.Артикул.ToLower().Contains(searchText))
                ).ToList();
            }

            if (SortAsc.IsChecked == true)
                filteredList = filteredList.OrderBy(x => x.Кол_во_На_Складе).ToList();
            else if (SortDesc.IsChecked == true)
                filteredList = filteredList.OrderByDescending(x => x.Кол_во_На_Складе).ToList();

            var result = filteredList.Select(t => new
            {
                Артикул = t.Артикул,
                Наименование_Товара = t.Наименование_Товара,
                Описание_Товара = t.Описание_Товара,
                Единица_Измерения = t.Единица_Измерения,
                Цена = t.Цена,
                Действующая_Скидка = t.Действующая_Скидка,
                Кол_во_На_Складе = t.Кол_во_На_Складе,
                Фото = t.Фото,

                Наименование_Категории = _allCategories
                    .Where(c => c.Id_Категории == t.Id_Категории)
                    .Select(c => c.Наименование_Категории)
                    .FirstOrDefault() ?? "Без категории",

                Наименование_Производителя = _allManufacturers
                    .Where(m => m.Id_Производителя == t.Id_Производителя)
                    .Select(m => m.Наименование_Производителя)
                    .FirstOrDefault() ?? "Не указан",

                Наименование_Поставщика = _allSuppliers
                    .Where(s => s.Id_Поставщика == t.Id_Поставщика)
                    .Select(s => s.Наименование_Поставщика)
                    .FirstOrDefault() ?? "Не указан",

                is_discount = t.Действующая_Скидка > 15,
                is_on_sale = t.Действующая_Скидка > 0,
                is_quantity = t.Кол_во_На_Складе <= 0,
                price_discount = (t.Цена == null || t.Действующая_Скидка == null)
                    ? (decimal?)null
                    : t.Цена - (t.Цена * t.Действующая_Скидка / 100)
            }).ToList();

            ListTovar.ItemsSource = result;
        }

        
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => Filtr();
        private void Sort_Checked(object sender, RoutedEventArgs e) { if (SortAsc != null) Filtr(); }
        private void CmbManufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e) => Filtr();

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authWin = new AuthorizationWindow();
            authWin.Show();
            this.Close();
        }

        // ОТКРЫТИЕ ОКНА ЗАКАЗОВ
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWin = new OrdersWindow(_currentRoleId, _currentRole, _currentFio);
            ordersWin.ShowDialog();
        }

        // ДОБАВЛЕНИЕ ТОВАРА
        private void BtnAddTovar_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditWindowOpen = true;
            try
            {
                AddEditTovarWindow addWin = new AddEditTovarWindow(null);
                addWin.ShowDialog();
                LoadData();
            }
            finally
            {
                _isEditWindowOpen = false;
            }
        }

        // РЕДАКТИРОВАНИЕ ТОВАРА
        private void ListTovar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentRoleId != 1) return;

            if (_isEditWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ListTovar.SelectedItem == null) return;

            dynamic selected = ListTovar.SelectedItem;
            string article = selected.Артикул;

            using (var context = new ShoeStore_BondarevEntities())
            {
                var product = context.Products.FirstOrDefault(p => p.Артикул == article);
                if (product == null) return;

                _isEditWindowOpen = true;
                try
                {
                    AddEditTovarWindow editWin = new AddEditTovarWindow(product);
                    editWin.ShowDialog();
                    LoadData();
                }
                finally
                {
                    _isEditWindowOpen = false;
                }
            }
        }

        
        private void BtnDeleteTovar_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRoleId != 1) return;

            if (ListTovar.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = ListTovar.SelectedItem;
            string article = selected.Артикул;
            string name = selected.Наименование_Товара;

            if (MessageBox.Show($"Вы точно хотите удалить товар \"{name}\"?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    bool isInOrders = context.OrderItems.Any(oi => oi.Артикул == article);
                    if (isInOrders)
                    {
                        MessageBox.Show("Нельзя удалить товар, который присутствует в заказе!",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var product = context.Products.FirstOrDefault(p => p.Артикул == article);
                    if (product != null)
                    {
                        if (!string.IsNullOrWhiteSpace(product.Фото))
                        {
                            string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.Фото);
                            if (System.IO.File.Exists(fullPath))
                            {
                                try { System.IO.File.Delete(fullPath); } catch { }
                            }
                        }

                        context.Products.Remove(product);
                        context.SaveChanges();

                        MessageBox.Show("Товар успешно удалён!", "Успех",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}