namespace BW1_E_commerce.Models
{
    public class ProductAddModel
    {
        public Guid? IdProd { get; set; }
        public string? Brand { get; set; }
        public string? Gender { get; set; }
        public string? Name { get; set; }
        public int? IdCategory { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? IdSize { get; set; }
        public int IdColor { get; set; }
        public int IdMaterial { get; set; }
        public List<ColorModel>? Colors { get; set; }
        public List<SizeModel>? Sizes { get; set; }
        public List<MaterialModel>? Materials { get; set; }
        public List<ColorModel>? SelectedColor { get; set; } = new List<ColorModel> { };
        public List<MaterialModel>? SelectedMaterials { get; set; } = new List<MaterialModel> { };
        public List<int>? SelectedSizes { get; set; } = new List<int> { };
        public string? Email { get; set; }

    }
}
