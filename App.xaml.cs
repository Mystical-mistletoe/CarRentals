using CarRentals.Data;
using CarRentals.Models;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CarRentals
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Инициализация базы данных ПЕРЕД открытием окна логина
            InitializeDatabase();

            // Теперь можно открывать окно логина
            LoginWindow login = new LoginWindow();
            login.Show(); 
        }

        private void InitializeDatabase()
        {
            // СОЗДАЁМ БАЗУ ДАННЫХ ДО ВСЕГО ОСТАЛЬНОГО
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                // Добавляем учётные записи, если их нет
                if (!db.AdAccounts.Any())
                {
                    db.AdAccounts.Add(new AdAccount { Login = "admin", Password = "555", Role = "admin" });
                    db.AdAccounts.Add(new AdAccount { Login = "manager", Password = "455", Role = "manager" });
                    db.SaveChanges();
                    MessageBox.Show("База данных успешно создана и заполнена тестовыми данными!");
                }
            }
        }
    }

}
