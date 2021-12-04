using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //All the Serialized stuff shows up in the editor for bugfixing and other devlopment needs
    //They're just variables containing values for gameplay, or booleans that check if the player is in a current state
    [Header("Raycasting Checks")]
    [SerializeField] public Transform leftGroundCheck;
    [SerializeField] public Transform rightGroundCheck;
    [SerializeField] public Transform slopeCheck;
    [SerializeField] public Transform wallCheck;
    [SerializeField] public Transform vaultCheck;

    [Header("Raycasting Settings")]
    [SerializeField] public int rayResolution;
    [SerializeField] public float rayLength;
    [SerializeField] public bool[] hitOutcomes;
    [SerializeField] public float wallRayLength;

    [Header("Layer Masks")]
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] public LayerMask deathLayer;
    [SerializeField] public LayerMask endLayer;

    [Header("Facing Direction")]
    [SerializeField] public float horizontalDirection = 1f;
    [SerializeField] public bool isFlipped => (horizontalDirection > 0f ? false : true);
    [SerializeField] private float verticalDirection;

    [Header("Movement Modifiers")]
    [SerializeField] public float acceleration;
    [SerializeField] public float speed;
    [SerializeField] public float maxMovementSpeed;
    [SerializeField] public float minMovementSpeed;

    [SerializeField] public float jumpHeight;

    [Header("Booleans")]
    [SerializeField] public bool canMove = true;
    [SerializeField] public bool isGrounded;
    [SerializeField] public bool isTouchingWall;
    [SerializeField] public bool canJump;
    [SerializeField] public bool justWallJumped;
    [SerializeField] public bool isWallRunning;
    [SerializeField] public bool WallRunTrigger;
    [SerializeField] public bool canWallRun = true;
    [SerializeField] public bool isSliding = false;
    [SerializeField] public bool isFunny = false;

    [Header("Player Components")]
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public SpriteRenderer sr;
    [SerializeField] private Animator animator;
    [SerializeField] public PhysicsMaterial2D friction;
    [SerializeField] public PhysicsMaterial2D frictionless;
    [SerializeField] public CapsuleCollider2D cc;
    [SerializeField] public ParticleSystem wallRunParticle;

    [Header("Corner Correction Variables")]
    [SerializeField] private float topRaycastLength;
    [SerializeField] private Vector3 edgeRaycastOffset;
    [SerializeField] private Vector3 innerRaycastOffset;
    [SerializeField] private bool CanCornerCorrect;

    private float fixedDeltaTime;


    //---------------------------------------------
    //When the code is first initialized, set the fixed Deltatime 
    private void Awake()
    {
        hitOutcomes = new bool[rayResolution];
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }
    void Start()
    {

    }

    void Update()
    {
        //Raycast downwards
        MultiCastRay();

        //Move the character based off input
        MoveCharacter();

        //Detect the ground based off Raycasts
        DetectGround();

        //Determines if the player can jump
        CanJump();

        //Slide on the wall
        WallSlide();

        //Jump
        Jump();

        //Flips the sprite when moving in the opposite direction
        FlipSprite();

        //Wallrun
        WallRun();
        StartCoroutine(executeWallRun());

        //Slide
        slide();

        //If you press E, slow down time
        if(Input.GetKeyDown("e"))
        {
            timeSlow(0.2f);
        }

        //Momemtum function, decreases speed as time goes on
        StartCoroutine(speedDecrease());

        //stops the timer and player movement at the end of the level
        endStage();
    }

    //----------------------------------------------

    //This function is for downwards raycasting, to check if the player is on the ground
    void MultiCastRay()
    {
        Vector2 startPoint = leftGroundCheck.transform.position;
        float xDistBetweenRays = 0;
        if (rayResolution - 2 <= 0)
        {
            xDistBetweenRays = Mathf.Abs(rightGroundCheck.transform.position.x - leftGroundCheck.transform.position.x);
        }
        else
        {
            xDistBetweenRays = Mathf.Abs(rightGroundCheck.transform.position.x - leftGroundCheck.transform.position.x) / (rayResolution - 1);
        }
        for (int i = 0; i < rayResolution; i++)
        {
            if (i == 0)
            {
                startPoint.x = leftGroundCheck.transform.position.x;
            }
            else if (i == rayResolution - 1)
            {
                startPoint.x = rightGroundCheck.transform.position.x;
            }
            else
            {
                startPoint.x = leftGroundCheck.transform.position.x + (i * xDistBetweenRays);
            }
            RaycastHit2D hit = Physics2D.Raycast(startPoint, Vector2.down, rayLength, groundLayer);
            RaycastHit2D hitNormal = Physics2D.Raycast(startPoint, hit.normal, rayLength, groundLayer);
            hitOutcomes[i] = hit.collider != null ? true : false;

            Debug.DrawRay(startPoint, Vector2.down * rayLength, Color.yellow);
            Debug.DrawRay(startPoint, hit.normal * rayLength, Color.red);
        }
    }

    //Constantly move the player forwards if canMove is true
    private void MoveCharacter()
    {
        //Add a force to the player
        if (canMove)
        {
            rb.AddForce(new Vector2(acceleration * horizontalDirection * Time.deltaTime, 0f));
        }

        //Limit the players max speed
        if (Mathf.Abs(rb.velocity.x) > speed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * speed, rb.velocity.y);
        }

        if (speed > maxMovementSpeed)
        {
            speed = maxMovementSpeed;
        }
    }

    //Changes the players direction, can be called from anywhere
    private void changeDirection()
    {
        if (horizontalDirection == 1f)
        {
            horizontalDirection = -1f;
        }
        else if (horizontalDirection == -1f)
        {
            horizontalDirection = 1f;
        }



    }

    //Flip the players sprite depending on their facing Direction
    private void FlipSprite()
    {
        if(isFlipped)
        {
            sr.flipX = true;
        } else if(!isFlipped)
        {
            sr.flipX = false;
        }
    }

    //Detect the ground from the raycast information above
    private void DetectGround()
    {
        for (int i = 0; i < rayResolution; i++)
        {
            if (hitOutcomes[i] == true)
            {
                isGrounded = true;
                justWallJumped = false;
                canWallRun = true;
                return;
            }
            else
            {
                isGrounded = false;
            }
        }
    }

    //Add a force to the player when they hit the jump key
    private void Jump()
    {
        if (Input.GetKeyDown("w") && canJump && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
        }
        else if (Input.GetKeyDown("w") && canJump && isTouchingWall && !isGrounded)
        {
            changeDirection();
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            rb.AddForce(new Vector2(2 * horizontalDirection, 0f), ForceMode2D.Impulse);
            speed = speed + 0.5f;
            justWallJumped = true;
        }
    }

    //Determines if the player is able to jump by using raycasting info
    private void CanJump()
    {
        if (isGrounded == true)
        {
            canJump = true;
            canWallRun = true;
        }
        else if (isGrounded == false)
        {
            canJump = false;
        }
    }

    //Lowers the players vertical velocity while moveing downwards on a wall
    private void WallSlide()
    {
        if (!isFlipped)
        {
            RaycastHit2D wall = Physics2D.Raycast(wallCheck.position, Vector2.right, wallRayLength, groundLayer);
            if (wall && rb.velocity.y < 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 1.05f);
                isTouchingWall = true;
                canJump = true;
                canWallRun = true;
            }
            else if (wall)
            {
                isTouchingWall = true;
                canJump = true;
            }
            else
            {
                isTouchingWall = false;
            }
        }
        else if (isFlipped)
        {
            RaycastHit2D wall = Physics2D.Raycast(wallCheck.position, -Vector2.right, wallRayLength, groundLayer);
            if (wall && rb.velocity.y < 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 1.05f);
                isTouchingWall = true;
                canJump = true;
                canWallRun = true;
            }
            else if (wall)
            {
                isTouchingWall = true;
                canJump = true;
            }
            else
            {
                isTouchingWall = false;
            }
        }
    }

    //Run along a wall if certain conditions are met, such as the players velocity, and if they are in contact with a wall based on raycasting info
    private void WallRun()
    {
        //This line below instantiates the raycast, and 'wallHit' is whatever the raycast hits.
        RaycastHit2D wallHit = Physics2D.Raycast(new Vector2(transform.position.x - (transform.localScale.x / 2), transform.position.y), Vector2.right, transform.localScale.x, wallLayer);
        if (wallHit && Input.GetKeyDown("w") && rb.velocity.y < 1f && !isGrounded && canWallRun && this.GetComponent<Grapple>().isGrappling == false)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            WallRunTrigger = true;
            wallRunParticle.Play();
        }
        if (Input.GetKeyUp("w") && isWallRunning && wallHit && !isGrounded)
        {
            WallRunTrigger = false;
            rb.AddForce(new Vector2(0, 3), ForceMode2D.Impulse);
            isWallRunning = false;
            wallRunParticle.Stop();
            //canWallRun = false;
        }
        else if (!wallHit || isGrounded)
        {
            WallRunTrigger = false;
            isWallRunning = false;
            wallRunParticle.Stop();
            //canWallRun = false;
        }
    }

    //Start wall running while a certain variable is true
    IEnumerator executeWallRun()
    {
        while (WallRunTrigger)
        {
            rb.gravityScale = 0.15f;
            isWallRunning = true;
            canWallRun = false;
            speed = speed + 0.005f;
            yield return null;
        }
        while (!WallRunTrigger)
        {
            rb.gravityScale = 1f;
            isWallRunning = false;
            yield return null;
        }
    }

    //Change the ground material to have friction while the player is sliding
    private void slide()
    {
        if (Input.GetKey("s") && isGrounded)
        {
            isSliding = true;
            canMove = false;
            animator.SetBool("isSliding", true);
            rb.sharedMaterial = friction;
            cc.size = new Vector2(1, 1);
            cc.offset = new Vector2(0, -0.5f);
        }  
        else
        {
            isSliding = false;
            canMove = true;
            animator.SetBool("isSliding", false);
            rb.sharedMaterial = frictionless;
            cc.size = new Vector2(1, 2);
            cc.offset = new Vector2(0, 0);
        }

        if(Input.GetKeyDown("s") && isGrounded)
        {
            rb.AddForce(new Vector2(5f, 0f), ForceMode2D.Impulse);
            speed = speed + 0.5f;
        }
    }

    //The actual function that slows down time; utilizes the timescale component
    public void timeSlow(float timeScale)
    {
        if(Time.timeScale == 1f)
        {
            Time.timeScale = timeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    //Removes a certain value from the players speed every frame update, until it reaches the minimum
    IEnumerator speedDecrease()
    {
        while(speed > minMovementSpeed)
        {
            speed = speed - 0.001f;
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
    }

    //Stop the player from moving when they come into contact with the end of the stage
    public void endStage()
    {
        if(cc.IsTouchingLayers(endLayer))
        {
            rb.velocity = new Vector2(0f, 0f);
            canMove = false;
            this.GetComponent<Respawn>().respawnLocation = this.GetComponent<Respawn>().startLocation;
        }
    }
}
