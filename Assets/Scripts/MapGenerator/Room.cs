using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;


[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
namespace CMPM146.MapGenerator {
    public class Room : MonoBehaviour {
        public const int GRID_SIZE = 12;
        public Tilemap tiles;
        [FormerlySerializedAs("weight"), SerializeField] internal int Weight;

        internal int Width = 1;
        internal int Height = 1;
        internal bool[] Occupancy = new bool[1];
        
        (int, int) GetSize() {
            return (tiles.cellBounds.max.x - tiles.cellBounds.min.x, tiles.cellBounds.max.y - tiles.cellBounds.min.y);
        }

        public Vector2Int GetGridSize() {
            (int w, int h) = GetSize();
            return new Vector2Int((w + 1) / GRID_SIZE, (h + 1) / GRID_SIZE);
        }

        public List<Vector2Int> GetOccupancy(Vector2Int? offset = null) {
            offset ??= Vector2Int.zero;
            List<Vector2Int> occupied = new();
            for (int i = 0; i < Occupancy.Length; i++) {
                int x = (i % Width) + offset.Value.x;
                int y = (i / Width) + offset.Value.y;
                if (Occupancy[i]) {
                    occupied.Add(new Vector2Int(x, y));
                }
            }
            return occupied;
        }
        
        public List<Vector2Int> GetGridCoordinates(Vector2Int offset) {
            List<Vector2Int> coordinates = new();
            (int width, int height) = GetSize();
            for (int x = 0; x < (width + 1) / GRID_SIZE; ++x) {
                for (int y = 0; y < (height + 1) / GRID_SIZE; ++y) {
                    coordinates.Add(new Vector2Int(x, y) + offset);
                }
            }

            return coordinates;
        }

        protected bool IsDoor(int dx, int dy) {
            int  x    = tiles.cellBounds.min.x + dx;
            int  y    = tiles.cellBounds.min.y + dy;
            Tile tile = tiles.GetTile<Tile>(new Vector3Int(x, y, 0));
            return tile.colliderType == Tile.ColliderType.None;
        }

        public List<Door> GetDoors() {
            return GetDoors(new Vector2Int(0, 0));
        }

        public List<Door> GetDoors(Vector2Int offset) {
            List<Door> doors = new();
            (int width, int height) = GetSize();
            for (int x = 0; x < width; ++x) {
                if (IsDoor(x, 0)) doors.Add(new Door(new Vector2Int(x, 0) + offset * GRID_SIZE, Door.Direction.SOUTH));
                if (IsDoor(x, height - 1))
                    doors.Add(new Door(new Vector2Int(x, height - 1) + offset * GRID_SIZE, Door.Direction.NORTH));
            }

            for (int y = 0; y < height; ++y) {
                if (IsDoor(0, y)) doors.Add(new Door(new Vector2Int(0, y) + offset * GRID_SIZE, Door.Direction.WEST));
                if (IsDoor(width - 1, y))
                    doors.Add(new Door(new Vector2Int(width - 1, y) + offset * GRID_SIZE, Door.Direction.EAST));
            }

            return doors;
        }

        public bool HasDoorOnSide(Door.Direction direction) {
            List<Door> doors = GetDoors();
            return doors.Any(d => d.GetDirection() == direction);
        }

        public GameObject Place(Vector2Int where) {
            Room newRoom = Instantiate(this, new Vector3(where.x * GRID_SIZE, where.y * GRID_SIZE),
                                       Quaternion.identity);
            return newRoom.gameObject;
        }

        void OnDrawGizmos() {
            Vector3    transformPosition = gameObject.transform.position;
            Vector2Int offset            = new((int)transformPosition.x, (int)transformPosition.y);
            List<Door> doors             = GetDoors();

            {
                Gizmos.color = Color.red;
                foreach (Vector3 n in doors.Select(door => door.GetLocalCoordinates()).Select(v => new Vector3(v.x + offset.x + 0.5f, v.y + offset.y + 0.5f, 0))) {
                    Gizmos.DrawSphere(n, 0.5f);
                }
            }

            // Will have to update this when we do non-rectangular rooms.
            {
                Gizmos.color   = Color.magenta;
                Vector2Int _   = GetGridSize();
                Vector3    dim = new(_.x * GRID_SIZE - 1, _.y * GRID_SIZE - 1);
                Gizmos.DrawWireCube((dim * 0.5f) + transformPosition, new Vector3(dim.x, dim.y, 0));
            }
        }
    }
}