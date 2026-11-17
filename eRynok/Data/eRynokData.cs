using System.Text.Json;
using eRynok.Models;

namespace eRynok.Data;

public static class eRynokData
{
    private static string _productsFilePath = string.Empty;
    private static string _categoriesFilePath = string.Empty;
    private static string _usersFilePath = string.Empty;
    private static string _sellerProfilesFilePath = string.Empty;
    private static string _buyerProfilesFilePath = string.Empty;
    private static string _ordersFilePath = string.Empty;

    public static List<Product> Products { get; private set; } = new();
    public static List<Category> Categories { get; private set; } = new();
    public static List<User> Users { get; private set; } = new();
    public static List<SellerProfile> SellerProfiles { get; private set; } = new();
    public static List<BuyerProfile> BuyerProfiles { get; private set; } = new();
    public static List<Order> Orders { get; private set; } = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public static void Initialize(string contentRootPath)
    {
        string productsDirectory = Path.Combine(
            contentRootPath,
            "Data",
            "Products");
        string categoriesDirectory = Path.Combine(
            contentRootPath,
            "Data",
            "Categories");
        string usersDirectory = Path.Combine(
            contentRootPath, 
            "Data", 
            "Users");
        string ordersDirectory = Path.Combine(contentRootPath, 
            "Data", 
            "Orders");

        Directory.CreateDirectory(productsDirectory);
        Directory.CreateDirectory(categoriesDirectory);
        Directory.CreateDirectory(usersDirectory);
        Directory.CreateDirectory(ordersDirectory);

        _productsFilePath = Path.Combine(productsDirectory, "products.json");
        _categoriesFilePath = Path.Combine(categoriesDirectory, "categories.json");
        _usersFilePath = Path.Combine(usersDirectory, "users.json");
        _sellerProfilesFilePath = Path.Combine(usersDirectory, "sellerProfiles.json");
        _buyerProfilesFilePath = Path.Combine(usersDirectory, "buyerProfiles.json");
        _ordersFilePath = Path.Combine(ordersDirectory, "orders.json");

        LoadProducts();
        LoadCategories();
        LoadUsers();
        LoadSellerProfiles();
        LoadBuyerProfiles();
        LoadOrders();
    }

    private static void LoadProducts()
    {
        if (File.Exists(_productsFilePath))
        {
            string json = File.ReadAllText(_productsFilePath);

            Products = JsonSerializer.Deserialize<List<Product>>(json)
                ?? new List<Product>();
        }
        else
        {
            Products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Samsung Galaxy S25",
                    Description = "Smartphone with 256GB storage",
                    Price = 35999,
                    ImageUrl = "https://example.com/samsung-s25.jpg",
                    CategoryId = 1,
                    UserId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "Apple MacBook Air M4",
                    Description = "Laptop with 16GB RAM and 512GB SSD",
                    Price = 52999,
                    ImageUrl = "https://example.com/macbook-air.jpg",
                    CategoryId = 2,
                    UserId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

            SaveProducts();
        }
    }

    private static void LoadCategories()
    {
        if (File.Exists(_categoriesFilePath))
        {
            string json = File.ReadAllText(_categoriesFilePath);

            Categories = JsonSerializer.Deserialize<List<Category>>(json)
                ?? new List<Category>();
        }
        else
        {
            Categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Smartphones",
                    Description = "Mobile phones and smartphones"
                },
                new Category
                {
                    Id = 2,
                    Name = "Laptops",
                    Description = "Laptops and notebooks"
                },
                new Category
                {
                    Id = 3,
                    Name = "Home Appliances",
                    Description = "Appliances for home use"
                },
                new Category
                {
                    Id = 4,
                    Name = "Clothing",
                    Description = "Clothing, shoes and accessories"
                },
                new Category
                {
                    Id = 5,
                    Name = "Computer Components",
                    Description = "Computer hardware and components"
                }
            };

