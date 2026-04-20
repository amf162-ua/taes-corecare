import { Activity, BarChart3, Cpu, HardDrive, Zap, Crown, Clock, ChevronDown, ChevronUp, Thermometer } from "lucide-react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { motion, AnimatePresence } from "motion/react";
import { useState, useEffect, useRef } from "react";

interface CategoryBenchmarksProps {
  onRunBenchmark: (benchmarkName: string) => void;
  isPremium: boolean;
  initialExpandedCategory?: string | null;
}

const categories = [
  {
    id: "cpu",
    name: "CPU TEST",
    description: "Pruebas de procesador",
    icon: Cpu,
    color: "orange",
    tests: [
      {
        id: "cpu-basic",
        name: "CPU - Test Básico",
        description: "Evaluación rápida del rendimiento del procesador",
        duration: "~2 min",
        premium: false,
      },
      {
        id: "cpu-advanced",
        name: "CPU - Test Avanzado",
        description: "Análisis completo con multihilo y estrés térmico",
        duration: "~10 min",
        premium: true,
      },
    ],
  },
  {
    id: "gpu",
    name: "GPU TEST",
    description: "Pruebas de tarjeta gráfica",
    icon: BarChart3,
    color: "cyan",
    tests: [
      {
        id: "gpu-basic",
        name: "GPU - Test Básico",
        description: "Rendimiento gráfico en resoluciones estándar",
        duration: "~3 min",
        premium: false,
      },
      {
        id: "gpu-advanced",
        name: "GPU - Test Avanzado",
        description: "Ray tracing, 4K y evaluación de VRAM",
        duration: "~15 min",
        premium: true,
      },
    ],
  },
  {
    id: "ram",
    name: "RAM TEST",
    description: "Pruebas de memoria",
    icon: Activity,
    color: "purple",
    tests: [
      {
        id: "ram-basic",
        name: "RAM - Test Básico",
        description: "Velocidad de lectura y escritura de memoria",
        duration: "~1 min",
        premium: false,
      },
      {
        id: "ram-advanced",
        name: "RAM - Test Avanzado",
        description: "Latencia, estabilidad y prueba de errores",
        duration: "~5 min",
        premium: true,
      },
    ],
  },
  {
    id: "disk",
    name: "DISK TEST",
    description: "Pruebas de almacenamiento",
    icon: HardDrive,
    color: "emerald",
    tests: [
      {
        id: "disk-basic",
        name: "Disco - Test Básico",
        description: "Velocidad secuencial de lectura/escritura",
        duration: "~2 min",
        premium: false,
      },
      {
        id: "disk-advanced",
        name: "Disco - Test Avanzado",
        description: "IOPS, acceso aleatorio y análisis de latencia",
        duration: "~8 min",
        premium: true,
      },
    ],
  },
  {
    id: "advanced",
    name: "TESTS AVANZADOS",
    description: "Pruebas especializadas",
    icon: Zap,
    color: "amber",
    tests: [
      {
        id: "thermal",
        name: "Prueba Térmica",
        description: "Monitoreo de temperaturas bajo carga sostenida",
        duration: "~20 min",
        premium: true,
      },
      {
        id: "complete",
        name: "Suite Completa",
        description: "Todos los benchmarks en secuencia automática",
        duration: "~45 min",
        premium: true,
      },
    ],
  },
];

const colorClasses: Record<string, any> = {
  orange: {
    gradient: "from-orange-500/20 to-orange-600/20",
    border: "border-orange-500/30",
    borderHover: "border-orange-400/50",
    text: "text-orange-400",
    glow: "shadow-orange-500/20",
  },
  cyan: {
    gradient: "from-cyan-500/20 to-cyan-600/20",
    border: "border-cyan-500/30",
    borderHover: "border-cyan-400/50",
    text: "text-cyan-400",
    glow: "shadow-cyan-500/20",
  },
  purple: {
    gradient: "from-purple-500/20 to-purple-600/20",
    border: "border-purple-500/30",
    borderHover: "border-purple-400/50",
    text: "text-purple-400",
    glow: "shadow-purple-500/20",
  },
  emerald: {
    gradient: "from-emerald-500/20 to-emerald-600/20",
    border: "border-emerald-500/30",
    borderHover: "border-emerald-400/50",
    text: "text-emerald-400",
    glow: "shadow-emerald-500/20",
  },
  amber: {
    gradient: "from-amber-500/20 to-amber-600/20",
    border: "border-amber-500/30",
    borderHover: "border-amber-400/50",
    text: "text-amber-400",
    glow: "shadow-amber-500/20",
  },
  red: {
    gradient: "from-red-500/20 to-red-600/20",
    border: "border-red-500/30",
    borderHover: "border-red-400/50",
    text: "text-red-400",
    glow: "shadow-red-500/20",
  },
};

