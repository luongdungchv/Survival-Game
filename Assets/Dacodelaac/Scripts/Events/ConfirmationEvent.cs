using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/ConfirmationEvent")]
    public class ConfirmationEvent : BaseEvent<ConfirmationData>
    {
    }
    
    public class ConfirmationData
    {
        public string Header;
        public string Message;
        public bool ShowYesBtn;
        public string YesBtn;
        public System.Action OnYesEvent;
        public bool ShowNoBtn;
        public string NoBtn;
        public System.Action OnNoEvent;
        public bool ShowCloseBtn;
        public System.Action OnCloseEvent;
        public List<string> Params;

        public ConfirmationData(string header, string message, 
            bool showYesBtn = true, string yesBtn = "", Action yesEvent = null, 
            bool showNoBtn = false, string noBtn = "", Action noEvent = null, 
            bool showCloseBtn = false, Action closeEvent = null, List<string> @params = null)
        {
            Header = header;
            Message = message;
            ShowYesBtn = showYesBtn;
            YesBtn = yesBtn;
            OnYesEvent = yesEvent;
            ShowNoBtn = showNoBtn;
            NoBtn = noBtn;
            OnNoEvent = noEvent;
            ShowCloseBtn = showCloseBtn;
            OnCloseEvent = closeEvent;
            Params = @params;
        }
    }
}