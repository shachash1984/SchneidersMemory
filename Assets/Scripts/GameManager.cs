using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour {

    #region Properties
    public bool gameOver;
    public bool gamePaused;
    public int pairsFound;
    public float timeLeft;
    public int score;
    public GameObject endGamePanel;
    [SerializeField] private Card CardPrefab;
    [SerializeField] private List<Texture> cardFaces;
    [SerializeField] private List<Vector3> cardPositions;
    [SerializeField] private GameObject panel;
    [SerializeField] private Texture BGSprite;
    [SerializeField] private Texture startSprite;
    [SerializeField] private GameObject pausePanel;
    private int symbolAmt = 6;
    [SerializeField] private List<Card> cards = new List<Card>();
    private int cardAmt = 12;
    [SerializeField] private Text timeText;
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip successSound;
    public AudioClip winMusic;
    public AudioClip failSound;
    #endregion

    #region Constructor (Singleton)
    static public GameManager S;
    private GameManager() { }
    public GameManager GetInstance()
    {
        if (S == null)
            S = new GameManager();
        return S;
    }
    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        if (S != null)
            Destroy(this.gameObject);
        else
            S = this;
        LoadStartScreen();  
    }    

    void Update()
    {        
        if (timeLeft <= 0f && !gameOver && !gamePaused)
            timeLeft = 0;
        else if (!gameOver && !gamePaused)
            timeLeft -= Time.deltaTime;

        try
        {
            timeText.text = ":רתונש ןמז " + timeLeft.ToString().Remove(3);
        }
        catch (ArgumentOutOfRangeException)
        {
            timeText.text = ":רתונש ןמז " + timeLeft.ToString();
        }        

        if (!gameOver &&(pairsFound >= 6 || timeLeft == 0f))
            StartCoroutine(EndGame());

        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();

    }
    #endregion

    #region Methods
    void LoadStartScreen()
    {
        gameOver = true;        
        endGamePanel.SetActive(true);
        endGamePanel.GetComponentInChildren<Text>().text = "";
        
    }

    public void Init()
    {
        Debug.Log("Init");
        foreach (Card c in cards)
        {            
            Destroy(c.gameObject);
        }
        cards.Clear();
        gameOver = false;        
        
        endGamePanel.SetActive(false);
        for (int i = 0; i < cardAmt; i++)
        {            
            Card tempCard = Instantiate(CardPrefab, new Vector3(0,-340), Quaternion.identity, panel.transform) as Card;
            tempCard.GetComponent<RawImage>().texture = tempCard.backSprite;
            tempCard.frontSprite = cardFaces[i % symbolAmt];            
            int index = UnityEngine.Random.Range(0, cardPositions.Count);
            tempCard.rTransform.localPosition = cardPositions[index];                       
            cardPositions.RemoveAt(index);
            cards.Add(tempCard);         
        }

        for (int i = 0; i < cardAmt; i++) //restting position list
        {
            if (i <= 3)
                cardPositions.Add(new Vector3((i * 320) - 485, 300));
            else if (i <= 7)
                cardPositions.Add(new Vector3(((i % 4) * 320) - 485, -10));
            else
                cardPositions.Add(new Vector3(((i % 4) * 320) - 485, -320));
        }

        ResetTimer();        
    }

    private void ResetTimer()
    {
        timeLeft = 30f;        
    }

    IEnumerator EndGame()
    {        
        score = pairsFound;        
        gameOver = true;
        pairsFound = 0;        
        yield return new WaitForSeconds(1f);
        endGamePanel.GetComponent<RawImage>().texture = BGSprite;
        endGamePanel.SetActive(true);
        if (timeLeft > 0)
        {
            endGamePanel.GetComponentInChildren<Text>().text = "!דובכה לכ";
            audioSource.clip = winMusic;
            audioSource.Play();
        }

        else
        {
            endGamePanel.GetComponentInChildren<Text>().text = "...ןמזה רמגנ" + "\n" + ":ואצמנש תוגוז" + "\n" + score;
            audioSource.clip = failSound;
            audioSource.Play();
        }
            
        StopCoroutine(EndGame());
    }

    public bool CompareCards(Card c, Card d)
    {        
        if (c.isShowing && d.isShowing && !c.foundMatch && !d.foundMatch)
        {
            c.foundMatch = c.Equals(d);
            d.foundMatch = c.foundMatch;
            if(c.foundMatch)
            {
                audioSource.clip = successSound;
                audioSource.Play();
            }
            return c.foundMatch;
        }                
        else
            return false;        
    }

    public void Flip(Card c)
    {
        if (!gameOver)
        {
            int openCardsCounter = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].isShowing && !cards[i].foundMatch)
                    openCardsCounter++;
            }
            if (openCardsCounter <= 1)
            {
                c.GetComponent<Animator>().Play("Flip");
                audioSource.clip = flipSound;
                audioSource.Play();
            }
                
        }        
    }

    public void CheckOpenCards()
    {
        if (!gameOver)
        {
            List<Card> pair = new List<Card>();
            for (int i = 0; i < cards.Count; i++)
            {
                if (!cards[i].foundMatch && cards[i].isShowing)
                    pair.Add(cards[i]);

                if (pair.Count == 2)
                {
                    if (CompareCards(pair[0], pair[1]))
                    {
                        foreach (Card t in pair)
                        {
                            t.foundMatch = true;
                            t.GetComponent<Button>().enabled = false;
                            t.GetComponent<Animator>().Play("Correct");
                        }
                        pairsFound++;                        
                        return;
                    }
                    else
                        StartCoroutine(HideUnMatchingCards(pair.ToArray()));
                }
            }
        }        
    }

    public IEnumerator HideUnMatchingCards(params Card[] pair)
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < pair.Length; i++)
        {
            pair[i].GetComponent<Animator>().Play("Flip");
        }
        StopCoroutine(HideUnMatchingCards());
    }

    /*public IEnumerator SetCardsInPosition(Card c, Vector3 pos, int index)
    {
        c.rTransform.localPosition = Vector3.Lerp(c.rTransform.localPosition, pos, 0.3f);
        yield return new WaitForSeconds(0.1f);

        if (c.rTransform.localPosition == pos)
        {
            StopCoroutine(SetCardsInPosition(c, pos, index));
            cardPositions.RemoveAt(index);
            cards.Add(c);
        }
            
    }*/

    public void Pause()
    {
        pausePanel.SetActive(true);
        gamePaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        gamePaused = false;
    }
    
    public void Quit()
    {
        Application.Quit();
    }
    #endregion
}
