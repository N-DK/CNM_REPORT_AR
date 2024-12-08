using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class ECommerceAPI : MonoBehaviour
{
    private const string OdooUrl = "https://testar2.odoo.com/web/dataset/call_kw";
    private const string OdooLoginUrl = "https://testar2.odoo.com/web/session/authenticate";

    private const string Username = "admin@gmail.com";
    private const string Password = "admin";
    private const string DatabaseName = "testar2";

    private string sessionId; // Lưu trữ session ID
    private static readonly HttpClient client = new HttpClient(); // Dùng HttpClient toàn cục

    public List<Product> productList; // Danh sách công khai để lưu danh sách sản phẩm

    async void Start()
    {
        // Đăng nhập và lấy danh sách sản phẩm khi bắt đầu
        await AuthenticateAndGetProducts();
    }

    // Đăng nhập và lấy danh sách sản phẩm
    public async Task AuthenticateAndGetProducts()
    {
        sessionId = await Authenticate();
        if (!string.IsNullOrEmpty(sessionId))
        {
            Debug.Log("Authentication successful. Session ID: " + sessionId);
            await GetProductList();
        }
        else
        {
            Debug.LogError("Authentication failed. Unable to fetch product list.");
        }
    }

    // Hàm đăng nhập
    private async Task<string> Authenticate()
    {
        var loginData = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new
            {
                db = DatabaseName,
                login = Username,
                password = Password
            },
            id = (string)null
        };

        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(loginData);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(OdooLoginUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Lấy session ID từ header Set-Cookie
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var cookie in cookies)
                    {
                        if (cookie.Contains("session_id"))
                        {
                            string sessionId = cookie.Split(';')[0].Split('=')[1];
                            return sessionId;
                        }
                    }
                }
                Debug.LogError("No session ID found in response headers.");
            }
            else
            {
                Debug.LogError("Login failed: " + response.StatusCode);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception during login: " + e.Message);
        }

        return null;
    }

    // Lấy danh sách sản phẩm
    private async Task GetProductList()
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogError("Session ID is null or empty. Cannot fetch product list.");
            return;
        }

        var requestData = new
        {
            jsonrpc = "2.0",
            method = "call",
            @params = new
            {
                model = "product.template",
                method = "search_read",
                args = new object[]
                {
                    new object[]
                    {
                        new object[] { "sale_ok", "=", true },
                        new object[] { "website_published", "=", true }
                    }
                },
                kwargs = new
                {
                    fields = new[] { "name", "list_price", "default_code", "website_url" }
                }
            },
            id = 1
        };

        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(OdooUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log("Product List Response: " + responseBody);
                ProcessResponse(responseBody);
            }
            else
            {
                Debug.LogError("Error fetching product list: " + response.StatusCode);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception occurred while fetching product list: " + e.Message);
        }
    }

    // Xử lý phản hồi danh sách sản phẩm
    private void ProcessResponse(string responseBody)
    {
        try
        {
            var jsonResponse = JObject.Parse(responseBody);
            var products = jsonResponse["result"];

            if (products != null && products.HasValues) // Kiểm tra nếu có sản phẩm
            {
                productList = new List<Product>(); // Khởi tạo danh sách

                foreach (var product in products.Children()) // Sử dụng Children() để lặp qua các sản phẩm
                {
                    string name = product["name"].ToString();
                    string price = product["list_price"].ToString();
                    string code = product["default_code"].ToString();
                    string url = product["website_url"].ToString();

                    // Tạo đối tượng Product và thêm vào danh sách
                    productList.Add(new Product
                    {
                        Name = name,
                        Price = price,
                        Code = code,
                        Url = url
                    });

                }
            }
            else
            {
                Debug.Log("No products found or response is invalid.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception during response processing: " + e.Message);
        }
    }
}

public class Product
{
    public string Name { get; set; }
    public string Price { get; set; }
    public string Code { get; set; }
    public string Url { get; set; }
}
