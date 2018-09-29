using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;

namespace WindowsFormsApplication1
{
    public partial class FC2Post : Form
    {
        private Program context = null;
        private Scraper scraper = null;

        public FC2Post(Program context, Scraper scraper)
        {
            this.InitializeComponent();
            this.context = context;
            this.scraper = scraper;
            this.setAccount2ComboBox3();
            this.setAccount2ComboBox4();
            this.setHistory2DataGridView1();
            this.setAccount2DataGridView2();
            this.setAccount2Label5(this.context.sID);
            this.setHeaderFooter();
            timer1.Tick += new EventHandler( IntervalExecution );
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�t�H�[�����[�h
        //_/
        private void FC2Post_Load(object sender, EventArgs e)
        {
            //���ԊԊu���g�R�h�i���ԁj�I���ς�
            this.comboBox2.SelectedIndex = 0;

            //����g�L�h��I���ς�
            this.comboBox1.SelectedIndex = 0;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�C���^�[�o������
        //_/
        private void IntervalExecution(object sender, EventArgs e)
        {
            var word = textBox1.Text.Trim();
            var movie = comboBox1.Text.Trim();

            //�X�g�b�v�{�^����L����
            this.button2.Enabled = false;
            this.label12.Text = "�L�����������ł�";
            if (!this.scraper.ScrapingItem(word))
            {
                this.pushStopPosting();
                timer1.Stop();
                MessageBox.Show("�L�[���[�h�ɑΉ�����j���[�X�L��������܂���");
                return;
            }
            if (movie.Equals(@"�L"))
            {
                this.label12.Text = "������������ł�";
                this.scraper.CreateEmbedCode(word);
            }
            this.label12.Text = "�u���O�ɓ��e���܂�";
            this.PostBlog();
            this.setHistory();

            //�X�g�b�v�{�^���L����
            this.button2.Enabled = true;
        }

        public void setTabIndex(int iArg)
        {
            this.tabControl1.SelectTab(iArg);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���݃��O�C�����̃��[�U
        //_/
        private void setAccount2Label5( string accountArg )
        {
            string query = string.Format("ID='{0}'", accountArg);
            var rows = this.context.dtAccount.Select(query);
            foreach ( var row in rows ){
                this.label5.Text = row["NN"].ToString();
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���[�U�E�f�[�^�O���b�h
        //_/
        private void setHistory2DataGridView1()
        {
            this.dataGridView1.DataSource = this.context.dtHistory;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���[�U�E�f�[�^�O���b�h
        //_/
        private void setAccount2DataGridView2()
        {
            this.dataGridView2.DataSource = this.context.dtAccount;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�ؑփ{�^����������
        //_/
        private void button6_Click(object sender, EventArgs e)
        {
            string sID = this.comboBox4.SelectedValue.ToString();
            this.context.sID = sID;
            setAccount2Label5(sID);
            setHeaderFooter();
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�ؑփR���{
        //_/
        private void setAccount2ComboBox4()
        {
            this.context.dtUser = this.context.dtAccount.Copy();
            this.comboBox4.DisplayMember = "NN";
            this.comboBox4.ValueMember = "ID";
            this.comboBox4.DataSource = this.context.dtUser;

            //���Y���[�U��I���ς�
            this.comboBox4.SelectedValue = this.context.sID;
            this.setAccount2Label5(this.context.sID);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�폜�R���{
        //_/
        private void setAccount2ComboBox3()
        {
            this.context.dtDelete = this.context.dtAccount.Copy();
            DataRow dr = this.context.dtDelete.NewRow();
            dr["ID"] = " ";
            this.context.dtDelete.Rows.InsertAt(dr, 0);
            this.comboBox3.DisplayMember = "NN";
            this.comboBox3.ValueMember = "ID";
            this.comboBox3.DataSource = this.context.dtDelete;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���e�X�^�[�g�{�^����������
        //_/
        private void button1_Click(object sender, EventArgs e)
        {
            if ("".Equals(context.sID))
            {
                MessageBox.Show("�u���OID��I�����Ă�������");
                return;
            }

            this.pushStartPosting();
            var word = textBox1.Text.Trim();
            var interval = comboBox2.Text.Trim();
            var movie = comboBox1.Text.Trim();
            if (word == null || "".Equals(word))
            {
                MessageBox.Show("���������͂��Ă�������");
                this.pushStopPosting();
                return;
            }
            this.context.KeyWord = word;
            if (interval == null || "".Equals(interval))
            {
                MessageBox.Show("���ԊԊu��I�����Ă�������");
                this.pushStopPosting();
                return;
            }
            timer1.Interval = int.Parse(interval) * 1000 * 60 *60;
            if (movie == null || "".Equals(movie))
            {
                MessageBox.Show("����̗L����I�����Ă�������");
                this.pushStopPosting();
                return;
            }

            //�X�g�b�v�{�^����L����
            this.button2.Enabled = false;

            this.label12.Text = "�L�����������ł�";
            if (!this.scraper.ScrapingItem(word))
            {
                this.pushStopPosting();
                MessageBox.Show("�L�[���[�h�ɑΉ�����j���[�X�L��������܂���");
                return;
            }
            if (movie.Equals(@"�L"))
            {
                this.label12.Text = "������������ł�";
                this.scraper.CreateEmbedCode(word);
            }
            if (!this.IsHeaderAndFooter())
            {
                MessageBox.Show("�w�b�_�ƃt�b�^���ŏ����e�P���͂��Ă�������");
                this.pushStopPosting();
                return;
            }
            this.label12.Text = "�u���O�ɓ��e���܂�";
            this.PostBlog();
            this.setHistory();
            timer1.Start();

            //�X�g�b�v�{�^���L����
            this.button2.Enabled = true;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�w�b�_�A�t�b�^���͊m�F����
        //_/
        public bool IsHeaderAndFooter()
        {
            this.context.ListHeader.Clear();
            bool rtn = false;
            foreach (KeyValuePair<int, string> pair in context.dicHeader)
            {
                if (!pair.Value.Equals("") && !pair.Value.Equals(null))
                {
                    this.context.ListHeader.Add(pair.Key);
                    rtn = true;
                }
            }
            if (rtn == false) return rtn;
            rtn = false;
            foreach (KeyValuePair<int, string> pair in context.dicFooter)
            {
                if (!pair.Value.Equals("") && !pair.Value.Equals(null))
                {
                    this.context.ListFooter.Add(pair.Key);
                    rtn = true;
                }
            }
            return rtn;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���e�X�^�[�g�{�^������������
        //_/
        private void pushStartPosting()
        {
            this.button1.Enabled = false;
            this.button2.Enabled = true;
            this.textBox1.Enabled = false;
            this.comboBox1.Enabled = false;
            this.comboBox2.Enabled = false;
            this.label11.ForeColor = Color.Blue;
            this.label11.Text = "���e�p����";
            this.label13.Text = "���e�p����";
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���e�X�g�b�v�{�^������������
        //_/
        private void button2_Click(object sender, EventArgs e)
        {
            this.pushStopPosting();
            timer1.Stop();
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/���e�X�g�b�v�{�^������������
        //_/
        private void pushStopPosting()
        {
            this.button2.Enabled = false;
            this.button1.Enabled = true;
            this.textBox1.Enabled = true;
            this.comboBox1.Enabled = true;
            this.comboBox2.Enabled = true;
            this.label11.ForeColor = Color.Red;
            this.label11.Text = "���e��~��";
            this.label13.Text = "���e��~��";
        }
            
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�u���O���e����
        //_/
        private void PostBlog()
        {
            //JavaScript�̃G���[�_�C�A���O��\�����Ȃ�
            webBrowser1.ScriptErrorsSuppressed = true;

            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/���O�C����ʑJ�ڌ�
            //_/Web�y�[�W�̕\������������܂őҋ@����
            //_/
            this.label12.Text = "���O�C�����ł�";
            string strUrl = "http://fc2.com/login.php?ref=blog";
            this.webBrowser1.Navigate(strUrl);

            //�ҋ@
            do Application.DoEvents();
            while (this.webBrowser1.IsBusy == true ||
                   this.webBrowser1.ReadyState != WebBrowserReadyState.Complete);

            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/�A�J�E���g������
            //_/Web�y�[�W�̕\������������܂őҋ@����
            //_/
            HtmlElement he = null;
            try
            {
                he = this.webBrowser1.Document.GetElementById("id");
                he.InnerText = this.context.sID;

                string pass = null;
                string query = "id='{0}'";
                var rows = this.context.dtAccount.Select(string.Format(query, this.context.sID));
                if (rows == null || rows.Count().Equals(0))
                {
                    MessageBox.Show("�p�X���[�h������܂���");
                    return;
                }
                foreach (var row in rows)
                {
                    pass = row["PW"].ToString();
                }
                he = this.webBrowser1.Document.GetElementById("pass");
                he.InnerText = pass;

                this.webBrowser1.Document.Forms[0].InvokeMember("submit");
            }
            catch (NullReferenceException ignore)
            {
                return;//NOP
            }

            //�ҋ@
            do Application.DoEvents();
            while (this.webBrowser1.IsBusy == true ||
                   this.webBrowser1.ReadyState != WebBrowserReadyState.Complete);

            //���O�C����AURI���擾
            string url = this.webBrowser1.Document.Url.AbsoluteUri;

            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/�V�K���e��ʑJ�ڌ�
            //_/Web�y�[�W�̕\������������܂őҋ@����
            //_/
            this.label12.Text = "�u���O�ɓ��e���ł�";
            strUrl = url + "?mode=editor&process=new";
            this.webBrowser1.Navigate(strUrl);

            //�ҋ@
            do Application.DoEvents();
            while (this.webBrowser1.IsBusy == true ||
                   this.webBrowser1.ReadyState != WebBrowserReadyState.Complete);

            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/�L���ۑ�������ʑJ�ڌ�
            //_/Web�y�[�W�̕\������������܂őҋ@����
            //_/
            //�h�L�������g�R���v���[�g�C�x���g�ǉ�
            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            Random rnd = new Random(Environment.TickCount);
            try
            {
                he = this.webBrowser1.Document.GetElementById("entry_title");
                he.InnerText = this.scraper.GetPostingData("NEWSTITLE");
                he = this.webBrowser1.Document.GetElementById("body");
                he.InnerText = this.context.dicHeader[this.context.ListHeader[rnd.Next(0, this.context.ListHeader.Count)]]
                             + "\n"
                             + this.scraper.GetPostingData("CONTENTS")
                             + "\n�m�z�M���n" + this.scraper.GetPostingData("URL") + "\n"
                             + "�����̋L���̒��쌠�͔z�M���ɋA�����܂�\n"
                             + this.scraper.GetPostingData("EMBEDCODE")
                             + "\n"
                             + this.context.dicFooter[this.context.ListFooter[rnd.Next(0, this.context.ListFooter.Count)]];
                this.webBrowser1.Document.Forms[0].InvokeMember("submit");
            }
            catch (NullReferenceException ignore)
            {
                return;//NOP
            }

            //�ҋ@
            do Application.DoEvents();
            while (this.webBrowser1.IsBusy == true ||
                   this.webBrowser1.ReadyState != WebBrowserReadyState.Complete);
            //�h�L�������g�R���v���[�g�C�x���g�폜
            this.webBrowser1.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);

            //���e�u���O��URL���擾�ł��Ȃ������ꍇ
            string blogurl = System.Text.RegularExpressions.Regex.Replace(url, "http://", "http://" + this.context.sID + ".");
            blogurl = System.Text.RegularExpressions.Regex.Replace(blogurl, "/control.php$", "");

            if ("#".Equals(this.scraper.GetPostingData("BLOGURL"))
             || "".Equals(this.scraper.GetPostingData("BLOGURL"))
             || this.scraper.GetPostingData("BLOGURL")==null)
            {
                this.scraper.SetBlogUrl(url);
            }

            //�u���O���e�����̕\��
            this.label12.Text = "�u���O�̓��e���������܂���";
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�h�L�������g�R���v���[�g�C�x���g
        //_/
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.scraper.GetBlogUrl(webBrowser1.DocumentText);
        }

        private void setHistory()
        {
            string key = this.scraper.GetPostingData("KEY");
            string watch = this.scraper.GetPostingData("WATCH");
            string title = this.scraper.GetPostingData("NEWSTITLE");
            string blogurl = this.scraper.GetPostingData("BLOGURL");
            DateTime dtNow = DateTime.Now;
            this.context.dtHistory.Rows.Add(key, dtNow, title, watch, blogurl);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�w�b�_�A�t�b�^���W�I�{�^���C�x���g
        //_/
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                this.textBox2.Text = this.context.dicHeader[0];
            else
                this.context.dicHeader[0] = this.textBox2.Text;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                this.textBox2.Text = this.context.dicHeader[1];
            else
                this.context.dicHeader[1] = this.textBox2.Text;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                this.textBox2.Text = this.context.dicHeader[2];
            else
                this.context.dicHeader[2] = this.textBox2.Text;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                this.textBox3.Text = this.context.dicFooter[0];
            else
                this.context.dicFooter[0] = this.textBox3.Text;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
                this.textBox3.Text = this.context.dicFooter[1];
            else
                this.context.dicFooter[1] = this.textBox3.Text;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
                this.textBox3.Text = this.context.dicFooter[2];
            else
                this.context.dicFooter[2] = this.textBox3.Text;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�R�����g�ύX������
        //_/
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                this.context.dicHeader[0] = this.textBox2.Text;
            else if (radioButton2.Checked)
                this.context.dicHeader[1] = this.textBox2.Text;
            else if (radioButton3.Checked)
                this.context.dicHeader[2] = this.textBox2.Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                this.context.dicFooter[0] = this.textBox3.Text;
            else if (radioButton5.Checked)
                this.context.dicFooter[1] = this.textBox3.Text;
            else if (radioButton6.Checked)
                this.context.dicFooter[2] = this.textBox3.Text;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�����L�[���[�h
        //_/
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.context.KeyWord = this.textBox1.Text;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/ID�ǉ��{�^����������
        //_/
        private void button4_Click(object sender, EventArgs e)
        {
            string nickname = textBox5.Text.Trim();
            string fc2id = textBox6.Text.Trim();
            string password = textBox7.Text.Trim();

            if ( nickname == null || "".Equals(nickname))
            {
                MessageBox.Show("�j�b�N�l�[���������͂ł�");
                return;
            }
            else if ( fc2id == null || "".Equals(fc2id))
            {
                MessageBox.Show("FC2ID�������͂ł�");
                return;
            }
            else if ( password == null || "".Equals(password))
            {
                MessageBox.Show("�p�X���[�h�������͂ł�");
                return;
            }

            //�e�L�X�g�{�b�N�X�A�N���A
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();

            //�A�J�E���g�ǉ����\�b�h�ďo
            this.addAccount(fc2id, password, nickname);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/ID�폜�{�^����������
        //_/
        private void button5_Click(object sender, EventArgs e)
        {
            var text = this.comboBox3.SelectedValue.ToString().Trim();
            if (text == null || "".Equals(text))
            {
                MessageBox.Show("�u���OID��I�����Ă�������");
                return;
            }
            string selectedID = comboBox3.SelectedValue.ToString().Trim();
            if (this.context.sID.CompareTo(selectedID).Equals(0))
            {
                MessageBox.Show("���O�C�����̃u���OID�͍폜�ł��܂���");
                return;
            }
            MessageBox.Show("�u���OID���폜���܂�����낵���ł���");
            this.delAccount(selectedID);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�A�J�E���g�ǉ�
        //_/
        private void addAccount(string fc2id, string password, string nickname)
        {
            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/�p�X�ƃj�b�N�l�[�������W�X�g���ɓo�^
            //_/
            string sRegKey = Program.REGKEY_FPID + @"\" + fc2id;
            RegistryKey rKey = Registry.CurrentUser.CreateSubKey(sRegKey);
            rKey.SetValue(Program.FPPD, password, RegistryValueKind.String);
            rKey.SetValue(Program.FPNN, nickname, RegistryValueKind.String);

            //�f�[�^�E�O���b�h�r���[�Ƀo�C���h���� DataTable �ɍs�ǉ�
            this.context.dtAccount.Rows.Add(fc2id, password, nickname);

            //�ؑփR���{�Ƀo�C���h���� DataTable �v�f�ǉ�
            this.context.dtUser.Rows.Add(fc2id, password, nickname);
    
            //�폜�R���{�Ƀo�C���h���� DataTable �v�f�ǉ�
            this.context.dtDelete.Rows.Add(fc2id, password, nickname);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�A�J�E���g�폜
        //_/
        private void delAccount(string fc2id)
        {
            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/FC2ID�����W�X�g������폜
            //_/
            string sRegKey = Program.REGKEY_FPID + @"\" + fc2id;
            Registry.CurrentUser.DeleteSubKeyTree(sRegKey);

            //�f�[�^�E�O���b�h�r���[�Ƀo�C���h���� DataTable ����s�폜
            if (this.context.dtAccount.Rows.Contains(fc2id) == true)
            {
                DataRow dr = this.context.dtAccount.Rows.Find(fc2id);
                this.context.dtAccount.Rows.Remove(dr);
            }
            //�ؑփR���{�Ƀo�C���h���� DataTable ����v�f�폜
            if (this.context.dtUser.Rows.Contains(fc2id) == true)
            {
                DataRow dr = this.context.dtUser.Rows.Find(fc2id);
                this.context.dtUser.Rows.Remove(dr);
            }
            //�폜�R���{�Ƀo�C���h���� DataTable ����v�f�폜
            if (this.context.dtDelete.Rows.Contains(fc2id) == true)
            {
                DataRow dr = this.context.dtDelete.Rows.Find(fc2id);
                this.context.dtDelete.Rows.Remove(dr);
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�����L�[�A�w�b�_�A�t�b�^�擾����
        //_/
        private void setHeaderFooter()
        {
            this.context.dicHeader.Clear();
            this.context.dicFooter.Clear();
            this.textBox1.Text = "";
            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/���W�X�g������擾���A�����ɓo�^
            //_/
            try
            {
                //���W�X�g���L�[���J��
                string sHeaderRegKey = Program.REGKEY_FPID + @"\" + this.context.sID + @"\" + Program.FPHF + @"\" + Program.FPHD;
                string sFooterRegKey = Program.REGKEY_FPID + @"\" + this.context.sID + @"\" + Program.FPHF + @"\" + Program.FPFT;
                string sKeyWordRegKey = Program.REGKEY_FPID + @"\" + this.context.sID;
                RegistryKey rKey = Registry.CurrentUser.CreateSubKey(sHeaderRegKey);
                this.context.dicHeader[0] = rKey.GetValue("1") as string == null ? "" : rKey.GetValue("1") as string;
                this.context.dicHeader[1] = rKey.GetValue("2") as string == null ? "" : rKey.GetValue("2") as string;
                this.context.dicHeader[2] = rKey.GetValue("3") as string == null ? "" : rKey.GetValue("3") as string;
                rKey.Close();
                rKey = Registry.CurrentUser.CreateSubKey(sFooterRegKey);
                this.context.dicFooter[0] = rKey.GetValue("1") as string == null ? "" : rKey.GetValue("1") as string;
                this.context.dicFooter[1] = rKey.GetValue("2") as string == null ? "" : rKey.GetValue("2") as string;
                this.context.dicFooter[2] = rKey.GetValue("3") as string == null ? "" : rKey.GetValue("3") as string;
                rKey.Close();
                rKey = Registry.CurrentUser.CreateSubKey(sKeyWordRegKey);
                this.context.KeyWord = rKey.GetValue(Program.KYWD) as String == null ? "" : rKey.GetValue(Program.KYWD) as string;
                rKey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (radioButton1.Checked)
                this.textBox2.Text = this.context.dicHeader[0];
            else if (radioButton2.Checked)
                this.textBox2.Text = this.context.dicHeader[1];
            else if (radioButton3.Checked)
                this.textBox2.Text = this.context.dicHeader[2];
            if (radioButton4.Checked)
                this.textBox3.Text = this.context.dicFooter[0];
            else if (radioButton5.Checked)
                this.textBox3.Text = this.context.dicFooter[1];
            else if (radioButton6.Checked)
                this.textBox3.Text = this.context.dicFooter[2];

            //�����L�[���[�h��ݒ�
            this.textBox1.Text = this.context.KeyWord;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�����L�[�A�w�b�_�A�t�b�^�ۑ�����
        //_/
        private void button3_Click(object sender, EventArgs e)
        {
            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/���W�X�g���ɓo�^
            //_/
            //�w�b�_�A�t�b�^��ۑ�
            string sHeaderRegKey = Program.REGKEY_FPID + @"\" + this.context.sID + @"\" + Program.FPHF + @"\" + Program.FPHD;
            string sFooterRegKey = Program.REGKEY_FPID + @"\" + this.context.sID + @"\" + Program.FPHF + @"\" + Program.FPFT;
            string sKeyWordRegKey = Program.REGKEY_FPID + @"\" + this.context.sID;
            RegistryKey rKey = Registry.CurrentUser.CreateSubKey(sHeaderRegKey);
            rKey.SetValue("1", this.context.dicHeader[0], RegistryValueKind.String);
            rKey.SetValue("2", this.context.dicHeader[1], RegistryValueKind.String);
            rKey.SetValue("3", this.context.dicHeader[2], RegistryValueKind.String);
            rKey.Close();
            rKey = Registry.CurrentUser.CreateSubKey(sFooterRegKey);
            rKey.SetValue("1", this.context.dicFooter[0], RegistryValueKind.String);
            rKey.SetValue("2", this.context.dicFooter[1], RegistryValueKind.String);
            rKey.SetValue("3", this.context.dicFooter[2], RegistryValueKind.String);
            rKey.Close();
            //�����L�[��ۑ�
            rKey = Registry.CurrentUser.CreateSubKey(sKeyWordRegKey);
            rKey.SetValue(Program.KYWD, this.context.KeyWord, RegistryValueKind.String);
            rKey.Close();
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�f�[�^�O���b�h�r���[�P�_�u���N���b�N
        //_/
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string url = (string)this.context.dtHistory.Rows[e.RowIndex]["BLOGURL"];
            Process extProcess = new Process();
            Process.Start(url);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/�f�[�^�O���b�h�r���[�Q�_�u���N���b�N
        //_/
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string sID = (string)this.context.dtAccount.Rows[e.RowIndex]["ID"];
            this.context.sID = sID;
            setAccount2Label5(sID);
            setHeaderFooter();
        }
    }
}
