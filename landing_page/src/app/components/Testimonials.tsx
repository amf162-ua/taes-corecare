import { Star, User } from "lucide-react";
import { motion } from "motion/react";

const testimonials = [
  {
    name: "Carlos Mendoza",
    role: "Gamer Profesional",
    content: "Increíble. La IA me explicó exactamente por qué mi PC tenía cuellos de botella y cómo solucionarlo. Mis FPS mejoraron un 40%.",
    rating: 5,
    avatar: "CM"
  },
  {
    name: "Ana Torres",
    role: "Creadora de Contenido",
    content: "Necesitaba entender si mi setup podía con edición 4K. El análisis fue super claro y me ahorró comprar componentes innecesarios.",
    rating: 5,
    avatar: "AT"
  },
  {
    name: "Miguel Ángel Ruiz",
    role: "Desarrollador",
    content: "Perfecta herramienta para optimizar entornos de desarrollo. Los reportes son técnicos pero accesibles. 10/10.",
    rating: 5,
    avatar: "MR"
  },
  {
    name: "Laura Jiménez",
    role: "Diseñadora Gráfica",
    content: "Me ayudó a identificar que mi RAM era el problema al trabajar con archivos grandes. La IA sugirió upgrades específicos y acertó.",
    rating: 5,
    avatar: "LJ"
  },
  {
    name: "David Sánchez",
    role: "Streamer",
    content: "Comparé mi setup con otros streamers y descubrí qué mejorar. Los benchmarks son super precisos y fáciles de interpretar.",
    rating: 5,
    avatar: "DS"
  },
  {
    name: "Patricia Vargas",
    role: "Ingeniera de Software",
    content: "Excelente para validar nuevas builds antes de comprar. La simulación es realista y los insights de la IA son oro puro.",
    rating: 5,
    avatar: "PV"
  }
];

export function Testimonials() {
  return (
    <section className="py-24 px-6 relative overflow-hidden">
      <div className="absolute inset-0 bg-[#0f1419]" />

      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-[#00D9FF] rounded-full blur-[200px] opacity-20" />

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="text-center mb-16">
          <h2 className="text-4xl md:text-5xl mb-4 bg-gradient-to-r from-white to-[#00D9FF] bg-clip-text text-transparent">
            Lo que dicen nuestros usuarios
          </h2>
          <p className="text-xl text-gray-400">
            Miles de personas ya confían en CoreCare
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {testimonials.map((testimonial, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              transition={{ duration: 0.5, delay: index * 0.1 }}
              viewport={{ once: true }}
              className="p-6 rounded-2xl bg-gradient-to-br from-[#1a1f2e]/50 to-[#252b3f]/50 backdrop-blur-sm border border-[#00D9FF]/20 hover:border-[#00D9FF]/40 transition-all duration-300"
            >
              <div className="flex items-center gap-4 mb-4">
                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-[#00D9FF] to-[#FF8C42] flex items-center justify-center">
                  <span className="text-white font-semibold">{testimonial.avatar}</span>
                </div>
                <div>
                  <h4 className="text-white">{testimonial.name}</h4>
                  <p className="text-sm text-[#00D9FF]">{testimonial.role}</p>
                </div>
              </div>

              <div className="flex gap-1 mb-4">
                {[...Array(testimonial.rating)].map((_, i) => (
                  <Star key={i} className="w-4 h-4 fill-[#FF8C42] text-[#FF8C42]" />
                ))}
              </div>

              <p className="text-gray-300 leading-relaxed italic">"{testimonial.content}"</p>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  );
}
