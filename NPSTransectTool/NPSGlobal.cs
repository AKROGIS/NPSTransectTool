using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;

namespace NPSTransectTool
{
    public sealed class NPSGlobal
    {
        private static volatile NPSGlobal instance;
        private static object syncRoot = new System.Object();
        private ESRI.ArcGIS.Framework.IApplication m_Application;
        private ESRI.ArcGIS.ArcMapUI.IMxDocument m_MxDoc;
        private ESRI.ArcGIS.Carto.IMap m_Map;
        private System.Xml.Linq.XDocument m_XMLConfig;
        private string m_XMLConfigFilePath;
        private ESRI.ArcGIS.Geodatabase.IWorkspace m_Workspace;
        private string m_DatabasePath;
        private string m_DLLPath;
        private ESRI.ArcGIS.Editor.IEditor m_Editor;
        private bool m_ProgramaticFeatureEdit;
        private TransectToolForm m_MainTransectForm;
        private System.Windows.Forms.Label m_ProgressLabel;
        private int m_ProgressStepTotal;
        private int m_ProgressStepIndex;
        private Random m_Randomizer;




        private IDocumentEvents_OpenDocumentEventHandler OnOpenDocument_EvntHndlr;
        private IDocumentEvents_CloseDocumentEventHandler OnCloseDocument_EvntHndlr;
        private IEditEvents_OnCreateFeatureEventHandler OnCreateFeature_EvntHndlr;

        private bool m_IsInitialized;
        private bool m_IsInitArcMapBindings;
        private string m_InitErrorMessage;

        private string m_LYR_HORIZON;
        private string m_LYR_ANIMALS;
        private string m_LYR_TRACKLOG;
        private string m_LYR_GPSPOINTLOG;
        private string m_LYR_RANDOMPOINTS;
        private string m_LYR_GENERATED_TRANSECTS;
        private string m_LYR_FLAT_AREAS;
        private string m_LYR_EXCLUDED_AREAS;
        private string m_LYR_SURVEY_BOUNDARY;

        public string LYR_SURVEY_BOUNDARY { get { return m_LYR_SURVEY_BOUNDARY; } }
        public string LYR_EXCLUDED_AREAS { get { return m_LYR_EXCLUDED_AREAS; } }
        public string LYR_FLAT_AREAS { get { return m_LYR_FLAT_AREAS; } }
        public string LYR_GENERATED_TRANSECTS { get { return m_LYR_GENERATED_TRANSECTS; } }
        public string LYR_RANDOMPOINTS { get { return m_LYR_RANDOMPOINTS; } }
        public string LYR_GPSPOINTLOG { get { return m_LYR_GPSPOINTLOG; } }
        public string LYR_TRACKLOG { get { return m_LYR_TRACKLOG; } }
        public string LYR_HORIZON { get { return m_LYR_HORIZON; } }
        public string LYR_ANIMALS { get { return m_LYR_ANIMALS; } }
        public string TBL_DEFAULTVALUES { get { return "defaultvalues"; } }

        public string DLLPath { get { return m_DLLPath; } }
        public string XMLConfigFilePath { get { return m_XMLConfigFilePath; } }
        public System.Xml.Linq.XDocument XMLConfig { get { return m_XMLConfig; } }
        private System.Collections.Specialized.NameValueCollection m_SaveOptionCollection;
        public bool IsInitialized { get { return m_IsInitialized; } }
        public string InitErrorMessage { get { return m_InitErrorMessage; } }
        public ESRI.ArcGIS.Carto.IMap Map { get { return m_Map; } }
        public ESRI.ArcGIS.Geodatabase.IWorkspace Workspace { get { return m_Workspace; } }
        public string DatabasePath { get { return m_DatabasePath; } }
        public ESRI.ArcGIS.Editor.IEditor Editor { get { return m_Editor; } }
        public bool ProgramaticFeatureEdit { get { return m_ProgramaticFeatureEdit; } set { m_ProgramaticFeatureEdit = value; } }
        public TransectToolForm MainTransectForm { get { return m_MainTransectForm; } set { m_MainTransectForm = value; } }
        public int ProgressStepTotal { get { return m_ProgressStepTotal; } set { m_ProgressStepTotal = value; } }
        public int ProgressStepIndex { get { return m_ProgressStepIndex; } set { m_ProgressStepIndex = value; } }
        public Random Randomizer { get { return m_Randomizer; } set { m_Randomizer = value; } }
        public ESRI.ArcGIS.ArcMapUI.IMxDocument Document { get { return m_MxDoc; } set { m_MxDoc = value; } }
        public System.Windows.Forms.Label ProgressLabel { get { return m_ProgressLabel; } set { m_ProgressLabel = value; } }
        public ESRI.ArcGIS.Framework.IApplication Application { get { return m_Application; } }

