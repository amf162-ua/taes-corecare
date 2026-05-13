import { Github, Twitter, Mail, Heart } from "lucide-react";

export function Footer() {
  return (
    <footer className="py-12 px-6 border-t border-[#00D9FF]/20 bg-[#0f1419]">
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          <div>
            <h3 className="text-xl mb-4">
              <span className="text-[#FF8C42]">Core</span>
              <span className="text-[#00D9FF]">Care</span>
            </h3>
            <p className="text-gray-400 text-sm">
              La plataforma de análisis de hardware más avanzada, potenciada por inteligencia artificial.
            </p>
          </div>

          <div>
            <h4 className="mb-4 text-white">Producto</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Características</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Precios</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Roadmap</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Changelog</a></li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-white">Recursos</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Documentación</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Guías</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Blog</a></li>
              <li><a href="#" className="text-gray-400 hover:text-[#00D9FF] transition-colors">Soporte</a></li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-white">Comunidad</h4>
            <div className="flex gap-4">
              <a href="#" className="w-10 h-10 rounded-lg bg-[#00D9FF]/20 hover:bg-[#00D9FF]/30 flex items-center justify-center transition-colors">
                <Github className="w-5 h-5 text-[#00D9FF]" />
              </a>
              <a href="#" className="w-10 h-10 rounded-lg bg-[#00D9FF]/20 hover:bg-[#00D9FF]/30 flex items-center justify-center transition-colors">
                <Twitter className="w-5 h-5 text-[#00D9FF]" />
              </a>
              <a href="#" className="w-10 h-10 rounded-lg bg-[#00D9FF]/20 hover:bg-[#00D9FF]/30 flex items-center justify-center transition-colors">
                <Mail className="w-5 h-5 text-[#00D9FF]" />
              </a>
            </div>
          </div>
        </div>

        <div className="pt-8 border-t border-[#00D9FF]/20 flex flex-col md:flex-row justify-between items-center gap-4">
          <p className="text-sm text-gray-400">
            © 2026 CoreCare. Todos los derechos reservados.
          </p>
          <p className="text-sm text-gray-400 flex items-center gap-2">
            Hecho con <Heart className="w-4 h-4 text-[#FF8C42] fill-[#FF8C42]" /> para la comunidad tech
          </p>
        </div>
      </div>
    </footer>
  );
}
