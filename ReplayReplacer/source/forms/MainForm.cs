using ReplayReplacer.source.replayreplacer;

namespace ReplayReplacer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();


            Replay r = new Replay();
            r.FromFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\BlazBlue Centralfiction\\Save\\Replay\\replay09.dat");

            ReplayList rl = new ReplayList();
            rl.FromFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\BlazBlue Centralfiction\\Save\\replay_list.dat");
            label1.Text = r.ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
