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
    public partial class EditArcPadSettings : Form
    {
        ITable m_ArcPadSSettingsTable;
        NPSGlobal m_NPS;
        List<string> m_TextList;
        List<int> m_ValuesList;

        public EditArcPadSettings()
        {
            InitializeComponent();
            m_NPS = NPSGlobal.Instance;
        }

        private void EditArcPadSettings_Load(object sender, EventArgs e)
        {
            string ErrorMessage = "";


            m_ArcPadSSettingsTable = Util.GetDefaultValuesTable(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                this.Close();
                return;
            }

            LoadArcPadOptions();

            lstPickList.DataSource = GetArcPadFields();
            if (lstPickList.Items.Count > 0)
                lstPickList.SelectedIndex = 0;

        }

        public void LoadArcPadOptions()
        {
            string ErrorMessage = "";

            ckbEnableHorizonButtonSheep.Checked =
                Util.GetArcPadDefaultValue("EnableHorizon", ref ErrorMessage) == "Y" ? true : false;

        }

        public void SaveArcPadOptions()
        {
            string ErrorMessage = "";

            Util.SetArcPadDefaultValue("EnableHorizon",
                (ckbEnableHorizonButtonSheep.Checked ? "Y" : "N"), ref ErrorMessage);
        }

        private List<string> GetArcPadFields()
        {
            IFields FieldList;
            string CurrentFieldName;
            int FieldCount;
            List<string> FieldNamesList;

            FieldNamesList = new List<string>();
            FieldList = m_ArcPadSSettingsTable.Fields;
            FieldCount = FieldList.FieldCount;

            for (int FieldIndex = 0; FieldIndex < FieldCount; FieldIndex++)
            {
                CurrentFieldName = FieldList.get_Field(FieldIndex).Name;

                if (CurrentFieldName != "FIELD1" && CurrentFieldName != "OBJECTID"
                    && CurrentFieldName != "OID" && CurrentFieldName != "OBSTYPE"
                    && CurrentFieldName != "DISTANCE" && CurrentFieldName != "OBSDIR"
                    && CurrentFieldName != "DEFNAME" && CurrentFieldName != "DEFVALUE"
                    && CurrentFieldName != "INCREBYTEN" && CurrentFieldName != "WEATHER"
                    && FieldList.get_Field(FieldIndex).Type != esriFieldType.esriFieldTypeOID)
                {
                    FieldNamesList.Add(CurrentFieldName);
                }
            }

            return FieldNamesList;
        }

        private List<string> GetValuesForArcPadField(string FieldName, out List<int> ValuesList)
        {
            ICursor ThisCursor = null;
            IRow ThisRow = null;
            List<string> TextList;
            string CurrenValue;
            int FieldIndex, OIDIndex, CurrentOID;

            TextList = new List<string>();
            ValuesList = new List<int>();

            FieldIndex = m_ArcPadSSettingsTable.FindField(FieldName);
            if (FieldIndex == -1) return TextList;

            OIDIndex = m_ArcPadSSettingsTable.FindField(m_ArcPadSSettingsTable.OIDFieldName);
            if (OIDIndex == -1) return TextList;

            ThisCursor = m_ArcPadSSettingsTable.Search(null, false);

            while ((ThisRow = ThisCursor.NextRow()) != null)
            {
                CurrenValue = (string)Util.SafeConvert(ThisRow.get_Value(FieldIndex), typeof(string));
                if (string.IsNullOrEmpty(CurrenValue.Trim())) continue;

                CurrentOID = (int)Util.SafeConvert(ThisRow.get_Value(OIDIndex), typeof(int));

                ValuesList.Add(CurrentOID);
                TextList.Add(CurrenValue);
            }

            ThisCursor = null;

            return TextList;
        }

        private void lstPickList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string SelFieldName;


            SelFieldName = (string)lstPickList.SelectedValue;
            m_TextList = GetValuesForArcPadField(SelFieldName, out m_ValuesList);

            lstListOptions.DataSource = m_TextList;
            if (lstListOptions.Items.Count > 0)
                lstListOptions.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string SelFieldName, NewValue;
            IRowBuffer ThisBuffer;
            ICursor ThisCursor;
            int SelFieldIndex;

            NewValue = txtNewItem.Text;
            if (string.IsNullOrEmpty(NewValue.Trim()))
            {
                MessageBox.Show("Please enter valid text for the new item.");
                return;
            }

            SelFieldName = (string)lstPickList.SelectedValue;
            if (string.IsNullOrEmpty(SelFieldName))
            {
                MessageBox.Show("Please select a primary Picklist to add items to.");
                return;
            }

            try
            {
                SelFieldIndex = m_ArcPadSSettingsTable.FindField(SelFieldName);
                ThisCursor = m_ArcPadSSettingsTable.Insert(true);
                ThisBuffer = m_ArcPadSSettingsTable.CreateRowBuffer();

                ThisBuffer.set_Value(SelFieldIndex, NewValue);
                ThisCursor.InsertRow(ThisBuffer);

                ThisBuffer = null;
                ThisCursor = null;

                txtNewItem.Text = "";

                lstPickList_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while adding new list item. " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string SelValue, PicklistFieldName;
            IQueryFilter ThisQueryFilter;


            SelValue = (string)lstListOptions.SelectedValue;
            if (string.IsNullOrEmpty(SelValue))
            {
                MessageBox.Show("Please select a Picklist item to delete.");
                return;
            }

            PicklistFieldName = lstPickList.SelectedItem.ToString();

            ThisQueryFilter = new QueryFilterClass();
            ThisQueryFilter.WhereClause = PicklistFieldName + "='" + SelValue + "'";


            try
            {
                m_ArcPadSSettingsTable.DeleteSearchedRows(ThisQueryFilter);
                lstPickList_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while deleting Picklist item. " + ex.Message);
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            string HelpText = @"
            ** Instructions **
            The Edit ArcPad Defaults Tool is used to edit the defaultvalues.dbf file used by ArcPad. The
            The defaultvalues.dbf file stores the options for the picklists found on the ArcPad sighting forms.
            The defaultvalues.dbf file is packaged with the Survey folder generated after an export. Any
            changes made now to the defaultvalues.dbf file will be included in the next export.";

            using (HelpMessageForm form = new HelpMessageForm())
            {
                form.txtHelpMessage.Text = HelpText;
                form.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            SaveArcPadOptions();

            this.Close();
        }


    }
}
