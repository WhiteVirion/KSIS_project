   M S B A M L   `   `   `  ���� F  BMusicClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:MusicClient.ViewsMusicClient.Views  ?$clr-namespace:MusicClient.ViewModelsMusicClient.ViewModels  7 clr-namespace:MusicClient.ModelsMusicClient.Models  ?$clr-namespace:MusicClient.ConvertersMusicClient.Converters  %    MusicClient.Views.AllTracksView   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:MusicClient.Views     ;
viewModels$clr-namespace:MusicClient.ViewModels     3models clr-namespace:MusicClient.Models     ;
converters$clr-namespace:MusicClient.Converters       � 	Resources  ��%    .��� ���#121212 � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$��Все треки��   HeaderTextStyle#��[  $��20q��� $��0��
���  ��$��0,0,20,0q�$��Right=����� *NowPlayingText+�� $��2�� �� Orientation$ 
Horizontal=�$��20,10,20,20q�$��Center=�.+��� $	��200��$��38��  SearchBoxStyle#��[ $��Center=�$��0,0,10,0q�$��14(�$
��10,8q����� *SearchQuery+ �� UpdateSourceTrigger$ PropertyChanged=�.�� J� InputBindings �� 
 �� Key$ Enter�� �� Command �� *SearchCommand+.����   AccentButtonStyle#��[ $��0q�$��Center=�$��38��$��38�� � ToolTip$ 
Найти������ *SearchCommand+.��F� �����`�� ��- � $� `�� ��.c �.c � $� p�� P#� �� @V� Ѐ� ��t� ��� ���!P�P��!р�� �t� �Ѐ� @V� Ӱ� P#� p��  $� `��  $� ���- `�� ��.c �.c �`�� ��- ��.c �.c �`�� @KL ���j @KL @KL ��j @KL `�� �@KL  � ��j ��� `�� ��� � � ��� ���  � ��� `�� Ӏ�� ��j  � @KL `�� @KL 	$��16��$��16�� �� Stretch$ Uniform=����� *
Foreground+	 �� RelativeSource	 �� 
 �� AncestorType

 Button�� $��3��
������ $��0q����� *	AllTracks+����  �� 
TargetType" ��.���  �� Property � �� �� Value$ Stretch=���  � ��
 ���� ��  � �� ��  �� 
TargetType" ��.R���  ��Name$ 
ItemBorder��#��z��  
HoverColor#��[ $��0,0,0,1q� �� Padding$
 10,8q�._���  �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  � ��# [ P�  �@��
IsSelected$
 True��. ���  � ��  AccentColor# [ ^�  ^� 
Conditions ��  �� Value True �� Binding `�   EqualityConverter `� 	Converter# [ .1��� ��  �� Path$ DataContext.SelectedTrack�	 �� 
 ListView��  �� *DataContext.IsPlaying+	 �� 
 ListView True.0���  � �� �	#FF3A3A3A����  �� DataType!   MusicClient.Models.TrackDto"  .R�� $��5q� � ColumnDefinitions �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��  ModernButtonStyle#��[ $��5q�$��0,0,10,0q�$+ %Воспроизвести/Пауза������ * DataContext.SelectTrackCommand+	 �� 
 ListView���� .��� $��16��$��16��.+�F�  F�Name$ PlayIcon��+���� z  nN � z �$���!�=� � z  nN 	$ Uniform=����� *
Foreground+	 �� 

 Button���� " F�.���  � y�$ Visible=� ^�  ��  True `� # [ .1��� �� $ DataContext.SelectedTrack�	 �� 
 ListView��  �� *DataContext.IsPlaying+	 �� 
 ListView True.0���  � y�$ 	Collapsed=�F� $ 	PauseIcon��a������[ ��!р�� ��!р�� @KL р�[ @KL р�[ ��!���� @KL р�� ��!р���!р�@KL р�� @KL 	$ Uniform=����� *
Foreground+	 �� 

 Button���� " F�.���  � y�$ 	Collapsed=� ^�  ��  True `� # [ .1��� �� $ DataContext.SelectedTrack�	 �� 
 ListView��  �� *DataContext.IsPlaying+	 �� 
 ListView True.0���  � y�$ Visible=��� $��1��$��Center=�.+��� $��SemiBold"�$��15(�  TextPrimary#��[ ���� *Title+�� $��13(�  TextSecondary#��[ ���� *Artist+�� $��2��$��Center=�$��Right=�$��0,0,20,0q�$��13(�#��[ ���� *
Duration+ 	 DurationConverter �� 	Converter# [	  �� FallbackValue$ 00:00���� $��3��#��[ $��8q�$��0,0,5,0q�$��32��$��32������ *#!DataContext.ToggleFavoriteCommand+	 �� 
 ListView����  _� ���� *	IsLiked+ "
 BoolToFavoriteTooltipConverter# [
  �� ConverterParameter$K EУдалить из Любимых|Добавить в Любимые��.��� .+�F� ����� � p�E���� 0�1��eR  `� �� �`� ��  �� Ӏ� �R �qC ��- �pr ��- ���� ��- 0y� �":  � ЪM �м� �": @8� ��-  �� ��- �`�*��- ��O�R ��O �� Ӏ�O�`� @� `� ;� @�1� � p�E	#��[ $��16��$��16��$ Uniform=����� " F�.���  � y�$ 	Collapsed=� ��  �� Binding �� *	IsLiked+  �� Value  True.a���  � y�$ Visible=�F� ����� �� ��- �@8� ��- м� �":  � ЪM �0y� �": ��� ��- �pr ��- ӠqC ��- �� �R ��  �� Ӏ� �`� �eR  `� ��� @�1� � p�E�;� 0�1�@� `� ��O�`� ��O �� Ӏ�O�R `�*��-  �� ��- � �� @KL �08@KL  -1@d  -1 �� � -10[� D
PH� �� >
ףp�1@� � = ��� 0t���c 0��  	= ��  	=  z � 	= �\ ��T  �D �pr  �D Ӏ��  �D �Ӡ P�S 0� p=
ףp@�z�G�)@p=
ףp@� b� pf[ �E� @KL  �� @KL 	���#B3B3B3$��16��$��16��$ Uniform=����� " F�.���  � y�$ Visible=� ��  �� *	IsLiked+  True.a���  � y�$ 	Collapsed=��� $��4��#��[ $��8q�$��0,0,5,0q�$ Скачать������ *DataContext.DownloadCommand+	 �� 
 ListView���� .��F� |������!@T� ���� @T� ���� ��- �@T� ��- �@T� @T� �@KL @T� � �  $� ���!@T� �@KL ���@KL  -1���! -1���!���@KL ��	���#B3B3B3$��16��$��16��$ Uniform=��� $��5��#��[ $��8q�$��0,0,5,0q�$ Удалить трек������ * DataContext.DeleteTrackCommand+	 �� 
 ListView���� .��F� �������[ ��!Ӏ�[ ��2 Ii @o@ z @o@� $� @o@���@o@����2����!р���j р�[ ��j р�[ ��!���! 	= ����  	= Ѡ@� ��- �`�� ��- � ��  	= �@KL  	= �@KL ��[ ���!��[ ���! 	= 	���#B3B3B3$��14��$��14��$ Uniform=