using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A GameTask that guides the player through a sequence of text blocks.
/// </summary>
public class TextTask : GameTask, IInputHandler
{
    // Interface with the input manager
    public InputManager inputManager;
    public int inputHandlerPriority = 0;

    // The next game task
    public GameTask nextGameTask;

    // Scripts relevant to the text task
    public GameObject canvas;
    public TextAsset tutorialText;
    public TextMeshProUGUI displayText;
    public Button backButton;
    public Button forwardButton;

    // Array that stores our text blocks
    private string[] textBlocks;
    int numTextBlocks;
    int currentTextBlock = 0;

    /// <summary>
    /// Parse the text and set the current text block.
    /// </summary>
    void Awake()
    {
        ParseText();
        SetCurrentTextBlock(0);
    }

    /// <summary>
    /// Sets up the text task.
    /// </summary>
    private void OnEnable()
    {
        // Register task to task manager and input handler
        AutoRegisterTaskToManager();
        inputManager.AddInputHandler(this, inputHandlerPriority);

        // Initialize the task
        canvas.SetActive(true);
        backButton.onClick.AddListener(GoBack);
        forwardButton.onClick.AddListener(GoForward);
    }

    /// <summary>
    /// Cleans up the text task.
    /// </summary>
    private void OnDisable()
    {
        // Complete task and remove it from the input handler
        inputManager.RemoveInputHandler(this);

        // Clean up for the next task
        canvas.SetActive(false);
        backButton.onClick.RemoveAllListeners();
        forwardButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Set the current text block.
    /// </summary>
    /// <param name="index">The desired text block index to access. Transitions to the next game task if index is greater than or equal to the number of text blocks.</param>
    public void SetCurrentTextBlock(int index)
    {
        // If index is less than the maximum, show the appropriate text block
        if (index < numTextBlocks)
        {
            currentTextBlock = Math.Max(Math.Min(index, numTextBlocks - 1), 0);
            displayText.text = textBlocks[currentTextBlock];
        }
        // Otherwise, stop this task and transition to the next
        else
        {
            TransitionToTask(nextGameTask);
        }
    }

    /// <summary>
    /// Go to the text block with specified offset from the current text block.
    /// </summary>
    /// <param name="offset">The specified offset from the current text block.</param>
    public void OffsetCurrentTextBlock(int offset)
    {
        SetCurrentTextBlock(currentTextBlock + offset);
    }

    /// <summary>
    /// Go back one text block.
    /// </summary>
    private void GoBack()
    {
        OffsetCurrentTextBlock(-1);
    }

    /// <summary>
    /// Go forward one text block.
    /// </summary>
    private void GoForward()
    {
        OffsetCurrentTextBlock(1);
    }

    /// <summary>
    /// Return true as long as the text task is active.
    /// </summary>
    /// <returns>True as long as the text task is active.</returns>
    public bool RequestingToHandleInput()
    {
        return true;
    }

    /// <summary>
    /// Parses a string by line into the textBlocks array.
    /// </summary>
    private void ParseText()
    {
        // list to store all of the read lines
        List<string> lines = new List<string>();

        try
        {
            // Create an instance of StreamReader to read from a file
            // The using statement also closes the StreamReader
            using (Stream stream = GenerateStreamFromString(tutorialText.ToString()))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                            lines.Add(line);
                    }
                }

            }

            textBlocks = lines.ToArray();
            numTextBlocks = textBlocks.Length;
        }
        catch (Exception exception)
        {
            lines.Clear();
            numTextBlocks = 1;
            textBlocks = new string[numTextBlocks];
            textBlocks[0] = exception.Message;
        }
    }

    /// <summary>
    /// Creates a memory stream from a string.
    /// </summary>
    /// <param name="s">The string we are opening as a memory stream.</param>
    /// <returns></returns>
    /// <remarks>https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string</remarks>
    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}


    

