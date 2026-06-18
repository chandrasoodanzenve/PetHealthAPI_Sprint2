namespace PetHealthAPI.Models
{
    /// <summary>
    /// Represents a request for user login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username of the user attempting to log in. This field is required for authentication.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Password of the user attempting to log in. This field is required for authentication.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}