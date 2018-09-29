using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Login : Form
    {
        private Program context = null;
        public Login(Program context)
        {
            InitializeComponent();
            this.context = context;
            this.setAccount2ComboBox1();
        }

        private void setAccount2ComboBox1()
        {
            DataTable dt = context.dtAccount.Copy();
            DataRow dr = dt.NewRow();
            dr["ID"] = " ";
            dt.Rows.InsertAt(dr, 0);
            this.comboBox1.DisplayMember = "NN";
            this.comboBox1.ValueMember = "ID";
            this.comboBox1.DataSource = dt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = "";
            if (this.comboBox1.SelectedValue != null)
            {
                text = this.comboBox1.SelectedValue.ToString().Trim();
            }
            context.sID = text;
            context.CLOSE_REASON = "";
            this.Close();
            this.Dispose();
        }
    }
}
