   M S B A M L   `   `   `  ����   MusicClient    MusicClient.LoginWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$!  Вход в Music Streaming��$	��600��$	��400�� � WindowStartupLocation$ CenterScreen=� � AllowsTransparency$
 True�� � WindowStyle$
 None=�
������  � 	Resources    PrimaryBackground �� Color  SecondaryBackground  CardBackground  AccentColor  TextPrimary  TextSecondary  
HoverColor  ModernTextBoxStyle �� 
TargetType �� Property �� Value	 �� 
TargetType
 �� CornerRadius ��Name �� Triggers P� Property P� Value  ModernPasswordBoxStyle 	 AccentButtonStyle �� Padding 
 LinkButtonStyle��%  &	        &	      &	 &     &	 9     &	 L     &	 _     &	 r     &	 �     7  7  7  &	 �    7  7  7  &		 �    7  &	
 m    7  �� $ #121212���� $ #181818���� $ #282828���� $ #03a1fc���� $ #FFFFFF���� $ #B3B3B3���� $ #1E1E1E���� " ��.���  � ��8   ��  � ��8  ��  � ��$ 2q���  � ��
 ���� ��  � ��$ 16,12q���  � ��$ 14(���  � �� �� "	 ��.R��� #��z��#��z��#��z��$
 4��._��� $ PART_ContentHost��#��z�� P�  �@J�	IsFocused$
 True��. ��� 8   � ���� " G�.���  � ��8   ��  � ��8  ��  � ��$ 2q���  � ��
 ���� ��  � ��$ 16,12q���  � ��$ 14(���  � �� �� "	 G�.R��� #��z��#��z��#��z��$
 4��._��� $ PART_ContentHost��#��z�� P�  �@J�	IsFocused$
 True��. ��� 8   � ���� " ��.���  � ��8   ��  � ��
 �   ���  � ��$ 0q���  � ��$ 24,12q���  � ��$ 16(���  � ��$
 Bold"���  �@�Cursor$
 Hand����  � �� �� "	 ��.R��� #��z��$
 25��# z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ���  � �� �#03a1fcP�  � ��$
 True��. ���  � �� �#03a1fc�� " ��.���  � ��
 ���� ��  � ��8   ��  � ��$ 0q���  �@�Cursor$
 Hand����  � ��$ 14(���  � �� �� "	 ��.R���  P�  �@J�IsMouseOver$
 True��. ���  � �� �#03a1fc��  � ��$ 	Underline{�.���� #��[  $
 8��._��  � RowDefinitions �� $��40 ��� $��* �.+�� $��0��#��[ .+���  �� Orientation$ 
Horizontal=�$��Center=�$
��15,0q�.+�A� $��12��$��12�����#FF5F56$��0,0,8,0q�A� $��12��$��12�����#FFBD2E$��0,0,8,0q�A� $��12��$��12�����#27CA3F�� $ ��Вход в аккаунт��#��[ $��Medium"�$��20,0,0,0q�$��Center=��� $��Right=�$ 
Horizontal=�.+��� -   $��×��
������ #��[ $��0q�$��40��$��30��$��16(��� $��1��$��Hidden=�$��0,0,0,0q�.���� $	��320��$��30q�.+��� $��Center=�$��0,0,0,30q�.+��� $	��180��$	��100��$��0,0,0,0q�$%��/MusicClient;component/icon.png���� $��СloudMusic��#��[ $��28(�$
��Bold"�$��Center=��� $8��2Локальное хранилище музыки��#��[ $��16(�$��Center=�$��0,8,0,0q�� .+�� -    �Name$ 	LoginGrid��$y�Visible=�.+��� .+��� $#��  Никнейм или Email��#��[ $��14(�$
��Bold"�$��0,0,0,8q��� -    ��Name$! LoginNicknameOrEmailTextBox��#��[ $��0,0,0,16q��� $��  Пароль��#��[ $��14(�$
��Bold"�G� -    G�Name$ LoginPasswordBox��#��[ $��0,0,0,24q��� -    ��Name$ LoginButton��$��
ВОЙТИ��#��[	 $��50��$��0,0,0,24q��� $ 
Horizontal=�$��Center=�$��0,0,0,0q�.+��� $��Нет аккаунта?��#��[ $��14(�$��Center=��� -   $ ShowRegisterFormButton��$*��$Зарегистрироваться��#��[
 $��8,0,0,0q�� -   $ RegisterGrid��$y�	Collapsed=�.+��� .+��� $��  Никнейм��#��[ $��14(�$
��Bold"�$��0,0,0,8q��� -   $ RegisterNicknameTextBox��#��[ $��0,0,0,16q��� $��  Email��#��[ $��14(�$
��Bold"�$��0,0,0,8q��� -	   $ RegisterEmailTextBox��#��[ $��0,0,0,16q��� $��  Пароль��#��[ $��14(�$
��Bold"�G� -
   $ RegisterPasswordBox��#��[ $��0,0,0,16q��� $'��!  Повторите пароль��#��[ $��14(�$
��Bold"�G� -   $  RegisterConfirmPasswordBox��#��[ $��0,0,0,24q��� -   $ RegisterButton��$*��$ЗАРЕГИСТРИРОВАТЬСЯ��#��[	 $��50��$��0,0,0,24q��� $ 
Horizontal=�$��Center=�$��0,0,0,0q�.+��� $%��Уже есть аккаунт?��#��[ $��14(�$��Center=��� -   $ ShowLoginFormButton��$��
Войти��#��[
 $��8,0,0,0q