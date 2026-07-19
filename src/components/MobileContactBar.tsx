/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React from 'react';
import { Phone, MessageSquare } from 'lucide-react';
import { BUSINESS_INFO } from '../data';

export default function MobileContactBar() {
  return (
    <div className="md:hidden fixed bottom-0 left-0 right-0 bg-neutral-900/95 backdrop-blur-md border-t border-neutral-800 py-3 px-4 flex items-center justify-between gap-3 z-40 shadow-xl pb-[calc(12px+env(safe-area-inset-bottom,0px))]">
      {/* Phone Call Trigger */}
      <a
        href={`tel:${BUSINESS_INFO.phone.replace(/\s+/g, '')}`}
        className="flex-1 bg-neutral-800 hover:bg-neutral-700 border border-neutral-700 text-white font-bold py-3 px-2 rounded-xl text-xs flex items-center justify-center transition-all cursor-pointer"
        id="mobile-bar-phone"
      >
        <Phone className="w-4 h-4 mr-1.5 text-blue-500" />
        Call 07940 598 976
      </a>

      {/* WhatsApp Quote Trigger */}
      <a
        href={BUSINESS_INFO.whatsappLink}
        target="_blank"
        rel="noreferrer"
        className="flex-1 bg-emerald-600 hover:bg-emerald-500 text-white font-bold py-3 px-2 rounded-xl text-xs flex items-center justify-center transition-all cursor-pointer shadow-lg shadow-emerald-950/20"
        id="mobile-bar-whatsapp"
      >
        <MessageSquare className="w-4 h-4 mr-1.5" />
        WhatsApp Quote
      </a>
    </div>
  );
}
