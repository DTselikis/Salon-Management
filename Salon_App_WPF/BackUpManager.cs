﻿using Microsoft.Win32;
using Salon_App_WPF.CustomersDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Xml.Serialization;

namespace Salon_App_WPF
{
    public class BackUpManager
    {
        private string connStr = Properties.Settings.Default.DBConnStr;

        private const string Package = "Salon Management";
        private const string BackUpFolder = "BackUp";
        private const string Daily = "Daily";
        private const string Monthly = "Monthly";
        private const string XMLFolder = "XML";

        private const string ResourcesFolder = "Resources";
        private const string ImagesFolder = "Images";
        private const string ProfileImageFolder = "ProfileImages";

        private string appData;
        private string dailyPath;
        private string monthlyPath;
        private string todayPath;
        private string profileImgPath;
        private string dbBasePath;

        private string destinationPath;
        private string destProfImgPath;


        public BackUpManager()
        {
            appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dailyPath = Path.Combine(appData, Package, BackUpFolder, Daily);
            monthlyPath = Path.Combine(appData, Package, BackUpFolder, Monthly);

            todayPath = Path.Combine(dailyPath, DateTime.Now.ToString("yyyy_MM_dd"));
            profileImgPath = Path.Combine(appData, Package, ResourcesFolder);
            profileImgPath = Path.Combine(profileImgPath, ImagesFolder, ProfileImageFolder);
            dbBasePath = todayPath;

            destinationPath = Path.Combine(todayPath, XMLFolder);
            destProfImgPath = Path.Combine(todayPath, ProfileImageFolder); 
        }

        public BackUpManager(string destinationPath)
        {
            this.destinationPath = destinationPath;
            this.destinationPath = Path.Combine(destinationPath, "XML");

            destProfImgPath = Path.Combine(destinationPath, ImagesFolder);
            dbBasePath = destinationPath;

            appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            profileImgPath = Path.Combine(appData, Package, ResourcesFolder);
            profileImgPath = Path.Combine(profileImgPath, ImagesFolder, ProfileImageFolder);
        }

        public void Initialize()
        {
            // Check if there are old backups to be removed
            DeleteOld();

            BackUp(false);
        }

        public void Export()
        {
            BackUp(true);
        }

        private void DeleteOld()
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(dailyPath))
                {
                    // Indexing starts from 0
                    int index = directory.LastIndexOf('\\') + 1;
                    string dirName = directory.Substring(index, directory.Length - index);
                    TimeSpan interval = DateTime.Today - DateTime.Parse(dirName.Replace('_', '/'));

                    // Delete records of last week
                    if (interval.Days >= 7) Directory.Delete(directory, true);
                }

