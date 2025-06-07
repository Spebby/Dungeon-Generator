using System;
using UnityEngine;


namespace CMPM146.MapGenerator {
    public struct Door {
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

        Vector2Int _coordinates;
        readonly Direction _direction;

        public Door(Vector2Int coordinates, Direction direction) {
            _coordinates = coordinates;
            _direction   = direction;
        }

        public Direction GetDirection() {
            return _direction;
        }

        public Vector2Int GetGridCoordinates() {
            return new Vector2Int(_coordinates.x / Room.GRID_SIZE, _coordinates.y / Room.GRID_SIZE);
        }

        public Vector3 GetLocalCoordinates() => new(_coordinates.x, _coordinates.y, 0);

        public Door GetMatching() {
            int x = _coordinates.x;
            int y = _coordinates.y;
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
            return match._coordinates == other._coordinates && match._direction == other._direction;
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

        public override bool Equals(object obj) =>
            obj is Door other && _direction == other._direction && _coordinates == other._coordinates;

        public override int GetHashCode() => HashCode.Combine(_direction, _coordinates);
    }
}