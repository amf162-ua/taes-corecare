import { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "./ui/dialog";
import { Progress } from "./ui/progress";
import { X, Cpu, Activity } from "lucide-react";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { motion } from "motion/react";

interface BenchmarkProgressProps {
  open: boolean;
  onClose: () => void;
  benchmarkName: string;
  isPremium: boolean;
  onComplete: (result: BenchmarkResult) => void;
}

export interface BenchmarkResult {
  id: string;
  name: string;
  date: string;
  score: number;
  cpuUsage: number;
  ramUsage: number;
  temperature: number;
  // Campos adicionales del diagrama
  duration: number; // en segundos
  peakCpuTemp: number;
  peakGpuTemp: number;
  avgCpuLoad: number;
  avgRamLoad: number;
  cpuClock: number; // MHz
  gpuClock: number; // MHz
  ramUsed: number; // GB
  diskLoad: number;
  diskReadRate: number; // MB/s
  diskWriteRate: number; // MB/s
}

export function BenchmarkProgress({ open, onClose, benchmarkName, isPremium, onComplete }: BenchmarkProgressProps) {
  const [progress, setProgress] = useState(0);
  const [realTimeData, setRealTimeData] = useState<Array<{
    time: number;
    cpu: number;
    ram: number;
    temp: number;
  }>>([]);
  const [currentMetrics, setCurrentMetrics] = useState({
    cpu: 0,
    ram: 0,
    temp: 50,
  });

  useEffect(() => {
    if (!open) {
      setProgress(0);
      setRealTimeData([]);
      setCurrentMetrics({ cpu: 0, ram: 0, temp: 50 });
      return;
    }

    const duration = 10000; // 10 seconds for demo
    const interval = 100;
    const steps = duration / interval;
    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const newProgress = (currentStep / steps) * 100;
      setProgress(newProgress);

      // Simulate real-time metrics for premium users
      if (isPremium) {
        const cpu = 20 + Math.random() * 60 + Math.sin(currentStep / 10) * 15;
        const ram = 30 + Math.random() * 50 + Math.cos(currentStep / 8) * 10;
        const temp = 50 + Math.random() * 30 + Math.sin(currentStep / 15) * 10;

        setCurrentMetrics({ cpu, ram, temp });
        setRealTimeData(prev => {
          const newData = [...prev, {
            time: currentStep,
            cpu: Math.round(cpu),
            ram: Math.round(ram),
            temp: Math.round(temp),
          }];
          // Keep only last 50 points
          return newData.slice(-50);
        });
      }

      if (currentStep >= steps) {
        clearInterval(timer);
        // Generate result
        const result: BenchmarkResult = {
          id: Date.now().toString(),
          name: benchmarkName,
          date: new Date().toLocaleString('es-ES'),
          score: Math.round(5000 + Math.random() * 5000),
          cpuUsage: Math.round(currentMetrics.cpu),
          ramUsage: Math.round(currentMetrics.ram),
          temperature: Math.round(currentMetrics.temp),
          // Campos adicionales del diagrama
          duration: 10, // en segundos
          peakCpuTemp: Math.round(70 + Math.random() * 30),
          peakGpuTemp: Math.round(80 + Math.random() * 20),
          avgCpuLoad: Math.round(50 + Math.random() * 30),
          avgRamLoad: Math.round(40 + Math.random() * 20),
          cpuClock: Math.round(2000 + Math.random() * 1000),
          gpuClock: Math.round(1500 + Math.random() * 500),
          ramUsed: Math.round(8 + Math.random() * 4),
          diskLoad: Math.round(50 + Math.random() * 30),
          diskReadRate: Math.round(100 + Math.random() * 50),
          diskWriteRate: Math.round(80 + Math.random() * 40),
        };
        onComplete(result);
      }
    }, interval);

    return () => clearInterval(timer);
  }, [open, benchmarkName, isPremium, onComplete, currentMetrics.cpu, currentMetrics.ram, currentMetrics.temp]);

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[800px] bg-[#1c2030] border-2 border-cyan-500/30 text-white">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 w-8 h-8 flex items-center justify-center rounded-lg bg-gray-800/50 hover:bg-red-500/20 border border-gray-700 hover:border-red-500/50 transition-all duration-300 z-50"
        >
          <X className="w-4 h-4 text-gray-400 hover:text-red-400" />
        </button>

        <DialogHeader>
          <DialogTitle className="text-2xl text-center bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent tracking-[2px] pr-8">
            {benchmarkName}
          </DialogTitle>
        </DialogHeader>

        <div className="mt-6">
          {!isPremium ? (
            // Basic progress bar for non-premium users
            <div className="space-y-6">
              <div className="text-center">
                <div className="text-5xl font-black text-cyan-400 mb-2">
                  {Math.round(progress)}%
                </div>
                <p className="text-gray-400 font-normal">Ejecutando benchmark...</p>
              </div>

              <div className="relative">
                <Progress value={progress} className="h-4 bg-gray-800" />
                <div 
                  className="absolute top-0 left-0 h-4 bg-gradient-to-r from-cyan-500 to-blue-500 rounded-full transition-all duration-300"
                  style={{ width: `${progress}%` }}
                />
              </div>

              <div className="grid grid-cols-3 gap-4 pt-4">
                <div className="bg-[#0a0e1a] rounded-lg p-4 border border-cyan-500/20">
                  <p className="text-xs text-gray-500 mb-1 font-normal">Tiempo restante</p>
                  <p className="text-lg font-bold text-cyan-400">
                    {Math.round((100 - progress) / 10)}s
                  </p>
                </div>
                <div className="bg-[#0a0e1a] rounded-lg p-4 border border-cyan-500/20">
                  <p className="text-xs text-gray-500 mb-1 font-normal">Estado</p>
                  <p className="text-lg font-bold text-green-400">
                    {progress < 100 ? "En proceso" : "Completado"}
                  </p>
                </div>
                <div className="bg-[#0a0e1a] rounded-lg p-4 border border-cyan-500/20">
                  <p className="text-xs text-gray-500 mb-1 font-normal">Progreso</p>
                  <p className="text-lg font-bold text-orange-400">
                    {Math.round(progress)}/100
                  </p>
                </div>
              </div>
            </div>
          ) : (
            // Real-time graphs for premium users
            <div className="space-y-6">
              <div className="grid grid-cols-3 gap-4">
                <motion.div 
                  className="bg-[#0a0e1a] rounded-lg p-4 border border-orange-500/30"
                  initial={{ scale: 0.9, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                  transition={{ duration: 0.3 }}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Cpu className="w-4 h-4 text-orange-400" />
                    <p className="text-xs text-gray-400 font-normal">CPU</p>
                  </div>
                  <p className="text-2xl font-black text-orange-400">
                    {Math.round(currentMetrics.cpu)}%
                  </p>
                </motion.div>

                <motion.div 
                  className="bg-[#0a0e1a] rounded-lg p-4 border border-cyan-500/30"
                  initial={{ scale: 0.9, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                  transition={{ duration: 0.3, delay: 0.1 }}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Activity className="w-4 h-4 text-cyan-400" />
                    <p className="text-xs text-gray-400 font-normal">RAM</p>
                  </div>
                  <p className="text-2xl font-black text-cyan-400">
                    {Math.round(currentMetrics.ram)}%
                  </p>
                </motion.div>

                <motion.div 
                  className="bg-[#0a0e1a] rounded-lg p-4 border border-red-500/30"
                  initial={{ scale: 0.9, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                  transition={{ duration: 0.3, delay: 0.2 }}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Activity className="w-4 h-4 text-red-400" />
                    <p className="text-xs text-gray-400 font-normal">TEMP</p>
                  </div>
                  <p className="text-2xl font-black text-red-400">
                    {Math.round(currentMetrics.temp)}°C
                  </p>
                </motion.div>
              </div>

              <div className="bg-[#0a0e1a] rounded-lg p-4 border border-cyan-500/20">
                <div className="mb-4 flex justify-between items-center">
                  <h3 className="text-sm font-bold text-cyan-400 tracking-[1px]">
                    MONITOREO EN TIEMPO REAL
                  </h3>
                  <div className="text-xs text-gray-500 font-normal">
                    Progreso: {Math.round(progress)}%
                  </div>
                </div>
                
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={realTimeData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
                    <XAxis 
                      dataKey="time" 
                      stroke="#64748b"
                      tick={{ fill: '#64748b', fontSize: 12 }}
                    />
                    <YAxis 
                      stroke="#64748b"
                      tick={{ fill: '#64748b', fontSize: 12 }}
                    />
                    <Tooltip 
                      contentStyle={{ 
                        backgroundColor: '#1c2030', 
                        border: '1px solid rgba(6, 182, 212, 0.3)',
                        borderRadius: '8px',
                        color: '#fff'
                      }}
                    />
                    <Legend />
                    <Line 
                      type="monotone" 
                      dataKey="cpu" 
                      stroke="#fb923c" 
                      strokeWidth={2}
                      dot={false}
                      name="CPU %"
                    />
                    <Line 
                      type="monotone" 
                      dataKey="ram" 
                      stroke="#06b6d4" 
                      strokeWidth={2}
                      dot={false}
                      name="RAM %"
                    />
                    <Line 
                      type="monotone" 
                      dataKey="temp" 
                      stroke="#ef4444" 
                      strokeWidth={2}
                      dot={false}
                      name="TEMP °C"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}