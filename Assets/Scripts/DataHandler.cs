using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DataHandler : MonoBehaviour
{
    private GameObject furniture;

    [SerializeField] private ButtonManager buttonPrefab;
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private List<Item> items;

    // Thêm trường để gắn script ECommerceAPI
    [SerializeField] private ECommerceAPI ecommerceAPI;
    private string serverUrl = "https://testar2.odoo.com/";

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

    [SerializeField] private Button buyNowButton;
    [SerializeField] private TextMeshProUGUI priceText;

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

        // Tạo một HashSet để lưu trữ tên sản phẩm đã tồn tại
        HashSet<string> existingProductNames = new HashSet<string>(productList.Select(p => p.Name));

        foreach (var item in item_obj)
        {
            Item currentItem = item as Item;

            if (currentItem != null &&
                (currentItem.name == "a 1" || currentItem.name == "a 2" || currentItem.name == "z" || currentItem.name == "z 1" ||
                existingProductNames.Contains(currentItem.name)))
            {
                Product matchingProduct = productList.Find(p => p.Name == currentItem.name);
                if (matchingProduct != null)
                {
                    if (float.TryParse(matchingProduct.Price.ToString(), out float price))
                    {
                        currentItem.price = price;
                        currentItem.url = serverUrl + matchingProduct.Url;
                    }
                    else
                    {
                        Debug.LogError($"Failed to convert price for item: {currentItem.name}");
                    }
                }
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
        priceText.text = items[id].price.ToString("C");
        buyNowButton.onClick.RemoveAllListeners();

        // Check if the item name is one of the specified names
        if (items[id].name == "a 1" || items[id].name == "a 2" || items[id].name == "z" || items[id].name == "z 1")
        {
            // Do not add the listener for these items
            Debug.Log($"Button disabled for item: {items[id].name}");
        }
        else
        {
            buyNowButton.onClick.AddListener(() => Application.OpenURL(items[id].url));
        }
    }

    public GameObject GetFurniture()
    {
        return furniture;
    }
}
