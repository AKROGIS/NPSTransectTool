using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;

namespace NPSTransectTool
{
    public partial class ImportBoundaryForm : Form
    {
        NPSGlobal m_NPS;

        public ImportBoundaryForm()
        {
            InitializeComponent();

            m_NPS = NPSGlobal.Instance;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {

            IFeatureClass SelShapeFile, SurveyBoundaryFC;
            string ErrorMessage="" ,Comments, Park, SurveyName;
            ESRI.ArcGIS.Geometry.ISpatialReference DefaultSpatRef, ImportSpatRef;
            IFeatureCursor ThisFCursor;
            IFeatureBuffer ThisFBuffer;
            IFeature ImportFeature;
            IFeatureCursor ImportCursor;
            int CommentsImpIndex, ParkImpIndex, SurveyNameImpIndex,
                CommentsIndex, ParkIndex, SurveyNameIndex, SurveyIDIndex,
                NextSurveyID,ImportCount;
            NewSurveyBoundaryForm form;



            //make sure we have a shapefile path
            if (string.IsNullOrEmpty(txtShapeFilePath.Text.Trim()))
            {
                MessageBox.Show("Please provide the path to the polygon ShapeFile containing the survey areas to import.");
                return;
            }

            //get the shape file at the specified path - make sure it's good
            SelShapeFile = Util.GetShapeFile(txtShapeFilePath.Text, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                return;
            }

            //make sure shapefile contains polygons
            if (SelShapeFile.ShapeType != ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("The selected ShapeFile is not a polygon shapefile");
                return;
            }

            //make sure there are features to import
            if (SelShapeFile.FeatureCount(null) == 0)
            {
                MessageBox.Show("There are no features in this ShapeFile");
                return;
            }

            //make sure the shapefile has the same spatial reference as the NPS geodatabase featureclasses
            DefaultSpatRef = Util.GetDefaultSpatialReference();
            ImportSpatRef = ((IGeoDataset)SelShapeFile).SpatialReference;
            if (Util.CompareSpatialReference(DefaultSpatRef, ImportSpatRef)==false)
            {
                MessageBox.Show("(Err) The selected ShapeFile has the coordinate system '"
                + ImportSpatRef.Name + "' which is different "
                + "from that of the NPS database coordinate system which is '"
                + DefaultSpatRef.Name + "'.");
                return;
            }

            //make sure we have the survey boundary feature class
            SurveyBoundaryFC = Util.GetFeatureClass(m_NPS.LYR_SURVEY_BOUNDARY,
                m_NPS.Workspace, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                return;
            }

            //get boundary fc survey field indexes
            SurveyIDIndex = SurveyBoundaryFC.FindField("SurveyID");
            SurveyNameIndex = SurveyBoundaryFC.FindField("SurveyName");
            ParkIndex = SurveyBoundaryFC.FindField("Park");
            CommentsIndex = SurveyBoundaryFC.FindField("Comments");

            //missing valid fields - can't go futher
            if (SurveyIDIndex == -1 || SurveyNameIndex == -1 || ParkIndex == -1 || CommentsIndex == -1)
            {
                MessageBox.Show("Survey Boundary FeatureClass is missing important fields needed for import");
                return;
            }

            //get import survey field indexes - may or may not have fields
            CommentsImpIndex = SelShapeFile.FindField("Comments");
            ParkImpIndex = SelShapeFile.FindField("Park");
            SurveyNameImpIndex = SelShapeFile.FindField("SurveyName");


            //insert cursor for survey boundary
            ThisFCursor = SurveyBoundaryFC.Insert(true);
            ThisFBuffer = SurveyBoundaryFC.CreateFeatureBuffer();

            form = new NewSurveyBoundaryForm();


            //try to get the next valid survey id
            NextSurveyID = Util.NextSurveyID(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                return;
            }
            
            ImportCount = 0;

            //loop through each shapefile boundary and inport it into database
            ImportCursor = SelShapeFile.Search(null, false);
            while ((ImportFeature = ImportCursor.NextFeature()) != null)
            {
                if (ImportFeature.Shape == null) continue;

                ThisFBuffer.Shape = ImportFeature.ShapeCopy;

                Comments = "";
                Park = "";
                SurveyName = "";

                //if the shapefile has fields for the boundary fc get, their values
                SurveyName = SurveyNameImpIndex == -1 ? "" :
                    (string)Util.SafeConvert(ImportFeature.get_Value(SurveyNameImpIndex), typeof(string));
                SurveyName = SurveyName.Trim();

                Comments = CommentsImpIndex == -1 ? "" :
                    (string)Util.SafeConvert(ImportFeature.get_Value(CommentsImpIndex), typeof(string));
                Comments = Comments.Trim();

                Park = ParkImpIndex == -1 ? "" :
                    (string)Util.SafeConvert(ImportFeature.get_Value(ParkImpIndex), typeof(string));
                Park = Park.Trim();

                //if user didn't check don't ask again and there isn't a survey name field or valid survey name
                //value, we will show the form to get user data
                if (form.ckbDontAskAgain.Checked == false && string.IsNullOrEmpty(SurveyName) == true)
                {
                    form.txtPark.Text = Park;
                    form.txtComments.Text = Comments;
                    form.txtSurveyName.Text = SurveyName;
                    form.txtSurveyID.Text = Convert.ToString(NextSurveyID);

                    form.ShowDialog();

                    Comments = form.txtComments.Text;
                    Park = form.txtPark.Text;
                    SurveyName = form.txtSurveyName.Text;
                }

                //if we don't have a survey name, the survey id will stand in for the
                //survey name
                SurveyName = string.IsNullOrEmpty(SurveyName.Trim()) ? Convert.ToString(NextSurveyID) : SurveyName;

                //set values for new boundary
                ThisFBuffer.set_Value(CommentsIndex,Comments);
                ThisFBuffer.set_Value(ParkIndex, Park);
                ThisFBuffer.set_Value(SurveyNameIndex, SurveyName);
                ThisFBuffer.set_Value(SurveyIDIndex, NextSurveyID);

                //insert boundary
                ThisFCursor.InsertFeature(ThisFBuffer);

                //get next valid surveyid
                NextSurveyID++;

                //count the number of features imported
                ImportCount++;
            }

            ImportCursor = null;
            ThisFCursor = null;

            MessageBox.Show("Import completed successfully. " + ImportCount + " feature(s) imported.");

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            bool Cancelled = false;
            string NewPath;

            NewPath = Util.OpenESRIDialog(txtShapeFilePath.Text, ref Cancelled);
            if (Cancelled == false) txtShapeFilePath.Text = NewPath;

        }
    }
}
