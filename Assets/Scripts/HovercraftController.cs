// HovercraftController_InputActions.cs
// Uses the *generated C# wrapper* for your Input Actions asset named: HovercraftInput
// Action map: "Player"
// Actions: "Move" (Vector2), "Jump" (Button), "StickToWalls" (Button)
//
// IMPORTANT:
// 1) Put your JSON into an Input Actions asset named "HovercraftInput".
// 2) In that asset's Inspector, enable: "Generate C# Class"
// 3) Set the generated class name to: HovercraftInput   (matches below)
// 4) Save/Apply so Unity generates HovercraftInput.cs
//
// This controller defaults to terrain/ramps hover.
// If you hold StickToWalls, it will attempt wall-sticking (optional, can be ignored).

using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class HovercraftController : MonoBehaviour
{
    [Header("Hover Points (4 corners recommended)")]
    [SerializeField] private Transform[] hoverPoints;

    [Header("Hover (Multi-ray spring)")]
    [Min(0.05f)] [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private float rayLengthMultiplier = 1.5f;
    [SerializeField] private float springStrength = 120f;
    [SerializeField] private float springDamping = 18f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Movement")]
    [SerializeField] private float thrustAccel = 25f;
    [SerializeField] private float turnTorqueAccel = 12f;
    [SerializeField] private float maxSpeed = 35f;

    [Header("Orientation (ramps/terrain)")]
    [SerializeField] private float alignToGroundSpeed = 6f;

    [Header("Handling")]
    [SerializeField] private float lateralFriction = 6f;
    [SerializeField] private float angularDamping = 2f;

    [Header("Jump")]
    [SerializeField] private float jumpImpulse = 7f;
    [SerializeField] private float jumpCooldown = 0.25f;

    [Header("Optional Wall Stick (hold action)")]
    [Tooltip("If enabled, holding StickToWalls will align 'up' to the surface normal even when steep.\n" +
             "If disabled, the craft only aligns on typical terrain/ramp angles.")]
    [SerializeField] private bool enableWallStickOption = false;

    [Tooltip("Max slope angle (degrees) where we still align in normal terrain mode.")]
    [Range(0f, 89f)]
    [SerializeField] private float maxAlignSlopeAngle = 70f;

    [Tooltip("How quickly we rotate in wall-stick mode.")]
    [SerializeField] private float wallStickAlignSpeed = 10f;

    private Rigidbody rb;

    // Input
    private HovercraftInput input;                  // generated wrapper
    private Vector2 move;
    private bool jumpQueued;
    private bool stickHeld;

    // Ground sampling
    private int groundedPointCount;
    private Vector3 accumulatedGroundNormal;
    private float lastJumpTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // normal gravity for terrain/ramps mode

        input = new HovercraftInput();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;

        input.Player.Jump.performed += OnJump;

        input.Player.StickToWalls.performed += OnStick;
        input.Player.StickToWalls.canceled += OnStick;
    }

    private void OnDisable()
    {
        // Unsubscribe first (good practice, prevents duplicate subscriptions in domain reload edge cases)
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;

        input.Player.Jump.performed -= OnJump;

        input.Player.StickToWalls.performed -= OnStick;
        input.Player.StickToWalls.canceled -= OnStick;

        input.Disable();
    }

    private void FixedUpdate()
    {
        if (hoverPoints == null || hoverPoints.Length == 0)
            return;

        ApplyHoverForces();
        AlignToGround();

        ApplyDrive();
        ApplySideFriction();
        ClampMaxSpeed();
        ApplyAngularDamping();

        if (jumpQueued)
            TryJump();
    }

    // ---------------- Input callbacks ----------------

    private void OnMove(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        // Queue for FixedUpdate so physics stays deterministic.
        jumpQueued = true;
    }

    private void OnStick(InputAction.CallbackContext ctx)
    {
        stickHeld = ctx.ReadValueAsButton();
    }

    // ---------------- Hover physics ----------------

    private void ApplyHoverForces()
    {
        groundedPointCount = 0;
        accumulatedGroundNormal = Vector3.zero;

        Vector3 downAxis = -transform.up;
        Vector3 upAxis = transform.up;

        float rayLength = hoverHeight * Mathf.Max(1f, rayLengthMultiplier);

        for (int i = 0; i < hoverPoints.Length; i++)
        {
            Transform p = hoverPoints[i];
            if (p == null) continue;

            Vector3 origin = p.position;

            if (!Physics.Raycast(origin, downAxis, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
                continue;

            // Only push when within hover height. Beyond that, no "spring".
            if (hit.distance > hoverHeight)
                continue;

            float compression = hoverHeight - hit.distance;

            // Damping using point velocity along the spring axis
            Vector3 pointVel = rb.GetPointVelocity(origin);
            float velAlongUp = Vector3.Dot(pointVel, upAxis);

            float springForce = compression * springStrength;
            float dampingForce = -velAlongUp * springDamping;

            float total = springForce + dampingForce;

            rb.AddForceAtPosition(upAxis * total, origin, ForceMode.Acceleration);

            accumulatedGroundNormal += hit.normal;
            groundedPointCount++;
        }
    }

    private void AlignToGround()
    {
        if (groundedPointCount <= 0)
            return;

        Vector3 avgNormal = (accumulatedGroundNormal / groundedPointCount).normalized;

        // Determine if we should align, depending on wall-stick option and slope angle.
        float slopeAngle = Vector3.Angle(avgNormal, Vector3.up);

        bool wantWallStick = enableWallStickOption && stickHeld;
        bool canAlignInTerrainMode = slopeAngle <= maxAlignSlopeAngle;

        if (!wantWallStick && !canAlignInTerrainMode)
            return;

        float speed = wantWallStick ? wallStickAlignSpeed : alignToGroundSpeed;

        Quaternion target = Quaternion.FromToRotation(transform.up, avgNormal) * rb.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, Time.fixedDeltaTime * speed));
    }

    // ---------------- Driving / handling ----------------

    private void ApplyDrive()
    {
        float throttle = Mathf.Clamp(move.y, -1f, 1f);
        float steer = Mathf.Clamp(move.x, -1f, 1f);

        rb.AddForce(transform.forward * (throttle * thrustAccel), ForceMode.Acceleration);
        rb.AddTorque(transform.up * (steer * turnTorqueAccel), ForceMode.Acceleration);
    }

    private void ApplySideFriction()
    {
        // Reduce sideways slip in local space, keep forward speed more intact.
        // Exponential decay gives stable behavior across different FixedDeltaTime.
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float t = 1f - Mathf.Exp(-lateralFriction * Time.fixedDeltaTime);
        localVel.x = Mathf.Lerp(localVel.x, 0f, t);

        rb.linearVelocity = transform.TransformDirection(localVel);
    }

    private void ClampMaxSpeed()
    {
        Vector3 v = rb.linearVelocity;
        float speed = v.magnitude;
        if (speed > maxSpeed)
            rb.linearVelocity = v * (maxSpeed / speed);
    }

    private void ApplyAngularDamping()
    {
        float t = 1f - Mathf.Exp(-angularDamping * Time.fixedDeltaTime);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, t);
    }

    // ---------------- Jump ----------------

    private void TryJump()
    {
        jumpQueued = false;

        if (Time.time < lastJumpTime + jumpCooldown)
            return;

        if (groundedPointCount <= 0)
            return;

        lastJumpTime = Time.time;

        Vector3 avgNormal = (accumulatedGroundNormal / groundedPointCount).normalized;
        rb.AddForce(avgNormal * jumpImpulse, ForceMode.Impulse);
    }

    // ---------------- Debug ----------------

    private void OnDrawGizmosSelected()
    {
        if (hoverPoints == null) return;

        Gizmos.color = Color.yellow;
        float rayLength = hoverHeight * Mathf.Max(1f, rayLengthMultiplier);

        foreach (var p in hoverPoints)
        {
            if (p == null) continue;
            Gizmos.DrawLine(p.position, p.position - transform.up * rayLength);
        }
    }
}
