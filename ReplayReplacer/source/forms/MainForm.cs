using ReplayReplacer.source.replayreplacer;

namespace ReplayReplacer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ReplayList rl = new ReplayList("C:\\Program Files (x86)\\Steam\\steamapps\\common\\BlazBlue Centralfiction\\Save\\replay_list.dat");
            label1.Text = rl.ToString();

            var p1NameLabels = rl.GetP1Names();
            for (int i = 0; i < p1NameLabels.Count; i++)
            {
                flowLayoutPanel1.Controls.Add(p1NameLabels[i]);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
