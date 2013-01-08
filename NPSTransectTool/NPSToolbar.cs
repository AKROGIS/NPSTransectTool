using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace NPSTransectTool
{
    /// <summary>
    /// Summary description for NPSToolbar.
    /// </summary>
    [Guid("93289446-D2A6-4b43-8A02-2A04ECD5021E")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("NPSTransectTool.NPSToolbar")]
    public sealed class NPSToolbar : BaseToolbar
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommandBars.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommandBars.Unregister(regKey);
        }

        #endregion
        #endregion

        public NPSToolbar()
        {
            
            //
            // TODO: Define your toolbar here by adding items
            //
            AddItem("NPSTransectTool.TitleDisplayCommand");
            BeginGroup(); //Separator
            AddItem("NPSTransectTool.StraightLineCommand");
            BeginGroup(); //Separator
            AddItem("NPSTransectTool.BufferCommand");
            BeginGroup(); //Separator
            AddItem("NPSTransectTool.TransectToolCommand");
        }


        public override string Caption
        {
            get
            {
                //TODO: Replace bar caption
                return "NPS Toolbar v"+ Util.GetAssemblyVersion();
            }
        }
        public override string Name
        {
            get
            {
                //TODO: Replace bar ID
                return "NPSToolbar_2_0";
            }
        }
    }
}