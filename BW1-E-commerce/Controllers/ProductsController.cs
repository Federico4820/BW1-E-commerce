using BW1_E_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BW1_E_commerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly string _connectionString;

        public ProductsController()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //4 SELECT: Category, Color, Sizes e Materials

        private async Task<List<Category>> GetCategories()
        {
            List<Category> categories = new List<Category>();
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Category";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(
                                new Category()
                                {
                                    IdCategory = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                        }
                    }
                }
            }
            return categories;
        }

        private async Task<List<Color>> GetColor()
        {
            List<Color> colors = new List<Color>();
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Colors";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            colors.Add(
                                new Color()
                                {
                                    IdColor = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                        }
                    }
                }
            }
            return colors;
        }

        private async Task<List<Size>> GetSizes()
        {
            List<Size> sizes = new List<Size>();
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "  SELECT * FROM Sizes ORDER BY id_size";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sizes.Add(
                                new Size()
                                {
                                    IdSize = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                        }
                    }
                }
            }
            return sizes;
        }

        private async Task<List<Material>> GetMaterials()
        {
            List<Material> materials = new List<Material>();
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Materials";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            materials.Add(
                                new Material()
                                {
                                    IdMaterial = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                });
                        }
                    }
                }
            }
            return materials;
        }



        public async Task<IActionResult> Add()
        {
            ViewBag.Categories = await GetCategories();
            ViewBag.Color = await GetColor();
            ViewBag.Sizes = await GetSizes();
            ViewBag.Material = await GetMaterials();
            var model = new ProductAddModel();
            return View(model);
        }




        public IActionResult Index()
        {
            return View();
        }
    }
}
