using AITranslator.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.Translator.Persistent
{
    /// <summary>
    /// 将数据保存到硬盘，或从硬盘读取出来
    /// </summary>
    public static class JsonPersister
    {
        /// <summary>
        /// 从硬盘读取数据
        /// </summary>
        /// <typeparam name="T">要被加载的类型</typeparam>
        /// <param name="filePath">文件地址</param>
        /// <returns>被加载成功的数据</returns>
        /// <exception cref="JsonDeserializeLoadException">加载失败异常</exception>
        public static T Load<T>(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            string json;
            T result;
            try
            {
                if (File.Exists(fileBakName))
                {
                    try
                    {
                        json = File.ReadAllText(fileBakName);
                        result = JsonConvert.DeserializeObject<T>(json);
                        File.Move(fileBakName, fileName, true);
                    }
                    catch (JsonReaderException err)
                    {
                        File.Delete(fileBakName);
                        json = File.ReadAllText(fileName);
                        result = JsonConvert.DeserializeObject<T>(json);
                    }
                }
                else
                {
                    json = File.ReadAllText(fileName);
                    result = JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (IOException)
            {
                throw new FileLoadException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (UnauthorizedAccessException)
            {
                throw new FileLoadException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
            catch (JsonException err)
            {
                throw new FileLoadException($"读取Json文件失败:{err.InnerException?.Message ?? err.Message}");
            }
            return result;
        }

        /// <summary>
        /// 保存数据到硬盘
        /// </summary>
        /// <typeparam name="T">要被保存的类型</typeparam>
        /// <param name="obj">要被保存的数据</param>
        /// <param name="filePath">要保存到的地址</param>
        /// <exception cref="JsonSerializeSaveException">保存失败异常</exception>
        public static void Save<T>(T obj, string filePath,bool hide = false)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string fileExtension = fileInfo.Extension;
            string fileName = fileInfo.FullName;
            string fileNameNoExtension = fileName[..^fileExtension.Length];
            string fileBakName = fileNameNoExtension + "_bak" + fileExtension;
            try
            {
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                File.WriteAllText(fileBakName, json);
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
            catch (UnauthorizedAccessException err)
            {
                throw new FileSaveException($"\r\n以下文件中的一个或多个被拒绝访问，请确保文件未被占用：\r\n[{fileBakName}]\r\n[{fileName}]\r\n");
            }
        }

    }
}
