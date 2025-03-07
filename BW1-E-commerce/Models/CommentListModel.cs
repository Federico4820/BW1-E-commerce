namespace BW1_E_commerce.Models
{
    public class CommentListModel
    {
        public List<SingleCommentModel> CommentList { get; set; }
        public int TotalComment { get; set; }
        public decimal AvgRating { get; set; }
    }
}
