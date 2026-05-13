import { Brain, Gauge, Shield, Zap, TrendingUp, FileText, Activity, MessageSquare } from "lucide-react";
import { motion } from "motion/react";

const features = [
  {
    icon: Activity,
    title: "Monitoreo en Tiempo Real",
    description: "Supervisa CPU, GPU, RAM y discos con sensores avanzados. Visualiza temperatura, carga y frecuencia al instante."
  },
  {
    icon: Gauge,
    title: "Benchmarks Profesionales",
    description: "Tests exhaustivos de rendimiento con historial y comparación. Identifica el verdadero potencial de tu hardware."
  },
  {
    icon: Brain,
    title: "IA que te Entiende",
    description: "Chat integrado que explica resultados complejos en lenguaje simple. Sin necesidad de conocimientos técnicos."
  },
  {
    icon: TrendingUp,
    title: "Asesor de Upgrades",
    description: "Recomendaciones inteligentes de mejoras. Detecta cuellos de botella y sugiere actualizaciones específicas."
  },
  {
    icon: FileText,
    title: "Reportes en PDF",
    description: "Genera informes completos con análisis histórico, tendencias y degradación de componentes."
  },
  {
    icon: Shield,
    title: "Panel de Control Completo",
    description: "Dashboard interactivo con KPIs, heatmaps y gestión de procesos. Control total de tu sistema."
  }
];

export function Features() {
  return (
    <section className="pt-56 pb-32 px-6 relative -mt-32">
      <div className="absolute inset-0 bg-gradient-to-b from-[#252b3f] to-[#0f1419]" />

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="text-center mb-16">
          <h2 className="text-4xl md:text-5xl mb-4 bg-gradient-to-r from-white to-[#00D9FF] bg-clip-text text-transparent">
            Características Principales
          </h2>
          <p className="text-xl text-gray-400 max-w-2xl mx-auto">
            Monitoreo avanzado, benchmarking profesional y análisis con IA en una sola aplicación
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {features.map((feature, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: index * 0.1 }}
              viewport={{ once: true }}
              className="p-6 rounded-2xl bg-gradient-to-br from-[#1a1f2e] to-[#252b3f] border border-[#00D9FF]/20 hover:border-[#00D9FF]/40 transition-all duration-300 hover:shadow-lg hover:shadow-[#00D9FF]/20"
            >
              <div className="w-12 h-12 rounded-lg bg-[#00D9FF]/20 flex items-center justify-center mb-4">
                <feature.icon className="w-6 h-6 text-[#00D9FF]" />
              </div>
              <h3 className="text-xl mb-2 text-white">{feature.title}</h3>
              <p className="text-gray-400 leading-relaxed">{feature.description}</p>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  );
}
