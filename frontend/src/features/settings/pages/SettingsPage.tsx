import { useEffect, useState } from "react";
import { AlertTriangle, Info, Settings as SettingsIcon, ShieldAlert } from "lucide-react";
import { PageTransition } from "@/components/PageTransition";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { useAuth } from "@/context/AuthContext";
import { getApiErrorMessage } from "@/lib/apiError";
import { timeAgo } from "@/lib/format";
import { useSettings, useUpdateSettings, type UpdateSettingsInput } from "../api/queries";

const EMPTY_FORM: UpdateSettingsInput = {
  siteName: "",
  supportEmail: "",
  allowNewRegistrations: true,
  maintenanceModeEnabled: false,
  maintenanceMessage: null,
  defaultCurrency: "usd",
  maxUploadSizeMb: 200,
};

/**
 * Platform-wide settings. Every value here is read live from the database by whatever it
 * controls — registration, maintenance mode, checkout currency, the upload ceiling — so saving a
 * change here has an immediate, real effect elsewhere in the product, not just on this screen.
 *
 * Viewable by any administrator; editable only by a super administrator, enforced by the API
 * regardless of what this page shows, since these switches reach every account on the platform.
 */
export function SettingsPage() {
  const { user } = useAuth();
  const canEdit = user?.roles.includes("SuperAdministrator") ?? false;

  const { data: settings, isLoading, isError, error } = useSettings();
  const update = useUpdateSettings();

  const [form, setForm] = useState<UpdateSettingsInput>(EMPTY_FORM);

  useEffect(() => {
    if (settings) {
      setForm({
        siteName: settings.siteName,
        supportEmail: settings.supportEmail,
        allowNewRegistrations: settings.allowNewRegistrations,
        maintenanceModeEnabled: settings.maintenanceModeEnabled,
        maintenanceMessage: settings.maintenanceMessage,
        defaultCurrency: settings.defaultCurrency,
        maxUploadSizeMb: settings.maxUploadSizeMb,
      });
    }
  }, [settings]);

  const set = <K extends keyof UpdateSettingsInput>(key: K, value: UpdateSettingsInput[K]) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  const submit = () => update.mutate(form);

  const currencyValid = /^[A-Za-z]{3}$/.test(form.defaultCurrency);
  const uploadSizeValid = form.maxUploadSizeMb >= 1 && form.maxUploadSizeMb <= 500;
  const canSubmit =
    canEdit && form.siteName.trim().length > 0 && form.supportEmail.trim().length > 0
    && currencyValid && uploadSizeValid;

  return (
    <PageTransition>
      <div className="mx-auto max-w-2xl space-y-6">
        <header>
          <h1 className="flex items-center gap-2 text-2xl font-semibold tracking-tight">
            <SettingsIcon className="h-6 w-6 text-primary" aria-hidden />
            Settings
          </h1>
          <p className="mt-1 text-muted-foreground">
            Platform-wide configuration. Changes take effect immediately.
          </p>
        </header>

        {isError && (
          <Alert variant="error">{getApiErrorMessage(error, "We could not load settings.")}</Alert>
        )}
        {update.isError && (
          <Alert variant="error">{getApiErrorMessage(update.error, "We could not save settings.")}</Alert>
        )}
        {update.isSuccess && (
          <Alert variant="success">Settings saved. The change is already in effect.</Alert>
        )}

        {!canEdit && !isLoading && (
          <p className="flex items-center gap-1.5 rounded-lg border border-border bg-muted/40 px-3 py-2 text-xs text-muted-foreground">
            <ShieldAlert className="h-3.5 w-3.5 shrink-0" aria-hidden />
            You can view these, but only a super administrator can change them.
          </p>
        )}

        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton key={index} className="h-28 rounded-[18px]" />
            ))}
          </div>
        ) : settings ? (
          <>
            <section className="space-y-4 rounded-[18px] border border-border bg-card p-5 shadow-soft">
              <h2 className="font-semibold">General</h2>
              <div className="space-y-1.5">
                <Label htmlFor="site-name">Site name</Label>
                <Input
                  id="site-name"
                  value={form.siteName}
                  onChange={(e) => set("siteName", e.target.value)}
                  disabled={!canEdit}
                  maxLength={100}
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="support-email">Support email</Label>
                <Input
                  id="support-email"
                  type="email"
                  value={form.supportEmail}
                  onChange={(e) => set("supportEmail", e.target.value)}
                  disabled={!canEdit}
                  maxLength={320}
                />
              </div>
            </section>

            <section className="rounded-[18px] border border-border bg-card p-5 shadow-soft">
              <div className="flex items-center justify-between gap-3">
                <div>
                  <h2 className="font-semibold">Registration</h2>
                  <p className="mt-0.5 text-sm text-muted-foreground">
                    Whether the public sign-up form accepts new accounts.
                  </p>
                </div>
                <Switch
                  checked={form.allowNewRegistrations}
                  onCheckedChange={(v) => set("allowNewRegistrations", v)}
                  disabled={!canEdit}
                  aria-label="Allow new registrations"
                />
              </div>
            </section>

            <section
              className={`space-y-4 rounded-[18px] border p-5 shadow-soft ${
                form.maintenanceModeEnabled
                  ? "border-warning/40 bg-warning/5"
                  : "border-border bg-card"
              }`}
            >
              <div className="flex items-center justify-between gap-3">
                <div>
                  <h2 className="font-semibold">Maintenance mode</h2>
                  <p className="mt-0.5 text-sm text-muted-foreground">
                    Blocks everyone except administrators from using the platform.
                  </p>
                </div>
                <Switch
                  checked={form.maintenanceModeEnabled}
                  onCheckedChange={(v) => set("maintenanceModeEnabled", v)}
                  disabled={!canEdit}
                  aria-label="Enable maintenance mode"
                />
              </div>

              {form.maintenanceModeEnabled && (
                <div className="flex items-start gap-2 rounded-lg bg-warning/10 px-3 py-2 text-xs text-warning">
                  <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden />
                  Every learner and lecturer will be shown the message below instead of the
                  platform, starting the moment you save.
                </div>
              )}

              <div className="space-y-1.5">
                <Label htmlFor="maintenance-message">Message shown while blocked</Label>
                <Textarea
                  id="maintenance-message"
                  value={form.maintenanceMessage ?? ""}
                  onChange={(e) => set("maintenanceMessage", e.target.value || null)}
                  disabled={!canEdit}
                  rows={2}
                  maxLength={500}
                  placeholder="The platform is undergoing maintenance. Please check back shortly."
                />
              </div>
            </section>

            <section className="space-y-4 rounded-[18px] border border-border bg-card p-5 shadow-soft">
              <h2 className="font-semibold">Limits</h2>

              <div className="space-y-1.5">
                <Label htmlFor="currency">Checkout currency</Label>
                <Input
                  id="currency"
                  value={form.defaultCurrency}
                  onChange={(e) => set("defaultCurrency", e.target.value.toLowerCase())}
                  disabled={!canEdit}
                  maxLength={3}
                  aria-invalid={!currencyValid}
                  className="max-w-[120px] uppercase"
                />
                <p className="text-xs text-muted-foreground">
                  Three letter ISO code (usd, eur, gbp…). New checkouts charge in this currency;
                  past transactions keep whatever they were actually charged in.
                </p>
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="upload-size">Maximum upload size (MB)</Label>
                <Input
                  id="upload-size"
                  type="number"
                  min={1}
                  max={500}
                  value={form.maxUploadSizeMb}
                  onChange={(e) => set("maxUploadSizeMb", Number(e.target.value))}
                  disabled={!canEdit}
                  aria-invalid={!uploadSizeValid}
                  className="max-w-[120px]"
                />
                <p className="text-xs text-muted-foreground">
                  Applies to files posted to the wall. Between 1 and 500 MB.
                </p>
              </div>
            </section>

            {settings.updatedAtUtc && (
              <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
                <Info className="h-3.5 w-3.5 shrink-0" aria-hidden />
                Last changed {timeAgo(settings.updatedAtUtc)}.
              </p>
            )}

            {canEdit && (
              <div className="flex justify-end">
                <Button onClick={submit} isLoading={update.isPending} disabled={!canSubmit}>
                  Save settings
                </Button>
              </div>
            )}
          </>
        ) : null}
      </div>
    </PageTransition>
  );
}
