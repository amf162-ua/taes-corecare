import { Activity, BarChart3, Cpu, Zap } from "lucide-react";
import logo from "figma:asset/ff462b39e40c4c3fb04568a1948b4399bbf34bc1.png";
import { motion } from "motion/react";

interface HeroSectionProps {
  onCategoryClick?: (categoryId: string) => void;
  hasUser?: boolean;
}

export function HeroSection({ onCategoryClick, hasUser }: HeroSectionProps) {
  return (
    <section className="relative py-24 overflow-hidden">
      {/* Animated background */}
      <div className="absolute inset-0 bg-gradient-to-br from-[#0a0e1a] via-[#1a1f35] to-[#0a0e1a]" />
      
      {/* Grid pattern overlay */}
      <div className="absolute inset-0 opacity-20" 
        style={{
          backgroundImage: `
            linear-gradient(rgba(6, 182, 212, 0.1) 1px, transparent 1px),
            linear-gradient(90deg, rgba(6, 182, 212, 0.1) 1px, transparent 1px)
          `,
          backgroundSize: '50px 50px'
        }} 
      />

      {/* Glow effects */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-orange-500/20 rounded-full blur-[120px] animate-pulse" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-cyan-500/20 rounded-full blur-[120px] animate-pulse" style={{ animationDelay: '1s' }} />
      
      <div className="container mx-auto px-6 relative z-10">
        <div className="max-w-5xl mx-auto text-center">
          <motion.div 
            className="flex justify-center mb-8"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.8, ease: "easeOut" }}
          >
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-r from-orange-500/30 to-cyan-500/30 rounded-full blur-3xl" />
              <img src={logo} alt="Core Care" className="h-40 relative z-10 drop-shadow-2xl" />
            </div>
          </motion.div>
          
          <motion.h1 
            className="text-7xl font-black mb-4 bg-gradient-to-r from-orange-400 via-cyan-400 to-blue-500 bg-clip-text text-transparent tracking-[4px]"
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ duration: 0.8, delay: 0.2 }}
          >
            CORE CARE
          </motion.h1>

          <motion.div
            className="mb-6"
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ duration: 0.8, delay: 0.3 }}
          >
            <p className="text-cyan-400/70 text-base tracking-[3px] mb-4 font-normal">
              POWERED BY AXIOM WORKS
            </p>
          </motion.div>
          
          <motion.p 
            className="text-xl text-gray-300 mb-12 leading-relaxed max-w-3xl mx-auto font-normal"
            initial={{ y: 20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ duration: 0.8, delay: 0.4 }}
          >
            Sistema profesional de benchmarking de última generación.
            <br />
            Evaluación exhaustiva de rendimiento con precisión absoluta.
          </motion.p>

          {hasUser && (
            <motion.div 
              className="grid grid-cols-2 md:grid-cols-4 gap-6 mt-16"
              initial={{ y: 40, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ duration: 0.8, delay: 0.6 }}
            >
              {[
                { icon: Cpu, label: "CPU TEST", color: "orange", categoryId: "cpu" },
                { icon: BarChart3, label: "GPU TEST", color: "cyan", categoryId: "gpu" },
                { icon: Activity, label: "RAM TEST", color: "purple", categoryId: "ram" },
                { icon: Zap, label: "DISK TEST", color: "emerald", categoryId: "disk" }
              ].map((item, index) => {
                const Icon = item.icon;
                return (
                  <motion.div
                    key={item.label}
                    className="relative group cursor-pointer"
                    initial={{ y: 20, opacity: 0 }}
                    animate={{ y: 0, opacity: 1 }}
                    transition={{ duration: 0.5, delay: 0.7 + index * 0.1 }}
                    whileHover={{ y: -8, scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    onClick={() => onCategoryClick?.(item.categoryId)}
                  >
                    <div className={`absolute inset-0 bg-gradient-to-br from-${item.color}-500/20 to-${item.color}-600/20 rounded-xl blur-xl group-hover:blur-2xl transition-all duration-300`} />
                    <div className={`relative flex flex-col items-center gap-3 p-6 bg-[#1c2030]/80 backdrop-blur-sm rounded-xl border border-${item.color}-500/30 group-hover:border-${item.color}-400/50 transition-all duration-300`}>
                      <div className={`w-16 h-16 bg-gradient-to-br from-${item.color}-500/20 to-${item.color}-600/20 rounded-lg flex items-center justify-center border border-${item.color}-500/30 group-hover:scale-110 transition-transform duration-300`}>
                        <Icon className={`w-8 h-8 text-${item.color}-400`} />
                      </div>
                      <span className={`font-bold text-sm text-${item.color}-400 tracking-[1px]`}>
                        {item.label}
                      </span>
                    </div>
                  </motion.div>
                );
              })}
            </motion.div>
          )}
        </div>
      </div>
    </section>
  );
}