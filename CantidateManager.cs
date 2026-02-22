using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class CandidateManager : MonoBehaviour
{
    public StarRatingSwapFade starUI;
    public GameObject[] cityPeoplePrefabs;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI teamText;
    public AudioSource audioSource;
    public AudioClip hireSound;
    public AudioClip rejectSound;

    public Transform spawnPoint;
    public Transform interviewSeat;

    [Header("Settings")]
    public float animationDelay = 1.5f; // Restore the missing variable for the coroutine
    public int teamSize = 5;

    private GameObject currentCharacter;
    private List<CandidateRecord> hiredTeam = new List<CandidateRecord>();

    private string[] names = { "Amina", "James", "Maria", "Ethan", "Li", "Noah", "Priya", "Jordan", "Kai", "Sam" };
    private string[] pronouns = { "she/her", "he/him", "they/them" };

    void Start()
    {
        if (cityPeoplePrefabs == null || cityPeoplePrefabs.Length == 0)
        {
            Debug.LogError("cityPeoplePrefabs is EMPTY.");
            return;
        }
        if (interviewSeat == null)
        {
            Debug.LogError("interviewSeat is NOT assigned.");
            return;
        }
        if (statsText == null)
        {
            Debug.LogError("statsText is NOT assigned.");
            return;
        }

        SpawnNewCandidate();
        UpdateTeamUI();
        UpdateStarsFromTeam();
    }

    public void NextCandidate(bool hired)
    {
        if (currentCharacter == null) return;

        CandidateStats stats = currentCharacter.GetComponent<CandidateStats>();
        Animator anim = currentCharacter.GetComponent<Animator>();

        if (hired && stats != null && hiredTeam.Count < teamSize)
        {
            CandidateRecord rec = new CandidateRecord
            {
                candidateName = stats.candidateName,
                pronouns = stats.pronouns,
                experienceYears = stats.experienceYears,
                educationTier = stats.educationTier,
                technical = stats.technical,
                adaptability = stats.adaptability,
                cultureAdd = stats.cultureAdd,
                longevity = stats.longevity,
                hiddenPotential = stats.hiddenPotential,
                maintenanceCost = stats.maintenanceCost,
                backgroundType = stats.backgroundType,
                thinkingStyle = stats.thinkingStyle
            };

            hiredTeam.Add(rec);
            UpdateTeamUI();
            UpdateStarsFromTeam();

            if (anim != null) anim.SetTrigger("hired");
            if (audioSource != null && hireSound != null) audioSource.PlayOneShot(hireSound);
        }
        else
        {
            if (anim != null) anim.SetTrigger("nothired");
            if (audioSource != null && rejectSound != null) audioSource.PlayOneShot(rejectSound);
        }

        CandidateMovement mover = currentCharacter.GetComponent<CandidateMovement>();
        if (mover != null) mover.StopAllCoroutines();

        StartCoroutine(ProcessNextCandidate());
    }

    private IEnumerator ProcessNextCandidate()
    {
        GameObject personToRemove = currentCharacter;
        currentCharacter = null; 

        yield return new WaitForSeconds(animationDelay);
        if (personToRemove != null) Destroy(personToRemove);

        if (hiredTeam.Count >= teamSize)
        {
            GlobalData.FinalTeam = new List<CandidateRecord>(hiredTeam);
            SceneManager.LoadScene("End Scene");
        }
        else
        {
            SpawnNewCandidate();
        }
    }

    private void SpawnNewCandidate()
    {
        int randomIndex = Random.Range(0, cityPeoplePrefabs.Length);
        GameObject prefab = cityPeoplePrefabs[randomIndex];

        currentCharacter = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        CandidateStats cs = currentCharacter.AddComponent<CandidateStats>();
        GenerateStats(cs);

        statsText.text =
            $"Name: {cs.candidateName} ({cs.pronouns})\n" +
            $"Experience: {cs.experienceYears} years\n" +
            $"Education Tier: {cs.educationTier}\n\n" +
            $"Technical: {cs.technical}\n" +
            $"Adaptability: {cs.adaptability}\n" +
            $"Culture Add: {cs.cultureAdd}\n" +
            $"Longevity: {cs.longevity}";

        CandidateMovement mover = currentCharacter.GetComponent<CandidateMovement>();
        if (mover == null) mover = currentCharacter.AddComponent<CandidateMovement>();
        StartCoroutine(mover.WalkToDesk(interviewSeat.position));
    }

    private void GenerateStats(CandidateStats c)
    {
        c.candidateName = names[Random.Range(0, names.Length)];
        c.pronouns = pronouns[Random.Range(0, pronouns.Length)];
        c.experienceYears = Random.Range(0, 16);
        c.educationTier = Random.Range(0, 4);

        bool traditional = Random.value < 0.5f;
        c.backgroundType = traditional ? 0 : 1;
        c.thinkingStyle = (Random.value < 0.2f) ? 1 : 0;

        if (traditional)
        {
            c.technical = Random.Range(70, 96);
            c.adaptability = Random.Range(40, 75);
            c.cultureAdd = Random.Range(30, 70);
            c.longevity = Random.Range(25, 70);
            c.hiddenPotential = Random.Range(35, 70);
            c.maintenanceCost = Random.Range(20, 45);
        }
        else
        {
            c.technical = Random.Range(45, 85);
            c.adaptability = Random.Range(70, 96);
            c.cultureAdd = Random.Range(65, 96);
            c.longevity = Random.Range(65, 96);
            c.hiddenPotential = Random.Range(75, 98);
            c.maintenanceCost = Random.Range(5, 20);
        }

        if (c.thinkingStyle == 1)
        {
            c.hiddenPotential = Mathf.Clamp(c.hiddenPotential + 8, 1, 100);
            c.technical = Mathf.Clamp(c.technical + 4, 1, 100);
        }
    }

    private void UpdateTeamUI()
    {
        if (teamText == null) return;

        teamText.text = $"Team ({hiredTeam.Count}/{teamSize})\n";
        for (int i = 0; i < hiredTeam.Count; i++)
        {
            // Fixed typo: changed CandidateRec to CandidateRecord
            CandidateRecord c = hiredTeam[i]; 
            teamText.text += $"{i + 1}. {c.candidateName} ({c.pronouns}) | TP {c.technical} | CA {c.cultureAdd}\n";
        }
    }

    private void UpdateStarsFromTeam()
    {
        if (starUI == null) return;

        float score0to100 = 0f;
        if (hiredTeam.Count > 0)
        {
            float sum = 0f;
            foreach (var c in hiredTeam)
                sum += 0.4f * c.technical + 0.25f * c.adaptability + 0.2f * c.cultureAdd + 0.15f * c.longevity;

            score0to100 = sum / hiredTeam.Count;
        }

        int rating0to4 = Mathf.FloorToInt(Mathf.InverseLerp(0f, 100f, score0to100) * 5f);
        rating0to4 = Mathf.Clamp(rating0to4, 0, 4);

        starUI.SetRating(rating0to4);
    }
}