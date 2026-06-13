#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace Dacodelaac.Utils
{
    public static class UrlUtils
    {
        public static void OpenXsollaShop(string purchaseId, string productId)
        {
#if DACODER_RELEASE
            var baseUrl = "http://soulhuntress.pantheraplay.com/";
#else
            var baseUrl = "https://sitebuilder.xsolla.com/preview/soul-huntress";
#endif

            var url = string.IsNullOrEmpty(productId)
                ? $"{baseUrl}?user-id={purchaseId}"
                : $"{baseUrl}?user-id={purchaseId}&purchase-sku={productId}";

            Application.OpenURL(url);
        }


        public static void OpenDiscord()
        {
            Application.OpenURL("https://discord.gg/rmG5m4GEF3");
        }
        public static void OpenFacebookFanpage()
        {
            if (IsFacebookAppInstalled())
            {
#if UNITY_ANDROID
                Application.OpenURL("fb://page/61574732186961");
#elif UNITY_IOS
                Application.OpenURL("fb://page?id=61574732186961");
#endif
            }
            else
            {
                Application.OpenURL("https://www.facebook.com/61574732186961");
            }
        }
        public static void OpenFacebookGroup()
        {
            if (IsFacebookAppInstalled())
            {
#if UNITY_ANDROID
                Application.OpenURL("fb://group/1571975226803580");
#elif UNITY_IOS
                Application.OpenURL("fb://group?id=1571975226803580");
#endif
            }
            else
            {
                Application.OpenURL("https://www.facebook.com/1571975226803580");
            }
        }

        static bool IsFacebookAppInstalled()
        {
#if UNITY_EDITOR
            return false;
#endif
#if UNITY_ANDROID
            var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var packageName = new AndroidJavaObject("java.lang.String", "com.facebook.katana");

            try
            {
                pm.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                return true; // Facebook app is installed
            }
            catch (AndroidJavaException)
            {
                return false; // Facebook app is not installed
            }
#endif

#if UNITY_IOS
            return IsFacebookAppInstalledIOS();
#endif
        }

        public static void NavigateToGooglePlayStore()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
        }

        public static void NavigateToAppStore(string appStoreId)
        {
            Application.OpenURL($"itms-apps://itunes.apple.com/app/id{appStoreId}");
        }

        public static void NavigateToUpdateGooglePlayServices()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.google.android.gms");
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern bool IsFacebookAppInstalledIOS();
#endif
    }
}