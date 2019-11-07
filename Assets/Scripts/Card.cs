using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum CardType { Question, Answer}

public class Card : MonoBehaviour {

    #region Properties
    public Texture backSprite;
    public Texture frontSprite;
    [HideInInspector] public RectTransform rTransform;
    public bool isShowing { get; set; }
    public bool foundMatch;
    
    
    
    #endregion

    #region Constructor
    public Card()
    {
        isShowing = false;        
    }
   
    #endregion

    #region MonoBehaviour Callbacks
    void Awake()
    {
        isShowing = false;
        foundMatch = false;
        rTransform = GetComponent<RectTransform>();             
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => GameManager.S.Flip(this));        
    }
    #endregion

    #region Methods
    public override bool Equals(object other)
    {
        Card a = other as Card;        
        return this.frontSprite.name == a.frontSprite.name;       
    }    

    public void ChangeSprite()  // gets called via animator
    {
        
        if (rTransform.localScale.x >= 0)
        {
            if (GetComponent<RawImage>().texture == frontSprite)            
                GetComponent<RawImage>().texture = backSprite;           
            else            
                GetComponent<RawImage>().texture = frontSprite;                          
        }
        isShowing = GetComponent<RawImage>().texture == frontSprite;

        if(isShowing)
            GameManager.S.CheckOpenCards();
        
    }   

    #endregion

}
