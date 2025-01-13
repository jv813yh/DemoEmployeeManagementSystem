using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.DTOs
{
    public class RegisterDto : AccountBaseDto
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Fullname { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [Compare(nameof(AccountBaseDto.Password))]
        public string ConfirmPassword { get; set; }
    }
}
