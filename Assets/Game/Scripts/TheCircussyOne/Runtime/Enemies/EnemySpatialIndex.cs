using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class EnemySpatialIndex
    {
        private const float MinimumCellSize = 0.1f;

        private readonly Dictionary<long, List<int>> cells = new();
        private readonly Stack<List<int>> cellPool = new();
        private readonly List<List<int>> activeCellLists = new();

        private float cellSize = 1f;

        public int ActiveCellCount => activeCellLists.Count;

        public void Rebuild(IReadOnlyList<EnemyRuntime> enemies, float requestedCellSize)
        {
            ClearCells();
            cellSize = Mathf.Max(MinimumCellSize, requestedCellSize);
            if (enemies == null)
            {
                return;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyRuntime enemy = enemies[i];
                if (!IsIndexable(enemy))
                {
                    continue;
                }

                long key = KeyFor(enemy.Position);
                if (!cells.TryGetValue(key, out List<int> indices))
                {
                    indices = GetCellList();
                    cells.Add(key, indices);
                }

                indices.Add(i);
            }
        }

        public void CollectNearbyIndices(Vector3 position, List<int> results)
        {
            if (results == null)
            {
                return;
            }

            results.Clear();
            Vector2Int cell = CellFor(position);
            for (int z = cell.y - 1; z <= cell.y + 1; z++)
            {
                for (int x = cell.x - 1; x <= cell.x + 1; x++)
                {
                    if (cells.TryGetValue(KeyFor(x, z), out List<int> indices))
                    {
                        results.AddRange(indices);
                    }
                }
            }
        }

        private void ClearCells()
        {
            for (int i = 0; i < activeCellLists.Count; i++)
            {
                activeCellLists[i].Clear();
                cellPool.Push(activeCellLists[i]);
            }

            activeCellLists.Clear();
            cells.Clear();
        }

        private List<int> GetCellList()
        {
            List<int> list = cellPool.Count > 0 ? cellPool.Pop() : new List<int>(8);
            activeCellLists.Add(list);
            return list;
        }

        private Vector2Int CellFor(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.z / cellSize));
        }

        private long KeyFor(Vector3 position)
        {
            Vector2Int cell = CellFor(position);
            return KeyFor(cell.x, cell.y);
        }

        private static long KeyFor(int x, int z)
        {
            return ((long)x << 32) ^ (uint)z;
        }

        private static bool IsIndexable(EnemyRuntime enemy)
        {
            return enemy != null
                && !enemy.IsDead
                && enemy.View != null
                && enemy.View.IsActive;
        }
    }
}
