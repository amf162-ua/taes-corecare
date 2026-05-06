using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreCare.Data;
using CoreCare.Models;
using CoreCare.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreCare.ViewModels
{
    public partial class ChatItemViewModel : ObservableObject
    {
        private readonly Chat _chat;

        public int Id => _chat.Id;
        public string ClientName => _chat.Client?.username ?? _chat.Client?.name ?? "Usuario desconocido";
        public string Subject => _chat.Subject;
        public DateTime CreatedAt => _chat.CreatedAt;
        public ChatStatus Status => _chat.Status;

        [ObservableProperty]
        private bool _hasUnreadMessages;

        public ChatItemViewModel(Chat chat, bool hasUnreadMessages = false)
        {
            _chat = chat;
            _hasUnreadMessages = hasUnreadMessages;
        }

        public Chat GetChat() => _chat;
    }

    public class ChatMessageViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public partial class AdminPanelViewModel : ObservableObject
    {
        private readonly DispatcherTimer _refreshTimer;

        [ObservableProperty]
        private string _adminUserLabel = "Usuario: -";

        [ObservableProperty]
        private ObservableCollection<ChatItemViewModel> _openChats = new();

        [ObservableProperty]
        private ChatItemViewModel? _selectedChat;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> _selectedChatMessages = new();

        [ObservableProperty]
        private string _newMessageText = string.Empty;

        [ObservableProperty]
        private int _pendingChatsCount = 0;

        public AdminPanelViewModel()
        {
            UpdateAdminLabel();
            LoadOpenChats();

            // Setup polling for new messages (~3 seconds)
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(3);
            _refreshTimer.Tick += (_, _) => RefreshChats();
            _refreshTimer.Start();
        }

        private void UpdateAdminLabel()
        {
            if (SessionService.CurrentUser != null)
            {
                var username = string.IsNullOrWhiteSpace(SessionService.CurrentUser.username) 
                    ? SessionService.CurrentUser.name 
                    : SessionService.CurrentUser.username;
                AdminUserLabel = $"Usuario: {username} (Administrador)";
            }
        }

        private void LoadOpenChats()
        {
            try
            {
                using var db = new CoreCareDbContext();
                
                var openChats = db.Chats
                    .Where(c => c.Status == ChatStatus.Abierto)
                    .Include(c => c.Client)
                    .Include(c => c.Messages)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                OpenChats.Clear();
                
                foreach (var chat in openChats)
                {
                    // Check if there are unread messages (messages from client that come after admin's last message)
                    var hasUnread = chat.Messages
                        .Where(m => m.SenderId == chat.ClientId)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault() is ChatMessage lastClientMsg &&
                        (chat.Messages.Where(m => m.SenderId != chat.ClientId).OrderByDescending(m => m.SentAt).FirstOrDefault()?.SentAt ?? DateTime.MinValue) < lastClientMsg.SentAt;

                    var viewModel = new ChatItemViewModel(chat, hasUnread);
                    OpenChats.Add(viewModel);
                }

                PendingChatsCount = openChats.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading open chats: {ex.Message}");
            }
        }

        private void RefreshChats()
        {
            LoadOpenChats();
            
            // If a chat is selected, refresh its messages
            if (SelectedChat != null)
            {
                LoadSelectedChatMessages();
            }
        }

        partial void OnSelectedChatChanged(ChatItemViewModel? value)
        {
            if (value != null)
            {
                LoadSelectedChatMessages();
                // Mark as read
                value.HasUnreadMessages = false;
            }
            else
            {
                SelectedChatMessages.Clear();
                NewMessageText = string.Empty;
            }
        }

        private void LoadSelectedChatMessages()
        {
            if (SelectedChat == null) return;

            try
            {
                using var db = new CoreCareDbContext();
                
                var chat = db.Chats
                    .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                    .Include(c => c.Client)
                    .FirstOrDefault(c => c.Id == SelectedChat.Id);

                if (chat == null) return;

                SelectedChatMessages.Clear();

                foreach (var message in chat.Messages.OrderBy(m => m.SentAt))
                {
                    var viewModel = new ChatMessageViewModel
                    {
                        Id = message.Id,
                        SenderName = message.Sender?.username ?? message.Sender?.name ?? "Usuario desconocido",
                        Message = message.Message,
                        SentAt = message.SentAt
                    };

                    SelectedChatMessages.Add(viewModel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chat messages: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SendMessage()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(NewMessageText))
                return;

            try
            {
                using var db = new CoreCareDbContext();

                var message = new ChatMessage
                {
                    ChatId = SelectedChat.Id,
                    SenderId = SessionService.CurrentUser?.Id ?? 0,
                    Message = NewMessageText,
                    SentAt = DateTime.UtcNow
                };

                db.ChatMessages.Add(message);
                db.SaveChanges();

                // Add to UI
                var messageVM = new ChatMessageViewModel
                {
                    Id = message.Id,
                    SenderName = SessionService.CurrentUser?.username ?? SessionService.CurrentUser?.name ?? "Administrador",
                    Message = message.Message,
                    SentAt = message.SentAt
                };

                SelectedChatMessages.Add(messageVM);
                NewMessageText = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CloseSelectedChat()
        {
            if (SelectedChat == null) return;

            try
            {
                using var db = new CoreCareDbContext();

                var chat = db.Chats.FirstOrDefault(c => c.Id == SelectedChat.Id);
                if (chat != null)
                {
                    chat.Status = ChatStatus.Cerrado;
                    chat.ClosedAt = DateTime.UtcNow;
                    db.SaveChanges();
                }

                // Clear selection and reload
                SelectedChat = null;
                LoadOpenChats();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing chat: {ex.Message}");
            }
        }
    }
}
