using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;

namespace NPSTransectTool
{
    public partial class EditArcPadSettings : Form
    {
        ITable m_ArcPadSSettingsTable;
        List<string> m_TextList;
        List<int> m_ValuesList;

        public EditArcPadSettings()
        {
            InitializeComponent();
        }

        private void EditArcPadSettings_Load(object sender, EventArgs e)
        {
            string ErrorMessage = "";


            m_ArcPadSSettingsTable = Util.GetDefaultValuesTable(ref ErrorMessage);
            if (string.IsNullOrEmpty(ErrorMessage) == false)
            {
                MessageBox.Show(ErrorMessage);
                Close();
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
                Util.GetArcPadDefaultValue("EnableHorizon", ref ErrorMessage) == "Y";

        }

        public void SaveArcPadOptions()
        {
            string ErrorMessage = "";

            Util.SetArcPadDefaultValue("EnableHorizon",
                (ckbEnableHorizonButtonSheep.Checked ? "Y" : "N"), ref ErrorMessage);
        }

        private List<string> GetArcPadFields()
        {
            var fieldNamesList = new List<string>();
            IFields fieldList = m_ArcPadSSettingsTable.Fields;
            int fieldCount = fieldList.FieldCount;

            for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
            {
                string currentFieldName = fieldList.Field[fieldIndex].Name;

                if (currentFieldName != "FIELD1" && currentFieldName != "OBJECTID"
                    && currentFieldName != "OID" && currentFieldName != "OBSTYPE"
                    && currentFieldName != "DISTANCE" && currentFieldName != "OBSDIR"
                    && currentFieldName != "DEFNAME" && currentFieldName != "DEFVALUE"
                    && currentFieldName != "INCREBYTEN" && currentFieldName != "WEATHER"
                    && fieldList.Field[fieldIndex].Type != esriFieldType.esriFieldTypeOID)
                {
                    fieldNamesList.Add(currentFieldName);
                }
            }

            return fieldNamesList;
        }

        private List<string> GetValuesForArcPadField(string fieldName, out List<int> valuesList)
        {
            IRow thisRow;

            var textList = new List<string>();
            valuesList = new List<int>();

            int fieldIndex = m_ArcPadSSettingsTable.FindField(fieldName);
            if (fieldIndex == -1) return textList;

            int OIDIndex = m_ArcPadSSettingsTable.FindField(m_ArcPadSSettingsTable.OIDFieldName);
            if (OIDIndex == -1) return textList;

            ICursor thisCursor = m_ArcPadSSettingsTable.Search(null, false);

            while ((thisRow = thisCursor.NextRow()) != null)
            {
                var currentValue = (string)Util.SafeConvert(thisRow.Value[fieldIndex], typeof(string));
                if (string.IsNullOrEmpty(currentValue.Trim())) continue;

                var currentOid = (int)Util.SafeConvert(thisRow.Value[OIDIndex], typeof(int));

                valuesList.Add(currentOid);
                textList.Add(currentValue);
            }

            return textList;
        }

        private void lstPickList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selFieldName = (string)lstPickList.SelectedValue;
            m_TextList = GetValuesForArcPadField(selFieldName, out m_ValuesList);

            lstListOptions.DataSource = m_TextList;
            if (lstListOptions.Items.Count > 0)
                lstListOptions.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string NewValue = txtNewItem.Text;
            if (string.IsNullOrEmpty(NewValue.Trim()))
            {
                MessageBox.Show("Please enter valid text for the new item.");
                return;
            }

            var selFieldName = (string)lstPickList.SelectedValue;
            if (string.IsNullOrEmpty(selFieldName))
            {
                MessageBox.Show("Please select a primary Picklist to add items to.");
                return;
            }

            try
            {
                int selFieldIndex = m_ArcPadSSettingsTable.FindField(selFieldName);
                ICursor thisCursor = m_ArcPadSSettingsTable.Insert(true);
                IRowBuffer thisBuffer = m_ArcPadSSettingsTable.CreateRowBuffer();

                thisBuffer.Value[selFieldIndex] = NewValue;
                thisCursor.InsertRow(thisBuffer);

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
            var selValue = (string)lstListOptions.SelectedValue;
            if (string.IsNullOrEmpty(selValue))
            {
                MessageBox.Show("Please select a Picklist item to delete.");
                return;
            }

            string picklistFieldName = lstPickList.SelectedItem.ToString();

            IQueryFilter thisQueryFilter = new QueryFilterClass();
            thisQueryFilter.WhereClause = picklistFieldName + "='" + selValue + "'";


            try
            {
                m_ArcPadSSettingsTable.DeleteSearchedRows(thisQueryFilter);
                lstPickList_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while deleting Picklist item. " + ex.Message);
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            const string helpText = @"
            ** Instructions **
            The Edit ArcPad Defaults Tool is used to edit the defaultvalues.dbf file used by ArcPad. The
            The defaultvalues.dbf file stores the options for the picklists found on the ArcPad sighting forms.
            The defaultvalues.dbf file is packaged with the Survey folder generated after an export. Any
            changes made now to the defaultvalues.dbf file will be included in the next export.";

            using (var form = new HelpMessageForm())
            {
                form.txtHelpMessage.Text = helpText;
                form.ShowDialog();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            SaveArcPadOptions();
            Close();
        }


    }
}
