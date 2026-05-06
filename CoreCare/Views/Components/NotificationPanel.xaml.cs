using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;

namespace CoreCare.Views.Components
{
    public partial class NotificationPanel : UserControl
    {
        private List<AppNotification> _notifications;

        public NotificationPanel()
        {
            InitializeComponent();
            LoadDummyNotifications();
        }

        private void LoadDummyNotifications()
        {
            _notifications = new List<AppNotification>
            {
                new AppNotification { Type = "success", Title = "Benchmark Completado", Message = "El test de CPU ha finalizado con éxito.", IsRead = false },
                new AppNotification { Type = "warning", Title = "Temperatura Alta", Message = "Se han detectado picos de 85°C durante la prueba.", IsRead = false },
                new AppNotification { Type = "info", Title = "Actualización", Message = "Hay nuevos drivers disponibles para tu GPU.", IsRead = true }
            };
            RefreshUI();
        }

        private void RefreshUI()
        {
            NotificationsList.ItemsSource = null;
            NotificationsList.ItemsSource = _notifications;

            int unreadCount = _notifications.Count(n => !n.IsRead);
            TxtUnreadCount.Text = unreadCount.ToString();
            UnreadBadge.Visibility = unreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var note = btn?.DataContext as AppNotification;
            if (note != null)
            {
                note.IsRead = true;
                RefreshUI();
                // Aquí podrías navegar a la página de historial si tiene link
            }
        }

        private void BtnReadAll_Click(object sender, RoutedEventArgs e)
        {
            _notifications.ForEach(n => n.IsRead = true);
            RefreshUI();
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            _notifications.Clear();
            RefreshUI();
        }
    }
}