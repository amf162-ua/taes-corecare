using System;

namespace CoreCare.ViewModels
{
    public class SupportChatItemViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public bool HasPendingReply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    public class SupportChatMessageViewModel
    {
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsMine { get; set; }
    }
}