using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Documents;

namespace AITranslator.View.Properties
{
    public enum InputType
    {
        All,
        Integer,
        Decimal,
        UnsignedInteger,
        UnsignedDecimal
    }
    public static class TextBoxAttachedProperties
    {
        static Regex integerRegex = new Regex("[^0-9]+");

        public static InputType GetInputType(DependencyObject obj)
        {
            return (InputType)obj.GetValue(IsOnlyNumberProperty);
        }
        public static void SetInputType(DependencyObject obj, InputType value)
        {
            obj.SetValue(IsOnlyNumberProperty, value);
        }

        public static readonly DependencyProperty IsOnlyNumberProperty =
            DependencyProperty.RegisterAttached("InputType", typeof(InputType), typeof(TextBox), new PropertyMetadata(InputType.All,
                (s, e) =>
                {

                    if (s is TextBox textBox)
                    {
                        InputType inputType = (InputType)e.NewValue;

                        CommandManager.RemovePreviewCanExecuteHandler(textBox, HandleCanExecute);
                        textBox.PreviewTextInput -= TxtInputUnsignedInteger;
                        textBox.PreviewTextInput -= TxtInputUnsignedDecimal;
                        textBox.PreviewTextInput -= TxtInputInteger;
                        textBox.PreviewTextInput -= TxtInputDecimal;

                        switch (inputType)
                        {
                            case InputType.All:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, true);
                                break;
                            case InputType.Integer:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                                CommandManager.AddPreviewCanExecuteHandler(textBox, HandleCanExecute);
                                textBox.PreviewTextInput += TxtInputInteger;
                                break;
                            case InputType.Decimal:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                                CommandManager.AddPreviewCanExecuteHandler(textBox, HandleCanExecute);
                                textBox.PreviewTextInput += TxtInputDecimal;
                                break;
                            case InputType.UnsignedInteger:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                                CommandManager.AddPreviewCanExecuteHandler(textBox, HandleCanExecute);
                                textBox.PreviewTextInput += TxtInputUnsignedInteger;
                                break;
                            case InputType.UnsignedDecimal:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                                CommandManager.AddPreviewCanExecuteHandler(textBox, HandleCanExecute);
                                textBox.PreviewTextInput += TxtInputUnsignedDecimal;
                                break;
                            default:
                                textBox.SetValue(InputMethod.IsInputMethodEnabledProperty, true);
                                break;
                        }

                    }
                }));

        // TODO 有符号整数输入限制未完成
        private static void TxtInputInteger(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            e.Handled = integerRegex.IsMatch(e.Text);
        }

        // TODO 有符号小数输入限制未完成
        private static void TxtInputDecimal(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (e.Text == ".")
            {
                e.Handled = textbox.Text.Contains('.');
            }
            else if (e.Text == "-")
            {

            }
            else
            {
                e.Handled = integerRegex.IsMatch(e.Text);
            }
        }

        /// <summary>
        /// 无符号整数输入限制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TxtInputUnsignedInteger(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = integerRegex.IsMatch(e.Text);
        }

        /// <summary>
        /// 无符号小数输入限制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TxtInputUnsignedDecimal(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            if (e.Text == ".")
                e.Handled = textbox.Text.Contains('.');
            else
                e.Handled = integerRegex.IsMatch(e.Text);
        }

        private static void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
            else if (e.Command == EditingCommands.Backspace)
            {
                if (textbox.SelectionLength == textbox.Text.Length || (textbox.CaretIndex == 1 && textbox.Text.Length == 1))
                {
                    e.CanExecute = false;
                    e.Handled = true;
                }
            }
            else if (e.Command == EditingCommands.Delete)
            {
                if (textbox.SelectionLength == textbox.Text.Length || (textbox.CaretIndex == 0 && textbox.Text.Length == 1))
                {
                    e.CanExecute = false;
                    e.Handled = true;
                }
            }
        }
    }

}
