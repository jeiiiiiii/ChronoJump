using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldHandler : MonoBehaviour
{
    [Header("Input Field Reference")]
    public TMP_InputField inputField;
    
    [Header("AI Integration")]
    public UnityAndGeminiV3 geminiScript;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the input field automatically if not assigned
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }
        
        // Try to find the Gemini script automatically if not assigned
        if (geminiScript == null)
        {
            geminiScript = FindObjectOfType<UnityAndGeminiV3>();
        }
        
        // Check if we found the input field
        if (inputField != null)
        {
            // Listen for when user presses Enter
            inputField.onSubmit.AddListener(OnEnterKeyPressed);
            
            // Make sure Enter submits instead of creating new lines
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            
            Debug.Log("InputFieldHandler is ready to go!");
        }
        else
        {
            Debug.LogError("No input field found! Make sure this script is on an Input Field or assign one manually.");
        }
        
        // Check if we found the Gemini script
        if (geminiScript == null)
        {
            Debug.LogError("No UnityAndGeminiV3 script found! Please assign it manually in the inspector.");
        }
        else
        {
            Debug.Log("Gemini AI integration connected!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // This method runs when user presses Enter
    void OnEnterKeyPressed(string userText)
    {
        HandleUserInput();
    }
    
    // Handle what happens when user sends a message
    void HandleUserInput()
    {
        // Get the text the user typed
        string userMessage = inputField.text.Trim();
        
        // Check if the message is not empty
        if (string.IsNullOrEmpty(userMessage))
        {
            Debug.Log("User tried to send empty message");
            return;
        }
        
        // Show what the user typed in the console
        Debug.Log("USER SAID: " + userMessage);
        
        // Clear the input field
        inputField.text = "";
        
        // Keep the input field focused so user can type again
        inputField.ActivateInputField();
        
        // Send message to Gemini AI
        if (geminiScript != null)
        {
            // Set the input field text for the Gemini script and trigger send
            geminiScript.inputField.text = userMessage;
            geminiScript.SendChat();
            Debug.Log("Message sent to Gemini AI: " + userMessage);
        }
        else
        {
            Debug.LogError("Gemini script not connected! Cannot send message to AI.");
        }
    }
}