using System.Windows;
using System.Windows.Controls;

namespace MusicClient.Dialogs
{
    public class InputDialog : Window
    {
        private TextBox textBox1;
        private TextBox textBox2;

        public string InputText1 { get; private set; }
        public string InputText2 { get; private set; }

        public InputDialog(string title, string prompt1, string prompt2)
        {
            Title = title;
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent; // Для скругленных углов, если основной фон окна будет скруглен

            // Основной Border для скругленных углов и фона
            Border mainBorder = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("SecondaryBackground"), // Используем ресурс из App.xaml или MainWindow.xaml
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20)
            };

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock promptBlock1 = new TextBlock { Text = prompt1, Margin = new Thickness(0, 0, 0, 5), Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary") };
            Grid.SetRow(promptBlock1, 0);
            grid.Children.Add(promptBlock1);

            textBox1 = new TextBox { Margin = new Thickness(0, 0, 0, 10), Style = (Style)FindResource("SearchBoxStyle") }; // Используем стиль из MainWindow
            Grid.SetRow(textBox1, 1);
            grid.Children.Add(textBox1);

            TextBlock promptBlock2 = new TextBlock { Text = prompt2, Margin = new Thickness(0, 10, 0, 5), Foreground = (System.Windows.Media.Brush)FindResource("TextPrimary") };
            Grid.SetRow(promptBlock2, 2);
            grid.Children.Add(promptBlock2);

            textBox2 = new TextBox { Margin = new Thickness(0, 0, 0, 10), Style = (Style)FindResource("SearchBoxStyle") }; // Используем стиль из MainWindow
            Grid.SetRow(textBox2, 3);
            grid.Children.Add(textBox2);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0,15,0,0)
            };

            Button okButton = new Button
            {
                Content = "OK",
                Style = (Style)FindResource("AccentButtonStyle"), // Используем стиль из MainWindow
                Width = 90,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += OkButton_Click;

            Button cancelButton = new Button
            {
                Content = "Отмена",
                Style = (Style)FindResource("ModernButtonStyle"), // Используем стиль из MainWindow
                Width = 90,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 4);
            grid.Children.Add(buttonPanel);

            mainBorder.Child = grid;
            Content = mainBorder;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText1 = textBox1.Text;
            InputText2 = textBox2.Text;
            DialogResult = true;
        }
    }
} 