using CMPM146.DamageSystem;
using CMPM146.Movement;
using CMPM146.Spells;
using CMPM146.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace CMPM146.Core {
    public class PlayerController : MonoBehaviour {
        public Hittable HP;
        public HealthBar healthui;
        public ManaBar manaui;

        public SpellCaster Spellcaster;
        public SpellUI spellui;
        public int speed;
        public Unit unit;
        public bool autoplay;

        [FormerlySerializedAs("current_waypoint")]
        public Vector3 currentWaypoint;

        public Transform[] waypoints;
        [FormerlySerializedAs("ponder_until")] public float ponderUntil;
        [FormerlySerializedAs("last_cast")] public float lastCast;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            unit                           =  GetComponent<Unit>();
            GameManager.Instance.Player    =  gameObject;
            EventBus.Instance.OnEnemyDeath += EnemyDeathHandler;
        }

        public void StartLevel() {
            Spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
            StartCoroutine(Spellcaster.ManaRegeneration());

            HP         =  new Hittable(100, Hittable.Team.PLAYER, gameObject);
            HP.OnDeath += Die;
            HP.team    =  Hittable.Team.PLAYER;

            // tell UI elements what to show
            healthui.SetHealth(HP);
            manaui.SetSpellCaster(Spellcaster);
            spellui.SetSpell(Spellcaster.Spell);
            currentWaypoint = waypoints[0].position;
        }

        // Update is called once per frame
        void Update() {
            if (GameManager.Instance.State != GameManager.GameState.INWAVE) return;
            if (autoplay) {
                Vector3 direction = currentWaypoint - transform.position;
                if (direction.magnitude > 0.5f) {
                    unit.movement = direction.normalized * speed;
                    ponderUntil   = Time.time + Random.value * 3;
                } else {
                    unit.movement = new Vector3(0, 0, 0);
                    if (ponderUntil < Time.time) {
                        if (Random.value < 0.5f)
                            ponderUntil = Time.time + Random.value * 3;
                        else
                            currentWaypoint = waypoints[Random.Range(0, waypoints.Length)].position;
                    }
                }

                if (lastCast + 1.75f < Time.time) {
                    GameObject target = GameManager.Instance.GetClosestEnemy(transform.position);
                    if (target != null) {
                        if ((target.transform.position - transform.position).magnitude < 5)
                            StartCoroutine(Spellcaster.Cast(transform.position, target.transform.position));
                        else {
                            StartCoroutine(Spellcaster.Cast(transform.position,
                                                            target.transform.position
                                                          + (Vector3)Random.insideUnitCircle * 5));
                        }
                    } else {
                        StartCoroutine(Spellcaster.Cast(transform.position,
                                                        transform.position + (Vector3)Random.insideUnitCircle));
                    }

                    lastCast = Time.time;
                }
            }

            if (GameManager.Instance.CurrentTime() >= GameManager.Instance.WinTime())
                GameManager.Instance.State = GameManager.GameState.PLAYERWIN;
        }

        void OnAttack(InputValue value) {
            if (autoplay) return;
            if (GameManager.Instance.State == GameManager.GameState.PREGAME
             || GameManager.Instance.State == GameManager.GameState.GAMEOVER) return;
            Vector2 mouseScreen = Mouse.current.position.value;
            Vector3 mouseWorld  = Camera.main.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0;
            StartCoroutine(Spellcaster.Cast(transform.position, mouseWorld));
        }

        void OnMove(InputValue value) {
            if (autoplay) return;
            if (GameManager.Instance.State == GameManager.GameState.PREGAME
             || GameManager.Instance.State == GameManager.GameState.GAMEOVER) return;
            unit.movement = value.Get<Vector2>() * speed;
        }

        void Die() {
            GameManager.Instance.State = GameManager.GameState.GAMEOVER;
        }

        void EnemyDeathHandler(EnemyController which) {
            HP.Heal(Random.Range(1, 4));
        }
    }
}