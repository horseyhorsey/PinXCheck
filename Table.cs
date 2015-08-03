using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PinballXManager
{
     public enum PinXSystemTypes
    {
        Custom,
        VisualPinball,
        FuturePinball
    };

    [XmlType("game")]
    public class Table : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public Table()
        { }
        public Table(string Gamename, string Description)
        {
            this.Name = Gamename;
            this.Description = Description;
        }

        //Constructor for MasterTables
        public Table(int IPDB,string Gamename, string manu,string type, int year,float rating,string theme,int players,string abbr,int units)
        {
            this.Name = Gamename;
            this.Manufacturer = manu;
            this.Year = year;
            this.Type = type;
            this.ipdbNumber = IPDB;
            this.Rating = convertRatings(rating);

            this.Theme = theme;
            this.Players = players;
            this.Abbreaviation = abbr;
            this.Units = units;


            this.Description = Gamename + " (" + manu + " " + year + ")";
            
        }
        public Table(string Gamename, string Description,string rom, string manu, int year,string type,
                       bool enabled, bool hidedmd, bool hideBG, int rating, string altExe, string table_dir, ObservableCollection<Table> master_tables
            ,bool desktop)
        {
            this.Name = Gamename;
            this.Description = Description;
            this.Rom = rom;
            this.Manufacturer = manu;
            this.Year = year;
            this.Type = type;
            this.Enabled = enabled;
            this.Hidedmd = hidedmd;
            this.Hidebackglass = hideBG;
            this.Rating = rating;
            this.AlternateExe = altExe;
            this.tableFileExists = getTableFileName(table_dir);
            List<Table> tables = new List<Table>(master_tables);
            this.matchedDescription = this.MatchDescription(this.Description, tables);
            this.Desktop = desktop;
        }

        public int convertRatings(float rating)        
        {
            if (rating >= 8.1)
                this.Rating = 5;
            else if (rating >=7.4 && rating <8.1)
                this.Rating = 4;
            else if (rating >=6.9 && rating < 7.4)
                this.Rating = 3;
            else if (rating >=6.5 && rating < 6.9)
                this.Rating = 2;
            else if (rating >=0 && rating < 6.5)
                this.Rating = 1;

            return this.Rating = (int)Rating;
        }

        public bool getTableFileName(string table_dir)
        {
            string file = table_dir + "\\" + this.Name;
           
            if (File.Exists(file + ".vpt") || File.Exists(file + ".vpx") || File.Exists(file + ".fpt"))
            {
                this.TableFileExists = true;
                return true;
            }
            else
            {
                this.TableFileExists = false;
                return false;
            }
        }

        public bool MatchDescription(string description,List<Table> tables_list)
        {
            var value = tables_list.Find(item => item.Description.ToUpper() == description.ToUpper());
            if (value != null)
            {
                this.matchedDescription = true;
                this.NotifyPropertyChanged("matchedDescription");

                return true;
            }
            else
            {
                this.matchedDescription = false;
                this.NotifyPropertyChanged("matchedDescription");

                return false;
            }
        }

        //public string getTableFileName(string )
        private string matchedName;
        [XmlIgnore]
        public string MatchedName
        {
            get { return matchedName; }
            set { matchedName = value;
            this.NotifyPropertyChanged("matchedName");
            }
        }
        private int matchedYear;
        [XmlIgnore]
        public int MatchedYear
        {
            get { return matchedYear; }
            set
            {
                matchedYear = value;
                this.NotifyPropertyChanged("matchedYear");
            }
        }
        private string matchedManu;
        [XmlIgnore]
        public string MatchedManu
        {
            get { return matchedManu; }
            set
            {
                matchedManu = value;
                this.NotifyPropertyChanged("matchedManu");
            }
        }
        private int matchedRating;
        [XmlIgnore]
        public int MatchedRating
        {
            get { return matchedRating; }
            set
            {
                matchedRating = value;
                this.NotifyPropertyChanged("matchedRating");
            }
        }
        private string matchedType;
        [XmlIgnore]
        public string MatchedType
        {
            get { return matchedType; }
            set
            {
                matchedType = value;
                this.NotifyPropertyChanged("matchedType");
            }
        }
        private string matchedB2SName;
        [XmlIgnore]
        public string MatchedB2SName
        {
            get { return matchedB2SName; }
            set
            {
                matchedB2SName = value;
                this.NotifyPropertyChanged("matchedB2SName");
            }
        }
        private int matchedPercent;
        [XmlIgnore]
        public int MatchedPercent
        {
            get { return matchedPercent; }
            set
            {
                matchedPercent = value;
                this.NotifyPropertyChanged("matchedPercent");
            }
        }
        #region Accessors & Variables
        private bool tableFileExists;
        [XmlIgnore]
        public bool TableFileExists
        {
            get { return tableFileExists; }
            set { tableFileExists = value;
            this.NotifyPropertyChanged("tableFileExists");
            }
        }
        private int ipdbNumber;
        [XmlIgnore]
        public int IPDBNumber
        {
            get { return ipdbNumber; }
            set { ipdbNumber = value; }
        }
        private bool matchedDescription;
        [XmlIgnore]
        public bool MatchedDescription
        {
            get { return matchedDescription; }
            set { matchedDescription = value;
            this.NotifyPropertyChanged("matchedDescription");
            }
        }
        private string theme;
        [XmlIgnore]
        public string Theme
        {
            get { return theme; }
            set
            {
                theme = value;
                this.NotifyPropertyChanged("theme");
            }
        }

        private string author;
        [XmlIgnore]
        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                this.NotifyPropertyChanged("author");
            }
        }
        
        private string abbreaviation;
        [XmlIgnore]
        public string Abbreaviation
        {
            get { return abbreaviation; }
            set
            {
                abbreaviation = value;
                this.NotifyPropertyChanged("abbreaviation");
            }
        }
        private int players;
        [XmlIgnore]
        public int Players
        {
            get { return players; }
            set
            {
                players = value;
                this.NotifyPropertyChanged("players");
            }
        }
        private int units;
        [XmlIgnore]
        public int Units
        {
            get { return units; }
            set
            {
                units = value;
                this.NotifyPropertyChanged("units");
            }
        }

        private bool flagRename;
        [XmlIgnore]
        public bool FlagRename
        {
            get { return flagRename; }
            set
            {
                flagRename = value;
                this.NotifyPropertyChanged("flagRename");
            }
        }
        private bool flagRenameB2S;
        [XmlIgnore]
        public bool FlagRenameB2S
        {
            get { return flagRenameB2S; }
            set
            {
                flagRenameB2S = value;
                this.NotifyPropertyChanged("flagRenameB2S");
            }
        }

        private bool haveWheels;
        [XmlIgnore]
        public bool HaveWheels
        {
            get { return haveWheels; }
            set
            {
                haveWheels = value;
                this.NotifyPropertyChanged("haveWheels");
            }
        }

        private bool haveBGImage;
        [XmlIgnore]
        public bool HaveBGImage
        {
            get { return haveBGImage; }
            set
            {
                haveBGImage = value;
                this.NotifyPropertyChanged("haveBGImage");
            }
        }

        private bool haveBGVids;
        [XmlIgnore]
        public bool HaveBGVids
        {
            get { return haveBGVids; }
            set
            {
                haveBGVids = value;
                this.NotifyPropertyChanged("haveBGVids");
            }
        }

        private bool haveDmdImg;
        [XmlIgnore]
        public bool HaveDmdImg
        {
            get { return haveDmdImg; }
            set
            {
                haveDmdImg = value;
                this.NotifyPropertyChanged("haveDmdImg");
            }
        }

        private bool haveDmdVids;
        [XmlIgnore]
        public bool HaveDmdVids
        {
            get { return haveDmdVids; }
            set
            {
                haveDmdVids = value;
                this.NotifyPropertyChanged("haveDmdVids");
            }
        }

        private bool haveRealDmdImg;
        [XmlIgnore]
        public bool HaveRealDmdImg
        {
            get { return haveRealDmdImg; }
            set
            {
                haveRealDmdImg = value;
                this.NotifyPropertyChanged("haveRealDmdImg");
            }
        }

        private bool haveRealDmdVids;
        [XmlIgnore]
        public bool HaveRealDmdVids
        {
            get { return haveRealDmdVids; }
            set
            {
                haveRealDmdVids = value;
                this.NotifyPropertyChanged("haveRealDmdVids");
            }
        }

        private bool haveTableAudio;
        [XmlIgnore]
        public bool HaveTableAudio
        {
            get { return haveTableAudio; }
            set
            {
                haveTableAudio = value;
                this.NotifyPropertyChanged("haveTableAudio");
            }
        }

        private bool haveTableImage;
        [XmlIgnore]
        public bool HaveTableImage
        {
            get { return haveTableImage; }
            set
            {
                haveTableImage = value;
                this.NotifyPropertyChanged("haveTableImage");
            }
        }

        private bool haveTableVideo;
        [XmlIgnore]
        public bool HaveTableVideo
        {
            get { return haveTableVideo; }
            set
            {
                haveTableVideo = value;
                this.NotifyPropertyChanged("haveTableVideo");
            }
        }

        private bool haveTableImageDT;
        [XmlIgnore]
        public bool HaveTableImageDT
        {
            get { return haveTableImageDT; }
            set
            {
                haveTableImageDT = value;
                this.NotifyPropertyChanged("haveTableImageDT");
            }
        }

        private bool haveTableVideoDT;
        [XmlIgnore]
        public bool HaveTableVideoDT
        {
            get { return haveTableVideoDT; }
            set
            {
                haveTableVideoDT = value;
                this.NotifyPropertyChanged("haveTableVideoDT");
            }
        }

        #region Flyers

        private bool haveFlyerBack;
        [XmlIgnore]
        public bool HaveFlyerBack
        {
            get { return haveFlyerBack; }
            set
            {
                haveFlyerBack = value;
                this.NotifyPropertyChanged("haveFlyerBack");
            }
        }
        private bool haveFlyerFront;
        [XmlIgnore]
        public bool HaveFlyerFront
        {
            get { return haveFlyerFront; }
            set
            {
                haveFlyerFront = value;
                this.NotifyPropertyChanged("haveFlyerFront");
            }
        }
        private bool haveInside1;
        [XmlIgnore]
        public bool HaveInside1
        {
            get { return haveInside1; }
            set
            {
                haveInside1 = value;
                this.NotifyPropertyChanged("haveInside1");
            }
        }
        private bool haveInside2;
        [XmlIgnore]
        public bool HaveInside2
        {
            get { return haveInside2; }
            set
            {
                haveInside2 = value;
                this.NotifyPropertyChanged("haveInside2");
            }
        }
        private bool haveInside3;
        [XmlIgnore]
        public bool HaveInside3
        {
            get { return haveInside3; }
            set
            {
                haveInside3 = value;
                this.NotifyPropertyChanged("haveInside3");
            }
        }
        private bool haveInside4;
        [XmlIgnore]
        public bool HaveInside4
        {
            get { return haveInside4; }
            set
            {
                haveInside4 = value;
                this.NotifyPropertyChanged("haveInside4");
            }
        }
        private bool haveInside5;
        [XmlIgnore]
        public bool HaveInside5
        {
            get { return haveInside5; }
            set
            {
                haveInside5 = value;
                this.NotifyPropertyChanged("haveInside5");
            }
        }
        private bool haveInside6;
        [XmlIgnore]
        public bool HaveInside6
        {
            get { return haveInside6; }
            set
            {
                haveInside6 = value;
                this.NotifyPropertyChanged("haveInside6");
            }
        }
        private bool haveInside7;
        [XmlIgnore]
        public bool HaveInside7
        {
            get { return haveInside7; }
            set
            {
                haveInside7 = value;
                this.NotifyPropertyChanged("haveInside7");
            }
        }
        private bool haveInside8;
        [XmlIgnore]
        public bool HaveInside8
        {
            get { return haveInside8; }
            set
            {
                haveInside8 = value;
                this.NotifyPropertyChanged("haveInside8");
            }
        }

        #endregion

        #region Instructions

        private bool haveInstruction;
        [XmlIgnore]
        public bool HaveInstruction
        {
            get { return haveInstruction; }
            set
            {
                haveInstruction = value;
                this.NotifyPropertyChanged("haveInstruction");
            }
        }
        private bool haveInstruction1;
        [XmlIgnore]
        public bool HaveInstruction1
        {
            get { return haveInstruction1; }
            set
            {
                haveInstruction1 = value;
                this.NotifyPropertyChanged("haveInstruction1");
            }
        }
        private bool haveInstruction2;
        [XmlIgnore]
        public bool HaveInstruction2
        {
            get { return haveInstruction2; }
            set
            {
                haveInstruction2 = value;
                this.NotifyPropertyChanged("haveInstruction2");
            }
        }
        private bool haveInstruction3;
        [XmlIgnore]
        public bool HaveInstruction3
        {
            get { return haveInstruction3; }
            set
            {
                haveInstruction3 = value;
                this.NotifyPropertyChanged("haveInstruction3");
            }
        }
        private bool haveInstruction4;
        [XmlIgnore]
        public bool HaveInstruction4
        {
            get { return haveInstruction4; }
            set
            {
                haveInstruction4 = value;
                this.NotifyPropertyChanged("haveInstruction4");
            }
        }
        private bool haveInstruction5;
        [XmlIgnore]
        public bool HaveInstruction5
        {
            get { return haveInstruction5; }
            set
            {
                haveInstruction5 = value;
                this.NotifyPropertyChanged("haveInstruction5");
            }
        }
        private bool haveInstruction6;
        [XmlIgnore]
        public bool HaveInstruction6
        {
            get { return haveInstruction6; }
            set
            {
                haveInstruction6 = value;
                this.NotifyPropertyChanged("haveInstruction6");
            }
        }

       

        #endregion

        #region DirectB2S
        private bool haveDirectB2S;
        [XmlIgnore]
        public bool HaveDirectB2S
        {
            get { return haveDirectB2S; }
            set
            {
                haveDirectB2S = value;
                this.NotifyPropertyChanged("haveDirectB2S");
            }
        }        
        #endregion

        #region Videos
        private bool haveGameplayVid;
        [XmlIgnore]
        public bool HaveGameplayVid
        {
            get { return haveGameplayVid; }
            set
            {
                haveGameplayVid = value;
                this.NotifyPropertyChanged("haveGameplayVid");
            }
        }
        private bool haveTutVid;
        [XmlIgnore]
        public bool HaveTutVid
        {
            get { return haveTutVid; }
            set
            {
                haveTutVid = value;
                this.NotifyPropertyChanged("haveTutVid");
            }
        }
        private bool havePromoVid;
        [XmlIgnore]
        public bool HavePromoVid
        {
            get { return havePromoVid; }
            set
            {
                havePromoVid = value;
                this.NotifyPropertyChanged("havePromoVid");
            }
        }
        #endregion


        private string name;
        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }

            set { name = value;
            this.NotifyPropertyChanged("name");
            }
            
        }
        private string description;
        [XmlElement("description")]
        public string Description
        {
            get { return description; }
            set { description = value;
            this.NotifyPropertyChanged("description");
            }
        }
        private string rom;
        [XmlElement("rom")]
        public string Rom
        {
            get { return rom; }
            set
            {
                rom = value;
                this.NotifyPropertyChanged("rom");
            }
        }
        private int year;
         [XmlElement("year")]
        public int Year
        {
            get { return year; }
            set { year = value;
            this.NotifyPropertyChanged("year");
            }
        }
        private int rating;
         [XmlElement("rating")]
        public int Rating
        {
            get { return rating; }
            set
            {
                rating = value;
            this.NotifyPropertyChanged("rating");
            }
        }        
        private string type;
         [XmlElement("type")]
        public string Type
        {
            get { return type; }
            set { type = value;
            this.NotifyPropertyChanged("type");
            }
        }
        private string manufacturer;
         [XmlElement("manufacturer")]
        public string Manufacturer
        {
            get { return manufacturer; }
            set { manufacturer = value;
            this.NotifyPropertyChanged("manufacturer");
            }
        }
        private bool enabled;
         [XmlElement("enabled")]
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value;
            this.NotifyPropertyChanged("enabled");
            }
        }
        private bool hidedmd;
         [XmlElement("hidedmd")]
        public bool Hidedmd
        {
            get { return hidedmd; }
            set { hidedmd = value;
            this.NotifyPropertyChanged("hidedmd");
            }
        }
        private bool hidebackglass;
         [XmlElement("hidebackglass")]
        public bool Hidebackglass
        {
            get { return hidebackglass; }
            set { hidebackglass = value;
            this.NotifyPropertyChanged("hidebackglass");
            }
        }
        private string alternateExe;        
         [XmlElement("alternateExe")]
        public string AlternateExe
        {
            get { return alternateExe; }
            set { alternateExe = value;
            this.NotifyPropertyChanged("alternateExe");
            }
           
        }
        private bool desktop;
        [XmlElement("Desktop")]
        public bool Desktop
         {
             get { return desktop; }
             set
             {
                 desktop = value;
                 this.NotifyPropertyChanged("desktop");
             }
         }

        
       
        #endregion
        
    }

    public class PinXSystem
    {
        public PinXSystem(int type, bool enabled,string id, string sysname, string workpath,string tablepath,string exe,string parameters,
                            bool launchBefore, string launchBeforePath, string launchBeforeexe, string launchBeforeParams,
            bool launchBeforeWaitForExit, bool launchBeforeHideWindow, bool launchAfter, bool launchAfterWaitForExit, string launchAfterexe,
            string launchAfterParams, bool launchAfterHideWindow, string launchAfterWorkingPath,string nvRam) 
        {
            this.SysType = type;
            this.ID = id;
            this.Enabled = enabled;
            this.SysName = sysname;
            this.WorkingPath = workpath;
            this.Executable = exe;
            this.TablePath = tablepath;
            this.Parameters = parameters;

            this.LaunchBefore = launchBefore;
            this.LaunchBeforePath = launchBeforePath;
            this.LaunchBeforeexe = launchBeforeexe;
            this.LaunchBeforeParams = launchBeforeParams;
            this.LaunchBeforeWaitForExit = launchBeforeWaitForExit;
            this.LaunchBeforeHideWindow = launchBeforeHideWindow;

            this.LaunchAfter = launchAfter;
            this.LaunchAfterWorkingPath = launchAfterWorkingPath;
            this.LaunchAfterexe = launchAfterexe;
            this.LaunchAfterParams = launchAfterParams;
            this.LaunchAfterWaitForExit = launchAfterWaitForExit;
            this.LaunchAfterHideWindow = launchAfterHideWindow;

            this.NVRAMPATH = nvRam;
            //PinXSystemTypes pi = PinXSystemTypes.Custom;
            //int i = (int)pi;
            
        }

        //public enum PinXSystemTypes {get; set;}


        public PinXSystem(string exe)
        {
            this.Executable = exe;
        }

        public PinXSystem()
        {
           
        }

        private int sysType;

        public int SysType
        {
            get { return sysType; }
            set { sysType = value; }
        }
        

        private bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        

        private string id;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }
        
        private string name;
        public string SysName
        {
            get { return name; }
            set { name = value; }
        }
        

        private string working_path;
        public string WorkingPath
        {
            get { return working_path; }
            set { working_path = value; }
        }
        private string table_dir;

        public string TablePath
        {
            get { return table_dir; }
            set { table_dir = value; }
        }

        private string executable;
        public string Executable
        {
            get { return executable; }
            set { executable = value; }
        }

        private string parameters;
        public string Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        private bool launchBefore;
	    public bool LaunchBefore
	    {
		    get { return launchBefore;}
		    set { launchBefore = value;}
	    }
        private string launchBeforePath;
	    public string LaunchBeforePath
	    {
		    get { return launchBeforePath;}
		    set { launchBeforePath = value;}
	    }

       private string launchBeforeexe;
	    public string LaunchBeforeexe
	    {
		    get { return launchBeforeexe;}
		    set { launchBeforeexe = value;}
	    }
	
        private string launchBeforeParams;
	    public string LaunchBeforeParams
	    {
		    get { return launchBeforeParams;}
		    set { launchBeforeParams = value;}
	    }

        private bool launchBeforeWaitForExit;
	    public bool LaunchBeforeWaitForExit
	    {
		    get { return launchBeforeWaitForExit;}
		    set { launchBeforeWaitForExit = value;}
	    }
        private bool launchBeforeHideWindow;
	    public bool LaunchBeforeHideWindow
	    {
		    get { return launchBeforeHideWindow;}
		    set { launchBeforeHideWindow = value;}
	    }

        private bool launchAfter;
	    public bool LaunchAfter
	    {
		    get { return launchAfter;}
		    set { launchAfter = value;}
	    }
	        private string launchAfterWorkingPath;

	    public string LaunchAfterWorkingPath
	    {
		    get { return launchAfterWorkingPath;}
		    set { launchAfterWorkingPath = value;}
	    }

        private string launchAfterexe;
	    public string LaunchAfterexe
	    {
		    get { return launchAfterexe;}
		    set { launchAfterexe = value;}
	    }

        private string launchAfterParams;
        public string LaunchAfterParams
        {
            get { return launchAfterParams; }
            set { launchAfterParams = value; }
        }

       private bool launchAfterHideWindow;
	    public bool LaunchAfterHideWindow
	    {
		    get { return launchAfterHideWindow;}
		    set { launchAfterHideWindow = value;}
	    }
	
        private bool launchAfterWaitForExit;
	    public bool LaunchAfterWaitForExit
	    {
		    get { return launchAfterWaitForExit;}
		    set { launchAfterWaitForExit = value;}
	    }

        private string NVramPath;

	    public string NVRAMPATH
	    {
		    get { return NVramPath;}
		    set { NVramPath = value;}
	    }
	  
    }
}
