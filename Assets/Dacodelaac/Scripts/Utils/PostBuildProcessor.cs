#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using Dacodelaac.DebugUtils;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public class PostBuildProcessor
    {
        [PostProcessBuild]
        public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) return;
            // Get plist
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");;
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
       
            // Get root
            var rootDict = plist.root;
            
            // App transport security settings
            // ironSource
            // var dict = rootDict.CreateDict("NSAppTransportSecurity");
            // dict.SetBoolean("NSAllowsArbitraryLoads", true);
       
            // Change value of LSApplicationQueriesSchemes in Xcode plist
            var querySchemeArray = rootDict.CreateArray("LSApplicationQueriesSchemes");
            querySchemeArray.AddString("fb");
            
            // SkAdNetwork
            // var skAdNetworkFilePath = Path.Combine(Application.dataPath, "SKAdNetwork.xml");
            // if (File.Exists(skAdNetworkFilePath))
            // {
            //     var skAdNetworkArray = rootDict.CreateArray("SKAdNetworkItems");
            //     using (var reader = new StreamReader(skAdNetworkFilePath))
            //     {
            //         while (!reader.EndOfStream)
            //         {
            //             var line = reader.ReadLine();
            //             if (!string.IsNullOrEmpty(line))
            //             {
            //                 line = line.Trim();
            //                 if (line.StartsWith("<string>") && line.EndsWith("</string>"))
            //                 {
            //                     line = line.Replace("<string>", "").Replace("</string>", "");
            //                     if (!string.IsNullOrEmpty(line))
            //                     {
            //                         dict = skAdNetworkArray.AddDict();
            //                         dict.SetString("SKAdNetworkIdentifier", line);
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     Dacoder.LogError("File not found: " + skAdNetworkFilePath);
            // }

            // Universal SKAN Reporting
            // ironSource
            // rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://postbacks-is.com");

            // Admob ID
            // rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-2100064277536286~9285077042");
            
            // ATT
            // rootDict.SetString("NSUserTrackingUsageDescription","This uses device info for more personalized ads and content" );

            //Tiktok
#if TikTok
            rootDict.SetString("TikTokAppID", "7613991345081860112");
#endif
            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }

        [PostProcessBuild]
        public static void ChangeXcodeCapacity(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS) return;
            
            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var mainTarget = proj.GetUnityMainTargetGuid();
            var frameworkTarget = proj.GetUnityFrameworkTargetGuid();
            
            void AddUniqueLinkerFlag(string targetGuid, string flag)
            {
                var existingFlags = proj.GetBuildPropertyForAnyConfig(targetGuid, "OTHER_LDFLAGS");
                if (existingFlags == null || !existingFlags.Contains(flag))
                {
                    proj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", flag);
                }
            }
            
            // disable bitcode
            proj.SetBuildProperty(frameworkTarget, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(proj.TargetGuidByName(PBXProject.GetUnityTestTargetName()), "ENABLE_BITCODE", "NO");
            
            AddUniqueLinkerFlag(frameworkTarget, "-lc++");
            AddUniqueLinkerFlag(mainTarget, "-lc++");
            
            proj.WriteToFile(projPath);
            // capability
            var manager = new ProjectCapabilityManager(projPath, "Entitlements.entitlements", 
                targetGuid: proj.GetUnityMainTargetGuid());

            manager.AddSignInWithApple();
            manager.AddPushNotifications(false);
            manager.AddAssociatedDomains(new string[]{"applinks:anhnguyensb.github.io"});

            manager.WriteToFile();
        }
        
    }
}
#endif