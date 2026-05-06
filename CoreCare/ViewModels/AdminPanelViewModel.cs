using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using CoreCare.Data;
using CoreCare.Models;
using CoreCare.Services;

namespace CoreCare.ViewModels
{
    public class ChatViewModel
    {
        public Chat Chat { get; set; }
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }
        public bool HasUnreadMessages { get; set; }

        public ChatViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();
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
                if (_selectedChat == value)
                {
                    return;
                }

                _selectedChat = value;
                OnPropertyChanged(nameof(SelectedChat));
                CommandManager.InvalidateRequerySuggested();
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
                Chats.Clear();
                var openChats = _db.Chats
                    .Include(c => c.Client)
                    .Where(c => c.Status == ChatStatus.Abierto)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                foreach (var chat in openChats)
                {
                    var messages = _db.ChatMessages
                        .Include(m => m.Sender)
                        .Where(m => m.ChatId == chat.Id)
                        .OrderBy(m => m.SentAt)
                        .ToList();

                    var chatVm = new ChatViewModel
                    {
                        Chat = chat,
                        HasUnreadMessages = DetectUnreadMessages(chat, messages)
                    };

                    foreach (var msg in messages)
                    {
                        chatVm.Messages.Add(new ChatMessageViewModel
                        {
                            Message = msg,
                            SenderName = msg.Sender?.name ?? "Unknown",
                            IsAdmin = msg.Sender?.Role == UserRole.Administrador
                        });
                    }

                    Chats.Add(chatVm);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading chats: {ex.Message}");
            }
        }

        private bool DetectUnreadMessages(Chat chat, List<ChatMessage> messages)
        {
            if (messages.Count == 0)
            {
                return false;
            }

            var lastClientMessage = messages
                .Where(message => message.Sender?.Role == UserRole.Cliente)
                .OrderByDescending(message => message.SentAt)
                .FirstOrDefault();

            var lastAdminMessage = messages
                .Where(message => message.Sender?.Role == UserRole.Administrador)
                .OrderByDescending(message => message.SentAt)
                .FirstOrDefault();

            if (lastClientMessage == null)
            {
                return false;
            }

            if (lastAdminMessage == null)
            {
                return true;
            }

            return lastClientMessage.SentAt > lastAdminMessage.SentAt;
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
                var chat = _db.Chats.FirstOrDefault(c => c.Id == SelectedChat.Chat.Id);
                if (chat == null) return;

                var currentUser = SessionService.CurrentUser;

                var newMessage = new ChatMessage
                {
                    ChatId = chat.Id,
                    SenderId = currentUser.Id,
                    Message = MessageInput,
                    SentAt = DateTime.UtcNow
                };

                _db.ChatMessages.Add(newMessage);
                _db.SaveChanges();

                MessageInput = "";
                LoadChats();
                RefreshChatSelection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
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
                LoadChats();
                RefreshChatSelection();
            };
            _pollTimer.Start();
        }

        private void RefreshChatSelection()
        {
            if (SelectedChat != null)
            {
                var updatedChat = Chats.FirstOrDefault(c => c.Chat.Id == SelectedChat.Chat.Id);
                if (updatedChat != null)
                {
                    SelectedChat = updatedChat;
                }
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

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}
