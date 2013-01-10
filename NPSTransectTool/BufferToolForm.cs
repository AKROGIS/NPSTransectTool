using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace NPSTransectTool
{
    public partial class BufferToolForm : Form
    {
        readonly NPSGlobal m_NPS;
        Dictionary<string, int> m_SurveysList;

        public BufferToolForm()
        {
            InitializeComponent();

            m_NPS = NPSGlobal.Instance;

            Util.ManageSavedValues(this, SavedValuesAction.Load);

        }

        private void BufferToolForm_Load(object sender, EventArgs e)
        {
            m_SurveysList = Util.GetSurveysList();
            m_SurveysList.Add("[Select Survey]", -1);

            var sortedDict = (from entry in m_SurveysList orderby entry.Key ascending select entry);
            foreach (KeyValuePair<string, int> row in sortedDict)
                cboSurveysList.Items.Add(row.Key);

            cboSurveysList.SelectedIndex = 0;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            Util.ManageSavedValues(this, SavedValuesAction.Save);
            Util.SaveConfigSettings();

            Close();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            IFeatureClass thisBufferFc = null;
            IFeatureClass thisLineFc = null;
            IFeatureClass trackLogFc = null;
            string errorMessage = "";
            string internalErr = "";
            ISelectionSet2 thisSelection = null;
            List<string> uniqueTransectIDs = null;
            double blindDistance = 0;
            double bufferDistance = 0;

            const string unionBufferFcName = "TEMP_UNION_FC";
            const string outerBufferFcName = "TEMP_OUTER_FC";
            const string blindBufferFcName = "TEMP_BLIND_FC";
            const string tempLineFcName = "TEMP_LINE_FC";
            string bufferFcName = "BUFFER_FC";
            string workspacePath = m_NPS.Workspace.PathName;

            var observedSides = new List<string> { "Left", "Right" };

            //get the batchid and surveyid for the new features
            int surveyId = m_SurveysList[cboSurveysList.Text];
            if (surveyId == -1) errorMessage = "Please select a survey to import to.";

            //valid blind distance
            if (string.IsNullOrEmpty(errorMessage))
            {
                if (double.TryParse(txtBlindDistance.Text, out blindDistance) == false)
                    errorMessage = "Please enter a valid blind distance.";
            }

            //validate buffer distance
            if (string.IsNullOrEmpty(errorMessage))
            {
                if (double.TryParse(txtBufferDistance.Text, out bufferDistance) == false)
                    errorMessage = "Please enter a valid buffer distance.";
            }



            if (string.IsNullOrEmpty(errorMessage))
            {
                m_NPS.ProgressLabel = lblProgressLabel;
                Util.SetProgressMessage("Preparing Buffer FeatureClass", 4);

                //use whatever name is set by user
                if (string.IsNullOrEmpty(txtBufferFCName.Text) == false)
                    bufferFcName = txtBufferFCName.Text;

                //create temp buffer feature class if it does not exist
                thisBufferFc = Util.GetFeatureClass(bufferFcName, ref errorMessage) ??
                               CreateBufferFC(bufferFcName, ref errorMessage);

                //delete all existing buffers
                ((ITable)thisBufferFc).DeleteSearchedRows(null);
            }


            //create temp line feature class (remove any old existing ones
            if (string.IsNullOrEmpty(errorMessage))
            {
                Util.SetProgressMessage("Preparing temp segment FeatureClass");

                Util.DeleteDataset(tempLineFcName, esriDatasetType.esriDTFeatureClass, ref errorMessage);
                thisLineFc = CreateTempLineFC(tempLineFcName, ref errorMessage);
            }

            //get tracklog fc
            if (string.IsNullOrEmpty(errorMessage))
                trackLogFc = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ref errorMessage);


            //check if we have a selection, if so get it
            if (string.IsNullOrEmpty(errorMessage))
            {
                thisSelection = Util.GetLayerSelection(m_NPS.LYR_TRACKLOG, ref errorMessage);
                errorMessage = "";
            }



            //get unique list of transects in selection or database
            if (string.IsNullOrEmpty(errorMessage))
            {
                Util.SetProgressMessage("Building Transect list");

                uniqueTransectIDs = Util.GetUniqueValues(trackLogFc, "TransectID", "SurveyID="
                    + surveyId, ref errorMessage);

            }

            if (string.IsNullOrEmpty(errorMessage) && uniqueTransectIDs != null)
                foreach (string TransectID in uniqueTransectIDs)
                {
                    Util.SetProgressMessage("Building buffers for Transect with ID " + TransectID, false);

                    //need to run code for both sides so array only has 2 items - Left and Right
                    foreach (string ObservedSide in observedSides)
                    {
                        IQueryFilter ThisQueryFilter = new QueryFilterClass
                        {
                            WhereClause = "SegType='OnTransect' and TransectID=" + TransectID
                                          + " and SurveyID=" + surveyId +
                                //everyone observes in the same direction so get direction of any observer that has it
                                          " and (PilotDir='" + ObservedSide + "' or " + " Obs1Dir='" + ObservedSide
                                          + "' or Obs2Dir='" + ObservedSide + "')"
                        };

                        //get all the segments for the current transect (or all the selected segments 
                        //from the current transect)
                        ICursor thisFCursor;
                        if (thisSelection == null)
                            thisFCursor = trackLogFc.Search(ThisQueryFilter, false) as ICursor;
                        else
                            thisSelection.Search(ThisQueryFilter, false, out thisFCursor);

                        //delete all features in temp line fc
                        ((ITable)thisLineFc).DeleteSearchedRows(null);

                        //copy the current batch of segments to line fc
                        int SegementsCount = Util.CopyFeatures(thisFCursor as IFeatureCursor, thisLineFc, null, ref errorMessage);

                        //there are no segments observed on the current direction and on the current transect so move on
                        if (SegementsCount == 0) continue;


                        //get rid of all temp featureclasses if any weren't deleted
                        Util.DeleteDataset(blindBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);
                        Util.DeleteDataset(outerBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);
                        Util.DeleteDataset(unionBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);

                        try
                        {
                            //generate blind buffers
                            string NewFCPath = System.IO.Path.Combine(workspacePath, blindBufferFcName);
                            IFeatureClass BlindBufferFC = Util.GP_Buffer_analysis(thisLineFc, NewFCPath, blindDistance,
                                                                                  ObservedSide, ref errorMessage);


                            //generate outer buffers
                            NewFCPath = System.IO.Path.Combine(workspacePath, outerBufferFcName);
                            IFeatureClass OuterBufferFC = Util.GP_Buffer_analysis(thisLineFc, NewFCPath, bufferDistance,
                                                                                  ObservedSide, ref errorMessage);


                            //generate union of buffers
                            NewFCPath = System.IO.Path.Combine(workspacePath, unionBufferFcName);
                            IFeatureClass UnionBufferFC = Util.GP_Union_analysis(BlindBufferFC, OuterBufferFC, NewFCPath,
                                                                                 ref errorMessage);

                            //import non-intersecting buffers (the buffer area offset of the segment)
                            ImportBuffers(surveyId, int.Parse(TransectID), UnionBufferFC, thisBufferFc,
                                          "FID_" + blindBufferFcName);

                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }

                        //get rid of all temp featureclasses for this run
                        Util.DeleteDataset(blindBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);
                        Util.DeleteDataset(outerBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);
                        Util.DeleteDataset(unionBufferFcName, esriDatasetType.esriDTFeatureClass, ref internalErr);
                    }
                }

            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(errorMessage))
            {
                //add buffer feature class as a layer to the map
                ESRI.ArcGIS.Carto.ILayer BufferLayer = Util.GetLayer(bufferFcName);
                if (BufferLayer == null) Util.AddDataToMapAsLayer(thisBufferFc as IGeoDataset, bufferFcName);

                MessageBox.Show("Buffer Tool completed successfully");

                Close();
            }
            else
                MessageBox.Show(errorMessage);


        }

        /// <summary>
        /// at any one time, the union fc will only contain buffers for a particular transect from a
        /// particular survey. import non-intersecting buffers and assign them the transect/survey id
        /// </summary>
        private void ImportBuffers(int SurveyID, int TransectID, IFeatureClass UnionFC, IFeatureClass BufferFC,
            string UnionFIDField)
        {
            IFeature thisFeature;

            int fidFieldIndex = UnionFC.FindField(UnionFIDField);

            int surveyIdFieldIndex = BufferFC.FindField("SurveyID");
            int transectIdFieldIndex = BufferFC.FindField("TransectID");

            IFeatureCursor insertCursor = BufferFC.Insert(true);
            IFeatureBuffer insertBuffer = BufferFC.CreateFeatureBuffer();

            IFeatureCursor thisFCursor = UnionFC.Search(null, false);

            while ((thisFeature = thisFCursor.NextFeature()) != null)
            {
                //exclude unwanted buffer polys
                if (fidFieldIndex > -1)
                {
                    var FID = (int)Util.SafeConvert(thisFeature.Value[fidFieldIndex], typeof(int));
                    if (FID != -1) continue;
                }

                //set new feature values
                insertBuffer.Shape = thisFeature.ShapeCopy;
                insertBuffer.Value[surveyIdFieldIndex] = SurveyID;
                insertBuffer.Value[transectIdFieldIndex] = TransectID;

                //insert feature
                insertCursor.InsertFeature(insertBuffer);
            }
        }


        /// <summary>
        /// create buffer featureclass used to store buffers
        /// </summary>
        private IFeatureClass CreateBufferFC(string FCName, ref string ErrorMessage)
        {
            IFields pfields = new FieldsClass();

            ISpatialReference thisSpatialReference = Util.GetDefaultSpatialReference();


            // create the surveyid id field
            IField pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "SurveyID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);

            // create the transect id field
            pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "TransectID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);

            //create the segementid id field
            pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "SegmentID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);


            return Util.CreateWorkspaceFeatureClass(FCName, m_NPS.Workspace, esriGeometryType.esriGeometryPolygon,
                 pfields, thisSpatialReference, ref ErrorMessage);


        }

        /// <summary>
        /// create a featureclass to hold lines that will be buffered
        /// </summary>
        private IFeatureClass CreateTempLineFC(string FCName, ref string ErrorMessage)
        {
            IFields pfields = new FieldsClass();

            ISpatialReference thisSpatialReference = Util.GetDefaultSpatialReference();


            // create the surveyid id field
            IField pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "SurveyID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);

            // create the transect id field
            pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "TransectID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);

            //create the segementid id field
            pField = new FieldClass();
            ((IFieldEdit)pField).Name_2 = "SegmentID";
            ((IFieldEdit)pField).Type_2 = esriFieldType.esriFieldTypeInteger;
            ((IFieldsEdit)pfields).AddField(pField);


            return Util.CreateWorkspaceFeatureClass(FCName, m_NPS.Workspace, esriGeometryType.esriGeometryPolyline,
                 pfields, thisSpatialReference, ref ErrorMessage);
        }
    }
}
