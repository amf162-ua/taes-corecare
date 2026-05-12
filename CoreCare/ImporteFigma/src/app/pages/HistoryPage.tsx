import { useOutletContext } from "react-router";
import { BenchmarkHistory } from "../components/BenchmarkHistory";
import { AppContext } from "./Layout";

export function HistoryPage() {
  const { benchmarkResults, onClearHistory, isPremium } = useOutletContext<AppContext>();

  return (
    <div className="min-h-screen py-8">
      <BenchmarkHistory 
        results={benchmarkResults}
        onClearHistory={onClearHistory}
        isPremium={isPremium}
      />
    </div>
  );
}