using System;
using UnityEngine;


namespace CMPM146.MapGenerator {
    public struct Door : IEquatable<Door> {
        public enum Direction {
            NORTH,
            EAST,
            SOUTH,
            WEST
        }

        public static Direction GetMatchingDirection(Direction direction) {
            return direction switch {
                Direction.NORTH => Direction.SOUTH,
                Direction.EAST  => Direction.WEST,
                Direction.SOUTH => Direction.NORTH,
                Direction.WEST  => Direction.EAST,
                _               => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        internal Vector2Int Coordinates;
        readonly Direction _direction;

        public Door(Vector2Int coordinates, Direction direction) {
            Coordinates = coordinates;
            _direction  = direction;
        }

        public Direction GetDirection() {
            return _direction;
        }

        public Vector2Int GetGridCoordinates() {
            return new Vector2Int(Coordinates.x / Room.GRID_SIZE, Coordinates.y / Room.GRID_SIZE);
        }

        public readonly Vector3 GetLocalCoordinates() => new(Coordinates.x, Coordinates.y, 0);

        public Door GetMatching() {
            int x = Coordinates.x;
            int y = Coordinates.y;
            // The +- 2 is b/c we have the padding of 1 unit for each room. Doors are guaranteed to touch that boundary
            // so we need to bump it over to the "correct" spot on the other side of the boundary.
            return _direction switch {
                Direction.EAST  => new Door(new Vector2Int(x + 2, y), Direction.WEST),
                Direction.WEST  => new Door(new Vector2Int(x - 2, y), Direction.EAST),
                Direction.NORTH => new Door(new Vector2Int(x, y + 2), Direction.SOUTH),
                Direction.SOUTH => new Door(new Vector2Int(x, y - 2), Direction.NORTH),
                _               => throw new Exception("Unknown direction!")
            };
        }

        public bool IsMatching(Door other) {
            Door match = GetMatching();
            return match.Coordinates == other.Coordinates && match._direction == other._direction;
        }

        public Direction GetMatchingDirection() => GetMatchingDirection(_direction);

        public override string ToString() {
            return GetGridCoordinates() + " " + _direction;
        }

        public bool IsVertical() {
            return _direction is Direction.NORTH or Direction.SOUTH;
        }

        public bool IsHorizontal() {
            return _direction is Direction.EAST or Direction.WEST;
        }

        // Mostest correct would also use door direction, but coordinates is the only neccesary information here.
        public override bool Equals(object obj) => obj is Door other && Coordinates == other.Coordinates;
        public bool Equals(Door other) => Coordinates.Equals(other.Coordinates);
        public static bool operator ==(Door left, Door right) => left.Coordinates == right.Coordinates;
        public static bool operator !=(Door left, Door right) => left.Coordinates != right.Coordinates;
        
        public override int GetHashCode() => HashCode.Combine(_direction, Coordinates);
    }
}