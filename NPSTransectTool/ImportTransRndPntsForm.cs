using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace NPSTransectTool
{
    public partial class ImportTransRndPntsForm : Form
    {
        readonly NPSGlobal m_NPS;
        Dictionary<string, int> m_SurveysList;
        ISelectionSet2 m_SelectionSet;
        esriGeometryType m_GeometryType;
        IFeatureClass m_ImportFC;
        readonly TransectToolForm m_TransectToolForm;

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
            string errorMessage = "";

            string fcName = m_GeometryType == esriGeometryType.esriGeometryPolyline
                                ? m_NPS.LYR_GENERATED_TRANSECTS : m_NPS.LYR_RANDOMPOINTS;

            cboBatches.DataSource = null;

            int surveyId = m_SurveysList[cboSurveysList.Text];

            if (surveyId == -1) return;

            List<string> batchIDs = Util.GetBatchIDs(fcName, Convert.ToString(surveyId), ref errorMessage);
            if (batchIDs.Count == 0) batchIDs.Add("1");

            cboBatches.DataSource = batchIDs;

            if (string.IsNullOrEmpty(errorMessage) == false)
                MessageBox.Show(errorMessage);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string errorMessage = "";
            bool allOkay = true;
            bool cancelled = false;

            string newPath = Util.OpenESRIDialog(txtBrowseFile.Text, ref cancelled);
            if (cancelled == false)
                txtBrowseFile.Text = newPath;
            else
                return;

            //make sure we got the shapefile
            m_ImportFC = Util.GetShapeFile(txtBrowseFile.Text, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                MessageBox.Show(errorMessage);
                allOkay = false;
            }

            if (allOkay)
            {
                //make sure the shapefile is the right shape type
                if (m_ImportFC.ShapeType != esriGeometryType.esriGeometryPoint
                    && m_ImportFC.ShapeType != esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show("The selected shapefile must be a point if random points are being "
                    + "imported or polylines if transects are being imported");
                    allOkay = false;
                }
            }

            if (allOkay)
            {
                //make sure that the shape file has the same coordinate system as the default nps coordinate system
                ISpatialReference importSfSpatRef = ((IGeoDataset)m_ImportFC).SpatialReference;
                ISpatialReference defaultSpatRef = Util.GetDefaultSpatialReference();

                if (Util.CompareSpatialReference(defaultSpatRef, importSfSpatRef) == false)
                {
                    MessageBox.Show("(Err) The selected shape file has the coordinate system '" + importSfSpatRef.Name
                   + "' which is different from that of the NPS database coordinate system which is '"
                   + defaultSpatRef.Name + "'.");
                    allOkay = false;

                }
            }

            //if something went run, clea shapefile
            if (allOkay == false) m_ImportFC = null;

            //if the shapefile was invalid and we have no selection, turn off ddls
            if (allOkay == false && m_SelectionSet == null)
            {

                cboBatches.Enabled = false;
                cboSurveysList.Enabled = false;
            }

            if (allOkay)
            {
                //if everything is good uptil this point, enable dropdownlists
                cboBatches.Enabled = true;
                cboSurveysList.Enabled = true;
                cboSurveysList.SelectedIndex = 0;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            int surveyId = -1;
            int firstLoadBatchId = -1;
            int batchId = -1;
            int nextTransectId = -1;
            string errorMessage = "";
            string resultMessage = "";
            string internalError = "";
            var importType = esriGeometryType.esriGeometryNull;
            IFeatureCursor importFCursor = null;
            IFeatureClass targetFc = null;
            IGeoDataset thisGeoRasterDs = null;
            int totalInExcluded = 0;
            int totalOutsideBndy = 0;
            int totalPassed = 0;
            int featureCount = 0;
            var newOIDs = new List<int>();
            string whereClause = "";
            string demUnits="";
            double targetLength = -1;


            //make sure we have a selection set or a shapefule
            if (m_ImportFC == null && m_SelectionSet == null)
            {
                errorMessage = @"No Random Points or Transects have been set for import. To select 
                                Random Points or Transects for import either select points or
                                polylines from the map or browse to a Shapefile containing points 
                                or polylines.";
            }

            //get the batchid and surveyid for the new features
            if (string.IsNullOrEmpty(errorMessage))
            {
                if (Util.IsNumeric(cboBatches.Text) == false)
                    errorMessage = "Please select a batch id to import to.";
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                batchId = Convert.ToInt32(cboBatches.Text);

                surveyId = m_SurveysList[cboSurveysList.Text];
                if (surveyId == -1) errorMessage = "Please select a survey to import to.";

            }


            //make sure we have a DEM file set
            if (string.IsNullOrEmpty(errorMessage))
            {
                string DemFilePath = m_TransectToolForm.txtDemFileLocation.Text.Trim();
                if (DemFilePath == "")
                    errorMessage = "Please set a DEM file on the Config tab that "
                    + "can be used for the specified Survey.";
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                if (m_TransectToolForm.cboDEMFileUnits.SelectedIndex == 0)
                    errorMessage = errorMessage + "Please set a valid unit for the DEM file from the Config tab.";
            }

            //validate data
            if (string.IsNullOrEmpty(errorMessage))
            {
                if (m_ImportFC != null)
                    importType = m_ImportFC.ShapeType;
                if (m_SelectionSet != null)
                    importType = m_GeometryType;

                if (m_ImportFC != null)
                    featureCount = m_ImportFC.FeatureCount(null);
                if (m_SelectionSet != null)
                    featureCount = m_SelectionSet.Count;
            }


            if (string.IsNullOrEmpty(errorMessage))
                if (importType == esriGeometryType.esriGeometryPoint)
                    if (featureCount < 2) errorMessage = "A minimum of 2 random points must be imported at one time.";


            if (string.IsNullOrEmpty(errorMessage))
            {
                if (importType == esriGeometryType.esriGeometryPolyline)
                    if (Util.IsNumeric(txtTargetLength.Text) == false)
                        errorMessage = "Please specify a valid Target Length for importing transects.";
                    else
                        targetLength = Convert.ToDouble(txtTargetLength.Text);

            }

            //get cursor on features to import
            if (string.IsNullOrEmpty(errorMessage))
            {
                if (m_ImportFC != null)
                    importFCursor = m_ImportFC.Search(null, false);
                if (m_SelectionSet != null)
                {
                    ICursor ImportCursor;
                    m_SelectionSet.Search(null, false, out ImportCursor);
                }
            }


            if (string.IsNullOrEmpty(errorMessage))
            {
                if (importType == esriGeometryType.esriGeometryPolyline)
                    targetFc = Util.GetFeatureClass(m_NPS.LYR_GENERATED_TRANSECTS, ref errorMessage);

                if (importType == esriGeometryType.esriGeometryPoint)
                    targetFc = Util.GetFeatureClass(m_NPS.LYR_RANDOMPOINTS, ref errorMessage);
            }


            if (string.IsNullOrEmpty(errorMessage))
            {
                m_NPS.ProgressLabel = lblProgressLabel;
                Util.SetProgressMessage("Clipping Raster", ((importType == esriGeometryType.esriGeometryPoint) ? 7 : 2));


                //get the DEM specified by the DEM filepath field
                thisGeoRasterDs = Util.OpenRasterDataset(m_NPS.MainTransectForm.txtDemFileLocation.Text,
                    ref errorMessage) as IGeoDataset;

                if (string.IsNullOrEmpty(errorMessage) == false)
                    errorMessage = errorMessage + "\r\n\r\nPlease set a valid DEM file from the Config tab.";
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                if (m_NPS.MainTransectForm.cboDEMFileUnits.SelectedIndex == 0)
                    errorMessage = "Please set a valid unit for the DEM file on the Config tab.";
                else
                    demUnits = m_NPS.MainTransectForm.cboDEMFileUnits.Text;
            }

            //clip the dem file to the size of the survey area
            if (string.IsNullOrEmpty(errorMessage))
                thisGeoRasterDs = Util.ClipRasterByBndPoly(thisGeoRasterDs, Convert.ToString(surveyId), ref errorMessage);


            if (string.IsNullOrEmpty(errorMessage))
            {
                //points will have a temp batch id since they will be updated later
                if (importType == esriGeometryType.esriGeometryPoint) firstLoadBatchId = 9999;
                if (importType == esriGeometryType.esriGeometryPolyline) firstLoadBatchId = batchId;

                //tranects will need to have a unique transect id
                if (importType == esriGeometryType.esriGeometryPolyline)
                {
                    //get next available transect id
                    nextTransectId = Util.GetHighestFieldValue(m_NPS.LYR_GENERATED_TRANSECTS,
                        "TransectID", "SurveyID=" + surveyId, ref errorMessage);
                    if (string.IsNullOrEmpty(errorMessage) == false) return;
                    nextTransectId++;
                }

            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                Util.SetProgressMessage("Importing" + ((importType == esriGeometryType.esriGeometryPoint)
                ? " Random Points" : " Transects"));


                try
                {
                    //load our features into temp feature class
                    LoadTempImportFc(surveyId, firstLoadBatchId, nextTransectId, importFCursor, targetFc, 
                        thisGeoRasterDs, targetLength, ref newOIDs,ref totalInExcluded, ref totalOutsideBndy, 
                        ref totalPassed,demUnits, ref errorMessage);

                    //if we have less than 2 successful inports, delete the 1 that was inserted
                    if (totalPassed < 2 && importType == esriGeometryType.esriGeometryPoint)
                    {
                        whereClause = newOIDs.Aggregate(whereClause, (current, OID) => current + ("or " + targetFc.OIDFieldName + "=" + OID));

                        if (whereClause != string.Empty) whereClause = whereClause.Substring(3);
                        Util.DeleteFeatures(targetFc, whereClause, ref internalError);

                        errorMessage = "A minimum of 2 random points must be evaludated succesfully to carry out import.";
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "Error occured. " + errorMessage + ". " + ex.Message;
                }

                resultMessage = "\r\n\r\nTotal in excluded areas = " + totalInExcluded
                        + "\r\nTotal outside survey boundary = " + totalOutsideBndy
                        + "\r\nTotal passed = " + totalPassed;
            }


            if (string.IsNullOrEmpty(errorMessage))
                if (importType == esriGeometryType.esriGeometryPoint)
                    Util.AddZValuesToPoints(surveyId, batchId, firstLoadBatchId, thisGeoRasterDs, demUnits, ref errorMessage);

                


            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show("Completed Successfully" + resultMessage);
                Close();
            }
            else
                MessageBox.Show(errorMessage + resultMessage);


        }

        public void LoadTempImportFc(int SurveyID, int BatchID, int nextTransectId, IFeatureCursor importFCursor,
            IFeatureClass TempImportFC, IGeoDataset ThisDEM, double TargetLength, ref List<int> newOIDs,
            ref int totalInExcluded, ref int totalOutsideBndy, ref int totalPassed,string thisDemUnits, ref string errorMessage)
        {

            IFeature importFeature;
            ESRI.ArcGIS.GeoAnalyst.ISurfaceOp2 npsSurfaceOp = null;


            thisDemUnits = thisDemUnits.ToLower();
            newOIDs = new List<int>();
            totalInExcluded = 0;
            totalOutsideBndy = 0;
            totalPassed = 0;

            IFeatureClass ExclPolyFC = Util.GetFeatureClass(m_NPS.LYR_EXCLUDED_AREAS, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false) return;

            IFeatureClass BndPolyFC = Util.GetFeatureClass(m_NPS.LYR_SURVEY_BOUNDARY, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false) return;


           

            esriGeometryType fType = TempImportFC.ShapeType;

            //get insert cursor in the nps feature class we are going to insert into
            IFeatureCursor importToCursor = TempImportFC.Insert(true);
            IFeatureBuffer importToBuffer = TempImportFC.CreateFeatureBuffer();

            if (fType == esriGeometryType.esriGeometryPolyline)
                npsSurfaceOp = new ESRI.ArcGIS.GeoAnalyst.RasterSurfaceOpClass();


            //loop through each import feature and import it to it's appropriate featureclass
            while ((importFeature = importFCursor.NextFeature()) != null)
            {
                //make sure the shape is valid
                if (importFeature.ShapeCopy == null)
                {
                    totalOutsideBndy++;
                    continue;
                }


                bool isInExcludedAreas;
                bool isInBndPoly;
                if (fType == esriGeometryType.esriGeometryPoint)
                {

                    //check if rand point  falls in excluded areas
                    isInExcludedAreas = Util.HasRelationshipWithFC(importFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                        ExclPolyFC, "SurveyID=" + SurveyID);

                    //if point is in excluded areas, don't add
                    if (isInExcludedAreas)
                    {
                        totalInExcluded++;
                        continue;
                    }

                    //check if  rand point is within boundary
                    isInBndPoly = Util.HasRelationshipWithFC(importFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                        BndPolyFC, "SurveyID=" + SurveyID);

                    //if random point is not in boundary, dont add it
                    if (isInBndPoly == false)
                    {
                        totalOutsideBndy++;
                        continue;
                    }

                }

                if (fType == esriGeometryType.esriGeometryPolyline)
                {

                    //check if new line falls in excluded areas
                    isInExcludedAreas = Util.HasRelationshipWithFC(importFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelCrosses,
                         ExclPolyFC, "SurveyID=" + SurveyID);

                    //if point is in excluded areas, don't add
                    if (isInExcludedAreas)
                    {
                        totalInExcluded++;
                        continue;
                    }

                    //check if new line is within in boundary
                    isInBndPoly = Util.HasRelationshipWithFC(importFeature.ShapeCopy, esriSpatialRelEnum.esriSpatialRelWithin,
                         BndPolyFC, "SurveyID=" + SurveyID);

                    //if random point is not in boundary, dont add it
                    if (isInBndPoly == false)
                    {
                        totalOutsideBndy++;
                        continue;
                    }

                }

                totalPassed++;

                //add feature to temp feature class
                importToBuffer.Shape = importFeature.ShapeCopy;
                importToBuffer.Value[importToBuffer.Fields.FindField("SurveyID")] = SurveyID;
                importToBuffer.Value[importToBuffer.Fields.FindField("BATCH_ID")] = BatchID;

                if (fType == esriGeometryType.esriGeometryPolyline)
                {


                    int thisFieldIndex = importToBuffer.Fields.FindField("Flown");
                    if (thisFieldIndex > -1) importToBuffer.Value[importToBuffer.Fields.FindField("Flown")] =  "N";

                    thisFieldIndex = importToBuffer.Fields.FindField("TransectID");
                    if (thisFieldIndex > -1) importToBuffer.Value[importToBuffer.Fields.FindField("TransectID")] = nextTransectId;

                    nextTransectId++;


                    var newTrnPolyline = importToBuffer.Shape as IPolyline;

                    thisFieldIndex = importToBuffer.Fields.FindField("LENGTH_MTR");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = newTrnPolyline.Length;

                    //add the name of the default projection
                    thisFieldIndex = importToBuffer.Fields.FindField("PROJECTION");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] =  newTrnPolyline.SpatialReference.Name;

                    thisFieldIndex = importToBuffer.Fields.FindField("TARGETLEN");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = TargetLength;



                    //clone from point
                    var tempPoint = ((ESRI.ArcGIS.esriSystem.IClone)newTrnPolyline.FromPoint).Clone() as IPoint;

                    //add from point projected coords
                    thisFieldIndex = importToBuffer.Fields.FindField("PROJTD_X1");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.X;
                    thisFieldIndex = importToBuffer.Fields.FindField("PROJTD_Y1");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.Y;

                    //add from point geo coords
                    ((IGeometry2)tempPoint).ProjectEx(Util.GetWGSSpatRef(), esriTransformDirection.esriTransformForward, null, false, 0, 0);
                    thisFieldIndex = importToBuffer.Fields.FindField("DD_LONG1");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.X;
                    thisFieldIndex = importToBuffer.Fields.FindField("DD_LAT1");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.Y;

                    //clone to point
                    tempPoint = ((ESRI.ArcGIS.esriSystem.IClone)newTrnPolyline.ToPoint).Clone() as IPoint;


                    //add to point projected coords
                    thisFieldIndex = importToBuffer.Fields.FindField("PROJTD_X2");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.X;
                    thisFieldIndex = importToBuffer.Fields.FindField("PROJTD_Y2");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.Y;

                    //add to point geo coords
                    ((IGeometry2)tempPoint).ProjectEx(Util.GetWGSSpatRef(), esriTransformDirection.esriTransformForward, null, false, 0, 0);
                    thisFieldIndex = importToBuffer.Fields.FindField("DD_LONG2");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.X;
                    thisFieldIndex = importToBuffer.Fields.FindField("DD_LAT2");
                    if (thisFieldIndex > -1) importToBuffer.Value[thisFieldIndex] = tempPoint.Y;

                    //get center point
                    IPoint centerPoint = new PointClass();
                    ((ICurve)newTrnPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, centerPoint);

                    //get elevation for transect center
                    IPolyline cPolyline;
                    double elev;
                    npsSurfaceOp.ContourAsPolyline(ThisDEM, centerPoint, out cPolyline, out elev);

                    //set elevation in meters
                    thisFieldIndex = importToBuffer.Fields.FindField("ELEV_M");
                    if (thisFieldIndex > -1)
                    {
                        if (thisDemUnits == "feet") importToBuffer.Value[thisFieldIndex] = 0.3048 * elev;
                        if (thisDemUnits == "meters") importToBuffer.Value[thisFieldIndex] = elev;

                    }

                    //get elevation in feet
                    thisFieldIndex = importToBuffer.Fields.FindField("ELEVFT");
                    if (thisFieldIndex > -1)
                    {
                        if (thisDemUnits == "feet") importToBuffer.Value[thisFieldIndex] = elev;
                        if (thisDemUnits == "meters") importToBuffer.Value[thisFieldIndex] = elev * 3.2808399;
                    }

                }

                newOIDs.Add((int)Util.SafeConvert(importToCursor.InsertFeature(importToBuffer), typeof(int)));

            }

        }

    }

}