                foreach (string directory in Directory.GetDirectories(monthlyPath))
                {
                    // Indexing starts from 0
                    int index = directory.LastIndexOf('\\') + 1;
                    string dirName = directory.Substring(index, directory.Length - index);
                    TimeSpan interval = DateTime.Today - DateTime.Parse(dirName.Replace('_', '/'));

                    // Delete records of last three months
                    if (interval.Days >= 90) Directory.Delete(directory, true);
                }
            }
            catch (DirectoryNotFoundException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private void BackUp(bool isExport)
        {
            // Backup only once a day
            if (!Directory.Exists(destinationPath) || isExport)
            {
                // If the directory already exists, this method does not create a new directory
                Directory.CreateDirectory(destinationPath);
                BackUpDBToXML(null);

                // If profile pictures exists
                if(Directory.Exists(profileImgPath))
                {
                    DirectoryCopy(profileImgPath, destProfImgPath, true);
                }

                BackUpDB();
            }
            // If today backup was completed check if any new records was added
            else
            {
                using (SqlConnection dbConn = new SqlConnection(connStr))
                {
                    try
                    {
                        dbConn.Open();
                    }
                    catch (SqlException ex)
                    {

                    }

                    string query = "SELECT COUNT(*) FROM dbo.Customers";

                    SqlCommand command = new SqlCommand(query, dbConn);

                    SqlDataReader dataReader = command.ExecuteReader();
                    
                    // At the very first run DB will be empty
                    if(dataReader.Read())
                    {
                        int numOfRecords = dataReader.GetInt32(0);

                        // If new records were added
                        if (numOfRecords > Directory.GetFiles(destinationPath).Length)
                        {
                            BackUpDBToXML(DateTime.Today);

                            BackUpTodayImages(profileImgPath, destProfImgPath);
                        }

                        BackUpDB();
                        
                    }
                }
            }

            // If user requested an export we don't want the monthly backup
            if (!isExport)
            {
                // Monthly backup
                // Change path to current month
                destinationPath = Path.Combine(monthlyPath, DateTime.Today.ToString("yyyy_MM"));
                if (!Directory.Exists(destinationPath))
                {
                    try
                    {
                        Directory.CreateDirectory(destinationPath);
                        BackUpDBToXML(null);

                        // Backup every profile image
                        if (Directory.Exists(profileImgPath))
                        {
                            DirectoryCopy(profileImgPath, Path.Combine(destinationPath, ImagesFolder), true);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                // Change path to current month
                dbBasePath = destinationPath;

                BackUpDB();

                // Retore original paths
                destinationPath = todayPath;
                dbBasePath = destinationPath;
            }
        }

        private void BackUpDBToXML(Nullable<DateTime> today)
        {
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {

                }

                string query = "SELECT * FROM dbo.Customers";
                if (today != null) query += " WHERE FirstVisit = @Today";

                SqlCommand command = new SqlCommand(query, dbConn);

                if (today != null)
                {
                    command.Parameters.AddWithValue("@Today", System.Data.SqlDbType.DateTime);
                    command.Parameters["@Today"].Value = today;
                }

                SqlDataReader customerReader = command.ExecuteReader();

                while(customerReader.Read())
                {
                    int customerID;
                    string firstName;
                    string lastName;
                    string nickName;
                    string phone;
                    string email;
                    Nullable<DateTime> firstVisit = null;
                    char gender;

                    customerID = customerReader.GetInt32(0);
                    if (customerReader[1] != System.DBNull.Value) firstName = customerReader.GetString(1); else firstName = String.Empty;
                    if (customerReader[2] != System.DBNull.Value) lastName = customerReader.GetString(2); else lastName = String.Empty;
                    if (customerReader[3] != System.DBNull.Value) nickName = customerReader.GetString(3); else nickName = String.Empty;
                    if (customerReader[4] != System.DBNull.Value) phone = customerReader.GetString(4); else phone = String.Empty;
                    if (customerReader[5] != System.DBNull.Value) email = customerReader.GetString(5); else email = String.Empty;
                    if (customerReader[6] != System.DBNull.Value) firstVisit = customerReader.GetDateTime(6); else firstVisit = null;
                    if (customerReader[7] != System.DBNull.Value) gender = Char.Parse(customerReader.GetString(7).Substring(0, 1)); else gender = '\0';

                    List<Note> notes = getNotes(customerID);

                    CreateXMLFile(new CustomerXML(customerID, firstName, lastName, nickName, phone, email, firstVisit, gender, notes));
                }

                command.Dispose();
                customerReader.Close();
                dbConn.Close();
            }
        }

        private void CreateXMLFile(CustomerXML customer)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(CustomerXML));

            StringBuilder fileName = new StringBuilder();
            fileName.Append(customer.FirstName);
            if (!customer.LastName.Equals(string.Empty)) fileName.Append("_").Append(customer.LastName);

            string fullPath = Path.Combine(destinationPath, fileName.ToString());

            // For records with the same name, append the last name or phone number
            if (File.Exists(fullPath))
            {
                if (!customer.Phone.Equals(string.Empty)) fileName.Append("_").Append(customer.Phone);
                else fileName.Append("_").Append(customer.CustomerID);

                fullPath = Path.Combine(destinationPath, fileName.ToString());
            }

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                try
                {
                    serializer.Serialize(stream, customer);
                }
                catch (Exception ex)
                {

                }
                
            }
        }

        private List<Note> getNotes(int customerID)
        {
            List<Note> notes = new List<Note>();
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {

                }

                // Notes will be saved from newer to older
                string noteQuery = "SELECT NoteID, Note, CreationDate FROM dbo.Notes WHERE CustomerID = @ID ORDER BY NoteID DESC";

                SqlCommand noteCommand = new SqlCommand(noteQuery, dbConn);
                noteCommand.Parameters.AddWithValue("@ID", System.Data.SqlDbType.Int);
                noteCommand.Parameters["@ID"].Value = customerID;

                SqlDataReader noteReader = noteCommand.ExecuteReader();

                while (noteReader.Read())
                {
                    notes.Add(new Note(noteReader.GetString(1), noteReader.GetDateTime(2)));
                }

                noteCommand.Dispose();
                noteReader.Close();
                dbConn.Close();
            }

            return notes;
        }

        private void BackUpDB()
        {
            using (SqlConnection dbConn = new SqlConnection(connStr))
            {
                try
                {
                    dbConn.Open();
                }
                catch (SqlException ex)
                {

                }

                string query = "BACKUP DATABASE @DBPath TO DISK = @Path";

                SqlCommand command = new SqlCommand(query, dbConn);
                command.Parameters.AddWithValue("@DBPath", System.Data.SqlDbType.NVarChar);
                command.Parameters.AddWithValue("@Path", System.Data.SqlDbType.NVarChar);
                command.Parameters["@DBPath"].Value = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SalonDB.mdf");
                command.Parameters["@Path"].Value = Path.Combine(dbBasePath, "SalonDB.mdf");

                command.ExecuteNonQuery();

                command.Dispose();
                dbConn.Close();
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the destination directory doesn't exist, create it.       
                Directory.CreateDirectory(destDirName);

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string tempPath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void BackUpTodayImages(string sourceDirName, string destDirName)
        {
            try
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                // If the destination directory doesn't exist, create it.       
                Directory.CreateDirectory(destDirName);

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.CreationTime.Date == DateTime.Today)
                    {
                        string tempPath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(tempPath, true);
                    }
                }
            } catch (Exception ex)
            {

            }
        }
    }

    public class CustomerXML : Customer {

        [XmlArrayItem("Note")]
        private List<Note> _notes;

        public List<Note> Notes { get { return this._notes; } set { this.Notes = value; } }
        public CustomerXML()
        {

        }

        public CustomerXML(int customerID, string firstName, string lastName, string nickName, string phone, string email, Nullable<DateTime> firstVisit, char gender, List<Note> notes) : base(customerID, firstName, lastName, nickName, phone, email, firstVisit, gender)
        {
            this._notes = notes;
        }
    }

    public class Note
    {
        private string _note;
        private Nullable<DateTime> _creationDate;

        public string Text { get { return this._note; } set { this._note = value; } }
        public Nullable<DateTime> CreationDate { get { return this._creationDate; } set { this.CreationDate = value; } }


        public Note()
        {

        }
        public Note(string note, Nullable<DateTime> creationDate)
        {
            this._note = note;
            this._creationDate = creationDate;
        }
    }
}