using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StorySubtitle
{
    [TextArea(2, 4)]
    public string text;
    public float displayDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
}

[System.Serializable]
public class CameraShot
{
    public CinemachineVirtualCamera virtualCamera;
    public float duration = 3f;
    public float blendDuration = 1f;
}

public class StoryEventController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private List<CameraShot> cameraShots = new List<CameraShot>();
    
    [Header("Subtitle Settings")]
    [SerializeField] private List<StorySubtitle> subtitles = new List<StorySubtitle>();
    
    [Header("Events")]
    public UnityEvent OnEventStarted;
    public UnityEvent OnEventEnded;
    
    [Header("Optional")]
    [SerializeField] private bool returnToPlayerCamera = true;
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    
    private bool isPlaying = false;
    private int currentShotIndex = 0;
    private int currentSubtitleIndex = 0;
    private StoryKeyPoint storyKeyPoint;
    
    public bool IsPlaying => isPlaying;
    
    private void Awake()
    {
        // Auto-find CinemachineBrain if not assigned
        if (cinemachineBrain == null)
        {
            cinemachineBrain = FindObjectOfType<CinemachineBrain>();
        }
        
        storyKeyPoint = GetComponent<StoryKeyPoint>();
        
        // Disable all virtual cameras except player camera
        InitializeCameras();
    }
    
    private void InitializeCameras()
    {
        foreach (var shot in cameraShots)
        {
            if (shot.virtualCamera != null)
            {
                shot.virtualCamera.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Start playing the story event
    /// </summary>
    public void PlayEvent()
    {
        if (isPlaying)
        {
            Debug.LogWarning("[StoryEventController] Event is already playing!");
            return;
        }
        
        StartCoroutine(PlayEventCoroutine());
    }
    
    private IEnumerator PlayEventCoroutine()
    {
        isPlaying = true;
        OnEventStarted?.Invoke();
        
        // Start subtitle display
        StartCoroutine(PlaySubtitlesCoroutine());
        
        // Play camera shots
        yield return StartCoroutine(PlayCameraShotsCoroutine());
        
        // Wait for subtitles to finish if they're still playing
        yield return new WaitForSeconds(0.5f);
        
        EndEvent();
    }
    
    private IEnumerator PlayCameraShotsCoroutine()
    {
        currentShotIndex = 0;
        
        while (currentShotIndex < cameraShots.Count)
        {
            CameraShot shot = cameraShots[currentShotIndex];
            
            if (shot.virtualCamera != null)
            {
                // Activate this camera
                shot.virtualCamera.gameObject.SetActive(true);
                
                // Wait for the shot duration
                yield return new WaitForSeconds(shot.duration);
                
                // Deactivate this camera
                shot.virtualCamera.gameObject.SetActive(false);
            }
            
            currentShotIndex++;
            
            // Small delay between shots
            if (currentShotIndex < cameraShots.Count)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    private IEnumerator PlaySubtitlesCoroutine()
    {
        currentSubtitleIndex = 0;
        
        while (currentSubtitleIndex < subtitles.Count)
        {
            StorySubtitle subtitle = subtitles[currentSubtitleIndex];
            
            // Display subtitle
            StorySubtitleUI.Instance?.ShowSubtitle(subtitle);
            
            // Wait for display duration + fade in/out
            yield return new WaitForSeconds(subtitle.displayDuration + subtitle.fadeInDuration + subtitle.fadeOutDuration);
            
            currentSubtitleIndex++;
        }
    }
    
    /// <summary>
    /// End the event and return control to player
    /// </summary>
    public void EndEvent()
    {
        if (!isPlaying)
            return;
        
        isPlaying = false;
        
        // Return to player camera
        if (returnToPlayerCamera && playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }
        
        // Hide any remaining subtitles
        StorySubtitleUI.Instance?.HideSubtitle();
        
        OnEventEnded?.Invoke();
        
        // Notify StoryKeyPoint to end
        if (storyKeyPoint != null)
        {
            storyKeyPoint.EndEvent();
        }
        
        Debug.Log("[StoryEventController] Event ended.");
    }
    
    /// <summary>
    /// Skip to the end of the event
    /// </summary>
    public void SkipEvent()
    {
        StopAllCoroutines();
        EndEvent();
    }
    
    // Helper method to set player camera reference
    public void SetPlayerCamera(CinemachineVirtualCamera camera)
    {
        playerCamera = camera;
    }
}
