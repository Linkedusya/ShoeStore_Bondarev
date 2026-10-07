using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ShoeStore_Bondarev
{
    public partial class AddEditTovarWindow : Window
    {
        private Product _currentProduct;
        private bool _isEditMode;
        private string _oldPhotoPath; 

        public AddEditTovarWindow(Product selectedProduct)
        {
            InitializeComponent();

            
            LoadComboBoxes();

            if (selectedProduct != null)
            {
                // Режим редактирования
                _isEditMode = true;
                _currentProduct = selectedProduct;
                _oldPhotoPath = selectedProduct.Фото;

                
                TbArticle.Text = _currentProduct.Артикул;
                TbArticle.IsReadOnly = true; // ID доступно только для чтения
                TbName.Text = _currentProduct.Наименование_Товара;
                TbDescription.Text = _currentProduct.Описание_Товара;
                TbPrice.Text = _currentProduct.Цена?.ToString();
                TbUnit.Text = _currentProduct.Единица_Измерения;
                TbQuantity.Text = _currentProduct.Кол_во_На_Складе?.ToString();
                TbDiscount.Text = _currentProduct.Действующая_Скидка?.ToString();

                CmbCategory.SelectedValue = _currentProduct.Id_Категории;
                CmbManufacturer.SelectedValue = _currentProduct.Id_Производителя;
                CmbSupplier.SelectedValue = _currentProduct.Id_Поставщика;

                
                if (!string.IsNullOrWhiteSpace(_currentProduct.Фото))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _currentProduct.Фото);
                    if (File.Exists(fullPath))
                    {
                        ImgTovar.Source = new BitmapImage(new Uri(fullPath));
                    }
                }
            }
            else
            {
                // Режим добавления
                _isEditMode = false;
                _currentProduct = new Product();

                
                TbArticle.Text = GenerateNewArticle();
                TbArticle.IsReadOnly = true; // ID не отображается и не редактируется
            }
        }

       
        private void LoadComboBoxes()
        {
            using (var context = new ShoeStore_BondarevEntities())
            {
                CmbCategory.ItemsSource = context.Categories.ToList();
                CmbManufacturer.ItemsSource = context.Manufacturers.ToList();
                CmbSupplier.ItemsSource = context.Suppliers.ToList();
            }
        }

        
        private string GenerateNewArticle()
        {
            using (var context = new ShoeStore_BondarevEntities())
            {
                var articles = context.Products
                    .Where(p => p.Артикул.StartsWith("А"))
                    .Select(p => p.Артикул)
                    .ToList();

                int maxNumber = 0;
                foreach (var art in articles)
                {
                    if (art.Length > 1 && int.TryParse(art.Substring(1), out int num))
                    {
                        if (num > maxNumber) maxNumber = num;
                    }
                }
                return "А" + (maxNumber + 1).ToString("D3");
            }
        }

        // фото
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            dialog.DefaultExt = ".jpg";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string sourcePath = dialog.FileName;
                    string fileName = Path.GetFileName(sourcePath);

                   
                    string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    string newPath = Path.Combine(targetDir, fileName);

                    
                    BitmapImage originalImage = new BitmapImage(new Uri(sourcePath));
                    var resizedImage = ResizeImage(originalImage, 300, 200);
                    SaveImage(resizedImage, newPath);

                    
                    if (!string.IsNullOrWhiteSpace(_oldPhotoPath))
                    {
                        string oldFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _oldPhotoPath);
                        if (File.Exists(oldFullPath))
                        {
                            try { File.Delete(oldFullPath); } catch { }
                        }
                    }

                   
                    ImgTovar.Source = new BitmapImage(new Uri(newPath));
                    _currentProduct.Фото = "Images\\" + fileName;
                    _oldPhotoPath = _currentProduct.Фото;

                    MessageBox.Show("Изображение успешно установлено!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки изображения: " + ex.Message, "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private BitmapSource ResizeImage(BitmapImage source, int width, int height)
        {
            var transformedBitmap = new TransformedBitmap(source,
                new System.Windows.Media.ScaleTransform(
                    width / (double)source.PixelWidth,
                    height / (double)source.PixelHeight));
            return transformedBitmap;
        }

       
        private void SaveImage(BitmapSource image, string path)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

       
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(TbName.Text))
            {
                MessageBox.Show("Введите наименование товара!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbCategory.SelectedItem == null || CmbManufacturer.SelectedItem == null || CmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию, производителя и поставщика!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal price;
            if (!decimal.TryParse(TbPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int quantity;
            if (!int.TryParse(TbQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int discount = 0;
            if (!string.IsNullOrWhiteSpace(TbDiscount.Text))
            {
                if (!int.TryParse(TbDiscount.Text, out discount) || discount < 0)
                {
                    MessageBox.Show("Скидка должна быть положительным числом!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (var context = new ShoeStore_BondarevEntities())
                {
                    if (_isEditMode)
                    {
                        // Редактирование
                        var productToUpdate = context.Products.FirstOrDefault(p => p.Артикул == _currentProduct.Артикул);
                        if (productToUpdate != null)
                        {
                            productToUpdate.Наименование_Товара = TbName.Text;
                            productToUpdate.Описание_Товара = TbDescription.Text;
                            productToUpdate.Единица_Измерения = TbUnit.Text;
                            productToUpdate.Цена = price;
                            productToUpdate.Кол_во_На_Складе = quantity;
                            productToUpdate.Действующая_Скидка = discount;
                            productToUpdate.Id_Категории = (int)CmbCategory.SelectedValue;
                            productToUpdate.Id_Производителя = (int)CmbManufacturer.SelectedValue;
                            productToUpdate.Id_Поставщика = (int)CmbSupplier.SelectedValue;
                            productToUpdate.Фото = _currentProduct.Фото;
                        }
                    }
                    else
                    {
                        // Добавление
                        _currentProduct.Артикул = TbArticle.Text;
                        _currentProduct.Наименование_Товара = TbName.Text;
                        _currentProduct.Описание_Товара = TbDescription.Text;
                        _currentProduct.Единица_Измерения = TbUnit.Text;
                        _currentProduct.Цена = price;
                        _currentProduct.Кол_во_На_Складе = quantity;
                        _currentProduct.Действующая_Скидка = discount;
                        _currentProduct.Id_Категории = (int)CmbCategory.SelectedValue;
                        _currentProduct.Id_Производителя = (int)CmbManufacturer.SelectedValue;
                        _currentProduct.Id_Поставщика = (int)CmbSupplier.SelectedValue;

                        context.Products.Add(_currentProduct);
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