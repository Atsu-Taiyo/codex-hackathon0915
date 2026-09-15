using UnityEngine;

namespace Hackathon.Map
{
    /// <summary>Camera-relative keyboard and click-to-walk control on a 3D floor.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Animation))]
    public sealed class CodexRoboMotor : MonoBehaviour
    {
        public float walkSpeed = 1.4f;
        public float turnSpeed = 540f;
        public Camera viewCamera;
        public LayerMask groundMask = 1 << 8;
        public bool acceptInput = true;
        public Vector3 Velocity { get; private set; }
        public bool HasDestination { get; private set; }
        public Vector3 Destination { get; private set; }
        public bool IsWalking => new Vector2(Velocity.x, Velocity.z).magnitude > .03f;
        CharacterController controller;
        Animation animationPlayer;
        float fallSpeed;
        Vector2 steering;
        bool wasWalking;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            animationPlayer = GetComponent<Animation>();
            if (viewCamera == null) viewCamera = Camera.main;
            animationPlayer.Play("Idle");
        }
        public void WalkTo(Vector3 destination)
        {
            Destination = destination; HasDestination = true; steering = Vector2.zero;
        }
        public void Steer(Vector2 direction)
        {
            steering = Vector2.ClampMagnitude(direction, 1); HasDestination = false;
        }
        public void StopWalking() { steering = Vector2.zero; HasDestination = false; }
        void Update()
        {
            if (acceptInput)
            {
                steering = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1);
                if (steering.sqrMagnitude > .01f) HasDestination = false;
                if (Input.GetMouseButtonDown(0) && viewCamera != null &&
                    Physics.Raycast(viewCamera.ScreenPointToRay(Input.mousePosition), out var hit, 200f, groundMask, QueryTriggerInteraction.Ignore))
                    WalkTo(hit.point);
            }
            Vector3 direction;
            float speed = walkSpeed;
            if (HasDestination)
            {
                var delta = Destination - transform.position; delta.y = 0;
                if (delta.magnitude <= .08f) { HasDestination = false; direction = Vector3.zero; }
                else { direction = delta.normalized; speed = Mathf.Min(walkSpeed, delta.magnitude / Mathf.Max(Time.deltaTime, .0001f)); }
            }
            else
            {
                var forward = viewCamera != null ? viewCamera.transform.forward : Vector3.forward;
                forward.y = 0; forward.Normalize();
                var right = Vector3.Cross(Vector3.up, forward);
                direction = forward * steering.y + right * steering.x;
            }
            if (direction.sqrMagnitude > .001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            fallSpeed = controller.isGrounded ? -2f : Mathf.Max(fallSpeed - 20f * Time.deltaTime, -30f);
            var before = transform.position;
            controller.Move((direction * speed + Vector3.up * fallSpeed) * Time.deltaTime);
            Velocity = (transform.position - before) / Mathf.Max(Time.deltaTime, .0001f);
            bool walking = IsWalking;
            if (walking != wasWalking) animationPlayer.CrossFade(walking ? "Walk" : "Idle", .15f);
            animationPlayer["Walk"].speed = Mathf.Clamp(new Vector2(Velocity.x, Velocity.z).magnitude / walkSpeed, .25f, 1.5f);
            wasWalking = walking;
        }
        void OnDisable() { StopWalking(); Velocity = Vector3.zero; }
    }
}
