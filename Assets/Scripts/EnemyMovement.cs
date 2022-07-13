using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    CharacterState characterState;
    [Range(0, 100f)][SerializeField] private float speed = 0f;

    void Start()
    {
        characterState = GetComponent<CharacterState>();
    }

    void Update()
    {
        if (characterState.horizontal > 0 || characterState.horizontal < 0)
        {
            GetComponent<Animator>().Play("Player_Running");
        }
        else
        {
            GetComponent<Animator>().Play("Player_Idle");
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
