/** One customer quote shown in the sign-in showcase. */
export interface Testimonial {
  quote: string;
  name: string;
  /** Job title, e.g. "Head of Digital Learning". */
  role: string;
  institution: string;
  /** 1..5, may be fractional. */
  rating: number;
  /** Optional avatar tint (any CSS colour). Falls back to brand purple. */
  accent?: string;
}
