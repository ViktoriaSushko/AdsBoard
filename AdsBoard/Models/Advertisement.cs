using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AdsBoard.Models
{
    public class Advertisement
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введіть назву оголошення")]
        [Display(Name = "Назва оголошення")]
        [StringLength(100)]
        public string Title { get; set; } = default!;
        [Display(Name = "Опис оголошення")]
        public string? Description { get; set; }
        [Required]
        [Display(Name = "Ціна, грн.")]
        public decimal Price { get; set; }
        [Required]
        [Display(Name = "Місто")]
        public string? City { get; set; }
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }
        [Display(Name = "Зображення")]
        public string? ImageUrl { get; set; }
        [Display(Name = "Дата створення")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        [Display(Name = "Статус")]
        public bool IsActive { get; set; }
        [Required]
        [Display(Name = "Категорія")]
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Власник")]
        public string UserId { get; set; }
        [Display(Name = "Категорія")]
        public virtual Category? Category { get; set; }
        public virtual IdentityUser? User { get; set; }

    }
}
