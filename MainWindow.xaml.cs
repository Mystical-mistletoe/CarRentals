using CarRentals.Data;
using CarRentals.Enums;
using CarRentals.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarRentals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDbContext db; //будет создано в конструкторе

        public MainWindow()
        {
            InitializeComponent();
            db = new AppDbContext();
            db.Database.EnsureCreated(); //что бд существует (или создает)
            cmbCarStatus.IsEnabled = false; //отключение, статус меняется автоматически

            this.Title = $"Аренда авто - {CurrentAccount.CurAccount.Role})";

            // Подписываемся на событие выбора бронирования для штрафа
            cmbBookingForFine.SelectionChanged += CmbBookingForFine_SelectionChanged;

            // Подписываемся на события для автоматического пересчёта
            dpStartDateTime.ValueChanged += DpStartDateTime_ValueChanged;
            dpEndDateTime.ValueChanged += DpEndDateTime_ValueChanged;
            cmbCarForBooking.SelectionChanged += CmbCarForBooking_SelectionChanged;

            // Менеджер может добавлять бронирования и штрафы
            if (CurrentAccount.CurAccount.Role == "manager")
            {

                btnAddCar.IsEnabled = false;
                btnUpdateCar.IsEnabled = false;
                btnDeleteCar.IsEnabled = false;
                btnAddUser.IsEnabled = false;
                btnUpdateUser.IsEnabled = false;
                btnDeleteUser.IsEnabled = false;

                MessageBox.Show("Добро пожаловать!");
            }
            else
            {
                MessageBox.Show("Админ: полный доступ!");
            }

            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadCars();
            LoadUserComboBox();
            LoadUsers();
            LoadCarComboBoxForBooking();
            LoadBookings();
            LoadFines();
            LoadStatistics();
        }

        /// <summary>
        /// АВТОМОБИЛИ
        /// </summary>
        private void LoadCars()
        {
            db.Cars.Load();
            lstCars.ItemsSource = db.Cars.Local.ToObservableCollection();
            lstCars.DisplayMemberPath = "Brand"; //что показывать в списке
        }

        private void UpdateCarStatusBasedOnBookings(int carId)
        {
            //проверка активных бронирований
            bool hasActiveBooking = db.Bookings.Any(b =>
                b.CarId == carId &&
                b.StatusBooking == BookingStatus.Active);

            // находим автомобиль
            var car = db.Cars.Find(carId);
            if (car != null)
            {
                if (hasActiveBooking)
                {
                    car.StatusCar = CarStatus.Rented;  // Арендован
                }
                else
                {
                    car.StatusCar = CarStatus.Available;  // Доступен
                }

                db.SaveChanges();
            }
        }

        private void LstCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCars.SelectedItem is Car selected)
            {
                txtBrand.Text = selected.Brand;
                txtModel.Text = selected.Model;
                txtYearCar.Text = selected.YearCar.ToString();
                txtLicensePlate.Text = selected.LicensePlate;
                txtPricePerHour.Text = selected.PricePerHour.ToString();
                txtPricePerDay.Text = selected.PricePerDay.ToString();

                cmbTransmissionType.SelectedIndex = selected.TransmissionType == TransmissionType.Automatic ? 1 : 0;
                cmbCarStatus.SelectedIndex = (int)selected.StatusCar - 1;
            }
        }

        private void BtnRefreshCars_Click(object sender, RoutedEventArgs e)
        {
            LoadCars();
            ClearCarFields();
        }

        private void BtnAddCar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBrand.Text))
            {
                MessageBox.Show("Введите марку автомобиля");
                return;
            }

            if (!int.TryParse(txtYearCar.Text, out int yearr) || yearr < 1900 || yearr > DateTime.Now.Year)
            {
                MessageBox.Show($"Год выпуска должен быть от 1900 до {DateTime.Now.Year}");
                return;
            }
            if (db.Cars.Any(c => c.LicensePlate == txtLicensePlate.Text))
            {
                MessageBox.Show("Автомобиль с таким номерным знаком уже существует");
                return;
            }

            var car = new Car
            {
                Brand = txtBrand.Text,
                Model = txtModel.Text,
                YearCar = int.TryParse(txtYearCar.Text, out int year) ? year : 0,
                LicensePlate = txtLicensePlate.Text,
                TransmissionType = cmbTransmissionType.SelectedIndex == 1 ? TransmissionType.Automatic : TransmissionType.Manual,
                PricePerHour = decimal.TryParse(txtPricePerHour.Text, out decimal priceHour) ? priceHour : 0,
                PricePerDay = decimal.TryParse(txtPricePerDay.Text, out decimal priceDay) ? priceDay : 0,
                StatusCar = (CarStatus)(cmbCarStatus.SelectedIndex + 1),
            };

            db.Cars.Add(car);
            db.SaveChanges();
            LoadCars();
            LoadCarComboBoxForBooking();
            ClearCarFields();
            MessageBox.Show("Автомобиль добавлен");
        }

        private void BtnUpdateCar_Click(object sender, RoutedEventArgs e)
        {
            if (lstCars.SelectedItem is Car selected)
            {
                if (!int.TryParse(txtYearCar.Text, out int yearr) || yearr < 1900 || yearr > DateTime.Now.Year)
                {
                    MessageBox.Show($"Год выпуска должен быть от 1900 до {DateTime.Now.Year}");
                    return;
                }
                selected.Brand = txtBrand.Text;
                selected.Model = txtModel.Text;
                selected.YearCar = int.TryParse(txtYearCar.Text, out int year) ? year : 0;
                selected.LicensePlate = txtLicensePlate.Text;
                selected.TransmissionType = cmbTransmissionType.SelectedIndex == 1 ? TransmissionType.Automatic : TransmissionType.Manual;
                selected.PricePerHour = decimal.TryParse(txtPricePerHour.Text, out decimal priceHour) ? priceHour : 0;
                selected.PricePerDay = decimal.TryParse(txtPricePerDay.Text, out decimal priceDay) ? priceDay : 0;
                selected.StatusCar = (CarStatus)(cmbCarStatus.SelectedIndex + 1);

                db.Cars.Update(selected);
                db.SaveChanges();

                LoadUserComboBox();
                LoadCarComboBoxForBooking();
                LoadStatistics();

                LoadCars();
                MessageBox.Show("Автомобиль обновлен");
            }
            else
            {
                MessageBox.Show("Выберите автомобиль для обновления");
            }
        }

        private void BtnDeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (lstCars.SelectedItem is Car selected)
            {
                //связанные бронирования
                var hasBookings = db.Bookings.Any(b => b.CarId == selected.CarId);

                if (hasBookings)
                {
                    MessageBox.Show($"Невозможно удалить автомобиль {selected.Brand} {selected.Model}.\n" +
                                   "Сначала удалите все бронирования, связанные с этим автомобилем.",
                                   "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Удалить автомобиль {selected.Brand} {selected.Model}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Cars.Remove(selected);
                    db.SaveChanges();
                    LoadCars();
                    LoadUserComboBox();
                    LoadCarComboBoxForBooking();
                    LoadStatistics();
                    ClearCarFields();
                    MessageBox.Show("Автомобиль удален");
                }
            }
        }

        private void ClearCarFields()
        {
            txtBrand.Text = "";
            txtModel.Text = "";
            txtYearCar.Text = "";
            txtLicensePlate.Text = "";
            txtPricePerHour.Text = "";
            txtPricePerDay.Text = "";
            cmbTransmissionType.SelectedIndex = -1;
            cmbCarStatus.SelectedIndex = -1;
            cmbCarStatus.IsEnabled = false;
        }

        /// <summary>
        /// ПОЛЬЗОВАТЕЛЬ
        /// </summary>
        private void LoadUsers()
        {
            var users = db.Users.ToList();
            lstUsers.ItemsSource = users;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (!email.Contains("@")) return false;

            string[] parts = email.Split('@');
            if (parts.Length != 2) return false;

            string domain = parts[1];
            return domain == "yandex.ru" || domain == "gmail.com";
        }

        private void LstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstUsers.SelectedItem is User selected)
            {
                txtUserId.Text = selected.UserId.ToString();
                txtLastName.Text = selected.LastName;
                txtEmail.Text = selected.Email;

                //количество бронирований
                int bookingsCount = db.Bookings.Count(b => b.UserId == selected.UserId);
                txtBookingsCount.Text = bookingsCount.ToString();

                //сумма неоплаченных штрафов
                decimal totalFines = db.Bookings
                    .Where(b => b.UserId == selected.UserId)
                    .SelectMany(b => b.Fines)
                    .Where(f => f.StatusFine == FineStatus.Unpaid)
                    .Sum(f => (decimal?)f.Amount) ?? 0;
                txtTotalFines.Text = totalFines.ToString("F2") + " ₽";
            }
        }

        private void BtnRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            ClearUserFields();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Введите email пользователя");
                return;
            }

            //Проверка на уникальность email
            if (db.Users.Any(u => u.Email == txtEmail.Text))
            {
                MessageBox.Show("Пользователь с таким email уже существует");
                return;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email должен содержать @ и оканчиваться на yandex.ru или gmail.com");
                return;
            }

            var user = new User
            {
                LastName = txtLastName.Text,
                Email = txtEmail.Text
            };

            db.Users.Add(user);
            db.SaveChanges();

            LoadUsers();
            LoadUserComboBox();
            ClearUserFields();

            MessageBox.Show("Пользователь добавлен");
        }

        private void BtnUpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem is User selected)
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Введите фамилию пользователя");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Введите email пользователя");
                    return;
                }

                //исключая тек польз
                if (db.Users.Any(u => u.Email == txtEmail.Text && u.UserId != selected.UserId))
                {
                    MessageBox.Show("Пользователь с таким email уже существует");
                    return;
                }

                if (!IsValidEmail(txtEmail.Text))
                {
                    MessageBox.Show("Email должен содержать @ и оканчиваться на yande.ru или gmail.com");
                    return;
                }

                selected.LastName = txtLastName.Text;
                selected.Email = txtEmail.Text;

                db.Users.Update(selected);
                db.SaveChanges();

                LoadUsers();
                LoadUserComboBox(); 
                ClearUserFields();

                MessageBox.Show("Пользователь обновлен");
            }
            else
            {
                MessageBox.Show("Выберите пользователя для обновления");
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem is User selected)
            {
                // есть ли бронирования
                bool hasBookings = db.Bookings.Any(b => b.UserId == selected.UserId);

                if (hasBookings)
                {
                    MessageBox.Show($"Невозможно удалить пользователя {selected.LastName}.\n" +
                                   "Сначала удалите все бронирования, связанные с этим пользователем.",
                                   "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Удалить пользователя {selected.LastName}?", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Users.Remove(selected);
                    db.SaveChanges();

                    LoadUsers();
                    LoadUserComboBox();
                    ClearUserFields();

                    MessageBox.Show("Пользователь удален");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления");
            }
        }

        private void ClearUserFields()
        {
            txtUserId.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtBookingsCount.Text = "";
            txtTotalFines.Text = "";
        }

        /// <summary>
        /// БРОНИРОВАНИЕ
        /// </summary>
        //бронирование
        private void LoadBookings()
        {
            db.Bookings.Include(b => b.Car).Include(b => b.User).Load(); //загруж связанных
            var bookingsList = db.Bookings.Local.ToObservableCollection();
            lstBookings.ItemsSource = bookingsList;
            lstBookings.DisplayMemberPath = "DisplayName";
        }

        private void LstBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstBookings.SelectedItem is Booking selected)
            {
                if (cmbUserForBooking.ItemsSource != null)
                {
                    cmbUserForBooking.SelectedValue = selected.UserId;
                }
                cmbCarForBooking.SelectedValue = selected.CarId;

                dpStartDateTime.Value = selected.StartDateTime;
                dpEndDateTime.Value = selected.EndDateTime;
                txtTotalPrice.Text = selected.TotalPrice.ToString();
                txtMileage.Text = selected.Mileage?.ToString();
                cmbBookingStatus.SelectedIndex = (int)selected.StatusBooking;
            }
        }

        private void BtnRefreshBookings_Click(object sender, RoutedEventArgs e)
        {
            LoadBookings();
            ClearBookingFields();
        }

        /// <summary>
        /// Проверяет, свободен ли автомобиль на указанный период
        /// </summary>
        private bool IsCarAvailableForBooking(int carId, DateTime startDate, 
            DateTime endDate, int? excludeBookingId = null)
        {
            // Проверяем, есть ли активные бронирования, которые пересекаются с указанным периодом
            var conflictingBookings = db.Bookings.Where(b =>
                b.CarId == carId &&
                b.StatusBooking == BookingStatus.Active && // только активные бронирования
                b.BookingId != excludeBookingId && // исключаем текущее бронирование при обновлении
                ((startDate >= b.StartDateTime && startDate < b.EndDateTime) || // новое начинается внутри существующего
                 (endDate > b.StartDateTime && endDate <= b.EndDateTime) ||     // новое заканчивается внутри существующего
                 (startDate <= b.StartDateTime && endDate >= b.EndDateTime)));  // новое полностью перекрывает существующее

            return !conflictingBookings.Any();
        }

        /// <summary>
        /// Рассчитывает стоимость аренды в зависимости от длительности (поминутно) и цен автомобиля
        /// Минимальное время аренды — 30 минут
        /// </summary>
        private decimal CalculateRentalCost(int carId, DateTime startDate, DateTime endDate)
        {
            // Находим автомобиль в базе данных
            var car = db.Cars.Find(carId);
            if (car == null) return 0;

            // Проверяем, что дата окончания позже даты начала
            if (endDate <= startDate) return 0;

            // Вычисляем длительность аренды в минутах
            TimeSpan duration = endDate - startDate;
            double totalMinutes = duration.TotalMinutes;

            // Минимальная длительность аренды — 20 минут
            if (totalMinutes < 20)
            {
                MessageBox.Show("Минимальное время аренды — 20 минут", "Предупреждение",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return 0;
            }

            // Округляем минуты до целого значения
            int minutesToPay = (int)Math.Ceiling(totalMinutes);

            // Рассчитываем стоимость: (минуты / 60) * цена_за_час
            // Но точнее: цена_за_час / 60 * минуты
            decimal pricePerMinute = car.PricePerHour / 60;
            decimal totalCost = pricePerMinute * minutesToPay;

            // Проверяем, не выгоднее ли посуточный тариф
            // Если аренда больше 23 часов, возможно, сутки будут дешевле
            if (totalMinutes >= 23 * 60) // 23 часа и более
            {
                int totalDays = (int)Math.Floor(totalMinutes / (24 * 60));
                double remainingMinutes = totalMinutes % (24 * 60);

                decimal dailyCost = totalDays * car.PricePerDay;

                if (remainingMinutes > 0)
                {
                    decimal remainingCost = (car.PricePerHour / 60) * (decimal)remainingMinutes;
                    dailyCost += remainingCost;
                }

                // Если посуточный тариф выгоднее — используем его
                if (dailyCost < totalCost)
                    totalCost = dailyCost;
            }

            // Округляем до двух знаков после запятой
            return Math.Round(totalCost, 2);
        }

        /// <summary>
        /// Основной метод пересчёта стоимости (вызывается при изменении дат или выборе авто)
        /// </summary>
        private void RecalculateTotalPrice()
        {
            // Проверяем, выбран ли автомобиль
            if (cmbCarForBooking.SelectedValue == null)
            {
                txtTotalPrice.Text = "";
                return;
            }

            // Получаем дату и время из DateTimePicker
            DateTime? startDateTime = dpStartDateTime.Value;
            DateTime? endDateTime = dpEndDateTime.Value;

            // Проверяем, что даты указаны
            if (startDateTime == null || endDateTime == null)
            {
                txtTotalPrice.Text = "";
                return;
            }

            // Проверяем, что дата окончания позже даты начала
            if (endDateTime.Value <= startDateTime.Value)
            {
                txtTotalPrice.Text = "Ошибка: дата окончания должна быть позже даты начала";
                return;
            }

            // Проверяем минимальную длительность (20 минут)
            TimeSpan duration = endDateTime.Value - startDateTime.Value;
            if (duration.TotalMinutes < 20)
            {
                txtTotalPrice.Text = "Ошибка: минимальное время аренды — 20 минут";
                return;
            }

            // Получаем ID автомобиля
            int carId = (int)cmbCarForBooking.SelectedValue;

            // Рассчитываем стоимость
            decimal totalPrice = CalculateRentalCost(carId, startDateTime.Value, endDateTime.Value);

            // Выводим в поле
            txtTotalPrice.Text = totalPrice.ToString("F2") + " ₽";
        }
        /// <summary>
        /// Пересчёт стоимости при изменении даты и времени начала аренды
        /// </summary>
        private void DpStartDateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RecalculateTotalPrice();
        }

        /// <summary>
        /// Пересчёт стоимости при изменении даты и времени окончания аренды
        /// </summary>
        private void DpEndDateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RecalculateTotalPrice();
        }

        /// <summary>
        /// Пересчёт стоимости при выборе другого автомобиля
        /// </summary>
        private void CmbCarForBooking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateTotalPrice();
        }

        private void BtnAddBooking_Click(object sender, RoutedEventArgs e)
        {

                if (cmbUserForBooking.SelectedValue == null)
                {
                    MessageBox.Show("Выберите пользователя");
                    return;
                }

            if (cmbCarForBooking.SelectedValue == null)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            DateTime startDate = dpStartDateTime.Value ?? DateTime.Now;
            DateTime endDate = dpEndDateTime.Value ?? DateTime.Now;

            if (endDate <= startDate )
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала");
                return;
            }

            // Проверка: минимальная длительность 20 минут
            TimeSpan duration = endDate - startDate;
            if (duration.TotalMinutes < 20)
            {
                MessageBox.Show("Минимальное время аренды — 20 минут. Выберите больший интервал.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int carId = (int)cmbCarForBooking.SelectedValue;

            // НОВАЯ ПРОВЕРКА: свободен ли автомобиль на выбранное время
            if (!IsCarAvailableForBooking(carId, startDate, endDate))
            {
                MessageBox.Show("Этот автомобиль уже забронирован на выбранное время!\n\n" +
                                "Автомобиль нельзя забронировать, так как у него есть активное бронирование в этот период.",
                                "Ошибка бронирования",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Рассчитываем стоимость
            decimal totalPrice = CalculateRentalCost(carId, startDate, endDate);

            // Если расчёт вернул 0 из-за ошибки валидации — прерываем
            if (totalPrice == 0 && duration.TotalMinutes >= 30)
            {
                MessageBox.Show("Не удалось рассчитать стоимость. Проверьте цены автомобиля.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var booking = new Booking
                {
                    UserId = (int)cmbUserForBooking.SelectedValue,
                    CarId = (int)cmbCarForBooking.SelectedValue,
                    StartDateTime = dpStartDateTime.Value ?? DateTime.Now,
                    EndDateTime = dpEndDateTime.Value ?? DateTime.Now,
                    TotalPrice = totalPrice,
                    StatusBooking = (BookingStatus)cmbBookingStatus.SelectedIndex,
                    Mileage = int.TryParse(txtMileage.Text, out int mileage) ? mileage : null,
                };

                db.Bookings.Add(booking);
                db.SaveChanges();
                LoadBookings();
                LoadFines(); //исправленный вариант. В прошлом разу не отображались актуально
                LoadStatistics();
                UpdateCarStatusBasedOnBookings(booking.CarId);
                ClearBookingFields();

                MessageBox.Show("Бронирование добавлено");

        }

        private void BtnUpdateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (lstBookings.SelectedItem is Booking selected)
            {
                if (cmbCarForBooking.SelectedItem is Car selectedCar)
                {
                    selected.CarId = selectedCar.CarId;
                }

                int oldCarId = selected.CarId;
                int newCarId = selected.CarId;

                DateTime startDate = dpStartDateTime.Value ?? DateTime.Now;
                DateTime endDate = dpEndDateTime.Value ?? DateTime.Now;

                if (endDate <= startDate)
                {
                    MessageBox.Show("Дата окончания должна быть позже даты начала");
                    return;
                }
                // Проверка: минимальная длительность 20 минут
                TimeSpan duration = endDate - startDate;
                if (duration.TotalMinutes < 20)
                {
                    MessageBox.Show("Минимальное время аренды — 20 минут. Выберите больший интервал.");
                    return;
                }

                // свободен ли автомобиль на выбранное время(исключая текущее бронирование)
                if (!IsCarAvailableForBooking(newCarId, startDate, endDate, selected.BookingId))
                {
                    MessageBox.Show("Этот автомобиль уже забронирован на выбранное время!\n\n" +
                                    "Изменения не могут быть сохранены, так как автомобиль занят.",
                                    "Ошибка обновления",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                selected.StartDateTime = dpStartDateTime.Value ?? DateTime.Now;
                selected.EndDateTime = dpEndDateTime.Value ?? DateTime.Now;
                selected.TotalPrice = CalculateRentalCost(newCarId, startDate, endDate);
                selected.StatusBooking = (BookingStatus)cmbBookingStatus.SelectedIndex;
                selected.Mileage = int.TryParse(txtMileage.Text, out int mileage) ? mileage : null;


                db.Bookings.Update(selected);
                db.SaveChanges();

                LoadBookings();

                LoadBookings();
                UpdateCarStatusBasedOnBookings(oldCarId);
                if (oldCarId != newCarId)
                {
                    UpdateCarStatusBasedOnBookings(newCarId);
                }

                LoadFines();
                LoadStatistics();
                LoadCars();

                MessageBox.Show("Бронирование обновлено");
            }
            else
            {
                MessageBox.Show("Выберите бронирование для обновления");
            }
        }

        private void BtnDeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (lstBookings.SelectedItem is Booking selected)
            {
                // Проверяем наличие связанных штрафов
                var hasFines = db.Fines.Any(f => f.BookingId == selected.BookingId);

                if (hasFines)
                {
                    MessageBox.Show($"Невозможно удалить бронирование #{selected.BookingId}.\n" +
                                   "Сначала удалите все связанные с ним штрафы.",
                                   "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Удалить бронирование #{selected.BookingId}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Bookings.Remove(selected);
                    db.SaveChanges();
                    LoadBookings();
                    LoadStatistics();
                    LoadFines();
                    ClearBookingFields();
                    MessageBox.Show("Бронирование удалено");
                }
            }
        }

        private void ClearBookingFields()
        {
            cmbUserForBooking.SelectedIndex = -1;
            cmbCarForBooking.SelectedIndex = -1;
            dpStartDateTime.Value = null;
            dpEndDateTime.Value = null;
            txtTotalPrice.Text = "";
            txtMileage.Text = "";
            txtCancellationReason.Text = "";
            cmbBookingStatus.SelectedIndex = -1;
        }

        //штрафы
        private void LoadFines()
        {
            db.Fines.Include(f => f.Booking).ThenInclude(b => b.Car).Load();
            lstFines.ItemsSource = db.Fines.Local.ToObservableCollection();

            lstFines.DisplayMemberPath = "DisplayName";
            LoadComboBoxesForFines();
        }


        private void LstFines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFines.SelectedItem is Fine selected)
            {
                cmbFineType.SelectedIndex = (int)selected.FineType;
                txtAmount.Text = selected.Amount.ToString();
                txtReason.Text = selected.Reason;
                dpIssueDate.Value = selected.IssueDate;
                dpPaymentDate.Value = selected.PaymentDate;
                cmbFineStatus.SelectedIndex = (int)selected.StatusFine;

                if (selected.Booking != null)
                {
                    cmbBookingForFine.SelectedValue = selected.BookingId;
                }
            }
        }

        private void BtnRefreshFines_Click(object sender, RoutedEventArgs e)
        {
            LoadFines();
            ClearFineFields();
        }

        private void CmbBookingForFine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBookingForFine.SelectedValue != null)
            {
                int bookingId = (int)cmbBookingForFine.SelectedValue;
                var booking = db.Bookings
                    .Include(b => b.Car)
                    .FirstOrDefault(b => b.BookingId == bookingId);

                if (booking != null)
                {
                    // Теперь показываем ДАТУ и ВРЕМЯ
                    txtBookingPeriod.Text = $"Автомобиль: {booking.Car?.Brand} {booking.Car?.Model}\n" +
                                            $"Начало бронирования: {booking.StartDateTime:dd.MM.yyyy HH:mm}\n" +
                                            $"Конец бронирования: {booking.EndDateTime:dd.MM.yyyy HH:mm}\n" +
                                            $"Штраф можно выписать только в этот период";
                }
            }
            else
            {
                txtBookingPeriod.Text = "";
            }
        }
        
        /// <summary>
        /// Проверяет, находится ли дата штрафа в пределах периода бронирования (с учётом времени)
        /// </summary>
        
        private bool IsFineDateWithinBookingPeriod(DateTime fineDate, 
            DateTime bookingStart, DateTime bookingEnd)
        {
            // Штраф должен быть выписан в период с начала до окончания бронирования (включительно)
            return fineDate >= bookingStart && fineDate <= bookingEnd;
        }

        private void BtnAddFine_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBookingForFine.SelectedValue == null)
            {
                MessageBox.Show("Выберите бронирование");
                return;
            }

            int bookingId = (int)cmbBookingForFine.SelectedValue;

            // получение DateTime из DateTimePicker (Value)
            DateTime issueDate = dpIssueDate.Value ?? DateTime.Now;
            DateTime? paymentDate = dpPaymentDate.Value;

            //получение периода бронирования
            var booking = db.Bookings.Find(bookingId);
            if (booking == null)
            {
                MessageBox.Show("Бронирование не найдено");
                return;
            }

            //дата штрафа должна быть в пределах периода бронирования (по ДАТАМ)
            if (!IsFineDateWithinBookingPeriod(issueDate, booking.StartDateTime, booking.EndDateTime))
            {
                MessageBox.Show(
                    $"Штраф можно выписать только в период действия бронирования!\n\n" +
                    $"Период бронирования: {booking.StartDateTime:dd.MM.yyyy HH:mm} - {booking.EndDateTime:dd.MM.yyyy HH:mm}\n" +
                    $"Дата выписки штрафа: {issueDate:dd.MM.yyyy HH:mm}\n\n" +
                    $"Штраф должен быть выписан не раньше начала и не позже окончания бронирования.",
                    "Ошибка валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (paymentDate.HasValue && paymentDate.Value.Date < issueDate.Date)
            {
                MessageBox.Show("Дата оплаты должна быть позже даты выписки штрафа");
                return;
            }

            var fine = new Fine
            {
                FineType = (FineType)cmbFineType.SelectedIndex,
                Amount = decimal.TryParse(txtAmount.Text, out decimal amount) ? amount : 0,
                Reason = txtReason.Text,
                IssueDate = issueDate,           // Используем Value
                PaymentDate = paymentDate,       // Используем Value
                StatusFine = (FineStatus)cmbFineStatus.SelectedIndex,
                BookingId = bookingId,
                CreatedAt = DateTime.Now
            };

            db.Fines.Add(fine);
            db.SaveChanges();
            LoadFines();
            LoadUsers();
            LoadStatistics();
            ClearFineFields();
            MessageBox.Show("Штраф добавлен");
        }

        private void BtnUpdateFine_Click(object sender, RoutedEventArgs e)
        {
            if (lstFines.SelectedItem is Fine selected)
            {
                int newBookingId = selected.BookingId;
                if (cmbBookingForFine.SelectedValue != null)
                    newBookingId = (int)cmbBookingForFine.SelectedValue;

                DateTime issueDate = dpIssueDate.Value ?? DateTime.Now;
                DateTime? paymentDate = dpPaymentDate.Value;

                // Проверка периода бронирования
                var booking = db.Bookings.Find(newBookingId);
                if (booking == null)
                {
                    MessageBox.Show("Выбранное бронирование не найдено");
                    return;
                }

                if (!IsFineDateWithinBookingPeriod(issueDate, booking.StartDateTime, booking.EndDateTime))
                {
                    MessageBox.Show(
                        $"Штраф можно выписать только в период действия бронирования!\n\n" +
                        $"Период бронирования: {booking.StartDateTime:dd.MM.yyyy HH:mm} - {booking.EndDateTime:dd.MM.yyyy HH:mm}\n" +
                        $"Дата выписки штрафа: {issueDate:dd.MM.yyyy HH:mm}",
                        "Ошибка валидации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (paymentDate.HasValue && paymentDate.Value.Date < issueDate.Date)
                {
                    MessageBox.Show("Дата оплаты должна быть позже даты выписки штрафа");
                    return;
                }

                selected.FineType = (FineType)cmbFineType.SelectedIndex;
                selected.Amount = decimal.TryParse(txtAmount.Text, out decimal amount) ? amount : 0;
                selected.Reason = txtReason.Text;
                selected.IssueDate = issueDate;
                selected.PaymentDate = paymentDate;
                selected.StatusFine = (FineStatus)cmbFineStatus.SelectedIndex;
                selected.BookingId = newBookingId;

                db.Fines.Update(selected);
                db.SaveChanges();
                LoadFines();
                LoadUsers();
                LoadStatistics();
                MessageBox.Show("Штраф обновлен");
            }
            else
            {
                MessageBox.Show("Выберите штраф для обновления");
            }
        }

        private void BtnDeleteFine_Click(object sender, RoutedEventArgs e)
        {
            if (lstFines.SelectedItem is Fine selected)
            {
                if (MessageBox.Show($"Удалить штраф #{selected.FineId}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Fines.Remove(selected);
                    db.SaveChanges();
                    LoadFines();
                    LoadStatistics();
                    ClearFineFields();
                    MessageBox.Show("Штраф удален");
                }
            }
        }

        private void ClearFineFields()
        {
            txtAmount.Text = "";
            txtReason.Text = "";
            dpIssueDate.Value = null;
            dpPaymentDate.Value = null;
            cmbFineType.SelectedIndex = -1;
            cmbFineStatus.SelectedIndex = -1;
        }

        //статистика
        private void LoadStatistics()
        {
            var stats = db.Cars
                .Select(c => new
                {
                    c.CarId,
                    CarName = $"{c.Brand} {c.Model}",
                    BookingsCount = db.Bookings.Count(b => b.CarId == c.CarId),
                    TotalRevenue = db.Bookings.Where(b => b.CarId == c.CarId).Sum(b => b.TotalPrice),
                    FinesCount = db.Fines.Count(f => f.Booking.CarId == c.CarId),
                    TotalFinesAmount = db.Fines.Where(f => f.Booking.CarId == c.CarId).Sum(f => f.Amount)
                })
                .ToList();

            dgStats.ItemsSource = stats;
        }

        //вспомогательные

        private void LoadUserComboBox()
        {
            var users = db.Users.ToList();
            cmbUserForBooking.ItemsSource = users.Select(u => new {
                u.UserId,
                UserDisplay = $"{u.LastName} ({u.Email})"
            }).ToList();
            cmbUserForBooking.DisplayMemberPath = "UserDisplay";
            cmbUserForBooking.SelectedValuePath = "UserId";
        }

        private void LoadCarComboBoxForBooking()
        {
            var cars = db.Cars.ToList();
            cmbCarForBooking.ItemsSource = cars.Select(c => new {
                c.CarId,
                CarDisplay = $"{c.Brand} {c.Model} ({c.LicensePlate})"
            }).ToList();
            cmbCarForBooking.DisplayMemberPath = "CarDisplay";
            cmbCarForBooking.SelectedValuePath = "CarId";
        }

        private void LoadComboBoxesForFines()
        {
            var bookings = db.Bookings
                .Include(b => b.Car)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingId)
                .ToList();

            cmbBookingForFine.ItemsSource = bookings.Select(b => new {
                b.BookingId,
                Display = $"#{b.BookingId} - {b.Car.Brand} {b.Car.Model} - {b.User.LastName} ({b.StartDateTime:dd.MM.yyyy})"
            }).ToList();

            cmbBookingForFine.DisplayMemberPath = "Display";
            cmbBookingForFine.SelectedValuePath = "BookingId";
        }
    }
}