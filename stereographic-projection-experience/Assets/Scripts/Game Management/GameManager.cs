using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

/// <summary>
/// A MonoBehaviour that initializes the game state and keeps track of game tasks.
/// </summary>
public class GameManager : MonoBehaviour
{
    public UnityEvent OnGameCompleted = new UnityEvent();

    // The set of GameTasks the GameManager is managing
    protected HashSet<GameTask> gameTaskSet = new HashSet<GameTask>();

    /// <summary>
    /// Adds a GameTask to be managed by the GameManager
    /// </summary>
    /// <param name="newTask"></param>
    public void AddTask(GameTask newTask)
    {
        gameTaskSet.Add(newTask);
        newTask.SetGameManager(this);
    }

    public bool ContainsTask(GameTask gameTask)
    {
        return gameTaskSet.Contains(gameTask);
    }

    /// <summary>
    /// Communicates to the GameManager that a task has completed.
    /// </summary>
    /// <param name="completedTask">The task we are letting the GameManager know has completed. Removes it from the game task set and calls CheckGameCompleted().</param>
    public void CompleteTask(GameTask completedTask)
    {
        // Make sure we are managing the completed task
        if (!gameTaskSet.Contains(completedTask))
        {
            Debug.LogError("The game task set does not contain " + completedTask.ToString());
        }

        gameTaskSet.Remove(completedTask);
        CheckGameCompleted();
    }

    /// <summary>
    /// Checks if the game is completed and logs if so.
    /// </summary>
    private void CheckGameCompleted()
    {
        if (gameTaskSet.Count == 0)
        {
            Debug.Log("The game is complete. Quitting the application.");
            Application.Quit();
        }   
    }
}

/// <summary>
/// An abstract class representing a game task managed by a GameManager
/// </summary>
public abstract class GameTask : MonoBehaviour
{
    // Private reference to the game manager
    private GameManager _gameManager;

    /// <summary>
    /// Internal method that allows the game manager to store a reference to itself inside of a game task.
    /// </summary>
    /// <param name="gameManager">The reference to the game manager we are storing.</param>
    internal void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    /// <summary>
    /// Searches for the GameManager and if it finds it stores a reference to it inside of the GameTask.
    /// </summary>
    public void AutoRegisterTaskToManager()
    {
        // If the game manager has not been set
        if (_gameManager == null)
        {
            // Try to find it and set it
            GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
            if (gameManager == null)
                Debug.LogError("Unable to find GameManager.");
            SetGameManager(gameManager);
        }
        // If the task has not been added to the manager
        if (!_gameManager.ContainsTask(this))
        {
            _gameManager.AddTask(this);
        }
    }

    /// <summary>
    /// Signals to the GameManager that the GameTask has completed.
    /// </summary>
    public void CompleteTask()
    {
        _gameManager.CompleteTask(this);
    }

    /// <summary>
    /// Transitions ot the next game task.
    /// </summary>
    /// <param name="nextGameTask">The next game task transition to.</param>
    public void TransitionToTask(GameTask nextGameTask)
    {
        
        gameObject.SetActive(false);

        // If the next game task is not null, transition to it
        if (nextGameTask != null)
        {
            nextGameTask.gameObject.SetActive(true);
            nextGameTask.AutoRegisterTaskToManager();
        }

        CompleteTask();
        GameObject.Destroy(this.gameObject);
    }
}