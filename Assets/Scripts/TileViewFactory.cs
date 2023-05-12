using System.Collections.Generic;
using UnityEngine;

public class TileViewFactory : MonoBehaviour
{
    [SerializeField] private List<GameTileView> _mountainViews;
    [SerializeField] private List<GameTileView> _acidViews;
    [SerializeField] private List<GameTileView> _groundViews;
    [SerializeField] private List<GameTileView> _borderViews;

    public GameTileView GetView(TileType type)
    {
        switch (type)
        {
            case TileType.Acid:
                return GetRandomFrom(_acidViews);
            case TileType.Mountain:
                return GetRandomFrom(_mountainViews);
            case TileType.Ground:
                return GetRandomFrom(_groundViews);
            case TileType.Border:
                return GetRandomFrom(_borderViews);
            default:
                return GetRandomFrom(_groundViews);
        }
    }

    private GameTileView GetRandomFrom(List<GameTileView> views) 
    {
        int index = Random.Range(0, views.Count);
        return views[index];
    }
}
