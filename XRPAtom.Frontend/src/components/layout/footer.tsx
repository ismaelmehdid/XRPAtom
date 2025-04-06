import Link from "next/link"

export default function Footer() {
  return (
    <footer className="border-t bg-background">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-between gap-4 py-8 sm:flex-row sm:py-6">
          <div className="flex flex-col items-center gap-2 sm:flex-row sm:gap-4">
            <p className="text-center text-sm leading-loose text-muted-foreground sm:text-left">
              &copy; {new Date().getFullYear()} XRPAtom. All rights reserved.
            </p>
          </div>
          <div className="flex flex-wrap justify-center gap-4 sm:gap-6">
            <Link
              href="/terms"
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              Terms
            </Link>
            <Link
              href="/privacy"
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              Privacy
            </Link>
            <Link
              href="/contact"
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-foreground"
            >
              Contact
            </Link>
          </div>
        </div>
      </div>
    </footer>
  )
}
