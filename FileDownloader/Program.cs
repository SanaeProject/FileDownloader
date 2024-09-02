using System.Text;
using System.Text.RegularExpressions;
using static FileDownloader.FileUtil;


namespace FileDownloader
{
    //ファイルダウンローダ
    public class FileDownloader
    {
        //URL,オプション
        static void Main(string[] args)
        {
            if (args.Length < 2 || !args.Contains("-o"))
            {
                Console.WriteLine("Usage: FileDownloader <url> -o <start> <end> <increment> [ -o <start> <end> <increment> ... ]");
                return;
            }

            string url = args[0];

            // オプションをパースする
            List<FileUtil.FileGenerationOptions> options = new List<FileUtil.FileGenerationOptions>();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 3 < args.Length)
                {
                    int start = int.Parse(args[i + 1]);
                    int end = int.Parse(args[i + 2]);
                    int increment = int.Parse(args[i + 3]);
                    options.Add(new FileUtil.FileGenerationOptions { First = start, Last = end, Increase = increment });
                    i += 3;
                }
            }

            // パターンに基づいてファイル名を生成
            List<string> fileNames = new List<string>();
            FileUtil.GenerateFileNames(url, fileNames, options.ToArray());

            // ダウンロード処理
            foreach (string fileUrl in fileNames)
            {
                string fileName = Path.GetFileName(fileUrl);
                Console.WriteLine($"Downloading {fileUrl} to {fileName}");
                FileUtil.Download(fileUrl, fileName).Wait();
            }
        }
    }
}