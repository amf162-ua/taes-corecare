import { History, Trash2, Award, FileX, Cpu, BarChart3, Activity, HardDrive, Thermometer, Zap, ChevronDown, ChevronUp, AlertTriangle, TrendingUp, ShoppingCart, FileText } from "lucide-react";
import { Button } from "./ui/button";
import { BenchmarkResult } from "./BenchmarkProgress";
import { motion, AnimatePresence } from "motion/react";
import { useState } from "react";
import { LineChart, Line, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { ReportModal } from "./ReportModal";

interface BenchmarkHistoryProps {
  results: BenchmarkResult[];
  onClearHistory: () => void;
  isPremium: boolean;
}

// Función para determinar el color según el valor y tipo de métrica
const getMetricColor = (value: number, type: string): string => {
  switch (type) {
    case 'duration':
      // Duración: < 20s = verde, 20-30s = blanco, 30-40s = naranja, > 40s = rojo
      if (value < 20) return 'text-green-400';
      if (value < 30) return 'text-white';
      if (value < 40) return 'text-orange-400';
      return 'text-red-400';
    
    case 'avgCpuLoad':
    case 'cpuUsage':
      // CPU: < 60% = verde, 60-80% = blanco, 80-90% = naranja, > 90% = rojo
      if (value < 60) return 'text-green-400';
      if (value < 80) return 'text-white';
      if (value < 90) return 'text-orange-400';
      return 'text-red-400';
    
    case 'avgRamLoad':
    case 'ramUsage':
      // RAM: < 50% = verde, 50-70% = blanco, 70-85% = naranja, > 85% = rojo
      if (value < 50) return 'text-green-400';
      if (value < 70) return 'text-white';
      if (value < 85) return 'text-orange-400';
      return 'text-red-400';
    
    case 'peakCpuTemp':
      // Temp CPU: < 60°C = verde, 60-75°C = blanco, 75-85°C = naranja, > 85°C = rojo
      if (value < 60) return 'text-green-400';
      if (value < 75) return 'text-white';
      if (value < 85) return 'text-orange-400';
      return 'text-red-400';
    
    case 'peakGpuTemp':
    case 'temperature':
      // Temp GPU: < 65°C = verde, 65-80°C = blanco, 80-90°C = naranja, > 90°C = rojo
      if (value < 65) return 'text-green-400';
      if (value < 80) return 'text-white';
      if (value < 90) return 'text-orange-400';
      return 'text-red-400';
    
    case 'score':
      // Score: > 12000 = verde, 8000-12000 = blanco, 5000-8000 = naranja, < 5000 = rojo
      if (value > 12000) return 'text-green-400';
      if (value > 8000) return 'text-white';
      if (value > 5000) return 'text-orange-400';
      return 'text-red-400';
    
    case 'cpuClock':
      // CPU Clock: > 4000 MHz = verde, 3500-4000 = blanco, 3000-3500 = naranja, < 3000 = rojo
      if (value > 4000) return 'text-green-400';
      if (value > 3500) return 'text-white';
      if (value > 3000) return 'text-orange-400';
      return 'text-red-400';
    
    case 'gpuClock':
      // GPU Clock: > 1800 MHz = verde, 1500-1800 = blanco, 1200-1500 = naranja, < 1200 = rojo
      if (value > 1800) return 'text-green-400';
      if (value > 1500) return 'text-white';
      if (value > 1200) return 'text-orange-400';
      return 'text-red-400';
    
    case 'ramUsed':
      // RAM Usada: < 8 GB = verde, 8-12 GB = blanco, 12-14 GB = naranja, > 14 GB = rojo
      if (value < 8) return 'text-green-400';
      if (value < 12) return 'text-white';
      if (value < 14) return 'text-orange-400';
      return 'text-red-400';
    
    case 'diskLoad':
      // Disk Load: < 40% = verde, 40-60% = blanco, 60-80% = naranja, > 80% = rojo
      if (value < 40) return 'text-green-400';
      if (value < 60) return 'text-white';
      if (value < 80) return 'text-orange-400';
      return 'text-red-400';
    
    case 'diskReadRate':
      // Disk Read: > 2000 MB/s = verde, 500-2000 = blanco, 200-500 = naranja, < 200 = rojo
      if (value > 2000) return 'text-green-400';
      if (value > 500) return 'text-white';
      if (value > 200) return 'text-orange-400';
      return 'text-red-400';
    
    case 'diskWriteRate':
      // Disk Write: > 1500 MB/s = verde, 400-1500 = blanco, 150-400 = naranja, < 150 = rojo
      if (value > 1500) return 'text-green-400';
      if (value > 400) return 'text-white';
      if (value > 150) return 'text-orange-400';
      return 'text-red-400';
    
    default:
      return 'text-white';
  }
};

// Función para determinar si el resultado es crítico
const isCriticalScore = (result: BenchmarkResult): boolean => {
  return result.score < 8000 || result.temperature > 80 || result.cpuUsage > 90 || result.ramUsage > 90;
};

// Función para generar descripción con IA
const generateAIDescription = (result: BenchmarkResult): string => {
  const isCritical = isCriticalScore(result);
  
  if (result.name.toLowerCase().includes("cpu")) {
    if (isCritical) {
      if (result.temperature > 80) {
        return "⚠️ El procesador está operando a temperaturas críticas. Se recomienda mejorar el sistema de refrigeración inmediatamente. El rendimiento actual está siendo limitado por el thermal throttling.";
      }
      if (result.cpuUsage > 90) {
        return "⚠️ Uso de CPU extremadamente alto detectado. El procesador está trabajando al límite de su capacidad. Considera actualizar a un modelo más potente para aplicaciones exigentes.";
      }
      return "⚠️ El rendimiento del CPU está por debajo del promedio esperado. Se recomienda verificar procesos en segundo plano y considerar una actualización de hardware.";
    }
    return "✓ El procesador funciona dentro de parámetros normales. Rendimiento estable y temperaturas controladas. Ideal para tareas multitarea y aplicaciones de uso general.";
  } else if (result.name.toLowerCase().includes("gpu")) {
    if (isCritical) {
      return "⚠️ La tarjeta gráfica muestra bajo rendimiento. Considera actualizar los drivers o mejorar el hardware para aplicaciones gráficas intensivas y gaming de última generación.";
    }
    return "✓ La GPU presenta un rendimiento óptimo. Capaz de manejar cargas gráficas intensivas con facilidad. Temperaturas estables durante la prueba.";
  } else if (result.name.toLowerCase().includes("ram")) {
    if (isCritical) {
      return "⚠️ Uso crítico de memoria RAM detectado. Se recomienda expandir la capacidad de memoria o cerrar aplicaciones innecesarias para evitar cuellos de botella.";
    }
    return "✓ La memoria RAM funciona correctamente con tiempos de acceso óptimos. Suficiente capacidad disponible para multitarea eficiente.";
  } else if (result.name.toLowerCase().includes("disco")) {
    if (isCritical) {
      return "⚠️ Velocidades de lectura/escritura por debajo del estándar. Considera migrar a un SSD NVMe para mejoras significativas en rendimiento del sistema.";
    }
    return "✓ El almacenamiento presenta velocidades de transferencia adecuadas. Los tiempos de carga son óptimos para la mayoría de aplicaciones.";
  } else if (result.name.toLowerCase().includes("térmica") || result.name.toLowerCase().includes("termica")) {
    if (isCritical) {
      return "🔥 ALERTA CRÍTICA: Temperaturas peligrosamente altas detectadas. Detén el uso intensivo inmediatamente y revisa el sistema de refrigeración. Riesgo de daño permanente al hardware.";
    }
    return "✓ Las temperaturas del sistema están dentro de rangos seguros. El sistema de refrigeración funciona eficientemente bajo carga.";
  }
  
  return "✓ Los resultados del benchmark indican un funcionamiento normal del sistema. No se detectaron anomalías significativas.";
};

// Función para generar recomendaciones de piezas
const generateRecommendations = (result: BenchmarkResult): Array<{ name: string; price: string; reason: string }> => {
  const recommendations: Array<{ name: string; price: string; reason: string }> = [];
  
  if (result.name.toLowerCase().includes("cpu")) {
    if (isCriticalScore(result)) {
      recommendations.push(
        { name: "AMD Ryzen 9 7950X", price: "$549", reason: "16 núcleos/32 hilos, ideal para tareas multi-thread" },
        { name: "Intel Core i9-14900K", price: "$589", reason: "Máximo rendimiento single-core y gaming" },
        { name: "Noctua NH-D15", price: "$109", reason: "Refrigeración premium para reducir temperaturas" }
      );
    } else {
      recommendations.push(
        { name: "AMD Ryzen 7 7800X3D", price: "$449", reason: "Excelente para gaming con caché 3D" },
        { name: "Arctic Liquid Freezer II 280", price: "$119", reason: "Refrigeración líquida eficiente" }
      );
    }
  } else if (result.name.toLowerCase().includes("gpu")) {
    if (isCriticalScore(result)) {
      recommendations.push(
        { name: "NVIDIA RTX 4090", price: "$1,599", reason: "Máximo rendimiento gráfico disponible" },
        { name: "AMD Radeon RX 7900 XTX", price: "$999", reason: "Excelente relación precio/rendimiento" },
        { name: "EVGA SuperNOVA 1000W", price: "$189", reason: "PSU potente para tarjetas high-end" }
      );
    } else {
      recommendations.push(
        { name: "NVIDIA RTX 4070 Ti", price: "$799", reason: "Balance perfecto para 1440p/4K" },
        { name: "AMD Radeon RX 7800 XT", price: "$499", reason: "Gran rendimiento para gaming" }
      );
    }
  } else if (result.name.toLowerCase().includes("ram")) {
    if (isCriticalScore(result)) {
      recommendations.push(
        { name: "G.Skill Trident Z5 64GB DDR5", price: "$269", reason: "64GB DDR5-6000 para multitarea extrema" },
        { name: "Corsair Vengeance 32GB DDR5", price: "$139", reason: "32GB DDR5-5600 confiable y rápida" }
      );
    } else {
      recommendations.push(
        { name: "Kingston Fury Beast 32GB DDR4", price: "$89", reason: "Upgrade económico y eficiente" }
      );
    }
  } else if (result.name.toLowerCase().includes("disco")) {
    if (isCriticalScore(result)) {
      recommendations.push(
        { name: "Samsung 990 Pro 2TB NVMe", price: "$179", reason: "7,450 MB/s lectura - Máxima velocidad" },
        { name: "WD Black SN850X 2TB", price: "$159", reason: "PCIe 4.0 con excelente rendimiento" }
      );
    } else {
      recommendations.push(
        { name: "Samsung 980 Pro 1TB", price: "$89", reason: "Balance perfecto precio/rendimiento" }
      );
    }
  }
  
  return recommendations;
};

// Generar datos para gráfica
const generateChartData = (result: BenchmarkResult) => {
  // Simular datos de progreso durante el benchmark
  return [
    { time: "0s", score: 0, cpu: 0, ram: 0, temp: 40 },
    { time: "5s", score: Math.floor(result.score * 0.2), cpu: Math.floor(result.cpuUsage * 0.3), ram: Math.floor(result.ramUsage * 0.2), temp: 45 },
    { time: "10s", score: Math.floor(result.score * 0.4), cpu: Math.floor(result.cpuUsage * 0.6), ram: Math.floor(result.ramUsage * 0.5), temp: 55 },
    { time: "15s", score: Math.floor(result.score * 0.7), cpu: Math.floor(result.cpuUsage * 0.9), ram: Math.floor(result.ramUsage * 0.8), temp: 68 },
    { time: "20s", score: Math.floor(result.score * 0.95), cpu: result.cpuUsage, ram: result.ramUsage, temp: result.temperature - 5 },
    { time: "25s", score: result.score, cpu: Math.floor(result.cpuUsage * 0.95), ram: Math.floor(result.ramUsage * 0.9), temp: result.temperature },
  ];
};

// Función para obtener el estilo según el tipo de benchmark
const getBenchmarkStyle = (name: string) => {
  if (name.toLowerCase().includes("cpu")) {
    return {
      icon: Cpu,
      gradient: "from-orange-500/20 to-red-500/20",
      border: "border-orange-500/30",
      borderHover: "border-orange-500/50",
      text: "text-orange-400",
    };
  } else if (name.toLowerCase().includes("gpu")) {
    return {
      icon: Zap,
      gradient: "from-green-500/20 to-emerald-500/20",
      border: "border-green-500/30",
      borderHover: "border-green-500/50",
      text: "text-green-400",
    };
  } else if (name.toLowerCase().includes("ram")) {
    return {
      icon: Activity,
      gradient: "from-purple-500/20 to-pink-500/20",
      border: "border-purple-500/30",
      borderHover: "border-purple-500/50",
      text: "text-purple-400",
    };
  } else if (name.toLowerCase().includes("disco")) {
    return {
      icon: HardDrive,
      gradient: "from-blue-500/20 to-cyan-500/20",
      border: "border-blue-500/30",
      borderHover: "border-blue-500/50",
      text: "text-blue-400",
    };
  } else if (name.toLowerCase().includes("térmica") || name.toLowerCase().includes("termica")) {
    return {
      icon: Thermometer,
      gradient: "from-red-500/20 to-orange-500/20",
      border: "border-red-500/30",
      borderHover: "border-red-500/50",
      text: "text-red-400",
    };
  }
  return {
    icon: Award,
    gradient: "from-cyan-500/20 to-blue-500/20",
    border: "border-cyan-500/30",
    borderHover: "border-cyan-500/50",
    text: "text-cyan-400",
  };
};

export function BenchmarkHistory({ results, onClearHistory, isPremium }: BenchmarkHistoryProps) {
  const [expandedResults, setExpandedResults] = useState<string[]>([]);
  const [reportModalOpen, setReportModalOpen] = useState(false);
  const [selectedBenchmark, setSelectedBenchmark] = useState<{ id: string; name: string } | null>(null);

  const toggleResult = (id: string) => {
    if (expandedResults.includes(id)) {
      setExpandedResults(expandedResults.filter(resultId => resultId !== id));
    } else {
      setExpandedResults([...expandedResults, id]);
    }
  };

  const handleOpenReport = (id: string, name: string) => {
    setSelectedBenchmark({ id, name });
    setReportModalOpen(true);
  };

  const handleCloseReport = () => {
    setReportModalOpen(false);
    setSelectedBenchmark(null);
  };

  return (
    <section className="relative py-16 overflow-hidden min-h-[70vh]">
      <div className="absolute inset-0 bg-gradient-to-b from-[#0a0e1a] via-[#151a2e] to-[#0a0e1a]" />
      
      <div className="container mx-auto px-6 relative z-10">
        <motion.div 
          className="text-center mb-12"
          initial={{ y: 20, opacity: 0 }}
          whileInView={{ y: 0, opacity: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          <div className="flex items-center justify-center gap-3 mb-4">
            <History className="w-8 h-8 text-cyan-400" />
            <h2 className="text-4xl font-black text-white tracking-[3px]">
              HISTORIAL
            </h2>
          </div>
          <div className="h-1 w-32 bg-gradient-to-r from-transparent via-cyan-500 to-transparent mx-auto mb-4" />
          
          {results.length > 0 && (
            <div className="flex justify-center">
              <Button
                onClick={onClearHistory}
                variant="ghost"
                className="text-red-400 hover:text-red-300 hover:bg-red-500/10 border border-red-500/30"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Limpiar Historial
              </Button>
            </div>
          )}
        </motion.div>

        {results.length === 0 ? (
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.5 }}
            className="max-w-md mx-auto text-center py-16"
          >
            <div className="w-24 h-24 bg-gradient-to-br from-cyan-500/20 to-blue-500/20 rounded-full flex items-center justify-center mx-auto mb-6 border border-cyan-500/30">
              <FileX className="w-12 h-12 text-cyan-400" />
            </div>
            <h3 className="text-2xl font-bold text-white mb-3">
              No hay resultados aún
            </h3>
            <p className="text-gray-400 font-normal">
              Ejecuta algunos benchmarks para ver tu historial de pruebas aquí
            </p>
          </motion.div>
        ) : (
          <div className="max-w-6xl mx-auto space-y-4">
            {results.map((result, index) => {
              const style = getBenchmarkStyle(result.name);
              const Icon = style.icon;
              const isCritical = isCriticalScore(result);
              const isExpanded = expandedResults.includes(result.id);
              const aiDescription = generateAIDescription(result);
              const recommendations = generateRecommendations(result);
              
              return (
                <motion.div
                  key={result.id}
                  initial={{ x: -20, opacity: 0 }}
                  whileInView={{ x: 0, opacity: 1 }}
                  viewport={{ once: true }}
                  transition={{ duration: 0.4, delay: index * 0.05 }}
                  className={`relative bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border ${
                    isCritical ? 'border-red-500/50 shadow-lg shadow-red-500/20' : style.border
                  } hover:${style.borderHover} p-6 transition-all duration-300`}
                >
                  {/* Critical Badge */}
                  {isCritical && (
                    <div className="absolute -top-3 -right-3 z-10">
                      <div className="bg-red-500 text-white px-3 py-1 rounded-full flex items-center gap-2 shadow-lg shadow-red-500/50 animate-pulse">
                        <AlertTriangle className="w-4 h-4" />
                        <span className="text-xs font-bold">CRÍTICO</span>
                      </div>
                    </div>
                  )}

                  <div className="flex items-center justify-between flex-wrap gap-4">
                    <div className="flex items-center gap-4">
                      <div className={`w-12 h-12 bg-gradient-to-br ${style.gradient} rounded-lg flex items-center justify-center border ${style.border}`}>
                        <Icon className={`w-6 h-6 ${style.text}`} />
                      </div>
                      <div>
                        <h3 className={`font-bold text-lg ${style.text}`}>{result.name}</h3>
                        <p className="text-sm text-gray-400 font-normal">{result.date}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-6">
                      <div className="text-center">
                        <p className="text-xs text-gray-500 mb-1 font-normal">Score</p>
                        <p className={`text-xl font-black ${style.text}`}>{result.score.toLocaleString()}</p>
                      </div>
                      <div className="text-center">
                        <p className="text-xs text-gray-500 mb-1 font-normal">CPU</p>
                        <p className="text-xl font-black text-orange-400">{result.cpuUsage}%</p>
                      </div>
                      <div className="text-center">
                        <p className="text-xs text-gray-500 mb-1 font-normal">RAM</p>
                        <p className="text-xl font-black text-purple-400">{result.ramUsage}%</p>
                      </div>
                      <div className="text-center">
                        <p className="text-xs text-gray-500 mb-1 font-normal">Temp</p>
                        <p className="text-xl font-black text-red-400">{result.temperature}°C</p>
                      </div>
                    </div>
                  </div>

                  {/* AI Description - Always visible */}
                  <div className="mt-4 p-4 bg-[#151a2e]/60 rounded-lg border border-cyan-500/20">
                    <div className="flex items-start gap-3">
                      <TrendingUp className={`w-5 h-5 mt-0.5 ${isCritical ? 'text-red-400' : 'text-cyan-400'}`} />
                      <div className="flex-1">
                        <p className="text-sm text-gray-300 font-normal leading-relaxed">
                          {aiDescription}
                        </p>
                      </div>
                    </div>
                  </div>

                  {/* Send Report Button - Only for critical benchmarks */}
                  {isCritical && (
                    <div className="mt-4">
                      <Button
                        onClick={() => handleOpenReport(result.id, result.name)}
                        className="w-full bg-gradient-to-r from-red-500 to-orange-500 hover:from-red-400 hover:to-orange-400 text-white font-bold border-0 shadow-lg shadow-red-500/20"
                      >
                        <FileText className="w-4 h-4 mr-2" />
                        Enviar Reporte a Empresa Asociada
                      </Button>
                    </div>
                  )}

                  {/* Premium: Expandable Details */}
                  {isPremium && (
                    <>
                      <div className="mt-4">
                        <Button
                          onClick={() => toggleResult(result.id)}
                          variant="ghost"
                          className={`w-full ${style.text} hover:bg-gray-500/10 border ${style.border}`}
                        >
                          {isExpanded ? (
                            <>
                              <ChevronUp className="w-4 h-4 mr-2" />
                              Ocultar Detalles Avanzados
                            </>
                          ) : (
                            <>
                              <ChevronDown className="w-4 h-4 mr-2" />
                              Ver Detalles Avanzados
                            </>
                          )}
                        </Button>
                      </div>

                      <AnimatePresence>
                        {isExpanded && (
                          <motion.div
                            initial={{ height: 0, opacity: 0 }}
                            animate={{ height: "auto", opacity: 1 }}
                            exit={{ height: 0, opacity: 0 }}
                            transition={{ duration: 0.3 }}
                            className="overflow-hidden"
                          >
                            <div className="mt-4 space-y-6">
                              {/* Detalles Técnicos del Benchmark */}
                              <div className="bg-[#151a2e]/60 backdrop-blur-sm rounded-xl border border-gray-500/20 p-6">
                                <h4 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
                                  <Activity className="w-5 h-5 text-cyan-400" />
                                  Detalles del Estudio
                                </h4>
                                <div className="grid grid-cols-2 gap-4">
                                  {/* Baseline Readings - Lecturas Base */}
                                  <div className="col-span-2">
                                    <h5 className="text-sm font-bold text-cyan-400 mb-3 uppercase tracking-wider">
                                      Lecturas Base (Baseline)
                                    </h5>
                                    <div className="grid grid-cols-3 gap-3">
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-cyan-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Duración</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.duration, 'duration')}`}>{result.duration}s</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-cyan-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Carga CPU Promedio</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.avgCpuLoad, 'avgCpuLoad')}`}>{result.avgCpuLoad}%</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-cyan-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Carga RAM Promedio</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.avgRamLoad, 'avgRamLoad')}`}>{result.avgRamLoad}%</p>
                                      </div>
                                    </div>
                                  </div>

                                  {/* Stress Test Readings - Lecturas de Estrés */}
                                  <div className="col-span-2">
                                    <h5 className="text-sm font-bold text-orange-400 mb-3 uppercase tracking-wider mt-4">
                                      Lecturas de Estrés (Stress Test)
                                    </h5>
                                    <div className="grid grid-cols-3 gap-3">
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-orange-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Temp. Pico CPU</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.peakCpuTemp, 'peakCpuTemp')}`}>{result.peakCpuTemp}°C</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-orange-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Temp. Pico GPU</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.peakGpuTemp, 'peakGpuTemp')}`}>{result.peakGpuTemp}°C</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-orange-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Score Final</p>
                                        <p className={`text-lg font-bold ${getMetricColor(result.score, 'score')}`}>{result.score.toLocaleString()}</p>
                                      </div>
                                    </div>
                                  </div>

                                  {/* Registry Details - Detalles del Registro */}
                                  <div className="col-span-2">
                                    <h5 className="text-sm font-bold text-amber-400 mb-3 uppercase tracking-wider mt-4">
                                      Registro del Sistema (RegistroBenchmark)
                                    </h5>
                                    <div className="grid grid-cols-4 gap-3">
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">CPU Clock</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.cpuClock, 'cpuClock')}`}>{result.cpuClock} MHz</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">GPU Clock</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.gpuClock, 'gpuClock')}`}>{result.gpuClock} MHz</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">RAM Usada</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.ramUsed, 'ramUsed')}`}>{result.ramUsed} GB</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Carga Disco</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.diskLoad, 'diskLoad')}`}>{result.diskLoad}%</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Lectura Disco</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.diskReadRate, 'diskReadRate')}`}>{result.diskReadRate} MB/s</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Escritura Disco</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.diskWriteRate, 'diskWriteRate')}`}>{result.diskWriteRate} MB/s</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Carga CPU</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.cpuUsage, 'cpuUsage')}`}>{result.cpuUsage}%</p>
                                      </div>
                                      <div className="bg-[#1c2030]/70 p-3 rounded-lg border border-amber-500/20">
                                        <p className="text-xs text-gray-400 mb-1 font-normal">Temp. Actual</p>
                                        <p className={`text-sm font-bold ${getMetricColor(result.temperature, 'temperature')}`}>{result.temperature}°C</p>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              </div>

                              {/* Performance Chart */}
                              <div className="bg-[#151a2e]/60 backdrop-blur-sm rounded-xl border border-gray-500/20 p-6">
                                <h4 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
                                  <BarChart3 className="w-5 h-5 text-cyan-400" />
                                  Gráfica de Ejecución
                                </h4>
                                <ResponsiveContainer width="100%" height={250}>
                                  <AreaChart data={generateChartData(result)}>
                                    <defs>
                                      <linearGradient id={`colorScore-${result.id}`} x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#06b6d4" stopOpacity={0.8}/>
                                        <stop offset="95%" stopColor="#06b6d4" stopOpacity={0}/>
                                      </linearGradient>
                                      <linearGradient id={`colorCpu-${result.id}`} x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#fb923c" stopOpacity={0.8}/>
                                        <stop offset="95%" stopColor="#fb923c" stopOpacity={0}/>
                                      </linearGradient>
                                      <linearGradient id={`colorTemp-${result.id}`} x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#f87171" stopOpacity={0.8}/>
                                        <stop offset="95%" stopColor="#f87171" stopOpacity={0}/>
                                      </linearGradient>
                                    </defs>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                                    <XAxis dataKey="time" stroke="#9ca3af" />
                                    <YAxis stroke="#9ca3af" />
                                    <Tooltip 
                                      contentStyle={{ 
                                        backgroundColor: '#1c2030', 
                                        border: '1px solid #374151',
                                        borderRadius: '8px'
                                      }}
                                    />
                                    <Area type="monotone" dataKey="cpu" stroke="#fb923c" fillOpacity={1} fill={`url(#colorCpu-${result.id})`} />
                                    <Area type="monotone" dataKey="temp" stroke="#f87171" fillOpacity={1} fill={`url(#colorTemp-${result.id})`} />
                                  </AreaChart>
                                </ResponsiveContainer>
                              </div>

                              {/* Recommendations */}
                              {recommendations.length > 0 && (
                                <div className="bg-[#151a2e]/60 backdrop-blur-sm rounded-xl border border-gray-500/20 p-6">
                                  <h4 className="text-lg font-bold text-white mb-4 flex items-center gap-2">
                                    <ShoppingCart className="w-5 h-5 text-amber-400" />
                                    Hardware Recomendado
                                  </h4>
                                  <div className="space-y-3">
                                    {recommendations.map((rec, idx) => (
                                      <motion.div
                                        key={idx}
                                        initial={{ x: -10, opacity: 0 }}
                                        animate={{ x: 0, opacity: 1 }}
                                        transition={{ delay: idx * 0.1 }}
                                        className="flex items-start gap-3 p-3 bg-[#1c2030]/50 rounded-lg border border-cyan-500/10 hover:border-cyan-500/30 transition-all"
                                      >
                                        <div className="w-8 h-8 bg-gradient-to-br from-cyan-500/20 to-blue-500/20 rounded-lg flex items-center justify-center flex-shrink-0">
                                          <span className="text-cyan-400 font-bold text-sm">{idx + 1}</span>
                                        </div>
                                        <div className="flex-1">
                                          <div className="flex items-center justify-between mb-1">
                                            <h5 className="font-bold text-white text-sm">{rec.name}</h5>
                                            <span className="text-amber-400 font-black text-sm">{rec.price}</span>
                                          </div>
                                          <p className="text-xs text-gray-400 font-normal">{rec.reason}</p>
                                        </div>
                                      </motion.div>
                                    ))}
                                  </div>
                                </div>
                              )}
                            </div>
                          </motion.div>
                        )}
                      </AnimatePresence>
                    </>
                  )}
                </motion.div>
              );
            })}
          </div>
        )}
      </div>

      {/* Report Modal */}
      <ReportModal
        isOpen={reportModalOpen}
        onClose={handleCloseReport}
        benchmarkId={selectedBenchmark?.id}
        benchmarkName={selectedBenchmark?.name}
      />
    </section>
  );
}