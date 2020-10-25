using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon_App_WPF
{
    public class Logger
    {
        private const string Package = "Salon Management";
        private const string SectionStr = "----------------------";

        private string _directory;
        private string _fileName;

        public Logger()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            this._directory = Path.Combine(appData, Package, "Resources");
            this._fileName = "log_" + DateTime.Today.ToString("yyyy_MM_dd") + ".log";

            try
            {
                Directory.CreateDirectory(this._directory);
            }
            catch (Exception ex)
            {

            }
        }

        public Logger(string filename)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            this._directory = Path.Combine(appData, Package, "Resources");
            this._fileName = filename + "_" + DateTime.Today.ToString("yyyy_MM_dd") + ".log";

            try
            {
                Directory.CreateDirectory(this._directory);
            }
            catch (Exception ex)
            {

            }
        }

        public void Section(string sectioName) {
            using (StreamWriter sw = File.AppendText(Path.Combine(this._directory, this._fileName)))
            {
                try
                {
                    sw.WriteLine(SectionStr + " " + sectioName + " " + SectionStr);
                }
                catch (Exception ex)
                {
                    sw.WriteLine("Logger");
                    sw.WriteLine(ex);
                }
            }
        }

        public void Log(string log)
        {
            using (StreamWriter sw = File.AppendText(Path.Combine(this._directory, this._fileName)))
            {
                try
                {
                    sw.Write(DateTime.Now.ToString("G") + " ");
                    sw.WriteLine(log);
                }
                catch (Exception ex)
                {
                    sw.WriteLine("Logger");
                    sw.WriteLine(ex);
                }
            }
        }
    }
}
