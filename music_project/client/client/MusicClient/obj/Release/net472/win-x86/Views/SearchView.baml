   M S B A M L   `   `   `  ���� F  BMusicClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:MusicClient.ViewsMusicClient.Views  ?$clr-namespace:MusicClient.ViewModelsMusicClient.ViewModels  ?$clr-namespace:MusicClient.ConvertersMusicClient.Converters  "    MusicClient.Views.SearchView   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:MusicClient.Views     ;
viewModels$clr-namespace:MusicClient.ViewModels     ;
converters$clr-namespace:MusicClient.Converters       � 	Resources  ��%    .��� $��20q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* �.+��� $��Поиск треков��   HeaderTextStyle#��[  $��0����  �� Orientation$ 
Horizontal=�$��1��$��0,0,0,20q�.+��� $	��300��  SearchBoxStyle#��[ $��Center=�$��0,0,10,0q�$��14(����� *SearchQuery+ �� UpdateSourceTrigger$ PropertyChanged=�.�� J� InputBindings �� 
 �� Key$ Enter�� �� Command �� *SearchCommand+.���� $��
Найти��  AccentButtonStyle#��[ $��15,10q�$��Center=����� *SearchCommand+� $��2��.+��� 
������ $��0q����� *SearchResults+����  �� 
TargetType" ��.���  �� Property � ��	 �� Value$	 Stretch=���  � ��
	 ���� ��  � ��	 �� 
 �� 
TargetType"
 ��.R��� #��z��  
HoverColor#��[ $��0,0,0,1q� �� Padding$
 10,8q�._���  �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  � ��#	 [ ���� .R�� $��5q� � ColumnDefinitions �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto �.+��� $��0��  ModernButtonStyle#��[ $��5q�$��0,0,10,0q� � ToolTip$  Воспроизвести������ *DataContext.PlayTrackCommand+ �� RelativeSource ��  �� AncestorType ListView���� .���� $	��▶��$��16(�$��Center=��� $��1��$��Center=�.+��� $��SemiBold"�$��15(�  TextPrimary#��[ ���� *Title+�� $��13(�  TextSecondary#��[ ���� *Artist+�� $��2��$��Center=�$��Right=�$��0,0,20,0q�$��13(�#��[ ���� *
Duration+  DurationConverter �� 	Converter# [ �� $6��0Введите запрос для поиска.��$��Center=�$��Center=�#��[ $��16(�y��� *SearchResults.Count+  NullToVisibilityConverter# [  �� ConverterParameter$ inverseIfZero���  � IsIndeterminate$
 True��$	��100��$��10��$��Center=�$	��Top=�$��0,20,0,0q�y��� *	IsLoading+ 	 BoolToVisibilityConverter# [	 