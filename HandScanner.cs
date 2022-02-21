using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class HandScanner : MonoBehaviour {

    //Array od gameObject - solution 
    public GameObject[] solution;

    // Declear variables
    [SerializeField] private Text topPanelText;
    [SerializeField] private Text bottomPanelText;
    private AudioSource audioSource;
    
    //sounds played when: success/ fail
    public AudioClip sfxSuccess;
    public AudioClip sfxFailure;

    //IP adress of door controller - wireless requ. to unlock the door when puzzle solved
    [SerializeField] string DoorControllerIPAddress;

    //array of gameObject - guess (player made)
    [SerializeField] private GameObject[] guess;

    private enum ScanStatus { Idle, Scanning, Success, Failure };
    private ScanStatus scanStatus = ScanStatus.Idle;

    //button touch
    public void onTouch(GameObject gO) {
        int finger = Input.touchCount - 1;
        guess[finger] = gO;
    }

    //button release
    public void onRelease(GameObject gO) {
        for(int i = 0; i < guess.Length; i++) {
            if(guess[i] == gO) { guess[i] = null; }
        }
    }
    // Use this for initialization
    void Start () {
        topPanelText.text = "Place Hand";
        guess = new GameObject[solution.Length];
        audioSource = GetComponent<AudioSource>(); //responsible for sound
    }

    // Update is called once per frame
    void Update() 
    {

        //number of fingers needed to unlock 
        int numFingers = Input.touchCount;

        switch (scanStatus) 
        {
            case ScanStatus.Idle:
                bottomPanelText.text = string.Format("{0} fingers scanned", numFingers);
                if (numFingers == solution.Length) {
                    scanStatus = ScanStatus.Scanning;
                }
                break;

                //scanning fingers 
            case ScanStatus.Scanning:
                Debug.Log("Scanning");
                topPanelText.text = "Scanning"; //updating new text on top of screen

                //loop runs
                bool allCorrect = true;
                for (int i = 0; i < solution.Length; i++) {
                    if(!(guess[i] != null && guess[i].Equals(solution[i]))){
                        allCorrect = false; //if all correct is true than we exit the loop and unlock
                    }
                }
                //unlock process begins
                if(allCorrect) 
                {
                    Debug.Log("Success"); //message shows
                    audioSource.clip = sfxSuccess; //success audio clip assigned
                    audioSource.Play(); // play audio clip
                    StartCoroutine(SendUnlockTrigger()); //makes request to unlock
                    topPanelText.text = "Access Granted"; // new text updated
                    scanStatus = ScanStatus.Success;
                }
                
                //if fingers not matched - puzzle fail!
                else 
                {
                    Debug.Log("Failure"); //failure message
                    audioSource.clip = sfxFailure; // failure sound
                    audioSource.Play();
                    topPanelText.text = "Access Denied"; //new message pops up
                    scanStatus = ScanStatus.Failure;
                }
                break;
                
                
////////// stopped commnting here //24:40
           
            case ScanStatus.Success:
                if (numFingers == 0) //no fingers touching screen - go back to idle screen
                {
                    topPanelText.text = "Place Hand";
                    scanStatus = ScanStatus.Idle;
                }
                break;

            case ScanStatus.Failure:
                if (numFingers == 0)  //no fingers touching screen - go back to idle screen
                {
                    topPanelText.text = "Place Hand";
                    scanStatus = ScanStatus.Idle;
                }
                break;

        }
       
    }
    
    //Puzzle solved - success 
    IEnumerator SendLockTrigger() {
        string URL = string.Concat(DoorControllerIPAddress, "/L");
        //unlocks door - calling web page
        UnityWebRequest www = UnityWebRequest.Get(URL);
        yield return www.SendWebRequest();

     //read the result - visual feed back of result
        //if error
        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError) {
            Debug.Log(www.error);
            bottomPanelText.text = www.error;
        }
        
        //if sucess 
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            bottomPanelText.text = www.downloadHandler.text;
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }

    // ?? 28:25
    IEnumerator SendUnlockTrigger() {
        string URL = string.Concat(DoorControllerIPAddress, "/H");
        UnityWebRequest www = UnityWebRequest.Get(URL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError) {
            Debug.Log(www.error);
            bottomPanelText.text = www.error;
        }
        else {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            bottomPanelText.text = www.downloadHandler.text;
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
}
