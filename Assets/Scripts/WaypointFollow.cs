using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollow : MonoBehaviour
{
    [SerializeField] GameObject[] waypoints;
    [SerializeField] float speed;
    StickyPlatform stickyPlatform;

    private int currentWaypointIndex = 0;

    private void Start()
    {
        stickyPlatform = GetComponent<StickyPlatform>();
    }

    void Update()
    {
        if (stickyPlatform.platformActive){
            if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                    stickyPlatform.platformActive = !stickyPlatform.platformActive;
                }
            }

            transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
        }
    }
        
}
