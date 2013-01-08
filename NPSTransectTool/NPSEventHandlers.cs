using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;

namespace NPSTransectTool
{
    static class NPSEventHandlers
    {

        /// <summary>
        /// handler for Editor feature creation event 
        /// </summary>
        public static void OnCreateFeature(ESRI.ArcGIS.Geodatabase.IObject pObject)
        {
            IFeature ThisFeature = null;
            NPSGlobal NPS;
            ILayer CurrentLayer;
            int SurveyIDIndex, CommentsIndex, ParkIndex, SurveyNameIndex, NextSurveyID;
            string ErrorMessage = "", SurveyName;

            NPS = NPSGlobal.Instance;

            //make sure the tools are active
            if (NPS.IsInitialized == false) return;

            //make sure object is IFeature
            if (!(pObject is IFeature))
                return;

            //get created feature
            ThisFeature = (IFeature)pObject;


            //make sure we have a reference to the local editor
            if (NPS.Editor == null)
                return;

            //get the current layer being edited
            CurrentLayer = ((IEditLayers)NPS.Editor).CurrentLayer;

            //not really necessary to check
            if ((CurrentLayer is IFeatureLayer) == false || (CurrentLayer as IFeatureLayer).FeatureClass == null) return;

            //we only care when new survey boundaries are created and created by the user, not in code
            if (((IDataset)(CurrentLayer as IFeatureLayer).FeatureClass).Name != NPS.LYR_SURVEY_BOUNDARY
                || NPS.ProgramaticFeatureEdit == true)
                return;

            //get indexes of fields we need to set for the new survey boundary feature
            SurveyIDIndex = ThisFeature.Fields.FindField("SurveyID");
            SurveyNameIndex = ThisFeature.Fields.FindField("SurveyName");
            ParkIndex = ThisFeature.Fields.FindField("Park");
            CommentsIndex = ThisFeature.Fields.FindField("Comments");

            if (SurveyIDIndex == -1 || SurveyNameIndex == -1 || ParkIndex == -1 || CommentsIndex == -1)
                return;

            //try to get the next valid survey id
            NextSurveyID = Util.NextSurveyID(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
                return;

            //get the user to enter info for the new survey boundary and save the
            //form data to the new survey
            using (NewSurveyBoundaryForm form = new NewSurveyBoundaryForm())
            {
                form.ckbDontAskAgain.Visible = false;
                form.txtSurveyID.Text = Convert.ToString(NextSurveyID);
                form.ShowDialog();

                //if we don't have a survey name, the survey id will stand in for the
                //survey name
                SurveyName = string.IsNullOrEmpty(form.txtSurveyName.Text.Trim()) ?
                    Convert.ToString(NextSurveyID) : form.txtSurveyName.Text;

                ThisFeature.set_Value(SurveyIDIndex, NextSurveyID);
                ThisFeature.set_Value(SurveyNameIndex, SurveyName);
                ThisFeature.set_Value(ParkIndex, form.txtPark.Text);
                ThisFeature.set_Value(CommentsIndex, form.txtComments.Text);
                ThisFeature.Store();
            }
        }

        /// <summary>
        /// handler for document when document is closing
        /// </summary>
        public static void CloseMxDocument()
        {
            NPSGlobal.Instance.UnInit();
        }

        /// <summary>
        /// handler for document when document is opening
        /// </summary>
        public static void OpenMxDocument()
        {
            NPSGlobal.Instance.Init();
        }
    }
}
