using System;
using System.Windows.Forms;

namespace VanillaStub
{
    public class IScreenLocker : Form
    {
        //Override form load to place center
        protected override void OnLoad(EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            base.OnLoad(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // IScreenLocker
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "IScreenLocker";
            this.Load += new System.EventHandler(this.IScreenLocker_Load);
            this.ResumeLayout(false);

        }

        private void IScreenLocker_Load(object sender, EventArgs e)
        {

        }
    }
}