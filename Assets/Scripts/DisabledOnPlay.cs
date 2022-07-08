using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledOnPlay : MonoBehaviour
{
    private SpriteRenderer gameObj;
     void Awake()
    {
        gameObject.SetActive(false);
    }

}
