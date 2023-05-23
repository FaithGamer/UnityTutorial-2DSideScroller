using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NotThisAndNotNullTilingRule", menuName = "MyAssets/NotThisAndNotNullTilingRule")]
public class NotThisAndNotNull : RuleTile<NotThisAndNotNull.Neighbor>
{
    
    public bool customField;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int NotThisAndNotNull = 3;
        public const int Null = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.NotThisAndNotNull:
                {
                    return tile != null && tile != this;
                };
            case Neighbor.Null:
                {
                    return tile == null;
                };

        }
        return base.RuleMatch(neighbor, tile);
    }
}