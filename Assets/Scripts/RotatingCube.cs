using UnityEngine;

/// <summary>
/// 프레임 체크용 회전하는 큐브
/// 메인 스레드가 블로킹되면 회전이 멈추는 것을 시각적으로 확인할 수 있습니다.
/// </summary>
public class RotatingCube : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("초당 회전 속도 (도 단위)")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Update()
    {
        // Y축을 중심으로 회전
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
