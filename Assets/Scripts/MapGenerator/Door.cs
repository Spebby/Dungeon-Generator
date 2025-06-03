using UnityEngine;


namespace CMPM146.MapGenerator {
    public class Door {
        public enum Direction {
            NORTH,
            EAST,
            SOUTH,
            WEST
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

        public Door GetMatching() {
            return _direction switch {
                Direction.EAST  => new Door(_coordinates + new Vector2Int(2, 0), Direction.WEST),
                Direction.WEST  => new Door(_coordinates + new Vector2Int(-2, 0), Direction.EAST),
                Direction.NORTH => new Door(_coordinates + new Vector2Int(0, 2), Direction.SOUTH),
                Direction.SOUTH => new Door(_coordinates + new Vector2Int(0, -2), Direction.NORTH),
                _               => null
            };
        }

        public bool IsMatching(Door other) {
            Door match = GetMatching();
            return match._coordinates == other._coordinates && match._direction == other._direction;
        }

        public Direction GetMatchingDirection() {
            return _direction switch {
                Direction.EAST  => Direction.WEST,
                Direction.WEST  => Direction.EAST,
                Direction.NORTH => Direction.SOUTH,
                Direction.SOUTH => Direction.NORTH,
                _               => Direction.NORTH
            };
        }

        public override string ToString() {
            return GetGridCoordinates() + " " + _direction;
        }

        public bool IsVertical() {
            return _direction is Direction.NORTH or Direction.SOUTH;
        }

        public bool IsHorizontal() {
            return _direction is Direction.EAST or Direction.WEST;
        }
    }
}