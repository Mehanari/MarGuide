using UnityEngine;

public class GameTile : MonoBehaviour
{
    private GameTileView _view;

    public Vector2Int Coordinates { get; set; }

    public TileType Type { get; set; }

    public int Cost 
    {
        get
        {
            switch (Type)
            {
                case TileType.Mountain:
                    return 2;
                case TileType.Ground:
                    return 1;
                case TileType.Acid:
                    return 0;
                default:
                    return 0;
            }
        }
    }

    public bool Walkable => Type != TileType.Acid;

    public void SetView(GameTileView view, int viewRotation)
    {
        if (_view != null)
        {
            Destroy(_view.gameObject);
        }
        _view = Instantiate(view);
        _view.transform.SetParent(transform, false);
        _view.transform.localPosition = new Vector3(0, 0, 0);
        _view.transform.localEulerAngles = new Vector3(0, viewRotation, 0);
    }

    public void SetColor(Color color)
    {
        _view.SetColor(color);
    }

    public void SetDefaultColor()
    {
        _view.SetDefaultColor();
    }
}
