using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AsyncCrowd
{
    public class ServerHandler : MonoBehaviour
    {
        public ServerResponse serverResponse;
        public string API_URL = "";
        private UserData userData;

        private int startTime;

        private long timestamp()
        {
            return new System.DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }


        // Start is called before the first frame update
        void Start()
        {
            startTime = (int)timestamp();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SendData(List<QuestionnaireController.QuestionnaireData> items, List<int> responses, string condition, Dictionary<string, int> timestamps)
        {

            bool isTest = false;
            List<QuestionAnswer> answers = new List<QuestionAnswer>();

            if (items.Count != responses.Count)
            {
                throw new Exception("questions and answers not same length");
            }

            for (int i = 0; i < responses.Count; i++)
            {
                answers.Add(new QuestionAnswer(items[i].shortKey, responses[i]));
            }

            userData = new UserData(answers, isTest, condition, timestamps);
            userData.setStart(startTime);
            userData.setEnd();

            // send it to the server
            SendDataToServer();
        }

        public string toJSON()
        {
            bool prettyPrint = true;
            return JsonUtility.ToJson(userData, prettyPrint);
        }

        public void SendDataToServer()
        {
            StartCoroutine(CheckForServerResponse());
            StartCoroutine(SendJSONToServer());
        }

        IEnumerator SendJSONToServer()
        {
            string data = toJSON();

            Debug.Log(data);

            UnityWebRequest www = new UnityWebRequest(API_URL);
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.method = UnityWebRequest.kHttpVerbPOST;

            yield return www.SendWebRequest();

            Debug.Log("Server callback");
            Debug.Log(www.result);

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                try
                {
                    serverResponse = JsonUtility.FromJson<ServerResponse>(www.downloadHandler.text);
                }
                catch (ArgumentException)
                {
                    // serverreponse does not fit ServerResponse class format
                    serverResponse = new ServerResponse("-1", www.downloadHandler.text);
                }
            }

            Debug.Log(serverResponse.status);
        }

        IEnumerator CheckForServerResponse()
        {
            while (serverResponse == null)
            {
                // do nothing
                yield return new WaitForSeconds(1);
            }

            // now we have a server response
            Debug.Log("Participant ID: " + serverResponse.PID);
        }
    }


    public class ServerResponse
    {
        public string PID;
        public string status;

        public ServerResponse(string pid, string status)
        {
            PID = pid;
            this.status = status;
        }

        override public string ToString()
        {
            return "ServerResponse(status:" + status + ", PID:" + PID + ")";
        }
    }
}