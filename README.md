# <div align="center">Salon Management</div>
<div align="center"><img src="https://i.imgur.com/cjczqof.png" height="480"></div>
<div><p>A simple desktop application to manage your customer base</p></div>

<a name="readme-top"></a>
## Table of contents
- [Features](#features)
	- [Customer base](#customer-base)
	- [Export](#export)
	- [Backups](#backups)
	- [Customizable](#customizable)
- [Configuration](configuration)

## Features

### Customer base
You can keep track of your customers by creating a record for each one of them including their personal details and notes.

### Export
You can export your local DB into .xml files to be able to easily migrate your customer base to any other environment you want.

### Backups
The program automatically keeps daily and monthly backups so you don’t miss anything!
<sub>*Daily backups will be deleted after 7 days of the day of their creations*</sub>
<sub>*Monthly backups will be deleted after 90 days of the day of their creation*</sub>

### Customizable
You can change some aspects the environment such as the colors, text size, clock visibility etc.

<p align="right"><sup>(<a href="#readme-top">back to top</a>)</p></sup>

## Configuration
### Logo

##### Homepage
If you want to add your business logo at the homepage, you should add a "logo.png" image file at Resources\Images\ and uncomment lines 12-18 of HomeControl.xaml

##### Program icon
If you want to add your business logo as the program icon, you should:
- add a "logo.ico" icon file at Resources\Images\
- add the following code in inside inside the Window property of the MainWindow.xaml:
```xml
Icon="Resources\Images\logo.ico"
```
<p align="right"><sup>(<a href="#readme-top">back to top</a>)</p></sup>
<div align="center"><img src="https://i.imgur.com/c2MYIDK.png" height="580"></img></div>
