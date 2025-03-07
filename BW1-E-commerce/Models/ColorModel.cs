namespace BW1_E_commerce.Models
{
    public class ColorModel
    {
        public int IdColor { get; set; }
        public string Name { get; set; }
        public Guid IdProdColor { get; set; }
        public List<string> ImgListModel { get; set; } = new List<string>();

    }
}