        private NPSGlobal()
        {
            m_IsInitialized = false;
            m_IsInitArcMapBindings = false;
            m_ProgressStepTotal = -1;
            m_ProgressStepIndex = -1;
            m_Randomizer = new Random();//set seed from current time
        }

        /// <summary>
        /// singleton logic
        /// </summary>
        public static NPSGlobal Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NPSGlobal();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// this is the first initialization code which binds to ArcMap objects and events
        /// </summary>
        public void InitArcMapBindings(ESRI.ArcGIS.Framework.IApplication application)
        {
            if (m_IsInitArcMapBindings) return;

            //reference ArcMap globals
            m_Application = application;
            m_MxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)m_Application.Document;

            //wireup ArcMap events
            WireDocumentEvents(m_Application.Document);

            m_IsInitArcMapBindings = true;
        }


        /// <summary>
        /// makes sure that everything necessary to run the NPS tools are loaded and in place
        /// </summary>
        public void Init()
        {
            m_IsInitialized = false;
            m_InitErrorMessage = "";

            //make sure we have an application object
            if (m_Application == null)
                m_InitErrorMessage = "Could not get a hold of the ArcMap instance.";

            //the toolbar can only work with the specified mxd file
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                if (m_Application.Document.Title != "NPS_Transect_Tool_DOTNET.mxd" &&
                    m_Application.Document.Title != "NPS_Transect_Tool_DOTNET")
                    m_InitErrorMessage = "This is not the NPS_Transect_Tool_DOTNET.mxd MXD file.(current file name:"
                        + m_Application.Document.Title + ")";

            //get global editor
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                InitEditor(ref m_InitErrorMessage);

            //wire up handlers to editor events
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                WireEditEvents(m_Editor);

            //get the currently open  map
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                m_Map = m_MxDoc.FocusMap;

            //set global path to dll
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                InitDLLPath(ref m_InitErrorMessage);

            //set globals from XML data
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                InitXMLConfig(m_DLLPath, ref m_InitErrorMessage);

            //set global layer names
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                InitLayers(ref m_InitErrorMessage);

            //get database/workspace
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                InitDatabase(m_Map, ref m_InitErrorMessage);

            //if there was an error during initialization, set global init state to false and log
            if (string.IsNullOrEmpty(m_InitErrorMessage))
                m_IsInitialized = true;

        }

        /// <summary>
        /// uninitialize the nps global instance
        /// </summary>
        public void UnInit()
        {
            m_IsInitialized = false;

            Util.SaveConfigSettings();
            UnwireEditEvents(m_Editor);

            m_DLLPath = null;
            m_Map = null;
            m_XMLConfig = null;
            m_XMLConfigFilePath = null;
            m_Workspace = null;
            m_DatabasePath = null;
            m_DLLPath = null;
            m_Editor = null;
            m_ProgramaticFeatureEdit = false;
            m_InitErrorMessage = null;

            m_LYR_HORIZON = m_LYR_ANIMALS = m_LYR_TRACKLOG
                = m_LYR_GPSPOINTLOG = m_LYR_RANDOMPOINTS
                = m_LYR_GENERATED_TRANSECTS = m_LYR_FLAT_AREAS
                = m_LYR_EXCLUDED_AREAS = m_LYR_SURVEY_BOUNDARY = null;
        }

        /// <summary>
        /// get the global editor object for ArcMap
        /// </summary>
        private void InitEditor(ref string ErrorMessage)
        {
            ESRI.ArcGIS.esriSystem.UID pID;

            try
            {
                ////store editor object for use throughout extension
                pID = new ESRI.ArcGIS.esriSystem.UIDClass();
                pID.Value = "esriEditor.Editor";
                m_Editor = (ESRI.ArcGIS.Editor.IEditor)m_Application.FindExtensionByCLSID(pID);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Could not locate Editor. " + ex.Message;
            }
        }

