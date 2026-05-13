import { useState } from "react";
import { User, Crown, Mail, Calendar, Cpu, HardDrive, MemoryStick, Monitor, Save, Edit2, X } from "lucide-react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { motion } from "motion/react";

export interface HardwareComponents {
  cpu: string;
  gpu: string;
  ram: string;
  storage: string;
  motherboard: string;
  psu: string;
  cooling: string;
}

interface UserProfileModalProps {
  open: boolean;
  onClose: () => void;
  user: { name: string; isPremium: boolean; email?: string; registerDate?: string };
  hardware: HardwareComponents;
  onSaveHardware: (hardware: HardwareComponents) => void;
}

export function UserProfileModal({ open, onClose, user, hardware, onSaveHardware }: UserProfileModalProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editedHardware, setEditedHardware] = useState<HardwareComponents>(hardware);

  const handleSave = () => {
    onSaveHardware(editedHardware);
    setIsEditing(false);
  };

  const handleCancel = () => {
    setEditedHardware(hardware);
    setIsEditing(false);
  };

  const handleChange = (field: keyof HardwareComponents, value: string) => {
    setEditedHardware(prev => ({ ...prev, [field]: value }));
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[700px] bg-[#1c2030] border-2 border-cyan-500/30 text-white max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <div className="flex items-center justify-center mb-4">
            <div className="relative">
              <div className="absolute inset-0 bg-gradient-to-r from-cyan-500/50 to-blue-500/50 rounded-full blur-2xl" />
              <div className="relative w-20 h-20 bg-gradient-to-br from-cyan-500 to-blue-600 rounded-full flex items-center justify-center border-2 border-cyan-400/50 shadow-lg shadow-cyan-500/50">
                <User className="w-10 h-10 text-white" />
              </div>
            </div>
          </div>
          
          <DialogTitle className="text-center text-3xl bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent tracking-[2px]">
            PERFIL DE USUARIO
          </DialogTitle>
          <DialogDescription className="text-center text-gray-400 text-sm font-normal">
            Información personal y configuración de hardware
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="profile" className="mt-6">
          <TabsList className="grid w-full grid-cols-2 bg-[#0a0e1a] border border-cyan-500/30">
            <TabsTrigger 
              value="profile"
              className="data-[state=active]:bg-cyan-500/20 data-[state=active]:text-cyan-400 text-gray-400"
            >
              <User className="w-4 h-4 mr-2" />
              MI PERFIL
            </TabsTrigger>
            <TabsTrigger 
              value="hardware"
              className="data-[state=active]:bg-cyan-500/20 data-[state=active]:text-cyan-400 text-gray-400"
            >
              <Cpu className="w-4 h-4 mr-2" />
              MI HARDWARE
            </TabsTrigger>
          </TabsList>

          {/* Profile Tab */}
          <TabsContent value="profile" className="mt-6 space-y-4">
            <div className="relative rounded-lg p-6 bg-gradient-to-br from-cyan-500/10 to-blue-500/10 border border-cyan-500/30">
              <div className="space-y-4">
                {/* Username */}
                <div className="flex items-center gap-4 p-4 bg-[#0a0e1a] rounded-lg border border-cyan-500/20">
                  <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/30 to-blue-500/30 rounded-full flex items-center justify-center border border-cyan-500/30">
                    <User className="w-6 h-6 text-cyan-400" />
                  </div>
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal">NOMBRE DE USUARIO</p>
                    <p className="text-lg font-bold text-white">{user.name}</p>
                  </div>
                  {user.isPremium && (
                    <div className="flex items-center gap-2 px-3 py-1.5 bg-gradient-to-r from-amber-500/20 to-orange-500/20 rounded-full border border-amber-500/30">
                      <Crown className="w-4 h-4 text-amber-400" />
                      <span className="text-xs font-bold text-amber-400 tracking-[1px]">PREMIUM</span>
                    </div>
                  )}
                </div>

                {/* Email */}
                <div className="flex items-center gap-4 p-4 bg-[#0a0e1a] rounded-lg border border-cyan-500/20">
                  <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/30 to-blue-500/30 rounded-full flex items-center justify-center border border-cyan-500/30">
                    <Mail className="w-6 h-6 text-cyan-400" />
                  </div>
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal">EMAIL</p>
                    <p className="text-base text-gray-300">{user.email || "usuario@corecare.com"}</p>
                  </div>
                </div>

                {/* Registration Date */}
                <div className="flex items-center gap-4 p-4 bg-[#0a0e1a] rounded-lg border border-cyan-500/20">
                  <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/30 to-blue-500/30 rounded-full flex items-center justify-center border border-cyan-500/30">
                    <Calendar className="w-6 h-6 text-cyan-400" />
                  </div>
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal">MIEMBRO DESDE</p>
                    <p className="text-base text-gray-300">{user.registerDate || "25 Marzo 2026"}</p>
                  </div>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-3 gap-3 mt-6">
                  <div className="p-3 bg-[#0a0e1a] rounded-lg border border-cyan-500/20 text-center">
                    <p className="text-2xl font-black text-cyan-400">47</p>
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal mt-1">BENCHMARKS</p>
                  </div>
                  <div className="p-3 bg-[#0a0e1a] rounded-lg border border-cyan-500/20 text-center">
                    <p className="text-2xl font-black text-green-400">94%</p>
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal mt-1">ÉXITO</p>
                  </div>
                  <div className="p-3 bg-[#0a0e1a] rounded-lg border border-cyan-500/20 text-center">
                    <p className="text-2xl font-black text-amber-400">12.5k</p>
                    <p className="text-xs text-gray-500 tracking-[1px] font-normal mt-1">SCORE AVG</p>
                  </div>
                </div>
              </div>
            </div>

            {!user.isPremium && (
              <div className="p-4 bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/30 rounded-lg">
                <div className="flex items-center gap-3">
                  <Crown className="w-8 h-8 text-amber-400 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-bold text-amber-400 tracking-[1px]">ACTUALIZA A PREMIUM</p>
                    <p className="text-xs text-gray-400 mt-1 font-normal">Desbloquea análisis avanzados y funciones exclusivas</p>
                  </div>
                </div>
              </div>
            )}
          </TabsContent>

          {/* Hardware Tab */}
          <TabsContent value="hardware" className="mt-6 space-y-4">
            <div className="flex items-center justify-between mb-4">
              <p className="text-sm text-gray-400 font-normal">
                Configura los componentes de tu ordenador para análisis más precisos
              </p>
              {!isEditing ? (
                <Button
                  onClick={() => setIsEditing(true)}
                  size="sm"
                  className="bg-cyan-500/20 hover:bg-cyan-500/30 text-cyan-400 border border-cyan-500/50"
                >
                  <Edit2 className="w-4 h-4 mr-2" />
                  EDITAR
                </Button>
              ) : (
                <div className="flex gap-2">
                  <Button
                    onClick={handleCancel}
                    size="sm"
                    variant="ghost"
                    className="text-gray-400 hover:text-white hover:bg-red-500/10"
                  >
                    <X className="w-4 h-4 mr-2" />
                    CANCELAR
                  </Button>
                  <Button
                    onClick={handleSave}
                    size="sm"
                    className="bg-green-500/20 hover:bg-green-500/30 text-green-400 border border-green-500/50"
                  >
                    <Save className="w-4 h-4 mr-2" />
                    GUARDAR
                  </Button>
                </div>
              )}
            </div>

            <div className="space-y-4">
              {/* CPU */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-cyan-500/10 to-blue-500/10 border border-cyan-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/30 to-blue-500/30 rounded-lg flex items-center justify-center border border-cyan-500/30 flex-shrink-0">
                    <Cpu className="w-6 h-6 text-cyan-400" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-cyan-400 tracking-[1px] font-normal">PROCESADOR (CPU)</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.cpu}
                        onChange={(e) => handleChange('cpu', e.target.value)}
                        placeholder="Ej: Intel Core i9-13900K"
                        className="bg-[#0a0e1a] border-cyan-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.cpu || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* GPU */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-green-500/10 to-emerald-500/10 border border-green-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-green-500/30 to-emerald-500/30 rounded-lg flex items-center justify-center border border-green-500/30 flex-shrink-0">
                    <Monitor className="w-6 h-6 text-green-400" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-green-400 tracking-[1px] font-normal">TARJETA GRÁFICA (GPU)</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.gpu}
                        onChange={(e) => handleChange('gpu', e.target.value)}
                        placeholder="Ej: NVIDIA RTX 4090"
                        className="bg-[#0a0e1a] border-green-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.gpu || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* RAM */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-purple-500/10 to-pink-500/10 border border-purple-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-purple-500/30 to-pink-500/30 rounded-lg flex items-center justify-center border border-purple-500/30 flex-shrink-0">
                    <MemoryStick className="w-6 h-6 text-purple-400" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-purple-400 tracking-[1px] font-normal">MEMORIA RAM</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.ram}
                        onChange={(e) => handleChange('ram', e.target.value)}
                        placeholder="Ej: 32GB DDR5 6000MHz"
                        className="bg-[#0a0e1a] border-purple-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.ram || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* Storage */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-orange-500/10 to-amber-500/10 border border-orange-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-orange-500/30 to-amber-500/30 rounded-lg flex items-center justify-center border border-orange-500/30 flex-shrink-0">
                    <HardDrive className="w-6 h-6 text-orange-400" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-orange-400 tracking-[1px] font-normal">ALMACENAMIENTO</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.storage}
                        onChange={(e) => handleChange('storage', e.target.value)}
                        placeholder="Ej: 2TB NVMe SSD"
                        className="bg-[#0a0e1a] border-orange-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.storage || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* Motherboard */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-blue-500/10 to-indigo-500/10 border border-blue-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-500/30 to-indigo-500/30 rounded-lg flex items-center justify-center border border-blue-500/30 flex-shrink-0">
                    <Cpu className="w-6 h-6 text-blue-400" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-blue-400 tracking-[1px] font-normal">PLACA BASE</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.motherboard}
                        onChange={(e) => handleChange('motherboard', e.target.value)}
                        placeholder="Ej: ASUS ROG Maximus Z790"
                        className="bg-[#0a0e1a] border-blue-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.motherboard || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* PSU */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-yellow-500/10 to-orange-500/10 border border-yellow-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-yellow-500/30 to-orange-500/30 rounded-lg flex items-center justify-center border border-yellow-500/30 flex-shrink-0">
                    <svg className="w-6 h-6 text-yellow-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-yellow-400 tracking-[1px] font-normal">FUENTE DE ALIMENTACIÓN</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.psu}
                        onChange={(e) => handleChange('psu', e.target.value)}
                        placeholder="Ej: Corsair RM1000x 1000W"
                        className="bg-[#0a0e1a] border-yellow-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.psu || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>

              {/* Cooling */}
              <motion.div 
                className="relative rounded-lg p-4 bg-gradient-to-br from-cyan-500/10 to-teal-500/10 border border-cyan-500/30"
                whileHover={{ scale: isEditing ? 1.01 : 1 }}
                transition={{ duration: 0.2 }}
              >
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-cyan-500/30 to-teal-500/30 rounded-lg flex items-center justify-center border border-cyan-500/30 flex-shrink-0">
                    <svg className="w-6 h-6 text-cyan-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                    </svg>
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label className="text-xs text-cyan-400 tracking-[1px] font-normal">REFRIGERACIÓN</Label>
                    {isEditing ? (
                      <Input
                        value={editedHardware.cooling}
                        onChange={(e) => handleChange('cooling', e.target.value)}
                        placeholder="Ej: Refrigeración líquida AIO 360mm"
                        className="bg-[#0a0e1a] border-cyan-500/30 text-white h-10"
                      />
                    ) : (
                      <p className="text-base text-white font-medium">{hardware.cooling || "No especificado"}</p>
                    )}
                  </div>
                </div>
              </motion.div>
            </div>

            {user.isPremium && (
              <div className="p-4 bg-gradient-to-r from-green-500/10 to-emerald-500/10 border border-green-500/30 rounded-lg mt-4">
                <div className="flex items-center gap-3">
                  <Cpu className="w-8 h-8 text-green-400 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-sm font-bold text-green-400 tracking-[1px]">DETECCIÓN AUTOMÁTICA DISPONIBLE</p>
                    <p className="text-xs text-gray-400 mt-1 font-normal">Los usuarios Premium pueden detectar automáticamente el hardware del sistema</p>
                  </div>
                </div>
              </div>
            )}
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}
