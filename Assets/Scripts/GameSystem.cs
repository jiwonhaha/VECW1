using System.Collections;
using System.Collections.Generic;
using System;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class GameSystem : MonoBehaviour
{
    NetworkContext context;

    GameObject player;

    public GameObject deviceSimulator;

    [Header("Game Simulation")]
    [SerializeField] bool isInGame;
    public bool isGameReset;

    [Header("UI Buttons")]
    [SerializeField] Button travellerButton;
    [SerializeField] Button inspectorButton;
    [SerializeField] Button supervisorButton;

    [Header("Object Button")]
    [SerializeField] GameObject inspectorPassButton;
    [SerializeField] GameObject inspectorRejectButton;
    [SerializeField] GameObject supervisorPassButton;
    [SerializeField] GameObject supervisorRejectButton;

    [Header("Scene Objects")]
    [SerializeField] GameObject doorGate;
    [SerializeField] GameObject cage;

    [Header("Passport")]
    [SerializeField] GameObject[] passports;

    [Header("Settings")]
    [SerializeField] int numberOfRounds;
    [SerializeField] int numberOfTravellers;
    [SerializeField] int numberOfInspectors;
    [SerializeField] int numberOfSupervisors;

    public int currentRounds;
    bool inspectorHasChosen;
    bool supervisorHasChosen;

    int currentNumberOftraveller = 0;
    int currentNumberOfinspector = 0;
    int currentNumberOfsupervisor = 0;

    private void Awake()
    {
        travellerButton.onClick.AddListener(TagTraveller);
        inspectorButton.onClick.AddListener(TagInspector);
        supervisorButton.onClick.AddListener(TagSupervisor);

#if UNITY_EDITOR
        deviceSimulator.SetActive(true);
#endif
    }

    private void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        context = NetworkScene.Register(this);

        isInGame = false;

        inspectorHasChosen = false;
        supervisorHasChosen = false;
    }

    private void Update()
    {
        // Remove ! to test the real play
        if (isInGame)
        {
            bool inspectorPass = inspectorPassButton.GetComponent<PassActive>().hasChosenChoice;
            bool inspectorReject = inspectorRejectButton.GetComponent<PassActive>().hasChosenChoice;
            bool supervisorPass = supervisorPassButton.GetComponent<PassActive>().hasChosenChoice;
            bool supervisorReject = supervisorRejectButton.GetComponent<PassActive>().hasChosenChoice;

            if (!inspectorHasChosen)
            {
                if (inspectorPass)
                {
                    Debug.Log("Round: " + currentRounds + ", inspector chose passed!");
                    doorGate.SetActive(false);
                    inspectorHasChosen = true;
                }
                if (inspectorReject)
                {
                    Debug.Log("Round: " + currentRounds + ", inspector chose rejected!");
                    cage.SetActive(true);
                    inspectorHasChosen = true;
                }
            }

            if (!supervisorHasChosen)
            {
                if (supervisorPass)
                {
                    Debug.Log("Round: " + currentRounds + ", supervisor chose passed!");
                    supervisorHasChosen = true;
                }
                if (supervisorReject)
                {
                    Debug.Log("Round: " + currentRounds + ", supervisor chose rejected!");
                    supervisorHasChosen = true;
                }
            }

            if (inspectorHasChosen && supervisorHasChosen)
            {
                isGameReset = true;
                StartCoroutine(GameReset());
            }
        }

        else
        {
            // Traveller Button
            if (currentNumberOftraveller == numberOfTravellers)
            {
                ColorBlock colors = travellerButton.colors;
                colors.disabledColor = Color.red; // Set the disabled color to red
                travellerButton.colors = colors;

                travellerButton.interactable = false;
            }
            else
            {
                ColorBlock colors = travellerButton.colors;
                colors.disabledColor = Color.green;
                travellerButton.colors = colors;

                travellerButton.interactable = true;
            }
            // Supervisor Button
            if (currentNumberOfsupervisor == numberOfSupervisors)
            {
                ColorBlock colors = supervisorButton.colors;
                colors.disabledColor = Color.red; // Set the disabled color to red
                supervisorButton.colors = colors;

                supervisorButton.interactable = false;
            }
            else
            {
                ColorBlock colors = supervisorButton.colors;
                colors.disabledColor = Color.green;
                supervisorButton.colors = colors;

                supervisorButton.interactable = true;
            }
            // Inspector Button
            if (currentNumberOfinspector == numberOfInspectors)
            {
                ColorBlock colors = inspectorButton.colors;
                colors.disabledColor = Color.red; // Set the disabled color to red
                inspectorButton.colors = colors;

                inspectorButton.interactable = false;
            }
            else
            {
                ColorBlock colors = inspectorButton.colors;
                colors.disabledColor = Color.green;
                inspectorButton.colors = colors;

                inspectorButton.interactable = true;
            }

            //if (currentNumberOfinspector == numberOfInspectors && currentNumberOfsupervisor == numberOfSupervisors && currentNumberOftraveller == numberOfTravellers)
            if (currentNumberOfinspector == numberOfInspectors && currentNumberOftraveller == numberOfTravellers)
            {
                isInGame = true;
                currentRounds = 1;

                Debug.Log("Start round " + currentRounds);

                SpawnPassport();
                
            }
        }
    }

    private struct ButtonMessage
    {
        public int totalOftraveller;
        public int totalOfinspector;
        public int totalOfsupervisor;
    }

    private void SpawnPassport()
    {

        // Generate a random index between 0 and 3 (since array indices are 0-based)
        int randomIndex = new System.Random().Next(0, passports.Length); // Random.Next is inclusive at the start, exclusive at the end

        // Select the GameObject using the random index
        GameObject selectedPassport = passports[randomIndex];

        // Spawn the selected GameObject
        Instantiate(selectedPassport, new Vector3(7.5f, 0.75f, 30.0f), UnityEngine.Random.rotation);
    }

    IEnumerator GameReset()
    {
        yield return new WaitForSeconds(3);

        GameObject passport = GameObject.FindGameObjectsWithTag("Passport")[0];
        Destroy(passport);

        cage.transform.position = new Vector3(-6f, 3f, 30f);
        cage.SetActive(false);

        doorGate.SetActive(true);

        inspectorHasChosen = false;
        supervisorHasChosen = false;

        isGameReset = false;

        currentRounds++;
        if (currentRounds > numberOfRounds)
        {
            Debug.Log("Game End!");
            isInGame = false;

            player.gameObject.tag = "Player";
            player.transform.position = new Vector3(0f, -0.25f, 0f);

            currentRounds = 0;

            currentNumberOfinspector = 0;
            currentNumberOfsupervisor = 0;
            currentNumberOftraveller = 0;

            ButtonMessage m = new ButtonMessage();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
        else
        {
            Debug.Log("Start round " + currentRounds);
            SpawnPassport();
        }
    }

    public void TagTraveller()
    {
        if (numberOfTravellers > currentNumberOftraveller)
        {
            player.transform.position = GameObject.Find("Marker (Traveller)").transform.position;
            player.gameObject.tag = "Traveller";

            Debug.Log("Player choose traveller role!");
            currentNumberOftraveller++;

            SpawnPassport();

            ButtonMessage m = new ButtonMessage();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
            
    }
    
    public void TagSupervisor()
    {
        if (numberOfSupervisors > currentNumberOfsupervisor)
        {
            player.transform.position = GameObject.Find("Marker (Supervisor)").transform.position;
            player.gameObject.tag = "Supervisor";

            Debug.Log("Player choose supervisor role!");
            currentNumberOfsupervisor++;

            ButtonMessage m = new ButtonMessage();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
    }

    public void TagInspector()
    {
        if (numberOfInspectors > currentNumberOfinspector)
        {
            player.transform.position = GameObject.Find("Marker (Inspector)").transform.position;
            player.gameObject.tag = "Inspector";

            Debug.Log("Player choose inspector role!");
            currentNumberOfinspector++;

            ButtonMessage m = new ButtonMessage();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var ButtonMessage = m.FromJson<ButtonMessage>();

        currentNumberOftraveller = ButtonMessage.totalOftraveller;
        currentNumberOfinspector = ButtonMessage.totalOfinspector;
        currentNumberOfsupervisor = ButtonMessage.totalOfsupervisor;
    }
}
