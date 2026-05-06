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
        public bool IsOutgoing { get; set; }
    }

    public partial class UserItemViewModel : ObservableObject
    {
        private readonly User _user;

        public int Id => _user.Id;
        public string Name => _user.name;
        public string Username => _user.username;
        public string Email => _user.email;

        [ObservableProperty]
        private UserRole _role;

        [ObservableProperty]
        private TipoPlan _plan;

        [ObservableProperty]
        private bool _isActive;

        public DateTime CreatedAt => _user.createdAt;

        public UserItemViewModel(User user)
        {
            _user = user;
            _role = user.Role;
            _plan = user.Plan;
            _isActive = user.IsActive;
        }

        public User GetUser() => _user;
    }

    public partial class AdminPanelViewModel : ObservableObject
    {
        private readonly DispatcherTimer _refreshTimer;
        private int? _selectedChatId;
        private bool _isRefreshingChats;

        [ObservableProperty]
        private string _adminUserLabel = "Usuario: -";

        [ObservableProperty]
        private ObservableCollection<ChatItemViewModel> _openChats = new();

        [ObservableProperty]
        private ChatItemViewModel? _selectedChat;

        [ObservableProperty]
        private bool _isDetailPaneOpen = false;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> _selectedChatMessages = new();

        [ObservableProperty]
        private string _newMessageText = string.Empty;

        [ObservableProperty]
        private int _pendingChatsCount = 0;

        // User Management Properties
        [ObservableProperty]
        private ObservableCollection<UserItemViewModel> _allUsers = new();

        [ObservableProperty]
        private ObservableCollection<UserItemViewModel> _filteredUsers = new();

        [ObservableProperty]
        private string _userSearchText = string.Empty;

        [ObservableProperty]
        private UserRole? _roleFilter = null;

        [ObservableProperty]
        private UserItemViewModel? _selectedUser;

        public bool HasActiveChat => _selectedChatId.HasValue;

        public AdminPanelViewModel()
        {
            UpdateAdminLabel();
            LoadOpenChats();
            LoadAllUsers();

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

                if (_selectedChatId.HasValue)
                {
                    SelectedChat = OpenChats.FirstOrDefault(chat => chat.Id == _selectedChatId.Value);
                    if (SelectedChat != null)
                    {
                        LoadSelectedChatMessages();
                    }
                }

                OnPropertyChanged(nameof(HasActiveChat));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading open chats: {ex.Message}");
            }
        }

        private void RefreshChats()
        {
            if (SelectedChat != null)
            {
                _selectedChatId = SelectedChat.Id;
            }

            _isRefreshingChats = true;
            try
            {
                LoadOpenChats();

                // If a chat is selected, refresh its messages
                if (SelectedChat != null)
                {
                    LoadSelectedChatMessages();
                }
            }
            finally
            {
                _isRefreshingChats = false;
            }
        }

        partial void OnSelectedChatChanged(ChatItemViewModel? value)
        {
            if (value == null && _isRefreshingChats && _selectedChatId.HasValue)
            {
                return;
            }

            _selectedChatId = value?.Id;
            OnPropertyChanged(nameof(HasActiveChat));

            if (value != null)
            {
                IsDetailPaneOpen = true;
                LoadSelectedChatMessages();
                // Mark as read
                value.HasUnreadMessages = false;
            }
            else
            {
                IsDetailPaneOpen = false;
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
                        SentAt = message.SentAt,
                        IsOutgoing = message.SenderId == SessionService.CurrentUser?.Id
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
                    SentAt = message.SentAt,
                    IsOutgoing = true
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

        // ===== USER MANAGEMENT =====

        private void LoadAllUsers()
        {
            try
            {
                using var db = new CoreCareDbContext();
                var users = db.Users.OrderBy(u => u.username).ToList();

                AllUsers.Clear();
                foreach (var user in users)
                {
                    AllUsers.Add(new UserItemViewModel(user));
                }

                ApplyUserFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        partial void OnUserSearchTextChanged(string value)
        {
            ApplyUserFilters();
        }

        partial void OnRoleFilterChanged(UserRole? value)
        {
            ApplyUserFilters();
        }

        private void ApplyUserFilters()
        {
            var filtered = AllUsers.AsEnumerable();

            // Filter by username or email
            if (!string.IsNullOrWhiteSpace(UserSearchText))
            {
                var search = UserSearchText.ToLower();
                filtered = filtered.Where(u => 
                    u.Username.ToLower().Contains(search) || 
                    u.Email.ToLower().Contains(search) ||
                    u.Name.ToLower().Contains(search));
            }

            // Filter by role
            if (RoleFilter.HasValue)
            {
                filtered = filtered.Where(u => u.Role == RoleFilter.Value);
            }

            FilteredUsers.Clear();
            foreach (var user in filtered)
            {
                FilteredUsers.Add(user);
            }
        }

        [RelayCommand]
        private void DeleteUser(UserItemViewModel? user)
        {
            if (user == null) return;

            try
            {
                using var db = new CoreCareDbContext();
                var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser != null)
                {
                    db.Users.Remove(dbUser);
                    db.SaveChanges();
                    LoadAllUsers();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleUserRole(UserItemViewModel? user)
        {
            if (user == null) return;

            try
            {
                using var db = new CoreCareDbContext();
                var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser != null)
                {
                    dbUser.Role = dbUser.Role == UserRole.Cliente ? UserRole.Administrador : UserRole.Cliente;
                    db.SaveChanges();
                    user.Role = dbUser.Role;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling user role: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleUserPlan(UserItemViewModel? user)
        {
            if (user == null) return;

            try
            {
                using var db = new CoreCareDbContext();
                var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser != null)
                {
                    dbUser.Plan = dbUser.Plan == TipoPlan.Basico ? TipoPlan.Premium : TipoPlan.Basico;
                    db.SaveChanges();
                    user.Plan = dbUser.Plan;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling user plan: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ClearUserFilters()
        {
            UserSearchText = string.Empty;
            RoleFilter = null;
        }
    }
}
