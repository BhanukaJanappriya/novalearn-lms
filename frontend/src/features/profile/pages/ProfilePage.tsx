import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ArrowLeft, Check, ImageOff, UserRound } from "lucide-react";
import { Alert } from "@/components/ui/alert";
import { Avatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { useAuth } from "@/context/AuthContext";
import { LearnerHeader } from "@/layouts/LearnerHeader";
import { getApiErrorMessage } from "@/lib/apiError";
import { avatarColor } from "@/features/users/lib/userVisuals";
import { useMyProfile, useSetMyAvatar } from "../api/queries";
import { AVATAR_STYLES, isUsablePictureUrl, presetsFor, type AvatarStyle } from "../lib/avatars";

/**
 * The signed-in person's own profile.
 *
 * Reachable only as yourself: the endpoints behind it take no user id, so this page has no way
 * to edit anybody else even if it wanted to.
 */
export function ProfilePage() {
  const { user } = useAuth();
  const { data: profile, isLoading, isError, error } = useMyProfile();
  const setAvatar = useSetMyAvatar();

  const [style, setStyle] = useState<AvatarStyle>("notionists");
  const [customUrl, setCustomUrl] = useState("");
  const [chosen, setChosen] = useState<string | null>(null);

  useEffect(() => {
    if (profile) {
      setChosen(profile.avatarUrl);
      setCustomUrl(profile.avatarUrl ?? "");
    }
  }, [profile]);

  // Seeded from the account id so the options offered are stable and personal.
  const presets = presetsFor(style, profile?.id ?? user?.id ?? "novalearn");

  const customIsValid = isUsablePictureUrl(customUrl);
  const isDirty = chosen !== (profile?.avatarUrl ?? null);

  return (
    <div className="min-h-screen">
      <LearnerHeader />

      <main className="mx-auto max-w-3xl px-6 py-10">
        <Link
          to="/dashboard"
          className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden />
          Back to dashboard
        </Link>

        <h1 className="mt-2 flex items-center gap-2 text-2xl font-semibold tracking-tight">
          <UserRound className="h-6 w-6 text-primary" aria-hidden />
          Your profile
        </h1>
        <p className="mt-1 text-muted-foreground">
          Only you can change your picture. Nobody else, including administrators, can set it for
          you.
        </p>

        {isError && (
          <Alert variant="error" className="mt-6">
            {getApiErrorMessage(error, "We could not load your profile.")}
          </Alert>
        )}
        {setAvatar.isError && (
          <Alert variant="error" className="mt-6">
            {getApiErrorMessage(setAvatar.error)}
          </Alert>
        )}

        {isLoading && <Skeleton className="mt-6 h-48 rounded-[18px]" />}

        {profile && (
          <>
            <section className="mt-6 flex flex-wrap items-center gap-5 rounded-[18px] border border-border bg-card p-6 shadow-soft">
              <Avatar
                name={profile.fullName}
                src={chosen}
                color={avatarColor(profile.email)}
                size="xl"
              />
              <div className="min-w-0">
                <h2 className="text-xl font-semibold">{profile.fullName}</h2>
                <p className="text-sm text-muted-foreground">{profile.email}</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {profile.roles.map((role) => (
                    <Badge key={role} variant="neutral">
                      {role}
                    </Badge>
                  ))}
                </div>
              </div>
            </section>

            <section className="mt-6 rounded-[18px] border border-border bg-card p-6 shadow-soft">
              <h2 className="font-semibold">Choose a picture</h2>

              <div className="mt-3 flex flex-wrap gap-1.5">
                {AVATAR_STYLES.map((option) => (
                  <button
                    key={option.id}
                    type="button"
                    onClick={() => setStyle(option.id)}
                    aria-pressed={style === option.id}
                    className={`rounded-lg px-3 py-1.5 text-xs font-medium transition-colors ${
                      style === option.id
                        ? "bg-primary/10 text-primary"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    }`}
                  >
                    {option.label}
                  </button>
                ))}
              </div>

              <ul className="mt-4 grid grid-cols-4 gap-3 sm:grid-cols-8">
                {presets.map((url) => {
                  const selected = chosen === url;

                  return (
                    <li key={url}>
                      <button
                        type="button"
                        onClick={() => setChosen(url)}
                        aria-label="Use this picture"
                        aria-pressed={selected}
                        className={`relative w-full rounded-full ring-offset-2 ring-offset-card transition-transform hover:scale-105 ${
                          selected ? "ring-2 ring-primary" : ""
                        }`}
                      >
                        <Avatar name={profile.fullName} src={url} size="lg" className="w-full" />
                        {selected && (
                          <span className="absolute -right-1 -top-1 rounded-full bg-primary p-0.5 text-primary-foreground">
                            <Check className="h-3 w-3" aria-hidden />
                          </span>
                        )}
                      </button>
                    </li>
                  );
                })}
              </ul>

              <div className="mt-6 space-y-1.5">
                <Label htmlFor="avatar-url">Or paste a picture address</Label>
                <div className="flex flex-wrap gap-2">
                  <Input
                    id="avatar-url"
                    value={customUrl}
                    onChange={(e) => setCustomUrl(e.target.value)}
                    placeholder="https://example.com/me.jpg"
                    maxLength={2048}
                    aria-invalid={!customIsValid}
                    className="min-w-[240px] flex-1"
                  />
                  <Button
                    variant="outline"
                    onClick={() => setChosen(customUrl.trim() || null)}
                    disabled={!customIsValid || customUrl.trim().length === 0}
                  >
                    Preview
                  </Button>
                </div>
                {!customIsValid && (
                  <p className="text-xs text-destructive">
                    Enter a full http or https web address.
                  </p>
                )}
                <p className="text-xs text-muted-foreground">
                  Uploading a file from your computer is not available yet, so a picture has to be
                  hosted somewhere already.
                </p>
              </div>

              <div className="mt-6 flex flex-wrap items-center justify-between gap-2 border-t border-border pt-4">
                <Button
                  variant="ghost"
                  onClick={() => setAvatar.mutate(null)}
                  disabled={setAvatar.isPending || profile.avatarUrl === null}
                >
                  <ImageOff className="h-4 w-4" />
                  Remove picture
                </Button>
                <Button
                  onClick={() => setAvatar.mutate(chosen)}
                  isLoading={setAvatar.isPending}
                  disabled={!isDirty}
                >
                  Save picture
                </Button>
              </div>

              {setAvatar.isSuccess && !isDirty && (
                <p className="mt-3 text-xs text-success">Your picture has been updated.</p>
              )}
            </section>
          </>
        )}
      </main>
    </div>
  );
}
