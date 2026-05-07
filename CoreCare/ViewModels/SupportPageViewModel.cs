using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using CoreCare.Data;
using CoreCare.Models;
using CoreCare.Services;

namespace CoreCare.ViewModels
{
    public class SupportPageViewModel : INotifyPropertyChanged
    {
        private CoreCareDbContext _db;
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
    }

    private void LoadUserChats()
        {
            try
            {
                UserChats.Clear();
                var currentUser = SessionService.CurrentUser;
                if (currentUser == null) return;

                var userChats = _db.Chats
                    .Include(c => c.Client)
                    .Include(c => c.Messages)
                        .ThenInclude(m => m.Sender)
                    .Where(c => c.ClientId == currentUser.Id && c.Status == ChatStatus.Abierto)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                foreach (var chat in userChats)
                {
                    var chatVm = new ChatViewModel
                    {
                        Chat = chat,
                        HasUnreadMessages = DetectUnreadMessages(chat, chat.Messages?.ToList() ?? new List<ChatMessage>())
                    };

                    if (chat.Messages != null)
                    {
                        foreach (var msg in chat.Messages.OrderBy(m => m.SentAt))
                        {
                            chatVm.Messages.Add(new ChatMessageViewModel
                            {
                                Message = msg,
                                SenderName = msg.Sender?.name ?? "Unknown",
                                IsAdmin = msg.Sender?.Role == UserRole.Administrador
                            });
                        }
                    }

                    UserChats.Add(chatVm);
                }

                OnPropertyChanged(nameof(HasNoChats));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR loading user chats: {ex}");
                Console.WriteLine($"Error loading user chats: {ex.Message}");
            }
        }

        private bool DetectUnreadMessages(Chat chat, List<ChatMessage> messages)
        {
            if (messages.Count == 0) return false;

            var lastClientMessage = messages
                .Where(m => m.Sender?.Role == UserRole.Cliente)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var lastAdminMessage = messages
                .Where(m => m.Sender?.Role == UserRole.Administrador)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            if (lastClientMessage == null) return false;
            if (lastAdminMessage == null) return true;

            return lastClientMessage.SentAt > lastAdminMessage.SentAt;
        }

        private void CreateChat(object parameter)
        {
            if (string.IsNullOrWhiteSpace(ChatSubject) || string.IsNullOrWhiteSpace(ChatMessage))
                return;

            try
            {
                var currentUser = SessionService.CurrentUser;
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

                var currentUser = SessionService.CurrentUser;

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
    }
}
