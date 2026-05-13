import { Download, Zap } from "lucide-react";
import { motion } from "motion/react";

export function Hero() {
  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-br from-[#0f1419] via-[#1a1f2e] to-[#252b3f]" />

      <div className="absolute inset-0 opacity-30">
        <div className="absolute top-20 left-20 w-72 h-72 bg-[#FF8C42] rounded-full blur-[128px]" />
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-[#00D9FF] rounded-full blur-[128px]" />
      </div>

      <div className="relative z-10 max-w-6xl mx-auto px-6 py-20 text-center">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6 }}
          className="flex justify-center mb-8"
        >
          <Zap className="w-16 h-16 text-[#FF8C42]" />
        </motion.div>

        <motion.h1
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8 }}
          className="mb-6"
        >
          <span className="block text-6xl md:text-7xl lg:text-8xl">
            <span className="bg-gradient-to-r from-[#FF8C42] to-[#FFB07C] bg-clip-text text-transparent">CORE </span>
            <span className="bg-gradient-to-r from-[#00D9FF] to-[#3DECFF] bg-clip-text text-transparent">CARE</span>
          </span>
          <span className="block mt-4 text-3xl md:text-4xl lg:text-5xl text-gray-300">
            Benchmark Inteligente para tu PC
          </span>
        </motion.h1>

        <motion.p
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.1 }}
          className="text-xl md:text-2xl text-gray-300 mb-12 max-w-3xl mx-auto leading-relaxed"
        >
          Monitoreo avanzado, benchmarking profesional y análisis con IA. <span className="text-[#00D9FF] font-medium">Entiende cada detalle técnico sin necesidad de ser un experto</span>.
          Nuestra IA te explica todo en lenguaje simple.
        </motion.p>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.2 }}
          className="flex justify-center"
        >
          <button className="px-10 py-5 bg-gradient-to-r from-[#00D9FF] to-[#00E5FF] hover:from-[#00E5FF] hover:to-[#3DECFF] text-[#0f1419] font-semibold rounded-xl transition-all duration-300 shadow-lg shadow-[#00D9FF]/50 hover:shadow-[#00E5FF]/60 flex items-center justify-center gap-3 text-lg border border-[#00D9FF]/20">
            <Download className="w-6 h-6" />
            Descargar Ahora
          </button>
        </motion.div>
      </div>
    </section>
  );
}
