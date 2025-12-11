using System;

namespace _Project.Script.Core
{
    public class PopupParam
    {
        public string title;
        public string message;

        public Action confirm;
        public Action cancel;

        public PopupParam()
        {
            
        }
        public PopupParam(string title, string message, Action confirm, Action cancel)
        {
            this.title = title;
            this.message = message;
            this.confirm = confirm;
            this.cancel = cancel;
        }

        public PopupParam(string title, string message)
        {
            this.title = title;
            this.message = message;
        }
    }
}
