using System.Collections;
using UnityEngine;


namespace CMPM146.Core {
    public class CoroutineManager : MonoBehaviour {
        public static CoroutineManager Instance;

        void Start() {
            Instance = this;
        }
        
        public void Run(IEnumerator coroutine) {
            StartCoroutine(coroutine);
        }
    }
}