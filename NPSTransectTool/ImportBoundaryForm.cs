using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace NPSTransectTool
{
    public partial class ImportBoundaryForm : Form
    {
        readonly NPSGlobal m_NPS;

        public ImportBoundaryForm()
        {
            InitializeComponent();

            m_NPS = NPSGlobal.Instance;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            string errorMessage="";
            IFeature importFeature;


            //make sure we have a shapefile path
            if (string.IsNullOrEmpty(txtShapeFilePath.Text.Trim()))
            {
                MessageBox.Show("Please provide the path to the polygon ShapeFile containing the survey areas to import.");
                return;
            }

            //get the shape file at the specified path - make sure it's good
            IFeatureClass selShapeFile = Util.GetShapeFile(txtShapeFilePath.Text, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                MessageBox.Show(errorMessage);
                return;
            }

            //make sure shapefile contains polygons
            if (selShapeFile.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show("The selected ShapeFile is not a polygon shapefile");
                return;
            }

            //make sure there are features to import
            if (selShapeFile.FeatureCount(null) == 0)
            {
                MessageBox.Show("There are no features in this ShapeFile");
                return;
            }

            //make sure the shapefile has the same spatial reference as the NPS geodatabase featureclasses
            ISpatialReference defaultSpatRef = Util.GetDefaultSpatialReference();
            ISpatialReference importSpatRef = ((IGeoDataset)selShapeFile).SpatialReference;
            if (Util.CompareSpatialReference(defaultSpatRef, importSpatRef)==false)
            {
                MessageBox.Show("(Err) The selected ShapeFile has the coordinate system '"
                + importSpatRef.Name + "' which is different "
                + "from that of the NPS database coordinate system which is '"
                + defaultSpatRef.Name + "'.");
                return;
            }

            //make sure we have the survey boundary feature class
            IFeatureClass surveyBoundaryFc = Util.GetFeatureClass(m_NPS.LYR_SURVEY_BOUNDARY,
                                                                  m_NPS.Workspace, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                MessageBox.Show(errorMessage);
                return;
            }

            //get boundary fc survey field indexes
            int surveyIdIndex = surveyBoundaryFc.FindField("SurveyID");
            int surveyNameIndex = surveyBoundaryFc.FindField("SurveyName");
            int parkIndex = surveyBoundaryFc.FindField("Park");
            int commentsIndex = surveyBoundaryFc.FindField("Comments");

            //missing valid fields - can't go futher
            if (surveyIdIndex == -1 || surveyNameIndex == -1 || parkIndex == -1 || commentsIndex == -1)
            {
                MessageBox.Show("Survey Boundary FeatureClass is missing important fields needed for import");
                return;
            }

            //get import survey field indexes - may or may not have fields
            int commentsImpIndex = selShapeFile.FindField("Comments");
            int parkImpIndex = selShapeFile.FindField("Park");
            int surveyNameImpIndex = selShapeFile.FindField("SurveyName");


            //insert cursor for survey boundary
            IFeatureCursor thisFCursor = surveyBoundaryFc.Insert(true);
            IFeatureBuffer thisFBuffer = surveyBoundaryFc.CreateFeatureBuffer();

            var form = new NewSurveyBoundaryForm();


            //try to get the next valid survey id
            int nextSurveyId = Util.NextSurveyID(ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                MessageBox.Show(errorMessage);
                return;
            }
            
            int ImportCount = 0;

            //loop through each shapefile boundary and inport it into database
            IFeatureCursor importCursor = selShapeFile.Search(null, false);
            while ((importFeature = importCursor.NextFeature()) != null)
            {
                if (importFeature.Shape == null) continue;

                thisFBuffer.Shape = importFeature.ShapeCopy;

                //if the shapefile has fields for the boundary fc get, their values
                string surveyName = surveyNameImpIndex == -1 ? "" :
                                        (string)Util.SafeConvert(importFeature.Value[surveyNameImpIndex], typeof(string));
                surveyName = surveyName.Trim();

                string comments = commentsImpIndex == -1 ? "" :
                                      (string)Util.SafeConvert(importFeature.Value[commentsImpIndex], typeof(string));
                comments = comments.Trim();

                string park = parkImpIndex == -1 ? "" :
                                  (string)Util.SafeConvert(importFeature.Value[parkImpIndex], typeof(string));
                park = park.Trim();

                //if user didn't check don't ask again and there isn't a survey name field or valid survey name
                //value, we will show the form to get user data
                if (form.ckbDontAskAgain.Checked == false && string.IsNullOrEmpty(surveyName))
                {
                    form.txtPark.Text = park;
                    form.txtComments.Text = comments;
                    form.txtSurveyName.Text = surveyName;
                    form.txtSurveyID.Text = Convert.ToString(nextSurveyId);

                    form.ShowDialog();

                    comments = form.txtComments.Text;
                    park = form.txtPark.Text;
                    surveyName = form.txtSurveyName.Text;
                }

                //if we don't have a survey name, the survey id will stand in for the
                //survey name
                surveyName = string.IsNullOrEmpty(surveyName.Trim()) ? Convert.ToString(nextSurveyId) : surveyName;

                //set values for new boundary
                thisFBuffer.Value[commentsIndex] = comments;
                thisFBuffer.Value[parkIndex] = park;
                thisFBuffer.Value[surveyNameIndex] = surveyName;
                thisFBuffer.Value[surveyIdIndex] = nextSurveyId;

                //insert boundary
                thisFCursor.InsertFeature(thisFBuffer);

                //get next valid surveyid
                nextSurveyId++;

                //count the number of features imported
                ImportCount++;
            }

            MessageBox.Show("Import completed successfully. " + ImportCount + " feature(s) imported.");

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            bool cancelled = false;

            string newPath = Util.OpenESRIDialog(txtShapeFilePath.Text, ref cancelled);
            if (cancelled == false) txtShapeFilePath.Text = newPath;

        }
    }
}
