namespace FullScreenWinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            https://social.msdn.microsoft.com/Forums/en-US/0ae1b84f-f114-43c1-a560-a1a4588c1eca/picturebox-fullscreen-display?forum=winforms
            InitializeComponent();
        }

    
        private void button1_Click(object sender, EventArgs e)
        {
            PreviewForm form = new PreviewForm();
            //form.SetImage(@"D:\temp\ai\399464388_237794612649174_8038027137018639542_n.jpg");
            form.SetDirectory(@"D:\temp\ai\rated");
            form.ShowDialog();
        }
    }
}
