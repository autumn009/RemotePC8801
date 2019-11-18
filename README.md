## RemotePC8801
Control PC-8801 from Windows 10 by serial RS-232C port
Created by Akira Kamata ©2019

# How to install
Extract files and copy to your local folder.

# Environment
 * .NET Framework 4.7.2
 * One RS-232C Port

# How to configure
1) Connect PC-8801 and your Windows Machine by RS-232C
2) Set to PC-8801 as 9600bps RS-232C Connection
3) open RemotePC8801 in Windows Machine
4) type "term "N81NNF",,8192[RETURN]" in PC-8801
5) Select COM port in RemotePC8801

# How to use
1) Push PortOpen button
2) select driveX in combo-box

# Commands
DISK INFO -Get infomations about the floppy disk by DSKF()
FILES     -Send FILES statement for test purpose
IMAGE     -Read Disk image to save D88 format file
SECTOR    -View Sector Dump by hexadecimal
DIRECT    -Excecute any BASIC statement/command
DEBUG     -Don't use
PortOpen  -Open Connection
PortClose -Close Connection

# Not implemented yet
'Write Disk Image' is not implemented
'Write Sector' is not implemented
