import { LegalDocument } from "./LegalDocument";

/** Privacy Policy. Shares its version string with the Terms of Service. */
export function PrivacyPage() {
  return (
    <LegalDocument title="Privacy Policy" version="2026-09-07">
      <p>
        This policy explains what personal data the NovaLearn learning platform collects, why, and
        what choices you have. Your institution is the controller of most of this data; NovaLearn
        processes it on their instructions to run the Service.
      </p>

      <section className="space-y-2">
        <h2>What we collect</h2>
        <ul>
          <li>
            <strong>Account details</strong> you provide at sign-up: name, email address, and the
            fact and time that you accepted these terms.
          </li>
          <li>
            <strong>Learning activity</strong>: enrolments, submissions, quiz attempts, grades and
            progress.
          </li>
          <li>
            <strong>Technical data</strong>: sign-in events, session and device information, and
            logs needed to keep the Service secure.
          </li>
        </ul>
      </section>

      <section className="space-y-2">
        <h2>How we use it</h2>
        <ul>
          <li>To provide the Service: run courses, record assessment, and show progress.</li>
          <li>To secure accounts: detect suspicious sign-ins, apply lockouts, and keep audit logs.</li>
          <li>To support you when you contact us, and to meet legal obligations.</li>
        </ul>
        <p>We do not sell personal data or use it for advertising.</p>
      </section>

      <section className="space-y-2">
        <h2>Sharing</h2>
        <p>
          Data is visible to staff at your institution in line with their role (for example, a
          lecturer sees their own courses' submissions). We use a small number of processors, such
          as hosting and email providers, under contracts that limit them to running the Service.
        </p>
      </section>

      <section className="space-y-2">
        <h2>Retention</h2>
        <p>
          Learning records are kept for as long as your institution requires them. Security logs are
          kept for a limited period and then removed. When your account is closed, personal data is
          deleted or anonymised unless it must be kept for a legal reason.
        </p>
      </section>

      <section className="space-y-2">
        <h2>Your rights</h2>
        <p>
          Depending on where you live, you may have the right to access, correct, export or delete
          your personal data, and to object to certain processing. Contact your institution's
          administrator to exercise these rights; they can escalate to NovaLearn where needed.
        </p>
      </section>

      <section className="space-y-2">
        <h2>Contact</h2>
        <p>
          For privacy questions, contact your institution's administrator or the support address in
          the platform footer.
        </p>
      </section>
    </LegalDocument>
  );
}
