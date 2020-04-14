using System.Collections.ObjectModel;
using System.Xml.Serialization;
using EVEMon.Common.Enumerations.UISettings;

namespace EVEMon.Common.SettingsObjects
{
    /// <summary>
    /// Root UI Settings Class
    /// </summary>
    public sealed class UISettings
    {
        private readonly Collection<string> m_confirmedTips;

        /// <summary>
        /// Initializes a new instance of the <see cref="UISettings"/> class.
        /// </summary>
        public UISettings()
        {
            MainWindowCloseBehaviour = CloseBehaviour.Exit;

            WindowLocations = new ModifiedSerializableDictionary<string, WindowLocationSettings>();
            Splitters = new ModifiedSerializableDictionary<string, int>();
            m_confirmedTips = new Collection<string>();
            
            CertificateDataBrowser = new CertificateBrowserSettings();
            BlueprintDataBrowser = new BlueprintBrowserSettings();
            SkillDataBrowser = new SkillBrowserSettings();
            ShipDataBrowser = new ShipBrowserSettings();
            ItemDataBrowser = new ItemBrowserSettings();

            CertificateCharacterDataBrowser = new CertificateBrowserSettings();
            BlueprintCharacterDataBrowser = new BlueprintBrowserSettings();
            SkillCharacterDataBrowser = new SkillBrowserSettings();
            ShipCharacterDataBrowser = new ShipBrowserSettings();
            ItemCharacterDataBrowser = new ItemBrowserSettings();

            CertificateBrowser = new CertificateBrowserSettings();
            BlueprintBrowser = new BlueprintBrowserSettings();
            SkillBrowser = new SkillBrowserSettings();
            ShipBrowser = new ShipBrowserSettings();
            ItemBrowser = new ItemBrowserSettings();

            SystemTrayTooltip = new TrayTooltipSettings();
            SkillPieChart = new SkillPieChartSettings();
            SystemTrayPopup = new TrayPopupSettings();
            MainWindow = new MainWindowSettings();
            PlanWindow = new PlanWindowSettings();
            Scheduler = new SchedulerUISettings();

            UseStoredSearchFilters = true;
        }

        /// <summary>
        /// When true, removes images and colours to make EVEMon looks like some boring business application.
        /// </summary>
        [XmlElement("safeForWork")]
        public bool SafeForWork { get; set; }

