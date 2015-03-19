using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CleanWindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void Message_NOTexture()
        {
            MessageBox.Show("" + Properties.message.Default.Message_NOTexture);
        }

        public void Message_NOFile()
        {
            MessageBox.Show("" + Properties.message.Default.Message_NOFile);
        }
    }
}
