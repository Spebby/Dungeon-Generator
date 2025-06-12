using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Exception = System.Exception;


namespace CMPM146.MapGenerator {
    public sealed class MapGenerator : MonoBehaviour {
        [Header("Archetypes")] 
        [SerializeField] string ResourceDirectory = "Rooms";
        [SerializeField, Tooltip("Do not manually assign!")] RoomArchetype[] archetypes;

        [Header ("Generation Settings")]
        [SerializeField] RoomCollection startRooms;
        [SerializeField] RoomCollection target;
        [SerializeField] RoomCollection treasureRooms;

        [SerializeField] Hallway verticalHallway;
        [SerializeField] Hallway horizontalHallway;
        
        // This isn't ideal. It would be nice if we could simply define rules in some sort of config file or
        // a SO and then the map would evaluate the rules there--we could simply just plug in new rules as we like.
        // For endrooms a priority system would be needed.
        [SerializeField, Range(0, 1f)] float TargetRoomDistance   = 1f;
        [SerializeField, Range(0, 1f)] float TreasureRoomDistance = 0.5f;
        
        public int MIN_SIZE = 5;
        public int MAX_SIZE;

        public int DIMENSION_DIFF = 2;

        // set this to a high value when the generator works
        // for debugging it can be helpful to test with few rooms
        // and, say, a threshold of 100 iterations
        public int THRESHOLD;

        [Tooltip("Should the generator try a different seed if the initial attempt fails?")] public bool RETRY;

        [Header("Seeds")]
        [SerializeField] bool useRandomSeed = true;
        [SerializeField] int seed;
        System.Random _rng = new();
        
        // keep the instantiated rooms and hallways here
        readonly List<GameObject> _generatedObjects = new();
        readonly Dictionary<Vector2Int, RoomNode> _roomGraph = new();
        int _iterations;

        readonly Dictionary<Door.Direction, RoomArchetype[]> _roomsByDir = new();

        static readonly Vector2Int STARTING_POS = Vector2Int.zero;

        int _failCount;
        const int FAILSAFE = 100;
        
        void Awake() {
            archetypes = Resources.LoadAll<RoomArchetype>(ResourceDirectory);
            List<RoomArchetype> a = archetypes.ToList();
            archetypes = a.ToArray();
            // ^ this is a silly way of doing it, but I don't really care
            
            foreach (Door.Direction dir in Door.Cardinals) {
                _roomsByDir[dir] = archetypes.Where(archetype => archetype.HasDoorOnSide(dir)).ToArray();
            }

            _failCount = 0;
        }

