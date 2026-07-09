import {
  Award,
  BookOpen,
  ClipboardCheck,
  DollarSign,
  LifeBuoy,
  Megaphone,
  MessagesSquare,
  UserPlus,
  type LucideIcon,
} from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { timeAgo } from "@/lib/format";
import type { ActivityCategory, ActivityItem, ActivityStatus } from "../api/types";

const categoryIcon: Record<ActivityCategory, LucideIcon> = {
  enrollment: UserPlus,
  course: BookOpen,
  assignment: ClipboardCheck,
  certificate: Award,
  payment: DollarSign,
  support: LifeBuoy,
  announcement: Megaphone,
  forum: MessagesSquare,
};

const statusVariant: Record<ActivityStatus, "success" | "warning" | "neutral" | "default"> = {
  success: "success",
  warning: "warning",
  info: "default",
  pending: "neutral",
};

export function ActivityFeed({ items }: { items: ActivityItem[] }) {
  return (
    <ol className="relative space-y-1">
      {items.map((item, i) => {
        const Icon = categoryIcon[item.category];
        const isLast = i === items.length - 1;
        return (
          <li key={item.id} className="relative flex gap-3 pb-4">
            {!isLast && <span className="absolute left-[19px] top-10 h-full w-px bg-border" aria-hidden />}
            <div className="relative shrink-0">
              <Avatar name={item.actorName} color={item.actorColor} size="md" />
              <span className="absolute -bottom-1 -right-1 flex h-5 w-5 items-center justify-center rounded-full bg-card ring-1 ring-border">
                <Icon className="h-3 w-3 text-muted-foreground" />
              </span>
            </div>
            <div className="min-w-0 flex-1 pt-0.5">
              <p className="text-sm leading-snug">
                <span className="font-medium">{item.actorName}</span>{" "}
                <span className="text-muted-foreground">{item.message}</span>
              </p>
              <div className="mt-1 flex items-center gap-2">
                <Badge variant={statusVariant[item.status]} className="capitalize">
                  {item.category}
                </Badge>
                <span className="text-xs text-muted-foreground">{timeAgo(item.timestamp)}</span>
              </div>
            </div>
          </li>
        );
      })}
    </ol>
  );
}
