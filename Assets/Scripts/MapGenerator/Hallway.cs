using System;
using UnityEngine;


namespace CMPM146.MapGenerator {
    public class Hallway : MonoBehaviour {
        public GameObject Place(Door door) {
            Vector2Int     where = door.GetGridCoordinates();
            Hallway newRoom =
                Instantiate(this, new Vector3(where.x * Room.GRID_SIZE, where.y * Room.GRID_SIZE),
                            Quaternion.identity);
            return newRoom.gameObject;
        }
    }
}