using UnityEngine;
using System.Collections;

public class DisplayTutorial : MonoBehaviour {
    
    //[SerializeField] private float TutrorialFadeTime;
    [SerializeField] private GameObject TutorialText;

    //private Vector3 OriginalTextPosition;
    private TextMesh TextField;
    private float CurrentTimeLeft;
    private int CurrentCode;
    private int CurrentSubcode;
    private DifficultyCurve parent;
    private GameLogic GameLogicInstance;

    void Start () {
        //OriginalTextPosition = TutorialText.transform.position;
        TextField = TutorialText.GetComponent<TextMesh>();
        parent = gameObject.GetComponentInParent<DifficultyCurve>();
        GameLogicInstance = gameObject.GetComponentInParent<GameLogic>();
    }
	
	void Update () {
        
        if (CurrentCode > 0) {
            CurrentTimeLeft -= Time.deltaTime;

            //If player died during tutorial (silly player)
            if (GameLogicInstance.mGameStatus == GameLogic.State.GameOver) {
                TextField.text = "";
                CurrentCode = 0;
            }

            //If tutorial has finished
            else if (CurrentTimeLeft < 0) {
                TextField.text = "";
                //TutorialText.transform.position = OriginalTextPosition;
                parent.NotifyTutorialDone(CurrentCode);
                CurrentCode = 0;
            }
        }
	}

    public void Display(int code) {
        CurrentCode = code;
        switch (code) {
            case 1:
                TextField.text = "This is you.";
                TutorialText.transform.position = new Vector3(GameLogic.ScreenBounds * 0.3f, GameLogic.ScreenHeight * -0.35f, 0f);
                CurrentTimeLeft = 5f;
                break;
            case 2:
                TextField.text = "Ahead of you is the\nneverending road.";
                TutorialText.transform.position = new Vector3(0f, GameLogic.ScreenHeight * 0.35f, 0f);
                CurrentTimeLeft = 3f;
                break;
            case 3:
                TextField.text = "You have been sent\nfrom your beloved blue fortress\nto conquer the road.";
                TutorialText.transform.position = new Vector3(GameLogic.ScreenBounds * -0.5f, GameLogic.ScreenHeight * -0.3f, 0f);
                CurrentTimeLeft = 5f;
                break;
            case 4:
                TextField.text = "Beware of the mighty soldiers\nfrom the red fortress!";
                TutorialText.transform.position = new Vector3(0f, GameLogic.ScreenHeight * 0.25f, 0f);
                CurrentTimeLeft = 3.5f;
                break;
            case 5:
                TextField.text = "Tap the screen or press W to shoot the enemy!";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 3f;
                break;
            case 6:
                TextField.text = "How far can you get?";
                TutorialText.transform.position = new Vector3(0f, GameLogic.ScreenHeight * -0.27f, 0f);
                CurrentTimeLeft = 2f;
                break;
            case 7:
                //This is a 'respiro' inbetween tutorial messages
                TextField.text = "";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 4.5f;
                break;
            case 8:
                //This is a gap before enemies change column for the first time
                TextField.text = "";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 1.5f;
                break;
            case 9:
                TextField.text = "Oh, no! The mighty red soldiers\nare attempting to dodge\nyour deadly bullets and invade\nyour fortress!";
                TutorialText.transform.position = new Vector3(GameLogic.ScreenBounds * 0.5f, 0f, 0f);
                CurrentTimeLeft = 5f;
                break;
            case 10:
                TextField.text = "Swipe sideways or press A or D\nto move left and right!";
                TutorialText.transform.position = new Vector3(GameLogic.ScreenBounds * 0.5f, 0f, 0f);
                CurrentTimeLeft = 3f;
                break;
            case 11:
                TextField.text = "Here comes the most feared soldier of the red fortress.";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 3f;
                break;
            case 12:
                TextField.text = "Can you beat him?";
                TutorialText.transform.position = new Vector3(0f, GameLogic.ScreenHeight * -0.2f, 0f);
                CurrentTimeLeft = 1.5f;
                break;
            case 13:
                TextField.text = "These are powerups.";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 1.5f;
                break;
            case 14:
                TextField.text = "They will boost your abilities.";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 1.8f;
                break;
            case 15:
                TextField.text = "Try to catch them!";
                TutorialText.transform.position = new Vector3(0f, 0f, 0f);
                CurrentTimeLeft = 1.5f;
                break;
            case 16:
                TextField.text = "You have new powerups!\nTo use them, tap on these buttons\nor use keys 1, 2, 3!";
                TutorialText.transform.position = new Vector3(GameLogic.ScreenBounds * 0.55f, GameLogic.ScreenHeight * 0.38f, 0f);
                CurrentTimeLeft = 3f;
                break;
        }
    }
}
