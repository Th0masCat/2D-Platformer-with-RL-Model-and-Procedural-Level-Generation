using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;
using TMPro;

public class IntelligentAgentScript : Agent
{
    [SerializeField] private Transform treasure;
    [SerializeField] private Transform spikes;

    static public int score = 0;
    [SerializeField] TextMeshProUGUI scoreBox;

    CharacterState characterState;

    private void Start()
    {
        scoreBox.text = "Score: " + score;
        characterState = GetComponent<CharacterState>();
    }

    public override void OnEpisodeBegin()
    {
        float[] possiblePositionX = new float[] { -8.78f, 5.29f };
        int randomIndex = Random.Range(0, possiblePositionX.Length);
        float positionX = possiblePositionX[randomIndex];

        treasure.transform.localPosition = new Vector3(positionX, -3.550007f, 0);
        spikes.transform.localPosition = (randomIndex == 0) ? new Vector3(possiblePositionX[1], -3.78f, 0) : new Vector3(possiblePositionX[0], -3.78f, 0);

        transform.localPosition = new Vector3(-2.05062f, -1.980007f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(treasure.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        characterState.horizontal = actions.ContinuousActions[0];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Collectibles"))
        {
            Debug.Log("Object Collected");
            score++;
            scoreBox.text = "Score: " + score;
            SetReward(+20f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("Ran into a trap");
            SetReward(-20f);
            EndEpisode();
        }
    }
}
