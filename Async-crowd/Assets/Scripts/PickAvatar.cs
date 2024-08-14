using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickAvatar : MonoBehaviour
{
    public bool isStartFlagOn;
    public string sceneToLoad;

    public GameObject[] instructionPages; // Array to hold the instruction pages
    public AudioClip[] instructionAudioClips; // Array to hold the audio clips for each page
    public GameObject avatarSelection; // Reference to the Avatar Selection game object

    private int currentPageIndex = 0; // Keeps track of the current page
    private AudioSource audioSource; // AudioSource to play the audio clips

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // Add AudioSource component to the same GameObject
        ShowCurrentInstructionPage();
        avatarSelection.SetActive(false);
    }

    private void Update()
    {
        // Detect if any button is pressed
        if (OVRInput.GetDown(OVRInput.Button.Any)) 
        {
            ShowNextInstructionPage();
        }

        if (isStartFlagOn)
        {
            isStartFlagOn = false;
            StartBoxPressed();
        }
    }

    private void ShowNextInstructionPage()
    {
        currentPageIndex++;

        if (currentPageIndex >= instructionPages.Length)
        {
            currentPageIndex = instructionPages.Length - 1;
        }

        ShowCurrentInstructionPage();
    }

    private void ShowCurrentInstructionPage()
    {
        for (int i = 0; i < instructionPages.Length; i++)
        {
            instructionPages[i].SetActive(i == currentPageIndex);
        }

        if (instructionAudioClips.Length > currentPageIndex && instructionAudioClips[currentPageIndex] != null)
        {
            audioSource.clip = instructionAudioClips[currentPageIndex];
            audioSource.Play();
        }

        if (currentPageIndex == instructionPages.Length - 1)
        {
            avatarSelection.SetActive(true);
        }
        else
        {
            avatarSelection.SetActive(false);
        }
    }

    private void StartBoxPressed()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // New method to handle button click and start the scene transition
    public void OnAvatarButtonClicked()
    {
        isStartFlagOn = true;
    }
}
