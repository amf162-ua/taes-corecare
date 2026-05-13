import { Rocket, ArrowRight } from "lucide-react";
import { motion } from "motion/react";

export function CTA() {
  return (
    <section className="py-24 px-6 relative">
      <div className="absolute inset-0 bg-gradient-to-t from-[#252b3f] to-[#0f1419]" />

      <div className="relative z-10 max-w-4xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8 }}
          viewport={{ once: true }}
          className="text-center p-12 rounded-3xl bg-gradient-to-br from-[#00D9FF]/20 to-[#FF8C42]/20 border border-[#00D9FF]/30 backdrop-blur-sm"
        >
          <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-gradient-to-br from-[#00D9FF] to-[#FF8C42] flex items-center justify-center">
            <Rocket className="w-8 h-8 text-white" />
          </div>

          <h2 className="text-4xl md:text-5xl mb-4 bg-gradient-to-r from-white to-[#00D9FF] bg-clip-text text-transparent">
            ¿Listo para descubrir el verdadero poder de tu PC?
          </h2>

          <p className="text-xl text-gray-300 mb-8 max-w-2xl mx-auto">
            Únete a miles de usuarios que ya optimizan su hardware con CoreCare
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <button className="px-8 py-4 bg-gradient-to-r from-[#00D9FF] to-[#00E5FF] hover:from-[#00E5FF] hover:to-[#3DECFF] text-[#0f1419] font-semibold rounded-xl transition-all duration-300 shadow-lg shadow-[#00D9FF]/50 hover:shadow-[#00E5FF]/60 flex items-center justify-center gap-2 border border-[#00D9FF]/20">
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
