import { Download } from "lucide-react";
import { motion } from "motion/react";

export function Hero() {
  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-br from-[#0f0718] via-[#1a0b2e] to-[#2d1b4e]" />

      <div className="absolute inset-0 opacity-30">
        <div className="absolute top-20 left-20 w-72 h-72 bg-purple-500 rounded-full blur-[128px]" />
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-purple-600 rounded-full blur-[128px]" />
      </div>

      <div className="relative z-10 max-w-6xl mx-auto px-6 py-20 text-center">
        <motion.h1
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8 }}
          className="mb-6"
        >
          <span className="block text-6xl md:text-7xl lg:text-8xl bg-gradient-to-r from-white via-purple-200 to-purple-400 bg-clip-text text-transparent">
            CoreCare
          </span>
          <span className="block mt-4 text-4xl md:text-5xl lg:text-6xl text-purple-300">
            Benchmark Inteligente para tu PC
          </span>
        </motion.h1>

        <motion.p
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.1 }}
          className="text-xl md:text-2xl text-gray-300 mb-12 max-w-3xl mx-auto leading-relaxed"
        >
          Analiza el rendimiento de tu hardware y <span className="text-purple-300 font-medium">entiende cada detalle técnico sin necesidad de ser un experto</span>.
          Nuestra IA te explica todo en lenguaje simple.
        </motion.p>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, delay: 0.2 }}
          className="flex justify-center"
        >
          <button className="px-10 py-5 bg-gradient-to-r from-purple-600 to-purple-500 hover:from-purple-500 hover:to-purple-400 text-white rounded-xl transition-all duration-300 shadow-lg shadow-purple-500/50 hover:shadow-purple-400/50 flex items-center justify-center gap-3 text-lg">
            <Download className="w-6 h-6" />
            Descargar Ahora
          </button>
        </motion.div>
      </div>
    </section>
  );
}
