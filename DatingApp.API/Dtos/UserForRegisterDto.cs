using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
  public class UserForRegisterDto
  {
    [Required]
    public string Username { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 4, ErrorMessage = "Debes digitar una contraseña entre 4 y 8 caracteres")]
    public string Password { get; set; }
  }
}