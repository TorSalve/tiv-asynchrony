/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class QuestionnaireController : MonoBehaviour
{
    private ExperimentController experimentController;
    public GameObject questionnaireCanvas;
    public TMP_Text mainText;
    public TMP_Text smallInstruction;
    public TMP_Text largeInstruction;
    public TMP_Text lowAnchorText;
    public TMP_Text highAnchorText;
    public GameObject scale;
    public List<GameObject> scales;
    public List<int> responses;
    private List<QuestionnaireData> items = new List<QuestionnaireData>();
    
    private GameObject currentScaleGO;
    private bool isStart;
    private bool isAllowedCheck;
    private bool isEnd;
    private int currentScale;
    public int currentItem;
    private StreamWriter questionnaireWriter;

    void Start()
    {
        experimentController = this.GetComponent<ExperimentController>();
        questionnaireCanvas.SetActive(false);
    }

    void Update()
    {
        if (!isStart)
        {
            if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.A))
            {
                if (!isEnd)
                {
                    isStart = true;
                    Debug.LogWarning(1);
                    mainText.text = items[currentItem].item;
                    lowAnchorText.text = items[currentItem].lowAnchor;
                    highAnchorText.text = items[currentItem].highAnchor;
                    scale.SetActive(true);
                    currentScaleGO.SetActive(true);
                    smallInstruction.text = (currentItem + 1).ToString("F0") + "/" + items.Count;
                    largeInstruction.text = "Use Left/Right to select and press A to confirm.";
                }
            }
        }
        else
        {
            if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentScale > 1)
                {
                    currentScaleGO.SetActive(false);
                    currentScale -= 1;
                    scales[currentScale - 1].SetActive(true);
                    currentScaleGO = scales[currentScale - 1];
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentScale < 7)
                {
                    currentScaleGO.SetActive(false);
                    currentScale += 1;
                    scales[currentScale - 1].SetActive(true);
                    currentScaleGO = scales[currentScale - 1];
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.A))
            {
                if (currentItem < items.Count)
                {
                    responses[currentItem] = currentScale;
                    currentScaleGO.SetActive(false);
                    Debug.LogWarning(
                        currentItem                                 + ", " +
                        experimentController.participantID          + ", " +
                        experimentController.vmType                 + ", " +
                        items[currentItem].item                     + ", " +
                        responses[currentItem]                      + ", " +
                        experimentController.currentTime.ToString("F3") );

                    if (!isAllowedCheck)
                    {
                        currentItem += 1;
                        if (currentItem < items.Count-1)
                        {
                            currentScale = 4;
                            currentScaleGO = scales[currentScale - 1];
                        }
                    }
                    else
                    {
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }
                    currentScaleGO.SetActive(true);

                    if (currentItem < items.Count)
                    {
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        smallInstruction.text = (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                    }
                    else
                    {
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + currentItem.ToString("F0") + "/" + items.Count.ToString();
                        largeInstruction.text = "Use Up/Down to check your responses and press B to proceed.";
                        isAllowedCheck = true;
                        currentItem = items.Count-1;
                    }
                }
            }

            if (isAllowedCheck)
            {
                if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (currentItem > 0)
                    {
                        currentItem -= 1;
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }
                }
                else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (currentItem < items.Count-1)
                    {
                        currentItem += 1;
                        smallInstruction.text = "Use Left/Right to select and press A to confirm.\n" + (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
                        mainText.text = items[currentItem].item;
                        lowAnchorText.text = items[currentItem].lowAnchor;
                        highAnchorText.text = items[currentItem].highAnchor;
                        foreach (var s in scales) s.SetActive(false);
                        scales[responses[currentItem] - 1].SetActive(true);
                        currentScale = responses[currentItem];
                        currentScaleGO = scales[responses[currentItem] - 1];
                    }

                }
                else if (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.B))
                {
                    StartCoroutine(WriteQuestionnaireData());
                    Debug.LogWarning("End, Write data");
                    isStart = false;
                    isEnd = true;
                    scale.SetActive(false);
                    smallInstruction.text = "";
                    largeInstruction.text = "";
                    experimentController.isQuestionnaireDone = true;
                    mainText.text = "Complete one condition.\nPlease do XXX to continue.";
                }
            }
        }
    }

    public void InitializeQuestionnaire()
    {
        string questionnairePath = Helpers.CreateDataPath(experimentController.participantID, "_" + experimentController.vmType.ToString());
        questionnaireWriter = new StreamWriter(questionnairePath, true);
        mainText.text = "Please fill out the questionnaire based on the last experience.";
        isStart = false;
        isEnd = false;
        scale.SetActive(false);

        // Initialize SPES questions
        List<QuestionnaireData> spesQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("I felt out of my body.", "never", "always"),
            new QuestionnaireData("I felt as if my (real) body were drifting toward the virtual body or as if the virtual body were drifting toward my (real) body.", "never", "always"),
            new QuestionnaireData("I felt as if the movements of the virtual body were influencing my own movements.", "never", "always"),
            new QuestionnaireData("It felt as if my (real) body were turning into an avatar body.", "never", "always"),
            new QuestionnaireData("At some point it felt as if my real body was starting to take on the posture or shape of the virtual body that I saw.", "never", "always"),
            new QuestionnaireData("I felt like I was wearing different clothes from when I came to the laboratory.", "never", "always"),
            new QuestionnaireData("I felt as if my body had changed.", "never", "always"),
            new QuestionnaireData("I felt a touch sensation in my body when I saw floor.", "never", "always"),
            new QuestionnaireData("I felt that my own body could be affected by floor.", "never", "always"),
            new QuestionnaireData("I felt as if the virtual body was my body.", "never", "always"),
            new QuestionnaireData("At some point it felt that the virtual body resembled my own (real) body in terms of shape skin tone or other visual features.", "never", "always"),
            new QuestionnaireData("I felt as if my body was located where I saw the virtual body.", "never", "always"),
            new QuestionnaireData("I felt like I could control the virtual body as if it was my own body.", "never", "always"),
            new QuestionnaireData("It seemed as if I felt the touch of the floor in the location where I saw the virtual feet touched.", "never", "always"), 
            new QuestionnaireData("It seemed as if the touch I felt was caused by the floor touching the virtual body.", "never", "always"),
            new QuestionnaireData("It seemed as if my body was touching the floor.", "never", "always")
        };

        // Initialize VRSQ questions
        List<QuestionnaireData> vrsqQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("Please rate how much general discomfort affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much fatigue affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much eyestrain affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much difficulty of focusing affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much headache affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much fullness of head affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much blurred vision affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much dizziness affected you during the experience.", "not at all", "very"),
            new QuestionnaireData("Please rate how much vertigo affected you during the experience.", "not at all", "very")
        };

        // Initialize SUS questions
        List<QuestionnaireData> susQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("I think that I would like to use this system frequently.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I found the system unnecessarily complex.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I thought the system was easy to use.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I think that I would need the support of a technical person to be able to use this system.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I found the various functions in this system were well integrated.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I thought there was too much inconsistency in this system.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I would imagine that most people would learn to use this system very quickly.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I found the system very cumbersome to use.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I felt very confident using the system.", "strongly disagree", "strongly agree"),
            new QuestionnaireData("I needed to learn a lot of things before I could get going with this system.", "strongly disagree", "strongly agree")
        };

        // Shuffle each block individually
        Helpers.Shuffle(spesQuestions);
        Helpers.Shuffle(vrsqQuestions);
        Helpers.Shuffle(susQuestions);

        // Combine all questions back into the main list
        items = new List<QuestionnaireData>();
        items.AddRange(spesQuestions);
        items.AddRange(vrsqQuestions);
        items.AddRange(susQuestions);

        responses = new List<int>(new int[items.Count]);
        currentItem = 0;
        currentScale = 4;
        currentScaleGO = scales[currentScale - 1];
        smallInstruction.text = "";
        largeInstruction.text = "Press A to begin.";
        isAllowedCheck = false;

        foreach (var s in scales) s.SetActive(false);
    }

    IEnumerator WriteQuestionnaireData()
    {
        questionnaireWriter.Write(
            "ParticipantID"         + "," +
            "Visuomotor_type"       + "," +
            "Item"                  + "," +
            "Response"              + "\n");

        for (int i = 0; i < responses.Count; i++)
        {
            questionnaireWriter.Write(
                experimentController.participantID                  + "," +
                experimentController.vmType                         + "," +
                items[i].item                                       + "," +
                responses[i]                                        + "\n");
        }
        questionnaireWriter.Flush();
        questionnaireWriter.Close();
        yield return 0;
    }

    public struct QuestionnaireData
    {
        public string item;
        public string lowAnchor;
        public string highAnchor;

        public QuestionnaireData(string item, string lowAnchor, string highAnchor)
        {
            this.item = item;
            this.lowAnchor = lowAnchor;
            this.highAnchor = highAnchor;
        }
    }
}

public static class Helpers
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int n = 0; n < list.Count; n++)
        {
            T tmp = list[n];
            int r = UnityEngine.Random.Range(n, list.Count);
            list[n] = list[r];
            list[r] = tmp;
        }
    }

    public static string CreateDataPath(int id, string note = "")
    {
        string fileName = "P" + id.ToString() + note + ".csv";
#if UNITY_EDITOR
        return Application.dataPath + "/Data/" + fileName;
#elif UNITY_ANDROID
        return Application.persistentDataPath + fileName;
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/" + fileName;
#else
        return Application.dataPath + "/" + fileName;
#endif
    }

    public static float RandomGaussian(float minValue = 0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    public static float DegreeToRadian(float deg)
    {
        return deg * Mathf.PI / 180;
    }
}*/
