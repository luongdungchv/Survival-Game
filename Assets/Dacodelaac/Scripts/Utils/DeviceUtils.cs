using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class DeviceUtils
    {
        public static string AdjustAdId;
        public static string GpsAdId;
        public static string Idfa;
        public static string ExternalDeviceId;

        public static string GetDeviceId()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
                //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
                var clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
                var objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
                var clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
                return clsSecure.CallStatic<string>("getString", objResolver, "android_id");
#elif UNITY_IPHONE
			    return UnityEngine.iOS.Device.vendorIdentifier; //TODO: Change when uninstall app. should implement store in keychain
#endif
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }
    }
}