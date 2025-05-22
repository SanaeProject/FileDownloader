# FileDownloader - C#でファイルダウンロードと処理を行うユーティリティ

## 概要
`FileDownloader` は、C# を使用してHTTP経由でファイルをダウンロードし、ファイル名の生成やHTML解析を行うユーティリティクラスです。

このライブラリは以下の機能を提供します:
- **ファイルダウンロード**
- **動的なファイル名生成**
- **HTMLからJavaScriptの配列データを取得**

## インストール方法
このプロジェクトでは `HtmlAgilityPack` を使用します。以下のコマンドでインストールできます。

```powershell
dotnet add package HtmlAgilityPack
```

## 使用方法

### ファイルのダウンロード

```csharp
using FileDownloader;

await FileUtil.Download("https://example.com/file.zip", "C:\\Downloads\\file.zip");
```

### 連番のファイル名を生成

```csharp
List<string> files = new List<string>();
FileUtil.GenerateFileNames("File_%3d.txt", files, new FileUtil.FileGenerationOptions { First = 1, Last = 5, Increase = 1 });

foreach (string file in files)
{
    Console.WriteLine(file);
}
// 出力: File_001.txt, File_002.txt, ..., File_005.txt
```

### HTMLからJavaScriptの配列を取得

```csharp
List<string> values = await FileUtil.GetArrayByHtml("https://example.com", "dataArray");
foreach (var val in values)
{
    Console.WriteLine(val);
}
```

## 依存関係
このクラスは以下のライブラリを使用します:
- [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack) - HTML解析用ライブラリ
- `System.Net.Http` - HTTPリクエスト処理用

## ライセンス
このプロジェクトは **MIT License** のもとで公開されています。