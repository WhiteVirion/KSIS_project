using MusicClient.Models;
using MusicClient.Services;
using System.Windows;
using System.Windows.Input; // Если не используется для перетаскивания, можно убрать
// TODO: Рассмотреть добавление System.ComponentModel.DataAnnotations для валидации Email, если нужно
// using System.Text.RegularExpressions; // Для более сложной валидации Email

namespace MusicClient
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Перемещение окна по ЛКМ
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            // Вход по Enter
            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    LoginOrRegisterBasedOnVisibility();
                }
            };
        }

        private void LoginOrRegisterBasedOnVisibility()
        {
            if (LoginGrid.Visibility == Visibility.Visible && LoginButton.IsEnabled)
            {
                LoginButton_Click(LoginButton, new RoutedEventArgs());
            }
            else if (RegisterGrid.Visibility == Visibility.Visible && RegisterButton.IsEnabled)
            {
                RegisterButton_Click(RegisterButton, new RoutedEventArgs());
            }
        }

        private void ShowRegisterFormButton_Click(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            RegisterGrid.Visibility = Visibility.Visible;
            // Опционально: очистить поля при переключении
            LoginNicknameOrEmailTextBox.Clear();
            LoginPasswordBox.Clear();
            // Опционально: фокус на первое поле регистрации
            RegisterNicknameTextBox.Focus(); 
        }

        private void ShowLoginFormButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
            // Опционально: очистить поля при переключении
            RegisterNicknameTextBox.Clear();
            RegisterEmailTextBox.Clear();
            RegisterPasswordBox.Clear();
            RegisterConfirmPasswordBox.Clear();
            // Опционально: фокус на первое поле входа
            LoginNicknameOrEmailTextBox.Focus();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string nicknameOrEmail = LoginNicknameOrEmailTextBox.Text;
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(nicknameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите никнейм/email и пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Предполагаем, что AuthService.LoginAsync может принимать никнейм или email.
            // Если AuthService.LoginAsync принимает только Nickname, то здесь нужна будет другая логика
            // или изменение AuthService для поддержки входа по Email.
            // Пока что оставляем как есть, предполагая, что серверная часть это обработает (например, попробует найти по Nickname, потом по Email).
            // Для текущей реализации сервера (AuthService) вход осуществляется только по Nickname.
            // Поэтому, по-хорошему, поле для входа должно быть только для Nickname, либо серверную часть нужно доработать.
            // Для упрощения, оставляем как есть, но пользователь должен будет вводить именно Nickname.
            var loginDto = new UserLoginDto { Nickname = nicknameOrEmail, Password = password };

            LoginButton.IsEnabled = false;
            ShowRegisterFormButton.IsEnabled = false; 

            var (success, message, authResponse) = await _apiService.LoginAsync(loginDto);

            LoginButton.IsEnabled = true;
            ShowRegisterFormButton.IsEnabled = true;

            if (success && authResponse != null)
            {
                MessageBox.Show(message, "Вход выполнен", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow mainWindow = new MainWindow(_apiService, authResponse);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(message ?? "Произошла ошибка при входе.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string nickname = RegisterNicknameTextBox.Text;
            string email = RegisterEmailTextBox.Text;
            string password = RegisterPasswordBox.Password;
            string confirmPassword = RegisterConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(nickname) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для регистрации.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // TODO: Добавить валидацию формата Email (Regex)
            // TODO: Добавить клиентскую валидацию длины/сложности пароля

            var registerDto = new UserRegisterDto { Nickname = nickname, Email = email, Password = password };

            RegisterButton.IsEnabled = false;
            ShowLoginFormButton.IsEnabled = false;

            var (success, message, authResponse) = await _apiService.RegisterAsync(registerDto);

            RegisterButton.IsEnabled = true;
            ShowLoginFormButton.IsEnabled = true;

            if (success)
            {
                MessageBox.Show((message ?? "Регистрация успешна!") + "\nТеперь вы можете войти, используя свои учетные данные.", "Регистрация успешна", MessageBoxButton.OK, MessageBoxImage.Information);
                RegisterPasswordBox.Clear();
                RegisterConfirmPasswordBox.Clear();
                // Переключаемся на форму входа
                ShowLoginFormButton_Click(sender, e); 
            }
            else
            {
                MessageBox.Show(message ?? "Произошла ошибка при регистрации.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        // TODO: Если есть обработчики для социальных сетей или "Забыли пароль?", 
        // их нужно будет реализовать или удалить кнопки из XAML, если они не нужны.
    }
}