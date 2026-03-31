import { useOutletContext, useNavigate } from "react-router";
import { HeroSection } from "../components/HeroSection";
import { AppContext } from "./Layout";

export function HomePage() {
  const { user } = useOutletContext<AppContext>();
  const navigate = useNavigate();

  const handleCategoryClick = (categoryId: string) => {
    navigate(`/benchmarks?category=${categoryId}`);
  };

  return (
    <div>
      <HeroSection onCategoryClick={handleCategoryClick} hasUser={!!user} />
    </div>
  );
}