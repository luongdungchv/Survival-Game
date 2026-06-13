using System;
using Dacodelaac.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Dacodelaac.Events
{
    public class BaseEventListener<TEvent, TResponse> : BaseMono, IEventListener
        where TEvent : BaseEvent
        where TResponse : UnityEvent
    {
        [SerializeField] TEvent @event;
        [SerializeField] TResponse response;

        public override void ListenEvents()
        {
            base.ListenEvents();
            @event.AddListener(this);
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            @event.RemoveListener(this);
        }

        public void OnEventRaised(IEvent e)
        {
            response?.Invoke();
        }
    }

    public class BaseEventListener<TType, TEvent, TResponse> : BaseMono, IEventListener<TType>
        where TEvent : BaseEvent<TType>
        where TResponse : UnityEvent<TType>
    {
        [SerializeField] protected TEvent @event;
        [SerializeField] TResponse response;

        public override void ListenEvents()
        {
            base.ListenEvents();
            @event.AddListener(this);
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            @event.RemoveListener(this);
        }

        public virtual void OnEventRaised(TType data)
        {
            response?.Invoke(data);
        }
    }

    public class BaseCombinedEventListener<TEvent, TResponse> : BaseMono, IEventListener
        where TEvent : BaseEvent
        where TResponse : UnityEvent
    {
        [SerializeField] TEvent[] events;
        [SerializeField] TResponse response;

        public override void ListenEvents()
        {
            base.ListenEvents();
            foreach (var @event in events)
            {
                @event.AddListener(this);
            }
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            foreach (var @event in events)
            {
                @event.RemoveListener(this);
            }
        }

        public void OnEventRaised(IEvent e)
        {
            response?.Invoke();
        }
    }

    public class BaseCombinedEventListener<TType, TEvent, TResponse> : BaseMono, IEventListener<TType>
        where TEvent : BaseEvent<TType>
        where TResponse : UnityEvent<TType>
    {
        [SerializeField] TEvent[] events;
        [SerializeField] TResponse response;

        public override void ListenEvents()
        {
            base.ListenEvents();
            foreach (var @event in events)
            {
                @event.AddListener(this);
            }
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            foreach (var @event in events)
            {
                @event.RemoveListener(this);
            }
        }

        public void OnEventRaised(TType data)
        {
            response?.Invoke(data);
        }
    }

    public class BaseConvertibleEventListener<TType, TEvent> : BaseMono, IEventListener<TType>
        where TEvent : BaseEvent<TType>
    {
        [SerializeField] protected TEvent @event;
        [SerializeField] ConvertType convertType;
        [SerializeField, HideInInspector] string format = "{0}";
        [SerializeField, HideInInspector] EventResponse response;
        [SerializeField, HideInInspector] IntegerEventResponse integerResponse;
        [SerializeField, HideInInspector] FloatEventResponse floatResponse;
        [SerializeField, HideInInspector] BooleanEventResponse booleanResponse;
        [SerializeField, HideInInspector] StringEventResponse stringResponse;

        public override void ListenEvents()
        {
            base.ListenEvents();
            @event.AddListener(this);
        }

        public override void StopListenEvents()
        {
            base.StopListenEvents();
            @event.RemoveListener(this);
        }

        public void OnEventRaised(TType data)
        {
            var obj = Convert.ChangeType(data, typeof(TType));
            switch (convertType)
            {
                case ConvertType.None:
                    response.Invoke();
                    break;
                case ConvertType.Integer:
                    integerResponse.Invoke((int) obj);
                    break;
                case ConvertType.Float:
                    floatResponse.Invoke((float) obj);
                    break;
                case ConvertType.Boolean:
                    booleanResponse.Invoke((bool) obj);
                    break;
                case ConvertType.String:
                    stringResponse.Invoke(string.Format(format, obj));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BaseConvertibleEventListener<,>), true)]
    public class BaseConvertibleEventListenerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var convertTypeProperty = serializedObject.FindProperty("convertType");
            var convertType = (ConvertType) convertTypeProperty.enumValueIndex;

            SerializedProperty prop = null;
            switch (convertType)
            {
                case ConvertType.None:
                    prop = serializedObject.FindProperty("response");
                    EditorGUILayout.PropertyField(prop);
                    break;
                case ConvertType.Integer:
                    prop = serializedObject.FindProperty("integerResponse");
                    EditorGUILayout.PropertyField(prop);
                    break;
                case ConvertType.Float:
                    prop = serializedObject.FindProperty("floatResponse");
                    EditorGUILayout.PropertyField(prop);
                    break;
                case ConvertType.Boolean:
                    prop = serializedObject.FindProperty("booleanResponse");
                    EditorGUILayout.PropertyField(prop);
                    break;
                case ConvertType.String:
                    prop = serializedObject.FindProperty("stringResponse");
                    EditorGUILayout.PropertyField(prop);
                    prop = serializedObject.FindProperty("format");
                    EditorGUILayout.PropertyField(prop);
                    break;
                case ConvertType.ShortDouble:
                    prop = serializedObject.FindProperty("shortDoubleResponse");
                    EditorGUILayout.PropertyField(prop);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    public enum ConvertType
    {
        None,
        Integer,
        Float,
        Boolean,
        String,
        ShortDouble,
    }
}