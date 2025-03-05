using System.Data.Common;
using System.Drawing;
using System.Reflection;
using System.Transactions;
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

        public async Task<ProductListModel> GetProducts()
        {
            var prodList = new ProductListModel()
            {
                ProductList = new List<ProductBaseModel>()
            };

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @" WITH ImageSelection AS (SELECT PC.id_prod, PCI.img_URL, ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn FROM ProdColorImages PCI JOIN ProdColor PC ON PCI.id_prodColor = PC.id_prodColor) SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, P.id_category, C.nome as category_name, P.gender, ISel.img_URL FROM Products P LEFT JOIN Category as C ON P.id_category = C.id_category LEFT JOIN ImageSelection ISel ON P.id_prod = ISel.id_prod AND ISel.rn = 1;";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            prodList.ProductList.Add(
                                new ProductBaseModel()
                                {
                                    IdProd = reader.GetGuid(0),
                                    Name = reader["nome"].ToString(),
                                    Brand = reader["brand"].ToString(),
                                    Price = decimal.Parse(reader["price"].ToString()),
                                    Description = reader["descr"].ToString().Split('/').Select(s => s.Trim()).ToList(),
                                    IdCategory = int.Parse(reader["id_category"].ToString()),
                                    NameCategory= reader["category_name"].ToString(),
                                    Gender = reader["gender"].ToString(),
                                    ImgURL = reader["img_URL"].ToString()
                                }
                            );
                        }
                    }

                }

            }
            return prodList;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.Prod = await GetProducts();
            return View();
        }


        [HttpGet("products/detail/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var detailProd = new ProductDetailModel();

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var queryDetail = @"
                    SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, Cat.nome as category, P.id_category, P.gender, P.stock
                    FROM Products as P
                    INNER JOIN Category as Cat
                    ON P.id_category = Cat.id_category
                    WHERE P.id_prod = @Id

                    SELECT MP.id_material, MP.percentage_mat, M.nome
                    FROM Products as P
                    INNER JOIN ProdMaterial as MP
                    ON P.id_prod = MP.id_prod
                    INNER JOIN Materials as M
                    ON MP.id_material = M.id_material
                    WHERE P.id_prod = @Id
                    ORDER BY MP.percentage_mat DESC

                    SELECT C.id_color, C.nome,STRING_AGG(PCI.img_URL, ', ') as img_URL
                    FROM Products as P
                    INNER JOIN ProdColor as PC
                    ON P.id_prod = PC.id_prod
                    INNER JOIN Colors as C
                    ON PC.id_color = C.id_color
                    INNER JOIN ProdColorImages as PCI
                    ON PC.id_prodColor = PCI.id_prodColor
                    WHERE P.id_prod = @Id
                    GROUP BY C.nome, C.id_color

                    SELECT S.id_size, S.nome
                    FROM Products as P
                    INNER JOIN ProdSize as PS
                    ON P.id_prod = PS.id_prod
                    INNER JOIN Sizes as S
                    ON PS.id_size = S.id_size
                    WHERE P.id_prod = @Id
                    ORDER BY S.id_size";

                await using (SqlCommand detail = new SqlCommand(queryDetail, connection))
                {
                    detail.Parameters.AddWithValue("@Id", id);
                    await using (SqlDataReader reader = await detail.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            detailProd.Id = reader.GetGuid(0);
                            detailProd.Name = reader["nome"].ToString();
                            detailProd.Brand = reader["brand"].ToString();
                            detailProd.Price = decimal.Parse(reader["price"].ToString());
                            detailProd.Description = reader["descr"].ToString().Split('/').Select(s => s.Trim()).ToList();
                            detailProd.Category = reader["category"].ToString();
                            detailProd.Gender = reader["gender"].ToString();
                            detailProd.Stock = int.Parse(reader["stock"].ToString());
                        }

                        await reader.NextResultAsync();
                        var materials = new List<MaterialModel>();
                        while (await reader.ReadAsync())
                        {
                            materials.Add(new MaterialModel
                            {
                                IdMaterial = reader.GetInt32(0),
                                Name = reader.GetString(2),
                                Percentage = reader.GetDecimal(1)
                            });
                        }
                        detailProd.Materials = materials;

                        await reader.NextResultAsync();
                        var colors = new List<ColorModel>();
                        while (await reader.ReadAsync())
                        {
                            colors.Add(new ColorModel
                            {
                                IdColor = reader.GetInt32(0),
                                Name = reader["nome"].ToString(),
                                ImgListModel = reader["img_URL"].ToString().Split(',').Select(s => s.Trim()).ToList()
                            });
                        }
                        detailProd.Colors = colors;

                        await reader.NextResultAsync();
                        var size = new List<SizeModel>();
                        while (await reader.ReadAsync())
                        {
                            size.Add(new SizeModel
                            {
                                IdSize = reader.GetInt32(0),
                                Name = reader.GetString(1),
                            });
                        }
                        detailProd.Sizes = size;
                    }
                }
            }
            ViewBag.Prod = await GetProducts();
            return View(detailProd);
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

        private async Task<List<ColorModel>> GetColor()
        {
            List<Models.ColorModel> colors = new List<Models.ColorModel>();
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
                                new Models.ColorModel()
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

        private async Task<List<SizeModel>> GetSizes()
        {
            List<Models.SizeModel> sizes = new List<Models.SizeModel>();
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
                                new Models.SizeModel()
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

        private async Task<List<MaterialModel>> GetMaterials()
        {
            List<MaterialModel> materials = new List<MaterialModel>();
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
                                new MaterialModel()
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

        [HttpPost]
        public async Task<IActionResult> AddSave(ProductAddModel model)
        {
            var nameV = HttpContext.Request.Form["Name"];

            try
            {
                await using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        var queryProd = "INSERT INTO Products (brand, gender, nome, id_category, descr, price, stock) OUTPUT INSERTED.id_prod VALUES (@brand, @gender, @nome, @id_category, @descr, @price, @stock)";
                        var productId = Guid.NewGuid();
                        await using (SqlCommand prod = new SqlCommand(queryProd, connection, (SqlTransaction)transaction))
                        {
                            prod.Parameters.AddWithValue("@brand", model.Brand);
                            prod.Parameters.AddWithValue("@gender", model.Gender);
                            prod.Parameters.AddWithValue("@nome", model.Name);
                            prod.Parameters.AddWithValue("@id_category", model.IdCategory);
                            prod.Parameters.AddWithValue("@descr", model.Description);
                            prod.Parameters.AddWithValue("@price", model.Price);
                            prod.Parameters.AddWithValue("@stock", model.Stock);

                            var result = await prod.ExecuteScalarAsync();

                            if (result == DBNull.Value || result == null)
                            {
                                throw new Exception("L'inserimento del prodotto non è andato a buon fine.");
                            }

                            productId = (Guid)result;
                        }

                        var queryProdSize = @"INSERT INTO ProdSize (id_prod, id_size) VALUES (@id_prod, @id_size)";

                        if (model.SelectedSizes != null && model.SelectedSizes.Count > 0)
                        {
                            foreach (var size in model.SelectedSizes)
                            {
                                await using (SqlCommand prodSize = new SqlCommand(queryProdSize, connection, (SqlTransaction)transaction))
                                {
                                    prodSize.Parameters.AddWithValue("@id_prod", productId);
                                    prodSize.Parameters.AddWithValue("@id_size", size);
                                    await prodSize.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        var queryPrdColor = @"INSERT INTO ProdColor(id_prod, id_color) OUTPUT INSERTED.id_prodColor VALUES (@id_prod, @id_color)";
                        var prodColor = Guid.NewGuid();
                        var validColor = model.SelectedColor
                            .Where(m => m != null && m.IdColor > 0 && m.ImgListModel != null && m.ImgListModel.Any(item => !string.IsNullOrEmpty(item)))
                            .ToList();

                        foreach (var color in validColor)
                        {
                            await using (SqlCommand prdColor = new SqlCommand(queryPrdColor, connection, (SqlTransaction)transaction))
                            {
                                prdColor.Parameters.AddWithValue("@id_prod", productId);
                                prdColor.Parameters.AddWithValue("@id_color", color.IdColor);
                                var result = await prdColor.ExecuteScalarAsync();

                                if (result == DBNull.Value || result == null)
                                {
                                    throw new Exception("L'inserimento del prodotto non è andato a buon fine. in color");
                                }

                                prodColor = (Guid)result;
                            }

                            var queryColorImg = @"INSERT INTO ProdColorImages(id_prodColor, img_URL) VALUES (@id_prodColor, @img_URL)";
                            var validImg = color.ImgListModel
                            .Where(m => m != null && !string.IsNullOrEmpty(m))
                            .ToList();
                            foreach (var img in validImg)
                            {
                                await using (SqlCommand prdImg = new SqlCommand(queryColorImg, connection, (SqlTransaction)transaction))
                                {
                                    prdImg.Parameters.AddWithValue("@id_prodColor", prodColor);
                                    prdImg.Parameters.AddWithValue("@img_URL", img);
                                    await prdImg.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        var queryMat = "INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES (@id_prod, @id_material, @percentage_mat)";
                        var validMaterials = model.SelectedMaterials
                            .Where(m => m.IdMaterial > 0 && m.Percentage > 0)
                            .ToList();
                        foreach (var mat in validMaterials)
                        {
                            await using (SqlCommand prdMat = new SqlCommand(queryMat, connection, (SqlTransaction)transaction))
                            {
                                prdMat.Parameters.AddWithValue("@id_prod", productId);
                                prdMat.Parameters.AddWithValue("@id_material", mat.IdMaterial);
                                prdMat.Parameters.AddWithValue("@percentage_mat", mat.Percentage);
                                await prdMat.ExecuteNonQueryAsync();
                            }
                        }
                        await transaction.CommitAsync();
                    }
                }
                return RedirectToAction("Add");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Si è verificato un errore durante l'inserimento dei dati. Riprova più tardi. Errore: {ex.Message}";
                return RedirectToAction("Add");
            }
        }

    }
}
