using UnityEngine;
using UnityEditor;

namespace Array2DEditor
{
    [CustomPropertyDrawer(typeof(Array2DTransforms))]
    public class Array2DTransformsDrawer : Array2DObjectDrawer<Transform>
    {
        protected override Vector2Int GetDefaultCellSizeValue() => new Vector2Int(96, 16);
    }
}
