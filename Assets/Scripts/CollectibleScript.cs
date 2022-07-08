using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectibleScript : MonoBehaviour
{
    static public int score = 0;
    [SerializeField] TextMeshProUGUI scoreBox;
    private Animator collectibleAnim;

    private void FixedUpdate()
    {
        scoreBox.text = "Score: " + score;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collectibles"))
        {
            collectibleAnim = collision.GetComponent<Animator>();
            collectibleAnim.SetTrigger("collected");
            Destroy(collision.gameObject);
            score++;
            scoreBox.text = "Score: " + score;
        }

        if (collision.gameObject.CompareTag("Arrows"))
        {
            collectibleAnim = collision.GetComponent<Animator>();
            collectibleAnim.SetTrigger("OnCollision");
        }
    }
}