        public void Generate() {
            // Mixing randoms here seems like bad practice, but I don't think it matters that much.
            if (useRandomSeed) seed = _rng.Next(int.MaxValue);
            int newSeed = seed;

            _rng = new System.Random(newSeed);
            
            // dispose of game objects from previous generation process
            foreach (GameObject go in _generatedObjects) Destroy(go);
            _generatedObjects.Clear();
            _roomGraph.Clear();

            RoomArchetype start = startRooms.GetRandomRoom(_rng);
            _roomGraph[STARTING_POS] = new RoomNode(start, STARTING_POS);
            // Markus's coordinate system can't handle anything outside Q1, this is a quick fix to that before I
            // eventually fix that coordinate system.
            
            // TODO: Start Rooms specifically needs to know what doors they have internally. They probably shouldn't use
            // and archetype.
            ReadOnlySpan<Vector2Int> occupancy = start.GetOccupancy(STARTING_POS);
            Vector2Int[] buf = ArrayPool<Vector2Int>.Shared.Rent(occupancy.Length);
            occupancy.CopyTo(buf);
            
            ReadOnlySpan<Door> initDoors = start.GetDoors(STARTING_POS);
            Door[] doorBuf = ArrayPool<Door>.Shared.Rent(initDoors.Length);
            initDoors.CopyTo(doorBuf.AsSpan(0, initDoors.Length));
            using RentBuffer<Door> doors = new (doorBuf, initDoors.Length);
            using RentBuffer<Vector2Int> occupied = new(buf, occupancy.Length);
            
            _iterations = 0;
            bool result;
            // This is gross but im too tired to make this cleaner rn
            try {
                result = GenerateWithBacktracking(occupied, doors, 1);
            } catch (Exception) {
                if (!RETRY) throw new Exception("Iteration limit exceeded");
                int orgSeed = seed;
                seed = _rng.Next(int.MaxValue);
                Debug.Log($"Seed {orgSeed} failed. Trying {seed}");
                _failCount++;
                if (_failCount == FAILSAFE) throw new Exception("Fail count exceeded.");
                Generate();
                seed = orgSeed;
                return;
            } // This is gross but I can't think of anything better rn
            
            if (!result && !RETRY) throw new Exception($"Map Generation failed after {_iterations} iterations!");
            FinaliseMap();
            _failCount = 0;
        }

        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        bool GenerateWithBacktracking(in RentBuffer<Vector2Int> occupancy, in RentBuffer<Door> doors, int depth) {
            if (_iterations++ > THRESHOLD) throw new Exception("Iteration limit exceeded");
            int doorCount = doors.Count;
            if (doorCount == 0) return ConstraintsCheck(occupancy, depth);

            // pick an entry door
            ref readonly Door entryDoor = ref doors.Buffer[_rng.Next(0, doorCount)];
            Door              match     = entryDoor.GetMatching();
            Vector2Int        offset    = match.GetGridCoordinates();
            
            // rent & fill baseDoors (not including the door we're trying)
            Door[] baseBuf   = ArrayPool<Door>.Shared.Rent(doorCount - 1);
            int    baseCount = 0;
            for (int i = 0; i < doorCount; ++i) {
                Door d = doors.Buffer[i];
                if (d == entryDoor) continue;
                baseBuf[baseCount++] = d;
            }
            using RentBuffer<Door> baseDoors = new(baseBuf, baseCount);
            
            RoomArchetype[] candidates = GetValidRooms(match.GetDirection());
            Span<int>       weights    = stackalloc int[candidates.Length];
            for (int i = 0; i < candidates.Length; i++) {
                weights[i] = candidates[i].Weight;
            }
            // This is a data structure that handles the unique weighted selection for us & keeps allocations low.
            using WeightedBag<RoomArchetype> bag = new(candidates, weights);

            while (bag.TryNext(_rng, out RoomArchetype hopeful)) {
                ReadOnlySpan<Vector2Int> occMap         = hopeful.GetOccupancy(offset);
                int                      occupancyCount = occupancy.Count;
                ReadOnlySpan<Vector2Int> baseOccupancy  = occupancy.Buffer.AsSpan(0, occupancyCount);
                if (!CanFit(occMap, baseOccupancy)) continue;

                int plusCount = hopeful.GetDoorCount();
                if (depth + plusCount + baseCount > MAX_SIZE) continue;

                // rent & build the frontier slice
                Door[] frontierBuf = ArrayPool<Door>.Shared.Rent(baseCount + plusCount - 1);
                baseDoors.Buffer.AsSpan(0, baseCount).CopyTo(frontierBuf);
                // Remove the matching door
                int write = baseCount;
                // This code is intended to remove doors who match the new room's doors.
                // If we don't do this filter step, then all paths that branch out will never reconnect.
                // Currently, this isn't working as intended. Investigate at some point.
                ReadOnlySpan<Door> plusDoors = hopeful.GetDoors(offset);
                for (int i = 0; i < plusCount; i++) {
                    Door plus = plusDoors[i];
                    if (plus == match) continue;
                    /* // This block doesn't currently work correctly, it's just wasted compute.
                    Door mirror = plus.GetMatching();
                    bool replaced = false;
                    for (int j = 0; j < baseCount; ++j) {
                        if (mirror != frontierBuf[j]) continue;
                        frontierBuf[j] = plus;
                        replaced  = true;
                        break;
                    }

                    if (!replaced) */
                    frontierBuf[write++] = plus;
                }
                
                using RentBuffer<Door> frontier = new(frontierBuf, write);

                // rent & build the merged occupancy slice
                int              occLen  = occupancyCount + occMap.Length;
                Vector2Int[]     occBuf  = ArrayPool<Vector2Int>.Shared.Rent(occLen);
                Span<Vector2Int> occSpan = occBuf.AsSpan(0, occLen);
                baseOccupancy.CopyTo(occSpan);
                occMap.CopyTo(occSpan.Slice(occupancyCount));
                using RentBuffer<Vector2Int> occRentBuffer = new(occBuf, occLen);
                
                // recurse
                if (!GenerateWithBacktracking(occRentBuffer, frontier, depth + 1)) continue;

                // Define room relations here for later.
                ReadOnlySpan<Door> temp = hopeful.GetDoors(offset);
                if (!_roomGraph.TryGetValue(offset, out RoomNode node)) {
                    node = new RoomNode(hopeful, offset);
                    foreach (Vector2Int tile in hopeful.GetOccupancy(offset)) {
                        _roomGraph[tile] = node;
                    }
                }

                foreach (Door door in temp) {
                    Vector2Int neighbourPos = door.GridCoordinates + Door.DirToVec(door.GetDirection());
                    if (!_roomGraph.TryGetValue(neighbourPos, out RoomNode neighbour)) continue;
                    node.Connect(neighbour);
                }
                
                return true;
            }
            
            return false;
        }

