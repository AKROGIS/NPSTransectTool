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
    public partial class TransectToolForm : Form
    {
        NPSGlobal m_NPS;
        Dictionary<string, int> m_SurveysList;

        public TransectToolForm()
        {
            InitializeComponent();

            m_NPS = NPSGlobal.Instance;
            m_NPS.MainTransectForm = this;
        }

        private void TransectToolForm_Load(object sender, EventArgs e)
        {

            //set the official list of DEM fileunits
            cboDEMFileUnits.Items.Add("(select DEM file units)");
            cboDEMFileUnits.Items.Add("Meters");
            cboDEMFileUnits.Items.Add("Feet");
            cboDEMFileUnits.SelectedIndex = 0;

            //get all surveys
            m_SurveysList = Util.GetSurveysList();
            m_SurveysList.Add("[Select Survey]", -1);

            //order surveys and add to survey drop down
            var sortedDict = (from entry in m_SurveysList orderby entry.Key ascending select entry);
            foreach (KeyValuePair<string, int> row in sortedDict)
                cboSurveysList.Items.Add(row.Key);

            Util.ManageSavedValues(this, SavedValuesAction.Load);

            //if user didn't asked that last survey selected be remembered, select first option
            if (ckbRememberLastSurvey.Checked == false || string.IsNullOrEmpty(cboSurveysList.Text) == true)
                cboSurveysList.SelectedIndex = 0;

            //if we don't have a DEM file disable units dropdown
            if (string.IsNullOrEmpty(txtDemFileLocation.Text))
                cboDEMFileUnits.Enabled = false;
        }

        private void btnGenerateExcludedAreas_Click(object sender, EventArgs e)
        {
            int SurveyID, MaxElevation;
            IGeoDataset ThisGeoRasterDS = null;
            string ErrorMessage = "";


            //make sure a survey id was selected
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1) ErrorMessage = "Please select a survey";


            //make sure the elevation value is valid
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.IsNumeric(txtMaxElevation.Text) == false)
                    ErrorMessage = "Please enter a valid numeric value for max elevation";

           

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //get the DEM specified by the DEM filepath field
                ThisGeoRasterDS = Util.OpenRasterDataset(txtDemFileLocation.Text, ref ErrorMessage) as IGeoDataset;
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                    ErrorMessage = ErrorMessage + "\r\n\r\nPlease set a valid DEM file from the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage = "Please set a valid unit for the DEM file on the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                Util.SetProgressMessage("Clipping Raster", 8);

                //clip the dem file to the size of the survey area
                ThisGeoRasterDS = Util.ClipRasterByBndPoly(ThisGeoRasterDS, Convert.ToString(SurveyID), ref ErrorMessage);
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //if we are replaceing all existing excluded areas for this survey, delete those features now
                if (ckbReplaceExcludedAreas.Checked)
                {
                    Util.DeleteFeatures(m_NPS.LYR_EXCLUDED_AREAS, "SurveyID=" + SurveyID, ref ErrorMessage);
                    ErrorMessage = "";
                }


                MaxElevation = Convert.ToInt32(txtMaxElevation.Text);

                try
                {
                    //build excluded area polygons for the specified survey area and the specified elevation value
                    Util.GenerateExcludedAreasPolygons(SurveyID, MaxElevation, rdbAboveElevation.Checked,
                        ThisGeoRasterDS, ref ErrorMessage);

                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message;
                }

                m_NPS.Document.ActiveView.Refresh();


            }


            Util.SetProgressMessage("");

            SetSurveyProgressLabels(SurveyID);

            if (string.IsNullOrEmpty(ErrorMessage))
                MessageBox.Show("Completed Successfully");
            else
                MessageBox.Show(ErrorMessage);
        }

        private void btnGenerateFlatAreas_Click(object sender, EventArgs e)
        {
            int SurveyID, MaxSlope = -1, MinSlope = -1;
            IGeoDataset ThisGeoRasterDS = null;
            string ErrorMessage = "";

            //make sure a survey id has been set
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1) ErrorMessage = "Please select a survey";


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //make sure the max and min slope are valid
                MaxSlope = (int)Util.SafeConvert(txtMaxSlope.Text, typeof(int));
                MinSlope = (int)Util.SafeConvert(txtMinSlope.Text, typeof(int));
                if (MaxSlope == -1 || MinSlope == -1) ErrorMessage = "Maximum and minimum slope must be valid integer values";

            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //get the DEM specified by the DEM filepath field
                ThisGeoRasterDS = Util.OpenRasterDataset(txtDemFileLocation.Text, ref ErrorMessage) as IGeoDataset;
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                    ErrorMessage += "\r\n\r\nPlease set a valid DEM file from the Config tab.";

            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage = "Please set a valid unit for the DEM file on the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                Util.SetProgressMessage("Clipping Raster", 9);

                //clip the dem file to the size of the survey area
                ThisGeoRasterDS = Util.ClipRasterByBndPoly(ThisGeoRasterDS, Convert.ToString(SurveyID), ref ErrorMessage);
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //if we are replaceing all existing flat areas for this survey, delete those features now
                if (ckbReplaceAllFlatAreas.Checked)
                {
                    Util.DeleteFeatures(m_NPS.LYR_FLAT_AREAS, "SurveyID=" + SurveyID, ref ErrorMessage);
                    ErrorMessage = "";
                }

                try
                {
                    //build flat area polygons for the specified survey area and the specified min/max slope
                    Util.GenerateFlatAreaPolygons(SurveyID, MinSlope, MaxSlope, ThisGeoRasterDS, ref ErrorMessage);

                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message;
                }



                m_NPS.Document.ActiveView.Refresh();
            }

            SetSurveyProgressLabels(SurveyID);

            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(ErrorMessage))
                MessageBox.Show("Completed Successfully");
            else
                MessageBox.Show(ErrorMessage);

        }

        private void btnGenerateRandPts_Click(object sender, EventArgs e)
        {
            int SurveyID, TotalRandPts = 0, NewBatchID = 0, RandPointGenerated = 0;
            string ErrorMessage = "", DEMUnits="";
            double StartX = -1, StartY = -1, GridPointSpacing = 0;
            IRasterDataset ThisRasterDS = null;
            ESRI.ArcGIS.Geometry.IPolygon SurveyBoundary;
            double SurveyArea;


            //make sure a survey id was selected
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1) ErrorMessage = "Please select a survey";

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //get the DEM specified by the DEM filepath field
                ThisRasterDS = Util.OpenRasterDataset(txtDemFileLocation.Text, ref ErrorMessage) as IRasterDataset;
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                    ErrorMessage += "\r\n\r\nPlease set a valid DEM file from the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage = "Please set a valid unit for the DEM file on the Config tab.";
                else
                    DEMUnits = cboDEMFileUnits.Text;
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (rdbRandomPoints.Checked)
                {
                    if (string.IsNullOrEmpty(ErrorMessage))
                        if (Util.IsNumeric(txtTotalRandomPoints.Text) == false)
                            ErrorMessage = "Total random points field must be a valid integer value";

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        TotalRandPts = Convert.ToInt32(txtTotalRandomPoints.Text);
                        if (TotalRandPts < 2) ErrorMessage = "2 is the minimum nuber of random points that can be generated.";
                    }
                }
                else
                {
                    if (Util.IsNumeric(txtGridPointSpacing.Text) == false)
                        ErrorMessage = "(Grid Point Spacing must be a valid numeric value";
                    else
                        GridPointSpacing = Convert.ToDouble(txtGridPointSpacing.Text);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        if (txtStartX.Text.Trim() != "")
                            if (Util.IsNumeric(txtStartX.Text) == false)
                                ErrorMessage = "The X position set is an invalid value";
                            else
                                StartX = Convert.ToDouble(txtStartX.Text);
                    }

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        if (txtStartY.Text.Trim() != "")
                            if (Util.IsNumeric(txtStartY.Text) == false)
                                ErrorMessage = "The Y position set is an invalid value";
                            else
                                StartY = Convert.ToDouble(txtStartY.Text);
                    }
                }
            }


            if (string.IsNullOrEmpty(ErrorMessage) && rdbGridPoints.Checked == true)
            {
                //
                // If a grid is being generated, and the [area of the survey] is less than  [Grid spacing^2] * 2 then
                //  cancel the operation with a warning:  "The Grid size you entered would not produce enought points"
                //

                SurveyBoundary = Util.GetSurveyBoundary(Convert.ToString(SurveyID), ref ErrorMessage);

                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    SurveyArea = ((ESRI.ArcGIS.Geometry.IArea)SurveyBoundary).Area;

                    if (SurveyArea < GridPointSpacing)
                        ErrorMessage = "The Grid size you entered would not produce enought points";
                }

            }

            if (string.IsNullOrEmpty(ErrorMessage))
                //make sure we have elevation/excluded areas for this survey
                if (Util.FeatureCount(m_NPS.LYR_EXCLUDED_AREAS, "SurveyID=" + SurveyID) == 0)
                    ErrorMessage = "No Elevation Areas found in " + m_NPS.LYR_EXCLUDED_AREAS + " for SurveyID " + SurveyID + "."
                       + " A SurveyID must have\r\nElevation Areas (Step 1) before random points can be generated for that Survey.";


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                //if we are replacing all existing radom points for this survey, delete those features now
                if (ckbReplaceAllRandPts.Checked)
                {
                    Util.DeleteFeatures(m_NPS.LYR_RANDOMPOINTS, "SurveyID=" + SurveyID, ref ErrorMessage);
                    ErrorMessage = "";
                }

                try
                {
                    //generate random points
                    if (rdbRandomPoints.Checked)
                    {
                        Util.SetProgressMessage("Generating random points", 6);
                        NewBatchID = Util.GenerateRandomPoints(SurveyID, TotalRandPts, ref ErrorMessage);
                    }
                    //genrate points in grid style
                    else
                    {
                        Util.SetProgressMessage("Generating grid points", 6);
                        NewBatchID = Util.GenerateGridPoints(SurveyID, StartX, StartY, GridPointSpacing, ref ErrorMessage);
                    }

                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message;

                }
            }


            if (string.IsNullOrEmpty(ErrorMessage))
            {
                RandPointGenerated = Util.FeatureCount(m_NPS.LYR_RANDOMPOINTS, "SurveyID=" + SurveyID);

                if (RandPointGenerated < 2)
                {
                    ErrorMessage = "A minimum of 2 random points must be generated"
                    + " in order for the Point Generation process to continue. Only "
                    + RandPointGenerated + " point(s) were generated. ";

                    if (rdbRandomPoints.Checked)
                        ErrorMessage += "Please try increasing the number of random points to generate.";
                    else
                        ErrorMessage += "Please try decreasing the grid spacing.";
                }
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                try
                {
                    //add elevation values to random points
                    Util.AddZValuesToPoints(SurveyID, NewBatchID, ThisRasterDS as IGeoDataset,DEMUnits, ref ErrorMessage);
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message;
                }

                m_NPS.Document.ActiveView.Refresh();
            }


            SetSurveyProgressLabels(SurveyID);

            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(ErrorMessage))
                MessageBox.Show("Completed Successfully");
            else
                MessageBox.Show(ErrorMessage);
        }

        private void btnGenerateTransLines_Click(object sender, EventArgs e)
        {
            int SurveyID, TransCreateAttempts = 0, TotalRandomPoints = 0, SpecifiedRandomPoints = -1,
                StraightCount = 0, ContourCount = 0, FailCount = 0, NewBacthID = 0;
            IGeoDataset ThisGeoRasterDS = null;
            string ErrorMessage = "", ResultMessage = "";

            //make sure a survey id was selected
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1) ErrorMessage = "Please select a survey";


            //get the number of attempts we'll make at trying to create a side of the transect
            TransCreateAttempts = Util.IsNumeric(txtMaxAttempts.Text) ? Convert.ToInt32(txtMaxAttempts.Text) : 25;



            //make sure we ahve excluded areas for this survey
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.FeatureCount(m_NPS.LYR_EXCLUDED_AREAS, "SurveyID=" + SurveyID) == 0)
                    ErrorMessage = "No excluded areas were found for this survey id. A survey"
                    + " must have excluded areas (Step 1) before transect lines can be generate for that survey.";


            //make sure we have flat areas for this survey
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.FeatureCount(m_NPS.LYR_FLAT_AREAS, "SurveyID=" + SurveyID) == 0)
                    ErrorMessage = "No flat areas were found for this survey id. A survey"
                    + " must have flat areas (Step 2) before transect lines can be generate for that survey.";


            //make sure we have random points
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                TotalRandomPoints = Util.FeatureCount(m_NPS.LYR_RANDOMPOINTS, "SurveyID=" + SurveyID);
                if (TotalRandomPoints == 0)
                    ErrorMessage = "No random points were found for this survey id. A "
                       + "survey must have random points (Step 3) before "
                       + "\r\ntransect lines can be generate for that survey.";

            }

            //if the user set a max number of transects to generate, validate it
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.IsNumeric(txtTransectTotal.Text))
                {
                    SpecifiedRandomPoints = Convert.ToInt32(txtTransectTotal.Text);
                    if (SpecifiedRandomPoints > TotalRandomPoints)
                        ErrorMessage = "Transects are generated from random points. You cannot "
                           + " create more transects than there are random points. Currently, there are "
                           + " a total of " + TotalRandomPoints + " available ";

                }

            //if the user set a valid number of transects to generate, set it
            if (SpecifiedRandomPoints > -1) TotalRandomPoints = SpecifiedRandomPoints;

            //make sure the transect line max and min lengths are specified
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.IsNumeric(txtMinTransLineLength.Text) == false || Util.IsNumeric(txtMaxTransLineLength.Text) == false)
                    ErrorMessage = "Transect line max and min lengths and total must be valid numerical values";


            //check target length
            if (string.IsNullOrEmpty(ErrorMessage))
                if (Util.IsNumeric(txtTargetLength.Text) == false)
                    ErrorMessage = "Target length must be a valid numerical value";


            //get the DEM specified by the DEM filepath field
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ThisGeoRasterDS = Util.OpenRasterDataset(txtDemFileLocation.Text, ref ErrorMessage) as IGeoDataset;
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                    ErrorMessage += "\r\n\r\nPlease set a valid DEM file from the Config tab.";
            }

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                if (cboDEMFileUnits.SelectedIndex == 0)
                    ErrorMessage =  "Please set a valid unit for the DEM file from the Config tab.";
            }

            Util.SetProgressMessage("Clipping Raster", 2);

            //clip the dem file to the size of the survey area
            //if (string.IsNullOrEmpty(ErrorMessage))
            //    ThisGeoRasterDS = Util.ClipRasterByBndPoly(ThisGeoRasterDS, Convert.ToString(SurveyID), ref ErrorMessage);


            //if we are replaceing all existing transects for this survey, delete those features now
            if (string.IsNullOrEmpty(ErrorMessage))
                if (ckbReplaceAllTransLines.Checked)
                {
                    Util.DeleteFeatures(m_NPS.LYR_GENERATED_TRANSECTS, "SurveyID=" + SurveyID, ref ErrorMessage);
                    ErrorMessage = "";
                }

            if (string.IsNullOrEmpty(ErrorMessage))
            {

                try
                {
                    Util.GenerateTransectLines(SurveyID, Convert.ToDouble(txtMaxTransLineLength.Text),
                        Convert.ToDouble(txtMinTransLineLength.Text), ThisGeoRasterDS, TransCreateAttempts,
                        TotalRandomPoints, Convert.ToDouble(txtTargetLength.Text), ref NewBacthID, ref StraightCount,
                        ref ContourCount, ref FailCount, ref ErrorMessage);

                    ResultMessage = "Straight: " + StraightCount + ", Contour: " + ContourCount + ", Failed: " + FailCount;

                    if ((StraightCount + ContourCount) < TotalRandomPoints)
                    {
                        ResultMessage = "Generation of Transect Lines successful! Only " + (StraightCount + ContourCount)
                            + " transects could be generated \r\nfrom the " + TotalRandomPoints
                            + " points available.\r\n\r\n" + ResultMessage;
                    }
                    else
                    {
                        ResultMessage = "Generation of Transect Lines successful!\r\n\r\n" + ResultMessage;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error occured. " + ErrorMessage + ". " + ex.Message + "\n" + ex.StackTrace;
                }

                m_NPS.Document.ActiveView.Refresh();
            }


            SetSurveyProgressLabels(SurveyID);

            Util.SetProgressMessage("");

            if (string.IsNullOrEmpty(ErrorMessage))
                MessageBox.Show(ResultMessage);
            else
                MessageBox.Show(ErrorMessage);

        }

        private void SetSurveyProgressLabels(int SurveyID)
        {
            //don't waste time and query each fc if no survey set
            if (SurveyID == -1)
            {
                lblStepOne.BackColor = SystemColors.GradientActiveCaption;
                lblStepTwo.BackColor = SystemColors.GradientActiveCaption;
                lblStepThree.BackColor = SystemColors.GradientActiveCaption;
                lblStepFour.BackColor = SystemColors.GradientActiveCaption;
                return;
            }

            lblStepOne.BackColor = Util.FeatureCount(m_NPS.LYR_EXCLUDED_AREAS, "SurveyID=" + SurveyID) == 0 ?
                SystemColors.GradientActiveCaption : ColorTranslator.FromHtml("#6CCF4D");

            lblStepTwo.BackColor = Util.FeatureCount(m_NPS.LYR_FLAT_AREAS, "SurveyID=" + SurveyID) == 0 ?
               SystemColors.GradientActiveCaption : ColorTranslator.FromHtml("#6CCF4D");

            lblStepThree.BackColor = Util.FeatureCount(m_NPS.LYR_RANDOMPOINTS, "SurveyID=" + SurveyID) == 0 ?
               SystemColors.GradientActiveCaption : ColorTranslator.FromHtml("#6CCF4D");

            lblStepFour.BackColor = Util.FeatureCount(m_NPS.LYR_GENERATED_TRANSECTS, "SurveyID=" + SurveyID) == 0 ?
               SystemColors.GradientActiveCaption : ColorTranslator.FromHtml("#6CCF4D");
        }

        private void btnStraightLineTool_Click(object sender, EventArgs e)
        {
            using (StraightLineForm form = new StraightLineForm())
            {
                form.ShowDialog();
            }
        }

        private void btnEditArcPadDefaults_Click(object sender, EventArgs e)
        {
            using (EditArcPadSettings form = new EditArcPadSettings())
            {
                form.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Util.ManageSavedValues(this, SavedValuesAction.Save);
            m_NPS.MainTransectForm = null;
            Util.SaveConfigSettings();
            this.Close();
        }

        private void btnImportBoundary_Click(object sender, EventArgs e)
        {
            using (ImportBoundaryForm form = new ImportBoundaryForm())
            {
                form.ShowDialog();
            }
        }

        private void btnImportRndPntsOrTrans_Click(object sender, EventArgs e)
        {
            using (ImportTransRndPntsForm form = new ImportTransRndPntsForm(this))
            {
                form.ShowDialog();
            }
        }

        private void btnBrowseDEM_Click(object sender, EventArgs e)
        {
            bool Cancelled = false;
            string NewPath;

            NewPath = Util.OpenESRIDialog(txtDemFileLocation.Text, ref Cancelled);
            if (Cancelled == false)
            {
                txtDemFileLocation.Text = NewPath;
                cboDEMFileUnits.Enabled = true;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            int SurveyID;
            string SurveyFolderPath, ErrorMessage = "", FCFilter, DBFolderPath, ThisFieldValue, InternalErr = "";
            bool CantDeleteSoReuse = false;
            IFeatureClass ExportingFC;
            string[] FCsToExport;

            //get the selected survey id
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1)
            {
                MessageBox.Show("Please select a survey to import to.");
                return;
            }


            DBFolderPath = System.IO.Path.GetDirectoryName(m_NPS.DatabasePath);
            SurveyFolderPath = System.IO.Path.Combine(txtExportDataPath.Text, "Survey");

            //if the folder exists, try to delete
            if (System.IO.Directory.Exists(SurveyFolderPath))
            {
                //ask user if they are willing to delete existing folder
                if (MessageBox.Show(@"A folder named 'Survey' already exists at this "
                    + "location. Click 'No' to abort the 'Export' operation or 'Yes' "
                    + "to override this folder.", "Survey Folder Exists",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                //try to delete existing folder or at least empty it out for resuse
                Util.DeleteOrEmptyFolder(SurveyFolderPath, true, ref CantDeleteSoReuse, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    MessageBox.Show(ErrorMessage);
                    return;
                }
            }

            //if we are here and we are not reusing a folder, try to create the surve folder at the specified path
            if (CantDeleteSoReuse == false)
            {
                Util.CreateDirectory(SurveyFolderPath, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    MessageBox.Show(ErrorMessage);
                    return;
                }
            }

            Util.SetProgressMessage("Creating Survey directory", 4);

            //check if the APLs folder exists
            if (System.IO.Directory.Exists(System.IO.Path.Combine(DBFolderPath, "APLs")) == false)
            {

                MessageBox.Show("The APLs directory containing the ArcPad layers"
                    + "is missing from path " + m_NPS.DatabasePath);
                Util.SetProgressMessage("");
                return;
            }

            //if we have the APL folder, copy it to the survey folder
            Util.CopyFolder(System.IO.Path.Combine(DBFolderPath, "APLs"), SurveyFolderPath, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                Util.SetProgressMessage("");
                return;
            }

            Util.SetProgressMessage("Copying over required files");

            //check if the defaultvalues.dbf file exists
            //if (System.IO.File.Exists(System.IO.Path.Combine(DBFolderPath, "defaultvalues.dbf")) == false)
            //{
            //    MessageBox.Show("The defaultvalues.dbf file containing NPS ArcPad Applet settings "
            //       + "is missing from path " + m_NPS.DLLPath);
            //    Util.SetProgressMessage("");
            //    return;
            //}

            //copy the defaultvalues.dbf file to the survey folder
            //Util.CopyFile(System.IO.Path.Combine(DBFolderPath, "defaultvalues.dbf"),
            //    System.IO.Path.Combine(SurveyFolderPath, "defaultvalues.dbf"), ref ErrorMessage);
            //if (string.IsNullOrEmpty(ErrorMessage) == false)
            //{
            //    MessageBox.Show(ErrorMessage);
            //    Util.SetProgressMessage("");
            //    return;
            //}


            Util.SetProgressMessage("Exporting defaultvalues table");

            //update defaultvalues table with survey specific values
            ThisFieldValue = Util.GetFirstRecordValue(m_NPS.LYR_SURVEY_BOUNDARY, "SurveyID", "SurveyID=" + SurveyID);
            Util.SetArcPadDefaultValue("SurveyID", ThisFieldValue, ref InternalErr);

            ThisFieldValue = Util.GetFirstRecordValue(m_NPS.LYR_SURVEY_BOUNDARY, "Park", "SurveyID=" + SurveyID);
            Util.SetArcPadDefaultValue("Park", ThisFieldValue, ref InternalErr);

            ThisFieldValue = Util.GetFirstRecordValue(m_NPS.LYR_SURVEY_BOUNDARY, "SurveyName", "SurveyID=" + SurveyID);
            Util.SetArcPadDefaultValue("SurveyName", ThisFieldValue, ref InternalErr);

            //export the defaultvalues table that resides in the geodatbase to the survey folder
            Util.ExportDefaultValuesTable(SurveyFolderPath, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                Util.SetProgressMessage("");
                return;
            }

            FCFilter = "SurveyID=" + SurveyID;

            //build list of feature classes to export
            FCsToExport = new string[]{m_NPS.LYR_GENERATED_TRANSECTS,m_NPS.LYR_ANIMALS,
                                     m_NPS.LYR_TRACKLOG,m_NPS.LYR_HORIZON,m_NPS.LYR_GPSPOINTLOG};

            //export each feature class in list
            foreach (string FCToExportName in FCsToExport)
            {
                //"OBJECTID=-1" will create an empty feature class
                FCFilter = (FCToExportName == m_NPS.LYR_GENERATED_TRANSECTS) ? "SurveyID=" + SurveyID : "OBJECTID=-1";

                Util.SetProgressMessage("Building " + FCToExportName + " ShapeFile in Survey folder", false);

                //get fc
                ExportingFC = Util.GetFeatureClass(FCToExportName, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    MessageBox.Show(ErrorMessage);
                    Util.SetProgressMessage("");
                    return;
                }

                //export fc
                Util.GP_FeatureclassToShapefile_conversion(ExportingFC, SurveyFolderPath, FCFilter, ref ErrorMessage);
                if (string.IsNullOrEmpty(ErrorMessage) == false)
                {
                    MessageBox.Show(ErrorMessage);
                    Util.SetProgressMessage("");
                    return;
                }
            }

            Util.SetProgressMessage("");
            MessageBox.Show("Export completed successfully");

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            int SurveyID;
            string ErrorMessage = "", DataSurveyID = "", InternalErr = "";
            IFeatureClass SurveyFC, DatabaseFC;
            IWorkspace ShapeFileWS;
            List<string> ImportedTransects;
            FormOptions ThisFormOptions;
            bool HasExistingData = false;


            ThisFormOptions = FormOptions.None;

            //get the selected survey id
            SurveyID = m_SurveysList[(string)cboSurveysList.Text];
            if (SurveyID == -1)
            {
                MessageBox.Show("Please select a survey to import to.");
                return;
            }

            Util.SetProgressMessage("Validating Survey directory", 4);

            //make sure the survey folder is a valid survey folder for import
            ShapeFileWS = Util.ValidateSurveyFolder(txtImportDataPath.Text, ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                Util.SetProgressMessage("");
                return;
            }

            //check if the survey folder is empty or has no flight records
            if (Util.HasRecordsOf(m_NPS.LYR_TRACKLOG, ShapeFileWS, "") == false)
            {
                MessageBox.Show("No transect data found in the TrackLog shapefile.");
                Util.SetProgressMessage("");
                return;
            }

            //get the first valid survey id from teh tracklog featureclass
            SurveyFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ShapeFileWS, ref ErrorMessage);
            DataSurveyID = Util.GetFirstRecordValue(SurveyFC, "SurveyID", "");
            if (string.IsNullOrEmpty(DataSurveyID))
            {
                MessageBox.Show("Could not determine the Survey to import data "
                + "into after looking at the contents of the Survey folder.");
                Util.SetProgressMessage("");
                return;
            }

            //get the uniq transect ids flown by interogating the tracklog
            ImportedTransects = Util.GetUniqueValues(SurveyFC, "TransectID", "", ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                Util.SetProgressMessage("");
                return;
            }

            Util.SetProgressMessage("Importing Survey data");

            //go through each transect and import all data from the shape files for each transect
            foreach (string CurrentTransectID in ImportedTransects)
            {


                //check if there is flight data for this transect
                DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ref ErrorMessage);
                HasExistingData = Util.HasRecordsOf(DatabaseFC, "TransectID=" + CurrentTransectID
                    + " AND SurveyID=" + SurveyID);

                //if we have existing data but user selected to not to override any transects with
                //existing data, we won't ask - we'll just move on to the next transect
                if (HasExistingData == true && ThisFormOptions == FormOptions.NoAll) continue;

                //if there is existing data and the user has not specified that we should override
                //all transects without asking, we will need to ask the user about the current transect
                if (HasExistingData == true && ThisFormOptions != FormOptions.YesAll)
                {
                    //find out what the user wants to do about the existing data
                    using (HasTransectForm form = new HasTransectForm())
                    {
                        form.lblExistingTransectID.Text = CurrentTransectID;
                        form.ShowDialog();
                        ThisFormOptions = form.UserFormOptions;
                    }
                }

                //if the user says no to overriding the current transect's data or said no previously
                //we won't import this transects data
                if (ThisFormOptions == FormOptions.No || ThisFormOptions == FormOptions.NoAll)
                    continue;

                Util.SetProgressMessage("Importing survey data for TransectID " + CurrentTransectID, false);


                //delete all existing data
                //==========================
                if (HasExistingData)
                {
                    //delete existing tracklog data for current survey id and transect
                    DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ref InternalErr);
                    Util.DeleteFeatures(DatabaseFC, "TransectID=" + CurrentTransectID
                        + " AND SurveyID=" + SurveyID, ref InternalErr);

                    //delete existing sighting data for current survey id and transect
                    DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_ANIMALS, ref InternalErr);
                    Util.DeleteFeatures(DatabaseFC, "TransectID=" + CurrentTransectID
                        + " AND SurveyID=" + SurveyID, ref InternalErr);

                    //delete existing horizon data for current survey id and transect
                    DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_HORIZON, ref InternalErr);
                    Util.DeleteFeatures(DatabaseFC, "TransectID=" + CurrentTransectID
                        + " AND SurveyID=" + SurveyID, ref InternalErr);
                }

                //update transect
                SurveyFC = Util.GetFeatureClass(m_NPS.LYR_GENERATED_TRANSECTS, ShapeFileWS, ref InternalErr);
                Util.UpdateTransect(CurrentTransectID, Convert.ToString(SurveyID), SurveyFC, ref InternalErr);


                //import tracklog feature
                SurveyFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ShapeFileWS, ref InternalErr);
                DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_TRACKLOG, ref InternalErr);
                Util.CopyFeatures(SurveyFC, "TransectID=" + CurrentTransectID + " AND SurveyID="
                    + SurveyID, DatabaseFC, null, ref InternalErr);

                //import animal feature
                SurveyFC = Util.GetFeatureClass(m_NPS.LYR_ANIMALS, ShapeFileWS, ref InternalErr);
                DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_ANIMALS, ref InternalErr);
                Util.CopyFeatures(SurveyFC, "TransectID=" + CurrentTransectID + " AND SurveyID="
                    + SurveyID, DatabaseFC, null, ref InternalErr);

                //import horizon feature
                SurveyFC = Util.GetFeatureClass(m_NPS.LYR_HORIZON, ShapeFileWS, ref InternalErr);
                DatabaseFC = Util.GetFeatureClass(m_NPS.LYR_HORIZON, ref InternalErr);
                Util.CopyFeatures(SurveyFC, "TransectID=" + CurrentTransectID + " AND SurveyID="
                    + SurveyID, DatabaseFC, null, ref InternalErr);

            }

            Util.SetProgressMessage("Importing GPS point log");


            //import the gps points - this is not transect id specific, it is survey id specific
            SurveyFC = Util.GetFeatureClass(m_NPS.LYR_GPSPOINTLOG, ShapeFileWS, ref InternalErr);
            Util.CopyGPSPointLog(SurveyFC, ref InternalErr);

            Util.SetProgressMessage("");

            m_NPS.Document.ActiveView.Refresh();

            //let user know that everything is okay
            MessageBox.Show("Import was successfully completed");
        }

        private void btnBrowseExportPath_Click(object sender, EventArgs e)
        {
            bool Cancelled = false;
            string NewPath;

            NewPath = Util.OpenESRIDialog(txtExportDataPath.Text, ref Cancelled);
            if (Cancelled == false) txtExportDataPath.Text = NewPath;
        }

        private void btnBrowseImportPath_Click(object sender, EventArgs e)
        {
            bool Cancelled = false;
            string NewPath;

            NewPath = Util.OpenESRIDialog(txtImportDataPath.Text, ref Cancelled);
            if (Cancelled == false) txtImportDataPath.Text = NewPath;
        }

        private void cboSurveysList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int SurveyID = -1;

            if (m_SurveysList.Keys.Contains(cboSurveysList.Text))
                SurveyID = m_SurveysList[(string)cboSurveysList.Text];


            SetSurveyProgressLabels(SurveyID);


            if (SurveyID == -1)
                lblCurrentSurveyID.Text = "---";
            else
                lblCurrentSurveyID.Text = Convert.ToString(SurveyID);

            tabTransectTabs_SelectedIndexChanged(null, null);

        }

        private void tabTransectTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            int SurveyID;

            SurveyID = m_SurveysList[(string)cboSurveysList.Text];

            if (tabTransectTabs.SelectedIndex == 3)
            {
                if (SurveyID == -1)
                    lblRandomPointsTotal.Text = "---";
                else
                    lblRandomPointsTotal.Text = "(" + Convert.ToString(Util.FeatureCount(
                        m_NPS.LYR_RANDOMPOINTS, "SurveyID=" + SurveyID) + " random points have been generated)");
            }
        }

        private void btnBufferTool_Click(object sender, EventArgs e)
        {
            //show buffer tool form
            using (BufferToolForm form = new BufferToolForm())
            {
                form.ShowDialog();
            }
        }

        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            using (InfoForm form = new InfoForm())
            {
                form.txtInfo.Text = Util.RunSystemChecks();
                form.ShowDialog();
            }
        }


    }
}
