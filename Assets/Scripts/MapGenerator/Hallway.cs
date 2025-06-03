using System;
using UnityEngine;


namespace CMPM146.MapGenerator {
    public class Hallway : MonoBehaviour {
        public GameObject Place(Door door) {
            Vector2Int     where = door.GetGridCoordinates();
            Door.Direction dir   = door.GetDirection();
            switch (dir) {
                case Door.Direction.EAST:
                    where.x++;
                    break;
                case Door.Direction.NORTH:
                    where.y++;
                    break;
                case Door.Direction.SOUTH:
                case Door.Direction.WEST:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int dx                      = 0;
            int dy                      = 0;
            if (door.IsHorizontal()) dx = -1;
            if (door.IsVertical()) dy   = -1;
            Hallway newRoom =
                Instantiate(this, new Vector3(where.x * Room.GRID_SIZE + dx, where.y * Room.GRID_SIZE + dy),
                            Quaternion.identity);
            return newRoom.gameObject;
        }
    }
}