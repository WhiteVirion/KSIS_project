   M S B A M L   `   `   `  ���� F  BMusicClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:MusicClient.ViewsMusicClient.Views  ?$clr-namespace:MusicClient.ViewModelsMusicClient.ViewModels  '    !MusicClient.Views.UploadTrackView   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:MusicClient.Views     ;
viewModels$clr-namespace:MusicClient.ViewModels     ���#121212.��� $��20q�  � RowDefinitions  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$.��(Загрузка нового трека��   HeaderTextStyle#��[  $��0,0,0,20q��� $��1��$*��$Выбрать аудиофайл...��  AccentButtonStyle#��[ $
��Left=�$
��15,8q�$��0,0,0,10q����� *SelectFileCommand+{��� *CanSelectFile+�� $��2��  TextSecondary#��[ $��0,0,0,10q����� *SelectedFilePath+ �� FallbackValue$  Файл не выбран���� $��3��$?��9Название трека (необязательно):��  TextPrimary#��[ $��0,10,0,5q��� $��4��  ModernTextBoxStyle#��[ $��0,0,0,10q����� *Title+ �� UpdateSourceTrigger$ PropertyChanged=�{��� *CanSelectFile+�� $��5��$:��4Исполнитель (необязательно):��#��[ $��0,0,0,5q��� $��6��#��[ $��0,0,0,20q����� *Artist+$ PropertyChanged=�{��� *CanSelectFile+�� $��7�� �� Orientation$ Vertical=�.+��� $*��$Загрузить на сервер��#��[ $
��Left=�$
��15,8q�$��0,0,0,10q����� *UploadCommand+{��� *	CanUpload+�  � Minimum$ 0Q� � Maximum$	 100Q�$��10��$��0,0,0,5q� � Value �� *UploadProgress+y��� *IsUploading+  BoolToVisibilityConverter �� 	Converter# [ �� $��SemiBold"�$
��Wrap=����� *StatusMessage+���� *StatusMessage+  StatusToColorConverter# [ 