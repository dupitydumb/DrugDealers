using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using System.Threading.Tasks;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    // Start is called before the first frame update
    [Header("Game Objects")]
    private GameObject drugTextPrefab;
    [SerializeField]
    public List<Drug> drugs = new List<Drug>();
    public Market market = new Market();
    public Player player = new Player(1000);

    public Action onPriceChange;
    public Action OnInventoryChange;
    public Action OnMarketChange;
    public Action OnFundsChange;

    void Awake()
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
        drugTextPrefab = Resources.Load<GameObject>("Prefabs/DrugText");
        GenerateDrugPresetList();
        GenerateDrug(10);
        StartCoroutine(RandomizeDrugPrices());
        GeneratePlayerInventory();
        player.Start();
        player.UpdateFundsText();
        OnInventoryChange += UpdatePlayerInventory;
        OnFundsChange += player.UpdateFundsText;
        StartCoroutine(GenerateCustomerRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateDrugPresetList()
    {
        // Info feilds: Rarity, Name, PurchasePrice, SellPrice, Quality, Quantity
        drugs.Add(new Drug(0.1f, "Aspirin", 10, 15, 1, 10));
        drugs.Add(new Drug(0.2f, "Ibuprofen", 20, 25, 2, 10));
        drugs.Add(new Drug(0.3f, "Paracetamol", 30, 35, 3, 10));
        drugs.Add(new Drug(0.4f, "Codeine", 40, 45, 4, 10));
        drugs.Add(new Drug(0.5f, "Morphine", 50, 55, 5, 10));
        drugs.Add(new Drug(0.6f, "Oxycodone", 60, 65, 6, 10));
        drugs.Add(new Drug(0.7f, "Hydrocodone", 70, 75, 7, 10));
        drugs.Add(new Drug(0.8f, "Fentanyl", 80, 85, 8, 10));
        drugs.Add(new Drug(0.9f, "Heroin", 90, 95, 9, 10));
        drugs.Add(new Drug(1f, "Carfentanil", 100, 105, 10, 10));

    }
    void GenerateDrug(int numberOfDrugs)
    {
        //Based on drug preset list, generate a random drug that available in the market, account for rarity, randomize the quantity and quality
        for (int i = 0; i < numberOfDrugs; i++)
        {
            int randomIndex = Random.Range(0, drugs.Count);
            Drug randomDrug = drugs[randomIndex];
            float randomRarity = Random.Range(0, 1);
            float randomQuality = Random.Range(1, 10);
            int randomQuantity = Random.Range(1, 15);
            //if the drug is with same name and quality, increase the quantity
            if (market.AvailableDrugs.Exists(d => d.Name == randomDrug.Name && d.Quality == randomQuality))
            {
                market.AvailableDrugs.Find(d => d.Name == randomDrug.Name && d.Quality == randomQuality).Quantity++;
                continue;
            }
            
            market.AvailableDrugs.Add(new Drug(randomRarity, randomDrug.Name, randomDrug.PurchasePrice, randomDrug.SellPrice, (int)randomQuality, randomQuantity));
        }
        SortTheMarket();
        GenerateMarket();
    }

    //Randomize Price at the market every 10 seconds

    void GenerateMarket()
    {
        foreach (Drug drug in market.AvailableDrugs)
        {
            GameObject drugText = Instantiate(drugTextPrefab, transform);
            drugText.GetComponent<DrugText>().SetDrug(drug);
            drugText.GetComponent<DrugText>().textType = TextType.OnMarket;
        }
    }

    IEnumerator GenerateCustomerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10, 20));
            GenerateCustomer();
        }
    }

    void GenerateCustomer()
    {
        //Generate a customer with random max budget and preferred quality
        float randomMaxBudget = Random.Range(50, 300);
        int randomPreferredQuality = Random.Range(1, 5);
        // List of funny and dynamic log templates
        List<string> logTemplates = new List<string>
        {
            "A customer strolls in with ${0} jingling in their pocket, eyeing quality {1} goods.",
            "Someone with ${0} to burn is looking for the good stuff. Quality {1}, or bust!",
            "Here comes a high roller! Wants nothing but quality {1} and has ${0} to prove it.",
            "With ${0} in their wallet, this customer won’t settle for less than quality {1}.",
            "A wild customer appears! They have ${0} and a craving for quality {1} products."
        };
        // Select a random template
        string selectedTemplate = logTemplates[Random.Range(0, logTemplates.Count)];

        // Format the selected template with the actual values
        string logMessage = string.Format(selectedTemplate, randomMaxBudget, randomPreferredQuality);

        // Log the dynamic and funny message
        EventGenerator.Instance.log.LogEvent(logMessage, EventGenerator.Instance.textPrefab);

        // Generate the customer object (assuming this part remains unchanged)
        Customer customer = new Customer(randomMaxBudget, randomPreferredQuality);
    }

    void SortTheMarket()
    {
        //Sort the market based on name then rarity
        market.AvailableDrugs.Sort((x, y) => x.Name.CompareTo(y.Name));
        market.AvailableDrugs.Sort((x, y) => x.Rarity.CompareTo(y.Rarity));
    }

    void GeneratePlayerInventory()
    {
        //Generate the player's inventory
        foreach (Drug drug in player.Inventory)
        {
            GameObject drugText = Instantiate(drugTextPrefab, transform);
            drugText.GetComponent<DrugText>().textType = TextType.InInventory;
            drugText.GetComponent<DrugText>().SetDrug(drug);

        }
    }

    //Update Player Inventory
    public void UpdatePlayerInventory()
    {
        Debug.Log("Updating Player Inventory");
        Transform inventoryParent = GameObject.Find("InventoryParent").transform;
        if (inventoryParent.childCount > 0)
        {
            foreach (Transform child in inventoryParent)
            {
                Destroy(child.gameObject);
            }
        }
        GeneratePlayerInventory();
    }

    IEnumerator RandomizeDrugPrices()
    {
        while (true) // Creates an infinite loop
        {
            yield return new WaitForSeconds(20); // Wait for 20 seconds

            foreach (Drug drug in market.AvailableDrugs)
            {
                // Randomize the purchase and sell prices, take the quality into account. the higher the quality, the higher the price and vice versa
                float randomPurchasePrice = drug.PurchasePrice + Random.Range(-10, 10) + drug.Quality * 2;
                float randomSellPrice = drug.SellPrice + Random.Range(-10, 10) + drug.Quality * 2.5f;
                drug.PurchasePrice = Mathf.Max(0, randomPurchasePrice); // Ensure the price is not negative
                drug.SellPrice = Mathf.Max(0, randomSellPrice); // Ensure the price is not negative
            }

            // Optionally, refresh the market display or notify the player of price changes here
            onPriceChange?.Invoke();
        }
    }
    public Drug selectedDrug;
    public void SetSelectedDrug(Drug drug)
    {
        selectedDrug = drug;
    }
    public void SetUnselectedDrug()
    {
        selectedDrug = null;
    }

    public void BuyDrug()
    {
        if (selectedDrug != null)
        {
            market.BuyDrug(selectedDrug, player);
        }
        OnInventoryChange?.Invoke();
        OnMarketChange?.Invoke();
        OnFundsChange?.Invoke();
    }

    public void SellDrug()
    {
        if (selectedDrug != null)
        {
            market.SellDrug(selectedDrug, player);
        }
        OnInventoryChange?.Invoke();
        OnMarketChange?.Invoke();
        OnFundsChange?.Invoke();
    }
}
[System.Serializable]
public class Drug
{
    public float Rarity;
    public string Name;
    public float PurchasePrice;
    public float SellPrice;
    public int Quality;
    public int Quantity;
    [SerializeField]
    public Drug(float rarity, string name, float purchasePrice, float sellPrice, int quality,int quantity)
    {
        Rarity = rarity;
        Name = name;
        PurchasePrice = purchasePrice;
        SellPrice = sellPrice;
        Quality = quality;
        Quantity = quantity;

        Initialize();
    }

