import { Rocket, ArrowRight } from "lucide-react";
import { motion } from "motion/react";

export function CTA() {
  return (
    <section className="py-24 px-6 relative">
      <div className="absolute inset-0 bg-gradient-to-t from-[#2d1b4e] to-[#0f0718]" />

      <div className="relative z-10 max-w-4xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8 }}
          viewport={{ once: true }}
          className="text-center p-12 rounded-3xl bg-gradient-to-br from-purple-600/20 to-purple-800/20 border border-purple-500/30 backdrop-blur-sm"
        >
          <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-gradient-to-br from-purple-600 to-purple-400 flex items-center justify-center">
            <Rocket className="w-8 h-8 text-white" />
          </div>

          <h2 className="text-4xl md:text-5xl mb-4 bg-gradient-to-r from-white to-purple-300 bg-clip-text text-transparent">
            ¿Listo para descubrir el verdadero poder de tu PC?
          </h2>

          <p className="text-xl text-gray-300 mb-8 max-w-2xl mx-auto">
            Únete a miles de usuarios que ya optimizan su hardware con nuestra plataforma de benchmark inteligente
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button className="px-8 py-4 bg-gradient-to-r from-purple-600 to-purple-500 hover:from-purple-500 hover:to-purple-400 text-white rounded-xl transition-all duration-300 shadow-lg shadow-purple-500/50 hover:shadow-purple-400/50 flex items-center justify-center gap-2">
              Comenzar Ahora
              <ArrowRight className="w-5 h-5" />
            </button>
            <button className="px-8 py-4 bg-white/10 hover:bg-white/20 text-white rounded-xl transition-all duration-300 backdrop-blur-sm border border-white/20">
              Ver Demo
            </button>
          </div>

          <p className="mt-6 text-sm text-gray-400">
            Resultados instantáneos • Análisis detallados
          </p>
        </motion.div>
      </div>
    </section>
  );
}
