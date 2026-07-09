import {
  Award,
  BarChart3,
  BookOpen,
  Boxes,
  CalendarCheck,
  ClipboardList,
  CreditCard,
  FileText,
  GraduationCap,
  LayoutDashboard,
  LifeBuoy,
  ScrollText,
  Settings,
  Shield,
  Sparkles,
  Users,
  Wallet,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  label: string;
  icon: LucideIcon;
  href: string;
  badge?: number;
}

export interface NavGroup {
  heading: string;
  items: NavItem[];
}

/** Grouped admin navigation. Hrefs are placeholders for future slices. */
export const navGroups: NavGroup[] = [
  {
    heading: "Overview",
    items: [{ label: "Dashboard", icon: LayoutDashboard, href: "/admin" }],
  },
  {
    heading: "People",
    items: [
      { label: "Users", icon: Users, href: "/admin/users" },
      { label: "Students", icon: GraduationCap, href: "/admin/students" },
      { label: "Lecturers", icon: Users, href: "/admin/lecturers" },
      { label: "Departments", icon: Boxes, href: "/admin/departments" },
    ],
  },
  {
    heading: "Academics",
    items: [
      { label: "Courses", icon: BookOpen, href: "/admin/courses" },
      { label: "Assessments", icon: ClipboardList, href: "/admin/assessments" },
      { label: "Assignments", icon: FileText, href: "/admin/assignments" },
      { label: "Attendance", icon: CalendarCheck, href: "/admin/attendance" },
      { label: "Certificates", icon: Award, href: "/admin/certificates" },
      { label: "Content", icon: BookOpen, href: "/admin/content" },
    ],
  },
  {
    heading: "Operations",
    items: [
      { label: "Finance", icon: Wallet, href: "/admin/finance" },
      { label: "Reports", icon: FileText, href: "/admin/reports" },
      { label: "Analytics", icon: BarChart3, href: "/admin/analytics" },
      { label: "AI Insights", icon: Sparkles, href: "/admin/insights" },
    ],
  },
  {
    heading: "System",
    items: [
      { label: "Audit Logs", icon: ScrollText, href: "/admin/audit" },
      { label: "Security", icon: Shield, href: "/admin/security" },
      { label: "Billing", icon: CreditCard, href: "/admin/billing" },
      { label: "Settings", icon: Settings, href: "/admin/settings" },
      { label: "Support", icon: LifeBuoy, href: "/admin/support" },
    ],
  },
];
