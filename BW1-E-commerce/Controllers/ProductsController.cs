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

        public async Task<IActionResult> Index()
        {
            var prodList = new ProductListModel()
            {
                ProductList = new List<ProductBaseModel>()
            };

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @" WITH ImageSelection AS (SELECT PC.id_prod, PCI.img_URL, ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn FROM ProdColorImages PCI JOIN ProdColor PC ON PCI.id_prodColor = PC.id_prodColor) SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, P.id_category, P.gender, ISel.img_URL FROM Products P LEFT JOIN ImageSelection ISel ON P.id_prod = ISel.id_prod AND ISel.rn = 1;";
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
                                    Gender = reader["gender"].ToString(),
                                    ImgURL = reader["img_URL"].ToString()
                                }
                            );
                        }
                    }

                }

            }
            return View(prodList);
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

        private async Task<List<BW1_E_commerce.Models.Color>> GetColor()
        {
            List<Models.Color> colors = new List<Models.Color>();
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
                                new Models.Color()
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

        private async Task<List<BW1_E_commerce.Models.Size>> GetSizes()
        {
            List<Models.Size> sizes = new List<Models.Size>();
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
                                new Models.Size()
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

                        if (model.Sizes != null && model.Sizes.Count > 0)
                        {
                            foreach (var size in model.Sizes)
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

        public async Task<IActionResult> Delete(Guid id)
        {
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"DELETE FROM ProdColorImages WHERE id_prodColor IN (SELECT id_prodColor FROM ProdColor WHERE id_prod = @id);
                DELETE FROM ProdMaterial WHERE id_prod = @id;
                DELETE FROM ProdColor WHERE id_prod = @id;
				DELETE FROM ProdSize WHERE id_prod = @id;
                DELETE FROM Products WHERE id_prod = @id";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        TempData["Success"] = "Prodotto eliminato con successo.";
                    }
                    else
                    {
                        TempData["Error"] = "Errore nell'eliminazione del prodotto.";
                    }
                }
            }

            return RedirectToAction("TableProducts");
        }

        public async Task<IActionResult> TableProducts()
        {
            {
                var prodList = new ProductListModel()
                {
                    ProductList = new List<ProductBaseModel>()
                };

                await using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @" WITH ImageSelection AS (SELECT PC.id_prod, PCI.img_URL, ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn FROM ProdColorImages PCI JOIN ProdColor PC ON PCI.id_prodColor = PC.id_prodColor) SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, P.id_category, P.gender, ISel.img_URL FROM Products P LEFT JOIN ImageSelection ISel ON P.id_prod = ISel.id_prod AND ISel.rn = 1;";
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
                                        Gender = reader["gender"].ToString(),
                                        ImgURL = reader["img_URL"].ToString()
                                    }
                                );
                            }
                        }

                    }

                }
                return View(prodList);
            }
        }

        [HttpGet]


        public async Task<IActionResult> Edit(Guid id)
        {
            var model = new EditProduct();
            ViewBag.Categories = await GetCategories();
            ViewBag.Color = await GetColor();
            ViewBag.Sizes = await GetSizes();
            ViewBag.Material = await GetMaterials();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    //Recupero dei dati principali del prodotto
                    var queryProd = @"
                SELECT id_prod, nome, brand, price, descr, id_category, gender, stock 
                FROM Products 
                WHERE id_prod = @id_prod";
                    using (var cmdProd = new SqlCommand(queryProd, connection))
                    {
                        cmdProd.Parameters.AddWithValue("@id_prod", id);
                        using (var reader = await cmdProd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.IdProd = (Guid)reader["id_prod"];
                                model.Name = reader["nome"].ToString();
                                model.Brand = reader["brand"].ToString();
                                model.Price = (decimal)reader["price"];
                                model.Description = reader["descr"].ToString();
                                model.IdCategory = Convert.ToInt32(reader["id_category"]);
                                model.Gender = reader["gender"].ToString();
                                model.Stock = Convert.ToInt32(reader["stock"]);
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }

                    //Recupero delle dimensioni (ProdSize)
                    var querySizes = "SELECT id_size FROM ProdSize WHERE id_prod = @id_prod";
                    using (var cmdSizes = new SqlCommand(querySizes, connection))
                    {
                        cmdSizes.Parameters.AddWithValue("@id_prod", id);
                        using (var reader = await cmdSizes.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                model.SelectedSizes.Add(Convert.ToInt32(reader["id_size"]));
                            }
                        }
                    }

                    // Recupero dei colori e relative immagini (ProdColor e ProdColorImages)
                    model.SelectedColors = new List<ColorEditModel>();
                    var queryColors = @"
                SELECT PC.id_prodColor, PC.id_color, PCI.img_URL 
                FROM ProdColor PC 
                LEFT JOIN ProdColorImages PCI ON PC.id_prodColor = PCI.id_prodColor 
                WHERE PC.id_prod = @id_prod";
                    using (var cmdColors = new SqlCommand(queryColors, connection))
                    {
                        cmdColors.Parameters.AddWithValue("@id_prod", id);
                        using (var reader = await cmdColors.ExecuteReaderAsync())
                        {
                            // Raggruppa i colori per id_prodColor per raccogliere tutte le immagini associate
                            var colorsDict = new Dictionary<Guid, ColorEditModel>();
                            while (await reader.ReadAsync())
                            {
                                var prodColorId = (Guid)reader["id_prodColor"];
                                var idColor = Convert.ToInt32(reader["id_color"]);
                                var imgUrl = reader["img_URL"] != DBNull.Value ? reader["img_URL"].ToString() : null;

                                if (!colorsDict.ContainsKey(prodColorId))
                                {
                                    var colorModel = new ColorEditModel
                                    {
                                        IdColor = idColor,
                                        ImgListModel = new List<string>()
                                    };
                                    if (!string.IsNullOrEmpty(imgUrl))
                                        colorModel.ImgListModel.Add(imgUrl);

                                    colorsDict.Add(prodColorId, colorModel);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(imgUrl))
                                        colorsDict[prodColorId].ImgListModel.Add(imgUrl);
                                }
                            }
                            model.SelectedColors = colorsDict.Values.ToList();
                        }
                    }

                    //Recupero dei materiali (ProdMaterial)
                    model.SelectedMaterials = new List<MaterialEditModel>();
                    var queryMaterials = "SELECT id_material, percentage_mat FROM ProdMaterial WHERE id_prod = @id_prod";
                    using (var cmdMat = new SqlCommand(queryMaterials, connection))
                    {
                        cmdMat.Parameters.AddWithValue("@id_prod", id);
                        using (var reader = await cmdMat.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var material = new MaterialEditModel
                                {
                                    IdMaterial = Convert.ToInt32(reader["id_material"]),
                                    Percentage = (decimal)reader["percentage_mat"]
                                };
                                model.SelectedMaterials.Add(material);
                            }
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Errore durante il caricamento del prodotto: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> EditSave(EditProduct model)
        {
            try
            {
                await using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        //Aggiornamento del record principale in Products
                        var queryUpdateProd = @"
                    UPDATE Products 
                    SET brand = @brand,
                        gender = @gender,
                        nome = @nome,
                        id_category = @id_category,
                        descr = @descr,
                        price = @price,
                        stock = @stock
                    WHERE id_prod = @id_prod";

                        await using (SqlCommand cmdUpdateProd = new SqlCommand(queryUpdateProd, connection, (SqlTransaction)transaction))
                        {
                            cmdUpdateProd.Parameters.AddWithValue("@brand", model.Brand);
                            cmdUpdateProd.Parameters.AddWithValue("@gender", model.Gender);
                            cmdUpdateProd.Parameters.AddWithValue("@nome", model.Name);
                            cmdUpdateProd.Parameters.AddWithValue("@id_category", model.IdCategory);
                            cmdUpdateProd.Parameters.AddWithValue("@descr", model.Description);
                            cmdUpdateProd.Parameters.AddWithValue("@price", model.Price);
                            cmdUpdateProd.Parameters.AddWithValue("@stock", model.Stock);
                            cmdUpdateProd.Parameters.AddWithValue("@id_prod", model.IdProd);

                            await cmdUpdateProd.ExecuteNonQueryAsync();
                        }

                        // Gestione delle tabelle figlie:
                        //dimensioni
                        var deleteProdSize = "DELETE FROM ProdSize WHERE id_prod = @id_prod";
                        await using (SqlCommand cmdDelSize = new SqlCommand(deleteProdSize, connection, (SqlTransaction)transaction))
                        {
                            cmdDelSize.Parameters.AddWithValue("@id_prod", model.IdProd);
                            await cmdDelSize.ExecuteNonQueryAsync();
                        }

                        if (model.SelectedSizes != null && model.SelectedSizes.Count > 0)
                        {
                            var queryProdSize = "INSERT INTO ProdSize (id_prod, id_size) VALUES (@id_prod, @id_size)";
                            foreach (var size in model.SelectedSizes)
                            {
                                await using (SqlCommand prodSize = new SqlCommand(queryProdSize, connection, (SqlTransaction)transaction))
                                {
                                    prodSize.Parameters.AddWithValue("@id_prod", model.IdProd);
                                    prodSize.Parameters.AddWithValue("@id_size", size);
                                    await prodSize.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // colori e  relative immagini
                        var deleteColorImages = "DELETE FROM ProdColorImages WHERE id_prodColor IN (SELECT id_prodColor FROM ProdColor WHERE id_prod = @id_prod)";
                        await using (SqlCommand cmdDelColorImages = new SqlCommand(deleteColorImages, connection, (SqlTransaction)transaction))
                        {
                            cmdDelColorImages.Parameters.AddWithValue("@id_prod", model.IdProd);
                            await cmdDelColorImages.ExecuteNonQueryAsync();
                        }
                        var deleteProdColor = "DELETE FROM ProdColor WHERE id_prod = @id_prod";
                        await using (SqlCommand cmdDelColor = new SqlCommand(deleteProdColor, connection, (SqlTransaction)transaction))
                        {
                            cmdDelColor.Parameters.AddWithValue("@id_prod", model.IdProd);
                            await cmdDelColor.ExecuteNonQueryAsync();
                        }

                        var queryPrdColor = "INSERT INTO ProdColor(id_prod, id_color) OUTPUT INSERTED.id_prodColor VALUES (@id_prod, @id_color)";
                        var queryColorImg = "INSERT INTO ProdColorImages(id_prodColor, img_URL) VALUES (@id_prodColor, @img_URL)";

                        var validColor = model.SelectedColors
                        .Where(m => m != null && m.IdColor > 0 && m.ImgListModel != null && m.ImgListModel.Any(item => !string.IsNullOrEmpty(item)))
                        .ToList();


                        foreach (var color in validColor)
                        {
                            Guid prodColorId;
                            await using (SqlCommand prdColor = new SqlCommand(queryPrdColor, connection, (SqlTransaction)transaction))
                            {
                                prdColor.Parameters.AddWithValue("@id_prod", model.IdProd);
                                prdColor.Parameters.AddWithValue("@id_color", color.IdColor);
                                var result = await prdColor.ExecuteScalarAsync();

                                if (result == DBNull.Value || result == null)
                                {
                                    throw new Exception("Errore nell'aggiornamento del colore del prodotto.");
                                }

                                prodColorId = (Guid)result;
                            }

                            var validImg = color.ImgListModel.Where(m => m != null && !string.IsNullOrEmpty(m)).ToList();
                            foreach (var img in validImg)
                            {
                                await using (SqlCommand prdImg = new SqlCommand(queryColorImg, connection, (SqlTransaction)transaction))
                                {
                                    prdImg.Parameters.AddWithValue("@id_prodColor", prodColorId);
                                    prdImg.Parameters.AddWithValue("@img_URL", img);
                                    await prdImg.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        //Gestione dei materiali
                        var deleteProdMat = "DELETE FROM ProdMaterial WHERE id_prod = @id_prod";
                        await using (SqlCommand cmdDelMat = new SqlCommand(deleteProdMat, connection, (SqlTransaction)transaction))
                        {
                            cmdDelMat.Parameters.AddWithValue("@id_prod", model.IdProd);
                            await cmdDelMat.ExecuteNonQueryAsync();
                        }

                        var queryMat = "INSERT INTO ProdMaterial (id_prod, id_material, percentage_mat) VALUES (@id_prod, @id_material, @percentage_mat)";
                        var validMaterials = model.SelectedMaterials
                            .Where(m => m.IdMaterial > 0 && m.Percentage > 0)
                            .ToList();
                        foreach (var mat in validMaterials)
                        {
                            await using (SqlCommand prdMat = new SqlCommand(queryMat, connection, (SqlTransaction)transaction))
                            {
                                prdMat.Parameters.AddWithValue("@id_prod", model.IdProd );
                                prdMat.Parameters.AddWithValue("@id_material", mat.IdMaterial);
                                prdMat.Parameters.AddWithValue("@percentage_mat", mat.Percentage);
                                await prdMat.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                }
                return RedirectToAction("TableProducts", new { id = model.IdProd });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Si è verificato un errore durante l'aggiornamento del prodotto. Errore: " + ex.Message;
                return RedirectToAction("Edit", new { id = model.IdProd });
            }
        }

    }
}
