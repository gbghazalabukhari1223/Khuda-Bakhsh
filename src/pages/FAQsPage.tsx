/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { PageId } from '../types';
import { GENERAL_FAQS, BUSINESS_INFO } from '../data';
import { ChevronDown, MessageSquare, Phone, HelpCircle } from 'lucide-react';

interface FAQsPageProps {
  onNavigate: (pageId: PageId) => void;
}

export default function FAQsPage({ onNavigate }: FAQsPageProps) {
  const [openIdx, setOpenIdx] = useState<number | null>(0);

  return (
    <div className="w-full bg-neutral-950 text-white selection:bg-blue-600 selection:text-white swr-site">
      
      {/* Page Header */}
      <section className="bg-neutral-900 border-b border-neutral-850 py-16 text-center">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <span className="text-xs font-bold uppercase tracking-widest text-blue-500 block mb-3">Help Desk Support</span>
          <h1 className="text-3xl sm:text-5xl font-extrabold text-white uppercase tracking-tight">
            Frequently Asked Questions
          </h1>
          <p className="mt-3 text-neutral-400 text-sm sm:text-base max-w-xl mx-auto">
            Get clear, honest information regarding bookings, bulky collections, access logistics, and material licensing.
          </p>
        </div>
      </section>

      {/* Accordions */}
      <section className="py-16 max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="space-y-4" id="faqs-page-accordions">
          {GENERAL_FAQS.map((faq, idx) => {
            const isOpen = openIdx === idx;
            return (
              <div 
                key={faq.id}
                className="bg-neutral-900 border border-neutral-800 rounded-xl overflow-hidden transition-all duration-300 hover:border-neutral-700"
              >
                <button
                  onClick={() => setOpenIdx(isOpen ? null : idx)}
                  className="w-full flex justify-between items-center text-left p-5 text-sm sm:text-base font-extrabold text-white hover:text-blue-400 transition-colors focus:outline-none"
                  aria-expanded={isOpen}
                  id={`faqs-page-btn-${faq.id}`}
                >
                  <span className="flex items-start">
                    <HelpCircle className="w-4 h-4 mr-3 text-blue-500 shrink-0 mt-0.5" />
                    <span>{faq.question}</span>
                  </span>
                  <ChevronDown className={`w-4 h-4 text-neutral-500 shrink-0 ml-4 transition-transform duration-250 ${isOpen ? 'rotate-185 text-blue-500' : 'rotate-0'}`} />
                </button>
                
                {isOpen && (
                  <div className="px-5 pb-5 pt-1 border-t border-neutral-900 text-xs sm:text-sm text-neutral-400 leading-relaxed animate-in fade-in duration-200 pl-12">
                    {faq.answer}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </section>

      {/* Still got questions panel */}
      <section className="bg-neutral-900 py-16 text-center border-t border-neutral-850">
        <div className="max-w-xl mx-auto px-4">
          <h2 className="text-xl sm:text-2xl font-extrabold text-white uppercase">Still have questions?</h2>
          <p className="mt-2 text-xs sm:text-sm text-neutral-400">
            Our polite, hands-on office team is ready to assist. Drop us a call or text us photographs on WhatsApp for a quick response.
          </p>
          <div className="mt-6 flex flex-col sm:flex-row gap-4 justify-center">
            <a 
              href={BUSINESS_INFO.whatsappLink}
              target="_blank"
              rel="noreferrer"
              className="bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-3.5 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer shadow-lg shadow-emerald-950/10"
            >
              <MessageSquare className="w-4 h-4 mr-2" />
              WhatsApp Help
            </a>
            <a 
              href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
              className="bg-neutral-800 hover:bg-neutral-700 text-white border border-neutral-700 py-3.5 px-6 rounded-lg text-xs tracking-wider uppercase inline-flex items-center justify-center cursor-pointer"
            >
              <Phone className="w-4 h-4 mr-2 text-blue-400" />
              Call 07940 598 976
            </a>
          </div>
        </div>
      </section>

    </div>
  );
}
