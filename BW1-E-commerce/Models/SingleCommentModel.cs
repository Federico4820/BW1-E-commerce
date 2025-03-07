namespace BW1_E_commerce.Models
{
    public class SingleCommentModel
    {
        public Guid IdComment { get; set;}
        public Guid idProd { get; set; }
        public Guid IdUser { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
