using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    private GameObject furniture;

    [SerializeField] private ButtonManager buttonPrefab;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private List<Item> items;

    // Thêm trường để gắn script ECommerceAPI
    [SerializeField] private ECommerceAPI ecommerceAPI;

    private int currrent_id = 0;

    private static DataHandler instance;
    public static DataHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataHandler>();
            }
            return instance;
        }
    }

    private async void Start()
    {
        if (ecommerceAPI != null)
        {
            await ecommerceAPI.AuthenticateAndGetProducts();
            LoadItems();
            CreateButtons();
        }
        else
        {
            Debug.LogError("ECommerceAPI script is not assigned!");
        }
    }

    void LoadItems()
    {
        var item_obj = Resources.LoadAll("Items", typeof(Item));
        var productList = ecommerceAPI.productList;

        foreach (var item in item_obj)
        {
            Item currentItem = item as Item;

            if (currentItem != null && 
                (currentItem.name == "a1" || currentItem.name == "a2" || currentItem.name == "z" || currentItem.name == "z1" || 
                ProductExistsInList(currentItem.name, productList)))
            {
                items.Add(currentItem);
            }
        }
    }

    void CreateButtons()
    {
        foreach (Item i in items)
        {
            ButtonManager b = Instantiate(buttonPrefab, buttonContainer.transform);
            b.ItemId = currrent_id;
            b.ButtonTexture = i.itemImage;
            currrent_id++;
        }
    }

    public void SetFurniture(int id)
    {
        furniture = items[id].itemPrefab;
    }

    public GameObject GetFurniture()
    {
        return furniture;
    }

    private bool ProductExistsInList(string itemName, List<Product> productList)
    {
        foreach (var product in productList)
        {
            if (product.Name == itemName)
            {
                return true; // Found a match
            }
        }
        return false; // No match found
    }
}
