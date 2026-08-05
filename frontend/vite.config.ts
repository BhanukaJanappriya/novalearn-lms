import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    proxy: {
      // Proxy API calls to the backend during development.
      "/api": {
        target: "https://localhost:7001",
        changeOrigin: true,
        secure: false,
      },
      // The SignalR hub sits outside /api and needs ws: true, or the connection
      // silently falls back to long polling.
      "/hubs": {
        target: "https://localhost:7001",
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
