using AITranslator.Exceptions;
using AITranslator.Translator.TranslateData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.Translator.Persistent
{
    public static class TxtPersister
    {
        public static List<string> Load(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                string strSource = ReadNovel(filePath).Replace("\r\n", "\n");
                return strSource.Split("\n").Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
            catch (IOException err)
            {
                throw new FileLoadException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (UnauthorizedAccessException)
            {
                throw new FileSaveException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
        }

        public static void Save(List<string> datas, string filePath, bool hide = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                string strString = string.Join('\n', datas);
                File.WriteAllText(fileBakName, strString);
                File.Delete(fileName);
                File.Copy(fileBakName, fileName, true);
                File.Delete(fileBakName);
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

        static string ReadNovel(string filenameTxt)
        {
            Encoding[] encodings =
            {
                Encoding.UTF8,
                Encoding.GetEncoding("shift_jis"),
                Encoding.GetEncoding("gbk"),
                Encoding.GetEncoding("big5")
            };
            foreach (Encoding encoding in encodings)
            {
                try
                {
                    Encoding enc = (Encoding)encoding.Clone();
                    enc.DecoderFallback = new DecoderExceptionFallback();
                    using (StreamReader file = new StreamReader(filenameTxt, enc))
                    {
                        string content = file.ReadToEnd();
                        return content;
                    }
                }
                catch (DecoderFallbackException)
                {
                    continue;
                }
            }
            throw new FileLoadException($"解码失败:{filenameTxt}，目前仅支持以下编码:UTF8/shift_jis/gbk/big5");
        }

    }
}
