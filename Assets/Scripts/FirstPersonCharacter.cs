using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCharacter : MonoBehaviour
{
    [SerializeField] private float runSpeed = 8f;                                       // The speed at which we want the character to move
    [SerializeField] private float strafeSpeed = 4f;                                    // The speed at which we want the character to be able to strafe
    [SerializeField] private float jumpPower = 5f;                                      // The power behind the characters jump. increase for higher jumps

    [SerializeField] private AdvancedSettings advanced = new AdvancedSettings();        // The container for the advanced settings ( done this way so that the advanced setting are exposed under a foldout
    [SerializeField] private bool lockCursor = true;

    [System.Serializable]
    public class AdvancedSettings                                                       // The advanced settings
    {
        public float gravityMultiplier = 1f;                                            // Changes the way gravity effect the player ( realistic gravity can look bad for jumping in game )
        public PhysicsMaterial zeroFrictionMaterial;                                     // Material used for zero friction simulation
        public PhysicsMaterial highFrictionMaterial;                                     // Material used for high friction ( can stop character sliding down slopes )
        public float groundStickyEffect = 5f;                                           // power of 'stick to ground' effect - prevents bumping down slopes.
    }

    private CapsuleCollider capsule;                                                    // The capsule collider for the first person character
    private const float jumpRayLength = 0.7f;                                           // The length of the ray used for testing against the ground when jumping
    public bool grounded { get; private set; }
    public Transform pivotPoint;
    private Vector2 input;
    private IComparer rayHitComparer;
    private bool isJumping = false;
    private float horizontal;
    private float vertical;

    void Awake()
    {
        // Set up a reference to the capsule collider.
        capsule = GetComponent<Collider>() as CapsuleCollider;
        grounded = true;
        rayHitComparer = new RayHitComparer();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    public void FixedUpdate()
    {
        float speed = runSpeed;

        bool jump = isJumping;

        input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1) input.Normalize();

        // Get a vector which is desired move as a world-relative direction, including speeds
        Vector3 desiredMove = pivotPoint.forward * input.y * speed + pivotPoint.right * input.x * strafeSpeed;

        // preserving current y velocity (for falling, gravity)
        float yv = GetComponent<Rigidbody>().linearVelocity.y;

        // add jump power
        if (grounded && jump)
        {
            yv += jumpPower;
            grounded = false;
        }

        // Set the rigidbody's velocity according to the ground angle and desired move
        GetComponent<Rigidbody>().linearVelocity = desiredMove + Vector3.up * yv;

        // Use low/high friction depending on whether we're moving or not
        if (desiredMove.magnitude > 0 || !grounded)
        {
            GetComponent<Collider>().material = advanced.zeroFrictionMaterial;
        }
        else
        {
            GetComponent<Collider>().material = advanced.highFrictionMaterial;
        }


        // Ground Check:

        // Create a ray that points down from the centre of the character.
        Ray ray = new Ray(pivotPoint.position, -transform.up);

        // Raycast slightly further than the capsule (as determined by jumpRayLength)
        RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * jumpRayLength);
        System.Array.Sort(hits, rayHitComparer);


        if (grounded || GetComponent<Rigidbody>().linearVelocity.y < jumpPower * .5f)
        {
            // Default value if nothing is detected:
            grounded = false;
            // Check every collider hit by the ray
            for (int i = 0; i < hits.Length; i++)
            {
                // Check it's not a trigger
                if (!hits[i].collider.isTrigger)
                {
                    // The character is grounded, and we store the ground angle (calculated from the normal)
                    grounded = true;

                    // stick to surface - helps character stick to ground - specially when running down slopes
                    //if (rigidbody.velocity.y <= 0) {
                    GetComponent<Rigidbody>().position = Vector3.MoveTowards(GetComponent<Rigidbody>().position, hits[i].point + Vector3.up * capsule.height * .5f, Time.deltaTime * advanced.groundStickyEffect);
                    //}
                    GetComponent<Rigidbody>().linearVelocity = new Vector3(GetComponent<Rigidbody>().linearVelocity.x, 0, GetComponent<Rigidbody>().linearVelocity.z);
                    break;
                }
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * capsule.height * jumpRayLength, grounded ? Color.green : Color.red);


        // add extra gravity
        GetComponent<Rigidbody>().AddForce(Physics.gravity * (advanced.gravityMultiplier - 1));
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.forward * 5f);
    }

    public void OnMove(InputValue inputValue)
    {
        Vector2 inputVector = inputValue.Get<Vector2>();
        horizontal = inputVector.x;
        vertical = inputVector.y;
    }

    public void MoveInput (Vector2 moveDirection)
    {
        horizontal = moveDirection.x;
        vertical = moveDirection.y;
    }

    public void OnJump(InputValue inputValue)
    {
        isJumping = inputValue.isPressed;
    }

    public void LookInput(Vector2 lookDirection)
    {
        // not used for movement, but can be used by a camera script to rotate the camera
    }

    //used for comparing distances
    class RayHitComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}