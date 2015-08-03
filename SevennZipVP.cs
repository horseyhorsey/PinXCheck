using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinballXManager
{
    class SevennZipVP
    {
        public string appPath = System.IO.Directory.GetCurrentDirectory();
        public string sevenzipPath = @"7za.exe";

        public void GetTableInfo(string tableFile,string tablename,string system,string xmlElement)
        {
            DirectoryInfo di = new DirectoryInfo("Temp");
            string Arguments = string.Empty;
            File.Delete("Table Data");
            File.Delete("AuthorName");
            var di2 = Path.Combine(di.FullName, @"Temp");
           // Directory.CreateDirectory(di.FullName + "\\" + tablename);
            //di = new DirectoryInfo("Temp");
           // Directory.CreateDirectory(di.FullName + "\\" + tablename);

            //string s = di.FullName;
           // s = @" "\"" + s + "\"" ";
            if (system =="Visual Pinball")
                if (xmlElement=="Author")
                    Arguments = @"e " + tableFile + " o" + di2 + " TableInfo\\AuthorName -y";
                else if (xmlElement=="Rom")
                    Arguments = @"e " + tableFile + " o" + di2 + " GameStg\\GameData -y";
            else if (system =="Future Pinball")
                Arguments = @"e " + tableFile + " o" + di2 +  " " + "\"" + "Future Pinball\\Table Data" + "\"" + " -y";
           ProcessStartInfo si = new ProcessStartInfo(sevenzipPath,Arguments);
           si.UseShellExecute = false;
           si.CreateNoWindow = true;
           Process.Start(si).WaitForExit();

           //Arguments = "x " + tableFile + " o" + appPath + "\\TableInfo" + "\\" + tablename + " GameStg\\GameData -y > NUL:";
           //si = new ProcessStartInfo(sevenzipPath, Arguments);
           //si.UseShellExecute = false;
           //si.CreateNoWindow = true;
           //Process.Start(si).WaitForExit();
        }

    }

    class VPLaunch
    {
        public void launchVP(string tpath, string tname, string executable, string wpath, string defaultExe, int keycode, string system, string scriptType, bool desktop, int camera = 0)
        {
            System.Windows.Forms.KeysConverter converter = new System.Windows.Forms.KeysConverter();
            string text = converter.ConvertToString(keycode);

            if (executable == string.Empty)
                executable = defaultExe;
            string Arguments = string.Empty;
            string exe = string.Empty;
            if (system == "Visual Pinball")
                exe = "VPLaunch.exe";
            else if (system == "P-ROC")
                exe = "PROCLaunch.exe";

            Arguments = tpath + " " + tname + " " + executable + " " + wpath + " " + text + " " + scriptType + " " + desktop + " " + camera;
            ProcessStartInfo si = new ProcessStartInfo(exe, Arguments);
            si.UseShellExecute = true;
            si.CreateNoWindow = true;
            Process.Start(si);
        }

        public void launchPROC(string tpath, string tname, string executable, string wpath, string defaultExe, int keycode, string system,string scriptType,bool desktop,int camera=0)
        {
            System.Windows.Forms.KeysConverter converter = new System.Windows.Forms.KeysConverter();
            string text = converter.ConvertToString(keycode);

            if (executable == string.Empty)
                executable = defaultExe;
            string Arguments = string.Empty;
            string exe = "PROCLaunch.exe";

            if (scriptType !=string.Empty)
                Arguments = tpath + " " + tname + " " + executable + " " + wpath + " " + text + " " + scriptType + " " + desktop + " " + camera;
            else
                Arguments = tpath + " " + tname + " " + executable + " " + wpath + " " + text + " " + scriptType + " " + desktop + " " + camera;
            ProcessStartInfo si = new ProcessStartInfo(exe, Arguments);
            si.UseShellExecute = true;
            si.CreateNoWindow = true;
            Process.Start(si);
        }

        public void lanuchBAM(string tpath, string tname, string executable, string wpath, string defaultExe, int keycode, string system, string scriptType, bool desktop, int camera = 0, string BAMExe="")
        {
            System.Windows.Forms.KeysConverter converter = new System.Windows.Forms.KeysConverter();
            string text = converter.ConvertToString(keycode);
            if (executable == string.Empty)
                executable = defaultExe;
            string Arguments = string.Empty;
            string exe = BAMExe;
            var p = Path.Combine(tpath, tname);
            if (scriptType != string.Empty)
                Arguments = @"/STAYINRAM /FPEXE:" + "\"" + executable + "\"" + " /open " + "\"" + p + "\"" + " /play /exit /arcaderender,,hide UseErrorLevel";
            else
                Arguments = @"/STAYINRAM /FPEXE:" + "\"" + executable + "\"" + " /open " + "\"" + p + "\"" + " /play /exit /arcaderender,,hide UseErrorLevel";
            ProcessStartInfo si = new ProcessStartInfo(exe, Arguments);
            si.WorkingDirectory = wpath;
            si.UseShellExecute = true;
            si.CreateNoWindow = true;
            Process.Start(si);
        }
    }

    public class MediaFiles : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        private FileInfo file;
        public FileInfo File
        {
            get { return file; }
            set
            {
                file = value;
                this.NotifyPropertyChanged("file");
            }
        }
        public bool TagRename { get; set; }
        public string MatchedName { get; set; }
        public string MediaType { get; set; }

        public MediaFiles() { }
        public MediaFiles(FileInfo file,string MatchedName,bool rename,string type) 
        {
            this.File = file;
            this.TagRename = rename;
            this.MatchedName = MatchedName;
            this.MediaType = type;
        }


    }
}

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SevenZipVisualPinball
//{
//    class Program
//    {       
//        static void Main(string[] args)
//        {
//            SevenZipVP sevenVP = new SevenZipVP();
//            string sevenzipPath = @"7z.exe";
//            string appPath = System.IO.Directory.GetCurrentDirectory();
//            string path = Path.Combine(appPath, @"TableInfo");
//            DirectoryInfo di = new DirectoryInfo(path);
//            FileInfo[] fi = di.GetFiles("*.*");

//            int counter = 0;
//            string line;
//            foreach (var item in fi)
//            {
//                // Read the file and display it line by line.
//                System.IO.StreamReader file =
//                   new System.IO.StreamReader(item.FullName, Encoding.Unicode);
//                while ((line = file.ReadLine()) != null)
//                {
//                    Console.WriteLine(line);
//                    counter++;
//                }

//                file.Close();
//            }

//            sevenVP.GetTableInfo(@"I:\Emulators\Visual Pinball\tables\301 Bullseye FS.vpt");
//            Console.Read();

//        }

       
//    }

//    class SevenZipVP
//    {
//        public void GetTableInfo(string tableFile)
//        {


//            Process p = new Process();
//            ProcessStartInfo si = new ProcessStartInfo(sevenzipPath);
//            si.Arguments = @"x " + "\"" + tableFile + "\"" + " TableInfo -y";
//            Console.WriteLine(sevenzipPath);
//            Console.WriteLine(si.Arguments);
//            p.StartInfo.FileName = sevenzipPath;
//            p.StartInfo = si;
//            p.Start();
//            p.WaitForExit(500);
//            p.Close();


//        }
//    }