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

        public async Task<ProductListModel> GetProducts(string gender = null, string search = null)
        {
            var prodList = new ProductListModel()
            {
                ProductList = new List<ProductBaseModel>()
            };

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"WITH ImageSelection AS (
                        SELECT PC.id_prod, PCI.img_URL, 
                               ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn
                        FROM ProdColorImages PCI 
                        JOIN ProdColor PC ON PCI.id_prodColor = PC.id_prodColor
                      ) 
                      SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, 
                             P.id_category, C.nome as category_name, P.gender, ISel.img_URL
                      FROM Products P 
                      LEFT JOIN Category as C ON P.id_category = C.id_category
                      LEFT JOIN ImageSelection ISel ON P.id_prod = ISel.id_prod AND ISel.rn = 1";

                if (!string.IsNullOrWhiteSpace(gender))
                {
                    query += " WHERE P.gender IN (@gender, 'U')";
                }

                // Se la ricerca non è vuota, aggiungiamo il filtro LIKE
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " AND (P.nome LIKE @search OR P.brand LIKE @search)";
                }

                await using (SqlCommand command = new SqlCommand(query, connection))
                {

                    if (!string.IsNullOrWhiteSpace(gender))
                    {
                        command.Parameters.AddWithValue("@gender", gender);
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        command.Parameters.AddWithValue("@search", $"%{search}%");
                    }

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
                                    NameCategory = reader["category_name"].ToString(),
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

        public async Task<IActionResult> Intro()
        {
            return View();
        }
        public async Task<IActionResult> Index(int? filter, string gender, string? search)
        {
            ViewBag.Prod = await GetProducts(gender, search);
            ViewBag.Categories = await GetCategories();
            if (filter.HasValue)
            {
                ViewBag.Filtered = await GetProductFiltered(filter.Value, gender);
            }
            ViewBag.Gender = gender;
            return View();
        }

        [HttpGet("products/detail/{id:guid}")]
        public async Task<IActionResult> Details(Guid id, string gender)
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
                            detailProd.IdCategory = int.Parse(reader["id_category"].ToString());
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
            ViewBag.Prod = await GetProducts(gender);
            ViewBag.Comments = await GetComments(id);
            ViewBag.Commen = new SingleCommentModel();
            ViewBag.Categories = await GetCategories();
            ViewBag.Gender = gender;
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

        //ACTION PER FAR FUNZIONARE IL FORM DI AGGIUNTA PRODOTTO
        public async Task<IActionResult> AddDelete()
        {
            ViewBag.Categories = await GetCategories();
            ViewBag.Color = await GetColor();
            ViewBag.Sizes = await GetSizes();
            ViewBag.Material = await GetMaterials();
            ViewBag.Prod = await GetProducts();
            ViewBag.AddProd = new ProductAddModel();
            return View();
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
                return RedirectToAction("AddDelete");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Si è verificato un errore durante l'inserimento dei dati. Riprova più tardi. Errore: {ex.Message}";
                return RedirectToAction("Add");
            }
        }

        //ACTION PER RECUPERARE I COMMENTI DI UN PRODOTTO
        public async Task<CommentListModel> GetComments(Guid id)
        {
            var commentList = new CommentListModel()
            {
                CommentList = new List<SingleCommentModel>()

            };
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT R.id_rating, R.id_prod, R.id_user, R.comment, R.rating, R.CreatedAt, U.email FROM Ratings as R LEFT JOIN Products as P ON P.id_prod= R.id_prod LEFT JOIN Users as U ON R.id_user=U.id_user WHERE P.id_prod = @Id";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            commentList.CommentList.Add(
                                new SingleCommentModel()
                                {
                                    IdComment = reader.GetGuid(0),
                                    IdUser = reader.GetGuid(2),
                                    Comment = reader.GetString(3),
                                    Rating = reader.GetInt32(4),
                                    CreatedAt = reader.GetDateTime(5),
                                    Email = reader.GetString(6)
                                }
                            );
                        }
                    }
                }
            }
            commentList.TotalComment = commentList.CommentList.Count;
            if (commentList.CommentList.Any())
            {
                commentList.AvgRating = (decimal)commentList.CommentList.Average(c => c.Rating);
            }
            else
            {
                commentList.AvgRating = 0;
            }
            return commentList;
        }

        //ACTION PER ADD DEL COMMENTO
        [HttpPost("products/write-comment")]
        public async Task<IActionResult> WriteComment(SingleCommentModel model, string gender)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Qualcosa è andato storto";
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }
            try
            {
                await using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    Guid idUser = Guid.NewGuid();
                    var queryUser = "SELECT id_user FROM Users WHERE email= @email";
                    await using (SqlCommand command = new SqlCommand(queryUser, connection))
                    {
                        command.Parameters.AddWithValue("@email", model.Email);
                        var result = await command.ExecuteScalarAsync() as Guid?;
                        if (result.HasValue)
                        {
                            idUser = result.Value;
                        }
                        else
                        {
                            var queryInsertUser = @" INSERT INTO Users (id_user, email, user_role) VALUES (@id_user, @email, @user_role)";

                            await using (SqlCommand insertCommand = new SqlCommand(queryInsertUser, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@id_user", idUser);
                                insertCommand.Parameters.AddWithValue("@email", model.Email);
                                insertCommand.Parameters.AddWithValue("@user_role", "User");
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }

                    }

                    var queryUser2 = "SELECT id_user FROM Ratings WHERE id_user= @id_user AND id_prod = @id_prod";
                    await using (SqlCommand command = new SqlCommand(queryUser2, connection))
                    {
                        command.Parameters.AddWithValue("@id_user", idUser);
                        command.Parameters.AddWithValue("@id_prod", model.idProd);
                        var result = await command.ExecuteScalarAsync() as Guid?;
                        if (result.HasValue)
                        {
                            var querUpdate = "UPDATE Ratings SET comment = @Comment, rating = @Rating, CreatedAt = @CreatedAt WHERE id_user = @UserId AND id_prod = @ProductId;";
                            await using (SqlCommand comment = new SqlCommand(querUpdate, connection))
                            {
                                comment.Parameters.AddWithValue("@UserId", idUser);
                                comment.Parameters.AddWithValue("@ProductId", model.idProd);
                                comment.Parameters.AddWithValue("@Comment", model.Comment);
                                comment.Parameters.AddWithValue("@Rating", model.Rating);
                                comment.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                                await comment.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            var queryComment = @" INSERT INTO Ratings (id_rating, id_prod, id_user, comment, rating, CreatedAt) VALUES (@IdRating, @ProductId, @UserId, @Comment, @Rating, @CreatedAt)";
                            await using (SqlCommand comment = new SqlCommand(queryComment, connection))
                            {
                                comment.Parameters.AddWithValue("@IdRating", Guid.NewGuid());
                                comment.Parameters.AddWithValue("@ProductId", model.idProd);
                                comment.Parameters.AddWithValue("@UserId", idUser);
                                comment.Parameters.AddWithValue("@Comment", model.Comment);
                                comment.Parameters.AddWithValue("@Rating", model.Rating);
                                comment.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                                await comment.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Si è verificato un errore durante l'inserimento dei dati. Riprova più tardi. Errore: {ex.Message}";
                return RedirectToAction("Add");
            }
            return RedirectToAction("Details", new { id = model.idProd, gender });
        }


        //ACTION PER AGGIUNTA AL CARRELLO
        public async Task<IActionResult> AddToCart(Guid idProd, int quantity, string price, int id_color, int id_size)
        {
            await using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = await conn.BeginTransactionAsync())
                {
                    var idOrder = Guid.NewGuid();
                    string checkOrderQuery = "SELECT TOP 1 id_order FROM Orders ORDER BY id_order DESC;";
                    await using (SqlCommand checkOrderCmd = new SqlCommand(checkOrderQuery, conn, (SqlTransaction)transaction))
                    {
                        object result = await checkOrderCmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            idOrder = (Guid)result;
                        }
                        else
                        {
                            idOrder = Guid.NewGuid();
                            string createOrderQuery = "INSERT INTO Orders (id_order, total) VALUES (@id_order, 0);";
                            await using (SqlCommand createOrderCmd = new SqlCommand(createOrderQuery, conn, (SqlTransaction)transaction))
                            {
                                createOrderCmd.Parameters.AddWithValue("@id_order", idOrder);
                                await createOrderCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // Controlla se il prodotto con la specifica combinazione di colore e taglia è già nel carrello
                    string checkCartQuery = @"
                SELECT qt 
                FROM Cart 
                WHERE id_order = @id_order 
                  AND id_prod = @idProd 
                  AND id_color = @id_color 
                  AND id_size = @id_size;";
                    await using (SqlCommand checkCartCmd = new SqlCommand(checkCartQuery, conn, (SqlTransaction)transaction))
                    {
                        checkCartCmd.Parameters.AddWithValue("@id_order", idOrder);
                        checkCartCmd.Parameters.AddWithValue("@idProd", idProd);
                        checkCartCmd.Parameters.AddWithValue("@id_color", id_color);
                        checkCartCmd.Parameters.AddWithValue("@id_size", id_size);

                        object cartResult = await checkCartCmd.ExecuteScalarAsync();

                        if (cartResult != null)
                        {
                            // Se il prodotto (con la combinazione colore/taglia) è già nel carrello, aggiorna la quantità
                            string updateCartQuery = @"
                        UPDATE Cart 
                        SET qt = qt + @quantity 
                        WHERE id_order = @id_order 
                          AND id_prod = @idProd 
                          AND id_color = @id_color 
                          AND id_size = @id_size;";
                            await using (SqlCommand updateCartCmd = new SqlCommand(updateCartQuery, conn, (SqlTransaction)transaction))
                            {
                                updateCartCmd.Parameters.AddWithValue("@id_order", idOrder);
                                updateCartCmd.Parameters.AddWithValue("@idProd", idProd);
                                updateCartCmd.Parameters.AddWithValue("@id_color", id_color);
                                updateCartCmd.Parameters.AddWithValue("@id_size", id_size);
                                updateCartCmd.Parameters.AddWithValue("@quantity", quantity);
                                await updateCartCmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Se il prodotto non è presente, lo inserisce nel carrello con il colore e la taglia selezionati
                            string insertCartQuery = @"
                        INSERT INTO Cart (id_order, id_prod, qt, price, id_color, id_size) 
                        VALUES (@id_order, @idProd, @quantity, @price, @id_color, @id_size);";
                            await using (SqlCommand insertCartCmd = new SqlCommand(insertCartQuery, conn, (SqlTransaction)transaction))
                            {
                                insertCartCmd.Parameters.AddWithValue("@id_order", idOrder);
                                insertCartCmd.Parameters.AddWithValue("@idProd", idProd);
                                insertCartCmd.Parameters.AddWithValue("@quantity", quantity);
                                insertCartCmd.Parameters.AddWithValue("@price", decimal.Parse(price));
                                insertCartCmd.Parameters.AddWithValue("@id_color", id_color);
                                insertCartCmd.Parameters.AddWithValue("@id_size", id_size);
                                await insertCartCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // Commit della transazione
                    await transaction.CommitAsync();
                }
            }

            return RedirectToAction("Cart");
        }
        private async Task<CartViewModel> GetCartItems()
        {
            CartViewModel cart = new CartViewModel();
            Guid idOrder;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string checkOrderQuery = @"
        SELECT TOP 1 id_order FROM Orders ORDER BY id_order DESC;";

                using (SqlCommand checkCmd = new SqlCommand(checkOrderQuery, conn))
                {
                    object result = await checkCmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        idOrder = (Guid)result;
                    }
                    else
                    {
                        idOrder = Guid.NewGuid();
                        string insertOrderQuery = @"
                INSERT INTO Orders (id_order, total) 
                VALUES (@idOrder, 0);";

                        using (SqlCommand insertCmd = new SqlCommand(insertOrderQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@idOrder", idOrder);
                            await insertCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                cart.IdOrder = idOrder;

                string query = @"
                WITH ImageSelection AS (
    SELECT 
        PC.id_prod, 
        PCI.img_URL, 
        ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn
    FROM 
        ProdColorImages PCI
    JOIN 
        ProdColor PC ON PCI.id_prodColor = PC.id_prodColor
)
SELECT 
    c.id_prod, 
    p.nome, 
    c.qt, 
    c.price,
    ISel.img_URL,
    k.nome AS Color,
    k.id_color,
    s.nome AS Size,
    s.id_size 
FROM 
    Cart c
JOIN 
    Products p ON c.id_prod = p.id_prod
LEFT JOIN 
    ImageSelection ISel ON p.id_prod = ISel.id_prod AND ISel.rn = 1
LEFT JOIN Colors AS k ON k.id_color = c.id_color
LEFT JOIN Sizes AS s ON s.id_size = c.id_size
WHERE 
    c.id_order = @idOrder;
";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idOrder", idOrder);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cart.Items.Add(new CartItem
                            {
                                IdProd = reader.GetGuid(0),
                                ProductName = reader.GetString(1),
                                Quantity = reader.GetInt32(2),
                                Price = reader.GetDecimal(3),
                                ImgUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Color = reader.GetString(5),
                                IdColor = reader.GetInt32(6),
                                Size = reader.GetString(7),
                                IdSize = reader.GetInt32(8)
                            });
                        }
                    }
                }
            }

            return cart;
        }
        public async Task<IActionResult> Cart()
        {
            CartViewModel cart = await GetCartItems();
            Console.WriteLine($"idOrder usato: {cart.IdOrder}");

            return View(cart);
        }
        [HttpPost] // Ensure this action only accepts POST requests
        public IActionResult RemoveFromCart(Guid idProd, int idColor, int idSize)
        {
            try
            {
                // SQL query to delete the cart item
                string query = @"
                DELETE FROM Cart 
                WHERE id_prod = @IdProd 
                AND id_color = @IdColor 
                AND id_size = @IdSize";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdProd", idProd);
                        command.Parameters.AddWithValue("@IdColor", idColor);
                        command.Parameters.AddWithValue("@IdSize", idSize);

                        // Execute the query
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            TempData["ErrorMessage"] = "Item not found in the cart.";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Item removed from the cart successfully.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while removing the item from the cart.";
                // Log the exception (ex) here
            }

            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> ClearCart()
        {
            Guid? idOrder = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string checkOrderQuery = @"
                SELECT TOP 1 id_order FROM Orders ORDER BY id_order DESC;";

                using (SqlCommand checkCmd = new SqlCommand(checkOrderQuery, conn))
                {
                    object result = await checkCmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        idOrder = (Guid)result;
                    }
                }

                if (idOrder.HasValue)
                {
                    string deleteQuery = "DELETE FROM Cart WHERE id_order = @idOrder;";

                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@idOrder", idOrder.Value);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }
                }
            }

            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> UpdateCartQuantity(Guid idProd, int quantity)
        {
            await using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = await conn.BeginTransactionAsync())
                {
                    Guid? idOrder = null;

                    // Recupera l'ID dell'ultimo ordine esistente
                    string checkOrderQuery = "SELECT TOP 1 id_order FROM Orders ORDER BY id_order DESC;";
                    await using (SqlCommand checkOrderCmd = new SqlCommand(checkOrderQuery, conn, (SqlTransaction)transaction))
                    {
                        object result = await checkOrderCmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            idOrder = (Guid)result;
                        }
                    }

                    // Se non esiste un ordine, interrompe l'operazione
                    if (!idOrder.HasValue)
                    {
                        TempData["Error"] = "Nessun ordine esistente.";
                        return RedirectToAction("Cart");
                    }

                    if (quantity > 0)
                    {
                        // Aggiorna la quantità del prodotto nel carrello
                        string updateCartQuery = "UPDATE Cart SET qt = @quantity WHERE id_order = @id_order AND id_prod = @idProd;";
                        await using (SqlCommand updateCartCmd = new SqlCommand(updateCartQuery, conn, (SqlTransaction)transaction))
                        {
                            updateCartCmd.Parameters.AddWithValue("@id_order", idOrder.Value);
                            updateCartCmd.Parameters.AddWithValue("@idProd", idProd);
                            updateCartCmd.Parameters.AddWithValue("@quantity", quantity);
                            await updateCartCmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        // Se la quantità è 0 o negativa, rimuove il prodotto dal carrello
                        string deleteCartQuery = "DELETE FROM Cart WHERE id_order = @id_order AND id_prod = @idProd;";
                        await using (SqlCommand deleteCartCmd = new SqlCommand(deleteCartQuery, conn, (SqlTransaction)transaction))
                        {
                            deleteCartCmd.Parameters.AddWithValue("@id_order", idOrder.Value);
                            deleteCartCmd.Parameters.AddWithValue("@idProd", idProd);
                            await deleteCartCmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Commit della transazione
                    await transaction.CommitAsync();
                }
            }

            return RedirectToAction("Cart");
        }
        public IActionResult Checkout(Guid idOrder)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string updateOrderQuery = @"
                DECLARE @total DECIMAL(10,2) = (SELECT COALESCE(SUM(qt * price), 0) FROM Cart WHERE id_order = @idOrder);
                UPDATE Orders SET total = @total WHERE id_order = @idOrder;";

                        using (SqlCommand cmd = new SqlCommand(updateOrderQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@idOrder", idOrder);
                            var rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("L'ordine non è stato trovato.");
                            }
                        }

                        string clearCartQuery = "DELETE FROM Cart WHERE id_order = @idOrder;";
                        using (SqlCommand cmd = new SqlCommand(clearCartQuery, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@idOrder", idOrder);
                            var rowsDeleted = cmd.ExecuteNonQuery();
                            if (rowsDeleted == 0)
                            {
                                throw new Exception("Nessun prodotto trovato nel carrello per questo ordine.");
                            }
                        }

                        trans.Commit();
                        return RedirectToAction("OrderSuccess");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        Console.WriteLine("Errore durante il checkout: " + ex.Message);
                        return StatusCode(500, "Errore durante il checkout: " + ex.Message);
                    }
                }
            }
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }

        //ACTION DI FILTRO!

        public async Task<ProductListModel> GetProductFiltered(int filter, string gender)
        {
            var prodList = new ProductListModel()
            {
                ProductList = new List<ProductBaseModel>()
            };

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @" WITH ImageSelection AS (SELECT PC.id_prod, PCI.img_URL, ROW_NUMBER() OVER (PARTITION BY PC.id_prod ORDER BY PCI.id_prodColorImage) AS rn FROM ProdColorImages PCI JOIN ProdColor PC ON PCI.id_prodColor = PC.id_prodColor) SELECT P.id_prod, P.nome, P.brand, P.price, P.descr, P.id_category, C.nome as category_name, P.gender, ISel.img_URL FROM Products P LEFT JOIN Category as C ON P.id_category = C.id_category LEFT JOIN ImageSelection ISel ON P.id_prod = ISel.id_prod AND ISel.rn = 1 WHERE C.id_category = @filter AND (P.gender = @gender OR P.gender = 'U'); ";
                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gender", gender);
                    command.Parameters.AddWithValue("@filter", filter);
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
                                    NameCategory = reader["category_name"].ToString(),
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


        //ACTION PER EDIT E DELETE + GRIGLIA ADMIN

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

            return RedirectToAction("AddDelete");
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
                                prdMat.Parameters.AddWithValue("@id_prod", model.IdProd);
                                prdMat.Parameters.AddWithValue("@id_material", mat.IdMaterial);
                                prdMat.Parameters.AddWithValue("@percentage_mat", mat.Percentage);
                                await prdMat.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                }
                return RedirectToAction("AddDelete");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Si è verificato un errore durante l'aggiornamento del prodotto. Errore: " + ex.Message;
                return RedirectToAction("Edit", new { id = model.IdProd });
            }
        }


    }
}
