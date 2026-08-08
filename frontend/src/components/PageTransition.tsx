import { motion } from "framer-motion";
import { fadeUp } from "@/lib/motion";

/**
 * Wraps a routed page so it fades and rises in.
 *
 * Deliberately enter-only. An exit animation would need AnimatePresence to hold the outgoing
 * page while the next one mounts, and every page here is a lazy chunk, so the wait produces a
 * blank flash on first visit. Animating only the arrival is smooth in both cases.
 */
export function PageTransition({ children }: { children: React.ReactNode }) {
  return (
    <motion.div initial="hidden" animate="visible" variants={fadeUp}>
      {children}
    </motion.div>
  );
}
