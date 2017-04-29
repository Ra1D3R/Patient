using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Patient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_Load_1(object sender, EventArgs e)
        {
            string ShaderDir;
            ShaderDir = "./shaders/";
            DirectoryInfo DI = new DirectoryInfo(ShaderDir);
            //FileInfo[] files = DI.GetFiles();
            dataGridView1.DataSource = DI.GetFiles();
            dataGridView1.Rows.Add(DI.Name, DI.Parent);
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            //dataGridView1.SelectedRows.
        }
    }
}
