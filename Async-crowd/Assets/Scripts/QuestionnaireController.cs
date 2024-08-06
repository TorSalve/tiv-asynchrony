using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class QuestionnaireController : MonoBehaviour
{
    private SingleAvatar singleAvatar;
    public GameObject questionnaireCanvas;
    public TMP_Text mainText;
    public TMP_Text smallInstruction;
    public TMP_Text largeInstruction;
    public TMP_Text lowAnchorText;
    public TMP_Text highAnchorText;
    public List<GameObject> spesScales;
    public List<GameObject> vrsqScales;
    public List<GameObject> susScales;
    public List<GameObject> sexScales;
    public List<GameObject> ageScales;
    public GameObject spesScaleGO;
    public GameObject vrsqScaleGO;
    public GameObject susScaleGO;
    public GameObject sexScaleGO;
    public GameObject ageScaleGO;
    public List<int> responses;
    private List<QuestionnaireData> items = new List<QuestionnaireData>();

    private List<QuestionnaireData> spesQuestions;
    private List<QuestionnaireData> vrsqQuestions;
    private List<QuestionnaireData> susQuestions;
    private List<QuestionnaireData> sexQuestions;
    private List<QuestionnaireData> ageQuestions;

    private GameObject currentScaleGO;
    private List<GameObject> currentScales;
    private bool isStart;
    private bool isAllowedCheck;
    private bool isEnd;
    private int currentSpesScale;
    private int currentVrsqScale;
    private int currentSusScale;
    private int currentSexScale;
    private int currentAgeScale;
    private int currentScale;
    public int currentItem;
    private StreamWriter questionnaireWriter;

    void Start()
    {
        singleAvatar = this.GetComponent<SingleAvatar>();
        questionnaireCanvas.SetActive(false);

        // Ensure all scales are initially disabled
        SetScalesActive(spesScales, false);
        SetScalesActive(vrsqScales, false);
        SetScalesActive(susScales, false);
        SetScalesActive(sexScales, false);
        SetScalesActive(ageScales, false);
        spesScaleGO.SetActive(false);
        vrsqScaleGO.SetActive(false);
        susScaleGO.SetActive(false);
        sexScaleGO.SetActive(false);
        ageScaleGO.SetActive(false);
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
                    InitializeCurrentQuestion();
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
                    currentScaleGO = currentScales[currentScale - 1];
                    currentScaleGO.SetActive(true);
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentScale < currentScales.Count)
                {
                    currentScaleGO.SetActive(false);
                    currentScale += 1;
                    currentScaleGO = currentScales[currentScale - 1];
                    currentScaleGO.SetActive(true);
                }
            }
            else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.A))
            {
                if (currentItem < items.Count)
                {
                    responses[currentItem] = currentScale;
                    currentScaleGO.SetActive(false);

                    Debug.LogWarning(
                        currentItem + ", " +
                        singleAvatar.participantID + ", " +
                        singleAvatar.VMType + ", " +
                        items[currentItem].item + ", " +
                        responses[currentItem] + ", " +
                        singleAvatar.currentTime.ToString("F3"));

                    currentItem += 1;

                    if (currentItem < items.Count)
                    {
                        InitializeCurrentQuestion();
                    }
                    else
                    {
                        StartCoroutine(EndQuestionnaire());
                    }
                }
            }
        }
    }

    private void InitializeCurrentQuestion()
    {
        DetermineCurrentScales();

        currentScaleGO = currentScales[currentScale - 1];
        currentScaleGO.SetActive(true);

        mainText.text = items[currentItem].item;
        lowAnchorText.text = items[currentItem].lowAnchor;
        highAnchorText.text = items[currentItem].highAnchor;
        smallInstruction.text = (currentItem + 1).ToString("F0") + "/" + items.Count.ToString();
    }

    private void DetermineCurrentScales()
    {
        SetScalesActive(spesScales, false);
        SetScalesActive(vrsqScales, false);
        SetScalesActive(susScales, false);
        SetScalesActive(sexScales, false);
        SetScalesActive(ageScales, false);

        spesScaleGO.SetActive(false);
        vrsqScaleGO.SetActive(false);
        susScaleGO.SetActive(false);
        sexScaleGO.SetActive(false);
        ageScaleGO.SetActive(false);

        if (spesQuestions.Contains(items[currentItem]))
        {
            currentScales = spesScales;
            currentScale = currentSpesScale;
            spesScaleGO.SetActive(true);
        }
        else if (vrsqQuestions.Contains(items[currentItem]))
        {
            currentScales = vrsqScales;
            currentScale = currentVrsqScale;
            vrsqScaleGO.SetActive(true);
        }
        else if (susQuestions.Contains(items[currentItem]))
        {
            currentScales = susScales;
            currentScale = currentSusScale;
            susScaleGO.SetActive(true);
        }
        else if (sexQuestions.Contains(items[currentItem]))
        {
            currentScales = sexScales;
            currentScale = currentSexScale;
            sexScaleGO.SetActive(true);
        }
        else if (ageQuestions.Contains(items[currentItem]))
        {
            currentScales = ageScales;
            currentScale = currentAgeScale;
            ageScaleGO.SetActive(true);
        }

        lowAnchorText.text = items[currentItem].lowAnchor;
        highAnchorText.text = items[currentItem].highAnchor;
    }

    private void SetScalesActive(List<GameObject> scales, bool isActive)
    {
        foreach (var scale in scales)
        {
            scale.SetActive(isActive);
        }
    }

    public void InitializeQuestionnaire()
    {
        string questionnairePath = Helpers.CreateDataPath(singleAvatar.participantID, "_" + singleAvatar.VMType.ToString());
        questionnaireWriter = new StreamWriter(questionnairePath, true);
        mainText.text = "Please fill out the questionnaire based on the last experience.";
        isStart = false;
        isEnd = false;

        spesQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("I felt out of my body.", "scalesspes1", "", ""),
            new QuestionnaireData("I felt as if my (real) body were drifting toward the virtual body or as if the virtual body were drifting toward my (real) body.", "scalesspes2", "", ""),
            new QuestionnaireData("I felt as if the movements of the virtual body were influencing my own movements.", "scalesspes3", "", ""),
            new QuestionnaireData("It felt as if my (real) body were turning into an avatar body.", "scalesspes4", "", ""),
            new QuestionnaireData("At some point it felt as if my real body was starting to take on the posture or shape of the virtual body that I saw.", "scalesspes5", "", ""),
            new QuestionnaireData("I felt like I was wearing different clothes from when I came to the laboratory.", "scalesspes6", "", ""),
            new QuestionnaireData("I felt as if my body had changed.", "scalesspes7", "", ""),
            new QuestionnaireData("I felt a touch sensation in my body when I saw floor.", "scalesspes8", "", ""),
            new QuestionnaireData("I felt that my own body could be affected by floor.", "scalesspes9", "", ""),
            new QuestionnaireData("I felt as if the virtual body was my body.", "scalesspes10", "", ""),
            new QuestionnaireData("At some point it felt that the virtual body resembled my own (real) body in terms of shape skin tone or other visual features.", "scalesspes11", "", ""),
            new QuestionnaireData("I felt as if my body was located where I saw the virtual body.", "scalesspes12", "", ""),
            new QuestionnaireData("I felt like I could control the virtual body as if it was my own body.", "scalesspes13", "", ""),
            new QuestionnaireData("It seemed as if I felt the touch of the floor in the location where I saw the virtual feet touched.", "scalesspes14", "", ""), 
            new QuestionnaireData("It seemed as if the touch I felt was caused by the floor touching the virtual body.", "scalesspes15", "", ""),
            new QuestionnaireData("It seemed as if my body was touching the floor.", "scalesspes16", "", "")
        };

        vrsqQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("Please rate how much general discomfort affected you during the experience.", "scalesvrsq1", "", ""),
            new QuestionnaireData("Please rate how much fatigue affected you during the experience.", "scalesvrsq2", "", ""),
            new QuestionnaireData("Please rate how much eyestrain affected you during the experience.", "scalesvrsq3", "", ""),
            new QuestionnaireData("Please rate how much difficulty of focusing affected you during the experience.", "scalesvrsq4", "", ""),
            new QuestionnaireData("Please rate how much headache affected you during the experience.", "scalesvrsq5", "", ""),
            new QuestionnaireData("Please rate how much fullness of head affected you during the experience.", "scalesvrsq6", "", ""),
            new QuestionnaireData("Please rate how much blurred vision affected you during the experience.", "scalesvrsq7", "", ""),
            new QuestionnaireData("Please rate how much dizziness affected you during the experience.", "scalesvrsq8", "", ""),
            new QuestionnaireData("Please rate how much vertigo affected you during the experience.", "scalesvrsq9", "", "")
        };

        susQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("I think that I would like to use this system frequently.", "scalessus1", "", ""),
            new QuestionnaireData("I found the system unnecessarily complex.", "scalessus2", "", ""),
            new QuestionnaireData("I thought the system was easy to use.", "scalessus3", "", ""),
            new QuestionnaireData("I think that I would need the support of a technical person to be able to use this system.", "scalessus4", "", ""),
            new QuestionnaireData("I found the various functions in this system were well integrated.", "scalessus5", "", ""),
            new QuestionnaireData("I thought there was too much inconsistency in this system.", "scalessus6", "", ""),
            new QuestionnaireData("I would imagine that most people would learn to use this system very quickly.", "scalessus7", "", ""),
            new QuestionnaireData("I found the system very cumbersome to use.", "scalessus8", "", ""),
            new QuestionnaireData("I felt very confident using the system.", "scalessus9", "", ""),
            new QuestionnaireData("I needed to learn a lot of things before I could get going with this system.", "scalessus10", "", "")
        };

        sexQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("Please, select your sex", "scalessex1", "", "")
        };

        ageQuestions = new List<QuestionnaireData>
        {
            new QuestionnaireData("Please, select your age group", "scalesage1", "", ""),
        };

        Helpers.Shuffle(spesQuestions);
        Helpers.Shuffle(vrsqQuestions);
        Helpers.Shuffle(susQuestions);

        items = new List<QuestionnaireData>();
        items.AddRange(spesQuestions);
        items.AddRange(vrsqQuestions);
        items.AddRange(susQuestions);
        items.AddRange(sexQuestions);
        items.AddRange(ageQuestions);

        responses = new List<int>(new int[items.Count]);
        currentItem = 0;
        currentSpesScale = 4;
        currentVrsqScale = 3;
        currentSusScale = 3;
        currentSexScale = 2;
        currentAgeScale = 4;

        SetScalesActive(spesScales, false);
        SetScalesActive(vrsqScales, false);
        SetScalesActive(susScales, false);
        SetScalesActive(sexScales, false);
        SetScalesActive(ageScales, false);

        smallInstruction.text = "";
        largeInstruction.text = "Press A to begin.";
        isAllowedCheck = false;

        foreach (var scale in currentScales)
        {
            scale.SetActive(false);
        }
    }

    IEnumerator EndQuestionnaire()
    {
        yield return WriteQuestionnaireData();
        isStart = false;
        isEnd = true;
        currentScaleGO.SetActive(false);
        smallInstruction.text = "";
        largeInstruction.text = "";
        singleAvatar.isQuestionnaireDone = true;
        mainText.text = "This is the end of the experiment. Thank you for participation.";
    }

    IEnumerator WriteQuestionnaireData()
    {
        questionnaireWriter.Write(
            "ParticipantID" + "," +
            "Visuomotor_type" + "," +
            "Item" + "," +
            "Response" + "\n");

        for (int i = 0; i < responses.Count; i++)
        {
            questionnaireWriter.Write(
                $"{{\"ParticipantID\":\"{singleAvatar.participantID}\",\"Visuomotor_type\":\"{singleAvatar.VMType}\",\"Item\":\"{items[i].shortKey}\",\"Response\":\"{responses[i]}\", \"{items[i].shortKey}\":\"{responses[i]}\"}}" + "\n");
        }
        questionnaireWriter.Flush();
        questionnaireWriter.Close();
        yield return 0;
    }

    public struct QuestionnaireData
    {
        public string item;
        public string shortKey;
        public string lowAnchor;
        public string highAnchor;

        public QuestionnaireData(string item, string shortKey, string lowAnchor, string highAnchor)
        {
            this.item = item;
            this.shortKey = shortKey;
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
}
