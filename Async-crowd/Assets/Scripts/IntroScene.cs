using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace AsyncCrowd
{
    public class IntroScene : MonoBehaviour
    {
        [Header("VR Controller Input")]
        [SerializeField] private InputActionReference nextInstructionPageAction;

        public bool isStartFlagOn;
        public string sceneToLoad;

        public GameObject[] instructionPages; // Array to hold the instruction pages
        public GameObject avatarSelection; // Reference to the Avatar Selection game object
        private int currentPageIndex = 0; // Keeps track of the current page

        private void Start()
        {
            ShowCurrentInstructionPage();
            avatarSelection.SetActive(false);
        }

        private void Update()
        {
            if (isStartFlagOn)
            {
                isStartFlagOn = false;
                StartBoxPressed();
            }
        }

        private void OnEnable()
        {
            nextInstructionPageAction.action.performed += OnNextInstructionPage;
        }

        private void OnDisable()
        {
            nextInstructionPageAction.action.performed -= OnNextInstructionPage;
        }

        private void OnNextInstructionPage(InputAction.CallbackContext context)
        {
            ShowNextInstructionPage();
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
}