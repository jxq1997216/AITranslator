using AITranslator.Exceptions;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.Translator.Tools
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    public static class ExpandedFuncs
    {
        /// <summary>
        /// 将替换词存储格式转换为Dictionary，用于存储到本地
        /// </summary>
        /// <param name="kvs"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToReplaceDictionary(this ObservableCollection<KeyValueStr> kvs)
        {
            Dictionary<string, string> Replaces = new Dictionary<string, string>();
            foreach (var item in kvs)
            {
                string key = item.Key;
                string value = item.Value;
                if (!string.IsNullOrWhiteSpace(key))
                    Replaces[key] = value;
            }
            return Replaces;
        }

        /// <summary>
        /// 将替换词存储格式转换为ObservableCollection，用于加载和显示
        /// </summary>
        /// <param name="kvs"></param>
        /// <returns></returns>
        public static ObservableCollection<KeyValueStr> ToReplaceCollection(this Dictionary<string, string> kvs)
        {
            ObservableCollection<KeyValueStr> result = new ObservableCollection<KeyValueStr>();
            foreach (var item in kvs)
            {

                string key = item.Key;
                string value = item.Value;
                result.Add(new KeyValueStr(key, value));
            }
            return result;
        }

        static char[] splitechars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(this string path)
        {
            List<string> tails = path.Split(splitechars).ToList();
            int t = tails.RemoveAll(p => p == "..");
            List<string> heads = AppDomain.CurrentDomain.SetupInformation.ApplicationBase!.Split(splitechars).ToList();
            heads.RemoveRange(heads.Count - t - 1, t);
            return string.Join("/", heads) + string.Join("/", tails);
        }

        static Regex regex_EnglishPath = new Regex(@"[^\x00-\x7F]+");   //匹配是否为英文路径
        /// <summary>
        /// 检验是否为纯英文路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsEnglishPath(this string path)
        {
            try
            {
                // 获取路径的绝对路径
                string fullPath = Path.GetFullPath(path);

                // 使用正则表达式检查绝对路径是否包含非英文字符

                return !regex_EnglishPath.IsMatch(fullPath);
            }
            catch (Exception)
            {
                // 处理异常，例如无效路径等
                return false;
            }
        }
        /// <summary>
        /// 替换反斜杠位斜杠
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReplaceSlash(this string path) => path.Replace('/', '\\');

        /// <summary>
        /// 捕获异常
        /// </summary>
        /// <param name="action"></param>
        /// <param name="errorAction"></param>
        /// <param name="ShowDialog"></param>
        /// <returns></returns>
        public static bool TryExceptions(Action action, Action<Exception> errorAction = null, bool ShowDialog = true)
        {
            try
            {
                action.Invoke();
            }
            catch (DicNotFoundException err)
            {
                errorAction?.Invoke(err);
                string errMsg = $"未找到文件夹错误:{err.Message}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{errMsg}");
                if (ShowDialog)
                    Window_Message.ShowDialog("错误", errMsg);
                return false;
            }
            catch (FileLoadException err)
            {
                errorAction?.Invoke(err);
                string errMsg = $"加载文件错误:{err.Message}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{errMsg}");
                if (ShowDialog)
                    Window_Message.ShowDialog("错误", errMsg);
                return false;
            }
            catch (FileSaveException err)
            {
                errorAction?.Invoke(err);
                string errMsg = $"保存文件错误:{err.Message}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{errMsg}");
                if (ShowDialog)
                    Window_Message.ShowDialog("错误", errMsg);
                return false;
            }
            catch (KnownException err)
            {
                errorAction?.Invoke(err);
                string errMsg = $"错误:{err.Message}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{errMsg}");
                if (ShowDialog)
                    Window_Message.ShowDialog("错误", errMsg);
                return false;
            }
            catch (Exception err)
            {
                errorAction?.Invoke(err);
                string errMsg = $"未知错误:{err}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{errMsg}");
                if (ShowDialog)
                    Window_Message.ShowDialog("错误", errMsg);
                return false;
            }
            return true;
        }
    }
}
