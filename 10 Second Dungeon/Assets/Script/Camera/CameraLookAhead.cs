using UnityEngine;

public class CameraLookAhead : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField] private Transform target;

    [Header("基本オフセット")]
    [SerializeField] private Vector3 baseOffset;

    [Header("先読み距離")]
    [SerializeField] private float lookAheadDistance = 2f;

    [Header("戻り速度")]
    [SerializeField] private float returnSpeed = 5f;

    [Header("追従スピード")]
    [SerializeField] private float followSpeed = 5f;

    private Vector3 currentLookAhead;

    void LateUpdate()
    {
        // WASD入力取得
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 targetLookAhead = new Vector3(inputX, inputY, 0) * lookAheadDistance;

        // なめらかに補間
        currentLookAhead = Vector3.Lerp(
            currentLookAhead,
            targetLookAhead,
            returnSpeed * Time.deltaTime
        );

        Vector3 targetPos = target.position + baseOffset + currentLookAhead;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }
}
