using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class EnemySpawner : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform player;                 // Ссылка на игрока
    [SerializeField] private HealthBar playerHealth;           // Ссылка на HP игрока
    [SerializeField] private UndergroundEnemy enemyPrefab;     // Префаб врага (включает Enemy)

    [Header("Spawn settings")]
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float minDistanceFromPlayer = 5f;
    [SerializeField] private float groundCheckHeight = 50f;
    [SerializeField] private float undergroundDepth = 2.5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Waves / limits")]
    [SerializeField] private int initialCount = 3;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private int maxAlive = 10;

    private readonly List<UndergroundEnemy> alive = new();

    private void Start() {
        // авто-настройка
        if (player == null) {
            var found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (playerHealth == null && player != null)
            playerHealth = player.GetComponentInChildren<HealthBar>();

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop() {
        for (int i = 0; i < initialCount; i++)
            SpawnOne();

        while (enabled) {
            yield return new WaitForSeconds(spawnInterval);
            SpawnOne();
        }
    }

    private void SpawnOne() {
        if (player == null || playerHealth == null) return;
        if (alive.Count >= maxAlive) return;

        if (TryGetSpawnPoint(out var spawnPos, out var groundY)) {
            var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyObj.Initialize(player, groundY);

            // Передаём ссылку на здоровье игрока в Enemy (на том же объекте)
            var enemyLogic = enemyObj.GetComponent<Enemy>();
            if (enemyLogic != null)
                enemyLogic.Initialize(playerHealth);

            enemyObj.OnDied += HandleEnemyDied;
            alive.Add(enemyObj);
        }
    }

    private void HandleEnemyDied(UndergroundEnemy e) {
        if (e == null) return;
        e.OnDied -= HandleEnemyDied;
        alive.Remove(e);
    }

    private bool TryGetSpawnPoint(out Vector3 spawnPos, out float groundY) {
        for (int i = 0; i < 20; i++) {
            Vector2 rnd = Random.insideUnitCircle * spawnRadius;
            Vector3 top = new Vector3(player.position.x + rnd.x, player.position.y + groundCheckHeight, player.position.z + rnd.y);

            if (Vector2.Distance(new Vector2(player.position.x, player.position.z),
                                 new Vector2(top.x, top.z)) < minDistanceFromPlayer)
                continue;

            if (Physics.Raycast(top, Vector3.down, out RaycastHit hit, groundCheckHeight * 2f, groundMask, QueryTriggerInteraction.Ignore)) {
                groundY = hit.point.y;
                spawnPos = new Vector3(hit.point.x, groundY - undergroundDepth, hit.point.z);
                return true;
            }
        }

        groundY = 0f;
        spawnPos = Vector3.zero;
        return false;
    }
}
