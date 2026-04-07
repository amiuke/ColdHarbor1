using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StorySubtitleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshPro text component for displaying subtitles")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    
    [Tooltip("Background panel GameObject (will be enabled/disabled)")]
    [SerializeField] private GameObject backgroundPanel;
    
    private Coroutine currentSubtitleCoroutine;
    private static StorySubtitleUI _instance;
    
    public static StorySubtitleUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StorySubtitleUI>();
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Validate references
        if (subtitleText == null || backgroundPanel == null)
        {
            Debug.LogError("[StorySubtitleUI] Please assign subtitleText and backgroundPanel in the Inspector!");
            enabled = false;
            return;
        }
        
        // Start hidden
        backgroundPanel.SetActive(false);
    }
    
    public void ShowSubtitle(StorySubtitle subtitle)
    {
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
        }
        
        currentSubtitleCoroutine = StartCoroutine(ShowSubtitleCoroutine(subtitle));
    }
    
    private IEnumerator ShowSubtitleCoroutine(StorySubtitle subtitle)
    {
        if (subtitleText == null || backgroundPanel == null)
            yield break;
        
        // Set text
        subtitleText.text = subtitle.text;
        
        // Show panel
        backgroundPanel.SetActive(true);
        
        // Wait for display duration
        yield return new WaitForSeconds(subtitle.displayDuration);
        
        // Hide panel
        backgroundPanel.SetActive(false);
        
        // Clear text
        subtitleText.text = "";
    }
    
    public void HideSubtitle()
    {
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
            currentSubtitleCoroutine = null;
        }
        
        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(false);
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }
    }
    
    public void ShowSimpleSubtitle(string text, float duration)
    {
        StorySubtitle subtitle = new StorySubtitle
        {
            text = text,
            displayDuration = duration,
            fadeInDuration = 0f,
            fadeOutDuration = 0f
        };
        
        ShowSubtitle(subtitle);
    }
}
