using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChinaSpriteNavigator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject playerCharacterObject; // Your PlayerCharacter GameObject
    [SerializeField] private GameObject descriptionObject;     // Your Description GameObject
    [SerializeField] private GameObject nameObject;            // Your Name GameObject
    [SerializeField] private Button nextButton;               // Your right arrow button
    [SerializeField] private Button backButton;               // Your left arrow button (optional)
    
    [Header("Character Data")]
    [SerializeField] private CharacterData[] characters;
    
    private SpriteRenderer characterSpriteRenderer;
    private Image characterImage;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI nameText;
    
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite characterSprite;
        [TextArea(4, 8)]
        public string description;
    }
    private int currentIndex = 0;
    
    void Start()
    {
        // Get components from the assigned GameObjects
        characterSpriteRenderer = playerCharacterObject.GetComponent<SpriteRenderer>();
        characterImage = playerCharacterObject.GetComponent<Image>();
        descriptionText = descriptionObject.GetComponent<TextMeshProUGUI>();
        nameText = nameObject.GetComponent<TextMeshProUGUI>();
        
        // Validate components - we need either SpriteRenderer OR Image
        if (characterSpriteRenderer == null && characterImage == null)
        {
            Debug.LogError("Neither SpriteRenderer nor Image component found on PlayerCharacter GameObject! Please add one of these components.");
            return;
        }
        
        if (descriptionText == null)
        {
            Debug.LogError("TextMeshProUGUI not found on Description GameObject!");
            return;
        }
        
        if (nameText == null)
        {
            Debug.LogError("TextMeshProUGUI not found on Name GameObject!");
            return;
        }
        
        // Log which component we're using
        if (characterSpriteRenderer != null)
        {
            Debug.Log("Using SpriteRenderer for character display");
        }
        else
        {
            Debug.Log("Using Image component for character display");
        }
        
        // Setup button click events
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNext);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowPrevious);
        }
        
        // Validate character data
        if (characters.Length == 0)
        {
            Debug.LogWarning("No character data assigned!");
            return;
        }
        
        // Show first character
        ShowCurrentCharacter();
    }
    
    public void ShowNext()
    {
        // Cycle to next index (loop back to 0 if at end)
        currentIndex = (currentIndex + 1) % characters.Length;
        ShowCurrentCharacter();
    }
    
    public void ShowPrevious()
    {
        // Cycle to previous index (loop to end if at beginning)
        currentIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        ShowCurrentCharacter();
    }
    
    public void ShowCharacter(int index)
    {
        // Show specific character by index
        if (index >= 0 && index < characters.Length)
        {
            currentIndex = index;
            ShowCurrentCharacter();
        }
    }
    
    private void ShowCurrentCharacter()
    {
        // Check if we have valid data
        if (characters.Length == 0)
        {
            Debug.LogWarning("No characters assigned!");
            return;
        }
        
        CharacterData currentCharacter = characters[currentIndex];
        
        // Update character sprite (try both SpriteRenderer and Image)
        if (currentCharacter.characterSprite != null)
        {
            if (characterSpriteRenderer != null)
            {
                characterSpriteRenderer.sprite = currentCharacter.characterSprite;
            }
            else if (characterImage != null)
            {
                characterImage.sprite = currentCharacter.characterSprite;
            }
        }
        
        // Update description text
        if (descriptionText != null)
        {
            descriptionText.text = currentCharacter.description;
        }
        
        // Update name text
        if (nameText != null)
        {
            nameText.text = currentCharacter.characterName;
        }
        
        // Optional: Log current character info
        Debug.Log($"Showing character: {currentCharacter.characterName} ({currentIndex + 1}/{characters.Length})");
    }
    
    // Public getters for current status
    public int CurrentIndex => currentIndex;
    public int TotalCharacters => characters.Length;
    public string CurrentCharacterName => characters.Length > 0 ? characters[currentIndex].characterName : "None";
}