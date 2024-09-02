using HtmlAgilityPack;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml;


namespace FileDownloader
{
    internal class FileUtil
    {
        private static readonly HttpClient _httpClient = new HttpClient();


        /*==============================================
         * ダウンロード関数
        ==============================================*/
        public static async Task Download(string url, string path)
        {
            //リクエストの作成
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            //ヘッダを読み取った時点でresponseを返す
            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if(!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            //成功
            if (response.IsSuccessStatusCode)
            {
                using (
                    //responseのstreamを作成
                    Stream rstream = await response.Content.ReadAsStreamAsync(),
                           //fileのstreamを作成
                           fstream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                )
                {
                    //書き込み
                    await rstream.CopyToAsync(fstream);
                }
            }
            else
            {
                Console.WriteLine($"Error:{response.StatusCode}={url}");
            }
        }


        /*==============================================
         * ファイル名列挙オプション
         * First    :初期値
         * Last     :終値
         * Increase :増分
        ==============================================*/
        public struct FileGenerationOptions
        {
            public int First { get; set; }
            public int Last { get; set; }

            public int Increase { get; set; }
        }


        /*==============================================
         * ファイル列挙関数
         * 
         * "File_000","File_001"など連番な場合
         * File_%3dとすることで3桁の数字が入る。
         * 
         * FileGenerationOptionで初期値,終値,増分を指定できる。
        ==============================================*/
        public static void GenerateFileNames(string fileName, List<string> files, params FileGenerationOptions[] parm)
        {
            //%から始まり数字が0個以上でdで終わるパターンを取得
            Match match = Regex.Match(fileName, @"%(\d{0,})d");

            //マッチしない場合追加して終了
            if (match.Value == "")
            {
                //ファイル名を追加
                files.Add(fileName);
                //終了
                return;
            }

            //全体の桁数を取得
            Match numMatch = Regex.Match(match.Value, @"\d");
            int allDigit = int.Parse(numMatch.Value == "" ? "1" : numMatch.Value);


            //現在の要素に対するオプション
            FileGenerationOptions Option;

            //parmが0つまり引数がない場合新規にオプションを作成
            if (parm.Length == 0)
                Option = new FileGenerationOptions() { First = 0, Last = (int)Math.Pow(10, allDigit) - 1, Increase = 1 };
            else
                Option = parm[0];


            //初期値,終値,増分
            for (int i = Option.First; i <= Option.Last; i += Option.Increase)
            {
                //ゼロを格納する。
                string zeros = "";

                //現在の桁数を取得 iが0の時logは計算させてはならない
                int nowDigit = (i != 0 ? (int)Math.Floor(Math.Log10(i)) : 0) + 1;

                //桁数が足りない場合0で埋める
                for (int j = 0; j < allDigit - nowDigit; j++)
                    zeros += "0";

                //マッチした所と値を入れ替え
                string newfilename = ReplaceFirst(fileName, match.Value, zeros + i);

                //自分自身をコール
                if (parm.Length > 1)
                    //現在のパラメタをスキップし次のパラメタの配列を渡す。
                    GenerateFileNames(newfilename, files, parm.Skip(1).ToArray());
                else
                    //パラメタがもうないので現在のパラメタを渡す。
                    GenerateFileNames(newfilename, files, Option);
            }
        }


        /*==============================================
         * 文字列の最初部分のみReplaceする。
        ==============================================*/
        static string ReplaceFirst(string text, string oldValue, string newValue)
        {
            //探す
            int pos = text.IndexOf(oldValue);
            if (pos < 0)
                return text; // oldValueが見つからない場合は元の文字列を返す

            return text.Substring(0, pos) + newValue + text.Substring(pos + oldValue.Length);
        }


        public const string PatternInt = @"\d";
        public const string PatternWord = @"\w";
        public const string PatternAny = @".";

        /*==============================================
        * htmlからjavascriptを取得し配列を取得する。
        ==============================================*/
        public static async Task<List<string>> GetArrayByHtml(string url, string arrayName, string valuePattern = PatternInt)
        {
            //リスト型
            List<string> values = new List<string>();

            //html読み取り
            string htmlContent = await _httpClient.GetStringAsync(url);

            //ドキュメント取得
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            //javascript取得
            HtmlNodeCollection htmlCollection = htmlDocument.DocumentNode.SelectNodes("//script");

            //マッチパターンを与える。
            string[] matchPattern = {
                //{arrayName}[n] = value
                $@"{arrayName}\s*\[\s*(\d+)\s*\]\s*=\s*({valuePattern}+)+\s*;",
         
                //{arrayName} = {value1,value2,value3...};
                $@"{arrayName}\s*=\s*\{{\s*({valuePattern}(,\s*{valuePattern}+)*)\s*\}};"
            };

            //パターンから値を取得する。
            Action<Match>[] getValue = {
                //Pattern0の時値を取得するラムダ式
                //{arrayName}[n] = value
                (matched) =>
                {
                    //0:マッチ全体 1:index 2:値
                    values.Add(matched.Groups[2].Value.Trim());
                },

                //Pattern1の時値を取得するラムダ式
                //{arrayName} = {value1,value2,value3...};
                (matched) =>
                {
                    // {value1,value2...} を取得し分解する。
                    string[] arrayValues = matched.Groups[1].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    //それぞれ追加
                    foreach (string v in arrayValues)
                        values.Add(v.Trim());
                }
            };

            //各ノードで実行
            foreach (HtmlNode node in htmlCollection)
            {
                //matchPattern[0]のマッチを取得。
                MatchCollection matches = Regex.Matches(node.InnerText, matchPattern[0]);
                if (matches.Count > 0)
                {
                    //pattern0の場合複数
                    foreach (Match matched in matches)
                        getValue[0](matched);

                    continue;
                }

                //matchPattern[1]のマッチを取得。
                matches = Regex.Matches(node.InnerText, matchPattern[1]);
                if (matches.Count > 0)
                {
                    getValue[1](matches[0]);

                    continue;
                }
            }

            return values;
        }
    }
}