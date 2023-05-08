using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Unscramble : MonoBehaviour
{
    //Scripts
    public ButtonsController buttonsController;
    public MasterQuizController masterQuizController;
    public Data dataBase;
    //Canvas gameObjects
    public GameObject answersButtonsHolder;
    public GameObject buttonBackgroundImagesHolder;
    public GameObject imageToHideButtons; //Hides buttons until they are ready to reveal
    public TextMeshProUGUI questionText;
    List<Button> buttonsInTheSentence = new List<Button>(); //Will store the buttons that will be in the sentence
    [Header("Answer Buttons")]
    List<Button> answerWordsButtons = new List<Button>();
    public Sprite buttonImage;
    public Sprite buttonUnderlineImage;
    //Variables
    int numberOfInstantiatedButtons = 30;
    int numberOfWordsInSentence = 0;
    float sentenceBoudaries = 660;
    public GameObject buttonBackgroundImage;
    public GameObject unscrableButtonPrefab;
    GameObject buttonPrefab;
    //Variables to be used in setting the position of values
    Button lastInstantiatedButton;
    float heightOfInstantiatedWord = 0;
    float heightOfNextWordsInSentence = 730; //Used to store the height of the next position of the words in the sentence
    float randomWordStartingPoint = 0;
    List<GameObject> buttonBackgrounds = new List<GameObject>(); //Stores the background images of every button
    List<Vector2> buttonsStartingPositions = new List<Vector2>(); //Stores the starting position of every button
    List<Vector2> buttonsPositionsInSentence = new List<Vector2>(); //Stores the ending position of every button
    Vector2 endPos; //The position in the sentence in which the current zbuttons will go

    [HideInInspector] public List<UnscrambleMode> sentences = new List<UnscrambleMode>();
    [HideInInspector] public List<UnscrambleMode> wrongAnsweredSentences = new List<UnscrambleMode>();
    [HideInInspector] public List<UnscrambleMode> rightAnsweredSentences = new List<UnscrambleMode>();
    bool sentensesAreSetUp = false; //Helps to identify if the list has already being filled

    public GameObject answersButtons;

    void InstantiateAnswersButtonsAndButtonBackgrounds() //Instantiates a number of backgrounds for every button
    {
        for (int i = 0; i < numberOfInstantiatedButtons; i++)
        {
            //Instantiate the buttons
            GameObject newButton = Instantiate(unscrableButtonPrefab, answersButtonsHolder.transform);
            answerWordsButtons.Add(newButton.GetComponentInChildren<Button>());
            int currentButton = i;
            answerWordsButtons[i].GetComponentInChildren<Button>().onClick.AddListener(() => OnButtonPress(currentButton));
            newButton.SetActive(false);
            //Add ContentSizeFitter
            ContentSizeFitter contentSizeFitter = answerWordsButtons[i].gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.enabled = false;
            //Add HorizontalLayoutGroup
            HorizontalLayoutGroup horizontalLayoutGroup = answerWordsButtons[i].gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayoutGroup.enabled = false;

            //Instantiate background image of buttons
            GameObject newButtonBackgroundImage = Instantiate(buttonBackgroundImage, buttonBackgroundImagesHolder.transform);
            newButtonBackgroundImage.SetActive(false);
            buttonBackgrounds.Add(newButtonBackgroundImage);
        }
    }
    public void CheckIfThereAreRemainingQuestions()
    {
        imageToHideButtons.SetActive(true);
        //if this is the first time the mode is loaded
        if (buttonPrefab == null)
        {
            randomWordStartingPoint = Random.Range(500, 600);

            buttonPrefab = unscrableButtonPrefab.transform.Find("Button").gameObject;
            InstantiateAnswersButtonsAndButtonBackgrounds();
        }
        
        //if there are no questions and there are wrong answered  questions, put the wrong answers into questions 
        if (sentences.Count == 0 && wrongAnsweredSentences.Count > 0)
        {
            for (int i = 0; i < wrongAnsweredSentences.Count; i++)
            {
                sentences.Add(wrongAnsweredSentences[i]);
            }
            wrongAnsweredSentences.Clear();
        }
        //else if there are no questions and there are right answered  questions, put the wrong answers into questions
        else if (sentences.Count == 0 && rightAnsweredSentences.Count > 0)
        {
            for (int i = 0; i < rightAnsweredSentences.Count; i++)
            {
                sentences.Add(rightAnsweredSentences[i]);
            }
            rightAnsweredSentences.Clear();
        }

        GoToNextQuestion();
    }

    public void GoToNextQuestion() //Sets up the words into buttons
    {
        SetUpQuestions();

        buttonsController.confirmationButton.onClick.RemoveListener(masterQuizController.Continue);
        buttonsController.confirmationButton.onClick.AddListener(CheckAnswer); //Assign the CheckAnswer() method to the CHECK button

        questionText.text = "Put the words in the right order";
        questionText.gameObject.SetActive(true);
        //Put the words of the sentence in a string along with the wrong words
        string sumOfCorrectAndWrongWords = sentences[0].correctSentence;
        for (int i = 0; i < sentences[0].wrongWords.Length; i++)
        {
            sumOfCorrectAndWrongWords += " " + sentences[0].wrongWords[i];
        }
        string[] words = sumOfCorrectAndWrongWords.Split(" ");
        
        numberOfWordsInSentence = words.Length;
        ShuffleWords(words);

        for (int i = 0; i < words.Length; i++)
        {
            answerWordsButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = words[i];
            answerWordsButtons[i].transform.parent.gameObject.SetActive(true);
        }
        //Make the buttons fit their content and then position them one next to the other
        StartCoroutine(AddContentSizeFitterToButtons());
    }

    void SetUpQuestions() // Takes the questions from the database and puts it to a new list from which the questions will be drawn
    {
        if (!sentensesAreSetUp)
        {
            sentensesAreSetUp = true;
            for (int i = 0; i < MasterQuizController.numberOfQuestionsToBeAsked; i++)
            {
                sentences.Add(dataBase.unscramble[i]);
            }

            ShuffleQuestions(sentences);
        }
    }
    
    void ShuffleQuestions(List<UnscrambleMode> sentences) //Shuffles the order if the sentences
    {
        for (int i = 0; i < sentences.Count; i++)
        {
            int r = Random.Range(i, sentences.Count);
            UnscrambleMode word = sentences[i];

            sentences[i] = sentences[r];
            sentences[r] = word;
        }
    }

    void ShuffleWords(string[] words) //Shuffles the words that will be displayed
    {
        for (int i = 0; i < words.Length; i++)
        {
            int r = Random.Range(i, words.Length);
            string word = words[i];

            words[i] = words[r];
            words[r] = word;
        }
    }
    void CheckAnswer()//Check if the player's answer is equal to the right sentence
    {
        buttonsController.confirmationButton.onClick.RemoveListener(CheckAnswer);
        buttonsController.confirmationButton.onClick.AddListener(masterQuizController.Continue);
        //temp1 and temp2 will contain the player's and the correct answers. Comparing them will show the result
        string temp1 = "";
        string temp2 = "";

        string[] wordsOfCorrectSentence = sentences[0].correctSentence.Split(" ");
        //Add the words of the correct sentence into the temp1
        for (int i = 0; i < wordsOfCorrectSentence.Length; i++)
        {
            temp1 += wordsOfCorrectSentence[i];
        }
        //Add the words of the sentence the player formed into temp2 
        for (int i = 0; i < buttonsInTheSentence.Count; i++)
        {
            temp2 += buttonsInTheSentence[i].GetComponentInChildren<TextMeshProUGUI>().text;
        }
        //if the correct sentenceis equal to the player's answer
        if (temp1 == temp2)
        {
            MasterQuizController.numberOfQuestionsAnsweredRight++;
            rightAnsweredSentences.Add(sentences[0]);
            masterQuizController.rightAnswerPanel.SetActive(true);
            //Applause the player
            int r = Random.Range(0, masterQuizController.correctAnswerApplause.Length);
            masterQuizController.rightAnswerPanel.GetComponentInChildren<TextMeshProUGUI>().text = masterQuizController.correctAnswerApplause[r];
            //Update the progress
            StartCoroutine(masterQuizController.ProgressUpdate(1));
        }
        else//if answer is wrong
        {
            //Show the player the correct answer
            masterQuizController.wrongAnswerPanel.SetActive(true);
            masterQuizController.wrongAnswerText.text = sentences[0].correctSentence;
            //Update the progress
            StartCoroutine(masterQuizController.ProgressUpdate(-1));
            wrongAnsweredSentences.Add(sentences[0]);
        }
        MasterQuizController.totalNumberOfQuestionsAnswered++;
        sentences.RemoveAt(0);
    }

    #region Buttons' movement
    void SetInitialPositionOfButtons()
    {
        for (int i = 0; i < answerWordsButtons.Count; i++)
        {
            if (i == 0) //if this is the first button being put in position
            {
                answerWordsButtons[0].transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                answerWordsButtons[0].GetComponent<RectTransform>().rect.width / 2 + -randomWordStartingPoint, heightOfInstantiatedWord);
            }
            else
            {   //if the new position of the button is within the bounds of the screen + 100 to space the word
                if (lastInstantiatedButton.transform.parent.GetComponent<RectTransform>().anchoredPosition.x +
                    lastInstantiatedButton.GetComponent<RectTransform>().rect.width / 2 + 100 +
                    answerWordsButtons[i].GetComponent<RectTransform>().rect.width <= randomWordStartingPoint)
                {
                    answerWordsButtons[i].transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    lastInstantiatedButton.transform.parent.GetComponent<RectTransform>().anchoredPosition.x +
                    lastInstantiatedButton.GetComponent<RectTransform>().rect.width / 2 +
                    answerWordsButtons[i].GetComponent<RectTransform>().rect.width / 2 + 100, heightOfInstantiatedWord);
                }
                else//else set the position of the button lower
                {
                    heightOfInstantiatedWord -= 170;
                    randomWordStartingPoint = Random.Range(500, 600);
                    answerWordsButtons[i].transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(answerWordsButtons[i].GetComponent<RectTransform>().rect.width / 2 - randomWordStartingPoint,
                    heightOfInstantiatedWord);
                }
            }

            buttonsStartingPositions.Add(answerWordsButtons[i].transform.parent.GetComponent<RectTransform>().anchoredPosition);
            buttonsPositionsInSentence.Add(new Vector2(0f,0f));
            lastInstantiatedButton = answerWordsButtons[i];
        }

        StartCoroutine(SetBackgroundImages());
    }
    IEnumerator MoveButtonOnClick(int button)
    {
        Vector2 startingPos = buttonsStartingPositions[button]; //Starting position of every button is stored in this list
        //if there are no buttons in the sentence
        if (buttonsInTheSentence.Count == 0)
        {
            endPos = new Vector2(-sentenceBoudaries + answerWordsButtons[button].GetComponent<RectTransform>().rect.width / 2, 730);
            buttonsPositionsInSentence[button] = endPos;
        }
        else
        {
            for (int i = 0; i < answerWordsButtons.Count; i++)
            {
                //If the button text is equal to  buttonsInTheSentence
                if (answerWordsButtons[i] == buttonsInTheSentence[buttonsInTheSentence.Count - 1])
                {
                    //if the word is big enough to get in this line, put it
                    if (buttonsPositionsInSentence[i].x + buttonsInTheSentence[buttonsInTheSentence.Count - 1].GetComponent<RectTransform>().rect.width / 2 + 20 +
                        answerWordsButtons[button].GetComponent<RectTransform>().rect.width <= sentenceBoudaries)
                    {
                        endPos = buttonsPositionsInSentence[i] + new Vector2(
                        buttonsInTheSentence[buttonsInTheSentence.Count - 1].GetComponent<RectTransform>().rect.width / 2 + 20 + answerWordsButtons[button].GetComponent<RectTransform>().rect.width / 2, 0);
                        
                        heightOfNextWordsInSentence = endPos.y;
                    }
                    else//else put it in the next line
                    {   //Set the height of the word equal to the last button in the sentence minus their half height minus 20 units to space them
                        heightOfNextWordsInSentence = heightOfNextWordsInSentence -
                        buttonsInTheSentence[buttonsInTheSentence.Count - 1].GetComponent<RectTransform>().rect.height / 2 -
                        answerWordsButtons[button].GetComponent<RectTransform>().rect.height / 2 - 20;
                        //lower the hight of the next word by an extra amount to make up for the underline width of the button
                        heightOfNextWordsInSentence -= 7.4f;

                        endPos = new Vector2(-sentenceBoudaries + answerWordsButtons[button].GetComponent<RectTransform>().rect.width / 2, heightOfNextWordsInSentence);
                    }
                    buttonsPositionsInSentence[button] = endPos;
                    break;
                }
            }
        }
        #region Button Transportation

        AnswersButtonsInteractability(false, button);

        float duration = 0.08f;
        float timeElapsed = 0f;
        float percentageCompleted = 0f;
        //If button is on the starting position, move it to sentence
        if (answerWordsButtons[button].transform.parent.GetComponent<RectTransform>().anchoredPosition == buttonsStartingPositions[button])
        {
            buttonBackgrounds[button].SetActive(true);

            buttonsInTheSentence.Add(answerWordsButtons[button]);
            while (percentageCompleted < 1)
            {
                timeElapsed += Time.deltaTime;
                percentageCompleted = timeElapsed / duration;

                answerWordsButtons[button].transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startingPos, buttonsPositionsInSentence[button], percentageCompleted);

                yield return null;
            }
        }
        else//Else, bring it back to starting position 
        {   //if there are at least two buttons and this is the last button
            if (buttonsInTheSentence.Count > 1 && buttonsInTheSentence[buttonsInTheSentence.Count - 1] == answerWordsButtons[button])
            {   //Set the height equal to the second last button
                heightOfNextWordsInSentence = buttonsInTheSentence[buttonsInTheSentence.Count - 2].transform.parent.GetComponent<RectTransform>().anchoredPosition.y;
            }

            Vector2 buttonEndPosition = answerWordsButtons[button].transform.parent.GetComponentInChildren<RectTransform>().anchoredPosition;
            //Find the button that's leaving the sentence and remove it from the list
            for (int i = 0; i < buttonsInTheSentence.Count; i++)
            {
                if (buttonsInTheSentence[i] == answerWordsButtons[button])
                {
                    StartCoroutine(RepositionButtonsInSentence(i, button));
                    break;
                }
            }
            //If the new sentence has no words
            if (buttonsInTheSentence.Count == 0)
            {
                buttonsController.DisableCheckButton();
            }

            while (percentageCompleted < 1)
            {
                timeElapsed += Time.deltaTime;
                percentageCompleted = timeElapsed / duration;

                answerWordsButtons[button].transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(buttonEndPosition, startingPos, percentageCompleted);

                yield return null;
            }
            buttonBackgrounds[button].SetActive(false);
        }
        AnswersButtonsInteractability(true, button);
        #endregion
    }

    IEnumerator RepositionButtonsInSentence(int buttonToRemoveFromSentence, int buttonToRemove) // first int is an index to the button in sentence, second index is an index to Buttons
    {
        Vector2 startingPos = new Vector2();
        Vector2 endOfPos = new Vector2();
        float duration = 0.08f;
        float flag = 0;
        RectTransform lastButton = answerWordsButtons[buttonToRemove].transform.parent.GetComponent<RectTransform>();

        //Variables that will store the previous buttons' position and width
        Vector2 lastButtonPos = lastButton.anchoredPosition;
        float lastButtonWidth = answerWordsButtons[buttonToRemove].GetComponent<RectTransform>().rect.width;
        float lastButtonHeight = answerWordsButtons[buttonToRemove].GetComponent<RectTransform>().rect.height;

        buttonsInTheSentence.RemoveAt(buttonToRemoveFromSentence);

        //For every button in sentence after the one pressed
        for (int i = buttonToRemoveFromSentence; i < buttonsInTheSentence.Count; i++)
        {
            RectTransform currentButton = buttonsInTheSentence[i].transform.parent.GetComponent<RectTransform>(); // Used to get the starting position of the button in the sentence
            float currentButtonWidth = buttonsInTheSentence[i].GetComponent<RectTransform>().rect.width; //Used to calculate the exact position of the endOfPos
            float currentButtonHeight = buttonsInTheSentence[i].GetComponent<RectTransform>().rect.height; //Used to calculate the height on which it will go if it doesn't fit the sentence

            startingPos = currentButton.anchoredPosition;
            #region Set end position of the button
            if (flag == 0) //if this is the first button to be moved, the position of the button should start at the start of the previous button
            {
                endOfPos = lastButtonPos + new Vector2(-lastButtonWidth / 2 + currentButtonWidth / 2, 0);
                
                flag++;
            }
            else //else put the position next to the last button
            {
                endOfPos = lastButtonPos + new Vector2(lastButtonWidth / 2 + currentButtonWidth / 2 + 20, 0);
            }
            //if out of boundaries
            if (endOfPos.x + currentButtonWidth / 2 > sentenceBoudaries)
            {
                heightOfNextWordsInSentence = lastButtonPos.y - lastButtonHeight / 2 - currentButtonHeight / 2 - 20;
                heightOfNextWordsInSentence -= 7.4f;
                endOfPos = new Vector2(-sentenceBoudaries + currentButtonWidth / 2, heightOfNextWordsInSentence);
            }
            #endregion

            //for all the buttons
            for (int j = 0; j < answerWordsButtons.Count; j++)
            {   //find the button that is going to relocate in the sentence and store its new position
                if (answerWordsButtons[j] == buttonsInTheSentence[i])
                {
                    buttonsPositionsInSentence[j] = endOfPos;
                    break;
                }
            }

            // if this is the last button to be moved, store it's height value
            if (buttonsInTheSentence[i] == buttonsInTheSentence[buttonsInTheSentence.Count - 1])
            {
                heightOfNextWordsInSentence = endOfPos.y;
            }
            //---Button Relocation----------------------------------------------------------------------------------------------
            AnswersButtonsInteractability(false, i);

            float timeElapsed = 0f;
            float percentageCompleted = 0f;
            while (percentageCompleted < 1)
            {
                timeElapsed += Time.deltaTime;
                percentageCompleted = timeElapsed / duration;

                currentButton.anchoredPosition = Vector2.Lerp(startingPos, endOfPos, percentageCompleted);

                yield return null;
            }
            AnswersButtonsInteractability(true, i);            

            lastButtonPos = currentButton.anchoredPosition;
            lastButtonWidth = currentButtonWidth;
            lastButtonHeight = currentButtonHeight;
        }
    }

    
    #endregion

    #region Button Sizing
    IEnumerator AddContentSizeFitterToButtons()
    {
        for (int i = 0; i < numberOfWordsInSentence; i++)
        {
            answerWordsButtons[i].gameObject.GetComponent<ContentSizeFitter>().enabled = true;
            answerWordsButtons[i].gameObject.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }
        yield return new WaitForEndOfFrame();

        StartCoroutine(DeactivateContentSizeFitterFromButtons());
        SetInitialPositionOfButtons();
    }

    IEnumerator SetBackgroundImages()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < numberOfWordsInSentence; i++)
        {
            //Set up the background image
            buttonBackgrounds[i].GetComponent<RectTransform>().anchoredPosition = buttonsStartingPositions[i];
            buttonBackgrounds[i].GetComponent<Image>().pixelsPerUnitMultiplier = 19;

            buttonBackgrounds[i].GetComponent<RectTransform>().sizeDelta = answerWordsButtons[i].gameObject.GetComponent<RectTransform>().sizeDelta;
        }
        imageToHideButtons.SetActive(false);
    }

    IEnumerator DeactivateContentSizeFitterFromButtons() //Called after AddContentSizeFitterToButtons(), it is used to deactivate the content size fitter so the button can be re-adjusted to appear bigger
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < numberOfWordsInSentence; i++)
        {   //Deactivate the content size fitter to be able to change the button size and horizontal layout group
            answerWordsButtons[i].gameObject.GetComponent<ContentSizeFitter>().enabled = false;
            //Resize the buttons to a bigger value
            answerWordsButtons[i].GetComponent<RectTransform>().sizeDelta += new Vector2(80, 40);

            answerWordsButtons[i].GetComponent<Image>().sprite = buttonImage;
            answerWordsButtons[i].GetComponent<Image>().pixelsPerUnitMultiplier = 19;


            //Setup the underline image of every button
            GameObject image = answerWordsButtons[i].transform.parent.Find("Image").gameObject;

            image.GetComponent<RectTransform>().anchoredPosition = answerWordsButtons[i].GetComponent<RectTransform>().anchoredPosition - new Vector2(0f, 7.4f);
            image.GetComponent<RectTransform>().sizeDelta = answerWordsButtons[i].GetComponent<RectTransform>().sizeDelta;
            image.GetComponent<Image>().sprite = buttonUnderlineImage;
            image.GetComponent<Image>().pixelsPerUnitMultiplier = 19;

        }
    }
    #endregion
    void OnButtonPress(int button) //What should happen if the player presses on a button
    {   //If the button's position is equal to its starting position
        if (answerWordsButtons[button].transform.parent.GetComponent<RectTransform>().anchoredPosition == buttonsStartingPositions[button])
        {
            if (!buttonsController.confirmationButton.interactable)
            {
                buttonsController.EnableCheckButton();
            }
        }

        StartCoroutine(MoveButtonOnClick(button));
    }

    void AnswersButtonsInteractability(bool flag, int i) //Controls the buttons' interactability
    {
        if (flag)
        {
            answerWordsButtons[i].interactable = true;
            answerWordsButtons[i].GetComponent<Image>().raycastTarget = true;
            answerWordsButtons[i].GetComponentInChildren<TextMeshProUGUI>().raycastTarget = true;
        }
        else
        {
            answerWordsButtons[i].interactable = false;
            answerWordsButtons[i].GetComponent<Image>().raycastTarget = false;
            answerWordsButtons[i].GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
        }
    }
    public void ResetValues()
    {
        imageToHideButtons.SetActive(true);

        questionText.text = "";
        questionText.gameObject.SetActive(false);
        buttonsInTheSentence.Clear();
        buttonsStartingPositions.Clear();
        buttonsPositionsInSentence.Clear();
        heightOfInstantiatedWord = 0;
        randomWordStartingPoint = Random.Range(500, 600);
        
        //for all the buttons that have been enabled
        for (int i = 0; i < numberOfWordsInSentence; i++)
        {
            //Disable the HorizontalLayoutGroup
            answerWordsButtons[i].gameObject.GetComponent<HorizontalLayoutGroup>().enabled = false;
            //Disable the buttons' parents and backgrounds
            answerWordsButtons[i].transform.parent.gameObject.SetActive(false);
            buttonBackgrounds[i].SetActive(false);
            //Remove the buttons' sprite so it can be re-assigned the next time to be displayed correctly
            answerWordsButtons[i].gameObject.GetComponent<Image>().sprite = null;
        }

        gameObject.SetActive(false);

        masterQuizController.formTheSentenceCorrectlyController.SetActive(false);
        masterQuizController.formTheSentenceCorrectlyModeGameObject.SetActive(false);
    }
}