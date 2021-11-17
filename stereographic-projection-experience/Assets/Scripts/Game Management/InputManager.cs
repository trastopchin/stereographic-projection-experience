using System.Collections.Generic;
using System.Linq; // For List.Max() etc.
using UnityEngine;

/// <summary>
/// A MonoBehaviour that ensures each participating script individually manages input.
/// </summary>
public class InputManager : MonoBehaviour
{
    // A dictionary from input handlers to priority
    private Dictionary<IInputHandler, int> inputHandlers = new Dictionary<IInputHandler, int>();
    // The controlling input handler
    private IInputHandler controllingInputHandler = null;
    // A lazily-computed list of input handlers requesting to handle input this frame
    private bool forceCheckedRequestsToHandleInput = false;
    private SortedList<int, IInputHandler> forceCheckedRequestingInputHandlers = new SortedList<int, IInputHandler>();

    /// <summary>
    /// Adds an input handler to manage.
    /// </summary>
    /// <param name="inputHandler">The input handler that we are adding to the input handler hash set and sorted list.</param>
    public void AddInputHandler(IInputHandler inputHandler, int priority)
    {
        if (inputHandlers.ContainsValue(priority))
        {
            Debug.LogError("Attempting to insert duplicate priority IInputHandler into the InputManager. Each managed IInputHandlers must have a distinct priority!");
        }
        inputHandlers.Add(inputHandler, priority);
    }

    public void RemoveInputHandler(IInputHandler inputHandler)
    {
        if (!inputHandlers.ContainsKey(inputHandler))
        {
            Debug.LogError("Attempting to remove IInputHandler not managed by InputManager.");
        }
        inputHandlers.Remove(inputHandler);
    }

    /// <summary>
    /// Each frame we reset this so we can lazily-compute the list of requesting input handlers.
    /// </summary>
    private void Update()
    {
        forceCheckedRequestsToHandleInput = false;
    }

    /// <summary>
    /// Requests the input handler to handle input.
    /// </summary>
    /// <param name="requestingInputHandler">The input handler that is requesting to manage input.</param>
    /// <returns>Whether or not the request to handle input was successful.</returns>
    public bool RequestToHandleInput(IInputHandler requestingInputHandler)
    {
        // Make sure inputHandler is our dictionary
        if (inputHandlers.ContainsKey(requestingInputHandler))
        {
            // If we not already checked all the managed input handlers
            if (!forceCheckedRequestsToHandleInput)
            {
                // Force check every single input handler
                ForceCheckRequestsToHandleInput();

                // If no one signaled true to wanting to handling input
                if (forceCheckedRequestingInputHandlers.Count == 0)
                {
                    // Return false
                    return false;
                }

                // Find the new controlling input handler
                // This works as long as they all have different priorities
                int maximumKey = forceCheckedRequestingInputHandlers.Keys.Max();
                IInputHandler maximumPriorityInputHandler = forceCheckedRequestingInputHandlers[maximumKey];

                // Assign the maximum priority input handler
                controllingInputHandler = maximumPriorityInputHandler;
            }

            // At this point test if we are the controlling input handler
            return controllingInputHandler == requestingInputHandler;
        }
        // If it's not in our dictionary return false
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Forces the manager to iterate through all managed input handlers and check if they are requesting to handle input.
    /// </summary>
    private void ForceCheckRequestsToHandleInput()
    {
        forceCheckedRequestsToHandleInput = true;
        forceCheckedRequestingInputHandlers.Clear();
        // Itreate through all of the input handlers
        foreach (KeyValuePair<IInputHandler, int> keyValuePair in inputHandlers)
        {
            // If requesting input, add to list sorted by priority
            if (keyValuePair.Key.RequestingToHandleInput())
            {
                forceCheckedRequestingInputHandlers.Add(keyValuePair.Value, keyValuePair.Key);
            }
        }
    }
}

/// <summary>
/// A helper interface for scripts that want to be managed by this script.
/// </summary>
public interface IInputHandler
{
    bool RequestingToHandleInput();
}