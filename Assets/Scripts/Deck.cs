using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Button apostarButton;
    public Text finalMessage;
    public Text probMessage;
    //nuestras variables
    public Text bankMessage;
    public Text betMessage;
    public Text dealerPoints;
    public Text playerPoints;

    public int[] values = new int[52];
    int cardIndex = 0;
    //bandera del caso especifico de derrota
    bool dealerWinsWOBlackJack = false;

    private void Awake()
    {
        InitCardValues();

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }

    private void InitCardValues()
    {
        int palo = 0;

        for (int i = 0; i < 52; i++)
        {
            palo++;

            if (palo <= 10)
            {
                if (palo == 1)
                {
                    values[i] = 11;
                }
                else
                {
                    values[i] = palo;
                }
            }
            else
            {
                values[i] = 10;
                if (palo == 13) palo = 0;
            }
        }
    }

    private void ShuffleCards()
    {
        int rnd;
        int temp;
        Sprite tempSprite;

        for (int i = 51; i >= 0; i--)
        {
            rnd = Random.Range(0, 52);

            temp = values[i];
            values[i] = values[rnd];
            values[rnd] = temp;

            tempSprite = faces[i];
            faces[i] = faces[rnd];
            faces[rnd] = tempSprite;
        }
    }


    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {

            PushDealer();
            PushPlayer();
        }
        if (player.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "Has hecho Blackjack";

            disableButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
        if (dealer.GetComponent<CardHand>().points == 21)
        {

            finalMessage.text = "El Dealer ha hecho Blackjack";
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

            disableButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
    }

    private void CalculateProbabilities()
    {

        // Probabilidad de que el jugador obtenga más de 21 si pide una carta
        string greater21Prob = "";

        if (player.GetComponent<CardHand>().points < 12)
        {
            greater21Prob = "\n X > 21: 0.0";
        }
        else
        {
            int pointsToLose = 22 - player.GetComponent<CardHand>().points;
            float cardsThatSurpass = 12;

            for (int i = pointsToLose; i <= 9; i++)
            {
                cardsThatSurpass += 4;
            }

            for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
            {
                if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= pointsToLose && i > 0)
                {
                    cardsThatSurpass -= 1;
                }
            }

            for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
            {
                if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= pointsToLose)
                {
                    cardsThatSurpass -= 1;
                }
            }

            greater21Prob = "\n X > 21: " + (cardsThatSurpass / (52.0 - cardIndex * 0.0)).ToString("F2");
        }

        string between17Prob = "";

        int cardsTill17 = 0;
        int cardsTill21 = 21 - player.GetComponent<CardHand>().points;
        float cardsThatGoIn = 0;

        if (player.GetComponent<CardHand>().points < 17)
        {
            cardsTill17 = 17 - player.GetComponent<CardHand>().points;
        }

        if (cardsTill21 > 11)
        {
            cardsTill21 = 11;
        }

        if (player.GetComponent<CardHand>().points < 6)
        {
            between17Prob = "\n 17<=X<=21: 0.0";
        }
        else
        {

            for (int i = cardsTill17; i <= cardsTill21; i++)
            {
                if (i == 10)
                {
                    cardsThatGoIn += 12;
                }
                else
                {
                    cardsThatGoIn += 4;
                }

            }

            for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
            {
                if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= cardsTill17 && dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value <= cardsTill21 && i > 0)
                {
                    cardsThatGoIn -= 1;
                }
            }

            for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
            {
                if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= cardsTill17 && player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value <= cardsTill21)
                {
                    cardsThatGoIn -= 1;
                }
            }

            between17Prob = "\n 17<=X<=21: " + (cardsThatGoIn / (52.0 - cardIndex * 0.0)).ToString("F2");
        }

        string hiddenCardProb = "";
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points - dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().value;
        int diffPoints = 0;

        if (playerPoints < dealerPoints)
        {
            hiddenCardProb = "\n Deal > Play:  1.0";
        }
        else
        {

            diffPoints = playerPoints - dealerPoints;

            if (diffPoints > 11)
            {
                hiddenCardProb = "\n Deal > Play:  0.0";
            }
            else
            {
                cardsThatGoIn = 0;

                for (int i = diffPoints++; i <= 11; i++)
                {

                    if (i == 10)
                    {
                        cardsThatGoIn += 12;
                    }
                    else
                    {
                        cardsThatGoIn += 4;
                    }

                }

                for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
                {
                    if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= diffPoints && i > 0)
                    {
                        cardsThatGoIn -= 1;
                    }
                }

                for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
                {
                    if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= diffPoints)
                    {
                        cardsThatGoIn -= 1;
                    }
                }

                hiddenCardProb = "\n Deal > Play:  " + (cardsThatGoIn / (52.0 - cardIndex * 0.0)).ToString("F2");
            }

            probMessage.text = hiddenCardProb + "\n\n" + between17Prob + "\n\n" + greater21Prob;
        }

    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        updatePoints();
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        updatePoints();
        CalculateProbabilities();
    }

    public void Hit()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Oh... ¡Te has pasado!";

            disableButtons();
        }
        else if(player.GetComponent<CardHand>().points == 21) 
        {
            finalMessage.text = "Has hecho Blackjack";

            disableButtons();
        }
    }

    public void Stand()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        bool end = false;

        while (!end)
        {
            if (dealer.GetComponent<CardHand>().points <= 16)
            {
                PushDealer();
            }
            else
            {
                if (player.GetComponent<CardHand>().points < dealer.GetComponent<CardHand>().points && dealer.GetComponent<CardHand>().points <= 21)
                {
                    dealerWinsWOBlackJack = true;
                    updatePoints();
                    finalMessage.text = "¡Has perdido!";
                }
                else if (player.GetComponent<CardHand>().points == dealer.GetComponent<CardHand>().points)
                {
                    finalMessage.text = "Empate";
                    player.GetComponent<CardHand>().bank += player.GetComponent<CardHand>().bet;
                }
                else
                {
                    finalMessage.text = "¡Has ganado!";
                    player.GetComponent<CardHand>().bank += player.GetComponent<CardHand>().bet * 2;
                }

                end = true;

                disableButtons();
            }
        } 
    }

    public void PlayAgain()
    {
        enableButtons();
        finalMessage.text = "";
        bankMessage.text = "Banca: " + player.GetComponent<CardHand>().bank.ToString() + " €";
        betMessage.text = "Apuesta: 0 €";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

    public void apostar()
    {
        if (player.GetComponent<CardHand>().bank != 0)
        {
            player.GetComponent<CardHand>().bank -= 10;
            player.GetComponent<CardHand>().bet += 10;
            bankMessage.text = "Banca: " + player.GetComponent<CardHand>().bank.ToString() + " €";
            betMessage.text = "Apuesta: " + player.GetComponent<CardHand>().bet.ToString() + " €";
        }
    }

    private void disableButtons()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        apostarButton.interactable = false;
    }

    private void enableButtons()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        apostarButton.interactable = true;
    }

    private void updatePoints()
    {
        playerPoints.text = "Puntos: " + player.GetComponent<CardHand>().points.ToString();
        dealerPoints.text = "Puntos: " + dealer.GetComponent<CardHand>().dealerSecondCard;
        if (player.GetComponent<CardHand>().cards.Count > 2 || dealer.GetComponent<CardHand>().cards.Count > 2 || player.GetComponent<CardHand>().points == 21 || dealerWinsWOBlackJack)
        {
            dealerPoints.text = "Puntos: " + dealer.GetComponent<CardHand>().points.ToString();
            dealerWinsWOBlackJack = false;
        }

    }
}
