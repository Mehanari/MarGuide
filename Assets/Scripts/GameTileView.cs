using UnityEngine;

public class GameTileView : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _renderers;
    private Color[] _defaultColors;
    private Material[] _materials;

    private void OnEnable()
    {
        _materials = new Material[_renderers.Length];
        _defaultColors= new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _materials[i] = _renderers[i].material;
            _defaultColors[i] = _materials[i].color;
        }
    }

    public void SetColor(Color color)
    {
        foreach (var material in _materials)
        {
            material.color = color;
        }
    }

    public void SetDefaultColor()
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].color = _defaultColors[i];
        }
    }
}
