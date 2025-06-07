using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


namespace CMPM146.MapGenerator {
    public class MapGenerator : MonoBehaviour {
        [Header ("Archetypes")]
        public RoomArchetype[] archetypes;
        
        [Header ("Generation Settings")]
        public Hallway verticalHallway;
        public Hallway horizontalHallway;

        public RoomArchetype startRooms;
        public Room target;

        public int MIN_SIZE = 5;
        
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
        List<Room> _generatedRooms;
        int _iterations;
        
        void Awake() {
            archetypes = Resources.LoadAll<RoomArchetype>("Rooms");
            List<RoomArchetype> a = archetypes.ToList();
            a.Remove(startRooms);
            archetypes = a.ToArray();
        }
        
        public void Generate() {
            // dispose of game objects from previous generation process
            foreach (GameObject go in _generatedObjects) {
                Destroy(go);
            }

            _generatedObjects.Clear();

            Room start = startRooms.GetRandomRoom();
            // Markus's coordinate system can't handle anything outside Q1, this is a quick fix to that before I
            // eventually fix that coordinate system.
            _generatedObjects.Add(start.Place(new Vector2Int(21, 21)));
            List<Door>       doors    = start.GetDoors(new Vector2Int(21, 21));
            List<Vector2Int> occupied = new() { new Vector2Int(21, 21) };
            _iterations = 0;
            bool result = GenerateWithBacktracking(occupied, doors, 1);
            if (!result) throw new Exception("Map Generation failed!");
            PlaceHallways(_generatedRooms);
        }

        bool GenerateWithBacktracking(in List<Vector2Int> occupied, in List<Door> doors, int depth) {
            if (_iterations > THRESHOLD) throw new Exception("Iteration limit exceeded");
            _iterations++;
            if (doors.Count == 0) return depth >= MIN_SIZE;

            Door       door       = doors[Random.Range(0, doors.Count)];
            Door       match      = door.GetMatching();
            Vector2Int offset     = match.GetGridCoordinates();
            List<Room> validRooms = GetValidRooms(match.GetDirection());
            
            List<Door> newDoors = new(doors);
            newDoors.RemoveSwapBack(door);

            while (validRooms.Count > 0) {
                Room hopeful = RoomArchetype.GetRandomRoom(validRooms);
                validRooms.RemoveSwapBack(hopeful);
                List<Vector2Int> occMap = hopeful.GetOccupancy(match.GetGridCoordinates());
                if (!CanFit(occMap, occupied)) continue;
                // TODO: the above code needs to be revised to check each door on the side we're trying to place.
                // this isn't a problem for 1x1 but will need adjustments for nxm
                
                // Attempt to place the room
                // Add new doors to the door list. Remove the door mirror.
                // TODO... adapt this to handle the case where a 2x2 room w/ 2 exits on same cardinal are being is
                // being tested. Need to make sure we test for both upper & lower door placement.
                List<Door> plusDoors = hopeful.GetDoors(offset);
                // Heuristic to avoid exploring bad seeds too deeply. We assume each door leads to a new room.
                // This overestimates slightly, this doesn't account for doors that connect to the same space.
                // (placed rooms) + (frontier) > MAX_SIZE -- No, there isn't an off-by one error. 
                if ((depth + plusDoors.Count + newDoors.Count) > MAX_SIZE) continue;
                
                plusDoors.RemoveSwapBack(match);
                newDoors.AddRange(plusDoors);
                
                List<Vector2Int> newOccupied = new(occupied);
                newOccupied.AddRange(occMap);
                
                if (!GenerateWithBacktracking(newOccupied, newDoors, depth + 1)) continue;
                // Then we're safe to place!
                GameObject obj = hopeful.Place(match.GetGridCoordinates());
                _generatedObjects.Add(obj);
                _generatedRooms.Add(obj.GetComponent<Room>());
                // TODO: revisit this such that we place at the *correct* door. Some sort of offset may be needed. 
                break;
            }
            
            return false;
        }

        void PlaceHallways(in List<Room> rooms) {
            List<Door> doors = new();
            foreach (Room room in rooms) {
                doors.AddRange(room.GetHallwaySideDoors(room.GetPivotCoordinates()));
            }

            GameObject[] hallways = new GameObject[doors.Count];
            for (int i = 0; i < doors.Count; i++) {
                Door door = doors[i];
                hallways[i] = door.IsHorizontal() ? horizontalHallway.Place(door) : verticalHallway.Place(door);
            }
            _generatedObjects.AddRange(hallways);
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