        /// <summary>
        /// unwire the editor events when we don't care to listen for events any more
        /// </summary>
        public void UnwireEditEvents(IEditor pEditor)
        {
            if (pEditor == null) return;

            ((IEditEvents_Event)pEditor).OnCreateFeature -= OnCreateFeature_EvntHndlr;
        }

        /// <summary>
        /// wire up handlers to the global editor so it will let us know when certain things
        /// happen
        /// </summary>
        private void WireEditEvents(IEditor pEditor)
        {
            UnwireEditEvents(pEditor);

            OnCreateFeature_EvntHndlr = new IEditEvents_OnCreateFeatureEventHandler
                (NPSEventHandlers.OnCreateFeature);
            ((IEditEvents_Event)pEditor).OnCreateFeature += OnCreateFeature_EvntHndlr;
        }

        /// <summary>
        /// wireup a handler to the document's close event so we can do some work before we
        /// exit
        /// </summary>
        private void WireDocumentEvents(ESRI.ArcGIS.Framework.IDocument ThisDocument)
        {
            UnWireDocumentEvents(ThisDocument);

            OnCloseDocument_EvntHndlr = new IDocumentEvents_CloseDocumentEventHandler
                (NPSEventHandlers.CloseMxDocument);
            ((IDocumentEvents_Event)ThisDocument).CloseDocument += OnCloseDocument_EvntHndlr;

            OnOpenDocument_EvntHndlr = new IDocumentEvents_OpenDocumentEventHandler
              (NPSEventHandlers.OpenMxDocument);
            ((IDocumentEvents_Event)ThisDocument).OpenDocument += OnOpenDocument_EvntHndlr;

        }

        /// <summary>
        /// remove the wired handlers
        /// </summary>
        private void UnWireDocumentEvents(ESRI.ArcGIS.Framework.IDocument ThisDocument)
        {
            ((IDocumentEvents_Event)ThisDocument).CloseDocument -= OnCloseDocument_EvntHndlr;
            ((IDocumentEvents_Event)ThisDocument).OpenDocument -= OnOpenDocument_EvntHndlr;
        }

