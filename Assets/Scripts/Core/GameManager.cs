using System.Collections.Generic;
using System.Linq;
using CMPM146.Spells;
using CMPM146.SpriteManagement;
using UnityEngine;


namespace CMPM146.Core {
    public class GameManager {
        public enum GameState {
            PREGAME,
            INWAVE,
            WAVEEND,
            COUNTDOWN,
            GAMEOVER,
            PLAYERWIN
        }

        public GameState State;

        public int Countdown;
        static GameManager _theInstance;

        public static GameManager Instance {
            get { return _theInstance ??= new GameManager(); }
        }

        public GameObject Player;
        public float StartTime;

        public ProjectileManager ProjectileManager;
        public SpellIconManager SpellIconManager;
        public EnemySpriteManager EnemySpriteManager;
        public PlayerSpriteManager PlayerSpriteManager;
        public RelicIconManager RelicIconManager;

        List<GameObject> _enemies;
        public int EnemyCount => _enemies.Count;

        public void AddEnemy(GameObject enemy) {
            _enemies.Add(enemy);
        }

        public void RemoveEnemy(GameObject enemy) {
            _enemies.Remove(enemy);
        }

        public GameObject GetClosestEnemy(Vector3 point) {
            if (_enemies == null || _enemies.Count == 0) return null;
            if (_enemies.Count == 1) return _enemies[0];
            return _enemies.Aggregate((a, b) => (a.transform.position - point).sqrMagnitude
                                              < (b.transform.position - point).sqrMagnitude
                                          ? a
                                          : b);
        }

        public GameObject GetClosestOtherEnemy(GameObject self) {
            Vector3 point = self.transform.position;
            if (_enemies == null || _enemies.Count < 2) return null;
            return _enemies.FindAll((a) => a != self).Aggregate((a, b) => (a.transform.position - point).sqrMagnitude
                                                                        < (b.transform.position - point).sqrMagnitude
                                                                    ? a
                                                                    : b);
        }

        public List<GameObject> GetEnemiesInRange(Vector3 point, float distance) {
            if (_enemies == null || _enemies.Count == 0) return null;
            return _enemies.FindAll((a) => (a.transform.position - point).magnitude <= distance);
        }

        GameManager() {
            _enemies = new List<GameObject>();
        }

        public float WinTime() {
            return 8 * 60;
        }

        public float CurrentTime() {
            return Time.time - StartTime;
        }
    }
}