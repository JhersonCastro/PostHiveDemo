using System.ComponentModel.DataAnnotations;

namespace DbContext
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static class Credentials
    {
        public class LoginRequest
        {
            [Required]
            [EmailAddress(ErrorMessage = "Email incorrect pattern")]
            public string Email { get; set; }
            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            public string Password { get; set; }
        }
        public class RegisterRequest
        {
            [Required]
            [EmailAddress(ErrorMessage = "Invalid Email Address")]
            public string Email { get; set; }
            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            public string Password { get; set; }
            [Required]
            [MinLength(6, ErrorMessage = "Name must be at least 6 characters")]
            public string Name { get; set; }
            [Required]
            [MinLength(6, ErrorMessage = "Nickname must be at least 6 characters")]
            public string NickName { get; set; }
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
