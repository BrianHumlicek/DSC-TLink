// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
// Copyright (C) 2024 Brian Humlicek
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace DSC.TLink.ITv2
{
	public enum ITv2Command : ushort
	{
		Simple_Ack = 0,
		Logging_Get_Event_Buffer_information = 256, // 0x0100
		Logging_Event_Buffer_Read_by_Event_Number = 257, // 0x0101
		Logging_Single_Event_Buffer_Read = 258, // 0x0102
		Logging_SMS_Preview_Read = 259, // 0x0103
		Logging_Single_SMS_Text_Read = 260, // 0x0104
		Logging_Delete_SMS = 261, // 0x0105
		Logging_Event_Buffer_Read_by_Date_Time_Stamp = 262, // 0x0106
		Logging_Panel_Event_Buffer_Notification = 272, // 0x0110
		Logging_Unread_SMS_Notification = 273, // 0x0111
		Logging_Panel_Communicable_Event_Tx_Notification = 274, // 0x0112
		Notification_Any_Life_Safety_Communicable_Event = 512, // 0x0200
		Notification_Text = 513, // 0x0201
		Notification_Life_Style_Zone_Status = 528, // 0x0210
		Notification_Time_Date_Broadcast = 544, // 0x0220
		Notification_Temperature_Broadcast = 545, // 0x0221
		Notification_Command_Output_Activation_Deprecated = 546, // 0x0222
		Notification_Chime_Broadcast = 547, // 0x0223
		Notification_Panel_Display_Configuration = 548, // 0x0224
		Notification_Exit_Delay = 560, // 0x0230
		Notification_Entry_Delay = 561, // 0x0231
		Notification_Arming_Disarming = 562, // 0x0232
		Notification_Arming_PreAlert = 563, // 0x0233
		Notification_Enrolment_Status = 564, // 0x0234
		Notification_General_Activity_Deprecated = 565, // 0x0235
		Notification_Access_Code_Length = 566, // 0x0236
		Notification_Partition_Buzzer_Type_Deprecated = 567, // 0x0237
		Notification_Partition_Quick_Exit = 568, // 0x0238
		Notification_Partition_Ready_Status = 569, // 0x0239
		Notification_System_Test = 570, // 0x023A
		Notification_Partition_Audible_Bell = 571, // 0x023B
		Notification_Partition_Alarm_Memory = 572, // 0x023C
		Notification_Miscellaneous_PreAlert = 573, // 0x023D
		Notification_Partition_Blank_Status = 574, // 0x023E
		Notification_Partition_Trouble_Status = 575, // 0x023F
		Notification_Partition_Bypass_Status = 576, // 0x0240
		Notification_Partition_Busy_Status = 577, // 0x0241
		Notification_Partition_Based_General_Activity = 578, // 0x0242
		Notification_Signal_Strength = 579, // 0x0243
		Notification_Command_Output_Activation_Extended = 580, // 0x0244
		Notification_Partition_Banner_Notification = 581, // 0x0245
		Notification_Partition_Buzzer_Type_Extended = 582, // 0x0246
		Notification_Buffer_Overload = 752, // 0x02F0
		Upgrade_Firmware_Available = 768, // 0x0300
		Upgrade_Start_Download = 769, // 0x0301
		Upgrade_Cancel_Download = 770, // 0x0302
		Upgrade_Download_Complete = 771, // 0x0303
		Upgrade_Firmware_Data_Packet = 772, // 0x0304
		Upgrade_Firmware_Available_Critical = 773, // 0x0305
		Upgrade_Firmware_Update_Status_Notification = 774, // 0x0306
		AccessLevel_Enter = 1024, // 0x0400
		AccessLevel_Exit = 1025, // 0x0401
		AccessLevel_Lead_InOut = 1026, // 0x0402
		Command_Error = 1281, // 0x0501
		Command_Response = 1282, // 0x0502
		Connection_Poll = 1536, // 0x0600
		Connection_Open_Session = 1546, // 0x060A
		Connection_End_session = 1547, // 0x060B
		Connection_Inform_TX_and_RX_buffer_sizes = 1548, // 0x060C
		Connection_Software_Version = 1549, // 0x060D
		Connection_Request_Access = 1550, // 0x060E
		Connection_Pass_Through_ITv2_Command = 1554, // 0x0612
		Connection_System_Capabilities = 1555, // 0x0613
		Connection_Panel_Status = 1556, // 0x0614
		Connection_Device_Enrolment = 1557, // 0x0615
		Connection_Encapsulated_Command_for_Long_Packets = 1570, // 0x0622
		Connection_Encapsulated_Command_for_Multiple_Packets = 1571, // 0x0623
		Configuration_Enter_Deprecated = 1792, // 0x0700
		Configuration_Exit = 1793, // 0x0701
		Configuration_Panel_Programming_Lead_InOut = 1794, // 0x0702
		Configuration_Access_Code_Wrapper = 1795, // 0x0703
		Configuration_Enter = 1796, // 0x0704
		Configuration_Installers_Section_Read = 1825, // 0x0721
		Configuration_Installers_Section_Write = 1826, // 0x0722
		Configuration_Read_Time_and_Date = 1841, // 0x0731
		Configuration_Read_Late_To_Open = 1842, // 0x0732
		Configuration_Read_Late_To_Open_Time = 1843, // 0x0733
		Configuration_Read_Voice_dialler = 1844, // 0x0734
		Configuration_Read_Bypass_Zone = 1845, // 0x0735
		Configuration_Read_Access_Code = 1846, // 0x0736
		Configuration_Read_Access_Code_Attribute = 1847, // 0x0737
		Configuration_Read_Access_Code_Partition_Assignment = 1848, // 0x0738
		Configuration_Read_Auto_Arm_Time = 1849, // 0x0739
		Configuration_Read_User_Code_Configuration = 1852, // 0x073C
		Configuration_Read_Late_To_Open_Time_Single_Day = 1853, // 0x073D
		Configuration_Read_Auto_Arm_Time_Read_Single_Day = 1854, // 0x073E
		Configuration_Write_Time_and_Date = 1857, // 0x0741
		Configuration_Write_Late_To_Open = 1858, // 0x0742
		Configuration_Write_Late_To_Open_Time = 1859, // 0x0743
		Configuration_Write_Voice_dialler = 1860, // 0x0744
		Configuration_Write_Bypass_Zone = 1861, // 0x0745
		Configuration_Write_Access_Code = 1862, // 0x0746
		Configuration_Write_Access_Code_Attribute = 1863, // 0x0747
		Configuration_Write_Access_Code_Partition_Assignment = 1864, // 0x0748
		Configuration_Write_Auto_Arm_Time_Program = 1865, // 0x0749
		Configuration_Write_Single_Zone_Bypass_Write = 1866, // 0x074A
		Configuration_Write_Group_Bypass = 1867, // 0x074B
		Configuration_Write_Proximity_Tag_Programming = 1868, // 0x074C
		Configuration_Write_Late_To_Open_Time_Single_Day = 1869, // 0x074D
		Configuration_Write_Auto_Arm_Time_Program_Single_Day = 1870, // 0x074E
		Configuration_Write_Reset_Programming_to_Factory_Default = 1871, // 0x074F
		Configuration_Write_Access_Codes_Label = 1872, // 0x0750
		Configuration_Read_Event_Reporting_Configuration = 1889, // 0x0761
		Configuration_Write_Event_Reporting_Configuration = 1890, // 0x0762
		Configuration_Notification_Zone_Assignment_Configuration = 1904, // 0x0770
		Configuration_Notification_Configuration = 1905, // 0x0771
		Configuration_Notification_Partition_Assignment = 1906, // 0x0772
		Configuration_Notification_Virtual_Zone_to_Zone_Assignment = 1907, // 0x0773
		Configuration_Notification_Configuration_Extend = 1908, // 0x0774
		ModuleStatus_Command_Request = 2048, // 0x0800
		ModuleStatus_Global_Status = 2064, // 0x0810
		ModuleStatus_Zone_Status = 2065, // 0x0811
		ModuleStatus_Partition_Status = 2066, // 0x0812
		ModuleStatus_Zone_Bypass_Status = 2067, // 0x0813
		ModuleStatus_System_Trouble_Status_Deprecated = 2068, // 0x0814
		ModuleStatus_Alarm_Memory_Information = 2069, // 0x0815
		ModuleStatus_Bus_Status = 2070, // 0x0816
		ModuleStatus_Trouble_Detail = 2071, // 0x0817
		ModuleStatus_Door_Chime_Status = 2073, // 0x0819
		ModuleStatus_Single_Zone_Bypass_Status = 2080, // 0x0820
		ModuleStatus_Grouped_Trouble_Status_Deprecated = 2081, // 0x0821
		ModuleStatus_System_Trouble_Status_New = 2082, // 0x0822
		ModuleStatus_Trouble_Detail_Notification = 2083, // 0x0823
		ModuleStatus_Zone_Alarm_Status = 2112, // 0x0840
		ModuleStatus_Miscellaneous_NonZone_Alarm_Status = 2113, // 0x0841
		ModuleStatus_Diagnostic_Status = 2114, // 0x0842
		ModuleControl_Partition_Arm_Control = 2304, // 0x0900
		ModuleControl_Partition_Disarm_Control = 2305, // 0x0901
		ModuleControl_Command_Output = 2306, // 0x0902
		ModuleControl_System_Testing_Deprecated = 2307, // 0x0903
		ModuleControl_Door_Chime_Status_Write_Deprecated = 2308, // 0x0904
		ModuleControl_Enable_DLS_Window = 2309, // 0x0905
		ModuleControl_User_Callup = 2310, // 0x0906
		ModuleControl_Auto_Arm_EnableDisable = 2311, // 0x0907
		ModuleControl_Activate_FAP = 2312, // 0x0908
		ModuleControl_Initiate_Label_Broadcast = 2313, // 0x0909
		ModuleControl_Partition_Quick_Exit = 2320, // 0x0910
		ModuleControl_Silence_Troubles_Deprecated = 2321, // 0x0911
		ModuleControl_User_Activity = 2322, // 0x0912
		ModuleControl_Partition_Banner_Display = 2323, // 0x0913
		ModuleControl_Partition_Buzzer_Control = 2324, // 0x0914
		ModuleControl_Virtual_Zone_Control = 2325, // 0x0915
		ModuleControl_Set_Partition_Mode = 2326, // 0x0916
		ModuleControl_Door_Chime_Status_Write_Extended = 2327, // 0x0917
		ImageTransfer_Image_File_Header_Command = 3074, // 0x0C02
		ImageTransfer_File_Transfer_Data_Blocks = 3075, // 0x0C03
		VirtualKeypad_Control = 3840, // 0x0F00
		VirtualKeypad_Notification_Key_Pressed = 3841, // 0x0F01
		VirtualKeypad_Notification_LCD_Update = 3842, // 0x0F02
		VirtualKeypad_Notification_LCD_Cursor = 3843, // 0x0F03
		VirtualKeypad_Notification_LED_Status = 3844, // 0x0F04
		Response_Get_Event_Buffer_Information = 16640, // 0x4100
		Response_Event_Buffer_Read_By_Date_Time_Stamp = 16641, // 0x4101
		Response_Event_Buffer_Read_By_Event_Number = 16641, // 0x4101
		Response_Single_Event_Buffer_Read = 16642, // 0x4102
		Response_SMS_Preview_Read = 16643, // 0x4103
		Response_Single_SMS_Text_Read = 16644, // 0x4104
		Response_Installers_Section_Read = 18209, // 0x4721
		Response_Time_and_Date = 18225, // 0x4731
		Response_Late_To_Open = 18226, // 0x4732
		Response_Late_To_Open_Time = 18227, // 0x4733
		Response_Voice_Dialler = 18228, // 0x4734
		Response_Bypass_Zone = 18229, // 0x4735
		Response_Access_Code = 18230, // 0x4736
		Response_Access_Code_Attribute = 18231, // 0x4737
		Response_Access_Code_Partition_Assignment = 18232, // 0x4738
		Response_Auto_Arm_Time = 18233, // 0x4739
		Response_User_Code_Configuration = 18236, // 0x473C
		Response_Late_To_Open_Time_Single_Day = 18237, // 0x473D
		Response_Auto_Arm_Time_Read_Single_Day = 18238, // 0x473E
		Response_Event_Reporting_Configuration_Read = 18273, // 0x4761
	}
}