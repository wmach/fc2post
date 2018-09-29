using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Sgml;
using System.Collections;

namespace WindowsFormsApplication1
{
    public class Scraper
    {
        private string liveDoorNewsURL = "http://news.livedoor.com/search/article/?type=article&word={0}&x=0&y=0";
        private string liveDoorSubstitution = "http://news.livedoor.com/summary/list/";

        private static string YOUTUBEEMBEDCODE = "<object width=\"384\" height=\"308\">" +
            "<param name=\"movie\" value=\"http://www.youtube.com/v/{0}&hl=ja_JP&fs=1&\">"+
            "</param><param name=\"allowFullScreen\" value=\"true\"></param>"+
            "<param name=\"allowscriptaccess\" value=\"always\"></param>"+
            "<embed src=\"http://www.youtube.com/v/{0}&hl=ja_JP&fs=1&\" type=\"application/x-shockwave-flash\"" +
            " allowscriptaccess=\"always\" allowfullscreen=\"true\" width=\"384\" height=\"308\"></embed></object>";

        private Program context = null;
        private ScraperLiveDoorNewsList scraperLiveDoorNewsList = null;
        private Dictionary<string, string> dicDtHistoryRow = new Dictionary<string, string>();

        public Scraper(Program context)
        {
            this.context = context;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/ブログのURL再設定処理
        //_/
        public void SetBlogUrl(string url)
        {
            this.dicDtHistoryRow.Remove("BLOGURL");
            string blogurl = System.Text.RegularExpressions.Regex.Replace(url, "http://", "http://" + this.context.sID + ".");
            blogurl = System.Text.RegularExpressions.Regex.Replace(blogurl, "/control.php$", "");
            this.dicDtHistoryRow.Add("BLOGURL", blogurl);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/投稿中データのセッション取得処理
        //_/
        public ArrayList GetSession(string strHtmlArg)
        {
            string strHtml = Encoding.UTF8.GetString(Encoding.GetEncoding("euc-jp").GetBytes(strHtmlArg));
            var root = XDocument.Load(new SgmlReader { DocType = "HTML", InputStream = new StringReader(strHtml) });
            var ns = root.Root.Name.Namespace;
            ArrayList arrList = new ArrayList();
            var result = root
                .Descendants(ns + "input")
                .Where(e => e.Attribute("type") != null
                         && e.Attribute("type").Value == "hidden"
                         && e.Attribute("name") != null
                         && e.Attribute("name").Value.StartsWith("entry["))
                .Select(e => new
                {
                    Sess = e.Attribute("name").Value + e.Attribute("value").Value
                });

            foreach (var es in result)
            {
                if (es.Sess.StartsWith("entry[sessid]"))
                    arrList.Add("entry[sessid]="+es.Sess.Replace("entry[sessid]", ""));
                if (es.Sess.StartsWith("entry[sespsd]"))
                    arrList.Add("entry[sespsd]="+es.Sess.Replace("entry[sespsd]", ""));
            }

            return arrList;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/投稿ブログのＵＲＬ取得処理
        //_/
        public void GetBlogUrl(string strHtmlArg)
        {
            string strHtml = Encoding.UTF8.GetString(Encoding.GetEncoding("euc-jp").GetBytes(strHtmlArg));

            var root = XDocument.Load(new SgmlReader { DocType = "HTML", InputStream = new StringReader(strHtml) });
            //XNamespace ns = "http://www.w3.org/1999/xhtml";
            var ns = root.Root.Name.Namespace;


            //検索結果をスクレイプ
            var result = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("id") != null
                         && e.Attribute("id").Value == "editor_area")
                .Select(e => new
                {
                    Anchor = e.Descendants(ns + "a").FirstOrDefault().Attribute("href").Value
                });

            //次ページ名標をキーに、ＵＲＬを辞書に登録
            foreach (var elm in result)
            {
                if (this.dicDtHistoryRow.ContainsKey("BLOGURL"))
                    this.dicDtHistoryRow.Remove("BLOGURL");
                this.dicDtHistoryRow.Add("BLOGURL", elm.Anchor);
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/記事スクレイピング処理
        //_/
        public bool ScrapingItem(string queryArg)
        {
            //投稿データのクリア
            this.dicDtHistoryRow.Clear();

            //検索語でひっかかっている場合
            if (this.scraperLiveDoorNewsList == null)
            {
                //記事検索処理の呼び出し
                this.SearchPostingNews(queryArg);

                //未投稿記事があったか判定
                if (!this.dicDtHistoryRow.ContainsKey("URL")
                || "".Equals(this.dicDtHistoryRow["URL"]))
                {
                    if (this.scraperLiveDoorNewsList == null)
                    {
                        this.scraperLiveDoorNewsList = new ScraperLiveDoorNewsList(this.context);
                    }
                }
            }
            //検索語でひっかからなかった場合ライブドアニュース一覧から取得
            if (this.scraperLiveDoorNewsList != null)
            {
                this.dicDtHistoryRow = this.scraperLiveDoorNewsList.ScrapingItem();

                //未投稿記事があったか判定
                if (!this.dicDtHistoryRow.ContainsKey("URL")
                || "".Equals(this.dicDtHistoryRow["URL"]))
                {
                    this.dicDtHistoryRow.Add("NEWSTITLE", "未投稿の記事がありません");
                    return false;
                }
            }
            //記事の取得
            var root = XDocument.Load(new SgmlReader { Href = this.dicDtHistoryRow["URL"] });
            XNamespace ns = "http://www.w3.org/1999/xhtml";
            Dictionary<string, string> dicRtn = new Dictionary<string, string>();

            //記事内容をスクレイプ
            var result = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "article")
                .Select(e => new
                {
                    Contents = e.Value
                });
            //名標をキーに、ニュース内容を辞書に登録
            foreach (var item in result)
            {
                string contents = System.Text.RegularExpressions.Regex.Replace(item.Contents, "。(　| )+", "。\n　");
                contents = contents
                         + "\n\n［配信元］" + this.dicDtHistoryRow["URL"] + "\n"
                         + "※この記事の著作権は配信元に帰属します\n";

                this.dicDtHistoryRow.Add("CONTENTS", contents);
            }

            return true;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/未ポスト記事検索処理
        //_/
        public void SearchPostingNews(string strQueryArg)
        {
            var strUrl = liveDoorNewsURL;
            Dictionary<string, string[]> rtnDic = null;

            //ライブドアニュース記事をスクレイプ
            if (!(rtnDic = PagingLiveDoor(string.Format(strUrl, HttpUtility.UrlEncode(strQueryArg, Encoding.GetEncoding("EUC-JP"))))).Count.Equals(0))
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
        public Dictionary<string, string[]> PagingLiveDoor(string urlArg)
        {
            //戻り値のオブジェクトを作成
            Dictionary<string, string[]> dicRtn = new Dictionary<string, string[]>();

            //ループブレイクにnullを設定
            string next = null;
            do
            {
                var item = ExtractItem(next == null ? urlArg : next);
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

            //タイトルライン検索結果をスクレイプ
            var resultTitle = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("id") != null
                         && e.Attribute("id").Value == "headline")
                .Select(e => new
                {
                    Anchor = e.Descendants(ns + "h2").FirstOrDefault()
                              .Descendants(ns + "a").FirstOrDefault().Attribute("href").Value,
                    Title = e.Descendants(ns + "h2").FirstOrDefault()
                              .Descendants(ns + "a").FirstOrDefault().Value
                });

            foreach (var item in root.Descendants(ns + "div"))
            {
                Console.WriteLine(item.Value);
            }

            //一覧検索結果をスクレイプ
            var resultList = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("id") != null
                         && e.Attribute("id").Value == "article-list")
                .Descendants(ns + "ul")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "article-list")
                .SelectMany(e => e.Descendants(ns + "li").Descendants(ns + "a"));

            //次ページをスクレイプ
            var nextPage = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "column-inner")
                .Descendants(ns + "ul")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "paging")
                .Descendants(ns + "li")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "next")
                .Select(e => new
                {
                    NextPage = e.Descendants(ns + "a").FirstOrDefault().Attribute("href").Value
                });

            //ＵＲＬをキーに、ニュースタイトル、ＵＲＬを辞書に登録（タイトルライン）
            foreach (var item in resultTitle)
            {
                string[] strAry = { item.Title, item.Anchor, "" };
                dicRtn.Add(item.Anchor, strAry);
            }

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
        //_/動画埋め込みコード作成処理
        //_/
        public void CreateEmbedCode(string queryArg)
        {
            SearchPostingMovie(queryArg);
            if (!this.dicDtHistoryRow.ContainsKey("WATCH")
            || "".Equals(this.dicDtHistoryRow["WATCH"]))
            {
                return;
            }
            this.dicDtHistoryRow.Add("EMBEDCODE", string.Format(Scraper.YOUTUBEEMBEDCODE, dicDtHistoryRow["WATCH"]));
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/未ポスト動画検索処理
        //_/
        public void SearchPostingMovie(string strQueryArg)
        {
            var strNew = "http://www.youtube.com/results?search_query={0}&aq=f";
            Dictionary<string, string> rtnDic = null;

            //動画のＵＲＬをスクレイプ
            if (!(rtnDic = PagingYoutube(string.Format(strNew, HttpUtility.UrlEncode(strQueryArg, Encoding.GetEncoding("UTF-8"))))).Count.Equals(0))
            {
                foreach (KeyValuePair<string, string> pair in rtnDic)
                {
                    this.dicDtHistoryRow.Add(pair.Key, pair.Value);
                }
                return;
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/ＹｏｕＴｕｂｅページング処理
        //_/
        public Dictionary<string, string> PagingYoutube(string urlArg)
        {
            //戻り値のオブジェクトを作成
            Dictionary<string, string> dicRtn = new Dictionary<string, string>();

            //ループブレイクにnullを設定
            string next = null;
            do
            {
                var item = ExtractMovie(next == null ? urlArg : next);
                foreach (KeyValuePair<string, string> pair in item)
                {
                    //次ページ名標の場合
                    if (pair.Key.Equals("NextPage"))
                    {
                        //次ページのＵＲＬをループブレイクに設定
                        next = pair.Value;

                        //ループ先頭にリエントリ
                        continue;
                    }
                    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
                    //_/
                    //_/未ポスト動画か判定
                    //_/
                    else if (!this.IsAlreadyPosted("WATCH", pair.Value))
                    {
                        dicRtn.Add("WATCH", pair.Value);
                        return dicRtn;
                    }
                }
            } while (next != null && !"".Equals(next));
            return dicRtn;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //_/
        //_/動画検索処理
        //_/
        public Dictionary<string, string> ExtractMovie(string strUrlArg)
        {
            XDocument root = null;
            try
            {
                root = XDocument.Load(new SgmlReader { Href = strUrlArg });
            }
            catch (Exception ignore)
            {
                Dictionary<string, string> rtn = new Dictionary<string, string>();
                rtn.Add("NextPage", "");
                return rtn;
            }
            var ns = root.Root.Name.Namespace;
            Dictionary<string, string> dicRtn = new Dictionary<string, string>();

            //検索結果をスクレイプ
            var result = root
                .Descendants(ns + "div")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value.StartsWith("video-entry"))
                         //&& e.Attribute("class").Value == "video-entry yt-uix-hovercard")
                .Select(e => new
                {
                    Anchor = e.Descendants(ns + "a").FirstOrDefault().Attribute("href").Value,
                });

            //次ページをスクレイプ
            var nextOrPrePage = root
                .Descendants(ns + "a")
                .Where(e => e.Attribute("class") != null
                         && e.Attribute("class").Value == "yt-uix-pager-link"
                         && e.Value == "次へ")
                .Select(e => new
                {
                    NextPage = e.Attribute("href").Value
                });

            //ＵＲＬ名標をキーに、ＵＲＬを辞書に登録
            foreach (var item in result)
            {
                dicRtn.Add(item.Anchor, System.Text.RegularExpressions.Regex.Replace(item.Anchor, "^/watch\\?v=", ""));
            }

            //次ページ名標をキーに、ＵＲＬを辞書に登録
            foreach (var item in nextOrPrePage)
            {
                if (dicRtn.ContainsKey("NextPage"))
                    dicRtn.Remove("NextPage");
                dicRtn.Add("NextPage", item != null ? "http://www.youtube.com" + item.NextPage : "");
            }

            //ページの全記事を辞書に登録して返す
            return dicRtn;
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
