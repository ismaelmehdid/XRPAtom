export default function ContactPage() {
    return (
      <div className="max-w-2xl mx-auto px-4 py-12">
        <h1 className="text-3xl font-bold mb-6">Contact Us</h1>
        <p className="mb-4">
          Have questions, feedback, or need support? We’d love to hear from you.
        </p>
        <p className="mb-4">
          You can reach us via email at{" "}
          <a href="mailto:support@xrpatom.com" className="text-primary underline">
            support@xrpatom.com
          </a>.
        </p>
        <p>
          We aim to respond to all inquiries within 1–2 business days.
        </p>
      </div>
    )
  }