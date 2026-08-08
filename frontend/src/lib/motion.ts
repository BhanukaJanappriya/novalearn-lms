import type { Transition, Variants } from "framer-motion";

/**
 * Shared motion vocabulary.
 *
 * Everything animated in the app should come from here, so timings stay consistent and there is
 * one place to slow the whole product down or speed it up. Nothing in this file checks
 * `prefers-reduced-motion`: that is handled once, globally, by the MotionConfig in App.
 */

/** The house curve. Quick to start, gentle to settle. */
export const easeOut: Transition = { duration: 0.35, ease: [0.22, 1, 0.36, 1] };

/** For anything the user is dragging into place or pressing. */
export const spring: Transition = { type: "spring", stiffness: 320, damping: 26 };

/** Content arriving: a short rise with the fade, never a slide from off screen. */
export const fadeUp: Variants = {
  hidden: { opacity: 0, y: 12 },
  visible: { opacity: 1, y: 0, transition: easeOut },
  exit: { opacity: 0, y: -8, transition: { duration: 0.2 } },
};

/** A plain crossfade, for swapping a skeleton for real content. */
export const fade: Variants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: easeOut },
  exit: { opacity: 0, transition: { duration: 0.2 } },
};

/**
 * Parent for a list or grid. Children arrive one after another rather than all at once, which
 * reads as the page assembling itself instead of blinking into place.
 */
export const staggerContainer: Variants = {
  hidden: {},
  visible: {
    transition: {
      staggerChildren: 0.05,
      // Lets the container settle before its children start moving.
      delayChildren: 0.04,
    },
  },
};

/** Child of {@link staggerContainer}. */
export const staggerItem: Variants = {
  hidden: { opacity: 0, y: 14 },
  visible: { opacity: 1, y: 0, transition: easeOut },
};

/**
 * Cards that lift under the cursor. Kept small: a big lift on a grid of twelve cards turns a
 * pointer sweep into a wave.
 */
export const hoverLift = {
  whileHover: { y: -4 },
  whileTap: { scale: 0.99 },
  transition: spring,
} as const;
