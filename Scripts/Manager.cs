using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using System.Threading.Tasks;


[Serializable]
public class MessageWrapper
{
    public string[] messages;
}
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
        logTemplates = LoadMessagesFromFile("logTemplates.json");
    }

    public List<string> LoadMessagesFromFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Messages/{fileName.Replace(".json", "")}");
        if (textAsset != null)
        {
            // Deserialize the JSON content into the MessageWrapper class
            MessageWrapper messageWrapper = JsonUtility.FromJson<MessageWrapper>(textAsset.text);
            if (messageWrapper != null && messageWrapper.messages != null)
            {
                // Convert the array to a list and return
                return new List<string>(messageWrapper.messages);
            }
            else
            {
                Debug.LogError("Failed to deserialize " + fileName);
                return new List<string>();
            }
        }
        else
        {
            Debug.LogError("Failed to load " + fileName);
            return new List<string>();
        }
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
        // add more
        drugs.Add(new Drug(0.1f, "Cocaine", 10, 15, 1, 10));
        drugs.Add(new Drug(0.2f, "LSD", 20, 25, 2, 10));
        drugs.Add(new Drug(0.3f, "MDMA", 30, 35, 3, 10));
        drugs.Add(new Drug(0.4f, "Methamphetamine", 40, 45, 4, 10));
        

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
            yield return new WaitForSeconds(Random.Range(5, 15));
            GenerateCustomer();
        }
    }

    private List<string> logTemplates = new List<string>();
    void GenerateCustomer()
    {
        //Generate a customer with random max budget and preferred quality
        float randomMaxBudget = Random.Range(50, 300);
        int randomPreferredQuality = Random.Range(1, 5);
        // List of funny and dynamic log templates
        // Select a random template
        string selectedTemplate = logTemplates[Random.Range(0, logTemplates.Count)];

        // Format the selected template with the actual values
        string logMessage = string.Format(selectedTemplate, randomMaxBudget, randomPreferredQuality);

        // Log the dynamic and funny message
        EventGenerator.Instance.log.LogEvent(logMessage, "#FFFFFF", EventGenerator.Instance.textPrefab);

        //randomize the customer' preferred rarity
        float randomPreferredRarity = Random.Range(0f, 1f);
        // Generate the customer object (assuming this part remains unchanged)
        Customer customer = new Customer(randomMaxBudget, randomPreferredQuality, randomPreferredRarity);
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
        SellPrice = PurchasePrice * 1.25f + Quality * 0.2f;
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
        Manager.Instance.OnMarketChange?.Invoke();
        Manager.Instance.OnFundsChange?.Invoke();
        
    }

    public void SellDrug(Drug drug, Player player)
    {
        if (player.Inventory.Contains(drug) && drug.Quantity > 0)
        {
            Debug.Log("Selling : " + drug.Name);
            player.Funds += drug.SellPrice;
            player.Inventory.Find(d => d.Name == drug.Name).Quantity--;
            if (player.Inventory.Find(d => d.Name == drug.Name).Quantity == 0)
            {
                player.Inventory.Remove(drug);
            }
        }
        Manager.Instance.OnMarketChange?.Invoke();
        Manager.Instance.OnFundsChange?.Invoke();
    }

    public void CustomerBuy(Drug drug, Player player)
    {

        int indexInInventory = player.Inventory.FindIndex(d => d.Name == drug.Name && d.Quality >= drug.Quality);
        if (player.Inventory.Contains(drug) && drug.Quantity > 0)
        {
            player.Funds += drug.SellPrice;
            player.Inventory.Find(d => d.Name == drug.Name).Quantity--;
            if (player.Inventory.Find(d => d.Name == drug.Name).Quantity == 0)
            {
                player.Inventory.Remove(drug);
            }
        }
        //if customer buys the drug, decrease the quantity of the drug in the inventory
        //If specific drug is not available in the inventory, searchf for the same name and better quality and sell it to the customer
        else if (player.Inventory.Exists(d => d.Name == drug.Name && d.Quality >= drug.Quality))
        {
            player.Funds += player.Inventory.Find(d => d.Name == drug.Name && d.Quality >= drug.Quality).SellPrice;
            player.Inventory.Find(d => d.Name == drug.Name && d.Quality >= drug.Quality).Quantity--;
            if (player.Inventory.Find(d => d.Name == drug.Name && d.Quality >= drug.Quality).Quantity == 0)
            {
                player.Inventory.Remove(drug);
            }
        }
        Manager.Instance.OnInventoryChange?.Invoke();
        Manager.Instance.OnMarketChange?.Invoke();
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
        Manager.Instance.OnInventoryChange += CleanInventory;

    }
    public Player(float funds)
    {
        Funds = funds;
    }

    public void CleanInventory()
    {
        //if the quantity of the drug is 0, remove the drug from the inventory
        Inventory.RemoveAll(d => d.Quantity <= 0);
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
    public float preferredRarity;

    public List<string> boughtMessages = new List<string>();
    public List<string> notBoughtMessages = new List<string>();
    public List<string> tooExpensiveMessages = new List<string>();
    
    private Drug selectedDrug;
    List<Drug> marketDrugs = Manager.Instance.market.AvailableDrugs;
    public Customer(float maxBudget, int preferredQuality, float preferredRarity = 0.5f)
    {
        this.maxBudget = maxBudget;
        this.preferredQuality = preferredQuality;
        
        // Calculate weights based on rarity difference
        var weights = marketDrugs.Select(d => 1 / (1 + Mathf.Abs(d.Rarity - preferredRarity))).ToList();
        float totalWeight = weights.Sum();

        // Normalize weights
        var normalizedWeights = weights.Select(w => w / totalWeight).ToList();
        // Select drugs based on normalized weights
        List<Drug> selectedDrugs = new List<Drug>();
        foreach (var drug in marketDrugs)
        {
            int index = marketDrugs.IndexOf(drug);
            float randomValue = Random.value; // Get a random value between 0 and 1
            if (randomValue < normalizedWeights[index] && drug.SellPrice <= maxBudget)
            {
                selectedDrugs.Add(drug);
                Debug.Log("adding + " + drug.Name);
            }
        }

        // Select a random drug from the selected drugs
        if (selectedDrugs.Count > 0)
        {
            selectedDrug = selectedDrugs[Random.Range(0, selectedDrugs.Count)];
        }
        else
        {
            //Search through the player's inventory and find cheapest drug compared to the market
            selectedDrug = marketDrugs.Where(d => d.SellPrice <= maxBudget).OrderBy(d => d.SellPrice).FirstOrDefault();
        }
        LoadMessages();
        Debug.Log("Customer created");
        SearchForDrugsAsync();
    }

    void LoadMessages()
    {
        boughtMessages = Manager.Instance.LoadMessagesFromFile("boughtMessages.json");
        notBoughtMessages = Manager.Instance.LoadMessagesFromFile("notBoughtMessages.json");
        tooExpensiveMessages = Manager.Instance.LoadMessagesFromFile("tooExpensiveMessages.json");

    }

    public async Task SearchForDrugsAsync()
    {
        List<Drug> inventoryDrugs = Manager.Instance.player.Inventory;
        List<Drug> selectedDrugs = marketDrugs.Where(d => d.Quality == preferredQuality && d.Rarity <= preferredRarity && d.SellPrice <= maxBudget).ToList();
        if (selectedDrugs.Count > 0)
        {
            // if player has the drug that the customer is looking for or better quality, sell the drug to the customer
            if (inventoryDrugs.Exists(d => d.Name == selectedDrug.Name && d.Quality >= selectedDrug.Quality && d.Quantity > 0))
            {
                //check if the price in player's inventory is less than the max budget
                if (inventoryDrugs.Find(d => d.Name == selectedDrug.Name && d.Quality >= selectedDrug.Quality).SellPrice <= maxBudget)
                {
                    EventGenerator.Instance.log.LogEvent(string.Format(boughtMessages[Random.Range(0, boughtMessages.Count)], selectedDrug.Name, selectedDrug.SellPrice), "#99FF42", EventGenerator.Instance.textPrefab);
                    Manager.Instance.market.CustomerBuy(selectedDrug, Manager.Instance.player);
                }
                else
                {
                    //make the text red
                    EventGenerator.Instance.log.LogEvent(string.Format(tooExpensiveMessages[Random.Range(0, tooExpensiveMessages.Count)], selectedDrug.Name), "red", EventGenerator.Instance.textPrefab);
                }
            }
            else
            {
                EventGenerator.Instance.log.LogEvent(string.Format(notBoughtMessages[Random.Range(0, notBoughtMessages.Count)], selectedDrug.Name), "#FFFFFF", EventGenerator.Instance.textPrefab);
            }
        }
    }
}