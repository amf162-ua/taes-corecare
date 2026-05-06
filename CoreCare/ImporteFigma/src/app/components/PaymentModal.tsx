import { useState } from "react";
import { CreditCard, Lock, ArrowLeft, CheckCircle2 } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { motion } from "motion/react";

interface PaymentModalProps {
  open: boolean;
  onClose: () => void;
  onPaymentSuccess: () => void;
  onBack: () => void;
}

export function PaymentModal({ open, onClose, onPaymentSuccess, onBack }: PaymentModalProps) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [cardNumber, setCardNumber] = useState("");
  const [cardName, setCardName] = useState("");
  const [expiryDate, setExpiryDate] = useState("");
  const [cvv, setCvv] = useState("");

  const formatCardNumber = (value: string) => {
    const numbers = value.replace(/\D/g, "");
    const groups = numbers.match(/.{1,4}/g);
    return groups ? groups.join(" ") : numbers;
  };

  const formatExpiryDate = (value: string) => {
    const numbers = value.replace(/\D/g, "");
    if (numbers.length >= 2) {
      return numbers.slice(0, 2) + "/" + numbers.slice(2, 4);
    }
    return numbers;
  };

  const handleCardNumberChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatCardNumber(e.target.value);
    if (formatted.replace(/\s/g, "").length <= 16) {
      setCardNumber(formatted);
    }
  };

  const handleExpiryChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatExpiryDate(e.target.value);
    if (formatted.replace(/\D/g, "").length <= 4) {
      setExpiryDate(formatted);
    }
  };

  const handleCvvChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const numbers = e.target.value.replace(/\D/g, "");
    if (numbers.length <= 3) {
      setCvv(numbers);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsProcessing(true);
    
    // Simular procesamiento de pago
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    setIsProcessing(false);
    onPaymentSuccess();
  };

  const isFormValid = 
    cardNumber.replace(/\s/g, "").length === 16 &&
    cardName.trim() !== "" &&
    expiryDate.length === 5 &&
    cvv.length === 3;

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px] bg-[#1c2030] border-2 border-cyan-500/30 text-white">
        <DialogHeader>
          <div className="flex items-center justify-between mb-4">
            <Button
              variant="ghost"
              onClick={onBack}
              className="text-cyan-400 hover:text-cyan-300 hover:bg-cyan-500/10 p-2"
            >
              <ArrowLeft className="w-5 h-5" />
            </Button>
            
            <div className="flex items-center justify-center flex-1">
              <div className="relative">
                <div className="absolute inset-0 bg-gradient-to-r from-cyan-500/50 to-blue-500/50 rounded-full blur-2xl" />
                <div className="relative w-16 h-16 bg-gradient-to-br from-cyan-500 to-blue-600 rounded-full flex items-center justify-center border-2 border-cyan-400/50 shadow-lg shadow-cyan-500/50">
                  <Lock className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>
            
            <div className="w-10" /> {/* Spacer para centrar */}
          </div>
          
          <DialogTitle className="text-center text-2xl bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent tracking-[2px]">
            PAGO SEGURO
          </DialogTitle>
          <DialogDescription className="text-center text-gray-400 text-sm font-normal">
            Ingresa los datos de tu tarjeta para completar la suscripción
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="mt-6 space-y-5">
          {/* Card Number */}
          <div className="space-y-2">
            <Label htmlFor="cardNumber" className="text-sm text-cyan-400 tracking-[1px] font-normal">
              NÚMERO DE TARJETA
            </Label>
            <div className="relative">
              <CreditCard className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-cyan-400/50" />
              <Input
                id="cardNumber"
                type="text"
                placeholder="1234 5678 9012 3456"
                value={cardNumber}
                onChange={handleCardNumberChange}
                className="pl-11 bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-600 h-12 focus:border-cyan-500 focus:ring-cyan-500/20"
              />
            </div>
          </div>

          {/* Cardholder Name */}
          <div className="space-y-2">
            <Label htmlFor="cardName" className="text-sm text-cyan-400 tracking-[1px] font-normal">
              NOMBRE DEL TITULAR
            </Label>
            <Input
              id="cardName"
              type="text"
              placeholder="Juan Pérez"
              value={cardName}
              onChange={(e) => setCardName(e.target.value.toUpperCase())}
              className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-600 h-12 focus:border-cyan-500 focus:ring-cyan-500/20"
            />
          </div>

          {/* Expiry and CVV */}
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="expiry" className="text-sm text-cyan-400 tracking-[1px] font-normal">
                FECHA EXP.
              </Label>
              <Input
                id="expiry"
                type="text"
                placeholder="MM/AA"
                value={expiryDate}
                onChange={handleExpiryChange}
                className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-600 h-12 focus:border-cyan-500 focus:ring-cyan-500/20"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="cvv" className="text-sm text-cyan-400 tracking-[1px] font-normal">
                CVV
              </Label>
              <Input
                id="cvv"
                type="text"
                placeholder="123"
                value={cvv}
                onChange={handleCvvChange}
                className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-600 h-12 focus:border-cyan-500 focus:ring-cyan-500/20"
              />
            </div>
          </div>

          {/* Price Summary */}
          <div className="relative rounded-lg p-4 bg-gradient-to-br from-cyan-500/10 to-blue-500/10 border border-cyan-500/30">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-gray-400 font-normal">Suscripción Premium</span>
              <span className="text-lg font-bold text-white">$9.99</span>
            </div>
            <div className="flex items-center justify-between border-t border-cyan-500/20 pt-2">
              <span className="text-xs text-gray-500 font-normal">Total a pagar hoy</span>
              <span className="text-2xl font-black text-cyan-400">$9.99</span>
            </div>
          </div>

          {/* Security Notice */}
          <div className="flex items-center gap-2 p-3 bg-cyan-500/5 border border-cyan-500/20 rounded-lg">
            <Lock className="w-4 h-4 text-cyan-400 flex-shrink-0" />
            <p className="text-xs text-gray-400 font-normal">
              Tu pago está protegido con encriptación de nivel bancario
            </p>
          </div>

          {/* Submit Button */}
          <Button
            type="submit"
            disabled={!isFormValid || isProcessing}
            className="w-full bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white border border-cyan-500/50 shadow-lg shadow-cyan-500/30 h-12 tracking-[1px] disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isProcessing ? (
              <motion.div
                className="flex items-center gap-2"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
              >
                <motion.div
                  className="w-5 h-5 border-2 border-white border-t-transparent rounded-full"
                  animate={{ rotate: 360 }}
                  transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
                />
                PROCESANDO PAGO...
              </motion.div>
            ) : (
              <>
                <CheckCircle2 className="w-5 h-5 mr-2" />
                PAGAR $9.99/MES
              </>
            )}
          </Button>

          <p className="text-xs text-center text-gray-500 font-normal">
            Al confirmar el pago, aceptas nuestros términos de servicio y política de cancelación
          </p>
        </form>
      </DialogContent>
    </Dialog>
  );
}
