using System.Collections;
using System.Collections.Generic;
using System;
using Ubiq.Messaging;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class GameSystem : MonoBehaviour
{
    NetworkContext context;

    GameObject[] players;
    GameObject simulator;

    [Header("Game Simulation")]
    [SerializeField] bool isInGame = false;
    private bool prevIsInGame = false;

    [Header("UI Buttons")]
    [SerializeField] Button travellerButton;
    [SerializeField] Button inspectorButton;
    [SerializeField] Button supervisorButton;

    [Header("Object Button")]
    [SerializeField] GameObject inspectorPassButton;
    [SerializeField] GameObject inspectorRejectButton;
    [SerializeField] GameObject supervisorPassButton;
    [SerializeField] GameObject supervisorRejectButton;

    [Header("Passport")]
    [SerializeField] GameObject Passport1;
    [SerializeField] GameObject Passport2;
    [SerializeField] GameObject Passport3;
    [SerializeField] GameObject Passport4;

    [Header("Settings")]
    [SerializeField] int numberOfRounds;

    [Header("Maximum number of each role")]
    [SerializeField] int numberOfTravellers;
    [SerializeField] int numberOfInspectors;
    [SerializeField] int numberOfSupervisors;

    int currentNumberOftraveller = 0;
    int currentNumberOfinspector = 0;
    int currentNumberOfsupervisor = 0;

    private void Awake()
    {
        travellerButton.onClick.AddListener(TagTraveller);
        inspectorButton.onClick.AddListener(TagInspector);
        supervisorButton.onClick.AddListener(TagSupervisor);
    }

    private void Start()
    {
        context = NetworkScene.Register(this);

//#if UNITY_EDITOR
//        simulator = GameObject.Find("XR Device Simulator");
//        simulator.SetActive(true);
//#endif
    }

    private void Update()
    {
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
    }

    private struct Message
    {
        public int totalOftraveller;
        public int totalOfinspector;
        public int totalOfsupervisor;
    }

    public void TagTraveller()
    {
        if (numberOfTravellers > currentNumberOftraveller)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            players[0].transform.position = GameObject.Find("Marker (Traveller)").transform.position;

            Debug.Log("Player choose traveller role!");
            currentNumberOftraveller++;

            // Randomly activate one of the four objects
            ActivateRandomObject();

            Message m = new Message();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
            
    }

    private void ActivateRandomObject()
    {
        // Place your GameObject references in an array
        GameObject[] passports = new GameObject[] { Passport1, Passport2, Passport3, Passport4 };

        // Generate a random index between 0 and 3 (since array indices are 0-based)
        int randomIndex = new System.Random().Next(0, passports.Length); // Random.Next is inclusive at the start, exclusive at the end

        // Select the GameObject using the random index
        GameObject selectedPassport = passports[randomIndex];

        // Activate the selected GameObject
        if (selectedPassport != null)
        {
            selectedPassport.SetActive(true);
        }
        else
        {
            Debug.LogError("Selected passport object not found!");
        }
    }

    public void TagSupervisor()
    {
        if (numberOfSupervisors > currentNumberOfsupervisor)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            players[0].transform.position = GameObject.Find("Marker (Supervisor)").transform.position;

            Debug.Log("Player choose supervisor role!");
            currentNumberOfsupervisor++;

            Message m = new Message();
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
            players = GameObject.FindGameObjectsWithTag("Player");
            players[0].transform.position = GameObject.Find("Marker (Inspector)").transform.position;

            Debug.Log("Player choose inspector role!");
            currentNumberOfinspector++;

            Message m = new Message();
            m.totalOftraveller = currentNumberOftraveller;
            m.totalOfsupervisor = currentNumberOfsupervisor;
            m.totalOfinspector = currentNumberOfinspector;
            context.SendJson(m);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage m)
    {
        var message = m.FromJson<Message>();

        currentNumberOftraveller = message.totalOftraveller;
        currentNumberOfinspector = message.totalOfinspector;
        currentNumberOfsupervisor = message.totalOfsupervisor;


    }
}
