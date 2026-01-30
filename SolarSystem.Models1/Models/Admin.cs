

namespace SolarSystem.Models1.Models
{
    public enum AdminRole
    {
        Viewer=1,
        Editor=2,
        MasterAdmin=3 // The "God Mode" admin 👑
    }

    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public AdminRole Role { get; set; } // Assign a role here
    }

}
