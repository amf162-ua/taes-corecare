import { Brain, Gauge, Shield, Zap, TrendingUp, FileText } from "lucide-react";
import { motion } from "motion/react";

const features = [
  {
    icon: Brain,
    title: "IA Avanzada",
    description: "Análisis inteligente que interpreta los resultados y te explica qué significan para tu uso específico."
  },
  {
    icon: Gauge,
    title: "Benchmarks Precisos",
    description: "Pruebas exhaustivas de CPU, GPU, RAM y almacenamiento con métricas profesionales."
  },
  {
    icon: Zap,
    title: "Resultados Instantáneos",
    description: "Obtén análisis completos en minutos, no en horas. Optimizado para máxima velocidad."
  },
  {
    icon: TrendingUp,
    title: "Comparativas",
    description: "Compara tu PC con miles de configuraciones similares y descubre cómo mejorar."
  },
  {
    icon: Shield,
    title: "Completamente Seguro",
    description: "Sin instalación de software sospechoso. Todo funciona en un entorno controlado."
  },
  {
    icon: FileText,
    title: "Informes Detallados",
    description: "Reportes completos con gráficos, explicaciones y recomendaciones personalizadas."
  }
];

export function Features() {
  return (
    /* He usado pt-4 para subirlo al máximo y overflow-visible para evitar cortes */
    <section className="pt-4 pb-32 px-6 relative overflow-visible">
      <div className="absolute inset-0 bg-gradient-to-b from-[#2d1b4e] to-[#0f0718]" />

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="text-center mb-16">
          {/* mb-8 separa el título de la frase de abajo; py-2 evita que el degradado se corte */}
          <h2 className="text-4xl md:text-5xl mb-8 bg-gradient-to-r from-white to-purple-300 bg-clip-text text-transparent py-2">
            ¿Por qué elegirnos? 
          </h2>
          <p className="text-xl text-gray-400 max-w-2xl mx-auto">
            La combinación perfecta entre tecnología de benchmarking y análisis con IA
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
              className="p-6 rounded-2xl bg-gradient-to-br from-[#1a0b2e] to-[#2d1b4e] border border-purple-500/20 hover:border-purple-500/40 transition-all duration-300 hover:shadow-lg hover:shadow-purple-500/20"
            >
              <div className="w-12 h-12 rounded-lg bg-purple-500/20 flex items-center justify-center mb-4">
                <feature.icon className="w-6 h-6 text-purple-400" />
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