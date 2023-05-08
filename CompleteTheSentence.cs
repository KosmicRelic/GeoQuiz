using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class CompleteTheSentence : MonoBehaviour
{
    public ButtonsController buttonsController;
    public MasterQuizController masterQuizController;
    public Data dataBase;
    //Canvas gameObjects
    public Canvas canvas;
    public TextMeshProUGUI questionText;
    [Header("Answer Buttons")]
    public Button[] answerButtons;
    public GameObject textAnswerPrefab;
    GameObject answerPrefab; //Stores the gameObject that will be the answer to complete the sentence
    public Image hideSentenceImage;
    public Image underline; //Instantiates under the right answer to show where the right answer goes
    Image underlinePrefab; //Instantiates under the right answer to show where the right answer goes

    //Variables
    string biggestString = ""; //The biggest string count will be the one to replace the correct answer in order to find the biggest limits of the required space in sentence
    string lastButtonPressed = "";
    public Vector2 positionOfRightAnswerInSentence; //This is the position in which things will take place in sentence
    List <string> wordsInSentence = new List<string>(); // Stores the words of the sentence in a list
    List<string> answers = new List<string>();


    [HideInInspector] public List<CompleteTheSentenceMode> questions = new List<CompleteTheSentenceMode>(); //Stores the values of the questions section from the dataBase
    [HideInInspector] public List<CompleteTheSentenceMode> rightAnsweredQuestions = new List<CompleteTheSentenceMode>(); //Stores the values of the questions section from the dataBase
    [HideInInspector] public List<CompleteTheSentenceMode> wrongAnsweredQuestions = new List<CompleteTheSentenceMode>(); //Stores the values of the questions section from the dataBase
    bool questionsAreSetUp = false; //Helps to identify if the list has already being filled
    void Start()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int button = i;
            answerButtons[i].onClick.AddListener(()=>OnAnswersButtonPress(button));
        }
    }

    public void CheckIfThereAreRemainingQuestions()
    {
        //if there are no questions and there are wrong answered  questions, put the wrong answers into questions 
        if (questions.Count == 0 && wrongAnsweredQuestions.Count > 0)
        {
            for (int i = 0; i < wrongAnsweredQuestions.Count; i++)
            {
                questions.Add(wrongAnsweredQuestions[i]);
            }
            wrongAnsweredQuestions.Clear();
        }//else if there are no questions and there are right answered  questions, put the wrong answers into questions
        else if (questions.Count == 0 && rightAnsweredQuestions.Count > 0)
        {
            for (int i = 0; i < rightAnsweredQuestions.Count; i++)
            {
                questions.Add(rightAnsweredQuestions[i]);
            }
            rightAnsweredQuestions.Clear();
        }

        GoToNextQuestion();
    }

    void GoToNextQuestion() 
    {
        SetUpQuestions();

        buttonsController.confirmationButton.onClick.RemoveListener(masterQuizController.Continue);
        buttonsController.confirmationButton.onClick.AddListener(CheckAnswer); //Assign the CheckAnswer() method to the CHECK button

        questionText.text = questions[0].sentence;

        for (int i = 0; i < questions[0].answers.Length; i++)
        {   //Add a new index to store the answer type of string
            answers.Add(questions[0].answers[i]);
        }
        //shuffle the List<string> of the answers before assigning them into the buttons
        buttonsController.ShuffleWords(answers);
        
        //assign the answers to the buttons
        for (int i = 0; i < answers.Count; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
        }

        StartCoroutine(FindRightWordPositionInSentence());        
    }

    void SetUpQuestions() // Takes the questions from the database and puts it to a new list from which the questions will be drawn
    {
        if (!questionsAreSetUp)
        {
            questionsAreSetUp = true;

            for (int i = 0; i < MasterQuizController.numberOfQuestionsToBeAsked; i++)
            {
                questions.Add(dataBase.completeTheSentence[i]);
            }

            ShuffleQuestions(questions);
        }
    }

    void ShuffleQuestions(List<CompleteTheSentenceMode> questions)
    {
        for (int i = 0; i < questions.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, questions.Count);
            CompleteTheSentenceMode word = questions[i];

            questions[i] = questions[r];
            questions[r] = word;
        }
    }

    void CheckAnswer()//Assigned to the checkAnswer button to check if the answer is correct
    {
        buttonsController.confirmationButton.onClick.RemoveListener(CheckAnswer);
        buttonsController.confirmationButton.onClick.AddListener(masterQuizController.Continue);
        //if answer is right
        if (lastButtonPressed == questions[0].rightAnswer)
        {
            ChangeAnswerTextColor(true);
            MasterQuizController.numberOfQuestionsAnsweredRight++;
            masterQuizController.rightAnswerPanel.SetActive(true);
            rightAnsweredQuestions.Add(questions[0]);
            //Applause the player
            int r = Random.Range(0, masterQuizController.correctAnswerApplause.Length);
            masterQuizController.rightAnswerPanel.GetComponentInChildren<TextMeshProUGUI>().text = masterQuizController.correctAnswerApplause[r];
            //Update the progress
            StartCoroutine(masterQuizController.ProgressUpdate(1));
        }
        else//if answer is wrong
        {
            ChangeAnswerTextColor(false);
            wrongAnsweredQuestions.Add(questions[0]);
            //Show the player the correct answer
            masterQuizController.wrongAnswerPanel.SetActive(true);
            masterQuizController.wrongAnswerText.text = questions[0].rightAnswer;
            //Update the progress
            StartCoroutine(masterQuizController.ProgressUpdate(-1));
        }
        MasterQuizController.totalNumberOfQuestionsAnswered++;
        questions.RemoveAt(0);
    }

    void OnAnswersButtonPress(int button) 
    {   //if the button pressed is the same as the previous one, then deselect it and make the CHECK button unavailable
        if (answerButtons[button].GetComponentInChildren<TextMeshProUGUI>().text == lastButtonPressed)
        {
            underlinePrefab.GetComponent<Image>().color = Color.white;
            buttonsController.DisableCheckButton();
            lastButtonPressed = "";
        }
        else
        {
            underlinePrefab.GetComponent<Image>().color = new Color32(73, 192, 248, 255);
            lastButtonPressed = answerButtons[button].GetComponentInChildren<TextMeshProUGUI>().text;
            buttonsController.EnableCheckButton();
        }
        answerPrefab.GetComponentInChildren<TextMeshProUGUI>().text = lastButtonPressed;
    }

    int FindBiggestAnswer() //Finds the longest string from the given asnswers
    {
        // Find the longest answer and store it in a variable
        for (int i = 0; i < questions[0].answers.Length; i++)
        {
            if (biggestString.Length < questions[0].answers[i].Length)
            {
                biggestString = questions[0].answers[i];
            }
        }
        int indexOfAnswerInSentence = 0;
        string[] rightAnswerWords = questions[0].rightAnswer.Split();
        wordsInSentence = questionText.text.Split(" ").ToList();
        //loop through the words in the sentence
        for (int i = 0; i < wordsInSentence.Count; i++)
        {   //loop throught the words of the right answer
            for (int j = 0; j < rightAnswerWords.Length; j++)
            {   //if the word in sentence is equal to the right answer word
                if (wordsInSentence[i + j] == rightAnswerWords[j])
                {   //if this is the last index of the rightAnswerWords
                    if (j == rightAnswerWords.Length-1)
                    {
                        indexOfAnswerInSentence = i;
                        wordsInSentence[i] = biggestString;
                        //Delete the extra indexes which contain the right answer words
                        for (int l = 0; l < rightAnswerWords.Length - 1; l++) 
                        {
                            wordsInSentence.RemoveAt(i + 1);
                        }

                        questionText.text = "";
                        //Pass the list of the words in the sentence text
                        for (int b = 0; b < wordsInSentence.Count; b++)
                        {
                            questionText.text += wordsInSentence[b] + " ";
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
        return indexOfAnswerInSentence;
    }

    #region Find position of right answer in sentence
    IEnumerator FindRightWordPositionInSentence() //finds the exact position that the right answer should be located in the sentence
    {   //stores the index in which the first right word is located in the sentence
        int indexOfAnswerInSentence = FindBiggestAnswer();
        //seperates the biggestString in an array
        string[] temp = biggestString.Split(" ");
        bool multipleWordsAnswer = (biggestString.Length > 1) ? true : false;

        yield return new WaitForEndOfFrame();

        Transform m_Transform = questionText.transform;
        TMP_TextInfo m_TextInfo = questionText.textInfo;

        #region Make Word Invisible
        for (int i = 0; i < temp.Length; i++)
        {
            MakeWordInvisible(m_TextInfo.wordInfo[indexOfAnswerInSentence + i]);
        }
        questionText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        #endregion

        #region Get vectors of first and last index

        Vector3 bottomLeft = Vector3.zero;
        Vector3 topLeft = Vector3.zero;
        Vector3 bottomRight = Vector3.zero;
        Vector3 topRight = Vector3.zero;

        float maxAscender = -Mathf.Infinity;
        float minDescender = Mathf.Infinity;

        //For the first and last character
        for (int j = 0; j < 2; j++)
        {
            int characterIndex = 0;
            //if there are two words and looking to store the last character, then get the last character of the last word
            if (multipleWordsAnswer && j == 1)
            {
                characterIndex = m_TextInfo.wordInfo[indexOfAnswerInSentence + temp.Length - 1].lastCharacterIndex;
            }
            else
            {
                characterIndex = (j == 0) ? m_TextInfo.wordInfo[indexOfAnswerInSentence].firstCharacterIndex : m_TextInfo.wordInfo[indexOfAnswerInSentence].lastCharacterIndex;
            }

            TMP_CharacterInfo currentCharInfo = m_TextInfo.characterInfo[characterIndex];

            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

            //If this is the first "for" round, store the vectors of the first character of the first word
            if (j == 0)
            {
                topLeft = m_Transform.TransformPoint(new Vector3(currentCharInfo.topLeft.x, maxAscender, 0));
                bottomLeft = m_Transform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, minDescender, 0));
            }//else store the vectors of the last character of the last word
            else
            {
                bottomRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.bottomRight.x, minDescender, 0));
                topRight = m_Transform.TransformPoint(new Vector3(currentCharInfo.topRight.x, maxAscender, 0));
            }
        }
        #endregion
        ShowUnderline(bottomLeft, topLeft, topRight, bottomRight);

        answerPrefab = Instantiate(textAnswerPrefab, positionOfRightAnswerInSentence, Quaternion.identity, canvas.transform);

        hideSentenceImage.gameObject.SetActive(false);
    }

    void MakeWordInvisible(TMP_WordInfo wInfo) //Makes the selected word invisible in the sentence
    {
        for (int i = 0; i < wInfo.characterCount; i++)
        {
            int characterIndex = wInfo.firstCharacterIndex + i;
            // Get the index of the material / sub text object used by this character.
            int meshIndex = questionText.textInfo.characterInfo[characterIndex].materialReferenceIndex;

            int vertexIndex = questionText.textInfo.characterInfo[characterIndex].vertexIndex;

            // Get a reference to the vertex wrongAnswerColor
            Color32[] vertexColors = questionText.textInfo.meshInfo[meshIndex].colors32;

            Color32 c = new Color32(0, 0, 0, 0);

            vertexColors[vertexIndex + 0] = c;
            vertexColors[vertexIndex + 1] = c;
            vertexColors[vertexIndex + 2] = c;
            vertexColors[vertexIndex + 3] = c;
        }
    }

    void ShowUnderline(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br) //Scales an image as an underline in the sentence
    {
        //To find the correct center point we use A+(B-A)/2 formula
        Vector3 averageBottom = br + (bl - br) / 2;
        Vector3 averageTop = tl + (bl - tl) / 2;
        Vector2 finalResult = new Vector3(averageBottom.x, averageTop.y,0);        
        positionOfRightAnswerInSentence = finalResult;
        //Modify underlinePrefab
        underlinePrefab = Instantiate(underline, canvas.transform);
        underlinePrefab.transform.position = positionOfRightAnswerInSentence - new Vector2(0, (tr.y - br.y) / 2);
        underlinePrefab.GetComponent<RectTransform>().sizeDelta = CorrectScaleWithCanvas(bl, br);
    }

    Vector2 CorrectScaleWithCanvas(Vector3 bl, Vector3 br) //Finds the correct position of two points in any resolution
    {
        float pointOne = 0f;
        float pointTwo = 0f;
        //if this is playing in as a built game
        if (Application.isPlaying && !Application.isEditor)
        {
            float width = Screen.currentResolution.width;

            //for pointOne
            pointOne = 1440 * bl.x;
            pointOne = pointOne / width;
            //for pointTwo
            pointTwo = 1440 * br.x;
            pointTwo = pointTwo / width;
        }
        //if this is editor game mode
        else if (Application.isPlaying && Application.isEditor)
        {
            float width = Screen.width;

            //for pointOne
            pointOne = 1440 * bl.x;
            pointOne = pointOne / width;
            //for pointTwo
            pointTwo = 1440 * br.x;
            pointTwo = pointTwo / width;
        }

        return new Vector2(pointTwo - pointOne, 4);
    }
    #endregion


    void ChangeAnswerTextColor(bool isAnswerRignt) 
    {
        Color rightAnswerColor = new Color32(147, 211,52,255);
        Color wrongAnswerColor = new Color32(238,86,85,255);
        Color defaultAnswerColor = new Color32(73, 192, 248,255);

        answerPrefab.GetComponentInChildren<TextMeshProUGUI>().color = (isAnswerRignt) ? rightAnswerColor : wrongAnswerColor;
    }

    public void ResetValues() 
    {
        lastButtonPressed = "";
        questionText.text = "";
        biggestString = "";

        hideSentenceImage.gameObject.SetActive(true);
        answers.Clear();

        Destroy(answerPrefab);
        Destroy(underlinePrefab.gameObject);

        masterQuizController.quizControllerGameObject.SetActive(false);
        masterQuizController.PickTheRightAnswersModeGameObject.SetActive(false);
    }
}