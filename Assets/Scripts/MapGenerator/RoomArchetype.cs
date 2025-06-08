using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;


[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
namespace CMPM146.MapGenerator {
    [CreateAssetMenu(fileName = "RoomArchetype", menuName = "MapGenerator/RoomArchetype")]
    public class RoomArchetype : ScriptableObject {
        public List<Room> rooms;
        public int Width  = 1;
        public int Height = 1;
        public bool[] Occupancy = {true};

        bool _initialized;
        
        void Init() {
            if (_initialized) return;
            foreach (Room room in rooms) {
                room.Width = Width;
                room.Height = Height;
                room.Occupancy = Occupancy;
            }
            _initialized = true;
        }
        
        public Room[] GetAllRooms() {
            Init();
            return rooms.ToArray();
        }
        
        public Room Get(int i) {
            if (i >= rooms.Count) throw  new IndexOutOfRangeException();
            Init();
            Room r = rooms[i];
            r.Width = Width;
            r.Height = Height;
            r.Occupancy = Occupancy;
            return r;
        }

        public Room GetRandomRoom(in System.Random rng) {
            Init();
            return GetRandomRoom(rooms, rng);
        }

        public static Room GetRandomRoom(in List<Room> collection, in System.Random rng) {
            int totalWeight = collection.Sum(room => room.Weight);
            if (totalWeight == 0) throw new InvalidOperationException("All room weights are zero.");

            int pick  = rng.Next(totalWeight);
            int accum = 0;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < collection.Count; i++) {
                Room room = collection[i];
                accum += room.Weight;
                if (pick < accum) return room;
            }

            return collection[^1]; // fallback (shouldn’t happen)
        }

        public bool HasDoorOnSide(Door.Direction direction) {
            if (rooms.Count == 0) throw new NullReferenceException("Room Archetype has no rooms defined!");
            Init();
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