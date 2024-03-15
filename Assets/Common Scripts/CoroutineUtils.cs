using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DL.Utils
{
    public static class CoroutineUtils
    {
        public static Coroutine Invoke(MonoBehaviour invoker, UnityAction callback, float delay, bool ignoreTimescale = false)
        {
            IEnumerator Delay()
            {
                if (ignoreTimescale) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
                callback();
            }
            return invoker.StartCoroutine(Delay());
        }
        public static Coroutine Invoke(MonoBehaviour invoker, UnityAction callback, UnityAction<float> onUpdate, float delay, bool ignoreTimescale = false)
        {
            IEnumerator Delay()
            {
                float elapsed = 0;
                while (elapsed < 1)
                {
                    var deltaTime = ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
                    elapsed += deltaTime / delay;
                    onUpdate?.Invoke(elapsed);
                    yield return null;
                }
                callback();
            }
            return invoker.StartCoroutine(Delay());
        }
        public static Coroutine SetInterval(MonoBehaviour invoker, UnityAction callback, float interval, int loop = -1, bool ignoreTimescale = false)
        {
            IEnumerator IEPeriod()
            {
                float count = 0;
                while (loop < 0 || count < loop)
                {
                    count++;
                    callback?.Invoke();
                    if (ignoreTimescale)
                        yield return new WaitForSecondsRealtime(interval);
                    else
                        yield return new WaitForSeconds(interval);
                }
            }
            return invoker.StartCoroutine(IEPeriod());
        }
        public static Coroutine SetInterval(MonoBehaviour invoker, UnityAction<int> callback, float interval, int loop = -1, bool ignoreTimescale = false)
        {
            IEnumerator IEPeriod()
            {
                int count = 0;
                while (loop < 0 || count < loop)
                {
                    callback?.Invoke(count);
                    count++;
                    if (ignoreTimescale)
                        yield return new WaitForSecondsRealtime(interval);
                    else
                        yield return new WaitForSeconds(interval);
                }
            }
            return invoker.StartCoroutine(IEPeriod());
        }
        public static Coroutine SetInterval(MonoBehaviour invoker, UnityAction update, UnityAction onComplete, float interval, int loop = -1, bool ignoreTimescale = false)
        {
            IEnumerator IEPeriod()
            {
                float count = 0;
                while (loop < 0 || count < loop)
                {
                    count++;
                    update?.Invoke();
                    if (ignoreTimescale)
                        yield return new WaitForSecondsRealtime(interval);
                    else
                        yield return new WaitForSeconds(interval);
                }
                onComplete?.Invoke();
            }
            return invoker.StartCoroutine(IEPeriod());
        }

        public static Coroutine SetInterval(MonoBehaviour invoker, UnityAction<int> update, UnityAction onComplete, float interval, int loop = -1, bool ignoreTimescale = false)
        {
            IEnumerator IEPeriod()
            {
                int count = 0;
                while (loop < 0 || count < loop)
                {
                    update?.Invoke(count);
                    count++;
                    if (ignoreTimescale)
                        yield return new WaitForSecondsRealtime(interval);
                    else
                        yield return new WaitForSeconds(interval);
                }
                onComplete?.Invoke();
            }
            return invoker.StartCoroutine(IEPeriod());
        }
        public static Coroutine AnimateTextNumber(this TMP_Text text, int targetValue, float duration)
        {
            var isNumber = int.TryParse(text.text, out var num);
            if (!isNumber) return null;
            IEnumerator IEAnimate()
            {
                var diff = Mathf.Abs(targetValue - num);
                var t = 0f;
                while (t < 1)
                {
                    var val = (int)Mathf.Lerp(num, targetValue, t);
                    text.text = val.ToString();
                    t += Time.deltaTime / duration;
                    yield return null;
                }
                text.text = targetValue.ToString();
            }
            return text.StartCoroutine(IEAnimate());
        }
    }
}