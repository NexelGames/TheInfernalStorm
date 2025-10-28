using System.Collections;
using UnityEngine;
using Game; // ��� Game.HealthBar

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour {
    [Header("Attack Settings")]
    [SerializeField] private float damagePerHit = 10f;        // ���� �� ���� ����
    [SerializeField] private float attackInterval = 1.0f;     // ����� ����� ������� (� ����� ������)
    [SerializeField] private string playerTag = "Player";     // ��� ������

    private HealthBar playerHealth;   // ������ ������� �������
    private Coroutine attackRoutine;
    private bool touchingPlayer;

    /// <summary>
    /// ������� �������� ����� ����� Instantiate
    /// </summary>
    public void Initialize(HealthBar health) {
        playerHealth = health;
    }

    private void Reset() {
        // ������ �������� ���������: ������� �� ������� �� ������
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // ===== �������� =====
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

    // ===== ������� (���� ����������� ��-�������) =====
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

    // ===== ������ ���� =====
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
            // �Ĩ� ����� ������ ������ (������ ���� � ����� ��������)
            yield return wait;

            if (!touchingPlayer || playerHealth == null)
                break;

            playerHealth.TakeDamage(damagePerHit);
        }

        attackRoutine = null;
    }
}
