import { LegalDocument } from "./LegalDocument";

/** Terms of Service. Version string must match the backend's TermsAgreement.CurrentVersion. */
export function TermsPage() {
  return (
    <LegalDocument title="Terms of Service" version="2026-09-07">
      <p>
        These terms govern your use of the NovaLearn learning platform ("the Service"). By creating
        an account or using the Service you agree to them. If you are using the Service on behalf of
        an institution, you confirm that you are authorised to accept these terms for it.
      </p>

      <section className="space-y-2">
        <h2>1. Your account</h2>
        <p>
          You are responsible for the activity that happens under your account and for keeping your
          password confidential. Tell your administrator or our support team promptly if you believe
          your account has been compromised. You must provide accurate registration details and keep
          them up to date.
        </p>
      </section>

      <section className="space-y-2">
        <h2>2. Acceptable use</h2>
        <p>You agree not to:</p>
        <ul>
          <li>Access, or try to access, accounts, data or systems that are not yours.</li>
          <li>Upload malware, or content that is unlawful, infringing, or harassing.</li>
          <li>Disrupt or overload the Service, or circumvent its access controls or rate limits.</li>
          <li>Share course materials outside the audience your institution has licensed them for.</li>
        </ul>
      </section>

      <section className="space-y-2">
        <h2>3. Content and intellectual property</h2>
        <p>
          Course materials, assessments and other content remain the property of whoever created or
          licensed them. You keep ownership of the work you submit, and you grant your institution
          and NovaLearn the licence needed to store it, mark it and operate the Service.
        </p>
      </section>

      <section className="space-y-2">
        <h2>4. Availability and changes</h2>
        <p>
          We work to keep the Service available but do not guarantee uninterrupted access. We may
          add, change or remove features, and we may update these terms. If we make a material
          change we will update the version date above and, where appropriate, ask you to accept the
          new terms on your next sign-in.
        </p>
      </section>

      <section className="space-y-2">
        <h2>5. Suspension and termination</h2>
        <p>
          Your institution can suspend or close your account. We may suspend access where we
          reasonably believe these terms have been broken or the Service is at risk. You can stop
          using the Service at any time and ask for your account to be closed.
        </p>
      </section>

      <section className="space-y-2">
        <h2>6. Liability</h2>
        <p>
          The Service is provided "as is". To the extent the law allows, NovaLearn is not liable for
          indirect or consequential loss, or for loss of data that you could reasonably have kept a
          copy of elsewhere.
        </p>
      </section>

      <section className="space-y-2">
        <h2>7. Contact</h2>
        <p>
          Questions about these terms can go to your institution's administrator or to the support
          address shown in the platform footer.
        </p>
      </section>
    </LegalDocument>
  );
}