            SaveCategories();
        }
    }


    private static void LoadUsers()
    {
        if (File.Exists(_usersFilePath))
        {
            string json = File.ReadAllText(_usersFilePath);

            Users = JsonSerializer.Deserialize<List<User>>(json)
                ?? new List<User>();
        }
        else
        {
            Users = new List<User>
        {
            new User
            {
                Id = 1,
                Name = "TechStore",
                Email = "techstore@example.com",
                PasswordHash = string.Empty,
                Role = UserRole.Seller
            },
            new User
            {
                Id = 2,
                Name = "MobileWorld",
                Email = "mobileworld@example.com",
                PasswordHash = string.Empty,
                Role = UserRole.Seller
            }
        };

            SaveUsers();
        }
    }

    private static void LoadSellerProfiles()
    {
        if (File.Exists(_sellerProfilesFilePath))
        {
            string json = File.ReadAllText(_sellerProfilesFilePath);

            SellerProfiles = JsonSerializer.Deserialize<List<SellerProfile>>(json)
                ?? new List<SellerProfile>();
        }
        else
        {
            SellerProfiles = new List<SellerProfile>();
        }

        var sellers = Users.Where(u => u.Role == UserRole.Seller);

        foreach (var seller in sellers)
        {
            bool profileExists = SellerProfiles.Any(p => p.UserId == seller.Id);

            if (!profileExists)
            {
                SellerProfiles.Add(new SellerProfile
                {
                    UserId = seller.Id,
                    Phone = string.Empty,
                    Address = string.Empty
                });
            }
        }

        SaveSellerProfiles();
    }

    private static void LoadBuyerProfiles()
    {
        if (File.Exists(_buyerProfilesFilePath))
        {
            string json = File.ReadAllText(_buyerProfilesFilePath);

            BuyerProfiles = JsonSerializer.Deserialize<List<BuyerProfile>>(json)
                ?? new List<BuyerProfile>();
        }
        else
        {
            BuyerProfiles = new List<BuyerProfile>();
        }

        var buyers = Users.Where(u => u.Role == UserRole.Buyer);

        foreach (var buyer in buyers)
        {
            bool profileExists = BuyerProfiles.Any(p => p.UserId == buyer.Id);

            if (!profileExists)
            {
                BuyerProfiles.Add(new BuyerProfile
                {
                    UserId = buyer.Id,
                    Phone = string.Empty,
                    Address = string.Empty
                });
            }
        }

        SaveBuyerProfiles();
    }

    private static void LoadOrders()
    {
        if (File.Exists(_ordersFilePath))
        {
            string json = File.ReadAllText(_ordersFilePath);

            Orders = JsonSerializer.Deserialize<List<Order>>(json)
                ?? new List<Order>();
        }
        else
        {
            Orders = new List<Order>();
            SaveOrders();
        }
    }

    public static void SaveProducts()
    {
        string json = JsonSerializer.Serialize(
            Products,
            _jsonOptions);

        string temporaryPath = _productsFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, _productsFilePath, true);
    }

    public static void SaveCategories()
    {
        string json = JsonSerializer.Serialize(
            Categories,
            _jsonOptions);

        string temporaryPath = _categoriesFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, _categoriesFilePath, true);
    }

    public static void SaveUsers()
    {
        string json = JsonSerializer.Serialize(
            Users,
            _jsonOptions);

        string temporaryPath = _usersFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, _usersFilePath, true);
    }

    public static void SaveSellerProfiles()
    {
        string json = JsonSerializer.Serialize(
            SellerProfiles,
            _jsonOptions);

        string temporaryPath = _sellerProfilesFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, _sellerProfilesFilePath, true);
    }

    public static void SaveBuyerProfiles()
    {
        string json = JsonSerializer.Serialize(
            BuyerProfiles,
            _jsonOptions);

        string temporaryPath = _buyerProfilesFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);

        File.Move(temporaryPath, _buyerProfilesFilePath, true);
    }

    public static void SaveOrders()
    {
        string json = JsonSerializer.Serialize(Orders, _jsonOptions);

        string temporaryPath = _ordersFilePath + ".tmp";

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _ordersFilePath, true);
    }
}
