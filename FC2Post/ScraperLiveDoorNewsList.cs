using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sgml;
using System.Xml.Linq;
using System.Web;

namespace WindowsFormsApplication1
{
    class ScraperLiveDoorNewsList
    {
        private string liveDoorSubstitution = "http://news.livedoor.com/summary/list/";

        private Program context = null;
        private Dictionary<string, string> dicDtHistoryRow = new Dictionary<string, string>();

        public ScraperLiveDoorNewsList(Program context)
        {
            this.context = context;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/記事スクレイピング処理
        //_/
        public Dictionary<String, String> ScrapingItem()
        {
            Dictionary<String, String> rtn = null;

            //投稿データのクリア
            this.dicDtHistoryRow.Clear();

            //記事検索処理の呼び出し
            this.SearchPostingNews();

            //未投稿記事があったか判定
            if (!this.dicDtHistoryRow.ContainsKey("URL")
            || "".Equals(this.dicDtHistoryRow["URL"]))
            {
                this.dicDtHistoryRow.Add("NEWSTITLE", "未投稿の記事がありません");
                return rtn;
            }
            //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
            //_/
            //_/記事は取得しない
            //_/
            rtn = this.dicDtHistoryRow;
            return rtn;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/未ポスト記事検索処理
        //_/
        public void SearchPostingNews()
        {
            var strUrl = liveDoorSubstitution;
            Dictionary<string, string[]> rtnDic = null;

            //ライブドアニュース記事をスクレイプし、記事があったか判定
            if ((rtnDic=PagingLiveDoor()) != null)
            {
                foreach (KeyValuePair<string, string[]> pair in rtnDic)
                {
                    this.dicDtHistoryRow.Add("KEY", pair.Key);
                    this.dicDtHistoryRow.Add("URL", pair.Value[1]);
                    this.dicDtHistoryRow.Add("NEWSTITLE", pair.Value[0]);
                    this.dicDtHistoryRow.Add("CONTSUB", "");
                }
                return;
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/liveDoorニュースページング処理
        //_/
        public Dictionary<string, string[]> PagingLiveDoor()
        {
            //戻り値のオブジェクトを作成
            Dictionary<string, string[]> dicRtn = new Dictionary<string, string[]>();

            //ループブレイクにnullを設定
            string next = null;
            do
            {
                var item = ExtractItem(next == null ? liveDoorSubstitution : next);
                foreach (KeyValuePair<string, string[]> pair in item)
                {
                    //次ページ名標の場合
                    if (pair.Key.Equals("NextPage"))
                    {
                        //次ページのＵＲＬをループブレイクに設定
                        next = pair.Value[0];

                        //ループ先頭にリエントリ
                        continue;
                    }
                    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
                    //_/
                    //_/未ポスト記事か判定
                    //_/
                    else if (!this.IsAlreadyPosted("KEY", pair.Key))
                    {
                        dicRtn.Add(pair.Key, pair.Value);
                        return dicRtn;
                    }
                }
            } while (next != null && !"".Equals(next));
            return dicRtn;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/記事検索処理
        //_/
        public Dictionary<string, string[]> ExtractItem(string strUrlArg)
        {
            XDocument root = null;
            try
            {
                root = XDocument.Load(new SgmlReader { Href = strUrlArg });
            }
            catch (Exception ignore)
            {
                Dictionary<string, string[]> rtn = new Dictionary<string, string[]>();
                rtn.Add("NextPage", new string[] { "" });
                return rtn;
            }

            XNamespace ns = "http://www.w3.org/1999/xhtml";
            Dictionary<string, string[]> dicRtn = new Dictionary<string, string[]>();

            //一覧検索結果をスクレイプ
            var resultList = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "column-inner")
                .Descendants(ns + "ul")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "summary-list autopagerize_page_element")
                .SelectMany(e => e.Descendants(ns + "li").Descendants(ns + "h2").Descendants(ns + "a"));

            //次ページをスクレイプ
            var nextPage = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "paging paging-bottom")
                .Descendants(ns + "ul")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "paging-deluxe")
                .Descendants(ns + "li")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "next")
                .Select(e => new
                {
                    NextPage = e.Descendants(ns + "a").FirstOrDefault().Attribute("href").Value
                });

            //ＵＲＬをキーに、ニュースタイトル、ＵＲＬを辞書に登録（リスト）
            foreach (var item in resultList)
            {
                string[] strArray = { item.Value, item.FirstAttribute.Value };
                dicRtn.Add(item.FirstAttribute.Value, strArray);
            }

            //次ページ名標をキーに、ＵＲＬを辞書に登録
            foreach (var item in nextPage)
            {
                dicRtn.Add("NextPage", new string[] { item != null ? "http://news.livedoor.com" + item.NextPage : "" });
            }

            //ページの全記事を辞書に登録して返す
            return dicRtn;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/投稿中データ取得処理
        //_/
        public string GetPostingData(string keyArg)
        {
            if (!this.dicDtHistoryRow.ContainsKey(keyArg))
                if (keyArg.Equals("KEY"))
                    return Guid.NewGuid().ToString();
                else
                    return "";
            else
                return this.dicDtHistoryRow[keyArg];
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/投稿履歴重複チェック処理
        //_/
        public bool IsAlreadyPosted(string keyNameArg, string keyValueArg)
        {
            bool rtn = false;
            string format = "{0}='{1}'";
            string query = string.Format(format, keyNameArg, keyValueArg);
            var rows = context.dtHistory.Select(query);
            if (rows!=null && !rows.Count().Equals(0))
            {
                rtn = true;
            }
            return rtn;
        }
    }
}
