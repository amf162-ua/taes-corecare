using System;
using System.Collections.Generic;

namespace CoreCare.Models
{
    public enum ChatStatus
    {
        Abierto,
        Cerrado
    }

    public class Chat
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public User? Client { get; set; }
        public string Subject { get; set; } = string.Empty;
        public ChatStatus Status { get; set; } = ChatStatus.Abierto;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }
        public int SenderId { get; set; }
        public User? Sender { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
