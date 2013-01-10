using System;
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
        public static void OnCreateFeature(IObject pObject)
        {
            string errorMessage = "";

            NPSGlobal nps = NPSGlobal.Instance;

            //make sure the tools are active
            if (nps.IsInitialized == false) return;

            //make sure object is IFeature
            if (!(pObject is IFeature))
                return;

            //get created feature
            var thisFeature = (IFeature)pObject;


            //make sure we have a reference to the local editor
            if (nps.Editor == null)
                return;

            //get the current layer being edited
            ILayer currentLayer = ((IEditLayers)nps.Editor).CurrentLayer;

            //not really necessary to check
            if (currentLayer == null || (currentLayer as IFeatureLayer).FeatureClass == null) return;

            //we only care when new survey boundaries are created and created by the user, not in code
            if (((IDataset)(currentLayer as IFeatureLayer).FeatureClass).Name != nps.LYR_SURVEY_BOUNDARY
                || nps.ProgramaticFeatureEdit)
                return;

            //get indexes of fields we need to set for the new survey boundary feature
            int surveyIdIndex = thisFeature.Fields.FindField("SurveyID");
            int surveyNameIndex = thisFeature.Fields.FindField("SurveyName");
            int parkIndex = thisFeature.Fields.FindField("Park");
            int commentsIndex = thisFeature.Fields.FindField("Comments");

            if (surveyIdIndex == -1 || surveyNameIndex == -1 || parkIndex == -1 || commentsIndex == -1)
                return;

            //try to get the next valid survey id
            int nextSurveyId = Util.NextSurveyID(ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage) == false)
                return;

            //get the user to enter info for the new survey boundary and save the
            //form data to the new survey
            using (var form = new NewSurveyBoundaryForm())
            {
                form.ckbDontAskAgain.Visible = false;
                form.txtSurveyID.Text = Convert.ToString(nextSurveyId);
                form.ShowDialog();

                //if we don't have a survey name, the survey id will stand in for the
                //survey name
                string surveyName = string.IsNullOrEmpty(form.txtSurveyName.Text.Trim()) ?
                                        Convert.ToString(nextSurveyId) : form.txtSurveyName.Text;

                thisFeature.Value[surveyIdIndex] = nextSurveyId;
                thisFeature.Value[surveyNameIndex] = surveyName;
                thisFeature.Value[parkIndex] = form.txtPark.Text;
                thisFeature.Value[commentsIndex] =  form.txtComments.Text;
                thisFeature.Store();
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
