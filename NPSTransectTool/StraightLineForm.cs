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
    public partial class StraightLineForm : Form
    {
        NPSGlobal m_NPS;
        string m_ErrorMessage;

        public StraightLineForm()
        {
            InitializeComponent();
            m_NPS = NPSGlobal.Instance;
            m_ErrorMessage = "";
        }

        private void StraightLineForm_Load(object sender, EventArgs e)
        {
            IFeatureCursor AnimalCursor, HorizonCursor;
            int ResultsCount = 0;
            string ErrorMessage = "", AnimalDisplayMessage = "", HorizonDisplayMessage = "", DefExpress = "";
            bool IsSelection = false;

            AnimalCursor = Util.GetLayerSelection(m_NPS.LYR_ANIMALS, true, true,
                ref ResultsCount, ref IsSelection, ref DefExpress, ref m_ErrorMessage);

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                this.txtMessage.Text = m_ErrorMessage;
                return;
            }

            if (IsSelection)
                AnimalDisplayMessage += "Selected Sightings Count  : " + ResultsCount + "\r\n";

            if (string.IsNullOrEmpty(DefExpress) == false)
                AnimalDisplayMessage += "Sightings Definition Query: " + DefExpress + "\r\n";

            if (string.IsNullOrEmpty(AnimalDisplayMessage))
                AnimalDisplayMessage += "All Sightings in the database will be processed.\r\n";



            HorizonCursor = Util.GetLayerSelection(m_NPS.LYR_HORIZON, true, true,
                ref ResultsCount, ref IsSelection, ref DefExpress, ref ErrorMessage);

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                this.txtMessage.Text = m_ErrorMessage;
                return;
            }

            if (IsSelection)
                HorizonDisplayMessage += "Selected Horizons Count  : " + ResultsCount + "\r\n";

            if (string.IsNullOrEmpty(DefExpress) == false)
                HorizonDisplayMessage += "Horizons Definition Query: " + DefExpress + "\r\n";

            if (string.IsNullOrEmpty(AnimalDisplayMessage))
                HorizonDisplayMessage += "All Horizons in the database will be processed.\r\n";


            this.txtMessage.Text = AnimalDisplayMessage + "\r\n\r\n" + HorizonDisplayMessage;

            AnimalCursor = null;
            HorizonCursor = null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            string HelpText = @"
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

            using (HelpMessageForm form = new HelpMessageForm())
            {
                form.txtHelpMessage.Text = HelpText;
                form.ShowDialog();
            }

        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            IFeatureCursor AnimalCursor, HorizonCursor;
            int SelectionCount = 0, AnimalUpdateCount = 0, HorizonUpdateCount = 0;
            string ErrorMessage = "", DefExpress = "";
            bool IsSelection = false;

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                MessageBox.Show(m_ErrorMessage);
                return;
            }

            AnimalCursor = Util.GetLayerSelection(m_NPS.LYR_ANIMALS, true, true,
               ref SelectionCount, ref IsSelection, ref DefExpress, ref m_ErrorMessage);

            HorizonCursor = Util.GetLayerSelection(m_NPS.LYR_HORIZON, true, true,
               ref SelectionCount, ref IsSelection, ref DefExpress, ref ErrorMessage);

            RecalDistance(AnimalCursor, ref AnimalUpdateCount, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                RecalDistance(HorizonCursor, ref HorizonUpdateCount, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    MessageBox.Show(ErrorMessage);
                }
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                MessageBox.Show("Update completed successfully!\r\n\r\nSightings updated: " + AnimalUpdateCount
                    + "\r\nHorizons updated: " + HorizonUpdateCount);
            }

            AnimalCursor = null;
            HorizonCursor = null;
        }

        private void RecalDistance(IFeatureCursor ThisCursor, ref int UpdateCount, ref string ErrorMessage)
        {

            IFeature ThisFeature, TransectFFeature;
            string TransectID, SurveyID;
            IFeatureClass TransLinesFC;
            IFeatureCursor TransectFCursor;
            int TransectIDIndex = 0, DistanceFieldIndex = 0, SurveyIDIndex = 0;
            IQueryFilter qFilter;
            ICurve ThisCurve;
            IPoint ThisPoint, pOutPoint = null;
            double DistanceAlongCurve = 0, DistanceFromCurve = 0, ClosestDistance = 0;
            bool RightSide = false, FirstRecord = false;


            UpdateCount = 0;

            //make sure we got the  transect lines featureclass
            TransLinesFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, m_NPS.Workspace, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
                return;

            //get first record
            ThisFeature = ThisCursor.NextFeature();

            //if we have no feature here then the recordset is empty
            if (ThisFeature == null)
            {
                ThisCursor = null;
                return;
            }


            //make sure we have the transect field
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                TransectIDIndex = ThisFeature.Fields.FindField("TransectID");
                if (TransectIDIndex < 0) ErrorMessage = "No transect id field found.";

            }

            //make sure we have the transect field
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                SurveyIDIndex = ThisFeature.Fields.FindField("SurveyID");
                if (SurveyIDIndex < 0) ErrorMessage = "No survey id field found.";

            }

            //make sure we have a distance field
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                DistanceFieldIndex = ThisFeature.Fields.FindField("DIST2TRANS");
                if (DistanceFieldIndex < 0) DistanceFieldIndex = ThisFeature.Fields.FindField("DistToSeg");
                if (DistanceFieldIndex < 0) ErrorMessage = "No distance field found.";
            }

            //things not okay so abort
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ThisCursor = null;
                return;
            }

            //loop through each feature, get it's transect segments and find the nearest one
            do
            {

                //get transect id for feature
                TransectID = (string)Util.SafeConvert(ThisFeature.get_Value(TransectIDIndex), typeof(string));
                if (string.IsNullOrEmpty(TransectID)) continue;

                //get survey id for feature
                SurveyID = (string)Util.SafeConvert(ThisFeature.get_Value(SurveyIDIndex), typeof(string));
                if (string.IsNullOrEmpty(SurveyID)) continue;

                //get point shape
                ThisPoint = ThisFeature.ShapeCopy as IPoint;


                //get all segments on transect
                qFilter = new QueryFilterClass();
                qFilter.WhereClause = "TransectID=" + TransectID + " and SurveyID=" + SurveyID
                    + " and SegType='OnTransect'";
                TransectFCursor = TransLinesFC.Search(qFilter, false);



                ClosestDistance = 0;
                FirstRecord = true;

                //check the distance of all segments and get the one nearest to the feature
                while ((TransectFFeature = TransectFCursor.NextFeature()) != null)
                {

                    ThisCurve = TransectFFeature.ShapeCopy as ICurve;

                    int segID = (int)Util.SafeConvert(TransectFFeature.get_Value(
                        TransectFFeature.Fields.FindField("SegmentID")),typeof(int));

                    //determine the distance between the two
                    if (ThisCurve != null && ThisPoint != null)
                    {
                        ThisCurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, ThisPoint, false, pOutPoint,
                            ref DistanceAlongCurve, ref DistanceFromCurve, ref RightSide);
                    }

                    //if this is the first record then the first distance is the currently closest distance
                    if (FirstRecord == true)
                    {
                        ClosestDistance = DistanceFromCurve;
                        FirstRecord = false;
                    }

                    //if the current distance is less than the last distance, it now becomes the closest distance
                    if (DistanceFromCurve < ClosestDistance)
                        ClosestDistance = DistanceFromCurve;


                }

                //release transect cursor
                TransectFCursor = null;

                //if first record is still true then there were no transects to check for distance so don't update
                if (FirstRecord == false)
                {

                    UpdateCount = UpdateCount + 1;

                    //update the feature with the closest distance
                    ThisFeature.set_Value(DistanceFieldIndex, ClosestDistance);

                    //save new distance value
                    ThisCursor.UpdateFeature(ThisFeature);

                }

            } while ((ThisFeature = ThisCursor.NextFeature()) != null);



        }


    }
}
