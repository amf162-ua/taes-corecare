import { Shield, Users, MessageSquare, Settings } from "lucide-react";
import { motion } from "motion/react";

export function AdminSection() {
  return (
    <section className="py-24 px-6 relative overflow-hidden">
      <div className="absolute inset-0 bg-gradient-to-b from-[#0f1419] to-[#1a1f2e]" />

      <div className="relative z-10 max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            whileInView={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.8 }}
            viewport={{ once: true }}
          >
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-[#00D9FF]/20 border border-[#00D9FF]/30 mb-6">
              <Shield className="w-5 h-5 text-[#00D9FF]" />
              <span className="text-[#00D9FF]">Para Administradores</span>
            </div>

            <h2 className="text-4xl md:text-5xl mb-6 bg-gradient-to-r from-white to-[#00D9FF] bg-clip-text text-transparent">
              Control total de tu organización
            </h2>

            <p className="text-xl text-gray-300 mb-8 leading-relaxed">
              Panel administrativo completo para gestionar múltiples equipos, usuarios y accesos desde un solo lugar.
            </p>

            <div className="space-y-6">
              <div className="flex gap-4">
                <div className="w-12 h-12 rounded-lg bg-[#00D9FF]/20 flex items-center justify-center flex-shrink-0">
                  <Users className="w-6 h-6 text-[#00D9FF]" />
                </div>
                <div>
                  <h3 className="text-lg mb-2 text-white">Gestión de Usuarios y Perfiles</h3>
                  <p className="text-gray-400">
                    Crea y administra múltiples perfiles de usuario con roles personalizados. Asigna permisos de administrador o cliente según sea necesario.
                  </p>
                </div>
              </div>

              <div className="flex gap-4">
                <div className="w-12 h-12 rounded-lg bg-[#00D9FF]/20 flex items-center justify-center flex-shrink-0">
                  <MessageSquare className="w-6 h-6 text-[#00D9FF]" />
                </div>
                <div>
                  <h3 className="text-lg mb-2 text-white">Chat Integrado</h3>
                  <p className="text-gray-400">
                    Sistema de mensajería interno para comunicación directa entre administradores y clientes. Comparte análisis y recomendaciones fácilmente.
                  </p>
                </div>
              </div>

              <div className="flex gap-4">
                <div className="w-12 h-12 rounded-lg bg-[#00D9FF]/20 flex items-center justify-center flex-shrink-0">
                  <Settings className="w-6 h-6 text-[#00D9FF]" />
                </div>
                <div>
                  <h3 className="text-lg mb-2 text-white">Panel de Administración</h3>
                  <p className="text-gray-400">
                    Acceso a funciones administrativas avanzadas. Visualiza datos de todos los equipos y toma decisiones informadas basadas en métricas consolidadas.
                  </p>
                </div>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, x: 20 }}
            whileInView={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.8 }}
            viewport={{ once: true }}
            className="relative"
          >
            <div className="relative rounded-2xl border border-[#00D9FF]/30 bg-gradient-to-br from-[#1a1f2e] to-[#252b3f] p-8 backdrop-blur-sm">
              <div className="absolute -top-3 -right-3 w-24 h-24 bg-[#00D9FF] rounded-full blur-[80px] opacity-50" />
              <div className="absolute -bottom-3 -left-3 w-32 h-32 bg-[#FF8C42] rounded-full blur-[100px] opacity-50" />

              <div className="relative space-y-4">
                <div className="p-4 rounded-lg bg-[#1a1f2e]/80 border border-[#00D9FF]/20">
                  <div className="flex items-center gap-3 mb-2">
                    <div className="w-8 h-8 rounded-full bg-[#00D9FF]" />
                    <div>
                      <p className="text-sm text-white">Admin: Carlos Mendoza</p>
                      <p className="text-xs text-gray-400">En línea</p>
                    </div>
                  </div>
                  <p className="text-sm text-gray-300">5 equipos monitoreados</p>
                </div>

                <div className="p-4 rounded-lg bg-[#1a1f2e]/80 border border-[#00D9FF]/20">
                  <div className="flex items-center gap-3 mb-2">
                    <div className="w-8 h-8 rounded-full bg-[#FF8C42]" />
                    <div>
                      <p className="text-sm text-white">Cliente: Ana Torres</p>
                      <p className="text-xs text-gray-400">Activa hace 5 min</p>
                    </div>
                  </div>
                  <p className="text-sm text-gray-300">1 equipo monitoreado</p>
                </div>

                <div className="p-4 rounded-lg bg-[#1a1f2e]/80 border border-[#00D9FF]/20">
                  <div className="flex items-center gap-3 mb-2">
                    <MessageSquare className="w-6 h-6 text-[#00D9FF]" />
                    <p className="text-sm text-white">3 mensajes nuevos</p>
                  </div>
                  <p className="text-xs text-gray-400">Chat interno disponible</p>
                </div>
              </div>
            </div>
          </motion.div>
        </div>
      </div>
    </section>
  );
}
