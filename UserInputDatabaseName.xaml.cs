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
using System.Windows.Shapes;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;


namespace PinballXManager
{
    /// <summary>
    /// Interaction logic for UserInputDatabaseName.xaml
    /// </summary>
    public partial class UserInputDatabaseName : MetroWindow
    {
        static List<Table> master_tables;

        private string systemName;
        public string SystemName
        {
            get { return systemName; }
            set { systemName = value; }
        }
        private string databaseName;
        public string DataBaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }
        
        public UserInputDatabaseName(List <Table> master_list)
        {
            InitializeComponent();
            master_tables = master_list;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            labelText.Content = "Creating database for system: \n" + SystemName;
        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (File.Exists(@"Databases\" + SystemName + "\\" + textBoxXmlName.Text + ".xml"))
            {
                MessageBox.Show("Database already exists under this name.");
                 return;
            } 
            XmlTextWriter writer = new XmlTextWriter(@"Databases\" + SystemName + "\\" + textBoxXmlName.Text + ".xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("menu");
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            this.Close();
              
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
           // SerializeToXML(master_tables, "Visual Pinball", "IPDB.xml");
        }

        static public void SerializeToXML(List<Table> tables, string systemname, string dbName)
        {
            foreach (var item in tables)
            {
                item.Name = item.Description;
            }
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
    }
}
