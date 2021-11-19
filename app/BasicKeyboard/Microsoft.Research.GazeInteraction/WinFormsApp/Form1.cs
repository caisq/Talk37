using System;
using System.Windows.Forms;
using WinFormsLib;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        private int _clickCount;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _clickCount++;
            label1.Text = $"Clicks = {_clickCount}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GazeInput.Start(this);
        }
    }
}
