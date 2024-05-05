using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    public GameObject card;
    public bool isDealer = false;
    public int points;
    private int coordY;


    public int bank = 1000;
    public int bet;
    public int dealerSecondCard;

    private void Awake()
    {
        points = 0;
        if (isDealer)
            coordY = 3;
        else
            coordY = -1;
    }

    public void Clear()
    {
        points = 0;
        if (isDealer)
            coordY = 3;
        else
            coordY = -1;
        foreach (GameObject g in cards)
        {
            Destroy(g);
        }
        cards.Clear();

        bet = 0; 
    }        

    public void InitialToggle()
    {
        // Despues del primer movimiento dar la vuelta a carta 1 del dealer
        cards[0].GetComponent<CardModel>().ToggleFace(true);              
    }

    public void Push(Sprite front, int value)
    {
        GameObject cardCopy = (GameObject)Instantiate(card);
        cards.Add(cardCopy);

        float coordX = 1.4f * (float)(cards.Count - 4);
        Vector3 pos = new Vector3(coordX, coordY);
        cardCopy.transform.position = pos;

        cardCopy.GetComponent<CardModel>().front = front;
        cardCopy.GetComponent<CardModel>().value = value;

        
        if (isDealer && cards.Count <= 1)
        {
            // primera carta boca abajo
            cardCopy.GetComponent<CardModel>().ToggleFace(false);
        }
        else if (isDealer && cards.Count == 2)
        {
            // sacar valor de segunda carta para puntos iniciales
            dealerSecondCard = cardCopy.GetComponent<CardModel>().value;
            cardCopy.GetComponent<CardModel>().ToggleFace(true);
        }
        else
        {
            // demas cartas boca arriba
            cardCopy.GetComponent<CardModel>().ToggleFace(true);
        }
        
        multipleAces(cardCopy);
    }

    private void multipleAces(GameObject cardCopy)
    {
        // metodo para que solo un As valga 11 puntos
        int val = 0;
        int aces = 0;
        foreach (GameObject f in cards)
        {
            if (f.GetComponent<CardModel>().value != 11)
                val += f.GetComponent<CardModel>().value;
            else
                aces++;
        }

        for (int i = 0; i < aces; i++)
        {
            if (val + 11 <= 21)
            {
                val = val + 11;
            }
            else
            {
                val = val + 1;
            }
        }

        points = val;
    }
}
