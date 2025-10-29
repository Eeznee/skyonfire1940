using JetBrains.Annotations;
using System.Linq;
using UnityEngine;

namespace Array2DEditor
{
    [System.Serializable]
    public class Array2DTransforms : Array2D<Transform>
    {
        [SerializeField]
        CellRowTransform[] cells = new CellRowTransform[2];

        protected override CellRow<Transform> GetCellRow(int idx)
        {
            return cells[idx];
        }

        /// <inheritdoc cref="Array2D{T}.Clone"/>
        protected override Array2D<Transform> Clone(CellRow<Transform>[] cells)
        {
            return new Array2DTransforms() { cells = cells.Select(r => new CellRowTransform(r)).ToArray() };
        }
    }

    [System.Serializable]
    public class CellRowTransform : CellRow<Transform>
    {
        /// <inheritdoc/>
        [UsedImplicitly]
        public CellRowTransform() { }

        /// <inheritdoc/>
        public CellRowTransform(CellRow<Transform> row)
            : base(row) { }
    }
}