using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TetrisAgent : Agent
{
    public TetrisGame tetrisGame;

    // Reset the environment
    public override void OnEpisodeBegin()
    {
        tetrisGame.ResetGame();
    }

    // Provide info to agent needed to make a decision
    public override void CollectObservations(VectorSensor sensor)
    {
        int id = tetrisGame.activeTetromino.tetrominoShape.getId(); // Give agent current tetromino shape ID
        for (int i = 0; i < 7; i++)
        {
            sensor.AddObservation(i == id ? 1f : 0f);
        }
        int rot = tetrisGame.activeTetromino.rotation%4; // Give agent current rotation
        for (int i = 0; i < 4; i++)
        {
            sensor.AddObservation(i == rot ? 1f : 0f);
        }
        //sensor.AddObservation(tetrisGame.tickRate); // get tick rate until next drop
        sensor.AddObservation(tetrisGame.activeTetromino.position.x / (float)(TetrisGame.WIDTH - 1)); // Add current width position in 0-1 clamp
        sensor.AddObservation(tetrisGame.activeTetromino.position.y / (float)(TetrisGame.HEIGHT - 1)); // Add current height position in 0-1 clamp
        sensor.AddObservation(tetrisGame.GetBoardObservation()); // Add Board view
    }

    // Decide reward
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float reward = 0;
        if(actionBuffers.DiscreteActions[0] == 1) // Move Left
        {
            reward = tetrisGame.AttemptMove(-1, 0);
        }
        else if (actionBuffers.DiscreteActions[0] == 2) // Move Right
        {
            reward = tetrisGame.AttemptMove(1, 0);
        }
        else if (actionBuffers.DiscreteActions[0] == 3) // Move Down
        {
            reward = tetrisGame.AttemptMove(0, 1);
        }
        else if (actionBuffers.DiscreteActions[0] == 4) // Rotate
        {
            tetrisGame.AttemptRotate();
        }
        if(reward>float.Epsilon)
        {
            AddReward(reward);
        }
        if (tetrisGame.GameLost)
        {
            EndEpisode();
        }
    }

    // Heuristic to allow for manual control of the agent (Heuristic Only mode)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        if(Input.GetKey(KeyCode.LeftArrow)) { actions[0] = 1; }
        else if (Input.GetKey(KeyCode.RightArrow)) { actions[0] = 2; }
        else if (Input.GetKey(KeyCode.DownArrow)) { actions[0] = 3; }
        else if (Input.GetKey(KeyCode.UpArrow)) { actions[0] = 4; }
    }
}
