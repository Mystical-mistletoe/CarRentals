using CarRentals;
using CarRentals.Data;
using CarRentals.Models;
using Microsoft.Identity.Client.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarRentals
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private AppDbContext db = new AppDbContext();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            //поиск перв элемента, удовл условию
            var user = db.AdAccounts.FirstOrDefault(a =>
            a.Login == txtLogin.Text && a.Password == txtPassword.Password);
            if (user != null)
            {
                //созраняет пользователя в статическое свойство
                CurrentAccount.CurAccount = user;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Неверный логин/пароль";
            }
        }

        
    }
    public static class CurrentAccount
    {
        public static AdAccount CurAccount { get; set; }
    }
}
