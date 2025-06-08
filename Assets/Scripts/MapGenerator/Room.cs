using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


namespace CMPM146.MapGenerator {
    public sealed class Room : MonoBehaviour {
        public const int GRID_SIZE = 12;
        public Tilemap tiles;
        [FormerlySerializedAs("weight"), SerializeField] internal int Weight;

        internal int Width = 1;
        internal int Height = 1;
        internal bool[] Occupancy = new bool[1];

        Door[] _doorsCache;
        Door[] _doorBuf;
        Vector2Int[] _occBuf;
        
        public Vector2Int GetGridSize() => new(Width, Height);

        public ReadOnlySpan<Vector2Int> GetOccupancy(in Vector2Int offset) {
            // This cache so we don't have to re-allocate
            if (_occBuf == null || _occBuf.Length < Occupancy.Length)
                _occBuf = new Vector2Int[Occupancy.Length];

            for (int i = 0; i < Occupancy.Length; i++) {
                if (!Occupancy[i]) continue;
                _occBuf[i] = new Vector2Int(
                    (i % Width) + offset.x,
                    (i / Width) + offset.y
                );
            }
            
            return _occBuf.AsSpan(0, _occBuf.Length);
        }

        public Vector2Int GetPivotCoordinates() => new((int)transform.position.x / GRID_SIZE, (int)transform.position.y / GRID_SIZE);

        // todo: refactor
        public List<Vector2Int> GetGridCoordinates(Vector2Int offset) {
            List<Vector2Int> coordinates = new();
            for (int x = 0; x < Width; ++x) {
                for (int y = 0; y < Height; ++y) {
                    coordinates.Add(new Vector2Int(x, y) + offset);
                }
            }

            return coordinates;
        }

        bool IsDoor(int dx, int dy) {
            int  x    = tiles.cellBounds.min.x + dx;
            int  y    = tiles.cellBounds.min.y + dy;
            Tile tile = tiles.GetTile<Tile>(new Vector3Int(x, y, 0));
            return tile.colliderType == Tile.ColliderType.None;
        }

        
        // Todo: refactor
        public List<Door> GetHallwaySideDoors(in Vector2Int offset) {
            List<Door> doors  = new();
            int        fWidth = (Width * GRID_SIZE) - 1, fHeight = (Height * GRID_SIZE) - 1;
            Vector2Int bump   = offset * GRID_SIZE;
            for (int x = 0; x < fWidth; ++x) {
                if (IsDoor(x, fHeight - 1)) doors.Add(new Door(new Vector2Int(x + bump.x, fHeight - 1 + bump.y), Door.Direction.NORTH));
            }

            for (int y = 0; y < fHeight; ++y) {
                if (IsDoor(fWidth - 1, y)) doors.Add(new Door(new Vector2Int(fWidth - 1 + bump.x, y + bump.y), Door.Direction.EAST));
            }

            return doors; 
        }

        internal int GetDoorCount() {
            if (_doorsCache == null) {
                GetDoors(Vector2Int.zero);
            }
            
            return _doorsCache!.Length;
        }

        internal ReadOnlySpan<Door> GetDoors(in Vector2Int offset) {
            if (_doorsCache != null) return OffsetCache(offset);
            
            // Calculate doors if not cached.
            List<Door> doors = new();
            int fWidth = (Width * GRID_SIZE) - 1, fHeight = (Height * GRID_SIZE) - 1;
            for (int x = 0; x < fWidth; ++x) {
                if (IsDoor(x, 0)) doors.Add(new Door(new Vector2Int(x, 0), Door.Direction.SOUTH));
                if (IsDoor(x, fHeight - 1)) doors.Add(new Door(new Vector2Int(x, fHeight - 1), Door.Direction.NORTH));
            }

            for (int y = 0; y < fHeight; ++y) {
                if (IsDoor(0, y)) doors.Add(new Door(new Vector2Int(0, y), Door.Direction.WEST));
                if (IsDoor(fWidth - 1, y)) doors.Add(new Door(new Vector2Int(fWidth - 1, y), Door.Direction.EAST));
            }

            // Then set cache & get offset.
            _doorsCache = doors.ToArray();
            _doorBuf    = new Door[_doorsCache.Length];
            return OffsetCache(offset);
        }

        ReadOnlySpan<Door> OffsetCache(in Vector2Int offset) {
            Vector2Int adjOffset = offset * GRID_SIZE;
            for (int i = 0; i < _doorsCache.Length; i++) {
                Door d = _doorsCache[i];
                _doorBuf[i] = new Door(d.Coordinates + adjOffset, d.GetDirection());
            }
            return _doorBuf.AsSpan();
        }

        public bool HasDoorOnSide(Door.Direction direction) {
            if (_doorsCache == null) GetDoors(Vector2Int.zero);
            for (int i = 0; i < _doorsCache!.Length; i++) {
                if (_doorsCache[i].GetDirection() == direction) return true;
            }

            return false;
        }

        public GameObject Place(Vector2Int where) {
            Room newRoom = Instantiate(this, new Vector3(where.x * GRID_SIZE, where.y * GRID_SIZE), Quaternion.identity);
            return newRoom.gameObject;
        }

        #if UNITY_EDITOR
        void OnDrawGizmos() {
            Vector3    transformPosition = gameObject.transform.position;
            Vector2Int offset            = new((int)transformPosition.x, (int)transformPosition.y);
            ReadOnlySpan<Door> doors     = GetDoors(offset);

            Gizmos.color = Color.red;
            for (int i =  0; i <  doors.Length; ++i) {
                Vector3 v = doors[i].GetLocalCoordinates();
                Gizmos.DrawSphere(new Vector3(v.x + 0.5f, v.y + 0.5f), 0.5f);
            }
            
            // Will have to update this when we do non-rectangular rooms.
            {
                Gizmos.color   = Color.magenta;
                Vector3    dim = new(Width * GRID_SIZE - 1, Height * GRID_SIZE - 1);
                Gizmos.DrawWireCube((dim * 0.5f) + transformPosition, new Vector3(dim.x, dim.y, 0));
            }
        }
        #endif
    }
}