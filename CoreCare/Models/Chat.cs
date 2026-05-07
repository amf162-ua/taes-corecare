using System;
using System.Collections.Generic;

namespace CoreCare.Models
{
    public enum ChatStatus { Abierto, Cerrado }

    public class Chat
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Subject { get; set; }
        public ChatStatus Status { get; set; } = ChatStatus.Abierto;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        // Navigation properties
        public User Client { get; set; }
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
