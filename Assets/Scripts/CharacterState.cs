using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState : MonoBehaviour
{
    public Rigidbody2D rigidBody2D;
    public float horizontal = 0f;
    public bool isFacingRight = true;

    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
    }
}
