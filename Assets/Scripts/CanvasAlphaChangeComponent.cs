using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CanvasAlphaChangeComponent : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _changeTime;
    [SerializeField] private UnityEvent _onComplete;
    private float _defaultAlpha;
    private float _elapsedTime;

    private void Awake()
    {
        _defaultAlpha = _canvasGroup.alpha;
    }

    public void ChangeAlphaToNew(float alpha)
    {

        StopAllCoroutines();
        StartCoroutine(OnAlphaChange(_canvasGroup.alpha, alpha));
    }

    public void CahangeAlphaToDefault()
    {
        StopAllCoroutines();
        StartCoroutine(OnAlphaChange(_canvasGroup.alpha, _defaultAlpha));
    }

    private IEnumerator OnAlphaChange(float startAlpha, float endAlpha)
    {
        _elapsedTime = 0f;
        while (_elapsedTime < _changeTime)
        {
            float percentage = _elapsedTime / _changeTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, percentage);
            _elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _onComplete?.Invoke();
        _canvasGroup.alpha = endAlpha;
        _elapsedTime = 0f;
    }
}
