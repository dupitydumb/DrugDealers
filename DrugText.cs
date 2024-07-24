using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TextType
{
    OnMarket,
    InInventory
}
public class DrugText : MonoBehaviour
{
    public TextType textType;
    private Drug assignedDrug;
    private TMP_Text drugNameText;
    private TMP_InputField purchasePriceText;
    private TMP_Text inputText;
    private TMP_Text qualityText;
    private TMP_Text quantityText;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        drugNameText = transform.Find("name").GetComponent<TMP_Text>();
        purchasePriceText = transform.Find("price").GetComponent<TMP_InputField>();
        qualityText = transform.Find("quality").GetComponent<TMP_Text>();
        quantityText = transform.Find("quantity").GetComponent<TMP_Text>();
        button = GetComponent<Button>();
        if (textType == TextType.OnMarket)
        {
            transform.SetParent(GameObject.Find("MarketParent").transform);
        }
        else if (textType == TextType.InInventory)
        {
            transform.SetParent(GameObject.Find("InventoryParent").transform);
        }

        button.onClick.AddListener(() => Manager.Instance.selectedDrug = assignedDrug);

        Manager.Instance.OnMarketChange += UpdateDrug;
        Manager.Instance.onPriceChange += UpdateDrug;
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == button.gameObject)
        {
            Manager.Instance.selectedDrug = assignedDrug;
        }

        // If assigned drug quantity is 0, remove the text object
        if (assignedDrug.Quantity == 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDrug(Drug drug)
    {
        assignedDrug = drug;
        if (drugNameText == null)
        {
            Start();
            SetDrug(drug);
        }
        drugNameText.text = drug.Name;
        if (textType == TextType.OnMarket)
        {
            //Find InputField child text and set it to the purchase price
            purchasePriceText.text = "$" + drug.PurchasePrice.ToString("F2");
            purchasePriceText.interactable = false;
        }
        else if (textType == TextType.InInventory)
        {
            purchasePriceText.text = "$" + drug.SellPrice.ToString("F2");
            purchasePriceText.interactable = true;
            purchasePriceText.onValueChanged.AddListener((string value) => UpdateDrugSellPrice());
        }
        qualityText.text = drug.Quality + "/10";
        quantityText.text = drug.Quantity + "g";
    }

    void UpdateDrugSellPrice()
    {
        Debug.Log("Updating sell price");
        // Remove the '$' sign and parse the new price
        if (float.TryParse(purchasePriceText.text.Replace("$", ""), out float newPrice))
        {
            //Serach the drug in the player's inventory and update the sell price
            Manager.Instance.player.Inventory.Find(d => d.Name == assignedDrug.Name).SellPrice = newPrice;
            
        }
        UpdateDrug();
    }
    public void UpdateDrug()
    {
        Debug.Log("Updating drug");
        if (textType == TextType.OnMarket)
        {
            purchasePriceText.text = "$" + assignedDrug.PurchasePrice.ToString("F2");
            purchasePriceText.interactable = false;
        }
        else if (textType == TextType.InInventory)
        {
            purchasePriceText.text = "$" + assignedDrug.SellPrice.ToString("F2");
            purchasePriceText.interactable = true;
        }
        qualityText.text = assignedDrug.Quality + "/10";
        quantityText.text = assignedDrug.Quantity + "g";
    }
}
