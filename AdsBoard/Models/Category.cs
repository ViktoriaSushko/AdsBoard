using System.ComponentModel.DataAnnotations;

namespace AdsBoard.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введіть назву категорії")]
        [Display(Name = "Назва категорії")]
        [StringLength(100)]
        public string Name { get; set; } = default!;
        public virtual List<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

    }
}
