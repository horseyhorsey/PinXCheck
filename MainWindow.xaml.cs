using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;
using MahApps.Metro.Controls;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System.Data;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SevenZip;
using System.Diagnostics;
using Xceed.Wpf.Toolkit;


namespace PinballXManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. 
    /// PinballX Directory I:\PinballX
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ObservableCollection<OddTables> oddTables;
        static List<PinXSystem> Pinxsystem;
        ObservableCollection<Table> master_tables;
        ObservableCollection<PinXSystem> systemExecutables;
        ObservableCollection<Table> tables = new ObservableCollection<Table>();
        // public BindingList<Table> tables;
        public string XMLPath;
        public const string ConfigPath = @"Config\PinballX.ini";
        PinballX px = new PinballX();
        public string ImageUrl { get; set; }
        public string[] wheel_icon;
        public string selected_wheel;
        public string MediaType = string.Empty;
        public List<string> Executables;
        public int gameCount;
        public string systemName;
        public string[] Executable;
        private string database_name;
        public bool matchToTable, matchToMasterTable, yearGreater, flagMatchEnabled = false, RegExBracketsTable, RegExBracketsMaster;
        public int distance;
        List<FileInfo> haveWheel = new List<FileInfo>();
        BackgroundWorker bw = new BackgroundWorker();
        Process clsProcess2 = new Process();

        List<MediaFiles> listWheels;
        List<MediaFiles> listTImgs;
        List<MediaFiles> listTVids;


        public MainWindow()
        {
            master_tables = new ObservableCollection<Table>();
            InitializeComponent();
            //Create master tables for database.
            // CreateMasterTables();

            try
            {
                XMLPath = @"Databases\Visual Pinball\Visual Pinball.xml";
                setSystemsFromINI();


                foreach (var item in Pinxsystem)
                {
                    comboBox_syslist.Items.Add(item.SysName);
                }

                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.ProgressChanged += bw_ProgressChanged;
                bw.DoWork += bw_DoWork;
            }
            catch (Exception)
            {
                
                //throw;
            }


        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //CreateMasterTables(); TODO

            lv.Items.Refresh();

            comboBox_syslist.SelectedIndex = 0;
            datagridSettings.ItemsSource = Pinxsystem;
            datagridSettings_Copy.ItemsSource = Pinxsystem;

        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

        }

        public int yearMatchFilter;
        public int scanCount;
        //Run match scan button
        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            progressBar1.Value = 0;
            RegExBracketsTable = false;
            RegExBracketsMaster = false;
            buttonRename.IsEnabled = false;
            if (radioTable.IsChecked.Value)
                matchToTable = true;
            else if (radioDesc.IsChecked.Value)
                matchToTable = false;

            if (radioTable2.IsChecked.Value)
                matchToMasterTable = true;
            else if (radioDesc2.IsChecked.Value)
                matchToMasterTable = false;

            if (yearGreat.IsChecked.Value)
                yearGreater = true;
            else if (yearLess.IsChecked.Value)
                yearGreater = false;

            if (regexTableName.IsChecked.Value)
                RegExBracketsTable = true;

            scanCount = tables.Count;
            yearMatchFilter = int.Parse(YearMatch.Text);
            progressBar1.Maximum = scanCount;

            distance = int.Parse(MatchDistance.Text);
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;

            bw.DoWork += bw_DoWork;

            GuiControlsEnabled(false);
            bw.RunWorkerAsync();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            List<Table> li = new List<Table>();
            int count = 0;
            int i = 0;
            string inputTableName, inputTableDescription, TableNameEdit="", TableDescriptionEdit="",masterTableName="",masterTableDesc="";
            string replacement = "";
            string patterns = @"\(.*\)";
            Regex rgx = new Regex(patterns);
            Table bestTableMatch = new Table();

            foreach (var table in tables)
            {                
                // Remove everything in the Parenthesys from Table or description
                inputTableName = table.Name;
                inputTableDescription = table.Description;
                if (RegExBracketsTable)
                {
                    TableNameEdit = rgx.Replace(inputTableName, replacement);
                    TableDescriptionEdit = rgx.Replace(inputTableDescription, replacement);
                }

                table.FlagRename = false;
                table.MatchedName = "";
                if (yearGreater)
                {
                    if (table.Year > yearMatchFilter || table.Year == 0)
                        flagMatchEnabled = true;
                    else flagMatchEnabled = false;
                }
                else
                {
                    if (table.Year < yearMatchFilter || table.Year == 0)
                        flagMatchEnabled = true;
                    else flagMatchEnabled = false;
                }

                if (flagMatchEnabled)
                {
                   if (!table.MatchedDescription)
                        {
                            foreach (var item in master_tables)
                            {
                                if (RegExBracketsMaster)
                                {
                                    masterTableName = rgx.Replace(item.Name, replacement);
                                    masterTableDesc = rgx.Replace(item.Description, replacement);
                                }

                                if (yearGreater)
                                {
                                    if (item.Year > yearMatchFilter )
                                        flagMatchEnabled = true;
                                    else flagMatchEnabled = false;
                                }
                                else
                                {
                                    if (item.Year < yearMatchFilter)
                                        flagMatchEnabled = true;
                                    else flagMatchEnabled = false;
                                }

                                if (flagMatchEnabled)
                                {
                                    if (bw.CancellationPending)
                                    {
                                        e.Cancel = true;
                                        return;
                                    }
                                    else
                                    {
                                        string pattern;
                                        string input;

                                        if (matchToTable)
                                        {
                                            if (RegExBracketsTable)
                                                pattern = TableNameEdit.ToUpper();
                                            else
                                                pattern = table.Name.ToUpper();
                                        }
                                        else
                                        {
                                            if (RegExBracketsTable)
                                                pattern = TableDescriptionEdit.ToUpper();
                                            else
                                                pattern = table.Description.ToUpper();                                           
                                        }

                                        if (matchToMasterTable)
                                        {
                                            input = item.Name.ToUpper();
                                        }
                                        else
                                        {
                                            input = item.Description.ToUpper();
                                        }

                                        Distance d = new Distance();
                                        i = d.LD(input, pattern);

                                        if (i <= distance)
                                        {
                                            distance = i;
                                            bestTableMatch = item;
                                            table.FlagRename = true;
                                        }

                                    }

                                }
                            }
                        }                    
                }
                //bw.ReportProgress(count);
                count++;
                li.Add(bestTableMatch);
                bestTableMatch = new Table();
                Int32 percentage = (Int32)Math.Round((double)(count * 100) / scanCount);
                bw.ReportProgress(percentage);

            }
            // Put the list into the background workers Result
            e.Result = li;
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //this.haveWheel = (List<FileInfo>)e.Result;
            if (!e.Cancelled)
            {
                List<Table> li = (List<Table>)e.Result;

                int i = 0;
                foreach (var item in (li))
                {
                    if (item != null)
                    {
                        tables.ElementAt(i).MatchedName = item.Description;
                        tables.ElementAt(i).MatchedManu = item.Manufacturer;
                        tables.ElementAt(i).MatchedRating = item.Rating;
                        tables.ElementAt(i).MatchedYear = item.Year;
                        tables.ElementAt(i).MatchedType = item.Type;
                    }
                    else
                        tables.ElementAt(i).MatchedName = "";
                    i++;
                }


                buttonRename.IsEnabled = true;
            }

            GuiControlsEnabled(true);

        }

        private void GuiControlsEnabled(bool enabled)
        {
            if (enabled)
            {
                tab2.IsEnabled = true;
                tab3.IsEnabled = true;
                tables_grid.CanUserAddRows = true;
                tables_grid.IsReadOnly = false;
                comboBox_syslist.IsEnabled = true;
                comboBox_xmllist.IsEnabled = true;
                buttonRunMatch.IsEnabled = true;
                buttonNewDB.IsEnabled = true;
                buttonSaveDB.IsEnabled = true;
                buttonLoadDB.IsEnabled = true;
            }
            else
            {
                tab2.IsEnabled = false;
                tab3.IsEnabled = false;
                tables_grid.CanUserAddRows = false;
                tables_grid.IsReadOnly = true;
                comboBox_syslist.IsEnabled = false;
                comboBox_xmllist.IsEnabled = false;
                buttonRunMatch.IsEnabled = false;
                buttonNewDB.IsEnabled = false;
                buttonSaveDB.IsEnabled = false;
                buttonLoadDB.IsEnabled = false;
            }
        }

        public void getDirectorysFromIni()
        {



        }

        private void populateDataGrid()
        {            
            //link business data to CollectionViewSource
            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = tables;
            itemCollectionViewSource.Filter += itemCollectionViewSource_Filter;

        }

        private void populateOddTables()
        {
            CollectionViewSource itemCollectionViewSourcetableFiles;
            itemCollectionViewSourcetableFiles = (CollectionViewSource)(FindResource("ItemCollectionViewSourceTableFiles"));
            itemCollectionViewSourcetableFiles.Source = oddTables;

        }

        private void itemCollectionViewSource_Filter(object sender, FilterEventArgs e)
        {

        }

        #region Scan XML
        #region XML attributes
        //        <game name="AbraCaDabra_FS_B2S">
        //    <description>Abra Ca Dabra (Gottlieb 1975)</description>
        //    <rom></rom>
        //    <manufacturer>Gottlieb</manufacturer>
        //    <year>1975</year>
        //    <type>EM</type>
        //    <hidedmd>True</hidedmd>
        //    <hidebackglass>True</hidebackglass>
        //    <enabled>True</enabled>
        //    <rating>0</rating>
        //</game>
        #endregion
        public void ScanXML()
        {

            try
            {
                 string name, desc, rom = string.Empty, manu = string.Empty, type, altExe;
            bool enabled, hidedmd, hidebackglass,desktop;
            int year = 2015, rating;

            XmlDocument xdoc = new XmlDocument();
            tables = new ObservableCollection<Table>();
            //tables = new List<Table>();
            xdoc.Load(XMLPath);

            gameCount = xdoc.SelectNodes("menu/game").Count;
            wheel_icon = new string[gameCount];

            int i = 0;
            foreach (XmlNode node in xdoc.SelectNodes("menu/game"))
            {
                name = node.SelectSingleNode("@name").InnerText;
                desc = node.SelectSingleNode("description").InnerText;

                if (node.SelectSingleNode("rom") != null)
                    if (!string.IsNullOrEmpty(node.SelectSingleNode("rom").InnerText))
                        rom = node.SelectSingleNode("rom").InnerText;
                    else
                        rom = string.Empty;

                if (node.SelectSingleNode("manufacturer") != null)
                    manu = node.SelectSingleNode("manufacturer").InnerText;

                if (node.SelectSingleNode("year") != null)
                    if (!string.IsNullOrEmpty(node.SelectSingleNode("year").InnerText))
                        year = Int32.Parse(node.SelectSingleNode("year").InnerText);
                    else
                        year = 2015;

                if (node.SelectSingleNode("type") != null)
                    type = node.SelectSingleNode("type").InnerText;
                else
                    type = "SS";

                if (node.SelectSingleNode("hidedmd") != null)
                    hidedmd = Convert.ToBoolean(node.SelectSingleNode("hidedmd").InnerText);
                else
                    hidedmd = true;
                if (node.SelectSingleNode("hidebackglass") != null)
                    hidebackglass = Convert.ToBoolean(node.SelectSingleNode("hidebackglass").InnerText);
                else
                    hidebackglass = true;
                if (node.SelectSingleNode("enabled") != null)
                    enabled = Convert.ToBoolean(node.SelectSingleNode("enabled").InnerText);
                else
                    enabled = true;

                if (node.SelectSingleNode("//rating") != null)
                    rating = Int32.Parse(node.SelectSingleNode("rating").InnerText);
                else
                    rating = 0;
                if (node.SelectSingleNode("alternateExe") != null)
                    altExe = node.SelectSingleNode("alternateExe").InnerText;
                else
                    altExe = " ";
                if (node.SelectSingleNode("Desktop") != null)
                    desktop = Convert.ToBoolean(node.SelectSingleNode("Desktop").InnerText);
                else
                    desktop = false;

                tables.Add(new Table(name, desc, rom, manu, year, type, enabled, hidedmd, hidebackglass, rating, altExe,
                                        Pinxsystem[comboBox_syslist.SelectedIndex].TablePath, master_tables,desktop));

                i++;
            }

            populateDataGrid();

            }
            catch (Exception)
            {
                
                throw;
            }
           

        }
        #endregion

        static public void SerializeToXML(List<Table> tables, string systemname, string dbName)
        {

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //  Add lib namespace with empty prefix
            ns.Add("", "");
            XmlRootAttribute root = new XmlRootAttribute("menu");
            //XmlSerializer serializer = new XmlSerializer(typeof(BindingList<Table>), root);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Table>), root);
            TextWriter textWriter = new StreamWriter(@"Databases\" + systemname + "\\" + dbName);
            serializer.Serialize(textWriter, tables, ns);
            textWriter.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string ps = Pinxsystem[comboBox_syslist.SelectedIndex].SysName;
            ScanXML();
            oddTables.Clear();
            ScanTablePath(Pinxsystem[comboBox_syslist.SelectedIndex].TablePath);

            //tables = DeserializeFromXML();
            // MessageBox.Show(tables.Count.ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(XMLPath);
            XmlElement root = xdoc.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("game");
            //XmlNode myNode = root.SelectSingleNode("game/@name"); //SelectSingleNode("menu/game");

            foreach (XmlNode node in nodes)
            {
                string table = node.SelectSingleNode("@name").InnerText;
                //MessageBox.Show(node["@name"].InnerText);
                if (table == "301 Bullseye FS")
                {
                    //node["price"].InnerText = textBox1.Text;
                    //MessageBox.Show(table);
                    node["description"].InnerText = "301 BullEysssssss";
                    xdoc.Save(XMLPath);
                }
            }

            //file01.Load(filePath);
            //XmlElement root = file01.DocumentElement;
            //XmlNodeList nodes = root.SelectNodes("/Food/Fruit");
            //foreach (XmlNode node in nodes)
            //{
            //    if (node["name"].InnerText == "apple")
            //    {
            //        node["price"].InnerText = textBox1.Text;
            //    }
            //}

        }

        private void tables_grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(tables_grid.SelectedItem.ToString());
            if (tables_grid.SelectedItem is Table)
            {
                media_wheel.Source = null;
                media_company.Source = null;
                Table table_click = (Table)tables_grid.SelectedItem;
                var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);


                Uri uriSource;
                // Try catch in case filename has unsupported charcters
                try
                {

                    string wheel = System.IO.Path.GetFullPath(@"Media\" + p.SysName + "\\Wheel Images\\" + table_click.Description + ".png");
                    if (File.Exists(wheel))
                    {
                        uriSource = new Uri(wheel);
                        media_wheel.Source = new BitmapImage(uriSource);
                    }
                }
                catch (Exception)
                {

                }


                string company = System.IO.Path.GetFullPath(@"Media\Company Logos\" + table_click.Manufacturer + ".png");
                if (File.Exists(company))
                {
                    uriSource = new Uri(company);
                    media_company.Source = new BitmapImage(uriSource);
                }

                try
                {
                    uriSource = new Uri(System.IO.Path.GetFullPath(@"\Media\" + p.SysName + "\\Table Videos\\" + table_click.Description + ".mp4"));
                    //    MessageBox.Show("null");               
                    //media_view.Source = null;
                    media_view.Source = uriSource;
                    //media_view.RenderTransformOrigin = new Point(0.5, 0.5);
                    //TransformGroup group = new TransformGroup();                   
                    //group.Children.Add(new TranslateTransform(this.media_view.ActualWidth, this.media_view.ActualHeight));
                    //group.Children.Add(new RotateTransform(270));
                    //this.media_view.RenderTransform = group;                    
                }
                catch (Exception)
                {

                }

                this.Title = "-PinX Check-\t" + database_name + "   Tables: " + tables_grid.Items.Count.ToString() + "   System: " + systemName;
            }

        }

        private ImageSource GetThumbnail(string fileName)
        {
            byte[] buffer = File.ReadAllBytes(fileName);
            MemoryStream memoryStream = new MemoryStream(buffer);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = 80;
            bitmap.DecodePixelHeight = 60;
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (tables_grid.Items.Count > 0)
            {
                    List<Table> tab = new List<Table>(tables);
                    tab.Sort((x, y) => String.Compare(x.Name, y.Name));
                    SerializeToXML(tab, systemName, database_name);
            }
        }

        private bool FilterByName(object item)
        {
            Table t = item as Table;
            return t.Name.Trim() == "Family";
        }

        private void comboBox_syslist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            systemName = Pinxsystem[comboBox_syslist.SelectedIndex].SysName;
            string workingPath = Pinxsystem[comboBox_syslist.SelectedIndex].WorkingPath;

            Uri uriSource;
            DirectoryInfo di;
            FileInfo[] fi;
            systemExecutables = new ObservableCollection<PinXSystem>();

            string logo = "";//@"I:\PinballX\Media\System Logos\" + comboBox_syslist.SelectedItem + ".png";
            if (File.Exists(logo))
            {
                uriSource = new Uri(logo);
                system_logo.Source = new BitmapImage(uriSource);
            }
            comboBox_xmllist.Items.Clear();
            combobox_exe.Items.Clear();

            if (datagrid_oddtables.Items.Count != 0)
                oddTables.Clear();
            //Add the executables to a combobox

            di = new DirectoryInfo(workingPath);

            if (Directory.Exists(workingPath))
            {
                fi = di.GetFiles("*.exe");
                combobox_exe.Items.Add("");
                foreach (var item in fi)
                {
                    combobox_exe.Items.Add(item.Name);
                }
                combobox_exe.SelectedIndex = 0;


                di = new DirectoryInfo(@"Databases\" + systemName + "\\");
                fi = di.GetFiles("*.xml");

                foreach (FileInfo file in fi)
                {
                    if (file.Name == systemName + ".xml")
                        comboBox_xmllist.Items.Insert(0, file);
                    else
                        comboBox_xmllist.Items.Add(file);
                }

                comboBox_xmllist.SelectedIndex = 0;
            }

        }

        private void comboBox_xmllist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FileInfo foo = (FileInfo)comboBox_xmllist.SelectedItem;// .Items [comboBox_syslist.SelectedIndex];
                filterText.Text = "";
                database_name = foo.Name;
                XMLPath = foo.FullName;

               ScanXML();

                var tablePath = Pinxsystem[comboBox_syslist.SelectedIndex].TablePath;
                if (tablePath != "")
                    ScanTablePath(Pinxsystem[comboBox_syslist.SelectedIndex].TablePath);

                db_text.Text = "Database: " + database_name;
                this.Title = "-PinX Check-\t" + database_name + "   Tables: " + tables_grid.Items.Count.ToString() + "   System: " + systemName;

            }
            catch (Exception)
            {

                //throw;
            }



        }

        public void ScanTablePath(string path)
        {
            string ext = string.Empty;
            string ext2 = string.Empty;
            DirectoryInfo di = new DirectoryInfo(path);
            if (systemName == "Visual Pinball")
            {
                ext = ".vpt";
                ext2 = ".vpx";
            }
            else if (systemName == "Future Pinball")
                ext = ".fpt";
            else if (systemName == "P-ROC")
            {
                ext = ".vpt";
                ext2 = ".vpx";
            }

            FileInfo[] fi;
            DirectoryInfo[] dia;
            List<Table> tab = new List<Table>(tables);
            oddTables = new ObservableCollection<OddTables>();

            fi = di.GetFiles("*.*");

            string tableExt = string.Empty;
            foreach (FileInfo file in fi)
            {
                tableExt = System.IO.Path.GetExtension(file.Name);
                if (ext == tableExt || ext2 == tableExt)
                {
                    bool exist = tab.Exists(x => x.Name == System.IO.Path.GetFileNameWithoutExtension(file.Name));
                    if (!exist)
                    {
                        oddTables.Add(new OddTables(file.Name, file.LastWriteTime));
                    }
                }
            }

            populateOddTables();
        }

        /// <summary>
        /// New DataBase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            List<Table> m_tables = new List<Table>(master_tables);
            UserInputDatabaseName ui = new UserInputDatabaseName(m_tables);
            ui.DataBaseName = database_name;
            ui.SystemName = systemName;
            ui.ShowDialog();

        }

        //Rename XML
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tables_grid.ItemsSource != null)
            {
                System.Windows.Controls.TextBox t = (System.Windows.Controls.TextBox)sender;
                string filter = t.Text;
                ICollectionView cv;
                if (lv.IsVisible)
                {
                    cv = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                }
                else
                {
                    cv = CollectionViewSource.GetDefaultView(tables_grid.ItemsSource);
                }

                if (filter == "")
                    cv.Filter = null;
                else
                {
                    cv.Filter = o =>
                    {
                        Table h = o as Table;
                        return (h.Description.ToUpper().Contains(filter.ToUpper()));
                    };
                }
            }
        }

        // Remove object from a list of tables
        // get the object from the selected item in the datagrid...
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Table table_row = tables_grid.SelectedItem as Table;
            tables.Remove(table_row);
        }

        private void apply_exe_Click(object sender, RoutedEventArgs e)
        {
            if (tables_grid.SelectedIndex == -1) return;

            var h = tables_grid.SelectedItem as Table;
            h.AlternateExe = combobox_exe.Text;

            //tables_grid.Items.Refresh(); 
        }


        private void CreateMasterTables()
        {
            string text_file;
            if (!File.Exists(@"Databases\IPDB BigList.txt"))
                text_file = @"Databases\IPDB List.txt";
            else
                text_file = @"Databases\IPDB BigList.txt";
            using (StreamReader sr = new StreamReader(text_file))
            {
                string line = string.Empty;
                string pattern = @"\|\|\|";
                string[] lineArray = System.Text.RegularExpressions.Regex.Split(line, pattern);

                while ((line = sr.ReadLine()) != null)
                {

                    lineArray = System.Text.RegularExpressions.Regex.Split(line, pattern);
                    string s = lineArray[1].Replace("\"", string.Empty);
                    string s1 = s.Replace(":", " ");
                    //master_tables.Add(new Table(s1, lineArray[1], Convert.ToInt32(lineArray[2]), lineArray[3], Convert.ToInt32(lineArray[4])));
                    master_tables.Add(new Table(Convert.ToInt32(lineArray[0]), lineArray[1], lineArray[2], lineArray[3], Convert.ToInt32(lineArray[4]), float.Parse(lineArray[5]),
                        lineArray[6], Convert.ToInt32(lineArray[7]), lineArray[8], Convert.ToInt32(lineArray[9])));
                }

            }

            CollectionViewSource MasterViewSource;
            MasterViewSource = (CollectionViewSource)(FindResource("MasterViewSource"));
            MasterViewSource.Source = master_tables;
            MasterViewSource.Filter += itemCollectionViewSource_Filter;


        }

        private void testMaster_Click(object sender, RoutedEventArgs e)
        {

            foreach (var item in master_tables)
            {
                var selectedTable = tables_grid.SelectedItem as Table;
                string pattern = item.Name;
                pattern.Replace("_", " ");
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                Match m = r.Match(selectedTable.Name);
                if (m.Success)
                {
                    System.Windows.MessageBox.Show(String.Format("Matched Game: {0}", item.Name));
                }

            }

        }

        private void tables_grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void testMaster_Copy_Click(object sender, RoutedEventArgs e)
        {
            tables_grid.Items.Refresh();
        }

        private void tables_grid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var s = sender as System.Windows.Controls.DataGrid;
            Table t = this.tables_grid.SelectedItem as Table;

            var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);

            if (s.CurrentColumn !=null)
                if (s.CurrentColumn.Header.ToString() == "Table File")
                {
                    t.getTableFileName(p.TablePath);
                }
                else if (s.CurrentColumn.Header.ToString() == "Description")
                {
                    var v = tables_grid.Items.GetItemAt(this.tables_grid.SelectedIndex) as Table;
                    List<Table> masters = new List<Table>(master_tables);
                    bool d = t.MatchDescription(t.Description, masters);

                }

        }

        public void buttonAddFile_Click(object sender, RoutedEventArgs e)
        {
            object[] intArray = null;

            if (datagrid_oddtables.SelectedItems.Count != 0)
            {
                int cnt = datagrid_oddtables.SelectedItems.Count;
                int i = 0;
                intArray = new object[cnt];
                foreach (OddTables item in datagrid_oddtables.SelectedItems)
                {
                    string newName = System.IO.Path.GetFileNameWithoutExtension(item.file);
                    var f = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
                    tables.Add(new Table(newName, newName));
                    var g = tables.Last();
                    g.getTableFileName(f.TablePath);
                    g.Enabled = check_game_enabled.IsChecked.Value;
                    g.Hidedmd = check_dmd.IsChecked.Value;
                    g.Hidebackglass = check_bg.IsChecked.Value;
                    g.AlternateExe = combobox_exe.Text;
                    intArray[i] = datagrid_oddtables.SelectedItem;
                    i++;
                }

                foreach (var item in intArray)
                {
                    oddTables.Remove((OddTables)item);
                }
                //oddTables.Remove((OddTables)intArray[0]);
                ScanTablePath(Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex).TablePath);
                populateOddTables();
            }

            this.Title = "-PinX Check-\t" + database_name + "   Tables: " + tables_grid.Items.Count.ToString() + "   System: " + systemName;
        }
        private void button_addgame_Copy1_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_oddtables.Items.Count != 0)
            {
                foreach (OddTables item in datagrid_oddtables.Items)
                {
                    string newName = System.IO.Path.GetFileNameWithoutExtension(item.file);
                    var f = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
                    tables.Add(new Table(newName, newName));
                    var g = tables.Last();
                    g.getTableFileName(f.TablePath);
                    g.Enabled = check_game_enabled.IsChecked.Value;
                    g.Hidedmd = check_dmd.IsChecked.Value;
                    g.Hidebackglass = check_bg.IsChecked.Value;
                }
                tables_grid.Items.Refresh();
            }
            oddTables.Clear();
        }
        private void button_addgame_Copy2_Click(object sender, RoutedEventArgs e)
        {
            var theme = ThemeManager.DetectAppStyle(System.Windows.Application.Current);

            // now set the Green accent and dark theme
            ThemeManager.ChangeAppStyle(System.Windows.Application.Current,
                                        ThemeManager.GetAccent("Blue"),
                                        ThemeManager.GetAppTheme("BaseDark"));
        }
        private void button_addgame_Copy3_Click(object sender, RoutedEventArgs e)
        {
            if (tables_grid.SelectedItems.Count > 0 && datagrid_oddtables.SelectedItems.Count > 0)
            {

                var t = tables_grid.SelectedItem as Table;
                var od = datagrid_oddtables.SelectedItem as OddTables;
                var p = Pinxsystem[comboBox_syslist.SelectedIndex].TablePath;
                t.Name = System.IO.Path.GetFileNameWithoutExtension(od.file);
                t.getTableFileName(p);
                oddTables.Remove(od);
            }


            datagrid_oddtables.Items.Refresh();

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (tables_grid.Items.Count > 0)
            {
                if (radioGroup1.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Enabled = true;
                    }
                }
                else if (radioGroup2.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Hidedmd = true;
                    }
                }
                else if (radioGroup3.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Hidebackglass = true;
                    }
                }


            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (tables_grid.Items.Count > 0)
            {
                if (radioGroup1.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Enabled = false;
                    }
                }
                else if (radioGroup2.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Hidedmd = false;
                    }
                }
                else if (radioGroup3.IsChecked.Value)
                {
                    foreach (var item in tables_grid.SelectedItems)
                    {
                        var t = item as Table;
                        t.Hidebackglass = false;
                    }
                }


            }
        }

        // Author
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            SevennZipVP sz = new SevennZipVP();
            string vpTable;
            string fpTable;
            string p = string.Empty;
            var s = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);

            if (s.SysName == "Visual Pinball" || s.SysName == "Future Pinball")
            {
                foreach (var item in tables_grid.SelectedItems)
                {
                    var itemTable = item as Table;
                    vpTable = itemTable.Name + ".vpt";
                    fpTable = itemTable.Name + ".fpt";
                    if (s.SysName == "Visual Pinball")
                        p = System.IO.Path.Combine(s.TablePath, vpTable);
                    else if (s.SysName == "Future Pinball")
                        p = System.IO.Path.Combine(s.TablePath, fpTable);


                    p = "\"" + p + "\"";
                    sz.GetTableInfo(p, itemTable.Name, s.SysName, "Author");
                    string line = null;
                    itemTable.Author = string.Empty;
                    DirectoryInfo di;// = new DirectoryInfo(@"TableInfo\");
                    FileInfo[] fi;// = di.GetFiles(@"*.*");

                    if (s.SysName == "Visual Pinball")
                    {
                        if (File.Exists("AuthorName"))
                        {
                            System.IO.StreamReader file = new System.IO.StreamReader("AuthorName", Encoding.Unicode);
                            line = file.ReadLine();
                            if (line != null || line != string.Empty)
                            {
                                file.Close();
                                file = new System.IO.StreamReader("AuthorName", Encoding.Unicode);
                                while ((line = file.ReadLine()) != null)
                                {
                                    itemTable.Author = line;
                                }
                                file.Close();
                            }
                        }
                    }
                    else if (s.SysName == "Future Pinball")
                    {
                        if (File.Exists("Table Data"))
                        {
                            System.IO.StreamReader file = new System.IO.StreamReader("Table Data", Encoding.Default);
                            line = file.ReadToEnd();
                            line.ToString();
                            if (line != null || line != string.Empty)
                            {
                                file.Close();
                                file = new System.IO.StreamReader("Table Data");
                                var myRegex = new Regex(@"0����-\\0\\0\\0(\w.*)");
                                MatchCollection AllMatches = myRegex.Matches(line);
                                foreach (Match SomeMatch in AllMatches)
                                {
                                    itemTable.Author = SomeMatch.Groups[1].Value;
                                }

                            }

                        }
                    }

                }
            }
        }

        //Scan for rom
        private void Button_Click_24(object sender, RoutedEventArgs e)
        {
            SevennZipVP sz = new SevennZipVP();
            string vpTable;
            string fpTable;
            string p = string.Empty;
            var s = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);

            if (s.SysName == "Visual Pinball" || s.SysName == "Future Pinball")
            {
                foreach (var item in tables_grid.SelectedItems)
                {
                    var itemTable = item as Table;
                    vpTable = itemTable.Name + ".vpt";
                    fpTable = itemTable.Name + ".fpt";
                    if (s.SysName == "Visual Pinball")
                        p = System.IO.Path.Combine(s.TablePath, vpTable);
                    else if (s.SysName == "Future Pinball")
                        p = System.IO.Path.Combine(s.TablePath, fpTable);


                    p = "\"" + p + "\"";
                    sz.GetTableInfo(p, itemTable.Name, s.SysName, "Rom");
                    string line = null;
                    itemTable.Rom = string.Empty;

                    if (s.SysName == "Visual Pinball")
                    {
                        //cGameName.*= "(\w+)"
                        if (File.Exists("GameData"))
                        {
                            System.IO.StreamReader file = new System.IO.StreamReader("GameData", Encoding.UTF8);
                            line = file.ReadLine();
                            if (line != null || line != string.Empty)
                            {
                                file.Close();
                                file = new System.IO.StreamReader("GameData", Encoding.UTF8);
                                line = file.ReadToEnd();

                                //var myRegex = new Regex(@"Const cGameName.*= *""(\w+)""");
                                var myRegex = new Regex(@"cGameName *= *""(\w+)""");
                                //Const cGameName= *"(\w+)"

                                MatchCollection AllMatches = myRegex.Matches(line);
                                foreach (Match SomeMatch in AllMatches)
                                {
                                    itemTable.Rom = SomeMatch.Groups[1].Value;
                                }

                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"Const RomSet1 = ""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }
                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"Const Romset1.*=.*""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }
                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"Const RS1.*=.*""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }




                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"GameName=""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }
                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"GameName = ""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }
                                if (itemTable.Rom == string.Empty)
                                {
                                    //  myRegex = new Regex(@"GameName.=""(\w+)""");
                                    myRegex = new Regex(@"Const cGameName.*= ""(\w+)""");
                                    AllMatches = myRegex.Matches(line);
                                    foreach (Match SomeMatch in AllMatches)
                                    {
                                        itemTable.Rom = SomeMatch.Groups[1].Value;
                                    }

                                }



                                file.Close();
                            }
                        }
                    }

                }
            }
        }

        // LOad INI
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            Pinxsystem.Clear();
            setSystemsFromINI();
            datagridSettings.ItemsSource = Pinxsystem;
            datagridSettings_Copy.ItemsSource = Pinxsystem;
        }

        // Save INI
        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            SaveSystemsIni();
        }

        private void SaveSystemsIni()
        {
            IniFileReader ini_file = new IniFileReader(ConfigPath);
            //ini_file.IniWriteValue("VisualPinball", "WorkingPath","");
            foreach (var item in Pinxsystem)
            {
                ini_file.IniWriteValue(item.ID, "Enabled", item.Enabled.ToString());
                ini_file.IniWriteValue(item.ID, "WorkingPath", item.WorkingPath);
                ini_file.IniWriteValue(item.ID, "TablePath", item.TablePath);
                ini_file.IniWriteValue(item.ID, "Executable", item.Executable);
                ini_file.IniWriteValue(item.ID, "Parameters", item.Parameters);

                ini_file.IniWriteValue(item.ID, "LaunchBeforeEnabled", item.LaunchBefore.ToString());
                ini_file.IniWriteValue(item.ID, "LaunchBeforeWorkingPath", item.LaunchBeforePath);
                ini_file.IniWriteValue(item.ID, "LaunchBeforeExecutable", item.LaunchBeforeexe);
                ini_file.IniWriteValue(item.ID, "LaunchBeforeParameters", item.LaunchBeforeParams);
                ini_file.IniWriteValue(item.ID, "LaunchBeforeWaitForExit", item.LaunchBeforeWaitForExit.ToString());
                ini_file.IniWriteValue(item.ID, "LaunchBeforeHideWindow", item.LaunchBeforeHideWindow.ToString());

                ini_file.IniWriteValue(item.ID, "LaunchAfterEnabled", item.LaunchAfter.ToString());
                ini_file.IniWriteValue(item.ID, "LaunchAfterWorkingPath", item.LaunchAfterWorkingPath);
                ini_file.IniWriteValue(item.ID, "LaunchAfterExecutable", item.LaunchAfterexe);
                ini_file.IniWriteValue(item.ID, "LaunchAfterParameters", item.LaunchAfterParams);
                ini_file.IniWriteValue(item.ID, "LaunchAfterWaitForExit", item.LaunchAfterWaitForExit.ToString());
                ini_file.IniWriteValue(item.ID, "LaunchAfterHideWindow", item.LaunchAfterHideWindow.ToString());

                if (item.ID == "VisualPinball")
                {
                    ini_file.IniWriteValue(item.ID, "NVRAMPath", item.NVRAMPATH);
                    ini_file.IniWriteValue(item.ID, "Bypass", "True");
                }

                if (item.ID == "FuturePinball")
                {
                    ini_file.IniWriteValue(item.ID, "FPRAMPath", item.NVRAMPATH);
                    ini_file.IniWriteValue(item.ID, "MouseClickFocus", "True");
                }


            }


        }

        private void setSystemsFromINI()
        {

            try
            {
                Pinxsystem = new List<PinXSystem>();
                IniFileReader ini_read = new IniFileReader(ConfigPath);
                string id = "VisualPinball";
                bool enabled;
                bool.TryParse(ini_read.IniReadValue("VisualPinball", "Enabled"), out enabled);
                string sysName = "Visual Pinball";
                string workpath = ini_read.IniReadValue("VisualPinball", "WorkingPath");
                string tablepath = ini_read.IniReadValue("VisualPinball", "TablePath");
                string parameters = ini_read.IniReadValue("VisualPinball", "Parameters");
                string exe = ini_read.IniReadValue("VisualPinball", "Executable");
                string nvram = ini_read.IniReadValue("VisualPinball", "NVRAMPath");
                int systemType;
                bool lbEnabled = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchBeforeEnabled"));
                string lbPath = ini_read.IniReadValue("VisualPinball", "LaunchBeforeWorkingPath");
                string lbExe = ini_read.IniReadValue("VisualPinball", "LaunchBeforeExecutable");
                string lbParams = ini_read.IniReadValue("VisualPinball", "LaunchBeforeParameters");
                bool lbWaitExit = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchBeforeWaitForExit"));
                bool lbHideWindow = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchBeforeHideWindow"));

                bool laEnabled = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchAfterEnabled"));
                string laPath = ini_read.IniReadValue("VisualPinball", "LaunchAfterWorkingPath");
                string laExe = ini_read.IniReadValue("VisualPinball", "LaunchAfterExecutable");
                bool laHideWindow = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchAfterHideWindow"));
                string laParams = ini_read.IniReadValue("VisualPinball", "LaunchAfterParameters");
                bool laWaitExit = Convert.ToBoolean(ini_read.IniReadValue("VisualPinball", "LaunchAfterWaitForExit"));

                Pinxsystem.Add(new PinXSystem(1, enabled, id, sysName, workpath, tablepath, exe, parameters, lbEnabled, lbPath, lbExe, lbParams,
                    lbWaitExit, lbHideWindow, laEnabled, laWaitExit, laExe, laParams, laHideWindow, laPath, nvram));

                id = "FuturePinball";
                bool.TryParse(ini_read.IniReadValue("FuturePinball", "Enabled"), out enabled);
                sysName = "Future Pinball";
                workpath = ini_read.IniReadValue("FuturePinball", "WorkingPath");
                tablepath = ini_read.IniReadValue("FuturePinball", "TablePath");
                parameters = ini_read.IniReadValue("FuturePinball", "Parameters");
                exe = ini_read.IniReadValue("FuturePinball", "Executable");
                nvram = ini_read.IniReadValue("FuturePinball", "NVRAMPath");

                lbEnabled = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchBeforeEnabled"));
                lbPath = ini_read.IniReadValue("FuturePinball", "LaunchBeforeWorkingPath");
                lbExe = ini_read.IniReadValue("FuturePinball", "LaunchBeforeExecutable");
                lbParams = ini_read.IniReadValue("FuturePinball", "LaunchBeforeParameters");
                lbWaitExit = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchBeforeWaitForExit"));
                lbHideWindow = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchBeforeHideWindow"));

                laEnabled = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchAfterEnabled"));
                laPath = ini_read.IniReadValue("FuturePinball", "LaunchAfterWorkingPath");
                laExe = ini_read.IniReadValue("FuturePinball", "LaunchAfterExecutable");
                laHideWindow = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchAfterHideWindow"));
                laParams = ini_read.IniReadValue("FuturePinball", "LaunchAfterParameters");
                laWaitExit = Convert.ToBoolean(ini_read.IniReadValue("FuturePinball", "LaunchAfterWaitForExit"));

                Pinxsystem.Add(new PinXSystem(2, enabled, id, sysName, workpath, tablepath, exe, parameters, lbEnabled, lbPath, lbExe, lbParams,
                    lbWaitExit, lbHideWindow, laEnabled, laWaitExit, laExe, laParams, laHideWindow, laPath, nvram));

                int i = 1;

                while (sysName != string.Empty)
                {
                    id = "System_" + i;
                    sysName = ini_read.IniReadValue(id, "Name");
                    workpath = ini_read.IniReadValue("System_" + i, "WorkingPath");
                    tablepath = ini_read.IniReadValue("System_" + i, "TablePath");
                    parameters = ini_read.IniReadValue("System_" + i, "Parameters");
                    exe = ini_read.IniReadValue("System_" + i, "Executable");
                    nvram = ini_read.IniReadValue("System_" + i, "NVRAMPath");

                    int.TryParse(ini_read.IniReadValue("System_" + i, "SystemType"), out systemType);

                    bool.TryParse(ini_read.IniReadValue("System_" + i, "Enabled"), out enabled);
                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchBeforeEnabled"), out lbEnabled);
                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchBeforeWaitForExit"), out lbWaitExit);
                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchBeforeHideWindow"), out lbHideWindow);

                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchAfterEnabled"), out laEnabled);
                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchAfterHideWindow"), out laHideWindow);
                    bool.TryParse(ini_read.IniReadValue("System_" + i, "LaunchAfterWaitForExit"), out laWaitExit);

                    lbPath = ini_read.IniReadValue("System_" + i, "LaunchBeforeWorkingPath");
                    lbExe = ini_read.IniReadValue("System_" + i, "LaunchBeforeExecutable");
                    lbParams = ini_read.IniReadValue("System_" + i, "LaunchBeforeParameters");
                    laPath = ini_read.IniReadValue("System_" + i, "LaunchAfterWorkingPath");
                    laExe = ini_read.IniReadValue("System_" + i, "LaunchAfterExecutable");
                    laParams = ini_read.IniReadValue("System_" + i, "LaunchAfterParameters");

                    if (sysName != string.Empty)
                    {
                        Pinxsystem.Add(new PinXSystem(systemType, enabled, id, sysName, workpath, tablepath, exe, parameters, lbEnabled, lbPath, lbExe, lbParams,
                            lbWaitExit, lbHideWindow, laEnabled, laWaitExit, laExe, laParams, laHideWindow, laPath, nvram));
                        i++;
                    }
                    else
                        return;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
          

        }

        private void butt_WorkingPath_Click(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.WorkingPath);
        }
        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.TablePath);
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.Executable);
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings_Copy.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.LBPath);
        }

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings_Copy.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.LBExe);
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings_Copy.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.LAPath);
        }

        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            var tg = datagridSettings_Copy.SelectedItem as PinXSystem;
            OpenFileDialogSettings(tg, PinXSettings.LAExe);
        }


        private void OpenFileDialogSettings(PinXSystem pinxSys, PinXSettings pi)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            OpenFileDialog fDialog = new OpenFileDialog();

            System.Windows.Forms.DialogResult result;

            if (pi == PinXSettings.WorkingPath)
            {
                dialog.SelectedPath = pinxSys.WorkingPath;
                result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.WorkingPath = dialog.SelectedPath;
            }
            else if (pi == PinXSettings.TablePath)
            {
                dialog.SelectedPath = pinxSys.TablePath;
                result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.TablePath = dialog.SelectedPath;
            }
            else if (pi == PinXSettings.LBPath)
            {
                dialog.SelectedPath = pinxSys.LaunchBeforePath;
                result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.LaunchBeforePath = dialog.SelectedPath;
            }
            else if (pi == PinXSettings.LAPath)
            {
                dialog.SelectedPath = pinxSys.LaunchAfterWorkingPath;
                result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.LaunchAfterWorkingPath = dialog.SelectedPath;
            }
            else if (pi == PinXSettings.NVRam)
            {
                dialog.SelectedPath = pinxSys.NVRAMPATH;
                result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.NVRAMPATH = dialog.SelectedPath;
            }
            else if (pi == PinXSettings.Executable)
            {
                fDialog.InitialDirectory = pinxSys.WorkingPath;
                result = fDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.Executable = System.IO.Path.GetFileName(fDialog.FileName);
            }
            else if (pi == PinXSettings.LBExe)
            {
                fDialog.InitialDirectory = pinxSys.LaunchBeforeexe;
                result = fDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.LaunchBeforeexe = System.IO.Path.GetFileName(fDialog.FileName);
            }
            else if (pi == PinXSettings.LAExe)
            {
                fDialog.InitialDirectory = pinxSys.LaunchAfterexe;
                result = fDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    pinxSys.LaunchAfterexe = System.IO.Path.GetFileName(fDialog.FileName);
            }


                datagridSettings.Items.Refresh();
                datagridSettings_Copy.Items.Refresh();
        }

        public enum PinXSettings
        {
            WorkingPath,
            TablePath,
            Executable,
            NVRam,
            LBPath,
            LBExe,
            LAPath,
            LAExe,

        }

        private void dataGridMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (tables_grid.ItemsSource != null)
            {
                System.Windows.Controls.CheckBox c = (System.Windows.Controls.CheckBox)sender;
                bool filter = c.IsChecked.Value;
                ICollectionView cv = CollectionViewSource.GetDefaultView(tables_grid.ItemsSource);
                if (!filter)
                    cv.Filter = null;
                else
                {
                    cv.Filter = o =>
                    {
                        Table h = o as Table;
                        return (!h.MatchedDescription);
                    };
                }
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tables_grid.ItemsSource != null)
            {
                System.Windows.Controls.CheckBox c = (System.Windows.Controls.CheckBox)sender;
                bool filter = c.IsChecked.Value;
                ICollectionView cv = CollectionViewSource.GetDefaultView(tables_grid.ItemsSource);
                if (!filter)
                    cv.Filter = null;
                else
                {
                    cv.Filter = o =>
                    {
                        Table h = o as Table;
                        return (!h.MatchedDescription);
                    };
                }
            }
        }

        //Stop Match Button
        private void Button_Click_19(object sender, RoutedEventArgs e)
        {
            if (bw.IsBusy)
                bw.CancelAsync();
        }

        public class OddTables
        {
            public OddTables(string file, DateTime dt)
            {
                this.date = dt;
                this.file = file;
                //MessageBox.Show(date.ToString());

            }
            public DateTime date { get; set; }
            public string file { get; set; }
        }

        public class Distance
        {

            /// <summary>

            /// Compute Levenshtein distance

            /// </summary>

            /// <param name="s">String 1</param>

            /// <param name="t">String 2</param>

            /// <returns>Distance between the two strings.

            /// The larger the number, the bigger the difference.

            /// </returns>

            public int LD(string s, string t)
            {

                int n = s.Length; //length of s

                int m = t.Length; //length of t

                int[,] d = new int[n + 1, m + 1]; // matrix

                int cost; // cost

                // Step 1

                if (n == 0) return m;

                if (m == 0) return n;

                // Step 2

                for (int i = 0; i <= n; d[i, 0] = i++) ;

                for (int j = 0; j <= m; d[0, j] = j++) ;

                // Step 3

                for (int i = 1; i <= n; i++)
                {

                    //Step 4

                    for (int j = 1; j <= m; j++)
                    {

                        // Step 5

                        cost = (t.Substring(j - 1, 1) == s.Substring(i - 1, 1) ? 0 : 1);

                        // Step 6

                        d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                                  d[i - 1, j - 1] + cost);

                    }

                }


                // Step 7


                return d[n, m];

            }

        }

        // Description Listview LV Window X
        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            closeDescSearch.Visibility = System.Windows.Visibility.Visible;
            lv.Visibility = System.Windows.Visibility.Visible;
        }
        private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.MessageBox.Show("");
        }
        private void closeDescSearch_Click(object sender, RoutedEventArgs e)
        {
            lv.Visibility = System.Windows.Visibility.Collapsed;
            closeDescSearch.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void lv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var t = lv.SelectedItem as Table;
            var r = tables_grid.Items.GetItemAt(this.tables_grid.SelectedIndex) as Table;

            r.Year = t.Year;
            r.Type = t.Type;
            r.Manufacturer = t.Manufacturer;
            r.Description = t.Description;
            //r.Rating = r.convertRatings(t.Rating);

            lv.Visibility = System.Windows.Visibility.Hidden;
            closeDescSearch.Visibility = System.Windows.Visibility.Collapsed;
            List<Table> masters = new List<Table>(master_tables);
            bool d = r.MatchDescription(t.Description, masters);

        }




        public bool IsProcessOpen(string name)
        {

            foreach (Process clsProcess in Process.GetProcesses())
            {

                if (clsProcess.ProcessName.Contains(name))
                {
                    clsProcess2 = clsProcess;
                    return true;
                }
            }

            return false;
        }

        // restart PinballX
        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            if (IsProcessOpen("PinballX"))
            {
                clsProcess2.Kill();
            }
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            if (!IsProcessOpen("PinballX"))
            {
                Process clsProcess2 = new Process();
                clsProcess2.StartInfo.FileName = "PinballX.exe";
                clsProcess2.Start();
            }
        }

        private void buttonRename_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in tables)
            {
                if (item.FlagRename)
                {
                    if (item.MatchedName != "")
                    {
                        item.Description = item.MatchedName;
                        item.MatchedDescription = true;
                        item.Year = item.MatchedYear;
                        item.Manufacturer = item.MatchedManu;
                        item.Rating = item.MatchedRating;
                        item.Type = item.MatchedType;
                        //item.MatchedName = string.Empty;
                        item.FlagRename = false;
                    }
                }
                else
                {
                    item.MatchedName = string.Empty;
                }
            }

            buttonRename.IsEnabled = false;
            filterCheckbox.IsChecked = false;
        }

        // Get ratings from Master IPDB
        private void Button_Click_23(object sender, RoutedEventArgs e)
        {
            foreach (var item in tables_grid.SelectedItems)
            {
                if (item != null)
                {
                    var t = item as Table;
                    List<Table> m_tables = new List<Table>(master_tables);
                    var value = m_tables.Find(itemFind => itemFind.Description.ToUpper() == t.Description.ToUpper());
                    if (value != null)
                        t.Rating = value.Rating;
                }
            }


        }

        private void radioGroup3_Checked(object sender, RoutedEventArgs e)
        {

        }

        //Scan Media Button
        private void scanMedia_Click(object sender, RoutedEventArgs e)
        {

            //string filter = string.Empty;
            //if (Path.Contains("Wheel"))
            //    filter = "*.png";

            //di = new DirectoryInfo(Path);
            //fi = di.GetFiles(filter);
            //foreach (var item in fi)
            //{
            //    List<string> FilesList = new List<string>();
            //}

            try
            {
                string filter = string.Empty;
                DirectoryInfo di = new DirectoryInfo("Media\\" + systemName + "\\Wheel Images");
                if (di.FullName.Contains("Wheel"))
                    filter = "*.png";

                FileInfo[] fi = di.GetFiles(filter);
                listWheels = new List<MediaFiles>();

                List<Table> tab = new List<Table>(tables);
                foreach (FileInfo file in fi)
                {
                    bool exist = tab.Exists(x => x.Description == System.IO.Path.GetFileNameWithoutExtension(file.Name));
                    if (!exist)
                    {
                        listWheels.Add(new MediaFiles(file, "", false, "Wheels"));
                    }
                    else
                    { }

                }

                CollectionViewSource ItemCollectionViewSourceWheelFiles;
                ItemCollectionViewSourceWheelFiles = (CollectionViewSource)(FindResource("ItemCollectionViewSourceWheelFiles"));
                ItemCollectionViewSourceWheelFiles.Source = listWheels;

                di = new DirectoryInfo("Media\\" + systemName + "\\Table Images");
                fi = di.GetFiles("*.png");
                listTImgs = new List<MediaFiles>();
                foreach (var file in fi)
                {
                    bool exist = tab.Exists(x => x.Description == System.IO.Path.GetFileNameWithoutExtension(file.Name));
                    if (!exist)
                    {
                        listTImgs.Add(new MediaFiles(file, "", false, "Table Image"));
                    }
                    else
                    { }

                }

                di = new DirectoryInfo("Media\\" + systemName + "\\Table Videos");
                fi = di.GetFiles("*.mp4");
                listTVids = new List<MediaFiles>();
                foreach (var file in fi)
                {
                    bool exist = tab.Exists(x => x.Description == System.IO.Path.GetFileNameWithoutExtension(file.Name));
                    if (!exist)
                    {
                        listTImgs.Add(new MediaFiles(file, "", false, "Table Vids"));
                    }
                    else
                    { }

                }

                if (DataGridMedia.Items.Count > 0)
                {
                    foreach (var item in DataGridMedia.Items)
                    {
                        var t = item as Table;

                        if (File.Exists("Media\\" + systemName + "\\Wheel Images\\" + t.Description + ".png"))
                        {
                            t.HaveWheels = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Backglass Images\\" + t.Description + ".png"))
                        {
                            t.HaveBGImage = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Backglass Videos\\" + t.Description + ".f4v"))
                        {
                            t.HaveBGVids = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Backglass Videos\\" + t.Description + ".mp4"))
                        {
                            t.HaveBGVids = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\DMD Images\\" + t.Description + ".png"))
                        {
                            t.HaveDmdImg = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\DMD Videos\\" + t.Description + ".f4v"))
                        {
                            t.HaveDmdVids = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\DMD Videos\\" + t.Description + ".mp4"))
                        {
                            t.HaveDmdVids = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Real DMD Images\\" + t.Description + ".png"))
                        {
                            t.HaveRealDmdImg = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Real DMD Videos\\" + t.Description + ".f4v"))
                        {
                            t.HaveRealDmdVids = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Real DMD Videos\\" + t.Description + ".mp4"))
                        {
                            t.HaveRealDmdVids = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Audio\\" + t.Description + ".mp3"))
                        {
                            t.HaveTableAudio = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Images\\" + t.Description + ".png"))
                        {
                            t.HaveTableImage = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Videos\\" + t.Description + ".f4v"))
                        {
                            t.HaveTableVideo = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Videos\\" + t.Description + ".mp4"))
                        {
                            t.HaveTableVideo = true;
                        }

                        if (File.Exists("Media\\" + systemName + "\\Table Images Desktop\\" + t.Description + ".png"))
                        {
                            t.HaveTableImageDT = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Videos Desktop\\" + t.Description + ".f4v"))
                        {
                            t.HaveTableVideoDT = true;
                        }
                        if (File.Exists("Media\\" + systemName + "\\Table Videos Desktop\\" + t.Description + ".mp4"))
                        {
                            t.HaveTableVideoDT = true;
                        }
                    }

                }
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        private void DataGridMedia_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {



        }

        private void DataGridMedia_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataGridMedia.SelectedItems.Count > 0)
            {
                var t = DataGridMedia.SelectedValue as Table;
                var dir = Directory.GetCurrentDirectory();
                Uri uriSource;

                try
                {
                    if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Wheels")
                    {
                        MediaType = "Wheels";
                        if (t.HaveWheels)
                        {
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Wheel Images\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);

                            CollectionViewSource ItemCollectionViewSourceWheelFiles;
                            ItemCollectionViewSourceWheelFiles = (CollectionViewSource)(FindResource("ItemCollectionViewSourceWheelFiles"));
                            ItemCollectionViewSourceWheelFiles.Source = listWheels;
                        }
                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Table Image")
                    {
                        MediaType = "Table Image";
                        if (t.HaveTableImage)
                        {
                            
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Table Images\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);

                            CollectionViewSource ItemCollectionViewSourceWheelFiles;
                            ItemCollectionViewSourceWheelFiles = (CollectionViewSource)(FindResource("ItemCollectionViewSourceWheelFiles"));
                            ItemCollectionViewSourceWheelFiles.Source = listTImgs;
                            
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Table Vids")
                    {
                        MediaType = "Table Vids";
                        if (t.HaveTableVideo)
                        {
                            string path = dir + "\\Media\\" + systemName + "\\Table Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\" + systemName + "\\Table Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);
                            image_wheel.Source = null;
                            media_view.Source = uriSource;

                            CollectionViewSource ItemCollectionViewSourceWheelFiles;
                            ItemCollectionViewSourceWheelFiles = (CollectionViewSource)(FindResource("ItemCollectionViewSourceWheelFiles"));
                            ItemCollectionViewSourceWheelFiles.Source = listTVids;
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "BG Image")
                    {
                        if (t.HaveBGImage)
                        {
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Backglass Images\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "BG Vids")
                    {
                        if (t.HaveBGVids)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\" + systemName + "\\Backglass Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\" + systemName + "\\Backglass Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "DMD Image")
                    {
                        if (t.HaveDmdImg)
                        {
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\DMD Images\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "DMD Vids")
                    {
                        if (t.HaveDmdVids)
                        {
                            string path = dir + "\\Media\\" + systemName + "\\DMD Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\" + systemName + "\\DMD Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);
                            image_wheel.Source = null;
                            media_view.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "RealDMD Image")
                    {
                        if (t.HaveRealDmdImg)
                        {
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Real DMD Images\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "RealDMD Vids")
                    {
                        if (t.HaveRealDmdVids)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\" + systemName + "\\Real DMD Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\" + systemName + "\\Real DMD Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Table Audio")
                    {
                        if (t.HaveTableAudio)
                        {
                            image_wheel.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Table Audio\\" + t.Description + ".mp3");
                            media_view.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Table Image DT")
                    {
                        if (t.HaveTableImageDT)
                        {
                            media_view.Source = null;
                            uriSource = new Uri(dir + "\\Media\\" + systemName + "\\Table Images Desktop\\" + t.Description + ".png");
                            image_wheel.Source = new BitmapImage(uriSource);
                        }

                    }
                    else if ((string)DataGridMedia.CurrentColumn.Header.ToString() == "Table Vids DT")
                    {
                        if (t.HaveTableVideoDT)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\" + systemName + "\\Table Videos Desktop\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\" + systemName + "\\Table Videos Desktop\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view.Source = uriSource;
                        }

                    }
                }
                catch (Exception)
                {


                }

            }
        }

        private void datagrid_oddwheels_MouseUp(object sender, MouseButtonEventArgs e)
        {


        }


        private void datagrid_oddwheels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var m = datagrid_oddwheels.SelectedItem as MediaFiles;

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                Uri imageSource = new Uri("file://" + m.File.FullName);
                image.UriSource = imageSource;
                image.EndInit();
                image_wheel.Source = image;
            }
            catch (Exception)
            {
                
            }

        }

        private void DataGridFlyers_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataGridFlyers.SelectedItems.Count > 0)
            {
                var t = DataGridMedia.SelectedValue as Table;
                var dir = Directory.GetCurrentDirectory();
                Uri uriSource;

                if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Flyer Back")
                {
                    if (t.HaveFlyerBack)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Back\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Flyer Front")
                {
                    if (t.HaveFlyerFront)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Front\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside1")
                {
                    if (t.HaveInside1)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside1\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside2")
                {
                    if (t.HaveInside2)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside2\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside3")
                {
                    if (t.HaveInside3)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside3\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside4")
                {
                    if (t.HaveInside4)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside4\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside5")
                {
                    if (t.HaveInside5)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside5\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside6")
                {
                    if (t.HaveInside6)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside6\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside7")
                {
                    if (t.HaveInside7)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside7\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }
                else if ((string)DataGridFlyers.CurrentColumn.Header.ToString() == "Inside8")
                {
                    if (t.HaveInside8)
                    {
                        media_view.Source = null;
                        uriSource = new Uri(dir + "\\Media\\Flyer Images\\Inside8\\" + t.Description + ".jpg");
                        image_flyers.Source = new BitmapImage(uriSource);
                    }
                }


            }
        }

        private void DataGridVideos_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataGridVideos.SelectedItems.Count > 0)
                {
                    var t = DataGridVideos.SelectedValue as Table;
                    var dir = Directory.GetCurrentDirectory();
                    Uri uriSource;

                    if ((string)DataGridVideos.CurrentColumn.Header.ToString() == "Gameplay")
                    {
                        if (t.HaveGameplayVid)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\Gameplay Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\Gameplay Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view2.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridVideos.CurrentColumn.Header.ToString() == "Tutorial")
                    {
                        if (t.HaveTutVid)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\Tutorial Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\Tutorial Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view2.Source = uriSource;
                        }

                    }
                    else if ((string)DataGridVideos.CurrentColumn.Header.ToString() == "Promotional")
                    {
                        if (t.HavePromoVid)
                        {
                            image_wheel.Source = null;
                            string path = dir + "\\Media\\Promotional Videos\\" + t.Description + ".f4v";
                            string path2 = dir + "\\Media\\Promotional Videos\\" + t.Description + ".mp4";

                            if (File.Exists(path))
                                uriSource = new Uri(path);
                            else
                                uriSource = new Uri(path2);

                            media_view2.Source = uriSource;
                        }

                    }
                }
            }
            catch (Exception)
            {


            }

        }

        private void DataGridInstructions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataGridInstructions.SelectedItems.Count > 0)
                {
                    var t = DataGridInstructions.SelectedValue as Table;
                    var dir = Directory.GetCurrentDirectory();
                    Uri uriSource;

                    //image_instructions
                    if ((string)DataGridInstructions.CurrentColumn.Header.ToString() == "Card")
                    {
                        if (t.HaveInstruction)
                        {
                            media_view.Source = null;
                            string path = dir + "/Media/Instruction Cards/" + t.Description + ".swf";
                            path = path.Replace(":\\", "$/");
                            //System.Windows.MessageBox.Show("file://127.0.0.1/" + path);
                            //file://127.0.0.1/i$/PinballX/Instruction Cards/50-50 (Bally 1965).swf
                            //I:\PinballX\Media\Instruction Cards
                            uriSource = new Uri("file://127.0.0.1/" + path);
                            WebBrowserCards.Source = uriSource;
                            //image_instructions.Source = new BitmapImage(uriSource);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        private void DataGridMedia_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataGridMedia_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {


        }

        private void DataGridMedia_CurrentCellChanged(object sender, EventArgs e)
        {

        }

        private void scanFlyers_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridFlyers.Items.Count > 0)
            {
                foreach (var item in DataGridFlyers.Items)
                {
                    var t = item as Table;

                    if (File.Exists("Media\\Flyer Images\\Back\\" + t.Description + ".jpg"))
                    {
                        t.HaveFlyerBack = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Front\\" + t.Description + ".jpg"))
                    {
                        t.HaveFlyerFront = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside1\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside1 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside2\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside2 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside3\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside3 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside4\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside4 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside5\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside5 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside6\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside6 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside7\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside7 = true;
                    }
                    if (File.Exists("Media\\Flyer Images\\Inside8\\" + t.Description + ".jpg"))
                    {
                        t.HaveInside8 = true;
                    }
                }

            }
        }

        private void scanCards_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridInstructions.Items.Count > 0)
            {
                foreach (var item in DataGridInstructions.Items)
                {
                    var t = item as Table;

                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + ".swf"))
                    {
                        t.HaveInstruction = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 1.swf"))
                    {
                        t.HaveInstruction1 = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 2.swf"))
                    {
                        t.HaveInstruction2 = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 3.swf"))
                    {
                        t.HaveInstruction3 = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 4.swf"))
                    {
                        t.HaveInstruction4 = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 5.swf"))
                    {
                        t.HaveInstruction5 = true;
                    }
                    if (File.Exists("Media\\Instruction Cards\\" + t.Description + " 6.swf"))
                    {
                        t.HaveInstruction6 = true;
                    }

                }

            }
        }

        private void scanVideos_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridVideos.Items.Count > 0)
            {
                foreach (var item in DataGridVideos.Items)
                {
                    var t = item as Table;

                    if (File.Exists("Media\\Gameplay Videos\\" + t.Description + ".mp4"))
                    {
                        t.HaveGameplayVid = true;
                    }
                    if (File.Exists("Media\\Tutorial Videos\\" + t.Description + ".mp4"))
                    {
                        t.HaveTutVid = true;
                    }
                    if (File.Exists("Media\\Promotional Videos\\" + t.Description + ".mp4"))
                    {
                        t.HavePromoVid = true;
                    }

                }

            }
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (tables_grid.ItemsSource != null)
            {
                System.Windows.Controls.CheckBox c = (System.Windows.Controls.CheckBox)sender;
                bool filter = c.IsChecked.Value;
                ICollectionView cv = CollectionViewSource.GetDefaultView(tables_grid.ItemsSource);
                if (!filter)
                    cv.Filter = null;
                else
                {
                    cv.Filter = o =>
                    {
                        Table h = o as Table;
                        return (!h.HaveDirectB2S);
                    };
                }
            }


        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (tables_grid.ItemsSource != null)
            {
                System.Windows.Controls.CheckBox c = (System.Windows.Controls.CheckBox)sender;
                bool filter = c.IsChecked.Value;
                ICollectionView cv = CollectionViewSource.GetDefaultView(tables_grid.ItemsSource);
                if (!filter)
                    cv.Filter = null;
                else
                {
                    cv.Filter = o =>
                    {
                        Table h = o as Table;
                        return (!h.MatchedDescription);
                    };
                }
            }
        }

        private void scanDirectB2s_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridB2s.Items.Count > 0)
            {
                var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
                try
                {
                    foreach (Table item in DataGridB2s.Items)
                    {
                        if (File.Exists(p.TablePath + "\\" + item.Description + ".directb2s"))
                            item.HaveDirectB2S = true;
                    }

                }
                catch (Exception)
                {

                }
            }
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            List<string> li = new List<string>();
            var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
            DirectoryInfo di = new DirectoryInfo(p.TablePath + "\\");
            FileInfo[] fi = di.GetFiles("*.directb2s");

            foreach (var table in tables)
            {
                int i = 0;
                int ii = 15;
                string bestMatch = string.Empty;

                if (!table.HaveDirectB2S)
                {
                    foreach (var item in fi)
                    {
                        string pattern = item.Name;
                        string input;
                        input = table.Description;

                        Distance d = new Distance();
                        i = d.LD(input, pattern);

                        if (i <= ii)
                        {
                            ii = i;
                            table.FlagRenameB2S = true;
                            table.MatchedPercent = i;
                            bestMatch = item.Name;
                        }

                    }

                    li.Add(bestMatch);
                    bestMatch = string.Empty;

                }


            }

            int iii = 0;
            foreach (var item in (li))
            {
                tables.ElementAt(iii).MatchedB2SName = item;
                iii++;

            }

        }

        private void Button_Click_25(object sender, RoutedEventArgs e)
        {
            var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
            foreach (Table item in tables)
            {
                if (item.FlagRenameB2S)
                {
                    if (!File.Exists(p.TablePath + "\\" + item.Description + ".directb2s"))
                    {
                        File.Move(p.TablePath + "\\" + item.MatchedB2SName, p.TablePath + "\\" + item.Description + ".directb2s");
                        item.HaveDirectB2S = true;
                        item.MatchedB2SName = string.Empty;
                    }
                }

            }
        }

        private void Button_Click_26(object sender, RoutedEventArgs e)
        {
            foreach (Table item in datagridB2sRename.SelectedItems)
            {
                item.FlagRenameB2S = false;
            }
        }

        private void Tab_Changed(object sender, SelectionChangedEventArgs e)
        {
            //FilterMissB2s.IsChecked = false;
        }

        private void Button_Click_27(object sender, RoutedEventArgs e)
        {
            lvb2s.Items.Clear();
            var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);
            DirectoryInfo di = new DirectoryInfo(p.TablePath + "\\");
            FileInfo[] fi = di.GetFiles("*.directb2s");

            foreach (FileInfo item in fi)
            {
                lvb2s.Items.Add(item);
            }


            lvb2s.Visibility = System.Windows.Visibility.Visible;
            closeB2SList.Visibility = System.Windows.Visibility.Visible;

        }

        private void Button_Click_28(object sender, RoutedEventArgs e)
        {
            closeB2SList.Visibility = System.Windows.Visibility.Collapsed;
            lvb2s.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void lvb2s_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var t = DataGridB2s.SelectedItem as Table;
            var p = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);

            if (!File.Exists(p.TablePath + "\\" + t.Description + ".directb2s"))
            {
                File.Move(p.TablePath + "\\" + lvb2s.SelectedItem.ToString(), p.TablePath + "\\" + t.Description + ".directb2s");
                t.HaveDirectB2S = true;
            }
            closeB2SList.Visibility = System.Windows.Visibility.Collapsed;
            lvb2s.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tables_grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseButtonState.Released == e.RightButton)
            {
                LEditor.Visibility = System.Windows.Visibility.Collapsed;
                CamMode.Visibility = System.Windows.Visibility.Collapsed;
                LoadCam.Visibility = System.Windows.Visibility.Collapsed;
                CamSetup.Visibility = System.Windows.Visibility.Collapsed;
                LScript.Visibility = System.Windows.Visibility.Collapsed;
                LBScript.Visibility = System.Windows.Visibility.Collapsed;
                LFScript.Visibility = System.Windows.Visibility.Collapsed;

                tables_grid.ContextMenu.Visibility = System.Windows.Visibility.Visible;
                if (systemName == "Visual Pinball")
                {
                    LEditor.Visibility = System.Windows.Visibility.Visible;
                    CamMode.Visibility = System.Windows.Visibility.Visible;
                    LoadCam.Visibility = System.Windows.Visibility.Visible;
                    CamSetup.Visibility = System.Windows.Visibility.Visible;
                    LScript.Visibility = System.Windows.Visibility.Visible;
                    LBScript.Visibility = System.Windows.Visibility.Visible;
                    LFScript.Visibility = System.Windows.Visibility.Visible;
                }
                else if (systemName == "Future Pinball")
                {
                    LEditor.Visibility = System.Windows.Visibility.Visible;
                    LScript.Visibility = System.Windows.Visibility.Visible;
                }
                else if (systemName == "P-ROC")
                {
                    LEditor.Visibility = System.Windows.Visibility.Visible;
                    CamMode.Visibility = System.Windows.Visibility.Visible;
                    LoadCam.Visibility = System.Windows.Visibility.Visible;
                    CamSetup.Visibility = System.Windows.Visibility.Visible;
                    LScript.Visibility = System.Windows.Visibility.Visible;
                    LBScript.Visibility = System.Windows.Visibility.Visible;
                    LFScript.Visibility = System.Windows.Visibility.Visible;
                }



            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var t = tables_grid.SelectedItem as Table;
            var s = Pinxsystem.ElementAt(comboBox_syslist.SelectedIndex);

            if (!t.TableFileExists)
                return;

            string ext = string.Empty;
            string ext2 = string.Empty;
            if (systemName == "P-ROC" || systemName == "Visual Pinball")
            {
                ext = ".vpt";
                ext2 = ".vpx";
            }
            else if (systemName == "Future Pinball")
            { }
            else
                return;

            IniFileReader ini_read = new IniFileReader(ConfigPath);
            var mi = e.Source as System.Windows.Controls.MenuItem;
            string menuItemHeader = mi.Header.ToString();
            int keyc = Convert.ToInt32(ini_read.IniReadValue("KeyCodes", "exitemulator"));
            
            //System.Windows.MessageBox.Show(keyc.ToString());
            bool UsingVPX = false;
            
            DirectoryInfo di = new DirectoryInfo(s.TablePath);                                 
            VPLaunch vp = new VPLaunch();
            FileInfo[] fi=null;
            if (systemName == "P-ROC" || systemName == "Visual Pinball")
            {
                fi = di.GetFiles(t.Name + ext2);
                if (fi.Length == 0)
                    fi = di.GetFiles(t.Name + ext);
                else
                    UsingVPX = true;
            }
            else if (systemName == "Future Pinball")
            {
                fi = di.GetFiles(t.Name + ".fpt");
            }

            if (systemName == "P-ROC")
            {
                if (menuItemHeader == "Load Camera")
                {
                    if (UsingVPX)
                        vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 1);
                }
                else if (menuItemHeader == "Cam Setup")
                {
                    if (UsingVPX)
                        vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 2);
                }
                else if (menuItemHeader == "Launch editor")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop,3);
                else if (menuItemHeader == "Launch to script")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop,4);
                else if (menuItemHeader == "Play Bsc Script")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Basic", t.Desktop,5);
                else if (menuItemHeader == "Edit Bsc Script")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Basic", t.Desktop);
                else if (menuItemHeader == "Play Full Script")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Full", t.Desktop,5);
                else if (menuItemHeader == "Edit Full Script")
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Full", t.Desktop);
                else
                    vp.launchPROC("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop);
            }
            else if (systemName == "Visual Pinball")
            {
                if (menuItemHeader == "Load Camera")
                {
                    if (UsingVPX)
                        vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 1);
                }
                else if (menuItemHeader == "Cam Setup")
                {
                    if (UsingVPX)
                        vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 2);
                }
                else if (menuItemHeader == "Launch editor")
                {
                   // System.Windows.MessageBox.Show("\"" + s.TablePath + "\"" + "\"" + fi[0].Name + "\"" + t.AlternateExe + "\"" + s.WorkingPath + "\"" + s.Executable + keyc + systemName + " none" + t.Desktop + " 3");
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 3);
                }
                else if (menuItemHeader == "Launch to script")
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 4);
                else if (menuItemHeader == "Play Bsc Script")
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Basic", t.Desktop, 5);
                else if (menuItemHeader == "Edit Bsc Script")
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Basic", t.Desktop);
                else if (menuItemHeader == "Play Full Script")
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Full", t.Desktop, 5);
                else if (menuItemHeader == "Edit Full Script")
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "Full", t.Desktop);
                else
                    vp.launchVP("\"" + s.TablePath + "\"", "\"" + fi[0].Name + "\"", t.AlternateExe, "\"" + s.WorkingPath + "\"", s.Executable, keyc, systemName, "none", t.Desktop, 0);
                       
            }
            else if (systemName == "Future Pinball")
            {
                try
                {
                    if (t.AlternateExe == "")
                        t.AlternateExe = "Future Pinball.exe";
                    vp.lanuchBAM(s.TablePath, fi[0].Name, t.AlternateExe, s.WorkingPath, s.Executable, keyc, systemName, "none", t.Desktop, 0, s.Executable); 
                }
                catch (Exception)
                {
                    
                }
                //string PinballXExe = Convert.ToInt32(ini_read.IniReadValue("FuturePinball", "exitemulator"));
               
            }
            
        }

        private void MatchMedia_Click(object sender, RoutedEventArgs e)
        {
            string bestMatch;
            //var f = item as FileInfo;

            #region Wheels           
            if (MediaType == "Wheels")
            {
                foreach (Table table in DataGridMedia.Items)
                {
                    if (!table.HaveWheels)
                    {
                        foreach (var item in listWheels)
                        {
                            bestMatch = string.Empty;
                            int i = 0;
                            int ii = Convert.ToInt32(_wheels_matchValue.Text);
                            string pattern = System.IO.Path.GetFileNameWithoutExtension(item.File.Name).ToUpper();
                            string input = table.Description.ToUpper();

                            Distance d = new Distance();
                            i = d.LD(input, pattern);
                            if (i <= ii)
                            {
                                ii = i;
                                bestMatch = table.Description;
                                item.TagRename = true;
                                item.MatchedName = bestMatch;
                            }
                        }
                    }

                }
            }
            #endregion
            else if (MediaType == "Table Image")
            {
                foreach (Table table in DataGridMedia.Items)
                {
                    if (!table.HaveTableImage)
                    {
                        foreach (var item in listTImgs)
                        {
                            bestMatch = string.Empty;
                            int i = 0;
                            int ii = Convert.ToInt32(_wheels_matchValue.Text);
                            string pattern = System.IO.Path.GetFileNameWithoutExtension(item.File.Name).ToUpper();
                            string input = table.Description.ToUpper();

                            Distance d = new Distance();
                            i = d.LD(input, pattern);
                            if (i <= ii)
                            {
                                ii = i;
                                bestMatch = table.Description;
                                item.TagRename = true;
                                item.MatchedName = bestMatch;
                            }
                        }
                    }

                }
            }
            else if (MediaType == "Table Vids")
            {
                foreach (Table table in DataGridMedia.Items)
                {
                    if (!table.HaveTableVideo)
                    {
                        foreach (var item in listTVids)
                        {
                            bestMatch = string.Empty;
                            int i = 0;
                            int ii = Convert.ToInt32(_wheels_matchValue.Text);
                            string pattern = System.IO.Path.GetFileNameWithoutExtension(item.File.Name).ToUpper();
                            string input = table.Description.ToUpper();

                            Distance d = new Distance();
                            i = d.LD(input, pattern);
                            if (i <= ii)
                            {
                                ii = i;
                                bestMatch = table.Description;
                                item.TagRename = true;
                                item.MatchedName = bestMatch;
                            }
                        }
                    }

                }
            }
            

        }

        //Delete Media Item
        private void Button_Click_29(object sender, RoutedEventArgs e)
        {





           // var m = s.SelectedItem as MediaFiles;

           // m.File.Delete();
           // s.Items.Remove(m);
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //MediaFiles table_row = datagrid_oddwheels.SelectedItem as MediaFiles;



            //try
            //{
            //    File.Delete(table_row.File.FullName);
            //}
            //catch (Exception)
            //{

            //}
        }

        private void Button_Click_30(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteWheel_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteWheel_Click(object sender, RoutedEventArgs e)
        {

            var p = System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"Media\Images\No DMD.png");
            var m = datagrid_oddwheels.SelectedItem as MediaFiles;
            MediaFiles table_row = datagrid_oddwheels.SelectedItem as MediaFiles;
            string name = table_row.File.FullName;
            datagrid_oddwheels.Items.Remove(m);


            //File.Delete(name);
        }

        private void MatchMediaRename_Click(object sender, RoutedEventArgs e)
        {
            image_wheel.Source = null;
            List<MediaFiles> MediaFilesList =null;

            if (MediaType == "Wheels")
                MediaFilesList = listWheels;
            else if (MediaType == "Table Image")
                MediaFilesList = listTImgs;
            else if (MediaType == "Table Vids")
                MediaFilesList = listTVids;
            foreach (MediaFiles item in MediaFilesList)
            {
                if (item.TagRename)
                {
                    try
                    {
                        if (MediaType=="Wheels")
                            File.Move(item.File.FullName, @"Media\" + systemName + "\\Wheel Images\\" + item.MatchedName + ".png");
                        else if (MediaType == "Table Image")
                            File.Move(item.File.FullName, @"Media\" + systemName + "\\Table Images\\" + item.MatchedName + ".png");
                    }
                    catch (Exception)
                    {

                    }
                   
                    //item.File.MoveTo(@"Media\" + systemName + "\\Wheel Images\\" + item.MatchedName + ".png");
                   // newMediaFilesList.Remove(m);
                }
            }
        }

        private void MatchMediaUncheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in datagrid_oddwheels.SelectedItems)
            {
                try
                {
                    var m = item as MediaFiles;
                    m.TagRename = false;
                }
                catch (Exception)
                {
                    
                }

            }

            datagrid_oddwheels.Items.Refresh();
        }




                        
        }
    }

