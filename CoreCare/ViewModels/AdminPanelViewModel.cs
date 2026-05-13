using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using CoreCare.Data;
using CoreCare.Models;
using CoreCare.Services;

namespace CoreCare.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private Chat _chat;
        private bool _hasUnreadMessages;
        private int _unreadMessagesCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public Chat Chat
        {
            get => _chat;
            set
            {
                if (_chat == value)
                {
                    return;
                }

                _chat = value;
                OnPropertyChanged(nameof(Chat));
            }
        }

        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }
        public bool HasUnreadMessages
        {
            get => _hasUnreadMessages;
            set => SetUnreadState(value ? 1 : 0);
        }

        public int UnreadMessagesCount
        {
            get => _unreadMessagesCount;
            set => SetUnreadState(value);
        }

        public ChatViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetUnreadState(int unreadMessagesCount)
        {
            if (_unreadMessagesCount == unreadMessagesCount)
            {
                return;
            }

            _unreadMessagesCount = Math.Max(0, unreadMessagesCount);
            _hasUnreadMessages = _unreadMessagesCount > 0;
            OnPropertyChanged(nameof(UnreadMessagesCount));
            OnPropertyChanged(nameof(HasUnreadMessages));
        }
    }

    public class ChatMessageViewModel
    {
        public ChatMessage Message { get; set; }
        public string SenderName { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UserViewModel
    {
        public User User { get; set; }
        public string RoleDisplay => User.Role.ToString();
        public string PlanDisplay => User.Plan.ToString();
    }

    public class AdminPanelViewModel : INotifyPropertyChanged
    {
        private CoreCareDbContext _db;
        private DispatcherTimer _pollTimer;
        private string _messageInput;
        private string _selectedRoleFilter = "Todos";
        private string _userSearchFilter = "";
        private ChatViewModel _selectedChat;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ChatViewModel> Chats { get; set; }
        public ObservableCollection<UserViewModel> FilteredUsers { get; set; }

        public ICommand SendMessageCommand { get; private set; }
        public ICommand CloseSelectedChatCommand { get; private set; }
        public ICommand DeleteUserCommand { get; private set; }
        public ICommand ToggleUserRoleCommand { get; private set; }
        public ICommand ToggleUserPlanCommand { get; private set; }
        public ICommand ClearUserFiltersCommand { get; private set; }

        public string MessageInput
        {
            get => _messageInput;
            set
            {
                if (_messageInput == value)
                {
                    return;
                }

                _messageInput = value;
                OnPropertyChanged(nameof(MessageInput));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string UserSearchFilter
        {
            get => _userSearchFilter;
            set
            {
                if (_userSearchFilter == value)
                {
                    return;
                }

                _userSearchFilter = value;
                RefreshUserList();
                OnPropertyChanged(nameof(UserSearchFilter));
            }
        }

        public string SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                if (_selectedRoleFilter == value)
                {
                    return;
                }

                _selectedRoleFilter = string.IsNullOrWhiteSpace(value) ? "Todos" : value;
                RefreshUserList();
                OnPropertyChanged(nameof(SelectedRoleFilter));
            }
        }

        public int TotalFilteredUsers => FilteredUsers.Count;

        public ChatViewModel SelectedChat
        {
            get => _selectedChat;
            set
            {
                try
                {
                    if (_selectedChat == value)
                    {
                        return;
                    }

                    _selectedChat = value;
                    OnPropertyChanged(nameof(SelectedChat));
                    CommandManager.InvalidateRequerySuggested();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR in SelectedChat setter: {ex}");
                    Console.WriteLine($"ERROR in SelectedChat setter: {ex}");
                }
            }
        }

        public AdminPanelViewModel()
        {
            _db = new CoreCareDbContext();
            Chats = new ObservableCollection<ChatViewModel>();
            FilteredUsers = new ObservableCollection<UserViewModel>();

            InitializeCommands();
            LoadChats();
            LoadUsers();
            StartPolling();
        }

        private void InitializeCommands()
        {
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
            CloseSelectedChatCommand = new RelayCommand(CloseSelectedChat, CanCloseChat);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            ToggleUserRoleCommand = new RelayCommand(ToggleUserRole, CanModifyUser);
            ToggleUserPlanCommand = new RelayCommand(ToggleUserPlan, CanModifyUser);
            ClearUserFiltersCommand = new RelayCommand(ClearUserFilters);
        }

        private void LoadChats()
        {
            try
            {
                var selectedChatId = SelectedChat?.Chat?.Id;
                var selectedChatVm = SelectedChat;

                Chats.Clear();
                
                // Recreate context to prevent connection issues
                _db?.Dispose();
                _db = new CoreCareDbContext();
                
                // Single query with all eager loading to prevent null references
                var openChats = _db.Chats
                    .AsNoTracking()
                    .Include(c => c.Client)
                    .Include(c => c.Messages)
                        .ThenInclude(m => m.Sender)
                    .Where(c => c.Status == ChatStatus.Abierto)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                foreach (var chat in openChats)
                {
                    try
                    {
                        var chatVm = selectedChatVm != null && chat.Id == selectedChatId
                            ? selectedChatVm
                            : new ChatViewModel();

                        chatVm.Chat = chat;
                        chatVm.UnreadMessagesCount = CountUnreadMessages(chat.Messages?.ToList() ?? new List<ChatMessage>());
                        chatVm.Messages.Clear();

                        // Use the already-loaded Messages from the chat
                        if (chat.Messages != null)
                        {
                            foreach (var msg in chat.Messages.OrderBy(m => m.SentAt))
                            {
                                try
                                {
                                    chatVm.Messages.Add(new ChatMessageViewModel
                                    {
                                        Message = msg,
                                        SenderName = msg.Sender?.name ?? "Invitado",
                                        IsAdmin = msg.Sender?.Role == UserRole.Administrador
                                    });
                                }
                                catch (Exception msgEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"ERROR adding message to VM: {msgEx}");
                                }
                            }
                        }

                        Chats.Add(chatVm);
                    }
                    catch (Exception chatEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR processing chat: {chatEx}");
                    }
                }

                if (selectedChatId.HasValue)
                {
                    var refreshedSelectedChat = Chats.FirstOrDefault(chat => chat.Chat.Id == selectedChatId.Value);
                    if (!ReferenceEquals(SelectedChat, refreshedSelectedChat))
                    {
                        SelectedChat = refreshedSelectedChat;
                    }
                    else
                    {
                        OnPropertyChanged(nameof(SelectedChat));
                        CommandManager.InvalidateRequerySuggested();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LoadChats: {ex}");
                MessageBox.Show($"Error al cargar chats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CountUnreadMessages(List<ChatMessage> messages)
        {
            if (messages.Count == 0) return 0;

            var lastAdminReplyAt = messages
                .Where(message => message.Sender?.Role == UserRole.Administrador)
                .OrderByDescending(message => message.SentAt)
                .Select(message => message.SentAt)
                .FirstOrDefault();

            if (lastAdminReplyAt == default)
            {
                return messages.Count(message => message.Sender?.Role == UserRole.Cliente);
            }

            return messages.Count(message =>
                message.Sender?.Role == UserRole.Cliente &&
                message.SentAt > lastAdminReplyAt);
        }

        private void LoadUsers()
        {
            try
            {
                RefreshUserList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private void RefreshUserList()
        {
            try
            {
                FilteredUsers.Clear();

                var query = _db.Users.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(UserSearchFilter))
                {
                    var searchTerm = UserSearchFilter.ToLower();
                    query = query.Where(u =>
                        u.username.ToLower().Contains(searchTerm) ||
                        u.name.ToLower().Contains(searchTerm) ||
                        u.email.ToLower().Contains(searchTerm)
                    );
                }

                // Apply role filter
                if (SelectedRoleFilter == "Cliente")
                {
                    query = query.Where(u => u.Role == UserRole.Cliente);
                }
                else if (SelectedRoleFilter == "Administrador")
                {
                    query = query.Where(u => u.Role == UserRole.Administrador);
                }

                var users = query.OrderBy(u => u.username).ToList();

                foreach (var user in users)
                {
                    FilteredUsers.Add(new UserViewModel { User = user });
                }

                OnPropertyChanged(nameof(TotalFilteredUsers));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing user list: {ex.Message}");
            }
        }

        private void SendMessage(object parameter)
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(MessageInput))
                return;

            try
            {
                var currentChatId = SelectedChat.Chat.Id;
                var messageText = MessageInput;

                // Use a fresh context for this operation
                using (var freshDb = new CoreCareDbContext())
                {
                    var chat = freshDb.Chats.FirstOrDefault(c => c.Id == currentChatId);
                    if (chat == null)
                    {
                        MessageBox.Show("El chat no existe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var currentUser = SessionService.CurrentUser;
                    if (currentUser == null)
                    {
                        MessageBox.Show("Sesión expirada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newMessage = new ChatMessage
                    {
                        ChatId = chat.Id,
                        SenderId = currentUser.Id,
                        Message = messageText,
                        SentAt = DateTime.UtcNow
                    };

                    freshDb.ChatMessages.Add(newMessage);
                    freshDb.SaveChanges();
                }

                MessageInput = "";
                LoadChats();
                RefreshChatSelection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR sending message: {ex}");
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSendMessage(object parameter)
        {
            return SelectedChat != null && !string.IsNullOrWhiteSpace(MessageInput);
        }

        private void CloseSelectedChat(object parameter)
        {
            if (SelectedChat == null) return;

            try
            {
                var chat = _db.Chats.FirstOrDefault(c => c.Id == SelectedChat.Chat.Id);
                if (chat != null)
                {
                    chat.Status = ChatStatus.Cerrado;
                    chat.ClosedAt = DateTime.UtcNow;
                    _db.SaveChanges();
                    LoadChats();
                    SelectedChat = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing chat: {ex.Message}");
            }
        }

        private bool CanCloseChat(object parameter)
        {
            return SelectedChat != null;
        }

        private void DeleteUser(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.Id == userVm.User.Id);
                    if (user != null)
                    {
                        _db.Users.Remove(user);
                        _db.SaveChanges();
                        RefreshUserList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting user: {ex.Message}");
                }
            }
        }

        private bool CanDeleteUser(object parameter)
        {
            return parameter is UserViewModel;
        }

        private void ToggleUserRole(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.Id == userVm.User.Id);
                    if (user != null)
                    {
                        user.Role = user.Role == UserRole.Administrador ? UserRole.Cliente : UserRole.Administrador;
                        _db.SaveChanges();
                        RefreshUserList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error toggling user role: {ex.Message}");
                }
            }
        }

        private bool CanModifyUser(object parameter)
        {
            return parameter is UserViewModel;
        }

        private void ToggleUserPlan(object parameter)
        {
            if (parameter is UserViewModel userVm)
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.Id == userVm.User.Id);
                    if (user != null)
                    {
                        user.Plan = user.Plan == TipoPlan.Basico ? TipoPlan.Premium : TipoPlan.Basico;
                        _db.SaveChanges();
                        RefreshUserList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error toggling user plan: {ex.Message}");
                }
            }
        }

        private void ClearUserFilters(object parameter)
        {
            UserSearchFilter = "";
            SelectedRoleFilter = "Todos";
            RefreshUserList();
        }

        private void StartPolling()
        {
            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = TimeSpan.FromSeconds(3);
            _pollTimer.Tick += (s, e) =>
            {
                try
                {
                    LoadChats();
                    RefreshChatSelection();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR in polling timer: {ex}");
                    Console.WriteLine($"Error in polling timer: {ex.Message}");
                }
            };
            _pollTimer.Start();
        }

        private void RefreshChatSelection()
        {
            try
            {
                if (SelectedChat != null)
                {
                    // Find the updated chat with the same ID
                    var updatedChat = Chats.FirstOrDefault(c => c.Chat.Id == SelectedChat.Chat.Id);
                    if (updatedChat != null && updatedChat != SelectedChat)
                    {
                        // Only update if it's a different instance
                        SelectedChat = updatedChat;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in RefreshChatSelection: {ex}");
                Console.WriteLine($"Error in RefreshChatSelection: {ex.Message}");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _pollTimer?.Stop();
            _db?.Dispose();
        }
    }
}
