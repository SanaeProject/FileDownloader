# FileDownloader - C#�Ńt�@�C���_�E�����[�h�Ə������s�����[�e�B���e�B

## �T�v
`FileDownloader` �́AC# ���g�p����HTTP�o�R�Ńt�@�C�����_�E�����[�h���A�t�@�C�����̐�����HTML��͂��s�����[�e�B���e�B�N���X�ł��B

���̃��C�u�����͈ȉ��̋@�\��񋟂��܂�:
- **�t�@�C���_�E�����[�h**
- **���I�ȃt�@�C��������**
- **HTML����JavaScript�̔z��f�[�^���擾**

## �C���X�g�[�����@
���̃v���W�F�N�g�ł� `HtmlAgilityPack` ���g�p���܂��B�ȉ��̃R�}���h�ŃC���X�g�[���ł��܂��B

```powershell
dotnet add package HtmlAgilityPack
```

## �g�p���@

### �t�@�C���̃_�E�����[�h

```csharp
using FileDownloader;

await FileUtil.Download("https://example.com/file.zip", "C:\\Downloads\\file.zip");
```

### �A�Ԃ̃t�@�C�����𐶐�

```csharp
List<string> files = new List<string>();
FileUtil.GenerateFileNames("File_%3d.txt", files, new FileUtil.FileGenerationOptions { First = 1, Last = 5, Increase = 1 });

foreach (string file in files)
{
    Console.WriteLine(file);
}
// �o��: File_001.txt, File_002.txt, ..., File_005.txt
```

### HTML����JavaScript�̔z����擾

```csharp
List<string> values = await FileUtil.GetArrayByHtml("https://example.com", "dataArray");
foreach (var val in values)
{
    Console.WriteLine(val);
}
```

## �ˑ��֌W
���̃N���X�͈ȉ��̃��C�u�������g�p���܂�:
- [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack) - HTML��͗p���C�u����
- `System.Net.Http` - HTTP���N�G�X�g�����p

## ���C�Z���X
���̃v���W�F�N�g�� **MIT License** �̂��ƂŌ��J����Ă��܂��B