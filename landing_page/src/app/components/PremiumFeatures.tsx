import { Crown, Bell, Users, CreditCard, BarChart3, History } from "lucide-react";
import { motion } from "motion/react";

const premiumFeatures = [
  {
    icon: BarChart3,
    title: "Análisis Histórico Ilimitado",
    description: "Accede a todo el historial de telemetría y benchmarks sin límites de tiempo."
  },
  {
    icon: Bell,
    title: "Alertas Personalizadas",
    description: "Notificaciones inteligentes sobre temperaturas, rendimiento y anomalías del sistema."
  },
  {
    icon: History,
    title: "Reportes Avanzados",
    description: "Exporta análisis de degradación, tendencias y comparativas en PDF de calidad profesional."
  },
  {
    icon: Users,
    title: "Múltiples Perfiles",
    description: "Gestiona varios equipos y usuarios con roles personalizados de administrador y cliente."
  }
];

export function PremiumFeatures() {
  return (
    <section className="py-24 px-6 relative">
      <div className="absolute inset-0 bg-gradient-to-b from-[#0f1419] via-[#1a1f2e] to-[#0f1419]" />

      <div className="absolute inset-0 opacity-20">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-[#FF8C42] rounded-full blur-[150px]" />
      </div>

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="text-center mb-16">
          <h2 className="text-4xl md:text-5xl mb-4 bg-gradient-to-r from-white to-[#FF8C42] bg-clip-text text-transparent">
            Características Avanzadas
          </h2>
          <p className="text-xl text-gray-400 max-w-2xl mx-auto">
            Herramientas profesionales para análisis profundo de tu hardware
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {premiumFeatures.map((feature, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: index * 0.1 }}
              viewport={{ once: true }}
              className="p-6 rounded-2xl bg-gradient-to-br from-[#1a1f2e]/50 to-[#252b3f]/50 border border-[#FF8C42]/30 hover:border-[#FF8C42]/50 transition-all duration-300"
            >
              <div className="w-12 h-12 rounded-lg bg-gradient-to-br from-[#FF8C42] to-[#FFB07C] flex items-center justify-center mb-4">
                <feature.icon className="w-6 h-6 text-white" />
              </div>
              <h3 className="text-lg mb-2 text-white">{feature.title}</h3>
              <p className="text-gray-400 text-sm leading-relaxed">{feature.description}</p>
            </motion.div>
          ))}
        </div>

      </div>
    </section>
  );
}