        bool ConstraintsCheck(RentBuffer<Vector2Int> occupancy, int depth) {
            if (depth < MIN_SIZE) return false;
            // calculate width and height of map
            int minX = occupancy.Buffer[0].x;
            int maxX = occupancy.Buffer[0].x;
            int minY = occupancy.Buffer[0].y;
            int maxY = occupancy.Buffer[0].y;

            for (int i = 1; i < occupancy.Count; i++) {
                Vector2Int t         = occupancy.Buffer[i];
                if (t.x < minX) minX = t.x;
                if (t.x > maxX) maxX = t.x;
                if (t.y < minY) minY = t.y;
                if (t.y > maxY) maxY = t.y;
            }

            int width  = maxX - minX + 1;
            int height = maxY - minY + 1;
            if (DIMENSION_DIFF < Math.Abs(width - height)) return false;

            // There is a visual bug here, where if Target replaces a 2x1 hallway, then it looks like
            // this check failed, even though it did succeed.
            // NOTE: I need to double-check if this actually fixed it, but I made the BFS take into account the
            // "desired size" of the room when trying to find the farthest. Should leave non-1x1s alone.
            return true;
        }
        
        void FinaliseMap() {
            List<Door>                       doors = new();
            HashSet<RoomNode> nodes = new(_roomGraph.Values);
            // Find adequate position for target.
            // Run BFS and find the longest path to a deadened from start, and the node w/ appropriate target.
            List<RoomNode> endrooms = RoomNode.GetEndRooms(_roomGraph[STARTING_POS]);

            ReplaceRoom(endrooms[Mathf.RoundToInt(TargetRoomDistance * (endrooms.Count - 1))], target);
            if (CanSpawnTreasureRoom() && endrooms.Count > 1) {
                ReplaceRoom(endrooms[Mathf.RoundToInt(TreasureRoomDistance * (endrooms.Count - 1))], treasureRooms);
            }
            
            // Instantiate everything
            foreach (RoomNode node in nodes) {
                _generatedObjects.Add(node.Archetype.GetRandomRoom(_rng).Place(node.Coordinates));
                ReadOnlySpan<Door> doorSpan = node.Archetype.GetHallwaySideDoors(node.Coordinates);
                doors.Capacity = doors.Count + doorSpan.Length;
                foreach (Door door in doorSpan) {
                    doors.Add(door);
                }
            }

            GameObject[] hallways = new GameObject[doors.Count];
            for (int i = 0; i < doors.Count; i++) {
                Door door = doors[i];
                hallways[i] = door.IsHorizontal() ? horizontalHallway.Place(door) : verticalHallway.Place(door);
            }
            _generatedObjects.AddRange(hallways);

            return;
            
            void ReplaceRoom(in RoomNode targetRoom, in RoomCollection collection) {
                ReadOnlySpan<Door> targetDoors = targetRoom.Archetype.GetDoors(Vector2Int.zero);

                if (targetDoors.Length != 1) {
                    Debug.LogWarning($"Expected dead-end room to have exactly 1 door, but found {targetDoors.Length}.");
                }

                // Replace the archetype at the farthest node
                Door.Direction exitDir = targetDoors[0].GetDirection();
                targetRoom.Archetype = collection.GetRandomRoomForDirection(_rng, exitDir);
            }
        }

        RoomArchetype[] GetValidRooms(in Door.Direction direction) => _roomsByDir[direction];
        
        static bool CanFit(in ReadOnlySpan<Vector2Int> occMap, in ReadOnlySpan<Vector2Int> occupied) {
            foreach (Vector2Int occ in occMap) {
                foreach (Vector2Int t in occupied) {
                    if (t == occ) return false;
                }
            }

            return true;
        }

