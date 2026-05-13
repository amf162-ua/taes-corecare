import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "./ui/dialog";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";

interface LoginModalProps {
  open: boolean;
  onClose: () => void;
  onLogin: (name: string) => void;
}

export function LoginModal({ open, onClose, onLogin }: LoginModalProps) {
  const [loginEmail, setLoginEmail] = useState("");
  const [loginPassword, setLoginPassword] = useState("");
  const [registerName, setRegisterName] = useState("");
  const [registerEmail, setRegisterEmail] = useState("");
  const [registerPassword, setRegisterPassword] = useState("");

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    const name = loginEmail.split("@")[0];
    onLogin(name);
    onClose();
  };

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();
    onLogin(registerName);
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[450px] bg-[#1c2030] border-2 border-cyan-500/30 text-white">
        <DialogHeader>
          <DialogTitle className="text-2xl text-center bg-gradient-to-r from-cyan-400 to-blue-400 bg-clip-text text-transparent tracking-[2px]">
            CORE CARE
          </DialogTitle>
          <DialogDescription className="text-center text-gray-400 font-normal">
            Inicia sesión o crea una cuenta para acceder a todos los benchmarks
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="login" className="mt-4">
          <TabsList className="grid w-full grid-cols-2 bg-[#0a0e1a] border border-cyan-500/20">
            <TabsTrigger 
              value="login"
              className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-cyan-500/20 data-[state=active]:to-blue-500/20 data-[state=active]:text-cyan-400 data-[state=active]:border data-[state=active]:border-cyan-500/50 text-xs tracking-[1px]"
            >
              INICIAR SESIÓN
            </TabsTrigger>
            <TabsTrigger 
              value="register"
              className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-cyan-500/20 data-[state=active]:to-blue-500/20 data-[state=active]:text-cyan-400 data-[state=active]:border data-[state=active]:border-cyan-500/50 text-xs tracking-[1px]"
            >
              REGISTRARSE
            </TabsTrigger>
          </TabsList>

          <TabsContent value="login">
            <form onSubmit={handleLogin} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="login-email" className="text-cyan-400 font-normal">
                  Correo electrónico
                </Label>
                <Input
                  id="login-email"
                  type="email"
                  placeholder="tu@email.com"
                  value={loginEmail}
                  onChange={(e) => setLoginEmail(e.target.value)}
                  required
                  className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-500 focus:border-cyan-400"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="login-password" className="text-cyan-400 font-normal">
                  Contraseña
                </Label>
                <Input
                  id="login-password"
                  type="password"
                  placeholder="••••••••"
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  required
                  className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-500 focus:border-cyan-400"
                />
              </div>
              <Button 
                type="submit" 
                className="w-full bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-500 hover:to-blue-500 text-white border border-cyan-500/50 shadow-lg shadow-cyan-500/20 tracking-[1px]"
              >
                INICIAR SESIÓN
              </Button>
            </form>
          </TabsContent>

          <TabsContent value="register">
            <form onSubmit={handleRegister} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="register-name" className="text-cyan-400 font-normal">
                  Nombre completo
                </Label>
                <Input
                  id="register-name"
                  type="text"
                  placeholder="Tu nombre"
                  value={registerName}
                  onChange={(e) => setRegisterName(e.target.value)}
                  required
                  className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-500 focus:border-cyan-400"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="register-email" className="text-cyan-400 font-normal">
                  Correo electrónico
                </Label>
                <Input
                  id="register-email"
                  type="email"
                  placeholder="tu@email.com"
                  value={registerEmail}
                  onChange={(e) => setRegisterEmail(e.target.value)}
                  required
                  className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-500 focus:border-cyan-400"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="register-password" className="text-cyan-400 font-normal">
                  Contraseña
                </Label>
                <Input
                  id="register-password"
                  type="password"
                  placeholder="••••••••"
                  value={registerPassword}
                  onChange={(e) => setRegisterPassword(e.target.value)}
                  required
                  className="bg-[#0a0e1a] border-cyan-500/30 text-white placeholder:text-gray-500 focus:border-cyan-400"
                />
              </div>
              <Button 
                type="submit" 
                className="w-full bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-500 hover:to-blue-500 text-white border border-cyan-500/50 shadow-lg shadow-cyan-500/20 tracking-[1px]"
              >
                CREAR CUENTA
              </Button>
            </form>
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}