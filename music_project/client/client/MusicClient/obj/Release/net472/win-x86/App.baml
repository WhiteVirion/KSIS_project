   M S B A M L   `   `   `  ���� F  BMusicClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:MusicClientMusicClient  ?$clr-namespace:MusicClient.ConvertersMusicClient.Converters      MusicClient.App   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     +localclr-namespace:MusicClient     ;
converters$clr-namespace:MusicClient.Converters       �� 	Resources   �� MergedDictionaries �� Source.   (MusicClient.Converters.DurationConverter   DurationConverter1   +MusicClient.Converters.BoolToColorConverter  BoolToColorConverter;   5MusicClient.Converters.BoolToFavoriteTooltipConverter " BoolToFavoriteTooltipConverter6   0MusicClient.Converters.NullToVisibilityConverter  NullToVisibilityConverter6   0MusicClient.Converters.BoolToVisibilityConverter  BoolToVisibilityConverter3   -MusicClient.Converters.StatusToColorConverter  StatusToColorConverterC   =MusicClient.Converters.StringNullOrEmptyToVisibilityConverter * &StringNullOrEmptyToVisibilityConverter7   1MusicClient.Converters.RepeatModeToColorConverter  RepeatModeToColorConverter7	   1MusicClient.Converters.BoolToPlayTooltipConverter  BoolToPlayTooltipConverter4
   .MusicClient.Converters.BoolToPlayIconConverter 	 BoolToPlayIconConverter.   (MusicClient.Converters.EqualityConverter 
 EqualityConverter  PrimaryBackground �� Color  SecondaryBackground  CardBackground  AccentColor  TextPrimary  TextSecondary  
HoverColor  ModernButtonStyle �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius	 �� Padding
 �� Triggers P� Property P� Value  AccentButtonStyle �� BasedOn  PlayerButtonStyle  HeaderTextStyle  SubHeaderTextStyle  	CardStyle �� Triggers  SearchBoxStyle ��Name  ModernTextBoxStyle  SpotifyThumbStyle  SpotifySliderStyle � RowDefinitions ��Name ��Name �� RadiusX �� RadiusY n�Name �� 
TargetName P� 
SourceName��  �� $ Resources/Styles.xaml?�%�  &	        &	      &	 
     &	      &	      &	      &	      &	 #     &	 (     &		 -     &	
 2     &	 7     &	 J     &	 ]     &	 p     &	 �     &	 �     &	 �     &	 �     7  7  7  &	     7  7  &	 o    7  7  7  &	 s    7  &	 �    7  &	 +    7  &	 �    7  7  &	 �    7  7  7  7  7  &	 e    &	 �    7                  	  
    �� $ #121212���� $ #181818���� $ #96d9ff���� $ #03a1fc���� $ #FFFFFF���� $ #B3B3B3���� $ #1E1E1E���� " ��.���  � ��
 ���� ��  � ��8   ��  � ��$ 0q���  � ��$ 15,10q���  � ��$ 14(���  � ��$ Medium"���  �@�Cursor$
 Hand����  � �� �� " ��.R��� #��z��$ 20��#	 z��._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ��� 8   � ���� 8   � ���� " ��8   .���  � ��8  ��  � ��
 �   ���  � ��$
 Bold"��� " ��.���  � ��
 ���� ��  � ��8   ��  � ��$ 0q���  � ��$ 8q���  � ��$	 3,0q���  � ��$ 16(���  � ��$ Medium"���  �@�Cursor$
 Hand����  � ��$ 36����  � ��$ 36����  � ��$ Center=���  � ��$ Center=���  � �� �� " ��.R��� #��z��$ 4��._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ��� 8   � ���� 8   � ��P�  � {�$ False��. ���  � �� �	#FF505050�� " ��.���  � ��8   ��  � ��$ 24(���  � ��$
 Bold"���  � ��$ 0,0,0,20q��� " ��.���  � ��8   ��  � ��$ 14(���  � ��$ Medium"��� " ��.���  � ��8   ��  �@��CornerRadius$ 8����  �@��Padding$ 16q���  � ��$ 8q� P�  �@J�IsMouseOver$
 True��. ���  � �� �#3E3E3E�� " ��.���  � ��8   ��  � ��8  ��  � ��$ 0q���  � ��$ 40,12,12,12q���  � ��$ 14(���  � �� �� " ��.R��� #��z��$ 20��._��� $ PART_ContentHost��#��z���� " ��.���  � ��8   ��  � ��8  ��  � ��8  ��  � ��$ 1q���  � ��$
 10,8q���  � ��$ 14(���  �@J�SnapsToDevicePixels$
 True����  � �� �� " ��.R��� #��z��#��z��#��z��$ 5��._��� $ PART_ContentHost��#��z��
 P�  �@J�IsMouseOver$
 True��. ��� 8   � ��P�  �@J�IsKeyboardFocused$
 True��. ��� 8   � ����  � ��$	 1.5q��� " n�.���  � �� �� " n�.R�A� ���#FFFFFF$��12��$��12��
������ $��0���� " ��.���  � �� �#4F4F4F��  � �� �#03a1fc��  � ��$ 4����  � �� �� " ��.R��  �� $
��Auto ��� $
��Auto �#��z���� $
��Auto �.+��� $ TrackBackground��$��1��$��4�����#4F4F4F���#4F4F4F$��1q�$ 2���� $ PART_SelectionRange��$��1��$��4�����#03a1fc$ 2��$ 2��$y�Hidden=�n� $ 
PART_Thumb��$��1��$��12��$��12�����#FFFFFF���#03a1fc$��0q�8��  $y�	Collapsed=�
 P�  �@J�IsMouseOver$
 True��. ��� $ 
PART_Thumb�� � y�$ Visible=��� $ TrackBackground�� � �� �#03a1fcP� $ 
PART_Thumb�� �@n�
IsDragging$
 True��. ��� $ 
PART_Thumb�� � y�$ Visible=