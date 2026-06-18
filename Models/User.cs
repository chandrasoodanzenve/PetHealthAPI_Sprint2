namespace PetHealthAPI.Models
{
        /// <summary>
        ///     Represents a user in the system.
        /// </summary>
    public class User
    {
        /// <summary>
        ///     Gets or sets the user's unique identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        ///     Gets or sets the user's username.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the user's password.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the user's role.
        /// </summary>
        public string Role { get; set; } = "Customer";
    }
}