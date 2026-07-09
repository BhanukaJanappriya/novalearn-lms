import { motion } from "framer-motion";

const COLORS = ["#8B5CF6", "#A78BFA", "#2a78d6", "#1baf7a", "#eda100", "#e34948"];

/** Lightweight one-shot confetti burst (no external dependency). */
export function Confetti({ count = 28 }: { count?: number }) {
  return (
    <div className="pointer-events-none absolute inset-0 z-20 flex items-center justify-center overflow-hidden">
      {Array.from({ length: count }).map((_, i) => {
        const angle = (Math.PI * 2 * i) / count + Math.random();
        const distance = 90 + Math.random() * 130;
        return (
          <motion.span
            key={i}
            initial={{ opacity: 1, x: 0, y: 0, scale: 1 }}
            animate={{
              opacity: 0,
              x: Math.cos(angle) * distance,
              y: Math.sin(angle) * distance + 60,
              scale: 0.4,
              rotate: Math.random() * 360,
            }}
            transition={{ duration: 0.9 + Math.random() * 0.4, ease: "easeOut" }}
            className="absolute h-2 w-2 rounded-[2px]"
            style={{ background: COLORS[i % COLORS.length] }}
          />
        );
      })}
    </div>
  );
}
