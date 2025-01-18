using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Tools;
using AITranslator.Translator.Translation;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// 清除翻译记录的确认窗口
    /// </summary>
    [ObservableObject]
    public partial class Window_TestVerificationTemplate : Window
    {
        Script<(bool, string)> _script;
        VerificationScriptInput _scriptInput = new VerificationScriptInput();

        [ObservableProperty]
        private string translated;
        [ObservableProperty]
        private string untranslated;
        [ObservableProperty]
        private bool isSuccessful = true;
        [ObservableProperty]
        private string failedMsg = "校验通过";
        [ObservableProperty]
        private bool testing;
        public Window_TestVerificationTemplate(string scriptPath)
        {
            InitializeComponent();
            try
            {
                _script = CSharpScript.Create<(bool, string)>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(VerificationScriptInput));
                _scriptInput.Translated = _scriptInput.Untranslated = string.Empty;
                _ = _script.RunAsync(_scriptInput).Result;
            }
            catch (Exception)
            {
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        private async void Button_Test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Testing = true;
                _scriptInput.Translated = Translated;
                _scriptInput.Untranslated = Untranslated;
                ScriptState<(bool, string)> state = await _script.RunAsync(_scriptInput);
                var result = state.ReturnValue;
                IsSuccessful = result.Item1;
                if (IsSuccessful)
                    FailedMsg = "校验通过";
                else
                    FailedMsg = result.Item2;
            }
            catch (CompilationErrorException err)
            {
                Window_Message.ShowDialog("错误", err.Message);
            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("未知错误", err.ToString());
            }
            finally
            {
                Testing = false;
            }
        }
    }
}
