import { Hero } from "./components/Hero";
import { Features } from "./components/Features";
import { PremiumFeatures } from "./components/PremiumFeatures";
import { AdminSection } from "./components/AdminSection";
import { Testimonials } from "./components/Testimonials";
import { CTA } from "./components/CTA";
import { Footer } from "./components/Footer";

export default function App() {
  return (
    <div className="min-h-screen bg-[#0f1419] dark">
      <Hero />
      <Features />
      <PremiumFeatures />
      <AdminSection />
      <Testimonials />
      <CTA />
      <Footer />
    </div>
  );
}