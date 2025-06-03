using System.Collections;
using CMPM146.AI;
using CMPM146.Core;
using CMPM146.DamageSystem;
using CMPM146.Enemies;
using CMPM146.Movement;
using CMPM146.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace CMPM146.Levels {
    public class EnemySpawner : MonoBehaviour {
        [FormerlySerializedAs("level_selector")]
        public Image levelSelector;

        public GameObject button;
        public GameObject enemy;
        public SpawnPoint[] SpawnPoints;

        void Start() {
            GameObject selector = Instantiate(button, levelSelector.transform);
            selector.transform.localPosition                        = new Vector3(0, 0);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel("Start");
        }

        public void StartLevel(string levelname) {
            levelSelector.gameObject.SetActive(false);

            // this is not nice: we should not have to be required to tell the player directly that the level is starting
            GameManager.Instance.Player.GetComponent<PlayerController>().StartLevel();
            GameManager.Instance.StartTime = Time.time;
            StartCoroutine(SpawnWave());
        }

        public void NextWave() {
            StartCoroutine(SpawnWave());
        }

        IEnumerator SpawnWave() {
            GameManager.Instance.State     = GameManager.GameState.COUNTDOWN;
            GameManager.Instance.Countdown = 3;
            for (int i = 3; i > 0; i--) {
                yield return new WaitForSeconds(1);
                GameManager.Instance.Countdown--;
            }

            GameManager.Instance.State = GameManager.GameState.INWAVE;

            StartCoroutine(SpawnZombies());
            StartCoroutine(SpawnSkeletons());
            StartCoroutine(SpawnWarlocks());

            yield return new WaitWhile(() => GameManager.Instance.Player.GetComponent<PlayerController>().HP.HP > 0);
            GameManager.Instance.State = GameManager.GameState.GAMEOVER;
        }

        IEnumerator SpawnZombies() {
            while (GameManager.Instance.State == GameManager.GameState.INWAVE) {
                SpawnPoint spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                Vector2    offset     = Random.insideUnitCircle * 1.8f;

                Vector3    initialPosition = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);
                GameObject newEnemy        = Instantiate(enemy, initialPosition, Quaternion.identity);

                newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.EnemySpriteManager.Get(0);
                EnemyController en = newEnemy.GetComponent<EnemyController>();
                en.AddAction("attack", new EnemyAttack(3, 1.75f, 5, 0.7f));
                en.monster  = "zombie";
                en.Behavior = BehaviorBuilder.MakeTree(en);
                en.HP       = new Hittable(50, Hittable.Team.MONSTERS, newEnemy);
                en.speed    = 3;
                GameManager.Instance.AddEnemy(newEnemy);
                yield return new WaitForSeconds(8f);
            }
        }

        IEnumerator SpawnSkeletons() {
            yield return new WaitForSeconds(12f);
            while (GameManager.Instance.State == GameManager.GameState.INWAVE) {
                SpawnPoint spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                Vector2    offset     = Random.insideUnitCircle * 1.8f;

                Vector3    initialPosition = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);
                GameObject newEnemy        = Instantiate(enemy, initialPosition, Quaternion.identity);

                newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.EnemySpriteManager.Get(1);
                EnemyController en = newEnemy.GetComponent<EnemyController>();
                en.AddAction("attack", new EnemyAttack(4, 1.75f, 12, 2.75f));
                en.monster  = "skeleton";
                en.Behavior = BehaviorBuilder.MakeTree(en);

                en.HP    = new Hittable(35, Hittable.Team.MONSTERS, newEnemy);
                en.speed = 6;
                GameManager.Instance.AddEnemy(newEnemy);
                yield return new WaitForSeconds(21f);
            }
        }

        IEnumerator SpawnWarlocks() {
            yield return new WaitForSeconds(6f);
            while (GameManager.Instance.State == GameManager.GameState.INWAVE) {
                SpawnPoint spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                Vector2    offset     = Random.insideUnitCircle * 1.8f;

                Vector3    initialPosition = spawnPoint.transform.position + new Vector3(offset.x, offset.y, 0);
                GameObject newEnemy        = Instantiate(enemy, initialPosition, Quaternion.identity);

                newEnemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.EnemySpriteManager.Get(19);
                EnemyController en = newEnemy.GetComponent<EnemyController>();
                en.AddAction("attack", new EnemyAttack(5, 6, 1, 0.5f));
                en.AddAction("heal", new EnemyHeal(10, 5, 15));
                en.AddAction("buff", new EnemyBuff(8, 5, 3, 8));
                en.AddAction("permabuff", new EnemyBuff(20, 5, 1));
                en.AddEffect("noheal", 1);
                en.monster  = "warlock";
                en.Behavior = BehaviorBuilder.MakeTree(en);
                en.HP       = new Hittable(20, Hittable.Team.MONSTERS, newEnemy);
                en.speed    = 3;
                GameManager.Instance.AddEnemy(newEnemy);
                yield return new WaitForSeconds(28f);
            }
        }
    }
}