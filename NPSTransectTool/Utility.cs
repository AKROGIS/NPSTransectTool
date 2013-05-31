using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.esriSystem;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace NPSTransectTool
{
    internal static class Util
    {
        /// <summary>
        ///     get the specified config value
        /// </summary>
        public static string GetConfigSetting(string OptionName)
        {
            return GetConfigSetting(OptionName, "NPSConfig");
        }

        /// <summary>
        ///     get the specified config value - specify parent element
        /// </summary>
        public static string GetConfigSetting(string OptionName, string ParentElement)
        {
            try
            {
                XDocument XDoc = NPSGlobal.Instance.XMLConfig;
                if (XDoc == null) return "";

                IEnumerable<XElement> results = from x in XDoc.Descendants(ParentElement)
                                                let xAttribute = x.Attribute("name")
                                                where xAttribute != null && xAttribute.Value.ToLower() == OptionName.ToLower()
                                                select x;

                return results.First().Value;

            }
            catch (Exception ex)
            {
                Debug.Print("Unhandled Exception " + ex.Message);
            }

            return "";
        }

        /// <summary>
        ///     change a config value - will be saved to file when ArcMap closes
        /// </summary>
        public static void SetConfigSetting(string OptionName, string OptionValue)
        {
            SetConfigSetting(OptionName, OptionValue, "NPSConfig");
        }

        /// <summary>
        ///     change a config value - will be saved to file when ArcMap closes - specify parent element
        /// </summary>
        public static void SetConfigSetting(string OptionName, string OptionValue, string ParentElement)
        {
            try
            {
                XDocument xDoc = NPSGlobal.Instance.XMLConfig;
                if (xDoc == null) return;

                IEnumerable<XElement> results = from x in xDoc.Descendants(ParentElement)
                                                let xAttribute = x.Attribute("name")
                                                where xAttribute != null && xAttribute.Value.ToLower() == OptionName.ToLower()
                                                select x;

                results.First().Value = OptionValue;
            }
            catch(Exception ex)
            {
                Debug.Print("Unhandled Exception " + ex.Message);
            }
        }

        /// <summary>
        ///     save in memory xml doc to file, occurs when ArcMap is closing or after certain operations
        /// </summary>
        public static void SaveConfigSettings()
        {
            try
            {
                XDocument xDoc = NPSGlobal.Instance.XMLConfig;
                if (xDoc == null) return;

                xDoc.Save(NPSGlobal.Instance.XMLConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.Print("Unhandled Exception " + ex.Message);
            }
        }

        /// <summary>
        ///     gets a cursor of all the selected features for a layer on the map that is bound to the specified
        ///     featureclass.
        /// </summary>
        public static IFeatureCursor GetLayerSelection(string NPSFCName, bool IsUpdateCursor,
                                                       bool ReturnAllIfNoSelection, ref int ResultCount,
                                                       ref bool IsSelection,
                                                       ref string DefExpress, ref string ErrorMessage)
        {
            int selectionCount = 0;
            ICursor theSelection;

            IQueryFilter thisQFilter = new QueryFilterClass();
            ResultCount = 0;
            IsSelection = false;

            //get the layer on the map for the specified featureclass
            ILayer mapLayer = GetLayerByFeatureClassName(NPSFCName);
            if (mapLayer == null)
            {
                ErrorMessage = string.Format("No layers on the map from the NPS geodatabase have"
                                             + " a FeatureClass named {0} as their source", NPSFCName);
                return null;
            }

            //check if there is a query filter set - if so set it in the filter
            DefExpress = ((IFeatureLayerDefinition)mapLayer).DefinitionExpression;
            if (string.IsNullOrEmpty(DefExpress) == false)
                thisQFilter.WhereClause = DefExpress;

            //check if we have a selection
            if (((IFeatureSelection)mapLayer).SelectionSet != null)
                selectionCount = (mapLayer as IFeatureSelection).SelectionSet.Count;

            //if we aren't no supposed to return any if no selection then abort now
            if (ReturnAllIfNoSelection == false && selectionCount == 0)
                return null;

            //if no selection, get all the features (filtered by whereclause if a defex was set)
            if (selectionCount == 0)
            {
                if (IsUpdateCursor)
                    theSelection = ((IFeatureLayer)mapLayer).FeatureClass.Update(thisQFilter, false) as ICursor;
                else
                    theSelection = ((IFeatureLayer)mapLayer).FeatureClass.Search(thisQFilter, false) as ICursor;

                ResultCount = ((IFeatureLayer)mapLayer).FeatureClass.FeatureCount(thisQFilter);
            }
                //if there is a selection get only the selection (filtered by whereclause if a defex was set)
            else
            {
                ResultCount = selectionCount;
                IsSelection = true;

                if (IsUpdateCursor)
                    ((ISelectionSet2)(((IFeatureSelection)mapLayer).SelectionSet)).Update(thisQFilter, false,
                                                                                              out theSelection);
                else
                    ((ISelectionSet2)(((IFeatureSelection)mapLayer).SelectionSet)).Search(thisQFilter, false,
                                                                                              out theSelection);
            }

            return theSelection as IFeatureCursor;
        }

        /// <summary>
        ///     get selection of a NPS Layer
        /// </summary>
        public static ISelectionSet2 GetLayerSelection(string NPSFCName, ref string ErrorMessage)
        {
            int selectionCount = 0;


            //get the layer on the map for the specified featureclass
            ILayer mapLayer = GetLayerByFeatureClassName(NPSFCName);
            if (mapLayer == null)
            {
                ErrorMessage = string.Format("No layers on the map from the NPS geodatabase have"
                                             + " a FeatureClass named {0} as their source", NPSFCName);
                return null;
            }


            //check if we have a selection
            if (((IFeatureSelection)mapLayer).SelectionSet != null)
                selectionCount = ((IFeatureSelection)mapLayer).SelectionSet.Count;

            if (selectionCount > 0) return ((IFeatureSelection)mapLayer).SelectionSet as ISelectionSet2;

            return null;
        }

        /// <summary>
        ///     get the selection set from the first non NPS layer found in the map
        /// </summary>
        public static ISelectionSet2 GetFirstNoneNPSSelectionSet(ref esriGeometryType fType,
                                                                 ref string ErrorMessage)
        {
            NPSGlobal nps = NPSGlobal.Instance;
            IMap thisMap = nps.Map;
            fType = esriGeometryType.esriGeometryNull;

            int totalLayers = thisMap.LayerCount;
            for (int indexLayer = 0; indexLayer < totalLayers; indexLayer++)
            {
                ILayer curLayer = thisMap.Layer[indexLayer];

                if (!(curLayer is IFeatureLayer)) continue;

                var thisDs = (curLayer as IFeatureLayer).FeatureClass as IDataset;
                if (thisDs == null) continue;

                if (thisDs.Workspace.PathName == nps.Workspace.PathName) continue;

                if (!(curLayer is IFeatureSelection)) continue;
                var fSelection = curLayer as IFeatureSelection;

                if (fSelection.SelectionSet == null) continue;

                if (fSelection.SelectionSet.Count == 0) continue;

                fType = (curLayer as IFeatureLayer).FeatureClass.ShapeType;
                return fSelection.SelectionSet as ISelectionSet2;
            }

            return null;
        }

        /// <summary>
        ///     get a layer that has the specifed FC as it's source and is part of the NPS workspace
        /// </summary>
        public static ILayer GetLayerByFeatureClassName(string fcName)
        {
            ILayer thisLayer;
            ILayer foundLayer = null;

            IMap thisMap = NPSGlobal.Instance.Map;
            string fcWorkspaceName = NPSGlobal.Instance.DatabasePath;

            //get list of all layers
            IEnumLayer layerList = thisMap.Layers;

            while ((thisLayer = layerList.Next()) != null)
            {
                //check if the current layer is a featurelayer
                if (!(thisLayer is IFeatureLayer)) continue;
                var thisFLayer = (IFeatureLayer) thisLayer;

                //make sure the fc is valid
                if (thisFLayer.FeatureClass == null) continue;

                //if it is a featurelayer, then get the featureclass and workspace names
                var thisDs = (IDataset) thisFLayer.FeatureClass;

                //if the featureclass names are the same and they are from the same workspace, then we found
                //the layer with the specified featureclass name
                if (thisDs.Name == fcName && thisDs.Workspace.PathName == fcWorkspaceName)
                {
                    foundLayer = thisLayer;
                    break;
                }
            }

            return foundLayer;
        }

        /// <summary>
        ///     get a feature class from the specified workspace
        /// </summary>
        public static IFeatureClass GetFeatureClass(String FCName, IWorkspace ThisWorkspace, ref string ErrorMessage)
        {
            IFeatureClass thisFc;

            try
            {
                if (ThisWorkspace == null || !(ThisWorkspace is IFeatureWorkspace))
                {
                    ErrorMessage = "Invalid workspace";
                    return null;
                }


                var thisFtrWs = (IFeatureWorkspace) ThisWorkspace;
                thisFc = thisFtrWs.OpenFeatureClass(FCName);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error why opening FeatureClass " + FCName + ". " + ex.Message;
                return null;
            }

            return thisFc;
        }

        /// <summary>
        ///     get a feature class from the nps workspace
        /// </summary>
        public static IFeatureClass GetFeatureClass(string FCName, ref string ErrorMessage)
        {
            return GetFeatureClass(FCName, NPSGlobal.Instance.Workspace, ref ErrorMessage);
        }

        /// <summary>
        ///     get a table from the specified workspace
        /// </summary>
        public static ITable GetTable(string tableName, IWorkspace thisWorkspace, ref string errorMessage)
        {
            try
            {
                return ((IFeatureWorkspace) thisWorkspace).OpenTable(tableName);
            }
            catch (Exception ex)
            {
                errorMessage = "Error occured while getting table from workspace. " + ex.Message;
                return null;
            }
        }

        public static ITable GetTable(string tableName, string WorkspacePath, ref string errorMessage)
        {
            IWorkspace ThisWorkspace = null;
            return GetTable(tableName, WorkspacePath, ref ThisWorkspace, ref errorMessage);
        }

        public static ITable GetTable(string tableName, string WorkspacePath, ref IWorkspace ThisWorkspace,
                                      ref string ErrorMessage)
        {
            ITable ThisTable = null;
            IFeatureWorkspace ThisFtrWS = null;

            try
            {
                ThisWorkspace = OpenShapeFileWorkspace(WorkspacePath, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

                if (ThisWorkspace != null && (ThisWorkspace is IFeatureWorkspace))
                {
                    ThisFtrWS = (IFeatureWorkspace) ThisWorkspace;
                    ThisTable = ThisFtrWS.OpenTable(tableName);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while getting table " + tableName
                               + " from workspace " + WorkspacePath + ". " + ex.Message;
                return null;
            }

            return ThisTable;
        }

        public static ITable GetTable(string tableName, ref string ErrorMessage)
        {
            return GetTable(tableName, NPSGlobal.Instance.Workspace, ref ErrorMessage);
        }

        /// <summary>
        ///     open a file workspace
        /// </summary>
        public static IWorkspace OpenShapeFileWorkspace(string WorkspaceFolderPath, ref string ErrorMessage)
        {
            IWorkspaceFactory ThisWF;

            ThisWF = new ShapefileWorkspaceFactoryClass();

            try
            {
                return ThisWF.OpenFromFile(WorkspaceFolderPath, 0);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while opening ShapeFile workspace "
                               + WorkspaceFolderPath + ". Make sure this is a path to a ShapeFile.\r\n\r\n" + ex.Message;
                return null;
            }
        }

        /// <summary>
        ///     get a shapefile from a specified path and file name
        /// </summary>
        public static IFeatureClass GetShapeFile(string ShapeFilePathAndName, ref string ErrorMessage)
        {
            string FilePath, FileName;
            IWorkspace SFWorkspace;

            try
            {
                FileName = Path.GetFileName(ShapeFilePathAndName);
                FilePath = Path.GetDirectoryName(ShapeFilePathAndName);
            }
            catch
            {
                ErrorMessage = "The path is  invalid";
                return null;
            }

            SFWorkspace = OpenShapeFileWorkspace(FilePath, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

            return GetFeatureClass(FileName, SFWorkspace, ref ErrorMessage);
        }

        /// <summary>
        ///     convert between types with no fear of exceptions
        /// </summary>
        public static object SafeConvert(object Value, Type ValueType)
        {
            if (ValueType == typeof (double))
            {
                try
                {
                    return Convert.ToDouble(Value);
                }
                catch
                {
                    return -1;
                }
            }
            if (ValueType == typeof (int))
            {
                try
                {
                    return Convert.ToInt32(Value);
                }
                catch
                {
                    return -1;
                }
            }
            if (ValueType == typeof (string))
            {
                try
                {
                    return Convert.ToString(Value);
                }
                catch
                {
                    return "";
                }
            }

            return null;
        }

        /// <summary>
        ///     return next available survey id
        /// </summary>
        public static int NextSurveyID(ref string ErrorMessage)
        {
            NPSGlobal NPS;
            IFeatureClass SurveyBoundaryFC;
            IFeatureCursor ThisFCursor;
            IFeature ThisFeature;
            int SurveyIDIndex;
            int SurveyID, HighestSurveyID = -1;

            NPS = NPSGlobal.Instance;

            SurveyBoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY,
                                               NPS.Workspace, ref ErrorMessage);

            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            SurveyIDIndex = SurveyBoundaryFC.FindField("SurveyID");
            if (SurveyIDIndex == -1)
            {
                ErrorMessage = "Could not find the SurveyID field in the "
                               + NPS.LYR_SURVEY_BOUNDARY + " FeatureClass";
                return -1;
            }

            try
            {
                ThisFCursor = SurveyBoundaryFC.Search(null, false);

                while ((ThisFeature = ThisFCursor.NextFeature()) != null)
                {
                    SurveyID = (int) SafeConvert(ThisFeature.get_Value(SurveyIDIndex), typeof (int));

                    if (SurveyID > HighestSurveyID)
                        HighestSurveyID = SurveyID;
                }

                ThisFCursor = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while attempting to obtain the next available Survey ID. "
                               + ex.Message;
            }

            if (HighestSurveyID < 0) HighestSurveyID = 999;

            return ++HighestSurveyID;
        }

        /// <summary>
        ///     get the highest numeric value in a field
        /// </summary>
        public static int GetHighestFieldValue(string FCName, string NumericFieldName, string WhereClause,
                                               ref string ErrorMessage)
        {
            NPSGlobal NPS;
            IFeatureClass EvalFC;
            IFeatureCursor ThisFCursor;
            IFeature ThisFeature;
            IQueryFilter ThisQueryFilter;
            int FieldIndex;
            int FieldValue, HighestFieldValue = -1;

            NPS = NPSGlobal.Instance;

            EvalFC = GetFeatureClass(FCName, NPS.Workspace, ref ErrorMessage);

            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            FieldIndex = EvalFC.FindField(NumericFieldName);
            if (FieldIndex == -1)
            {
                ErrorMessage = "Could not find the " + NumericFieldName + " field in the "
                               + FCName + " FeatureClass";
                return -1;
            }

            esriFieldType FieldType = EvalFC.Fields.get_Field(EvalFC.FindField(NumericFieldName)).Type;
            if (FieldType != esriFieldType.esriFieldTypeInteger && FieldType != esriFieldType.esriFieldTypeSingle
                && FieldType != esriFieldType.esriFieldTypeSmallInteger && FieldType != esriFieldType.esriFieldTypeOID
                && FieldType != esriFieldType.esriFieldTypeDouble)
            {
                ErrorMessage = "Not a numeric field";
                return -1;
            }

            try
            {
                ThisQueryFilter = new QueryFilterClass();
                ThisQueryFilter.WhereClause = WhereClause;

                ThisFCursor = EvalFC.Search(ThisQueryFilter, false);

                while ((ThisFeature = ThisFCursor.NextFeature()) != null)
                {
                    FieldValue = (int) SafeConvert(ThisFeature.get_Value(FieldIndex), typeof (int));

                    if (FieldValue > HighestFieldValue)
                        HighestFieldValue = FieldValue;
                }

                ThisFCursor = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while attempting to obtain the next available " + NumericFieldName + ". "
                               + ex.Message;
            }

            if (HighestFieldValue == -1) HighestFieldValue = 0;

            return HighestFieldValue;
        }

        /// <summary>
        ///     open a system dialog box to select files
        /// </summary>
        public static string OpenFileDialog(string StartPath)
        {
            DialogResult Results;
            OpenFileDialog ThisFileDialog;

            ThisFileDialog = new OpenFileDialog();

            if (string.IsNullOrEmpty(StartPath)) StartPath = "c:\\";

            ThisFileDialog.InitialDirectory = StartPath;
            ThisFileDialog.Filter = "All files (*.*)|*.*";
            ThisFileDialog.RestoreDirectory = false;
            ThisFileDialog.CheckFileExists = true;
            ThisFileDialog.Multiselect = false;

            Results = ThisFileDialog.ShowDialog();

            if (Results == DialogResult.OK)
            {
                return ThisFileDialog.FileName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        ///     open an ArcMap dialog box to select esri files
        /// </summary>
        public static string OpenESRIDialog(string OldPath, ref bool Cancelled)
        {
            IGxDialog pGXDialog = null;
            IEnumGxObject pEnumGxObject = null;
            IGxObject pGxObject = null;
            bool boolOK;
            object objOldPath;
            string SelFilePathAndName = "";

            objOldPath = OldPath;

            pGXDialog = new GxDialogClass();
            pGXDialog.Title = "Open File";
            pGXDialog.ButtonCaption = "Open";
            pGXDialog.AllowMultiSelect = false;
            pGXDialog.RememberLocation = true;
            pGXDialog.set_StartingLocation(ref objOldPath);

            boolOK = pGXDialog.DoModalOpen(0, out pEnumGxObject);
            if (boolOK)
            {
                pGxObject = pEnumGxObject.Next();
                SelFilePathAndName = pGxObject.FullName;
                Cancelled = false;
            }
            else
            {
                Cancelled = true;
            }

            pEnumGxObject = null;
            pGxObject = null;
            pGXDialog = null;


            return SelFilePathAndName;
        }

        /// <summary>
        ///     get a copy of the default spatial reference used in the NPS geodatabase
        /// </summary>
        public static ISpatialReference GetDefaultSpatialReference()
        {
            NPSGlobal NPS;
            IEnumDataset DatasetList;
            IDataset ThisDataset;
            IClone ThisClone = null;

            NPS = NPSGlobal.Instance;
            DatasetList = NPS.Workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);

            while ((ThisDataset = DatasetList.Next()) != null)
            {
                ThisClone = ((IGeoDataset) ThisDataset).SpatialReference as IClone;
                break;
            }

            if (ThisClone == null) return null;

            return ThisClone.Clone() as ISpatialReference;
        }

        /// <summary>
        ///     determine if two spatial references are the same
        /// </summary>
        public static bool CompareSpatialReference(ISpatialReference SpatRef1, ISpatialReference SpatRef2)
        {
            //clone comparism determines if two objects have the same property
            return ((IClone) SpatRef1).IsEqual(
                SpatRef2 as IClone);
        }

        /// <summary>
        ///     get survey ids and survey names
        /// </summary>
        public static Dictionary<string, int> GetSurveysList()
        {
            Dictionary<string, int> Surveys;
            NPSGlobal NPS;
            IFeature ThisFeature;
            IFeatureCursor ThisFCursor;
            IFeatureClass BoundaryFC;
            string ErrorMessage = "", SurveyName;
            int SurveyIDIndex, SurveyNameIndex, SurveyID;

            NPS = NPSGlobal.Instance;
            Surveys = new Dictionary<string, int>();

            BoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, NPS.Workspace, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return Surveys;

            SurveyIDIndex = BoundaryFC.FindField("SurveyID");
            SurveyNameIndex = BoundaryFC.FindField("SurveyName");

            if (SurveyIDIndex == -1 || SurveyNameIndex == -1) return Surveys;


            ThisFCursor = BoundaryFC.Search(null, false);

            while ((ThisFeature = ThisFCursor.NextFeature()) != null)
            {
                SurveyID = SurveyIDIndex == -1
                               ? -1
                               : (int) SafeConvert(ThisFeature.get_Value(SurveyIDIndex), typeof (int));

                if (SurveyID == -1) continue;

                SurveyName = SurveyNameIndex == -1
                                 ? ""
                                 : (string) SafeConvert(ThisFeature.get_Value(SurveyNameIndex), typeof (string));

                if (string.IsNullOrEmpty(SurveyName.Trim())) SurveyName = Convert.ToString(SurveyID);

                Surveys.Add(SurveyName, SurveyID);
            }

            ThisFCursor = null;

            return Surveys;
        }

        /// <summary>
        ///     get all the unique batch ids in the specified nps layer and survey. if no survey is
        ///     provided, get the unique batch ids for the whole nps layer
        /// </summary>
        public static List<string> GetBatchIDs(string NPSFCName, string SurveyID, ref string ErrorMessage)
        {
            IFeatureClass ThisFeatureClass;
            NPSGlobal NPS;
            List<string> BatchIDs;
            string WhereClause;

            NPS = NPSGlobal.Instance;
            BatchIDs = new List<string>();
            WhereClause = string.IsNullOrEmpty(SurveyID) ? "" : "SurveyID=" + SurveyID;

            ThisFeatureClass = GetFeatureClass(NPSFCName, NPS.Workspace, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return BatchIDs;


            return GetUniqueValues(ThisFeatureClass, "BATCH_ID", WhereClause, ref ErrorMessage);
        }

        /// <summary>
        ///     get the unique values from a specified field in a specified featureclass using the
        ///     specified filter
        /// </summary>
        public static List<string> GetUniqueValues(IFeatureClass ThisFC,
                                                   string FieldName, string WhereClause, ref string ErrorMessage)
        {
            IDataStatistics ThisDataStatistics;
            IEnumerator ValueList;
            IFeatureCursor ThisFCursor;
            IQueryFilter ThisQueryFilter;
            List<string> UniqueValues;
            string CurrentValue;

            UniqueValues = new List<string>();

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = WhereClause;

            try
            {
                ThisFCursor = ThisFC.Search(ThisQueryFilter, false);

                ThisDataStatistics = new DataStatisticsClass();
                ThisDataStatistics.Field = FieldName;
                ThisDataStatistics.Cursor = ThisFCursor as ICursor;

                ValueList = ThisDataStatistics.UniqueValues;

                while (ValueList.MoveNext())
                {
                    CurrentValue = (string) SafeConvert(ValueList.Current, typeof (string));
                    if (string.IsNullOrEmpty(CurrentValue)) continue;

                    UniqueValues.Add(CurrentValue);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while obtaining unique values. " + ex.Message;
            }

            return UniqueValues;
        }

        /// <summary>
        ///     load/save all controls with their saved values. controls are mapped to saved values via
        ///     their tag property. the tag property contains the name of the saved option they are bounded to.
        /// </summary>
        public static void ManageSavedValues(Control ParentControl,
                                             SavedValuesAction Action)
        {
            int TotalControls;
            Control ThisControl;
            string SavedValue, OptionName;

            TotalControls = ParentControl.Controls.Count;
            for (int Index = 0; Index < TotalControls; Index++)
            {
                ThisControl = ParentControl.Controls[Index];
                SavedValue = "";
                OptionName = (string) SafeConvert(ThisControl.Tag, typeof (string));

                if (string.IsNullOrEmpty(OptionName) == false)
                {
                    if (Action == SavedValuesAction.Load)
                    {
                        SavedValue = GetConfigSetting(OptionName, "StoredOption");

                        if (ThisControl is TextBox)
                            (ThisControl as TextBox).Text = SavedValue;

                        if (ThisControl is CheckBox)
                            (ThisControl as CheckBox).Checked = SavedValue == "Y" ? true : false;

                        if (ThisControl is ComboBox)
                            (ThisControl as ComboBox).Text = SavedValue;

                        if (ThisControl is Label)
                            (ThisControl as Label).Text = SavedValue;
                    }

                    if (Action == SavedValuesAction.Save)
                    {
                        if (ThisControl is TextBox)
                            SavedValue = (ThisControl as TextBox).Text;

                        if (ThisControl is CheckBox)
                            SavedValue = (ThisControl as CheckBox).Checked ? "Y" : "N";

                        if (ThisControl is ComboBox)
                            SavedValue = (ThisControl as ComboBox).Text;

                        if (ThisControl is Label)
                            SavedValue = (ThisControl as Label).Text;

                        SetConfigSetting(OptionName, SavedValue, "StoredOption");
                    }
                }


                if (ThisControl.Controls.Count > 0)
                    ManageSavedValues(ThisControl, Action);
            }
        }

        /// <summary>
        ///     get a raster dataset from the specified folder and file name
        /// </summary>
        public static IRasterDataset OpenRasterDataset(string RasterPathAndName, ref string ErrorMessage)
        {
            IWorkspace ThisWorkspace;
            string FilePath, FileName;

            try
            {
                FileName = Path.GetFileName(RasterPathAndName);
                FilePath = Path.GetDirectoryName(RasterPathAndName);
            }
            catch
            {
                ErrorMessage = "Invalid path specified";
                return null;
            }


            ThisWorkspace = OpenRasterWorkspace(FilePath, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

            try
            {
                return ((IRasterWorkspace) ThisWorkspace).OpenRasterDataset(FileName);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error while getting raster at path "
                               + RasterPathAndName + ". " + ex.Message;
            }

            return null;
        }

        /// <summary>
        ///     open a raster workspace at the specified folder path
        /// </summary>
        public static IWorkspace OpenRasterWorkspace(string WorkspacePath, ref string ErrorMessage)
        {
            IWorkspaceFactory ThisWorkspaceFactory;

            try
            {
                ThisWorkspaceFactory = new RasterWorkspaceFactoryClass();
                return ThisWorkspaceFactory.OpenFromFile(WorkspacePath, 0);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while opening raster workspace at "
                               + WorkspacePath + ". " + ex.Message;
            }

            return null;
        }

        /// <summary>
        ///     get she polygon shape representing a survey area
        /// </summary>
        public static IPolygon GetSurveyBoundary(string SurveyID, ref string ErrorMessage)
        {
            IFeature ThisFeature;
            IFeatureCursor ThisFCursor;
            IFeatureClass BoundaryFC;
            IQueryFilter ThisQueryFilter;
            NPSGlobal NPS;
            IPolygon BoundaryPoly = null;

            NPS = NPSGlobal.Instance;

            BoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

            try
            {
                ThisQueryFilter = new QueryFilterClass();
                ThisQueryFilter.WhereClause = "SurveyID=" + SurveyID;

                ThisFCursor = BoundaryFC.Search(ThisQueryFilter, false);

                ThisFeature = ThisFCursor.NextFeature();
                BoundaryPoly = ThisFeature.ShapeCopy as IPolygon;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while obtaining Survey Boundary shape. " + ex.Message;
            }


            return BoundaryPoly;
        }

        /// <summary>
        ///     get a clip of a larger raster using the specified survey boundary as the cut out shape
        /// </summary>
        public static IGeoDataset ClipRasterByBndPoly(IGeoDataset RasterToClip, string SurveyID, ref string ErrorMessage)
        {
            IPolygon SurveyBoundary, SurveyEnvelopePoly, DEMEnvelopePoly;
            IExtractionOp ExtractOp;
            IGeoDataset ClippedRaster = null;

            SurveyBoundary = GetSurveyBoundary(SurveyID, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

            SurveyEnvelopePoly = EnvelopeToPolygon(SurveyBoundary.Envelope);
            DEMEnvelopePoly = EnvelopeToPolygon(RasterToClip.Extent);

            if (((IRelationalOperator) DEMEnvelopePoly).Contains(SurveyEnvelopePoly) == false)
            {
                ErrorMessage = "The boundary polygon for SurveyID " + SurveyID
                               + " is out of range of the Raster extent.";
                return null;
            }

            try
            {
                ExtractOp = new RasterExtractionOpClass();
                ClippedRaster = ExtractOp.Polygon(RasterToClip, SurveyEnvelopePoly, true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while clipping raster to boundary of survey "
                               + SurveyID + ". " + ex.Message;
            }


            return ClippedRaster;
        }

        /// <summary>
        ///     convert an evelope to a polygon
        /// </summary>
        public static IPolygon EnvelopeToPolygon(IEnvelope ThisEnvelope)
        {
            IPolygon EnvelopePoly;
            object emtpy = Missing.Value;
            IPoint EnveCorner;

            EnvelopePoly = new PolygonClass();

            EnveCorner = ((IClone) ThisEnvelope.Envelope.UpperLeft).Clone() as IPoint;
            ((IPointCollection) EnvelopePoly).AddPoint(EnveCorner, ref emtpy, ref emtpy);

            EnveCorner = ((IClone) ThisEnvelope.Envelope.UpperRight).Clone() as IPoint;
            ((IPointCollection) EnvelopePoly).AddPoint(EnveCorner, ref emtpy, ref emtpy);

            EnveCorner = ((IClone) ThisEnvelope.Envelope.LowerRight).Clone() as IPoint;
            ((IPointCollection) EnvelopePoly).AddPoint(EnveCorner, ref emtpy, ref emtpy);

            EnveCorner = ((IClone) ThisEnvelope.Envelope.LowerLeft).Clone() as IPoint;
            ((IPointCollection) EnvelopePoly).AddPoint(EnveCorner, ref emtpy, ref emtpy);

            EnvelopePoly.Close();

            return EnvelopePoly;
        }

        /// <summary>
        ///     delete features from a nps featureclass with the specified filter
        /// </summary>
        public static void DeleteFeatures(string NPSFCName, string WhereClause, ref string ErrorMessage)
        {
            ITable ThisTable;
            IQueryFilter ThisQueryFilter;

            ThisTable = GetFeatureClass(NPSFCName, ref ErrorMessage) as ITable;
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = WhereClause;

            try
            {
                ThisTable.DeleteSearchedRows(ThisQueryFilter);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while deleting feature in " + NPSFCName + ". " + ex.Message;
            }
        }

        /// <summary>
        ///     delete feature through cursor from specified featureclass
        /// </summary>
        public static void DeleteFeatures(IFeatureClass ThisFeatureClass, string WhereClause, ref string ErrorMessage)
        {
            IQueryFilter ThisQueryFilter;
            IFeature ThisFeature;
            IFeatureCursor ThisFCursor;

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = WhereClause;


            try
            {
                ThisFCursor = ThisFeatureClass.Update(ThisQueryFilter, false);

                while ((ThisFeature = ThisFCursor.NextFeature()) != null)
                {
                    ThisFeature.Delete();
                }

                ThisFCursor = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while deleting features in "
                               + ((IDataset) ThisFeatureClass).Name + ". " + ex.Message;
            }
        }

        /// <summary>
        ///     copy features from one feature class to the other. if feature classes have two fields with the same name and type
        ///     the data will be copied over
        /// </summary>
        public static int CopyFeatures(IFeatureClass FromFC, string FromWhereClause, IFeatureClass ToFC,
                                       List<string> FieldsToUpdate, ref string ErrorMessage)
        {
            IQueryFilter ThisQueryFilter;
            IFeatureCursor ReadCursor;
            int CopyCount;

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = FromWhereClause;
            ReadCursor = FromFC.Search(ThisQueryFilter, false);

            CopyCount = CopyFeatures(ReadCursor, ToFC, FieldsToUpdate, ref ErrorMessage);

            ReadCursor = null;

            return CopyCount;
        }

        /// <summary>
        ///     copy features from a cursor to a featureclass.
        /// </summary>
        public static int CopyFeatures(IFeatureCursor FromFCursor, IFeatureClass ToFC,
                                       List<string> FieldsToUpdate, ref string ErrorMessage)
        {
            IFeatureCursor InsertCursor;
            IFeature ReadFeature;
            IFeatureBuffer InsertBuffer;
            int ReadFieldIndex, ReadFieldCount, InsertFieldIndex, CopyTotal;
            string ReadFieldName;


            CopyTotal = 0;
            InsertCursor = ToFC.Insert(true);
            InsertBuffer = ToFC.CreateFeatureBuffer();


            while ((ReadFeature = FromFCursor.NextFeature()) != null)
            {
                if (ReadFeature.Shape == null) continue;

                InsertBuffer.Shape = ReadFeature.ShapeCopy;

                if (FieldsToUpdate == null)
                {
                    ReadFieldCount = ReadFeature.Fields.FieldCount;

                    for (ReadFieldIndex = 0; ReadFieldIndex < ReadFieldCount; ReadFieldIndex++)
                    {
                        ReadFieldName = ReadFeature.Fields.get_Field(ReadFieldIndex).Name;

                        InsertFieldIndex = ToFC.FindField(ReadFieldName);

                        if (InsertFieldIndex != -1 && ReadFieldName.ToUpper() != "SHAPE_AREA"
                            && ReadFieldName.ToUpper() != "SHAPE_LENGTH"
                            && ReadFieldName.ToUpper() != "OBJECTID"
                            && ReadFieldName.ToUpper() != "OBJECTID_1"
                            && ReadFieldName.ToUpper() != "SHAPE"
                            && ReadFeature.Fields.get_Field(ReadFieldIndex).Type != esriFieldType.esriFieldTypeOID
                            && ReadFeature.Fields.get_Field(ReadFieldIndex).Type != esriFieldType.esriFieldTypeGeometry)
                        {
                            if (ReadFeature.Fields.get_Field(ReadFieldIndex).Type ==
                                ToFC.Fields.get_Field(InsertFieldIndex).Type)
                                InsertBuffer.set_Value(InsertFieldIndex, ReadFeature.get_Value(ReadFieldIndex));
                        }
                    }
                }
                else
                {
                    foreach (string FieldName in FieldsToUpdate)
                    {
                        ReadFieldIndex = ReadFeature.Fields.FindField(FieldName);
                        InsertFieldIndex = ToFC.FindField(FieldName);

                        if (ReadFieldIndex != -1 && InsertFieldIndex != -1)
                            if (ReadFeature.Fields.get_Field(ReadFieldIndex).Type ==
                                ToFC.Fields.get_Field(InsertFieldIndex).Type)
                                InsertBuffer.set_Value(InsertFieldIndex, ReadFeature.get_Value(ReadFieldIndex));
                    }
                }

                CopyTotal++;
                InsertCursor.InsertFeature(InsertBuffer);
            }

            FromFCursor = null;
            InsertCursor = null;

            return CopyTotal;
        }

        /// <summary>
        ///     update a single transect feature with the matching feature in the specified feature class
        /// </summary>
        public static void UpdateTransect(string TransectID, string SurveyID, IFeatureClass FromFC,
                                          ref string ErrorMessage)
        {
            IQueryFilter ThisQueryFilter;
            IFeatureCursor TransectCursor, FromCursor;
            IFeature TransectFeature;
            IFeatureClass TrasectFC;
            IFeature FromFeature;
            int ReadFieldIndex, InsertFieldIndex;
            string FromWhereClause;
            List<string> FieldsToUpdate;
            NPSGlobal NPS;

            NPS = NPSGlobal.Instance;

            FromWhereClause = "TransectID=" + TransectID + " AND SurveyID=" + SurveyID;


            //get transect feature and cursor
            TrasectFC = GetFeatureClass(NPS.LYR_GENERATED_TRANSECTS, ref ErrorMessage);
            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = FromWhereClause;
            TransectCursor = TrasectFC.Update(ThisQueryFilter, false);
            TransectFeature = TransectCursor.NextFeature();
            if (TransectFeature == null)
            {
                TransectCursor = null;
                return;
            }

            //get source transect
            ThisQueryFilter = null;
            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = FromWhereClause;
            FromCursor = FromFC.Search(ThisQueryFilter, false);
            FromFeature = FromCursor.NextFeature();
            if (FromFeature == null)
            {
                FromCursor = null;
                TransectCursor = null;
                return;
            }

            FieldsToUpdate = new List<string>
                {
                    "PILOTLNAM",
                    "OBSLNAM1",
                    "OBSLNAM2",
                    "FLOWNDATE",
                    "WEATHER",
                    "FLOWN",
                    "TURBDUR",
                    "TURBINT",
                    "PRECIP",
                    "CLOUDCOVER",
                    "TEMPRTURE",
                    "AIRCRAFT"
                };

            foreach (string FieldName in FieldsToUpdate)
            {
                ReadFieldIndex = FromFC.FindField(FieldName);
                InsertFieldIndex = TrasectFC.FindField(FieldName);

                if (ReadFieldIndex != -1 && InsertFieldIndex != -1)
                    if (FromFC.Fields.get_Field(ReadFieldIndex).Type ==
                        TrasectFC.Fields.get_Field(InsertFieldIndex).Type)
                        TransectFeature.set_Value(InsertFieldIndex, FromFeature.get_Value(ReadFieldIndex));
            }

            TransectCursor.UpdateFeature(TransectFeature);

            TransectCursor = null;
            FromCursor = null;
        }

        /// <summary>
        ///     delete a layer form the map
        /// </summary>
        public static bool DeleteLayerFromMap(string LayerName)
        {
            NPSGlobal NPS;
            IMap ThisMap;
            int LayerTotal;
            IFeatureLayer ThisFLayer;
            bool Deleted = false;


            NPS = NPSGlobal.Instance;
            ThisMap = NPS.Map;

            LayerTotal = ThisMap.LayerCount;
            for (int LayerIndex = 0; LayerIndex < LayerTotal; LayerIndex++)
            {
                if (!(ThisMap.get_Layer(LayerIndex) is IFeatureLayer)) continue;

                ThisFLayer = ThisMap.get_Layer(LayerIndex) as IFeatureLayer;
                if (ThisFLayer.Name.ToLower() == LayerName.ToLower())
                {
                    ThisMap.DeleteLayer(ThisFLayer);
                    Deleted = true;
                    break;
                }
            }

            return Deleted;
        }

        /// <summary>
        ///     delete a featureclass that is within the NPS geodatabase
        /// </summary>
        public static void DeleteDataset(string FCName, esriDatasetType DatasetType, ref string ErrorMessage)
        {
            DeleteDataset(FCName, DatasetType, NPSGlobal.Instance.Workspace, ref ErrorMessage);
        }

        /// <summary>
        ///     delete a featureclass that is within the specified workspace
        /// </summary>
        public static void DeleteDataset(string DSName, esriDatasetType DatasetType,
                                         IWorkspace ThisWorkspace, ref string ErrorMessage)
        {
            string InternalMessage = "";
            IDataset ThisDataset = null;

            try
            {
                switch (DatasetType)
                {
                    case esriDatasetType.esriDTFeatureClass:
                        ThisDataset = GetFeatureClass(DSName, ThisWorkspace, ref InternalMessage) as IDataset;
                        break;
                    case esriDatasetType.esriDTTable:
                        ThisDataset = GetTable(DSName, ThisWorkspace, ref InternalMessage) as IDataset;
                        break;
                }

                if (string.IsNullOrEmpty(InternalMessage) == false) return;

                ThisDataset.Delete();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while deleting dataset " + DSName + ". " + ex.Message;
            }
        }

        /// <summary>
        ///     generate exlcuded area polygons for the specified survey and elevation
        /// </summary>
        public static void GenerateExcludedAreasPolygons(int SurveyID, int MaximumElevInFeet, bool IsAboveElevation,
                                                         IGeoDataset ClippedRasterDS, ref string ErrorMessage)
        {
            IQueryFilter ThisQueryFilter;
            IRasterDescriptor ThisRasterDescriptor;
            IGeoProcessor ThisGeoProcessor = null;
            IVariantArray GPParams;
            IFeatureClass NewElvPolyFC, BoundaryFC, ElvPolyFC;
            IConversionOp ThisConversionOp;
            string TempElvFCName, TempAggFCName, InternalErrors = "", TempClipFCName;
            IFeatureBuffer InsertBuffer;
            IFeatureCursor InsertCursor, TempCursor;
            IFeature TempFeature;
            int ShapeAreaIndex;
            double CurrentShapeArea, MaxShapeArea;
            int SurveyIDIndex;
            NPSGlobal NPS;

            NPS = NPSGlobal.Instance;
            TempElvFCName = "NPS_TEMP_TempElvPolyFCName";
            TempAggFCName = "NPS_TEMP_TempElevationFC";
            TempClipFCName = "NPS_TEMP_TempClipFCName";


            ElvPolyFC = GetFeatureClass(NPS.LYR_EXCLUDED_AREAS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            BoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            SurveyIDIndex = ElvPolyFC.FindField("SurveyID");
            if (SurveyIDIndex == -1)
            {
                ErrorMessage = "Could not find the SurveyID field on the Excluded Areas FeatureClass.";
                return;
            }

            SetProgressMessage("Deleting old temp files");
            DeleteDataset(TempElvFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);
            DeleteDataset(TempAggFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);


            try
            {
                SetProgressMessage("Building excluded area polygons from DEM");

                //determine which values to exclude
                ThisQueryFilter = new QueryFilterClass();

                if (IsAboveElevation)
                    ThisQueryFilter.WhereClause = " Value >= " + MaximumElevInFeet;
                else
                    ThisQueryFilter.WhereClause = " Value <= " + MaximumElevInFeet;

                ThisRasterDescriptor = new RasterDescriptorClass();
                ThisRasterDescriptor.Create(ClippedRasterDS as IRaster, ThisQueryFilter, "Value");

                ThisConversionOp = new RasterConversionOpClass();
                NewElvPolyFC = ThisConversionOp.RasterDataToPolygonFeatureData(ThisRasterDescriptor as IGeoDataset,
                                                                               NPS.Workspace, TempElvFCName, true) as
                               IFeatureClass;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while generating a FeatureClass of excluded "
                               + "areas from the default DEM file. " + ex.Message;
                return;
            }


            try
            {
                SetProgressMessage("Aggregating excluded area polygons");

                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                GPParams.Add(Path.Combine(NPS.DatabasePath, TempElvFCName));
                GPParams.Add(Path.Combine(NPS.DatabasePath, TempAggFCName));
                GPParams.Add("1 Meters");

                ThisGeoProcessor.Execute("AggregatePolygons_management", GPParams, null);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:AggregatePolygons_management\r\n"
                                             + "Params:{0},{1},{2}\r\nException:{3}",
                                             Path.Combine(NPS.DatabasePath, TempElvFCName),
                                             Path.Combine(NPS.DatabasePath, TempAggFCName), "1 Meters", ex.Message);
                ThisGeoProcessor = null;
                return;
            }

            ThisGeoProcessor = null;

            DeleteLayerFromMap(TempAggFCName);

            NewElvPolyFC = GetFeatureClass(TempAggFCName, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                ErrorMessage = "Could not find temporary FeatureClass " + TempAggFCName + ". " + ErrorMessage;
                return;
            }

            SetProgressMessage("Validating excluded area polygons");

            NewElvPolyFC = GP_Clip_analysis(TempClipFCName, NewElvPolyFC, BoundaryFC, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;


            InsertBuffer = ElvPolyFC.CreateFeatureBuffer();
            InsertCursor = ElvPolyFC.Insert(true);

            ThisQueryFilter = null;
            ThisQueryFilter = new QueryFilterClass();
            TempCursor = NewElvPolyFC.Search(ThisQueryFilter, false);


            ShapeAreaIndex = NewElvPolyFC.FindField(NewElvPolyFC.AreaField.Name);

            MaxShapeArea = (double) SafeConvert(GetFirstRecordValue(BoundaryFC,
                                                                    BoundaryFC.AreaField.Name, "SurveyID=" + SurveyID),
                                                typeof (double));

            SetProgressMessage("Importing excluded area polygons to Survey");

            while ((TempFeature = TempCursor.NextFeature()) != null)
            {
                if (TempFeature.Shape == null) continue;

                CurrentShapeArea = (double) SafeConvert(TempFeature.get_Value(ShapeAreaIndex), typeof (double));
                if ((CurrentShapeArea/MaxShapeArea) >= 0.95) continue;


                InsertBuffer.Shape = TempFeature.ShapeCopy;
                InsertBuffer.set_Value(SurveyIDIndex, SurveyID);

                InsertCursor.InsertFeature(InsertBuffer);
            }
            
            InsertCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TempCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(InsertCursor);


            SetProgressMessage("Cleaning up temp files");
            DeleteDataset(TempElvFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);
            DeleteDataset(TempAggFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);


            CreateFillerPolygon(ElvPolyFC, SurveyID, ref InternalErrors);
        }

        /// <summary>
        ///     generate flatarea poylgons for the specified survey using the specified raster
        /// </summary>
        public static void GenerateFlatAreaPolygons(int SurveyID, double MinSlope, double MaxSlope,
                                                    IGeoDataset ClippedRasterDS, ref string ErrorMessage)
        {
            IGeoProcessor ThisGeoProcessor = null;
            IRasterDescriptor ThisRasterDescriptor;
            IConversionOp ThisConversionOp;
            IGeoDataset FloatRasterDS = null, IntRasterDS = null;
            IVariantArray GPParams;
            ISurfaceOp ThisSurfaceOp;
            IMathOp ThisMathOp;
            IFeatureClass TempAggregateFC = null,
                          TempAggregateClipFC = null,
                          BoundaryFC = null,
                          FlatAreasFC = null;
            IQueryFilter ThisQueryFilter;
            IFeatureCursor FlatAreasCursor, TempAggregateClipCursor;
            IFeatureBuffer FlatAreasFBuffer;
            IFeature TempAggregateClipFeature;
            string TempFlatAreasPolyFCName, TempAggregateFCName, InternalErrors = "", TempClipFCName;
            object zFactor;
            NPSGlobal NPS;
            int SurveyIDIndex;

            TempClipFCName = "NPS_TEMP_ClippedFC";
            TempAggregateFCName = "NPS_TEMP_FlatAreasAggrFC";
            TempFlatAreasPolyFCName = "NPS_TEMP_FlatAreasFC";
            NPS = NPSGlobal.Instance;
            zFactor = 1;


            BoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            FlatAreasFC = GetFeatureClass(NPS.LYR_FLAT_AREAS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            SurveyIDIndex = FlatAreasFC.FindField("SurveyID");
            if (SurveyIDIndex == -1)
            {
                ErrorMessage = "Could not find a SurveyID field on the " + ((IDataset) FlatAreasFC).Name +
                               " FeatureClass.";
                return;
            }


            SetProgressMessage("Deleting old temp files");
            DeleteDataset(TempFlatAreasPolyFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggregateFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggregateFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);
            DeleteDataset(TempClipFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);

            ThisSurfaceOp = new RasterSurfaceOpClass();

            try
            {
                SetProgressMessage("Computing slope values from DEM");

                FloatRasterDS = ThisSurfaceOp.Slope(ClippedRasterDS,
                                                    esriGeoAnalysisSlopeEnum.esriGeoAnalysisSlopeDegrees, ref zFactor);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while converting DEM evelation values to float slope values. " +
                               ex.Message;
                return;
            }

            try
            {
                SetProgressMessage("Converting DEM values to integers");

                ThisMathOp = new RasterMathOpsClass();
                IntRasterDS = ThisMathOp.Int(FloatRasterDS);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while converting DEM float slope values to slope integer values. " +
                               ex.Message;
                return;
            }

            try
            {
                SetProgressMessage("Building flat area polygons from DEM");

                //10.1 uses deferred evaluation of rasters (like that generated by the slope above)
                //It appears that the IRasterDescriptor.Create() is haveing trouble with this.
                // Forces the evaluation by querying the Height property.
                var rasterProps = (IRasterProps)IntRasterDS;
                Int32 int32_height = rasterProps.Height;

                ThisQueryFilter = new QueryFilterClass();
                ThisQueryFilter.WhereClause = " Value >= " + MinSlope + " AND Value <= " + MaxSlope;

                ThisRasterDescriptor = new RasterDescriptorClass();
                ThisRasterDescriptor.Create(IntRasterDS as IRaster, ThisQueryFilter, "Value");

                ThisConversionOp = new RasterConversionOpClass();
                var unused = ThisConversionOp.RasterDataToPolygonFeatureData(ThisRasterDescriptor as IGeoDataset,
                                                                NPS.Workspace, TempFlatAreasPolyFCName, true) as IFeatureClass;
                if (unused == null)
                    return;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while generating a FeatureClass of flat "
                               + "areas from the default DEM file. " + ex.Message;
                return;
            }


            try
            {
                SetProgressMessage("Aggregating flat area polygons");

                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                GPParams.Add(Path.Combine(NPS.DatabasePath, TempFlatAreasPolyFCName));
                GPParams.Add(Path.Combine(NPS.DatabasePath, TempAggregateFCName));
                GPParams.Add("1 Meters");

                ThisGeoProcessor.Execute("AggregatePolygons_management", GPParams, null);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:AggregatePolygons_management\r\n"
                                             + "Params:{0},{1},{2}\r\nException:{3}",
                                             Path.Combine(NPS.DatabasePath, TempFlatAreasPolyFCName),
                                             Path.Combine(NPS.DatabasePath, TempAggregateFCName), "1 Meters", ex.Message);
                ThisGeoProcessor = null;
                return;
            }

            ThisGeoProcessor = null;

            DeleteLayerFromMap(TempAggregateFCName);

            TempAggregateFC = GetFeatureClass(TempAggregateFCName, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                ErrorMessage = "Could not find temporary FeatureClass " + TempAggregateFCName + ". " + ErrorMessage;
                return;
            }

            SetProgressMessage("Validating flat area polygons");

            TempAggregateClipFC = GP_Clip_analysis(TempClipFCName, TempAggregateFC, BoundaryFC, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;


            FlatAreasFBuffer = FlatAreasFC.CreateFeatureBuffer();
            FlatAreasCursor = FlatAreasFC.Insert(true);

            ThisQueryFilter = null;
            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = "SHAPE_AREA > 25000";
            TempAggregateClipCursor = TempAggregateClipFC.Search(ThisQueryFilter, false);


            SetProgressMessage("Importing flat area polygons to Survey");

            while ((TempAggregateClipFeature = TempAggregateClipCursor.NextFeature()) != null)
            {
                if (TempAggregateClipFeature.Shape == null) continue;

                FlatAreasFBuffer.Shape = TempAggregateClipFeature.ShapeCopy;
                FlatAreasFBuffer.set_Value(SurveyIDIndex, SurveyID);

                FlatAreasCursor.InsertFeature(FlatAreasFBuffer);
            }

            FlatAreasCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(TempAggregateClipCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(FlatAreasCursor);


            SetProgressMessage("Cleaning up temp files");
            DeleteDataset(TempFlatAreasPolyFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggregateFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempClipFCName, esriDatasetType.esriDTFeatureClass, ref InternalErrors);
            DeleteDataset(TempAggregateFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);
            DeleteDataset(TempClipFCName + "_Tbl", esriDatasetType.esriDTTable, ref InternalErrors);


            CreateFillerPolygon(FlatAreasFC, SurveyID, ref InternalErrors);
        }

        /// <summary>
        ///     generate random points within survey boundary
        /// </summary>
        public static int GenerateRandomPoints(int SurveyID, int TotalPoints, ref string ErrorMessage)
        {
            int SurveyIDFieldIndex, BatchIDFieldIndex, NextBatcID, PointIndex;
            IFeatureClass RandPointsFC, ExcludeFC;
            IFeatureBuffer InsertBuffer;
            IFeatureCursor InsertCursor;
            NPSGlobal NPS;
            IRelationalOperator ThisRelOp;
            IPolygon BoundaryPoly;
            IPoint NewPoint;
            double UpperBoundX, LowerBoundX, UpperBoundY, LowerBoundY, ValueX, ValueY;

            NPS = NPSGlobal.Instance;

            BoundaryPoly = GetSurveyBoundary(Convert.ToString(SurveyID), ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            ExcludeFC = GetFeatureClass(NPS.LYR_EXCLUDED_AREAS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            RandPointsFC = GetFeatureClass(NPS.LYR_RANDOMPOINTS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            SurveyIDFieldIndex = RandPointsFC.FindField("SurveyID");
            BatchIDFieldIndex = RandPointsFC.FindField("BATCH_ID");

            InsertCursor = RandPointsFC.Insert(true);
            InsertBuffer = RandPointsFC.CreateFeatureBuffer();

            NextBatcID = GetHighestFieldValue(NPS.LYR_RANDOMPOINTS,
                                              "BATCH_ID", "SurveyID=" + SurveyID, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;
            NextBatcID++;

            UpperBoundX = BoundaryPoly.Envelope.XMax;
            LowerBoundX = BoundaryPoly.Envelope.XMin;
            UpperBoundY = BoundaryPoly.Envelope.YMax;
            LowerBoundY = BoundaryPoly.Envelope.YMin;

            ThisRelOp = BoundaryPoly as IRelationalOperator;

            PointIndex = 0;
            while (PointIndex < TotalPoints)
            {
                NewPoint = new PointClass();
                NewPoint.SpatialReference = GetDefaultSpatialReference();

                ValueX = ((UpperBoundX - LowerBoundX)*GetRandomNumber()) + LowerBoundX;
                ValueY = ((UpperBoundY - LowerBoundY)*GetRandomNumber()) + LowerBoundY;
                NewPoint.PutCoords(ValueX, ValueY);

                if (ThisRelOp.Contains(NewPoint) == false) continue;

                if (HasRelationshipWithFC(NewPoint, esriSpatialRelEnum.esriSpatialRelWithin, ExcludeFC,
                                          "SurveyID=" + SurveyID))
                    continue;

                InsertBuffer.Shape = ((IClone) NewPoint).Clone() as IPoint;
                InsertBuffer.set_Value(SurveyIDFieldIndex, SurveyID);
                InsertBuffer.set_Value(BatchIDFieldIndex, NextBatcID);

                PointIndex++;

                InsertCursor.InsertFeature(InsertBuffer);
            }

            InsertCursor = null;
            return NextBatcID;
        }

        /// <summary>
        ///     generate transects for spicified survey
        /// </summary>
        public static void GenerateTransectLines(int SurveyID, double MaxLineLength, double MinLineLength,
                                                 IGeoDataset ClippedRasterDS, int TransCreateAttempts, int TransectTotal,
                                                 double TargetLength,
                                                 ref int ThisBatchID, ref int StraightCount, ref int ContourCount,
                                                 ref int FailCount, ref string ErorrMessage)
        {
            NPSGlobal NPS;
            IFeatureClass RandPointsFC, TransectFC, BoundaryFC, ExcludedFC, FlatAreasFC;
            IFeature RandPntFeature;
            IFeatureCursor RandPntCursor;
            IFeatureCursor InsertCursor;
            IFeatureBuffer InsertBuffer;
            IQueryFilter ThisQueryFilter;
            int TransectIDFieldIndex,
                CorruptCount = 0,
                TransectCount = 0,
                ElevValFieldIndex,
                NewBatchID,
                NewTransectID,
                FlownFieldIndex,
                AcceptedFieldIndex,
                PT_IDFieldIndex,
                SurveyIDFieldIndex,
                BatchIDFieldIndex,
                TargetLengthFieldIndex,
                TrnsectElevValFieldIndex,
                TrnsectElevValFTFieldIndex,
                ElevValFTFieldIndex;
            IPolyline NewTrnPolyline;
            IPoint TempPoint;
            string TransType = "", ErrorMessage = "";

            NPS = NPSGlobal.Instance;

            //get all necessary feature classes
            RandPointsFC = GetFeatureClass(NPS.LYR_RANDOMPOINTS, ref ErorrMessage);
            if (string.IsNullOrEmpty(ErorrMessage) == false) return;

            TransectFC = GetFeatureClass(NPS.LYR_GENERATED_TRANSECTS, ref ErorrMessage);
            if (string.IsNullOrEmpty(ErorrMessage) == false) return;

            BoundaryFC = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, ref ErorrMessage);
            if (string.IsNullOrEmpty(ErorrMessage) == false) return;

            ExcludedFC = GetFeatureClass(NPS.LYR_EXCLUDED_AREAS, ref ErorrMessage);
            if (string.IsNullOrEmpty(ErorrMessage) == false) return;

            FlatAreasFC = GetFeatureClass(NPS.LYR_FLAT_AREAS, ref ErorrMessage);
            if (string.IsNullOrEmpty(ErorrMessage) == false) return;


            GetDatasetUnits(ClippedRasterDS, ref ErorrMessage);


            //get next available batch id
            NewBatchID = GetHighestFieldValue(NPS.LYR_GENERATED_TRANSECTS,
                                              "BATCH_ID", "SurveyID=" + SurveyID, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;
            NewBatchID++;

            ThisBatchID = NewBatchID;

            //get next available transect id
            NewTransectID = GetHighestFieldValue(NPS.LYR_GENERATED_TRANSECTS,
                                                 "TransectID", "SurveyID=" + SurveyID, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;
            NewTransectID++;

            //field indexes on transect fc
            TransectIDFieldIndex = TransectFC.FindField("TransectID");
            FlownFieldIndex = TransectFC.FindField("Flown");
            PT_IDFieldIndex = TransectFC.FindField("PT_ID");
            SurveyIDFieldIndex = TransectFC.FindField("SurveyID");
            BatchIDFieldIndex = TransectFC.FindField("BATCH_ID");
            TargetLengthFieldIndex = TransectFC.FindField("TARGETLEN");
            TrnsectElevValFieldIndex = TransectFC.FindField("ELEV_M");
            TrnsectElevValFTFieldIndex = TransectFC.FindField("ELEVFT");

            //rnd point indexes
            AcceptedFieldIndex = RandPointsFC.FindField("HASTRANS");
            RandPointsFC.FindField(RandPointsFC.OIDFieldName);
            ElevValFieldIndex = RandPointsFC.FindField("ELEV_M");
            ElevValFTFieldIndex = RandPointsFC.FindField("ELEV_FT");

            //open updatable reader to random points for this survey
            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = "SurveyID=" + SurveyID;
            RandPntCursor = RandPointsFC.Update(ThisQueryFilter, false);

            //create an insert cursor for isnerting transects into transect featureclass
            InsertCursor = TransectFC.Insert(true);
            InsertBuffer = TransectFC.CreateFeatureBuffer();


            string CorruptFlag;

            while ((RandPntFeature = RandPntCursor.NextFeature()) != null)
            {
                //if we are at the requested transect count, stop making transects
                if (TransectCount == TransectTotal) break;

                //make sure the random point shape is valid
                if (RandPntFeature.ShapeCopy == null) continue;

                //check if this point is corrupted, if so don't process it
                CorruptFlag = (string) SafeConvert(RandPntFeature.get_Value(AcceptedFieldIndex), typeof (string));
                if (CorruptFlag == "?")
                {
                    CorruptCount++;
                    continue;
                }

                //set point to in transect generation mode. if a crash occurs because of a point then
                //we know which point it was
                if (AcceptedFieldIndex > -1)
                {
                    RandPntFeature.set_Value(AcceptedFieldIndex, "?");
                    RandPntCursor.UpdateFeature(RandPntFeature);
                }

                TransType = "";

                //generate new transect from random point
                NewTrnPolyline = CreateTransPolylineFromRandPoint(RandPntFeature.ShapeCopy as IPoint,
                                                                  MaxLineLength, MinLineLength, BoundaryFC, ExcludedFC,
                                                                  FlatAreasFC, SurveyID, ClippedRasterDS,
                                                                  TransCreateAttempts, ref TransType, ref ErrorMessage);

                //update counters on startus of transect generation
                if (TransType == "straight") StraightCount++;
                if (TransType == "contour") ContourCount++;
                if (TransType == "") FailCount++;

                //if we successfully generated a transect...
                if (NewTrnPolyline != null)
                {
                    //set the transect id field and update transect id counter
                    if (TransectIDFieldIndex > -1)
                    {
                        InsertBuffer.set_Value(TransectIDFieldIndex, NewTransectID);
                        NewTransectID++;
                    }

                    //set new transect feature fields
                    InsertBuffer.Shape = NewTrnPolyline;
                    if (FlownFieldIndex > -1) InsertBuffer.set_Value(FlownFieldIndex, "N");
                    if (PT_IDFieldIndex > -1) InsertBuffer.set_Value(PT_IDFieldIndex, RandPntFeature.OID);
                    if (SurveyIDFieldIndex > -1) InsertBuffer.set_Value(SurveyIDFieldIndex, SurveyID);
                    if (BatchIDFieldIndex > -1) InsertBuffer.set_Value(BatchIDFieldIndex, NewBatchID);
                    if (TargetLengthFieldIndex > -1) InsertBuffer.set_Value(TargetLengthFieldIndex, TargetLength);

                    //update random point fields
                    if (AcceptedFieldIndex > -1) RandPntFeature.set_Value(AcceptedFieldIndex, "Y");


                    if (TransectFC.FindField("LENGTH_MTR") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("LENGTH_MTR"), NewTrnPolyline.Length);

                    if (TransectFC.FindField("PROJECTION") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("PROJECTION"), NewTrnPolyline.SpatialReference.Name);


                    //save from point coords in transect fields
                    TempPoint = ((IClone) NewTrnPolyline.FromPoint).Clone() as IPoint;

                    if (TransectFC.FindField("PROJTD_X1") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("PROJTD_X1"), TempPoint.X);

                    if (TransectFC.FindField("PROJTD_Y1") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("PROJTD_Y1"), TempPoint.Y);

                    //project transect from point to geo coordinates and save coords in transect fields
                    ((IGeometry2) TempPoint).ProjectEx(GetWGSSpatRef(),
                                                       esriTransformDirection.esriTransformForward, null, false, 0, 0);

                    if (TransectFC.FindField("DD_LONG1") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("DD_LONG1"), TempPoint.X);

                    if (TransectFC.FindField("DD_LAT1") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("DD_LAT1"), TempPoint.Y);


                    //save to point coords in transect fields
                    TempPoint = ((IClone) NewTrnPolyline.ToPoint).Clone() as IPoint;

                    if (TransectFC.FindField("PROJTD_X2") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("PROJTD_X2"), TempPoint.X);

                    if (TransectFC.FindField("PROJTD_Y2") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("PROJTD_Y2"), TempPoint.Y);

                    //project transect to point to geo coordinates and save coords in transect fields
                    ((IGeometry2) TempPoint).ProjectEx(GetWGSSpatRef(),
                                                       esriTransformDirection.esriTransformForward, null, false, 0, 0);

                    if (TransectFC.FindField("DD_LONG2") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("DD_LONG2"), TempPoint.X);

                    if (TransectFC.FindField("DD_LAT2") > -1)
                        InsertBuffer.set_Value(TransectFC.FindField("DD_LAT2"), TempPoint.Y);

                    if (TrnsectElevValFieldIndex > -1 && ElevValFieldIndex > -1)
                        InsertBuffer.set_Value(TrnsectElevValFieldIndex, RandPntFeature.get_Value(ElevValFieldIndex));


                    if (TrnsectElevValFTFieldIndex > -1 && ElevValFTFieldIndex > -1)
                        InsertBuffer.set_Value(TrnsectElevValFTFieldIndex, RandPntFeature.get_Value(ElevValFTFieldIndex));

                    //add new feature to feature class
                    InsertCursor.InsertFeature(InsertBuffer);

                    TransectCount++;
                }
                else
                {
                    //update random point fields
                    if (AcceptedFieldIndex > -1) RandPntFeature.set_Value(AcceptedFieldIndex, "N");
                }

                SetProgressMessage(
                    string.Format("Generating Transects (Straight:{0}, Contour:{1}, Failed:{2}, Corrupted:{3})",
                                  StraightCount, ContourCount, FailCount, CorruptCount), false);

                RandPntCursor.UpdateFeature(RandPntFeature);
            }

            InsertCursor = null;
            RandPntCursor = null;
        }

        /// <summary>
        ///     create a transect from a source point
        /// </summary>
        public static IPolyline CreateTransPolylineFromRandPoint(IPoint RandPoint, double MaxLineLength,
                                                                 double MinLineLength,
                                                                 IFeatureClass BndPolyFC, IFeatureClass ExclPolyFC,
                                                                 IFeatureClass FlatAreasPolyFC, int SurveyID,
                                                                 IGeoDataset npsDemDataset,
                                                                 int TransCreateAttempts, ref string TranType,
                                                                 ref string ErrorMessage)
        {
            bool IsInExcludedAreas, IsInBndPoly;
            IPointCollection NewLine = null, NewLine2 = null, pPolyline = null;
            IPoint npsToPoint;
            IPolyline npsGetPoint;
            double DrawnAngle = -1;
            object Missing = Type.Missing;
            TranType = "";


            //generate the first line in the transect
            NewLine = TryCreateTransectSide(RandPoint, SurveyID, ref DrawnAngle, FlatAreasPolyFC,
                                            ExclPolyFC, BndPolyFC, MaxLineLength, MinLineLength, TransCreateAttempts) as
                      IPointCollection;

            if (NewLine != null)
            {
                //generate the second line in the transect
                NewLine2 = TryCreateTransectSide(RandPoint, SurveyID, ref DrawnAngle, FlatAreasPolyFC,
                                                 ExclPolyFC, BndPolyFC, MaxLineLength, MinLineLength,
                                                 TransCreateAttempts) as IPointCollection;
            }

            //if we have both lines in the transect, create transect
            if (NewLine != null && NewLine2 != null)
            {
                pPolyline = new PolylineClass();
                npsToPoint = new PointClass();

                npsGetPoint = NewLine as IPolyline;
                npsGetPoint.QueryToPoint(npsToPoint);
                pPolyline.AddPoint(npsToPoint, ref Missing, ref Missing);

                pPolyline.AddPoint(RandPoint, ref Missing, ref Missing);

                npsGetPoint = NewLine2 as IPolyline;
                npsGetPoint.QueryToPoint(npsToPoint);
                pPolyline.AddPoint(npsToPoint, ref Missing, ref Missing);

                TranType = "straight";
            }


            //if transects could not be drawn (pPolyline is still nothing), get contour instead
            if (pPolyline == null)
            {
                //try to create a contour since straight lines failed
                pPolyline = GetContourLineFromPoint(RandPoint, MaxLineLength, MinLineLength, npsDemDataset,
                                                    SurveyID, BndPolyFC, ExclPolyFC) as IPointCollection;

                if (pPolyline != null)
                {
                    //check if contour is within boundary
                    IsInBndPoly = HasRelationshipWithFC(pPolyline as IGeometry, esriSpatialRelEnum.esriSpatialRelWithin,
                                                        BndPolyFC, "SurveyID=" + SurveyID);

                    //check if contour falls in excluded areas
                    IsInExcludedAreas = HasRelationshipWithFC(pPolyline as IGeometry,
                                                              esriSpatialRelEnum.esriSpatialRelCrosses,
                                                              ExclPolyFC, "SurveyID=" + SurveyID);

                    //if contour obtained falls in excluded areas, abandon poyline
                    if (IsInBndPoly == false || IsInExcludedAreas) pPolyline = null;
                }

                if (pPolyline != null) TranType = "contour";
            }

            GC.Collect();

            return pPolyline as IPolyline;
        }

        /// <summary>
        ///     create a contour transect from a source point
        /// </summary>
        public static IPolyline GetContourLineFromPoint(IPoint npsPoint, double MaxLineLength, double MinLineLength,
                                                        IGeoDataset npsDemDataset, int SurveyID, IFeatureClass BndPolyFC,
                                                        IFeatureClass ExclPolyFC)
        {
            ISurfaceOp2 npsSurfaceOp;
            IPolyline npsPolyline, npsClippedPolyline, PrevTryPolyline = null;
            bool DidMaxLengthFit, IsInBndPoly, IsInExcludedAreas;
            int LengthAttempts;
            double CurTransLength, LengthTryIncrements, ElevValue;


            //determine a set number of times to try generating intermidiate lengths in both min and max
            //lengths were successful
            LengthAttempts = 20;
            LengthTryIncrements = (MaxLineLength - MinLineLength)/LengthAttempts;

            //create a polyline from the elevation for the random point
            npsSurfaceOp = new RasterSurfaceOpClass();
            npsPolyline = new PolylineClass();
            npsSurfaceOp.ContourAsPolyline(npsDemDataset, npsPoint, out npsPolyline, out ElevValue);
            npsSurfaceOp = null;
            GC.Collect();

            if (npsPolyline == null) return null;


            DidMaxLengthFit = false;

            //try to cut the contour into the max length specified
            npsClippedPolyline = ClipContour(npsPoint, npsPolyline, (MaxLineLength/2), (MaxLineLength/2));

            IsInBndPoly = false;
            IsInExcludedAreas = false;

            if (npsClippedPolyline != null)
            {
                //check if contour is within boundary
                IsInBndPoly = HasRelationshipWithFC(npsClippedPolyline,
                                                    esriSpatialRelEnum.esriSpatialRelWithin, BndPolyFC,
                                                    "SurveyID=" + SurveyID);

                //check if contour falls in excluded areas
                IsInExcludedAreas = HasRelationshipWithFC(npsClippedPolyline,
                                                          esriSpatialRelEnum.esriSpatialRelCrosses, ExclPolyFC,
                                                          "SurveyID=" + SurveyID);
            }

            //if contour obtained falls in excluded areas, abandon poyline
            if (IsInBndPoly == false || IsInExcludedAreas) npsClippedPolyline = null;

            //remember if max length fit
            if (npsClippedPolyline != null) DidMaxLengthFit = true;


            //if the max length did not fit, try the min length
            if (npsClippedPolyline == null)
                npsClippedPolyline = ClipContour(npsPoint, npsPolyline, (MinLineLength/2), (MinLineLength/2));

            //check if the shortest length can fit
            if (npsClippedPolyline != null)
            {
                //check if contour is within boundary
                IsInBndPoly = HasRelationshipWithFC(npsClippedPolyline,
                                                    esriSpatialRelEnum.esriSpatialRelWithin, BndPolyFC,
                                                    "SurveyID=" + SurveyID);

                //check if contour falls in excluded areas
                IsInExcludedAreas = HasRelationshipWithFC(npsClippedPolyline,
                                                          esriSpatialRelEnum.esriSpatialRelCrosses, ExclPolyFC,
                                                          "SurveyID=" + SurveyID);
            }

            //if the min failed then clear it
            if (IsInBndPoly == false || IsInExcludedAreas) npsClippedPolyline = null;

            //if the max length failed but the min length fit, then try various lengths
            if (npsClippedPolyline != null && DidMaxLengthFit == false)
            {
                CurTransLength = (MinLineLength/2) + LengthTryIncrements;
                while (npsClippedPolyline != null)
                {
                    PrevTryPolyline = npsClippedPolyline;
                    npsClippedPolyline = ClipContour(npsPoint, npsPolyline, CurTransLength, CurTransLength);

                    IsInBndPoly = false;
                    IsInExcludedAreas = false;

                    if (npsClippedPolyline != null)
                    {
                        //check if contour is within boundary
                        IsInBndPoly = HasRelationshipWithFC(npsClippedPolyline,
                                                            esriSpatialRelEnum.esriSpatialRelWithin, BndPolyFC,
                                                            "SurveyID=" + SurveyID);

                        //check if contour falls in excluded areas
                        IsInExcludedAreas = HasRelationshipWithFC(npsClippedPolyline,
                                                                  esriSpatialRelEnum.esriSpatialRelCrosses, ExclPolyFC,
                                                                  "SurveyID=" + SurveyID);
                    }

                    //if contour obtained falls in excluded areas, abandon poyline
                    if (IsInBndPoly == false || IsInExcludedAreas) npsClippedPolyline = null;

                    CurTransLength = CurTransLength + LengthTryIncrements;
                }

                //set the last successful as the clipped polyline
                npsClippedPolyline = PrevTryPolyline;
            }

            return npsClippedPolyline;
        }

        /// <summary>
        ///     clip a polyline at the specified lengths
        /// </summary>
        public static IPolyline ClipContour(IPoint npsPoint, IPolyline npsPolyline, double RangeLeft, double RangeRight)
        {
            ICurve NewPolyline = null, pl2, pl1;
            IPoint npsExactPoint;
            double PolylineLength,
                   DistanceFromStart = 0,
                   DistanceFromCurve = 0,
                   NewLineEnd,
                   AmountOver,
                   start1 = 0,
                   start2 = 0,
                   end1 = 0,
                   end2 = 0,
                   NewLineStart;
            bool npsRightSide = false, IsOutOfBoundsLeft, IsOutOfBoundsRight;
            ISegmentCollection newpl = null;


            if (npsPolyline == null) return null;

            PolylineLength = npsPolyline.Length;

            //get the distance of the point from the start of the contour
            npsExactPoint = new PointClass();
            npsPolyline.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, npsPoint, false,
                                              npsExactPoint, ref DistanceFromStart, ref DistanceFromCurve,
                                              ref npsRightSide);

            //if (contour)polyline is not a closed line
            if (npsPolyline.IsClosed == false)
            {
                IsOutOfBoundsLeft = false;
                IsOutOfBoundsRight = false;


                NewLineStart = DistanceFromStart - RangeLeft;
                NewLineEnd = DistanceFromStart + RangeRight;

                if (NewLineStart < 0) IsOutOfBoundsLeft = true;

                if (NewLineEnd > PolylineLength) IsOutOfBoundsRight = true;


                //if contour is outbound to the left, then try to shift it to the right
                if (IsOutOfBoundsLeft && IsOutOfBoundsRight == false)
                {
                    AmountOver = (DistanceFromStart - RangeLeft)*-1;

                    if (((DistanceFromStart + RangeRight) + AmountOver) < PolylineLength)
                    {
                        NewLineStart = 0;
                        NewLineEnd = (DistanceFromStart + RangeRight) + AmountOver;
                    }
                    else
                    {
                        NewLineStart = -1;
                        NewLineEnd = -1;
                    }
                }

                //if contour is out of bounds to the right, then try to shift it to the left
                if (IsOutOfBoundsLeft == false && IsOutOfBoundsRight)
                {
                    AmountOver = (DistanceFromStart + RangeRight) - PolylineLength;

                    if (((DistanceFromStart - RangeLeft) - AmountOver) >= 0)
                    {
                        NewLineStart = (DistanceFromStart - RangeLeft) - AmountOver;
                        NewLineEnd = PolylineLength;
                    }
                    else
                    {
                        NewLineStart = -1;
                        NewLineEnd = -1;
                    }
                }

                //since the contour is to short on both ends to create the new polyline,abound operation
                if (IsOutOfBoundsLeft && IsOutOfBoundsRight)
                {
                    NewLineStart = -1;
                    NewLineEnd = -1;
                }

                if (NewLineStart != -1 && NewLineEnd != -1)
                {
                    NewPolyline = new PolylineClass();
                    npsPolyline.GetSubcurve(NewLineStart, NewLineEnd, false, out NewPolyline);
                }
            }
            else //if polyline is closed
            {
                if ((RangeLeft + RangeRight) < PolylineLength)
                {
                    pl1 = new PolylineClass();
                    pl2 = new PolylineClass();
                    newpl = new PolylineClass();

                    //if desired subcurve does not cross the start/end point of the closed polyline(circular) then get
                    //a sub curve as normal
                    if ((DistanceFromStart - RangeLeft) >= 0 && (DistanceFromStart + RangeRight) < PolylineLength)
                    {
                        NewLineStart = DistanceFromStart - RangeLeft;
                        NewLineEnd = DistanceFromStart + RangeRight;

                        NewPolyline = new PolylineClass();
                        npsPolyline.GetSubcurve(NewLineStart, NewLineEnd, false, out NewPolyline);
                    }
                    else
                    {
                        //if desired subcurve does cross the start/end point of the closed polyline(circular)
                        //then two subcurves will have to be formed each ending at the start/end point of the circle

                        //if left range oversteps start/end then...
                        if ((DistanceFromStart - RangeLeft) < 0)
                        {
                            start1 = PolylineLength - ((DistanceFromStart - RangeLeft)*-1);
                            end1 = PolylineLength;
                            start2 = 0;
                            end2 = DistanceFromStart + RangeRight;
                        }

                        //if right range oversteps start/end then...
                        if ((DistanceFromStart + RangeRight) > PolylineLength)
                        {
                            start1 = DistanceFromStart - RangeLeft;
                            end1 = PolylineLength;
                            start2 = 0;
                            end2 = (DistanceFromStart + RangeRight) - PolylineLength;
                        }

                        //combine sub curves
                        npsPolyline.GetSubcurve(start1, end1, false, out pl1);
                        npsPolyline.GetSubcurve(start2, end2, false, out pl2);
                        newpl.AddSegmentCollection(pl1 as ISegmentCollection);
                        newpl.AddSegmentCollection(pl2 as ISegmentCollection);
                        NewPolyline = newpl as ICurve;
                    }
                }
            }


            return NewPolyline as IPolyline;
        }

        /// <summary>
        ///     try to generate a transect line side in an allowed area from a source point
        /// </summary>
        public static IPolyline TryCreateTransectSide(IPoint RandPoint, int SurveyID, ref double DrawnAngle,
                                                      IFeatureClass FlatAreasPolyFC, IFeatureClass ExclPolyFC,
                                                      IFeatureClass BndPolyFC,
                                                      double MaxLineLength, double MinLineLength,
                                                      int TransCreateAttempts)
        {
            int LengthAttempts, Counter;
            double LengthTryIncrements, TryAngle, CurTryLength, StraightAngle;
            IPolyline PrevLineAttempt = null, NewLine = null;
            bool DidDrawMaxLength, IsStaticAngle;


            //determine a set number of times to try generating intermidiate lengths in both min and max
            //lengths were successful
            LengthAttempts = 20;
            LengthTryIncrements = (MaxLineLength - MinLineLength)/LengthAttempts;

            //get opposite angle to draw straight line
            StraightAngle = -1;
            if (DrawnAngle != -1)
            {
                StraightAngle = (DrawnAngle + 180);
                if (StraightAngle > 360) StraightAngle = StraightAngle - 360;
            }

            //try max length the max number of attempts. each attempt is at a new random angle
            DidDrawMaxLength = false;
            TryAngle = -1;
            IsStaticAngle = false;

            if (DrawnAngle != -1)
            {
                TryAngle = StraightAngle;
                IsStaticAngle = true;
            }

            Counter = 0;
            while (NewLine == null && Counter < TransCreateAttempts)
            {
                NewLine = TryCreateTransLine(MaxLineLength/2, RandPoint, SurveyID, ref TryAngle, IsStaticAngle,
                                             FlatAreasPolyFC, ExclPolyFC, BndPolyFC);
                if (NewLine != null) break;

                Counter = Counter + 1;
                TryAngle = DrawnAngle;
                IsStaticAngle = false;
            }


            //remember if we were able to draw the max length
            if (NewLine != null) DidDrawMaxLength = true;

            //if we could not draw max length, try shortest length
            if (NewLine == null)
            {
                TryAngle = -1;
                IsStaticAngle = false;
                if (DrawnAngle != -1)
                {
                    TryAngle = StraightAngle;
                    IsStaticAngle = true;
                }

                Counter = 0;
                while (NewLine == null && Counter < TransCreateAttempts)
                {
                    NewLine = TryCreateTransLine(MinLineLength/2, RandPoint, SurveyID, ref TryAngle, IsStaticAngle,
                                                 FlatAreasPolyFC, ExclPolyFC, BndPolyFC);
                    if (NewLine != null) break;
                    Counter = Counter + 1;
                    TryAngle = DrawnAngle;
                    IsStaticAngle = false;
                }
            }

            //if we could not draw max length line but succeeded at drawing shortest line, try various lengths
            //at same angle until we fail and we'll use last successful length
            if (NewLine != null && DidDrawMaxLength == false)
            {
                Counter = 0;
                IsStaticAngle = true;
                CurTryLength = MinLineLength/2;
                while (NewLine != null && Counter < TransCreateAttempts)
                {
                    PrevLineAttempt = NewLine;
                    CurTryLength = CurTryLength + LengthTryIncrements;

                    NewLine = TryCreateTransLine(CurTryLength, RandPoint, SurveyID, ref TryAngle, IsStaticAngle,
                                                 FlatAreasPolyFC, ExclPolyFC, BndPolyFC);

                    Counter = Counter + 1;
                }

                NewLine = PrevLineAttempt;
            }

            //remember drawn angle
            DrawnAngle = TryAngle;


            return NewLine;
        }

        /// <summary>
        ///     try to generate a transect line in an allowed area from a source point
        /// </summary>
        public static IPolyline TryCreateTransLine(double TransLength, IPoint RandPoint, int SurveyID,
                                                   ref double TryAngle,
                                                   bool IsStaticAngle, IFeatureClass FlatAreasPolyFC,
                                                   IFeatureClass ExclPolyFC, IFeatureClass BndPolyFC)
        {
            IPoint EndPoint;
            double Upperbound, Lowerbound, AngleInRadians, AngleInDegrees, PI;
            bool IsInBndPoly, IsInExcludedAreas, IsInFlatAreas;
            IPointCollection NewLine;
            ITransform2D npsTransform2D;
            object Missing = Type.Missing;


            PI = 4*Math.Atan(1);
            IsInExcludedAreas = false;
            IsInFlatAreas = false;
            IsInBndPoly = false;


            //make the endpoint the max trans distance from the rand(center) point
            EndPoint = new PointClass();
            EndPoint.PutCoords(RandPoint.X, RandPoint.Y + TransLength);


            //generate a line from random point to new point
            NewLine = new PolylineClass();
            NewLine.AddPoint(RandPoint, ref Missing, ref Missing);
            NewLine.AddPoint(EndPoint, ref Missing, ref Missing);

            if (IsStaticAngle)
                AngleInDegrees = TryAngle;
            else if (TryAngle == -1)
            {
                //generate a random angle at which to rotate line using random point as axis
                Upperbound = 360;
                Lowerbound = 1;
                AngleInDegrees = Convert.ToInt32((Upperbound - Lowerbound + 1)*GetRandomNumber() + Lowerbound);
            }
            else
            {
                //generate a random angle 90 degrees to the left and right of a specified try angle
                AngleInDegrees = GetRandomAngleInRange(Convert.ToInt32(TryAngle), 90, 90);
            }


            //convert angle from degrees to radians
            AngleInRadians = AngleInDegrees*(PI/180)*-1;

            //rotate line at random angle
            npsTransform2D = NewLine as ITransform2D;
            npsTransform2D.Rotate(RandPoint, AngleInRadians);

            //check if new line falls in flat areas

            IsInFlatAreas = HasRelationshipWithFC(NewLine as IGeometry, esriSpatialRelEnum.esriSpatialRelWithin,
                                                  FlatAreasPolyFC, "SurveyID=" + SurveyID);

            //check if  new line   falls in excluded areas
            IsInExcludedAreas = HasRelationshipWithFC(NewLine as IGeometry, esriSpatialRelEnum.esriSpatialRelCrosses,
                                                      ExclPolyFC, "SurveyID=" + SurveyID);

            //check if   new line  is within boundary
            IsInBndPoly = HasRelationshipWithFC(NewLine as IGeometry, esriSpatialRelEnum.esriSpatialRelWithin,
                                                BndPolyFC, "SurveyID=" + SurveyID);

            if (IsInExcludedAreas == false && IsInBndPoly && IsInFlatAreas)
            {
                //return new line
                TryAngle = AngleInDegrees;
                return NewLine as IPolyline;
            }
            return null;
        }

        /// <summary>
        ///     get a random angle at which to generate a transect
        /// </summary>
        public static int GetRandomAngleInRange(int Angle, int RangeLeft, int RangeRight)
        {
            int Counter, RandAngle, ReStartCounter;
            int[] DegreeList, BannedValues;


            DegreeList = new int[361];
            BannedValues = new int[361];


            ReStartCounter = -1;

            //create array of all posible degrees as well a check for banned/invalid values
            for (Counter = 1; Counter <= 360; Counter++)
            {
                DegreeList[Counter] = Counter;
                BannedValues[Counter] = 0;
            }

            //set all the values in the ban range to the right to 1
            for (Counter = Angle; Counter <= (RangeRight + Angle); Counter++)
            {
                if (ReStartCounter != -1)
                {
                    if (Counter == ReStartCounter + 1) break;
                }

                if (Counter == 361)
                {
                    ReStartCounter = RangeRight - (Counter - Angle);
                    if (ReStartCounter == 0) break;
                    Counter = 1;
                }

                BannedValues[Counter] = 1;
            }

            //set all the values in the ban range to the left to 1
            ReStartCounter = -1;
            for (Counter = Angle; Counter <= (Angle - RangeLeft); Counter--)
            {
                if (ReStartCounter != -1)
                {
                    if (Counter == ReStartCounter) break;
                }

                if (Counter == 0)
                {
                    ReStartCounter = 360 - (RangeLeft - Angle);
                    if ((RangeLeft - Angle) == 0) break;
                    Counter = 360;
                }

                BannedValues[Counter] = 1;
            }

            //get a random number that is not in the ban range and return it
            RandAngle = -1;
            while (RandAngle == -1)
            {
                RandAngle = Convert.ToInt32((360 - 1 + 1)*GetRandomNumber() + 1);

                if (BannedValues[RandAngle] == 1)
                    RandAngle = -1;
                else
                    break;
            }

            return RandAngle;
        }

        /// <summary>
        ///     add elevation values from the specified batch of points for the specified survey
        /// </summary>
        public static void AddZValuesToPoints(int SurveyID, int NewBatchID, IGeoDataset ThisRasterDS,
                                              string ThisDEMUnits, ref string ErrorMessage)
        {
            AddZValuesToPoints(SurveyID, NewBatchID, -1, ThisRasterDS, ThisDEMUnits, ref ErrorMessage);
        }

        public static void AddZValuesToPoints(int SurveyID, int NewBatchID, int TempBatchID,
                                              IGeoDataset ThisRasterDS, string ThisDEMUnits, ref string ErrorMessage)
        {
            IGeoProcessor ThisGeoProcessor = null;
            IVariantArray GPParams;
            IFeatureClass TempFCHolderFC = null, PointFC = null;
            string CopyRandFilter = "", TempElePoints, TempFCHolder, InternalError = "";
            NPSGlobal NPS;
            int GPLIndex = 0;


            NPS = NPSGlobal.Instance;
            TempFCHolder = "TempFCHolder";
            TempElePoints = "TempElePoints";


            ThisDEMUnits = ThisDEMUnits.ToLower();


            if (string.IsNullOrEmpty(ErrorMessage))
                PointFC = GetFeatureClass(NPS.LYR_RANDOMPOINTS, ref ErrorMessage);


            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                SetProgressMessage("Deleting old temp files");

                //delete the temporary table if it already exists
                DeleteDataset(TempElePoints, esriDatasetType.esriDTFeatureClass, ref InternalError);
                DeleteDataset(TempElePoints + "_Tbl", esriDatasetType.esriDTTable, ref InternalError);
                DeleteDataset(TempFCHolder, esriDatasetType.esriDTFeatureClass, ref InternalError);
                DeleteDataset(TempFCHolder + "_Tbl", esriDatasetType.esriDTTable, ref InternalError);
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                try
                {
                    SetProgressMessage("Copying random points to temp FeatureClass");

                    ThisGeoProcessor = new GeoProcessor();
                    GPParams = new VarArrayClass();

                    GPParams.Add(NPS.Workspace.PathName);
                    GPParams.Add(TempFCHolder);
                    GPParams.Add("POINT");
                    GPParams.Add(Path.Combine(NPS.Workspace.PathName, ((IDataset) PointFC).Name));


                    ThisGeoProcessor.Execute("CreateFeatureClass_management", GPParams, null);
                }
                catch (Exception ex)
                {
                    ErrorMessage = string.Format("Geoprocessor error:\r\nTask:CreateFeatureClass_management\r\n"
                                                 + "Params:{0},{1},{2},{3}\r\nException:{4}", NPS.Workspace.PathName,
                                                 TempFCHolder, "POINT",
                                                 Path.Combine(NPS.Workspace.PathName, ((IDataset) PointFC).Name),
                                                 ex.Message);
                }

                ThisGeoProcessor = null;
                DeleteLayerFromMap(TempFCHolder);
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                CopyRandFilter = " SurveyID=" + SurveyID;
                if (TempBatchID != -1) CopyRandFilter += " And BATCH_ID=" + TempBatchID;

                //get the temp feature class and copy only surveyid points to it
                TempFCHolderFC = GetFeatureClass(TempFCHolder, ref ErrorMessage);

                if (string.IsNullOrEmpty(ErrorMessage))
                    CopyFeatures(PointFC, CopyRandFilter, TempFCHolderFC, null, ref ErrorMessage);

                FeatureCount(TempFCHolderFC, "");
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                try
                {
                    SetProgressMessage("Extracting point elevation from DEM");

                    ThisGeoProcessor = new GeoProcessor();
                    GPParams = new VarArrayClass();

                    //GPParams.Add(TempFCHolderFC);
                    //GPParams.Add(ThisRasterDS);
                    //GPParams.Add(System.IO.Path.Combine(NPS.Workspace.PathName, TempElePoints));
                    //GeoResult = ThisGeoProcessor.Execute("ExtractValuesToPoints_sa", GPParams, null);

                    GPParams.Add(ThisRasterDS);
                    GPParams.Add(TempFCHolderFC);
                    GPParams.Add("Spot"); // field name to add
                    GPParams.Add(1); // Z-Factor
                    ThisGeoProcessor.Execute("SurfaceSpot_3D", GPParams, null);
                }
                catch (Exception ex)
                {
                    ErrorMessage = string.Format("Geoprocessor error:\r\nTask:SurfaceSpot_3D\r\n"
                                                 + "Params:{0},{1},{2},{3}\r\nException:{4}",
                                                 ((IDataset) ThisRasterDS).Name,
                                                 ((IDataset) TempFCHolderFC).Name, "Spot", 1, ex.Message);
                }

                ThisGeoProcessor = null;

                //delete GPL[index] layers created from SurfaceSpot geoprocessor call
                while (GPLIndex < 10)
                {
                    DeleteLayerFromMap("GPL" + GPLIndex);
                    GPLIndex++;
                }
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                SetProgressMessage("Importing updated random points");

                //delete all points for that survey
                DeleteFeatures(PointFC, CopyRandFilter, ref ErrorMessage);


                //replace the points of that survey with these points
                CopyFeaturesForPoints(TempFCHolderFC, "", PointFC, NewBatchID, ThisDEMUnits, ref ErrorMessage);
            }

            SetProgressMessage("Cleaning up temp files");
            DeleteDataset(TempElePoints, esriDatasetType.esriDTFeatureClass, ref InternalError);
            DeleteDataset(TempElePoints + "_Tbl", esriDatasetType.esriDTTable, ref InternalError);
            DeleteDataset(TempFCHolder, esriDatasetType.esriDTFeatureClass, ref InternalError);
            DeleteDataset(TempFCHolder + "_Tbl", esriDatasetType.esriDTTable, ref InternalError);
        }

        /// <summary>
        ///     custom copy function for copy points with elevation value over to random points featureclass
        /// </summary>
        public static void CopyFeaturesForPoints(IFeatureClass FromFC, string FromWhereClause, IFeatureClass ToFC,
                                                 int NewBatchID, string ThisDEMUnits, ref string ErrorMessage)
        {
            IFeatureCursor FromFCCursor, ToFCCursor;
            IFeatureBuffer ToFCBuffer;
            IFeature FromFCFeature;
            IQueryFilter qFilter;
            string CurrentFieldName;
            double ZValue = 0;
            IPoint ThisPoint;
            int FromFieldTotal,
                FromFieldCount,
                CurrentFieldIndex,
                ElevFTFieldIndex,
                ProjFieldIndex,
                ProjX,
                ProjY,
                Lat,
                Lng,
                LatP,
                LngP,
                ElevMFieldIndex,
                HasTransFieldIndex,
                count = 0;
            IPoint TempPoint;
            string thisVal;


            ThisDEMUnits = ThisDEMUnits.ToLower();

            ProjX = ToFC.FindField("PROJTD_X");
            ProjY = ToFC.FindField("PROJTD_Y");
            Lat = ToFC.FindField("PTDD_LAT");
            Lng = ToFC.FindField("PTDD_LONG");
            LatP = ToFC.FindField("PTDM_LAT");
            LngP = ToFC.FindField("PTDM_LONG");
            ElevMFieldIndex = ToFC.FindField("ELEV_M");
            ElevFTFieldIndex = ToFC.FindField("ELEV_FT");
            ProjFieldIndex = ToFC.FindField("PROJECTION");
            HasTransFieldIndex = ToFC.FindField("HASTRANS");

            //get all feature from FromFC that must be copied over
            qFilter = new QueryFilterClass();
            qFilter.WhereClause = FromWhereClause;
            FromFCCursor = FromFC.Search(qFilter, false);


            //get an insert cursor and buffer to add features to ToFC
            ToFCBuffer = ToFC.CreateFeatureBuffer();
            ToFCCursor = ToFC.Insert(true);


            //loop through all features in FromFC that need to be added to ToFC
            while ((FromFCFeature = FromFCCursor.NextFeature()) != null)
            {
                count = count + 1;

                //add shape to new ToFC feature
                ToFCBuffer.Shape = FromFCFeature.ShapeCopy;
                ThisPoint = ToFCBuffer.Shape as IPoint;

                if (ThisPoint == null) continue;

                //look for match field names and copy over field value
                FromFieldTotal = FromFC.Fields.FieldCount;
                for (FromFieldCount = 0; FromFieldCount < FromFieldTotal; FromFieldCount++)
                {
                    //get the from features current field name
                    CurrentFieldName = FromFC.Fields.get_Field(FromFieldCount).Name;

                    //if (CurrentFieldName == "RASTERVALU" || CurrentFieldName == "RASTERVALUE")
                    if (CurrentFieldName == "Spot")
                    {
                        ZValue = (double) SafeConvert(FromFCFeature.get_Value(FromFieldCount), typeof (double));
                        ThisPoint.Z = ZValue;
                    }

                    //if toFC feature has a field with the same name and that name is
                    //not one of those listed below, copy over value
                    CurrentFieldIndex = ToFCBuffer.Fields.FindField(CurrentFieldName);
                    if (CurrentFieldIndex > -1
                        && CurrentFieldName.ToUpper() != "SHAPE"
                        && CurrentFieldName.ToUpper() != "SHAPE_AREA"
                        && CurrentFieldName.ToUpper() != "SHAPE_LENGTH"
                        && CurrentFieldName.ToUpper() != "OBJECTID"
                        && CurrentFieldName.ToUpper() != "OBJECTID_1"
                        && FromFC.Fields.get_Field(FromFieldCount).Type != esriFieldType.esriFieldTypeOID
                        && FromFC.Fields.get_Field(FromFieldCount).Type != esriFieldType.esriFieldTypeGeometry)
                    {
                        object testVal = FromFCFeature.get_Value(FromFieldCount);
                        if (CurrentFieldName == "BATCH_ID" && NewBatchID != -1)
                            testVal = NewBatchID;


                        ToFCBuffer.set_Value(CurrentFieldIndex, testVal);
                    }
                }


                if (ProjX > -1) ToFCBuffer.set_Value(ProjX, ThisPoint.X);
                if (ProjY > -1) ToFCBuffer.set_Value(ProjY, ThisPoint.Y);


                if (ProjFieldIndex > -1)
                    ToFCBuffer.set_Value(ProjFieldIndex, ((IGeoDataset) ToFC).SpatialReference.Name);

                if (HasTransFieldIndex > -1) ToFCBuffer.set_Value(HasTransFieldIndex, "P");


                TempPoint = ProjectToWGS(ThisPoint);
                if (Lng > -1) ToFCBuffer.set_Value(Lng, TempPoint.X);

                if (Lat > -1) ToFCBuffer.set_Value(Lat, TempPoint.Y);

                if (LngP > -1)
                {
                    thisVal = DDtoDMS(TempPoint.X, true);
                    ToFCBuffer.set_Value(LngP, thisVal);
                }

                if (LatP > -1)
                {
                    thisVal = DDtoDMS(TempPoint.Y, false);
                    ToFCBuffer.set_Value(LatP, thisVal);
                }


                //set elev value to feature
                if (ElevMFieldIndex > -1)
                {
                    if (ThisDEMUnits == "feet") ToFCBuffer.set_Value(ElevMFieldIndex, 0.3048*ZValue);
                    if (ThisDEMUnits == "meters") ToFCBuffer.set_Value(ElevMFieldIndex, ZValue);
                }

                if (ElevFTFieldIndex > -1)
                {
                    if (ThisDEMUnits == "feet") ToFCBuffer.set_Value(ElevFTFieldIndex, ZValue);
                    if (ThisDEMUnits == "meters") ToFCBuffer.set_Value(ElevFTFieldIndex, ZValue*3.2808399);
                }


                ToFCCursor.InsertFeature(ToFCBuffer);
            }

            ToFCCursor = null;
            FromFCCursor = null;
        }

        /// <summary>
        ///     project a point the WGS coordinates
        /// </summary>
        public static IPoint ProjectToWGS(IPoint ThisPoint)
        {
            ISpatialReference pSpRef1;
            SpatialReferenceEnvironment pSpRFc;
            IGeographicCoordinateSystem pGCS;
            IPoint NewPoint;
            IGeometry NewGeom;
            IGeometry pGeo;

            NewPoint = new PointClass();
            NewPoint.X = ThisPoint.X;
            NewPoint.Y = ThisPoint.Y;
            NewPoint.Z = ThisPoint.Z;

            NewGeom = NewPoint;
            pGeo = ThisPoint;
            NewGeom.SpatialReference = pGeo.SpatialReference;

            if (NewGeom.SpatialReference is IUnknownCoordinateSystem)
            {
                return NewGeom as IPoint;
            }

            pSpRFc = new SpatialReferenceEnvironmentClass();
            pGCS = pSpRFc.CreateGeographicCoordinateSystem((int) esriSRGeoCSType.esriSRGeoCS_WGS1984);
            pSpRef1 = pGCS;
            pSpRef1.SetFalseOriginAndUnits(-180, -90, 1000000);


            NewGeom.Project(pSpRef1);

            return NewGeom as IPoint;
        }

        /// <summary>
        ///     get the WGS spatial reference (lat/lng)
        /// </summary>
        public static ISpatialReference GetWGSSpatRef()
        {
            ISpatialReference pSpRef1;
            SpatialReferenceEnvironment pSpRFc;
            IGeographicCoordinateSystem pGCS;

            pSpRFc = new SpatialReferenceEnvironmentClass();
            pGCS = pSpRFc.CreateGeographicCoordinateSystem((int) esriSRGeoCSType.esriSRGeoCS_WGS1984);
            pSpRef1 = pGCS;
            pSpRef1.SetFalseOriginAndUnits(-180, -90, 1000000);

            return pSpRef1;
        }

        /// <summary>
        ///     build the DMLS string representation of a lat or lng in decimal degrees
        /// </summary>
        public static string DDtoDMS(double coordinate, bool IsLng)
        {
            // Set flag if number is negative
            bool neg = coordinate < 0d;

            // Work with a positive number
            coordinate = Math.Abs(coordinate);

            // Get d/m/s components
            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = Math.Round(coordinate);

            // Create padding character
            char pad;
            char.TryParse("0", out pad);

            // Create d/m/s strings
            string dd = d.ToString();
            string mm = m.ToString().PadLeft(2, pad);
            string ss = s.ToString().PadLeft(2, pad);

            // Append d/m/s
            string dms = string.Format("{0}°{1}'{2}\"", dd, mm, ss);

            // Append compass heading
            if (IsLng)
                dms += neg ? "W" : "E";
            else
                dms += neg ? "S" : "N";


            // Return formated string
            return dms;
        }

        /// <summary>
        ///     get the units of a dataset
        /// </summary>
        public static string GetDatasetUnits(IGeoDataset npsGeoDataset, ref string ErrorMessage)
        {
            ISpatialReference npsSpatialReference;
            IProjectedCoordinateSystem npsPrjCoordSys;
            IGeographicCoordinateSystem npsGeoCoordSys;
            string UnitName;

            UnitName = "";

            try
            {
                npsSpatialReference = npsGeoDataset.SpatialReference;

                if (npsSpatialReference is IProjectedCoordinateSystem)
                {
                    npsPrjCoordSys = npsSpatialReference as IProjectedCoordinateSystem;
                    UnitName = npsPrjCoordSys.CoordinateUnit.Name;
                }

                if (npsSpatialReference is IGeographicCoordinateSystem)
                {
                    npsGeoCoordSys = npsSpatialReference as IGeographicCoordinateSystem;
                    UnitName = npsGeoCoordSys.CoordinateUnit.Name;
                }

                if (string.IsNullOrEmpty(UnitName))
                    ErrorMessage = "(Err) Units could not be obtained from dataset.";
            }
            catch (Exception ex)
            {
                ErrorMessage = "(Err) Error occured while trying to obtain Units from dataset." + ex.Message;
            }

            return UnitName;
        }

        /// <summary>
        ///     generate points in the form of a grid over the span of the survey area of the specified survey
        /// </summary>
        public static int GenerateGridPoints(int SurveyID, double StartX, double StartY,
                                             double PointSpacing, ref string ErrorMessage)
        {
            IPolygon BoundaryPoly;
            NPSGlobal NPS;
            int NextBatcID;
            IPoint StartPoint;
            IPoint[] NewPoints;
            IFeatureBuffer InsertBuffer;
            IFeatureCursor InsertCursor;
            int SurveyIDFieldIndex, BatchIDFieldIndex, GridPointTotal = 0;
            IFeatureClass ExcludeFC, RandPointsFC;

            NPS = NPSGlobal.Instance;

            //get the area polygon for this survey
            BoundaryPoly = GetSurveyBoundary(Convert.ToString(SurveyID), ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            ExcludeFC = GetFeatureClass(NPS.LYR_EXCLUDED_AREAS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            RandPointsFC = GetFeatureClass(NPS.LYR_RANDOMPOINTS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            SurveyIDFieldIndex = RandPointsFC.FindField("SurveyID");
            BatchIDFieldIndex = RandPointsFC.FindField("BATCH_ID");

            //get the next vailable batch id for this survey's grid points
            NextBatcID = GetHighestFieldValue(NPS.LYR_RANDOMPOINTS,
                                              "BATCH_ID", "SurveyID=" + SurveyID, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;
            NextBatcID++;

            if (StartX != -1 && StartY != -1)
            {
                StartPoint = new PointClass();
                StartPoint.SpatialReference = GetDefaultSpatialReference();
                StartPoint.PutCoords(StartX, StartY);

                if (((IRelationalOperator) BoundaryPoly).Contains(StartPoint) == false)
                {
                    ErrorMessage = "The starting point must be within the survey boundary area.";
                    return -1;
                }

                NewPoints = GenerateGridPointsFromCenter(StartPoint, BoundaryPoly.Envelope,
                                                         PointSpacing, ref GridPointTotal, ref ErrorMessage);
            }
            else
            {
                NewPoints = GenerateGridPointsFromEnve(BoundaryPoly.Envelope, PointSpacing,
                                                       ref GridPointTotal, ref ErrorMessage);
            }

            if (string.IsNullOrEmpty(ErrorMessage) == false) return -1;

            InsertCursor = RandPointsFC.Insert(true);
            InsertBuffer = RandPointsFC.CreateFeatureBuffer();


            for (int GridPointIndex = 0; GridPointIndex < GridPointTotal; GridPointIndex++)
            {
                IPoint NewPoint = NewPoints[GridPointIndex];

                if (((IRelationalOperator) BoundaryPoly).Contains(NewPoint) == false) continue;


                if (HasRelationshipWithFC(NewPoint, esriSpatialRelEnum.esriSpatialRelWithin, ExcludeFC,
                                          "SurveyID=" + SurveyID))
                    continue;

                InsertBuffer.Shape = ((IClone) NewPoint).Clone() as IPoint;
                InsertBuffer.set_Value(SurveyIDFieldIndex, SurveyID);
                InsertBuffer.set_Value(BatchIDFieldIndex, NextBatcID);

                InsertCursor.InsertFeature(InsertBuffer);
            }
            InsertCursor = null;

            return NextBatcID;
        }

        /// <summary>
        ///     generate grid points form envelope
        /// </summary>
        public static IPoint[] GenerateGridPointsFromEnve(IEnvelope Boundary,
                                                          double PointSpacing, ref int GridPointIndex,
                                                          ref string ErrorMessage)
        {
            double upperboundx, lowerboundx, upperboundy, lowerboundy, IncrementX, IncrementY;
            IRelationalOperator npsRelOperator;
            IPoint npsPointTest;
            IPoint[] NewPoints;
            int Counter = 0;


            //set bounds (extent) for random number generation
            upperboundx = Boundary.XMax;
            lowerboundx = Boundary.XMin;
            upperboundy = Boundary.YMax;
            lowerboundy = Boundary.YMin;

            GridPointIndex = 0;
            NewPoints = new IPoint[500000];

            //set the boundary polygon to the IRelationalOperator interface to access 'Contains' function
            npsRelOperator = Boundary as IRelationalOperator;

            //starting location
            IncrementX = lowerboundx;
            IncrementY = upperboundy;


            //loops until the number of valid points reach requested total points
            while (Counter < 10000000)
            {
                if (IncrementX > upperboundx)
                {
                    if (IncrementY < lowerboundy) break;

                    IncrementY = IncrementY - PointSpacing;
                    IncrementX = lowerboundx;

                    npsPointTest = new PointClass();
                    //npsPointTest.SpatialReference = Util.GetDefaultSpatialReference();
                    npsPointTest.PutCoords(IncrementX, IncrementY);
                    IncrementX = IncrementX + PointSpacing;
                }
                else
                {
                    npsPointTest = new PointClass();
                    //npsPointTest.SpatialReference = Util.GetDefaultSpatialReference();
                    npsPointTest.PutCoords(IncrementX, IncrementY);
                    IncrementX = IncrementX + PointSpacing;
                }


                if (npsRelOperator.Contains(npsPointTest))
                {
                    NewPoints[GridPointIndex] = npsPointTest;
                    GridPointIndex++;
                }

                Counter = Counter + 1;
            }


            return NewPoints;
        }

        /// <summary>
        ///     generate grid points starting from the specified center point
        /// </summary>
        public static IPoint[] GenerateGridPointsFromCenter(IPoint centerPoint, IEnvelope Boundary,
                                                            double Spacing, ref int GridPointIndex,
                                                            ref string ErrorMessage)
        {
            int PntPerPhase, Unreachable, Counter = 1, PointPerPhaseCount = 1;
            double DistFromStart;
            bool GoRoundAgain = true;
            IPoint[] ThesePoints;
            IPoint OtherPoint = null, NewPoint = null;


            PntPerPhase = 1;
            Unreachable = 9000000;
            Counter = 0;
            DistFromStart = Spacing;
            PointPerPhaseCount = 1;

            ThesePoints = new IPoint[500000];

            //add center point to array
            ThesePoints[GridPointIndex] = centerPoint;
            GridPointIndex++;

            //Both the starting distance below the center point and the
            //number of points in each direction of a cycle are adjusted
            //each cycle to continuously move outward
            for (Counter = 1; Counter <= Unreachable; Counter++)
            {
                //create point directly below center point to begin cycle
                NewPoint = new PointClass();
                NewPoint.X = centerPoint.X;
                NewPoint.Y = centerPoint.Y - DistFromStart;
                ThesePoints[GridPointIndex] = NewPoint;
                GridPointIndex++;


                //add points moving right
                for (PointPerPhaseCount = 1; PointPerPhaseCount <= PntPerPhase; PointPerPhaseCount++)
                {
                    OtherPoint = new PointClass();
                    OtherPoint.X = NewPoint.X + (Spacing*PointPerPhaseCount);
                    OtherPoint.Y = NewPoint.Y;
                    ThesePoints[GridPointIndex] = OtherPoint;
                    GridPointIndex++;

                    if (GoRoundAgain == false) GoRoundAgain = ((IRelationalOperator) Boundary).Contains(OtherPoint);
                }
                NewPoint = OtherPoint;


                //add points moving up
                for (PointPerPhaseCount = 1; PointPerPhaseCount <= (PntPerPhase*2); PointPerPhaseCount++)
                {
                    OtherPoint = new PointClass();
                    OtherPoint.X = NewPoint.X;
                    OtherPoint.Y = NewPoint.Y + (Spacing*PointPerPhaseCount);
                    ThesePoints[GridPointIndex] = OtherPoint;
                    GridPointIndex++;

                    if (GoRoundAgain == false) GoRoundAgain = ((IRelationalOperator) Boundary).Contains(OtherPoint);
                }
                NewPoint = OtherPoint;

                //add points moving left
                for (PointPerPhaseCount = 1; PointPerPhaseCount <= (PntPerPhase*2); PointPerPhaseCount++)
                {
                    OtherPoint = new PointClass();
                    OtherPoint.X = NewPoint.X - (Spacing*PointPerPhaseCount);
                    OtherPoint.Y = NewPoint.Y;
                    ThesePoints[GridPointIndex] = OtherPoint;
                    GridPointIndex++;

                    if (GoRoundAgain == false) GoRoundAgain = ((IRelationalOperator) Boundary).Contains(OtherPoint);
                }
                NewPoint = OtherPoint;

                //add points moving down
                for (PointPerPhaseCount = 1; PointPerPhaseCount <= (PntPerPhase*2); PointPerPhaseCount++)
                {
                    OtherPoint = new PointClass();
                    OtherPoint.X = NewPoint.X;
                    OtherPoint.Y = NewPoint.Y - (Spacing*PointPerPhaseCount);
                    ThesePoints[GridPointIndex] = OtherPoint;

                    if (GoRoundAgain == false) GoRoundAgain = ((IRelationalOperator) Boundary).Contains(OtherPoint);
                }
                NewPoint = OtherPoint;

                //add points moving right (back to origin)
                for (PointPerPhaseCount = 1; PointPerPhaseCount <= (PntPerPhase - 1); PointPerPhaseCount++)
                {
                    OtherPoint = new PointClass();
                    OtherPoint.X = NewPoint.X + (Spacing*PointPerPhaseCount);
                    OtherPoint.Y = NewPoint.Y;
                    ThesePoints[GridPointIndex] = OtherPoint;
                    GridPointIndex++;

                    if (GoRoundAgain == false) GoRoundAgain = ((IRelationalOperator) Boundary).Contains(OtherPoint);
                }
                NewPoint = OtherPoint;

                //as long as at least one point in the cycle was within the boundary envelope,
                //add another cycle of points
                if (GoRoundAgain == false) break;

                GoRoundAgain = false;

                PntPerPhase = PntPerPhase + 1;
                DistFromStart = DistFromStart + Spacing;
            }

            return ThesePoints;
        }

        /// <summary>
        ///     check if a shape has the specified relationship with any features in the specified feature class
        /// </summary>
        public static bool HasRelationshipWithFC(IGeometry GeomToCheck, esriSpatialRelEnum SpatRel,
                                                 IFeatureClass FeatureClassToSearchIn, string WhereClause)
        {
            ISpatialFilter ThisSpatialFilter;
            IFeatureCursor ThisFeatureCursor;
            IFeature CheckFeature;
            bool DoesRelate = false;

            ThisSpatialFilter = new SpatialFilterClass();
            ThisSpatialFilter.set_GeometryEx(GeomToCheck, false);
            ThisSpatialFilter.GeometryField = FeatureClassToSearchIn.ShapeFieldName;
            ThisSpatialFilter.SpatialRel = SpatRel;
            ThisSpatialFilter.WhereClause = WhereClause;

            try
            {
                ThisFeatureCursor = FeatureClassToSearchIn.Search(ThisSpatialFilter, false);
                CheckFeature = ThisFeatureCursor.NextFeature();
                DoesRelate = CheckFeature == null ? false : true;
            }
            catch
            {
            }


            ThisFeatureCursor = null;
            CheckFeature = null;

            return DoesRelate;
        }

        /// <summary>
        ///     get a random number between the specified range
        /// </summary>
        public static double GetRandomNumber()
        {
            return Convert.ToDouble(NPSGlobal.Instance.Randomizer.Next(0, 100))/100.0f;
        }

        /// <summary>
        ///     used by the GenerateFlatAreas and GenerateExcludedAreas logic to add in at least one polygon if their
        ///     processes result in zero polygons being generated
        /// </summary>
        public static void CreateFillerPolygon(IFeatureClass ThisFeatureClass, int SurveyID, ref string ErrorMessage)
        {
            IFeatureCursor ThisFCursor;
            IFeatureBuffer ThisFBuffer;
            IQueryFilter ThisQueryFilter;
            IPolygon BoundarPoly;
            int SurveyIDIndex;

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = "SurveyID=" + SurveyID;

            SurveyIDIndex = ThisFeatureClass.FindField("SurveyID");
            if (SurveyIDIndex == -1)
            {
                ErrorMessage = "Could not find SurveyID field in " + ((IDataset) ThisFeatureClass).Name +
                               " FeatureClass.";
                return;
            }

            if (ThisFeatureClass.FeatureCount(ThisQueryFilter) > 0) return;

            BoundarPoly = GetSurveyBoundary(Convert.ToString(SurveyID), ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            try
            {
                BoundarPoly = EnvelopeToPolygon(BoundarPoly.Envelope.UpperLeft.Envelope);

                ThisFCursor = ThisFeatureClass.Insert(true);
                ThisFBuffer = ThisFeatureClass.CreateFeatureBuffer();

                ThisFBuffer.Shape = BoundarPoly;
                ThisFBuffer.set_Value(SurveyIDIndex, SurveyID);

                ThisFCursor.InsertFeature(ThisFBuffer);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while inserting filler shape for "
                               + ((IDataset) ThisFeatureClass).Name + " FeatureClass. " + ex.Message;
            }

            ThisFCursor = null;
        }

        /// <summary>
        ///     clips one feature class by another and returns the resulting feature class. the resulting featureclass
        ///     is a temp feature class and will need to be deleted after it's features have been processed
        /// </summary>
        public static IFeatureClass GP_Clip_analysis(string TempClipFCName, IFeatureClass InputFC,
                                                     IFeatureClass ClipFC, ref string ErrorMessage)
        {
            IVariantArray GPParams;
            IGeoProcessor ThisGeoProcessor = null;
            IFeatureClass TempClipFC = null;
            NPSGlobal NPS;

            NPS = NPSGlobal.Instance;


            try
            {
                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                GPParams.Add(InputFC);
                GPParams.Add(ClipFC);
                GPParams.Add(Path.Combine(NPS.DatabasePath, TempClipFCName));

                ThisGeoProcessor.Execute("Clip_analysis", GPParams, null);

                DeleteLayerFromMap(TempClipFCName);

                TempClipFC = GetFeatureClass(TempClipFCName, ref ErrorMessage);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:Clip_analysis\r\n"
                                             + "Params:{0},{1},{2}\r\nException:{3}", ((IDataset) InputFC).Name,
                                             ((IDataset) ClipFC).Name,
                                             Path.Combine(NPS.DatabasePath, TempClipFCName), ex.Message);
                ThisGeoProcessor = null;
                return null;
            }

            ThisGeoProcessor = null;

            return TempClipFC;
        }

        /// <summary>
        ///     create a new feature class with buffer polygons of the input feature class
        /// </summary>
        public static IFeatureClass GP_Buffer_analysis(IFeatureClass FCToBuffer, string OutFCPathAndName,
                                                       double BufferDistance, string BufferSide, ref string ErrorMessage)
        {
            IVariantArray GPParams;
            IGeoProcessor ThisGeoProcessor = null;
            IFeatureClass ResultsFC = null;
            string FCName;


            try
            {
                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                GPParams.Add(FCToBuffer);
                GPParams.Add(OutFCPathAndName);
                GPParams.Add(BufferDistance);
                GPParams.Add(BufferSide.ToUpper());
                GPParams.Add("FLAT");
                GPParams.Add("NONE");

                ThisGeoProcessor.Execute("Buffer_analysis", GPParams, null);


                FCName = OutFCPathAndName.Substring(OutFCPathAndName.LastIndexOf("\\") + 1);
                ResultsFC = GetFeatureClass(FCName, ref ErrorMessage);

                DeleteLayerFromMap(FCName);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:Buffer_analysis\r\n"
                                             + "Params:{0},{1},{2},{3},{4}\r\nException:{5}",
                                             ((IDataset) FCToBuffer).Name,
                                             OutFCPathAndName, BufferDistance, BufferSide, "NONE", ex.Message);
                ThisGeoProcessor = null;
                return null;
            }

            ThisGeoProcessor = null;

            return ResultsFC;
        }

        /// <summary>
        ///     create a new feature class with buffer polygons of the input feature class
        /// </summary>
        public static IFeatureClass GP_Union_analysis(IFeatureClass FirstFC, IFeatureClass SecondFC,
                                                      string OutFCPathAndName, ref string ErrorMessage)
        {
            IVariantArray GPParams;
            IGeoProcessor ThisGeoProcessor = null;
            IFeatureClass ResultsFC = null;
            string FCName, FCList;


            try
            {
                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                FCList = ((IDataset) FirstFC).FullName + ";" + ((IDataset) SecondFC).FullName;

                FCList = Path.Combine(((IDataset) FirstFC).Workspace.PathName, ((IDataset) FirstFC).Name)
                         + ";" + Path.Combine(((IDataset) FirstFC).Workspace.PathName, ((IDataset) SecondFC).Name);

                GPParams.Add(FCList);
                GPParams.Add(OutFCPathAndName);
                GPParams.Add("ALL");

                ThisGeoProcessor.Execute("Union_analysis", GPParams, null);


                FCName = OutFCPathAndName.Substring(OutFCPathAndName.LastIndexOf("\\") + 1);
                ResultsFC = GetFeatureClass(FCName, ref ErrorMessage);

                DeleteLayerFromMap(FCName);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:Union_analysis\r\n"
                                             + "Params:{0},{1},{2},{3}\r\nException:{4}", ((IDataset) FirstFC).Name,
                                             ((IDataset) SecondFC).Name, OutFCPathAndName, "ALL", ex.Message);
                ThisGeoProcessor = null;
                return null;
            }

            ThisGeoProcessor = null;

            return ResultsFC;
        }

        public static void ExecuteGP(string GPToolName, List<object> Params, ref string ErrorMessage)
        {
            IVariantArray GPParams;
            IGeoProcessor ThisGeoProcessor = null;


            try
            {
                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                foreach (object ThisParam in Params)
                    GPParams.Add(ThisParam);


                ThisGeoProcessor.Execute(GPToolName, GPParams, null);
            }
            catch
            {
            }

            ThisGeoProcessor = null;
        }

        /// <summary>
        ///     export a featureclass as a shapefile
        /// </summary>
        public static void GP_FeatureclassToShapefile_conversion(IFeatureClass ThisFC, string ShapeFileFolderPath,
                                                                 string WhereClause, ref string ErrorMessage)
        {
            IVariantArray GPParams;
            IGeoProcessor ThisGeoProcessor = null;
            IFeatureLayer TempLayer = null;


            try
            {
                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                TempLayer = new FeatureLayerClass();
                TempLayer.FeatureClass = ThisFC;
                ((ITableDefinition) TempLayer).DefinitionExpression = WhereClause;
                TempLayer.Name = ((IDataset) ThisFC).Name;

                GPParams.Add(TempLayer);
                GPParams.Add(ShapeFileFolderPath);
                ThisGeoProcessor.OverwriteOutput = true;
                ThisGeoProcessor.Execute("FeatureclassToShapefile_conversion", GPParams, null);
            }
            catch (Exception ex)
            {
                ErrorMessage = string.Format("Geoprocessor error:\r\nTask:Clip_analysis\r\n"
                                             + "Params:{0},{1}\r\nException:{2}", ((IDataset) ThisFC).Name,
                                             ShapeFileFolderPath, ex.Message);
            }

            ThisGeoProcessor = null;
        }

        /// <summary>
        ///     try to delete a folder and if we can't delete it, try to empty it out for resuse
        /// </summary>
        public static void DeleteOrEmptyFolder(string FolderPath, bool TryEmptying, ref bool IsReusing,
                                               ref string ErrorMessage)
        {
            IsReusing = false;

            try
            {
                Directory.Delete(FolderPath, true);
            }
            catch (Exception ex)
            {
                if (TryEmptying)
                {
                    try
                    {
                        foreach (string FilePath in Directory.GetFiles(FolderPath))
                        {
                            File.Delete(FilePath);
                        }
                    }
                    catch (Exception ex2)
                    {
                        ErrorMessage = "Could not remove existing Survey folder at path " + FolderPath + ". " +
                                       ex2.Message;
                        return;
                    }
                    IsReusing = true;
                }
                else
                {
                    ErrorMessage = "Could not remove existing Survey folder at path " + FolderPath + ". " + ex.Message;
                }
            }
        }

        /// <summary>
        ///     try creating a folder
        /// </summary>
        public static void CreateDirectory(string FolderPath, ref string ErrorMessage)
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Could not create folder at path " + FolderPath + ". " + ex.Message;
            }
        }

        /// <summary>
        ///     recursively copy a folder to a specified destination folder
        /// </summary>
        public static void CopyFolder(string SourceFolder, string DestinationFolder, ref string ErrorMessage)
        {
            string[] folders, files;
            string LocalError = "";

            if (SourceFolder == DestinationFolder) return;

            if (Directory.Exists(DestinationFolder) == false)
                CreateDirectory(DestinationFolder, ref ErrorMessage);

            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            files = Directory.GetFiles(SourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file) ?? "";
                string dest = Path.Combine(DestinationFolder, name);
                CopyFile(file, dest, ref LocalError);
            }

            folders = Directory.GetDirectories(SourceFolder);
            foreach (string folder in folders)
            {
                if (Path.GetFileName(folder) == ".svn") continue;
                string name = Path.GetFileName(folder) ?? "";
                string dest = Path.Combine(DestinationFolder, name);
                CopyFolder(folder, dest, ref ErrorMessage);
            }
        }

        /// <summary>
        ///     copy a file from one folder to the next, overrides a file if it already exists in the desitnation folder
        /// </summary>
        public static void CopyFile(string FilePath, string DestinationFolder, ref string ErrorMessage)
        {
            try
            {
                File.Copy(FilePath, DestinationFolder, true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while copying file "
                               + FilePath + " to destination " + DestinationFolder + ". " + ex.Message;
            }
        }

        /// <summary>
        ///     make sure the Survey folder is valid for import by the ArcMap extension
        /// </summary>
        public static IWorkspace ValidateSurveyFolder(string SurveyFolderPath, ref string ErrorMessage)
        {
            string[] NPSFCNames;
            NPSGlobal NPS;
            IWorkspace ShapeFileWS;
            NPS = NPSGlobal.Instance;

            //get the survey folder path and validate it
            SurveyFolderPath = Path.Combine(SurveyFolderPath, "Survey");
            if (Directory.Exists(SurveyFolderPath) == false)
            {
                ErrorMessage = "There is no Survey folder at the specified path: " + SurveyFolderPath;
                return null;
            }

            //make sure this is a valid shapefile workspace
            ShapeFileWS = OpenShapeFileWorkspace(SurveyFolderPath, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return null;

            NPSFCNames = new[]
                {
                    NPS.LYR_GENERATED_TRANSECTS, NPS.LYR_ANIMALS,
                    NPS.LYR_TRACKLOG, NPS.LYR_HORIZON, NPS.LYR_GPSPOINTLOG
                };


            //make sure that each feature class is present in the survey folder
            foreach (string FCName in NPSFCNames)
            {
                GetFeatureClass(FCName, ShapeFileWS, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    ErrorMessage = "The survey folder is missing a Shapefile. " + ErrorMessage;
                    return null;
                }
            }

            return ShapeFileWS;
        }

        /// <summary>
        ///     check if the featureclass is empty
        /// </summary>
        public static bool HasRecordsOf(IFeatureClass ThisFC, string WhereClause)
        {
            IQueryFilter ThisQF;
            IFeature ThisFeature;
            IFeatureCursor ThisFCursor;
            bool HasRecords = false;


            try
            {
                ThisQF = new QueryFilterClass();
                ThisQF.WhereClause = WhereClause;

                ThisFCursor = ThisFC.Search(ThisQF, false);
                ThisFeature = ThisFCursor.NextFeature();
                HasRecords = ThisFeature == null ? false : true;
                ThisFCursor = null;
            }
            catch
            {
            }

            return HasRecords;
        }

        public static bool HasRecordsOf(string FCName, IWorkspace ThisWS, string WhereClause)
        {
            IFeatureClass ThisFC;
            string ErrorMessage = "";

            ThisFC = GetFeatureClass(FCName, ThisWS, ref ErrorMessage);
            if (ErrorMessage != "") return false;

            return HasRecordsOf(ThisFC, WhereClause);
        }

        public static bool HasRecordsOf(string FCName, string WherehClause)
        {
            return HasRecordsOf(FCName, NPSGlobal.Instance.Workspace, WherehClause);
        }

        /// <summary>
        ///     count the number of features in a feature class
        /// </summary>
        public static int FeatureCount(IFeatureClass ThisFC, string WhereClause)
        {
            IQueryFilter ThisQF;

            ThisQF = new QueryFilterClass();
            ThisQF.WhereClause = WhereClause;

            try
            {
                return ThisFC.FeatureCount(ThisQF);
            }
            catch
            {
            }

            return -1;
        }

        /// <summary>
        ///     count the number of features in a feature class
        /// </summary>
        public static int FeatureCount(string FCName, IWorkspace ThisWS, string WhereClause)
        {
            IFeatureClass ThisFC;
            string ErrorMessage = "";

            ThisFC = GetFeatureClass(FCName, ThisWS, ref ErrorMessage);
            if (ErrorMessage != "") return -1;

            return FeatureCount(ThisFC, WhereClause);
        }

        public static int FeatureCount(string FCName, string WhereClause)
        {
            return FeatureCount(FCName, NPSGlobal.Instance.Workspace, WhereClause);
        }

        /// <summary>
        ///     specific logic for copying over gps points from the Survey folder's gps log into the
        ///     database gps log
        /// </summary>
        public static void CopyGPSPointLog(IFeatureClass NewGPSPointsFC, ref string ErrorMessage)
        {
            IFeatureCursor FromFCCursor, ToFCCursor;
            IFeatureBuffer ToFCBuffer;
            IFeature FromFCFeature;
            IFeatureClass GPSPointsFC;
            int FromFieldTotal = 0,
                FromFieldCount = 0,
                CurrentFieldIndex = 0,
                SurveyID = 0,
                PilotLNamIndex = 0,
                AircraftIndex = 0,
                SurveyIDIndex = 0;
            string PilotLNam = "", Aircraft = "";
            NPSGlobal NPS;


            NPS = NPSGlobal.Instance;

            GPSPointsFC = GetFeatureClass(NPS.LYR_GPSPOINTLOG, ref ErrorMessage);

            SurveyIDIndex = NewGPSPointsFC.FindField("SurveyID");
            AircraftIndex = NewGPSPointsFC.FindField("Aircraft");
            PilotLNamIndex = NewGPSPointsFC.FindField("PILOTLNAM");
            SurveyID = -1;

            FromFCCursor = NewGPSPointsFC.Search(null, false);

            while ((FromFCFeature = FromFCCursor.NextFeature()) != null)
            {
                SurveyID = (int) SafeConvert(FromFCFeature.get_Value(SurveyIDIndex), typeof (int));
                PilotLNam = (string) SafeConvert(FromFCFeature.get_Value(PilotLNamIndex), typeof (string));
                Aircraft = (string) SafeConvert(FromFCFeature.get_Value(AircraftIndex), typeof (string));
                if (SurveyID > -1 && Aircraft != "" && PilotLNam != "") break;
            }
            FromFCCursor = null;

            if (SurveyID == -1) return;

            ToFCBuffer = GPSPointsFC.CreateFeatureBuffer();
            ToFCCursor = GPSPointsFC.Insert(true);

            FromFCCursor = NewGPSPointsFC.Search(null, false);

            while ((FromFCFeature = FromFCCursor.NextFeature()) != null)
            {
                ToFCBuffer.Shape = FromFCFeature.ShapeCopy;


                FromFieldTotal = NewGPSPointsFC.Fields.FieldCount;
                for (int FieldIndex = 0; FieldIndex < FromFieldTotal; FieldIndex++)
                {
                    CurrentFieldIndex = GPSPointsFC.FindField(NewGPSPointsFC.Fields.get_Field(FieldIndex).Name);
                    if (CurrentFieldIndex > -1 && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Name != "SHAPE_AREA"
                        && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Name != "SHAPE_LENGTH"
                        && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Name != "OBJECTID"
                        && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Name != "OBJECTID_1"
                        && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Type != esriFieldType.esriFieldTypeGeometry
                        && GPSPointsFC.Fields.get_Field(CurrentFieldIndex).Type != esriFieldType.esriFieldTypeOID)
                    {
                        ToFCBuffer.set_Value(CurrentFieldIndex, FromFCFeature.get_Value(FromFieldCount));
                    }
                }


                if (ToFCBuffer.Fields.FindField("SurveyID") > -1)
                    ToFCBuffer.set_Value(ToFCBuffer.Fields.FindField("SurveyID"), SurveyID);

                if (ToFCBuffer.Fields.FindField("PILOTLNAM") > -1)
                    ToFCBuffer.set_Value(ToFCBuffer.Fields.FindField("PILOTLNAM"), PilotLNam);

                if (ToFCBuffer.Fields.FindField("AIRCRAFT") > -1)
                    ToFCBuffer.set_Value(ToFCBuffer.Fields.FindField("AIRCRAFT"), Aircraft);

                ToFCCursor.InsertFeature(ToFCBuffer);
            }

            ToFCCursor = null;
            FromFCCursor = null;
        }

        /// <summary>
        ///     get the first valid value from the first row at the specified column
        /// </summary>
        public static string GetFirstRecordValue(IFeatureClass ThisFC, string FieldName, string WhereClause)
        {
            IQueryFilter qFilter;
            IFeature ThisFeature;
            IFeatureCursor ThisFCursor;
            int FieldIndex;
            string FirstValidValue = "";

            FieldIndex = ThisFC.FindField(FieldName);
            if (FieldIndex == -1) return "";

            qFilter = new QueryFilterClass();
            qFilter.WhereClause = WhereClause;

            ThisFCursor = ThisFC.Search(qFilter, false);


            while ((ThisFeature = ThisFCursor.NextFeature()) != null)
            {
                FirstValidValue = (string) SafeConvert(ThisFeature.get_Value(FieldIndex), typeof (string));
                if (string.IsNullOrEmpty(FirstValidValue) == false) break;
            }
            ThisFCursor = null;

            return FirstValidValue;
        }

        public static string GetFirstRecordValue(string LocalFCName, string FieldName, string WhereClause)
        {
            string ErrorMessage = "";

            IFeatureClass ThisFC;

            ThisFC = GetFeatureClass(LocalFCName, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return "";

            return GetFirstRecordValue(ThisFC, FieldName, WhereClause);
        }

        /// <summary>
        ///     check if an object represents a numeric value
        /// </summary>
        public static bool IsNumeric(string ThisValue)
        {
            double val;
            return Double.TryParse(ThisValue, out val);
        }

        /// <summary>
        ///     update main form progress label with an update message
        /// </summary>
        public static void SetProgressMessage(string ThisMessage, bool AdvanceStep)
        {
            if (ThisMessage == "")
            {
                NPSGlobal.Instance.ProgressLabel = null;
                NPSGlobal.Instance.ProgressStepTotal = -1;
                NPSGlobal.Instance.ProgressStepIndex = -1;
            }

            if (NPSGlobal.Instance.ProgressStepTotal > -1 && NPSGlobal.Instance.ProgressStepIndex > -1)
            {
                ThisMessage = "(" + NPSGlobal.Instance.ProgressStepIndex + " of "
                              + NPSGlobal.Instance.ProgressStepTotal + ") " + ThisMessage;
                if (AdvanceStep) NPSGlobal.Instance.ProgressStepIndex++;
            }

            if (ThisMessage != "") ThisMessage += "........";

            if (NPSGlobal.Instance.ProgressLabel == null)
            {
                if (NPSGlobal.Instance.MainTransectForm != null)
                    NPSGlobal.Instance.MainTransectForm.lblProgressMessage.Text = ThisMessage;
            }
            else
                NPSGlobal.Instance.ProgressLabel.Text = ThisMessage;

            Application.DoEvents();
        }

        public static void SetProgressMessage(string ThisMessage, int StepTotal)
        {
            NPSGlobal.Instance.ProgressStepTotal = StepTotal;
            NPSGlobal.Instance.ProgressStepIndex = 1;
            SetProgressMessage(ThisMessage);
        }

        public static void SetProgressMessage(string ThisMessage)
        {
            SetProgressMessage(ThisMessage, true);
        }

        /// <summary>
        ///     get a default value in the defaultvalues table
        /// </summary>
        public static string GetArcPadDefaultValue(string DefName, ref string ErrorMessage)
        {
            NPSGlobal NPS;
            ITable ArcPadDefaults;
            IQueryFilter ThisFilter;
            ICursor ThisCursor;
            IRow ThisRow;
            string RetValue = "";
            int DefValueFieldIndex, DefNameFieldIndex;


            NPS = NPSGlobal.Instance;

            ArcPadDefaults = GetDefaultValuesTable(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return "";

            DefValueFieldIndex = ArcPadDefaults.FindField("DEFVALUE");
            DefNameFieldIndex = ArcPadDefaults.FindField("DEFNAME");

            if (DefValueFieldIndex == -1 || DefNameFieldIndex == -1)
            {
                ErrorMessage = NPS.TBL_DEFAULTVALUES + " is missing default values fields";
                return "";
            }

            try
            {
                ThisFilter = new QueryFilterClass();
                ThisFilter.WhereClause = "DEFNAME='" + DefName + "'";

                ThisCursor = ArcPadDefaults.Search(ThisFilter, false);
                ThisRow = ThisCursor.NextRow();

                if (ThisRow != null)
                    RetValue = (string) SafeConvert(ThisRow.get_Value(DefValueFieldIndex), typeof (string));

                ThisCursor = null;
            }
            catch
            {
            }

            return RetValue;
        }

        /// <summary>
        ///     set a default value in the defaultvalues table
        /// </summary>
        public static void SetArcPadDefaultValue(string DefName, string DefValue, ref string ErrorMessage)
        {
            NPSGlobal NPS;
            ITable ArcPadDefaults;
            IQueryFilter ThisFilter;
            ICursor ThisCursor;
            IRow ThisRow;
            int DefValueFieldIndex, DefNameFieldIndex;


            NPS = NPSGlobal.Instance;

            ArcPadDefaults = GetDefaultValuesTable(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            DefValueFieldIndex = ArcPadDefaults.FindField("DEFVALUE");
            DefNameFieldIndex = ArcPadDefaults.FindField("DEFNAME");

            if (DefValueFieldIndex == -1 || DefNameFieldIndex == -1)
            {
                ErrorMessage = NPS.TBL_DEFAULTVALUES + " is missing default values fields";
                return;
            }

            try
            {
                ThisFilter = new QueryFilterClass();
                ThisFilter.WhereClause = "DEFNAME='" + DefName + "'";

                ThisCursor = ArcPadDefaults.Update(ThisFilter, false);
                ThisRow = ThisCursor.NextRow();

                if (ThisRow != null)
                {
                    ThisRow.set_Value(DefValueFieldIndex, DefValue);
                    ThisCursor.UpdateRow(ThisRow);
                }

                ThisCursor = null;
            }
            catch
            {
            }
        }

        /// <summary>
        ///     get defaultvalues table
        /// </summary>
        public static ITable GetDefaultValuesTable(ref string ErrorMessage)
        {
            //string DBFolderPath;
            NPSGlobal NPS;

            NPS = NPSGlobal.Instance;

            //DBFolderPath = System.IO.Path.GetDirectoryName(NPS.DatabasePath);
            //return Util.GetTable(NPS.TBL_DEFAULTVALUES, DBFolderPath, ref ErrorMessage);

            return GetTable(NPS.TBL_DEFAULTVALUES, ref ErrorMessage);
        }

        /// <summary>
        ///     export default values table to a standonly dbase file at the specified file path
        /// </summary>
        public static void ExportDefaultValuesTable(string ExportTablePath, ref string ErrorMessage)
        {
            IGeoProcessor ThisGeoProcessor = null;
            IVariantArray GPParams;
            ITable DefaultValuesTable;
            IWorkspace ShapeFileWorkspace;

            try
            {
                DefaultValuesTable = GetDefaultValuesTable(ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false) return;

                ShapeFileWorkspace = OpenShapeFileWorkspace(ExportTablePath, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false) return;

                ThisGeoProcessor = new GeoProcessor();
                GPParams = new VarArrayClass();

                GPParams.Add(DefaultValuesTable);
                GPParams.Add(ShapeFileWorkspace);
                GPParams.Add("defaultvalues.dbf");

                ThisGeoProcessor.Execute("TableToTable_conversion", GPParams, null);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while exporting defaultvalues table. " + ex.Message;
            }
        }

        /// <summary>
        ///     create polyline, point or polygon (simple) featureclass
        /// </summary>
        public static IFeatureClass CreateWorkspaceFeatureClass(string FCName, IWorkspace featWorkspace,
                                                                esriGeometryType geomType, IFields pfields,
                                                                ISpatialReference pSR, ref string ErrorMessage)
        {
            IField pField;
            UIDClass pCLSID;
            IGeometryDef pGeomDef;

            pCLSID = new UIDClass();
            pCLSID.Value = "esricore.Feature";

            //create geom def
            pGeomDef = new GeometryDefClass();
            ((IGeometryDefEdit) pGeomDef).GeometryType_2 = geomType;
            ((IGeometryDefEdit) pGeomDef).SpatialReference_2 = pSR;

            //create the geometry field
            pField = new FieldClass();
            ((IFieldEdit) pField).Name_2 = "SHAPE";
            ((IFieldEdit) pField).AliasName_2 = "SHAPE";
            ((IFieldEdit) pField).Type_2 = esriFieldType.esriFieldTypeGeometry;
            ((IFieldEdit) pField).GeometryDef_2 = pGeomDef;

            //add geometry to field collection
            ((IFieldsEdit) pfields).AddField(pField);

            // create the object id field
            pField = new FieldClass();
            ((IFieldEdit) pField).Name_2 = "OBJECTID";
            ((IFieldEdit) pField).AliasName_2 = "OBJECTID";
            ((IFieldEdit) pField).Type_2 = esriFieldType.esriFieldTypeOID;
            ((IFieldsEdit) pfields).AddField(pField);

            try
            {
                //create the feature class
                return ((IFeatureWorkspace) featWorkspace).CreateFeatureClass(FCName, pfields, pCLSID,
                                                                              null, esriFeatureType.esriFTSimple,
                                                                              "SHAPE", "");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error occured while create FeatureClass. " + ex.Message;
                return null;
            }
        }

        /// <summary>
        ///     add raster or featureclass to map as layer
        /// </summary>
        public static void AddDataToMapAsLayer(IGeoDataset RasterDSOrFeatureClass, string LayerName)
        {
            ILayer ThisLayer = null;

            if (RasterDSOrFeatureClass is IRasterDataset)
            {
                //add rasterdataset as raster layer
                ThisLayer = new RasterLayerClass();
                ((IRasterLayer) ThisLayer).CreateFromDataset(RasterDSOrFeatureClass as IRasterDataset);
            }

            if (RasterDSOrFeatureClass is IFeatureClass)
            {
                //add feature class to a feature layer
                ThisLayer = new FeatureLayerClass();
                ((IFeatureLayer) ThisLayer).FeatureClass = RasterDSOrFeatureClass as IFeatureClass;
            }

            ThisLayer.Name = LayerName;

            //Add the raster layer to ArcMap
            NPSGlobal.Instance.Document.FocusMap.AddLayer(ThisLayer);
            NPSGlobal.Instance.Document.ActiveView.Refresh();
        }

        /// <summary>
        ///     get layer by name from the current arcmap instace
        /// </summary>
        public static ILayer GetLayer(string LayerName)
        {
            IEnumLayer LayerList;
            ILayer ThisLayer;


            LayerList = NPSGlobal.Instance.Document.FocusMap.get_Layers(null, true);

            while ((ThisLayer = LayerList.Next()) != null)
            {
                if (ThisLayer.Name == LayerName) return ThisLayer;
            }
            return null;
        }

        public static string RunSystemChecks()
        {
            string DEMFilePath,
                   ErrorMessage = "",
                   MXDPath,
                   ArcGISVersion,
                   ScratchWSPath = "",
                   CurrentWSPath = "",
                   GPError = "";
            var CheckResults = new StringBuilder();
            RegistryKey RegKey;
            IGeoProcessor ThisGeoProcessor = null;
            List<string> WorkspacePaths;
            IGeoDataset GeoDS;
            IDataset ThisDS;
            NPSGlobal NPS;
            bool IsOK;


            NPS = NPSGlobal.Instance;

            //ArcMap version and service packs
            //====================
            try
            {
                CheckResults.Append("ArcGIS Version:");
                CheckResults.Append("\r\n");

                RegKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\ESRI\ArcInfo\Desktop\8.0");
                ArcGISVersion = RegKey.GetValue("RealVersion", "") as string;
            }
            catch
            {
                ArcGISVersion = "!!!Could not read Realersion info!!!";
            }
            CheckResults.Append(ArcGISVersion);
            CheckResults.Append("\r\n\r\n");

            try
            {
                CheckResults.Append("ArcGIS Build:");
                CheckResults.Append("\r\n");

                RegKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\ESRI\ArcInfo\Desktop\8.0");
                ArcGISVersion = RegKey.GetValue("BuildNumber", "") as string;
            }
            catch
            {
                ArcGISVersion = "!!!Could not read Build info!!!";
            }
            CheckResults.Append(ArcGISVersion);
            CheckResults.Append("\r\n\r\n");


            //dem file path check
            //====================
            IsOK = true;

            DEMFilePath = NPS.MainTransectForm.txtDemFileLocation.Text;
            CheckResults.Append("DEM File Path:");
            CheckResults.Append("\r\n");
            CheckResults.Append(DEMFilePath);

            CheckResults.Append("\r\n");
            if (DEMFilePath.Contains(" "))
            {
                IsOK = false;
                CheckResults.Append("!!!Spaces are not allowed in the path to the DEM file!!!");
            }
            if (string.IsNullOrEmpty(DEMFilePath))
            {
                IsOK = false;
                CheckResults.Append("No DEM file set");
            }

            if (IsOK) CheckResults.Append("OK");


            CheckResults.Append("\r\n\r\n");


            //DEM file projection check
            //====================
            CheckResults.Append("DEM file projection:");
            CheckResults.Append("\r\n");

            GeoDS = OpenRasterDataset(DEMFilePath, ref ErrorMessage) as IGeoDataset;
            if (string.IsNullOrEmpty(ErrorMessage) == false || GeoDS == null)
                CheckResults.Append("!!!Could not load DEM file to determine projection!!!");
            else
                CheckResults.Append(GeoDS.SpatialReference.Name);

            ErrorMessage = "";
            CheckResults.Append("\r\n\r\n");


            //MXD projection check
            //====================
            CheckResults.Append("MXD projection:");
            CheckResults.Append("\r\n");
            CheckResults.Append(NPS.Map.SpatialReference.Name);

            CheckResults.Append("\r\n\r\n");


            //Main FCs path check
            //====================
            CheckResults.Append("Main FeatureClasses paths:");
            CheckResults.Append("\r\n");

            CheckResults.Append(NPS.LYR_SURVEY_BOUNDARY + ": ");
            ThisDS = GetFeatureClass(NPS.LYR_SURVEY_BOUNDARY, ref ErrorMessage) as IDataset;
            if (string.IsNullOrEmpty(ErrorMessage) == false || ThisDS == null)
                CheckResults.Append("!!!Could not find FeatureClass!!!");
            else
                CheckResults.Append(Path.Combine(ThisDS.Workspace.PathName, ThisDS.Name));

            ErrorMessage = "";
            CheckResults.Append("\r\n");

            //==========
            CheckResults.Append(NPS.LYR_EXCLUDED_AREAS + ": ");
            ThisDS = GetFeatureClass(NPS.LYR_EXCLUDED_AREAS, ref ErrorMessage) as IDataset;
            if (string.IsNullOrEmpty(ErrorMessage) == false || ThisDS == null)
                CheckResults.Append("!!!Could not find FeatureClass!!!");
            else
                CheckResults.Append(Path.Combine(ThisDS.Workspace.PathName, ThisDS.Name));

            ErrorMessage = "";
            CheckResults.Append("\r\n");

            //==========
            CheckResults.Append(NPS.LYR_RANDOMPOINTS + ": ");
            ThisDS = GetFeatureClass(NPS.LYR_RANDOMPOINTS, ref ErrorMessage) as IDataset;
            if (string.IsNullOrEmpty(ErrorMessage) == false || ThisDS == null)
                CheckResults.Append("!!!Could not find FeatureClass!!!");
            else
                CheckResults.Append(Path.Combine(ThisDS.Workspace.PathName, ThisDS.Name));

            ErrorMessage = "";
            CheckResults.Append("\r\n");

            //==========
            CheckResults.Append(NPS.LYR_GENERATED_TRANSECTS + ": ");
            ThisDS = GetFeatureClass(NPS.LYR_GENERATED_TRANSECTS, ref ErrorMessage) as IDataset;
            if (string.IsNullOrEmpty(ErrorMessage) == false || ThisDS == null)
                CheckResults.Append("!!!Could not find FeatureClass!!!");
            else
                CheckResults.Append(Path.Combine(ThisDS.Workspace.PathName, ThisDS.Name));

            ErrorMessage = "";


            CheckResults.Append("\r\n\r\n");

            //Workspace Paths
            //===================
            CheckResults.Append("Workspaces active in MXD:");
            CheckResults.Append("\r\n");
            WorkspacePaths = GetUniqueWorkspacesInMXD();
            if (WorkspacePaths.Count > 0)
            {
                for (int Index = 0; Index < WorkspacePaths.Count; Index++)
                {
                    CheckResults.Append(WorkspacePaths[Index]);
                    if (Index != WorkspacePaths.Count - 1) CheckResults.Append("\r\n");
                }
            }

            CheckResults.Append("\r\n\r\n");


            try
            {
                ThisGeoProcessor = new GeoProcessor();
            }
            catch (Exception ex)
            {
                GPError = ex.Message;
                ThisGeoProcessor = null;
            }


            //Scratch workspace path check
            //====================
            CheckResults.Append("Scratch Workspace Path:");
            CheckResults.Append("\r\n");

            if (ThisGeoProcessor != null)
            {
                ScratchWSPath = ThisGeoProcessor.GetEnvironmentValue("scratchWorkspace") as string;
                if (ScratchWSPath == null)
                {
                    ScratchWSPath = "NULL";
                    CheckResults.Append(ScratchWSPath);
                }
                else
                {
                    CheckResults.Append(ScratchWSPath);

                    if (ScratchWSPath.Contains(" "))
                        CheckResults.Append("!!!Spaces are not allowed in the scratch workspace path!!!");
                    else
                        CheckResults.Append("OK");
                }
            }
            else
            {
                CheckResults.Append("Could not load geoprocessor. " + GPError);
            }


            CheckResults.Append("\r\n\r\n");


            //Current workspace path check
            //====================
            CheckResults.Append("Current Workspace Path:");
            CheckResults.Append("\r\n");

            if (ThisGeoProcessor != null)
            {
                CurrentWSPath = ThisGeoProcessor.GetEnvironmentValue("workspace") as string;

                if (CurrentWSPath == null)
                {
                    CurrentWSPath = "NULL";
                    CheckResults.Append(CurrentWSPath);
                }
                else
                {
                    CheckResults.Append(CurrentWSPath);

                    if (CurrentWSPath.Contains(" "))
                        CheckResults.Append("!!!Spaces are not allowed in the current workspace path!!!");
                    else
                        CheckResults.Append("OK");
                }
            }
            else
            {
                CheckResults.Append("Could not load geoprocessor. " + GPError);
            }

            CheckResults.Append("\r\n\r\n");
            ThisGeoProcessor = null;


            //Read/Write scratch workspace path check
            //====================
            if (string.IsNullOrEmpty(ScratchWSPath) == false)
            {
                if (ScratchWSPath != "NULL")
                {
                    CheckResults.Append("Scratch Workspace Path Security:");
                    CheckResults.Append("\r\n");

                    if (CheckPathPrivelages(ScratchWSPath, ref ErrorMessage) == false)
                        CheckResults.Append("!!!Failed. " + ErrorMessage + "!!!");
                    else
                        CheckResults.Append("OK");

                    CheckResults.Append("\r\n\r\n");
                }
            }


            //Read/Write current workspace path check
            //====================
            if (string.IsNullOrEmpty(CurrentWSPath) == false)
            {
                if (CurrentWSPath != "NULL")
                {
                    CheckResults.Append("Current Workspace Path Security:");
                    CheckResults.Append("\r\n");

                    if (CheckPathPrivelages(CurrentWSPath, ref ErrorMessage) == false)
                        CheckResults.Append("!!!Failed. " + ErrorMessage + "!!!");
                    else
                        CheckResults.Append("OK");

                    CheckResults.Append("\r\n\r\n");
                }
            }


            //path to dll
            //====================
            CheckResults.Append("Path to DLL:");
            CheckResults.Append("\r\n");
            CheckResults.Append(NPS.DLLPath);

            CheckResults.Append("\r\n\r\n");


            //path to database
            //====================
            IsOK = true;

            CheckResults.Append("Path to Database:");
            CheckResults.Append("\r\n");
            CheckResults.Append(NPS.DatabasePath);
            CheckResults.Append("\r\n");

            if (NPS.DatabasePath.Contains(" "))
            {
                IsOK = false;
                CheckResults.Append("!!!Spaces are not allowed in the path to the NPS.gdb file!!!");
            }
            if (IsOK) CheckResults.Append("OK");


            CheckResults.Append("\r\n\r\n");


            //path to mxd
            //====================
            IsOK = true;

            CheckResults.Append("Path to MXD:");
            CheckResults.Append("\r\n");
            MXDPath = NPS.Application.Templates.get_Item(NPS.Application.Templates.Count - 1);
            CheckResults.Append(MXDPath);
            CheckResults.Append("\r\n");

            if (MXDPath.Contains(" "))
            {
                IsOK = false;
                CheckResults.Append("!!!Spaces are not allowed in the path to the MXD file!!!");
            }
            if (IsOK) CheckResults.Append("OK");

            return CheckResults.ToString();
        }

        public static List<string> GetUniqueWorkspacesInMXD()
        {
            NPSGlobal NPS = NPSGlobal.Instance;
            var WSPaths = new List<string>();

            for (int LayerIndex = 0; LayerIndex < NPS.Map.LayerCount; LayerIndex++)
            {
                ILayer ThisLayer = NPS.Map.get_Layer(LayerIndex);

                if (ThisLayer is IFeatureLayer)
                {
                    IFeatureClass ThisFC = ((IFeatureLayer) ThisLayer).FeatureClass;
                    if (ThisFC != null)
                    {
                        if (WSPaths.IndexOf(((IDataset) ThisFC).Workspace.PathName) == -1)
                            WSPaths.Add(((IDataset) ThisFC).Workspace.PathName);
                    }
                }
            }

            return WSPaths;
        }

        public static bool CheckPathPrivelages(string FolderPath, ref string ErrorMessage)
        {
            Stream ThisStream;
            StreamReader ThisReader;
            StreamWriter ThisWriter;
            string TestFile;

            TestFile = Path.Combine(FolderPath, "test.txt");

            try
            {
                //try create                  
                ThisStream = File.Open(TestFile, FileMode.OpenOrCreate);
                ThisStream.Close();

                //try opening and writing
                using (ThisWriter = new StreamWriter(TestFile))
                {
                    ThisWriter.WriteLine("test");
                }

                //try reading
                using (ThisReader = new StreamReader(TestFile))
                {
                    ThisReader.ReadLine();
                }

                //remove test file
                File.Delete(TestFile);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;

                try
                {
                    File.Delete(TestFile);
                }
                catch
                {
                }

                return false;
            }
        }

        /// <summary>
        ///     get the version number of the current assembly
        /// </summary>
        public static string GetAssemblyVersion()
        {
            Version ThisVersion;


            try
            {
                ThisVersion = Assembly.GetExecutingAssembly().GetName().Version;

                return string.Format("{0}.{1}.{2}",
                                     ThisVersion.Major,
                                     ThisVersion.Minor,
                                     ThisVersion.Build);
            }
            catch
            {
                return "?.?.?";
            }
        }
    }


    public enum SavedValuesAction
    {
        Load,
        Save
    }
}