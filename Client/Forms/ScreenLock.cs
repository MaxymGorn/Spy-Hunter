using System.Windows.Forms;

namespace VanillaStub.Forms
{
    public partial class ScreenLock : IScreenLocker
    {
        public ScreenLock()
        {
            InitializeComponent();
        }
        private void OnClose(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        private void ScreenLock_Load(object sender, System.EventArgs e)
        {

        }
    }
}