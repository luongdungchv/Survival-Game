using System.Text;
using Dacodelaac.Core;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Dacodelaac.DebugUtils
{
// #if !DACODER_RELEASE
//     public static class MemoryProfilerStarter
//     {
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//         static void OnAfterSceneLoadRuntimeMethod()
//         {
//             var memoryProfiler = Object.FindObjectOfType<MemoryProfiler>();
//             if (memoryProfiler == null)
//             {
//                 var go = new GameObject("Memory Profiler");
//                 go.AddComponent<MemoryProfiler>();
//             }
//         }
//     }
// #endif

    public class MemoryProfiler : BaseMono
    {
        GUIStyle style;
        StringBuilder stringBuilder;
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.green;
            stringBuilder = new StringBuilder();
        }

        void OnGUI()
        {
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(5, 5, 200, 120), GUIContent.none);
            DrawGUI("Total:\t", Profiler.GetTotalAllocatedMemoryLong(), 0);
            DrawGUI("Reserved:\t", Profiler.GetTotalReservedMemoryLong(), 1);
            DrawGUI("M.Used:\t", Profiler.GetMonoUsedSizeLong(), 2);
            DrawGUI("M.Heap:\t", Profiler.GetMonoHeapSizeLong(), 3);
        }

        void DrawGUI(string s, long value, int line)
        {
            stringBuilder.Clear();
            stringBuilder.Append(s);
            stringBuilder.Append((value / 1048576).ToString());
            GUI.Label(new Rect(10, 10 + line * 30, 200, 30), stringBuilder.ToString(), style);
        }
    }
}