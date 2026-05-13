import { createBrowserRouter } from "react-router";
import { Layout } from "./pages/Layout";
import { HomePage } from "./pages/HomePage";
import { BenchmarksPage } from "./pages/BenchmarksPage";
import { HistoryPage } from "./pages/HistoryPage";
import { Processes } from "./pages/Processes";
import { ReportsPage } from "./pages/ReportsPage";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: Layout,
    children: [
      { index: true, Component: HomePage },
      { path: "benchmarks", Component: BenchmarksPage },
      { path: "history", Component: HistoryPage },
      { path: "processes", Component: Processes },
      { path: "reports", Component: ReportsPage },
    ],
  },
]);