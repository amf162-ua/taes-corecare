import { Outlet } from "react-router";
import { useState } from "react";
import { Header } from "../components/Header";
import { LoginModal } from "../components/LoginModal";
import { PremiumModal } from "../components/PremiumModal";
import { PaymentModal } from "../components/PaymentModal";
import { UserProfileModal, HardwareComponents } from "../components/UserProfileModal";
import { BenchmarkProgress, BenchmarkResult } from "../components/BenchmarkProgress";
import { Notification } from "../components/NotificationPanel";
import { toast, Toaster } from "sonner";

export interface User {
  name: string;
  isPremium: boolean;
}

export interface AppContext {
  user: User | null;
  benchmarkResults: BenchmarkResult[];
  onRunBenchmark: (benchmarkName: string) => void;
  onClearHistory: () => void;
  isPremium: boolean;
}

export function Layout() {
  const [user, setUser] = useState<User | null>(null);
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showPremiumModal, setShowPremiumModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [showProfileModal, setShowProfileModal] = useState(false);
  const [showBenchmarkProgress, setShowBenchmarkProgress] = useState(false);
  const [currentBenchmark, setCurrentBenchmark] = useState("");
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [hardware, setHardware] = useState<HardwareComponents>({
    cpu: "",
    gpu: "",
    ram: "",
    storage: "",
    motherboard: "",
    psu: "",
    cooling: "",
  });
  const [benchmarkResults, setBenchmarkResults] = useState<BenchmarkResult[]>([
    {
      id: "1",
      name: "CPU - Test Avanzado",
      score: 15847,
      cpuUsage: 92,
      ramUsage: 68,
      temperature: 78,
      date: "15 Mar 2026, 14:30",
      duration: 25,
      peakCpuTemp: 82,
      peakGpuTemp: 75,
      avgCpuLoad: 88,
      avgRamLoad: 65,
      cpuClock: 4200,
      gpuClock: 1850,
      ramUsed: 10.5,
      diskLoad: 45,
      diskReadRate: 520,
      diskWriteRate: 380,
    },
    {
      id: "2",
      name: "GPU - Test Básico",
      score: 6340,
      cpuUsage: 45,
      ramUsage: 55,
      temperature: 85,
      date: "15 Mar 2026, 12:15",
      duration: 18,
      peakCpuTemp: 68,
      peakGpuTemp: 89,
      avgCpuLoad: 42,
      avgRamLoad: 52,
      cpuClock: 3600,
      gpuClock: 1650,
      ramUsed: 8.2,
      diskLoad: 32,
      diskReadRate: 310,
      diskWriteRate: 245,
    },
    {
      id: "3",
      name: "RAM - Test Avanzado",
      score: 9876,
      cpuUsage: 38,
      ramUsage: 88,
      temperature: 58,
      date: "14 Mar 2026, 18:45",
      duration: 22,
      peakCpuTemp: 62,
      peakGpuTemp: 65,
      avgCpuLoad: 35,
      avgRamLoad: 85,
      cpuClock: 3800,
      gpuClock: 1500,
      ramUsed: 14.8,
      diskLoad: 28,
      diskReadRate: 450,
      diskWriteRate: 420,
    },
    {
      id: "4",
      name: "Disco - Test Básico",
      score: 8234,
      cpuUsage: 28,
      ramUsage: 42,
      temperature: 52,
      date: "14 Mar 2026, 16:20",
      duration: 30,
      peakCpuTemp: 55,
      peakGpuTemp: 58,
      avgCpuLoad: 25,
      avgRamLoad: 40,
      cpuClock: 3200,
      gpuClock: 1400,
      ramUsed: 6.5,
      diskLoad: 78,
      diskReadRate: 3200,
      diskWriteRate: 2800,
    },
    {
      id: "5",
      name: "Prueba Térmica",
      score: 7890,
      cpuUsage: 95,
      ramUsage: 72,
      temperature: 85,
      date: "13 Mar 2026, 10:00",
      duration: 35,
      peakCpuTemp: 92,
      peakGpuTemp: 88,
      avgCpuLoad: 92,
      avgRamLoad: 70,
      cpuClock: 4500,
      gpuClock: 1900,
      ramUsed: 11.2,
      diskLoad: 55,
      diskReadRate: 420,
      diskWriteRate: 350,
    },
    {
      id: "6",
      name: "CPU - Test Básico",
      score: 11200,
      cpuUsage: 78,
      ramUsage: 45,
      temperature: 62,
      date: "12 Mar 2026, 09:30",
      duration: 20,
      peakCpuTemp: 68,
      peakGpuTemp: 60,
      avgCpuLoad: 75,
      avgRamLoad: 42,
      cpuClock: 3900,
      gpuClock: 1600,
      ramUsed: 7.8,
      diskLoad: 38,
      diskReadRate: 380,
      diskWriteRate: 290,
    },
  ]);

  const handleLogin = (name: string) => {
    setUser({ name, isPremium: false });
    toast.success(`¡Bienvenido, ${name}!`);
    
    // Notificación de bienvenida
    const welcomeNotification: Notification = {
      id: `notif-welcome-${Date.now()}`,
      type: "info",
      title: "👋 ¡Bienvenido a Core Care!",
      message: "Has iniciado sesión correctamente. Ahora puedes ejecutar benchmarks y ver el historial de pruebas. Actualiza a Premium para acceder a análisis avanzados.",
      timestamp: new Date().toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" }),
      read: false,
    };
    
    setNotifications(prev => [welcomeNotification, ...prev]);
  };

  const handleLogout = () => {
    setUser(null);
    setNotifications([]);
    toast.info("Sesión cerrada");
  };

  const handleUpgrade = () => {
    if (user) {
      setUser({ ...user, isPremium: true });
      setShowPremiumModal(false);
      setShowPaymentModal(false);
      toast.success("¡Ahora eres usuario Premium! 🎉");
      
      // Notificación de upgrade
      const premiumNotification: Notification = {
        id: `notif-premium-${Date.now()}`,
        type: "success",
        title: "👑 ¡Bienvenido a Premium!",
        message: "Ahora tienes acceso a benchmarks avanzados, gráficas de ejecución detalladas y recomendaciones de hardware personalizadas. ¡Disfruta de todas las funciones!",
        timestamp: new Date().toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" }),
        read: false,
      };
      
      setNotifications(prev => [premiumNotification, ...prev]);
    }
  };

  const handleProceedToPayment = () => {
    setShowPremiumModal(false);
    setShowPaymentModal(true);
  };

  const handleBackToPremium = () => {
    setShowPaymentModal(false);
    setShowPremiumModal(true);
  };

  const handleRunBenchmark = (benchmarkName: string) => {
    if (!user) {
      toast.error("Debes iniciar sesión para ejecutar benchmarks");
      setShowLoginModal(true);
      return;
    }
    setCurrentBenchmark(benchmarkName);
    setShowBenchmarkProgress(true);
    toast.success(`Iniciando ${benchmarkName}...`);
  };

  const handleBenchmarkComplete = (result: BenchmarkResult) => {
    setBenchmarkResults(prev => [result, ...prev]);
    toast.success(`¡${result.name} completado! Score: ${result.score.toLocaleString()}`);
    
    // Crear notificación
    const isCritical = result.score < 8000 || result.temperature > 80 || result.cpuUsage > 90 || result.ramUsage > 90;
    
    const newNotification: Notification = {
      id: `notif-${Date.now()}`,
      type: isCritical ? "warning" : "success",
      title: isCritical ? "⚠️ Benchmark completado - Resultado crítico" : "✅ Benchmark completado",
      message: `${result.name} finalizado con un score de ${result.score.toLocaleString()}. ${
        isCritical 
          ? "Se detectaron valores críticos que requieren tu atención." 
          : "Todos los parámetros están dentro de rangos normales."
      }`,
      timestamp: new Date().toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" }),
      link: "/history",
      read: false,
    };
    
    setNotifications(prev => [newNotification, ...prev]);
  };

  const handleMarkAsRead = (id: string) => {
    setNotifications(prev => 
      prev.map(notif => 
        notif.id === id ? { ...notif, read: true } : notif
      )
    );
  };

  const handleMarkAllAsRead = () => {
    setNotifications(prev => 
      prev.map(notif => ({ ...notif, read: true }))
    );
  };

  const handleClearNotifications = () => {
    setNotifications([]);
  };

  const handleClearHistory = () => {
    setBenchmarkResults([]);
    toast.info("Historial limpiado");
  };

  const handleSaveHardware = (newHardware: HardwareComponents) => {
    setHardware(newHardware);
    toast.success("Configuración de hardware guardada correctamente");
    
    // Notificación de hardware actualizado
    const hardwareNotification: Notification = {
      id: `notif-hardware-${Date.now()}`,
      type: "success",
      title: "⚙️ Hardware actualizado",
      message: "La configuración de tu hardware se ha guardado correctamente. Los benchmarks usarán esta información para análisis más precisos.",
      timestamp: new Date().toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" }),
      read: false,
    };
    
    setNotifications(prev => [hardwareNotification, ...prev]);
  };

  const context: AppContext = {
    user,
    benchmarkResults,
    onRunBenchmark: handleRunBenchmark,
    onClearHistory: handleClearHistory,
    isPremium: user?.isPremium || false,
  };

  return (
    <div className="min-h-screen bg-[#0a0e1a]">
      <Toaster position="top-right" richColors theme="dark" closeButton />
      
      <Header
        onLoginClick={() => setShowLoginModal(true)}
        onPremiumClick={() => setShowPremiumModal(true)}
        onProfileClick={() => setShowProfileModal(true)}
        user={user}
        onLogout={handleLogout}
        notifications={notifications}
        onMarkAsRead={handleMarkAsRead}
        onMarkAllAsRead={handleMarkAllAsRead}
        onClearNotifications={handleClearNotifications}
      />

      <main>
        <Outlet context={context} />
      </main>

      <footer className="relative bg-[#0a0e1a] border-t border-cyan-500/20 py-8 mt-16">
        <div className="absolute inset-0 bg-gradient-to-t from-cyan-500/5 to-transparent" />
        <div className="container mx-auto px-6 text-center relative">
          <p className="text-gray-500 text-sm tracking-[1px] font-normal">
            © 2026 AXIOM WORKS. TODOS LOS DERECHOS RESERVADOS.
          </p>
        </div>
      </footer>

      <LoginModal
        open={showLoginModal}
        onClose={() => setShowLoginModal(false)}
        onLogin={handleLogin}
      />

      <PremiumModal
        open={showPremiumModal}
        onClose={() => setShowPremiumModal(false)}
        onUpgrade={handleUpgrade}
        onProceedToPayment={handleProceedToPayment}
      />

      <PaymentModal
        open={showPaymentModal}
        onClose={() => setShowPaymentModal(false)}
        onPaymentSuccess={handleUpgrade}
        onBack={handleBackToPremium}
      />

      <UserProfileModal
        open={showProfileModal}
        onClose={() => setShowProfileModal(false)}
        user={user || { name: "", isPremium: false }}
        hardware={hardware}
        onSaveHardware={handleSaveHardware}
      />

      <BenchmarkProgress
        open={showBenchmarkProgress}
        onClose={() => setShowBenchmarkProgress(false)}
        benchmarkName={currentBenchmark}
        isPremium={user?.isPremium || false}
        onComplete={handleBenchmarkComplete}
      />
    </div>
  );
}