        /// <summary>
        /// Gets or sets the main window close behaviour.
        /// </summary>
        /// <value>The main window close behaviour.</value>
        [XmlElement("closeBehaviour")]
        public CloseBehaviour MainWindowCloseBehaviour { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use stored search filters].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use stored search filters]; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("useStoredSearchFilters")]
        public bool UseStoredSearchFilters { get; set; }

        /// <summary>
        /// Gets or sets the main window.
        /// </summary>
        /// <value>
        /// The main window settings.
        /// </value>
        [XmlElement("mainWindow")]
        public MainWindowSettings MainWindow { get; set; }

        /// <summary>
        /// Gets or sets the plan window.
        /// </summary>
        /// <value>
        /// The plan window settings.
        /// </value>
        [XmlElement("planWindow")]
        public PlanWindowSettings PlanWindow { get; set; }

        /// <summary>
        /// Gets or sets the settings for the planner skill browser.
        /// </summary>
        /// <value>
        /// The skill browser settings.
        /// </value>
        [XmlElement("skillBrowser")]
        public SkillBrowserSettings SkillBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the planner certificate browser.
        /// </summary>
        /// <value>
        /// The certificate browser settings.
        /// </value>
        [XmlElement("certificateBrowser")]
        public CertificateBrowserSettings CertificateBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the planner ship browser.
        /// </summary>
        /// <value>
        /// The ship browser settings.
        /// </value>
        [XmlElement("shipBrowser")]
        public ShipBrowserSettings ShipBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the planner item browser.
        /// </summary>
        /// <value>
        /// The item browser settings.
        /// </value>
        [XmlElement("itemBrowser")]
        public ItemBrowserSettings ItemBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the planner blueprint browser.
        /// </summary>
        /// <value>
        /// The blueprint browser settings.
        /// </value>
        [XmlElement("blueprintBrowser")]
        public BlueprintBrowserSettings BlueprintBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data skill browser of a character.
        /// </summary>
        /// <value>
        /// The skill character data browser settings.
        /// </value>
        [XmlElement("skillCharacterDataBrowser")]
        public SkillBrowserSettings SkillCharacterDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data certificate browser of a character.
        /// </summary>
        /// <value>
        /// The certificate character data browser settings.
        /// </value>
        [XmlElement("certificateCharacterDataBrowser")]
        public CertificateBrowserSettings CertificateCharacterDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data ship browser of a character.
        /// </summary>
        /// <value>
        /// The ship character data browser settings.
        /// </value>
        [XmlElement("shipCharacterDataBrowser")]
        public ShipBrowserSettings ShipCharacterDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the settings for the data item browser of a character.
        /// </summary>
        /// <value>
        /// The item character data browser settings.
        /// </value>
        [XmlElement("itemCharacterDataBrowser")]
        public ItemBrowserSettings ItemCharacterDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data blueprint browser of a character.
        /// </summary>
        /// <value>
        /// The blueprint character data browser settings.
        /// </value>
        [XmlElement("blueprintCharacterDataBrowser")]
        public BlueprintBrowserSettings BlueprintCharacterDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data skill browser.
        /// </summary>
        /// <value>
        /// The skill data browser settings.
        /// </value>
        [XmlElement("skillDataBrowser")]
        public SkillBrowserSettings SkillDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data certificate browser.
        /// </summary>
        /// <value>
        /// The certificate data browser settings.
        /// </value>
        [XmlElement("certificateDataBrowser")]
        public CertificateBrowserSettings CertificateDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data ship browser.
        /// </summary>
        /// <value>
        /// The ship data browser settings.
        /// </value>
        [XmlElement("shipDataBrowser")]
        public ShipBrowserSettings ShipDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the settings for the data item browser.
        /// </summary>
        /// <value>
        /// The item data browser settings.
        /// </value>
        [XmlElement("itemDataBrowser")]
        public ItemBrowserSettings ItemDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the settings for the data blueprint browser.
        /// </summary>
        /// <value>
        /// The blueprint data browser settings.
        /// </value>
        [XmlElement("blueprintDataBrowser")]
        public BlueprintBrowserSettings BlueprintDataBrowser { get; set; }

        /// <summary>
        /// Gets or sets the system tray icon.
        /// </summary>
        /// <value>
        /// The system tray icon settings.
        /// </value>
        [XmlElement("systemTrayIcon")]
        public SystemTrayBehaviour SystemTrayIcon { get; set; }

        /// <summary>
        /// Gets or sets the system tray popup.
        /// </summary>
        /// <value>
        /// The system tray popup settings.
        /// </value>
        [XmlElement("systemTrayPopup")]
        public TrayPopupSettings SystemTrayPopup { get; set; }

        /// <summary>
        /// Gets or sets the scheduler.
        /// </summary>
        /// <value>
        /// The scheduler settings.
        /// </value>
        [XmlElement("calendar")]
        public SchedulerUISettings Scheduler { get; set; }

        /// <summary>
        /// Gets or sets the system tray tooltip.
        /// </summary>
        /// <value>
        /// The system tray tooltip settings.
        /// </value>
        [XmlElement("systemTrayTooltip")]
        public TrayTooltipSettings SystemTrayTooltip { get; set; }

        /// <summary>
        /// Gets or sets the skill pie chart.
        /// </summary>
        /// <value>
        /// The skill pie chart settings.
        /// </value>
        [XmlElement("skillPieChart")]
        public SkillPieChartSettings SkillPieChart { get; set; }

        /// <summary>
        /// Gets or sets the window locations.
        /// </summary>
        /// <value>
        /// The window locations.
        /// </value>
        [XmlElement("locations")]
        public ModifiedSerializableDictionary<string, WindowLocationSettings> WindowLocations { get; set; }

        /// <summary>
        /// Gets or sets the splitters.
        /// </summary>
        /// <value>
        /// The splitters.
        /// </value>
        [XmlElement("splitters")]
        public ModifiedSerializableDictionary<string, int> Splitters { get; set; }

        /// <summary>
        /// Gets or sets the confirmed tips.
        /// </summary>
        /// <value>
        /// The confirmed tips.
        /// </value>
        [XmlArray("confirmedTips")]
        [XmlArrayItem("tip")]
        public Collection<string> ConfirmedTips => m_confirmedTips;
    }
}
