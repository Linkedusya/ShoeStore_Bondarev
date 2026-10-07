using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShoeStore_Bondarev
{
    public partial class OrdersWindow : Window
    {
        private int _currentRoleId;
        private static bool _isEditOrderWindowOpen = false;

        public OrdersWindow(int roleId, string role, string fio)
        {
            InitializeComponent();
            _currentRoleId = roleId;

            // Отображение роли
            if (roleId == 4)
                TbUserRole.Text = "Гость";
            else
                TbUserRole.Text = $"Пользователь: {fio} | Роль: {role}";

            // Панель администратора — только для админа (ID=1)
            if (roleId != 1)
            {
                AdminPanel.Visibility = Visibility.Collapsed;
            }

            LoadOrders();
        }

        // Загрузка списка заказов
        private void LoadOrders()
        {
            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    var orders = context.Orders.ToList();
                    var statuses = context.Statuses.ToList();
                    var pickupPoints = context.PickupPoints.ToList();

                    var result = orders.Select(o => new
                    {
                        Номер_Заказа = o.Номер_Заказа,
                        Артикул_Заказа = o.Номер_Заказа,
                        Статус_Заказа = statuses
                            .Where(s => s.IdStatus == o.IdStatus)
                            .Select(s => s.Наименование_Статуса)
                            .FirstOrDefault() ?? "Не указан",
                        Адрес_Пункта_Выдачи = pickupPoints
                            .Where(p => p.Id_Пункта == o.Адрес_Пункта_Выдачи)
                            .Select(p => p.Город + ", " + p.Улица + ", " + p.Дом)
                            .FirstOrDefault() ?? "Не указан",
                        Дата_Заказа = o.Дата_Заказа,
                        Дата_Доставки = o.Дата_Доставки
                    }).ToList();

                    ListOrders.ItemsSource = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Назад
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Добавить заказ
        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditOrderWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditOrderWindowOpen = true;
            try
            {
                AddEditOrderWindow addWin = new AddEditOrderWindow(null, _currentRoleId);
                addWin.ShowDialog();
                LoadOrders();
            }
            finally
            {
                _isEditOrderWindowOpen = false;
            }
        }

        // Редактирование (двойной клик)
        private void ListOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentRoleId != 1) return; // Только админ

            if (_isEditOrderWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ListOrders.SelectedItem == null) return;

            dynamic selected = ListOrders.SelectedItem;
            int orderNumber = selected.Номер_Заказа;

            using (var context = new ShoeStore_BondarevEntities())
            {
                var order = context.Orders.FirstOrDefault(o => o.Номер_Заказа == orderNumber);
                if (order == null) return;

                _isEditOrderWindowOpen = true;
                try
                {
                    AddEditOrderWindow editWin = new AddEditOrderWindow(order, _currentRoleId);
                    editWin.ShowDialog();
                    LoadOrders();
                }
                finally
                {
                    _isEditOrderWindowOpen = false;
                }
            }
        }

        // Удаление заказа
        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRoleId != 1) return; // Только админ

            if (ListOrders.SelectedItem == null)
            {
                MessageBox.Show("Выберите заказ для удаления!", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = ListOrders.SelectedItem;
            int orderNumber = selected.Номер_Заказа;

            if (MessageBox.Show($"Вы точно хотите удалить заказ №{orderNumber}?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    // Проверка: есть ли в заказе товары
                    bool hasItems = context.OrderItems.Any(oi => oi.Номер_Заказа == orderNumber);
                    if (hasItems)
                    {
                        MessageBox.Show("Нельзя удалить заказ, в котором есть товары!",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var order = context.Orders.FirstOrDefault(o => o.Номер_Заказа == orderNumber);
                    if (order != null)
                    {
                        context.Orders.Remove(order);
                        context.SaveChanges();

                        MessageBox.Show("Заказ успешно удалён!", "Успех",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders();
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