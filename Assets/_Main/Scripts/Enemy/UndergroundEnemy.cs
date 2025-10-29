using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class UndergroundEnemy : MonoBehaviour {
    public event Action<UndergroundEnemy> OnDied;

    private enum State { Rising, Following, Dead }
    private State state = State.Rising;

    [Header("References")]
    [SerializeField] private Transform model;                 // Дочерний объект с моделью (Mesh/Animator)
    [SerializeField] private NavMeshAgent agent;              // Необязателен

    [Header("Rise")]
    [SerializeField] private float riseDuration = 1.0f;
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Follow (без NavMesh)")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 1.2f;

    [Header("Face settings")]
    [Tooltip("Если true — вращение только по Y (крутимся в плане). Если false — полный 3D-взгляд.")]
    [SerializeField] private bool faceOnlyYaw = true;

    [Tooltip("Смещение ориентации модели, если её «перед» не совпадает с мировым +Z.")]
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Tooltip("Скорость поворота модели в град/сек. 0 = мгновенно.")]
    [SerializeField] private float faceRotateSpeed = 0f;

    [Header("Facing clamp (опционально)")]
    [Tooltip("Ограничивать максимальный угол отклонения взгляда от текущего вперёд?")]
    [SerializeField] private bool limitFacingAngle = false;

    [Tooltip("Макс. угол (в градусах). Для faceOnlyYaw — по горизонту, иначе — в 3D.")]
    [SerializeField] private float maxFacingAngle = 90f;

    [Header("Separation (избежание захода друг в друга)")]
    [Tooltip("Слой, на котором находятся другие враги")]
    [SerializeField] private LayerMask enemyLayer = ~0;

    [Tooltip("Радиус, в пределах которого учитываем других врагов")]
    [SerializeField] private float separationRadius = 0.8f;

    [Tooltip("Сила отталкивания (чем больше, тем агрессивнее расходятся)")]
    [SerializeField] private float separationForce = 4f;

    [Tooltip("Максимальное отталкивание за кадр (в метрах), чтобы избежать рывков")]
    [SerializeField] private float maxSeparationStep = 0.2f;

    [Tooltip("Игнорировать очень слабые толчки (стабильность)")]
    [SerializeField] private float separationEpsilon = 0.001f;

    [Tooltip("Включить separation даже при NavMeshAgent")]
    [SerializeField] private bool separationWithAgent = true;

    // --- Targets ---
    private Transform followTarget;   // На кого бежать (обычно игрок)
    private Transform faceTarget;     // На кого смотреть (обычно камера)
    private float groundY;

    // буфер без аллокаций
    private static readonly Collider[] _overlapBuffer = new Collider[32];

    /// <summary>
    /// Инициализация врага.
    /// </summary>
    public void Initialize(Transform player, float groundHeight, Transform cameraTransform = null) {
        followTarget = player;
        faceTarget = cameraTransform != null ? cameraTransform : (Camera.main != null ? Camera.main.transform : null);
        groundY = groundHeight;
    }

    public void SetFaceCamera(Transform cameraTransform) => faceTarget = cameraTransform;

    private void Awake() {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null) {
            agent.enabled = false;          // Включим после выхода на поверхность
            agent.updateRotation = false;   // Повороты делаем сами, на model
            agent.angularSpeed = 0f;        // Доп. защита от автоповорота

            // Немного повысим качество избегания и зададим радиус
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.radius = Mathf.Max(agent.radius, separationRadius * 0.5f);
            // Разные приоритеты уменьшают «застревание»
            agent.avoidancePriority = UnityEngine.Random.Range(20, 80);
        }
    }

    private void OnEnable() => StartCoroutine(RiseRoutine());

    private IEnumerator RiseRoutine() {
        state = State.Rising;

        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, groundY, start.z);

        float t = 0f;
        float safeDur = Mathf.Max(0.01f, riseDuration);
        while (t < 1f) {
            t += Time.deltaTime / safeDur;
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
        if (state == State.Dead) return;

        // Движение родителя (позиция)
        if (state == State.Following && followTarget != null) {
            if (agent != null && agent.enabled && agent.isOnNavMesh) {
                agent.destination = followTarget.position;

                // Доп. separation поверх агента при желании
                if (separationWithAgent)
                    ApplySeparation();
            }
            else {
                Vector3 to = followTarget.position - transform.position;
                to.y = 0f;
                float dist = to.magnitude;
                if (dist > stoppingDistance) {
                    Vector3 dir = to / Mathf.Max(dist, 0.0001f);
                    transform.position += dir * moveSpeed * Time.deltaTime;
                }

                // Для простого перемещения separation обязателен
                ApplySeparation();
            }
        }

        // Если цели взгляда нет — попробуем подцепить основную камеру
        if (faceTarget == null && Camera.main != null)
            faceTarget = Camera.main.transform;
    }

    private void LateUpdate() => FaceTargetModel();

    private void FaceTargetModel() {
        if (model == null || faceTarget == null) return;

        Vector3 dir = faceTarget.position - model.position;

        if (faceOnlyYaw) {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f) return;

            if (limitFacingAngle) {
                Vector3 fwd = model.forward; fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-6f) {
                    fwd.Normalize();
                    Vector3 dirN = dir.normalized;
                    float angle = Vector3.Angle(fwd, dirN);
                    if (angle > maxFacingAngle) {
                        float t = maxFacingAngle / Mathf.Max(angle, 0.0001f);
                        dir = Vector3.Slerp(fwd, dirN, t);
                    }
                }
            }

            dir.Normalize();
        }
        else {
            if (dir.sqrMagnitude < 1e-6f) return;

            if (limitFacingAngle) {
                Vector3 fwd = model.forward;
                if (fwd.sqrMagnitude > 1e-6f) {
                    fwd.Normalize();
                    Vector3 dirN = dir.normalized;
                    float angle = Vector3.Angle(fwd, dirN);
                    if (angle > maxFacingAngle) {
                        float t = maxFacingAngle / Mathf.Max(angle, 0.0001f);
                        dir = Vector3.Slerp(fwd, dirN, t);
                    }
                }
            }

            dir.Normalize();
        }

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(rotationOffsetEuler);

        if (faceRotateSpeed > 0f)
            model.rotation = Quaternion.RotateTowards(model.rotation, targetRot, faceRotateSpeed * Time.deltaTime);
        else
            model.rotation = targetRot;
    }

    /// <summary>
    /// Простейший separation: отталкиваемся от ближайших врагов в радиусе.
    /// Работает в плоскости XZ, без вертикальных толчков.
    /// </summary>
    private void ApplySeparation() {
        if (separationRadius <= 0f || separationForce <= 0f) return;

        Vector3 pos = transform.position;
        int count = Physics.OverlapSphereNonAlloc(pos, separationRadius, _overlapBuffer, enemyLayer, QueryTriggerInteraction.Ignore);

        if (count <= 0) return;

        Vector3 push = Vector3.zero;
        for (int i = 0; i < count; i++) {
            Collider col = _overlapBuffer[i];
            if (col == null) continue;

            // игнорируем себя
            if (col.attachedRigidbody != null && col.attachedRigidbody.transform == transform) continue;
            if (col.transform == transform || col.transform.IsChildOf(transform) || transform.IsChildOf(col.transform)) continue;

            // игнорируем, если это не враг
            if (!col.TryGetComponent<UndergroundEnemy>(out var other)) continue;

            Vector3 delta = pos - col.transform.position;
            delta.y = 0f;
            float dist = delta.magnitude;
            if (dist < 1e-4f) continue;

            // Чем ближе — тем сильнее толчок (обратная пропорция)
            float strength = Mathf.Clamp01((separationRadius - dist) / separationRadius);
            Vector3 dir = delta / dist;
            push += dir * strength;
        }

        if (push.sqrMagnitude < separationEpsilon * separationEpsilon) return;

        // Нормируем по силе и времени, ограничиваем шаг
        Vector3 step = push.normalized * separationForce * Time.deltaTime;
        if (step.magnitude > maxSeparationStep)
            step = step.normalized * maxSeparationStep;

        // Если агент активен — слегка подвинем destination (чтобы не спорить с навмешем)
        if (agent != null && agent.enabled && agent.isOnNavMesh) {
            // Сдвигаем текущую позицию немного вручную — агент это переварит.
            transform.position += new Vector3(step.x, 0f, step.z);
        }
        else {
            transform.position += new Vector3(step.x, 0f, step.z);
        }
    }

    public void Kill() {
        if (state == State.Dead) return;
        state = State.Dead;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (separationRadius > 0f) {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, separationRadius);
        }
    }
#endif
}
