   M S B A M L   `   `   `  ����B  >Monitor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:MonitorMonitor  5   	       Monitor.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   'localclr-namespace:Monitor     5   	   \ XMicrosoft.Kinect.Toolkit.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nullS OMicrosoft.Kinect.Toolkit, Version=1.7.0.0, Culture=neutral, PublicKeyToken=null2k(http://schemas.microsoft.com/kinect/2013   5   	   -   5          � Title$  Face Tracking 3D��5   	   $	��910��5   	   $
��1342��6   .��5      � -   5         �Name$ 
layoutGrid��5      $��
10 0 10 10q�6    � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      $��* �6   .+�5   
   $   Monitor.TexturedFaceMeshViewer  -   5          Name$ faceTrackingVisualizer��5   O   $��1��6'   $
��Left=�64   $	��Top=�6o   $	��690��6�   $��0,42,0,0q�6�   ]� 5   
   $��0��6   $��0 0 0 20q�6!   .+�5      �� $
��Left=�6   $��Images\Logo.png��6*   $
��Fill=�6C   $��56��6R   $	��183��6^   $��0 10 0 5q�6j   �� 5      -   5         ��Name$ image1��5       $��53��6   $
��Fill=�60   $	��380��6?   5      (� 5   
   $��1��6    (� Stretch$ Uniform=�6   $��Center=�61   .��5      � 5      4  .Microsoft.Kinect.Toolkit.Controls.KinectRegion  5   
   -   5          Name$ kinectRegion��5      $
��Left=�6/   $��804,0.333,0,0q�6J   $��1��6a   $	��Top=�6n   $	��770��6�   $	��511��6�   :  4Microsoft.Kinect.Toolkit.Controls.KinectCircleButton  5   
   -   5        $
��EXIT��5      $
��Left=�6.   $��1090,362.333,0,0q�6I   $��1��6c   $	��Top=�6p   $	��259��6�   $	��225��6�   6�   5       � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush	 �� 
TargetType
 �� Property �� Value��%�   &	        &	 #     &	 F     '�� i     �� 5   
   $ 	#ff6e6e6e��62   �� 5   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   "	 ��6   .�5      �� 
 �@J�SnapsToDevicePixels . 64   5      5       5!      