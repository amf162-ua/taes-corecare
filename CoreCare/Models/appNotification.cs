using System;

namespace CoreCare.Models
{
    public class AppNotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } // "success", "warning", "info"
        public string Title { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm");
        public bool IsRead { get; set; }

        // Propiedad calculada para el puntito cian
        public bool ShowUnreadIndicator => !IsRead;
    }
}