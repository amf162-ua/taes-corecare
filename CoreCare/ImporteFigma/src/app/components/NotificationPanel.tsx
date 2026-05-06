import { Bell, X, CheckCircle, AlertTriangle, Info, TrendingUp, Clock } from "lucide-react";
import { Button } from "./ui/button";
import { motion, AnimatePresence } from "motion/react";
import { useNavigate } from "react-router";

export interface Notification {
  id: string;
  type: "success" | "warning" | "info";
  title: string;
  message: string;
  timestamp: string;
  link?: string;
  read: boolean;
}

interface NotificationPanelProps {
  notifications: Notification[];
  onMarkAsRead: (id: string) => void;
  onMarkAllAsRead: () => void;
  onClearAll: () => void;
}

export function NotificationPanel({ 
  notifications, 
  onMarkAsRead, 
  onMarkAllAsRead,
  onClearAll 
}: NotificationPanelProps) {
  const navigate = useNavigate();
  const unreadCount = notifications.filter(n => !n.read).length;

  const getIcon = (type: string) => {
    switch (type) {
      case "success":
        return <CheckCircle className="w-5 h-5 text-green-400" />;
      case "warning":
        return <AlertTriangle className="w-5 h-5 text-orange-400" />;
      case "info":
        return <Info className="w-5 h-5 text-cyan-400" />;
      default:
        return <Bell className="w-5 h-5 text-gray-400" />;
    }
  };

  const getColor = (type: string) => {
    switch (type) {
      case "success":
        return {
          bg: "from-green-500/10 to-green-600/10",
          border: "border-green-500/30",
          text: "text-green-400",
        };
      case "warning":
        return {
          bg: "from-orange-500/10 to-orange-600/10",
          border: "border-orange-500/30",
          text: "text-orange-400",
        };
      case "info":
        return {
          bg: "from-cyan-500/10 to-cyan-600/10",
          border: "border-cyan-500/30",
          text: "text-cyan-400",
        };
      default:
        return {
          bg: "from-gray-500/10 to-gray-600/10",
          border: "border-gray-500/30",
          text: "text-gray-400",
        };
    }
  };

  const handleNotificationClick = (notification: Notification) => {
    onMarkAsRead(notification.id);
    if (notification.link) {
      navigate(notification.link);
    }
  };

  return (
    <div className="bg-[#1c2030]/95 backdrop-blur-xl rounded-xl border border-cyan-500/30 shadow-2xl shadow-cyan-500/10 w-96 max-h-[600px] flex flex-col">
      {/* Header */}
      <div className="p-4 border-b border-cyan-500/20">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            <Bell className="w-5 h-5 text-cyan-400" />
            <h3 className="font-bold text-white text-lg">Notificaciones</h3>
            {unreadCount > 0 && (
              <span className="bg-cyan-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                {unreadCount}
              </span>
            )}
          </div>
        </div>
        
        {notifications.length > 0 && (
          <div className="flex gap-2">
            <Button
              onClick={onMarkAllAsRead}
              variant="ghost"
              className="text-xs text-cyan-400 hover:bg-cyan-500/10 h-7 px-3"
            >
              Marcar todas como leídas
            </Button>
            <Button
              onClick={onClearAll}
              variant="ghost"
              className="text-xs text-red-400 hover:bg-red-500/10 h-7 px-3"
            >
              Limpiar todo
            </Button>
          </div>
        )}
      </div>

      {/* Notifications List */}
      <div className="flex-1 overflow-y-auto p-2">
        {notifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <Bell className="w-16 h-16 text-gray-600 mb-4" />
            <p className="text-gray-400 font-normal">No hay notificaciones</p>
            <p className="text-gray-600 text-sm font-normal mt-1">
              Te avisaremos cuando completes un benchmark
            </p>
          </div>
        ) : (
          <div className="space-y-2">
            <AnimatePresence>
              {notifications.map((notification, index) => {
                const colors = getColor(notification.type);
                return (
                  <motion.div
                    key={notification.id}
                    initial={{ x: -20, opacity: 0 }}
                    animate={{ x: 0, opacity: 1 }}
                    exit={{ x: 20, opacity: 0 }}
                    transition={{ duration: 0.3, delay: index * 0.05 }}
                    onClick={() => handleNotificationClick(notification)}
                    className={`relative p-4 rounded-lg border ${colors.border} bg-gradient-to-br ${colors.bg} cursor-pointer hover:border-cyan-400/50 transition-all group ${
                      !notification.read ? "ring-2 ring-cyan-500/20" : ""
                    }`}
                  >
                    {/* Unread indicator */}
                    {!notification.read && (
                      <div className="absolute top-2 right-2 w-2 h-2 bg-cyan-400 rounded-full animate-pulse" />
                    )}

                    <div className="flex items-start gap-3">
                      <div className="flex-shrink-0 mt-0.5">
                        {getIcon(notification.type)}
                      </div>
                      
                      <div className="flex-1 min-w-0">
                        <h4 className={`font-bold text-sm ${colors.text} mb-1`}>
                          {notification.title}
                        </h4>
                        <p className="text-gray-300 text-xs font-normal mb-2 leading-relaxed">
                          {notification.message}
                        </p>
                        <div className="flex items-center gap-2 text-gray-500 text-xs">
                          <Clock className="w-3 h-3" />
                          <span className="font-normal">{notification.timestamp}</span>
                        </div>
                      </div>
                    </div>

                    {notification.link && (
                      <div className="mt-3 pt-3 border-t border-gray-700/50">
                        <div className="flex items-center gap-2 text-cyan-400 text-xs font-bold group-hover:gap-3 transition-all">
                          <TrendingUp className="w-4 h-4" />
                          <span>Ver detalles en historial</span>
                        </div>
                      </div>
                    )}
                  </motion.div>
                );
              })}
            </AnimatePresence>
          </div>
        )}
      </div>
    </div>
  );
}
