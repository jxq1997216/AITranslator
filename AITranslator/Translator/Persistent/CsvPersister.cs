using AITranslator.Exceptions;
using AITranslator.View.Models;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Text;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.Translator.Persistent
{
    public static class CsvPersister
    {
        static CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false, MissingFieldFound = null };
        public static Dictionary<string, string?> Load(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                Dictionary<string, string?> dic;
                if (File.Exists(fileBakName))
                {
                    try
                    {
                        dic = ReadData(fileBakName);
                        File.Move(fileBakName, fileName, true);
                    }
                    catch (Exception)
                    {
                        File.Delete(fileBakName);
                        dic = ReadData(filePath);
                    }
                }
                else
                    dic = ReadData(filePath);

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
        }

        public static void Save(ICollection<KeyValuePair<string, string?>> datas, string filePath, bool hide = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                string strString;
                KeyValueStruct[] records = datas.Select(s => new KeyValueStruct(s.Key, s.Value)).ToArray();

                using (var writer = new StringWriter())
                {
                    using (var csv = new CsvWriter(writer, config))
                        csv.WriteRecords(records);
                    strString = writer.ToString();
                }

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


        static Dictionary<string, string?> ReadData(string path)
        {
            Dictionary<string, string?> TppKvs = new Dictionary<string, string?>();

            KeyValueStruct[] records;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                using (CsvReader csv = new CsvReader(reader, config))
                {
                    records = csv.GetRecords<KeyValueStruct>().ToArray();
                }
            }
            foreach (KeyValueStruct record in records)
                TppKvs.Add(record.Key, record.Value);
            return TppKvs;
        }

        public static Dictionary<string, Dictionary<string, string?>> LoadFromFolder(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, "*.csv", SearchOption.AllDirectories);
            if (files.Length == 0)
                return new Dictionary<string, Dictionary<string, string?>>();
            Dictionary<string, Dictionary<string, string?>> csvDicDatas = new Dictionary<string, Dictionary<string, string?>>();
            foreach (string file in files)
            {
                Dictionary<string, string?> dics = Load(file).Where(s => !string.IsNullOrWhiteSpace(s.Key)).ToDictionary();
                if (dics.Count != 0)
                    csvDicDatas.Add(file[(folderPath.Length + 1)..], dics);
            }
            return csvDicDatas;
        }




        public static void SaveToFolder(string folderPath, Dictionary<string, Dictionary<string, string?>> csvDicDatas)
        {
            Directory.CreateDirectory(folderPath);
            foreach (var csvFileName in csvDicDatas.Keys)
            {
                string filePath = $"{folderPath}/{csvFileName}";
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                Save(csvDicDatas[csvFileName], filePath);
            }

        }

        public static Dictionary<string, string?> LoadMergeDicFromFolder(string folderPath)
        {
            Dictionary<string, Dictionary<string, string?>> csvDicDatas = LoadFromFolder(folderPath);
            return csvDicDatas.ToMergeDic();
        }

        public static void SaveMergeDicToFolder(string folderPath, Dictionary<string, string?> mergeDics)
        {
            Dictionary<string, Dictionary<string, string?>> csvDicDatas = new Dictionary<string, Dictionary<string, string?>>();
            foreach (var mergeDic in mergeDics)
            {
                string[] strs = mergeDic.Key.Split("||");
                string fileName = strs[0];
                string needTransStr = string.Join("||", strs[1..]);
                string translatedStr = mergeDic.Value;
                if (!csvDicDatas.ContainsKey(fileName))
                    csvDicDatas[fileName] = new Dictionary<string, string?>();
                csvDicDatas[fileName][needTransStr] = translatedStr;
            }

            SaveToFolder(folderPath, csvDicDatas);
        }

        public static Dictionary<string, string?> ToMergeDic(this Dictionary<string, Dictionary<string, string?>> csvDicDatas)
        {
            Dictionary<string, string?> result = new Dictionary<string, string?>();
            foreach (var filePath in csvDicDatas.Keys)
            {
                foreach (var needTransStr in csvDicDatas[filePath].Keys)
                    result[$"{filePath}||{needTransStr}"] = csvDicDatas[filePath][needTransStr];
            }
            return result;
        }

        public static Dictionary<string, Dictionary<string, string?>> ToSeparateDic(this Dictionary<string, string?> mergeDics)
        {
            Dictionary<string, Dictionary<string, string?>> csvDicDatas = new Dictionary<string, Dictionary<string, string?>>();
            foreach (var mergeDic in mergeDics)
            {
                string[] strs = mergeDic.Key.Split("||");
                string fileName = strs[0];
                string needTransStr = string.Join("||", strs[1..]);
                string translatedStr = mergeDic.Value;
                if (!csvDicDatas.ContainsKey(fileName))
                    csvDicDatas[fileName] = new Dictionary<string, string?>();
                csvDicDatas[fileName].Add(needTransStr, translatedStr);
            }
            return csvDicDatas;
        }

        public static int GetTotalCount(this Dictionary<string, Dictionary<string, string?>> mergeDics)
        {
            int count = 0;
            foreach (var value in mergeDics.Values)
                count += value.Count;
            return count;
        }
    }
}
