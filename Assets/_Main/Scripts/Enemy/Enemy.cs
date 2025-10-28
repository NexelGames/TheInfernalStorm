using System.Collections;
using UnityEngine;
using Game; // для Game.HealthBar

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour {
    [Header("Attack Settings")]
    [SerializeField] private float damagePerHit = 10f;        // Урон за один удар
    [SerializeField] private float attackInterval = 1.0f;     // Пауза между ударами (и перед первым)
    [SerializeField] private string playerTag = "Player";     // Тег игрока

    private HealthBar playerHealth;   // Ссылку передаёт спавнер
    private Coroutine attackRoutine;
    private bool touchingPlayer;

    /// <summary>
    /// Спавнер вызывает сразу после Instantiate
    /// </summary>
    public void Initialize(HealthBar health) {
        playerHealth = health;
    }

    private void Reset() {
        // Удобно работать триггером: события не зависят от физики
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // ===== ТРИГГЕРЫ =====
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag(playerTag)) return;
        if (playerHealth == null) return;

        touchingPlayer = true;
        StartAttackLoopIfNeeded();
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag(playerTag)) return;

        touchingPlayer = false;
        StopAttackLoop();
    }

    // ===== КОЛЛИЖН (если используешь не-триггер) =====
    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.CompareTag(playerTag)) return;
        if (playerHealth == null) return;

        touchingPlayer = true;
        StartAttackLoopIfNeeded();
    }

    private void OnCollisionExit(Collision collision) {
        if (!collision.collider.CompareTag(playerTag)) return;

        touchingPlayer = false;
        StopAttackLoop();
    }

    private void OnDisable() => StopAttackLoop();

    // ===== ЛОГИКА АТАК =====
    private void StartAttackLoopIfNeeded() {
        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackCoroutine());
    }

    private void StopAttackLoop() {
        if (attackRoutine != null) {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator AttackCoroutine() {
        var wait = new WaitForSeconds(Mathf.Max(attackInterval, 0.05f));

        while (touchingPlayer && playerHealth != null) {
            // ЖДЁМ перед каждым ударом (первый урон — после ожидания)
            yield return wait;

            if (!touchingPlayer || playerHealth == null)
                break;

            playerHealth.TakeDamage(damagePerHit);
        }

        attackRoutine = null;
    }
}
