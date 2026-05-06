import { Crown, User, LogOut, Home, Activity, History, Bell, Cpu, FileText } from "lucide-react";
import { Button } from "./ui/button";
import logo from "figma:asset/ff462b39e40c4c3fb04568a1948b4399bbf34bc1.png";
import { motion, AnimatePresence } from "motion/react";
import { Link, useLocation } from "react-router";
import { useState, useRef, useEffect } from "react";
import { NotificationPanel, Notification } from "./NotificationPanel";

interface HeaderProps {
  onLoginClick: () => void;
  onPremiumClick: () => void;
  onProfileClick: () => void;
  user: { name: string; isPremium: boolean } | null;
  onLogout: () => void;
  notifications: Notification[];
  onMarkAsRead: (id: string) => void;
  onMarkAllAsRead: () => void;
  onClearNotifications: () => void;
}

export function Header({ 
  onLoginClick, 
  onPremiumClick,
  onProfileClick,
  user, 
  onLogout,
  notifications = [],
  onMarkAsRead,
  onMarkAllAsRead,
  onClearNotifications
}: HeaderProps) {
  const location = useLocation();
  const [showNotifications, setShowNotifications] = useState(false);
  const notificationRef = useRef<HTMLDivElement>(null);
  const unreadCount = notifications?.filter(n => !n.read).length || 0;

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  // Close notifications when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (notificationRef.current && !notificationRef.current.contains(event.target as Node)) {
        setShowNotifications(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);
  
  return (
    <header className="border-b border-cyan-500/20 bg-[#0a0e1a]/95 backdrop-blur-xl sticky top-0 z-50">
      <div className="absolute inset-0 bg-gradient-to-r from-orange-500/5 via-transparent to-cyan-500/5" />
      <div className="w-full px-4 py-3 relative">
        <div className="flex items-center justify-between gap-4">
          <motion.div 
            className="flex items-center gap-3 flex-shrink-0"
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.5 }}
          >
            <Link to="/" className="flex items-center gap-3 hover:opacity-80 transition-opacity">
              <img src={logo} alt="Core Care" className="h-12" />
              <div className="hidden sm:block">
                <h1 className="text-xl font-black text-white tracking-[3px]">
                  CORE CARE
                </h1>
                <p className="text-[10px] text-cyan-400/70 tracking-[2px] font-normal">
                  AXIOM WORKS
                </p>
              </div>
            </Link>
          </motion.div>

          {/* Navigation - Solo visible cuando hay usuario */}
          {user && (
            <motion.nav 
              className="flex items-center gap-1 flex-shrink mx-auto"
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: 0.1 }}
            >
              <Link to="/">
                <Button
                  variant="ghost"
                  size="sm"
                  className={`flex items-center gap-1.5 px-3 py-2 text-xs transition-all duration-300 ${
                    isActive('/') 
                      ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/50' 
                      : 'text-gray-400 hover:text-white hover:bg-cyan-500/10'
                  }`}
                >
                  <Home className="w-3.5 h-3.5" />
                  <span className="hidden md:inline">INICIO</span>
                </Button>
              </Link>
              
              <Link to="/benchmarks">
                <Button
                  variant="ghost"
                  size="sm"
                  className={`flex items-center gap-1.5 px-3 py-2 text-xs transition-all duration-300 ${
                    isActive('/benchmarks') 
                      ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/50' 
                      : 'text-gray-400 hover:text-white hover:bg-cyan-500/10'
                  }`}
                >
                  <Activity className="w-3.5 h-3.5" />
                  <span className="hidden md:inline">BENCHMARKS</span>
                </Button>
              </Link>

              <Link to="/history">
                <Button
                  variant="ghost"
                  size="sm"
                  className={`flex items-center gap-1.5 px-3 py-2 text-xs transition-all duration-300 ${
                    isActive('/history') 
                      ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/50' 
                      : 'text-gray-400 hover:text-white hover:bg-cyan-500/10'
                  }`}
                >
                  <History className="w-3.5 h-3.5" />
                  <span className="hidden md:inline">HISTORIAL</span>
                </Button>
              </Link>

              <Link to="/processes">
                <Button
                  variant="ghost"
                  size="sm"
                  className={`flex items-center gap-1.5 px-3 py-2 text-xs transition-all duration-300 ${
                    isActive('/processes') 
                      ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/50' 
                      : 'text-gray-400 hover:text-white hover:bg-cyan-500/10'
                  }`}
                >
                  <Cpu className="w-3.5 h-3.5" />
                  <span className="hidden md:inline">PROCESOS</span>
                </Button>
              </Link>

              <Link to="/reports">
                <Button
                  variant="ghost"
                  size="sm"
                  className={`flex items-center gap-1.5 px-3 py-2 text-xs transition-all duration-300 ${
                    isActive('/reports') 
                      ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/50' 
                      : 'text-gray-400 hover:text-white hover:bg-cyan-500/10'
                  }`}
                >
                  <FileText className="w-3.5 h-3.5" />
                  <span className="hidden md:inline">REPORTES</span>
                </Button>
              </Link>
            </motion.nav>
          )}

          <motion.div
            className="flex items-center gap-2 flex-shrink-0"
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.5 }}
          >
            {user && (
              <>
                {/* Notifications Button */}
                <div className="relative" ref={notificationRef}>
                  <Button
                    onClick={() => setShowNotifications(!showNotifications)}
                    variant="ghost"
                    size="sm"
                    className="relative text-gray-400 hover:text-white hover:bg-cyan-500/10 p-2"
                  >
                    <Bell className="w-4 h-4" />
                    {unreadCount > 0 && (
                      <span className="absolute -top-1 -right-1 w-4 h-4 bg-cyan-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center animate-pulse">
                        {unreadCount > 9 ? "9+" : unreadCount}
                      </span>
                    )}
                  </Button>

                  {/* Notification Panel */}
                  <AnimatePresence>
                    {showNotifications && (
                      <motion.div
                        initial={{ opacity: 0, y: -10, scale: 0.95 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        exit={{ opacity: 0, y: -10, scale: 0.95 }}
                        transition={{ duration: 0.2 }}
                        className="absolute right-0 top-full mt-2 z-50"
                      >
                        <NotificationPanel
                          notifications={notifications}
                          onMarkAsRead={onMarkAsRead}
                          onMarkAllAsRead={onMarkAllAsRead}
                          onClearAll={onClearNotifications}
                        />
                      </motion.div>
                    )}
                  </AnimatePresence>
                </div>

                {!user.isPremium && (
                  <Button
                    onClick={onPremiumClick}
                    size="sm"
                    className="border-2 border-amber-500/50 bg-gradient-to-r from-amber-500/10 to-orange-500/10 text-amber-400 hover:bg-amber-500/20 hover:border-amber-400 flex items-center gap-1.5 transition-all duration-300 px-3 py-2 text-xs"
                  >
                    <Crown className="w-3.5 h-3.5" />
                    <span className="hidden sm:inline">PREMIUM</span>
                  </Button>
                )}

                <div className="flex items-center gap-2">
                  <button 
                    onClick={onProfileClick}
                    className="hidden lg:flex items-center gap-2 px-3 py-1.5 bg-gradient-to-r from-cyan-500/10 to-blue-500/10 rounded-lg border border-cyan-500/30 hover:bg-cyan-500/20 hover:border-cyan-500/50 transition-all duration-300 cursor-pointer"
                  >
                    <User className="w-3.5 h-3.5 text-cyan-400" />
                    <span className="text-xs font-medium text-white">
                      {user.name}
                    </span>
                    {user.isPremium && (
                      <Crown className="w-3.5 h-3.5 text-amber-400" />
                    )}
                  </button>
                  <Button 
                    onClick={onLogout} 
                    variant="ghost" 
                    size="sm"
                    className="text-gray-400 hover:text-white hover:bg-red-500/10 p-2"
                  >
                    <LogOut className="w-4 h-4" />
                  </Button>
                </div>
              </>
            )}

            {!user && (
              <Button 
                onClick={onLoginClick}
                size="sm"
                className="bg-gradient-to-r from-cyan-600 to-cyan-700 hover:from-cyan-500 hover:to-cyan-600 text-white border border-cyan-500/50 shadow-lg shadow-cyan-500/20 px-4 text-xs"
              >
                INICIAR SESIÓN
              </Button>
            )}
          </motion.div>
        </div>
      </div>
    </header>
  );
}