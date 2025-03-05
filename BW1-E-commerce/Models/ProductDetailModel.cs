namespace BW1_E_commerce.Models
{
    public class ProductDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public List<string> Description { get; set; }
        public string Category { get; set; }
        public string Gender { get; set; }
        public int Stock { get; set; }
        public List<ColorModel> Colors { get; set; }
        public List<MaterialModel> Materials { get; set; }
        public List<SizeModel> Sizes { get; set; }

    }
}
