import { motion, AnimatePresence } from "motion/react";
import { X, Send, Building2, FileText, CheckCircle2 } from "lucide-react";
import { Button } from "./ui/button";
import { useState } from "react";

interface ReportModalProps {
  isOpen: boolean;
  onClose: () => void;
  benchmarkName?: string;
  benchmarkId?: string;
  onReportSubmitted?: (report: {
    benchmarkName: string;
    description: string;
    companyId: string;
    companyName: string;
    companyIcon: string;
  }) => void;
}

const companies = [
  { id: "1", name: "AMD Support", icon: "🔴" },
  { id: "2", name: "NVIDIA Support", icon: "🟢" },
  { id: "3", name: "Intel Technical Team", icon: "🔵" },
  { id: "4", name: "Microsoft Support", icon: "🟦" },
  { id: "5", name: "CoreCare Premium Support", icon: "⭐" },
  { id: "6", name: "Hardware Diagnostics Inc.", icon: "🔧" },
];

export function ReportModal({ isOpen, onClose, benchmarkName, benchmarkId, onReportSubmitted }: ReportModalProps) {
  const [description, setDescription] = useState("");
  const [selectedCompany, setSelectedCompany] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!description.trim() || !selectedCompany) {
      return;
    }

    setIsSubmitting(true);

    // Simular envío del reporte
    setTimeout(() => {
      setIsSubmitting(false);
      setIsSuccess(true);

      // Después de 2 segundos, cerrar el modal
      setTimeout(() => {
        setIsSuccess(false);
        setDescription("");
        setSelectedCompany("");
        onClose();
      }, 2000);
    }, 1500);
  };

  const handleClose = () => {
    if (!isSubmitting && !isSuccess) {
      setDescription("");
      setSelectedCompany("");
      onClose();
    }
  };

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleClose}
          />

          {/* Modal */}
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              className="bg-[#1c2030] rounded-2xl border border-cyan-500/30 shadow-2xl shadow-cyan-500/10 max-w-2xl w-full max-h-[90vh] overflow-hidden"
              initial={{ scale: 0.9, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.9, opacity: 0, y: 20 }}
              onClick={(e) => e.stopPropagation()}
            >
              {!isSuccess ? (
                <>
                  {/* Header */}
                  <div className="relative bg-gradient-to-r from-red-500/10 via-orange-500/10 to-red-500/10 border-b border-red-500/30 p-6">
                    <div className="absolute inset-0 bg-gradient-to-r from-red-500/5 to-orange-500/5" />
                    <div className="relative flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-gradient-to-br from-red-500/20 to-orange-500/20 rounded-xl flex items-center justify-center border border-red-500/30">
                          <FileText className="w-6 h-6 text-red-400" />
                        </div>
                        <div>
                          <h2 className="text-2xl font-black text-white tracking-wide">
                            ENVIAR REPORTE
                          </h2>
                          <p className="text-sm text-gray-400 font-normal">
                            Benchmark: {benchmarkName}
                          </p>
                        </div>
                      </div>
                      <Button
                        onClick={handleClose}
                        disabled={isSubmitting}
                        variant="ghost"
                        className="text-gray-400 hover:text-white hover:bg-white/5"
                      >
                        <X className="w-5 h-5" />
                      </Button>
                    </div>
                  </div>

                  {/* Form */}
                  <form onSubmit={handleSubmit} className="p-6 space-y-6 overflow-y-auto max-h-[calc(90vh-200px)]">
                    {/* Description */}
                    <div className="space-y-2">
                      <label className="flex items-center gap-2 text-sm font-bold text-cyan-400 tracking-wide">
                        <FileText className="w-4 h-4" />
                        DESCRIBE EL PROBLEMA
                      </label>
                      <textarea
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        placeholder="Explica detalladamente qué problema has experimentado durante el benchmark..."
                        required
                        rows={6}
                        disabled={isSubmitting}
                        className="w-full bg-[#151a2e] border border-cyan-500/30 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-cyan-500/50 focus:ring-2 focus:ring-cyan-500/20 transition-all resize-none font-normal disabled:opacity-50 disabled:cursor-not-allowed"
                      />
                      <p className="text-xs text-gray-500 font-normal">
                        Mínimo 10 caracteres • Incluye detalles sobre el comportamiento anormal
                      </p>
                    </div>

                    {/* Company Selection */}
                    <div className="space-y-3">
                      <label className="flex items-center gap-2 text-sm font-bold text-cyan-400 tracking-wide">
                        <Building2 className="w-4 h-4" />
                        EMPRESA ASOCIADA
                      </label>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {companies.map((company) => (
                          <motion.button
                            key={company.id}
                            type="button"
                            onClick={() => setSelectedCompany(company.id)}
                            disabled={isSubmitting}
                            whileHover={{ scale: isSubmitting ? 1 : 1.02 }}
                            whileTap={{ scale: isSubmitting ? 1 : 0.98 }}
                            className={`p-4 rounded-xl border-2 transition-all text-left disabled:opacity-50 disabled:cursor-not-allowed ${
                              selectedCompany === company.id
                                ? "bg-cyan-500/20 border-cyan-500/50 shadow-lg shadow-cyan-500/20"
                                : "bg-[#151a2e] border-cyan-500/20 hover:border-cyan-500/40"
                            }`}
                          >
                            <div className="flex items-center gap-3">
                              <span className="text-2xl">{company.icon}</span>
                              <div className="flex-1">
                                <p className={`font-bold ${
                                  selectedCompany === company.id ? "text-cyan-400" : "text-white"
                                }`}>
                                  {company.name}
                                </p>
                                <p className="text-xs text-gray-500 font-normal">
                                  Soporte técnico especializado
                                </p>
                              </div>
                              {selectedCompany === company.id && (
                                <motion.div
                                  initial={{ scale: 0 }}
                                  animate={{ scale: 1 }}
                                  className="w-6 h-6 bg-cyan-500 rounded-full flex items-center justify-center"
                                >
                                  <CheckCircle2 className="w-4 h-4 text-white" />
                                </motion.div>
                              )}
                            </div>
                          </motion.button>
                        ))}
                      </div>
                    </div>

                    {/* Info Box */}
                    <div className="bg-orange-500/10 border border-orange-500/30 rounded-xl p-4">
                      <div className="flex gap-3">
                        <div className="flex-shrink-0">
                          <div className="w-8 h-8 bg-orange-500/20 rounded-lg flex items-center justify-center">
                            <FileText className="w-4 h-4 text-orange-400" />
                          </div>
                        </div>
                        <div>
                          <h4 className="font-bold text-orange-400 mb-1">
                            Información del reporte
                          </h4>
                          <p className="text-sm text-gray-400 font-normal">
                            El reporte incluirá automáticamente los datos del benchmark ({benchmarkName}), 
                            resultados técnicos y tu descripción del problema. La empresa seleccionada 
                            recibirá la solicitud y te contactará para ayudarte.
                          </p>
                        </div>
                      </div>
                    </div>
                  </form>

                  {/* Footer */}
                  <div className="border-t border-cyan-500/20 p-6 flex items-center justify-end gap-3 bg-[#151a2e]/50">
                    <Button
                      type="button"
                      onClick={handleClose}
                      disabled={isSubmitting}
                      variant="ghost"
                      className="text-gray-400 hover:text-white border border-gray-500/30 hover:border-gray-400/50"
                    >
                      Cancelar
                    </Button>
                    <Button
                      onClick={handleSubmit}
                      disabled={!description.trim() || !selectedCompany || isSubmitting}
                      className="bg-gradient-to-r from-cyan-500 to-blue-500 hover:from-cyan-400 hover:to-blue-400 text-white font-bold px-6 border-0 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {isSubmitting ? (
                        <>
                          <motion.div
                            className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full mr-2"
                            animate={{ rotate: 360 }}
                            transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
                          />
                          Enviando...
                        </>
                      ) : (
                        <>
                          <Send className="w-4 h-4 mr-2" />
                          Enviar Reporte
                        </>
                      )}
                    </Button>
                  </div>
                </>
              ) : (
                /* Success State */
                <div className="p-12">
                  <motion.div
                    className="text-center"
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ type: "spring", duration: 0.5 }}
                  >
                    <div className="w-20 h-20 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-full flex items-center justify-center mx-auto mb-6 border border-green-500/30">
                      <CheckCircle2 className="w-10 h-10 text-green-400" />
                    </div>
                    <h3 className="text-2xl font-black text-white mb-3 tracking-wide">
                      ¡REPORTE ENVIADO!
                    </h3>
                    <p className="text-gray-400 font-normal">
                      Tu solicitud ha sido enviada exitosamente. La empresa seleccionada se pondrá en contacto contigo pronto.
                    </p>
                  </motion.div>
                </div>
              )}
            </motion.div>
          </div>
        </>
      )}
    </AnimatePresence>
  );
}