using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName = "Main Scene"; 
    public float waitTime = 5.0f;

    [Header("Character Animations")]
    // An array to hold all 6 characters
    public Animator[] characterAnimators; 
    public string cutsceneTriggerName = "isWalking"; 

    void Start()
    {
        // 1. Loop through every animator in the list and trigger the animation
        foreach (Animator anim in characterAnimators)
        {
            if (anim != null)
            {
                anim.SetTrigger(cutsceneTriggerName);
            }
        }

        // 2. Start the timer to change scenes
        StartCoroutine(WaitAndLoad());
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(nextSceneName);
    }
}