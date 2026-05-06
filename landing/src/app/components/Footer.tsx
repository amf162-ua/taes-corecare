import { Github, Twitter, Mail, Heart } from "lucide-react";

export function Footer() {
  return (
    <footer className="py-12 px-6 border-t border-purple-500/20 bg-[#0f0718]">
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          <div>
            <h3 className="text-xl mb-4 text-purple-400">CoreCare</h3>
            <p className="text-gray-400 text-sm">
              La plataforma de análisis de hardware más avanzada, potenciada por inteligencia artificial.
            </p>
          </div>

          <div>
            <h4 className="mb-4 text-white">Producto</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Características</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Precios</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Roadmap</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Changelog</a></li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-white">Recursos</h4>
            <ul className="space-y-2 text-sm">
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Documentación</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Guías</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Blog</a></li>
              <li><a href="#" className="text-gray-400 hover:text-purple-400 transition-colors">Soporte</a></li>
            </ul>
          </div>

          <div>
            <h4 className="mb-4 text-white">Comunidad</h4>
            <div className="flex gap-4">
              <a href="#" className="w-10 h-10 rounded-lg bg-purple-500/20 hover:bg-purple-500/30 flex items-center justify-center transition-colors">
                <Github className="w-5 h-5 text-purple-400" />
              </a>
              <a href="#" className="w-10 h-10 rounded-lg bg-purple-500/20 hover:bg-purple-500/30 flex items-center justify-center transition-colors">
                <Twitter className="w-5 h-5 text-purple-400" />
              </a>
              <a href="#" className="w-10 h-10 rounded-lg bg-purple-500/20 hover:bg-purple-500/30 flex items-center justify-center transition-colors">
                <Mail className="w-5 h-5 text-purple-400" />
              </a>
            </div>
          </div>
        </div>

        <div className="pt-8 border-t border-purple-500/20 flex flex-col md:flex-row justify-between items-center gap-4">
          <p className="text-sm text-gray-400">
            © 2026 CoreCare. Todos los derechos reservados.
          </p>
          <p className="text-sm text-gray-400 flex items-center gap-2">
            Hecho con <Heart className="w-4 h-4 text-purple-500 fill-purple-500" /> para la comunidad tech
          </p>
        </div>
      </div>
    </footer>
  );
}
