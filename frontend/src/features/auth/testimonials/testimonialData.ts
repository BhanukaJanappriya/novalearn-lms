import type { Testimonial } from "./types";

/** Sign-in showcase quotes. Kept to ~20 words so the card height stays stable across the set. */
export const testimonials: Testimonial[] = [
  {
    quote:
      "We moved four faculties onto NovaLearn in one term. Assessment turnaround went from ten days to three, and the grade emails stopped.",
    name: "Dr. Priya Menon",
    role: "Head of Digital Learning",
    institution: "Riverside Polytechnic",
    rating: 4.9,
    accent: "#7C3AED",
  },
  {
    quote:
      "The analytics show me which topics a cohort is struggling with before the exam, not after. I restructured two lectures on what the dashboards told me.",
    name: "James Whitlock",
    role: "Senior Lecturer, Computer Science",
    institution: "Halden University",
    rating: 4.8,
    accent: "#4F46E5",
  },
  {
    quote:
      "Launching a new programme used to take months of course-building. With the templates and enrolment rules, our last one went from approval to first class in a fortnight.",
    name: "Aisha Rahman",
    role: "Programme Director",
    institution: "Meridian College",
    rating: 5.0,
    accent: "#0D9488",
  },
  {
    quote:
      "Compliance training was a spreadsheet nightmare. Completion is tracked automatically now, and I can hand auditors a report in a minute instead of a week.",
    name: "Tom Vasquez",
    role: "Learning & Development Manager",
    institution: "Northgate Business School",
    rating: 4.7,
    accent: "#D97706",
  },
  {
    quote:
      "Adoption is the hard part of any platform, and this one just made sense to staff. Nine in ten of our academics were running live courses within a month.",
    name: "Prof. Eleanor Cross",
    role: "Dean of Undergraduate Studies",
    institution: "Ashcombe Institute of Technology",
    rating: 4.9,
    accent: "#6366F1",
  },
];