    private void Initialize()
    {
        // Initialization code here
        AdjustPrice(); // For example, adjusting the price as part of initialization
    }

    void AdjustPrice()
    {
        //Randomize the purchase and sell prices, take the quality into account. the higher the quality, the higher the price and vice versa
        PurchasePrice += Random.Range(-10, 10) + Quality * 0.5f;
        SellPrice += Random.Range(-10, 10) + Quality * 0.8f;
    }
}

[System.Serializable]
public class Market
{
    public List<Drug> AvailableDrugs { get; set; } = new List<Drug>();

    public void BuyDrug(Drug drug, Player player)
    {
        if (player.Funds >= drug.PurchasePrice && AvailableDrugs.Contains(drug) && drug.Quantity > 0)
        {
            player.Funds -= drug.PurchasePrice;
            //if the player already has the drug same drug name and same quality, increase the quantity
            if (player.Inventory.Exists(d => d.Name == drug.Name && d.Quality == drug.Quality))
            {
                player.Inventory.Find(d => d.Name == drug.Name && d.Quality == drug.Quality).Quantity++;
            }
            else
            {
                Debug.Log("Player does not have the drug");
                player.Inventory.Add(new Drug(drug.Rarity, drug.Name, drug.PurchasePrice, drug.SellPrice, drug.Quality, 1));
            }

            //Decrease the quantity of the drug in the market
            AvailableDrugs.Find(d => d.Name == drug.Name).Quantity--;
            if (AvailableDrugs.Find(d => d.Name == drug.Name).Quantity == 0)
            {
                AvailableDrugs.Remove(drug);
            }
            Manager.Instance.OnMarketChange?.Invoke();


        }

        Manager.Instance.OnFundsChange?.Invoke();
        
    }

