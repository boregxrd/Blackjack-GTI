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

            DisableButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
        if (dealer.GetComponent<CardHand>().points == 21)
        {

            finalMessage.text = "El Dealer ha hecho Blackjack";
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

            DisableButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
    }

    private string CalculateGreater21Probability(int pointsToLose, GameObject player, GameObject dealer, int cardIndex)
    {
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

        return (cardsThatSurpass / (52.0 - cardIndex * 0.0)).ToString("F2");
    }

    private string CalculateBetween17Probability(int cardsTill17, int cardsTill21, GameObject player, GameObject dealer, int cardIndex)
    {
        float cardsThatGoIn = 0;

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

        return (cardsThatGoIn / (52.0 - cardIndex * 0.0)).ToString("F2");
    }

    private string CalculateHiddenCardProbability(int playerPoints, int dealerPoints, int diffPoints, GameObject player, GameObject dealer, int cardIndex)
    {
        float cardsThatGoIn = 0;

        if (diffPoints > 11)
        {
            return "0.0";
        }

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

        return (cardsThatGoIn / (52.0 - cardIndex * 0.0)).ToString("F2");
    }

    private void CalculateProbabilities()
    {
        // Probabilidad de que el jugador obtenga más de 21 si pide una carta
        string greater21Prob = "";
        string between17Prob = "";
        string hiddenCardProb = "";

        int pointsToLose = 22 - player.GetComponent<CardHand>().points;
        int cardsTill17 = 0;
        int cardsTill21 = 21 - player.GetComponent<CardHand>().points;
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points - dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().value;
        int diffPoints = playerPoints - dealerPoints;

        // Cálculo de la probabilidad para obtener más de 21
        if (player.GetComponent<CardHand>().points < 12)
        {
            greater21Prob = "\n X > 21: 0.0";
        }
        else
        {
            greater21Prob = "\n X > 21: " + CalculateGreater21Probability(pointsToLose, player, dealer, cardIndex);
        }

        // Cálculo de la probabilidad para obtener entre 17 y 21
        if (player.GetComponent<CardHand>().points < 6)
        {
            between17Prob = "\n 17<=X<=21: 0.0";
        }
        else
        {
            cardsTill17 = player.GetComponent<CardHand>().points < 17 ? 17 - player.GetComponent<CardHand>().points : 0;
            cardsTill21 = cardsTill21 > 11 ? 11 : cardsTill21;
            between17Prob = "\n 17<=X<=21: " + CalculateBetween17Probability(cardsTill17, cardsTill21, player, dealer, cardIndex);
        }

        // Cálculo de la probabilidad de que la carta oculta del dealer supere la del jugador
        if (playerPoints < dealerPoints)
        {
            hiddenCardProb = "\n Deal > Play:  1.0";
        }
        else
        {
            hiddenCardProb = "\n Deal > Play:  " + CalculateHiddenCardProbability(playerPoints, dealerPoints, diffPoints, player, dealer, cardIndex);
        }

        probMessage.text = hiddenCardProb + "\n\n" + between17Prob + "\n\n" + greater21Prob;
    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        UpdatePoints();
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        UpdatePoints();
        CalculateProbabilities();
    }

    public void Hit()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Oh... ¡Te has pasado!";

            DisableButtons();
        }
        else if(player.GetComponent<CardHand>().points == 21) 
        {
            finalMessage.text = "Has hecho Blackjack";

            DisableButtons();
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
                    UpdatePoints();
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

                DisableButtons();
            }
        } 
    }

    public void PlayAgain()
    {
        EnableButtons();
        finalMessage.text = "";
        bankMessage.text = "Banca: " + player.GetComponent<CardHand>().bank.ToString() + " €";
        betMessage.text = "Apuesta: 0 €";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

    public void Bet()
    {
        if (player.GetComponent<CardHand>().bank != 0)
        {
            player.GetComponent<CardHand>().bank -= 10;
            player.GetComponent<CardHand>().bet += 10;
            bankMessage.text = "Banca: " + player.GetComponent<CardHand>().bank.ToString() + " €";
            betMessage.text = "Apuesta: " + player.GetComponent<CardHand>().bet.ToString() + " €";
        }
    }

    private void DisableButtons()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        apostarButton.interactable = false;
    }

    private void EnableButtons()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        apostarButton.interactable = true;
    }

    private void UpdatePoints()
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
