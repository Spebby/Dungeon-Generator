using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace CMPM146.MapGenerator {
    public sealed class MapGenerator : MonoBehaviour {
        [Header("Archetypes")] 
        [SerializeField] string ResourceDirectory = "Rooms";
        [SerializeField, Tooltip("Do not manually assign!")] RoomArchetype[] archetypes;
        
        [Header ("Generation Settings")]
        [SerializeField] Hallway verticalHallway;
        [SerializeField] Hallway horizontalHallway;

        [SerializeField] RoomArchetype startRooms;
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

        [Header("Misc Settings")]
        [SerializeField] bool useRandomSeed = true;
        [SerializeField] int seed = 0;
        System.Random _rng = new();
        
        // keep the instantiated rooms and hallways here
        List<GameObject> _generatedObjects;
        int _iterations;

        readonly Dictionary<Door.Direction, Room[]> _roomsByDir = new();

        void Awake() {
            archetypes = Resources.LoadAll<RoomArchetype>(ResourceDirectory);
            List<RoomArchetype> a = archetypes.ToList();
            a.Remove(startRooms);
            archetypes = a.ToArray();
            // ^ this is a silly way of doing it but i dont really care
            
            
            Door.Direction[] dirs = {
                Door.Direction.NORTH,
                Door.Direction.SOUTH,
                Door.Direction.EAST,
                Door.Direction.WEST
            };
            
            foreach (Door.Direction dir in dirs) {
                List<Room> valid = new();
                foreach (RoomArchetype archetype in archetypes) {
                    if (archetype.HasDoorOnSide(dir)) {
                        valid.AddRange(archetype.GetAllRooms());
                    }
                }
                _roomsByDir[dir] = valid.ToArray();
            }
        }

        public void Generate() {
            // Mixing randoms here seems like bad practice but I don't think it matters that much.
            if (useRandomSeed) seed = UnityEngine.Random.Range(0, int.MaxValue);
            _rng = new System.Random(seed);
            
            // dispose of game objects from previous generation process
            foreach (GameObject go in _generatedObjects) Destroy(go);
            _generatedObjects.Clear();

            Room start = startRooms.GetRandomRoom(_rng);
            // Markus's coordinate system can't handle anything outside Q1, this is a quick fix to that before I
            // eventually fix that coordinate system.
            Vector2Int startOffset = new(21, 21);
            _generatedObjects.Add(start.Place(startOffset));
            
            ReadOnlySpan<Vector2Int> occupancy = start.GetOccupancy(startOffset);
            Vector2Int[] buf = ArrayPool<Vector2Int>.Shared.Rent(occupancy.Length);
            occupancy.CopyTo(buf);
            
            ReadOnlySpan<Door> initDoors = start.GetDoors(startOffset);
            Door[] doorBuf = ArrayPool<Door>.Shared.Rent(initDoors.Length);
            initDoors.CopyTo(doorBuf.AsSpan(0, initDoors.Length));
            using Slice<Door> doors = new (doorBuf, initDoors.Length);
            using Slice<Vector2Int> occupied = new(buf, occupancy.Length);
            
            _iterations = 0;
            bool result = GenerateWithBacktracking(occupied, doors, 1);
            if (!result) throw new Exception("Map Generation failed!");
            PlaceHallways();
        }

        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        bool GenerateWithBacktracking(in Slice<Vector2Int> occupancy, in Slice<Door> doors, int depth) {
            if (_iterations++ > THRESHOLD) throw new Exception("Iteration limit exceeded");
            if (doors.Count == 0) return depth >= MIN_SIZE;

            // pick an entry door
            Door       entryDoor  = doors.Buffer[_rng.Next(0, doors.Count)];
            Door       match   = entryDoor.GetMatching();
            Vector2Int offset     = match.GetGridCoordinates();
            Room[] candidates     = GetValidRooms(match.GetDirection());
            Span<int> weights     = stackalloc int[candidates.Length];
            for (int i = 0; i < candidates.Length; i++) {
                weights[i] = candidates[i].Weight;
            }
            
            // This is a data structure that handles the unique weighted selection for us & keeps allocations low.
            using WeightedBag<Room> bag = new(candidates, weights);
            
            // rent & fill baseDoors (not including the door we're trying)
            Door[] baseBuf      = ArrayPool<Door>.Shared.Rent(doors.Count - 1);
            int    baseCount    = 0;
            for (int i = 0; i < doors.Count; ++i) {
                Door d = doors.Buffer[i];
                if (d == entryDoor) continue;
                baseBuf[baseCount++] = d;
            }

            using Slice<Door> baseDoors = new(baseBuf, baseCount);

            while (bag.TryNext(_rng, out Room hopeful)) {
                ReadOnlySpan<Vector2Int> occMap = hopeful.GetOccupancy(match.GetGridCoordinates());
                if (!CanFit(occMap, occupancy.Buffer.AsSpan(0, occupancy.Count))) continue;

                if (depth + hopeful.GetDoorCount() + baseCount > MAX_SIZE) continue;
                ReadOnlySpan<Door> plusDoors = hopeful.GetDoors(offset);

                // rent & build the frontier slice
                Door[] frontierBuf = ArrayPool<Door>.Shared.Rent(baseCount + plusDoors.Length - 1);
                baseDoors.Buffer.AsSpan(0, baseCount).CopyTo(frontierBuf);
                // Remove the matching door
                int write = baseCount;
                for (int i = 0; i < plusDoors.Length; i++) {
                    Door t = plusDoors[i];
                    if (t == match) continue;
                    frontierBuf[write++] = t;
                }

                using Slice<Door> frontier = new(frontierBuf, write);

                // rent & build the merged occupancy slice
                int          occLen = occupancy.Count + occMap.Length;
                Vector2Int[] occBuf = ArrayPool<Vector2Int>.Shared.Rent(occLen);
                Array.Copy(occupancy.Buffer, 0, occBuf, 0, occupancy.Count);
                occMap.CopyTo(occBuf.AsSpan(occupancy.Count, occMap.Length));
                using Slice<Vector2Int> occSlice = new(occBuf, occLen);

                // recurse
                if (!GenerateWithBacktracking(occSlice, frontier, depth + 1)) continue;

                // we succeeded—place the room and return
                GameObject obj = hopeful.Place(match.GetGridCoordinates());
                _generatedObjects.Add(obj);
                return true;
            }

            return false;
        }

        void PlaceHallways() {
            List<Room> rooms = _generatedObjects.Select(go => go.GetComponent<Room>()).ToList();
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

        Room[] GetValidRooms(in Door.Direction direction) => _roomsByDir[direction];
        
        static bool CanFit(in ReadOnlySpan<Vector2Int> occMap, in ReadOnlySpan<Vector2Int> occupied) {
            foreach (Vector2Int occ in occMap) {
                foreach (Vector2Int t in occupied) {
                    if (t == occ) return false;
                }
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