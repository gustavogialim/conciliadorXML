   M S B A M L   `   `   `  ����M  IConciliadorDeNotas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:ConciliadorDeNotasConciliadorDeNotas  5   	   #    ConciliadorDeNotas.Resultados   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:ConciliadorDeNotas     5   	     � Title$  
Resultados��5   	   $	��700��6   $	��742��6)    � WindowStartupLocation$ CenterScreen=�65    � 	Resources 5
       �� Source�� 5   
   $ /Dicionario.xaml?�6   %    .��5      � -   5         �Name$ gridResultados��5      
�������.+�5   
   �� 
���   �$��0,0,0,0q�6%   $��32��66   ���	#FF002549$��Stretch=�6Y   $	��Top=�6w   ._�5      ��  �� Orientation$ 
Horizontal=�6   $��Center=�62   $	��Top=�6O   .+�5      �� 
�������$��23(�60   $��30��6>   $#��Resultados da Análise do XML��6J    �� TextAlignment$ Center=�6n   5      5      �� 5   
   
���   �$��
0,34,389,0q�6%   $��32��69   ���	#FF48464D$��Stretch=�6\   $	��Top=�6z   ._�5      �� $ 
Horizontal=�6   $��Stretch=�62   $	��Top=�6P   .+�5      �� 
�������$��23(�60   $��30��6>   $#��Lista de Produtos da análise��6J   $
 Left=�6n   $	��343��6�   5      5      �� 5   
   -   5         ��Name$ labelTotalValorNotas��5   @   $��0,85,0,0q�6   $��14(�6&   $��21��64   $)��#Valor total dos produtos: R$ 500,00��6^   $��Right=�6�   $	��Top=�6�   $	��304��6�   6�   �� 5   
   -   5        $ labelDadosDoEmitente��5   R   $��0,66,0,0q�6   $��14(�6&   $
��Bold"�64   $��21��6F   $��Dados da empresa:��6p   $
��Left=�6�   $	��Top=�6�   $	��137��6�   6�   �� 5   
   -   5        $ labelCNPJRazaoSocial��5   B   $��
0,87,309,0q�6   $��14(�6(   $��21��66   $?��9CNPJ: 01234567891012, Razão Social: Gustavo Gialim Costa��6`   $��Stretch=�6�   $	��Top=�6�   6�   �� 5    
   -   5         ��Name$ btnRelatorio��5       $��Right=�6'   $	��Top=�6C   
�������	 � Cursor$
	 Hand��6n   $��0q�6|   $��0,32,5,0q�6�   $��53��6�   .��5!      �� $��Stretch=�6   $��Stretch=�67   $��48��6S   .+�5"      �� $��/imgs/report.png��6   $��48��62   $��48��6=   $
��Left=�6I   5#      5$      &   System.Windows.Controls.DataGrid  5&   
   -   5        
  Name$
 
dgListagem��5&   /      AutoGenerateColumns .     
IsReadOnly . $��12(�6U   $��Stretch=�6c   $��	0,113,0,0q�6�   $��Stretch=�6�   $
��Auto��6�   $
��Auto��6�      RowDetailsVisibilityMode$ 	Collapsed=�6�   ��6�   Z�    RowDetailsTemplate 5'      �� 5(      .R�5)      �� $	��135��6   $	��700��6*   .+�5*      �� $��Center=�6#   $	��700��6@   $
��Auto��6L   $	��Top=�6Z   ��6r   �� *
listaErros+ �� View 5+      �� 5,   "   .P�5-   &   �� $��Erros��65    �� Width$	 685��6D    �� DisplayMemberBinding 6P   �� *	Message+5.   #   50      51      52         Columns 55      4  .System.Windows.Controls.DataGridTemplateColumn  56      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$ 15 6)      HeaderStyle 57      �� 58       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6    .�59      ��  �� Property � �� �� Value 5:   "   �� 5;   &   5=   '   5?      5@         CellTemplate 5C      �� 5D      .R�5E      �� $ 	btExpande��6U   -   5        $
	 Hand��5E   G   $��+��6h   $	��-10q�6t   $��16(�6�   $��15��6�   $��15��6�   y�6%   �� *
Visibility+6�   5F      5H      0  *System.Windows.Controls.DataGridTextColumn  5I         Header$ Cód.��6%   $ 40 63      
Foreground �	#FF3333331  +System.Windows.Controls.DataGridBoundColumn   Binding 6>   �� *cProd+6X     5J      $ Descrição��6%   $ * 68    �	#FF333333 6B   �� *xProd+6\     5K      $ 	Valor Un.��6%   $ 85 68    �	#FF333333 6C   �� *vProd+   ElementStyle 5L      �� 5M      " ��6    .�5N      ��  � ��$ Right=�6D   5O      5Q        5R      $ 
Quantidade��6%   $ 73 69    �	#FF333333 6D   �� *qCom+ 5S      �� 5T      " ��6    .�5U      ��  � ��$ Right=�6D   5V      5X        5Y      $ Valor Total��6%   $ 90 6:    �	#FF333333 6E   �� *
vProdTotal+   PriceConverter1 �� 	Converter# [   5Z      �� 5[      " ��6    .�5\      ��  � ��$ Right=�6D   5]      5_        5`      $	 NCM��6%   $ 65 62    �	#FF333333 6=   �� *NCM+6U     5a      $
 CEST��6%   $ 55 63    �	#FF333333 6>   �� *CEST+6W     5b      $
 CFOP��6%   $ 50 63    �	#FF333333 6>   �� *CFOP+6W     5c      $ CST PIS��6%   $ 60 66    �	#FF333333 6A   �� *	CST_PIS+6]     5d      $ 
CST COFINS��6%   $ 75 69    �	#FF333333 6D   �� *
CST_COFINS+6c     5e      $ 
Qtd. Erros��6%   $ 65 69    �	#FF333333 6D   �� *quantidadeErros+6h     5f       5g      �� 5h      .R�5i      �� $��Center=�6)   $��Center=�6D   .+�5j   "   A� $��12��6*   $��12��65   
���   �$��Center=�6�   $��Center=�6�   ��6P   �� *	CorStatus+  ConverteToBrush# [ 6�    ^� ToolTip 5k   "   _� 5l   &   $��Status do Pruduto.��6.   5n      5o      5q      5s      5u      5v      