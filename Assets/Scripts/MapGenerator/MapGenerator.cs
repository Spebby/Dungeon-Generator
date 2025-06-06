using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace CMPM146.MapGenerator {
    public class MapGenerator : MonoBehaviour {
        public List<Room> rooms;

        [FormerlySerializedAs("vertical_hallway")]
        public Hallway verticalHallway;

        [FormerlySerializedAs("horizontal_hallway")]
        public Hallway horizontalHallway;

        public RoomArchetype startRooms;
        public Room target;

        public int MIN_SIZE = 5;
        
        public RoomArchetype[] archetypes;

        void Awake() {
            archetypes = Resources.LoadAll<RoomArchetype>("Rooms");
            List<RoomArchetype> a = archetypes.ToList();
            a.Remove(startRooms);
            archetypes = a.ToArray();
        }
        
        // Constraint: How big should the dungeon be at most
        // this will limit the run time (~10 is a good value 
        // during development, later you'll want to set it to 
        // something a bit higher, like 25-30)
        public int MAX_SIZE;

        // set this to a high value when the generator works
        // for debugging it can be helpful to test with few rooms
        // and, say, a threshold of 100 iterations
        public int THRESHOLD;

        // keep the instantiated rooms and hallways here 
        List<GameObject> _generatedObjects;

        int _iterations;

        public void Generate() {
            // dispose of game objects from previous generation process
            foreach (GameObject go in _generatedObjects) {
                Destroy(go);
            }

            _generatedObjects.Clear();

            Room start = startRooms.GetRandomRoom();
            _generatedObjects.Add(start.Place(new Vector2Int(0, 0)));
            List<Door>       doors    = start.GetDoors();
            List<Vector2Int> occupied = new() { new Vector2Int(0, 0) };
            _iterations = 0;
            bool result = GenerateWithBacktracking(occupied, doors, 1);
            if (!result) throw new Exception("Map Generation failed!");
        }

        bool GenerateWithBacktracking(in List<Vector2Int> occupied, in List<Door> doors, int depth) {
            if (_iterations > THRESHOLD) throw new Exception("Iteration limit exceeded");
            if (doors.Count == 0 && rooms.Count < MIN_SIZE) return false;

            Door       door       = doors[Random.Range(0, doors.Count)];
            Door       match      = door.GetMatching();
            List<Room> validRooms = GetValidRooms(match.GetDirection());
            
            List<Door> newDoors = new(doors);
            newDoors.RemoveSwapBack(door);

            while (validRooms.Count > 0) {
                Room hopeful = validRooms[Random.Range(0, validRooms.Count)];
                validRooms.RemoveSwapBack(hopeful);
                List<Vector2Int> occMap = hopeful.GetOccupancy(match.GetGridCoordinates());
                if (!CanFit(occMap, occupied)) continue;
                // TODO: the above code needs to be revised to check each door on the side we're trying to place.
                // this isn't a problem for 1x1 but will need adjustments for nxm
                
                // Attempt to place the room
                List<Vector2Int> newOccupied = new(occupied);
                newOccupied.AddRange(occMap);
                // Add new doors to the door list. Remove the door mirror.
                List<Door> plusDoors = hopeful.GetDoors();
                plusDoors.RemoveSwapBack(match);
                newDoors.AddRange(plusDoors);
                
                if (!GenerateWithBacktracking(newOccupied, newDoors, depth + 1)) continue;
                // Then we're save to place!
                hopeful.Place(match.GetGridCoordinates());
                // TODO: revisit this such that we place at the *correct* door. Some sort of offset may be needed. 
                break;
            }
            
            return true;
        }

        
        List<Room> GetValidRooms(in Door.Direction direction) {
            List<Room> valid = new();
            foreach (RoomArchetype archetype in archetypes) {
                if (archetype.HasDoorOnSide(direction)) {
                    valid.AddRange(archetype.GetAllRooms());
                }
            }
            return valid;
        }

        static bool CanFit(in List<Vector2Int> occMap, in List<Vector2Int> occupied) {
            foreach (Vector2Int occ in occMap) {
                if (occupied.Contains(occ)) return false;
            }

            return true;
        }

        void Start() {
            _generatedObjects = new List<GameObject>();
            Generate();
        }

        void Update() {
            if (Keyboard.current.gKey.wasPressedThisFrame) Generate();
        }
    }
}