using UnityEngine;
using UnityEngine.UI;

public class UIFrameLoading : MonoBehaviour
{
    [SerializeField] private Image _slider;
    [SerializeField] private float _fillTime = 2.0f;   
    [SerializeField] private AnimationCurve _curve;    

    private bool _isRunning;
    private float _t;

    private const float Max_Fake = 0.9f;

    private void Awake()
    {
        if (_curve == null)
            _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        _slider.fillAmount = 0f;
    }

    private void OnEnable()
    {
        ManagerLoaderScene.OnAdditionalWorkRunningChanged += HandleRunningChanged;
    }

    private void OnDisable()
    {
        ManagerLoaderScene.OnAdditionalWorkRunningChanged -= HandleRunningChanged;
    }

    private void HandleRunningChanged(bool isRunning)
    {
        _isRunning = isRunning;

        if (isRunning)
        {            
            _t = 0f;
            _slider.fillAmount = 0f;
        }
        else
        {                        
            _slider.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        _t += Time.deltaTime / _fillTime;
        float t01 = Mathf.Clamp01(_t);              // 0 ~ 1
        float eased01 = _curve.Evaluate(t01);       // 0 ~ 1 (곡선 적용)
        
        float target = Mathf.Lerp(0f, Max_Fake, eased01);
        
        if (target < _slider.fillAmount)
            target = _slider.fillAmount;

        _slider.fillAmount = target;
    }
}
