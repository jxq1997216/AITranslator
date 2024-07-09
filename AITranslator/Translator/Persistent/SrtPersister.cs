using AITranslator.Exceptions;
using AITranslator.Translator.TranslateData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.Translator.Persistent
{
    public static class SrtPersister
    {
        public static Dictionary<int, SrtData> Load(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                string[] srtStrings = File.ReadAllText(filePath).Replace("\r\n", "\n").Split("\n\n");
                Dictionary<int, SrtData> dic = new Dictionary<int, SrtData>();
                for (int i = 0; i < srtStrings.Length; i++)
                {
                    string srtString = srtStrings[i];
                    if (string.IsNullOrWhiteSpace(srtString))
                        continue;

                    string[] srt_string = srtString.Split('\n');
                    srt_string = srt_string.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    int index = int.Parse(srt_string[0]);
                    //if (index != i + 1)
                    //    throw new FileLoadException($"\r\nSrt文件格式错误:非连续序号！");
                    dic.Add(index, new SrtData(srt_string[1..]));
                }

                return dic;
            }
            catch (IOException err)
            {
                throw new FileLoadException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (UnauthorizedAccessException)
            {
                throw new FileSaveException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (FormatException err)
            {
                throw new FileLoadException($"Srt文件格式错误:格式错误！");
            }
        }

        public static void Save(Dictionary<int, SrtData> datas, string filePath,bool hide = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var data in datas)
                    stringBuilder.Append($"{data.Key}\n{data.Value}\n\n");
                string strString = stringBuilder.ToString();
                File.WriteAllText(fileBakName, strString);
                File.Move(fileBakName, fileName, true);
                if (hide)
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    attributes |= FileAttributes.Hidden;
                    File.SetAttributes(filePath, attributes);
                }
            }
            catch (IOException)
            {
                throw new FileSaveException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (UnauthorizedAccessException)
            {
                throw new FileSaveException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
        }
    }
}
