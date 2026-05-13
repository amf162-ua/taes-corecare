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
    public class SupportPageViewModel : INotifyPropertyChanged
    {
        private const string GuestUserName = "invitado";
        private const string GuestDisplayName = "Invitado";
        private CoreCareDbContext _db;
        private DispatcherTimer _pollTimer;
        private string _chatSubject;
        private string _chatMessage;
        private string _clientMessageInput;
        private ChatViewModel _selectedChat;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ChatViewModel> UserChats { get; set; }
        public ICommand CreateChatCommand { get; private set; }
        public ICommand SendClientMessageCommand { get; private set; }

    public string ChatSubject
    {
        get => _chatSubject;
        set
        {
            if (_chatSubject == value) return;
            _chatSubject = value;
            OnPropertyChanged(nameof(ChatSubject));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ChatMessage
    {
        get => _chatMessage;
        set
        {
            if (_chatMessage == value) return;
            _chatMessage = value;
            OnPropertyChanged(nameof(ChatMessage));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string ClientMessageInput
    {
        get => _clientMessageInput;
        set
        {
            if (_clientMessageInput == value) return;
            _clientMessageInput = value;
            OnPropertyChanged(nameof(ClientMessageInput));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ChatViewModel SelectedChat
    {
        get => _selectedChat;
        set
        {
            if (_selectedChat == value) return;
            _selectedChat = value;
            if (_selectedChat != null)
            {
                _selectedChat.UnreadMessagesCount = 0;
            }
            OnPropertyChanged(nameof(SelectedChat));
        }
    }

    public bool HasNoChats => UserChats.Count == 0;

    public SupportPageViewModel()
    {
        _db = new CoreCareDbContext();
        UserChats = new ObservableCollection<ChatViewModel>();
        CreateChatCommand = new RelayCommand(CreateChat, CanCreateChat);
        SendClientMessageCommand = new RelayCommand(SendClientMessage, CanSendClientMessage);
        LoadUserChats();
        StartPolling();
    }

    private void LoadUserChats()
    {
        try
        {
            var selectedChatId = SelectedChat?.Chat?.Id;
            var selectedChatVm = SelectedChat;

            UserChats.Clear();
            var currentUser = GetSupportUser();
            if (currentUser == null) return;

            var userChats = _db.Chats
                .AsNoTracking()
                .Include(c => c.Client)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Where(c => c.ClientId == currentUser.Id && c.Status == ChatStatus.Abierto)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            foreach (var chat in userChats)
            {
                var chatVm = selectedChatVm != null && chat.Id == selectedChatId
                    ? selectedChatVm
                    : new ChatViewModel();

                chatVm.Chat = chat;
                chatVm.Messages.Clear();
                chatVm.UnreadMessagesCount = chat.Id == selectedChatId
                    ? 0
                    : CountUnreadAdminReplies(chat.Messages?.ToList() ?? new List<ChatMessage>());

                if (chat.Messages != null)
                {
                    foreach (var msg in chat.Messages.OrderBy(m => m.SentAt))
                    {
                        chatVm.Messages.Add(new ChatMessageViewModel
                        {
                            Message = msg,
                            SenderName = msg.Sender?.name ?? GuestDisplayName,
                            IsAdmin = msg.Sender?.Role == UserRole.Administrador
                        });
                    }
                }

                UserChats.Add(chatVm);
            }

            if (selectedChatId.HasValue)
            {
                SelectedChat = UserChats.FirstOrDefault(chat => chat.Chat.Id == selectedChatId.Value);
            }

            OnPropertyChanged(nameof(HasNoChats));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR loading user chats: {ex}");
            Console.WriteLine($"Error loading user chats: {ex.Message}");
        }
    }

    private int CountUnreadAdminReplies(List<ChatMessage> messages)
    {
        if (messages.Count == 0)
        {
            return 0;
        }

        var lastClientMessageTime = messages
            .Where(message => message.Sender?.Role == UserRole.Cliente)
            .Select(message => message.SentAt)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        return messages.Count(message =>
            message.Sender?.Role == UserRole.Administrador &&
            message.SentAt > lastClientMessageTime);
    }

        private void CreateChat(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ChatSubject) || string.IsNullOrWhiteSpace(ChatMessage))
                return;

            try
            {
                var currentUser = GetSupportUser();
                if (currentUser == null) return;

                var newChat = new Chat
                {
                    ClientId = currentUser.Id,
                    Subject = ChatSubject,
                    Status = ChatStatus.Abierto,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Chats.Add(newChat);
                _db.SaveChanges();

                // Add the initial message
                var initialMessage = new ChatMessage
                {
                    ChatId = newChat.Id,
                    SenderId = currentUser.Id,
                    Message = ChatMessage,
                    SentAt = DateTime.UtcNow
                };

                _db.ChatMessages.Add(initialMessage);
                _db.SaveChanges();

                // Clear inputs
                ChatSubject = "";
                ChatMessage = "";

                // Reload chats
                LoadUserChats();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR creating chat: {ex}");
                Console.WriteLine($"Error creating chat: {ex.Message}");
            }
        }

        private bool CanCreateChat(object parameter)
        {
            return !string.IsNullOrWhiteSpace(ChatSubject) && !string.IsNullOrWhiteSpace(ChatMessage);
        }

        private void SendClientMessage(object parameter)
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(ClientMessageInput))
                return;

            try
            {
                var chat = _db.Chats.FirstOrDefault(c => c.Id == SelectedChat.Chat.Id);
                if (chat == null) return;

                var currentUser = GetSupportUser();
                if (currentUser == null) return;

                var newMessage = new ChatMessage
                {
                    ChatId = chat.Id,
                    SenderId = currentUser.Id,
                    Message = ClientMessageInput,
                    SentAt = DateTime.UtcNow
                };

                _db.ChatMessages.Add(newMessage);
                _db.SaveChanges();

                // Clear input
                ClientMessageInput = "";

                // Reload the specific chat to show new message
                LoadUserChats();
                SelectedChat = UserChats.FirstOrDefault(c => c.Chat.Id == chat.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR sending client message: {ex}");
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private bool CanSendClientMessage(object parameter)
        {
            return SelectedChat != null && !string.IsNullOrWhiteSpace(ClientMessageInput);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StartPolling()
        {
            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            _pollTimer.Tick += (sender, args) =>
            {
                try
                {
                    LoadUserChats();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR in support polling: {ex}");
                }
            };

            _pollTimer.Start();
        }

        private User GetSupportUser()
        {
            var currentUser = SessionService.CurrentUser;
            if (currentUser != null)
            {
                return currentUser;
            }

            return EnsureGuestUser();
        }

        private User EnsureGuestUser()
        {
            var guestUser = _db.Users.FirstOrDefault(user =>
                user.username == GuestUserName ||
                user.email == "invitado@corecare.local" ||
                user.name == GuestDisplayName);

            if (guestUser != null)
            {
                return guestUser;
            }

            guestUser = new User
            {
                name = GuestDisplayName,
                username = GuestUserName,
                email = "invitado@corecare.local",
                password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), workFactor: 12),
                Role = UserRole.Cliente,
                createdAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Users.Add(guestUser);
            _db.SaveChanges();

            return guestUser;
        }
    }
}
