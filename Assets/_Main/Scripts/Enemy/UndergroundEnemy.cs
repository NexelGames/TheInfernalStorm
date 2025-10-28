using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class UndergroundEnemy : MonoBehaviour {
    public event Action<UndergroundEnemy> OnDied;

    private enum State { Rising, Following, Dead }
    private State state = State.Rising;

    [Header("References")]
    [SerializeField] private Transform model;                 // дочерний объект-модель (обязательно)
    [SerializeField] private NavMeshAgent agent;              // опционально

    [Header("Rise")]
    [SerializeField] private float riseDuration = 1.0f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Follow (без NavMesh)")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.2f;

    [Header("Face settings")]
    [SerializeField] private bool faceOnlyYaw = true;                 // крутиться только по Y
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero; // сдвиг, если «перед» модели не по +Z
    [SerializeField] private float faceRotateSpeed = 0f;              // 0 = мгновенно, >0 = град/сек

    private Transform target;
    private float groundY;

    public void Initialize(Transform player, float groundHeight) {
        target = player;
        groundY = groundHeight;
    }

    private void Awake() {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null) {
            agent.enabled = false;
            agent.updateRotation = false; // повороты делаем сами на model
            agent.angularSpeed = 0f;      // дополнительная защита от автоповорота
        }
    }

    private void OnEnable() {
        StartCoroutine(Rise());
    }

    private IEnumerator Rise() {
        state = State.Rising;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, groundY, start.z);

        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / Mathf.Max(0.01f, riseDuration);
            float k = riseCurve.Evaluate(t);
            transform.position = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }
        transform.position = end;

        if (agent != null) {
            agent.enabled = true;
            agent.stoppingDistance = stoppingDistance;
        }

        state = State.Following;
    }

    private void Update() {
        if (state == State.Dead || target == null) return;

        // движение родителя
        if (state == State.Following) {
            if (agent != null && agent.enabled && agent.isOnNavMesh) {
                agent.destination = target.position;
            }
            else {
                Vector3 to = target.position - transform.position;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist > stoppingDistance) {
                    Vector3 dir = to / Mathf.Max(dist, 0.0001f);
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }
            }
        }
    }

    // ВАЖНО: поворачиваем модель в LateUpdate, чтобы перебить анимации/другие скрипты
    private void LateUpdate() {
        FaceTargetModel();
    }

    private void FaceTargetModel() {
        if (model == null || target == null) return;

        Vector3 dir = target.position - model.position;
        if (faceOnlyYaw) dir.y = 0f;
        if (dir.sqrMagnitude < 1e-6f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up)
                             * Quaternion.Euler(rotationOffsetEuler);

        if (faceRotateSpeed > 0f) {
            model.rotation = Quaternion.RotateTowards(model.rotation, targetRot, faceRotateSpeed * Time.deltaTime);
        }
        else {
            model.rotation = targetRot; // мгновенный поворот
        }
    }

    public void Kill() {
        if (state == State.Dead) return;
        state = State.Dead;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }
}
