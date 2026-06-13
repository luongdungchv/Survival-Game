using Dacodelaac.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SampleAnim : BaseMono
{
    [SerializeField] public AnimationClip clip;

    public override void Initialize()
    {
        Sample(0);
    }

    public void Sample(int frame)
    {
        if (clip != null)
        {
            var t = frame / clip.frameRate;
            clip.SampleAnimation(gameObject, t);
        }
    }

    public void Sample(float rate)
    {
        if (clip != null)
        {
            clip.SampleAnimation(gameObject, rate * clip.length);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SampleAnim))]
public class SampleAnimEditor : Editor
{
    SampleAnim sampleAnim;
    int frame;

    void OnEnable()
    {
        sampleAnim = target as SampleAnim;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (Application.isPlaying || sampleAnim.clip == null) return;
        
        var frames = (int) (sampleAnim.clip.length * sampleAnim.clip.frameRate);
        frame = EditorGUILayout.IntSlider("Frame", frame, 0, frames);
        sampleAnim.Sample(frame);
    }
}
#endif