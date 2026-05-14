using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 1.5f;
    private float distanceTraveled;

    [Header("Sprites: Walking")]
    public Sprite[] walkUp;
    public Sprite[] walkDown;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;

    [Header("Sprites: Idle")]
    public Sprite[] idleUp;
    public Sprite[] idleDown;
    public Sprite[] idleLeft;
    public Sprite[] idleRight;

    [Header("Animation Settings")]
    public float walkAnimationSpeed = 0.1f;
    public float idleAnimationSpeed = 0.2f;
    public float idleDelay = 0.2f;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;

    private float idleTimer;
    private float animationTimer;
    private int frameIndex;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            idleTimer = 0f;
            lastDirection = movement;

            AnimateCharacter(GetCurrentAnimation(), walkAnimationSpeed);
            HandleFootsteps();
        }
        else
        {
            idleTimer += Time.deltaTime;
            distanceTraveled = 0f;
            if (idleTimer >= idleDelay)
            {
                AnimateCharacter(GetIdleAnimation(), idleAnimationSpeed);
            }
            else
            {
                Sprite[] idleFrames = GetIdleAnimation();
                if (idleFrames.Length > 0)
                {
                    spriteRenderer.sprite = idleFrames[0];
                }
                animationTimer = 0f;
                frameIndex = 0;
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 nextPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        
        if (movement != Vector2.zero)
        {
            distanceTraveled += Vector2.Distance(rb.position, nextPosition);
        }

        rb.MovePosition(nextPosition);
    }

    void HandleFootsteps()
    {
        if (distanceTraveled >= stepInterval)
        {
            PlayFootstep();
            distanceTraveled = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length > 0 && footstepSource != null)
        {
            int index = Random.Range(0, footstepSounds.Length);
            footstepSource.PlayOneShot(footstepSounds[index]);
        }
    }

    void AnimateCharacter(Sprite[] currentAnimation, float speed)
    {
        if (currentAnimation.Length == 0) return;

        animationTimer += Time.deltaTime;

        if (animationTimer >= speed)
        {
            animationTimer = 0f;
            frameIndex++;

            if (frameIndex >= currentAnimation.Length)
                frameIndex = 0;

            spriteRenderer.sprite = currentAnimation[frameIndex];
        }
    }

    Sprite[] GetCurrentAnimation()
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            return movement.x > 0 ? walkRight : walkLeft;
        }
        else
        {
            return movement.y > 0 ? walkUp : walkDown;
        }
    }

    Sprite[] GetIdleAnimation()
    {
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            return lastDirection.x > 0 ? idleRight : idleLeft;
        }
        else
        {
            return lastDirection.y > 0 ? idleUp : idleDown;
        }
    }
}