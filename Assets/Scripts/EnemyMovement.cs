using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    CharacterState characterState;
    [Range(0, 100f)][SerializeField] private float speed = 0f;
    Animator enemyAnim;

    void Start()
    {
        characterState = GetComponent<CharacterState>();
        enemyAnim = GetComponent<Animator>();
    }

    void Update()
    {
        if (characterState.horizontal > 0 || characterState.horizontal < 0)
        {
            enemyAnim.SetInteger("state", 1);
        }
        else
        {
            enemyAnim.SetInteger("state", 0);
        }
    }

    void FixedUpdate()
    {
        // Calculate the move factor at this step
        float moveFactor = characterState.horizontal * Time.deltaTime * speed;

        // Flipping sprite according to movement direction...
        if (moveFactor > 0 && !characterState.isFacingRight) flipSprite();
        else if (moveFactor < 0 && characterState.isFacingRight) flipSprite();

        // Let's move!
        characterState.rigidBody2D.velocity = new Vector2(moveFactor, characterState.rigidBody2D.velocity.y);

    }

    private void flipSprite()
    {
        characterState.isFacingRight = !characterState.isFacingRight;
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }
}
