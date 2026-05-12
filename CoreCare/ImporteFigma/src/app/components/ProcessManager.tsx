import { useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import { Activity, StopCircle, Cpu, MemoryStick, AlertTriangle, XCircle, CheckCircle, Loader2, ShieldAlert } from "lucide-react";
import { Button } from "./ui/button";

interface Process {
  id: string;
  name: string;
  cpuUsage: number;
  ramUsage: number;
  status: "running" | "stopped" | "critical" | "stopping" | "failed";
  pid: number;
  isProtected?: boolean;
  originalCpuUsage?: number;
  originalRamUsage?: number;
}

export function ProcessManager() {
  const [processes, setProcesses] = useState<Process[]>([
    {
      id: "1",
      name: "Chrome.exe",
      cpuUsage: 45,
      ramUsage: 2.5,
      status: "running",
      pid: 8472,
    },
    {
      id: "2",
      name: "Discord.exe",
      cpuUsage: 28,
      ramUsage: 1.8,
      status: "running",
      pid: 5234,
    },
    {
      id: "3",
      name: "Spotify.exe",
      cpuUsage: 15,
      ramUsage: 0.9,
      status: "running",
      pid: 3421,
    },
    {
      id: "4",
      name: "Background Service",
      cpuUsage: 92,
      ramUsage: 4.2,
      status: "critical",
      pid: 1923,
    },
    {
      id: "5",
      name: "System Process",
      cpuUsage: 78,
      ramUsage: 3.5,
      status: "critical",
      pid: 4412,
      isProtected: true, // Este no se puede detener
    },
    {
      id: "6",
      name: "Windows Update",
      cpuUsage: 35,
      ramUsage: 1.2,
      status: "running",
      pid: 6721,
    },
    {
      id: "7",
      name: "Antivirus Scan",
      cpuUsage: 88,
      ramUsage: 2.9,
      status: "critical",
      pid: 2156,
      isProtected: true, // Este tampoco se puede detener
    },
    {
      id: "8",
      name: "Steam.exe",
      cpuUsage: 22,
      ramUsage: 1.5,
      status: "running",
      pid: 9384,
    },
  ]);

  const handleStopProcess = (id: string) => {
    const process = processes.find(p => p.id === id);
    if (!process) return;

    // Guardar valores originales
    setProcesses(prevProcesses =>
      prevProcesses.map(p =>
        p.id === id 
          ? { 
              ...p, 
              status: "stopping" as const,
              originalCpuUsage: p.cpuUsage,
              originalRamUsage: p.ramUsage,
            } 
          : p
      )
    );

    // Simular el proceso de detención (2-3 segundos)
    setTimeout(() => {
      if (process.isProtected) {
        // Si está protegido, falla
        setProcesses(prevProcesses =>
          prevProcesses.map(p =>
            p.id === id 
              ? { 
                  ...p, 
                  status: "failed" as const,
                  cpuUsage: p.originalCpuUsage || p.cpuUsage,
                  ramUsage: p.originalRamUsage || p.ramUsage,
                } 
              : p
          )
        );
      } else {
        // Si no está protegido, se detiene con éxito
        setProcesses(prevProcesses =>
          prevProcesses.map(p =>
            p.id === id 
              ? { 
                  ...p, 
                  status: "stopped" as const,
                  cpuUsage: 0,
                  ramUsage: 0,
                } 
              : p
          )
        );

        // Después de 1 segundo, eliminar el proceso de la lista
        setTimeout(() => {
          setProcesses(prevProcesses => prevProcesses.filter(p => p.id !== id));
        }, 1000);
      }
    }, 2000);
  };

  const getProcessColor = (process: Process) => {
    if (process.status === "stopping") return "text-gray-400";
    if (process.status === "stopped") return "text-gray-500";
    if (process.status === "failed") return "text-yellow-400";
    if (process.status === "critical") return "text-red-400";
    if (process.cpuUsage > 50) return "text-orange-400";
    return "text-cyan-400";
  };

  const getProcessBorder = (process: Process) => {
    if (process.status === "stopping") return "border-gray-500/50 animate-pulse";
    if (process.status === "stopped") return "border-gray-500/30";
    if (process.status === "failed") return "border-yellow-500/50 shadow-lg shadow-yellow-500/20";
    if (process.status === "critical") return "border-red-500/50 shadow-lg shadow-red-500/20";
    if (process.cpuUsage > 50) return "border-orange-500/30";
    return "border-cyan-500/30";
  };

  const criticalCount = processes.filter(p => p.status === "critical").length;
  const runningCount = processes.filter(p => p.status === "running" || p.status === "critical").length;
  const stoppedCount = processes.filter(p => p.status === "stopped").length;

  return (
    <section className="relative py-16 overflow-hidden min-h-screen">
      <div className="absolute inset-0 bg-gradient-to-b from-[#0a0e1a] via-[#151a2e] to-[#0a0e1a]" />
      
      {/* Animated Background Effects */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <motion.div
          className="absolute top-1/4 -left-32 w-96 h-96 bg-cyan-500/5 rounded-full blur-3xl"
          animate={{
            x: [0, 100, 0],
            y: [0, 50, 0],
          }}
          transition={{ duration: 20, repeat: Infinity }}
        />
        <motion.div
          className="absolute bottom-1/4 -right-32 w-96 h-96 bg-orange-500/5 rounded-full blur-3xl"
          animate={{
            x: [0, -100, 0],
            y: [0, -50, 0],
          }}
          transition={{ duration: 15, repeat: Infinity }}
        />
      </div>

      <div className="container mx-auto px-6 relative z-10">
        {/* Header */}
        <motion.div
          className="text-center mb-12"
          initial={{ y: 20, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ duration: 0.6 }}
        >
          <div className="flex items-center justify-center gap-3 mb-4">
            <Activity className="w-8 h-8 text-cyan-400" />
            <h2 className="text-4xl font-black text-white tracking-[3px]">
              MONITOR DE PROCESOS
            </h2>
          </div>
          <div className="h-1 w-32 bg-gradient-to-r from-transparent via-cyan-500 to-transparent mx-auto mb-4" />
          <p className="text-gray-400 font-normal max-w-2xl mx-auto">
            Controla los subprocesos que están consumiendo recursos excesivos del sistema
          </p>
        </motion.div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8 max-w-5xl mx-auto">
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.1 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-cyan-500/30 p-6"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-normal mb-1">Procesos Activos</p>
                <p className="text-3xl font-black text-cyan-400">{runningCount}</p>
              </div>
              <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/20 to-blue-500/20 rounded-lg flex items-center justify-center">
                <Activity className="w-6 h-6 text-cyan-400" />
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.2 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-red-500/30 p-6"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-normal mb-1">Procesos Críticos</p>
                <p className="text-3xl font-black text-red-400">{criticalCount}</p>
              </div>
              <div className="w-12 h-12 bg-gradient-to-br from-red-500/20 to-orange-500/20 rounded-lg flex items-center justify-center">
                <AlertTriangle className="w-6 h-6 text-red-400" />
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-gray-500/30 p-6"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-sm font-normal mb-1">Procesos Detenidos</p>
                <p className="text-3xl font-black text-gray-400">{stoppedCount}</p>
              </div>
              <div className="w-12 h-12 bg-gradient-to-br from-gray-500/20 to-gray-600/20 rounded-lg flex items-center justify-center">
                <CheckCircle className="w-6 h-6 text-gray-400" />
              </div>
            </div>
          </motion.div>
        </div>

        {/* Process List */}
        <div className="max-w-6xl mx-auto space-y-4">
          <AnimatePresence mode="popLayout">
            {processes.map((process, index) => {
              const isHighUsage = process.cpuUsage > 50 || process.ramUsage > 2;
              const isCritical = process.status === "critical";
              const isStopped = process.status === "stopped";
              const isStopping = process.status === "stopping";
              const isFailed = process.status === "failed";

              return (
                <motion.div
                  key={process.id}
                  initial={{ x: -20, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  exit={{ x: 20, opacity: 0, height: 0, marginBottom: 0 }}
                  transition={{ duration: 0.3 }}
                  className={`relative bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border ${getProcessBorder(
                    process
                  )} p-6 transition-all duration-300 ${isStopping ? 'opacity-50' : isStopped ? 'opacity-60' : ''}`}
                >
                  {/* Critical Badge */}
                  {isCritical && !isStopped && !isStopping && !isFailed && (
                    <div className="absolute -top-3 -right-3 z-10">
                      <div className="bg-red-500 text-white px-3 py-1 rounded-full flex items-center gap-2 shadow-lg shadow-red-500/50 animate-pulse">
                        <AlertTriangle className="w-4 h-4" />
                        <span className="text-xs font-bold">ALTO CONSUMO</span>
                      </div>
                    </div>
                  )}

                  {/* Stopping Badge */}
                  {isStopping && (
                    <div className="absolute -top-3 -right-3 z-10">
                      <div className="bg-gray-600 text-white px-3 py-1 rounded-full flex items-center gap-2 shadow-lg animate-pulse">
                        <Loader2 className="w-4 h-4 animate-spin" />
                        <span className="text-xs font-bold">DETENIENDO...</span>
                      </div>
                    </div>
                  )}

                  {/* Stopped Badge */}
                  {isStopped && (
                    <div className="absolute -top-3 -right-3 z-10">
                      <div className="bg-green-600 text-white px-3 py-1 rounded-full flex items-center gap-2 shadow-lg shadow-green-500/50">
                        <CheckCircle className="w-4 h-4" />
                        <span className="text-xs font-bold">DETENIDO</span>
                      </div>
                    </div>
                  )}

                  {/* Failed Badge */}
                  {isFailed && (
                    <div className="absolute -top-3 -right-3 z-10">
                      <div className="bg-yellow-600 text-white px-3 py-1 rounded-full flex items-center gap-2 shadow-lg shadow-yellow-500/50">
                        <ShieldAlert className="w-4 h-4" />
                        <span className="text-xs font-bold">NO SE PUEDE DETENER</span>
                      </div>
                    </div>
                  )}

                  <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex items-center gap-4 flex-1">
                      <div
                        className={`w-12 h-12 bg-gradient-to-br ${
                          isStopping
                            ? 'from-gray-500/20 to-gray-600/20'
                            : isStopped
                            ? 'from-green-500/20 to-emerald-500/20'
                            : isFailed
                            ? 'from-yellow-500/20 to-orange-500/20'
                            : isCritical
                            ? 'from-red-500/20 to-orange-500/20'
                            : 'from-cyan-500/20 to-blue-500/20'
                        } rounded-lg flex items-center justify-center border ${getProcessBorder(process)}`}
                      >
                        {isStopping ? (
                          <Loader2 className="w-6 h-6 text-gray-400 animate-spin" />
                        ) : isFailed ? (
                          <ShieldAlert className="w-6 h-6 text-yellow-400" />
                        ) : (
                          <Activity className={`w-6 h-6 ${getProcessColor(process)}`} />
                        )}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-1">
                          <h3 className={`font-bold text-lg ${getProcessColor(process)}`}>
                            {process.name}
                          </h3>
                          <span className="text-xs text-gray-500 font-mono">PID: {process.pid}</span>
                          {process.isProtected && (
                            <span className="text-xs bg-yellow-500/20 text-yellow-400 px-2 py-0.5 rounded-full border border-yellow-500/30 font-bold">
                              PROTEGIDO
                            </span>
                          )}
                        </div>
                        <div className="flex items-center gap-4">
                          <div className="flex items-center gap-2">
                            <Cpu className="w-4 h-4 text-orange-400" />
                            <span className="text-sm text-gray-400 font-normal">CPU:</span>
                            <span
                              className={`text-sm font-bold ${
                                isStopping || isStopped
                                  ? 'text-gray-500'
                                  : process.cpuUsage > 70
                                  ? 'text-red-400'
                                  : process.cpuUsage > 50
                                  ? 'text-orange-400'
                                  : 'text-green-400'
                              }`}
                            >
                              {process.cpuUsage}%
                            </span>
                          </div>
                          <div className="flex items-center gap-2">
                            <MemoryStick className="w-4 h-4 text-purple-400" />
                            <span className="text-sm text-gray-400 font-normal">RAM:</span>
                            <span
                              className={`text-sm font-bold ${
                                isStopping || isStopped
                                  ? 'text-gray-500'
                                  : process.ramUsage > 3
                                  ? 'text-red-400'
                                  : process.ramUsage > 2
                                  ? 'text-orange-400'
                                  : 'text-green-400'
                              }`}
                            >
                              {process.ramUsage} GB
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Progress Bars and Action */}
                    <div className="flex items-center gap-6">
                      {/* CPU Progress */}
                      <div className="w-32">
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-xs text-gray-500 font-normal">CPU</span>
                          <span className="text-xs text-gray-400 font-bold">{process.cpuUsage}%</span>
                        </div>
                        <div className="h-2 bg-[#151a2e] rounded-full overflow-hidden">
                          <motion.div
                            className={`h-full ${
                              isStopping || isStopped
                                ? 'bg-gray-500'
                                : process.cpuUsage > 70
                                ? 'bg-gradient-to-r from-red-500 to-orange-500'
                                : process.cpuUsage > 50
                                ? 'bg-gradient-to-r from-orange-500 to-yellow-500'
                                : 'bg-gradient-to-r from-green-500 to-cyan-500'
                            }`}
                            initial={{ width: 0 }}
                            animate={{ width: `${process.cpuUsage}%` }}
                            transition={{ duration: 0.5 }}
                          />
                        </div>
                      </div>

                      {/* RAM Progress */}
                      <div className="w-32">
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-xs text-gray-500 font-normal">RAM</span>
                          <span className="text-xs text-gray-400 font-bold">{process.ramUsage} GB</span>
                        </div>
                        <div className="h-2 bg-[#151a2e] rounded-full overflow-hidden">
                          <motion.div
                            className={`h-full ${
                              isStopping || isStopped
                                ? 'bg-gray-500'
                                : process.ramUsage > 3
                                ? 'bg-gradient-to-r from-red-500 to-pink-500'
                                : process.ramUsage > 2
                                ? 'bg-gradient-to-r from-orange-500 to-purple-500'
                                : 'bg-gradient-to-r from-purple-500 to-blue-500'
                            }`}
                            initial={{ width: 0 }}
                            animate={{ width: `${(process.ramUsage / 8) * 100}%` }}
                            transition={{ duration: 0.5 }}
                          />
                        </div>
                      </div>

                      {/* Stop Button */}
                      <Button
                        onClick={() => handleStopProcess(process.id)}
                        disabled={isStopped || isStopping}
                        variant="ghost"
                        className={`${
                          isStopped || isStopping
                            ? 'text-gray-500 cursor-not-allowed opacity-50'
                            : isFailed
                            ? 'text-yellow-400 hover:text-yellow-300 hover:bg-yellow-500/10 border-yellow-500/30 hover:border-yellow-500/50'
                            : 'text-red-400 hover:text-red-300 hover:bg-red-500/10 border-red-500/30 hover:border-red-500/50'
                        } border px-4 py-2 transition-all`}
                      >
                        {isStopping ? (
                          <>
                            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                            Deteniendo
                          </>
                        ) : isStopped ? (
                          <>
                            <CheckCircle className="w-4 h-4 mr-2" />
                            Detenido
                          </>
                        ) : (
                          <>
                            <StopCircle className="w-4 h-4 mr-2" />
                            Frenar
                          </>
                        )}
                      </Button>
                    </div>
                  </div>

                  {/* Warning Message for Critical Processes */}
                  {isCritical && !isStopped && !isStopping && !isFailed && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: "auto", opacity: 1 }}
                      className="mt-4 p-3 bg-red-500/10 rounded-lg border border-red-500/30"
                    >
                      <div className="flex items-start gap-2">
                        <AlertTriangle className="w-4 h-4 text-red-400 mt-0.5 flex-shrink-0" />
                        <p className="text-sm text-red-300 font-normal">
                          Este proceso está consumiendo recursos excesivos y puede afectar el rendimiento del sistema.
                          Se recomienda detenerlo si no es esencial.
                        </p>
                      </div>
                    </motion.div>
                  )}

                  {/* Failed Message */}
                  {isFailed && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: "auto", opacity: 1 }}
                      className="mt-4 p-3 bg-yellow-500/10 rounded-lg border border-yellow-500/30"
                    >
                      <div className="flex items-start gap-2">
                        <ShieldAlert className="w-4 h-4 text-yellow-400 mt-0.5 flex-shrink-0" />
                        <p className="text-sm text-yellow-300 font-normal">
                          Este proceso está protegido por el sistema y no se puede detener. Es un proceso crítico necesario para el funcionamiento del sistema operativo.
                        </p>
                      </div>
                    </motion.div>
                  )}
                </motion.div>
              );
            })}
          </AnimatePresence>
        </div>
      </div>
    </section>
  );
}
