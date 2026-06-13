using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/NotificationEvent")]
    public class NotificationEvent : BaseEvent<NotificationData>
    {
        
    }
    
    public class NotificationData
    {
        public string Text1;
        public string Text2;
        public Sprite BackGround;
        public Sprite Icon;
        public bool IsCenter;

        public NotificationData(string text1, string text2, Sprite backGround, Sprite icon, bool isCenter = false)
        {
            Text1 = text1;
            Text2 = text2;
            BackGround = backGround;
            Icon = icon;
            IsCenter = isCenter;
        }

        public const string PURCHASE_SUCCESS = "PURCHASE SUCCESS!";
        public const string SUBSCRIBE_SUCCESS = "SUBSCRIBE SUCCESS!";
        public const string CLAIM_SUCCESS = "CLAIM SUCCESS!";
        public const string FOUND_NEW_ITEM = "FOUND NEW ITEM!";
        public const string SUPPLY_DROPS = "SUPPLY HAS BEEN DROPPED!";
        public const string INVENTORY_FULL = "INVENTORY FULL";
        public const string EXCHANGE_SUCCESS = "EXCHANGE_SUCCESS";
    }
}