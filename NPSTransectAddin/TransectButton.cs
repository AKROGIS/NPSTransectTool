using System;
using System.Windows.Forms;
using NPSTransectTool;

namespace NPSTransectAddin
{
    public class TransectButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public TransectButton()
        {
            try
            {
                var nps = NPSGlobal.Instance;
                nps.InitArcMapBindings(ArcMap.Application);
                nps.Init();
                Enabled = nps.IsInitialized;
                if (!String.IsNullOrEmpty(nps.InitErrorMessage))
                    MessageBox.Show(nps.InitErrorMessage, "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetType() + " encountered a problem.\n\n" + ex,
                                "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnClick()
        {
            if (Enabled)
            {
                try
                {
                    using (var form = new TransectToolForm())
                    {
                        form.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(GetType() + " encountered a problem.\n\n" + ex,
                                    "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnUpdate()
        {
            Enabled = NPSGlobal.Instance.IsInitialized;
        }

    }
}
