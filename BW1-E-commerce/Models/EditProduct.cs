using System.ComponentModel.DataAnnotations;

namespace BW1_E_commerce.Models
{
    public class EditProduct
    {
        public Guid IdProd { get; set; }
        public string? Brand { get; set; }
        public string? Gender { get; set; }
        public string? Name { get; set; }
        public int? IdCategory { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? Email { get; set; }

        // Liste di opzioni disponibili (da popolare nella GET action)
        public List<SizeModel> Sizes { get; set; } = new List<SizeModel>();
        public List<ColorEditModel> Colors { get; set; } = new List<ColorEditModel>();
        public List<MaterialModel> Materials { get; set; } = new List<MaterialModel>();

        // Valori selezionati dall'utente (per l'aggiornamento)
        public List<int> SelectedSizes { get; set; } = new List<int>();
        public List<ColorEditModel> SelectedColors { get; set; } = new List<ColorEditModel>();
        public List<MaterialEditModel> SelectedMaterials { get; set; } = new List<MaterialEditModel>();
    }
}
