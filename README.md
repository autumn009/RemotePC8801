## RemotePC8801
Control PC-8801 from Windows 10 by serial RS-232C port

Created by Akira Kamata ©2019

# How to install
Extract files and copy to your local folder.

# Environment
 * .NET Framework 4.7.2
 * One RS-232C Port

# How to configure
1) Connect PC-8801 and your Windows machine by RS-232C
2) Set the PC-8801's serial configuration as a 9600bps RS-232C connection
3) open RemotePC8801 in Windows machine
4) type "term "N81NNF",,8192[RETURN]" in PC-8801
5) Select COM port in RemotePC8801

# How to use
1) Push PortOpen button
2) select driveX in combo-box

# Commands
 * DISK INFO -Get information about the floppy disk by DSKF()
 * FILES     -Send FILES statement for test purposes
 * IMAGE     -Read disk image to save D88 format file
 * SECTOR    -View sector dump in hexadecimal
 * DIRECT    -Execute any BASIC statement/command
 * DEBUG     -Don't use
 * PortOpen  -Open Connection
 * PortClose -Close Connection

# Not implemented yet
 * 'Write Disk Image' is not implemented
 * 'Write Sector' is not implemented
