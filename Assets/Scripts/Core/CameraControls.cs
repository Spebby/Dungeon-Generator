using UnityEngine;
using UnityEngine.InputSystem;


namespace CMPM146.Core {
    public class CameraControls : MonoBehaviour {
        public Vector2 move;
        public float speed;

        void Update() {
            transform.Translate(move.x * Time.deltaTime * speed, move.y * Time.deltaTime * speed, 0, Space.World);
        }

        void OnMove(InputValue value) {
            move = value.Get<Vector2>();
        }
    }
}