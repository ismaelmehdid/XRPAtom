"use client"

import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/auth-context"
import { ArrowRight, LogIn } from "lucide-react"
import Link from "next/link"

export default function AboutPage() {
  const { isLoggedIn, login, isTSO, userData } = useAuth()

  return (
    <div className="container mx-auto px-4 py-12">
      <div className="max-w-3xl mx-auto">
        <h1 className="text-3xl font-bold mb-6">About XRPAtom</h1>

        <div className="prose dark:prose-invert max-w-none">
          <p className="text-lg mb-4">
            XRPAtom is a cutting-edge platform designed to provide seamless XRP transaction management, analytics, and
            security for both individual users and enterprises.
          </p>

          <h2 className="text-2xl font-semibold mt-8 mb-4">Our Mission</h2>
          <p>
            Our mission is to simplify the XRP ecosystem by providing powerful tools that make transactions faster, more
            secure, and more accessible to everyone.
          </p>

          <h2 className="text-2xl font-semibold mt-8 mb-4">Key Features</h2>
          <ul className="list-disc pl-6 space-y-2">
            <li>Fast and secure XRP transactions</li>
            <li>Advanced analytics and reporting</li>
            <li>Enterprise-grade security</li>
            <li>Intuitive user interface</li>
            <li>Comprehensive API for developers</li>
            <li>24/7 customer support</li>
          </ul>

          <h2 className="text-2xl font-semibold mt-8 mb-4">Technology</h2>
          <p>
            XRPAtom is built using cutting-edge technologies including Next.js for the frontend and C# for the backend,
            ensuring optimal performance, security, and scalability.
          </p>
        </div>

        <div className="mt-8">
        {isLoggedIn ? (
                <Button asChild>
                  <Link href="/dashboard">
                    Go to Dashboard <ArrowRight className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              ) : (
                <Button asChild>
                  <Link href="/login">
                    Get Started <LogIn className="ml-2 h-4 w-4" />
                  </Link>
                </Button>
              )}
        </div>
      </div>
    </div>
  )
}