export function CategoryBenchmarks({ onRunBenchmark, isPremium, initialExpandedCategory }: CategoryBenchmarksProps) {
  const [expandedCategory, setExpandedCategory] = useState<string | null>(initialExpandedCategory || null);
  const sectionRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (initialExpandedCategory) {
      setExpandedCategory(initialExpandedCategory);
      // Scroll to section with a small delay to ensure rendering
      setTimeout(() => {
        sectionRef.current?.scrollIntoView({ behavior: "smooth", block: "center" });
      }, 200);
    }
  }, [initialExpandedCategory]);

  const toggleCategory = (categoryId: string) => {
    setExpandedCategory(expandedCategory === categoryId ? null : categoryId);
  };

  return (
    <section ref={sectionRef} className="relative py-16 overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-b from-[#0a0e1a] via-[#151a2e] to-[#0a0e1a]" />
      
      {/* Grid pattern */}
      <div className="absolute inset-0 opacity-10" 
        style={{
          backgroundImage: `
            linear-gradient(rgba(6, 182, 212, 0.2) 1px, transparent 1px),
            linear-gradient(90deg, rgba(6, 182, 212, 0.2) 1px, transparent 1px)
          `,
          backgroundSize: '40px 40px'
        }} 
      />

      {/* Animated glow effects */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-orange-500/10 rounded-full blur-[120px] animate-pulse" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-cyan-500/10 rounded-full blur-[120px] animate-pulse" style={{ animationDelay: '1s' }} />
      
      <div className="container mx-auto px-6 relative z-10">
        <motion.div 
          className="text-center mb-12"
          initial={{ y: 20, opacity: 0 }}
          whileInView={{ y: 0, opacity: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          <h2 className="text-5xl font-black mb-4 bg-gradient-to-r from-orange-400 via-cyan-400 to-blue-500 bg-clip-text text-transparent tracking-[3px]">
            BENCHMARKS DISPONIBLES
          </h2>
          <div className="h-1 w-32 bg-gradient-to-r from-transparent via-cyan-500 to-transparent mx-auto mb-6" />
          <p className="text-gray-400 text-lg font-normal">
            Selecciona una categoría para ver todos sus tests disponibles
          </p>
        </motion.div>

        <div className="max-w-5xl mx-auto space-y-4">
          {categories.map((category, index) => {
            const Icon = category.icon;
            const isExpanded = expandedCategory === category.id;
            const colors = colorClasses[category.color];

            return (
              <motion.div
                key={category.id}
                initial={{ y: 20, opacity: 0 }}
                whileInView={{ y: 0, opacity: 1 }}
                viewport={{ once: true }}
                transition={{ duration: 0.5, delay: index * 0.1 }}
              >
                {/* Category Header */}
                <div 
                  className={`relative group cursor-pointer`}
                  onClick={() => toggleCategory(category.id)}
                >
                  <div className={`absolute inset-0 bg-gradient-to-br ${colors.gradient} rounded-xl blur-xl opacity-0 group-hover:opacity-100 transition-all duration-500`} />
                  
                  <div className={`relative p-6 rounded-xl border ${isExpanded ? colors.borderHover : colors.border} bg-[#1c2030]/80 backdrop-blur-sm hover:bg-[#1c2030] transition-all duration-300`}>
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className={`w-14 h-14 rounded-xl bg-gradient-to-br ${colors.gradient} border ${colors.border} flex items-center justify-center`}>
                          <Icon className={`w-7 h-7 ${colors.text}`} />
                        </div>
                        <div>
                          <h3 className={`font-bold text-xl ${colors.text} tracking-[1px]`}>
                            {category.name}
                          </h3>
                          <p className="text-sm text-gray-400 font-normal mt-1">
                            {category.description}
                          </p>
                        </div>
                      </div>
                      
                      <motion.div
                        animate={{ rotate: isExpanded ? 180 : 0 }}
                        transition={{ duration: 0.3 }}
                      >
                        <ChevronDown className={`w-6 h-6 ${colors.text}`} />
                      </motion.div>
                    </div>
                  </div>
                </div>

                {/* Expanded Tests */}
                <AnimatePresence>
                  {isExpanded && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: "auto", opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.3 }}
                      className="overflow-hidden"
                    >
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 px-4">
                        {category.tests.map((test, testIndex) => {
                          const isLocked = test.premium && !isPremium;

                          return (
                            <motion.div
                              key={test.id}
                              initial={{ x: -20, opacity: 0 }}
                              animate={{ x: 0, opacity: 1 }}
                              transition={{ duration: 0.3, delay: testIndex * 0.1 }}
                              className={`relative p-5 rounded-lg border ${colors.border} bg-[#1a1f35]/60 backdrop-blur-sm ${
                                isLocked ? "opacity-60" : ""
                              }`}
                            >
                              {test.premium && (
                                <Badge
                                  variant="secondary"
                                  className="absolute top-3 right-3 bg-gradient-to-r from-amber-500/20 to-amber-600/20 text-amber-400 border border-amber-500/30 backdrop-blur-sm text-[10px] tracking-[1px]"
                                >
                                  <Crown className="w-3 h-3 mr-1" />
                                  PREMIUM
                                </Badge>
                              )}

                              <h4 className={`font-bold text-base mb-2 ${colors.text}`}>
                                {test.name}
                              </h4>
                              <p className="text-sm text-gray-400 mb-3 font-normal">
                                {test.description}
                              </p>

                              <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2 text-sm text-gray-500">
                                  <Clock className="w-4 h-4" />
                                  <span className="font-normal">{test.duration}</span>
                                </div>

                                <Button
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    onRunBenchmark(test.name);
                                  }}
                                  disabled={isLocked}
                                  size="sm"
                                  className={`${
                                    isLocked
                                      ? "bg-gray-700/50 hover:bg-gray-700/50 cursor-not-allowed border border-gray-600/30"
                                      : `bg-gradient-to-r ${colors.gradient} hover:scale-105 border ${colors.border} ${colors.text} hover:shadow-lg ${colors.glow}`
                                  } transition-all duration-300`}
                                >
                                  {isLocked ? "PREMIUM" : "EJECUTAR"}
                                </Button>
                              </div>
                            </motion.div>
                          );
                        })}
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </motion.div>
            );
          })}
        </div>
      </div>
    </section>
  );
}