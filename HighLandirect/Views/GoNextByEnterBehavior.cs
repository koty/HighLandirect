using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HighLandirect.Views
{
    /// <summary>
    /// http://tnakamura.hatenablog.com/entry/20090812/attached_behavior
    /// </summary>
    class GoNextByEnterBehavior
    {
        // 添付プロパティの初期値は false。
        // コールバックを指定しておく。
        public static readonly DependencyProperty EnterCommand = DependencyProperty.RegisterAttached("EnterCommand",
            typeof(bool),
            typeof(GoNextByEnterBehavior),
            new UIPropertyMetadata(false, EnterCommandChanged));

        public static bool GetEnterCommand(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnterCommand);
        }

        public static void SetEnterCommand(DependencyObject obj, bool value)
        {
            obj.SetValue(EnterCommand, value);
        }

        // EnterCommand の値が変更されたときに呼び出される。
        // KeyDown イベントハンドラの登録＆解除を行う。
        public static void EnterCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = sender as UIElement;
            if (element == null)
            {
                return;
            }

            if (GetEnterCommand(element))
            {
                element.KeyDown += textBox_KeyDown;
            }
            else
            {
                element.KeyDown -= textBox_KeyDown;
            }
        }

        // Enter キーが押されたら、次のコントロールにフォーカスを移動する
        private static void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.None) && (e.Key == Key.Enter))
            {
                UIElement element = sender as UIElement;
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
