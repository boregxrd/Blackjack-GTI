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
    public Text finalMessage;
    public Text probMessage;
    public Text bancaMessage;
    public Text apuestaMessage;

    public int[] values = new int[52];
    int cardIndex = 0;    
       
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
            finalMessage.text = "El Jugador ha hecho Blackjack";
            hitButton.interactable = false;
            stickButton.interactable = false;
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
        if (dealer.GetComponent<CardHand>().points == 21)
        {

            finalMessage.text = "El Dealer ha hecho Blackjack";
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true); /
            hitButton.interactable = false;
            stickButton.interactable = false;
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
    }

    private void CalculateProbabilities()
    {

        // Probabilidad de que el jugador obtenga más de 21 si pide una carta
        string probMayor21 = "";

        if (player.GetComponent<CardHand>().points < 12)
        {
            probMayor21 = "\n - X > 21: 0.0";
        }
        else
        {
            int puntosParaPasarse = 22 - player.GetComponent<CardHand>().points;
            float cartasQueSobrepasan = 12;

            for (int i = puntosParaPasarse; i <= 9; i++)
            {
                cartasQueSobrepasan += 4;
            }

            for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
            {
                if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= puntosParaPasarse && i > 0)
                {
                    cartasQueSobrepasan -= 1;
                }
            }

            for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
            {
                if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= puntosParaPasarse)
                {
                    cartasQueSobrepasan -= 1;
                }
            }

            Debug.Log("Pasar de 21");
            Debug.Log("puntos para pasarse: " + puntosParaPasarse + ", cartas que sobrepasan: " + cartasQueSobrepasan + ", Cartas en la mesa: " + cardIndex);

            probMayor21 = "\n X > 21: " + cartasQueSobrepasan / (52.0 - cardIndex * 0.0);
        }

        string probEntre17 = "";

        int cartasHasta17 = 0;
        int cartasHasta21 = 21 - player.GetComponent<CardHand>().points;
        float cartasQueEntran = 0;

        if (player.GetComponent<CardHand>().points < 17)
        {
            cartasHasta17 = 17 - player.GetComponent<CardHand>().points;
        }

        if (cartasHasta21 > 11)
        {
            cartasHasta21 = 11;
        }

        if (player.GetComponent<CardHand>().points < 6)
        {
            probEntre17 = "\n - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta: 0.0";
        }
        else
        {

            for (int i = cartasHasta17; i <= cartasHasta21; i++)
            {
                if (i == 10)
                {
                    cartasQueEntran += 12;
                }
                else
                {
                    cartasQueEntran += 4;
                }

            }

            for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
            {
                if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= cartasHasta17 && dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value <= cartasHasta21 && i > 0)
                {
                    cartasQueEntran -= 1;
                }
            }

            for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
            {
                if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= cartasHasta17 && player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value <= cartasHasta21)
                {
                    cartasQueEntran -= 1;
                }
            }

            Debug.Log("Entre 17 y 21");
            Debug.Log("cartas hasta 17: " + cartasHasta17 + ", cartas hasta 21: " + cartasHasta21 + ", cartas que entran: " + cartasQueEntran + ", Cartas en la mesa: " + cardIndex);

            probEntre17 = "\n - 17<=X<=21: " + cartasQueEntran / (52.0 - cardIndex * 0.0);
        }

        string probCartaOculta = "";
        int puntosPlayer = player.GetComponent<CardHand>().points;
        int puntosDelaer = dealer.GetComponent<CardHand>().points - dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().value;
        int puntosDiferencia = 0;

        if (puntosPlayer < puntosDelaer)
        {
            probCartaOculta = "\n - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador:  1.0";
        }
        else
        {

            puntosDiferencia = puntosPlayer - puntosDelaer;

            if (puntosDiferencia > 11)
            {
                probCartaOculta = "\n - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador:  0.0";
            }
            else
            {
                cartasQueEntran = 0;

                for (int i = puntosDiferencia++; i <= 11; i++)
                {

                    if (i == 10)
                    {
                        cartasQueEntran += 12;
                    }
                    else
                    {
                        cartasQueEntran += 4;
                    }

                }

                for (int i = 0; i < dealer.GetComponent<CardHand>().cards.Count; i++)
                {
                    if (dealer.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= puntosDiferencia && i > 0)
                    {
                        cartasQueEntran -= 1;
                    }
                }

                for (int i = 0; i < player.GetComponent<CardHand>().cards.Count; i++)
                {
                    if (player.GetComponent<CardHand>().cards[i].GetComponent<CardModel>().value >= puntosDiferencia)
                    {
                        cartasQueEntran -= 1;
                    }
                }

                Debug.Log("Carta Oculta");
                Debug.Log("Puntos Player: " + puntosPlayer + ", puntos Dealer: " + puntosDelaer + ", Puntos Diferencia: " + puntosDiferencia + ", cartas que entran: " + cartasQueEntran);

                probCartaOculta = "\n - Deal > Play:  " + cartasQueEntran / (52.0 - cardIndex * 0.0);
            }

            probMessage.text = probCartaOculta + "\n\n" + probEntre17 + "\n\n" + probMayor21;
        }

    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }       

    public void Hit()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true); 

       PushPlayer();

        if (player.GetComponent<CardHand>().points > 21) 
        {
            finalMessage.text = "El Jugador ha perdido";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        bool fin = false;

        do 
        {
            if (dealer.GetComponent<CardHand>().points <= 16) 
            {
                PushDealer(); 
            }
            else
            {
                if (player.GetComponent<CardHand>().points < dealer.GetComponent<CardHand>().points && dealer.GetComponent<CardHand>().points <= 21) 
                {
                    finalMessage.text = "El Dealer ha GANADO";
                }
                else
                {
                    finalMessage.text = "El Juegador ha GANADO";
                    player.GetComponent<CardHand>().banca += player.GetComponent<CardHand>().apuesta * 2; 
                }

                if (player.GetComponent<CardHand>().points == dealer.GetComponent<CardHand>().points) 
                {
                    finalMessage.text = "EMPATE";
                    player.GetComponent<CardHand>().banca += player.GetComponent<CardHand>().apuesta; 
                }
                fin = true; // Salimos del bucle
                hitButton.interactable = false;
                stickButton.interactable = false;
            }
        } while (!fin);

    }
    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        bancaMessage.text = "Banca: " + player.GetComponent<CardHand>().banca.ToString() + " €";
        apuestaMessage.text = "Apuesta: 0 €";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

    public void apostar()
    {
        if (player.GetComponent<CardHand>().banca != 0) 
        {
            player.GetComponent<CardHand>().banca -= 10;
            player.GetComponent<CardHand>().apuesta += 10;
            bancaMessage.text = "Banca: " + player.GetComponent<CardHand>().banca.ToString() + " €"; 
            apuestaMessage.text = "Apuesta: " + player.GetComponent<CardHand>().apuesta.ToString() + " €";
        }

    }

    
}
