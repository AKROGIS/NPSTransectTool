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
using ESRI.ArcGIS.ArcMapUI;

namespace NPSTransectTool
{
    public partial class BufferToolForm : Form
    {
        NPSGlobal m_NPS;
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

            this.Close();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

            IFeatureClass ThisBufferFC = null, ThisLineFC = null, TrackLogFC = null,
                BlindBufferFC = null, OuterBufferFC = null, UnionBufferFC = null;
            string ErrorMessage = "", InternalErr = "", BufferFCName, TempLineFCName, WorkspacePath,
                BlindBufferFCName, OuterBufferFCName, UnionBufferFCName, NewFCPath;
            ISelectionSet2 ThisSelection = null;
            List<string> UniqueTransectIDs = null, ObservedSides;
            IQueryFilter ThisQueryFilter;
            ICursor ThisFCursor = null;
            int SurveyID = -1, SegementsCount;
            double BlindDistance = 0, BufferDistance = 0;
            ESRI.ArcGIS.Carto.ILayer BufferLayer;


            UnionBufferFCName = "TEMP_UNION_FC";
            OuterBufferFCName = "TEMP_OUTER_FC";
            BlindBufferFCName = "TEMP_BLIND_FC";
            TempLineFCName = "TEMP_LINE_FC";
            BufferFCName = "BUFFER_FC";
            WorkspacePath = m_NPS.Workspace.PathName;

            ObservedSides = new List<string>() { "Left", "Right" };

            //get the batchid and surveyid for the new features
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1) ErrorMessage = "Please select a survey to import to.";

