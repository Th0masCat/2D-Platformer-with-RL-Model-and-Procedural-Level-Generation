using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOver : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerMovt playerMovtScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerMovtScript = GetComponent<PlayerMovt>();
    }

    private void FixedUpdate()
    {
        if(transform.position.y <= -20f)
        {
            gameOver();
        }
    }

    IEnumerator RestartCoroutine()
    {
        Debug.Log("Coroutine started: " + Time.time);
        
        yield return new WaitForSeconds(0.7f);
        
        Debug.Log("Coroutine Ended: " + Time.time);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            gameOver();
        }
    }

    void gameOver()
    {
        rb.bodyType = RigidbodyType2D.Static;
        playerMovtScript.enabled = false;
        anim.SetTrigger("death");
        CollectibleScript.score = 0;
        StartCoroutine(RestartCoroutine());
    }
}
