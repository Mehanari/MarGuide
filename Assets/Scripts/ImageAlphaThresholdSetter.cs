using UnityEngine;
using UnityEngine.UI;

public class ImageAlphaThresholdSetter : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private float _threshold;

    private void Awake()
    {
        _image.alphaHitTestMinimumThreshold = _threshold;
    }
}