            //valid blind distance
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (double.TryParse(txtBlindDistance.Text, out BlindDistance) == false)
                    ErrorMessage = "Please enter a valid blind distance.";
            }

            //validate buffer distance
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (double.TryParse(txtBufferDistance.Text, out BufferDistance) == false)
                    ErrorMessage = "Please enter a valid buffer distance.";
            }



            if (string.IsNullOrEmpty(ErrorMessage))
            {
                m_NPS.ProgressLabel = lblProgressLabel;
                Util.SetProgressMessage("Preparing Buffer FeatureClass", 4);

                //use whatever name is set by user
                if (string.IsNullOrEmpty(txtBufferFCName.Text) == false)
                    BufferFCName = txtBufferFCName.Text;

                //create temp buffer feature class if it does not exist
                ThisBufferFC = Util.GetFeatureClass(BufferFCName, ref ErrorMessage);
                if (ThisBufferFC == null)
                    ThisBufferFC = CreateBufferFC(BufferFCName, ref ErrorMessage);

                //delete all existing buffers
                ((ITable)ThisBufferFC).DeleteSearchedRows(null);
            }


            //create temp line feature class (remove any old existing ones
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                Util.SetProgressMessage("Preparing temp segment FeatureClass");

                Util.DeleteDataset(TempLineFCName, esriDatasetType.esriDTFeatureClass, ref ErrorMessage);
                ThisLineFC = CreateTempLineFC(TempLineFCName, ref ErrorMessage);
            }

            //get tracklog fc
            if (string.IsNullOrEmpty(ErrorMessage))
                TrackLogFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ref ErrorMessage);


            //check if we have a selection, if so get it
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ThisSelection = Util.GetLayerSelection(m_NPS.LYR_TRACKLOG, ref ErrorMessage);
                ErrorMessage = "";
            }



            //get unique list of transects in selection or database
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                Util.SetProgressMessage("Building Transect list");

                UniqueTransectIDs = Util.GetUniqueValues(TrackLogFC, "TransectID", "SurveyID="
                    + SurveyID, ref ErrorMessage);

            }

            if (string.IsNullOrEmpty(ErrorMessage))
                foreach (string TransectID in UniqueTransectIDs)
                {
                    Util.SetProgressMessage("Building buffers for Transect with ID " + TransectID, false);

                    //need to run code for both sides so array only has 2 items - Left and Right
                    foreach (string ObservedSide in ObservedSides)
                    {
                        ThisQueryFilter = new QueryFilterClass();
                        ThisQueryFilter.WhereClause = "SegType='OnTransect' and TransectID=" + TransectID
                            + " and SurveyID=" + SurveyID +
                            //everyone observes in the same direction so get direction of any observer that has it
                            " and (PilotDir='" + ObservedSide + "' or " + " Obs1Dir='" + ObservedSide
                            + "' or Obs2Dir='" + ObservedSide + "')";

                        //get all the segments for the current transect (or all the selected segments 
                        //from the current transect)
                        if (ThisSelection == null)
                            ThisFCursor = TrackLogFC.Search(ThisQueryFilter, false) as ICursor;
                        else
                            ThisSelection.Search(ThisQueryFilter, false, out ThisFCursor);

                        //delete all features in temp line fc
                        ((ITable)ThisLineFC).DeleteSearchedRows(null);

                        //copy the current batch of segments to line fc
                        SegementsCount = Util.CopyFeatures(ThisFCursor as IFeatureCursor, ThisLineFC, null, ref ErrorMessage);

                        //there are no segments observed on the current direction and on the current transect so move on
                        if (SegementsCount == 0) continue;


                        //get rid of all temp featureclasses if any weren't deleted
                        Util.DeleteDataset(BlindBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);
                        Util.DeleteDataset(OuterBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);
                        Util.DeleteDataset(UnionBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);

                        try
                        {
                            //generate blind buffers
                            NewFCPath = System.IO.Path.Combine(WorkspacePath, BlindBufferFCName);
                            BlindBufferFC = Util.GP_Buffer_analysis(ThisLineFC, NewFCPath, BlindDistance,
                                ObservedSide, ref ErrorMessage);


                            //generate outer buffers
                            NewFCPath = System.IO.Path.Combine(WorkspacePath, OuterBufferFCName);
                            OuterBufferFC = Util.GP_Buffer_analysis(ThisLineFC, NewFCPath, BufferDistance,
                                ObservedSide, ref ErrorMessage);


                            //generate union of buffers
                            NewFCPath = System.IO.Path.Combine(WorkspacePath, UnionBufferFCName);
                            UnionBufferFC = Util.GP_Union_analysis(BlindBufferFC, OuterBufferFC, NewFCPath,
                                ref ErrorMessage);

                            //import non-intersecting buffers (the buffer area offset of the segment)
                            ImportBuffers(SurveyID, int.Parse(TransectID), UnionBufferFC, ThisBufferFC, 
                                "FID_" + BlindBufferFCName);

                        }
                        catch { }

                        //get rid of all temp featureclasses for this run
                        Util.DeleteDataset(BlindBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);
                        Util.DeleteDataset(OuterBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);
                        Util.DeleteDataset(UnionBufferFCName, esriDatasetType.esriDTFeatureClass, ref InternalErr);

                        ThisFCursor = null;
                    }

                }

            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //add buffer feature class as a layer to the map
                BufferLayer = Util.GetLayer(BufferFCName);
                if (BufferLayer == null) Util.AddDataToMapAsLayer(ThisBufferFC as IGeoDataset, BufferFCName);

                MessageBox.Show("Buffer Tool completed successfully");

                this.Close();
            }
            else
                MessageBox.Show(ErrorMessage);


        }

        /// <summary>
        /// at any one time, the union fc will only contain buffers for a particular transect from a
        /// particular survey. import non-intersecting buffers and assign them the transect/survey id
        /// </summary>
        private void ImportBuffers(int SurveyID, int TransectID, IFeatureClass UnionFC, IFeatureClass BufferFC,
            string UnionFIDField)
        {
            IFeatureCursor ThisFCursor, InsertCursor;
            IFeatureBuffer InsertBuffer;
            IFeature ThisFeature;
            int SurveyIDFieldIndex, TransectIDFieldIndex, FIDFieldIndex, FID;


            FIDFieldIndex = UnionFC.FindField(UnionFIDField);

            SurveyIDFieldIndex = BufferFC.FindField("SurveyID");
            TransectIDFieldIndex = BufferFC.FindField("TransectID");

            InsertCursor = BufferFC.Insert(true);
            InsertBuffer = BufferFC.CreateFeatureBuffer();

            ThisFCursor = UnionFC.Search(null, false);

            while ((ThisFeature = ThisFCursor.NextFeature()) != null)
            {
                //exclude unwanted buffer polys
                if (FIDFieldIndex > -1)
                {
                    FID = (int)Util.SafeConvert(ThisFeature.get_Value(FIDFieldIndex), typeof(int));
                    if (FID != -1) continue;
                }

                //set new feature values
                InsertBuffer.Shape = ThisFeature.ShapeCopy;
                InsertBuffer.set_Value(SurveyIDFieldIndex, SurveyID);
                InsertBuffer.set_Value(TransectIDFieldIndex, TransectID);

                //insert feature
                InsertCursor.InsertFeature(InsertBuffer);
            }

            InsertCursor = null;
            ThisFCursor = null;
        }


        /// <summary>
        /// create buffer featureclass used to store buffers
        /// </summary>
        private IFeatureClass CreateBufferFC(string FCName, ref string ErrorMessage)
        {
            IFields pfields;
            IField pField;
            ISpatialReference ThisSpatialReference;


            pfields = new FieldsClass();

            ThisSpatialReference = Util.GetDefaultSpatialReference();


            // create the surveyid id field
            pField = new FieldClass();
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
                 pfields, ThisSpatialReference, ref ErrorMessage);


        }

        /// <summary>
        /// create a featureclass to hold lines that will be buffered
        /// </summary>
        private IFeatureClass CreateTempLineFC(string FCName, ref string ErrorMessage)
        {
            IFields pfields;
            IField pField;
            ISpatialReference ThisSpatialReference;


            pfields = new FieldsClass();

            ThisSpatialReference = Util.GetDefaultSpatialReference();


            // create the surveyid id field
            pField = new FieldClass();
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
                 pfields, ThisSpatialReference, ref ErrorMessage);
        }
    }
}
