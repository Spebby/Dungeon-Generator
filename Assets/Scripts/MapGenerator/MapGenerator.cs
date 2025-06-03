using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace CMPM146.MapGenerator {
    public class MapGenerator : MonoBehaviour {
        public List<Room> rooms;

        [FormerlySerializedAs("vertical_hallway")]
        public Hallway verticalHallway;

        [FormerlySerializedAs("horizontal_hallway")]
        public Hallway horizontalHallway;

        public Room start;
        public Room target;

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

            _generatedObjects.Add(start.Place(new Vector2Int(0, 0)));
            List<Door>       doors    = start.GetDoors();
            List<Vector2Int> occupied = new();
            occupied.Add(new Vector2Int(0, 0));
            _iterations = 0;
            GenerateWithBacktracking(occupied, doors, 1);
        }

        bool GenerateWithBacktracking(List<Vector2Int> occupied, List<Door> doors, int depth) {
            if (_iterations > THRESHOLD) throw new System.Exception("Iteration limit exceeded");
            return false;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _generatedObjects = new List<GameObject>();
            Generate();
        }

        // Update is called once per frame
        void Update() {
            if (Keyboard.current.gKey.wasPressedThisFrame)
                Generate();
        }
    }
}