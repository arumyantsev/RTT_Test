using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("TPS Camera (When Moving)")]
    public Vector3 movingOffset = new Vector3(0, 3, 8);
    public Vector3 movingRotation = new Vector3(10, 0, 0);

    [Header("RTS Camera (When Stopped)")]
    public Vector3 stoppedOffset = new Vector3(0, 7, 8);
    public Vector3 stoppedRotation = new Vector3(30, 0, 0);

    [Header("Shoulder Settings")]
    public float horizontalOffset = 2f;

    [Header("Smoothness")]
    public float followSpeed = 5f;
    public float offsetLerpSpeed = 5f;
    public float transitionSpeed = 2f; // camera blend speed when switching states

    [Header("Collision Settings")]
    public float flipCheckDistance = 1.0f;

    private bool isFollowing = false;
    private float currentOffset;
    private Vector3 currentPosOffset;
    private Vector3 currentRotOffset;

    private Unit unitScript;

    private void Start()
    {
        currentOffset = horizontalOffset;
        currentPosOffset = movingOffset;
        currentRotOffset = movingRotation;

        if (target != null)
        {
            unitScript = target.GetComponent<Unit>();
        }
    }

    public void FollowTarget(Transform newTarget)
    {
        target = newTarget;
        isFollowing = true;
        unitScript = target.GetComponent<Unit>();
    }

    public void StopFollowing()
    {
        isFollowing = false;
        target = null;
        unitScript = null;
    }

    private void LateUpdate()
    {
        if (!isFollowing || target == null || unitScript == null) return;

        // Blend to stop/move offsets
        Vector3 targetOffset = unitScript.IsMoving() ? movingOffset : stoppedOffset;
        Vector3 targetRotation = unitScript.IsMoving() ? movingRotation : stoppedRotation;

        currentPosOffset = Vector3.Lerp(currentPosOffset, targetOffset, transitionSpeed * Time.deltaTime);
        currentRotOffset = Vector3.Lerp(currentRotOffset, targetRotation, transitionSpeed * Time.deltaTime);

        // Forward/right directions
        Vector3 forwardDir = target.forward;
        forwardDir.y = 0;
        forwardDir.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);
        Vector3 sideOrigin = target.position + Vector3.up * 1.5f;

        // Check wall side
        bool isRightBlocked = Physics.Raycast(sideOrigin, rightDir, flipCheckDistance);
        bool isLeftBlocked = Physics.Raycast(sideOrigin, -rightDir, flipCheckDistance);

        if (horizontalOffset >= 0 && isLeftBlocked && !isRightBlocked)
            currentOffset = Mathf.Lerp(currentOffset, -Mathf.Abs(horizontalOffset), offsetLerpSpeed * Time.deltaTime);
        else if (horizontalOffset <= 0 && isRightBlocked && !isLeftBlocked)
            currentOffset = Mathf.Lerp(currentOffset, Mathf.Abs(horizontalOffset), offsetLerpSpeed * Time.deltaTime);
        else
            currentOffset = Mathf.Lerp(currentOffset, horizontalOffset, offsetLerpSpeed * Time.deltaTime);

        // Compute desired camera position
        Vector3 desiredPosition = target.position
                                - forwardDir * currentPosOffset.z
                                + Vector3.up * currentPosOffset.y
                                + rightDir * currentOffset;

        // Prevent wall clipping
        Vector3 castOrigin = target.position + Vector3.up * 1.5f;
        Vector3 directionToCamera = (desiredPosition - castOrigin).normalized;
        float distance = Vector3.Distance(castOrigin, desiredPosition);

        if (Physics.Raycast(castOrigin, directionToCamera, out RaycastHit hit, distance))
            desiredPosition = hit.point - directionToCamera * 0.2f;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Blend look direction and apply extra rotation
        Vector3 lookDirToUnit = (target.position - transform.position).normalized;
        Vector3 blendedLookDir = Vector3.Lerp(forwardDir, lookDirToUnit, 0.3f);
        blendedLookDir.y = 0;

        if (blendedLookDir != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(blendedLookDir);
            desiredRotation *= Quaternion.Euler(currentRotOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, followSpeed * Time.deltaTime);
        }
    }
}
