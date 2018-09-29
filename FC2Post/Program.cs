using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsFormsApplication1
{
    public class Program
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/クラス変数
        //_/
        public static string REGKEY_FPID= @"SOFTWARE\Tools\fpst\fpid";
        public static string FPPD       = @"fppd";
        public static string FPNN       = @"fpnn";
        public static string FPHF       = @"fphf";
        public static string FPHD       = @"fphd";
        public static string FPFT       = @"fpft";
        public static string KYWD       = @"kywd";

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/インスタンス変数
        //_/
        public string CLOSE_REASON      = "SOME REASON DONT KNOW";
        public Dictionary<int, string> dicHeader
                                        = new Dictionary<int, string>();
        public Dictionary<int, string> dicFooter
                                        = new Dictionary<int, string>();
        public string KeyWord           = null;
        public string sID               = null;
        public DataTable dtAccount      = null;
        public DataTable dtUser         = null;
        public List<int> ListHeader     = new List<int>();
        public List<int> ListFooter     = new List<int>();
        public DataTable dtDelete       = null;
        public DataTable dtHistory      = null;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Program self = new Program();
            self.doOperation();
        }

        private void doOperation()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            int tabIndex = 0;
            this.dtAccount = getAccount();
            this.dtHistory = getHistory();
            if (dtAccount.Rows.Count == 0)
            {
                tabIndex = 1;
                this.CLOSE_REASON = "";
                this.sID = "";
            }
            else
            {
                Application.Run(new Login(this));
            }
            if ("".Equals(this.CLOSE_REASON))
            {
                Scraper scraper = new Scraper(this);
                FC2Post post = new FC2Post(this, scraper);
                post.setTabIndex(tabIndex);
                Application.Run(post);
            }
        }

        private DataTable getHistory()
        {
            DataTable dtHistory = new DataTable();
            DataColumn[] aryDC = new DataColumn[1];
            aryDC[0] = dtHistory.Columns.Add("KEY", typeof(string));
            dtHistory.Columns.Add("POSTDATE", typeof(string));
            dtHistory.Columns.Add("NEWSTITLE", typeof(string));
            dtHistory.Columns.Add("WATCH", typeof(string));
            dtHistory.Columns.Add("BLOGURL", typeof(string));
            dtHistory.PrimaryKey = aryDC;
            return dtHistory;
        }

        private DataTable getAccount()
        {
            //レジストリの値の名前配列
            string[] aryKeyNames = null;
            DataTable dtAccount = new DataTable();
            DataColumn[] aryDC = new DataColumn[1];
            aryDC[0] = dtAccount.Columns.Add("ID", typeof(string));
            dtAccount.Columns.Add("PW", typeof(string));
            dtAccount.Columns.Add("NN", typeof(string));
            dtAccount.PrimaryKey = aryDC;

            //レジストリの取得
            try
            {
                //レジストリキーを開く
                RegistryKey rKey = Registry.CurrentUser.CreateSubKey(Program.REGKEY_FPID);
                //レジストリの値の名前を取得
                aryKeyNames = rKey.GetSubKeyNames();
                //レジストリキーを閉じる
                rKey.Close();
                foreach (string keyName in aryKeyNames)
                {
                    string fc2id = keyName;
                    rKey = Registry.CurrentUser.OpenSubKey(Program.REGKEY_FPID + @"\" + fc2id);
                    string password = (string)rKey.GetValue(Program.FPPD);
                    string nickname = (string)rKey.GetValue(Program.FPNN);
                    dtAccount.Rows.Add(fc2id,password,nickname);
                }
                rKey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtAccount;
        }
    }
}
