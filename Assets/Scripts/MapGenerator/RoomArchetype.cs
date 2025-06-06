using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace CMPM146.MapGenerator {
    [CreateAssetMenu(fileName = "RoomArchetype", menuName = "MapGenerator/RoomArchetype")]
    public class RoomArchetype : ScriptableObject {
        public List<Room> rooms;
        public int Width  = 1;
        public int Height = 1;
        public bool[] Occupancy = {true};

        public Room[] GetAllRooms() {
            Room[] r = rooms.ToArray();
            foreach (Room room in r) {
                room.Width = Width;
                room.Height = Height;
                room.Occupancy = Occupancy;
            }

            return r;
        }
        
        public Room Get(int i) {
            if (i >= rooms.Count) throw  new IndexOutOfRangeException();
            Room r = rooms[i];
            r.Width = Width;
            r.Height = Height;
            r.Occupancy = Occupancy;
            return r;
        }
        
        public Room GetRandomRoom() {
            int totalWeight = rooms.Sum(r => r.Weight);
            if (totalWeight == 0) throw new InvalidOperationException("All room weights are zero.");

            int pick       = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (Room room in rooms) {
                cumulative += room.Weight;
                if (pick >= cumulative) continue;
                
                room.Weight    = Width;
                room.Height    = Height;
                room.Occupancy = Occupancy;
                return room;
            }

            throw new Exception("Weighted selection failed unexpectedly.");
        }

        public bool HasDoorOnSide(Door.Direction direction) {
            if (rooms.Count == 0) throw new NullReferenceException("Room Archetype has no rooms defined!");
            Room r = rooms[0];
            return r.HasDoorOnSide(direction);
        }
        
        
        internal void ResizeGrid(int newWidth, int newHeight) {
            if (newWidth < 1 || newHeight < 1) throw new ArgumentOutOfRangeException();
            bool[] newOccupancy = new bool[newWidth * newHeight];
            for (int y = 0; y < Mathf.Min(Height, newHeight); y++) {
                for (int x = 0; x < Mathf.Min(Width, newWidth); x++) {
                    int oldIndex = y * Width + x;
                    int newIndex = y * newWidth + x;
                    newOccupancy[newIndex] = Occupancy != null && oldIndex < Occupancy.Length && Occupancy[oldIndex];
                }
            }

            Width     = newWidth;
            Height    = newHeight;
            Occupancy = newOccupancy;
        }
    }
}