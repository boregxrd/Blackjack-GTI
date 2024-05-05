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
    public Text bancaMessage;
    public Text apuestaMessage;
    public Text ptosDealer;
    public Text ptosJugador;

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
            finalMessage.text = "Has hecho Blackjack";

            desactivarButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
        if (dealer.GetComponent<CardHand>().points == 21)
        {

            finalMessage.text = "El Dealer ha hecho Blackjack";
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

            desactivarButtons();

            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
    }

    private void CalculateProbabilities()
    {

        // Probabilidad de que el jugador obtenga más de 21 si pide una carta
        string probMayor21 = "";

        if (player.GetComponent<CardHand>().points < 12)
        {
            probMayor21 = "\n X > 21: 0.0";
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

            probMayor21 = "\n X > 21: " + (cartasQueSobrepasan / (52.0 - cardIndex * 0.0)).ToString("F2");
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
            probEntre17 = "\n 17<=X<=21: 0.0";
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

            probEntre17 = "\n 17<=X<=21: " + (cartasQueEntran / (52.0 - cardIndex * 0.0)).ToString("F2");
        }

        string probCartaOculta = "";
        int puntosPlayer = player.GetComponent<CardHand>().points;
        int puntosDelaer = dealer.GetComponent<CardHand>().points - dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().value;
        int puntosDiferencia = 0;

        if (puntosPlayer < puntosDelaer)
        {
            probCartaOculta = "\n Deal > Play:  1.0";
        }
        else
        {

            puntosDiferencia = puntosPlayer - puntosDelaer;

            if (puntosDiferencia > 11)
            {
                probCartaOculta = "\n Deal > Play:  0.0";
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

                probCartaOculta = "\n Deal > Play:  " + (cartasQueEntran / (52.0 - cardIndex * 0.0)).ToString("F2");
            }

            probMessage.text = probCartaOculta + "\n\n" + probEntre17 + "\n\n" + probMayor21;
        }

    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        actualizarPuntos();
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        actualizarPuntos();
        CalculateProbabilities();
    }

    public void Hit()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Oh... ¡Te has pasado!";

            desactivarButtons();
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
                    finalMessage.text = "¡Has perdido!";

                }
                else if (player.GetComponent<CardHand>().points == dealer.GetComponent<CardHand>().points)
                {
                    finalMessage.text = "Empate";
                    player.GetComponent<CardHand>().banca += player.GetComponent<CardHand>().apuesta;
                }
                else
                {
                    finalMessage.text = "¡Has ganado!";
                    player.GetComponent<CardHand>().banca += player.GetComponent<CardHand>().apuesta * 2;
                }

                fin = true;

                desactivarButtons();
            }
        } while (!fin);

    }
    public void PlayAgain()
    {
        activarButtons();
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

    private void desactivarButtons()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        apostarButton.interactable = false;
    }

    private void activarButtons()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        apostarButton.interactable = true;
    }

    private void actualizarPuntos()
    {
        ptosJugador.text = "Puntos: " + player.GetComponent<CardHand>().points.ToString();
        if (player.GetComponent<CardHand>().cards.Count == 2)
            ptosDealer.text = "Puntos: " + dealer.GetComponent<CardHand>().segundaDealer;
        else if (dealer.GetComponent<CardHand>().cards.Count > 2)
            ptosDealer.text = "Puntos: " + dealer.GetComponent<CardHand>().points.ToString();
    }
}
