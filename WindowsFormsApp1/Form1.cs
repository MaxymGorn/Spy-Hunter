using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Visible = false;
        }


        public async Task
StartMessage(int delay, string title, string text, ToolTipIcon toolTipIcon)
        {
            await Task.Run(() => {notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(delay, title, text, toolTipIcon);
            notifyIcon1.Visible = false; });

        }

    }
}
