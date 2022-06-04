using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovt : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    private float horzMovt;

    [SerializeField] private float jumpSpeed;
    [SerializeField] private float movtSpeed;
    [SerializeField] LayerMask groundLayer;

    private SpriteRenderer playerSprite;
    private Animator playerAnim;

    private enum MovementState{idle, running, jump, fall};
    private MovementState state;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerSprite = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterSpriteController();  //Fliping the sprite and animation state
        playerMovement();   //Movement Script
        playerJump();   //Jump and double jump prevention
       
    }

    void playerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround())
        {
            rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        }
    }

    void playerMovement()
    {
        horzMovt = Input.GetAxisRaw("Horizontal");
        Vector2 movt = new Vector2(horzMovt, 0);

        transform.Translate(movt * movtSpeed * Time.deltaTime);
    }

    void CharacterSpriteController()
    {
        if (Input.GetKey(KeyCode.A))
        {
            playerSprite.flipX = true;
            state = MovementState.running;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            playerSprite.flipX = false;
            state = MovementState.running;
        }
        else
        {
            state = MovementState.idle;
        }

        if(rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }else if(rb.velocity.y < -0.1f)
        {
            state = MovementState.fall;
        }

        playerAnim.SetInteger("state", (int)state);
    }

    private bool isOnGround()
    {
        return Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0.0f, Vector2.down, 0.1f, groundLayer);
    }

}
