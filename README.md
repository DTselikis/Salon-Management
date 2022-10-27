# Salon Management
A simple program for your customer base

![The program's homepage. Here you can search for an existing customer, navigate to your customer base, export your customer base and customize the enviroment](https://i.imgur.com/cjczqof.png)
## Features
### Customer base
You can keep track of your customers by creating a record for each one of them including their personal details and notes.
### Export
You can export your local DB into .xml files to be able to easily migrate your customer base to any other environment you want.
### Backups
The program automatically keeps daily and monthly backups so you don’t miss anything!
<sub><i> Daily backups will be deleted after 7 days of the day of their creations
Monthly backups will be deleted after 90 days of the day of their creation</i></sub>

### Customizable
You can change some aspects the environment such as the colors, text size, clock visibility etc.
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

### Screenshots
| <img src="https://i.imgur.com/3F8a301.png" width=420 alt="You can create a profile for each of your customers and include some basic information and notes"> | <img src="https://i.imgur.com/7nn9daa.png" width=420 alt="You can see and manage your customers"> |
|--|--|
| <img src="https://i.imgur.com/i4lYWiZ.png" width=420 alt="You can search customers"> | <img src="https://i.imgur.com/KlnI842.png" width=420 alt="You can customize various aspects of the program"> |
