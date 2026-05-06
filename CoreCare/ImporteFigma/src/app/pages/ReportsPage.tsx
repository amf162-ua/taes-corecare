import { motion, AnimatePresence } from "motion/react";
import { FileText, Clock, CheckCircle2, XCircle, AlertCircle, Building2, Calendar, Eye, MessageSquare, ChevronDown, ChevronUp, Filter } from "lucide-react";
import { Button } from "../components/ui/button";
import { useState } from "react";

type ReportStatus = "pending" | "in-review" | "resolved" | "rejected";

interface Report {
  id: string;
  benchmarkName: string;
  description: string;
  company: string;
  companyIcon: string;
  status: ReportStatus;
  date: string;
  lastUpdate: string;
  response?: string;
  ticketId: string;
}

const mockReports: Report[] = [
  {
    id: "1",
    benchmarkName: "CPU Stress Test",
    description: "El sistema se sobrecalienta rápidamente al ejecutar este benchmark. La temperatura alcanza los 95°C en menos de 10 segundos y el rendimiento cae drásticamente.",
    company: "Intel Technical Team",
    companyIcon: "🔵",
    status: "in-review",
    date: "18/03/2026 14:30",
    lastUpdate: "18/03/2026 15:45",
    response: "Hemos recibido tu reporte y nuestro equipo técnico está analizando los datos del benchmark. Estamos investigando posibles problemas de throttling térmico.",
    ticketId: "INT-2026-4521",
  },
  {
    id: "2",
    benchmarkName: "GPU Performance Test",
    description: "La tarjeta gráfica muestra artefactos visuales durante el benchmark y el driver se reinicia constantemente.",
    company: "NVIDIA Support",
    companyIcon: "🟢",
    status: "resolved",
    date: "17/03/2026 10:15",
    lastUpdate: "18/03/2026 09:20",
    response: "Hemos identificado el problema. Se trataba de una versión desactualizada del driver. Actualiza a la versión 535.98 o superior. El problema debería estar resuelto.",
    ticketId: "NVD-2026-8834",
  },
  {
    id: "3",
    benchmarkName: "Memory Bandwidth Test",
    description: "El test muestra velocidades de lectura muy por debajo de lo esperado para DDR5-6000. Solo alcanza 3200 MT/s.",
    company: "CoreCare Premium Support",
    companyIcon: "⭐",
    status: "pending",
    date: "18/03/2026 16:20",
    lastUpdate: "18/03/2026 16:20",
    ticketId: "CCS-2026-1092",
  },
  {
    id: "4",
    benchmarkName: "Disk I/O Test",
    description: "Las velocidades de escritura están muy limitadas y el sistema se congela ocasionalmente durante el benchmark.",
    company: "Microsoft Support",
    companyIcon: "🟦",
    status: "rejected",
    date: "16/03/2026 08:45",
    lastUpdate: "17/03/2026 11:30",
    response: "Después de revisar los logs, determinamos que el problema está relacionado con un conflicto de software de terceros (antivirus) que no está bajo nuestro soporte. Te recomendamos contactar al fabricante del antivirus.",
    ticketId: "MSF-2026-7623",
  },
  {
    id: "5",
    benchmarkName: "Multi-Core Performance",
    description: "Solo 6 de 8 núcleos aparecen activos durante el benchmark. El sistema no utiliza todos los cores disponibles.",
    company: "AMD Support",
    companyIcon: "🔴",
    status: "in-review",
    date: "17/03/2026 13:50",
    lastUpdate: "18/03/2026 08:15",
    response: "Estamos investigando el problema. Por favor, verifica que el perfil de energía esté configurado en 'Alto rendimiento' y que no haya limitaciones en la BIOS.",
    ticketId: "AMD-2026-3341",
  },
];

