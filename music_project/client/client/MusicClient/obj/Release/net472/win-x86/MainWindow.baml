   M S B A M L   `   `   `  ���� F  BMusicClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:MusicClientMusicClient  ?$clr-namespace:MusicClient.ConvertersMusicClient.Converters      MusicClient.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     +localclr-namespace:MusicClient     ;
converters$clr-namespace:MusicClient.Converters       � Title$  Music Streaming��$	��800��$
��1200�� � WindowStartupLocation$ CenterScreen=� � AllowsTransparency$
 True�� � WindowStyle$
 None=�
������  � 	Resources 1   +MusicClient.Converters.BoolToColorConverter   BoolToColorConverter4   .MusicClient.Converters.BoolToPlayIconConverter  BoolToPlayIconConverter7   1MusicClient.Converters.BoolToPlayTooltipConverter  BoolToPlayTooltipConverter7   1MusicClient.Converters.RepeatModeToColorConverter  RepeatModeToColorConverter  FadeInAnimation �� TargetProperty W� From	 W� To g� Duration��%{   &	        &	      &	 
     &	      &	              �� .��W� $ Opacity� 0 1$ 0:0:0.3D�.����   PrimaryBackground#��[ 	 �� CornerRadius$	 8��._�� 
 � RowDefinitions
 �� $��40 ��� $��* ��� $��70 �.+�� -    �Name$ TitleBar��$��0��  SecondaryBackground#��[  � ColumnDefinitions �� $��* ��� $
��Auto �.+���  �� Orientation$ 
Horizontal=�$��Center=�$
��15,0q�.+�A� $��12��$��12�����#FF5F56$��0,0,8,0q�A� $��12��$��12�����#FFBD2E$��0,0,8,0q�A� $��12��$��12�����#27CA3F�� $��Music Streaming��  TextPrimary#��[ $��Medium"�$��20,0,0,0q�$��Center=��� $��1��$ 
Horizontal=�$��Center=�$��30��.+��� -    ��Name$ MinimizeButton��$	��−��  ModernButtonStyle#��[ $��45��$��30���� -   $ MaximizeButton��$	��□��#��[ $��45��$��50��$��Center=�$��Center=��� -   $ CloseButton��$��×��#��[ $��45��$��50��$��Center=�$��Center=�� $��1�� �� $	��240 ��� $��* �.+��� $��0��#��[ ._��� $
��Auto=�.���� $��
24,24,24,0q�.+��� $ 
Horizontal=�$��0,0,0,32q�.+�A� $��32��$��32�� 	 AccentColor#��[	 �� $��
CloudMusic��#��[ $��26(�$
��Bold"�$��Center=�$��12,0,0,0q��� .+��� $��🏠 Главная��#��[ $
��Left=�$��Center=�$��18(����� *ShowAllTracksCommand+�� $$��📚 Моя медиатека��#��[ $��Stretch=�$
��Left=�$��18(����� *ShowFavoritesCommand+�� $��👤 Профиль��#��[ $��Stretch=�$
��Left=�$��18(����� *ShowProfileCommand+�� $&�� 📤 Загрузить трек��#��[ $��18(����� *ShowUploadTrackViewCommand+�� -   $ LogoutButton��$��🚪 Выход��#��[ $��Stretch=�$
��Left=�$��18(�$��0,40,0,0q�� $��1��#��[ .+��� ���� *CurrentView+�� $��2�����	#FF282828 �� Padding$	 5,0q�._�� $
��10,0q� �� $
��Auto �$	��200��$	��300���� $��* ��� $
��Auto �$	��180��.+��� $��0��$ 
Horizontal=�$��Center=�.+��� $
��🎵��$��30(�$��Center=�$��0,0,10,0q� 
 TextSecondary#��[
 �� $��Center=�.+��� $��SemiBold"�$��13(�#��[ $��CharacterEllipsis=����� *SelectedTrack.Title+ �� FallbackValue$  Трек не выбран�� � ToolTip �� *SelectedTrack.Title+�� $��11(�#��[
 $��CharacterEllipsis=����� *SelectedTrack.Artist+$. (Исполнитель не указан�� �� *SelectedTrack.Artist+� $��1��$��Center=�
 �� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��Center=�$��0,0,0,2q�.+���   PlayerButtonStyle#��[ $	��⏮��$# Предыдущий трек������ *PreviousTrackCommand+�� #��[ $��40��$��40������ *PlayCommand+ �� *	IsPlaying+ �� 	Converter# [ .��� .+�F� +���� z  nN � z �$���!�=� � z  nN 	$��16��$��16�� �� Stretch$ Uniform=����� *
Foreground+ �� RelativeSource ��  �� AncestorType
 Button����  �� 
TargetType" F�.���  �� Property � y� �� Value$ Visible=� �� Triggers ��  �� Binding �� *	IsPlaying+ �� Value True.a���  � y�$ 	Collapsed=�F� a������[ ��!р�� ��!р�� @KL р�[ @KL р�[ ��!���� @KL р�� ��!р���!р�@KL р�� @KL 	$��16��$��16��$ Uniform=����� *
Foreground+ �� 
 Button���� " F�.���  � y�$ 	Collapsed=� ��  �� *	IsPlaying+ True.a���  � y�$ Visible=��� #��[ $	��⏭��$! Следующий трек������ *NextTrackCommand+� $��1��$��
10,0,10,-2q�$��Center=� �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��#��[
 $��10(�$��Center=�$��0,0,5,0q����� *CurrentTime+$
 0:00���� -    ��Name$ ProgressSlider��$��1��$��Center=� � Value �� *CurrentPosition+ �� Mode$ TwoWay=� � Maximum �� *TotalDuration+$ 1���� $��2��#��[
 $��10(�$��Center=�$��5,0,0,0q����� *TotalDuration+  DurationConverter# [ $
 0:00���� $��2��$ 
Horizontal=�$��Center=�$��Right=�$��0,0,10,0q�.+��� $
��🔊��$��16(�$��Center=�$��0,0,5,0q�#��[
 �� -   $ VolumeSlider��$��80��  � Minimum$  0Q�$ 1Q�$��Center=� �� *Volume+$ TwoWay=�r� -   ! r�Name$! Player��" r� LoadedBehavior$" Manual=�# r� UnloadedBehavior$# Manual=�$y�	Collapsed=