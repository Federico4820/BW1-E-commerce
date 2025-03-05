namespace BW1_E_commerce.Models
{
    public class ProductBaseModel
    {
        public Guid? IdProd { get; set; }
        public string? Brand { get; set; }
        public string? Gender { get; set; }
        public string? Name { get; set; }
        public int? IdCategory { get; set; }
        public List<string>? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string ImgURL { get; set; }

    }
}