const getStatusConfig = (status: ReportStatus) => {
  switch (status) {
    case "pending":
      return {
        label: "PENDIENTE",
        icon: Clock,
        color: "text-yellow-400",
        bg: "bg-yellow-500/20",
        border: "border-yellow-500/30",
        gradient: "from-yellow-500/20 to-orange-500/20",
      };
    case "in-review":
      return {
        label: "EN REVISIÓN",
        icon: AlertCircle,
        color: "text-blue-400",
        bg: "bg-blue-500/20",
        border: "border-blue-500/30",
        gradient: "from-blue-500/20 to-cyan-500/20",
      };
    case "resolved":
      return {
        label: "RESUELTO",
        icon: CheckCircle2,
        color: "text-green-400",
        bg: "bg-green-500/20",
        border: "border-green-500/30",
        gradient: "from-green-500/20 to-emerald-500/20",
      };
    case "rejected":
      return {
        label: "RECHAZADO",
        icon: XCircle,
        color: "text-red-400",
        bg: "bg-red-500/20",
        border: "border-red-500/30",
        gradient: "from-red-500/20 to-orange-500/20",
      };
  }
};

export function ReportsPage() {
  const [expandedReports, setExpandedReports] = useState<string[]>([]);
  const [filterStatus, setFilterStatus] = useState<ReportStatus | "all">("all");

  const toggleReport = (id: string) => {
    if (expandedReports.includes(id)) {
      setExpandedReports(expandedReports.filter(reportId => reportId !== id));
    } else {
      setExpandedReports([...expandedReports, id]);
    }
  };

  const filteredReports = filterStatus === "all" 
    ? mockReports 
    : mockReports.filter(report => report.status === filterStatus);

  const pendingCount = mockReports.filter(r => r.status === "pending").length;
  const inReviewCount = mockReports.filter(r => r.status === "in-review").length;
  const resolvedCount = mockReports.filter(r => r.status === "resolved").length;
  const rejectedCount = mockReports.filter(r => r.status === "rejected").length;

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
            <FileText className="w-8 h-8 text-cyan-400" />
            <h2 className="text-4xl font-black text-white tracking-[3px]">
              ESTADO DE REPORTES
            </h2>
          </div>
          <div className="h-1 w-32 bg-gradient-to-r from-transparent via-cyan-500 to-transparent mx-auto mb-4" />
          <p className="text-gray-400 font-normal max-w-2xl mx-auto">
            Seguimiento en tiempo real de tus reportes enviados a empresas asociadas
          </p>
        </motion.div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8 max-w-5xl mx-auto">
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.1 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-yellow-500/30 p-4"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-xs font-normal mb-1">Pendientes</p>
                <p className="text-2xl font-black text-yellow-400">{pendingCount}</p>
              </div>
              <Clock className="w-8 h-8 text-yellow-400" />
            </div>
          </motion.div>

          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.2 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-blue-500/30 p-4"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-xs font-normal mb-1">En Revisión</p>
                <p className="text-2xl font-black text-blue-400">{inReviewCount}</p>
              </div>
              <AlertCircle className="w-8 h-8 text-blue-400" />
            </div>
          </motion.div>

          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-green-500/30 p-4"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-xs font-normal mb-1">Resueltos</p>
                <p className="text-2xl font-black text-green-400">{resolvedCount}</p>
              </div>
              <CheckCircle2 className="w-8 h-8 text-green-400" />
            </div>
          </motion.div>

          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.4 }}
            className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-red-500/30 p-4"
          >
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-400 text-xs font-normal mb-1">Rechazados</p>
                <p className="text-2xl font-black text-red-400">{rejectedCount}</p>
              </div>
              <XCircle className="w-8 h-8 text-red-400" />
            </div>
          </motion.div>
        </div>

        {/* Filter Buttons */}
        <div className="flex items-center justify-center gap-3 mb-8 flex-wrap">
          <Button
            onClick={() => setFilterStatus("all")}
            variant="ghost"
            className={`${
              filterStatus === "all"
                ? "bg-cyan-500/20 text-cyan-400 border-cyan-500/50"
                : "text-gray-400 hover:text-white border-gray-500/30"
            } border transition-all`}
          >
            <Filter className="w-4 h-4 mr-2" />
            Todos
          </Button>
          <Button
            onClick={() => setFilterStatus("pending")}
            variant="ghost"
            className={`${
              filterStatus === "pending"
                ? "bg-yellow-500/20 text-yellow-400 border-yellow-500/50"
                : "text-gray-400 hover:text-white border-gray-500/30"
            } border transition-all`}
          >
            <Clock className="w-4 h-4 mr-2" />
            Pendientes
          </Button>
          <Button
            onClick={() => setFilterStatus("in-review")}
            variant="ghost"
            className={`${
              filterStatus === "in-review"
                ? "bg-blue-500/20 text-blue-400 border-blue-500/50"
                : "text-gray-400 hover:text-white border-gray-500/30"
            } border transition-all`}
          >
            <AlertCircle className="w-4 h-4 mr-2" />
            En Revisión
          </Button>
          <Button
            onClick={() => setFilterStatus("resolved")}
            variant="ghost"
            className={`${
              filterStatus === "resolved"
                ? "bg-green-500/20 text-green-400 border-green-500/50"
                : "text-gray-400 hover:text-white border-gray-500/30"
            } border transition-all`}
          >
            <CheckCircle2 className="w-4 h-4 mr-2" />
            Resueltos
          </Button>
          <Button
            onClick={() => setFilterStatus("rejected")}
            variant="ghost"
            className={`${
              filterStatus === "rejected"
                ? "bg-red-500/20 text-red-400 border-red-500/50"
                : "text-gray-400 hover:text-white border-gray-500/30"
            } border transition-all`}
          >
            <XCircle className="w-4 h-4 mr-2" />
            Rechazados
          </Button>
        </div>

        {/* Reports List */}
        <div className="max-w-5xl mx-auto space-y-4">
          <AnimatePresence mode="popLayout">
            {filteredReports.length === 0 ? (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -20 }}
                className="bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-cyan-500/30 p-12 text-center"
              >
                <FileText className="w-16 h-16 text-gray-600 mx-auto mb-4" />
                <h3 className="text-xl font-bold text-white mb-2">
                  No hay reportes en esta categoría
                </h3>
                <p className="text-gray-400 font-normal">
                  Intenta cambiar el filtro para ver otros reportes
                </p>
              </motion.div>
            ) : (
              filteredReports.map((report, index) => {
                const statusConfig = getStatusConfig(report.status);
                const StatusIcon = statusConfig.icon;
                const isExpanded = expandedReports.includes(report.id);

                return (
                  <motion.div
                    key={report.id}
                    initial={{ x: -20, opacity: 0 }}
                    animate={{ x: 0, opacity: 1 }}
                    exit={{ x: 20, opacity: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className={`bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border ${statusConfig.border} p-6 transition-all duration-300`}
                  >
                    {/* Header */}
                    <div className="flex items-start justify-between gap-4 mb-4">
                      <div className="flex items-start gap-4 flex-1">
                        <div className={`w-12 h-12 bg-gradient-to-br ${statusConfig.gradient} rounded-lg flex items-center justify-center border ${statusConfig.border}`}>
                          <StatusIcon className={`w-6 h-6 ${statusConfig.color}`} />
                        </div>
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-2">
                            <h3 className="font-bold text-lg text-white">
                              {report.benchmarkName}
                            </h3>
                            <span className={`text-xs ${statusConfig.bg} ${statusConfig.color} px-3 py-1 rounded-full border ${statusConfig.border} font-bold`}>
                              {statusConfig.label}
                            </span>
                          </div>
                          <div className="flex items-center gap-4 text-sm text-gray-400 font-normal">
                            <div className="flex items-center gap-2">
                              <Building2 className="w-4 h-4" />
                              <span>{report.companyIcon} {report.company}</span>
                            </div>
                            <div className="flex items-center gap-2">
                              <Calendar className="w-4 h-4" />
                              <span>{report.date}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-xs text-gray-500 font-normal mb-1">Ticket ID</p>
                        <p className="text-sm font-bold text-cyan-400 font-mono">
                          {report.ticketId}
                        </p>
                      </div>
                    </div>

                    {/* Description */}
                    <div className="bg-[#151a2e]/60 rounded-lg border border-gray-500/20 p-4 mb-4">
                      <div className="flex items-start gap-2">
                        <MessageSquare className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
                        <div className="flex-1">
                          <p className="text-xs text-gray-500 font-normal mb-1">Tu mensaje:</p>
                          <p className="text-sm text-gray-300 font-normal">
                            {report.description}
                          </p>
                        </div>
                      </div>
                    </div>

                    {/* Response Section */}
                    {report.response && (
                      <div className={`bg-gradient-to-r ${statusConfig.gradient} rounded-lg border ${statusConfig.border} p-4 mb-4`}>
                        <div className="flex items-start gap-2">
                          <Eye className={`w-4 h-4 ${statusConfig.color} mt-0.5 flex-shrink-0`} />
                          <div className="flex-1">
                            <p className={`text-xs ${statusConfig.color} font-bold mb-1`}>
                              Respuesta de {report.company}:
                            </p>
                            <p className="text-sm text-white font-normal">
                              {report.response}
                            </p>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Footer */}
                    <div className="flex items-center justify-between">
                      <div className="text-xs text-gray-500 font-normal">
                        Última actualización: {report.lastUpdate}
                      </div>
                      <Button
                        onClick={() => toggleReport(report.id)}
                        variant="ghost"
                        className="text-cyan-400 hover:text-cyan-300 hover:bg-cyan-500/10"
                      >
                        {isExpanded ? (
                          <>
                            <ChevronUp className="w-4 h-4 mr-2" />
                            Ocultar detalles
                          </>
                        ) : (
                          <>
                            <ChevronDown className="w-4 h-4 mr-2" />
                            Ver más detalles
                          </>
                        )}
                      </Button>
                    </div>

                    {/* Expanded Details */}
                    <AnimatePresence>
                      {isExpanded && (
                        <motion.div
                          initial={{ height: 0, opacity: 0 }}
                          animate={{ height: "auto", opacity: 1 }}
                          exit={{ height: 0, opacity: 0 }}
                          transition={{ duration: 0.3 }}
                          className="overflow-hidden"
                        >
                          <div className="mt-4 pt-4 border-t border-gray-500/20 space-y-3">
                            <div className="grid grid-cols-2 gap-4">
                              <div className="bg-[#151a2e]/60 rounded-lg border border-gray-500/20 p-3">
                                <p className="text-xs text-gray-500 font-normal mb-1">Estado del ticket</p>
                                <p className={`text-sm font-bold ${statusConfig.color}`}>
                                  {statusConfig.label}
                                </p>
                              </div>
                              <div className="bg-[#151a2e]/60 rounded-lg border border-gray-500/20 p-3">
                                <p className="text-xs text-gray-500 font-normal mb-1">Prioridad</p>
                                <p className="text-sm font-bold text-orange-400">
                                  {report.status === "pending" || report.status === "in-review" ? "Alta" : "Normal"}
                                </p>
                              </div>
                            </div>
                            <div className="bg-[#151a2e]/60 rounded-lg border border-gray-500/20 p-3">
                              <p className="text-xs text-gray-500 font-normal mb-2">Línea de tiempo</p>
                              <div className="space-y-2">
                                <div className="flex items-center gap-3">
                                  <div className="w-2 h-2 bg-cyan-400 rounded-full"></div>
                                  <div className="flex-1">
                                    <p className="text-xs text-white font-normal">Reporte creado</p>
                                    <p className="text-xs text-gray-500 font-normal">{report.date}</p>
                                  </div>
                                </div>
                                {report.response && (
                                  <div className="flex items-center gap-3">
                                    <div className={`w-2 h-2 ${statusConfig.color.replace('text-', 'bg-')} rounded-full`}></div>
                                    <div className="flex-1">
                                      <p className="text-xs text-white font-normal">Respuesta recibida</p>
                                      <p className="text-xs text-gray-500 font-normal">{report.lastUpdate}</p>
                                    </div>
                                  </div>
                                )}
                              </div>
                            </div>
                          </div>
                        </motion.div>
                      )}
                    </AnimatePresence>
                  </motion.div>
                );
              })
            )}
          </AnimatePresence>
        </div>
      </div>
    </section>
  );
}
