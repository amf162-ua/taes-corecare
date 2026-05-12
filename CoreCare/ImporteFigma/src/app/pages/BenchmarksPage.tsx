import { useState, useEffect } from "react";
import { useOutletContext, useSearchParams } from "react-router";
import { CategoryBenchmarks } from "../components/CategoryBenchmarks";
import { AppContext } from "./Layout";

export function BenchmarksPage() {
  const { onRunBenchmark, isPremium } = useOutletContext<AppContext>();
  const [searchParams] = useSearchParams();
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  useEffect(() => {
    const category = searchParams.get("category");
    if (category) {
      setSelectedCategory(category);
    }
  }, [searchParams]);

  return (
    <div className="min-h-screen">
      <CategoryBenchmarks
        onRunBenchmark={onRunBenchmark}
        isPremium={isPremium}
        initialExpandedCategory={selectedCategory}
      />
    </div>
  );
}