        /// <summary>
        /// obtain path to executing dll
        /// </summary>
        private void InitDLLPath(ref string ErrorMessage)
        {
            try
            {
                m_DLLPath = System.Reflection.Assembly.GetAssembly(GetType()).Location;
                m_DLLPath = m_DLLPath.Substring(0, m_DLLPath.LastIndexOf(@"\"));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while locating path to NPS DLL. " + ex.Message;
            }
        }

        /// <summary>
        /// find and load the xml configuration file and set the globals that
        /// need to be set from the contents of the file
        /// </summary>
        private void InitXMLConfig(string DLLPath, ref string ErrorMessage)
        {

            try
            {
                m_XMLConfigFilePath = System.IO.Path.Combine(DLLPath, "NPSConfig.xml");
                m_XMLConfig = System.Xml.Linq.XDocument.Load(m_XMLConfigFilePath);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while parsing XML configuration file. " + ex.Message;
                return;
            }
        }

        /// <summary>
        /// set global layer names from xml config file
        /// </summary>
        private void InitLayers(ref string ErrorMessage)
        {

            m_LYR_FLAT_AREAS = Util.GetConfigSetting("FlatAreas", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_FLAT_AREAS))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the FlatAreas layer");
                return;
            }

            m_LYR_GENERATED_TRANSECTS = Util.GetConfigSetting("TransectLines", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_GENERATED_TRANSECTS))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the TransectLines layer");
                return;
            }

            m_LYR_RANDOMPOINTS = Util.GetConfigSetting("RandomPoints", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_RANDOMPOINTS))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the RandomPoints layer");
                return;
            }

            m_LYR_SURVEY_BOUNDARY = Util.GetConfigSetting("BoundaryAreas", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_SURVEY_BOUNDARY))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the BoundaryAreas layer");
                return;
            }

            m_LYR_TRACKLOG = Util.GetConfigSetting("TrackLog", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_TRACKLOG))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the TrackLog layer");
                return;
            }

            m_LYR_ANIMALS = Util.GetConfigSetting("Animals", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_ANIMALS))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the Animals layer");
                return;
            }

            m_LYR_EXCLUDED_AREAS = Util.GetConfigSetting("ElevationAreas", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_EXCLUDED_AREAS))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the ElevationAreas layer");
                return;
            }

            m_LYR_HORIZON = Util.GetConfigSetting("Horizon", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_HORIZON))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the Horizon layer");
                return;
            }

            m_LYR_GPSPOINTLOG = Util.GetConfigSetting("GPSPoints", "FeatureClass");
            if (string.IsNullOrEmpty(m_LYR_GPSPOINTLOG))
            {
                ErrorMessage = string.Format("The XML file is missing the name for the GPSPoints layer");
                return;
            }

        }

        /// <summary>
        /// load collection with saved options from the xml config file
        /// </summary>
        private void InitSavedOptionsCollection(ref string ErrorMessage)
        {
            IEnumerable<System.Xml.Linq.XElement> SavedOptions;

            m_SaveOptionCollection = new System.Collections.Specialized.NameValueCollection();
            m_SaveOptionCollection.Add("RememberLastSurveyID", "");
            m_SaveOptionCollection.Add("TransectTotal", "");
            m_SaveOptionCollection.Add("StartX", "");
            m_SaveOptionCollection.Add("StartY", "");
            m_SaveOptionCollection.Add("MaxTransLineLength", "");
            m_SaveOptionCollection.Add("BlindAreaBuffer", "");
            m_SaveOptionCollection.Add("BufferDistance", "");
            m_SaveOptionCollection.Add("BufferFCName", "");
            m_SaveOptionCollection.Add("MinTransLineLength", "");
            m_SaveOptionCollection.Add("MaxElevation", "");
            m_SaveOptionCollection.Add("MaxSlope", "");
            m_SaveOptionCollection.Add("MinSlope", "");
            m_SaveOptionCollection.Add("ImportDataPath", "");
            m_SaveOptionCollection.Add("ExportDataPath", "");
            m_SaveOptionCollection.Add("GridPointSpacing", "");
            m_SaveOptionCollection.Add("TotalRandomPoints", "");
            m_SaveOptionCollection.Add("SurveyID", "");
            m_SaveOptionCollection.Add("DemFileLocation", "");

            SavedOptions = m_XMLConfig.Descendants("NPSConfig");
            foreach (System.Xml.Linq.XElement ThisOption in SavedOptions)
            {
                for (int Index = 0; Index < m_SaveOptionCollection.Keys.Count; Index++)
                {
                    if (m_SaveOptionCollection.GetKey(Index).ToLower() == ThisOption.Name.LocalName.ToLower())
                        m_SaveOptionCollection[m_SaveOptionCollection.GetKey(Index)] = ThisOption.Value;
                }
            }

            SavedOptions = m_XMLConfig.Descendants("StoredOptions");
            foreach (System.Xml.Linq.XElement ThisOption in SavedOptions)
            {
                for (int Index = 0; Index < m_SaveOptionCollection.Keys.Count; Index++)
                {
                    if (m_SaveOptionCollection.GetKey(Index).ToLower() == ThisOption.Name.LocalName.ToLower())
                        m_SaveOptionCollection[m_SaveOptionCollection.GetKey(Index)] = ThisOption.Value;
                }
            }

        }

        /// <summary>
        /// obtain the database workspace and path dynamically from the layers in the map
        /// </summary>
        private void InitDatabase(ESRI.ArcGIS.Carto.IMap ThisMap, ref string ErrorMessage)
        {
            ESRI.ArcGIS.Carto.ILayer ThisLayer;
            ESRI.ArcGIS.Geodatabase.IDataset ThisDataset;
            int LayerCount;

            LayerCount = ThisMap.LayerCount;

            for (int LayerIndex = 0; LayerIndex < LayerCount; LayerIndex++)
            {
                ThisLayer = ThisMap.get_Layer(LayerIndex);

                if (!(ThisLayer is ESRI.ArcGIS.Carto.IFeatureLayer))
                    continue;

                if (((ESRI.ArcGIS.Carto.IFeatureLayer)ThisLayer).FeatureClass == null)
                    continue;

                ThisDataset = (ESRI.ArcGIS.Geodatabase.IDataset)
                    ((ESRI.ArcGIS.Carto.IFeatureLayer)ThisLayer).FeatureClass;

                if (ThisDataset.Name != m_LYR_GENERATED_TRANSECTS)
                    continue;

                m_Workspace = ThisDataset.Workspace;
                m_DatabasePath = ThisDataset.Workspace.PathName;
                break;

            }

            if (string.IsNullOrEmpty(m_DatabasePath))
                ErrorMessage = "Could not find the transect layer used to detect the database.";

        }

    }

}
