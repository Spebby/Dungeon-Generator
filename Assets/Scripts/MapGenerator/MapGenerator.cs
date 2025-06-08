using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace CMPM146.MapGenerator {
    public class MapGenerator : MonoBehaviour {
        [Header("Archetypes")]
        public RoomArchetype[] archetypes;

        [Header("Generation Settings")]
        public Hallway verticalHallway;
        public Hallway horizontalHallway;

        public RoomArchetype startRooms;
        public Room target;

        public int MIN_SIZE = 5;
        public int MAX_SIZE = 30;
        public int THRESHOLD;

        List<GameObject> _generatedObjects;
        List<Room> _generatedRooms;
        int _iterations;

        bool _targetPlaced;

        void Awake() {
            archetypes = Resources.LoadAll<RoomArchetype>("Rooms");
            List<RoomArchetype> a = archetypes.ToList();
            a.Remove(startRooms);
            archetypes = a.ToArray();
        }

        public void Generate() {
            foreach (GameObject go in _generatedObjects) {
                Destroy(go);
            }

            _generatedObjects.Clear();
            _generatedRooms = new List<Room>();
            _targetPlaced = false;

            Room start = startRooms.GetRandomRoom();
            _generatedObjects.Add(start.Place(new Vector2Int(21, 21)));
            _generatedRooms.Add(start);

            List<Door> doors = start.GetDoors(new Vector2Int(21, 21));
            List<Vector2Int> occupied = new() { new Vector2Int(21, 21) };

            _iterations = 0;
            bool result = GenerateWithBacktracking(occupied, doors, 1);
            if (!result || !_targetPlaced) throw new Exception("Map Generation failed or no target room placed!");

            PlaceHallways(_generatedRooms);
        }

        bool GenerateWithBacktracking(in List<Vector2Int> occupied, in List<Door> doors, int depth) {
            if (_iterations > THRESHOLD) throw new Exception("Iteration limit exceeded");
            _iterations++;
            if (doors.Count == 0) return depth >= MIN_SIZE;

            Door door = doors[Random.Range(0, doors.Count)];
            Door match = door.GetMatching();
            Vector2Int offset = match.GetGridCoordinates();

            bool forceTarget = !_targetPlaced && depth >= MIN_SIZE;

            List<Room> validRooms = GetValidRooms(match.GetDirection(), forceTarget);
            List<Door> newDoors = new(doors);
            newDoors.Remove(door);

            while (validRooms.Count > 0) {
                int index = Random.Range(0, validRooms.Count);
                Room hopeful = validRooms[index];
                validRooms.RemoveAt(index);

                List<Vector2Int> occMap = hopeful.GetOccupancy(match.GetGridCoordinates());
                if (!CanFit(occMap, occupied)) continue;

                List<Door> plusDoors = hopeful.GetDoors(offset);
                if ((depth + plusDoors.Count + newDoors.Count) > MAX_SIZE) continue;

                plusDoors.Remove(match);
                List<Door> updatedDoors = new List<Door>(newDoors);
                updatedDoors.AddRange(plusDoors);

                List<Vector2Int> newOccupied = new(occupied);
                newOccupied.AddRange(occMap);

                if (!GenerateWithBacktracking(newOccupied, updatedDoors, depth + 1)) continue;

                GameObject obj = hopeful.Place(match.GetGridCoordinates());
                _generatedObjects.Add(obj);
                _generatedRooms.Add(obj.GetComponent<Room>());

                if (hopeful == target) _targetPlaced = true;

                return true;
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

        List<Room> GetValidRooms(in Door.Direction direction, bool forceTarget) {
            List<Room> valid = new();

            if (forceTarget) {
                if (target.HasDoorOnSide(direction)) {
                    valid.Add(target);
                }
            } else {
                foreach (RoomArchetype archetype in archetypes) {
                    if (archetype.HasDoorOnSide(direction)) {
                        valid.AddRange(archetype.GetAllRooms());
                    }
                }
                if (!_targetPlaced && target.HasDoorOnSide(direction)) {
                    valid.Add(target);
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
