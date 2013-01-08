using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace NPSTransectTool
{
    public partial class ImportTransRndPntsForm : Form
    {
        NPSGlobal m_NPS;
        Dictionary<string, int> m_SurveysList;
        ISelectionSet2 m_SelectionSet;
        ESRI.ArcGIS.Geometry.esriGeometryType m_GeometryType;
        IFeatureClass m_ImportFC;
        TransectToolForm m_TransectToolForm;

        public ImportTransRndPntsForm(TransectToolForm form)
        {
            InitializeComponent();
            m_TransectToolForm = form;
            m_NPS = NPSGlobal.Instance;
        }

        private void ImportTransRndPntsForm_Load(object sender, EventArgs e)
        {
            string ErrorMessage = "";

            m_SelectionSet = Util.GetFirstNoneNPSSelectionSet(ref m_GeometryType, ref ErrorMessage);
            if (m_SelectionSet == null)
            {
                cboBatches.Enabled = false;
                cboSurveysList.Enabled = false;
            }

            m_SurveysList = Util.GetSurveysList();
            m_SurveysList.Add("[Select Survey]", -1);

            var sortedDict = (from entry in m_SurveysList orderby entry.Key ascending select entry);
            foreach (KeyValuePair<string, int> row in sortedDict)
                cboSurveysList.Items.Add(row.Key);


        }

        private void cboSurveyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int SurveyID;
            string FCName, ErrorMessage = "";
            List<string> BatchIDs;


            FCName = m_GeometryType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline
                ? m_NPS.LYR_GENERATED_TRANSECTS : m_NPS.LYR_RANDOMPOINTS;

            cboBatches.DataSource = null;

            SurveyID = m_SurveysList[(string)cboSurveysList.Text];

            if (SurveyID == -1) return;

            BatchIDs = Util.GetBatchIDs(FCName, Convert.ToString(SurveyID), ref ErrorMessage);
            if (BatchIDs.Count == 0) BatchIDs.Add("1");

            cboBatches.DataSource = BatchIDs;

            if (string.IsNullOrEmpty(ErrorMessage) == false)
                MessageBox.Show(ErrorMessage);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string ErrorMessage = "";
            bool AllOkay = true;
            ISpatialReference DefaultSpatRef, ImportSFSpatRef;
            bool Cancelled = false;
            string NewPath;

            NewPath = Util.OpenESRIDialog(txtBrowseFile.Text, ref Cancelled);
            if (Cancelled == false)
                txtBrowseFile.Text = NewPath;
            else
                return;

            //make sure we got the shapefile
            m_ImportFC = Util.GetShapeFile(txtBrowseFile.Text, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                AllOkay = false;
            }

            if (AllOkay)
            {
                //make sure the shapefile is the right shape type
                if (m_ImportFC.ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint
                    && m_ImportFC.ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show("The selected shapefile must be a point if random points are being "
                    + "imported or polylines if transects are being imported");
                    AllOkay = false;
                }
            }

            if (AllOkay)
            {
                //make sure that the shape file has the same coordinate system as the default nps coordinate system
                ImportSFSpatRef = ((IGeoDataset)m_ImportFC).SpatialReference;
                DefaultSpatRef = Util.GetDefaultSpatialReference();

                if (Util.CompareSpatialReference(DefaultSpatRef, ImportSFSpatRef) == false)
                {
                    MessageBox.Show("(Err) The selected shape file has the coordinate system '" + ImportSFSpatRef.Name
                   + "' which is different from that of the NPS database coordinate system which is '"
                   + DefaultSpatRef.Name + "'.");
                    AllOkay = false;

                }
            }

            //if something went run, clea shapefile
            if (AllOkay == false) m_ImportFC = null;

            //if the shapefile was invalid and we have no selection, turn off ddls
            if (AllOkay == false && m_SelectionSet == null)
            {

                cboBatches.Enabled = false;
                cboSurveysList.Enabled = false;
            }

            if (AllOkay)
            {
                //if everything is good uptil this point, enable dropdownlists
                cboBatches.Enabled = true;
                cboSurveysList.Enabled = true;
                cboSurveysList.SelectedIndex = 0;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            int SurveyID = -1, FirstLoadBatchID = -1, BatchID = -1, NextTransectID = -1;
            string DemFilePath = "", ErrorMessage = "", ResultMessage = "", InternalError = "";
            esriGeometryType ImportType = esriGeometryType.esriGeometryNull;
            IFeatureCursor ImportFCursor = null;
            ICursor ImportCursor = null;
            IFeatureClass TargetFC = null;
            IGeoDataset ThisGeoRasterDS = null;
            int TotalInExcluded = 0, TotalOutsideBndy = 0, TotalPassed = 0, FeatureCount = 0;
            List<int> NewOIDs = new List<int>();
            string WhereClause = "",DEMUnits="";
            double TargetLength = -1;


            //make sure we have a selection set or a shapefule
            if (m_ImportFC == null && m_SelectionSet == null)
            {
                ErrorMessage = @"No Random Points or Transects have been set for import. To select 
                                Random Points or Transects for import either select points or
                                polylines from the map or browse to a Shapefile containing points 
                                or polylines.";
            }

            //get the batchid and surveyid for the new features
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (Util.IsNumeric(cboBatches.Text) == false)
                    ErrorMessage = "Please select a batch id to import to.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                BatchID = Convert.ToInt32(cboBatches.Text);

                SurveyID = m_SurveysList[(string)cboSurveysList.Text];
                if (SurveyID == -1) ErrorMessage = "Please select a survey to import to.";

            }


            //make sure we have a DEM file set
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                DemFilePath = m_TransectToolForm.txtDemFileLocation.Text.Trim();
                if (DemFilePath == "")
                    ErrorMessage = "Please set a DEM file on the Config tab that "
                    + "can be used for the specified Survey.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (m_TransectToolForm.cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage = ErrorMessage + "Please set a valid unit for the DEM file from the Config tab.";
            }

            //validate data
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (m_ImportFC != null)
                    ImportType = m_ImportFC.ShapeType;
                if (m_SelectionSet != null)
                    ImportType = m_GeometryType;

                if (m_ImportFC != null)
                    FeatureCount = m_ImportFC.FeatureCount(null);
                if (m_SelectionSet != null)
                    FeatureCount = m_SelectionSet.Count;
            }


            if (string.IsNullOrEmpty(ErrorMessage))
                if (ImportType == esriGeometryType.esriGeometryPoint)
                    if (FeatureCount < 2) ErrorMessage = "A minimum of 2 random points must be imported at one time.";


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (ImportType == esriGeometryType.esriGeometryPolyline)
                    if (Util.IsNumeric(txtTargetLength.Text) == false)
                        ErrorMessage = "Please specify a valid Target Length for importing transects.";
                    else
                        TargetLength = Convert.ToDouble(txtTargetLength.Text);

            }

            //get cursor on features to import
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (m_ImportFC != null)
                    ImportFCursor = m_ImportFC.Search(null, false);
                if (m_SelectionSet != null)
                    m_SelectionSet.Search(null, false, out ImportCursor);
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (ImportType == esriGeometryType.esriGeometryPolyline)
                    TargetFC = Util.GetFeatureClass(m_NPS.LYR_GENERATED_TRANSECTS, ref ErrorMessage);

                if (ImportType == esriGeometryType.esriGeometryPoint)
                    TargetFC = Util.GetFeatureClass(m_NPS.LYR_RANDOMPOINTS, ref ErrorMessage);
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                m_NPS.ProgressLabel = lblProgressLabel;
                Util.SetProgressMessage("Clipping Raster", ((ImportType == esriGeometryType.esriGeometryPoint) ? 7 : 2));


                //get the DEM specified by the DEM filepath field
                ThisGeoRasterDS = Util.OpenRasterDataset(m_NPS.MainTransectForm.txtDemFileLocation.Text,
                    ref ErrorMessage) as IGeoDataset;

                if (string.IsNullOrEmpty(ErrorMessage) == false)
                    ErrorMessage = ErrorMessage + "\r\n\r\nPlease set a valid DEM file from the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (m_NPS.MainTransectForm.cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage = "Please set a valid unit for the DEM file on the Config tab.";
                else
                    DEMUnits = m_NPS.MainTransectForm.cboDEMFileUnits.Text;
            }

            //clip the dem file to the size of the survey area
            if (string.IsNullOrEmpty(ErrorMessage))
                ThisGeoRasterDS = Util.ClipRasterByBndPoly(ThisGeoRasterDS, Convert.ToString(SurveyID), ref ErrorMessage);


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //points will have a temp batch id since they will be updated later
                if (ImportType == esriGeometryType.esriGeometryPoint) FirstLoadBatchID = 9999;
                if (ImportType == esriGeometryType.esriGeometryPolyline) FirstLoadBatchID = BatchID;

                //tranects will need to have a unique transect id
                if (ImportType == esriGeometryType.esriGeometryPolyline)
                {
                    //get next available transect id
                    NextTransectID = Util.GetHighestFieldValue(m_NPS.LYR_GENERATED_TRANSECTS,
                        "TransectID", "SurveyID=" + SurveyID, ref ErrorMessage);
                    if (string.IsNullOrEmpty(ErrorMessage) == false) return;
                    NextTransectID++;
                }

            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                Util.SetProgressMessage("Importing" + ((ImportType == esriGeometryType.esriGeometryPoint)
                ? " Random Points" : " Transects"));


                try
                {
                    //load our features into temp feature class
                    LoadTempImportFC(SurveyID, FirstLoadBatchID, NextTransectID, ImportFCursor, TargetFC, 
                        ThisGeoRasterDS, TargetLength, ref NewOIDs,ref TotalInExcluded, ref TotalOutsideBndy, 
                        ref TotalPassed,DEMUnits, ref ErrorMessage);

                    //if we have less than 2 successful inports, delete the 1 that was inserted
                    if (TotalPassed < 2 && ImportType == esriGeometryType.esriGeometryPoint)
                    {
                        foreach (int OID in NewOIDs)
                            WhereClause += "or " + TargetFC.OIDFieldName + "=" + OID;

                        if (WhereClause != string.Empty) WhereClause = WhereClause.Substring(3);
                        Util.DeleteFeatures(TargetFC, WhereClause, ref InternalError);

                        ErrorMessage = "A minimum of 2 random points must be evaludated succesfully to carry out import.";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message;
                }

                ResultMessage = "\r\n\r\nTotal in excluded areas = " + TotalInExcluded
                        + "\r\nTotal outside survey boundary = " + TotalOutsideBndy
                        + "\r\nTotal passed = " + TotalPassed;
            }


            if (string.IsNullOrEmpty(ErrorMessage))
                if (ImportType == esriGeometryType.esriGeometryPoint)
                    Util.AddZValuesToPoints(SurveyID, BatchID, FirstLoadBatchID, ThisGeoRasterDS, DEMUnits, ref ErrorMessage);

                


            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                MessageBox.Show("Completed Successfully" + ResultMessage);
                this.Close();
            }
            else
                MessageBox.Show(ErrorMessage + ResultMessage);


        }

        public void LoadTempImportFC(int SurveyID, int BatchID, int NextTransectID, IFeatureCursor ImportFCursor,
            IFeatureClass TempImportFC, IGeoDataset ThisDEM, double TargetLength, ref List<int> NewOIDs,
            ref int TotalInExcluded, ref int TotalOutsideBndy, ref int TotalPassed,string ThisDEMUnits, ref string ErrorMessage)
        {

            IFeature ImportFeature;
            IFeatureCursor ImportToCursor = null;
            IFeatureBuffer ImportToBuffer = null;
            esriGeometryType FType;
            bool IsInExcludedAreas, IsInBndPoly;
            ESRI.ArcGIS.GeoAnalyst.ISurfaceOp2 npsSurfaceOp = null;
            int ThisFieldIndex;
            IPoint TempPoint;
            IPolyline NewTrnPolyline;
            double Elev;
            IPolyline CPolyline;
            IPoint centerPoint;
            IFeatureClass ExclPolyFC, BndPolyFC;


            ThisDEMUnits = ThisDEMUnits.ToLower();
            NewOIDs = new List<int>();
            TotalInExcluded = 0;
            TotalOutsideBndy = 0;
            TotalPassed = 0;

            ExclPolyFC = Util.GetFeatureClass(m_NPS.LYR_EXCLUDED_AREAS, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;

            BndPolyFC = Util.GetFeatureClass(m_NPS.LYR_SURVEY_BOUNDARY, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false) return;


           

            FType = TempImportFC.ShapeType;

            //get insert cursor in the nps feature class we are going to insert into
            ImportToCursor = TempImportFC.Insert(true);
            ImportToBuffer = TempImportFC.CreateFeatureBuffer();

            if (FType == esriGeometryType.esriGeometryPolyline)
                npsSurfaceOp = new ESRI.ArcGIS.GeoAnalyst.RasterSurfaceOpClass();


            //loop through each import feature and import it to it's appropriate featureclass
            while ((ImportFeature = ImportFCursor.NextFeature()) != null)
            {
                //make sure the shape is valid
                if (ImportFeature.ShapeCopy == null)
                {
                    TotalOutsideBndy++;
                    continue;
                }


                if (FType == esriGeometryType.esriGeometryPoint)
                {

                    //check if rand point  falls in excluded areas
                    IsInExcludedAreas = Util.HasRelationshipWithFC(ImportFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                        ExclPolyFC, "SurveyID=" + SurveyID);

                    //if point is in excluded areas, don't add
                    if (IsInExcludedAreas)
                    {
                        TotalInExcluded++;
                        continue;
                    }

                    //check if  rand point is within boundary
                    IsInBndPoly = Util.HasRelationshipWithFC(ImportFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                        BndPolyFC, "SurveyID=" + SurveyID);

                    //if random point is not in boundary, dont add it
                    if (IsInBndPoly == false)
                    {
                        TotalOutsideBndy++;
                        continue;
                    }

                }

                if (FType == esriGeometryType.esriGeometryPolyline)
                {

                    //check if new line falls in excluded areas
                    IsInExcludedAreas = Util.HasRelationshipWithFC(ImportFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelCrosses,
                         ExclPolyFC, "SurveyID=" + SurveyID);

                    //if point is in excluded areas, don't add
                    if (IsInExcludedAreas)
                    {
                        TotalInExcluded++;
                        continue;
                    }

                    //check if new line is within in boundary
                    IsInBndPoly = Util.HasRelationshipWithFC(ImportFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                         BndPolyFC, "SurveyID=" + SurveyID);

                    //if random point is not in boundary, dont add it
                    if (IsInBndPoly == false)
                    {
                        TotalOutsideBndy++;
                        continue;
                    }

                }

                TotalPassed++;

                //add feature to temp feature class
                ImportToBuffer.Shape = ImportFeature.ShapeCopy;
                ImportToBuffer.set_Value(ImportToBuffer.Fields.FindField("SurveyID"), SurveyID);
                ImportToBuffer.set_Value(ImportToBuffer.Fields.FindField("BATCH_ID"), BatchID);

                if (FType == esriGeometryType.esriGeometryPolyline)
                {


                    ThisFieldIndex = ImportToBuffer.Fields.FindField("Flown");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ImportToBuffer.Fields.FindField("Flown"), "N");

                    ThisFieldIndex = ImportToBuffer.Fields.FindField("TransectID");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ImportToBuffer.Fields.FindField("TransectID"), NextTransectID);

                    NextTransectID++;


                    NewTrnPolyline = ImportToBuffer.Shape as IPolyline;

                    ThisFieldIndex = ImportToBuffer.Fields.FindField("LENGTH_MTR");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, NewTrnPolyline.Length);

                    //add the name of the default projection
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("PROJECTION");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, NewTrnPolyline.SpatialReference.Name);

                    ThisFieldIndex = ImportToBuffer.Fields.FindField("TARGETLEN");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TargetLength);



                    //clone from point
                    TempPoint = ((ESRI.ArcGIS.esriSystem.IClone)NewTrnPolyline.FromPoint).Clone() as IPoint;

                    //add from point projected coords
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("PROJTD_X1");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.X);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("PROJTD_Y1");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.Y);

                    //add from point geo coords
                    ((IGeometry2)TempPoint).ProjectEx(Util.GetWGSSpatRef(), esriTransformDirection.esriTransformForward, null, false, 0, 0);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("DD_LONG1");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.X);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("DD_LAT1");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.Y);

                    //clone to point
                    TempPoint = ((ESRI.ArcGIS.esriSystem.IClone)NewTrnPolyline.ToPoint).Clone() as IPoint;


                    //add to point projected coords
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("PROJTD_X2");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.X);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("PROJTD_Y2");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.Y);

                    //add to point geo coords
                    ((IGeometry2)TempPoint).ProjectEx(Util.GetWGSSpatRef(), esriTransformDirection.esriTransformForward, null, false, 0, 0);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("DD_LONG2");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.X);
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("DD_LAT2");
                    if (ThisFieldIndex > -1) ImportToBuffer.set_Value(ThisFieldIndex, TempPoint.Y);

                    //get center point
                    centerPoint = new PointClass();
                    ((ICurve)NewTrnPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPoint);

                    //get elevation for transect center
                    CPolyline = new PolylineClass();
                    npsSurfaceOp.ContourAsPolyline(ThisDEM, centerPoint, out CPolyline, out Elev);

                    //set elevation in meters
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("ELEV_M");
                    if (ThisFieldIndex > -1)
                    {
                        if (ThisDEMUnits == "feet") ImportToBuffer.set_Value(ThisFieldIndex, 0.3048 * Elev);
                        if (ThisDEMUnits == "meters") ImportToBuffer.set_Value(ThisFieldIndex, Elev);

                    }

                    //get elevation in feet
                    ThisFieldIndex = ImportToBuffer.Fields.FindField("ELEVFT");
                    if (ThisFieldIndex > -1)
                    {
                        if (ThisDEMUnits == "feet") ImportToBuffer.set_Value(ThisFieldIndex, Elev);
                        if (ThisDEMUnits == "meters") ImportToBuffer.set_Value(ThisFieldIndex, Elev * 3.2808399);
                    }

                }

                NewOIDs.Add((int)Util.SafeConvert(ImportToCursor.InsertFeature(ImportToBuffer), typeof(int)));

            }

            ImportFCursor = null;
            ImportToCursor = null;
            ImportToBuffer = null;

        }

    }

}