    public void SellDrug(Drug drug, Player player)
    {
        if (player.Inventory.Contains(drug) && drug.Quantity > 0)
        {
            player.Funds += drug.SellPrice;
            player.Inventory.Find(d => d.Name == drug.Name).Quantity--;
            if (player.Inventory.Find(d => d.Name == drug.Name).Quantity == 0)
            {
                player.Inventory.Remove(drug);
            }
        }

        Manager.Instance.OnFundsChange?.Invoke();
    }

}

[System.Serializable]
public class Player
{
    // TODO: Add a property to store the player's funds. make it one!
    public float Funds { get; set; }
    public List<Drug> Inventory { get; set; } = new List<Drug>();

    public PlayerStatus playerStatus;

    public TMPro.TMP_Text fundsText;
    public void Start()
    {
        fundsText = GameObject.Find("FundsText").GetComponent<TMPro.TMP_Text>();
        Manager.Instance.onPriceChange += UpdateFundsText;

    }
    public Player(float funds)
    {
        Funds = funds;
    }

    public void UpdateFundsText()
    {
        fundsText.text = $"Funds: ${Funds.ToString("F2")}";
    }
}

public class Customer
{
    public float maxBudget;
    public int preferredQuality;
    public float preferredRarity = 0.1f; // Default value for preferredRarity

    public Customer(float maxBudget, int preferredQuality)
    {
        this.maxBudget = maxBudget;
        this.preferredQuality = preferredQuality;
        Debug.Log("Customer created");
        
        SearchForDrugsAsync();
    }

