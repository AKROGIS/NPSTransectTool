using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace NPSTransectTool
{
    public partial class StraightLineForm : Form
    {
        private readonly NPSGlobal m_NPS;
        private string m_ErrorMessage;

        public StraightLineForm()
        {
            InitializeComponent();
            m_NPS = NPSGlobal.Instance;
            m_ErrorMessage = "";
        }

        private void StraightLineForm_Load(object sender, EventArgs e)
        {
            int resultsCount = 0;
            string errorMessage = "";
            string animalDisplayMessage = "";
            string horizonDisplayMessage = "";
            string defExpress = "";
            bool isSelection = false;

            Util.GetLayerSelection(m_NPS.LYR_ANIMALS, true, true,
                                   ref resultsCount, ref isSelection, ref defExpress, ref m_ErrorMessage);

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                txtMessage.Text = m_ErrorMessage;
                return;
            }

            if (isSelection)
                animalDisplayMessage += "Selected Sightings Count  : " + resultsCount + "\r\n";

            if (string.IsNullOrEmpty(defExpress) == false)
                animalDisplayMessage += "Sightings Definition Query: " + defExpress + "\r\n";

            if (string.IsNullOrEmpty(animalDisplayMessage))
                animalDisplayMessage += "All Sightings in the database will be processed.\r\n";


            Util.GetLayerSelection(m_NPS.LYR_HORIZON, true, true,
                                   ref resultsCount, ref isSelection, ref defExpress, ref errorMessage);

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                txtMessage.Text = m_ErrorMessage;
                return;
            }

            if (isSelection)
                horizonDisplayMessage += "Selected Horizons Count  : " + resultsCount + "\r\n";

            if (string.IsNullOrEmpty(defExpress) == false)
                horizonDisplayMessage += "Horizons Definition Query: " + defExpress + "\r\n";

            if (string.IsNullOrEmpty(animalDisplayMessage))
                horizonDisplayMessage += "All Horizons in the database will be processed.\r\n";


            txtMessage.Text = animalDisplayMessage + "\r\n\r\n" + horizonDisplayMessage;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            const string helpText = @"
            ** Instructions **

            The Straight-Line Tool is used to recalculate the distance of sightings' and horizons' 
            to the closest segment on the transect they were observed. The tool may be run on all 
            sightings and horizons for all surveys in the database or just a subset. To specify a 
            subset, either:

                a. Create a selection of sightings and/or horizons which distance will be recalculated
                b. Specify a definition query on the animals (DB Animal) and/or the Horizon layer by 
                   right-clicking on the layer and navigating to Properties-->Definition Query

            If a subset is not specified, then all sightings and horizons in the database will have 
            their distance recalculated. 

            After deciding which sightings will be processed, click 'Run' to begin the 
            distance recalculation. ";

            using (var form = new HelpMessageForm())
            {
                form.txtHelpMessage.Text = helpText;
                form.ShowDialog();
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            int selectionCount = 0;
            int animalUpdateCount;
            int horizonUpdateCount = 0;
            string errorMessage = "";
            string defExpress = "";
            bool isSelection = false;

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                MessageBox.Show(m_ErrorMessage);
                return;
            }

            IFeatureCursor animalCursor = Util.GetLayerSelection(m_NPS.LYR_ANIMALS, true, true,
                                                                 ref selectionCount, ref isSelection, ref defExpress,
                                                                 ref m_ErrorMessage);

            IFeatureCursor HorizonCursor = Util.GetLayerSelection(m_NPS.LYR_HORIZON, true, true,
                                                                  ref selectionCount, ref isSelection, ref defExpress, ref errorMessage);

            RecalDistance(animalCursor, out animalUpdateCount, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                MessageBox.Show(errorMessage);
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                RecalDistance(HorizonCursor, out horizonUpdateCount, ref errorMessage);
                if (string.IsNullOrEmpty(errorMessage) == false)
                {
                    MessageBox.Show(errorMessage);
                }
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show("Update completed successfully!\r\n\r\nSightings updated: " + animalUpdateCount
                                + "\r\nHorizons updated: " + horizonUpdateCount);
            }

        }

        private void RecalDistance(IFeatureCursor thisCursor, out int updateCount, ref string errorMessage)
        {
            int transectIdIndex = 0;
            int distanceFieldIndex = 0;
            int surveyIdIndex = 0;
            double distanceAlongCurve = 0;
            double distanceFromCurve = 0;
            bool rightSide = false;


            updateCount = 0;

            //make sure we got the  transect lines featureclass
            IFeatureClass transLinesFc = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, m_NPS.Workspace, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
                return;

            //get first record
            IFeature thisFeature = thisCursor.NextFeature();

            //if we have no feature here then the recordset is empty
            if (thisFeature == null)
                return;


            //make sure we have the transect field
            if (string.IsNullOrEmpty(errorMessage))
            {
                transectIdIndex = thisFeature.Fields.FindField("TransectID");
                if (transectIdIndex < 0) errorMessage = "No transect id field found.";
            }

            //make sure we have the transect field
            if (string.IsNullOrEmpty(errorMessage))
            {
                surveyIdIndex = thisFeature.Fields.FindField("SurveyID");
                if (surveyIdIndex < 0) errorMessage = "No survey id field found.";
            }

            //make sure we have a distance field
            if (string.IsNullOrEmpty(errorMessage))
            {
                distanceFieldIndex = thisFeature.Fields.FindField("DIST2TRANS");
                if (distanceFieldIndex < 0) distanceFieldIndex = thisFeature.Fields.FindField("DistToSeg");
                if (distanceFieldIndex < 0) errorMessage = "No distance field found.";
            }

            //things not okay so abort
            if (!string.IsNullOrEmpty(errorMessage))
                return;


            //loop through each feature, get it's transect segments and find the nearest one
            do
            {
                //get transect id for feature
                var transectId = (string) Util.SafeConvert(thisFeature.Value[transectIdIndex], typeof (string));
                if (string.IsNullOrEmpty(transectId)) continue;

                //get survey id for feature
                var surveyId = (string) Util.SafeConvert(thisFeature.Value[surveyIdIndex], typeof (string));
                if (string.IsNullOrEmpty(surveyId)) continue;

                //get point shape
                var thisPoint = thisFeature.ShapeCopy as IPoint;


                //get all segments on transect
                IQueryFilter qFilter = new QueryFilterClass();
                qFilter.WhereClause = "TransectID=" + transectId + " and SurveyID=" + surveyId
                                      + " and SegType='OnTransect'";
                IFeatureCursor transectFCursor = transLinesFc.Search(qFilter, false);


                double closestDistance = 0;
                bool firstRecord = true;

                //check the distance of all segments and get the one nearest to the feature
                IFeature transectFFeature;
                while ((transectFFeature = transectFCursor.NextFeature()) != null)
                {
                    var thisCurve = transectFFeature.ShapeCopy as ICurve;

                    //var segID = (int) Util.SafeConvert(transectFFeature.Value[transectFFeature.Fields.FindField("SegmentID")], typeof (int));

                    //determine the distance between the two
                    if (thisCurve != null && thisPoint != null)
                    {
                        thisCurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, thisPoint, false, null,
                                                        ref distanceAlongCurve, ref distanceFromCurve, ref rightSide);
                    }

                    //if this is the first record then the first distance is the currently closest distance
                    if (firstRecord)
                    {
                        closestDistance = distanceFromCurve;
                        firstRecord = false;
                    }

                    //if the current distance is less than the last distance, it now becomes the closest distance
                    if (distanceFromCurve < closestDistance)
                        closestDistance = distanceFromCurve;
                }

                //if first record is still true then there were no transects to check for distance so don't update
                if (firstRecord == false)
                {
                    updateCount = updateCount + 1;

                    //update the feature with the closest distance
                    thisFeature.Value[distanceFieldIndex] = closestDistance;

                    //save new distance value
                    thisCursor.UpdateFeature(thisFeature);
                }
            } while ((thisFeature = thisCursor.NextFeature()) != null);
        }
    }
}