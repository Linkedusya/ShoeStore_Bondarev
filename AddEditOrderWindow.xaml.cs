using System;
using System.Linq;
using System.Windows;

namespace ShoeStore_Bondarev
{
    public partial class AddEditOrderWindow : Window
    {
        private Order _currentOrder;
        private bool _isEditMode;
        private int _roleId;

        public AddEditOrderWindow(Order selectedOrder, int roleId)
        {
            InitializeComponent();
            _roleId = roleId;

            LoadComboBoxes();

            if (selectedOrder != null)
            {
                // Режим редактирования
                _isEditMode = true;
                _currentOrder = selectedOrder;

                TbArticle.Text = _currentOrder.Номер_Заказа.ToString();
                TbArticle.IsReadOnly = true;

                CmbStatus.SelectedValue = _currentOrder.IdStatus;
                CmbPickupPoint.SelectedValue = _currentOrder.Адрес_Пункта_Выдачи;
                DpOrderDate.SelectedDate = _currentOrder.Дата_Заказа;
                DpDeliveryDate.SelectedDate = _currentOrder.Дата_Доставки;
            }
            else
            {
                // Режим добавления
                _isEditMode = false;
                _currentOrder = new Order();

                
                TbArticle.Text = GenerateNewOrderNumber();
                TbArticle.IsReadOnly = true;

                DpOrderDate.SelectedDate = DateTime.Now;
            }
        }

        
        private void LoadComboBoxes()
        {
            using (var context = new ShoeStore_BondarevEntities())
            {
                CmbStatus.ItemsSource = context.Statuses.ToList();

                
                var pickupPoints = context.PickupPoints.ToList()
                    .Select(p => new
                    {
                        Id_Пункта = p.Id_Пункта,
                        Адрес = p.Город + ", " + p.Улица + ", " + p.Дом
                    }).ToList();

                CmbPickupPoint.ItemsSource = pickupPoints;
            }
        }

        
        private string GenerateNewOrderNumber()
        {
            using (var context = new ShoeStore_BondarevEntities())
            {
                var maxNumber = context.Orders.Any()
                    ? context.Orders.Max(o => o.Номер_Заказа)
                    : 0;
                return (maxNumber + 1).ToString();
            }
        }

        
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
           
            if (CmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус заказа!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbPickupPoint.SelectedItem == null)
            {
                MessageBox.Show("Выберите пункт выдачи!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    if (_isEditMode)
                    {
                        // Редактирование
                        var orderToUpdate = context.Orders.FirstOrDefault(o => o.Номер_Заказа == _currentOrder.Номер_Заказа);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.IdStatus = (int)CmbStatus.SelectedValue;
                            orderToUpdate.Адрес_Пункта_Выдачи = (int)CmbPickupPoint.SelectedValue;
                            orderToUpdate.Дата_Заказа = DpOrderDate.SelectedDate;
                            orderToUpdate.Дата_Доставки = DpDeliveryDate.SelectedDate;
                        }
                    }
                    else
                    {
                        // Добавление
                        _currentOrder.Номер_Заказа = int.Parse(TbArticle.Text);
                        _currentOrder.IdStatus = (int)CmbStatus.SelectedValue;
                        _currentOrder.Адрес_Пункта_Выдачи = (int)CmbPickupPoint.SelectedValue;
                        _currentOrder.Дата_Заказа = DpOrderDate.SelectedDate;
                        _currentOrder.Дата_Доставки = DpDeliveryDate.SelectedDate;

                        context.Orders.Add(_currentOrder);
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные сохранены!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}