        void Start() {
            Generate();
        }

        void Update() {
            if (Keyboard.current.gKey.wasPressedThisFrame) Generate();
        }
        
        bool CanSpawnTreasureRoom() {
            return true;
        }
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    sealed class RoomNode : IEquatable<RoomNode> {
        public RoomArchetype Archetype { get; set; }
        public readonly Vector2Int Coordinates;
        HashSet<RoomNode> Neighbours { get; }

        readonly int _size;
        
        public RoomNode(in RoomArchetype archetype, in Vector2Int coordinates) {
            Archetype   = archetype;
            Coordinates = coordinates;
            Neighbours  = new HashSet<RoomNode>();
            _size        = Archetype.GetOccupancy(Vector2Int.zero).Length;
        }

		public void Connect(in RoomNode other) {
			Neighbours.Add(other);
			other.Neighbours.Add(this);
		}

        public ReadOnlySpan<Vector2Int> GetOccupiedTiles() => Archetype == null ? null : Archetype.GetOccupancy(Coordinates).ToArray();
        
                #region Path Finding
        public static List<RoomNode> FindPath(RoomNode start, RoomNode target) {
            if (start == null || target == null) return null;
            if (start == target) return new List<RoomNode> { start };

            Dictionary<RoomNode, RoomNode> cameFrom = new();
            Queue<RoomNode>                queue    = new();
            HashSet<RoomNode>              visited  = new() { start };

            queue.Enqueue(start);
            while (queue.Count > 0) {
                RoomNode current = queue.Dequeue();

                if (current == target) {
                    // Reconstruct path
                    List<RoomNode> path = new();
                    for (RoomNode node = target; node != null; node = cameFrom.GetValueOrDefault(node)) {
                        path.Add(node);
                    }
                    path.Reverse();
                    return path;
                }

                foreach (RoomNode neighbour in current.Neighbours) {
                    if (!visited.Add(neighbour)) continue;
                    cameFrom[neighbour] = current;
                    queue.Enqueue(neighbour);
                }
            }

            return null;
        }

		public static (RoomNode node, int distance) GetFarthestRoom(RoomNode start, int desiredSize = -1) {
            if (start == null) return (null, -1);

            HashSet<RoomNode>                    visited = new() { start };
            Queue<(RoomNode node, int distance)> queue   = new();
            queue.Enqueue((start, 0));

            RoomNode farthest = null;
            int      maxDist  = -1;

            while (queue.Count > 0) {
                (RoomNode current, int dist) = queue.Dequeue();

                bool isSizeMatch = desiredSize < 0 || current._size == desiredSize;

                if (isSizeMatch && dist > maxDist) {
                    maxDist  = dist;
                    farthest = current;
                }

                foreach (RoomNode neighbour in current.Neighbours) {
                    if (!visited.Add(neighbour)) continue;
                    queue.Enqueue((neighbour, dist + 1));
                }
            }

            return (farthest, maxDist);
        }

        public static List<RoomNode> GetEndRooms(RoomNode start) {
            if (start == null) {
                return new List<RoomNode> {
                    Capacity = 0
                };
            }

            List<RoomNode>    endRooms = new();
            HashSet<RoomNode> visited  = new() { start };
            Queue<RoomNode>   queue    = new();
            queue.Enqueue(start);

            while (queue.Count > 0) {
                RoomNode current = queue.Dequeue();

                if (current.Neighbours.Count == 1 && current._size == 1) {
                    endRooms.Add(current);
                }

                foreach (RoomNode neighbour in current.Neighbours) {
                    if (!visited.Add(neighbour)) continue;
                    queue.Enqueue(neighbour);
                }
            }

            return endRooms;
        }
        #endregion
        
        #region Equatable
        public override bool Equals(object obj) => obj is RoomNode other && Coordinates == other.Coordinates;
        public bool Equals(RoomNode other) => other != null && Coordinates == other.Coordinates;
        public static bool operator ==(RoomNode left, RoomNode right) =>
            ReferenceEquals(left, right) || (left is not null && right is not null && left.Coordinates == right.Coordinates);
        public static bool operator !=(RoomNode left, RoomNode right) => !(left == right);
        
        public override int GetHashCode() => HashCode.Combine(Coordinates);
        #endregion
    }
}
