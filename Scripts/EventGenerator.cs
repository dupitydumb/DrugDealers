using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class EventGenerator : MonoBehaviour
{
    public static EventGenerator Instance { get; private set; }
    private List<GameEvent> events; // List to hold events
    public PlayerStatus playerStatus; // Player's status


    [Header("UI")]
    public TMP_Text healthText;
    public TMP_Text reputationText;

    public GenerateLog log = new GenerateLog();
    public GameObject logPanel;
    public GameObject textPrefab;

    private float minEventTime = 5f;
    private float maxEventTime = 10f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // Initialize player status
        playerStatus = Manager.Instance.player.playerStatus;

        // Initialize the events list and add some events with effects
        events = new List<GameEvent>
        {
            // Generate a random event that increases the player's health by 10
            new GameEvent(TextUtils.ColorText("Stranger gave you a apple, he thought you looked hungry. Your health increased by 10.", "green"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Health += 10 }, 0.2f),
            // Generate a random event that decreases the player's health by 10
            new GameEvent(TextUtils.ColorText("You were hit by a car. Your health decreased by 10.", "red"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Health -= 10 }, 0.2f),
            // Generate a random event that increases player money by 50-100
            new GameEvent(TextUtils.ColorText("You found a wallet on the street. You found $50.", "green"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Funds += 50 }, 0.2f),
            // Generate a random event that decreases player money by 50-100
            new GameEvent(TextUtils.ColorText("You were pickpocketed. You lost $50.", "red"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Funds -= 50 }, 0.2f),
            // Generate a random event that increases player reputation by 10
            new GameEvent(TextUtils.ColorText("You helped an old lady cross the street. Your reputation increased by 10.", "green"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Reputation += 10 }, 0.2f),
            // Generate a random event that decreases player reputation by 10
            new GameEvent(TextUtils.ColorText("You were caught shoplifting. Your reputation decreased by 10.", "red"),
            new List<Action<PlayerStatus>> { playerStatus => playerStatus.Reputation -= 10 }, 0.2f)
            



        };

        // Start the event generation routine
        StartCoroutine(GenerateEventRoutine());
    }

    private IEnumerator GenerateEventRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minEventTime, maxEventTime)); // Wait for a random time
            GenerateAndLogEvent(); // Generate and log an event
        }
    }

    private void GenerateAndLogEvent()
    {
        float totalProbability = events.Sum(e => e.Probability); // Calculate the total probability of all events
        float randomValue = UnityEngine.Random.Range(0, totalProbability); // Generate a random value between 0 and the total probability
        float cumulative = 0; // Variable to store the cumulative probability

        foreach (var gameEvent in events)
        {
            cumulative += gameEvent.Probability; // Add the current event's probability to the cumulative variable

            if (randomValue <= cumulative) // Check if the random value is less than or equal to the cumulative probability
            {
                //Random text color
                string[] colors = { "green", "red", "blue", "yellow", "orange", "purple" };
                string randomColor = colors[UnityEngine.Random.Range(0, colors.Length)];

                // Log the event
                log.LogEvent(gameEvent.logText, randomColor, textPrefab);

                // Apply the effects of the event on the player
                foreach (var effect in gameEvent.EffectsOnPlayer)
                {
                    effect(playerStatus);
                }

                Manager.Instance.player.playerStatus.UpdateUI(); // Update the player's UI

                break; // Exit the loop
            }
        }
    }
}


public class GenerateLog
{
    public void LogEvent(string logText, string Color, GameObject textPrefab)
    {
        //Format if text is interpolated
        //Create a new text object
        GameObject newText = UnityEngine.Object.Instantiate(textPrefab, GameObject.FindWithTag("LogText").transform);
        newText.GetComponentInChildren<TMPro.TMP_Text>().text = logText;
        //Convert the color string to a color
        Color color;
        ColorUtility.TryParseHtmlString(Color, out color);
        newText.GetComponentInChildren<TMPro.TMP_Text>().color = color;
    }

}

public class GameEvent
{
    public string logText;
    public List<Action<PlayerStatus>> EffectsOnPlayer { get; set; }
    public float Probability { get; set; }

    public GameEvent(string logText, List<Action<PlayerStatus>> EffectOnPlayer, float Probability)
    {
        this.logText = logText;
        this.EffectsOnPlayer = EffectOnPlayer;
        this.Probability = Probability;
    }
}


public static class TextUtils
{
    public static string ColorText(string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }
}