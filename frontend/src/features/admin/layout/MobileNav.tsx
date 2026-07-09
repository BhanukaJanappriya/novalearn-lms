import { NavLink } from "react-router-dom";
import { cn } from "@/lib/utils";
import { navGroups } from "./nav";

/** Flat scrollable nav used inside the mobile drawer. */
export function MobileNav({ onNavigate }: { onNavigate: () => void }) {
  return (
    <nav className="h-[calc(100vh-4rem)] overflow-y-auto px-3 py-4">
      {navGroups.map((group) => (
        <div key={group.heading} className="mb-4">
          <p className="mb-1 px-2 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
            {group.heading}
          </p>
          <ul className="space-y-0.5">
            {group.items.map((item) => {
              const Icon = item.icon;
              return (
                <li key={item.href}>
                  <NavLink
                    to={item.href}
                    end={item.href === "/admin"}
                    onClick={onNavigate}
                    className={({ isActive }) =>
                      cn(
                        "flex items-center gap-3 rounded-xl px-2.5 py-2 text-sm font-medium transition-colors",
                        isActive
                          ? "bg-primary/10 text-primary"
                          : "text-muted-foreground hover:bg-muted hover:text-foreground",
                      )
                    }
                  >
                    <Icon className="h-[18px] w-[18px] shrink-0" />
                    <span className="truncate">{item.label}</span>
                  </NavLink>
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </nav>
  );
}