    public async Task SearchForDrugsAsync()
    {
        Debug.Log("Searching for drugs..."); // Log the start of the search
        await Task.Delay(500); // Delay for 1 second to simulate asynchronous operation

        List<Drug> marketDrugs = Manager.Instance.market.AvailableDrugs;
        List<Drug> inventoryDrugs = Manager.Instance.player.Inventory;

        List<Drug> selectedDrugs = marketDrugs.Where(d => d.Quality == preferredQuality && d.Rarity <= preferredRarity && d.SellPrice <= maxBudget).ToList();


        //Bought message
        List<string> boughtMessages = new List<string>
        {
            //Generate 20 different messages with different humor
            "Customer bought {0} for ${1}.",
            "She runs away with {0} in her hands, leaving ${1} behind.",
            "He hands over ${1} and takes {0} with a smile.",
            "He hand you ${1} and takes {0} with a grin.",
            "She hands you phone worth ${1} and takes {0}",
            "He hands you a watch worth ${1} and takes {0}",
            "Police arrives and takes {0} and leaves ${1} behind.",
            "Hoddie man takes {0} and leaves ${1} behind.",
            //Design more messages with different humor. create a imaginative story of the customer
            "Weird DND character takes {0} and leaves ${1} behind.",
            "A customer with a parrot on his shoulder takes {0} and leaves ${1} behind.",
            "A customer with a cat in his bag takes {0} and leaves ${1} behind.",
            "A weebo customer takes {0} and leaves ${1} behind.",
        };

        //Not bought message
        List<string> notBoughtMessages = new List<string>
        {
            //Generate 20 different messages with different humor
            "Customer did not find {0}.",
            "She leaves without buying {0}.",
            "He leaves without buying {0}.",
            "With a sad face, she leaves without buying {0}.",
            "Really? He leaves without buying {0}.",
        };

        //too expensive message
        List<string> tooExpensiveMessages = new List<string>
        {
            //Generate 20 different messages with different humor
            "Customer thinks {0} is too expensive.",
            "She thinks {0} is too expensive.",
            "He thinks {0} is too expensive.",
            "With a sad face, she thinks {0} is too expensive.",
            "Really? He thinks {0} is too expensive.",
        };

        if (selectedDrugs.Count > 0)
        {
            Drug selectedDrug = selectedDrugs[Random.Range(0, selectedDrugs.Count)];

            // Log event
            EventGenerator.Instance.log.LogEvent($"Customer looking for {selectedDrug.Name} with quality {selectedDrug.Quality} and price ${selectedDrug.SellPrice}.", EventGenerator.Instance.textPrefab);
            await Task.Delay(500); // Delay for 0.5 seconds before checking inventory

            // if player has the drug that the customer is looking for or better quality, sell the drug to the customer
            if (inventoryDrugs.Exists(d => d.Name == selectedDrug.Name && d.Quality >= selectedDrug.Quality))
            {
                //check if the price is less than the max budget
                if (selectedDrug.SellPrice <= maxBudget * 0.6)
                {
                    Manager.Instance.market.SellDrug(selectedDrug, Manager.Instance.player);
                    EventGenerator.Instance.log.LogEvent(string.Format(boughtMessages[Random.Range(0, boughtMessages.Count)], selectedDrug.Name, selectedDrug.SellPrice), EventGenerator.Instance.textPrefab);
                }
                else
                {
                    EventGenerator.Instance.log.LogEvent(string.Format(tooExpensiveMessages[Random.Range(0, tooExpensiveMessages.Count)], selectedDrug.Name), EventGenerator.Instance.textPrefab);
                }
            }
            else
            {
                EventGenerator.Instance.log.LogEvent(string.Format(notBoughtMessages[Random.Range(0, notBoughtMessages.Count)], selectedDrug.Name), EventGenerator.Instance.textPrefab);
            }

            List<string> positiveReactions = new List<string>
            {
                "Customer is happy with the purchase.",
                "Customer is satisfied with the transaction.",
                "Customer leaves with a smile on their face.",
                //Generate more positive reactions with more humor
                "Customer is ecstatic about the deal.",
                "Customer is over the moon with the purchase.",
                "Customer is thrilled with the transaction."
            };

            List<string> negativeReactions = new List<string>
            {
                "Customer is not happy with the purchase.",
                "Customer is dissatisfied with the transaction.",
                "Customer leaves with a frown on their face.",
                "Customer is disappointed with the deal.",
                "Customer is upset about the purchase.",
                "Customer is angry with the transaction."
            };
        }
    }
}