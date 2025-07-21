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

    [Header("Wall Adjustment")]
    public float wallCheckDistance = 1.0f;
    public float wallPushX = 1.5f;
    public float wallRotateY = 30f;

    [Header("Wall Mode Smoothing")]
    public float wallExitBuffer = 1.5f;
    public float wallBlendSpeed = 3f;

    [Header("RTS Drone Camera Shake")]
    public float rtsShakeYPosRange = 0.5f;
    public float rtsShakeZPosRange = 0.3f;
    public float rtsShakeYRotRange = 2f;
    public float rtsShakeZRotRange = 2f;
    public float rtsShakeSpeed = 1.5f;

    [Header("Camera FOV Settings")]
    public float tpsFOV = 60f;
    public float rtsFOV = 75f;
    public float fovLerpSpeed = 5f;

    [Header("Smoothness")]
    public float followSpeed = 5f;
    public float offsetLerpSpeed = 5f;
    public float transitionSpeed = 2f;

    private bool isFollowing = false;
    private float currentOffset;
    private Vector3 currentPosOffset;
    private Vector3 currentRotOffset;
    private float wallEffectBlend = 0f;
    private float shakeTime = 0f;

    private Unit unitScript;
    private Camera cam;

    private void Start()
    {
        currentOffset = horizontalOffset;
        currentPosOffset = movingOffset;
        currentRotOffset = movingRotation;

        if (target != null)
            unitScript = target.GetComponent<Unit>();

        cam = Camera.main;
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

        shakeTime += Time.deltaTime * rtsShakeSpeed;

        // Direction setup
        Vector3 forwardDir = target.forward;
        forwardDir.y = 0;
        forwardDir.Normalize();
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);
        Vector3 sideOrigin = target.position + Vector3.up * 1.5f;

        // Wall detection
        bool leftBlocked = Physics.Raycast(sideOrigin, -rightDir, wallCheckDistance);
        bool rightBlocked = Physics.Raycast(sideOrigin, rightDir, wallCheckDistance);

        // Check extended range for wall exit buffer
        bool wallNearby = false;
        float wallDistance = Mathf.Infinity;
        RaycastHit wallHit;

        if (Physics.Raycast(sideOrigin, -rightDir, out wallHit, wallCheckDistance + wallExitBuffer))
        {
            wallNearby = true;
            wallDistance = wallHit.distance;
        }
        else if (Physics.Raycast(sideOrigin, rightDir, out wallHit, wallCheckDistance + wallExitBuffer))
        {
            wallNearby = true;
            wallDistance = wallHit.distance;
        }

        float targetBlend = 0f;

        if (wallNearby && wallDistance <= wallCheckDistance)
            targetBlend = 1f;
        else if (wallNearby && wallDistance <= wallCheckDistance + wallExitBuffer)
            targetBlend = 1f - Mathf.InverseLerp(wallCheckDistance, wallCheckDistance + wallExitBuffer, wallDistance);
        else
            targetBlend = 0f;

        wallEffectBlend = Mathf.MoveTowards(wallEffectBlend, targetBlend, wallBlendSpeed * Time.deltaTime);

        // Base offset and rotation
        Vector3 baseOffset = unitScript.IsMoving() ? movingOffset : stoppedOffset;
        Vector3 baseRotation = unitScript.IsMoving() ? movingRotation : stoppedRotation;

        // RTS camera shake
        if (!unitScript.IsMoving())
        {
            float shakeY = Mathf.Sin(shakeTime) * rtsShakeYPosRange;
            float shakeZ = Mathf.Cos(shakeTime * 0.8f) * rtsShakeZPosRange;

            float rotShakeY = Mathf.Sin(shakeTime * 0.6f) * rtsShakeYRotRange;
            float rotShakeZ = Mathf.Cos(shakeTime * 0.7f) * rtsShakeZRotRange;

            baseOffset.y += shakeY;
            baseOffset.z += shakeZ;

            baseRotation.y += rotShakeY;
            baseRotation.z += rotShakeZ;
        }

        // Wall-adjusted values
        float sideSign = rightBlocked ? -1 : (leftBlocked ? 1 : Mathf.Sign(horizontalOffset));
        float wallXOffset = horizontalOffset + sideSign * wallPushX;
        float wallYRot = baseRotation.y + sideSign * wallRotateY;

        Vector3 adjustedOffset = new Vector3(wallXOffset, baseOffset.y, baseOffset.z);
        Vector3 adjustedRotation = new Vector3(baseRotation.x, wallYRot, baseRotation.z);

        // Blend between base and wall-modified
        Vector3 targetOffset = Vector3.Lerp(baseOffset, adjustedOffset, wallEffectBlend);
        Vector3 targetRotation = Vector3.Lerp(baseRotation, adjustedRotation, wallEffectBlend);

        currentPosOffset = Vector3.Lerp(currentPosOffset, targetOffset, transitionSpeed * Time.deltaTime);
        currentRotOffset = Vector3.Lerp(currentRotOffset, targetRotation, transitionSpeed * Time.deltaTime);
        currentOffset = Mathf.Lerp(currentOffset, targetOffset.x, offsetLerpSpeed * Time.deltaTime);

        // Calculate desired camera position
        Vector3 desiredPosition = target.position
                                - forwardDir * currentPosOffset.z
                                + Vector3.up * currentPosOffset.y
                                + rightDir * currentOffset;

        // Obstacle avoidance
        Vector3 castOrigin = target.position + Vector3.up * 1.5f;
        Vector3 directionToCamera = (desiredPosition - castOrigin).normalized;
        float distance = Vector3.Distance(castOrigin, desiredPosition);

        if (Physics.Raycast(castOrigin, directionToCamera, out RaycastHit hit, distance))
            desiredPosition = hit.point - directionToCamera * 0.2f;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Orbit around unit center using rotation offset
        Vector3 pivotPoint = target.position + Vector3.up * 1.5f;
        Quaternion orbitRotation = Quaternion.Euler(0, currentRotOffset.y, 0);
        Vector3 offsetDir = orbitRotation * (transform.position - pivotPoint);
        Vector3 rotatedCameraPos = pivotPoint + offsetDir;

        transform.position = Vector3.Lerp(transform.position, rotatedCameraPos, followSpeed * Time.deltaTime);

        // Look at unit with tilt
        Vector3 lookDir = pivotPoint - transform.position;

        if (lookDir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            lookRot *= Quaternion.Euler(currentRotOffset.x, 0, currentRotOffset.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, followSpeed * Time.deltaTime);
        }

        // Smooth FOV transition
        if (cam != null)
        {
            float targetFOV = unitScript.IsMoving() ? tpsFOV : rtsFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }
    }
}
