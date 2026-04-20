import { Crown, Check } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";

interface PremiumModalProps {
  open: boolean;
  onClose: () => void;
  onUpgrade: () => void;
  onProceedToPayment: () => void;
}

export function PremiumModal({ open, onClose, onUpgrade, onProceedToPayment }: PremiumModalProps) {
  const features = [
    "Acceso a todos los benchmarks avanzados",
    "Suite completa de pruebas automatizadas",
    "Análisis térmico detallado",
    "Pruebas de estrés prolongadas",
    "Exportación de resultados en PDF",
    "Comparación con base de datos global",
    "Soporte técnico prioritario",
    "Actualizaciones anticipadas",
  ];

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[520px] bg-[#1c2030] border-2 border-amber-500/30 text-white">
        <DialogHeader>
          <div className="flex items-center justify-center mb-4">
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-r from-amber-500/50 to-orange-500/50 rounded-full blur-2xl" />
              <div className="relative w-20 h-20 bg-gradient-to-br from-amber-500 to-orange-600 rounded-full flex items-center justify-center border-2 border-amber-400/50 shadow-lg shadow-amber-500/50">
                <Crown className="w-10 h-10 text-white" />
              </div>
            </div>
          </div>
          <DialogTitle className="text-center text-3xl bg-gradient-to-r from-amber-400 to-orange-400 bg-clip-text text-transparent tracking-[2px]">
            CORE CARE PREMIUM
          </DialogTitle>
          <DialogDescription className="text-center text-gray-400 text-base font-normal">
            Desbloquea todo el potencial de tu sistema
          </DialogDescription>
        </DialogHeader>

        <div className="mt-6">
          <div className="relative rounded-lg p-8 mb-6 text-center overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-br from-amber-500/20 to-orange-500/20 border-2 border-amber-500/30" />
            <div className="relative">
              <div className="text-5xl font-black text-white mb-2">
                $9.99
              </div>
              <div className="text-sm text-amber-400 tracking-[2px] font-normal">
                POR MES
              </div>
            </div>
          </div>

          <div className="space-y-3 mb-6 max-h-64 overflow-y-auto pr-2">
            {features.map((feature, index) => (
              <div key={index} className="flex items-start gap-3 group">
                <div className="w-5 h-5 rounded-full bg-gradient-to-br from-green-500/30 to-emerald-500/30 flex items-center justify-center flex-shrink-0 mt-0.5 border border-green-500/30 group-hover:border-green-400/50 transition-colors">
                  <Check className="w-3 h-3 text-green-400" />
                </div>
                <span className="text-sm text-gray-300 font-normal">
                  {feature}
                </span>
              </div>
            ))}
          </div>

          <Button
            onClick={onProceedToPayment}
            className="w-full bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-400 hover:to-orange-500 text-white border border-amber-500/50 shadow-lg shadow-amber-500/30 h-12 tracking-[1px]"
            size="lg"
          >
            <Crown className="w-5 h-5 mr-2" />
            ACTUALIZAR A PREMIUM
          </Button>

          <p className="text-xs text-center text-gray-500 mt-4 font-normal">
            Cancela en cualquier momento. Sin compromisos.
          </p>
        </div>
      </DialogContent>
    </Dialog>
  );
}