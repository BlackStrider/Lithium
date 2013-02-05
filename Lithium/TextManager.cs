using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lithium
{
    class TextManager
    {
        private delegate void TextChanger();

        public TextManager() {}

        public void ShowMessage(RichTextBox rtb, string NickName, string message)
        {
            Thread mChanger = new Thread(new ThreadStart(delegate() { this.ChangeTextProperly(rtb, NickName, message); }));
            mChanger.Start();
        }

        public void ShowSystemMessage(RichTextBox rtb, string message)
        {
            Thread mChanger = new Thread(new ThreadStart(delegate() { this.ChangeTextProperly(rtb, message); }));
            mChanger.Start();
        }

        //for usual messages
        private void ChangeTextProperly(RichTextBox rtb, string nick, string msg) 
        {
            if (rtb.Dispatcher.CheckAccess())
            {
                rtb.AppendText("[" + nick + "]" + msg + "\n");
                return;
            }
            else
            {
                rtb.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TextChanger(delegate() { this.ChangeTextProperly(rtb, nick, msg); }));
            }
        }

        //for system messages
        private void ChangeTextProperly(RichTextBox rtb, string msg)
        {
            if (rtb.Dispatcher.CheckAccess())
            {
                rtb.AppendText(msg + "\n");
                return;
            }
            else
            {
                rtb.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new TextChanger(delegate() { this.ChangeTextProperly(rtb, msg); }));
            }
        }
    }
}
